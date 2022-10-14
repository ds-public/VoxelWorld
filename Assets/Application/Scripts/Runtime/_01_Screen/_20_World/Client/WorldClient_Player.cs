using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;


using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(プレイヤー[ローカル]情報管理)
	/// </summary>
	public partial class WorldClient
	{
		private string m_LocalPlayerPath = "Client/LoaclPlayer.json" ;

		//-------------------------------------------------------------------------------------------

		// プレイヤー情報をロードする
		private bool LoadLocalPlayer()
		{
			if( StorageAccessor.Exists( m_LocalPlayerPath ) != StorageAccessor.Target.File )
			{
				// プレイヤー識別子は取得していない
				return false ;
			}

			byte[] data = StorageAccessor.Load( m_LocalPlayerPath ) ;
			if( data == null || data.Length == 0 )
			{
				// 失敗
				return false ;
			}

			var player = DataPacker.Deserialize<LocalPlayerData>( data, false, Settings.DataTypes.Json ) ;
			if( player == null )
			{
				// 失敗
				return false ;
			}

			// プレイヤー識別子を取得する
			m_PlayerId = player.PlayerId ;

			// 成功
			return true ;
		}

		// プレイヤー情報をセーブする
		private bool SaveLocalPlayer()
		{
			var player = new LocalPlayerData()
			{
				ServerAddress		= PlayerData.ServerAddress,
				ServerPortNumber	= PlayerData.ServerPortNumber,
				PlayerId			= m_PlayerId	// プレイヤー識別子を更新する
			} ;

			var data = DataPacker.Serialize( player, false, Settings.DataTypes.Json ) ;
			if( data == null || data.Length == 0 )
			{
				// 失敗
				return false ;
			}

			// ストレージにセーブする
			return StorageAccessor.Save( m_LocalPlayerPath, data, makeFolder:true ) ;
		}

		//-------------------------------------------------------------------------------------------


		// プレイヤーアクターの状態を視点の種別に応じて切り替える
		private void UpdatePlayerActor()
		{
			if( m_PlayerViewType == PlayerViewTypes.FirstPerson )
			{
				// 主観視点

				// 目の位置を設定
				if( m_IsPlayerSneaking == false )
				{
					// ノーマル状態
					m_PlayerActor.Eye.localPosition = new Vector3( 0, 1.2f, 0 ) ;
				}
				else
				{
					// スニーク状態
					m_PlayerActor.Eye.localPosition = new Vector3( 0, 0.9f, 0.1f ) ;
				}

				m_PlayerActor.GetCamera().transform.localPosition = Vector3.zero ;
				m_PlayerActor.GetCamera().transform.localRotation = Quaternion.identity ;

				// 自身の見た目は表示しない
				m_PlayerActor.HideFigure() ;
			}
			else
			if( m_PlayerViewType == PlayerViewTypes.ThirdPerson_Normal )
			{
				// 客観視点(通常)

				m_PlayerActor.Eye.localPosition = new Vector3( 0, 2f, 0 ) ;

				float d = GetAvailableDistance( -6f ) ;
				Debug.Log( "Distance:" + d ) ;
				m_PlayerActor.GetCamera().transform.localPosition = new Vector3( 0, 0, d ) ;
				m_PlayerActor.GetCamera().transform.localRotation = Quaternion.identity ;

				// 自身の見た目は表示する
				m_PlayerActor.ShowFigure() ;
			}
			else
			if( m_PlayerViewType == PlayerViewTypes.ThirdPerson_Invert )
			{
				// 客観視点(反対)

				m_PlayerActor.Eye.localPosition = new Vector3( 0, 1.2f, 0 ) ;

				float d = GetAvailableDistance(  4f ) ;
				m_PlayerActor.GetCamera().transform.localPosition = new Vector3( 0, 0, d ) ;
				m_PlayerActor.GetCamera().transform.localRotation = Quaternion.Euler( 0, 180f, 0 ) ;

				// 自身の見た目は表示する
				m_PlayerActor.ShowFigure() ;
			}
		}

		// 目からカメラまでの可能な最大距離を取得する
		private float GetAvailableDistance( float max )
		{
			Transform ct = m_PlayerActor.GetCamera().transform ;
			Transform et = m_PlayerActor.Eye ;
			Vector3 p ;
			int bx, bz, by ;
			Vector3 d ;

			int fa ;
			float fm, fv ;

			float fx, fz, fy ;

			float sign ;


			if( max != 0 )
			{
				if( max >  0 )
				{
					sign =  1 ;
				}
				else
				{
					sign = -1 ;
					max = -max ;
				}

				//---------------------------------

				// +0.5 ずつ増減させる
				float now = 0 ;

				while( now <  max )
				{
					now += 0.5f ;
					if( now >  max )
					{
						now  = max ;
					}

					ct.localPosition = new Vector3( 0, 0, now * sign ) ;
					p = ct.position ;

					bx = ( int )p.x ;
					bz = ( int )p.z ;
					by = ( int )p.y ;

					if( GetBlock( bx, bz, by ) != 0 )
					{
						// ブロックにヒットする

						// 目からカメラへの方向ベクトル
						d = ( ct.position - et.position ).normalized ;

						// X Z Y の絶対値で最も小さい面に接触する
						fa = 0 ;
						fm = Mathf.Infinity ;

						fv = Mathf.Abs( d.x ) ;
						if( fv <  fm )
						{
							// X の差分
							fa = 0 ;
							fm  = fv ;
						}
						fv = Mathf.Abs( d.z ) ;
						if( fv <  fm )
						{
							// Z の差分
							fa = 1 ;
							fm  = fv ;
						}
						fv = Mathf.Abs( d.y ) ;
						if( fv <  fm )
						{
							// Y の差分
							fa = 2 ;
							fm  = fv ;
						}

						if( fa == 0 )
						{
							// X の差分が最も小さく　X 面と接触する
							if( d.x >  0 )
							{
								// -X 方向の面と接触
								fx = ( float )( bx + 0 ) ;
								d *= ( Mathf.Abs( fx - et.position.x ) / fm ) ;
								return + d.magnitude ;
							}
							else
							if( d.x <  0 )
							{
								// +X 方向の面と接触
								fx = ( float )( bx + 1 ) ;
								d *= ( Mathf.Abs( fx - et.position.x ) / fm ) ;
								return + d.magnitude ;
							}
							else
							{
								// 接触
								return 0 ;
							}
						}
						if( fa == 1 )
						{
							// Z の差分が最も小さく　Z 面と接触する
							if( d.z >  0 )
							{
								// -Z 方向の面と接触
								fz = ( float )( bz + 0 ) ;
								d *= ( Mathf.Abs( fz - et.position.z ) / fm ) ;
								return + d.magnitude ;
							}
							else
							if( d.z <  0 )
							{
								// +Z 方向の面と接触
								fz = ( float )( bz + 1 ) ;
								d *= ( Mathf.Abs( fz - et.position.z ) / fm ) ;
								return + d.magnitude ;
							}
							else
							{
								// 接触
								return 0 ;
							}
						}
						if( fa == 1 )
						{
							// Y の差分が最も小さく　Y 面と接触する
							if( d.y >  0 )
							{
								// -Y 方向の面と接触
								fy = ( float )( by + 0 ) ;
								d *= ( Mathf.Abs( fy - et.position.y ) / fm ) ;
								return + d.magnitude ;
							}
							else
							if( d.y <  0 )
							{
								// +Y 方向の面と接触
								fy = ( float )( by + 1 ) ;
								d *= ( Mathf.Abs( fy - et.position.y ) / fm ) ;
								return + d.magnitude ;
							}
							else
							{
								// 接触
								return 0 ;
							}
						}
						break ;
					}
				}

				// どのブロックともヒットしなければここに来る
				return now * sign ;
			}
			else
			{
				return max ;
			}
		}


		// プレイヤーの姿勢を設定する
		private void SetPlayerTransform( Vector3 p, Vector3 d )
		{
			SetPlayerTransform( p.x, p.z, p.y, d.x, d.z, d.y ) ;
		}

		// プレイヤーの姿勢を設定する
		private void SetPlayerTransform( float px, float pz, float py, float dx, float dz, float dy )
		{
			m_PlayerActor.SetPosition( px, py, pz ) ;

			// Up を先に設定してから
			m_PlayerActor.Up = Vector3.up ;

			// Forward を後に設定する必要がある
			Vector3 direction = new Vector3( dx, 0, dz ) ;
			direction.Normalize() ;	// 念のため正規化する
			m_PlayerActor.Forward = direction ;	// Up を Forward の後に設定してはいけない(Forward が強制的に[0,0,1]にされてしまう)
		}
	}
}
