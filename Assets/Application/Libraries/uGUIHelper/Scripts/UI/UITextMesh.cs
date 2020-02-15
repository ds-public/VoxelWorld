//#if TextMeshPro

using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;

using TMPro ;

namespace uGUIHelper
{
	[RequireComponent( typeof( TextMeshProUGUI ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UITextMesh : UIView
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return Vector2.zero ;
				}
				return new Vector2( textMesh.preferredWidth, textMesh.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int FontSize
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return 0 ;
				}
				return ( int )textMesh.fontSize ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.fontSize != value )
				{
					textMesh.fontSize = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return null ;
				}
				return textMesh.text ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.text != value )
				{
					textMesh.text = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public TMP_FontAsset Font
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return null ;
				}
				return textMesh.font ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.font != value )
				{
					textMesh.font = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return Color.white ;
				}
				return textMesh.color ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.color.r != value.r || textMesh.color.g != value.g || textMesh.color.b != value.b || textMesh.color.a != value.a )
				{
					textMesh.color = value ;
				}
			}
		}
		
		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyles FontStyle
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return FontStyles.Normal ;
				}
				return textMesh.fontStyle ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.fontStyle != value )
				{
					textMesh.fontStyle = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return false ;
				}
				return textMesh.richText ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.richText != value )
				{
					textMesh.richText = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAlignmentOptions Alignment
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return TextAlignmentOptions.Center ;
				}
				return textMesh.alignment ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.alignment != value )
				{
					textMesh.alignment = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return false ;
				}
				return textMesh.enableAutoSizing ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.enableAutoSizing != value )
				{
					textMesh.enableAutoSizing = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  bool HorizontalOverflow
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return false ;
				}
				return ( ! textMesh.enableWordWrapping ) ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.enableWordWrapping != ( ! value ) )
				{
					textMesh.enableWordWrapping = ( ! value ) ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  TextOverflowModes VerticalOverflow
		{
			get
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return TextOverflowModes.Overflow ;
				}
				return textMesh.overflowMode ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.overflowMode != value )
				{
					textMesh.overflowMode = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return 1.0f ;
				}
				return textMesh.lineSpacing ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}

				if( textMesh.lineSpacing != value )
				{
					textMesh.lineSpacing = value ;

					if( m_AutoSizeFitting == true )
					{
						Resize() ;
					}

					// 加工用に頂点情報をバックアップする
//					SetTextInfo() ;
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
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return false ;
				}
				return textMesh.raycastTarget ;
			}
			set
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh == null )
				{
					return ;
				}
				textMesh.raycastTarget = value ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 装飾の一部を変更するか
		/// </summary>
		public bool	IsCustomized = false ;

		/// <summary>
		/// アウトラインの色(実行時にのみ反映)
		/// </summary>
		public Color	OutlineColor = Color.black ;

		private Material	m_BasisMaterial = null ;
		private Material	m_CloneMaterial = null ;

		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params uint[] code )
		{
			if( code == null || code.Length == 0 )
			{
				return ;
			}

			int i, l = code.Length ;

			Color[] color = new Color[ l ] ;
			uint c ;
			byte r, g, b ,a ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = code[ i ] ;

				r = ( byte )( ( c >> 16 ) & 0xFF ) ;
				g = ( byte )( ( c >>  8 ) & 0xFF ) ;
				b = ( byte )(   c         & 0xFF ) ;
				a = ( byte )( ( c >> 24 ) & 0xFF ) ;

				color[ i ] = new Color32(  r, g, b, a ) ;
			}

			SetGradientColor( color ) ;
		}
		
		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params Color[] colors )
		{
			TextMeshProUGUI textMesh = _textMesh ;
			if( textMesh == null )
			{
				return ;
			}

			if( colors == null || colors.Length == 0 )
			{
				return ;
			}

			int l = colors.Length ;

			if( l == 1 )
			{
				// 単色

				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 0 ],
					bottomLeft	= colors[ 0 ],
					bottomRight	= colors[ 0 ],
				} ;

				textMesh.colorGradient = vg ;
			}
			else
			if( l == 2 || l == 3 )
			{
				// 上下
				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 0 ],
					bottomLeft	= colors[ 1 ],
					bottomRight	= colors[ 1 ],
				} ;

				textMesh.colorGradient = vg ;
			}
			else
			if( l >= 4 )
			{
				// 全体
				VertexGradient vg =	new VertexGradient()
				{
					topLeft		= colors[ 0 ],
					topRight	= colors[ 1 ],
					bottomLeft	= colors[ 2 ],
					bottomRight	= colors[ 3 ],
				} ;

				textMesh.colorGradient = vg ;
			}
		}

		//-----------------------------------------------------------

		// ローカライズをリクエスト中か
		private bool m_LocalizeRequest ;

		//--------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			TextMeshProUGUI textMesh = _textMesh ;
			if( textMesh == null )
			{
				textMesh = gameObject.AddComponent<TextMeshProUGUI>() ;
			}
			if( textMesh == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	defaultTextColor = Color.white ;

			TMP_FontAsset	defaultFontAsset = null ;
			Material		defaultFontMaterial = null ;
			int				defaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					defaultTextColor	= ds.textColor ;

					defaultFontAsset	= ds.fontAsset ;
					defaultFontMaterial	= ds.fontMaterial ; 
					defaultFontSize		= ds.fontSize ;
				}
			}
			
#endif

			textMesh.color = defaultTextColor ;

			if( defaultFontAsset == null )
			{
				textMesh.font = Resources.Load<TMP_FontAsset>( "Fonts & Materials/LiberationSans SDF" ) ;
			}
			else
			{
				textMesh.font = defaultFontAsset ;
			}

			if( defaultFontMaterial != null )
			{
				textMesh.fontMaterial = defaultFontMaterial ; 
			}

			if( defaultFontSize <= 0 )
			{
				textMesh.fontSize = 32 ;
			}
			else
			{
				textMesh.fontSize = defaultFontSize ;
			}

			textMesh.alignment = TextAlignmentOptions.TopLeft ;

			textMesh.enableWordWrapping = false ;
			textMesh.overflowMode = TextOverflowModes.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			textMesh.raycastTarget = false ;

		}

/*		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;

				if( textMesh != null )
				{
					textMesh.RegisterDirtyVerticesCallback( OnDirtyVertices ) ;
				}
			}
		}*/


		override protected void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;

				if( textMesh != null )
				{
					if( IsCustomized == true )
					{
						if( textMesh.material != null )
						{
							m_BasisMaterial = textMesh.fontMaterial ;
							m_CloneMaterial = GameObject.Instantiate<Material>( textMesh.fontMaterial ) ;
							textMesh.fontMaterial = m_CloneMaterial ;

							UpdateCustom( textMesh ) ;
						}
					}

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

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh != null )
				{
					if( IsCustomized == true )
					{
						if( textMesh.material != null )
						{
							UpdateCustom( textMesh ) ;
						}
					}
				}
			}
		}

		private void UpdateCustom( TextMeshProUGUI textMesh )
		{
			if( m_CloneMaterial != null )
			{
				if( m_CloneMaterial.HasProperty( "_OutlineColor" ) == true )
				{
					m_CloneMaterial.SetColor(	"_OutlineColor",	OutlineColor ) ;

					textMesh.SetMaterialDirty() ;
				}
			}
		}


		override protected void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_CloneMaterial != null )
			{
				TextMeshProUGUI textMesh = _textMesh ;
				textMesh.fontMaterial = m_BasisMaterial ;

				DestroyImmediate( m_CloneMaterial ) ;
				m_CloneMaterial = null ;

				m_BasisMaterial = null ;
			}

			if( m_LocalizeRequest == true )
			{
				// ローカライズのリクエスト中である場合はリクエストをキャンセルする
				UILocalize.RemoveRequest( OnLocalized ) ;
				m_LocalizeRequest = false ;
			}
		}

		override protected void OnLateUpdate()
		{
//			ProcessVertexModifier() ;
			
			if( m_AutoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			TextMeshProUGUI t = _textMesh ;
			RectTransform r = GetRectTransform() ;
			if( r != null && t != null )
			{
				Vector2 tSize = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.enableWordWrapping == false )
					{
						tSize.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.overflowMode == TextOverflowModes.Overflow )
					{
						tSize.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = tSize ;
			}
		}


		//---------------------------------------------------------------------------

		// ルビフリ方法
		// http://baba-s.hatenablog.com/entry/2019/01/10/122500

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color32[] colors, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || colors == null || colors.Length == 0 )
			{
				return false ;
			}

			textMesh.ForceMeshUpdate() ;

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;
			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}

			//--------------

			if( length <= 0 )
			{
				length  = colors.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= colors.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  colors.Length )
			{
				length = colors.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex ;

			Color32[] baseColors ;

			Color32 replaceColor ;

			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					if( textInfo.characterInfo[ p ].isVisible == true )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex	= textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						replaceColor = new Color32
						(
							colors[ p ].r,
							colors[ p ].g,
							colors[ p ].b,
							colors[ p ].a
						) ;

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			//----------------------------------------------------------
			
			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Colors32 ) ;

			return true ;
		}

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color[] colors )
		{
			if( colors == null || colors.Length == 0 )
			{
				return false ;
			}

			int i, l = colors.Length ;
			Color32[] colors32 = new Color32[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				colors32[ i ] = colors[ i ] ;
			}

			return SetColors( colors32 ) ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// 文字の表示状態を変更する情報体
		/// </summary>
		public class Modifier
		{
			public Vector2	position = Vector2.zero ;
			public float	rotation = 0 ;
			public Vector2	scale = Vector2.one ;

			public float	gamma = 1 ;
			public float	alpha = 1 ;
		}

		/// <summary>
		/// 頂点を変化させる
		/// </summary>
		/// <param name="modifiers"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool SetModifiers( Modifier[] modifiers, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || modifiers == null || modifiers.Length == 0 )
			{
				return false ;
			}

			// メッシュ情報を一新する
			textMesh.ForceMeshUpdate() ;

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;

			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}
			
			//--------------

			if( length <= 0 )
			{
				length  = modifiers.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= modifiers.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  modifiers.Length )
			{
				length = modifiers.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex ;

			Vector3[] baseVertices ;
			Color32[] baseColors ;

			Modifier modifier ;

			Vector3[]	replaceVertices ;
			Vector2		center ;

			Color32		replaceColor ;

			float dx, dy, vx, vy ;
			float cv, sv ;
			
			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					modifier = modifiers[ p ] ;

					if( modifier != null )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex	= textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseVertices	= textInfo.meshInfo[ materialIndex ].vertices ;
						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						// Vertices
						replaceVertices = new Vector3[ 4 ] ;
						center = Vector2.zero ;
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Vertex
							replaceVertices[ j ] = new Vector3
							(
								baseVertices[ vertexIndex + j ].x,
								baseVertices[ vertexIndex + j ].y,
								baseVertices[ vertexIndex + j ].z
							) ;

							center.x += replaceVertices[ j ].x ;
							center.y += replaceVertices[ j ].y ;
						}

						center.x /= 4f ; 
						center.y /= 4f ;

						// 回転→拡縮→移動
						cv = Mathf.Cos( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						sv = Mathf.Sin( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// 回転
							dx = replaceVertices[ j ].x - center.x ;
							dy = replaceVertices[ j ].y - center.y ;

							vx = ( dx * cv ) - ( dy * sv ) ;
							vy = ( dx * sv ) + ( dy * cv ) ;

							// 拡縮
							vx *= modifier.scale.x ;
							vy *= modifier.scale.y ;

							// 移動
							vx += modifier.position.x ;
							vy += modifier.position.y ;

							replaceVertices[ j ].x = vx + center.x ;
							replaceVertices[ j ].y = vy + center.y ;
						}

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseVertices[ vertexIndex + j ] = replaceVertices[ j ] ;
						}

						// Color
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Color
							replaceColor = new Color32
							(
								baseColors[ vertexIndex + j ].r,
								baseColors[ vertexIndex + j ].g,
								baseColors[ vertexIndex + j ].b,
								baseColors[ vertexIndex + j ].a
							) ;

							if( modifier.gamma != 1 )
							{
								replaceColor.r = ( byte )Math.Round( ( float )replaceColor.r * modifier.gamma ) ;
								replaceColor.g = ( byte )Math.Round( ( float )replaceColor.g * modifier.gamma ) ;
								replaceColor.b = ( byte )Math.Round( ( float )replaceColor.b * modifier.gamma ) ;
							}

							if( modifier.alpha != 1 )
							{
								replaceColor.a = ( byte )Math.Round( ( float )replaceColor.a * modifier.alpha ) ;
							}

							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32 ) ;

			//----------------------------------------------------------

			return true ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// １文字ずつ表示する
		/// </summary>
		/// <param name="message"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public UIView.MovableState Play( string message, float duration = 0.1f, int fadeWidth = 2, Func<bool> onFinished = null )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || string.IsNullOrEmpty( message ) == true )
			{
				return null ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親以上がアクティブになっていないので再生できない
				Debug.LogWarning( "Parent is not active" ) ;
				return null ;
			}

			UIView.MovableState state = new MovableState() ;
			StartCoroutine( Play_Private( message, duration, fadeWidth, onFinished, state ) ) ;
			return state ;
		}

		private IEnumerator Play_Private( string message, float duration, int fadeWidth, Func<bool> onFinished, UIView.MovableState state )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			textMesh.text = message ;

			int i, j, l = message.Length ;
			float t, f ;

			Modifier[] m = new Modifier[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m[ i ] = new Modifier(){ alpha = 0 } ;
			}

			SetModifiers( m ) ;

			//----------------------------------

			if( duration <= 0 )
			{
				duration  = 0.05f ;
			}

			if( fadeWidth <= 0 )
			{
				// アルファフェードインを使わずに１文字ずつ表示する(時間は0.1)
				t = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					while( t <  duration )
					{
						t += Time.unscaledDeltaTime ;

						if( t >= duration )
						{
							m[ i ].alpha = 1 ;
						}

						SetModifiers( m, i, l - i ) ;

						if( t >= duration )
						{
							t -= duration ;
							break ;
						}

						if( onFinished != null )
						{
							if( onFinished() == true )
							{
								break ;	// 強制終了
							}
						}

						yield return null ;
					}
				}
			}
			else
			{
				// アルファフェードインを使い１文字ずつ表示する

				float wait = duration * fadeWidth ;

				t = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					while( t <  wait )
					{
						t += Time.unscaledDeltaTime ;

						for( j  = 0 ; j <  fadeWidth ; j ++ )
						{
							f = t - ( duration * j ) ;

							if( f >= wait )
							{
								f  = wait ;
							}
							if( f <  0 )
							{
								f  = 0 ;
							}

							if( ( ( i + j ) <  l ) && f >  0 )
							{
								m[ i + j ].alpha = ( f / wait ) ;
							}
						}
	
						SetModifiers( m, i, l - i ) ;

						if( t >= wait )
						{
							t -= duration ;

							break ;
						}

						if( onFinished != null )
						{
							if( onFinished() == true )
							{
								break ;	// 強制終了
							}
						}
						
						yield return null ;
					}
				}
			}

			// 元の状態に戻す
			textMesh.ForceMeshUpdate() ;

			state.IsDone = true ;
		}
	}
}


//#endif