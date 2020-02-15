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
	public class BattleUnit : NormalUnit
	{
		public enum ActionCategory
		{
			Unknown = -1,
			Defend	=  0,
			Attack	=  1,
			Skill	=  2,
			item	=  3,
			Move	=  4,
			Away	=  5,
			
			Win		= 10000,	// 勝利:デバッグ
			Lose	= 10001,	// 敗北:デバッグ
		}

		[Serializable]
		public class ActionTarget
		{
			public int	line = -1 ;
			public int	side = -1 ;
		}


		[Serializable]
		public class ActionData
		{
			public	ActionCategory	category ;
			public	long			skillId ;
			public	ActionTarget	target = new ActionTarget() ;

			public ActionData()
			{
				category = ActionCategory.Unknown ;
			}

			public ActionData( ActionCategory tCategory, long tSkillId = 0 )
			{
				category	= tCategory ;
				skillId		= tSkillId ;
			}
		}
		
		//-----------------------------------------------------------
		
		public	long	action_pattern_id ;

		//-----------------------------------------------------------

		public	int		action_wait ;
		public	int		action_type ;

		public	bool	guard ;

		public	int		turn ;		// 経過ターン

		public	uint	condition ;	// 状態
		
		//-----------------------------------------------------------

		public	ActionData	action = new ActionData() ;

		//-------------------------------------------------------------------------------------------

		// プレイヤー側
		public BattleUnit( __u.UnitData tUnit, int tLine, int tSide ) : base( tUnit, tLine, tSide )
		{
			Setup() ;
		}
		
		// エネミー側
		public BattleUnit( __m.EnemyUnitData tUnit, int tLine, int tSide ) : base( tUnit, tLine, tSide )
		{
			action_pattern_id	= tUnit.action_pattern_id ;

			//----------------------------------

			Setup() ;
		}

		//-------------------------------------------------------------------------------------------

/*		public int[]	Duplicate( int[] tData )
		{
			int l = tData.Length ;

			int[]	tCopy = new int[ l ] ;
			Array.Copy( tData, tCopy, l ) ;

			return tCopy ;
		}

		public long[]	Duplicate( long[] tData )
		{
			int l = tData.Length ;

			long[]	tCopy = new long[ l ] ;
			Array.Copy( tData, tCopy, l ) ;

			return tCopy ;
		}

		public long[]	DuplicateSkillId( long[] tData )
		{
			List<long> tSkillId = new List<long>() ;

			int i, l = tData.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tData[ i ] >  0 )
				{
					tSkillId.Add( tData[ i ] ) ;
				}
			}

			if( tSkillId.Count == 0 )
			{
				return null ;
			}

			return tSkillId.ToArray() ;
		}*/




		public void Setup()
		{
//			hp_now = 1 ;	瀕死デバッグ

			turn = 0 ;

			// 最初の待ち時間
			UpdateActionWait() ;
		}

		// 待ち時間を更新する
		public void UpdateActionWait()
		{
			action_wait = 2000 / Speed ;
			action_type = 0 ;
		}

		// 現在の行動に対する待ち時間を取得する(後でスキルの種類などで変化する適切な値が取れるようにする)
		public int GetNextActionWait()
		{
			return 2000 / Speed ;
		}
	}
}
