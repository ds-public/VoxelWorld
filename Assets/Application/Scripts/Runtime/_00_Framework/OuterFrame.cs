using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DSW
{
	/// <summary>
	/// 画面外の外枠クラス Version 2020/08/27 0
	/// </summary>
	public class OuterFrame : MonoBehaviour
	{
		// シングルトンインスタンス
		private static OuterFrame m_Instance ;

		/// <summary>
		/// インスタンス
		/// </summary>
		public  static OuterFrame   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

		/// <summary>
		/// ＶＲ対応用にキャンバスを取得できるようにする
		/// </summary>
		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		/// <summary>
		/// キャンバスの仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static bool SetCanvasResolution( float width, float height )
		{
			if( m_Instance == null || m_Instance.m_Canvas == null )
			{
				return false ;
			}

			m_Instance.m_Canvas.SetResolution( width, height, true ) ;

			return true ;
		}

		//-------------------------------------

		[SerializeField]
		protected UIView	m_Display ;

		[SerializeField]
		protected UIImage	m_Frame_U ;

		[SerializeField]
		protected UIImage	m_Frame_D ;

		[SerializeField]
		protected UIImage	m_Frame_L ;

		[SerializeField]
		protected UIImage	m_Frame_R ;

		//-----------------------------------------------------------

		public UICanvas TargetCanvas
		{
			get
			{
				return m_Canvas ;
			}
		}
		
		//---------------------------------------------------------------------------

		[SerializeField]
		protected int m_ScreenWidth ;

		[SerializeField]
		protected int m_ScreenHeight ;

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			m_ScreenWidth  = 0 ;
			m_ScreenHeight = 0 ;

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			float width  =  960 ;
			float height =  540 ;

			Settings settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			SetCanvasResolution( width, height ) ;

			//----------------------------------------------------------

			m_Display.SetSize( width, height ) ;
		}
			
		internal void Update()
		{
			if( m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height )
			{
				if( ( ( float )Screen.width / ( float )Screen.height ) >  ( m_Display.Size.x / m_Display.Size.y ) )
				{
					m_Display.SetActive( true ) ;

					m_Frame_U.SetActive( false ) ;
					m_Frame_D.SetActive( false ) ;

					m_Frame_L.SetActive( true ) ;
					m_Frame_R.SetActive( true ) ;
				}
				else
				if( ( ( float )Screen.width / ( float )Screen.height ) <  ( m_Display.Size.x / m_Display.Size.y ) )
				{
					m_Display.SetActive( true ) ;

					m_Frame_U.SetActive( true ) ;
					m_Frame_D.SetActive( true ) ;

					m_Frame_L.SetActive( false ) ;
					m_Frame_R.SetActive( false ) ;
				}
				else
				{
					m_Display.SetActive( false ) ;

					m_Frame_U.SetActive( false ) ;
					m_Frame_D.SetActive( false ) ;

					m_Frame_L.SetActive( false ) ;
					m_Frame_R.SetActive( false ) ;
				}

				m_ScreenWidth  = Screen.width ;
				m_ScreenHeight = Screen.height ;
			}
		}
	}
}
