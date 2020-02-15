using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	public static class InventoryData
	{
		//---------------------------------------------------------------------------

		public static ItemData GetById( long id )
		{
			return UserData.Memory.InventoryItems.FirstOrDefault( _ => _.id == id ) ;
		}

		/// <summary>
		/// 指定のカテゴリのアイテムを取得する
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="region">Region.</param>
		/// <param name="sort">If set to <c>true</c> sort.</param>
		public static ItemData[] GetItems( __m.ItemRegion region, bool sort = true )
		{
			IEnumerable<ItemData> q = UserData.Memory.InventoryItems.Where( _ => _.Region == region ) ;

			if( sort == true )
			{
				q = q.OrderBy( _ => _.item_id ) ;
			}

			return q.ToArray() ;
		}

		/// <summary>
		/// 指定のカテゴリのアイテムの複製を取得する
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
		}

		/// <summary>
		/// 所持品の数
		/// </summary>
		public static int Count
		{
			get
			{
				return UserData.Memory.InventoryItems.Count ;
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

			if( UserData.Memory.InventoryItems.Count >  0 )
			{
				if( UserData.Memory.InventoryItems[ 0 ].id <= 0 )
				{
					Debug.LogError( "UserData - Player(PlayerItem) に使用してはいけない id 値が使用されている : " + UserData.Memory.InventoryItems[ 0 ].id ) ;
					return -1 ;
				}

				int i, l = UserData.Memory.InventoryItems.Count ;
				for( i  = 0, id = 1 ; i <  l ; i ++, id ++ )
				{
					if( UserData.Memory.InventoryItems[ i ].id != id )
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
			if( UserData.Memory.InventoryItems.Count <= 1 )
			{
				// ソートの必要無し
				return ;
			}

			if( reverse == false )
			{
				// 昇順ソート
				UserData.Memory.InventoryItems.Sort( ( a, b ) => ( int )( a.id - b.id ) ) ;
			}
			else
			{
				// 降順ソート
				UserData.Memory.InventoryItems.Sort( ( a, b ) => ( int )( b.id - a.id ) ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定した種類のアイテム(道具・装備・素材)を指定した個数追加する[倉庫からの引出・店舗からの購入]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool Add( long itemId, int count )
		{
			if( ( Count + count ) >  CommonData.InventoryMax )
			{
				Debug.LogError( "Inventoryが溢れるためこれ以上は追加できない" ) ;
				return false ;	// 溢れるので不可
			}

			//----------------------------------

			while( count >  0 )
			{
				ItemData item = new ItemData
				{
					id = GetUsableId(),
					item_id = itemId,
					count = 0
				} ;

				UserData.Memory.InventoryItems.Add( item ) ;

				count -- ;
			}

			//----------------------------------

			// 一応ソートしておく
			Sort() ;

			return true ;
		}

		/// <summary>
		/// 指定した種類のアイテム(道具・装備・素材)を追加する[倉庫からの引出・店舗からの購入]
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="oItem">O item.</param>
		public static long Add( ItemData oItem )
		{
			if( Count >= CommonData.InventoryMax )
			{
				Debug.LogError( "Inventoryが溢れるためこれ以上は追加できない:" + oItem.Name ) ;
				return 0 ;
			}

			//----------------------------------

			ItemData item = new ItemData
			{
				id = GetUsableId()
			} ;
			item.Write( oItem ) ;
			item.count = 0 ;

			UserData.Memory.InventoryItems.Add( item ) ;

			//----------------------------------

			// 一応ソートしておく
			Sort() ;

			return item.id ;
		}

		/// <summary>
		/// 指定の種類のアイテム(道具・素材)を指定した個数削除する[倉庫への格納・店舗への売却]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static int Remove( long itemId, int count )
		{
			// 基本的にスタック出来ないものでは呼ばれない想定
			__m.ItemData mItem = __m.ItemData.GetById( itemId ) ;
			if( mItem.Region != __m.ItemRegion.Goods && mItem.Region != __m.ItemRegion.Material )
			{
				// スタック出来ないタイプのものにはこのメソッドは使用できない
				Debug.LogWarning( "道具・素材　以外はこのメソッドでは削除できない : " + mItem.Region ) ;
				return 0 ;
			}

			//----------------------------------

			ItemData[] items = UserData.Memory.InventoryItems.Where( _ => _.item_id == itemId ).Take( count ).ToArray() ;

			if( items.Length <  count )
			{
				Debug.LogWarning( "[警告] 削除対象の数が指定した個数に満たない : " + items.Length + " / " + count ) ;
			}

			count = items.Length ;
			if( count == 0 )
			{
				return count ;
			}

			// そのまま IEnumerable<PlayerItem> を渡してはダメ(例外が発生する)
			UserData.Memory.InventoryItems.RemoveRange( items ) ;
			
			return count ;
		}

		/// <summary>
		/// 指定した種類のアイテム(道具・装備・素材)を削除する
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool Remove( long id )
		{
			ItemData item = UserData.Memory.InventoryItems.FirstOrDefault( _ => _.id == id ) ;
			if( item == null )
			{
				Debug.LogError( "この識別子は存在しない : " + id ) ;
				return false ;
			}

			return UserData.Memory.InventoryItems.Remove( item ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したクラス・指定したスロットで装備可能な武具を列挙する
		/// </summary>
		/// <returns>The equipments.</returns>
		/// <param name="classType">Class type.</param>
		/// <param name="part">Part.</param>
		public static ItemData[] GetEquipments( __m.ClassType classType, __m.EquipmentPart part )
		{
			return UserData.Memory.InventoryItems.Where
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
		}

		/// <summary>
		/// 指定した種類のアイテムの所持数を取得する
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static int GetItemCount( long itemId )
		{
			return UserData.Memory.InventoryItems.Count( _ => _.item_id == itemId ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を退避する
		/// </summary>
		public static void SaveWork()
		{
			UserData.Memory.InventoryItems_Work = new List<ItemData>() ;

			UserData.Memory.InventoryItems.ForEach
			(
				_ =>
				{
					UserData.Memory.InventoryItems_Work.Add( _.Clone() ) ;
				}
			) ;
		}

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を復帰する
		/// </summary>
		public static void LoadWork()
		{
			UserData.Memory.InventoryItems = UserData.Memory.InventoryItems_Work ;

			UserData.Memory.InventoryItems_Work = null ;
		}

		/// <summary>
		/// 預り所用の作業領域を破棄する
		/// </summary>
		public static void FreeWork()
		{
			UserData.Memory.InventoryItems_Work = null ;
		}

		/// <summary>
		/// 完全にいっぱい状態か取得する
		/// </summary>
		public static bool IsFull
		{
			get
			{
				return  InventoryData.Count >= CommonData.InventoryMax ;
			}
		}

	}
}
