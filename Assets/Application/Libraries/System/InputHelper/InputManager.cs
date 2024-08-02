using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem ;
#endif

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace InputHelper
{
	/// <summary>
	/// 入力操作クラス Version 2024/08/02 0
	/// </summary>
	[DefaultExecutionOrder( -90 )]
	public partial class InputManager : MonoBehaviour
	{
		// 注意：パッド系の操作をした場合、一度パッド系の操作を完全解除するまで、実際のバッド系入力ができないようにする事も可能になっている。→modeEnabled

#if UNITY_EDITOR
		/// <summary>
		/// InputManager を生成
		/// </summary>
		[MenuItem( "GameObject/Helper/InputHelper/InputManager", false, 24 )]
		public static void CreateInputManager()
		{
			var go = new GameObject( "InputManager" ) ;
			go.AddComponent<InputManager>() ;
		
			var t = go.transform ;
			t.SetParent( null ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;
		
			Selection.activeGameObject = go ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		// インプットマネージャのインスタンス(シングルトン)
		private static InputManager m_Instance = null ; 

		/// <summary>
		/// インプットマネージャのインスタンス(シングルトン)
		/// </summary>
		public  static InputManager   Instance => m_Instance ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// インプットマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="inputSystemEnabled"></param>
		/// <returns></returns>
		public static InputManager Create( Transform parent = null, bool inputSystemEnabled = false )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
			
#if UNITY_EDITOR
			if( InputManagerSettings.Check() == false )
			{
				Debug.LogWarning( "InputManager[Edit->Project Settings->Input]に必要なパラメータが設定されていません\n[Tools->Initialize InputManager]を実行してください" ) ;
				return null ;
			}
#endif
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindAnyObjectByType( typeof( InputManager ) ) as InputManager ;
			if( m_Instance == null )
			{
				var go = new GameObject( "InputManager" ) ;
				if( parent != null )
				{
					go.transform.SetParent( parent, false ) ;
				}

				go.AddComponent<InputManager>() ;

				//---------------------------------------------------------

				// InputSystem の有効かどうかを格納する
				m_Instance.m_InputSystemEnabled = inputSystemEnabled ;

				if( m_Instance.m_InputSystemEnabled == false )
				{
					go.name = "InputManager [InputSystem - Old]" ;
				}
				else
				{
					go.name = "InputManager [InputSystem - New]" ;
				}

				// 基本入力モジュールをセットアップする
				m_Instance.Initialize( m_Instance.m_InputSystemEnabled ) ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// インプットマネージャのインスタンスを破棄する
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
			}
		}
	
		//-----------------------------------------------------------------

		/// <summary>
		/// インスタンス生成時に呼び出される
		/// </summary>
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			var instanceOther = GameObject.FindAnyObjectByType( typeof( InputManager ) ) as InputManager ;
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
		}

		/// <summary>
		/// インスタンス破棄時に呼び出される
		/// </summary>
		internal void OnDestroy()
		{
			if( m_Instance == this )
			{
				Terminate() ;

				m_Instance  = null ;
			}
		}
	
		//-------------------------------------------------------------------------------------------

		// 基本入力モジュールをセットアップする
		private void Initialize( bool inputSystemEnabled )
		{
			// Keyboard の実装を生成する
			Keyboard.Initialize( inputSystemEnabled, this ) ;

			// Moue の実装を生成する
			Mouse.Initialize( inputSystemEnabled, this ) ;

			// GamePad の実装を生成する
			GamePad.Initialize( inputSystemEnabled, this ) ;

			//----------------------------------
			// 例外が発生した際にカーソルを強制表示させる措置

			// メインスレッド用のフック
			Application.logMessageReceived -= OnExceptionOccurred ;
			Application.logMessageReceived += OnExceptionOccurred ;

			// サブスレッド用のフック
			Application.logMessageReceivedThreaded -= OnExceptionOccurred ;
			Application.logMessageReceivedThreaded += OnExceptionOccurred ;
		}

		// 後始末を行う
		private void Terminate()
		{
			// メインスレッド用のフック
			Application.logMessageReceived -= OnExceptionOccurred ;

			// サブスレッド用のフック
			Application.logMessageReceivedThreaded -= OnExceptionOccurred ;

			//--------------

			// ポインターが非表示になっている可能性があるので念のため表示しておく
			if( m_CursorProcessing == true )
			{
				UnityEngine.Cursor.visible = true ;
			}

			// 振動を強制停止
			StopMotor() ;
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
				// カーソルを強制表示する
				Cursor.visible = true ;
			}
		}

		//---------------------------------------------------------------------------

		// InputSystem を有効にするかどうか
		private bool m_InputSystemEnabled ;

		/// <summary>
		/// InputSystem が有効かどうか
		/// </summary>
		public static bool InputSystemEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
#if !ENABLE_INPUT_SYSTEM
				return false ;
#else
				return m_Instance.m_InputSystemEnabled ;
#endif
			}
		}

		//-----------------------------------

		// フォーカスを得ている状態かどうか
		private bool m_IsFocus ;

		/// <summary>
		/// フォーカスを得ているか状態かどうか
		/// </summary>
		public static bool IsFocus
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_IsFocus ;
			}
		}

		internal void OnApplicationFocus( bool focus )
		{
			m_IsFocus = focus ;
		}

		//---------------------------------------------------------------------------

		// m_Enabled という名前は MonoBehaviour で定義されているので使ってはいけない
		public bool ControlEnabled = true ;

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void Enable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ControlEnabled = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Disable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ControlEnabled = false ;
		}

		/// <summary>
		/// 有効無効状態
		/// </summary>
		public static bool IsControlEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.ControlEnabled ;
			}
			set
			{
				if( m_Instance == null )
				{
					return  ;
				}

				m_Instance.ControlEnabled = value ;
			}
		}

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public bool Invert = false ;

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public static bool IsInvert
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.Invert ;
			}
			set
			{
				m_Instance.Invert = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼び出される(描画)
		/// </summary>
		internal void Update()
		{
			// 毎フレームの処理
			ProcessUpdate() ;
		}

		/// <summary>
		/// 毎フレーム呼び出される(物理)
		/// </summary>
		internal void FixedUpdate()
		{
			// 毎フレームの処理
			ProcessFixedUpdate() ;
		}

		//-------------------------------------------------------------------------------------------
		// 毎フレームの処理

		private float	m_Tick = 0 ;
		private Vector3 m_Position = Vector3.zero ;

		private const float m_DriftThreshold = 0.1f ;

		// 毎フレーム呼び出される(描画)
		private void ProcessUpdate()
		{
			if( ControlEnabled == false )
			{
				return ;
			}

			//----------------------------------------------------------

			if( InputProcessingType == InputProcessingTypes.Switching )
			{
				// いずれか片方の入力のみ可能(Pointer・GamePadの最初の入力は無効＝切り替え扱い)

				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード
					if( m_InputSwitching == true )
					{
						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool button_0 = GetMouseButton( 0 ) ;
						bool button_1 = GetMouseButton( 1 ) ;
						bool button_2 = GetMouseButton( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						// 切り替えた直後は一度全開放しないと入力できない
						if( button_0 == false && button_1 == false && button_2 == false )
						{
							m_InputSwitching = false ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputSwitching = false ;
							}
						}
					}
					else
					{
						// Pointer モード有効中
						Pointer.Update( false ) ;

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						int buttonAll = GamePad.GetButtonAll() ;
						var axis_0 = GamePad.GetAxis( 0 ) ;
						var axis_1 = GamePad.GetAxis( 1 ) ;
						var axis_2 = GamePad.GetAxis( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						// ドリフト対策
						float ax0 = Mathf.Abs( axis_0.x ) ;
						float ay0 = Mathf.Abs( axis_0.y ) ;
						float ax1 = Mathf.Abs( axis_1.x ) ;
						float ay1 = Mathf.Abs( axis_1.y ) ;
						float ax2 = Mathf.Abs( axis_2.x ) ;
						float ay2 = Mathf.Abs( axis_2.y ) ;
						float ax = Mathf.Max( ax0, ax1, ax2 ) ;
						float ay = Mathf.Max( ay0, ay1, ay2 ) ;

						if( buttonAll != 0 || ax >  m_DriftThreshold || ay >  m_DriftThreshold )
						{
							// GamePad モードへ移行
							SetInputType_Private( InputTypes.GamePad ) ;
						}
					}
				}
				else
				{
					// 現在は GamePad モード
					if( m_InputSwitching == true )
					{
						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						// 切り替えた直後は一度全開放しないと入力できない
						int buttonAll = GamePad.GetButtonAll() ;
						var axis_0 = GamePad.GetAxis( 0 ) ;
						var axis_1 = GamePad.GetAxis( 1 ) ;
						var axis_2 = GamePad.GetAxis( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						// ドリフト対策
						float ax0 = Mathf.Abs( axis_0.x ) ;
						float ay0 = Mathf.Abs( axis_0.y ) ;
						float ax1 = Mathf.Abs( axis_1.x ) ;
						float ay1 = Mathf.Abs( axis_1.y ) ;
						float ax2 = Mathf.Abs( axis_2.x ) ;
						float ay2 = Mathf.Abs( axis_2.y ) ;
						float ax = Mathf.Max( ax0, ax1, ax2 ) ;
						float ay = Mathf.Max( ay0, ay1, ay2 ) ;

						if( buttonAll == 0 && ax <  m_DriftThreshold && ay <  m_DriftThreshold )
						{
							m_InputSwitching = false ;
							m_Position = MousePosition ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する(DualShock4を繋いている時にXbox互換のプロファイルに切り替えると右スティックが-1,-1で入りっぱなし扱いになってしまうため)
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputSwitching = false ;
								m_Position = MousePosition ;
							}
						}
					}
					else
					{
						// GamePad モード有効中
						GamePad.Update( false ) ;

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool button_0 = GetMouseButton( 0 ) ;
						bool button_1 = GetMouseButton( 1 ) ;
						bool button_2 = GetMouseButton( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						if( m_Position.Equals( MousePosition ) == false || button_0 == true || button_1 == true || button_2 == true )
						{
							// Pointer モードへ移行
							SetInputType_Private( InputTypes.Pointer ) ;
						}
					}
				}
			}
			else
			{
				// 両方の入力が同時に可能(Pointer・GamePadの最初の入力は有効＝切り替えと同時に効果を発揮する)

				m_InputSwitching = false ;
				m_IgnoreInputSwitching = true ;

				// 最後に入力された方を現在のモードとする
				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード扱い

					int buttonAll = GamePad.GetButtonAll() ;
					var axis_0 = GamePad.GetAxis( 0 ) ;
					var axis_1 = GamePad.GetAxis( 1 ) ;
					var axis_2 = GamePad.GetAxis( 2 ) ;

					// ドリフト対策
					float ax0 = Mathf.Abs( axis_0.x ) ;
					float ay0 = Mathf.Abs( axis_0.y ) ;
					float ax1 = Mathf.Abs( axis_1.x ) ;
					float ay1 = Mathf.Abs( axis_1.y ) ;
					float ax2 = Mathf.Abs( axis_2.x ) ;
					float ay2 = Mathf.Abs( axis_2.y ) ;
					float ax = Mathf.Max( ax0, ax1, ax2 ) ;
					float ay = Mathf.Max( ay0, ay1, ay2 ) ;

					if( buttonAll != 0 || ax >  m_DriftThreshold || ay >  m_DriftThreshold )
					{
						// GamePad モードへ移行
						SetInputType_Private( InputTypes.GamePad ) ;
					}
				}
				else
				{
					// 現在は GamePad モード扱い

					bool button_0 = GetMouseButton( 0 ) ;
					bool button_1 = GetMouseButton( 1 ) ;
					bool button_2 = GetMouseButton( 2 ) ;

					if( m_Position.Equals( MousePosition ) == false || button_0 == true || button_1 == true || button_2 == true )
					{
						// Pointer モードへ移行
						SetInputType_Private( InputTypes.Pointer ) ;
					}
				}

				//---------------------------------

				// Pointer
				Pointer.Update( false ) ;

				// GamePad
				GamePad.Update( false ) ;
			}

			// 共通ルーチンの呼び出し
			ExecuteCommonProcessing() ;
		}

		// 毎フレーム呼び出される(物理)
		private void ProcessFixedUpdate()
		{
			if( ControlEnabled == false )
			{
				return ;
			}

			//----------------------------------------------------------

			// Pointer
			Pointer.Update( true ) ;

			// GamePad
			GamePad.Update( true ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 入力タイプを強制指定する
		/// </summary>
		/// <param name="inputType"></param>
		public static bool SetInputType( InputTypes inputType )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.SetInputType_Private( inputType ) ;

			// 共通ルーチンの呼び出し
			m_Instance.ExecuteCommonProcessing() ;

			return true ;
		}

		// 入力タイプを強制指定する
		private void SetInputType_Private( InputTypes inputType )
		{
			if( inputType == InputTypes.GamePad )
			{
				// GamePad モードへ移行

				m_InputType = InputTypes.GamePad ;

				m_SystemCursorVisible = false ;

				m_OnInputTypeChanged?.Invoke( m_InputType ) ;
				m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
			}
			else
			if( inputType == InputTypes.Pointer )
			{
				// Pointer モードへ移行

				m_InputType = InputTypes.Pointer ;

				m_SystemCursorVisible = true ;

				m_OnInputTypeChanged?.Invoke( m_InputType ) ;
				m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
			}

			if( InputProcessingType == InputProcessingTypes.Switching )
			{
				// 最初の入力を無効化(切り替え用)にするための変数初期化
				m_InputSwitching = true ;
				m_Tick = 0 ;
			}
			else
			{
				// GamePad モード解除判定用に現在の Pointer の位置を記録する
				m_Position = MousePosition ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// 共通ルーチン

		private void ExecuteCommonProcessing()
		{
			//----------------------------------------------------------
			// カーソルの表示制御

			if( CursorProcessing == true )
			{
				// カーソルの表示制御が有効になっている
				bool isVisible = m_SystemCursorVisible & CursorVisible ;

				if( isVisible != m_ActiveCursorVisible )
				{
					// カーソルの表示状態が変化する
					m_ActiveCursorVisible = isVisible ;

					if( m_ActiveCursorVisible == true )
					{
						// カーソルは表示
						UnityEngine.Cursor.visible = true ;
					}
					else
					{
						// カーソルは隠蔽
						UnityEngine.Cursor.visible = false ;
					}
				}
			}

			//----------------------------------------------------------
			// アップデート時のコールバック

			if( m_Updater.Count >  0 )
			{
				Updater updater = m_Updater.Peek() ;
				updater.Action( updater.Option ) ;
			}
		}
	}
}
