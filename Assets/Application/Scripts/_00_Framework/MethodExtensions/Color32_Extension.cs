using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// Color32 型のメソッド拡張(注意:構造体の拡張メソッドには ref が必要)
	/// </summary>
	public static class Color32_Extension
	{
		/// <summary>
		/// 32ビット符号なし整数から色を設定する
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static void SetARGB( ref this Color32 c, uint argb )
		{
			c.r = ( byte )( ( argb >> 16 ) & 0xFF ) ;
			c.g = ( byte )( ( argb >>  8 ) & 0xFF ) ;
			c.b = ( byte )(   argb         & 0xFF ) ;
			c.a = ( byte )( ( argb >> 24 ) & 0xFF ) ;
		}

		/// <summary>
		/// 色を設定する
		/// </summary>
		/// <param name="c"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		public static void SetRGBA( ref this Color32 c, byte r, byte g, byte b, byte a )
		{
			c.r = r ;
			c.g = g ;
			c.b = b ;
			c.a = a ;
		}

		/// <summary>
		/// 別の色を上書きする
		/// </summary>
		/// <param name="c0"></param>
		/// <param name="c1"></param>
		public static void Set( ref this Color32 c0, Color32 c1 )
		{
			c0.r = c1.r ;
			c0.g = c1.g ;
			c0.b = c1.b ;
			c0.a = c1.a ;
		}
	}
}

