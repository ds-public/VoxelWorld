using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// List 型のメソッド拡張
	/// </summary>
	public static class List_Extension
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return true ;
			}

			return false ;
		}

		public static void RemoveRange<T>( this List<T> list, IEnumerable<T> target )
		{
			foreach( T element in target )
			{
				list.Remove( element ) ;
			}
		}

		/// <summary>
		/// リストの末尾を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static T GetLast<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return default ;
			}

			return list[ list.Count - 1 ] ;
		}

		/// <summary>
		/// リストの末尾を削除する
		/// </summary>
		/// <param name="list"></param>
		public static void RemoveLast<T>( this List<T> list )
		{
			if( list == null || list.Count == 0 )
			{
				return ;
			}

			list.RemoveAt( list.Count - 1 ) ;
		}

		//---------------------------------------------------------------------------

		// 以下、必要になったら順次メソッドを追加する
	}
}

