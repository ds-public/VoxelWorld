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
			private static readonly Dictionary<KeyCodes, KeyCode> m_KeyCodeMapper = new ()
			{
				{ KeyCodes.Backspace,		KeyCode.Backspace		},
				{ KeyCodes.Delete,			KeyCode.Delete			},
				{ KeyCodes.Tab,				KeyCode.Tab				},
				{ KeyCodes.Clear,			KeyCode.Clear			},
				{ KeyCodes.Return,			KeyCode.Return			},
				{ KeyCodes.Pause,			KeyCode.Pause			},
				{ KeyCodes.Escape,			KeyCode.Escape			},
				{ KeyCodes.Space,			KeyCode.Space			},

				{ KeyCodes.Keypad0,			KeyCode.Keypad0			},
				{ KeyCodes.Keypad1,			KeyCode.Keypad1			},
				{ KeyCodes.Keypad2,			KeyCode.Keypad2			},
				{ KeyCodes.Keypad3,			KeyCode.Keypad3			},
				{ KeyCodes.Keypad4,			KeyCode.Keypad4			},
				{ KeyCodes.Keypad5,			KeyCode.Keypad5			},
				{ KeyCodes.Keypad6,			KeyCode.Keypad6			},
				{ KeyCodes.Keypad7,			KeyCode.Keypad7			},
				{ KeyCodes.Keypad8,			KeyCode.Keypad8			},
				{ KeyCodes.Keypad9,			KeyCode.Keypad9			},

				{ KeyCodes.KeypadPeriod,	KeyCode.KeypadPeriod	},
				{ KeyCodes.KeypadDivide,	KeyCode.KeypadDivide	},
				{ KeyCodes.KeypadMultiply,	KeyCode.KeypadMultiply	},
				{ KeyCodes.KeypadMinus,		KeyCode.KeypadMinus		},
				{ KeyCodes.KeypadPlus,		KeyCode.KeypadPlus		},
				{ KeyCodes.KeypadEnter,		KeyCode.KeypadEnter		},
				{ KeyCodes.KeypadEquals,	KeyCode.KeypadEquals	},

				{ KeyCodes.UpArrow,			KeyCode.UpArrow			},
				{ KeyCodes.DownArrow,		KeyCode.DownArrow		},
				{ KeyCodes.RightArrow,		KeyCode.RightArrow		},
				{ KeyCodes.LeftArrow,		KeyCode.LeftArrow		},

				{ KeyCodes.Insert,			KeyCode.Insert			},
				{ KeyCodes.Home,			KeyCode.Home			},
				{ KeyCodes.End,				KeyCode.End				},
				{ KeyCodes.PageUp,			KeyCode.PageUp			},
				{ KeyCodes.PageDown,		KeyCode.PageDown		},
				
				{ KeyCodes.F1,				KeyCode.F1		},
				{ KeyCodes.F2,				KeyCode.F2		},
				{ KeyCodes.F3,				KeyCode.F3		},
				{ KeyCodes.F4,				KeyCode.F4		},
				{ KeyCodes.F5,				KeyCode.F5		},
				{ KeyCodes.F6,				KeyCode.F6		},
				{ KeyCodes.F7,				KeyCode.F7		},
				{ KeyCodes.F8,				KeyCode.F8		},
				{ KeyCodes.F9,				KeyCode.F9		},
				{ KeyCodes.F10,				KeyCode.F10		},
				{ KeyCodes.F11,				KeyCode.F11		},
				{ KeyCodes.F12,				KeyCode.F12		},
				{ KeyCodes.F13,				KeyCode.F13		},
				{ KeyCodes.F14,				KeyCode.F14		},
				{ KeyCodes.F15,				KeyCode.F15		},

				{ KeyCodes.Alpha0,			KeyCode.Alpha0	},
				{ KeyCodes.Alpha1,			KeyCode.Alpha1	},
				{ KeyCodes.Alpha2,			KeyCode.Alpha2	},
				{ KeyCodes.Alpha3,			KeyCode.Alpha3	},
				{ KeyCodes.Alpha4,			KeyCode.Alpha4	},
				{ KeyCodes.Alpha5,			KeyCode.Alpha5	},
				{ KeyCodes.Alpha6,			KeyCode.Alpha6	},
				{ KeyCodes.Alpha7,			KeyCode.Alpha7	},
				{ KeyCodes.Alpha8,			KeyCode.Alpha8	},
				{ KeyCodes.Alpha9,			KeyCode.Alpha9	},

				{ KeyCodes.Exclaim,			KeyCode.Exclaim			},
				{ KeyCodes.DoubleQuote,		KeyCode.DoubleQuote		},
				{ KeyCodes.Hash,			KeyCode.Hash			},
				{ KeyCodes.Dollar,			KeyCode.Dollar			},
				{ KeyCodes.Percent,			KeyCode.Percent			},
				{ KeyCodes.Ampersand,		KeyCode.Ampersand		},
				{ KeyCodes.Quote,			KeyCode.Quote			},
				{ KeyCodes.LeftParen,		KeyCode.LeftParen		},
				{ KeyCodes.RightParen,		KeyCode.RightParen		},
				{ KeyCodes.Asterisk,		KeyCode.Asterisk		},
				{ KeyCodes.Plus,			KeyCode.Plus			},
				{ KeyCodes.Comma,			KeyCode.Comma			},
				{ KeyCodes.Minus,			KeyCode.Minus			},
				{ KeyCodes.Period,			KeyCode.Period			},
				{ KeyCodes.Slash,			KeyCode.Slash			},
				{ KeyCodes.Colon,			KeyCode.Colon			},
				{ KeyCodes.Semicolon,		KeyCode.Semicolon		},
				{ KeyCodes.Less,			KeyCode.Less			},
				{ KeyCodes.Equals,			KeyCode.Equals			},
				{ KeyCodes.Greater,			KeyCode.Greater			},
				{ KeyCodes.Question,		KeyCode.Question		},
				{ KeyCodes.At,				KeyCode.At				},
				{ KeyCodes.LeftBracket,		KeyCode.RightBracket	},  // 左右逆転しているバグあり
				{ KeyCodes.Backslash,		KeyCode.Backslash		},
				{ KeyCodes.RightBracket,	KeyCode.LeftBracket     },  // 左右逆転しているバグあり
				{ KeyCodes.Caret,			KeyCode.Caret			},
				{ KeyCodes.Underscore,		KeyCode.Underscore		},
				{ KeyCodes.BackQuote,		KeyCode.BackQuote		},

				{ KeyCodes.A,				KeyCode.A	},
				{ KeyCodes.B,				KeyCode.B	},
				{ KeyCodes.C,				KeyCode.C	},
				{ KeyCodes.D,				KeyCode.D	},
				{ KeyCodes.E,				KeyCode.E	},
				{ KeyCodes.F,				KeyCode.F	},
				{ KeyCodes.G,				KeyCode.G	},
				{ KeyCodes.H,				KeyCode.H	},
				{ KeyCodes.I,				KeyCode.I	},
				{ KeyCodes.J,				KeyCode.J	},
				{ KeyCodes.K,				KeyCode.K	},
				{ KeyCodes.L,				KeyCode.L	},
				{ KeyCodes.M,				KeyCode.M	},
				{ KeyCodes.N,				KeyCode.N	},
				{ KeyCodes.O,				KeyCode.O	},
				{ KeyCodes.P,				KeyCode.P	},
				{ KeyCodes.Q,				KeyCode.Q	},
				{ KeyCodes.R,				KeyCode.R	},
				{ KeyCodes.S,				KeyCode.S	},
				{ KeyCodes.T,				KeyCode.T	},
				{ KeyCodes.U,				KeyCode.U	},
				{ KeyCodes.V,				KeyCode.V	},
				{ KeyCodes.W,				KeyCode.W	},
				{ KeyCodes.X,				KeyCode.X	},
				{ KeyCodes.Y,				KeyCode.Y	},
				{ KeyCodes.Z,				KeyCode.Z	},

				{ KeyCodes.LeftCurlyBracket,	KeyCode.LeftCurlyBracket	},
				{ KeyCodes.Pipe,				KeyCode.Pipe				},
				{ KeyCodes.RightCurlyBracket,	KeyCode.RightCurlyBracket	},
				{ KeyCodes.Tilde,				KeyCode.Tilde				},
				{ KeyCodes.Numlock,				KeyCode.Numlock				},
				{ KeyCodes.CapsLock,			KeyCode.CapsLock			},
				{ KeyCodes.ScrollLock,			KeyCode.ScrollLock			},
				{ KeyCodes.RightShift,			KeyCode.RightShift			},
				{ KeyCodes.LeftShift,			KeyCode.LeftShift			},
				{ KeyCodes.RightControl,		KeyCode.RightControl		},
				{ KeyCodes.LeftControl,			KeyCode.LeftControl			},
				{ KeyCodes.RightAlt,			KeyCode.RightAlt			},
				{ KeyCodes.LeftAlt,				KeyCode.LeftAlt				},
				{ KeyCodes.LeftMeta,			KeyCode.LeftMeta			},
				{ KeyCodes.LeftCommand,			KeyCode.LeftCommand			},
				{ KeyCodes.LeftApple,			KeyCode.LeftApple			},
				{ KeyCodes.LeftWindows,			KeyCode.LeftWindows			},
				{ KeyCodes.RightMeta,			KeyCode.RightMeta			},
				{ KeyCodes.RightCommand,		KeyCode.RightCommand		},
				{ KeyCodes.RightApple,			KeyCode.RightApple			},
				{ KeyCodes.RightWindows,		KeyCode.RightWindows		},
				{ KeyCodes.AltGr,				KeyCode.AltGr				},
				{ KeyCodes.Help,				KeyCode.Help				},
				{ KeyCodes.Print,				KeyCode.Print				},
				{ KeyCodes.SysReq,				KeyCode.SysReq				},
				{ KeyCodes.Break,				KeyCode.Break				},
				{ KeyCodes.Menu,				KeyCode.Menu				},
			} ;

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

				//---------------------------------

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true && Enabled == true )
					{
						// キーボードのキーのボタンへの割り当て(ループでは回さない)
						if( GetButtonByMappingKey( GamePad.B1 ) == true ){ buttonFlags |= B1 ; }
						if( GetButtonByMappingKey( GamePad.B2 ) == true ){ buttonFlags |= B2 ; }
						if( GetButtonByMappingKey( GamePad.B3 ) == true ){ buttonFlags |= B3 ; }
						if( GetButtonByMappingKey( GamePad.B4 ) == true ){ buttonFlags |= B4 ; }

						if( GetButtonByMappingKey( GamePad.R1 ) == true ){ buttonFlags |= R1 ; }
						if( GetButtonByMappingKey( GamePad.L1 ) == true ){ buttonFlags |= L1 ; }
						if( GetButtonByMappingKey( GamePad.R2 ) == true ){ buttonFlags |= R2 ; }
						if( GetButtonByMappingKey( GamePad.L2 ) == true ){ buttonFlags |= L2 ; }
						if( GetButtonByMappingKey( GamePad.R3 ) == true ){ buttonFlags |= R3 ; }
						if( GetButtonByMappingKey( GamePad.L3 ) == true ){ buttonFlags |= L3 ; }

						if( GetButtonByMappingKey( GamePad.O1 ) == true ){ buttonFlags |= O1 ; }
						if( GetButtonByMappingKey( GamePad.O2 ) == true ){ buttonFlags |= O2 ; }
						if( GetButtonByMappingKey( GamePad.O3 ) == true ){ buttonFlags |= O3 ; }
						if( GetButtonByMappingKey( GamePad.O4 ) == true ){ buttonFlags |= O4 ; }
					}
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
						if( GetButton( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetButton( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}
					else
					{
						// ボタン１とボタン２の入れ替え：あり
						if( GetButton( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true )
						{
							buttonFlags |= B1 ;
						}
						if( GetButton( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true )
						{
							buttonFlags |= B2 ;
						}
					}

					if( SwapB3toB4 == false )
					{
						// ボタン３とボタン４の入れ替え：なし
						if( GetButton( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetButton( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}
					else
					{
						// ボタン３とボタン４の入れ替え：あり
						if( GetButton( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true )
						{
							buttonFlags |= B3 ;
						}
						if( GetButton( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true )
						{
							buttonFlags |= B4 ;
						}
					}

					if( GetButton( buttonNames[ profile.ButtonNumbers[  4 ] ] ) == true )
					{
						buttonFlags |= R1 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[  5 ] ] ) == true )
					{
						buttonFlags |= L1 ;
					}

					if( profile.ButtonNumbers[  6 ] >= 0 )
					{
						if( GetButton( buttonNames[ profile.ButtonNumbers[  6 ] ] ) == true )
						{
							buttonFlags |= R2 ;
						}
					}
					else
					{
						var axis = GetAxis( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
						if( profile.AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= R2 ;
						}
					}

					if( profile.ButtonNumbers[  7 ] >= 0 )
					{
						if( GetButton( buttonNames[ profile.ButtonNumbers[  7 ] ] ) == true )
						{
							buttonFlags |= L2 ;
						}
					}
					else
					{
						var axis = GetAxis( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
						if( profile.AnalogButtonCorrection == true )
						{
							axis = axis * 0.5f + 0.5f ;
						}
						if( axis >= profile.AnalogButtonThreshold )
						{
							buttonFlags |= L2 ;
						}
					}

					if( GetButton( buttonNames[ profile.ButtonNumbers[  8 ] ] ) == true )
					{
						buttonFlags |= R3 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[  9 ] ] ) == true )
					{
						buttonFlags |= L3 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[ 10 ] ] ) == true )
					{
						buttonFlags |= O1 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[ 11 ] ] ) == true )
					{
						buttonFlags |= O2 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[ 12 ] ] ) == true )
					{
						buttonFlags |= O3 ;
					}
					if( GetButton( buttonNames[ profile.ButtonNumbers[ 13 ] ] ) == true )
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

				//---------------------------------

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true && Enabled == true )
					{
						// キーボードのキーのボタンへの割り当て(ループでは回さない)
						if( GetButtonByMappingKey( buttonIdentity ) == true )
						{
							return true ;
						}
					}
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
								if( GetButton( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true ){ return true ; }
							}
						break ;

						case B2 :
							if( SwapB1toB2 == false )
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  1 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  0 ] ] ) == true ){ return true ; }
							}
						break ;

						case B3 :
							if( SwapB3toB4 == false )
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true ){ return true ; }
							}
						break ;

						case B4 :
							if( SwapB3toB4 == false )
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  3 ] ] ) == true ){ return true ; }
							}
							else
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  2 ] ] ) == true ){ return true ; }
							}
						break ;

						case R1 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[  4 ] ] ) == true ){ return true ; }
						break ;

						case L1 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[  5 ] ] ) == true ){ return true ; }
						break ;

						case R2 :
							if( profile.ButtonNumbers[  6 ] >= 0 )
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  6 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = GetAxis( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
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
							if( profile.ButtonNumbers[  7 ] >= 0 )
							{
								if( GetButton( buttonNames[ profile.ButtonNumbers[  7 ] ] ) == true ){ return true ; }
							}
							else
							{
								var axis = GetAxis( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
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
							if( GetButton( buttonNames[ profile.ButtonNumbers[  8 ] ] ) == true ){ return true ; }
						break ;

						case L3 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[  9 ] ] ) == true ){ return true ; }
						break ;

						case O1 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[ 10 ] ] ) == true ){ return true ; }
						break ;

						case O2 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[ 11 ] ] ) == true ){ return true ; }
						break ;

						case O3 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[ 12 ] ] ) == true ){ return true ; }
						break ;

						case O4 :
							if( GetButton( buttonNames[ profile.ButtonNumbers[ 13 ] ] ) == true ){ return true ; }
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
			
				//---------------------------------

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToAxisEnabled == true && Enabled == true )
					{
						// キーボードのキーのアクシスへの割り当て

						// ＷＡＳＤ
						GetAxisByMappingKey_WASD( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// カーソル
						GetAxisByMappingKey_Cursor( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// ナンバー
						GetAxisByMappingKey_Number( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// 右側記号
						GetAxisByMappingKey_RightSymbol( axisIdentity, ref oAxisX, ref oAxisY ) ;

						// カスタム
						GetAxisByMappingKey_Custom( axisIdentity, ref oAxisX, ref oAxisY ) ;
					}
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					// ゲームパッドの接続数が０なら処理はここで終了

					// 縦軸の符号反転
					if( m_Owner.Invert == true && axisIdentity != TB )
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
						case DP :
							// SCX
							axisX = GetAxis( axisNames[ profile.AxisNumbers[  0 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX = axisX ;
							}

							// SCY
							axisY = GetAxis( axisNames[ profile.AxisNumbers[  1 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = axisY ;
							}
						break ;

						case LS :
							// SLX
							axisX = GetAxis( axisNames[ profile.AxisNumbers[  2 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX =   axisX ;
							}

							// SLY
							axisY = GetAxis( axisNames[ profile.AxisNumbers[  3 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = - axisY ;
							}
						break ;

						case RS :
							// SRX
							axisX = GetAxis( axisNames[ profile.AxisNumbers[  4 ] ] ) ;
							if( axisX != 0 )
							{
								oAxisX =   axisX ;
							}

							// SRY
							axisY = GetAxis( axisNames[ profile.AxisNumbers[  5 ] ] ) ;
							if( axisY != 0 )
							{
								oAxisY = - axisY ;
							}
						break ;

						case TB :
							// R2
							axisX = GetAxis( axisNames[ profile.AxisNumbers[  6 ] ] ) ;
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
							axisY = GetAxis( axisNames[ profile.AxisNumbers[  7 ] ] ) ;
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
				if( m_Owner.Invert == true && axisIdentity != TB )
				{
					oAxisY = - oAxisY ;
				}

				return new Vector2( oAxisX, oAxisY ) ;
			}

			//----------------------------------------------------------
			// 特殊割当キー

			private bool GetButtonByMappingKey( int buttonIdentity )
			{
				var keyCodes = m_MappingKeyboardToButton[ buttonIdentity ] ;

				if( keyCodes == null || keyCodes.Length == 0 )
				{
					// 無効
					return false ;
				}

				foreach( var keyCode in keyCodes )
				{
					if( Input.GetKey( m_KeyCodeMapper[ keyCode ] ) == true )
					{
						// 押されている
						return true ;
					}
				}

				return false ;
			}

			// WASD
			private void GetAxisByMappingKey_WASD( int axisIdentity, ref float oAxisX, ref float oAxisY )
			{
				if( m_MappingKeyboardToAxis_WASD == null || m_MappingKeyboardToAxis_WASD.Length == 0 )
				{
					// 無し
					return ;
				}

				// Array.Contains はメモリを食う上に遅いので使用しない
				int i ;
				for( i = 0 ; i <  m_MappingKeyboardToAxis_WASD.Length ; i ++ )
				{
					if( m_MappingKeyboardToAxis_WASD[ i ] == axisIdentity )
					{
						break ;
					}
				}
				if( i >= m_MappingKeyboardToAxis_WASD.Length )
				{
					// 無し
					return ;
				}

				//----------------------------------

				// SCX
				if( oAxisX == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.D ] ) == true )
					{
						oAxisX = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.A ] ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.W ] ) == true )
					{
						oAxisY = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.S ] ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Cursor
			private void GetAxisByMappingKey_Cursor( int axisIdentity, ref float oAxisX, ref float oAxisY )
			{
				if( m_MappingKeyboardToAxis_Cursor == null || m_MappingKeyboardToAxis_Cursor.Length == 0 )
				{
					// 無し
					return ;
				}

				// Array.Contains はメモリを食う上に遅いので使用しない
				int i ;
				for( i = 0 ; i <  m_MappingKeyboardToAxis_Cursor.Length ; i ++ )
				{
					if( m_MappingKeyboardToAxis_Cursor[ i ] == axisIdentity )
					{
						break ;
					}
				}
				if( i >= m_MappingKeyboardToAxis_Cursor.Length )
				{
					// 無し
					return ;
				}

				//----------------------------------

				// SCX
				if( oAxisX == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.RightArrow ] ) == true )
					{
						oAxisX = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.LeftArrow ] ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.UpArrow ] ) == true )
					{
						oAxisY = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.DownArrow ] ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Number
			private void GetAxisByMappingKey_Number( int axisIdentity, ref float oAxisX, ref float oAxisY )
			{
				if( m_MappingKeyboardToAxis_Number == null || m_MappingKeyboardToAxis_Number.Length == 0 )
				{
					// 無し
					return ;
				}

				// Array.Contains はメモリを食う上に遅いので使用しない
				int i ;
				for( i = 0 ; i <  m_MappingKeyboardToAxis_Number.Length ; i ++ )
				{
					if( m_MappingKeyboardToAxis_Number[ i ] == axisIdentity )
					{
						break ;
					}
				}
				if( i >= m_MappingKeyboardToAxis_Number.Length )
				{
					// 無し
					return ;
				}

				//----------------------------------

				// SCX
				if( oAxisX == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Keypad6 ] ) == true )
					{
						oAxisX = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Keypad4 ] ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Keypad8 ] ) == true )
					{
						oAxisY = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Keypad2 ] ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// RightSymbol
			private void GetAxisByMappingKey_RightSymbol( int axisIdentity, ref float oAxisX, ref float oAxisY )
			{
				if( m_MappingKeyboardToAxis_RightSymbol == null || m_MappingKeyboardToAxis_RightSymbol.Length == 0 )
				{
					// 無し
					return ;
				}

				// Array.Contains はメモリを食う上に遅いので使用しない
				int i ;
				for( i = 0 ; i <  m_MappingKeyboardToAxis_RightSymbol.Length ; i ++ )
				{
					if( m_MappingKeyboardToAxis_RightSymbol[ i ] == axisIdentity )
					{
						break ;
					}
				}
				if( i >= m_MappingKeyboardToAxis_RightSymbol.Length )
				{
					// 無し
					return ;
				}

				//----------------------------------

				// SCX
				if( oAxisX == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.RightBracket ] ) == true )
					{
						oAxisX = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Semicolon ] ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.At ] ) == true )
					{
						oAxisY = +1 ;
					}
					if( Input.GetKey( m_KeyCodeMapper[ KeyCodes.Colon ] ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Custom
			private void GetAxisByMappingKey_Custom( int axisIdentity, ref float oAxisX, ref float oAxisY )
			{
				//----------------------------------

				// SX
				if( oAxisX == 0 )
				{
					if( GetAxisDirectionByMappingKey( axisIdentity, 0 ) == true )
					{
						oAxisX = +1 ;
					}
					if( GetAxisDirectionByMappingKey( axisIdentity, 1 ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SY
				if( oAxisY == 0 )
				{
					if( GetAxisDirectionByMappingKey( axisIdentity, 2 ) == true )
					{
						oAxisY = +1 ;
					}
					if( GetAxisDirectionByMappingKey( axisIdentity, 3 ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// ゲームパッドのアクシス方向にマッピングされたキーボードのキーが押されているか判定する
			private bool GetAxisDirectionByMappingKey( int axisIdentity, int axisDirection )
			{
				var key = ( axisIdentity, axisDirection ) ;

				var keyCodes = m_MappingKeyboardToAxisDirection[ key ] ;

				if( keyCodes == null || keyCodes.Length == 0 )
				{
					// 無効
					return false ;
				}

				foreach( var keyCode in keyCodes )
				{
					if( Input.GetKey( m_KeyCodeMapper[ keyCode ] ) == true )
					{
						// 押されている
						return true ;
					}
				}

				// 押されていない
				return false ;
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
