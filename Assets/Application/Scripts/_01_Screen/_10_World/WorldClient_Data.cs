using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// クライアント(データ)
	/// </summary>
	public partial class WorldClient : MonoBehaviour
	{
		/// <summary>
		/// 縦６４チャンクをチャンクセットから追加する
		/// </summary>
		/// <param name="activeChunkSetData"></param>
		private void AddAllChunks( ActiveChunkSetData activeChunkSetData )
		{
			int y ;
			for( y  =  0 ; y <  64 ; y ++ )
			{
				AddChunk( activeChunkSetData.X, activeChunkSetData.Z, y, activeChunkSetData.Chunks[ y ] ) ;
			}
		}

		/// <summary>
		/// チャンクを追加する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="chunkData"></param>
		private void AddChunk( int x, int z, int y, ChunkData chunkData )
		{
			ActiveChunkData activeChunk = new ActiveChunkData( this, x, z, y, chunkData ) ;

			long cid = ( long )( y << 24 ) | ( long )( z << 12 ) | ( long )x ;
			ActiveChunks.Add( cid, activeChunk ) ;
		}

		/// <summary>
		/// 縦６４チャンクをチャンクセットから削除する
		/// </summary>
		/// <param name="activeChunkSetData"></param>
		private void RemoveAllChunks( ActiveChunkSetData activeChunkSetData )
		{
			int y ;
			for( y  =  0 ; y <  64 ; y ++ )
			{
				RemoveChunk( activeChunkSetData.X, activeChunkSetData.Z, y ) ;
			}
		}

		/// <summary>
		/// チャンクを削除する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		private void RemoveChunk( int x, int z, int y )
		{
			long cid = ( long )( y << 24 ) | ( long )( z << 12 ) | ( long )x ;

			if( ActiveChunks.ContainsKey( cid ) == true )
			{
				ActiveChunks[ cid ].DeleteModel() ;
				ActiveChunks.Remove( cid ) ;
			}
		}

		/// <summary>
		/// 全チャンクのメッシュを生成する(既に生成済みや生成不要なものは無視する)
		/// </summary>
		private int BuildAllChunks()
		{
			int c = 0 ;

			foreach( var activeChunk in ActiveChunks )
			{
				if( activeChunk.Value.CreateModel( m_BoxelRoot.transform ) == true )
				{
					c ++ ;
				}
			}

			return c ;
		}

		//---------------------------------------------------------------------------

		private readonly List<long> m_ChunkSetRequests = new List<long>() ;

		/// <summary>
		/// 現在位置に応じたチャンクの展開と除去を行う
		/// </summary>
		private void UpdateChunk()
		{
			// 現在の位置から周囲+10～-10チャンクを展開する

			int px = ( int )( m_Camera.transform.position.x / 16 ) ;
			int pz = ( int )( m_Camera.transform.position.z / 16 ) ;

			int length = 10 + 1 ;

			int x0 = px - length ;
			int x0i = x0 + 1 ;
			if( x0 <     0 )
			{
				x0  =    0 ;
			}
			if( x0i <     0 )
			{
				x0i  =    0 ;
			}

			int x1 = px + length ;
			int x1i = x1 - 1 ;
			if( x1 >  4095 )
			{
				x1  = 4095 ;
			}
			if( x1i >  4095 )
			{
				x1i  = 4095 ;
			}

			int z0 = pz - length ;
			int z0i = z0 + 1 ;
			if( z0 <     0 )
			{
				z0  =    0 ;
			}
			if( z0i <     0 )
			{
				z0i  =    0 ;
			}

			int z1 = pz + length ;
			int z1i = z1 - 1 ;
			if( z1 >  4095 )
			{
				z1  = 4095 ;
			}
			if( z1i >  4095 )
			{
				z1i  = 4095 ;
			}

			// 一旦全ての使用中クラグをクリアする
			foreach( var activeChunkSetData in ActiveChunkSets )
			{
				activeChunkSetData.Value.IsUsing = false ;
			}

			int x, z ;
			long csid ;

			float t = Time.realtimeSinceStartup ;

			for( z  = z0 ; z < z1 ; z ++ )
			{
				for( x  = x0 ; x <= x1 ; x ++ )
				{
					csid = ( long )( z << 12 ) | ( long )x ;

					// 東西南北の＋１の範囲のチャンクは破棄せず維持する
					if( ActiveChunkSets.ContainsKey( csid ) == true )
					{
						// 既に存在しているので継続して使用する
						ActiveChunkSets[ csid ].IsUsing = true ;
					}
					else
					if( x >= x0i && x <= x1i && z >= z0i && z <= z1i )
					{
						// 全範囲に存在しておらず且つ必要範囲にも存在していなもののみ読み出す
						if( m_ChunkSetRequests.Contains( csid ) == false )
						{
							// ただし未リクエスト状態のもののみ

							// リクエストを登録する
							m_ChunkSetRequests.Add( csid ) ;

							// 通信想定：ロード要求をサーバーに送る(結果がコールバックで返る)
							m_WorldServer.LoadChunkSetData( x, z, ( ChunkSetData chunkSetData ) =>
							{
								if( m_ChunkSetRequests.Contains( chunkSetData.Csid ) == true )
								{
									// まだリクエストが有効であれば保存して展開する

									ActiveChunkSetData activeChunkSetData = ToActiveChunkSetData( chunkSetData ) ;

									// 非同期なのでチャンクセット識別子はコールバックで送られたものを使用する必要がある
									ActiveChunkSets.Add( activeChunkSetData.Csid, activeChunkSetData ) ;	// 読みだしたセットを登録
									AddAllChunks( activeChunkSetData ) ;	// 各チャンクを展開

									activeChunkSetData.IsUsing = true ;

									// リクエストを終了する(登録を破棄)
									m_ChunkSetRequests.Remove( activeChunkSetData.Csid ) ;
								}
							} ) ;
						}
					}
				}
			}

			// 使用しなくなるものを破棄する
			Dictionary<long, ActiveChunkSetData> activeChunkSets = new Dictionary<long, ActiveChunkSetData>() ;

			foreach( var activeChunkSetData in ActiveChunkSets )
			{
				if( activeChunkSetData.Value.IsUsing == false )
				{
					// 使用しなくなる

					// リクエストが発行(登録)中であれば破棄する
					if( m_ChunkSetRequests.Contains( activeChunkSetData.Value.Csid ) == true )
					{
						m_ChunkSetRequests.Remove( activeChunkSetData.Value.Csid ) ;
					}

					// そのチャンクセットに含まれる全チャンクを破棄する
					RemoveAllChunks( activeChunkSetData.Value ) ;
				}
				else
				{
					// まだ使用する
					activeChunkSets.Add( activeChunkSetData.Key, activeChunkSetData.Value ) ;
				}
			}

//			Debug.LogWarning( "e0:" + e0 + " e1:" + e1 + " d0:" + d0 + " d1:" + d1 ) ;

			// 更新
			ActiveChunkSets = activeChunkSets ;

			// チャンクのメッシュを生成する(移動による作り直しは想定していないので生成済みのメッシュは更新しない)
			t = Time.realtimeSinceStartup ;
			int c = BuildAllChunks() ;
			if( c >  0 )
			{
//				Debug.LogWarning( "新規展開チャンク(作)　個数:" + c + "　時間:" + ( Time.realtimeSinceStartup - t ) ) ;
			}
		}

		/// <summary>
		/// アクティブなチャンクセットを生成する
		/// </summary>
		/// <param name="chunkSetData"></param>
		/// <returns></returns>
		private ActiveChunkSetData ToActiveChunkSetData( ChunkSetData chunkSetData )
		{
			ActiveChunkSetData activeChunkSetData = new ActiveChunkSetData()
			{
				X = chunkSetData.X,
				Z = chunkSetData.Z,
				Chunks = chunkSetData.Chunks
			} ;

			return activeChunkSetData ;
		}

		//-------------------------------------------------------------

		/// <summary>
		/// 絶対座標からブロックの種類を取得する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int GetBlock( int x, int z, int y )
		{
			if( x <  0x0000 || x >  0xFFFF || z <  0x0000 || z >= 0xFFFF || y <  0x0000 || y >  0x03FF )
			{
				return 0 ;	// 範囲外
			}

			long cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
			if( ActiveChunks.ContainsKey( cid ) == false )
			{
				// チャンクがロードされていない
				return 0 ;
			}

			return ActiveChunks[ cid ].Block[ x & 0x0F, z & 0x0F, y & 0x0F ] ;
		}

		/// <summary>
		/// 絶対座標でブロックを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="b"></param>
		private void SetBlock( int x, int z, int y, int b )
		{
			if( x <  0x0000 || x >  0xFFFF || z <  0x0000 || z >= 0xFFFF || y <  0x0000 || y >  0x03FF )
			{
				return ;	// 範囲外
			}

			List<long> cids = new List<long>() ;

			long cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
			if( ActiveChunks.ContainsKey( cid ) == false )
			{
				// チャンクがロードされていない
				return ;
			}

			cids.Add( cid ) ;

			int lx = x & 0x0F ;
			int lz = z & 0x0F ;
			int ly = y & 0x0F ;

			ActiveChunks[ cid ].Block[ lx, lz, ly ] = ( short )( b & 0xFFFF ) ;
			ActiveChunks[ cid ].UpdateBlockFaces( lx, lz, ly ) ;

			int nx, nz, ny ;

			// 周囲６方向のブロックのメッシュも更新する

			// X-
			nx = x - 1 ;
			if( nx >= 0x0000 && nx <= 0xFFFF )
			{
				cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( nx & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( nx & 0x0F, lz, ly ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			// X+
			nx = x + 1 ;
			if( nx >= 0x0000 && nx <= 0xFFFF )
			{
				cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( nx & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( nx & 0x0F, lz, ly ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			// Z-
			nz = z - 1 ;
			if( nz >= 0x0000 && nz <= 0xFFFF )
			{
				cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( nz & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( lx, nz & 0x0F, ly ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			// Z+
			nz = z + 1 ;
			if( nz >= 0x0000 && nz <= 0xFFFF )
			{
				cid = ( long )( ( y & 0x03F0 ) << 20 ) | ( long )( ( nz & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( lx, nz & 0x0F, ly ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			// Y-
			ny = y - 1 ;
			if( ny >= 0x0000 && ny <= 0xFFFF )
			{
				cid = ( long )( ( ny & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( lx, lz, ny & 0x0F ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			// Y+
			ny = y + 1 ;
			if( ny >= 0x0000 && ny <= 0xFFFF )
			{
				cid = ( long )( ( ny & 0x03F0 ) << 20 ) | ( long )( ( z & 0xFFF0 ) <<  8 ) | ( long )( ( x & 0xFFF0 ) >> 4 ) ;
				if( ActiveChunks.ContainsKey( cid ) == true )
				{
					ActiveChunks[ cid ].UpdateBlockFaces( lx, lz, ny & 0x0F ) ;
					if( cids.Contains( cid ) == false )
					{
						cids.Add( cid ) ;
					}
				}
			}

			foreach( long targetCid in cids )
			{
				ActiveChunks[ targetCid ].UpdateMesh() ;
			}
		}

		/// <summary>
		/// カメラから伸ばしたレイにヒットするブロックの座標とその直前の無空間の座標を取得する
		/// </summary>
		/// <param name="distance"></param>
		/// <returns></returns>
		private bool GetRaycastTargetBlock( float distance, out BlockPosition exist, out BlockPosition empty )
		{
			Vector3 fv = m_Camera.transform.forward ;
			
			Vector3 p0 = m_Camera.transform.position ;
			Vector3 p1 = m_Camera.transform.position + fv * distance ;

			// p0 から p1 に含まれるセルで最初にブロックが存在するセルの座標を取得する

			int x0, z0, y0 ;
			int x1, z1, y1 ;

			x0 = ( int )p0.x ;
			z0 = ( int )p0.z ;
			y0 = ( int )p0.y ;

			x1 = ( int )p1.x ;
			z1 = ( int )p1.z ;
			y1 = ( int )p1.y ;

			int dx, dz, dy ;

			dx = x1 - x0 ;
			dz = z1 - z0 ;
			dy = y1 - y0 ;

			int lx, lz, ly ;

			lx = Mathf.Abs( dx ) ;
			lz = Mathf.Abs( dz ) ;
			ly = Mathf.Abs( dy ) ;

			int sx, sz, sy ;

			sx = Mathi.Sign( dx ) ;
			sz = Mathi.Sign( dz ) ;
			sy = Mathi.Sign( dy ) ;

			exist.X = x0 ;
			exist.Z = z0 ;
			exist.Y = y0 ;

			empty.X = x0 ;
			empty.Z = z0 ;
			empty.Y = y0 ;



			float fx, fz, fy ;

			Vector3 faceNormal ;
			Vector3 pointDirection ;

			Vector3 cp ;

			float d0, d1 ;

			float minDistance = Mathf.Infinity ;
			float hitDistance ;

			int x, z, y ;
			int cx, cz, cy ;

			y = y0 ;
			for( cy  = 0 ; cy <= ly ; cy ++ )
			{
				z = z0 ;
				for( cz  = 0 ; cz <= lz ; cz ++ )
				{
					x = x0 ;
					for( cx  = 0 ; cx <= lx ; cx ++ )
					{
						// 現在位置は無視する
						if( x == x0 && z == z0 && y == y0 )
						{
							x += sx ;
							continue ;
						}

						// 確認位置にブロックが存在するか判定する
						if( GetBlock( x, z, y ) == 0 )
						{
							x += sx ;
							continue ;
						}

						// 向いている方向によって判定面を選別する
						
						// X
						if( sx != 0 )
						{
							// レイの始点と終点と平面が交差するか判定する

							// 平面のX
							fx = sx > 0 ? x : x + 1 ;

							faceNormal.x = - sx ;
							faceNormal.z =    0 ;
							faceNormal.y =    0 ;

							pointDirection.x = p0.x - fx ;
							pointDirection.z = p0.z - z ;
							pointDirection.y = p0.y - y ;

							d0 = Vector3.Dot( faceNormal, pointDirection ) ;

							pointDirection.x = p1.x - fx ;
							pointDirection.z = p1.z - z ;
							pointDirection.y = p1.y - y ;

							d1 = Vector3.Dot( faceNormal, pointDirection ) ;

							if( ( d0 * d1 ) <= 0 )
							{
								// 交差か方点が面上にある
								cp = fv * ( ( fx - p0.x ) / fv.x ) + p0 ;

								if( cp.z >= z && cp.z <= ( z + 1 ) && cp.y >= y && cp.y <= ( y + 1 ) )
								{
									// 面の範囲内でこの座標は有効
									hitDistance = ( cp - p0 ).magnitude ;
									if( hitDistance <  minDistance )
									{
										minDistance  = hitDistance ;

										exist.X = x ;
										exist.Z = z ;
										exist.Y = y ;

										empty.X = exist.X - sx ;
										empty.Z = exist.Z ;
										empty.Y = exist.Y ;
									}
								}
							}
						}

						// Z
						if( sz != 0 )
						{
							// レイの始点と終点と平面が交差するか判定する

							// 平面のZ
							fz = sz > 0 ? z : z + 1 ;

							faceNormal.x =    0 ;
							faceNormal.z = - sz ;
							faceNormal.y =    0 ;

							pointDirection.x = p0.x - x ;
							pointDirection.z = p0.z - fz ;
							pointDirection.y = p0.y - y ;

							d0 = Vector3.Dot( faceNormal, pointDirection ) ;

							pointDirection.x = p1.x - x ;
							pointDirection.z = p1.z - fz ;
							pointDirection.y = p1.y - y ;

							d1 = Vector3.Dot( faceNormal, pointDirection ) ;

							if( ( d0 * d1 ) <= 0 )
							{
								// 交差か方点が面上にある
								cp = fv * ( ( fz - p0.z ) / fv.z ) + p0 ;

								if( cp.x >= x && cp.x <= ( x + 1 ) && cp.y >= y && cp.y <= ( y + 1 ) )
								{
									// 面の範囲内なのでこの座標は有効
									hitDistance = ( cp - p0 ).magnitude ;
									if( hitDistance <  minDistance )
									{
										minDistance  = hitDistance ;

										exist.X = x ;
										exist.Z = z ;
										exist.Y = y ;

										empty.X = exist.X ;
										empty.Z = exist.Z - sz ;
										empty.Y = exist.Y ;
									}
								}
							}
						}

						// Y
						if( sy != 0 )
						{
							// レイの始点と終点と平面が交差するか判定する

							// 平面のY
							fy = sy > 0 ? y : y + 1 ;

							faceNormal.x =    0 ;
							faceNormal.z =    0 ;
							faceNormal.y = - sy ;

							pointDirection.x = p0.x - x ;
							pointDirection.z = p0.z - z ;
							pointDirection.y = p0.y - fy ;

							d0 = Vector3.Dot( faceNormal, pointDirection ) ;

							pointDirection.x = p1.x - x ;
							pointDirection.z = p1.z - z ;
							pointDirection.y = p1.y - fy ;

							d1 = Vector3.Dot( faceNormal, pointDirection ) ;

							if( ( d0 * d1 ) <= 0 )
							{
								// 交差か方点が面上にある
								cp = fv * ( ( fy - p0.y ) / fv.y ) + p0 ;

								if( cp.x >= x && cp.x <= ( x + 1 ) && cp.z >= z && cp.z <= ( z + 1 ) )
								{
									// 面の範囲内なのでこの座標は有効
									hitDistance = ( cp - p0 ).magnitude ;
									if( hitDistance <  minDistance )
									{
										minDistance  = hitDistance ;

										exist.X = x ;
										exist.Z = z ;
										exist.Y = y ;

										empty.X = exist.X ;
										empty.Z = exist.Z ;
										empty.Y = exist.Y - sy ;
									}
								}
							}
						}

						x += sx ;
					}

					z += sz ;
				}

				y += sy ;
			}

			return ( minDistance != Mathf.Infinity ) ;
		}
	}
}