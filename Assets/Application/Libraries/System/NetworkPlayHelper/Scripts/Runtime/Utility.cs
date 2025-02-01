#nullable enable
#pragma warning disable CA1822
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable IDE0028
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable IDE0300
#pragma warning disable IDE0305


using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;
using System.Security.Cryptography ;



namespace NetworkPlayHelper
{
	/// <summary>
	/// 時間計測用のクラス
	/// </summary>
	public class Timer
	{
		// ※１ティックは１００ナノ秒
		// ※１ミリ秒＝１００００ティック
		private const long m_CorrectValue = 10000 ;

		// タイマー計測開始の基準時間(単位はミリ秒)
		private long m_BaseTicks ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Timer()
		{
			// 現在基準時刻を記録する
			Start() ;
		}

		/// <summary>
		/// 現在基準時刻を更新する
		/// </summary>
		public void Start()
		{
			m_BaseTicks = DateTime.Now.Ticks / m_CorrectValue ;
		}

		/// <summary>
		/// 計測開始からの経過時間を Long 値で取得する(単位はミリ秒)
		/// </summary>
		public long DaltaTicks
		{
			get
			{
				long nowTicks = DateTime.Now.Ticks / m_CorrectValue ;

				return nowTicks - m_BaseTicks ;
			}
		}

		/// <summary>
		/// 計測開始からの経過時間を float 値で取得する(単位は秒)
		/// </summary>
		public float Dalta
		{
			get
			{
				long nowTicks = DateTime.Now.Ticks / m_CorrectValue ;

				return ( float )( nowTicks - m_BaseTicks ) / 1000.0f ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 現在日時を Long 値で取得する(単位はミリ秒)
		/// </summary>
		public static long NowTicks
		{
			get
			{
				return DateTime.Now.Ticks / m_CorrectValue ;
			}
		}

		/// <summary>
		/// 現在日時を float 値で取得する(単位は秒)
		/// </summary>
		public static float Now
		{
			get
			{
				return ( float )( DateTime.Now.Ticks / m_CorrectValue ) / 1000.0f ;
			}
		}
	}
}
