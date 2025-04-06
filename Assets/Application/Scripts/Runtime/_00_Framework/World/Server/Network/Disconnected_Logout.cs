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
		// 切断(ログアウト)の要求を受信したら呼び出される
		private void WS_OnDisconnected( ActiveClient activeClient )
		{
			//----------------------------------------------------------
			// サーバー側の処理

			if( m_ActiveClients == null || m_ActiveClients.Count == 0 )
			{
				// 既にサーバーはシャットダウンされている
				Debug.Log( "<color=#007F7F>[SERVER] Shutdown already running.</color>" ) ;
				return ;
			}

			if( m_ActiveClients.ContainsKey( activeClient.Client ) == false )
			{
				// 既にクライアントが管理下に無い
				Debug.Log( "<color=#007F7F>[SERVER] Not found clint = " + activeClient.Client.EndPoint + "</color>" ) ;
				return ;
			}

			//----------------------------------------------------------

			// ログアウトしたクライアントに関連するチャンクセットで破棄可能なものを全て破棄する
			FreeChunkSetsWithClientId( activeClient.Client.Id.ToString() ) ;

			//----------------------------------

			if( activeClient.Player != null )
			{
				// プレイヤーを削除する(プレイヤーデータはログイン時にロードされるためログイン前の切断だとロードされていない可能性がある)
				DeletePlayer( activeClient.Player ) ;
				activeClient.Player = null ;
			}

			//----------------------------------

			// クライアントを除外する
			m_ActiveClients.Remove( activeClient.Client ) ;

			//----------------------------------------------------------
			// レスポンスを返す

			WS_Send_Response_Logout_Other( activeClient ) ;
		}

		// ログアウトに対する応答を返す
		private void WS_Send_Response_Logout_Other( ActiveClient disconnectedActiveClient )
		{
			//----------------------------------------------------------
			// 他人へのレスポンス

			// その他のアクティブなクライアントに対して新しいプレイヤーが加わった事を通知する
			if( m_ActiveClients != null && m_ActiveClients.Count >  0 )
			{
				var response = Packet.ServerResponseTypes.Logout_Other.Encode
				(
					disconnectedActiveClient.Client.Id.ToString()	// 新しくログインしたプレイヤーのクライアント識別子
				) ;

				foreach( var activeClient in m_ActiveClients.Values )
				{
					if( activeClient.Client.Id != disconnectedActiveClient.Client.Id )
					{
						activeClient.Client.SendTcp( response ) ;
					}
				}
			}
		}
	}
}
