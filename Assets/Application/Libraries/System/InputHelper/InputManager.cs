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
	/// 入力操作クラス Version 2024/03/13 0
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

			// Pointer の実装を生成する
			Pointer.Initialize( this ) ;

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
			UnityEngine.Cursor.visible = true ;

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
				// いずれか片方の入力のみ可能

				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード
					if( m_InputHold == true )
					{
						// 切り替えた直後は一度全開放しないと入力できない
						if( GetMouseButton( 0 ) == false && GetMouseButton( 1 ) == false && GetMouseButton( 2 ) == false )
						{
							m_InputHold = false ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputHold = false ;
							}
						}
					}
					else
					{
						// Pointer モード有効中
						Pointer.Update( false ) ;

						Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
						Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
						Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

						if( GamePad.GetButtonAll() != 0 || axis_0.x != 0 || axis_0.y != 0 || axis_1.x != 0 || axis_1.y != 0 || axis_2.x != 0 || axis_2.y != 0 )
						{
							// GamePad モードへ移行

							m_SystemCursorVisible = false ;

							m_InputType = InputTypes.GamePad ;
							m_InputHold = true ;
							m_Tick = 0 ;

							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
							m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
						}
					}
				}
				else
				{
					// 現在は GamePad モード
					if( m_InputHold == true )
					{
						// 切り替えた直後は一度全開放しないと入力できない
						Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
						Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
						Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

						if( GamePad.GetButtonAll() == 0 && axis_0.x == 0 && axis_0.y == 0 && axis_1.x == 0 && axis_1.y == 0 && axis_2.x == 0 && axis_2.y == 0 )
						{
							m_InputHold = false ;

							m_Position = MousePosition ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する(DualShock4を繋いている時にXbox互換のプロファイルに切り替えると右スティックが-1,-1で入りっぱなし扱いになってしまうため)
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputHold = false ;
							}
						}
					}
					else
					{
						// GamePad モード有効中
						GamePad.Update( false ) ;

						if( m_Position.Equals( MousePosition ) == false || GetMouseButton( 0 ) == true || GetMouseButton( 1 ) == true || GetMouseButton( 2 ) == true )
						{
							// Pointer モードへ移行

							m_SystemCursorVisible = true ;

							m_InputType = InputTypes.Pointer ;
							m_InputHold = true ;

							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
							m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
						}
					}
				}
			}
			else
			{
				// 両方の入力が同時に可能

				// 最後に入力された方を現在のモードとする
				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード扱い

					Vector2 axis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 axis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 axis_2 = GamePad.GetAxis( 2 ) ;

					if( GamePad.GetButtonAll() != 0 || axis_0.x != 0 || axis_0.y != 0 || axis_1.x != 0 || axis_1.y != 0 || axis_2.x != 0 || axis_2.y != 0 )
					{
						// GamePad モードへ移行

						// GamePad モード解除判定用に現在の Pointer の位置を記録する
						m_Position = MousePosition ;

						m_SystemCursorVisible = false ;

						m_InputType = InputTypes.GamePad ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}
				else
				{
					// 現在は GamePad モード扱い

					if( m_Position.Equals( MousePosition ) == false || GetMouseButton( 0 ) == true || GetMouseButton( 1 ) == true || GetMouseButton( 2 ) == true )
					{
						// Pointer モードへ移行

						m_SystemCursorVisible = true ;

						m_InputType = InputTypes.Pointer ;

						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						m_OnInputTypeChangedDelegate?.Invoke( m_InputType ) ;
					}
				}

				//---------------------------------

				// Pointer
				Pointer.Update( false ) ;

				// GamePad
				GamePad.Update( false ) ;
			}

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

			if( m_Updater.Count >  0 )
			{
				Updater updater = m_Updater.Peek() ;
				updater.Action( updater.Option ) ;
			}
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
	}
}
