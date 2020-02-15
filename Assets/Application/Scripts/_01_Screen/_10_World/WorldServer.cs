using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DBS.nScreen.nWorld
{
	public class WorldServer : MonoBehaviour
	{
		// 仮のストレージ
		private readonly Dictionary<long, ChunkSetData>	m_ChunkSetDataPool = new Dictionary<long, ChunkSetData>() ;

		[Serializable]
		public class ChunkSetRequest
		{
			public int X ;
			public int Z ;
			public Action<ChunkSetData> OnLoaded ;
		}

		[SerializeField]
		private List<ChunkSetRequest>	m_ChunkSetRequests = new List<ChunkSetRequest>() ;



		void Update()
		{
			if( m_ChunkSetRequests.Count >  0 )
			{
				// クライアントからのチャンクセットの取得要求あり

				// 要求を削除
				ChunkSetRequest request = m_ChunkSetRequests[ 0 ] ;
				m_ChunkSetRequests.RemoveAt( 0 ) ;

				// 生成を開始
				ChunkSetData chunkSetData = MakeChunkSetData( request ) ;
				request.OnLoaded( chunkSetData ) ;
			}
		}

		/// <summary>
		/// チャンクセットのロード要求を行う
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="onLoaded"></param>
		public void LoadChunkSetData( int x, int z, Action<ChunkSetData> onLoaded )
		{
			ChunkSetRequest request = new ChunkSetRequest()
			{
				X = x,
				Z = z,
				OnLoaded = onLoaded
			} ;

			m_ChunkSetRequests.Add( request ) ;
		}


		/// <summary>
		/// チャンクセットの取得または生成を行う
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="onLoaded"></param>
		public ChunkSetData MakeChunkSetData( ChunkSetRequest request )
		{
			int x = request.X ;
			int z = request.Z ;

			long csid = ( long )( z << 12 ) | ( long )x ;

			if( m_ChunkSetDataPool.ContainsKey( csid ) == true )
			{
				// 既に生成済み
				return m_ChunkSetDataPool[ csid ] ;
			}

			//-------------------------------------------------
			// 未生成のチャンクセットであるため新規に生成する

			ChunkSetData chunkSetData = new ChunkSetData()
			{
				X = x,
				Z = z
			} ;

			int[,] heightMap = new int[ 16, 16 ] ;	// x z

			float ox = x * 16 ;
			float oz = z * 16 ;

			int maxHeight = -1 ;

			float pn ;

			int bx, by, bz ;
			for( bz  = 0 ; bz <= 15 ; bz ++ )
			{
				for( bx  = 0 ; bx <= 15 ; bx ++ )
				{
					pn = PerlinNoise.GetValue( ( float )( ox + bx ) / 16, ( float )( oz + bz ) / 16 ) ;
					by = ( int )( ( pn * 8 ) + ( 512 ) ) ;

					heightMap[ bx, bz ] = by ;

					if( by >  maxHeight )
					{
						maxHeight = by ;
					}
				}
			}

			//-------------------------
			ChunkData chunkData ;

			int y0, y1 ;

			int h, hy ;
			for( h  =  0 ; h <  64 ; h ++ )
			{
				y0 =  h * 16 ;
				y1 = y0 + 15 ;

				if( y0 >  maxHeight )
				{
					// 全てが空になるチャンク
					break ;	// それ以上上のチャンクは存在しない
				}

				chunkData = new ChunkData() ;
				
				for( bz  = 0 ; bz <= 15 ; bz ++ )
				{
					for( bx  =  0 ; bx <= 15 ; bx ++ )
					{
						hy = heightMap[ bx, bz ] ;

						if( hy <  y0 )
						{
							// ここのブロックは全て無し
							continue ;
						}
						else
						if( hy >  y1 )
						{
							// ここのブロックは全て有り
							hy = 15 ;
						}
						else
						{
							// ここのブロックは一部有り
							hy -= y0 ;
						}

						for( by  =  0 ; by <= hy ; by ++ )
						{
							chunkData.Block[ bx, bz, by ] = 1 ;
						}
					}
				}

				chunkSetData.Chunks[ h ] = chunkData ;
			}

			// プールに追加する
			m_ChunkSetDataPool.Add( csid, chunkSetData ) ;

			return chunkSetData ;
		}

		/// <summary>
		/// 指定の絶対位置のブロックを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool SetBlock( int x, int z, int y, short b )
		{
			long csid = ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;

			if( m_ChunkSetDataPool.ContainsKey( csid ) == false )
			{
				// 指定の位置に対応するチャンクは生成されていない
				Debug.LogError( "[エラー]指定位置に対応するチャンクは生成されていない: x = " + x + " z = " + z ) ;
				return false ;
			}

			// 縦は64チャンクある
			int h = ( y & 0x03F0 ) >> 4 ;

			if( b != 0 )
			{
				if( m_ChunkSetDataPool[ csid ].Chunks[ h ] == null )
				{
					// これまで完全な空白チャンクだった
					m_ChunkSetDataPool[ csid ].Chunks[ h ] = new ChunkData() ;
				}

				m_ChunkSetDataPool[ csid ].Chunks[ h ].Block[ x & 0x0F, z & 0x0F, y & 0x0F ] = b ;
			}
			else
			{
				if( m_ChunkSetDataPool[ csid ].Chunks[ h ] != null )
				{
					m_ChunkSetDataPool[ csid ].Chunks[ h ].Block[ x & 0x0F, z & 0x0F, y & 0x0F ] = b ;

					// 完全に空白になったか確認する
					int lx, lz, ly, c = 0 ;
					for( ly  = 0 ; ly <= 15 ; ly ++ )
					{
						for( lz  = 0 ; lz <= 15 ; lz ++ )
						{
							for( lx  = 0 ; lx <= 15 ; lx ++ )
							{
								if( m_ChunkSetDataPool[ csid ].Chunks[ h ].Block[ lx, lz, ly ] != 0 )
								{
									c ++ ;
								}
							}
						}
					}

					if( c == 0 )
					{
						// 完全空白化
						m_ChunkSetDataPool[ csid ].Chunks[ h ] = null ;
					}
				}
			}

			return true ;	// 成功
		}

	}
}

