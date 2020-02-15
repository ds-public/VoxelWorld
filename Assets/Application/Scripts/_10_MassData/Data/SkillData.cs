using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// スキル情報クラス
	/// </summary>
	public class SkillData
	{
		public	long	id ;	// 識別子

		public	string	name ;	// 名称
		public	string	Name
		{
			get
			{
				return name ;
			}
		}

		public	int		category ;	// SP か MP か
		public	int		Category
		{
			get
			{
				return category ;
			}
		}
		
		public	int		cost ;	// 消費コスト
		public	int		Cost
		{
			get
			{
				return cost ;
			}
		}
		

		public	long	influence_id ;	// 効果
		
		public	string	description ;	// 説明文
		public	string	Description
		{
			get
			{
				return description ;
			}
		}

		//---------------------------------------------------------------------------

		public static SkillData GetById( long id )
		{
			return MassData.SkillTable.FirstOrDefault( _ => _.id == id ) ;
		}
		
		public InfluenceData GetInfluence()
		{
			if( influence_id == 0 )
			{
				return null ;
			}
			
			return InfluenceData.GetById( influence_id ) ;
		}
		
		public int Scene
		{
			get
			{
				return GetInfluence().scene ;
			}
		}
		
		public int Area
		{
			get
			{
				return GetInfluence().area ;
			}
		}
		
		public int Width
		{
			get
			{
				return GetInfluence().width ;
			}
		}
		
		public int Range
		{
			get
			{
				return GetInfluence().range ;
			}
		}

		public bool Alive
		{
			get
			{
				return GetInfluence().alive ;
			}
		}

		public int Process
		{
			get
			{
				return GetInfluence().process ;
			}
		}

		public int[] Data
		{
			get
			{
				return GetInfluence().data ;
			}
		}
		
		public long EffectId
		{
			get
			{
				return GetInfluence().effect_id ;
			}
		}
		
		public int EffectTarget
		{
			get
			{
				return GetInfluence().effect_target ;
			}
		}
		
	}
}
