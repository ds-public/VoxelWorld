using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

namespace DBS.World.Packet
{
	/// <summary>
	/// プレイヤーデータ
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class PacketBase
	{
		[SerializeField]
		public	string	Signature = "VWPD" ;
	}
}
