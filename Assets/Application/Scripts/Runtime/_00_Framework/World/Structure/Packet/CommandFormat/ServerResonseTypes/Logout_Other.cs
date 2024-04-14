using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DSW.World.Packet.ServerResponseTypes
{
	/// <summary>
	/// 他のプレイヤーが去った際のレスポンス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class Logout_Other : ServerResponseDataBase<Logout_Other>
	{
		[SerializeField]
		public string	ClientId ;

		//-----------------------------------

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="playerName"></param>
		/// <param name="colorType"></param>
		/// <returns></returns>
		public static byte[] Encode( string clientId )
		{
			var context = new Logout_Other()
			{
				ClientId	= clientId
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
			return Encode( CommandTypes.Logout_Other, data ) ;
		}
	}
}
