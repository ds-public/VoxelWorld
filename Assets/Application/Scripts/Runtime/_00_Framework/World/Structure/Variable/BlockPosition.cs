using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using uGUIHelper ;
using TransformHelper ;

namespace DSW.World
{
	/// <summary>
	/// ブロック座標を定義する
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public struct BlockPosition
	{
		public int X ;
		public int Z ;
		public int Y ;

		public void Clear()
		{
			X = 0 ;
			Z = 0 ;
			Y = 0 ;
		}

		public override string ToString()
		{
			return "( " + X + ", " + Z + ", " + Y + " )" ;
		}
	}
}
