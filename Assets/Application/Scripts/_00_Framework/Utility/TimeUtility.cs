using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	public class TimeUtility
	{
		private static DateTime UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		private static int m_CorrectTime = 0 ;  // サーバータイムとクライアントタイムの補正用の差分値

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetRealCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now ) ;
		}

		/// <summary>
		/// サーバータイムとクライアントタイムの補正用の差分値を保存する
		/// </summary>
		/// <param name="tCorrectTime"></param>
		public static void SetCorrectTime( int tCorrectTime )
		{
			m_CorrectTime = tCorrectTime ;
		}

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now, m_CorrectTime ) ;
		}

		private static int GetUnixTime( DateTime tTargetTime, int tCorrectTime = 0 )
		{
			// UTC時間に変換
			tTargetTime = tTargetTime.ToUniversalTime() ;

			// UNIXエポックからの経過時間を取得
			TimeSpan tElapsedTime = tTargetTime - UNIX_EPOCH ;

			// 経過秒数に変換
			return ( int )tElapsedTime.TotalSeconds + tCorrectTime ;
		}

		public static int GetTodayUnixTime( long tUnixTime = 0 )
		{
			if( tUnixTime >  0 )
			{
				DateTime tNow = UnixTimeToDateTime( tUnixTime ) ;
				return GetUnixTime( new DateTime( tNow.Year, tNow.Month, tNow.Day, 0, 0, 0 ) ) ;
			}
			else
			{
				return GetUnixTime( DateTime.Today ) ;
			}
		}

		/// <summary>
		/// 日時クラスのインスタンスを取得する
		/// </summary>
		/// <param name="tUnixTime"></param>
		/// <returns></returns>
		public static DateTime UnixTimeToDateTime( long tUnixTime = 0 )
		{
			if( tUnixTime == 0 )
			{
				tUnixTime = GetCurrentUnixTime() ;
			}

			TimeSpan tElapsedTime = new TimeSpan( 0, 0, ( int )tUnixTime ) ;
			
			return new DateTime( tElapsedTime.Ticks + UNIX_EPOCH.Ticks ).ToLocalTime() ;
		}


		/// <summary>
		/// 日時文字列を取得する(デバッグ用)
		/// </summary>
		/// <param name="tUnixTime"></param>
		/// <returns></returns>
		public static string GetDateTimeString( long tUnixTime  = 0 )
		{
			if( tUnixTime == 0 )
			{
				tUnixTime = GetCurrentUnixTime() ;
			}

			DateTime dt = UnixTimeToDateTime( tUnixTime ) ;

			return string.Format( "{0,0:D4}/{1,0:D2}/{2,0:D2} {3,0:D2}:{4,0:D2}:{5,0:D2}", dt.Year, dt.Month, dt.DayOfYear, dt.Hour, dt.Minute, dt.Second ) ;
		}
	}
}
