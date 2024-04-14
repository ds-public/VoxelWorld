using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DSW.World.Packet.ServerResponseTypes
{
	/// <summary>
	/// クライアントリクエストの基底クラス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class Login : ServerResponseDataBase<Login>
	{
		[SerializeField]
		public string	ClientId ;

		[SerializeField]
		public string	PlayerId ;

		[SerializeField]
		public string	Name ;

		[SerializeField]
		public byte		ColorType ;

		[SerializeField]
		public Vector3	Position ;

		[SerializeField]
		public Vector3	Direction ;

		//-----------------------------------

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="playerName"></param>
		/// <param name="colorType"></param>
		/// <returns></returns>
		public static byte[] Encode( string clientId, string playerId, string name, byte colorType, Vector3 position, Vector3 direction )
		{
			var context = new Login()
			{
				ClientId	= clientId,	// 自分自身に送ってもあまり意味は無いが他のクライアント側で他のプレイヤーとフォーマットを合わせるために送る
				PlayerId	= playerId,
				Name		= name,
				ColorType	= colorType,
				Position	= position,
				Direction	= direction
			} ;

			return Encode( DataPacker.Serialize( context, false, Settings.DataTypes.MessagePack ) ) ;
		}

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Encode( byte[] data )
		{
			return Encode( CommandTypes.Login, data ) ;
		}
	}
}
