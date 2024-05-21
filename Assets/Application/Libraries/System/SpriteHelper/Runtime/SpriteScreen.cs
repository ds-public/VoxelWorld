using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2024/05/20
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public partial class SpriteScreen : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// SpriteScreen を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteScreen", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteScreen" )]					// ポップアップメニューから
		public static void CreateSpriteScreen()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child SpriteScreen" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteScreen" ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteScreen>() ;
			component.SetDefault() ;	// 初期状態に設定する

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ヒエラルキーでの階層パス名を取得する
		/// </summary>
		public string Path
		{
			get
			{
				string path = name ;

				var t = transform.parent ;
				while( t != null )
				{
					path = $"{t.name}/{path}" ;
					t = t.parent ;
				}
				return path ;
			}
		}

		/// <summary>
		/// Component を追加する(ショートカット)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T : UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public void SetDefault()
		{
			// カメラ追加
			var go = new GameObject( "SpriteCamera" ) ;
			go.transform.SetParent( transform, false ) ;
			go.transform.SetLocalPositionAndRotation( new Vector3(  0,  0, -1000 ), Quaternion.identity ) ;

			var camera = go.AddComponent<Camera>() ;
			camera.clearFlags = CameraClearFlags.SolidColor ;
			camera.backgroundColor = new Color32(  64,  64,  64, 255 ) ;
			camera.orthographic  = true ;
			camera.nearClipPlane =    0.1f ;
			camera.farClipPlane  = 2000.0f ;

			//----------------------------------

			m_Camera = camera ;

			m_SafeAreaEnabled = true ;

			m_BasicWidth  = 1080 ;
			m_BasicHeight = 1920 ;

			m_LimitWidth  = 1440 ;
			m_LimitHeight = 2560 ;

			m_Pivot = new ( 0, 0 ) ;

			m_HorizontalAnchorType	= HorizontalAnchorTypes.Stretch ;
			m_VerticalAnchorType	= VerticalAnchorTypes.Stretch ;

			m_ProjectionSizeAdjustment = true ;

			m_IsDirty = true ;
		}

		//-------------------------------------------------------------------------------------------

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

		/// <summary>
		/// スプライトスクリーン用のカメラ
		/// </summary>
		public Camera SpriteCamera
		{
			get
			{
				return m_Camera ;
			}
			set
			{
				m_Camera = value ;
			}
		}


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
		public bool SafeAreaEnabled
		{
			get
			{
				return m_SafeAreaEnabled ;
			}
			set
			{
				if( m_SafeAreaEnabled != value )
				{
					m_SafeAreaEnabled  = value ;

					m_IsDirty = true ;
				}
			}
		}

		//-----------------------------------------------------------

		[Header( "基準解像度" )]

		// 基準解像度
		[SerializeField]
		protected float					m_BasicWidth ;

		public float BasicWidth
		{
			get
			{
				return m_BasicWidth ;
			}
			set
			{
				if( m_BasicWidth != value )
				{
					m_BasicWidth = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_BasicHeight ;

		public float BasicHeight
		{
			get
			{
				return m_BasicHeight ;
			}
			set
			{
				if( m_BasicHeight != value )
				{
					m_BasicHeight = value ;

					m_IsDirty = true ;
				}
			}
		}


		[Header( "最大解像度" )]

		// 最大解像度
		[SerializeField]
		protected float					m_LimitWidth ;

		public float LimitWidth
		{
			get
			{
				return m_LimitWidth ;
			}
			set
			{
				if( m_LimitWidth != value )
				{
					m_LimitWidth = value ;

					m_IsDirty = true ;
				}
			}
		}


		[SerializeField]
		protected float					m_LimitHeight ;

		public float LimitHeight
		{
			get
			{
				return m_LimitHeight ;
			}
			set
			{
				if( m_LimitHeight != value )
				{
					m_LimitHeight = value ;

					m_IsDirty = true ;
				}
			}
		}

		//-----------------------------------------------------------

		[Header( "ピボット" )]

		// 最大解像度
		[SerializeField]
		protected Vector2					m_Pivot ;

		public Vector2 Pivot
		{
			get
			{
				return m_Pivot ;
			}
			set
			{
				if( m_Pivot != value )
				{
					m_Pivot = value ;

					m_IsDirty = true ;
				}
			}
		}

		//-----------------------------------------------------------

		[Header( "ビューポートの表示位置指定" )]

		[SerializeField]
		protected HorizontalAnchorTypes	m_HorizontalAnchorType = HorizontalAnchorTypes.Stretch ;

		public HorizontalAnchorTypes HorizontalAnchorType
		{
			get
			{
				return m_HorizontalAnchorType ;
			}
			set
			{
				if( m_HorizontalAnchorType != value )
				{
					m_HorizontalAnchorType  = value ;

					m_IsDirty = true ;
				}
			}
		}


		[SerializeField]
		protected float					m_ViewportMarginL ;

		public float ViewportMarginL
		{
			get
			{
				return m_ViewportMarginL ;
			}
			set
			{
				if( m_ViewportMarginL != value )
				{
					m_ViewportMarginL  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportMarginR ;

		public float ViewportMarginR
		{
			get
			{
				return m_ViewportMarginR ;
			}
			set
			{
				if( m_ViewportMarginR != value )
				{
					m_ViewportMarginR  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportOffsetX ;

		public float ViewportOffsetX
		{
			get
			{
				return m_ViewportOffsetX ;
			}
			set
			{
				if( m_ViewportOffsetX != value )
				{
					m_ViewportOffsetX  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportWidth  ;

		public float ViewportWidth
		{
			get
			{
				return m_ViewportWidth  ;
			}
			set
			{
				if( m_ViewportWidth  != value )
				{
					m_ViewportWidth   = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected VerticalAnchorTypes	m_VerticalAnchorType = VerticalAnchorTypes.Stretch ;

		public VerticalAnchorTypes VerticalAnchorType
		{
			get
			{
				return m_VerticalAnchorType ;
			}
			set
			{
				if( m_VerticalAnchorType != value )
				{
					m_VerticalAnchorType  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportMarginB ;

		public float ViewportMarginB
		{
			get
			{
				return m_ViewportMarginB ;
			}
			set
			{
				if( m_ViewportMarginB != value )
				{
					m_ViewportMarginB  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportMarginT ;

		public float ViewportMarginT
		{
			get
			{
				return m_ViewportMarginT ;
			}
			set
			{
				if( m_ViewportMarginT != value )
				{
					m_ViewportMarginT  = value ;

					m_IsDirty = true ;
				}
			}
		}


		[SerializeField]
		protected float					m_ViewportOffsetY ;

		public float ViewportOffsetY
		{
			get
			{
				return m_ViewportOffsetY ;
			}
			set
			{
				if( m_ViewportOffsetY != value )
				{
					m_ViewportOffsetY  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected float					m_ViewportHeight ;

		public float ViewportHeight
		{
			get
			{
				return m_ViewportHeight ;
			}
			set
			{
				if( m_ViewportHeight != value )
				{
					m_ViewportHeight  = value ;

					m_IsDirty = true ;
				}
			}
		}

		[SerializeField]
		protected bool					m_ProjectionSizeAdjustment = true ;

		/// <summary>
		/// プロジェクトションサイズへの反映
		/// </summary>
		public bool ProjectionSizeAdjustment
		{
			get
			{
				return m_ProjectionSizeAdjustment ;
			}
			set
			{
				if( m_ProjectionSizeAdjustment != value )
				{
					m_ProjectionSizeAdjustment  = value ;

					m_IsDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		[Header( "現在の画面解像度 ※ReadOnly" )]

		[SerializeField]
		protected float					m_ScreenWidth ;

		public float					ScreenWidth  => m_ScreenWidth ;

		[SerializeField]
		protected float					m_ScreenHeight ;

		public float					ScreenHeight => m_ScreenHeight ;


		// SafeArea の変化確認用(width height で確認してはダメ)
		[Header( "現在の SafeArea [Screen座標系] ※ReadOnly")]

		[SerializeField]
		protected float					m_SafeArea_xMin ;

		public float					SafeAreaSL => m_SafeArea_xMin ;

		[SerializeField]
		protected float					m_SafeArea_xMax ;

		public float					SafeAreaSR => ( m_ScreenWidth  - m_SafeArea_xMax ) ;

		[SerializeField]
		private float					m_SafeArea_yMin ;

		public float					SafeAreaSB => m_SafeArea_yMin ;

		[SerializeField]
		private float					m_SafeArea_yMax ;

		public float					SafeAreaST => ( m_ScreenHeight - m_SafeArea_yMax ) ;


		[Header( "SafeArea による Padding [Canvas座標系] ※ReadOnly" )]

		[SerializeField]
		protected float					m_SafeAreaL ;

		public float					SafeAreaCL => m_SafeAreaL ;

		[SerializeField]
		protected float					m_SafeAreaR ;

		public float					SafeAreaCR => m_SafeAreaR ;

		[SerializeField]
		protected float					m_SafeAreaB ;

		public float					SafeAreaCB => m_SafeAreaB ;

		[SerializeField]
		protected float					m_SafeAreaT ;

		public float					SafeAreaCT => m_SafeAreaT ;

		//-------------------------------------------------------------------------------------------

		// 更新が必要かどうか
		protected bool					m_IsDirty = false ;

		// セーフエリアの取得
		private Func<Rect>				m_OnSafeAreaGet ;

		/// <summary>
		/// セーフエリア取得のコールバックを設定する
		/// </summary>
		/// <param name="onSafeAreaGet"></param>
		public void SetOnSafeAreaGet( Func<Rect> onSafeAreaGet )
		{
			m_OnSafeAreaGet = onSafeAreaGet ;

			m_IsDirty = true ;
		}


		// 現在の画面サイズ(Canvas系)

		private Vector2 m_Size ;

		/// <summary>
		/// 現在の画面解像度(Canvasスケール)
		/// </summary>
		public  Vector2 Size => GetSize() ;

		/// <summary>
		/// 現在の画面解像度(Canvasスケール)を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetSize()
		{
			if( m_Size.x <= 0 || m_Size.y <= 0 )
			{
				return new Vector2( m_BasicWidth, m_BasicHeight ) ;
			}

			return m_Size ;
		}

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			if( m_Camera == null )
			{
				// カメラの指定が無ければ同じノードからの取得を試みる

				TryGetComponent<Camera>( out m_Camera ) ;
			}
		}

		/// <summary>
		/// 継承クラスで基準解像度と最大解像度を設定する
		/// </summary>
		/// <param name="basicWidth"></param>
		/// <param name="basicHeight"></param>
		/// <param name="limitWIdth"></param>
		/// <param name="limitHeight"></param>
		public void SetResolution( float basicWidth, float basicHeight, float limitWIdth, float limitHeight )
		{
			m_BasicWidth	= basicWidth ;
			m_BasicHeight	= basicHeight ;

			m_LimitWidth	= limitWIdth ;
			m_LimitHeight	= limitHeight ;

			m_IsDirty		= true ;
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
			m_SafeAreaL = 0 ;
			m_SafeAreaR = 0 ;
			m_SafeAreaB = 0 ;
			m_SafeAreaT = 0 ;

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
				m_SafeAreaL = canvasWidth  * safeAreaL / m_ScreenWidth  ;
				m_SafeAreaR = canvasWidth  * safeAreaR / m_ScreenWidth  ;
				m_SafeAreaB = canvasHeight * safeAreaB / m_ScreenHeight ;
				m_SafeAreaT = canvasHeight * safeAreaT / m_ScreenHeight ;

				m_SafeAreaL -= outerL ;
				if( m_SafeAreaL <  0 )
				{
					m_SafeAreaL  = 0 ;
				}

				m_SafeAreaR -= outerR ;
				if( m_SafeAreaR <  0 )
				{
					m_SafeAreaR  = 0 ;
				}

				m_SafeAreaB -= outerB ;
				if( m_SafeAreaB <  0 )
				{
					m_SafeAreaB  = 0 ;
				}

				m_SafeAreaT -= outerT ;
				if( m_SafeAreaT <  0 )
				{
					m_SafeAreaT  = 0 ;
				}

				//----------------------------------

				if( m_SafeAreaL >  0 || m_SafeAreaR >  0 || m_SafeAreaB >  0 || m_SafeAreaT >  0 )
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
					m_SafeAreaL = canvasWidth  * safeAreaL / m_ScreenWidth  ;
					m_SafeAreaR = canvasWidth  * safeAreaR / m_ScreenWidth  ;
					m_SafeAreaB = canvasHeight * safeAreaB / m_ScreenHeight ;
					m_SafeAreaT = canvasHeight * safeAreaT / m_ScreenHeight ;

					// SafeAreaCorrector のサイズ
					viewportW = canvasWidth  - m_SafeAreaL - m_SafeAreaR ;
					viewportH = canvasHeight - m_SafeAreaB - m_SafeAreaT ;
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
			float viewportX = m_SafeAreaL ;
			float viewportY = m_SafeAreaB ;

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

			float viewportWidth  = m_ViewportWidth ;
			float viewportHeight = m_ViewportHeight ;

			if( viewportWidth  <= 0 )
			{
				viewportWidth   = m_BasicWidth  ;
			}

			if( viewportHeight <= 0 )
			{
				viewportHeight  = m_BasicHeight ;
			}


			// 横方向
			switch( m_HorizontalAnchorType )
			{
				// 横は引き伸ばし
				case HorizontalAnchorTypes.Stretch :
					if( ( m_ViewportMarginL + m_ViewportMarginR ) <  viewportW )
					{
						viewportX += m_ViewportMarginL ;
						viewportW -= ( m_ViewportMarginL + m_ViewportMarginR ) ;
					}
				break ;

				//-------------

				// 横は左寄せ
				case HorizontalAnchorTypes.Left :
					if( m_ViewportOffsetX >= 0 && ( m_ViewportOffsetX + viewportWidth  ) <= viewportW )
					{
						viewportX += m_ViewportOffsetX ;
						viewportW  = viewportWidth  ;
					}
				break ;

				// 横は中央
				case HorizontalAnchorTypes.Center :
					if( viewportWidth  <= viewportW  )
					{
						float offsetLimitX = ( viewportW  - viewportWidth  ) * 0.5f ;
						if( Mathf.Abs( m_ViewportOffsetX ) <= offsetLimitX )
						{
							viewportX += ( offsetLimitX + m_ViewportOffsetX ) ;
							viewportW  = viewportWidth  ;
						}
					}
				break ;

				// 横は右寄せ
				case HorizontalAnchorTypes.Right :
					if( m_ViewportOffsetX <= 0 && ( viewportWidth  - m_ViewportOffsetX ) <= viewportW )
					{
						viewportX += ( viewportW - viewportWidth  + m_ViewportOffsetX ) ;
						viewportW  = viewportWidth  ;
					}
				break ;
			}

			// 縦方向
			switch( m_VerticalAnchorType )
			{
				// 縦は引き伸ばし
				case VerticalAnchorTypes.Stretch :
					if( ( m_ViewportMarginB + m_ViewportMarginT ) <  viewportH )
					{
						viewportY += m_ViewportMarginB ;
						viewportH -= ( m_ViewportMarginB + m_ViewportMarginT ) ;
					}
				break ;

				//-------------

				// 縦は下寄せ
				case VerticalAnchorTypes.Bottom :
					if( m_ViewportOffsetY >= 0 && ( m_ViewportOffsetY + viewportHeight ) <= viewportH )
					{
						viewportY += m_ViewportOffsetY ;
						viewportH  = viewportHeight ;
					}
				break ;

				// 縦は中央
				case VerticalAnchorTypes.Middle :
					if( viewportHeight <= viewportH )
					{
						float offsetLimitY = ( viewportH - viewportHeight ) * 0.5f ;
						if( Mathf.Abs( m_ViewportOffsetY ) <= offsetLimitY )
						{
							viewportY += ( offsetLimitY + m_ViewportOffsetY ) ;
							viewportH  = viewportHeight ;
						}
					}
				break ;

				// 縦は上寄せ
				case VerticalAnchorTypes.Top :
					if( m_ViewportOffsetY <= 0 && ( viewportHeight - m_ViewportOffsetY ) <= viewportH )
					{
						viewportY += ( viewportH - viewportHeight + m_ViewportOffsetY ) ;
						viewportH  = viewportHeight ;
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

			// 強制的に平行投影モードにする
			m_Camera.orthographic = true ;

			if( m_ProjectionSizeAdjustment == true )
			{
				// プロジェクションサイズの反映有効
				m_Camera.orthographicSize = viewportH * 0.5f ;
			}

			//----------------------------------------------------------
			// ピボットの反映

			m_Camera.transform.localPosition = new Vector3( - viewportW * m_Pivot.x, - viewportH * m_Pivot.y, m_Camera.transform.localPosition.z ) ;

			//----------------------------------------------------------
			// 解像度を更新

			m_Size = new Vector2 ( viewportW, viewportH ) ;

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
			if( m_OnSafeAreaGet != null )
			{
				return m_OnSafeAreaGet() ;
			}

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
