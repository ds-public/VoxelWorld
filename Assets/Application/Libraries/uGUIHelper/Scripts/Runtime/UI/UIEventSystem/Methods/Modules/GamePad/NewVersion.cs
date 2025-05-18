#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// ゲームパッド制御
	/// </summary>
	public partial class GamePad
	{
		//-------------------------------------------------------------------------------------------
		// 新版

		/// <summary>
		/// 新版の実装
		/// </summary>
		public partial class Implementation_NewVersion : IImplementation
		{
			//------------------------------------------------------------------------------------------
			// 独自メソッド

			/// <summary>
			/// 全てのボタンが押されているかどうか判定する
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public int GetButtonAll( int playerNumber = -1 )
			{
				// ゲームパッドの場合はキーボードのマッピングもあるため単純にゲームパッドデバイスの有無で処理の終了は出来ない

				int buttonFlags = 0 ;

				//----------------------------------

				// 接続しているしているプレイヤー(最大４)
				int max = NumberOfGamePads <  MaximumNumberOfPlayers ? NumberOfGamePads : MaximumNumberOfPlayers ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了
					return buttonFlags ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamepad = Gamepad.all[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					if( SwapB1toB2 == false )
					{
						// ボタン１とボタン２の入れ替え：なし
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  0 ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  1 ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}
					else
					{
						// ボタン１とボタン２の入れ替え：あり
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  1 ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  0 ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}

					if( SwapB3toB4 == false )
					{
						// ボタン３とボタン４の入れ替え：なし
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  2 ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  3 ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}
					else
					{
						// ボタン３とボタン４の入れ替え：あり
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  3 ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  2 ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}

					if( GetGamePadButton( gamepad, profile.ButtonNumbers[  4 ] ) == true )
					{
						buttonFlags |= R1 ;
					}
					if( GetGamePadButton( gamepad, profile.ButtonNumbers[  5 ] ) == true )
					{
						buttonFlags |= L1 ;
					}

					if( profile.ButtonNumbers[  6 ] >= 0 )
					{
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[  6 ] ) == true )
						{
							buttonFlags |= R2 ;
						}
					}
					else
					{
						float axis = GetGamePadAxis( gamepad, profile.AxisNumbers[  6 ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= R2 ;
						}
					}

					if( profile.ButtonNumbers[  7 ] >= 0 )
					{
						if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 7 ] ) == true )
						{
							buttonFlags |= L2 ;
						}
					}
					else
					{
						float axis = GetGamePadAxis( gamepad, profile.AxisNumbers[  7 ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= L2 ;
						}
					}

					if( GetGamePadButton( gamepad, profile.ButtonNumbers[  8 ] ) == true )
					{
						buttonFlags |= R3 ;
					}
					if( GetGamePadButton( gamepad, profile.ButtonNumbers[  9 ] ) == true )
					{
						buttonFlags |= L3 ;
					}
					if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 10 ] ) == true )
					{
						buttonFlags |= O1 ;
					}
					if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 11 ] ) == true )
					{
						buttonFlags |= O2 ;
					}
				}

				//---------------------------------------------------------

				return buttonFlags ;
			}

			/// <summary>
			/// ボタンが押されているかどうか判定する
			/// </summary>
			/// <param name="buttonIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonIdentity, int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = NumberOfGamePads <  MaximumNumberOfPlayers ? NumberOfGamePads : MaximumNumberOfPlayers ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了
					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamepad = Gamepad.all[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					switch( buttonIdentity )
					{
						case B1 :
							if( SwapB1toB2 == false )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  0 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  1 ] ) == true ){ return true ; }
							}
						break ;

						case B2 :
							if( SwapB1toB2 == false )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  1 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  0 ] ) == true ){ return true ; }
							}
						break ;

						case B3 :
							if( SwapB3toB4 == false )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  2 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  3 ] ) == true ){ return true ; }
							}
						break ;

						case B4 :
							if( SwapB3toB4 == false )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  3 ] ) == true ){ return true ; }
							}
							else
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  2 ] ) == true ){ return true ; }
							}
						break ;

						case R1 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[  4 ] ) == true ){ return true ; }
						break ;

						case L1 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[  5 ] ) == true ){ return true ; }
						break ;

						case R2 :
							if( profile.ButtonNumbers[  6 ] >= 0 )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[  6 ] ) == true ){ return true ; }
							}
							else
							{
								float axis = GetGamePadAxis( gamepad, profile.AxisNumbers[  6 ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case L2 :
							if( profile.ButtonNumbers[  7 ] >= 0 )
							{
								if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 7 ] ) == true ){ return true ; }
							}
							else
							{
								float axis = GetGamePadAxis( gamepad, profile.AxisNumbers[  7 ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case R3 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[  8 ] ) == true ){ return true ; }
						break ;

						case L3 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[  9 ] ) == true ){ return true ; }
						break ;

						case O1 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 10 ] ) == true ){ return true ; }
						break ;

						case O2 :
							if( GetGamePadButton( gamepad, profile.ButtonNumbers[ 11 ] ) == true ){ return true ; }
						break ;
					}
				}

				return false ;
			}

			/// <summary>
			/// アクシスが押されているかどうか判定する
			/// </summary>
			/// <param name="axisIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public Vector2 GetAxis( int axisIdentity, int playerNumber = -1 )
			{
				float oAxisX = 0 ;
				float oAxisY = 0 ;

				//----------------------------------

				// 接続しているしているプレイヤー(最大４)
				int max = NumberOfGamePads <  MaximumNumberOfPlayers ? NumberOfGamePads : MaximumNumberOfPlayers ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					// 縦軸の符号反転
					if( m_Owner.Invert == true && axisIdentity != TriggerButton )
					{
						oAxisY = - oAxisY ;
					}

					return new Vector2( oAxisX, oAxisY ) ; ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamepad = Gamepad.all[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					float axisX, axisY ;

					switch( axisIdentity )
					{
						case DPad :
							// SCX
							axisX = GetGamePadAxis( gamepad, profile.AxisNumbers[  0 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SCY
							axisY = GetGamePadAxis( gamepad, profile.AxisNumbers[  1 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case LStick :
							// SLX
							axisX = GetGamePadAxis( gamepad, profile.AxisNumbers[  2 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SLY
							axisY = GetGamePadAxis( gamepad, profile.AxisNumbers[  3 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case RStick :
							// SRX
							axisX = GetGamePadAxis( gamepad, profile.AxisNumbers[  4 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SRY
							axisY = GetGamePadAxis( gamepad, profile.AxisNumbers[  5 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case TriggerButton :
							// R2
							axisX = GetGamePadAxis( gamepad, profile.AxisNumbers[  6 ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
								if( oAxisX <  0 )
								{
									oAxisX  = - oAxisX ;
								}
							}

							// L2
							axisY = GetGamePadAxis( gamepad, profile.AxisNumbers[  7 ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
								if( oAxisY <  0 )
								{
									oAxisY  = - oAxisY ;
								}
							}
						break ;
					}
				}

				// 縦軸の符号反転
				if( m_Owner.Invert == true && axisIdentity != TriggerButton )
				{
					oAxisY = - oAxisY ;
				}

				return new Vector2( oAxisX, oAxisY ) ;
			}

			//----------------------------------------------------------

			private static bool GetGamePadButton( Gamepad gamepad, int buttonNumber )
			{
				bool state = false ;

				switch( buttonNumber )
				{
					case  0 : state = gamepad.buttonSouth.isPressed			; break ;
					case  1 : state = gamepad.buttonEast.isPressed			; break ;
					case  2 : state = gamepad.buttonWest.isPressed			; break ;
					case  3 : state = gamepad.buttonNorth.isPressed			; break ;
					case  4 : state = gamepad.rightShoulder.isPressed		; break ;
					case  5 : state = gamepad.leftShoulder.isPressed		; break ;
					case  6 : state = gamepad.rightTrigger.isPressed		; break ;
					case  7 : state = gamepad.leftTrigger.isPressed			; break ;
					case  8 : state = gamepad.rightStickButton.isPressed	; break ;
					case  9 : state = gamepad.leftStickButton.isPressed		; break ;
					case 10 : state = gamepad.startButton.isPressed			; break ;
					case 11 : state = gamepad.selectButton.isPressed		; break ;
				}

				return state ;
			}

			private static float GetGamePadAxis( Gamepad gamepad, int axisNumber )
			{
				float value = 0 ;

				switch( axisNumber )
				{
					case  0 : value = ( gamepad.dpad.left.isPressed == true ? -1 : 0 ) + ( gamepad.dpad.right.isPressed == true ? +1 : 0 ) 	; break ;
					case  1 : value = ( gamepad.dpad.down.isPressed == true ? -1 : 0 ) + ( gamepad.dpad.up.isPressed    == true ? +1 : 0 ) 	; break ;
					case  2 : value = gamepad.leftStick.ReadValue().x	; break ;
					case  3 : value = gamepad.leftStick.ReadValue().y	; break ;
					case  4 : value = gamepad.rightStick.ReadValue().x	; break ;
					case  5 : value = gamepad.rightStick.ReadValue().y	; break ;
					case  6 : value = gamepad.rightTrigger.ReadValue()	; break ;
					case  7 : value = gamepad.leftTrigger.ReadValue()	; break ;
				}


				//---------------------------------

				float sign = Mathf.Sign( value ) ;
				value = value <  0 ? - value : value ;
				if( value <  m_AxisLowerThreshold )
				{
					// チャタリング防止
					value = 0 ;
				}
				else
				{
					if( value >  m_AxisUpperThreshold )
					{
						// 最大補正
						value = 1 ;
					}
					else
					{
						// フィッティング
						value = ( value - m_AxisLowerThreshold ) / ( m_AxisUpperThreshold - m_AxisLowerThreshold ) ;
					}
					value *= sign ;
				}

				//---------------------------------

				return value ;
			}

			//----------------------------------------------------------
			// 振動関係

			/// <summary>
			/// 振動を開始させる(範囲は 0～1)
			/// </summary>
			/// <param name="lowerSpeed"></param>
			/// <param name="upperSpeed"></param>
			/// <param name="duration"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool SetMotorSpeeds( float lowerSpeed, float upperSpeed, float duration = 1.0f, int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = NumberOfGamePads <  MaximumNumberOfPlayers ? NumberOfGamePads : MaximumNumberOfPlayers ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了
					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamepad = Gamepad.all[ p ] ;

					// プレイヤーの保持情報を設定する
					m_Players[ p ].SetHapticsState( true, duration ) ;

					gamepad.SetMotorSpeeds( lowerSpeed, upperSpeed ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を停止させる
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool StopMotor( int playerNumber = -1 )
			{
				// 接続しているしているプレイヤー(最大４)
				int max = NumberOfGamePads <  MaximumNumberOfPlayers ? NumberOfGamePads: MaximumNumberOfPlayers ;

				if( playerNumber <  -1 || playerNumber >= max )
				{
					// 全プレイヤーで判定指定
					playerNumber  = -1 ;
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了
					return false ;
				}

				int p, ps, pe ;

				if( playerNumber <  0 )
				{
					// 全プレイヤーで判定
					ps = 0 ;
					pe = max - 1 ;
				}
				else
				{
					// 各プレイヤーで判定
					ps = playerNumber ;
					pe = playerNumber ;
				}

				for( p  = ps ; p <= pe ; p ++ )
				{
					var gamepad = Gamepad.all[ p ] ;

					// プレイヤーの保持情報を設定する
					m_Players[ p ].SetHapticsState( false, 0 ) ;

					gamepad.SetMotorSpeeds( 0, 0 ) ;
				}

				return true ;
			}

			/// <summary>
			/// 振動を一時停止させる
			/// </summary>
			/// <returns></returns>
			public bool PauseHaptics()
			{
				if( NumberOfGamePads == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				// 一時停止
				InputSystem.PauseHaptics() ;

				return true ;
			}

			/// <summary>
			/// 振動を再開させる
			/// </summary>
			/// <returns></returns>
			public bool ResumeHaptics()
			{
				if( NumberOfGamePads == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				// 一時停止
				InputSystem.ResumeHaptics() ;

				return true ;
			}

			/// <summary>
			/// 振動を停止させる(パラメータもリセットされる)
			/// </summary>
			/// <returns></returns>
			public bool ResetHaptics()
			{
				if( NumberOfGamePads == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					return false ;
				}

				// 一時停止
				InputSystem.ResetHaptics() ;

				return true ;
			}
		}
	}
}
#endif
