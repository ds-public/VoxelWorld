using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Security.Cryptography ;

using UnityEngine ;
using UnityEngine.Networking ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

using StorageHelper ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(シングルトン) Version 2019/09/13 0
	/// </summary>
	public partial class AssetBundleManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AssetBundleManagerを生成する
		/// </summary>
		[MenuItem("GameObject/Helper/AssetBundleHelper/AssetBundleManager", false, 24)]
		public static void CreateAssetBundleManager()
		{
			GameObject go = new GameObject( "AssetBundleManager" ) ;
		
			Transform t = go.transform ;
			t.SetParent( null ) ;
			t.localPosition = Vector3.zero ;
			t.localRotation = Quaternion.identity ;
			t.localScale = Vector3.one ;
		
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
			m_Instance = GameObject.FindObjectOfType( typeof( AssetBundleManager ) ) as AssetBundleManager ;
			if( m_Instance == null )
			{
				GameObject go = new GameObject( "AssetBundleManager" ) ;
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

			// 保存されている全マニフェスト情報を読み出す
			LoadSystemFile() ;

			if( m_ManifestInfo == null )
			{
				m_ManifestInfo = new List<ManifestInfo>() ;
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
			m_ResourceCache = new Dictionary<string, UnityEngine.Object>() ;

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
	
		void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			AssetBundleManager instanceOther = GameObject.FindObjectOfType( typeof( AssetBundleManager ) ) as AssetBundleManager ;
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

	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;

			//-----------------------------
		
			Setup() ;
		}

		IEnumerator Start()
		{
			if( m_LoadManifestOnAwake == true )
			{
				// 自動でマニフェストをロードする
				yield return LoadAllManifestsAsync() ;
			}
		}
	
		void Update()
		{
			// 破棄対象になっているアセットバンドルを破棄する
			AutoCleaning() ;
		}
	
		void OnDestroy()
		{
			if( m_Instance == this )
			{
				// ローカル情報の上書きを行う
				SaveAllManifestInfo_Private() ;

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
			manifestName	= string.Empty ;
			assetBundlePath	= string.Empty ;
			assetName		= string.Empty ;

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
				path = path.Substring( i, path.Length - i ) ;
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
				path = path.Substring( 0, i + 1 ) ;
			}

			if( string.IsNullOrEmpty( path ) == true )
			{
				return false ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( m_DefaultManifestName ) == false )
			{
				// デフォルトマニフェスト名の指定がある
				manifestName = m_DefaultManifestName ;
				assetBundlePath = path ;
			}
			else
			{
				l = path.Length ;
				i = path.IndexOf( '/' ) ;
				if( l >  0 && i >= 0 )
				{
					// マニフェスト名を取得
					manifestName = path.Substring( 0, i ) ;
				}
				else
				{
					return false ;
				}

				// アセットバンドル名を取得する
				if( ( l - ( i + 1 ) ) >  0 )
				{
					assetBundlePath = path.Substring( i + 1, l - ( i + 1 ) ) ;
				}
				else
				{
					return false ;
				}
			}
			
			i = assetBundlePath.IndexOf( "//" ) ;
			if( i >= 0 )
			{
				// 複合アセットのアセットバンドルとみなす
				assetName = assetBundlePath.Substring( i + 2, assetBundlePath.Length - ( i + 2 ) ) ;
				assetBundlePath = assetBundlePath.Substring( 0, i ) ;
			}

			// アセットバンドル名は大文字と小文字の区別を無くす(管理上は全て小文字管理)
			assetBundlePath = assetBundlePath.ToLower() ;

			return true ;
		}
	}
}

