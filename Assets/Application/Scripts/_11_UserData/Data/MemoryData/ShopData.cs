using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	public static class ShopData
	{
		//---------------------------------------------------------------------------

		public static ItemData GetById( long id )
		{
			return UserData.Memory.ShopItems.FirstOrDefault( _ => _.id == id ) ;
		}

		public static ItemData GetByItemId( long itemId )
		{
			return UserData.Memory.ShopItems.FirstOrDefault( _ => _.item_id == itemId ) ;
		}

		/// <summary>
		/// 指定のカテゴリのアイテムを取得する
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="region">Region.</param>
		/// <param name="sort">If set to <c>true</c> sort.</param>
		public static ItemData[] GetItems( __m.ItemRegion region, bool sort = true )
		{
			IEnumerable<ItemData> q = UserData.Memory.ShopItems.Where( _ => _.Region == region ) ;

			if( sort == true )
			{
				q = q.OrderBy( _ => _.item_id ) ;
			}

			return q.ToArray() ;
		}

		/// <summary>
		/// 指定のカテゴリのアイテムを取得する
		/// </summary>
		/// <param name="category"></param>
		/// <param name="sort"></param>
		/// <returns></returns>
		public static ItemData[] GetItems( __m.ItemCategory category, bool sort = true )
		{
			IEnumerable<ItemData> q = UserData.Memory.ShopItems.Where( _ => _.Category == category ) ;

			if( sort == true )
			{
				q = q.OrderBy( _ => _.item_id ) ;
			}

			return q.ToArray() ;
		}
		
/*		/// <summary>
		/// 指定のカテゴリのアイテムを複製して取得する
		/// </summary>
		/// <returns>The duplicated items.</returns>
		/// <param name="region">Region.</param>
		/// <param name="sort">If set to <c>true</c> sort.</param>
		public static ItemData[] GetDuplicatedItems( __m.ItemRegion region, bool sort = true )
		{
			ItemData[] items = GetItems( region, sort ) ;

			if( items.IsNullOrEmpty() )
			{
				return null ;
			}

			int i, l = items.Length ;
			ItemData[] duplicatedItems = new ItemData[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				duplicatedItems[ i ] = items[ i ].Clone() ;
			}

			return duplicatedItems ;
		}*/

		/// <summary>
		/// 所持品の数
		/// </summary>
		public static int Count
		{
			get
			{
				return UserData.Memory.ShopItems.Count ;
			}
		}

		/// <summary>
		/// 使用可能な＝空いている識別子を取得する
		/// </summary>
		/// <returns></returns>
		public static long GetUsableId()
		{
			Sort( false ) ;

			long id = 1 ;

			if( UserData.Memory.ShopItems.Count >  0 )
			{
				if( UserData.Memory.ShopItems[ 0 ].id <= 0 )
				{
					Debug.LogError( "UserData - Player(PlayerItem) に使用してはいけない id 値が使用されている : " + UserData.Memory.InventoryItems[ 0 ].id ) ;
					return -1 ;
				}

				int i, l = UserData.Memory.ShopItems.Count ;
				for( i  = 0, id = 1 ; i <  l ; i ++, id ++ )
				{
					if( UserData.Memory.ShopItems[ i ].id != id )
					{
						// この id 値は使用可能
						return id ;
					}
				}
			}

			return id ;
		}

		/// <summary>
		/// アイテムを識別子でソートする
		/// </summary>
		public static void Sort( bool reverse = false )
		{
			if( UserData.Memory.ShopItems.Count <= 1 )
			{
				// ソートの必要無し
				return ;
			}

			if( reverse == false )
			{
				// 昇順ソート
				UserData.Memory.ShopItems.Sort( ( a, b ) => ( int )( a.id - b.id ) ) ;
			}
			else
			{
				// 降順ソート
				UserData.Memory.ShopItems.Sort( ( a, b ) => ( int )( b.id - a.id ) ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定した種類のアイテム(道具・装備・素材)を指定した個数追加する[店舗への売却]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static void Add( long itemId, int count )
		{
			ItemData item = UserData.Memory.ShopItems.FirstOrDefault( _ => _.item_id == itemId ) ;

			if( item == null )
			{
				// 初めて追加
				item = new ItemData
				{
					id = GetUsableId(),
					item_id = itemId,
					count = count
				} ;

				UserData.Memory.ShopItems.Add( item ) ;
			}
			else
			{
				// 既にあるものに追加
				item.count += count ;
				if( item.count >  999999999 )
				{
					item.count  = 999999999 ;
				}
			}

			//----------------------------------

			// 一応ソートしておく
			Sort() ;
		}

		/// <summary>
		/// 指定の種類のアイテム(道具・装備・素材)を指定した個数削除する[店舗での購入]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool Remove( long itemId, int count )
		{
			ItemData item = UserData.Memory.ShopItems.FirstOrDefault( _ => _.item_id == itemId ) ;

			if( item == null )
			{
				return false ;
			}

			if( item.count <  count )
			{
				return false ;
			}

			item.count -= count ;

			return true ;
		}

		//-----------------------------------------------------------

/*		/// <summary>
		/// 指定したクラス・指定したスロットで装備可能な武具を列挙する
		/// </summary>
		/// <param name="classType"></param>
		/// <param name="part"></param>
		/// <returns></returns>
		public static ItemData[] GetEquipments( __m.ClassType classType, __m.EquipmentPart part )
		{
			return UserData.Memory.ShopItems.Where
			(
				( ItemData item ) =>
				{
					if( item.Region == __m.ItemRegion.Equipment )
					{
						__m.EquipmentData equipment = __m.EquipmentData.GetById( item.RegionId ) ;
						if( equipment.IsUsableClass( classType ) == true && equipment.Part == part )
						{
							// 該当する
							return true ;
						}
					}
					// 該当しない
					return false ;
				}
			).ToArray() ;
		}*/

		/// <summary>
		/// 指定した種類のアイテムの所持数を取得する
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static int GetItemCount( long itemId )
		{
			// ショップではアイテムは無制限にスタックされるので基本的には１スロットの個数が返される
			ItemData item = UserData.Memory.ShopItems.FirstOrDefault( _ => _.item_id == itemId ) ;
			if( item == null )
			{
				return 0 ;
			}

			return item.count ;
		}

		/// <summary>
		/// 指定したアイテムを生成できる数を取得する
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static int GetCreationItemCount( long itemId )
		{
			__m.ItemData item = __m.ItemData.GetById( itemId ) ;
			__m.GoodsData goods = item.GetGoods() ;
			if( goods == null || goods.Materials == null || goods.Materials.Length == 0 )
			{
				return 0 ;
			}

			int count = 0 ;
			int i, l = goods.Materials.Length, c ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				c = GetItemCount( goods.Materials[ i ].ItemId ) / goods.Materials[ i ].ItemCount ;
				if( c == 0 )
				{
					return 0 ;	// １つも作れない
				}

				if( count == 0 )
				{
					count  = c ;
				}
				else
				if( c <  count )
				{
					count  = c ;
				}
			}

			return count ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// そのアイテムを扱っているか確認する
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static bool Exists( long itemId )
		{
			ItemData item =	UserData.Memory.ShopItems.FirstOrDefault( _ => _.item_id == itemId ) ;
			return ( item != null ) ;
		}
		
		/// <summary>
		/// 新規に購入できるようになるアイテムを取得する
		/// </summary>
		/// <returns></returns>
		public static __m.ItemData[] GetAvailableItems()
		{
			List<__m.ItemData> pool = MassData.ItemTable.Where( _ => ( _.Region == __m.ItemRegion.Goods && _.Buy >  0 ) ).ToList() ;
			if( pool == null || pool.Count == 0 )
			{
				return null ;
			}

			// 購入可能になるアイテムのうちまだショップで扱っていないもののみ取得する
			pool = pool.Where( _ => !Exists( _.id ) ).ToList() ; 
			if( pool == null || pool.Count == 0 )
			{
				return null ;
			}

			List<__m.ItemData> items = new List<__m.ItemData>() ;

			foreach( __m.ItemData item in pool )
			{
				bool available = true ;
				__m.GoodsData goods = item.GetGoods() ;
				foreach( __m.GoodsData.MaterialData material in goods.Materials )
				{
					ItemData userItem = GetByItemId( material.ItemId ) ;
					if( userItem == null || userItem.count <  material.ItemCount )
					{
						available = false ;
						break ;
					}
				}

				if( available == true )
				{
					// このアイテムは購入可能となった
					items.Add( item ) ;
				}
			}

			if( items.Count == 0 )
			{
				return null ;
			}

			// その内で素材が全て集まったもののみ取得する
			return items.ToArray() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を退避する
		/// </summary>
/*		public static void SaveWork()
		{
			UserData.Memory.ShopItems_Work = new List<ItemData>() ;

			UserData.Memory.ShopItems.ForEach
			(
				_ =>
				{
					UserData.Memory.ShopItems_Work.Add( _.Clone() ) ;
				}
			) ;
		}

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を復帰する
		/// </summary>
		public static void LoadWork()
		{
			UserData.Memory.ShopItems = UserData.Memory.ShopItems_Work ;

			UserData.Memory.ShopItems_Work = null ;
		}

		/// <summary>
		/// 預り所用の作業領域を破棄する
		/// </summary>
		public static void FreeWork()
		{
			UserData.Memory.ShopItems_Work = null ;
		}*/
	}
}

