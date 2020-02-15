using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[RequireComponent( typeof( UnityEngine.UI.Text ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UIText : UIView
	{
		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
		[SerializeField][HideInInspector]
		protected bool m_AutoSizeFitting = true ;

		public bool AutoSizeFitting
		{
			get
			{
				return m_AutoSizeFitting ;
			}
			set
			{
				if( m_AutoSizeFitting != value )
				{
					m_AutoSizeFitting  = value ;
					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// ローカライズキー
		/// </summary>
		[SerializeField][HideInInspector]
		protected string m_LocalizeKey = string.Empty ;

		public string LocalizeKey
		{
			get
			{
				return m_LocalizeKey ;
			}
			set
			{
				if( m_LocalizeKey != value )
				{
					m_LocalizeKey  = value ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 TextSize
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return Vector2.zero ;
				}
				return new Vector2( text.preferredWidth, text.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int FontSize
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return 0 ;
				}
				return text.fontSize ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.fontSize = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string Text
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return null ;
				}
				return text.text ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.text = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public Font Font
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return null ;
				}
				return text.font ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.font = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return Color.white ;
				}
				return text.color ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return null ;
				}
				return text.material ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.material = value ;
			}
		}

		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyle FontStyle
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return FontStyle.Normal ;
				}
				return text.fontStyle ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.fontStyle = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// リッチテキスト(ショートカット)
		/// </summary>
		public bool SupportRichText
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return false ;
				}
				return text.supportRichText ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.supportRichText = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAnchor Alignment
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return TextAnchor.MiddleCenter ;
				}
				return text.alignment ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.alignment = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// リサイズ(ショートカット)
		/// </summary>
		public bool ResizeTextForBestFit
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return false ;
				}
				return text.resizeTextForBestFit ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.resizeTextForBestFit = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  HorizontalWrapMode HorizontalOverflow
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return HorizontalWrapMode.Overflow ;
				}
				return text.horizontalOverflow ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.horizontalOverflow = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  VerticalWrapMode VerticalOverflow
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return VerticalWrapMode.Overflow ;
				}
				return text.verticalOverflow ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.verticalOverflow = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// 改行時の下方向への移動量係数(ショートカット)
		/// </summary>
		public  float LineSpacing
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return 1.0f ;
				}
				return text.lineSpacing ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.lineSpacing = value ;

				if( m_AutoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public bool RaycastTarget
		{
			get
			{
				Text text = _text ;
				if( text == null )
				{
					return false ;
				}
				return text.raycastTarget ;
			}
			set
			{
				Text text = _text ;
				if( text == null )
				{
					return ;
				}
				text.raycastTarget = value ;
			}
		}

		//--------------------------------------------------

		public Shadow	Shadow
		{
			get
			{
				return _shadow ;
			}
		}

		public Outline	Outline
		{
			get
			{
				return _outline ;
			}
		}

		public UIGradient Gradient
		{
			get
			{
				return _gradient ;
			}
		}

		//--------------------------------------------------
		
		// ローカライズをリクエスト中か
		private bool m_LocalizeRequest ;

		//--------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Text text = _text ;
			if( text == null )
			{
				text = gameObject.AddComponent<Text>() ;
			}
			if( text == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	defaultTextColor = Color.white ;

			Font	defaultFont = null ;
			int		defaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultTextColor	= ds.textColor ;

					defaultFont			= ds.font ;
					defaultFontSize		= ds.fontSize ;
				}
			}
			
#endif

			text.color = defaultTextColor ;

			if( defaultFont == null )
			{
				text.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				text.font = defaultFont ;
			}

			if( defaultFontSize <= 0 )
			{
				text.fontSize = 32 ;
			}
			else
			{
				text.fontSize = defaultFontSize ;
			}

			text.alignment = TextAnchor.MiddleLeft ;

			text.horizontalOverflow = HorizontalWrapMode.Overflow ;
			text.verticalOverflow   = VerticalWrapMode.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				text.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			text.raycastTarget = false ;
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				Text text = _text ;

				if( text != null )
				{
					if( string.IsNullOrEmpty( m_LocalizeKey ) == false )
					{
						// ローカライズ対応処理を行う
						if( UILocalize.AddRequest( OnLocalized, m_LocalizeKey ) == false )
						{
							// 準備が整っていないのでキューに貯められた
							m_LocalizeRequest = true ;
						}
					}
				}
			}
		}

		/// <summary>
		/// ローカライズの準備が整い変換後の文字列が返された(ローカライズマネージャーの準備が整わないと設定できない)
		/// </summary>
		/// <param name="text"></param>
		private void OnLocalized( string text )
		{
			Text = text ;
			m_LocalizeRequest = false ;	// ローカライズが行われた
		}

		protected override void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_LocalizeRequest == true )
			{
				// ローカライズのリクエスト中である場合はリクエストをキャンセルする
				UILocalize.RemoveRequest( OnLocalized ) ;
				m_LocalizeRequest = false ;
			}
		}

		override protected void OnLateUpdate()
		{
			if( m_AutoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			Text t = _text ;
			RectTransform r = GetRectTransform() ;
			if( r != null && t != null )
			{
				Vector2 tSize = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.horizontalOverflow == HorizontalWrapMode.Overflow )
					{
						tSize.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.verticalOverflow == VerticalWrapMode.Overflow )
					{
						tSize.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = tSize ;
			}
		}
	}
}

