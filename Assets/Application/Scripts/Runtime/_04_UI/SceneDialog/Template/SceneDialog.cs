using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using DSW.UI ;

namespace DSW.Screens.TemplateClasses.UI
{
	/// <summary>
	/// サンプルダイアログのクラス
	/// </summary>
	public class SampleDialog : SceneDialogBase
	{
		//-------------------------------------------------------------------------------------------
		// このダイアログ固有のＵＩの定義

		//-------------------------------------------------------------------------------------------

		// 閉じるボタンが押されたかどうか
		private bool			m_IsClosed ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns></returns>
		public async UniTask Open()
		{
			// ここにダイアログ固有の処理を記述してください

			//----------------------------------------------------------

			// 閉じるボタンが押されたかどうかのフラグをクリアする
			m_IsClosed = false ;

			// ダイアログを開く
			await OpenBase( null, () =>
			{
				// ダイアログが閉じた後に呼び出される
				m_IsClosed = true ;	// 閉じるボタンがされた
			} ) ;

			// 何らかのアクション(閉じるボタンなど)が実行されるのを待つ
			await WaitUntil( () => ( m_IsClosed == true ) ) ;

			// ダイアログが閉じられた
		}
	}
}
