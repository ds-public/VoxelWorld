using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	public partial class InputManager
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

			// 変化前の入力タイプ
			public static InputTypes			PreviousInputType	= InputTypes.Pointer ;

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
		public static void SetInputProcessingType( InputProcessingTypes inputProcessingType, InputTypes inputType = InputTypes.Unknown )
		{
			if( Settings.InputProcessingType == inputProcessingType )
			{
				// 現在と同じなら何も処理しない
				return ;
			}

			//----------------------------------------------------------

			Settings.InputProcessingType = inputProcessingType ;

			if( inputType == InputTypes.Unknown )
			{
				if( m_Instance != null )
				{
					m_Instance.SetInputType_Private( Settings.InputType ) ;
				}
			}
			else
			{
				if( Settings.InputProcessingType == InputProcessingTypes.Switching )
				{
					if( m_Instance == null )
					{
						if( inputType == InputTypes.Pointer )
						{
							Settings.InputType = InputTypes.Pointer ;

							UnityEngine.Cursor.visible = true ;
						}
						else
						if( inputType == InputTypes.Keyboard )
						{
							Settings.InputType = InputTypes.Keyboard ;

							UnityEngine.Cursor.visible = false ;
						}
						else
						if( inputType == InputTypes.GamePad )
						{
							Settings.InputType = InputTypes.GamePad ;

							UnityEngine.Cursor.visible = false ;
						}
					}
					else
					{
						m_Instance.SetInputType_Private( inputType ) ;
					}
				}
				else
				if( Settings.InputProcessingType == InputProcessingTypes.Parallel )
				{
					// デュアルにする場合は念のためポインターを表示する(シングルのゲームパッド状態からの移行)

					if( m_Instance == null )
					{
						Settings.InputType = inputType ;

						UnityEngine.Cursor.visible = true ;
					}
					else
					{
						m_Instance.SetInputType_Private( inputType ) ;
					}
				}
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
		/// 最後の入力タイプ(ダイナミック)
		/// </summary>
		public InputTypes ActiveInputType => Settings.InputType ;

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
