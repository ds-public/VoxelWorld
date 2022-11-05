using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using DSW.World ;

namespace DSW.Screens
{
	/// <summary>
	/// ワールドの制御処理
	/// </summary>
	public partial class World
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクション選択
		/// </summary>
		/// <returns></returns>
		private async UniTask<State> State_ActionSelecting( State previous )
		{
			//----------------------------------------------------------
			// ワールドサーバーを起動する

			if( PlayerData.PlayMode == PlayerData.PlayModes.Single )
			{
				// シングルプレイモードのみサーバーを起動する

				WorldServer.Instance.gameObject.SetActive( true ) ;

				var resultCode = await WorldServer.Play() ;

				if( resultCode == WorldServer.ResultCodes.PortNumberAlreadyInUse )
				{
					await Dialog.Open( "エラー", "サーバーの起動に失敗しました\nポート番号(" + PlayerData.ServerPortNumber + ")が既に使用中の可能性があります\n\nタイトル画面に取ります", new string[]{ "タイトル" } ) ;
					ApplicationManager.Reboot() ;
					return State.Unknown ;
				}

				Debug.Log( "<color=#00FF00>[CLIENT] サーバーを起動しました(localhost)</color>" ) ;
			}

			//----------------------------------------------------------
			// クライアントのサーバーへの接続を行う

			m_Client.gameObject.SetActive( true ) ;

			// クライアントの準備を整える
			if( await m_Client.Prepare() == false )
			{
				// ロビーに戻る
				Scene.LoadWithFade( Scene.Screen.Lobby, blockingFadeIn:true ).Forget() ;
				return  State.Unknown ;
			}

			m_CanvasBackground.SetActive( false ) ;

			//----------------------------------------------------------

			// ガイドメッセージの表示設定を行う
			m_Client.SetGuideMessage() ;

			Blocker.Off() ;

			//----------------------------------------------------------
			// ループ

			while( true )
			{
				// クライアントから抜ける要求は発せられたらループを抜ける
				// 注意:IsDisconnected より先に IsQuit の判定をかけないと IsDisconnected が反応してしまう
				if( m_Client.IsQuit == true )
				{
					// 自ら切断した
					break ;
				}

				if( m_Client.IsDisconnected == true )
				{
					// 切断された
					await Dialog.Open( "警告", "サーバーから切断されました", new string[]{ "閉じる" } ) ;
					m_Client.Shutdown() ;
				}

				await Yield() ;
			}

			//----------------------------------------------------------
			// ワールドサーバーを停止する

			Blocker.On() ;

			if( PlayerData.PlayMode == PlayerData.PlayModes.Single )
			{
				// シングルプレイモードのみサーバーを停止する

				// 少し待つ
				await WaitForSeconds( 0.5f ) ;

				WorldServer.Stop() ;

				WorldServer.Instance.gameObject.SetActive( false ) ;
			}

			//----------------------------------------------------------

			// ロビーに戻る
			Scene.LoadWithFade( Scene.Screen.Lobby, blockingFadeIn:true ).Forget() ;
			return  State.Unknown ;
		}
	}
}
