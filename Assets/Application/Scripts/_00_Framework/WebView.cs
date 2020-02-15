using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using GREE ;

using uGUIHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// ウェブビュー表示クラス Version 2018/12/04 0
	/// </summary>
	public class WebView
	{
		/// <summary>
		/// ウェブビューを表示する
		/// </summary>
		/// <param name="tURL">表示するサイトのＵＲＬ</param>
		/// <param name="tCallback">サイトで入力あった場合に呼び出されるコールバックメソッド(ウェブビューのゲームオブジェクト・サイトからの入力文字列)</param>
		/// <returns>ウェブビューのゲームオブジェクト</returns>
		public static GameObject Open( string tURL, Action<GameObject,string> tCallback, UIView tArea = null )
		{
			WebViewObject tWebViewObject = GameObject.FindObjectOfType<WebViewObject>() ;
			if( tWebViewObject != null )
			{
				// 既にウェブビューは開かれている
				tWebViewObject.callback = tCallback ;

				tWebViewObject.LoadURL( tURL ) ;
				tWebViewObject.EvaluateJS() ;
				
				return tWebViewObject.gameObject ;
			}

			string tUA = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.52 Safari/537.36" ;

			// 新規にウェブビューを生成する
			tWebViewObject = ( new GameObject( "WebViewObject" ) ).AddComponent<WebViewObject>() ;
			tWebViewObject.Init
			(
				tCallback,	// Callback
				false,		// Transparency
				tUA,		// UserAgent
				( GameObject tWebView, string tErrorMessage ) =>
				{
					// Error
				},
				( GameObject tWebView, string tErrorMessage ) =>
				{
					// HttpError
				},
				( GameObject tWebView, string tUrl ) =>
				{
					// Loaded
#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )

					tWebViewObject.EvaluateJS
					( @"
						if( window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl )
						{
							window.Unity =
							{
								call: function( msg )
								{
									window.webkit.messageHandlers.unityControl.postMessage( msg ) ;
								}
							}
						}
						else
						{
							window.Unity =
							{
								call: function( msg )
								{
									window.location = 'unity:' + msg ;
								}
							}
						}
					" ) ;
#endif
				},
				false,
				( GameObject tWebView, string tUrl ) =>
				{
					// Started
				}
			) ;
			
			int tXMin = 0, tXMax = 0, tYMin = 0, tYMax = 0 ;

			if( tArea != null )
			{
				Vector2 s = tArea.GetCanvasSize() ;
//				Debug.LogWarning( "キャンバスの実サイズ:" + s ) ;

				Rect r = tArea.RectInCanvas ;
//				Debug.LogWarning( "キャンバス上の位置:" + r.xMin + " " + r.xMax + " " + r.yMin + " " + r.yMax ) ;

				float w = s.x * 0.5f ;
				float h = s.y * 0.5f ;

				tXMin = ( int )( Screen.width  * ( w + r.xMin ) / s.x ) ;
				tXMax = ( int )( Screen.width  * ( w - r.xMax ) / s.x ) ;

				tYMin = ( int )( Screen.height * ( h + r.yMin ) / s.y ) ;
				tYMax = ( int )( Screen.height * ( h - r.yMax ) / s.y ) ;
			}


			tWebViewObject.SetMargins( tXMin, tYMax, tXMax, tYMin ) ;
			tWebViewObject.SetVisibility( true ) ;

			tWebViewObject.LoadURL( tURL ) ;
			tWebViewObject.EvaluateJS() ;

			//------------------------------------------------------------------
			// オマケ
			
#if UNITY_EDITOR

			UICanvas tCanvas = UICanvas.CreateWithCamera( tWebViewObject.transform, 540, 960 ) ;

			Camera tCamera = tCanvas.WorldCamera ;
			tCamera.depth = 99 ;
			tCamera.clearFlags = CameraClearFlags.Nothing ;
//			tCanvas.SetResolution( 540, 960 ) ;
			
//			tCanvas.SetViewport( 540, 960, 0, 0, 270, 480, UICanvas.ScreenMatchMode.Expand ) ;
//			tCanvas.SetViewport( 960, 540, 0, 0, 480, 270, UICanvas.ScreenMatchMode.Height ) ;


			UIImage tScreen = tCanvas.AddView<UIImage>( "WebView Screen" ) ;

			if( tArea == null )
			{
				tScreen.SetAnchorToStretch() ;
			}
			else
			{
				tScreen.SetAnchorToCenter() ;

//				Vector2 s = tArea.GetCanvasSize() ;
				Rect r = tArea.RectInCanvas ;

				tScreen.SetSize( r.width, r.height ) ;
				tScreen.SetPosition( ( r.xMax + r.xMin ) * 0.5f, ( r.yMax + r.yMin ) * 0.5f ) ;
			}
			tScreen.Color = new Color( 0.0f, 0.0f, 0.0f, 0.5f ) ;
			tScreen.isInteraction = true ;
			tScreen.SetOnClick( OnClick ) ;

			UIText tMessage = tScreen.AddView<UIText>() ;
			tMessage.SetAnchorToCenter() ;
			tMessage.Color = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
			tMessage.isOutline = true ;
			tMessage.FontSize = 30 ;
			tMessage.Alignment = TextAnchor.MiddleCenter ;
			tMessage.HorizontalOverflow = HorizontalWrapMode.Wrap ;
			tMessage.Width = tScreen.Width ;

			tMessage.Text = "ダミーのウェブビューです\n閉じるには画面を\nクリックしてください" ;

			m_Callbak = tCallback ;
#endif

			return tWebViewObject.gameObject ;
		}

#if UNITY_EDITOR

		private static Action<GameObject,string> m_Callbak = null ;

		private static void OnClick( string tIdentity, UIView tView )
		{
			if( m_Callbak != null )
			{
				m_Callbak( null, "close" ) ;
				m_Callbak  = null ;
			}

			GameObject.Destroy( tView.transform.parent.parent.gameObject ) ;
		}
#endif

	}
}
