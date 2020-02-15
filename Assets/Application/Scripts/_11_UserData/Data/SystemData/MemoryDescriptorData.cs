using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using MessagePack ;


using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	/// <summary>
	/// セーブスロットに表示される情報
	/// </summary>
	[MessagePackObject(keyAsPropertyName:true)]
	public class MemoryDescriptorData
	{
		/// <summary>
		/// データが存在するか否か
		/// </summary>
		public bool		Exist ;

		/// <summary>
		/// 顔イメージの参照名
		/// </summary>
		public string	Face ;

		/// <summary>
		/// 先頭ユニット名
		/// </summary>
		public string	UnitName ;

		/// <summary>
		/// プレイ時間(単位:秒)
		/// </summary>
		public int		PlayTime ;

		/// <summary>
		/// クラスの識別子
		/// </summary>
		public int		ClassType ;

		/// <summary>
		/// クラスのレベル
		/// </summary>
		public int		ClassLevel ;

		//-----------------------------------------------------------

		public MemoryDescriptorData()
		{
			Exist = false ;
		}

		/// <summary>
		/// 現在の展開情報からデスクリプターの内容を更新する
		/// </summary>
		public void Build()
		{
			Exist = true ;

			//----------------------------------

			int line, side ;
			__u.UnitData playerUnit = null ;

			__w.NormalUnit[][] activeTeam = UserData.Memory.ActiveTeam ;

			for( line  = 0 ; line <  2 ; line ++ )
			{
				for( side  = 0 ; side <  activeTeam[ line ].Length ; side ++ )
				{
					if( activeTeam[ line ][ side ] != null )
					{
						playerUnit = activeTeam[ line ][ side ].GetPlayerUnit() ;
						break ;
					}
				}
				if( playerUnit != null )
				{
					// 基本的に前衛には絶対にユニットが存在するので line のループとこの判定は本来は必要無い
					break ;
				}
			}
			
			if( playerUnit == null )
			{
				// 異常
				return ;
			}

			//----------------------------------

			Face = playerUnit.ImagePath ;

			UnitName = playerUnit.Name ;

			PlayTime = __u.CommonData.PlayTime + ( int )( Time.realtimeSinceStartup - __u.CommonData.PlayTimeBase ) ;

			ClassType = ( int )playerUnit.ClassType ;

			ClassLevel = playerUnit.Level ;

			//----------------------------------

		}
	}
}

