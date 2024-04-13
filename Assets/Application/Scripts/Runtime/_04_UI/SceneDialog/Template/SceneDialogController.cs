using System ;
using System.Collections.Generic ;
using UnityEngine ;

using DSW.UI ;


// 注意
// #region SAMPLE 内は、実装の際は削除してください

namespace DSW.Screens.TemplateClasses.UI
{
	/// <summary>
	/// シーン固有ダイアログの全体管理クラス
	/// </summary>
	public class SceneDialogController : SceneDialogControllerBase
	{
		//-----------------------------------

		[Header( "シーン固有ダイアログ群" )]

#region SAMPLE

		[SerializeField]
		protected   SampleDialog			m_SampleDialog ;
		/// <summary>
		/// サンプルダイアログ
		/// </summary>
		public		SampleDialog			  SampleDialog => m_SampleDialog ;

#endregion

		//-----------------------------------------------------------

		/// <summary>
		/// 固有ダイアログ群を登録する
		/// </summary>
		/// <param name="dialogs"></param>
		protected override void RegisterDialog( in List<SceneDialogBase> dialogs )
		{
#region SAMPLE
			// ここに固有ダイアログ群の登録を記述してください

			dialogs.Add( m_SampleDialog ) ;

#endregion
		}
	}
}


