using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DBS.World
{
	/// <summary>
	/// ネームプレートの表示制御
	/// </summary>
	public class NamePlate : ExMonoBehaviour
	{
		private UIView	m_View ;
		public  UIView    View
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


		[SerializeField]
		protected UITextMesh	m_PlayerName ;

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 表示の有無を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			View.SetActive( state ) ;
		}

		/// <summary>
		/// プレイヤー名を設定する
		/// </summary>
		/// <param name="playerName"></param>
		public void SetPlayerName( string playerName )
		{
			m_PlayerName.Text = playerName ;

			float tw = m_PlayerName.TextWidth ;

			View.Width = tw + 32 ;
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="position"></param>
		public void SetPosition( Vector2 position )
		{
			View.SetPosition( position ) ;
		}
	}
}
