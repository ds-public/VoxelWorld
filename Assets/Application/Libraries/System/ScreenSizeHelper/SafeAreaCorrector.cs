using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.EventSystems ;


namespace ScreenSizeHelper
{
	/// <summary>
	/// SafeArea を考慮した Canvas の設定を行う(Canvas に AddComponent して使用する) Version 2024/02/22
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( RectTransform ) )]
	[RequireComponent( typeof( CanvasScaler ) )]
	[DefaultExecutionOrder( -10 )]	// 通常よりも早く Update をコールさせる
	public sealed class SafeAreaCorrector : UIBehaviour
	{
		[Header( "セーフエリアの部分に外枠を表示しないようにするか ※ReadOnly" )]

		// 基準解像度
		[SerializeField]
		private bool					m_SafeAreaVisible ;

		/// <summary>
		/// セーフエリアの部分に外枠を表示しないようにするか
		/// </summary>
		public bool SafeAreaVisible
		{
			get
			{
				return m_SafeAreaVisible ;
			}
			set
			{
				if( m_SafeAreaVisible != value )
				{
					m_SafeAreaVisible = value ;
					m_IsDirty = true ;
				}
			}
		}

		[Header( "基準解像度 ※ReadOnly" )]

		// 基準解像度
		[SerializeField]
		private float					m_BasicWidth ;

		[SerializeField]
		private float					m_BasicHeight ;


		[Header( "最大解像度 ※ReadOnly" )]

		// 最大解像度
		[SerializeField]
		private float					m_LimitWidth ;

		[SerializeField]
		private float					m_LimitHeight ;


		[Header( "現在の画面解像度 ※ReadOnly" )]

		[SerializeField]
		private float					m_ScreenWidth ;

		[SerializeField]
		private float					m_ScreenHeight ;


		// SafeArea の変化確認用(width height で確認してはダメ)
		[Header( "現在の SafeArea [Screen座標系] ※ReadOnly")]

		[SerializeField]
		private float					m_SafeArea_xMin ;

		[SerializeField]
		private float					m_SafeArea_xMax ;

		[SerializeField]
		private float					m_SafeArea_yMin ;

		[SerializeField]
		private float					m_SafeArea_yMax ;


		[Header( "SafeArea による Padding [Canvas座標系] ※ReadOnly" )]

		[SerializeField]
		private float					m_PaddingL ;

		[SerializeField]
		private float					m_PaddingR ;

		[SerializeField]
		private float					m_PaddingB ;

		[SerializeField]
		private float					m_PaddingT ;


		[Header( "SafeArea 補正用の RectTransform ※ReadOnly" )]

		[SerializeField]
		private RectTransform			m_SafeAreaCorrector ;

		//-----------------------------------

		// SafeAreaCorrector の RectTransform の更新が必要かどうか
		private bool					m_IsDirty = false ;

		//-----------------------------------------------------------

		// Canvas の Canvas
		private Canvas					m_Canvas ;

		// Canvas の CanvasScaler
		private CanvasScaler			m_CanvasScaler ;

		// カメラに付いた ViewportSizeFitter 側で SafeArea の処理が行われているか
		private bool					m_SafeAreaEnabledInViewportSizeFitter ;

		// セーフエリアの情報が必要な際に呼び出される
		private Func<Rect>				m_OnSafeAreaGet ;

		//--------------------------------------------------------------

		protected override void Awake()
		{
			base.Awake() ;

			//----------------------------------

			// CanvasScaler を取得する(同 GameObject に Canvas が付いている前提)
			TryGetComponent<Canvas>( out m_Canvas ) ;
			if( m_Canvas != null )
			{
				if( m_Canvas.renderMode != RenderMode.ScreenSpaceOverlay && m_Canvas.worldCamera != null )
				{
					// カメラが設定されている
					var camera = m_Canvas.worldCamera ;
					if( camera.gameObject.TryGetComponent<ViewportSizeFitterBase>( out var viewportSizeFitter ) == true )
					{
						m_SafeAreaEnabledInViewportSizeFitter = viewportSizeFitter.SafeAreaEnabed ;
					}
				}
			}

			// CanvasScaler を取得する(同 GameObject に Canvas が付いている前提)
			TryGetComponent<CanvasScaler>( out m_CanvasScaler ) ;
		}

		/// <summary>
		/// 基準解像度と最大解像度を設定する
		/// </summary>
		/// <param name="basicWidth"></param>
		/// <param name="basicHeight"></param>
		/// <param name="limitWIdth"></param>
		/// <param name="limitHeight"></param>
		public void SetResolution( float basicWidth, float basicHeight, float limitWIdth, float limitHeight, Func<Rect> onSafeAreaGet )
		{
			m_BasicWidth	= basicWidth ;
			m_BasicHeight	= basicHeight ;

			m_LimitWidth	= limitWIdth ;
			m_LimitHeight	= limitHeight ;

			m_OnSafeAreaGet = onSafeAreaGet ;
		}

		// SafeAreaCorrector を生成する
		private void CreateSafeAreaCorrector()
		{
			if( m_SafeAreaCorrector != null )
			{
				// 既に生成済みになっている
				return ;
			}

			//----------------------------------------------------------
			// Canvas の子を全て取得する

			var children = new List<Transform>() ;

			int i, l = transform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				children.Add( transform.GetChild( i ) ) ;
			}

			//----------------------------------------------------------
			// SafeAreaCorrector(RectTransform)を生成する 

			var safeAreaCorrector = new GameObject( "SafeAreaCorrector" ) ;
			safeAreaCorrector.transform.SetParent( transform, false ) ;

			m_SafeAreaCorrector = safeAreaCorrector.AddComponent<RectTransform>() ;

			// ここは固定設定(Anchor=Center・Pivot=Center)
			m_SafeAreaCorrector.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			m_SafeAreaCorrector.anchoredPosition	= Vector2.zero ;
			m_SafeAreaCorrector.localScale			= Vector3.one ;
			m_SafeAreaCorrector.anchorMin			= new Vector2( 0.5f, 0.5f ) ;
			m_SafeAreaCorrector.anchorMax			= new Vector2( 0.5f, 0.5f ) ;
			m_SafeAreaCorrector.pivot				= new Vector2( 0.5f, 0.5f ) ;

			//----------------------------------------------------------
			// 親を繋ぎ変える

			for( i  = 0 ; i <  l ; i ++ )
			{
				children[ i ].SetParent( m_SafeAreaCorrector, false ) ;
			}

			//----------------------------------------------------------

			m_SafeAreaCorrector.hideFlags = HideFlags.NotEditable ;
		}

		// SafeAreaCorrector を破棄する
		private void DeleteSafeAreaCorrector()
		{
			if( m_SafeAreaCorrector == null )
			{
				// 既に破棄済みになっている
				return ;
			}

			//----------------------------------------------------------
			// 親を繋ぎ変える

			int i, l = m_SafeAreaCorrector.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_SafeAreaCorrector.GetChild( i ).SetParent( transform, false ) ;
			}

			//----------------------------------------------------------
			// SafeAreaCorrector(RectTransform)を破棄する 

			if( Application.isPlaying == true )
			{
				Destroy( m_SafeAreaCorrector.gameObject ) ;
			}
			else
			{
				DestroyImmediate( m_SafeAreaCorrector.gameObject ) ;
			}

			m_SafeAreaCorrector = null ;
		}

		//-------------------------------------------------------------------------------------------

		protected override void Start()
		{
			base.Start() ;

			if( Application.isPlaying == true )	// 予め AddComponent されていた場合にヒエラルキー構造を変えてしまわないために実行時のみ処理を行うようにする
			{
				// 実行時のみ処理する

				if( m_CanvasScaler == null )
				{
					// 最低限 CanvasScaler が必要
					return ;
				}

				//----------------------------------------------------------

				// 最初に１度強制更新を行う
				Refresh() ;
			}
		}

		internal void Update()
		{
			// OnRectTransformDimensionsChange の挙動が不安定過ぎて信用できないため毎フレーム変化をチェックする

			if( Application.isPlaying == true )	// 予め AddComponent されていた場合にヒエラルキー構造を変えてしまわないために実行時のみ処理を行うようにする
			{
				// 実行時のみ処理する

				if( m_CanvasScaler == null )
				{
					// 最低限 CanvasScaler が必要
					return ;
				}

				//----------------------------------------------------------

				// 変更を確認する
				CheckUpdate() ;

				// 必要に応じて SafeAreaCorrector の RectTransform を更新する
				if( m_IsDirty == true )
				{
					Refresh() ;
				}
			}
		}

		// 変更を確認する
		private void CheckUpdate()
		{
			// セーフエリアの情報を取得する
			var safeArea = GetSafeArea() ;

			// 画面解像度とセーフエリアの変化確認
			if
			(
				// 画面の回転などによる解像度の動的変化
				m_ScreenWidth	!= Screen.width		||
				m_ScreenHeight	!= Screen.height	||
				// SafeArea の動的変化(iPhone のダイナミックアイランド等)
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

		// SafeAreaCorrector のサイズを更新する
		private void Refresh()
		{
			if( m_CanvasScaler == null )
			{
				// 最低限 CanvasScaler が必要
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

			float safeAreaCorrectorWidth ;
			float safeAreaCorrectorHeight ;

			float safeAreaCorrectorCenterX ;
			float safeAreaCorrectorCenterY ;

			//------------------------------------------------------------------------------------------

			// パディング(Canvas座標系単位)を一度初期化する
			m_PaddingL = 0 ;
			m_PaddingR = 0 ;
			m_PaddingB = 0 ;
			m_PaddingT = 0 ;

			// CanvasScaler に設定する基準解像度
			requiredBasicWidth  = m_BasicWidth ;
			requiredBasicHeight = m_BasicHeight ;

			//----------------------------------

			if( m_SafeAreaEnabledInViewportSizeFitter == false )
			{
				// 通常のキャンバスサイズを取得する
				( canvasWidth, canvasHeight ) = GetCanvasSize( requiredBasicWidth, requiredBasicHeight ) ;

				// Screen 座標系によるセーフエリアの各サイズ
				safeAreaL =                  m_SafeArea_xMin ;
				safeAreaR = m_ScreenWidth  - m_SafeArea_xMax ;
				safeAreaB =                  m_SafeArea_yMin ;
				safeAreaT = m_ScreenHeight - m_SafeArea_yMax ;

				// 通常のキャンバスサイズでアウターフレームでセーフエリアがまかなえるか確認する

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

				if( ( m_PaddingL >  0 || m_PaddingR >  0 || m_PaddingB >  0 || m_PaddingT >  0 ) )
				{
					// セーフエリアが存在するだけで SafeAreaCorrector は必須になる

					// SafeAreaCorrector を生成する
					CreateSafeAreaCorrector() ;

					//---------------------------------

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

						requiredBasicWidth  /= ( 1.0f - safeAreaRatioX ) ;
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

					// キャンバスサイズが変化したので Canvas 座標系でのセーフエリアの量を再度算出する
					m_PaddingL = canvasWidth  * safeAreaL / m_ScreenWidth  ;
					m_PaddingR = canvasWidth  * safeAreaR / m_ScreenWidth  ;
					m_PaddingB = canvasHeight * safeAreaB / m_ScreenHeight ;
					m_PaddingT = canvasHeight * safeAreaT / m_ScreenHeight ;

					if( m_SafeAreaVisible == false )
					{
						// セーフエリアは除外(通常)

						// SafeAreaCorrector のサイズ
						safeAreaCorrectorWidth  = canvasWidth  - ( m_PaddingL + m_PaddingR ) ;
						safeAreaCorrectorHeight = canvasHeight - ( m_PaddingB + m_PaddingT ) ;

						// SafeAreaCorrector のセンター
						safeAreaCorrectorCenterX = ( m_PaddingL - m_PaddingR ) * 0.5f ;
						safeAreaCorrectorCenterY = ( m_PaddingB - m_PaddingT ) * 0.5f ;
					}
					else
					{
						// セーフエリアは表示(特殊)

						// SafeAreaCorrector のサイズ
						safeAreaCorrectorWidth  = canvasWidth  ;
						safeAreaCorrectorHeight = canvasHeight  ;

						// SafeAreaCorrector のセンター
						safeAreaCorrectorCenterX = 0 ;
						safeAreaCorrectorCenterY = 0 ;
					}

					//--------------------------------
					// 注意
					// セーフエリアを除外しない(セーフエリアにも表示する)というケースは、
					// 原則 OuterFrame(ScreenSizeFitterForOurerFrame) のみである
					//
					// セーフエリアに透過部分が多数あり、
					// ２Ｄ(ＵＩ)しないが、３Ｄは表示したい、というケースに対応するためである。
					// ただしこの場合、
					// ３Ｄのカメラについた ViewportSizeFitter では、
					// SafeAreaEnabled を false にして、
					// カメラ側でのセーフエリア対応は無効にする必要がある。
					//--------------------------------


					//---------------------------------------------------------
					// SafeAreaCorrector の設定

					// ※分かりにくくなるのでフルストレッチにはしない

					// アンカーは中心
					m_SafeAreaCorrector.anchorMin			= new Vector2( 0.5f, 0.5f ) ;
					m_SafeAreaCorrector.anchorMax			= new Vector2( 0.5f, 0.5f ) ;

					// ピボットは中心
					m_SafeAreaCorrector.pivot				= new Vector2( 0.5f, 0.5f ) ;

					// サイズとセンター
					m_SafeAreaCorrector.sizeDelta			= new Vector2( safeAreaCorrectorWidth,   safeAreaCorrectorHeight  ) ;
					m_SafeAreaCorrector.anchoredPosition	= new Vector2( safeAreaCorrectorCenterX, safeAreaCorrectorCenterY ) ;

					// 強制更新
					m_SafeAreaCorrector.ForceUpdateRectTransforms() ;
				}
				else
				{
					// SafeAreaCorrector を破棄する
					DeleteSafeAreaCorrector() ;
				}
			}
			else
			{
				// SafeAreaCorrector を破棄する
				DeleteSafeAreaCorrector() ;
			}

			//--------------------------------------------------------
			// 注意
			// ViewportSizeFitter によって SafeArea の対応を行ってしまった場合、
			// Canvas 側では、SafeArea ついて一切考慮する必要が無くなる。
			//
			// ※予め、ビューポートの範囲が、SafeArea を除いた箇所に調整される。
			//
			// よって、Canvas 側では、
			// 単に基準解像度(一切補正なし)を CanvasScaler に設定するだけで良くなる。
			//--------------------------------------------------------

			//----------------------------------------------------------
			// Canvas の設定

			// Canvas の基準解像度を設定する
			m_CanvasScaler.referenceResolution = new Vector2
			(
				requiredBasicWidth,
				requiredBasicHeight
			) ;

			m_CanvasScaler.uiScaleMode		= CanvasScaler.ScaleMode.ScaleWithScreenSize ;
			m_CanvasScaler.screenMatchMode	= CanvasScaler.ScreenMatchMode.Expand ;

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

		// セーフエリアの情報を取得する
		private Rect GetSafeArea()
		{
			if( m_OnSafeAreaGet != null )
			{
				return m_OnSafeAreaGet() ;
			}
			else
			{
				return Screen.safeArea ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンポーネントが Enabled になる際に呼び出される
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable() ;

			m_IsDirty = true ;
		}

		/// <summary>
		/// コンポーネントが Disabled になる際に呼び出される
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable() ;

			m_IsDirty = true ;
		}

		//-----------------------------------

		/// <summary>
		/// RectTransform に変化があった際に呼び出される(Canvas 内全ての RectTransform が反応)
		/// </summary>
		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange() ;

			if( enabled == false )
			{
				return ;
			}

			// 変更を確認する
			CheckUpdate() ;

#if UNITY_EDITOR
			if( m_IsDirty == true )
			{
				if( Application.isPlaying == false )
				{
					// 即時実行(この後 Update が呼ばれる保証が無い) ※ただし実行はエディターモード限定
					Refresh() ;
				}
			}
#endif
		}
	}
}

