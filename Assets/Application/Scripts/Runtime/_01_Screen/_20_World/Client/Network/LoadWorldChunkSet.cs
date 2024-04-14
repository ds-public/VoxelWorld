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
		// 通信:チャンクセット展開の要求
		private bool WS_Send_Request_LoadWorldChunkSet( int csId )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// ロードリクエストが出されていたら無視する
			if( m_ChunkSetRequests.Contains( csId ) == true )
			{
				return false ;
			}

			m_ChunkSetRequests.Add( csId ) ;

			//----------------------------------------------------------

			// パケットを生成する
			var request = Packet.ClientRequestTypes.LoadWorldChunkSet.Encode
			(
				csId
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}

		//-----------------------------------

		// 圧縮されたチャンク情報を受信したら呼び出される
		private void WS_OnReceived_Response_LoadWorldChunkSet( byte[] data )
		{
			var context = Packet.ServerResponseTypes.LoadWorldChunkSet.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			int		csId		= context.CsId ;
			byte[]	chunkSet	= context.ChunkSet ;

			//----------------------------------------------------------
			// クライアント側の処理

//			Debug.Log( "[CLIENT]チャンクセット受信:CsId = " + csId.ToString( "X4" ) + " Size = " + chunkSet.Length ) ;

			if( m_ChunkSetRequests.Contains( csId ) == false )
			{
				// 既にリクエストが取り消されている
				return ;
			}

			//--------------
			// リクエストは有効

			// ※現在はメインスレッド

			// サブスレッドでチャンクの展開を行う(メインスレッドを専有すると重くなるため)
			OpenChunkSet( csId, chunkSet ) ; 
		}
	}
}
