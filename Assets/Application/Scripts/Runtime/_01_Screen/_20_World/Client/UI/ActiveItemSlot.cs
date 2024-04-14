using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DSW.World
{
	/// <summary>
	/// アクティブなアイテムの表示制御
	/// </summary>
	public class ActiveItemSlot : ExMonoBehaviour
	{
		[SerializeField]
		protected UIButton  m_Button ;

		[SerializeField]
		protected UIImage   m_Cursor ;

		private int m_Index ;
		private Action<int>     m_OnSelected ;

		//-------------------------------------------------------------------------------------

		public void Setup( int index, Action<int> onSelected )
		{
			m_Index         = index ;
			m_OnSelected    = onSelected ;
				
			m_Button.SetOnButtonClick
			(
				( string identity, UIButton button ) =>
				{
					m_OnSelected?.Invoke( m_Index ) ;
				}
			) ;
		}

		public void SetCursor( bool state )
		{
			m_Cursor.SetActive( state ) ;
		}
	}
}
