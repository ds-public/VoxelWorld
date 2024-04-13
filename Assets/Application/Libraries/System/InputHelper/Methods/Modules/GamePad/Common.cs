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
	/// ゲームパッド制御
	/// </summary>
	public partial class GamePad
	{
		/// <summary>
		/// 全てゲームパッドの有効状況
		/// </summary>
		public static bool	Enabled { get ; set ; } = true ;

		/// <summary>
		/// 最大のプレイヤーの数
		/// </summary>
		public const int	MaximumNumberOfPlayers	=  4 ;

		/// <summary>
		/// 最大のボタンの数
		/// </summary>
		public const int	MaximumNumberOfButtons	= 16 ;

		/// <summary>
		/// 最大のアクシスの数
		/// </summary>
		public const int	MaximumNumberOfAxes		=  4 ;

		//-----------------------------------

		/// <summary>
		/// プレイヤーいずれかの識別番号
		/// </summary>
		public const int PlayerAny = -1 ;

		/// <summary>
		/// プレイヤー１の識別番号
		/// </summary>
		public const int Player1 = 0 ;

		/// <summary>
		/// プレイヤー２の識別番号
		/// </summary>
		public const int Player2 = 1 ;

		/// <summary>
		/// プレイヤー３の識別番号
		/// </summary>
		public const int Player3 = 2 ;

		/// <summary>
		/// プレイヤー４の識別番号
		/// </summary>
		public const int Player4 = 3 ;

		//-----------------------------------

		/// <summary>
		/// 基本ボタン１
		/// </summary>
		public const int B1	= 0x0001 ;

		/// <summary>
		/// 基本ボタン２
		/// </summary>
		public const int B2	= 0x0002 ;

		/// <summary>
		/// 基本ボタン３
		/// </summary>
		public const int B3	= 0x0004 ;

		/// <summary>
		/// 基本ボタン４
		/// </summary>
		public const int B4	= 0x0008 ;

		/// <summary>
		/// Ｒボタン１
		/// </summary>
		public const int R1	= 0x0010 ;

		/// <summary>
		/// Ｌボタン１
		/// </summary>
		public const int L1	= 0x0020 ;

		/// <summary>
		/// Ｒボタン２
		/// </summary>
		public const int R2	= 0x0040 ;

		/// <summary>
		/// Ｌボタン２
		/// </summary>
		public const int L2	= 0x0080 ;

		/// <summary>
		/// Ｒボタン３
		/// </summary>
		public const int R3	= 0x0100 ;
		
		/// <summary>
		/// Ｌボタン３
		/// </summary>
		public const int L3	= 0x0200 ;

		/// <summary>
		/// オプションボタン１
		/// </summary>
		public const int O1	= 0x0400 ;

		/// <summary>
		/// オプションボタン２
		/// </summary>
		public const int O2	= 0x0800 ;

		/// <summary>
		/// オプションボタン３
		/// </summary>
		public const int O3	= 0x1000 ;

		/// <summary>
		/// オプションボタン４
		/// </summary>
		public const int O4	= 0x2000 ;

		//---------------

		/// <summary>
		/// 方向ボタン
		/// </summary>
		public const int DP = 0 ;

		/// <summary>
		/// 左スティック
		/// </summary>
		public const int LS = 1 ; 

		/// <summary>
		/// 右スティック
		/// </summary>
		public const int RS = 2 ;

		/// <summary>
		/// トリガーボタン(アナログ取得)
		/// </summary>
		public const int TB = 3 ;

		//-----------------------------------------------------------

		// アクシスのしきい値(上限)
		private static float m_AxisUpperThreshold = 0.98f ;

		/// <summary>
		/// アクシスのしきい値(上限)
		/// </summary>
		public static float AxisUpperThreshold
		{
			get
			{
				return m_AxisUpperThreshold ;
			}
			set
			{
				m_AxisUpperThreshold = value ;
				if( m_AxisUpperThreshold >  1.0f )
				{
					m_AxisUpperThreshold  = 1.0f ;
				}
				else
				if( m_AxisUpperThreshold <  0.5f )
				{
					m_AxisUpperThreshold  = 0.5f ;
				}
			}
		}

		// アクシスのしきい値(下限)
		private static float m_AxisLowerThreshold = 0.2f ;

		/// <summary>
		/// アクシスのしきい値(下限)
		/// </summary>
		public static float AxisLowerThreshold
		{
			get
			{
				return m_AxisLowerThreshold ;
			}
			set
			{
				m_AxisLowerThreshold = value ;
				if( m_AxisLowerThreshold <  0.0f )
				{
					m_AxisLowerThreshold  = 0.0f ;
				}
				else
				if( m_AxisLowerThreshold >  0.4f )
				{
					m_AxisLowerThreshold  = 0.4f ;
				}
			}
		}

		/// <summary>
		/// リピートを開始するまでの時間(秒)
		/// </summary>
		public static float RepeatStartingTime { get ; set ; } = 0.5f ;

		/// <summary>
		/// リピートを繰り返す間隔の時間(秒)
		/// </summary>
		public static float RepeatIntervalTime { get ; set ; } = 0.05f ;

		/// <summary>
		/// 完全アナログ値をデジタルと認識するしきい値
		/// </summary>
		public static float AnalogToDigitalThreshold { get ; set ; } = 0.75f ;
		
		/// <summary>
		/// ボタン１とボタン２の入れ替え
		/// </summary>
		public static bool SwapB1toB2 { get ; set ; } = false ;	// ボタン１とボタン２を入れ替えるかどうか(入れ替えない場合はＤＳの場合は×が決定になる)

		/// <summary>
		/// ボタン３とボタン４の入れ替え
		/// </summary>
		public static bool SwapB3toB4 { get ; set ; } = false ;	// ボタン３とボタン４を入れ替えるかどうか

		/// <summary>
		/// キーボードのボタンマッピング
		/// </summary>
		public static bool MappingKeyboardToButtonEnabled { get ; set ; } = true ;

		/// <summary>
		/// キーボードのアクシスマッピング
		/// </summary>
		public static bool MappingKeyboardToAxisEnabled { get ; set ; } = true ;

		//-------------------------------------------------------------------------------------------

		// ボタン識別子群(インデックス番号→ボタン識別子)
		private static readonly int[] m_ButtonIdentities =
		{
			B1, B2, B3, B4,
			R1, L1, R2, L2, R3, L3,
			O1, O2, O3, O4,
		} ;

		// ボタン識別子のインデックス番号との関係(ボタン番号ではない事に注意)　ボタン識別子には対応する順番値(インデックス)が存在する
		private static readonly Dictionary<int,int> m_ButtonIdentityToIndex = new ()
		{
			{ B1,  0 }, { B2,  1 }, { B3,  2 }, { B4,  3 },
			{ R1,  4 }, { L1,  5 }, { R2,  6 }, { L2,  7 }, { R3,  8 }, { L3,  9 },
			{ O1, 10 }, { O2, 11 }, { O3, 12 }, { O4, 13 },
		} ;

		//-----

		// アクシス識別子群(インデックス番号→アクシス識別子)
		private static readonly int[] m_AxisIdentities =
		{
			DP, LS, RS, TB,
		} ;

		// アクシス識別子のインデックス番号との関係(アクシス番号ではない事に注意)　アクシス識別子には対応する順番値(インデックス)が存在する
		private static readonly Dictionary<int,int> m_AxisIdentityToIndex = new ()
		{
			{ DP,  0 }, { LS,  1 }, { RS,  2 }, { TB,  3 },
		} ;

		//-------------------------------------------------------------------------------------------
		// リレーション関係

		// identity → ( index → profile-mapping : [index] ) → number
		//  ↓↑
		//  index

		//-------------------------------------------------------------------------------------------

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

				// プロファイル情報を初期化する
				m_Profiles.Clear() ;
				m_Profiles.Add( -1, Profile_Xbox ) ;		// デフォルトプロファイル
			}
#if ENABLE_INPUT_SYSTEM
			else
			{
				// 新版の実装を採用
				m_Implementation = new Implementation_NewVersion() ;

				// プロファイル情報を初期化する
				m_Profiles.Clear() ;
				m_Profiles.Add( -1, Profile_InputSystem ) ;	// デフォルトプロファイル
			}
#endif
			//----------------------------------

			// プレイヤー情報を初期化する
			m_Players.Clear() ;
			for( int playerNumber  = 0 ; playerNumber <  MaximumNumberOfPlayers ; playerNumber ++ )
			{
				m_Players.Add( new Player( playerNumber ) ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プロファイル情報
		/// </summary>
		public class Profile
		{
			/// <summary>
			/// ボタン番号群
			/// </summary>
			public int[]	ButtonNumbers	= null ;

			/// <summary>
			/// アクシス番号群
			/// </summary>
			public int[]	AxisNumbers		= null ;

			/// <summary>
			/// アナログボタンの入力を補正するかどうか
			/// </summary>
			public bool		AnalogButtonCorrection	= false ;

			/// <summary>
			/// アナログボタンのデジタル判定時のしきい値
			/// </summary>
			public float	AnalogButtonThreshold	= 0.2f ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="buttonNumbers"></param>
			/// <param name="axisNumbers"></param>
			/// <param name="analogButtonCorrection"></param>
			/// <param name="analogButtonThreshold"></param>
			public Profile( int[] buttonNumbers, int[] axisNumbers, bool analogButtonCorrection, float analogButtonThreshold )
			{
				ButtonNumbers			= buttonNumbers ;
				AxisNumbers				= axisNumbers ;
				AnalogButtonCorrection	= analogButtonCorrection ;
				AnalogButtonThreshold	= analogButtonThreshold ;
			}
		}

		//-----------------------------------------------------------
		// 旧版のプロファイル

#if UNITY_EDITOR || UNITY_STANDALONE

		/// <summary>
		/// XboxPad のプロファイル情報(旧 InputManager 用)
		/// </summary>
		public static Profile Profile_Xbox { get ; set ; } = new
		(
			new int[]{  0,  1,  2,  3,  5,  4, -1, -1,  9,  8,  7,  6,	11, 10 },
			new int[]{  6,  7,  1,  2,  4,  5, 10,  9 },
			false, 0.4f
		) ;

#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE)

		/// <summary>
		/// XboxPad のプロファイル情報(旧 InputManager 用)
		/// </summary>
		public static Profile Profile_Xbox { get ; set ; } = new
		(
			new int[]{  0,  1,  2,  3,  5,  4, -1, -1,  9,  8,  7,  6,	11, 10 },
			new int[]{  5,  6,  1,  2,  3,  4,  8,  7 },
			false, 0.4f
		) ;
#endif

		/// <summary>
		/// DuckShock4 のプロファイル情報(旧 InputManager 用)
		/// </summary>
		public static Profile Profile_DualShock { get ; set ; } = new
		(
			new int[]{  1,  2,  0,  3,  5,  4,  7,  6, 11, 10,  9,  8, 13, 12 },
			new int[]{  7,  8,  1,  2,  3,  6,  5,  4 },
			true,  0.4f
		) ;

		//-----------------------------------------------------------
		// 新版のプロファイル

		/// <summary>
		/// InputSystem のプロファイル情報(新 InputSystem 用)
		/// </summary>
		public static Profile Profile_InputSystem { get ; set ; } = new
		(
			new int[]{  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13 },
			new int[]{  0,  1,  2,  3,  4,  5,  6,  7 },
			true,  0.4f
		) ;

		//-----------------------------------------------------------

		// プロファイル情報(0～7)
		private static readonly Dictionary<int,Profile> m_Profiles = new () ;

		/// <summary>
		/// プロファィル情報を追加する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <param name="buttonNumbers"></param>
		/// <param name="axisNumbers"></param>
		/// <param name="analogButtonCorrection"></param>
		/// <param name="analogButtonThreshold"></param>
		/// <returns></returns>
		public static bool AddProfile
		(
			int profileNumber,
			int[] buttonNumbers, int[] axisNumbers,
			bool analogButtonCorrection, float analogButtonThreshold
		)
		{
			return AddProfile
			(
				profileNumber,
				new Profile
				(
					buttonNumbers, axisNumbers,
					analogButtonCorrection, analogButtonThreshold
				)
			) ;
		}

		/// <summary>
		/// プロファィル情報を追加する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <param name="profile"></param>
		/// <returns></returns>
		public static bool AddProfile( int profileNumber, Profile profile )
		{
			if( profileNumber <  0 )
			{
				return false ;
			}

			if( m_Profiles.ContainsKey( profileNumber ) == false )
			{
				m_Profiles.Add( profileNumber, profile ) ;
			}
			else
			{
				m_Profiles[ profileNumber ] = profile ;
			}

			return true ;
		}

		/// <summary>
		/// プロファイル情報を削除する
		/// </summary>
		/// <param name="profileNumber">0～</param>
		/// <returns></returns>
		public static bool RemoveProfile( int profileNumber )
		{
			if( profileNumber <  0 )
			{
				return false ;
			}

			if( m_Profiles.ContainsKey( profileNumber ) == false )
			{
				return false ;
			}
			else
			{
				m_Profiles.Remove( profileNumber ) ;
			}

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プレイヤー情報管理
		/// </summary>
		public class Player
		{
			/// <summary>
			/// 番号
			/// </summary>
			public int Number { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// プレイヤーの使用するプロファイル番号
			/// </summary>
			public int	ProfileNumber = -1 ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public Player( int number )
			{
				// 番号を記録
				Number = number ;

				//---------------------------------

				int length ;

				// ボタン
				length = Math.Min( MaximumNumberOfButtons, m_ButtonIdentities.Length ) ;
				m_ButtonStates = new ButtonState[ length, 2 ] ;
				for( int buttonIndex  = 0 ; buttonIndex <  length ; buttonIndex ++ )
				{
					m_ButtonStates[ buttonIndex, 0 ] = new ButtonState() ;
					m_ButtonStates[ buttonIndex, 1 ] = new ButtonState() ;
				}

				// アクシス
				length = Math.Min( MaximumNumberOfAxes, m_AxisIdentities.Length ) ;
				m_AxisStates = new AxisState[ length, 2 ] ;
				for( int axisIndex  = 0 ; axisIndex <  length ; axisIndex ++ )
				{
					m_AxisStates[ axisIndex, 0 ] = new AxisState() ;
					m_AxisStates[ axisIndex, 1 ] = new AxisState() ;
				}
			}

			/// <summary>
			/// ボタン用状態
			/// </summary>
			public class ButtonState
			{
				public bool		RepeatKeepFlag ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public bool		IsRepeat ;
				public bool		IsDown ;
				public bool		IsUp ;
			}

			private readonly ButtonState[,] m_ButtonStates ;

			/// <summary>
			/// アクシス用状態
			/// </summary>
			public class AxisState
			{
				public bool		RepeatKeepFlag ;
				public Vector2	RepeatKeepData ;
				public float	RepeatWakeTime ;
				public float	RepeatLoopTime ;
				public Vector2	IsRepeat ;
				public Vector2	IsDown ;
				public Vector2	IsUp ;
			}

			private readonly AxisState[,] m_AxisStates ;

			//--------------
			// 振動関係

			// 振動中かどうか
			private bool	m_IsHapticsMoving ;

			// 振動時間
			private float	m_HapticsDuration ;

			// 開始時間
			private float	m_HapticsWakeTime ;

			//------------------------------------------------------------------------------------------
			// アクセス禁止メソッド

			/// <summary>
			/// ボタンの状態更新を行う(アクセス禁止)
			/// </summary>
			/// <param name="buttonIndex"></param>
			/// <param name="buttonIdentity"></param>
			/// <param name="buttonFlags"></param>
			public void UpdateButtonStates( int buttonIndex, int buttonIdentity, int buttonFlags, int slotNumber )
			{
				ButtonState state = m_ButtonStates[ buttonIndex, slotNumber ] ;

				//---------------------------------

				state.IsRepeat	= false ;
				state.IsDown	= false ;
				state.IsUp		= false ;

				if( ( buttonFlags & buttonIdentity ) != 0 )
				{
					if( state.RepeatKeepFlag == false )
					{
						// リピート開始
						state.IsRepeat = true ;

						state.RepeatKeepFlag = true ;
						state.RepeatWakeTime = Time.realtimeSinceStartup ;
						state.RepeatLoopTime = Time.realtimeSinceStartup ;

						state.IsDown = true ;
					}
					else
					{
						// リピート最中
						if( ( Time.realtimeSinceStartup - state.RepeatWakeTime ) >= RepeatStartingTime )
						{
							// リピート中
							if( ( Time.realtimeSinceStartup - state.RepeatLoopTime ) >= RepeatIntervalTime )
							{
								state.IsRepeat = true ;
								
								state.RepeatLoopTime = Time.realtimeSinceStartup ;
							}
						}
					}
				}
				else
				{
					// リピート解除
					if( state.RepeatKeepFlag == true )
					{
						state.IsUp = true ;

						state.RepeatKeepFlag  = false ;
					}
				}
			}

			/// <summary>
			/// アクシスの状態更新を行う(アクセス禁止)
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <param name="axisIndex"></param>
			/// <param name="axis"></param>
			public void UpdateAxisStates( int axisIndex, Vector2 axis, int slotNumber )
			{
				AxisState state = m_AxisStates[ axisIndex, slotNumber ] ;

				//---------------------------------

				if( axis.x >=     AnalogToDigitalThreshold   )
				{
					axis.x  =  1 ;
				}
				else
				if( axis.x <= ( - AnalogToDigitalThreshold ) )
				{
					axis.x  = -1 ;
				}
				else
				{
					axis.x  =  0 ;
				}

				if( axis.y >=     AnalogToDigitalThreshold   )
				{
					axis.y  =  1 ;
				}
				else
				if( axis.y <= ( - AnalogToDigitalThreshold ) )
				{
					axis.y  = -1 ;
				}
				else
				{
					axis.y  =  0 ;
				}

				state.IsRepeat	= Vector2.zero ;
				state.IsDown	= Vector2.zero ;
				state.IsUp		= Vector2.zero ;

				if( axis.x != 0 || axis.y != 0 )
				{
					if( state.RepeatKeepFlag == false )
					{
						// ホールド開始
						state.IsRepeat = axis ;

						state.RepeatKeepFlag = true ;
						state.RepeatKeepData = axis ;
						state.RepeatWakeTime = Time.realtimeSinceStartup ;
						state.RepeatLoopTime = Time.realtimeSinceStartup ;

						state.IsDown = axis ;
					}
					else
					{
						// ホールド最中
						if( ( Time.realtimeSinceStartup - state.RepeatWakeTime ) >= RepeatStartingTime )
						{
							// リピート中
							if( ( Time.realtimeSinceStartup - state.RepeatLoopTime ) >= RepeatIntervalTime )
							{
								state.IsRepeat = axis ;
	
								state.RepeatLoopTime = Time.realtimeSinceStartup ;
							}
						}
					}
				}
				else
				{
					// ホールド解除
					if( state.RepeatKeepFlag == true )
					{
						state.IsUp = state.RepeatKeepData ;

						state.RepeatKeepFlag = false ;
						state.RepeatKeepData = Vector2.zero ;
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 公開メソッド


			//----------------------------------------------------------
			// 互換メソッド


			//----------------------------------------------------------
			// 独自メソッド

			//--------------
			// ボタン関連

			/// <summary>
			/// ボタンが押されたかどうか判定する
			/// </summary>
			/// <param name="buttonIndex"></param>
			/// <returns></returns>
			public bool GetButtonDown( int buttonIndex, int slotNumber )
			{
				return m_ButtonStates[ buttonIndex, slotNumber ].IsDown ;
			}

			/// <summary>
			/// ボタンが離されたかどうか判定する
			/// </summary>
			/// <param name="buttonIndex"></param>
			/// <param name="isFixed"></param>
			/// <returns></returns>
			public bool GetButtonUp( int buttonIndex, int slotNumber )
			{
				return m_ButtonStates[ buttonIndex, slotNumber ].IsUp ;
			}

			/// <summary>
			/// ボタンが押されているかどうか判定する(リピート有効)
			/// </summary>
			/// <param name="buttonIndex"></param>
			/// <param name="isFixed"></param>
			/// <returns></returns>
			public bool GetRepeatButton( int buttonIndex, int slotNumber )
			{
				return m_ButtonStates[ buttonIndex, slotNumber ].IsRepeat ;
			}

			//--------------
			// アクシス関連

			/// <summary>
			/// アクシス(デジタル扱い)が押されたかどうか判定する
			/// </summary>
			/// <param name="axisIndex"></param>
			/// <param name="isFixed"></param>
			/// <returns></returns>
			public Vector2 GetAxisDown( int axisIndex, int slotNumber )
			{
				return m_AxisStates[ axisIndex, slotNumber ].IsDown ;
			}

			/// <summary>
			/// アクシス(デジタル扱い)が離されたどうか判定する
			/// </summary>
			/// <param name="axisIndex"></param>
			/// <param name="isFixed"></param>
			/// <returns></returns>
			public Vector2 GetAxisUp( int axisIndex, int slotNumber )
			{
				return m_AxisStates[ axisIndex, slotNumber ].IsUp ;
			}

			/// <summary>
			/// アクシス(デジタル扱い)が押されたているかどうか判定する(リピート有効)
			/// </summary>
			/// <param name="axisIndex"></param>
			/// <param name="isFixed"></param>
			/// <returns></returns>
			public Vector2 GetRepeatAxis( int axisIndex, int slotNumber )
			{
				return m_AxisStates[ axisIndex, slotNumber ].IsRepeat ;
			}

			//--------------
			// 振動関連

			/// <summary>
			/// ハプティクスの状態更新を行う
			/// </summary>
			public void UpdateHapticsState()
			{
				if( m_IsHapticsMoving == true && m_HapticsDuration >  0 )
				{
					float deltaTime = Time.realtimeSinceStartup - m_HapticsWakeTime ;
					if( deltaTime >  m_HapticsDuration )
					{
						// 停止させる
						StopMotor( Number ) ;
					}
				}
			}

			/// <summary>
			/// 振動時間を設定する
			/// </summary>
			/// <param name="duration"></param>
			public void SetHapticsState( bool isMoving, float duration )
			{
				if( isMoving == true )
				{
					m_IsHapticsMoving = true ;
					m_HapticsDuration = duration ;
					m_HapticsWakeTime = Time.realtimeSinceStartup ;
				}
				else
				{
					m_IsHapticsMoving = false ;
					m_HapticsDuration = 0 ;
					m_HapticsWakeTime = 0 ;
				}
			}
		}

		//-----------------------------------

		// プレイヤー群
		private static readonly List<Player> m_Players = new () ;


		//-------------------------------------------------------------------------------------------
		// 公開メソッド

		/// <summary>
		/// プレイヤーごとのプロフィール番号を設定する(初期はデフォルト＝－１が設定されている)
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <param name="profileNumber"></param>
		/// <returns></returns>
		public static bool SetProfileNumber( int playerNumber, int profileNumber )
		{
			if( playerNumber <  0 || playerNumber >= MaximumNumberOfPlayers || profileNumber <  0 )
			{
				return false ;
			}

			m_Players[ playerNumber ].ProfileNumber = profileNumber ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 実装インターフェース
		/// </summary>
		public interface IImplementation
		{
			//----------------------------------------------------------
			// 互換メソッド

			/// <summary>
			/// 接続中のゲームパッドの数
			/// </summary>
			int NumberOfGamePads{ get ; }

			/// <summary>
			/// 接続中のゲームパッドの名前を取得する
			/// </summary>
			/// <returns></returns>
			string[] GetJoystickNames() ;

			/// <summary>
			/// ボタンが押されているかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			bool GetButton( string buttonName ) ;

			/// <summary>
			/// ボタンが押されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			bool GetButtonDown( string buttonName ) ;

			/// <summary>
			/// ボタンが離されたかどうかの判定
			/// </summary>
			/// <param name="buttonName"></param>
			/// <returns></returns>
			bool GetButtonUp( string buttonName ) ;

			/// <summary>
			/// アクシスの状態を所得
			/// </summary>
			/// <param name="axisName"></param>
			/// <returns></returns>
			float GetAxis( string axisName ) ;

			//----------------------------------------------------------
			// 独自メソッド

			//--------------
			// ボタン関連

			/// <summary>
			/// 全てのボタンの状態を取得する
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <param name="profileNumber"></param>
			/// <returns></returns>
			int GetButtonAll( int playerNumber = -1 ) ;

			/// <summary>
			/// ボタンの状態を取得する
			/// </summary>
			/// <param name="buttonIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <param name="profileNumber"></param>
			/// <returns></returns>
			bool GetButton( int buttonIdentity, int playerNumber = -1 ) ;

			//--------------
			// アクシス関連

			/// <summary>
			/// アクシスの状態を取得する
			/// </summary>
			/// <param name="axisIdentity"></param>
			/// <param name="playerNumber"></param>
			/// <param name="profileNumber"></param>
			/// <returns></returns>
			Vector2 GetAxis( int axisIdentity, int playerNumber = -1 ) ;

			//--------------
			// 振動関連

			/// <summary>
			/// 振動を開始させる(範囲は 0～1)
			/// </summary>
			/// <param name="lowerSpeed"></param>
			/// <param name="upperSpeed"></param>
			/// <param name="duration"></param>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			bool SetMotorSpeeds( float lowerSpeed, float upperSpeed, float duration = 1.0f, int playerNumber = -1 ) ;

			/// <summary>
			/// 振動を停止させる
			/// </summary>
			/// <param name="playerNumber"></param>
			/// <returns></returns>
			bool StopMotor( int playerNumber = -1 ) ;

			/// <summary>
			/// 振動を一時停止させる
			/// </summary>
			/// <returns></returns>
			bool PauseHaptics() ;

			/// <summary>
			/// 振動を再開させる
			/// </summary>
			/// <returns></returns>
			bool ResumeHaptics() ;

			/// <summary>
			/// 振動を停止させる(パラメータもリセットされる)
			/// </summary>
			/// <returns></returns>
			bool ResetHaptics() ;
		}

		// 実装のインスタンス
		private static IImplementation m_Implementation ;

		//-------------------------------------------------------------------------------------------
		// 共通

		// ゲームパッドの各ボタンへのキーボードのキーマッピング(デフォルト)
		private static readonly Dictionary<int,KeyCodes[]> m_MappingKeyboardToButton = new ()
		{
			{ GamePad.B1, new KeyCodes[]{ KeyCodes.Z,			KeyCodes.Comma,		KeyCodes.Less		} },
			{ GamePad.B2, new KeyCodes[]{ KeyCodes.X,			KeyCodes.Period,	KeyCodes.Greater	} },
			{ GamePad.B3, new KeyCodes[]{ KeyCodes.C,			KeyCodes.Slash,		KeyCodes.Question	} },
			{ GamePad.B4, new KeyCodes[]{ KeyCodes.V,			KeyCodes.Backslash,	KeyCodes.Underscore	} },

			{ GamePad.R1, new KeyCodes[]{ KeyCodes.E,			KeyCodes.Keypad9						} },
			{ GamePad.L1, new KeyCodes[]{ KeyCodes.Q,			KeyCodes.Keypad7						} },
			{ GamePad.R2, new KeyCodes[]{ KeyCodes.RightShift,	KeyCodes.Keypad3						} },
			{ GamePad.L2, new KeyCodes[]{ KeyCodes.LeftShift,	KeyCodes.Keypad1						} },
			{ GamePad.R3, new KeyCodes[]{ KeyCodes.RightControl											} },
			{ GamePad.L3, new KeyCodes[]{ KeyCodes.LeftControl											} },

			{ GamePad.O1, new KeyCodes[]{ KeyCodes.Insert												} },
			{ GamePad.O2, new KeyCodes[]{ KeyCodes.Delete												} },
			{ GamePad.O3, new KeyCodes[]{ KeyCodes.Home													} },
			{ GamePad.O4, new KeyCodes[]{ KeyCodes.End													} },
		} ;

		/// <summary>
		/// ボタンへの任意のキー群のマッピングを行う
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static bool SetMappingKeyboardToButton( int buttonIdentity, params KeyCodes[] keyCodes )
		{
			if( m_MappingKeyboardToButton.ContainsKey( buttonIdentity ) == false )
			{
				// ボタン番号が不正
				return false ;
			}

			//----------------------------------

			if( keyCodes == null || keyCodes.Length == 0 )
			{
				m_MappingKeyboardToButton[ buttonIdentity ] = null ;
			}
			else
			{
				m_MappingKeyboardToButton[ buttonIdentity ] = keyCodes ;
			}

			return true ;
		}

		// ゲームパッドのボタンにマッピングされたキーボードのキーが押されているか判定する
		private static bool GetButtonByMappingKey( int buttonIdentity )
		{
			var keyCodes = m_MappingKeyboardToButton[ buttonIdentity ] ;

			if( keyCodes == null || keyCodes.Length == 0 || Enabled == false )
			{
				// 無効
				return false ;
			}

			foreach( var keyCode in keyCodes )
			{
				if( Keyboard.GetKey( keyCode ) == true )
				{
					// 押されている
					return true ;
				}
			}

			// 押されていない
			return false ;
		}

		//-------------------------------------------------------------------------------------------

		// ゲームパッドの各アクシス方法へのキーボードのキーマッピング(デフォルト)
		private static readonly Dictionary<( int, int ),KeyCodes[]> m_MappingKeyboardToAxisDirection = new ()
		{
			{ ( 0, 0 ), null },	// →
			{ ( 0, 1 ), null },	// ←
			{ ( 0, 2 ), null },	// ↑
			{ ( 0, 3 ), null },	// ↓

			{ ( 1, 0 ), null },	// →
			{ ( 1, 1 ), null },	// ←
			{ ( 1, 2 ), null },	// ↑
			{ ( 1, 3 ), null },	// ↓

			{ ( 2, 0 ), null },	// →
			{ ( 2, 1 ), null },	// ←
			{ ( 2, 2 ), null },	// ↑
			{ ( 2, 3 ), null },	// ↓

			{ ( 3, 0 ), null },	// R3
			{ ( 3, 1 ), null },	// L3
			{ ( 3, 2 ), null },	//
			{ ( 3, 3 ), null },	//
		} ;

		/// <summary>
		/// ボタンへの任意のキー群のマッピングを行う
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static bool SetMappingKeyboardToAxisDirection( int axisIdentity, int axisDirection, params KeyCodes[] keyCodes )
		{
			var key = ( axisIdentity, axisDirection ) ;

			if( m_MappingKeyboardToAxisDirection.ContainsKey( key ) == false )
			{
				// ボタン番号が不正
				return false ;
			}

			//----------------------------------

			if( keyCodes == null || keyCodes.Length == 0 )
			{
				m_MappingKeyboardToAxisDirection[ key ] = null ;
			}
			else
			{
				m_MappingKeyboardToAxisDirection[ key ] = keyCodes ;
			}

			return true ;
		}

		// ゲームパッドのアクシス方向にマッピングされたキーボードのキーが押されているか判定する
		private static bool GetAxisDirectionByMappingKey( int axisIdentity, int axisDirection )
		{
			var key = ( axisIdentity, axisDirection ) ;

			var keyCodes = m_MappingKeyboardToAxisDirection[ key ] ;

			if( keyCodes == null || keyCodes.Length == 0 || Enabled == false )
			{
				// 無効
				return false ;
			}

			foreach( var keyCode in keyCodes )
			{
				if( Keyboard.GetKey( keyCode ) == true )
				{
					// 押されている
					return true ;
				}
			}

			// 押されていない
			return false ;
		}

		// ナンバーキーのアクシスへの反映
		private static void GetAxisByMappingKey_Custom( int axisIdentity, ref float oAxisX, ref float oAxisY )
		{
			//----------------------------------

			// SX
			if( oAxisX == 0 )
			{
				if( GetAxisDirectionByMappingKey( axisIdentity, 0 ) == true )
				{
					oAxisX = +1 ;
				}
				if( GetAxisDirectionByMappingKey( axisIdentity, 1 ) == true )
				{
					oAxisX = -1 ;
				}
			}

			// SY
			if( oAxisY == 0 )
			{
				if( GetAxisDirectionByMappingKey( axisIdentity, 2 ) == true )
				{
					oAxisY = +1 ;
				}
				if( GetAxisDirectionByMappingKey( axisIdentity, 3 ) == true )
				{
					oAxisY = -1 ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		private static int[] m_MappingKeyboardToAxis_WASD = new int[]{ 0, 1, 2 } ;

		/// <summary>
		/// ＷＡＳＤキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_WASD( params int[] axisIdentities )
		{
			if( axisIdentities == null || axisIdentities.Length == 0 )
			{
				m_MappingKeyboardToAxis_WASD = null ;
			}
			else
			{
				m_MappingKeyboardToAxis_WASD = axisIdentities ;
			}
		}

		// ＷＡＳＤキーのアクシスへの反映
		private static void GetAxisByMappingKey_WASD( int axisIdentity, ref float oAxisX, ref float oAxisY )
		{
			if( m_MappingKeyboardToAxis_WASD == null || m_MappingKeyboardToAxis_WASD.Length == 0 || Enabled == false )
			{
				// 無し
				return ;
			}

			if( m_MappingKeyboardToAxis_WASD.Contains( axisIdentity ) == false )
			{
				// 無し
				return ;
			}

			//----------------------------------

			// SCX
			if( oAxisX == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.D ) == true )
				{
					oAxisX = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.A ) == true )
				{
					oAxisX = -1 ;
				}
			}

			// SCY
			if( oAxisY == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.W ) == true )
				{
					oAxisY = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.S ) == true )
				{
					oAxisY = -1 ;
				}
			}
		}

		//-----------------------------------

		private static int[] m_MappingKeyboardToAxis_Cursor = new int[]{ 0, 1, 2 } ;

		/// <summary>
		/// カーソルキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_Cursor( params int[] axisIdentities )
		{
			if( axisIdentities == null || axisIdentities.Length == 0 )
			{
				m_MappingKeyboardToAxis_Cursor = null ;
			}
			else
			{
				m_MappingKeyboardToAxis_Cursor = axisIdentities ;
			}
		}

		// カーソルキーのアクシスへの反映
		private static void GetAxisByMappingKey_Cursor( int axisIdentity, ref float oAxisX, ref float oAxisY )
		{
			if( m_MappingKeyboardToAxis_Cursor == null || m_MappingKeyboardToAxis_Cursor.Length == 0 || Enabled == false )
			{
				// 無し
				return ;
			}

			if( m_MappingKeyboardToAxis_Cursor.Contains( axisIdentity ) == false )
			{
				// 無し
				return ;
			}

			//----------------------------------

			// SCX
			if( oAxisX == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.RightArrow ) == true )
				{
					oAxisX = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.LeftArrow ) == true )
				{
					oAxisX = -1 ;
				}
			}

			// SCY
			if( oAxisY == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.UpArrow ) == true )
				{
					oAxisY = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.DownArrow ) == true )
				{
					oAxisY = -1 ;
				}
			}
		}

		//-----------------------------------

		private static int[] m_MappingKeyboardToAxis_Number = new int[]{ 0, 1, 2 } ;

		/// <summary>
		/// ナンバーキーのアクシスへの割り当てを設定する
		/// </summary>
		/// <param name="axisNumbers"></param>
		public static void SetMappingKeyboardToAxis_Number( params int[] axisIdentities )
		{
			if( axisIdentities == null || axisIdentities.Length == 0 )
			{
				m_MappingKeyboardToAxis_Number = null ;
			}
			else
			{
				m_MappingKeyboardToAxis_Number = axisIdentities ;
			}
		}

		// ナンバーキーのアクシスへの反映
		private static void GetAxisByMappingKey_Number( int axisIdentity, ref float oAxisX, ref float oAxisY )
		{
			if( m_MappingKeyboardToAxis_Number == null || m_MappingKeyboardToAxis_Number.Length == 0 || Enabled == false )
			{
				// 無し
				return ;
			}

			if( m_MappingKeyboardToAxis_Number.Contains( axisIdentity ) == false )
			{
				// 無し
				return ;
			}

			//----------------------------------

			// SCX
			if( oAxisX == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.Keypad6 ) == true )
				{
					oAxisX = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.Keypad4 ) == true )
				{
					oAxisX = -1 ;
				}
			}

			// SCY
			if( oAxisY == 0 )
			{
				if( Keyboard.GetKey( KeyCodes.Keypad8 ) == true )
				{
					oAxisY = +1 ;
				}
				if( Keyboard.GetKey( KeyCodes.Keypad2 ) == true )
				{
					oAxisY = -1 ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetNames()
			=> GetJoystickNames() ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Repeat 系の状態更新
		/// </summary>
		public static void Update( bool isFixed )
		{
			int playerNumber ;
			int buttonIndex, axisIndex ;
			int buttonFlags ;

			int buttonIndexMax	= Math.Min( MaximumNumberOfButtons,	m_ButtonIdentities.Length	) ;
			int axisIndexMax	= Math.Min( MaximumNumberOfAxes,	m_AxisIdentities.Length		) ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( playerNumber  = 0 ; playerNumber <  max ; playerNumber ++ )
			{
				// プレイヤー情報
				var player = m_Players[ playerNumber ] ;

				// Button
				buttonFlags = GetButtonAll( playerNumber ) ;	// ボタンの押下状態(ビット単位のフラグ)
				for( buttonIndex  = 0 ; buttonIndex <  buttonIndexMax  ; buttonIndex ++ )
				{
					player.UpdateButtonStates( buttonIndex, m_ButtonIdentities[ buttonIndex ], buttonFlags, slotNumber ) ;
				}

				// Axis
				for( axisIndex  = 0 ; axisIndex <  axisIndexMax ; axisIndex ++ )
				{
					player.UpdateAxisStates( axisIndex, GetAxis( m_AxisIdentities[ axisIndex ], playerNumber ), slotNumber ) ;
				}

				// Haptics
				player.UpdateHapticsState() ;
			}
		}

		//-------------------------------------------------------------------------------------------------------------------
		// 公開メソッド

		//-----------------------------------------------------------
		// 互換メソッド

		//---------------
		// 情報関連

		/// <summary>
		/// 接続中のゲームパッドの数
		/// </summary>
		public static int NumberOfGamePads
		{
			get
			{
				if( m_Implementation == null )
				{
					throw new Exception( "Not implemented." ) ;
				}
				return m_Implementation.NumberOfGamePads ;
			}
		}

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetJoystickNames()
		{
			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetJoystickNames() ;
		}

		//---------------
		// ボタン関連

		/// <summary>
		/// ボタンが押されているかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButton( string buttonName )
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
			return m_Implementation.GetButton( buttonName ) ;
		}

		/// <summary>
		/// ボタンが押されたかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButtonDown( string buttonName )
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
			return m_Implementation.GetButtonDown( buttonName ) ;
		}

		/// <summary>
		/// ボタンが離されたかどうかの判定
		/// </summary>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		public static bool GetButtonUp( string buttonName )
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
			return m_Implementation.GetButtonUp( buttonName ) ;
		}

		//---------------
		// アクシス関連

		/// <summary>
		/// アクシスの状態を取得
		/// </summary>
		/// <param name="axisName"></param>
		/// <returns></returns>
		public static float GetAxis( string axisName )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return 0 ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetAxis( axisName ) ;
		}

		//-----------------------------------------------------------
		// 独自メソッド

		//---------------
		// ボタン関連

		/// <summary>
		/// 全てのボタンが押されているかどうか判定する
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static int GetButtonAll( int playerNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return 0 ;
			}

			if( m_Implementation == null )
			{
				throw new Exception( "Not implemented." ) ;
			}
			return m_Implementation.GetButtonAll( playerNumber ) ;
		}

		/// <summary>
		/// ボタンが押されているかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int buttonIdentity, int playerNumber = -1 )
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
			return m_Implementation.GetButton( buttonIdentity, playerNumber ) ;
		}

		/// <summary>
		/// ボタンが押されたかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonDown( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			//----------------------------------

			int buttonIndex = m_ButtonIdentityToIndex[ buttonIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				if( m_Players[ p ].GetButtonDown( buttonIndex, slotNumber ) == true )
				{
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// ボタンが離されたかどうか判定する
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonUp( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			//----------------------------------

			int buttonIndex = m_ButtonIdentityToIndex[ buttonIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				if( m_Players[ p ].GetButtonUp( buttonIndex, slotNumber ) == true )
				{
					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// ボタンが押されているかどうか判定する(リピート有効)
		/// </summary>
		/// <param name="buttonIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool GetButtonRepeat( int buttonIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return false ;
			}

			//----------------------------------

			int buttonIndex = m_ButtonIdentityToIndex[ buttonIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				if( m_Players[ p ].GetRepeatButton( buttonIndex, slotNumber ) == true )
				{
					return true ;
				}
			}

			return false ;
		}

		//---------------
		// アクシス関連

		/// <summary>
		/// アクシスが押されているかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxis( int axisIdentity, int playerNumber = -1 )
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
			return m_Implementation.GetAxis( axisIdentity, playerNumber ) ;
		}

		/// <summary>
		/// アクシス(デジタル扱い)が押されたかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisDown( int axisIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			//----------------------------------

			int axisIndex = m_AxisIdentityToIndex[ axisIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			Vector2 oAxis = Vector2.zero ;

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				var axis = m_Players[ p ].GetAxisDown( axisIndex, slotNumber ) ;
				if( axis.x != 0 )
				{
					oAxis.x = axis.x ;
				}
				if( axis.y != 0 )
				{
					oAxis.y = axis.y ;
				}
			}

			return oAxis ;
		}

		/// <summary>
		/// アクシス(デジタル扱い)が離されたかどうか判定する
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisUp( int axisIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			//----------------------------------

			int axisIndex = m_AxisIdentityToIndex[ axisIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			Vector2 oAxis = Vector2.zero ;

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				var axis = m_Players[ p ].GetAxisUp( axisIndex, slotNumber ) ;
				if( axis.x != 0 )
				{
					oAxis.x = axis.x ;
				}
				if( axis.y != 0 )
				{
					oAxis.y = axis.y ;
				}
			}

			return oAxis ;
		}

		/// <summary>
		/// アクシス(デジタル扱い)が押されているかどうか判定する(リピート有効)
		/// </summary>
		/// <param name="axisIdentity"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisRepeat( int axisIdentity, int playerNumber = -1, bool isFixed = false )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( m_Owner == null || m_Owner.ControlEnabled == false || Enabled == false )
			{
				// 無効
				return Vector2.zero ;
			}

			//----------------------------------

			int axisIndex = m_AxisIdentityToIndex[ axisIdentity ] ;

			int p, ps, pe ;

			// 接続しているしているプレイヤー(最大４)しかし０ならば１にする
			int max = Math.Max( Math.Min( NumberOfGamePads, MaximumNumberOfPlayers ), 1 ) ;
			
			if( playerNumber <  0 || playerNumber >= max )
			{
				// 全プレイヤーで判定
				ps = 0 ;
				pe = max - 1 ;
			}
			else
			{
				// 各プレイヤーで判定
				ps = playerNumber ;
				pe = playerNumber ;
			}

			Vector2 oAxis = Vector2.zero ;

			int slotNumber = ( isFixed == false ? 0 : 1 ) ;

			for( p  = ps ; p <= pe ; p ++ )
			{
				var axis = m_Players[ p ].GetRepeatAxis( axisIndex, slotNumber ) ;
				if( axis.x != 0 )
				{
					oAxis.x = axis.x ;
				}
				if( axis.y != 0 )
				{
					oAxis.y = axis.y ;
				}
			}

			return oAxis ;
		}

		//---------------
		// 振動関連

		/// <summary>
		/// 振動を開始させる(範囲は 0～1)
		/// </summary>
		/// <param name="lowerSpeed"></param>
		/// <param name="upperSpeed"></param>
		/// <param name="duration"></param>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool SetMotorSpeeds( float lowerSpeed, float upperSpeed, float duration = 1.0f, int playerNumber = -1 )
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
			return m_Implementation.SetMotorSpeeds( lowerSpeed, upperSpeed, duration, playerNumber ) ;
		}

		/// <summary>
		/// 振動を停止させる
		/// </summary>
		/// <param name="playerNumber"></param>
		/// <returns></returns>
		public static bool StopMotor( int playerNumber = -1 )
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
			return m_Implementation.StopMotor( playerNumber ) ;
		}

		/// <summary>
		/// 振動を一時停止させる
		/// </summary>
		/// <returns></returns>
		public static bool PauseHaptics()
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
			return m_Implementation.PauseHaptics() ;
		}

		/// <summary>
		/// 振動を再開させる
		/// </summary>
		/// <returns></returns>
		public static bool ResumeHaptics()
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
			return m_Implementation.ResumeHaptics() ;
		}

		/// <summary>
		/// 振動を停止させる(パラメータもリセットされる)
		/// </summary>
		/// <returns></returns>
		public static bool ResetHaptics()
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
			return m_Implementation.ResetHaptics() ;
		}

		//-------------------------------------------------------------------------------------------------------------------
		// ボタン・アクシスの定義

		/// <summary>
		/// プレイヤー１ボタン００識別名
		/// </summary>
		public const string Player1Button00 = "Player_1_Button_00" ;

		/// <summary>
		/// プレイヤー１ボタン０１識別名
		/// </summary>
		public const string Player1Button01 = "Player_1_Button_01" ;

		/// <summary>
		/// プレイヤー１ボタン０２識別名
		/// </summary>
		public const string Player1Button02 = "Player_1_Button_02" ;

		/// <summary>
		/// プレイヤー１ボタン０３識別名
		/// </summary>
		public const string Player1Button03 = "Player_1_Button_03" ;

		/// <summary>
		/// プレイヤー１ボタン０４識別名
		/// </summary>
		public const string Player1Button04 = "Player_1_Button_04" ;

		/// <summary>
		/// プレイヤー１ボタン０５識別名
		/// </summary>
		public const string Player1Button05 = "Player_1_Button_05" ;

		/// <summary>
		/// プレイヤー１ボタン０６識別名
		/// </summary>
		public const string Player1Button06 = "Player_1_Button_06" ;

		/// <summary>
		/// プレイヤー１ボタン０７識別名
		/// </summary>
		public const string Player1Button07 = "Player_1_Button_07" ;

		/// <summary>
		/// プレイヤー１ボタン０８識別名
		/// </summary>
		public const string Player1Button08 = "Player_1_Button_08" ;

		/// <summary>
		/// プレイヤー１ボタン０９識別名
		/// </summary>
		public const string Player1Button09 = "Player_1_Button_09" ;

		/// <summary>
		/// プレイヤー１ボタン１０識別名
		/// </summary>
		public const string Player1Button10 = "Player_1_Button_10" ;

		/// <summary>
		/// プレイヤー１ボタン１１識別名
		/// </summary>
		public const string Player1Button11 = "Player_1_Button_11" ;

		/// <summary>
		/// プレイヤー１ボタン１２識別名
		/// </summary>
		public const string Player1Button12 = "Player_1_Button_12" ;

		/// <summary>
		/// プレイヤー１ボタン１３識別名
		/// </summary>
		public const string Player1Button13 = "Player_1_Button_13" ;

		/// <summary>
		/// プレイヤー１ボタン１４識別名
		/// </summary>
		public const string Player1Button14 = "Player_1_Button_14" ;

		/// <summary>
		/// プレイヤー１ボタン１５識別名
		/// </summary>
		public const string Player1Button15 = "Player_1_Button_15" ;

		//---------------

		/// <summary>
		/// プレイヤー２ボタン００識別名
		/// </summary>
		public const string Player2Button00 = "Player_2_Button_00" ;

		/// <summary>
		/// プレイヤー２ボタン０１識別名
		/// </summary>
		public const string Player2Button01 = "Player_2_Button_01" ;

		/// <summary>
		/// プレイヤー２ボタン０２識別名
		/// </summary>
		public const string Player2Button02 = "Player_2_Button_02" ;

		/// <summary>
		/// プレイヤー２ボタン０３識別名
		/// </summary>
		public const string Player2Button03 = "Player_2_Button_03" ;

		/// <summary>
		/// プレイヤー２ボタン０４識別名
		/// </summary>
		public const string Player2Button04 = "Player_2_Button_04" ;

		/// <summary>
		/// プレイヤー２ボタン０５識別名
		/// </summary>
		public const string Player2Button05 = "Player_2_Button_05" ;

		/// <summary>
		/// プレイヤー２ボタン０６識別名
		/// </summary>
		public const string Player2Button06 = "Player_2_Button_06" ;

		/// <summary>
		/// プレイヤー２ボタン０７識別名
		/// </summary>
		public const string Player2Button07 = "Player_2_Button_07" ;

		/// <summary>
		/// プレイヤー２ボタン０８識別名
		/// </summary>
		public const string Player2Button08 = "Player_2_Button_08" ;

		/// <summary>
		/// プレイヤー２ボタン０９識別名
		/// </summary>
		public const string Player2Button09 = "Player_2_Button_09" ;

		/// <summary>
		/// プレイヤー２ボタン１０識別名
		/// </summary>
		public const string Player2Button10 = "Player_2_Button_10" ;

		/// <summary>
		/// プレイヤー２ボタン１１識別名
		/// </summary>
		public const string Player2Button11 = "Player_2_Button_11" ;

		/// <summary>
		/// プレイヤー２ボタン１２識別名
		/// </summary>
		public const string Player2Button12 = "Player_2_Button_12" ;

		/// <summary>
		/// プレイヤー２ボタン１３識別名
		/// </summary>
		public const string Player2Button13 = "Player_2_Button_13" ;

		/// <summary>
		/// プレイヤー２ボタン１４識別名
		/// </summary>
		public const string Player2Button14 = "Player_2_Button_14" ;

		/// <summary>
		/// プレイヤー２ボタン１５識別名
		/// </summary>
		public const string Player2Button15 = "Player_2_Button_15" ;

		//---------------

		/// <summary>
		/// プレイヤー３ボタン００識別名
		/// </summary>
		public const string Player3Button00 = "Player_3_Button_00" ;

		/// <summary>
		/// プレイヤー３ボタン０１識別名
		/// </summary>
		public const string Player3Button01 = "Player_3_Button_01" ;

		/// <summary>
		/// プレイヤー３ボタン０２識別名
		/// </summary>
		public const string Player3Button02 = "Player_3_Button_02" ;

		/// <summary>
		/// プレイヤー３ボタン０３識別名
		/// </summary>
		public const string Player3Button03 = "Player_3_Button_03" ;

		/// <summary>
		/// プレイヤー３ボタン０４識別名
		/// </summary>
		public const string Player3Button04 = "Player_3_Button_04" ;

		/// <summary>
		/// プレイヤー３ボタン０５識別名
		/// </summary>
		public const string Player3Button05 = "Player_3_Button_05" ;

		/// <summary>
		/// プレイヤー３ボタン０６識別名
		/// </summary>
		public const string Player3Button06 = "Player_3_Button_06" ;

		/// <summary>
		/// プレイヤー３ボタン０７識別名
		/// </summary>
		public const string Player3Button07 = "Player_3_Button_07" ;

		/// <summary>
		/// プレイヤー３ボタン０８識別名
		/// </summary>
		public const string Player3Button08 = "Player_3_Button_08" ;

		/// <summary>
		/// プレイヤー３ボタン０９識別名
		/// </summary>
		public const string Player3Button09 = "Player_3_Button_09" ;

		/// <summary>
		/// プレイヤー３ボタン１０識別名
		/// </summary>
		public const string Player3Button10 = "Player_3_Button_10" ;

		/// <summary>
		/// プレイヤー３ボタン１１識別名
		/// </summary>
		public const string Player3Button11 = "Player_3_Button_11" ;

		/// <summary>
		/// プレイヤー３ボタン１２識別名
		/// </summary>
		public const string Player3Button12 = "Player_3_Button_12" ;

		/// <summary>
		/// プレイヤー３ボタン１３識別名
		/// </summary>
		public const string Player3Button13 = "Player_3_Button_13" ;

		/// <summary>
		/// プレイヤー３ボタン１４識別名
		/// </summary>
		public const string Player3Button14 = "Player_3_Button_14" ;

		/// <summary>
		/// プレイヤー３ボタン１５識別名
		/// </summary>
		public const string Player3Button15 = "Player_3_Button_15" ;

		//---------------

		/// <summary>
		/// プレイヤー4ボタン００識別名
		/// </summary>
		public const string Player4Button00 = "Player_4_Button_00" ;

		/// <summary>
		/// プレイヤー４ボタン０１識別名
		/// </summary>
		public const string Player4Button01 = "Player_4_Button_01" ;

		/// <summary>
		/// プレイヤー４ボタン０２識別名
		/// </summary>
		public const string Player4Button02 = "Player_4_Button_02" ;

		/// <summary>
		/// プレイヤー４ボタン０３識別名
		/// </summary>
		public const string Player4Button03 = "Player_4_Button_03" ;

		/// <summary>
		/// プレイヤー４ボタン０４識別名
		/// </summary>
		public const string Player4Button04 = "Player_4_Button_04" ;

		/// <summary>
		/// プレイヤー４ボタン０５識別名
		/// </summary>
		public const string Player4Button05 = "Player_4_Button_05" ;

		/// <summary>
		/// プレイヤー４ボタン０６識別名
		/// </summary>
		public const string Player4Button06 = "Player_4_Button_06" ;

		/// <summary>
		/// プレイヤー４ボタン０７識別名
		/// </summary>
		public const string Player4Button07 = "Player_4_Button_07" ;

		/// <summary>
		/// プレイヤー４ボタン０８識別名
		/// </summary>
		public const string Player4Button08 = "Player_4_Button_08" ;

		/// <summary>
		/// プレイヤー４ボタン０９識別名
		/// </summary>
		public const string Player4Button09 = "Player_4_Button_09" ;

		/// <summary>
		/// プレイヤー４ボタン１０識別名
		/// </summary>
		public const string Player4Button10 = "Player_4_Button_10" ;

		/// <summary>
		/// プレイヤー４ボタン１１識別名
		/// </summary>
		public const string Player4Button11 = "Player_4_Button_11" ;

		/// <summary>
		/// プレイヤー４ボタン１２識別名
		/// </summary>
		public const string Player4Button12 = "Player_4_Button_12" ;

		/// <summary>
		/// プレイヤー４ボタン１３識別名
		/// </summary>
		public const string Player4Button13 = "Player_4_Button_13" ;

		/// <summary>
		/// プレイヤー４ボタン１４識別名
		/// </summary>
		public const string Player4Button14 = "Player_4_Button_14" ;

		/// <summary>
		/// プレイヤー４ボタン１５識別名
		/// </summary>
		public const string Player4Button15 = "Player_4_Button_15" ;

		//-----------------------------------

		/// <summary>
		/// プレイヤー１アクシス００識別名
		/// </summary>
		public const string Player1Axis00 = "Player_1_Axis_00" ;

		/// <summary>
		/// プレイヤー１アクシス０１識別名
		/// </summary>
		public const string Player1Axis01 = "Player_1_Axis_01" ;

		/// <summary>
		/// プレイヤー１アクシス０２識別名
		/// </summary>
		public const string Player1Axis02 = "Player_1_Axis_02" ;

		/// <summary>
		/// プレイヤー１アクシス０３識別名
		/// </summary>
		public const string Player1Axis03 = "Player_1_Axis_03" ;

		/// <summary>
		/// プレイヤー１アクシス０４識別名
		/// </summary>
		public const string Player1Axis04 = "Player_1_Axis_04" ;

		/// <summary>
		/// プレイヤー１アクシス０５識別名
		/// </summary>
		public const string Player1Axis05 = "Player_1_Axis_05" ;

		/// <summary>
		/// プレイヤー１アクシス０６識別名
		/// </summary>
		public const string Player1Axis06 = "Player_1_Axis_06" ;

		/// <summary>
		/// プレイヤー１アクシス０７識別名
		/// </summary>
		public const string Player1Axis07 = "Player_1_Axis_07" ;

		/// <summary>
		/// プレイヤー１アクシス０８識別名
		/// </summary>
		public const string Player1Axis08 = "Player_1_Axis_08" ;

		/// <summary>
		/// プレイヤー１アクシス０９識別名
		/// </summary>
		public const string Player1Axis09 = "Player_1_Axis_09" ;

		/// <summary>
		/// プレイヤー１アクシス１０識別名
		/// </summary>
		public const string Player1Axis10 = "Player_1_Axis_10" ;

		/// <summary>
		/// プレイヤー１アクシス１１識別名
		/// </summary>
		public const string Player1Axis11 = "Player_1_Axis_11" ;

		/// <summary>
		/// プレイヤー１アクシス１２識別名
		/// </summary>
		public const string Player1Axis12 = "Player_1_Axis_12" ;

		/// <summary>
		/// プレイヤー１アクシス１３識別名
		/// </summary>
		public const string Player1Axis13 = "Player_1_Axis_13" ;

		/// <summary>
		/// プレイヤー１アクシス１４識別名
		/// </summary>
		public const string Player1Axis14 = "Player_1_Axis_14" ;

		/// <summary>
		/// プレイヤー１アクシス１５識別名
		/// </summary>
		public const string Player1Axis15 = "Player_1_Axis_15" ;

		//---------------

		/// <summary>
		/// プレイヤー２アクシス００識別名
		/// </summary>
		public const string Player2Axis00 = "Player_2_Axis_00" ;

		/// <summary>
		/// プレイヤー２アクシス０１識別名
		/// </summary>
		public const string Player2Axis01 = "Player_2_Axis_01" ;

		/// <summary>
		/// プレイヤー２アクシス０２識別名
		/// </summary>
		public const string Player2Axis02 = "Player_2_Axis_02" ;

		/// <summary>
		/// プレイヤー２アクシス０３識別名
		/// </summary>
		public const string Player2Axis03 = "Player_2_Axis_03" ;

		/// <summary>
		/// プレイヤー２アクシス０４識別名
		/// </summary>
		public const string Player2Axis04 = "Player_2_Axis_04" ;

		/// <summary>
		/// プレイヤー２アクシス０５識別名
		/// </summary>
		public const string Player2Axis05 = "Player_2_Axis_05" ;

		/// <summary>
		/// プレイヤー２アクシス０６識別名
		/// </summary>
		public const string Player2Axis06 = "Player_2_Axis_06" ;

		/// <summary>
		/// プレイヤー２アクシス０７識別名
		/// </summary>
		public const string Player2Axis07 = "Player_2_Axis_07" ;

		/// <summary>
		/// プレイヤー２アクシス０８識別名
		/// </summary>
		public const string Player2Axis08 = "Player_2_Axis_08" ;

		/// <summary>
		/// プレイヤー２アクシス０９識別名
		/// </summary>
		public const string Player2Axis09 = "Player_2_Axis_09" ;

		/// <summary>
		/// プレイヤー２アクシス１０識別名
		/// </summary>
		public const string Player2Axis10 = "Player_2_Axis_10" ;

		/// <summary>
		/// プレイヤー２アクシス１１識別名
		/// </summary>
		public const string Player2Axis11 = "Player_2_Axis_11" ;

		/// <summary>
		/// プレイヤー２アクシス１２識別名
		/// </summary>
		public const string Player2Axis12 = "Player_2_Axis_12" ;

		/// <summary>
		/// プレイヤー２アクシス１３識別名
		/// </summary>
		public const string Player2Axis13 = "Player_2_Axis_13" ;

		/// <summary>
		/// プレイヤー２アクシス１４識別名
		/// </summary>
		public const string Player2Axis14 = "Player_2_Axis_14" ;

		/// <summary>
		/// プレイヤー２アクシス１５識別名
		/// </summary>
		public const string Player2Axis15 = "Player_2_Axis_15" ;

		//---------------

		/// <summary>
		/// プレイヤー３アクシス００識別名
		/// </summary>
		public const string Player3Axis00 = "Player_3_Axis_00" ;

		/// <summary>
		/// プレイヤー３アクシス０１識別名
		/// </summary>
		public const string Player3Axis01 = "Player_3_Axis_01" ;

		/// <summary>
		/// プレイヤー３アクシス０２識別名
		/// </summary>
		public const string Player3Axis02 = "Player_3_Axis_02" ;

		/// <summary>
		/// プレイヤー３アクシス０３識別名
		/// </summary>
		public const string Player3Axis03 = "Player_3_Axis_03" ;

		/// <summary>
		/// プレイヤー３アクシス０４識別名
		/// </summary>
		public const string Player3Axis04 = "Player_3_Axis_04" ;

		/// <summary>
		/// プレイヤー３アクシス０５識別名
		/// </summary>
		public const string Player3Axis05 = "Player_3_Axis_05" ;

		/// <summary>
		/// プレイヤー３アクシス０６識別名
		/// </summary>
		public const string Player3Axis06 = "Player_3_Axis_06" ;

		/// <summary>
		/// プレイヤー３アクシス０７識別名
		/// </summary>
		public const string Player3Axis07 = "Player_3_Axis_07" ;

		/// <summary>
		/// プレイヤー３アクシス０８識別名
		/// </summary>
		public const string Player3Axis08 = "Player_3_Axis_08" ;

		/// <summary>
		/// プレイヤー３アクシス０９識別名
		/// </summary>
		public const string Player3Axis09 = "Player_3_Axis_09" ;

		/// <summary>
		/// プレイヤー３アクシス１０識別名
		/// </summary>
		public const string Player3Axis10 = "Player_3_Axis_10" ;

		/// <summary>
		/// プレイヤー３アクシス１１識別名
		/// </summary>
		public const string Player3Axis11 = "Player_3_Axis_11" ;

		/// <summary>
		/// プレイヤー３アクシス１２識別名
		/// </summary>
		public const string Player3Axis12 = "Player_3_Axis_12" ;

		/// <summary>
		/// プレイヤー３アクシス１３識別名
		/// </summary>
		public const string Player3Axis13 = "Player_3_Axis_13" ;

		/// <summary>
		/// プレイヤー３アクシス１４識別名
		/// </summary>
		public const string Player3Axis14 = "Player_3_Axis_14" ;

		/// <summary>
		/// プレイヤー３アクシス１５識別名
		/// </summary>
		public const string Player3Axis15 = "Player_3_Axis_15" ;

		//---------------

		/// <summary>
		/// プレイヤー４アクシス００識別名
		/// </summary>
		public const string Player4Axis00 = "Player_4_Axis_00" ;

		/// <summary>
		/// プレイヤー４アクシス０１識別名
		/// </summary>
		public const string Player4Axis01 = "Player_4_Axis_01" ;

		/// <summary>
		/// プレイヤー４アクシス０２識別名
		/// </summary>
		public const string Player4Axis02 = "Player_4_Axis_02" ;

		/// <summary>
		/// プレイヤー４アクシス０３識別名
		/// </summary>
		public const string Player4Axis03 = "Player_4_Axis_03" ;

		/// <summary>
		/// プレイヤー４アクシス０４識別名
		/// </summary>
		public const string Player4Axis04 = "Player_4_Axis_04" ;

		/// <summary>
		/// プレイヤー４アクシス０５識別名
		/// </summary>
		public const string Player4Axis05 = "Player_4_Axis_05" ;

		/// <summary>
		/// プレイヤー４アクシス０６識別名
		/// </summary>
		public const string Player4Axis06 = "Player_4_Axis_06" ;

		/// <summary>
		/// プレイヤー４アクシス０７識別名
		/// </summary>
		public const string Player4Axis07 = "Player_4_Axis_07" ;

		/// <summary>
		/// プレイヤー４アクシス０８識別名
		/// </summary>
		public const string Player4Axis08 = "Player_4_Axis_08" ;

		/// <summary>
		/// プレイヤー４アクシス０９識別名
		/// </summary>
		public const string Player4Axis09 = "Player_4_Axis_09" ;

		/// <summary>
		/// プレイヤー４アクシス１０識別名
		/// </summary>
		public const string Player4Axis10 = "Player_4_Axis_10" ;

		/// <summary>
		/// プレイヤー４アクシス１１識別名
		/// </summary>
		public const string Player4Axis11 = "Player_4_Axis_11" ;

		/// <summary>
		/// プレイヤー４アクシス１２識別名
		/// </summary>
		public const string Player4Axis12 = "Player_4_Axis_12" ;

		/// <summary>
		/// プレイヤー４アクシス１３識別名
		/// </summary>
		public const string Player4Axis13 = "Player_4_Axis_13" ;

		/// <summary>
		/// プレイヤー４アクシス１４識別名
		/// </summary>
		public const string Player4Axis14 = "Player_4_Axis_14" ;

		/// <summary>
		/// プレイヤー４アクシス１５識別名
		/// </summary>
		public const string Player4Axis15 = "Player_4_Axis_15" ;

		//-----------------------------------------------------------
		// Editor からの参照用

		/// <summary>
		/// ゲームパッドのボタンの識別名を取得する
		/// </summary>
		/// <param name="p"></param>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string GetButtonName( int p, int i )
		{
			return $"Player_{p}_Button_{i:D2}" ;
		}

		/// <summary>
		/// ゲームパッドのアクシスの識別名を取得する
		/// </summary>
		/// <param name="p"></param>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string GetAxisName( int p, int i )
		{
			return $"Player_{p}_Axis_{i:D2}" ;
		}

		//-------------------------------------------------------------------------------------------
		// 旧版でのみ使用する(ボタン番号・アクシス番号→ボタン名・アクシス名への変換用)

		// ボタン割り当て
		private static readonly string[][]	m_ButtonNames =
		{
			new string[]
			{
				Player1Button00, Player1Button01, Player1Button02, Player1Button03, Player1Button04, Player1Button05, Player1Button06, Player1Button07, Player1Button08, Player1Button09, Player1Button10, Player1Button11, Player1Button12, Player1Button13, Player1Button14, Player1Button15,
			},
			new string[]
			{
				Player2Button00, Player2Button01, Player2Button02, Player2Button03, Player2Button04, Player2Button05, Player2Button06, Player2Button07, Player2Button08, Player2Button09, Player2Button10, Player2Button11, Player2Button12, Player2Button13, Player2Button14, Player2Button15,
			},
			new string[]
			{
				Player3Button00, Player3Button01, Player3Button02, Player3Button03, Player3Button04, Player3Button05, Player3Button06, Player3Button07, Player3Button08, Player3Button09, Player3Button10, Player3Button11, Player3Button12, Player3Button13, Player3Button14, Player3Button15,
			},
			new string[]
			{
				Player4Button00, Player4Button01, Player4Button02, Player4Button03, Player4Button04, Player4Button05, Player4Button06, Player4Button07, Player4Button08, Player4Button09, Player4Button10, Player4Button11, Player4Button12, Player4Button13, Player4Button14, Player4Button15,
			},
		} ;

		// アクシス割り当て
		private static readonly string[][]	m_AxisNames =
		{
			new string[]
			{
				Player1Axis00, Player1Axis01, Player1Axis02, Player1Axis03, Player1Axis04, Player1Axis05, Player1Axis06, Player1Axis07, Player1Axis08, Player1Axis09, Player1Axis10, Player1Axis11, Player1Axis12, Player1Axis13, Player1Axis14, Player1Axis15,
			},
			new string[]
			{
				Player2Axis00, Player2Axis01, Player2Axis02, Player2Axis03, Player2Axis04, Player2Axis05, Player2Axis06, Player2Axis07, Player2Axis08, Player2Axis09, Player2Axis10, Player2Axis11, Player2Axis12, Player2Axis13, Player2Axis14, Player2Axis15,
			},
			new string[]
			{
				Player3Axis00, Player3Axis01, Player3Axis02, Player3Axis03, Player3Axis04, Player3Axis05, Player3Axis06, Player3Axis07, Player3Axis08, Player3Axis09, Player3Axis10, Player3Axis11, Player3Axis12, Player3Axis13, Player3Axis14, Player3Axis15,
			},
			new string[]
			{
				Player4Axis00, Player4Axis01, Player4Axis02, Player4Axis03, Player4Axis04, Player4Axis05, Player4Axis06, Player4Axis07, Player4Axis08, Player4Axis09, Player4Axis10, Player4Axis11, Player4Axis12, Player4Axis13, Player4Axis14, Player4Axis15,
			},
		} ;
	}
}
