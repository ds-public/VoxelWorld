using UnityEngine ;
using System ;
using System.Collections.Generic ;
using System.Linq ;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem ;
#endif


namespace InputHelper
{
	/// <summary>
	/// キーボード制御
	/// </summary>
	public partial class Keyboard
	{
		//-------------------------------------------------------------------------------------------
		// ↓共有不可

		private static InputManager m_Owner ;

		/// <summary>
		/// 初期化を行う
		/// </summary>
		public static void Initialize( bool inputSystemEnabled, InputManager owner )
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
		/// 全てキーボードの有効状況
		/// </summary>
		public static bool	Enabled { get ; set ; } = true ;

		/// <summary>
		/// リピートを開始するまでの時間(秒)
		/// </summary>
		public static float RepeatStartingTime { get ; set ; } = 0.50f ;

		/// <summary>
		/// リピートを繰り返す間隔の時間(秒)
		/// </summary>
		public static float RepeatIntervalTime { get ; set ; } = 0.05f ;

		//-------------------------------------------------------------------------------------------

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
			void Update() ;

			/// <summary>
			/// いずれかキーが押されているかどどうか
			/// </summary>
			/// <returns></returns>
			bool IsAnyKey() ;

			/// <summary>
			/// いずれかキーが押されたかどうか
			/// </summary>
			/// <returns></returns>
			bool IsAnyKeyDown() ;

			/// <summary>
			/// いずれかキーが離されたかどうか
			/// </summary>
			/// <returns></returns>
			bool IsAnyKeyUp() ;

			/// <summary>
			/// どのキーが押されているか確認する(デバッグ用)
			/// </summary>
			void CheckAllKeys() ;

			//---------------------------------------------------------------------------------

			/// <summary>
			/// キーが押されているかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKey( KeyCodes keyCode ) ;

			/// <summary>
			/// キーが押されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKeyDown( KeyCodes keyCode ) ;

			/// <summary>
			/// キーが離されたかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKeyUp( KeyCodes keyCode ) ;

			/// <summary>
			/// キーがリピート付きで押されているかどうかの判定
			/// </summary>
			/// <param name="keyCode"></param>
			/// <returns></returns>
			bool GetKeyRepeat( KeyCodes keyCode ) ;
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

			m_Implementation.Update() ;

			return true ;
		}

		/// <summary>
		/// いずれかキーが押されているか確認する
		/// </summary>
		/// <exception cref="Exception"></exception>
		public static bool IsAnyKey()
		{
			if( Enabled == false )
			{
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.IsAnyKey() ;
		}

		/// <summary>
		/// いずれかキーが押されているか確認する
		/// </summary>
		/// <exception cref="Exception"></exception>
		public static bool IsAnyKeyDown()
		{
			if( Enabled == false )
			{
				return false ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.IsAnyKeyDown() ;
		}

		/// <summary>
		/// どのキーが押されているか確認する(デバッグ用)
		/// </summary>
		/// <exception cref="Exception"></exception>
		public static void CheckAllKeys()
		{
			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			m_Implementation.CheckAllKeys() ;
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// キーが押されているかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKey( KeyCodes keyCode )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			if ( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}

			return m_Implementation.GetKey( keyCode ) ;
		}

		/// <summary>
		/// キーが押されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyDown( KeyCodes keyCode )
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

			return m_Implementation.GetKeyDown( keyCode ) ;
		}

		/// <summary>
		/// キーが離されたかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyUp( KeyCodes keyCode )
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

			return m_Implementation.GetKeyUp( keyCode ) ;
		}

		/// <summary>
		/// リピート付きでキーが押されているかどうかの判定
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static bool GetKeyRepeat( KeyCodes keyCode )
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

			return m_Implementation.GetKeyRepeat( keyCode ) ;
		}

		// ↑共有可能
		//-------------------------------------------------------------------------------------------

	}   // class
}   // namespace

