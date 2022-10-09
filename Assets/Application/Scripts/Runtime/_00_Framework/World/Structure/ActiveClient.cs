using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;


using Cysharp.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;


using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

using DBS.WorldServerClasses ;

using DBS.World.Packet ;

namespace DBS.World
{
	/// <summary>
	/// クライアントの管理クラス(定義のみ)
	/// </summary>
	public class ActiveClient : ExWebSocketBehavior<ActiveClient>
	{
		// カスタマイズしたい場合はフィールドやメソッドを追加する

		// 関係するプレイヤーデータのインスタンス
		public WorldPlayerData	Player ;

		//-----------------------------------------------------------

		/// <summary>
		/// 諸々の後始末を行う
		/// </summary>
		public void Delete()
		{
		}
	}
}
