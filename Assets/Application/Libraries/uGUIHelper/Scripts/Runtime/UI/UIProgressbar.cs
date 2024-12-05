using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// プログレスバークラス(複合UI)
	/// </summary>
	public class UIProgressbar : UIImage
	{
		/// <summary>
		/// 領域部のインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Scope ;
		public UIImage		Scope{ get{ return m_Scope ; } set{ m_Scope = value ; } }

		/// <summary>
		/// 画像部のインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage m_Thumb ;
		public UIImage		Thumb{ get{ return m_Thumb ; } set{ m_Thumb = value ; } }

		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		[SerializeField]
		protected UINumberMesh m_LabelMesh ;
		public    UINumberMesh   LabelMesh{ get{ return m_LabelMesh ; } set{ m_LabelMesh = value ; } }

		/// <summary>
		/// 形状タイプ
		/// </summary>
		public enum ShapeTypes
		{
			Rectangle,
			Circle,
		}

		[SerializeField][HideInInspector]
		private ShapeTypes m_ShapeType = ShapeTypes.Rectangle ;

		/// <summary>
		/// 形状タイプ
		/// </summary>
		public  ShapeTypes   ShapeType
		{
			get
			{
				return m_ShapeType ;
			}
			set
			{
				if( m_ShapeType != value )
				{
					m_ShapeType  = value ;

					m_IsRefresh = true ;
				}
			}
		}

		/// <summary>
		/// バーの表示タイプ
		/// </summary>
		public enum DisplayTypes
		{
			Stretch = 0,
			Mask = 1,
		}

		[SerializeField][HideInInspector]
		private DisplayTypes m_DisplayType = DisplayTypes.Stretch ;

		/// <summary>
		/// バーの表示タイプ
		/// </summary>
		public  DisplayTypes   DisplayType
		{
			get
			{
				return m_DisplayType ;
			}
			set
			{
				if( m_DisplayType != value )
				{
					m_DisplayType  = value ;

					m_IsRefresh = true ;
				}
			}
		}


		/// <summary>
		/// リング型の反転の有無
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_ThumbClockwise = false ;

		/// <summary>
		/// リング型の反転の有無
		/// </summary>
		public  bool   ThumbClockwise
		{
			get
			{
				return m_ThumbClockwise ;
			}
			set
			{
				// 同値かどうかのチェックを行ってはらない(ゲージの表示が違っている状態であっても同値だと更新されない)
                if( m_ThumbClockwise != value )
                {
				    m_ThumbClockwise  = value ;

				    m_IsRefresh = true ;
                }
			}
		}

		/// <summary>
		/// 値(係数)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Value = 1 ;

		/// <summary>
		/// 値(係数)
		/// </summary>
		public  float   Value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				// 同値かどうかのチェックを行ってはらない(ゲージの表示が違っている状態であっても同値だと更新されない)
				m_Value = value ;

				m_IsRefresh = true ;
			}
		}

		/// <summary>
		/// 値(即値)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Number = 100.0f ;

		/// <summary>
		/// 値(即値)
		/// </summary>
		public  float   Number
		{
			get
			{
				return m_Number ;
			}
			set
			{
				// 同値かどうかのチェックを行ってはらない(ゲージの表示が違っている状態であっても同値だと更新されない)
				m_Number = value ;

				m_IsRefresh = true ;
			}
		}

		/// <summary>
		/// 反転の有無
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_IsReverse = false ;

		/// <summary>
		/// 反転の有無
		/// </summary>
		public  bool   IsReverse
		{
			get
			{
				return m_IsReverse ;
			}
			set
			{
				// 同値かどうかのチェックを行ってはらない(ゲージの表示が違っている状態であっても同値だと更新されない)
				m_IsReverse = value ;

				m_IsRefresh = true ;
			}
		}

		//-----------------------------------------------------

		private bool m_IsRefresh = true ;

		//-----------------------------------------------------

		override protected void OnAwake()
		{
			base.OnAwake() ;

			m_IsRefresh = true ;
		}

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		protected override void OnBuild( string option = "" )
		{
			if( string.IsNullOrEmpty( option ) == true || option == "Rectangle" )
			{
                m_ShapeType = ShapeTypes.Rectangle ;

				Vector2 size = GetCanvasSize() ;
				if( size.x >  0 && size.y >  0 )
				{
					float s ;
					if( size.x <= size.y )
					{
						s = size.x ;
					}
					else
					{
						s = size.y ;
					}
					SetSize( s * 0.5f, s * 0.05f ) ;
				}


				Sprite defaultFrameSprite = null ;
				Sprite defaultThumbSprite = null ;

#if UNITY_EDITOR

				if( Application.isPlaying == false )
				{
					// メニューから操作した場合のみ自動設定を行う
					var ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
					if( ds != null )
					{
						defaultFrameSprite		= ds.ProgressbarFrame ;
						defaultThumbSprite		= ds.ProgressbarThumb ;
					}
				}
			
#endif

				SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/UIProgressbar" ) ;

				// Frame
				Image frame = CImage ;

				if( defaultFrameSprite == null )
				{
					frame.sprite = spriteSet[ "UIProgressbar_Frame" ] ;
				}
				else
				{
					frame.sprite = defaultFrameSprite ;
				}
				frame.type = Image.Type.Sliced ;
				frame.fillCenter = true ;

				if( IsCanvasOverlay == true )
				{
					frame.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
				}

				UIView fillArea = AddView<UIView>( "Fill Area" ) ;
				fillArea.SetAnchorToStretch() ;

				// Mask
				m_Scope = fillArea.AddView<UIImage>( "Scope" ) ;
				m_Scope.SetAnchorToStretch() ;
				m_Scope.SetMargin( 0, 0, 0, 0 ) ;

				m_Scope.IsMask = true ;
				m_Scope.ShowMaskGraphic = false ;

				if( IsCanvasOverlay == true )
				{
					m_Scope.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
				}

				// Thumb
				m_Thumb = m_Scope.AddView<UIImage>( "Thumb" ) ;
				m_Thumb.SetAnchorToStretch() ;
				m_Thumb.SetMargin( 0, 0, 0, 0 ) ;

				if( defaultThumbSprite == null )
				{
					m_Thumb.Sprite = spriteSet[ "UIProgressbar_Thumb" ] ;
				}
				else
				{
					m_Thumb.Sprite = defaultThumbSprite ;
				}
				m_Thumb.Type = Image.Type.Sliced ;
				m_Thumb.FillCenter = true ;

				if( IsCanvasOverlay == true )
				{
					m_Thumb.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
				}

				UpdateThumb() ;

				// Label
				m_LabelMesh = AddView<UINumberMesh>( "Label" ) ;
				m_LabelMesh.FontSize = ( int )( this.Height * 0.6f ) ;
				m_LabelMesh.IsOutline = true ;
				m_LabelMesh.Percent = true ;

//			    if( IsCanvasOverlay == true )
//			    {
//				    m_Label.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
//			    }

				UpdateLabel() ;

//			    DestroyImmediate( atlas ) ;
			}
			else
            if( option == "Circle" )
			{
				// Ring タイプ

                m_ShapeType = ShapeTypes.Circle ;
                m_ThumbClockwise = true ;

				Vector2 size = GetCanvasSize() ;
				if( size.x >  0 && size.y >  0 )
				{
					float s ;
					if( size.x <= size.y )
					{
						s = size.x ;
					}
					else
					{
						s = size.y ;
					}
					SetSize( s * 0.1f, s * 0.1f ) ;
				}

				SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/UISimpleJoystick" ) ;

				// Frame
				Image frame = CImage ;

				frame.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
				frame.type = Image.Type.Sliced ;
				frame.fillCenter = true ;

				if( IsCanvasOverlay == true )
				{
					frame.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
				}

				// Thumb
				m_Thumb = AddView<UIImage>( "Thumb" ) ;
				m_Thumb.SetAnchorToStretch() ;
				m_Thumb.SetMargin( 0, 0, 0, 0 ) ;

				m_Thumb.Sprite = spriteSet[ "UISimpleJoystick_Thumb_Type_0" ] ;

				m_Thumb.Type = Image.Type.Simple ;

				if( IsCanvasOverlay == true )
				{
					m_Thumb.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
				}

                m_Thumb.Type = Image.Type.Filled ;
                m_Thumb.FillOriginType = FillOriginTypes.Top ;

				UpdateThumb() ;

				// Label
				m_LabelMesh = AddView<UINumberMesh>( "Label" ) ;
				m_LabelMesh.FontSize = ( int )( this.Height * 0.2f ) ;
				m_LabelMesh.IsOutline = true ;
				m_LabelMesh.Percent = true ;
			}
		}

		//----------------------------------------------------

		// HorizontalLayoutGroup VerticalLayoutGroup GridLayoutGroup は Update() のタイミングで処理され。
		// それまで、その子ビュー deltaSize は正確な値が取れない。
		// m_Scope.Width もその影響を受けるため、UIProgress の処理は LateUpdate() のタイミングで行う必要がある。
		// すなわち UIView.Width UIView.Height は、LayoutGroup の子である場合は、
		// 最初のフレームでは正確な値が取れない。
		// 回避するためには、スタティックメソッド Canvas.ForceUpdateCanvases() ; を実行してから値を取る事。
		protected override void OnLateUpdate()
		{
			if( m_IsRefresh == true )
			{
				UpdateThumb() ;
				UpdateLabel() ;

				m_IsRefresh = false ;
			}
		}
		
		/// <summary>
		/// Thumb 更新
		/// </summary>
		private void UpdateThumb()
		{
			float value = m_Value ;
//			if( value <  0.01f )
//			{
//				// 極端に小さい値だとバグるため最小制限を入れる
//				value  = 0.01f ;
//			}

			if( m_IsReverse == true )
			{
				value = 1.0f - value ;
			}

			if( m_ShapeType == ShapeTypes.Rectangle )
			{
				if( m_Scope != null && m_Thumb != null )
				{
					if( m_DisplayType == DisplayTypes.Stretch )
					{
						if( value <= 0 )
						{
							m_Scope.SetActive( false ) ;
						}
						else
						{
							m_Scope.SetActive( true ) ;
	
							m_Scope.SetAnchorToStretch() ;
							m_Scope.SetAnchorMin(       0, 0 ) ;
							m_Scope.SetAnchorMax( value, 1 ) ;

							m_Thumb.SetAnchorToStretch() ;
							m_Thumb.SetMargin(   0,   0,   0,   0 ) ;
						}
					}
					else
					if( m_DisplayType == DisplayTypes.Mask )
					{
						if( value <= 0 )
						{
							m_Scope.SetActive( false ) ;
						}
						else
						{
							m_Scope.SetActive( true ) ;
							m_Scope.SetAnchorToStretch() ;

							float d = m_Scope.Width * ( 1.0f - value ) ;

							m_Scope.SetMargin(  0, d,  0,  0 ) ;
	
							m_Thumb.SetAnchorToStretch() ;
							m_Thumb.SetMargin(  0,-d,  0,  0 ) ;
						}
					}
				}
			}
			else
			if( m_ShapeType == ShapeTypes.Circle )
			{
				if( m_Thumb != null )
				{
					m_Thumb.FillClockwise   = m_ThumbClockwise ;
					m_Thumb.FillAmount      = value ;
				}
			}
		}

		/// <summary>
		/// Label 更新
		/// </summary>
		private void UpdateLabel()
		{
			if( m_LabelMesh != null )
			{
				m_LabelMesh.Value = m_Value * m_Number ;
			}
		}
	}
}
