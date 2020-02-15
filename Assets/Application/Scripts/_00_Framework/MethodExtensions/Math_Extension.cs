using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// Math 型のメソッド拡張
	/// </summary>
	public static class Mathi
	{
		/// <summary>
		/// 切り上げを行う(整数版)
		/// </summary>
		/// <returns>The ceiling.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static int Ceiling( int a, int b )
		{
			if( b == 0 )
			{
				return 0 ;
			}

			int c = a / b ;
			if( ( a % b ) >  0 )
			{
				c ++ ;
			}

			return c ;
		}
		
		/// <summary>
		/// 浮動小数値がほぼ0かどうかを判定する
		/// </summary>
		/// <returns><c>true</c>, if zero was ised, <c>false</c> otherwise.</returns>
		/// <param name="v">V.</param>
		/// <param name="threshold">Threshold.</param>
		public static bool IsZero( float v, float threshold = 0.00001f )
		{
			v = Mathf.Abs( v ) ;
			return ( v <  threshold ) ;
		}
		
		/// <summary>
		/// ほぼ同じかどうか判定する
		/// </summary>
		/// <returns><c>true</c>, if same was ised, <c>false</c> otherwise.</returns>
		/// <param name="v0">V0.</param>
		/// <param name="v1">V1.</param>
		/// <param name="threshold">Threshold.</param>
		public static bool IsSame( float v0, float v1, float threshold = 0.00001f )
		{
			return ( v0 <= ( v1 + threshold ) && v0 >= ( v1 - threshold ) ) ;
		}

		/// <summary>
		/// 整数値の符号を返す
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static int Sign( int v )
		{
			if( v >  0 )
			{
				return  1 ;
			}
			else
			if( v <  0 )
			{
				return -1 ;
			}
			else
			{
				return  0 ;
			}
		}
	}
}
