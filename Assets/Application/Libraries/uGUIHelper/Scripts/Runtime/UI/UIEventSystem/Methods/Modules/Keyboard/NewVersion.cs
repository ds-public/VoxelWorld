#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace uGUIHelper.InputAdapter
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
				{ KeyCodes.SysReq,				Key.OEM1			    },	// 未対応
				{ KeyCodes.Break,				Key.Pause				},	// 統合
				{ KeyCodes.Menu,				Key.ContextMenu			},
			} ;

			//---------------------------------------------------------------------------------

			private static KeyCodes[] m_KeyCodes ;

			/// <summary>
			/// ボタン用状態
			/// </summary>
			public class KeyState
			{
				public bool		RepeatKeepFlag ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public bool		IsRepeat ;
				public bool		IsDown ;
				public bool		IsUp ;
			}

			private static KeyState[,] m_KeyStates ;

			private static Dictionary<KeyCodes,int> m_KeyCodeToIndexMapper ;

			//---------------------------------------------------------------------------------

			/// <summary>
			/// 初期化を行う
			/// </summary>
			public void Initialize()
			{
				// キーの全種
				m_KeyCodes = m_KeyCodeMapper.Keys.ToArray() ;

				int keyIndex ;
				int numberOfKeys = m_KeyCodes.Length ;

				// キーの順番
				m_KeyCodeToIndexMapper = new Dictionary<KeyCodes,int>() ;
				for( keyIndex  = 0 ; keyIndex <  numberOfKeys ; keyIndex ++ )
				{
					m_KeyCodeToIndexMapper.Add( m_KeyCodes[ keyIndex ], keyIndex ) ;
				}
				
				// キーの状態
				m_KeyStates = new KeyState[ numberOfKeys, 2 ] ;
				for( keyIndex  = 0 ; keyIndex <  numberOfKeys ; keyIndex ++ )
				{
					m_KeyStates[ keyIndex, 0 ] = new () ;	// Update 用
					m_KeyStates[ keyIndex, 1 ] = new () ;	// FixedUpdate 用
				}
			}

			/// <summary>
			/// フレーム毎の更新呼び出し
			/// </summary>
			public void Update( bool fromFixedUpdate )
			{
				UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current ;
				if( keyboard == null )
				{
					// キーボードデバイスが存在しない
					return ;
				}

				//-----------------------------------------------------------------------------

				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				int keyIndex ;
				int numberOfKeys = m_KeyCodes.Length ;

				KeyState state ;

				float time = Time.realtimeSinceStartup ;

				for( keyIndex  = 0 ; keyIndex <  numberOfKeys ; keyIndex ++ )
				{
					state = m_KeyStates[ keyIndex, slotNumber ] ;

					//---------------------------------

					state.IsRepeat	= false ;
					state.IsDown	= false ;
					state.IsUp		= false ;

					bool isPressed = keyboard[ m_KeyCodeMapper[ m_KeyCodes[ keyIndex ] ] ].isPressed ;
					if( isPressed == true )
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

				return keyboard[ m_KeyCodeMapper[ keyCode ] ].isPressed ;
			}

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyDown( KeyCodes keyCode, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				int keyIndex = m_KeyCodeToIndexMapper[ keyCode ] ;

				return m_KeyStates[ keyIndex, slotNumber ].IsDown ;
			}

			/// <summary>
			/// キーが離されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyUp( KeyCodes keyCode, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				int keyIndex = m_KeyCodeToIndexMapper[ keyCode ] ;

				return m_KeyStates[ keyIndex, slotNumber ].IsUp ;
			}

			/// <summary>
			/// リピート付きでキーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyRepeat( KeyCodes keyCode, bool fromFixedUpdate )
			{
				int slotNumber = ( fromFixedUpdate == false ? 0 : 1 ) ;

				// SlotNumber = 0 : Update
				// SlotNumber = 1 : FixedUpdate

				int keyIndex = m_KeyCodeToIndexMapper[ keyCode ] ;

				return m_KeyStates[ keyIndex, slotNumber ].IsRepeat ;
			}
		}
	}
}
#endif
