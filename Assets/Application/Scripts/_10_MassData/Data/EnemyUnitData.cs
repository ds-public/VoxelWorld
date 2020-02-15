using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m=DBS.MassDataCategory ;

namespace DBS.MassDataCategory
{
	public class EnemyUnitData
	{
		public	long	id ;

		public	string	name ;

		public	int		level ;

		public	int		hp_now ;
		public	int		hp_max ;

		public	int		sp_now ;
		public	int		sp_max ;

		public	int		mp_now ;
		public	int		mp_max ;

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

		//-----------------------------------

		public	int[]	regist ;	// 物理耐性

		public	int		range ;

		//---------------------------------------------------------------------------

		/// <summary>
		/// 射程距離
		/// </summary>
		public int Range
		{
			get
			{
				// 射程距離(後で武器から取るようにする)
				return range ;
			}
		}


		public	long	action_pattern_id ;

		public	long[]	skill_ids ;	// 使用可能なスキル

		public	string	image ;

		//---------------------------------------------------------------------------

		public static EnemyUnitData GetById( long id )
		{
			return MassData.EnemyUnitTable.FirstOrDefault( _ => _.id == id ) ;
		}

		//-----------------------------------------------------------
		// スキル関係

		/// <summary>
		/// 所持しているスキルを取得する(1=移動中可能・2=戦闘中可能)
		/// </summary>
		/// <returns></returns>
		public __m.SkillData[] GetSkills( int priority, bool filter )
		{
			if( skill_ids.IsNullOrEmpty() == true )
			{
				// 無し
				return null ;
			}

			List<__m.SkillData> skills = new List<__m.SkillData>() ;

			skill_ids.ForEach
			(
				( long id ) =>
				{
					if( id >  0 )
					{
						// 有効な識別子
						__m.SkillData skill = __m.SkillData.GetById( id ) ;
						if( priority == 0 || ( priority >= 1 && ( skill.Scene & priority ) != 0 ) )
						{
							skills.Add( skill ) ;
						}
					}
				}
			) ;

			if( skills.Count == 0 )
			{
				// 使用可能なスキルは１つも無い
				return null ;
			}

			if( priority == 0 || filter == true )
			{
				// 全対象なのでここで終了(もしくは使用可能なもののみ)
				return skills.ToArray() ;
			}

			//----------------------------------
			// 除外された側のカテゴリのスキルを追加する

			skill_ids.ForEach
			(
				( long id ) =>
				{
					if( id >  0 )
					{
						// 有効な識別子
						__m.SkillData skill = __m.SkillData.GetById( id ) ;
						if( priority == 0 || ( priority >= 1 && ( skill.Scene & priority ) != 0 ) )
						{
							if( skills.Contains( skill ) == false )
							{
								skills.Add( skill ) ;
							}
						}
					}
				}
			) ;

			return skills.ToArray() ;
		}


	}
}
