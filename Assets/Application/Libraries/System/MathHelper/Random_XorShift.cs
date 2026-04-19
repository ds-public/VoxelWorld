using System ;
using System.Collections ;
using UnityEngine ;


/// <summary>
/// 乱数生成のパッケージ Version 2026/02/17
/// </summary>
namespace MathHelper
{
	/// <summary>
	/// インスタンスを生成しなくても使えるランダムクラス
	/// </summary>
	public static class Random_XorShift
	{
		private static readonly XorShift m_XorShift = new () ;

		static Random_XorShift()
		{
            SetSeed() ; // ０は不可

			// 初期化時に現在時刻を元にランダムな回数（最大10回）乱数を読み捨てる
			int i, l = DateTime.Now.Second ;
			for( i = 0 ; i < l ; i ++ )
			{
				m_XorShift.Get() ;
			}
		}

		public static ulong Seed
		{
			get
			{
				return m_XorShift.Seed ;
			}
			set
			{
				m_XorShift.Seed = value ;
			}
		}

		/// <summary>
		/// 時間値をシードにする
		/// </summary>
		public static void SetSeed( ulong? seed = null )
		{
			m_XorShift.Seed = ( seed == null ? ( ulong )GetUnixTime() : seed.Value ) ;
		}

		// UNIXエポックを表すDateTimeオブジェクトを取得
		private static readonly DateTime UNIX_EPOCH = new ( 1970, 1, 1, 0, 0, 0, 0 ) ;

		private static long GetUnixTime()
		{
			var dt = DateTime.Now ;

			// UTC時間に変換
			dt.ToUniversalTime() ;

			// UNIXエポックからの経過時間を取得
			TimeSpan elapsedTime = dt - UNIX_EPOCH ;
			
			// 経過秒数に変換
			return ( long )elapsedTime.TotalSeconds ;
		}


		public static ulong Get()
		{
			return m_XorShift.Get() ;
		}

		public static int Get( int max, bool limit = true )
		{
			return ( int )m_XorShift.Get( ( ulong )max, limit ) ;
		}

		public static int Get( int min, int max, bool limit = true, bool swap = false )
		{
			return m_XorShift.Get( min, max, limit, swap ) ;
		}

		public static long Get( long max, bool limit = true )
		{
			return ( long )m_XorShift.Get( ( ulong )max, limit ) ;
		}

		public static ulong Get( ulong max, bool limit = true )
		{
			return m_XorShift.Get( max, limit ) ;
		}

		public static float Get( float min, float max, bool swap = false )
		{
			return m_XorShift.Get( min, max, swap ) ;
		}
	}

	//---------------------------------------------------------

	/// <summary>
	/// XorShift アルゴリズムの乱数生成クラス
	/// </summary>
	public class XorShift
	{
		// 初期の根値
		private ulong m_RandomSeed0 ;
		private ulong m_RandomSeed1 ;

		/// <summary>
		/// コンストラクタ(デフォルト)
		/// </summary>
		public XorShift()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="seed"></param>
		public XorShift( ulong seed )
		{
			Seed = seed ;
		}

		/// <summary>
		/// 疑似乱数根
		/// </summary>
		public ulong Seed
		{
			get
			{
				return m_RandomSeed0 ;
			}
			set
			{
				ulong z = ( value += 0x9E3779B97F4A7C15UL ) ;
				z = ( z ^ ( z >> 30 ) ) * 0xBF58476D1CE4E5B9UL ;
				z = ( z ^ ( z >> 27 ) ) * 0x94D049BB133111EBUL ;
				m_RandomSeed0 = z ^ ( z >> 31 ) ;
				m_RandomSeed1 = m_RandomSeed0 ^ 0x9E3779B97F4A7C15UL;
			}
		}

		/// <summary>
		/// 整数値の範囲で乱数値を取得する(xorshift)
		/// </summary>
		/// <returns></returns>
		public ulong Get()
		{
            ulong s1 = m_RandomSeed0 ;
            ulong s0 = m_RandomSeed1 ;

            m_RandomSeed0 = s0 ;
            s1 ^= s1 << 23 ; // a
            m_RandomSeed1 = s1 ^ s0 ^ ( s1 >> 17 ) ^ ( s0 >> 26 ) ; // b, c
            return m_RandomSeed1 + s0 ; // 最後に加算するのが "+" の由来
        }

		/// <summary>
		/// ０から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public int Get( int max, bool limit = true )
		{
			if( max <= 0 )
			{
				return 0 ; // 値が不正
			}

			return ( int )( Get() % ( ulong )( max + ( limit ? 1 : 0 ) ) ) ;
		}

		/// <summary>
		/// ０から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public ulong Get( ulong max, bool limit = true )
		{
			if( max <= 0 )
			{
				return 0 ; // 値が不正
			}

			return ( ulong )( Get() % ( ulong )( max + ( ulong )( limit ? 1 : 0 ) ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns></returns>
		public int Get( int min, int max, bool limit = true, bool swap = false )
		{
			if( min >  max )
			{
				if( swap == true )
				{
					// 値を入れ替える
					( min, max ) = ( max, min ) ;
				}
				else
				{
					return 0 ;  // 値が不正
				}
			}

			if( min == max )
			{
				return min ;
			}

			return min + ( int )( Get() % ( ulong )( ( max - min ) + ( limit ? 1 : 0 ) ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の小数型乱数を返す
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns></returns>
		public float Get( float min, float max, bool isSwap = false )
		{
			if( min >  max )
			{
				if( isSwap == true )
				{
					// 値を入れ替える
					( min, max ) = ( max, min ) ;
				}
				else
				{
					return 0f ;  // 値が不正
				}
			}

			if( min == max )
			{
				return min ;
			}

			ulong r = Get() ;

			double ratio = ( double )r / ( double )0xFFFFFFFFFFFFFFFFL ;   // 百分率に変換する

			return min + ( float )( ( double )( max - min ) * ratio ) ;
		}

	}   // class
}   // namsepace
