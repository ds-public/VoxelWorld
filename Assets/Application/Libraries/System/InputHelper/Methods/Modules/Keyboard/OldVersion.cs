using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	/// <summary>
	/// キーボード制御
	/// </summary>
	public partial class Keyboard
	{
		// 旧版
		public class Implementation_OldVersion : IImplementation
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
				{ KeyCodes.LeftBracket,		KeyCode.LeftBracket		},
				{ KeyCodes.Backslash,		KeyCode.Backslash		},
				{ KeyCodes.RightBracket,	KeyCode.RightBracket	},
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

			// リピート監視対象キーに入力が無い場合にリピート監視対象キーを解放するまでの時間
			private const float m_RepeatCleaningTime		= 1.0f ;

			//----------------------------------

			// リピート監視対象から外す対象の種別
			private static KeyCodes[]						m_RepeatCleaningTargets ;

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
					float time = Time.realtimeSinceStartup ;

					// 監視の解放対象数
					int count = 0 ;

					foreach( ( var keyCode, var keyState ) in m_KeyHashStates )
					{
						//---------------------------------

						keyState.IsRepeat	= false ;

						bool isPressed = Input.GetKey( m_KeyCodeMapper[ keyCode ] ) ;
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
					var keyState = new KeyState() ;
					m_KeyHashStates.Add( keyCode, keyState ) ;

					float time = Time.realtimeSinceStartup ;

					bool isPressed = Input.GetKey( m_KeyCodeMapper[ keyCode ] ) ;
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

			//----------------------------------------------------------

			/// <summary>
			/// どのキーが押されているか確認する
			/// </summary>
			public void CheckAllKeys()
			{
				foreach( KeyCode keyCode in Enum.GetValues( typeof( KeyCode ) ) )
				{
					if( Input.GetKey( keyCode ) == true )
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
				return Input.GetKey( m_KeyCodeMapper[ keyCode ] ) ;
			}

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyDown( KeyCodes keyCode )
			{
				return Input.GetKeyDown( m_KeyCodeMapper[ keyCode ] ) ;
			}

			/// <summary>
			/// キーが離されたどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			public bool GetKeyUp( KeyCodes keyCode )
			{
				return Input.GetKeyUp( m_KeyCodeMapper[ keyCode ] ) ;
			}

			/// <summary>
			/// リピート付きでキーが押されているかどうかの判定
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
