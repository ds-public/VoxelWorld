using System ;
using System.Collections ;
using System.Collections.Generic ;
//using System.Linq ;
using UnityEngine ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace MeshHelper
{
	/// <summary>
	/// ２Ｄメッシュ Version 2024/06/14
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( MeshRenderer ) )]
	[RequireComponent( typeof( MeshFilter ) )]
	public class SoftMesh2D : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/MeshHelper/SoftMesh2D", false, 22 )]	// ポップアップメニューから
//		[MenuItem( "MeshHelper/Add a SoftMesh2D" )]					// メニューから
		public static void CreateSoftMesh2D()
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

			Undo.RecordObject( go, "Add a child SoftMesh2D" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SoftMesh2D" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SoftMesh2D>() ;
			component.SetDefault( true ) ;	// 初期状態に設定する

            // 一番上に移動させる
			while( ComponentUtility.MoveComponentUp( component ) ){}

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

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public void SetDefault( bool useSample = false )
		{
			if( useSample == false )
			{
				// サンプルは設定しない
				return ;
			}

			var sprites = Resources.LoadAll<Sprite>( "MeshHelper/Sprites/Sample" ) ;
			if( sprites != null && sprites.Length >  0 )
			{
				Sprite = sprites[ 0 ] ;
			}
		}

		//-----------------------------------------------------------
		// テクスチャ

		[SerializeField][HideInInspector]
		protected Texture m_Texture ;

		/// <summary>
		/// テクスチャ
		/// </summary>
		public Texture Texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				if( m_Texture != value )
				{
					m_Texture			= value ;
					m_IsMaterialDirty	= true ;
				}
			}
		}

		// スプライトからスプライトを更新する
		private void UpdateTexture( Sprite sprite )
		{
			if( sprite != null )
			{
				Texture = sprite.texture ;
			}
			else
			{
				Texture = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// スプライト
		[SerializeField][HideInInspector]
		private Sprite	m_Sprite = null ;

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public    Sprite	 Sprite
		{
			get
			{
				return m_Sprite ;
			}
			set
			{
				if( m_Sprite != value )
				{
					m_Sprite  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
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
				if( m_SpriteSet != value )
				{
					// 基本的にはインスタンスは維持して中身の情報を入れ替えるのでここが呼ばれる事は無い

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

		// テクスチャのサイズにリサイズする
		private void SetNativeSize()
		{
			if( m_Sprite != null )
			{
				Size = new Vector2( m_Sprite.rect.width, m_Sprite.rect.height ) ; 
			}
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
		/// メッシュ形状
		/// </summary>
		public enum ShapeTypes
		{
			Rectangle	= 1,
			Circle		= 2,
		}

		[ SerializeField ][ HideInInspector ]
		protected ShapeTypes m_ShapeType = ShapeTypes.Rectangle ;

		/// <summary>
		/// 形状の種別
		/// </summary>
		public    ShapeTypes  ShapeType
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
					m_IsMeshDirty = true ;

					if( CCollider2D != null || CCollider3D != null )
					{
						RemoveCollider() ;
						AddCollider() ;
					}
				}
			}
		}

		//------------------------------
		// 以下は共通

		/// <summary>
		/// オフセット
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Vector2 m_Offset = Vector2.zero ;

		public    Vector2   Offset
		{
			get
			{
				return m_Offset ;
			}
			set
			{
				if( m_Offset.Equals( value ) == false )
				{
					m_Offset = value ;
					m_IsMeshDirty = true ;
				}
			}
		}	

		/// <summary>
		/// サイズ
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Vector2 m_Size = Vector2.one ;

		public    Vector2   Size
		{
			get
			{
				return m_Size ;
			}
			set
			{
				if( m_Size.Equals( value ) == false )
				{
					m_Size = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		protected bool	m_FlipX ;

		/// <summary>
		/// 左右反転
		/// </summary>
		public bool FlipX
		{
			get
			{
				return m_FlipX ;
			}
			set
			{
				if( m_FlipX != value )
				{
					m_FlipX  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected bool	m_FlipY ;

		/// <summary>
		/// 上下反転
		/// </summary>
		public bool FlipY
		{
			get
			{
				return m_FlipY ;
			}
			set
			{
				if( m_FlipY != value )
				{
					m_FlipY  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		// 線の太さ(０で塗りつぶし)
		[ SerializeField ][ HideInInspector ]
		protected float m_LineWidth = 0 ;

		/// <summary>
		/// 線の太さ(０で塗りつぶし)
		/// </summary>
		public    float  LineWidth
		{
			get
			{
				return m_LineWidth ;
			}
			set
			{
				if( m_LineWidth != value )
				{
					m_LineWidth  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//-----------------------------------

		/// <summary>
		/// メッシュの展開方法(四角限定)
		/// </summary>
		public enum ImageTypes
		{
			/// <summary>
			/// ボーダー無効
			/// </summary>
			Simple = 0,

			/// <summary>
			/// ボーダー有効
			/// </summary>
			Sliced = 1,
		}

		[HideInInspector][SerializeField]
		protected ImageTypes m_ImageType = ImageTypes.Simple ;

		/// <summary>
		/// メッシュの展開方法(四角限定)
		/// </summary>
		public    ImageTypes  ImageType
		{
			get
			{
				return m_ImageType ;
			}
			set
			{
				if( m_ImageType != value )
				{
					m_ImageType  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		/// <summary>
		/// スライスを考慮した基本サイズ
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Vector2 m_BorderSize = Vector2.one ;

		public    Vector2   BorderSize
		{
			get
			{
				return m_BorderSize ;
			}
			set
			{
				if( m_BorderSize.Equals( value ) == false )
				{
					m_BorderSize = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		// ９パッチタイプの場合に中央を描画するかどうか(四角限定)
		[ SerializeField ][ HideInInspector ]
		protected bool m_FillCenter = true ;

		/// <summary>
		/// ９パッチタイプの場合に中央を描画するかどうか(四角限定)
		/// </summary>
		public    bool  FillCenter
		{
			get
			{
				return m_FillCenter ;
			}
			set
			{
				if( m_FillCenter != value )
				{
					m_FillCenter  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		// テクスチャのタイリング(四角限定)
		[ SerializeField ][ HideInInspector ]
		protected bool m_Tiling = false ;

		/// <summary>
		/// テクスチャのタイリング(四角限定)
		/// </summary>
		public    bool   Tiling
		{
			get
			{
				return m_Tiling ;
			}
			set
			{
				if( m_Tiling != value )
				{
					m_Tiling  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		/// <summary>
		/// テクスチャの張り方(円・塗りつぶし有り限定)
		/// </summary>
		public enum DecalTypes
		{
			/// <summary>
			/// 通常
			/// </summary>
			Normal = 0,

			/// <summary>
			/// 効果
			/// </summary>
			Effect = 1,
		}

		[HideInInspector][SerializeField]
		protected DecalTypes m_DecalType = DecalTypes.Normal ;

		/// <summary>
		/// テクスチャの張り方(円・塗りつぶし有り限定)
		/// </summary>
		public    DecalTypes  DecalType
		{
			get
			{
				return m_DecalType ;
			}
			set
			{
				if( m_DecalType != value )
				{
					m_DecalType  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 横方向の分割数(四角専用)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected int m_GridX = 1 ;

		/// <summary>
		/// 分割数
		/// </summary>
		public    int  GridX
		{
			get
			{
				return m_GridX ;
			}
			set
			{
				if( m_GridX != value )
				{
					m_GridX  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		/// <summary>
		/// 縦方向の分割数(四角専用)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected int m_GridY = 1 ;

		/// <summary>
		/// 分割数
		/// </summary>
		public    int  GridY
		{
			get
			{
				return m_GridY ;
			}
			set
			{
				if( m_GridY != value )
				{
					m_GridY  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

        /// <summary>
        /// グリッドを設定する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetGrid( int x, int y )
        {
            if( x <= 0 )
            {
                x  = 1 ;
            }
            if( y <= 0 )
            {
                y  = 1 ;
            }

            if( m_GridX != x || m_GridY != y )
            {
                m_GridX = x ;
                m_GridY = y ;
                m_IsMeshDirty = true ;
            }
        }

		/// <summary>
		/// 分割数(円専用)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected int m_Split = 0 ;

		/// <summary>
		/// 分割数
		/// </summary>
		public    int  Split
		{
			get
			{
				return m_Split ;
			}
			set
			{
				if( m_Split != value )
				{
					m_Split  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 基本色(頂点カラー)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Color m_BasisColor = Color.white ;

		/// <summary>
		/// 基本色(頂点カラー)
		/// </summary>
		public    Color  BasisColor
		{
			get
			{
				return m_BasisColor ;
			}
			set
			{
				if
				(
					m_BasisColor.r != value.r ||
					m_BasisColor.g != value.g ||
					m_BasisColor.b != value.b ||
					m_BasisColor.a != value.a
				)
				{
					m_BasisColor.r  = value.r ;
					m_BasisColor.g  = value.g ;
					m_BasisColor.b  = value.b ;
					m_BasisColor.a  = value.a ;

					m_IsMeshDirty = true ;
				}
			}
		}

		/// <summary>
		/// 内側色(頂点カラー)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Color m_InnerColor = Color.white ;

		public    Color  InnerColor
		{
			get
			{
				return m_InnerColor ;
			}
			set
			{
				if
				(
					m_InnerColor.r != value.r ||
					m_InnerColor.g != value.g ||
					m_InnerColor.b != value.b ||
					m_InnerColor.a != value.a
				)
				{
					m_InnerColor.r  = value.r ;
					m_InnerColor.g  = value.g ;
					m_InnerColor.b  = value.b ;
					m_InnerColor.a  = value.a ;

					m_IsMeshDirty = true ;
				}
			}
		}

		/// <summary>
		/// 外側色(頂点カラー)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Color m_OuterColor = Color.white ;

		public    Color  OuterColor
		{
			get
			{
				return m_OuterColor ;
			}
			set
			{
				if
				(
					m_OuterColor.r != value.r ||
					m_OuterColor.g != value.g ||
					m_OuterColor.b != value.b ||
					m_OuterColor.a != value.a
				)
				{
					m_OuterColor.r  = value.r ;
					m_OuterColor.g  = value.g ;
					m_OuterColor.b  = value.b ;
					m_OuterColor.a  = value.a ;

					m_IsMeshDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// 以下は図形の形によってあったりなかったりする項目



		//-----------------------------------------------------------

		/// <summary>
		/// どちらの方向を向いた面にするか
		/// </summary>
		public enum DirectionTypes
		{
			X_Axis = 0,
			Y_Axis = 1,
			Z_Axis = 2,
		}

		[ SerializeField ][ HideInInspector ]
		protected  DirectionTypes m_DirectionType = DirectionTypes.Z_Axis ;

		/// <summary>
		/// メッシュの上下方向の軸
		/// </summary>
		public     DirectionTypes   DirectionType
		{
			get
			{
				return m_DirectionType ;
			}
			set
			{
				if( m_DirectionType != value )
				{
					m_DirectionType		= value ;
					m_IsMeshDirty		= true ;

					if( CCollider2D != null || CCollider3D != null )
					{
						RemoveCollider() ;
						AddCollider() ;
					}
				}
			}
		}

		[ SerializeField ][ HideInInspector ]
		protected  bool m_IsDirectionInverse = false ;

		/// <summary>
		/// メッシュの上下方向を反転するかどうか
		/// </summary>
		public     bool   IsDirectionInverse
		{
			get
			{
				return m_IsDirectionInverse ;
			}
			set
			{
				if( m_IsDirectionInverse != value )
				{
					m_IsDirectionInverse  = value ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//--------------------------------------------------------------------------------------------

		// マテリアル
		[SerializeField][HideInInspector]
		private Material m_Material ;

		// 複製マテリアル
		private Material m_DuplicatedMaterial ;

		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material	Material
		{
			get
			{
				if( Application.isPlaying == false )
				{
					return m_Material ;
				}
				else
				{
					return m_DuplicatedMaterial == null ? m_Material : m_DuplicatedMaterial ;
				}
			}
			set
			{
				if( m_Material != value )
				{
					// オリジナルに変更がある場合は複製は削除する必要がある
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
					}

					m_Material = value ;

					// マテリアルの設定を更新する
					UpdateMaterial( m_MaterialColor, m_Texture ) ;
				}
			}
		}

		//-----------------------------------

		// マテリアル
		[SerializeField][HideInInspector]
		private Color m_MaterialColor = Color.white ;

		/// <summary>
		/// マテリアルカラー
		/// </summary>
		public Color Color
		{
			get
			{
				return m_MaterialColor ;
			}
			set
			{
				if( m_MaterialColor.Equals( value ) == false )
				{
					m_MaterialColor = value ;
					m_IsMaterialDirty = true ;
				}
			}
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// コライダーの自動調整
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected bool m_ColliderAdjustment = true ;

		/// <summary>
		/// コライダーの自動調整
		/// </summary>
		public    bool	 ColliderAdjustment
		{
			get
			{
				return m_ColliderAdjustment ;
			}
			set
			{
				if( m_ColliderAdjustment != value )
				{
					m_ColliderAdjustment	= value ;

					if( m_ColliderAdjustment == true )
					{
						m_IsColliderDirty	= true ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// メッシュレンダラー
		private MeshRenderer	m_MeshRenderer ;

		// メッシュフィルター
		private MeshFilter		m_MeshFilter ;

		// メッシュ
		private Mesh			m_Mesh = null ;

		// マテリアルの更新が必要かどうか
		private bool			m_IsMaterialDirty = false ;

		// メッシュの更新が必要かどうか
		private bool			m_IsMeshDirty = false ;

		// コライダーの更新が必要かどうか
		private bool			m_IsColliderDirty = false ;

		//---------------------------------------------------------------

		internal void Awake()
		{
			// 複製時を想定した強制更新
			m_IsMaterialDirty	= true ;
			m_IsMeshDirty		= true ;
			m_IsColliderDirty	= true ;
		}

		/// <summary>
		/// 開始する際に呼び出される
		/// </summary>
		internal void Start()
		{
			m_MeshRenderer	= GetComponent<MeshRenderer>() ;

			if( TryGetComponent<MeshFilter>( out m_MeshFilter ) == true )
			{
				m_Mesh = new Mesh() ;
				if( Application.isPlaying == false )
				{
					m_MeshFilter.sharedMesh = m_Mesh ;
				}
				else
				{
					m_MeshFilter.mesh = m_Mesh ;
				}
			}

			//----------------------------------

			// 強制更新
			Refresh() ;
		}

		/// <summary>
		/// 更新される際に呼び出される(LateUpdate でなければならない。Mesh の変更要求を Update より後に実行すると、反映が次のフレームになってしまうため)
		/// </summary>
		internal void LateUpdate()
		{
			if( m_IsMaterialDirty == true || m_IsMeshDirty == true || m_IsColliderDirty == true )
			{
				// 更新が必要
				Refresh() ;
			}

			//-----------------------------------
#if false
			if( m_Mesh != null )
			{
				Material material ;

				if( m_DuplicatedMaterial == null )
				{
					material = m_Material ;
				}
				else
				{
					material = m_DuplicatedMaterial ;
				}

				// メッシュを描画する			
//				Graphics.DrawMesh( m_Mesh, transform.position, transform.rotation, material, 0 ) ;
				Graphics.DrawMesh( m_Mesh, transform.localToWorldMatrix, material, 0 ) ;
			}
#endif
		}

		/// <summary>
		/// 破棄される際に呼び出される
		/// </summary>
		internal void OnDestroy()
		{
			// アトラス内スプライトのキャッシュをクリアする
			CleanupAtlasSprites() ;

			if( m_Mesh != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Mesh ) ;
				}
				else
				{
					Destroy( m_Mesh ) ;
				}
				m_Mesh = null ;
			}

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
					if( Application.isPlaying == false )
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								DestroyImmediate( sprite.Value ) ;
							}
						}
					}
					else
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								Destroy( sprite.Value ) ;
							}
						}
					}

					m_SpritesInAtlas.Clear() ;
				}

				m_SpritesInAtlas = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクティブ状態を切り替える
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// アクティブかどうか
		/// </summary>
		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		/// <summary>
		/// レイヤーを設定する
		/// </summary>
		/// <param name="layer"></param>
		public void SetLayer( int layer )
		{
			gameObject.layer = layer ;
		}

		/// <summary>
		/// サイズを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetSize( float x, float y )
		{
			Size = new Vector2( x, y ) ;
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void SetPosition( float x, float y, float z )
		{
			transform.localPosition = new Vector3( x, y, z ) ;
		}

		/// <summary>
		/// マテリアルの色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( Color color )
		{
			Color = color ;
		}

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
		/// 更新する
		/// </summary>
		public void Refresh()
		{
			// テクスチャ設定の保険
			if( m_Texture == null && m_Sprite != null )
			{
				m_Texture  = m_Sprite.texture ;
				m_IsMaterialDirty = true ;
			}

			//----------------------------------

			if( m_IsMaterialDirty == true )
			{
				// マテリアル設定の更新
				UpdateMaterial( m_MaterialColor, m_Texture ) ;
			}

			//--------------

			if( m_IsMeshDirty == true )
			{
				// 形状の更新
				switch( m_ShapeType )
				{
					case ShapeTypes.Rectangle	: CreateRectangle()	; break ;
					case ShapeTypes.Circle		: CreateCircle()	; break ;
				}
			}

			//--------------

			// コライダーの更新
			if( m_ColliderAdjustment == true )
			{
				if( m_IsMeshDirty == true || m_IsColliderDirty == true )
				{
					AdjustCollider() ;
				}
			}

			//----------------------------------------------------------

			m_IsMaterialDirty	= false ;
			m_IsMeshDirty		= false ;
			m_IsColliderDirty	= false ;
		}

		//-------------------------------------------------------------------------------------------

		// マテリアルの設定を更新する
		private void UpdateMaterial( Color color, Texture texture )
		{
			if( m_Material == null )
			{
				m_Material = Resources.Load<Material>( "MeshHelper/Materials/2D/Default2D" ) ;
			}

			if( m_Material != null )
			{
				if( m_DuplicatedMaterial == null )
				{
					if( m_Material.color.Equals( color ) == false || ( texture != null && m_Material.mainTexture != texture ) )
					{
						if( m_DuplicatedMaterial == null )
						{
							m_DuplicatedMaterial = Instantiate( m_Material ) ;
						}
					}
				}

				if( m_DuplicatedMaterial == null )
				{
					// オリジナルのまま

					if( m_MeshRenderer != null )
					{
						if( Application.isPlaying == false )
						{
							// Editor
							m_MeshRenderer.sharedMaterial = m_Material ;
						}
						else
						{
							// Runtime
							m_MeshRenderer.material = m_Material ;
						}
					}
				}
				else
				{
					// 既に複製が生成済み

					m_DuplicatedMaterial.color			= color ;
					m_DuplicatedMaterial.mainTexture	= texture ;

					if( m_MeshRenderer != null )
					{
						if( Application.isPlaying == false )
						{
							// Editor
							m_MeshRenderer.sharedMaterial = m_DuplicatedMaterial ;
						}
						else
						{
							// Runtime
							m_MeshRenderer.material = m_DuplicatedMaterial ;
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メッシュを更新する
		/// </summary>
		/// <param name="aVD"></param>
		/// <param name="aCD"></param>
		/// <param name="aTD"></param>
		/// <param name="aID"></param>
		public void Build( string modelName, Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI, DirectionTypes directionType, bool isDirectionInverse, Vector3 offset )
		{
			//----------------------------------------------------------
			// 基準軸変更の対応

			//----------------------------------------------------------
			// 基準軸変更の対応

			int i, l ;

			Vector3 v ;
			Vector3 n ;

			float vs, ns ;

			if( directionType == DirectionTypes.Y_Axis )
			{
				// ＹとＺを入れ替え
				l = aV.Length ;

				if( isDirectionInverse == false )
				{
					// 通常
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  =   v.y ;
						v.y = - v.z ;
						v.z =   vs ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  =   n.y ;
						n.y = - n.z ;
						n.z =   ns ;

						aN[ i ] = n ;
					}
				}
				else
				{
					// 反転
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  =   v.y ;
						v.y = - v.z ;
						v.z =   vs ;

						v.x = - v.x ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  =   n.y ;
						n.y = - n.z ;
						n.z =   ns ;

						n.y = - n.y ;

						aN[ i ] = n ;
					}
				}
			}
			else
			if( directionType == DirectionTypes.X_Axis )
			{
				// ＸとＺを入れ替え
				l = aV.Length ;

				if( isDirectionInverse == false )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  =   v.x ;
						v.x =   v.z ;
						v.z =   vs ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  =   n.x ;
						n.x =   n.z ;
						n.z =   ns ;

						aN[ i ] = n ;
					}
				}
				else
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  = - v.x ;
						v.x =   v.z ;
						v.z =   vs ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  = - n.x ;
						n.x =   n.z ;
						n.z =   ns ;

						aN[ i ] = n ;
					}
				}
			}
			else
			if( directionType == DirectionTypes.Z_Axis )
			{
				if( isDirectionInverse == true )
				{
					l = aV.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						v.x = - v.x ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						n.z = - n.z ;

						aN[ i ] = n ;
					}
				}
			}

			//----------------------------------------------------------
			// オフセットを加える

			if( offset.x != 0 || offset.y != 0 || offset.z != 0 )
			{
				l = aV.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					aV[ i ] += offset ;
				}
			}

			//----------------------------------------------------------
			// メッシュ生成

			if( m_Mesh == null )
			{
				m_Mesh = new () ;
			}
			else
			{
				m_Mesh.Clear() ;
			}

			// 頂点数に応じてインデックスのビット数に適切なものを設定する
			if( aV.Length >= 65535 )
			{
				// タイリングでは必須といえる
				m_Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 ;	// 頂点数の最大値を増やす
			}
			else
			{
				m_Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16 ;
			}

			m_Mesh.name			= modelName ;
			m_Mesh.vertices		= aV ;
			if( aN != null )
			{
				m_Mesh.normals	= aN ;
			}
			m_Mesh.colors		= aC ;
			m_Mesh.uv			= aT ;

			m_Mesh.triangles	= aI ;

			// １メッシュあたりのポリゴン数に制限は無い模様
#if false
			// ポリゴン数が 65536 を超える場合は複数のサブメッシュに分割する
			int limit = 65536 * 3 ;
			if( aI.Length <= limit )
			{
				// 分割なし
				Debug.Log( "分割なし" ) ;

				m_Mesh.triangles	= aI ;
			}
			else
			{
				// 分割あり
				Debug.Log( "分割あり" ) ;

				int count = aI.Length ;

				int index = count / limit ;
				if( ( count % limit ) >  0 )
				{
					index ++ ;
				}

				Debug.Log( "分割数 : " + index ) ;

				// サブメッシュ数は SetTriangles() でサブメッシュを設定する前に設定する必要がある
				m_Mesh.subMeshCount = index ;

				int point ;

				var aL = new List<int>( aI ) ;

				index = 0 ;
				for( point  = 0 ; point <  count ; point += limit )
				{
					int range = count - point ;
					range = Math.Min( range, limit ) ;

					m_Mesh.SetTriangles( aL.GetRange( point, range ), index ) ;
					index ++ ;
				}
			}
#endif
			if( aN == null )
			{
				m_Mesh.RecalculateNormals() ;
			}
			m_Mesh.RecalculateBounds() ;
		}

		//--------------------------------------------------------------------------------------------

		// 四角のメッシュを生成する
		private void CreateRectangle()
		{
			CreateRectangle
			(
				m_Offset.x, m_Offset.y,
				m_Size.x, m_Size.y,
				m_Sprite,
				m_BasisColor, m_InnerColor, m_OuterColor,
				m_LineWidth,
				m_GridX, m_GridY,
				m_Tiling,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// 四角を生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="c"></param>
		/// <param name="uv"></param>
		/// <param name="split"></param>
		/// <param name="tiling"></param>
		public void CreateRectangle
		(
			float px, float py,
			float sx, float sy,
			Sprite sprite,
			Color color,
			Color innerColor,
			Color outerColor,
			float lineWidth,
			int gridX, int gridY,
			bool tiling,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset		= new Vector2( px, py ) ;
			m_Size			= new Vector2( sx, sy ) ;

			m_Sprite		= sprite ;

			m_BasisColor	= color ;
			m_InnerColor	= innerColor ;
			m_OuterColor	= outerColor ;

			m_GridX			= gridX ;
			m_GridY			= gridY ;

			m_Tiling		= tiling ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			float x, y ;
			int i ;

			//--------------

			// ４つの頂点
			float vx0, vy0 ;
			float vx1, vy1 ;
			float vz ;

			float tx0, ty0 ;
			float tx1, ty1 ;

			//--------------

			int lx, ly ;
			
			//----------------------------------------------------------

			var normal = new Vector3(  0,  0, -1 ) ;

			if( lineWidth >  0 && sx >  ( lineWidth * 2 ) &&  sx >  ( lineWidth * 2 ) )
			{
				// 塗りつぶし無し

				// ９パッチの座標をピックアップする

				vx0 = sx * - 0.5f ;
				vx1 = sx *   0.5f ;
				vy0 = sy * - 0.5f ;
				vy1 = sy *   0.5f ;

				vz  = 0 ;

				if( sprite != null )
				{
					tx0 = sprite.rect.xMin / sprite.texture.width  ;
					tx1 = sprite.rect.xMax / sprite.texture.width  ;
					ty0 = sprite.rect.yMin / sprite.texture.height ;
					ty1 = sprite.rect.yMax / sprite.texture.height ;
				}
				else
				{
					tx0 = 0 ;
					tx1 = 1 ; 
					ty0 = 0 ;
					ty1 = 1 ;
				}

				// 頂点は１６箇所固定

				var tV = new Vector3[ 4, 4 ] ;
				var tC = new Color[ 4, 4 ] ;
				var tT = new Vector2[ 4, 4 ] ;

				//-------------

				float vx0_i = vx0 + lineWidth ;
				float vx1_i = vx1 - lineWidth ;

				float vy0_i = vy0 + lineWidth ;
				float vy1_i = vy1 - lineWidth ;

				float vw = vx1 - vx0 ;
				float vh = vy1 - vy0 ;

				float tw = tx1 - tx0 ;
				float th = ty1 - ty0 ;

				float tx0_i ;
				float tx1_i ;
				float ty0_i ;
				float ty1_i ;

				if( sprite != null )
				{
					float bl = sprite.border.x ;
					float br = sprite.border.z ;
					float bb = sprite.border.y ;
					float bt = sprite.border.w ;

					if( bl >  0 )
					{
						tx0_i = tx0 + ( bl / sprite.texture.width ) ;
					}
					else
					{
						tx0_i = tx0 + ( tw * lineWidth / vw ) ;
					}

					if( br >  0 )
					{
						tx1_i = tx1 - ( br / sprite.texture.width ) ;
					}
					else
					{
						tx1_i = tx1 - ( tw * lineWidth / vw ) ;
					}

					if( bb >  0 )
					{
						ty0_i = ty0 + ( bb / sprite.texture.height ) ;
					}
					else
					{
						ty0_i = ty0 + ( th * lineWidth / vh ) ;
					}

					if( bt >  0 )
					{
						ty1_i = ty1 - ( bt / sprite.texture.height ) ;
					}
					else
					{
						ty1_i = ty1 - ( th * lineWidth / vh ) ;
					}
				}
				else
				{
					tx0_i = tx0 + ( tw * lineWidth / vw ) ;
					tx1_i = tx1 - ( tw * lineWidth / vw ) ;
					ty0_i = ty0 + ( th * lineWidth / vh ) ;
					ty1_i = ty1 - ( th * lineWidth / vh ) ;
				}

				if( m_FlipX == true )
				{
					// 横方向のスワップ
					( tx0, tx0_i, tx1_i, tx1 ) = ( tx1, tx1_i, tx0_i, tx0 ) ;
				}

				if( m_FlipY == true )
				{
					// 縦方向のスワップ
					( ty0, ty0_i, ty1_i, ty1 ) = ( ty1, ty1_i, ty0_i, ty0 ) ;
				}

				//--

				innerColor *= color ;
				outerColor *= color ;

				//---

				tV[ 0, 0 ].x = vx0   ; tV[ 0, 0 ].y = vy0   ; tV[ 0, 0 ].z = vz ;
				tC[ 0, 0 ] = outerColor ;
				tT[ 0, 0 ].x = tx0   ; tT[ 0, 0 ].y = ty0   ;

				tV[ 0, 1 ].x = vx0_i ; tV[ 0, 1 ].y = vy0   ; tV[ 0, 1 ].z = vz ;
				tC[ 0, 1 ] = outerColor ;
				tT[ 0, 1 ].x = tx0_i ; tT[ 0, 1 ].y = ty0   ;

				tV[ 0, 2 ].x = vx1_i ; tV[ 0, 2 ].y = vy0   ; tV[ 0, 2 ].z = vz ;
				tC[ 0, 2 ] = outerColor ;
				tT[ 0, 2 ].x = tx1_i ; tT[ 0, 2 ].y = ty0   ;

				tV[ 0, 3 ].x = vx1   ; tV[ 0, 3 ].y = vy0   ; tV[ 0, 3 ].z = vz ;
				tC[ 0, 3 ] = outerColor ;
				tT[ 0, 3 ].x = tx1   ; tT[ 0, 3 ].y = ty0   ;

				//---

				tV[ 1, 0 ].x = vx0   ; tV[ 1, 0 ].y = vy0_i ; tV[ 1, 0 ].z = vz ;
				tC[ 1, 0 ] = outerColor ;
				tT[ 1, 0 ].x = tx0   ; tT[ 1, 0 ].y = ty0_i ;

				tV[ 1, 1 ].x = vx0_i ; tV[ 1, 1 ].y = vy0_i ; tV[ 1, 1 ].z = vz ;
				tC[ 1, 1 ] = innerColor ;
				tT[ 1, 1 ].x = tx0_i ; tT[ 1, 1 ].y = ty0_i ;

				tV[ 1, 2 ].x = vx1_i ; tV[ 1, 2 ].y = vy0_i ; tV[ 1, 2 ].z = vz ;
				tC[ 1, 2 ] = innerColor ;
				tT[ 1, 2 ].x = tx1_i ; tT[ 1, 2 ].y = ty0_i ;

				tV[ 1, 3 ].x = vx1   ; tV[ 1, 3 ].y = vy0_i ; tV[ 1, 3 ].z = vz ;
				tC[ 1, 3 ] = outerColor ;
				tT[ 1, 3 ].x = tx1   ; tT[ 1, 3 ].y = ty0_i ;

				//---

				tV[ 2, 0 ].x = vx0   ; tV[ 2, 0 ].y = vy1_i ; tV[ 2, 0 ].z = vz ;
				tC[ 2, 0 ] = outerColor ;
				tT[ 2, 0 ].x = tx0   ; tT[ 2, 0 ].y = ty1_i ;

				tV[ 2, 1 ].x = vx0_i ; tV[ 2, 1 ].y = vy1_i ; tV[ 2, 1 ].z = vz ;
				tC[ 2, 1 ] = innerColor ;
				tT[ 2, 1 ].x = tx0_i ; tT[ 2, 1 ].y = ty1_i ;

				tV[ 2, 2 ].x = vx1_i ; tV[ 2, 2 ].y = vy1_i ; tV[ 2, 2 ].z = vz ;
				tC[ 2, 2 ] = innerColor ;
				tT[ 2, 2 ].x = tx1_i ; tT[ 2, 2 ].y = ty1_i ;

				tV[ 2, 3 ].x = vx1   ; tV[ 2, 3 ].y = vy1_i ; tV[ 2, 3 ].z = vz ;
				tC[ 2, 3 ] = outerColor ;
				tT[ 2, 3 ].x = tx1   ; tT[ 2, 3 ].y = ty1_i ;

				//---

				tV[ 3, 0 ].x = vx0   ; tV[ 3, 0 ].y = vy1   ; tV[ 3, 0 ].z = vz ;
				tC[ 3, 0 ] = outerColor ;
				tT[ 3, 0 ].x = tx0   ; tT[ 3, 0 ].y = ty1   ;

				tV[ 3, 1 ].x = vx0_i ; tV[ 3, 1 ].y = vy1   ; tV[ 3, 1 ].z = vz ;
				tC[ 3, 1 ] = outerColor ;
				tT[ 3, 1 ].x = tx0_i ; tT[ 3, 1 ].y = ty1   ;

				tV[ 3, 2 ].x = vx1_i ; tV[ 3, 2 ].y = vy1   ; tV[ 3, 2 ].z = vz ;
				tC[ 3 , 2 ] = outerColor ;
				tT[ 3, 2 ].x = tx1_i ; tT[ 3, 2 ].y = ty1   ;

				tV[ 3, 3 ].x = vx1   ; tV[ 3, 3 ].y = vy1   ; tV[ 3, 3 ].z = vz ;
				tC[ 3, 3 ] = outerColor ;
				tT[ 3, 3 ].x = tx1   ; tT[ 3, 3 ].y = ty1   ;

				//------------


				for( ly  = 0 ; ly <  4 ; ly ++ )
				{
					for( lx  = 0 ; lx <  4 ; lx ++ )
					{
						aV.Add( tV[ ly, lx ] ) ;
						aN.Add( normal ) ; 
						aC.Add( tC[ ly, lx ] ) ;
						aT.Add( tT[ ly, lx ] ) ;
					}
				}

				//---

				// インデックス
				i = 0 ;

				// 左下
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i ++ ;

				// 中下
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i ++ ;

				// 右下
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 1 ) ;
				
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i += 2 ;

				// 左中
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i += 2 ;

				// 右中
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i += 2 ;

				// 左上
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 1 ) ;
				
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i ++ ;

				// 中上
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;

				i ++ ;

				// 右上
				aI.Add( i + 0 ) ;
				aI.Add( i + 4 ) ;
				aI.Add( i + 5 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 5 ) ;
				aI.Add( i + 1 ) ;
			}
			else
			{
				// 塗りつぶし有り

				float bl = 0, br = 0, bb = 0, bt = 0 ;

				if( sprite != null )
				{
					bl = sprite.border.x ;
					br = sprite.border.z ;
					bb = sprite.border.y ;
					bt = sprite.border.w ;
				}

				bool isBorder =
					( bl >  0 || br >  0 || bb >  0 || bt >  0 ) ;

				if( m_ImageType == ImageTypes.Simple || isBorder == false )
				{
					// Simple

					int gx1 = m_GridX ;
					if( gx1 <  1 )
					{
						gx1  = 1 ;
					}
					int gx2 = gx1 + 1 ;

					int gy1 = m_GridY ;
					if( gy1 <  1 )
					{
						gy1  = 1 ;
					}
					int gy2 = gy1 + 1 ;
					
					var tV = new Vector2[ gy2, gx2 ] ;
					var tC = new Color[ gy2, gx2 ] ;
					var tT = new Vector2[ gy2, gx2 ] ;

					float vsx = sx * -0.5f ;
					float vsy = sy * -0.5f ;

					float vdx = sx / ( float )gx1 ;
					float vdy = sy / ( float )gy1 ;

					if( sprite != null )
					{
						tx0 = sprite.rect.xMin / sprite.texture.width  ;
						ty0 = sprite.rect.yMin / sprite.texture.height ;

						tx1 = sprite.rect.xMax / sprite.texture.width  ;
						ty1 = sprite.rect.yMax / sprite.texture.height ;
					}
					else
					{
						tx0 = 0 ;
						ty0 = 0 ;

						tx1 = 1 ;
						ty1 = 1 ;
					}

					if( m_FlipX == true )
					{
						// 横方向のスワップ
						( tx0, tx1 ) = ( tx1, tx0 ) ;
					}

					if( m_FlipY == true )
					{
						// 縦方向のスワップ
						( ty0, ty1 ) = ( ty1, ty0 ) ;
					}

					float tdx = ( tx1 - tx0 ) / ( float )gx1 ;
					float tdy = ( ty1 - ty0 ) / ( float )gy1 ;

					for( ly  = 0 ; ly <= gy1 ; ly ++ )
					{
						for( lx  = 0 ; lx <= gx1 ; lx ++ )
						{
							x = vsx + vdx * lx ;
							y = vsy + vdy * ly ;

							tV[ ly, lx ] = new Vector2( x, y ) ;

							x = tx0 + tdx * lx ;
							y = ty0 + tdy * ly ;

							tT[ ly, lx ] = new Vector2( x, y ) ;
						}
					}

					Color cp ;

					if( gx1 <= 1 && gy1 <= 1 )
					{
						// 単色
						cp = innerColor * color ;

						for( ly  = 0 ; ly <= gy1 ; ly ++ )
						{
							for( lx  = 0 ; lx <= gx1 ; lx ++ )
							{
								tC[ ly, lx ] = cp ;
							}
						}
					}
					else
					{
						// 内外

						float xh = sx * 0.5f ;
						float yh = sy * 0.5f ;

						float rx, ry ;
						float factor ;

						for( ly  = 0 ; ly <= gy1 ; ly ++ )
						{
							for( lx  = 0 ; lx <= gx1 ; lx ++ )
							{
								rx = Mathf.Abs( tV[ ly, lx ].x / xh ) ;
								ry = Mathf.Abs( tV[ ly, lx ].y / yh ) ;

	//							factor = ( rx + ry ) * 0.5f ;
								factor = Mathf.Max( rx, ry ) ;
								tC[ ly, lx ] = Color.Lerp( innerColor, outerColor, factor ) * color ;
							}
						}
					}

					vz = 0 ;
					Vector3 v ;

					if( tiling == false || ( gx1 <= 1 && gy1 <= 1 ) )
					{
						// タイリング無効

						// インデックス

						if( gx1 <= 1 && gy1 <= 1 )
						{
							// 分割なし

							v = tV[ 0, 0 ] ;
							aV.Add( new Vector3( v.x, v.y, vz ) ) ;
							aN.Add( normal ) ;
							aC.Add( tC[ 0, 0 ] ) ;
							aT.Add( tT[ 0, 0 ] ) ;

							v = tV[ 0, 1 ] ;
							aV.Add( new Vector3( v.x, v.y, vz ) ) ;
							aN.Add( normal ) ;
							aC.Add( tC[ 0, 1 ] ) ;
							aT.Add( tT[ 0, 1 ] ) ;

							v = tV[ 1, 0 ] ;
							aV.Add( new Vector3( v.x, v.y, vz ) ) ;
							aN.Add( normal ) ;
							aC.Add( tC[ 1, 0 ] ) ;
							aT.Add( tT[ 1, 0 ] ) ;

							v = tV[ 1, 1 ] ;
							aV.Add( new Vector3( v.x, v.y, vz ) ) ;
							aN.Add( normal ) ;
							aC.Add( tC[ 1, 1 ] ) ;
							aT.Add( tT[ 1, 1 ] ) ;

							//----------

							aI.Add( 0 ) ;
							aI.Add( 2 ) ;
							aI.Add( 3 ) ;

							aI.Add( 0 ) ;
							aI.Add( 3 ) ;
							aI.Add( 1 ) ;
						}
						else
						{
							// 分割あり

							int gxh = gx1 >> 1 ;
							int gyh = gy1 >> 1 ;

							//----------

							// 頂点情報
							for( ly  = 0 ; ly <= gy1 ; ly ++ )
							{
								for( lx  = 0 ; lx <= gx1 ; lx ++ )
								{
									v = tV[ ly, lx ] ;
									aV.Add( new Vector3( v.x, v.y, vz ) ) ;
									aN.Add( normal ) ;
									aC.Add( tC[ ly, lx ] ) ;
									aT.Add( tT[ ly, lx ] ) ;
								}
							}

							//----------

							// インデックス
							for( ly  = 0 ; ly <  gy1 ; ly ++ )
							{
								i = ly * gx2 ;

								for( lx  = 0 ; lx <  gx1 ; lx ++ )
								{
									if( ( lx <  gxh && ly <  gyh ) || ( lx >= gxh && ly >= gyh ) )
									{
										aI.Add( i ) ;
										aI.Add( i + gx2 ) ;
										aI.Add( i + gx2 + 1 ) ;

										aI.Add( i ) ;
										aI.Add( i + gx2 + 1 ) ;
										aI.Add( i + 1 ) ;
									}
									else
									{
										aI.Add( i ) ;
										aI.Add( i + gx2 ) ;
										aI.Add( i + 1 ) ;

										aI.Add( i + 1 ) ;
										aI.Add( i + gx2 ) ;
										aI.Add( i + gx2 + 1 ) ;
									}

									i ++ ;
								}
							}
						}
					}
					else
					{
						// タイリング有効
						int mx, my ;

						var tp0 = new Vector2( tx0, ty0 ) ;
						var tp1 = new Vector2( tx0, ty1 ) ;
						var tp2 = new Vector2( tx1, ty1 ) ;
						var tp3 = new Vector2( tx1, ty0 ) ;

						//-----------

						int gxh = gx1 >> 1 ;
						int gyh = gy1 >> 1 ;

						i = 0 ;
						for( ly  = 0 ; ly <  gy1 ; ly ++ )
						{
							for( lx  = 0 ; lx <  gx1 ; lx ++ )
							{
								// 0
								v = tV[ ly, lx ] ;
								aV.Add( new Vector3( v.x, v.y, vz ) ) ;
								aN.Add( normal ) ;
								aC.Add( tC[ ly, lx ] ) ;
								aT.Add( tp0 ) ;

								// 1
								my = ly + 1 ;
								v = tV[ my, lx ] ;
								aV.Add( new Vector3( v.x, v.y, vz ) ) ;
								aN.Add( normal ) ;
								aC.Add( tC[ my, lx ] ) ;
								aT.Add( tp1 ) ;

								// 2
								mx = lx + 1 ;
								my = ly + 1 ;
								v = tV[ my, mx ] ;
								aV.Add( new Vector3( v.x, v.y, vz ) ) ;
								aN.Add( normal ) ;
								aC.Add( tC[ my, mx ] ) ;
								aT.Add( tp2 ) ;

								// 3
								mx = lx + 1 ;
								v = tV[ ly, mx ] ;
								aV.Add( new Vector3( v.x, v.y, vz ) ) ;
								aN.Add( normal ) ;
								aC.Add( tC[ ly, mx ] ) ;
								aT.Add( tp3 ) ;

								//----------

								// インデックス

								if( ( lx <  gxh && ly <  gyh ) || ( lx >= gxh && ly >= gyh ) )
								{
									aI.Add( i ) ;
									aI.Add( i + 1 ) ;
									aI.Add( i + 2 ) ;

									aI.Add( i ) ;
									aI.Add( i + 2 ) ;
									aI.Add( i + 3 ) ;
								}
								else
								{
									aI.Add( i ) ;
									aI.Add( i + 1 ) ;
									aI.Add( i + 3 ) ;

									aI.Add( i + 3 ) ;
									aI.Add( i + 1 ) ;
									aI.Add( i + 2 ) ;
								}

								i += 4 ;
							}
						}
					}
				}
				else
				if( m_ImageType == ImageTypes.Sliced )
				{
					// Sliced(Sprite の設定が必須)

					if( bl <  0 )
					{
						bl  = 0 ;
					}
					if( br <  0 )
					{
						br  = 0 ;
					}
					if( bb <  0 )
					{
						bb  = 0 ;
					}
					if( bt <  0 )
					{
						bt  = 0 ;
					}

					// ※Sliced の Tiling = false の場合 Grid 分割は無効になる

					int gx1 = m_GridX ;
					if( gx1 <  1 )
					{
						gx1  = 1 ;
					}
//					int gx2 = gx1 + 1 ;

					int gy1 = m_GridY ;
					if( gy1 <  1 )
					{
						gy1  = 1 ;
					}
//					int gy2 = gy1 + 1 ;					

					// 横方向の頂点数(2～4)
					int px2 = 2 ;
					if( bl >  0 )
					{
						px2 ++ ;
					}
					if( br >  0 )
					{
						px2 ++ ;
					}

					// 縦方向の頂点数(2～4)
					int py2 = 2 ;
					if( bb >  0 )
					{
						py2 ++ ;
					}
					if( bt >  0 )
					{
						py2 ++ ;
					}

					// ボーダーサイズ(横)
					float borderSizeX = m_BorderSize.x ;
					if( borderSizeX >  sx )
					{
						borderSizeX  = sx ;
					}

					// ボーダーサイズ(縦)
					float borderSizeY = m_BorderSize.y ;
					if( borderSizeY >  sy )
					{
						borderSizeY  = sy ;
					}

					//--------------------------------------------------------

					if( m_Tiling == false || ( gx1 <= 1 && gy1 <= 1 ) )
					{
						// タイリング無効

						// ※Grid 分割は無効

						var tV = new Vector2[ py2, px2 ] ;
						var tC = new Color[ py2, px2 ] ;
						var tT = new Vector2[ py2, px2 ] ;

						//-------------------------------

						vx0 = sx * -0.5f ;
						vy0 = sy * -0.5f ;

						vx1 = sx * +0.5f ;
						vy1 = sy * +0.5f ;

						float vx0_i, vx1_i ;
						float vy0_i, vy1_i ;

						if( m_FlipX == false )
						{
							vx0_i = vx0 + ( borderSizeX * bl / sprite.texture.width  ) ;
							vx1_i = vx1 - ( borderSizeX * br / sprite.texture.width  ) ;
						}
						else
						{
							vx0_i = vx0 + ( borderSizeX * br / sprite.texture.width  ) ;
							vx1_i = vx1 - ( borderSizeX * bl / sprite.texture.width  ) ;
						}

						if( m_FlipY == false )
						{
							vy0_i = vy0 + ( borderSizeY * bb / sprite.texture.height ) ;
							vy1_i = vy1 - ( borderSizeY * bt / sprite.texture.height ) ;
						}
						else
						{
							vy0_i = vy0 + ( borderSizeY * bt / sprite.texture.height ) ;
							vy1_i = vy1 - ( borderSizeY * bb / sprite.texture.height ) ;
						}

						//-----------

						Color oc = outerColor * color ;
						Color ic = innerColor * color ;

						Color cx0, cx1 ;
						Color cy0, cy1 ;
						Color cx0_i, cx1_i ;
						Color cy0_i, cy1_i ;

						float factor ;

						//-----------

						cx0 = oc ;

						factor = ( vx0_i / vx0 ) ;
						cx0_i = Color.Lerp( ic, oc, factor ) ;

						factor = ( vx1_i / vx1 ) ;
						cx1_i = Color.Lerp( ic, oc, factor ) ;

						cx1 = oc ;

						//-----------

						cy0 = oc ;

						factor = ( vy0_i / vy0 ) ;
						cy0_i = Color.Lerp( ic, oc, factor ) ;

						factor = ( vy1_i / vy1 ) ;
						cy1_i = Color.Lerp( ic, oc, factor ) ;

						cy1 = oc ;

						//-----------

						float tx0_i, tx1_i ;
						float ty0_i, ty1_i ;

						tx0 = sprite.rect.xMin / sprite.texture.width  ;
						ty0 = sprite.rect.yMin / sprite.texture.height ;

						tx1 = sprite.rect.xMax / sprite.texture.width  ;
						ty1 = sprite.rect.yMax / sprite.texture.height ;

						tx0_i = ( sprite.rect.xMin + bl ) / sprite.texture.width  ;
						tx1_i = ( sprite.rect.xMax - br ) / sprite.texture.width  ;

						ty0_i = ( sprite.rect.yMin + bb ) / sprite.texture.height ;
						ty1_i = ( sprite.rect.yMax - bt ) / sprite.texture.height ;

						if( m_FlipX == true )
						{
							// 横方向のスワップ
							( tx0,   tx1   ) = ( tx1,   tx0   ) ;
							( tx0_i, tx1_i ) = ( tx1_i, tx0_i ) ;
						}

						if( m_FlipY == true )
						{
							// 縦方向のスワップ
							( ty0,   ty1   ) = ( ty1,   ty0   ) ;
							( ty0_i, ty1_i ) = ( ty1_i, ty0_i ) ;
						}

						//-------------------------------

						int xc ;
						int yc ;

						//-----------

						xc = 0 ;
						yc = 0 ;

						tV[ yc, xc ] = new Vector2( vx0, vy0 ) ;
						tC[ yc, xc ] = cx0 * cy0 ;
						tT[ yc, xc ] = new Vector2( tx0, ty0 ) ;

						xc ++ ;

						if( bl >  0 )
						{
							tV[ yc, xc ] = new Vector2( vx0_i, vy0 ) ;
							tC[ yc, xc ] = cx0_i * cy0 ;
							tT[ yc, xc ] = new Vector2( tx0_i, ty0 ) ;

							xc ++ ;
						}

						if( br >  0 )
						{
							tV[ yc, xc ] = new Vector2( vx1_i, vy0 ) ;
							tC[ yc, xc ] = cx1_i * cy0 ;
							tT[ yc, xc ] = new Vector2( tx1_i, ty0 ) ;

							xc ++ ;
						}

						tV[ yc, xc ] = new Vector2( vx1, vy0 ) ;
						tC[ yc, xc ] = cx1 * cy0 ;
						tT[ yc, xc ] = new Vector2( tx1, ty0 ) ;

						//-----------

						if( bb >  0 )
						{
							xc = 0 ;
							yc ++ ;

							tV[ yc, xc ] = new Vector2( vx0, vy0_i ) ;
							tC[ yc, xc ] = cx0 * cy0_i ;
							tT[ yc, xc ] = new Vector2( tx0, ty0_i ) ;

							xc ++ ;

							if( bl >  0 )
							{
								tV[ yc, xc ] = new Vector2( vx0_i, vy0_i ) ;
								tC[ yc, xc ] = cx0_i * cy0_i ;
								tT[ yc, xc ] = new Vector2( tx0_i, ty0_i ) ;

								xc ++ ;
							}

							if( br >  0 )
							{
								tV[ yc, xc ] = new Vector2( vx1_i, vy0_i ) ;
								tC[ yc, xc ] = cx1_i * cy0_i ;
								tT[ yc, xc ] = new Vector2( tx1_i, ty0_i ) ;

								xc ++ ;
							}

							tV[ yc, xc ] = new Vector2( vx1, vy0_i ) ;
							tC[ yc, xc ] = cx1 * cy0_i ;
							tT[ yc, xc ] = new Vector2( tx1, ty0_i ) ;
						}

						//-----------

						if( bt >  0 )
						{
							xc = 0 ;
							yc ++ ;

							tV[ yc, xc ] = new Vector2( vx0, vy1_i ) ;
							tC[ yc, xc ] = cx0 * cy1_i ;
							tT[ yc, xc ] = new Vector2( tx0, ty1_i ) ;

							xc ++ ;

							if( bl >  0 )
							{
								tV[ yc, xc ] = new Vector2( vx0_i, vy1_i ) ;
								tC[ yc, xc ] = cx0_i * cy1_i ;
								tT[ yc, xc ] = new Vector2( tx0_i, ty1_i ) ;

								xc ++ ;
							}

							if( br >  0 )
							{
								tV[ yc, xc ] = new Vector2( vx1_i, vy1_i ) ;
								tC[ yc, xc ] = cx1_i * cy1_i ;
								tT[ yc, xc ] = new Vector2( tx1_i, ty1_i ) ;

								xc ++ ;
							}

							tV[ yc, xc ] = new Vector2( vx1, vy1_i ) ;
							tC[ yc, xc ] = cx1 * cy1_i ;
							tT[ yc, xc ] = new Vector2( tx1, ty1_i ) ;
						}

						//-----------

						xc = 0 ;
						yc ++ ;

						tV[ yc, xc ] = new Vector2( vx0, vy1 ) ;
						tC[ yc, xc ] = cx0 * cy1 ;
						tT[ yc, xc ] = new Vector2( tx0, ty1 ) ;

						xc ++ ;

						if( bl >  0 )
						{
							tV[ yc, xc ] = new Vector2( vx0_i, vy1 ) ;
							tC[ yc, xc ] = cx0_i * cy1 ;
							tT[ yc, xc ] = new Vector2( tx0_i, ty1 ) ;

							xc ++ ;
						}

						if( br >  0 )
						{
							tV[ yc, xc ] = new Vector2( vx1_i, vy1 ) ;
							tC[ yc, xc ] = cx1_i * cy1 ;
							tT[ yc, xc ] = new Vector2( tx1_i, ty1 ) ;

							xc ++ ;
						}

						tV[ yc, xc ] = new Vector2( vx1, vy1 ) ;
						tC[ yc, xc ] = cx1 * cy1 ;
						tT[ yc, xc ] = new Vector2( tx1, ty1 ) ;

						//-------------------------------
						// 格納する

						vz = 0 ;
						Vector2 v ;

						int qx, qy ;

						// インデックス
						int px1 = px2 - 1 ;
						int py1 = py2 - 1 ;

						int pxh = px1 >> 1 ;
						int pyh = py1 >> 1 ;

						bool isCenterX, isCenterY ;

						//-----------

						// 頂点情報
						for( qy  = 0 ; qy <  py2 ; qy ++ )
						{
							for( qx  = 0 ; qx <  px2 ; qx ++ )
							{
								v = tV[ qy, qx ] ;
								aV.Add( new Vector3( v.x, v.y, vz ) ) ;
								aN.Add( normal ) ;
								aC.Add( tC[ qy, qx ] ) ;
								aT.Add( tT[ qy, qx ] ) ;
							}
						}

						// インデックス
						for( qy  = 0 ; qy <  py1 ; qy ++ )
						{
							i = qy * px2 ;

							isCenterY = false ;
							if( py1 == 3 && qy == 1 )
							{
								isCenterY = true ;
							}
							else
							if( py1 == 2 && ( ( bb >  0 && qy == 1 ) || ( bb == 0 && qy == 0 ) ) )
							{
								isCenterY = true ;
							}

							for( qx  = 0 ; qx <  px1 ; qx ++ )
							{
								// 中央に相当する部分かどうか

								isCenterX = false ;
								if( px1 == 3 && qx == 1 )
								{
									isCenterX = true ;
								}
								else
								if( px1 == 2 && ( ( bl >  0 && qx == 1 ) || ( bl == 0 && qx == 0 ) ) )
								{
									isCenterX = true ;
								}

								if( m_FillCenter == true || ( isCenterX & isCenterY ) == false )
								{
									if( ( qx <  pxh && qy <  pyh ) || ( qx >= pxh && qy >= pyh ) )
									{
										aI.Add( i ) ;
										aI.Add( i + px2 ) ;
										aI.Add( i + px2 + 1 ) ;

										aI.Add( i ) ;
										aI.Add( i + px2 + 1 ) ;
										aI.Add( i + 1 ) ;
									}
									else
									{
										aI.Add( i ) ;
										aI.Add( i + px2 ) ;
										aI.Add( i + 1 ) ;

										aI.Add( i + 1 ) ;
										aI.Add( i + px2 ) ;
										aI.Add( i + px2 + 1 ) ;
									}
								}

								i ++ ;
							}
						}
					}
					else
					{
						// タイリング有効

						// ※頂点が共有できない

						var tV = new Vector2[ gy1, gx1, py2, px2 ] ;
						var tC = new Color[ gy1, gx1, py2, px2 ] ;
						var tT = new Vector2[ py2, px2 ] ;

						//-------------------------------

						float ssx = sx / gx1 ;
						float ssy = sy / gy1 ;

						float gridBorderSizeX = borderSizeX / gx1 ;
						float gridBorderSizeY = borderSizeY / gy1 ;

						float shx = ssx * 0.5f ;
						float shy = ssy * 0.5f ;

						//-------------------------------

						vx0 = sx * -0.5f ;
						vy0 = sy * -0.5f ;

						vx1 = sx * +0.5f ;
						vy1 = sy * +0.5f ;

						float gsx = vx0 + shx ;
						float gsy = vy0 + shy ;
						float gcx, gcy ;

						float vgx0,   vgx1 ;
						float vgy0,   vgy1 ;
						float vgx0_i, vgx1_i ;
						float vgy0_i, vgy1_i ;

						Color oc = outerColor * color ;
						Color ic = innerColor * color ;

						Color cgx0, cgx1 ;
						Color cgy0, cgy1 ;

						Color cgx0_i, cgx1_i ;
						Color cgy0_i, cgy1_i ;

						float factor ;

						int xc ;
						int yc ;

						for( ly = 0 ; ly <  gy1 ; ly ++ )
						{
							for( lx  = 0 ; lx <  gx1 ; lx ++ )
							{
								// グリッドの中心
								gcx = gsx + ( ssx * lx ) ;
								gcy = gsy + ( ssy * ly ) ;

								vgx0 = gcx - shx ;
								vgy0 = gcy - shy ; 

								vgx1 = gcx + shx ;
								vgy1 = gcy + shy ;

								if( m_FlipX == false )
								{
									vgx0_i = vgx0 + ( gridBorderSizeX * bl / sprite.texture.width  ) ;
									vgx1_i = vgx1 - ( gridBorderSizeX * br / sprite.texture.width  ) ;
								}
								else
								{
									vgx0_i = vgx0 + ( gridBorderSizeX * br / sprite.texture.width  ) ;
									vgx1_i = vgx1 - ( gridBorderSizeX * bl / sprite.texture.width  ) ;
								}

								if( m_FlipY == false )
								{
									vgy0_i = vgy0 + ( gridBorderSizeY * bb / sprite.texture.height ) ;
									vgy1_i = vgy1 - ( gridBorderSizeY * bt / sprite.texture.height ) ;
								}
								else
								{
									vgy0_i = vgy0 + ( gridBorderSizeY * bt / sprite.texture.height ) ;
									vgy1_i = vgy1 - ( gridBorderSizeY * bb / sprite.texture.height ) ;
								}

								//-----------

								factor = vgx0 / vx0 ;
								cgx0 = Color.Lerp( ic, oc, factor ) ;

								factor = ( vgx0_i / vx0 ) ;
								cgx0_i = Color.Lerp( ic, oc, factor ) ;

								factor = ( vgx1_i / vx1 ) ;
								cgx1_i = Color.Lerp( ic, oc, factor ) ;

								factor = vgx1 / vx1 ;
								cgx1 = Color.Lerp( ic, oc, factor ) ;

								//---------

								factor = vgy0 / vy0 ;
								cgy0 = Color.Lerp( ic, oc, factor ) ;

								factor = ( vgy0_i / vy0 ) ;
								cgy0_i = Color.Lerp( ic, oc, factor ) ;

								factor = ( vgy1_i / vy1 ) ;
								cgy1_i = Color.Lerp( ic, oc, factor ) ;

								factor = vgy1 / vy1 ;
								cgy1 = Color.Lerp( ic, oc, factor ) ;

								//-------------------------------

								xc = 0 ;
								yc = 0 ;

								tV[ ly, lx, yc, xc ] = new Vector2( vgx0, vgy0 ) ;
								tC[ ly, lx, yc, xc ] = cgx0 * cgy0 ;

								xc ++ ;

								if( bl >  0 )
								{
									tV[ ly, lx, yc, xc ] = new Vector2( vgx0_i, vgy0 ) ;
									tC[ ly, lx, yc, xc ] = cgx0_i * cgy0 ;

									xc ++ ;
								}

								if( br >  0 )
								{
									tV[ ly, lx, yc, xc ] = new Vector2( vgx1_i, vgy0 ) ;
									tC[ ly, lx, yc, xc ] = cgx1_i * cgy0 ;

									xc ++ ;
								}

								tV[ ly, lx, yc, xc ] = new Vector2( vgx1, vgy0 ) ;
								tC[ ly, lx, yc, xc ] = cgx1 * cgy0 ;

								//-----------

								if( bb >  0 )
								{
									xc = 0 ;
									yc ++ ;

									tV[ ly, lx, yc, xc ] = new Vector2( vgx0, vgy0_i ) ;
									tC[ ly, lx, yc, xc ] = cgx0 * cgy0_i ;

									xc ++ ;

									if( bl >  0 )
									{
										tV[ ly, lx, yc, xc ] = new Vector2( vgx0_i, vgy0_i ) ;
										tC[ ly, lx, yc, xc ] = cgx0_i * cgy0_i ;

										xc ++ ;
									}

									if( br >  0 )
									{
										tV[ ly, lx, yc, xc ] = new Vector2( vgx1_i, vgy0_i ) ;
										tC[ ly, lx, yc, xc ] = cgx1_i * cgy0_i ;

										xc ++ ;
									}

									tV[ ly, lx, yc, xc ] = new Vector2( vgx1, vgy0_i ) ;
									tC[ ly, lx, yc, xc ] = cgx1 * cgy0_i ;
								}

								//-----------

								if( bt >  0 )
								{
									xc = 0 ;
									yc ++ ;

									tV[ ly, lx, yc, xc ] = new Vector2( vgx0, vgy1_i ) ;
									tC[ ly, lx, yc, xc ] = cgx0 * cgy1_i ;

									xc ++ ;

									if( bl >  0 )
									{
										tV[ ly, lx, yc, xc ] = new Vector2( vgx0_i, vgy1_i ) ;
										tC[ ly, lx, yc, xc ] = cgx0_i * cgy1_i ;

										xc ++ ;
									}

									if( br >  0 )
									{
										tV[ ly, lx, yc, xc ] = new Vector2( vgx1_i, vgy1_i ) ;
										tC[ ly, lx, yc, xc ] = cgx1_i * cgy1_i ;

										xc ++ ;
									}

									tV[ ly, lx, yc, xc ] = new Vector2( vgx1, vgy1_i ) ;
									tC[ ly, lx, yc, xc ] = cgx1 * cgy1_i ;
								}

								//-----------

								xc = 0 ;
								yc ++ ;

								tV[ ly, lx, yc, xc ] = new Vector2( vgx0, vgy1 ) ;
								tC[ ly, lx, yc, xc ] = cgx0 * cgy1 ;

								xc ++ ;

								if( bl >  0 )
								{
									tV[ ly, lx, yc, xc ] = new Vector2( vgx0_i, vgy1 ) ;
									tC[ ly, lx, yc, xc ] = cgx0_i * cgy1 ;

									xc ++ ;
								}

								if( br >  0 )
								{
									tV[ ly, lx, yc, xc ] = new Vector2( vgx1_i, vgy1 ) ;
									tC[ ly, lx, yc, xc ] = cgx1_i * cgy1 ;

									xc ++ ;
								}

								tV[ ly, lx, yc, xc ] = new Vector2( vgx1, vgy1 ) ;
								tC[ ly, lx, yc, xc ] = cgx1 * cgy1 ;
							}
						}

						//-------------------------------
						// テクスチャは全グリッドで共通

						float tx0_i, tx1_i ;
						float ty0_i, ty1_i ;

						tx0 = sprite.rect.xMin / sprite.texture.width  ;
						ty0 = sprite.rect.yMin / sprite.texture.height ;

						tx1 = sprite.rect.xMax / sprite.texture.width  ;
						ty1 = sprite.rect.yMax / sprite.texture.height ;

						tx0_i = ( sprite.rect.xMin + bl ) / sprite.texture.width  ;
						tx1_i = ( sprite.rect.xMax - br ) / sprite.texture.width  ;

						ty0_i = ( sprite.rect.yMin + bb ) / sprite.texture.height ;
						ty1_i = ( sprite.rect.yMax - bt ) / sprite.texture.height ;

						if( m_FlipX == true )
						{
							// 横方向のスワップ
							( tx0,   tx1   ) = ( tx1,   tx0   ) ;
							( tx0_i, tx1_i ) = ( tx1_i, tx0_i ) ;
						}

						if( m_FlipY == true )
						{
							// 縦方向のスワップ
							( ty0,   ty1   ) = ( ty1,   ty0   ) ;
							( ty0_i, ty1_i ) = ( ty1_i, ty0_i ) ;
						}

						//-------------------------------

						xc = 0 ;
						yc = 0 ;

						tT[ yc, xc ] = new Vector2( tx0, ty0 ) ;

						xc ++ ;

						if( bl >  0 )
						{
							tT[ yc, xc ] = new Vector2( tx0_i, ty0 ) ;

							xc ++ ;
						}

						if( br >  0 )
						{
							tT[ yc, xc ] = new Vector2( tx1_i, ty0 ) ;

							xc ++ ;
						}

						tT[ yc, xc ] = new Vector2( tx1, ty0 ) ;

						//-----------

						if( bb >  0 )
						{
							xc = 0 ;
							yc ++ ;

							tT[ yc, xc ] = new Vector2( tx0, ty0_i ) ;

							xc ++ ;

							if( bl >  0 )
							{
								tT[ yc, xc ] = new Vector2( tx0_i, ty0_i ) ;

								xc ++ ;
							}

							if( br >  0 )
							{
								tT[ yc, xc ] = new Vector2( tx1_i, ty0_i ) ;

								xc ++ ;
							}

							tT[ yc, xc ] = new Vector2( tx1, ty0_i ) ;
						}

						//-----------

						if( bt >  0 )
						{
							xc = 0 ;
							yc ++ ;

							tT[ yc, xc ] = new Vector2( tx0, ty1_i ) ;

							xc ++ ;

							if( bl >  0 )
							{
								tT[ yc, xc ] = new Vector2( tx0_i, ty1_i ) ;

								xc ++ ;
							}

							if( br >  0 )
							{
								tT[ yc, xc ] = new Vector2( tx1_i, ty1_i ) ;

								xc ++ ;
							}

							tT[ yc, xc ] = new Vector2( tx1, ty1_i ) ;
						}

						//-----------

						xc = 0 ;
						yc ++ ;

						tT[ yc, xc ] = new Vector2( tx0, ty1 ) ;

						xc ++ ;

						if( bl >  0 )
						{
							tT[ yc, xc ] = new Vector2( tx0_i, ty1 ) ;

							xc ++ ;
						}

						if( br >  0 )
						{
							tT[ yc, xc ] = new Vector2( tx1_i, ty1 ) ;

							xc ++ ;
						}

						tT[ yc, xc ] = new Vector2( tx1, ty1 ) ;

						//-------------------------------
						// 格納する

						vz = 0 ;
						Vector2 v ;

						int qx, qy ;

						// インデックス
						int px1 = px2 - 1 ;
						int py1 = py2 - 1 ;

						int gxh = gx1 >> 1 ;
						int gyh = gy1 >> 1 ;

						bool isCenterX, isCenterY ;

						//-----------

						int o = 0 ;

						for( ly  = 0 ; ly <  gy1 ; ly ++ )
						{
							for( lx  = 0 ; lx <  gx1 ; lx ++ )
							{
								// 頂点情報
								for( qy  = 0 ; qy <  py2 ; qy ++ )
								{
									for( qx  = 0 ; qx <  px2 ; qx ++ )
									{
										v = tV[ ly, lx, qy, qx ] ;
										aV.Add( new Vector3( v.x, v.y, vz ) ) ;
										aN.Add( normal ) ;
										aC.Add( tC[ ly, lx, qy, qx ] ) ;
										aT.Add( tT[ qy, qx ] ) ;
									}
								}

								// インデックス
								for( qy  = 0 ; qy <  py1 ; qy ++ )
								{
									i = o + qy * px2 ;

									isCenterY = false ;
									if( py1 == 3 && qy == 1 )
									{
										isCenterY = true ;
									}
									else
									if( py1 == 2 && ( ( bb >  0 && qy == 1 ) || ( bb == 0 && qy == 0 ) ) )
									{
										isCenterY = true ;
									}

									for( qx  = 0 ; qx <  px1 ; qx ++ )
									{
										// 中央に相当する部分かどうか

										isCenterX = false ;
										if( px1 == 3 && qx == 1 )
										{
											isCenterX = true ;
										}
										else
										if( px1 == 2 && ( ( bl >  0 && qx == 1 ) || ( bl == 0 && qx == 0 ) ) )
										{
											isCenterX = true ;
										}

										if( m_FillCenter == true || ( isCenterX & isCenterY ) == false )
										{
											if( ( lx <  gxh && ly <  gyh ) || ( lx >= gxh && ly >= gyh ) )
											{
												aI.Add( i ) ;
												aI.Add( i + px2 ) ;
												aI.Add( i + px2 + 1 ) ;

												aI.Add( i ) ;
												aI.Add( i + px2 + 1 ) ;
												aI.Add( i + 1 ) ;
											}
											else
											{
												aI.Add( i ) ;
												aI.Add( i + px2 ) ;
												aI.Add( i + 1 ) ;

												aI.Add( i + 1 ) ;
												aI.Add( i + px2 ) ;
												aI.Add( i + px2 + 1 ) ;
											}
										}

										i ++ ;
									}
								}

								o += ( px2 * py2 ) ;
							}
						}
					}
				}
			}

			//-----------------------------------------------------------

			Build( "Rectangle", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//-------------------------------------------------------------------------

		// 円を生成する
		private void CreateCircle()
		{
			CreateCircle
			(
				m_Offset.x, m_Offset.y,
				m_Size.x, m_Size.y,
				m_Sprite,
				m_BasisColor, m_InnerColor, m_OuterColor,
				m_LineWidth,
				m_Split,
				m_DecalType,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// 円を生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="c"></param>
		/// <param name="split"></param>
		public void CreateCircle
		(
			float px, float py,
			float sx, float sy,
			Sprite sprite,
			Color color, Color innerColor, Color outerColor,
			float lineWidth,
			int split,
			DecalTypes decalType,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset		= new Vector2( px, py ) ;
			m_Size			= new Vector2( sx, sy ) ;

			m_Sprite		= sprite ;

			m_BasisColor	= color ;
			m_InnerColor	= innerColor ;
			m_OuterColor	= outerColor ;

			m_LineWidth		= lineWidth ;
			m_Split			= split ;
			m_DecalType		= decalType ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			// 分割数
			split = 4 << split ;

			outerColor *= color ;
			innerColor *= color ;

			//------------------------------------------------------------------------------------------

			// 実際に頂点バッファを生成する

			var size = new Vector2( sx, sy ) ;

			int i, j ;
			float a ;

			Vector2 p = Vector2.zero ;

			float vrw = size.x * 0.5f ;
			float vrh = size.y * 0.5f ;
//			float vcx = px ;
//			float vcy = py ;
			
			float tfw = 0 ;
			float tfh = 0 ;

			float trw = 0 ;
			float trh = 0 ;
			float tcx = 0 ;
			float tcy = 0 ;

			if( sprite != null )
			{
				tfw = sprite.texture.width ;
				tfh = sprite.texture.height ;

				trw = sprite.rect.width  * 0.5f ;
				trh = sprite.rect.height * 0.5f ;

				tcx = sprite.rect.x + trw ;
				tcy = sprite.rect.y + trh ;
			}

			var normalVector = new Vector3(  0,  0, -1 ) ;

			//-----------------------------------------

			if( lineWidth >  0  )
			{
				// 中心の塗りつぶしは無し
			
				float r ;
			
				Vector2 po = Vector2.zero ;
				Vector2 pi ;
				
				Vector2[] aVo = new Vector2[ split ] ;
				Vector2[] aVi = new Vector2[ split ] ;
						
				Vector2[] aTo = null ;
				Vector2[] aTi = null ;

				if( sprite != null )
				{
					aTo = new Vector2[ split ] ;
					aTi = new Vector2[ split ] ;
				}

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;

					po.x = p.x * vrw ;
					po.y = p.y * vrh ;
					
					pi = po - ( p.normalized * lineWidth ) ;

					aVo[ i ] = new Vector2( po.x, po.y ) ;	// ９０度回転させてＹ符号反転
					aVi[ i ] = new Vector2( pi.x, pi.y ) ;

					if( sprite != null )
					{
						r = pi.magnitude / po.magnitude ;
						
						aTo[ i ] = new Vector2( p.x * 1 * trw + tcx, p.y * 1 * trh + tcy ) ;
						aTi[ i ] = new Vector2( p.x * r * trw + tcx, p.y * r * trh + tcy ) ;
					}
				}
				
				//-------------------------------------------------
				
				// 実際に頂点データを生成する

				int vi ;
			
				for( i  = 0 ; i <  split ; i ++ )
				{
					vi = aV.Count ;
					
					j = ( i + 1 ) % split ;

					// １点目
					aV.Add( new Vector3( aVo[ i ].x, aVo[ i ].y, 0 ) ) ;
					aN.Add( normalVector ) ;
					aC.Add( outerColor ) ;

					if( sprite != null )
					{
						aT.Add( new Vector2( aTo[ i ].x / tfw, aTo[ i ].y / tfh ) ) ;
					}

					// ２点目
					aV.Add( new Vector3( aVo[ j ].x, aVo[ j ].y, 0 ) ) ;
					aN.Add( normalVector ) ;
					aC.Add( outerColor ) ;

					if( sprite != null )
					{
						aT.Add( new Vector2( aTo[ j ].x / tfw, aTo[ j ].y / tfh ) ) ;
					}

					// ３点目
					aV.Add( new Vector3( aVi[ i ].x, aVi[ i ].y, 0 ) ) ;
					aN.Add( normalVector ) ;
					aC.Add( innerColor ) ;

					if( sprite != null )
					{
						aT.Add( new Vector2( aTi[ i ].x / tfw, aTi[ i ].y / tfh ) ) ;
					}

					// ４点目
					aV.Add( new Vector3( aVi[ j ].x, aVi[ j ].y, 0 ) ) ;
					aN.Add( normalVector ) ;
					aC.Add( innerColor ) ;

					if( sprite != null )
					{
						aT.Add( new Vector2( aTi[ j ].x / tfw, aTi[ j ].y / tfh ) ) ;
					}

					//インデックス
				
					aI.Add( vi + 0 ) ;
					aI.Add( vi + 1 ) ;
					aI.Add( vi + 2 ) ;

					aI.Add( vi + 1 ) ;
					aI.Add( vi + 3 ) ;
					aI.Add( vi + 2 ) ;
				}
			}
			else
			{
				// 中心の塗りつぶしは有り
			
				Vector2[] aVf = new Vector2[ split ] ;
			
				Vector2[] aTf = null ;

				if( sprite != null && decalType == DecalTypes.Normal )
				{
					aTf = new Vector2[ split ] ;
				}

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;

					aVf[ i ] = new Vector2( p.x * vrw, p.y * vrh ) ;	// ９０度回転させてＹ符号反転

					if( sprite != null && decalType == DecalTypes.Normal )
					{
						aTf[ i ] = new Vector2( p.x * trw + tcx, p.y * trh + tcy ) ;
					}
				}
			
				//-------------------------------------------------
				
				// 実際に頂点データを生成する(２タイプから選択可能)

				int vi ;
			
				if( decalType == DecalTypes.Normal )
				{
					// テクスチャの張り方：通常

					// 中心
					aV.Add( new Vector3( 0, 0, 0 ) ) ;
					aN.Add( normalVector ) ;
					aC.Add( innerColor ) ;

					if( sprite != null )
					{
						aT.Add( new Vector2( tcx / tfw, tcy / tfh ) ) ;
					}

					//-----------

					for( i  = 0 ; i <  split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % split ;

						// １点目
						aV.Add( new Vector3( aVf[ i ].x, aVf[ i ].y, 0 ) ) ;
						aN.Add( normalVector ) ;
						aC.Add( outerColor ) ;

						if( sprite != null )
						{
							aT.Add( new Vector2( aTf[ i ].x / tfw, aTf[ i ].y / tfh ) ) ;
						}

						// ２点目
						aV.Add( new Vector3( aVf[ j ].x, aVf[ j ].y, 0 ) ) ;
						aN.Add( normalVector ) ;
						aC.Add( outerColor ) ;

						if( sprite != null )
						{
							aT.Add( new Vector2( aTf[ j ].x / tfw, aTf[ j ].y / tfh ) ) ;
						}

						// インデックス
						aI.Add( 0 ) ;
						aI.Add( vi + 0 ) ;
						aI.Add( vi + 1 ) ;
					}
				}
				else
				if( decalType == DecalTypes.Effect )
				{
					// テクスチャの張り方：効果

					float tx0 = 0 ;
					float ty0 = 0 ;

					float tx1 = 0 ;
					float ty1 = 0 ;

					float tx2 = 0 ;
					float ty2 = 0 ;

					if( sprite != null )
					{
						tx0 = tcx				/ tfw ;
						ty0 = sprite.rect.yMax	/ tfh ;

						tx1 = sprite.rect.xMin	/ tfw ;
						ty1 = sprite.rect.yMin	/ tfh ;

						tx2 = sprite.rect.xMax	/ tfw ;
						ty2 = sprite.rect.yMin	/ tfh ;
					}

					for( i  = 0 ; i <  split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % split ;

						// １点目
						aV.Add( new Vector3( 0, 0, 0 ) ) ;
						aN.Add( normalVector ) ;
						aC.Add( innerColor ) ;

						if( sprite != null )
						{
							aT.Add( new Vector2( tx0, ty0 ) ) ;
						}

						// ２点目
						aV.Add( new Vector3( aVf[ i ].x, aVf[ i ].y, 0 ) ) ;
						aN.Add( normalVector ) ;
						aC.Add( outerColor ) ;

						if( sprite != null )
						{
							aT.Add( new Vector2( tx1, ty1 ) ) ;
						}

						// ３点目
						aV.Add( new Vector3( aVf[ j ].x, aVf[ j ].y, 0 ) ) ;
						aN.Add( normalVector ) ;
						aC.Add( outerColor ) ;

						if( sprite != null )
						{
							aT.Add( new Vector2( tx2, ty2 ) ) ;
						}

						// インデックス
						aI.Add( vi + 0 ) ;
						aI.Add( vi + 1 ) ;
						aI.Add( vi + 2 ) ;
					}
				}
			}

			//-----------------------------------------------------------

			Build( "Circle", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コライダーの位置と大きさをメッシュと同じに合わせる
		/// </summary>
		public void AdjustCollider()
		{
			if( m_DirectionType == DirectionTypes.Z_Axis )
			{
				if( ShapeType == ShapeTypes.Rectangle )
				{
					if( m_DirectionType == DirectionTypes.Z_Axis )
					{
						if( m_Collider2D == null )
						{
							return ;
						}

						float sx = m_Size.x ;
						float sy = m_Size.y ;

						var collider2D = m_Collider2D as BoxCollider2D ;
						collider2D.offset	= new ( m_Offset.x, m_Offset.y ) ;
						collider2D.size		= new ( sx, sy ) ;
					}
				}
				else
				if( ShapeType == ShapeTypes.Circle )
				{
					if( m_DirectionType == DirectionTypes.Z_Axis )
					{
						if( m_Collider2D == null )
						{
							return ;
						}

						float sx = m_Size.x ;
						float sy = m_Size.y ;

						var collider2D = m_Collider2D as CircleCollider2D ;
						collider2D.offset	= new ( m_Offset.x, m_Offset.y ) ;

						float r = Mathf.Max( sx, sy ) ;

						collider2D.radius = r * 0.5f ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// キャッシュ
		private Collider2D	m_Collider2D = null ;

		/// <summary>
		/// Collider(ショートカット)
		/// </summary>
		public Collider2D CCollider2D
		{
			get
			{
				if( m_Collider2D == null )
				{
					gameObject.TryGetComponent<Collider2D>( out m_Collider2D ) ;
				}
				return m_Collider2D ;
			}
		}

		// キャッシュ
		private Collider	m_Collider3D = null ;

		/// <summary>
		/// Collider(ショートカット)
		/// </summary>
		public Collider		CCollider3D
		{
			get
			{
				if( m_Collider3D == null )
				{
					gameObject.TryGetComponent<Collider>( out m_Collider3D ) ;
				}
				return m_Collider3D ;
			}
		}

		/// <summary>
		/// コライダーが存在するかの判定を行う
		/// </summary>
		public bool IsCollider
		{
			get
			{
				return ( CCollider2D != null ) || ( CCollider3D != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddCollider() ;
				}
				else
				{
					RemoveCollider() ;
				}
			}
		}

		/// <summary>
		/// Collider の追加
		/// </summary>
		public void AddCollider()
		{
			if( CCollider2D != null || CCollider3D != null )
			{
				return ;
			}

			//--------------

			if( ShapeType == ShapeTypes.Rectangle )
			{
				if( m_DirectionType == DirectionTypes.Z_Axis )
				{
					m_Collider2D = gameObject.AddComponent<BoxCollider2D>() ;

					if( TryGetComponent<Rigidbody2D>( out _ ) == false )
					{
						var rigidbody2d = gameObject.AddComponent<Rigidbody2D>() ;
						rigidbody2d.gravityScale = 0 ;
					}
				}
				else
				{
					m_Collider3D = gameObject.AddComponent<MeshCollider>() ;

					if( TryGetComponent<Rigidbody>( out _ ) == false )
					{
						var rigidbody3d = gameObject.AddComponent<Rigidbody>() ;
						rigidbody3d.useGravity = true ;
					}
				}
			}
			else
			if( ShapeType == ShapeTypes.Circle )
			{
				if( m_DirectionType == DirectionTypes.Z_Axis )
				{
					m_Collider2D = gameObject.AddComponent<CircleCollider2D>() ;

					if( TryGetComponent<Rigidbody2D>( out _ ) == false )
					{
						var rigidbody2d = gameObject.AddComponent<Rigidbody2D>() ;
						rigidbody2d.gravityScale = 0 ;
					}
				}
				else
				{
					m_Collider3D = gameObject.AddComponent<MeshCollider>() ;

					if( TryGetComponent<Rigidbody>( out _ ) == false )
					{
						var rigidbody3d = gameObject.AddComponent<Rigidbody>() ;
						rigidbody3d.useGravity = true ;
					}
				}
			}

			//--------------

			m_IsColliderDirty = true ;
		}

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
			if( TryGetComponent<Rigidbody>( out var rigidbody3d ) == true )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( rigidbody3d ) ;
				}
				else
				{
					Destroy( rigidbody3d ) ;
				}
			}

			if( m_Collider3D != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Collider3D ) ;
				}
				else
				{
					Destroy( m_Collider3D ) ;
				}
	
				m_Collider3D = null ;
			}

			//----------------------------------

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

			if( m_Collider2D != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Collider2D ) ;
				}
				else
				{
					Destroy( m_Collider2D ) ;
				}
	
				m_Collider2D = null ;
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

