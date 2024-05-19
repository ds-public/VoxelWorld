using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace uGUIHelper.InputAdapter
{
	public partial class UIEventSystem
	{
		// 現在の入力の処理タイプ
		public InputProcessingTypes InputProcessingType = InputProcessingTypes.Parallel ;

		/// <summary>
		/// 入力の処理タイプを設定する
		/// </summary>
		/// <returns></returns>
		public static bool SetInputProcessingType( InputProcessingTypes inputProcessingType )
		{
			if( m_Instance == null )
			{
				// 失敗
				return false ;
			}

			m_Instance.SetInputProcessingType_Private( inputProcessingType ) ;
			return true ;
		}

		// 入力の処理タイプを設定する
		private void SetInputProcessingType_Private( InputProcessingTypes inputProcessingType )
		{
			if( InputProcessingType == inputProcessingType )
			{
				// 現在と同じなら何も処理しない
				return ;
			}

			//----------------------------------------------------------

			InputProcessingType = inputProcessingType ;

			if( InputProcessingType == InputProcessingTypes.Switching )
			{
				// シングルにする場合は初期状態はポインターとする

				m_InputType = InputTypes.Pointer ;
				m_InputHold = false ;

				UnityEngine.Cursor.visible = true ;
			}
			else
			if( InputProcessingType == InputProcessingTypes.Parallel )
			{
				// デュアルにする場合は念のためポインターを表示する(シングルのゲームパッド状態からの移行)

				UnityEngine.Cursor.visible = true ;
			}
		}

		/// <summary>
		/// 現在の入力の処理タイプ
		/// </summary>
		public static InputProcessingTypes GetInputProcessingType()
		{
			if( m_Instance == null )
			{
				return InputProcessingTypes.Unknown ;
			}

			return m_Instance.InputProcessingType ;
		}

		//-------------------------------------------------------------------------------------------

		// 現在の入力タイプ
		private InputTypes	m_InputType	= InputTypes.Pointer ;	// デフォルトはポインターモード
		private bool		m_InputHold	= false ;

		/// <summary>
		/// 現在の入力タイプ
		/// </summary>
		public static InputTypes InputType
		{
			get
			{
				if( m_Instance == null )
				{
					return InputTypes.Unknown ;
				}

				return m_Instance.m_InputType ;
			}
		}

		/// <summary>
		/// 現在のモードが実際に有効かどうか
		/// </summary>
		public static bool InputEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return ! m_Instance.m_InputHold ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// コンポーネントなので public フィールドを使ってはいけない(インスタンスが生成された際にデフォルト値で初期化されてしまい事前に設定した値は無効化される)

		/// <summary>
		/// カーソルの制御状態
		/// </summary>
		public static bool CursorProcessing => m_CursorProcessing ;

		// カーソルの制御状態
		private static bool m_CursorProcessing = true ;

		/// <summary>
		/// カーソルの制御の有無を設定する
		/// </summary>
		/// <returns></returns>
		public static void SetCursorProcessing( bool state )
		{
			m_CursorProcessing = state ;
		}

		//---------------

		// カーソルの表示状態
		private bool m_ActiveCursorVisible = true ;

		// システム制御のカーソルの表示状態
		private bool m_SystemCursorVisible = true ;

		/// <summary>
		/// カーソルの表示状態(public のフィールドにしてはいけない)
		/// </summary>
		public bool CursorVisible => m_CursorVisible ;

		// カーソルの表示状態(public のフィールドにしてはいけない)
		private bool m_CursorVisible = true ;

		/// <summary>
		/// カーソルの表示状態のを設定する
		/// </summary>
		/// <returns></returns>
		public static bool SetCursorVisible( bool state )
		{
			if( m_Instance == null )
			{
				// 失敗
				return false ;
			}

			m_Instance.m_CursorVisible = state ;

			return true ;
		}
	}
}
