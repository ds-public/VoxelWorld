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
		// ログインを要求する
		private bool WS_Send_Request_Login()
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

//			Debug.Log( "[CLIENT] Login" ) ;

			// パケットを生成する
			var request = Packet.ClientRequestTypes.Login.Encode
			(
				m_PlayerId,
				PlayerData.PlayerName,
				PlayerData.ColorType
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}

		//-----------------------------------

		// ログイン要求に対するレスポンスが返されると呼び出される
		private void WS_OnReceived_Response_Login( byte[] data )
		{
//			Debug.Log( "[CLIENT] ログインのレスポンスを受信した" ) ;

			var context = Packet.ServerResponseTypes.Login.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			// クライアント識別子
			m_ClientId				= context.ClientId ;

			// プレイヤー識別子
			m_PlayerId				= context.PlayerId ;

			// プレイヤー名
			string	playerName		= context.Name ;

			// プレイヤー色
			byte	playerColorType	= context.ColorType ;

			// 位置
			Vector3 playerPosition	= context.Position ;

			// 方向
			Vector3 playerDirection	= context.Direction ;

			//----------------------------------------------------------
			// クライアント側の処理

			// プレイヤーの姿勢を設定する
			SetPlayerTransform( playerPosition, playerDirection ) ;

			// クライアント情報を保存しておく
			SaveLocalPlayer() ;

			//--------------

			// 定期送信用の初期情報を設定する(自分自身の情報)

			Vector3 p = m_PlayerActor.Position ;
			Vector3 d = m_PlayerActor.Forward ;

			m_TransformInterval		= Time.realtimeSinceStartup ;
			m_TransformPosition.Set( p.x, p.y, p.z ) ;
			m_TransformDirection.Set( d.x, d.y, d.z ) ;

			//--------------

			// クライアント部プレイヤー情報にも登録する

			var clientPlayer = new ClientPlayerData( this )
			{
				ClientId	= m_ClientId,		// 他のプレイヤーとフォーマットを合わせるため
				Name		= playerName,
				ColorType	= playerColorType,
				Position	= playerPosition,
				Direction	= playerDirection
			} ;

			m_ClientPlayers.Add( m_ClientId, clientPlayer ) ;

			// 自分自身のアクターは既に生成済みなので作成する必要は無い
			clientPlayer.SetActor( m_PlayerActor ) ;

			//--------------

			// ログイン済み
			m_IsLogin = true ;

			// 終了ボタンを押せるようにする
			m_EndButton.Interactable = true ;

			// ポーズ中のＵＩを表示する
			ShowPausingUI() ;

			AddLog( "ログインしました(PID=" + m_PlayerId + ")" ) ;
		}
	}
}
