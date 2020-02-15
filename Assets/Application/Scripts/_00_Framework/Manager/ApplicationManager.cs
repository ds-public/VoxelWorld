using UnityEngine ;
using System ;
using System.Collections ;
using UnityEngine.Assertions ;

#if UNITY_EDITOR
using UnityEditor ;
#endif



// 要 SceneManagementHelper パッケージ
using SceneManagementHelper ;

// 要 InputHelper パッケージ
using InputHelper ;

// 要 AudioHelper パッケージ
using AudioHelper ;

// 要 AssetBundlerHelper パッケージ
using AssetBundleHelper ;

// 要 StorageHealper パッケージ
using StorageHelper ;

using uGUIHelper ;

using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;
using __w = DBS.WorkDataCategory ;


namespace DBS
{
	/// <summary>
	/// アプリケーションマネージャクラス Version 2017/08/13 0
	/// </summary>
	public class ApplicationManager : SingletonManagerBase<ApplicationManager>
	{
		/// <summary>
		/// アプリケーションマネージャのインスタンスを生成する（スクリプトから生成する場合）
		/// </summary>
		/// <param name="tRunInBackground">バックグラウンド中でも実行するかどうか</param>
		/// <returns>アプリケーションマネージャのインスタンス</returns>
		public static ApplicationManager Create( bool tRunInBackground = true, string tServerName = "" )
		{
			bool tAtFast = false || ( m_Instance == null ) ;

			m_Instance = Create( null ) ;	// 基底クラスのものを利用する

			// 注意：独自の Create メソッド(オーバーロードメソッド)を用意する場合は、最初の引数に null 許容型を使ってはいけない。
			// 　　　null 許容型を使うと、上のメソッドで再帰永久ループが発生してしまう。

			if( tAtFast == true )
			{
				m_Instance.m_RunInBackground	= tRunInBackground ;
				m_Instance.m_ServerName			= tServerName ;     // 接続先サーバー 
			}

			return m_Instance ;
		}
		
		//-----------------------------------------------------------------
	
		// アプリケーションマネージャが初期化済みかどうか
		private bool m_Initialized ;

		/// <summary>
		/// アプリケーションマネージャが初期化済みかどうか
		/// </summary>
		public  static bool  IsInitialized
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Initialized ;
			}
		}

		// バックグラウンド状態での動作
		[SerializeField]
		private bool m_RunInBackground = true ;

		/// <summary>
		/// バックグラウンド状態での動作
		/// </summary>
		public static bool RunInBackground
		{
			get
			{
				if( m_Instance == null )
				{
#if UNITY_EDITOR
					Debug.LogError( "ApplicationManager is not create !" ) ;
#endif
					return false ;
				}
				return m_Instance.m_RunInBackground ;
			}
		}
		
		// 接続先サーバー(受け渡しのみ)
		private string m_ServerName = "" ;

		// StreamingAssets に　AssetBundle が存在するかどうか
		private bool m_HasAssetBundle ;

		// プラットフォームタイプ
		private Settings.PlatformType m_PlatformType = Settings.PlatformType.Default ;

		//-----------------------------------------------------------------
		
		// 最速で実行される特殊メソッド
	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	    static void OnRuntimeMethodLoad()
	    {
			if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer )
			{
				// スクリプトで強制的にウィンドウサイズを設定する(ProjectSettings の値が取得出来てその値を設定出来るのが理想)
//				Screen.SetResolution( 270, 480, false, 60 ) ;
				Screen.SetResolution( 960, 540, false, 60 ) ;
			}

			if
			(
				Application.platform == RuntimePlatform.WindowsEditor	||
				Application.platform == RuntimePlatform.WindowsPlayer	||
				Application.platform == RuntimePlatform.OSXEditor		||
				Application.platform == RuntimePlatform.OSXPlayer		||
				Application.platform == RuntimePlatform.LinuxEditor		||
				Application.platform == RuntimePlatform.LinuxPlayer
			)
			{
				// 入力モードをカスタム(ＰＣ用)にする
				UIEventSystem.ProcessType = StandaloneInputModuleWrapper.ProcessType.Custom ;
			}
	    }

		//-----------------------------------------------------------------
		
		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
		new protected void Awake()
		{
			base.Awake() ;

			// アサートの出力を有効化
			Assert.IsTrue( true ) ;

			// スリープ禁止
			Screen.sleepTimeout = SleepTimeout.NeverSleep ;

			// デフォルトでマルチタッチを禁止しておく(必要があればシーンごとに許可)
			Input.multiTouchEnabled = false ;

			// 画面の向き設定
//			Screen.orientation = ScreenOrientation.AutoRotation ;	// 回転許可
			Screen.orientation = ScreenOrientation.Landscape ;		// 回転許可
			Screen.autorotateToPortrait           = false ;			// 縦許可
			Screen.autorotateToPortraitUpsideDown = false ;			// 縦許可
			Screen.autorotateToLandscapeLeft      = true ;			// 横禁止
			Screen.autorotateToLandscapeRight     = true ;			// 横禁止

			// 描画フレームレートの設定（３０）
			Application.targetFrameRate = 30 ;

			// 処理フレームレートの設定（２０）
			Time.fixedDeltaTime = 0.050f ;

			//----------------------------------------------------------

			//----------------------------------------------------------

			// 簡易デバッグログ表示を有効にする
//			DebugScreen.Create( 0xFFFF0000 ) ;
		}
	
		//-----------------------------------------------------------------
	
		IEnumerator Start()
		{
			// バックグラウンド動作設定(ＵｎｉｔｙＥｄｉｔｏｒのみ / Ｕｎｔｙには実機環境で値を書き込んだら落ちるバージョンが存在する)
			#if UNITY_EDITOR

			Application.runInBackground = RunInBackground ;

			#endif

//			Debug.LogWarning( "runInBackground:" + Application.runInBackground ) ;
			//-----------------------------

			// プラットフォームタイプを読み出す
			Settings settings = Resources.Load<Settings>( "Settings/Settings" ) ;
			if( settings != null )
			{
				m_PlatformType = settings.SelectedPlatformType ;
			}

			//---------------------------------------------------------

			// StreaminAssets に AssetBundle が存在するか確認する
			string exist = null ;
			yield return StartCoroutine( StorageAccessor.LoadTextFromStreamingAssets( "ibrains/moe/Exist.txt", ( _ ) => { exist = _ ; } ) ) ;
			if( string.IsNullOrEmpty( exist ) == false )
			{
				Debug.LogWarning( "[StreamingAssets に AssetBundle が存在する]" ) ;
				m_HasAssetBundle = true ;
			}

			//----------------------------------------------------------
			
			// ローカライズマネージャを生成する(最速で生成する)
			if( LocalizeManager.Instance == null )
			{
				LocalizeManager.Create( transform ) ;
				LocalizeManager.Load() ;	// ダミーロード
			}

			// プレイヤー情報保持用のゲームオブジェクトを生成する
			if( MassDataManager.Instance == null )
			{
				MassDataManager.Create( transform ) ;
			}

			// プレイヤー情報保持用のゲームオブジェクトを生成する
			if( UserDataManager.Instance == null )
			{
				UserDataManager.Create( transform ) ;
			}

			// プレイヤー情報保持用のゲームオブジェクトを生成する
			if( WorkDataManager.Instance == null )
			{
				WorkDataManager.Create( transform ) ;
			}

			// プリファレンス情報保持用のゲームオブジェクトを生成する
			if( PreferenceManager.Instance == null )
			{
				PreferenceManager.Create( transform ) ;
			}

			//----------------------------------------------------------

			// ネットワークマネージャを生成する
/*
			if( NetworkManager.instance == null )
			{
				NetworkManager.Create( transform ) ;
				if( string.IsNullOrEmpty( m_ServerName ) == false )
				{
					NetworkManager.serverName = m_ServerName ;
				}
			}

			// ダウンロードマネージャを生成する
			if( DownloadManager.instance == null )
			{
				DownloadManager.Create( transform ) ;
			}
*/
			
			//--------------------------------------------------------------------------

			// 他のマネージャ系が生成されていなければ生成する

			// 汎用シーンマネージャの生成
			if( EnhancedSceneManager.Instance == null )
			{
				EnhancedSceneManager.Create( transform ) ;
			}

			// 汎用オーディオマネージャの生成
			if( AudioManager.instance == null )
			{
				AudioManager.Create( transform, true ) ;
			}

			// 汎用インプットマネージャの生成
			if( InputManager.Instance == null )
			{
				InputManager.Create( transform ) ;

				// 名前からプロフィールを設定する
				string[] tNames = GamePad.GetNames() ;

				if( tNames != null && tNames.Length >  0 )
				{
					GamePad.SetProfile( 0, GamePad.Profile_Xbox ) ;
					GamePad.SetProfile( 1, GamePad.Profile_PlayStation ) ;

					int i, l = tNames.Length ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						Debug.LogWarning( "GamePad : " + i + " Name = " + tNames[ i ] ) ;

						if( string.IsNullOrEmpty( tNames[ i ] ) == false )
						{
							if( tNames[ i ].ToLower().Contains( "Xbox".ToLower() ) == true )
							{
								Debug.LogWarning( "Player " + i + " = Xbox" ) ;
								GamePad.SetDefaultProfileNumber( i, 0 ) ;
							}
							else
							{
								Debug.LogWarning( "Player " + i + " = PlayStation" ) ;
								GamePad.SetDefaultProfileNumber( i, 1 ) ;
							}
						}
					}
				}
			}

			// 汎用アセットバンドルマネージャの生成
			if( AssetBundleManager.Instance == null )
			{
				AssetBundleManager.Create( transform ) ;
			}

			//----------------------------------------------------------

			// スクリーンマネージャを生成する
			if( ScreenManager.Instance == null )
			{
				ScreenManager.Create( transform ) ;
				ScreenManager.SetScreenAttributeData( ScreenAttribute.Data ) ;
			}

			//----------------------------------------------------------

			// 常駐型のサブシーンをロードする
			yield return StartCoroutine( LoadResidentSubScenes() ) ;

			//------------------------------------------------------------------

			//----------------------------------------------------------

			// ネットワークマネージャにマスターデータマネージャとゲームデータマネージャのデータ更新用のデリゲートを設定する

/*			MassDataManager.Setup() ;
			UserDataManager.Setup() ;*/

			//----------------------------------------------------------

			// 最初の通信を行いマスターデータ・ゲームデータをセットアップする
			// 通信中に汎用プログレスや汎用ダイアログを使用するので
			// 通信は必ずそれらが展開されてから行うこと
			
/*			yield return NetAPI.sync.Timestamp( ( tResponseData ) =>
			{
				Debug.LogWarning( "サーバーのタイムスタンプ値:" + tResponseData.Success + " " +  tResponseData.ServerTime ) ;

				// サーバータイムとクライアントタイムの差分を補正値として記録する
				TimeUtility.SetCorrectTime( tResponseData.ServerTime - TimeUtility.GetRealCurrentUnixTime() ) ;
			} ) ;*/

			// ここには通信で成功した時しか来ない
//			Debug.LogWarning( "タイムスタンプ値:" + ( tRequest.data as Common.Response.Timestamp ).Timestamp_ ) ;

//			Debug.LogWarning( "----- シーン名:" + Scene.name ) ;

/*			long	tPlayerId		= Player.id ;
			string	tPlayerToken	= Player.token ;

			// ScriptableObject から接続先を読み込む(デフォルトの接続先)
			ConnectData tConnectData = Resources.Load<ConnectData>( "ConnectData" ) ;
			if( tConnectData.playerId != 0 && string.IsNullOrEmpty( tConnectData.playerToken ) == false )
			{
				tPlayerId		= tConnectData.playerId ;
				tPlayerToken	= tConnectData.playerToken ;

				Debug.Log( "===== ConnectData の PlayerId と PlayerToken を使用する =====" ) ;
				Debug.Log( "Id    : " + tPlayerId		) ;
				Debug.Log( "Token : " + tPlayerToken	) ;
			}

			// 現在のシーンがサーバーセレクト・ブート・タイトルでなければ自動ログインする(デバッグ機能)
			if( Scene.name != Scene.SelectServer && Scene.name != Scene.Boot && Scene.name != Scene.Title )
			{
				if( tPlayerId <= 0 )
				{
					// ユーザー未作成
					Debug.LogWarning( "ユーザーが作成されていないためタイトルシーンを起動してユーザーを作成してください" ) ;
				}
				else
				{
					// ユーザー作成済み

					// 自動的にログインしてゲームデータを最新の状態にする

					// ログインを行う(通信が完了するまで待つ：基本的にエラーが起きた場合は NetworkManager 内のみで処理が完結してここには制御は戻らないのでエラーの場合を考慮する必要は無い)
					yield return NetAPI.auth.Login( tPlayerId, tPlayerToken, ( tResponseData ) =>
					{
						// アクセストークンを　NetworkManager に保存(更新)する
						NetworkManager.accessToken = tResponseData.AccessToken ;

						Debug.Log( "===== ApplicationManager での自動ログイン成功 =====" ) ;
						Debug.Log( "Id    : " + tPlayerId		) ;
						Debug.Log( "Token : " + tPlayerToken	) ;
						Debug.Log( "AccessToken : " + NetworkManager.accessToken ) ;
					} ) ;
				}
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogWarning( "======= サーバーセレクトシーン・ブートシーン・タイトルシーンのいずれかから開始したので自動ログインは行わない" ) ;
			}
#endif
*/
			//----------------------------------------------------------

			// アセットバンドルのマニフェストをロードする
			if( AssetBundleManager.Instance != null )
			{
				// マニフェストをダウンロードする
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				AssetBundleManager.SecretPathEnabled = false ;  // アセットバンドルをローカルストレージに保存いる際にファイル名を暗号化する	
#else
//				AssetBundleManager.secretPathEnabled = true ;  // アセットバンドルをローカルストレージに保存いる際にファイル名を暗号化する	
#endif
	
/*				if( Scene.name != Scene.SelectServer && Scene.name != Scene.Boot && Scene.name != Scene.Title && Scene.name != Scene.Battle && Scene.name != Scene.ModelViewer && Scene.name != Scene.BattleModelViewer && Scene.name != Scene.EventChecker && Scene.name != Scene.Event )
				{
					// デバッグモード(シーン直接指定)で起動
					AssetBundleManager.localPriority = AssetBundleManager.LocalPriority.StreamingAssets ;	// 優先 StreamingAssets > Resources > AssetBundle
//					AssetBundleManager.localPriority = AssetBundleManager.LocalPriority.High ;	// 優先 StreamingAssets > Resources > AssetBundle
					Debug.LogWarning( "[注意]各種アセットはデバッグ用の StreamingAssets 下の AssetBundle を優先的に使用します" ) ;
					AssetBundleManager.useResources = AssetBundleManager.UserResources.Same ;       // ネットワーク上のアセットバンドルが見つからない場合は Resources から探す
					AssetBundleManager.useStreamingAssets = true ;	// ネットワーク上のアセットバンドルが見つからない場合は StreamingAssets から探す
				}
				else
				{*/
					AssetBundleManager.LoadPriorityType = AssetBundleManager.LoadPriority.Local ; // 優先 Resources > StreamingAssets > AssetBundle
					Debug.LogWarning( "[注意]各種アセットは Resources を優先的に使用します" ) ;
					AssetBundleManager.UseResources = AssetBundleManager.UserResources.Same ;       // ネットワーク上のアセットバンドルが見つからない場合は Resources から探す
					AssetBundleManager.UseStreamingAssets = true ;	// ネットワーク上のアセットバンドルが見つからない場合は StreamingAssets から探す
/*				}*/

				AssetBundleManager.FastLoadEnabled = false ;	// 一部同期化で高速化読み出し

				// 実際はマスターの通信が完了してからそちらから取得する
				string tDomainName = "http://vms010.ibrains.co.jp/ibrains/moe/" ;
//				string tDomainName = "http://localhost/ibrains/moe/" ;


				// マニフェストを登録
				AssetBundleManager.AddManifest( tDomainName + Define.assetBundlePlatformName + "/" + Define.assetBundlePlatformName ) ;

				// デフォルトマニフェスト名を登録する
				AssetBundleManager.DefaultManifestName = Define.assetBundlePlatformName ;

				// 登録された全てのマニフェストのダウンロード
				yield return AssetBundleManager.LoadAllManifestsAsync() ;

				// 全ての Manifest がダウンロードされるのを待つ
				while( AssetBundleManager.IsAllManifestsCompleted == false )
				{
					if( AssetBundleManager.GetAnyManifestError( out string tManifestName, out string tManifestError ) == true )
					{
#if UNITY_EDITOR
						Debug.LogError( "マニフェストのロードでエラーが発生しました:" + tManifestName + " -> " + tManifestError );
#endif
						break;
					}

					yield return null ;
				}
				
				// 各マニフェストのキャッシュサイズを設定するサンプル
				AssetBundleManager.ManifestInfo tM = AssetBundleManager.GetManifest( Define.assetBundlePlatformName ) ;
				if( tM != null )
				{
					tM.CacheSize = 1024 * 1024 * 1024 ;	// キャッシュサイズを１ＧＢに設定

					string sizeName = "" ;
					long size = tM.CacheSize ;
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
						sizeName = ( size / ( 1024L * 1024L * 1024L ) ) + "GB" ;
					}
					Debug.LogWarning( "マニフェスト " + tM.ManifestName +" のキャッシュサイズを " + sizeName + " に制限しました。" ) ;

//					tM.fastLoadEnabled = false ;	// 非同期では完全に非同期でロードする
				}

				//---------------------------------------------------------


				// 仮のデバッグ動作として、アセットバンドル化対象リソースがローカルに存在しない場合、
				// 絶対に必要系のアセットバンドルを、ここでダウンロードしてしまう。

/*				if( hasResources == false )
				{
					// アセットバンドル化対象リソースがローカルに存在しない
					yield return StartCoroutine( Asset.DownloadAssetBundleAsync( "Texts", true  ) ) ;
					yield return StartCoroutine( Asset.DownloadAssetBundleAsync( "Textures/ItemIcon", true  ) ) ;
					yield return StartCoroutine( Asset.DownloadAssetBundleAsync( "Textures/SkillIcon", true  ) ) ;
					yield return StartCoroutine( Asset.DownloadAssetBundleAsync( "Textures/UnitIcon", true  ) ) ;
					yield return StartCoroutine( Asset.DownloadAssetBundleAsync( "Textures/Tips", true  ) ) ;
				}*/
			}

			//----------------------------------------------------------

			//----------------------------------------------------------

			// MassData を展開する
//			yield return MassDataManager.LoadAsync() ;

			// UserData を展開する
//			yield return UserDataManager.LoadAsync() ;

			//----------------------------------------------------------

			//----------------------------------------------------------

//			Debug.LogWarning( "ユーザーデータをセーブしました" ) ;
//			UserData.Save() ;

			//----------------------------------------------------------

			// 初期化が完了した
			m_Initialized = true ;

			//----------------------------------------------------------

/*			if( Scene.name != Scene.SelectServer && Scene.name != Scene.Boot && Scene.name != Scene.Title )
			{
				if( tPlayerId <= 0 )
				{
					// ユーザー未作成
#if UNITY_EDITOR
					Debug.LogWarning( "タイトルシーンへ強制的に遷移します" ) ;
#endif
					Scene.Load( Scene.Title ) ;
				}
			}*/
		}

		//-----------------------------------------------------------------
	
		// 常駐型のサブシーンをロードする
		private IEnumerator LoadResidentSubScenes()
		{
			if( SceneMask.Instance == null )
			{
				// フェード演出用のシーンをロードする
				SceneMask sceneMask = null ;
				yield return EnhancedSceneManager.AddAsync<SceneMask>( "SceneMask", ( _ ) => { sceneMask = _[ 0 ] ; } ) ;
				if( sceneMask != null )
				{
					DontDestroyOnLoad( sceneMask.gameObject ) ;
//					sceneMask.gameObject.SetActive( false ) ;
					SceneMask.Show() ;
				}
			}

			if( Dialog.Instance == null )
			{
				// ダイアログ用のシーンをロードする
				Dialog dialog = null ;
				yield return StartCoroutine( EnhancedSceneManager.AddAsync<Dialog>( "Dialog", ( _ ) =>{ dialog = _[ 0 ] ; } ) ) ;
				if( dialog != null )
				{
					DontDestroyOnLoad( dialog.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
					dialog.gameObject.SetActive( false ) ;
				}
			}
			
			if( Fade.Instance == null )
			{
				// フェード演出用のシーンをロードする
				Fade fade = null ;
				yield return StartCoroutine( EnhancedSceneManager.AddAsync<Fade>( "Fade", ( _ ) => { fade = _[ 0 ] ; } ) ) ;
				if( fade != null )
				{
					DontDestroyOnLoad( fade.gameObject ) ;
					fade.gameObject.SetActive( false ) ;
				}
			}

			if( Progress.Instance == null )
			{
				// プログレス演出用のシーンをロードする
				Progress progress = null ;
				yield return StartCoroutine( EnhancedSceneManager.AddAsync<Progress>( "Progress", ( _ ) => { progress = _[ 0 ] ; } ) ) ;
				if( progress != null )
				{
					DontDestroyOnLoad( progress.gameObject ) ;
					progress.gameObject.SetActive( false ) ;
				}
			}

			if( Ripple.Instance == null )
			{
				// タッチ演出用のシーンをロードする
				Ripple ripple = null ;
				yield return StartCoroutine( EnhancedSceneManager.AddAsync<Ripple>( "Ripple", ( _ ) => { ripple = _[ 0 ] ; } ) ) ;
				if( ripple != null )
				{
					DontDestroyOnLoad( ripple.gameObject ) ;
				}
			}

			if( AlertDialog.instance == null )
			{
				// ダイアログ用のシーンをロードする
				AlertDialog alertDialog = null ;
				yield return EnhancedSceneManager.AddAsync<AlertDialog>( "AlertDialog", ( _ ) => { alertDialog = _[ 0 ] ; } ) ;
				if( alertDialog != null )
				{
					DontDestroyOnLoad( alertDialog.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
					alertDialog.gameObject.SetActive( false ) ;
				}
			}

			//--------------------------------------------------------------------------

			if( SceneMask.Instance != null )
			{
				SceneMask.Hide() ;
			}
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// ローカルにリソースを持っているかどうか
		/// </summary>
		public static bool HasResources
		{
			get
			{
				TextAsset tTA = Resources.Load<TextAsset>( "Exist" ) ;
				return tTA != null ;
			}
		}

		/// <summary>
		/// SteamingAssets に AssetBundle を持っているかどうか
		/// </summary>
		public static bool HasAssetBundle
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_HasAssetBundle ;
			}
		}

		/// <summary>
		/// メーラーを起動する
		/// </summary>
		/// <param name="tAddress"></param>
		/// <param name="tTitle"></param>
		/// <param name="tMessage"></param>
		public static void OpenMailer( string tAddress, string tTitle, string tMessage )
		{
			if( string.IsNullOrEmpty( tTitle ) == false )
			{
				tTitle		= System.Uri.EscapeDataString( tTitle	) ;
			}

			if( string.IsNullOrEmpty( tMessage ) == false )
			{
				tMessage	= System.Uri.EscapeDataString( tMessage	) ;
			}

			Application.OpenURL( "mailto:" + tAddress + "?subject=" + tTitle + "&body=" + tMessage ) ;
		}

		/// <summary>
		/// UnityEditor を一時停止する
		/// </summary>
		/// <param name="message"></param>
		public static void Pause( string message = null )
		{
			if( string.IsNullOrEmpty( message ) == false )
			{
				Debug.Log( "[PAUSE] " + message ) ;
			}

#if UNITY_EDITOR
			EditorApplication.isPaused = true ;				
#endif
		}

		/// <summary>
		/// モバイル環境かどうか判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsMobile()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return ( m_Instance.m_PlatformType == Settings.PlatformType.Mobile || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ) ;
		}
	}
}
