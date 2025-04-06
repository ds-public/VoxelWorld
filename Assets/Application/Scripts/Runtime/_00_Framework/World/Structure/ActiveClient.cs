using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;


using Cysharp.Threading.Tasks ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

using SocketHelper ;

using DSW.WorldServerClasses ;

using DSW.World.Packet ;


namespace DSW.World
{
/*
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
*/


	public class ActiveClient
	{
		/// <summary>
		/// クライアント識別
		/// </summary>
		public ClientHandler	Client ;


		/// <summary>
		/// プレイヤーデータ
		/// </summary>
		public WorldPlayerData	Player ;
	}

}
