using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

// 参考
// https://minecraft-ja.gamepedia.com/%E3%83%81%E3%83%A3%E3%83%B3%E3%82%AF
// https://minecraft-ja.gamepedia.com/Chunk%E3%83%95%E3%82%A9%E3%83%BC%E3%83%9E%E3%83%83%E3%83%88

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// チャンクデータの管理クラス
	/// </summary>
	public class ActiveChunkData
	{
		public int	X ;	// チャンクのＸ座標
		public int	Z ;	// チャンクのＺ座標
		public int	Y ;	// チャンクのＹ座標

		private readonly long m_CidX0 ;
		private readonly long m_CidX1 ;
		private readonly long m_CidZ0 ;
		private readonly long m_CidZ1 ;
		private readonly long m_CidY0 ;
		private readonly long m_CidY1 ;

		public bool			IsCompleted ;

		// ブロック情報
		public short[,,]	Block = new short[ 16, 16, 16 ] ;	// x z y

		// ゲームオブジェクト参照
		// メッシュ

		private readonly WorldClient m_Owner ;



		public GameObject	Model ;

		private MeshRenderer	m_MeshRenderer ;
		private MeshFilter		m_MeshFilter ;
		private Mesh			m_Mesh ;

		private readonly List<short>	m_BlockIndices = new List<short>() ;

		private List<Vector3>	m_Vertices	= new List<Vector3>() ;
		private List<Vector3>	m_Normals	= new List<Vector3>() ;
		private	List<Color32>	m_Colors	= new List<Color32>() ;
		private	List<Vector2>	m_UVs		= new List<Vector2>() ;



		public Vector3[] BoundingBox ;

		//-----------------------------------------------------

		/// <summary>
		/// チャンクデータを生成する
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public ActiveChunkData( WorldClient owner, int x, int z, int y, ChunkData chunkData )
		{
			m_Owner = owner ;

			// チャンク座標
			X = x ;
			Z = z ;
			Y = y ;

			// X-方向のチャンク
			m_CidX0 = -1 ;
			if( X >     0 )
			{
				m_CidX0 = ( long )( Y << 24 ) | ( long )( Z << 12 ) | ( long )( X - 1 ) ;
			}

			// X+方向のチャンク
			m_CidX1 = -1 ;
			if( X <  4095 )
			{
				m_CidX1 = ( long )( Y << 24 ) | ( long )( Z << 12 ) | ( long )( X + 1 ) ;
			}

			// Z-方向のチャンク
			m_CidZ0 = -1 ;
			if(  Z >     0 )
			{
				m_CidZ0 = ( long )( Y << 24 ) | ( long )( ( Z - 1 ) << 12 ) | ( long )X ;
			}

			// Z+方向のチャンク
			m_CidZ1 = -1 ;
			if( m_CidZ1 <  4095 )
			{
				m_CidZ1 = ( long )( Y << 24 ) | ( long )( ( Z + 1 ) << 12 ) | ( long )X ;
			}

			// Y-方向のチャンク
			m_CidY0 = - 1 ;
			if( Y >  0 )
			{
				m_CidY0 = ( long )( ( Y - 1 ) << 24 ) | ( long )( Z << 12 ) | ( long )X ;
			}

			// Y+方向のチャンク
			m_CidY1 = -1 ;
			if( Y <  63 )
			{
				m_CidY1 = ( long )( ( Y + 1 ) << 24 ) | ( long )( Z << 12 ) | ( long )X ;
			}

			//----------------------------------

			if( chunkData != null )
			{
				//-------------------------
				// 中身のあるチャンク

				int bx, by, bz ;
				for( bz  = 0 ; bz <= 15 ; bz ++ )
				{
					for( bx  = 0 ; bx <= 15 ; bx ++ )
					{
						for( by  = 0 ; by <= 15 ; by ++ )
						{
							Block[ bx, bz, by ] = chunkData.Block[ bx, bz, by ] ;
						}
					}
				}
			}
		}

		private static Vector3				m_Nx0 = new Vector3( -1,  0,  0 ) ;
		private static Vector3				m_Nx1 = new Vector3(  1,  0,  0 ) ;
		private static Vector3				m_Nz0 = new Vector3(  0,  0, -1 ) ;
		private static Vector3				m_Nz1 = new Vector3(  0,  0,  1 ) ;
		private static Vector3				m_Ny0 = new Vector3(  0, -1,  0 ) ;
		private static Vector3				m_Ny1 = new Vector3(  0,  1,  0 ) ;

		private static readonly Vector3[]	m_Nx0s = { m_Nx0, m_Nx0, m_Nx0, m_Nx0 } ;
		private static readonly Vector3[]	m_Nx1s = { m_Nx1, m_Nx1, m_Nx1, m_Nx1 } ;
		private static readonly Vector3[]	m_Nz0s = { m_Nz0, m_Nz0, m_Nz0, m_Nz0 } ;
		private static readonly Vector3[]	m_Nz1s = { m_Nz1, m_Nz1, m_Nz1, m_Nz1 } ;
		private static readonly Vector3[]	m_Ny0s = { m_Ny0, m_Ny0, m_Ny0, m_Ny0 } ;
		private static readonly Vector3[]	m_Ny1s = { m_Ny1, m_Ny1, m_Ny1, m_Ny1 } ;

		private static Color32				m_White = new Color32( 255, 255, 255, 255 ) ;
		private static Color32				m_Green = new Color32(   0, 255,  63, 255 ) ;

		private static readonly Color32[]	m_Whites = { m_White, m_White, m_White, m_White } ;
		private static readonly Color32[]	m_Greens = { m_Green, m_Green, m_Green, m_Green } ;


		/// <summary>
		/// メッシュを更新する
		/// </summary>
		/// <param name="parent"></param>
		public bool CreateModel( Transform parent )
		{
			if( IsCompleted == true )
			{
				// 既にメッシュを生成済みなので無視する(６方向全てにチャンクが存在し表示対象チャンクだが一切のポリゴンが存在しない場合はメッシュを生成しないのでモデルの有無で処理済みを判定してはならない)
				return false ;
			}

			//-------------------------

			// 注意：メッシュの作り直しコストが発生してしまうので実際は６方向全てにチャンクが存在するチャンクしかメッシユ化しない
			bool enableChunk = true ;
			
			// 横方向に隣接するいずれかのチャンクが存在しないチャンクは表示対象外となる(実際はこの判定は必要なくなる)
			if( X >=    1 && X <= 4094 && Z >=    1 && Z <= 4094 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ４方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==    0 && Z ==    0 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==    1 && Z ==    0 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==    0 && Z ==    1 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==    1 && Z ==    1 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false
				)
				{
					// ２方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==   0 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( X ==   1 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( Z ==   0 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}
			else
			if( Z ==   1 )
			{
				if
				(
					m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == false ||
					m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == false
				)
				{
					// ３方向いずれかのチャンクがロードされていないチャンクはモデルを生成しない(チャンク側面の作り直しを行わないため)
					enableChunk = false ;
				}
			}

			if( enableChunk == false )
			{
				return false ;	// まだメッシュの生成が可能になっていないチャンク
			}

			//-------------------------------------------------

			m_BlockIndices.Clear() ;	// ブロック配置情報をクリアする

			m_Vertices.Clear() ;
			m_Normals.Clear() ;
			m_Colors.Clear() ;
			m_UVs.Clear() ;

			int x, z, y ;
	
			for( y  =  0 ; y <= 15 ; y ++ )
			{
				for( z  =  0 ; z <= 15 ; z ++ )
				{
					for( x  =  0 ; x <= 15 ; x ++ )
					{
						AddBlockFaces( x, z, y, ref m_Vertices, ref m_Normals, ref m_Colors, ref m_UVs ) ;
					}
				}
			}

			//-------------------------------------------------

			m_Mesh = new Mesh() ;

			Model = new GameObject( "Chunk[" + X + "," + Y + "," + Z + "]" ) ;
			m_MeshRenderer = Model.AddComponent<MeshRenderer>() ;
			m_MeshRenderer.materials = new Material[]{ m_Owner.DefaultMaterial } ;
			m_MeshFilter = Model.AddComponent<MeshFilter>() ;
			m_MeshFilter.mesh = m_Mesh ;

			Model.transform.SetParent( parent, false ) ;
			Model.transform.localPosition = new Vector3( X * 16, Y * 16, Z * 16 ) ;

			float x0 = Model.transform.position.x ;
			float y0 = Model.transform.position.y ;
			float z0 = Model.transform.position.z ;
			float x1 = x0 + 16 ;
			float y1 = y0 + 16 ;
			float z1 = z0 + 16 ;

			BoundingBox = new Vector3[]
			{
				new Vector3( x0, y0, z0 ),
				new Vector3( x1, y0, z0 ),
				new Vector3( x0, y1, z0 ),
				new Vector3( x1, y1, z0 ),
				new Vector3( x0, y0, z1 ),
				new Vector3( x1, y0, z1 ),
				new Vector3( x0, y1, z1 ),
				new Vector3( x1, y1, z1 ),
			} ;

			//----------------------------------

			UpdateMesh() ;
			
			//----------------------------------

			// このチャンクは処理済み
			IsCompleted = true ;

			return true ;
		}

		/// <summary>
		/// メッシュを更新する
		/// </summary>
		public void UpdateMesh()
		{
			if( m_Mesh == null )
			{
				// まだメッシュ化の準備が出来ていない
				return ;
			}

			//----------------------------------

			m_Mesh.Clear() ;

			if( m_Vertices.Count == 0 )
			{
				return ;
			}

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
			m_Mesh.SetIndices( indices, MeshTopology.Quads, 0 ) ;
		}

		/// <summary>
		/// メッシュを破棄する
		/// </summary>
		public void DeleteModel()
		{
			if( Model != null )
			{
				GameObject.DestroyImmediate( Model ) ;
				Model = null ;
				m_Mesh = null ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// ブロックを更新する(ブロックの種別を設定した後に呼び出すこと)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		public void UpdateBlockFaces( int x, int z, int y )
		{
			RemoveBlockFaces( x, z, y ) ;
			AddBlockFaces( x, z, y, ref m_Vertices, ref m_Normals, ref m_Colors, ref m_UVs ) ;
		}

		// １つのブロックの全ての面を追加する
		private void AddBlockFaces( int x, int z, int y, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT )
		{
			if( Block[ x, z, y ] == 0 )
			{
				return ;	// 無し
			}

			short bc = Block[ x, z, y ] ;
			short bi = ( short )( ( y << 8 ) | ( z << 4 ) | x ) ;

			bool neighborEmpty ;
			int upperBlock ;

			// 上方向のブロック
			if( y == 15 )
			{
				// 上
				if( m_Owner.ActiveChunks.ContainsKey( m_CidY1 ) == true )
				{
					// 上チャンクの一番下のブロック
					upperBlock = m_Owner.ActiveChunks[ m_CidY1 ].Block[ x, z,  0 ] ;
				}
				else
				{
					upperBlock = 0 ;	// ブロックは無いものとみなす
				}
			}
			else
			{
				upperBlock = Block[ x, z, y + 1 ] ;
			}

			if( x ==  0 || x == 15 || z ==  0 || z == 15 || y ==  0 || y == 15 )
			{
				// 重い処理

				// X-面の判定
				if( x ==  0 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidX0 ) == true && m_Owner.ActiveChunks[ m_CidX0 ].Block[ 15, z, y ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x - 1, z, y ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// X-面の追加
					AddFaceX0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}

				// X+面の判定
				if( x == 15 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidX1 ) == true && m_Owner.ActiveChunks[ m_CidX1 ].Block[  0, z, y ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x + 1, z, y ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// X+面の追加
					AddFaceX1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}

				// Z-面の判定
				if( z ==  0 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidZ0 ) == true && m_Owner.ActiveChunks[ m_CidZ0 ].Block[ x, 15, y ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x, z - 1, y ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Z-面の追加
					AddFaceZ0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}

				// Z+面の判定
				if( z == 15 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidZ1 ) == true && m_Owner.ActiveChunks[ m_CidZ1 ].Block[ x,  0, y ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x, z + 1, y ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Z+面の追加
					AddFaceZ1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}

				// Y-面の判定
				if( y ==  0 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidY0 ) == true && m_Owner.ActiveChunks[ m_CidY0 ].Block[ x, z, 15 ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x, z, y - 1 ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Y-面の追加
					AddFaceY0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}

				// Y+面の判定
				if( y == 15 )
				{
					neighborEmpty = !( m_Owner.ActiveChunks.ContainsKey( m_CidY1 ) == true && m_Owner.ActiveChunks[ m_CidY1 ].Block[ x, z,  0 ] != 0 ) ;
				}
				else
				{
					neighborEmpty =	( Block[ x, z, y + 1 ] == 0 ) ;
				}
				if( neighborEmpty == true )
				{
					// Y+面の追加
					AddFaceY1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
			}
			else
			{
				// 軽い処理
				if( Block[ x - 1, z, y ] == 0 )
				{
					// X-面の追加
					AddFaceX0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
				if( Block[ x + 1, z, y ] == 0 )
				{
					// X-面の追加
					AddFaceX1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
				if( Block[ x, z - 1, y ] == 0 )
				{
					// Z-面の追加
					AddFaceZ0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
				if( Block[ x, z + 1, y ] == 0 )
				{
					// Z+面の追加
					AddFaceZ1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
				if( Block[ x, z, y - 1 ] == 0 )
				{
					// Y-面の追加
					AddFaceY0( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
				if( Block[ x, z, y + 1 ] == 0 )
				{
					// Y+面の追加
					AddFaceY1( x, z, y, bc, ref aV, ref aN, ref aC, ref aT, upperBlock ) ;
					m_BlockIndices.Add( bi ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// X-面の追加
		private void AddFaceX0( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x, y,     z + 1 ) ) ;
			aV.Add( new Vector3( x, y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x, y + 1, z     ) ) ;
			aV.Add( new Vector3( x, y,     z     ) ) ;

			aN.AddRange( m_Nx0s ) ;
			aC.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// X+面の追加
		private void AddFaceX1( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x + 1, y,     z     ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z     ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x + 1, y,     z + 1 ) ) ;

			aN.AddRange( m_Nx1s ) ;
			aC.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// Y-面の追加
		private void AddFaceY0( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x,     y,     z + 1 ) ) ;
			aV.Add( new Vector3( x,     y,     z     ) ) ;
			aV.Add( new Vector3( x + 1, y,     z     ) ) ;
			aV.Add( new Vector3( x + 1, y,     z + 1 ) ) ;

			aN.AddRange( m_Ny0s ) ;
			aC.AddRange( GetBlockVC( ti, 1, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 1, upperBlock ) ) ;
		}

		// Y+面の追加
		private void AddFaceY1( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x,     y + 1, z     ) ) ;
			aV.Add( new Vector3( x,     y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z     ) ) ;

			aN.AddRange( m_Ny1s ) ;
			aC.AddRange( GetBlockVC( ti, 0, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 0, upperBlock ) ) ;
		}

		// Z-面の追加
		private void AddFaceZ0( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x,     y,     z     ) ) ;
			aV.Add( new Vector3( x,     y + 1, z     ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z     ) ) ;
			aV.Add( new Vector3( x + 1, y,     z     ) ) ;

			aN.AddRange( m_Nz0s ) ;
			aC.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}

		// Z+面の追加
		private void AddFaceZ1( int x, int z, int y, int ti, ref List<Vector3> aV, ref List<Vector3> aN, ref List<Color32> aC, ref List<Vector2> aT, int upperBlock )
		{
			aV.Add( new Vector3( x + 1, y,     z + 1 ) ) ;
			aV.Add( new Vector3( x + 1, y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x,     y + 1, z + 1 ) ) ;
			aV.Add( new Vector3( x,     y,     z + 1 ) ) ;

			aN.AddRange( m_Nz1s ) ;
			aC.AddRange( GetBlockVC( ti, 2, upperBlock ) ) ;

			// ブロックの種類で変わる
			aT.AddRange( GetBlockUV( ti, 2, upperBlock ) ) ;
		}


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
		/// チャンク内のブロックを削除する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		public bool RemoveBlockFaces( int x, int z, int y )
		{
			if( m_BlockIndices.Count == 0 )
			{
				return false ;	// ブロックは１つも存在しない
			}

			short bi = ( short )( ( y << 8 ) | ( z << 4 ) | x ) ;

			int offset = m_BlockIndices.IndexOf( bi ) ;
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
				if( m_BlockIndices[ index ] != bi )
				{
					break ;
				}
				length ++ ;
			}

			// offset から length ぶんの情報を削除する
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
	}
}

