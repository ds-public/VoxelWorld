using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using uGUIHelper ;
using CSVHelper ;
using TransformHelper ;


namespace DBS._Screen._Template
{
	/// <summary>
	/// テンプレートスクリーンのコントロールクラス
	/// </summary>
	public class Template : MonoBehaviour
	{
		// スクリーン
		[SerializeField]
		protected UIImage	m_Screen ;

		// ボタン
		[SerializeField]
		protected UIButton	m_Button ;

		//-------------------------------------------------------------------------------------------

		// [] で囲まれたコメント部分の処理は必須処理

		void Awake()
		{
			// [ApplicationManager を起動する(既にヒエラルキーにインスタンスが生成されていればスルーされる)]
			ApplicationManager.Create() ;
		}

		IEnumerator Start()
		{
			// [ApplicationManager の準備が整うのを待つ]
			if( ApplicationManager.IsInitialized == false )
			{
				yield return new WaitWhile( () => ApplicationManager.IsInitialized == false ) ;
			}

			//----------------------------------------------------------

			if( ScreenManager.IsProcessing == false )
			{
				// いきなりこのシーンを開いたケース(デバッグ動作)
				yield return ScreenManager.SetupAsync( Scene.Screen.Template ) ;
			}

			// シーンの遷移が発生する際に呼び出されるデリゲートメソッドを登録する
			ScreenManager.SetOnExit( OnExit ) ;

			//----------------------------------------------------------

			// [フェードアウト(ブラックアウト)中に行いたい処理を記述する(シーンのセットアップ)]

			// スクリーンをタッチしたら反応するようにする
			// 注意：この設定はあくまでサンプルなので実際のシーンではこの設定は削除すること
			m_Screen.isInteraction = true ;					// スクリーンのインタラクションを許可する
			m_Screen.SetOnClick( OnClick ) ;				// スクリーンがタッチされた際に呼び出されるメソッドを設定する
			m_Button.SetOnButtonClick( OnButtonClick ) ;	// ボタンがタッチされた際に呼び出されるメソッドを設定する

			//----------------------------------------------------------

			// [シーンの準備が完全に整ったのでフェードイン許可のフラグを有効にする]
			Scene.Ready = true ;
			ScreenManager.Ready = true ;

			//----------------------------------------------------------

			// [フェードイン完了を待つ]
			yield return new WaitWhile( () => ( Scene.IsFading == true || ScreenManager.IsProcessing == true ) ) ;

			//----------------------------------------------------------

			// [フェードイン完了後に画面で動かしたい処理を記述する]

			// 注意：このダイアログ表示はあくまでサンプルなので実際のシーンではこのダイアログ表示は削除すること
//			Dialog.Open( "テンプレートシーン解説", "ダイアログを閉じた後\n画面をタッチすると\nタイトルへ遷移します" ) ;
			Dialog.Open( "", "ダイアログを閉じた後\n画面をタッチすると\nタイトルへ遷移します" ) ;
			// 汎用ダイアログを使用しメッセージを表示する
		}

		// シーン遷移要求が発生した際に呼び出されるメソッド
		private IEnumerator OnExit( string tScreenName )
		{
			Debug.LogWarning( "------> " + tScreenName + " へ遷移します" ) ;
			yield return null ;
		}

		//-------------------------------------------------------------------------------------------

		// 対象がタッチ(クリック)された際に呼び出されるメソッド
		// 注意：このメソッドはあくまでサンプルなので実際のシーンではこのメソッドは削除すること
		private void OnClick( string tIdentity, UIView tView )
		{
			ScreenManager.Load( Scene.Screen.Title ) ;  // タイトルへ遷移する
			// 注意：アウトゲーム内では原則としてシーンの遷移に ScreenManager.Load を使用すること
		}

		// 対象がタッチ(クリック)された際に呼び出されるメソッド(ボタン限定)
		// 注意：このメソッドはあくまでサンプルなので実際のシーンではこのメソッドは削除すること
		private void OnButtonClick( string tIdentity, UIButton tButton )
		{
			// 固有ダイアログを開く
//			StartCoroutine( OpenTemplateDialog( "シーンから開いたダイアログ", "<ruby=ぶんしょう>文章</ruby>" ) ) ;
		}

		// 固有ダイアログを開く
/*		private IEnumerator OpenTemplateDialog( string tTitle, string tMessage )
		{
			nDialog.Template[] tDialog = { null } ;

			// 固有ダイアログシーンを加算する
			yield return Scene.AddAsync<nDialog.Template>( Scene.Dialog.Template, tDialog ) ;

			// 固有ダイアログのレイアウトを設定する
			yield return tDialog[ 0 ].Open( tTitle, tMessage,
			( tResult ) =>
			{
				if( tResult == true )
				{
					Debug.LogWarning( "------> 固有ダイアログの結果は [決定]" ) ;
				}
				else
				{
					Debug.LogWarning( "------> 固有ダイアログの結果は [キャンセル]" ) ;
				}
			} ) ;
		}*/
	}
}

