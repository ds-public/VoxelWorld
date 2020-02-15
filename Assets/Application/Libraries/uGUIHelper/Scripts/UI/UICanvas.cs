using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	[ RequireComponent(typeof(UnityEngine.Canvas))]
	[ RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
	[ RequireComponent(typeof(UnityEngine.UI.GraphicRaycaster))]
//	[ RequireComponent(typeof(GraphicRaycasterWrapper))]

	/// <summary>
	/// uGUI:Canvas クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UICanvas : UIView
	{
		public enum AutomaticRenderMode
		{
			None				= 0,
			ScreenSpaceOverlay	= 1,
			ScreenSpaceCamera	= 2,
			WorldSpace			= 3,
		}


		/// <summary>
		/// 実行時に簡易的にパラメータを設定する
		/// </summary>
		public AutomaticRenderMode	renderMode = AutomaticRenderMode.None ;

		/// <summary>
		/// 仮想解像度の横幅
		/// </summary>
		public float	width  = 960 ;

		/// <summary>
		/// 仮想解像度の縦幅
		/// </summary>
		public float	height = 540 ;

		/// <summary>
		/// 表示の優先順位値(大きい方が手前)
		/// </summary>
		public int		depth = 54 ;

		/// <summary>
		/// ターゲットカメラカメラ
		/// </summary>
		public Camera	renderCamera
		{
			get
			{
				return m_RenderCamera ;
			}
			set
			{
				m_RenderCamera = value ;
			}
		}

		[SerializeField]
		private Camera m_RenderCamera = null ;

		/// <summary>
		/// ＶＲモードで表示した際の基準位置からの前方への距離
		/// </summary>
		public float	vrDistance = 1.0f ;

		/// <summary>
		/// ＶＲモードで表示した際の縦の視野に対する大きさの比率
		/// </summary>
		public float	vrScale = 1.0f ;


		//-----------------------------------

		/// <summary>
		/// オーバーレイ表示を行うか
		/// </summary>
		public bool		isOverlay = false ;

		//-----------------------------------------------------
	
		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
		override protected void OnAwake()
		{
			if( Application.isPlaying == true )
			{
				// 実行中のみイベントシステムを生成する
				UIEventSystem.Create() ;

				//---------------------------------------------------------

				// 実行時に簡易的に状態を設定する

				if( renderMode != AutomaticRenderMode.None )
				{
					SetRenderMode( renderMode ) ;
				}
			}
		}

		/// <summary>
		/// 表示モードを設定する
		/// </summary>
		/// <param name="tDisplayMode"></param>
		public void SetRenderMode( AutomaticRenderMode renderMode )
		{
			if( renderMode == AutomaticRenderMode.ScreenSpaceOverlay || renderMode == AutomaticRenderMode.ScreenSpaceCamera )
			{
				// ２Ｄモード
				if( renderMode == AutomaticRenderMode.ScreenSpaceOverlay )
				{
					Canvas canvas = GetCanvas() ;
					if( canvas != null )
					{
						canvas.renderMode = RenderMode.ScreenSpaceOverlay ;
						canvas.sortingOrder = depth ;
					}
				}
				else
				if( renderMode == AutomaticRenderMode.ScreenSpaceCamera )
				{
					Canvas canvas = GetCanvas() ;
					if( canvas != null )
					{
						canvas.renderMode = RenderMode.ScreenSpaceCamera ;
						Camera camera = canvas.worldCamera ;
						if( camera == null )
						{
							camera = m_RenderCamera ;
						}
						if( camera == null )
						{
							camera = GetComponentInChildren<Camera>() ;
						}
						if( camera != null )
						{
							canvas.worldCamera = camera ;
						}
						if( canvas.worldCamera != null )
						{
							canvas.worldCamera.gameObject.SetActive( true ) ;
							canvas.worldCamera.depth = depth ;
						}
					}
				}

				CanvasScaler canvasScaler = GetCanvasScaler() ;
				if( canvasScaler != null )
				{
					canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize ;
					canvasScaler.referenceResolution = new Vector2( width, height ) ;
//					tCanvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand ;
				}

				this.Px = 0 ;
				this.Py = 0 ;
				this.Pz = 0 ;
			}
			else
			if( renderMode == AutomaticRenderMode.WorldSpace )
			{
				// ３Ｄモード
				Canvas canvas = GetCanvas() ;
				if( canvas != null )
				{
					canvas.renderMode = RenderMode.WorldSpace ;
					Camera camera = canvas.worldCamera ;
					if( camera == null )
					{
						camera = GetComponentInChildren<Camera>() ;
						if( camera != null )
						{
							camera.gameObject.SetActive( false ) ;
						}
					}
					else
					{
						// カメラが無効でも設定されているとレイキャストに引っかからなくなるので null にする必要がある
						camera.gameObject.SetActive( false ) ;
//						tCamera.depth = depth ;
						canvas.worldCamera = null ;
					}
					canvas.sortingOrder = depth ;
				}

//				CanvasScaler tCanvasScaler = _canvasScaler ;
//				if( tCanvasScaler != null )
//				{
//					tCanvasScaler.dynamicPixelsPerUnit = 3 ;
//					tCanvasScaler.referencePixelsPerUnit = 1 ;
//				}

				this.Px = 0 ;
				this.Py = 0 ;
				this.Pz = 0 ;

				this.Width  = width ;
				this.Height = height ;
			}
		}

		//-----------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = null )
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				canvas = gameObject.AddComponent<Canvas>() ;
			}
			if( canvas == null )
			{
				// 異常
				return ;
			}
				
			canvas.renderMode = RenderMode.ScreenSpaceOverlay ;
			canvas.pixelPerfect = true ;
				
			canvas.sortingOrder = 0 ;	// 表示を強制的に更新するために初期化が必要
				
			ResetRectTransform() ;

			//------------------------------------------

			CanvasScaler canvasScaler = GetCanvasScaler() ;
			if( canvasScaler == null )
			{
				canvasScaler = gameObject.AddComponent<CanvasScaler>() ;
			}
			if( canvasScaler == null )
			{
				// 異常
				return ;
			}

			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize ;
			canvasScaler.referenceResolution = new Vector2( 960, 640 ) ;
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
			canvasScaler.matchWidthOrHeight = 1.0f ;
			canvasScaler.referencePixelsPerUnit = 100.0f ;
			
			GraphicRaycasterWrapper graphicRaycaster = GetGraphicRaycaster() ;
			if( graphicRaycaster == null )
			{
				graphicRaycaster = gameObject.AddComponent<GraphicRaycasterWrapper>() ;
			}
			if( graphicRaycaster == null )
			{
				// 異常
				return ;
			}
				
			graphicRaycaster.ignoreReversedGraphics = true ;
			graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None ;
		}

		//-----------------------------------------------------
		
		/// <summary>
		/// 仮想解像度を設定する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetResolution( float w, float h )
		{
			CanvasScaler canvasScaler = GetCanvasScaler() ;
			if( canvasScaler == null )
			{
				return ;
			}
		
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize ;

			if( w <= 0 && h <= 0 )
			{
				w = Screen.width ;
				h = Screen.height ;
				canvasScaler.referenceResolution = new Vector2( w, h ) ;
				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;
			}
			else
			if( w <= 0 && h >  0 )
			{
				w = h ;
				canvasScaler.referenceResolution = new Vector2( w, h ) ;
				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
				canvasScaler.matchWidthOrHeight = 1.0f ;
			}
			else
			if( w >  0 && h <= 0 )
			{
				h = w ;
				canvasScaler.referenceResolution = new Vector2( w, h ) ;
				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
				canvasScaler.matchWidthOrHeight = 0.0f ;
			}
			else
			{
				canvasScaler.referenceResolution = new Vector2( w, h ) ;
//				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;
				canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
//				canvasScaler.matchWidthOrHeight = 1.0f ;
			}

			canvasScaler.referencePixelsPerUnit = 100.0f ;
		}
		
		/// <summary>
		/// ソーティングオーダーを設定する
		/// </summary>
		/// <param name="tOrder"></param>
		public void SetSortingOrder( int order )
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				return ;
			}
		
			canvas.sortingOrder = order ;
		}

		/// <summary>
		/// キャンバスのカメラを取得する
		/// </summary>
		/// <param name="tCameraDepth"></param>
		public Camera GetWorldCamera()
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				return null ;
			}

			return canvas.worldCamera ;
		}

		/// <summary>
		/// カメラをセットする
		/// </summary>
		/// <param name="tCamera"></param>
		/// <returns></returns>
		public bool SetWorldCamera( Camera camera )
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				return false ;
			}

			canvas.worldCamera = camera ;

			return true ;
		}

		/// <summary>
		/// キャンバスのカメラ
		/// </summary>
		public Camera WorldCamera
		{
			get
			{
				return GetWorldCamera() ;
			}
			set
			{
				SetWorldCamera( value ) ;
			}
		}
		
		/// <summary>
		/// キャンバスのカメラデプスを設定する
		/// </summary>
		/// <param name="tCameraDepth"></param>
		public void SetCameraDepth( float depth )
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				return ;
			}

			if( canvas.worldCamera == null )
			{
				return ;
			}

			canvas.worldCamera.depth = depth ;
		}

		/// <summary>
		/// キャンバスのカメラデプスを取得する
		/// </summary>
		/// <returns></returns>
		public float GetCameraDepth()
		{
			Canvas canvas = GetCanvas() ;
			if( canvas == null )
			{
				return -1 ;
			}

			if( canvas.worldCamera == null )
			{
				return -1 ;
			}

			return canvas.worldCamera.depth ;
		}

		/// <summary>
		/// スクリーンマッチモードを設定する
		/// </summary>
		/// <param name="tMode"></param>
		/// <param name="tScale"></param>
		/// <returns></returns>
		public bool SetScreenMatchMode( CanvasScaler.ScreenMatchMode tMode, float tScale )
		{
			CanvasScaler canvasScaler = GetCanvasScaler() ;
			if( canvasScaler == null )
			{
				return false ;
			}

			canvasScaler.screenMatchMode = tMode ;
			canvasScaler.matchWidthOrHeight = tScale ;
			
			return true ;
		}

		/// <summary>
		/// スクリーンに対する配置方法
		/// </summary>
		public enum ScreenMatchMode
		{
			Expand = 0,
			Width  = 1,
			Height = 2,
		}

		/// <summary>
		/// キャンバスの画面に対する表示領域を設定する
		/// </summary>
		/// <param name="aw">横方向の想定解像度</param>
		/// <param name="ah">縦方向の想定解像度</param>
		/// <param name="dx">表示領域の左上の座標Ｘ</param>
		/// <param name="dy">表示領域の左上の座標Ｙ</param>
		/// <param name="dw">表示領域の横幅の想定解像度</param>
		/// <param name="dh">表示領域の縦幅の想定解像度</param>
		public void SetViewport( float aw, float ah, float dx, float dy, float dw, float dh, ScreenMatchMode screenMatchMode )
		{
			Camera camera = WorldCamera ;

			if( camera == null )
			{
				return ;
			}

			float tSW = Screen.width ;
			float tSH = Screen.height ;
		
			if( camera.targetTexture != null )
			{
				// バックバッファのサイズを基準にする必要がある
				tSW = camera.targetTexture.width ;
				tSH = camera.targetTexture.height ;
			}

			float sx, sy, sw, sh ;

			// 想定アスペクト比(仮想解像度)
			float tVW = aw ;
			float tVH = ah ;

			float vw, vh ;

			// 仮想解像度での表示領域
			float tRX = dx ;
			float tRY = dy ;
			float tRW = dw ;
			float tRH = dh ;

			float rx, ry, rw, rh ;

			sx = 0 ;
			sy = 0 ;
			sw = 1 ;
			sh = 1 ;

//			rx = 0 ;
//			ry = 0 ;
//			rw = 1 ;
//			rh = 1 ;

			if( ( tSH / tSW ) >= ( tVH / tVW ) )
			{
				// 縦の方が長い(横はいっぱい表示)

//				Debug.LogWarning( "縦の方が長い" ) ;

				vw = tVW ;
				vh = tVH ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

				if( screenMatchMode == ScreenMatchMode.Expand )
				{
					// 常にアスペクトを維持する(Expand)　縦に隙間が出来る
					sx = 0 ;
					sw = 1 ;
					sh = tSW * vh / vw ;
					sh /= tSH ;
					sy = ( 1.0f - sh ) * 0.5f ;
				}
				else
				if( screenMatchMode == ScreenMatchMode.Width )
				{
					// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が増加
					sx = 0 ;
					sw = 1 ;
					sy = 0 ;
					sh = 1 ;

					tVH = tVW * tSH / tSW ;
				}
				else
				if( screenMatchMode == ScreenMatchMode.Height )
				{
					// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が減少
					sx = 0 ;
					sw = 1 ;
					sy = 0 ;
					sh = 1 ;

					tVW = tVH * tSW / tSH ;

					// 解像度が減少した分表示位置を移動させる
					tRX -= ( ( vw - tVW ) * 0.5f ) ;
				}
			}
			else
			{
				// 横の方が長い(縦はいっぱい表示)

//				Debug.LogWarning( "横の方が長い" ) ;

				vh = tVH ;
				vw = tVW ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

				if( screenMatchMode == ScreenMatchMode.Expand )
				{
					// 常にアスペクトを維持する(Expand)　横に隙間が出来る
					sy = 0 ;
					sh = 1 ;
					sw = tSH * vw / vh ;
					sw /= tSW ;
					sx = ( 1.0f - sw ) * 0.5f ;
				}
				else
				if( screenMatchMode == ScreenMatchMode.Height )
				{
					// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が増加
					sx = 0 ;
					sw = 1 ;
					sy = 0 ;
					sh = 1 ;

					tVW = tVH * tSW / tSH ;
				}
				else
				if( screenMatchMode == ScreenMatchMode.Width )
				{
					// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が減少
					sx = 0 ;
					sw = 1 ;
					sy = 0 ;
					sh = 1 ;

					tVH = tVW * tSH / tSW ;

					// 解像度が減少した分表示位置を移動させる
					tRY -= ( ( vh - tVH ) * 0.5f ) ;
				}
			}

			//----------------------------------------------------------

			if( tRW >  0 )
			{
				rx = tRX / tVW ;
				rw = tRW / tVW ;
			}
			else
			{
				// ０以下ならフル指定
				rx = 0 ;
				rw = 1 ;
			}

			if( tRH >  0 )
			{
				ry = tRY / tVH ;
				rh = tRH / tVH ;
			}
			else
			{
				// ０以下ならフル指定
				ry = 0 ;
				rh = 1 ;
			}
			
			if( rx <  0 )
			{
				rw += rx ;
				rx = 0 ;
			}
			if( rx >  0 && ( rx + rw ) >  1 )
			{
				rw -= ( ( rx + rw ) - 1 ) ;
			}
			if( rw >  1 )
			{
				rw  = 1 ;
			}
			if( rw == 0 )
			{
				rx = 0 ;
				rw = 1 ;
			}
			
			if( ry <  0 )
			{
				rh += ry ;
				ry = 0 ;
			}
			if( ry >  0 && ( ry + rh ) >  1 )
			{
				rh -= ( ( ry + rh ) - 1 ) ;
			}
			if( rh >  1 )
			{
				rh  = 1 ;
			}
			if( rh == 0 )
			{
				ry = 0 ;
				rh = 1 ;
			}

//			Debug.LogWarning( "sx:" + sx + " sw:" + sw + " rx:" + rx + " rw:" + rw ) ;
//			Debug.LogWarning( "sy:" + sy + " sh:" + sh + " ry:" + ry + " rh:" + rh ) ;
			camera.rect = new Rect( sx + rx * sw, sy + ( sh - ( ( ry + rh ) * sh ) ), rw * sw, rh * sh ) ;
		}



		//-------------------------------------------------------------------------------------
	
		/// <summary>
		/// キャンバスを生成する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas Create( float w = 0, float h = 0 )
		{
			return Create( null, w, h ) ;
		}
		
		/// <summary>
		/// キャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas Create( Transform parent = null, float w = 0, float h = 0 )
		{
			GameObject canvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			if( parent != null )
			{
				canvasGO.transform.SetParent( parent, false ) ;
			}
		
			UICanvas uiCanvas = canvasGO.AddComponent<UICanvas>() ;
			uiCanvas.SetDefault() ;
			uiCanvas.SetResolution( w, h ) ;
		
			canvasGO.transform.localPosition = new Vector3( 0, 0, -100 ) ;
			canvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			canvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			canvasGO.layer = 5 ;
		
			return uiCanvas ;
		}
		
		/// <summary>
		/// 子にカメラを持つ形でキャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas CreateWithCamera( Transform parent = null, float w = 0, float h = 0, int depth = 0 )
		{
			float rw = w ;
			float rh = h ;
			if( rw <= 0 && rh <= 0 )
			{
//				rw = Screen.width ;
				rh = Screen.height ;
			}
			else
			if( rw <= 0 && rh >  0 )
			{
//				rw = rh ;
			}
			else
			if( rw >  0 && rh <= 0 )
			{
				rh = rw ;
			}

			GameObject canvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			if( parent != null )
			{
				canvasGO.transform.SetParent( parent, false ) ;
			}

			UICanvas uiCanvas = canvasGO.AddComponent<UICanvas>() ;
			uiCanvas.SetDefault() ;
			uiCanvas.SetResolution( w, h ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			canvasGO.layer = 5 ;
		
			//------------------------

			GameObject cameraGO = new GameObject( "Camera" ) ;
			cameraGO.transform.SetParent( canvasGO.transform, false ) ;
		
			cameraGO.transform.localPosition = new Vector3( 0, 0, -100 ) ;
			cameraGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			cameraGO.transform.localScale = new Vector3( 1, 1, 1 ) ;
		
			Camera camera = cameraGO.AddComponent<Camera>() ;
		
			camera.clearFlags = CameraClearFlags.SolidColor ;
			camera.backgroundColor = new Color( 0, 0, 0, 0 ) ;
		
			camera.orthographic = true ;
			camera.orthographicSize = rh * 0.5f ;
			camera.nearClipPlane = 0.1f ;
			camera.farClipPlane  = 20000.0f ;
		
			camera.cullingMask = 1 << 5 ;

			camera.depth = depth ;

			camera.stereoTargetEye = StereoTargetEyeMask.None ;

			//------------------------
			
			Canvas canvas = uiCanvas.GetCanvas() ;

			canvas.renderMode = RenderMode.ScreenSpaceCamera ;
			canvas.pixelPerfect = true ;
			canvas.worldCamera = camera ;
			canvas.planeDistance = 100 ;
		
			//----------------------------------------------------------

			canvasGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
			canvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			canvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			return uiCanvas ;		
		}

		/// <summary>
		/// 親にカメラを持つ形でキャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas CreateOnCamera( Transform parent = null, float w = 0, float h = 0, int depth = 0 )
		{
			float rw = w ;
			float rh = h ;
			if( rw <= 0 && rh <= 0 )
			{
//				rw = Screen.width ;
				rh = Screen.height ;
			}
			else
			if( rw <= 0 && rh >  0 )
			{
//				rw = rh ;
			}
			else
			if( rw >  0 && rh <= 0 )
			{
				rh = rw ;
			}

			Camera camera = null ;
		
			if( parent != null )
			{
				camera = parent.GetComponent<Camera>() ;
			}

			if( camera == null )
			{
				GameObject cameraGO = new GameObject( "Camera" ) ;
				if( parent != null )
				{
					cameraGO.transform.SetParent( parent, false ) ;
				}
		
				cameraGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
				cameraGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
				cameraGO.transform.localScale = new Vector3( 1, 1, 1 ) ;
		
				camera = cameraGO.AddComponent<Camera>() ;
			
				camera.clearFlags = CameraClearFlags.SolidColor ;
				camera.backgroundColor = new Color( 0, 0, 0, 0 ) ;
		
				camera.orthographic = true ;
				camera.orthographicSize = rh * 0.5f ;
				camera.nearClipPlane = 0.1f ;
				camera.farClipPlane  = 20000.0f ;
		
				camera.cullingMask = 1 << 5 ;

				camera.depth = depth ;

				camera.stereoTargetEye = StereoTargetEyeMask.None ;
			}

			//------------------------
		
			GameObject canvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			canvasGO.transform.SetParent( camera.transform, false ) ;
		
			UICanvas uiCanvas = canvasGO.AddComponent<UICanvas>() ;
			uiCanvas.SetDefault() ;
			uiCanvas.SetResolution( w, h ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			canvasGO.layer = 5 ;
		
			//------------------------

			Canvas canvas = uiCanvas.GetCanvas() ;

			canvas.renderMode = RenderMode.ScreenSpaceCamera ;
			canvas.pixelPerfect = true ;
			canvas.worldCamera = camera ;
			canvas.planeDistance = 100 ;
		
			//----------------------------------------------------------

			canvasGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
			canvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			canvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			return uiCanvas ;
		
		}

		/// <summary>
		/// blocksRaycasts へのショートカットプロパティ
		/// </summary>
		public bool BlocksRaycasts
		{
			get
			{
				CanvasGroup canvasGroup = GetCanvasGroup() ;
				if( canvasGroup == null )
				{
					return false ;
				}

				return canvasGroup.blocksRaycasts ;
			}
			set
			{
				CanvasGroup canvasGroup = GetCanvasGroup() ;
				if( canvasGroup == null )
				{
					return ;
				}

				canvasGroup.blocksRaycasts = value ;
			}
		}
	}
}

