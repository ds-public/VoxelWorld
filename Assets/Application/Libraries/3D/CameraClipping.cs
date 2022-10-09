//#define USE_URP
using System ;
using System.Collections ;

using UnityEngine ;

#if USE_URP
using UnityEngine.Rendering.Universal ;
#endif


[ RequireComponent( typeof( Camera ) ) ]
	
[ ExecuteInEditMode ]


/// <summary>
/// カメラの表示領域をクリッピングするコンポーネントクラス Version 2021/06/23 0
/// </summary>
public class CameraClipping : MonoBehaviour
{
	/// <summary>
	/// 縦方向の視野角
	/// </summary>
	[Header("縦方向の視野角です。物理カメラを使用しない場合は設定が必要です。")]
	public float	FieldOfView = 60f ;

	/// <summary>
	/// 画面の仮想解像度
	/// </summary>
	[Header("固定アスペクト比を算出するための仮の解像度を設定します。")]
	public Vector2	ScreenSize = new Vector2( 480, 480 ) ;

	/// <summary>
	/// ビューポートによるクリッピングを行うかどうか
	/// </summary>
	[Header("固定アスペクト比領域内でのビューポートによるクリッピングを行うかどうかを指定します(URP環境下では無効)")]
	public bool		UseViewport = false ;

	/// <summary>
	/// 画面に対する表示領域
	/// </summary>
	[Header("固定アスペクト比領域内でのビューポートの仮の解像度を設定します(ビューポート不使用・URP環境下では無効)")]
	public Rect		ScreenView = new Rect(   0,   0, 480, 480 ) ;
	
	/// <summary>
	/// スクリーンに対する配置方法の定義
	/// </summary>
	public enum ScreenMatchModes
	{
		Expand = 0,		// 実画面の中心
		Width  = 1,		// 横にのみ合わせる
		Height = 2,		// 縦にのみ合わせる
	}

	/// <summary>
	/// スクリーンに対する配置方法
	/// </summary>
	[Header("実画面に対する固定アスペクト比領域の配置方法を指定します。")]
	public ScreenMatchModes ScreenMatchMode = ScreenMatchModes.Expand ;

	//----------------------------------------------------------------------------

	// 対象のカメラのインスタンス
	private Camera m_Camera = null ;

#if USE_URP
	// URPのカメラデータ
	private UniversalAdditionalCameraData m_CameraData = null ;
#endif
	//----------------------------------------------------------------------------

	// カメラ情報を取得する
	private void GetCamera()
	{
		m_Camera = GetComponent<Camera>() ;

#if USE_URP
		if( m_Camera != null )
		{
			// URP 環境下であるかの判定用
			m_CameraData = m_Camera.GetUniversalAdditionalCameraData() ;
		}
#endif           
	}


	internal void Awake()
	{
		if( Application.isPlaying == true )
		{
			GetCamera() ;
			if( enabled == true )
			{
				Process() ;	// 最速での実行が安全
			}
		}
	}

	internal void Start()
	{
		GetCamera() ;
	}

	internal void Update()
	{
		Process() ;
	}

	/// <summary>
	/// 強制的にビューポートを更新する
	/// </summary>
	public void Refresh()
	{
		Process() ;
	}

	private void Process()
	{
		if( m_Camera != null && ScreenSize.x >  0 && ScreenSize.y >  0 )
		{
			UpdateCameraViewport( m_Camera, ScreenSize.x, ScreenSize.y, ScreenView.x, ScreenView.y, ScreenView.width, ScreenView.height ) ;
        }
	}

	//--------------------------------------------------------------------------------------------

	// 固定アスペクト比を設定
	private void UpdateCameraViewport( Camera targetCamera, float screenSizeW, float screenSizeH, float screenViewX, float screenViewY, float screenViewW, float screenViewH )
	{
		if( targetCamera == null )
		{
			return ;
		}

		float realScreenSizeW ;
		float realScreenSizeH ;
		
		if( targetCamera.targetTexture == null )
		{
			realScreenSizeW = Screen.width ;
			realScreenSizeH = Screen.height ;
		}
		else
		{
			// バックバッファのサイズを基準にする必要がある
			realScreenSizeW = targetCamera.targetTexture.width ;
			realScreenSizeH = targetCamera.targetTexture.height ;
		}

		float sx, sy, sw, sh ;

		// 想定アスペクト比(仮想解像度)　値を書き換えるので念の為複製する
		float aspectW = screenSizeW ;
		float aspectH = screenSizeH ;

		float vw, vh ;

		// 仮想解像度での表示領域　値を書き換えるので念の為複製する
		float viewX = screenViewX ;
		float viewY = screenViewY ;
		float viewW = screenViewW ;
		float viewH = screenViewH ;

		float rx, ry, rw, rh ;

		sx = 0 ;
		sy = 0 ;
		sw = 1 ;
		sh = 1 ;

		if( ( realScreenSizeH / realScreenSizeW ) >= ( aspectH / aspectW ) )
		{
			// 縦の方が長い(横はいっぱい表示)

//			Debug.LogWarning( "縦の方が長い" ) ;

			vw = aspectW ;
			vh = aspectH ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

			if( ScreenMatchMode == ScreenMatchModes.Expand )
			{
				// 常にアスペクトを維持する(Expand)　縦に隙間が出来る
				sx = 0 ;
				sw = 1 ;
				sh = realScreenSizeW * vh / vw ;
				sh /= realScreenSizeH ;
				sy = ( 1.0f - sh ) * 0.5f ;
			}
			else
			if( ScreenMatchMode == ScreenMatchModes.Width )
			{
				// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が増加
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				aspectH = aspectW * realScreenSizeH / realScreenSizeW ;
			}
			else
			if( ScreenMatchMode == ScreenMatchModes.Height )
			{
				// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が減少
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				aspectW = aspectH * realScreenSizeW / realScreenSizeH ;

				// 解像度が減少した分表示位置を移動させる
				viewX -= ( ( vw - aspectW ) * 0.5f ) ;
			}
		}
		else
		{
			// 横の方が長い(縦はいっぱい表示)

//			Debug.LogWarning( "横の方が長い" ) ;

			vh = aspectH ;
			vw = aspectW ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

			if( ScreenMatchMode == ScreenMatchModes.Expand )
			{
				// 常にアスペクトを維持する(Expand)　横に隙間が出来る
				sy = 0 ;
				sh = 1 ;
				sw = realScreenSizeH * vw / vh ;
				sw /= realScreenSizeW ;
				sx = ( 1.0f - sw ) * 0.5f ;
			}
			else
			if( ScreenMatchMode == ScreenMatchModes.Height )
			{
				// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が増加
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				aspectW = aspectH * realScreenSizeW / realScreenSizeH ;
			}
			else
			if( ScreenMatchMode == ScreenMatchModes.Width )
			{
				// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が減少
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				aspectH = aspectW * realScreenSizeH / realScreenSizeW ;

				// 解像度が減少した分表示位置を移動させる
				viewY -= ( ( vh - aspectH ) * 0.5f ) ;
			}
		}

		//----------------------------------------------------------

		if( viewW >  0 )
		{
			rx = viewX / aspectW ;
			rw = viewW / aspectW ;
		}
		else
		{
			// ０以下ならフル指定
			rx = 0 ;
			rw = 1 ;
		}

		if( viewH >  0 )
		{
			ry = viewY / aspectH ;
			rh = viewH / aspectH ;
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

//		Debug.LogWarning( "sx:" + sx + " sw:" + sw + " rx:" + rx + " rw:" + rw ) ;
//		Debug.LogWarning( "sy:" + sy + " sh:" + sh + " ry:" + ry + " rh:" + rh ) ;

		//-------------------------------------------------------------------------------------------

		bool useViewport = UseViewport ;

#if USE_URP
		if( m_CameraData != null )
		{
			// URP環境下ではビューポートは使用できない
			useViewport = false ;
		}
#endif

		if( useViewport == false )
		{
			// ビューポートを使用しないか URP を使用している(ビューポートの設定が実質できない)
			float aspectRatio = realScreenSizeW / realScreenSizeH ;

			// 実画面のアスペクト比をカメラに設定する(超重要)
			m_Camera.aspect = aspectRatio ;

			if( m_Camera.usePhysicalProperties == false )
			{
				// 物理カメラ不使用
				if( sh == 1.0f )
				{
					// 実画面は想定のアスペクト比より横長
					m_Camera.fieldOfView = FieldOfView ;
				}
				else
				{
					// 実画面は想定のアスペクト比より横長
					float t = Mathf.Tan( Mathf.PI * FieldOfView / 360f ) * aspectW / aspectH ;
					float a = 360f * Mathf.Atan( t ) / Mathf.PI ;

					m_Camera.fieldOfView = Camera.HorizontalToVerticalFieldOfView( a, aspectRatio ) ;
				}
			}
		}
		else
		{
			// ビューポートを使用し且つ URP を使用していない
			targetCamera.rect = new Rect( sx + ( rx * sw ), sy + ( ry * sh ), rw * sw, rh * sh ) ;
		}
	}
}
