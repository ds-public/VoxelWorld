using System ;

using UnityEngine ;
using UnityEngine.UI ;

using UnityEngine.Experimental.Rendering ;
using UnityEngine.Rendering ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:RawImage クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.RawImage ) ) ]
	public class UIRawImage : UIView
	{
		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture Texture
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return null ;
				}
				return rawImage.texture ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return Color.white ;
				}
				return rawImage.color ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return null ;
				}
				return rawImage.material ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.material = value ;
			}
		}
		
		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		override public bool RaycastTarget
		{
			get
			{
				return CRawImage != null ? CRawImage.raycastTarget : false ;
			}
			set
			{
				if( CRawImage != null )
				{
					CRawImage.raycastTarget = value ;
				}
			}
		}

		/// <summary>
		/// ＵＶ座標(ショートカット)
		/// </summary>
		public Rect UVRect
		{
			get
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return new Rect( 0, 0, 1, 1 ) ;
				}
				return rawImage.uvRect ;
			}
			set
			{
				RawImage rawImage = CRawImage ;
				if( rawImage == null )
				{
					return ;
				}
				rawImage.uvRect = value ;
			}
		}
	
		/// <summary>
		/// RectRransform のサイズをスプライトのサイズに合わせる
		/// </summary>
		public void SetNativeSize()
		{
			if( Texture == null )
			{
				return ;
			}

			SetSize( Texture.width, Texture.height ) ;
		}

		//----------------------------------------------------------------------------------

		/// <summary>
		/// ブラー効果の強度
		/// </summary>
		public enum BlurIntensities
		{
			/// <summary>
			/// 無し
			/// </summary>
			None,

			/// <summary>
			/// 弱い
			/// </summary>
			Soft,

			/// <summary>
			/// 標準
			/// </summary>
			Normal,

			/// <summary>
			/// 強い
			/// </summary>
			Hard,
		}


		/// <summary>
		/// 全画面のブラーエフェクトとして使用するかどうかとその強さ
		/// </summary>
		[SerializeField]
		protected BlurIntensities m_BlurIntensity = BlurIntensities.None ;

		/// <summary>
		/// 全画面のブラーエフェクトとして使用するかどうかとその強さ
		/// </summary>
		public BlurIntensities BlurIntensity
		{
			get
			{
				return m_BlurIntensity ;
			}
			set
			{
				if( m_BlurIntensity != value )
				{
					m_BlurIntensity  = value ;

					if( m_BlurIntensity == BlurIntensities.None )
					{
						DisposeBlur() ;
					}
					else
					{
						DeleteDrawableTexture() ;
					}
				}
			}
		}

		//-----------------------------------

		/// <summary>
		/// 自動的に描画可能なテクスチャを生成するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AutoCreateDrawableTexture = false ;

		/// <summary>
		/// 自動的に描画可能なテクスチャを生成するかどうか
		/// </summary>
		public bool AutoCreateDrawableTexture{ get{ return m_AutoCreateDrawableTexture ; } set{ m_AutoCreateDrawableTexture = value ; } }

		//-------------------------------------------------------------------------------------------

		// 縦方向の座標を反転するかどうか
		[SerializeField][HideInInspector]
		private bool m_FlipVertical = false ;

		/// <summary>
		/// 縦方向の座標を反転するかどうか
		/// </summary>
		public bool IsFlipVertical
		{
			get
			{
				return m_FlipVertical ;
			}
			set
			{
				m_FlipVertical = value ;
			}
		}

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			var rawImage = CRawImage ;

			if( rawImage == null )
			{
				rawImage = gameObject.AddComponent<RawImage>() ;
			}
			if( rawImage == null )
			{
				// 異常
				return ;
			}

			rawImage.color = Color.white ;
			rawImage.raycastTarget = false ;

			if( IsCanvasOverlay == true )
			{
				rawImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
		}

		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( m_AutoCreateDrawableTexture == true )
			{
				CreateDrawableTexture() ;
			}
		}

		// GameObject が有効になる際に呼び出される
		protected override void OnEnable()
		{
			base.OnEnable() ;

			//----------------------------------

			if( m_BlurIntensity != BlurIntensities.None )
			{
				AddCallback() ;
			}
		}

		// GameObject が無効になる際に呼び出される
		protected override void OnDisable()
		{
			// ブラー関連処理の後始末を行う
			DisposeBlur() ;

			//----------------------------------

			base.OnDisable() ;
		}

		// GameObject が破棄される際に呼び出される
		protected override void OnDestroy()
		{
			// ブラー関連処理の後始末を行う
			DisposeBlur() ;

			//----------------------------------

			// 描画可能テクスチャが生成されていれば破棄する
			DeleteDrawableTexture() ;

			//----------------------------------

			base.OnDestroy() ;
		}

		//-------------------------------------------------------------------
		// 描画可能テクスチャの情報

		private int m_Width  = 0 ;
		private int m_Height = 0 ;
		
		private Color32[] m_Pixels = null ;

		private Texture2D m_DrawableTexture = null ;

		//-----------------------------------------------------------

		/// <summary>
		/// 描画可能なテクスチャを生成して割り当てる
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public bool CreateDrawableTexture( int width = 0, int height = 0 )
		{
			if( CRawImage == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			DeleteDrawableTexture() ;

			// 描画可能テクスチャを生成して割り当てる
			if( width == 0 )
			{
				width = ( int )this.Width ;
			}
			if( height == 0 )
			{
				height = ( int )this.Height ;
			}

			m_Width  = width ;
			m_Height = height ;

			m_Pixels = new Color32[ m_Width * m_Height ] ;

			m_DrawableTexture = new Texture2D( m_Width, m_Height, TextureFormat.ARGB32, false ) ;

			m_DrawableTexture.SetPixels32( m_Pixels ) ;
			m_DrawableTexture.Apply() ;

			CRawImage.texture = m_DrawableTexture ;

			return true ;
		}

		/// <summary>
		/// 描画可能なテクスチャを破棄する
		/// </summary>
		public void DeleteDrawableTexture()
		{
			if( m_DrawableTexture != null )
			{
				if( Texture == m_DrawableTexture )
				{
					Texture  = null ;
				}

				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_DrawableTexture ) ;
				}
				else
				{
					Destroy( m_DrawableTexture ) ;
				}
				m_DrawableTexture = null ;
			}

			m_Pixels = null ;

			m_Width  = 0 ;
			m_Height = 0 ;
		}

		//-----------------------------------------------------------
		// 各種描画メソッド群

		/// <summary>
		/// 直線を描画する
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="color"></param>
		/// <param name="pixels"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public bool DrawLine( int x0, int y0, int x1, int y1, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			int dx ;
			int ax ;

			if( x1 >  x0 )
			{
				dx = x1 - x0 + 1 ;
				ax =  1 ;
			}
			else
			if( x0 >  x1 )
			{
				dx = x0 - x1 + 1 ;
				ax = -1 ;
			}
			else
			{
				dx = 1  ;
				ax = 0  ;
			}

			int dy ;
			int ay ;

			if( y1 >  y0 )
			{
				dy = y1 - y0 + 1 ;
				ay =  1 ;
			}
			else
			if( y0 >  y1 )
			{
				dy = y0 - y1 + 1 ;
				ay = -1 ;
			}
			else
			{
				dy = 1  ;
				ay = 0  ;
			}

			int px = x0 ;
			int py = y0 ;

			int cx = 0 ;
			int cy = 0 ;

			int x, y ;
			int tx, ty ;

			Color32 c = GetColor( color ) ;

			if( dx >= dy )
			{
				for( x  = 0 ; x <  dx ; x ++ )
				{
//					DrawPixel( px, py, color ) ;
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}
					
					px += ax ;

					cy += dy ;
					if( cy >= dx )
					{
						cy -= dx ;
						py += ay ;
					}
				}
			}
			else
			{
				for( y  = 0 ; y <  dy ; y ++ )
				{
//					DrawPixel( px, py, color ) ;
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}

					py += ay ;

					cx += dx ;
					if( cx >= dy )
					{
						cx -= dy ;
						px += ax ;
					}
				}
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}

		/// <summary>
		/// 点を描画する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public bool DrawPixel( int x, int y, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			if( x >= 0 && x <  m_Width && y >= 0 && y <  m_Height )
			{
				if( m_FlipVertical == true )
				{
					y = m_Height - 1 - y ;
				}

				m_Pixels[ y * m_Width + x ] = GetColor( color ) ;
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}
		
		/// <summary>
		/// 塗りつぶされた矩形を描画する
		/// </summary>
		/// <param name="tColor"></param>
		/// <param name="tUpdate"></param>
		/// <returns></returns>
		public bool FillRectangle( uint color, bool isUpdate = true )
		{
			return FillRectangle( 0, 0, m_Width, m_Height, color, isUpdate ) ;
		}

		/// <summary>
		/// 塗りつぶされた矩形を描画する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public bool FillRectangle( int x, int y, int w, int h, uint color, bool isUpdate = true )
		{
			if( m_Pixels == null )
			{
				return false ;
			}

			//----------------------------------

			Color32 c = GetColor( color ) ;

			int lx, ly ;
			int px, py ;
			int tx, ty ;

			py = y ;
			for( ly  = 0 ; ly <  h ; ly ++ )
			{
				px = x ;
				for( lx  = 0 ; lx <  w ; lx ++ )
				{
					if( px >= 0 && px <  m_Width && py >= 0 && py <  m_Height )
					{
						tx = px ;
						ty = py ;
						if( m_FlipVertical == true )
						{
							ty = m_Height - 1 - ty ;
						}

						m_Pixels[ ty * m_Width + tx ] = c ;
					}
					px ++ ;
				}
				py ++ ;
			}

			if( isUpdate == true )
			{
				UpdateDrawableTexture() ;
			}

			return true ;
		}


		private Color32 GetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color >>  0 ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			return new Color32( r, g, b, a ) ;
		}

		// 描画可能テクスチャを更新する
		public bool UpdateDrawableTexture()
		{
			if( m_DrawableTexture == null )
			{
				return false ;
			}

			m_DrawableTexture.SetPixels32( m_Pixels ) ;
			m_DrawableTexture.Apply() ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// ブラーエフェクト関連

		// マテリアルのパス
		private const string m_FlipVerticalMaterialPath = "uGUIHelper/Materials/FlipVertical" ;

		/// <summary>
		/// ボックスフィルタリングをするためのマテリアル
		/// </summary>
		private static Material m_FlipVerticalMaterial ;

		/// <summary>
		/// ボックスフィルタリングをするためのマテリアル
		/// </summary>
		private static Material FlipVerticalMaterial
		{
			get
			{
				if( m_FlipVerticalMaterial == null )
				{
					// 無ければロードする
					m_FlipVerticalMaterial = ( Material )Resources.Load
					(
						m_FlipVerticalMaterialPath,
						typeof( Material )
					) ;
				}

				return m_FlipVerticalMaterial ;
			}
		}

		//---------------

		// マテリアルのパス
		private const string m_BlurFilterMaterialPath = "uGUIHelper/Materials/BlurFilter" ;

		/// <summary>
		/// ボックスフィルタリングをするためのマテリアル
		/// </summary>
		private static Material m_BlurFilterMaterial ;

		/// <summary>
		/// ボックスフィルタリングをするためのマテリアル
		/// </summary>
		private static Material BlurFilterMaterial
		{
			get
			{
				if( m_BlurFilterMaterial == null )
				{
					// 無ければロードする
					m_BlurFilterMaterial = ( Material )Resources.Load
					(
						m_BlurFilterMaterialPath,
						typeof( Material )
					) ;
				}

				return m_BlurFilterMaterial ;
			}
		}

		//-----------------------------------------------------------

		// ブラーエフェクトを描画するレンダーテクスチャ
		private RenderTexture m_BlurKeptTexture ;

		// Enable になってから OnWillRenderCanvases() が呼ばれた回数
		private int m_CallCount ;

		// RectTransform の操作禁止化
		private DrivenRectTransformTracker m_RectTransformTracker ;

		private Action<bool> m_OnBlurProcessing ;

		/// <summary>
		/// ブラー処理時のコールバックを登録する
		/// </summary>
		/// <param name="onBlurProcessing"></param>
		public void SetOnBlurProcessing( Action<bool> onBlurProcessing )
		{
			m_OnBlurProcessing = onBlurProcessing ;
		}

		//-----------------------------------------------------------

		// コールバックを登録する
		private void AddCallback()
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases ;
			Canvas.willRenderCanvases += OnWillRenderCanvases ;

			CRawImage.enabled = false ;

			m_CallCount = 0 ;
		}

		// コールバックを解除する
		private void RemoveCallback()
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases ;
		}

		//-----------------------------------

		// キャンバスへの描画が行われる前に呼び出される
		private void OnWillRenderCanvases()
		{
			m_CallCount ++ ;
			if( m_CallCount == 1 )
			{
				// 最初の呼び出しは無視する

				// ブラー開始時のコールバックを呼ぶ
				m_OnBlurProcessing?.Invoke( true ) ;

				return ;
			}

			//----------------------------------

			ProcessBlur() ;

			RemoveCallback() ;

			//----------------------------------

			// ブラー開始時のコールバックを呼ぶ
			m_OnBlurProcessing?.Invoke( false ) ;
		}

		// ブラー処理を行う
		private void ProcessBlur()
		{
			//----------------------------------------------------------

			// 表示テクスチャをクリアする
			CRawImage.enabled = false ;
			Texture = null ;

			//----------------------------------------------------------
			// ブラーの強度から反復処理の回数を決定する

			int		division = 2 ;

			int		level = 1 ;
			float	delta = 1 ;

			switch( m_BlurIntensity )
			{
				case BlurIntensities.Soft	: level = 0 ; delta = 1.0f ; break ;
				case BlurIntensities.Normal	: level = 1 ; delta = 1.0f ; break ;
				case BlurIntensities.Hard	: level = 2 ; delta = 1.0f ; break ;
			}

			//------------------------------------------------------------------------------------------

			// 画面サイズ
			int screenWidth  = Screen.width ;
			int screenHeight = Screen.height ;

			//--------------


			// 表示するレンダーテクスチャのサイズを決定する
			int width  = screenWidth  / division ;
			int height = screenHeight / division ; 

			// レンダーテクスチャが存在済みで且つサイズが変わっていたら破棄する
			if
			(
				( m_BlurKeptTexture != null ) &&
				(
					( m_BlurKeptTexture.width  != width  ) ||
					( m_BlurKeptTexture.height != height )
				)
			)
			{
				DeleteBlurKeptTexture() ;
			}

			//--------------

			// レンダーテクスチャを生成する
			CreateBlurKeptTexture( width, height ) ;

			//------------------------------------------------------------------------------------------
			// コマンドバッファを生成する

			// コマンドの作成
			var commandBuffer = new CommandBuffer() ;

			//----------------------------------

			// キャプチャ用のコマンドを取得する
			int captureNameId = Shader.PropertyToID( "_Capture" ) ;
			var captureIdentifier = new RenderTargetIdentifier( captureNameId ) ;

			// 画面キャプチャを描画するレンダーテクスチャを作成するコマンドを格納する
			commandBuffer.GetTemporaryRT
			(
				captureNameId,
				screenWidth,
				screenHeight,
				0,
				FilterMode.Point,
				SystemInfo.GetGraphicsFormat( DefaultFormat.LDR ),
				1,
				false,
				RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
				false
			) ;

			//--------------

			// 画面キャプチャをレンダーテクスチャに描画するコマンドを格納する

			RenderTargetIdentifier originRenderTarget  = default ;

#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				// 既に作成済みのレンダーテクスチャ
				RenderTexture gameViewRT     = null ;

				// 全ての生成中のレンダーテクスチャを取得する
				var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>() ;
				foreach( var renderTexture in renderTextures )
				{
					// ※ GameView RT という名前は固定
					if( renderTexture.name == "GameView RT" )
					{
						// Game タブのウィンドウに対応するキャプチャ用のレンダーテクスチャは生成済みになっている
						gameViewRT = renderTexture ;
						break ;
					}
				}

				if( gameViewRT == null )
				{
					// GameView の RenderTexture が取得できなかった
					Debug.LogWarning( $"[uGUIHelper] GameView RT is null.\nGameObject : {Path}" ) ;
					captureIdentifier = 0 ;
					return ;
				}

				originRenderTarget = gameViewRT ;
			}
			else
#endif
			{
				// 描画対象になっているテクスチャ(フレームバッファ)
				originRenderTarget = BuiltinRenderTextureType.BindableTexture ;
			}

			//----

			// 画面キャプチャを一時的に生成したレンダーテクスチャに描画するコマンドを格納する
			commandBuffer.Blit
			(
				originRenderTarget,
				captureIdentifier
			) ;

			// キャプチャまでのコマンドが格納された

			//----------------------------------------------------------

			// ２つの作業用レンダーテクスチャを生成する

			int i, l = 2 ;

			var identifiers				= new RenderTextureIdentifier[ l ] ;

			for( i = 0 ; i <  l ; i ++ )
			{
				identifiers[ i ] = new RenderTextureIdentifier( $"_BlurRT_{i}" ) ; ;

				// コマンドバッファにレンダーテクスチャ生成を追加する
				commandBuffer.GetTemporaryRT
				(
					identifiers[ i ].NameId,	// レンダーテクスチャの識別子
					width,
					height,
					0,
					FilterMode.Bilinear,
					SystemInfo.GetGraphicsFormat( DefaultFormat.LDR ),
					1,
					false,
					RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
					false
				) ;
			}

			//--------------

			// キャプチャー画像を作業用のレンダーテクスチャの１枚目にフィルタ付きで描画する

			// シェーダーに渡す値を設定する
			commandBuffer.SetGlobalFloat
			(
				"_SamplingDelta",
				delta
			) ;

			// レンダリングを実行する
			commandBuffer.Blit
			(
				captureIdentifier,					// 描画元のレンダーテクスチャの識別子(最初は画面キャプチャーしたレンダーテクスチャ)
				identifiers[ 0 ].Identifier,		// 描画先のレンダーテクスチャの識別子
				FlipVerticalMaterial,				// マテリアル(シェーダー)
				0									// シェーダーのパス
			) ;

			//----------------------------------------------------------
			// 作業用のレンダーテクスチャ間で交互に描画する

			for( i  = 0 ; i <  level ; i ++ )
			{
				// シェーダーに渡す値を設定する
				commandBuffer.SetGlobalFloat
				(
					"_SamplingDelta",
					delta
				) ;

				// レンダリングを実行する(大きい→小さい)
				commandBuffer.Blit
				(
					identifiers[ 0 ].Identifier,		// 描画元のレンダーテクスチャの識別子(最初は画面キャプチャーしたレンダーテクスチャ)
					identifiers[ 1 ].Identifier,		// 描画先のレンダーテクスチャの識別子
					BlurFilterMaterial,						// マテリアル(シェーダー)
					0									// シェーダーのパス
				) ;

				// シェーダーに渡す値を設定する
				commandBuffer.SetGlobalFloat
				(
					"_SamplingDelta",
					delta
				) ;

				// レンダリングを実行する(大きい→小さい)
				commandBuffer.Blit
				(
					identifiers[ 1 ].Identifier,		// 描画元のレンダーテクスチャの識別子(最初は画面キャプチャーしたレンダーテクスチャ)
					identifiers[ 0 ].Identifier,		// 描画先のレンダーテクスチャの識別子
					BlurFilterMaterial,						// マテリアル(シェーダー)
					0									// シェーダーのパス
				) ;
			}

			//----------------------------------
			// 最後に表示用のレンダーテクスチャに描画する

			// シェーダーに渡す値を設定する
			commandBuffer.SetGlobalFloat
			(
				"_SamplingDelta",
				delta
			) ;

			// レンダリングを実行する(最も大きい→画面キャプチャが描画されたテクスチャ)
			commandBuffer.Blit
			(
				identifiers[ 0 ].Identifier,
				m_BlurKeptTexture,
				BlurFilterMaterial,			// マテリアル(シェーダー)
				0							// シェーダーのパス
			) ;

			//----------------------------------
			// 作業用のレンダーテクスチャを破棄する

			for( i  = ( l - 1 ) ; i >= 0 ; i -- )
			{
				// 一時テクスチャを解放する(小さい方から)
				commandBuffer.ReleaseTemporaryRT( identifiers[ i ].NameId ) ;
			}

			//------------------------------------------------------------------------------------------

			// 最後に画面キャプチャ用のレンダーテクスチャを破棄するコマンドを格納する

			// 一時的に生成した画面キャプチャ用のレンダーテクスチャを破棄する
			commandBuffer.ReleaseTemporaryRT( captureNameId ) ;

			// 退避しておいた元のレンダーターゲットを設定する
			commandBuffer.SetRenderTarget( originRenderTarget ) ;

			//------------------------------------------------------------------------------------------
			// コマンドバッファの実行

			// コマンドバッファを実行する
			Graphics.ExecuteCommandBuffer( commandBuffer ) ;

			// コマンドバッファを破棄する
			commandBuffer.Release() ;

			//----------------------------------

			// テクスチャを設定する
			Texture = m_BlurKeptTexture ;

			// 表示を行う
			CRawImage.enabled = true ;

			//----------------------------------

			// RectTransform を編集不可にしてルートキャンバスに追従させる
			FitToScreen() ;

			//----------------------------------------------------------
		}

		// RectTransform を編集不可にしてルートキャンバスに追従させる
		private void FitToScreen()
		{
			var canvas = CRawImage.canvas ;
			if( canvas == null )
			{
				return ;
			}

			var rootCanvas		= canvas.rootCanvas ;
			var rootTransform	= ( RectTransform )rootCanvas.transform ;
			var size			= rootTransform.rect.size ;

			var rectTransform = CRawImage.rectTransform ;

			//----------------------------------

			m_RectTransformTracker.Clear() ;

			m_RectTransformTracker.Add
			(
				this,
				rectTransform,
				DrivenTransformProperties.SizeDelta
			) ;

			rectTransform.SetSizeWithCurrentAnchors
			(
				RectTransform.Axis.Horizontal,
				size.x
			) ;

			rectTransform.SetSizeWithCurrentAnchors
			(
				RectTransform.Axis.Vertical,
				size.y
			) ;

			m_RectTransformTracker.Add
			(
				this,
				rectTransform,
				DrivenTransformProperties.AnchoredPosition3D
			) ;

			rectTransform.position = rootTransform.position ;
		}

		// ブラー関連処理の後始末を行う
		private void DisposeBlur()
		{
			RemoveCallback() ;

			DeleteBlurKeptTexture() ;

			m_RectTransformTracker.Clear() ;
		}

		//-------------------------------------------------------------------------------------------

		// レンダーテクスチャを生成する
		private void CreateBlurKeptTexture( int width, int height )
		{
			if( m_BlurKeptTexture == null )
			{
				// レンダーテクスチャを生成する
				m_BlurKeptTexture = new RenderTexture
				(
					width,
					height,
					0,
					RenderTextureFormat.ARGB32,
					RenderTextureReadWrite.Default
				)
				{
					name				= $"BlurTexture_{GetInstanceID()}",
					enableRandomWrite	= false,
					memorylessMode		= RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA,
					filterMode			= FilterMode.Bilinear,
					useMipMap			= false,
					autoGenerateMips	= false,
					useDynamicScale		= false,
					antiAliasing		= 1,
					anisoLevel			= 1,
					depth				= 0,
					wrapMode			= TextureWrapMode.Clamp,
					vrUsage				= VRTextureUsage.OneEye,
					hideFlags			= HideFlags.DontSave | HideFlags.NotEditable
				} ;
				m_BlurKeptTexture.Create() ;
			}
		}

		// レンダーテクスチャを破棄する
		private void DeleteBlurKeptTexture()
		{
			if( m_BlurKeptTexture != null )
			{
				if( Texture == m_BlurKeptTexture )
				{
					Texture  = null ;
				}

				//-------------

				if( m_BlurKeptTexture.IsCreated() == true )
				{
					m_BlurKeptTexture.Release() ;
				}

#if UNITY_EDITOR
				if( Application.IsPlaying( m_BlurKeptTexture ) == false )
				{
					DestroyImmediate
					(
						m_BlurKeptTexture,
						true
					) ;
				}
				else
#endif
				{
					Destroy( m_BlurKeptTexture ) ;
				}

				m_BlurKeptTexture = null ;
			}
		}

		//-----------------------------------------------------------

		public readonly struct RenderTextureIdentifier
		{
			public readonly int                    NameId ;
			public readonly RenderTargetIdentifier Identifier ;

			public RenderTextureIdentifier( int nameId )
			{
				NameId     = nameId ;
				Identifier = new RenderTargetIdentifier( nameId ) ;
			}

			public RenderTextureIdentifier( string name ) : this( Shader.PropertyToID( name ) )
			{
			}
		}
	}
}

