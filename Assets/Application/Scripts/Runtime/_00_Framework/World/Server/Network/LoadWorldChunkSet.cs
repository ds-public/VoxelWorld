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
		// チャンクセットのロード要求を受信したら呼び出される
		private void WS_OnReceived_Request_LoadWorldChunkSet( ActiveClient client, byte[] data )
		{
			var context = Packet.ClientRequestTypes.LoadWorldChunkSet.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// リクエストバタメータの展開

			// チャンクセット識別子
			int csId = context.CsId ;

			//----------------------------------------------------------
			// サーバー側の処理

			// チャンクセットを展開または取得する
			var chunkSet = LoadChunkSet( csId, client.ID ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_LoadWorldChunkSet( client, csId, chunkSet ) ;
		}

		// チャンクセット展開の応答
		private void WS_Send_Response_LoadWorldChunkSet( ActiveClient client, int csId, byte[] chunkSet )
		{
			//----------------------------------------------------------
			// 自身へのレスポンス

			var response = Packet.ServerResponseTypes.LoadWorldChunkSet.Encode
			(
				csId,
				chunkSet
			) ;

			client.SendData( response ) ;
		}
	}
}
