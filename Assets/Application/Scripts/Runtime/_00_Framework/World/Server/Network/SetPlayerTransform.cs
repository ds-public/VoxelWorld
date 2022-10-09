using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

namespace DBS.World
{
	/// <summary>
	/// サーバー(ウェブソケット)
	/// </summary>
	public partial class WorldServer
	{
		// プレイヤーの位置と方向を設定
		private void WS_OnReceived_Request_SetPlayerTransform( ActiveClient client, byte[] data )
		{
			var context = Packet.ClientRequestTypes.SetPlayerTransform.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// リクエストパラメータの展開

			var position	= context.Position ;
			var direction	= context.Direction ;

			//----------------------------------------------------------
			// サーバー側の処理

			// プレイヤー情報を取得する
			WorldPlayerData player = client.Player ;
			if( player == null )
			{
				// 不正
				return ;
			}

			//--------------

			// プレイヤーの位置と方向を設定する
			player.SetTransform( position, direction ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_SetPlayerTransform_Other( client, player ) ;
		}

		// 他のプレイヤーへ位置と方向の通知
		private void WS_Send_Response_SetPlayerTransform_Other( ActiveClient client, WorldPlayerData player )
		{
			//----------------------------------------------------------
			// 他人へのレスポンス

			// その他のアクティブなクライアントに対して位置と方向が変化した事を通知する
			if( m_ActiveClients != null && m_ActiveClients.Count >  1 )
			{
				// レスポンス
				var response = Packet.ServerResponseTypes.SetPlayerTransform_Other.Encode
				(
					client.ID,
					player.Position,
					player.Direction
				) ;

				foreach( var activeClient in m_ActiveClients.Values )
				{
					if( activeClient.ID != client.ID )
					{
						activeClient.SendData( response ) ;
					}
				}
			}
		}
	}
}
