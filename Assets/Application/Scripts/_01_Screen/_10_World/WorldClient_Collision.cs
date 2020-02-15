using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// クライアント(コリジョン)
	/// </summary>
	public partial class WorldClient : MonoBehaviour
	{
		private bool ProcessMoving( Vector2 velocity )
		{
//			Vector2 extraVelocity ;

			int i ;
			for( i  = 0 ; i <  3 ; i ++ )
			{
				if( ProcessMovingMinimal( velocity, out Vector2 additionalVelocity ) == true )
				{
					// 移動完了(接触なし)
					return true ;
				}

				if( additionalVelocity.magnitude == 0 )
				{
					// 移動完了(接触あり)
					return false ;
				}

				// ズリ移動あり
				velocity = additionalVelocity ;
			}

			return false ;
		}

		/// <summary>
		/// 最小単位の水平方向移動
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="bx"></param>
		/// <param name="bz"></param>
		/// <returns></returns>
		private bool ProcessMovingMinimal( Vector2 velocity, out Vector2 additionalVelocity )
		{
			// 追加のズリ移動
			additionalVelocity = Vector2.zero ;

			float radius = 0.4f ;

			Vector3 p0 = m_Player.position ;
			Vector3 p1 = p0 + new Vector3( velocity.x, 0, velocity.y ) ;

			float p0_x0, p0_x1, p0_z0, p0_z1 ;
			float p1_x0, p1_x1, p1_z0, p1_z1 ;
			float px0, px1, pz0, pz1 ;

			int bx0, bx1, bz0, bz1 ;

			// 検査対象ブロックは、移動前から移動後の円と接触する全てのブロック

			p0_x0 = p0.x - radius ;
			p0_x1 = p0.x + radius ;
			p0_z0 = p0.z - radius ;
			p0_z1 = p0.z + radius ;

			p1_x0 = p1.x - radius ;
			p1_x1 = p1.x + radius ;
			p1_z0 = p1.z - radius ;
			p1_z1 = p1.z + radius ;

			// 最小X
			if( p0_x0 <= p1_x0 )
			{
				px0 = p0_x0 ;
			}
			else
			{
				px0 = p1_x0 ;
			}

			// 最大X
			if( p0_x1 >= p1_x1 )
			{
				px1 = p0_x1 ;
			}
			else
			{
				px1 = p1_x1 ;
			}

			// 最小Z
			if( p0_z0 <= p1_z0 )
			{
				pz0 = p0_z0 ;
			}
			else
			{
				pz0 = p1_z0 ;
			}

			// 最大Z
			if( p0_z1 >= p1_z1 )
			{
				pz1 = p0_z1 ;
			}
			else
			{
				pz1 = p1_z1 ;
			}


			bx0 = ( int )px0 ;
			if( bx0 <      0 )
			{
				bx0  =     0 ;
			}

			bx1 = ( int )px1 ;
			if( ( px1 % 1 ) == 0 )
			{
				bx1 -- ;
			}
			if( bx1 >  65535 )
			{
				bx1  = 65535 ;
			}

			bz0 = ( int )pz0 ;
			if( bz0 <      0 )
			{
				bz0  =     0 ;
			}

			bz1 = ( int )pz1 ;
			if( ( pz1 % 1 ) == 0 )
			{
				bz1 -- ;
			}
			if( bz1 >  65535 )
			{
				bz1  = 65535 ;
			}

			// 移動後の体積の存在する範囲
			float py0 = p0.y ;
			float py1 = p0.y + 1.48f ;

			int by0 = ( int )py0 ;
			int by1 = ( int )py1 ;	// 上位置から判定対象ブロック座標を算出する
			if( ( py1 % 1 ) == 0 )
			{
				by1 -- ;
			}

			// ※座標はちょっとずつ変わっていて判定対象も異なる可能性があるので検査対象のブロック座標が同じでも毎回判定する

			//-------------------------


			int x, z, y ;
			int b ;

			List<CollisionLine>		lines = new List<CollisionLine>() ;		// 壁用
			List<CollisionPoint>	points = new List<CollisionPoint>() ;	// 角用

			CollisionLine cl ;
			CollisionPoint cp ;

			float ty0, ty1 ;


			for( y  = by0 ; y <= by1 ; y ++ )
			{
				for( z  = bz0 ; z <= bz1 ; z ++ )
				{
					for( x  = bx0 ; x <= bx1 ; x ++ )
					{
						b = GetBlock( x, z, y ) ;

						if( b != 0 )
						{
							ty0 = y ;		// ひとまずの下面の高さ
							ty1 = y + 1 ;	// ひとまずの上面の高さ

							if( ( ty1 <  py0 || ty0 >  py1 ) == false )
							{
								// 体積内に入る(垂直方向で接触する可能性がある)場合のみ判定リストに追加する

								// 壁用の線コリジョンを追加する

								// 西
								if( GetBlock( x - 1, z, y ) == 0 )
								{
									cl = new CollisionLine()
									{
										Points = new Vector2[]
										{
											new Vector2( x,     z     ),
											new Vector2( x,     z + 1 )
										}
									} ;
									lines.Add( cl ) ;
								}

								// 北
								if( GetBlock( x, z + 1, y ) == 0 )
								{
									cl = new CollisionLine()
									{
										Points = new Vector2[]
										{
											new Vector2( x,     z + 1 ),
											new Vector2( x + 1, z + 1 )
										}
									} ;
									lines.Add( cl ) ;
								}

								// 東
								if( GetBlock( x + 1, z, y ) == 0 )
								{
									cl = new CollisionLine()
									{
										Points = new Vector2[]
										{
											new Vector2( x + 1, z + 1 ),
											new Vector2( x + 1, z     )
										}
									} ;
									lines.Add( cl ) ;
								}

								// 南
								if( GetBlock( x, z - 1, y ) == 0 )
								{
									cl = new CollisionLine()
									{
										Points = new Vector2[]
										{
											new Vector2( x + 1, z     ),
											new Vector2( x,     z     )
										}
									} ;
									lines.Add( cl ) ;
								}

								// 角用の点コリジョンを追加する

								// 南西
								if
								(
									GetBlock( x,     z - 1, y ) == 0 &&
									GetBlock( x - 1, z - 1, y ) == 0 &&
									GetBlock( x - 1, z,     y ) == 0
								)
								{
									cp = new CollisionPoint()
									{
										Point = new Vector2( x,     z    ),
										Lines = new Vector2[]
										{
											new Vector2( -1,  0 ),
											new Vector2(  0,  1 )
										}
									} ;
									points.Add( cp ) ;
								}

								// 西北
								if
								(
									GetBlock( x - 1, z,     y ) == 0 &&
									GetBlock( x - 1, z + 1, y ) == 0 &&
									GetBlock( x,     z + 1, y ) == 0
								)
								{
									cp = new CollisionPoint()
									{
										Point = new Vector2( x,     z + 1 ),
										Lines = new Vector2[]
										{
											new Vector2(  0,  1 ),
											new Vector2(  1,  0 )
										}
									} ;
									points.Add( cp ) ;
								}

								// 北東
								if
								(
									GetBlock( x,     z + 1, y ) == 0 &&
									GetBlock( x + 1, z + 1, y ) == 0 &&
									GetBlock( x + 1, z,     y ) == 0
								)
								{
									cp = new CollisionPoint()
									{
										Point = new Vector2( x + 1, z + 1 ),
										Lines = new Vector2[]
										{
											new Vector2(  1,  0 ),
											new Vector2(  0, -1 )
										}
									} ;
									points.Add( cp ) ;
								}

								// 東南
								if
								(
									GetBlock( x + 1, z,     y ) == 0 &&
									GetBlock( x + 1, z - 1, y ) == 0 &&
									GetBlock( x,     z - 1, y ) == 0
								)
								{
									cp = new CollisionPoint()
									{
										Point = new Vector2( x + 1, z     ),
										Lines = new Vector2[]
										{
											new Vector2(  0, -1 ),
											new Vector2( -1,  0 )
										}
									} ;
									points.Add( cp ) ;
								}
							}
						}
					}
				}
			}

			if( lines.Count == 0 && points.Count == 0 )
			{
				// 移動ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

//			Debug.LogWarning( "登録されたコライダー数:" + list.Count ) ;

			// 接触判定を行う

			Vector2 minimalVelocity = velocity ;
			Vector2 resultVelocity ;
			Vector2 extraVelocity ;

			Vector2 xz = new Vector2( p0.x, p0.z ) ;
			bool isHit = false ;

			if( lines.Count >  0 )
			{
				// 線(壁)との接触の可能性あり
				if( CheckCollisionLine( lines.ToArray(), xz, radius, velocity, out resultVelocity, out extraVelocity ) == true )
				{
					isHit = true ;
					minimalVelocity = resultVelocity ;
					additionalVelocity = extraVelocity ;
				}
			}

			if( points.Count >  0 )
			{
				// 点(角)との接触の可能性あり
				if( CheckCollisionPoint( points.ToArray(), xz, radius, velocity, out resultVelocity, out extraVelocity ) == true )
				{
					if( resultVelocity.magnitude <  minimalVelocity.magnitude )
					{
						isHit = true ;
						minimalVelocity = resultVelocity ;
						additionalVelocity = extraVelocity ;
					}
				}
			}

			if( isHit == false )
			{
				// 接触しないので移動ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

//			Debug.LogWarning( "接触した上での移動量:" + minD + " / " + delta ) ;

			// 接触する(最も小さい移動を行う)
			m_Player.position = p0 + new Vector3( minimalVelocity.x, 0, minimalVelocity.y ) ;
			return false ;	// 移動不可
		}


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

		/// <summary>
		/// ジャンプ処理を行う(最小単位)＝突き抜け防止のため
		/// </summary>
		/// <param name="dy"></param>
		/// <returns></returns>
		private bool ProcessJumping( float dy )
		{
			Vector3 p0 = m_Player.position ;				// 移動前
			Vector3 p1 = p0 + ( new Vector3( 0, dy, 0 ) ) ;	// 移動後

			// 移動後の体積の存在する範囲
			float py0 = p0.y + 1.48f ;
			float py1 = p1.y + 1.48f ;

			int y  = ( int )py1 ;	// 上位置から判定対象ブロック座標を算出する
			if( ( py1 % 1 ) == 0 )
			{
				y -- ;
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

			int x, z ;
			int b ;

			List<FlatBox> list = new List<FlatBox>() ;
			FlatBox fb ;

			float ty ;

			for( z  = z0 ; z <= z1 ; z ++ )
			{
				for( x  = x0 ; x <= x1 ; x ++ )
				{
					b = GetBlock( x, z, y ) ;

					if( b != 0 )
					{
						ty = y ;	// ひとまずの下面の高さ(実際はセル内の任意の上向きの面)

						if( ty >= py0 && ty <  py1 )
						{
							// 体積内に入る(垂直方向で接触する可能性がある)場合のみ判定リストに追加する
							fb = new FlatBox( new Vector2[]
							{
								new Vector2( x,     z     ),
								new Vector2( x,     z + 1 ),
								new Vector2( x + 1, z + 1 ),
								new Vector2( x + 1, z     )
							}, ty ) ;

							list.Add( fb ) ;
						}
					}
				}
			}

			if( list.Count == 0 )
			{
				// 上昇ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

			// 接触判定を行う
			float minY = Mathf.Infinity ;
			int i, l = list.Count ;

			Vector2 xz = new Vector2( p1.x, p1.z ) ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				// 接触の可能性あり
				if( CollisionCheckVertical( list[ i ].XZ, xz, radius ) == true )
				{
					// 接触する(より小さい方)
					if( list[ i ].Y <  minY )
					{
						minY  = list[ i ].Y ;
					}
				}
			}

			if( minY == Mathf.Infinity )
			{
				// 接触しないので上昇ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

			// 接触する
			m_Player.position = new Vector3( p1.x, minY - 1.48f, p1.z ) ;
			return false ;
		}

		/// <summary>
		/// 重力落下処理を行う(最小単位)＝突き抜け防止のため
		/// </summary>
		/// <param name="dy"></param>
		/// <returns></returns>
		private bool ProcessFalling( float dy )
		{
			Vector3 p0 = m_Player.position ;				// 移動前
			Vector3 p1 = p0 - ( new Vector3( 0, dy, 0 ) ) ;	// 移動後

			// 移動後の体積の存在する範囲
			float py1 = p0.y ;
			float py0 = p1.y ;

			int y  = ( int )py0 ;	// 下位置から判定対象ブロック座標を算出する

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

			int x, z ;
			int b ;

			List<FlatBox> list = new List<FlatBox>() ;
			FlatBox fb ;

			float ty ;

			for( z  = z0 ; z <= z1 ; z ++ )
			{
				for( x  = x0 ; x <= x1 ; x ++ )
				{
					b = GetBlock( x, z, y ) ;

					if( b != 0 )
					{
						ty = y + 1.0f ;	// ひとまずの上面の高さ(実際はセル内の任意の上向きの面)

						if( ty >  py0 && ty <= py1 )
						{
							// 体積内に入る(垂直方向で接触する可能性がある)場合のみ判定リストに追加する
							fb = new FlatBox( new Vector2[]
							{
								new Vector2( x,     z     ),
								new Vector2( x,     z + 1 ),
								new Vector2( x + 1, z + 1 ),
								new Vector2( x + 1, z     )
							}, ty ) ;

							list.Add( fb ) ;
						}
					}
				}
			}

			if( list.Count == 0 )
			{
				// 落下ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

			// 接触判定を行う
			float maxY = -1 ;
			int i, l = list.Count ;

			Vector2 xz = new Vector2( p1.x, p1.z ) ;

			for( i  = 0 ; i<  l ; i ++ )
			{
				// 接触の可能性あり
				if( CollisionCheckVertical( list[ i ].XZ, xz, radius ) == true )
				{
					// 接触する(より大きい方)
					if( list[ i ].Y >  maxY )
					{
						maxY  = list[ i ].Y ;
					}
				}
			}

			if( maxY <  0 )
			{
				// 接触しないので落下ＯＫ
				m_Player.position = p1 ;
				return true ;
			}

			// 接触する
			m_Player.position = new Vector3( p1.x, maxY, p1.z ) ;
			return false ;
		}

		private bool IsCreateBlock( int x, int z, int y )
		{
			BlockPosition[] bps = GetPlayerCollision() ;
			if( bps.Length == 0 )
			{
				// ありえない
				return true ;
			}

			Debug.LogWarning( "対象数:" + bps.Length ) ;

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
			Vector3 p0 = m_Player.position ;				// 移動前

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

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// ２次元で単純に凸形と円が接触するか判定する(注意:凸形の頂点はループ状にすること(Ｚ型はＮＧ)
		/// </summary>
		/// <returns></returns>
		private bool CollisionCheckVertical( Vector2[] points, Vector2 center, float radius )
		{
			int i, l = points.Length ;

			Vector2 v0 ;
			Vector2 v1 ;

			float distance, length_f, length_c ;

			// 凸形の各頂点と各線分と円の接触判定を行う

			// 先に全頂点との接触判定を行う
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 頂点から円の中心までのベクトル
				v0 = center - points[ i ] ;

				if( v0.magnitude <= ( radius + 0.01f ) )
				{
					// 頂点から円の中心までの距離が円の半径より短いのでその頂点と接触している
					return true ;
				}
			}

			// ※先に全頂点との接触判定を優先的に行っておかないと押し戻しの時に問題が生じる
			// 　押し戻しは線分より頂点を優先して処理しなければならないため

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

				if( distance >      ( radius + 0.01f )   )
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

			// 全て線分の内側で且つ各線分より半径以上に離れている(すごく内側にある)
			return true ;
		}

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 線分との当たり判定用
		/// </summary>
		public class CollisionLine
		{
			public Vector2[] Points ;
		}

		/// <summary>
		/// ２次元で単純に凸形と円が接触するか判定する(注意:凸形の頂点はループ状にすること(Ｚ型はＮＧ)　移動方向ベクトルを渡し実際に移動可能な座標を返す
		/// </summary>
		/// <returns></returns>
		private bool CheckCollisionLine( CollisionLine[] lines, Vector2 oldPosition, float radius, Vector2 velocity, out Vector2 resultVelocity, out Vector2 extraVelocity )
		{
			// 現在位置が線にめり込んでいると簡単にすり抜けが起きてしまうので



			// 注意：円の移動量が大きい(凸形が小さい)と線分と交差もしないすり抜け現象が起きてしまう事が避けられない
			// 　　　よって計算負荷を軽くする意味でも円の一度の移動量を小さくし凸形を一定以上の大きさにする事をルール付けする
			// 　　　円は一回の移動量は頂点か線分に必ず接触する量でなければならない

			// 注意：移動可能距離は頂点との接触が必ずしも最短というわけではない
			// 　　　頂点と線分の全てで接触確認を行い
			// 　　　その中の接触するもので最も短い移動距離のものを選択する必要がある

			// 注意：総合的な結果がおかしくなるので実際は接触しない頂点との接触判定を行ってはならない

			// 移動元と移動先から移動ボリュームを作る

			Vector2 newPosition = oldPosition + velocity ;

			Vector2 direction = velocity.normalized ;	// 移動量ベクトルの単位ベクトル(移動方向ベクトル)

			Vector2 mv_svl = new Vector2( - direction.y,   direction.x ) ;
			Vector2 mv_svr = new Vector2(   direction.y, - direction.x ) ;

			Vector2 oep = oldPosition ; // - ( direction * radius ) ;
			Vector2 mv_pl0 = oep + ( mv_svl * radius ) ;
			Vector2 mv_pr1 = oep + ( mv_svr * radius ) ;

			Vector2 nep = newPosition ; // + ( direction * radius ) ;
			Vector2 mv_pl1 = nep + ( mv_svl * radius ) ;
			Vector2 mv_pr0 = nep + ( mv_svr * radius ) ;


			// 移動ボリュームの左右の線分
			Vector2 mv_vl = mv_pl1 - mv_pl0 ;
			Vector2 mv_vr = mv_pr1 - mv_pr0 ;


			int i, l = lines.Length ;

			Vector2 cl_p0 ;
			Vector2 cl_p1 ;
			Vector2 cl_v ;

			Vector2 v0 ;
			Vector2 v1 ;


			Vector2 mv_lr ;

			Vector2 v2 ;

			float dl, dc, dh, dr ;
			float cos_v ;

			Vector2 cl_cp ;

			Vector2 minimalVelocity = velocity ;
			Vector2 mv ;

			bool isHit = false ;

			extraVelocity = Vector2.zero ;

			//-------------------------------------------------

			// 各コリジョン線分が判定対象か確認する
			for( i  = 0 ; i <  l ; i ++ )
			{
				cl_p0 = lines[ i ].Points[ 0 ] ;
				cl_p1 = lines[ i ].Points[ 1 ] ;
				cl_v = cl_p1 - cl_p0 ;

				// この線分が移動方向に対して裏側を見せていたら無視
				if( ( direction.x * ( - cl_v.y ) + direction.y * cl_v.x ) >= 0 )
				{
					continue ;
				}

				//---------------------

//				Debug.LogWarning( "1)有効な線分:" + i + " " + ( direction.x * ( - cl_v.y ) + direction.y * cl_v.x ) + " " + direction + " " + ( new Vector2( - cl_v.y, cl_v.x ) ) ) ;

				// 線と線の隙間のすり抜けが発生するため０＝接線は接触扱いにする

				// 線分の２点共が移動ボリュームに左のラインより外にあれば無視
				v0 = cl_p0 - mv_pl0 ;
				v1 = cl_p1 - mv_pl0 ;

				if
				(
					( mv_vl.x * v0.y - mv_vl.y * v0.x ) >  0 &&
					( mv_vl.x * v1.y - mv_vl.y * v1.x ) >  0
				)
				{
					continue ;
				}

				// 線分の２点共が移動ボリュームに右のラインより外にあれば無視
				v0 = cl_p0 - mv_pr0 ;
				v1 = cl_p1 - mv_pr0 ;

				if
				(
					( mv_vr.x * v0.y - mv_vr.y * v0.x ) >  0 &&
					( mv_vr.x * v1.y - mv_vr.y * v1.x ) >  0
				)
				{
					continue ;
				}

//				Debug.LogWarning( "有効な線分:[" + i +"]" ) ;

				//---------------------

				// 線分の長さ取得と単位ベクトル化
				v0 = cl_p1 - cl_p0 ;
//				dl = v0.magnitude ;
				v0.Normalize() ;

				//---------------------

				// 移動先の円と線分の接触判定
				v1 = newPosition - cl_p0 ;

				if
				(
					( cl_p0 - newPosition ).magnitude >= radius &&
					( cl_p1 - newPosition ).magnitude >= radius
				)
				{
					// 線分の頂点が２つ共に移動先の円の外

					dh = v0.x * v1.y - v0.y * v1.x ;	// 垂直線の長さ

					dc = v0.x * v1.x + v0.y * v1.y ;
					cl_cp  = cl_p0 + ( v0 * dc ) ;	// 円の中心から線分に垂直線を下ろした時に交差するポイント

					mv_lr = mv_pr0 - mv_pl1 ;
					v2 = cl_cp - mv_pl1 ;
					if( ( mv_lr.x * v2.y - mv_lr.y * v2.x ) >= 0 && dh >= radius )
					{
						continue ;	// 線分は移動先の円より前方にあって交差しない
					}
				}

				//---------------------

//				Debug.LogWarning( "有効な線分:[" + i +"]" ) ;

				// 移動元の円と線分の接触判定(面が裏側になるので半径の符号を反転させて判定)
				v1 = oldPosition - cl_p0 ;

				if
				(
					( cl_p0 - oldPosition ).magnitude >= radius &&
					( cl_p1 - oldPosition ).magnitude >= radius
				)
				{
					// 線分の頂点が２つ共に移動先の円の外


					dh = v0.x * v1.y - v0.y * v1.x ;	// 垂直線の長さ

					dc = v0.x * v1.x + v0.y * v1.y ;
					cl_cp  = cl_p0 + ( v0 * dc ) ;	// 円の中心から線分に垂直線を下ろした時に交差するポイント

					mv_lr = mv_pl0 - mv_pr1 ;
					v2 = cl_cp - mv_pr1 ;
					if( ( mv_lr.x * v2.y - mv_lr.y * v2.x ) >= 0 && dh >= radius )
					{
						continue ;	// 線分は移動先の円より前方にあって交差しない
					}
				}

//				Debug.LogWarning( "有効な線分:[" + i + "] " ) ;

				//---------------------------------------------
				// 有効な線分に対する最も近い接触点を算出する

				// １点が左右の領域内に入っているかどうか

				// 円に最も近い線分上の点は円の中心から線分への垂直線の交点
				// これは円がどちらの方向へ移動しようが変わらない

				v0 = cl_p1 - cl_p0 ;
				dl = v0.magnitude ;
				v0.Normalize() ;
				v1 = oldPosition - cl_p0 ;

				dh = v0.x * v1.y - v0.y * v1.x ;	// sin : 円の中心から線分への垂直距離

//				Debug.LogWarning( "線分への距離:" + dh ) ;
				// 線分の逆法線(裏側向き)
				v1.x =   v0.y ;
				v1.y = - v0.x ;

//				Debug.LogWarning( "垂直線:" + v1 ) ;
				// 線分の法線(逆)と移動方向ベクトルの内積(完全一致なら1)
				cos_v = direction.x * v1.x + direction.y * v1.y ;

				// 円の線分に一番近い外周の点から線分への垂直距離
				dr = dh - radius ;

//				Debug.LogWarning( "線分との角度[" + i +"]:" + cos_v ) ;
				if( cos_v <  1 && cos_v >  0 )
				{
					// 線に対して完全に垂直に進行する場合は移動は停止するがそうでなければ角度に応じて左右にズリ移動していく

					// 進行方向に対する移動可能距離
					dr /= cos_v ;
				}

				// 円の線分に一番近い外周から移動方向へベクトルを伸ばして接触する線分上の座標
				cl_cp = oldPosition + ( v1 * radius ) + ( dr * direction ) ;

				v1 = cl_cp - cl_p0 ;

				// 内積を使い線分上の位置値(0～1)を算出する(符号も有効化)
				dc = v0.x * v1.x + v0.y * v1.y ;

//				Debug.LogWarning( "dc値[" + i + "]" + dc ) ;

				if( dc >= 0 && dc <= dl )
				{
					// 移動時にこの線分に接触する(接触しないのであれば無視する)

					// 有効な線分上の点(線の端も含めないと先の端に直進されるとすり抜けられてしまう)
//						Debug.LogWarning( "[" + i + "]接点距離:" + dr ) ;

					// 線(壁)への接触が確定した場合はごく僅かに押し戻す
					mv = direction * ( dr - 0.00f ) ;

					if( mv.magnitude <  minimalVelocity.magnitude )
					{
						// 移動可能距離をより短い方に更新する
						minimalVelocity = mv ;

						// ズリ移動係数(垂直の場合は0・水平の場合は-1～+1)
						cos_v = v0.x * direction.x + v0.y * direction.y ;

						// 本来の移動量に足りない分を左右にズリ移動させる
						dc = ( velocity.magnitude - minimalVelocity.magnitude ) ;	// 足りない分の移動量
						extraVelocity =    v0    * dc * cos_v ;	// v0は線分に並行な単位ベクトル・dcは足りない分の移動量・cos_vはズリ移動量係数
					}
					isHit = true ;
				}
			}

			resultVelocity = minimalVelocity ;	// 接線ギリギリではなく僅かに移動可能距離を減らす

//			Debug.LogWarning( "[最終決定した移動ベクトルと距離:" + minimalVelocity + " " + minimalVelocity.magnitude + " " + isHit + " " + extraVelocity ) ;
			return isHit ;
		}

		/// <summary>
		/// 点との当たり判定用
		/// </summary>
		public class CollisionPoint
		{
			public Vector2 Point ;
			public Vector2[] Lines = new Vector2[ 2 ] ;	// 角度
		}

		/// <summary>
		/// ２次元で単純に凸形と円が接触するか判定する(注意:凸形の頂点はループ状にすること(Ｚ型はＮＧ)　移動方向ベクトルを渡し実際に移動可能な座標を返す
		/// </summary>
		/// <returns></returns>
		private bool CheckCollisionPoint( CollisionPoint[] points, Vector2 oldPosition, float radius, Vector2 velocity, out Vector2 resultVelocity, out Vector2 extraVelocity )
		{
			extraVelocity = Vector2.zero ;

			// 注意：円の移動量が大きい(凸形が小さい)と線分と交差もしないすり抜け現象が起きてしまう事が避けられない
			// 　　　よって計算負荷を軽くする意味でも円の一度の移動量を小さくし凸形を一定以上の大きさにする事をルール付けする
			// 　　　円は一回の移動量は頂点か線分に必ず接触する量でなければならない

			// 注意：移動可能距離は頂点との接触が必ずしも最短というわけではない
			// 　　　頂点と線分の全てで接触確認を行い
			// 　　　その中の接触するもので最も短い移動距離のものを選択する必要がある

			// 注意：総合的な結果がおかしくなるので実際は接触しない頂点との接触判定を行ってはならない

			// 移動元と移動先から移動ボリュームを作る

			Vector2 newPosition = oldPosition + velocity ;

			Vector2 direction = velocity.normalized ;	// 移動量ベクトルの単位ベクトル(移動方向ベクトル)

			Vector2 mv_svl = new Vector2( - direction.y,   direction.x ) ;
			Vector2 mv_svr = new Vector2(   direction.y, - direction.x ) ;

			Vector2 oep = oldPosition ; // - ( direction * radius ) ;
			Vector2 mv_pl0 = oep + ( mv_svl * radius ) ;
			Vector2 mv_pr1 = oep + ( mv_svr * radius ) ;

			Vector2 nep = newPosition ; // + ( direction * radius ) ;
			Vector2 mv_pl1 = nep + ( mv_svl * radius ) ;
			Vector2 mv_pr0 = nep + ( mv_svr * radius ) ;


			// 移動ボリュームの左右の線分
			Vector2 mv_vl = mv_pl1 - mv_pl0 ;
			Vector2 mv_vr = mv_pr1 - mv_pr0 ;


			int i, l = points.Length ;

			Vector2 cl_p0 ;

			Vector2 v0 ;

			Vector2 mv_lr ;

			Vector2 v2 ;

			Vector2 lv0 ;
			Vector2 lv1 ;

			float dc, dh, dr ;
			float cos_v, sin_v ;

			Vector2 minimalVelocity = velocity ;
			Vector2 mv ;

			bool isHit = false ;


			//-------------------------------------------------

			// 各コリジョン点分が判定対象か確認する
			for( i  = 0 ; i <  l ; i ++ )
			{
				cl_p0 = points[ i ].Point ;

				// この点(角)が進行方向に対して有効かどうか確認する

				lv0 = points[ i ].Lines[ 0 ] ;	// 点(角)に接続する線分(0)
				lv1 = points[ i ].Lines[ 1 ] ;	// 点(角)に接続する線分(1)

				// 点(角)に接続する２つの線分が進行方向に対して裏面を見せていたらこの点(角)の接触判定は無視する
				if( ( direction.x * lv0.y - direction.y * lv0.x ) <  0 && ( direction.x * lv1.y - direction.y * lv1.x ) <  0 )
				{
					continue ;
				}

				//---------------------

//				Debug.LogWarning( "1)有効な点分:" + i + " S0:" + ( direction.x * lv0.y - direction.y * lv0.x ) + " S1:" + ( direction.x * lv1.y - direction.y * lv1.x ) ) ;

				// 頂点が移動ボリュームに左のラインより外にあれば無視
				v0 = cl_p0 - mv_pl0 ;

				if
				(
					( mv_vl.x * v0.y - mv_vl.y * v0.x ) >= 0
				)
				{
					continue ;
				}

				// 頂点が移動ボリュームに右のラインより外にあれば無視
				v0 = cl_p0 - mv_pr0 ;

				if
				(
					( mv_vr.x * v0.y - mv_vr.y * v0.x ) >= 0
				)
				{
					continue ;
				}

				//---------------------

				// 移動先の円と頂点の接触判定

				dh = ( cl_p0 - newPosition ).magnitude ;

				mv_lr = mv_pr0 - mv_pl1 ;
				v2 = cl_p0 - mv_pl1 ;

				dc = mv_lr.x * v2.y - mv_lr.y * v2.x ;

				if( dh >  radius && dc >  0 )
				{
					// 頂点が移動先の円の前
					continue ;
				}

				//---------------------

				// 移動元の円と頂点の接触判定(面が裏側になるので半径の符号を反転させて判定)

				dh = ( cl_p0 - oldPosition ).magnitude ;

				mv_lr = mv_pl0 - mv_pr1 ;
				v2 = cl_p0 - mv_pr1 ;

				dc = mv_lr.x * v2.y - mv_lr.y * v2.x ;

				if( dh >  radius && dc >  0 )
				{
					// 頂点が移動元の円の後
					continue ;
				}


//				Debug.LogWarning( "2)有効な点分:[" + i + "] " ) ;

				//---------------------------------------------
				// 有効な線分に対する最も近い接触点を算出する

				// １点が左右の領域内に入っているかどうか

				// 線分の２点共が移動ボリュームに左のラインより外にあれば無視
//				Debug.LogWarning( "頂点有効" ) ;

					// 隣にブロックがある場合などこの始点の角判定を無視できる

					// 始点までの到達距離を計算する

				v0 = oldPosition - cl_p0 ;	// 始点から中心までの距離
				dr = v0.x * ( - direction.x ) + v0.y * ( - direction.y ) ;	// cos
				dh = v0.x * ( - direction.y ) - v0.y * ( - direction.x ) ;	// sin

//					Debug.LogWarning( "dr:" + dr + " dh:" + dh ) ;

				cos_v = dh / radius ;
				sin_v = Mathf.Sqrt( 1 - cos_v * cos_v ) ;
				dc = radius * sin_v ;

				mv = direction * ( dr - dc ) ;



				if( mv.magnitude <  minimalVelocity.magnitude )
				{
					// 移動可能距離をより短い方に更新する
					minimalVelocity = mv ;

					// 中心から点(角)への方向と移動方向が一致する場合はズリ移動は発生させない

					// 接触点において中心から接触点に向かって伸ばしたベクトルに垂直なベクトルを線分とみなし
					// 線分上でのズリ移動を行う

					// 接触点まで移動後に中心から接触点へのベクトルを算出
					Vector2 cp = oldPosition + minimalVelocity * direction ;
					v2 = ( cl_p0 - cp ).normalized ;

					// 接線(線分)に変更
					v0.x =   v2.y ;
					v0.y = - v2.x ;

					// ズリ移動係数(垂直の場合は0・水平の場合は-1～+1)
					cos_v = v0.x * direction.x + v0.y * direction.y ;

					// 本来の移動量に足りない分を左右にズリ移動させる
					dc = ( velocity.magnitude - minimalVelocity.magnitude ) ;	// 足りない分の移動量
					extraVelocity =    v0    * dc * cos_v ;	// v0は線分に並行な単位ベクトル・dcは足りない分の移動量・cos_vはズリ移動量係数
				}
//				Debug.LogWarning( "始点距離:" + ( dr - dc ) ) ;
				isHit = true ;
			}

			resultVelocity = minimalVelocity ;

//			Debug.LogWarning( "[最終決定した移動ベクトルと距離:" + minimalVelocity + " " + minimalVelocity.magnitude + " " + isHit ) ;
			return isHit ;
		}
	}
}
