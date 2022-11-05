using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;

using DSW.World.Packet ;

namespace DSW.World.Packet.ClientRequestTypes
{
	/// <summary>
	/// クライアントリクエストの基底クラス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class ClientRequestDataBase<T>
	{
		/// <summary>
		/// パケットのエンコード
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Encode( CommandTypes commandType, byte[] data )
		{
			var response = new ClientRequest( commandType, data ) ;
			return DataPacker.Serialize( response, false, Settings.DataTypes.MessagePack ) ;
		}

		/// <summary>
		/// パケットのデコード
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static T Decode( byte[] data )
		{
			return DataPacker.Deserialize<T>( data, false, Settings.DataTypes.MessagePack ) ;
		}
	}
}
