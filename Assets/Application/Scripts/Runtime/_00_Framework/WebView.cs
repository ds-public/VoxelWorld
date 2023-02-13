using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace DSW
{
	//-------------------------------------------------------------------
	// 重要:
	// HTML 側では、以下のように記述する
	//
	// <a href="" onclick="Unity.call( 'anyString' )">Send Callback</a>
	//
	// Unity には、anyString という文字列が送られる
	//-------------------------------------------------------------------

	/// <summary>
	/// ウェブビュー表示クラス Version 2023/02/02 0
	/// </summary>
	public class WebView : ExMonoBehaviour
	{
		/// <summary>
		/// ウェブビューを表示する
		/// </summary>
		/// <param name="url">表示するサイトのＵＲＬ</param>
		/// <param name="callback">サイトで入力あった場合に呼び出されるコールバックメソッド(ウェブビューのゲームオブジェクト・サイトからの入力文字列)</param>
		/// <param name="area"></param>
		/// <param name="onLoaded"></param>
		/// <param name="onError"></param>
		/// <param name="customHeaders"></param>
		/// <returns>ウェブビューのゲームオブジェクト</returns>
		public static WebViewObject Open( string url, Action<GameObject,string> callback, UIView area = null, Action<WebViewObject,string> onLoaded = null, Action<WebViewObject,string> onError = null, IReadOnlyDictionary<string, string> customHeaders = null )
		{
			m_CustomCallback = callback ;

			WebViewObject webViewObject = GameObject.FindObjectOfType<WebViewObject>() ;
			if( webViewObject != null )
			{
				// 既にウェブビューは開かれている

				// メッセージ用のコールバック設定
//				webViewObject.Callback = callback ;

				// メッセージ用のコールバック(カスタム)
				webViewObject.Callback = OnCustomCallback ;

				// ページロード完了時のコールバック設定(拡張)
				webViewObject.OnLoaded	= onLoaded ;
				webViewObject.OnError	= onError ;

				if ( customHeaders != null )
				{
					foreach ( var header in customHeaders )
					{
						webViewObject.AddCustomHeader( header.Key, header.Value ) ;
					}
				}

				webViewObject.LoadURL( url ) ;
				
				return webViewObject ;
			}

			// UserAgent(なりすまし)
			string userAgent = "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36" ;

			// Unity にメッセージを送るには HTML で以下のように記述する
			// <a href="javascript:void(0)" onclick="Unity.call('any message');">Send Message</a></p>
			// <input type="submit" value="Send Message" onclick="Unity.call('any message'); return false;"/>

			// 最後に開いたＵＲＬ
			string lastLoadedUrl = string.Empty ;

			// 新規にウェブビューを生成する
			webViewObject = ( new GameObject( "WebViewObject" ) ).AddComponent<WebViewObject>() ;
			webViewObject.Init
			(
				OnCustomCallback,		// Callback
				false,					// Transparency
				true,					// Zoom
				userAgent,				// UserAgent
				( GameObject webView, string errorMessage ) =>
				{
					// Error
					OnError( webView, "Error", errorMessage, lastLoadedUrl ).Forget() ;
				},
				( GameObject webView, string errorMessage ) =>
				{
					// HttpError(一時的に無効化)
//					OnError( webView, "HttpError", errorMessage ).Forget() ;
				},
				( GameObject webView, string targetUrl ) =>
				{
					// Loaded
//					DebugScreen.Out( "onLoaded:" + targetUrl ) ;

#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )

					webViewObject.EvaluateJS
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
					// ロード完了時にコールバックを呼び出す(拡張)
					WebViewObject instance = webView.GetComponent<WebViewObject>() ;
					if( instance != null )
					{
						instance.OnLoaded?.Invoke( instance, targetUrl ) ;
					}

					// 最後に開いたＵＲＬを保存しておく
					lastLoadedUrl = targetUrl ;
				},
				true,	// enableWKWebView
				0,		// wkContentMode
				( GameObject webView, string targetUrl ) =>
				{
					// Started
//					DebugScreen.Out( "onStart:" + targetUrl ) ;
				}
			) ;

			// ページロード完了時のコールバック設定(拡張)
			webViewObject.OnLoaded	= onLoaded ;
			webViewObject.OnError	= onError ;

			//--------------------------------------------------------------------------
			// 表示位置設定

			int xMin = 0, xMax = 0, yMin = 0, yMax = 0 ;

			if( area != null )
			{
				Vector2 s = area.GetCanvasSize() ;
//				Debug.LogWarning( "キャンバスの実サイズ:" + s ) ;

				Rect r = area.RectInCanvas ;
//				Debug.LogWarning( "キャンバス上の位置:" + r.xMin + " " + r.xMax + " " + r.yMin + " " + r.yMax ) ;

				float w = s.x * 0.5f ;
				float h = s.y * 0.5f ;

				xMin = ( int )( Screen.width  * ( w + r.xMin ) / s.x ) ;
				xMax = ( int )( Screen.width  * ( w - r.xMax ) / s.x ) ;

				yMin = ( int )( Screen.height * ( h + r.yMin ) / s.y ) ;
				yMax = ( int )( Screen.height * ( h - r.yMax ) / s.y ) ;
			}

			webViewObject.SetMargins( xMin, yMax, xMax, yMin ) ;
			webViewObject.SetVisibility( true ) ;

			// カスタムヘッダー情報があれば設定します
			if ( customHeaders != null )
			{
				webViewObject.ClearCustomHeader() ;
				foreach ( var header in customHeaders )
				{
					webViewObject.AddCustomHeader( header.Key, header.Value ) ;
				}
			}

			webViewObject.LoadURL( url ) ;

			//------------------------------------------------------------------
			// UnityEditor でのデバッグ用
			
#if UNITY_EDITOR

			float width  = 960 ;
			float height = 540 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			UICanvas canvas = UICanvas.Create( webViewObject.transform, width, height ) ;
			canvas.name = "Canvas" ;
			canvas.SetScreenMatchMode( UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand, 1 ) ;
			canvas.SortingOrder = 1200 ;

//			Camera canvasCamera = canvas.WorldCamera ;
//			canvasCamera.depth = 99 ;
//			canvasCamera.clearFlags = CameraClearFlags.Nothing ;
//			canvas.SetResolution( 540, 960 ) ;
			
//			canvas.SetViewport( 540, 960, 0, 0, 270, 480, UICanvas.ScreenMatchMode.Expand ) ;
//			canvas.SetViewport( 960, 540, 0, 0, 480, 270, UICanvas.ScreenMatchMode.Height ) ;


			UIImage screen = canvas.AddView<UIImage>( "WebView Screen" ) ;

			if( area == null )
			{
				screen.SetAnchorToStretch() ;
			}
			else
			{
				screen.SetAnchorToCenter() ;

//				Vector2 s = tArea.GetCanvasSize() ;
				Rect r = area.RectInCanvas ;

//				Debug.LogWarning( "仮サイズ:" + r ) ;

				screen.SetSize( r.width, r.height ) ;
				screen.SetPosition( ( r.xMax + r.xMin ) * 0.5f, ( r.yMax + r.yMin ) * 0.5f ) ;
			}

			screen.Color = new Color( 0.0f, 0.0f, 0.0f, 0.5f ) ;
			screen.RaycastTarget = true ;
			screen.IsInteraction = true ;
			screen.SetOnClick( OnClick ) ;

			UIText message = screen.AddView<UIText>() ;
			message.SetAnchorToCenter() ;
			message.Color = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
			message.IsOutline = true ;
			message.FontSize = 30 ;
			message.Alignment = TextAnchor.MiddleCenter ;
			message.HorizontalOverflow = HorizontalWrapMode.Wrap ;
			message.Width = screen.Width ;

			message.Text = "ダミーのウェブビューです\n閉じるには画面を\nクリックしてください" ;

			m_Callbak = callback ;
#endif
			return webViewObject ;
		}


		//-------------------------------------------------------------------------------------------

		private static Action<GameObject,string> m_CustomCallback ;

		private static void OnCustomCallback( GameObject webViewObject, string text )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return ;
			}

			//----------------------------------

			string key_OpenBrowser = "OpenBrowser " ;

			if( text.IndexOf( key_OpenBrowser ) == 0 )
			{
				// 外部ブラウザを開く
				string url = text.Replace( key_OpenBrowser, "" ) ;
				url = url.TrimStart( ' ' ).TrimEnd( ' ' ) ;

				ApplicationManager.OpenURL( url ) ;
			}
			else
			{
				if( m_CustomCallback != null )
				{
					m_CustomCallback( webViewObject, text ) ;
				}
			}
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// エラーが発生した際の処理
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private static async UniTask OnError( GameObject webView, string title, string message, string url )
		{
			WebViewObject webViewObject = webView.GetComponent<WebViewObject>() ;
			if( webViewObject == null )
			{
				return ;
			}

			webViewObject.SetVisibility( false ) ;
			await Dialog.Open( title, message + "\n\n" + url, new string[]{ "CLOSE" } ) ;
			webViewObject.SetVisibility( true ) ;

			webViewObject.OnError?.Invoke( webViewObject, message ) ;
		}

#if UNITY_EDITOR

		private static Action<GameObject,string> m_Callbak = null ;

		private static void OnClick( string identity, UIView view )
		{
			if( m_Callbak != null )
			{
				m_Callbak( null, "close" ) ;
				m_Callbak  = null ;
			}

			GameObject.Destroy( view.transform.parent.parent.gameObject ) ;
		}
#endif

		/// <summary>
		/// 表示する
		/// </summary>
		/// <param name="webViewObject"></param>
		/// <returns></returns>
		public static bool Show( WebViewObject webViewObject )
		{
			if( webViewObject == null )
			{
				return false ;
			}

			webViewObject.SetVisibility( true ) ;

#if UNITY_EDITOR
			// デバッグ表示
			Transform screen = webViewObject.transform.Find( "Canvas/WebView Screen" ) ;
			if( screen != null )
			{
				screen.gameObject.SetActive( true ) ;
			}
#endif
			return true ;
		}

		/// <summary>
		/// 消去する
		/// </summary>
		/// <param name="webViewObject"></param>
		/// <returns></returns>
		public static bool Hide( WebViewObject webViewObject )
		{
			if( webViewObject == null )
			{
				return false ;
			}

			webViewObject.SetVisibility( false ) ;

#if UNITY_EDITOR
			// デバッグ消去
			Transform screen = webViewObject.transform.Find( "Canvas/WebView Screen" ) ;
			if( screen != null )
			{
				screen.gameObject.SetActive( false ) ;
			}
#endif
			return true ;
		}

		/// <summary>
		/// 破棄する
		/// </summary>
		/// <param name="webViewObject"></param>
		/// <returns></returns>
		public static bool Destroy( WebViewObject webViewObject )
		{
			if( webViewObject == null )
			{
				return false ;
			}

			Destroy( webViewObject.gameObject ) ;

			return true ;
		}
	}
}
