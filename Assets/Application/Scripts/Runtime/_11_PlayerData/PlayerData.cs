using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using CSVHelper ;
using JsonHelper ;
using StorageHelper ;

namespace DSW
{
	/// <summary>
	/// ゲーム全体から参照されるプレイヤー系データを保持するクラス Version 2022/10/01
	/// </summary>
	public partial class PlayerData
	{
		/// <summary>
		/// プレイモード種別
		/// </summary>
		public enum PlayModes
		{
			Single,
			Multi,
		}

		/// <summary>
		/// プレイモード
		/// </summary>
		public static PlayModes	PlayMode ;


		/// <summary>
		/// プレイヤー名
		/// </summary>
		public static string	PlayerName ;

		/// <summary>
		/// カラータイプ
		/// </summary>
		public static byte		ColorType ;

		/// <summary>
		/// サーバーアドレス
		/// </summary>
		public static string	ServerAddress ;

		/// <summary>
		/// サーバーポート番号
		/// </summary>
		public static int		ServerPortNumber ;
	}
}
