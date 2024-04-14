using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DSW.World.Packet.ServerResponseTypes
{
	/// <summary>
	/// 他のプレイヤーが加わった際のレスポンス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class Login_Other : ServerResponseDataBase<Login_Other>
	{
		[SerializeField]
		public string	ClientId ;

		[SerializeField]
		public bool		IsNew ;

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
		public static byte[] Encode( string clientId, bool isNew, string name, byte colorType, Vector3 position, Vector3 direction )
		{
			var context = new Login_Other()
			{
				ClientId	= clientId,
				IsNew		= isNew,
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
			return Encode( CommandTypes.Login_Other, data ) ;
		}
	}
}
