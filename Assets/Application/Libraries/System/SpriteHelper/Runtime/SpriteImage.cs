using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2024/02/25
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( SpriteRenderer ) )]
	public partial class SpriteImage : SpriteBasis
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteImage", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteImage" )]					// ポップアップメニューから
		public static void CreateSpriteImage()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child SpriteImage" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteImage" ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteImage>() ;
			component.SetDefault( true ) ;	// 初期状態に設定する

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public void SetDefault( bool useSample = false )
		{
			var spriteRenderer = CSpriteRenderer ;
			if( spriteRenderer == null )
			{
				return ; 
			}

			//----------------------------------

			if( useSample == false )
			{
				// サンプルは設定しない
				return ;
			}

			var sprites = Resources.LoadAll<Sprite>( "SpriteHelper/Textures/Sample" ) ;
			if( sprites != null && sprites.Length >  0 )
			{
				spriteRenderer.sprite = sprites[ 0 ] ;

				SetSprites( sprites ) ;
			}
		}


		//-------------------------------------------------------------------------------------------

		// インスタンスキャッシュ
		protected SpriteRenderer	m_SpriteRenderer ;

		/// <summary>
		/// キャッシュされた SpriteRenderer を取得する
		/// </summary>
		public	SpriteRenderer	CSpriteRenderer
		{
			get
			{
				if( m_SpriteRenderer == null )
				{
					TryGetComponent<SpriteRenderer>( out m_SpriteRenderer ) ;
				}
				return m_SpriteRenderer ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動作有無のショートカット
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.enabled ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.enabled = value ;
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

		[SerializeField][HideInInspector]
		private MultiModeSprite m_SpriteSet = null ;

		/// <summary>
		/// スプライトセットのインスタンス
		/// </summary>
		public  MultiModeSprite  SpriteSet
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
				m_SpriteSet = new MultiModeSprite() ;
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

//					if( resize == true )
//					{
//						SetNativeSize() ;
//					}

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

//					if( resize == true )
//					{
//						SetNativeSize() ;
//					}

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

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.sprite : null ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.sprite = value ;
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
				return CSpriteRenderer != null ? CSpriteRenderer.color : Color.white ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.color = value ;
				}
			}
		}

		/// <summary>
		/// 透過度
		/// </summary>
		public float Alpha
		{
			get
			{
				return CSpriteRenderer != null ? 0 : CSpriteRenderer.color.a ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					var color = CSpriteRenderer.color ;
					color.a = value ;
					CSpriteRenderer.color = color ;
				}

			}
		}

		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color MaterialColor
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return Color.white ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						return CSpriteRenderer.sharedMaterial == null ? Color.white : CSpriteRenderer.sharedMaterial.color ;
					}
					else
					{
						return CSpriteRenderer.material == null ? Color.white : CSpriteRenderer.material.color ;
					}
				}
				else
				{
					// 複製あり

					return m_DuplicatedMaterial.color ;
				}
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						if( CSpriteRenderer.sharedMaterial != null )
						{
							m_DuplicatedMaterial = Instantiate( CSpriteRenderer.sharedMaterial ) ;
							CSpriteRenderer.sharedMaterial = m_DuplicatedMaterial ;
						}
					}
					else
					{
						if( CSpriteRenderer.material != null )
						{
							m_DuplicatedMaterial = Instantiate( CSpriteRenderer.material ) ;
							CSpriteRenderer.material = m_DuplicatedMaterial ;
						}
					}

					if( m_DuplicatedMaterial != null )
					{
						m_DuplicatedMaterial.color = value ;
					}
				}
				else
				{
					// 複製あり

					m_DuplicatedMaterial.color = value ;
				}
			}
		}

		/// <summary>
		/// 透過度
		/// </summary>
		public float MaterialAlpha
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 1 ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						return CSpriteRenderer.sharedMaterial == null ? 1 : CSpriteRenderer.sharedMaterial.color.a ;
					}
					else
					{
						return CSpriteRenderer.material == null ? 1 : CSpriteRenderer.material.color.a ;
					}
				}
				else
				{
					// 複製あり

					return m_DuplicatedMaterial.color.a ;
				}
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						if( CSpriteRenderer.sharedMaterial != null )
						{
							m_DuplicatedMaterial = Instantiate( CSpriteRenderer.sharedMaterial ) ;
							CSpriteRenderer.sharedMaterial = m_DuplicatedMaterial ;
						}
					}
					else
					{
						if( CSpriteRenderer.material != null )
						{
							m_DuplicatedMaterial = Instantiate( CSpriteRenderer.material ) ;
							CSpriteRenderer.material = m_DuplicatedMaterial ;
						}
					}

					if( m_DuplicatedMaterial != null )
					{
						var color = m_DuplicatedMaterial.color ;
						color.a = value ;
						m_DuplicatedMaterial.color = color ;
					}
				}
				else
				{
					// 複製あり

					var color = m_DuplicatedMaterial.color ;
					color.a = value ;
					m_DuplicatedMaterial.color = color ;
				}
			}
		}

		/// <summary>
		/// 左右反転
		/// </summary>
		public bool FlipX
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.flipX ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.flipX = value ;
				}
			}
		}

		/// <summary>
		/// 上下反転
		/// </summary>
		public bool FlipY
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.flipY ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.flipY = value ;
				}
			}
		}

		/// <summary>
		/// 描画モード
		/// </summary>
		public SpriteDrawMode DrawMode
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.drawMode : SpriteDrawMode.Simple ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.drawMode = value ;
				}
			}
		}

		/// <summary>
		/// 横幅
		/// </summary>
		public float BoundsWidth
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.bounds.size.x : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					var bounds = CSpriteRenderer.bounds ;

					bounds.size = new Vector2( value,  bounds.size.y ) ;

					CSpriteRenderer.bounds = bounds ;
				}
			}
		}

		/// <summary>
		/// 縦幅
		/// </summary>
		public float BoundsHeight
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.bounds.size.y : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					var bounds = CSpriteRenderer.bounds ;

					bounds.size = new Vector2( bounds.size.x, value ) ;

					CSpriteRenderer.bounds = bounds ;
				}
			}
		}


		/// <summary>
		/// 横幅
		/// </summary>
		public float Width
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.size.x : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.size = new Vector2( value,  CSpriteRenderer.size.y ) ;
				}
			}
		}

		/// <summary>
		/// 縦幅
		/// </summary>
		public float Height
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.size.y : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.size = new Vector2( CSpriteRenderer.size.x, value ) ;
				}
			}
		}

		/// <summary>
		/// マスクインタラクション(スプライトの形にくり抜き制御・スプライトの内側か外側)
		/// </summary>
		public SpriteMaskInteraction MaskInteraction
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.maskInteraction : SpriteMaskInteraction.None ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.maskInteraction = value ;
				}
			}
		}

		/// <summary>
		/// ソートポイント(スプライト同士が同じ SortingLayer OrderInLayer であった時に、どこを基準にソートするか : Center = Center の Y 値が小さい方が手前・Pivot = Pivot の Y 値が小さい方が手前)
		/// </summary>
		public SpriteSortPoint SortPoint
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.spriteSortPoint : SpriteSortPoint.Center ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.spriteSortPoint = value ;
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
				if( CSpriteRenderer == null )
				{
					return null ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						return  CSpriteRenderer.sharedMaterial ;
					}
					else
					{
						return  CSpriteRenderer.material ;
					}
				}
				else
				{
					// 複製あり

					return m_DuplicatedMaterial ;
				}
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				if( m_DuplicatedMaterial == null )
				{
					// 複製なし

					if( Application.isPlaying == false )
					{
						CSpriteRenderer.sharedMaterial = value ;
					}
					else
					{
						CSpriteRenderer.material = value ;
					}
				}
				else
				{
					// 複製あり

					Color color = m_DuplicatedMaterial.color ;

					if( Application.isPlaying == false )
					{
						DestroyImmediate( m_DuplicatedMaterial ) ;
					}
					else
					{
						Destroy( m_DuplicatedMaterial ) ;
					}
					m_DuplicatedMaterial = null ;

					//--

					if( value == null )
					{
						if( Application.isPlaying == false )
						{
							CSpriteRenderer.sharedMaterial = null ;
						}
						else
						{
							CSpriteRenderer.material = null ;
						}
					}
					else
					{
						m_DuplicatedMaterial = Instantiate( value ) ;

						if( Application.isPlaying == false )
						{
							CSpriteRenderer.sharedMaterial = m_DuplicatedMaterial ;
						}
						else
						{
							CSpriteRenderer.material = m_DuplicatedMaterial ;
						}

						m_DuplicatedMaterial.color = color ;
					}
				}
			}
		}

		/// <summary>
		/// ソーティングレイヤー名
		/// </summary>
		public string SortingLayerName
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return null ;
				}

				return CSpriteRenderer.sortingLayerName ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.name == value )
					{
						// 発見(設定可能)
						CSpriteRenderer.sortingLayerName = value ;
					}
				}
			}
		}

		/// <summary>
		/// ソーティングレイヤー値
		/// </summary>
		public int SortingLayer
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return -1 ;
				}

				string sortingLayerName = CSpriteRenderer.sortingLayerName ;

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.name == sortingLayerName )
					{
						// 発見
						return layer.value ;
					}
				}

				// 発見出来ず
				return -1 ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				string sortingLayerName = string.Empty ;

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.value == value )
					{
						// 発見
						sortingLayerName = layer.name ;
					}
				}

				if( string.IsNullOrEmpty( sortingLayerName ) == true )
				{
					// 発見できず
					return ;
				}

				CSpriteRenderer.sortingLayerName = sortingLayerName ;
			}
		}

		/// <summary>
		/// ソーティングレイヤー(オーダーインレイヤーと同じ)
		/// </summary>
		public int SortingOrder
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 0 ;
				}

				return CSpriteRenderer.sortingOrder ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				CSpriteRenderer.sortingOrder = value ;
			}
		}

		/// <summary>
		/// オーダーインレイヤー
		/// </summary>
		public int OrderInLayer
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 0 ;
				}

				return CSpriteRenderer.sortingOrder ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				CSpriteRenderer.sortingOrder = value ;
			}
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
//				SetSpriteAndResize( sprite ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 複製マテリアル
		private Material m_DuplicatedMaterial ;


		//-----------------------------------------------------------

//		internal void OnEnable(){}

//		internal void OnDisable(){}

		internal void OnDestroy()
		{
			// アトラス内スプライトのキャッシュをクリアする
			CleanupAtlasSprites() ;

			//--------------

			// マテリアルが複製されていたら破棄する
			if( m_DuplicatedMaterial != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_DuplicatedMaterial ) ;
				}
				else
				{
					Destroy( m_DuplicatedMaterial ) ;
				}
				m_DuplicatedMaterial = null ;
			}
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

		//-------------------------------------------------------------------------------------------
/*
		internal void Update()
		{
//			Debug.Log( "サイズ : " + CSpriteRenderer.size ) ;

			//----------------------------------------------------------

			// 将来的に連結・多関節キャラクターを扱うのであれば考える

			// 子への色反映(UIButton の場合は別処理を行うのでここでは処理しない)
//			if( m_IsApplyColorToChildren == true && m_EffectiveColorReplacing == false )
//			{
//				ApplyColorToChidren( m_EffectiveColor, true ) ;
//			}

		}
*/
		//-------------------------------------------------------------------------------------------

		// コライダー２Ｄキャッシュ
		protected Collider2D m_Collider ;

		/// <summary>
		/// Collider2D(ショートカット)
		/// </summary>
		public virtual Collider2D CCollider
		{
			get
			{
				if( m_Collider == null )
				{
					gameObject.TryGetComponent<Collider2D>( out m_Collider ) ;
				}
				return m_Collider ;
			}
		}
		
		/// <summary>
		/// Collider2D の有無
		/// </summary>
		public bool IsCollider
		{
			get
			{
				return ( CCollider != null ) ;
			}
		}
		
		/// <summary>
		/// Collider の追加
		/// </summary>
		public void AddCollider<T>() where T : Collider2D
		{
			if( CCollider != null )
			{
				return ;
			}
		
			T collider ;
		
			collider = gameObject.AddComponent<T>() ;
			collider.enabled = true ;
		}

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
			var collider = CCollider ;
			if( collider == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( collider ) ;
			}
			else
			{
				Destroy( collider ) ;
			}
		}

		//----------
		
		// キャッシュ
		private Animator m_Animator = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		public virtual Animator CAnimator
		{
			get
			{
				if( m_Animator == null )
				{
					gameObject.TryGetComponent<Animator>( out m_Animator ) ;
				}
				return m_Animator ;
			}
		}
		
		/// <summary>
		/// Animator の有無
		/// </summary>
		public bool IsAnimator
		{
			get
			{
				return ( CAnimator != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddAnimator() ;
				}
				else
				{
					RemoveAnimator() ;
				}
			}
		}
		
		/// <summary>
		/// Animator の追加
		/// </summary>
		public void AddAnimator()
		{
			if( CAnimator != null )
			{
				return ;
			}
		
			Animator animator ;
		
			animator = gameObject.AddComponent<Animator>() ;
			animator.speed = 1 ;
		}

		/// <summary>
		/// Animator の削除
		/// </summary>
		public void RemoveAnimator()
		{
			var animator = CAnimator ;
			if( animator == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( animator ) ;
			}
			else
			{
				Destroy( animator ) ;
			}
		}

		//-------------------------------------------------------------------------------------------


	}
}
