#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace InputHelper
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
			// 互換メソッド

			/// <summary>
			/// 接続中のゲームパッドの数
			/// </summary>
			public int NumberOfGamePads
			{
				get
				{
					return Gamepad.all.Count ;
				}
			}

			/// <summary>
			/// 接続中のゲームパッドの名前を取得する
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public string[] GetJoystickNames()
			{
				var availableNames = new List<string>() ;

				if( Gamepad.all.Count >  0 )
				{
					foreach( var gamepad in Gamepad.all )
					{
						availableNames.Add( gamepad.name ) ;
					}
				}

				return availableNames.ToArray() ;
			}

			//--------------

			/// <summary>
			/// ゲームパッド用のボタン定義
			/// </summary>
			public class GamePadButtonDefinition
			{
				public int	PlayerNumber ;
				public int	ButtonNumber ;

				public GamePadButtonDefinition( int playerNumber, int buttonNumber )
				{
					PlayerNumber = playerNumber ;
					ButtonNumber = buttonNumber ;
				}
			}

			/// <summary>
			/// ボタン定義
			/// </summary>
			public class ButtonDefinition
			{
				public KeyCodes[]					KeyCodes ;
				public int[]						MouseButtonNumbers ;
				public GamePadButtonDefinition[]	GamePadButtonDefinitions ;
			}

			// ボタン定義群
			private static readonly Dictionary<string,ButtonDefinition> m_ButtonDefinitions = new ()
			{
				{ "Fire1",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.LeftControl },
						MouseButtonNumbers = new int[]{ 0 },
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  0 ) }
					}
				},
				{ "Fire2",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.LeftAlt },
						MouseButtonNumbers = new int[]{ 1 },
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  1 ) }
					}
				},
				{ "Fire3",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.LeftShift },
						MouseButtonNumbers = new int[]{ 2 },
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  2 ) }
					}
				},
				{ "Jump",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.Space },
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  3 ) }
					}
				},
				{ "Submit",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.Return },
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  0 ) }
					}
				},
				{ "Cancel",
					new ButtonDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.Escape },
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition( -1,  1 ) }
					}
				},
				{ GamePad.Player1Button00,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  0 ) }
					}
				},
				{ GamePad.Player1Button01,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  1 ) }
					}
				},
				{ GamePad.Player1Button02,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  2 ) }
					}
				},
				{ GamePad.Player1Button03,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  3 ) }
					}
				},
				{ GamePad.Player1Button04,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  4 ) }
					}
				},
				{ GamePad.Player1Button05,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  5 ) }
					}
				},
				{ GamePad.Player1Button06,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  6 ) }
					}
				},
				{ GamePad.Player1Button07,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  7 ) }
					}
				},
				{ GamePad.Player1Button08,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  8 ) }
					}
				},
				{ GamePad.Player1Button09,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0,  9 ) }
					}
				},
				{ GamePad.Player1Button10,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 10 ) }
					}
				},
				{ GamePad.Player1Button11,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 11 ) }
					}
				},
				{ GamePad.Player1Button12,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 12 ) }
					}
				},
				{ GamePad.Player1Button13,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 13 ) }
					}
				},
				{ GamePad.Player1Button14,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 14 ) }
					}
				},
				{ GamePad.Player1Button15,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  0, 15 ) }
					}
				},

				{ GamePad.Player2Button00,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  0 ) }
					}
				},
				{ GamePad.Player2Button01,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  1 ) }
					}
				},
				{ GamePad.Player2Button02,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  2 ) }
					}
				},
				{ GamePad.Player2Button03,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  3 ) }
					}
				},
				{ GamePad.Player2Button04,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  4 ) }
					}
				},
				{ GamePad.Player2Button05,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  5 ) }
					}
				},
				{ GamePad.Player2Button06,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  6 ) }
					}
				},
				{ GamePad.Player2Button07,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  7 ) }
					}
				},
				{ GamePad.Player2Button08,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  8 ) }
					}
				},
				{ GamePad.Player2Button09,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1,  9 ) }
					}
				},
				{ GamePad.Player2Button10,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 10 ) }
					}
				},
				{ GamePad.Player2Button11,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 11 ) }
					}
				},
				{ GamePad.Player2Button12,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 12 ) }
					}
				},
				{ GamePad.Player2Button13,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 13 ) }
					}
				},
				{ GamePad.Player2Button14,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 14 ) }
					}
				},
				{ GamePad.Player2Button15,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  1, 15 ) }
					}
				},
				{ GamePad.Player3Button00,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  0 ) }
					}
				},
				{ GamePad.Player3Button01,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  1 ) }
					}
				},
				{ GamePad.Player3Button02,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  2 ) }
					}
				},
				{ GamePad.Player3Button03,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  3 ) }
					}
				},
				{ GamePad.Player3Button04,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  4 ) }
					}
				},
				{ GamePad.Player3Button05,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  5 ) }
					}
				},
				{ GamePad.Player3Button06,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  6 ) }
					}
				},
				{ GamePad.Player3Button07,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  7 ) }
					}
				},
				{ GamePad.Player3Button08,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  8 ) }
					}
				},
				{ GamePad.Player3Button09,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2,  9 ) }
					}
				},
				{ GamePad.Player3Button10,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 10 ) }
					}
				},
				{ GamePad.Player3Button11,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 11 ) }
					}
				},
				{ GamePad.Player3Button12,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 12 ) }
					}
				},
				{ GamePad.Player3Button13,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 13 ) }
					}
				},
				{ GamePad.Player3Button14,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 14 ) }
					}
				},
				{ GamePad.Player3Button15,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  2, 15 ) }
					}
				},
				{ GamePad.Player4Button00,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  0 ) }
					}
				},
				{ GamePad.Player4Button01,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  1 ) }
					}
				},
				{ GamePad.Player4Button02,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  2 ) }
					}
				},
				{ GamePad.Player4Button03,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  3 ) }
					}
				},
				{ GamePad.Player4Button04,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  4 ) }
					}
				},
				{ GamePad.Player4Button05,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  5 ) }
					}
				},
				{ GamePad.Player4Button06,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  6 ) }
					}
				},
				{ GamePad.Player4Button07,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  7 ) }
					}
				},
				{ GamePad.Player4Button08,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  8 ) }
					}
				},
				{ GamePad.Player4Button09,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3,  9 ) }
					}
				},
				{ GamePad.Player4Button10,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 10 ) }
					}
				},
				{ GamePad.Player4Button11,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 11 ) }
					}
				},
				{ GamePad.Player4Button12,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 12 ) }
					}
				},
				{ GamePad.Player4Button13,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 13 ) }
					}
				},
				{ GamePad.Player4Button14,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 14 ) }
					}
				},
				{ GamePad.Player4Button15,
					new ButtonDefinition()
					{
						KeyCodes = null,
						MouseButtonNumbers = null,
						GamePadButtonDefinitions = new GamePadButtonDefinition[]{ new GamePadButtonDefinition(  3, 15 ) }
					}
				},
			} ;

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( string buttonName )
			{
				if( m_ButtonDefinitions.ContainsKey( buttonName ) == false )
				{
					// 定義情報が見つからない
					return false ;
				}

				var buttonDefinition = m_ButtonDefinitions[ buttonName ] ;

				//---------------------------------

				bool state = false ;

				// キーボード
				if( state == false && buttonDefinition.KeyCodes != null && buttonDefinition.KeyCodes.Length >  0 )
				{
					foreach( var keyCode in buttonDefinition.KeyCodes )
					{
						if( Keyboard.GetKey( keyCode ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// マウス
				if( state == false && buttonDefinition.MouseButtonNumbers != null && buttonDefinition.MouseButtonNumbers.Length >  0 )
				{
					foreach( var mouseButtonNumber in buttonDefinition.MouseButtonNumbers )
					{
						if( Mouse.GetButton( mouseButtonNumber ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// ゲームパッド
				if( state == false && buttonDefinition.GamePadButtonDefinitions != null && buttonDefinition.GamePadButtonDefinitions.Length >  0 )
				{
					foreach( var gamePadButtonDefinition in buttonDefinition.GamePadButtonDefinitions )
					{
						var playerNumber = gamePadButtonDefinition.PlayerNumber ;
						var buttonNumber = gamePadButtonDefinition.ButtonNumber ;

						if( playerNumber <  0 )
						{
							// 全体対象
							for( playerNumber  = 0 ; playerNumber <  Gamepad.all.Count ; playerNumber ++ )
							{
								state = IsGamePadButtonPressed( playerNumber, buttonNumber ) ;
								if( state == true )
								{
									break ;
								}
							}
						}
						else
						{
							// 個別対象
							if( playerNumber <  Gamepad.all.Count )
							{
								state = IsGamePadButtonPressed( playerNumber, buttonNumber ) ;
							}
						}
					}

					static bool IsGamePadButtonPressed( int playerNumber, int buttonNumber )
					{
						bool state = false ;

						var gamepad = Gamepad.all[ playerNumber ] ;
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
				}

				return state ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			public bool GetButtonDown( string buttonName )
			{
				if( m_ButtonDefinitions.ContainsKey( buttonName ) == false )
				{
					// 定義情報が見つからない
					return false ;
				}

				var buttonDefinition = m_ButtonDefinitions[ buttonName ] ;

				//---------------------------------

				bool state = false ;

				// キーボード
				if( state == false && buttonDefinition.KeyCodes != null && buttonDefinition.KeyCodes.Length >  0 )
				{
					foreach( var keyCode in buttonDefinition.KeyCodes )
					{
						if( Keyboard.GetKeyDown( keyCode ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// マウス
				if( state == false && buttonDefinition.MouseButtonNumbers != null && buttonDefinition.MouseButtonNumbers.Length >  0 )
				{
					foreach( var mouseButtonNumber in buttonDefinition.MouseButtonNumbers )
					{
						if( Mouse.GetButtonDown( mouseButtonNumber ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// ゲームパッド
				if( state == false && buttonDefinition.GamePadButtonDefinitions != null && buttonDefinition.GamePadButtonDefinitions.Length >  0 )
				{
					foreach( var gamePadButtonDefinition in buttonDefinition.GamePadButtonDefinitions )
					{
						var playerNumber = gamePadButtonDefinition.PlayerNumber ;
						var buttonNumber = gamePadButtonDefinition.ButtonNumber ;

						if( playerNumber <  0 )
						{
							// 全体対象
							for( playerNumber  = 0 ; playerNumber <  Gamepad.all.Count ; playerNumber ++ )
							{
								state = IsGamePadButtonWasPressedThisFrame( playerNumber, buttonNumber ) ;
								if( state == true )
								{
									break ;
								}
							}
						}
						else
						{
							// 個別対象
							if( playerNumber <  Gamepad.all.Count )
							{
								state = IsGamePadButtonWasPressedThisFrame( playerNumber, buttonNumber ) ;
							}
						}
					}

					static bool IsGamePadButtonWasPressedThisFrame( int playerNumber, int buttonNumber )
					{
						bool state = false ;

						var gamepad = Gamepad.all[ playerNumber ] ;
						switch( buttonNumber )
						{
							case  0 : state = gamepad.buttonSouth.wasPressedThisFrame		; break ;
							case  1 : state = gamepad.buttonEast.wasPressedThisFrame		; break ;
							case  2 : state = gamepad.buttonWest.wasPressedThisFrame		; break ;
							case  3 : state = gamepad.buttonNorth.wasPressedThisFrame		; break ;
							case  4 : state = gamepad.rightShoulder.wasPressedThisFrame		; break ;
							case  5 : state = gamepad.leftShoulder.wasPressedThisFrame		; break ;
							case  6 : state = gamepad.rightTrigger.wasPressedThisFrame		; break ;
							case  7 : state = gamepad.leftTrigger.wasPressedThisFrame		; break ;
							case  8 : state = gamepad.rightStickButton.wasPressedThisFrame	; break ;
							case  9 : state = gamepad.leftStickButton.wasPressedThisFrame	; break ;
							case 10 : state = gamepad.startButton.wasPressedThisFrame		; break ;
							case 11 : state = gamepad.selectButton.wasPressedThisFrame		; break ;
						}

						return state ;
					}
				}

				return state ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			public bool GetButtonUp( string buttonName )
			{
				if( m_ButtonDefinitions.ContainsKey( buttonName ) == false )
				{
					// 定義情報が見つからない
					return false ;
				}

				var buttonDefinition = m_ButtonDefinitions[ buttonName ] ;

				//---------------------------------

				bool state = false ;

				// キーボード
				if( state == false && buttonDefinition.KeyCodes != null && buttonDefinition.KeyCodes.Length >  0 )
				{
					foreach( var keyCode in buttonDefinition.KeyCodes )
					{
						if( Keyboard.GetKeyUp( keyCode ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// マウス
				if( state == false && buttonDefinition.MouseButtonNumbers != null && buttonDefinition.MouseButtonNumbers.Length >  0 )
				{
					foreach( var mouseButtonNumber in buttonDefinition.MouseButtonNumbers )
					{
						if( Mouse.GetButtonUp( mouseButtonNumber ) == true )
						{
							state = true ;
							break ;
						}
					}
				}

				// ゲームパッド
				if( state == false && buttonDefinition.GamePadButtonDefinitions != null && buttonDefinition.GamePadButtonDefinitions.Length >  0 )
				{
					foreach( var gamePadButtonDefinition in buttonDefinition.GamePadButtonDefinitions )
					{
						var playerNumber = gamePadButtonDefinition.PlayerNumber ;
						var buttonNumber = gamePadButtonDefinition.ButtonNumber ;

						if( playerNumber <  0 )
						{
							// 全体対象
							for( playerNumber  = 0 ; playerNumber <  Gamepad.all.Count ; playerNumber ++ )
							{
								state = IsGamePadButtonWasReleasedThisFrame( playerNumber, buttonNumber ) ;
								if( state == true )
								{
									break ;
								}
							}
						}
						else
						{
							// 個別対象
							if( playerNumber <  Gamepad.all.Count )
							{
								state = IsGamePadButtonWasReleasedThisFrame( playerNumber, buttonNumber ) ;
							}
						}
					}

					static bool IsGamePadButtonWasReleasedThisFrame( int playerNumber, int buttonNumber )
					{
						bool state = false ;

						var gamepad = Gamepad.all[ playerNumber ] ;
						switch( buttonNumber )
						{
							case  0 : state = gamepad.buttonSouth.wasReleasedThisFrame		; break ;
							case  1 : state = gamepad.buttonEast.wasReleasedThisFrame		; break ;
							case  2 : state = gamepad.buttonWest.wasReleasedThisFrame		; break ;
							case  3 : state = gamepad.buttonNorth.wasReleasedThisFrame		; break ;
							case  4 : state = gamepad.rightShoulder.wasReleasedThisFrame	; break ;
							case  5 : state = gamepad.leftShoulder.wasReleasedThisFrame		; break ;
							case  6 : state = gamepad.rightTrigger.wasReleasedThisFrame		; break ;
							case  7 : state = gamepad.leftTrigger.wasReleasedThisFrame		; break ;
							case  8 : state = gamepad.rightStickButton.wasReleasedThisFrame	; break ;
							case  9 : state = gamepad.leftStickButton.wasReleasedThisFrame	; break ;
							case 10 : state = gamepad.startButton.wasReleasedThisFrame		; break ;
							case 11 : state = gamepad.selectButton.wasReleasedThisFrame		; break ;
						}

						return state ;
					}
				}

				return state ;
			}

			//----

			/// <summary>
			/// ゲームパッド用のアクシス定義
			/// </summary>
			public class GamePadAxisDefinition
			{
				public int	PlayerNumber ;
				public int	AxisNumber ;

				public GamePadAxisDefinition( int playerNumber, int axisNumber )
				{
					PlayerNumber	= playerNumber ;
					AxisNumber		= axisNumber ;
				}
			}

			/// <summary>
			/// アクシス定義
			/// </summary>
			public class AxisDefinition
			{
				public KeyCodes[]					KeyCodes ;
				public bool							MouseWheel ;
				public GamePadAxisDefinition[]		GamePadAxisDefinitions ;
			}

			// アクシス定義群
			private static readonly Dictionary<string,AxisDefinition> m_AxisDefinitions = new ()
			{
				{ "Horizontal",
					new AxisDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.LeftArrow, KeyCodes.RightArrow, KeyCodes.A, KeyCodes.D },
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition( -1,  2 ) }
					}
				},
				{ "Vertical",
					new AxisDefinition()
					{
						KeyCodes = new KeyCodes[]{ KeyCodes.DownArrow, KeyCodes.UpArrow, KeyCodes.S, KeyCodes.W },
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition( -1,  3 ) }
					}
				},
				{ "Mouse ScrollWheel",
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = true,
						GamePadAxisDefinitions = null
					}
				},
				{ GamePad.Player1Axis00,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  0 ) }
					}
				},
				{ GamePad.Player1Axis01,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  1 ) }
					}
				},
				{ GamePad.Player1Axis02,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  2 ) }
					}
				},
				{ GamePad.Player1Axis03,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  3 ) }
					}
				},
				{ GamePad.Player1Axis04,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  4 ) }
					}
				},
				{ GamePad.Player1Axis05,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  5 ) }
					}
				},
				{ GamePad.Player1Axis06,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  6 ) }
					}
				},
				{ GamePad.Player1Axis07,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  0,  7 ) }
					}
				},

				{ GamePad.Player2Axis00,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  0 ) }
					}
				},
				{ GamePad.Player2Axis01,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  1 ) }
					}
				},
				{ GamePad.Player2Axis02,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  2 ) }
					}
				},
				{ GamePad.Player2Axis03,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  3 ) }
					}
				},
				{ GamePad.Player2Axis04,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  4 ) }
					}
				},
				{ GamePad.Player2Axis05,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  5 ) }
					}
				},
				{ GamePad.Player2Axis06,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  6 ) }
					}
				},
				{ GamePad.Player2Axis07,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  1,  7 ) }
					}
				},

				{ GamePad.Player3Axis00,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  0 ) }
					}
				},
				{ GamePad.Player3Axis01,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  1 ) }
					}
				},
				{ GamePad.Player3Axis02,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  2 ) }
					}
				},
				{ GamePad.Player3Axis03,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  3 ) }
					}
				},
				{ GamePad.Player3Axis04,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  4 ) }
					}
				},
				{ GamePad.Player3Axis05,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  5 ) }
					}
				},
				{ GamePad.Player3Axis06,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  6 ) }
					}
				},
				{ GamePad.Player3Axis07,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  2,  7 ) }
					}
				},

				{ GamePad.Player4Axis00,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  0 ) }
					}
				},
				{ GamePad.Player4Axis01,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  1 ) }
					}
				},
				{ GamePad.Player4Axis02,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  2 ) }
					}
				},
				{ GamePad.Player4Axis03,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  3 ) }
					}
				},
				{ GamePad.Player4Axis04,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  4 ) }
					}
				},
				{ GamePad.Player4Axis05,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  5 ) }
					}
				},
				{ GamePad.Player4Axis06,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  6 ) }
					}
				},
				{ GamePad.Player4Axis07,
					new AxisDefinition()
					{
						KeyCodes = null,
						MouseWheel = false,
						GamePadAxisDefinitions = new GamePadAxisDefinition[]{ new GamePadAxisDefinition(  3,  7 ) }
					}
				},
			} ;

			/// <summary>
			/// アクシスの状態を取得
			/// </summary>
			/// <param name="axisName"></param>
			/// <returns></returns>
			public float GetAxis( string axisName )
			{
				if( m_AxisDefinitions.ContainsKey( axisName ) == false )
				{
					// 定義情報が見つからない
					return 0 ;
				}

				var axisDefinition = m_AxisDefinitions[ axisName ] ;

				//---------------------------------

				float value = 0 ;

				// キーボード
				if( value == 0 && axisDefinition.KeyCodes != null && axisDefinition.KeyCodes.Length >  0 )
				{
					int i, l = axisDefinition.KeyCodes.Length ;
					for( i  = 0 ; i <  l ; i += 2 )
					{
						if( Keyboard.GetKey( axisDefinition.KeyCodes[ i + 0 ] ) == true )
						{
							value -= 1 ;
						}
						if( Keyboard.GetKey( axisDefinition.KeyCodes[ i + 1 ] ) == true )
						{
							value += 1 ;
						}
					}
				}

				// マウス
				if( value == 0 && axisDefinition.MouseWheel == true )
				{
					var delta = Mouse.ScrollDelta ;
					if( delta.x != 0 )
					{
						value = delta.x ;
					}
					else
					if( delta.y != 0 )
					{
						value = delta.y ;
					}

					if( value <  -1 )
					{
						value  = -1 ;
					}
					else
					if( value >  +1 )
					{
						value  = +1 ;
					}
				}

				// ゲームパッド
				if( value == 0 && axisDefinition.GamePadAxisDefinitions != null && axisDefinition.GamePadAxisDefinitions.Length >  0 )
				{
					foreach( var gamePadAxisDefinition in axisDefinition.GamePadAxisDefinitions )
					{
						var playerNumber	= gamePadAxisDefinition.PlayerNumber ;
						var axisNumber		= gamePadAxisDefinition.AxisNumber ;

						if( playerNumber <  0 )
						{
							// 全体対象
							for( playerNumber  = 0 ; playerNumber <  Gamepad.all.Count ; playerNumber ++ )
							{
								value = GetGamePadAxis( playerNumber, axisNumber ) ;
								if( value != 0 )
								{
									break ;
								}
							}
						}
						else
						{
							// 個別対象
							if( playerNumber <  Gamepad.all.Count )
							{
								value = GetGamePadAxis( playerNumber, axisNumber ) ;
							}
						}
					}

					static float GetGamePadAxis( int playerNumber, int axisNumber )
					{
						float value = 0 ;

						var gamepad = Gamepad.all[ playerNumber ] ;
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

						//-------------------------------

						float sign = Mathf.Sign( value ) ;
						value = Mathf.Abs( value ) ;
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

						//-------------------------------

						return value ;
					}
				}

				return value ;
			}
		}
	}
}
#endif
