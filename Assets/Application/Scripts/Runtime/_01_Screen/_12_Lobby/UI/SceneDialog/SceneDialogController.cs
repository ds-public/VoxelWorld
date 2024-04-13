using System ;
using System.Collections.Generic ;
using UnityEngine ;

using DSW.UI ;


namespace DSW.Screens.LobbyClasses.UI
{
	/// <summary>
	/// シーン固有ダイアログの全体管理クラス
	/// </summary>
	public class SceneDialogController : SceneDialogControllerBase
	{
		//-----------------------------------

		[Header( "シーン固有ダイアログ群" )]

		[SerializeField]
		protected   AddressSelectionDialog		m_AddressSelectionDialog ;
		/// <summary>
		/// アドレス選択ダイアログ
		/// </summary>
		public		AddressSelectionDialog		  AddressSelectionDialog => m_AddressSelectionDialog ;

		[SerializeField]
		protected   AddressInputDialog			m_AddressInputDialog ;
		/// <summary>
		/// アドレス入力ダイアログ
		/// </summary>
		public		AddressInputDialog			  AddressInputDialog => m_AddressInputDialog ;

		//-----------------------------------------------------------

		/// <summary>
		/// 固有ダイアログ群を登録する
		/// </summary>
		/// <param name="dialogs"></param>
		protected override void RegisterDialog( in List<SceneDialogBase> dialogs )
		{
			dialogs.Add( m_AddressSelectionDialog ) ;
			dialogs.Add( m_AddressInputDialog ) ;
		}
	}
}


