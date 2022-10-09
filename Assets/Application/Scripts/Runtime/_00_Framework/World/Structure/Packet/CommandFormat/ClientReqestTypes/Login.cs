using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DBS.World.Packet.ClientRequestTypes
{
	/// <summary>
	/// クライアントリクエストの基底クラス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class Login : ClientRequestDataBase<Login>
	{
		[SerializeField]
		public string	PlayerId ;

		[SerializeField]
		public string	PlayerName ;

		[SerializeField]
		public byte		ColorType ;

		//-----------------------------------

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="playerName"></param>
		/// <param name="colorType"></param>
		/// <returns></returns>
		public static byte[] Encode( string playerId, string playerName, byte colorType )
		{
			var context = new Login()
			{
				PlayerId	= playerId,
				PlayerName	= playerName,
				ColorType	= colorType
			} ;

			return Encode( CommandTypes.Login, DataPacker.Serialize( context, false, Settings.DataTypes.MessagePack ) ) ;
		}
	}
}
