using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

//using UnityEngine.Serialization ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Image ) ) ]
	public class UIImage : UIView
	{
		/// <summary>
		/// 動作有無のショートカット
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CImage != null && CImage.enabled ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.enabled = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteAtlas 限定
		
		// スプライトアトラス(Unity標準機能)
		[SerializeField][HideInInspector]
		private SpriteAtlas m_SpriteAtlas = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  SpriteAtlas  SpriteAtlas
		{
			get
			{
				return m_SpriteAtlas ;
			}
			set
			{
				if( m_SpriteAtlas != value )
				{
					// アトラス内スプライトのキャッシュをクリアする
					CleanupAtlasSprites() ;

					m_SpriteAtlas  = value ;
				}
			}
		}

		// SpriteAtlas から取得した Sprite は Destroy() が必要であるためキャッシュする
		private Dictionary<string,Sprite> m_SpritesInAtlas ;

		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定

//		[FormerlySerializedAs("m_AtlasSprite")]
		[SerializeField]
		private SpriteSet m_SpriteSet = null ;

		/// <summary>
		/// スプライトセットのインスタンス
		/// </summary>
		public  SpriteSet  SpriteSet
		{
			get
			{
				return m_SpriteSet ;
			}
			set
			{
				if( m_SpriteSet != value )
				{
					m_SpriteSet  = value ;
				}
			}
		}

		/// <summary>
		/// アトラススプライトの要素となるスプライト群を設定する
		/// </summary>
		/// <param name="sprites"></param>
		/// <returns></returns>
		public bool SetSprites( Sprite[] sprites )
		{
			if( sprites == null || sprites.Length == 0 )
			{
				return false ;
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet == null )
			{
				m_SpriteSet = new SpriteSet() ;
			}
			else
			{
				m_SpriteSet.ClearSprites() ;
			}

			m_SpriteSet.SetSprites( sprites ) ;

			//----------------------------------------------------------

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// アトラス内のスプライトをキャッシュにためつつ取得する
		private Sprite GetSpriteInAtlasFromCache( string spriteName )
		{
			Sprite sprite ;

			if( m_SpritesInAtlas != null )
			{
				if( m_SpritesInAtlas.Count >  0 )
				{
					if( m_SpritesInAtlas.ContainsKey( spriteName ) == true )
					{
						// 既にキャッシュに存在する
						return m_SpritesInAtlas[ spriteName ] ;
					}
				}
			}

			//----------------------------------

			// 実際のアトラスに存在するか確認する
			sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
			if( sprite != null )
			{
				// GetSprite()で取得したSpriteオブジェクトの名前は「"～(Clone)"」のように
				// なっているため、「"(Clone)"」が付かない名前に上書き
				sprite.name = spriteName ;

				// キャッシュを生成する
				m_SpritesInAtlas ??= new Dictionary<string, Sprite>() ;

				// 存在するのでキャッシュに貯める
				m_SpritesInAtlas.Add( spriteName, sprite ) ;
			}

			return sprite ;
		}
		
		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="resize">画像のサイズに合わせてリサイズを行うかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string spriteName, bool resize = false )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					Sprite = sprite ;

					if( resize == true )
					{
						SetNativeSize() ;
					}

					return true ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					Sprite = sprite ;

					if( resize == true )
					{
						SetNativeSize() ;
					}

					return true ;
				}
			}

			//----------------------------------------------------------

			return false ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------

			return null ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color       ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			Color = new Color32( r, g, b, a ) ;
		}

		/// <summary>
		/// 画像の向きを設定する
		/// </summary>
		public UIInversion.DirectionTypes Inversion
		{
			get
			{
				return CInversion != null ? CInversion.DirectionType : UIInversion.DirectionTypes.None ;
			}
			set
			{
				if( value == UIInversion.DirectionTypes.None )
				{
					IsInversion = false ;
				}
				else
				{
					if( CInversion == null )
					{
						IsInversion = true ;
					}
					CInversion.DirectionType = value ;
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
				return CImage != null ? CImage.sprite : null ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.sprite = value ;
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
				return CImage != null ? CImage.color : Color.white ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.color = value ;
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
				return CImage != null ? CImage.material : null ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.material = value ;
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public override bool RaycastTarget
		{
			get
			{
				return CImage != null && CImage.raycastTarget ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.raycastTarget = value ;
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
				return CImage != null ? CImage.type : Image.Type.Simple ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.type = value ;
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
				return CImage != null && CImage.fillCenter ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillCenter = value ;
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
				return CImage != null ? CImage.fillMethod : Image.FillMethod.Horizontal ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillMethod = value ;
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
				return CImage != null ? ( FillOriginTypes )CImage.fillOrigin : FillOriginTypes.Bottom ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillOrigin = ( int )value ;
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
				return CImage != null ? CImage.fillAmount : 0 ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillAmount = value ;
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
				return CImage != null && CImage.fillClockwise ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.fillClockwise = value ;
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
				return CImage != null && CImage.preserveAspect ;
			}
			set
			{
				if( CImage != null )
				{
					CImage.preserveAspect = value ;
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
				return CMask != null && CMask.showMaskGraphic ;
			}
			set
			{
				if( CMask != null )
				{
					CMask.showMaskGraphic = value ;
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

		/// <summary>
		/// シャドーカラーを設定する
		/// </summary>
		public Color ShadowColor
		{
			get
			{
				return CShadow != null ? CShadow.effectColor : Color.white ; 
			}
			set
			{
				if( CShadow == null )
				{
					return ;
				}
				CShadow.effectColor = value ;
			}
		}

		/// <summary>
		/// アウトラインカラーを設定する
		/// </summary>
		public Color OutlineColor
		{
			get
			{
				return COutline != null ? COutline.effectColor : Color.white ; 
			}
			set
			{
				if( COutline == null )
				{
					return ;
				}
				COutline.effectColor = value ;
			}
		}
		
		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		protected override void OnBuild( string option = "" )
		{
			Image image = CImage != null ? CImage : gameObject.AddComponent<Image>() ;
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
//				image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
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
		/// <param name="path">リソースのパス</param>
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
		/// スプライトを設定する
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		public void SetSprite( Sprite sprite, bool resize = false )
		{
			if( resize == false )
			{
				Sprite = sprite ;
			}
			else
			{
				SetSpriteAndResize( sprite ) ;
			}
		}

		/// <summary>
		/// スプライトを設定しスプライトのサイズでリサイズする
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
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
		/// <param name="sprite">スプライトのインスタンス</param>
		/// <param name="size">リサイズ後のサイズ</param>
		public void SetSpriteAndResize( Sprite sprite, Vector2 size )
		{
			Sprite = sprite ;
			Size   = size ;
		}

		//-----------------------------------------------------------

		protected override void OnEnable()
		{
			base.OnEnable() ;

			//----------------------------------

			// アクティブになった際に１回は必ず更新する
			m_RefreshChildrenColor = true ;

			m_PreviousColor.r = 0 ;
			m_PreviousColor.g = 0 ;
			m_PreviousColor.b = 0 ;
			m_PreviousColor.a = 0 ;
		}

		protected override void OnDisable()
		{
			base.OnDisable() ;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy() ;

			// アトラス内スプライトのキャッシュをクリアする
			CleanupAtlasSprites() ;
		}

		// キャッシュされたアトラス内スプライト群を破棄する
		private void CleanupAtlasSprites()
		{
			if( m_SpritesInAtlas != null )
			{
				if( m_SpritesInAtlas.Count >  0 )
				{
					if( Application.isPlaying == true )
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								Destroy( sprite.Value ) ;
							}
						}
					}
					else
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								DestroyImmediate( sprite.Value ) ;
							}
						}
					}

					m_SpritesInAtlas.Clear() ;
				}

				m_SpritesInAtlas = null ;
			}
		}
	}
}
