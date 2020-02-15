using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DBS.DialogStyle
{
	/// <summary>
	/// ダイアログスタイルの基底クラス
	/// </summary>
	public class DialogStyleBase : MonoBehaviour
	{
		// 共通に存在するプロパティ

		/// <summary>
		/// フェード用のコンポーネントのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage	m_Fade = null ;

		/// <summary>
		/// ウィンドウ用のコンポーネントのインスタンス
		/// </summary>
		[SerializeField]
		protected UIImage	m_Window = null ;

		/// <summary>
		/// パディング
		/// </summary>
		public RectOffset	padding = new RectOffset() ;

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンのパディング
		/// </summary>
		public RectOffset buttonPadding = new RectOffset() ;

		/// <summary>
		/// ボタンの最低サイズ
		/// </summary>
		public Vector2 minimunButtonSize = new Vector2( 120,  48 ) ;

		/// <summary>
		/// ボタンの最大サイズ
		/// </summary>
		public Vector2 maximumButtonSize = new Vector2( 240,  48 ) ;

		// タイトルとメッセージの有無で縦方向の長さを自動調整する
		// ボタンの最低横幅は決まっているがボタンのラベルによって自動的に広がる

		//-----------------------------------------------------------

		/// <summary>
		/// 状態に変化があったかどうか
		/// </summary>
		protected bool m_Dirty = false ;

		/// <summary>
		/// 最終的なコールバック呼び出しで渡される結果文字列
		/// </summary>
		protected int m_Result = -1 ;	// 結果

		/// <summary>
		/// アクティブと非アクティブ状態を設定する
		/// </summary>
		/// <param name="tState"></param>
		public void SetActive( bool tState )
		{
			gameObject.SetActive( tState ) ;
		}

		/// <summary>
		/// フェードインで表示する
		/// </summary>
		virtual public IEnumerator FadeIn()
		{
			m_Fade.PlayTween( "FadeIn" ) ;
			m_Window.PlayTween( "FadeIn" ) ;

			// フェードイン効果が終了するのを待つ
			yield return new WaitWhile( () => ( m_Fade.IsAnyTweenPlaying | m_Window.IsAnyTweenPlaying ) ) ;

			Visible() ;
		}

		/// <summary>
		/// ＵＩの状態を完成させる
		/// </summary>
		virtual public void Commit(){}

		/// <summary>
		/// 準備を行う
		/// </summary>
		virtual public void Prepare()
		{
			m_Dirty = true ;
		}

		/// <summary>
		/// ダイアログが表示されたら呼び正される
		/// </summary>
		virtual protected void Visible(){}

		void Start()
		{
			Commit() ;	// コミット忘れようの対策
		}
	
		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <param name="tResult">ダイアログが閉じられた後に呼び出すコールバックに渡す文字列</param>
		virtual public void Close( int tResult = -1 )
		{
			m_Result = tResult ;
			StartCoroutine( FadeOut() ) ;
		}

		/// <summary>
		/// ダイアログをフォードアウト効果付きで非表示にして最後にクローズ時のデリゲードを呼び出す
		/// </summary>
		/// <returns>列挙子</returns>
		virtual protected IEnumerator FadeOut()
		{
			m_Fade.PlayTween( "FadeOut" ) ;
			m_Window.PlayTween( "FadeOut" ) ;

			// フェードアウト効果が終了するのを待つ
			yield return new WaitWhile( () => ( m_Fade.IsAnyTweenPlaying | m_Window.IsAnyTweenPlaying ) ) ;

			m_Fade.SetActive( false ) ;
			m_Window.SetActive( false ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 必要に応じてボタンをリサイズする
		protected void ResizeButton( UIButton tButton )
		{
			float bw, bh, tw, th ;

			if( tButton.Label == null || string.IsNullOrEmpty( tButton.Label.Text ) == true )
			{
				return ;	// 文字列が存在しない場合はリサイズしない
			}

			// 横幅のチェック
			bw = tButton.Width ;
			tw = tButton.Label.TextWidth + buttonPadding.horizontal ;
			if( tw >  bw )
			{
				// 文字に対して現在のボタンの横幅が小さすぎる
				bw  = tw ;
					
				if( bw >  maximumButtonSize.x )
				{
					bw  = maximumButtonSize.x ;	// 上限を突破するので上限に合わせる
				}
			}
			if( bw <  minimunButtonSize.x )
			{
				bw  = minimunButtonSize.x ;	// 下限を突破するので下限に合わせる
			}

			// 縦幅のチェック
			bh = tButton.Height ;
			th = tButton.Label.TextHeight + buttonPadding.vertical ;
			if( th >  bh )
			{
				// 文字に対して現在のボタンの縦幅が小さすぎる
				bh  = th ;
					
				if( bh >  maximumButtonSize.y )
				{
					bh  = maximumButtonSize.y ;	// 上限を突破するので上限に合わせる
				}
			}
			if( bh <  minimunButtonSize.y )
			{
				bh  = minimunButtonSize.y ;	// 下限を突破するので下限に合わせる
			}

			tButton.SetSize( bw, bh ) ;
		}
	}
}
