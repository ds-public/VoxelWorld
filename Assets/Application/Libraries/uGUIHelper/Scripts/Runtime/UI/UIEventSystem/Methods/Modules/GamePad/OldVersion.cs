using System ;
using System.Collections.Generic ;
using UnityEngine ;


namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// ゲームパッド制御
	/// </summary>
	public partial class GamePad
	{
		//-------------------------------------------------------------------------------------------
		// 旧版

		/// <summary>
		/// 旧版の実装
		/// </summary>
		public partial class Implementation_OldVersion : IImplementation
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
					var buttonNames	= m_ButtonNames[ p ] ;
					var axisNames	= m_AxisNames[ p ] ;

					// プロファイル取得
					var profile	= m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					if( SwapB1toB2 == false )
					{
						// ボタン１とボタン２の入れ替え：なし
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}
					else
					{
						// ボタン１とボタン２の入れ替え：あり
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}

					if( SwapB3toB4 == false )
					{
						// ボタン３とボタン４の入れ替え：なし
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}
					else
					{
						// ボタン３とボタン４の入れ替え：あり
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}

					//------------

					// DPad_R
					if( profile.ButtonNumbers[  4 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  4 ] ] ) == true )
						{
							buttonFlags |= DPad_R ;
						}
					}
					else
					{
						var axis =   GetAxis_Compatible( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= DPad_R ;
						}
					}

					// DPad_L
					if( profile.ButtonNumbers[  5 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  5 ] ] ) == true )
						{
							buttonFlags |= DPad_L ;
						}
					}
					else
					{
						var axis = - GetAxis_Compatible( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= DPad_L ;
						}
					}

					// DPad_U
					if( profile.ButtonNumbers[  6 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  6 ] ] ) == true )
						{
							buttonFlags |= DPad_U ;
						}
					}
					else
					{
						var axis =   GetAxis_Compatible( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= DPad_U ;
						}
					}

					// DPad_D
					if( profile.ButtonNumbers[  7 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  7 ] ] ) == true )
						{
							buttonFlags |= DPad_D ;
						}
					}
					else
					{
						var axis = - GetAxis_Compatible( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= DPad_D ;
						}
					}

					//------------

					// R1
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  8 ] ] ) == true )
					{
						buttonFlags |= R1 ;
					}

					// L1
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  9 ] ] ) == true )
					{
						buttonFlags |= L1 ;
					}

					// R2
					if( profile.ButtonNumbers[ 10 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 10 ] ] ) == true )
						{
							buttonFlags |= R2 ;
						}
					}
					else
					{
						var axis = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
						if( profile.AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= R2 ;
						}
					}

					// L2
					if( profile.ButtonNumbers[  7 ] >= 0 )
					{
						if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 11 ] ] ) == true )
						{
							buttonFlags |= L2 ;
						}
					}
					else
					{
						var axis = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
						if( profile.AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= L2 ;
						}
					}

					// R3
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 12 ] ] ) == true )
					{
						buttonFlags |= R3 ;
					}

					// L3
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 13 ] ] ) == true )
					{
						buttonFlags |= L3 ;
					}

					//------------

					// O1
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 14 ] ] ) == true )
					{
						buttonFlags |= O1 ;
					}

					// O2
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 15 ] ] ) == true )
					{
						buttonFlags |= O2 ;
					}

					// O3
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 16 ] ] ) == true )
					{
						buttonFlags |= O3 ;
					}

					// O4
					if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 17 ] ] ) == true )
					{
						buttonFlags |= O4 ;
					}
				}

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
					var buttonNames	= m_ButtonNames[ p ] ;
					var axisNames	= m_AxisNames[ p ] ;

					// プロファイル取得
					var profile	= m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					switch( buttonIdentity )
					{
						case B1 :
							if( SwapB1toB2 == false )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true ){ return true ; }
							}
						break ;

						case B2 :
							if( SwapB1toB2 == false )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true ){ return true ; }
							}
						break ;

						case B3 :
							if( SwapB3toB4 == false )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true ){ return true ; }
							}
						break ;

						case B4 :
							if( SwapB3toB4 == false )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true ){ return true ; }
							}
						break ;

						case DPad_R :
							if( profile.ButtonNumbers[  4 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  4 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis =   GetAxis_Compatible( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case DPad_L :
							if( profile.ButtonNumbers[  5 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  5 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = - GetAxis_Compatible( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case DPad_U :
							if( profile.ButtonNumbers[  6 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  6 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis =   GetAxis_Compatible( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case DPad_D :
							if( profile.ButtonNumbers[  7 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  7 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = - GetAxis_Compatible( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case R1 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  8 ] ] ) == true ){ return true ; }
						break ;

						case L1 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[  9 ] ] ) == true ){ return true ; }
						break ;

						case R2 :
							if( profile.ButtonNumbers[ 10 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 10 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
								if( profile.AnalogButtonCorrection == true )
								{
									axis = axis * 0.5f + 0.5f ;
								}
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case L2 :
							if( profile.ButtonNumbers[ 11 ] >= 0 )
							{
								if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 11 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
								if( profile.AnalogButtonCorrection == true )
								{
									axis = axis * 0.5f + 0.5f ;
								}
								if( axis >= profile.AnalogButtonThreshold )
								{
									return true ;
								}
							}
						break ;

						case R3 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 12 ] ] ) == true ){ return true ; }
						break ;

						case L3 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 13 ] ] ) == true ){ return true ; }
						break ;

						case O1 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 14 ] ] ) == true ){ return true ; }
						break ;

						case O2 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 15 ] ] ) == true ){ return true ; }
						break ;

						case O3 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 16 ] ] ) == true ){ return true ; }
						break ;

						case O4 :
							if( GetButton_Compatible( buttonNames[ profile.ButtonNumbers[ 17 ] ] ) == true ){ return true ; }
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

					// ゲームパッドの接続数が０なら処理はここで終了

					// 縦軸の符号反転
					if( m_Owner.Invert == true && axisIdentity != TriggerButton )
					{
						oAxisY = - oAxisY ;
					}

					return new Vector2( oAxisX, oAxisY ) ;
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
					var axisNames	= m_AxisNames[ p ] ;

					// プロファイル取得
					var profile = m_Profiles[ m_Players[ p ].ProfileNumber ] ;

					//--------------------------------
					// 以下判定

					float axisX, axisY ;

					switch( axisIdentity )
					{
						case DPad :
							// SCX
							axisX = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SCY
							axisY = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case LStick :
							// SLX
							axisX = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  2 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX =   axisX ;
							}

							// SLY
							axisY = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  3 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = - axisY ;
							}
						break ;

						case RStick :
							// SRX
							axisX = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  4 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX =   axisX ;
							}

							// SRY
							axisY = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  5 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = - axisY ;
							}
						break ;

						case TriggerButton :
							// R2
							axisX = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
							if( profile.AnalogButtonCorrection == true )
							{
								axisX = axisX * 0.5f + 0.5f ;
							}
							if( axisX != 0 )
							{
								oAxisX = axisX ;
								if( oAxisX <  0 )
								{
									oAxisX  = - oAxisX ;
								}
							}

							// L2
							axisY = GetAxis_Compatible( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
							if( profile.AnalogButtonCorrection == true )
							{
								axisY = axisY * 0.5f + 0.5f ;
							}
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
				// 旧版では振動は全面的に使用できない
				Debug.LogWarning( "Vibration function is not available on older input systems." ) ;
				return false ;
			}

			/// <summary>
			/// 振動を停止させる
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			public bool StopMotor( int playerNumber = -1 )
			{
				// 旧版では振動は全面的に使用できない
				Debug.LogWarning( "Vibration function is not available on older input systems." ) ;
				return false ;
			}

			/// <summary>
			/// 振動を一時停止させる
			/// </summary>
			/// <returns></returns>
			public bool PauseHaptics()
			{
				// 旧版では振動は全面的に使用できない
				Debug.LogWarning( "Vibration function is not available on older input systems." ) ;
				return false ;
			}

			/// <summary>
			/// 振動を再開させる
			/// </summary>
			/// <returns></returns>
			public bool ResumeHaptics()
			{
				// 旧版では振動は全面的に使用できない
				Debug.LogWarning( "Vibration function is not available on older input systems." ) ;
				return false ;
			}

			/// <summary>
			/// 振動を停止させる(パラメータもリセットされる)
			/// </summary>
			/// <returns></returns>
			public bool ResetHaptics()
			{
				// 旧版では振動は全面的に使用できない
				Debug.LogWarning( "Vibration function is not available on older input systems." ) ;
				return false ;
			}
		}
	}
}
