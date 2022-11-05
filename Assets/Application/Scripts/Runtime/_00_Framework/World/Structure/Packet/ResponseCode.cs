using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DSW.World.Packet
{
	/// <summary>
	/// パケットのコマンドレスポンス種別
	/// </summary>
	public enum ResponseCodes
	{
		/// <summary>
		/// 成功
		/// </summary>
		Successful		= 0,

		/// <summary>
		/// エラー
		/// </summary>
		Error			= 10000,
	}
}
