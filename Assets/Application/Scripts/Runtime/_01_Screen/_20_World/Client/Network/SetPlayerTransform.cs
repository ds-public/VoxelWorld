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

using DBS.World.Packet ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(ウェブソケット)
	/// </summary>
	public partial class WorldClient
	{
		// プレイヤーの位置と方向を設定する
		private bool WS_Send_Request_SetPlayerTransform( Vector3 playerPosition, Vector3 playerDirection )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// パケットを生成する
			var request = Packet.ClientRequestTypes.SetPlayerTransform.Encode
			(
				playerPosition,
				playerDirection
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}
	}
}
