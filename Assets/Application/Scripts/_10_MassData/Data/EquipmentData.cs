using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// アイテム情報クラス
	/// </summary>
	public class EquipmentData
	{
		public	long	id ;			// 識別子

		public	string	name ;			// 名称

		public	int		part ;		// 部位[0=武器(主)・1=武器(補)・2=防具(体)・3=防具(頭)・4=防具(手)・5=防具(足)・6=装飾１・7=装飾２]
		public EquipmentPart Part
		{
			get
			{
				return ( EquipmentPart )part ;
			}
		}

		public	int		usable ;		// 職業
		public	bool	IsUsableClass( ClassType classType )
		{
			return ( ( usable & ( 1 << ( int )classType ) ) != 0 ) ;
		}


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

		public	int		width ;
		public	int		range ;
		public	float	weight ;

		public	string	description ;

		//---------------------------------------------------------------------------

		public static EquipmentData GetById( long id )
		{
			return MassData.EquipmentTable.FirstOrDefault( _ => _.id == id ) ;
		}

	}
}
