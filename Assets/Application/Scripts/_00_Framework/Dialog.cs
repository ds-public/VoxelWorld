using UnityEngine ;
using System ;
using System.Collections ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;
using SceneManagementHelper ;

namespace DBS
{
	/// <summary>
	/// タイアログクラス(汎用ダイアログの表示に使用する) Version 2017/08/31 0
	/// </summary>
	public class Dialog : MonoBehaviour
	{
		/// <summary>
		/// ダイアログの表示状態クラス
		/// </summary>
		public class State : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool				IsDone ;

			/// <summary>
			/// 選択されたもの
			/// </summary>
			public int				Index = -1 ;

			/// <summary>
			/// 結果値
			/// </summary>
			public System.Object	Result ;

			/// <summary>
			/// 任意の型にキャストして結果を取得する
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="defaultValue"></param>
			/// <returns></returns>
			public T GetResult<T>( T defaultValue = default )
			{
				if( Result == null )
				{
					return defaultValue ;
				}

				return ( T )Result ;
			}

			public T GetCurrent<T>( T defaultValue = default )
			{
				if( Result == null )
				{
					return defaultValue ;
				}

				return ( T )Result ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// シングルトンインスタンス
		private static Dialog m_Instance ;

		/// <summary>
		/// ダイアログクラスのインスタンス
		/// </summary>
		public  static Dialog   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		//-------------------------------------

		// ダイアログの展開レイヤー
		[SerializeField,Header("ダイアログを展開するルートノード")]
		protected UIView	m_Layer ;

		//-------------------------------------

		// スタイルごとのウィンドウ

		// １ボタンタイプ
		[SerializeField,Header("スタンダードタイプ")]
		protected DialogStyle.Standard	m_Standard ;

		//-------------------------------------

		/// <summary>
		/// 肯定的な選択肢を選んだ際に再生される効果音の名前
		/// </summary>
		public	string							soundOfPositive = "" ;

		// 表示中のダイアログの数
		[SerializeField]
		protected int m_DialogCount ;

		//-----------------------------------

		/// <summary>
		/// 外側にも反応するかどうか(ただしボタンが１つの場合のみ)
		/// </summary>
		/// 
		public static bool OutsideEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_OutsideEnable ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_OutsideEnable = value ;
			}
		}
		
		private bool m_OutsideEnable = true ;

		//---------------------------------------------------------------------------

		void Awake()
		{
			m_Instance = this ;

			//----------------------------------------------------------

			// 各要素を非表示状態にしておく

			if( m_Standard != null )
			{
				m_Standard.SetActive( false ) ;
			}

			//----------------------------------------------------------

			// 表示しているダイアログの数
			m_DialogCount = m_Layer.transform.childCount ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 任意のスタイルのダイアログを開く
		/// </summary>
		/// <typeparam name="T">スタイルの型(クラス)</typeparam>
		/// <param name="tCallback">ダイアログ内で発生したアクションを受け取るコールバックメソッド</param>
		/// <returns></returns>
		public static T Open<T>( Action<T,string,string> tCallback ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.Open_Private<T>( tCallback ) ;

		}

		// 任意のスタイルのダイアログを開く
		private T Open_Private<T>( Action<T,string,string> tCallback ) where T : UnityEngine.Component
		{
			if( m_Layer.transform.childCount == 0 )
			{
				// ダイアログシステムを有効化する
				gameObject.SetActive( true ) ;
			}

			//----------------------------------------------------------

			DialogStyle.DialogStyleBase styleBase = null ;

			if( m_Standard is T )
			{
				// スタンダードタイプ
				DialogStyle.Standard dialog = m_Layer.AddPrefab<DialogStyle.Standard>( m_Standard.gameObject ) ;
				dialog.Callback = tCallback as Action<DialogStyle.Standard,string,int> ;
				styleBase = dialog ;
			}
			
			if( styleBase == null )
			{
				// 該当するスタイルが存在しない
				return null ;
			}

			// 汎用的な準備処理
			styleBase.Prepare() ;

			// 表示順を調整する
			float z = GetHiestPriority() ;
			Vector3 p = styleBase.transform.position ;
			p.z = z ;
			styleBase.transform.position = p ;

			m_Layer.SortChildByZ() ;

			// フェードインを実行して表示を要求する
			FadeIn( styleBase ) ;

			return  styleBase as T ;	// インスタンスを返す(設定を反映させるためには Commit を実行する必要がある)
		}

		//-----------------------------------------------------------

		// ダイアログをフェードイン効果付きで表示する
		private void FadeIn( DialogStyle.DialogStyleBase styleBase )
		{
			// ダイアログコントローラー自体をアクティブにしておく
			styleBase.SetActive( true ) ;

			// フェードインで表示する
			StartCoroutine(	styleBase.FadeIn() ) ;
		}

		void Update()
		{
			if( m_Layer.transform.childCount != m_DialogCount )
			{
				if( m_Layer.transform.childCount == 0 )
				{
					// 表示しているダイアログが無くなったらスリープ状態に入る
					gameObject.SetActive( false ) ;
				}

				// 現在のダイアログの表示数を更新する
				m_DialogCount = m_Layer.transform.childCount ;
			}
		}
		
		//-------------------------------------------------------------------------------------------
		
		// 新しいダイプのダイアログ
		
		/// <summary>
		/// シンプルなダイアログを開く
		/// </summary>
		/// <returns>The open.</returns>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="buttonLabels">Button labels.</param>
		/// <param name="onClosed">On closed.</param>
		/// <param name="buttonIndex">Button index.</param>
		public static State Open( string title, string message, string[] buttonLabels = null, Action<int> onClosed = null, int buttonIndex = 0 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.Open_Private( title, message, buttonLabels, onClosed, buttonIndex ) ;
		}

		// シンプルなダイアログを開く
		private State Open_Private( string title, string message, string[] buttonLabels, Action<int> onClosed, int buttonIndex )
		{
			State state = new State() ;

			DBS.DialogStyle.Standard dialog = Dialog.Open<DBS.DialogStyle.Standard>( null ) ;
			dialog.Title.Text				= title ;
			dialog.Message.Text				= message ;
			dialog.SelectionButtonLabels	= buttonLabels ;
			dialog.SelectionButtonIndex		= buttonIndex ;
			dialog.AutoClose				= true ;
			dialog.AutoCloseCallback		= onClosed ;
			dialog.state					= state ;
			dialog.Commit() ;

			return state ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 表示中のダイアログで最もプライオリティ値が高い値を取得する
		private int GetHiestPriority()
		{
			int i, l = m_Layer.transform.childCount ;

			if( l == 0 )
			{
				return 0 ;
			}

			float highestZ = - Mathf.Infinity ;
			float z ;

			for( i  = 0 ; i <  l ; i ++  )
			{
				z = m_Layer.transform.GetChild( i ).transform.position.z ;
				if( z >  highestZ )
				{
					highestZ  = z ;
				}
			}

			return ( int )highestZ ;
		}

		/// <summary>
		/// 肯定的な回答を選んだ際に再生される効果音名を設定する
		/// </summary>
		/// <param name="tSoundOfPositive">効果音名</param>
		public static void SetSoundOfPositive( string soundOfPositive )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.soundOfPositive = soundOfPositive ;
		}


		//-------------------------------------------------------------------------------------------

	}
}
