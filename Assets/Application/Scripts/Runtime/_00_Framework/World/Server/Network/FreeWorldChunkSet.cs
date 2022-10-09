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
		private void WS_OnReceived_Request_FreeWorldChunkSet( ActiveClient client, byte[] data )
		{
			var context = Packet.ClientRequestTypes.FreeWorldChunkSet.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// リクエストパラメータの展開

			// チャンクセット識別子
			int csId = context.CsId ;

			//----------------------------------------------------------
			// サーバー側の処理

			// 必要に応じてチャンクを破棄する
			FreeChunkSet( csId, client.ID ) ;
		}

	}
}
