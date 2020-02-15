using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __w=DBS.WorkDataCategory ;
using MessagePack ;

using CSVHelper ;

namespace DBS.UserDataCategory
{
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class TeamData
	{
		//---------------------------------------------------------------------------
		// 保存対象

		public long		id ;
		public long[]	front ;
		public long[]	back ;
		

		//---------------------------------------------------------------------------

		public static TeamData GetById( long id )
		{
			return UserData.Memory.Teams.FirstOrDefault( _ => _.id == id ) ;
		}


		

		// 後でこのクラスはアクティブチームとチームセットの保持クラスにする。
		// データ構造も単純な行列ではなく、クラス内部で複数のＣＳＶを展開するようにする。
		
		//-----------------------------------------------------------


		//-----------------------------------------------------------

		/// <summary>
		/// Common 内にあるアクティブなチームデータを展開する
		/// </summary>
		public static void OpenActiveTeam()
		{
			UserData.Memory.ActiveTeam = new __w.NormalUnit[ 2 ][] ;

			//----------------------------------------------------------
			
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			int i, l ;

			long playerUnitId ;
			UnitData playerUnit ;

			//--------------
			// 前衛
			activeTeam[ 0 ] = new __w.NormalUnit[ 3 ] ;

			l = UserData.Memory.Common.ActiveTeam.front.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( UserData.Memory.Common.ActiveTeam.front[ i ] >  0 )
				{
					playerUnitId = UserData.Memory.Common.ActiveTeam.front[ i ] ;
					playerUnit = UnitData.GetById( playerUnitId ) ;

					activeTeam[ 0 ][ i ] = new __w.NormalUnit( playerUnit, 0, i ) ;
				}
			}

			//--------------
			// 後衛
			activeTeam[ 1 ] = new __w.NormalUnit[ 3 ] ;

			l = UserData.Memory.Common.ActiveTeam.back.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( UserData.Memory.Common.ActiveTeam.back[ i ] >  0 )
				{
					playerUnitId = UserData.Memory.Common.ActiveTeam.back[ i ] ;
					playerUnit = UnitData.GetById( playerUnitId ) ;

					activeTeam[ 1 ][ i ] = new __w.NormalUnit( playerUnit, 1, i ) ;
				}
			}
		}
		
		/// <summary>
		/// チーム編成で設定したユニットをアクティブチームに設定する
		/// </summary>
		/// <returns>The active team.</returns>
		/// <param name="teamUnits">Team units.</param>
		public static __w.NormalUnit[][] SetActiveTeam( UnitData[] teamUnits, bool full = false )
		{
			int i ;
			
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;
			
			// 前衛
			activeTeam[ 0 ] = new __w.NormalUnit[ 3 ] ;
			for( i  = 0 ; i <  3 ; i ++ )
			{
//				if( teamUnits[ 0 + i ] != null )
//				{
//					Debug.LogWarning( "あるよ:" + ( 0 + i ) + " : " + teamUnits[ 0 + i ].Name ) ;
//				}
				activeTeam[ 0 ][ i ] = ( teamUnits[ 0 + i ] != null ) ? new __w.NormalUnit( teamUnits[ 0 + i ], 0, i ) : null ;	
			}
			
			// 後衛
			activeTeam[ 1 ] = new __w.NormalUnit[ 3 ] ;
			for( i  = 0 ; i <  3 ; i ++ )
			{
//				if( teamUnits[ 3 + i ] != null )
//				{
//					Debug.LogWarning( "あるよ:" + ( 0 + i ) + " : " + teamUnits[ 3 + i ].Name ) ;
//				}
				activeTeam[ 1 ][ i ] = ( teamUnits[ 3 + i ] != null ) ? new __w.NormalUnit( teamUnits[ 3 + i ], 1, i ) : null ;	
			}
			
			// コモンデータの方も更新してやる必要がある
			UserData.Memory.Common.ActiveTeam.front = new long[ 3 ] ;
			for( i  = 0 ; i <  3 ; i ++ )
			{
				UserData.Memory.Common.ActiveTeam.front[ i ] = ( teamUnits[ 0 + i ] != null ) ? teamUnits[ 0 + i ].id : 0 ;
			}
			
			UserData.Memory.Common.ActiveTeam.back = new long[ 3 ] ;
			for( i  = 0 ; i <  3 ; i ++ )
			{
				UserData.Memory.Common.ActiveTeam.back[ i ]  = ( teamUnits[ 3 + i ] != null ) ? teamUnits[ 3 + i ].id : 0 ;
			}
				
			return GetActiveTeam( full ) ;
		}
		
		
		/// <summary>
		/// アクティブチームのユニットを取得する
		/// </summary>
		/// <param name="full"></param>
		/// <returns></returns>
		public static __w.NormalUnit[][] GetActiveTeam( bool full = false )
		{
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			if( full == true )
			{
				return activeTeam ;
			}

			//----------------------------------

			// 平常用のデータを展開する
			__w.NormalUnit[][] team = new __w.NormalUnit[ 2 ][] ;
			
			//--------------

			int line, side, i, l = 3 ;

			for( line  = 0 ; line <= 1 ; line ++ )
			{
				side = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( activeTeam[ line ][ i ] != null )
					{
						// ユニットが存在する
						side ++ ;
					}
				}

				if( side >  0 )
				{
					team[ line ] = new __w.NormalUnit[ side ] ;

					side = 0 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( activeTeam[ line ][ i ] != null )
						{
							team[ line ][ side ] = activeTeam[ line ][ i ] ;
							side ++ ;
						}
					}
				}
			}

			return team ;
		}

		/// <summary>
		/// Common 内にあるアクティブなチームデータを展開する
		/// </summary>
		public static UnitData[] GetActiveTeamUnits()
		{
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			UnitData[] teamUnits = new UnitData[ 6 ] ;

			int i, l ;

			// 前衛
			if( activeTeam[ 0 ] != null && activeTeam[ 0 ].Length >  0 )
			{
				l = activeTeam[ 0 ].Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( activeTeam[ 0 ][ i ] != null && activeTeam[ 0 ][ i ].UnitId >  0 )
					{
						teamUnits[ 0 + i ] = activeTeam[ 0 ][ i ].GetPlayerUnit() ;
					}
				}
			}

			// 後衛
			if( activeTeam[ 1 ] != null && activeTeam[ 1 ].Length >  0 )
			{
				l = activeTeam[ 1 ].Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( activeTeam[ 1 ][ i ] != null && activeTeam[ 1 ][ i ].UnitId >  0 )
					{
						teamUnits[ 3 + i ] = activeTeam[ 1 ][ i ].GetPlayerUnit() ;
					}
				}
			}

			return teamUnits ;
		}

		/// <summary>
		/// アクティブチーム内のユニットの数を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetActiveTeamUnitCount()
		{
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			int count = 0 ;

			int i, l ;

			// 前衛
			if( activeTeam[ 0 ] != null && activeTeam[ 0 ].Length >  0 )
			{
				l = activeTeam[ 0 ].Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( activeTeam[ 0 ][ i ] != null && activeTeam[ 0 ][ i ].UnitId >  0 )
					{
						count ++ ;
					}
				}
			}

			// 後衛
			if( activeTeam[ 1 ] != null && activeTeam[ 1 ].Length >  0 )
			{
				l = activeTeam[ 1 ].Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( activeTeam[ 1 ][ i ] != null && activeTeam[ 1 ][ i ].UnitId >  0 )
					{
						count ++ ;
					}
				}
			}

			return count ;
		}

		/// <summary>
		/// ２つのユニットの場所を入れ替える
		/// </summary>
		/// <param name="l0"></param>
		/// <param name="s0"></param>
		/// <param name="l1"></param>
		/// <param name="s1"></param>
		public static void SwapUnit( int l0, int s0, int l1, int s1 )
		{
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			// インスタンス入れ替え
			__w.NormalUnit swap = activeTeam[ l0 ][ s0 ] ;
			activeTeam[ l0 ][ s0 ] = activeTeam[ l1 ][ s1 ] ;
			activeTeam[ l1 ][ s1 ] = swap ;

			// 位置情報更新(Emptyのケースがあるのでnullチェック必須)
			if( activeTeam[ l0 ][ s0 ] != null )
			{
				activeTeam[ l0 ][ s0 ].Line = l0 ;
				activeTeam[ l0 ][ s0 ].Side = s0 ;
			}
			if( activeTeam[ l1 ][ s1 ] != null )
			{
				activeTeam[ l1 ][ s1 ].Line = l1 ;
				activeTeam[ l1 ][ s1 ].Side = s1 ;
			}

			//----------------------------------

			// Common に反映させる
			int side ;
			for( side  = 0 ; side <  3 ; side ++ )
			{
				UserData.Memory.Common.ActiveTeam.front[ side ] = 0 ;	// 一旦初期化
				UserData.Memory.Common.ActiveTeam.back[ side ] = 0 ;	// 一旦初期化
			}

			if( activeTeam[ 0 ].IsNullOrEmpty() == false )
			{
				for( side  = 0 ; side <  activeTeam[ 0 ].Length ; side ++ )
				{
					UserData.Memory.Common.ActiveTeam.front[ side ] = activeTeam[ 0 ][ side ] != null ? activeTeam[ 0 ][ side ].UnitId : 0 ;
				}
			}

			if( activeTeam[ 1 ].IsNullOrEmpty() == false )
			{
				for( side  = 0 ; side <  activeTeam[ 1 ].Length ; side ++ )
				{
					UserData.Memory.Common.ActiveTeam.back[ side ] = activeTeam[ 1 ][ side ] != null ? activeTeam[ 1 ][ side ].UnitId : 0 ;
				}
			}
		}

		/// <summary>
		/// 全回復させる
		/// </summary>
		public static void RestoreAll()
		{
			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			int line, side ;
			for( line  = 0 ; line <  2 ; line ++ )
			{
				if( activeTeam[ line ] != null )
				{
					for( side  = 0 ; side <  activeTeam[ line ].Length ; side ++ )
					{
						if( activeTeam[ line ][ side ] != null )
						{
							UnitData.GetById(	activeTeam[ line ][ side ].UnitId ).Restore() ;
						}
					}
				}
			}
		}

		//-----------------------------------------------------

		/// <summary>
		///	設定されているスロット数を返す
		/// </summary>
		public static int Count
		{
			get
			{
				return UserData.Memory.Teams.Count( _ => ( _.IsEmpty == false ) ) ;
			}
		}

		public bool IsEmpty
		{
			get
			{
				if( front == null && back == null )
				{
					return true ;
				}

				if( front != null )
				{
					if( front.Any( _ => _ != 0 ) == true )
					{
						return false ;
					}
				}

				if( back  != null )
				{
					if( back.Any( _ => _ != 0 ) == true )
					{
						return false ;
					}
				}

				return true ;
			}
		}
	}
}
