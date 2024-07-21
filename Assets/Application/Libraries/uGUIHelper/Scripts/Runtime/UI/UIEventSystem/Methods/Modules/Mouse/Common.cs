using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem ;
#endif


namespace uGUIHelper.InputAdapter
{
	/// <summary>
	/// マウス制御
	/// </summary>
	public partial class Mouse
	{
		private static UIEventSystem m_Owner ;

		//-------------------------------------------------------------------------------------------

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

		/// <summary>
		/// リピート開始までの時間(秒)
		/// </summary>
		public static float		RepeatStartingTime { get ; set ; } = 0.5f ;

		/// <summary>
		/// リピートする間隔の時間(秒)
		/// </summary>
		public static float		RepeatIntervalTime { get ; set ; } = 0.05f ;


		//-------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// 実装インターフェース
		/// </summary>
		public interface IImplementation
		{
			/// <summary>
			/// 初期化を行う
			/// </summary>
			void Initialize() ;

			/// <summary>
			/// フレーム毎の更新
			/// </summary>
			void Update( bool fromFixedUpdate ) ;

			//-----------------------------------------------------------

			/// <summary>
			/// ポインターの位置
			/// </summary>
			Vector3 Position{ get ; }

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButton( int buttonNumber ) ;

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButtonDown( int buttonNumber, bool fromFixedUpdate ) ;

			/// <summary>
			/// ボタンが離されたどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButtonUp( int buttonNumber, bool fromFixedUpdate ) ;

			/// <summary>
			/// リピート付きでボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <param name="fromFixedUpdate"></param>
			/// <returns></returns>
			bool GetButtonRepeat( int buttonNumber, bool fromFixedUpdate ) ;

			//----------------------------------

			/// <summary>
			/// ホイールの移動量
			/// </summary>
			Vector2 ScrollDelta{ get ; }
		}

		// 実装のインスタンス
		private static IImplementation m_Implementation ;

		//-------------------------------------------------------------------------------------------------------------------
		// 公開メソッド

		/// <summary>
		/// 初期化を行う
		/// </summary>
		public static void Initialize( bool inputSystemEnabled, UIEventSystem owner )
		{
			m_Owner = owner ;

			if( inputSystemEnabled == false )
			{
				// 旧版の実装を採用
				m_Implementation = new Implementation_OldVersion() ;
			}
#if ENABLE_INPUT_SYSTEM
			else
			{
				// 新版の実装を採用
				m_Implementation = new Implementation_NewVersion() ;
			}
#endif
			m_Implementation.Initialize() ;
		}

		/// <summary>
		/// 毎フレーム実行する処理
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool Update( bool fromFixedUpdate )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			m_Implementation.Update( fromFixedUpdate ) ;

			return true ;
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ポインターの位置
		/// </summary>
		public static Vector3 Position
		{
			get
			{
				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}

				return m_Implementation.Position ;
			}
		}

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButton( buttonNumber ) ;
		}

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonNumber, bool fromFixedUpdate = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonDown( buttonNumber, fromFixedUpdate ) ;
		}

		/// <summary>
		/// ボタンが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonNumber, bool fromFixedUpdate = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonUp( buttonNumber, fromFixedUpdate ) ;
		}

		/// <summary>
		/// リピート付きでボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonNumber, bool fromFixedUpdate = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonRepeat( buttonNumber, fromFixedUpdate ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ホイールの移動量
		/// </summary>
		public static Vector2 ScrollDelta
		{
			get
			{
				// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
				if( m_Owner == null || m_Owner.ControlEnabled == false )
				{
					// 無効
					return Vector2.zero ;
				}

				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}

				return m_Implementation.ScrollDelta ;
			}
		}
	}
}
