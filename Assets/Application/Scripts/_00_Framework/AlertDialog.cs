using UnityEngine ;
using System ;
using System.Collections ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;
using SceneManagementHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// タイアログクラス(汎用ダイアログの表示に使用する) Version 2017/09/15 0
	/// </summary>
	public class AlertDialog : MonoBehaviour
	{
		// シングルトンインスタンス
		private static AlertDialog m_Instance = null ;

		/// <summary>
		/// ダイアログクラスのインスタンス
		/// </summary>
		public  static AlertDialog   instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		// ダイアログの展開レイヤー
		[SerializeField,Header("ダイアログを展開するルートノード")]
		private UIView	m_Layer = null ;

		//-------------------------------------

		// スタイルごとのウィンドウ

		// スタンダードタイプ
		[SerializeField,Header("スタンダードタイプ")]
		private DialogStyle.Standard	m_Standard = null ;

		//-------------------------------------

		/// <summary>
		/// 肯定的な選択肢を選んだ際に再生される効果音の名前
		/// </summary>
		public	string							soundOfPositive = "" ;

		// 表示中のダイアログの数
		[SerializeField]
		private int m_DialogCount = 0 ;

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

			DialogStyle.DialogStyleBase tBase = null ;

			if( m_Standard is T )
			{
				// スタンダードタイプ
				DialogStyle.Standard tDialog = m_Layer.AddPrefab<DialogStyle.Standard>( m_Standard.gameObject ) ;
				tDialog.Callback = tCallback as Action<DialogStyle.Standard,string,int> ;
				tBase = tDialog ;
			}

			if( tBase == null )
			{
				// 該当するスタイルが存在しない
				return null ;
			}

			// 汎用的な準備処理
			tBase.Prepare() ;

			// 表示順を調整する
			float z = GetHiestPriority() ;
			Vector3 p = tBase.transform.position ;
			p.z = z ;
			tBase.transform.position = p ;

			m_Layer.SortChildByZ() ;

			// フェードインを実行して表示を要求する
			FadeIn( tBase ) ;

			return  tBase as T ;	// インスタンスを返す(設定を反映させるためには Commit を実行する必要がある)
		}

		// ダイアログをフェードイン効果付きで表示する
		private void FadeIn( DialogStyle.DialogStyleBase tBase )
		{
			// ダイアログコントローラー自体をアクティブにしておく
			tBase.SetActive( true ) ;

			// フェードインで表示する
			tBase.FadeIn() ;
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
		
		/// <summary>
		/// シンプルなダイアログを開く
		/// </summary>
		/// <param name="tMessage">メッセージ文字列</param>
		/// <param name="tLabel">ボタンのラベル情報の格納された配列(結果文字列:表示文字列)</param>
		/// <param name="tOnClosed">ダイアログが閉じられた際に呼び出されるアクションメソッド</param>
		public static Dialog.State Open( string tTitle, string tMessage, string[] tButtonLabel = null, Action<int> tOnClosed = null )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.Open_Private( tTitle, tMessage, tButtonLabel, tOnClosed ) ;
		}

		// シンプルなダイアログを開く
		private Dialog.State Open_Private( string tTitle, string tMessage, string[] tButtonLabel, Action<int> tOnClosed )
		{
			if( tButtonLabel == null )
			{
				tButtonLabel = new string[]{ "OK" } ;	// 完全デフォルト
			}

			Dialog.State tState = new Dialog.State() ;

			DialogStyle.Standard tDialog = AlertDialog.Open<DialogStyle.Standard>( null ) ;
			tDialog.Title.Text = tTitle ;
			tDialog.Message.Text = tMessage ;
			tDialog.SelectionButtonLabels = tButtonLabel ;
			tDialog.AutoClose = true ;
			tDialog.AutoCloseCallback = tOnClosed ;
			tDialog.state = tState ;
			tDialog.Commit() ;

			return tState ;
		}

		/// <summary>
		/// 時間で消えるシンプルなダイアログを開く
		/// </summary>
		/// <param name="tMessage">メッセージ文字列</param>
		/// <param name="tLabel">ボタンのラベル情報の格納された配列(結果文字列:表示文字列)</param>
		/// <param name="tOnClosed">ダイアログが閉じられた際に呼び出されるアクションメソッド</param>
		public static Dialog.State Show( string tTitle, string tMessage, float tTime = 2.0f, Action<int> tOnClosed = null, uint tTextColor = 0, int tTextAlign = 0 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.Show_Private( tTitle, tMessage, tTime, tOnClosed, tTextColor, tTextAlign ) ;
		}

		// 時間で消えるシンプルなダイアログを開く
		private Dialog.State Show_Private( string tTitle, string tMessage, float tTime, Action<int> tOnClosed, uint tTextColor = 0, int tTextAlign = 0 )
		{
			Dialog.State tState = new Dialog.State() ;

			DialogStyle.Standard tDialog = AlertDialog.Open<DialogStyle.Standard>( null ) ;
			tDialog.padding = new RectOffset( 24, 24, 48, 48 ) ;
			tDialog.Title.Text = tTitle ;
			tDialog.Message.Text = tMessage ;
			tDialog.Message.FontSize = 30 ;
			tDialog.SelectionButtonLabels = null ;
			tDialog.AutoClose = true ;
			tDialog.AutoCloseCallback = tOnClosed ;
			tDialog.state = tState ;
			tDialog.Commit() ;

			return tState ;
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

			float tHighestZ = - Mathf.Infinity ;
			float z ;

			for( i  = 0 ; i <  l ; i ++  )
			{
				z = m_Layer.transform.GetChild( i ).transform.position.z ;
				if( z >  tHighestZ )
				{
					tHighestZ  = z ;
				}
			}

			return ( int )tHighestZ ;
		}

		/// <summary>
		/// 肯定的な回答を選んだ際に再生される効果音名を設定する
		/// </summary>
		/// <param name="tSoundOfPositive">効果音名</param>
		public static void SetSoundOfPositive( string tSoundOfPositive )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.soundOfPositive = tSoundOfPositive ;
		}


		//-------------------------------------------------------------------------------------------

	}
}