using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// String 型のメソッド拡張
	/// </summary>
	public static class String_Extension
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty( this String s )
		{
			return string.IsNullOrEmpty( s ) ;
		}

		/// <summary>
		/// 文字列中の半角数値を全角数値に置き換える
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToLarge( this string s )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return "" ;
			}

			int i, l = s.Length ;

			char[] code = new char[ l ] ;

			char c ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = s[ i ] ;

				if( c >= '0' && c <= '9' )
				{
					c = ( char )( ( int )c - ( int )'0' + ( int )'０' ) ;
				}
				else
				if( c >= 'a' && c <= 'z' )
				{
					c = ( char )( ( int )c - ( int )'a' + ( int )'ａ' ) ;
				}
				else
				if( c >= 'A' && c <= 'Z' )
				{
					c = ( char )( ( int )c - ( int )'A' + ( int )'Ａ' ) ;
				}
				
				code[ i ] = c ;
			}

			return new string( code ) ;
		}

		/// <summary>
		/// 同じ長さの全て同じ文字の文字列に変換する
		/// </summary>
		/// <returns>The secret.</returns>
		/// <param name="s">S.</param>
		/// <param name="c">C.</param>
		public static string ToSecret( this string s, char c )
		{
			if( string.IsNullOrEmpty( s ) == true )
			{
				return s ;
			}
			
			int i, l = s.Length ;
			char[] a = new char[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				a[ i ] = c ;
			}
			
			return new string( a ) ;
		}
	}
}

