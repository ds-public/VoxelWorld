using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m = DBS.MassDataCategory ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// アイテム情報クラス
	/// </summary>
	public class ItemData
	{
		public	long	id ;			// 識別子

		public	string	name ;			// 名称
		public	string	Name
		{
			get
			{
				return name ;
			}
		}

		public	int			region ;		// 種別
		public	ItemRegion	Region
		{
			get
			{
				return ( ItemRegion )region ;
			}
		}

		public	long	region_id ;	// リレーション
		public	long	RegionId
		{
			get
			{
				return region_id ;
			}
		}

		public	int				category ;
		public	ItemCategory	Category
		{
			get
			{
				return ( ItemCategory )category ;
			}
		}


		public	int		icon ;			// アイコン



		public	string	description ;	// 説明文
		public	string	Description
		{
			get
			{
				return description ;
			}
		}

		public	int		buy ;			// 買値
		public	int		Buy
		{
			get
			{
				return buy ;
			}
		}
		
		public	int		sell ;			// 売値
		public	int		Sell
		{
			get
			{
				return sell ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// このアイテムを消耗品とみなして消耗品情報を取得する
		/// </summary>
		/// <returns></returns>
		public GoodsData GetGoods()
		{
		
		
			if( Region != ItemRegion.Goods )
			{
				Debug.LogWarning( "This item is not goods : " + id ) ;
				return null ;
			}

			return GoodsData.GetById( region_id ) ;
		}

		/// <summary>
		/// このアイテムを装備品とみなして装備品情報を取得する
		/// </summary>
		/// <returns></returns>
		public EquipmentData GetEquipment()
		{
			if( Region != ItemRegion.Equipment )
			{
				Debug.LogWarning( "This item is not equipment : " + id ) ;
				return null ;
			}

			return EquipmentData.GetById( region_id ) ;
		}

		//-----------------------------------------------------------

		public static ItemData GetById( long id )
		{
			return MassData.ItemTable.FirstOrDefault( _ => _.id == id ) ;
		}
		
		public InfluenceData GetInfluence()
		{
			if( Region == ItemRegion.Goods )
			{
				return GetGoods().GetInfluence() ;
			}
			
			return null ;
		} 
		
		public int Scene
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Scene ;
				}
				
				return 0 ;
			}
		}
		
		public int Area
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Area ;
				}
				
				return -1 ;
			}
		}
		
		public int Width
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Width ;
				}
				
				return -1 ;
			}
		}
		
		public int Range
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Range ;
				}
				
				return -1 ;
			}
		}

		public bool Alive
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Alive ;
				}
				
				return false ;
			}
		}

		public int Process
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Process ;
				}
				
				return -1 ;
			}
		}

		public int[] Data
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().Data ;
				}
				
				return null ;
			}
		}
		
		public long EffectId
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().EffectId ;
				}
				
				return 0 ;
			}
		}
		
		public int EffectTarget
		{
			get
			{
				if( Region == ItemRegion.Goods )
				{
					// goods
					return GetGoods().EffectTarget ;
				}
				
				return -1 ;
			}
		}
		
		
	}
}
