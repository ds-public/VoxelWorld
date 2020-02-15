using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// Array 型のメソッド拡張
	/// </summary>
	public static class Array_Extension
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty( this Array array )
		{
			if( array == null || array.Length == 0 )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// 配列内の要素全てに処理を適用する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T[] ForEach<T>( this T[] array, Action<T> action )
		{
			if( array == null || array.Length == 0 || action == null )
			{
				return array ;
			}


			foreach( T v in array )
			{
				action( v ) ;
			}

			return array ;
		}

		/// <summary>
		/// 配列に含まれるか確認する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool Contains<T>( this T[] array, T pattern ) where T : class
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( typeof( T ) == typeof( string ) )
			{
				// 文字列
				foreach( T element in array )
				{
					string p0 = element as string ;
					string p1 = pattern as string ;

					if( p0 == p1 )
					{
						return true ;
					}
				}
			}
			else
			{
				// その他
				foreach( T element in array )
				{
					if( element == ( object )pattern )
					{
						return true ;
					}
				}
			}

			return false ;
		}

		/// <summary>
		/// そのオブジェクトが含まれる場合のインデックス値を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static int IndexOf<T>( this T[] array, T pattern )
		{
			if( array == null || array.Length == 0 )
			{
				return -1 ;
			}

			int i, l = array.Length ;

			if( typeof( T ) == typeof( string ) )
			{
				// 文字列
				T element ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					element = array[ i ] ;

					string p0 = element as string ;
					string p1 = pattern as string ;

					if( p0 == p1 )
					{
						return i ;
					}
				}
			}
			else
			{
				// その他
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( ( object )array[ i ] == ( object )pattern )
					{
						return i ;
					}
				}
			}

			return -1 ;
		}

		/// <summary>
		/// 途中に挿入する(最後は
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool Insert<T>( this T[] array, int index, T element )
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( index <  0 || index >= array.Length )
			{
				return false ;
			}

			//----------------------------------

			int i, l = array.Length - ( index + 1 ) ;
			if( l >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					array[ array.Length - 1 - i ] = array[ array.Length - 2 - i ] ;
				}
			}

			array[ index ] = element ;

			return true ;
		}

		/// <summary>
		/// 途中に挿入する(最後は
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool RemoveAt<T>( this T[] array, int index )
		{
			if( array == null || array.Length == 0 )
			{
				return false ;
			}

			if( index <  0 || index >= array.Length )
			{
				return false ;
			}

			//----------------------------------

			int i, l = array.Length - ( index + 1 ) ;
			if( l >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					array[ index + i ] = array[ index + i + 1 ] ;
				}
			}

			array[ array.Length - 1 ] = default ;

			return true ;
		}

		/// <summary>
		/// 配列を指定の値で塗りつぶす
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="fillValue"></param>
		/// <returns></returns>
		public static T[] Fill<T>( this T[] array, T fillValue = default )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			int i, l = array.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				array[ i ] = fillValue ;	// 配列への代入は foreach は使用できない
			}

			return array ;
		}


		/// <summary>
		/// 配列の要素を全て初期化する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] Clear<T>( this T[] array )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			Array.Clear( array, 0, array.Length ) ;

			return array ;
		}

		/// <summary>
		/// 簡易コピーを生成する(配列内部の参照は元のまま)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] Duplicate<T>( this T[] array )
		{
			if( array == null || array.Length == 0 )
			{
				return array ;
			}

			return array.Clone() as T[] ;
		}

		/// <summary>
		/// ２次元配列
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static T[] GetColumns<T>( this T[,] array, int index )
		{
			if( array == null || array.GetLength( 0 ) == 0 || array.GetLength( 1 ) == 0 || index <  0 || index >= array.GetLength( 1 ) )
			{
				return null ;
			}

			int i, l = array.GetLength( 0 ) ;
			T[] columns = new T[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				columns[ i ] = array[ i, index ] ;
			}

			return columns ;
		}

		//---------------------------------------------------------------------------

		// 以下、必要になったら順次メソッドを追加する
	}
}

