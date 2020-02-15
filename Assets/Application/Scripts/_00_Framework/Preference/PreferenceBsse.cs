using UnityEngine ;
using System ;
using System.Collections ;

using StorageHelper ;

namespace DBS
{
	/// <summary>
	/// プリファレンス系データ(ローカルストレージ保存)の基底クラス
	/// </summary>
	public class PreferenceBase
	{
		/// <summary>
		/// ストレージのパス
		/// </summary>
		[NonSerialized]
		public string path = "" ;


		/// <summary>
		/// 既にセーブされたファイルがストレージに存在するか
		/// </summary>
		/// <returns></returns>
		public bool IsSaved
		{
			get
			{
				string tPath = Define.folder + path ;

				if( string.IsNullOrEmpty( tPath ) == true )
				{
					// パスが設定されていない
					return false ;
				}

				if( StorageAccessor.Exists( tPath ) != StorageAccessor.Target.File )
				{
					return false ;
				}

				return true ;
			}
		}


		/// <summary>
		/// データをセーブする
		/// </summary>
		/// <returns></returns>
		public bool SaveToStorage()
		{
			string tPath = Define.folder + path ;

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				// パスが設定されていない
				Debug.Assert( false, "パスが設定されていません:" + GetType() ) ;
				return false ;
			}

			string tJson = JsonUtility.ToJson( this ) ;

//			Debug.LogWarning( "SAVE:" + tJson + " " + tJson.Length ) ;

			return StorageAccessor.SaveText( tPath, tJson, true, Define.cryptoKey, Define.cryptoVector ) ;
		}

		/// <summary>
		/// データをセーブする
		/// </summary>
		/// <returns></returns>
		public bool LoadFromStorage()
		{
			string tPath = Define.folder + path ;

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				// パスが設定されていない
				return false ;
			}

			string tJson = StorageAccessor.LoadText( tPath, Define.cryptoKey, Define.cryptoVector ) ;
			if( string.IsNullOrEmpty( tJson ) == true )
			{
				return false ;
			}

//			Debug.LogWarning( "LOAD:" + tJson + " : " + tJson.Length ) ;

			JsonUtility.FromJsonOverwrite( tJson, this ) ;

			return true ;
		}

		/// <summary>
		/// データを削除する
		/// </summary>
		/// <returns></returns>
		public bool RemoveInStorage()
		{
			string tPath = Define.folder + path ;

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				// パスが設定されていない
				return false ;
			}

			return StorageAccessor.Remove( tPath ) ;
		}
	}
}
