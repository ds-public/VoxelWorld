using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;


#if UNITY_EDITOR
using UnityEditorInternal ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Button クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Button ) ) ]
	public class UIButton : UIImage
	{
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				Button button = CButton ;
				if( button == null )
				{
					return false ;
				}

				bool state = button.interactable ;

				if( m_FakeInvalidation == true )
				{
					state = m_InteractableOfFake ;
				}

				return state ;
			}
			set
			{
				Button button = CButton ;
				if( button == null )
				{
					return ;
				}

				m_InteractableOfFake = value ;

				bool state = value ;

				if( m_FakeInvalidation == true )
				{
					// 暗化でもクリックできる
					state = true ;

					SetFakeColors() ;
				}

				button.interactable = state ;

				ProcessApplyColorToChildren( true, true ) ;
			}
		}

		// 有効無効の希望設定値
		protected bool m_InteractableOfFake ;

		/// <summary>
		/// 有効無効の希望設定値
		/// </summary>
		public    bool   InteractableOfFake => m_InteractableOfFake ;

		//---------------

		// 暗化でもボタンが押せる対応
		protected bool		m_FakeInvalidation = false ;

		/// <summary>
		/// 暗化でもボタンが押せる対応
		/// </summary>
		public    bool		  FakeInvalidation => m_FakeInvalidation ;

		// 暗化でもボタンが押せる際のクリックされた際のコールバック
		protected Action	m_OnClickOfFake = null ;

		// 暗化でもボタンが押せる際の色
		protected Color		m_DisableColorOfFake = new Color32( 127, 127, 127, 255 ) ;

		/// <summary>
		/// 暗化でもボタンが押せる際の色
		/// </summary>
		public    Color		  DisableColorOfFake => m_DisableColorOfFake ;

		// 暗化でもボタンが押せる際にトランジションを行うかどうか
		protected bool		m_EnableTransitionOfFake = true ;

		/// <summary>
		/// 暗化でもボタンが押せる際にトランジションを行うかどうか
		/// </summary>
		public    bool		  EnableTransitionOfFake => m_EnableTransitionOfFake ;


		//---------------
		// Color Tint のバックアップ

		protected Color		m_Backup_NormalColor ;
		protected Color		m_Backup_HighlightColor ;
		protected Color		m_Backup_PressedColor ;
		protected Color		m_Backup_SelectedColor ;
		protected Color		m_Backup_DisableColor ;


		/// <summary>
		/// 暗化でもボタンがおせるモードを有効化する
		/// </summary>
		/// <param name="onClick"></param>
		/// <param name="disableColor"></param>
		/// <param name="enableTransition"></param>
		public void EnableFakeInvalidation( Action onClickOfFake, uint disableColorOfFake = 0xFF8F8F8F, bool enableTransitionOfFake = true )
		{
			Color32 color = new Color32
			(
				( byte )( ( disableColorOfFake >> 16 ) & 0xFF ),
				( byte )( ( disableColorOfFake >>  8 ) & 0xFF ),
				( byte )(   disableColorOfFake         & 0xFF ),
				( byte )( ( disableColorOfFake >> 24 ) & 0xFF )
			) ;

			EnableFakeInvalidation( onClickOfFake, color, enableTransitionOfFake ) ;
		}

		/// <summary>
		/// 暗化でもボタンがおせるモードを有効化する
		/// </summary>
		/// <param name="onClick"></param>
		/// <param name="disableColor"></param>
		/// <param name="enableTransition"></param>
		public void EnableFakeInvalidation( Action onClickOfFake, Color disableColorOfFake, bool enableTransitionOfFake = true )
		{
			if( m_FakeInvalidation == true )
			{
				// 既にモードが変わっている
				return ;
			}

			//----------------------------------------------------------

			m_FakeInvalidation			= true ;

			m_OnClickOfFake				= onClickOfFake ;
			m_DisableColorOfFake		= disableColorOfFake ;
			m_EnableTransitionOfFake	= enableTransitionOfFake ;

			//----------------------------------

			if( CButton != null )
			{
				// 色の退避
				var colors = CButton.colors ;

				m_Backup_NormalColor		= colors.normalColor ;
				m_Backup_HighlightColor		= colors.highlightedColor ;
				m_Backup_PressedColor		= colors.pressedColor ;
				m_Backup_SelectedColor		= colors.selectedColor ;
				m_Backup_DisableColor		= colors.disabledColor ;
			}

			Interactable				= m_InteractableOfFake ;
		}

		/// <summary>
		/// 暗化でもボタンが押せるモードを無効化する
		/// </summary>
		public void DisableFakeInvalidation()
		{
			if( m_FakeInvalidation == false )
			{
				// モードが変わっていない
				return ;
			}

			//----------------------------------------------------------

			m_FakeInvalidation			= false ;

			if( CButton != null )
			{
				// 色の復帰
				var colors = CButton.colors ;

				colors.normalColor			= m_Backup_NormalColor ;
				colors.highlightedColor		= m_Backup_HighlightColor ;
				colors.pressedColor			= m_Backup_PressedColor;
				colors.selectedColor		= m_Backup_SelectedColor ;
				colors.disabledColor		= m_Backup_DisableColor ;

				CButton.colors = colors ;
			}

			//----------------------------------

			Interactable				= m_InteractableOfFake ;
		}

		protected void SetFakeColors()
		{
			if( m_FakeInvalidation == false )
			{
				return ;
			}

			if( m_InteractableOfFake == true )
			{
				// 有効状態
				if( CButton != null )
				{
					var colors = CButton.colors ;

					colors.normalColor			= m_Backup_NormalColor ;
					colors.highlightedColor		= m_Backup_HighlightColor ;
					colors.pressedColor			= m_Backup_PressedColor;
					colors.selectedColor		= m_Backup_SelectedColor ;
					colors.disabledColor		= m_Backup_DisableColor ;

					CButton.colors = colors ;
				}
			}
			else
			{
				// 無効状態
				if( CButton != null )
				{
					// 色の退避
					var colors = CButton.colors ;

					colors.normalColor			= m_DisableColorOfFake ;
					colors.highlightedColor		= m_DisableColorOfFake ;
					colors.pressedColor			= m_DisableColorOfFake ;
					colors.selectedColor		= m_DisableColorOfFake ;
					colors.disabledColor		= m_DisableColorOfFake ;

					CButton.colors = colors ;
				}
			}
		}

		//-----------------------------------

		/// <summary>
		/// ラベルのビューのインスタンス(キャッシュ)
		/// </summary>
		public    UIText		  Label{ get{ return m_Label ; } set{ m_Label = value ; } }
		[SerializeField]
		protected UIText		m_Label ;
		
		/// <summary>
		/// リッチラベルのビューのインスタンス(キャッシュ)
		/// </summary>
		public    UIRichText	  RichLabel{ get{ return m_RichLabel ; } set{ m_RichLabel = value ; } }
		[SerializeField]
		protected UIRichText	m_RichLabel ; 

		/// <summary>
		/// ラベルメッシュのビューのインスタンス(キャッシュ)
		/// </summary>
		public    UITextMesh	  LabelMesh{ get{ return m_LabelMesh ; } set{ m_LabelMesh = value ; } }
		[SerializeField]
		protected UITextMesh	m_LabelMesh ;

		/// <summary>
		/// ラベルのテキスト
		/// </summary>
		public string LabelText
		{
			get
			{
				if( m_Label != null )
				{
					return m_Label.Text ;
				}
				if( m_RichLabel != null )
				{
					return m_RichLabel.Text ;
				}
				if( m_LabelMesh != null )
				{
					return m_LabelMesh.Text ;
				}
				return null ;	
			}
			set
			{
				if( m_Label != null )
				{
					m_Label.Text = value ;
				}
				if( m_RichLabel != null )
				{
					m_RichLabel.Text = value ;
				}
				if( m_LabelMesh != null )
				{
					m_LabelMesh.Text = value ;
				}
			}
		}

		/// <summary>
		/// テキスト自体の横幅
		/// </summary>
		public float LabelTextWidth
		{
			get
			{
				if( m_Label != null )
				{
					return m_Label.TextSize.x ;
				}
				if( m_RichLabel != null )
				{
					return m_RichLabel.TextSize.x ;
				}
				if( m_LabelMesh != null )
				{
					return m_LabelMesh.TextSize.x ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// テキスト自体の縦幅
		/// </summary>
		public float LabelTextHeight
		{
			get
			{
				if( m_Label != null )
				{
					return m_Label.TextSize.y ;
				}
				if( m_RichLabel != null )
				{
					return m_RichLabel.TextSize.y ;
				}
				if( m_LabelMesh )
				{
					return m_LabelMesh.TextSize.y ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// アイコン画像
		/// </summary>
		public    UIImage	  Icon{ get{ return m_Icon ; } set{ m_Icon = value ; } }
		[SerializeField]
		protected UIImage	m_Icon ;

		/// <summary>
		/// 無効化状態の際のマスク画像
		/// </summary>
		public    UIImage	  DisableMask{ get{ return m_DisableMask ; } set{ m_DisableMask = value ; } }
		[SerializeField]
		protected UIImage	m_DisableMask ;

		/// <summary>
		/// クリック時のトランジションを有効にするかどうか
		/// </summary>
		public bool ClickTransitionEnabled{ get{ return m_ClickTransitionEnabled ; } set{ m_ClickTransitionEnabled = value ; } }
		[SerializeField]
		protected bool m_ClickTransitionEnabled = false ;

		/// <summary>
		/// クリック時のトランジションを待ってからクリックのコールバックを呼び出す
		/// </summary>
		public bool WaitForTransition{ get{ return m_WaitForTransition ; } set{ m_WaitForTransition = value ; } }
		[SerializeField]
		protected bool m_WaitForTransition = false ;

		/// <summary>
		/// ピボットを自動的に実行時に中心にする
		/// </summary>
		public bool AutoPivotToCenter{ get{ return m_AutoPivotToCenter ; } set{ m_AutoPivotToCenter = value ; } }
		[SerializeField]
		protected bool m_AutoPivotToCenter = false ;

		/// <summary>
		/// ボタングルーブ(同じボタングループを設定むしたボタン間で状態を共有する)
		/// </summary>
		public    UIButtonGroup   TargetButtonGroup{ get{ return m_TargetButtonGroup ; } set{ m_TargetButtonGroup = value ; } }
		[SerializeField]
		protected UIButtonGroup m_TargetButtonGroup = null ;

		//-----------------------------------------------------
	
		protected bool	m_IsButtonClicked = false ;

		public bool IsButtonClicked
		{
			get
			{
				return m_IsButtonClicked ;
			}
		}

		protected int m_IsButtonClickedCountTime = 0 ;

		private bool	m_PreviousInteractable ;

		//-----------------------------------------------------------


		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Image image = CImage ;

			if( image == null )
			{
				image = gameObject.AddComponent<Image>() ;
			}
			if( image == null )
			{
				// 異常
				return ;
			}

			Button button = CButton ;

			if( button == null )
			{
				button = gameObject.AddComponent<Button>() ;
			}
			if( button == null )
			{
				// 異常
				return ;
			}

#if UNITY_EDITOR
			// Image コンポーネントを一番上にもってくる
			while( ComponentUtility.MoveComponentUp( image ) ){}
#endif
			//---------------------------------

			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				SetSize( size.y * 0.25f, size.y * 0.075f ) ;
			}
			
			ColorBlock colorBlock ;

			colorBlock = button.colors ;
			colorBlock.fadeDuration = 0.2f ;
			button.colors = colorBlock ;
				
			// Image

			Sprite	defaultSprite	= null ;
			Color	defaultColor	= button.colors.disabledColor ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultSprite		= ds.ButtonFrame ;
					defaultColor		= ds.ButtonDisabledColor ;
				}
			}
			
#endif

			if( defaultSprite == null )
			{
				image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			}
			else
			{
				image.sprite = defaultSprite ;
			}

			colorBlock = button.colors ;
			colorBlock.disabledColor = defaultColor ;
			button.colors = colorBlock ;

			image.color = Color.white ;
			image.type = Image.Type.Sliced ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------

			// トランジションを追加
			IsTransition = true ;

			ResetRectTransform() ;
		}

		// 派生クラスの Awake
		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( m_TargetButtonGroup != null )
			{
				// ボタングループが有効な場合に自身を登録する
				m_TargetButtonGroup.Add( this ) ;

				IsInteraction = true ;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable() ;

			bool state = false ;
			if( CButton != null )
			{
				state = CButton.interactable ;
			}
			m_PreviousInteractable = state ;

			// 子に色を反映させる処理の事前準備を行う(全ての子よりも先に処理しなければならないため Awake のタイミングで実行する)
			ProcessApplyColorToChildren( false, false ) ;
		}

		/// <summary>
		/// 子にもボタンの状態による色変化を強制的に反映させる
		/// </summary>
		/// <param name="isForce"></param>
		protected void ProcessApplyColorToChildren( bool isForce, bool withMyself )
		{
			if( isForce == true )
			{
				m_RefreshChildrenColor = true ;
			}

			// 有効状態の保存
			if( m_IsApplyColorToChildren == true )
			{
				if( m_EffectiveColorReplacing == true )
				{
					if( CButton != null && CButton.targetGraphic != null && CButton.targetGraphic.canvasRenderer != null )
					{
						if( CButton.colors.fadeDuration == 0 )
						{
							// 一瞬で切り替わる
							Color color ;
							if( CButton.interactable == false )
							{
								color = CButton.colors.disabledColor ;
							}
							else
							{
								color = CButton.colors.normalColor ;
							}

							ApplyColorToChidren( color, withMyself ) ;
						}
						else
						{
							// 通常の切り替え
							Color color ;
							if( withMyself == false )
							{
								color = CButton.targetGraphic.canvasRenderer.GetColor() ;
							}
							else
							{
								// 自身も含めて色を変えるケースは特殊(自身の色も強制的に変えて即時反映させたい場合)
								if( CButton.interactable == false )
								{
									color = CButton.colors.disabledColor ;
								}
								else
								{
									color = CButton.colors.normalColor ;
								}
							}

							ApplyColorToChidren( color, withMyself ) ;
						}
					}
				}
				else
				{
					// UIButton の場合、Transition Color の影響を受けるため、自身の影響色は設定できない。
					// 自身の色は、Color で変える必要がある。※Transition Color は全て白にする。
					ApplyColorToChidren( m_EffectiveColor, withMyself ) ;
				}
			}
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( CButton != null )
				{
					if( m_FakeInvalidation == false )
					{
						m_InteractableOfFake = CButton.interactable ;
					}

					CButton.onClick.AddListener( OnButtonClickInner ) ;

					if( AutoPivotToCenter == true )
					{
						SetPivot( 0.5f, 0.5f, true ) ;	
					}

					// 色の強制反映
//					SetFakeColors() ;
					ProcessApplyColorToChildren( true, true ) ;
				}
			}
		}

		// 更新
		override protected void OnUpdate()
		{
//			base.OnUpdate() ;	// UIImage の OnUpdate() は実行してはいけない

			if( m_IsButtonClicked == true && m_IsButtonClickedCountTime != Time.frameCount )
			{
				m_IsButtonClicked = false ;
			}

			//----------------------------------------------------------

			bool state = false ;
			if( CButton != null )
			{
				state = CButton.interactable ;
			}

			// 有効状態の変化を監視する
			if( m_PreviousInteractable != state )
			{
				m_PreviousInteractable  = state ;
				if( DisableMask != null )
				{
					DisableMask.SetActive( ! state ) ;
				}

				m_RefreshChildrenColor = true ;
			}

			//----------------------------------------------------------

			ProcessApplyColorToChildren( false, false ) ;
		}

		//-------------------------------------------------------------------------------------------

		// Down(ButtonGroup用)
		override protected void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerDownBasic( pointer, fromScrollView ) ;

			if( m_TargetButtonGroup != null )
			{
				m_TargetButtonGroup.SetState( this, Interactable, true ) ;
			}
		}

		override protected void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_TargetButtonGroup != null )
			{
				// ボタングループが有効な場合に自身の登録を抹消する
				m_TargetButtonGroup.Remove( this ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Normal

		/// <summary>
		/// ボタンをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIButton> OnButtonClickAction ;
		
		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="button">ビューのインスタンス</param>
		public delegate void OnButtonClick( string identity, UIButton button ) ;

		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnButtonClick OnButtonClickDelegate ;

		//-----------------------------------

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onButtonClickAction">アクションメソッド</param>
		public void SetOnButtonClick( Action<string, UIButton> onButtonClickAction, float repeatPressDecissionTime = 0, float repeatPressIntervalTime = 0 )
		{
			if( repeatPressDecissionTime <= 0 || repeatPressIntervalTime <= 0 )
			{
				// ノーマル
				OnButtonClickAction = onButtonClickAction ;
				SetOnRepeatPressInButton( null, 0, 0 ) ;
			}
			else
			{
				// リピート
				OnButtonClickAction = null ;
				SetOnRepeatPressInButton( onButtonClickAction, repeatPressDecissionTime, repeatPressIntervalTime ) ;
			}
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onButtonClickDelegate">デリゲートメソッド</param>
		public void AddOnButtonClick( OnButtonClick onButtonClickDelegate, float repeatPressDecissionTime = 0, float repeatPressIntervalTime = 0 )
		{
			if( repeatPressDecissionTime <= 0 || repeatPressIntervalTime <= 0 )
			{
				// ノーマル
				OnButtonClickDelegate += onButtonClickDelegate ;
				RemoveOnRepeatPressInButton( onButtonClickDelegate ) ;
			}
			else
			{
				// リピート
				OnButtonClickDelegate -= onButtonClickDelegate ;
				AddOnRepeatPressInButton( onButtonClickDelegate, repeatPressDecissionTime, repeatPressIntervalTime ) ;
			}
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onButtonClickDelegate">デリゲートメソッド</param>
		public void RemoveOnButtonClick( OnButtonClick onButtonClickDelegate )
		{
			OnButtonClickDelegate -= onButtonClickDelegate ;
			RemoveOnRepeatPressInButton( onButtonClickDelegate ) ;
		}

		//-----------------------------------
		// リピート版の制御

		protected Action<string, UIButton> m_OnButtonClickActionForRepeatPress ;
		protected OnButtonClick m_OnButtonClickDelegateForRepeatPress ;

		// 設定
		protected void SetOnRepeatPressInButton( Action<string, UIButton> onButtonClickAction, float repeatPressDecissionTime, float repeatPressIntervalTime )
		{
			if( onButtonClickAction != null )
			{
				IsInteraction = true ;
				m_OnButtonClickActionForRepeatPress = onButtonClickAction ;
				RemoveOnRepeatPress( OnRepeatPressInner ) ;
				AddOnRepeatPress( OnRepeatPressInner, repeatPressDecissionTime, repeatPressIntervalTime ) ;
			}
			else
			{
				m_OnButtonClickActionForRepeatPress = null ;
				RemoveOnRepeatPress( OnRepeatPressInner ) ;
			}
		}

		// 追加
		protected void AddOnRepeatPressInButton( OnButtonClick onButtonClickAction, float repeatPressDecissionTime, float repeatPressIntervalTime )
		{
			if( onButtonClickAction != null )
			{
				IsInteraction = true ;
				m_OnButtonClickDelegateForRepeatPress += onButtonClickAction ;
				RemoveOnRepeatPress( OnRepeatPressInner ) ;
				AddOnRepeatPress( OnRepeatPressInner, repeatPressDecissionTime, repeatPressIntervalTime ) ;
			}
			else
			{
				m_OnButtonClickDelegateForRepeatPress -= onButtonClickAction ;
				RemoveOnRepeatPress( OnRepeatPressInner ) ;
			}
		}

		// 削除
		protected void RemoveOnRepeatPressInButton( OnButtonClick onButtonClickAction )
		{
			if( onButtonClickAction != null )
			{
				m_OnButtonClickDelegateForRepeatPress -= onButtonClickAction ;
				RemoveOnRepeatPress( OnRepeatPressInner ) ;
			}
		}

		// リピートボタン化した際のコールバック
		protected void OnRepeatPressInner( string identity, UIView view, int repeatCount )
		{
			if( Interactable == false )
			{
				return ;	// Button の Interactable は無視してコールバックは発生するので Interactable チェックが必要
			}

			m_OnButtonClickActionForRepeatPress?.Invoke( identity, view as UIButton ) ;
			m_OnButtonClickDelegateForRepeatPress?.Invoke( identity, view as UIButton ) ;
		}

		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// ボタンクリックを強制的に実行する
		/// </summary>
		public void ExecuteButtonClick()
		{
			OnButtonClickInner() ;
		}

		// 内部リスナー
		private void OnButtonClickInner()
		{
			//----------------------------------
			// このクリックが有効か判定する

			if( CanClickExecution() == false )
			{
				// 無効
				return ;
			}

			//----------------------------------

			if( m_FakeInvalidation == false || ( m_FakeInvalidation == true && m_InteractableOfFake == true ) )
			{
				if( ClickTransitionEnabled == false || ( ClickTransitionEnabled == true && WaitForTransition == false ) )
				{
					m_IsButtonClicked = true ;
					m_IsButtonClickedCountTime = Time.frameCount ;

					if( IsInteraction == false && IsInteractionForScrollView == false )
					{
						// IsInteraction が true だと UIView の方が反応してしまうので IsInteraction が false の場合のみ呼び出す
						if( OnSimpleClickAction != null || OnSimpleClickDelegate != null )
						{
							OnSimpleClickAction?.Invoke() ;
							OnSimpleClickDelegate?.Invoke() ;
						}
					}

					if( OnButtonClickAction != null || OnButtonClickDelegate != null )
					{
						string identity = Identity ;
						if( string.IsNullOrEmpty( identity ) == true )
						{
							identity = name ;
						}
	
						OnButtonClickAction?.Invoke( identity, this ) ;
						OnButtonClickDelegate?.Invoke( identity, this ) ;
					}
				}

				if( ClickTransitionEnabled == true )
				{
					UITransition transition = CTransition ;
					if( transition != null )
					{
						transition.OnClicked( WaitForTransition ) ;
					}
				}
			}
			else
			if( m_FakeInvalidation == true && m_InteractableOfFake == false )
			{
				// 暗化でもボタンが押せる際のコールバックを呼ぶ
				m_OnClickOfFake?.Invoke() ;
			}
		}
		
		internal protected void OnButtonClickFromTransition()
		{
			if( WaitForTransition == true )
			{
				//----------------------------------
				// このクリックが有効か判定する

				if( CanClickExecution() == false )
				{
					// 無効
					return ;
				}

				//----------------------------------

				m_IsButtonClicked = true ;
				m_IsButtonClickedCountTime = Time.frameCount ;

				if( IsInteraction == false )
				{
					// IsInteraction が true だと UIView の方が反応してしまうので IsInteraction が false の場合のみ呼び出す
					if( OnSimpleClickAction != null || OnSimpleClickDelegate != null )
					{
						OnSimpleClickAction?.Invoke() ;
						OnSimpleClickDelegate?.Invoke() ;
					}
				}

				if( OnButtonClickAction != null || OnButtonClickDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}
	
					OnButtonClickAction?.Invoke( identity, this ) ;
					OnButtonClickDelegate?.Invoke( identity, this ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを追加する
		/// </summary>
		/// <param name="onClick">リスナーメソッド</param>
		public void AddOnClickListener( UnityAction onClick )
		{
			Button button = CButton ;
			if( button != null )
			{
				button.onClick.AddListener( onClick ) ;
			}
		}
		
		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを削除する
		/// </summary>
		/// <param name="onClick">リスナーメソッド</param>
		public void RemoveOnClickListener( UnityAction onClick )
		{
			Button button = CButton ;
			if( button != null )
			{
				button.onClick.RemoveListener( onClick ) ;
			}
		}

		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを全て削除する
		/// </summary>
		public void RemoveOnClickAllListeners()
		{
			Button button = CButton ;
			if( button != null )
			{
				button.onClick.RemoveAllListeners() ;
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="text">ラベルの文字列</param>
		/// <param name="color">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIText AddLabel( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( Label == null )
			{
				Label = AddView<UIText>() ;
				Label.name = "Label" ;
			}

			UIText label = Label ;

			//----------------------------------

			if( fontSize <= 0 )
			{
				fontSize  = ( int )( Size.y * 0.6f ) ;
			}

			label.Alignment = TextAnchor.MiddleCenter ;
		
			label.Text = text ;

			Font	defaultFont = null ;
			int		defaultFontSize = 0 ;
			Color	defaultColor = ARGB( color ) ;
			bool	defaultShadow = false ;
			bool	defaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultFont		= ds.Text_Font ;
					defaultFontSize	= ds.ButtonLabelFontSize ;
					defaultColor	= ds.ButtonLabelColor ;
					defaultShadow	= ds.ButtonLabelShadow ;
					defaultOutline	= ds.ButtonLabelOutline ;
				}
			}
			
#endif

			if( defaultFont == null )
			{
				label.Font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				label.Font = defaultFont ;
			}
			
			if( defaultFontSize == 0 )
			{
				label.FontSize = fontSize ;
			}
			else
			{
				label.FontSize = defaultFontSize ;
			}

			label.Color = defaultColor ;

			label.IsShadow	= defaultShadow ;
			label.IsOutline	= defaultOutline ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				label.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return label ;
		}

		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="text">ラベルの文字列</param>
		/// <param name="color">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIRichText AddRichLabel( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( RichLabel == null )
			{
				RichLabel = AddView<UIRichText>() ;
				RichLabel.name = "Label" ;
			}

			UIRichText label = RichLabel ;

			//----------------------------------

			if( fontSize <= 0 )
			{
				fontSize  = ( int )( Size.y * 0.6f ) ;
			}

			label.Alignment = TextAnchor.MiddleCenter ;
		
			label.Text = text ;

			Font	defaultFont = null ;
			int		defaultFontSize = 0 ;
			Color	defaultColor = ARGB( color ) ;
			bool	defaultShadow = false ;
			bool	defaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultFont		= ds.Text_Font ;
					defaultFontSize	= ds.ButtonLabelFontSize ;
					defaultColor	= ds.ButtonLabelColor ;
					defaultShadow	= ds.ButtonLabelShadow ;
					defaultOutline	= ds.ButtonLabelOutline ;
				}
			}
			
#endif

			if( defaultFont == null )
			{
				label.Font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				label.Font = defaultFont ;
			}
			
			if( defaultFontSize == 0 )
			{
				label.FontSize = fontSize ;
			}
			else
			{
				label.FontSize = defaultFontSize ;
			}

			label.Color = defaultColor ;

			label.IsShadow	= defaultShadow ;
			label.IsOutline	= defaultOutline ;


			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				label.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return label ;
		}

		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="text">ラベルの文字列</param>
		/// <param name="color">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UITextMesh AddLabelMesh( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( LabelMesh == null )
			{
				LabelMesh = AddView<UITextMesh>() ;
				LabelMesh.name = "Label" ;
			}

			UITextMesh label = LabelMesh ;

			//----------------------------------
			
			if( fontSize <= 0 )
			{
				fontSize  = ( int )( Size.y * 0.6f ) ;
			}

			label.Alignment = TMPro.TextAlignmentOptions.Center ;
		
			label.Text = text ;

//			Font	defaultFont = null ;
			int		defaultFontSize = 0 ;
			Color	defaultColor = ARGB( color ) ;
			bool	defaultShadow = false ;
			bool	defaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
//					defaultFont		= ds.Font ;
					defaultFontSize	= ds.ButtonLabelFontSize ;
					defaultColor	= ds.ButtonLabelColor ;
					defaultShadow	= ds.ButtonLabelShadow ;
					defaultOutline	= ds.ButtonLabelOutline ;
				}
			}
			
#endif

			// TextMeshPro ではフォントは設定できない
//			if( defaultFont == null )
//			{
//				label.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
//			}
//			else
//			{
//				label.font = tDefaultFont ;
//			}
			
			if( defaultFontSize == 0 )
			{
				label.FontSize = fontSize ;
			}
			else
			{
				label.FontSize = defaultFontSize ;
			}

			label.Color = defaultColor ;

			label.IsShadow	= defaultShadow ;
			label.IsOutline	= defaultOutline ;

			return label ;
		}

		/// <summary>
		/// いずれかのラベルが存在するか確認する
		/// </summary>
		/// <returns></returns>
		public bool HasAnyLabels()
		{
			if( Label != null )
			{
				return true ;
			}

			if( RichLabel != null )
			{
				return true ;
			}

			if( LabelMesh != null )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// ラベルのテキストを設定する
		/// </summary>
		/// <param name="labelText"></param>
		public void SetLabelText( string labelText )
		{
			if( Label != null )
			{
				Label.Text = labelText ;
			}

			if( RichLabel != null )
			{
				RichLabel.Text = labelText ;
			}

			if( LabelMesh != null )
			{
				LabelMesh.Text = labelText ;
			}
		}

		/// <summary>
		/// ラベルのテキストを設定する
		/// </summary>
		/// <param name="labelText"></param>
		public void SetLabelColor( Color labelColor )
		{
			if( Label != null )
			{
				Label.Color = labelColor ;
			}

			if( RichLabel != null )
			{
				RichLabel.Color = labelColor ;
			}

			if( LabelMesh != null )
			{
				LabelMesh.Color = labelColor ;
			}
		}

		/// <summary>
		/// ラベルのテキストを設定する
		/// </summary>
		/// <param name="labelText"></param>
		public void SetLabelColor( uint labelColor )
		{
			if( Label != null )
			{
				Label.SetColor( labelColor ) ;
			}

			if( RichLabel != null )
			{
				RichLabel.SetColor( labelColor ) ;
			}

			if( LabelMesh != null )
			{
				LabelMesh.SetColor( labelColor ) ;
			}
		}

		/// <summary>
		/// ラベルのマテリアルを設定する
		/// </summary>
		/// <param name="labelText"></param>
		public void SetLabelMaterial( Material labelMaterial )
		{
			if( Label != null )
			{
				Label.Material = labelMaterial ;
			}

			if( RichLabel != null )
			{
				RichLabel.Material = labelMaterial ;
			}

			if( LabelMesh != null )
			{
				LabelMesh.Material = labelMaterial ;
			}
		}


		/// <summary>
		/// ラベルの横幅を取得する
		/// </summary>
		/// <param name="labelText"></param>
		public float GetLabelTextWidth()
		{
			if( Label != null )
			{
				return Label.TextWidth ;
			}

			if( RichLabel != null )
			{
				return RichLabel.TextWidth ;
			}

			if( LabelMesh != null )
			{
				return LabelMesh.TextWidth ;
			}

			return 0 ;
		}

		/// <summary>
		/// アイコン画像を設定する
		/// </summary>
		/// <param name="iconImage"></param>
		/// <param name="resize"></param>
		/// <returns></returns>
		public bool SetIcon( Sprite iconImage, bool resize = false )
		{
			if( Icon == null )
			{
				return false ;
			}

			Icon.SetSprite( iconImage, resize ) ;

			return true ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="color">カラー値(AARRGGBB)</param>
		public void On( uint color = 0xFFFFFFFF, bool interactableChange = true )
		{
			if( ( color & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   color         & 0xFF ) ;
				byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

				On( new Color32( r, g, b, a ), interactableChange ) ;
			}
		}

		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="color"></param>
		/// <param name="interactableChange"></param>
		public void On( Color color, bool interactableChange = true )
		{
			if( interactableChange == true )
			{
				Interactable = true ;
			}

			Image image = CImage ;
			image.color = color ;

			m_EffectiveColor = color ;

			if( m_IsApplyColorToChildren == true )
			{
				ApplyColorToChidren( image.color ) ;
			}
		}

		/// <summary>
		/// 特殊なインタラクション無効化
		/// </summary>
		/// <param name="color">カラー値(AARRGGBB)</param>
		public void Off( uint color = 0xC0C0C0C0, bool interactableChange = true )
		{
			if( ( color & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   color         & 0xFF ) ;
				byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

				Off( new Color32( r, g, b, a ), interactableChange ) ;
			}
		}

		/// <summary>
		/// 特殊なインタラクション無効化
		/// </summary>
		/// <param name="color"></param>
		/// <param name="interactableChange"></param>
		public void Off( Color color, bool interactableChange = true )
		{
			if( interactableChange == true )
			{
				Interactable = false ;
			}

			Image image = CImage ;
			image.color = color ;

			m_EffectiveColor = color ;

			if( m_IsApplyColorToChildren == true )
			{
				ApplyColorToChidren( image.color ) ;
			}
		}

		/// <summary>
		/// 特殊なインタラクション設定
		/// </summary>
		/// <param name="state"></param>
		public void SetInteractable( bool state, uint color = 0 )
		{
			if( state == true )
			{
				if( color == 0 )
				{
					color = 0xFFFFFFFF ;
				}
				On( color ) ;
			}
			else
			{
				if( color == 0 )
				{
					color = 0xC0C0C0C0 ;
				}
				Off( color ) ;
			}
		}

		/// <summary>
		/// クリックされるのを待つ
		/// </summary>
		/// <returns></returns>
		public UIView.AsyncState WaitFor()
		{
			if( gameObject.activeInHierarchy == false )
			{
				Debug.LogWarning( "GameObject がアクティブになっていません : " + name ) ;
				return null ;
			}

			UIView.AsyncState state = new AsyncState( this ) ;
			StartCoroutine( WaitFor_Private( state ) ) ;
			return state ;
		}

		private IEnumerator WaitFor_Private( UIView.AsyncState state )
		{
			while( m_IsButtonClicked == false )
			{
				yield return null ;
			}

			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------
		// Transition の色を設定する

		/// <summary>
		/// 通常色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetTransitionNormalColor( Color color )
		{
			var colors = CButton.colors ;
			colors.normalColor = color ;
			CButton.colors = colors ;
		}

		/// <summary>
		/// 強調色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetTransitionHighlightColor( Color color )
		{
			var colors = CButton.colors ;
			colors.highlightedColor = color ;
			CButton.colors = colors ;
		}

		/// <summary>
		/// 押下色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetTransitionPressedColor( Color color )
		{
			var colors = CButton.colors ;
			colors.pressedColor = color ;
			CButton.colors = colors ;
		}

		/// <summary>
		/// 選択色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetTransitionSelectedColor( Color color )
		{
			var colors = CButton.colors ;
			colors.selectedColor = color ;
			CButton.colors = colors ;
		}

		/// <summary>
		/// 無効色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetTransitionDisabledColor( Color color )
		{
			var colors = CButton.colors ;
			colors.disabledColor = color ;
			CButton.colors = colors ;
		}

		//-------------------------------------------------------------------------------------------
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
				m_IsLostFocusForButton = false ;

				//---------------------------------

//				Debug.Log( "[UIButton] OnApplicationFocus Path = " + Path + " isFocus = " + isFocus ) ;

				// レジューム時にボタンの状態を復旧させる
				if( CButton != null && CButton.transition == Selectable.Transition.ColorTint && CButton.targetGraphic != null && CButton.targetGraphic.canvasRenderer != null )
				{
					Color color ;
					if( CButton.interactable == false )
					{
						color = CButton.colors.disabledColor ;
					}
					else
					{
						color = CButton.colors.normalColor ;
					}

					// 自身を含める
					var canvasRenderer = CButton.targetGraphic.canvasRenderer ;
					if( canvasRenderer != null )
					{
						canvasRenderer.SetColor( color ) ;
					}

					m_PreviousInteractable = CButton.interactable ;
					m_RefreshChildrenColor = false ;
				}

				//---------------------------------------------------------
				// 子にも反映させるかどうか

				if( m_IsApplyColorToChildren == true )
				{
					if( m_EffectiveColorReplacing == true )
					{
						var canvasRenderer = GetComponent<CanvasRenderer>() ;
						if( canvasRenderer != null )
						{
							ApplyColorToChidren( canvasRenderer.GetColor(), false ) ;
						}
					}
				}

			}
		}
//#endif
		//-------------------------------------------------------------------------------------------
	}
}
