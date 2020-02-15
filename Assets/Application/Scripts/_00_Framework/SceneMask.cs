using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// 単色の画面マスク制御クラス Version 2019/09/18 0
	/// </summary>
	public class SceneMask : MonoBehaviour
	{
		// シングルトンインスタンス
		private static SceneMask m_Instance = null ; 

		/// <summary>
		/// フェードクラスのインスタンス
		/// </summary>
		public  static SceneMask   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		// フェード部分のインスタンス
		[SerializeField]
		protected UIImage	m_SceneMask ;

		/// <summary>
		/// デフォルトの色
		/// </summary>
		public Color DefaultColor = new Color( 0, 0, 0, 1 ) ;

		//---------------------------------------------------------------------------

		void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。
		}

		//-----------------------------------

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void Show( uint aarrggbb = 0x00000000 )
		{
			Color32 color= new Color32
			(
				( byte )( ( aarrggbb >> 16 ) & 0xFF ),
				( byte )( ( aarrggbb >>  8 ) & 0xFF ),
				( byte )( ( aarrggbb >>  0 ) & 0xFF ),
				( byte )( ( aarrggbb >> 24 ) & 0xFF )
			) ;

			Show( color ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void Show( Color color )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Show_Private( color ) ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void Show_Private( Color color )
		{
			if( m_SceneMask == null )
			{
				return ;
			}

			if( color.a == 0 )
			{
				color = DefaultColor ;
			}

			m_SceneMask.Color = color ;

			m_SceneMask.Alpha = 1 ;

			gameObject.SetActive( true ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void Hide()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Hide_Private() ;
		}


		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void Hide_Private()
		{
			if( m_SceneMask == null )
			{
				return ;
			}

			gameObject.SetActive( false ) ;
		}
	}
}
