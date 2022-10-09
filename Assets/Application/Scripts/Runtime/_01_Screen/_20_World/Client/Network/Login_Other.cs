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
		// ログイン要求に対するレスポンスが返されると呼び出される
		private void WS_OnReceived_Response_Login_Other( byte[] data )
		{
			var context = Packet.ServerResponseTypes.Login_Other.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			// クライアント識別子
			string	clientId		= context.ClientId ;

			// 新規のログインか(falseで既にログイン済みのプレイヤー)
			bool	isNew			= context.IsNew ;

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

			// クライアント部プレイヤー情報にも登録する(他のプレイヤー)
			var clientPlayer = new ClientPlayerData( this )
			{
				ClientId	= clientId,
				Name		= playerName,
				ColorType	= playerColorType,
				Position	= playerPosition,
				Direction	= playerDirection
			} ;

			// クライアントプレイヤーを追加する
			m_ClientPlayers.Add( clientId, clientPlayer ) ;

			// アクターを生成する
			clientPlayer.CreateActor( m_WorldRoot, m_PlayerActor_Other ) ;

			// ネームプレートを生成する
			clientPlayer.CreateNamePlate( m_NamePlateRoot, m_NamePlate_Other, m_PlayerActor.GetCamera() ) ;

			//--------------

			if( isNew == true )
			{
				AddLog( "<color=#FFFF00>" + playerName + "がログインしました</color>" ) ;
			}
		}
	}
}
