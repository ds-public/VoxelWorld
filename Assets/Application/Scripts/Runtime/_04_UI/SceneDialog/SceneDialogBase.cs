using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

using Cysharp.Threading.Tasks ;

namespace DSW.UI
{
	/// <summary>
	/// シーン固有のダイアログの基底クラス
	/// </summary>
	public class SceneDialogBase : ExMonoBehaviour
	{
		private	UIView			m_View ;
		public	UIView			  View
		{
			get
			{
				if( m_View == null )
				{
					m_View = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}

		//-----------------------------------

		[Header( "ダイアログ共通ＵＩ" )]

		[SerializeField]
		protected UIView		m_Fade ;

		[SerializeField]
		protected UIImage		m_Window ;

		[SerializeField]
		protected UIButton		m_CloseButton ;

		//-------------------------------------------------------------------------------------------

		// ダイアログコントローラー
		protected SceneDialogControllerBase		m_DialogController ;

		/// <summary>
		/// オーナー(ダイアログコントローラー)を設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner( SceneDialogControllerBase dialogController )
		{
			m_DialogController = dialogController ;
		}

		//-------------------------------------------------------------------------------------------

		// 閉じる前に確認が必要な場合のコールバック
		private Action<Action>	m_OnCloseConfirm ;

		// 閉じられた際に呼び出されるコールバック
		private Action			m_OnClosed ;

		// 外側タッチでダイアログを閉じるかどうか
		private bool			m_OutsideEnabled ;

		// 最初からタッチ状態かの判定に利用するフラグ
		private bool			m_IsPressing ;

		//-------------------------------------------------------------------------------------------

		// 閉じるボタンを押した際に自動でウィンドウのフェードアウトを行わないようにするかどうか
		private bool			m_FadeOutDisabled = false ;

		//-----------------------------------

		// SE 再生を行わないかどうか
		private bool			m_IsNotSe ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			if( m_Fade != null )
			{
				m_Fade.SetActive( false ) ;
			}
			m_Window.SetActive( false ) ;
		}

		/// <summary>
		/// ダイアログが開かれているか確認する
		/// </summary>
		public bool IsOpening
		{
			get
			{
				return View.ActiveSelf ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// ダイアログを開く際に呼ぶ
		protected async UniTask OpenBase( Action<bool> onOpen, Action onClosed, Action<Action> onCloseConfirm = null, bool outsideEnabled = true, bool fadeOutDisabled = false, bool isNotSe = false )
		{
			// 閉じる前に確認が必要な場合のコールバック
			m_OnCloseConfirm	= onCloseConfirm ;

			// 閉じられた際に呼び出されるコールバック
			m_OnClosed			= onClosed ;

			// 外側タッチでダイアログを閉じるかどうか
			m_OutsideEnabled	= outsideEnabled ;

			// 自動フェードアウトを無効にするか
			m_FadeOutDisabled	= fadeOutDisabled ;

			// SE 再生を行わないかどうか
			m_IsNotSe			= isNotSe ;

			//----------------------------------

			// ウィンドウが非表示状態ならフェードインで表示する
			if( View.ActiveSelf == false )
			{
				await FadeIn( onOpen ) ;
			}

			//----------------------------------

			// 閉じるボタンのコールバックを設定する
			m_CloseButton.SetOnSimpleClick( () =>
			{
				if( m_IsNotSe == false )
				{
					SE.Play( SE.Cancel ) ;
				}

				if( m_OnCloseConfirm != null )
				{
					m_OnCloseConfirm( OnCloseConfirm ) ;
				}
				else
				{
					if( m_FadeOutDisabled == false )
					{
						FadeOut().Forget() ;
					}
					else
					{
						m_OnClosed?.Invoke() ;
					}
				}
			} ) ;
		}

		// 閉じる前に確認が必要な際に閉じる事が問題無い場合に呼んでもらう
		private	void OnCloseConfirm()
		{
			// 閉じる
			if( m_FadeOutDisabled == false )
			{
				FadeOut().Forget() ;
			}
			else
			{
				m_OnClosed?.Invoke() ;
			}
		}

		/// <summary>
		/// ダイアログを閉じる(外部からの呼び出し専用)
		/// </summary>
		/// <returns></returns>
		public async UniTask Close()
		{
			if( View.ActiveSelf == false )
			{
				// 表示れていない
				return ;
			}

			// コールバックは発生させない
			m_OnClosed = null ;

			await FadeOut() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フェードインでダイアログを表示する
		/// </summary>
		/// <returns></returns>
		private async UniTask FadeIn( Action<bool> onOpen )
		{
			// コントローラー全体の開始を行う
			bool isControllerAwaked = m_DialogController.StartupController() ;

			//----------------------------------

			Blocker.On() ;

			//----------------------------------------------------------

			View.SetActive( true ) ;

			if( m_Fade != null )
			{
				m_Fade.SetActive( true ) ;

				// ノッチ(セーフエリフ)対策
				m_Fade.SetMarginY( -960, -960 ) ;
			}

			m_Window.SetActive( true ) ;

			// ウィンドウが開く前に後に呼ぶ
			onOpen?.Invoke( false ) ;

			await WhenAll
			(
				m_Fade != null ? m_Fade.PlayTween( "FadeIn" ) : null,
				m_Window.PlayTween( "FadeIn" )
			) ;

			// ウィンドウが開ききった後に呼ぶ
			onOpen?.Invoke( true ) ;

			//----------------------------------------------------------
			// 外側タッチで閉じる処理

			// ウィンドウ部分はタッチしても無効化するためレイキャストを有効にする
			m_Window.RaycastTarget = true ;

			//--------------

			// 最初からタッチしていた場合は一度タッチを解除する必要がありそのためのフラグ
			m_IsPressing = Input.GetMouseButton( 0 );

			if( m_Fade != null )
			{
				m_Fade.IsInteraction = true ;
				m_Fade.RaycastTarget = true ;
				m_Fade.SetOnSimplePress( ( bool state ) =>
				{
					if( m_OutsideEnabled == true )
					{
						if( state == true  )
						{
							if( m_IsPressing == false )
							{
								// 外側を触れたら閉じる扱いとする

								if( m_IsNotSe == false )
								{
									SE.Play( SE.Cancel ) ;
								}

								if( m_OnCloseConfirm != null )
								{
									m_OnCloseConfirm( OnCloseConfirm ) ;
								}
								else
								{
									if( m_FadeOutDisabled == false )
									{
										FadeOut().Forget() ;
									}
									else
									{
										m_OnClosed?.Invoke() ;
									}
								}

								m_IsPressing = true ;
							}
						}
						else
						{
							m_IsPressing = false ;
						}
					}
				} ) ;
			}

			//----------------------------------

			Blocker.Off() ;
		}

		/// <summary>
		/// フェードアウトでダイアログを隠蔽する
		/// </summary>
		/// <returns></returns>
		private async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			//----------------------------------------------------------

			Blocker.On() ;

			//----------------------------------

			// 不要なコールバックを解除する
			if( m_Fade != null )
			{
				m_Fade.SetOnPress( null ) ;
				m_Fade.RaycastTarget = false ;
				m_Fade.IsInteraction = false ;
			}

			//----------------------------------------------------------
			// フェードアウトを行う

			await WhenAll
			(
				m_Fade != null ? m_Fade.PlayTweenAndHide( "FadeOut" ) : null,
				m_Window.PlayTweenAndHide( "FadeOut" )
			) ;

			if( m_Fade != null )
			{
				m_Fade.SetActive( false ) ;
			}

			m_OnClosed?.Invoke() ;

			View.SetActive( false ) ;

			//----------------------------------------------------------

			m_OnClosed = null ;

			// コントローラー全体の終了を行う
			m_DialogController.CleanupController() ;

			//----------------------------------

			Blocker.Off() ;
		}
	}
}
