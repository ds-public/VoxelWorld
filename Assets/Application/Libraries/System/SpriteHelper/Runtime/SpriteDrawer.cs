using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// ２Ｄメッシュ Version 2024/07/18
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( MeshRenderer ) )]
	[RequireComponent( typeof( MeshFilter ) )]
	public class SpriteDrawer : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteDrawer", false, 22 )]	// ポップアップメニューから
//		[MenuItem( "SpriteHelper/Add a SpriteDrawer" )]					// メニューから
		public static void CreateSpriteDrawer()
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

			Undo.RecordObject( go, "Add a child SpriteDrawer" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteDrawer" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteDrawer>() ;
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

			var sprites = Resources.LoadAll<Sprite>( "SpriteHelper/Textures/SpriteSet" ) ;
			if( sprites != null && sprites.Length >  0 )
			{
				Sprite = sprites[ 0 ] ;

				SetSprites( sprites ) ;
			}

			Size = new Vector2( 120, 120 ) ;
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

					m_IsTextureCoordinateDirty = true ;
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
				var sprite = GetSpriteInAtlasFromCache( spriteName ) ;
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
				var sprite = m_SpriteSet[ spriteName ] ;
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
				var sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				var sprite = m_SpriteSet[ spriteName ] ;
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
				var sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				var sprite = m_SpriteSet[ spriteName ] ;
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
				var sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				var sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		//-------------------------------------------------------------------------------------------
		// Private 系

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

		//-------------------------------------------------------------------------------------------

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

					m_IsOffsetAndSizeDirty = true ;
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

					m_IsOffsetAndSizeDirty = true ;
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

					m_IsTextureCoordinateDirty = true ;
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

					m_IsTextureCoordinateDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 基本色(頂点カラー)
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected Color m_VertexColor = Color.white ;

		/// <summary>
		/// 基本色(頂点カラー)
		/// </summary>
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

					m_IsVertexColorDirty = true ;
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
					UpdateMaterial( m_MaterialColor, m_Texture, false ) ;
				}
			}
		}

		/// <summary>
		/// 複製されたマテリアル
		/// </summary>
		public Material DuplicatedMaterial
		{
			get
			{
				if( m_DuplicatedMaterial == null )
				{
					// マテリアルの設定を更新する
					UpdateMaterial( m_MaterialColor, m_Texture, true ) ;
				}

				return m_DuplicatedMaterial ;
			}
		}

		//-----------------------------------

		// マテリアル
		[SerializeField][HideInInspector]
		private Color m_MaterialColor = Color.white ;

		/// <summary>
		/// マテリアルカラー
		/// </summary>
		public Color MaterialColor
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

		//-------------------------------------------------------------------------------------------
		// ショートカット系プロパティ

		/// <summary>
		/// ソーティングレイヤーネーム
		/// </summary>
		public string SortingLayerName
		{
			get
			{
				if( m_MeshRenderer == null )
				{
					return string.Empty ;
				}

				return m_MeshRenderer.sortingLayerName ;
			}
			set
			{
				if( m_MeshRenderer == null )
				{
					return ;
				}

				foreach( var layer in UnityEngine.SortingLayer.layers )
				{
					if( layer.name == value )
					{
						// 発見(設定可能)
						m_MeshRenderer.sortingLayerName = value ;
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
				if( m_MeshRenderer == null )
				{
					return -1 ;
				}

				string sortingLayerName = m_MeshRenderer.sortingLayerName ;

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
				if( m_MeshRenderer == null )
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

				m_MeshRenderer.sortingLayerName = sortingLayerName ;
			}
		}

		/// <summary>
		/// ソーティングレイヤー(オーダーインレイヤーと同じ)
		/// </summary>
		public int SortingOrder
		{
			get
			{
				if( m_MeshRenderer == null )
				{
					return 0 ;
				}

				return m_MeshRenderer.sortingOrder ;
			}
			set
			{
				if( m_MeshRenderer == null )
				{
					return ;
				}

				m_MeshRenderer.sortingOrder = value ;
			}
		}

		/// <summary>
		/// オーダーインレイヤー
		/// </summary>
		public int OrderInLayer
		{
			get
			{
				if( m_MeshRenderer == null )
				{
					return 0 ;
				}

				return m_MeshRenderer.sortingOrder ;
			}
			set
			{
				if( m_MeshRenderer == null )
				{
					return ;
				}

				m_MeshRenderer.sortingOrder = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// メッシュレンダラー
		private MeshRenderer	m_MeshRenderer				= null ;

		// メッシュフィルター
		private MeshFilter		m_MeshFilter				= null ;

		// メッシュ
		private Mesh			m_Mesh						= null ;

		//-----------------------------------

		// マテリアルの更新が必要かどうか
		private bool			m_IsMaterialDirty			= true ;

		// メッシュの更新が必要かどうか
		private bool			m_IsOffsetAndSizeDirty		= true ;

		[SerializeField][HideInInspector]
		private Vector3[]		m_Mesh_vertices				= new Vector3[ 4 ] ;

		// カラーの更新が必要かどうか
		private bool			m_IsVertexColorDirty		= true ;

		[SerializeField][HideInInspector]
		private Color[]			m_Mesh_colors				= new Color[ 4 ] ;

		// スプライトの更新の必要かどうか
		private bool			m_IsTextureCoordinateDirty	= true ;

		[SerializeField][HideInInspector]
		private Vector2[]		m_Mesh_uv					= new Vector2[ 4 ] ;

		//---------------------------------------------------------------

		internal void Awake()
		{
			// 複製時を想定した強制更新
			m_IsMaterialDirty			= true ;
			m_IsOffsetAndSizeDirty		= true ;
			m_IsVertexColorDirty		= true ;
			m_IsTextureCoordinateDirty	= true ;
		}

		/// <summary>
		/// 開始する際に呼び出される
		/// </summary>
		internal void Start()
		{
			if( m_MeshRenderer == null )
			{
				TryGetComponent<MeshRenderer>( out m_MeshRenderer ) ;
			}

			if( TryGetComponent<MeshFilter>( out m_MeshFilter ) == true )
			{
				m_Mesh = CreateMesh() ;

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
			if
			(
				m_IsMaterialDirty			== true ||
				m_IsOffsetAndSizeDirty		== true ||
				m_IsVertexColorDirty		== true ||
				m_IsTextureCoordinateDirty	== true
			)
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
		public void SetPosition( float x, float y )
		{
			transform.localPosition = new Vector3( x, y, transform.localPosition.z ) ;
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
		public void SetVertexColor( Color color )
		{
			VertexColor = color ;
		}

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetVertexColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color       ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			VertexColor = new Color32( r, g, b, a ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 法線
		private static readonly Vector3[] m_Normals = { new (  0,  0, -1 ), new (  0,  0, -1 ), new (  0,  0, -1 ), new (  0,  0, -1 ) } ;

		// 最初のメッシュを生成する
		private Mesh CreateMesh()
		{
			var mesh = new Mesh()
			{
				// 頂点は４つだけなので１６ビット幅で十分
				indexFormat = UnityEngine.Rendering.IndexFormat.UInt16,

				name		= name,

				vertices	= new Vector3[ 4 ],
				normals		= m_Normals,
				colors		= new Color[ 4 ],
				uv			= new Vector2[ 4 ],

				triangles	= new int[]{  0, 1, 3,  0, 3, 2  }
			} ;

			UpdateOffsetAndSize( mesh ) ;
			UpdateVertexColor( mesh ) ;
			UpdateTextureCoordinate( mesh ) ;

			mesh.RecalculateBounds() ;

			return mesh ;
		}

		// 更新する
		private void Refresh()
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
				UpdateMaterial( m_MaterialColor, m_Texture, false ) ;
			}

			//--------------

			if( m_IsOffsetAndSizeDirty == true )
			{
				UpdateOffsetAndSize( m_Mesh ) ;
			}

			if( m_IsVertexColorDirty == true )
			{
				UpdateVertexColor( m_Mesh ) ;
			}

			if( m_IsTextureCoordinateDirty == true )
			{
				UpdateTextureCoordinate( m_Mesh ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// マテリアルの設定を更新する
		private void UpdateMaterial( Color color, Texture texture, bool isForceMaterialDuplication )
		{
			// Awake や Start より前に呼ばれる可能性がある
			if( m_MeshRenderer == null )
			{
				TryGetComponent<MeshRenderer>( out m_MeshRenderer ) ;
			}

			if( m_MeshRenderer == null )
			{
				Debug.LogError( "MeshRenderer is null." ) ;
				return ;
			}

			//----------------------------------------------------------

			if( m_Material == null )
			{
				m_Material = Resources.Load<Material>( "SpriteHelper/Materials/DefaultSprite" ) ;
			}

			if( m_Material != null )
			{
				if( m_DuplicatedMaterial == null )
				{
					if( m_Material.color.Equals( color ) == false || ( texture != null && m_Material.mainTexture != texture ) || isForceMaterialDuplication == true )
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
				else
				{
					// 既に複製が生成済み

					m_DuplicatedMaterial.color			= color ;
					m_DuplicatedMaterial.mainTexture	= texture ;

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

			//----------------------------------

			m_IsMaterialDirty			= false ;
		}

		// オフセットとサイズをメッシュに設定する
		private void UpdateOffsetAndSize( Mesh mesh )
		{
			float hx = m_Size.x * 0.5f ;
			float hy = m_Size.y * 0.5f ;

			float xMin = -hx + m_Offset.x ;
			float yMin = -hy + m_Offset.y ;
			float xMax = +hx + m_Offset.x ;
			float yMax = +hy + m_Offset.y ;

			//----------------------------------

			m_Mesh_vertices[ 0 ] = new ( xMin, yMin, 0 ) ;
			m_Mesh_vertices[ 1 ] = new ( xMax, yMin, 0 ) ;
			m_Mesh_vertices[ 2 ] = new ( xMin, yMax, 0 ) ;
			m_Mesh_vertices[ 3 ] = new ( xMax, yMax, 0 ) ;

			mesh.vertices = m_Mesh_vertices ;

			//----------------------------------

			m_IsOffsetAndSizeDirty		= false ;
		}

		// 頂点カラーをメッシュに設定する
		private void UpdateVertexColor( Mesh mesh )
		{
			m_Mesh_colors[ 0 ] = m_VertexColor ;
			m_Mesh_colors[ 1 ] = m_VertexColor ;
			m_Mesh_colors[ 2 ] = m_VertexColor ;
			m_Mesh_colors[ 3 ] = m_VertexColor ;

			mesh.colors = m_Mesh_colors ;

			//----------------------------------

			m_IsVertexColorDirty		= false ;
		}

		// テクスチャＵＶをメッシュに設定する
		private void UpdateTextureCoordinate( Mesh mesh )
		{
			float xMin, xMax ;
			float yMin, yMax ;

			if( m_Sprite != null )
			{
				xMin = m_Sprite.rect.xMin / m_Sprite.texture.width  ;
				yMin = m_Sprite.rect.yMin / m_Sprite.texture.height ;

				xMax = m_Sprite.rect.xMax / m_Sprite.texture.width  ;
				yMax = m_Sprite.rect.yMax / m_Sprite.texture.height ;
			}
			else
			{
				xMin = 0 ;
				yMin = 0 ;

				xMax = 1 ;
				yMax = 1 ;
			}

			if( m_FlipX == true )
			{
				// 横方向のスワップ
				( xMin, xMax ) = ( xMax, xMin ) ;
			}

			if( m_FlipY == true )
			{
				// 縦方向のスワップ
				( yMin, yMax ) = ( yMax, yMin ) ;
			}

			//----------------------------------

			m_Mesh_uv[ 0 ] = new ( xMin, yMin ) ;
			m_Mesh_uv[ 1 ] = new ( xMax, yMin ) ;
			m_Mesh_uv[ 2 ] = new ( xMin, yMax ) ;
			m_Mesh_uv[ 3 ] = new ( xMax, yMax ) ;

			mesh.uv = m_Mesh_uv ;

			//----------------------------------

			m_IsTextureCoordinateDirty	= false ;
		}
	}
}

