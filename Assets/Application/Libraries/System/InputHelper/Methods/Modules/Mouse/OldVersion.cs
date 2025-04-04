using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		// 旧版
		public class Implementation_OldVersion : IImplementation
		{
			private Vector3 m_MousePosition = Vector3.zero ;
			
			private int m_TouchId = -1 ;
			private Vector3 m_TouchPosition = Vector3.zero ;

			private Vector3 m_PointerPosition	= Vector3.zero ;
			private Vector3 m_PointerDelta		= Vector3.zero ;

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

			private ButtonState[] m_ButtonStates ;

			//-----------------------------------------------------------------------------------------

			/// <summary>
			/// 初期化を行う
			/// </summary>
			/// <param name="numberOfButtons"></param>
			public void Initialize()
			{
				// ボタンの状態
				m_ButtonStates = new ButtonState[ NumberOfButtons ] ;
				for( int buttonIndex  = 0 ; buttonIndex <  NumberOfButtons ; buttonIndex ++ )
				{
					m_ButtonStates[ buttonIndex ] = new ButtonState() ;	// Update 用
				}

				// 基準位置を初期化する
				m_PointerPosition = Position ;
			}

			/// <summary>
			/// フレーム毎の更新呼び出し
			/// </summary>
			public void Update( out bool button_0, out bool button_1, out bool button_2 )
			{
				button_0 = false ;
				button_1 = false ;
				button_2 = false ;

				//---------------------------------

				int buttonIndex ;
				int numberOfButtons = m_ButtonStates.GetLength( 0 ) ;

				ButtonState state ;

				float time = Time.realtimeSinceStartup ;

				for( buttonIndex  = 0 ; buttonIndex <  numberOfButtons ; buttonIndex ++ )
				{
					state = m_ButtonStates[ buttonIndex ] ;

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
							state.RepeatWakeTime = time ;
							state.RepeatLoopTime = time ;

							state.IsDown = true ;
						}
						else
						{
							// リピート最中
							if( ( time - state.RepeatWakeTime ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( time - state.RepeatLoopTime ) >= RepeatIntervalTime )
								{
									state.RepeatLoopTime = time ;

									state.IsRepeat = true ;
								}
							}
						}

						switch( buttonIndex )
						{
							case 0 : button_0 = true ; break ;
							case 1 : button_1 = true ; break ;
							case 2 : button_2 = true ; break ;
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

				//---------------------------------
				// 移動量を更新する

				var pointerPosition = Position ;
				m_PointerDelta    = pointerPosition - m_PointerPosition ;
				m_PointerPosition = pointerPosition ;
			}

			//-----------------------------------------------------------------------------------------

			/// <summary>
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

					if( Input.mousePresent == true )
					{
						// マウスは繋がっている
						mousePosition = Input.mousePosition ;
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
			/// ポインターの移動量
			/// </summary>
			public Vector3 Delta
			{
				get
				{
					Vector3 pointerDelta = Vector3.zero ;

					//---------------------------------------------------------
					// マウス処理

					if( Input.mousePresent == true )
					{
						// マウスは繋がっている
						pointerDelta = Input.mousePositionDelta ;
					}

					if( pointerDelta.x == 0 && pointerDelta.y == 0 )
					{
						// 入力が無い場合はタッチも含めた値を使用する
						pointerDelta = m_PointerDelta ;	// ひとまず描画フレームレートの値を使用する
					}

					return pointerDelta ;
				}
			}

			/// <summary>
			/// ポインターが画面内にあるかどうか
			/// </summary>
			public bool IsInside
			{
				get
				{
					// マウス判定
					if( Input.mousePresent == true )
					{
						// マウスは繋がっている
						var pointerPosition = Input.mousePosition ;

						float w = Screen.width ;
						float h = Screen.height ;

						return ( pointerPosition.x >= 0 && pointerPosition.y >= 0 && pointerPosition.x <  w && pointerPosition.y <  h ) ;
					}

					// タッチ判定
					return ( Input.touchCount == 1 ) ;
				}
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonNumber )
			{
				bool state = Input.GetMouseButton( buttonNumber ) ;

				// NewVersion と挙動を共通化するためボタン１・ボタン２のタッチエミュレーションは無効化する
				if( buttonNumber != 0 )
				{
					if( Input.touchCount >  0 )
					{
						state = false ;
					}
				}

				return state ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonNumber )
			{
				return m_ButtonStates[ buttonNumber ].IsDown ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonNumber )
			{
				return m_ButtonStates[ buttonNumber ].IsUp ;
			}

			/// <summary>
			/// リピート付きでボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonRepeat( int buttonNumber )
			{
				return m_ButtonStates[ buttonNumber ].IsRepeat ;
			}

			//----------------------------------

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			public Vector2 ScrollDelta
			{
				get
				{
					return Input.mouseScrollDelta ;
				}
			}
		}
	}
}

