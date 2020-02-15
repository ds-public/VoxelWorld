using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

namespace DBS
{
	/// <summary>
	/// ユーザー系の情報を保持したり処理したりするクラス Version 2017/08/13 0
	/// </summary>
	public class UserDataManager : SingletonManagerBase<UserDataManager>
	{
		public UserData user = new UserData() ;	// ユーザーデータ
	
		//---------------------------------------------------------------------------

		/// <summary>
		/// 全てのユーザー系データをセーブする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Save_Private() ;
		}

		// 全てのユーザー系データをセーブする
		private bool Save_Private()
		{
			user.SaveToStorage() ;

			return true ;
		}

		/// <summary>
		/// 全てのユーザー系データをロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private() ;
		}

		// 全てのユーザー系データをロードする
		private bool Load_Private()
		{
			user.LoadFromStorage() ;

			return true ;
		}

		//---------------------------------------------------------------------------

		public static AsyncState LoadAsync()
		{
			AsyncState tState ;

			if( m_Instance == null )
			{
				return null ;
			}

			tState = new AsyncState() ;
			m_Instance.StartCoroutine( m_Instance.LoadAsync_Private( tState ) ) ;
			return tState ;
		}
			

		private IEnumerator LoadAsync_Private( AsyncState tState )
		{
			yield return StartCoroutine( UserData.LoadAsync() ) ;
			if( tState != null )
			{
				tState.IsDone = true ;
			}
		}	

		//---------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// ユーザー系データをストレージから読み出す
			user.LoadFromStorage() ;
		}
		
		//---------------------------------------------------------------------------
	}
}

