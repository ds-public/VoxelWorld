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
	public class WorldPlayerData
	{
		/// <summary>
		/// 識別子
		/// </summary>
		[SerializeField]
		public string	Id ;

		/// <summary>
		/// 名前
		/// </summary>
		[SerializeField]
		public string	Name ;

		/// <summary>
		/// 色
		/// </summary>
		[SerializeField]
		public byte		ColorType ;

		/// <summary>
		/// 座標
		/// </summary>
		[SerializeField]
		public Vector3	Position ;

		/// <summary>
		/// 方向
		/// </summary>
		[SerializeField]
		public Vector3	Direction ;

		//----------------------------------

		/// <summary>
		/// プレイヤーの位置と方向を設定する
		/// </summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		public void SetTransform( Vector3 position, Vector3 direction )
		{
			Position	= position ;
			Direction	= direction ;
		}

		/// <summary>
		/// 全て削除する
		/// </summary>
		public void Delete()
		{
		}

		//-------------------------------------------------------------------------------------------
	}
}
