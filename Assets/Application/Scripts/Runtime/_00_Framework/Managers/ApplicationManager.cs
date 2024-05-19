#if !UNITY_EDITOR && DEVELOPMENT_BUILD
#define USE_EXCEPTION_DIALOG
#endif

using System ;
using System.Collections ;
using System.Collections.Generic ;

using System.IO ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.Assertions ;

using MessagePack ;

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

using DSW.World ;


namespace DSW
{
	/// <summary>
	/// アプリケーションマネージャクラス Version 2024/05/19 0
	/// </summary>
	public class ApplicationManager : SingletonManagerBase<ApplicationManager>
	{
		/// <summary>
		/// アプリケーションマネージャのインスタンスを生成する（スクリプトから生成する場合）
		/// </summary>
		/// <param name="tRunInBackground">バックグラウンド中でも実行するかどうか</param>
		/// <returns>アプリケーションマネージャのインスタンス</returns>
		public static ApplicationManager Create()
		{
			m_Instance = Create( null ) ;	// 基底クラスのものを利用する

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

		//-----------------------------------------------------------

		/// <summary>
		/// ダウンロードのフェーズごとの実行状態
		/// </summary>
		public enum DownloadingPhaseStates
		{
			None		= 0,
			Completed	= 1,
		}

		protected DownloadingPhaseStates m_DownloadingPhase1State = DownloadingPhaseStates.None ;

		/// <summary>
		/// ダウンロードのフェーズ１の状況
		/// </summary>
		public  static DownloadingPhaseStates  DownloadingPhase1State
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_DownloadingPhase1State ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_DownloadingPhase1State = value ;
			}
		}

		protected DownloadingPhaseStates m_DownloadingPhase2State = DownloadingPhaseStates.None ;

		/// <summary>
		/// ダウンロードのフェーズ２の状況
		/// </summary>
		public  static DownloadingPhaseStates  DownloadingPhase2State
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}

				return m_Instance.m_DownloadingPhase2State ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_DownloadingPhase2State = value ;
			}
		}

		/// <summary>
		/// ダウンロードリクエストの種別
		/// </summary>
		public enum DownloadingRequestTypes
		{
			Unknown	= 0,
			Phase1	= 1,
			Phase2	= 2,
		}

		//-----------------------------------

		// ムービーをダウンロードで再生したか
		protected bool m_IsMoviePlayedByDownload = false ;

		/// <summary>
		/// ムービーをダウンロードで再生したか
		/// </summary>
		public  static bool  IsMoviePlayedByDownload
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsMoviePlayedByDownload ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_IsMoviePlayedByDownload = value ;
			}
		}

		//-----------------------------------------------------------

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
		
		// 接続先エンドポイント(受け渡しのみ)
		[SerializeField]
		protected string m_EndPoint = string.Empty ;

		// プラットフォームタイプ
		private Settings.PlatformType m_PlatformType = Settings.PlatformType.Default ;

		// アプリケーション全体の仮想解像度の横幅
		public float CanvasWidth ;

		// アプリケーション全体の仮想解像度の縦幅
		public float CanvasHeight ;

		//-----------------------------------------------------------------
		
		// 最速で実行される特殊メソッド
	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	    static void OnRuntimeMethodLoad()
	    {
			// アサートの出力を有効化
			Assert.IsTrue( true ) ;

			// ログ出力を無効
//			Debug.unityLogger.logEnabled = false ;

			// スリープ禁止
			Screen.sleepTimeout = SleepTimeout.NeverSleep ;

			// デフォルトでマルチタッチを禁止しておく(必要があればシーンごとに許可)
			Input.multiTouchEnabled = false ;

			// 画面の向き設定
//			Screen.orientation = ScreenOrientation.AutoRotation ;	// 回転許可
			Screen.orientation = ScreenOrientation.LandscapeLeft ;		// 回転許可
			Screen.autorotateToPortrait           = false ;			// 縦許可
			Screen.autorotateToPortraitUpsideDown = false ;			// 縦許可
			Screen.autorotateToLandscapeLeft      = true ;			// 横禁止
			Screen.autorotateToLandscapeRight     = true ;			// 横禁止

			//----------------------------------------------------------

//			Settings settings = ApplicationManager.LoadSettings() ;

#if !UNITY_EDITOR

			// 参考
			// https://techblog.kayac.com/unity-fixed-dpi

			//----------------------------------------------------------
			// スクリプトで強制的に表示解像度を設定する

			float nativeWidth  = Screen.currentResolution.width ;
			float nativeHeight = Screen.currentResolution.height ;

			Debug.Log( "[Native Resolution] W = " + nativeWidth + " H = " + nativeHeight ) ;

			float screenWidth  =  960 ;
			float screenHeight =  540 ;

			Settings settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				screenWidth  = settings.BasicWidth ;
				screenHeight = settings.BasicHeight ;
			}

			bool fullScreen = false ;
			int  frameRate = 60 ;

			//---------------------------------------------------------

#if UNITY_STANDALONE

			// スタンドアロン環境

			float scale = 1.0f ;

			if( ( screenWidth == nativeWidth && screenHeight <= nativeHeight ) || ( screenWidth <= nativeWidth || screenHeight == nativeHeight ) )
			{
				// フルスクリーン可能でもウィンドウ指定なら無視する
				if( settings.IsFullScreen == true )
				{
					// 表示解像度の横と縦のいずれかが実解像度と同じで且つ表示解像度が実解像度以下であればフルスクリーン表示にする
					fullScreen = true ;
				}
			}
			else
			{
				if( ( screenWidth >  ( nativeWidth  * 0.8f ) ) && ( screenHeight >  ( nativeHeight * 0.8f ) ) )
				{
					// 実解像度の８０％より表示解像度が大きい場合はアスペクト比を維持した状態で８０％になるようにする
					if( screenWidth >  screenHeight )
					{
						// 表示解像度は横長
						scale = ( nativeWidth  * 0.8f ) / screenWidth ;
					}
					else
					{
						// 表示解像度は縦長
						scale = ( nativeHeight * 0.8f ) / screenHeight ;
					}
				}
				else
				if( ( screenWidth  >  ( nativeWidth * 0.8f ) ) )
				{
					// 実解像度の８０％より表示解像度が大きい場合はアスペクト比を維持した状態で８０％になるようにする

					// 表示解像度は横だけ溢れている
					scale = ( nativeWidth  * 0.8f ) / screenWidth ;
				}
				else
				if( ( screenHeight >  ( nativeHeight * 0.8f ) ) )
				{
					// 実解像度の８０％より表示解像度が大きい場合はアスペクト比を維持した状態で８０％になるようにする

					// 表示解像度は縦だけ溢れている
					scale = ( nativeHeight * 0.8f ) / screenHeight ;
				}
				else
				{
					while( ( screenWidth  * scale ) <= ( nativeWidth * 0.5f ) && ( screenHeight * scale ) <= ( nativeHeight * 0.5f ) )
					{
						// 実解像度の1/2以下なら表示解像度を整数倍になるよう増やしいてく
						scale += 1.0f ;
					}
				}
			}

			screenWidth  *= scale ;
			screenHeight *= scale ; 
#endif
			//---------------------------------------------------------

#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE

			// モバイル環境

			// 常にフルスクリーン
			fullScreen = true ;

			// 実解像度が表示解像度より低い場合はそのままとする
			if( nativeWidth <  screenWidth || nativeHeight <  screenHeight )
			{
				screenWidth  = nativeWidth ;
				screenHeight = nativeHeight ;
			}
			else
			{
				// 実解像度が表示解像度の1.5倍以上の場合は実解像度の短い辺の長さに合わせた表示解像度の調整を行う
				if( nativeWidth >= ( screenWidth * 1.5f ) && nativeHeight >= ( screenHeight * 1.5f ) )
				{
					if( nativeWidth <  nativeHeight )
					{
						// 横の方が短いので横を基準とした調整を行う
						screenHeight = screenWidth  * nativeHeight / nativeWidth ;
					}
					else
					{
						// 縦の方が短いので縦を基準とした調整を行う
						screenWidth  = screenHeight * nativeWidth  / nativeHeight ;
					}
				}
				else
				{
					// そのまでの差がなければ表示解像度は実解像度と同じにする
					screenWidth  = nativeWidth ;
					screenHeight = nativeHeight ;
				}
			}
#endif
			//---------------------------------------------------------

			Debug.Log( "[Screen Resolution] W = " + screenWidth + " H = " + screenHeight ) ;

			// 表示解像度を設定する
			Screen.SetResolution( ( int )screenWidth, ( int )screenHeight, fullScreen == true ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed, new RefreshRate(){ numerator = ( uint )frameRate, denominator = 1 } ) ;
#endif
			//----------------------------------------------------------
			// Unity の情報収集を無効化する

			UnityEngine.Analytics.Analytics.enabled				= false ;
			UnityEngine.Analytics.Analytics.deviceStatsEnabled	= false ;
			UnityEngine.Analytics.Analytics.limitUserTracking	= true ;
#if UNITY_2018_3_OR_NEWER
			UnityEngine.Analytics.Analytics.initializeOnStartup = false ;
#endif
			//----------------------------------------------------------
			// MessagePack 用の GeneratedResolver を設定する

#if ENABLE_IL2CPP || ( !UNITY_EDITOR && ( IOS || IPHONE ) ) || CHECK_IL2CPP
			// IL2CPP 用コード
			Debug.Log( "[IL2CPP用の自動生成コードを使用する]" ) ;

			MessagePack.Resolvers.StaticCompositeResolver.Instance.Register
			(
				MessagePack.Resolvers.GeneratedResolver.Instance,
				MessagePack.Unity.UnityResolver.Instance,
				MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver.Instance,
				MessagePack.Resolvers.StandardResolver.Instance
			) ;

			MessagePackSerializerOptions option = MessagePackSerializerOptions.Standard.WithResolver( MessagePack.Resolvers.StaticCompositeResolver.Instance ) ;
			MessagePackSerializer.DefaultOptions = option ;
#else
			// 汎用コード

			// Private アクセスは常に許可
			MessagePackSerializer.DefaultOptions = MessagePack.Resolvers.StandardResolverAllowPrivate.Options ;
#endif
	    }

		//-----------------------------------------------------------------
		
		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
		new protected void Awake()
		{
			// 簡易デバッグログ表示を有効にする
//			DebugScreen.Create( 0xFFFFFFFF, 32, 24 ) ;

			base.Awake() ;

			//----------------------------------------------------------
			// フレームレートを設定する

			// デフォルト
			int frameRate_Rendering = 60 ;
			int frameRate_FixedTime = 60 ;
			Settings.VsyncTypes vsyncType = Settings.VsyncTypes.Invalid ;

			Settings settings =  LoadSettings() ;
			if( settings != null )
			{
				frameRate_Rendering = settings.FrameRate_Rendering ;
				frameRate_FixedTime = settings.FrameRate_FixedTime ;
				vsyncType			= settings.VsyncType ;
			}

			SetFrameRate( frameRate_Rendering, frameRate_FixedTime, vsyncType ) ;

			//----------------------------------------------------------
			// ファサードクラス群を追加する

			SetFacadeClasses() ;

			//----------------------------------------------------------
		}
	
		/// <summary>
		/// フレームレートを設定する
		/// </summary>
		/// <param name="rendering"></param>
		/// <param name="fixedTime"></param>
		public static void SetFrameRate( int rendering, int fixedTime, Settings.VsyncTypes vsyncType )
		{
			// 描画フレームレートの設定
			Application.targetFrameRate = rendering ;

			// 処理フレームレートの設定
			Time.fixedDeltaTime = 1.0f / ( float )fixedTime ;

			// 垂直同期の設定
			QualitySettings.vSyncCount = ( int )vsyncType ;

			Debug.Log( "<color=#FFFF00>描画フレームレート:" + Application.targetFrameRate + " 処理フレームレート:" + fixedTime + " 垂直同期:" + vsyncType + "</color>" ) ;
			DebugScreen.Out( "<color=#FFFF00>描画フレームレート:" + Application.targetFrameRate + " 処理フレームレート:" + fixedTime + " 垂直同期:" + vsyncType + "</color>" ) ;
		}

		/// <summary>
		/// ファサードクラス群を追加する
		/// </summary>
		private void SetFacadeClasses()
		{
			//----------------------------------------------------------
			// ファサードクラスを GameObject の生存期間に同期させる(UniTaskをコロスため)

			var facade = new GameObject( "Facade Classes" ) ;
			facade.transform.SetParent( transform, false ) ;
			facade.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			facade.transform.localScale		= Vector3.one ;

			facade.AddComponent<Preference>() ;

			facade.AddComponent<Scene>() ;
			facade.AddComponent<Asset>() ;

			facade.AddComponent<BGM>() ;
			facade.AddComponent<SE>() ;

			facade.AddComponent<Zip>() ;
			facade.AddComponent<GZip>() ;

			facade.AddComponent<WebView>() ;
		}

		//-----------------------------------------------------------------
	
		// Start は async void が正しい
		internal async void Start()
		{
			Prepare().Forget() ;
			await Yield() ;
		}

		// 各種準備処理を行う
		private async UniTask Prepare()
		{
			// ApplicationManager のセットアップに時間がかかるためプログレスだけ最優先でロードして表示する

			//----------------------------------------------------------

			// オーディオ関連のマネージャを生成する(Listenerが1つも無いとログで警告されるため)
			await CreateAudioManagers() ;

			// 常駐型のサブシーンをロードする
			await LoadResidentSubScenes() ;

			//------------------------------------------------------------------
			// プラットフォームタイプを読み出す
			Settings settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_PlatformType = settings.SelectedPlatformType ;

				CanvasWidth  = settings.BasicWidth ;
				CanvasHeight = settings.BasicHeight ;

				// RunInBackground や ServerName は、いずれ Settings で設定できるようにする
//				m_RunInBackground = true ;
			}

			//---------------------------------------------------------
			// バックグラウンド動作設定(ＵｎｉｔｙＥｄｉｔｏｒのみ / Ｕｎｔｙには実機環境で値を書き込んだら落ちるバージョンが存在する)
#if UNITY_EDITOR
			Application.runInBackground = RunInBackground ;
#endif
//			Debug.LogWarning( "runInBackground:" + Application.runInBackground ) ;
			//----------------------------------------------------------

			// 他のマネージャ系が生成されていなければ生成する

			// マスターデータマネージャのゲームオブジェクトを生成する
			if( MasterDataManager.Instance == null )
			{
				MasterDataManager.Create( transform ) ;
			}

			// プレイヤーデータマネージャのゲームオブジェクトを生成する
			if( PlayerDataManager.Instance == null )
			{
				PlayerDataManager.Create( transform ) ;
			}

			//----------------------------------------------------------
			// このタイミングのエンドポイント設定は保険の意味合い程度で実際はブートで上書きされる

			if( Define.DevelopmentMode == true )
			{
				// デバッグ機能のエンドポイントが設定されていたら使用する
				string endPointKey = "EndPoint" ;
				if( Preference.HasKey( endPointKey ) == true )
				{
					m_EndPoint = Preference.GetValue<string>( endPointKey ) ;
				}
			}

			if( ( string.IsNullOrEmpty( m_EndPoint ) == true || m_EndPoint == "http://localhost/" ) && settings != null )
			{
				var endPoints = settings.EndPoints ;
				int endPointIndex = ( int )settings.EndPoint ;
				if( endPoints != null && endPoints.Length >  0 && endPointIndex >= 0 && endPointIndex <  endPoints.Length )
				{
					m_EndPoint = endPoints[ endPointIndex ].Path ;	// デフォルトは開発サーバー
				}
			}

			// WebAPI マネージャを生成する
			if( WebAPIManager.Instance == null )
			{
				WebAPIManager.Create( transform ) ;
				if( string.IsNullOrEmpty( m_EndPoint ) == false )
				{
//					PlayerData.EndPoint = m_EndPoint ;	// 後で PlayerData の構造を変更したら対応する
					WebAPIManager.EndPoint = m_EndPoint ;	//	= PlayerData.EndPoint ;	// 後で PlayerData の構造を変更したら対応する
				}
			}

			// ダウンロードマネージャを生成する
			if( DownloadManager.Instance == null )
			{
				DownloadManager.Create( transform ) ;
			}

			//--------------------------------------------------------------------------

			// 汎用シーンマネージャの生成
			if( EnhancedSceneManager.Instance == null )
			{
				EnhancedSceneManager.Create( transform ) ;
			}

			//----------------------------------------------------------

			// 汎用インプットマネージャの生成
			if( InputManager.Instance == null )
			{
				InputManager.Create( transform, true ) ;
				GamePadMapper.Setup() ;
			}

			// 汎用アセットバンドルマネージャの生成
			if( AssetBundleManager.Instance == null )
			{
				AssetBundleManager.Create( transform ) ;
			}

			//----------------------------------------------------------
			// カーソルの制御を設定する

			// カーソルの制御設定

			InputManager.SetInputProcessingType( InputProcessingTypes.Switching ) ;
			InputManager.SetCursorProcessing( true ) ;

			uGUIHelper.InputAdapter.UIEventSystem.SetInputProcessingType( uGUIHelper.InputAdapter.InputProcessingTypes.Parallel ) ;
			uGUIHelper.InputAdapter.UIEventSystem.SetCursorProcessing( false ) ;

			// ゲームパッドが繋がっていたらゲームパッドを初期の入力状態にする
			if( GamePad.NumberOfGamePads >  0 )
			{
				InputManager.SetInputType( InputTypes.GamePad ) ;
			}

			//----------------------------------------------------------
			// プレイヤー情報の初期値を設定しておく

			// ワールドの共通設定をロードする
			WorldSettings.Load() ;

			// シングル
			PlayerData.PlayMode			= PlayerData.PlayModes.Single ;

			string key ;

			key = "PlayerName" ;
			if( Preference.HasKey( key ) == true )
			{
				PlayerData.PlayerName = Preference.GetValue<string>( key ) ;
			}
			else
			{
				PlayerData.PlayerName		= "ステーブ" ;
			}

			key = "ColorType" ;
			if( Preference.HasKey( key ) == true )
			{
				PlayerData.ColorType = Preference.GetValue<byte>( key ) ;
			}
			else
			{
				PlayerData.ColorType		= 0 ;
			}

			// シングル
			PlayerData.ServerAddress	= "localhost" ;

			key = "ServerPortNumber" ;
			if( Preference.HasKey( key ) == true )
			{
				PlayerData.ServerPortNumber = Preference.GetValue<int>( key ) ;
			}
			else
			{
				PlayerData.ServerPortNumber	= 0 ;
			}

			if( PlayerData.ServerPortNumber	== 0 )
			{
				if( settings != null )
				{
					PlayerData.ServerPortNumber	= settings.ServerPortNumber ;
				}
				else
				{
					PlayerData.ServerPortNumber	= 32000 ;
				}
			}

			//----------------------------------------------------------

			// サーバー検知パケットを一定時間ごとに送信する
			ServerDetactor.Create( -1, parent:transform, isSuspending:true ) ;

			//----------------------------------------------------------

			// 初期化が完了した
			m_Initialized = true ;

			//----------------------------------------------------------

#if USE_EXCEPTION_DIALOG
			// 例外ダイアログの設定(実機且つデバッグビルドのみ)
			if( settings.EnableExceptionDialog == true )
			{
				AddExceptionCallback() ;
			}
#endif
		}

#if USE_EXCEPTION_DIALOG

		// 例外発生時のコールバックを追加する
		private void AddExceptionCallback()
		{
			// メインスレッド用のフック
			Application.logMessageReceived -= OnExceptionOccurred ;
			Application.logMessageReceived += OnExceptionOccurred ;

			// サブスレッド用のフック
			Application.logMessageReceivedThreaded -= OnExceptionOccurred ;
			Application.logMessageReceivedThreaded += OnExceptionOccurred ;
		}

		/// <summary>
		/// 例外が発生した際に例外ダイアログを表示する(実機且つデバッグビルドのみ)
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="stackTrace"></param>
		/// <param name="type"></param>
		private void OnExceptionOccurred( string condition, string stackTrace, LogType type )
		{
			if( type == LogType.Exception )
			{
				// 例外情報を追加する
				m_Exceptions.Add( ( condition, stackTrace ) ) ;
			}
		}

		// 例外情報スタック
		private List<( string, string )> m_Exceptions = new List<(string, string)>() ;

		// 例外情報ダイアログが開かれた状態かどうか
		private bool m_IsExceptionDialogOpening = false ;

		// 例外情報ダイアログを開く
		private async UniTask OpenExceptionDialog()
		{
			m_IsExceptionDialogOpening = true ;			// ダイアログが開かれている

			// 全ての例外を表示しきるまで繰り返す
			while( m_Exceptions.Count >  0 )
			{
				var exception = m_Exceptions[ 0 ] ;

				// 例外情報ダイアログを開く
				await Dialog.OpenException( "Exception", exception.Item1 + "\n" + exception.Item2, new string[]{ "CLOSE" } ) ;

				m_Exceptions.RemoveAt( 0 ) ;
			}

			m_IsExceptionDialogOpening = false ;	// ダイアログが閉じられている
		}

#endif
		//-------------------------------------------------------------------------------------------

		internal void Update()
		{
#if USE_EXCEPTION_DIALOG
			// サブスレッドで発生した例外もキャッチして表示したいため例外情報はリストにためてメインスレッドで表示する
			if( m_Exceptions.Count >  0 && m_IsExceptionDialogOpening == false )
			{
				// 例外発生情報ダイアログを開く
				OpenExceptionDialog().Forget() ;
			}
#endif
			//----------------------------------
			// タイマー関係

			float delta = Time.unscaledDeltaTime ;

			if( m_IsMasterTimePausing == false )
			{
				// タイマー増加
				m_MasterTimeDelta = delta ;
				m_MasterTime += delta ;
			}
			else
			{
				m_MasterTimeDelta = 0 ;
			}
		}

		//-----------------------------------------------------------------
	
		/// <summary>
		/// オーディオ関連のマネージャを生成する
		/// </summary>
		/// <returns></returns>
		private async UniTask CreateAudioManagers()
		{
			bool audioRunInBackground ;
#if UNITY_EDITOR
			audioRunInBackground = RunInBackground ;
#else
			audioRunInBackground = false ;	// 実機ではバックグラウンドでは必ず止める
#endif

#if !USE_ADX2
			// 汎用オーディオマネージャの生成
			if( AudioManager.Instance == null )
			{
				AudioManager.Create( transform, true, audioRunInBackground ) ;
				await Yield() ;
			}
#else
			// 汎用オーディオマネージャ(ADX2)の生成
			if( AudioManager_ADX2.Instance == null )
			{
				AudioManager_ADX2.Create( transform, enableListener:true, runInbackground:audioRunInBackground ) ;
				await AudioManager_ADX2.WaitForInitialization() ;
			}

			// ＡＤＸ２のキャッシュ管理マネージャの生成
			if( CueSheetManager.Instance == null )
			{
				CueSheetManager.Create( transform ) ;
			}
#endif
		}

		//-----------------------------------------------------------------
	
		// 常駐型のサブシーンをロードする
		private async UniTask LoadResidentSubScenes()
		{
			//  999
			if( OuterFrame.Instance == null )
			{
				// 外枠のシーンをロードする
				OuterFrame outerFrame = null ;
				await When( EnhancedSceneManager.AddAsync<OuterFrame>( "OuterFrame", ( _ ) => { outerFrame = _[ 0 ] ; } ) ) ;
				if( outerFrame != null )
				{
					DontDestroyOnLoad( outerFrame.gameObject ) ;
				}
			}

			//  990
			if( Blocker.Instance == null )
			{
				// 入力禁止用のシーンをロードする
				Blocker blocker = null ;
				await When( EnhancedSceneManager.AddAsync<Blocker>( "Blocker", ( _ ) => { blocker = _[ 0 ] ; } ) ) ;
				if( blocker != null )
				{
					DontDestroyOnLoad( blocker.gameObject ) ;
//					sceneMask.gameObject.SetActive( false ) ;
					Blocker.On( null, 0xFF000000 ) ;
				}
			}

			//  995
			if( Progress.Instance == null )
			{
				// プログレス演出用のシーンをロードする
				Progress progress = null ;
				await When( EnhancedSceneManager.AddAsync<Progress>( "Progress", ( _ ) => { progress = _[ 0 ] ; } ) ) ;
				if( progress != null )
				{
					DontDestroyOnLoad( progress.gameObject ) ;
					progress.gameObject.SetActive( false ) ;
				}
			}

			//----------------------------------------------------------

			//  940
			if( Toast.Instance == null )
			{
				// トーストのシーンをロードする
				Toast toast = null ;
				await When( EnhancedSceneManager.AddAsync<Toast>( "Toast", ( _ ) => { toast = _[ 0 ] ; } ) ) ;
				if( toast != null )
				{
					DontDestroyOnLoad( toast.gameObject ) ;
					toast.gameObject.SetActive( false ) ;
				}
			}

			//  993
			if( Movie.Instance == null )
			{
				// ムービー再生用のシーンをロードする
				Movie movie = null ;
				await When( EnhancedSceneManager.AddAsync<Movie>( "Movie", ( _ ) =>{ movie = _[ 0 ] ; } ) ) ;
				if( movie != null )
				{
					DontDestroyOnLoad( movie.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
					movie.gameObject.SetActive( false ) ;
				}
			}

			//  994
			if( Fade.Instance == null )
			{
				// フェード演出用のシーンをロードする
				Fade fade = null ;
				await When( EnhancedSceneManager.AddAsync<Fade>( "Fade", ( _ ) => { fade = _[ 0 ] ; } ) ) ;
				if( fade != null )
				{
					DontDestroyOnLoad( fade.gameObject ) ;
					fade.gameObject.SetActive( false ) ;
				}
			}

			//  996
			if( Dialog.Instance == null )
			{
				// ダイアログ用のシーンをロードする
				Dialog dialog = null ;
				await When( EnhancedSceneManager.AddAsync<Dialog>( "Dialog", ( _ ) =>{ dialog = _[ 0 ] ; } ) ) ;
				if( dialog != null )
				{
					DontDestroyOnLoad( dialog.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。
					dialog.gameObject.SetActive( false ) ;
				}
			}
			
			//  998
			if( Ripple.Instance == null )
			{
				// タッチ演出用のシーンをロードする
				Ripple ripple = null ;
				await When( EnhancedSceneManager.AddAsync<Ripple>( "Ripple", ( _ ) => { ripple = _[ 0 ] ; } ) ) ;
				if( ripple != null )
				{
					DontDestroyOnLoad( ripple.gameObject ) ;
				}
			}
			
			//--------------------------------------------------------------------------

			if( Blocker.Instance != null )
			{
				Blocker.Off() ;
			}
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// アプリケーション全体の仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public static bool SetResolution( float width, float height )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			// 確認用
			m_Instance.CanvasWidth  = width ;
			m_Instance.CanvasHeight = height ;

			   Blocker.SetCanvasResolution( width, height ) ;
			     Movie.SetCanvasResolution( width, height ) ;
			    Dialog.SetCanvasResolution( width, height ) ;
			      Fade.SetCanvasResolution( width, height ) ;
			  Progress.SetCanvasResolution( width, height ) ;
			    Ripple.SetCanvasResolution( width, height ) ;
			OuterFrame.SetCanvasResolution( width, height ) ;
//			   Profile.SetCanvasResolution( width, height ) ;

			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// 設定をロードする
		/// </summary>
		public static Settings LoadSettings()
		{
			return Resources.Load<Settings>( "ScriptableObjects/Settings" ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 常駐するタイプのアセットバンドル(シーンが変わっても維持)のバス群
		private readonly static string[] m_ResidentAssetBundlePaths =
		{
//			"External|Scenes",
//			"External|ReferencedAssets",
//			"Shaders",
		} ;

		/// <summary>
		/// アセットバンドルに追い出したシーン関連のアセットバンドルを展開する
		/// </summary>
		public static void LoadResidentAssetBundle()
		{
//			Debug.Log( "[Load Resident AssetBundle]" ) ;
//
//			foreach( var path in m_ResidentAssetBundlePaths )
//			{
//				if( Asset.LoadAssetBundle( path, true ) == false )
//				{
//					Debug.LogError( "Could not load AssetBundle : " + path ) ;
//				}
//			}
		}

		/// <summary>
		/// アセットバンドルに追い出したシーン関連のアセットバンドルを破棄する
		/// </summary>
		public static void FreeAssetBundleOfScenes()
		{
//			Debug.Log( "[Free Resident AssetBundle]" ) ;
//
//			// シーン関連のアセットバンドルが展開中であれば破棄する
//			foreach( var path in m_ResidentAssetBundlePaths )
//			{
//				Asset.FreeAssetBundle( path ) ;
//			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// リブートを実行する
		/// </summary>
		public static void Reboot()
		{
			// BGMが再生されている可能性があるので止める
			BGM.StopMain( 0.5f ) ;

			// 常駐型のチートパネルが開いている可能性があるので強制的に閉じる
//			Cheat.Hide() ;

			//----------------------------------

			// アセットバンドルに追い出したシーン関連のアセットバンドルを破棄する
			FreeAssetBundleOfScenes() ;

			AssetBundleManager.ClearResourceCache( true, true ) ;

			//----------------------------------

			// ブロッカーを無効化する
			Blocker.Off() ;

			// リブート対象シーンをロージする
			Scene.LoadWithFade( Scene.Screen.Reboot ).Forget() ;

			// タスクをまとめてキャンセルする(メモ)
//			throw new OperationCanceledException() ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// 外部ブラウザを開く
		/// </summary>
		/// <param name="url"></param>
		public static void OpenURL( string url )
		{
			Application.OpenURL( url ) ;
		}

		/// <summary>
		/// メーラーを起動する
		/// </summary>
		/// <param name="address"></param>
		/// <param name="title"></param>
		/// <param name="message"></param>
		public static void OpenMailer( string address, string title, string message )
		{
			if( string.IsNullOrEmpty( title ) == false )
			{
				title		= Uri.EscapeDataString( title	) ;
			}

			if( string.IsNullOrEmpty( message ) == false )
			{
				message	= Uri.EscapeDataString( message	) ;
			}

			Application.OpenURL( "mailto:" + address + "?subject=" + title + "&body=" + message ) ;
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

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// LocalAssets が存在するか確認する
		/// </summary>
		/// <returns></returns>
		public static bool HasLocalAssets
		{
			get
			{
#if !UNITY_EDITOR
				return false ;
#else
				string path = "Assets/Application/AssetBundle/list_remote.txt" ;
				return File.Exists( path ) ;
#endif
			}
		}

		/// <summary>
		/// LocalAssets が存在するか確認する
		/// </summary>
		/// <returns></returns>
		public static bool HasLocalAssetBundle
		{
			get
			{
#if !UNITY_EDITOR
				return false ;
#else
				string path = "AssetBundle/" + Define.AssetBundlePlatformName ;
				return Directory.Exists( path ) ;
#endif
			}
		}

		/// <summary>
		/// StreamingAssets にアセットバンドルが存在するか確認する
		/// </summary>
		/// <param name="path"></param>
		/// <param name="onResult"></param>
		/// <returns></returns>
		public static AsyncState HasAssetBundleInStreamingAssets( string path, Action<bool> onResult )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			var state = new AsyncState( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.HasAssetBundleInStreamingAssets_Private( path, onResult, state ) ) ;
			return state ;
		}

		private IEnumerator HasAssetBundleInStreamingAssets_Private( string path, Action<bool> onResult, AsyncState state )
		{
			bool hasAssetBundle = false ;
			yield return StartCoroutine( StorageAccessor.ExistsInStreamingAssetsAsync( path, ( bool result ) =>
			{
				hasAssetBundle = result ;
			} ) ) ;

			onResult?.Invoke( hasAssetBundle ) ;

			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------
		// ストレージの空き容量チェック

		/// <summary>
		/// ストレージの空き容量をチェックして足りなければ警告ダイアログを出す
		/// </summary>
		/// <param name="needSize"></param>
		/// <returns></returns>
		public static async UniTask<bool> CheckStorage( long requiredSize )
		{
			long reserveSize = 10 * 1024 * 1024 ;	// 予備に 10MB

			// 空き容量を確認する(単位はMB)
			long freeSize = StorageMonitor.GetFree() ;
			long needSize = requiredSize + reserveSize ;	// 予備に 30MB 確保(ローカルへの情報保存分)

			if( freeSize <  needSize )
			{
				string sizeName = ExString.GetSizeName( needSize ) ;

				await Dialog.Open( "注意", "ストレージの空き容量が足りません\nあと <color=#FF7F00>" + sizeName + "</color> 必要です\nストレージの空き容量を確保してから\nアプリを起動するようお願い致します", new string[]{ "閉じる" } ) ;
				return false ;
			}

			// 空き容量は足りている
			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// タイマー系

		// ゲーム全体の時間経過
		private double	m_MasterTime ;

		// ゲーム全体の時間経過(差分)
		private double	m_MasterTimeDelta ;

		// マスタータイムがボーズ中かどうか
		private bool	m_IsMasterTimePausing ;

		//-----------------------------------

		/// <summary>
		/// マスタータイム
		/// </summary>
		public static double MasterTime
		{
			get
			{
				if( m_Instance == null )
				{
					// まだ準備が出来ていない
					return 0 ;
				}
				return m_Instance.m_MasterTime ;
			}
		}

		/// <summary>
		/// マスタータイム
		/// </summary>
		public static double MasterTimeDelta
		{
			get
			{
				if( m_Instance == null )
				{
					// まだ準備が出来ていない
					return 0 ;
				}
				return m_Instance.m_MasterTimeDelta ;
			}
		}

		/// <summary>
		/// マスタータイムを一時停止させる
		/// </summary>
		public static void PauseTime()
		{
			if( m_Instance == null )
			{
				return ;
			}
			
			m_Instance.m_IsMasterTimePausing = true ;
		}

		/// <summary>
		/// マスタータイムの一時停止を解除する
		/// </summary>
		public static void UnpauseTime()
		{
			if( m_Instance == null )
			{
				return ;
			}
			
			m_Instance.m_IsMasterTimePausing = false ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セーフエリアを取得する
		/// </summary>
		/// <returns></returns>
		public static Rect GetSafeArea()
		{
			Rect safeArea = Screen.safeArea ;

#if SAFE_AREA_EXSAMPLE
			if( Application.isPlaying == true )
			{
				// SafeAreaCorrector の生成・削除が行われシーンファイルが更新扱いにならないようにするため
				// ランタイム実行時のみ試験的セーフエリアは有効とする

				// UnityEditor - GameView では解像度(Screen .width .height)が可変であるためセーフエリアは解像度に対する比率で考える事
				float safeArea_L = Screen.width  *   0 / 540 ;	// セーフエリア(左)[仮] / 画面解像度(横)[仮]
				float safeArea_R = Screen.width  *   0 / 540 ;	// セーフエリア(右)[仮] / 画面解像度(横)[仮]
				float safeArea_B = Screen.height *  80 / 960 ;	// セーフエリア(下)[仮] / 画面解像度(縦)[仮]
				float safeArea_T = Screen.height *  40 / 960 ;	// セーフエリア(上)[仮] / 画面解像度(縦)[仮]

				// セーフエリアの大きさ単位は画面(Screen)解像度単位
				safeArea = new Rect
				(
					safeArea_L,
					safeArea_B,
					Screen.width  - safeArea_L - safeArea_R,
					Screen.height - safeArea_B - safeArea_T
				) ;
			}
#endif
			return safeArea ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アプリケーションを終了する
		/// </summary>
		public static void Quit()
		{
#if !UNITY_EDITOR
			Application.Quit() ;
#else
			EditorApplication.isPlaying = false ;
#endif
		}
	}
}
