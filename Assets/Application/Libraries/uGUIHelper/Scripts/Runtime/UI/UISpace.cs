using System.Collections ;
using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace uGUIHelper
{
	/// <summary>
	/// ＵＩの一部として表示したいカメラを簡単につくるためのコンポーネント（クリッピングは効かない事に注意）
	/// </summary>
	public class UISpace : UIView
	{
		/// <summary>
		/// ワールドルート
		/// </summary>
		public Transform WorldRoot
		{
			get
			{
				return m_WorldRoot ;
			}
			set
			{
				if( m_WorldRoot != value )
				{
#if UNITY_EDITOR
					if( m_WorldRoot != null )
					{
						m_WorldRoot.hideFlags = HideFlags.None ;
					}
#endif
					m_WorldRoot = value ;
#if UNITY_EDITOR
					if( m_WorldRoot != null )
					{
						// Transform のみを対象とする(GameObject を対象とすると、全てのコンポーネントが編集不能となる)
						m_WorldRoot.hideFlags = HideFlags.NotEditable ;
					}
#endif
				}
			}
		}

		[SerializeField][HideInInspector]
		private Transform m_WorldRoot = null ;


		/// <summary>
		/// レンダリング対象のカメラ
		/// </summary>
		public  Camera   TargetCamera
		{
			get
			{
				return m_TargetCamera ;
			}
			set
			{
				m_TargetCamera = value ;
			}
		}
		
		[SerializeField][HideInInspector]
		private Camera m_TargetCamera = null ;

		/// <summary>
		/// 実行時にカメラのカリングマスクと３Ｄ空間のレイヤーを指定したレイヤーに設定するかどうか
		/// </summary>
		public bool IsForceWorldLayer
		{
			get
			{
				return m_IsForceWorldLayer ;
			}
			set
			{
				m_IsForceWorldLayer = value ;
			}
		}

		[SerializeField][HideInInspector]
		private bool m_IsForceWorldLayer = true ;

		/// <summary>
		/// 強制設定するレイヤー(００～３１)
		/// </summary>
		public int WorldLayer
		{
			get
			{
				return m_WorldLayer ;
			}
			set
			{
				if( m_WorldLayer != value )
				{
					m_WorldLayer  = value ;

					if( m_IsForceWorldLayer == true )
					{
						// Editor モードでも自動適用を行う
						SetWorldLayer( m_WorldLayer ) ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_WorldLayer = 0 ;

		/// <summary>
		/// カメラデプスを設定する
		/// </summary>
		/// <param name="depth"></param>
		/// <returns></returns>
		public bool SetCameraDepth( int depth )
		{
			if( m_TargetCamera == null )
			{
				return false ;
			}

			m_TargetCamera.depth = depth ;

			return true ;
		}

		/// <summary>
		/// カリングマスクを設定する
		/// </summary>
		/// <param name="cullingMask"></param>
		/// <returns></returns>
		public bool SetCullingMask( int cullingMask )
		{
			if( m_TargetCamera == null )
			{
				return false ;
			}

			m_TargetCamera.cullingMask = cullingMask ;

			return true ;
		}

		/// <summary>
		/// レンダーレクスチャを使用するかどうか
		/// </summary>
		public  bool	  RenderTextureEnabled
		{
			get
			{
				return m_RenderTextureEnabled ;
			}
			set
			{
				if( m_RenderTextureEnabled != value )
				{
					m_RenderTextureEnabled  = value ;

					if( m_RenderTextureEnabled == true )
					{
						// 有効になった
						CreateRenderTexture( false ) ;
					}
					else
					{
						// 無効になった
						DeleteRenderTexture( false ) ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_RenderTextureEnabled = false ;

		/// <summary>
		/// レンダーテクスチャのインスタンス
		/// </summary>
		public RenderTexture	Texture
		{
			get
			{
				return m_RenderTexture ;
			}
		}

		[SerializeField][HideInInspector]
		private RenderTexture	m_RenderTexture = null ;

		/// <summary>
		/// レンダーイメージのインスタンス
		/// </summary>
		public UIRawImage Image
		{
			get
			{
				return m_RenderImage ;
			}
		}

		[SerializeField][HideInInspector]
		private UIRawImage		m_RenderImage = null ;

		// 操作禁止用の Tracker (HideFlags.NotEditable でも代用は出来そうだが・・・)


		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector2			m_Size ;

		/// <summary>
		/// 縦方向の角度を固定化するかどうか
		/// </summary>
		public  bool			  FlexibleFieldOfView
		{
			get
			{
				return m_FlexibleFieldOfView ;
			}
			set
			{
				m_FlexibleFieldOfView = value ;
			}
		}

		[SerializeField][HideInInspector]
		private bool			m_FlexibleFieldOfView = true ;

		/// <summary>
		/// 縦方向の基本的な長さ
		/// </summary>
		public  float			  BasisHeight
		{
			get
			{
				return m_BasisHeight ;
			}
			set
			{
				m_BasisHeight = value ;
			}
		}

		[SerializeField][HideInInspector]
		private float			m_BasisHeight = 0 ;



		/// <summary>
		/// カメラの焦点の位置
		/// </summary>
		public Vector2	CameraOffset{ get{ return m_CameraOffset ; } set{ m_CameraOffset = value ; } }

		[SerializeField]
		protected Vector2 m_CameraOffset = Vector2.zero ;


		//-----------------------------------------------------------

		private float m_InitialFieldOfView = 0 ;

		//-----------------------------------------------------------

		// レンダーテクスチャが有効な場合のみ使用可能なパラメータ

		[SerializeField]
		protected bool m_ImageMask = false ;
		public bool	ImageMask{ get{ return m_ImageMask ; } set{ m_ImageMask = value ; } }

		[SerializeField]
		protected bool m_ImageInversion = false ;
		public bool ImageInversion{ get{ return m_ImageInversion ; } set{ m_ImageInversion = value ; } }

		[SerializeField]
		protected bool m_ImageShadow = false ;
		public bool ImageShadow{ get{ return m_ImageShadow ; } set{ m_ImageShadow = value ; } }

		[SerializeField]
		protected bool m_ImageOutline = false ;
		public bool ImageOutline{ get{ return m_ImageOutline ; } set{ m_ImageOutline = value ; } }

		[SerializeField]
		protected bool m_ImageGradient = false ;
		public bool ImageGradient{ get{ return m_ImageGradient ; } set{ m_ImageGradient = value ; } }

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行される）　※AddView から呼ばれる可能性があるため UNITY_EDITOR 限定には出来ない
		/// </summary>
		/// <param name="option"></param>
		protected override void OnBuild( string option = "" )
		{
			GetRectTransform() ;

			ResetRectTransform() ;

			var worldRoot = new GameObject( "3D" ) ;
			worldRoot.transform.SetLocalPositionAndRotation( new Vector3( 0, 0, -3 ), Quaternion.identity ) ;
			worldRoot.transform.localScale = Vector3.one ;
			worldRoot.transform.SetParent( transform, false ) ;

			m_WorldRoot = worldRoot.transform ;

			// ３Ｄ用のカメラを生成する
			var camera3d = new GameObject( "3D Camera" ) ;
			
			camera3d.transform.SetLocalPositionAndRotation( new Vector3( 0, 0, -3 ), Quaternion.identity ) ;
			camera3d.transform.localScale = Vector3.one ;
			camera3d.transform.SetParent( worldRoot.transform, false ) ;

			m_TargetCamera = camera3d.AddComponent<Camera>() ;

			// ３Ｄ用のライトを生成する

			var light3d = new GameObject( "Directional Light" ) ;
			light3d.transform.SetLocalPositionAndRotation( new Vector3( 0, 0, -3 ), Quaternion.identity ) ;
			light3d.transform.localScale = Vector3.one ;
			light3d.transform.SetParent( worldRoot.transform, false ) ;

			var light = light3d.AddComponent<Light>() ;
			light.type = LightType.Directional ;
			light.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			// ディレクショナルライトのデフォルトの方向はＺ＋(画面奥)を向いている
			// つまり３Ｄオブジェクトの正面から光が当たる形である

			//----------------------------------
			// レイヤー対象

			m_IsForceWorldLayer = true ;
//			m_TargetLayer = 24 ;
		}


//		protected override void OnAwake()
//		{
//		}

		protected override void OnStart()
		{
			if( m_TargetCamera != null )
			{
				if( m_RenderTextureEnabled == true )
				{
					CreateRenderTexture( true ) ;
					m_Size = Size ;
				}

				m_InitialFieldOfView = m_TargetCamera.fieldOfView ;
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			//---------------------------------------------------------

			if( m_WorldRoot != null && m_WorldRoot.parent != null )
			{
				// 常にグローバルで１倍になるようにスケールを調整する

				float gsx = m_WorldRoot.parent.lossyScale.x ;
				float gsy = m_WorldRoot.parent.lossyScale.y ;
				float gsz = m_WorldRoot.parent.lossyScale.z ;

				if( gsx != 0 && gsy != 0 && gsz != 0 )
				{
					// １つも０が無い事が処理条件

					float lsx = 1.0f / gsx ;
					float lsy = 1.0f / gsy ;
					float lsz = 1.0f / gsz ;

					if( m_WorldRoot.localScale.x != lsx || m_WorldRoot.localScale.y != lsy || m_WorldRoot.localScale.z != lsz )
					{
						// ローカルスケールを更新する
						m_WorldRoot.localScale = new Vector3( lsx, lsy, lsz ) ;
					}
				}
#if UNITY_EDITOR
				m_WorldRoot.hideFlags = HideFlags.NotEditable ;
#endif
			}

			//----------------------------------

			if( m_RenderTextureEnabled == true && ( m_Size.x != Size.x || m_Size.y != Size.y ) )
			{
				// サイズ変更が発生したので作り直す
				CreateRenderTexture( true ) ;

				m_Size.x = Size.x ;
				m_Size.y = Size.y ;
			}

			//---------------------------------

			if( m_TargetCamera != null )
			{
				if( m_FlexibleFieldOfView == true && m_BasisHeight >  0 )
				{
					// 画面の縦幅に応じて３Ｄカメラの画角を調整し３Ｄの見た目の大きさを画面の縦比に追従させる
					float fov = m_InitialFieldOfView * 0.5f ;
					float distance = ( m_BasisHeight * 0.5f ) / Mathf.Tan( 2.0f * Mathf.PI * fov / 360.0f ) ;
					float height = this.Height * 0.5f ;
					float tanV = height / distance ;
					fov = ( 360.0f * Mathf.Atan( tanV ) / ( 2.0f * Mathf.PI ) ) ;
					m_TargetCamera.fieldOfView = fov * 2.0f ;
				}
			}
		}

		private static readonly Rect m_FullRendering = new ( 0, 0, 1, 1 ) ;

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			//----------------------------------------------------------

			if( Application.isPlaying == true )
			{
				if( m_IsForceWorldLayer == true )
				{
					SetWorldLayer( m_WorldLayer ) ;
				}
			}

			SetupCamera() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 強制的に３Ｄ空間全体のレイヤーを設定する
		/// </summary>
		/// <param name="layer"></param>
		public void SetWorldLayer( int worldLayer )
		{
			m_WorldLayer = worldLayer ;

			int cullingMask = ( 1 << m_WorldLayer ) ;

			if( m_TargetCamera != null )
			{
				m_TargetCamera.cullingMask = cullingMask ;
			}

			if( m_WorldRoot != null )
			{
				SetLayerRecursively( m_WorldRoot, m_WorldLayer ) ;

				//----------------------------------------------------------
				// ライトがある場合に対象をレイヤーに絞る

				var lights = m_WorldRoot.GetComponentsInChildren<Light>( true ) ;
				if( lights != null && lights.Length >  0 )
				{
					foreach( var light in lights )
					{
						light.cullingMask = cullingMask ;
					}
				}
			}
		}

		// 子を全て含めてレイヤーマスクを設定する(Transform 版)
		private void SetLayerRecursively( Transform root, int layer )
		{
			root.gameObject.layer = layer ;

			foreach( Transform childTransform in root.transform )
			{
				SetLayerRecursively( childTransform.gameObject, layer ) ;
			}
		}

		// 子を全て含めてレイヤーマスクを設定する(GameObject 版)
		private void SetLayerRecursively( GameObject root, int layer )
		{
			root.layer = layer ;

			foreach( Transform childTransform in root.transform )
			{
				SetLayerRecursively( childTransform.gameObject, layer ) ;
			}
		}

		// ガメラの画角やビューポートの設定を行う
		private void SetupCamera()
		{
			if( m_TargetCamera == null )
			{
				// 不可
				return ;
			}

			//----------------------------------

			// RectTransform の位置に Camera を調整する
			if( m_RenderTextureEnabled == false )
			{
				// フレームバッファに直接表示
				var canvasSize = GetCanvasSize() ;
				var spaceRect = RectInCanvas ;

				float cw = canvasSize.x ;
				float ch = canvasSize.y ;

				// 画面の左下基準
				spaceRect.x += ( cw * 0.5f ) ;
				spaceRect.y += ( ch * 0.5f ) ;

				spaceRect.x      /= cw ;
				spaceRect.y      /= ch ;
				spaceRect.width  /= cw ;
				spaceRect.height /= ch ;

				m_TargetCamera.rect = spaceRect ;
			}
			else
			{
				// レンダーテクスチャに表示
				m_TargetCamera.rect = m_FullRendering ;
			}

			//---------------------------------

			if( m_CameraOffset.x == 0 && m_CameraOffset.y == 0 )
			{
				// オフセット位置の操作は無し

				m_TargetCamera.ResetProjectionMatrix() ;
//				Debug.LogWarning( "元の行列:\n" + m_TargetCamera.projectionMatrix ) ;
			}
			else
			{
				// オフセット位置の操作が有り

				float sw, sh ;
				if( m_RenderTexture == null )
				{
					sw = Screen.width ;
					sh = Screen.height ;
				}
				else
				{
					sw = m_RenderTexture.width ;
					sh = m_RenderTexture.height ;
				}

				m_TargetCamera.projectionMatrix = GetPerspectiveOffCenter( m_TargetCamera, sw, sh, m_CameraOffset ) ;
//				Debug.LogWarning( "今の行列:\n" + m_TargetCamera.projectionMatrix ) ;
			}
		}

		/// <summary>
		/// 強制更新
		/// </summary>
		public void Refresh()
		{
			Render() ;
		}

		/// <summary>
		/// レンダーテクスチャが有効な時に強制的にカメラの描画を実行する
		/// </summary>
		public void Render()
		{
			if( m_TargetCamera != null && m_RenderTexture != null && m_TargetCamera.targetTexture == m_RenderTexture )
			{
				m_TargetCamera.Render() ;

#if UNITY_EDITOR
				if( Application.isPlaying == false )
				{
					// Editor モード中のレンダーテクスチャの自動更新
					EditorUtility.SetDirty( m_RenderTexture ) ;
				}
#endif
			}
		}

		protected override void OnDestroy()
		{
			// RenderTexture も破棄する
			DeleteRenderTexture( false ) ;
		}

		//-------------------------------------------------------------------------------------------

		// レンダーテクスチャを生成する
		private void CreateRenderTexture( bool isResize )
		{
			//----------------------------------

			// 以前のレンダーテクスチャがあれば破棄する(これをしないとメモリリークしまくってヤバいことになる)
			DeleteRenderTexture( isResize ) ;

			//------------------------------------------------------------------------------------------

			if( m_TargetCamera == null )
			{
				// レンダリング対象のカメラが設定されていない
				return ;
			}

			//----------------------------------
			// RanderTexture の生成

			// 背景の塗りつぶし方法に関してはカメラの設定をそのまま使用する
//			m_TargetCamera.clearFlags = CameraClearFlags.SolidColor ;

			if( m_RenderTexture == null && this.Width >  0 && this.Height >  0 )
			{
				m_RenderTexture = new RenderTexture( ( int )this.Width, ( int )this.Height, 24 )
				{
					antiAliasing = 2,
					depth = 24	// 24以上にしないとステンシルがサポートされない事に注意する
				} ;
				m_RenderTexture.Create() ;
			}

			if( m_RenderTexture == null )
			{
				// 通常はここにはこない(Space の Width か Height が 0 だとレンダーテクスチャが生成されない)
				return ;
			}

			//----------------------------------
			// RanderTexture の表示用の RawImage の生成と設定

			if( m_RenderImage == null )
			{
				m_RenderImage = AddView<UIRawImage>( "RawImage(Auto Generated)" ) ;
				var rt = m_RenderImage.GetRectTransform() ;
				if( rt != null )
				{
					rt.hideFlags = HideFlags.NotEditable ;
				}
			}

			//--------------

			m_RenderImage.SetAnchorToStretch() ;

			m_TargetCamera.targetTexture = m_RenderTexture ;

			m_RenderImage.Texture = m_RenderTexture ;

			m_RenderImage.IsMask		= m_ImageMask ;
			m_RenderImage.IsInversion	= m_ImageInversion ;
			m_RenderImage.IsShadow		= m_ImageShadow ;
			m_RenderImage.IsOutline		= m_ImageOutline ;
			m_RenderImage.IsGradient	= m_ImageGradient ;


			//----------------------------------------------------------
			// 最初は強制レンダリングを行う

			SetupCamera() ;

			Render() ;
		}

		// レンダーテクスチャを破棄する
		private void DeleteRenderTexture( bool isResize )
		{
			if( m_TargetCamera != null )
			{
				m_TargetCamera.targetTexture = null ;
			}

			if( isResize == false && m_RenderImage != null )
			{
				m_RenderImage.Texture = null ;
				if( Application.isPlaying == true )
				{
					Destroy( m_RenderImage.gameObject ) ;
				}
				else
				{
					DestroyImmediate( m_RenderImage.gameObject ) ;
				}
				m_RenderImage = null ;
			}

			if( m_RenderTexture != null )
			{
				m_RenderTexture.Release() ;
//				if( Application.isPlaying == true )
//				{
//					Destroy( m_RenderTexture ) ;
//				}
//				else
//				{
//					DestroyImmediate( m_RenderTexture ) ;
//				}
				m_RenderTexture = null ;
			}

			// 背景の塗りつぶし方法に関してはカメラの設定をそのまま使用する
//			if( m_TargetCamera != null )
//			{
//				m_TargetCamera.clearFlags = CameraClearFlags.Depth ;
//			}
		}

		// カメラのオフセットをずらす場合の画角行列情報を計算する
		private Matrix4x4 GetPerspectiveOffCenter( Camera camera, float sw, float sh, Vector2 cameraOffset )
		{
			float xMin = ( camera.rect.xMin - 0.5f ) * 2.0f ;
			float xMax = ( camera.rect.xMax - 0.5f ) * 2.0f ;
			float yMin = ( camera.rect.yMin - 0.5f ) * 2.0f ;
			float yMax = ( camera.rect.yMax - 0.5f ) * 2.0f ;

			float fov   = camera.fieldOfView ;
			float ncp	= camera.nearClipPlane ;
			float fcp	= camera.farClipPlane ;

			//----------------------------------------------------------

			float vw = xMax - xMin ;
			float vh = yMax - yMin ;

			if( vw <= 0 || vh <= 0 )
			{
				return camera.projectionMatrix ;	// ０はダメよ
			}

			sw *= vw ;
			sh *= vh ;

			float ah = Mathf.Tan( 2.0f * Mathf.PI * fov / 360.0f ) ;
			float aw = ah * sh / sw ;

			//----------------------------------------------------------

			float x = aw ;
			float y = ah ;

			float a = ( xMax + xMin ) / vw ;
			float b = ( yMax + yMin ) / vh ;

			float c = - ( fcp + ncp ) / ( fcp - ncp ) ;
			float d = - ( 2.0f * fcp * ncp ) / ( fcp - ncp ) ;
			float e = - 1.0f ;

			var m = new Matrix4x4() ;
			m[ 0, 0 ] = x ;
			m[ 0, 1 ] = 0 ;
			m[ 0, 2 ] = a ;
			m[ 0, 3 ] = cameraOffset.x ;
			m[ 1, 0 ] = 0 ;
			m[ 1, 1 ] = y ;
			m[ 1, 2 ] = b ;
			m[ 1, 3 ] = cameraOffset.y ;
			m[ 2, 0 ] = 0 ;
			m[ 2, 1 ] = 0 ;
			m[ 2, 2 ] = c ;
			m[ 2, 3 ] = d ;
			m[ 3, 0 ] = 0 ;
			m[ 3, 1 ] = 0 ;
			m[ 3, 2 ] = e ;
			m[ 3, 3 ] = 0 ;

			//----------------------------------------------------------

			return m ;
		}
	}
}
