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

			/// <summary>
			/// アクシス用状態
			/// </summary>
			public class AxisState
			{
				public int      AxisNumber ;

				public bool		RepeatKeepFlag ;
				public Vector2  RepeatKeepData ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public Vector2	IsRepeat ;
				public Vector2	IsDown ;
				public Vector2	IsUp ;
			}

			private AxisState[] m_AxisStates ;

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

				// アクシスの状態
				m_AxisStates = new AxisState[ 1 ] ;
				m_AxisStates[ 0 ] = new AxisState(){ AxisNumber = Mouse.Wheel_Roll } ;

				// 基準位置を初期化する
				m_PointerPosition = Position ;
			}

			/// <summary>
			/// フレーム毎の更新呼び出し
			/// </summary>
			public void Update( out bool button_0, out bool button_1, out bool button_2, out bool button_3, out bool button_4 )
			{
				button_0 = false ;
				button_1 = false ;
				button_2 = false ;
				button_3 = false ;
				button_4 = false ;

				//-----------------------------------------------------------------------------
				// ホイールに関する特殊処理を実行する

				UpdateWheelButton() ;

				float time = Time.realtimeSinceStartup ;

				//-----------------------------------------------------------------------------

				int buttonIndex ;
				int numberOfButtons = m_ButtonStates.GetLength( 0 ) ;

				ButtonState buttonState ;

				for( buttonIndex  = 0 ; buttonIndex <  numberOfButtons ; buttonIndex ++ )
				{
					buttonState = m_ButtonStates[ buttonIndex ] ;

					//---------------------------------

					buttonState.IsRepeat	= false ;
					buttonState.IsDown	= false ;
					buttonState.IsUp		= false ;

					if( GetButton( buttonIndex ) == true )
					{
						if( buttonState.RepeatKeepFlag == false )
						{
							// リピート開始
							buttonState.IsRepeat	= true ;

							buttonState.RepeatKeepFlag = true ;
							buttonState.RepeatWakeTime = time ;
							buttonState.RepeatLoopTime = time ;

							buttonState.IsDown = true ;
						}
						else
						{
							// リピート最中
							if( ( time - buttonState.RepeatWakeTime ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( time - buttonState.RepeatLoopTime ) >= RepeatIntervalTime )
								{
									buttonState.RepeatLoopTime = time ;

									buttonState.IsRepeat = true ;
								}
							}
						}

						switch( buttonIndex )
						{
							case 0 : button_0 = true ; break ;
							case 1 : button_1 = true ; break ;
							case 2 : button_2 = true ; break ;
							case 3 : button_3 = true ; break ;
							case 4 : button_4 = true ; break ;
						}
					}
					else
					{
						// リピート解除
						if( buttonState.RepeatKeepFlag == true )
						{
							buttonState.IsUp = true ;

							buttonState.RepeatKeepFlag  = false ;
						}
					}
				}

				//-----------------------------------------------------------------------------
				// アクシスに関する更新処理

				int axisIndex ;
				int numberOfAxs = m_AxisStates.GetLength( 0 ) ;

				AxisState axisState ;
				Vector2 axis ;

				for( axisIndex  = 0 ; axisIndex <  numberOfAxs ; axisIndex ++ )
				{
					axisState = m_AxisStates[ axisIndex ] ;

					//---------------------------------

					axisState.IsRepeat	= Vector2.zero ;
					axisState.IsDown	= Vector2.zero ;
					axisState.IsUp		= Vector2.zero ;

					axis = GetAxis( axisState.AxisNumber ) ;

					if( axis.x != 0 && axis.y != 0 )
					{
						if( axisState.RepeatKeepFlag == false )
						{
							// リピート開始
							axisState.IsRepeat	= axis ;

							axisState.RepeatKeepFlag = true ;
							axisState.RepeatKeepData = axis ;
							axisState.RepeatWakeTime = time ;
							axisState.RepeatLoopTime = time ;

							axisState.IsDown = axis ;
						}
						else
						{
							// リピート最中
							if( ( time - axisState.RepeatWakeTime ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( time - axisState.RepeatLoopTime ) >= RepeatIntervalTime )
								{
									axisState.IsRepeat = axis ;

									axisState.RepeatLoopTime = time ;
								}
							}
						}
					}
					else
					{
						// リピート解除
						if( axisState.RepeatKeepFlag == true )
						{
							axisState.IsUp = axisState.RepeatKeepData ;

							axisState.RepeatKeepFlag = false ;
							axisState.RepeatKeepData = Vector2.zero ;
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

			//----------------------------------------------------------

			// ホイール回転(上)
			private bool  m_IsWheelRolling_U = false ;
			private float m_WheelRollingVelocity_U = 0 ;
			private float m_WheelRollingStaringTime_U = 0 ;

			// ホイール回転(下)
			private bool  m_IsWheelRolling_D = false ;
			private float m_WheelRollingVelocity_D = 0 ;
			private float m_WheelRollingStaringTime_D = 0 ;

			// ホイールをボタンとして扱う場合の状態値を更新する
			private void UpdateWheelButton()
			{
				float dy = Input.mouseScrollDelta.y ;

				// ↑回転
				if( m_IsWheelRolling_U == false )
				{
					if( dy >  0 )
					{
						m_IsWheelRolling_U = true ;

						m_WheelRollingVelocity_U  = dy ;

						// 時間設定
						m_WheelRollingStaringTime_U = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					if( dy >  0 )
					{
						m_WheelRollingVelocity_U += dy ;

						// 時間更新
						m_WheelRollingStaringTime_U = Time.realtimeSinceStartup ;
					}
					else
					{
						if( ( Time.realtimeSinceStartup - m_WheelRollingStaringTime_U ) >= 0.1f )
						{
							// 解放
							m_IsWheelRolling_U = false ;
							m_WheelRollingVelocity_U = 0 ;
						}
					}
				}

				// ↓回転
				if( m_IsWheelRolling_D == false )
				{
					if( dy <  0 )
					{
						m_IsWheelRolling_D = true ;

						m_WheelRollingVelocity_D  = dy ;

						// 時間設定
						m_WheelRollingStaringTime_D = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					if( dy <  0 )
					{
						m_WheelRollingVelocity_D -= dy ;

						// 時間更新
						m_WheelRollingStaringTime_D = Time.realtimeSinceStartup ;
					}
					else
					{
						if( ( Time.realtimeSinceStartup - m_WheelRollingStaringTime_D ) >= 0.1f )
						{
							// 解放
							m_IsWheelRolling_D = false ;
							m_WheelRollingVelocity_D = 0 ;
						}
					}
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

				if( buttonNumber >= 0 && buttonNumber <= 2 )
				{
					state = Input.GetMouseButton( buttonNumber ) ;

					// NewVersion と挙動を共通化するためボタン１・ボタン２のタッチエミュレーションは無効化する
					if( buttonNumber != 0 )
					{
						if( Input.touchCount >  0 )
						{
							state = false ;
						}
					}
				}
				else
				if( buttonNumber >= 3 && buttonNumber <= 4 )
				{
					switch( buttonNumber )
					{
						case 3 :
						{
							state = ( m_WheelRollingVelocity_U >  0 ) ; // 閾値に大きな値が必要かと思ったが 1 ずつしか変化しない模様
						}
						break ;
						case 4 :
						{
							state = ( m_WheelRollingVelocity_D <  0 ) ; // 閾値に大きな値が必要かと思ったが 1 ずつしか変化しない模様
						}
						break ;
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
			/// アクシスが押されているかどうかの判定
			/// </summary>
			/// <param name="axisNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxis( int axisNumber )
			{
				if( axisNumber == Mouse.Wheel_Roll )
				{
					float y = 0 ;

					if( m_WheelRollingVelocity_U >  0 )
					{
						y += 1.0f ;
					}
					if( m_WheelRollingVelocity_D <  0 )
					{
						y -= 1.0f ;
					}

					return new Vector2( 0, y ) ;
				}

				return Vector2.zero ;
			}

			/// <summary>
			/// アクシスが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxisDown( int axisNumber )
			{
				int axisIndex = 0 ;
				switch( axisNumber )
				{
					case Mouse.Wheel_Roll :
						axisIndex = 0 ;
					break ;
				}

				return m_AxisStates[ axisIndex ].IsDown ;
			}

			/// <summary>
			/// アクシスが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxisUp( int axisNumber )
			{
				int axisIndex = 0 ;
				switch( axisNumber )
				{
					case Mouse.Wheel_Roll :
						axisIndex = 0 ;
					break ;
				}

				return m_AxisStates[ axisIndex ].IsUp ;
			}

			/// <summary>
			/// リピート付きでアクシスが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxisRepeat( int axisNumber )
			{
				int axisIndex = 0 ;
				switch( axisNumber )
				{
					case Mouse.Wheel_Roll :
						axisIndex = 0 ;
					break ;
				}

				return m_AxisStates[ axisIndex ].IsRepeat ;
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

	}   // class
}   // namespace

