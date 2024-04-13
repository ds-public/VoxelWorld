using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;


namespace InputHelper
{
	/// <summary>
	/// ゲームパッド制御
	/// </summary>
	public partial class GamePad
	{
		//-------------------------------------------------------------------------------------------
		// 旧版

		/// <summary>
		/// 旧版の実装
		/// </summary>
		public partial class Implementation_OldVersion : IImplementation
		{
			//------------------------------------------------------------------------------------------
			// 互換メソッド

			/// <summary>
			/// 接続中のゲームパッドの数
			/// </summary>
			public int NumberOfGamePads
			{
				get
				{
					var names = GetJoystickNames() ;
					return names.Length ;
				}
			}

			/// <summary>
			/// 接続中のゲームパッドの名前を取得する
			/// </summary>
			/// <returns></returns>
			public string[] GetJoystickNames()
			{
				var acquiredNames = Input.GetJoystickNames() ;

				var availableNames = new List<string>() ;

				// 空白を除外する必要がある
				if( acquiredNames != null && acquiredNames.Length >  0 )
				{
					foreach( var acquiredName in acquiredNames )
					{
						if( string.IsNullOrEmpty( acquiredName ) == false )
						{
							availableNames.Add( acquiredName ) ;
						}
					}
				}

				return availableNames.ToArray() ;
			}

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			public bool GetButton( string buttonName )
			{
				return Input.GetButton( buttonName ) ;
			}

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			public bool GetButtonDown( string buttonName )
			{
				return Input.GetButtonDown( buttonName ) ;
			}

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			public bool GetButtonUp( string buttonName )
			{
				return Input.GetButtonUp( buttonName ) ;
			}

			/// <summary>
			/// アクシスの状態を取得
			/// </summary>
			/// <param name="axisName"></param>
			/// <returns></returns>
			public float GetAxis( string axisName )
			{
				var value = Input.GetAxis( axisName ) ;

				//---------------------------------

				float sign = Mathf.Sign( value ) ;
				value = Mathf.Abs( value ) ;
				if( value <  m_AxisLowerThreshold )
				{
					// チャタリング防止
					value = 0 ;
				}
				else
				{
					if( value >  m_AxisUpperThreshold )
					{
						// 最大補正
						value = 1 ;
					}
					else
					{
						// フィッティング
						value = ( value - m_AxisLowerThreshold ) / ( m_AxisUpperThreshold - m_AxisLowerThreshold ) ;
					}
					value *= sign ;
				}

				//---------------------------------

				return value ;
			}
		}
	}
}
