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
		// ブロックを設定する(他のプレイヤー限定)
		private void WS_OnReceived_Response_SetWorldBlock_Other( byte[] data )
		{
			var context = Packet.ServerResponseTypes.SetWorldBlock_Other.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			short x		= context.X ;
			short z		= context.Z ;
			short y		= context.Y ;
			short block	= context.Block ;

			//----------------------------------------------------------
			// クライアント側の処理
			
			// クライアント側のワールド情報にも反映する
			SetBlock( x, z, y, block ) ;
		}
	}
}
