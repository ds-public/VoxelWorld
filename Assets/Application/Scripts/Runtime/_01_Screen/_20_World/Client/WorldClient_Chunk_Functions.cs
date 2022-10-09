using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.World
{
	/// <summary>
	/// クライアント:チャンク処理関係
	/// </summary>
	public partial class WorldClient
	{
		/// <summary>
		/// 全チャンクのメッシュを生成する(既に生成済みや生成不要なものは無視する)
		/// </summary>
		private int BuildAllChunks()
		{
			int c = 0 ;

			Vector3 p = m_PlayerActor.Position ;

			// ブロック座標の限界範囲に収める

			int bx = ( int )p.x ;
			int bz = ( int )p.z ;
			int by = ( int )p.y ;

			int bxMin = WorldSettings.WORLD_X_MIN ;
			int bxMax = WorldSettings.WORLD_X_MAX - 1 ;
			if( bx <  bxMin )
			{
				bx  = bxMin ;
			}
			else
			if( bx >  bxMax )
			{
				bx  = bxMax ;
			}

			int bzMin = WorldSettings.WORLD_Z_MIN ;
			int bzMax = WorldSettings.WORLD_Z_MAX - 1 ;
			if( bz <  bzMin )
			{
				bz  = bzMin ;
			}
			else
			if( bz >  bzMax )
			{
				bz  = bzMax ;
			}

			int byMin = WorldSettings.WORLD_Y_MIN ;
			int byMax = WorldSettings.WORLD_Y_MAX - 1 ;
			if( by <  byMin )
			{
				by  = byMin ;
			}
			else
			if( by >  byMax )
			{
				by  = byMax ;
			}

			//----------------------------------
			// 上下左右前後３チャンク(計２７チャンク)の座標範囲を算出する

			// チャンク座標
			int cx = ( bx >> 4 ) ;
			int cz = ( bz >> 4 ) ;
			int cy = ( by >> 4 ) ;

			// 必須:現在地の上下左右前後３チャンク(計２７チャンク)は１フレームでメッシュを作る

			int cxMin = WorldSettings.CHUNK_SET_X_MIN ;
			int cx0 = cx - 1 ;
			if( cx0 <  cxMin )
			{
				cx0  = cxMin ;
			}

			int cxMax = WorldSettings.CHUNK_SET_X_MAX ;
			int cx1 = cx + 1 ;
			if( cx1 >  cxMax )
			{
				cx1  = cxMax ;
			}

			int czMin = WorldSettings.CHUNK_SET_Z_MIN ;
			int cz0 = cz - 1 ;
			if( cz0 <  czMin )
			{
				cz0  = czMin ;
			}

			int czMax = WorldSettings.CHUNK_SET_Z_MAX ;
			int cz1 = cz + 1 ;
			if( cz1 >  czMax )
			{
				cz1  = czMax ;
			}

			int cyMin = WorldSettings.CHUNK_Y_MIN ;
			int cy0 = cy - 1 ;
			if( cy0 <  cyMin )
			{
				cy0  = cyMin ;
			}

			int cyMax = WorldSettings.CHUNK_Y_MAX ;
			int cy1 = cy + 1 ;
			if( cy1 >  cyMax )
			{
				cy1  = cyMax ;
			}

			//------------------------------------------------------------------------------------------
			// メッシュモデルを更新(生成・破棄)する

			int c0 = 0 ;
			int c1 = 0 ;

			int cId ;

			// メッシュ作りの処理時間計測開始
			float t = Time.realtimeSinceStartup ;

			// １フレームの時間の1/2の時間をリミットとする
			float limit = 0.5f / Application.targetFrameRate ;

			// 自身の周囲上下左右前後３チャンク(計２７チャンク)のメッシュを作る
			for( cy  = cy0 ; cy <= cy1 ; cy ++ )
			{
				for( cz  = cz0 ; cz <= cz1 ; cz ++ )
				{
					for( cx  = cx0 ; cx <  cx1 ; cx ++ )
					{
						cId = ( cy << 24 ) | ( cz << 12 ) | cx ; 
						if( m_ActiveChunks.ContainsKey( cId ) == true )
						{
							if( m_ActiveChunks[ cId ].RefreshModel( this, m_ChunkRoot.transform, false ) == true )
							{
								// 上下左右前後３チャンク(計２７チャンク)の内に実際に処理されたチャンク数
								c0 ++ ;
							}
						}
					}
				}
			}

			//----------------------------------

			// その他のチャンクについてはフレームレート低下を抑えるため
			// 一定時間内に収まる範囲でメッシュを作る
			var targetChunks = m_ActiveChunks.Values.Where( _ => _.IsCompleted == false && _.CanCreate( this ) == true ).OrderBy( _ => _.GetDistance( p.x, p.z, p.y ) ).ToArray() ;

			if( targetChunks != null && targetChunks.Length >  0 )
			{
				// 最低限１つは時間無視でメッシュを作る(でないと永久にメッシュ作りが終わらない)
				if( targetChunks[ 0 ].RefreshModel( this, m_ChunkRoot.transform, true ) == true )
				{
					// その他の実際に処理されたチャンク数
					c1 ++ ;
				}

				if( ( Time.realtimeSinceStartup - t ) <  limit )
				{
					// 後は時間が許す範囲でメッシュを作る
					int i, l = targetChunks.Length ;
					if( l >  1 )
					{
						for( i  = 1 ; i <  l ; i ++ )
						{
							if( targetChunks[ i ].RefreshModel( this, m_ChunkRoot.transform, true ) == true )
							{
								// その他の実際に処理されたチャンク数
								c1 ++ ;
							}

							if( ( Time.realtimeSinceStartup - t ) >  limit )
							{
								// 時間的限界に達したのでここでメッシュを作りは終了する(残りは次のフレームに回す)
								break ;
							}
						}
					}
				}
			}

			//----------------------------------

			// パフォーマンスモニタリング(処理対象チャンク数)
			if( m_P_ProcrssingChunk_T_Now.Value == 0 )
			{
				if( targetChunks.Length >  0 )
				{
					m_P_ProcrssingChunk_T_Max.Value = targetChunks.Length ;
				}
			}
			m_P_ProcrssingChunk_T_Now.Value = targetChunks.Length ;

			// パフォーマンスモニタリング(処理完了チャンク数)
			if( m_P_ProcrssingChunk_C_Now.Value == 0 )
			{
				if( c1 >  0 )
				{
					m_P_ProcrssingChunk_C_Max.Value = c1 ;
				}
			}
			m_P_ProcrssingChunk_C_Now.Value = c1 ;

//			if( c0 >  0  || c1 >  0 || targetChunks.Length >  0 )
//			{
//				Debug.Log( "<color=#00FF00>[CLIENT] メッシュ作成数:" + c0 + " " + c1 + " " + targetChunks.Length + " " + m_ActiveChunks.Count + "</color>" ) ;
//			}

			return c ;
		}

		//-------------------------------------------------------------

		/// <summary>
		/// 絶対座標からブロックの種類を取得する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int GetBlock( int bx, int bz, int by )
		{
			if( bx <  0x0000 || bx >  0xFFFF || bz <  0x0000 || bz >= 0xFFFF || by <  0x0000 || by >  0x03FF )
			{
				return 0 ;	// 範囲外
			}

			int cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
			if( m_ActiveChunks.ContainsKey( cId ) == false )
			{
				// チャンクがロードされていない
				return 0 ;
			}

			return m_ActiveChunks[ cId ].Block[ bx & 0x0F, bz & 0x0F, by & 0x0F ] ;
		}

		/// <summary>
		/// 絶対座標でブロックを設定する(同時にメッシュの更新も行う[高速])
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="b"></param>
		private bool SetBlock( int bx, int bz, int by, int bi )
		{
			if( bx <  0x0000 || bx >  0xFFFF || bz <  0x0000 || bz >= 0xFFFF || by <  0x0000 || by >  0x03FF )
			{
				return false ;	// 範囲外
			}

			int cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
			if( m_ActiveChunks.ContainsKey( cId ) == false )
			{
				// 対象チャンクがロードされていない(基本的にはありえない)
				return false ;
			}

			//------------------------------------------------------------------------------------------
			// まずは対象となるブロックを更新する

			int blx = bx & 0x0F ;
			int blz = bz & 0x0F ;
			int bly = by & 0x0F ;

			m_ActiveChunks[ cId ].Block[ blx, blz, bly ] = ( short )( bi & 0xFFFF ) ;
			m_ActiveChunks[ cId ].UpdateBlockFaces( this, blx, blz, bly ) ;

			//------------------------------------------------------------------------------------------
			// 周囲６方向のブロックのメッシュも(必要であれば)更新する

			int bnx, bnz, bny ;

			// X-
			bnx = bx - 1 ;
			if( bnx >= 0x0000 && bnx <= 0xFFFF )
			{
				cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bnx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, bnx & 0x0F, blz, bly ) ;
				}
			}

			// X+
			bnx = bx + 1 ;
			if( bnx >= 0x0000 && bnx <= 0xFFFF )
			{
				cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bnx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, bnx & 0x0F, blz, bly ) ;
				}
			}

			// Z-
			bnz = bz - 1 ;
			if( bnz >= 0x0000 && bnz <= 0xFFFF )
			{
				cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bnz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, blx, bnz & 0x0F, bly ) ;
				}
			}

			// Z+
			bnz = bz + 1 ;
			if( bnz >= 0x0000 && bnz <= 0xFFFF )
			{
				cId = ( int )( ( by & 0x03F0 ) << 20 ) | ( int )( ( bnz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, blx, bnz & 0x0F, bly ) ;
				}
			}

			// Y-
			bny = by - 1 ;
			if( bny >= 0x0000 && bny <= 0xFFFF )
			{
				cId = ( int )( ( bny & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, blx, blz, bny & 0x0F ) ;
				}
			}

			// Y+
			bny = by + 1 ;
			if( bny >= 0x0000 && bny <= 0xFFFF )
			{
				cId = ( int )( ( bny & 0x03F0 ) << 20 ) | ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;
				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					// 対象は同じチャンク内かもしれないし違うチャンクかもしれない
					m_ActiveChunks[ cId ].UpdateBlockFaces( this, blx, blz, bny & 0x0F ) ;
				}
			}

			return true ;
		}

		/// <summary>
		/// 整数値の符号を返す
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public int Sign( int v )
		{
			if( v >  0 )
			{
				return  1 ;
			}
			else
			if( v <  0 )
			{
				return -1 ;
			}
			else
			{
				return  0 ;
			}
		}

		/// <summary>
		/// 整数値の符号を返す
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public float Sign( float v )
		{
			if( v >  0 )
			{
				return  1 ;
			}
			else
			if( v <  0 )
			{
				return -1 ;
			}
			else
			{
				return  0 ;
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

			sx = Sign( dx ) ;
			sz = Sign( dz ) ;
			sy = Sign( dy ) ;

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
