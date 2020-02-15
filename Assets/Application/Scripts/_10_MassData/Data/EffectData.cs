using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS.MassDataCategory
{
	public class EffectData
	{
		public	long	id ;

		public	string	resource_id ;

		public	float	speed ;

		public	string	se_id ;

		//-----------------------------------------------------------

		public static EffectData GetById( long id )
		{
			return MassData.EffectTable.FirstOrDefault( _ => _.id == id ) ;
		}
	}
}
