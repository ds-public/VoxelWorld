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
		// 通信:チャンクセット解放の要求
		private bool WS_Send_Request_FreeWorldChunkSet( int csId )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// ロードリクエストが出されていたら破棄する
			if( m_ChunkSetRequests.Contains( csId ) == true )
			{
				m_ChunkSetRequests.Remove( csId ) ;
			}

			//----------------------------------------------------------

			// パケットを生成する
			var request = Packet.ClientRequestTypes.FreeWorldChunkSet.Encode
			(
				csId
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}
	}
}
