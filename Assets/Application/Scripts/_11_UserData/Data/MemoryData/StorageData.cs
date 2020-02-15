using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	/// <summary>
	/// 倉庫の操作クラス
	/// </summary>
	public static class StorageData
	{
		[NonSerialized]
		public static int StackMax = 999 ;	// 道具と素材はこの数までスタック可能

		//---------------------------------------------------------------------------

		//---------------------------------------------------------------------------

		public static ItemData GetById( long id )
		{
			return UserData.Memory.StorageItems.FirstOrDefault( _ => _.id == id ) ;
		}

		/// <summary>
		/// 指定のカテゴリのアイテムを取得する
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="region">Region.</param>
		/// <param name="sort">If set to <c>true</c> sort.</param>
		public static ItemData[] GetItems( __m.ItemRegion region, bool sort = true )
		{
			IEnumerable<ItemData> q = UserData.Memory.StorageItems.Where( _ => _.Region == region ) ;

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

			// 配列にインスタンス格納系は foreach は使えない
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
				return UserData.Memory.StorageItems.Count ;
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

			if( UserData.Memory.StorageItems.Count >  0 )
			{
				if( UserData.Memory.StorageItems[ 0 ].id <= 0 )
				{
					Debug.LogError( "UserData - Player(PlayerItem) に使用してはいけない id 値が使用されている : " + UserData.Memory.StorageItems[ 0 ].id ) ;
					return -1 ;
				}

				int i, l = UserData.Memory.StorageItems.Count ;
				for( i  = 0, id = 1 ; i <  l ; i ++, id ++ )
				{
					if( UserData.Memory.StorageItems[ i ].id != id )
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
			if( UserData.Memory.StorageItems.Count <= 1 )
			{
				// ソートの必要無し
				return ;
			}

			if( reverse == false )
			{
				// 昇順ソート
				UserData.Memory.StorageItems.Sort( ( a, b ) => ( int )( a.id - b.id ) ) ;
			}
			else
			{
				// 降順ソート
				UserData.Memory.StorageItems.Sort( ( a, b ) => ( int )( b.id - a.id ) ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定した種類のアイテム(道具・素材)を指定した個数追加する[倉庫への格納]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool Add( long itemId, int count )
		{
			// 基本的にスタック出来ないものでは呼ばれない想定
			__m.ItemData mItem = __m.ItemData.GetById( itemId ) ;
			if( mItem.Region != __m.ItemRegion.Goods && mItem.Region != __m.ItemRegion.Material )
			{
				// スタック出来ないタイプのものにはこのメソッドは使用できない
				Debug.LogWarning( "道具・素材　以外はこのメソッドでは削除できない : " + mItem.Region ) ;
				return false ;
			}

			//----------------------------------

			ItemData item = GetByItemId( itemId ) ;
			if( item != null )
			{
				// 既に所持しているものがある

				int max = StorageData.StackMax - item.count ;

				if( count <= max )
				{
					item.count += count ;
					return true ;	// 終了
				}
				else
				{
					if( Count >= CommonData.StorageMax )
					{
						// 溢れるので格納出来ない
						return false ;
					}

					count -= max ;

					int s = Mathi.Ceiling( count, StorageData.StackMax ) ;

					if( s >  ( CommonData.StorageMax - Count ) )
					{
						// 溢れるので格納できない
						return false ;
					}

					item.count = StorageData.StackMax ;
				}
			}

			//----------------------------------
			// カウントが無くなるまで別スロットに追加していく

			while( count >  0 )
			{
				item = new ItemData
				{					
					id = GetUsableId(),	// 使用可能な識別子を取得する
					item_id = itemId,
					count = count > StorageData.StackMax ? StorageData.StackMax : count
				} ;

				UserData.Memory.StorageItems.Add( item ) ;

				count -= StorageData.StackMax ;
			}

			//----------------------------------

			Sort() ;

			return true ;	// 無事追加できた
		}

		/// <summary>
		/// 指定した種類のアイテム(装備)を追加する[倉庫への格納]
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="oItem">O item.</param>
		public static long Add( ItemData oItem )
		{
			if( oItem.Region != __m.ItemRegion.Equipment )
			{
				// 道具・素材
				Debug.LogWarning( "装備　以外はこのメソッドでは追加できない : " + oItem.Region ) ;
				return 0 ;
			}

			//----------------------------------

			// 装備
			if( Count >= CommonData.StorageMax )
			{
				Debug.LogError( "Storageが溢れるためこれ以上は追加できない:" + oItem.Name ) ;
				return 0 ;	// 溢れる
			}

			ItemData item = new ItemData
			{
				id = GetUsableId()	// 使用可能な識別子を取得する
			} ;
			item.Write( oItem ) ;
			item.count = 0 ;

			// 問題無いので追加する

			UserData.Memory.StorageItems.Add( item ) ;

			//----------------------------------

			// 一応ソートしておく
			Sort() ;

			return item.id ;
		}
		
		/// <summary>
		/// 指定した種類のアイテム(道具・素材)を指定した個数削除する[倉庫からの引出・店舗への売却]
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool Remove( long itemId, int count )
		{
			// 基本的にスタック出来ないものでは呼ばれない想定
			__m.ItemData mItem = __m.ItemData.GetById( itemId ) ;
			if( mItem.Region != __m.ItemRegion.Goods && mItem.Region != __m.ItemRegion.Material )
			{
				// スタック出来ないタイプのものにはこのメソッドは使用できない
				Debug.LogWarning( "道具・素材　以外はこのメソッドでは削除できない : " + mItem.Region ) ;
				return false ;
			}

			//----------------------------------

			if( GetItemCount( itemId ) <  count )
			{
				// 指定した個数分所持していないので不可
				return false ;
			}
			
			//----------------------------------

			ItemData item ;
			int min ;

			while( count >  0 )
			{
				item = GetByItemId( itemId ) ;
				if( item == null )
				{
					// ありえない
					return false ;
				}

				min = Math.Min( item.count, count ) ;
				item.count -= min ;
				count -= min ;

				if( item.count == 0 )
				{
					// スロットから削除する
					if( UserData.Memory.StorageItems.Remove( item ) == false )
					{
						Debug.LogError( "このアイテムは存在しない : " + item.Name ) ;
						return false ;
					}
				}
			}

			return true ;
		}

		/// <summary>
		/// 指定した種類のアイテム(装備)を削除する[倉庫からの引出・店舗への売却]
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool Remove( long id )
		{
			ItemData item = UserData.Memory.StorageItems.FirstOrDefault( _ => _.id == id ) ;
			if( item == null )
			{
				Debug.LogError( "この識別子は存在しない : " + id ) ;
				return false ;
			}

			if( item.Region != __m.ItemRegion.Equipment )
			{
				// スタック出来ないタイプのものにはこのメソッドは使用できない
				Debug.LogWarning( "装備　以外はこのメソッドでは削除できない : " + item.Region ) ;
				return false ;
			}

			return UserData.Memory.StorageItems.Remove( item ) ;
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
			return UserData.Memory.StorageItems.Where
			(
				( ItemData item ) =>
				{
					if( item.Region == __m.ItemRegion.Equipment )
					{
						__m.EquipmentData equipment = __m.EquipmentData.GetById( item.RegionId ) ;
						if( equipment.IsUsableClass( classType ) == true  && equipment.Part == part )
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
		/// 種類に合致するアイテムで元も数が少ないものを返す
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static ItemData GetByItemId( long itemId )
		{
			// First だと該当が無い場合は例外発生
			// FirstOrDefault だと該当が無い場合は null を返してくれる

			return UserData.Memory.StorageItems.Where( _ => _.item_id == itemId ).OrderBy( _ => _.count ).FirstOrDefault() ;
		}
		

		/// <summary>
		/// 指定した種類のアイテムの所持数を取得する
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static int GetItemCount( long itemId )
		{
			return UserData.Memory.StorageItems.Where( _ => _.item_id == itemId ).Sum( _ => _.count ) ;
		}


		//-----------------------------------------------------------

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を退避する
		/// </summary>
		public static void SaveWork()
		{
			UserData.Memory.StorageItems_Work = new List<ItemData>() ;

			UserData.Memory.StorageItems.ForEach
			(
				_ =>
				{
					UserData.Memory.StorageItems_Work.Add( _.Clone() ) ;
				}
			) ;
		}

		/// <summary>
		/// 預り所処理用に現在の預り所の情報を復帰する
		/// </summary>
		public static void LoadWork()
		{
			UserData.Memory.StorageItems = UserData.Memory.StorageItems_Work ;

			UserData.Memory.StorageItems_Work = null ;
		}

		/// <summary>
		/// 預り所用の作業領域を破棄する
		/// </summary>
		public static void FreeWork()
		{
			UserData.Memory.StorageItems_Work = null ;
		}

		/// <summary>
		/// 完全にいっぱい状態か取得する
		/// </summary>
		public static bool IsFull
		{
			get
			{
				// まだストレージに格納可能か確認する
				if( StorageData.Count >= CommonData.StorageMax )
				{
					// 空スロットが１つもない
					int i ;
					for( i  = 0 ; i <  UserData.Memory.StorageItems.Count ; i ++ )
					{
						ItemData item = UserData.Memory.StorageItems[ i ] ;
							
						if( item.Region == __m.ItemRegion.Goods || item.Region == __m.ItemRegion.Material )
						{
							if( item.count <  StorageData.StackMax )
							{
								break ;
							}
						}
					}

					if( i >= UserData.Memory.StorageItems.Count )
					{
						return true ;	// フル
					}
				}

				return false ;
			}
		}
		
	}

	//----------------------------------------------------------------------------

	// 

}
