using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// スクリーンのサイズ調整クラス Version 2022/09/19 0
	/// </summary>
	public class ScreenSizeFitter : ExMonoBehaviour
	{
		// セーフエリアの処理を有効にするか
		[SerializeField]
		protected bool		m_SafeAreaEnabled = true ;

		//---------------------------------------------------------------------------

		// 直親のキャンバス
		private Canvas	m_Canvas ;
		private UIView	m_Screen ;

		private RectTransform m_CanvasRectTransform ;

		private float	m_BasicWidth ;
		private float	m_BasicHeight ;

		private float	m_LimitWidth ;
		private float	m_LimitHeight ;

		//-----------------------------------

		private int		m_ScreenWidth ;
		private int		m_ScreenHeight ;

		private float	m_CanvasWidth ;
		private float	m_CanvasHeight ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			// 直親のキャンバスを探す

			Transform t = transform.parent ;
			while( t != null )
			{
				m_Canvas = t.GetComponent<Canvas>() ;
				if( m_Canvas != null )
				{
					break ;	// キャンバスを発見した
				}

				t = t.parent ;
			}

			if( m_Canvas != null )
			{
				m_CanvasRectTransform = m_Canvas.GetComponent<RectTransform>() ;
			}

			m_Screen = GetComponent<UIView>() ;

			//----------------------------------

			// キャンバスの解像度を設定する
			m_BasicWidth  =  960 ;
			m_BasicHeight =  540 ;

			m_LimitWidth  = 1280 ;
			m_LimitHeight =  720 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_BasicWidth  = settings.BasicWidth ;
				m_BasicHeight = settings.BasicHeight ;

				m_LimitWidth  = settings.LimitWidth ;
				m_LimitHeight = settings.LimitHeight ;
			}
		}

		internal void Start()
		{
			if( m_CanvasRectTransform == null || m_Screen == null )
			{
				return  ;
			}

			//----------------------------------

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			Refresh() ;

			//---------------------------------
			// 現在の値を保存する

			m_ScreenWidth  = Screen.width ;
			m_ScreenHeight = Screen.height ;

			m_CanvasWidth  = canvasWidth ;
			m_CanvasHeight = canvasHeight ;
		}

		// Canvas の deltaSize の更新の後の処理が好ましいので LateUpdate() で処理する
		internal void LateUpdate()
		{
			if( m_CanvasRectTransform == null || m_Screen == null )
			{
				return  ;
			}

			//----------------------------------

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			// 実解像度が変化したら更新する
			if( m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height || canvasWidth != m_CanvasWidth || canvasHeight != m_CanvasHeight )
			{
				// 更新
				Refresh() ;

				//---------------------------------
				// 現在の値を保存する

				m_ScreenWidth  = Screen.width ;
				m_ScreenHeight = Screen.height ;

				m_CanvasWidth  = canvasWidth ;
				m_CanvasHeight = canvasHeight ;
			}
		}

		// 表示更新する
		private bool Refresh()
		{
			m_Screen.SetAnchorToCenter() ;

			float canvasWidth  = m_CanvasRectTransform.sizeDelta.x ;
			float canvasHeight = m_CanvasRectTransform.sizeDelta.y ;

			float x = 0, y = 0 ;
			float width ;
			float height ;

			//------------------------------------------------------------------------------------------

			if( Screen.width >= Screen.height )
			{
				// 画面は横長
				if( ( canvasWidth  / canvasHeight ) >  ( m_BasicWidth  / m_BasicHeight ) )
				{
					// より横長
					float w = canvasWidth  ;

					if( w >  m_LimitWidth  )
					{
						w  = m_LimitWidth  ;
					}

					x		= 0 ;
					width	= w ;
					height	= m_BasicHeight ;
				}
				else
				if( ( canvasWidth  / canvasHeight ) <  ( m_BasicWidth  / m_BasicHeight ) )
				{
					// 基準より少ない横長
					float h = canvasHeight ;

					if( h >  m_LimitHeight )
					{
						h  = m_LimitHeight ;
					}

					x		= 0 ;
					width	= m_BasicWidth ;
					height	= h ;
				}
				else
				{
					// 丁度
					y		= 0 ;
					width	= m_BasicWidth ;
					height	= m_BasicHeight ;
				}
			}
			else
			{
				// 画面は縦長

				if( ( canvasHeight / canvasWidth  ) >  ( m_BasicHeight / m_BasicWidth  ) )
				{
					// より縦長
					float h = canvasHeight ;

					if( h >  m_LimitHeight )
					{
						h  = m_LimitHeight ;
					}

					y		= 0 ;
					height	= h ;
					width	= m_BasicWidth  ;
				}
				else
				if( ( canvasHeight / canvasWidth  ) <  ( m_BasicHeight / m_BasicWidth  ) )
				{
					// 基準より少ない縦長
					float w = canvasWidth  ;

					if( w >  m_LimitWidth  )
					{
						w  = m_LimitWidth  ;
					}

					y		= 0 ;
					height	= m_BasicHeight ;
					width	= w ;
				}
				else
				{
					// 丁度
					y		= 0 ;
					height	= m_BasicHeight ;
					width	= m_BasicWidth  ;
				}
			}

			//----------------------------------------------------------

			if( m_SafeAreaEnabled == true )
			{
				// セーフエリアの外にはみ出た部分を削る

				var safeArea = Screen.safeArea ;

				if( Screen.width  >= Screen.height )
				{
					// 画面は横長

					float xMin = canvasWidth  * ( float )safeArea.xMin / ( float )Screen.width  ;
					float xMax = canvasWidth  * ( float )safeArea.xMax / ( float )Screen.width  ;

					// 画面左部のマージン幅
					float marginL = xMin ;

					// 画面右部のマージン幅
					float marginR = canvasWidth  - xMax ;

					// 現在のマージン幅
					float margin = ( canvasWidth  - width  ) * 0.5f ;

					if( marginL <= margin )
					{
						// 画面左部のマージンは現在のままで良い
						marginL  = margin ; 
					}

					if( marginR <= margin )
					{
						// 画面右部のマージンは現在のままで良い
						marginR  = margin ;
					}

					// 画面左部と画面右部でマージン量が異なる場合に縦位置の補正をかける
					// 画面左部の方が太ければ右へ・画面左部の方が太ければ左へ
					x = ( marginR - marginL ) * 0.5f ;

					// 画面の縦幅をセーフエリアを反映したものに変更
					width = canvasWidth  - ( marginL + marginR ) ;
				}
				else
				{
					// 画面は縦長
					float yMin = canvasHeight * ( float )safeArea.yMin / ( float )Screen.height ;
					float yMax = canvasHeight * ( float )safeArea.yMax / ( float )Screen.height ;

					// 画面上部のマージン幅
					float marginT = yMin ;

					// 画面下部のマージン幅
					float marginB = canvasHeight - yMax ;

					// 現在のマージン幅
					float margin = ( canvasHeight - height ) * 0.5f ;

					if( marginT <= margin )
					{
						// 画面上部のマージンは現在のままで良い
						marginT  = margin ; 
					}

					if( marginB <= margin )
					{
						// 画面下部のマージンは現在のままで良い
						marginB  = margin ;
					}

					// 画面上部と画面下部でマージン量が異なる場合に縦位置の補正をかける
					// 画面上部の方が太ければ下へ・画面下部の方が太ければ上へ
					y = ( marginB - marginT ) * 0.5f ;

					// 画面の縦幅をセーフエリアを反映したものに変更
					height = canvasHeight - ( marginT + marginB ) ;
				}
			}

			//----------------------------------------------------------

			m_Screen.SetPosition( x, y ) ;
			m_Screen.SetSize( width, height ) ;

			return true ;
		}
	}
}
