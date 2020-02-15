using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.XR ;
using UnityEngine.EventSystems ;

using uGUIHelper ;
using AudioHelper ;
using TransformHelper ;

namespace DBS
{
	public class VRDeviceManager : SingletonManagerBase<VRDeviceManager>
	{
		private GameObject		m_VRCameraBase							= null ;
		private Camera			m_VRCamera								= null ;

		private GameObject		m_VRCameraBase_Distance					= null ;

		private GameObject		m_VRCameraBase_Source					= null ;	// 監視対象
		private Camera			m_VRCamera_Source						= null ;


		private UICanvas		m_VRCameraUI							= null ;

		private UIImage			m_VRCameraUI_Fade						= null ;
		private UIImage			m_VRCameraUI_Reticle					= null ;
		private UIImage			m_VRCameraUI_Reticle_ProgressFrame		= null ;
		private UIImage			m_VRCameraUI_Reticle_ProgressGauge		= null ;

		private bool			m_VRCameraUI_Reticle_Visible			= true ;
		
		[SerializeField]
		private float			m_VRCameraUI_Reticle_DefaultDistance	= 5f ;      // デフォルトのカメラからの距離

//		[SerializeField]
		private bool			m_VRCameraUI_Reticle_UseNormal			= true ;			// レティクルがヒットした物体の法線の向きに合わせるかどうか

		private Quaternion		m_VRCameraUI_Reticle_OriginalRotation ;	// 初期の回転
		private Vector3			m_VRCameraUI_Reticle_OriginalScale ;	// 初期の縮尺

		//---------------------------------------------------------------------------

		/// <summary>
		/// ＶＲカメラを取得する
		/// </summary>
		/// <returns></returns>
		public static Camera GetVRCamera()
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.m_VRCamera ;
		}
		
		//---------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// 土台を生成する
			m_VRCameraBase = new GameObject( "VR" ) ;

			// 原点じゃないと気持ち悪い
			m_VRCameraBase.transform.localPosition = Vector3.zero ;
			m_VRCameraBase.transform.localRotation = Quaternion.identity ;
			m_VRCameraBase.transform.localScale = Vector3.one ;

			m_VRCameraBase_Distance = new GameObject( "VR Distance" ) ;
			m_VRCameraBase_Distance.transform.localPosition = Vector3.zero ;
			m_VRCameraBase_Distance.transform.localRotation = Quaternion.identity ;
			m_VRCameraBase_Distance.transform.localScale = Vector3.one ;
			m_VRCameraBase_Distance.transform.SetParent( m_VRCameraBase.transform, false ) ;
			m_VRCameraBase_Distance.hideFlags = HideFlags.HideInHierarchy ;

			// カメラを生成する
			GameObject tCameraObject = new GameObject( "VR Camera" ) ;

			// 原点じゃないと気持ち悪い
			tCameraObject.transform.localPosition = Vector3.zero ;
			tCameraObject.transform.localRotation = Quaternion.identity ;
			tCameraObject.transform.localScale = Vector3.one ;

			tCameraObject.transform.SetParent( m_VRCameraBase.transform ) ;
			tCameraObject.SetActive( false ) ;

			m_VRCamera = tCameraObject.AddComponent<Camera>() ;
			m_VRCamera.tag = "Untagged" ;
			m_VRCamera.stereoTargetEye = StereoTargetEyeMask.None ;

			tCameraObject.AddComponent<FlareLayer>() ;
			AudioListener tAudioListener = tCameraObject.AddComponent<AudioListener>() ;
			AudioListener.volume = 1 ;
			tAudioListener.enabled = false ;

			//----------------------------------------------------------

			// ＶＲ専用のＵＩ
			SoftTransform t = m_VRCamera.gameObject.AddComponent<SoftTransform>() ;

			m_VRCameraUI = t.AddObject<UICanvas>( "VRCameraUI" ) ;
			m_VRCameraUI.width	= 2 ;
			m_VRCameraUI.height	= 2 ;
			m_VRCameraUI.depth	= 1000000 ;
			m_VRCameraUI.isOverlay = true ;

			m_VRCameraUI.GetCanvas().renderMode = RenderMode.WorldSpace ;
			m_VRCameraUI.GetCanvas().worldCamera = null ;

			m_VRCameraUI.GetCanvasScaler().dynamicPixelsPerUnit = 15 ;
			m_VRCameraUI.GetCanvasScaler().referencePixelsPerUnit = 1 ;

			m_VRCameraUI.Width  = 2 ;
			m_VRCameraUI.Height = 2 ;
			m_VRCameraUI.Px = 0 ;
			m_VRCameraUI.Py = 0 ;
			m_VRCameraUI.Pz = 0 ;

//			m_VRCameraUI.SetActive( false ) ;

			//----------------------------------

			m_VRCameraUI_Reticle = m_VRCameraUI.AddView<UIImage>( "Reticle" ) ;
			m_VRCameraUI_Reticle.SetAnchorToCenter() ;
			m_VRCameraUI_Reticle.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle.Sprite = Resources.Load<Sprite>( "Textures/VR/ReticleCenter" ) ;
			m_VRCameraUI_Reticle.Width  = 3.2f ;
			m_VRCameraUI_Reticle.Height = 3.2f ;
			m_VRCameraUI_Reticle.Px = 0 ;
			m_VRCameraUI_Reticle.Py = 0 ;
			m_VRCameraUI_Reticle.Pz = 0.75f ;
			m_VRCameraUI_Reticle.SetScale( 0.02f, 0.02f, 0.02f ) ;
			m_VRCameraUI_Reticle.Color = new Color32( 255,   0, 191, 255 ) ;
			m_VRCameraUI_Reticle.RaycastTarget = false ;
			m_VRCameraUI_Reticle.Type = Image.Type.Simple ;

			m_VRCameraUI_Reticle_ProgressFrame = m_VRCameraUI_Reticle.AddView<UIImage>( "Frame" ) ;
			m_VRCameraUI_Reticle_ProgressFrame.SetAnchorToStretch() ;
			m_VRCameraUI_Reticle_ProgressFrame.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle_ProgressFrame.Sprite = Resources.Load<Sprite>( "Textures/VR/ReticleCircle" ) ;
			m_VRCameraUI_Reticle_ProgressFrame.Pz = 0 ;
			m_VRCameraUI_Reticle_ProgressFrame.SetScale( 1, 1, 1 ) ;
			m_VRCameraUI_Reticle_ProgressFrame.Color = new Color32( 255,   0, 191,  79 ) ;
			m_VRCameraUI_Reticle_ProgressFrame.RaycastTarget = false ;
			m_VRCameraUI_Reticle_ProgressFrame.Type = Image.Type.Simple ;

			m_VRCameraUI_Reticle_ProgressGauge = m_VRCameraUI_Reticle_ProgressFrame.AddView<UIImage>( "Gauge" ) ;
			m_VRCameraUI_Reticle_ProgressGauge.SetAnchorToStretch() ;
			m_VRCameraUI_Reticle_ProgressGauge.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle_ProgressGauge.Sprite = Resources.Load<Sprite>( "Textures/VR/ReticleCircle" ) ;
			m_VRCameraUI_Reticle_ProgressGauge.Pz = 0 ;
			m_VRCameraUI_Reticle_ProgressGauge.SetScale( 1, 1, 1 ) ;
			m_VRCameraUI_Reticle_ProgressGauge.Color = new Color32( 255,   0, 191, 255 ) ;
			m_VRCameraUI_Reticle_ProgressGauge.RaycastTarget = false ;
			m_VRCameraUI_Reticle_ProgressGauge.Type = Image.Type.Filled ;
			m_VRCameraUI_Reticle_ProgressGauge.FillMethod = Image.FillMethod.Radial360 ;
			m_VRCameraUI_Reticle_ProgressGauge.FillOriginType = UIImage.FillOriginTypes.Bottom ;
			m_VRCameraUI_Reticle_ProgressGauge.FillAmount = 0 ;
			m_VRCameraUI_Reticle_ProgressGauge.FillClockwise = true ;
			m_VRCameraUI_Reticle_ProgressGauge.PreserveAspect = false ;

			m_VRCameraUI_Reticle_OriginalRotation	= m_VRCameraUI_Reticle.transform.localRotation ;
			m_VRCameraUI_Reticle_OriginalScale		= m_VRCameraUI_Reticle.LocalScale ;

			//----------------------------------

			// フェード(キャンバス内の描画優先順位はWorldSpaceであろうともＺ値は関係しないようなのでレティクルの後ら生成配置する)

			m_VRCameraUI_Fade = m_VRCameraUI.AddView<UIImage>( "Fade" ) ;
			m_VRCameraUI_Fade.SetAnchorToStretch() ;
			m_VRCameraUI_Fade.SetMargin( 0, 0, 0, 0 ) ;
			m_VRCameraUI_Fade.IsCanvasGroup = true ;
			m_VRCameraUI_Fade.Color = new Color32(   0,   0,  0, 255 ) ;
			m_VRCameraUI_Fade.SetActive( false ) ;

			m_VRCameraUI.Px = 0 ;
			m_VRCameraUI.Py = 0 ;
			m_VRCameraUI.Pz = 0.5f ;

			UITween tween ;

			tween = m_VRCameraUI_Fade.AddTween( "FadeIn" ) ;
			tween.Delay			= 0 ;
			tween.Duration		= 0.5f ;
			tween.AlphaEnabled	= true ;
			tween.AlphaFrom		= 1 ;
			tween.AlphaTo		= 0 ;
			tween.PlayOnAwake	= false ;

			tween = m_VRCameraUI_Fade.AddTween( "FadeOut" ) ;
			tween.Delay			= 0 ;
			tween.Duration		= 0.5f ;
			tween.AlphaEnabled	= true ;
			tween.AlphaFrom		= 0 ;
			tween.AlphaTo		= 1 ;
			tween.PlayOnAwake	= false ;

			//----------------------------------------------------------

			// 常駐
			DontDestroyOnLoad( m_VRCameraBase ) ;
		}

		new protected void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_VRCamera_Source		= null ;
				m_VRCameraBase_Source	= null ;

				// 生成したオブジェクトを破棄する
				m_VRCamera = null ;

				if( m_VRCameraBase != null )
				{
					DestroyImmediate( m_VRCameraBase ) ;
					m_VRCameraBase = null ;
				}
			}

			base.OnDestroy() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// シーンのＶＲ用カメラに同期させる
		/// </summary>
		/// <param name="tVRCameraBase"></param>
		/// <returns></returns>
		public static bool ReplaceVRCamera( GameObject tVRCameraBase )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ReplaceVRCamera_Private( tVRCameraBase ) ;
		}

		private bool ReplaceVRCamera_Private( GameObject tVRCameraBase )
		{
			if( tVRCameraBase == null )
			{
				return false ;
			}

			m_VRCamera_Source = tVRCameraBase.GetComponentInChildren<Camera>() ;
			if( m_VRCamera_Source == null || m_VRCamera_Source.gameObject == tVRCameraBase )
			{
				return false ;
			}

			//----------------------------------
			// 有効なカメラが見つかったのでいくつかの設定をコピーする

			//----------------------------------

			// このカメラは無効化する
			m_VRCamera_Source.gameObject.SetActive( false ) ;
			m_VRCamera_Source.tag = "Untagged" ;
			m_VRCamera_Source.stereoTargetEye = StereoTargetEyeMask.None ;

			m_VRCamera.tag = "MainCamera" ;
			m_VRCamera.stereoTargetEye = StereoTargetEyeMask.Both ;
			m_VRCamera.gameObject.SetActive( true ) ;
			AudioListener tAudioListener = m_VRCamera.gameObject.GetComponent<AudioListener>() ;
			if( tAudioListener != null )
			{
				if( AudioManager.instance != null )
				{
					AudioManager.listenerEnabled = false ;
				}

				tAudioListener.enabled = true ;
			}

			// 土台部分の状態をコピーする
			m_VRCameraBase_Source = tVRCameraBase ;

			ReplaceTransform( m_VRCameraBase_Source.transform, m_VRCameraBase.transform ) ;
			ReplaceCamera( m_VRCamera_Source, m_VRCamera ) ;

			return true ;
		}

		//---------------------------------------------------------------------------

		void LateUpdate()
		{
			if( m_VRCameraBase != null && m_VRCameraBase_Source != null && m_VRCamera != null && m_VRCamera_Source != null )
			{
				float tDistance ;

				// フレームの最後にカメラの状態を同期させる
				ReplaceTransform( m_VRCameraBase_Source.transform, m_VRCameraBase.transform ) ;
				ReplaceCamera( m_VRCamera_Source, m_VRCamera ) ;

				//---------------------------------------------------------

//				Debug.LogWarning( "カメラの向いている方向:" + m_VRCamera.transform.forward ) ;

				int i, l = m_AdjustCanvas.Count ;

				UICanvas	tCanvas ;
				float		tFieldOfView ;
				float		tScale ;
				int			tLost = 0 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					tCanvas			= m_AdjustCanvas[ i ].canvas ;
					if( tCanvas != null )
					{
						if( XRSettings.enabled == true )
						{
							// ＶＲ有効
							tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;

							tFieldOfView	= m_AdjustCanvas[ i ].fieldOfView ;

							float tAngle = Mathf.PI * tFieldOfView / 360.0f ;

							// ベースに追従する
							tDistance = tCanvas.vrDistance ;

							// 計算誤差が出るので位置算出用のオブジェクトを使って座標を取得する
							m_VRCameraBase_Distance.transform.localPosition = new Vector3( 0, 0, tDistance ) ;
							tCanvas.transform.position = m_VRCameraBase_Distance.transform.position ;

							tCanvas.transform.rotation = m_VRCameraBase.transform.rotation ;

							tScale = ( tCanvas.vrDistance * Mathf.Tan( tAngle ) / ( tCanvas.height * 0.5f ) ) * tCanvas.vrScale ;
							tCanvas.transform.localScale = new Vector3( tScale, tScale, tScale ) ;	// 回転を考慮してＺもスケーリングすること
						}
						else
						{
							// ＶＲ無効
							tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceCamera ) ;
						}
					}
					else
					{
						m_AdjustCanvas[ i ].canvas = null ;
						tLost ++ ;
					}
				}

				// キャンバスが消失したものは除外する(行儀が悪いので本来は明示的に除去すべき)
				if( l >  0 && tLost >  0 )
				{
					List<AdjustCanvas> tAdjustCanvas = new List<AdjustCanvas>() ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_AdjustCanvas[ i ].canvas != null )
						{
							tAdjustCanvas.Add( m_AdjustCanvas[ i ] ) ;
						}
					}
					m_AdjustCanvas = tAdjustCanvas ;
				}

				//---------------------------------------------------------

				GameObject tUI = null ;

				if( XRSettings.enabled == true )
				{
					// ＶＲモード
					m_VRCameraUI.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;
					m_VRCameraUI.SetScale( 1 ) ;	// ScreenSpaceOverlay になった際にスケールが変化しているので元に戻す必要がある

					float tNearClipPlane = m_VRCamera.nearClipPlane ;
					tNearClipPlane = tNearClipPlane + tNearClipPlane * 0.1f ;

					if( m_VRCameraUI_Fade.Pz <  tNearClipPlane )
					{
						m_VRCameraUI_Fade.Pz  = tNearClipPlane ;
					}

					//--------------------------------

					// レティクルを処理する

					if( m_VRCameraUI_Reticle_Visible == true && m_FadeShowing == false && m_FadePlaying == false )
					{
						// フェード中はレイがフェードに当たってレティクルの位置が飛びまくるのでレティクルの表示をしないようにする
						m_VRCameraUI_Reticle.SetActive( true ) ;
						
						tUI = UIEventSystem.GetLookingObject() ;
						if( tUI == null )
						{
							Ray tRay = new Ray( m_VRCamera.transform.position, m_VRCamera.transform.forward ) ;
							RaycastHit tHit ;
			
							if( Physics.Raycast( tRay, out tHit, Mathf.Infinity ) == true )
							{
								SetReticleView( m_VRCamera, tHit ) ;
							}
							else
							{
								SetReticleView( m_VRCamera ) ;
							}
						}
						else
						{
							// ＵＩの無限平面とレイキャストの交点を求める
							Vector3 tPlane_N = - tUI.transform.forward ;
							Vector3 tPlane_P = tUI.transform.position ;

							Vector3 tRay_P = m_VRCamera.transform.position ;
							Vector3 tRay_D = m_VRCamera.transform.forward ;

							float h = Vector3.Dot( tPlane_N, tPlane_P ) ;
							Vector3 tCross_P = tRay_P + tRay_D * ( ( h - Vector3.Dot( tPlane_N, tRay_P ) ) / Vector3.Dot( tPlane_N, tRay_D ) ) ;

							tDistance = ( tCross_P - tRay_P ).magnitude ;

							SetReticleView( tCross_P, tPlane_N, tDistance ) ;
						}
					}
					else
					{
						m_VRCameraUI_Reticle.SetActive( false ) ;
					}
				}
				else
				{
					m_VRCameraUI.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceOverlay ) ;
					m_VRCameraUI_Reticle.SetActive( false ) ;
				}
			}
		}

		private void ReplaceTransform( Transform t1, Transform t2 )
		{
			t2.position		= t1.position ;
			t2.rotation		= t1.rotation ;
			t2.localScale	= t2.localScale ;
		}

		private void ReplaceCamera( Camera c1, Camera c2 )
		{
			c2.nearClipPlane = c1.nearClipPlane ;
		}

		/// <summary>
		/// レティクルの位置を設定する(視線の方向に何も無いケース)
		/// </summary>
		private void SetReticleView( Camera tCamera )
		{
			float tNearClipPlane = tCamera.nearClipPlane ;
//			tNearClipPlane = tNearClipPlane + tNearClipPlane * 1 ;

			float tDistance = m_VRCameraUI_Reticle_DefaultDistance ;
			if( tDistance <  tNearClipPlane )
			{
				tDistance  = tNearClipPlane ;
			}

//			Debug.LogWarning( "CP:" + tCamera.transform.position + " CF:" + tCamera.transform.forward + " D:" + tDistance ) ;
//			Debug.LogWarning( "--R:" + ( tCamera.transform.position + tCamera.transform.forward * tDistance ) ) ;
			// レティクルの位置をカメラの前方に設定する
			m_VRCameraUI_Reticle.transform.position = tCamera.transform.position + tCamera.transform.forward * tDistance ;
	
			// 回転は初期状態のものを使う
			m_VRCameraUI_Reticle.transform.localRotation	= m_VRCameraUI_Reticle_OriginalRotation ;

			// 縮尺は初期状態のものに距離を乗算したものにする
			m_VRCameraUI_Reticle.transform.localScale		= m_VRCameraUI_Reticle_OriginalScale * tDistance ;
		}
		
		/// <summary>
		/// レティクルの位置を設定する(視線の方向に物体が存在するケース)
		/// </summary>
		/// <param name="hit"></param>
		private void SetReticleView( Camera tCamera, RaycastHit tHit )
        {
			m_VRCameraUI_Reticle.transform.position		= tHit.point ;	// Ｚ値がおおきくなると表示も小さくなるか

			// 重要なポイントとして位置が３Ｄ系なのでキャンバススケールは１にし個々のＵＩのスケールを調整(極小)にする必要がある
			if( m_VRCameraUI_Reticle_UseNormal == true )
			{
				// レティクルの法線方向を物体の法線方向に合わせる
				m_VRCameraUI_Reticle.transform.rotation			= Quaternion.FromToRotation( Vector3.forward, tHit.normal ) ;
			}
			else
			{
				// レティクルの法線方向を物体の法線方向に合わせない
				m_VRCameraUI_Reticle.transform.localRotation	= m_VRCameraUI_Reticle_OriginalRotation ;
			}

			// 最大距離より近いとレティクルが大きく表示される
			m_VRCameraUI_Reticle.transform.localScale			= m_VRCameraUI_Reticle_OriginalScale * tHit.distance ;	// 距離を掛け合わせることで遠くいっても見た目上の大きさを一定以上に保つ事ができる
		}

		/// <summary>
		/// レティクルの位置を設定する(視線の方向に物体が存在するケース)
		/// </summary>
		/// <param name="hit"></param>
		private void SetReticleView( Vector3 tPoint, Vector3 tNormal, float tDistance )
        {
			m_VRCameraUI_Reticle.transform.position		= tPoint ;	// Ｚ値がおおきくなると表示も小さくなるか

			// 重要なポイントとして位置が３Ｄ系なのでキャンバススケールは１にし個々のＵＩのスケールを調整(極小)にする必要がある
			if( m_VRCameraUI_Reticle_UseNormal == true )
			{
				// レティクルの法線方向を物体の法線方向に合わせる
				m_VRCameraUI_Reticle.transform.rotation			= Quaternion.FromToRotation( Vector3.forward, tNormal ) ;
			}
			else
			{
				// レティクルの法線方向を物体の法線方向に合わせない
				m_VRCameraUI_Reticle.transform.localRotation	= m_VRCameraUI_Reticle_OriginalRotation ;
			}

			// 最大距離より近いとレティクルが大きく表示される
			m_VRCameraUI_Reticle.transform.localScale			= m_VRCameraUI_Reticle_OriginalScale * tDistance ;	// 距離を掛け合わせることで遠くいっても見た目上の大きさを一定以上に保つ事ができる
		}


		//-----------------------------------------------------------

		public class AdjustCanvas
		{
			public UICanvas		canvas ;
			public float		fieldOfView ;

			public AdjustCanvas( UICanvas tCanvas, float tFieldOfView )
			{
				canvas			= tCanvas ;
				fieldOfView		= tFieldOfView ;
			}
		}

		private List<AdjustCanvas> m_AdjustCanvas = new List<AdjustCanvas>() ;
		
		public static bool AddAdjustCanvas( UICanvas tCanvas, float tFieldOfView )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.AddAdjustCanvas_Private( tCanvas, tFieldOfView ) ;
		}

		private bool AddAdjustCanvas_Private( UICanvas tCanvas, float tFieldOfView )
		{
			int i, l = m_AdjustCanvas.Count ;

			for( i  = 0 ; i < l ; i ++ )
			{
				if( m_AdjustCanvas[ i ].canvas == tCanvas )
				{
					m_AdjustCanvas[ i ].fieldOfView		= tFieldOfView ;
					return true ;
				}
			}

//			tCanvas.worldCamera = m_VRCamera ;
			
			tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;

			m_AdjustCanvas.Add( new AdjustCanvas( tCanvas, tFieldOfView ) ) ;
			return true ;
		}

		public static bool RemoveAdjustCanvas( UICanvas tCanvas )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.RemoveAdjustCanvas_Private( tCanvas ) ;
		}

		private bool RemoveAdjustCanvas_Private( UICanvas tCanvas )
		{
			int i, l = m_AdjustCanvas.Count ;

			for( i  = 0 ; i < l ; i ++ )
			{
				if( m_AdjustCanvas[ i ].canvas == tCanvas )
				{
					m_AdjustCanvas.RemoveAt( i ) ;
					return true ;
				}
			}

			return false ;
		}

		//---------------------------------------------------------------------------

		// レティクル操作関係

		/// <summary>
		/// レティクルの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticle( bool tVisible, bool tProgress, float tGauge )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetReticle_Private( tVisible, tProgress, tGauge ) ;
		}

		private bool SetReticle_Private( bool tVisible, bool tProgress, float tGauge )
		{
			if( m_VRCameraUI_Reticle == null || m_VRCameraUI_Reticle_ProgressFrame == null || m_VRCameraUI_Reticle_ProgressGauge == null )
			{
				return false ;
			}

			m_VRCameraUI_Reticle_Visible = tVisible ;
			if( tVisible == true && XRSettings.enabled == true )
			{
				m_VRCameraUI_Reticle.SetActive( true ) ;
			}
			else
			{
				m_VRCameraUI_Reticle.SetActive( false ) ;
			}

			m_VRCameraUI_Reticle_ProgressFrame.SetActive( tProgress ) ;
			m_VRCameraUI_Reticle_ProgressGauge.FillAmount = tGauge ;

			return true ;
		}

		/// <summary>
		/// レティクルのプログレスの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticleProgress( bool tProgress, float tGauge )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetReticleProgress_Private( tProgress, tGauge ) ;
		}

		private bool SetReticleProgress_Private( bool tProgress, float tGauge )
		{
			if( m_VRCameraUI_Reticle == null || m_VRCameraUI_Reticle_ProgressFrame == null || m_VRCameraUI_Reticle_ProgressGauge == null )
			{
				return false ;
			}

			m_VRCameraUI_Reticle_ProgressFrame.SetActive( tProgress ) ;
			m_VRCameraUI_Reticle_ProgressGauge.FillAmount = tGauge ;

			return true ;
		}


		//---------------------------------------------------------------------------

		// フェード操作関係

		/// <summary>
		/// デフォルトの色
		/// </summary>
		public Color fadeDefaultColor = new Color( 0, 0, 0, 1 ) ;

		/// <summary>
		/// デフォルトの遅延時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float fadeDefaultDelay = 0 ;

		/// <summary>
		/// デフォルトの実行時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float fadeDefaultDuration = -1 ;

		//-----------------------------------

		/// <summary>
		/// フェードの状態クラス
		/// </summary>
		public class Status : CustomYieldInstruction
		{
			public Status()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( isDone == false )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool isDone = false ;
		}

		//-----------------------------------

		private bool m_FadeShowing = false ;

		/// <summary>
		/// 表示状態
		/// </summary>
		public static bool isFadeShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_FadeShowing ;
			}
		}

		// 実行状態
		private bool m_FadePlaying = false ;

		/// <summary>
		/// 実行状態
		/// </summary>
		public static bool isFadePlaying
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_FadePlaying ;
			}
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void ShowFade( uint tAARRGGBB = 0x00000000 )
		{
			Color32 tColor= new Color32
			(
				( byte )( ( tAARRGGBB >> 16 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  8 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  0 ) & 0xFF ),
				( byte )( ( tAARRGGBB >> 24 ) & 0xFF )
			) ;

			ShowFade( tColor ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void ShowFade( Color tColor )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ShowFade_Private( tColor ) ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void ShowFade_Private( Color color )
		{
			if( m_VRCameraUI_Fade == null )
			{
				return ;
			}

			UITween tween = m_VRCameraUI_Fade.GetTween( "FadeOut" ) ;
			if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return ;	// 放っておいても表示になるので無視する
			}

			m_VRCameraUI_Fade.StopTweenAll() ;

			if( color.a == 0 )
			{
				color = fadeDefaultColor ;
			}

			m_VRCameraUI_Fade.Color = color ;

			m_VRCameraUI_Fade.Alpha = 1 ;

			gameObject.SetActive( true ) ;

			m_FadeShowing = true ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void HideFade()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.HideFade_Private() ;
		}


		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void HideFade_Private()
		{
			if( m_VRCameraUI_Fade == null )
			{
				return ;
			}

			UITween tween = m_VRCameraUI_Fade.GetTween( "FadeIn" ) ;
			if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return ;	// 放っておいても非表示になるので無視する
			}

			m_VRCameraUI_Fade.StopTweenAll() ;
			
			gameObject.SetActive( false ) ;

			m_FadeShowing = false ;
		}

		//---------------

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		public static Status FadeIn( float tDelay = 0, float tDuration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Status tStatus = new Status() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.FadeIn_Private( tDelay, tDuration, tStatus ) ) ;

			return tStatus ;
		}

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator FadeIn_Private( float tDelay, float tDuration, Status tStatus )
		{
			if( m_VRCameraUI_Fade == null )
			{
				tStatus.isDone = true ;
				yield break ;
			}

			m_FadePlaying = true ;

			m_VRCameraUI_Fade.Color = fadeDefaultColor ;

			if( tDelay <  0 )
			{
				tDelay  = fadeDefaultDelay ;
			}

			if( tDuration <  0 )
			{
				tDuration = fadeDefaultDuration ;
			}

			m_VRCameraUI_Fade.PlayTween( "FadeIn", tDelay, tDuration ) ;

			yield return new WaitWhile( () => m_VRCameraUI_Fade.IsAnyTweenPlaying == true ) ;

//			gameObject.SetActive( false ) ;
			m_VRCameraUI_Fade.SetActive( false ) ;

			m_FadePlaying = false ;

			tStatus.isDone = true ;

			m_FadeShowing = false ;
		}

		//---------------

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Status FadeOut( float tDelay = 0, float tDuration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Status tStatus = new Status() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.FadeOut_Private( tDelay, tDuration, tStatus ) ) ;

			return tStatus ;
		}

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator FadeOut_Private( float tDelay, float tDuration, Status tStatus )
		{
			if( m_VRCameraUI_Fade == null )
			{
				tStatus.isDone = true ;
				yield break ;
			}

			m_FadePlaying = true ;

			m_VRCameraUI_Fade.Color = fadeDefaultColor ;

			if( tDelay <  0 )
			{
				tDelay  = fadeDefaultDelay ;
			}

			if( tDuration <  0 )
			{
				tDuration = fadeDefaultDuration ;
			}

			m_VRCameraUI_Fade.PlayTween( "FadeOut", tDelay, tDuration ) ;

			yield return new WaitWhile( () => m_VRCameraUI_Fade.IsAnyTweenPlaying == true ) ;

			m_FadePlaying = false ;

			tStatus.isDone = true ;

			m_FadeShowing = true ;
		}
		
		//---------------

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void SetFadeDefaultColor( uint tAARRGGBB )
		{
			Color32 tColor = new Color32
			(
				( byte )( ( tAARRGGBB >> 16 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  8 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  0 ) & 0xFF ),
				( byte )( ( tAARRGGBB >> 24 ) & 0xFF )
			) ;

			SetFadeDefaultColor( tColor ) ;
		}

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tColor">色</param>
		public static void SetFadeDefaultColor( Color tColor )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultColor = tColor ;
		}

		/// <summary>
		/// フェードのデフォルトの遅延時間を設定する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		public static void SetFadeDefaultDelay( float tDelay )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultDelay = tDelay ;
		}

		/// <summary>
		/// フェードのデフォルトの実行時間を設定する
		/// </summary>
		/// <param name="tDuration">実行時間(秒)</param>
		public static void SetFadeDefaultDuration( float tDuration )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultDuration = tDuration ;
		}


	}
}
