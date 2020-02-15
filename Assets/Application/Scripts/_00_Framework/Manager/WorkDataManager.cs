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
	public class WorkDataManager : SingletonManagerBase<WorkDataManager>
	{
		public WorkData work = new WorkData() ;	// ユーザーデータ

		// 任意のオブジェクトキャッシュ
		public class CacheData
		{
			public System.Object	Value ;
			public bool				Alive ;
		}
		private readonly Dictionary<string,CacheData>	m_CacheData = new Dictionary<string, CacheData>() ;

		/// <summary>
		/// キャッシュを強制的にクリアする
		/// </summary>
		/// <returns></returns>
		public static bool ClearCacheData()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_CacheData.Clear() ;

			return true ;
		}

		/// <summary>
		/// キャッシュデータを設定する
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="alive"></param>
		/// <returns></returns>
		public static bool SetCacheData( string label, System.Object value, bool alive = false )
		{
			if( m_Instance == null || string.IsNullOrEmpty( label ) == true || value == null )
			{
				return false ;
			}

			if( m_Instance.m_CacheData.ContainsKey( label ) == false )
			{
				m_Instance.m_CacheData.Add( label, new CacheData(){ Value = value, Alive = alive } ) ;
			}
			else
			{
				m_Instance.m_CacheData[ label ] = new CacheData(){ Value = value, Alive = alive } ;
			}

			Debug.LogWarning( "[WorkData] キャッシュに登録しました : " + label ) ;

			return true ;
		}
		
		/// <summary>
		/// キャッシュデータを取得する
		/// </summary>
		/// <returns>The cache data.</returns>
		/// <param name="label">Label.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetCacheData<T>( string label ) where T : class 
		{
			if( m_Instance == null || string.IsNullOrEmpty( label ) == true )
			{
				return default ;
			}

			if( m_Instance.m_CacheData.ContainsKey( label ) == false )
			{
				return default ;
			}

			if( ( m_Instance.m_CacheData[ label ].Value is T ) == false )
			{
				return default ;
			}

			return m_Instance.m_CacheData[ label ].Value as T ;
		}


		//-------------------------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

//			Debug.LogWarning( "------- デリゲート登録" ) ;
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged ;
//			UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded ;
			// Unload は、常駐型に切り替わったシーンなど破棄されるシーンの回数分呼ばれるためイマイチ使えない
		}
		
		new protected void OnDestroy()
		{
			base.OnDestroy() ;

//			Debug.LogWarning( "------- デリゲート破棄" ) ;
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged ;
//			UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded ;
			// Unload は、常駐型に切り替わったシーンなど破棄されるシーンの回数分呼ばれるためイマイチ使えない
		}

		private void OnActiveSceneChanged( UnityEngine.SceneManagement.Scene tFromScene, UnityEngine.SceneManagement.Scene tToScene )
		{
			RefreshCacheData() ;
		}

		private void RefreshCacheData()
		{
			if( m_CacheData.Count == 0 )
			{
				return ;
			}

			int i, l = m_CacheData.Count ;
			string[] labels = new string[ l ] ;
			m_CacheData.Keys.CopyTo( labels, 0 ) ;

			for( i  = 0 ; i < l ; i ++ )
			{
				if( m_CacheData[ labels[ i ] ].Alive == false )
				{
					// このデータはシーン切り替えと同時に消去してよい
					m_CacheData.Remove( labels[ i ] ) ;
				}
			}
		}

	}
}
