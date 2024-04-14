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
		// ログインの要求を受信したら呼び出される
		private void WS_OnReceived_Request_Login( ActiveClient client, byte[] data )
		{
			var context = Packet.ClientRequestTypes.Login.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// リクエストパラメータの展開

			// プレイヤー識別子を取得する
			string playerId = context.PlayerId ;

//			Debug.Log( "[SERVER] Login 要求あり:" + client.ID ) ;

			// プレイヤー名を取得する
			string playerName = context.PlayerName ;

//			Debug.Log( "名前:" + playerName ) ;

			// プレイヤーアクター色を取得する
			byte colorType = context.ColorType ;

//			Debug.Log( "色:" + colorType ) ;

			//----------------------------------------------------------
			// サーバー側の処理



			WorldPlayerData player = null ;

//			Debug.Log( "PlayerId : " + playerId ) ;

			if( string.IsNullOrEmpty( playerId ) == false )
			{
				// 既存プレイヤーのはず
				player = GetPlayerById( playerId, playerName, colorType ) ;

//				Debug.Log( "既存プレイヤー:" + player ) ;
			}

			if( player == null )
			{
				// 新規プレイヤー
				player = CreateNewPlayer( playerName, colorType ) ;
			}

			// 関係するプレイヤーのインスタンスをクライアントスロットに保存しておく
			client.Player = player ;

			Debug.Log( "<color=#00FFFF>[SERVER] プレイヤー(PID:" + player.Id + ")がログインしました</color>" ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_Login( client, player ) ;

			//----------------------------------------------------------
			// 他のプレイヤーが既にログインしていればその情報も送る

		}

		// ログインに対する応答を返す
		private void WS_Send_Response_Login( ActiveClient client, WorldPlayerData player )
		{
			//----------------------------------------------------------
			// 自身へのレスポンス

			var response = Packet.ServerResponseTypes.Login.Encode
			(
				client.ID,	// 自分自身に送ってもあまり意味は無いが他のクライアント側で他のプレイヤーとフォーマットを合わせるために送る
				player.Id,	// 初めてログインした時のために送る
				player.Name,
				player.ColorType,
				player.Position,
				player.Direction
			) ;

			client.SendData( response ) ;

			// 既にログインしているプレイヤーの情報を送る
			if( m_ActiveClients != null && m_ActiveClients.Count >  0 )
			{
				foreach( var activeClient in m_ActiveClients.Values )
				{
					if( activeClient.ID != client.ID )
					{
						var otherPlayer = activeClient.Player ;

						response = Packet.ServerResponseTypes.Login_Other.Encode
						(
							activeClient.ID,	// 既にいるプレイヤーのクライアント識別子
							false,
							otherPlayer.Name,
							otherPlayer.ColorType,
							otherPlayer.Position,
							otherPlayer.Direction
						) ;

						client.SendData( response ) ;
					}
				}
			}

			//----------------------------------------------------------
			// 他人へのレスポンス

			// その他のアクティブなクライアントに対して新しいプレイヤーが加わった事を通知する
			if( m_ActiveClients != null && m_ActiveClients.Count >  0 )
			{
				response = Packet.ServerResponseTypes.Login_Other.Encode
				(
					client.ID,	// 新しくログインしたプレイヤーのクライアント識別子
					true,
					player.Name,
					player.ColorType,
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
