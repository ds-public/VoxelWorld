using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using Cysharp.Threading.Tasks ;

namespace DBS.Screens.DownloadingClasses.UI
{
	/// <summary>
	/// ロア表示パネル
	/// </summary>
	public partial class LorePanel : ExMonoBehaviour
	{
		private UIView				m_View ;

		/// <summary>
		/// パネルのビューを取得する
		/// </summary>
		public	UIView				  View
		{
			get
			{
				if( m_View == null )
				{
					m_View  = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}

		//-----------------------------------------------------------

		[SerializeField]
		protected UIView			m_InteractionPlane ;

		[SerializeField]
		protected UITextMesh		m_Title ;

		[SerializeField]
		protected UITextMesh		m_Description ;

		[SerializeField]
		protected UIView			m_PageButtonBase ;

		[SerializeField]
		protected UIButton			m_BackButton ;

		[SerializeField]
		protected UIButton			m_NextButton ;

		[SerializeField]
		protected UIView			m_PagingDotBase ;

		[SerializeField]
		protected UIImage			m_PagingDot_Template ;

		//-------------------------------------------------------------------------------------------

		private List<Downloading.LoreStructure>	m_Lores ;

		private int							m_PageOffset ;
		private int							m_PageLength ;

		private List<UIImage>				m_PagingDots ;

		private float						m_LastUpdateAt ;
		private const float					m_IntervalTime = 5 ;

		private bool						m_Busy ;

		//-------------------------------------------------------------------------------------------

		// 画面生成を行う
		public void Prepare( List<Downloading.LoreStructure> lores )
		{
			// 情報を取得する

			if( lores == null || lores.Count == 0 )
			{
				// ページ情報が取得できない(異常)
				return ;
			}

			m_Lores = lores ;

			m_LastUpdateAt	= 0 ;
			m_Busy			= false ;

			//----------------------------------------------------------

			// 最初の表示ページ状態
			m_PageOffset = 0 ;
			m_PageLength = m_Lores.Count ;

			if( m_PageLength == 1 )
			{
				// ページ選択できない

				//---------------------------------

//				m_PagingDotBase.SetActive( false ) ;
//				m_PageButtonBase.SetActive( false ) ;

				//---------------------------------
				// それでもドットは表示する

				m_PagingDotBase.SetActive( true ) ;
				m_PageButtonBase.SetActive( true ) ;

				int i, l ;

				// 展開済みのページドットを破棄する
				Terminate() ;

				// ページドットを展開する
				m_PagingDot_Template.SetActive( false ) ;

				m_PagingDots = new List<UIImage>() ;

				l = m_PageLength ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var pagingDot = m_PagingDotBase.AddPrefab<UIImage>( m_PagingDot_Template.gameObject ) ;
					pagingDot.SetActive( true ) ;

					m_PagingDots.Add( pagingDot ) ;
				}

				SetPageState( m_PageOffset ) ;

				//---------------------------------
				// コールバックを設定する

				m_BackButton.SetOnButtonClick( null ) ;
				m_NextButton.SetOnButtonClick( null ) ;

				m_InteractionPlane.SetOnFlick( null ) ;
			}
			else
			{
				// ページ選択できる

				//---------------------------------

				m_PagingDotBase.SetActive( true ) ;
				m_PageButtonBase.SetActive( true ) ;

				int i, l ;

				// 展開済みのページドットを破棄する
				Terminate() ;

				// ページドットを展開する
				m_PagingDot_Template.SetActive( false ) ;

				m_PagingDots = new List<UIImage>() ;

				l = m_PageLength ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					var pagingDot = m_PagingDotBase.AddPrefab<UIImage>( m_PagingDot_Template.gameObject ) ;
					pagingDot.SetActive( true ) ;

					m_PagingDots.Add( pagingDot ) ;
				}

				SetPageState( m_PageOffset ) ;

				//---------------------------------
				// コールバックを設定する(ページが切り替わる時にブロッカーで画面入力を一度遮断しているためリピート入力は正常に動かない)

				m_BackButton.SetOnButtonClick( ( string identity, UIButton button ) =>
				{
					SE.Play( SE.Selection ) ;

					ChangePage( -1 ) ;
				} ) ;

				m_NextButton.SetOnButtonClick( ( string identity, UIButton button ) =>
				{
					SE.Play( SE.Selection ) ;

					ChangePage( +1 ) ;
				} ) ;

				// フリック
				m_InteractionPlane.SetOnFlick( ( string identity, UIView view, Vector2 start, Vector2 end ) =>
				{
					Vector2 distance = end - start ;

					if( distance.x >  +120 )
					{
						ChangePage( -1 ) ;
					}
					else
					if( distance.x <  -120 )
					{
						ChangePage( +1 ) ;
					}
				} ) ;
			}

			//----------------------------------
			// 最初のページをセットする

			SetDescription( m_PageOffset ) ;
		}

		// 指定したプレートに指定したページ情報を展開する
		private void SetDescription( int pageOffset )
		{
			m_Title.Text		= m_Lores[ pageOffset ].Title ;
			m_Description.Text	= m_Lores[ pageOffset ].Description ;

			// 最終更新時間を更新する
			m_LastUpdateAt = Time.realtimeSinceStartup ;
		}

		// ページ変更処理
		private void ChangePage( int direction )
		{
			if( m_Busy == true )
			{
				return ;
			}

			//----------------------------------

			m_Busy = true ;

			if( direction <  0 )
			{
				// 戻る
				m_PageOffset -- ;
				if( m_PageOffset <  0 )
				{
					m_PageOffset += m_PageLength ;
				}

				SetDescription( m_PageOffset ) ;

				//---------------------------------

				SetPageState( m_PageOffset ) ;
			}
			else
			if( direction >  0 )
			{
				// 進む
				m_PageOffset ++ ;
				if( m_PageOffset == m_PageLength )
				{
					m_PageOffset = 0 ;
				}

				SetDescription( m_PageOffset ) ;

				//---------------------------------

				SetPageState( m_PageOffset ) ;
			}

			m_Busy = false ;
		}


		// 何ページかを表す部分を更新する
		private void SetPageState( int pageOffset )
		{
			int i, l = m_PageLength ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( i == pageOffset )
				{
					// ページ表示中
					m_PagingDots[ i ].SetSpriteInAtlas( "Type2_On" ) ;
				}
				else
				{
					m_PagingDots[ i ].SetSpriteInAtlas( "Type1_Off" ) ;
				}
			}

			m_BackButton.Interactable = m_PageLength >  1 ;
			m_NextButton.Interactable = m_PageLength >  1 ;
		}

		// 展開した不要なものを破棄する
		public void Terminate()
		{
			int i, l ;

			// 展開済みのページドットを破棄する
			if( m_PagingDots != null )
			{
				l = m_PagingDots.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_PagingDots[ i ] != null )
					{
						DestroyImmediate( m_PagingDots[ i ].gameObject ) ;
						m_PagingDots[ i ] = null ;
					}
				}
				m_PagingDots = null ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// 一定時間放置している場合は勝手にページを進ませる

		internal void Update()
		{
			if( m_LastUpdateAt >  0 && m_Busy == false )
			{
				float lastUpdateAt = Time.realtimeSinceStartup ;

				if( ( lastUpdateAt - m_LastUpdateAt ) >  m_IntervalTime )
				{
					// ページを変更する
					ChangePage( +1 ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			if( View.ActiveSelf == true )
			{
				return ;
			}

			await When( View.PlayTween( "FadeIn" ) ) ;
		}

		/// <summary>
		/// フェードアウト
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			await When( View.PlayTweenAndHide( "FadeOut" ) ) ;
		}
	}
}

