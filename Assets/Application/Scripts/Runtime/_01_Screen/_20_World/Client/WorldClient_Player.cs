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

				// カメラ(目)の位置を設定
				if( m_IsPlayerSneaking == false )
				{
					// ノーマル状態
					m_Camera.transform.localPosition = new Vector3( 0, 1.2f, 0 ) ;
				}
				else
				{
					// スニーク状態
					m_Camera.transform.localPosition = new Vector3( 0, 0.9f, 0.1f ) ;
				}

				// 自身の見た目は表示しない
				m_PlayerActor.HideFigure() ;
			}
			else
			if( m_PlayerViewType == PlayerViewTypes.ThirdPerson_Normal )
			{
				// 客観視点(通常)

				m_Camera.transform.localPosition = new Vector3( 0, 3f, -3.5f ) ;

				// 自身の見た目は表示する
				m_PlayerActor.ShowFigure() ;
			}
			else
			if( m_PlayerViewType == PlayerViewTypes.ThirdPerson_Invert )
			{
				// 客観視点(反対)

				m_Camera.transform.localPosition = new Vector3( 0, 4f,  4f ) ;

				// 自身の見た目は表示する
				m_PlayerActor.ShowFigure() ;
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

			Vector3 direction = new Vector3( dx, 0, dz ) ;
			direction.Normalize() ;	// 念のため正規化する

			m_PlayerActor.Forward = direction ;
			m_PlayerActor.Up = new Vector3( 0, 1, 0 ) ;
		}



	}
}
