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
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class ItemData
	{
		public	long		id ;
		public	long		item_id ;

		public	int[]		data ;
		
		public	int			count ;	// スタック数(0でスタックしない)
		
		//---------------------------------------------------------------------------

		public __m.ItemData GetItem()
		{
			if( item_id == 0 )
			{
				return null ;
			}

			return __m.ItemData.GetById( item_id ) ;
		}
		
		/// <summary>
		/// このアイテムのカテゴリー(1=道具・2=装備・3=素材)
		/// </summary>
		public	__m.ItemRegion			Region
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? __m.ItemRegion.Unknown : item.Region ;
			}
		}

		/// <summary>
		/// カテゴリーごとの識別子
		/// </summary>
		public long			RegionId
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? -1 : item.region_id ;
			}
		}

		public __m.ItemCategory	Category
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? __m.ItemCategory.Unknown : item.Category ;
			}
		}


		/// <summary>
		/// 名前
		/// </summary>
		public string Name
		{
			get
			{
				__m.ItemData item = GetItem() ;
				return item == null ? "" : item.name ;
			}
		}

		/// <summary>
		/// 説明
		/// </summary>
		public string Description
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? "" : item.Description ;
			}
		}

		/// <summary>
		/// 買値
		/// </summary>
		public int Buy
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? 0 : item.buy ;
			}
		}

		/// <summary>
		/// 売値
		/// </summary>
		public int Sell
		{
			get
			{
				__m.ItemData item =	GetItem() ;
				return item == null ? 0 : item.sell ;
			}
		}


		/// <summary>
		/// このアイテムが消耗品である場合に消耗品情報を取得する
		/// </summary>
		/// <returns></returns>
		public __m.GoodsData GetGoods()
		{
			if( item_id == 0 )
			{
				// 設定値がおかしい
				return null ;
			}

			__m.ItemData item = GetItem() ;
			if( item == null || item.Region != __m.ItemRegion.Goods || item.region_id == 0 )
			{
				// 消耗品ではない
				return null ;
			}

			return __m.GoodsData.GetById( item.region_id ) ;
		}
		
		/// <summary>
		/// このアイテムが装備品である場合に装備品情報を取得する
		/// </summary>
		/// <returns></returns>
		public __m.EquipmentData GetEquipment()
		{
			if( item_id == 0 )
			{
				// 設定値がおかしい
				return null ;
			}

			__m.ItemData item = GetItem() ;
			if( item == null || item.Region != __m.ItemRegion.Equipment || item.region_id == 0 )
			{
				// 装備品ではない
				return null ;
			}

			return __m.EquipmentData.GetById( item.region_id ) ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// 装備品限定でアイテム上書
		/// </summary>
		public void Write( ItemData item )
		{
			item_id = item.item_id ;

			if( data != null && data.Length >  0  && item.data != null && item.data.Length >  0 )
			{
				int i, l0 = data.Length, l1 = item.data.Length ;
				for( i  = 0 ; ( i <  l0 && i <  l1 ) ; i ++ )
				{
					data[ i ] = item.data[ i ] ;
				}
			}
			
			count = item.count ;
		}

		/// <summary>
		/// 装備品限定でアイテム消去
		/// </summary>
		public void Clear()
		{
			item_id = 0 ;

			if( data != null && data.Length >  0  )
			{
				int i, l = data.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					data[ i ] = 0 ;
				}
			}
			
			count = 0 ;
		}

		/// <summary>
		/// 複製する
		/// </summary>
		/// <returns></returns>
		public ItemData Clone()
		{
			ItemData item = new ItemData
			{
				id		= this.id,
				item_id	= this.item_id,
				data	= this.data.Duplicate() as int[],
				count	= this.count
			} ;
			
			return item ;
		}
	}
}

