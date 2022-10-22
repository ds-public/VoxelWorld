using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using DBS.World ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(コリジョン)
	/// </summary>
	public partial class WorldClient
	{
		// 水平方向への移動と押し戻し
		private bool ProcessMoving_Slice( Vector2 velocity, bool isSneak )
		{
			// プレイヤーコライダー半径
			float radius = m_PlayerRadius ;

			if( isSneak == true && m_IsFalling == false && m_IsJumping == false )
			{
				// スニーク中は移動速度を半分にする
				velocity *= 0.5f ;
			}

			//----------------------------------

			// 移動後の座標
			Vector3 p = m_PlayerActor.Position + new Vector3( velocity.x, 0, velocity.y ) ;

			//----------------------------------

			// 検査対象ブロックは、移動前から移動後の円と接触する全てのブロック

			float p_x0, p_x1, p_z0, p_z1 ;

			int bx, bz ;
			int bx0, bx1, bz0, bz1 ;

			// 座標情報を更新する
			void RefreshPosition()
			{
				p_x0 = p.x - radius ;
				p_x1 = p.x + radius ;
				p_z0 = p.z - radius ;
				p_z1 = p.z + radius ;

				// 範囲限定
				if( p_x0 <  WorldSettings.WORLD_X_MIN )
				{
					p_x0  = WorldSettings.WORLD_X_MIN ;
				}

				if( p_x1 >  WorldSettings.WORLD_X_MAX )
				{
					p_x1  = WorldSettings.WORLD_X_MAX ;
				}

				if( p_z0 <  WorldSettings.WORLD_Z_MIN )
				{
					p_z0  = WorldSettings.WORLD_Z_MIN ;
				}

				if( p_z1 >  WorldSettings.WORLD_Z_MAX )
				{
					p_z1  = WorldSettings.WORLD_Z_MAX ;
				}

				// 現在の水平方向の座標(ブロック単位)
				bx = ( int )p.x ;
				bz = ( int )p.z ;
			}

			RefreshPosition() ;

			//----------------------------------
			// 垂直方向の検査範囲

			// 移動後の体積の存在する範囲
			float py0 = p.y ;
			float py1 = p.y + m_PlayerHeight ;

			int by0 = ( int )py0 ;
			int by1 = ( int )py1 ;	// 上位置から判定対象ブロック座標を算出する
			if( ( py1 % 1 ) == 0 )
			{
				by1 -- ;
			}

			//----------------------------------------------------------

			// 少し補正をかける
			radius *= 1.02f ;

			int y ;

			float cx, cz ;
			float dx, dz ;
			float d ;

			for( y  = by0 ; y <= by1 ; y ++ )
			{
				// X- の検査
				if( ( p_x0 % 1 ) != 0 )	// ブロック境界の場合は検査不要
				{
					bx0 = ( int )p_x0 ;

					if( bx0 >=     0 && bx0 <  bx && GetBlock( bx0, bz, y ) != 0 )
					{
						// X- にはブロックがあるので X+ 方向に押し返す
						p.x = ( float )bx0 + 1.0f + radius ;
						RefreshPosition() ;
					}
				}

				// X+ の検査
				if( ( p_x1 % 1 ) != 0 )	// ブロック境界の場合は検査不要
				{
					bx1 = ( int )p_x1 ;

					if( bx1 <= 65535 && bx1 >  bx && GetBlock( bx1, bz, y ) != 0 )
					{
						// X + にはブロックがあるので X- 方向に押し返す
						p.x = ( float )bx1 - radius ;
						RefreshPosition() ;
					}
				}

				// Z- の検査
				if( ( p_z0 % 1 ) != 0 )	// ブロック境界の場合は検査不要
				{
					bz0 = ( int )p_z0 ;

					if( bz0 >=     0 && bz0 <  bz && GetBlock( bx, bz0, y ) != 0 )
					{
						// Z - にはブロックがあるので Z + 方向に押し返す
						p.z = ( float )bz0 + 1.0f + radius ;
						RefreshPosition() ;
					}
				}

				// Z+ の検査
				if( ( p_z1 % 1 ) != 0 )	// ブロック境界の場合は検査不要
				{
					bz1 = ( int )p_z1 ;

					if( bz1 <= 65535 && bz1 >  bz && GetBlock( bx, bz1, y ) != 0 )
					{
						// Z+ にはブロックがあるので Z- 方向に押し返す
						p.z = ( float )bz1 - radius ;
						RefreshPosition() ;
					}
				}

				//---------------------------------

				// X- Z- の検査
				if( ( p_x0 % 1 ) != 0 && ( p_z0 % 1 ) != 0 )
				{
					bx0 = ( int )p_x0 ;
					bz0 = ( int )p_z0 ;

					if( bx0 >=     0 && bz0 >=     0 && bx0 <  bx && bz0 <  bz && GetBlock( bx0, bz0, y ) != 0 )
					{
						// 角座標
						cx = bx0 + 1 ;
						cz = bz0 + 1 ;

						dx = cx - p.x ;
						dz = cz - p.z ;

						// 中心から角までの距離
						d = Mathf.Sqrt( dx * dx + dz * dz ) ; 

						if( d >  0 && d <  radius )
						{
							// 角がコライダー内にめり込んでいる

							// 正規化
							dx /= d ;
							dz /= d ;

							d = radius - d ;

							// 押し返す
							p.x -= ( dx * d ) ;
							p.z -= ( dz * d ) ;

							RefreshPosition() ;
						}
					}
				}

				// X+ Z- の検査
				if( ( p_x1 % 1 ) != 0 && ( p_z0 % 1 ) != 0 )
				{
					bx1 = ( int )p_x1 ;
					bz0 = ( int )p_z0 ;

					if( bx1 <= 65535 && bz0 >=     0 && bx1 >  bx && bz0 <  bz && GetBlock( bx1, bz0, y ) != 0 )
					{
						// 角座標
						cx = bx1     ;
						cz = bz0 + 1 ;

						dx = cx - p.x ;
						dz = cz - p.z ;

						// 中心から角までの距離
						d = Mathf.Sqrt( dx * dx + dz * dz ) ; 

						if( d >  0 && d <  radius )
						{
							// 角がコライダー内にめり込んでいる

							// 正規化
							dx /= d ;
							dz /= d ;

							d = radius - d ;

							// 押し返す
							p.x -= ( dx * d ) ;
							p.z -= ( dz * d ) ;

							RefreshPosition() ;
						}
					}
				}

				// X- Z+ の検査
				if( ( p_x0 % 1 ) != 0 && ( p_z1 % 1 ) != 0 )
				{
					bx0 = ( int )p_x0 ;
					bz1 = ( int )p_z1 ;

					if( bx0 >=     0 && bz1 <= 65535 && bx0 <  bx && bz1 >  bz && GetBlock( bx0, bz1, y ) != 0 )
					{
						// 角座標
						cx = bx0 + 1 ;
						cz = bz1     ;

						dx = cx - p.x ;
						dz = cz - p.z ;

						// 中心から角までの距離
						d = Mathf.Sqrt( dx * dx + dz * dz ) ; 

						if( d >  0 && d <  radius )
						{
							// 角がコライダー内にめり込んでいる

							// 正規化
							dx /= d ;
							dz /= d ;

							d = radius - d ;

							// 押し返す
							p.x -= ( dx * d ) ;
							p.z -= ( dz * d ) ;

							RefreshPosition() ;
						}
					}
				}

				// X+ Z+ の検査
				if( ( p_x1 % 1 ) != 0 && ( p_z1 % 1 ) != 0 )
				{
					bx1 = ( int )p_x1 ;
					bz1 = ( int )p_z1 ;

					if( bx1 <= 65535 && bz1 <= 65535 && bx1 >  bx && bz1 >  bz && GetBlock( bx1, bz1, y ) != 0 )
					{
						// 角座標
						cx = bx1     ;
						cz = bz1     ;

						dx = cx - p.x ;
						dz = cz - p.z ;

						// 中心から角までの距離
						d = Mathf.Sqrt( dx * dx + dz * dz ) ; 

						if( d >  0 && d <  radius )
						{
							// 角がコライダー内にめり込んでいる

							// 正規化
							dx /= d ;
							dz /= d ;

							d = radius - d ;

							// 押し返す
							p.x -= ( dx * d ) ;
							p.z -= ( dz * d ) ;

							RefreshPosition() ;
						}
					}
				}
			}

			// 壁接触による押し戻しを処理した後にスニーク状態であれば落下判定と落下防止の押し戻し処理を行う
			//----------------------------------------------------------

			if( isSneak == true && m_IsFalling == false && m_IsJumping == false )
			{
				// スニークが有効な場合は落下先には移動できない
				// 移動先で落下するか判定する(落下量で符号は+)
				( bool isFalling, _ ) = ProcessFalling(  0.1f, p, isSneak:true ) ;
				if( isFalling == true )
				{
//					Debug.Log( "<color=#00FF00>スニークチェックで落下すると認識した:" + p.z + "</color>" ) ;
					// 落下するので移動量を抑制する
					( _, p ) = ProcessStopFalling( m_PlayerActor.Position, p, isDebug:false ) ;
				}
			}

			//----------------------------------------------------------

			// プレイヤーの座標を更新
			m_PlayerActor.Position = p ;

			// まだ前進可能
			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// 垂直方向の接触判定

		// 上昇処理
		private ( bool, Vector3 ) ProcessJumping( float dy, Vector3 p0 )
		{
			bool isJumping = m_IsJumping ;
			Vector3 existPosition = p0 ;
			Vector3 afterPosition ;

			// 上昇量が 0.4f を超える場合は 0.4f ずつに分割して処理する
			float sdy, ldy = 0.4f ;
			while( dy >  0 )
			{
				sdy = dy ;
				if( sdy >  ldy )
				{
					sdy  = ldy ;
				}

				// 落下処理
				( isJumping, afterPosition ) = ProcessJumping_Slice( sdy, existPosition ) ;
				existPosition = afterPosition ;

				if( isJumping == false )
				{
					// 上昇終了(ヒット)
					break ;
				}

				dy -= ldy ;
				if( dy <= 0 )
				{
					// 上昇終了(移動量)
					break ;
				}
			}

			return ( isJumping, existPosition ) ;
		}

		// 上昇処理(最少)
		private ( bool, Vector3 ) ProcessJumping_Slice( float dy, Vector3 p0 )
		{
			Vector3 p1 = p0 + ( new Vector3( 0, dy, 0 ) ) ;	// 移動後

			// 移動後の体積の存在する範囲
			float py1 = p1.y + m_PlayerHeight ;

			int by  = ( int )py1 ;	// 上位置から判定対象ブロック座標を算出する
			if( ( py1 % 1 ) == 0 )
			{
				by -- ;
			}

			if( CheckVerticalHit( by, p1, m_PlayerRadius, false, false ) == false )
			{
				// 接触は無しなので上昇可能
				return ( true, p1 ) ;
			}
			else
			{
				// 接触するので上昇停止
				return ( false, new Vector3( p1.x, by - m_PlayerHeight, p1.z ) ) ;
			}
		}

		// 落下処理
		private ( bool, Vector3 ) ProcessFalling( float dy, Vector3 p0, bool isSneak )
		{
			bool isFalling = m_IsFalling ;
			Vector3 existPosition = p0 ;
			Vector3 afterPosition ;

			// 落下量が 0.4f を超える場合は 0.4f ずつに分割して処理する
			float sdy, ldy = 0.4f ;
			while( dy >  0 )
			{
				sdy = dy ;
				if( sdy >  ldy )
				{
					sdy  = ldy ;
				}

				// 落下処理
				( isFalling, afterPosition ) = ProcessFalling_Slice( sdy, existPosition, isSneak ) ;
				existPosition = afterPosition ;

				if( isFalling == false )
				{
					// 落下終了
					break ;
				}

				dy -= ldy ;
				if( dy <= 0 )
				{
					// 落下終了
					break ;
				}
			}

			return ( isFalling, existPosition ) ;
		}
		
		// 落下処理(最少)
		private ( bool, Vector3 ) ProcessFalling_Slice( float dy, Vector3 p0, bool isSneak )
		{
			Vector3 p1 = p0 - ( new Vector3( 0, dy, 0 ) ) ;	// 移動後

			int by  = ( int )p1.y ;	// 下位置から判定対象ブロック座標を算出する

			// ※座標はちょっとずつ変わっていて判定対象も異なる可能性があるので検査対象のブロック座標が同じでも毎回判定する

			//----------------------------------

			float radius = m_PlayerRadius ;
			if( isSneak == true )
			{
				// スニーク中
				radius *= 0.98f ;// 計算誤差があるの少し半径を狭める
			}
			
			//-----------------------------------

			if( CheckVerticalHit( by, p1, radius, isSneak, false ) == false )
			{
				// 接触は無しなので落下可能
				return ( true, p1 ) ;
			}
			else
			{
				// 接触するので落下終了
				return ( false, new Vector3( p1.x, by + 1, p1.z ) ) ;
			}
		}

		//-----------------------------------------------------------

		private List<( int, int )> m_FloorPattern = new List<(int, int)>() ;

		// 指定の高さ(ブロック単位)と現在位置の円内にヒットするブロックが存在するか判定する
		private bool CheckVerticalHit( int by, Vector3 p, float radius, bool isSneak, bool isDebug = false )
		{
			float fx0 = p.x - radius ;
			float fx1 = p.x + radius ;
			float fz0 = p.z - radius ;
			float fz1 = p.z + radius ;

			int bx0 = ( int )fx0 ;
			int bx1 = ( int )fx1 ;
			if( ( fx1 % 1 ) == 0 )
			{
				bx1 -- ;	// X+ が境界の場合の X+1 のブロックは除外
			}
			int bz0 = ( int )fz0 ;
			int bz1 = ( int )fz1 ;
			if( ( fz1 % 1 ) == 0 )
			{
				bz1 -- ;	// Z+ が境界の場合の Z+1 のブロックは除外
			}

			int bx, bz ;

			int mbx = ( int )p.x ;
			int mbz = ( int )p.z ;

			m_FloorPattern.Clear() ;

			for( bz  = bz0 ; bz <= bz1 ; bz ++ )
			{
				for( bx  = bx0 ; bx <= bx1 ; bx ++ )
				{
					if( GetBlock( bx, bz, by ) != 0 )
					{
						// 円の範囲に衝突するブロックが存在する

						// 足元のブロックのパターンを生成する(ブロックあり)
						m_FloorPattern.Add( ( bx - mbx, bz - mbz ) ) ;
					}
				}
			}

			//----------------------------------------------------------

			if( isDebug == true )
			{
				string s = string.Empty ;

				int x, z ;

				for( z  =  1 ; z >= -1 ; z -- )
				{
					for( x  = -1 ; x <= 1 ; x ++ )
					{
						if( m_FloorPattern.Contains( ( x, z ) ) == true )
						{
							s += "■" ;
						}
						else
						{
							s += "□" ;
						}
					}
					s += "\n" ;
				}

				Debug.Log( "PT:\n" + s ) ;
			}


			//----------------------------------------------------------

			if
			(
				m_FloorPattern.Contains( (  0,  0 ) ) == true ||
				m_FloorPattern.Contains( ( -1,  0 ) ) == true ||
				m_FloorPattern.Contains( ( +1,  0 ) ) == true ||
				m_FloorPattern.Contains( (  0, -1 ) ) == true ||
				m_FloorPattern.Contains( (  0, +1 ) ) == true
			)
			{
				// ヒットする
				return true ;
			}

			//----------------------------------------------------------
			// 斜め方向のチェックを行う

			Vector2 v0 = new Vector2( p.x, p.z ) ;

			if( m_FloorPattern.Contains( ( -1, -1 ) ) == true )
			{
				Vector2 v1 = new Vector2( mbx + 0, mbz + 0 ) ;

				if( ( v1 - v0 ).magnitude <  radius )
				{
					// ヒットする
					return true ;
				}
			}

			if( m_FloorPattern.Contains( ( +1, -1 ) ) == true )
			{
				Vector2 v1 = new Vector2( mbx + 1, mbz + 0 ) ;

				if( ( v1 - v0 ).magnitude <  radius )
				{
					// ヒットする
					return true ;
				}
			}

			if( m_FloorPattern.Contains( ( -1, +1 ) ) == true )
			{
				Vector2 v1 = new Vector2( mbx + 0, mbz + 1 ) ;

				double d = ( v1 - v0 ).magnitude ;
				if( d <  radius )
				{
					// ヒットする
					return true ;
				}
			}

			if( m_FloorPattern.Contains( ( +1, +1 ) ) == true )
			{
				Vector2 v1 = new Vector2( mbx + 1, mbz + 1 ) ;

				if( ( v1 - v0 ).magnitude <  radius )
				{
					// ヒットする
					return true ;
				}
			}

			// ヒットしない
			return false ;
		}


		// スニーク中の落下防止処理
		private ( bool, Vector3 ) ProcessStopFalling( Vector3 p0, Vector3 p1, bool isDebug = false )
		{
			// 移動先の足元のブロック座標を計算する

			// 足元のブロックパターンを検査する時は通常の半径で行う
			float radius = m_PlayerRadius ;

			int mbx = ( int )p1.x ;
			int mbz = ( int )p1.z ;

			float fx0 = p0.x - radius ;
			float fx1 = p0.x + radius ;
			float fz0 = p0.z - radius ;
			float fz1 = p0.z + radius ;

			int x0 = ( int )fx0 ;
			int x1 = ( int )fx1 ;
			if( ( fx1 % 1 ) == 0 )
			{
				x1 -- ;	// X+ が境界の場合の X+1 のブロックは除外
			}
			int z0 = ( int )fz0 ;
			int z1 = ( int )fz1 ;
			if( ( fz1 % 1 ) == 0 )
			{
				z1 -- ;	// Z+ が境界の場合の Z+1 のブロックは除外
			}

			int by  = ( int )( p0.y - 0.1f ) ;	// 下位置から判定対象ブロック座標を算出する

			int bx, bz ;

			m_FloorPattern.Clear() ;

			for( bz  = z0 ; bz <= z1 ; bz ++ )
			{
				for( bx  = x0 ; bx <= x1 ; bx ++ )
				{
					if( GetBlock( bx, bz, by ) != 0 )
					{
						// 円の範囲の下な衝突するブロックが存在する

						// 足元のブロックのパターンを生成する
						m_FloorPattern.Add( ( bx - mbx, bz - mbz ) ) ;
					}
				}
			}

			//----------------------------------------------------------

			if( isDebug == true )
			{
				string s = string.Empty ;

				for( bz  =  1 ; bz >= -1 ; bz -- )
				{
					for( bx  = -1 ; bx <=  1 ; bx ++ )
					{	
						if( m_FloorPattern.Contains( ( bx, bz ) ) == true )
						{
							s += "■" ;
						}
						else
						{
							s += "□" ;
						}
					}
					s += "\n" ;
				}

				Debug.Log( "FP:\n" + s ) ;
			}

			//----------------------------------------------------------

			// 以下のスニークによる距離抑制はスニーク用の半径で行う
			radius *= 0.96f ;

			//----------------------------------
			// 不要な判定対象ブロックを除去する
			// (斜め方向のブロックは、隣接するブロックがあれば判定対象から除去できる)

			// X-1,Z-1 のチェック
			if( m_FloorPattern.Contains( ( -1, -1 ) ) == true )
			{
				if( m_FloorPattern.Contains( (  0, -1 ) ) == true || m_FloorPattern.Contains( ( -1,  0 ) ) == true )
				{
					// X-1, Z-1 は不要
					m_FloorPattern.Remove( ( -1, -1 ) ) ;
				}
			}

			// X+1,Z-1 のチェック
			if( m_FloorPattern.Contains( ( +1, -1 ) ) == true )
			{
				if( m_FloorPattern.Contains( (  0, -1 ) ) == true || m_FloorPattern.Contains( ( +1,  0 ) ) == true )
				{
					// X+1, Z-1 は不要
					m_FloorPattern.Remove( ( +1, -1 ) ) ;
				}
			}

			// X-1,Z+1 のチェック
			if( m_FloorPattern.Contains( ( -1, +1 ) ) == true )
			{
				if( m_FloorPattern.Contains( (  0, +1 ) ) == true || m_FloorPattern.Contains( ( -1,  0 ) ) == true )
				{
					// X-1, Z+1 は不要
					m_FloorPattern.Remove( ( -1, +1 ) ) ;
				}
			}

			// X+1,Z+1 のチェック
			if( m_FloorPattern.Contains( ( +1, +1 ) ) == true )
			{
				if( m_FloorPattern.Contains( (  0, +1 ) ) == true || m_FloorPattern.Contains( ( +1,  0 ) ) == true )
				{
					// X+1, Z+1 は不要
					m_FloorPattern.Remove( ( +1, +1 ) ) ;
				}
			}

			//----------------------------------
			// 移動を抑制する

			//--------------
			// 角からの半径範囲に抑制

			float limit = radius * 0.7071f ;	// 45度

			// X-1,Z-1
			if( m_FloorPattern.Contains( ( -1, -1 ) ) == true )
			{
//				Debug.Log( "<color=#FFFF00>角:X-1,Z-1 : " + p1 + "</color>" ) ;
				return ( true, StopOnCorner( mbx + 0, mbz + 0, p1, radius, limit ) ) ;
			}

			// X+1,Z-1
			if( m_FloorPattern.Contains( ( +1, -1 ) ) == true )
			{
//				Debug.Log( "角:X+1,Z-1" ) ;
				return ( true, StopOnCorner( mbx + 1, mbz + 0, p1, radius, limit ) ) ;
			}

			// X-1,Z+1
			if( m_FloorPattern.Contains( ( -1, +1 ) ) == true )
			{
//				Debug.Log( "角:X-1,Z+1" ) ;
				return ( true, StopOnCorner( mbx + 0, mbz + 1, p1, radius, limit ) ) ;
			}

			// X+1,Z+1
			if( m_FloorPattern.Contains( ( +1, +1 ) ) == true )
			{
//				Debug.Log( "角:X+1,Z+1" ) ;
				return ( true, StopOnCorner( mbx + 1, mbz + 1, p1, radius, limit ) ) ;
			}

			//--------------
			// 端からの距離で抑制

			// X-1,Z=0
			if( m_FloorPattern.Contains( ( -1,  0 ) ) == true )
			{
//				Debug.Log( "端からの距離で抑制(押し戻し) -1,  0" ) ;
				float vx = ( mbx + 0 ) + radius ;
				if( p1.x >  vx )
				{
					p1.x  = vx ;
				}
			}

			// X+1,Z=0
			if( m_FloorPattern.Contains( ( +1,  0 ) ) == true )
			{
//				Debug.Log( "端からの距離で抑制(押し戻し) +1,  0" ) ;
				float vx = ( mbx + 1 ) - radius ;
				if( p1.x <  vx )
				{
					p1.x  = vx ;
				}
			}

			// X=0,Z-1
			if( m_FloorPattern.Contains( (  0, -1 ) ) == true )
			{
//				Debug.Log( "端からの距離で抑制(押し戻し)  0, -1" ) ;
				float vz = ( mbz + 0 ) + radius ;
				if( p1.z >  vz )
				{
					p1.z  = vz ;
				}
			}

			// X=0,Z+1
			if( m_FloorPattern.Contains( (  0, +1 ) ) == true )
			{
//				Debug.Log( "端からの距離で抑制(押し戻し)  0, +1" ) ;
				float vz = ( mbz + 1 ) - radius ;
				if( p1.z <  vz )
				{
					p1.z  = vz ;
				}
			}

			return ( true, p1 ) ;
		}

		// 角の進行を止める
		private Vector3 StopOnCorner( float cpx, float cpz, Vector3 pa, float radius, float limit )
		{
			Vector2 cv0 = new Vector2( cpx, cpz ) ;
			Vector2 cv1 = new Vector2( pa.x, pa.z ) ;

			float sx, sy, hx, hy ;

			Vector2 d = ( cv1 - cv0 ) ;
			if( d.magnitude >  radius )
			{
				float dX = Mathf.Abs( d.x ) ;
				float dY = Mathf.Abs( d.y ) ;

				if( dX >  dY && dY <  limit )
				{
//					Debug.Log( "Xが長い:" + d ) ;
					// 長い方を短くする
					sx = Sign( d.x ) ;
					hx = Mathf.Sqrt( radius * radius - d.y * d.y ) ;
					d.x = hx * sx ;
				}
				else
				if( dY >  dX && dX <  limit )
				{
//					Debug.Log( "Yが長い:" + d ) ;
					// 長い方を短くする
					sy = Sign( d.y ) ;
					hy = Mathf.Sqrt( radius * radius - d.x * d.x ) ;
					d.y = hy * sy ;
				}
				else
				{
//					Debug.Log( "XYが同じ長さ" ) ;
					d.x = Sign( d.x ) * limit ;
					d.y = Sign( d.y ) * limit ;
				}

				return new Vector3( cv0.x + d.x, pa.y, cv0.y + d.y ) ;
			}
			else
			{
				return pa ;
			}
		}
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 四角コリジョン情報
		/// </summary>
		public class FlatBox
		{
			public Vector2[] XZ = new Vector2[ 4 ] ;
			public float Y ;

			public FlatBox( Vector2[] xz, float y )
			{
				XZ = xz ;
				Y  = y ;
			}
		}
		
		// 指定の位置にブロックが作れるか確認する
		private bool IsCreateBlock( int x, int z, int y )
		{
			BlockPosition[] bps = GetPlayerCollision() ;
			if( bps.Length == 0 )
			{
				// ありえない
				return true ;
			}

//			Debug.LogWarning( "対象数:" + bps.Length ) ;

			foreach( var bp in bps )
			{
				if( bp.X == x && bp.Z == z && bp.Y == y )
				{
					return false ;	// 作れない
				}
			}

			return true ;
		}

		/// <summary>
		/// プレイヤーが含まれるブロックを取得する
		/// </summary>
		/// <returns></returns>
		private BlockPosition[] GetPlayerCollision()
		{
			Vector3 p0 = m_PlayerActor.Position ;				// 移動前

			// 移動後の体積の存在する範囲
			float fy0 = p0.y ;
			float fy1 = p0.y + 1.48f ;

			int y0 = ( int )fy0 ;	// 下位置から判定対象ブロック座標を算出する
			int y1 = ( int )fy1 ;
			if( ( fy1 % 1 ) == 0 )
			{
				y1 -- ;
			}

			// ※座標はちょっとずつ変わっていて判定対象も異なる可能性があるので検査対象のブロック座標が同じでも毎回判定する

			//-------------------------

			float radius = 0.4f ;

			float fx0 = p0.x - radius ;
			float fx1 = p0.x + radius ;
			float fz0 = p0.z - radius ;
			float fz1 = p0.z + radius ;

			int x0 = ( int )fx0 ;
			int x1 = ( int )fx1 ;
			if( ( fx1 % 1 ) == 0 )
			{
				x1 -- ;
			}
			int z0 = ( int )fz0 ;
			int z1 = ( int )fz1 ;
			if( ( fz1 % 1 ) == 0 )
			{
				z1 -- ;
			}

			int x, z, y ;

			List<Vector2[]> list = new List<Vector2[]>() ;
			Vector2[] fb ;


			for( z  = z0 ; z <= z1 ; z ++ )
			{
				for( x  = x0 ; x <= x1 ; x ++ )
				{
					// 体積内に入る(垂直方向で接触する可能性がある)場合のみ判定リストに追加する
					fb = new Vector2[]
					{
						new Vector2( x,     z     ),
						new Vector2( x,     z + 1 ),
						new Vector2( x + 1, z + 1 ),
						new Vector2( x + 1, z     )
					};

					list.Add( fb ) ;
				}
			}

			List<BlockPosition> bp = new List<BlockPosition>() ;

			Vector2 xz = new Vector2( p0.x, p0.z ) ;

			int i, l = list.Count ;
			for( i  = 0 ; i<  l ; i ++ )
			{
				// 接触の可能性あり
				if( CollisionCheckVertical( list[ i ], xz, radius ) == true )
				{
					for( y  = y0 ; y <= y1 ; y ++ )
					{
						bp.Add( new BlockPosition(){ X = ( int )list[ i ][ 0 ].x, Z = ( int )list[ i ][ 0 ].y, Y = y } ) ;
					}
				}
			}

			return bp.ToArray() ;
		}

		/// <summary>
		/// ２次元で単純に凸形と円が接触するか判定する(注意:凸形の頂点はループ状にすること(Ｚ型はＮＧ)
		/// </summary>
		/// <returns></returns>
		private bool CollisionCheckVertical( Vector2[] points, Vector2 center, float radius )
		{
			int i, l = points.Length ;

			Vector2 v0 ;

			// 凸形の各頂点と各線分と円の接触判定を行う

			// 誤差の関係で距離が微妙に少なくなってしまうので補正をかける
			float correctRadius = radius * 1f ;

			// 先に全頂点との接触判定を行う
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 頂点から円の中心までのベクトル
				v0 = center - points[ i ] ;

				if( v0.magnitude <  correctRadius )	// 半径と同じ距離の場合は接触とはみなさない(押し戻しの際に半径と同じ距離となるため)
				{
					// 頂点から円の中心までの距離が円の半径より短いのでその頂点と接触している(四隅のいずれかが円の内側に入る)
					return true ;
				}
			}

			//----------------------------------------------------------

			// ※先に全頂点との接触判定を優先的に行っておかないと押し戻しの時に問題が生じる
			// 　押し戻しは線分より頂点を優先して処理しなければならないため

			Vector2 v1 ;

			float distance, length_f, length_c ;

			// 次に全線分との接触判定を行う
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 頂点から円の中心までのベクトル
				v0 = center - points[ i ] ;

				// 今の頂点から次の頂点までのベクトル
				v1 = points[ ( i + 1 ) % l ] - points[ i ] ;
				length_f = v1.magnitude ;	// 線分の長さ
				v1.Normalize() ;			// 線分を単位ベクトル化

				// 外積から円が線分の外にあるか中にあるか判定する
				distance = v1.x * v0.y - v1.y * v0.x ;

				if( distance >=     radius )
				{
					// 完全に線分の外側にあって接触しない
					return false ;
				}
				else
				if( distance >= ( - radius ) )
				{
					// 円はこの線分上に位置している(線分の両端の頂点よりもさらに外側に外れている場合がある)

					// 内積で線分上にある中心からの垂直線との交点までの線分基準点からの距離を求める
					length_c = v1.x * v0.x + v1.y * v0.y ;

					if( length_c >= 0 && length_c <= length_f )
					{
						// 線分を垂直に広げた範囲内に円は位置しているので接触しているとみなせる
						return true ;
					}
					else
					{
						// 線分上にあっても頂点とも接触していないので完全に外側で接触しないとみなせる
						return false ;
					}
				}
				// 完全に内側にある場合は次の線分との接触判定となる
			}

			//----------------------------------------------------------

			// 全て線分の内側で且つ各線分より半径以上に離れている(すごく内側にある)
			return true ;
		}



		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 立方体の構造体
		/// </summary>
		public class CubeStructure
		{
			/// <summary>
			/// 頂点(８)
			/// </summary>
			public Vector3[]	Vertices ;

			/// <summary>
			/// インデックス(６×４)
			/// </summary>
			public byte[][]		Indices ;

			//----------------------------------------------------------

			public ( bool, Vector3 ) IsHitSphere( Vector3 center, float radius )
			{
				int fi, fl = Indices.Length ;
				int vi, vl ;


				Vector3 normal ;
				Vector3 v0, v1, v2 ;
				float distance ;
				Vector3 direction ;
				float width ;

				Vector3 crossingPoint ;

				List<int> nearFaces = new List<int>() ;

				int innerCount = 0 ;

				byte[] fvi ;
				for( fi  = 0 ; fi <  fl ; fi ++ )
				{
					// 面の頂点インデックス群
					fvi = Indices[ fi ] ;

					// 面の頂点数(最低３)
					vl = fvi.Length ;

					// 面の外側に向かって垂直な単位ベクトルを算出する
					v0 = Vertices[ fvi[ 0      ] ] ;
					v1 = Vertices[ fvi[ 1      ] ] ;
					v2 = Vertices[ fvi[ vl - 1 ] ] ;

					// 外積単位ベクトル(面の法線ベクトル)
					normal = Vector3.Cross( v1 - v0, v2 - v0 ).normalized ;

//					Debug.Log( "法線ベクトル:" + fi + " n = " + normal ) ;

					// 内積から面から球の中心への距離を算出
//					direction = center - v0 ;
//					Debug.Log( "面の基準頂点から球の中心へのベクトル:" + fi + " d = " + direction ) ;

					distance = Vector3.Dot( ( center - v0 ), normal ) ;

					if( distance >= radius )
					{
//						Debug.Log( "完全に面の外側:" + fi + " d = " + distance + " r = " + radius ) ;

						// １つの面であっても外側にあれば接触はしないと判定する
						return ( false, center ) ;
					}

//					Debug.Log( "面からの距離: " + fi + " d= " + distance ) ;

					if( distance <  radius && distance >  ( - radius ) )
					{
						// 面と接触する

						// 接触点が面の内側にあるならその面とだけ接触すると判定する

						crossingPoint = center - ( normal * distance ) ;

						for( vi  = 0 ; vi <  vl ; vi ++ )
						{
							v0 = Vertices[ fvi[   vi            ] ] ;
							v1 = Vertices[ fvi[ ( vi + 1 ) % vl ] ] ;

							// 外積が面の外側と内側のどちらを向いているか算出する
							direction = Vector3.Cross( v1 - v0, crossingPoint - v0 ) ;

							// 面の外側を向いている垂直単位ベクトルと内積を計算し接触点が面の内側か外側か判定する
							if( Vector3.Dot( normal, direction ) <  0 )
							{
								// 面の外側にある
								break ;
							}
						}

						if( vi >= vl )
						{
							// すべての面の線分との判定で内側にあると判定された
							// すなわち接触点は面の内側にある

//							Debug.Log( "接触点は面内部にある:" + fi ) ;

							// この面とのみ接触すると判断し押し戻しを行う
							center += ( normal * ( radius - distance ) ) ;

							return ( true, center ) ;
						}

						// 接触点は面の内側にはないのでこの面を角線分との接触候補とする
						nearFaces.Add( fi ) ;
					}
					else
					{
						// 球は完全の面の内側にある
						innerCount ++ ;
					}
				}

				// すべての面との検査完了
				//---------------------------------

//				Debug.Log( "近くにある面の数:" + nearFaces.Count ) ;

				if( innerCount == fl )
				{
//					Debug.Log( "球は完全に立方体の内側にある" ) ;

					// すべての面の内側にあるため押し戻しができない
					return ( true, center ) ;
				}

				//---------------------------------

				if( nearFaces.Count >  3 )
				{
					// ４つ以上の面と近接関係にある場合は球と立方体が融合した状態なので押し戻しができない(立方体より球が大きい等)
					return ( true, center ) ;
				}

				//---------------------------------------------------------

				if( nearFaces.Count == 1 )
				{
					// 球は立方体に近いだけで接触はしていない
					return ( false, center ) ;
				}

				if( nearFaces.Count == 2 )
				{
					// 球は立方体の２つの面に近いため立方体の端(線分)と接触している可能性がある
					List<byte> shareIndices = new List<byte>() ;

					fvi = Indices[ nearFaces[ 0 ] ] ;

					var fvi1 = Indices[ nearFaces[ 1 ] ] ;

					foreach( var index in fvi )
					{
						if( fvi1.Contains( index ) == true )
						{
							// こんインデックスは対象の面全てに含まれる
							shareIndices.Add( index ) ;
						}
					}

					// 共有頂点は必ず２点だけのはずである
					if( shareIndices.Count != 2 )
					{
						Debug.LogWarning( "異常発生:" + shareIndices.Count ) ;
						return ( true, center ) ;
					}

					// 端の頂点
					v0 = Vertices[ shareIndices[ 0 ] ] ;
					v1 = Vertices[ shareIndices[ 1 ] ] ;

					// 線分中の球の中心に最も近い場所(垂直)を算出する
					normal = ( v1 - v0 ).normalized ;

					distance = Vector3.Dot( normal, center - v0 ) ;

					// 線分の長さ
					width = ( v1 - v0 ).magnitude ;
					if( distance <  0 || distance >  width )
					{
						// この場合は近い面は３つにならなければおかしい
						Debug.LogWarning( "異常発生: width = " + width + " distance = " + distance ) ;
						return ( false, center ) ;
					}

					// 線分の中の球の中心に最も近い点
					crossingPoint = v0 + ( normal * distance ) ;

					// 線分の中の球の中心に最も近い点から球の中心までの距離
					distance = ( center - crossingPoint ).magnitude ;

					if( distance >= radius )
					{
						// 接触しない
						return ( false, center ) ;
					}

					// 接触するので押し戻す
					normal = ( center - crossingPoint ).normalized ;
					center += ( normal * ( radius - distance ) ) ;

//					Debug.Log( "線に対して押し戻しを行う : " + ( radius - distance ) ) ;

					return ( true, center ) ;
				}

				if( nearFaces.Count == 3 )
				{
					// 球は立方体の３つの面に近いため立方体の角(頂点)と接触している可能性がある
					List<byte> shareIndices = new List<byte>() ;

					fvi = Indices[ nearFaces[ 0 ] ] ;

					var fvi1 = Indices[ nearFaces[ 1 ] ] ;
					var fvi2 = Indices[ nearFaces[ 2 ] ] ;

					foreach( var index in fvi )
					{
						if( fvi1.Contains( index ) == true && fvi2.Contains( index ) == true )
						{
							// こんインデックスは対象の面全てに含まれる
							shareIndices.Add( index ) ;
						}
					}

					// 共有頂点は必ず１点だけのはずである
					if( shareIndices.Count != 1 )
					{
						Debug.LogWarning( "異常発生:" + shareIndices.Count ) ;
						return ( true, center ) ;
					}

					// 角の頂点
					crossingPoint = Vertices[ shareIndices[ 0 ] ] ;

					// 角の頂点から球の中心までの距離
					distance = ( center - crossingPoint ).magnitude ;

					if( distance >= radius )
					{
						// 接触しない
						return ( false, center ) ;
					}

					// 接触するので押し戻す
					normal = ( center - crossingPoint ).normalized ;
					center += ( normal * ( radius - distance ) ) ;

//					Debug.Log( "点に対して押し戻しを行う:" + ( radius - distance ) ) ;

					return ( true, center ) ;
				}

				// 通常はここには来ない
				Debug.LogWarning( "異常発生" ) ;
				return ( false, center ) ;
			}
		}

		//-----------------------------------------------------------

		// 球と立方体の接触判定及び押し戻しを行う
		private ( bool, Vector3 ) IsHitSphereAndCube( Vector3 center, float radius, CubeStructure cube )
		{
			// いずれかの面より外側にあれば接触しないと判定する
			return cube.IsHitSphere( center, radius ) ;
		}

		// 球と立方体の接触判定及び押し戻しを行う
		private ( bool, Vector3 ) IsHitSphereAndCube( Vector3 center, float radius, int bx, int bz, int by )
		{
			float bx0 = bx ;
			float bx1 = bx + 1 ;

			float bz0 = bz ;
			float bz1 = bz + 1 ;

			float by0 = by ;
			float by1 = by + 1 ;

			//----------------------------------

			var cube = new CubeStructure() ;

			cube.Vertices = new Vector3[]
			{
				new Vector3( bx0, by0, bz0 ),
				new Vector3( bx1, by0, bz0 ),
				new Vector3( bx0, by0, bz1 ),
				new Vector3( bx1, by0, bz1 ),

				new Vector3( bx0, by1, bz0 ),
				new Vector3( bx1, by1, bz0 ),
				new Vector3( bx0, by1, bz1 ),
				new Vector3( bx1, by1, bz1 ),
			} ;

			cube.Indices = new byte[][]
			{
				new byte[]{ 0, 1, 3, 2 },
				new byte[]{ 4, 5, 1, 0 },
				new byte[]{ 6, 7, 5, 4 },
				new byte[]{ 2, 3, 7, 6 },
				new byte[]{ 6, 4, 0, 2 },
				new byte[]{ 5, 7, 3, 1 },
			} ;

			// いずれかの面より外側にあれば接触しないと判定する
			return cube.IsHitSphere( center, radius ) ;
		}

		// 指定した位置の周辺のブロックと接触があるか判定する
		private ( bool, Vector3 ) IsHotSphereAdnCube( Vector3 position, float radius )
		{
			// 周囲のブロックと接触判定を行う

			float px0 = position.x - radius ;
			float px1 = position.x + radius ;

			float pz0 = position.z - radius ;
			float pz1 = position.z + radius ;

			float py0 = position.y - radius ;
			float py1 = position.y + radius ;

			int bx0 = ( int )px0 ;
			int bx1 = ( int )px1 ;
			if( ( px1 % 1 ) == 0 )
			{
				bx1 -- ; 
			}

			int bz0 = ( int )pz0 ;
			int bz1 = ( int )pz1 ;
			if( ( pz1 % 1 ) == 0 )
			{
				bz1 -- ; 
			}

			int by0 = ( int )py0 ;
			int by1 = ( int )py1 ;
			if( ( py1 % 1 ) == 0 )
			{
				by1 -- ; 
			}

			int xMin = WorldSettings.WORLD_X_MIN ;
			if( bx0 <  xMin )
			{
				bx0  = xMin ;
			}
			int xMax = WorldSettings.WORLD_X_MAX - 1 ;
			if( bx1 >  xMax )
			{
				bx1  = xMax ;
			}

			int zMin = WorldSettings.WORLD_Z_MIN ;
			if( bz0 <  zMin )
			{
				bz0  = zMin ;
			}
			int zMax = WorldSettings.WORLD_Z_MAX - 1 ;
			if( bz1 >  zMax )
			{
				bz1  = zMax ;
			}

			int yMin = WorldSettings.WORLD_Y_MIN ;
			if( by0 <  yMin )
			{
				by0  = yMin ;
			}
			int yMax = WorldSettings.WORLD_Y_MAX - 1 ;
			if( by1 >  yMax )
			{
				by1  = yMax ;
			}

			int bx, bz, by ;

			bool isTotalHit = false, isBlockHit ;
			Vector3 movedPosition = position ;

			for( by  = by0 ; by <= by1 ; by ++ )
			{
				for( bz  = bz0 ; bz <= bz1 ; bz ++ )
				{
					for( bx  = bx0 ; bx <= bx1 ; bx ++ )
					{
						if( GetBlock( bx, bz, by ) != 0 )
						{
							// ブロックが存在する
							( isBlockHit, movedPosition ) = IsHitSphereAndCube( movedPosition, radius, bx, bz, by ) ;
							if( isBlockHit == true )
							{
								isTotalHit = true ;
							}
						}
					}
				}
			}

			return ( isTotalHit, movedPosition ) ;
		}
	}
}
