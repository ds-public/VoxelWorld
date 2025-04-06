using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

namespace DSW.World
{
	/// <summary>
	/// サーバー(ウェブソケット)
	/// </summary>
	public partial class WorldServer
	{
		// チャンクセットのロード要求を受信したら呼び出される
		private void WS_OnReceived_Request_LoadWorldChunkSet( ActiveClient activeClient, byte[] data )
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
			var chunkSet = LoadChunkSet( csId, activeClient.Client.Id.ToString() ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_LoadWorldChunkSet( activeClient, csId, chunkSet ) ;
		}

		// チャンクセット展開の応答
		private void WS_Send_Response_LoadWorldChunkSet( ActiveClient activeClient, int csId, byte[] chunkSet )
		{
			//----------------------------------------------------------
			// 自身へのレスポンス

			var response = Packet.ServerResponseTypes.LoadWorldChunkSet.Encode
			(
				csId,
				chunkSet
			) ;

			activeClient.Client.SendTcp( response ) ;
		}
	}
}
