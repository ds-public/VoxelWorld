using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

using CSVHelper ;

using DBS.WorkDataCategory ;

namespace DBS
{
	/// <summary>
	/// ゲーム全体から参照される静的データを保持するクラス
	/// </summary>
	public class WorkData
	{
		//-------------------------------------------------------------------------------------------

		// 成長係数
		private Dictionary<string,GrowingFactor[]>	m_GrowingFactor = new Dictionary<string, GrowingFactor[]>() ;

		public static Dictionary<string,GrowingFactor[]>	GrowingFactor
		{
			get
			{
				return WorkDataManager.Instance.work.m_GrowingFactor ;
			}
			set
			{
				WorkDataManager.Instance.work.m_GrowingFactor = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		public static bool SetCacheData( string label, System.Object value, bool alive = false )
		{
			return WorkDataManager.SetCacheData( label, value, alive ) ;
		}

		public static T GetCacheData<T>( string label ) where T : class
		{
			return WorkDataManager.GetCacheData<T>( label ) ;
		}
	}
}
