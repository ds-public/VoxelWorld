using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using MessagePack ;


using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	[MessagePackObject(keyAsPropertyName:true)]
	public class CommonData
	{
		//---------------------------------------------------------------------------
		// 保存対象

		/// <summary>
		/// 所持金
		/// </summary>
		public	int		money ;

		/// <summary>
		/// アイテムの最大所持可能数
		/// </summary>
		public	int		inventory_max ;

		/// <summary>
		/// アイテムの最大所持可能数
		/// </summary>
		public	int		storage_max ;

		/// <summary>
		/// アイテムの最大所持可能数
		/// </summary>
		public	int		shop_max ;

		/// <summary>
		/// パーティメンバーが１人もいない状況を許容するか
		/// </summary>
		public	bool	team_empty ;

		[IgnoreMember]
		public bool TeamEmpty
		{
			get
			{
				return team_empty ;
			}
		}

		/// <summary>
		/// ゲーム内現在時刻(分単位)
		/// </summary>
		public	int		world_time ;

		/// <summary>
		/// プレイ時間(単位:秒)
		/// </summary>
		public int		play_time ;

		//---------------

		// アクティブなチーム
		public TeamData ActiveTeam ;

		//---------------

		// マップ画面での選択中の階層のインデックス
		public int	mapFloorIndex ;

		// マップ画面での表示状態(0=階層・1=編集)
		public int	mapViewStatus ;

		//-----------------------------------

		/// <summary>
		/// プレイ時間計測用の基準時間(単位:ミリ秒)
		/// </summary>
		[NonSerialized][IgnoreMember]
		public float	play_time_base ;	// 計測用

		
		//---------------------------------------------------------------------------

		public CommonData()
		{
			money			= 1000 ;

			inventory_max	= 30 ;
			storage_max		= 20 ;

			world_time		= 8 * 60 ;

			play_time		= 0 ;

			ActiveTeam = new TeamData
			{
				front	= new long[]{ 1, 2, 3 },
				back	= new long[]{ 4, 5, 0 }
			} ;

			//----------------------------------

			play_time_base	= Time.realtimeSinceStartup ;
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 所持金
		/// </summary>
		public static int Money
		{
			get
			{
				return UserData.Memory.Common.money ;
			}
			set
			{
				UserData.Memory.Common.money = value ;
			}
		}


		/// <summary>
		/// アイテムの所持最大数
		/// </summary>
		public static int InventoryMax
		{
			get
			{
				return UserData.Memory.Common.inventory_max ;
			}
			set
			{
				UserData.Memory.Common.inventory_max = value ;
			}
		}

		/// <summary>
		/// アイテムの所持最大数
		/// </summary>
		public static int StorageMax
		{
			get
			{
				return UserData.Memory.Common.storage_max ;
			}
			set
			{
				UserData.Memory.Common.storage_max = value ;
			}
		}

		/// <summary>
		/// アイテムの所持最大数
		/// </summary>
		public static int ShopMax
		{
			get
			{
				return UserData.Memory.Common.shop_max ;
			}
			set
			{
				UserData.Memory.Common.shop_max = value ;
			}
		}



		/// <summary>
		/// 現在時刻(分単位)
		/// </summary>
		public static int WorldTime
		{
			get
			{
				return UserData.Memory.Common.world_time ;
			}
			set
			{
				UserData.Memory.Common.world_time = value ;
			}
		}

		/// <summary>
		/// 現在時刻(分単位)
		/// </summary>
		public static int WorldDate
		{
			get
			{
				return UserData.Memory.Common.world_time / 3600 ;
			}
		}

		/// <summary>
		/// 現在時刻(分単位)
		/// </summary>
		public static int WorldHour
		{
			get
			{
				return ( UserData.Memory.Common.world_time % 3600 ) / 60 ;
			}
		}

		// 後でこのあほらしい構造を止める

		/// <summary>
		/// プレイ時間
		/// </summary>
		public static int PlayTime
		{
			get
			{
				return UserData.Memory.Common.play_time ;
			}
			set
			{
				UserData.Memory.Common.play_time = value ;
			}
		}


		/// <summary>
		/// プレイ時間計測用の基準値
		/// </summary>
		public static float PlayTimeBase
		{
			get
			{
				return UserData.Memory.Common.play_time_base ;
			}
			set
			{
				UserData.Memory.Common.play_time_base = value ;
			}
		}
	}
}
