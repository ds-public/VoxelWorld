using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

/// <summary>
/// ＤＢＳパッケージ
/// </summary>
namespace DBS.MassDataCategory
{
	public enum ItemRegion
	{
		Unknown		= -1,	// 不明

		Goods		=  1,	// 消耗品
		Equipment	=  2,	// 装備品
		Material	=  3,	// 素材品
	}
}