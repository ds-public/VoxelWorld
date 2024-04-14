using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		// 旧版
		public class Implementation_OldVersion : IImplementation
		{
			/// <summary>
			/// ポインターの位置
			/// </summary>
			public Vector3 Position
			{
				get
				{
					return Input.mousePosition ;
				}
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButton( int buttonNumber )
			{
				return Input.GetMouseButton( buttonNumber ) ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonNumber )
			{
				return Input.GetMouseButtonDown( buttonNumber ) ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonNumber )
			{
				return Input.GetMouseButtonUp( buttonNumber ) ;
			}

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			public Vector2 ScrollDelta
			{
				get
				{
					return Input.mouseScrollDelta ;
				}
			}
		}
	}
}

