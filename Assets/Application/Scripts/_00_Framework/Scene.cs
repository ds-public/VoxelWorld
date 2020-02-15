using UnityEngine ;
using System ;
using System.Collections ;

// 要 SceneManagementHelper パッケージ
using SceneManagementHelper ;

using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// シーンクラス(シーンの展開や追加に使用する)  Version 2019/09/18 0
	/// </summary>
	public class Scene
	{
		// スクリーン
		public class Screen
		{
			/// <summary>
			/// Title
			/// </summary>
			public const string Title					= "Screen_Title" ;

			//----------------------------------------------------------
			// Town

			/// <summary>
			/// Square
			/// </summary>
			public const string World					= "Screen_World" ;

			/// <summary>
			/// Town
			/// </summary>
			public const string Template				= "Screen_Template" ;

			/// <summary>
			/// Reboot
			/// </summary>
			public const string Reboot					= "Screen_Title" ;
		}

		// ダイアログ
		public class Dialog
		{
			/// <summary>
			/// TownInformation
			/// </summary>
			public const string	TownInformation			= "Dialog_TownInformation" ;

			public const string Town					= "Dialog_Town_2" ;

			public const string Keyboard				= "Dialog_Keyboard" ;

			public const string Item					= "Dialog_Item" ;

			public const string PlayerData				= "Dialog_PlayerData" ;
			
			public const string Template				= "Dialog_Template" ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == true || string.IsNullOrEmpty( Error ) == false )
					{
						return false ;   // 終了
					}
					return true ;    // 継続
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error = string.Empty ;

			/// <summary>
			/// インスタンス
			/// </summary>
			public UnityEngine.Object[]		Instances = null ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// フェード付きシーン遷移で false になるまでフェードインを待たせる
		/// </summary>
		public static bool Ready = true ;

		private static bool m_Fading = false ;
		public static bool IsFading
		{
			get
			{
				return m_Fading ;
			}
		}

		//-----------------------------------------------------------------

		// Load LoadAsync

		/// <summary>
		/// 指定した名前のシーンを展開する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load( string sceneName, string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( "Scene.Load( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする
			
			return EnhancedSceneManager.Load( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを展開する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( "Scene.Load<T>( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする
			
			return EnhancedSceneManager.Load<T>( sceneName, onLoaded, targetName, label, value ) ;
		}
		
		/// <summary>
		/// 指定した名前のシーンを展開する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Request LoadAsync( string sceneName, string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( "Scene.LoadAsync( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAsync_Private( sceneName, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator LoadAsync_Private( string sceneName, string label, System.Object value, Request request )
		{
			UIEventSystem.Disable( 23 ) ;

			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.LoadAsync( sceneName, label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン展開", sceneName + "の展開に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}


		/// <summary>
		/// 指定した名前のシーンを展開する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Request LoadAsync<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( "Scene.LoadAsync<T>( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( LoadAsync_Private<T>( sceneName, onLoaded, targetName, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator LoadAsync_Private<T>( string sceneName, Action<T[]> onLoaded, string targetName, string label, System.Object value, Request request ) where T : UnityEngine.Component
		{
			UIEventSystem.Disable( 23 ) ;

			T[] targets = null ;
			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.LoadAsync<T>( sceneName, ( _ ) => { targets = _ ; }, targetName, label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				request.Instances = targets ;
				onLoaded?.Invoke( targets ) ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン展開", sceneName + "の展開に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}
		
		//-----------------------------------

		/// <summary>
		/// フェード演出込みで指定した名前のシーンを展開する(呼び出し元のシーンは途中で破棄されてしまうため常駐型のゲームオブジェクト上で実行する必要がある)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <param name="tFastLoad">シーンの展開そのものは同期で行い処理にかかるトータルの時間を短くするか(true=する・false=しない)</param>
		/// <param name="tOnFadeOutFinished">フェードアウトが完了し画面が完全に単色で塗りつぶされたタイミングで呼び出される</param>
		/// <param name="tBlockingFadeIn">許可されるまでフェードインをブロックする</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool LoadWithFade( string sceneName, string label = "", System.Object value = null, bool fastLoad = true, Action<string> onFadeOutFinished = null, bool blockingFadeIn = false )
		{
			if( ApplicationManager.Instance == null )
			{
				return false ;
			}

			ApplicationManager.Instance.StartCoroutine( LoadWithFade_Private( sceneName, label, value, fastLoad, onFadeOutFinished, blockingFadeIn ) ) ;

			return true ;
		}

		/// <summary>
		/// フェード演出込みで指定した名前のシーンを展開する(呼び出し元のシーンは途中で破棄されてしまうため常駐型のゲームオブジェクト上で実行する必要がある)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <param name="tFastLoad">シーンの展開そのものは同期で行い処理にかかるトータルの時間を短くするか(true=する・false=しない)</param>
		/// <param name="tOnFadeOutFinished">フェードアウトが完了し画面が完全に単色で塗りつぶされたタイミングで呼び出される</param>
		/// <param name="tBlockingFadeIn">許可されるまでフェードインをブロックする</param>
		/// <returns>列挙子</returns>
		private static IEnumerator LoadWithFade_Private( string sceneName, string label = "", System.Object value = null, bool fastLoad = true, Action<string> onFadeOutFinished = null, bool blockingFadeIn = false )
		{
			Ready = false ;
			m_Fading = true ;

			// フェードアウト演出を実行する
			yield return Fade.Out() ;

			// フェードアウトが終わった際に呼び出すコールバックメソッドを呼び出す
			onFadeOutFinished?.Invoke( sceneName ) ;

			// デフォルトで準備完了状態とする
			Ready |= ( blockingFadeIn == false ) ;

			Asset.ClearResourceCache( "Scene.LoadWithFade( " + sceneName + " )" ) ;	// リソースキャッシュをクリアする

			// コンテンツ部の新しいシーンを展開する
			if( fastLoad == true )
			{
				// 同期版
				EnhancedSceneManager.Load( sceneName, label, value ) ;
			}
			else
			{
				// 非同期版
				yield return EnhancedSceneManager.LoadAsync( sceneName, label, value ) ;
			}

			// ロードされたシーンで準備が完了するまでフェードインを行わせたくない場合にこのフラグを操作する
			yield return new WaitWhile( () => Ready == false ) ;

			// フェードイン演出を実行する
			yield return Fade.In() ;

			m_Fading = false ;
		}

		//-----------------------------------------------------------

		// Add AddAsync
		
		/// <summary>
		/// 指定した名前のシーンを追加する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add( string sceneName, string label = "", System.Object value = null )
		{
			// 新しいシーンを追加する
			return EnhancedSceneManager.Add( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			// 新しいシーンを追加する
			return EnhancedSceneManager.Add<T>( sceneName, onLoaded, targetName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync( string sceneName, string label = "", System.Object value = null )
		{
			// 新しいシーンを追加する
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( AddAsync_Private( sceneName, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator AddAsync_Private( string sceneName, string label, System.Object value, Request request )
		{
			UIEventSystem.Disable( 23 ) ;

			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.AddAsync( sceneName, label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン加算", sceneName + "の加算に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}

		/// <summary>
		/// 指定した名前のシーンを追加する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync<T>( string sceneName, Action<T[]> onLoaded, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			// 新しいシーンを追加する
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( AddAsync_Private<T>( sceneName, onLoaded, targetName, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator AddAsync_Private<T>( string sceneName, Action<T[]> onLoaded, string targetName, string label, System.Object value, Request request ) where T : UnityEngine.Component
		{
			UIEventSystem.Disable( 23 ) ;

			T[] targets = null ;
			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.AddAsync<T>( sceneName, ( _ ) => { targets = _ ; }, targetName, label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				request.Instances = targets ;
				onLoaded?.Invoke( targets ) ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン加算", sceneName + "の加算に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}

		//-----------------------------------------------------

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back( string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( "Scene.Back()" ) ;	// リソースキャッシュをクリアする
			
			return EnhancedSceneManager.Back( label, value ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back<T>( Action<T[]> onLoaded = null, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( "Scene.Back<T>()" ) ;	// リソースキャッシュをクリアする
			
			return EnhancedSceneManager.Back<T>( onLoaded, targetName, label, value ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(非同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Request BackAsync( string label = "", System.Object value = null )
		{
			Asset.ClearResourceCache( "Scene.BackAsync()" ) ;	// リソースキャッシュをクリアする

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( BackAsync_Private( label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator BackAsync_Private( string label, System.Object value, Request request )
		{
			UIEventSystem.Disable( 23 ) ;

			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.BackAsync( label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン後退", "後退に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}

		/// <summary>
		/// 現在のシーンの１つ前に展開されていたシーンを展開する(非同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Request BackAsync<T>( Action<T[]> onLoaded = null, string targetName = "", string label = "", System.Object value = null ) where T : UnityEngine.Component
		{
			Asset.ClearResourceCache( "Scene.BackAsync<T>()" ) ;	// リソースキャッシュをクリアする

			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( BackAsync_Private<T>( onLoaded, targetName, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator BackAsync_Private<T>( Action<T[]> onLoaded, string targetName, string label, System.Object value, Request request ) where T : UnityEngine.Component
		{
			UIEventSystem.Disable( 23 ) ;

			T[] targets = null ;
			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.BackAsync<T>( ( _ ) => { targets = _ ; }, targetName, label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				request.Instances = targets ;
				onLoaded?.Invoke( targets ) ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン後退", "後退に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				yield break ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// フェード演出込みで現在のシーンの１つ前に展開されていたシーンを展開する
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <param name="tFastLoad">シーンの展開そのものは同期で行い処理にかかるトータルの時間を短くするか(true=する・false=しない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool BackWithFade( string label = "", System.Object value = null, bool fastLoad = true )
		{
			if( ApplicationManager.Instance == null )
			{
				return false ;
			}

			ApplicationManager.Instance.StartCoroutine( BackWithFade_Private( label, value, fastLoad ) ) ;

			return true ;
		}

		/// <summary>
		/// フェード演出込みで現在のシーンの１つ前に展開されていたシーンを展開する
		/// </summary>
		/// <returns>The with fade private.</returns>
		/// <param name="tLabel">T label.</param>
		/// <param name="tValue">T value.</param>
		/// <param name="tFastLoad">If set to <c>true</c> t fast load.</param>
		private static IEnumerator BackWithFade_Private( string label = "", System.Object value = null, bool fastLoad = true )
		{
			// フェードアウト演出を実行する
			yield return Fade.Out() ;

			Asset.ClearResourceCache( "Scene.BackWithFade()" ) ;	// リソースキャッシュをクリアする

			// コンテンツ部の新しいシーンを展開する
			if( fastLoad == true )
			{
				// 同期版
				EnhancedSceneManager.Back( label, value ) ;
			}
			else
			{
				// 非同期版
				yield return EnhancedSceneManager.BackAsync( label, value ) ;
			}

			// フェードイン演出を実行する
			yield return Fade.In() ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// 指定した名前のシーンを削除する
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Remove( string sceneName, string label = "", System.Object value = null )
		{
			return EnhancedSceneManager.Remove( sceneName, label, value ) ;
		}

		/// <summary>
		/// 指定した名前のシーンを削除する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Request RemoveAsync( string sceneName, Action<bool> onResult = null, string label = "", System.Object value = null )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( RemoveAsync_Private( sceneName, onResult, label, value, request ) ) ;
			return request ;
		}

		private static IEnumerator RemoveAsync_Private( string sceneName, Action<bool> onResult, string label, System.Object value, Request request )
		{
			UIEventSystem.Disable( 23 ) ;

			bool result = false ;
			EnhancedSceneManager.Request subRequest ;
			yield return subRequest = EnhancedSceneManager.RemoveAsync( sceneName, ( _ ) => { result = _ ; } , label, value ) ;

			if( subRequest.IsDone == true )
			{
				// 成功
				UIEventSystem.Enable( 23 ) ;

				request.IsDone = true ;
				onResult?.Invoke( true ) ;
				yield break ;
			}
			else
			{
				// 失敗(エラーダイアログの表示)
				UIEventSystem.Enable( 23 ) ;

				UIEventSystem.Push() ;
				UIEventSystem.Enable( -1 ) ;

				yield return AlertDialog.Open( "シーン削除", sceneName + "の削除に失敗しました" ) ;

				UIEventSystem.Pop() ;

				request.Error = "Failed" ;
				onResult?.Invoke( false ) ;
				yield break ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １つ前のシーンの名前を取得する
		/// </summary>
		/// <returns>１つ前のシーンの名前(nullで存在しない)</returns>
		public static string GetPreviousName()
		{
			return EnhancedSceneManager.GetPreviousName() ;
		}

		/// <summary>
		/// 現在のシーン名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetActiveName()
		{
			return EnhancedSceneManager.GetActiveName() ;
		}

		/// <summary>
		/// 現在のシーン名
		/// </summary>
		/// <returns></returns>
		public static string Name
		{
			get
			{
				// 現在のシーンの名前を取得する
				return EnhancedSceneManager.GetActiveName() ;
			}
		}

		/// <summary>
		/// 指定の名前のシーンが現在ロードされているか取得する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public static bool IsLoaded( string sceneName )
		{
			return EnhancedSceneManager.IsLoaded( sceneName ) ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// 受け渡しパラメータを設定する
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tObject">受け渡しパラメータの値</param>
		/// <returns></returns>
		public static bool SetParameter( string label, System.Object value )
		{
			return EnhancedSceneManager.SetParameter( label, value ) ;
		}
		
		/// <summary>
		/// 受け渡しパラメータを取得する
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tClear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか(true=する・false=しない)</param>
		/// <returns>受け渡しパラメータのインスタンス</returns>
		public static System.Object GetParameter( string label, bool clear = true )
		{
			return EnhancedSceneManager.GetParameter( label, clear ) ;
		}

		/// <summary>
		/// 受け渡しパラメータを取得する
		/// </summary>
		/// <typeparam name="T">任意の型</typeparam>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tClear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか(true=する・false=しない)</param>
		/// <returns>任意の型の受け渡しパラメータのインスタンス</returns>
		public static T GetParameter<T>( string label, bool clear = true ) where T : class
		{
			return EnhancedSceneManager.GetParameter<T>( label, clear ) as T ;
		}

		/// <summary>
		/// 受け渡しパラメータが存在するか確認する
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter( string label )
		{
			return EnhancedSceneManager.ContainsParameter( label ) ;
		}

		/// <summary>
		/// 受け渡しパラメータが存在するか確認する
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter<T>( string label )
		{
			return EnhancedSceneManager.ContainsParameter<T>( label ) ;
		}

		/// <summary>
		/// 受け渡しパラメータを削除する
		/// </summary>
		/// <param name="tLabel"></param>
		/// <returns></returns>
		public static bool RemoveParameter( string label )
		{
			return EnhancedSceneManager.RemoveParameter( label ) ;
		}
	}
}

