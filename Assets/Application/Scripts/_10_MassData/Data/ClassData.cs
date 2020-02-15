using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	public class ClassData
	{
		public	long	id ;

		public	int		type ;

		public	string	name ;

		public	int		hp ;
		public	int		sp ;
		public	int		mp ;

		public	int		attack ;
		public	int		defense ;

		public	int		accuracy ;
		public	int		evasion ;

		public	int		intelligence ;
		public	int		mind ;
		public	int		grace ;

		public	int		speed ;

		public	int		vitality ;
		public	int		mentality ;
		public	int		luck ;

		//---------------------------------------------------------------------------

		public static ClassData GetById( long id )
		{
			return MassData.ClassTable.FirstOrDefault( _ => _.id == id ) ;
		}

		public static ClassData GetByType( long type )
		{
			return MassData.ClassTable.FirstOrDefault( _ => _.type == type ) ;
		}

	}
}
