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
			private static readonly Dictionary<KeyCodes, Key> m_KeyCodeMapper = new ()
			{
				{ KeyCodes.Backspace,		    Key.Backspace			},
				{ KeyCodes.Delete,			    Key.Delete				},
				{ KeyCodes.Tab,				    Key.Tab					},
				{ KeyCodes.Clear,			    Key.OEM1				},	// 未対応
				{ KeyCodes.Return,			    Key.Enter				},
				{ KeyCodes.Pause,			    Key.Pause				},
				{ KeyCodes.Escape,			    Key.Escape				},
				{ KeyCodes.Space,			    Key.Space				},

				{ KeyCodes.Keypad0,			    Key.Numpad0				},
				{ KeyCodes.Keypad1,			    Key.Numpad1				},
				{ KeyCodes.Keypad2,			    Key.Numpad2				},
				{ KeyCodes.Keypad3,			    Key.Numpad3				},
				{ KeyCodes.Keypad4,			    Key.Numpad4				},
				{ KeyCodes.Keypad5,			    Key.Numpad5				},
				{ KeyCodes.Keypad6,			    Key.Numpad6				},
				{ KeyCodes.Keypad7,			    Key.Numpad7				},
				{ KeyCodes.Keypad8,			    Key.Numpad8				},
				{ KeyCodes.Keypad9,			    Key.Numpad9				},

				{ KeyCodes.KeypadPeriod,	    Key.NumpadPeriod		},
				{ KeyCodes.KeypadDivide,	    Key.NumpadDivide		},
				{ KeyCodes.KeypadMultiply,	    Key.NumpadMultiply		},
				{ KeyCodes.KeypadMinus,		    Key.NumpadMinus			},
				{ KeyCodes.KeypadPlus,		    Key.NumpadPlus			},
				{ KeyCodes.KeypadEnter,		    Key.NumpadEnter			},
				{ KeyCodes.KeypadEquals,	    Key.NumpadEquals		},

				{ KeyCodes.UpArrow,			    Key.UpArrow				},
				{ KeyCodes.DownArrow,		    Key.DownArrow			},
				{ KeyCodes.RightArrow,		    Key.RightArrow			},
				{ KeyCodes.LeftArrow,		    Key.LeftArrow			},

				{ KeyCodes.Insert,			    Key.Insert				},
				{ KeyCodes.Home,			    Key.Home				},
				{ KeyCodes.End,				    Key.End					},
				{ KeyCodes.PageUp,			    Key.PageUp				},
				{ KeyCodes.PageDown,		    Key.PageDown			},

				{ KeyCodes.F1,				    Key.F1		            },
				{ KeyCodes.F2,				    Key.F2		            },
				{ KeyCodes.F3,				    Key.F3		            },
				{ KeyCodes.F4,				    Key.F4		            },
				{ KeyCodes.F5,				    Key.F5		            },
				{ KeyCodes.F6,				    Key.F6		            },
				{ KeyCodes.F7,				    Key.F7		            },
				{ KeyCodes.F8,				    Key.F8		            },
				{ KeyCodes.F9,				    Key.F9		            },
				{ KeyCodes.F10,				    Key.F10		            },
				{ KeyCodes.F11,				    Key.F11		            },
				{ KeyCodes.F12,				    Key.F12		            },
				{ KeyCodes.F13,				    Key.OEM1		        },	// 未対応
				{ KeyCodes.F14,				    Key.OEM1		        },	// 未対応
				{ KeyCodes.F15,				    Key.OEM1		        },	// 未対応

				{ KeyCodes.Alpha0,			    Key.Digit0	            },
				{ KeyCodes.Alpha1,			    Key.Digit1	            },
				{ KeyCodes.Alpha2,			    Key.Digit2	            },
				{ KeyCodes.Alpha3,			    Key.Digit3	            },
				{ KeyCodes.Alpha4,			    Key.Digit4	            },
				{ KeyCodes.Alpha5,			    Key.Digit5	            },
				{ KeyCodes.Alpha6,			    Key.Digit6	            },
				{ KeyCodes.Alpha7,			    Key.Digit7	            },
				{ KeyCodes.Alpha8,			    Key.Digit8	            },
				{ KeyCodes.Alpha9,			    Key.Digit9	            },

				{ KeyCodes.Exclaim,			    Key.Digit1			    },	// 統合
				{ KeyCodes.DoubleQuote,		    Key.Digit2			    },	// 統合
				{ KeyCodes.Hash,			    Key.Digit3			    },	// 統合
				{ KeyCodes.Dollar,			    Key.Digit4			    },	// 統合
				{ KeyCodes.Percent,			    Key.Digit5			    },	// 統合
				{ KeyCodes.Ampersand,		    Key.Digit6			    },	// 統合
				{ KeyCodes.Quote,			    Key.Digit7			    },	// 統合
				{ KeyCodes.LeftParen,		    Key.Digit8			    },	// 統合
				{ KeyCodes.RightParen,		    Key.Digit9			    },	// 統合
				{ KeyCodes.Asterisk,		    Key.Quote		        },	// 統合
				{ KeyCodes.Plus,			    Key.Semicolon		    },	// 統合
				{ KeyCodes.Comma,			    Key.Comma			    },
				{ KeyCodes.Minus,			    Key.Minus			    },
				{ KeyCodes.Period,			    Key.Period			    },
				{ KeyCodes.Slash,			    Key.Slash			    },
				{ KeyCodes.Colon,			    Key.Quote	            },	// 日本語キーボードは対応が異なる
				{ KeyCodes.Semicolon,		    Key.Semicolon		    },
				{ KeyCodes.Less,			    Key.Comma			    },	// 統合
				{ KeyCodes.Equals,			    Key.Minus			    },  // 統合
				{ KeyCodes.Greater,			    Key.Period			    },	// 統合
				{ KeyCodes.Question,		    Key.Slash			    },	// 統合
				{ KeyCodes.At,				    Key.LeftBracket		    },  // 日本語キーボードは対応が異なる
				{ KeyCodes.LeftBracket,		    Key.RightBracket        },  // 日本語キーボードは対応が異なる
				{ KeyCodes.Backslash,		    Key.OEM2			    },	// 英語(BackSlash) 日本語(OEM2)
				{ KeyCodes.RightBracket,	    Key.Backslash           },  // 日本語キーボードは対応が異なる
				{ KeyCodes.Caret,			    Key.Equals		        },	// 日本語キーボードは対応が異なる
				{ KeyCodes.Underscore,		    Key.OEM2			    },	// 統合 英語(BackSlash) 日本語(OEM2)
				{ KeyCodes.BackQuote,		    Key.LeftBracket		    },	// 未対応

				{ KeyCodes.A,				    Key.A		            },
				{ KeyCodes.B,				    Key.B		            },
				{ KeyCodes.C,				    Key.C		            },
				{ KeyCodes.D,				    Key.D		            },
				{ KeyCodes.E,				    Key.E		            },
				{ KeyCodes.F,				    Key.F		            },
				{ KeyCodes.G,				    Key.G		            },
				{ KeyCodes.H,				    Key.H		            },
				{ KeyCodes.I,				    Key.I		            },
				{ KeyCodes.J,				    Key.J		            },
				{ KeyCodes.K,				    Key.K		            },
				{ KeyCodes.L,				    Key.L		            },
				{ KeyCodes.M,				    Key.M		            },
				{ KeyCodes.N,				    Key.N		            },
				{ KeyCodes.O,				    Key.O		            },
				{ KeyCodes.P,				    Key.P		            },
				{ KeyCodes.Q,				    Key.Q		            },
				{ KeyCodes.R,				    Key.R		            },
				{ KeyCodes.S,				    Key.S		            },
				{ KeyCodes.T,				    Key.T		            },
				{ KeyCodes.U,				    Key.U		            },
				{ KeyCodes.V,				    Key.V		            },
				{ KeyCodes.W,				    Key.W		            },
				{ KeyCodes.X,				    Key.X		            },
				{ KeyCodes.Y,				    Key.Y		            },
				{ KeyCodes.Z,				    Key.Z		            },

				{ KeyCodes.LeftCurlyBracket,	Key.RightBracket		},	// 統合
				{ KeyCodes.Pipe,				Key.OEM1			    },	// 未対応
				{ KeyCodes.RightCurlyBracket,	Key.Backslash		    },	// 統合
				{ KeyCodes.Tilde,				Key.Equals			    },	// 統合
				{ KeyCodes.Numlock,				Key.NumLock				},
				{ KeyCodes.CapsLock,			Key.CapsLock			},
				{ KeyCodes.ScrollLock,			Key.ScrollLock			},
				{ KeyCodes.RightShift,			Key.RightShift			},
				{ KeyCodes.LeftShift,			Key.LeftShift			},
				{ KeyCodes.RightControl,		Key.RightCtrl			},
				{ KeyCodes.LeftControl,			Key.LeftCtrl			},
				{ KeyCodes.RightAlt,			Key.RightAlt			},
				{ KeyCodes.LeftAlt,				Key.LeftAlt				},
				{ KeyCodes.LeftMeta,			Key.LeftMeta			},
				{ KeyCodes.LeftCommand,			Key.LeftCommand			},
				{ KeyCodes.LeftApple,			Key.LeftApple			},
				{ KeyCodes.LeftWindows,			Key.LeftWindows			},
				{ KeyCodes.RightMeta,			Key.RightMeta			},
				{ KeyCodes.RightCommand,		Key.RightCommand		},
				{ KeyCodes.RightApple,			Key.RightApple			},
				{ KeyCodes.RightWindows,		Key.RightWindows		},
				{ KeyCodes.AltGr,				Key.AltGr				},
				{ KeyCodes.Help,				Key.OEM1				},	// 未対応
				{ KeyCodes.Print,				Key.PrintScreen			},
				{ KeyCodes.SysReq,				Key.OEM1			    },	// 未対応
				{ KeyCodes.Break,				Key.Pause				},	// 統合
				{ KeyCodes.Menu,				Key.ContextMenu			},
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

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true && Enabled == true )
					{
						UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
						if( keyboard != null )
						{
							// キーボードのキーのボタンへの割り当て(ループでは回さない)
							if( GetButtonByMappingKey( GamePad.B1, keyboard ) == true ){ buttonFlags |= B1 ; }
							if( GetButtonByMappingKey( GamePad.B2, keyboard ) == true ){ buttonFlags |= B2 ; }
							if( GetButtonByMappingKey( GamePad.B3, keyboard ) == true ){ buttonFlags |= B3 ; }
							if( GetButtonByMappingKey( GamePad.B4, keyboard ) == true ){ buttonFlags |= B4 ; }

							if( GetButtonByMappingKey( GamePad.R1, keyboard ) == true ){ buttonFlags |= R1 ; }
							if( GetButtonByMappingKey( GamePad.L1, keyboard ) == true ){ buttonFlags |= L1 ; }
							if( GetButtonByMappingKey( GamePad.R2, keyboard ) == true ){ buttonFlags |= R2 ; }
							if( GetButtonByMappingKey( GamePad.L2, keyboard ) == true ){ buttonFlags |= L2 ; }
							if( GetButtonByMappingKey( GamePad.R3, keyboard ) == true ){ buttonFlags |= R3 ; }
							if( GetButtonByMappingKey( GamePad.L3, keyboard ) == true ){ buttonFlags |= L3 ; }

							if( GetButtonByMappingKey( GamePad.O1, keyboard ) == true ){ buttonFlags |= O1 ; }
							if( GetButtonByMappingKey( GamePad.O2, keyboard ) == true ){ buttonFlags |= O2 ; }
							if( GetButtonByMappingKey( GamePad.O3, keyboard ) == true ){ buttonFlags |= O3 ; }
							if( GetButtonByMappingKey( GamePad.O4, keyboard ) == true ){ buttonFlags |= O4 ; }
						}
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

				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToButtonEnabled == true & Enabled == true )
					{
						// キーボードのキーのボタンへの割り当て(ループでは回さない)
						UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
						if( keyboard != null )
						{
							if( GetButtonByMappingKey( buttonIdentity, keyboard ) == true )
							{
								return true ;
							}
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
			
				if( playerNumber <= 0 )
				{
					// 全プレイヤー指定またはプレイヤーが１人の場合のみキーボードの追加判定が加わる
					if( MappingKeyboardToAxisEnabled == true && Enabled == true )
					{
						// キーボードのキーのアクシスへの割り当て

						UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
						if( keyboard != null )
						{
							// ＷＡＳＤ
							GetAxisByMappingKey_WASD( axisIdentity, ref oAxisX, ref oAxisY, keyboard ) ;

							// カーソル
							GetAxisByMappingKey_Cursor( axisIdentity, ref oAxisX, ref oAxisY, keyboard ) ;

							// ナンバー
							GetAxisByMappingKey_Number( axisIdentity, ref oAxisX, ref oAxisY, keyboard ) ;

							// 右側記号
							GetAxisByMappingKey_RightSymbol( axisIdentity, ref oAxisX, ref oAxisY, keyboard ) ;

							// カスタム
							GetAxisByMappingKey_Custom( axisIdentity, ref oAxisX, ref oAxisY, keyboard ) ;
						}
					}
				}

				//------------------------------------------------------------------------------------------
				// 以下は実際のゲームパッドの処理

				if( max == 0 )
				{
					// ゲームパッドの接続数が０なら処理はここで終了

					// 縦軸の符号反転
					if( m_Owner.Invert == true && axisIdentity != TB )
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
						case DP :
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

						case LS :
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

						case RS :
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

						case TB :
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
				if( m_Owner.Invert == true && axisIdentity != TB )
				{
					oAxisY = - oAxisY ;
				}

				return new Vector2( oAxisX, oAxisY ) ;
			}

			//----------------------------------------------------------
			// 特殊割当キー

			private bool GetButtonByMappingKey( int buttonIdentity, UnityEngine.InputSystem.Keyboard keyboard )
			{
				var keyCodes = m_MappingKeyboardToButton[ buttonIdentity ] ;

				if( keyCodes == null || keyCodes.Length == 0 )
				{
					// 無効
					return false ;
				}

				foreach( var keyCode in keyCodes )
				{
					if( keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed == true )
					{
						// 押されている
						return true ;
					}
				}

				return false ;
			}

			// WASD
			private void GetAxisByMappingKey_WASD( int axisIdentity, ref float oAxisX, ref float oAxisY, UnityEngine.InputSystem.Keyboard keyboard )
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
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.D ] ].isPressed == true )
					{
						oAxisX = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.A ] ].isPressed == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.W ] ].isPressed == true )
					{
						oAxisY = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.S ] ].isPressed == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Cursor
			private void GetAxisByMappingKey_Cursor( int axisIdentity, ref float oAxisX, ref float oAxisY, UnityEngine.InputSystem.Keyboard keyboard )
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
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.RightArrow ] ].isPressed == true )
					{
						oAxisX = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.LeftArrow ] ].isPressed == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.UpArrow ] ].isPressed == true )
					{
						oAxisY = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.DownArrow ] ].isPressed == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Number
			private void GetAxisByMappingKey_Number( int axisIdentity, ref float oAxisX, ref float oAxisY, UnityEngine.InputSystem.Keyboard keyboard )
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
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Keypad6 ] ].isPressed == true )
					{
						oAxisX = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Keypad4 ] ].isPressed == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Keypad8 ] ].isPressed == true )
					{
						oAxisY = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Keypad2 ] ].isPressed == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// RightSymbol
			private void GetAxisByMappingKey_RightSymbol( int axisIdentity, ref float oAxisX, ref float oAxisY, UnityEngine.InputSystem.Keyboard keyboard )
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
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.RightBracket ] ].isPressed == true )
					{
						oAxisX = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Semicolon ] ].isPressed == true )
					{
						oAxisX = -1 ;
					}
				}

				// SCY
				if( oAxisY == 0 )
				{
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.At ] ].isPressed == true )
					{
						oAxisY = +1 ;
					}
					if( keyboard[ m_KeyCodeMapper[ KeyCodes.Colon ] ].isPressed == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// Custom
			private void GetAxisByMappingKey_Custom( int axisIdentity, ref float oAxisX, ref float oAxisY, UnityEngine.InputSystem.Keyboard keyboard )
			{
				// SX
				if( oAxisX == 0 )
				{
					if( GetAxisDirectionByMappingKey( axisIdentity, 0, keyboard ) == true )
					{
						oAxisX = +1 ;
					}
					if( GetAxisDirectionByMappingKey( axisIdentity, 1, keyboard ) == true )
					{
						oAxisX = -1 ;
					}
				}

				// SY
				if( oAxisY == 0 )
				{
					if( GetAxisDirectionByMappingKey( axisIdentity, 2, keyboard ) == true )
					{
						oAxisY = +1 ;
					}
					if( GetAxisDirectionByMappingKey( axisIdentity, 3, keyboard ) == true )
					{
						oAxisY = -1 ;
					}
				}
			}

			// ゲームパッドのアクシス方向にマッピングされたキーボードのキーが押されているか判定する
			private bool GetAxisDirectionByMappingKey( int axisIdentity, int axisDirection, UnityEngine.InputSystem.Keyboard keyboard )
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
					if( keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed == true )
					{
						// 押されている
						return true ;
					}
				}

				// 押されていない
				return false ;
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
