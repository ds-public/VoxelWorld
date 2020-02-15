using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// マネージャの基底クラス	Version 2017/08/13 0
	/// </summary>
	public class SingletonManagerBase<T> : MonoBehaviour where T : MonoBehaviour
	{
		/// <summary>
		/// シングルトンマネージャのインスタンス
		/// </summary>
		protected static T m_Instance ; 

		/// <summary>
		/// シングルトンマネージャのインスタンス
		/// </summary>
		public  static T   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		/// <summary>
		/// シングルトンマネージャを生成する
		/// </summary>
		/// <returns>フェイシャルマネージャのインスタンス</returns>
		public static T Create( Transform tParent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}

			m_Instance = FindObjectOfType( typeof( T ) ) as T;
			if( m_Instance == null )
			{
				string tName = typeof( T ).ToString() ;
				int i = tName.LastIndexOf( '.' ) ;
				if( i >= 0 )
				{
					tName = tName.Substring( i + 1, tName.Length - ( i + 1 ) ) ;
				}
				GameObject tGameObject = new GameObject( tName ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}
				
				m_Instance = tGameObject.AddComponent<T>() ;
			}

			return m_Instance ;
		}

		/// <summary>
		/// シングルトンマネージャを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}

		//-----------------------------------------------------------

		// 生成
		protected void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				DestroyImmediate( gameObject ) ;
				return ;
			}
		
			T tInstanceOther = FindObjectOfType( typeof( T ) ) as T;
			if( tInstanceOther != null )
			{
				if( tInstanceOther != this )
				{
					DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this as T ;
		
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}

	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;

			//----------------------------

			// 派生クラスの Awake を呼ぶ
//			OnAwake() ;
		}

		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
//		virtual protected void OnAwake(){}


		// 破棄された際に呼び出される
		protected void OnDestroy()
		{
			// 派生クラスの破棄を呼ぶ
//			OnTerminate() ;

			//----------------------------

			if( m_Instance == this )
			{
				m_Instance  = null ;	// 最後の１つが破棄されたのでシングルトンのインスタンスをクリアする
			}
		}

		/// <summary>
		/// 派生クラスの破棄
		/// </summary>
//		virtual protected void OnTerminate(){}


	}
}
