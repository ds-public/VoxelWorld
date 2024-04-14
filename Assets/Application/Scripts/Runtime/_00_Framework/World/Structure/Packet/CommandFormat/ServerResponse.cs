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
	public class ServerResponse : PacketBase
	{
		[SerializeField]
		public CommandTypes			CommandType ;

		[SerializeField]
		public ResponseCodes		ResponseCode ;

		[SerializeField]
		public byte[]				Data ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="commandType"></param>
		public ServerResponse( CommandTypes commandType, ResponseCodes responseCode, byte[] data = null )
		{
			this.CommandType	= commandType ;
			this.ResponseCode	= responseCode ;
			this.Data			= data ;
		}
	}
}
