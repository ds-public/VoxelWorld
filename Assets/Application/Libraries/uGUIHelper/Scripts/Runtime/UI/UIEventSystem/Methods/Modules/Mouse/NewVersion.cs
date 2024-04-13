#if ENABLE_INPUT_SYSTEM

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

using UnityEngine.InputSystem ;


namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		// 新版
		public class Implementation_NewVersion : IImplementation
		{
			/// ポインターの位置
			/// </summary>
			public Vector3 Position
			{
				get
				{
					UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
					if( mouse == null )
					{
						return Vector3.zero ;
					}

					return mouse.position.ReadValue() ;
				}
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonNumber )
			{
				UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
				if( mouse == null )
				{
					return false ;
				}

				return buttonNumber switch
				{
					0 => mouse.leftButton.isPressed,
					1 => mouse.rightButton.isPressed,
					2 => mouse.middleButton.isPressed,
					_ => false,
				} ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonNumber )
			{
				UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
				if( mouse == null )
				{
					return false ;
				}

				return buttonNumber switch
				{
					0 => mouse.leftButton.wasPressedThisFrame,
					1 => mouse.rightButton.wasPressedThisFrame,
					2 => mouse.middleButton.wasPressedThisFrame,
					_ => false,
				} ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonNumber )
			{
				UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
				if( mouse == null )
				{
					return false ;
				}

				return buttonNumber switch
				{
					0 => mouse.leftButton.wasReleasedThisFrame,
					1 => mouse.rightButton.wasReleasedThisFrame,
					2 => mouse.middleButton.wasReleasedThisFrame,
					_ => false,
				} ;
			}

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			public Vector2 ScrollDelta
			{
				get
				{
					UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current ;
					if( mouse == null )
					{
						return Vector2.zero ;
					}

					// 割る値は要調整
					return mouse.scroll.ReadValue() ;
				}
			}
		}
	}
}
#endif
