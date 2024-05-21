using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace MeshHelper
{
	/// <summary>
	/// ３Ｄメッシュ Version 2024/05/21
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( MeshRenderer ) )]
	[RequireComponent( typeof( MeshFilter ) )]
	public class SoftMesh3D : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/MeshHelper/SoftMesh3D", false, 22 )]	// ポップアップメニューから
//		[MenuItem( "MeshHelper/Add a SoftMesh3D" )]					// メニューから
		public static void CreateSoftMesh3D()
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

			Undo.RecordObject( go, "Add a child SoftMesh3D" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SoftMesh3D" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SoftMesh3D>() ;
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

		// スプライトからテクスチャを更新する
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
		public bool SetSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					Sprite = sprite ;

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

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// メッシュ形状
		/// </summary>
		public enum ShapeTypes
		{
			Cube		= 1,
			Sphere		= 2,
			Capsule		= 3,
			Cylinder	= 4,
			Cone		= 5,
		}

		[SerializeField][HideInInspector]
		protected ShapeTypes m_ShapeType = ShapeTypes.Cube ;

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
					AdjustTexture() ;
					m_IsMeshDirty = true ;

					if( CCollider3D != null )
					{
						RemoveCollider() ;
						AddCollider() ;
					}

					if( CRigidbody3D != null )
					{
						RemoveRigidbody() ;
						AddRigidbody() ;
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
		protected Vector3 m_Offset = Vector3.zero ;

		public    Vector3   Offset
		{
			get
			{
				return m_Offset ;
			}
			set
			{
				if( m_Offset.Equals( value ) == false )
				{
					m_Offset		= value ;
					m_IsMeshDirty	= true ;
				}
			}
		}	

		/// <summary>
		/// サイズ
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Vector3 m_Size = Vector3.one ;

		public    Vector3   Size
		{
			get
			{
				return m_Size ;
			}
			set
			{
				if( m_Size.Equals( value ) == false )
				{
					m_Size			= value ;
					m_IsMeshDirty	= true ;
				}
			}
		}	

		//-----------------------------------------------------------

		/// <summary>
		/// 分割数
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected int m_Split = 0 ;

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
		/// 頂点カラー
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Color m_VertexColor = Color.white ;

		public    Color  VertexColor
		{
			get
			{
				return m_VertexColor ;
			}
			set
			{
				if
				(
					m_VertexColor.r != value.r ||
					m_VertexColor.g != value.g ||
					m_VertexColor.b != value.b ||
					m_VertexColor.a != value.a
				)
				{
					m_VertexColor.r  = value.r ;
					m_VertexColor.g  = value.g ;
					m_VertexColor.b  = value.b ;
					m_VertexColor.a  = value.a ;

					m_IsMeshDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メッシュ形状がカプセル・シリンダーの場合の方向
		/// </summary>
		public enum DirectionTypes
		{
			X_Axis = 0,
			Y_Axis = 1,
			Z_Axis = 2,
		}

		[ SerializeField ][ HideInInspector ]
		protected  DirectionTypes m_DirectionType = DirectionTypes.Y_Axis ;

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
					m_DirectionType	 = value ;
					m_IsMeshDirty = true ;

					if( CCollider3D != null )
					{
						RemoveCollider() ;
						AddCollider() ;
					}

					if( CRigidbody3D != null )
					{
						RemoveRigidbody() ;
						AddRigidbody() ;
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

					Refresh() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// 形状ごとの固有(上書)スプライト

		[SerializeField][HideInInspector]
		protected bool m_IsSpriteOverwrite ;

		public bool IsSpriteOverwrite
		{
			get
			{
				return m_IsSpriteOverwrite ;
			}
			set
			{
				if( m_IsSpriteOverwrite != value )
				{
					m_IsSpriteOverwrite  = value ;
					AdjustTexture() ;
					m_IsMeshDirty = true ;
				}
			}
		}

		// 適切なテクスチャを設定する
		private void AdjustTexture()
		{
			if( m_IsSpriteOverwrite == false )
			{
				UpdateTexture( m_Sprite ) ;
			}
			else
			{
				if( m_ShapeType == ShapeTypes.Cube )
				{
					Sprite[] sprites = { m_SpriteF1, m_SpriteF2, m_SpriteF3, m_SpriteF4, m_SpriteF5, m_SpriteF6 } ;
					foreach( var sprite in sprites )
					{
						if( sprite != null && sprite.texture != m_Texture )
						{
							UpdateTexture( sprite ) ;
							break ;
						}
					}
				}
				else
				if( m_ShapeType == ShapeTypes.Sphere )
				{
					Sprite[] sprites = { m_SpriteM } ;
					foreach( var sprite in sprites )
					{
						if( sprite != null && sprite.texture != m_Texture )
						{
							UpdateTexture( sprite ) ;
							break ;
						}
					}
				}
				else
				if( m_ShapeType == ShapeTypes.Capsule || m_ShapeType == ShapeTypes.Cylinder )
				{
					Sprite[] sprites = { m_SpriteT, m_SpriteM, m_SpriteB } ;
					foreach( var sprite in sprites )
					{
						if( sprite != null && sprite.texture != m_Texture )
						{
							UpdateTexture( sprite ) ;
							break ;
						}
					}
				}
				else
				if( m_ShapeType == ShapeTypes.Cone )
				{
					Sprite[] sprites = { m_SpriteT, m_SpriteB } ;
					foreach( var sprite in sprites )
					{
						if( sprite != null && sprite.texture != m_Texture )
						{
							UpdateTexture( sprite ) ;
							break ;
						}
					}
				}
			}
		}

		//-----------------------------------------------------------
		// Cube

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF1 ;

		public Sprite SpriteF1
		{
			get
			{
				return m_SpriteF1 ;
			}
			set
			{
				if( m_SpriteF1 != value )
				{
					m_SpriteF1  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF2 ;

		public Sprite SpriteF2
		{
			get
			{
				return m_SpriteF2 ;
			}
			set
			{
				if( m_SpriteF2 != value )
				{
					m_SpriteF2  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF3 ;

		public Sprite SpriteF3
		{
			get
			{
				return m_SpriteF3 ;
			}
			set
			{
				if( m_SpriteF3 != value )
				{
					m_SpriteF3  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF4 ;

		public Sprite SpriteF4
		{
			get
			{
				return m_SpriteF4 ;
			}
			set
			{
				if( m_SpriteF4 != value )
				{
					m_SpriteF4  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF5 ;

		public Sprite SpriteF5
		{
			get
			{
				return m_SpriteF5 ;
			}
			set
			{
				if( m_SpriteF5 != value )
				{
					m_SpriteF5  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteF6 ;

		public Sprite SpriteF6
		{
			get
			{
				return m_SpriteF6 ;
			}
			set
			{
				if( m_SpriteF6 != value )
				{
					m_SpriteF6  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		//-----------------------------------------------------------
		// Sphere Capsule Cylinder

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteT ;

		public Sprite SpriteT
		{
			get
			{
				return m_SpriteT ;
			}
			set
			{
				if( m_SpriteT != value )
				{
					m_SpriteT  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteM ;

		public Sprite SpriteM
		{
			get
			{
				return m_SpriteM ;
			}
			set
			{
				if( m_SpriteM != value )
				{
					m_SpriteM  = value ;
					UpdateTexture( value ) ;
					m_IsMeshDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected Sprite m_SpriteB ;

		public Sprite SpriteB
		{
			get
			{
				return m_SpriteB ;
			}
			set
			{
				if( m_SpriteB != value )
				{
					m_SpriteB  = value ;
					UpdateTexture( value ) ;
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
					m_ColliderAdjustment  = value ;

					if( m_ColliderAdjustment == true )
					{
						m_IsColliderDirty = true ;
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
			TryGetComponent<MeshRenderer>( out m_MeshRenderer ) ;

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
				// メッシュの更新が必要
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

		//-------------------------------------------------------------------------

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
		public bool ActiveeSelf
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
		public void SetSize( float x, float y, float z )
		{
			Size = new Vector3( x, y, z ) ;
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

		//-------------------------------------------------------------------------
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

			//--------------

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
					case ShapeTypes.Cube		: CreateCube()		; break ;
					case ShapeTypes.Sphere		: CreateSphere()	; break ;
					case ShapeTypes.Capsule		: CreateCapsule()	; break ;
					case ShapeTypes.Cylinder	: CreateCylinder()	; break ;
					case ShapeTypes.Cone		: CreateCone()		; break ;
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
				m_Material = Resources.Load<Material>( "MeshHelper/Materials/3D/Default3D" ) ;
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

			int i, l ;

			Vector3 v ;
			Vector3 n ;

			float vs, ns ;

			if( directionType == DirectionTypes.Y_Axis )
			{
				if( isDirectionInverse == true )
				{
					l = aV.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						v.x = - v.x ;
						v.y = - v.y ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						n.x = - n.x ;
						n.y = - n.y ;

						aN[ i ] = n ;
					}
				}
			}
			else
			if( directionType == DirectionTypes.X_Axis )
			{
				// ＸとＹを入れ替え
				l = aV.Length ;

				if( isDirectionInverse == false )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  = - v.x ;
						v.x =   v.y ;
						v.y =   vs ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  = - n.x ;
						n.x =   n.y ;
						n.y =   ns ;

						aN[ i ] = n ;
					}
				}
				else
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  = - v.x ;
						v.x =   v.y ;
						v.y =   vs ;

						v.x = - v.x ;
						v.y = - v.y ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  = - n.x ;
						n.x =   n.y ;
						n.y =   ns ;

						n.x = - n.x ;
						n.y = - n.y ;

						aN[ i ] = n ;
					}
				}
			}
			else
			if( directionType == DirectionTypes.Z_Axis )
			{
				// ＹとＺを入れ替え
				l = aV.Length ;

				if( isDirectionInverse == false )
				{
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
					for( i  = 0 ; i <  l ; i ++ )
					{
						v = aV[ i ] ;

						vs  =   v.y ;
						v.y = - v.z ;
						v.z =   vs ;

						v.y = - v.y ;
						v.z = - v.z ;

						aV[ i ] = v ;

						n = aN[ i ] ;

						ns  =   n.y ;
						n.y = - n.z ;
						n.z =   ns ;

						n.y = - n.y ;
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

			if( aN == null )
			{
				m_Mesh.RecalculateNormals() ;
			}
			m_Mesh.RecalculateBounds() ;
		}

		//--------------------------------------------------------------------------------------------

		// キューブ型のメッシュを生成する
		private void CreateCube()
		{
			CreateCube
			(
				m_Offset.x, m_Offset.y, m_Offset.z,
				m_Size.x, m_Size.y, m_Size.z,
				m_VertexColor,
				m_Split,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// キューブを生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="sz"></param>
		/// <param name="uv"></param>
		public void CreateCube
		(
			float px, float py, float pz,
			float sx, float sy, float sz,
			Color color,
			int split,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset				= new Vector3( px, py, pz ) ;
			m_Size					= new Vector3( sx, sy, sz ) ;

			m_VertexColor			= color ;
			m_Split					= split ;

			m_DirectionType			= directionType ;
			m_IsDirectionInverse	= isDirectionInverse ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			float xMin = - sx * 0.5f, xMax = sx * 0.5f, yMin = - sy * 0.5f, yMax = sy * 0.5f, zMin = - sz * 0.5f, zMax = sz * 0.5f ;
			int i = 0 ;

			// 頂点
			Vector3[,] tV = new Vector3[ 6, 4 ]
			{
				{ new Vector3( xMin, yMin, zMin ), new Vector3( xMin, yMax, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMin, zMin ) },
				{ new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMax, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMax, zMin ) },
				{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMin, zMin ) },
				{ new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMin, zMax ) },
				{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMin, zMin ), new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMin, zMax ) },
				{ new Vector3( xMax, yMin, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMin, zMax ) },
			} ;

			// 法線
			Vector3[] tN = new Vector3[]
			{
				new (  0,  0, -1 ),
				new (  0,  1,  0 ),
				new ( -1,  0,  0 ),
				new (  1,  0,  0 ),
				new (  0, -1,  0 ),
				new (  0,  0,  1 ),
			} ;

			int l = tV.GetLength( 0 ) ;

			float tx0, ty0, tx1, ty1 ;

			Sprite[] faceSprites = { m_SpriteF1, m_SpriteF2, m_SpriteF3, m_SpriteF4, m_SpriteF5, m_SpriteF6 } ;
			Sprite sprite ;

			//----------------------------------

			// ６面を処理する
			int f ;
			for( f  = 0 ; f <  l ; f ++ )
			{
				if( m_IsSpriteOverwrite == false )
				{
					sprite = m_Sprite ;
				}
				else
				{
					sprite = faceSprites[ f ] ;
				}

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

				//-------------

				// 0
				aV.Add( tV[ f, 0 ] ) ;
				aN.Add( tN[ f ] ) ; 
				aC.Add( color ) ;
				aT.Add( new Vector2( tx0, ty0 ) ) ;

				// 1
				aV.Add( tV[ f, 1 ] ) ;
				aN.Add( tN[ f ] ) ; 
				aC.Add( color ) ;
				aT.Add( new Vector2( tx0, ty1 ) ) ;

				// 2
				aV.Add( tV[ f, 2 ] ) ;
				aN.Add( tN[ f ] ) ; 
				aC.Add( color ) ;
				aT.Add( new Vector2( tx1, ty1 ) ) ;

				// 3
				aV.Add( tV[ f, 3 ] ) ;
				aN.Add( tN[ f ] ) ; 
				aC.Add( color ) ;
				aT.Add( new Vector2( tx1, ty0 ) ) ;

				//-------------

				aI.Add( i + 0 ) ;
				aI.Add( i + 1 ) ;
				aI.Add( i + 2 ) ;
				
				aI.Add( i + 0 ) ;
				aI.Add( i + 2 ) ;
				aI.Add( i + 3 ) ;

				i += 4 ;
			}

			Build( "Cube", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//------------------------------------------------------------

		// スフィア型のメッシュを生成する
		private void CreateSphere()
		{
			CreateSphere
			(
				m_Offset.x, m_Offset.y, m_Offset.z,
				m_Size.x, m_Size.y, m_Size.z,
				m_VertexColor,
				m_Split,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}
	
		/// <summary>
		/// スフィアを生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="pz"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="sz"></param>
		/// <param name="color"></param>
		/// <param name="split"></param>
		public void CreateSphere
		(
			float px, float py, float pz,
			float sx, float sy, float sz,
			Color color, int split,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_DirectionType	= directionType ;

			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset				= new Vector3( px, py, pz ) ;
			m_Size					= new Vector3( sx, sy, sz ) ;

			m_VertexColor			= color ;
			m_Split					= split ;

			m_DirectionType			= directionType ;
			m_IsDirectionInverse	= isDirectionInverse ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			int o = 0, i ;

			int l ;
			int ly ;
			float a, r ;
		
			int s1  = 1 << split ;
			int s2  = 2 << split ;
			int s4  = 4 << split ;
			int s4p = s4  + 1 ;

			float hsx = sx * 0.5f ;
			float hsy = sy * 0.5f ;
			float hsz = sz * 0.5f ;

			float x, y, z ;

			float vx, vy, vz ;
			float nx, ny, nz ;
			float tx, ty ;

			float tx0, ty0, tx1, ty1 ;

			Sprite sprite ;

			//----------------------------------------------------------
			// テクスチャ

			if( m_IsSpriteOverwrite == false )
			{
				sprite = m_Sprite ;
			}
			else
			{
				sprite = m_SpriteM ;
			}

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

			float tdw = ( ( tx1 - tx0 ) / ( float )s4 ) ;
			float tdh = ( ( ty1 - ty0 ) / ( float )s2 ) ;

			//----------------------------------

			// 一番上の頂点
			vx = 0 ;
			vy =   hsy ;
			vz = 0 ;

			nx =  0 ;
			ny =  1 ;
			nz =  0 ;

			ty = ty1 ;

			for( i  = 0 ; i <  s4 ; i ++ )
			{
				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;

				tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
				aT.Add( new ( tx, ty ) ) ;

				aI.Add( o + i          ) ;
				aI.Add( o + i + 1 + s4 ) ;
				aI.Add( o + i     + s4 ) ;
			}

			o += s4 ;

			//--------------
			// 赤道から上の頂点

			for( ly  = 1 ; ly <  s1 ; ly ++ )
			{
				a = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
				r = Mathf.Sin( a ) ;
				y = Mathf.Cos( a ) ;

				vy = y * hsy ;

				ty = ty1 - ( tdh * ly ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) * r ;
					z =   Mathf.Cos( a ) * r ;

					vx = x * hsx ;
					vz = z * hsz ;

					nx = x ;
					ny = y ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i           ) ;
					aI.Add( o + i + 1       ) ;
					aI.Add( o + i     + s4p ) ;

					aI.Add( o + i + 1       ) ;
					aI.Add( o + i + 1 + s4p ) ;
					aI.Add( o + i     + s4p ) ;
				}

				o += s4p ;
			}

			//--------------
			// 赤道の頂点

			vy =   0 ;

			ty = ty1 - ( tdh * s1 ) ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
				x = - Mathf.Sin( a ) ;
				z =   Mathf.Cos( a ) ;

				vx = x * hsx ;
				vz = z * hsz ;

				nx = x ;
				ny = 0 ;
				nz = z ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( color ) ;

				tx = tx0 + tdw * i ;
				aT.Add( new ( tx, ty ) ) ;
			}

			// インデックス
			if( split == 0 )
			{
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i           ) ;
					aI.Add( o + i + 1       ) ;
					aI.Add( o + i     + s4p ) ;
				}
			}
			else
			{
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i           ) ;
					aI.Add( o + i + 1       ) ;
					aI.Add( o + i     + s4p ) ;

					aI.Add( o + i + 1       ) ;
					aI.Add( o + i + 1 + s4p ) ;
					aI.Add( o + i     + s4p ) ;
				}
			}

			o += s4p ;

			//--------------
			// 赤道から下の頂点

			for( ly  = 1 ; ly <  s1 ; ly ++ )
			{
				a = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
				r =   Mathf.Cos( a ) ;
				y = - Mathf.Sin( a ) ;

				vy = y * hsy ;

				ty = ty1 - ( tdh * ( s1 + ly ) ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) * r ;
					z =   Mathf.Cos( a ) * r ;

					vx = x * hsx ;
					vz = z * hsz ;

					nx = x ;
					ny = y ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				if( ly <  ( s1 - 1 ) )
				{
					for( i  = 0 ; i <  s4 ; i ++ )
					{
						aI.Add( o + i           ) ;
						aI.Add( o + i + 1       ) ;
						aI.Add( o + i     + s4p ) ;

						aI.Add( o + i + 1       ) ;
						aI.Add( o + i + 1 + s4p ) ;
						aI.Add( o + i     + s4p ) ;
					}
				}
				else
				{
					for( i  = 0 ; i <  s4 ; i ++ )
					{
						aI.Add( o + i           ) ;
						aI.Add( o + i + 1       ) ;
						aI.Add( o + i     + s4p ) ;
					}
				}

				o += s4p ;
			}

			//--------------
			// 一番下の頂点

			vx =  0 ;
			vy = - hsy ;
			vz =  0 ;

			nx =  0 ;
			ny = -1 ;
			nz =  0 ;

			ty = ty0 ;

			for( l  = 0 ; l <  s4 ; l ++ )
			{
				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;

				tx = tx0 + tdw * l + ( tdw * 0.5f ) ;
				aT.Add( new ( tx, ty ) ) ;
			}

			//-----------------------------------------------------------

			Build( "Sphere", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//------------------------------------------------------------

		// カプセル型のメッシュを生成する
		private void CreateCapsule()
		{
			CreateCapsule
			(
				m_Offset.x, m_Offset.y, m_Offset.z,
				m_Size.x, m_Size.y, m_Size.z,
				m_VertexColor,
				m_Split,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// カプセルを生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="pz"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="sz"></param>
		/// <param name="color"></param>
		/// <param name="split"></param>
		public void CreateCapsule
		(
			float px, float py, float pz,
			float sx, float sy, float sz,
			Color color,
			int split,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			if( sx == 0 || sy == 0 || sz == 0 )
			{
				return ;
			}

			//----------------------------------

			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset				= new ( px, py, pz ) ;
			m_Size					= new ( sx, sy, sz ) ;

			m_VertexColor			= color ;
			m_Split					= split ;

			m_DirectionType			= directionType ;
			m_IsDirectionInverse	= isDirectionInverse ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			int o = 0, p, i ;

			int l ;
			int ly ;
			float a, r ;

			float vx, vy, vz ;
			float nx, ny, nz ;

			int s1  = 1 << split ;	// 上半球と下半球の分割数(１は無し)
			int s4  = 4 << split ;
			int s4p = s4 + 1 ;

			float hsx = sx * 0.50f ;
			float hsy = sy * 0.50f ;
			float hsz = sz * 0.50f ;

			float x, y, z ;
			float tx, ty ;

			float tx0, ty0, tx1, ty1 ;

			Sprite sprite ;

			float tdw, tdh ;
			float tcx, tcy ;
			float tbx, tby ;

			//----------------------------------
			// 半球部分の比率

			float tr ;	// 上部分比率
			float mr ;	// 筒部分比率
			float br ;	// 下部分比率

			float radius = Mathf.Max( hsx, hsz ) ;

			if( hsy >  radius )
			{
				// 筒部分あり
				tr = 0.5f * radius / hsy ;
				br = tr ;
				mr = 1.0f - tr - br ;
			}
			else
			{
				// 筒部分なし
				tr = 0.5f ;
				br = 0.5f ;
				mr = 0.0f ;
			}

			//----------------------------------

			if( m_IsSpriteOverwrite == false )
			{
				// テクスチャが１枚の場合

				sprite = m_Sprite ;

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

				tdw = ( tx1 - tx0 ) / ( float )s4 ;
				tdh = ( ty1 - ty0 ) ;

				//---------------------------------

				// 一番上の頂点(その周辺のインデックスも先行設定)

				ty = ty1 ;

				for( i  = 0 ; i <  s4 ; i ++ )
				{
					vx =  0 ;
					vy =   hsy ;
					vz =  0 ;

					nx =  0 ;
					ny =  1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;

					// インデックス
					aI.Add( o + i          ) ;
					aI.Add( o + i + 1 + s4 ) ;
					aI.Add( o + i     + s4 ) ;
				}

				o += s4 ;

				//---

				// 上半球の頂点(Split = 0 であれば無視される)
				for( ly  = 1 ; ly <  s1 ; ly ++ )
				{
					a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
					r  = Mathf.Sin( a ) ;
					y  = Mathf.Cos( a ) ;

					//-

					y  = 2.0f * ( ( y * tr ) + ( mr * 0.5f ) ) ;

					ty = ty1 - ( tdh * tr * ly / s1 ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a  = 2.0f * Mathf.PI * i / s4 ;
						x  = - Mathf.Sin( a ) * r ;
						z  =   Mathf.Cos( a ) * r ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx = x ;
						ny = y ;
						nz = z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;

						tx = tx0 + tdw * i ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス
					for( i  = 0 ; i <  s4 ; i ++ )
					{
						aI.Add( o + i           ) ;
						aI.Add( o + i + 1       ) ;
						aI.Add( o + i     + s4p ) ;

						aI.Add( o + i + 1       ) ;
						aI.Add( o + i + 1 + s4p ) ;
						aI.Add( o + i     + s4p ) ;
					}

					o += s4p ;
				}

				//---------------------------------

				if( mr == 0 )
				{
					// 筒部分なし

					y  = 0.0f ;

					ty = ty1 - ( tdh * 0.5f ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a  = 2.0f * Mathf.PI * i / s4 ;
						x  = - Mathf.Sin( a ) ;
						z  =   Mathf.Cos( a ) ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx = x ;
						ny = y ;
						nz = z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;

						tx = tx0 + tdw * i ;
						aT.Add( new ( tx, ty ) ) ;
					}

					//--------------------------------

					// インデックス
					if( s1 >= 2 )
					{
						// 側面
						// 下半球に分割点がある　→　下半球の一番上の一周

						// 四角
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}
					else
					{
						// 下半球に分割点がない　→　下極点の一周

						// 三角
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					//--------------------------------

					o += s4p ;
				}
				else
				{
					// 筒部分あり

					// 筒の頂点(常に上下２点)
					for( ly  = 0 ; ly <= 1 ; ly ++ )
					{
						y = 2.0f * ( ( mr * 0.5f ) - ( mr * ly ) ) ;

						ty = ty1 - ( tdh * tr ) - ( tdh * mr * ly ) ;

						for( i  = 0 ; i <= s4 ; i ++ )
						{
							a  = 2.0f * Mathf.PI * i / s4 ;
							x  = - Mathf.Sin( a ) ;
							z  =   Mathf.Cos( a ) ;

							vx = x * hsx ;
							vy = y * hsy ;
							vz = z * hsz ;

							nx = x ;
							ny = y ;
							nz = z ;

							aV.Add( new ( vx, vy, vz ) ) ;
							aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
							aC.Add( color ) ;

							tx = tx0 + tdw * i ;
							aT.Add( new ( tx, ty ) ) ;
						}

						//--------------------------------

						// インデックス
						if( ly == 0 || s1 >= 2 )
						{
							// 側面
							// 下半球に分割点がある　→　下半球の一番上の一周

							// 四角
							for( i  = 0 ; i <  s4 ; i ++ )
							{
								aI.Add( o + i           ) ;
								aI.Add( o + i + 1       ) ;
								aI.Add( o + i     + s4p ) ;

								aI.Add( o + i + 1       ) ;
								aI.Add( o + i + 1 + s4p ) ;
								aI.Add( o + i     + s4p ) ;
							}
						}
						else
						{
							// 下半球に分割点がない　→　下極点の一周

							// 三角
							for( i  = 0 ; i <  s4 ; i ++ )
							{
								aI.Add( o + i           ) ;
								aI.Add( o + i + 1       ) ;
								aI.Add( o + i     + s4p ) ;
							}
						}

						//--------------------------------

						o += s4p ;
					}
				}

				//---------------------------------

				// 下半球の頂点(Split = 0 であれば無視される)
				for( ly  = 1 ; ly <  s1 ; ly ++ )
				{
					a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
					r  =   Mathf.Cos( a ) ;
					y  = - Mathf.Sin( a ) ;

					//-

					y  = 2.0f * ( ( y * br ) - ( mr * 0.5f ) ) ;

					ty = ty1 - ( tdh * ( tr + mr ) ) - ( tdh * br * ly / s1 ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a = 2.0f * Mathf.PI * i / s4 ;
						x = - Mathf.Sin( a ) * r ;
						z =   Mathf.Cos( a ) * r ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx = x ;
						ny = y ;
						nz = z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;

						tx = tx0 + tdw * i ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス
					if( ly <  ( s1 - 1 ) )
					{
						// 下半球の２段目～極の周囲より１つ上まで

						for( i  = 0 ; i <  s4 ; i ++ )
						{
							// 四角
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}
					else
					{
						// 下半球の極の周囲

						for( i  = 0 ; i <  s4 ; i ++ )
						{
							// 三角
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					o += s4p ;
				}

				//-------------

				// 一番下の頂点

				ty = ty0 ;

				for( l  = 0 ; l <  s4 ; l ++ )
				{
					vx =  0 ;
					vy = - hsy ;
					vz =  0 ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * l + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;
				}
			}
			else
			{
				// テクスチャが３枚の場合

				sprite = m_SpriteT ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//---------------------------------

				// 一番上の頂点

				vx =  0 ;
				vy =   hsy ;
				vz =  0 ;

				nx =  0 ;
				ny =  1 ;
				nz =  0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( color ) ;

				aT.Add( new ( tcx, tcy ) ) ;

				p = o ;
				o ++ ;	// 一番上の頂点分

				//---

				// 一番上の頂点の周辺の頂点

				a  = ( 2.0f * Mathf.PI *  90f ) / ( s1 * 360f ) ;
				r  = Mathf.Sin( a ) ;
				y  = Mathf.Cos( a ) ;

				y  = 2.0f * ( ( y * tr ) + ( mr * 0.5f ) ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a  = 2.0f * Mathf.PI * i / s4 ;
					x  = - Mathf.Sin( a ) * r ;
					z  =   Mathf.Cos( a ) * r ;

					vx = x * hsx ;
					vy = y * hsy ;
					vz = z * hsz ;

					nx = x ;
					ny = y ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					tx = tcx + ( - x * tbx / ( float )s1 ) ;
					ty = tcy + (   z * tby / ( float )s1 ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス(極の周囲)
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( p         ) ;	// 極の頂点
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
				}

				// インデックス(先行設定)
				if( s1 >= 2 )
				{
					// 上半球には分割点がある

					for( i  = 0 ; i <  s4 ; i ++ )
					{
						aI.Add( o + i           ) ;
						aI.Add( o + i + 1       ) ;
						aI.Add( o + i     + s4p ) ;

						aI.Add( o + i + 1       ) ;
						aI.Add( o + i + 1 + s4p ) ;
						aI.Add( o + i     + s4p ) ;
					}
				}

				o += s4p ;	// １頂点分多い

				//-------------

				// 上半球の頂点

				// ※上半球の最下部頂点は筒部分とＵＶが共有されないため頂点は筒部分とは別にする必要がある

				for( ly  = 2 ; ly <= s1 ; ly ++ )
				{
					a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
					r  = Mathf.Sin( a ) ;
					y  = Mathf.Cos( a ) ;

					y  = 2.0f * ( ( y * tr ) + ( mr * 0.5f ) ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a = 2.0f * Mathf.PI * i / s4 ;
						x = - Mathf.Sin( a ) * r ;
						z =   Mathf.Cos( a ) * r ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx = x ;
						ny = y ;
						nz = z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;

						tx = tcx + ( - x * tbx * ( float )ly / ( float )s1 ) ;
						ty = tcy + (   z * tby * ( float )ly / ( float )s1 ) ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス
					if( ly <  s1 )
					{
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					o += s4p ;
				}

				//---------------------------------
				// 筒部分

				if( mr >  0 )
				{
					sprite = m_SpriteM ;

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

					tdw = ( tx1 - tx0 ) / ( float )s4 ;
					tdh = ( ty1 - ty0 ) ;

					// 筒の頂点
					for( ly  = 0 ; ly <= 1 ; ly ++ )
					{
						y  = 2.0f * ( ( mr * 0.5f ) - ( mr * ly ) ) ;

						ty = ty1 - ( tdh * ly ) ;

						for( i  = 0 ; i <= s4 ; i ++ )
						{
							a  = 2.0f * Mathf.PI * i / s4 ;
							x  = - Mathf.Sin( a ) ;
							z  =   Mathf.Cos( a ) ;

							vx = x * hsx ;
							vy = y * hsy ;
							vz = z * hsz ;

							nx = x ;
							ny = y ;
							nz = z ;

							aV.Add( new ( vx, vy, vz ) ) ;
							aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
							aC.Add( color ) ;

							tx = tx0 + tdw * i ;
							aT.Add( new ( tx, ty ) ) ;
						}

						//--

						// インデックス
						if( ly == 0 )
						{
							for( i  = 0 ; i <  s4 ; i ++ )
							{
								aI.Add( o + i           ) ;
								aI.Add( o + i + 1       ) ;
								aI.Add( o + i     + s4p ) ;

								aI.Add( o + i + 1       ) ;
								aI.Add( o + i + 1 + s4p ) ;
								aI.Add( o + i     + s4p ) ;
							}
						}
						
						o += s4p ;
					}
				}

				//---------------------------------
				// 下半球

				sprite = m_SpriteB ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//-------------

				// ※下半球の最上部頂点は筒部分とＵＶが共有されないため頂点は筒部分とは別にする必要がある

				// 下半球の頂点(筒の一番下よりも下)
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
					r  =   Mathf.Cos( a ) ;
					y  = - Mathf.Sin( a ) ;

					y  = 2.0f * ( ( y * br ) - ( mr * 0.5f ) ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a = 2.0f * Mathf.PI * i / s4 ;
						x = - Mathf.Sin( a ) * r ;
						z =   Mathf.Cos( a ) * r ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx = x ;
						ny = y ;
						nz = z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;

						tx = tcx + (   x * tbx * ( float )( s1 - ly ) / ( float )s1 ) ;
						ty = tcy + ( - z * tby * ( float )( s1 - ly ) / ( float )s1 ) ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス(先行設定)
					if( ly <  ( s1 - 1 ) )
					{
						// 筒の最下部より１つ下から極の周囲より１上まで

						for( i  = 0 ; i <  s4 ; i ++ )
						{
							// 四角
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}
					else
					{
						// 極の周囲
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							// 三角
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					o += s4p ;
				}

				//-------------

				// 一番下の頂点の周辺

				// 極の頂点
				p = o + s4p ;

				ly = s1 - 1 ;

				a  = ( 2.0f * Mathf.PI * ly * 90f ) / ( s1 * 360f ) ;
				r  =   Mathf.Cos( a ) ;
				y  = - Mathf.Sin( a ) ;

				y  = 2.0f * ( ( y * br ) - ( mr * 0.5f ) ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) * r ;
					z =   Mathf.Cos( a ) * r ;

					vx = x * hsx ;
					vy = y * hsy ;
					vz = z * hsz ;

					nx = x ;
					ny = y ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					tx = tcx + (   x * tbx / ( float )s1 ) ;
					ty = tcy + ( - z * tby / ( float )s1 ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// 一周回った右端は、最初の左端と頂点・法線の情報は同じであってもＵＶの情報が異なっているため、別の頂点にする必要がある。極の周辺はその必要は無いが、筒部分の処理を簡単にするため、円周の最初と最後の頂点は別にする。

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
					aI.Add( p         ) ;	// 極の頂点
				}

				//---

				// 一番下の頂点
				vx =  0 ;
				vy = - hsy ;
				vz =  0 ;

				nx =  0 ;
				ny = -1 ;
				nz =  0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( color ) ;

				aT.Add( new ( tcx, tcy ) ) ;
			}

			//-----------------------------------------------------------

			Build( "Capsule", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//-------------------------------------------------------------------------

		// シリンダー型のメッシュを生成する
		private void CreateCylinder()
		{
			CreateCylinder
			(
				m_Offset.x, m_Offset.y, m_Offset.z,
				m_Size.x, m_Size.y, m_Size.z,
				m_VertexColor,
				m_Split,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// シリンダーを生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="pz"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="sz"></param>
		/// <param name="color"></param>
		/// <param name="split"></param>
		public void CreateCylinder
		(
			float px, float py, float pz,
			float sx, float sy, float sz,
			Color color,
			int split,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset		= new ( px, py, pz ) ;
			m_Size			= new ( sx, sy, sz ) ;

			m_VertexColor	= color ;
			m_Split			= split ;
		
			m_DirectionType			= directionType ;
			m_IsDirectionInverse	= isDirectionInverse ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			int o = 0, i, p ;

			int ly ;
			float a ;
		
			float vx, vy, vz ;
			float nx, ny, nz ;
		
			int s4  = 4 << split ;
			int s4p = s4 + 1 ;

			float hsx = sx * 0.5f ;
			float hsy = sy * 0.5f ;
			float hsz = sz * 0.5f ;

			float x, y, z ;
			float tx, ty ;

			float tx0, ty0, tx1, ty1 ;

			Sprite sprite ;

			float tdw, tdh ;
			float tcx, tcy ;
			float tbx, tby ;

			//----------------------------------

			if( m_IsSpriteOverwrite == false )
			{
				// テクスチャが１枚の場合

				sprite = m_Sprite ;

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

				tdw = ( tx1 - tx0 ) / ( float )s4 ;
				tdh = ( ty1 - ty0 ) ;

				//---------------------------------

				// 上面の中心の頂点(その周辺のインデックスも先行設定)

				ty = ty1 ;

				for( i  = 0 ; i <  s4 ; i ++ )
				{
					vx =  0 ;
					vy =   hsy ;
					vz =  0 ;

					nx =  0 ;
					ny =  1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i          ) ;
					aI.Add( o + i + 1 + s4 ) ;
					aI.Add( o + i     + s4 ) ;
				}

				o += s4 ;

				//---

				// 上面の外周の頂点(インデックスの設定は不要)　※法線方向が異なるため筒部分と頂点の共有は出来ない
				ty = ty1 - ( tdh * 0.25f ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =     hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny =  1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i ;
					aT.Add( new ( tx, ty ) ) ;
				}

				o += s4p ;

				//---------------------------------
				// 筒部分

				for( ly  = 0 ; ly <= 1 ; ly ++ )
				{
					y = 1.0f - ly * 2 ;

					ty = ty1 - ( tdh * 0.25f ) - ( tdh * 0.5f * ly ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a = 2.0f * Mathf.PI * i / s4 ;
						x = - Mathf.Sin( a ) ;
						z =   Mathf.Cos( a ) ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx =  x ;
						ny =  0 ;
						nz =  z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;
	
						tx = tx0 + ( tdw * i  ) ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス
					if( ly == 0 )
					{
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					o += s4p ;
				}

				//---------------------------------

				// 下面の外周の頂点(その中心のインデックスも先行設定)　※法線方向が異なるため筒部分と頂点の共有は出来ない

				ty = ty1 - ( tdh * 0.75f ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + ( tdw * i ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i           ) ;
					aI.Add( o + i + 1       ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
					aI.Add( o + i     + s4p ) ;	// 極の頂点
				}

				//---

				// 下面の中心の頂点(インデックスの設定は不要)
				ty = ty0 ;

				for( i  = 0 ; i <  s4 ; i ++ )
				{
					vx =  0 ;
					vy = - hsy ;
					vz =  0 ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;
				}
			}
			else
			{
				// テクスチャが３枚の場合

				sprite = m_SpriteT ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//---------------------------------

				// 上面の中心

				vx = 0 ;
				vy =   hsy ;
				vz = 0 ;

				nx = 0 ;
				ny = 1 ;
				nz = 0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;
				aT.Add( new ( tcx, tcy ) ) ;

				p = o ;
				o ++ ;	// 一番上の頂点分

				// 上面の外周
				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =     hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny =  1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					tx = tcx + ( - x * tbx ) ;
					ty = tcy + (   z * tby ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// ※一周回った右端は、最初の左端と頂点・法線の情報は同じであってもＵＶの情報が異なっているため、別の頂点にする必要がある。極の周辺はその必要は無いが、筒部分の処理を簡単にするため、円周の最初と最後の頂点は別にする。

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( p         ) ;	// 極の頂点
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
				}

				o += s4p ;	// １頂点分多い

				//---------------------------------
				// 筒部分

				sprite = m_SpriteM ;

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

				tdw = ( tx1 - tx0 ) / ( float )s4 ;
				tdh = ( ty1 - ty0 ) ;

				// 筒の頂点
				
				for( ly  = 0 ; ly <= 1 ; ly ++ )
				{
					y = 1.0f - ly * 2 ;

					ty = ty1 - ( tdh * ly ) ;

					for( i  = 0 ; i <= s4 ; i ++ )
					{
						a = 2.0f * Mathf.PI * i / s4 ;
						x = - Mathf.Sin( a ) ;
						z =   Mathf.Cos( a ) ;

						vx = x * hsx ;
						vy = y * hsy ;
						vz = z * hsz ;

						nx =  x ;
						ny =  0 ;
						nz =  z ;

						aV.Add( new ( vx, vy, vz ) ) ;
						aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
						aC.Add( color ) ;
	
						tx = tx0 + ( tdw * i  ) ;
						aT.Add( new ( tx, ty ) ) ;
					}

					// インデックス
					if( ly == 0 )
					{
						for( i  = 0 ; i <  s4 ; i ++ )
						{
							aI.Add( o + i           ) ;
							aI.Add( o + i + 1       ) ;
							aI.Add( o + i     + s4p ) ;

							aI.Add( o + i + 1       ) ;
							aI.Add( o + i + 1 + s4p ) ;
							aI.Add( o + i     + s4p ) ;
						}
					}

					o += s4p ;
				}

				//---------------------------------
				// 下面

				sprite = m_SpriteB ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//-------------

				// 下面の周囲の頂点

				p = o + s4p ;

				// ※下半球の最上部頂点は筒部分とＵＶが共有されないため頂点は筒部分とは別にする必要がある

				// 下半球の頂点(筒の一番下よりも下)
				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tcx + (   x * tbx ) ;
					ty = tcy + ( - z * tby ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
					aI.Add( p         ) ;	// 極の頂点
				}

				//---

				// 下面の中心
				vx =  0 ;
				vy = - hsy ;
				vz =  0 ;

				nx =  0 ;
				ny = -1 ;
				nz =  0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;

				aT.Add( new ( tcx, tcy ) ) ;
			}

			//-----------------------------------------------------------

			Build( "Cylinder", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//-------------------------------------------------------------------------

		// コーン型のメッシュを生成する
		private void CreateCone()
		{
			CreateCone
			(
				m_Offset.x, m_Offset.y, m_Offset.z,
				m_Size.x, m_Size.y, m_Size.z,
				m_VertexColor,
				m_Split,
				m_DirectionType,
				m_IsDirectionInverse
			) ;
		}

		/// <summary>
		/// コーンを生成する
		/// </summary>
		/// <param name="px"></param>
		/// <param name="py"></param>
		/// <param name="pz"></param>
		/// <param name="sx"></param>
		/// <param name="sy"></param>
		/// <param name="sz"></param>
		/// <param name="color"></param>
		/// <param name="split"></param>
		public void CreateCone
		(
			float px, float py, float pz,
			float sx, float sy, float sz,
			Color color,
			int split,
			DirectionTypes directionType,
			bool isDirectionInverse
		)
		{
			// 値を更新しておく(プログラムから更新生成される場合用)
			m_Offset				= new ( px, py, pz ) ;
			m_Size					= new ( sx, sy, sz ) ;

			m_VertexColor			= color ;
			m_Split					= split ;
		
			m_DirectionType			= directionType ;
			m_IsDirectionInverse	= isDirectionInverse ;

			//--------------

			var aV = new List<Vector3>() ;
			var aN = new List<Vector3>() ;
			var aC = new List<Color>() ;
			var aT = new List<Vector2>() ;
			var aI = new List<int>() ;
		
			int o = 0, i, p ;

			float a ;
		
			float vx, vy, vz ;
			float nx, ny, nz ;
		
			int s4  = 4 << split ;
			int s4p = s4 + 1 ;

			float hsx = sx * 0.5f ;
			float hsy = sy * 0.5f ;
			float hsz = sz * 0.5f ;

			float x, z ;
			float tx, ty ;

			float tx0, ty0, tx1, ty1 ;

			Sprite sprite ;

			float tdw, tdh ;
			float tcx, tcy ;
			float tbx, tby ;

			//----------------------------------

			if( m_IsSpriteOverwrite == false )
			{
				// テクスチャが１枚の場合

				sprite = m_Sprite ;

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

				tdw = ( tx1 - tx0 ) / ( float )s4 ;
				tdh = ( ty1 - ty0 ) ;

				//---------------------------------

				// 上面の中心の頂点(その周辺のインデックスも先行設定)

				ty = ty1 ;

				for( i  = 0 ; i <  s4 ; i ++ )
				{
					vx =  0 ;
					vy =   hsy ;
					vz =  0 ;

					nx = 0 ;
					ny = 1 ;
					nz = 0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i          ) ;
					aI.Add( o + i + 1 + s4 ) ;
					aI.Add( o + i     + s4 ) ;
				}

				o += s4 ;

				//---

				// 上面の外周の頂点(インデックスの設定は不要)　※法線方向が異なるため底面部分と頂点の共有は出来ない
				ty = ty1 - ( tdh * 0.5f ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx = x ;
					ny = 0 ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i ;
					aT.Add( new ( tx, ty ) ) ;
				}

				o += s4p ;

				//---------------------------------

				// 底面の外周の頂点(その中心のインデックスも先行設定)　※法線方向が異なるため筒部分と頂点の共有は出来ない

				ty = ty1 - ( tdh * 0.5f ) ;

				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + ( tdw * i ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i           ) ;
					aI.Add( o + i + 1       ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
					aI.Add( o + i     + s4p ) ;	// 極の頂点
				}

				//---

				// 下面の中心の頂点(インデックスの設定は不要)
				ty = ty0 ;

				for( i  = 0 ; i <  s4 ; i ++ )
				{
					vx =  0 ;
					vy = - hsy ;
					vz =  0 ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tx0 + tdw * i + ( tdw * 0.5f ) ;
					aT.Add( new ( tx, ty ) ) ;
				}
			}
			else
			{
				// テクスチャが２枚の場合

				sprite = m_SpriteT ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//---------------------------------

				// 上面の中心

				vx = 0 ;
				vy =   hsy ;
				vz = 0 ;

				nx = 0 ;
				ny = 1 ;
				nz = 0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;

				aT.Add( new ( tcx, tcy ) ) ;

				p = o ;
				o ++ ;	// 一番上の頂点分

				//---

				// 上面の外周
				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx = x ;
					ny = 0 ;
					nz = z ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
					aC.Add( color ) ;

					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					tx = tcx + ( - x * tbx ) ;
					ty = tcy + (   z * tby ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// ※一周回った右端は、最初の左端と頂点・法線の情報は同じであってもＵＶの情報が異なっているため、別の頂点にする必要がある。極の周辺はその必要は無いが、筒部分の処理を簡単にするため、円周の最初と最後の頂点は別にする。

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( p         ) ;	// 極の頂点
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
				}

				o += s4p ;	// １頂点分多い

				//---------------------------------
				// 底面

				sprite = m_SpriteB ;

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

				// 中心
				tcx = ( tx0 + tx1 ) * 0.5f ;
				tcy = ( ty0 + ty1 ) * 0.5f ;

				// 半径
				tbx = ( tx1 - tx0 ) * 0.5f ;
				tby = ( ty1 - ty0 ) * 0.5f ;

				//-------------

				// 下面の周囲の頂点

				p = o + s4p ;

				// ※下半球の最上部頂点は筒部分とＵＶが共有されないため頂点は筒部分とは別にする必要がある

				// 下半球の頂点(筒の一番下よりも下)
				for( i  = 0 ; i <= s4 ; i ++ )
				{
					a = 2.0f * Mathf.PI * i / s4 ;
					x = - Mathf.Sin( a ) ;
					z =   Mathf.Cos( a ) ;

					vx = x * hsx ;
					vy =   - hsy ;
					vz = z * hsz ;

					nx =  0 ;
					ny = -1 ;
					nz =  0 ;

					aV.Add( new ( vx, vy, vz ) ) ;
					aN.Add( new ( nx, ny, nz ) ) ;
					aC.Add( color ) ;

					tx = tcx + (   x * tbx ) ;
					ty = tcy + ( - z * tby ) ;
					aT.Add( new ( tx, ty ) ) ;
				}

				// インデックス
				for( i  = 0 ; i <  s4 ; i ++ )
				{
					aI.Add( o + i     ) ;
					aI.Add( o + i + 1 ) ;	// 最後の頂点は最初の頂点とは別なので % s4 する必要は無い
					aI.Add( p         ) ;	// 極の頂点
				}

				//---

				// 下面の中心
				vx =  0 ;
				vy = - hsy ;
				vz =  0 ;

				nx =  0 ;
				ny = -1 ;
				nz =  0 ;

				aV.Add( new ( vx, vy, vz ) ) ;
				aN.Add( new ( nx, ny, nz ) ) ;
				aC.Add( color ) ;

				aT.Add( new ( tcx, tcy ) ) ;
			}

			//-----------------------------------------------------------

			Build( "Cone", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray(), directionType, isDirectionInverse, m_Offset ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コライダーの位置と大きさをメッシュと同じに合わせる
		/// </summary>
		public void AdjustCollider()
		{
			if( m_Collider3D == null )
			{
				return ;
			}

			//----------------------------------

			if( m_ShapeType == ShapeTypes.Cube )
			{
				var collider = m_Collider3D as BoxCollider ;
				collider.center	= m_Offset ;

				float sx = m_Size.x ;
				float sy = m_Size.y ;
				float sz = m_Size.z ;

				if( m_DirectionType == DirectionTypes.X_Axis )
				{
					( sx, sy ) = ( sy, sx ) ;
				}
				else
				if( m_DirectionType == DirectionTypes.Z_Axis )
				{
					( sy, sz ) = ( sz, sy ) ;
				}

				collider.size	= new Vector3( sx, sy, sz ) ; 
			}
			else
			if( m_ShapeType == ShapeTypes.Sphere )
			{
				var collider = m_Collider3D as SphereCollider ;
				collider.center	= m_Offset ;

				float sx = m_Size.x ;
				float sy = m_Size.y ;
				float sz = m_Size.z ;

				if( m_DirectionType == DirectionTypes.X_Axis )
				{
					( sx, sy ) = ( sy, sx ) ;
				}
				else
				if( m_DirectionType == DirectionTypes.Z_Axis )
				{
					( sy, sz ) = ( sz, sy ) ;
				}

				float r = Mathf.Max( sx, sy, sz ) ;

				collider.radius = r * 0.5f ;
			}
			else
			if( m_ShapeType == ShapeTypes.Capsule )
			{
				var collider = m_Collider3D as CapsuleCollider  ;
				collider.center	= m_Offset ;

				float sx = m_Size.x ;
				float sy = m_Size.y ;
				float sz = m_Size.z ;

				if( m_DirectionType == DirectionTypes.X_Axis )
				{
					( sx, sy ) = ( sy, sx ) ;
				}

				float r = Mathf.Max( sx, sz ) ;

				collider.radius = r * 0.5f ;
				collider.height = sy ;
				collider.direction = ( int )DirectionType ;
			}
			else
			if( m_ShapeType == ShapeTypes.Cylinder )
			{
				// MeshCollider なので不要
			}
			else
			if( m_ShapeType == ShapeTypes.Cone )
			{
				// MeshCollider なので不要
			}
		}

		//-------------------------------------------------------------------------------------------

		// キャッシュ
		private Collider	m_Collider3D = null ;

		/// <summary>
		/// Collider(ショートカット)
		/// </summary>
		public Collider CCollider3D
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
				return ( CCollider3D != null ) ;
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
			if( CCollider3D != null )
			{
				return ;
			}

			//--------------

			if( m_ShapeType == ShapeTypes.Cube )
			{
				m_Collider3D = gameObject.AddComponent<BoxCollider>() ;
			}
			else
			if( m_ShapeType == ShapeTypes.Sphere )
			{
				m_Collider3D = gameObject.AddComponent<SphereCollider>() ;
			}
			else
			if( m_ShapeType == ShapeTypes.Capsule )
			{
				m_Collider3D = gameObject.AddComponent<CapsuleCollider>() ;
			}
			else
			{
				m_Collider3D = gameObject.AddComponent<MeshCollider>() ;
			}

			//--------------

			m_IsColliderDirty = true ;
		}

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
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
		}

		//----------

		// キャッシュ
		private Rigidbody	m_Rigidbody3D	= null ;

		/// <summary>
		/// RigidBody(ショートカット)
		/// </summary>
		public Rigidbody CRigidbody3D
		{
			get
			{
				if( m_Rigidbody3D == null )
				{
					gameObject.TryGetComponent<Rigidbody>( out m_Rigidbody3D ) ;
				}
				return m_Rigidbody3D ;
			}
		}

		/// <summary>
		/// Rigidbody が存在するかの判定を行う
		/// </summary>
		public bool IsRigidbody
		{
			get
			{
				return ( CRigidbody3D != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddRigidbody() ;
				}
				else
				{
					RemoveRigidbody() ;
				}
			}
		}

		/// <summary>
		/// Rigidbody の追加
		/// </summary>
		public void AddRigidbody()
		{
			if( CRigidbody3D != null )
			{
				return ;
			}

			//--------------

			m_Rigidbody3D = gameObject.AddComponent<Rigidbody>() ;
		}

		/// <summary>
		/// Rigidbody の削除
		/// </summary>
		public void RemoveRigidbody()
		{
			if( m_Rigidbody3D != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Rigidbody3D ) ;
				}
				else
				{
					Destroy( m_Rigidbody3D ) ;
				}
	
				m_Rigidbody3D = null ;
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

