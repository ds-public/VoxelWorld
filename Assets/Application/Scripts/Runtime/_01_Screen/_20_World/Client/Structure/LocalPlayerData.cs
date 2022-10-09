using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;


namespace DBS.World
{
	/// <summary>
	/// プレイヤーデータ
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class LocalPlayerData
	{
		/// <summary>
		/// サーバーアドレス
		/// </summary>
		[SerializeField]
		public string	ServerAddress ;

		/// <summary>
		/// サーバーポート番号
		/// </summary>
		[SerializeField]
		public int		ServerPortNumber ;

		/// <summary>
		/// プレイヤー識別子
		/// </summary>
		[SerializeField]
		public string	PlayerId ;
	}
}
