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
	/// uGUI:Button クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Button ) ) ]
	public class UIButton : UIImage
	{
		/// <summary>
		/// ラベルのビューのインスタンス(キャッシュ)
		/// </summary>
		[SerializeField]
		protected UIText		m_Label ;
		public UIText Label{ get{ return m_Label ; } set{ m_Label = value ; } }
		
		/// <summary>
		/// リッチラベルのビューのインスタンス(キャッシュ)
		/// </summary>
		[SerializeField]
		protected UIRichText	m_RichLabel ; 
		public UIRichText	RichLabel{ get{ return m_RichLabel ; } set{ m_RichLabel = value ; } }

		/// <summary>
		/// ラベルメッシュのビューのインスタンス(キャッシュ)
		/// </summary>
		[SerializeField]
		protected UITextMesh	m_LabelMesh ;
		public UITextMesh	LabelMesh{ get{ return m_LabelMesh ; } set{ m_LabelMesh = value ; } }


		/// <summary>
		/// 無効化状態の際のマスク画像
		/// </summary>
		[SerializeField]
		protected UIImage	m_DisableMask ;
		public UIImage		DisableMask{ get{ return m_DisableMask ; } set{ m_DisableMask = value ; } }


		/// <summary>
		/// クリック時のトランジションを有効にするかどうか
		/// </summary>
		[SerializeField]
		protected bool m_ClickTransitionEnabled = false ;
		public bool ClickTransitionEnabled{ get{ return m_ClickTransitionEnabled ; } set{ m_ClickTransitionEnabled = value ; } }

		/// <summary>
		/// クリック時のトランジションを待ってからクリックのコールバックを呼び出す
		/// </summary>
		[SerializeField]
		protected bool m_WaitForTransition = false ;
		public bool WaitForTransition{ get{ return m_WaitForTransition ; } set{ m_WaitForTransition = value ; } }

		/// <summary>
		/// 子オブジェクトに対しても色変化を伝搬させる
		/// </summary>
		[SerializeField]
		protected bool m_ColorTransmission = false ;
		public bool ColorTransmission{ get{ return m_ColorTransmission ; } set{ m_ColorTransmission = value ; } }

		/// <summary>
		/// ピボットを自動的に実行時に中心にする
		/// </summary>
		[SerializeField]
		protected bool m_AutoPivotToCenter = true ;
		public bool AutoPivotToCenter{ get{ return m_AutoPivotToCenter ; } set{ m_AutoPivotToCenter = value ; } }
		
		//-----------------------------------------------------
	
		private Color	m_ActiveColor = new Color() ;

		protected bool m_ButtonClick = false ;

		public bool IsButtonClick
		{
			get
			{
				return m_ButtonClick ;
			}
		}

		protected int m_ButtonClickCountTime = 0 ;

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Button button = _button ;

			if( button == null )
			{
				button = gameObject.AddComponent<Button>() ;
			}
			if( button == null )
			{
				// 異常
				return ;
			}

			Image image = _image ;

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
					defaultSprite		= ds.buttonFrame ;
					defaultColor		= ds.buttonDisabledColor ;
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
			isTransition = true ;

			// イベントトリガーは不要
//			isEventTrigger = false ;

			ResetRectTransform() ;
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _button != null )
				{
					_button.onClick.AddListener( OnButtonClickInner ) ;

					if( AutoPivotToCenter == true )
					{
						SetPivot( 0.5f, 0.5f, true ) ;	
					}
				}

				if( _image != null )
				{
					Image image = _image ;
					m_ActiveColor.r = image.color.r ;
					m_ActiveColor.g = image.color.g ;
					m_ActiveColor.b = image.color.b ;
					m_ActiveColor.a = image.color.a ;
				}
			}
		}

		//---------------------------------------------
	
		/// <summary>
		/// ボタンをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIButton> OnButtonClickAction ;
		
		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		public delegate void ButtonClickDelegate( string identity, UIButton view ) ;

		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲート
		/// </summary>
		public ButtonClickDelegate OnButtonClickDelegate ;

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
			if( ClickTransitionEnabled == false || ( ClickTransitionEnabled == true && WaitForTransition == false ) )
			{
				m_ButtonClick = true ;
				m_ButtonClickCountTime = Time.frameCount ;

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
				UITransition transition = _transition ;
				if( transition != null )
				{
					transition.OnClicked( WaitForTransition ) ;
				}
			}
		}
		
		internal protected void OnButtonClickFromTransition()
		{
			if( WaitForTransition == true )
			{
				m_ButtonClick = true ;
				m_ButtonClickCountTime = Time.frameCount ;

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


		/// <summary>
		/// ボタンをクリックされた際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnButtonClickAction">アクションメソッド</param>
		public void SetOnButtonClick( Action<string, UIButton> onButtonClickAction )
		{
			OnButtonClickAction = onButtonClickAction ;
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnButtonClickDelegate">デリゲートメソッド</param>
		public void AddOnButtonClick( ButtonClickDelegate onButtonClickDelegate )
		{
			OnButtonClickDelegate += onButtonClickDelegate ;
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnButtonClickDelegate">デリゲートメソッド</param>
		public void RemoveOnButtonClick( ButtonClickDelegate onButtonClickDelegate )
		{
			OnButtonClickDelegate -= onButtonClickDelegate ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを追加する
		/// </summary>
		/// <param name="tOnClick">リスナーメソッド</param>
		public void AddOnClickListener( UnityEngine.Events.UnityAction onClick )
		{
			Button button = _button ;
			if( button != null )
			{
				button.onClick.AddListener( onClick ) ;
			}
		}
		
		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを削除する
		/// </summary>
		/// <param name="tOnClick">リスナーメソッド</param>
		public void RemoveOnClickListener( UnityEngine.Events.UnityAction onClick )
		{
			Button button = _button ;
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
			Button button = _button ;
			if( button != null )
			{
				button.onClick.RemoveAllListeners() ;
			}
		}
	
		//---------------------------------------------
		
		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIText AddLabel( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( Label == null )
			{
				Label = AddView<UIText>() ;
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
					defaultFont		= ds.font ;
					defaultFontSize	= ds.buttonLabelFontSize ;
					defaultColor	= ds.buttonLabelColor ;
					defaultShadow	= ds.buttonLabelShadow ;
					defaultOutline	= ds.buttonLabelOutline ;
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

			label.isShadow	= defaultShadow ;
			label.isOutline	= defaultOutline ;

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
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIRichText AddRichLabel( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( RichLabel == null )
			{
				RichLabel = AddView<UIRichText>() ;
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
					defaultFont		= ds.font ;
					defaultFontSize	= ds.buttonLabelFontSize ;
					defaultColor	= ds.buttonLabelColor ;
					defaultShadow	= ds.buttonLabelShadow ;
					defaultOutline	= ds.buttonLabelOutline ;
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

			label.isShadow	= defaultShadow ;
			label.isOutline	= defaultOutline ;


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
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UITextMesh AddLabelMesh( string text, uint color = 0xFFFFFFFF, int fontSize = 0 )
		{
			if( LabelMesh == null )
			{
				LabelMesh = AddView<UITextMesh>() ;
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
//					defaultFont		= ds.font ;
					defaultFontSize	= ds.buttonLabelFontSize ;
					defaultColor	= ds.buttonLabelColor ;
					defaultShadow	= ds.buttonLabelShadow ;
					defaultOutline	= ds.buttonLabelOutline ;
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

			label.isShadow	= defaultShadow ;
			label.isOutline	= defaultOutline ;

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
		/// <param name="tLabelText"></param>
		public void SetAnyLabelsText( string tLabelText )
		{
			if( Label != null )
			{
				Label.Text = tLabelText ;
			}

			if( RichLabel != null )
			{
				RichLabel.Text = tLabelText ;
			}

			if( LabelMesh != null )
			{
				LabelMesh.Text = tLabelText ;
			}
		}


		//---------------------------------------------------------------------------
		
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				Button button = _button ;
				if( button == null )
				{
					return false ;
				}
				return button.interactable ;
			}
			set
			{
				Button button = _button ;
				if( button == null )
				{
					return ;
				}
				button.interactable = value ;

				if( DisableMask != null )
				{
					DisableMask.SetActive( ! value ) ;
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="tARGB">カラー値(AARRGGBB)</param>
		public void On( uint color = 0xFFFFFFFF )
		{
			Interactable = true ;
			if( ( color & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   color         & 0xFF ) ;
				byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

				Image image = _image ;
				if( image != null )
				{
					image.color = new Color32( r, g, b, a ) ;
					if( ColorTransmission == true )
					{
						SetColorOfChildren( image.color ) ;
					}
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="tARGB">カラー値(AARRGGBB)</param>
		public void Off( uint color = 0xC0C0C0C0 )
		{
			Interactable = false ;
			if( ( color & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   color         & 0xFF ) ;
				byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

				Image image = _image ;
				if( image != null )
				{
					image.color = new Color32( r, g, b, a ) ;
					if( ColorTransmission == true )
					{
						SetColorOfChildren( image.color ) ;
					}
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション設定
		/// </summary>
		/// <param name="tState"></param>
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

		public void SetColorOfChildren( Color color )
		{
			int i, l ;

			Image[] images = transform.GetComponentsInChildren<Image>( true ) ;
			if( images != null && images.Length >  0 )
			{
				l = images.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( images[ i ].transform != transform )
					{
						images[ i ].color = color ;
					}
				}
			}

/*			Text[] texts = transform.GetComponentsInChildren<Text>( true ) ;
			if( texts != null && texts.Length >  0 )
			{
				l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( texts[ i ].transform != transform )
					{
						texts[ i ].color = tColor ;
					}
				}
			}

			RichText[] richTexts = transform.GetComponentsInChildren<RichText>( true ) ;
			if( richTexts != null && richTexts.Length >  0 )
			{
				l = richTexts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( richTexts[ i ].transform != transform )
					{
						richTexts[ i ].color = tColor ;
					}
				}
			}*/
		}

		override protected void OnUpdate()
		{
			if( m_ButtonClick == true && m_ButtonClickCountTime != Time.frameCount )
			{
				m_ButtonClick = false ;
			}

			if( ColorTransmission == true )
			{
				if( _image != null )
				{
					Image image = _image ;
					if( image.color.Equals( m_ActiveColor ) == false )
					{
						m_ActiveColor.r = image.color.r ;
						m_ActiveColor.g = image.color.g ;
						m_ActiveColor.b = image.color.b ;
						m_ActiveColor.a = image.color.a ;

						SetColorOfChildren( m_ActiveColor ) ;
					}
				}
			}
		}
	}
}
