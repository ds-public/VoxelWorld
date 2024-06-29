using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;


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
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		public partial class ManifestInfo
		{
			// ManifestInfo :: CorrectPath

			/// <summary>
			/// パスに // が無く、正しい AssetBundlePath が分からない場合に検査する
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="assetPath"></param>
			/// <returns></returns>
			internal protected bool CorrectPath( ref string assetBundlePath, ref string assetPath, out bool isSingle )
			{
				// 単体ファイルではない
				isSingle = false ;

				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == true )
				{
					// AssetBundlePath は正しいものになっている
					return true ;
				}

				//---------------------------------

				string originalAssetBundlePath = assetBundlePath ;

				// AssetBundlePath が間違っている可能性があるため、子階層を削り、正しい AssetBundlePath 及び AssetPath を走査する

				int i ;

				assetPath ??= string.Empty ;

				while( true )
				{
					i = assetBundlePath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						if( assetPath.Length == 0 )
						{
							assetPath = assetBundlePath[ ( i + 1 ).. ] ;
						}
						else
						{
							assetPath = $"{assetBundlePath[ ( i + 1 ).. ]}/{assetPath}" ;
						}

						assetBundlePath = assetBundlePath[ ..i ] ;

						if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == true )
						{
							// アセットバンドルパスの正しいものを発見した
							return true ;
						}
					}
					else
					{
						break ;
					}
				}

				//---------------------------------------------------------
				// １アセットバンドルファイル＝１アセットファイルのケース

				assetBundlePath = originalAssetBundlePath ;
				int p0 = assetBundlePath.LastIndexOf( '/' ) ;
				int p1 = assetBundlePath.LastIndexOf( '.' ) ;
				if( p1 >= 0 && p1 >  p0 )
				{
					// 拡張子あり(最後の拡張子を使用する)
					if( p0 >= 0 )
					{
						p0 ++ ;
						assetPath = assetBundlePath[ p0.. ] ;
					}
					else
					{
						assetPath = assetBundlePath ;
					}

					assetBundlePath = assetBundlePath[ ..p1 ] ;

					if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == true )
					{
						// アセットバンドルパスの正しいものを発見した

						// 単体ファイルである
						isSingle = true ;

						return true ;
					}
				}

				//---------------------------------------------------------

				// パスの指定が間違っている
				return false ;
			}


			// ManifestInfo :: AllAssetPaths
						
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットバンドルに含まれる全てのアセットのパスとクラス型</returns>
			internal protected ( string, Type )[] GetAllAssetPaths
			(
				string assetBundlePath,
				AssetBundleManager instance
			)
			{
				// 既にキャッシュされている場合はそれを利用するためキャッシュは有効にしてリリースメソッドを実行する

				( AssetBundle assetBundle, ManifestInfo.AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 参照カウントを増加させる
				assetBundleCache.IncrementCachingReferenceCount( 1 ) ;

				// アセットのロード
				var allAssetPaths = m_AssetBundleHash[ assetBundlePath ].GetAllAssetPaths( assetBundle, assetBundlePath, LocalAssetsRootPath, instance ) ;

				// 参照カウントを減少させる
				assetBundleCache.DecrementCachingReferenceCount( 1, true ) ;

				return allAssetPaths ;
			}
			
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator GetAllAssetPaths_Coroutine
			(
				string assetBundlePath,
				Action<( string, Type )[]> onLoaded, Action<string> onError,
				Request request,
				AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath,( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 参照カウントを増加させる
				assetBundleCache.IncrementCachingReferenceCount( 1 ) ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 実際のパスを取得する
				var allAssetPaths = assetBundleInfo.GetAllAssetPaths( assetBundle, assetBundlePath, LocalAssetsRootPath, instance ) ;
				
				// 参照カウントを減少させる
				assetBundleCache.DecrementCachingReferenceCount( 1, true ) ;

				if( allAssetPaths != null )
				{
					onLoaded?.Invoke( allAssetPaths ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//----------------------------------

			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットバンドルに含まれる全てのアセットのパスとクラス型</returns>
			internal protected string[] GetAllAssetPaths
			(
				string assetBundlePath, Type type,
				AssetBundleManager instance
			)
			{
				( AssetBundle assetBundle, ManifestInfo.AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 参照カウントを増加させる
				assetBundleCache.IncrementCachingReferenceCount( 1 ) ;

				// アセットのロード
				var allAssetPaths = m_AssetBundleHash[ assetBundlePath ].GetAllAssetPaths( assetBundle, assetBundlePath, type, LocalAssetsRootPath, instance ) ;

				// 参照カウントを減少させる
				assetBundleCache.DecrementCachingReferenceCount( 1, true ) ;

				return allAssetPaths ;
			}
			
			/// <summary>
			/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator GetAllAssetPaths_Coroutine
			(
				string assetBundlePath, Type type,
				Action<string[]> onLoaded, Action<string> onError,
				Request request,
				AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, ( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 参照カウントを増加させる
				assetBundleCache.IncrementCachingReferenceCount( 1 ) ;

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 実際のパスを取得する
				var allAssetPaths = assetBundleInfo.GetAllAssetPaths( assetBundle, assetBundlePath, type, LocalAssetsRootPath, instance ) ;
				
				// 参照カウントを減少させる
				assetBundleCache.DecrementCachingReferenceCount( 1, true ) ;

				if( allAssetPaths != null )
				{
					onLoaded?.Invoke( allAssetPaths ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//------------------------------------------------------------------------------------------

			// ManifestInfo :: Asset

			/// <summary>
			/// アセットを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="assetName"></param>
			/// <param name="type"></param>
			/// <param name="isCaching"></param>
			/// <param name="isRetain"></param>
			/// <param name="instance"></param>
			/// <returns></returns>
			internal protected ( UnityEngine.Object, AssetBundleCacheElement ) LoadAsset
			(
				string assetBundlePath, string assetName, Type type, bool isSingle,
				AssetBundleManager instance
			)
			{
				( AssetBundle assetBundle, AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return ( null, null ) ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				var asset = m_AssetBundleHash[ assetBundlePath ].LoadAsset
				(
					assetBundle,
					assetBundlePath, assetName, type, isSingle,
					LocalAssetsRootPath,
					instance
				) ;

				return ( asset, assetBundleCache ) ;
			}
			
			/// <summary>
			/// アセットを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadAsset_Coroutine
			(
				string assetBundlePath, string assetName, Type type, bool isSingle,
				Action<UnityEngine.Object, AssetBundleCacheElement> onLoaded, Action<string> onError,
				Request request,
				AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, ( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期(現状使用できない)
//					Debug.LogWarning( "非同期" ) ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAssetHolder, tRequest, tInstance ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadAsset( assetBundle, assetBundlePath, assetName, type, isSingle, LocalAssetsRootPath, instance ) ;
//				}
				
				if( asset != null )
				{
					onLoaded?.Invoke( asset, assetBundleCache ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllAssets

			/// <summary>
			/// 全てのアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>全てのアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected ( UnityEngine.Object[], AssetBundleCacheElement ) LoadAllAssets
			(
				string assetBundlePath, Type type,
				string localAssetPath, AssetBundleManager instance
			)
			{
				( AssetBundle assetBundle, AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return ( null, null ) ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				var assets = m_AssetBundleHash[ assetBundlePath ].LoadAllAssets( assetBundle, type, LocalAssetsRootPath, localAssetPath, instance ) ;

				// 参照カウントをアセット分増やす(依存アセットも含む)

				return ( assets, assetBundleCache ) ;
			}

			/// <summary>
			/// 全てのアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="type">アセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAllAssets_Coroutine
			(
				string assetBundlePath, Type type,
				Action<UnityEngine.Object[], AssetBundleCacheElement> onLoaded, Action<string> onError,
				Request request,
				string localAssetPath,
				AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, ( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssetsHolder, tRequest, tInstance, tResourcePath ) ) ;
//					allSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllAssets( assetBundle, type, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets, assetBundleCache ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: SubAsset

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="subAssetName">サブアセット名</param>
			/// <param name="type">サブアセットのクラス型</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected ( UnityEngine.Object, AssetBundleCacheElement ) LoadSubAsset
			(
				string assetBundlePath, string assetName, string subAssetName, Type type, bool isSingle,
				string localAssetPath, AssetBundleManager instance
			)
			{
				( AssetBundle assetBundle, AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return ( null, null ) ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットのロード
				var asset = m_AssetBundleHash[ assetBundlePath ].LoadSubAsset( assetBundle, assetBundlePath, assetName, subAssetName, type, isSingle, LocalAssetsRootPath, localAssetPath, instance ) ;

				// 参照カウントはここで処理する

				return ( asset, assetBundleCache ) ;
			}

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="subAssetName">サブアセット名</param>
			/// <param name="type">サブアセットのクラス型</param>
			/// <param name="onLoaded">ロード成功時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保持するかどうか</param>
			/// <param name="onError">ロード失敗時のコールバック</param>
			/// <param name="request">非同期待ちオブジェクト</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadSubAsset_Coroutine
			(
				string assetBundlePath, string assetName, string subAssetName, Type type, bool isSingle,
				Action<UnityEngine.Object, AssetBundleCacheElement> onLoaded, Action<string> onError,
				Request request,
				string localAssetPath, AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, ( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					yield return instance.StartCoroutine( assetBundleInfo.LoadSubAsset_Coroutine( assetBundle, assetBundleName, assetName, subAssetName, type, rSubAssetHolder, request, instance, resourcePath ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadSubAsset( assetBundle, assetBundlePath, assetName, subAssetName, type, isSingle, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( asset != null )
				{
					onLoaded?.Invoke( asset, assetBundleCache ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllSubAssets

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath"></param>
			/// <param name="assetName"></param>
			/// <param name="type"></param>
			/// <param name="isCaching"></param>
			/// <param name="isRetain"></param>
			/// <param name="localAssetPath"></param>
			/// <param name="instance"></param>
			/// <returns></returns>
			internal protected ( UnityEngine.Object[], AssetBundleCacheElement ) LoadAllSubAssets
			(
				string assetBundlePath, string assetName, Type type, bool isSingle,
				string localAssetPath, AssetBundleManager instance
			)
			{
				// アセットバンドルをロードする
				( AssetBundle assetBundle, AssetBundleCacheElement assetBundleCache ) =
					LoadAssetBundle( assetBundlePath, instance ) ;
				if( assetBundle == null )
				{
					return ( null, null ) ;	// 失敗
				}

				//---------------------------------------------------------

				// アセットをロードする
				var assets = m_AssetBundleHash[ assetBundlePath ].LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, isSingle, LocalAssetsRootPath, localAssetPath, instance ) ;

				return ( assets, assetBundleCache ) ;
			}

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetName">アセット名</param>
			/// <param name="type">アセットのクラスタイプ</param>
			/// <param name="onLoaded">読み込み完了時のコールバック</param>
			/// <param name="keep">アセットバンドルを永続保存するか</param>
			/// <param name="onError">読み込み失敗時のコールバック</param>
			/// <param name="request">非同期待ち制御</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <param name="instance">保持インスタンス</param>
			/// <param name="resourcePath">リソースパス</param>
			/// <returns></returns>
			internal protected IEnumerator LoadAllSubAssets_Coroutine
			(
				string assetBundlePath, string assetName, Type type, bool isSingle,
				Action<UnityEngine.Object[], AssetBundleCacheElement> onLoaded, Action<string> onError,
				Request request,
				string localAssetPath, AssetBundleManager instance
			)
			{
				AssetBundle				assetBundle			= null ;
				AssetBundleCacheElement	assetBundleCache	= null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundlePath, ( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, onError, request, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return instance.StartCoroutine( assetBundleInfo.LoadAllSubAssets_Coroutine( assetBundle, assetBundleName, assetName, type, rAllSubAssetsHolder, tRequest, instance, resourcePath ) ) ;
//					allSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, isSingle, LocalAssetsRootPath, localAssetPath, instance ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets, assetBundleCache ) ;
				}
				else
				{
					onError?.Invoke( "Could not load ." ) ;
				}
			}

			//--------------------------------------

			// ManifestInfo :: AssetBundle

			/// <summary>
			/// アセットバンドルを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <param name="assetBundleCaching">アセットバンドルのキャッシングタイプ</param>
			/// <returns></returns>
			internal protected ( AssetBundle, AssetBundleCacheElement ) LoadAssetBundle
			(
				string assetBundlePath,
				AssetBundleManager instance
			)
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return ( null, null ) ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新対象になっているのでメモリキャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath, false ) ;

					// 更新対象になっているので取得不可
					return ( null, null ) ;
				}

				//-------------------------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する
				string[] dependentAssetBundlePaths = null ;

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						//-------------------------------------------------------

						AssetBundleInfo			dependentAssetBundleInfo ;
						AssetBundle				dependentAssetBundle ;
						AssetBundleCacheElement	dependentAssetBundleCache ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
#if UNITY_EDITOR
								Debug.LogWarning( "同期:依存するアセットバンドルが取得できない(要Update):" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
								// 更新対象であるため取得不可
								RemoveAssetBundleCacheForced( dependentAssetBundlePath, false ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return ( null, null ) ;
							}
						}

						//-------------------------------
						// ここに来るという事は依存するアセットバンドルが全て取得可能であるという事を示す

						// 依存するアセットバンドルでキャッシュにためていないものはためていく
						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
//							Debug.LogWarning( "同期:依存するアセットバンドル名:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;

							// キャッシュに存在するか確認する
							dependentAssetBundleCache = GetCachedAssetBundle( dependentAssetBundlePath ) ;
							if( dependentAssetBundleCache == null )
							{
								// キャッシュに存在しない
								dependentAssetBundle = CreateAssetBundle( dependentAssetBundleInfo, instance ) ;
								if( dependentAssetBundle != null )
								{
									// キャッシュにためる
									dependentAssetBundleCache = AddAssetBundleCache( dependentAssetBundlePath, dependentAssetBundle ) ;
								}
								else
								{
#if UNITY_EDITOR
									Debug.LogWarning( "同期:依存するアセットバンドルが取得できない(IOエラー):" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
									// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
									return ( null, null ) ;
								}
							}
						}
					}
				}

				//-------------------------------------------------------------
				
				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				AssetBundle				assetBundle ;
				AssetBundleCacheElement	assetBundleCache ;

				( assetBundleCache ) = GetCachedAssetBundle( assetBundlePath ) ;
				if( assetBundleCache == null )
				{
					// キャッシュには存在しない

					// 保存されたアセットバンドルを同期で展開する
					assetBundle = CreateAssetBundle( assetBundleInfo, instance ) ;

					// キャッシュにためる(参照カウントで管理しない場合であっても同フレーム中はキャッシュに貯めておく必要がある)
					assetBundleCache = AddAssetBundleCache( assetBundlePath, assetBundle ) ;
				}

				// 依存関係にあるアセットバンドルのパスを設定する　※先に依存関係でロードされている場合は依存アセットバンドルの依存アセットバンドルが設定されていないのでここで改めて設定する必要がある
				// ※あらゆる状況で必ずキャッシュインスタンスは生成される
				assetBundleCache.DependentAssetBundlePaths = dependentAssetBundlePaths ;

				//---------------------------------------------------------

				if( assetBundleInfo.IsRetain == true && assetBundleCache.RetainReferenceCount == 0 )
				{
					// 維持指定があり且つ維持状態になっていないメモリ展開アセットバンドルの維持カウントを増加させる
					assetBundleCache.IncrementRetainReferenceCount() ;
				}

				//---------------------------------------------------------

				// アセットバンドルのインスタンスを返す(予測不能のエラーとしてnullが返る可能性も0ではない)
				return ( assetBundleCache.AssetBundle, assetBundleCache ) ;
			}

			/// <summary>
			/// アセットバンドルを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="onLoaded">アセットバンドルのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAssetBundle_Coroutine
			(
				string assetBundlePath,
				Action<AssetBundle, AssetBundleCacheElement> onLoaded, Action<string> onResult,
				Request request,
				AssetBundleManager instance
			)
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックをかける
				if( assetBundleInfo.Busy == true )
				{
					yield return new WaitWhile( () => assetBundleInfo.Busy == true ) ;
				}

				// 排他ロック有効
				assetBundleInfo.Busy = true ;

				//------------------------------------------

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// メモリキャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath, false ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
					(
						assetBundleInfo, LocationType, null, onResult,
						true,
						request,
						instance
					) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// 失敗

						// 排他ロック無効
						assetBundleInfo.Busy = false ;

						yield break ;
					}
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				string[] dependentAssetBundlePaths = null ;

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						//-------------------------------------------------------

						AssetBundleInfo			dependentAssetBundleInfo ;
						AssetBundle				dependentAssetBundle ;
						AssetBundleCacheElement dependentAssetBundleCache ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
//							Debug.LogWarning( "非同期:依存するアセットバンドル名:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;

							//------------------------------

							// 非同期で同じアセットバンドルにアクセスする場合は排他ロックがかかる
							if( dependentAssetBundleInfo.Busy == true )
							{
								yield return new WaitWhile( () => dependentAssetBundleInfo.Busy == true ) ;
							}
							
							//------------------------------

							// 排他ロック有効
							dependentAssetBundleInfo.Busy = true ;

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// メモリキャッシュから削除する
								RemoveAssetBundleCacheForced( dependentAssetBundlePath, false ) ;

								// 更新対象になっているのでダウンロードを試みる
								yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
								(
									dependentAssetBundleInfo, LocationType,
									null, onResult,
									true,
									request,
									instance
								) ) ;
							}

							if( dependentAssetBundleInfo.UpdateRequired == false )
							{
								// 依存アセットバンドルのダウンロードに成功
								( dependentAssetBundleCache ) = GetCachedAssetBundle( dependentAssetBundlePath ) ;
								if( dependentAssetBundleCache == null )
								{
									// キャッシュには存在しないのでロードする
//									if( instance.m_FastLoadEnabled == false || FastLoadEnabled == false )
//									{
//										// 非同期(低速)
//										yield return instance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( dependentAssetBundleLocalPath, ( _ ) => { dependentAssetBundle = _ ; } ) ) ;
//									}
//									else
//									{
										// 同期(高速)
										dependentAssetBundle = CreateAssetBundle( dependentAssetBundleInfo, instance ) ;
//									}

									if( dependentAssetBundle != null )
									{
										// キャッシュにためる
										dependentAssetBundleCache = AddAssetBundleCache( dependentAssetBundlePath, dependentAssetBundle ) ;
									}
									else
									{
#if UNITY_EDITOR
										Debug.LogWarning( "非同期:依存するアセットバンドルが取得できない(IOエラー):" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
									}
								}
							}

							// 排他ロック解除
							dependentAssetBundleInfo.Busy = false ;

							//------------------------------

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
#if UNITY_EDITOR
								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードに失敗した:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;
#endif
								// 依存アセットバンドルのダウンロードに失敗
								onResult?.Invoke( "Could not dependent load." ) ;

								// 排他ロック無効
								assetBundleInfo.Busy = false ;

								yield break ;
							}
						}
					}
				}

				//----------------------------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する

				AssetBundle				assetBundle ;
				AssetBundleCacheElement assetBundleCache ;

				( assetBundleCache ) = GetCachedAssetBundle( assetBundlePath ) ;
				if( assetBundleCache == null )
				{
					// キャッシュには存在しない

					// StreamingAssets へののダイレクトアクセスの場合

//					if( instance.m_FastLoadEnabled == false || FastLoadEnabled == false )
//					{
//						// 非同期(低速)
//						yield return instance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( assetBundleLocalPath, ( _ ) => { assetBundle = _ ; } ) ) ;
//					}
//					else
//					{
						// 同期(高速)
						assetBundle = CreateAssetBundle( assetBundleInfo, instance ) ;
//					}

					// キャッシュにためる
					assetBundleCache = AddAssetBundleCache( assetBundlePath, assetBundle ) ;
				}

				// 依存関係にあるアセットバンドルのパスを設定する　※先に依存関係でロードされている場合は依存アセットバンドルの依存アセットバンドルが設定されていないのでここで改めて設定する必要がある
				// ※あらゆる状況で必ずキャッシュインスタンスは生成される
				assetBundleCache.DependentAssetBundlePaths = dependentAssetBundlePaths ;

				//---------------------------------------------------------

				if( assetBundleInfo.IsRetain == true && assetBundleCache.RetainReferenceCount == 0 )
				{
					// 維持指定があり且つ維持状態になっていないメモリ展開アセットバンドルの維持カウントを増加させる
					assetBundleCache.IncrementRetainReferenceCount() ;
				}

				//---------------------------------------------------------

				// アセットバンドルのインスタンスを返す(予測不能のエラーとしてnullが返る可能性も0ではない)
				onLoaded?.Invoke( assetBundleCache.AssetBundle, assetBundleCache ) ;
				onResult?.Invoke( string.Empty ) ;

				//---------------------------------------------------------

				// 排他ロック解除
				assetBundleInfo.Busy = false ;
			}

			// アセットバンドルのインスタンスを生成する
			private AssetBundle CreateAssetBundle( AssetBundleInfo assetBundleInfo, AssetBundleManager instance )
			{
				AssetBundle assetBundle ;

				if( IsStreamingAssetsOnly == true || ( StreamingAssetsDirectAccessEnabled == true && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets ) )
				{
					// StreamingAssets にダイレクトアクセス
					assetBundle = StorageAccessor_LoadAssetBundleFromStreamingAssets( $"{StreamingAssetsRootPath}/{assetBundleInfo.Path}" ) ;
				}
				else
				{
					// Storage にアクセス
					assetBundle = StorageAccessor_LoadAssetBundle( $"{StorageCacheRootPath}{ManifestName}/{assetBundleInfo.Path}" ) ;
#if UNITY_EDITOR
					if( instance.m_IsRecording == true )
					{
						// 使用するアセットバンドルの情報を記録中
						instance.RecordUsingAssetBundle_Private( ManifestName, assetBundleInfo.Path, assetBundleInfo.Size ) ;
					}
#endif
				}

				return assetBundle ;
			}

			/// <summary>
			/// アセットバンドルを解放する(参照カウントが０になった際にのみ実際に解放される)
			/// </summary>
			/// <param name="assetBundle"></param>
			/// <returns></returns>
			internal protected int ReleaseAssetBundle( AssetBundle assetBundle, bool withAssets )
			{
				var assetBundleCache = m_AssetBundleCache.Values.FirstOrDefault( _ => _.AssetBundle == assetBundle ) ;
				if( assetBundleCache == null )
				{
					// 該当無し
					return -1 ;
				}

				return ReleaseAssetBundle( assetBundleCache.Path, withAssets ) ;
			}

			/// <summary>
			/// アセットバンドルを解放する(参照カウントが０になった際にのみ実際に解放される)
			/// </summary>
			/// <param name="assetBundle"></param>
			/// <returns></returns>
			internal protected int ReleaseAssetBundle( string assetBundlePath, bool withAssets )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundlePath ) == false )
				{
					// 元々ロードされていない
					return -1 ;
				}

				var assetBundleCache = m_AssetBundleCache[ assetBundlePath ] ;

				return DecrementCachingReferenceCount( assetBundleCache, 1, withAssets ) ;
			}

			/// <summary>
			/// 参照カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			internal protected int IncrementCachingReferenceCount( AssetBundleCacheElement assetBundleCache, int count )
			{
				return IncrementCachingReferenceCount_Private( assetBundleCache, count, new () ) ;
			}

			/// <summary>
			/// 参照カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			private int IncrementCachingReferenceCount_Private( AssetBundleCacheElement assetBundleCache, int count, List<AssetBundleCacheElement> assetBundleCacheMarks )
			{
				if( assetBundleCacheMarks.Contains( assetBundleCache ) == true )
				{
					Debug.LogWarning( "[AssetBundleManager] 循環参照が発生したため処理を中断します" ) ;
					return -1 ;
				}

				//---------------------------------

				// 参照カウントを増加させる
				assetBundleCache.CachingReferenceCount += count ;

				// 処理済みにマークする
				assetBundleCacheMarks.Add( assetBundleCache ) ;

				//---------------------------------

				if( assetBundleCache.DependentAssetBundlePaths != null && assetBundleCache.DependentAssetBundlePaths.Length >  0 )
				{
					// 依存関係にあるアセットバンドルが存在する
					foreach( var dependentAssetBundlePath in assetBundleCache.DependentAssetBundlePaths )
					{
						if( m_AssetBundleCache.ContainsKey( dependentAssetBundlePath ) == true )
						{
							// 再帰的に依存関係にあるアセットバンドルのアセット参照カウントを増加させる
							IncrementCachingReferenceCount_Private( m_AssetBundleCache[ dependentAssetBundlePath ], count, assetBundleCacheMarks ) ;
						}
					}
				}

				// 変化後の参照カウント
				return assetBundleCache.CachingReferenceCount ;
			}

			/// <summary>
			/// 参照カウントを減少させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			internal protected int DecrementCachingReferenceCount( AssetBundleCacheElement assetBundleCache, int count, bool withAssets )
			{
				return DecrementCachingReferenceCount_Private( assetBundleCache, count, withAssets, new () ) ;
			}

			/// <summary>
			/// 参照カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			private int DecrementCachingReferenceCount_Private( AssetBundleCacheElement assetBundleCache, int count, bool withAssets, List<AssetBundleCacheElement> assetBundleCacheMarks )
			{
				if( assetBundleCacheMarks.Contains( assetBundleCache ) == true )
				{
					Debug.LogWarning( "[AssetBundleManager] 循環参照が発生したため処理を中断します" ) ;
					return -1 ;
				}

				//---------------------------------

				if( assetBundleCache.CachingReferenceCount >  0 )
				{
					// 参照カウントを減少させる
					assetBundleCache.CachingReferenceCount -= count ;

					if( assetBundleCache.CachingReferenceCount <= 0 && assetBundleCache.RetainReferenceCount <= 0 )
					{
						// 破棄対象となった
						if( assetBundleCache.AssetBundle != null )
						{
//							Debug.Log( "<color=#FF7F00>アセットバンドルを破棄する " + assetBundleCache.Path + " : " + withAssets + "</color>" ) ;
							assetBundleCache.AssetBundle.Unload( withAssets ) ;
							assetBundleCache.AssetBundle = null ;
						}
						else
						{
							Debug.LogWarning( "異常" ) ;
						}

						m_AssetBundleCache.Remove( assetBundleCache.Path ) ;
#if UNITY_EDITOR
						m_AssetBundleCacheViewer.Remove( assetBundleCache ) ;
#endif
					}
				}
				else
				{
					Debug.LogWarning( "異常" ) ;
				}

				// 処理済みにマークする
				assetBundleCacheMarks.Add( assetBundleCache ) ;

				//---------------------------------

				if( assetBundleCache.DependentAssetBundlePaths != null && assetBundleCache.DependentAssetBundlePaths.Length >  0 )
				{
					// 依存関係にあるアセットバンドルが存在する
					foreach( var dependentAssetBundlePath in assetBundleCache.DependentAssetBundlePaths )
					{
						if( m_AssetBundleCache.ContainsKey( dependentAssetBundlePath ) == true )
						{
							// 再帰的に依存関係にあるアセットバンドルのアセット参照カウントを増加させる
							DecrementCachingReferenceCount_Private( m_AssetBundleCache[ dependentAssetBundlePath ], count, withAssets, assetBundleCacheMarks ) ;
						}
					}
				}

				// 変化後の参照カウント
				return assetBundleCache.CachingReferenceCount ;
			}

			//--------------

			/// <summary>
			/// 維持カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			internal protected int IncrementRetainReferenceCount( AssetBundleCacheElement assetBundleCache )
			{
				return IncrementRetainReferenceCount_Private( assetBundleCache, new () ) ;
			}

			/// <summary>
			/// 維持カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			private int IncrementRetainReferenceCount_Private( AssetBundleCacheElement assetBundleCache, List<AssetBundleCacheElement> assetBundleCacheMarks )
			{
				if( assetBundleCacheMarks.Contains( assetBundleCache ) == true )
				{
					Debug.LogWarning( "[AssetBundleManager] 循環参照が発生したため処理を中断します" ) ;
					return -1 ;
				}

				//---------------------------------

				// 参照カウントを増加させる
				assetBundleCache.RetainReferenceCount ++ ;

				// 処理済みにマークする
				assetBundleCacheMarks.Add( assetBundleCache ) ;

				//---------------------------------

				if( assetBundleCache.DependentAssetBundlePaths != null && assetBundleCache.DependentAssetBundlePaths.Length >  0 )
				{
					// 依存関係にあるアセットバンドルが存在する
					foreach( var dependentAssetBundlePath in assetBundleCache.DependentAssetBundlePaths )
					{
						if( m_AssetBundleCache.ContainsKey( dependentAssetBundlePath ) == true )
						{
							// 再帰的に依存関係にあるアセットバンドルのアセット参照カウントを増加させる
							IncrementRetainReferenceCount_Private( m_AssetBundleCache[ dependentAssetBundlePath ], assetBundleCacheMarks ) ;
						}
					}
				}

				// 変化後の維持カウント
				return assetBundleCache.RetainReferenceCount ;
			}

			/// <summary>
			/// 維持カウントを減少させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			internal protected int DecrementRetainReferenceCount( AssetBundleCacheElement assetBundleCache, bool withAssets )
			{
				return DecrementRetainReferenceCount_Private( assetBundleCache, withAssets, new () ) ;
			}

			/// <summary>
			/// 維持カウントを増加させる　※循環参照を遮断する処理が無いと永久ループからのフリーズが発生するので何か対策を考える
			/// </summary>
			/// <param name="element"></param>
			private int DecrementRetainReferenceCount_Private( AssetBundleCacheElement assetBundleCache, bool withAssets, List<AssetBundleCacheElement> assetBundleCacheMarks )
			{
				if( assetBundleCacheMarks.Contains( assetBundleCache ) == true )
				{
					Debug.LogWarning( "[AssetBundleManager] 循環参照が発生したため処理を中断します" ) ;
					return -1 ;
				}

				//---------------------------------

				if( assetBundleCache.RetainReferenceCount >  0 )
				{
					// 参照カウントを減少させる
					assetBundleCache.RetainReferenceCount -- ;

					if( assetBundleCache.RetainReferenceCount <= 0 )
					{
						// 破棄対象となった
						if( assetBundleCache.AssetBundle != null )
						{
//							Debug.Log( "<color=#FF7F00>アセットバンドルを破棄する " + assetBundleCache.Path + " : " + withAssets + "</color>" ) ;

							assetBundleCache.AssetBundle.Unload( withAssets ) ;
							assetBundleCache.AssetBundle = null ;
						}
						else
						{
							Debug.LogWarning( "異常" ) ;
						}

						m_AssetBundleCache.Remove( assetBundleCache.Path ) ;
#if UNITY_EDITOR
						m_AssetBundleCacheViewer.Remove( assetBundleCache ) ;
#endif
					}
				}
				else
				{
					Debug.LogWarning( "異常" ) ;
				}

				// 処理済みにマークする
				assetBundleCacheMarks.Add( assetBundleCache ) ;

				//---------------------------------

				if( assetBundleCache.DependentAssetBundlePaths != null && assetBundleCache.DependentAssetBundlePaths.Length >  0 )
				{
					// 依存関係にあるアセットバンドルが存在する
					foreach( var dependentAssetBundlePath in assetBundleCache.DependentAssetBundlePaths )
					{
						if( m_AssetBundleCache.ContainsKey( dependentAssetBundlePath ) == true )
						{
							// 再帰的に依存関係にあるアセットバンドルのアセット参照カウントを増加させる
							DecrementRetainReferenceCount_Private( m_AssetBundleCache[ dependentAssetBundlePath ], withAssets, assetBundleCacheMarks ) ;
						}
					}
				}

				// 変化後の参照カウント
				return assetBundleCache.RetainReferenceCount ;
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// アセットバンドルのＵＲＩを取得する
			/// </summary>
			internal protected string GetUri( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					Debug.LogWarning( "Not found Path = " + assetBundlePath ) ;
					return null ;
				}

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				return GetSelectedUri( assetBundleInfo, LocationType ) ;
			}

			/// <summary>
			/// アセットバンドルのダウンロードを行う　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundle_Coroutine( string assetBundlePath, bool isKeep, Action<long,float,float> onProgress, Action<long,string> onResult, bool isManifestSaving, Request request, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					onResult?.Invoke( 0, "Could not load." ) ;
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;
				assetBundleInfo.IsKeep = isKeep ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックをかける
				if( assetBundleInfo.Busy == true )
				{
					yield return new WaitWhile( () => assetBundleInfo.Busy == true ) ;
				}

				// 排他ロック有効
				assetBundleInfo.Busy = true ;

				//------------------------------------------

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// メモリキャッシュから破棄する
					RemoveAssetBundleCacheForced( assetBundlePath, false ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
					(
						assetBundleInfo,
						LocationType,
						( float downloadingProgress, float writingProgress ) =>
						{
							onProgress?.Invoke( assetBundleInfo.Size, downloadingProgress, writingProgress ) ;
						},
						( string error ) =>
						{
							// 何らかのエラーが発生している
							// assetBundleInfo.UpdateRequired が true のままなので無視しても良い
						},
						isManifestSaving,
						request,
						instance
					) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// 失敗
						onResult?.Invoke( assetBundleInfo.Size, "Can not load " + assetBundlePath ) ;

						// 排他ロック無効
						assetBundleInfo.Busy = false ;

						yield break ;
					}
				}

				// 成功
				onResult?.Invoke( assetBundleInfo.Size, string.Empty ) ;

				//---------------------------------------------------------

				// 排他ロック解除
				assetBundleInfo.Busy = false ;
			}
#if false
			/// <summary>
			/// タグで指定されたアセットバンドルのダウンロードを行う　※直接呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundlesWithTags_Coroutine( string[] tags, bool keep, Action<int,int> onProgress, Request request, AssetBundleManager instance )
			{
				string[] assetBundlePaths =	GetAllAssetBundlePathsWithTags( tags, true, true ) ;

				if( assetBundlePaths == null || assetBundlePaths.Length == 0 )
				{
					// ダウンロード対象となるファイルが見つからない
					request.Error = "Target not found." ;
					yield break ;
				}

				//---------------------------------------------------------
				
				// 全体サイズを計算する
				int entireDataSize = 0 ;
				foreach( var assetBundlePath in assetBundlePaths )
				{
					AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;
					entireDataSize += assetBundleInfo.Size ;
				}
				request.EntireDataSize = entireDataSize ;
				request.EntireFileCount = assetBundlePaths.Length ;

				// ダウンロードを行う
				int storedDataSize = 0 ;
				foreach( var assetBundlePath in assetBundlePaths )
				{
					AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

					// 念のためキャッシュから削除する
					RemoveAssetBundleCacheForced( assetBundlePath ) ;

					// 更新対象になっているのでダウンロードを試みる
					Request subRequest = new Request( instance ) ;

					storedDataSize = request.StoredDataSize ;

					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine
					(
						assetBundleInfo,
						keep,
						( float progress ) =>
						{
							request.StoredDataSize = storedDataSize + subRequest.StoredDataSize ;
							request.Progress = ( float )request.StoredDataSize / ( float )request.EntireDataSize ;

							onProgress?.Invoke( request.StoredFileCount, request.StoredDataSize ) ;
						},
						( string error ) =>
						{
							if( string.IsNullOrEmpty( error ) == true )
							{
								request.StoredFileCount ++ ;

								onProgress?.Invoke( request.StoredFileCount, request.StoredDataSize ) ;
							}
						},
						subRequest,
						instance
					) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// ダウンロード失敗
						request.Error = "Could not load " + assetBundlePath ;
						yield break ;
					}
				}

				// 成功
				request.IsDone = true ;
			}
#endif
			//------------------------------------------------------------------------------------------

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// アセットバンドルをキャッシュから削除する　※呼び出し非推奨
			/// </summary>
			/// <param name="assetBundleName">アセットバンドル名</param>
			/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected bool DeleteAssetBundleFromStorageCache( string assetBundlePath, AssetBundleManager instance )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// ベースパス
				string path = $"{StorageCacheRootPath}{ManifestName}/" ;
				
				// 削除した
				StorageAccessor_Remove( $"{path}{assetBundleInfo.Path}" ) ;

				// ファイルが存在しなくなったフォルダも削除する
				if( instance.m_SecurityEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( path ) ;
				}

				// 更新必要フラグをオンにする
				assetBundleInfo.UpdateRequired = true ;

				return true ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットバンドルの保有を確認する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Contains( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return true ;
			}

			/// <summary>
			/// アセットバンドルの存在を確認する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Exists( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					Debug.LogWarning( "Unknown AssetBundle Path = " + assetBundlePath ) ;
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新対象になっているので取得不可
					return false ;
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					var dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;
					if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
					{
						// 依存するものが存在する
#if UNITY_EDITOR
						if( dependentAssetBundlePaths.Length >= TOO_MUCH_DEPENDENCE )
						{
							// 明らかに過量な一定数の依存ファイルが存在する場合に警告を出す
							string wm = "[AssetBundleManager] Too much dependence : " + assetBundlePath + " ( " + dependentAssetBundlePaths.Length + " )\n" ;
							int wi ;
							for( wi  = 0 ; wi <  dependentAssetBundlePaths.Length ; wi ++ )
							{
								wm += " + " + dependentAssetBundlePaths[ wi ] + "\n" ;
							}
							Debug.LogWarning( wm ) ;
						}
#endif
						AssetBundleInfo	dependentAssetBundleInfo ;

						foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
						{
							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCacheForced( dependentAssetBundlePath, false ) ;

								Debug.LogWarning( "----- ※存在:依存するアセットバンドルに更新が必要なものがある:" + dependentAssetBundlePath + " <- " + assetBundlePath ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return false ;
							}
						}
					}
				}

				// 依存も含めて全て問題の無い状態になっている
				return true ;
			}
			
			/// <summary>
			/// アセットバンドルのサイズを取得する
			/// </summary>
			/// <param name="assetBundlePath">アセットバンドルのパス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected long GetSize( string assetBundlePath )
			{
				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return 0 ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return assetBundleInfo.Size ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="updateRequiredOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePaths( bool updateRequiredOnly = true )
			{
				var paths = new List<string>() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( updateRequiredOnly == false || ( updateRequiredOnly == true && assetBundleInfo.UpdateRequired == true ) )
					{
						paths.Add( assetBundleInfo.Path ) ;
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				return paths.ToArray() ;
			}

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="updateRequiredOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePathsWithTags( string[] tags, bool updateRequiredOnly = true, bool isDependency = true )
			{
				var paths = new List<string>() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( updateRequiredOnly == false || ( updateRequiredOnly == true && assetBundleInfo.UpdateRequired == true ) )
					{
						foreach( var tag in tags )
						{
							if( assetBundleInfo.ContainsTag( tag ) == true )
							{
								paths.Add( assetBundleInfo.Path ) ;
							}
						}
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				if( isDependency == true )
				{
					// 依存関係にあるものも含めて取得する
					var temporaryPaths = new List<string>() ;
					foreach( var path in paths )
					{
						temporaryPaths.Add( path ) ;	// まずは自身を追加する

						var dependentAssetBundlePaths = GetAllDependentAssetBundlePaths( path, updateRequiredOnly ) ;
						if( dependentAssetBundlePaths != null && dependentAssetBundlePaths.Length >  0 )
						{
							foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
							{
								if( temporaryPaths.Contains( dependentAssetBundlePath ) == false )
								{
									temporaryPaths.Add( dependentAssetBundlePath ) ;	// 未追加の場合のみ追加する
								}
							}
						}
					}
					paths = temporaryPaths ;
				}

				return paths.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 依存関係にあるアセットバンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllDependentAssetBundlePaths( string assetBundlePath, bool updateRequiredOnly = true )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				var dependentAssetBundlePaths = m_Manifest.GetAllDependencies( assetBundlePath ) ;

				if( dependentAssetBundlePaths == null || dependentAssetBundlePaths.Length == 0 )
				{
					return null ;	// 依存するアセットバンドル存在しない
				}

				if( updateRequiredOnly == false )
				{
					// そのまま返す
					return dependentAssetBundlePaths ;
				}

				//---------------------------------------------------------

				// 更新が必要なもののみ返す

				var paths = new List<string>() ;

				AssetBundleInfo	dependentAssetBundleInfo ;

				foreach( var dependentAssetBundlePath in dependentAssetBundlePaths )
				{
					dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundlePath ] ;
					if( dependentAssetBundleInfo.UpdateRequired == true )
					{
						// 更新対象限定
						paths.Add( dependentAssetBundlePath ) ;
					}
				}

				if( paths.Count == 0 )
				{
					return null ;
				}

				return paths.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 非アセットバンドルの直接パスを取得する
			/// </summary>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string GetAssetFilePath( string assetBundlePath, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				// そのファイルが更新対象か確認する
				// 存在を確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// そのような名前のアセットバンドルは存在しない
					return null ;
				}

				var assetBundleInfo = m_AssetBundleHash[ assetBundlePath ] ;

				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新が必要なファイルなので存在しないものとみなす
					return null ;
				}

				if( IsStreamingAssetsOnly == true || ( StreamingAssetsDirectAccessEnabled == true && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets ) )
				{
					// StreamingAssets が対象

					// 環境パスを取得する
					return StorageAccessor_GetPathFromStreamingAssets( $"{StreamingAssetsRootPath}/{assetBundleInfo.Path}" ) ;
				}
				else
				{
					// Storage が対象
#if UNITY_EDITOR
					if( string.IsNullOrEmpty( assetBundleInfo.Hash ) == true )
					{
						// ネイティブパスを参照した時点で必要なものとみなす
						if( instance.m_IsRecording == true )
						{
							instance.RecordUsingAssetBundle_Private( ManifestName, assetBundlePath, assetBundleInfo.Size ) ;
						}
					}
#endif
					// 環境パスを取得する
					return StorageAccessor_GetPath( $"{StorageCacheRootPath}{ManifestName}/{assetBundleInfo.Path}" ) ;
				}
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 指定のアセットバンドルのキャッシュ内での動作を設定する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <returns>結果(true=成功・失敗)</returns>
			public bool SetKeepFlag( string assetBundlePath, bool isKeep )
			{
				// 全て小文字化
				assetBundlePath = assetBundlePath.ToLower() ;

				// 存在を確認する
				if( m_AssetBundleHash.ContainsKey( assetBundlePath ) == false )
				{
					// そのような名前のアセットバンドルは存在しない
					return false ;
				}

				m_AssetBundleHash[ assetBundlePath ].IsKeep = isKeep ;

				return true ;
			}

			/// <summary>
			/// 破棄可能なアセットバンドルをタイムスタンプの古い順に破棄してキャッシュの空き容量を確保する
			/// </summary>
			/// <param name="size">必要なキャッシュサイズ</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			private bool Cleanup( long requireSize )
			{
				// キープ対象全てとキープ非対象でタイムスタンプの新しい順にサイズを足していきキャッシュの容量をオーバーするまで検査する
				var freeAssetBundleInfo = new List<AssetBundleInfo>() ;

				long freeCacheSize = CacheSize ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( assetBundleInfo.UpdateRequired == false )
					{
						// 更新の必要の無い最新の状態のアセットバンドル
						if( assetBundleInfo.IsKeep == true )
						{
							// 常時保持する必要のあるアセットパンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								freeCacheSize -= assetBundleInfo.Size ;
//							}
						}
						else
						{
							// 空き容量が足りなくなったら破棄してもよいアセットバンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								// 破棄可能なアセットバンドルの情報を追加する
								freeAssetBundleInfo.Add( assetBundleInfo ) ;
//							}
						}
					}
					else
					if( assetBundleInfo.IsDownloading == true )
					{
						// 並列ダウンロード中
						freeCacheSize -= assetBundleInfo.Size ;
					}
				}

				if( freeCacheSize <  requireSize )
				{
					// 破棄しないアセットバンドルだけで既に空き容量が足りない
					return false ;
				}
				
				if( freeAssetBundleInfo.Count == 0 )
				{
					// 空き容量は足りる
					return true ;
				}

				//--------------------------------

				// 破棄できるアセットバンドルで既にに実体を持っているものをタイムスタンプの新しいものの順にソートする
				freeAssetBundleInfo.Sort( ( a, b ) => ( int )( a.LastUpdateTime - b.LastUpdateTime ) ) ;

				int i, l = freeAssetBundleInfo.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					freeCacheSize -= freeAssetBundleInfo[ i ].Size ;
					if( freeCacheSize <  requireSize )
					{
						// ここから容量が足りない
						break ;
					}
				}

				if( i >= l )
				{
					// 空き容量は足りる
					return true ;
				}

				int s = i ;

				// ここからアセットバンドルを破棄する(古いものを優先的に破棄)
				for( i  = s ; i <  l ; i ++ )
				{
					// 一時的に削除するだけなので空になったフォルダまで削除する事はしない
					StorageAccessor_Remove( $"{StorageCacheRootPath}{ManifestName}/{freeAssetBundleInfo[ i ].Path}" ) ;
					
					freeAssetBundleInfo[ i ].LastUpdateTime = 0L ;
					freeAssetBundleInfo[ i ].UpdateRequired = true ;
				}

				Modified = true ;

				// マニフェスト情報を保存しておく
//				Save( tInstance ) ;	// この後の追加保存でマニフェスト情報を保存するのでここでは保存しない

				return true ;
			}
		}
	}
}
