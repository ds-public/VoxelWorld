using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;


using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

using DSW.World.Packet ;

namespace DSW.World
{
	/// <summary>
	/// クライアント(ウェブソケット)
	/// </summary>
	public partial class WorldClient
	{
		// サーバーにブロック設定のリクエストを送る
		private bool WS_Send_Request_SetWorldBlock( short x, short z, short y, short block )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// パケットを生成する
			var request = Packet.ClientRequestTypes.SetWorldBlock.Encode
			(
				x,
				z,
				y,
				block
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}
	}
}
