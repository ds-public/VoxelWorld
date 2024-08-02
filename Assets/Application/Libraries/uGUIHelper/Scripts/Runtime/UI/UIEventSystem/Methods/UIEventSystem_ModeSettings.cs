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
				m_InputSwitching = false ;

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
		/// 最後の入力タイプ
		/// </summary>
		public InputTypes LastInputType => m_InputType ;

		// 入力モードを切り替え中かどうか
		private bool		m_InputSwitching	= false ;

		/// <summary>
		/// 入力モードを切り替え中かどうか
		/// </summary>
		public bool InputSwitching
		{
			get
			{
				if( m_IgnoreInputSwitching == true )
				{
					// 常に入力は有効
					return false ;
				}

				return m_InputSwitching ;
			}
		}

		// 入力モード切り替え中の値を無視して常に入力を有効にするかどうか
		private bool        m_IgnoreInputSwitching = false ;

		/// <summary>
		/// 入力モード切り替え中の値を無視して常に入力を有効にするかどうか
		/// </summary>
		public bool IgnoreInputSwitching => m_IgnoreInputSwitching ;

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
