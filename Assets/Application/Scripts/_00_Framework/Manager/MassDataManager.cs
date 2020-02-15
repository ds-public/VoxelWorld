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
	public class MassDataManager : SingletonManagerBase<MassDataManager>
	{
		public MassData mass = new MassData() ;	// ユーザーデータ
	
		//---------------------------------------------------------------------------

		/// <summary>
		/// 全てのユーザー系データをロードする
		/// </summary>
		/// <returns></returns>
//		public static bool Load()
//		{
//			if( m_Instance == null )
//			{
//				return false ;
//			}
//
//			return m_Instance.Load_Private() ;
//		}

		// 全てのユーザー系データをロードする
//		private bool Load_Private()
//		{
//			mass.LoadFromFile() ;
//
//			return true ;
//		}

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
			yield return StartCoroutine( mass.LoadFromFileAsync( null ) ) ;
			if( tState != null )
			{
				tState.IsDone = true ;
			}
		}
			
		//---------------------------------------------------------------------------

//		new protected void Awake()
//		{
//			base.Awake() ;
//
//			// ユーザー系データをストレージから読み出す
//			mass.LoadFromFile() ;
//		}
	
		IEnumerator Start()
		{
//			yield return StartCoroutine( mass.LoadFromFileAsync( null ) ) ;
			yield return null ;	// ダイアログなどを使っているのでインスタンス生成直後にデータ読み出しを行ってはならない
		}
			
		//---------------------------------------------------------------------------
	}
}
