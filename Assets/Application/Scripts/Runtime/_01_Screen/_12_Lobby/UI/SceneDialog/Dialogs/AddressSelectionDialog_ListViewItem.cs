using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using DSW.UI ;

namespace DSW.Screens.LobbyClasses.UI
{
	public class AddressSelectionDialog_ListViewItem : ExMonoBehaviour
	{
		private		UIImage			m_View ;
		protected	UIImage			  View
		{
			get
			{
				if( m_View == null )
				{
					m_View = GetComponent<UIImage>() ;
				}
				return m_View ;
			}
		}

		//-----------------------------------------------------------

		[SerializeField]
		protected UITextMesh	m_ServerName ;

		[SerializeField]
		protected UITextMesh	m_ServerAddress ;

		//-----------------------------------------------------------

		private int				m_Index ;
		private Action<int>		m_OnSelected ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 表示スタイルを設定する
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="serverAddress"></param>
		/// <param name="index"></param>
		/// <param name="onSelected"></param>
		public void SetStyle( string serverName, string serverAddress, int index, Action<int> onSelected )
		{
			m_ServerName.Text		= serverName ;
			m_ServerAddress.Text	= serverAddress ;

			m_Index					= index ;
			m_OnSelected			= onSelected ;

			//----------------------------------
			// クリック時のコールバックを設定する

			View.RaycastTarget = true ;
			View.IsInteractionForScrollView = true ;

			View.SetOnSimpleClick( () =>
			{
//				SE.Play( SE.Decision ) ;
				m_OnSelected?.Invoke( m_Index ) ;
			} ) ;
		}
	}
}

