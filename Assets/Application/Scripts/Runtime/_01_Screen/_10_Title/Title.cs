using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using MathHelper ;

namespace DSW.Screens
{
	public partial class Title : ScreenBase
	{
		[SerializeField]
		protected UIImage		m_Screen ;

		[SerializeField]
		protected UIImage		m_Background ;

		[SerializeField]
		protected Sprite[]		m_BackgroundImages = new Sprite[ 4 ] ;

		[SerializeField]
		protected UITextMesh	m_Version ;


		//-------------------------------------------------------------------------------------------

		override protected void  OnAwake()
		{
		}

		override protected async UniTask OnStart()
		{
			m_Screen.IsInteraction = true ;
			m_Screen.SetOnSimpleClick( OnClick ) ;

			//----------------------------------------------------------

			// 背景をランダムに選択する
			int r = m_BackgroundImages.Length ;
			r = Random_XorShift.Get( 0, r - 1 ) ;
			m_Background.Sprite = m_BackgroundImages[ r ] ;

			// バージョン値をロードして設定する
			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_Version.Text = settings.SystemVersionName ;
			}


			// ＢＧＭ再生
			BGM.PlayMain( BGM.Title ) ;

			//----------------------------------------------------------

			// フェードインを許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;
		}

		private void OnClick()
		{
			SE.Play( SE.Decision ) ;

			// ダウンロード後に遷移する画面を指定する
			ToNextScreen( Scene.Screen.Lobby ) ;
		}

		// 次の画面に遷移させる
		private void ToNextScreen( string nextScreenName )
		{
			Blocker.On() ;

			BGM.StopMain( 0.5f ) ;

			//----------------------------------------------------------

			// Phase 2 のダウンロード状態を未状態にする
			ApplicationManager.DownloadingPhase2State = ApplicationManager.DownloadingPhaseStates.None ;

			// ダウンロード Phase をどこまで実行するか指定する →Phase 1・Phase 2のダウンロードを実行してから目的のシーンに飛ぶ(Phase 1のダウンロードが実行済みならスルーする)
			Scene.SetParameter( "DownloadingRequestType",  ApplicationManager.DownloadingRequestTypes.Phase2 ) ;

			// ダウンロード画面経由で指定された画面に遷移する
			Scene.LoadWithFade( Scene.Screen.Downloading, "StartScreenName", nextScreenName, blockingFadeIn:true ).Forget() ;
		}

	}
}
