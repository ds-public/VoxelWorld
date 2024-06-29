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

			//----------------------------------

			/// <summary>
			/// アセットバンドルを扱う準備が完了しているかどうかを示す
			/// </summary>
			public bool		Completed { get ; private set ; } = false ;	// 本来は　private にして、アクセサで readonly にすべきだが、Editor の作成を省略するため、あえて public にする。

			/// <summary>
			/// 状態に変化があったかどうか
			/// </summary>
			public bool		Modified { get ; private set ; } = false ;	// 本来は　private にして、アクセサで readonly にすべきだが、Editor の作成を省略するため、あえて public にする。

			//----------------------------------

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
			[SerializeField,Header( "【アセットバンドル情報】" )]
			private List<AssetBundleInfo> m_AssetBundleInfo ;	// ※readonly属性にするとインスペクターで表示できなくなるので付けてはだめ

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報の高速アクセス用のハッシュリスト
			/// </summary>
			private Dictionary<string,AssetBundleInfo> m_AssetBundleHash ;	// ショートカットアクセスのためディクショナリも用意する


			/// <summary>
			/// <summary>
			/// マニフェスト内の全アセットバンドル情報(固定)
			/// </summary>
			[SerializeField,Header( "【アセットバンドル情報(固定)】" )]
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
				/// <summary>
				/// キャッシュが属しているマニフェスト
				/// </summary>
				[NonSerialized]
				public	ManifestInfo	Manifest ;

				//---------------------------------

				/// <summary>
				/// パス(デバッグ表示用)
				/// </summary>
				public	string			Path ;

				/// <summary>
				/// アセットバンドル
				/// </summary>
				public	AssetBundle		AssetBundle ;

				/// <summary>
				/// フレームカウント(即時破棄はせず一定フレーム経過後に破棄される)
				/// </summary>
				public	int				FrameCount ;

				/// <summary>
				/// 依存関係にあるアセットバンドルのパス群
				/// </summary>
				public	string[]		DependentAssetBundlePaths ;

				//-------------

				/// <summary>
				/// 参照カウント
				/// </summary>
				public	int				CachingReferenceCount ;

				/// <summary>
				/// アセットバンドル保持の参照重複数(１以上で通常までの破棄は無視される)
				/// </summary>
				public	int				RetainReferenceCount ;

				//-----------------------------------------------------------------------------------------

				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="path"></param>
				/// <param name="assetBundle"></param>
				/// <param name="isCaching"></param>
				/// <param name="isRetain"></param>
				public AssetBundleCacheElement( ManifestInfo manifest, string path, AssetBundle assetBundle )
				{
					Manifest		= manifest ;

					Path			= path ;
					AssetBundle		= assetBundle ;
				}

				/// <summary>
				/// キャッシュ参照カウントを増加させる
				/// </summary>
				/// <param name="addtionalCount"></param>
				public void IncrementCachingReferenceCount( int count )
				{
					Manifest.IncrementCachingReferenceCount( this, count ) ;
				}

				/// <summary>
				/// キャッシュ参照カウントを減少させる
				/// </summary>
				/// <param name="addtionalCount"></param>
				public void DecrementCachingReferenceCount( int count, bool withAssets )
				{
					Manifest.DecrementCachingReferenceCount( this, count, withAssets ) ;
				}

				/// <summary>
				/// 維持カウントを増加させる
				/// </summary>
				public void IncrementRetainReferenceCount()
				{
					Manifest.IncrementRetainReferenceCount( this ) ;
				}
				/// <summary>
				/// 維持カウントを減少させる
				/// </summary>
				public void DecrementRetainReferenceCount( bool withAssets )
				{
					Manifest.DecrementRetainReferenceCount( this, withAssets ) ;
				}
			}

			/// <summary>
			/// アセットバンドルのキャッシュ
			/// </summary>
			private Dictionary<string,AssetBundleCacheElement>		m_AssetBundleCache ;

#if UNITY_EDITOR
			/// <summary>
			/// デバッグ用のキャッシュ中のアセットバンドルの表示リスト
			/// </summary>
			[SerializeField,Header( "【アセットバンドルキャッシュ情報】" )]
			private List<AssetBundleCacheElement>	m_AssetBundleCacheViewer = null ;
#endif

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// アセットバンドルのメモリキャッシュへの存在確認と同時にキャッシュするならば状態の更新
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="isCaching"></param>
			/// <returns></returns>
			internal protected AssetBundleCacheElement GetCachedAssetBundle
			(
				string assetBundlePath
			)
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == true )
				{
					// 既に登録済みなのでフレームカウントを更新して戻る(少なくともこのフレームで破棄されてはならない)
					m_AssetBundleCache[ assetBundlePath ].FrameCount = Time.frameCount ;

					// メモリキャッシュに存在する
					return m_AssetBundleCache[ assetBundlePath ] ;
				}

				// メモリキャッシュには存在しない
				return null ;
			}

			/// <summary>
			/// キャッシュにアセットバンドルを追加する
			/// </summary>
			/// <param name="name"></param>
			/// <param name="assetBundle"></param>
			internal protected AssetBundleCacheElement AddAssetBundleCache
			(
				string assetBundlePath, AssetBundle assetBundle
			)
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == true )
				{
					// 既に登録済みなのでフレームカウントを更新して戻る(少なくともこのフレームで破棄されてはならない)
					m_AssetBundleCache[ assetBundlePath ].FrameCount	= Time.frameCount ;

					return m_AssetBundleCache[ assetBundlePath ] ;
				}

				//---------------------------------

				// キャッシュを追加する(参照カウントで管理しない場合であっても同フレームの間はキャッシュに貯めておく)　※isCaching = true で参照カウントでの管理対象
				var element = new AssetBundleCacheElement( this, assetBundlePath, assetBundle ) ;
				m_AssetBundleCache.Add( assetBundlePath, element ) ;
#if UNITY_EDITOR
				m_AssetBundleCacheViewer.Add( element ) ;
#endif
				return element ;
			}

			/// <summary>
			/// キャッシュからアセットバンドルを強制的に削除する(内部処理)
			/// </summary>
			/// <param name="assetBundlePath"></param>
			internal protected bool RemoveAssetBundleCacheForced( string assetBundlePath, bool withAssets )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == false )
				{
					return false ;	// 元々キャッシュには存在しない
				}

				m_AssetBundleCache[ assetBundlePath ].AssetBundle.Unload( withAssets ) ;
				m_AssetBundleCache[ assetBundlePath ].AssetBundle = null ;

				// 以下順番に注意(先に本体を削ってしまうとデバッグ用の表示から削れなくなる=null例外)
#if UNITY_EDITOR
				m_AssetBundleCacheViewer.Remove( m_AssetBundleCache[ assetBundlePath ] ) ;
#endif
				m_AssetBundleCache.Remove( assetBundlePath ) ;

				return true ;
			}

			/// <summary>
			/// 通常のメモリ展開アセットバンドルの破棄で破棄されないようにするかどうかの設定を行う
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="isRetain"></param>
			/// <returns></returns>
			internal protected bool SetAssetBundleRetaining( string assetBundlePath, bool isRetain, bool withAssets )
			{
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// アセットバンドルが見つからない
					return false ;
				}

				//---------------------------------

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				if( assetBundleInfo.IsRetain == isRetain )
				{
					// 元々設定が同じなら何もしない
					return true ;
				}

				// 設定を更新する
				assetBundleInfo.IsRetain = isRetain ;

				//---------------------------------

				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == false )
				{
					return true ;	// 元々キャッシュには存在しない
				}

				//---------------------------------

				var assetBundleCache = m_AssetBundleCache[ assetBundlePath ] ;

				if( isRetain == true )
				{
					// 維持は有効(維持カウントを増加させる)
					assetBundleCache.IncrementRetainReferenceCount() ;
				}
				else
				{
					// 維持は無効(維持カウントを減少させる)
					assetBundleCache.DecrementRetainReferenceCount( withAssets ) ;
				}

				return true ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// アセットバンドルキャッシュをクリアする
			/// </summary>
			public void ClearAssetBundleCache( CacheReleaseTypes cacheReleaseType )
			{
				if( m_AssetBundleCache.Count == 0 )
				{
					return ;
				}

				//---------------------------------

				if( cacheReleaseType == CacheReleaseTypes.Limited )
				{
					// 非キャッシグ・非保持・異なるフレームカウントのものを破棄する
					var paths = new List<string>() ;

					foreach( var element in m_AssetBundleCache )
					{
						if( element.Value.CachingReferenceCount == 0 && element.Value.RetainReferenceCount == 0 && element.Value.FrameCount != Time.frameCount )
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
							m_AssetBundleCacheViewer.Remove( m_AssetBundleCache[ path ] ) ;
#endif
							m_AssetBundleCache.Remove( path ) ;
						}
					}
				}
				else
				if( cacheReleaseType == CacheReleaseTypes.Standard )
				{
					// 非保持のものを破棄する
					var paths = new List<string>() ;

					foreach( var element in m_AssetBundleCache )
					{
						if( element.Value.RetainReferenceCount == 0 )
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
							m_AssetBundleCacheViewer.Remove( m_AssetBundleCache[ path ] ) ;
#endif
							m_AssetBundleCache.Remove( path ) ;
						}
					}
				}
				else
				if( cacheReleaseType == CacheReleaseTypes.Perfect )
				{
					// 強制的に全てのアセットバンドルを破棄する
					var paths = new List<string>() ;

					foreach( var element in m_AssetBundleCache )
					{
						element.Value.AssetBundle.Unload( true ) ;
						element.Value.AssetBundle = null ;

						paths.Add( element.Key ) ;
					}
#if UNITY_EDITOR
					m_AssetBundleCacheViewer.Clear() ;
#endif
					m_AssetBundleCache.Clear() ;	
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
				public List<AssetBundleFileInfo>	Files = new () ;
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
					m_AssetBundleInfo = new () ;
				}
				else
				{
					m_AssetBundleInfo.Clear() ;
				}

				if( m_AssetBundleHash == null )
				{
					m_AssetBundleHash = new () ;
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
					m_AssetBundleCache			= new () ;
#if UNITY_EDITOR
					m_AssetBundleCacheViewer	= new () ;
#endif
				}
				else
				{
					m_AssetBundleCache.Clear() ;
#if UNITY_EDITOR
					m_AssetBundleCacheViewer.Clear() ;
#endif
				}

				//-----------------------------------------------------------------------------------------
				// StreamingAssets に固定アセットバンドルを格納しているかどうか

				// StorageAndStreamingAssets から Storage または StreamingAssets に変わった場合も初期化が必要

				if( m_AssetBundleInfo_Constant == null )
				{
					m_AssetBundleInfo_Constant = new () ;
				}
				else
				{
					m_AssetBundleInfo_Constant.Clear() ;
				}

				if( m_AssetBundleHash_Constant == null )
				{
					m_AssetBundleHash_Constant = new () ;
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
				if( IsStreamingAssetsOnly == false )	// 対象が StreamingAssets 且つ DirectAccesssEnabled が有効の場合のみ(DirectAccessEnabled が無効の場合は、コピーがデータフォルダに存在するため、再コピーする必要がある)
				{
					foreach( var assetBundleInfo in m_AssetBundleInfo )
					{
						if( assetBundleInfo.LocationPriority == LocationPriorities.Storage )	// 実際のロード対象がデータフォルダになっている(実際のロード対象が StreamingAssets になっている場合、最新は StreamingAssets 側であるため、データフォルダにコピーは存在しない)
						{
							assetBundleInfo.UpdateRequired = true ;	// 更新が必要扱いにする
							Modified = true ;
						}
					}
				}
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
					string storagePath = $"{StorageCacheRootPath}{ManifestName}/" ;
				
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
						storagePath = $"{StorageCacheRootPath}{ManifestName}/{assetBundleInfo.Path}" ;

						if( StorageAccessor_Exists( storagePath ) == StorageAccessor.TargetTypes.File )
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
				public string	Hash ;
				public uint		Crc ;
				public string[]	Tags ;

				public AssetBundleAdditionalInfo( int size, string hash, uint crc, string[] tags )
				{
					Size	= size ;
					Hash	= hash ;
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
	
				string[] assetBundlePaths = null ;
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
					// Manifest ファイルはダウンロード・ストレージに保存した上でバイナリデータを取得する
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
					// 重要：リモートから取得した最新のマニフェスト情報

					// バイナリからアセットバンドルを生成する
					var assetBundle = AssetBundle.LoadFromMemory( manifestData ) ;
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

					// パスのみを取得する
					assetBundlePaths = m_Manifest.GetAllAssetBundles() ;
					if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						assetBundle.Unload( true ) ;

						Error = "No AssetBundles" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}
					
					// パスとハッシュを取得する
					l = assetBundlePaths.Length ;
					assetBundlePathAndHashs = new KeyValuePair<string, string>[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						assetBundlePathAndHashs[ i ] = new ( assetBundlePaths[ i ], m_Manifest.GetAssetBundleHash( assetBundlePaths[ i ] ).ToString() ) ;
					}

					// アセットバンドル破棄(重要)
					assetBundle.Unload( false ) ;
				}

				//---------------------------------------------------------
				// CRCを展開する

				string assetBundlePath ;

				int size ;
				string hash ;
				uint crc ;
				string[] tags ;

				Dictionary<string,AssetBundleAdditionalInfo> additionalInfoHash = null ;

				//-----------------------------------------------------------------------------------------
				// Crc[Csv版]

				if( string.IsNullOrEmpty( crcCsvText ) == false )
				{
					// CRC[CSV版]ファイルを展開する

					//--------------------------------------------------------
#if UNITY_EDITOR
					// 確認用にＣＲＣ[CSV版]ファイルを保存する(ファイルが存在しなくても動作上の支障は無い)
					string path = $"{StorageCacheRootPath}{ManifestName}/" ;
					if( StorageAccessor_SaveText( $"{path}{ManifestName}.csv", crcCsvText, makeFolder:true ) == true )
					{
						Debug.Log( "[AssetBundleManager] Save CRC[CSV] File : " + ManifestName + "\n -> "+ path + ManifestName + ".csv" ) ;
					}
					else
					{
						Debug.LogWarning( "[AssetBundleManager] Save CRC[CSV] File : " + ManifestName + "\n -> "+ path + ManifestName + ".csv" + " is failed." ) ;
					}
#endif
					//--------------------------------------------------------

					additionalInfoHash = new () ;

					// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する

					crcCsvText = crcCsvText.Replace( "\n", "\x0A" ) ;
					crcCsvText = crcCsvText.Replace( "\x0D\x0A", "\x0A" ) ;

					var lines = crcCsvText.Split( '\x0A' ) ;
					l = lines.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( lines[ i ] ) == false )
						{
							var keyAndValue = lines[ i ].Split( ',' ) ;
	
							if( keyAndValue.Length >  0  && string.IsNullOrEmpty( keyAndValue[ 0 ] ) == false )
							{
								// フォーマットが古いものか新しいものか判定する

								assetBundlePath = keyAndValue[ 0 ].ToLower() ;

								size	= 0 ;
								hash	= string.Empty ;
								crc		= 0 ;
								tags	= null ;

								if( keyAndValue.Length >  1 && string.IsNullOrEmpty( keyAndValue[ 1 ] ) == false )
								{
									int.TryParse( keyAndValue[ 1 ], out size ) ;
								}
								if( keyAndValue.Length >  2 && string.IsNullOrEmpty( keyAndValue[ 2 ] ) == false )
								{
									if( keyAndValue[ 2 ].Length >= 16 )
									{
										// 新バージョンのフォーマット

										hash = keyAndValue[ 2 ] ;

										if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
										{
											uint.TryParse( keyAndValue[ 3 ], out crc ) ;
										}
										if( keyAndValue.Length >  4 && string.IsNullOrEmpty( keyAndValue[ 4 ] ) == false )
										{
											tags = keyAndValue[ 4 ].Split( ' ' ) ;
										}
									}
									else
									{
										// 古バージョンのフォーマット

										uint.TryParse( keyAndValue[ 2 ], out crc ) ;

										if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
										{
											tags = keyAndValue[ 3 ].Split( ' ' ) ;
										}
									}
								}

								additionalInfoHash.Add( assetBundlePath, new ( size, hash, crc, tags ) ) ;
							}
						}
					}
				}

				//-----------------------------------------------------------------------------------------
				// Crc[Json版]

				if( string.IsNullOrEmpty( crcJsonText ) == false )
				{
					// CRC[JSON版]ファイルを展開する

					// Jenkins でビルドした際に妙な改行が入っている事があるため検査して不備があれば修正する
					crcJsonText = CorrectCrcJsonText( crcJsonText ) ;

					//--------------------------------------------------------
#if UNITY_EDITOR
					// 確認用にＣＲＣ[JSON版]ファイルを保存する(ファイルが存在しなくても動作上の支障は無い)
					string path = $"{StorageCacheRootPath}{ManifestName}/" ;
					if( StorageAccessor_SaveText( $"{path}{ManifestName}.json", crcJsonText, makeFolder:true ) == true )
					{
						Debug.Log( "[UnityEditorOnly : AssetBundleManager] Save CRC[JSON] File : " + ManifestName + "\n -> "+ path + ManifestName + ".json" ) ;
					}
					else
					{
						Debug.LogWarning( "[UnityEditorOnly : AssetBundleManager] Save CRC[JSON] File : " + ManifestName + "\n -> "+ path + ManifestName + ".json" + " is failed." ) ;
					}
#endif
					//--------------------------------------------------------

					Debug.Log( crcJsonText ) ;
					var json = JsonUtility.FromJson<JsonDeserializer>( crcJsonText ) ;
					if( json != null )
					{
						additionalInfoHash = new () ;

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

				// Manifest : 実際に有効なアセットバンドルに対して .crc または .json の情報を設定する
				if( assetBundlePathAndHashs != null )
				{
					// アセットバンドルファイルの情報を追加する
					foreach( var assetBundlePathAndHash in assetBundlePathAndHashs )
					{
						assetBundlePath = assetBundlePathAndHash.Key ;

						size	= 0 ;
						hash	= assetBundlePathAndHash.Value ;
						crc		= 0 ;
						tags	= null ;

						if( additionalInfoHash != null && additionalInfoHash.ContainsKey( assetBundlePath ) == true )
						{
							size	= additionalInfoHash[ assetBundlePath ].Size ;

							if( string.IsNullOrEmpty( additionalInfoHash[ assetBundlePath ].Hash ) == false )
							{
								// .crc .json ファイル側のハッシュで上書きする
								hash	= additionalInfoHash[ assetBundlePath ].Hash ;
							}

							crc		= additionalInfoHash[ assetBundlePath ].Crc ;
							tags	= additionalInfoHash[ assetBundlePath ].Tags ;
						}

						node = new ( assetBundlePath, size, hash, crc, tags, 0L ) ;
						m_AssetBundleInfo.Add( node ) ;
						if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
						{
							m_AssetBundleHash.Add( assetBundlePath, node ) ;
						}
					}
				}

				// 非アセットバンドル化ファイルの情報を追加する
				if( additionalInfoHash != null && additionalInfoHash.Count >  0 )
				{
					foreach( var additionalInfo in additionalInfoHash )
					{
						if( m_AssetBundleHash.ContainsKey( additionalInfo.Key ) == false )
						{
							// アセットバンドルとして追加されていないので非アセットバンドル化ファイルとみなす
							size	= additionalInfo.Value.Size ;                                                                                                                                                                                                                  
							hash	= additionalInfo.Value.Hash ;
							crc		= additionalInfo.Value.Crc ;
							tags	= additionalInfo.Value.Tags ;

							node = new ( additionalInfo.Key, size, hash, crc, tags, 0L ) ;	// アセットバンドルではないのでハッシュ値は存在しない
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

									if( StorageAccesor_ExistsInStreamingAssets( $"{StreamingAssetsRootPath}/{assetBundleInfo.Path}" ) == true )
									{
										// StreamingAssets にファイルが存在している
										// 重要：非アセットバンドルのケースもあるのでＣＲＣで確認する
										if
										(
											assetBundleInfo.Size == assetBundleInfo_Constant.Size &&
											assetBundleInfo.Crc  == assetBundleInfo_Constant.Crc
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
						assetBundleInfo.UpdateRequired   = false ;
						assetBundleInfo.LocationPriority = LocationPriorities.StreamingAssets ;	// 処理上は意味は無いが Inspector で見た時に勘違いするので優先を StreamingAssets にしておく
					}
				}

				//------------------------------------------------------------
				
				m_Busy = false ;	// 処理終了

				//-----------------------------------------------------------

				onCompleted?.Invoke() ;
			}

			// Jsonテキストを検査しておかしな箇所があれば修正した状態のものを返す
			private string CorrectCrcJsonText( string crcJsonText )
			{
				if( string.IsNullOrEmpty( crcJsonText ) == true )
				{
					// 処理出来ない
					return crcJsonText ;
				}

				//---------------------------------------------------------

				var newLines = new List<string>() ;

				crcJsonText = crcJsonText.Replace( "\n", "\x0A" ) ;
				crcJsonText = crcJsonText.Replace( "\x0D\x0A", "\x0A" ) ;

				var oldLines = crcJsonText.Split( '\x0A' ) ;
				int i, l = oldLines.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 念のため改行的なものとカンマを完全に削除しておく
					oldLines[ i ] = oldLines[ i ].Trim( ',' ) ;

					if( string.IsNullOrEmpty( oldLines[ i ] ) == false )
					{
						// 文字列が存在する
						newLines.Add( oldLines[ i ] ) ;
					}
				}

				if( newLines.Count >= 3 )
				{
					// 最低でも３行は存在するはず

					var sb = new StringBuilder() ;

					// 最初の行
					sb.Append( newLines[ 0 ] ) ;
					sb.Append( "\n" ) ;

					l = newLines.Count - 1 ;

					for( i  = 1 ; i <  ( l - 1 ) ; i ++ )
					{
						sb.Append( newLines[ i ] ) ;
						sb.Append( ",\n" ) ;
					}

					sb.Append( newLines[ l - 1 ] ) ;
					sb.Append( "\n" ) ;				// 最後の項目はカンマ無し

					// 最後の行
					sb.Append( newLines[ l ] ) ;

					crcJsonText = sb.ToString() ;
				}

//				Debug.Log( "<color=#FF00FF>-------- CrcJsonText 結果</color>\n" + crcJsonText ) ;

				return crcJsonText ;
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
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( $"{StreamingAssetsRootPath}{ManifestName}", ( _1, _2 ) => { data = _1 ; } ) ) ;
				}

				// Remote
				if( data == null && string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false && string.IsNullOrEmpty( LocalAssetBundleRootPath ) == true && LocationType != LocationTypes.StreamingAssets )
				{
					string path = $"{RemoteAssetBundleRootPath}{ManifestName}?time={GetClientTime()}" ;

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
					data = File_Load( $"{LocalAssetBundleRootPath}{ManifestName}" ) ;
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
					string path = $"{RemoteAssetBundleRootPath}{ManifestName}.{extension}?time={GetClientTime()}" ;

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
					text = File_LoadText( $"{LocalAssetBundleRootPath}{ManifestName}.{extension}" ) ;
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

				string path = $"{StorageCacheRootPath}{ManifestName}/" ;

				string fullPath = $"{path}{ManifestName}.manifest" ;

				if( StorageAccessor_Exists( fullPath ) != StorageAccessor.TargetTypes.File )
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
				var files = storedManifest.Files ;
				var hash = new Dictionary<string, AssetBundleFileInfo>() ;

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
							if( assetBundleInfo.Size == node.Size && assetBundleInfo.Hash == node.Hash )
							{
								// サイズとハッシュは同じである

								// 逐次保存の場合ファイルサイズは正してもＣＲＣに問題があるな可能性があるのでファイルが完全な状態かもチェックする
								if( node.IsCompleted == true )
								{
									// 実際にファイルが存在しているか確認する
									size = StorageAccessor_GetSize( $"{path}{assetBundlePath}" ) ;
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
							// 非アセットバンドルはハッシュ値が存在しない場合があるためＣＲＣ値を使用する
							if( assetBundleInfo.Size == node.Size && assetBundleInfo.Crc == node.Crc )
							{
								// ＣＲＣとサイズは同じである

								// 逐次保存の場合ファイルサイズは正してもＣＲＣに問題があるな可能性があるのでファイルが完全な状態かもチェックする
								if( node.IsCompleted == true )
								{
									// 実際にファイルが存在しているか確認する
									size = StorageAccessor_GetSize( $"{path}{assetBundlePath}" ) ;
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

						if( StorageAccessor_Remove( $"{path}{file.Path}" ) == true )
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

				string path = $"{StorageCacheRootPath}{ManifestName}/{ManifestName}.manifest" ;

				var storedManifest = new StoredManifest() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					storedManifest.Files.Add( new ()
					{
						Path			= assetBundleInfo.Path,
						Size			= assetBundleInfo.Size,
						Hash			= assetBundleInfo.Hash,
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
	
				string[] assetBundlePaths = null ;
				KeyValuePair<string,string>[] assetBundlePathAndHashs = null ;

				int i, l ;

//				bool manifestDone = false ;
				byte[] manifestData = null ;

//				bool crcCsvDone = false ;
				string crcCsvText = null ;

//				bool crcJsonDone = false ;
				string crcJsonText = null ;

				//------------------------------------

				if( CrcOnly == false )
				{
					// StreamingAssets からロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( $"{StreamingAssetsRootPath}{ManifestName}", ( _1, _2 ) => { manifestData = _1 ; } ) ) ;
//					manifestDone = true ;
				}
//				else
//				{
//					manifestDone = true ;
//				}

				// Crc[Csv版] ファイルをダウンロードする
//				yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssetsAsync( $"{StreamingAssetsRootPath}{ManifestName}.csv", ( _1, _2 ) => { crcCsvText = _1 ; } ) ) ;
//				crcCsvDone = ( string.IsNullOrEmpty( crcCsvText ) == false ) ; ;

				// Crc[Json版] ファイルをダウンロードする
				yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssetsAsync( $"{StreamingAssetsRootPath}{ManifestName}.json", ( _1, _2 ) => { crcJsonText = _1 ; } ) ) ;
//				crcJsonDone = ( string.IsNullOrEmpty( crcJsonText ) == false ) ;

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
					// 重要：リモートから取得した最新のマニフェスト情報

					// バイナリからアセットバンドルを生成する
					var assetBundle = AssetBundle.LoadFromMemory( manifestData ) ;
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

					// アセットバンドルパスを取得する
					assetBundlePaths = m_Manifest.GetAllAssetBundles() ;
					if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						assetBundle.Unload( true ) ;

						Error = "No AssetBundles" ;
						onError?.Invoke( Error ) ;
						m_Busy = false ;
						yield break ;
					}
					
					// パスとハッシュを取得する
					l = assetBundlePaths.Length ;
					assetBundlePathAndHashs = new KeyValuePair<string, string>[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						assetBundlePathAndHashs[ i ] = new ( assetBundlePaths[ i ], m_Manifest.GetAssetBundleHash( assetBundlePaths[ i ] ).ToString() ) ;
					}

					assetBundle.Unload( false ) ;
				}

				//---------------------------------------------------------
				// CRCを展開する

				string assetBundlePath ;

				int			size ;
				string		hash ;
				uint		crc ;
				string[]	tags ;

				Dictionary<string,AssetBundleAdditionalInfo> additionalInfoHash = null ;

				//-----------------------------------------------------------------------------------------
				// Crc[Csv版]

				if( string.IsNullOrEmpty( crcCsvText ) == false )
				{
					// CRC[CSV版]ファイルを展開する

					//--------------------------------------------------------

					additionalInfoHash = new () ;

					// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する

					crcCsvText = crcCsvText.Replace( "\n", "\x0A" ) ;
					crcCsvText = crcCsvText.Replace( "\x0D\x0A", "\x0A" ) ;

					var lines = crcCsvText.Split( '\x0A' ) ;
					l = lines.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( lines[ i ] ) == false )
						{
							var keyAndValue = lines[ i ].Split( ',' ) ;
	
							if( keyAndValue.Length >  0  && string.IsNullOrEmpty( keyAndValue[ 0 ] ) == false )
							{
								// フォーマットが古いものか新しいものか判定する

								assetBundlePath = keyAndValue[ 0 ].ToLower() ;

								size	= 0 ;
								hash	= string.Empty ;
								crc		= 0 ;
								tags	= null ;

								if( keyAndValue.Length >  1 && string.IsNullOrEmpty( keyAndValue[ 1 ] ) == false )
								{
									int.TryParse( keyAndValue[ 1 ], out size ) ;
								}
								if( keyAndValue.Length >  2 && string.IsNullOrEmpty( keyAndValue[ 2 ] ) == false )
								{
									if( keyAndValue[ 2 ].Length >= 16 )
									{
										// 新バージョンのフォーマット

										hash = keyAndValue[ 2 ] ;

										if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
										{
											uint.TryParse( keyAndValue[ 3 ], out crc ) ;
										}
										if( keyAndValue.Length >  4 && string.IsNullOrEmpty( keyAndValue[ 4 ] ) == false )
										{
											tags = keyAndValue[ 4 ].Split( ' ' ) ;
										}
									}
									else
									{
										// 古バージョンのフォーマット

										uint.TryParse( keyAndValue[ 2 ], out crc ) ;

										if( keyAndValue.Length >  3 && string.IsNullOrEmpty( keyAndValue[ 3 ] ) == false )
										{
											tags = keyAndValue[ 3 ].Split( ' ' ) ;
										}
									}
								}

								additionalInfoHash.Add( assetBundlePath, new ( size, hash, crc, tags ) ) ;
							}
						}
					}
				}

				//-----------------------------------------------------------------------------------------
				// Crc[Json版]

				if( string.IsNullOrEmpty( crcJsonText ) == false )
				{
					// CRC[JSON版]ファイルを展開する

					// Jenkins でビルドした際に妙な改行が入っている事があるため検査して不備があれば修正する
					crcJsonText = CorrectCrcJsonText( crcJsonText ) ;

					//--------------------------------------------------------

					var json = JsonUtility.FromJson<JsonDeserializer>( crcJsonText ) ;
					if( json != null )
					{
						additionalInfoHash = new () ;

						foreach( var assetBundleFile in json.AssetBundleFiles )
						{
							additionalInfoHash.Add( assetBundleFile.Path.ToLower(), assetBundleFile ) ;
						}
					}
				}

				//-------------

				// 一旦、パスとハッシュを突っ込む
				m_AssetBundleInfo_Constant.Clear() ;
				m_AssetBundleHash_Constant.Clear() ;

				ManifestInfo.AssetBundleInfo node ;

				// Manifest : 実際に有効なアセットバンドルに対して .crc または .json の情報を設定する
				if( assetBundlePathAndHashs != null )
				{
					// アセットバンドルファイルの情報を追加する
					foreach( var assetBundlePathAndHash in assetBundlePathAndHashs )
					{
						assetBundlePath = assetBundlePathAndHash.Key ;

						size	= 0 ;
						hash	=  assetBundlePathAndHash.Value ;
						crc		= 0 ;
						tags	= null ;

						if( additionalInfoHash != null && additionalInfoHash.ContainsKey( assetBundlePath ) == true )
						{
							size	= additionalInfoHash[ assetBundlePath ].Size ;

							if( string.IsNullOrEmpty( additionalInfoHash[ assetBundlePath ].Hash ) == false )
							{
								// .crc .json ファイル側のハッシュで上書きする
								hash	= additionalInfoHash[ assetBundlePath ].Hash ;
							}

							crc		= additionalInfoHash[ assetBundlePath ].Crc ;
							tags	= additionalInfoHash[ assetBundlePath ].Tags ;
						}

						node = new ( assetBundlePath, size, hash, crc, tags, 0L ) ;
						m_AssetBundleInfo_Constant.Add( node ) ;
						if( m_AssetBundleHash_Constant.ContainsKey( assetBundlePath ) == false )
						{
							m_AssetBundleHash_Constant.Add( assetBundlePath, node ) ;
						}
					}
				}

				// 非アセットバンドル化ファイルの情報を追加する
				if( additionalInfoHash != null && additionalInfoHash.Count >  0 )
				{
					foreach( var additionalInfo in additionalInfoHash )
					{
						if( m_AssetBundleHash_Constant.ContainsKey( additionalInfo.Key ) == false )
						{
							// アセットバンドルとして追加されていないので非アセットバンドル化ファイルとみなす
							size	= additionalInfo.Value.Size ;                                                                                                                                                                                                                  
							hash	= additionalInfo.Value.Hash ;	// 存在しない
							crc		= additionalInfo.Value.Crc ;
							tags	= additionalInfo.Value.Tags ;

							node = new ( additionalInfo.Key, size, hash, crc, tags, 0L ) ;	// アセットバンドルではないのでハッシュ値は存在しない
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
