using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;


// 要 AssetBundleHelper パッケージ
using AssetBundleHelper ;

using uGUIHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// アセットクラス(アセット全般の読み出しに使用する) Version 2017/08/21 0
	/// </summary>
	public class Asset
	{
		//-----------------------------------------------------------------

		/// <summary>
		/// キャッシュタイプ
		/// </summary>
		public enum CachingType
		{
			None			= 0,	// キャッシュしない
			ResourceOnly	= 1,	// リソースのみキャッシャする
			AssetBundleOnly	= 2,	// アセットバンドルのみキャッシュする
			Same			= 3,	// リソース・アセットバンドルともにキャッシュする
		}

		// 注意：アセットバンドルから展開させるリソースのインスタンスについて
		//
		// パスは同じでも異なるアセットバンドルのインスタンスから展開された同じパスのリソースは、
		// 別のリソースとして扱われる(別のインスタンスとなる)
		// よってアセットバンドルの展開そのものと、
		// 展開されたアセットバンドルからのリソースの展開は、重々注意する必要がある。
		// (重複展開され、無駄にメモリを消費する事になる。その他にもバグの要因となる。)

		// None：展開されたリソースのインスタンスは別個
		// 　動作をきちんと理解していないと危険なタイプである。
		// 　同じ動作を２度行った際の展開されたリソースのインスタンスは別物になる。
		// 　そのシーンで１度しか使わないようなものに対してのみ使用すること。
		//
		// ResourceOnly：展開されたリソースのインスタンスは同一
		// 　展開されたリソースそのもののインスタンスは同一になるが、
		// 　リソース群が同じアセットバンドルに内包されている場合、
		// 　何度も無駄にアセットバンドルの展開が発生する事になる。
		//
		// AssetBundleOnly：展開されたリソースのインスタンスは同一
		// 　展開されたアセットバンドルのインスタンス内に、
		// 　展開されたリソースのインスタンス(実体はシステムリソースキャッシュにある)を
		// 　保持しているため、同じリソースであればリソースの再展開は行われず、
		// 　同一のリソースのリソースのインスタンスが返される。
		//
		// Same：展開されたリソースのインスタンスは同一
		// 　ResourceOnly とと同じく、リソースキャッシュに保存された、
		// 　展開されたリソースのインスタンスを返すので、
		// 　展開されたリソースのインスタンスは同一になる。

		//-----------------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == true || string.IsNullOrEmpty( Error ) == false )
					{
						return false ;    // 終了
					}
					return true ;   // 継続
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// アセット
			/// </summary>
			public System.Object Asset = null ;

			/// <summary>
			/// 指定の型でアセットを取得する(直接 .asset でも良い)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public T GetAsset<T>() where T : class
			{
				if( Asset == null )
				{
					return null ;
				}

				return Asset as T ;
			}

			/// <summary>
			/// インスタンス
			/// </summary>
			public UnityEngine.Object[]	Assets = null ;

			/// <summary>
			/// 指定の型でインスタンスを取得する(直接 .instances でも良い)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public T[] GetAssets<T>() where T : UnityEngine.Object
			{
				if( Assets == null || Assets.Length == 0 )
				{
					return null ;
				}

				T[] assets = new T[ Assets.Length ] ;
				for( int i  = 0 ; i <  Assets.Length ; i ++ )
				{
					assets[ i ] = Assets[ i ] as T ;
				}
				
				return assets ;
			}

			/// <summary>
			/// アセットバンドル
			/// </summary>
			public AssetBundle	AssetBundle = null ;
			
			//----------------------------------------------------------

			// リザルトコード
			public int	ResultCode = 0 ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string Error = "" ;

			/// <summary>
			/// ダウンロード状況
			/// </summary>
			public float Progress = 0 ;
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		// Asset

		/// <summary>
		/// アセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static T Load<T>( string path, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAsset<T>( path, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		/// <summary>
		/// アセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static UnityEngine.Object Load( string path, Type type, CachingType cachingType = CachingType.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAsset( path, type, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		//---------------

		/// <summary>
		/// アセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync<T>( string path, Action<T> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAsync_Private( path, typeof( T ), ( _ ) => { onLoaded?.Invoke( _ as T ) ; }, cachingType, isNoDialog, request ) ) ;
			return request ;
		}

		/// <summary>
		/// アセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync( string path, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAsync_Private( path, type, onLoaded, cachingType, isNoDialog, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// アセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadAsync_Private( string path, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType, bool isNoDialog, Request request )
		{
			while( true )
			{
				UnityEngine.Object asset = null ;
				var assetRequest = AssetBundleManager.LoadAssetAsync( path, type, ( _ ) => { asset = _ ; }, ( AssetBundleManager.CachingType )cachingType, false ) ;
				yield return assetRequest ;
	
				if( asset != null )
				{
					// 成功
					request.IsDone = true ;
					onLoaded?.Invoke( asset ) ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// AllAssets
		
		/// <summary>
		/// 全てのアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>全てのサブアセットのインスタンスが格納されたディクショナリ配列</returns>
		public static T[] LoadAll<T>( string path, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAllAssets<T>( path, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		/// <summary>
		/// 全てのアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>全てのサブアセットのインスタンスが格納されたディクショナリ配列</returns>
		public static UnityEngine.Object[] LoadAll( string path, Type type, CachingType cachingType = CachingType.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAllAssets( path, type, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		//---------------

		/// <summary>
		/// 全てのアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllAsync<T>( string path, Action<T[]> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAllAsync_Private
			(
				path, typeof( T ),
				( _ ) =>
				{
					if( onLoaded != null )
					{
						T[] assets = new T[ _.Length ] ;
						for( int i  = 0 ; i <  _.Length ; i ++ )
						{
							assets[ i ] = _[ i ] as T ;
						}
						onLoaded( assets ) ;
					}
				},
				cachingType, isNoDialog, request
			) ) ;
			return request ;
		}

		/// <summary>
		/// 全てのアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAllAsync_Private( path, type, onLoaded, cachingType, isNoDialog, request ) ) ;
			return request ;
		}

		/// <summary>
		/// 全てのアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadAllAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingType cachingType, bool isNoDialog, Request request )
		{
			while( true )
			{
				UnityEngine.Object[] assets = null ;
				var assetRequest = AssetBundleManager.LoadAllAssetsAsync( path, type, ( _ ) => { assets = _ ; }, ( AssetBundleManager.CachingType )cachingType, false ) ;
				yield return assetRequest ;
	
				if( assets != null )
				{
					// 成功
					request.IsDone = true ;
					onLoaded?.Invoke( assets ) ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// SubAsset

		/// <summary>
		/// サブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセットの名前</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static T LoadSub<T>( string path, string subAssetName, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadSubAsset<T>( path, subAssetName, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		/// <summary>
		/// サブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセットの名前</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>任意のコンポーネント型のアセットのインスタンス</returns>
		public static UnityEngine.Object LoadSub( string path, string subAssetName, Type type, CachingType cachingType = CachingType.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadSubAsset( path, subAssetName, type, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		//---------------

		/// <summary>
		/// サブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセットの名前</param>
		/// <param name="rSubAsset">読み出されたサブアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadSubAsync<T>( string path, string subAssetName, Action<T> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadSubAsync_Private( path, subAssetName, typeof( T ), ( _ ) => { onLoaded?.Invoke( _ as T ) ; }, cachingType, isNoDialog, request ) ) ;
			return request ;
		}

		/// <summary>
		/// サブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセットの名前</param>
		/// <param name="rSubAsset">読み出されたサブアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadSubAsync( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadSubAsync_Private( path, subAssetName, type, onLoaded, cachingType, isNoDialog, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// サブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセットの名前</param>
		/// <param name="rSubAsset">読み出されたサブアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadSubAsync_Private( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType, bool isNoDialog, Request request )
		{
			while( true )
			{
				UnityEngine.Object asset = null ;
				var assetRequest = AssetBundleManager.LoadSubAssetAsync( path, subAssetName, type, ( _ ) => { asset = _ ; }, ( AssetBundleManager.CachingType )cachingType, false ) ;
				yield return assetRequest ;
					
				if( asset != null )
				{
					// 成功
					request.IsDone = true ;
					onLoaded?.Invoke( asset ) ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		// AllSubAssets
		
		/// <summary>
		/// 全てのサブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>全てのサブアセットのインスタンスが格納されたディクショナリ配列</returns>
		public static T[] LoadAllSub<T>( string path, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAllSubAssets<T>( path, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		/// <summary>
		/// 全てのサブアセットを読み出す(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>全てのサブアセットのインスタンスが格納されたディクショナリ配列</returns>
		public static UnityEngine.Object[] LoadAllSub( string path, Type type, CachingType cachingType = CachingType.None )
		{
			// 同期ロードで失敗してもエラーダイアログは表示されないので同期ロードは成功前提です
			return AssetBundleManager.LoadAllSubAssets( path, type, ( AssetBundleManager.CachingType )cachingType ) ;
		}

		//---------------

		/// <summary>
		/// てのサブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllSubAsync<T>( string path, Action<T[]> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAllSubAsync_Private
			(
				path, typeof( T ),
				( _ ) =>
				{
					if( onLoaded != null )
					{
						T[] assets = new T[ _.Length ] ;
						for( int i  = 0 ; i <  _.Length ; i ++ )
						{
							assets[ i ] = _[ i ] as T ;
						}
						onLoaded( assets ) ;
					}
				},
				cachingType, isNoDialog, request
			) ) ;
			return request ;
		}

		/// <summary>
		/// 全てのサブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllSubAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingType cachingType = CachingType.None, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAllSubAsync_Private( path, type, onLoaded, cachingType, isNoDialog, request ) ) ;
			return request ;
		}

		/// <summary>
		/// てのサブアセットを読み出す(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAllSubAssets">読み出された全てのサブアセットのインスタンスが格納されたディクショナリ配列を格納する要素数１以上の任意のディクショナリの配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadAllSubAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingType cachingType, bool isNoDialog, Request request )
		{
			while( true )
			{
				UnityEngine.Object[] assets = null ;
				var assetRequest = AssetBundleManager.LoadAllSubAssetsAsync( path, type, ( _ ) => { assets = _ ; }, ( AssetBundleManager.CachingType )cachingType, false ) ;
				yield return assetRequest ;
	
				if( assets != null )
				{
					// 成功
					request.IsDone = true ;
					onLoaded?.Invoke( assets ) ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// Scene

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadSceneAsync( string path, string sceneName = null, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, isNoDialog, 0, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadSceneAsync<T>( string path, Action<T[]> onLoaded, string targetName = null, string sceneName = null, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private
			(
				path, typeof( T ),
				( _ ) =>
				{
					if( onLoaded != null )
					{
						T[] targets = new T[ _.Length ] ;
						for( int i  = 0 ; i <  _.Length ; i ++ )
						{
							targets[ i ] = _[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, sceneName, isNoDialog, 0, request
			) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request LoadAScenesync( string path, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName = null, string sceneName = null, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, isNoDialog, 0, request ) ) ;
			return request ;
		}
		//---------------

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request AddSceneAsync( string path, string sceneName = null, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, isNoDialog, 1, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request AddSceneAsync<T>( string path, Action<T[]> onLoaded, string targetName = null, string sceneName = null, bool isNoDialog = false ) where T : UnityEngine.Object
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private
			(
				path, typeof( T ),
				( _ ) =>
				{
					if( onLoaded != null )
					{
						T[] targets = new T[ _.Length ] ;
						for( int i  = 0 ; i <  _.Length ; i ++ )
						{
							targets[ i ] = _[ i ] as T ;
						}
						onLoaded( targets ) ;
					}
				},
				targetName, sceneName, isNoDialog, 1, request
			) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		public static Request AddAScenesync( string path, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName = null, string sceneName = null, bool isNoDialog = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, isNoDialog, 0, request ) ) ;
			return request ;
		}

		//-------------------------------------------

		/// <summary>
		/// シーンを展開または加算する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadOrAddSceneAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, string sceneName, bool isNoDialog, int mode, Request request )
		{
			while( true )
			{
				UnityEngine.Object[] targets = null ;
				AssetBundleManager.Request assetRequest ;
				if( mode == 0 )
				{
					// Load
					assetRequest = AssetBundleManager.LoadSceneAsync( path, type, ( _ ) => { targets = _ ; }, targetName, sceneName, false ) ;
				}
				else
				{
					// Add
					assetRequest =  AssetBundleManager.AddSceneAsync( path, type, ( _ ) => { targets = _ ; }, targetName, sceneName, false ) ;
				}
				yield return assetRequest ;
	
				if( targets != null )
				{
					// 成功
					request.IsDone = true ;
					onLoaded?.Invoke( targets ) ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// AssetBundle

		/// <summary>
		/// アセットバンドルをダウンロードする
		/// </summary>
		/// <param name="tPath"></param>
		/// <param name="tKeep"></param>
		/// <returns></returns>
		public static Request DownloadAssetBundleAsync( string path, bool keep = false, bool isNoDialog = false  )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine(	DownloadAssetBundleAsync_Private( path, keep, isNoDialog, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// アセットバンドルをダウンロードする(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">リソースまたはアセットバンドルのパス</param>
		/// <param name="rAsset">読み出されたアセットのインスタンスを格納する要素数１以上の任意のコンポーネント型の配列</param>
		/// <param name="tCaching">読み出されたアセットをメモリキャッシュにためるかどうか</param>
		/// <returns>列挙子</returns>
		private static IEnumerator DownloadAssetBundleAsync_Private( string path, bool keep, bool isNoDialog, Request request )
		{
			while( true )
			{
				var assetRequest = AssetBundleManager.DownloadAssetBundleAsync( path, keep ) ;
				while( true )
				{
					request.Progress = assetRequest.Progress ;
					if( assetRequest.IsDone == true || string.IsNullOrEmpty( assetRequest.Error ) == false )
					{
						break ;
					}

					yield return null ;
				}

				if( assetRequest.IsDone == true )
				{
					// 成功
					request.IsDone = true ;
					yield break ;
				}
				else
				{
					// 失敗
					if( isNoDialog == false )
					{
						// エラーダイアログを表示する
						bool reboot = false ;
						yield return ApplicationManager.Instance.StartCoroutine( OpenDownloadErrorDialog( assetRequest.ResultCode, path, ( _ ) => { reboot = _ ; } ) ) ;
						if( reboot == true )
						{
							// コルーチン終了(シングルトンである ApplicationManager のコルーチンスタックが残り続けてしまうため)
							request.Error = "Reboot" ;
							yield break ;
						}
						// Rebootしなければリトライする
					}
					else
					{
						// エラーダイアログを表示しない(そのままエラーとなる)
						request.Error = "Throw" ;
						yield break ;
					}
				}
			}
		}

		/// <summary>
		/// アセットバンドルを破棄する
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public static bool RemoveAssetBundle( string path )
		{
			return AssetBundleManager.RemoveAssetBundle( path ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルが管理対象に含まれているか確認する
		/// </summary>
		/// <param name="tNeedUpdateOnly"></param>
		/// <returns></returns>
		public static bool Contains( string path )
		{
			return AssetBundleManager.Contains( path ) ;
		}

		/// <summary>
		/// 指定のパスにアセットバンドルファイルが存在するか確認する
		/// </summary>
		/// <param name="tPath">パス</param>
		/// <returns></returns>
		public static bool Exists( string path )
		{
			return AssetBundleManager.Exists( path ) ;
		}

		/// <summary>
		/// 指定のパスにアセットバンドルファイルのサイズを取得する
		/// </summary>
		/// <param name="tPath">パス</param>
		/// <returns></returns>
		public static int GetSize( string path )
		{
			return AssetBundleManager.GetSize( path ) ;
		}

		/// <summary>
		/// アセットバンドルのパス群を取得する
		/// </summary>
		/// <param name="tNeedUpdateOnly"></param>
		/// <returns></returns>
		public static string[] GetAllAssetBundlePaths( bool updateRequiredOnly = false )
		{
			return AssetBundleManager.GetAllAssetBundlePaths( updateRequiredOnly ) ;
		}

		/// <summary>
		/// 依存関係にあるアセットバンドルのパス群を取得する
		/// </summary>
		/// <param name="tAssetBundleName"></param>
		/// <param name="tNeedUpdateOnly"></param>
		/// <returns></returns>
		public static string[] GetAllDependentAssetBundlePaths( string assetBundlePath, bool updateRequiredOnly = false )
		{
			return AssetBundleManager.GetAllDependentAssetBundlePaths( assetBundlePath, updateRequiredOnly ) ;
		}

		//-------------------------------------------------------------------------------------------

		// ダウンロードエラーダイアログを表示する
		public static IEnumerator OpenDownloadErrorDialog( int resultCode, string path, Action<bool> onAction )
		{
			string title = "リソース取得エラー" ;
			string message = "データのダウンロードに失敗しました" ;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			message += ( "\n\n" + path ) ;

			message += "\n\n" ;

			if( resultCode ==  1 )
			{
				message += "名前が不正" ;
			}
			else
			if( resultCode ==  2 )
			{
				message += "リモートのファイルが存在しないかデータが不正" ;
			}
			else
			if( resultCode ==  3 )
			{
				message += "ローカルストレージ足らず保存失敗" ;
			}
			else
			{
				message += "エラー原因が不明" ;
			}
#endif

			//----------------------------------------------------------

			string rebootSceneName	= Scene.Screen.Title ;
			bool executeReboot		= false ;	// リブートする場合のリブート後の開始シーン名

			// ダイアログを出すためプログレス表示中であれば消去する
			Progress.Hide() ;

			// シーンの遷移中だと入力が禁止されているので復帰させる
			UIEventSystem.Push() ;
			UIEventSystem.Enable( -1 ) ;

			yield return AlertDialog.Open
			(
				title,
				message,
				new string[]{ "リトライ", "リブート" }, 
				( int result ) =>
				{
					if( result == 1 )
					{
						executeReboot = true ;
					}
				}
			) ;

			// 入力を全面的に禁止する
			UIEventSystem.Pop() ;

			//----------------------------------------------------------

			if( executeReboot == true )
			{
				// リブートを行う
				Progress.IsOn = false ;	// プログレスの継続表示状態になっていればリセットする

				UIEventSystem.Enable( -1 ) ;
				UIEventSystem.Clear() ;

				if( ScreenManager.Instance != null )
				{
					// ヘッダーフッターが表示されていれば破棄しなければならないのでこちらを使う
					yield return ScreenManager.RebootAsync() ;
				}
				else
				{
					Scene.LoadWithFade( rebootSceneName ) ;

					// 完全にシーンの切り替えが終了する(古いシーンが破棄される)のを待つ(でないと古いシーンが悪さをする)
					yield return new WaitWhile( () => Scene.IsFading == true ) ;
				}

				onAction?.Invoke( true ) ;
			}
			else
			{
				// リトライを行う

				if( Progress.IsOn == true )
				{
					// プログレス表示要請での通信であるため再度プログレスを表示する
					Progress.Show() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		// 切りが無いので AssetBundleManager のメソッドをそのまま使えば良いものはそのまま使う事。
		// 失敗した際にダイアログを表示する等の挙動が必要な場合にのみ上記のメソッド群のようにラッパーメソッドを用意すべし

		/// <summary>
		/// メモリリソースキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCache( string message = "" )
		{
#if UNITY_EDITOR
			Debug.Log( "[AssetBundleManager] キャッシュをクリアします : " + message ) ;
#endif
			return AssetBundleManager.ClearResourceCache( false ) ;
		}
		
		/// <summary>
		/// ローカルストレージ内に保存されたアセットバンドルを全て消去する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Cleanup()
		{
			return AssetBundleManager.Cleanup() ;
		}
	}
}
