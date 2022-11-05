using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DSW.World.Packet.ClientRequestTypes
{
	/// <summary>
	/// ブロックの設定
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class SetWorldBlock : ClientRequestDataBase<SetWorldBlock>
	{
		[SerializeField]
		public short			X ;

		[SerializeField]
		public short			Z ;

		[SerializeField]
		public short			Y ;

		[SerializeField]
		public short			Block ;

		//-----------------------------------

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="playerName"></param>
		/// <param name="colorType"></param>
		/// <returns></returns>
		public static byte[] Encode( short x, short z, short y, short block )
		{
			var context = new SetWorldBlock()
			{
				X		= x,
				Z		= z,
				Y		= y,
				Block	= block
			} ;

			return Encode( CommandTypes.SetWorldBlock, DataPacker.Serialize( context, false, Settings.DataTypes.MessagePack ) ) ;
		}
	}
}
