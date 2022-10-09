using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DBS.World.Packet.ClientRequestTypes
{
	/// <summary>
	/// チャンクセット解放の要求
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class FreeWorldChunkSet : ClientRequestDataBase<FreeWorldChunkSet>
	{
		[SerializeField]
		public int		CsId ;

		//-----------------------------------

		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="playerName"></param>
		/// <param name="colorType"></param>
		/// <returns></returns>
		public static byte[] Encode( int csId )
		{
			var context = new FreeWorldChunkSet()
			{
				CsId	= csId
			} ;

			return Encode( CommandTypes.FreeWorldChunkSet, DataPacker.Serialize( context, false, Settings.DataTypes.MessagePack ) ) ;
		}
	}
}
