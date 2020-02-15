using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;


using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;


/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS.WorkDataCategory
{
	/// <summary>
	/// ゲーム全体から参照されるプレイヤー系データを保持するクラス
	/// </summary>
	[Serializable]
	public class GrowingFactor
	{
		public int		level ;
		public double	value ;

		public GrowingFactor( int tLevel, double tValue )
		{
			level	= tLevel ;
			value	= tValue ;
		}


		public static GrowingFactor[] Create( int tLevel_1, double tValue_1, int tLevel_2, double tValue_2 )
		{
			if( tLevel_1 >  tLevel_2 )
			{
				int		tSwap_Level = tLevel_1 ;
				double	tSwap_Value = tValue_1 ;

				tLevel_1 = tLevel_2 ;
				tValue_1 = tValue_2 ;

				tLevel_2 = tSwap_Level ;
				tValue_2 = tSwap_Value ;
			}


			double tL1		= tLevel_1 - 1 ;
			double tL1m2	= ( tLevel_1 * tLevel_1 - 1 ) ;
			double tV1		= tValue_1 - 1 ;

			double tL2		= tLevel_2 - 1 ;
			double tL2m2	= ( tLevel_2 * tLevel_2 - 1 ) ;
			double tV2		= tValue_2 - 1 ;

			double a = ( tL2   * tV1 - tL1   * tV2 ) / ( tL2 * tL1m2 - tL1 * tL2m2 ) ;
			double b = ( tL2m2 * tV1 - tL1m2 * tV2 ) / ( tL1 * tL2m2 - tL2 * tL1m2 ) ;
			double c = 1 - ( a + b ) ;


			GrowingFactor[] tGF = new GrowingFactor[ tLevel_2 + 1 ] ;

			int tLevel ;

			for( tLevel  = 0 ; tLevel <= tLevel_2 ; tLevel ++ )
			{
				tGF[ tLevel ] = new GrowingFactor( tLevel, a * tLevel * tLevel + b * tLevel + c ) ;

//				Debug.LogWarning( "成長係数: Level = " + tGF[ tLevel ].level + " Value = " + tGF[ tLevel ].value ) ;
			}

			return tGF ;
		}

	}
}
