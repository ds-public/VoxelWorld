using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;


namespace ScreenSizeHelper
{
	/// <summary>
	/// SafeArea を考慮した Canvas の設定を行う Version 2023/12/17
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[DefaultExecutionOrder( -10 )]	// 通常よりも早く Update をコールさせる
	public class ViewportSizeFitterBase : MonoBehaviour
	{
		/// <summary>
		/// 横方向のアンカー種別
		/// </summary>
		public enum HorizontalAnchorTypes
		{
			Stretch,
			Left,
			Center,
			Right,
		}

		/// <summary>
		/// 縦方向のアンカー種別
		/// </summary>
		public enum VerticalAnchorTypes
		{
			Stretch,
			Bottom,
			Middle,
			Top,
		}

		/// <summary>
		/// ＦｏＶの指定方向
		/// </summary>
		public enum FovAxisTypes
		{
			Horizontal,
			Vertical,
		}

		//-----------------------------------------------------------


		[Header( "対象のカメラ" )]

		[SerializeField]
		protected Camera				m_Camera ;

		[Header( "セーフエリア対応が必要かどうか" )]

		[SerializeField]
		protected bool					m_SafeAreaEnabled = true ;

		//-----------------------------------
		// 注意
		// SafeAreaEnabled は、
		// ベースカメラとスタックされたオーバーレイカメラで、
		// 一律、同じ設定にしておく必要がある。
		//
		// スタックされたオーバーレイカメラで false、
		// すなわちセーフエリアの対応を行わないようにしても、
		// ベースカメラで true になっていると、
		// セーフエリアによるビューポート操作が、
		// スタックされたオーバーレイカメラにも適用されてしまう。
		//-----------------------------------


		/// <summary>
		/// セーフエリアの対応を行っているか
		/// </summary>
		public bool SafeAreaEnabed => m_SafeAreaEnabled ;

		[Header( "細かい表示位置指定" )]

		[SerializeField]
		protected HorizontalAnchorTypes	m_HorizontalAnchorType = HorizontalAnchorTypes.Stretch ;

		[SerializeField]
		protected float					m_MarginL ;

		[SerializeField]
		protected float					m_MarginR ;

		[SerializeField]
		protected float					m_OffsetX ;

		[SerializeField]
		protected float					m_Width ;

		[SerializeField]
		protected VerticalAnchorTypes	m_VerticalAnchorType = VerticalAnchorTypes.Stretch ;

		[SerializeField]
		protected float					m_MarginB ;

		[SerializeField]
		protected float					m_MarginT ;

		[SerializeField]
		protected float					m_OffsetY ;

		[SerializeField]
		protected float					m_Height ;

		// カメラの調整

		// Perspective モードでＦｏＶを自動調整したい場合の基準値の方向
		[SerializeField]
		protected FovAxisTypes			m_CorrectingFieldOfViewAxisType = FovAxisTypes.Vertical ;

		// Perspective モードでＦｏＶを自動調整したい場合の基準値
		[SerializeField]
		protected float					m_CorrectingFieldOfView = 0 ;

		// Orthographic モードでサイズを自動調整したい場合の基準値
		[SerializeField]
		protected float					m_CorrectingOrthographicSize = 0 ;

		//-----------------------------------

		[Header( "基準解像度 ※ReadOnly" )]

		// 基準解像度
		[SerializeField]
		protected float					m_BasicWidth ;

		[SerializeField]
		protected float					m_BasicHeight ;


		[Header( "最大解像度 ※ReadOnly" )]

		// 最大解像度
		[SerializeField]
		protected float					m_LimitWidth ;

		[SerializeField]
		protected float					m_LimitHeight ;


		[Header( "現在の画面解像度 ※ReadOnly" )]

		[SerializeField]
		protected float					m_ScreenWidth ;

		[SerializeField]
		protected float					m_ScreenHeight ;


		// SafeArea の変化確認用(width height で確認してはダメ)
		[Header( "現在の SafeArea [Screen座標系] ※ReadOnly")]

		[SerializeField]
		protected float					m_SafeArea_xMin ;

		[SerializeField]
		protected float					m_SafeArea_xMax ;

		[SerializeField]
		private float					m_SafeArea_yMin ;

		[SerializeField]
		private float					m_SafeArea_yMax ;


		[Header( "SafeArea による Padding [Canvas座標系] ※ReadOnly" )]

		[SerializeField]
		protected float					m_PaddingL ;

		[SerializeField]
		protected float					m_PaddingR ;

		[SerializeField]
		protected float					m_PaddingB ;

		[SerializeField]
		protected float					m_PaddingT ;


		//-----------------------------------

		// 更新が必要かどうか
		protected bool					m_IsDirty = false ;


		//-----------------------------------

		internal void Awake()
		{
			//----------------------------------
			// 解像度情報を取得する

			// 基準解像度
			m_BasicWidth	= 1080 ;
			m_BasicHeight	= 1920 ;

			m_LimitWidth	= 1440 ;
			m_LimitHeight	= 1920 ;

			// 継承クラスの Awake() 呼び出し
			OnAwake() ;

			//----------------------------------------------------------

			if( m_Camera == null )
			{
				// カメラの指定が無ければ同じノードからの取得を試みる

				TryGetComponent<Camera>( out m_Camera ) ;
			}
		}

		// 継承クラスで基準解像度と最大解像度を設定してもらう
		protected virtual void OnAwake(){}

		/// <summary>
		/// 継承クラスで基準解像度と最大解像度を設定する
		/// </summary>
		/// <param name="basicWidth"></param>
		/// <param name="basicHeight"></param>
		/// <param name="limitWIdth"></param>
		/// <param name="limitHeight"></param>
		protected void SetResolution( float basicWidth, float basicHeight, float limitWIdth, float limitHeight )
		{
			m_BasicWidth	= basicWidth ;
			m_BasicHeight	= basicHeight ;

			m_LimitWidth	= limitWIdth ;
			m_LimitHeight	= limitHeight ;
		}

		//-------------------------------------------------------------------------------------------

		internal void Start()
		{
			if( m_Camera == null )
			{
				// 最低限 Camera が必要
				return ;
			}

			//----------------------------------------------------------

			Refresh() ;
		}

		internal void Update()
		{
			if( m_Camera == null )
			{
				// 最低限 Camera が必要
				return ;
			}

			//----------------------------------------------------------

			// 変更を確認する
			CheckUpdate() ;

			// 必要に応じてカメラのビューポートを更新する
			if( m_IsDirty == true )
			{
				Refresh() ;
			}
		}

		// 変更を確認する
		private void CheckUpdate()
		{
			// セーフエリアの情報を取得する
			var safeArea = GetSafeArea() ;

			if
			(
				// 画面の回転など
				m_ScreenWidth	!= Screen.width		||
				m_ScreenHeight	!= Screen.height	||
				// SafeArea の動的変化
				m_SafeArea_xMin	!= safeArea.xMin	||
				m_SafeArea_xMax	!= safeArea.xMax	||
				m_SafeArea_yMin	!= safeArea.yMin	||
				m_SafeArea_yMax	!= safeArea.yMax
			)
			{
				// OnRectTransformDimensionsChange は呼ばれない事がある
				m_IsDirty = true ;
			}
		}

		// カメラのビューポートを更新する
		private void Refresh()
		{
			if( m_Camera == null )
			{
				// 最低限 Camera が必要
				return ;
			}

			//------------------------------------------------------------------------------------------

			// Screen のサイズを更新する
			m_ScreenWidth	= Screen.width ;
			m_ScreenHeight	= Screen.height ;

			// セーフエリアの情報を取得する
			var safeArea = GetSafeArea() ;

			// SafeArea を更新する
			m_SafeArea_xMin	= safeArea.xMin ;
			m_SafeArea_xMax	= safeArea.xMax ;
			m_SafeArea_yMin	= safeArea.yMin ;
			m_SafeArea_yMax	= safeArea.yMax ;

			//------------------------------------------------------------------------------------------

			float requiredBasicWidth ;
			float requiredBasicHeight ;

			float safeAreaL ;
			float safeAreaR ;
			float safeAreaB ;
			float safeAreaT ;

			float canvasWidth ;
			float canvasHeight ;

			float canvasRatioX ;
			float canvasRatioY ;

			float safeAreaRatioX ;
			float safeAreaRatioY ;

			bool  isSafeAreaOver ;

			float viewportW ;
			float viewportH ;

			//------------------------------------------------------------------------------------------

			// パディング(Canvas座標系単位)を一度初期化する
			m_PaddingL = 0 ;
			m_PaddingR = 0 ;
			m_PaddingB = 0 ;
			m_PaddingT = 0 ;

			// CanvasScaler に設定する基準解像度
			requiredBasicWidth  = m_BasicWidth ;
			requiredBasicHeight = m_BasicHeight ;

			// 通常のキャンバスサイズを取得する
			( canvasWidth, canvasHeight ) = GetCanvasSize( requiredBasicWidth, requiredBasicHeight ) ;

			//----------------------------------

			// 通常のキャンバスサイズでアウターフレームでセーフエリアがまかなえるか確認する

			if( m_SafeAreaEnabled == true )
			{
				// セーフエリアを処理する
				
				// Screen 座標系によるセーフエリアの各サイズ
				safeAreaL =                  m_SafeArea_xMin ;
				safeAreaR = m_ScreenWidth  - m_SafeArea_xMax ;
				safeAreaB =                  m_SafeArea_yMin ;
				safeAreaT = m_ScreenHeight - m_SafeArea_yMax ;

				float outerL = 0 ;
				float outerR = 0 ;
				float outerB = 0 ;
				float outerT = 0 ;

				if( canvasWidth  >  m_LimitWidth  )
				{
					outerL = outerR = ( canvasWidth  - m_LimitWidth  ) * 0.5f ;
				}
				if( canvasHeight >  m_LimitHeight )
				{
					outerB = outerT = ( canvasHeight - m_LimitHeight ) * 0.5f ;
				}

				// Canvas 座標系でのセーフエリアの量を算出する
				m_PaddingL = canvasWidth  * safeAreaL / m_ScreenWidth  ;
				m_PaddingR = canvasWidth  * safeAreaR / m_ScreenWidth  ;
				m_PaddingB = canvasHeight * safeAreaB / m_ScreenHeight ;
				m_PaddingT = canvasHeight * safeAreaT / m_ScreenHeight ;

				m_PaddingL -= outerL ;
				if( m_PaddingL <  0 )
				{
					m_PaddingL  = 0 ;
				}

				m_PaddingR -= outerR ;
				if( m_PaddingR <  0 )
				{
					m_PaddingR  = 0 ;
				}

				m_PaddingB -= outerB ;
				if( m_PaddingB <  0 )
				{
					m_PaddingB  = 0 ;
				}

				m_PaddingT -= outerT ;
				if( m_PaddingT <  0 )
				{
					m_PaddingT  = 0 ;
				}

				//----------------------------------

				if( m_PaddingL >  0 || m_PaddingR >  0 || m_PaddingB >  0 || m_PaddingT >  0 )
				{
					// セーフエリアが存在する

					// 基準解像度に対するキャンバスの余剰部分の占める割合
					canvasRatioX = ( canvasWidth  / requiredBasicWidth  ) - 1.0f ;
					canvasRatioY = ( canvasHeight / requiredBasicHeight ) - 1.0f ;

					// 基準解像度に対してセーフエリアを加味した場合の必要解像度比率

					// 画面全体に対するセーフエリアが占める割合
					safeAreaRatioX = ( ( safeAreaL + safeAreaR ) / m_ScreenWidth  ) ;
					safeAreaRatioY = ( ( safeAreaB + safeAreaT ) / m_ScreenHeight ) ;

					//--------------------------------------------------------
					// 注意
					// 以下の基準解像度の補正処理は、
					// セーフエリアによって解像度が基準解像度未満に下がる場合のみ適用される。
					// 実キャンバス解像度から基準解像度を減算し、
					// セーフエリア以上の余剰領域が存在する場合、
					// 基準解像度の補正は行わず、
					// SafeAreaCorrector のサイズ調整により、
					// SafeArea の対応を行う。
					//
					// SafeAreaCorrector のサイズ = 実キャンバス解像度 - セーフエリア解像度
					//--------------------------------------------------------

					isSafeAreaOver = false ;

					if( safeAreaRatioX >  0 && safeAreaRatioX <  1 && canvasRatioX <  safeAreaRatioX )
					{
						// 横方向に関して CanvasScale の設定解像度を調整する必要がある

						requiredBasicWidth /= ( 1.0f - safeAreaRatioX ) ;
						isSafeAreaOver = true ;
					}

					if( safeAreaRatioY >  0 && safeAreaRatioY <  1 && canvasRatioY <  safeAreaRatioY )
					{
						// 縦方向に関して CanvasScale の設定解像度を調整する必要がある
						requiredBasicHeight /= ( 1.0f - safeAreaRatioY ) ;
						isSafeAreaOver = true ;
					}

					if( isSafeAreaOver == true )
					{
						// 基準解像度に補正が入るためキャンバスサイズを取得し直す
						( canvasWidth, canvasHeight ) = GetCanvasSize( requiredBasicWidth, requiredBasicHeight ) ;
					}

					//-------------

					// この時点での canvasWidth canvasHeight がビューポート全体(0,0)-(1,1)に対応する解像度となる

					// キャンバスサイズが変化したので Canvas 座標系でのセーフエリアの量を再度算出する
					m_PaddingL = canvasWidth  * safeAreaL / m_ScreenWidth  ;
					m_PaddingR = canvasWidth  * safeAreaR / m_ScreenWidth  ;
					m_PaddingB = canvasHeight * safeAreaB / m_ScreenHeight ;
					m_PaddingT = canvasHeight * safeAreaT / m_ScreenHeight ;

					// SafeAreaCorrector のサイズ
					viewportW = canvasWidth  - m_PaddingL - m_PaddingR ;
					viewportH = canvasHeight - m_PaddingB - m_PaddingT ;
				}
				else
				{
					// セーフエリアは存在しない
					viewportW = canvasWidth ;
					viewportH = canvasHeight ;
				}
			}
			else
			{
				// セーフエリアは処理しない
				viewportW = canvasWidth ;
				viewportH = canvasHeight ;
			}

			//----------------------------------

			// セーフエリアを処理した場合は Padding に 0 より大きい値が入っている場合がある
			float viewportX = m_PaddingL ;
			float viewportY = m_PaddingB ;

			// ビューポート解像度が最大解像度と超えていた場合に制限をかける

			if( viewportW >  m_LimitWidth )
			{
				viewportX += ( ( viewportW - m_LimitWidth  ) * 0.5f ) ;
				viewportW   = m_LimitWidth  ;
			}

			if( viewportH >  m_LimitHeight )
			{
				viewportY += ( ( viewportH - m_LimitHeight ) * 0.5f ) ;
				viewportH  = m_LimitHeight  ;
			}

			//----------------------------------------------------------
			// 注意
			// カメラ(２Ｄ・３Ｄ)の描画は、
			// 最大解像度のアスペクトを限界して制限がかかる。
			// (つまり、それ以上は画角も増えない←画角をアスペクトに応じて可変にしているならば)
			//
			// ただし、セーフエリアに関しては、YES とも NO とも言える。
			//
			// SafeAreaEnabled を true とした場合、
			// セーフエリアには描画しないように制限がかかる。
			//
			// SafeAreaEnabled を false とした場合、
			// セーフエリアに対して制限を行わないため描画は行われる。
			// セーフエリアに、多数の透過部分がある場合、
			// そこを見せるため、あえて false とするという手もある。

			//----------------------------------------------------------

			// さらにビューポートの範囲内で範囲指定があれば適用する

			// 横方向
			switch( m_HorizontalAnchorType )
			{
				// 横は引き伸ばし
				case HorizontalAnchorTypes.Stretch :
					if( ( m_MarginL + m_MarginR ) <  viewportW )
					{
						viewportX += m_MarginL ;
						viewportW -= ( m_MarginL + m_MarginR ) ;
					}
				break ;

				// 横は左寄せ
				case HorizontalAnchorTypes.Left :
					if( m_OffsetX >= 0 && ( m_OffsetX + m_Width  ) <= viewportW )
					{
						viewportX += m_OffsetX ;
						viewportW  = m_Width  ;
					}
				break ;

				// 横は中央
				case HorizontalAnchorTypes.Center :
					if( m_Width  <= viewportW  )
					{
						float offsetLimitX = ( viewportW  - m_Width  ) * 0.5f ;
						if( Mathf.Abs( m_OffsetX ) <= offsetLimitX )
						{
							viewportX += ( offsetLimitX + m_OffsetX ) ;
							viewportW  = m_Width  ;
						}
					}
				break ;

				// 横は右寄せ
				case HorizontalAnchorTypes.Right :
					if( m_OffsetX <= 0 && ( m_Width  - m_OffsetX ) <= viewportW )
					{
						viewportX += ( viewportW - m_Width  + m_OffsetX ) ;
						viewportW  = m_Width ;
					}
				break ;
			}

			// 縦方向
			switch( m_VerticalAnchorType )
			{
				// 縦は引き伸ばし
				case VerticalAnchorTypes.Stretch :
					if( ( m_MarginB + m_MarginT ) <  viewportH )
					{
						viewportY += m_MarginB ;
						viewportH -= ( m_MarginB + m_MarginT ) ;
					}
				break ;

				// 縦は下寄せ
				case VerticalAnchorTypes.Bottom :
					if( m_OffsetY >= 0 && ( m_OffsetY + m_Height ) <= viewportH )
					{
						viewportY += m_OffsetY ;
						viewportH  = m_Height ;
					}
				break ;

				// 縦は中央
				case VerticalAnchorTypes.Middle :
					if( m_Height <= viewportH )
					{
						float offsetLimitY = ( viewportH - m_Height ) * 0.5f ;
						if( Mathf.Abs( m_OffsetY ) <= offsetLimitY )
						{
							viewportY += ( offsetLimitY + m_OffsetY ) ;
							viewportH  = m_Height ;
						}
					}
				break ;

				// 縦は上寄せ
				case VerticalAnchorTypes.Top :
					if( m_OffsetY <= 0 && ( m_Height - m_OffsetY ) <= viewportH )
					{
						viewportY += ( viewportH - m_Height + m_OffsetY ) ;
						viewportH  = m_Height ;
					}
				break ;
			}

			//--------------
			// 表示位置確定(ビューポート値をキャンバス座標系からビューポート座標系に変換する)

			float vx = viewportX / canvasWidth  ;
			float vw = viewportW / canvasWidth  ;

			float vy = viewportY / canvasHeight ;
			float vh = viewportH / canvasHeight ;

			m_Camera.rect = new Rect( vx, vy, vw, vh ) ;

			//----------------------------------------------------------
			// 投影の自動調整

			if( m_VerticalAnchorType == VerticalAnchorTypes.Stretch )
			{
				// 縦方向がストレッチの場合のみ投影の自動調整は有効になる
				if( m_Camera.orthographic == false )
				{
					// 投射投影

					if( m_CorrectingFieldOfView >  0 )
					{
						// ０よりも大きい値が設定されている場合のみ自動調整は有効

						if( m_CorrectingFieldOfViewAxisType == FovAxisTypes.Vertical )
						{
							// 縦
							float distance = ( m_BasicHeight * 0.5f ) / Mathf.Tan( Mathf.PI * m_CorrectingFieldOfView * 0.5f / 180.0f ) ;

							m_Camera.fieldOfView  = 2.0f * ( 180.0f * Mathf.Atan( ( viewportH * 0.5f ) / distance ) / Mathf.PI ) ;
						}
						else
						{
							// 横
							float distance = ( m_BasicWidth  * 0.5f ) / Mathf.Tan( Mathf.PI * m_CorrectingFieldOfView * 0.5f / 180.0f ) ;

							m_Camera.fieldOfView = 2.0f * ( 180.0f * Mathf.Atan( ( viewportH * 0.5f ) / distance ) / Mathf.PI ) ;
						}
					}
				}
				else
				{
					// 平行投影
					if( m_CorrectingOrthographicSize >  0 )
					{
						// ０よりも大きい値が設定されている場合のみ自動調整は有効
						m_Camera.orthographicSize = m_CorrectingOrthographicSize * viewportH / m_BasicHeight ;
					}
				}
			}

			//----------------------------------------------------------

			// 更新終了
			m_IsDirty = false ;
		}

		//-----------------------------------

		// キャンバスのサイズを計算する
		private ( float, float ) GetCanvasSize( float basicWidth, float basicHeight )
		{
			float screenWidth  = Screen.width ;
			float screenHeight = Screen.height ;

			float canvasWidth ;
			float canvasHeight ;

			if( screenWidth  >= screenHeight )
			{
				// 画面は横長
				float screenAspect = screenWidth  / screenHeight ;
				float basicAspect  = basicWidth   / basicHeight ;

				if( screenAspect >  basicAspect )
				{
					// より横長　→　基準は縦幅
					canvasHeight = basicHeight ;
					canvasWidth  = basicHeight * screenWidth  / screenHeight ;
				}
				else
				if( screenAspect <  basicAspect )
				{
					// 基準より少ない横長　→　基準は横幅
					canvasWidth  = basicWidth ;
					canvasHeight = basicWidth  * screenHeight / screenWidth ; 
				}
				else
				{
					// 丁度
					canvasWidth  = basicWidth ;
					canvasHeight = basicHeight ;
				}
			}
			else
			{
				// 画面は縦長
				float screenAspect = screenHeight  / screenWidth ;
				float baseAspect   = basicHeight   / basicWidth ;

				if( screenAspect >  baseAspect )
				{
					// より縦長　→　基準は横幅
					canvasWidth  = basicWidth ;
					canvasHeight = basicWidth  * screenHeight / screenWidth ;
				}
				else
				if( screenAspect <  baseAspect )
				{
					// 基準より少ない縦長　→　基準は縦幅
					canvasHeight = basicHeight ;
					canvasWidth  = basicHeight * screenWidth  / screenHeight ;
				}
				else
				{
					// 丁度
					canvasWidth  = basicWidth ;
					canvasHeight = basicHeight ;
				}
			}

			return ( canvasWidth, canvasHeight ) ;
		}

		/// <summary>
		/// セーフエリアの情報が必要な際に呼び出される(継承して正しいセーフエリア情報を返すようにする必要がある)
		/// </summary>
		/// <returns></returns>
		protected virtual Rect GetSafeArea()
		{
			return Screen.safeArea ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンポーネントが Enabled になる際に呼び出される
		/// </summary>
		internal void OnEnable()
		{
			m_IsDirty = true ;
		}

		/// <summary>
		/// コンポーネントが Disabled になる際に呼び出される
		/// </summary>
		internal void OnDisable()
		{
			m_IsDirty = true ;
		}

#if UNITY_EDITOR
		/// <summary>
		/// (Inspectorで)プロパティの編集が行われた際に呼び出される ※m_SafeAreaEnabled の変化の即時反映
		/// </summary>
		internal void OnValidate()
		{
			Refresh() ;
		}
#endif
	}
}

