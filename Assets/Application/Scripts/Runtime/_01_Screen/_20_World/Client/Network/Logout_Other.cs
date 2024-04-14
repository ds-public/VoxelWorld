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
		// 他のプレイヤーのログアウトを処理する(他のプレイヤー限定)
		private void WS_OnReceived_Response_Logout_Other( byte[] data )
		{
			var context = Packet.ServerResponseTypes.Logout_Other.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			// クライアント識別子
			string clientId			= context.ClientId ;

			//----------------------------------------------------------
			// クライアント側の処理

			// クライアント側のプレイヤー情報にも反映する
			if( m_ClientPlayers.ContainsKey( clientId ) == true )
			{
				var clientPlayer = m_ClientPlayers[ clientId ] ;

				AddLog( "<color=#FFFF00>" + clientPlayer.Name + "が世界を去りました</color>" ) ;

				// ネームプレートを破棄する
				clientPlayer.DeleteNamePlate() ;

				// アクターを破棄する
				clientPlayer.DeleteActor() ;

				// クライアントプレイヤーを除外する
				m_ClientPlayers.Remove( clientId ) ;
			}
		}


	}
}
