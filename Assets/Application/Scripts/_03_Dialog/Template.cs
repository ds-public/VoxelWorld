using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using uGUIHelper ;
using InputHelper ;

namespace DBS.nDialog
{
	/// <summary>
	/// テンプレートダイアログのコントロールクラス
	/// </summary>
	public class Template : DialogBase
	{
		//-----------------------------------------------------------
		// 量産ダイアログ固有の情報

		[SerializeField]
		protected UITextMesh	m_Title ;			// 固有ダイアログのタイトル部

		[SerializeField]
		protected UITextMesh	m_Message ;			// 固有ダイアログのメッセージ部

		private Action<int>		m_OnClosed ;		// 固有ダイアログが閉じられた際に呼び出すコールバックメソッド

		//-------------------------------------------------------------------------------------------

		// ダイアログシーンの名前を設定する
		protected override string SetDialogSceneName()
		{
			return Scene.Dialog.Template ;
		}

		// 単体デバッグを実行する
		protected override IEnumerator RunDebug()
		{
			Open( "タイトル", "メッセージ", new string[]{ "YES", "NO" }, null ) ;

			yield return null ;
		}
		
		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns>The open.</returns>
		/// <param name="title">Title.</param>
		/// <param name="message">Message.</param>
		/// <param name="selectionButtonLabels">Selection button labels.</param>
		/// <param name="onClosed">On closed.</param>
		public Dialog.State Open( string title, string message, string[] selectionButtonLabels, Action<int> onClosed )
		{
			m_Title.Text	= title ;		// タイトル文字列を設定する
			m_Message.Text	= message ;	// メッセージ文字列を設定する

			//----------------------------------------------------------

			// ボタン周りの準備を行う
			UseStandardButton
			(
				selectionButtonLabels, 0,
				( int cIndex ) =>
				{
					// ボタンが押された
					Close( cIndex ) ;	// ダイアログを閉じる
				}
			) ;

			//----------------------------------------------------------

			m_OnClosed		= onClosed ;	// コールバックメソッドを保存する

			//----------------------------------------------------------

			// ダイアログを開く
			return base.OpenBase() ;
		}

		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <param name="result">Result.</param>
		public void Close( int result )
		{
			StartCoroutine( base.CloseBase
			(
				() =>
				{
					m_OnClosed?.Invoke( result ) ;
				}
			) ) ;
		}
	}
}

