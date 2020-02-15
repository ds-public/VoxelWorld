using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

using DBS.nLayout ;

using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// スクリーンの全体管理クラス Version 2019/09/18 0
	/// </summary>
	public class ScreenManager : SingletonManagerBase<ScreenManager>
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スクリーンデータを設定する
		/// </summary>
		/// <param name="tData"></param>
		/// <returns></returns>
		public static bool SetScreenAttributeData( ScreenAttribute[] screenAttributeData )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_ScreenAttributeData = screenAttributeData ;

			return false ;
		}
		
		// スクリーンデータ
		[SerializeField]
		private ScreenAttribute[] m_ScreenAttributeData = null ;
		
		/// <summary>
		/// 各レイアウトの基本情報
		/// </summary>
		[Serializable]
		public class LayoutData
		{
			public string		LayoutName ;
			public GameObject	LayoutRoot ;
			public LayoutBase	LayoutBase ;

			public LayoutData( string layoutName, GameObject layoutRoot, LayoutBase layoutBase )
			{
				LayoutName	= layoutName ;
				LayoutRoot	= layoutRoot ;
				LayoutBase	= layoutBase ;
			}
		}
		
		[SerializeField][NonSerialized]
		private List<LayoutData>	m_LayoutData = new List<LayoutData>() ;

		//-----------------------------------------------------------

		// 現在アクティブなフッターメニューボタン
		[SerializeField][NonSerialized]
		private int m_ActiveScreenAttributeDataIndex = -1 ;

		// シーンの遷移履歴
		public class ScreenAttributeHistory
		{
			public	string	ScreenAttributeName ;		// シーン名
			public	Dictionary<string,System.Object>	StackedData ;

			public ScreenAttributeHistory( string screenAttributeName, Dictionary<string,System.Object> stackedData )
			{
				ScreenAttributeName	= screenAttributeName ;

				StackedData = new Dictionary<string, object>() ;

				int i, l = stackedData.Count ;
				string[] keys = new string[ l ] ;
				stackedData.Keys.CopyTo( keys, 0 ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					StackedData.Add( keys[ i ], stackedData[ keys[ i ] ] ) ;
				}
			}
		}

		// シーンの遷移履歴
		[SerializeField][NonSerialized]
		private List<ScreenAttributeHistory>	m_ScreenAttributeHistory = new List<ScreenAttributeHistory>() ;

		// 書き込み専用の一時的なスタックデータ
		private Dictionary<string,System.Object>	m_StackedDataForSave = new Dictionary<string, object>() ;

		// 読み出し専用の一時的なスタック領域
		private Dictionary<string,System.Object>	m_StackedDataForLoad = new Dictionary<string, object>() ;

		// 直前の遷移が戻ったかどうかのフラグ
		private bool m_IsBack = false ;

		/// <summary>
		/// 直前の遷移か戻るだったかどうか
		/// </summary>
		public static bool IsBack
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsBack ;
			}
		}

		//-------------------------------------------------------------------------------------------

		private bool	m_FirstFillFadeIn = false ;

		private bool	m_Dropping = false ;

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

		private void OnActiveSceneChanged( UnityEngine.SceneManagement.Scene fromScene, UnityEngine.SceneManagement.Scene toScene )
		{
			if( m_Processing == null )
			{
				string screenName = toScene.name ;	// 遷移前のスクリーン
				m_ActiveScreenAttributeDataIndex = GetScreenAttributeDataIndexByScreenName( screenName ) ;
				Debug.LogWarning( "ScreenManager.Load を使わずにアクティブになったスクリーン名:" + screenName + " スクリーンインデックス番号:" + GetScreenAttributeDataIndexByScreenName( screenName ) ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 展開されているレイアウトのアクセスインターフェースを取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tLayoutName"></param>
		/// <returns></returns>
		public static T GetLayout<T>( string layoutName ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetLayout_Private<T>( layoutName ) ;
		}

		// 展開されているレイアウトのアクセスインターフェースを取得する
		private T GetLayout_Private<T>( string layoutName ) where T : UnityEngine.Component
		{
			int i, l = m_LayoutData.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_LayoutData[ i ].LayoutName == layoutName )
				{
					// 合致する名前を発見した
					return m_LayoutData[ i ].LayoutRoot.GetComponent<T>() ;
				}
			}

			return null ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 履歴を積む
		/// </summary>
		/// <param name="tSceneName"></param>
		/// <returns></returns>
		public static bool AddHistory( string screenAttributeName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_ScreenAttributeHistory.Add( new ScreenAttributeHistory( screenAttributeName, m_Instance.m_StackedDataForSave ) ) ;
			m_Instance.m_StackedDataForSave.Clear() ;

			return true ;
		}

		
		/// <summary>
		/// 遷移履歴を消去する
		/// </summary>
		public static bool ClearHistory( int level = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearHistory_Private( level ) ;
		}

		private  bool ClearHistory_Private( int level )
		{
			if( level <= 0 )
			{
				// 全ヒストリー消去
				m_ScreenAttributeHistory.Clear() ;
			}
			else
			{
				// 指定レベル分だけ消去
				int i, l = m_ScreenAttributeHistory.Count ;

				if( level >  l )
				{
					level  = l ;
				}

				for( i  = 0 ; i <  level ; i ++ )
				{
					l = m_ScreenAttributeHistory.Count ;
					m_ScreenAttributeHistory.RemoveAt( l - 1 ) ;
				}
			}

			return true ;
		}

		/// <summary>
		/// 現在のシーンで一時的に保存したい値を保存する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		/// <returns></returns>
		public static bool SetStackData( string key, System.Object value )
		{
			if( m_Instance == null || string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_Instance.m_StackedDataForSave.ContainsKey( key ) == true )
			{
				return false ;
			}

			m_Instance.m_StackedDataForSave.Add( key, value ) ;

			return true ;
		}

		/// <summary>
		/// 現在のシーンで一時的に保存したい値を消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearStackData()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_StackedDataForSave.Clear() ;

			return true ;
		}

		/// <summary>
		/// 現在のシーンで一時的に保存した値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public static System.Object GetStackData( string key )
		{
			if( m_Instance == null || string.IsNullOrEmpty( key ) == true )
			{
				return null ;
			}

			if( m_Instance.m_StackedDataForLoad.ContainsKey( key ) == false )
			{
				return null ;
			}

			return m_Instance.m_StackedDataForLoad[ key ] ;
		}

		/// <summary>
		/// 現在のシーンで一時的に保存した値を確認する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public static bool ContainsStackData( string key )
		{
			if( m_Instance == null || string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_Instance.m_StackedDataForLoad.ContainsKey( key ) == false )
			{
				return false ;
			}

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// シーン名からシーンデータのインデックスを取得する
		private int GetScreenAttributeDataIndexByScreenAttributeName( string screenAttributeName )
		{
			int i, l = m_ScreenAttributeData.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_ScreenAttributeData[ i ].AttributeName == screenAttributeName )
				{
					// 発見
					return i ;
				}
			}

			return -1 ;	// 発見出来ず
		}

		// シーン名からシーンデータのインデックスを取得する
		private int GetScreenAttributeDataIndexByScreenName( string screenName )
		{
			int i, l = m_ScreenAttributeData.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_ScreenAttributeData[ i ].ScreenName == screenName )
				{
					// 発見
					return i ;
				}
			}

			return -1 ;	// 発見出来ず
		}

		// シーン名からシーンデータを取得する
		private ScreenAttribute GetScreenAttributeDataByScreenAttributeName( string screenAttributeName, out int oScreenAttributeDataIndex )
		{
			oScreenAttributeDataIndex = GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) ;
			if( oScreenAttributeDataIndex <  0 )
			{
				return null ;
			}

			return m_ScreenAttributeData[ oScreenAttributeDataIndex ] ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 履歴にスタックされたスクリーン名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetStackedScreenAttributeName( int level = 0 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetStackedScreenAttributeName_Private( level ) ;
		}

		private string GetStackedScreenAttributeName_Private( int level )
		{
			if( level <  0 )
			{
				level  = 0 ;
			}

			string screenAttributeName = string.Empty ;

			if( m_ScreenAttributeHistory.Count >  0 && m_ScreenAttributeHistory.Count >  level )
			{
				ScreenAttributeHistory tScreenAttributeHistory = m_ScreenAttributeHistory[ m_ScreenAttributeHistory.Count - 1 - level ] ;
				screenAttributeName = tScreenAttributeHistory.ScreenAttributeName ;
			}

			return screenAttributeName ;
		}

		/// <summary>
		/// 履歴にスタックされたスクリーン名を照合する
		/// </summary>
		/// <returns></returns>
		public static bool IsStackedScreenAttributeName( string screenAttributeName, int level = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.IsStackedScreenAttributeName_Private( screenAttributeName, level ) ;
		}

		private bool IsStackedScreenAttributeName_Private( string screenAttributeName, int level )
		{
			if( string.IsNullOrEmpty( screenAttributeName ) == true || level <  0 )
			{
				return false ;
			}

			if( m_ScreenAttributeHistory.Count == 0 || m_ScreenAttributeHistory.Count <= level )
			{
				return false ;
			}

			ScreenAttributeHistory tScreenAttributeHistory = m_ScreenAttributeHistory[ m_ScreenAttributeHistory.Count - 1 - level ] ;

			if( tScreenAttributeHistory.ScreenAttributeName != screenAttributeName )
			{
				return false ;
			}
			
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// 遷移処理を実行中かどうか
		private IEnumerator m_Processing = null ;

		/// <summary>
		/// 遷移処理を実行中かどうか
		/// </summary>
		public static bool IsProcessing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				
				return m_Instance.m_Processing != null || m_Instance.m_FirstFillFadeIn_Running == true ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 最初から該当がシーンが開かれている場合にフッターの設定を行う
		/// </summary>
		/// <param name="tIndex"></param>
		/// <returns></returns>
		public static AsyncState SetupAsync( string screenAttributeName )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			AsyncState state = new AsyncState() ;

			m_Instance.StartCoroutine( m_Instance.SetupAsync_Private( screenAttributeName, state ) ) ;

			return state ;
		}

		// 最初から該当がシーンが開かれている場合にフッターの設定を行う
		private IEnumerator SetupAsync_Private( string screenAttributeName, AsyncState state )
		{
			int screenAttributeDataIndex = GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) ;
			if( screenAttributeDataIndex <  0 )
			{
				state.Error = "Bad screen attribute name" ;	// シーン名が異常
				yield break ;
			}

			//----------------------------------------------------------

			ScreenAttribute screenAttributeDataNew = m_ScreenAttributeData[ screenAttributeDataIndex ] ;

			//----------------------------------------------------------

			// レイアウトの破棄と展開を行う
			yield return StartCoroutine( ProcessLayout( screenAttributeDataNew ) ) ;

			//----------------------------------------------------------
			
			// 各レイアウト内で準備用のデリゲートが登録されたはずなのでそれの実行完了を待つ
			if( onSetup != null )
			{
				yield return StartCoroutine( onSetup( screenAttributeDataNew ) ) ;
			}

			//----------------------------------------------------------

			// ＢＧＭ再生
			if( string.IsNullOrEmpty( screenAttributeDataNew.BgmName ) == false )
			{
				// 再生あり
				BGM.PlayMain( screenAttributeDataNew.BgmName ) ;
			}

			//----------------------------------------------------------

			// 選択中のインデックスを更新する
			m_ActiveScreenAttributeDataIndex = screenAttributeDataIndex ;

			// 最初のフィルフェードインフラグをクリアする
			m_FirstFillFadeIn = false ;

			//----------------------------------------------------------

			// 準備が整った
			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スクリーンの遷移を行う
		/// </summary>
		/// <param name="tSceneName"></param>
		/// <returns></returns>
		public static bool Load( string screenAttributeName, bool historyClear = false )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private( screenAttributeName, historyClear ) ;
		}

		/// スクリーンの遷移を行う
		private bool Load_Private( string screenAttributeName, bool historyClear )
		{
			if( m_Processing != null )
			{
				// 遷移中
				return false ;
			}

			//----------------------------------

			int screenAttributeDataIndex = GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) ;

			// スクリーン名に該当するシーンデータを選出する
			if( screenAttributeDataIndex <  0 )
			{
				// 該当するシーンデータが存在しない
				return false ;
			}

			// 戻るではない
			m_IsBack = false ;

			ScreenAttribute screenAttributeDataNew = m_ScreenAttributeData[ screenAttributeDataIndex ] ;

			// 戻るではない場合のみスタックを操作する
			if( historyClear == true )
			{
				// メニューボタンで直接遷移であったり特殊なスクリーンに遷移する場合は履歴を消去する
				m_ScreenAttributeHistory.Clear() ;
			}
			else
			{
				ScreenAttribute screenAttributeDataOld = null ;	// null になる事はありえる(type=0のスクリーンを直接開いてSetupAsyncを実行せずに再びtype=0のスクリーンに遷移)
				if( m_ActiveScreenAttributeDataIndex >= 0 )
				{
					screenAttributeDataOld = m_ScreenAttributeData[ m_ActiveScreenAttributeDataIndex ] ;
				}

				if( screenAttributeDataOld != null && screenAttributeDataNew.ScreenName != screenAttributeDataOld.ScreenName )
				{
					// インナーシーンの遷移であるためヒストリーに積む(ただし同シーンである場合は積まない)
					m_ScreenAttributeHistory.Add( new ScreenAttributeHistory( screenAttributeDataOld.AttributeName, m_StackedDataForSave ) ) ;
					m_StackedDataForSave.Clear() ;
					m_StackedDataForLoad.Clear() ;
				}
			}

			m_Processing = LoadAsync_Private( screenAttributeName, null ) ;
			StartCoroutine( m_Processing ) ;

			return true ;

		}

		/// <summary>
		/// スクリーンの復帰を行う
		/// </summary>
		public static bool Back( string screenAttributeName = Scene.Screen.Title )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Back_Private( screenAttributeName ) ;
		}

		/// スクリーンの復帰を行う
		private bool Back_Private( string screenAttributeName )
		{
			if( m_Processing != null )
			{
				// 遷移中
				return false ;
			}

			//----------------------------------

			if( m_ScreenAttributeHistory.Count >  0 )
			{
				// 履歴がある
				ScreenAttributeHistory tScreenHistory = m_ScreenAttributeHistory[ m_ScreenAttributeHistory.Count - 1 ] ;
				m_ScreenAttributeHistory.RemoveAt( m_ScreenAttributeHistory.Count - 1 ) ;

				// １つ前のシーンへ戻る
				screenAttributeName = tScreenHistory.ScreenAttributeName ;

				// 読み出し専用のスタックデータ領域に格納する
				m_StackedDataForLoad = tScreenHistory.StackedData ;
			}

			// スクリーン名に該当するシーンデータを選出する
			if( GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) <  0 )
			{
				// 該当するシーンデータが存在しない
				return false ;
			}

			// 戻る
			m_IsBack = true ;

			Debug.LogWarning( "-----戻り先の名前:" + screenAttributeName ) ;

			m_Processing = LoadAsync_Private( screenAttributeName, null ) ;
			StartCoroutine( m_Processing ) ;

			return true ;
		}

		// 強制的にリセットする際のシーン遷移
		/// <summary>
		/// 内部シーンの遷移を行う
		/// </summary>
		/// <param name="tSceneName"></param>
		/// <returns></returns>
		public static AsyncState RebootAsync( string screenAttributeName = Scene.Screen.Reboot )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			//-------------------------------------------------

			return m_Instance.RebootAsync_Private( screenAttributeName ) ;
		}

		private AsyncState RebootAsync_Private( string screenAttributeName )
		{
			// 遷移中であれば停止させる
			if( m_Processing != null )
			{
				StartCoroutine( m_Processing ) ;
				m_Processing = null ;
			}

			// スクリーン名に該当するシーンデータを選出する
			if( GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) <  0 )
			{
				// 該当するシーンデータが存在しない
				return null ;
			}

			//-------------------------------------------------

			// 戻るではない
			m_IsBack = false ;

			// 履歴をクリアする
			m_ScreenAttributeHistory.Clear() ;

			// リブートを実行する
			AsyncState state = new AsyncState() ;
			StartCoroutine( LoadAsync_Private( screenAttributeName, state ) ) ;
			return state ;
		}

		//---------------------------------------------------------------------------

		// スクリーンの遷移を行う
		private IEnumerator LoadAsync_Private( string screenAttributeName, AsyncState state )
		{
			// 入力を全面的に禁止する
			UIEventSystem.Disable( 22 ) ;

			//----------------------------------------------------------

//			Debug.LogWarning( "------ m_ActiveScreenDataIndex:" + m_ActiveScreenDataIndex ) ;

			ScreenAttribute screenAttributeDataOld = null ;	// null になる事はありえる(type=0のスクリーンを直接開いてSetupAsyncを実行せずに再びtype=0のスクリーンに遷移)
			if( m_ActiveScreenAttributeDataIndex >= 0 )
			{
				screenAttributeDataOld = m_ScreenAttributeData[ m_ActiveScreenAttributeDataIndex ] ;
			}

			// スクリーン名に該当するシーンデータを選出する
			int screenAttributeDataIndex = GetScreenAttributeDataIndexByScreenAttributeName( screenAttributeName ) ;
			ScreenAttribute screenAttributeDataNew = m_ScreenAttributeData[ screenAttributeDataIndex ] ;

			//----------------------------------------------------------

			// 遷移前のコールバック呼び出し(レイアウト用)
			if( OnLoad != null )
			{
				yield return StartCoroutine( OnLoad( screenAttributeName ) ) ;
			}

			//----------------------------------------------------------

			// 遷移前のコールバック呼び出し(スクリーン用)
			if( OnExit != null )
			{
				yield return StartCoroutine( OnExit( screenAttributeName ) ) ;
				
				OnExit = null ;	// 一度呼び出したら必ず初期化する
			}

			//----------------------------------------------------------

			string oldBgmName = BGM.GetMainName() ;
			if(	string.IsNullOrEmpty( oldBgmName ) == false &&	oldBgmName != screenAttributeDataNew.BgmName && screenAttributeDataNew.BgmName != null )
			{
				// ＢＧＭ停止(この後のシーン切り替えが重いとフェードアウトの途中で固まる。かといってフェードアウトを強制的に待つとシーン切り替えの時間が延びるので、あえてフェードアウトが途中で固まるままにする。)
				if( screenAttributeDataNew.Type == 0 )
				{
					yield return BGM.StopMainAsync( 0.2f ) ;    // 時間を 0.5f → 0.2f に短縮した
				}
				else
				{
					BGM.StopMain( 0.2f ) ;
				}
			}

			//----------------------------------------------------------
			
			// デフォルトで準備完了状態とする
			if( screenAttributeDataNew.Type == -1 )
			{
				m_Ready = false ;	// 遷移先のシーンでの準備完了通知を待たない
			}
			else
			if( screenAttributeDataNew.Type ==  0 )
			{
				m_Ready = true ;	// 遷移先のシーンでの準備完了通知を待たない
			}
			else
			if( screenAttributeDataNew.Type ==  1 )
			{
				m_Ready = false ;	// フェード終了を待つようになる
			}

			//----------------------------------

			// レイアウト用の通常のフェードアウトのデリゲートを呼ぶ(終了を待たない)
			if( OnFadeOut != null )
			{
				StartCoroutine( OnFadeOut( screenAttributeDataOld, screenAttributeDataNew ) ) ;
			}

			//----------------------------------------------------------

			// 遷移前のコールバック呼び出し(スクリーン用)
			if( OnDrop != null )
			{
				StartCoroutine( Drop( screenAttributeName ) ) ;

				OnDrop = null ;	// 一度呼び出したら必ず初期化する
			}

			//----------------------------------------------------------

			// レイアウト用の塗りつぶしフェードアウトのデリゲートを呼ぶ(終了を待つ)
			if( OnFillFadeOut != null )
			{
				yield return StartCoroutine( OnFillFadeOut( screenAttributeDataOld, screenAttributeDataNew ) ) ;
			}

			//----------------------------------------------------------

			if( m_Dropping == true )
			{
				yield return new WaitWhile( () => m_Dropping == true ) ;
			}

			//----------------------------------------------------------

			if( screenAttributeDataNew.Type == -1 )
			{
				yield return Fade.Out( 0, 0.25f ) ;
			}
			else
			if( screenAttributeDataNew.Type ==  0 )
			{
				Debug.LogWarning( "------------フェード表示" ) ;
				SceneMask.Show() ;
				yield return new WaitForEndOfFrame() ;
			}

			//----------------------------------------------------------

			// レイアウトの破棄と展開を行う
			yield return StartCoroutine( ProcessLayout( screenAttributeDataNew ) ) ;

			//----------------------------------------------------------

			// 注意：破棄→展開を明示的に行わないと前の画面が表示されてしまうケースがある

			// シーンを破棄する(最後にロードしたしたシーンを破棄しようとすると警告が出て挙動的によろしく無さそうなので無し)
//			if( tScreenDataOld != null )
//			{
//				yield return Scene.RemoveAsync( tScreenDataOld.screenName ) ;
//			}

			// シーンを展開する
			yield return Scene.LoadAsync( screenAttributeDataNew.ScreenName ) ;

			if( screenAttributeDataNew.Type == 0 )
			{
				Debug.LogWarning( "------------フェード解除" ) ;
				yield return new WaitForEndOfFrame() ;
				SceneMask.Hide() ;
			}

			//---------------------------------------------------------

			// 必要に応じてＢＧＭを再生する
//			tOldBgmName = BGM.GetMainName() ;
			if( string.IsNullOrEmpty( screenAttributeDataNew.BgmName ) == false && screenAttributeDataNew.BgmName != oldBgmName )
			{
				BGM.PlayMain( screenAttributeDataNew.BgmName ) ;
			}

			//----------------------------------------------------------

			// ロードされたシーンで準備が完了するまでフェードインを行わせたくない場合は遷移先のシーンでこのフラグを操作する
			if( m_Ready == false )
			{
/*				float w = 3.0f ;	// ３秒経過でも抜けるようにしておく(保険)
				float t = 0.0f ;

				while( t <  w && m_Ready == false )
				{
					t += Time.deltaTime ;
					yield return null ;					
				}*/

				yield return new WaitWhile( () => m_Ready == false ) ;
			}

			//----------------------------------------------------------

			// 塗りつぶしフェードインを実行する(終了を待つ)
			if( OnFillFadeIn != null )
			{
				yield return StartCoroutine( OnFillFadeIn( screenAttributeDataOld, screenAttributeDataNew ) ) ;
			}

			// フェードインを実行する(終了を待たない)
			if( OnFadeIn != null )
			{
				StartCoroutine( OnFadeIn( screenAttributeDataOld, screenAttributeDataNew ) ) ;
			}

			// フルフェードイン
			if( screenAttributeDataNew.Type == -1 )
			{
				yield return Fade.In( 0, 0.25f ) ;
			}

			//----------------------------------------------------------

			// 新しいシーンに完全に切り替わった
			
			// アクテイブなシーンデータのインデックス番号を更新する(このタイミングで書き換える必要がある・FadeIn FadeOut 等のメソッド内で、直で m_ActiveSceneDataIndex を参照しているため書き換えるタイミングが重要になる。本来はこのようなコードは好ましくないので、後々時間が取れるなら修正したい。)
			m_ActiveScreenAttributeDataIndex = screenAttributeDataIndex ;

			//----------------------------------------------------------

			// 入力を全面的に許可する(入力禁止を全てリセット)
			UIEventSystem.Enable( 22 ) ;	// シンプルシーンに遷移する場合は禁止カウントをリセット

			//----------------------------------------------------------

			if( state == null )
			{
				m_Processing = null ;
			}
			else
			{
				state.IsDone = true ;
			}

			//----------------------------------------------------------

			// Scene の履歴は使用しないのでクリアする(←いや、使用するので
//			SceneManagementHelper.EnhancedSceneManager.ClearHistory() ;
		}

		private IEnumerator Drop( string screenAttributeName )
		{
			m_Dropping = true ;
			yield return StartCoroutine( OnDrop( screenAttributeName ) ) ;
			m_Dropping = false ;
		}

		//-------------------------------------------------------------------------------------

		// レイアウトの破棄と展開を行う
		private IEnumerator ProcessLayout( ScreenAttribute screenAttributeDataNew )
		{
			// 新旧のスクリーンでレイアウトを照合して必要なものは展開し不要なものは破棄する
			List<string> loadLayout		= new List<string>() ;
			List<LayoutData> freeLayout	= new List<LayoutData>() ;

			int i, j, l, m ;
			if( screenAttributeDataNew.LayoutNames == null )
			{
				// 全て不要になる
				l = m_LayoutData.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					freeLayout.Add( m_LayoutData[ i ] ) ;
				}
			}
			else
			{
				string layoutName ;
				bool f ;

				// 必要になるものを照合する

				l = screenAttributeDataNew.LayoutNames.Length ;
				if( l >  0 )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						layoutName = screenAttributeDataNew.LayoutNames[ i ] ;
						
						f = true ;

						m = m_LayoutData.Count ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							if( m_LayoutData[ j ].LayoutName == layoutName )
							{
								// 既に展開済み
								m_LayoutData[ j ].LayoutBase.Refresh() ;	// 継続して使用する場合はシーンのロード時にリフレッシュが必要になる
								f = false ;
								break  ;
							}
						}

						if( f == true )
						{
							// 展開が必要(ただし重複は禁止)
							if( loadLayout.Contains( layoutName ) == false )
							{
								loadLayout.Add( layoutName ) ;
							}
						}
					}
				}

				// 不要になるものを照合する
				l = m_LayoutData.Count ;
				if( l >  0 )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						layoutName = m_LayoutData[ i ].LayoutName ;
						
						f = true ;

						m = screenAttributeDataNew.LayoutNames.Length ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							if( layoutName == screenAttributeDataNew.LayoutNames[ j ] )
							{
								// 継続使用
								f = false ;
								break  ;
							}
						}

						if( f == true )
						{
							// 新しいシーンでは使用されないので破棄する
							if( freeLayout.Contains( m_LayoutData[ i ] ) == false )
							{
								freeLayout.Add( m_LayoutData[ i ] ) ;
							}
						}
					}
				}
			}

			if( freeLayout.Count >  0 )
			{
				// 破棄する
				l = freeLayout.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					DestroyImmediate( freeLayout[ i ].LayoutRoot ) ;
					m_LayoutData.Remove( freeLayout[ i ] ) ;
				}
			}

			if( loadLayout.Count >  0 )
			{
				// 展開する
				string layoutName ;
				UnityEngine.SceneManagement.Scene scene ;
				GameObject[] go ;
				LayoutBase layoutBase ;

				l = loadLayout.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					layoutName = loadLayout[ i ] ;
					yield return Scene.AddAsync( layoutName ) ;

					scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( layoutName ) ;
					go = scene.GetRootGameObjects() ;

					if( go != null && go.Length >  0 )
					{
						layoutBase = go[ 0 ].GetComponent<LayoutBase>() ;
						if( layoutBase != null )
						{
							// 初期化完了を待つ
							yield return new WaitWhile( () => layoutBase.IsInitialized() == false ) ;

							// 展開中のレイアウト情報に登録する
							m_LayoutData.Add( new LayoutData( layoutName, go[ 0 ], layoutBase ) ) ;
	
							// 展開と同時に常駐フラグをオンにする
							DontDestroyOnLoad( go[ 0 ] ) ;
							
							//-----------------------------
				
							// 原点じゃないと気持ち悪い
							go[ 0 ].transform.localPosition = Vector3.zero ;
							go[ 0 ].transform.localRotation = Quaternion.identity ;
							go[ 0 ].transform.localScale = Vector3.one ;
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------------

		// 準備状況
		private bool m_Ready = false ;

		/// <summary>
		/// 準備状況の取得と設定
		/// </summary>
		public static bool Ready
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.m_Ready ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.m_Ready = value ;

				if( value == true && m_Instance.m_FirstFillFadeIn == false )
				{
					m_Instance.m_FirstFillFadeIn = true ;

					// 最初のフェードインのデリゲートを呼ぶ
					m_Instance.StartCoroutine( m_Instance.FirstFillFadeIn_Private() ) ;
				}
			}
		}

		private bool m_FirstFillFadeIn_Running = false ;

		// 最初の塗りつぶしフェードインを実行する
		private IEnumerator FirstFillFadeIn_Private()
		{
			if( OnFillFadeIn != null )
			{
				// 入力を禁止
				UIEventSystem.Disable( 22 ) ;

				m_FirstFillFadeIn_Running = true ;
				
				ScreenAttribute screenData = null ;
				if( m_ActiveScreenAttributeDataIndex >= 0 )
				{
					// 少なくともこのメソッドが呼ばれるタイミングでは Setup が実行されているはずである
					screenData = m_ScreenAttributeData[ m_ActiveScreenAttributeDataIndex ] ;
				}

				yield return StartCoroutine( OnFillFadeIn( null, screenData ) ) ;

				m_FirstFillFadeIn_Running = false ;

				// 入力を許可(強制)
				UIEventSystem.Enable( 22 ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 直接展開時の表示設定用のデリゲート
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnSetup( ScreenAttribute newScreenAttribute ) ;	// どのスクリーンの時に生成されたか

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnSetup onSetup ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
/*		public static bool SetOnLayoutLoaded( OnSetup tOnSetup )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onSetup = tOnSetup ;

			return true ;
		}*/

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool AddOnSetup( OnSetup onSetup )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.onSetup += onSetup ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool RemoveOnSetup( OnSetup onSetup )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onSetup -= onSetup ;

			return true ;
		}
		
		//-----------------------------------

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドの定義
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnLoadDelegate( string screenAttributeName ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnLoadDelegate OnLoad ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
/*		public static bool SetOnLoad( OnLoad tOnLoad )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onLoad = tOnLoad ;

			return true ;
		}*/

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool AddOnLoad( OnLoadDelegate onLoad )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.OnLoad += onLoad ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool RemoveOnLoad( OnLoadDelegate onLoad )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnLoad -= onLoad ;

			return true ;
		}
				
		//-----------------------------------

		/// <summary>
		/// 画面塗りつぶしフェードイン(終了を待つ)
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnFillFadeInDelegate( ScreenAttribute oldScreenAttribute, ScreenAttribute newScreenAttribute ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnFillFadeInDelegate OnFillFadeIn ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnFillFadeIn"></param>
//		public static bool SetOnFillFadeIn( OnFillFadeIn tOnFillFadeIn )
//		{
//			if( m_Instance == null )
//			{
//				return false ;
//			}
//
//			m_Instance.onFillFadeIn = tOnFillFadeIn ;
//
//			return true ;
//		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnFillFadeIn"></param>
		public static bool AddOnFillFadeIn( OnFillFadeInDelegate onFillFadeIn )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.OnFillFadeIn += onFillFadeIn ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnFillFadeIn"></param>
		public static bool RemoveOnFillFadeIn( OnFillFadeInDelegate onFillFadeIn )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnFillFadeIn -= onFillFadeIn ;

			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// フェードイン(終了を待たない)
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnFadeInDelegate( ScreenAttribute oldScreenAttribute, ScreenAttribute newScreenAttribute ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnFadeInDelegate OnFadeIn ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnFadeIn"></param>
//		public static bool SetOnFadeIn( OnFadeIn tOnFadeIn )
//		{
//			if( m_Instance == null )
//			{
//				return false ;
//			}
//
//			m_Instance.onFadeIn = tOnFadeIn ;
//
//			return true ;
//		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnFadeIn"></param>
		public static bool AddOnFadeIn( OnFadeInDelegate onFadeIn )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.OnFadeIn += onFadeIn ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnFadeIn"></param>
		public static bool RemoveOnFadeIn( OnFadeInDelegate onFadeIn )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnFadeIn -= onFadeIn ;

			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// フェードアウト(終了を待たない)
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnFadeOutDelegate( ScreenAttribute oldScreenAttribute, ScreenAttribute newScreenAttribute ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnFadeOutDelegate OnFadeOut ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnFadeOut"></param>
//		public static bool SetOnFadeOut( OnFadeOut tOnFadeOut )
//		{
//			if( m_Instance == null )
//			{
//				return false ;
//			}
//
//			m_Instance.onFadeOut = tOnFadeOut ;
//
//			return true ;
//		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnFadeOut"></param>
		public static bool AddOnFadeOut( OnFadeOutDelegate onFadeOut )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.OnFadeOut += onFadeOut ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnFadeOut"></param>
		public static bool RemoveOnFadeOut( OnFadeOutDelegate onFadeOut )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnFadeOut -= onFadeOut ;

			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// 画面塗りつぶしフェードアウト(終了を待つ)
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnFillFadeOutDelegate( ScreenAttribute oldScreenAttribute, ScreenAttribute newScreenAttribute ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnFillFadeOutDelegate OnFillFadeOut ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnFillFadeOut"></param>
//		public static bool SetOnFillFadeOut( OnFillFadeOut tOnFillFadeOut )
//		{
//			if( m_Instance == null )
//			{
//				return false ;
//			}
//
//			m_Instance.onFillFadeOut = tOnFillFadeOut ;
//
//			return true ;
//		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnFillFadeOut"></param>
		public static bool AddOnFillFadeOut( OnFillFadeOutDelegate onFillFadeOut )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.OnFillFadeOut += onFillFadeOut ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnFillFadeOut"></param>
		public static bool RemoveOnFillFadeOut( OnFillFadeOutDelegate onFillFadeOut )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnFillFadeOut -= onFillFadeOut ;

			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドの定義
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnExitDelegate( string screenName ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnExitDelegate OnExit ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool SetOnExit( OnExitDelegate onExit )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnExit = onExit ;

			return true ;
		}
/*
		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool AddOnSceneExit( OnExit tOnExit )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.onExit += tOnExit ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool RemoveOnExit( OnExit tOnExit )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onExit -= tOnExit ;

			return true ;
		}
*/		

		//-----------------------------------

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドの定義
		/// </summary>
		/// <param name="tScreenName"></param>
		/// <returns></returns>
		public delegate IEnumerator OnDropDelegate( string screenAttributeName ) ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッド
		/// </summary>
		public OnDropDelegate OnDrop ;

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを設定する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool SetOnDrop( OnDropDelegate onDrop )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.OnDrop = onDrop ;

			return true ;
		}
/*
		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを追加する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool AddOnDrop( OnDrop tOnDrop )
		{
			if( m_Instance == null )
			{
				return false  ;
			}

			m_Instance.onDrop += tOnDrop ;

			return true ;
		}

		/// <summary>
		/// スクリーンを抜ける際に呼び出されるデリゲートメソッドを削除する
		/// </summary>
		/// <param name="tOnScreenExit"></param>
		public static bool RemoveOnDrop( OnDrop tOnDrop )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onDrop -= tOnDrop ;

			return true ;
		}
*/		


	}
}
