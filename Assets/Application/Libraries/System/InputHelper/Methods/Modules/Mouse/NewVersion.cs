#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace InputHelper
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		// 新版
		public class Implementation_NewVersion : IImplementation
		{
			private Vector3 m_MousePosition = Vector3.zero ;
			
			private int m_TouchId = -1 ;
			private Vector3 m_TouchPosition = Vector3.zero ;

			/// <summary>
			/// ボタン用状態
			/// </summary>
			public class ButtonState
			{
				public bool		RepeatKeepFlag ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public bool		IsRepeat ;
				public bool		IsDown ;
				public bool		IsUp ;
			}

			private static ButtonState[,] m_ButtonStates ;

			//-----------------------------------------------------------------------------------------

			/// <summary>
			/// 初期化を行う
			/// </summary>
			/// <param name="numberOfButtons"></param>
			public void Initialize()
			{
				// ボタンの状態
				m_ButtonStates = new ButtonState[ NumberOfButtons, 2 ] ;
				for( int buttonIndex  = 0 ; buttonIndex <  NumberOfButtons ; buttonIndex ++ )
				{
					m_ButtonStates[ buttonIndex, 0 ] = new ButtonState() ;	// Update 用
					m_ButtonStates[ buttonIndex, 1 ] = new ButtonState() ;	// FixedUpdate 用
				}
			}

			/// <summary>
			/// フレーム毎の更新呼び出し
			/// </summary>
			public void Update( bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				int buttonIndex ;
				int numberOfButtons = m_ButtonStates.GetLength( 0 ) ;

				for( buttonIndex  = 0 ; buttonIndex <  numberOfButtons ; buttonIndex ++ )
				{
					ButtonState state = m_ButtonStates[ buttonIndex, slotNumber ] ;

					//---------------------------------

					state.IsRepeat	= false ;
					state.IsDown	= false ;
					state.IsUp		= false ;

					if( GetButton( buttonIndex ) == true )
					{
						if( state.RepeatKeepFlag == false )
						{
							// リピート開始
							state.IsRepeat	= true ;

							state.RepeatKeepFlag = true ;
							state.RepeatWakeTime = Time.realtimeSinceStartup ;
							state.RepeatLoopTime = Time.realtimeSinceStartup ;

							state.IsDown = true ;
						}
						else
						{
							// リピート最中
							if( ( Time.realtimeSinceStartup - state.RepeatWakeTime ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( Time.realtimeSinceStartup - state.RepeatLoopTime ) >= RepeatIntervalTime )
								{
									state.RepeatLoopTime = Time.realtimeSinceStartup ;

									state.IsRepeat = true ;
								}
							}
						}
					}
					else
					{
						// リピート解除
						if( state.RepeatKeepFlag == true )
						{
							state.IsUp = true ;

							state.RepeatKeepFlag  = false ;
						}
					}
				}
			}

			//-----------------------------------------------------------------------------------------

			/// ポインターの位置
			/// </summary>
			public Vector3 Position
			{
				get
				{
					Vector3 pointerPosition ;

					//---------------------------------------------------------
					// マウス処理

					var mousePosition = m_MousePosition ;

					UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
					if( mouse != null )
					{
						// マウスは繋がっている
						mousePosition = mouse.position.ReadValue() ;
					}

					//---------------------------------------------------------
					// タッチ処理(マウスの入力が無い場合)

					if( mousePosition.Equals( m_MousePosition ) == true )
					{
						// タッチも確認する

						if( Input.touchCount == 1 )
						{
							var touchPosition = m_TouchPosition ;

							var touch = Input.GetTouch( 0 ) ;
							if( m_TouchId <  0 )
							{
								// 解放後初めてのタッチ
								m_TouchId = touch.fingerId ;
								touchPosition = touch.position ;
							}
							else
							if( m_TouchId == touch.fingerId )
							{
								// 継続タッチ(同じ指であれば処理する)
								touchPosition = touch.position ;
							}

							if( touchPosition.Equals( m_TouchPosition ) == false )
							{
								m_TouchPosition = touchPosition ;
							}

							pointerPosition = touchPosition ;
						}
						else
						if( Input.touchCount == 0 )
						{
							// 解放
							if( m_TouchId != -1 )
							{
								m_TouchId  = -1 ;

								// マウスの入力があるまでタッチの最後の位置を維持する
								pointerPosition = m_TouchPosition ;

								m_MousePosition = m_TouchPosition ;
							}
							else
							{
								pointerPosition = m_MousePosition ;
							}
						}
						else
						{
							// ２本以上

							// マウスの入力があるまでタッチの最後の位置を維持する
							pointerPosition = m_TouchPosition ;
						}
					}
					else
					{
						// マウスの移動があった
						m_MousePosition = mousePosition ;

						pointerPosition = mousePosition ;
					}

					return pointerPosition ;
				}
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonNumber )
			{
				bool state = false ;

				//---------------------------------------------------------
				// マウス処理

				UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
				if( mouse != null )
				{
					// マウスは繋がっている
					state = buttonNumber switch
					{
						0 => mouse.leftButton.isPressed,
						1 => mouse.rightButton.isPressed,
						2 => mouse.middleButton.isPressed,
						_ => false,
					} ;
				}

				//---------------------------------------------------------
				// タッチ処理(マウスの入力が無い場合)

				if( state == false )
				{
					// タッチも確認する
					if( buttonNumber == 0 )
					{
						state = Input.touchCount >  0 ;
					}
				}

				//---------------------------------------------------------

				return state ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonNumber, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				return m_ButtonStates[ buttonNumber, slotNumber ].IsDown ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonNumber, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				return m_ButtonStates[ buttonNumber, slotNumber ].IsUp ;
			}

			/// <summary>
			/// リピート付きでボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonRepeat( int buttonNumber, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				return m_ButtonStates[ buttonNumber, slotNumber ].IsRepeat ;
			}

			//----------------------------------

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			public Vector2 ScrollDelta
			{
				get
				{
					UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
					if( mouse == null )
					{
						return Vector2.zero ;
					}

					// 割る値は要調整
					return mouse.scroll.ReadValue() ;
				}
			}
		}
	}
}
#endif

