using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	public class ExperienceData
	{
		public	long	id ;

		public	int		level ;

		public	int		value ;
		public	int		total ;

		//---------------------------------------------------------------------------

		public static ExperienceData GetById( long id )
		{
			return MassData.ExperienceTable.FirstOrDefault( _ => _.id == id ) ;
		}

		/// <summary>
		/// 累積値を計算する
		/// </summary>
		public static void Prepare()
		{
			int total = 0 ;
			foreach( var e in MassData.ExperienceTable )
			{
				total += e.value ;
				e.total = total ;
			}
		}

		/// <summary>
		/// 指定の経験値からレベルを取得する
		/// </summary>
		/// <param name="experience"></param>
		/// <returns></returns>
		public static int GetLevel( int experience )
		{
			ExperienceData t = MassData.ExperienceTable.Where( _ => ( _.total > experience ) ).OrderBy( _ => _.level ).FirstOrDefault() ;
			
			return t != null ? t.level : MassData.ExperienceTable.Last().level ;
		}

		/// <summary>
		/// 指定したレベルでの次レベルになるために必要な経験値量を取得する
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public static int GetValue( int level )
		{
			ExperienceData t = MassData.ExperienceTable.FirstOrDefault( _ => level == _.level ) ;

			return ( t != null ) ? t.value : 0 ;
		}

		/// <summary>
		/// 指定したレベルになるまで取得したトータルの経験値量を取得する
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public static int GetTotal( int level )
		{
			ExperienceData t = MassData.ExperienceTable.FirstOrDefault( _ => level == _.level ) ;

			return ( t != null ) ? t.total : 0 ;
		}
	}
}
