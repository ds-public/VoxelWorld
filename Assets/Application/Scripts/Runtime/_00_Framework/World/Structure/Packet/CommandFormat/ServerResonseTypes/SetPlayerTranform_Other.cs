using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;

using DSW.World.Packet ;

namespace DSW.World.Packet.ServerResponseTypes
{
	/// <summary>
	/// 他のプレイヤーの位置と方向
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class SetPlayerTransform_Other : ServerResponseDataBase<SetPlayerTransform_Other>
	{
		[SerializeField]
		public string	ClientId ;

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
		public static byte[] Encode( string clientId, Vector3 position, Vector3 direction )
		{
			var context = new SetPlayerTransform_Other()
			{
				ClientId	= clientId,
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
			return Encode( CommandTypes.SetPlayerTransform_Other, data ) ;
		}
	}
}
