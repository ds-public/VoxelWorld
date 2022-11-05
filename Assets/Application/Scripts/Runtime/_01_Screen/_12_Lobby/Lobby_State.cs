using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace DSW.Screens
{
	/// <summary>
	/// ロビーの制御処理
	/// </summary>
	public partial class Lobby
	{
		/// <summary>
		/// 状態(ステート)
		/// </summary>
		public enum State
		{
			/// <summary>
			/// 自動選択:ゲームではこれを指定する必要がある
			/// </summary>
			Auto,

			/// <summary>
			/// 行動選択
			/// </summary>
			ActionSelecting,

			//----------------------------------

			/// <summary>
			/// 不明
			/// </summary>
			Unknown,
		}
	}
}
