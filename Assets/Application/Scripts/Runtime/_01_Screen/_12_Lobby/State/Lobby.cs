using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using MathHelper ;

using DSW.Screens.LobbyClasses.UI ;

namespace DSW.Screens
{
	public partial class Lobby : ScreenBase
	{
		[Header( "全体" )]

		[SerializeField]
		protected UIImage			m_Screen ;

		[SerializeField]
		protected UIImage			m_Background ;

		// 背景画像
		[SerializeField]
		protected Sprite[]			m_BackgroundImages = new Sprite[ 4 ] ;

		[Header( "モード選択パネル" )]

		[SerializeField]
		protected ModeSettingPanel	m_ModeSettingPanel ;

		//-----------------------------------------------------------

		[Header( "固有ダイアログ" )]

		[SerializeField]
		protected SceneDialogController m_DialogController ;

		/// <summary>
		/// 固有ダイアログコントローラー
		/// </summary>
		public SceneDialogController	DialogController =>m_DialogController ;

		//-------------------------------------------------------------------------------------------

		[Header( "開始ステート" )]

		/// <summary>
		/// 開始ステート
		/// </summary>
		[SerializeField]
		protected State	m_StartupState = State.Auto ;

		/// <summary>
		/// 現在ステート
		/// </summary>
		[SerializeField]
		protected State m_CurrentState = State.Unknown ;

		//-------------------------------------------------------------------------------------------

		override protected void  OnAwake()
		{
			m_ModeSettingPanel.View.SetActive( false ) ;
		}

		override protected async UniTask OnStart()
		{
			//----------------------------------------------------------

			// 最初のステート
			State startupState = m_StartupState ;
			if( Define.DevelopmentMode == false )
			{
				startupState = State.Auto ;
			}

			if( startupState == State.Auto )
			{
				startupState  = State.ActionSelecting ;
			}

			//----------------------------------------------------------


			// 背景をランダムに選択する
			int r = m_BackgroundImages.Length ;
			r = Random_XorShift.Get( 0, r - 1 ) ;
			m_Background.Sprite = m_BackgroundImages[ r ] ;

			// ＢＧＭ再生
			BGM.PlayMain( BGM.Lobby ) ;

			//----------------------------------------------------------

			// フェードインを許可する
			Scene.Ready = true ;

			// フェード完了を待つ
			await Scene.WaitForFading() ;

			//----------------------------------------------------------

			// メイン(ステートマシン)処理実行
			MainLoop( startupState ).Forget() ;
		}

		/// <summary>
		/// メインループ(ステートマシン)処理 ※メソッド名に Main を使うと実行時にエラーがでる(原因不明)
		/// </summary>
		/// <returns></returns>
		private async UniTask MainLoop( State startupState )
		{
			State previous = State.Unknown ;
			State next = startupState ;

			//----------------------------------------------------------

			while( true )
			{
				m_CurrentState = next ;
				switch( m_CurrentState )	// 各略型はC#8.0以降が必要なのでUnity2020を待つべし
				{
					case State.ActionSelecting			: next = await State_ActionSelecting( previous )		; break ;   // アクション選択

					case State.Unknown					:
					default								: next = State.Unknown									; break ;	// 不明なケースでは厩舎終了(保険：通常は Unknown になる事はない)
				}
				previous = m_CurrentState ;

				if( next == State.Unknown )
				{
					break ;	// ここが無いと Unknown になった際に UnityEditor がフリーズするので注意
				}
			}
		}
	}
}
