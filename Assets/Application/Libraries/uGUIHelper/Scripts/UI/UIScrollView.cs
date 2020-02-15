using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
//	[RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
	[RequireComponent(typeof(ScrollRectWrapper))]
	public class UIScrollView : UIImage
	{
		/// <summary>
		/// ビューポート
		/// </summary>
		[SerializeField]
		protected UIImage		m_Viewport ;
		
		/// <summary>
		/// ビューポート
		/// </summary>
		public UIImage viewport
		{
			get
			{
				if( m_Viewport == null )
				{
					ScrollRect scrollRect = _scrollRect ;
					if( scrollRect != null )
					{
						m_Viewport = scrollRect.viewport.GetComponent<UIImage>() ;
					}
				}
				return m_Viewport ;
			}
		}

		/// <summary>
		/// コンテント
		/// </summary>
		[SerializeField]
		protected UIView		m_Content ;

		/// <summary>
		/// Content のインスタンス
		/// </summary>
		public UIView			content
		{
			get
			{
				if( m_Content == null )
				{
					ScrollRect scrollRect = _scrollRect ;
					if( scrollRect != null )
					{
						m_Content = scrollRect.content.GetComponent<UIView>() ;
					}
				}
				return m_Content ;
			}
		}
		

		/// <summary>
		/// 水平方向スクロール設定(ショートカット)
		/// </summary>
		public bool horizontal
		{
			get
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return false ;
				}
				return tScrollRect.horizontal ;
			}
			set
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return ;
				}
				tScrollRect.horizontal = value ;
			}
		}

		/// <summary>
		/// 垂直方向スクロール設定(ショートカット)
		/// </summary>
		public bool vertical
		{
			get
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return false ;
				}
				return tScrollRect.vertical ;
			}
			set
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return ;
				}
				tScrollRect.vertical = value ;
			}
		}

		/// <summary>
		/// 水平スクロールバー(ショートカット)
		/// </summary>
		public ScrollbarWrapper horizontalScrollbar
		{
			get
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return null ;
				}
				ScrollbarWrapper tScrollbar = ( ScrollbarWrapper )tScrollRect.horizontalScrollbar ;
				if( tScrollbar == null )
				{
					if( m_HorizontalScrollbarElastic != null )
					{
						return m_HorizontalScrollbarElastic.GetComponent<ScrollbarWrapper>() ;
					}
				}
				return tScrollbar ;
			}
			set
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return ;
				}
				tScrollRect.horizontalScrollbar = value ;
			}
		}
//		public UIScrollbar horizontalScrollbar ;
	
		/// <summary>
		/// 垂直スクロールバー(ショートカット)
		/// </summary>
		public ScrollbarWrapper verticalScrollbar
		{
			get
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return null ;
				}
				ScrollbarWrapper tScrollbar = ( ScrollbarWrapper )tScrollRect.verticalScrollbar ;
				if( tScrollbar == null )
				{
					if( m_VerticalScrollbarElastic != null )
					{
						tScrollbar = m_VerticalScrollbarElastic.GetComponent<ScrollbarWrapper>() ;
					}
				}
				return tScrollbar ;
			}
			set
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;
				if( tScrollRect == null )
				{
					return ;
				}
				tScrollRect.verticalScrollbar = value ;
			}
		}
//		public UIScrollbar verticalScrollbar ;

	
		public enum HorizontalScrollbarPosition
		{
			Top    = 0,
			Bottom = 1,
		}

		/// <summary>
		/// 水平スクロールバーの基準位置(ショートカット)
		/// </summary>
		[SerializeField][HideInInspector]
		private HorizontalScrollbarPosition m_HorizontalScrollbarPosition = HorizontalScrollbarPosition.Bottom ;
		public  HorizontalScrollbarPosition   horizontalScrollbarPosition
		{
			get
			{
				return m_HorizontalScrollbarPosition ;
			}
			set
			{
				if( m_HorizontalScrollbarPosition != value )
				{
					m_HorizontalScrollbarPosition  = value ;
					ResetHorizontalScrollbar() ;
				}
			}
		}

		public enum VerticalScrollbarPosition
		{
			Left  = 0,
			Right = 1,
		}

		/// <summary>
		/// 垂直スクロールバーの基準位置(ショートカット)
		/// </summary>
		[SerializeField][HideInInspector]
		private VerticalScrollbarPosition m_VerticalScrollbarPosition = VerticalScrollbarPosition.Right ;
		public  VerticalScrollbarPosition   verticalScrollbarPosition
		{
			get
			{
				return m_VerticalScrollbarPosition ;
			}
			set
			{
				if( m_VerticalScrollbarPosition != value )
				{
					m_VerticalScrollbarPosition  = value ;
					ResetVerticalScrollbar() ;
				}
			}
		}

		/// <summary>
		/// スクロールバーのフェード効果
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_ScrollbarFadeEnabled      = true ;
		public  bool   scrollbarFadeEnabled
		{
			get
			{
				return m_ScrollbarFadeEnabled ;
			}
			set
			{
				m_ScrollbarFadeEnabled = value ;

				if( m_ScrollbarFadeEnabled == false )
				{
					float tAlpha = 1 ;

					if( m_HorizontalScrollbarCanvasGroup != null )
					{
						m_HorizontalScrollbarCanvasGroup.alpha = tAlpha ;
						m_HorizontalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
					}

					if( m_VerticalScrollbarCanvasGroup != null )
					{
						m_VerticalScrollbarCanvasGroup.alpha = tAlpha ;
						m_VerticalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
					}
				}
			}
		} 

		/// <summary>
		/// スクロールバーのフェードイン時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeInDuration   = 0.2f ;
		public  float   scrollbarFadeInDuration
		{
			get
			{
				return m_ScrollbarFadeInDuration ;
			}
			set
			{
				m_ScrollbarFadeInDuration = value ;
			}
		}

		/// <summary>
		/// スクロールバーのホールド時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeHoldDuration = 1.0f ;
		public  float   scrollbarFadeHoldDuration
		{
			get
			{
				return m_ScrollbarFadeHoldDuration ;
			}
			set
			{
				m_ScrollbarFadeHoldDuration = value ;
			}
		}
	
		/// <summary>
		/// スクロールバーのフェードアウト時間
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_ScrollbarFadeOutDuration  = 0.2f ;
		public  float   scrollbarFadeOutDuration
		{
			get
			{
				return m_ScrollbarFadeOutDuration ;
			}
			set
			{
				m_ScrollbarFadeOutDuration = value ;
			}
		}
		

		/// <summary>
		/// コンイントがビューより小さい場合にスクロールバーを非表示にする
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_HidingScrollbarIfContentFew  = true ;
		public  bool   hidingScrollbarIfContentFew
		{
			get
			{
				return m_HidingScrollbarIfContentFew ;
			}
			set
			{
				m_HidingScrollbarIfContentFew = value ;
			}
		}
		
		/// <summary>
		/// コンイントがビューより小さい場合にスクロールバーを非表示にする
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_InvalidateScrollIfContentFew  = true ;
		public  bool   invalidateScrollIfContentFew
		{
			get
			{
				return m_InvalidateScrollIfContentFew ;
			}
			set
			{
				m_InvalidateScrollIfContentFew = value ;
			}
		}
		


		private int   m_ScrollbarFadeState = 0 ;
		private float m_ScrollbarFadeTime  = 0 ;
		private float m_ScrollbarFadeAlpha = 0 ;
		private float m_ScrollbarBaseTime  = 0 ;

		// デフォルトのスクロールバーは Elastic にまともに対応していないので独自に制御する
		[SerializeField][HideInInspector]
		private UIScrollbar	m_HorizontalScrollbarElastic = null ;
		public  UIScrollbar   horizontalScrollbarElastic
		{
			get
			{
				return m_HorizontalScrollbarElastic ;
			}
			set
			{
				m_HorizontalScrollbarElastic = value ;
			}
		}

		// デフォルトのスクロールバーは Elastic にまともに対応していないので独自に制御する
		[SerializeField][HideInInspector]
		private UIScrollbar	m_VerticalScrollbarElastic = null ;
		public  UIScrollbar   verticalScrollbarElastic
		{
			get
			{
				return m_VerticalScrollbarElastic ;
			}
			set
			{
				m_VerticalScrollbarElastic = value ;
			}
		}


		//--------------------------------------------------
	
		// 種別
		public enum BuildType
		{
			Unknown		= 0,
			ScrollView	= 1,
			ListView	= 2,
			Dropdown	= 3,
		}
		public BuildType buildType = BuildType.Unknown ;


		// 方向
		public enum Direction
		{
			Unknown		= 0,
			Horizontal	= 1,
			Vertical	= 2,
		}
		
		/// <summary>
		/// スクロールビューが動いているかどうか
		/// </summary>
		public bool isMoving
		{
			get
			{
				if( _scrollRect.velocity.magnitude >  0 || IsSnapping() == true || m_IsAutoMoving == true )
				{
					return true ;
				}
				return false ;
			}
		}

		private bool m_IsContentDrag = false ;
		private bool m_IsMoving = false ;

		private bool m_IsAutoMoving = false ;

		//------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			ScrollRectWrapper tScrollRect = _scrollRect ;

			if( tScrollRect == null )
			{
				tScrollRect = gameObject.AddComponent<ScrollRectWrapper>() ;
			}
			if( tScrollRect == null )
			{
				// 異常
				return ;
			}
			
			Image tImage = _image ;

			//-------------------------------------

			BuildType tBuildType = BuildType.Unknown ;
			Direction tDirection = Direction.Unknown ;

			if( tOption.ToLower() == "sh" )
			{
				tBuildType = BuildType.ScrollView ;
				tDirection = Direction.Horizontal ;
			}
			else
			if( tOption.ToLower() == "sv" )
			{
				tBuildType = BuildType.ScrollView ;
				tDirection = Direction.Vertical ;
			}
			else
			if( tOption.ToLower() == "dropdown" )
			{
				tBuildType = BuildType.Dropdown ;
				tDirection = Direction.Vertical ;
			}
			else
			{
				// デフォルト
				tBuildType = BuildType.ScrollView;
				tDirection = Direction.Unknown ;
			}

			buildType = tBuildType ;	// 後から変更は出来ない

			// 基本的な大きさを設定
			float s = 100.0f ;
			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				if( tSize.x <= tSize.y )
				{
					s = tSize.x ;
				}
				else
				{
					s = tSize.y ;
				}
				s = s * 0.5f ;
			}
				
			ResetRectTransform() ;

			// 方向を設定
			if( tDirection == Direction.Horizontal )
			{
				tScrollRect.horizontal = true ;
				tScrollRect.vertical   = false ;
				SetSize( s, s * 0.75f ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tScrollRect.horizontal = false ;
				tScrollRect.vertical   = true ;
				SetSize( s * 0.75f, s ) ;
			}
			else
			{
				tScrollRect.horizontal = true ;
				tScrollRect.vertical   = true ;
				SetSize( s, s ) ;
			}

			if( tBuildType == BuildType.Dropdown )
			{
				tScrollRect.movementType = ScrollRect.MovementType.Clamped ;
			}

			// Mask 等を設定する Viewport を設定(スクロールバーは表示したいので ScrollRect と Mask は別の階層に分ける)
			m_Viewport = AddView<UIImage>( "Viewport" ) ;
			m_Viewport.SetAnchorToStretch() ;
			m_Viewport.SetMargin( 0, 0, 0, 0 ) ;
			m_Viewport.SetPivot( 0, 1 ) ;
			tScrollRect.viewport = m_Viewport.GetRectTransform() ;

			// マスクは CanvasRenderer と 何等かの表示を伴うコンポートと一緒でなければ使用できない
//			Mask tMask = m_Viewport.gameObject.AddComponent<Mask>() ;
//			tMask.showMaskGraphic = false ;
			m_Viewport.gameObject.AddComponent<RectMask2D>() ;
//			m_Viewport.color = new Color( 0, 0, 0, 0 ) ;
			m_Viewport._image.enabled = false ;

			// Content を追加する
			UIView tContent = CreateContent( m_Viewport, tBuildType, tDirection ) ;
			if( tContent != null )
			{
				tScrollRect.content = tContent.GetRectTransform() ;
				m_Content = tContent ;
				
				if( tBuildType == BuildType.Dropdown )
				{
					CreateDropdownItem( tContent ) ;
				}
			}

			// 自身の Image
			tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			tImage.color = new Color32( 255, 255, 255,  63 ) ;
			tImage.type = Image.Type.Sliced ;

			// ホイール感度
			tScrollRect.scrollSensitivity = 10 ;
		}

		// デフォルトの Content を生成する
		private UIView CreateContent( UIView tParent, BuildType tBuildType, Direction tDirection )
		{
			UIView tContent = null ;

			if( tBuildType == BuildType.ScrollView )
			{
				// ScrollView

				tContent = tParent.AddView<UIView>( "Content" ) ;

				if( tDirection == Direction.Horizontal )
				{
					// 横スクロール
					tContent.SetAnchorToLeftStretch() ;
					tContent.SetMargin( 0, 0, 0, 0 ) ;
				
					tContent.Width = this.Width ;

					tContent.Height =  0 ;
					tContent.Px =  0 ;
					tContent.Py =  0 ;

					tContent.SetPivot( 0.0f, 1.0f ) ;
						
//					tContent._contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize ;
//					tContent._contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.Unconstrained ;

//					tContent.AddComponent<HorizontalLayoutGroup>() ;
//					tContent._horizontalLayoutGroup.childAlignment = TextAnchor.UpperLeft ;
				}
				else
				if( tDirection == Direction.Vertical )
				{
					// 縦スクロール
					tContent.SetAnchorToStretchTop() ;
					tContent.SetMargin( 0, 0, 0, 0 ) ;
					
					tContent.Height = this.Height ;

					tContent.Width =  0 ;
					tContent.Py =  0 ;
					tContent.Px =  0 ;
						
					tContent.SetPivot( 0.0f, 1.0f ) ;

//					tContent._contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained ;
//					tContent._contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize ;

//					tContent.AddComponent<VerticalLayoutGroup>() ;
//					tContent._verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft ;
				}
				else
				{
					tContent.SetAnchorToLeftTop() ;
					tContent.SetMargin( 0, 0, 0, 0 ) ;
					
					tContent.Width  = this.Width ;
					tContent.Height = this.Height ;
					tContent.Px =  0 ;
					tContent.Py =  0 ;

					tContent.SetPivot( 0.0f, 1.0f ) ;
				}
			}
			else
			if( tBuildType == BuildType.Dropdown )
			{
				// Dropdown
				UIDropdown tDropdown = null ;
				if( transform.parent != null )
				{
					tDropdown = transform.parent.gameObject.GetComponent<UIDropdown>() ;
				}

				if( tDropdown == null )
				{
					return null ;
				}

				tContent = tParent.AddView<UIView>( "Content" ) ;

				tContent.SetAnchorToStretchTop() ;
				tContent.SetMargin( 0, 0, 0, 0 ) ;
					
				tContent.Width  =  0 ;
				tContent.Height = tDropdown.Height ;
				tContent.Py =  0 ;
				tContent.Px =  0 ;

				tContent.SetPivot( 0.5f, 1.0f ) ;
			}

			return tContent ;
		}

		public UIView dropdownItem = null ;

		// テンプレートのアイテムを生成する
		private void CreateDropdownItem( UIView tParent )
		{
			// Dropdown 専用

			UIDropdown tDropdown = null ;
			if( transform.parent != null )
			{
				tDropdown = transform.parent.gameObject.GetComponent<UIDropdown>() ;
			}

			if( tDropdown == null )
			{
				return ;
			}

			UIToggle tItem = tParent.AddView<UIToggle>( "Item(Template)", "no group" ) ;

			tItem.SetAnchorToStretchMiddle() ;
			tItem.Height = tDropdown.Height ;

			tItem.Background.SetAnchorToStretch() ;
			tItem.Background.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			tItem.Background.name = "Item Background" ;
				
			tItem.Checkmark.SetAnchorToLeftMiddle() ;
			tItem.Checkmark.SetPosition( 10,  0 ) ;
			tItem.Checkmark.SetSize( tItem.Height * 0.66f, tItem.Height * 0.66f ) ;
			tItem.Checkmark.name = "Item Chackmark" ;

			tItem.Label.SetAnchorToStretch() ;
			tItem.Label.SetMargin( 20, 10,  2,  1 ) ;
			tItem.Label.Alignment = TextAnchor.MiddleLeft ;
			tItem.Label.Color = new Color32(  50,  50,  50, 255 ) ;
			tItem.Label.FontSize = tDropdown.CaptionText.FontSize ;
			tItem.Label.Text = "Option A" ;
			tItem.Label.name = "Item Label" ;

			tDropdown._dropdown.itemText = tItem.Label._text ;
				
			dropdownItem = tItem ;	// 外部からアクセス可能な変数にインスタンスを保持する
		}


		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _scrollRect!= null )
				{
					_scrollRect.onValueChanged.AddListener( OnValueChangedInner ) ;
				}

				if( buildType == BuildType.Dropdown )
				{
					// ドロップダウン用のスクロールビューの場合にリストの表示位置を設定する
					UIDropdown tDropdown = transform.parent.GetComponent<UIDropdown>() ;

					// ドロップダウン用のスクロールビュー
					if( tDropdown != null )
					{
						float tItemSize = 0 ;

						if( tDropdown._dropdown.itemText != null )
						{
							UIView tView = tDropdown._dropdown.itemText.transform.parent.GetComponent<UIView>() ;
							if( tView != null )
							{
								tItemSize = tView.Height ;
							}
						}
						else
						if( tDropdown._dropdown.itemImage != null )
						{
							UIView tView = tDropdown._dropdown.itemImage.transform.parent.GetComponent<UIView>() ;
							if( tView != null )
							{
								tItemSize = tView.Height ;
							}
						}

						if( tItemSize >  0 )
						{
							float tOffset = tDropdown.Value * tItemSize ;

							float tViewSize = viewSize ;
							float tContentSize = contentSize ;

							if( ( tOffset + tViewSize ) >  tContentSize )
							{
								tOffset = tContentSize - tViewSize ;
							}

							content.Ry = tOffset ;
						}
					}
				}

				//---------------------------------------------------------
				// スクロールバー関係


				if( m_HorizontalScrollbarElastic != null )
				{
					m_HorizontalScrollbarElasticLength = m_HorizontalScrollbarElastic.length ;
				}

				if( m_VerticalScrollbarElastic != null )
				{
					m_VerticalScrollbarElasticLength = m_VerticalScrollbarElastic.length ;
				}

				// スクロールバーのフェード関係
				if( horizontalScrollbar != null )
				{
					m_HorizontalScrollbarCanvasGroup = horizontalScrollbar.GetComponent<CanvasGroup>() ;
				}

				if( verticalScrollbar != null )
				{
					m_VerticalScrollbarCanvasGroup   = verticalScrollbar.GetComponent<CanvasGroup>() ;
				}

				m_ScrollbarFadeState = 0 ;
				m_ScrollbarFadeTime  = 0 ;
				m_ScrollbarFadeAlpha = 0 ;
			}
		}

		/// <summary>
		/// 方向を取得する
		/// </summary>
		public Direction direction
		{
			get
			{
				ScrollRectWrapper tScrollView = _scrollRect ;

				// 横スクロール
				if( tScrollView.horizontal == true  && tScrollView.vertical == false )
				{
					return  Direction.Horizontal ;
				}

				// 縦スクロール
				if( tScrollView.horizontal == false && tScrollView.vertical == true  )
				{
					return Direction.Vertical ;
				}
				
				return Direction.Unknown ;
			}
		}

		// 表示領域の幅を取得する
		public float viewSize
		{
			get
			{
				if( m_Viewport == null )
				{
					if( _scrollRect != null && _scrollRect.viewport != null )
					{
						m_Viewport = _scrollRect.viewport.GetComponent<UIImage>() ;
					}
				}

				if( m_Viewport == null )
				{
					return 0 ;
				}

				// 横スクロール
				if( direction == Direction.Horizontal )
				{
					return m_Viewport.Width ;
				}

				// 縦スクロール
				if( direction == Direction.Vertical )
				{
					return m_Viewport.Height ;
				}
				
				return 0 ;
			}
		}

		// コンテントの幅を取得する
		public float contentSize
		{
			get
			{
				// 横スクロール
				if( direction == Direction.Horizontal )
				{
					return content.Width ;
				}

				// 縦スクロール
				if( direction == Direction.Vertical )
				{
					return content.Height ;
				}

				return 0 ;
			}
			set
			{
				// 横スクロール
				if( direction == Direction.Horizontal )
				{
					content.Width = value ;
				}

				// 縦スクロール
				if( direction == Direction.Vertical )
				{
					content.Height = value ;
				}
			}
		}

		// 現在位置を取得する
		public float contentPosition
		{
			get
			{
				// 横スクロール
				if( direction == Direction.Horizontal )
				{
					return - content.Rx ;
				}

				// 縦スクロール
				if( direction == Direction.Vertical )
				{
					return   content.Ry ;
				}
				
				return 0 ;
			}
			set
			{
				// 横スクロール
				if( direction == Direction.Horizontal )
				{
					content.Rx = - value ;
				}

				// 縦スクロール
				if( direction == Direction.Vertical )
				{
					content.Ry =   value ;
				}
			}
		}

		//------------------------------------------------------
		

		/// <summary>
		/// 派生クラスの Update
		/// </summary>
		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				ScrollRectWrapper tScrollRect = _scrollRect ;

				//---------------------------------------------------------

				// スクロールバーのフェード関係
				if( horizontalScrollbar != null && m_HorizontalScrollbarCanvasGroup != null )
				{
					if( m_HidingScrollbarIfContentFew == true )
					{
						if( this is UIListView )
						{
							UIListView tListView = this as UIListView ;
							if( direction == Direction.Horizontal )
							{
								if( tListView.infinity == false )
								{
									if( tListView.contentSize <= viewport.Width )
									{
										m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
									}
									else
									{
										m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
									}
								}
							}
						}
						else
						if( this is UIScrollView )
						{
							if( content.Width <= viewport.Width )
							{
								m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
							}
							else
							{
								m_HorizontalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
							}
						}
					}
				}

				if( verticalScrollbar != null && m_VerticalScrollbarCanvasGroup != null )
				{
					if( m_HidingScrollbarIfContentFew == true )
					{
						if( this is UIListView )
						{
							UIListView tListView = this as UIListView ;
							if( direction == Direction.Vertical )
							{
								if( tListView.infinity == false )
								{
									if( tListView.contentSize <= viewport.Height )
									{
										m_VerticalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
									}
									else
									{
										m_VerticalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
									}
								}
							}
						}
						else
						if( this is UIScrollView )
						{
							if( content.Height <= viewport.Height )
							{
								m_VerticalScrollbarCanvasGroup.gameObject.SetActive( false ) ;
							}
							else
							{
								m_VerticalScrollbarCanvasGroup.gameObject.SetActive( true ) ;
							}
						}
					}
				}

				if( m_InvalidateScrollIfContentFew == true && tScrollRect != null )
				{
					if( this is UIListView )
					{
						UIListView tListView = this as UIListView ;

						if( direction == Direction.Horizontal )
						{
							if( tListView.infinity == false )
							{
								if( tListView.contentSize <= viewport.Width )
								{
									tScrollRect.enabled = false ;
								}
								else
								{
									tScrollRect.enabled = true ;
								}
							}
						}
						else
						if( direction == Direction.Vertical )
						{
							if( tListView.infinity == false )
							{
								if( tListView.contentSize <= viewport.Height )
								{
									tScrollRect.enabled = false ;
								}
								else
								{
									tScrollRect.enabled = true ;
								}
							}
						}
					}
					else
					if( this is UIScrollView )
					{
						if( content.Width <= viewport.Width )
						{
							tScrollRect.horizontal = false ;
						}
						else
						{
							tScrollRect.horizontal = true ;
						}

						if( content.Height <= viewport.Height )
						{
							tScrollRect.vertical = false ;
						}
						else
						{
							tScrollRect.vertical = true ;
						}
					}
				}

				//---------------------------------------------------------

				ProcessScrollbarElastic() ;

				if( m_ScrollbarFadeEnabled == true && ( isInteraction == true || isInteractionForScrollView == true ) )
				{
					ProcessScrollbarFade() ;
				}

				if( tScrollRect != null )
				{
					if( m_IsContentDrag != tScrollRect.isDrag )
					{
						m_IsContentDrag  = tScrollRect.isDrag ;

						OnContentDragInner( m_IsContentDrag ) ;	// ドラッグ
					}
				}

				bool tIsMoving = isMoving ;
				if( m_IsMoving != tIsMoving )
				{
					m_IsMoving  = tIsMoving ;

					OnContentMoveInner( m_IsMoving ) ;

					if( tIsMoving == false )
					{
						OnStoppedInner( contentPosition ) ;	// 停止
					}
				}
			}

			//-----------------------------------------

			if( m_RemoveHorizontalScrollbar == true )
			{
				RemoveHorizontalScrollbar() ;
				m_RemoveHorizontalScrollbar = false ;
			}
		
			if( m_RemoveVerticalScrollbar == true )
			{
				RemoveVerticalScrollbar() ;
				m_RemoveVerticalScrollbar = false ;
			}
		}


		//--------------------------------------------------------

		virtual protected bool IsSnapping()
		{
			return false ;
		}
		
		//--------------------------------------------------------

		private bool m_RemoveHorizontalScrollbar = false ;
		public bool isHorizontalScrollber
		{
			get
			{
				if( horizontalScrollbar == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddHorizontalScrollbar() ;
				}
				else
				{
					m_RemoveHorizontalScrollbar = true ;
				}
			}
		}

		/// <summary>
		/// 水平スクロールバーを追加する
		/// </summary>
		public void AddHorizontalScrollbar()
		{
			if( horizontalScrollbar != null )
			{
				return ;
			}
		
			ScrollRectWrapper tScrollRect = _scrollRect ;
			if( tScrollRect != null )
			{
				UIScrollbar tScrollbar = AddView<UIScrollbar>( "Scrollber(H)", "H" ) ;
				tScrollbar.IsCanvasGroup = true ;
				tScrollRect.horizontalScrollbar = tScrollbar._scrollbar ;
//				horizontalScrollbar = tScrollbar ;

				ResetHorizontalScrollbar() ;
			}
		}
		
		/// <summary>
		/// 水平スクロールバーをリセットする
		/// </summary>
		public void ResetHorizontalScrollbar()
		{
			if( horizontalScrollbar == null )
			{
				return ;
			}
		
			UIScrollbar tScrollbar = horizontalScrollbar.GetComponent<UIScrollbar>() ;
//			UIScrollbar tScrollbar = horizontalScrollbar ;
			if( tScrollbar == null )
			{
				return ;
			}

			if( m_HorizontalScrollbarPosition == HorizontalScrollbarPosition.Top )
			{
				tScrollbar.SetAnchorToStretchTop() ;
				tScrollbar.SetMargin( 0, 0, 0, 0 ) ;
				tScrollbar.Py =  0 ;
				tScrollbar.Height = 30 ;
				tScrollbar.Width  =  0 ;
				tScrollbar.SetPivot( 0, 1 ) ;
			}
			else
			if( m_HorizontalScrollbarPosition == HorizontalScrollbarPosition.Bottom )
			{
				tScrollbar.SetAnchorToStretchBottom() ;
				tScrollbar.SetMargin( 0, 0, 0, 0 ) ;
				tScrollbar.Py =  0 ;
				tScrollbar.Height = 30 ;
				tScrollbar.Width  =  0 ;
				tScrollbar.SetPivot( 0, 0 ) ;
			}

			_scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport ;
	//		_scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide ;
			_scrollRect.horizontalScrollbarSpacing = 0 ;
		}

		/// <summary>
		/// 水平スクロールバーを削除する
		/// </summary>
		public void RemoveHorizontalScrollbar()
		{
			if( horizontalScrollbar == null )
			{
				return ;
			}
		
			Scrollbar h = horizontalScrollbar ;

			horizontalScrollbar = null ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( h.gameObject ) ;
			}
			else
			{
				Destroy( h.gameObject ) ;
			}
		}

		//-----------------------------------------------------------

		private bool m_RemoveVerticalScrollbar = false ;
		public bool isVerticalScrollber
		{
			get
			{
				if( verticalScrollbar == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddVerticalScrollbar() ;
				}
				else
				{
					m_RemoveVerticalScrollbar = true ;
				}
			}
		}

		/// <summary>
		/// 垂直スクロールバーを追加する
		/// </summary>
		public void AddVerticalScrollbar()
		{
			if( verticalScrollbar != null )
			{
				return ;
			}
		
			ScrollRectWrapper tScrollRect = _scrollRect ;
			if( tScrollRect != null )
			{
				UIScrollbar tScrollbar = AddView<UIScrollbar>( "Scrollbar(V)", "V" ) ;
				tScrollbar.IsCanvasGroup = true ;
				tScrollRect.verticalScrollbar = tScrollbar._scrollbar ;
//				verticalScrollbar = tScrollbar ;

				ResetVerticalScrollbar() ;
			}
		}

		/// <summary>
		/// 垂直スクロールバーをリセットする
		/// </summary>
		public void ResetVerticalScrollbar()
		{
			if( verticalScrollbar == null )
			{
				return ;
			}
		
			UIScrollbar tScrollbar = verticalScrollbar.GetComponent<UIScrollbar>() ;
//			UIScrollbar tScrollbar = verticalScrollbar ;
			if( tScrollbar == null )
			{
				return ;
			}

			if( m_VerticalScrollbarPosition == VerticalScrollbarPosition.Left )
			{
				tScrollbar.SetAnchorToLeftStretch() ;
				tScrollbar.SetMargin( 0, 0, 0, 0 ) ;
				tScrollbar.Px =  0 ;
				tScrollbar.Width  = 30 ;
				tScrollbar.Height =  0 ;
				tScrollbar.SetPivot( 0, 1 ) ;
			}
			else
			if( m_VerticalScrollbarPosition == VerticalScrollbarPosition.Right )
			{
				tScrollbar.SetAnchorToRightStretch() ;
				tScrollbar.SetMargin( 0, 0, 0, 0 ) ;
				tScrollbar.Px =  0 ;
				tScrollbar.Width  = 30 ;
				tScrollbar.Height =  0 ;
				tScrollbar.SetPivot( 1, 1 ) ;
			}

			_scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport ;
	//		_scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide ;
			_scrollRect.verticalScrollbarSpacing = 0 ;
		}


		/// <summary>
		/// 垂直スクロールバーを削除する
		/// </summary>
		public void RemoveVerticalScrollbar()
		{
			if( verticalScrollbar == null )
			{
				return ;
			}
			
			Scrollbar v = verticalScrollbar ;

			verticalScrollbar = null ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( v.gameObject ) ;
			}
			else
			{
				Destroy( v.gameObject ) ;
			}
		}

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, Vector2> onValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDelegate( string tIdentity, UIScrollView tView, Vector2 tValue ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDelegate onValueChangedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIScrollView, Vector2> tOnValueChangedAction )
		{
			onValueChangedAction = tOnValueChangedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate += tOnValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate -= tOnValueChangedDelegate ;
		}

		// 内部リスナー
		private void OnValueChangedInner( Vector2 tValue )
		{
			if( onValueChangedAction != null || onValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onValueChangedAction?.Invoke( identity, this, tValue ) ;
				onValueChangedDelegate?.Invoke( identity, this, tValue ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<Vector2> tOnValueChanged )
		{
			ScrollRectWrapper tScrollRect = _scrollRect ;
			if( tScrollRect != null )
			{
				tScrollRect.onValueChanged.AddListener( tOnValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangeListener( UnityEngine.Events.UnityAction<Vector2> tOnValueChanged )
		{
			ScrollRectWrapper tScrollRect = _scrollRect ;
			if( tScrollRect != null )
			{
				tScrollRect.onValueChanged.RemoveListener( tOnValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			ScrollRectWrapper tScrollRect = _scrollRect ;
			if( tScrollRect != null )
			{
				tScrollRect.onValueChanged.RemoveAllListeners() ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, bool> onContentMoveAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnContentMoveDelegate( string tIdentity, UIScrollView tView, bool tState ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnContentMoveDelegate onContentMoveDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnContentMove( Action<string, UIScrollView, bool> tOnContentMoveAction )
		{
			onContentMoveAction = tOnContentMoveAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnContentMove( OnContentMoveDelegate tOnContentMoveDelegate )
		{
			onContentMoveDelegate += tOnContentMoveDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnContentMove( OnContentMoveDelegate tOnContentMoveDelegate )
		{
			onContentMoveDelegate -= tOnContentMoveDelegate ;
		}

		// 内部リスナー
		private void OnContentMoveInner( bool tState )
		{
			if( onContentMoveAction != null || onContentMoveDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onContentMoveAction?.Invoke( identity, this, tState ) ;
				onContentMoveDelegate?.Invoke( identity, this, tState ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, bool> onContentDragAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnContentDragDelegate( string tIdentity, UIScrollView tView, bool tState ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnContentDragDelegate onContentDragDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnContentDrag( Action<string, UIScrollView, bool> tOnContentDragAction )
		{
			onContentDragAction = tOnContentDragAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnContentDrag( OnContentDragDelegate tOnContentDragDelegate )
		{
			onContentDragDelegate += tOnContentDragDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnContentDrag( OnContentDragDelegate tOnContentDragDelegate )
		{
			onContentDragDelegate -= tOnContentDragDelegate ;
		}

		// 内部リスナー
		private void OnContentDragInner( bool tState )
		{
			if( onContentDragAction != null || onContentDragDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onContentDragAction?.Invoke( identity, this, tState ) ;
				onContentDragDelegate?.Invoke( identity, this, tState ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UIScrollView, float> onStoppedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnStoppedDelegate( string tIdentity, UIScrollView tView, float tContentPosition ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnStoppedDelegate onStoppedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnStopped( Action<string, UIScrollView, float> tOnStoppedAction )
		{
			onStoppedAction = tOnStoppedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnStopped( OnStoppedDelegate tOnStoppedDelegate )
		{
			onStoppedDelegate += tOnStoppedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnStopped( OnStoppedDelegate tOnStoppedDelegate )
		{
			onStoppedDelegate -= tOnStoppedDelegate ;
		}

		// 内部リスナー
		private void OnStoppedInner( float tContentPosition )
		{
			if( onStoppedAction != null || onStoppedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onStoppedAction?.Invoke( identity, this, tContentPosition ) ;
				onStoppedDelegate?.Invoke( identity, this, tContentPosition ) ;
			}
		}
		
		//---------------------------------------------------------------------------

		// スロールバーのフェード関係

		// スクロールバーの状態変化
		private void ChangeScrollbarFadeState( bool tState )
		{
			if( tState == true )
			{
				// 押された
				if( m_ScrollbarFadeState == 0 || m_ScrollbarFadeState == 4 )
				{
					// 完全非表示中かフェードアウト中

					m_ScrollbarFadeState  = 1 ;	// フェードイン

					// 現在のアルファ値から初期の時間値を算出する
					m_ScrollbarFadeTime = m_ScrollbarFadeInDuration * m_ScrollbarFadeAlpha ;
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
				else
				if( m_ScrollbarFadeState == 3 )
				{
					// 一定時間待ってフェードアウト中
					m_ScrollbarFadeState = 2 ;	// リセット
				}
			}
			else
			{
				// 離された
				if( m_ScrollbarFadeState == 1 )
				{
					// フェードイン中

					if( m_ScrollbarFadeHoldDuration == 0 )
					{
						m_ScrollbarFadeState = 4 ;	// フェードアウト
	
						// 現在のアルファ値から初期の時間値を算出する
						m_ScrollbarFadeTime = m_ScrollbarFadeOutDuration * ( 1 - m_ScrollbarFadeAlpha ) ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				if( m_ScrollbarFadeState == 2 )
				{
					// 表示中

					if( m_ScrollbarFadeHoldDuration >  0 )
					{
						m_ScrollbarFadeState = 3 ;	// 一定時間待ってフェードアウト
		
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
					else
					{
						m_ScrollbarFadeState = 4 ;	// フェードアウト
		
						// 現在のアルファ値から初期の時間値を算出する
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
			}
		}

		private float m_HorizontalScrollbarElasticLength = 0 ;
		private float m_VerticalScrollbarElasticLength = 0 ;

		private void ProcessScrollbarElastic()
		{
			if( content == null )
			{
				return ;
			}

			if( m_HorizontalScrollbarElastic != null && this.Width >  0 && content.Width >  0 )
			{
				// 横方向
				if( content.Width <= this.Width )
				{
					m_HorizontalScrollbarElastic.offset = 0 ;
					m_HorizontalScrollbarElastic.length = 1 ;
				}
				else
				{
					if( m_HorizontalScrollbarElasticLength == 0 )
					{
						m_HorizontalScrollbarElasticLength = m_HorizontalScrollbarElastic.length ;
					}

					if( m_HorizontalScrollbarElasticLength >  0 )
					{
						float l = this.Width / content.Width ;

						if( l <  m_HorizontalScrollbarElasticLength )
						{
							l  = m_HorizontalScrollbarElasticLength ;	// 最低保証
						}

						float tW = viewport.Width ;

						float o = ( - content.Rx ) / ( content.Width - tW ) ;	// 位置

						if( o <  0 )
						{
							l  = l + o ;
						}
						else
						if( o >  1 )
						{
							l = l - ( o - 1 ) ;
						}

						m_HorizontalScrollbarElastic.offset = o ;
						m_HorizontalScrollbarElastic.length = l ;
					}
				}
			}

			if( m_VerticalScrollbarElastic != null && this.Height >  0 && content.Height >  0 )
			{
				// 縦方向
				if( content.Height <= this.Height )
				{
					m_VerticalScrollbarElastic.offset = 0 ;
					m_VerticalScrollbarElastic.length = 1 ;
				}
				else
				{
					if( m_VerticalScrollbarElasticLength == 0 )
					{
						m_VerticalScrollbarElasticLength = m_VerticalScrollbarElastic.length ;
					}

					if( m_VerticalScrollbarElasticLength >  0 )
					{
						float l = this.Height / content.Height ;

						if( l <  m_VerticalScrollbarElasticLength )
						{
							l  = m_VerticalScrollbarElasticLength ;	// 最低保証
						}

						float tH = viewport.Height ;

						float o = (   content.Ry ) / ( content.Height - tH ) ;	// 位置

						if( o <  0 )
						{
							l  = l + (   content.Ry                         / tH ) * 1.0f ;
						}
						else
						if( o >  1 )
						{
							l = l -  ( ( content.Ry - ( content.Height - tH ) ) / tH ) * 1.0f ;
						}

						m_VerticalScrollbarElastic.offset = 1 - o ;
						m_VerticalScrollbarElastic.length = l ;
					}
				}
			}
		}

		/// <summary>
		/// スクロールバーのフェード処理を停止する
		/// </summary>
		public void StopScrollbarFade()
		{
			m_ScrollbarFadeState = 0 ;

			float tAlpha = 0 ;

			if( m_HorizontalScrollbarCanvasGroup != null )
			{
				m_HorizontalScrollbarCanvasGroup.alpha = tAlpha ;
				m_HorizontalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
			}

			if( m_VerticalScrollbarCanvasGroup != null )
			{
				m_VerticalScrollbarCanvasGroup.alpha = tAlpha ;
				m_VerticalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
			}
		}

		private CanvasGroup m_HorizontalScrollbarCanvasGroup = null ;
		private CanvasGroup m_VerticalScrollbarCanvasGroup   = null ;

		private bool m_Drag = false ;

		// スクロールバーのフェード処理
		private void ProcessScrollbarFade()
		{
			if( _scrollRect.isDrag == true )
			{
				if( m_Drag == false )
				{
					if( contentSize >  viewSize )
					{
						ChangeScrollbarFadeState( true ) ;
					}
					m_Drag = true ;
				}
			}
			else
			{
				if( _scrollRect.isPress == true )
				{
					if( m_Drag == false )
					{
						if( contentSize >  viewSize )
						{
							ChangeScrollbarFadeState( true ) ;
						}
						m_Drag = true ;
					}
				}
				else
				{
					if( m_Drag == true )
					{
						if( contentSize >  viewSize )
						{
							ChangeScrollbarFadeState( true ) ;
						}
						m_Drag = false ;
					}
				}
			}

			float tAlpha = 0 ;

			if( m_ScrollbarFadeState == 1 )
			{
				// フェードイン中
				if( m_ScrollbarFadeInDuration >  0 )
				{
					// フェードインを実行
					float tTime = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) + m_ScrollbarFadeTime ;
					if( tTime >= m_ScrollbarFadeInDuration )
					{
						tTime  = m_ScrollbarFadeInDuration ;
					}

					tAlpha = tTime / m_ScrollbarFadeInDuration ;
					if( tTime >= m_ScrollbarFadeInDuration )
					{
						m_ScrollbarFadeState = 2 ;	// 表示状態へ
	
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					// 即時表示状態へ
					tAlpha = 1 ;

					m_ScrollbarFadeState = 2 ;	// 表示状態へ

					m_ScrollbarFadeTime = 0 ;
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 2 )
			{
				tAlpha = 1 ;

				bool tScrollbar = false ;
				if( horizontalScrollbar != null && horizontalScrollbar.isPress == true )
				{
					tScrollbar = true ;
				}
				if( verticalScrollbar != null && verticalScrollbar.isPress == true )
				{
					tScrollbar = true ;
				}

				if( m_Press == false && isMoving == false && tScrollbar == false )
				{
					// クリックで表示されたケース
					if( m_ScrollbarFadeHoldDuration >  0 )
					{
						float tTime = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) ;
						if( tTime >= m_ScrollbarFadeHoldDuration )
						{
							// 時間が経過したのでフェードアウトへ
	
							m_ScrollbarFadeState = 4 ;	// 表示状態へ
	
							m_ScrollbarFadeTime = 0 ;
							m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
						}
					}
					else
					{
						// 即時フェードアウトへ
						m_ScrollbarFadeState = 4 ;	// 表示状態へ
	
						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 3 )
			{
				// 一定時間待ってフェードアウト
				tAlpha = 1 ;

				bool tScrollbar = false ;
				if( horizontalScrollbar != null && horizontalScrollbar.isPress == true )
				{
					tScrollbar = true ;
				}
				if( verticalScrollbar != null && verticalScrollbar.isPress == true )
				{
					tScrollbar = true ;
				}

				if( isMoving == false && tScrollbar == false )
				{
					float tTime = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) ;
					if( tTime >= m_ScrollbarFadeHoldDuration )
					{
						// 時間が経過したのでフェードアウトへ

						m_ScrollbarFadeState = 4 ;	// 表示状態へ

						m_ScrollbarFadeTime = 0 ;
						m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
					}
				}
				else
				{
					m_ScrollbarBaseTime = Time.realtimeSinceStartup ;
				}
			}
			else
			if( m_ScrollbarFadeState == 4 )
			{
				// フェードウト中
				if( m_ScrollbarFadeOutDuration >  0 )
				{
					// フェードアウトを実行
					float tTime = ( Time.realtimeSinceStartup - m_ScrollbarBaseTime ) + m_ScrollbarFadeTime ;
					if( tTime >= m_ScrollbarFadeOutDuration )
					{
						tTime  = m_ScrollbarFadeOutDuration ;
					}
	
					tAlpha = 1 - ( tTime / m_ScrollbarFadeInDuration ) ;
					if( tTime >= m_ScrollbarFadeOutDuration )
					{
						m_ScrollbarFadeState = 0 ;	// 非表示状態へ
					}
				}
				else
				{
					// 即時非表示へ

					tAlpha = 0 ;

					m_ScrollbarFadeState = 0 ;	// 非表示状態へ
				}

			}

			m_ScrollbarFadeAlpha = tAlpha ;

			//------------------------------------------

			if( m_HorizontalScrollbarCanvasGroup != null )
			{
				m_HorizontalScrollbarCanvasGroup.alpha = tAlpha ;
				m_HorizontalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
			}

			if( m_VerticalScrollbarCanvasGroup != null )
			{
				m_VerticalScrollbarCanvasGroup.alpha = tAlpha ;
				m_VerticalScrollbarCanvasGroup.blocksRaycasts = tAlpha == 1 ? true : false ; ;
			}
		}

		// スクロールバーの位置を設定する
		virtual internal protected void SetPositionFromScrollbar( Direction tDirection, float tValue )
		{
			if( content == null )
			{
				return ;
			}

			if( tValue <  0 )
			{
				tValue  = 0 ;
			}
			else
			if( tValue >  1 )
			{
				tValue  = 1 ;
			}

			if( tDirection == Direction.Horizontal )
			{
				// 横位置を設定する
				if( content.Width <= 0 || this.Width <= 0 )
				{
					return ;
				}

				if( content.Width <= this.Width )
				{
					return ;
				}

				float l = content.Width - this.Width ;
				float o = l * tValue ;

				content.Px = - o ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				// 縦位置を設定する
				if( content.Height <= 0 || this.Height <= 0 )
				{
					return ;
				}

				if( content.Height <= this.Height )
				{
					return ;
				}

				float l = content.Height - this.Height ;
				float o = l * ( 1 - tValue ) ;

				content.Py =   o ;
			}

			ChangeScrollbarFadeState( true ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定の位置まで移動させる
		/// </summary>
		/// <param name="tContentPosition"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public MovableState MoveToPosition( float tContentPosition, float tDuration )
		{
			if( tDuration <= 0 )
			{
				return null ;
			}

			MovableState tState = new MovableState() ;
			StartCoroutine( MoveToPosition_Private( tContentPosition, tDuration, tState ) ) ;
			return tState ;
		}

		protected IEnumerator MoveToPosition_Private( float contentPositionTo, float duration, MovableState state )
		{
			// 自動移動開始
			m_IsAutoMoving = true ;

			float contentPositionFrom = contentPosition ;

			float baseTime = Time.realtimeSinceStartup ;
			
			float time, factor, delta ;


			// ムーブ処理中
			while( true )
			{
				time = Time.realtimeSinceStartup - baseTime ;

				factor = time / duration ;
				if( factor >  1 )
				{
					factor  = 1 ;
				}

				delta = contentPositionTo - contentPositionFrom ;
				delta = UITween.GetValue( 0, delta, factor, UITween.ProcessTypes.Ease, UITween.EaseTypes.EaseOutQuad ) ;
				
				contentPosition = contentPositionFrom + delta ;

				if( factor >= 1 || m_IsContentDrag == true || m_Press == true )
				{
					break ;
				}

				yield return null ;
			}
			
			if( state != null )
			{
				state.IsDone = true ;
			}

			// 自動移動終了
			m_IsAutoMoving = false ;
		}
	}
}
