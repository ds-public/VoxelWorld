using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;


namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Image ) ) ]
	public class UIImage : UIView
	{
		[SerializeField][HideInInspector]
		private UIAtlasSprite m_AtlasSprite = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  UIAtlasSprite  AtlasSprite
		{
			get
			{
				return m_AtlasSprite ;
			}
			set
			{
				if( m_AtlasSprite != value )
				{
					m_AtlasSprite  = value ;

					Sprite sprite = null ;
					if( m_AtlasSprite != null )
					{
						if( m_AtlasSprite.length >  0 )
						{
							sprite = m_AtlasSprite[ m_AtlasSprite.GetNameList()[ 0 ] ] ;
						}
					}
					Sprite = sprite ;
				}

				// インスタンスは保持しない(本来は private メソッドでやるべきではない)
				m_AtlasSprite = null ;
			}
		}

		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string spriteName, bool resize = false )
		{
			if( m_AtlasSprite == null )
			{
				return false ;	// 基本的にありえない
			}

			if( m_AtlasSprite.texture == null && string.IsNullOrEmpty( m_AtlasSprite.path ) == false )
			{
				m_AtlasSprite.Load() ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return false ;
			}

			Sprite = m_AtlasSprite[ spriteName ] ;

			if( resize == true )
			{
				SetNativeSize() ;
			}

			return true ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string path, string spriteName, bool resize = false )
		{
			if( m_AtlasSprite == null )
			{
				return false ;	// 基本的にありえない
			}

			if( m_AtlasSprite.texture == null && string.IsNullOrEmpty( path ) == false )
			{
				m_AtlasSprite.Load( path ) ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return false ;
			}

			Sprite = m_AtlasSprite[ spriteName ] ;

			if( resize == true )
			{
				SetNativeSize() ;
			}

			return true ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return null ;
			}

			return m_AtlasSprite[ spriteName ] ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ spriteName ].rect.width ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ spriteName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ spriteName ].rect.height ;
		}

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="tColor"></param>
		public void SetColor( uint color )
		{
			if( _image != null )
			{
				byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
				byte b = ( byte )( ( color       ) & 0xFF ) ;
				byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

				_image.color = new Color32( r, g, b, a ) ;
			}
		}

		/// <summary>
		/// 画像の向きを設定する
		/// </summary>
		public UIInversion.Direction Inversion
		{
			get
			{
				return _inversion != null ? _inversion.direction : UIInversion.Direction.None ;
			}
			set
			{
				if( value == UIInversion.Direction.None )
				{
					isInversion = false ;
				}
				else
				{
					if( _inversion == null )
					{
						isInversion = true ;
					}
					_inversion.direction = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return _image?.sprite ;
			}
			set
			{
				if( _image != null )
				{
					_image.sprite = value ;
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
				return _image != null ? _image.color : Color.white ;
			}
			set
			{
				if( _image != null )
				{
					_image.color = value ;
				}
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				return _image?.material ;
			}
			set
			{
				if( _image != null )
				{
					_image.material = value ;
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
				return _image != null ? _image.raycastTarget : false ;
			}
			set
			{
				if( _image != null )
				{
					_image.raycastTarget = value ;
				}
			}
		}
		
		/// <summary>
		/// タイプ(ショートカット)
		/// </summary>
		public Image.Type Type
		{
			get
			{
				return _image != null ? _image.type : Image.Type.Simple ;
			}
			set
			{
				if( _image != null )
				{
					_image.type = value ;
				}
			}
		}
	
		/// <summary>
		/// フィルセンター(ショートカット)
		/// </summary>
		public bool FillCenter
		{
			get
			{
				return _image != null ? _image.fillCenter : false ;
			}
			set
			{
				if( _image != null )
				{
					_image.fillCenter = value ;
				}
			}
		}

		/// <summary>
		/// フィルメソッド(ショートカット)
		/// </summary>
		public Image.FillMethod FillMethod
		{
			get
			{
				return _image != null ? _image.fillMethod : Image.FillMethod.Horizontal ;
			}
			set
			{
				if( _image != null )
				{
					_image.fillMethod = value ;
				}
			}
		}

		public enum FillOriginTypes
		{
			Bottom	= 0,
			Right	= 1,
			Top		= 2,
			Left	= 3,
		}

		/// <summary>
		/// フィルオリジン(ショートカット)
		/// </summary>
		public FillOriginTypes FillOriginType
		{
			get
			{
				return _image != null ? ( FillOriginTypes )_image.fillOrigin : FillOriginTypes.Bottom ;
			}
			set
			{
				if( _image != null )
				{
					_image.fillOrigin = ( int )value ;
				}
			}
		}

		/// <summary>
		/// フィルアマウント(ショートカット)
		/// </summary>
		public float FillAmount
		{
			get
			{
				return _image != null ? _image.fillAmount : 0 ;
			}
			set
			{
				if( _image != null )
				{
					_image.fillAmount = value ;
				}
			}
		}

		/// <summary>
		/// クロックワイズ(ショートカット)
		/// </summary>
		public bool FillClockwise
		{
			get
			{
				return _image != null ? _image.fillClockwise : false ;
			}
			set
			{
				if( _image != null )
				{
					_image.fillClockwise = value ;
				}
			}
		}

		/// <summary>
		/// プリサーブアスペクト(ショートカット)
		/// </summary>
		public bool PreserveAspect
		{
			get
			{
				return _image != null ? _image.preserveAspect : false ;
			}
			set
			{
				if( _image != null )
				{
					_image.preserveAspect = value ;
				}
			}
		}

		/// <summary>
		/// マスクのグラフィック表示の有無(ショートカット)
		/// </summary>
		public bool ShowMaskGraphic
		{
			get
			{
				return _mask != null ? _mask.showMaskGraphic : false ;
			}
			set
			{
				if( _mask != null )
				{
					_mask.showMaskGraphic = value ;
				}
			}
		}

		/// <summary>
		/// RectRransform のサイズをスプライトのサイズに合わせる
		/// </summary>
		public void SetNativeSize()
		{
			if( Sprite != null )
			{
				SetSize( Sprite.rect.width, Sprite.rect.height ) ;
			}
		}

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Image image = _image ;

			if( image == null )
			{
				image = gameObject.AddComponent<Image>() ;
			}
			if( image == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			if( option.ToLower() == "panel" )
			{
				// Panel
				image.color = new Color32( 255, 255, 255, 100 ) ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			
				SetAnchorToStretch() ;
//				SetSize( 0, 0 ) ;
			}
			else
			{
				// Default
//				tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
				image.color = Color.white ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			}

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			image.raycastTarget = false ;
		}

		/// <summary>
		/// リソースからスプライトをロードする
		/// </summary>
		/// <param name="tPath">リソースのパス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool LoadSpriteFromResources( string path )
		{
			Sprite sprite = Resources.Load<Sprite>( path ) ;
			if( sprite == null )
			{
				return false ;
			}

			Sprite = sprite ;

			return true ;
		}

		/// <summary>
		/// スプライトを設定しスプライトのサイズでリサイズする
		/// </summary>
		/// <param name="tSprite">スプライトのインスタンス</param>
		public void SetSpriteAndResize( Sprite sprite )
		{
			Sprite = sprite ;

			if( sprite != null )
			{
				Size = sprite.rect.size ;
			}
		}

		/// <summary>
		/// スプライトを設定し任意のサイズにリサイズする
		/// </summary>
		/// <param name="tSprite">スプライトのインスタンス</param>
		/// <param name="tSize">リサイズ後のサイズ</param>
		public void SetSpriteAndResize( Sprite sprite, Vector2 size )
		{
			Sprite = sprite ;
			Size   = size ;
		}
	}
}
