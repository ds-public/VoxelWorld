using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;


namespace DSW.World.Packet
{
	/// <summary>
	/// クライアントリクエストの基底クラス
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class ClientRequest : PacketBase
	{
		[SerializeField]
		public CommandTypes		CommandType ;

		[SerializeField]
		public byte[]			Data ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="commandType"></param>
		public ClientRequest( CommandTypes commandType, byte[] data )
		{
			this.CommandType	= commandType ;
			this.Data			= data ;
		}
	}
}
