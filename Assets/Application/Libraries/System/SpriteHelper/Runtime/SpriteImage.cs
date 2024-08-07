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
	/// スプライト制御クラス  Version 2024/05/21
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
			var go = Selection.activeGameObject ;
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

			var t = child.transform ;
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

			var sprites = Resources.LoadAll<Sprite>( "SpriteHelper/Textures/SpriteSet" ) ;
			if( sprites != null && sprites.Length >  0 )
			{
				spriteRenderer.sprite = sprites[ 0 ] ;

				SetSprites( sprites ) ;
			}

			var material = Resources.Load<Material>( "SpriteHelper/Materials/DefaultSprite" ) ;
			if( material != null )
			{
				spriteRenderer.sharedMaterial = material ;
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

					Sprite = null ;	// 選択中のスプライトも初期化する
				}
			}
		}

		// SpriteAtlas から取得した Sprite は Destroy() が必要であるためキャッシュする
		private Dictionary<string,Sprite> m_SpritesInAtlas ;

		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定

		[SerializeField][HideInInspector]
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
				// 基本的にはインスタンスは維持して中身の情報を入れ替えるのでここが呼ばれる事は無い

				if( m_SpriteSet != value )
				{
					m_SpriteSet  = value ;

					Sprite = null ;	// 選択中のスプライトも初期化する
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

		// アトラス内のスプライトをキャッシュにためつつ取得する
		private Sprite[] GetSpritesInAtlas()
		{
			if( m_SpriteAtlas != null )
			{
				CleanupAtlasSprites() ;

				int count = m_SpriteAtlas.spriteCount ;
				if( count >  0 )
				{
					var sprites = new Sprite[ count ] ;
					if( m_SpriteAtlas.GetSprites( sprites ) == count )
					{
						// キャッシュを生成する
						m_SpritesInAtlas ??= new Dictionary<string, Sprite>() ;

						string key = "(Clone)" ;

						foreach( var sprite in sprites )
						{
							// 存在するのでキャッシュに貯める
							string spriteName = sprite.name ;
							if( spriteName.Contains( key ) == true )
							{
								spriteName = spriteName.Replace( key, string.Empty ) ;
								sprite.name = spriteName ;
							}

							m_SpritesInAtlas.Add( spriteName, sprite ) ;
						}

						return sprites ;
					}
				}
			}

			return null ;
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
		/// 全てのスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite[] GetSprites()
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				return GetSpritesInAtlas() ;
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				return m_SpriteSet.GetSprites() ;
			}

			//----------------------------------------------------------

			return null ;
		}

		/// <summary>
		/// 全てのスプライトの識別名を取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNames()
		{
			var sprites = GetSprites() ;
			if( sprites == null )
			{
				return null ;
			}

			var spriteNames = new List<string>() ;

			int i, l = sprites.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				spriteNames.Add( sprites[ i ].name ) ;
			}

			return spriteNames.OrderBy( _ => _ ).ToArray() ;
		}

		/// <summary>
		/// スプライトの数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSpriteCount()
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				return m_SpriteAtlas.spriteCount ;
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				return m_SpriteSet.SpriteCount ;
			}

			//----------------------------------------------------------

			return 0 ;
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
				}

				if( m_DuplicatedMaterial != null )
				{
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
				}

				if( m_DuplicatedMaterial != null )
				{
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

		// 再生中のアニメーション名
		[SerializeField]
		protected string	m_PlayingAnimationName ;

		/// <summary>
		/// 再生中のアニメーション名
		/// </summary>
		public string PlayingAnimationName
		{
			get
			{
				return m_PlayingAnimationName ;
			}
			set
			{
				m_PlayingAnimationName = value ;
			}
		}

		// ループするかどうか
		[SerializeField]
		protected bool		m_IsAnimationLooping = true ;

		/// <summary>
		/// ループするかどうか
		/// </summary>
		public bool IsAnimationLooping
		{
			get
			{
				return m_IsAnimationLooping ;
			}
			set
			{
				m_IsAnimationLooping = value ;
			}
		}

		// 速度係数
		[SerializeField]
		protected float		m_AnimationSpeed = 1.0f ;

		/// <summary>
		/// 速度係数
		/// </summary>
		public float AnimationSpeed
		{
			get
			{
				return m_AnimationSpeed ;
			}
			set
			{
				m_AnimationSpeed = value ;
			}
		}


		// 自動でアニメーションを再生するかどうか
		[SerializeField]
		protected bool		m_AnimationPlayOnAwake ;

		/// <summary>
		/// 自動でアニメーションを再生するかどうか
		/// </summary>
		public bool AnimationPlayOnAwake
		{
			get
			{
				return m_AnimationPlayOnAwake ;
			}
			set
			{
				m_AnimationPlayOnAwake = value ;
			}
		}


		// 再生中かどうか
		private bool		m_IsAnimationPlaying ;

		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public  bool		IsAnimationPlaying
		{
			get
			{
				return m_IsAnimationPlaying ;
			}
			set
			{
				m_IsAnimationPlaying = value ;
			}
		}


		// アニメーションの再生が終了したら呼び出されるコールバック
		private Action<string>	m_OnAimationFinished ;

		/// <summary>
		/// アニメーション終了時に呼び出すコールバックを登録する(ただしワンショット再生時のみ)
		/// </summary>
		/// <param name="OnAninationFinished"></param>
		public void SetOnAninationFinished( Action<string> OnAninationFinished )
		{
			m_OnAimationFinished = OnAninationFinished ;
		}

		//---------------

		/// <summary>
		/// アニメーション定義
		/// </summary>
		[Serializable]
		public class AnimationDescriptor
		{
			/// <summary>
			/// コンストラクタ
			/// </summary>
			public AnimationDescriptor()
			{
				Reset() ;
			}

			/// <summary>
			/// 初期化する
			/// </summary>
			public void Reset()
			{
				AnimationName = "Default" ;
			}

			/// <summary>
			/// 識別名
			/// </summary>
			public string AnimationName = "Default" ;

			[Serializable]
			public class FrameDescriptor
			{
				/// <summary>
				/// スプライト識別名
				/// </summary>
				public string SpriteName ;

				/// <summary>
				/// 表示時間
				/// </summary>
				public float Duration ;
			}

			/// <summary>
			/// フレームで表示するスプライト名
			/// </summary>
			public List<FrameDescriptor>	Frames ;

			//----------------------------------------------------------

			/// <summary>
			/// フレーム群を全て消去する
			/// </summary>
			public void ClearAllFrames()
			{
				Frames = null ;
			}

			/// <summary>
			/// フレーム群を設定する
			/// </summary>
			/// <param name="frames"></param>
			public void SetFrames( IEnumerable<( string spriteName, float duration )> frames )
			{
				if( Frames == null )
				{
					Frames = new List<FrameDescriptor>() ;
				}
				else
				{
					Frames.Clear() ;
				}

				AddFrames( frames ) ;
			}

			/// <summary>
			/// フレーム群を追加する
			/// </summary>
			/// <param name="frames"></param>
			public void AddFrames( IEnumerable<( string spriteName, float duration )> frames )
			{
				Frames ??= new List<FrameDescriptor>() ;

				foreach( var ( spriteName, duration ) in frames )
				{
					if( string.IsNullOrEmpty( spriteName ) == false && duration >  0 )
					{
						Frames.Add( new FrameDescriptor(){ SpriteName = spriteName, Duration = duration } ) ;
					}
				}
			}

			/// <summary>
			/// フレーム群を設定する
			/// </summary>
			/// <param name="frames"></param>
			public void SetFrames( IEnumerable<string> frames, float duration )
			{
				if( Frames == null )
				{
					Frames = new List<FrameDescriptor>() ;
				}
				else
				{
					Frames.Clear() ;
				}

				AddFrames( frames, duration ) ;
			}

			/// <summary>
			/// フレーム群を追加する
			/// </summary>
			/// <param name="frames"></param>
			public void AddFrames( IEnumerable<string> frames, float duration )
			{
				Frames ??= new List<FrameDescriptor>() ;

				foreach( var spriteName in frames )
				{
					if( string.IsNullOrEmpty( spriteName ) == false && duration >  0 )
					{
						Frames.Add( new FrameDescriptor(){ SpriteName = spriteName, Duration = duration } ) ;
					}
				}
			}

			/// <summary>
			/// トータル再生時間を取得する
			/// </summary>
			/// <returns></returns>
			public float GetTime()
			{
				if( Frames == null || Frames.Count == 0 )
				{
					return 0 ;
				}

				float time = 0 ;

				foreach( var frame in Frames )
				{
					time += frame.Duration ;
				}

				return time ;
			}

			/// <summary>
			/// 表示フレームのインデックス番号を取得する
			/// </summary>
			/// <param name="playingTime"></param>
			/// <param name="isLooping"></param>
			/// <returns></returns>
			public int GetFrameIndex( float playingTime, bool isLooping, out float oneShotTime )
			{
				oneShotTime = GetTime() ;
				if( oneShotTime <= 0 )
				{
					return 0 ;
				}

				//---------------------------------

				if( isLooping == true )
				{
					// ループあり
					playingTime %= oneShotTime ;
				}

				if( playingTime <  oneShotTime )
				{
					int index ;
					for( index  = 0 ; index <  Frames.Count ; index ++ )
					{
						if( playingTime <  Frames[ index ].Duration )
						{
							return index ;
						}

						playingTime -= Frames[ index ].Duration ;
					}
				}

				return Frames.Count - 1 ;
			}
		}

		// アニメーション情報群
		[SerializeField]
		protected List<AnimationDescriptor> m_Animations ;

		/// <summary>
		/// アニメーション情報群
		/// </summary>
		public List<AnimationDescriptor> Animations => m_Animations ;

#if UNITY_EDITOR
		[SerializeField][HideInInspector]
		private int m_AnimationCount = 0 ;

		internal void OnValidate()
		{
			if( Application.isPlaying == false )
			{
				int animationCount = 0 ;
				if( m_Animations != null && m_Animations.Count >  0 )
				{
					animationCount = m_Animations.Count ;
				}

				if( animationCount != m_AnimationCount )
				{
					if( m_AnimationCount == 0 && animationCount == 1 )
					{
						// Serializable クラスは、オブジェクトが生成された際に、ＵＩのデフォルト状態値が書き込まれてしまうため、それの回避策。(スクリプトで new を行えばデフォルト値は正しく設定される) 
						m_Animations[ 0 ].Reset() ;
					}

					m_AnimationCount = animationCount ;
				}
			}
		}
#endif

		private Dictionary<string,AnimationDescriptor> m_HashAnimations ;
		private float	m_AnimationTimer		= 0 ;
		private bool	m_AnimationAwake		= false ;	// false の場合は表示の強制更新が必要
		private int		m_AnimationIndex		= 0 ;
		private bool	m_IsAnimationPausing	= false ;

		// アニメーションの準備する
		private void PrepareAnimation()
		{
			if( m_HashAnimations == null )
			{
				m_HashAnimations = new Dictionary<string,AnimationDescriptor>() ;

				if( m_Animations != null && m_Animations.Count >  0 )
				{
					foreach( var animation in m_Animations )
					{
						if( m_HashAnimations.ContainsKey( animation.AnimationName ) == false )
						{
							m_HashAnimations.Add( animation.AnimationName, animation ) ;
						}
					}
				}
			}
		}

		/// <summary>
		/// 全てのアニメーションを削除する
		/// </summary>
		public void ClearAllAnimations()
		{
			m_HashAnimations	= null ;
			m_Animations		= null ;
		}

		/// <summary>
		/// アニメーションを追加する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="frames"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public bool AddAnimation( string animationName, IEnumerable<( string, float )> frames )
		{
			if( string.IsNullOrEmpty( animationName ) == true || frames == null )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			// 事前に削除する
			RemoveAnimation( animationName ) ;

			//----------------------------------

			var animation = new AnimationDescriptor(){ AnimationName = animationName } ;
			animation.SetFrames( frames ) ;

			m_Animations.Add( animation ) ;
			m_HashAnimations.Add( animationName, animation ) ;

			//----------------------------------

			return true ;
		}

		/// <summary>
		/// アニメーションを追加する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="frames"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public bool AddAnimation( string animationName, IEnumerable<string> frames, float duration )
		{
			if( string.IsNullOrEmpty( animationName ) == true || frames == null || duration <= 0 )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			// 事前に削除する
			RemoveAnimation( animationName ) ;

			//----------------------------------

			var animation = new AnimationDescriptor(){ AnimationName = animationName } ;
			animation.SetFrames( frames, duration ) ;

			m_Animations.Add( animation ) ;
			m_HashAnimations.Add( animationName, animation ) ;

			//----------------------------------

			return true ;
		}

		/// <summary>
		/// アニメーションを削除する
		/// </summary>
		/// <param name="animationName"></param>
		/// <returns></returns>
		public bool RemoveAnimation( string animationName )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			if( m_HashAnimations != null && m_HashAnimations.Count >  0 )
			{
				if( m_HashAnimations.ContainsKey( animationName ) == true )
				{
					m_HashAnimations.Remove( animationName ) ;
				}
			}

			if( m_Animations != null && m_Animations.Count >  0 )
			{
				var animations = m_Animations.Where( _ => _.AnimationName == animationName ).ToArray() ;
				if( animations != null && animations.Length >  0 )
				{
					m_Animations.RemoveRange( animations ) ;
				}
			}

			return true ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, Action<string> onFinished = null, bool isRestart = true )
		{
			return PlayAnimation( animationName, m_IsAnimationLooping, m_AnimationSpeed, onFinished, isRestart ) ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, bool isLooping, Action<string> onFinished = null, bool isRestart = true )
		{
			return PlayAnimation( animationName, isLooping, m_AnimationSpeed, onFinished, isRestart ) ;
		}

		/// <summary>
		/// アニメーションを再生する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="isLooping"></param>
		/// <param name="speed"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public bool PlayAnimation( string animationName, bool isLooping, float speed, Action<string> onFinished = null, bool isRestart = true )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			if( m_HashAnimations.ContainsKey( animationName ) == false )
			{
				// 再生不可
				return false ;
			}

			//----------------------------------

			if( isRestart == false )
			{
				// 既に再生中のアニメーションと同じアニメーションの再生は無視する
				if( m_IsAnimationPlaying == true )
				{
					if( m_PlayingAnimationName == animationName )
					{
						// 既に同じアニメーションが再生中になっている
						return true ;
					}
				}
			}

			//----------------------------------

			m_PlayingAnimationName	= animationName ;
			m_IsAnimationLooping	= isLooping ;
			m_AnimationSpeed		= speed ;
			m_OnAimationFinished	= onFinished ;

			//----------------------------------

			var animation = GetAnimation( m_PlayingAnimationName ) ;
			if( animation != null )
			{
				// 再生有効
				PlayAnimation_Private() ;

				return true ;
			}

			// 再生不可
			return false ;
		}

		// アニメーションの再生を実行する
		private void PlayAnimation_Private()
		{
			m_IsAnimationPlaying	= true ;
			m_IsAnimationPausing	= false ;

			m_AnimationTimer		= 0 ;
			m_AnimationAwake		= false ;
			m_AnimationIndex		= 0 ;
		}

		/// <summary>
		/// アニメーションを停止させる
		/// </summary>
		public void StopAnimation()
		{
			m_IsAnimationPlaying = false ;
			m_IsAnimationPausing = false ;

			m_AnimationAwake	 = false ;
		}

		/// <summary>
		/// 一時停止を実行する
		/// </summary>
		public void PauseAnimation()
		{
			if( m_IsAnimationPlaying == true )
			{
				m_IsAnimationPausing = true ;
			}
		}

		/// <summary>
		/// 一時停止を終了する
		/// </summary>
		public void UnpauseAnimation()
		{
			if( m_IsAnimationPlaying == true )
			{
				m_IsAnimationPausing = false ;

				m_AnimationAwake	 = false ;
			}
		}

		/// <summary>
		/// 再生中のアニメーションの状態を取得する
		/// </summary>
		/// <param name="animationName"></param>
		/// <param name="timer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool GetPlayingAnimationState( out string animationName, out float timer, out int index )
		{
			animationName	= null ;
			timer			= 0 ;
			index			= 0 ;

			if( m_IsAnimationPlaying == false )
			{
				// アニメーションは再生中ではない
				return false ;
			}

			//----------------------------------------------------------

			animationName	= m_PlayingAnimationName ;

			timer			= m_AnimationTimer ;
			index			= m_AnimationIndex ;

			return true ;
		}

		/// <summary>
		/// 再生中のアニメーションの状態を強制的に設定する
		/// </summary>
		/// <returns></returns>
		public bool SetPlayingAnimationState( string animationName, float timer, int index )
		{
			if( string.IsNullOrEmpty( animationName ) == true )
			{
				// 不可
				return false ;
			}

			//----------------------------------

			PrepareAnimation() ;

			if( m_HashAnimations.ContainsKey( animationName ) == false )
			{
				// 再生不可
				return false ;
			}

			//----------------------------------

			var animation = GetAnimation( animationName ) ;
			if( animation != null )
			{
				if( animation.Frames == null || animation.Frames.Count == 0 )
				{
					// 再生不可
					return false ;
				}

				index %= animation.Frames.Count ;

				// 再生有効
				m_PlayingAnimationName = animationName ;

				m_IsAnimationPlaying = true ;

				m_AnimationTimer = timer ;
				m_AnimationAwake = false ;
				m_AnimationIndex = index ;
				
				return true ;
			}

			// 再生不可
			return false ;
		}

		//---------------

		// 再生するアニメーションを取得する
		private AnimationDescriptor GetAnimation( string playingAnimationName )
		{
			if( ( m_SpriteAtlas != null || m_SpriteSet != null ) && m_HashAnimations != null && m_HashAnimations.Count >  0 )
			{
				if( string.IsNullOrEmpty( playingAnimationName ) == false && m_HashAnimations.ContainsKey( playingAnimationName ) == true )
				{
					var animation = m_HashAnimations[ playingAnimationName ] ;
					if( animation != null && animation.Frames != null && animation.Frames.Count >  0 )
					{
						return animation ;
					}
				}
			}

			return null ;
		}

		//-----------------------------------------------------------

		internal void Start()
		{
			if( Application.isPlaying == true )
			{
				PrepareAnimation() ;

				var animation = GetAnimation( m_PlayingAnimationName ) ;
				if( animation != null )
				{
					if( m_AnimationPlayOnAwake == true )
					{
						// 自動再生有効
						PlayAnimation_Private() ;
					}
				}
			}
		}

		internal void Update()
		{
			if( Application.isPlaying == true )
			{
				var animation = GetAnimation( m_PlayingAnimationName ) ;
				if( animation != null )
				{
					if( m_IsAnimationPlaying == true )
					{
						if( m_IsAnimationPausing == false )
						{
							// ポーズ中でなければ時間を進める
							m_AnimationTimer += ( Time.deltaTime * m_AnimationSpeed ) ;
						}

						int animationIndex = animation.GetFrameIndex( m_AnimationTimer, m_IsAnimationLooping, out var oneShotTime ) ;
						if( m_AnimationAwake == false || m_AnimationIndex != animationIndex || m_AnimationTimer >= oneShotTime )
						{
							m_AnimationAwake  = true ;
							m_AnimationIndex  = animationIndex ;

							var spriteName = animation.Frames[ m_AnimationIndex ].SpriteName ;

							// スプライト画像を設定する
							SetSpriteInAtlas( spriteName ) ;

							if( m_IsAnimationLooping == false )
							{
								// ループなし
								if( m_AnimationTimer >= oneShotTime )
								{
									// 再生終了
									m_OnAimationFinished?.Invoke( m_PlayingAnimationName ) ;
									m_IsAnimationPlaying = false ;
								}
							}
							else
							{
								// ループあり
								if( oneShotTime >  0 )
								{
									m_AnimationTimer %= oneShotTime ;
								}
							}
						}
					}
				}
			}
		}

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
			collider.isTrigger = true ;

			if( TryGetComponent<Rigidbody2D>( out _ ) == false )
			{
				var rigidbody2d = gameObject.AddComponent<Rigidbody2D>() ;
				rigidbody2d.gravityScale = 0 ;
			}
		}

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
			if( TryGetComponent<Rigidbody2D>( out var rigidbody2d ) == true )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( rigidbody2d ) ;
				}
				else
				{
					Destroy( rigidbody2d ) ;
				}
			}

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
		// Interpolation 関係

		/// <summary>
		/// 線形補間値
		/// </summary>
		public float InterpolationValue
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 0 ;
				}

				Material material ;
				if( m_DuplicatedMaterial == null )
				{
					// 複製なし
					if( Application.isPlaying == false )
					{
						material = CSpriteRenderer.sharedMaterial ;
					}
					else
					{
						material = CSpriteRenderer.material ;
					}
				}
				else
				{
					// 複製あり
					material = m_DuplicatedMaterial ;
				}

				if( material == null )
				{
					return 0 ;
				}

				//---------------------------------

				string key = "_InterpolationValue" ;

				if( material.HasFloat( key ) == false )
				{
					return 0 ;
				}

				return material.GetFloat( key ) ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				Material material ;

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
				}

				if( m_DuplicatedMaterial == null )
				{
					return ;
				}

				material = m_DuplicatedMaterial ;
				
				//---------------------------------

				string key = "_InterpolationValue" ;

				if( material.HasFloat( key ) == false )
				{
					return ;
				}

				material.SetFloat( key, value ) ;
			}
		}

		/// <summary>
		/// 線形補間色
		/// </summary>
		public Color InterpolationColor
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return Color.white ;
				}

				Material material ;
				if( m_DuplicatedMaterial == null )
				{
					// 複製なし
					if( Application.isPlaying == false )
					{
						material = CSpriteRenderer.sharedMaterial ;
					}
					else
					{
						material = CSpriteRenderer.material ;
					}
				}
				else
				{
					// 複製あり
					material = m_DuplicatedMaterial ;
				}

				if( material == null )
				{
					return Color.white ;
				}

				//---------------------------------

				string key = "_InterpolationColor" ;

				if( material.HasColor( key ) == false )
				{
					return Color.white ;
				}

				return material.GetColor( key ) ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				Material material ;

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
				}

				if( m_DuplicatedMaterial == null )
				{
					return ;
				}

				material = m_DuplicatedMaterial ;
				
				//---------------------------------

				string key = "_InterpolationColor" ;

				if( material.HasColor( key ) == false )
				{
					return ;
				}

				material.SetColor( key, value ) ;
			}
		}
	}

	/// <summary>
	/// リストの拡張メソッド
	/// </summary>
	public static class ExList
	{
		/// <summary>
		/// 要素をまとめて破棄する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="collection"></param>
		public static void RemoveRange<T>( this List<T> list, IEnumerable<T> collection )
		{
			if( collection == null )
			{
				return ;
			}

			foreach( T element in collection )
			{
				list.Remove( element ) ;
			}
		}
	}
}
