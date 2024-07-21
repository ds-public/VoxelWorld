using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;


namespace DSW.Screens
{
	/// <summary>
	/// ロビーの制御処理
	/// </summary>
	public partial class Lobby
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクション選択
		/// </summary>
		/// <returns></returns>
		private async UniTask<State> State_ActionSelecting( State previous )
		{
			// 準備
			m_ModeSettingPanel.Prepare( this ) ;

			// フェードイン
			await m_ModeSettingPanel.FadeIn() ;

			Blocker.Off() ;

			// ループ
			( PlayerData.PlayModes playMode, string playerName, byte colorType, string serverAddress, int serverPortNumber )
				= await m_ModeSettingPanel.WaitFor() ;

			// 他の画面への受け渡し用に保存する
			PlayerData.PlayMode			= playMode ;
			PlayerData.PlayerName		= playerName ;
			PlayerData.ColorType		= colorType ;
			PlayerData.ServerAddress	= serverAddress ;
			PlayerData.ServerPortNumber	= serverPortNumber ;


			Debug.Log( "<color=#FF00FF>サーバーの待ち受けポート番号 : " + serverPortNumber + "</color>" ) ;

			// フェードアウト
			await m_ModeSettingPanel.FadeOut() ;

			//----------------------------------------------------------




			//----------------------------------------------------------
			
			// ワールドへ遷移する
			ToWorld() ;

			//----------------------------------

			return State.Unknown ;
		}

		// ワールドへ遷移する
		private void ToWorld()
		{
			Blocker.On() ;

			Scene.LoadWithFade( Scene.Screen.World, blockingFadeIn:true ).Forget() ;
		}

	}
}
