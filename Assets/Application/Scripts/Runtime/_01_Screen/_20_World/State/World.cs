using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.SceneManagement ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

using DSW.World ;

namespace DSW.Screens
{
	/// <summary>
	/// クライアント(メイン)
	/// </summary>
	public partial class World : ScreenBase
	{
		[SerializeField]
		protected UICanvas		m_CanvasBackground ;

		[SerializeField]
		protected WorldClient	m_Client ;

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

		override protected void OnAwake()
		{
			// 最初に見えてはいけないものを非表示にする

			m_CanvasBackground.SetActive( true ) ;
			m_Client.gameObject.SetActive( false ) ;
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

			//----------------------------------
			// ＢＧＭ再生

			await BGM.PlayMainAsync( BGM.World ) ;

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
