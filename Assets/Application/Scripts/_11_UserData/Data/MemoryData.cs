using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;
using CSVHelper ;
using JsonHelper ;

using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	/// <summary>
	/// 全プレイヤーデータ群で共通の情報
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class MemoryData
	{
		//---------------------------------------------------------------------------
		// 保存対象

		public	CommonData						Common ;

//		[IgnoreMember]
		public	List<UnitData>					Units ;

//		[IgnoreMember]
		public	List<TeamData>					Teams ;

//		[IgnoreMember]
		public	List<ItemData>					InventoryItems ;

//		[IgnoreMember]
		public	List<ItemData>					StorageItems ;

//		[IgnoreMember]
		public	List<ItemData>					ShopItems ;

//		[IgnoreMember]
		public	List<MapData>					Maps ;

		//---------------------------------------------------------------------------
		// 非保存対象
		
		// アクティブユニットチーム
		[NonSerialized][IgnoreMember]
		public __w.NormalUnit[][]				ActiveTeam ;

		[NonSerialized][IgnoreMember]
		public List<ItemData>					InventoryItems_Work ;

		[NonSerialized][IgnoreMember]
		public List<ItemData>					StorageItems_Work ;

		[NonSerialized][IgnoreMember]
		public List<ItemData>					ShopItems_Work ;

		//---------------------------------------------------------------------------

		private const string m_Path = "UserData/PlayerMemory_%1.dat" ;
		
		/// <summary>
		/// ロードする
		/// </summary>
		/// <returns></returns>
		public static IEnumerator LoadAsync( MemoryData[] o )
		{
			Settings settings = Resources.Load<Settings>( "Settings/Settings" ) ;

			if( settings.UserDataLocation == Settings.LoadFrom.Storage )
			{
				// ストレージからのロードを試みる
				yield return UniRx.StartCoroutine( LoadFromStorageAsync( UserData.System.LastSelectedIndex, o ) ) ;
			}

			if( o[ 0 ] == null )
			{
				// ストレージから読み出せていなければリソースから読み出す(デバッグ用データ)
				yield return UniRx.StartCoroutine( LoadFromResourceAsync( o ) ) ;
			}
		}

		/// <summary>
		/// ストレージからのロードを試みる
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static IEnumerator LoadFromStorageAsync( int index, MemoryData[] o )
		{
			string path = m_Path.Replace( "%1", index.ToString( "D2" ) ) ;

			if( StorageAccessor.Exists( path ) != StorageAccessor.Target.File )
			{
				// ファイルが存在しない
				yield break ;
			}

			byte[] data = StorageAccessor.Load( path, Define.cryptoKey, Define.cryptoVector ) ;
			if( data.IsNullOrEmpty() == true )
			{
				yield break ;
			}

			o[ 0 ] = DataUtility.Deserialize<MemoryData>( data ) ;
		}

		// リソースから読み出す
		public static IEnumerator LoadFromResourceAsync( MemoryData[] o )
		{
			o[ 0 ] = new MemoryData() ;

			yield return UniRx.StartCoroutine( o[ 0 ].LoadFromResourceAsync_Private() ) ;
		}

		private IEnumerator LoadFromResourceAsync_Private()
		{
			//----------------------------------------------------------
			// 共通設定項目

			// プレイヤーコモンデータを読み込む(アクティブなチームデータもここにあり)
			Common	= UserData.LoadObjectFromJson<CommonData>( "Data/UserData//CommonData" ) ;
			
			//----------------------------------------------------------

			// プレイヤーユニットデータを読み込む(新)
			Units	= UserData.LoadArrayFromJson<UnitData>( "Data/UserData//UnitData" ) ;

			//----------------------------------------------------------

			// プレイヤーチームデータを読み込む(新)
//			Teams	= UserData.LoadArrayFromJson<TeamData>( "Data/UserData//TeamData" ) ;
			
			Teams	= new List<TeamData>() ;
			for( int i  = 0 ; i <  6 ; i ++ )
			{
				Teams.Add( new TeamData() ) ;
			}

			//----------------------------------------------------------

			// プレイヤーインベントリデータを読み込む(新)
			InventoryItems	= UserData.LoadArrayFromJson<ItemData>( "Data/UserData//InventoryData" ) ;
			
			// プレイヤーストレージデータを読み込む(新)
			StorageItems	= UserData.LoadArrayFromJson<ItemData>( "Data/UserData//StorageData" ) ;

			// プレイヤーショップデータを読み込む(新)
			ShopItems	= UserData.LoadArrayFromJson<ItemData>( "Data/UserData//ShopData" ) ;

			// マップデータを読み込む
			Maps = new List<MapData>() ;
			for( int i  =   0 ; i <  10 ; i ++ )
			{
				Maps.Add( new MapData() ) ;
			}

			//----------------------------------------------------------

			yield break ;
		}

		/// <summary>
		/// 準備処理を行う
		/// </summary>
		public void Prepare()
		{
			// いくつかのワーク情報を展開する
			foreach( var unit in UserData.Memory.Units )
			{
				// 装備品がある場合とない場合でのパラメータを計算・保存しておく
				unit.Prepare() ;
			}

			TeamData.OpenActiveTeam() ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// メモリーデータをセーブする
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Save( int index )
		{
			string path = m_Path.Replace( "%1", index.ToString( "D2" ) ) ;

			byte[] data = DataUtility.Serialize<MemoryData>( this ) ;

			return StorageAccessor.Save( path, data, true, Define.cryptoKey, Define.cryptoVector ) ;
		}

	}
}
