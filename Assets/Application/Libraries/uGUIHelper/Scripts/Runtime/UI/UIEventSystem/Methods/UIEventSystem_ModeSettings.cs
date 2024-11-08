using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace uGUIHelper.InputAdapter
{
	public partial class UIEventSystem
	{
		/// <summary>
		/// インスタンスが破棄されてしまう可能性があるためこれらの値はスタティックで保持する
		/// </summary>
		public static class Settings
		{
			// 現在の入力の処理タイプ
			public static InputProcessingTypes  InputProcessingType = InputProcessingTypes.Parallel ;

			// 現在の入力タイプ(UIEventSystem がシーン単位で破棄されてしまうため、この値のみ static で保持する)
			public static InputTypes	        InputType	        = InputTypes.Pointer ;	// デフォルトはポインターモード

			//-------------------------------------------------

			// マウスカーソルの位置
			public static Vector2               MousePosition ;

			//-------------------------------------------------

			// カーソルの制御状態
			public static bool                  CursorProcessing = true ;

			// カーソルの表示状態
			public static bool                  ActiveCursorVisible = true ;

			// システム制御のカーソルの表示状態
			public static bool                  SystemCursorVisible = true ;

			// カーソルの表示状態(public のフィールドにしてはいけない)
			public static bool                  CursorVisible = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 入力の処理タイプを設定する
		/// </summary>
		/// <returns></returns>
		public static void SetInputProcessingType( InputProcessingTypes inputProcessingType, InputTypes inputType = InputTypes.Pointer )
		{
			if( Settings.InputProcessingType == inputProcessingType )
			{
				// 現在と同じなら何も処理しない
				return ;
			}

			//----------------------------------------------------------

			Settings.InputProcessingType = inputProcessingType ;

			if( Settings.InputProcessingType == InputProcessingTypes.Switching )
			{
				if( inputType == InputTypes.Pointer )
				{
					Settings.InputType = InputTypes.Pointer ;

					UnityEngine.Cursor.visible = true ;
				}
				else
				if( inputType == InputTypes.GamePad )
				{
					Settings.InputType = InputTypes.GamePad ;

					UnityEngine.Cursor.visible = false ;
				}
			}
			else
			if( Settings.InputProcessingType == InputProcessingTypes.Parallel )
			{
				// デュアルにする場合は念のためポインターを表示する(シングルのゲームパッド状態からの移行)

				UnityEngine.Cursor.visible = true ;
			}
		}

		/// <summary>
		/// 現在の入力の処理タイプ
		/// </summary>
		public static InputProcessingTypes InputProcessingType => Settings.InputProcessingType ;

		/// <summary>
		/// 現在の入力タイプ
		/// </summary>
		public static InputTypes InputType  => Settings.InputType ;

		/// <summary>
		/// 最後の入力タイプ
		/// </summary>
		public InputTypes LastInputType => Settings.InputType ;

		//-------------------------------------------------------------------------------------------
		// コンポーネントなので Dynamic なフィールドを使ってはいけない(インスタンスが生成された際にデフォルト値で初期化されてしまい事前に設定した値は無効化される)

		/// <summary>
		/// カーソルの制御状態
		/// </summary>
		public static bool CursorProcessing => Settings.CursorProcessing ;

		/// <summary>
		/// カーソルの制御の有無を設定する
		/// </summary>
		/// <returns></returns>
		public static void SetCursorProcessing( bool state )
		{
			Settings.CursorProcessing = state ;
		}

		//---------------

		/// <summary>
		/// カーソルの表示状態(public のフィールドにしてはいけない)
		/// </summary>
		public bool CursorVisible => Settings.CursorVisible ;

		/// <summary>
		/// カーソルの表示状態のを設定する
		/// </summary>
		/// <returns></returns>
		public static void SetCursorVisible( bool state )
		{
			Settings.CursorVisible = state ;
		}
	}
}
