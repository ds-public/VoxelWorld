using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.EventSystems ;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem ;
using UnityEngine.InputSystem.UI ;
#endif

// ※Keyboard Mouse Pointer のクラス名が被るので namespace は UIEventSystem 全般を uGUIHelper.InputAdapter とする

namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// uGUI:EventSystem の機能拡張コンポーネントクラス
	/// </summary>
	[ExecuteInEditMode][DefaultExecutionOrder( -900 )]
	public partial class UIEventSystem : MonoBehaviour
	{
		// シングルトンインスタンス
		private static UIEventSystem m_Instance ;

		//---------------------------------------------------------

		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <returns></returns>
		public static UIEventSystem Create()
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
#if UNITY_EDITOR
			if( GameObject.FindAnyObjectByType<EventSystem>() != null )
			{
				Debug.LogWarning( "既にシーン内に EventSystem が存在しています。UIEventSystm への差し替えを推奨します。" ) ;
			}
#endif
			//----------------------------------------------------------

			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindAnyObjectByType( typeof( UIEventSystem ) ) as UIEventSystem ;
			if( m_Instance == null )
			{
				var go = new GameObject( "EventSystem" ) ;
				go.AddComponent<UIEventSystem>() ;
			
				var t = go.transform ;
				t.SetParent( null ) ;
				t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
				t.localScale	= Vector3.one ;

				//---------------------------------------------------------

#if !ENABLE_INPUT_SYSTEM
				m_Instance.m_InputSystemEnabled = false ;	// InputSystem は強制的に使用できなくする
#else
				// InputSystem を有効にするかどうか
				var settings = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( settings != null )
				{
					// InputSystem を使用するかどうかは設定次第
					m_Instance.m_InputSystemEnabled = settings.InputSystemEnabled ;
				}
				else
				{
					// 設定が無けれは InputSystem は使用しない
					m_Instance.m_InputSystemEnabled = false ;
				}
#endif
				if( m_Instance.m_InputSystemEnabled == false )
				{
					go.name = "EventSystem [InputSysyem - Old]" ;
				}
				else
				{
					go.name = "EventSystem [InputSysyem - New]" ;
				}

				//----------------------------------

				// 基本入力モジュールをセットアップする
				m_Instance.Initialize( m_Instance.m_InputSystemEnabled ) ;
			}

			//----------------------------------------------------------

			return m_Instance ;
		}
	
		// インスタンス破棄
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

		//-------------------------------------------------------------------------------------------
	
		internal void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する（シーンを加えようとした場合）
			if( m_Instance != null )
			{
				Debug.LogWarning( "Destroy Myself !!:" + name ) ;
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			gameObject.transform.localScale	= Vector3.one ;

			//-----------------------------

			if( TryGetComponent<EventSystem>( out var _ ) == false )
			{
				gameObject.AddComponent<EventSystem>() ;
			}
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
			//----------------------------------------------------------
			// UIEventSystem 固有(最重要)

			if( inputSystemEnabled == false )
			{
				// スタンダード版のインプットモジュールを使用する
				if( TryGetComponent<StandaloneInputModule>( out var m_StandaloneInputModule ) == false )
				{
					m_StandaloneInputModule		= gameObject.AddComponent<StandaloneInputModule>() ;
				}
			}
#if ENABLE_INPUT_SYSTEM
			else
			{
				// インプットシステム版のインプットモジュールを使用する
				if( TryGetComponent<InputSystemUIInputModule>( out var m_InputSystemUIInputModule ) == false )
				{
					m_InputSystemUIInputModule	= gameObject.AddComponent<InputSystemUIInputModule>() ;
				}
			}
#endif
			//----------------------------------------------------------

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

			//----------------------------------
			// UIEventSystem 固有

			if( focus == true )
			{
				// フォーカスを得た

				// Hover と Press の監視
				ProcessHoverAndPress_OnApplicationFocus() ;
			}
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
			m_Instance.SetState( true ) ;
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
			m_Instance.SetState( false ) ;
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
					return ;
				}

				m_Instance.ControlEnabled = value ;
				m_Instance.SetState( value ) ;
			}
		}

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public bool Invert = false ;

		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public bool IsInvert
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

		internal void Update()
		{
			// 毎フレームの処理
			ProcessUpdate() ;

			//----------------------------------
			// UIEventSystem 固有

			// Hover と Press の監視
			ProcessHoverAndPress_Update() ;
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
				// いずれか片方の入力のみ可能(Pointer・GamePadの最初の入力は無効＝切り替え扱い)

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

						var axis_0 = GamePad.GetAxis( 0 ) ;
						var axis_1 = GamePad.GetAxis( 1 ) ;
						var axis_2 = GamePad.GetAxis( 2 ) ;

						if( GamePad.GetButtonAll() != 0 || axis_0.x != 0 || axis_0.y != 0 || axis_1.x != 0 || axis_1.y != 0 || axis_2.x != 0 || axis_2.y != 0 )
						{
							// GamePad モードへ移行
							SetInputType_Private( InputTypes.GamePad ) ;
						}
					}
				}
				else
				{
					// 現在は GamePad モード
					if( m_InputHold == true )
					{
						// 切り替えた直後は一度全開放しないと入力できない
						var axis_0 = GamePad.GetAxis( 0 ) ;
						var axis_1 = GamePad.GetAxis( 1 ) ;
						var axis_2 = GamePad.GetAxis( 2 ) ;

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
							SetInputType_Private( InputTypes.Pointer ) ;
						}
					}
				}
			}
			else
			{
				// 両方の入力が同時に可能(Pointer・GamePadの最初の入力は有効＝切り替えと同時に効果を発揮する)

				// 最後に入力された方を現在のモードとする
				if( m_InputType == InputTypes.Pointer )
				{
					// 現在は Pointer モード扱い

					var axis_0 = GamePad.GetAxis( 0 ) ;
					var axis_1 = GamePad.GetAxis( 1 ) ;
					var axis_2 = GamePad.GetAxis( 2 ) ;

					if( GamePad.GetButtonAll() != 0 || axis_0.x != 0 || axis_0.y != 0 || axis_1.x != 0 || axis_1.y != 0 || axis_2.x != 0 || axis_2.y != 0 )
					{
						// GamePad モードへ移行
						SetInputType_Private( InputTypes.GamePad ) ;
					}
				}
				else
				{
					// 現在は GamePad モード扱い

					if( m_Position.Equals( MousePosition ) == false || GetMouseButton( 0 ) == true || GetMouseButton( 1 ) == true || GetMouseButton( 2 ) == true )
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
				m_InputHold = true ;
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

		//-------------------------------------------------------------------------------------------------------------------
		// 以下は UIEventSystem 固有

		//-------------------------------------------------------------------------------------------------------------------
		// Hover と Press は独自に監視する

		private GameObject						m_ActiveHover_GameObject ;
		private GameObject						m_ActivePress_GameObject ;

		private readonly PointerEventData		m_CT_EventDataCurrentPosition = new ( EventSystem.current ) ;
		private readonly List<RaycastResult>	m_CT_Results = new () ;
		private GameObject						m_CT_GameObject ;

		// Hover と Press の監視
		private void ProcessHoverAndPress_OnApplicationFocus()
		{
			m_ActiveHover_GameObject = null ;
			m_ActivePress_GameObject = null ;

			//----------------------------------

			if( m_InputSystemEnabled == false )
			{
				// 旧インプットシステムの場合のみ処理する
				if( m_CT_GameObject != null )
				{
					// 押されたままの扱いになっている GameObject がある
					PointerEventData pointerEvent = new ( EventSystem.current )
					{
						pointerPress = m_CT_GameObject
					} ;

					ExecuteEvents.Execute( pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler ) ;

					m_CT_GameObject = null ;
				}
			}
		}
		
		// Hover と Press の監視
		private void ProcessHoverAndPress_Update()
		{
			//----------------------------------------------------------
			// Raycast

			m_CT_EventDataCurrentPosition.position = MousePosition ;
			m_CT_Results.Clear() ;
			EventSystem.current.RaycastAll( m_CT_EventDataCurrentPosition, m_CT_Results ) ;

			//----------------------------------------------------------
			// Hover

			if( m_CT_Results.Count >= 1 )
			{
				m_ActiveHover_GameObject = m_CT_Results[ 0 ].gameObject ;
			}
			else
			{
				m_ActiveHover_GameObject = null ;
			}

			//----------------------------------------------------------
			// Press

			int button ;

			//----------------------------------
			// 押された

			button = 0 ;
			for( int i  = 0 ; i <= 2 ; i ++ )
			{
				if( GetMouseButtonDown( i ) == true )
				{
					button |= ( 1 << i ) ;
				}
			}

			if( button != 0 )
			{
				button = 0 ;
				for( int i  = 0 ; i <= 2 ; i ++ )
				{
					if( GetMouseButton( i ) == true )
					{
						button ++ ;
					}
				}

				if( button == 1 )
				{
					// 最初のプレスのみ処理対象とする
					if( m_CT_Results.Count >= 1 )
					{
						// レイキャスト対象
						m_ActivePress_GameObject = m_CT_Results[ 0 ].gameObject ;

						m_CT_GameObject = m_CT_Results[ 0 ].gameObject ;
					}
				}
			}

			//----------------------------------
			// 継続中

			if( m_InputSystemEnabled == false )
			{
				// 旧インプットシステムの場合のみ処理する
				if( m_CT_GameObject != null )
				{
					button = 0 ;
					for( int i  = 0 ; i <= 2 ; i ++ )
					{
						if( GetMouseButton( i ) == true )
						{
							button ++ ;
						}
					}

					if( button >  0 )
					{
						if( m_CT_Results.Count >= 1 )
						{
							if( m_CT_Results[ 0 ].gameObject != m_CT_GameObject )
							{
								// 一度外に出たら以後はメッセージは送らないようにする
								m_CT_GameObject = null ;
							}
						}
					}
				}
			}

			//----------------------------------
			// 離された

			button = 0 ;
			for( int i  = 0 ; i <= 2 ; i ++ )
			{
				if( GetMouseButtonUp( i ) == true )
				{
					button |= ( 1 << i ) ;
				}
			}

			if( button != 0 )
			{
				button = 0 ;
				for( int i  = 0 ; i <= 2 ; i ++ )
				{
					if( GetMouseButton( i ) == true )
					{
						button ++ ;
					}
				}

				if( button == 0 )
				{
					// 全て離された
					m_ActivePress_GameObject = null ;

					m_CT_GameObject = null ;
				}
			}
		}

		/// <summary>
		/// イベントシステムの状態を取得する
		/// </summary>
		/// <returns></returns>
		public static bool State
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
	
				if( m_Instance.gameObject.TryGetComponent<EventSystem>( out var eventSystem ) == false )
				{
					return false ;
				}
	
				return eventSystem.enabled ;
			}
		}

		private bool SetState( bool state )
		{
			if( gameObject.TryGetComponent<EventSystem>( out var eventSystem ) == false )
			{
				return false ;
			}
			
			eventSystem.enabled = state ;

			return eventSystem.enabled ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がホバー状態か判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsHovering( GameObject target )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_ActiveHover_GameObject == null )
			{
				return false ;
			}

			return ( m_Instance.m_ActiveHover_GameObject == target ) ;
		}

		/// <summary>
		/// 指定のゲームオブジェクト(UI)がプレス状態か判定する
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsPressing( GameObject target )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_ActivePress_GameObject == null )
			{
				return false ;
			}

			return ( m_Instance.m_ActivePress_GameObject == target ) ;
		}
	}
}
