// 強制的にムービー再生
#define PLAY_MOVIE

// ムービー演出を完全にカット
//#define DISABLE_MOVIE

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using AssetBundleHelper ;

namespace DSW.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理
	public partial class Downloading
	{
		/// <summary>
		/// ロア構造体
		/// </summary>
		public class LoreStructure
		{
			public string	Title ;
			public string	Description ;
		}


		/// <summary>
		/// 実際にダウンロードを行う
		/// </summary>
		/// <returns></returns>
		private async UniTask<bool> Process( bool ignorePlayerData )
		{
			// 必要なアセットバンドルを取得する(チュートリアル終了前はチュートリアルで必要な分のみ取得する)
			var targetAssetBundlePaths = GetTargetAssetBundlePaths( ignorePlayerData ) ;
			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				// 必要なアセットバンドルは全て揃っている
				return true ;	// 処理を継続して良い
			}

			//------------------------------------------------------------------------------------------

			// ロアメッセージをピックアップする(５種類)
			List<LoreStructure> lores = new List<LoreStructure>()
			{
				new LoreStructure(){ Title = "題名１", Description = "説明１" },
				new LoreStructure(){ Title = "題名２", Description = "説明２" },
				new LoreStructure(){ Title = "題名３", Description = "説明３" },
				new LoreStructure(){ Title = "題名４", Description = "説明４" },
				new LoreStructure(){ Title = "題名５", Description = "説明５" },
			} ;

			//----------------------------------

			// ムービーファイルがダンロード済みか確認する
			// これにより今後の流れが２つに分岐する

			// ムービーから続けて別のアセットもダウンロードするかどうか(続けて別のアセットのダウンロードを行う場合は次のダウンロード確認は行わない)
			bool movieAndOther = false ;

#if !DISABLE_MOVIE
			// ムービーがあるか確認する
			if( Asset.Exists( m_MoviePath ) == false )
			{
				// ムービーファイルが無いためムービーファイルのダウンロードと必要なアセットバンドルをダウンロードする(対象はチュートリアル前と後で変化する)

				// ロア画面でムービーファイルをダウンロードする

				Dictionary<string,AssetBundleManager.DownloadEntity> movieAssetBundlePaths = new Dictionary<string, AssetBundleManager.DownloadEntity>() ;
				movieAssetBundlePaths.Add( m_MoviePath, new AssetBundleManager.DownloadEntity(){ Path = m_MoviePath, Keep = true } ) ;

				// ダウンロード確認(ムービー＋他のアセット)
//				if( await OpenDownloadConfirmDialog( movieAssetBundlePaths ) == false )
				if( await OpenDownloadConfirmDialog( targetAssetBundlePaths ) == false )
				{
					// キャンセル
					return false ;
				}

				// ムービーのみダウンロードする
				await Execute( movieAssetBundlePaths, true, lores, "お待ち下さい", true ) ;

				// ムービーとその他を続けてダウンロードする
				movieAndOther = true ;
			}
#endif
			//----------------------------------------------------------

			// 改めてダウンロードするアセットバンドルを取得する(ムービーが除外されている)
			targetAssetBundlePaths = GetTargetAssetBundlePaths( ignorePlayerData ) ;
			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				// 必要なアセットバンドルは全て揃っている(ダウンロードは不要)
				return true ;	// 処理を継続して良い
			}

			// 注意：ムービーから続けてダウンロードする場合はここではダウンロード確認を行わない
			if( movieAndOther == false )
			{
				// ダウンロード確認
				if( await OpenDownloadConfirmDialog( targetAssetBundlePaths ) == false )
				{
					// キャンセル
					return false ;
				}
			}

			//----------------------------------------------------------

			// チュートリアル前ではムービー再生ダウンロード
			// ムービー再生中にダウンロードが終わらなかった場合はロアに切り替わる

			// チュートリアル後では最初からロアダウンロード

			bool isSkipMovie = false ;

			// プレイヤーデータを無視しない場合はチュートリアルの終了状況によりムービーを再生しないか決定する
			if( movieAndOther == true )
			{
				isSkipMovie = false ;
			}
#if !PLAY_MOVIE
			else
			{
//				if( ignorePlayerData == false )
//				{
//					isSkipMovie = Tutorial.IsCompleted ;
//				}
			}
#endif
			await Execute( targetAssetBundlePaths, isSkipMovie, lores, "ゲームを開始できます", isSkipMovie ) ;
			
			// 処理継続
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// アセットバンドルをダウンロードして良いか確認する
		private async UniTask<bool> OpenDownloadConfirmDialog( Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths )
		{
			// ダウンロードに必要なサイズ
			long totalSize = GetTotalDownloadSize( targetAssetBundlePaths ) ;

			if( totalSize <  0 )
			{
				Blocker.Off() ;
				await Dialog.Open( "注意", "ダウンロードサイズが異常です\n\nタイトル画面に戻ります" + totalSize, new string[]{ "閉じる" } ) ;
				Blocker.On() ;
				return false ;
			}

			//------------------------------------------------------------------------------------------
			// ストレージ空き容量のチェックを行う

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
			// 空き容量を確認する
			if( await ApplicationManager.CheckStorage( totalSize ) == false )
			{
				return false ;
			}
#endif
			//----------------------------------------------------------

			// ダウンロードを行ってよいか確認する
			string message = "<color=#FF7F00>" + ExString.GetSizeName( totalSize ) + " </color>のデータを\nダウンロードします\n\nよろしいですか？" ;

			Blocker.Off() ;
			int index = await Dialog.Open( "ダウンロード確認", message, new string[]{ "ダウンロード", "キャンセル" }, outsideEnabled:false ) ;
			Blocker.On() ;
			if( index == 1 )
			{
				// ダウンロードキャンセルでタイトルへ戻る
				return false ;
			}

			// ダウンロード実行
			return true ;
		}

		//-------------------------------------------------------------------------------------------
		
		// アセットバンドル群のダウンロードを実行する
		private async UniTask Execute( Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths, bool isSkipMovie, List<LoreStructure> lores, string completedMessage, bool isAllManifestsSaving )
		{
			// プログレスバネルを準備する
			m_ProgressPanel.Prepare() ;

			if( isSkipMovie == false )
			{
				// ムービーを再生する
				m_MoviePanel.View.SetActive( true ) ;
				m_MoviePanel.Play( m_MoviePath ).Forget() ;

				// 一度再生した事のフラグを立てる(続けてチュートリアルで再生しようとしたらスキップする)
				ApplicationManager.IsMoviePlayedByDownload = true ;

				m_LorePanel.View.SetActive( false ) ;
			}
			else
			{
				// 最初からロアを表示する
				m_LorePanel.Prepare( lores ) ;
				await m_LorePanel.FadeIn() ;
			}

			//----------------------------------

			// プログレスをフェードインする
			await m_ProgressPanel.FadeIn() ;

			// ダウンロード実行
			bool downloadCompleted = false ;

			//--------------
			// ディレイ
//			if( m_MoviePanel.View.ActiveSelf == true )
//			{
//				await WaitForSeconds( 60 ) ;	// デバッグ
//			}
			//--------------

			if( targetAssetBundlePaths == null || targetAssetBundlePaths.Count == 0 )
			{
				Debug.LogWarning( "ダウンロード対象が異常です" ) ;
			}


			// 指定したアセットバンドルをダウンロードする
			DownloadAssetBundleAsync
			(
				this, targetAssetBundlePaths,
				( long downloadedSize, long writtenSize, long totalSize, int storedFile, int totalFile, AssetBundleManager.DownloadEntity[] targets, int nowParallel, int maxParallel, int httpVersion ) =>
				{
					m_ProgressPanel.Set( downloadedSize, writtenSize, totalSize, storedFile, totalFile, targets, nowParallel, maxParallel, httpVersion ) ;
				},
				() =>
				{
					// ダウンロード完了
					downloadCompleted = true ;
				},
				isAllManifestsSaving
			).Forget() ;

			//----------------------------------------------------------

			Blocker.Off() ;

			// ダウンロード完了まで待機する
			while( true )
			{
				if( downloadCompleted == true )
				{
					// ダウンロード完了
					break ;
				}

				//---------------------------------------------------------
				// ダウンロードは終わっていないがムービーが終了してしまった

				if( m_MoviePanel.View.ActiveSelf == true )
				{
					if( m_MoviePanel.IsPlaying == false )
					{
						// ムービーバネルをフェードアウトで消してロアを表示する
						m_LorePanel.Prepare( lores ) ;
						m_LorePanel.View.SetActive( true ) ;
						m_LorePanel.View.Alpha = 1 ;	// フェードアウトでアルファが０になっている対策

						// ムービーバネルをフェードアウトする
						await m_MoviePanel.FadeOut() ;
					}
				}

				await Yield() ;	// これが無いとフリーズするので注意
			}

			//----------------------------------------------------------
			// ダウンロード終了

			// ダウンロード終了の形に表示を変える
			await m_ProgressPanel.Complete( completedMessage ) ;

			// ダウンロードが終了したのでムービー再生中であればスキップボタンを出す
			if( m_MoviePanel.View.ActiveSelf == true )
			{
				if( m_MoviePanel.IsPlaying == true )
				{
					m_MoviePanel.ShowSkipButton().Forget() ;
				}

				// ムービーの再生が終了するのを待つ
				await WaitWhile( () => ( m_MoviePanel.IsPlaying == true ) ) ;

				m_LorePanel.View.SetActive( false ) ;
				m_MoviePanel.View.SetActive( false ) ;

				await When( m_ProgressPanel.FadeOut() ) ;
			}

			if( m_MoviePanel.View.ActiveSelf == false && m_LorePanel.View.ActiveSelf == true )
			{
				await WhenAll
				(
					m_LorePanel.FadeOut(),
					m_ProgressPanel.FadeOut()
				) ;
			}

			//----------------------------------------------------------

			if( isAllManifestsSaving == false )
			{
				// ムービーを再生している場合はフェードアウトしたタイミングでマニフェストを保存する
				AssetBundleManager.SaveAllManifestInfo() ;
			}

			//----------------------------------------------------------
			// 画面はそのままに黒フェードさせる

			Blocker.On() ;
		}
	}
}
