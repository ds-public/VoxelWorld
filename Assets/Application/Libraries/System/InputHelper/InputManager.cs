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
	/// 入力操作クラス Version 2025/04/15 0
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
			
#if UNITY_EDITOR && !ENABLE_INPUT_SYSTEM
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

			//-------------------------------------------------

			// インスタンス生成直後に一度アップデートで実行さるる処理を呼ぶ
			ExecuteCommonProcessing() ;
		}

		// 後始末を行う
		private void Terminate()
		{
			// メインスレッド用のフック
			Application.logMessageReceived -= OnExceptionOccurred ;

			// サブスレッド用のフック
			Application.logMessageReceivedThreaded -= OnExceptionOccurred ;

			//--------------

			// 振動を強制停止
			StopMotor() ;
		}

		// アプリケーション終了時に呼び出される
		internal void OnApplicationQuit()
		{
			// ポインターが非表示になっている可能性があるので念のため表示しておく
			if( Settings.CursorProcessing == true )
			{
				UnityEngine.Cursor.visible = true ;
			}        
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

		//-------------------------------------------------------------------------------------------
		// 毎フレームの処理

		private float	m_Tick = 0 ;

		// 入力モードを切り替え中かどうか
		private  bool    m_InputSwitching	= false ;

		// 入力モード切り替え中の値を無視して常に入力を有効にするかどうか
		private  bool    m_IgnoreInputSwitching = false ;

		/// <summary>
		/// 入力モードを切り替え中かどうか(public にしているが利用者に解放するものではない)
		/// </summary>
		public bool InputSwitching
		{
			get
			{
				if( m_IgnoreInputSwitching == true )
				{
					// 常に入力は有効
					return false ;
				}

				return m_InputSwitching ;
			}
		}

		/// <summary>
		/// 入力モード切り替え中の値を無視して常に入力を有効にするかどうか(public にしているが利用者に解放するものではない)
		/// </summary>
		public bool IgnoreInputSwitching => m_IgnoreInputSwitching ;

		//-----------------------------

		// 毎フレーム呼び出される(描画)
		private void ProcessUpdate()
		{
			if( ControlEnabled == false )
			{
				return ;
			}

			//----------------------------------------------------------

			// Keyboard
			Keyboard.Update() ;

			if( Settings.InputProcessingType == InputProcessingTypes.Switching )
			{
				// いずれか片方の入力のみ可能(Pointer・GamePadの最初の入力は無効＝切り替え扱い)

				if( Settings.BasisInputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード
					if( m_InputSwitching == true )
					{
						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool button_0 = Mouse.GetButton( 0 ) ;
						bool button_1 = Mouse.GetButton( 1 ) ;
						bool button_2 = Mouse.GetButton( 2 ) ;

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
						Mouse.Update() ;

						// 拡張操作用にアップデートが必要
						GamePad.Update() ;

						InputTypes basisInputType = InputTypes.Unknown ;
						InputTypes extraInputType = Settings.ExtraInputType ;

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						// キーボードの基本操作が行われた
						if( GamePad.IsKeyboardInput( InputCategories.Basis ) == true )
						{
							basisInputType = InputTypes.Keyboard ;
						}

						// ゲームパッドの基本操作が行われた
						if( GamePad.IsGamePadInput( InputCategories.Basis ) == true )
						{
							basisInputType = InputTypes.GamePad ;
						}

						// キーボードの拡張操作が行われた
						if( GamePad.IsKeyboardInput( InputCategories.Extra ) == true )
						{
							extraInputType = InputTypes.Keyboard ;
						}

						// ゲームパッドの基本操作が行われた
						if( GamePad.IsGamePadInput( InputCategories.Extra ) == true )
						{
							extraInputType = InputTypes.GamePad ;
						}

						m_IgnoreInputSwitching = false ;

						//-------------

						if( basisInputType == InputTypes.Keyboard || basisInputType == InputTypes.GamePad )
						{
							// 入力モード移行
							SetInputType_Private( basisInputType, basisInputType ) ;
						}
						else
						{
							if( Settings.ExtraInputType != extraInputType )
							{
								// 拡張操作の入力モードのみ変更
								SetInputType_Private( Settings.BasisInputType, extraInputType ) ;
							}
						}
					}
				}
				else
				if( Settings.BasisInputType == InputTypes.Keyboard )
				{
					// 現在はキーボード入力モード
					if( m_InputSwitching == true )
					{
						// 切り替わった直後のキーボード入力モード

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						// キーボードの操作が行われたかどうか
						bool isBasisAction = false ;
						bool isExtraAction = false ;

						if( Settings.PreviousBasisInputType == InputTypes.Pointer || Settings.PreviousBasisInputType == InputTypes.Mouse )
						{
							isBasisAction = GamePad.IsKeyboardInput( InputCategories.Basis ) ;
							isExtraAction = GamePad.IsKeyboardInput( InputCategories.Extra ) ;
						}

						m_IgnoreInputSwitching = false ;

						//-------------

						// 切り替えた直後は一度全開放しないと入力できない
						if( isBasisAction == false && isExtraAction == false )
						{
							// 本当にキーボード入力モードに移行する
							m_InputSwitching = false ;
							Settings.MousePosition = MousePosition ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputSwitching = false ;
								Settings.MousePosition = MousePosition ;
							}
						}
					}
					else
					{
						// 本当の Keyboard 入力モード

						// GamePad モード有効中
						GamePad.Update() ;

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool mb0 = Mouse.GetButton( 0 ) ;
						bool mb1 = Mouse.GetButton( 1 ) ;
						bool mb2 = Mouse.GetButton( 2 ) ;

						bool mbu0 = Mouse.GetButtonUp( 0 ) ;
						bool mbu1 = Mouse.GetButtonUp( 1 ) ;
						bool mbu2 = Mouse.GetButtonUp( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						if
						(
							( Settings.MousePosition.Equals( MousePosition ) == false && mb0 == false && mb1 == false && mb2 == false ) ||
							mbu0 == true || mbu1 == true || mbu2 == true
						)
						{
							// Pointer モードへ移行
							SetInputType_Private( InputTypes.Pointer, InputTypes.Keyboard ) ;
						}
						else
						{
							m_IgnoreInputSwitching = true ;

							// ゲームパッドの操作が行われたかどうか
							bool isBasisAction = GamePad.IsGamePadInput( InputCategories.Basis ) ;
							bool isExtraAction = GamePad.IsGamePadInput( InputCategories.Extra ) ;

							m_IgnoreInputSwitching = false ;

							if( isBasisAction == true || isExtraAction == true )
							{
								// GamePad モードへ移行
								SetInputType_Private( InputTypes.GamePad, InputTypes.GamePad ) ;
							}
						}
					}
				}
				else
				if( Settings.BasisInputType == InputTypes.GamePad )
				{
					// 現在は GamePad モード
					if( m_InputSwitching == true )
					{
						// 切り替わった直後の GamePad 入力モード

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool isBasisAction = false ;
						bool isExtraAction = false ;

						if( Settings.PreviousBasisInputType == InputTypes.Pointer || Settings.PreviousBasisInputType == InputTypes.Mouse )
						{
							// ゲームパッドの操作が行われたかどうか
							isBasisAction = GamePad.IsGamePadInput( InputCategories.Basis ) ;
							isExtraAction = GamePad.IsGamePadInput( InputCategories.Extra ) ;
						}

						m_IgnoreInputSwitching = false ;

						//-------------

						// 切り替えた直後は一度全開放しないと入力できない
						if( isBasisAction == false && isExtraAction == false )
						{
							// 本当にキーボード入力モードに移行する
							m_InputSwitching = false ;
							Settings.MousePosition = MousePosition ;
						}
						else
						{
							// 入力し続けても１秒経過したら強制解除する
							m_Tick += Time.unscaledDeltaTime ;
							if( m_Tick >= 1 )
							{
								m_InputSwitching = false ;
								Settings.MousePosition = MousePosition ;
							}
						}
					}
					else
					{
						// 本当のゲームパッド入力モード
						// (ゲームパッド入力モード有効中)

						GamePad.Update() ;

						//-------------
						// 一時的に入力を強制有効化

						m_IgnoreInputSwitching = true ;

						bool mb0 = Mouse.GetButton( 0 ) ;
						bool mb1 = Mouse.GetButton( 1 ) ;
						bool mb2 = Mouse.GetButton( 2 ) ;

						bool mbu0 = Mouse.GetButtonUp( 0 ) ;
						bool mbu1 = Mouse.GetButtonUp( 1 ) ;
						bool mbu2 = Mouse.GetButtonUp( 2 ) ;

						m_IgnoreInputSwitching = false ;

						//-------------

						if
						(
							( Settings.MousePosition.Equals( MousePosition ) == false && mb0 == false && mb1 == false && mb2 == false ) ||
							mbu0 == true || mbu1 == true || mbu2 == true
						)
						{
							// ポインターモードへ移行
							SetInputType_Private( InputTypes.Pointer, InputTypes.GamePad ) ;
						}
						else
						{
							m_IgnoreInputSwitching = true ;

							// キーボードの操作が行われたかどうか
							bool isBasisAction = GamePad.IsKeyboardInput( InputCategories.Basis ) ;
							bool isExtraAction = GamePad.IsKeyboardInput( InputCategories.Extra ) ;

							m_IgnoreInputSwitching = false ;

							if( isBasisAction == true || isExtraAction == true )
							{
								// キーボード入力モードへ移行
								SetInputType_Private( InputTypes.Keyboard, InputTypes.Keyboard ) ;
							}
						}
					}
				}
			}
			else
			if( Settings.InputProcessingType == InputProcessingTypes.Parallel )
			{
				// 両方の入力が同時に可能(Pointer・GamePadの最初の入力は有効＝切り替えと同時に効果を発揮する)

				// Mouse
				Mouse.Update( out bool button_0, out bool button_1, out bool button_2 ) ;

				// GamePad
				GamePad.Update() ;

				//---------------------------------

				m_InputSwitching = false ;
				m_IgnoreInputSwitching = true ;

				// 最後に入力された方を現在のモードとする
				if( Settings.BasisInputType == InputTypes.Pointer )
				{
					// 現在はポインター入力モード扱い

					InputTypes inputType = InputTypes.Unknown ;

					// キーボードの基本操作が行われた
					if( GamePad.IsKeyboardInput( InputCategories.Basis ) == true )
					{
						inputType = InputTypes.Keyboard ;
					}

					// ゲームパッドの基本操作が行われた
					if( GamePad.IsGamePadInput( InputCategories.Basis ) == true )
					{
						inputType = InputTypes.GamePad ;
					}

					if( inputType == InputTypes.Keyboard || inputType == InputTypes.GamePad )
					{
						// モード移行
						SetInputType_Private( inputType, inputType ) ;
					}
				}
				else
				if( Settings.BasisInputType == InputTypes.Keyboard )
				{
					// 現在はキーボード入力モード扱い

					if( Settings.MousePosition.Equals( MousePosition ) == false || button_0 == true || button_1 == true || button_2 == true )
					{
						// ポインター入力モードへ移行
						SetInputType_Private( InputTypes.Pointer, InputTypes.Keyboard ) ;
					}
					else
					{
						// ゲームパッドの基本操作・拡張操作が行われた
						if( GamePad.IsGamePadInput( InputCategories.Basis ) == true || GamePad.IsGamePadInput( InputCategories.Extra ) == true )
						{
							// ゲームパッド入力モードへ移行
							SetInputType_Private( InputTypes.GamePad, InputTypes.GamePad ) ;
						}
					}
				}
				else
				if( Settings.BasisInputType == InputTypes.GamePad )
				{
					// 現在はゲームパッド入力モード扱い

					if( Settings.MousePosition.Equals( MousePosition ) == false || button_0 == true || button_1 == true || button_2 == true )
					{
						// Pointer モードへ移行
						SetInputType_Private( InputTypes.Pointer, InputTypes.GamePad ) ;
					}
					else
					{
						// キーボードの基本操作・拡張操作が行われた
						if( GamePad.IsKeyboardInput( InputCategories.Basis ) == true || GamePad.IsKeyboardInput( InputCategories.Extra ) == true )
						{
							// キーボード入力モードへ移行
							SetInputType_Private( InputTypes.Keyboard, InputTypes.Keyboard ) ;
						}
					}
				}
			}
			else
			{
				// 入力モードのコントロールは無し

				// Mouse
				Mouse.Update() ;

				// GamePad
				GamePad.Update() ;
		   }

			//----------------------------------------------------------

			// 共通ルーチンの呼び出し
			ExecuteCommonProcessing() ;
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

			m_Instance.SetInputType_Private( inputType, inputType ) ;

			// 共通ルーチンの呼び出し
			m_Instance.ExecuteCommonProcessing() ;

			return true ;
		}

		/// <summary>
		/// 入力タイプを強制指定する
		/// </summary>
		/// <param name="inputType"></param>
		public static bool SetInputType( InputTypes basisInputType, InputTypes extraInputType )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.SetInputType_Private( basisInputType, extraInputType ) ;

			// 共通ルーチンの呼び出し
			m_Instance.ExecuteCommonProcessing() ;

			return true ;
		}

		// 入力タイプを強制指定する
		private void SetInputType_Private( InputTypes basisInputType, InputTypes extraInputType )
		{
			bool inputSwitching = true ;

			if( Settings.InputProcessingType != InputProcessingTypes.None )
			{
				if( basisInputType == InputTypes.Pointer || basisInputType == InputTypes.Mouse )
				{
					// ポインター入力モードへ移行

					// 基本操作
					if( Settings.BasisInputType != basisInputType )
					{
						Settings.PreviousBasisInputType = Settings.BasisInputType ;
						Settings.BasisInputType = basisInputType ;

						Settings.SystemCursorVisible = true ;

						inputSwitching = true ;
					}

					// ※拡張操作のみ変更される事がある

					// 拡張操作
					Settings.ExtraInputType = extraInputType ;

					m_OnInputTypeChanged?.Invoke( Settings.BasisInputType, Settings.ExtraInputType ) ;
					m_OnInputTypeChangedDelegate?.Invoke( Settings.BasisInputType, Settings.ExtraInputType ) ;
				}
				else
				if( basisInputType == InputTypes.Keyboard || basisInputType == InputTypes.GamePad )
				{
					// キーボード入力モードまたはゲームパッド入力モードへ移行

					inputSwitching = ( Settings.BasisInputType != InputTypes.Keyboard && Settings.BasisInputType != InputTypes.GamePad ) ;

					//---------------------------------

					Settings.PreviousBasisInputType = Settings.BasisInputType ;
					Settings.BasisInputType = basisInputType ;
					Settings.ExtraInputType = extraInputType ;

					Settings.SystemCursorVisible = false ;

					m_OnInputTypeChanged?.Invoke( Settings.BasisInputType, Settings.ExtraInputType ) ;
					m_OnInputTypeChangedDelegate?.Invoke( Settings.BasisInputType, Settings.ExtraInputType ) ;
				}
			}
			else
			{
				Settings.BasisInputType = basisInputType ;
				Settings.ExtraInputType = extraInputType ;

				Settings.SystemCursorVisible = ( basisInputType == InputTypes.Pointer || basisInputType == InputTypes.Mouse ) ;
			}

			//--------------

			if( Settings.InputProcessingType == InputProcessingTypes.Switching )
			{
				// 最初の入力を無効化(切り替え用)にするための変数初期化
				m_InputSwitching = inputSwitching ;
				m_Tick = 0 ;
			}
			else
			{
				// GamePad モード解除判定用に現在の Pointer の位置を記録する
				Settings.MousePosition = MousePosition ;
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
				bool isVisible = Settings.SystemCursorVisible & CursorVisible ;

				if( isVisible != Settings.ActiveCursorVisible )
				{
					// カーソルの表示状態が変化する
					Settings.ActiveCursorVisible = isVisible ;

					if( Settings.ActiveCursorVisible == true )
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
