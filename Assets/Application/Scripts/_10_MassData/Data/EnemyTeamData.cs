using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	public class EnemyTeamData
	{
		public long		id ;
		public string	name ;
		public long[]	front ;
		public long[]	back ;

		//---------------------------------------------------------------------------

		public static EnemyTeamData GetById( long id )
		{
			return MassData.EnemyTeamTable.FirstOrDefault( _ => _.id == id ) ;
		}
	}
}
