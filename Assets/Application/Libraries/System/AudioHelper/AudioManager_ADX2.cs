//#define USE_CRI_ADX2

#if USE_CRI_ADX2

//#define USE_MICROPHONE
#define UseCPK
#define UseEncrypt

using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using CriWare ;

using UnityEngine ;
using UnityEngine.SceneManagement ;

#if UNITY_EDITOR
using UnityEditor ;
#endif



// シングルトンはエディタモードに対応させてはダメ
// なぜならシングルトンを判定するスタティックなインスタンス変数がシリアライズ化出来ないため


/// <summary>
/// オーディオヘルパーパッケージ
/// </summary>
namespace AudioHelper
{
	/// <summary>
	/// オーディオ全般の管理クラス Version 2025/11/02 0
	/// </summary>
	public class AudioManager_ADX2 : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AudioManager_ADX2 を生成
		/// </summary>
		[MenuItem( "GameObject/Helper/AudioHelper/AudioManager_ADX2", false, 24 )]
		public static void CreateAudioManager()
		{
			var go = new GameObject( "AudioManager_ADX2" ) ;

			Transform t = go.transform ;
			t.SetParent( null ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			go.AddComponent<AudioManager_ADX2>() ;
			Selection.activeGameObject = go ;
		}
#endif

		// オーディオマネージャのインスタンス(シングルトン)
		private static AudioManager_ADX2 m_Instance = null ;

		/// <summary>
		/// オーディオマネージャのインスタンス
		/// </summary>
		public  static AudioManager_ADX2   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//---------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public Request( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && string.IsNullOrEmpty( Error ) == true && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool		IsDone = false ;

			//----------------------------------------------------------

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error = string.Empty ;
		}

		//---------------------------------------------------------

		// リスナー（Awake 時にクリアされるのでシリアライズ化して保持する意味が無い）
		private	CriAtomListener m_Listener = null ;

		/// <summary>
		/// オーディオリスナーのインスタンス
		/// </summary>
		public	CriAtomListener   Listener{ get{ return m_Listener ; } }

		/// <summary>
		/// チャンネル情報
		/// </summary>
		public	List<AudioChannel_ADX2> Channels = new () ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public	int Max = 32 ;

		//-----------------------------------------------------------

		private CriAtomSource m_CurSettingSource = null ;

		//-----------------------------------------------------------

#if UNITY_EDITOR
		// 残っているキューシートの名前をモニタリングする
		[SerializeField]
		protected List<string>	m_CueSheetNames ;
#endif

		//-----------------------------------------------------------

		// 発音毎に割り振られるユニークな識別子
		private int m_PlayId = 0 ;

		// １増加した発音毎に割り振られるユニークな識別子
		private int GetPlayId()
		{
			m_PlayId = ( m_PlayId + 1 ) & 0x7FFFFFFF ;
			return m_PlayId ;
		}

		//---------------

		/// <summary>
		/// タグ情報
		/// </summary>
		public class TagData
		{
			public float	BaseVolume ;
			public int		MaxChannels ;
		}

		// タグ情報群
		private readonly Dictionary<string, TagData> m_Tags = new () ;

		// ベースボリュームを追加する
		private void AddBaseVolume_Private( string tag, float baseVolume )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				m_Tags.Add( tag, new TagData(){ BaseVolume = baseVolume, MaxChannels = 0 } ) ;
			}
			else
			{
				m_Tags[ tag ].BaseVolume = baseVolume ;
			}
		}

		// ベースボリュームを取得する
		private float GetBaseVolume_Private( string tag )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return 1.0f ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				return 1.0f ;
			}
			else
			{
				return m_Tags[ tag ].BaseVolume ;
			}
		}

		// 最大チャンネル数を追加する
		private void AddMaxChannels_Private( string tag, int maxChannels )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				m_Tags.Add( tag, new TagData(){ BaseVolume = 1, MaxChannels = maxChannels } ) ;
			}
			else
			{
				m_Tags[ tag ].MaxChannels = maxChannels ;
			}
		}

		// タグごとの最大チャンネル数を取得する
		private int GetMaxChannels_Private( string tag )
		{
			if( string.IsNullOrEmpty( tag ) == true )
			{
				return 0 ;
			}

			if( m_Tags.ContainsKey( tag ) == false )
			{
				return 0 ;
			}
			else
			{
				return m_Tags[ tag ].MaxChannels ;
			}
		}

		//---------------

		/// <summary>
		/// スタートフェードとストップフェードのエフェクト
		/// </summary>
		internal protected class FadeEffect
		{
			/// <summary>
			/// フェード時間
			/// </summary>
			internal protected float Duration ;

			/// <summary>
			/// 基準時間
			/// </summary>
			internal protected float StartTime ;

			/// <summary>
			/// 開始音量
			/// </summary>
			internal protected float StartVolume ;

			/// <summary>
			/// 終了音量
			/// </summary>
			internal protected float EndVolume ;
		}


		// プレイフェード用
		private readonly Dictionary<AudioChannel_ADX2,FadeEffect> m_FadePlayChannels = new () ;
		private readonly Dictionary<AudioChannel_ADX2,FadeEffect> m_FadeStopChannels = new () ;

		/// <summary>
		/// マスターボリューム
		/// </summary>
		public float MasterVolume = 1.0f ;

		// ミュート
		private bool m_Mute = false ;

		// ミュートリスト
		private readonly List<string> m_MuteChannels = new () ;

		// バックグラウンド中の処理(時間経過)を有効にするかどうか
		private bool		m_RunInBackground = false ;

		// バックグラウンド中の発音(無音再生)を有効にするかどうか
		private bool		m_MuteInBackground = false ;

		/// <summary>
		/// バックグラウンド再生を有効にするかどうか
		/// </summary>
		public	static bool		  RunInBackground
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_RunInBackground ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_RunInBackground = value ;
			}
		}

#if UNITY_EDITOR
		// サスペンド中かどうか
		private bool        m_IsSuspending = false ;
#endif
		/// <summary>
		/// リスナーを有効にするかどうか
		/// </summary>
		private bool		m_EnableListaner = true ;

		/// <summary>
		/// 低遅延モードを有効にするか(Android限定)
		/// </summary>
		private bool		m_EnableLowLatency = false ;

		/// <summary>
		/// プレビューモードを有効化するかどうか
		/// </summary>
		private bool        m_PreviewModeEnabled = false ;

		/// <summary>
		/// バイノーラライザーを有効にするかどうか
		/// </summary>
		private bool        m_BinauralizerEnabled = false ;

		/// <summary>
		/// 使用中のバインダーの数
		/// </summary>
		[SerializeField]
		protected int		m_NumberOfBinders = 0 ;

		/// <summary>
		/// バインド可能最大数
		/// </summary>
		[SerializeField]
		protected int		m_MaxNumberOfBinders ;

		/// <summary>
		/// 暗号化キー
		/// </summary>
		[SerializeField]
		protected string	m_EncryptKey = "" ;

		/// <summary>
		/// Atom の復号化を有効にするか
		/// </summary>
		[SerializeField]
		protected bool		m_AtomDecryptEnabled = true ;

		/// <summary>
		/// Mana の復号化を有効にするか
		/// </summary>
		[SerializeField]
		protected bool		m_ManaDecryptEnabled = true ;


		//-----------------------------------------------------

		// Cri ADX2

		private CriWareInitializer	m_CriWareInitializer ;
		private CriWareErrorHandler	m_CriWareErrorHandler ;


		[Serializable]
		public class CueSheetCache
		{
			public string	Name ;
			public bool		Keep ;
			public int		Count ;
			public bool		IsGarbage ;
			public bool		IsStreaming ;

			public CriFsBinder	Binder ;
			public uint			BindId ;

			private readonly bool				m_UseBind ;
			private readonly AudioManager_ADX2	m_Owner ;


			public CueSheetCache( string name, bool keep, bool isStreaming, bool useBind, AudioManager_ADX2 owner )
			{
				Name		= name ;
				Keep		= keep ;
				Count		= 0 ;
				IsGarbage	= false ;
				IsStreaming	= isStreaming ;

				m_UseBind	= useBind ;
				m_Owner		= owner ;

				if( useBind == true )
				{
					// 使用中のバインダーの数を増加させる
					m_Owner.m_NumberOfBinders ++ ;
				}
			}

			public void Terminate()
			{
				if( m_UseBind == true )
				{
					// 使用中のバインダーの数を減少させる
					m_Owner.m_NumberOfBinders -- ;
				}
#if UseCPK
				if( Binder != null )
				{
					CriFsBinder.Unbind( BindId ) ;
					BindId = 0 ;

					Binder.Dispose() ;
					Binder = null ;
				}
#endif
			}
		}

		[SerializeField][NonSerialized]
		private List<CueSheetCache>	m_CueSheetCache ;

		// アクセス高速化のためのハッシュ
		private Dictionary<string,CueSheetCache>	m_CueSheetCache_Hash ;

		// 登録処理中のキューシート名
		private readonly List<string>	m_ProcessingCueSheedNames = new () ;

		// 初期化済みかの判定用
		private bool m_IsInitialized ;

		//-----------------------------------------------------

#if USE_MICROPHONE
		// マイク関係
		private AudioSource			m_MicrophoneRecordingAudioSource	= null ;
		private AudioClip			m_MicrophoneRecordingAudioClip		= null ;
		private	int					m_MicrophoneRecordingFrequency		= 44100 ;
		private int					m_MicrophoneRecordingTime			= 1 ;
		private float[]				m_MicrophoneRecordingBuffer			= null ;
		private int					m_MicrophoneRecordingPosition		= 0 ;
		private Action<float[],int>	m_MicrophoneCallback				= null ;
		private bool				m_MicrophoneCallbackDataSkip		= false ;
		private float[]				m_MicrophoneCallbackBuffer			= null ;
#endif

		//-----------------------------------------------------

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		public delegate void OnStopped( int playId ) ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲート
		/// </summary>
		public OnStopped OnStoppedDelegate ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onStoppedDelegate">追加するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool AddOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate += onStoppedDelegate ;

			return true ;
		}

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onStoppedDelegate">削除するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveOnStopped( OnStopped onStoppedDelegate )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnStoppedDelegate -= onStoppedDelegate ;

			return true ;
		}

		//---------------------------------------------------------

		/// <summary>
		/// オーディオマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="runInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>オーディオマネージャのインスタンス</returns>
		public static AudioManager_ADX2 Create
		(
			Transform parent = null,
			int maxNumberOfBinders = 16,
			string encryptKey = "",
			bool atomDecryptEnabled = true,
			bool manaDecryptEnabled = true,
			bool enableListener = true,
			bool runInBackground = false,
			bool muteInBackground = true,
			bool enableLowLatency = false,
			bool previewModeEnabled = false,
			bool binauralizerEnabled = false
		)
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			//-------------------------
#if UNITY_EDITOR
			// Unity Audio Diable を有効化する 
			var path        = "ProjectSettings/AudioManager.asset" ;
			var assets = AssetDatabase.LoadAllAssetsAtPath( path ) ;
			if( assets != null && assets.Length >  0 )
			{
				var manager     = assets.FirstOrDefault() ;
				if( manager != null )
				{
					var so          = new SerializedObject( manager ) ;
					if( so != null )
					{
						var property    = so.FindProperty( "m_DisableAudio" ) ;
						if( property != null )
						{
							property.boolValue = true ;
							so.ApplyModifiedProperties() ;
						}
					}
				}
			}
#endif
			//-------------------------
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindAnyObjectByType( typeof( AudioManager_ADX2 ) ) as AudioManager_ADX2 ;
			if( m_Instance == null )
			{
				var go = new GameObject( "AudioManager_ADX2" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<AudioManager_ADX2>() ;
			}

			m_Instance.m_MaxNumberOfBinders	    = maxNumberOfBinders ;
			m_Instance.m_EncryptKey			    = encryptKey ;
			m_Instance.m_AtomDecryptEnabled	    = atomDecryptEnabled ;
			m_Instance.m_ManaDecryptEnabled	    = manaDecryptEnabled ;

			m_Instance.m_RunInBackground	    = runInBackground ;
			m_Instance.m_MuteInBackground	    = muteInBackground ;
			m_Instance.m_EnableListaner		    = enableListener ;
			m_Instance.m_EnableLowLatency	    = enableLowLatency ;

			m_Instance.m_PreviewModeEnabled     = previewModeEnabled ;
			m_Instance.m_BinauralizerEnabled    = binauralizerEnabled ;

			return m_Instance ;
		}

		/// <summary>
		/// オーディオマネージャのインスタンスを破棄する
		/// </summary>
		public static void Delete()
		{
			if( m_Instance != null )
			{
				ClearAll() ;

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

			var instanceOther = GameObject.FindAnyObjectByType( typeof( AudioManager_ADX2 ) ) as AudioManager_ADX2 ;
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
			gameObject.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			gameObject.transform.localScale = Vector3.one ;

			//-----------------------------

			m_CueSheetCache			= new List<CueSheetCache>() ;
			m_CueSheetCache_Hash	= new Dictionary<string, CueSheetCache>() ;

			//-----------------------------

			// チャンネルリストをクリアする
			Channels.Clear() ;

			// ミュートリストをクリアする
			m_MuteChannels.Clear() ;

			// フェードプレイリストをクリアする
			m_FadePlayChannels.Clear() ;

			// フェードストップリストをクリアする
			m_FadeStopChannels.Clear() ;

			//----------------------------------------------------------

			AudioListener standardListener = GameObject.FindAnyObjectByType( typeof( AudioListener ) ) as AudioListener ;
			if( standardListener == null )
			{
				// 標準のリスナーも１つ付けておく
				gameObject.AddComponent<AudioListener>() ;
			}
		}

		internal IEnumerator Start()
		{
			// バイノーラライザー有効化(Initialize を実行する前に実行する必要がありそうな気配)
			CriAtomExAsr.EnableBinauralizer( m_BinauralizerEnabled ) ; 

			//----------------------------------------------------------
			// ADX2 固有の初期化

			// 確実にCriWareInitializer.Initialize()を他のコンポーネントより先に実行するためStart()で一連のADX2の一連のセットアップ処理を行う

			// 最初にCriWareInitializer.Initialize()を実行する必要がある
			var initializer = new GameObject( "Initializer" ) ;
			initializer.transform.SetParent( transform, false ) ;
			initializer.SetActive( false ) ;	// 煩雑だがAwake()内の処理を一時的に凍結するために仕方がない処理

			//----------------------------------
			// 共通

			m_CriWareInitializer	= initializer.AddComponent<CriWareInitializer>() ;
			m_CriWareInitializer.dontDestroyOnLoad = ( initializer.transform.parent == null ) ;

			// プレビューモードの有効化
			m_CriWareInitializer.atomConfig.inGamePreviewMode =
				m_PreviewModeEnabled == true ?
					CriAtomConfig.InGamePreviewSwitchMode.Enable :
					CriAtomConfig.InGamePreviewSwitchMode.Default ;

			m_CriWareInitializer.fileSystemConfig.numberOfLoaders		= m_MaxNumberOfBinders ;
			m_CriWareInitializer.fileSystemConfig.numberOfBinders		= m_MaxNumberOfBinders ;	// バインド可能数をデフォルトの８から１６に増強

			// 同時発音数関係
			m_CriWareInitializer.atomConfig.maxVirtualVoices                        = 40 ;  // Default : 32
			m_CriWareInitializer.atomConfig.standardVoicePoolConfig.memoryVoices    = 24 ;  // Default : 16
			m_CriWareInitializer.atomConfig.standardVoicePoolConfig.streamingVoices =  8 ;  // Default :  8

#if UseEncrypt
			m_CriWareInitializer.DecrypterConfig.key					= m_EncryptKey ;			// 暗号化キー
			if( string.IsNullOrEmpty( m_EncryptKey ) == false )
			{
				m_CriWareInitializer.useDecrypter = true ;
			}

			m_CriWareInitializer.DecrypterConfig.enableAtomDecryption = m_AtomDecryptEnabled ;
			m_CriWareInitializer.DecrypterConfig.enableManaDecryption = m_ManaDecryptEnabled ;
#endif

			//----------------------------------

			// iOS 限定
			m_CriWareInitializer.atomConfig.iosBufferingTime			=  50 ;	// 何秒分バッファリングするか(ミリ秒)
			m_CriWareInitializer.atomConfig.iosOverrideIPodMusic		= false ;

			// Android 限定
			m_CriWareInitializer.atomConfig.androidBufferingTime		= 120 ;	// 何秒分バッファリングするか(ミリ秒)
			m_CriWareInitializer.atomConfig.androidStartBufferingTime	=  80 ;	// 何秒分バッファリングされたところで再生を開始するか(ミリ秒)
			m_CriWareInitializer.atomConfig.androidLowLatencyStandardVoicePoolConfig.memoryVoices = m_EnableLowLatency ? 3 : 0 ;	// 効果音系の遅延を軽減する
			m_CriWareInitializer.atomConfig.androidUsesAndroidFastMixer	= true ;

			initializer.SetActive( true ) ;

			// ErrorHandlerを生成する
			var errorHandler = new GameObject( "ErrorHandler" ) ;
			errorHandler.transform.SetParent( transform, false ) ;
			errorHandler.SetActive( false ) ;	// 煩雑だがAwake()内の処理を一時的に凍結するために仕方がない処理

			m_CriWareErrorHandler	= errorHandler.AddComponent<CriWareErrorHandler>() ;
			m_CriWareErrorHandler.dontDestroyOnLoad = ( errorHandler.transform.parent == null ) ;

			errorHandler.SetActive( true ) ;

			// Atomを生成する
			var atom = new GameObject( "Atom" ) ;
			atom.transform.SetParent( transform, false ) ;
			atom.AddComponent<CriAtom>() ;

			// CriAtom を生成すると CriAtomServer も自動で生成されているはずなのでシーン遷移で消されないように親を変更する
			CriAtomServer server = GameObject.FindAnyObjectByType( typeof( CriAtomServer ) ) as CriAtomServer ;
			if( server != null )
			{
				server.gameObject.transform.SetParent( transform, false ) ;
				server.name = "Server" ;
			}

			// リスナーを張り付けておく
			CreateAudioListener( m_EnableListaner ) ;

			// 使用中のバインダー数を減少
			m_NumberOfBinders = 0 ;

			//----------------------------------------------------------
#if false
			// 機種の遅延量を測定する
			CriAtomExLatencyEstimator.InitializeModule() ;

			while( CriAtomExLatencyEstimator.GetCurrentInfo().status == CriAtomExLatencyEstimator.Status.Processing )
			{
				yield return null ;
			}

			CriAtomExLatencyEstimator.EstimatorInfo info = CriAtomExLatencyEstimator.GetCurrentInfo() ;

			int latency = -1 ;

			if( info.status == CriAtomExLatencyEstimator.Status.Done )
			{
				latency = ( int )info.estimated_latency ;
			}

			CriAtomExLatencyEstimator.FinalizeModule() ;
#endif
			//----------------------------------------------------------

			// シーンがロードされた際に呼び出されるデリゲートを登録する
			SceneManager.activeSceneChanged += OnActiveSceneChanged ;

			// 初期化完了
			m_IsInitialized = true ;

			//-------------------------------------------------

			yield break ;
		}

		// オーディオリスナーを生成する
		private bool CreateAudioListener( bool isEnabled )
		{
			if( m_Listener != null )
			{
				m_Listener.enabled = isEnabled ;
				return true ;	// 既に自身のオーディオリスナーを生成済みになっている
			}

			CriAtomListener listener = GameObject.FindAnyObjectByType( typeof( CriAtomListener ) ) as CriAtomListener ;

			var go = new GameObject( "Listener" ) ;
			go.transform.SetParent( transform, false ) ;

			// 存在しない(自身のオーディオリスナーを生成する)
			m_Listener = go.AddComponent<CriAtomListener>() ;

			if( listener == null )
			{
				// 存在しない

				m_Listener.enabled = isEnabled ;	// 有効化しておく
			}
			else
			{
				// 存在する
#if UNITY_EDITOR
				Debug.LogWarning( "[AudioManager_ADX] 既に CriAtomListener が存在します" ) ;
#endif
				m_Listener.enabled = false ;	// 無効化しておく
			}

			//----------------------------------

			return true ;
		}

		// 何等かのシーンがロードされた際に呼び出される
		private void OnActiveSceneChanged( Scene fromScene, Scene toScene )
		{
			bool isEnabled = true ;
			if( m_Listener != null )
			{
				isEnabled = m_Listener.enabled ;
			}

			CreateAudioListener( isEnabled ) ;

			//----------------------------------

			// 不要になったキューシートを破棄する
			RemoveAllCueSheets_Private( false ) ;
		}

		/// <summary>
		/// 初期化が完了するのを待つ
		/// </summary>
		/// <returns></returns>
		public static Request WaitForInitialization()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.WaitForInitialization_Private( request ) ) ;
			return request ;
		}

		private IEnumerator WaitForInitialization_Private( Request request )
		{
			yield return new WaitWhile( () => ( m_IsInitialized == false ) ) ;

			request.IsDone = true ;
		}

		internal void Update()
		{
			if( m_IsInitialized == false )
			{
				return ;	// 初期化が完了していない
			}

			//-------------------------

#if UNITY_EDITOR
			if( m_IsSuspending == false )
			{
				// GameView の ミュート設定を反映させる
				CriAtomExAsr.SetBusVolume( "MasterOut", EditorUtility.audioMasterMute ? 0f : AudioListener.volume * MasterVolume ) ;
			}
#endif
			//-------------------------

			// ソースを監視して停止している（予定）

			// 一定時間ごとにコールされる
			Process() ;

#if USE_MICROPHONE
			// マイクの録音
			ProcessMicrophoneRecording() ;
#endif
			// シーン切替時に破棄できなかった破棄対象のキューシートを破棄する
			RemoveAllCueSheetsAtFrame_Private() ;
		}

		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				// シーンがロードされた際に呼び出されるデリゲートを削除する
				SceneManager.activeSceneChanged -= OnActiveSceneChanged ;

				RemoveAllCueSheets_Private( true ) ;
				SetAcf( null ) ;

				m_Listener = null ;
				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// ＡＣＦファイルのパスを設定する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static void SetAcf( string path )
		{
			if( string.IsNullOrEmpty( path ) == false )
			{
				CriAtomEx.RegisterAcf( null, path ) ;
			}
			else
			{
				CriAtomEx.UnregisterAcf() ;
			}
		}

		/// <summary>
		/// キューシートを追加する(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool AddCueSheet( string cueSheetName, string acbPath, string awbPath = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.AddCueSheet_Private( cueSheetName, acbPath, awbPath, keep ) ;
		}

		private bool AddCueSheet_Private( string cueSheetName, string acbPath, string awbPath, bool keep )
		{
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == true )
			{
				// 既にロード済みである
				m_CueSheetCache_Hash[ cueSheetName ].IsGarbage = false ;	// このシーンでは残す
				return true ;
			}

			CriAtomCueSheet cueSheet = CriAtom.AddCueSheet( cueSheetName, acbPath, awbPath ) ;
			if( cueSheet == null )
			{
				return false ;
			}
#if UNITY_EDITOR
			if( !string.IsNullOrEmpty( awbPath ) && m_NumberOfBinders >= m_MaxNumberOfBinders )
			{
				Debug.LogError( "[Error] The binder is already used to the maximum. ( " + m_NumberOfBinders + "/" + m_MaxNumberOfBinders + " ) : add " + awbPath ) ;
			}
#endif
			// 現在ロード中のキューシートとして情報を追加する(同期には cpk は使用できないためバインド数が増加するかは awb を使用しているかどうか)
			var cueSheetCache = new CueSheetCache( cueSheetName, keep, !string.IsNullOrEmpty( awbPath ), !string.IsNullOrEmpty( awbPath ), this ) ;
			m_CueSheetCache.Add( cueSheetCache  ) ;
			m_CueSheetCache_Hash.Add( cueSheetName, cueSheetCache ) ;

			return true ;
		}

		/// <summary>
		/// キューシートの登録処理中かどうか確認する
		/// </summary>
		/// <param name="cueSheetName"></param>
		/// <returns></returns>
		public static bool IsCueSheetProcessing( string cueSheetName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.IsCueSheetProcessing_Private( cueSheetName ) ;
		}

		private bool IsCueSheetProcessing_Private( string cueSheetName )
		{
			return m_ProcessingCueSheedNames.Contains( cueSheetName ) ;
		}

		/// <summary>
		/// キューシートを追加する(非同期)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Request AddCueSheetAsync( string cueSheetName, string acbPath, string awbPath = null, string cpkPath = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.AddCueSheetAsync_Private( cueSheetName, acbPath, awbPath, cpkPath, keep, request ) ) ;
			return request ;
		}

		private IEnumerator AddCueSheetAsync_Private( string cueSheetName, string acbPath, string awbPath, string cpkPath, bool keep, Request request )
		{
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == true )
			{
				// 既にロード済みである
				m_CueSheetCache_Hash[ cueSheetName ].IsGarbage = false ;	// このシーンでは残す
				request.IsDone = true ;
				yield break ;
			}

			//----------------------------------

			if( m_ProcessingCueSheedNames.Contains( cueSheetName ) == true )
			{
				// まだ登録は完了していないが既に登録処理中のキューシートである
				request.IsDone = true ;
				yield break ;
			}

			// 処理中のキューシート名を登録する
			m_ProcessingCueSheedNames.Add( cueSheetName ) ;

			//----------------------------------

			if( string.IsNullOrEmpty( cpkPath ) == true )
			{
				// acb + awb
				CriAtomCueSheet cueSheet = CriAtom.AddCueSheetAsync( cueSheetName, acbPath, awbPath ) ;

				if( cueSheet == null )
				{
					m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
					request.Error = "Cound not load " + acbPath ;
					yield break ;
				}

				while( cueSheet.IsLoading == true && cueSheet.IsError == false )
				{
					yield return null ;
				}

				if( cueSheet.IsError == true )
				{
					m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
					request.Error = "Cound not load " + acbPath ;
					yield break ;
				}
#if UNITY_EDITOR
				if( !string.IsNullOrEmpty( awbPath ) && m_NumberOfBinders >= m_MaxNumberOfBinders )
				{
					Debug.LogError( "[Error] The binder is already used to the maximum. ( " + m_NumberOfBinders + "/" + m_MaxNumberOfBinders + " ) : add " + awbPath ) ;
				}
#endif
				// 現在ロード中のキューシートとして情報を追加する(cpk を使用していなくとも awb を使用していればバインド数は増加する)
				var cueSheetCache = new CueSheetCache( cueSheetName, keep, !string.IsNullOrEmpty( awbPath ), !string.IsNullOrEmpty( awbPath ), this ) ;
				m_CueSheetCache.Add( cueSheetCache  ) ;
				m_CueSheetCache_Hash.Add( cueSheetName, cueSheetCache ) ;
			}
			else
			{
				// cpk
#if UseCPK
				if( m_NumberOfBinders >= m_MaxNumberOfBinders )
				{
					// バインド限界数を超える場合使用していないキューシートを破棄
					RemoveAllCueSheets(false);
#if UNITY_EDITOR
					if( m_NumberOfBinders >= m_MaxNumberOfBinders )
					{
						Debug.LogError( "[Error] The binder is already used to the maximum. ( " + m_NumberOfBinders + "/" + m_MaxNumberOfBinders + " ) : add " + cpkPath ) ;
					}
#endif
				}
				var binder = new CriFsBinder() ;
				CriFsBindRequest bindRequest = CriFsUtility.BindCpk( binder, cpkPath ) ;

				yield return bindRequest.WaitForDone( this ) ;

				if( bindRequest.error != null )
				{
					// エラー発生
					binder.Dispose() ;

					m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
					request.Error = "Cound not load " + cpkPath + "\n" + bindRequest.error ;
					yield break ;
				}

				uint bindId = bindRequest.bindId ;

				//---------------------------------

				acbPath = null ;
				awbPath = null ;

				int count = CriFsBinder.GetNumContentsFiles( bindId ) ;
				CriFsBinder.GetContentsFileInfoByIndex( bindId, 0, count, out CriFsBinder.ContentsFileInfo[] info ) ;

				if( info != null && info.Length == count )
				{
					int index ;
					for( index  = 0 ; index <  count ; index ++ )
					{
						string path = info[ index ].fileName ;
						string extension = Path.GetExtension( path ) ;

						if( extension == ".acb" )
						{
							acbPath = path ;
						}
						else
						if( extension == ".awb" )
						{
							awbPath = path ;
						}
					}
				}

				CriAtomCueSheet cueSheet = CriAtom.AddCueSheetAsync( cueSheetName, acbPath, awbPath, binder ) ;

				if( cueSheet == null )
				{
					// エラー発生
					CriFsBinder.Unbind( bindId ) ;
					binder.Dispose() ;

					m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
					request.Error = "Cound not load " + cpkPath ;
					yield break ;
				}

				while( cueSheet.IsLoading == true && cueSheet.IsError == false )
				{
					yield return null ;
				}

				if( cueSheet.IsError == true )
				{
					// エラー発生
					CriFsBinder.Unbind( bindId ) ;
					binder.Dispose() ;

					m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
					request.Error = "Cound not load " + cpkPath ;
					yield break ;
				}

				// 現在ロード中のキューシートとして情報を追加する(cpk は常にバインド数を増加させる)
				var cueSheetCache = new CueSheetCache( cueSheetName, keep, !string.IsNullOrEmpty( awbPath ), true, this )
				{
					Binder = binder,
					BindId = bindId
				} ;

				m_CueSheetCache.Add( cueSheetCache  ) ;

				if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == true )
				{
					Debug.LogWarning( "既に登録済みのキューシート:" + cueSheetName ) ;
				}
				m_CueSheetCache_Hash.Add( cueSheetName, cueSheetCache ) ;
#else
				Debug.LogError( "[Bad Rqeust] Cpk cannot be used : " + cpkPath ) ;
#endif
			}

			// 成功
			m_ProcessingCueSheedNames.Remove( cueSheetName ) ;
			request.IsDone = true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// キューシートを確認する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool HasCueSheet( string cueSheetName )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.HasCueSheet_Private( cueSheetName ) ;
		}

		private bool HasCueSheet_Private( string cueSheetName )
		{
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == true )
			{
				// 既にロード済みである
				m_CueSheetCache_Hash[ cueSheetName ].IsGarbage = false ; // このシーンでは残す
				return true ;
			}

			return false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// キューシートの使用数を取得します
		/// </summary>
		/// <param name="cueSheetName">キューシート名</param>
		/// <param name="useCount">現時点の使用数</param>
		/// <returns></returns>
		public static bool TryGetHasCueSheetCount( string cueSheetName, out int useCount )
		{
			if( m_Instance == null )
			{
				useCount = 0 ;
				return false ;
			}
			return m_Instance.TryGetHasCueSheetCount_Private( cueSheetName, out useCount ) ;
		}

		private bool TryGetHasCueSheetCount_Private( string cueSheetName, out int useCount )
		{
			if ( m_CueSheetCache_Hash.TryGetValue( cueSheetName, out var cache ) )
			{
				// 既にロード済みである
				cache.IsGarbage = false ; // このシーンでは残す
				useCount        = cache.Count ;
				return true ;
			}

			useCount = 0 ;
			return false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// キューシートを破棄する(同期)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool RemoveCueSheet( string cueSheetName )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.RemoveCueSheet_Private( cueSheetName ) ;
		}

		private bool RemoveCueSheet_Private( string cueSheetName )
		{
			bool result = DisposeCueSheet_Private( cueSheetName ) ;

#if UNITY_EDITOR
			//----------------------------------
			// キューシートの状態をモニタリングする
			if( result == true )
			{
				UpdateCueSheetMonitor() ;
			}
#endif
			return result ;
		}


		/// <summary>
		/// キューシートキャッシュをクリアする
		/// </summary>
		public static void RemoveAllCueSheets( bool isForce )
		{
			if( m_Instance == null )
			{
				return ;
			}
			m_Instance.RemoveAllCueSheets_Private( isForce ) ;
		}

		//-----------------------------------

		// 削除可能なものを削除する
		private readonly List<string> m_RemoveTargets = new () ;

		private void RemoveAllCueSheets_Private( bool isForce )
		{
			if( isForce == false )
			{
				// 削除可能なもののみ削除する

				m_RemoveTargets.Clear() ;

				foreach( var cueSheetCache in m_CueSheetCache )
				{
					if( cueSheetCache.Keep == false )
					{
						if( cueSheetCache.Count == 0 )
						{
							// 破棄可能なので破棄対象に追加
							m_RemoveTargets.Add( cueSheetCache.Name ) ;
						}
						else
						{
							// 破棄可能だが含まれるキューが使用中であるため破棄を保留しガベージフラグをオンにする(次のシーン中に破棄できるタイミングで破棄する)
							cueSheetCache.IsGarbage = true ;
						}
					}
				}

				// 破棄可能なキューシートを全て破棄する
				foreach( var cueSheetName in m_RemoveTargets )
				{
					DisposeCueSheet_Private( cueSheetName ) ;
				}
			}
			else
			{
				// 全てを強制的に削除する
				StopAll() ;

				//----------------------------------

				m_RemoveTargets.Clear() ;

				// 全対象
				foreach( var cueSheetCache in m_CueSheetCache )
				{
					m_RemoveTargets.Add( cueSheetCache.Name ) ;
				}

				foreach( var cueSheetName in m_RemoveTargets )
				{
					// 実際に破棄する
					DisposeCueSheet_Private( cueSheetName ) ;
				}

				// 実際は不要
				m_CueSheetCache.Clear() ;
				m_CueSheetCache_Hash.Clear() ;
			}

#if UNITY_EDITOR
			//----------------------------------
			// キューシートの状態をモニタリングする
			UpdateCueSheetMonitor() ;
#endif
		}

		/// <summary>
		/// シーンが変わる際に破棄できなかったキューシートを破棄する
		/// </summary>
		private void RemoveAllCueSheetsAtFrame_Private()
		{
			m_RemoveTargets.Clear() ;

			if( m_CueSheetCache != null && m_CueSheetCache.Count >  0 )
			{
				foreach( var cueSheetCache in m_CueSheetCache )
				{
					if( cueSheetCache.Keep == false && cueSheetCache.Count == 0 && cueSheetCache.IsGarbage == true )
					{
						// 破棄可能なので破棄対象に追加
						m_RemoveTargets.Add( cueSheetCache.Name ) ;
					}
				}
			}

			// 破棄可能なキューシートを全て破棄する
			foreach( var cueSheetName in m_RemoveTargets )
			{
				// 実際に破棄する
				DisposeCueSheet_Private( cueSheetName ) ;
			}

#if UNITY_EDITOR
			//----------------------------------
			// キューシートの状態をモニタリングする
			UpdateCueSheetMonitor() ;
#endif
		}

		// 実際にキュートーとを破棄する
		private bool DisposeCueSheet_Private( string cueSheetName )
		{
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == false )
			{
				// ロードされていない
				return true ;
			}

			//-------------------------------------------------
			// キューシートに関連するチャンネルを強制停止する

			int i, l ;
			AudioChannel_ADX2 audioChannel ;

			// 終了したチャンネルを監視してコールバックを発生させる
			l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.CueSheetName == cueSheetName && audioChannel.IsUsing == true )
					{
						// ワンショット再生の自然停止待ちだとここで再生終了をチェックするしかない
//						Debug.Log( "<color=#FFFF00>--------->チャンネルから完全に破棄:" + audioChannel.CueName + "</color>" ) ;

						if( audioChannel.IsPlaying == true )
						{
							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;
						}

						// キューシートに対する参照カウントを減らす
						m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;

						// 以後は完全に発音が停止するまでチャンネルへの操作はできない
						audioChannel.Clear() ;

						audioChannel.IsUsing = false ;	// 多重にコールバックが呼ばれてしまうと問題なので１回呼んだらフラグを落として２回以上呼ばれないようにする
					}

					//-------------------------------

					// 真の意味でチャンネルが解放された
					audioChannel.Busy = false ;
				}
			}

			//-------------------------------------------------

//			Debug.Log( "<color=#FFFF00>[重要]キューシートを破棄する:" + cueSheetName + "</color>" ) ;

			// キューシートを破棄(メモリを開放)
			CriAtom.RemoveCueSheet( cueSheetName ) ;

			var cueSheetCache = m_CueSheetCache_Hash[ cueSheetName ] ;
			cueSheetCache.Terminate() ;	// 内部で Unbind を実行している
			m_CueSheetCache.Remove( cueSheetCache  ) ;
			m_CueSheetCache_Hash.Remove( cueSheetName ) ;

			return true ;
		}

#if UNITY_EDITOR
		private readonly List<string> m_MonitoringCueSheetNames = new () ;

		// キューシートの登録状態を更新する
		private void UpdateCueSheetMonitor()
		{
			if( m_CueSheetCache == null | m_CueSheetCache.Count == 0 )
			{
				m_MonitoringCueSheetNames.Clear() ;
				m_CueSheetNames = m_MonitoringCueSheetNames ;
			}
			else
			{
				m_MonitoringCueSheetNames.Clear() ;
				foreach( var cueSheetCache in m_CueSheetCache )
				{
					m_MonitoringCueSheetNames.Add( cueSheetCache.Name ) ;
				}
				m_CueSheetNames = m_MonitoringCueSheetNames ;
			}
		}
#endif

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// カテゴリーごとのＡＩＳＡＣコントロールを設定する
		/// </summary>
		/// <param name="categoryName"></param>
		/// <param name="controlName"></param>
		/// <param name="value"></param>
		public static void SetAisacControl( string categoryName, string controlName, float value )
		{
			CriAtomExCategory.SetAisacControl( categoryName, controlName, value ) ;
		}

		//---------

		/// <summary>
		/// ゲーム変数の個数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetNumberOfGameVariables()
		{
			return CriAtomEx.GetNumGameVariables() ;
		}

		/// <summary>
		/// ゲーム変数を設定する(識別子が数値版)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		public static void SetGameVariable( uint id, float value )
		{
			CriAtomEx.SetGameVariable( id, value ) ;
		}

		/// <summary>
		/// ゲーム変数を取得する(識別子が数値版)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		public static float GetGameVariable( uint id )
		{
			return CriAtomEx.GetGameVariable( id ) ;
		}

		/// <summary>
		/// ゲーム変数を設定する(識別子が文字列版)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		public static void SetGameVariable( string id, float value )
		{
			CriAtomEx.SetGameVariable( id, value ) ;
		}

		/// <summary>
		/// ゲーム変数を取得する(識別子が文字列版)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		public static float GetGameVariable( string id )
		{
			return CriAtomEx.GetGameVariable( id ) ;
		}

		//-------------------------------------------------------------------------------------------

#if USE_MICROPHONE
		/// <summary>
		/// マイク入力を開始する
		/// </summary>
		/// <param name="rMicrophone"></param>
		/// <returns></returns>
		public static IEnumerator StartMicrophone( int recordingTime, int recordingFrequency, int bufferSize = 0, bool dataSkip = false, Action<float[],int> callback = null )
		{
			if( m_Instance == null )
			{
				yield break ;
			}

			yield return m_Instance.StartCoroutine( m_Instance.StartMicrophone_Private( recordingTime, recordingFrequency, bufferSize, dataSkip, callback ) ) ;
		}

		// マイク入力を開始する
		private IEnumerator StartMicrophone_Private( int recordingTime, int recordingFrequency, int bufferSize, bool dataSkip, Action<float[],int> callback )
		{
			if( Microphone.devices == null || Microphone.devices.Length == 0 )
			{
				// マイクが接続されていない模様
				yield break ;
			}

			//----------------------------------------------------------

			if( m_MicrophoneRecordingAudioSource == null )
			{
				GameObject go = new GameObject( "Microphone" ) ;
				go.transform.SetParent( transform, false ) ;

				// AudioManager に直で AudioSource を Add してはダメ
				m_MicrophoneRecordingAudioSource = tGameObject.AddComponent<AudioSource>() ;
				m_MicrophoneRecordingAudioSource.playOnAwake = false ;
			}

			m_MicrophoneRecordingAudioSource.enabled = true ;

			m_MicrophoneRecordingTime		= recordingTime ;
			m_MicrophoneRecordingFrequency	= recordingFrequency ;

			AudioClip audioClip = Microphone.Start( null, true, m_MicrophoneRecordingTime, m_MicrophoneRecordingFrequency ) ;

			m_MicrophoneCallback				= callback ;
			if( callback != null && bufferSize >  0 )
			{
				m_MicrophoneRecordingAudioClip	= audioClip ;
				m_MicrophoneRecordingBuffer		= new float[ m_MicrophoneRecordingTime * m_MicrophoneRecordingFrequency ] ;

				m_MicrophoneCallbackBuffer		= new float[ bufferSize ] ;

				m_MicrophoneCallbackDataSkip	= dataSkip ;
				m_MicrophoneRecordingPosition	= 0 ;
			}

			m_MicrophoneRecordingAudioSource.clip = audioClip ;

			// 準備が整うまで待つ
			yield return new WaitWhile( () => Microphone.GetPosition( null ) <= 0 ) ;

			m_MicrophoneRecordingAudioSource.timeSamples = Microphone.GetPosition( null ) ;
			m_MicrophoneRecordingAudioSource.loop = true ;
			m_MicrophoneRecordingAudioSource.Play() ;
		}

		/// <summary>
		/// マイク入力を停止する
		/// </summary>
		/// <param name="tMicrophone"></param>
		/// <returns></returns>
		public static bool StopMicrophone()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopMicrophone_Private() ;
		}

		private bool StopMicrophone_Private()
		{
			if( m_MicrophoneRecordingAudioSource != null )
			{
				m_MicrophoneRecordingAudioSource.Stop() ;
				m_MicrophoneRecordingAudioSource.enabled = false ;
			}

			Microphone.End( null ) ;

			if( m_MicrophoneCallback != null )
			{
				m_MicrophoneCallbackBuffer		= null ;
				m_MicrophoneRecordingBuffer		= null ;
				m_MicrophoneRecordingAudioClip	= null ;

				m_MicrophoneCallback			= null ;
			}

			return true ;
		}

		/// <summary>
		/// マイクのオーディオソースを取得する
		/// </summary>
		/// <returns></returns>
		public static AudioSource GetMicrophone()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.m_MicrophoneRecordingAudioSource ;
		}

		// マイクの録音処理
		private void ProcessMicrophoneRecording()
		{
			if( m_MicrophoneRecordingAudioClip == null || m_MicrophoneRecordingBuffer == null || m_MicrophoneCallback == null || m_MicrophoneCallbackBuffer == null )
			{
				return ;
			}

			//----------------------------------------------------------

			int position = Microphone.GetPosition( null ) ;
			if( position <  0 || position == m_MicrophoneRecordingPosition )
			{
				return ;
			}

			int cb_Size = m_MicrophoneCallbackBuffer.Length ;
			int rb_Size = m_MicrophoneRecordingBuffer.Length ;

			int size ;

			// サンプルデータをコピーする
			m_MicrophoneRecordingAudioClip.GetData( m_MicrophoneRecordingBuffer, 0 ) ;

			while( true )
			{
				if( m_MicrophoneRecordingPosition <  position )
				{
					// そのまま引き算で録音サイズ数が計算できる
					size = position - m_MicrophoneRecordingPosition ;
				}
				else
				{
					// バッファの反対側に回っているので若干計算が複雑になる
					size = ( rb_Size - m_MicrophoneRecordingPosition ) + position ;
				}

				if( size <  cb_Size )
				{
					// 必要量に足りない
					return ;
				}

				// レコーディングバッファのサンプルデータをコルーバックバッファにコピーする
				if( ( m_MicrophoneRecordingPosition + cb_Size ) <= rb_Size )
				{
					// １回でコピー可能
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, cb_Size ) ;
				}
				else
				{
					// ２回でコピー可能
					int o = tRB_Size - m_MicrophoneRecordingPosition ;
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, o ) ;
					Array.Copy( m_MicrophoneRecordingBuffer, 0,                             m_MicrophoneCallbackBuffer, 0, cb_Size - o ) ;
				}

				// コールバック実行
				m_MicrophoneCallback( m_MicrophoneCallbackBuffer, m_MicrophoneRecordingFrequency ) ;

				// 読み取り位置を変化させる
				if( m_MicrophoneCallbackDataSkip == false )
				{
					// データスキップ無し
					m_MicrophoneRecordingPosition = ( m_MicrophoneRecordingPosition + cb_Size ) % rb_Size ;
				}
				else
				{
					// データスキップ有り
					m_MicrophoneRecordingPosition = position ;
					return ;	// 終了
				}
			}
		}
#endif
		//-----------------------------------------------------------------

		// 以下スクリプトから利用するメソッド群

		/// <summary>
		/// リスナーの状態
		/// </summary>
		public static bool ListenerEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.ListenerEnabled_Private ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.ListenerEnabled_Private = value ;
			}
		}

		// リスナーの状態
		private bool ListenerEnabled_Private
		{
			get
			{
				if( m_Listener == null )
				{
					return false ;
				}

				return m_Listener.enabled ;
			}
			set
			{
				if( m_Listener == null )
				{
					return ;
				}

				m_Listener.enabled = value ;
			}
		}


		/// <summary>
		/// いずれかの空いているオーディオチャンネルを１つ取得する
		/// </summary>
		/// <returns>オーディオチャンネルのインスタンス</returns>
		public static AudioChannel_ADX2 GetChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetChannel_Private( null, null ) ;
		}

		// いずれかの空いているオーディオチャンネルを１つ取得する
		private AudioChannel_ADX2 GetChannel_Private( string tag, string path )
		{
			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;

			//----------------------------------------------------------
			// デバッグ用モニタリング
#if false
			List<AudioChannel_ADX2> records = Channels.OrderBy( _ => _.Tag ).ToList() ;
			Debug.Log( "<color=#FFDF00>=========チャンネル状況 : " + l + " / " + Max + "</color>" ) ;

			var info = CriAtomExVoicePool.GetNumUsedVoices( CriAtomExVoicePool.VoicePoolId.StandardMemory ) ;
			Debug.Log( "<color=#FF00FF>ボイスプール状況: " + info.numUsedVoices + " / " + info.numPoolVoices + "</color>" ) ;


			for( i  = 0 ; i <  l ; i ++ )
			{
				var record = records[ i ] ;
				if( record != null )
				{
					string color ;

					if( record.IsRunning == true && record.IsPlaying == true )
					{
						color = "FF7F00" ;
					}
					else
					{
						color = "007F7F" ;
					}

					if( record.PlayId >= 0 )
					{
						Debug.Log( "<color=#" + color + ">チャンネル状態:" + record.CueSheetName + ":"  + record.CueName + " Run:" + record.IsRunning.ToString().Substring( 0, 1 ) + " Play:" + record.IsPlaying.ToString().Substring( 0,1 ) + " Pause:" + record.IsPausing.ToString().Substring( 0, 1 ) + " Status:" + record.Status + " Loop:" + record.Loop.ToString().Substring( 0, 1 ) + " SLoop:" + record.SourceLoop.ToString().Substring( 0, 1 )  + " Tag:" + record.Tag + " PID = " + record.PlayId + "</color>" ) ;
					}
				}
			}
			Debug.Log( "<color=#FFDF00>====================</color>" ) ;
#endif
			//----------------------------------

			if( string.IsNullOrEmpty( tag ) == false && m_Tags.ContainsKey( tag ) == true && m_Tags[ tag ].MaxChannels >  0 )
			{
				// タグごとのチャンネル数の制限が有る

				int count = 0 ;

				// 現在再生中のチャンネルで指定のタグに該当するものをカウントする
				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel != null && audioChannel.IsPlaying == true && audioChannel.Tag == tag )
					{
						count ++ ;
					}
				}

				if( count >= m_Tags[ tag ].MaxChannels )
				{
					// 既に最大数までチャンネルを使用している
					Debug.Log( $"<color=#FFFF00>このタグの最大チャンネルまで使用済み: {count} /  {m_Tags[ tag ].MaxChannels} 再生出来なかったソース: {path}</color>" ) ;
					return null ;	// 最大チャンネルまで使用済み
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				audioChannel = Channels[ i ] ;
				if( audioChannel != null && audioChannel.IsPlaying == false && audioChannel.IsPausing == false && audioChannel.IsLocking == false )
				{
//					Debug.Log( "空いているチャンネル:" + Channels[ i ].CueName + " P:" + Channels[ i ].IsPlaying + " P:" + Channels[ i ].IsPausing + " L:" + Channels[ i ].IsLocking ) ;

					// 空いているチャンネルを発見した
					return audioChannel ;
				}
			}

			//----------------------------------------------------------

			if( l >= Max )
			{
				Debug.LogWarning( $"<color=#FF7F00>最大チャンネルまで使用済み: {l} / {Max} 再生出来なかったソース: {path}</color>" ) ;
				return null ;	// 最大チャンネルまで使用済み
			}

			// 空いているソースが無いので追加する

			var audioChannelObject = new GameObject( "Source - " + Channels.Count.ToString( "D02" ) ) ;
			audioChannelObject.transform.SetParent( transform, false ) ;

			audioChannel = new AudioChannel_ADX2( audioChannelObject.AddComponent<CriAtomSource>(), m_EnableLowLatency ) ;
			Channels.Add( audioChannel ) ;

			return audioChannel ;
		}

		/// <summary>
		/// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		/// </summary>
		/// <returns>使用中の全てのチャンネルのインスタンス</returns>
		public static string[] GetUsedChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetUsedChannel_Private() ;
		}

		// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		private string[] GetUsedChannel_Private()
		{
			var list = new List<string>() ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true || Channels[ i ].IsPausing == true || Channels[ i ].IsLocking == true )
					{
						// 使用中のチャンネルを発見した
						list.Add( Channels[ i ].CueName ) ;
					}
				}
			}

			if( list.Count == 0 )
			{
				return null ;
			}

			return list.ToArray() ;
		}

		// 指定した識別子に該当する再生中のオーディオチャンネルを取得する
		private AudioChannel_ADX2 GetChannelByPlayId( int playId )
		{
			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( ( audioChannel.IsPlaying == true || audioChannel.IsPausing == true || audioChannel.IsLocking == true ) && audioChannel.PlayId == playId )
					{
						// 該当するチャンネルを発見した
						return audioChannel ;
					}
				}
			}

			// 該当するチャンネルが存在しない1
			return null ;
		}

		//---------------------------------------------------------


		/// <summary>
		/// サウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="sheetName">キューシート名</param>
		/// <param name="cueName">キュー名</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play
		(
			string sheetName, string cueName,
			bool loop = false,
			float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f,
			string selector = null, string label = null,
			string tag = null
		)
		{
			if( m_Instance == null )
			{
				return -1 ;
			}

			return m_Instance.Play_Private
			(
				sheetName, cueName,
				loop,
				volume, pan, pitch,
				selector, label,
				tag
			) ;
		}

		//----------------

		// サウンドを再生する(オーディオクリップから)
		private int Play_Private
		(
			string cueSheetName, string cueName,
			bool loop,
			float volume, float pan, float pitch,
			string selector, string label,
			string tag
		)
		{
			var audioChannel = GetChannel_Private( tag, $"{cueSheetName}|{cueName}" ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}

			//-----------------------------------------------------

			// キューシートのロード状況を確認する
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == false )
			{
				// キューシートがロードされていない
				return -1 ;
			}
			m_CueSheetCache_Hash[ cueSheetName ].Count ++ ;

			//----------------------------------------------------------

			int playId = GetPlayId() ;

			//----------------------------------------------------------
			// 音源とリスナーの位置を同じにする

			Transform   sourceTransform         = null ;
			Transform   listenerTransform       = null ;
			Transform   trueListenerTransform   = null ;
			float       distanceScale           = 1 ;

			if( m_Listener != null && m_Listener.enabled == true )
			{
				// マネージャのものが有効化されている
				sourceTransform         = m_Listener.transform ;
				listenerTransform       = m_Listener.transform ;
				trueListenerTransform   = m_Listener.transform ;
			}
			else
			{
				// その他のものを使用する
				var listener = GameObject.FindAnyObjectByType( typeof( CriAtomListener ) ) as CriAtomListener ;
				if( listener != null )
				{
					sourceTransform         = listener.transform ;
					listenerTransform       = listener.transform ;
					trueListenerTransform   = listener.transform ;
				}
			}

			//----------------------------------------------------------

			// 再生する
			audioChannel.Play
			(
				playId,
				cueSheetName, cueName,
				loop,
				GetBaseVolume( tag ), volume, pan,
				sourceTransform, listenerTransform, trueListenerTransform, distanceScale, false,
				pitch,
				selector, label,
				tag,
				m_CueSheetCache_Hash[ cueSheetName ].IsStreaming
			) ;

			if( string.IsNullOrEmpty( tag ) == false && m_MuteChannels.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}

			// 成功したらソースのインスタンスを返す
			return playId ;
		}

		//---------------------------------

		/// <summary>
		/// フェード付きでサウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="sheetName">キューシート名</param>
		/// <param name="cueName">キュー名</param>
		/// <param name="duration">フェードの時間(秒)</param>
		/// <param name="loop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade
		(
			string sheetName, string cueName,
			float duration = 1.0f,
			bool loop = false,
			float volume = 1.0f, float pan = 0.0f, float pitch = 0.0f,
			string selector = null, string label = null,
			string tag = null
		)
		{
			if( m_Instance == null )
			{
				return -1 ;
			}

			return m_Instance.PlayFade_Private
			(
				sheetName, cueName,
				duration,
				loop,
				volume, pan, pitch,
				selector, label,
				tag
			) ;
		}

		//--------------------

		// フェード付きでサウンドを再生する(オーディオクリップから)
		private int PlayFade_Private
		(
			string cueSheetName, string cueName,
			float duration,
			bool loop,
			float volume, float pan, float pitch,
			string selector, string label,
			string tag
		)
		{
			var audioChannel = GetChannel_Private( tag, $"{cueSheetName}|{cueName}" ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}

			//-----------------------------------------------------

			// キューシートのロード状況を確認する
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == false )
			{
				// キューシートがロードされていない
				return -1 ;
			}
			m_CueSheetCache_Hash[ cueSheetName ].Count ++ ;

			//-----------------------------------------------------

			// プレイフェードに登録する

			// 一旦破棄
			RemoveChannelFading( audioChannel ) ;

			var effect = new FadeEffect()
			{
				StartTime	= Time.realtimeSinceStartup,
				Duration	= duration,
				StartVolume	= 0,
				EndVolume	= 1
			} ;

			m_FadePlayChannels.Add( audioChannel, effect ) ;

			// フェード処理中であるというロックをかける
			audioChannel.Lock() ;

			//----------------------------------------------------------

			int playId = GetPlayId() ;

			//----------------------------------------------------------
			// 音源とリスナーの位置を同じにする

			Transform   sourceTransform         = null ;
			Transform   listenerTransform       = null ;
			Transform   trueListenerTransform   = null ;
			float       distanceScale           = 1 ;

			if( m_Listener != null && m_Listener.enabled == true )
			{
				// マネージャのものが有効化されている
				sourceTransform         = m_Listener.transform ;
				listenerTransform       = m_Listener.transform ;
				trueListenerTransform   = m_Listener.transform ;
			}
			else
			{
				// その他のものを使用する
				var listener = GameObject.FindAnyObjectByType( typeof( CriAtomListener ) ) as CriAtomListener ;
				if( listener != null )
				{
					sourceTransform         = listener.transform ;
					listenerTransform       = listener.transform ;
					trueListenerTransform   = listener.transform ;
				}
			}

			//----------------------------------------------------------

			// 再生する
			audioChannel.Play
			(
				playId,
				cueSheetName, cueName,
				loop,
				GetBaseVolume( tag ), volume, pan,
				sourceTransform, listenerTransform, trueListenerTransform, distanceScale, false,
				pitch,
				selector, label,
				tag,
				m_CueSheetCache_Hash[ cueSheetName ].IsStreaming
			) ;

			if( string.IsNullOrEmpty( tag ) == false && m_MuteChannels.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}

			// 成功したらソースのインスタンスを返す
			return playId ;
		}

		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="sheetName">キューシート名</param>
		/// <param name="cueName">キュー名</param>
		/// <param name="position">音源のワールド空間座標</param>
		/// <param name="listenerTransform">リスナーのトランスフォーム</param>
		/// <param name="scale">距離の係数</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static int Play3D
		(
			string cueSheetName, string cueName,
			Transform sourceTransform, Transform listenerTransform = null, float distanceScale = 1, bool isRealTimeUpdating = true,
			bool loop = false,
			float volume = 1.0f, float pitch = 0.0f,
			string selector = null, string label = null,
			string tag = null
		)
		{
			if( m_Instance == null )
			{
				return -1 ;
			}

			return m_Instance.Play3D_Private
			(
				cueSheetName, cueName,
				sourceTransform, listenerTransform, distanceScale, isRealTimeUpdating,
				loop,
				volume, pitch,
				selector, label,
				tag
			) ;
		}

		// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		private int Play3D_Private
		(
			string cueSheetName, string cueName,
			Transform sourceTransform, Transform listenerTransform, float distanceScale, bool isRealTimeUpdatimg,
			bool loop,
			float volume, float pitch,
			string selector, string label,
			string tag
		)
		{
			var audioChannel = GetChannel_Private( tag, $"{cueSheetName}|{cueName}" ) ;
			if( audioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}

			//-----------------------------------------------------

			// キューシートのロード状況を確認する
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == false )
			{
				// キューシートがロードされていない
				return -1 ;
			}
			m_CueSheetCache_Hash[ cueSheetName ].Count ++ ;

			//----------------------------------------------------------
#if false
			// デバッグ
			CriAtomExPlayer criAtomExPlayer = new CriAtomExPlayer() ;
			CriAtomExAcb cueSheet = CriAtom.GetCueSheet( cueSheetName ).acb ;

			criAtomExPlayer.SetCue( cueSheet, cueName ) ;
			criAtomExPlayer.SetVolume( volume ) ;
			criAtomExPlayer.Start() ;
#endif
			//----------------------------------------------------------

			int playId = GetPlayId() ;

			//----------------------------------------------------------
			// 音源位置の設定

			CriAtomListener trueListener ;

			if( m_Listener != null )
			{
				trueListener = m_Listener ;
			}
			else
			{
				trueListener = GameObject.FindAnyObjectByType( typeof( CriAtomListener ) ) as CriAtomListener ;
			}

			if( trueListener == null )
			{
				Debug.LogWarning( "[AudioManager_ADX2] Not found listener." ) ;
				return -1 ;
			}

			//----------------------------------

			// その他のものを使用する
			if( listenerTransform == null )
			{
				listenerTransform  = trueListener.transform ;
			}

			//------------------------------------------------------------------------------------------
			
			// 再生する
			audioChannel.Play
			(
				playId,
				cueSheetName, cueName,
				loop,
				GetBaseVolume( tag ), volume, 0,
				sourceTransform, listenerTransform, trueListener.transform, distanceScale, isRealTimeUpdatimg,
				pitch,
				selector, label,
				tag,
				m_CueSheetCache_Hash[ cueSheetName ].IsStreaming
			) ;

			if( string.IsNullOrEmpty( tag ) == false && m_MuteChannels.Contains( tag ) == true )
			{
				// ミュート対象のタグ
				audioChannel.Mute = true ;
			}

			// 成功したらソースのインスタンスを返す
			return playId ;
		}

		//-------------------------------------------------------------------------------------------
		// ＡＤＸ固有の機能

		/// <summary>
		/// バスセンドレベルを設定する
		/// </summary>
		/// <param name="busName"></param>
		/// <param name="sendLevel"></param>
		/// <returns></returns>
		public static bool SetBusSendLevel( int playId, string busName, float sendLevel = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetBusSendLevel_Private( playId, busName, sendLevel ) ;
		}

		// バスセンドレベルを設定する
		private bool SetBusSendLevel_Private( int playId, string busName, float sendLevel )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.SetBusSendLevel( busName, sendLevel ) ;
		}

		/// <summary>
		/// Ａｉｓｃコントロールを設定する
		/// </summary>
		/// <param name="busName"></param>
		/// <param name="sendLevel"></param>
		/// <returns></returns>
		public static bool SetAiscControl( int playId, string controlName, float value )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetAiscControl_Private( playId, controlName, value ) ;
		}

		// Ａｉｓｃコントロールを設定する
		private bool SetAiscControl_Private( int playId, string busName, float sendLevel )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.SetAiscControl( busName, sendLevel ) ;
		}

		/// <summary>
		/// セレクターラベルを設定する
		/// </summary>
		/// <param name="selector"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool SetSelectorLabel( int playId, string selector, string label )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetSelectorLabel_Private( playId, selector, label ) ;
		}

		// セレクターラベルを設定する
		private bool SetSelectorLabel_Private( int playId, string selector, string label )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.SetSelectorLabel( selector, label ) ;
		}

		/// <summary>
		/// セレクターラベルを削除する
		/// </summary>
		/// <param name="selector"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool UnsetSelectorLabel( int playId, string selector )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.UnsetSelectorLabel_Private( playId, selector ) ;
		}

		// セレクターラベルを削除する
		private bool UnsetSelectorLabel_Private( int playId, string selector )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.UnsetSelectorLabel( selector ) ;
		}

		/// <summary>
		/// セレクターラベルを全て削除する
		/// </summary>
		/// <param name="selector"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool ClearSelectorLabels( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearSelectorLabels_Private( playId ) ;
		}

		// セレクターラベルを全て削除する
		private bool ClearSelectorLabels_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.ClearSelectorLabels() ;
		}

		//---------------------------------------------------------

		/// <summary>
		/// Cue設定をセットする
		/// </summary>
		/// <param name="cueSheetName"></param>
		/// <param name="cueName"></param>
		/// <returns></returns>
		public static bool SetCueSetting( string cueSheetName, string cueName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetCueSetting_Private( cueSheetName, cueName ) ;
		}

		// Cue設定をセットする
		public bool SetCueSetting_Private( string cueSheetName, string cueName )
		{
			// キューシートのロード状況を確認する
			if( m_CueSheetCache_Hash.ContainsKey( cueSheetName ) == false )
			{
				// キューシートがロードされていない
				Debug.LogWarningFormat( "[サウンド] キューシート '{0}' がロードされていません", cueSheetName ) ;
				return false ;
			}
			m_CueSheetCache_Hash[ cueSheetName ].Count ++ ;

			//----------------------------------------------------------

			if( m_CurSettingSource == null )
			{
				var cueSettingSourceObject = new GameObject( "CueSettingSource" ) ;
				cueSettingSourceObject.transform.SetParent( transform, false ) ;

				m_CurSettingSource = cueSettingSourceObject.AddComponent<CriAtomSource>() ;
			}

			// キューシート上にそのキュー名があるのか
			CriAtomExAcb acb = CriAtom.GetAcb( cueSheetName ) ;
			if( acb == null )
			{
				Debug.LogWarningFormat( "[サウンド] キューシート '{0}' を読み込めませんでした", cueSheetName ) ;
				return false ;
			}

			if( acb.GetCueInfo(cueName, out CriAtomEx.CueInfo _ ) == false)
			{
				Debug.LogWarningFormat
				(
					"[サウンド] キュー名 '{0}' が キューシート '{1}' から読み取れませんでした",
					cueName,
					cueSheetName
				) ;
				return false ;
			}

			//----------------------------------

			m_CurSettingSource.cueSheet = cueSheetName ;
			m_CurSettingSource.cueName  = cueName ;

			// 再生
			m_CurSettingSource.Play() ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音のオーディオクリップ名を取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static string GetName( int playId )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetName_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private string GetName_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//----------------------------------

			return audioChannel.CueName ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.IsPlaying_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPlaying_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return audioChannel.IsPlaying ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.IsPausing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPausing_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == false )
			{
				return false ;
			}

			return audioChannel.IsPausing ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsUsing( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.IsUsing_Private( playId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsUsing_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == true || audioChannel.IsPausing == true || audioChannel.IsLocking == true )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// オーディオチャンネルごとのミュート状態を設定する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Mute( int playId, bool state )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Mute_Private( playId, state ) ;
		}

		// オーディオチャンネルごとのミュート状態を設定する
		private bool Mute_Private( int playId, bool state )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Mute = state ;	// 完全停止

			return true ;	// 発見
		}

		/// <summary>
		/// オーディオチャンネルごとにサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Stop_Private( playId ) ;
		}

		// オーディオチャンネルごとにサウンドを完全停止させる
		private bool Stop_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			audioChannel.Stop() ;	// 完全停止

			// フェードリストから除外する
			RemoveChannelFading( audioChannel ) ;

			// 即時呼び出す
			if( audioChannel.IsUsing == true )
			{
				OnStoppedDelegate?.Invoke( playId ) ;

				// キューシートに対する参照カウントを減らす
				m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;

				// 以後は完全に発音が停止するまでチャンネルへの操作はできない
				audioChannel.Clear() ;

				audioChannel.IsUsing = false ;
			}

			return true ;	// 成功
		}

		/// <summary>
		/// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="duration">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFade( int playId, float duration = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return  m_Instance.StopFade_Private( playId, duration ) ;
		}

		// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		private bool StopFade_Private( int playId, float duration )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のチャンネルを発見した（ただし既にフェード対象に対する多重の効果はかけられない）

			if( audioChannel.IsPlaying == true && m_FadeStopChannels.ContainsKey( audioChannel ) == false )
			{
//				Debug.Log( "<color=#FFFF00>------>フェードアウト対象2:" + audioChannel.CueName + "</color>" ) ;

				// 一旦破棄
				RemoveChannelFading( audioChannel ) ;	// 多重実行は禁止したが保険

				var effect = new FadeEffect()
				{
					Duration	= duration,
					StartTime	= Time.realtimeSinceStartup,
					StartVolume	= 1,
					EndVolume	= 0
				} ;

				m_FadeStopChannels.Add( audioChannel, effect ) ;

				// フェード処理中なのでロックする
				audioChannel.Lock() ;

				return true ;   // 発見
			}
			else
			{
				// ポーズ中であれば即時停止させる
				audioChannel.Stop() ;

				// リストから除外する
				RemoveChannelFading( audioChannel ) ;

				// 即時呼び出す
				if( audioChannel.IsUsing == true )
				{
					OnStoppedDelegate?.Invoke( playId ) ;

					// キューシートに対する参照カウントを減らす
					m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;

					// 以後は完全に発音が停止するまでチャンネルへの操作はできない
					audioChannel.Clear() ;

					audioChannel.IsUsing = false ;
				}
			}

			return false ;
		}

		/// <summary>
		/// オーディオチャンネルごとにサウンドを一時停止させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Pause_Private( playId ) ;
		}

		// オーディオチャンネルごとにでサウンドを一時停止させる
		private bool Pause_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した

			if( audioChannel.IsPlaying == true && audioChannel.IsPausing == false )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				PauseChannelFading( audioChannel ) ;

				audioChannel.Pause() ;	// 一時停止
			}

			return true ;
		}

		/// <summary>
		/// オーディオチャンネルごとに一時停止を解除させる
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Unpause_Private( playId ) ;
		}

		// オーディオチャンネルごとに一時停止を解除させる
		private bool Unpause_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( audioChannel.IsPlaying == false )
			{
				// 再生中ではない
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した

			// 注意：AudioChannel では、終了中なのか中断中なのか区別がつかない
			if( audioChannel.IsPausing == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				UnpauseChannelFading( audioChannel ) ;

				audioChannel.Unpause() ;	// 再開
			}
			else
			{
				// ポーズ中でなく普通に終わっているだけなら再生する
				audioChannel.Unpause() ;
			}

			return true ;	// 発見
		}

		//---------------------------------

		/// <summary>
		/// オーディオチャンネルごとのボリュームを設定する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="volume">ボリューム値(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetVolume( int playId, float volume )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetVolume_Private( playId, volume ) ;
		}

		// オーディオチャンネルごとのボリュームを設定する
		private bool SetVolume_Private( int playId, float volume )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( volume >  1 )
			{
				volume  = 1 ;
			}
			else
			if( volume <  0 )
			{
				volume  = 0 ;
			}

			audioChannel.Volume = volume ;

			return true ;	// 発見
		}

		/// <summary>
		/// オーディオチャンネルごとのボリューム係数を設定する
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="volume">ボリューム値(0～)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetVolumeFactor( int playId, float volume )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetVolumeFactor_Private( playId, volume ) ;
		}

		// オーディオチャンネルごとのボリューム係数を設定する
		private bool SetVolumeFactor_Private( int playId, float volume )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( volume <  0 )
			{
				volume  = 0 ;
			}

			audioChannel.StepVolume = volume ;

			return true ;	// 発見
		}

		/// <summary>
		/// オーディオチャンネルごとのピッチを設定する(-1で１オクターブ↓・+1で１オクターブ↑)
		/// </summary>
		/// <param name="playId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="volume">ピッチ値(-8～0～+8)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetPitch( int playId, float pitch )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetPitch_Private( playId, pitch ) ;
		}

		// オーディオチャンネルごとのピッチを設定する(-1で１オクターブ↓・+1で１オクターブ↑)
		private bool SetPitch_Private( int playId, float pitch )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( pitch >  +8 )
			{
				pitch  = +8 ;
			}
			else
			if( pitch <  -8 )
			{
				pitch  = -8 ;
			}

			audioChannel.Pitch = pitch  ;

			return true ;	// 発見
		}

		//-----------------------------------

		/// <summary>
		/// 何らかのチャンネルが鳴っているか確認する
		/// </summary>
		/// <returns>鳴っているチャンネル数</returns>
		public static int IsPlaying()
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.IsPlaying_Private() ;

		}

		// 何らかのチャンネルが鳴っているか確認する
		private int IsPlaying_Private()
		{
			int count = 0 ;

			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true )
					{
						// 該当するチャンネルを発見した
						count ++ ;
					}
				}
			}

			return count ;
		}

		/// <summary>
		/// 指定のタグで何らかのチャンネルが鳴っているか確認する
		/// </summary>
		/// <returns>鳴っているチャンネル数</returns>
		public static int IsPlaying( string tag )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.IsPlaying_Private( tag ) ;

		}

		// 何らかのチャンネルが鳴っているか確認する
		private int IsPlaying_Private( string tag )
		{
			int count = 0 ;

			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && ( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) ) )
					{
						// 該当するチャンネルを発見した
//						Debug.Log( "<color=#FFFF00>----->鳴っている環境音:" + audioChannel.CueName + " Play = " + audioChannel.IsPlaying + "</color>" ) ;
						count ++ ;
					}
				}
			}

			return count ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 再生中の位置(秒)を取得する
		/// </summary>
		/// <param name="playId"></param>
		/// <returns></returns>
		public static float GetTime( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetTime_Private( playId ) ;
		}

		// 再生中の位置(秒)を取得する
		private float GetTime_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------

			return audioChannel.Time ;
		}

		/// <summary>
		/// 再生中の位置(サンプル数)を取得する
		/// </summary>
		/// <param name="playId"></param>
		/// <returns></returns>
		public static int GetTimeSamples( int playId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetTimeSamples_Private( playId ) ;
		}

		// 再生中の位置(秒)を取得する
		private int GetTimeSamples_Private( int playId )
		{
			var audioChannel = GetChannelByPlayId( playId ) ;
			if( audioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------

			return audioChannel.TimeSamples ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// キューの情報を取得します
		/// </summary>
		/// <param name="cueSheetName">キューシート名</param>
		/// <param name="cueName"><see cref="cueSheetName"/> 内にある キュー名</param>
		/// <param name="result">キューの情報</param>
		/// <returns></returns>
		public static bool TryGetCueInfo( string cueSheetName, string cueName, out CriAtomEx.CueInfo result )
		{
			if ( string.IsNullOrEmpty( cueSheetName ) )
			{
				result = default ;
				return false ;
			}

			if ( string.IsNullOrEmpty( cueName ) )
			{
				result = default ;
				return false ;
			}

			// キューシートの取得
			var acb = CriAtom.GetAcb( cueSheetName ) ;
			if ( acb == null )
			{
				result = default ;
				return false ;
			}

			// キューシートからキュー名の情報を取得
			return acb.GetCueInfo( cueName, out result ) ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// オーディオチャンネルごとの現在の状態を取得する
		/// </summary>
		/// <param name="tAudioChannel">オーディオチャンネルのインスタンス</param>
		/// <returns>現在の状態(-1=不正な引数・0=一時停止か完全停止・1=再生</returns>
		public static int GetStatus( AudioChannel audioChannel )
		{
			if( audioChannel == null )
			{
				return -1 ;
			}

			if( audioChannel.IsPlaying() == true )
			{
				return 1 ;	// 再生中
			}

			return 0 ;	// 停止中か一時停止中
		}
#endif
		//---------------------------------------------------------

		// 個別に停止したい場合は再生時に取得したオーディオソースに対して Stop() を実行する

		// オーディオに関しては実装が不完全と言える
		// 音自体の再生停止は runInBackground とは別に管理しなくてはならない（別に管理出来るようにする）
		// （あまり使い道は無いかもしれないがゲームが止まっても音は流し続ける事も可能）
		// しかしフェード効果は Update で処理しているためバックグウンド時はフェード効果が正しく処理されなくなってしまう

		// サスペンド・レジューム
		internal void OnApplicationPause( bool state )
		{
			if( state == true )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}

		// フォーカス
		internal void OnApplicationFocus( bool state )
		{
			if( state == false )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}

		// 全サスペンド
		private bool SuspendAll_Private()
		{
#if UNITY_EDITOR
			m_IsSuspending = true ;
#endif
			if( m_MuteInBackground == true )
			{
				// バックグラウンド時に完全に消音が有効
				CriAtomExAsr.SetBusVolume( "MasterOut", 0f ) ;
			}

			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( m_RunInBackground == true )
			{
				return true ;
			}

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						PauseChannelFading( Channels[ i ] ) ;

						Channels[ i ].Pause() ;	// 一時停止

						Channels[ i ].Suspend() ;
					}
				}
			}

			return true ;
		}

		// 全レジューム
		private bool ResumeAll_Private()
		{
#if UNITY_EDITOR
			m_IsSuspending = false ;
#endif
			if( m_MuteInBackground == true )
			{
				// バックグラウンド時に完全に消音が有効
#if UNITY_EDITOR
				CriAtomExAsr.SetBusVolume( "MasterOut", EditorUtility.audioMasterMute ? 0f : AudioListener.volume * MasterVolume ) ;
#else
				CriAtomExAsr.SetBusVolume( "MasterOut", AudioListener.volume * MasterVolume ) ;
#endif
			}

			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( m_RunInBackground == true )
			{
				return true ;
			}

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPausing == true && Channels[ i ].IsSuspending == true )
					{
						// リプレイ状態になるのでベースタイムとベースボリュームを更新する
						UnpauseChannelFading( Channels[ i ] ) ;

						Channels[ i ].Unpause() ;	// 一時停止解除

						Channels[ i ].Resume() ;
					}
				}
			}

			return true ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// 全オーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteAll( bool state )
		{
			return MuteWithTag( state, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="state">ミュート状態(true=オン・false=オフ)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteWithTag( bool state, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.MuteWithTag_Private( state, tag ) ;
		}

		// タグで指定されたオーディオチャンネルのミュート状態を設定する
		private bool MuteWithTag_Private( bool state, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( Channels[ i ].IsPlaying == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
						{
							Channels[ i ].Mute = state ;	// ミュート設定
						}
					}
				}
			}

			// タグ指定でミュートリストに保存する
			if( string.IsNullOrEmpty( tag ) == false )
			{
				if( state == true )
				{
					// オン
					if( m_MuteChannels.Contains( tag ) == false )
					{
						m_MuteChannels.Add( tag ) ;
					}
				}
				else
				{
					// オフ
					if( m_MuteChannels.Contains( tag ) == true )
					{
						m_MuteChannels.Remove( tag ) ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return StopAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopAll_Private( tag ) ;
		}

		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopAll_Private( string tag )
		{
			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
					{
						// 再生中のチャンネルを発見した
						audioChannel.Stop() ;

						//-----------

						// フェードリストから除外する
						RemoveChannelFading( audioChannel ) ;

						// 即時呼び出す
						if( audioChannel.IsUsing == true )
						{
							// 以後、のチャンネルに対する操作はできない
							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

							if( string.IsNullOrEmpty( audioChannel.CueSheetName ) == false )
							{
								// キューシートに対する参照カウントを減らす
								if( m_CueSheetCache_Hash.ContainsKey( audioChannel.CueSheetName ) == true )
								{
									m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;
								}
								else
								{
									Debug.LogWarning( "指定のキューシート[ " + audioChannel.CueSheetName + " ]がキャッシュに見つかりません。" ) ;
									Debug.LogWarning( "キュー[ " +  audioChannel.CueName + " ]がまだ再生中に、明示的にキュシート[ " + audioChannel.CueSheetName + " ]が破棄された可能性があります。" ) ;
									Debug.LogWarning( "キューシート[ " + audioChannel.CueSheetName + " ]を明示的に破棄する前に、先にキュー[ " + audioChannel.CueName + " ]を停止してください。" ) ;
									Debug.Log( "==========================" ) ;
									Debug.Log( "現在のキューシートの登録数 : " + m_CueSheetCache_Hash.Count ) ;
									foreach( var item in m_CueSheetCache_Hash )
									{
										Debug.Log( "キャッシュされたキューシート : <color=#00FF00>" + item.Key.ToString() + "</color>" ) ;
									}
								}
							}

							// 以後は完全に発音が停止するまでチャンネルへの操作はできない
							audioChannel.Clear() ;

							audioChannel.IsUsing = false ;
						}
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFadeAll( float fadeDuration )
		{
			return StopFadeAll( fadeDuration, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFadeAll( float fadeDuration, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopFadeAll_Private( fadeDuration, tag ) ;
		}

		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopFadeAll_Private( float fadeDuration, string tag )
		{
			if( fadeDuration <= 0 )
			{
				return StopAll_Private( tag ) ;
			}

			//----------------------------

			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
					{
//						Debug.Log( "<color=#FFFF00>------>フェードアウト対象1:" + audioChannel.CueName + "</color>" ) ;

						// 再生中のチャンネルを発見した
						StopFade( audioChannel.PlayId, fadeDuration ) ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを一時停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return PauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを一時停止させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.PauseAll_Private( tag ) ;
		}

		// タグで指定されたオーディオチャンネルを一時停止させる
		private bool PauseAll_Private( string tag )
		{
			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && audioChannel.IsPausing == false )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
						{
							// ポーズ状態になるのでベースタイムとベースボリュームを更新する
							PauseChannelFading( audioChannel ) ;

							audioChannel.Pause() ;	// 一時停止
						}
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return UnpauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll( string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.UnpauseAll_Private( tag ) ;
		}

		// タグで指定されたオーディオチャンネルの一時停止を解除させる
		private bool UnpauseAll_Private( string tag )
		{
			AudioChannel_ADX2 audioChannel ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == true && audioChannel.IsPausing == true )
					{
						if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && audioChannel.Tag == tag ) )
						{
							// ポーズ解除状態になるのでベースタイムとベースボリュームを更新する
							UnpauseChannelFading( audioChannel ) ;

							audioChannel.Unpause() ;	// 再開
						}
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全にクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearAll()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearAll_Private() ;
		}

		// 全オーディオチャンネルを完全にクリアする
		private bool ClearAll_Private()
		{
			StopAll_Private( null ) ;

			int i, l = Channels.Count ;
			for( i  = l - 1 ; i >= 0 ; i -- )
			{
				Channels[ i ]?.Destroy() ;
			}

			m_FadeStopChannels.Clear() ;

			m_FadePlayChannels.Clear() ;

			m_MuteChannels.Clear() ;

			Channels.Clear() ;

			return true ;
		}

		// --------------------------------------------------------

		/// <summary>
		/// 全体共通のミュート状態を設定する
		/// </summary>
		/// <param name="state">全体共通のミュート状態(true=オン・false=オフ)</param>
		public static void Mute( bool state )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_Mute = state ;

			if( m_Instance.m_Mute == false )
			{
				CriAtomExAsr.SetBusVolume( "MasterOut", AudioListener.volume * m_Instance.MasterVolume ) ;
			}
			else
			{
				AudioListener.volume = 0 ;
			}
		}

		/// <summary>
		/// マスターボリュームを設定する
		/// </summary>
		/// <param name="volume">マスターボリューム(0～1)</param>
		public static void SetMasterVolume( float masterVolume )
		{
			if( m_Instance == null )
			{
				return ;
			}

			if( masterVolume >  1 )
			{
				masterVolume  = 1 ;
			}
			else
			if( masterVolume <  0 )
			{
				masterVolume  = 0 ;
			}

			m_Instance.MasterVolume = masterVolume ;

			if( m_Instance.m_Mute == false )
			{
				CriAtomExAsr.SetBusVolume( "MasterOut", AudioListener.volume * m_Instance.MasterVolume ) ;
			}
		}

		/// <summary>
		/// 全オーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume )
		{
			return SetAllVolumes( volume, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="tag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float volume, string tag )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetAllVolumes_Private( volume, tag ) ;
		}

		// タグで指定されたオーディオチャンネルのボリュームを設定する
		private bool SetAllVolumes_Private( float volume, string tag )
		{
			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						Channels[ i ].Volume = volume ;	// 一時停止
					}
				}
			}

			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ダグで指定されたベースボリュームを設定する
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="baseVolume"></param>
		/// <returns></returns>
		public static bool SetBaseVolume( string tag, float baseVolume )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetBaseVolume_Private( tag, baseVolume ) ;
		}

		// タグで指定されたベースボリュームを設定する
		private bool SetBaseVolume_Private( string tag, float baseVolume )
		{
			AddBaseVolume_Private( tag, baseVolume ) ;

			int i, l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tag ) == true || ( string.IsNullOrEmpty( tag ) == false && Channels[ i ].Tag == tag ) )
					{
						Channels[ i ].BaseVolume = baseVolume ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// ダグで指定されたベースボリュームを取得する
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static float GetBaseVolume( string tag )
		{
			if( m_Instance == null )
			{
				return 1 ;
			}

			return m_Instance.GetBaseVolume_Private( tag ) ;
		}

		//-----

		/// <summary>
		/// タグごとの最大チャンネル数を設定する
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="maxChannels"></param>
		/// <returns></returns>
		public static bool SetMaxChannels( string tag, int maxChannels )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetMaxChannels_Private( tag, maxChannels ) ;
		}

		// タグごとの最大チャンネル数を設定する
		private bool SetMaxChannels_Private( string tag, int maxChannels )
		{
			AddMaxChannels_Private( tag, maxChannels ) ;

			return true ;
		}

		/// <summary>
		/// タグごとの最大チャンネル数を取得する
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static int GetMaxChannels( string tag )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}

			return m_Instance.GetMaxChannels_Private( tag ) ;
		}

		//---------------------------------------------------------

		// 毎フレームの処理を行う
		private void Process()
		{
			// 現在の時間を取得する
			float currentTime = Time.realtimeSinceStartup ;

			int i, l ;
			AudioChannel_ADX2 audioChannel ;

			//-----------------------------------------------------

			// プレイフェードを処理する

			if( m_FadePlayChannels.Count >  0 )
			{
				l = m_FadePlayChannels.Count ;
				var audioChannels = new AudioChannel_ADX2[ l ] ;

				m_FadePlayChannels.Keys.CopyTo( audioChannels, 0 ) ;

				float deltaTime ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannels[ i ] ;

					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する

						deltaTime = currentTime - m_FadePlayChannels[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadePlayChannels[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadePlayChannels[ audioChannel ].StartVolume + ( ( m_FadePlayChannels[ audioChannel ].EndVolume - m_FadePlayChannels[ audioChannel ].StartVolume ) * deltaTime / m_FadePlayChannels[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了
							audioChannel.FadeVolume = m_FadePlayChannels[ audioChannel ].EndVolume ;

							// リストから除外する
							m_FadePlayChannels.Remove( audioChannel ) ;
						}
					}
				}
			}

			//-----------------------------

			// ストップフェードを処理する

			if( m_FadeStopChannels.Count >  0 )
			{
				l = m_FadeStopChannels.Count ;
				var audioChannels = new AudioChannel_ADX2[ l ] ;

				m_FadeStopChannels.Keys.CopyTo( audioChannels, 0 ) ;

				float deltaTime ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					audioChannel = audioChannels[ i ] ;

					if( audioChannel.IsPlaying == true )
					{
						// 再生中のみ処理する

						deltaTime = currentTime - m_FadeStopChannels[ audioChannel ].StartTime ;
						if( deltaTime <  m_FadeStopChannels[ audioChannel ].Duration )
						{
							// まだ続く
							audioChannel.FadeVolume = m_FadeStopChannels[ audioChannel ].StartVolume + ( ( m_FadeStopChannels[ audioChannel ].EndVolume - m_FadeStopChannels[ audioChannel ].StartVolume ) * deltaTime / m_FadeStopChannels[ audioChannel ].Duration ) ;
						}
						else
						{
							// 終了

//							Debug.Log( "<color=#FFFF00>---------->フェードアウト終了:" + audioChannel.CueName + "</color>" ) ;

							// 再生も停止する
							audioChannel.Stop() ;

							audioChannel.FadeVolume = m_FadeStopChannels[ audioChannel ].EndVolume ;

							// リストから除外する
							m_FadeStopChannels.Remove( audioChannel ) ;

				//			Debug.Log( "フェードストップが終わったので破棄:" + mFadeStopList.Count ) ;

							//------------------------------

							// 即時呼び出す(Stop()実行後に即座にStatus==Stopとはならないため、このタイミングでそのチャンネルへの操作が一切できないようにする)
							if( audioChannel.IsUsing == true )
							{
//								Debug.Log( "<color=#FFFF00>--------->チャンネルから完全に破棄:" + audioChannel.CueName + "</color>" ) ;

								// 以後、のチャンネルに対する操作はできない
								OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

								// キューシートに対する参照カウントを減らす
								m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;

								// 以後は完全に発音が停止するまでチャンネルへの操作はできない
								audioChannel.Clear() ;

								audioChannel.IsUsing = false ;
							}
						}
					}
				}
			}

			//------------------------------------------------------------------------------------------

			// 終了したチャンネルを監視してコールバックを発生させる
			l = Channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Channels[ i ] != null )
				{
					audioChannel = Channels[ i ] ;
					if( audioChannel.IsPlaying == false )
					{
						if( audioChannel.IsUsing == true )
						{
							// ワンショット再生の自然停止待ちだとここで再生終了をチェックするしかない
//							Debug.Log( "<color=#FFFF00>--------->チャンネルから完全に破棄:" + audioChannel.CueName + "</color>" ) ;

							OnStoppedDelegate?.Invoke( audioChannel.PlayId ) ;

							// キューシートに対する参照カウントを減らす
							m_CueSheetCache_Hash[ audioChannel.CueSheetName ].Count -- ;

							// 以後は完全に発音が停止するまでチャンネルへの操作はできない
							audioChannel.Clear() ;

							audioChannel.IsUsing = false ;	// 多重にコールバックが呼ばれてしまうと問題なので１回呼んだらフラグを落として２回以上呼ばれないようにする
						}

						//-------------------------------

						// 真の意味でチャンネルが解放された
						audioChannel.Busy = false ;
					}
					else
					{
						// 再生中のチャンネルに対して３Ｄ空間再生の場合はソース(音源)の相対位置を更新する
						audioChannel.UpdateReletivePosition3d() ;
					}
				}
			}
		}

		// ポーズ状態になるのでベースタイムとベースボリュームを更新する
		private void PauseChannelFading( AudioChannel_ADX2 audioChannel )
		{
			float currentTime = Time.realtimeSinceStartup ;
			float deltaTime ;

			// プレイリスト
			if( m_FadePlayChannels.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadePlayChannels[ audioChannel ].StartTime ;

				if( deltaTime <  m_FadePlayChannels[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadePlayChannels[ audioChannel ].Duration   = m_FadePlayChannels[ audioChannel ].Duration - deltaTime ;

					m_FadePlayChannels[ audioChannel ].StartTime   = currentTime ;
					m_FadePlayChannels[ audioChannel ].StartVolume = audioChannel.FadeVolume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadePlayChannels[ audioChannel ].EndVolume ;	// 最終的なボリューム
					m_FadePlayChannels.Remove( audioChannel ) ;	// リストから破棄
				}
			}

			// ストップリスト
			if( m_FadeStopChannels.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				deltaTime = currentTime - m_FadeStopChannels[ audioChannel ].StartTime ;

				if( deltaTime <  m_FadeStopChannels[ audioChannel ].Duration )
				{
					// まだ続く
					m_FadeStopChannels[ audioChannel ].Duration   = m_FadeStopChannels[ audioChannel ].Duration - deltaTime ;

					m_FadeStopChannels[ audioChannel ].StartTime   = currentTime ;
					m_FadeStopChannels[ audioChannel ].StartVolume = audioChannel.FadeVolume ;
				}
				else
				{
					// 終了
					audioChannel.FadeVolume = m_FadeStopChannels[ audioChannel ].EndVolume ;	// 最終的なボリューム（ストップの場合は０）

					m_FadeStopChannels.Remove( audioChannel ) ;

					// ここに来る事はあまり考えられないが念のため
					audioChannel.Stop() ;
				}
			}
		}

		// リプレイ状態になるのでベースタイムとベースボリュームを更新する
		private void UnpauseChannelFading( AudioChannel_ADX2 audioChannel )
		{
			float currentTime = Time.realtimeSinceStartup ;

			// プレイリスト
			if( m_FadePlayChannels.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadePlayChannels[ audioChannel ].StartTime   = currentTime ;
			}

			// ストップリスト
			if( m_FadeStopChannels.ContainsKey( audioChannel ) == true )
			{
				// 対象のソース
				m_FadeStopChannels[ audioChannel ].StartTime   = currentTime ;
			}
		}

		// フェードリストから除外する
		private void RemoveChannelFading( AudioChannel_ADX2 audioChannel )
		{
			if( m_FadePlayChannels.ContainsKey( audioChannel ) == true )
			{
				m_FadePlayChannels.Remove( audioChannel ) ;
			}

			if( m_FadeStopChannels.ContainsKey( audioChannel ) == true )
			{
				m_FadeStopChannels.Remove( audioChannel ) ;
			}
		}

		//-------------------------------------------------------------------------------------
		// 固有拡張機能群

		/// <summary>
		/// バイノーラライザーの有効化の設定を行う
		/// </summary>
		/// <param name="isEnabled"></param>
		public static void EnableBinauralizer( bool isEnabled )
		{
			CriAtomExAsr.EnableBinauralizer( isEnabled ) ; 
		}
	}

	//--------------------------------------------------------------------------------------------

	// AudioChannel

	/// <summary>
	/// オーディオチャンネルクラス(オーディオソースの機能拡張クラス)
	/// </summary>
	[Serializable]
	public class AudioChannel_ADX2
	{
		/// <summary>
		/// キューシート名
		/// </summary>
		public string	CueSheetName ;

		/// <summary>
		/// キュー名
		/// </summary>
		public string	CueName ;

		/// <summary>
		/// 発音中の音のユニークな識別子
		/// </summary>
		public int		PlayId ;

		/// <summary>
		/// タグ名
		/// </summary>
		public string	Tag ;	// タグ

		/// <summary>
		/// チャンネルを使用したかどうか
		/// </summary>
		public bool		IsUsing = false ;

		/// <summary>
		/// 内部的に使用しているか
		/// </summary>
		public bool		Busy	= false ;

		//---------------------------------

		// オーディオソースのインスタンス
		private CriAtomSource   m_AudioSource ;

		/// <summary>
		/// 低遅延モードが有効かどうか
		/// </summary>
		private readonly bool   m_EnableLowLatency ;

		/// <summary>
		/// ロック状態
		/// </summary>
		private bool            m_IsLocking ;

		/// <summary>
		/// サスペンド状態
		/// </summary>
		private bool            m_IsSuspending ;

		/// <summary>
		/// ミュート
		/// </summary>
		private bool            m_Mute = false ;

		//-----------------------------------

		/// <summary>
		/// ベースボリーム
		/// </summary>
		private float           m_BaseVolume    = 1.0f ;

		/// <summary>
		/// ボリーム
		/// </summary>
		private float           m_Volume        = 1.0f ;

		/// <summary>
		/// ステップボリューム
		/// </summary>
		private float           m_StepVolume    = 1.0f ;

		/// <summary>
		/// フェードボリューム
		/// </summary>
		private float           m_FadeVolume    = 1.0f ;

		/// <summary>
		/// ピッチ
		/// </summary>
		private float           m_Pitch         = 0.0f ;

		//-----------------------------------------------------

		//-----------------------------------------------------
		// リアルタイム３Ｄ空間追従用

		// ソース(音源)トランスフォーム
		private Transform   m_SourceTransform ;

		// リスナー(聴き手)トランスフォーム
		private Transform   m_ListenerTransform ;

		// 本当のリスナー(聴き手)トランスフォーム
		private Transform   m_TrueListenerTransform ;

		// 距離補正係数
		private float       m_DistanceScale ;

		// 毎フレームソース(音源)の位置を更新するか
		private bool        m_IsRealTimeUpdating ;

		//-----------------------------

		// 有効なソース(音源)の絶対位置は格納されているか
		private bool        m_SourcePositionEnabled ;

		// 最後のソース(音源)の絶対位置
		private Vector3     m_SourcePosition ;

		// 有効なソース(音源)の相対位置は格納されているか
		private bool        m_RelativePositionEnabled ;

		// 有効なソース(音源)の相対位置
		private Vector3     m_RelativePosition ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="audioSource">オーディオソースのインスタンス</param>
		internal protected AudioChannel_ADX2( CriAtomSource audioSource, bool enableLowLatency )
		{
			m_AudioSource		= audioSource ;
			m_EnableLowLatency	= enableLowLatency ;
		}

		/// <summary>
		/// オーディオチャンネルの破棄を行う
		/// </summary>
		internal protected void Destroy()
		{
			if( m_AudioSource != null )
			{
				GameObject.Destroy( m_AudioSource.gameObject ) ;
				m_AudioSource = null ;
			}
		}

		/// <summary>
		/// ミュート状態
		/// </summary>
		internal protected bool Mute
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}

				return m_Mute ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_Mute = value ;
				if( m_Mute == true )
				{
					m_AudioSource.volume = 0 ;
				}
				else
				{
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// 総合的なループ状態
		/// </summary>
		internal protected bool Loop
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}

				return m_AudioSource.loop | SourceLoop ;
			}
		}

		/// <summary>
		/// ソースのループ状態
		/// </summary>
		internal protected bool SourceLoop
		{
			get
			{
				if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
				{
//					Debug.LogWarning( "Bad Setting : CueSheetName = " + CueSheetName + " CueName = " + CueName ) ;
					return false ;
				}

				//---------------------------------

				// キューシート上にそのキュー名があるのか
				CriAtomExAcb acb = CriAtom.GetAcb( CueSheetName ) ;
				if( acb == null )
				{
//					Debug.LogWarningFormat( m_AudioSource, "[サウンド] キューシート '{0}' を読み込めませんでした", CueSheetName ) ;
					return false ;
				}

				if( acb.GetCueInfo( CueName, out CriAtomEx.CueInfo info ) == false )
				{
//					Debug.LogWarningFormat
//					(
//						m_AudioSource,
//						"[サウンド] キュー名 '{0}' が キューシート '{1}' から読み取れませんでした",
//						CueName,
//						CueSheetName
//					) ;
					return false ;
				}

//				if( acb.GetWaveFormInfo( CueName, out CriAtomEx.WaveformInfo waveInfo ) == true )
//				{
//					// ウェーブ情報
//				}

				return ( info.length <  0 ) ;
			}
		}

		/// <summary>
		/// ベースボリューム(0～1)
		/// </summary>
		internal protected float BaseVolume
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}

				return m_Volume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_BaseVolume = value ;
				m_FadeVolume = 1 ;
				if( m_Mute == false )
				{
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float Volume
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}

				return m_Volume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_Volume = value ;
				m_FadeVolume = 1 ;
				if( m_Mute == false )
				{
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float StepVolume
		{
			get
			{
				return m_StepVolume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_StepVolume = value ;
				if( m_Mute == false )
				{
					UpdateVolume() ;
				}
			}
		}

		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float FadeVolume
		{
			get
			{
				return m_FadeVolume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_FadeVolume = value ;
				if( m_Mute == false )
				{
					UpdateVolume() ;
				}
			}
		}

		// ボリュームを更新する
		private void UpdateVolume()
		{
			if( m_AudioSource == null )
			{
				return ;
			}

			if( m_Mute == false )
			{
				m_AudioSource.volume = m_BaseVolume * m_Volume * m_StepVolume * m_FadeVolume ;
			}
		}

		//---------

		/// <summary>
		/// ピッチ(０でオリジナル・－１で１オクターブ↓・＋１で１オクターブ↑)
		/// </summary>
		internal protected float Pitch
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}

				return m_Pitch ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}

				m_Pitch = value ;
				m_AudioSource.pitch		= m_Pitch * 1200.0f ;   // １オクターブは 1200 になっている
			}
		}

		//-----------------------------------

		/// <summary>
		/// 再生位置(秒)
		/// </summary>
		internal protected float Time
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}

				return ( float )m_AudioSource.time / 1000.0f ;
			}
		}

		/// <summary>
		/// 再生位置(サンプル)
		/// </summary>
		internal protected int TimeSamples
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}

				if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
				{
					return 0 ;

				}

				// キューシート上にそのキュー名があるのか
				CriAtomExAcb acb = CriAtom.GetAcb( CueSheetName ) ;
				if( acb == null )
				{
					return 0 ;
				}

//				if( acb.GetCueInfo( CueName, out CriAtomEx.CueInfo cueInfo ) == false )
//				{
//					return 0 ;
//				}

				if( acb.GetWaveFormInfo( CueName, out CriAtomEx.WaveformInfo waveformInfo ) == false )
				{
					return 0 ;
				}

				//---------------------------------

				return ( int )( waveformInfo.samplingRate * m_AudioSource.time / 1000 ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// サウンドを再生する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="cueSheetName">キューシート名</param>
		/// <param name="cueName">キュー名</param>
		/// <param name="loop">ループの有無(true=する・false=しない)</param>
		/// <param name="volume">ボリューム(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="pitch">ピッチ(-1=1オクターブ下～0=通常～+1=1オクターブ上)</param>
		/// <param name="tag">タグ名</param>
		internal protected void Play
		(
			int playId,
			string cueSheetName, string cueName,
			bool loop,
			float baseVolume, float volume,
			float pan,
			Transform sourceTransform, Transform listenerTransform, Transform trueListenerTransform, float distanceScale, bool isRealTimeUpdating,
			float pitch,
			string selector, string label,
			string tag,
			bool isStreaming
		)
		{
			if( m_AudioSource == null || string.IsNullOrEmpty( cueSheetName ) == true || string.IsNullOrEmpty( cueName ) == true )
			{
				return ;
			}

			// ＳＥなどの音は消えてもチャンネルとしては動き続けている状態で別の音を鳴らすとループ設定が効かないので一度明示的に停止させる必要がある
			m_AudioSource.Stop() ;

			//----------------------------------------------------------

			m_SourceTransform       = sourceTransform ;
			m_ListenerTransform     = listenerTransform ;
			m_TrueListenerTransform = trueListenerTransform ;
			m_DistanceScale         = distanceScale ;
			m_IsRealTimeUpdating    = isRealTimeUpdating ;

			Vector3 sourcePosition = GetRelativePosition3d() ;

			//----------------------------------

			PlayId = playId ;

			CueSheetName	= cueSheetName ;
			CueName			= cueName ;
			Tag				= tag ;

			// キューシート上にそのキュー名があるのか
			CriAtomExAcb acb = CriAtom.GetAcb( cueSheetName ) ;
			if( acb == null )
			{
				Debug.LogWarningFormat( m_AudioSource, "[サウンド] キューシート '{0}' を読み込めませんでした", cueSheetName ) ;
				return ;
			}

			if( acb.GetCueInfo( cueName, out CriAtomEx.CueInfo cueInfo ) == false )
			{
				Debug.LogWarningFormat
				(
					m_AudioSource,
					"[サウンド] キュー名 '{0}' が キューシート '{1}' から読み取れませんでした",
					cueName,
					cueSheetName
				) ;
				return ;
			}

			if( cueInfo.length <  0 )
			{
				// ソースがループなのでチャンネルのループは無効化する
				loop = false ;
			}

			//----------------------------------

			m_Pitch = pitch ;
			m_AudioSource.pitch		= m_Pitch * 1200.0f ;

			m_AudioSource.cueSheet	= cueSheetName ;
			m_AudioSource.cueName	= cueName ;

			// ループ
			m_AudioSource.loop		= loop ;

			// Android 限定：遅延軽減を有効化する(ストリーミング再生では音が鳴らなくなってしまうので注意)
			m_AudioSource.androidUseLowLatencyVoicePool = m_EnableLowLatency && !isStreaming ;

			//-------------------------
			// ノイズが乗るので再生前にもボリュームを設定しておく

			// ベースボリューム
			m_BaseVolume	= baseVolume ;

			// ボリューム
			m_Volume		= volume ;

			// ステップボリューム
			m_StepVolume    = 1 ;

			// フェードボリューム
			m_FadeVolume	= 1 ;

			// ボリュームをチャンネルに反映させる
			UpdateVolume() ;

			//-------------------------

			if( pan >   1 )
			{
				pan  =  1 ;
			}
			else
			if( pan <  -1 )
			{
				pan  = -1 ;
			}

			// パン
			m_AudioSource.pan3dAngle = pan * 180.0f ;

			// 位置
			m_AudioSource.gameObject.transform.position = sourcePosition ;

			//-------------------------
			// セレクターの指定があるかどうか

			if( string.IsNullOrEmpty( selector ) == false && string.IsNullOrEmpty( label ) == false )
			{
				m_AudioSource.player.SetSelectorLabel( selector, label ) ;
			}
			else
			{
				m_AudioSource.player.ClearSelectorLabels() ;
			}

			//-------------------------

			// 再生
			m_AudioSource.Play() ;

#if UNITY_EDITOR
			if( m_AudioSource.status == CriAtomSource.Status.Error )
			{
				Debug.LogWarning( "Could not play : CueSheetName = " + cueSheetName + " CueName = " + CueName + "</color>" ) ;
				var vp = CriAtomExVoicePool.GetNumUsedVoices( CriAtomExVoicePool.VoicePoolId.StandardMemory ) ;
				Debug.LogWarning( "Voice Pool : " + vp.numUsedVoices + " / " + vp.numPoolVoices ) ;
			}
#endif
			//--------------

			// 念のためプレイ実行後にも再度反映しておく
			UpdateVolume() ;

			//-----------------------------------------------------

			IsUsing = true ;	// 最後にクリーンアップされたかの判定にも使用する

			Busy	= true ;	// 内部的に使用している状態とする
		}

		// ３Ｄポジョニング再生の参考:https://game.criware.jp/learn/tutorial/unity/unity_shokyu_04/

		// リスナーからみたソースの相対位置を取得する
		private Vector3 GetRelativePosition3d()
		{
			if( m_ListenerTransform == null || m_TrueListenerTransform == null )
			{
				if( m_RelativePositionEnabled == true )
				{
					return m_RelativePosition ;
				}
				else
				{
					return Vector3.zero ;
				}
			}

			//-------------------------------------------------

			// 音源の位置(ワールド座標系)
			Vector3 sourcePosition ;

			if( m_SourceTransform != null )
			{
				sourcePosition = m_SourceTransform.position ;

				m_SourcePosition = sourcePosition ;

				// 有効な音源の絶対位置が格納されている
				m_SourcePositionEnabled = true ;
			}
			else
			{
				if( m_SourcePositionEnabled == true )
				{
					// 最後のソースの絶対位置
					sourcePosition = m_SourcePosition ;
				}
				else
				{
					sourcePosition = m_ListenerTransform.position ;
				}
			}

			//-------------------------------------------------

			// 音源の座標を真のリスナーのローカル座標系に変換する
			sourcePosition = m_ListenerTransform.InverseTransformPoint( sourcePosition ) ;

			// スケール調整を行う
			sourcePosition *= m_DistanceScale ;

			// 音源の座標を真のリスナーのワールド座標系に変換する
			m_RelativePosition = m_TrueListenerTransform.TransformPoint( sourcePosition ) ;

			// 有効な音源の相対位置が格納されている
			m_RelativePositionEnabled = true ;

			// 結果として真のリスナーからの相対位置を返す
			return m_RelativePosition ;
		}

		/// <summary>
		/// 真のリスナーからの相対的な位置を更新する
		/// </summary>
		public void UpdateReletivePosition3d()
		{
			if( m_IsRealTimeUpdating == false )
			{
				return ;
			}

			m_AudioSource.gameObject.transform.position = GetRelativePosition3d() ;
		}

		/// <summary>
		/// サウンドを完全停止する
		/// </summary>
		internal protected void Stop()
		{
			if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
			{
				// このチャンネルに対する操作はできない
				return ;
			}

			//----------------------------------

			if( m_AudioSource == null )
			{
				return ;
			}

//			Debug.Log( "<color=#FFFF00>------>Stop直前の状態:" + CueName + " Status = " + m_AudioSource.status + "</color>" ) ;

			if( m_AudioSource.status == CriAtomSource.Status.Prep || m_AudioSource.status == CriAtomSource.Status.Playing )
			{
				m_AudioSource.Stop() ;
			}

//			Debug.Log( "<color=#FFFF00>------>Stop直後の状態:" + CueName + " Status = " + m_AudioSource.status + "</color>" ) ;

			// ストップでボーズもビジィも解除される
			m_IsLocking	= false ;
		}

		/// <summary>
		/// サウンドを一時停止する
		/// </summary>
		internal protected void Pause()
		{
			if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
			{
				// このチャンネルに対する操作はできない
				return ;
			}

			//----------------------------------

			if( m_AudioSource == null )
			{
				return ;
			}

			if( m_AudioSource.status == CriAtomSource.Status.Playing && m_AudioSource.IsPaused() == false )
			{
				m_AudioSource.Pause( true ) ;
			}
		}

		/// <summary>
		/// サウンドの一時停止を解除する
		/// </summary>
		internal protected void Unpause()
		{
			if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
			{
				// このチャンネルに対する操作はできない
				return ;
			}

			//----------------------------------

			if( m_AudioSource == null )
			{
				return ;
			}

			if( m_AudioSource.status == CriAtomSource.Status.Playing && m_AudioSource.IsPaused() == true )
			{
				m_AudioSource.Pause( false ) ;
			}
		}

		//-----------------------------------------------------------

		internal protected void Lock()
		{
			m_IsLocking = true ;
		}

		internal protected void Unlock()
		{
			m_IsLocking = false ;
		}

		internal protected void Suspend()
		{
			m_IsSuspending = true ;
		}

		internal protected void Resume()
		{
			m_IsSuspending = false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 情報を消去する
		/// </summary>
		internal protected void Clear()
		{
			// 情報をクリアする

			if( m_AudioSource != null )
			{
				// チャンネルのキューシート名とキュー名をクリアするのが重要なのかもしれない
				m_AudioSource.cueSheet	= null ;
				m_AudioSource.cueName	= null ;

				m_AudioSource.loop		= false ;
//				m_AudioSource.volume	= 1 ;		// 即座に停止しない場合があるためボリュームをリセットしてはならない
			}

			//--------------
			// 以後、このチャンネルに対する操作はできない

			CueSheetName	= null ;
			CueName			= null ;

			Tag				= null ;

			PlayId			= -1 ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ステータス
		/// </summary>
		internal protected string Status
		{
			get
			{
				if( m_AudioSource == null )
				{
					return CriAtomSource.Status.Error.ToString() ;
				}

				return m_AudioSource.status.ToString() ;
			}
		}

		/// <summary>
		/// ステータス
		/// </summary>
		internal protected CriAtomSource.Status NativeStatus
		{
			get
			{
				if( m_AudioSource == null )
				{
					return CriAtomSource.Status.Error ;
				}

				return m_AudioSource.status ;
			}
		}

		/// <summary>
		/// 実際に再生中かの判定を行う
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsPlaying
		{
			get
			{
				// Stop() を実行しても実際に Status = Stop になるまではかなりの時間を要する場合がある。
				// よって、Stop() 実行後、そのチャンネルは無効なチャンネルとするが、
				// 再利用はすぐには出来ないようにする。

				//---------------------------------

//				if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
//				{
//					// このチャンネルに対する操作はできない
//					return false ;
//				}

				//----------------------------------

				if( m_AudioSource == null )
				{
					return false ;
				}

				// 再利用可能な状態は、PlayEnd Stop Error のみ
				return ( m_AudioSource.status == CriAtomSource.Status.Prep || m_AudioSource.status == CriAtomSource.Status.Playing ) ;
			}
		}

		/// <summary>
		/// 一時停止中か判定を行う
		/// </summary>
		/// <returns>結果(true=一時停止中・false=一時停止中ではない)</returns>
		internal protected bool IsPausing
		{
			get
			{
				if( string.IsNullOrEmpty( CueSheetName ) == true || string.IsNullOrEmpty( CueName ) == true )
				{
					// このチャンネルに対する操作はできない
					return false ;
				}

				//----------------------------------

				if( m_AudioSource == null )
				{
					return false ;
				}

				return m_AudioSource.IsPaused() ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ロック中かどうか
		/// </summary>
		internal protected bool IsLocking
		{
			get
			{
				return m_IsLocking ;
			}
		}

		/// <summary>
		/// 中断中かどうか
		/// </summary>
		internal protected bool IsSuspending
		{
			get
			{
				return m_IsSuspending ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ＡＤＸ固有の機能

		/// <summary>
		/// バスセンドレベルを設定する
		/// </summary>
		/// <param name="busName"></param>
		/// <param name="sendLevel"></param>
		/// <returns></returns>
		internal protected bool SetBusSendLevel( string busName, float sendLevel = 0 )
		{
			if( m_AudioSource == null )
			{
				return false ;
			}

			m_AudioSource.SetBusSendLevel( busName, sendLevel ) ;

			return true ;
		}

		/// <summary>
		/// Ａｉｓｃコントロールを設定する
		/// </summary>
		/// <param name="controlName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal protected bool SetAiscControl( string controlName, float value )
		{
			if( m_AudioSource == null )
			{
				return false ;
			}

			m_AudioSource.SetAisacControl( controlName, value ) ;

			return true ;
		}

		/// <summary>
		/// セレクターラベルを設定する
		/// </summary>
		/// <param name="selector"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		internal protected bool SetSelectorLabel( string selector, string label )
		{
			if( m_AudioSource == null )
			{
				return false ;
			}

			if( string.IsNullOrEmpty( selector ) == false && string.IsNullOrEmpty( label ) == false )
			{
				m_AudioSource.player.SetSelectorLabel( selector, label ) ;
			}
			else
			if( string.IsNullOrEmpty( selector ) == false && string.IsNullOrEmpty( label ) == true )
			{
				m_AudioSource.player.UnsetSelectorLabel( selector ) ;
			}
			else
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// セレクターラベルを削除する
		/// </summary>
		/// <param name="selector"></param>
		/// <returns></returns>
		internal protected bool UnsetSelectorLabel( string selector )
		{
			if( m_AudioSource == null )
			{
				return false ;
			}

			if( string.IsNullOrEmpty( selector ) == true )
			{
				return  false ;
			}

			m_AudioSource.player.UnsetSelectorLabel( selector ) ;

			return true ;
		}

		/// <summary>
		/// セレクターラベルを全て削除する
		/// </summary>
		/// <returns></returns>
		internal protected bool ClearSelectorLabels()
		{
			if( m_AudioSource == null )
			{
				return false ;
			}

			m_AudioSource.player.ClearSelectorLabels() ;

			return true ;
		}


		//-------------------------------------------------------------------------------------
	}
}

#endif
