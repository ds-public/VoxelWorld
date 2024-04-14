using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using InputHelper ;

namespace DSW
{
	public class GamePadMapper
	{
		/// <summary>
		/// ボタンのスキンの種類
		/// </summary>
		public enum SkinTypes
		{
			XboxPad		= 0,
			DualShock	= 1,
		}

		public static SkinTypes SkinType = SkinTypes.DualShock ;

		/// <summary>
		/// ゲームパッドを使用できるようにする
		/// </summary>
		public static void Setup()
		{
			if( InputManager.InputSystemEnabled == false )
			{
				GamePad.AddProfile( 0, GamePad.Profile_Xbox ) ;
				GamePad.AddProfile( 1, GamePad.Profile_DualShock ) ;

				// 名前からデフォルトのプロフィールを設定する
				string[] names = GamePad.GetNames() ;

				if( names != null && names.Length >  0 )
				{
					int i, l = names.Length ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						Debug.Log( "GamePad : " + i + " Name = " + names[ i ] ) ;

						if( string.IsNullOrEmpty( names[ i ] ) == false )
						{
							if
							(
								names[ i ].ToLower().Contains( "Xbox".ToLower() ) == true ||
								names[ i ].ToLower().Contains( "X-box".ToLower() ) == true
							)
							{
								Debug.Log( "Player " + i + " = Xbox" ) ;
								GamePad.SetProfileNumber( i, 0 ) ;
								SkinType = SkinTypes.XboxPad ;
								GamePad.SwapB1toB2 = false ;
							}
							else
							{
								Debug.Log( "Player " + i + " = PlayStation" ) ;
								GamePad.SetProfileNumber( i, 1 ) ;
								SkinType = SkinTypes.DualShock ;
								GamePad.SwapB1toB2 = true ;
							}
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 方向キーの入力状態を取得する(アナログ化)
		/// </summary>
		/// <returns></returns>
		public static ( int x, int y ) GetAxisOfDigital()
		{
			int x = 0 ;
			int y = 0 ;

			Vector2 axis0 = GamePad.GetAxis( 0 ) ;
			Vector2 axis1 = GamePad.GetAxis( 1 ) ;

			if( axis0.x <  0 || axis1.x <  0 )
			{
				// ←
				x = -1 ;
			}
			else
			if( axis0.x >  0 || axis1.x >  0 )
			{
				// →
				x =  1 ;
			}

			if( axis0.y <  0 || axis1.y <  0 )
			{
				// ↑
				y = -1 ;
			}
			else
			if( axis0.y >  0 || axis1.y >  0 )
			{
				// ↓
				y =  1 ;
			}

			return ( x, y ) ;
		}

		/// <summary>
		/// 方向キーの入力状態を取得する(アナログ化)
		/// </summary>
		/// <returns></returns>
		public static ( int x, int y ) GetRepeatAxisOfDigital()
		{
			int x = 0 ;
			int y = 0 ;

			Vector2 axis0 = GamePad.GetAxisRepeat( 0 ) ;
			Vector2 axis1 = GamePad.GetAxisRepeat( 1 ) ;

			if( axis0.x <  0 || axis1.x <  0 )
			{
				// ←
				x = -1 ;
			}
			else
			if( axis0.x >  0 || axis1.x >  0 )
			{
				// →
				x =  1 ;
			}

			// ↑は減少・↓は増加に変えている
			if( axis0.y <  0 || axis1.y <  0 )
			{
				// ↓
				y =  1 ;
			}
			else
			if( axis0.y >  0 || axis1.y >  0 )
			{
				// ↑
				y = -1 ;
			}

			return ( x, y ) ;
		}
	}
}
