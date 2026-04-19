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
		//-------------------------------------------------------------------------------------------
		// ↓共有不可

		// オーナーのインスタンス
		private static UIEventSystem m_Owner ;

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

		// ↑共有不可
		//-------------------------------------------------------------------------------------------
		// ↓共有可能

		/// <summary>
		/// 全てマウスの有効状況
		/// </summary>
		public static bool	Enabled { get ; set ; } = true ;

		//-----------------------------

		/// <summary>
		/// 無意味ボタン
		/// </summary>
		public const int        None        = -1 ;

		/// <summary>
		/// 左ボタン番号
		/// </summary>
		public const int		LB          =  0 ;

		/// <summary>
		/// 右ボタン番号
		/// </summary>
		public const int		RB          =  1 ;

		/// <summary>
		/// 中ボタン番号
		/// </summary>
		public const int		MB          =  2 ;

		/// <summary>
		/// ホイールの上回転
		/// </summary>
		public const int        Wheel_U     =  3 ;

		/// <summary>
		/// ホイールの下回転
		/// </summary>
		public const int        Wheel_D     =  4 ;

		/// <summary>
		/// ホイール回転
		/// </summary>
		public const int        Wheel_Roll  = 12 ;

		/// <summary>
		/// ホイール移動
		/// </summary>
		public const int        Wheel_Drag  = 13 ;

		/// <summary>
		/// ポインター移動
		/// </summary>
		public const int        Move        = 20 ;

		//---------

		/// <summary>
		/// ボタンの数
		/// </summary>
		public const int		NumberOfButtons = 5 ;

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
			void Update( out bool button_0, out bool button_1, out bool button_2, out bool button_3, out bool button_4 ) ;

			//-----------------------------------------------------------

			/// <summary>
			/// ポインターの位置
			/// </summary>
			Vector3 Position{ get ; }

			/// <summary>
			/// ポインターの移動量
			/// </summary>
			Vector3 Delta{ get ; }

			/// <summary>
			/// ポインターのが画面内にあるかどうか
			/// </summary>
			bool IsInside{ get ; }

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
			bool GetButtonDown( int buttonNumber ) ;

			/// <summary>
			/// ボタンが離されたどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			bool GetButtonUp( int buttonNumber ) ;

			/// <summary>
			/// リピート付きでボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <param name="fromFixedUpdate"></param>
			/// <returns></returns>
			bool GetButtonRepeat( int buttonNumber ) ;

			/// <summary>
			/// アクシスが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			Vector2 GetAxis( int axisNumber ) ;

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			Vector2 GetAxisDown( int axisNumber ) ;

			/// <summary>
			/// ボタンが離されたどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <returns></returns>
			Vector2 GetAxisUp( int axisNumber ) ;

			/// <summary>
			/// リピート付きでボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonNumber"></param>
			/// <param name="fromFixedUpdate"></param>
			/// <returns></returns>
			Vector2 GetAxisRepeat( int axisNumber ) ;

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
		/// 毎フレーム実行する処理
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool Update()
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

			m_Implementation.Update( out _, out _, out _, out _, out _ ) ;

			return true ;
		}

		/// <summary>
		/// 毎フレーム実行する処理
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool Update( out bool button_0, out bool button_1, out bool button_2, out bool button_3, out bool button_4 )
		{
			button_0 = false ;
			button_1 = false ;
			button_2 = false ;
			button_3 = false ;
			button_4 = false ;

			//----------------------------------

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

			m_Implementation.Update( out button_0, out button_1, out button_2, out button_3, out button_4 ) ;

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
		/// ポインターの移動量
		/// </summary>
		public static Vector3 Delta
		{
			get
			{
				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}

				return m_Implementation.Delta ;
			}
		}

		/// <summary>
		/// ポインターがが画面内にあるかどうか
		/// </summary>
		public static bool IsInside
		{
			get
			{
				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}

				return m_Implementation.IsInside ;
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
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
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
		public static bool GetButtonDown( int buttonNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonDown( buttonNumber ) ;
		}

		/// <summary>
		/// ボタンが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonUp( buttonNumber ) ;
		}

		/// <summary>
		/// リピート付きでボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetButtonRepeat( buttonNumber ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アクシスが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxis( int axisNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetAxis( axisNumber ) ;
		}

		/// <summary>
		/// アクシスが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisDown( int axisNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetAxisDown( axisNumber ) ;
		}

		/// <summary>
		/// アクシスが離されたどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisUp( int axisNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetAxisUp( axisNumber ) ;
		}

		/// <summary>
		/// リピート付きでアクシスが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisRepeat( int axisNumber )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetAxisRepeat( axisNumber ) ;
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
				if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
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

		// ↑共有可能
		//-------------------------------------------------------------------------------------------

	}   // class
}   // namespace
