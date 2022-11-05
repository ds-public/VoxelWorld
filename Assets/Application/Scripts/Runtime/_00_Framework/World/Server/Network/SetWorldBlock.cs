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
		private void WS_Send_Response_SetWorldBlock_Other( ActiveClient client, short bx, short bz, short by, short block )
		{
			//----------------------------------------------------------
			// 他人へのレスポンス

			// その他のアクティブなクライアントに対してワールドのブロックの状態が変化した事を通知する
			if( m_ActiveClients != null && m_ActiveClients.Count >  1 )
			{
				// パケット情報生成
				var response = Packet.ServerResponseTypes.SetWorldBlock_Other.Encode
				(
					bx,
					bz,
					by,
					block
				) ;

				// チャンクセット識別子
				int csId = ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;

				// 他のクライアント全てにブロック状態が変化した事を通知する
				foreach( var activeClient in m_ActiveClients.Values )
				{
					if( activeClient.ID != client.ID && m_ActiveChunkSets[ csId ].ContainsClientId( activeClient.ID ) == true )
					{
						// 送信者と異なるクライアント識別子・且つ・そのクライアント識別子が対象ブロックの属するチャンクセットをロード済みの場合のみ
						// 遠く離れた(ローカルではそのチャンクセットを持っていない)他のプレイヤーに通知するのは無駄なので省く
						activeClient.SendData( response ) ;
					}
				}
			}
		}
	}
}
