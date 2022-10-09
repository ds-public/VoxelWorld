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
		// 他のプレイヤーの位置と方向を設定する(他のプレイヤー限定)
		private void WS_OnReceived_Response_SetPlayerTransform_Other( byte[] data )
		{
			var context = Packet.ServerResponseTypes.SetPlayerTransform_Other.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			// クライアント識別子
			string clientId			= context.ClientId ;

			// 位置
			Vector3 playerPosition	= context.Position ;

			// 方向
			Vector3 playerDirection	= context.Direction ;

			//----------------------------------------------------------
			// クライアント側の処理

			// クライアント側のプレイヤー情報にも反映する
			if( m_ClientPlayers.ContainsKey( clientId ) == true )
			{
				m_ClientPlayers[ clientId ].SetTransform( playerPosition, playerDirection, m_NamePlateRoot, m_PlayerActor.GetCamera() ) ;
			}
		}


	}
}
