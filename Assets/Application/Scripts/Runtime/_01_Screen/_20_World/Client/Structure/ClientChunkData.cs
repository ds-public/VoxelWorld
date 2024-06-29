using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

// 参考
// https://minecraft-ja.gamepedia.com/%E3%83%81%E3%83%A3%E3%83%B3%E3%82%AF
// https://minecraft-ja.gamepedia.com/Chunk%E3%83%95%E3%82%A9%E3%83%BC%E3%83%9E%E3%83%83%E3%83%88

using DSW.World ;

namespace DSW.World
{
	/// <summary>
	/// チャンクデータの管理クラス
	/// </summary>
	public class ClientChunkData
	{
		public int	X ;	// チャンクのＸ座標
		public int	Z ;	// チャンクのＺ座標
		public int	Y ;	// チャンクのＹ座標

		// ブロック情報
//		public short[,,]	Block = new short[ 16, 16, 16 ] ;	// x z y

		private readonly ChunkSetStreamData	m_ChunkSetStream ;
		private readonly int				m_Offset ;

		//-----------------------------------

		/// <summary>
		/// 属するチャンクセット識別子
		/// </summary>
		public int CsId
		{
			get
			{
				return ( Z << 12 ) | X ;
			}
		}

		/// <summary>
		/// チャンク識別子
		/// </summary>
		public int CId
		{
			get
			{
				return ( Y << 24 ) | ( Z << 12 ) | X ;
			}
		}

		//-----------------------------------

		// チャンク内の表示対象となっている総ブロック数
		private int			m_SolidBlickCount ;

		// 隣接する上下左右前後６方向のチャンクの識別子
		private readonly int m_CIdX0 ;
		private readonly int m_CIdX1 ;
		private readonly int m_CIdZ0 ;
		private readonly int m_CIdZ1 ;
		private readonly int m_CIdY0 ;
		private readonly int m_CIdY1 ;

		/// <summary>
		/// チャンクのバウンディングボックス(視錐台による表示判定用)
		/// </summary>
		public Vector3[]	BoundingBox ;

		//-----------------------------------------------------------

		/// <summary>
		/// メッシュは存在しないが基本的な準備は済んでいるかどうか
		/// </summary>
		public	bool		  IsInitialized
		{
			get
			{
				return ( m_IsInnerInitialized == true && m_IsOuterInitialized == true ) ;
			}
		}

		private bool		m_IsInnerInitialized ;
		private bool		m_IsOuterInitialized ;


		/// <summary>
		/// メッシュも含めてチャンクは完成した状態であるかどうか
		/// </summary>
		public	bool		  IsCompleted => m_IsCompleted ;
		private	bool		m_IsCompleted ;


		//-------------------------------------------------------------------------------------------

		// ゲームオブジェクト参照
		// メッシュ

		public GameObject		Model ;

		private MeshRenderer	m_MeshRenderer ;
		private MeshFilter		m_MeshFilter ;
		private Mesh			m_Mesh ;

		private readonly List<Vector3>	m_Vertices	= new () ;
		private readonly List<Vector3>	m_Normals	= new () ;
		private	readonly List<Color32>	m_Colors	= new () ;
		private	readonly List<Vector2>	m_UVs		= new () ;
		
		private readonly List<short>	m_BlockIndices = new () ;

		//-----------------------------------------------------

		/// <summary>
		/// チャンクデータを生成する(サブスレッドから呼ばれる)
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public ClientChunkData( int x, int z, int y, ChunkSetStreamData chunkSetStream, int offset, bool isEmpty )
		{
			// チャンク座標
			X = x ;
			Z = z ;
			Y = y ;

			m_ChunkSetStream	= chunkSetStream ;
			m_Offset			= offset ;

			//----------------------------------
			// 空ではないブロック数をカウントする

			m_SolidBlickCount = 0 ;

			if( isEmpty == false )
			{
				int i ;
				for( i  =    0 ; i <  4096 ; i ++ )
				{
					if( m_ChunkSetStream.GetShort( offset ) != 0 )
					{
						m_SolidBlickCount ++ ;
					}

					offset += 2 ;
				}
			}

			//----------------------------------
			// 上下左右前後６方向のチャンク識別子

			// X-方向のチャンク
			int cxMin = WorldSettings.CHUNK_SET_X_MIN ;
			m_CIdX0 = -1 ;
			if( X >  cxMin )
			{
				m_CIdX0 = ( int )( Y << 24 ) | ( int )( Z << 12 ) | ( int )( X - 1 ) ;
			}

			// X+方向のチャンク
			int cxMax = WorldSettings.CHUNK_SET_X_MAX ;
			m_CIdX1 = -1 ;
			if( X <  cxMax )
			{
				m_CIdX1 = ( int )( Y << 24 ) | ( int )( Z << 12 ) | ( int )( X + 1 ) ;
			}

			// Z-方向のチャンク
			int czMin = WorldSettings.CHUNK_SET_Z_MIN ;
			m_CIdZ0 = -1 ;
			if( Z >  czMin )
			{
				m_CIdZ0 = ( int )( Y << 24 ) | ( int )( ( Z - 1 ) << 12 ) | ( int )X ;
			}

			// Z+方向のチャンク
			int czMax = WorldSettings.CHUNK_SET_Z_MAX ;
			m_CIdZ1 = -1 ;
			if( Z <  czMax )
			{
				m_CIdZ1 = ( int )( Y << 24 ) | ( int )( ( Z + 1 ) << 12 ) | ( int )X ;
			}

			// Y-方向のチャンク
			int cyMin = WorldSettings.CHUNK_Y_MIN ;
			m_CIdY0 = - 1 ;
			if( Y >  cyMin )
			{
				m_CIdY0 = ( int )( ( Y - 1 ) << 24 ) | ( int )( Z << 12 ) | ( int )X ;
			}

			// Y+方向のチャンク
			int cyMax = WorldSettings.CHUNK_Y_MAX ;
			m_CIdY1 = -1 ;
			if( Y <  cyMax )
			{
				m_CIdY1 = ( int )( ( Y + 1 ) << 24 ) | ( int )( Z << 12 ) | ( int )X ;
			}

			//-------------
			// バウンディングボックスを作る(ビューボリューム判定用)

			float x0 = X  * 16 ;
			float y0 = Y  * 16 ;
			float z0 = Z  * 16 ;
			float x1 = x0 + 16 ;
			float y1 = y0 + 16 ;
			float z1 = z0 + 16 ;

			BoundingBox = new Vector3[]
			{
				new ( x0, y0, z0 ),
				new ( x1, y0, z0 ),
				new ( x0, y1, z0 ),
				new ( x1, y1, z0 ),
				new ( x0, y0, z1 ),
				new ( x1, y0, z1 ),
				new ( x0, y1, z1 ),
				new ( x1, y1, z1 ),
			} ;

			//----------------------------------------------------------

			m_IsInnerInitialized = false ;
			m_IsOuterInitialized = false ;

			m_IsCompleted	= false ;

			//------------------------------------------------------------------------------------------
			// 内側だけフェース情報を展開する

			m_Vertices.Clear() ;
			m_Normals.Clear() ;
			m_Colors.Clear() ;
			m_UVs.Clear() ;

			m_BlockIndices.Clear() ;	// ブロック配置情報をクリアする

			AddInnerBlockFaces() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ブロックを取得する
		/// </summary>
		/// <param name="blx"></param>
		/// <param name="blz"></param>
		/// <param name="bly"></param>
		/// <returns></returns>
		public short GetBlock( int blx, int blz, int bly )
		{
			return m_ChunkSetStream.GetShort( m_Offset + ( ( ( bly << 8 ) + ( ( blz << 4 ) + blx ) ) << 1 ) ) ;
		}

		/// <summary>
		/// ブロックを設定する
		/// </summary>
		/// <param name="blx"></param>
		/// <param name="blz"></param>
		/// <param name="bly"></param>
		/// <param name="bi"></param>
		public void SetBlock( int blx, int blz, int bly, short bi )
		{
			m_ChunkSetStream.SetShort( m_Offset + ( ( ( bly << 8 ) + ( ( blz << 4 ) + blx ) ) << 1 ), bi ) ;
		}

		//-------------------------------------------------------------------------------------------
		// テクスチャユーティリティ

		private static Vector3				m_Nx0 = new ( -1,  0,  0 ) ;
		private static Vector3				m_Nx1 = new (  1,  0,  0 ) ;
		private static Vector3				m_Nz0 = new (  0,  0, -1 ) ;
		private static Vector3				m_Nz1 = new (  0,  0,  1 ) ;
		private static Vector3				m_Ny0 = new (  0, -1,  0 ) ;
		private static Vector3				m_Ny1 = new (  0,  1,  0 ) ;

		private static readonly Vector3[]	m_Nx0s = { m_Nx0, m_Nx0, m_Nx0, m_Nx0 } ;
		private static readonly Vector3[]	m_Nx1s = { m_Nx1, m_Nx1, m_Nx1, m_Nx1 } ;
		private static readonly Vector3[]	m_Nz0s = { m_Nz0, m_Nz0, m_Nz0, m_Nz0 } ;
		private static readonly Vector3[]	m_Nz1s = { m_Nz1, m_Nz1, m_Nz1, m_Nz1 } ;
		private static readonly Vector3[]	m_Ny0s = { m_Ny0, m_Ny0, m_Ny0, m_Ny0 } ;
		private static readonly Vector3[]	m_Ny1s = { m_Ny1, m_Ny1, m_Ny1, m_Ny1 } ;

		private static Color32				m_White = new ( 255, 255, 255, 255 ) ;
		private static Color32				m_Green = new (   0, 255,  63, 255 ) ;

		private static readonly Color32[]	m_Whites = { m_White, m_White, m_White, m_White } ;
		private static readonly Color32[]	m_Greens = { m_Green, m_Green, m_Green, m_Green } ;

		/// <summary>
		/// このメッシュを作る事が可能か判定する
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public bool CanCreate(  WorldClient owner )
		{
			var activeChunks = owner.GetActiveChunks() ;

			// 注意：メッシュの作り直しコストが発生してしまうので実際は６方向全てにチャンクが存在するチャンクしかメッシユ化しない
			bool canCreate = true ;


			int xMin = WorldSettings.CHUNK_SET_X_MIN + 1 ;
			int xMax = WorldSettings.CHUNK_SET_X_MAX - 1 ;
			int zMin = WorldSettings.CHUNK_SET_Z_MIN + 1 ;
			int zMax = WorldSettings.CHUNK_SET_Z_MAX - 1 ;

			// 横方向に隣接するいずれかのチャンクが存在しないチャンクは表示対象外となる(実際はこの判定は必要なくなる)
			if( X >= xMin && X <= xMax && Z >= zMin && Z <= zMax )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ４方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==    0 && Z ==    0 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==    1 && Z ==    0 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==    0 && Z ==    1 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==    1 && Z ==    1 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==   0 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( X ==   1 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( Z ==   0 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}
			else
			if( Z ==   1 )
			{
				if
				(
					activeChunks.ContainsKey( m_CIdX0 ) == false ||
					activeChunks.ContainsKey( m_CIdX1 ) == false ||
					activeChunks.ContainsKey( m_CIdZ0 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					canCreate = false ;
				}
			}

			return canCreate ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メッシュモデルを更新する
		/// </summary>
		/// <param name="parent"></param>
		public bool RefreshModel( WorldClient owner, Transform parent, bool ignoreCheckCanCreate )
		{
			// 事前にチェックしている場合は処理を省く
			if( ignoreCheckCanCreate == false )
			{
				// 隣接するチャンクが展開されているか確認する
				if( CanCreate( owner ) == false )
				{
					// まだメッシュの生成が可能になっていないチャンク(隣接する周囲にロードされていないチャンクが存在する)
					m_IsCompleted = false ;	// 隣接するチャンクが再び全て揃った際に作り直しが必要になる
					return false ;
				}
			}

			//----------------------------------

			if( m_IsCompleted == true )
			{
				// 既にメッシュを生成済みなので無視する(６方向全てにチャンクが存在し表示対象チャンクだが一切のポリゴンが存在しない場合はメッシュを生成しないのでモデルの有無で処理済みを判定してはならない)
				return false ;
			}

			//-------------------------

			if( m_IsInnerInitialized == false )
			{
				// 内側のフェース情報を展開する
				AddInnerBlockFaces() ;
			}

			if( m_IsOuterInitialized == false )
			{
				// 外側のフェース情報を展開する
				AddOuterBlockfaces( owner ) ;
			}

			//----------------------------------

			// メッシュモデルを更新(生成・破棄)する
			UpdateModel( owner, parent ) ;
			
			//----------------------------------

			return true ;
		}

		// Advanced Mesh API
		// 参考
		// https://bluebirdofoz.hatenablog.com/entry/2020/08/02/223528
		// https://bluebirdofoz.hatenablog.com/entry/2020/08/06/225928
		// https://bluebirdofoz.hatenablog.com/entry/2020/08/07/065108

		/// <summary>
		/// メッシュモデルを更新する
		/// </summary>
		public void UpdateModel( WorldClient owner, Transform parent )
		{
			if( m_Vertices.Count >  0 )
			{
				// モデルが必要

				if( m_Mesh == null )
				{
					// モデルを生成
					CreateModel( owner, parent ) ;
				}
				else
				{
					// モデルを更新
					m_Mesh.Clear() ;
				}

				//---------------------------------------------------------

				// 頂点数に応じてインデックスのビット数に適切なものを設定する
				if( m_Vertices.Count >= 65535 )
				{
					m_Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 ;	// 頂点数の最大値を増やす
				}
				else
				{
					m_Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16 ;
				}

				//---------------------------------------------------------

				// メッシュに必要情報を設定する
				m_Mesh.SetVertices( m_Vertices ) ;
				m_Mesh.SetNormals( m_Normals ) ;
				m_Mesh.SetColors( m_Colors ) ;
				m_Mesh.SetUVs( 0, m_UVs ) ;

				int i, l = m_Vertices.Count ;

	//			Debug.LogWarning( "頂点数:" + l ) ;

				int[] indices = new int[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					indices[ i ] = i ;
				}
				m_Mesh.SetIndices( indices, MeshTopology.Quads, 0 ) ;	// 四角ポリゴン
			}
			else
			{
				// モデルは不要

				if( m_Mesh != null )
				{
					// モデルを破棄
					DeleteModel() ;
				}
			}

			//----------------------------------

			// このチャンクは完成済み
			m_IsCompleted = true ;
		}

		// モデルを生成する
		private  void CreateModel( WorldClient owner, Transform parent )
		{
			m_Mesh = new Mesh() ;

			Model = new GameObject( "Chunk[" + X + "," + Z + "," + Y + "]" ) ;
			m_MeshRenderer = Model.AddComponent<MeshRenderer>() ;
			m_MeshRenderer.materials = new Material[]{ owner.BlockMaterial } ;	// ロードしたマテリアルは明示的な破棄は不要
			m_MeshFilter = Model.AddComponent<MeshFilter>() ;
			m_MeshFilter.mesh = m_Mesh ;

			Model.transform.SetParent( parent, false ) ;
			Model.transform.localPosition = new Vector3( X * 16, Y * 16, Z * 16 ) ;
		}

		/// <summary>
		/// メッシュモデルを完全に破棄する
		/// </summary>
		public void CleanupModel()
		{
			m_IsCompleted			= false ;
			m_IsOuterInitialized	= false ;
			m_IsInnerInitialized	= false ;

			//----------------------------------

			DeleteModel() ;
		}

		// モデルを破棄する
		private void DeleteModel()
		{
			if( m_MeshFilter != null )
			{
				m_MeshFilter.mesh = null ;
				m_MeshFilter = null ;
			}

			if( m_MeshRenderer != null )
			{
				m_MeshRenderer = null ;
			}

			if( Model != null )
			{
				GameObject.DestroyImmediate( Model ) ;
				Model	= null ;
			}

			if( m_Mesh != null )
			{
				GameObject.DestroyImmediate( m_Mesh ) ;
				m_Mesh	= null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ブロックをセットしつつフェースを更新する
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="blx"></param>
		/// <param name="blz"></param>
		/// <param name="bly"></param>
		/// <param name="bi"></param>
		public void SetBlock( WorldClient owner, int blx, int blz, int bly, short bi )
		{
			// 変更前の値
			short bio = GetBlock( blx, blz, bly ) ;

			SetBlock( blx, blz, bly, bi ) ;

			// 変更後の値との比較
			if( bio == 0 && bi != 0 )
			{
				// 表示ブロック数増加
				m_SolidBlickCount ++ ;
			}
			else
			if( bio != 0 && bi == 0 )
			{
				// 表示ブロック数減少
				m_SolidBlickCount -- ;
			}

			//--------------

			UpdateBlockFaces( owner, blx, blz, bly ) ;
		}

		/// <summary>
		/// ブロックを更新する(ブロックの種別を設定した後に呼び出すこと)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		public void UpdateBlockFaces( WorldClient owner, int blx, int blz, int bly )
		{
			if( m_IsInnerInitialized == false || m_IsOuterInitialized == false )
			{
				// 指定ブロックのみのフェース情報を更新できるようになるための条件が整っていない
				return ;
			}

			RemoveBlockFaces( blx, blz, bly ) ;
			AddBlockFaces( owner, blx, blz, bly ) ;

			// チャンクをダーティ状態にする
			m_IsCompleted = false ;
		}

		/// <summary>
		/// 内側のブロック群のフェース情報を追加する
		/// </summary>
		private void AddInnerBlockFaces()
		{
			if( m_SolidBlickCount == 0 )
			{
				m_IsInnerInitialized = true ;
				return ;
			}

			//----------------------------------

			int blx, blz, bly ;

			// 全ブロックの表示される面から頂点・法線・発色・ＵＶを設定する
			for( bly  =  1 ; bly <= 14 ; bly ++ )
			{
				for( blz  =  1 ; blz <= 14 ; blz ++ )
				{
					for( blx  =  1 ; blx <= 14 ; blx ++ )
					{
						AddBlockFacesForInner( blx, blz, bly ) ;
					}
				}
			}

			m_IsInnerInitialized = true ;
		}

		/// <summary>
		/// 外側のブロック群のフェース情報を追加する
		/// </summary>
		private void AddOuterBlockfaces( WorldClient owner )
		{
			int blx, blz, bly ;

			// 全ブロックの表示される面から頂点・法線・発色・ＵＶを設定する
			for( bly  =  0 ; bly <= 15 ; bly ++ )
			{
				for( blz  =  0 ; blz <= 15 ; blz ++ )
				{
					for( blx  =  0 ; blx <= 15 ; blx ++ )
					{
						if( blx ==  0 || blx == 15 || blz ==  0 || blz == 15 || bly ==  0 || bly == 15 )
						{
							AddBlockFaces( owner, blx, blz, bly ) ;
						}
					}
				}
			}

			m_IsOuterInitialized = true ;
		}

		//-----------------------------------------------------------

		// １つのブロックの全ての面を追加する(内側用)
		private void AddBlockFacesForInner( int blx, int blz, int bly )
		{
			short bc = GetBlock( blx, blz, bly ) ;
			if( bc == 0 )
			{
				return ;	// 無し
			}

			short bmi = ( short )( ( bly << 8 ) | ( blz << 4 ) | blx ) ;

			short upperBlock ;

			// 上方向のブロック(現状は土ブロックの繋がり判別のためのみ利用)
			upperBlock = GetBlock( blx, blz, bly + 1 ) ;

			// 軽い処理
			if( GetBlock( blx - 1, blz, bly ) == 0 )
			{
				// X-面の追加
				AddFaceX0( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
			if( GetBlock( blx + 1, blz, bly ) == 0 )
			{
				// X-面の追加
				AddFaceX1( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
			if( GetBlock( blx, blz - 1, bly ) == 0 )
			{
				// Z-面の追加
				AddFaceZ0( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
			if( GetBlock( blx, blz + 1, bly ) == 0 )
			{
				// Z+面の追加
				AddFaceZ1( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
			if( GetBlock( blx, blz, bly - 1 ) == 0 )
			{
				// Y-面の追加
				AddFaceY0( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
			if( GetBlock( blx, blz, bly + 1 ) == 0 )
			{
				// Y+面の追加
				AddFaceY1( blx, blz, bly, bc, upperBlock ) ;
				m_BlockIndices.Add( bmi ) ;
			}
		}

		// １つのブロックの全ての面を追加する
		private void AddBlockFaces( WorldClient owner, int blx, int blz, int bly )
		{
			short bc = GetBlock( blx, blz, bly ) ;
			if( bc == 0 )
			{
				return ;	// 無し
			}

			short bmi = ( short )( ( bly << 8 ) | ( blz << 4 ) | blx ) ;

			bool neighborEmpty ;
			int upperBlock ;

			var activeChunks = owner.GetActiveChunks() ;

			// 上方向のブロック
			if( bly == 15 )
			{
				// 上
				if( activeChunks.ContainsKey( m_CIdY1 ) == true )
				{
					// 上チャンクの一番下のブロック
					upperBlock = activeChunks[ m_CIdY1 ].GetBlock( blx, blz,  0 ) ;
				}
				else
				{
					upperBlock = 0 ;	// ブロックは無いものとみなす
				}
			}
			else
			{
				upperBlock = GetBlock( blx, blz, bly + 1 ) ;
			}

			if( blx ==  0 || blx == 15 || blz ==  0 || blz == 15 || bly ==  0 || bly == 15 )
			{
				// 重い処理

				// X-面の判定
				if( blx ==  0 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdX0 ) == true && activeChunks[ m_CIdX0 ].GetBlock( 15, blz, bly ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx - 1, blz, bly ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// X-面の追加
					AddFaceX0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}

				// X+面の判定
				if( blx == 15 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdX1 ) == true && activeChunks[ m_CIdX1 ].GetBlock(  0, blz, bly ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx + 1, blz, bly ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// X+面の追加
					AddFaceX1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}

				// Z-面の判定
				if( blz ==  0 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdZ0 ) == true && activeChunks[ m_CIdZ0 ].GetBlock( blx, 15, bly ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx, blz - 1, bly ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Z-面の追加
					AddFaceZ0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}

				// Z+面の判定
				if( blz == 15 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdZ1 ) == true && activeChunks[ m_CIdZ1 ].GetBlock( blx,  0, bly ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx, blz + 1, bly ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Z+面の追加
					AddFaceZ1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}

				// Y-面の判定
				if( bly ==  0 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdY0 ) == true && activeChunks[ m_CIdY0 ].GetBlock( blx, blz, 15 ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx, blz, bly - 1 ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Y-面の追加
					AddFaceY0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}

				// Y+面の判定
				if( bly == 15 )
				{
					neighborEmpty = !( activeChunks.ContainsKey( m_CIdY1 ) == true && activeChunks[ m_CIdY1 ].GetBlock( blx, blz,  0 ) != 0 ) ;
				}
				else
				{
					neighborEmpty =	( GetBlock( blx, blz, bly + 1 ) == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Y+面の追加
					AddFaceY1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
			}
			else
			{
				// 軽い処理
				if( GetBlock( blx - 1, blz, bly ) == 0 )
				{
					// X-面の追加
					AddFaceX0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
				if( GetBlock( blx + 1, blz, bly ) == 0 )
				{
					// X-面の追加
					AddFaceX1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
				if( GetBlock( blx, blz - 1, bly ) == 0 )
				{
					// Z-面の追加
					AddFaceZ0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
				if( GetBlock( blx, blz + 1, bly ) == 0 )
				{
					// Z+面の追加
					AddFaceZ1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
				if( GetBlock( blx, blz, bly - 1 ) == 0 )
				{
					// Y-面の追加
					AddFaceY0( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
				if( GetBlock( blx, blz, bly + 1 ) == 0 )
				{
					// Y+面の追加
					AddFaceY1( blx, blz, bly, bc, upperBlock ) ;
					m_BlockIndices.Add( bmi ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// X-面の追加
		private void AddFaceX0( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx, bly,     blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx, bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx, bly + 1, blz     ) ) ;
			m_Vertices.Add( new Vector3( blx, bly,     blz     ) ) ;

			m_Normals.AddRange( m_Nx0s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// X+面の追加
		private void AddFaceX1( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz + 1 ) ) ;

			m_Normals.AddRange( m_Nx1s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// Y-面の追加
		private void AddFaceY0( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx,     bly,     blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx,     bly,     blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz + 1 ) ) ;

			m_Normals.AddRange( m_Ny0s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 1, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 1, upperBlock ) ) ;
		}

		// Y+面の追加
		private void AddFaceY1( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx,     bly + 1, blz     ) ) ;
			m_Vertices.Add( new Vector3( blx,     bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz     ) ) ;

			m_Normals.AddRange( m_Ny1s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 0, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 0, upperBlock ) ) ;
		}

		// Z-面の追加
		private void AddFaceZ0( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx,     bly,     blz     ) ) ;
			m_Vertices.Add( new Vector3( blx,     bly + 1, blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz     ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz     ) ) ;

			m_Normals.AddRange( m_Nz0s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// Z+面の追加
		private void AddFaceZ1( int blx, int blz, int bly, int ti, int upperBlock )
		{
			m_Vertices.Add( new Vector3( blx + 1, bly,     blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx + 1, bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx,     bly + 1, blz + 1 ) ) ;
			m_Vertices.Add( new Vector3( blx,     bly,     blz + 1 ) ) ;

			m_Normals.AddRange( m_Nz1s ) ;
			m_Colors.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			m_UVs.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		//---------------

		// ブロックの配色を取得する
		private Color32[] GetBlockVC( int index, int direction, int upperBlock )
		{
			if( index == 1 )
			{
				// 土の例外処理
				if( direction == 0 )
				{
					// 横
					if( upperBlock == 0 )
					{
						// 上も空
						return m_Greens ;
					}
				}
			}

			return m_Whites ;
		}

		// 上・下・横・横
		private readonly int[,] m_BlockToTexture = new int[,]
		{
			{   0,   0,   0,   0 },	//  00番(無)
			{ 240, 242, 243, 243 },	//  01番(土)　上・下・横:1・横:2
			{ 226, 226, 226, 226 },	//  02番(砂)　上・下・横:1・横:2
			{ 227, 227, 241, 241 },	//  03番(石)　上・下・横:1・横:2
			{ 229, 229, 228, 228 },	//  04番(木)　上・下・横:1・横:2
			{ 233, 233, 233, 233 },	//  05番(緑)　上・下・横:1・横:2
			{ 232, 232, 232, 232 },	//  06番(氷)　上・下・横:1・横:2
			{ 231, 231, 231, 231 },	//  07番(金)　上・下・横:1・横:2
			{ 246, 246, 198, 198 },	//  08番(ブ)　上・下・横:1・横:2
			{ 210, 210, 210, 210 },	//  09番(炭)　上・下・横:1・横:2
			{ 192, 192, 192, 192 },	//  10番(吸)　上・下・横:1・横:2
		} ;

		// ブロックのＵＶを取得する
		private Vector2[] GetBlockUV( int index, int direction, int upperBlock )
		{
			if( index == 1 )
			{
				// 土の例外処理
				if( direction == 2 )
				{
					// 横
					if( upperBlock == 1 )
					{
						// 上も土
						direction = 1 ;	// 横のテクスチャを下のテクスチャにする
					}
				}
			}

			index = m_BlockToTexture[ index, direction ] ;

			Vector2[] uv = new Vector2[ 4 ] ;

			float x = index & 0x0F ;
			float y = index / 0x10 ;

			float x0 =   x       / 16 ;
			float x1 = ( x + 1 ) / 16 ;
			float y0 =   y       / 16 ;
			float y1 = ( y + 1 ) / 16 ;

			uv[ 0 ] = new Vector2( x0, y0 ) ;
			uv[ 1 ] = new Vector2( x0, y1 ) ;
			uv[ 2 ] = new Vector2( x1, y1 ) ;
			uv[ 3 ] = new Vector2( x1, y0 ) ;

			return uv ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// チャンク内の外側のブロック群に関連するフェース情報を全て削除する
		/// </summary>
		public void RemoveOuterBlockFaces()
		{
			if( m_IsOuterInitialized == false )
			{
				// 外側のフェース情報はまだ生成されていない
				return ;
			}

			//----------------------------------

			int blx, blz, bly ;

			for( bly  =  0 ; bly <= 15 ; bly ++ )
			{
				for( blz  =  0 ; blz <= 15 ; blz ++ )
				{
					for( blx  =  0 ; blx <= 15 ; blx ++ )
					{
						if( blx ==  0 || blx == 15 || blz ==  0 || blz == 15 || bly ==  0 || bly == 15 )
						{
							RemoveBlockFaces( blx, blz, bly ) ;
						}
					}
				}
			}

			m_IsOuterInitialized = false ;
		}

		/// <summary>
		/// チャンク内のブロックを削除する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		public bool RemoveBlockFaces( int blx, int blz, int bly )
		{
			if( m_BlockIndices.Count == 0 )
			{
				return false ;	// ブロックは１つも存在しない
			}

			short bo = ( short )( ( bly << 8 ) | ( blz << 4 ) | blx ) ;

			int offset = m_BlockIndices.IndexOf( bo ) ;
			if( offset <  0 )
			{
				// この位置にブロックは存在しない
				return false ;
			}

			// オフセット位置からのこのブロックに関係する頂点数を取得する
			int length = 1 ;
			int index ;
			for( index  = offset + 1 ; index <  m_BlockIndices.Count ; index ++ )
			{
				if( m_BlockIndices[ index ] != bo )
				{
					break ;
				}
				length ++ ;
			}

			// offset から length 分の情報を削除する
			m_BlockIndices.RemoveRange( offset, length ) ;

			// offset と length は面単位なので点単位にするには値を４倍にする必要がある
			offset *= 4 ;
			length *= 4 ;

			m_Vertices.RemoveRange( offset, length ) ;
			m_Normals.RemoveRange( offset, length ) ;
			m_Colors.RemoveRange( offset, length ) ;
			m_UVs.RemoveRange( offset, length ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 入力されたワールド座標からチャンクの中心のワールド座標までの距離を取得する(３次元)
		/// </summary>
		/// <param name="px"></param>
		/// <param name="pz"></param>
		/// <param name="py"></param>
		/// <returns></returns>
		public float GetDistance( float px, float pz, float py )
		{
			// チャンクの中心のワールド座標
			float cx = ( X * 16.0f ) + 8.0f ;
			float cz = ( Z * 16.0f ) + 8.0f ;
			float cy = ( Y * 16.0f ) + 8.0f ;

			float dx = cx - px ;
			float dz = cz - pz ;
			float dy = cy - py ;

			return Mathf.Sqrt( dx * dx + dz * dz + dy * dy ) ;
		}

		/// <summary>
		/// 入力されたワールド座標からチャンクの中心のワールド座標までの距離を取得する(２次元)
		/// </summary>
		/// <param name="px"></param>
		/// <param name="pz"></param>
		/// <param name="py"></param>
		/// <returns></returns>
		public float GetDistance( float px, float pz )
		{
			// チャンクの中心のワールド座標
			float cx = ( X * 16.0f ) + 8.0f ;
			float cz = ( Z * 16.0f ) + 8.0f ;

			float dx = cx - px ;
			float dz = cz - pz ;

			return Mathf.Sqrt( dx * dx + dz * dz ) ;
		}
	}
}

