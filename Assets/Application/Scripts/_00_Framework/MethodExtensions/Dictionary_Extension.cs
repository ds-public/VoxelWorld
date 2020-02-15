using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// Dictionary 型のメソッド拡張
	/// </summary>
	public static class Dictionary_Extension
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T1,T2>( this Dictionary<T1,T2> dictionary )
		{
			if( dictionary == null || dictionary.Count == 0 )
			{
				return true ;
			}

			return false ;
		}

		//---------------------------------------------------------------------------

		// 以下、必要になったら順次メソッドを追加する
	}
}

