#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace InputHelper
{
	/// <summary>
	/// キーボード制御
	/// </summary>
	public partial class Keyboard
	{
		// 新版
		public class Implementation_NewVersion : IImplementation
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
				{ KeyCodes.SysReq,				Key.OEM1		        },	// 未対応
				{ KeyCodes.Break,				Key.Pause				},	// 統合
				{ KeyCodes.Menu,				Key.ContextMenu			},
			} ;

			//---------------------------------------------------------------------------------

			/// <summary>
			/// ボタン用状態
			/// </summary>
			public class KeyState
			{
				public bool		RepeatKeepFlag ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public bool		IsRepeat ;
			}

			//--------------

			// リピート監視対象キーと監視中の状態
			private static Dictionary<KeyCodes,KeyState>	m_KeyHashStates ;

			// リピート監視対象から外す対象の種別
			private static KeyCodes[]						m_RepeatCleaningTargets ;

			//--------------

			// リピート監視対象キーに入力が無い場合にリピート監視対象キーを解放するまでの時間
			private const float m_RepeatCleaningTime		= 1.0f ;

			//---------------------------------------------------------------------------------

			/// <summary>
			/// 初期化を行う
			/// </summary>
			public void Initialize()
			{
				// キーの状態(リピート処理専用)
				m_KeyHashStates = new () ;

				// リピート監視対象から外す対象の種別
				m_RepeatCleaningTargets = new KeyCodes[ m_KeyCodeMapper.Count ] ;
			}

			/// <summary>
			/// フレーム毎の更新呼び出し
			/// </summary>
			public void Update()
			{
				if( m_KeyHashStates.Count >  0 )
				{
					UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
					if( keyboard == null )
					{
						// キーボードデバイスが存在しない
						return ;
					}

					//-----------------------------------------------------------------------------

					float time = Time.realtimeSinceStartup ;

					// 監視の解放対象数
					int count = 0 ;

					foreach( ( var keyCode, var keyState ) in m_KeyHashStates )
					{
						//---------------------------------

						keyState.IsRepeat	= false ;

						bool isPressed = keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed ;
						if( isPressed == true )
						{
							if( keyState.RepeatKeepFlag == false )
							{
								// リピート開始

								keyState.RepeatKeepFlag = true ;
								keyState.RepeatWakeTime = time ;
								keyState.RepeatLoopTime = time ;

								keyState.IsRepeat		= true ;
							}
							else
							{
								// リピート最中
								if( ( time - keyState.RepeatWakeTime ) >= RepeatStartingTime )
								{
									// リピート中
									if( ( time - keyState.RepeatLoopTime ) >= RepeatIntervalTime )
									{
										keyState.RepeatLoopTime = time ;

										keyState.IsRepeat = true ;
									}
								}
							}
						}
						else
						{
							if( keyState.RepeatKeepFlag == true )
							{
								// リピート解除

								keyState.RepeatKeepFlag = false ;
								keyState.RepeatWakeTime = time ;
							}
							else
							{
								if( ( time - keyState.RepeatWakeTime ) >= m_RepeatCleaningTime )
								{
									// このキーは監視対象から外れる
									m_RepeatCleaningTargets[ count ] = keyCode ;
									count ++ ;
								}
							}
						}
					}

					//------------

					// 監視が不要になった対象を監視対象から除外する
					for( int index  = 0 ; index <  count ; index ++ )
					{
						m_KeyHashStates.Remove(	m_RepeatCleaningTargets[ index ] ) ;
					}
				}
			}

			// リピート監視対象キーの登録
			private bool RegisterRepeatProcessingTarget( KeyCodes keyCode )
			{
				if( m_KeyHashStates.ContainsKey( keyCode ) == false )
				{
					UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
					if( keyboard == null )
					{
						// キーボードデバイスが存在しない
						return false ;
					}

					//-----------------------------------------------------------------------------

					var keyState = new KeyState() ;
					m_KeyHashStates.Add( keyCode, keyState ) ;

					float time = Time.realtimeSinceStartup ;

					bool isPressed = keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed ;
					if( isPressed == true )
					{
						// 登録時は押されていた
						keyState.RepeatKeepFlag = true ;
						keyState.RepeatWakeTime = time ;
						keyState.RepeatLoopTime = time ;

						keyState.IsRepeat		= true ;
					}
					else
					{
						// 登録時は離されていた
						keyState.RepeatWakeTime = time ;
					}
				}

				// 既に登録済みである
				return m_KeyHashStates[ keyCode ].IsRepeat ;
			}

			// リピート監視対象キーの解放
			private void UnregisterRepeatProcessingTarget( KeyCodes keyCode )
			{
				if( m_KeyHashStates.ContainsKey( keyCode ) == true )
				{
					m_KeyHashStates.Remove(	keyCode ) ;
				}
			}

			//----------------------------------------------------------

			/// <summary>
			/// どのキーが押されているか確認する
			/// </summary>
			public void CheckAllKeys()
			{
				UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
				if( keyboard == null )
				{
					// キーボードデバイスが存在しない
					return ;
				}

				foreach( Key keyCode in Enum.GetValues( typeof( Key ) ) )
				{
					if( keyCode == Key.None || keyCode == Key.IMESelected )
					{
						// エラーになってしまうのでスキップ
						continue ;
					}

					if( keyboard[ keyCode ].wasPressedThisFrame == true )
					{
						Debug.Log( "Pressing Key : " + keyCode ) ;
					}
				}
			}

			//---------------------------------------------------------------------------------

			/// <summary>
			/// キーが押されているかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKey( KeyCodes keyCode )
			{
				UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
				if( keyboard == null )
				{
					return false ;
				}

				bool isPressed = keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed ;
				if( isPressed == true )
				{
					RegisterRepeatProcessingTarget( keyCode ) ;
				}

				return isPressed ;
			}

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyDown( KeyCodes keyCode )
			{
				UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
				if( keyboard == null )
				{
					return false ;
				}

				bool isPressed = keyboard[ m_KeyCodeMapper[ keyCode ] ].wasPressedThisFrame ;
				if( isPressed == true )
				{
					RegisterRepeatProcessingTarget( keyCode ) ;
				}

				return isPressed ;
			}

			/// <summary>
			/// キーが離されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyUp( KeyCodes keyCode )
			{
				UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
				if( keyboard == null )
				{
					return false ;
				}

				bool isReleased = keyboard[ m_KeyCodeMapper[ keyCode ] ].wasReleasedThisFrame ;
				if( isReleased == true )
				{
					UnregisterRepeatProcessingTarget( keyCode ) ;
				}

				return isReleased ;
			}

			/// <summary>
			/// リピート付きでキーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyRepeat( KeyCodes keyCode )
			{
				return RegisterRepeatProcessingTarget( keyCode ) ;
			}
		}
	}
}
#endif
