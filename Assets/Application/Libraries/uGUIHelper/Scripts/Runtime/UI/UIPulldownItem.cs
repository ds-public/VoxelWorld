using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using TMPro ;


namespace uGUIHelper
{
	/// <summary>
	/// プルダウンリストのアイテム
	/// </summary>
	public class UIPulldownItem : MonoBehaviour
	{
		protected UIImage		m_View ;

		/// <summary>
		/// 自身のビュー
		/// </summary>
		public    UIView		  View
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


		[SerializeField]
		protected UIImage		m_Highlight ;

		/// <summary>
		/// Pointer 用カーソル
		/// </summary>
		public UIImage			  Highlight
		{
			get
			{
				return m_Highlight ;
			}
			set
			{
				m_Highlight = value ;
			}
		}

		[SerializeField]
		protected UIImage		m_PadCursor ;

		/// <summary>
		/// GamePad 用カーソル
		/// </summary>
		public UIImage			  PadCursor
		{
			get
			{
				return m_PadCursor ;
			}
			set
			{
				m_PadCursor = value ;
			}
		}


		[SerializeField]
		protected UIToggle		m_Checkmark ;

		/// <summary>
		/// チェック
		/// </summary>
		public UIToggle			  Checkmark
		{
			get
			{
				return m_Checkmark ;
			}
			set
			{
				m_Checkmark = value ;
			}
		}

		[SerializeField]
		protected UITextMesh	m_CaptionText ;

		/// <summary>
		/// ラベル
		/// </summary>
		public UITextMesh		  CaptionText
		{
			get
			{
				return m_CaptionText ;
			}
			set
			{
				m_CaptionText = value ;
			}
		}

		[SerializeField]
		protected UIImage		m_CaptionImage ;

		/// <summary>
		/// サムネイル
		/// </summary>
		public UIImage			  CaptionImage
		{
			get
			{
				return m_CaptionImage ;
			}
			set
			{
				m_CaptionImage = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		protected int			m_Index ;
		protected Action<int>	m_OnItemSelected ;
		protected Action<int>	m_OnItemEntering ;

		protected bool			m_PointerModeEnabled ;
		protected bool			m_IsHover ;

		//-------------------------------------------------------------------------------------------

		internal void OnEnable()
		{
			m_IsHover = View.IsHover ;

			if( m_Highlight != null )
			{
				m_Highlight.SetActive( m_IsHover && m_PointerModeEnabled ) ;
			}
		}

		/// <summary>
		/// プルダウンアイテムの表示を更新する
		/// </summary>
		/// <param name="pulldownItem"></param>
		public virtual void SetStyle
		(
			int index,
			UIPulldown.PulldownItem pulldownItem,
			bool pointerModeEnabled,
			bool isPadCursorVisible,
			bool isCheck,
			Action<int> onItemSelected,
			Action<int> onItemEntering
		)
		{
			View.name = $"Item[{index}] - {pulldownItem.Text}" ;

			m_Index				= index ;
			m_OnItemSelected	= onItemSelected ;
			m_OnItemEntering	= onItemEntering ;

			// Pointer モードになっているか
			m_PointerModeEnabled	= pointerModeEnabled ;

			//----------------------------------------------------------

			View.RaycastTarget = true ;
			View.IsInteractionForScrollView = true ;

			View.SetOnSimpleClick( () =>
			{
				m_OnItemSelected?.Invoke( m_Index ) ;
			} ) ;

			View.SetOnSimpleHover( ( bool isHover ) =>
			{
				if( isHover == true )
				{
//					Debug.Log( "<color=#00FFFF>入りました : " + name + "</color>" ) ;
					if( m_Highlight != null )
					{
						m_Highlight.SetActive( m_PointerModeEnabled ) ;
					}
					m_IsHover = true ;

					if( m_PointerModeEnabled == true )
					{
						m_OnItemEntering?.Invoke( m_Index ) ;
					}
				}
				else
				{
//					Debug.Log( "<color=#FF7F00>出ました : " + name + "</color>" ) ;
					if( m_Highlight != null )
					{
						m_Highlight.SetActive( false ) ;
					}
					m_IsHover = false ;
				}
			} ) ;

			if( m_Highlight != null )
			{
				m_Highlight.SetActive( m_IsHover && m_PointerModeEnabled ) ;
			}

			if( m_PadCursor != null )
			{
				m_PadCursor.SetActive( isPadCursorVisible ) ;
			}

			if( m_Checkmark != null )
			{
				m_Checkmark.SetValue( isCheck, false ) ;
			}

			if( CaptionText != null )
			{
				m_CaptionText.Text = pulldownItem.Text ;
			}

			if( m_CaptionImage != null )
			{
				m_CaptionImage.Sprite = pulldownItem.Image ;
			}
		}
	}
}

