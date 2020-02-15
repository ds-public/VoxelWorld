using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBS
{
	/// <summary>
	/// ユニットの状態定義
	/// </summary>
	public enum UnitCondition
	{
		Unknown			= -9,
		Weakening		= -3,
		Enhancement		= -2,
		Normal			= -1,

		Poison			=  0,
		Paralysis		=  1,
		Sleep			=  2,
		Confusion		=  3,
		Charm			=  4,
		Silence			=  5,
		Bind			=  6,
		Stone			=  7,
	}
}
