using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using uGUIHelper ;
using Cysharp.Threading.Tasks ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace DSW.UI
{
	/// <summary>
	/// ボタンのトランジション Version 2024/04/13
	/// </summary>
	/// <summary>
	/// Transition コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.Animator ) ) ][ ExecuteInEditMode ]
	public class ButtonTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		//-----------------------------------------------------------

		[Header( "アニメーションを有効にするかどうか" )]

		[SerializeField]
		protected bool		m_AnimatonEnabled	= true ;


		[Header( "タップ時のエフェクト" )]

		[SerializeField]
		protected Ripple.ButtonEffectTypes	m_EffectType ;

		/// <summary>
		/// タップ時のエフェクト(外部からの強制変更用)
		/// </summary>
		public Ripple.ButtonEffectTypes EffectType
		{
			get
			{
				return m_EffectType ;
			}
			set
			{
				m_EffectType = value ;
			}
		}


		//-----------------------------------

		[Header( "上書きする情報" )]

		// 色情報を上書きするかどうか
		[SerializeField]
		protected	bool	m_IsOverride	= false ;

		/// <summary>
		/// 色情報を上書きするかどうか
		/// </summary>
		public		bool	  IsOverride
		{
			get
			{
				return m_IsOverride ;
			}
			set
			{
				if( m_IsOverride != value )
				{
					m_IsOverride = value ;
					ResetState() ;
				}
			}
		}

		// ノーマル状態の色
		[SerializeField]
		protected	Color32	m_Normal		= new ( 255, 255, 255, 255 ) ;

		/// <summary>
		/// ノーマル状態の色
		/// </summary>
		public		Color32	  Normal
		{
			get
			{
				return m_Normal ;
			}
			set
			{
				if( CompareColor( m_Normal, value ) == false )
				{
					ReplaceColor( ref value, ref m_Normal ) ;
					ResetState() ;
				}
			}
		}


		// ハイライト状態の色
		[SerializeField]
		protected	Color32	m_Highlighted	= new ( 247, 247, 247, 255 ) ;

		/// <summary>
		/// ハイライト状態の色
		/// </summary>
		public		Color32	  Highlighted
		{
			get
			{
				return m_Highlighted ;
			}
			set
			{
				if( CompareColor( m_Highlighted, value ) == false )
				{
					ReplaceColor( ref value, ref m_Highlighted ) ;
					ResetState() ;
				}
			}
		}


		// プレス状態の色
		[SerializeField]
		protected	Color32	m_Pressed		= new ( 199, 199, 199, 255 ) ;

		/// <summary>
		/// プレス状態の色
		/// </summary>
		public		Color32	  Pressed
		{
			get
			{
				return m_Pressed ;
			}
			set
			{
				if( CompareColor( m_Pressed, value ) == false )
				{
					ReplaceColor( ref value, ref m_Pressed ) ;
					ResetState() ;
				}
			}
		}


		// セレクト状態の色
		[SerializeField]
		protected	Color32	m_Selected		= new ( 247, 247, 247, 255 ) ;

		/// <summary>
		/// セレクト状態の色
		/// </summary>
		public		Color32	  Selected
		{
			get
			{
				return m_Pressed ;
			}
			set
			{
				if( CompareColor( m_Selected, value ) == false )
				{
					ReplaceColor( ref value, ref m_Selected ) ;
					ResetState() ;
				}
			}
		}


		// ディスエーブル状態の色
		[SerializeField]
		protected	Color32	m_Disabled		= new ( 143, 143, 143, 255 ) ;

		/// <summary>
		/// ディスエーブル状態の色
		/// </summary>
		public		Color32	  Disabled
		{
			get
			{
				return m_Disabled ;
			}
			set
			{
				if( CompareColor( m_Disabled, value ) == false )
				{
					ReplaceColor( ref value, ref m_Disabled ) ;
					ResetState() ;
				}
			}
		}

		// フェードの変化時間
		[SerializeField]
		protected	float	m_FadeDuration	= 0.1f ;

		/// <summary>
		/// フェードの変化時間
		/// </summary>
		public		float	  FadeDuration
		{
			get
			{
				return m_FadeDuration ;
			}
			set
			{
				m_FadeDuration = value ;
			}
		}

		// エフェクトが終了した際に呼び出すコールバックを設定する
		private Action m_OnEffectFinished ;

		/// <summary>
		/// エフェクトが終了した際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onEffectFinished"></param>
		public void SetOnEffectFinished( Action onEffectFinished )
		{
			m_OnEffectFinished = onEffectFinished ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// まとめて色を変える
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="highlighted"></param>
		/// <param name="pressed"></param>
		/// <param name="selected"></param>
		/// <param name="disabled"></param>
		/// <param name="fadeDuration"></param>
		public void SetOverrideColors( Color32 normal, Color32 highlighted, Color32 pressed, Color32 selected, Color32 disabled, float fadeDuration = 0.1f )
		{
			m_IsOverride = true ;

			ReplaceColor( ref normal,		ref m_Normal		) ;
			ReplaceColor( ref highlighted,	ref m_Highlighted	) ;
			ReplaceColor( ref pressed,		ref m_Pressed		) ;
			ReplaceColor( ref selected,		ref m_Selected		) ;
			ReplaceColor( ref disabled,		ref m_Disabled		) ;

			m_FadeDuration = fadeDuration ;

			ResetState() ;
		}

		/// <summary>
		/// まとめて色を変える
		/// </summary>
		/// <param name="normal"></param>
		/// <param name="highlighted"></param>
		/// <param name="pressed"></param>
		/// <param name="selected"></param>
		/// <param name="disabled"></param>
		/// <param name="fadeDuration"></param>
		public void SetOverrideColors( Color32 color, float fadeDuration = 0.1f )
		{
			SetOverrideColors( color, color, color, color, color, fadeDuration ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ボタン状態
		/// </summary>
		public enum StateTypes
		{
			Normal		= 0,
			Highlighted	= 1,
			Pressed		= 2,
			Disabled	= 3,
			Selected	= 4,
			Finished	= 5,

			Unknown		= -1,
		}

		//-----------------------------------------------------------

		private bool	m_IsHover ;

		/// <summary>
		/// ポインターが上にあるか
		/// </summary>
		public  bool	  IsHober	=> m_IsHover ;
		
		private bool	m_IsPress ;

		/// <summary>
		/// 押されているか
		/// </summary>
		public	bool	  IsPress	=> m_IsPress ;


		private bool	m_IsClick ;

		private StateTypes m_State = StateTypes.Unknown ;

		/// <summary>
		/// 現在の状態
		/// </summary>
		public  StateTypes   State
		{
			get
			{
				return m_State ;
			}
		}

		//--------------------------------

		private CanvasRenderer	m_CanvasRenderer ;

		// ボタン
		private Button	m_Button ;
		
		// ビュー
		private UIView	m_View ;

		// クリックした瞬間での位置と大きさ
		private bool	m_Clicked ;
		private Vector2	m_Clicked_Position ;
		private Vector2 m_Clicked_Size ;

		//-----------------------------------

#if false
		private Animator m_Animator ;
#endif

		//---------------

		private bool	m_ColorDisabled ;

		private Color32	m_ColorBefore	= new ( 255, 255, 255, 255 ) ;
		private Color32	m_ColorAfter	= new ( 255, 255, 255, 255 ) ;


		private Color32 m_ColorProcess	= new ( 255, 255, 255, 255 ) ;
		private float	m_ColorProcess_BaseTime ;
		private float	m_ColorProcess_FadeDuration ;


		//-----------------------------------------------------------------
		// 色取得メソッド群

		// ノーマル色を取得する
		private Color GetNormalColor()
		{
			if( m_ColorDisabled == true )
			{
				return Color.white ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_Normal ;
			}

			return m_GeneralColor.Normal ;
		}

		// ハイライト色を取得する
		private Color GetHighlightedColor()
		{
			if( m_ColorDisabled == true )
			{
				return Color.white ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_Highlighted ;
			}

			return m_GeneralColor.Highlighted ;
		}

		// プレス色を取得する
		private Color GetPressedColor()
		{
			if( m_ColorDisabled == true )
			{
				return Color.white ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_Pressed ;
			}

			return m_GeneralColor.Pressed ;
		}

		// セレクト色を取得する
		private Color GetSelectedColor()
		{
			if( m_ColorDisabled == true )
			{
				return Color.white ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_Selected ;
			}

			return m_GeneralColor.Selected ;
		}

		// ディスエーブル色を取得する
		private Color GetDisabledColor()
		{
			if( m_ColorDisabled == true )
			{
				return Color.white ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_Disabled ;
			}

			return m_GeneralColor.Disabled ;
		}

		// 色変化時間を取得する
		private float GetFadeDuration()
		{
			if( m_ColorDisabled == true )
			{
				return 0 ;
			}

			if( m_IsOverride == true || m_GeneralColor == null )
			{
				return m_FadeDuration ;
			}

			return m_GeneralColor.FadeDuration ;
		}


		//-----------------------------------------------------------------

		// 展開時に実行される
		internal void Awake()
		{
			if( m_Button == null )
			{
				m_Button = GetComponent<Button>() ;
			}

			if( m_View == null )
			{
				m_View = GetComponent<UIView>() ;
			}

			if( m_View != null )
			{
				if( m_Button == null )
				{
					// Button ではない
					m_CanvasRenderer = GetComponent<CanvasRenderer>() ;
				}
				else
				{
					// Button である
					if( m_Button.targetGraphic != null && m_Button.targetGraphic.canvasRenderer != null )
					{
						m_CanvasRenderer = m_Button.targetGraphic.canvasRenderer ;
					}
				}

				if( m_Button != null )
				{
					// Transition が ColorTint か Animation になっていたら無効化する(SpriteSwap はタブのトグルで使用しているのでそのままにする)
					if( m_Button.transition == Selectable.Transition.ColorTint || m_Button.transition == Selectable.Transition.Animation )
					{
						m_Button.transition  = Selectable.Transition.None ;
					}
					else
					if( m_Button.transition == Selectable.Transition.SpriteSwap )
					{
						// SpriteSwap を使用している場合は色変化を無効化する
						m_ColorDisabled = true ;
					}

					if( m_Button.interactable == true )
					{
						// 有効色
						m_ColorBefore	= GetNormalColor() ;
					}
					else
					{
						// 無効ス色
						m_ColorBefore	= GetDisabledColor() ;
					}
				}
				else
				{
					m_ColorBefore	= GetNormalColor() ;
				}

				//---------------------------------------------------------

				SetCanvasRendererColor( m_ColorBefore ) ;
				ReplaceColor( ref m_ColorBefore, ref m_ColorAfter ) ;

				m_View.RemoveOnSimpleClick( OnClicked ) ;
				m_View.AddOnSimpleClick( OnClicked ) ;
			}
		}

		// 色を設定する
		private void SetCanvasRendererColor( Color color )
		{
			if( m_CanvasRenderer != null )
			{
				m_CanvasRenderer.SetColor( color ) ;
			}

			// 子にも反映させるかどうか
			if( m_View != null )
			{
				if( m_View.IsApplyColorToChildren == true )
				{
					m_View.ApplyColorToChidren( color, true ) ;
				}
			}
		}

		private void ResetState()
		{
			// レジューム時にボタンの状態を復旧させる
			m_State = StateTypes.Unknown ;

			m_IsHover = false ;
			m_IsPress = false ;
			m_IsClick = false ;

			if( m_View != null )
			{
				m_View.StopAnimator() ;
				m_View.SetScale( 1, 1 ) ;

				if( m_Button != null )
				{
					if( m_Button.interactable == true )
					{
						ChangeTransitionState( StateTypes.Normal, false, 0 ) ;
					}
					else
					{
						ChangeTransitionState( StateTypes.Disabled, false, 0 ) ;
					}
				}
				else
				{
					ChangeTransitionState( StateTypes.Normal, false, 0 ) ;
				}
			}
		}

		// 開始時に実行される
		internal void OnEnable()
		{
			if( Application.isPlaying == true )
			{
				ResetState() ;
			}
		}

		// 毎フレーム実行される
		internal void Update()
		{
			if( Application.isPlaying == true )
			{
				// 無効状態に変化した解除した事を判定して処理する
				if( m_State != StateTypes.Selected && m_State != StateTypes.Finished )
				{
					bool isDisable = false ;
					if( m_Button != null )
					{
						if( m_Button.interactable == false )
						{
							isDisable = true ;
						}
					}

					if( m_State != StateTypes.Disabled && isDisable == true )
					{
						// 有効状態から無効状態にする
						ChangeTransitionState( StateTypes.Disabled ) ;
					}
					else
					if( m_State == StateTypes.Disabled && isDisable == false )
					{
						// 無効状態から有効状態にする
						if( m_IsPress == false )
						{
							if( m_IsHover == false )
							{
								ChangeTransitionState( StateTypes.Normal ) ;
							}
							else
							{
								ChangeTransitionState( StateTypes.Highlighted ) ;
							}
						}
						else
						{
							ChangeTransitionState( StateTypes.Pressed ) ;
						}
					}
				}

				//---------------------------------------------------------

				// 色変化を処理する
				if( m_ColorProcess_BaseTime != 0 && m_ColorProcess_FadeDuration != 0 )
				{
					ProcessColorTransition() ;
				}
			}
			else
			{
				ResetState() ;
			}
		}

		//---------------------------------------------
	
		// Enter
		public void OnPointerEnter( PointerEventData pointer )
		{
			// → Release 状態であれば Highlight へ遷移

			if( m_State == StateTypes.Normal )
			{
				ChangeTransitionState( StateTypes.Highlighted ) ;
			}

//			Debug.Log( "Enter:" + name ) ;
			m_IsHover = true ;
		}

		// Exit
		public void OnPointerExit( PointerEventData pointer )
		{
			// → Release 状態であれば Normal へ遷移
			if( m_State == StateTypes.Highlighted )
			{
				ChangeTransitionState( StateTypes.Normal ) ;
			}

//			Debug.Log( "Exit:" + name ) ;
			m_IsHover = false ;
		}

		// Down
		public void OnPointerDown( PointerEventData pointer )
		{
			// → Press 状態へ遷移
			if( m_State == StateTypes.Normal || m_State == StateTypes.Highlighted )
			{
				ChangeTransitionState( StateTypes.Pressed ) ;
			}

//			Debug.Log( "<color=#FF7F00>[Press]:" + name + "</color>" ) ;
			m_IsPress = true ;

			//----------------------------------

			if( m_View != null && m_EffectType != Ripple.ButtonEffectTypes.None )
			{
				// プレスした時点での座標と大きさを記録する
//				Vector2 position	= m_View.PositionInCanvas ;
				Vector2 position	= m_View.GetPositionIn<ScreenSizeFitter>() ;
				Vector2 size		= m_View.Size ;

				// 補正は不要
//				Vector2 pivot		= m_View.Pivot ;
//				position.x += ( size.x * ( 0.5f - pivot.x ) ) ; 
//				position.y += ( size.y * ( 0.5f - pivot.y ) ) ; 

				m_Clicked			= true ;
				m_Clicked_Position	= position ;
				m_Clicked_Size		= size ;
			}
		}

		// Up
		public void OnPointerUp( PointerEventData pointer )
		{
			// → Enter 状態であれば Highlight へ遷移
			// → Exit  状態であれば Normal へ遷移

			if( m_State == StateTypes.Pressed )
			{
				if( m_IsHover == false )
				{
					ChangeTransitionState( StateTypes.Normal ) ;
				}
				else
				{
					ChangeTransitionState( StateTypes.Highlighted ) ;
				}
			}

//			Debug.Log( "<color=#00FFFF>[Release]:" + name + "</color>" ) ;
			m_IsPress = false ;
		}

		// Clicked(UIButton から呼び出される)
		private void OnClicked()
		{
			if( m_State != StateTypes.Disabled && m_IsClick == false )
			{
				// 押されたボタンを同階層で最も手前に表示する
//				if( transform.parent != null )
//				{
//					UIView parent = transform.parent.GetComponent<UIView>() ;
//					if( parent != null )
//					{
//						Vector3 p =	transform.localPosition ;
//						p.z = 10 ;
//						transform.localPosition = p ;
//						parent.SortChildByZ() ;
//					}
//				}

				//---------------------------------

				m_IsClick = true ;
				ChangeTransitionState( StateTypes.Selected ) ;
				m_IsPress = false ;
			}
		}

		//-------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------
		// アニメーター用

		// トランジジョンの状態を変える
		private bool ChangeTransitionState( StateTypes state, bool animationEnabled = true, float fadeDuration = -1 )
		{
			if( m_View == null )
			{
				// 以下は処理できない
				return false ;
			}

			//----------------------------------------------------------

			if( m_State == state || gameObject.activeSelf == false )
			{
				// 状態が同じなので処理しない
				return true ;
			}

			m_State			= state ;

			//----------------------------------------------------------
			// アニメーション

			if( animationEnabled == true )
			{
				if( m_AnimatonEnabled == true && m_View.CAnimator != null )
				{
					// アニメーションは有効
					if( m_State == StateTypes.Normal )
					{
						m_View.PlayAnimator( "Normal" ) ;
					}
					else
					if( m_State == StateTypes.Highlighted )
					{
						m_View.PlayAnimator( "Highlighted" ) ;
					}
					else
					if( m_State == StateTypes.Pressed )
					{
						m_View.PlayAnimator( "Pressed" ) ;
					}
					else
					if( m_State == StateTypes.Disabled )
					{
						m_View.PlayAnimator( "Disabled" ) ;
					}
					else
					if( m_State == StateTypes.Selected )
					{
						m_View.PlayAnimator( "Selected", onFinished:( bool state ) =>
						{
							// アニメションが終了したら通常状態へ戻す(エフェクトを表示しないケース対策)
							m_IsClick = false ;

							m_State = StateTypes.Normal ;
							m_View.PlayAnimator( "Normal" ) ;
						} ) ;
					}
				}
				else
				{
					// アニメーションは無効
					if( m_State == StateTypes.Selected )
					{
						PlayEffect() ;

						// アニメションが終了したら通常状態へ戻す(エフェクトを表示しないケース対策)
						m_IsClick = false ;

						m_State = StateTypes.Normal ;
					}
				}
			}
			else
			{
				// 強制的に１倍にするだけ
				m_View.SetScale( 1, 1 ) ;
			}

			//----------------------------------------------------------
			// 色変化

			if( m_ColorDisabled == false )
			{
				if( m_State == StateTypes.Normal )
				{
					m_ColorAfter = GetNormalColor() ;
				}
				else
				if( m_State == StateTypes.Highlighted )
				{
					m_ColorAfter = GetHighlightedColor() ;
				}
				else
				if( m_State == StateTypes.Pressed )
				{
					m_ColorAfter = GetPressedColor() ;
				}
				else
				if( m_State == StateTypes.Disabled )
				{
					m_ColorAfter = GetDisabledColor() ;
				}
				else
				if( m_State == StateTypes.Selected )
				{
					m_ColorAfter = GetSelectedColor() ;
				}

				if( fadeDuration <  0 )
				{
					fadeDuration = GetFadeDuration() ;
				}

				if( fadeDuration == 0 )
				{
					m_ColorProcess_BaseTime			= 0 ;
					m_ColorProcess_FadeDuration		= 0 ;

					ReplaceColor( ref m_ColorAfter, ref m_ColorBefore ) ;

					SetCanvasRendererColor( m_ColorAfter ) ;
				}
				else
				{
					// 時間経過で色を変化させる
					if( m_ColorProcess_BaseTime != 0 || m_ColorProcess_FadeDuration != 0 )
					{
						// 現在変化の途中
						ReplaceColor( ref m_ColorProcess, ref m_ColorBefore ) ;

						m_ColorProcess_BaseTime			= 0 ;
						m_ColorProcess_FadeDuration		= 0 ;
					}

					if( CompareColor( m_ColorBefore, m_ColorAfter ) == false )
					{
						// 色値が異なるので時間で変化させる
						m_ColorProcess_BaseTime			= Time.realtimeSinceStartup ;
						m_ColorProcess_FadeDuration		= fadeDuration ;
					}
				}
			}

			//----------------------------------------------------------

			return true ;
		}

		/// <summary>
		/// アニメーションによるイベント
		/// </summary>
		/// <param name="idntity"></param>
		public void OnAnimationEvent( string _ )
		{
//			Debug.Log( "アニメーション発火:" + identity ) ;
			PlayEffect() ;
		}

		// クリックエフェクトを表示する
		private void PlayEffect()
		{
			if( m_View != null && m_EffectType != Ripple.ButtonEffectTypes.None )
			{
				if( m_Clicked == true )
				{
					m_Clicked = false ;
					Ripple.PlayButtonEffect( m_EffectType, m_Clicked_Position, m_Clicked_Size, m_OnEffectFinished ) ;
				}
			}
		}


		// 色変化を処理する
		private void ProcessColorTransition()
		{
			float deltaTime = Time.realtimeSinceStartup - m_ColorProcess_BaseTime ;

			if( deltaTime >  m_ColorProcess_FadeDuration )
			{
				deltaTime  = m_ColorProcess_FadeDuration ;
			}

			float factor = deltaTime / m_ColorProcess_FadeDuration ;

			m_ColorProcess = Color.Lerp( m_ColorBefore, m_ColorAfter, factor ) ;

			SetCanvasRendererColor( m_ColorProcess ) ;

			if( deltaTime == m_ColorProcess_FadeDuration )
			{
				// 色変化を終了させる
				ReplaceColor( ref m_ColorProcess, ref m_ColorBefore ) ;

				m_ColorProcess_BaseTime		= 0 ;
				m_ColorProcess_FadeDuration	= 0 ;
			}
		}



		// ２つの色値が等しいか判定する
		private bool CompareColor( Color c0, Color c1 )
		{
			return ( c0.r == c1.r && c0.g == c1.g && c0.b == c1.b && c0.a == c1.a ) ;
		}

		// 色を上書きする
		private void ReplaceColor( ref Color32 c0, ref Color32 c1 )
		{
			c1.r = c0.r ;
			c1.g = c0.g ;
			c1.b = c0.b ;
			c1.a = c0.a ;
		}

		//-------------------------------------------------------------------------------------------------------------------
//#if false
		// ボタン状態復帰用のフラグ
		private bool m_IsLostFocusForButton = false ;

		internal void OnApplicationFocus( bool isFocus )
		{
			if( isFocus == false )
			{
				m_IsLostFocusForButton = true ;
			}
			else
			if( isFocus == true && m_IsLostFocusForButton == true )
			{
//				Debug.Log( "[ButtonTransition] OnApplicationFocus Path = " + name + " isFocus = " + isFocus + " UIButton = " + m_UIButton ) ;

				//---------------------------------

				m_IsLostFocusForButton = false ;

				//---------------------------------------------------------

				ResetState() ;
			}
		}
//#endif

		//-------------------------------------------------------------------------------------------------------------------
		// メニュー
#if UNITY_EDITOR

		private const string m_AnimationControllerPath	= "Assets/Application/ReferencedAssets/Animations/_00_Framework/Buttons/ButtonTransition/Default.controller" ;

//		private const string m_PositiveButtonEffectPath	= "Assets/Application/ReferencedAssets/Prefabs/_01_Screen/_00_General/PositiveButtonEffect.prefab" ;
//		private const string m_NegativeButtonEffectPath	= "Assets/Application/ReferencedAssets/Prefabs/_01_Screen/_00_General/NegativeButtonEffect.prefab" ;

		//-----------------------------------

		[MenuItem( "ButtonTransition/AddComponent" )]
		private static void ButtonTansition_AddComponent()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( go.TryGetComponent<uGUIHelper.UITransition>( out var _ ) == true )
			{
				EditorUtility.DisplayDialog( "警告", "ButtonTransition を追加するには\nUITransition を削除する必要があります", "閉じる" ) ;
				return ;
			}

			//----------------------------------

			Undo.RecordObject( go, "Add a ButtonTransition" ) ;	// アンドウバッファに登録

			if( go.TryGetComponent<ButtonTransition>( out var buttonTransition ) == false )
			{
				buttonTransition = go.AddComponent<ButtonTransition>() ;
				if( buttonTransition == null )
				{
					return ;	// エラー
				}
			}

			//----------------------------------
			// Animator

			if( buttonTransition.TryGetComponent<Animator>( out var animator ) == false )
			{
				animator = go.AddComponent<Animator>() ;
			}

			if( animator != null )
			{
				if( animator.runtimeAnimatorController == null )
				{
					var animationController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>( m_AnimationControllerPath ) ;
					if( animationController != null )
					{
						animator.runtimeAnimatorController = animationController ;
					}
				}
			}

			buttonTransition.m_EffectType = Ripple.ButtonEffectTypes.None ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		[MenuItem( "ButtonTransition/AddComponent And PositiveEffect" )]
		private static void ButtonTansition_AddComponentAndPositiveEffect()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( go.TryGetComponent<uGUIHelper.UITransition>( out var _ ) == true )
			{
				EditorUtility.DisplayDialog( "警告", "ButtonTransition を追加するには\nUITransition を削除する必要があります", "閉じる" ) ;
				return ;
			}

			//----------------------------------

			Undo.RecordObject( go, "Add a ButtonTransition" ) ;	// アンドウバッファに登録

			if( go.TryGetComponent<ButtonTransition>( out var buttonTransition ) == false )
			{
				buttonTransition = go.AddComponent<ButtonTransition>() ;
				if( buttonTransition == null )
				{
					return ;	// エラー
				}
			}

			//----------------------------------
			// Animator

			if( buttonTransition.TryGetComponent<Animator>( out var animator ) == false )
			{
				animator = go.AddComponent<Animator>() ;
			}

			if( animator != null )
			{
				if( animator.runtimeAnimatorController == null )
				{
					var animationController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>( m_AnimationControllerPath ) ;
					if( animationController != null )
					{
						animator.runtimeAnimatorController = animationController ;
					}
				}
			}

			buttonTransition.m_EffectType = Ripple.ButtonEffectTypes.Positive ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		[ MenuItem( "ButtonTransition/AddComponent And NegativeEffect" ) ]
		private static void ButtonTansition_AddComponentAndNegativeEffect()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( go.TryGetComponent<uGUIHelper.UITransition>( out var _ ) == true )
			{
				EditorUtility.DisplayDialog( "警告", "ButtonTransition を追加するには\nUITransition を削除する必要があります", "閉じる" ) ;
				return ;
			}

			//----------------------------------

			Undo.RecordObject( go, "Add a ButtonTransition" ) ;	// アンドウバッファに登録

			if( go.TryGetComponent<ButtonTransition>( out var buttonTransition ) == false )
			{
				buttonTransition = go.AddComponent<ButtonTransition>() ;
				if( buttonTransition == null )
				{
					return ;	// エラー
				}
			}

			//----------------------------------
			// Animator

			if( buttonTransition.TryGetComponent<Animator>( out var animator ) == false )
			{
				animator = go.AddComponent<Animator>() ;
			}

			if( animator != null )
			{
				if( animator.runtimeAnimatorController == null )
				{
					var animationController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>( m_AnimationControllerPath ) ;
					if( animationController != null )
					{
						animator.runtimeAnimatorController = animationController ;
					}
				}
			}

			buttonTransition.m_EffectType = Ripple.ButtonEffectTypes.Negative ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

#endif
		//-------------------------------------------------------------------------------------------

		// ボタン全体で共通の色変化情報
		protected static ButtonTransitionColor m_GeneralColor ;

		/// <summary>
		/// 準備を行う
		/// </summary>
		/// <returns></returns>
		public static bool Prepare()
		{
			// ボタン全体で共通の色変化情報を読み出す
			string path = "ScriptableObjects/ButtonTransitionColor" ;
			m_GeneralColor = Resources.Load<ButtonTransitionColor>( path ) ;

			return ( m_GeneralColor != null ) ;
		}
	}
}
