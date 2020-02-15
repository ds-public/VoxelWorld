using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

// 参考
// https://minecraft-ja.gamepedia.com/%E3%83%81%E3%83%A3%E3%83%B3%E3%82%AF
// https://minecraft-ja.gamepedia.com/Chunk%E3%83%95%E3%82%A9%E3%83%BC%E3%83%9E%E3%83%83%E3%83%88

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// 縦方向の６４チャンクを１つにまとめたデータ(この単位で読み書きされる)
	/// </summary>
	public class ActiveChunkSetData : ChunkSetData
	{
		// このチャンクセットが現在使用中かどうか
		public bool IsUsing ;
	}
}
