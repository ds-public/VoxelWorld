using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// 端末時刻の取得クラス Version 2022/09/19
	/// </summary>
	public class ClientTime
	{
		private static readonly DateTime m_UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

		private static int m_CorrectTime = 0 ;  // サーバータイムとクライアントタイムの補正用の差分値

		/// <summary>
		/// 現在時刻
		/// </summary>
		public static long Now	=> GetCurrentUnixTime() ;


		public static long UTC	=> Now ;

		/// <summary>
		/// 現在時刻
		/// </summary>
		public static long LTC
		{
			get
			{
				//----------------------------------
				// タイムゾーンの補正を加算する(UTCであるため)
				TimeZoneInfo zone = TimeZoneInfo.Local ;
				TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;

				return UTC + ( long )offset.TotalSeconds ;
			}
		}

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static long GetRealCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now ) ;
		}

		/// <summary>
		/// 現在時刻(JST)を取得する
		/// </summary>
		/// <returns></returns>
		public static long GetCurrentUnixTime()
		{
			return GetUnixTime( DateTime.Now, m_CorrectTime ) ;
		}

		private static long GetUnixTime( DateTime targetTime, long correctTime = 0 )
		{
			// 1970/01/01 を減算する
			TimeSpan dateTime = targetTime - m_UNIX_EPOCH ;

			// タイムゾーンの補正を減算する
			TimeZoneInfo zone = TimeZoneInfo.Local ;
			TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
			dateTime -= offset ;

			return ( long )dateTime.TotalSeconds ;
		}

		/// <summary>
		/// Tick に変換する
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		/// <param name="hour"></param>
		/// <param name="minute"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static long GetTick( int year, int month, int day, int hour, int minute, int second, bool isUtc = true )
		{
			// ローカルタイムのものが取得される
			DateTime targetTime = new DateTime( year, month, day, hour, minute, second ) ;

			// 1970/01/01 を減算する
			TimeSpan dateTime = targetTime - m_UNIX_EPOCH ;

			//----------------------------------------------------------
			if( isUtc == true )
			{
				// タイムゾーンの補正を減算する
				TimeZoneInfo zone = TimeZoneInfo.Local ;
				TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
				dateTime -= offset ;
			}
			//----------------------------------------------------------

			return ( long )dateTime.TotalSeconds ;
		}

		public static long GetTodayUnixTime( long tickTime = 0 )
		{
			if( tickTime >  0 )
			{
				DateTime now = ToDateTime( tickTime ) ;
				return GetUnixTime( new DateTime( now.Year, now.Month, now.Day, 0, 0, 0 ) ) ;
			}
			else
			{
				return GetUnixTime( DateTime.Today ) ;
			}
		}

		/// <summary>
		/// サーバータイムとクライアントタイムの補正用の差分値を保存する
		/// </summary>
		/// <param name="correctTime"></param>
		public static void SetCorrectTime( int correctTime )
		{
			m_CorrectTime = correctTime ;
		}

		/// <summary>
		/// 日時クラスのインスタンスを取得する
		/// </summary>
		/// <param name="unixTime"></param>
		/// <returns></returns>
		public static DateTime ToDateTime( long tick = 0, bool isUtc = true )
		{
			if( tick == 0 )
			{
				tick = Now ;
			}
			
			// 1970/01/01 を加算する
			DateTime dateTime = m_UNIX_EPOCH.AddSeconds( tick ) ;

			//----------------------------------
			if( isUtc == true )
			{
				// UTCの場合タイムゾーンの補正を加算する(UTCであるため)
				TimeZoneInfo zone = TimeZoneInfo.Local ;
				TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
				dateTime += offset ;
			}
			//----------------------------------

			return dateTime ;
		}


		/// <summary>
		/// 日時文字列を取得する(デバッグ用)
		/// </summary>
		/// <param name="unixTime"></param>
		/// <returns></returns>
		public static string GetDateTimeString( long tickTime = 0 )
		{
			if( tickTime == 0 )
			{
				tickTime = GetCurrentUnixTime() ;
			}

			DateTime dt = ToDateTime( tickTime ) ;

			return string.Format( "{0,0:D4}/{1,0:D2}/{2,0:D2} {3,0:D2}:{4,0:D2}:{5,0:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second ) ;
		}
	}
}
