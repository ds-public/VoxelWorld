using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;
using AudioHelper ;

namespace DSW.Screens
{
	/// <summary>
	/// 起動画面の制御クラス Version 2022/09/19 0
	/// </summary>
	public class Boot : ScreenBase
	{
		[SerializeField]
		protected UIImage		m_Screen ;

		//-----------------------------------------------------------

		/// <summary>
		/// 画面種別
		/// </summary>
		public enum ScreenCodes
		{
			Title,
			World,
		} ;

		/// <summary>
		/// 最初の遷移画面
		/// </summary>
		[SerializeField]
		protected ScreenCodes m_FirstScreenCode = ScreenCodes.Title ;	// SerializeField が付いているものは readonly を付けてはだめ 

		/// <summary>
		/// 画面情報クラス
		/// </summary>
		[Serializable]
		public class ScreenDescriptor
		{
			public string		name ;
			public ScreenCodes	ScreenCode ;

			public ScreenDescriptor( ScreenCodes screenCode, string screenName )
			{
				ScreenCode = screenCode ;
				name = screenName ;
			}
		}

		/// <summary>
		/// 画面情報リスト
		/// </summary>
		[SerializeField]
		protected List<ScreenDescriptor>	m_ScreenDescriptors = new List<ScreenDescriptor>() ;	// readonly は付けてはだめ

		/// <summary>
		/// 画面情報リストを更新する(画面が増えたらここにコードに追加すること)
		/// </summary>
		public void Refresh()
		{
			m_ScreenDescriptors = new List<ScreenDescriptor>()		// readonly は付けてはだめ
			{
				new ScreenDescriptor( ScreenCodes.Title,	Scene.Screen.Title		),
				new ScreenDescriptor( ScreenCodes.World,	Scene.Screen.World		),
			} ;
		}

		//-------------------------------------------------------------------------------------------

		override protected void OnAwake()
		{
			// リスト更新
			Refresh() ;
		}

		override protected async UniTask OnStart()
		{
#if false
			// ウェブビューの表示テスト
			bool isVisible = true ;
			WebView.Open( "https://www.google.co.jp", ( webViewObject, text ) =>
			{
				isVisible = false ;
			}, m_Screen ) ;
			await WaitWhile( () => isVisible ) ;
#endif
			//----------------------------------------------------------

			// 準備処理
			Prepare() ;

			//----------------------------------------------------------
			// 画面が暗転中にこの画面の準備を整える

			string startScreenName = Scene.Screen.Title ;

			var startScreen = m_ScreenDescriptors.FirstOrDefault( _ => _.ScreenCode == m_FirstScreenCode ) ;
			if( startScreenName != null )
			{
				startScreenName = startScreen.name ;
			}

			//----------------------------------

			// 最初のシーンへ遷移する
			ToStartScreen( startScreenName ) ;

			await Yield() ;
		}

		//-------------------------------------------------------------------------------------

		// ダウンロードシーン経由で最初のシーンへ遷移する
		private void ToStartScreen( string startScreenName )
		{
			ApplicationManager.DownloadingRequestTypes downloadingRequestType = ApplicationManager.DownloadingRequestTypes.Phase2 ;

			if( startScreenName == Scene.Screen.Title )
			{
				// タイトルのみフェーズ１のダウンロードを行う
				downloadingRequestType = ApplicationManager.DownloadingRequestTypes.Phase1 ;
			}

			// ダウンロードのフェーズを保存する
			Scene.SetParameter( "DownloadingRequestType", downloadingRequestType ) ;

			// ダウンロード画面経由で選択された画面に遷移する
			Scene.LoadWithFade( Scene.Screen.Downloading, "StartScreenName", startScreenName, blockingFadeIn:true ).Forget() ;
		}

		/// <summary>
		/// 各種準備
		/// </summary>
		private void Prepare()
		{
			// ログアウト状態にする
//			PlayerDataManager.LogOut() ;

			// ダウンロードの各フェーズの状態をクリアする
			ApplicationManager.DownloadingPhase1State = ApplicationManager.DownloadingPhaseStates.None ;
			ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.None ;
		}
	}
}
