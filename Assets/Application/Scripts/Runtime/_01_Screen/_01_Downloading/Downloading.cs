using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;
using AssetBundleHelper ;

using DSW.Screens.DownloadingClasses.UI ;

using DSW.World ;

namespace DSW.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新) Version 2022/10/01
	/// </summary>
	public partial class Downloading : ScreenBase
	{
		[Header( "各種パネル" )]

		[SerializeField]
		protected	LorePanel		m_LorePanel ;

		[SerializeField]
		protected	MoviePanel		m_MoviePanel ;

		[SerializeField]
		protected	ProgressPanel	m_ProgressPanel ;

		//-------------------------------------------------------------------------------------------

		// 後で Define か Constant に変える
		private const string m_MoviePath = "Movies/Sample" ;		// アセットバンドル版
//		private const string m_MoviePath = "Movies/Sample.mp4" ;	// オリジナルソース版

		//-------------------------------------------------------------------------------------------

		override protected void OnAwake()
		{
			// 最初に見えてはいけない表示物を非表示にする
			m_LorePanel.View.SetActive( false ) ;
			m_MoviePanel.View.SetActive( false ) ;
			m_ProgressPanel.View.SetActive( false ) ;
		}

		override protected async UniTask OnStart()
		{
			//----------------------------------------------------------
			// 画面が暗転中にこの画面の準備を整える

			// 設定を読み出す
			Settings settings = ApplicationManager.LoadSettings() ;

			// スリープ禁止
			Screen.sleepTimeout = SleepTimeout.NeverSleep ;

			//----------------------------------------------------------

			// フェードイン(画面表示)を許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;

			//------------------------------------------------------------------------------------------

			// ダウンロードのリクエストタイプを取得する
			ApplicationManager.DownloadingRequestTypes downloadingRequestType =
				Scene.GetParameter<ApplicationManager.DownloadingRequestTypes>( "DownloadingRequestType" ) ;

			if( downloadingRequestType != ApplicationManager.DownloadingRequestTypes.Phase1 && downloadingRequestType != ApplicationManager.DownloadingRequestTypes.Phase2 )
			{
				Debug.LogError( "Unknown DownloadingRequestType : " + downloadingRequestType ) ;
				return ;
			}

			//----------------------------------------------------------
			// Phase 1

			if
			(
				ApplicationManager.DownloadingPhase1State == ApplicationManager.DownloadingPhaseStates.None &&
				( downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase1 || downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase2 )
			)
			{
				Progress.On( "準備中" ) ;

				//---------------------------------------------------------

				// プレイヤーデータのローカル部分をロードする
//				PlayerData.LoadLocal() ;

				// オプションをロードする(ボリューム関係が必要であるためタイトル画面の前にロードする必要がある)
//				Option.Load() ;

				//---------------------------------------------------------

				// マスターデータのダウンロード(CheckVersion があるのでアセットバンドルのセットアップより前に行う必要がある)
//				bool result = await MasterDataManager.DownloadAsync( false, ( float progress ) =>
//				{
//					SetProgress( progressMin, progressMax, progress ) ;
//				} ) ;

//				if( result == false )
//				{
//					// 失敗
//					return ;
//				}

				// マスターデータをメモリに展開する
//				await MasterDataManager.LoadAsync() ;

				//---------------------------------------------------------

				// アセットバンドル全体共通の設定を行う
				SetupGeneralAssetBundleSettings() ;

				//---------------------------------------------------------
				// ローカルのアセットバンドル情報をロードする

				await SetupInternalAssetBundleSettings( this ) ;

				//---------------------------------------------------------

				// タイトル前から展開しておきたいアセットを展開する
				await LoadConstantAsset( 1, this ) ;

				//---------------------------------------------------------

				// 直後に重い処理があるとプログレスの最後のフレームのまま表示が残り続けるのできっちりプログレスが消えるのを待つ
				await Progress.OffAsync() ;

				//---------------------------------

				// Phase 1 完了
				ApplicationManager.DownloadingPhase1State = ApplicationManager.DownloadingPhaseStates.Completed ;
				Debug.Log( "<color=#00FF00>[Downloading] Phase 1 Completed !!</color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			// Phase 2

			string startScreenName = Scene.GetParameter<string>( "StartScreenName" ) ;

			// プレイヤーデータを無視するのか(true であれば BEASTログインを行わない)
			bool ignorePlayerData = false ;
//				!string.IsNullOrEmpty( startScreenName ) &&
//				Scene.IgnoreLoginScreens.Contains( startScreenName ) ;

			Debug.Log( "<color=#FF7F00>ログインをスルーするかどうか : " + ignorePlayerData + "</color>" ) ;


			if
			(
				ApplicationManager.DownloadingPhase2State == ApplicationManager.DownloadingPhaseStates.None &&
				downloadingRequestType == ApplicationManager.DownloadingRequestTypes.Phase2
			)
			{
				//---------------------------------------------------------
				// このタイミングでログインしていないければログインする(本来はタイトル画面で行われている)

				// ただしログインを無視しないシーンであれば
/*				if( PlayerData.IsLogin == false )
				{
					// 未ログインであればここでログインする
					if( await PlayerDataManager.Login() == false )
					{
						return ;	// ログイン出来なければこれ以上先には進ませない
					}
					Debug.Log( "<color=\"#00FFFF\">[Downloading] Login OK !!</color>" ) ;
				}*/

				//---------------------------------------------------------
				// リモートのアセットバンドル情報をダウンロードする

				Progress.On( "データ確認中" ) ;

				await SetupExternalAssetBundleSettings( this, ignorePlayerData ) ;

				await Progress.OffAsync() ;

				//---------------------------------------------------------
				// リモートのアセットバンドルを必要に応じて消去する

				ClearConstantAssetBundle() ;

				//-----------------------------------------------------------------------------------------

				// 実際にダウンロードを行う
				if( await Process( ignorePlayerData ) == false )
				{
					// キャンセルしてタイトル画面に戻る

					// スリープ許可
					Screen.sleepTimeout = SleepTimeout.SystemSetting ;

					Blocker.Off() ;

					// タイトル画面へ
					Scene.LoadWithFade( Scene.Screen.Title, blockingFadeIn:true, fadeType:Fade.FadeTypes.Loading ).Forget() ;

					throw new OperationCanceledException() ;	// タスクキャンセル
				}

				//-----------------------------------------------------------------------------------------
				// ダウンロード完了後

				// タイトル前から展開しておきたいアセットを展開する(再ダウンロードの都合で全て破棄されてしまうのでもう一度ロードする)
				await LoadConstantAsset( 1, this ) ;

				// 最初から展開しておきたいアセットを展開する
				await LoadConstantAsset( 2, this ) ;

				//---------------------------------------------------------

				// 強制消去フラグを完結させる
				await RefreshClearFlag() ;

				//---------------------------------------------------------

				// Phase 2 完了
				ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.Completed ;
				Debug.Log( "<color=#00FF00>[Downloading] Phase 2 Completed !!</color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			// スリープ許可
			Screen.sleepTimeout = SleepTimeout.SystemSetting ;

			//------------------------------------------------------------------------------------------

			// 終わったら指定のシーンに遷移する
			if( string.IsNullOrEmpty( startScreenName ) == true || startScreenName == Scene.Screen.Downloading || startScreenName == Scene.Screen.Title )
			{
				// いきなりこのシーンから始まった場合のケース(特殊なケースはダウンロードとタイトルのみ)
				// タイトルへ
				startScreenName = Scene.Screen.Title ;
			}

			Debug.Log( "<color=#FFFFFF>ダウンロードフェーズ後の遷移先のシーン:" + startScreenName + "</color>" ) ;

			Blocker.Off() ;

			//----------------------------------------------------------

			bool isGauge = false ;

//			if( PlayerData.TitleDiaplayed == true )
//			{
//				if
//				(
//					startScreenName == Scene.Screen.Square	||
//					startScreenName == Scene.Screen.Battle
//				)
//				{
//					isGauge = true ;
//				}
//			}

			Scene.LoadWithFade( startScreenName, blockingFadeIn:true, fadeType:Fade.FadeTypes.Loading, isGauge:isGauge ).Forget() ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アセットバンドルキャッシュを削除する
		/// </summary>
		public static async UniTask ClearCache( ExMonoBehaviour owner )
		{
			Blocker.On() ;

			string manifestName = "Default" ;

			if( AssetBundleManager.HasManifest( manifestName ) == false )
			{
				// 登録されていない
				Progress.On( "データ確認中" ) ;

				// マニフェスト情報をロードする
				await SetupExternalAssetBundleSettings( owner, false ) ;

				await Progress.OffAsync() ;

				if( AssetBundleManager.HasManifest( manifestName ) == false )
				{
					// ロード出来なかった
					Debug.Log( "Can not load Manifest = " + manifestName ) ;

					return ;
				}
			}

			//----------------------------------------------------------

			// キャッシュで消して良いのはタイトル画面後にダウンロードで取得するもののみ
			// StreamingAssets に配置されたアプリケーション内包アセットバンドルは不可
			AssetBundleManager.Cleanup( manifestName ) ;

			Blocker.Off() ;

			//----------------------------------------------------------

//			Dialog.Open( "キャッシュ消去", "データキャッシュを\n消去しました", new string[]{ "閉じる~0" } ).Forget() ;
			Toast.Show( "データキャッシュを消去しました" ) ;
		}

		//-------------------------------------------------------------------------------------------
	}
}
