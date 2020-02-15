using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;


using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	/// <summary>
	/// 全プレイヤーデータ群で共通の情報
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class SystemData
	{
		//---------------------------------------------------------------------------
		// 保存対象

		/// <summary>
		/// 最後にセーブした位置
		/// </summary>
		public int							LastSelectedIndex ;

		/// <summary>
		/// セーブ情報(セーブデータそのものではない)
		/// </summary>
		public MemoryDescriptorData[]		MemoryDescriptors ;

		//---------------------------------------------------------------------------

		public SystemData()
		{
			LastSelectedIndex = 0 ;

			MemoryDescriptors = new MemoryDescriptorData[ 6 ] ;

			// 注意：中身代入には foreach は使えないので for で回す必要がある
			for( int i  = 0 ; i <  MemoryDescriptors.Length ; i ++ )
			{
				MemoryDescriptors[ i ] = new MemoryDescriptorData() ;
			}

			// 注意：Vector2 など宣言したら初期化しなくても中身が入っているものは struct
		}

		//-----------------------------------------------------------

		private const string m_Path = "UserData/PlayerSystem.dat" ;
		
		/// <summary>
		/// ロードする
		/// </summary>
		/// <returns></returns>
		public static IEnumerator LoadAsync( SystemData[] o )
		{
			Settings settings = Resources.Load<Settings>( "Settings/Settings" ) ;

			if( settings.UserDataLocation == Settings.LoadFrom.Storage )
			{
				// ストレージからのロードを試みる
				yield return UniRx.StartCoroutine( LoadFromStorageAsync( o ) ) ;
			}

			if( o[ 0 ] == null )
			{
				// ストレージから読み出せていなければリソースから読み出す(デバッグ用データ)
				yield return UniRx.StartCoroutine( LoadFromResourceAsync( o ) ) ;
			}
		}

		// ストレージからロードする
		private static IEnumerator LoadFromStorageAsync( SystemData[] o )
		{
			// ファイルが存在しない
			if( StorageAccessor.Exists( m_Path ) != StorageAccessor.Target.File )
			{
				yield break ;	// 読み出せない
			}

			byte[] data = StorageAccessor.Load( m_Path, Define.cryptoKey, Define.cryptoVector ) ;
			if( data.IsNullOrEmpty() == true )
			{
				yield break ;	// 読み出せない
			}

			o[ 0 ] = DataUtility.Deserialize<SystemData>( data ) ;
		}

		// リソースからロードする
		public static IEnumerator LoadFromResourceAsync( SystemData[] o )
		{
			o[ 0 ] = new SystemData() ;
			
			yield break ;
		}

		/// <summary>
		/// 準備処理を行う
		/// </summary>
		public void Prepare()
		{
		}

		// 参考
		// http://ntgame.wpblog.jp/2018/07/16/post-1819/
		// http://neue.cc/2017/03/13_550.html
		// https://github.com/neuecc/MessagePack-CSharp

		//-----------------------------------------------------------

		/// <summary>
		/// システムデータをセーブする
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Save( int index = -1 )
		{
			// PlayrerDataDescription を更新する

			if( index >= 0 )
			{
				LastSelectedIndex = index ;
				MemoryDescriptors[ index ].Build() ;
			}

			byte[] data = DataUtility.Serialize<SystemData>( this ) ;
			StorageAccessor.Save( m_Path, data, true, Define.cryptoKey, Define.cryptoVector ) ;
			
			return true ;
		}


	}
}

