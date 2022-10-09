using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DBS.World.Packet
{
	/// <summary>
	/// パケットのコマンド種別
	/// </summary>
	public enum CommandTypes
	{
		/// <summary>
		/// ログインの要求・プレイヤーの情報の応答
		/// </summary>
		Login,

		/// <summary>
		/// 他のプレイヤーが加わった応答
		/// </summary>
		Login_Other,

		/// <summary>
		/// 他のプレイヤーが去った応答
		/// </summary>
		Logout_Other,

		/// <summary>
		/// プレイヤーの位置と方向を設定する要求
		/// </summary>
		SetPlayerTransform,

		/// <summary>
		/// プレイヤーの位置と方向を設定する要求(他のプレイヤーから)
		/// </summary>
		SetPlayerTransform_Other,

		/// <summary>
		/// チャンクセット取得の要求・応答
		/// </summary>
		LoadWorldChunkSet,

		/// <summary>
		/// チャンクセット解放の要求
		/// </summary>
		FreeWorldChunkSet,

		/// <summary>
		/// ブロック設定の要求と応答
		/// </summary>
		SetWorldBlock,

		/// <summary>
		/// ブロック設定の要求と応答(他のプレイヤーから)
		/// </summary>
		SetWorldBlock_Other,


	}
}
