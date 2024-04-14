using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DSW.World.Packet.ClientRequestTypes
{
	/// <summary>
	/// プレイヤーの位置と方向の設定
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class SetPlayerTransform : ClientRequestDataBase<SetPlayerTransform>
	{
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
		public static byte[] Encode( Vector3 position, Vector3 direction )
		{
			var context = new SetPlayerTransform()
			{
				Position	= position,
				Direction	= direction
			} ;

			return Encode( CommandTypes.SetPlayerTransform, DataPacker.Serialize( context, false, Settings.DataTypes.MessagePack ) ) ;
		}
	}
}
