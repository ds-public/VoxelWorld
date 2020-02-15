using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// 効果情報クラス
	/// </summary>
	public class InfluenceData
	{
		public	long	id ;	// 識別子

		public	string	name ;	// 効果名称(使用しない)
		
		// 使用可能状況
		public	int		scene ;	// 1=キャンプ・2=バトル・3=両方

		// 対象
		public	int		area ;	// 味方か敵か
		public	int		width ;	// 範囲
		public	int		range ;	// 距離
		public	bool	alive ;	// 生存限定か

		public	int		process ;	// 処理

		public	int[]	data ;		// 多目的パラメータ

		public	long	effect_id ;		// 表示エフェクト
		public	int		effect_target ;	// 表示エフェクトの表示タイプ(全体型か個体型か)

		//---------------------------------------------------------------------------

		public static InfluenceData GetById( long id )
		{
			return MassData.InfluenceTable.FirstOrDefault( _ => _.id == id ) ;
		}
	}
}
