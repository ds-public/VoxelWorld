using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	/// <summary>
	/// ポインター制御
	/// </summary>
	public static class Pointer
	{
		/// <summary>
		/// 左ボタン番号
		/// </summary>
		public const int		LB = 0 ;

		/// <summary>
		/// 右ボタン番号
		/// </summary>
		public const int		RB = 1 ;

		/// <summary>
		/// 中ボタン番号
		/// </summary>
		public const int		MB = 2 ;

		/// <summary>
		/// ボタンの数
		/// </summary>
		public const int		NumberOfButtons = 3 ;

		//-----------------------------------

		/// <summary>
		/// ポインターの表示状態
		/// </summary>
		public static bool Visible
		{
			get
			{
				return UnityEngine.Cursor.visible ;
			}
			set
			{
				if( InputManager.CursorProcessing == true )
				{
					// InputManager でのカーソル制御は有効

					InputManager.SetCursorVisible( value ) ;
				}
				else
				{
					// InputManager でのカーソル制御は無効

					UnityEngine.Cursor.visible = value ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 毎フレーム更新(引数 isFixed は FixedUpdate からの呼び出しかどうか)
		/// </summary>
		public static void Update( bool fromFixedUpdate )
			=> Mouse.Update( fromFixedUpdate ) ;

		//-----------------------------------

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector3 Position
			=> Mouse.Position ;

		/// <summary>
		/// ポインターが押されているかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonIndex )
			=> Mouse.GetButton( buttonIndex ) ;

		/// <summary>
		/// ポインターが押されたかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonIndex, bool fromFixedUpdate = false )
			=> Mouse.GetButtonDown( buttonIndex, fromFixedUpdate ) ;

		/// <summary>
		/// ポインターが離されたかどうか
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonIndex, bool fromFixedUpdate = false )
			=> Mouse.GetButtonUp( buttonIndex, fromFixedUpdate ) ;

		/// <summary>
		/// リピート付きでポインターの状態を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonIndex, bool fromFixedUpdate = false )
			=> Mouse.GetButtonRepeat( buttonIndex, fromFixedUpdate ) ;
	}
}
