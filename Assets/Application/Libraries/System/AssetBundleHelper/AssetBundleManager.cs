using System ;
using System.Collections ;
using System.Collections.Generic ;

using System.Threading ;

using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(シングルトン) Version 2024/05/08 0
	/// </summary>
	public partial class AssetBundleManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AssetBundleManagerを生成する
		/// </summary>
		[MenuItem( "GameObject/Helper/AssetBundleHelper/AssetBundleManager", false, 24 )]
		public static void CreateAssetBundleManager()
		{
			var go = new GameObject( "AssetBundleManager" ) ;
		
			go.transform.SetParent( null ) ;
			go.transform.SetLocalPositionAndRotation( Vector2.zero, Quaternion.identity ) ;
			go.transform.localScale = Vector3.one ;
		
			go.AddComponent<AssetBundleManager>() ;
			Selection.activeGameObject = go ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		// シングルトンインスタンス
		private static AssetBundleManager m_Instance ;

		/// <summary>
		/// AssetBundleManagerのシングルトンインスタンスを取得する
		/// </summary>
		public  static AssetBundleManager   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		// 初期化済みかどうか
		private bool m_Initialized ;

		//-----------------------------------

		/// <summary>
		/// アセットバンドルのパス情報
		/// </summary>
		public class DownloadEntity
		{
			public string	Path ;
			public bool		Keep ;
		}

		// サブスレッドでのストレージ書き込みキャンセル用のトークンソース
		private CancellationTokenSource	m_WritingCancellationSource ;

		/// <summary>
		/// アセットバンドルの保存場所タイプ
		/// </summary>
		public enum LocationTypes
		{
			/// <summary>
			/// Storage のみ
			/// </summary>
			Storage,

			/// <summary>
			/// StreamingAssets のみ (読み込みのみ・更新不可)
			/// </summary>
			StreamingAssets,

			/// <summary>
			/// Storage および StreamingAssets (StreamingAssets より新しいものがあれば Storage に保存されている)
			/// </summary>
			StorageAndStreamingAssets,
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// AssetBundleManagerのシングルトンインスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static AssetBundleManager Create( Transform parent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindAnyObjectByType( typeof( AssetBundleManager ) ) as AssetBundleManager ;
			if( m_Instance == null )
			{
				var go = new GameObject( "AssetBundleManager" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<AssetBundleManager>() ;
			}

			//----------------------------------------------------------
			
			// 各種初期設定(０フレームでマニフェストのロード要求が走る可能性もあるのでこのタイミングで行う)
			m_Instance.Setup() ;

			return m_Instance ;
		}
		
		// 各種初期設定を行う
		private void Setup()
		{
			if( m_Initialized == true )
			{
				return ;
			}

			//----------------------------------

			if( m_ManifestInfo == null )
			{
				m_ManifestInfo = new () ;
			}
			else
			if( m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					manifestInfo.Clear() ;
				}
			}

			// リソースキャッシュを生成する
			if( m_ResourceCache == null )
			{
				m_ResourceCache			= new () ;
				m_ResourceCacheDetector = new () ;
#if UNITY_EDITOR
				m_ResourceCacheViewer = new () ;
#endif
			}
			else
			{
				m_ResourceCache.Clear() ;
				m_ResourceCacheDetector.Clear() ;
#if UNITY_EDITOR
				m_ResourceCacheViewer.Clear() ;
#endif
			}

			//----------------------------------

			// 受信バッファキャッシュを生成する
			CreateLargeReceiveBufferCache() ;
			CreateSmallReceiveBufferCache() ;

			//----------------------------------

			// ストレージへの書き込みを中断するキャンセルトークンを生成する
			m_WritingCancellationSource = new () ;

			//----------------------------------

			m_Initialized = true ;	// 初期化済み状態らする
		}

		/// <summary>
		/// AssetBundleManagerのシングルトンインスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}
	
		//-----------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			var instanceOther = GameObject.FindAnyObjectByType( typeof( AssetBundleManager ) ) as AssetBundleManager ;
			if( instanceOther != null )
			{
				if( instanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
		
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}

//			gameObject.hideFlags = HideFlags.HideInHierarchy ;

			//-----------------------------

			// 原点じゃないと気持ち悪い
			gameObject.transform.SetLocalPositionAndRotation( Vector2.zero, Quaternion.identity ) ;
			gameObject.transform.localScale	= Vector3.one ;

			//-----------------------------
		
			Setup() ;
		}

		internal IEnumerator Start()
		{
			if( m_LoadManifestOnAwake == true )
			{
				// 自動でマニフェストをロードする
				yield return LoadAllManifestsAsync() ;
			}
		}
	
		internal void Update()
		{
			// 破棄対象になっているアセットバンドルを破棄する
			AutoCleaningAssetBundleCache() ;
		}
	
		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				// ストレージへの書き込みを中断させる
				if( m_WritingCancellationSource != null )
				{
					m_WritingCancellationSource.Cancel() ;
					m_WritingCancellationSource.Dispose() ;
					m_WritingCancellationSource = null ;
				}

				// 終了時に呼び出して欲しいコールバックを呼ぶ
				CallOnQuitCallbacks() ;

				// 受信バッファキャッシュを破棄する
				DeleteSmallReceiveBufferCache() ;
				DeleteLargeReceiveBufferCache() ;

				if( m_UseLocalAssets == false )
				{
					// ローカル情報の上書きを行う(この処理は不要だが保険)
					SaveAllManifestInfo_Private() ;
				}

				m_Instance  = null ;
			}
		}
	
		//--------------------------------------------------------------------------

		/// <summary>
		/// アセットパスから関連するマニフェスト名とアセットバンドル名を取得する
		/// </summary>
		/// <param name="path"></param>
		/// <param name="rManifestName"></param>
		/// <param name="rAssetBundleName"></param>
		/// <param name="rAssetName"></param>
		/// <returns></returns>
		private bool GetManifestNameAndAssetBundleName( string path, out string manifestName, out string assetBundlePath, out string assetName )
		{
			manifestName		= string.Empty ;
			assetBundlePath		= string.Empty ;
			assetName			= string.Empty ;

			if( string.IsNullOrEmpty( path ) == true )
			{
				return false ;
			}

			int i, l ;

			// パスの先頭にスラッシュがあれば削除する
			i = 0 ;
			while( i <  path.Length && path[ i ] == '/' )
			{
				i ++ ;
			}

			if( i >= path.Length )
			{
				return false ;
			}

			if( i >  0 )
			{
				path = path[ ..i ] ;
			}

			if( string.IsNullOrEmpty( path ) == true )
			{
				return false ;
			}

			// パスの末尾にスラッシュがあれば削除する
			i = path.Length - 1 ;
			while( i >= 0 && path[ i ] == '/' )
			{
				i -- ;
			}

			if( i <  0 )
			{
				return false ;
			}

			if( i <  ( path.Length - 1 ) )
			{
				path = path[ ..( i + 1 ) ] ;
			}

			if( string.IsNullOrEmpty( path ) == true )
			{
				return false ;
			}

			//------------------------------------------------
			// コロンがあればマニフェストが指定されているものとみなす
			// そうでなければデフォルトのマニフェストを使用する

			l = path.Length ;
			if( l == 0 )
			{
				return false ;
			}

			i = path.IndexOf( m_ManifestSeparator ) ;
			if( i <  0 )
			{
				// マニフェストの指定が無い
				if( string.IsNullOrEmpty( m_DefaultManifestName ) == false )
				{
					// デフォルトマニフェスト名が設定されている
					manifestName = m_DefaultManifestName ;
					assetBundlePath = path ;
				}
				else
				{
					// デフォルトマニフェスト名が設定されていない
					return false ;
				}
			}
			else
			{
				// マニフェストの指定が有る

				// マニフェスト名を取得
				manifestName = path[ ..i ] ;

				// アセットバンドル名を取得する
				i ++ ;
				if( ( l - i ) >  0 )
				{
					assetBundlePath = path[ i.. ] ;
				}
				else
				{
					// アセットバンドル名が異常
					return false ;
				}
			}

			//----------------------------------

			i = assetBundlePath.IndexOf( "//" ) ;
			if( i >= 0 )
			{
				// 複合アセットのアセットバンドルとみなす
				assetName = assetBundlePath[ ( i + 2 ).. ] ;
				assetBundlePath = assetBundlePath[ ..i ] ;
			}

			// アセットバンドル名は大文字と小文字の区別を無くす(管理上は全て小文字管理)
			assetBundlePath = assetBundlePath.ToLower() ;

			return true ;
		}

		/// <summary>
		/// 破棄可能なアセットバンドルを破棄する
		/// </summary>
		private void AutoCleaningAssetBundleCache()
		{
			if( m_AsyncProcessingCount >  0 )
			{
				// 非同期ロード中はアセットバンドルキャッシュの自動破棄処理は停止させる
				return ;
			}

			//----------------------------------

			if( m_ManifestInfo == null || m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					// 各マニフェストのアセットバンドルキャッシュもクリアする
					manifestInfo.ClearAssetBundleCache( CacheReleaseTypes.Limited ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 数値をコンピューターのサイズ表記に変換する
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		private string GetSizeName( long size )
		{
			string sizeName = "Value Overflow" ;

			if( size <  1024L )
			{
				sizeName = size + " byte" ;
			}
			else
			if( size <  ( 1024L * 1024L ) )
			{
				sizeName = ( size / 1024L ) + " KB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
			{
				double value = ( double )size / ( double )( 1024L * 1024L * 1024L ) ;
				value = ( double )( ( int )( value * 1000 ) ) / 1000 ;	// 少数までわかるようにする
				sizeName = value + " GB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L ) ) + " TB" ;
			}
			else
			if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L * 1024L ) )
			{
				sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L * 1024L ) ) + " PB" ;
			}

			return sizeName ;
		}

		// Unity2017のクソみたいなバグ対策(HeaderのValueに ( ) が入っているとHeader全体がおかしくなる)
		private string EscapeHttpHeaderValue( string s )
		{
			s = s.Replace( "(", "&#40" ) ;
			s = s.Replace( ")", "&#41" ) ;
			return s ;
		}
	}
}

