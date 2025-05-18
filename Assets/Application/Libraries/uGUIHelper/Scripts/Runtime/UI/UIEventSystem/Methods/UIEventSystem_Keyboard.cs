using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace uGUIHelper.InputAdapter
{
	public partial class UIEventSystem
	{
		// Keyboard 関係

		/// <summary>
		/// キーボードのリピートを開始するまでの時間(秒)
		/// </summary>
		public static float KeyboardRepeatStartingTime
		{
			get{ return Keyboard.RepeatStartingTime ; }
			set{ Keyboard.RepeatStartingTime = value ; }
		}

		/// <summary>
		/// <summary>
		/// キーボードのリピートを繰り返す間隔の時間(秒)
		/// </summary>
		public static float KeyboardRepeatIntervalTime
		{
			get{ return Keyboard.RepeatIntervalTime ; }
			set{ Keyboard.RepeatIntervalTime = value ; }
		}

		//-------------------------------------------------------------------------------------------
		// 互換メソッド

		/// <summary>
		/// 押されているキーを確認する
		/// </summary>
		public static void CheckAllKeys()
			=> Keyboard.CheckAllKeys() ;

		/// <summary>
		/// キーが押されているかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKey( KeyCodes keyCode )
			=> Keyboard.GetKey( keyCode ) ;

		/// <summary>
		/// キーが押されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyDown( KeyCodes keyCode )
			=> Keyboard.GetKeyDown( keyCode ) ;

		/// <summary>
		/// キーが離されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyUp( KeyCodes keyCode )
			=> Keyboard.GetKeyUp( keyCode ) ;

		/// <summary>
		/// キーがリピート付きで押されているかどうか
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyRepeat( KeyCodes keyCode )
			=> Keyboard.GetKeyRepeat( keyCode ) ;
	}
}
