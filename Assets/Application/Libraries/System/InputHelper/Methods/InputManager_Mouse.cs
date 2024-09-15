using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	public partial class InputManager
	{
		// Mouse 関係

		/// <summary>
		/// ボタンのリピートを開始するまでの時間(秒)
		/// </summary>
		public static float MouseRepeatStartingTime
		{
			get{ return Mouse.RepeatStartingTime ; }
			set{ Mouse.RepeatStartingTime = value ; }
		}

		/// <summary>
		/// <summary>
		/// ボタンのリピートを繰り返す間隔の時間(秒)
		/// </summary>
		public static float MouseRepeatIntervalTime
		{
			get{ return Mouse.RepeatIntervalTime ; }
			set{ Mouse.RepeatIntervalTime = value ; }
		}

		//-------------------------------------------------------------------------------------------
		// 互換メソッド

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector3 MousePosition
			=> Mouse.Position ;

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButton( int buttonNumber )
			=> Mouse.GetButton( buttonNumber ) ;

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButtonDown( int buttonNumber, bool fromFixedUpdate = false )
			=> Mouse.GetButtonDown( buttonNumber, fromFixedUpdate ) ;

		/// <summary>
		/// ボタンが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButtonUp( int buttonNumber, bool fromFixedUpdate = false )
			=> Mouse.GetButtonUp( buttonNumber, fromFixedUpdate ) ;

		/// <summary>
		/// リピート付きでボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetMouseButtonRepeat( int buttonNumber, bool fromFixedUpdate = false )
			=> Mouse.GetButtonRepeat( buttonNumber, fromFixedUpdate ) ;

		/// <summary>
		/// ホイールの移動量
		/// </summary>
		public static Vector2 MouseScrollDelta
			=> Mouse.ScrollDelta ;
	}
}
