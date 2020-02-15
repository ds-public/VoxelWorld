using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// アクションパターン
	/// </summary>
	public class ActionPatternData
	{
		public enum SignType
		{
			Unknown,
			E,
			NE,
			UE,
			U,
			DE,
			D,
		}

		/// <summary>
		/// 条件と対象で共通機能をまとめたクラス
		/// </summary>
		public class FunctionBase
		{
			// 文字列の最初に判定文字のいずれかがあるか確認する(合致した場合は削除する)
			protected bool IsWord( ref string rString, params string[] tWord )
			{
				int i, l = tWord.Length ;
				string c0 = rString.ToLower() ;
				string c1 ;

				for( i = 0 ; i < l ; i++ )
				{
					c1 = tWord[ i ].ToLower() ;
					if( c0.IndexOf( c1, System.StringComparison.CurrentCulture ) == 0 )
					{
						break ;
					}
				}

				if( i >= l )
				{
					// 無し
					return false ;
				}

				// 有り
				l = tWord[ i ].Length ;
				rString = rString.Substring( l, rString.Length - l ) ;

				return true ;
			}

			// 文字列の最初に判定文字のいずれかがあるか確認する(合致した場合は削除しない)
			protected bool IsWordAndKeep( ref string rString, params string[] tWord )
			{
				int i, l = tWord.Length ;
				string c0 = rString.ToLower() ;
				string c1 ;

				for( i = 0 ; i < l ; i++ )
				{
					c1 = tWord[ i ].ToLower() ;
					if( c0.IndexOf( c1, System.StringComparison.CurrentCulture ) == 0 )
					{
						break ;
					}
				}

				if( i >= l )
				{
					// 無し
					return false ;
				}

				// 有り
				return true ;
			}

			// 符号種別を取得する
			protected SignType GetSign( ref string rString )
			{
				SignType tSign = SignType.Unknown ;

				if( IsWord( ref rString, "==", "=" ) == true )
				{
					tSign = SignType.E ;	// 同一
				}
				else
				if( IsWord( ref rString, "!=" ) == true )
				{
					tSign = SignType.NE ;
				}
				else
				if( IsWord( ref rString, "<=" ) == true )
				{
					tSign = SignType.DE ;	// 以下
				}
				else
				if( IsWord( ref rString, "<" ) == true )
				{
					tSign = SignType.D ;	// 未満
				}
				else
				if( IsWord( ref rString, ">=" ) == true )
				{
					tSign = SignType.UE ;	// 以上
				}
				else
				if( IsWord( ref rString, ">" ) == true )
				{
					tSign = SignType.U ;	// 超過
				}

				return tSign ;
			}

			protected UnitCondition GetState( ref string rString )
			{
				if( IsWord( ref rString, "正常", "NONE" ) == true )
				{
					return UnitCondition.Normal ;
				}
				else
				if( IsWord( ref rString, "強化", "ENHANCEMENT" ) == true )
				{
					return UnitCondition.Enhancement ;
				}
				else
				if( IsWord( ref rString, "弱化", "WEAKENING" ) == true )
				{
					return UnitCondition.Weakening ;
				}
				else
				if( IsWord( ref rString, "猛毒", "POISON" ) == true )
				{
					return UnitCondition.Poison ;
				}
				else
				if( IsWord( ref rString, "麻痺", "PARALYSIS" ) == true )
				{
					return UnitCondition.Paralysis ;
				}
				else
				if( IsWord( ref rString, "睡眠", "SLEEP" ) == true )
				{
					return UnitCondition.Sleep ;
				}
				else
				if( IsWord( ref rString, "混乱", "CONFUSION" ) == true )
				{
					return UnitCondition.Confusion ;
				}
				else
				if( IsWord( ref rString, "魅了", "CHARM" ) == true )
				{
					return UnitCondition.Charm ;
				}
				else
				if( IsWord( ref rString, "沈黙", "SILENCE" ) == true )
				{
					return UnitCondition.Silence ;
				}
				else
				if( IsWord( ref rString, "拘束", "BIND" ) == true )
				{
					return UnitCondition.Bind ;
				}
				else
				if( IsWord( ref rString, "石化", "STONE" ) == true )
				{
					return UnitCondition.Stone ;
				}

				return UnitCondition.Unknown ;
			}

			//----------------------------------------------------------

			// 指定の値が条件を満たしているか判定する
			protected bool CheckValue( int tValue, SignType tValueSign, int tValue_0, int tValue_1 )
			{
				switch( tValueSign )
				{
					case SignType.E :
						if( tValue_1 <  0 )
						{
							// 単独値
							if( tValue == tValue_0 )
							{
								return true ;
							}
						}
						else
						{
							// 範囲値
							if( tValue >= tValue_0 && tValue <= tValue_1 )
							{
								return true ;
							}
						}
					break ;
					
					case SignType.NE :
						if( tValue_1 <  0 )
						{
							// 単独値
							if( tValue != tValue_0 )
							{
								return true ;
							}
						}
						else
						{
							// 範囲値
							if( tValue <  tValue_0 || tValue >  tValue_1 )
							{
								return true ;
							}
						}
					break ;
					
					case SignType.UE :
						if( tValue >= tValue_0 )
						{
							return true ;
						}
					break ;
					
					case SignType.U :
						if( tValue >  tValue_0 )
						{
							return true ;
						}
					break ;
					
					case SignType.DE :
						if( tValue <= tValue_0 )
						{
							return true ;
						}
					break ;
					
					case SignType.D :
						if( tValue <  tValue_0 )
						{
							return true ;
						}
					break ;
				}
				
				return false ;
			}

			protected bool CheckState( uint tUnitCondition, SignType tValueSign, UnitCondition tState )
			{
				int i, l ;

				uint tMask = 0 ;
				if( tState == UnitCondition.Enhancement )
				{
				}
				else
				if( tState == UnitCondition.Weakening )
				{
					UnitCondition[] tCL =
					{
						UnitCondition.Poison,
						UnitCondition.Paralysis,
						UnitCondition.Sleep,
						UnitCondition.Confusion,
						UnitCondition.Charm,
						UnitCondition.Silence,
						UnitCondition.Bind,
						UnitCondition.Stone,
					} ;

					l = tCL.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tMask = ( tMask | ( uint )( 1 << ( int )tCL[ i ] ) ) ;
					}
				}
				else
				{
					tMask = ( uint )( 1 << ( int )tState ) ;
				}

				if( tValueSign == SignType.E )
				{
					if( ( tState == UnitCondition.Normal && tUnitCondition == 0 ) || ( tUnitCondition & tMask ) != 0 )
					{
						return true ;
					}
				}
				else
				if( tValueSign == SignType.NE )
				{
					if( ( tState == UnitCondition.Normal && tUnitCondition != 0 ) || ( tUnitCondition & tMask ) == 0 )
					{
						return true ;
					}
				}

				return false ;
			}
		}

		//-------------------------------------------------------------------------------------------

		public enum ConditionValueCategory
		{
			Turn,
			Hp,
			Sp,
			Mp,
			State,
		}

		public enum ConditionTargetType
		{
			Unknown,
			Myself,
			Player,
			Player_Front,
			Player_Back,
			Enemy,
			Enemy_Front,
			Enemy_Back,
			Target,
		}

		public class ConditionData : FunctionBase
		{
			public	ConditionValueCategory	valueCategory ;
			public	SignType				valueSign ;
			public	int[]					value = new int[ 2 ] ;
			public	UnitCondition			state ;

			public	ConditionTargetType		targetType ;
			public	SignType				targetSign ;
			public	int[]					targetCount = new int[ 2 ] ;
			public	bool					targetAll ;

			// 条件を展開する
			public bool Set( string tCondition )
			{
				// 対象を判定する
				if( IsWord( ref tCondition, "Turn", "ターン" ) == true )
				{
					// ターンをチェックする
					valueCategory = ConditionValueCategory.Turn ;

					// サインタイプを取得
					valueSign = GetSign( ref tCondition ) ;
					if( valueSign != SignType.E && valueSign != SignType.NE )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					value[ 0 ] = GetNumber( ref tCondition, out value[ 1 ] ) ;

					if( value[ 0 ] <= 0 || ( value[ 1 ] == 1 && value[ 0 ] <  2 ) )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}
				}
				else
				if
				(
					IsWordAndKeep( ref tCondition, "HP", "ＨＰ" ) == true ||
					IsWordAndKeep( ref tCondition, "SP", "ＳＰ" ) == true ||
					IsWordAndKeep( ref tCondition, "MP", "ＭＰ" ) == true
				)
				{
					// カテゴリ
					if( IsWord( ref tCondition, "HP", "ＨＰ" ) == true )
					{
						valueCategory = ConditionValueCategory.Hp ;
					}
					else
					if( IsWord( ref tCondition, "SP", "ＳＰ" ) == true )
					{
						valueCategory = ConditionValueCategory.Sp ;
					}
					else
					if( IsWord( ref tCondition, "MP", "ＭＰ" ) == true )
					{
						valueCategory = ConditionValueCategory.Mp ;
					}

					//---------------------------------

					// サインタイプを取得
					valueSign = GetSign( ref tCondition ) ;

					if( valueSign == SignType.Unknown )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					value[ 0 ] = GetValue( ref tCondition, out value[ 1 ] ) ;

					if( value[ 0 ] <  0 )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					// 対象情報取得
					targetType = GetTarget( ref tCondition, out targetSign, out targetCount[ 0 ], out targetCount[ 1 ], out targetAll ) ;
					if( targetType == ConditionTargetType.Unknown )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}
				}
				else
				if( IsWord( ref tCondition, "State", "状態" ) == true )
				{
					// ターンをチェックする
					valueCategory = ConditionValueCategory.State ;

					// サインタイプを取得
					valueSign = GetSign( ref tCondition ) ;
					if( valueSign != SignType.E && valueSign != SignType.NE )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					state = GetState( ref tCondition ) ;

					if( state == UnitCondition.Unknown )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					// 対象情報取得
					targetType = GetTarget( ref tCondition, out targetSign, out targetCount[ 0 ], out targetCount[ 1 ], out targetAll ) ;
					if( targetType == ConditionTargetType.Unknown )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}
				}

				return true ;
			}

			//------------------------------------------------------------------------------------------

			// ターン数を取得する
			private int GetNumber( ref string rString, out int oMultiple )
			{
				oMultiple = 0 ;

				int i, l = rString.Length ;
				char c ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return 0 ;
				}

				string tNumberCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tNumberCode, out int tNumber ) ;
			
				if( rString.Length >  0 )
				{
					if( rString[ 0 ] == 'x' || rString[ 0 ] == 'X' )
					{
						oMultiple = 1 ;
						rString = rString.Substring( 1, rString.Length - 1 ) ;
					}
				}

				return tNumber ;
			}

			// 能力判定値を取得する
			private int GetValue( ref string rString, out int oValue )
			{
				int i, l ;
				char c ;

				string tValueCode ;
				
				oValue = -1 ;

				//----------------------------------------------------------

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tValueCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tValueCode, out int tValue ) ;

				if( rString.Length == 0 || rString[ 0 ] != '%' )
				{
					// 不正
					return -1 ;
				}

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				if( rString.Length == 0 || rString[ 0 ] != '^' )
				{
					// ここで終了
					return tValue ;
				}

				//----------------------------------

				// 範囲指定

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tValueCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tValueCode, out oValue ) ;

				if( rString.Length == 0 || rString[ 0 ] != '%' )
				{
					// 不正
					return -1 ;
				}

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				if( oValue <  tValue )
				{
					// 入れ替え
					int tSwap = tValue ;
					tValue = oValue ;
					oValue = tSwap ;
				}

				//----------------------------------

				return tValue ;
			}

			// 対象を取得する
			private ConditionTargetType GetTarget( ref string rString, out SignType oSign, out int oCount_0, out int oCount_1, out bool oAll )
			{
				oSign = SignType.Unknown ;
				oCount_0 =  0 ;
				oCount_1 = -1 ;
				oAll = false ;

				//----------------------------------------------------------

				if( rString.Length == 0 )
				{
					return ConditionTargetType.Myself ;	// 自身を対象
				}

				if( rString[ 0 ] != ':' )
				{
					// 不正
					return ConditionTargetType.Unknown ;
				}

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				//----------------------------------------------------------

				ConditionTargetType tTarget = ConditionTargetType.Unknown ;

				if( IsWord( ref rString, "m", "自身", "自分" ) == true )
				{
					tTarget = ConditionTargetType.Myself ;
				}
				else
				if( IsWord( ref rString, "pf", "味方前衛" ) == true )
				{
					tTarget = ConditionTargetType.Player_Front ;
				}
				else
				if( IsWord( ref rString, "pb", "味方後衛" ) == true )
				{
					tTarget = ConditionTargetType.Player_Back ;
				}
				else
				if( IsWord( ref rString, "pa", "p", "味方全体", "味方" ) == true )
				{
					tTarget = ConditionTargetType.Player ;
				}
				else
				if( IsWord( ref rString, "ef", "敵前衛" ) == true )
				{
					tTarget = ConditionTargetType.Enemy_Front ;
				}
				else
				if( IsWord( ref rString, "eb", "敵後衛" ) == true )
				{
					tTarget = ConditionTargetType.Enemy_Back ;
				}
				else
				if( IsWord( ref rString, "ea", "e", "敵全体", "敵" ) == true )
				{
					tTarget = ConditionTargetType.Enemy ;
				}
				else
				if( IsWord( ref rString, "t", "対象" ) == true )
				{
					tTarget = ConditionTargetType.Target ;
				}
			
				if( tTarget == ConditionTargetType.Myself || tTarget == ConditionTargetType.Unknown )
				{
					// 数指定は出来ないのでここで終了
					return tTarget ;
				}
			
				//----------------------------------------------------------

				// 指定が無い場合は >0 (>=1) 扱いとなる

				if( rString.Length == 0 )
				{
					oSign = SignType.U ;
					oCount_0 =  0 ;
					oCount_1 = -1 ;

					return tTarget ;
				}

				oSign = GetSign( ref rString ) ;
				if( oSign == SignType.Unknown )
				{
					// 不正
					return ConditionTargetType.Unknown ;
				}

				// 数を取得する
				oCount_0 = GetCount( ref rString, out oCount_1, out oAll ) ;
				if( oCount_0 <  0 )
				{
					return ConditionTargetType.Unknown ;
				}

				if( oSign != SignType.E && oSign != SignType.NE && oCount_1 >  -1 )
				{
					// == != 以外は範囲指定はできない
					return ConditionTargetType.Unknown ;
				}

				//----------------------------------

				return tTarget ;
			}

			// 能力判定値を取得する
			private int GetCount( ref string rString, out int oCount, out bool oAll )
			{
				oCount = -1 ;
				oAll = false ;

				int i, l ;
				char c ;

				string tCountCode ;
				
				//----------------------------------------------------------

				// 最高か最低か(条件の場合はエラーになる)

				if( rString.Length > 0 )
				{
					if( IsWord( ref rString, "all", "全て" ) == true )
					{
						oCount = -1;
						oAll = true;
						return 0;
					}
				}

				//----------------------------------------------------------

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tCountCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tCountCode, out int tCount ) ;

				//----------------------------------

				if( rString.Length == 0 || rString[ 0 ] != '^' )
				{
					// ここで終了
					return tCount ;
				}

				//----------------------------------

				// 範囲指定

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tCountCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tCountCode, out oCount ) ;

				if( oCount <  tCount )
				{
					// 入れ替え
					int tSwap = tCount ;
					tCount = oCount ;
					oCount = tSwap ;
				}
				//----------------------------------

				return tCount ;
			}

			//------------------------------------------------------------------------------------------

			// 条件を判定する
			public bool Check( __w.BattleUnit tActiveUnit, int tActionCategory, long tSkillId, __w.BattleUnit[][][] tUnit )
			{
//				__w.BattleUnit[][][] tUnit = WorkData.battleUnit ;

				int l, t, c ;

				int		tActionArea ;
				int		tActionRange ;
				int		tActionWidth ;
				bool	tActionAlive ;

				if( tActionCategory == 1 )
				{
					// 攻撃
					tActionArea		= 1 ;					// 陣営
					tActionRange	= tActiveUnit.Range ;	// 距離
					tActionWidth	= 1 ;					// 範囲
					tActionAlive	= true ;				// 生死
				}
				else
				if( tActionCategory == 2 )
				{
					// 技術
					__m.SkillData tSkill = __m.SkillData.GetById( tSkillId ) ;

					tActionArea		= tSkill.Area ;			// 陣営
					tActionRange	= tSkill.Range ;		// 距離
					tActionWidth	= tSkill.Width ;		// 範囲
					tActionAlive	= tSkill.Alive ;		// 生死
				}
				else
				{
					// 防御
					tActionArea		= 0 ;
					tActionRange	= 0 ;
					tActionWidth	= 0 ;
					tActionAlive	= true ;
				}

				//---------------------------------------------------------

				// 射程や生死の状況で行動が取れない可能性があるのを確認する
				int tArea, tLine, tSide ;
				int tDistance ;

				List<__w.BattleUnit> tList = new List<__w.BattleUnit>() ;

				tArea = tActiveUnit.Area ^ tActionArea ;

				for( tLine  = 0 ; tLine <= 1 ; tLine ++ )
				{
					if( tActionArea == 0 )
					{
						// 味方陣営
						tDistance = tLine - tActiveUnit.Line ;
						if( tDistance <  0 )
						{
							tDistance = - tDistance ;
						}
					}
					else
					{
						// 敵陣営
						tDistance = tLine + tActiveUnit.Line + tActionArea ;
					}

					if( tDistance <= tActionRange && tUnit[ tArea ][ tLine ] != null && tUnit[ tArea ][ tLine ].Length >  0 )
					{
						l = tUnit[ tArea ][ tLine ].Length ;
						for( tSide  = 0 ; tSide <  l ; tSide ++ )
						{
							if( ( tActionAlive == true && tUnit[ tArea ][ tLine][ tSide ].HpNow >  0 ) || ( tActionAlive == false && tUnit[ tArea ][ tLine ][ tSide ].HpNow == 0 ) )
							{
								tList.Add( tUnit[ tArea ][ tLine ][ tSide ] ) ;
							}
						}
					}
				}

				if( tList.Count == 0 )
				{
					// 対象が存在しないのでこの行動は選択できない
					return false ;
				}

				//----------------------------------------------------------

//				Debug.LogWarning( "判定対象:" + valueCategory ) ;

				// 対象を判定する
				if( valueCategory == ConditionValueCategory.Turn )
				{
					// ターンをチェックする


					int tNumber		= value[ 0 ] ;
					int tMultiple	= value[ 1 ] ;

					if( valueSign == SignType.E )
					{
						// 同じか
						if( tMultiple == 0 )
						{
							if( tActiveUnit.turn == tNumber )
							{
								return true ;	// 条件を満たす
							}
						}
						else
						{
							if( ( tActiveUnit.turn % tNumber ) == 0 )
							{
								return true ;	// 条件を満たす
							}
						}
					}
					else
					if( valueSign == SignType.NE )
					{
						// 違うか
						if( tMultiple == 0 )
						{
							if( tActiveUnit.turn != tNumber )
							{
								return true ;	// 条件を満たす
							}
						}
						else
						{
							if( ( tActiveUnit.turn % tNumber ) != 0 )
							{
								return true ;	// 条件を満たす
							}
						}
					}
				}
				else
				if
				(
					valueCategory == ConditionValueCategory.Hp ||
					valueCategory == ConditionValueCategory.Sp ||
					valueCategory == ConditionValueCategory.Mp ||
					valueCategory == ConditionValueCategory.State
				)
				{
					// カテゴリ
					int  tCategoryValue ;

					//---------------------------------

					// 必要な情報は全て揃ったので実際の判定を行う

//					Debug.LogWarning( "ターゲット:" + targetType ) ;

					if( targetType == ConditionTargetType.Myself )
					{
						// 自身

						if( valueCategory == ConditionValueCategory.Hp || valueCategory == ConditionValueCategory.Sp || valueCategory == ConditionValueCategory.Mp )
						{
							tCategoryValue = 0 ;
							switch( valueCategory )
							{
								case ConditionValueCategory.Hp : tCategoryValue = ( int )( tActiveUnit.HpRatio * 100 ) ; break ;
								case ConditionValueCategory.Sp : tCategoryValue = ( int )( tActiveUnit.SpRatio * 100 ) ; break ;
								case ConditionValueCategory.Mp : tCategoryValue = ( int )( tActiveUnit.MpRatio * 100 ) ; break ;
							}

							if( CheckValue( tCategoryValue, valueSign, value[ 0 ], value[ 1 ] ) == true )
							{
								// 条件を満たす
								return true ;
							}
						}
						else
						if( valueCategory == ConditionValueCategory.State )
						{
							if( CheckState( tActiveUnit.condition, valueSign, state ) == true )
							{
								// 条件を満たす
								return true ;
							}
						}
					}
					else
					if
					(
						targetType == ConditionTargetType.Player		||
						targetType == ConditionTargetType.Player_Front	||
						targetType == ConditionTargetType.Player_Back	||
						targetType == ConditionTargetType.Enemy			||
						targetType == ConditionTargetType.Enemy_Front	||
						targetType == ConditionTargetType.Enemy_Back
					)
					{
						// 固定範囲
						tArea = 0 ;
						int tLine_0 = 0, tLine_1 = 0 ;
						
						switch( targetType )
						{
							case ConditionTargetType.Player :
								tArea = tActiveUnit.Area ;
								tLine_0 = 0 ;
								tLine_1 = 1 ;
							break ;
							case ConditionTargetType.Player_Front :
								tArea = tActiveUnit.Area ;
								tLine_0 = 0 ;
								tLine_1 = 0 ;
							break ;
							case ConditionTargetType.Player_Back :
								tArea = tActiveUnit.Area ;
								tLine_0 = 1 ;
								tLine_1 = 1 ;
							break ;
							case ConditionTargetType.Enemy :
								tArea = tActiveUnit.Area ^ 1 ;
								tLine_0 = 0 ;
								tLine_1 = 1 ;
							break ;
							case ConditionTargetType.Enemy_Front :
								tArea = tActiveUnit.Area ^ 1 ;
								tLine_0 = 0 ;
								tLine_1 = 0 ;
							break ;
							case ConditionTargetType.Enemy_Back :
								tArea = tActiveUnit.Area ^ 1 ;
								tLine_0 = 1 ;
								tLine_1 = 1 ;
							break ;
						}

						bool tAlive = !( value[ 0 ] == 0 && ( valueSign == SignType.E || valueSign == SignType.NE ) ) ;

						t = 0 ;
						c = 0 ;
						for( tLine  = tLine_0 ; tLine <= tLine_1 ; tLine ++ )
						{
							if( tUnit[ tArea ][ tLine ] != null && tUnit[ tArea ][ tLine ].Length >  0 )
							{
								l = tUnit[ tArea ][ tLine ].Length ;
								for( tSide  = 0 ; tSide <  l ; tSide ++ )
								{
									if( ( tAlive == true && tUnit[ tArea ][ tLine ][ tSide ].HpNow >  0 ) || tAlive == false )
									{
										t ++ ;

										if( valueCategory == ConditionValueCategory.Hp || valueCategory == ConditionValueCategory.Sp || valueCategory == ConditionValueCategory.Mp )
										{
											tCategoryValue = 0 ;
											switch( valueCategory )
											{
												case ConditionValueCategory.Hp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].HpRatio * 100 ) ; break ;
												case ConditionValueCategory.Sp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].SpRatio * 100 ) ; break ;
												case ConditionValueCategory.Mp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].MpRatio * 100 ) ; break ;
											}

											if( CheckValue( tCategoryValue, valueSign, value[ 0 ], value[ 1 ] ) == true )
											{
												// 条件を満たす
												c ++ ;
											}
										}
										else
										if( valueCategory == ConditionValueCategory.State )
										{
											if( CheckState( tUnit[ tArea ][ tLine ][ tSide ].condition, valueSign, state ) == true )
											{
												// 条件を満たす
												c ++ ;
											}
										}
									}
								}
							}
						}

						// 数を確認する
						int tc0, tc1 ;

						if( targetAll == false )
						{
							// 人数指定
							tc0 = targetCount[ 0 ] ;
							tc1 = targetCount[ 1 ] ;
						}
						else
						{
							// 全員指定
							tc0 =  t ;
							tc1 = -1 ;
						}

						if( CheckValue( c, targetSign, tc0, tc1 ) == true )
						{
							// 人数条件も満たしている
							return true ;
						}
					}
					else
					if( targetType == ConditionTargetType.Target )
					{
						// 対象範囲

						if( tActionWidth == 0 )
						{
							// 自身対象
							if( valueCategory == ConditionValueCategory.Hp || valueCategory == ConditionValueCategory.Sp || valueCategory == ConditionValueCategory.Mp )
							{
								tCategoryValue = 0 ;
								switch( valueCategory )
								{
									case ConditionValueCategory.Hp : tCategoryValue = ( int )( tActiveUnit.HpRatio * 100 ) ; break ;
									case ConditionValueCategory.Sp : tCategoryValue = ( int )( tActiveUnit.SpRatio * 100 ) ; break ;
									case ConditionValueCategory.Mp : tCategoryValue = ( int )( tActiveUnit.MpRatio * 100 ) ; break ;
								}

								if( CheckValue( tCategoryValue, valueSign, value[ 0 ], value[ 1 ] ) == true )
								{
									// 条件を満たす
									return true ;
								}
							}
							else
							if( valueCategory == ConditionValueCategory.State )
							{
								if( CheckState( tActiveUnit.condition, valueSign, state ) == true )
								{
									// 条件を満たす
									return true ;
								}
							}
						}

						//--------------------------------------------------------

						tArea = tActiveUnit.Area ^ tActionArea ;

						int ta = 0 ;
						int ca = 0 ;

						int[] tl = { 0, 0 } ;
						int[] cl = { 0, 0 } ;

						bool tAlive = !( value[ 0 ] == 0 && ( valueSign == SignType.E || valueSign == SignType.NE ) ) ;

						for( tLine  = 0 ; tLine <= 1 ; tLine ++ )
						{
							if( tActionArea == 0 )
							{
								// 味方陣営
								tDistance = tLine - tActiveUnit.Line ;
								if( tDistance <  0 )
								{
									tDistance = - tDistance ;
								}
							}
							else
							{
								// 敵陣営
								tDistance = tLine + tActiveUnit.Line + tActionArea ;
							}

							if( tDistance <= tActionRange && tUnit[ tArea ][ tLine ] != null && tUnit[ tArea ][ tLine ].Length >  0 )
							{
								l = tUnit[ tArea ][ tLine ].Length ;
								for( tSide  = 0 ; tSide <  l ; tSide ++ )
								{
									if( ( tAlive == true && tUnit[ tArea ][ tLine ][ tSide ].HpNow >  0 ) || tAlive == false )
									{
										ta ++ ;
										tl[ tLine ] ++ ;

										if( valueCategory == ConditionValueCategory.Hp || valueCategory == ConditionValueCategory.Sp || valueCategory == ConditionValueCategory.Mp )
										{
											tCategoryValue = 0 ;
											switch( valueCategory )
											{
												case ConditionValueCategory.Hp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].HpRatio * 100 ) ; break ;
												case ConditionValueCategory.Sp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].SpRatio * 100 ) ; break ;
												case ConditionValueCategory.Mp : tCategoryValue = ( int )( tUnit[ tArea ][ tLine ][ tSide ].MpRatio * 100 ) ; break ;
											}

//											Debug.LogWarning( "条件判定:" + tCategoryValue + " " + valueSign + " " + value[ 0 ] + " " + value[ 1 ] ) ;
											if( CheckValue( tCategoryValue, valueSign, value[ 0 ], value[ 1 ] ) == true )
											{
												// 条件を満たす
												ca ++ ;
												cl[ tLine ] ++ ;
											}
										}
										else
										if( valueCategory == ConditionValueCategory.State )
										{
											if( CheckState( tUnit[ tArea ][ tLine ][ tSide ].condition, valueSign, state ) == true )
											{
												// 条件を満たす
												ca ++ ;
												cl[ tLine ] ++ ;
											}
										}
									}
								}
							}
						}


						// 数を確認する
						int tc0, tc1 ;

						if( tActionWidth == 1 || tActionWidth == 3 )
						{
							// 単体か全体

							if( targetAll == false )
							{
								// 人数指定
								tc0 = targetCount[ 0 ] ;
								tc1 = targetCount[ 1 ] ;
							}
							else
							{
								// 全員指定
								tc0 = ta ;
								tc1 = -1 ;
							}

//							Debug.LogWarning( "ここきてる？:" + ca + " " +targetSign + " " + tc0 + " " + tc1 ) ;
							if( CheckValue( ca, targetSign, tc0, tc1 ) == true )
							{
								// 人数条件も満たしている
								return true ;
							}
						}
						else
						{
							// 一列
							for( tLine  = 0 ; tLine <= 1 ; tLine ++ )
							{
								if( tl[ tLine ] >  0 )
								{
									// 対象が存在している列でなければ判定無効
									if( targetAll == false )
									{
										// 人数指定
										tc0 = targetCount[ 0 ] ;
										tc1 = targetCount[ 1 ] ;
									}
									else
									{
										// 全員指定
										tc0 = tl[ tLine ] ;
										tc1 = -1 ;
									}
	
									if( CheckValue( cl[ tLine ], targetSign, tc0, tc1 ) == true )
									{
										// 人数条件も満たしている
										break ;
									}
								}
							}

							if( tLine <  2 )
							{
								// 人数条件も満たしている列が存在する
								return true ;
							}
						}
					}
				}


				// 条件を満たさない
				return false ;
			}

			//------------------------------------------------------------------------------------------

		}


		//-------------------------------------------------------------------------------------------

		public enum TargetCategory
		{
			Unknown,
			Hp,
			Sp,
			Mp,
			MaxHp,
			MaxSp,
			MaxMp,
			P_Ap,
			P_Dp,
			M_Ap,
			M_Dp,
			State,
		}

		public class TargetData : FunctionBase
		{
			public	TargetCategory	category ;
			public	SignType		sign ;
			public	int[]			value = new int[ 2 ] ;
			public	int				limit ;
			public	UnitCondition	state ;


			public bool Set( string tTarget )
			{
				// 対象を判定する

				category = TargetCategory.Unknown ;

				if( IsWord( ref tTarget, "HP", "ＨＰ", "現在HP", "現在ＨＰ" ) == true )
				{
					category = TargetCategory.Hp ;
				}
				else
				if( IsWord( ref tTarget, "SP", "ＳＰ", "現在SP", "現在ＳＰ" ) == true )
				{
					category = TargetCategory.Sp ;
				}
				else
				if( IsWord( ref tTarget, "MP", "ＭＰ", "現在MP", "現在ＭＰ" ) == true )
				{
					category = TargetCategory.Mp ;
				}
				else
				if( IsWord( ref tTarget, "MAXHP", "ＭａｘＨＰ", "最大HP", "最大ＨＰ" ) == true )
				{
					category = TargetCategory.MaxHp ;
				}
				else
				if( IsWord( ref tTarget, "MAXSP", "ＭａｘＳＰ", "最大SP", "最大ＳＰ" ) == true )
				{
					category = TargetCategory.MaxSp ;
				}
				else
				if( IsWord( ref tTarget, "MAXMP", "ＭａｘＭＰ", "最大MP", "最大ＭＰ" ) == true )
				{
					category = TargetCategory.MaxMp ;
				}
				else
				if( IsWord( ref tTarget, "PAP", "ＰＡＰ", "物理攻撃力" ) == true )
				{
					category = TargetCategory.P_Ap ;
				}
				else
				if( IsWord( ref tTarget, "PDP", "ＰＤＰ", "物理防御力" ) == true )
				{
					category = TargetCategory.P_Dp ;
				}
				else
				if( IsWord( ref tTarget, "MAP", "ＭＡＰ", "魔法攻撃力" ) == true )
				{
					category = TargetCategory.M_Ap ;
				}
				else
				if( IsWord( ref tTarget, "MDP", "ＭＤＰ", "魔法防御力" ) == true )
				{
					category = TargetCategory.M_Dp ;
				}
				else
				if( IsWord( ref tTarget, "STATE", "ＳＴＡＴＥ", "状態" ) == true )
				{
					category = TargetCategory.State ;
				}

				if( category == TargetCategory.Unknown )
				{
					return false ;	// 不明化カテゴリ
				}
					
				//---------------------------------

				// サインタイプを取得
				sign = GetSign( ref tTarget ) ;

				if( sign == SignType.Unknown )
				{
					// 書式が不正なのでこの条件は無効
					return false ;
				}

				if
				(
					category == TargetCategory.Hp	||
					category == TargetCategory.Sp	||
					category == TargetCategory.Mp
				)
				{
					value[ 0 ] = GetValue( ref tTarget, out value[ 1 ], out limit ) ;

					if( value[ 0 ] <  0 )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}
				}
				else
				if
				(
					category == TargetCategory.MaxHp	||
					category == TargetCategory.MaxSp	||
					category == TargetCategory.MaxMp	||
					category == TargetCategory.P_Ap		||
					category == TargetCategory.P_Dp		||
					category == TargetCategory.M_Ap		||
					category == TargetCategory.M_Dp
				)
				{
					if( sign != SignType.E && sign != SignType.NE )
					{
						return false ;
					}

					value[ 0 ] = GetValue( ref tTarget, out value[ 1 ], out limit ) ;

					if( value[ 0 ] <  0 || limit == 0 )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}

					// 絞り込みは最大か最小しか有効にならない
				}
				else
				if( category == TargetCategory.State )
				{
					if( sign != SignType.E && sign != SignType.NE )
					{
						return false ;
					}

					state = GetState( ref tTarget ) ;

					if( state == UnitCondition.Unknown )
					{
						// 書式が不正なのでこの条件は無効
						return false ;
					}
				}
				else
				{
					return false ;
				}

				// 展開成功
				return true ;
			}

			// 能力判定値を取得する
			private int GetValue( ref string rString, out int oValue, out int oLimit )
			{
				int i, l ;
				char c ;

				string tValueCode ;

				oValue = -1 ;
				oLimit = 0 ;

				//----------------------------------------------------------

				if( rString.Length >  0 )
				{
					if( rString[ 0 ] == '↑' )
					{
						oLimit =  1 ;
						return 0 ;
					}
					else
					if( rString[ 0 ] == '↓' )
					{
						oLimit = -1 ;
						return 0 ;
					}
				}

				//----------------------------------------------------------

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tValueCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tValueCode, out int tValue ) ;

				if( rString.Length == 0 || rString[ 0 ] != '%' )
				{
					// 不正
					return -1 ;
				}

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				if( rString.Length == 0 || rString[ 0 ] != '^' )
				{
					// ここで終了
					return tValue ;
				}

				//----------------------------------

				// 範囲指定

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				l = rString.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = rString[ i ] ;

					if( c <  '0' || c >  '9' )
					{
						break ;
					}
				}

				if( i == 0 )
				{
					// 不正
					return -1 ;
				}

				tValueCode = rString.Substring( 0, i ) ;
				rString = rString.Substring( i, rString.Length - i ) ;

				int.TryParse( tValueCode, out oValue ) ;

				if( rString.Length == 0 || rString[ 0 ] != '%' )
				{
					// 不正
					return -1 ;
				}

				rString = rString.Substring( 1, rString.Length - 1 ) ;

				if( oValue <  tValue )
				{
					// 入れ替え
					int tSwap = tValue ;
					tValue = oValue ;
					oValue = tSwap ;
				}

				//----------------------------------

				return tValue ;
			}

			/// <summary>
			/// 設定に応じて対象の絞り込みを行う
			/// </summary>
			/// <param name="rList"></param>
			/// <returns></returns>
			public List<__w.BattleUnit> Check( ref List<__w.BattleUnit> rList )
			{
				if( rList.Count <= 1 )
				{
					// もう処理の必要は無い
					return rList ;
				}
				
				//---------------------------------------------------------

				List<__w.BattleUnit> tList = new List<__w.BattleUnit>() ;	// 絞り込み後の対象

				int i, l ;
				int tCategoryValue ;
				int tCategoryValueLimit ;
				int tIndex ;

//				Debug.LogWarning( "-----C:" + category ) ;


				if
				(
					category == TargetCategory.Hp	||
					category == TargetCategory.Sp	||
					category == TargetCategory.Mp
				)
				{
					// 範囲か限界か( == != > >= < <= )

					if( limit == 0 )
					{
						// 範囲
						l = rList.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tCategoryValue = 0 ;
							switch( category )
							{
								case TargetCategory.Hp : tCategoryValue = ( int )( rList[ i ].HpRatio * 100 ) ; break ;
								case TargetCategory.Sp : tCategoryValue = ( int )( rList[ i ].SpRatio * 100 ) ; break ;
								case TargetCategory.Mp : tCategoryValue = ( int )( rList[ i ].MpRatio * 100 ) ; break ;
							}

							if( CheckValue( tCategoryValue, sign, value[ 0 ], value[ 1 ] ) == true )
							{
								tList.Add( rList[ i ] ) ;
							}
						}
					}
					else
					{
						// 限界
						if( limit >  0 )
						{
							// 最高
							tCategoryValueLimit = 0 ;
						}
						else
						{
							// 最低
							tCategoryValueLimit = 99999999 ;
						}

						tIndex = -1 ;
						
						l = rList.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tCategoryValue = 0 ;
							switch( category )
							{
								case TargetCategory.Hp : tCategoryValue = rList[ i ].HpNow ; break ;
								case TargetCategory.Sp : tCategoryValue = rList[ i ].SpNow ; break ;
								case TargetCategory.Mp : tCategoryValue = rList[ i ].MpNow ; break ;
							}

							if( ( limit >  0 && tCategoryValue >  tCategoryValueLimit ) || ( limit <  0 && tCategoryValue <  tCategoryValueLimit ) )
							{
								// 更新
								tCategoryValueLimit = tCategoryValue ;
								tIndex = i ;
							}
						}

						// １つが決まったはず
						if( sign == SignType.E )
						{
							// １つだけ
							tList.Add( rList[ tIndex ] ) ;
						}
						else
						{
							// その他
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( i != tIndex )
								{
									tList.Add( rList[ i ] ) ;
								}
							}
						}
					}
				}
				else
				if
				(
					category == TargetCategory.MaxHp	||
					category == TargetCategory.MaxSp	||
					category == TargetCategory.MaxMp	||
					category == TargetCategory.P_Ap		||
					category == TargetCategory.P_Dp		||
					category == TargetCategory.M_Ap		||
					category == TargetCategory.M_Dp
				)
				{
					// 限界か( == != )

					if( limit >  0 )
					{
						// 最高
						tCategoryValueLimit = 0 ;
					}
					else
					{
						// 最低
						tCategoryValueLimit = 99999999 ;
					}

					tIndex = -1 ;
						
					l = rList.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tCategoryValue = 0 ;
						switch( category )
						{
							case TargetCategory.MaxHp	: tCategoryValue = rList[ i ].HpMax			; break ;
							case TargetCategory.MaxSp	: tCategoryValue = rList[ i ].SpMax			; break ;
							case TargetCategory.MaxMp	: tCategoryValue = rList[ i ].MpMax			; break ;
							case TargetCategory.P_Ap	: tCategoryValue = rList[ i ].PAttack		; break ;
							case TargetCategory.P_Dp	: tCategoryValue = rList[ i ].PDefense		; break ;
							case TargetCategory.M_Ap	: tCategoryValue = rList[ i ].MAttack		; break ;
							case TargetCategory.M_Dp	: tCategoryValue = rList[ i ].MDefense		; break ;
						}

						if( ( limit >  0 && tCategoryValue >  tCategoryValueLimit ) || ( limit <  0 && tCategoryValue <  tCategoryValueLimit ) )
						{
							// 更新
							tCategoryValueLimit = tCategoryValue ;
							tIndex = i ;
						}
					}

					// １つが決まったはず
					if( sign == SignType.E )
					{
						// １つだけ
						tList.Add( rList[ tIndex ] ) ;
					}
					else
					{
						// その他
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( i != tIndex )
							{
								tList.Add( rList[ i ] ) ;
							}
						}
					}
				}
				else
				if( category == TargetCategory.State )
				{
					// 即値か( == != )

					uint tMask = 0 ;
					if( state == UnitCondition.Enhancement )
					{

					}
					else
					if( state == UnitCondition.Weakening )
					{
						UnitCondition[] tCL =
						{
							UnitCondition.Poison,
							UnitCondition.Paralysis,
							UnitCondition.Sleep,
							UnitCondition.Confusion,
							UnitCondition.Charm,
							UnitCondition.Silence,
							UnitCondition.Bind,
							UnitCondition.Stone,
						} ;

						l = tCL.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tMask = ( tMask | ( uint )( 1 << ( int )tCL[ i ] ) ) ;
						}
					}
					else
					{
						tMask = ( uint )( 1 << ( int )state ) ;
					}

					l = rList.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( rList[ i ].HpNow >  0 && ( ( state == UnitCondition.Normal && rList[ i ].condition == 0 ) || ( rList[ i ].condition & tMask ) != 0 ) )
						{
							tList.Add( rList[ i ] ) ;
						}
					}
				}				

				//---------------------------------------------------------

				if( tList.Count == 0 )
				{
					// この対象指定は候補が無くなってしまうので無効
					return rList ;
				}

				// 新しい対象候補を返す
				return tList ;
			}
		}
		
		//-------------------------------------------------------------------------------------------


		public int		priority ;
		public int		weight ;
		public int		action_category ;
		public long		skill_id ;
		public string[]	condition ;
		public string[]	target ;
		public int		invalid_priority ;
		public bool		ignore_cost ;

		//---------------------------------------------------------------------------
		
		// 展開された条件データ
		public ConditionData[]	conditionData ;

		// 展開された対象データ
		public TargetData[]		targetData ;

		//---------------------------------------------------------------------------

		public static List<ActionPatternData> GetById( long id )
		{
			ActionPatternContainerData c = MassData.ActionPatternContainerTable.FirstOrDefault( _ => _.id == id ) ;
			return c?.ActionPatternTable ;
		}

		//-----------------------------------------------------------

		public void Convert( string tName )
		{
			if( condition == null || condition.Length == 0 || target == null || target.Length == 0 )
			{
				return ;	// 基本はありえない
			}

			int i, l, p ;
				
			//----------------------------------

			l = condition.Length ;

			conditionData = new ConditionData[ l ] ;

			// 展開した条件データは前詰めにする
			p = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( condition[ i ] ) == false )
				{
					conditionData[ p ] = new ConditionData() ;
	
					if( conditionData[ p ].Set( condition[ i ] ) == true )
					{
						p ++ ;
					}
					else
					{
						conditionData[ p ] = null ;
	
						Debug.LogError( "ActionPattern Condition( " + i +" ) Syntax Error : " + condition[ i ] + " / " + tName ) ;
					}
				}
			}

			//----------------------------------------------------------

			l = target.Length ;

			targetData = new TargetData[ l ] ;

			// 展開した条件データは前詰めにする
			p = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( target[ i ] ) == false )
				{
					targetData[ p ] = new TargetData() ;
	
					if( targetData[ p ].Set( target[ i ] ) == true )
					{
						p ++ ;
					}
					else
					{
						targetData[ p ] = null ;
	
						Debug.LogError( "ActionPattern Target( " + i +" ) Syntax Error : " + target[ i ] + " / " + tName ) ;
					}
				}
			}
		}

		//------------------------------------------------------------------------------------------
	}

	public class ActionPatternContainerData
	{
		public long						id ;
		public List<ActionPatternData>	ActionPatternTable ;
	}

}
