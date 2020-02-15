using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using CSVHelper ;
using JsonHelper ;
using StorageHelper ;


using DBS.UserDataCategory ;

namespace DBS
{
	/// <summary>
	/// ゲーム全体から参照されるプレイヤー系データを保持するクラス
	/// </summary>
	public class UserData : PreferenceBase
	{
		//---------------------------------------------------------------------------
		// 以下は全体で１つしかない共通管理情報

		private SystemData				m_System ;
		public static SystemData		  System
		{
			get
			{
				return UserDataManager.Instance.user.m_System ;
			}
		}

		//-----------------------------------

		private MemoryData				m_Memory ;
		public static MemoryData		  Memory
		{
			get
			{
				return UserDataManager.Instance.user.m_Memory ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// デフォルト状態に戻す
		/// </summary>
		/// <returns></returns>
		public static bool SetDefault()
		{
			UserData p = UserDataManager.Instance.user ;
			if( p == null )
			{
				return false ;
			}

			return true ;
		}
		

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public UserData()
		{
			path = "user.bin" ;	// 保存ファイル名を設定
		}

		public static AsyncState LoadAsync()
		{
			AsyncState state = new AsyncState() ;
			UniRx.StartCoroutine( UserDataManager.Instance.user.LoadAsync_Private( state ) ) ;
			return state ;
		}
		

		private IEnumerator LoadAsync_Private( AsyncState state )
		{
			SystemData[] s = { null } ;
			yield return UniRx.StartCoroutine( SystemData.LoadAsync( s ) ) ;
			if( s[ 0 ] != null )
			{
				m_System = s[ 0 ] ;
				m_System.Prepare() ;
			}

			MemoryData[] m = { null } ;
			yield return UniRx.StartCoroutine( MemoryData.LoadAsync( m ) ) ;
			if( m[ 0 ] != null )
			{
				m_Memory = m[ 0 ] ;
				m_Memory.Prepare() ;
			}

			//----------------------------------------------------------

			state.IsDone = true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// プレイヤーデータをストレージに書き込む(ショートカットアクセス)
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save( int index )
		{
			if( UserData.System.Save( index ) == false )
			{
				return false ;
			}

			if( UserData.Memory.Save( index ) == false )
			{
				return false ;
			}

			return true ;
		}

		//-----------------------------------------------------------
		// 以下はユーティリティメソッド

		/// <summary>
		/// 単独系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static T LoadObjectFromJson<T>( string path ) where T : class
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
				Debug.LogError( "データ異常:" + path ) ;
				return null ;
			}

			JsonObject jo = new JsonObject( ta.text ) ;

//			Debug.LogWarning( "Json:\n" + jo.ToString( "\t" ) ) ;
			return JsonUtility.FromJson<T>( jo.ToString() ) ;
		}
		
		/// <summary>
		/// 配列系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<T> LoadArrayFromJson<T>( string path ) where T : class
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
				Debug.LogError( "データ異常:" + path ) ;
				return null ;
			}

			JsonArray ja = new JsonArray( ta.text ) ;
			int i, l = ja.Length ;

			List<T> list = new List<T>() ;

			T o ;
			for( i  = 0 ; i <  l ; i ++ )
			{
//				Debug.LogWarning( "Json:\n" + ( ja[ i ] as JsonObject ).ToString( "\t" ) ) ;
				o = JsonUtility.FromJson<T>( ( ja[ i ] as JsonObject ).ToString() ) ;
				list.Add( o ) ;
			}

			return list ;
		}



	}
}
