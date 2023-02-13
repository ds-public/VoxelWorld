using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

using StorageHelper ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		// 警告を出す依存数
		private const int TOO_MUCH_DEPENDENCE = 10 ;

		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		[Serializable]
		public partial class ManifestInfo
		{
			/// <summary>
			/// マニフェスト名
			/// </summary>
			public string	ManifestName = string.Empty ;

			/// <summary>
			/// ＣＲＣファイルのみ
			/// </summary>
			public bool		CrcOnly ;

			/// <summary>
			/// ストレージキャッシュ保存時のルートパス
			/// </summary>
			public string	StorageCacheRootPath = string.Empty ;

			/// <summary>
			/// StreamingAssetsのルートパス(設定があれば有効)
			/// </summary>
			public string	StreamingAssetsRootPath = string.Empty ;

			/// <summary>
			/// リモート側のアセットバンドルのルートパス(設定があれば有効)
			/// </summary>
			public string	RemoteAssetBundleRootPath = string.Empty ;

			/// <summary>
			/// ローカル側のアセットバンドルのルートパス(設定があれば有効)
			/// </summary>
			public string	LocalAssetBundleRootPath = string.Empty ;

			/// <summary>
			/// デバッグ動作用のローカルアセットのルートパス
			/// </summary>
			public string	LocalAssetsRootPath = string.Empty ;

			/// <summary>
			/// アセットバンドルの場所
			/// </summary>
			public LocationTypes	LocationType = LocationTypes.Storage ;

			/// <summary>
			/// StreamingAssets に対するダイレクトアクセスを有効にするか(実機 Android は不可)
			/// </summary>
			public bool		StreamingAssetsDirectAccessEnabled = false ;

			/// <summary>
			/// StreamingAssets 限定(読み出しのみ)
			/// </summary>
			public bool		IsStreamingAssetsOnly
			{
				get
				{
					return ( LocationType == LocationTypes.StreamingAssets && StreamingAssetsDirectAccessEnabled == true ) ;
				}
			}


			/// <summary>
			/// レガシータイプのアセットバンドルであるかどうかを示す
			/// </summary>
			public bool		LegacyType = false ;

			/// <summary>
			/// マニフェストごとのキャッシュサイズ(0で無制限)
			/// </summary>
			public long		CacheSize = 0L ;	// 0 は無制限

			/// <summary>
			/// マニフェストのロード進行度
			/// </summary>
			public float	Progress { get ; private set ; } = 0 ;

			/// <summary>
			/// プロトコルバージョン
			/// </summary>
			public int		HttpVersion { get ; private set ; } = 0 ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error { get ; private set ; } = "" ;

			/// <summary>
			/// 非同期版のロードを行う際に通信以外処理を全て同期で行う(時間は短縮させるが別のコルーチンの呼び出し頻度が下がる)
			/// </summary>
			public bool		FastLoadEnabled = true ;

			/// <summary>
			/// ダウンロード時に直接ストレージに保存を有効にするか
			/// </summary>
			public bool		DirectSaveEnabled = true ;

			/// <summary>
			/// アセットバンドルを扱う準備が完了しているかどうかを示す
			/// </summary>
			public bool		Completed { get ; private set ; } = false ;	// 本来は　private にして、アクセサで readonly にすべきだが、Editor を作成を省略するため、あえて public にする。

			/// <summary>
			/// 状態に変化があったかどうか
			/// </summary>
			public bool		Modified { get ; private set ; } = false ;	// 本来は　private にして、アクセサで readonly にすべきだが、Editor を作成を省略するため、あえて public にする。

			/// <summary>
			/// 現在ダウンロード中の数
			/// </summary>
			public int		DownloadingCount = 0 ;

			//----------------------------------------------------------

			/// <summary>
			/// ダウンロード状態
			/// </summary>
			public enum DownloadStates
			{
				Processing,
				Successed,
				Failed,
			}

			/// <summary>
			/// <summary>
			/// マニフェスト内の全アセットバンドル情報
			/// </summary>
			[SerializeField,Header("【アセットバンドル情報】")]
			private List<AssetBundleInfo> m_AssetBundleInfo ;	// ※readonly属性にするとインスペクターで表示できなくなるので付けてはだめ

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報の高速アクセス用のハッシュリスト
			/// </summary>
			private Dictionary<string,AssetBundleInfo> m_AssetBundleHash ;	// ショートカットアクセスのためディクショナリも用意する


			/// <summary>
			/// <summary>
			/// マニフェスト内の全アセットバンドル情報(固定)
			/// </summary>
			[SerializeField,Header("【アセットバンドル情報(固定)】")]
			private List<AssetBundleInfo> m_AssetBundleInfo_Constant ;	// ※readonly属性にするとインスペクターで表示できなくなるので付けてはだめ

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報の高速アクセス用のハッシュリスト(固定)
			/// </summary>
			private Dictionary<string,AssetBundleInfo> m_AssetBundleHash_Constant ;	// ショートカットアクセスのためディクショナリも用意する

			//----------------------------------

			// 展開中のマニフェストのインスタンス
			private AssetBundleManifest	m_Manifest ;

			//----------------------------------------------------------

			/// <summary>
			/// アセットバンドルキャッシュ
			/// </summary>
			[Serializable]
			public class AssetBundleCacheElement
			{
				public	string		Path ;			// デバッグ表示用のパス
				public	AssetBundle	AssetBundle ;
				public	bool		IsCaching ;		// アセットバンドルをキャッシュするか(ただしシーン切り替え時などのキャッシュ消去呼び出し時は破棄される)
				public	bool		IsRetain ;		// キャッシュしたアセットバンドルは明示的な破棄要求があるまで破棄しないようにするか
				public	int			FrameCount ;

				public	bool		Mark ;			// スイープコントロール用のマーク

				public AssetBundleCacheElement( string path, AssetBundle assetBundle, bool isCaching, bool isRetain )
				{
					Path			= path ;
					AssetBundle		= assetBundle ;
					IsCaching		= isCaching ;
					IsRetain		= isRetain ;

					Mark			= true ;
				}
			}

			/// <summary>
			/// アセットバンドルのキャッシュ
			/// </summary>
			private Dictionary<string,AssetBundleCacheElement> m_AssetBundleCache = null ;

#if UNITY_EDITOR
			/// <summary>
			/// デバッグ用のキャッシュ中のアセットバンドルの表示リスト
			/// </summary>
			[SerializeField,Header("【アセットバンドルキャッシュ情報】")]
			private List<AssetBundleCacheElement>	m_AssetBundleCacheInfo = null ;
#endif

			/// <summary>
			/// アセットバンドルのメモリキャッシュへの存在確認と同時にキャッシャするならば状態の更新
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="isCaching"></param>
			/// <returns></returns>
			public bool CheckAssetBundleCache( string assetBundlePath, bool isCaching )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == true )
				{
					// 既に登録済みなのでフレームカウントを更新して戻る(少なくともこのフレームで破棄されてはならない)
					m_AssetBundleCache[ assetBundlePath ].FrameCount = Time.frameCount ;
					if( isCaching == true )
					{
						// 既にキャッシュに溜まっていてもキャッシングが有効になっていない場合に有効化指示があれば有効化する
						m_AssetBundleCache[ assetBundlePath ].IsCaching = true ;
					}

					// メモリキャッシュに存在する
					return true ;
				}

				// メモリキャッシュには存在しない
				return false ;
			}

			/// <summary>
			/// キャッシュにアセットバンドルを追加する
			/// </summary>
			/// <param name="name"></param>
			/// <param name="assetBundle"></param>
			public void AddAssetBundleCache( string assetBundlePath, AssetBundle assetBundle, bool isCaching, bool isRetain )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == true )
				{
					// 既に登録済みなのでフレームカウントを更新して戻る(少なくともこのフレームで破棄されてはならない)
					m_AssetBundleCache[ assetBundlePath ].FrameCount	= Time.frameCount ;
					m_AssetBundleCache[ assetBundlePath ].Mark			= true ;
					return ;
				}

				// キャッシュを追加する
				var element = new AssetBundleCacheElement( assetBundlePath, assetBundle, isCaching, isRetain ) ;
				m_AssetBundleCache.Add( assetBundlePath, element ) ;
#if UNITY_EDITOR
				m_AssetBundleCacheInfo.Add( element ) ;
#endif
			}

			/// <summary>
			/// キャッシュからアセットバンドルを強制的に削除する(内部処理)
			/// </summary>
			/// <param name="assetBundlePath"></param>
			public bool RemoveAssetBundleCacheForced( string assetBundlePath )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == false )
				{
					return false ;	// 元々キャッシュには存在しない
				}

				m_AssetBundleCache[ assetBundlePath ].AssetBundle.Unload( false ) ;
				m_AssetBundleCache[ assetBundlePath ].AssetBundle = null ;

				// 以下順番に注意(先に本体を削ってしまうとデバッグ用の表示から削れなくなる=null例外)
#if UNITY_EDITOR
				m_AssetBundleCacheInfo.Remove( m_AssetBundleCache[ assetBundlePath ] ) ;
#endif
				m_AssetBundleCache.Remove( assetBundlePath ) ;

				return true ;
			}

			// キャッシュ削除時のワーク
//			private readonly Dictionary<string,AssetBundleCacheElement> m_RemoveElements = new Dictionary<string,AssetBundleCacheElement>() ;

			/// <summary>
			/// アセットバンドルキャッシュをクリアする
			/// </summary>
			public void ClearAssetBundleCache( bool isAbsolute, bool noMarkingOnly )
			{
				if( m_AssetBundleCache.Count == 0 )
				{
					return ;
				}

				if( isAbsolute == false )
				{
					// 非キャッシグ・非保持・異なるフレームカウントのものを破棄する
					List<string> paths = new List<string>() ;

					foreach( var element in m_AssetBundleCache )
					{
						if( element.Value.IsCaching == false && element.Value.IsRetain == false && element.Value.FrameCount != Time.frameCount )
						{
							element.Value.AssetBundle.Unload( false ) ;
							element.Value.AssetBundle = null ;

							paths.Add( element.Key ) ;
						}
					}

					if( paths.Count >  0 )
					{
						foreach( var path in paths )
						{
#if UNITY_EDITOR
							m_AssetBundleCacheInfo.Remove( m_AssetBundleCache[ path ] ) ;
#endif
							m_AssetBundleCache.Remove( path ) ;
						}
					}
				}
				else
				{
					// 強制的に全てのアセットバンドルを破棄する
					List<string> paths = new List<string>() ;

					foreach( var element in m_AssetBundleCache )
					{
						element.Value.AssetBundle.Unload( false ) ;
						element.Value.AssetBundle = null ;

						paths.Add( element.Key ) ;
					}
#if UNITY_EDITOR
					m_AssetBundleCacheInfo.Clear() ;
#endif
					m_AssetBundleCache.Clear() ;	
				}
			}


			/// <summary>
			/// アセットバンドルキャッシュのマークをクリアする
			/// </summary>
			public void ClearAssetBundleCacheMarks()
			{
				if( m_AssetBundleCache.Count == 0 )
				{
					return ;
				}

				foreach( var element in m_AssetBundleCache )
				{
					if( element.Value.IsRetain == false )
					{
						// 非常駐タイプのもののマークを消去する
						element.Value.Mark = false ;
					}
				}
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// ストレージに保存するアセットバンドルの情報
			/// </summary>
			[Serializable]
			public class AssetBundleFileInfo
			{
				public	string		Path ;
				public	string		Hash ;
				public	long		Size ;
				public	uint		Crc ;
				public	bool		IsCompleted ;
				public	string[]	Tags ;
				public	long		LastUpdateTime ;
			}

			[Serializable]
			public class StoredManifest
			{
				public List<AssetBundleFileInfo>	Files = new List<AssetBundleFileInfo>() ;
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// セットアップ
			/// </summary>
			/// <param name="filePath">リモート側のマニフェストのパス</param>
			/// <param name="cacheSize">マニフェストごとのキャッシュサイズ(0で無制限)</param>
			public void Setup
			(
				string	manifestName, bool crcOnly, string storageCacheRootPath,
				string	streamingAssetsRootPath,
				bool	streamingAssetsDirectAccessEnabled,
				string	remoteAssetsRootPath,
				string	localAssetBundleRootPath,
				string	localAssetsRootPath,
				LocationTypes	locationType,
				long	cacheSize,
				bool fastLoadEnabled,
				bool directSaveEnabled
			)
			{
				//---------------------------------------------------------
				// 不正な指定であればエラーを出して抜ける

				if( locationType == LocationTypes.StreamingAssets || locationType == LocationTypes.StorageAndStreamingAssets )
				{
					if( string.IsNullOrEmpty( streamingAssetsRootPath ) == true )
					{
						Debug.LogError( "[Error]streamingAssetsRootPath is null" ) ;
						return ;
					}
				}

				// 以下は許容する(リモートのマニフェストがダウンロード出来なかった場合の動作確認[デバッグ]用)
				else
				if( locationType == LocationTypes.Storage || locationType == LocationTypes.StorageAndStreamingAssets )
				{
					if( string.IsNullOrEmpty( remoteAssetsRootPath ) == true && string.IsNullOrEmpty( localAssetBundleRootPath ) == true )
					{
						Debug.LogWarning( "[Error]remoteAssetsRootPath and localAssetBundleRootPath is null" ) ;
//						return ;	// 許容
					}
				}

				//---------------------------------------------------------

				if( m_AssetBundleInfo == null )
				{
					m_AssetBundleInfo = new List<AssetBundleInfo>() ;
				}
				else
				{
					m_AssetBundleInfo.Clear() ;
				}

				if( m_AssetBundleHash == null )
				{
					m_AssetBundleHash = new Dictionary<string, AssetBundleInfo>() ;
				}
				else
				{
					m_AssetBundleHash.Clear() ;
				}

				//---------------------------------------------------------

				// 各名前を保存する

				// マニフェストファイル名(拡張子込み)を保存する
				ManifestName = manifestName ;

				CrcOnly						= crcOnly ;

				StorageCacheRootPath		= string.Empty ;
				if( string.IsNullOrEmpty( storageCacheRootPath ) == false )
				{
					StorageCacheRootPath		= storageCacheRootPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) + "/" ;
				}

				//---------------------------------

				StreamingAssetsRootPath		= string.Empty ;
				if( string.IsNullOrEmpty( streamingAssetsRootPath ) == false )
				{
					StreamingAssetsRootPath		= streamingAssetsRootPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) + "/" ;
				}

				StreamingAssetsDirectAccessEnabled	= streamingAssetsDirectAccessEnabled ;

				//-------------

				RemoteAssetBundleRootPath	= string.Empty ;
				if( string.IsNullOrEmpty( remoteAssetsRootPath ) == false )
				{
					RemoteAssetBundleRootPath	= remoteAssetsRootPath.Replace( "\\", "/" ).TrimEnd( '/' ) + "/" ;
				}

				LocalAssetBundleRootPath	= string.Empty ;
				if( string.IsNullOrEmpty( localAssetBundleRootPath ) == false )
				{
					LocalAssetBundleRootPath	= localAssetBundleRootPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) + "/" ;
				}

				//-------------

				LocalAssetsRootPath			= string.Empty ;
				if( string.IsNullOrEmpty( localAssetsRootPath ) == false )
				{
					LocalAssetsRootPath			= localAssetsRootPath.Replace( "\\", "/" ).TrimStart( '/' ).TrimEnd( '/' ) + "/" ;
				}

				//-------------

				LocationType				= locationType ;

				//---------------------------------------------------------

				CacheSize			= cacheSize ;
				FastLoadEnabled		= fastLoadEnabled ;
				DirectSaveEnabled	= directSaveEnabled ;

				//---------------------------------------------------------

				if( m_AssetBundleCache == null )
				{
					m_AssetBundleCache = new Dictionary<string, AssetBundleCacheElement>() ;
#if UNITY_EDITOR
					m_AssetBundleCacheInfo = new List<AssetBundleCacheElement>() ;
#endif
				}
				else
				{
					m_AssetBundleCache.Clear() ;
#if UNITY_EDITOR
					m_AssetBundleCacheInfo.Clear() ;
#endif
				}

				//-----------------------------------------------------------------------------------------
				// StreamingAssets に固定アセットバンドルを格納しているかどうか

				// StorageAndStreamingAssets から Storage または StreamingAssets に変わった場合も初期化が必要

				if( m_AssetBundleInfo_Constant == null )
				{
					m_AssetBundleInfo_Constant = new List<AssetBundleInfo>() ;
				}
				else
				{
					m_AssetBundleInfo_Constant.Clear() ;
				}

				if( m_AssetBundleHash_Constant == null )
				{
					m_AssetBundleHash_Constant = new Dictionary<string, AssetBundleInfo>() ;
				}
				else
				{
					m_AssetBundleHash_Constant.Clear() ;
				}
			}

			//-----------------------------------------------------------------------------------

			//-----------------------------------------------------------------------------------

			/// <summary>
			/// 通信エラーチェック
			/// </summary>
			/// <param name="unityWebRequest"></param>
			/// <returns></returns>
			private bool IsNetworkError( UnityWebRequest unityWebRequest )
			{
#if UNITY_2020_2_OR_NEWER
				var result = unityWebRequest.result ;
				return
					( result == UnityWebRequest.Result.ConnectionError			) ||
					( result == UnityWebRequest.Result.DataProcessingError		) ||
					( result == UnityWebRequest.Result.ProtocolError			) ||
					( string.IsNullOrEmpty( unityWebRequest.error ) == false	) ;
#else
				return
					( unityWebRequest.isHttpError		== true					) ||
					( unityWebRequest.isNetworkError	== true					) ||
					( string.IsNullOrEmpty( unityWebRequest.error ) == false	) ;
#endif
			}


			/// <summary>
			/// 保持している情報をクリアする
			/// </summary>
			public void Clear()
			{
				m_AssetBundleInfo.Clear() ;
				m_AssetBundleHash.Clear() ;
			}

			/// <summary>
			/// 全てのアセットバンドルを更新が必要な対象とする
			/// </summary>
			internal void SetAllUpdateRequired()
			{
				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					assetBundleInfo.UpdateRequired = true ;	// 更新が必要扱いにする
				}
				Modified = true ;
			}

			/// <summary>
			/// キャッシュ内の全てのアセットバンドルファイルを削除する
			/// </summary>
			internal void RemoveAllFiles()
			{
				if( SecurityEnabled == false )
				{
					// パスは難読化されていない

					// フォルダごとまとめて削除する

					// ベースパス
					string storagePath = StorageCacheRootPath + ManifestName + "/" ;
				
					// マニフェストが内包する全アセットバンドルファイルを削除する
					StorageAccessor_Remove( storagePath, true ) ;
				}
				else
				{
					// パスが難読化されている

					string storagePath ;

					// 対象ファイルを個別に全て削除する
					foreach( var assetBundleInfo in	m_AssetBundleInfo )
					{
						// アセットバンドルファイルの保存パス
						storagePath = StorageCacheRootPath + ManifestName + "/" + assetBundleInfo.Path ;

						if( StorageAccessor_Exists( storagePath ) == StorageAccessor.Target.File )
						{
							StorageAccessor_Remove( storagePath ) ;
						}
					}
				}

				//---------------------------------

				// マニフェストが内包する全アセットバンドルファイルを更新対象とする
				SetAllUpdateRequired() ;

				// マニフェスト情報を保存する
				Save() ;
			}

			// マニフェストが展開中かを示す
			private bool m_Busy = false ;

			//----------------------------------------------------------

			/// <summary>
			/// アセットバンドルの追加情報
			/// </summary>
			[Serializable]
			public class AssetBundleAdditionalInfo
			{
				public string	Path ;
				public int		Size ;
				public uint		Crc ;
				public string[]	Tags ;

				public AssetBundleAdditionalInfo( int size, uint crc, string[] tags )
				{
					Size	= size ;
					Crc		= crc ;
					Tags	= tags ;
				}
			}

			/// <summary>
			/// Crc[Json版]をデシリアライズするためのパッキングクラス
			/// </summary>
			[Serializable]
			public class JsonDeserializer
			{
				public AssetBundleAdditionalInfo[]	AssetBundleFiles ;
			}

			/// <summary>
			/// マニフェストを展開する(非同期)
			/// </summary>
			/// <param name="onCompleted"></param>
			/// <param name="onError"></param>
			/// <param name="instance"></param>
			/// <returns></returns>
			internal protected IEnumerator LoadAsync( Action onCompleted, Action<string> onError, AssetBundleManager instance )
			{
				//---------------------------------------------------------

				while( m_Busy )
				{
					yield return null ;	// 同じマニフェストに対し同時に処理を行おうとした場合は排他処理を行う
				}

				m_Busy = true ;

				//---------------------------------------------------------
				// 固定アセットバンドルが存在する場合に固定アセットバンドルの情報をロードする

				if( LocationType == LocationTypes.StorageAndStreamingAssets )
				{
					yield return instance.StartCoroutine( LoadConstantAsync( null, null, instance ) ) ;
				}

				//------------------------------------------------------------
	
				Progress = 0 ;
				Error = string.Empty ;

				//------------------------------------
	
				KeyValuePair<string,string>[] assetBundlePathAndHashs = null ;


				int i, l ;

				bool manifestDone = false ;
				byte[] manifestData = null ;

				bool crcCsvDone = false ;
				string crcCsvText = null ;

				bool crcJsonDone = false ;
				string crcJsonText = null ;

				//------------------------------------

				// Manifest ファイルをダウンロードする
				if( CrcOnly == false )
				{
					instance.StartCoroutine( LoadManifestAsync( ( bool _1, byte[] _2 ) => { manifestDone = _1 ; manifestData = _2 ; }, instance ) ) ;
				}
				else
				{
					manifestDone = true ;
				}

				// Crc[Csv版] ファイルをダウンロードする
//				instance.StartCoroutine( LoadCrcAsync( "csv", ( bool _1, string _2 ) => { crcCsvDone = _1 ; crcCsvText = _2 ; }, instance ) ) ;
				crcCsvDone = true ;

				// Crc[Json版] ファイルをダウンロードする
				instance.StartCoroutine( LoadCrcAsync( "json", ( bool _1, string _2 ) => { crcJsonDone = _1 ; crcJsonText = _2 ; }, instance ) ) ;
//				crcJsonDone = true ;

				//-------------

				// Manifest と Crc 両方のダンロードが終わるのを待つ
				while( true )
				{
					if( manifestDone == true && crcCsvDone == true && crcJsonDone == true )
					{
						break ;
					}

					yield return null ;
				}

				//---------------------------------------------------------

				if( CrcOnly == false )
				{
					// Manifest を展開する

					//--------------------------------------------------------

					if( manifestData == null || manifestData.Length == 0 )
					{
						// データが取得出来ない
						Error = "Could not load data : " + ManifestName ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;

						yield break ;
					}

					//------------------------------------

					// バイナリからアセットバンドルを生成する
					AssetBundle assetBundle = AssetBundle.LoadFromMemory( manifestData ) ;
					if( assetBundle == null )
					{
						// アセットバンドルが生成出来ない
						Error = "Could not create AssetBundle" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}

					m_Manifest = assetBundle.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" ) ;
					if( m_Manifest == null )
					{
						// マニフェストが取得出来ない
						assetBundle.Unload( true ) ;

						Error = "Could not get Manifest" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}

					string[] assetBundlePaths = m_Manifest.GetAllAssetBundles() ;
					if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						assetBundle.Unload( true ) ;

						Error = "No AssetBundles" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}
					
					// パスとハッシュを取得
					l = assetBundlePaths.Length ;
					assetBundlePathAndHashs = new KeyValuePair<string, string>[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						assetBundlePathAndHashs[ i ] = new KeyValuePair<string, string>( assetBundlePaths[ i ], m_Manifest.GetAssetBundleHash( assetBundlePaths[ i ] ).ToString() ) ;
					}

					// アセットバンドル破棄(重要)
					assetBundle.Unload( false ) ;
				}

				//---------------------------------------------------------
				// CRCを展開する

				int size ;
				uint crc ;
				string[] tags ;

				Dictionary<string,AssetBundleAdditionalInfo> additionalInfoHash = null ;

				if( string.IsNullOrEmpty( crcCsvText ) == false )
				{
					// CRC[CSV版]ファイルを展開する

					//--------------------------------------------------------
#if UNITY_EDITOR
					// 確認用にＣＲＣ[CSV版]ファイルを保存する
					string path =  StorageCacheRootPath + ManifestName + "/" ;
					StorageAccessor_SaveText( path + ManifestName + ".csv", crcCsvText ) ;
					Debug.Log( "[AssetBundleManager] Save CRC[CSV] File : " + ManifestName + "\n -> "+ path + ManifestName + ".csv" ) ;
#endif
					//--------------------------------------------------------

					additionalInfoHash = new Dictionary<string,AssetBundleAdditionalInfo>() ;

					// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する
					string[] line = crcCsvText.Split( '\n' ) ;
					l = line.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( line[ i ] ) == false )
						{
							string[] keyAndValue = line[ i ].Split( ',' ) ;
	
							if( keyAndValue.Length >  0  && string.IsNullOrEmpty( keyAndValue[ 0 ] ) == false )
							{
								size = 0 ;
								if( keyAndValue.Length >  1 && string.IsNullOrEmpty( keyAndValue[ 1 ] ) == false )
								{
									int.TryParse( keyAndValue[ 1 ], out size ) ;
								}
								crc = 0 ;
								if( keyAndValue.Length >  2 && string.IsNullOrEmpty( keyAndValue[ 2 ] ) == false )
								{
									uint.TryParse( keyAndValue[ 2 ], out crc ) ;
								}
								tags = null ;
								if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
								{
									tags = keyAndValue[ 3 ].Split( ' ' ) ;
								}

								additionalInfoHash.Add( keyAndValue[ 0 ].ToLower(), new AssetBundleAdditionalInfo( size, crc, tags ) ) ;
							}
						}
					}
				}

				//-------------

				if( string.IsNullOrEmpty( crcJsonText ) == false )
				{
					// CRC[JSON版]ファイルを展開する

					//--------------------------------------------------------
#if UNITY_EDITOR
					// 確認用にＣＲＣ[CSV版]ファイルを保存する
					string path =  StorageCacheRootPath + ManifestName + "/" ;
					StorageAccessor_SaveText( path + ManifestName + ".json", crcCsvText ) ;
					Debug.Log( "[UnityEditorOnly : AssetBundleManager] Save CRC[JSON] File : " + ManifestName + "\n -> "+ path + ManifestName + ".json" ) ;
#endif
					//--------------------------------------------------------

					var json = JsonUtility.FromJson<JsonDeserializer>( crcJsonText ) ;
					if( json != null )
					{
						additionalInfoHash = new Dictionary<string,AssetBundleAdditionalInfo>() ;

						foreach( var assetBundleFile in json.AssetBundleFiles )
						{
							additionalInfoHash.Add( assetBundleFile.Path.ToLower(), assetBundleFile ) ;
						}
					}
				}

				//---------------------------------------------------------
				// オンメモリのローカルマニフェストを生成する

				m_AssetBundleInfo.Clear() ;
				m_AssetBundleHash.Clear() ;

				ManifestInfo.AssetBundleInfo node ;

				// Manifest
				if( assetBundlePathAndHashs != null )
				{
					// アセットバンドルファイルの情報を追加する
					foreach( var assetBundlePathAndHash in assetBundlePathAndHashs )
					{
						size	= 0 ;
						crc		= 0 ;
						tags	= null ;
						if( additionalInfoHash != null && additionalInfoHash.ContainsKey( assetBundlePathAndHash.Key ) == true )
						{
							size	= additionalInfoHash[ assetBundlePathAndHash.Key ].Size ;                                                                                                                                                                                                                  
							crc		= additionalInfoHash[ assetBundlePathAndHash.Key ].Crc ;
							tags	= additionalInfoHash[ assetBundlePathAndHash.Key ].Tags ;
						}

						node = new ManifestInfo.AssetBundleInfo( assetBundlePathAndHash.Key, assetBundlePathAndHash.Value, size, crc, tags, 0L ) ;
						m_AssetBundleInfo.Add( node ) ;
						if( m_AssetBundleHash.ContainsKey( assetBundlePathAndHash.Key ) == false )
						{
							m_AssetBundleHash.Add( assetBundlePathAndHash.Key, node ) ;
						}
					}
				}

				// Crc
				if( additionalInfoHash != null && additionalInfoHash.Count >  0 )
				{
					// 非アセットバンドル化ファイルの情報を追加する
					foreach( var additionalInfo in additionalInfoHash )
					{
						if( m_AssetBundleHash.ContainsKey( additionalInfo.Key ) == false )
						{
							// アセットバンドルとして追加されていないので非アセットバンドル化ファイルとみなす
							size	= additionalInfo.Value.Size ;                                                                                                                                                                                                                  
							crc		= additionalInfo.Value.Crc ;
							tags	= additionalInfo.Value.Tags ;

							node = new ManifestInfo.AssetBundleInfo( additionalInfo.Key, "", size, crc, tags, 0L ) ;	// アセットバンドルではないのでハッシュ値は存在しない
							m_AssetBundleInfo.Add( node ) ;
							if( m_AssetBundleHash.ContainsKey( additionalInfo.Key ) == false )
							{
								m_AssetBundleHash.Add( additionalInfo.Key, node ) ;
							}
						}
					}
				}
				else
				{
					Debug.LogWarning( "[CRC Not found] " + ManifestName + ".crc" ) ;
				}

				//-----------------------------------------------------------------------------------------

				bool isRequiredLocalManifestUpdating = false ;

				if( IsStreamingAssetsOnly == false )
				{
					// ローカルマニフェストの情報を元に更新すべきアセットバンドルのフラグを立てる
					isRequiredLocalManifestUpdating = Verify( instance ) ;
				}

				//--------------------------------------------------------

				if( LocationType == LocationTypes.StorageAndStreamingAssets )
				{
					// Storage と StreamingAssets のどちらを優先すべきか判定する

					foreach( var assetBundleInfo in m_AssetBundleInfo )
					{
						if( assetBundleInfo.UpdateRequired == true )
						{
							// 更新が必要なもののみ StreamingAssets と比較する

							// 基本はストレージ参照になる
							assetBundleInfo.LocationPriority = LocationPriorities.Storage ;

							if( m_AssetBundleHash_Constant.ContainsKey( assetBundleInfo.Path ) == true )
							{
								// StreamingAssets にも存在する
								var assetBundleInfo_Constant = m_AssetBundleHash_Constant[ assetBundleInfo.Path ] ;

								if( StreamingAssetsDirectAccessEnabled == true )
								{
									// StreamingAssets へダイレクトアクセス可能な環境のみ StreamingAssets に残存するアセットバンドルが使用できる

									if( StorageAccesor_ExistsInStreamingAssets( StreamingAssetsRootPath + "/" + assetBundleInfo.Path ) == true )
									{
										// StreamingAssets にファイルが存在している
										if
										(
											assetBundleInfo.Size	== assetBundleInfo_Constant.Size	&&
											assetBundleInfo.Crc		== assetBundleInfo_Constant.Crc
										)
										{
											// 完全に同一ファイルが StreamingAssets に存在しているので StreamingAssets の方を優先する
											assetBundleInfo.LocationPriority = LocationPriorities.StreamingAssets ;

											// StreamingAssest へのダイレクトアクセスが有効なのでダウンロード済み扱いとする
											assetBundleInfo.UpdateRequired = false ;	// ダウンロード済み扱いとする
											Modified = true ;
										}
									}
								}
							}
						}
					}
				}

				//--------------------------------------------------------

				// 使用可能状態となった
				Completed = true ;

				//--------------------------------------------------------

				if( IsStreamingAssetsOnly == false )
				{
					// ローカルマニフェストの保存(更新)が必要であればここで保存(更新)しておく
					if( isRequiredLocalManifestUpdating == true )
					{
						Save() ;
					}

					//------------------------------------------------------------

#if UNITY_EDITOR
					Debug.Log( "<color=#00FF00>HTTP Version = " + HttpVersion + "</color>" ) ;
#endif
					// プロトコルの設定を行う
					SetHttpSettings() ;
				}
				else
				{
					// StreamingAssets のみ参照且つダイレクトアクセスが可能な環境

					// StreamingAssets 使用且つダイレクトアクセスの場合は全てのアセットバンドルを最新版をダウンロード済みの完全な状態として扱う
					foreach( var assetBundleInfo in m_AssetBundleInfo )
					{
						assetBundleInfo.UpdateRequired	= false ;
						assetBundleInfo.LocationPriority = LocationPriorities.StreamingAssets ;	// 処理上は意味は無いが Inspector で見た時に勘違いするので優先を StreamingAssets にしておく
					}
				}

				//------------------------------------------------------------
				
				m_Busy = false ;	// 処理終了

				//-----------------------------------------------------------

				onCompleted?.Invoke() ;
			}

			// マニフェストフェイルをダウンロードする(ＣＲＣファイルと並列ダウンロード用)
			private IEnumerator LoadManifestAsync( Action<bool,byte[]> onLoaded, AssetBundleManager instance )
			{
				byte[] data = null ;

				//---------------------------------------------------------

				// StreamingAssets からロードを試みる
				if( data == null && string.IsNullOrEmpty( StreamingAssetsRootPath ) == false && ( LocationType == LocationTypes.StreamingAssets || ( LocationType == LocationTypes.StorageAndStreamingAssets && string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == true && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == true ) ) )
				{
					// ストリーミングアセットから読み出してみる
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( StreamingAssetsRootPath + ManifestName, ( _1, _2 ) => { data = _1 ; } ) ) ;
				}

				// Remote
				if( data == null && string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == true && LocationType != LocationTypes.StreamingAssets )
				{
					string path = RemoteAssetBundleRootPath + ManifestName + "?time=" + GetClientTime() ;

					yield return instance.StartCoroutine
					(
						DownloadFromRemote
						(
							path,
							0,		// 予めサイズがわかっているとメモリ確保が若干最適化される
							null,	// ダイレクトにストレージに保存する場合のストレージのパスを指定する
							0,
							( DownloadStates state, byte[] downloadedData, float progress, long downloadedSize, string errorMessage, int version ) =>
							{
								Progress = progress ;

								if( version != 0 )
								{
									// プロトコルパージョンを保存する
									HttpVersion = version ;
								}

								if( state == DownloadStates.Successed )
								{
									// 成功
									data = downloadedData ;
								}
								else
								if( state == DownloadStates.Failed )
								{
									// 失敗
									Error = errorMessage ;
								}
							},
							instance
						)
					) ;
				}

#if UNITY_EDITOR
				// UnityEditor 専用 : Local
				if( data == null && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == false && LocationType != LocationTypes.StreamingAssets )
				{
					// ローカルファイル からダウンロードを試みる
					data = File_Load( LocalAssetBundleRootPath + ManifestName ) ;
				}
#endif
				//---------------------------------

				// ダウンロードしたデータをコールバックで返す
				onLoaded?.Invoke( true, data ) ;
			}

			// ＣＲＣファイルをダウンロードする(マニフェストファイルと並列ダウンロード用)
			private IEnumerator LoadCrcAsync( string extension, Action<bool,string> onLoaded, AssetBundleManager instance )
			{
				string text = null ;
				byte[] data = null ;

				//---------------------------------------------------------

				// StreamingAssets
				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( StreamingAssetsRootPath ) == false && ( LocationType == LocationTypes.StreamingAssets || ( LocationType == LocationTypes.StorageAndStreamingAssets && string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == true && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == true ) ) )
				{
					// ストリーミングアセットから読み出す
					yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssetsAsync( StreamingAssetsRootPath + ManifestName + "." + extension, ( _1, _2 ) => { text = _1 ; } ) ) ;
				}

				// Remote
				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == true && LocationType != LocationTypes.StreamingAssets )
				{
					string path = RemoteAssetBundleRootPath + ManifestName + "." + extension + "?time=" + GetClientTime() ;

					yield return instance.StartCoroutine
					(
						DownloadFromRemote
						(
							path,
							0,		// 予めサイズがわかっているとメモリ確保が若干最適化される
							null,	// ダイレクトにストレージに保存する場合のストレージのパスを指定する
							0,
							( DownloadStates state, byte[] downloadedData, float progress, long downloadedSize, string errorMessage, int version ) =>
							{
								Progress = progress ;

								if( version != 0 )
								{
									// プロトコルパージョンを保存する
									HttpVersion = version ;
								}

								if( state == DownloadStates.Successed )
								{
									// 成功
									data = downloadedData ;
								}
								else
								if( state == DownloadStates.Failed )
								{
									// 失敗
									Error = errorMessage ;
								}
							},
							instance
						)
					) ;


					if( string.IsNullOrEmpty( Error ) == true && data != null && data.Length >  0 )
					{
						// 成功
						text = UTF8Encoding.UTF8.GetString( data ) ;
					}
				}

#if UNITY_EDITOR
				// UnityEditor 専用 : Local
				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == false && LocationType != LocationTypes.StreamingAssets )
				{
					// ローカルファイル からダウンロードを試みる
					text = File_LoadText( LocalAssetBundleRootPath + ManifestName + "." + extension ) ;
				}
#endif
				//---------------------------------

				// ダウンロードしたデータをコールバックで返す
				onLoaded?.Invoke( true, text ) ;
			}

			//--------------------------------------------------------------------------

			// ローカルの情報をマージし更新すべきファイルのフラグを立てる
			private bool Verify( AssetBundleManager instance )
			{
				//---------------------------------------------------------

				string path = StorageCacheRootPath + ManifestName + "/" ;

				string fullPath = path + ManifestName + ".manifest" ;

				if( StorageAccessor_Exists( fullPath ) != StorageAccessor.Target.File )
				{
					// ファイルが保存されていない(保存する必要がある)
					Modified = true ;
					return true ;
				}

				string text = StorageAccessor_LoadText( fullPath ) ;
				if( string.IsNullOrEmpty( text ) == true )
				{
					// ローカルマニフェストは保存されていない(保存する必要がある)
					Modified = true ;
					return true ;
				}

				//---------------------------------

				// ローカルマニフェストをメモリに展開する
				StoredManifest storedManifest ;

				try
				{
					storedManifest = JsonUtility.FromJson<StoredManifest>( text ) ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( e.Message ) ;
					return true ;	// エラー発生(再保存すべし)
				}

				//---------------------------------------------------------

				// メモリに展開されたローカルマニフェストの各アセットバンドル情報を参照高速化のためハッシュ参照化する
				AssetBundleFileInfo node ;
				List<AssetBundleFileInfo> files = storedManifest.Files ;
				Dictionary<string,AssetBundleFileInfo> hash = new Dictionary<string, AssetBundleFileInfo>() ;

				foreach( var file in files )
				{
					hash.Add( file.Path, file ) ;
				}

				//----------------------------------------------
					
				// リモートマニフェストをベースに更新が必要なアセットバンドルをチェックする
				string assetBundlePath ;
				long size ;

				bool updateRequired ;
				int modifiedCount = 0 ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					// チェック前の更新フラグを比較用に保存する
					updateRequired = assetBundleInfo.UpdateRequired ;

					assetBundlePath = assetBundleInfo.Path ;
					if( hash.ContainsKey( assetBundlePath ) == true )
					{
						// 既に記録した事があるアセットバンドルである
						node = hash[ assetBundlePath ] ;

						if( string.IsNullOrEmpty( assetBundleInfo.Hash ) == false )
						{
							// 純アセットバンドル(assetBundleInfo=ダウンロードした最新のもの・node=ローカルストレージ保存のもの)
							if( assetBundleInfo.Hash == node.Hash && assetBundleInfo.Size == node.Size )
							{
								// ハッシュとサイズは同じである

								// 逐次保存の場合ファイルサイズは正してもＣＲＣに問題があるな可能性があるのでファイルが完全な状態かもチェックする
								if( node.IsCompleted == true )
								{
									// 実際にファイルが存在しているか確認する
									size = StorageAccessor_GetSize( path + assetBundlePath ) ;
									if( size >  0 && size == assetBundleInfo.Size )
									{
										// 実際にファイルが存在しサイズも問題ないのでこのファイルは更新しない
										assetBundleInfo.LastUpdateTime	= node.LastUpdateTime ;
										assetBundleInfo.Size			= size ;		// ＣＲＣファイルでのサイズと同じになるはず(ＣＲＣファイルにもサイズを持たせる事：サイズ部分にＣＲＣ値がずれて入り半日潰すバグを発生させた事があった事を忘れるな)
										assetBundleInfo.IsCompleted		= true ;		// 完全な状態のファイルである
										assetBundleInfo.UpdateRequired	= false ;		// 更新不要
									}
								}
							}
							else
							{
								// ハッシュかサイズが異なる(リモートの方が新しいファイルとなっている)
								modifiedCount ++ ;
							}
						}
						else
						{
							// 非アセットバンドル(assetBundleInfo=ダウンロードした最新のもの・node=ローカルストレージ保存のもの)
							if( assetBundleInfo.Crc == node.Crc && assetBundleInfo.Size == node.Size )
							{
								// ＣＲＣとサイズは同じである

								// 逐次保存の場合ファイルサイズは正してもＣＲＣに問題があるな可能性があるのでファイルが完全な状態かもチェックする
								if( node.IsCompleted == true )
								{
									// 実際にファイルが存在しているか確認する
									size = StorageAccessor_GetSize( path + assetBundlePath ) ;
									if( size >  0 && size == assetBundleInfo.Size )
									{
										// 実際にファイルが存在しサイズも問題ないのでこのファイルは更新しない
										assetBundleInfo.LastUpdateTime	= node.LastUpdateTime ;
										assetBundleInfo.Size			= size ;		// ＣＲＣファイルでのサイズと同じになるはず(ＣＲＣファイルにもサイズを持たせる事：サイズ部分にＣＲＣ値がずれて入り半日潰すバグを発生させた事があった事を忘れるな)
										assetBundleInfo.IsCompleted		= true ;		// 完全な状態のファイルである
										assetBundleInfo.UpdateRequired	= false ;		// 更新不要
									}
								}
							}
							else
							{
								// ＣＲＣかサイズが異なる(リモートの方が新しいファイルとなっている)
								modifiedCount ++ ;
							}
						}
					}
					else
					{
						// 新規に追加されたアセットバンドルがある
						modifiedCount ++ ;
					}
				}

				//---------------------------------------------------------

				// 次に参照が無くなったファイルを削除する

				int removeTargetFileCount = 0 ;	// 削除対象になるファイルの数(実際に削除されたかは見ない)
				int removedFileCount = 0 ;		// 実際に削除されたファイル

				foreach( var file in files )
				{
					bool exist = m_AssetBundleHash.ContainsKey( file.Path ) ;
					if
					(
						  exist == false ||
						( exist == true  && StreamingAssetsDirectAccessEnabled == true && m_AssetBundleHash[ file.Path ].LocationPriority == LocationPriorities.StreamingAssets )
					)
					{
						// ストレージ側のファイルの削除条件
						// ------------------------------------------------------
						// このファイルは参照が無くなる
						// または
						// StreamingAssets へのダイレクトアクセスが可能で且つ StreamingAssets の方が新しい

						if( StorageAccessor_Remove( path + file.Path ) == true )
						{
							removedFileCount ++ ;	// 実際に削除されたファイルをカウントする
						}

						// 削除対象となるファイルをカウントする
						removeTargetFileCount ++ ;
					}
				}

#if UNITY_EDITOR
				if( removedFileCount >  0 )
				{
					Debug.Log( "<color=#FFFF00>[AssetBundleManager] 不要になって削除されたファイル数:" + removedFileCount + "</color>" ) ;
				}
#endif
				//---------------------------------

				// ファイルが存在しなくなったフォルダも削除する(難読化が有効だとフォルダに分かれていない)
				if( instance.m_SecurityEnabled == false && removedFileCount >  0 )
				{
					StorageAccessor_RemoveAllEmptyFolders( path ) ;
				}

				//---------------------------------

				// 保存の必要がある
				Modified = ( modifiedCount >  0 ) ;

				// ローカルマニフェストを更新(保存)する必要があるかどうか
				bool isRequiredLocalManifestUpdating = ( modifiedCount >  0 ) || ( removeTargetFileCount >  0 ) ;

				return isRequiredLocalManifestUpdating ;
			}

			/// <summary>
			/// 最新のマニフェスト情報をローカルストレージに保存する(同期版)
			/// </summary>
			/// <returns>結果(true=成功・false=失敗</returns>
			public bool Save()
			{
				if( Completed == false )
				{
					// 保存する事はできない

					Debug.Log( "<color=#FFFF00>[Manifest Save] Failed : Could not complete - ManifestName = " + ManifestName + "</color>" ) ;
					return false ;
				}

				if( Modified == false )
				{
					// 変更がなければ保存はしない
					return true ;
				}

				//---------------------------------------------------------

				string path = StorageCacheRootPath + ManifestName + "/" + ManifestName + ".manifest" ;

				StoredManifest storedManifest = new StoredManifest() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					storedManifest.Files.Add( new AssetBundleFileInfo()
					{
						Path			= assetBundleInfo.Path,
						Hash			= assetBundleInfo.Hash,
						Size			= assetBundleInfo.Size,
						Crc				= assetBundleInfo.Crc,
						IsCompleted		= assetBundleInfo.IsCompleted,
						Tags			= assetBundleInfo.Tags,
						LastUpdateTime	= assetBundleInfo.LastUpdateTime
					} ) ;
				}

				// これが重い
				// 0.1秒ほどかかる事がある(メインスレッドをブロックする)
				string text = JsonUtility.ToJson( storedManifest ) ;

				bool result = StorageAccessor_SaveText( path, text, true ) ;

				if( result == true )
				{
					Modified = false ;
				}

				Debug.Log( "<color=#00FF00>[Manifest Save] ManifestName = " + ManifestName + "</color>" ) ;

				return result ;
			}

#if false
			/// <summary>
			/// 最新のマニフェスト情報をローカルストレージに保存する(非同期版)
			/// </summary>
			/// <returns>結果(true=成功・false=失敗</returns>
			public IEnumerator SaveAsync( AssetBundleManager instance, Action<bool> onResult = null )
			{
				float t ;

				//---------------------------------------------------------

				t = Time.realtimeSinceStartup ;

				string path = StorageCacheRootPath + ManifestName + "/" + ManifestName + ".manifest" ;

				StoredManifest storedManifest = new StoredManifest() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					storedManifest.Files.Add( new AssetBundleFileInfo()
					{
						Path			= assetBundleInfo.Path,
						Hash			= assetBundleInfo.Hash,
						Size			= assetBundleInfo.Size,
						Crc				= assetBundleInfo.Crc,
						IsCompleted		= assetBundleInfo.IsCompleted,
						Tags			= assetBundleInfo.Tags,
						LastUpdateTime	= assetBundleInfo.LastUpdateTime
					} ) ;
				}

				Debug.Log( "<color=#FFFF00>[AssetBundleManager] Manifest SAVE 1 < " + ManifestName + " > " + ( Time.realtimeSinceStartup - t ) + "</color>" ) ;

				yield return null ;

				//---------------------------------------------------------

				t = Time.realtimeSinceStartup ;

				// これが重い
				// 0.1秒ほどかかる事がある(メインスレッドをブロックする)
				string text = JsonUtility.ToJson( storedManifest ) ;

				Debug.Log( "<color=#FFFF00>[AssetBundleManager] Manifest SAVE 2 < " + ManifestName + " > " + ( Time.realtimeSinceStartup - t ) + "</color>" ) ;

				yield return null ;

				//---------------------------------------------------------

				t = Time.realtimeSinceStartup ;

				bool result = StorageAccessor_SaveText( path, text, true ) ;

				byte[] data = Encoding.UTF8.GetBytes( text ) ;

				// 非同期保存
				yield return instance.StartCoroutine
				(
					// 非同期でストレージに保存する
					StorageAccessor_SaveAsync
					(
						path,
						data,
						true,
						null,
						null,
						null,
						null,
						instance.m_WritingCancellationSource.Token
					)
				) ;

				Debug.Log( "<color=#FFFF00>[AssetBundleManager] Manifest SAVE 3 < " + ManifestName + " > " + ( Time.realtimeSinceStartup - t ) + "</color>" ) ;

				//---------------------------------------------------------

				onResult?.Invoke( result ) ;
			}
#endif

			//----------------------------------------------------------------------
			// StreamingAssets をサブマニフェストとする場合の情報の読み出しを行う

			/// <summary>
			/// マニフェストを展開する(非同期)
			/// </summary>
			/// <param name="onCompleted"></param>
			/// <param name="onError"></param>
			/// <param name="instance"></param>
			/// <returns></returns>
			internal protected IEnumerator LoadConstantAsync( Action onCompleted, Action<string> onError, AssetBundleManager instance )
			{
				//------------------------------------
	
				Progress = 0 ;
				Error = string.Empty ;

				//------------------------------------
	
				KeyValuePair<string,string>[] assetBundlePathAndHashs = null ;

				int i, l ;

				byte[] data = null ;

				string text ;

				//------------------------------------

				if( CrcOnly == false )
				{
					// StreamingAssets からロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( StreamingAssetsRootPath + ManifestName, ( _1, _2 ) => { data = _1 ; } ) ) ;

					//--------------------------------------------------------

					if( data == null || data.Length == 0 )
					{
						// データが取得出来ない
						Error = "Could not load data : " + ManifestName ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}

					//------------------------------------

					// バイナリからアセットバンドルを生成する
					AssetBundle assetBundle = AssetBundle.LoadFromMemory( data ) ;
					if( assetBundle == null )
					{
						// アセットバンドルが生成出来ない
						Error = "Could not create AssetBundle" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}

					m_Manifest = assetBundle.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" ) ;
					if( m_Manifest == null )
					{
						// マニフェストが取得出来ない
						assetBundle.Unload( true ) ;

						Error = "Could not get Manifest" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}

					string[] assetBundlePaths = m_Manifest.GetAllAssetBundles() ;
					if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						assetBundle.Unload( true ) ;

						Error = "No AssetBundles" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}
					
					// パスとハッシュを取得
					l = assetBundlePaths.Length ;
					assetBundlePathAndHashs = new KeyValuePair<string, string>[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						assetBundlePathAndHashs[ i ] = new KeyValuePair<string, string>( assetBundlePaths[ i ], m_Manifest.GetAssetBundleHash( assetBundlePaths[ i ] ).ToString() ) ;
					}

					assetBundle.Unload( false ) ;
				}

				//---------------------------------------------------------

				// ＣＲＣファイルのロードを試みる
				text = string.Empty ;
				data = null ;	// 初期化が必要

				//-------------

				Dictionary<string,AssetBundleAdditionalInfo> additionalInfoHash = null ;
				int size ;
				uint crc ;
				string[] tags ;

				//-----------------------------------------------------------------------------------------
				// Crc[Csv版]

				if( additionalInfoHash == null )
				{
					// StreamingAssets からロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssetsAsync( StreamingAssetsRootPath + ManifestName + ".csv", ( _1, _2 ) => { text = _1 ; } ) ) ;

					if( string.IsNullOrEmpty( text ) == false )
					{
						//--------------------------------------------------------

						additionalInfoHash = new Dictionary<string,AssetBundleAdditionalInfo>() ;

						// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する
						string[] line = text.Split( '\n' ) ;
						l = line.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( string.IsNullOrEmpty( line[ i ] ) == false )
							{
								string[] keyAndValue = line[ i ].Split( ',' ) ;
	
								if( keyAndValue.Length >  0  && string.IsNullOrEmpty( keyAndValue[ 0 ] ) == false )
								{
									size = 0 ;
									if( keyAndValue.Length >  1 && string.IsNullOrEmpty( keyAndValue[ 1 ] ) == false )
									{
										int.TryParse( keyAndValue[ 1 ], out size ) ;
									}
									crc = 0 ;
									if( keyAndValue.Length >  2 && string.IsNullOrEmpty( keyAndValue[ 2 ] ) == false )
									{
										uint.TryParse( keyAndValue[ 2 ], out crc ) ;
									}
									tags = null ;
									if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
									{
										tags = keyAndValue[ 3 ].Split( ' ' ) ;
									}

									additionalInfoHash.Add( keyAndValue[ 0 ].ToLower(), new AssetBundleAdditionalInfo( size, crc, tags ) ) ;
								}
							}
						}
					}
				}

				//-----------------------------------------------------------------------------------------
				// Crc[Json版]

				if( additionalInfoHash == null )
				{
					// StreamingAssets からロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssetsAsync( StreamingAssetsRootPath + ManifestName + ".json", ( _1, _2 ) => { text = _1 ; } ) ) ;

					if( string.IsNullOrEmpty( text ) == false )
					{
						//--------------------------------------------------------

						var json = JsonUtility.FromJson<JsonDeserializer>( text ) ;
						if( json != null )
						{
							additionalInfoHash = new Dictionary<string,AssetBundleAdditionalInfo>() ;

							foreach( var assetBundleFile in json.AssetBundleFiles )
							{
								additionalInfoHash.Add( assetBundleFile.Path.ToLower(), assetBundleFile ) ;
							}
						}
					}
				}

				//-------------

				// 一旦、パスとハッシュを突っ込む
				m_AssetBundleInfo_Constant.Clear() ;
				m_AssetBundleHash_Constant.Clear() ;

				ManifestInfo.AssetBundleInfo node ;

				if( assetBundlePathAndHashs != null )
				{
					// アセットバンドルファイルの情報を追加する
					foreach( var assetBundlePathAndHash in assetBundlePathAndHashs )
					{
						size	= 0 ;
						crc		= 0 ;
						tags	= null ;
						if( additionalInfoHash != null && additionalInfoHash.ContainsKey( assetBundlePathAndHash.Key ) == true )
						{
							size	= additionalInfoHash[ assetBundlePathAndHash.Key ].Size ;                                                                                                                                                                                                                  
							crc		= additionalInfoHash[ assetBundlePathAndHash.Key ].Crc ;
							tags	= additionalInfoHash[ assetBundlePathAndHash.Key ].Tags ;
						}

						node = new ManifestInfo.AssetBundleInfo( assetBundlePathAndHash.Key, assetBundlePathAndHash.Value, size, crc, tags, 0L ) ;
						m_AssetBundleInfo_Constant.Add( node ) ;
						if( m_AssetBundleHash_Constant.ContainsKey( assetBundlePathAndHash.Key ) == false )
						{
							m_AssetBundleHash_Constant.Add( assetBundlePathAndHash.Key, node ) ;
						}
					}
				}

				if( additionalInfoHash != null && additionalInfoHash.Count >  0 )
				{
					// 非アセットバンドル化ファイルの情報を追加する
					foreach( var additionalInfo in additionalInfoHash )
					{
						if( m_AssetBundleHash_Constant.ContainsKey( additionalInfo.Key ) == false )
						{
							// アセットバンドルとして追加されていないので非アセットバンドル化ファイルとみなす
							size	= additionalInfo.Value.Size ;                                                                                                                                                                                                                  
							crc		= additionalInfo.Value.Crc ;
							tags	= additionalInfo.Value.Tags ;

							node = new ManifestInfo.AssetBundleInfo( additionalInfo.Key, "", size, crc, tags, 0L ) ;	// アセットバンドルではないのでハッシュ値は存在しない
							m_AssetBundleInfo_Constant.Add( node ) ;
							if( m_AssetBundleHash_Constant.ContainsKey( additionalInfo.Key ) == false )
							{
								m_AssetBundleHash_Constant.Add( additionalInfo.Key, node ) ;
							}
						}
					}
				}
				else
				{
					Debug.LogWarning( "[CRC Not found] " + ManifestName + ".crc" ) ;
				}

				//-----------------------------------------------------------

				// 使用可能状態となった
				onCompleted?.Invoke() ;
			}
		}
	}
}
