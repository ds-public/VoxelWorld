using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// パッドの対象プレイヤー
	/// </summary>
	public enum PadPlayerTargets
	{
		/// <summary>
		/// 全プレイヤー対象
		/// </summary>
		All			= -1,

		/// <summary>
		/// プレイヤー１のみ対象
		/// </summary>
		Player1		=  0,

		/// <summary>
		/// プレイヤー２のみ対象
		/// </summary>
		Player2		=  1,

		/// <summary>
		/// プレイヤー３のみ対象
		/// </summary>
		Player3		=  2,

		/// <summary>
		/// プレイヤー４のみ対象
		/// </summary>
		Player4		=  3,
	}

	/// <summary>
	/// PadAdapter コンポーネントクラス
	/// </summary>
	public class UIPadAdapter : MonoBehaviour
	{
		/// <summary>
		/// フォーカスの状態
		/// </summary>
		public bool Focus{ get{ return m_Focus ; } set{ m_Focus = value ; } }

		[SerializeField]
		protected bool		m_Focus = true ;

		//-----------------------------------

		/// <summary>
		/// 対象プレイヤーのタイプ
		/// </summary>
		public PadPlayerTargets PadPlayerTarget
		{
			get { return m_PadPlayerTarget ; }
			set { m_PadPlayerTarget = value ; }
		}

		[SerializeField]
		protected PadPlayerTargets	m_PadPlayerTarget = PadPlayerTargets.All ;

		//-----------------------------------------------------------

		/// <summary>
		/// 基本ボタン１
		/// </summary>
		public bool B1{ get{ return m_B1 ; } set{ m_B1 = value ; } }

		[SerializeField]
		protected bool m_B1 ;

		/// <summary>
		/// 基本ボタン２
		/// </summary>
		public bool B2{ get{ return m_B2 ; } set{ m_B2 = value ; } }

		[SerializeField]
		protected bool m_B2 ;

		/// <summary>
		/// 基本ボタン３
		/// </summary>
		public bool B3{ get{ return m_B3 ; } set{ m_B3 = value ; } }

		[SerializeField]
		protected bool m_B3 ;

		/// <summary>
		/// 基本ボタン４
		/// </summary>
		public bool B4{ get{ return m_B4 ; } set{ m_B4 = value ; } }

		[SerializeField]
		protected bool m_B4 ;

		//-----

		/// <summary>
		/// 右ボタン１
		/// </summary>
		public bool R1{ get{ return m_R1 ; } set{ m_R1 = value ; } }

		[SerializeField]
		protected bool m_R1 ;

		/// <summary>
		/// 左ボタン１
		/// </summary>
		public bool L1{ get{ return m_L1 ; } set{ m_L1 = value ; } }

		[SerializeField]
		protected bool m_L1 ;

		/// <summary>
		/// 右ボタン２
		/// </summary>
		public bool R2{ get{ return m_R2 ; } set{ m_R2 = value ; } }

		[SerializeField]
		protected bool m_R2 ;

		/// <summary>
		/// 左ボタン２
		/// </summary>
		public bool L2{ get{ return m_L2 ; } set{ m_L2 = value ; } }

		[SerializeField]
		protected bool m_L2 ;

		/// <summary>
		/// 右ボタン３
		/// </summary>
		public bool R3{ get{ return m_R3 ; } set{ m_R3 = value ; } }

		[SerializeField]
		protected bool m_R3 ;

		/// <summary>
		/// 左ボタン３
		/// </summary>
		public bool L3{ get{ return m_L3 ; } set{ m_L3 = value ; } }

		[SerializeField]
		protected bool m_L3 ;

		//-----

		/// <summary>
		/// 拡張ボタン１
		/// </summary>
		public bool O1{ get{ return m_O1 ; } set{ m_O1 = value ; } }

		[SerializeField]
		protected bool m_O1 ;

		/// <summary>
		/// 拡張ボタン２
		/// </summary>
		public bool O2{ get{ return m_O2 ; } set{ m_O2 = value ; } }

		[SerializeField]
		protected bool m_O2 ;

		/// <summary>
		/// 拡張ボタン３
		/// </summary>
		public bool O3{ get{ return m_O3 ; } set{ m_O3 = value ; } }

		[SerializeField]
		protected bool m_O3 ;

		/// <summary>
		/// 拡張ボタン４
		/// </summary>
		public bool O4{ get{ return m_O4 ; } set{ m_O4 = value ; } }

		[SerializeField]
		protected bool m_O4 ;

		//---------------

		/// <summary>
		/// ボタンのリピート挙動の有効化
		/// </summary>
		public bool ButtonRepeatPressEnabled{ get{ return m_ButtonRepeatPressEnabled ; } set{ m_ButtonRepeatPressEnabled = value ; } }

		[SerializeField]
		protected bool m_ButtonRepeatPressEnabled = false ;


		/// <summary>
		/// ボタンをリピートする場合の開始までの時間
		/// </summary>
		public float ButtonRepeatPressStartingTime{ get{ return m_ButtonRepeatPressStartingTime ; } set{ m_ButtonRepeatPressStartingTime = value ; } }

		[SerializeField]
		protected float m_ButtonRepeatPressStartingTime = 0.75f ;

		/// <summary>
		/// ボタンをリピートする場合の継続までの時間
		/// </summary>
		public float ButtonRepeatPressIntervalTime{ get{ return m_ButtonRepeatPressIntervalTime ; } set{ m_ButtonRepeatPressIntervalTime = value ; } }

		[SerializeField]
		protected float m_ButtonRepeatPressIntervalTime = 0.25f ;

		//-----

		/// <summary>
		/// ボタンのリピート挙動の有効化
		/// </summary>
		public bool ButtonLongPressEnabled{ get{ return m_ButtonLongPressEnabled ; } set{ m_ButtonLongPressEnabled = value ; } }

		[SerializeField]
		protected bool m_ButtonLongPressEnabled = false ;

		/// <summary>
		/// ボタンを長押しにした場合の反応までの時間
		/// </summary>
		public float ButtonLongPressDecisionTime{ get{ return m_ButtonLongPressDecisionTime ; } set{ m_ButtonLongPressDecisionTime = value ; } }

		[SerializeField]
		protected float m_ButtonLongPressDecisionTime = 0.75f ;

		//---------------

		/// <summary>
		/// 十字ボタン右
		/// </summary>
		public bool DP_R{ get{ return m_DP_R ; } set{ m_DP_R = value ; } }

		[SerializeField]
		protected bool m_DP_R ;

		/// <summary>
		/// 十字ボタン左
		/// </summary>
		public bool DP_L{ get{ return m_DP_L ; } set{ m_DP_L = value ; } }

		[SerializeField]
		protected bool m_DP_L ;

		/// <summary>
		/// 十字ボタン上
		/// </summary>
		public bool DP_U{ get{ return m_DP_U ; } set{ m_DP_U = value ; } }

		[SerializeField]
		protected bool m_DP_U ;

		/// <summary>
		/// 十字ボタン下
		/// </summary>
		public bool DP_D{ get{ return m_DP_D ; } set{ m_DP_D = value ; } }

		[SerializeField]
		protected bool m_DP_D ;

		//-----

		/// <summary>
		/// 左スティック右
		/// </summary>
		public bool LS_R{ get{ return m_LS_R ; } set{ m_LS_R = value ; } }

		[SerializeField]
		protected bool m_LS_R ;

		/// <summary>
		/// 左スティック左
		/// </summary>
		public bool LS_L{ get{ return m_LS_L ; } set{ m_LS_L = value ; } }

		[SerializeField]
		protected bool m_LS_L ;

		/// <summary>
		/// 左スティック上
		/// </summary>
		public bool LS_U{ get{ return m_LS_U ; } set{ m_LS_U = value ; } }

		[SerializeField]
		protected bool m_LS_U ;

		/// <summary>
		/// 左スティック下
		/// </summary>
		public bool LS_D{ get{ return m_LS_D ; } set{ m_LS_D = value ; } }

		[SerializeField]
		protected bool m_LS_D ;

		//-----

		/// <summary>
		/// 右スティック右
		/// </summary>
		public bool RS_R{ get{ return m_RS_R ; } set{ m_RS_R = value ; } }

		[SerializeField]
		protected bool m_RS_R ;

		/// <summary>
		/// 右スティック左
		/// </summary>
		public bool RS_L{ get{ return m_RS_L ; } set{ m_RS_L = value ; } }

		[SerializeField]
		protected bool m_RS_L ;

		/// <summary>
		/// 右スティック上
		/// </summary>
		public bool RS_U{ get{ return m_RS_U ; } set{ m_RS_U = value ; } }

		[SerializeField]
		protected bool m_RS_U ;

		/// <summary>
		/// 右スティック下
		/// </summary>
		public bool RS_D{ get{ return m_RS_D ; } set{ m_RS_D = value ; } }

		[SerializeField]
		protected bool m_RS_D ;

		//---------------

		/// <summary>
		/// アクシスをリピートを有効化するかどうか
		/// </summary>
		public bool AxisRepeatPressEnabled{ get{ return m_AxisRepeatPressEnabled ; } set{ m_AxisRepeatPressEnabled = value ; } }

		[SerializeField]
		protected bool m_AxisRepeatPressEnabled = false ;


		/// <summary>
		/// アクシスをリピートにした場合の開始までの時間
		/// </summary>
		public float AxisRepeatPressStartingTime{ get{ return m_AxisRepeatPressStartingTime ; } set{ m_AxisRepeatPressStartingTime = value ; } }

		[SerializeField]
		protected float m_AxisRepeatPressStartingTime = 0.75f ;

		/// <summary>
		/// アクシスをリピートにした場合の継続までの時間
		/// </summary>
		public float AxisRepeatPressIntervalTime{ get{ return m_AxisRepeatPressIntervalTime ; } set{ m_AxisRepeatPressIntervalTime = value ; } }

		[SerializeField]
		protected float m_AxisRepeatPressIntervalTime = 0.25f ;

		//-----

		/// <summary>
		/// アクシスを長押しを有効化するかどうか
		/// </summary>
		public bool AxisLongPressEnabled{ get{ return m_AxisLongPressEnabled ; } set{ m_AxisLongPressEnabled = value ; } }

		[SerializeField]
		protected bool m_AxisLongPressEnabled = false ;

		/// <summary>
		/// アクシスを長押しにした場合の反応までの時間
		/// </summary>
		public float AxisLongPressDecisionTime{ get{ return m_AxisLongPressDecisionTime ; } set{ m_AxisLongPressDecisionTime = value ; } }

		[SerializeField]
		protected float m_AxisLongPressDecisionTime = 0.75f ;

		//-----------------------------------

		/// <summary>
		/// Press を実行するか
		/// </summary>
		public bool ToPress{ get{ return m_ToPress ; } set{ m_ToPress = value ; } }

		[SerializeField]
		protected bool m_ToPress ;

		/// <summary>
		/// Click を実行するか
		/// </summary>
		public bool ToClick{ get{ return m_ToClick ; } set{ m_ToClick = value ; } }

		[SerializeField]
		protected bool m_ToClick ;


		/// <summary>
		/// 押した際に Click を実行するか
		/// </summary>
		public bool ToClickOnDown{ get{ return m_ToClickOnDown ; } set{ m_ToClickOnDown = value ; } }

		[SerializeField]
		protected bool m_ToClickOnDown = true ;

		//---------------

		/// <summary>
		/// 余剰入力を有効にするかどうか(レイキャストブロックされても最後の入力は継続するかどうか)
		/// </summary>
		[SerializeField]
		protected bool m_SurplusInputEnabled = false ;
		/// <summary>
		/// 位置の固定化と常時表示をにするかどうか
		/// </summary>
		public    bool   SurplusInputEnabled
		{
			get{ return m_SurplusInputEnabled ; }
			set{ m_SurplusInputEnabled = value ; }
		}

		//-----------------------------------

#if UNITY_EDITOR

		/// <summary>
		/// Inspector の折りたたみフラグ(エディター専用なので Inspector 以外からの参照は禁止)
		/// </summary>
		public bool Foldout{ get{ return m_Foldout ; } set{ m_Foldout = value ; } }

		[SerializeField]
		protected bool	m_Foldout = true ;
#endif
		//-----------------------------------------------------------

		private UIView	m_View ;

		private bool	m_IsPressing ;

		private int		m_ButtonRepeatState		= 0 ;
		private float	m_ButtonRepeatTimer		= 0 ;
		private int		m_AxisRepeatState		= 0 ;
		private float	m_AxisRepeatTimer		= 0 ;
		private int		m_RepeatCount			= 0 ;

		private int		m_ButtonLongPressFlags	= 0 ;
		private float	m_ButtonLongPressTimer	= 0 ;
		private int		m_AxisLongPressFlags	= 0 ;
		private float	m_AxisLongPressTimer	= 0 ;

		private bool	m_IsPadEnabed			= false ;

		private bool	m_InteractionBlocking	= false ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			TryGetComponent<UIView>( out m_View ) ;
		}

		internal void Update()
		{
			if( Application.isPlaying == true && m_View != null && m_Focus == true )
			{
				ProcessPad() ;
			}			
		}

		internal void OnEnable()
		{
			if( Application.isPlaying == true && m_View != null )
			{
				// InputProcessingTypes が Swicth の場合に InputType が変化したら呼び出されるコールバックを追加する
				UIEventSystem.AddOnInputTypeChanged( OnInputTypeChanged ) ;

				m_IsPadEnabed = ( UIEventSystem.InputType == InputTypes.GamePad ) ;
				m_InteractionBlocking = !( m_View.IsAnyTweenPlayingInParents == false && ( m_View.Alpha >  0 ) && IsPadAvailable() == true ) ;
			}
		}

		internal void OnDisable()
		{
			if( Application.isPlaying == true && m_View != null )
			{
				// InputProcessingTypes が Swicth の場合に InputType が変化したら呼び出されるコールバックを削除する
				UIEventSystem.RemoveOnInputTypeChanged( OnInputTypeChanged ) ;
			}
		}

		// InputProcessingTypes が Swicth の場合に InputType が変化したら呼び出されるコールバック
		private void OnInputTypeChanged( InputTypes inputType )
		{
			if( m_View.ActiveInHierarchy == true && m_View.IsAnyTweenPlayingInParents == false && ( m_View.Alpha >  0 ) && IsPadAvailable() == true )
			{
				m_IsPadEnabed = ( inputType == InputTypes.GamePad ) ;
				m_View.CallOnPadInputStateChanged( m_IsPadEnabed ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// パッドを処理する
		private void ProcessPad()
		{
			bool inputAvailable ;
			if( m_SurplusInputEnabled == false )
			{
				// 通常
				inputAvailable = ( m_View.ActiveInHierarchy == true && m_View.IsAnyTweenPlayingInParents == false && ( m_View.Alpha >  0 ) && IsPadAvailable() == true ) ;
			}
			else
			{
				// 特殊
				if( m_IsPressing == true )
				{
					inputAvailable = true ;
				}
				else
				{
					inputAvailable = ( m_View.ActiveInHierarchy == true && m_View.IsAnyTweenPlayingInParents == false && ( m_View.Alpha >  0 ) && IsPadAvailable() == true ) ;
				}
			}


			if( inputAvailable == true )
			{
				//---------------------------------------------------------

				int buttonFlags	;
				int axisFlags ;

				if( m_InteractionBlocking == true )
				{
					// 入力にブロックがかかった場合は入力が一度完全に開放されるのを待つ
					buttonFlags	= CheckButton() ;
					axisFlags	= CheckAxis() ;

					if( buttonFlags == 0 && axisFlags == 0 )
					{
						m_InteractionBlocking = false ;
					}

					// ブロッキングが解除されるまで入力を受け付けない
					return ;
				}

				//-----------------------------------------------------------------------------------------

				// 入力モードが変わっていたらコールバックを呼ぶ
				bool isPadEnabled = ( UIEventSystem.InputType == InputTypes.GamePad ) ;
				if( m_IsPadEnabed != isPadEnabled )
				{
					m_IsPadEnabed  = isPadEnabled ;
					m_View.CallOnPadInputStateChanged( m_IsPadEnabed ) ;
				}

				//-----------------------------------------------------------------------------------------
				// 最初は必ず Down 系で処理する必要がある(でないとＵＩのブロッカー状態の変動で入って欲しくない入力が入ってしまう[ボタンの押下状態はビュー単位ではなくグローバル管理である必要がある)

				int buttonDownFlags	= CheckButtonDown() ;
				int axisDownFlags	= CheckAxisDown() ;

				//---------------------------------------------------------
				// Press

				if( buttonDownFlags != 0 || axisDownFlags != 0 )
				{
					// ボタンまたはアクシスが押された

					if( m_IsPressing == false )
					{
						m_IsPressing	= true ;

						//-------------------------------------------------------

						if( m_ToPress == true )
						{
							if( m_View is UIButton button )
							{
								if( button.Interactable == true )
								{
									button.ExecutePress( true ) ;
								}
							}
							else
							if( m_View is UIView view )
							{
								if( view.IsInteraction == true )
								{
									view.ExecutePress( true ) ;
								}
							}
						}

						if( m_ToClick == true && m_ToClickOnDown == true )
						{
							if( m_View is UIButton button )
							{
								if( button.Interactable == true )
								{
									button.ExecuteClick() ;
								}
							}
							else
							if( m_View is UIView view )
							{
								if( view.IsInteraction == true )
								{
									view.ExecuteClick() ;
								}
							}
						}

						//--------------------------------------------------------

						if( buttonDownFlags != 0 )
						{
							m_View.CallOnPadButtonDown( buttonDownFlags ) ;
						}

						//------------

						int i, l = 3 ;
						var axisVectorFlags = new Vector2[ l ] ;
						var margedAxisVectorFlag = Vector2.zero ;

						if( axisDownFlags != 0 )
						{
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( ( axisDownFlags & 0x000F ) != 0 )
								{
									var axis = Vector2.zero ;

									if( ( axisDownFlags & 1 ) != 0 )
									{
										axis.x += 1 ;
									}
									axisDownFlags >>= 1 ;

									if( ( axisDownFlags & 1 ) != 0 )
									{
										axis.x -= 1 ;
									}
									axisDownFlags >>= 1 ;

									if( axis.x != 0 )
									{
										margedAxisVectorFlag.x = axis.x ;
									}

									if( ( axisDownFlags & 1 ) != 0 )
									{
										axis.y += 1 ;
									}
									axisDownFlags >>= 1 ;

									if( ( axisDownFlags & 1 ) != 0 )
									{
										axis.y -= 1 ;
									}
									axisDownFlags >>= 1 ;

									if( axis.y != 0 )
									{
										margedAxisVectorFlag.y = axis.y ;
									}

									axisVectorFlags[ i ] = axis ;
								}
								else
								{
									axisDownFlags >>= 4 ;
								}
							}

							m_View.CallOnPadAxisDown( axisVectorFlags, margedAxisVectorFlag ) ;
						}

						m_View.CallOnPadDown( buttonDownFlags, axisVectorFlags, margedAxisVectorFlag ) ;
					}
				}

				//-----------------------------------------------------------------------------------------
				// 以下は押しっぱなし系の処理

				buttonFlags	= CheckButton() ;
				axisFlags	= CheckAxis() ;

				//---------------------------------------------------------
				// RepeatPress

				if( m_ButtonRepeatPressEnabled == true || m_AxisRepeatPressEnabled == true )
				{
					int buttonRepeatPressFlags = 0 ;

					if( m_ButtonRepeatPressEnabled == true )
					{
						buttonRepeatPressFlags	= CheckButtonRepeatPress( buttonFlags ) ;
					}

					int axisRepeatPressFlags = 0 ;

					if( m_AxisRepeatPressEnabled == true )
					{
						axisRepeatPressFlags = CheckAxisRepeatPress( axisFlags ) ;
					}

					if( buttonRepeatPressFlags != 0 || axisRepeatPressFlags != 0 )
					{
						// ボタンまたはアクシスが押された

						if( m_ToPress == true )
						{
							if( m_View is UIButton button )
							{
								if( button.Interactable == true )
								{
									button.ExecuteRepeatPress( m_RepeatCount ) ;
								}
							}
							else
							if( m_View is UIView view )
							{
								if( view.IsInteraction == true )
								{
									view.ExecuteRepeatPress( m_RepeatCount ) ;
								}
							}
						}

						//--------------------------------------------------------

						if( m_RepeatCount >  0 )
						{
							// 最初の１回目は無視する(Down でコールしている)

							if( buttonRepeatPressFlags != 0 )
							{
								m_View.CallOnPadButtonDown( buttonRepeatPressFlags ) ;
							}

							//------------

							int i, l = 3 ;
							var axisVectorFlags = new Vector2[ l ] ;
							var margedAxisVectorFlag = Vector2.zero ;

							if( axisRepeatPressFlags != 0 )
							{
								for( i  = 0 ; i <  l ; i ++ )
								{
									if( ( axisRepeatPressFlags & 0x000F ) != 0 )
									{
										var axis = Vector2.zero ;

										if( ( axisRepeatPressFlags & 1 ) != 0 )
										{
											axis.x += 1 ;
										}
										axisRepeatPressFlags >>= 1 ;

										if( ( axisRepeatPressFlags & 1 ) != 0 )
										{
											axis.x -= 1 ;
										}
										axisRepeatPressFlags >>= 1 ;

										if( axis.x != 0 )
										{
											margedAxisVectorFlag.x = axis.x ;
										}

										if( ( axisRepeatPressFlags & 1 ) != 0 )
										{
											axis.y += 1 ;
										}
										axisRepeatPressFlags >>= 1 ;

										if( ( axisRepeatPressFlags & 1 ) != 0 )
										{
											axis.y -= 1 ;
										}
										axisRepeatPressFlags >>= 1 ;

										if( axis.y != 0 )
										{
											margedAxisVectorFlag.y = axis.y ;
										}

										axisVectorFlags[ i ] = axis ;
									}
									else
									{
										axisRepeatPressFlags >>= 4 ;
									}
								}

								m_View.CallOnPadAxisDown( axisVectorFlags, margedAxisVectorFlag ) ;
							}

							m_View.CallOnPadDown( buttonRepeatPressFlags, axisVectorFlags, margedAxisVectorFlag ) ;
						}

						//-------------------------------

						m_RepeatCount ++ ;
					}
				}

				//---------------------------------------------------------
				// LongPress

				if( m_ButtonLongPressEnabled == true || m_AxisLongPressEnabled == true )
				{
					int buttonLongPressFlags = 0 ;

					if( m_ButtonLongPressEnabled == true )
					{
						buttonLongPressFlags	= CheckButtonLongPress( buttonFlags ) ;
					}

					int axisLongPressFlags = 0 ;

					if( m_AxisLongPressEnabled == true )
					{
						axisLongPressFlags = CheckAxisLongPress( axisFlags ) ;
					}

					if( buttonLongPressFlags != 0 || axisLongPressFlags != 0 )
					{
						// ボタンまたはアクシスが押された

						if( m_ToPress == true )
						{
							if( m_View is UIButton button )
							{
								if( button.Interactable == true )
								{
									button.ExecuteLongPress() ;
								}
							}
							else
							if( m_View is UIView view )
							{
								if( view.IsInteraction == true )
								{
									view.ExecuteLongPress() ;
								}
							}
						}

						//--------------------------------------------------------

						if( buttonLongPressFlags != 0 )
						{
							m_View.CallOnPadButtonLongPress( buttonLongPressFlags ) ;
						}

						//------------

						int i, l = 3 ;
						var axisVectorFlags = new Vector2[ l ] ;
						var margedAxisVectorFlag = Vector2.zero ;

						if( axisLongPressFlags != 0 )
						{
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( ( axisLongPressFlags & 0x000F ) != 0 )
								{
									var axis = Vector2.zero ;

									if( ( axisLongPressFlags & 1 ) != 0 )
									{
										axis.x += 1 ;
									}
									axisLongPressFlags >>= 1 ;

									if( ( axisLongPressFlags & 1 ) != 0 )
									{
										axis.x -= 1 ;
									}
									axisLongPressFlags >>= 1 ;

									if( axis.x != 0 )
									{
										margedAxisVectorFlag.x = axis.x ;
									}

									if( ( axisLongPressFlags & 1 ) != 0 )
									{
										axis.y += 1 ;
									}
									axisLongPressFlags >>= 1 ;

									if( ( axisLongPressFlags & 1 ) != 0 )
									{
										axis.y -= 1 ;
									}
									axisLongPressFlags >>= 1 ;

									if( axis.y != 0 )
									{
										margedAxisVectorFlag.y = axis.y ;
									}

									axisVectorFlags[ i ] = axis ;
								}
								else
								{
									axisLongPressFlags >>= 4 ;
								}
							}

							m_View.CallOnPadAxisLongPress( axisVectorFlags, margedAxisVectorFlag ) ;
						}

						m_View.CallOnPadLongPress( buttonLongPressFlags, axisVectorFlags, margedAxisVectorFlag ) ;
					}
				}

				//---------------------------------------------------------

				if( buttonFlags == 0 && axisFlags == 0 )
				{
					// ボタンおよびアクシスが離された

					if( m_IsPressing == true )
					{
						m_IsPressing  = false ;

						// 離された場合の共通処理
						CallRelease() ;
					}
				}

				//-------------

				if( m_View is UIPadAxis uiPadAxis )
				{
					uiPadAxis.SetAxisFromPadAdapter( GetAxis() ) ;
				}
			}
			else
			{
				// ブロッキングが行われて入力が遮断された

				if( m_InteractionBlocking == false )
				{
					// 入力をブロックする
					m_InteractionBlocking  = true ;

					//--------------------------------

					if( m_IsPressing == true )
					{
						m_IsPressing  = false ;

						// 離された場合の共通処理
						CallRelease() ;
					}

					//------------

					if( m_View is UIPadAxis uiPadAxis )
					{
						uiPadAxis.SetAxisFromPadAdapter( Vector2.zero ) ;
					}
				}
			}

			// 離された場合の共通処理
			void CallRelease()
			{
				if( m_ToPress == true )
				{
					if( m_View is UIButton button )
					{
						if( button.Interactable == true )
						{
							button.ExecutePress( false ) ;
						}
					}
					else
					if( m_View is UIView view )
					{
						if( view.IsInteraction == true )
						{
							view.ExecutePress( false ) ;
						}
					}
				}

				if( m_ToClick == true && m_ToClickOnDown == false )
				{
					if( m_View is UIButton button )
					{
						if( button.Interactable == true )
						{
							button.ExecuteButtonClick() ;
						}
					}
					else
					if( m_View is UIView view )
					{
						if( view.IsInteraction == true )
						{
							view.ExecuteClick() ;
						}
					}
				}

				//--------------------------------

				m_View.CallOnPadButtonUp() ;
				m_View.CallOnPadAxisUp() ;

				m_View.CallOnPadUp() ;

				//--------------------------------

				m_RepeatCount	= 0 ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// ボタン系の押下フラグを取得する
		private int CheckButtonDown()
		{
			int playerNumber = ( int )m_PadPlayerTarget ;

			//----------------------------------

			int buttonFlags = 0x0000 ;

			if( m_B1 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.B1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B1 ;
				}
			}

			if( m_B2 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.B2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B2 ;
				}
			}

			if( m_B3 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.B3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B3 ;
				}
			}

			if( m_B4 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.B4, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B4 ;
				}
			}

			//--------------

			if( m_R1 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.R1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R1 ;
				}
			}

			if( m_L1 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.L1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L1 ;
				}
			}

			if( m_R2 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.R2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R2 ;
				}
			}

			if( m_L2 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.L2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L2 ;
				}
			}

			if( m_R3 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.R3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R3 ;
				}
			}

			if( m_L3 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.L3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L3 ;
				}
			}

			//--------------

			if( m_O1 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.O1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O1 ;
				}
			}

			if( m_O2 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.O2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O2 ;
				}
			}

			if( m_O3 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.O3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O3 ;
				}
			}

			if( m_O4 == true )
			{
				if( UIEventSystem.GetButtonDown( GamePad.O4, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O4 ;
				}
			}

			//----------------------------------

			return buttonFlags ;
		}

		// アクシス系の押下フラグを取得する
		private int CheckAxisDown( float digitalThreshold = 0.5f )
		{
			int playerNumber = ( int )m_PadPlayerTarget ;

			//----------------------------------

			int axisFlags = 0x0000 ;

			if( m_DP_R == true || m_DP_L == true || m_DP_U == true || m_DP_D == true )
			{
				var axis = UIEventSystem.GetAxisDown( 0, playerNumber ) ;

				if( m_DP_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0001 ;
				}

				if( m_DP_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0002 ;
				}

				if( m_DP_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0004 ;
				}

				if( m_DP_D == true && axis.y <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0008 ;
				}
			}

			//----

			if( m_LS_R == true || m_LS_L == true || m_LS_U == true || m_LS_D == true )
			{
				var axis = UIEventSystem.GetAxisDown( 1, playerNumber ) ;

				if( m_LS_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0010 ;
				}

				if( m_LS_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0020 ;
				}

				if( m_LS_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0040 ;
				}

				if( m_LS_D == true && axis.y <  ( - digitalThreshold ) ) 
				{
					axisFlags |= 0x0080 ;
				}
			}

			//----

			if( m_RS_R == true || m_RS_L == true || m_RS_U == true || m_RS_D == true )
			{
				var axis = UIEventSystem.GetAxisDown( 2, playerNumber ) ;

				if( m_RS_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0100 ;
				}

				if( m_RS_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0200 ;
				}

				if( m_RS_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0400 ;
				}

				if( m_RS_D == true && axis.y <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0800 ;
				}
			}

			//----------------------------------

			return axisFlags ;
		}

		// ボタン系の押下フラグを取得する
		private int CheckButton()
		{
			int playerNumber = ( int )m_PadPlayerTarget ;

			//----------------------------------

			int buttonFlags = 0x0000 ;

			if( m_B1 == true )
			{
				if( UIEventSystem.GetButton( GamePad.B1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B1 ;
				}
			}

			if( m_B2 == true )
			{
				if( UIEventSystem.GetButton( GamePad.B2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B2 ;
				}
			}

			if( m_B3 == true )
			{
				if( UIEventSystem.GetButton( GamePad.B3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B3 ;
				}
			}

			if( m_B4 == true )
			{
				if( UIEventSystem.GetButton( GamePad.B4, playerNumber ) == true )
				{
					buttonFlags |= GamePad.B4 ;
				}
			}

			//--------------

			if( m_R1 == true )
			{
				if( UIEventSystem.GetButton( GamePad.R1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R1 ;
				}
			}

			if( m_L1 == true )
			{
				if( UIEventSystem.GetButton( GamePad.L1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L1 ;
				}
			}

			if( m_R2 == true )
			{
				if( UIEventSystem.GetButton( GamePad.R2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R2 ;
				}
			}

			if( m_L2 == true )
			{
				if( UIEventSystem.GetButton( GamePad.L2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L2 ;
				}
			}

			if( m_R3 == true )
			{
				if( UIEventSystem.GetButton( GamePad.R3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.R3 ;
				}
			}

			if( m_L3 == true )
			{
				if( UIEventSystem.GetButton( GamePad.L3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.L3 ;
				}
			}

			//--------------

			if( m_O1 == true )
			{
				if( UIEventSystem.GetButton( GamePad.O1, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O1 ;
				}
			}

			if( m_O2 == true )
			{
				if( UIEventSystem.GetButton( GamePad.O2, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O2 ;
				}
			}

			if( m_O3 == true )
			{
				if( UIEventSystem.GetButton( GamePad.O3, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O3 ;
				}
			}

			if( m_O4 == true )
			{
				if( UIEventSystem.GetButton( GamePad.O4, playerNumber ) == true )
				{
					buttonFlags |= GamePad.O4 ;
				}
			}

			//----------------------------------

			return buttonFlags ;
		}

		// アクシス系の押下フラグを取得する
		private int CheckAxis( float digitalThreshold = 0.5f )
		{
			int playerNumber = ( int )m_PadPlayerTarget ;

			//----------------------------------

			int axisFlags = 0x0000 ;

			if( m_DP_R == true || m_DP_L == true || m_DP_U == true || m_DP_D == true )
			{
				var axis = UIEventSystem.GetAxis( 0, playerNumber ) ;

				if( m_DP_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0001 ;
				}

				if( m_DP_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0002 ;
				}

				if( m_DP_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0004 ;
				}

				if( m_DP_D == true && axis.y <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0008 ;
				}
			}

			//----

			if( m_LS_R == true || m_LS_L == true || m_LS_U == true || m_LS_D == true )
			{
				var axis = UIEventSystem.GetAxis( 1, playerNumber ) ;

				if( m_LS_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0010 ;
				}

				if( m_LS_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0020 ;
				}

				if( m_LS_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0040 ;
				}

				if( m_LS_D == true && axis.y <  ( - digitalThreshold ) ) 
				{
					axisFlags |= 0x0080 ;
				}
			}

			//----

			if( m_RS_R == true || m_RS_L == true || m_RS_U == true || m_RS_D == true )
			{
				var axis = UIEventSystem.GetAxis( 2, playerNumber ) ;

				if( m_RS_R == true && axis.x >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0100 ;
				}

				if( m_RS_L == true && axis.x <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0200 ;
				}

				if( m_RS_U == true && axis.y >  ( + digitalThreshold ) )
				{
					axisFlags |= 0x0400 ;
				}

				if( m_RS_D == true && axis.y <  ( - digitalThreshold ) )
				{
					axisFlags |= 0x0800 ;
				}
			}

			//----------------------------------

			return axisFlags ;
		}

		//---------------
		// Repeat

		private int CheckButtonRepeatPress( int buttonFlags )
		{
			if( buttonFlags != 0 )
			{
				if( m_ButtonRepeatState == 0 )
				{
					// 処理開始

					m_ButtonRepeatTimer = Time.realtimeSinceStartup ;

					m_ButtonRepeatState = 1 ;
				}
				else
				if( m_ButtonRepeatState == 1 )
				{
					// 開始判定
					if( ( Time.realtimeSinceStartup - m_ButtonRepeatTimer ) <  m_ButtonRepeatPressStartingTime )
					{
						// まだ
						buttonFlags = 0 ;
					}
					else
					{
						// 有効(開放するまで反応しなくなる)
						m_ButtonRepeatTimer = Time.realtimeSinceStartup ;

						m_ButtonRepeatState = 2 ;
					}
				}
				else
				if( m_ButtonRepeatState == 2 )
				{
					// 継続判定
					if( ( Time.realtimeSinceStartup - m_ButtonRepeatTimer ) <  m_ButtonRepeatPressIntervalTime )
					{
						// まだ
						buttonFlags = 0 ;
					}
					else
					{
						m_ButtonRepeatTimer = Time.realtimeSinceStartup ;
					}
				}
			}
			else
			{
				// 開放
				m_ButtonRepeatState = 0 ;
			}

			//----------------------------------

			return buttonFlags ;
		}

		private int CheckAxisRepeatPress( int axisFlags )
		{
			if( axisFlags != 0 )
			{
				if( m_AxisRepeatState == 0 )
				{
					// 処理開始

					m_AxisRepeatTimer = Time.realtimeSinceStartup ;

					m_AxisRepeatState = 1 ;
				}
				else
				if( m_AxisRepeatState == 1 )
				{
					// 開始判定
					if( ( Time.realtimeSinceStartup - m_AxisRepeatTimer ) <  m_AxisRepeatPressStartingTime )
					{
						// まだ
						axisFlags = 0 ;
					}
					else
					{
						m_AxisRepeatTimer = Time.realtimeSinceStartup ;

						m_AxisRepeatState = 2 ;
					}
				}
				else
				if( m_AxisRepeatState == 2 )
				{
					// 継続判定
					if( ( Time.realtimeSinceStartup - m_AxisRepeatTimer ) <  m_AxisRepeatPressIntervalTime )
					{
						// まだ
						axisFlags = 0 ;
					}
					else
					{
						m_AxisRepeatTimer = Time.realtimeSinceStartup ;
					}
				}
			}
			else
			{
				// 開放
				m_AxisRepeatState = 0 ;
			}

			//----------------------------------

			return axisFlags ;
		}

		//---------------
		// LongPress

		private int CheckButtonLongPress( int buttonFlags )
		{
			if( buttonFlags != 0 )
			{
				if( m_ButtonLongPressFlags == 0 )
				{
					// 処理開始

					m_ButtonLongPressFlags = buttonFlags ;
					m_ButtonLongPressTimer = Time.realtimeSinceStartup ;

					buttonFlags = 0 ;
				}
				else
				{
					if( ( buttonFlags & m_ButtonLongPressFlags ) != 0 )
					{
						// いずれかのボタンを継続して押し続けていれば良い

						if( m_ButtonLongPressTimer >  0 )
						{
							// 計測が開始されている
							if( ( Time.realtimeSinceStartup - m_ButtonLongPressTimer ) <  m_ButtonLongPressDecisionTime )
							{
								// まだ
								buttonFlags = 0 ;
							}
							else
							{
								// 有効(開放するまで反応しなくなる)
								m_ButtonLongPressTimer = 0 ;
							}
						}
						else
						{
							// 一度実行後
							buttonFlags = 0 ;
						}
					}
					else
					{
						buttonFlags = 0 ;
					}
				}
			}
			else
			{
				// 開放
				m_ButtonLongPressFlags = 0 ;
			}

			//----------------------------------

			return buttonFlags ;
		}

		private int CheckAxisLongPress( int axisFlags )
		{
			if( axisFlags != 0 )
			{
				if( m_AxisLongPressFlags == 0 )
				{
					// 処理開始

					m_AxisLongPressFlags = axisFlags ;
					m_AxisLongPressTimer = Time.realtimeSinceStartup ;

					axisFlags = 0 ;
				}
				else
				{
					if( ( axisFlags & m_AxisLongPressFlags ) != 0 )
					{
						// いずれかのボタンを継続して押し続けていれば良い

						if( m_AxisLongPressTimer >  0 )
						{
							// 計測が開始されている
							if( ( Time.realtimeSinceStartup - m_AxisLongPressTimer ) <  m_AxisLongPressDecisionTime )
							{
								// まだ
								axisFlags = 0 ;
							}
							else
							{
								// 有効(開放するまで反応しなくなる)
								m_AxisLongPressTimer = 0 ;
							}
						}
						else
						{
							// 一度実行後
							axisFlags = 0 ;
						}
					}
					else
					{
						axisFlags = 0 ;
					}
				}
			}
			else
			{
				// 開放
				m_AxisLongPressFlags = 0 ;
			}

			//----------------------------------

			return axisFlags ;
		}

		//-----------------------------------------------------------

		// アクシスの状態を取得する
		private Vector2 GetAxis()
		{
			int playerNumber = ( int )m_PadPlayerTarget ;

			//----------------------------------

			Vector2 axisResult = Vector2.zero ;

			if( m_DP_R == true || m_DP_L == true || m_DP_U == true || m_DP_D == true )
			{
				var axis = UIEventSystem.GetAxis( 0, playerNumber ) ;

				if( m_DP_R == true && axis.x >  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_DP_L == true && axis.x <  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_DP_U == true && axis.y >  0 )
				{
					axisResult.y = axis.y ;
				}

				if( m_DP_D == true && axis.y <  0 )
				{
					axisResult.y = axis.y ;
				}
			}

			//----

			if( m_LS_R == true || m_LS_L == true || m_LS_U == true || m_LS_D == true )
			{
				var axis = UIEventSystem.GetAxis( 1, playerNumber ) ;

				if( m_LS_R == true && axis.x >  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_LS_L == true && axis.x <  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_LS_U == true && axis.y >  0 )
				{
					axisResult.y = axis.y ;
				}

				if( m_LS_D == true && axis.y <  0 )
				{
					axisResult.y = axis.y ;
				}
			}

			//----

			if( m_RS_R == true || m_RS_L == true || m_RS_U == true || m_RS_D == true )
			{
				var axis = UIEventSystem.GetAxis( 2, playerNumber ) ;

				if( m_RS_R == true && axis.x >  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_RS_L == true && axis.x <  0 )
				{
					axisResult.x = axis.x ;
				}

				if( m_RS_U == true && axis.y >  0 )
				{
					axisResult.y = axis.y ;
				}

				if( m_RS_D == true && axis.y <  0 )
				{
					axisResult.y = axis.y ;
				}
			}

			//----------------------------------

			return axisResult ;
		}

		//-------------------------------------------------------------------------------------------

		// パッドが押せるか確認する
		private readonly PointerEventData		m_PA_EventDataCurrentPosition = new ( EventSystem.current ) ;
		private readonly List<RaycastResult>	m_PA_Results = new () ;

		// このＵＩが現在有効な状態か確認する
		private bool IsPadAvailable()
		{
			if( m_View == null || EventSystem.current == null )
			{
				// 使用可能扱いとする
				return true ;
			}

			//------------------------------------------------------------------------------------------

			// スクリーン座標を計算する
			( var padViewPoints, var padViewCenter ) = GetScreenArea( gameObject ) ;

			//----------------------------------

			// 一時的にレイキャストターゲットを有効化する(よって Graphic コンポーネント必須)
			bool raycastTarget = true ;
			if( m_View.RaycastTarget == false )
			{
				raycastTarget = m_View.RaycastTarget ;
				m_View.RaycastTarget = true ;
			}

			//----------------------------------------------------------

			bool isAvailable = false ;

			// レイキャストを実行しヒットする対象を検出する
			m_PA_EventDataCurrentPosition.position = padViewCenter ;
			m_PA_Results.Clear() ;
			EventSystem.current.RaycastAll( m_PA_EventDataCurrentPosition, m_PA_Results ) ;

			// ヒットしない事は基本的にありえない
			foreach( var result in m_PA_Results )
			{
				if( result.gameObject == gameObject )
				{
					// 有効
					isAvailable = true ;
					break ;
				}
				else
				{
					// レイキャストヒット対象がゲームパッド対象ビューそのものでなくても親に含まれていたらスルーする
					if( IsContainParent( gameObject, result.gameObject ) == false )
					{
						// 親では無い
						( var blockerPoints, var blockerCenter ) = GetScreenArea( result.gameObject ) ;
						if( IsCompleteBlocking( padViewPoints, blockerPoints ) == true )
						{
							// 無効
							isAvailable = false ;
							break ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// レイキャスト無効でもヒット判定を有効にしていた場合は設定を元に戻す
			if( raycastTarget == false )
			{
				m_View.RaycastTarget = raycastTarget ;
			}

			//----------------------------------

			// 対象のゲームパッド対応ビューが有効か無効か返す
			return isAvailable ;
		}

		// スクリーン上の矩形範囲を取得する
		private ( Vector2[], Vector2 ) GetScreenArea( GameObject go )
		{
			if( go.TryGetComponent<RectTransform>( out var rt ) == false )
			{
				// 取得出来ない
				throw new Exception( "Not foud rectTransform." ) ;
			}

			//----------------------------------

			// 横幅・縦幅
			float tw = rt.rect.width ;
			float th = rt.rect.height ;

			// レイキャストパディング
			Vector4 raycastPadding = Vector4.zero ;

			if( go.TryGetComponent<UnityEngine.UI.Image>( out var image ) == true )
			{
				raycastPadding = image.raycastPadding ;
			}

			float tx0 = ( tw * ( 0 - rt.pivot.x ) ) + raycastPadding.x ;	// x = left
			float ty0 = ( th * ( 0 - rt.pivot.y ) ) + raycastPadding.y ;	// y = bottom
			float tx1 = ( tw * ( 1 - rt.pivot.x ) ) - raycastPadding.z ;	// z = right
			float ty1 = ( th * ( 1 - rt.pivot.y ) ) - raycastPadding.w ;	// w = top

			// 角の座標(まだローカルの２次元)	※順番は右回りである事に注意(Ｚ型ではない)
			var points = new Vector2[ 4 ]
			{
				new ( tx0, ty0 ),
				new ( tx1, ty0 ),
				new ( tx1, ty1 ),
				new ( tx0, ty1 ),
			} ;

			// ローカル座標をワールド座標に変換(ローカルのローテーションとスケールも反映)
			int i, l = points.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// ローテーションとスケールも考慮するため個別に分ける
				points[ i ] = rt.TransformPoint( points[ i ] ) ;
			}

			//----------------------------------

			Camera targetCamera ;

			var parentCanvas = rt.transform.GetComponentInParent<Canvas>() ;
			if( parentCanvas != null )
			{
				if( parentCanvas.worldCamera != null )
				{
					// Screen Space - Camera
					targetCamera = parentCanvas.worldCamera ;
				}
				else
				{
					// Screen Space - Overlay
					targetCamera = Camera.main ;
				}
			}
			else
			{
				throw new Exception( "Not foud canvas." ) ;
			}

			// スクリーン座標に変換する
			Vector2 center = Vector2.zero ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				points[ i ] = RectTransformUtility.WorldToScreenPoint( targetCamera, points[ i ] ) ;
				center += points[ i ] ;
			}

			center /= l ;

			return ( points, center ) ;
		}

		// ブロッカーの親にこのＵＩが含まれているかどうか
		private bool IsContainParent( GameObject target, GameObject blocker )
		{
			while( blocker.transform.parent != null )
			{
				blocker = blocker.transform.parent.gameObject ;

				if( blocker == target )
				{
					// 含まれている
					return true ;
				}
			}

			// 含まれていない
			return false ;
		}

		// このＵＩがブロッカーの内側に完全に隠されているか確認する
		private bool IsCompleteBlocking( Vector2[] targetPoints, Vector2[] blockerPoints )
		{
			int oi, ol = blockerPoints.Length ;
			int ii, il = targetPoints.Length ;

			for( oi = 0 ; oi <  ol ; oi ++ )
			{
				var op0 = blockerPoints[ oi ] ;
				var op1 = blockerPoints[ ( oi + 1 ) % ol ] ;

				for( ii = 0 ; ii <  il ; ii ++ )
				{
					var ip = targetPoints[ ii ] ;

					// 外積を用いて表裏判定を行う(Cross)
					var v0 = op1 - op0 ;
					var v1 = ip  - op0 ;
					var cross = ( v0.x * v1.y ) - ( v0.y * v1.x ) ;

					if( cross <  0 )
					{
						// 外側にある
						return false ;
					}
				}
			}

			// 全て内側にある
			return true ;
		}
	}
}
