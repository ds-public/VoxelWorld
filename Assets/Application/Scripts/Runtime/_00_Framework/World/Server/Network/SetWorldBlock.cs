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
		// ブロック設定の要求を受けた
		private void WS_OnReveived_Request_SetWorldBlock( ActiveClient client, byte[] data )
		{
			var context = Packet.ClientRequestTypes.SetWorldBlock.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// リクエストパラメータの展開

			short x		= context.X ;
			short z		= context.Z ;
			short y		= context.Y ;
			short block	= context.Block ;

			//----------------------------------------------------------
			// サーバー側の処理

			// チャンクの状態を設定する
			SetBlock( x, z, y, block ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_SetWorldBlock_Other( client, x, z, y, block ) ;
		}

		// 他のプレイヤーへ位置と方向の通知
		private void WS_Send_Response_SetWorldBlock_Other( ActiveClient client, short x, short z, short y, short block )
		{
			//----------------------------------------------------------
			// 他人へのレスポンス

			// その他のアクティブなクライアントに対してワールドのブロックの状態が変化した事を通知する
			if( m_ActiveClients != null && m_ActiveClients.Count >  1 )
			{
				var response = Packet.ServerResponseTypes.SetWorldBlock_Other.Encode
				(
					x,
					z,
					y,
					block
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
