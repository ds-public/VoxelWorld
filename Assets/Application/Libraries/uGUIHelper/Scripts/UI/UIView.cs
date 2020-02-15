using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	[ RequireComponent( typeof( RectTransform ) ) ]
	
	[ ExecuteInEditMode ]

	/// <summary>
	/// uGUIの使い勝手を向上させるヘルパークラス(基底クラス)
	/// </summary>
	public class UIView : UIBehaviour
	{
		public const string version = "Version 2020/02/13 0" ;
		// ソースコード
		// https://bitbucket.org/Unity-Technologies/ui/src/2019.1/


#if UNITY_EDITOR
		[MenuItem( "Tools/UIView/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIView[] views = UIEditorUtility.FindComponents<UIView>
			(
				"Assets/Application",
				( _ ) =>
				{
//					_.m_MaterialType = ( MaterialTypes )_.m_MaterialType_ ;
					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIViewの数:" + c ) ;
		}
#endif



	//	[Tooltip("識別子")]

		/// <summary>
		/// View の識別子
		/// </summary>
		public string Identity = null ;
		
		/// <summary>
		/// View の識別子または名前を返す
		/// </summary>
		public string IdentityOrName
		{
			get
			{
				if( string.IsNullOrEmpty( Identity )  == false )
				{
					return Identity ;
				}

				return name ;
			}
		}

		/// <summary>
		/// キャンバスグループのアルファ値が指定値未満の場合はレイキャストを無効化する
		/// </summary>
		public float disableRaycastUnderAlpha = 0.0f ;

		// 画面表示状態
		private bool m_Visible = true ;	// 自身を含めた子を画面に表示するか

		/// <summary>
		/// 画面表示状態
		/// </summary>
		public bool Visible
		{
			get
			{
				return m_Visible ;
			}
		}

		/// <summary>
		/// 表示状態を設定する
		/// </summary>
		/// <param name="tState"></param>
		public void SetVisible( bool state )
		{
			if( state == false )
			{
				Hide() ;
			}
			else
			{
				Show() ;
			}
		}

		/// <summary>
		/// 自身を含めて子を非表示にする
		/// </summary>
		public void Hide()
		{
			m_Visible = false ;

			if( GetCanvasGroup() == null )
			{
				AddCanvasGroup() ;
			}

			Alpha = m_LocalAlpha ;
		}

		public void Show()
		{
			m_Visible = true ;

			if( GetCanvasGroup() != null )
			{
				Alpha = m_LocalAlpha ;
			}
		}

		override protected void OnDisable()
		{
			base.OnDisable() ;

			m_Hover = false ;	// 非アクティブになったら一度ホバーフラグはクリアする
			m_Press = false ;	// 非アクティブになったら一度プレスフラグはクリアする
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動くものの表示確認用クラス
		/// </summary>
		public class MovableState : CustomYieldInstruction
		{
			public MovableState()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == true || string.IsNullOrEmpty( Error ) == false )
					{
						return false ;   // 終了
					}
					return true ;    // 継続
				}
			}

			/// <summary>
			/// 終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラーが発生したかどうか
			/// </summary>
			public string	Error = string.Empty ;
		}

		//-------------------------------------------------------------------------------------------
	
		override protected void Awake()
		{
			base.Awake() ;

			if( GetRectTransform() == null )
			{
				// 無ければくっつける(Imageなどは自動でAddしているので重複Addしないように注意する必要がある）
				gameObject.AddComponent<RectTransform>() ;
				ResetRectTransform() ;
			}

			this.Alpha = this.Alpha ;	// 半透明具合による CanvasRaycast の有効無効設定
		
			//-------------------------------------------------

			// 基本的なインタラクションイベントを登録する(非アクティブの場合はStartはアクティブになるまで呼ばれないためAwakeでないとマズい)
			if( Application.isPlaying == true )
			{
				AddInteractionCallback() ;
				AddInteractionForScrollViewCallback() ;
			}

			//-------------------------------------------------

			// 継承クラスの Awake() を実行する
			OnAwake() ;
			
			// Tween などで使用する基準情報を保存する(コンポーネントの実行順が不確定なので Awake で必ず実行すること)
			SetLocalState() ;
		}
		
		// RectTransfrom を初期化する
		protected void ResetRectTransform()
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				rectTransform.anchoredPosition3D = Vector3.zero ;
				rectTransform.localScale = Vector3.one ;
				rectTransform.localRotation = Quaternion.identity ;
			}
		}

		virtual protected void OnAwake(){}

		// Tween などで使用する基準情報を保存する
		protected void SetLocalState()
		{
			LocalPosition = GetRectTransform().anchoredPosition3D ;
			LocalRotation = GetRectTransform().localEulerAngles ;
			LocalScale    = GetRectTransform().localScale ;

			if( GetCanvasGroup() != null )
			{
				LocalAlpha = GetCanvasGroup().alpha ;
			}
		}
		
		/// <summary>
		/// デフォルト設定を実行する（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		public void SetDefault( string option = "" )
		{
			// レイヤーを設定
			gameObject.layer = GetCanvasTargetLayerOfFirst() ;	// ＵＩ

			// 子オブジェクトを全て破棄する
			int i, l = transform.childCount ;
			if( l >  0 )
			{
				GameObject[] childObjects = new GameObject[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					childObjects[ i ] = transform.GetChild( i ).gameObject ;
				}
				for( i  = 0 ; i <  l ; i ++ )
				{
					Destroy( childObjects[ i ] ) ;
				}
			}

			// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
			OnBuild( option ) ;

			//-------------------------------------------------------

			// Tween などで使用する基準情報を保存する(変化している可能性があるので更新)
			SetLocalState() ;
		}
		
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		virtual protected void OnBuild( string option = "" ){}

		override protected void Start()
		{
			base.Start() ;

			OnStart() ;
		}

		virtual protected void OnStart(){}

		void Update()
		{
			#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tweenChecker = false ;
				UITween[] tweens = GetComponents<UITween>() ;
				if( tweens != null && tweens.Length >  0 )
				{
					for( int i  = 0 ; i <  tweens.Length ; i++ )
					{
						if( tweens[ i ].IsChecker == true )
						{
							tweenChecker = true ;
							break ;
						}
					}
				}

				if( tweenChecker == false )
				{
					// ３つの値が異なっていれば更新する
					if( GetRectTransform() != null )
					{
						if( m_LocalPosition != GetRectTransform().anchoredPosition3D )
						{
							m_LocalPosition  = GetRectTransform().anchoredPosition3D ;
						}
						if( m_LocalRotation != GetRectTransform().localEulerAngles )
						{
							m_LocalRotation  = GetRectTransform().localEulerAngles ;
						}
						if( m_LocalScale != GetRectTransform().localScale )
						{
							m_LocalScale  = GetRectTransform().localScale ;
						}
					}
					if( GetCanvasGroup() != null )
					{
						if( m_LocalAlpha != GetCanvasGroup().alpha )
						{
							m_LocalAlpha  = GetCanvasGroup().alpha ;
						}
					}
				}
			}

			RemoveComponents() ;

			#endif

			SingleClickCheckProcess() ;

			OnUpdate() ;

			if( m_Hover == true )
			{
				if( m_Press == false && m_DragState == PointerState.None && UIEventSystem.ProcessType == StandaloneInputModuleWrapper.ProcessType.Default )
				{
					OnHoverInner( PointerState.Moving, GetLocalPosition( Input.mousePosition ) ) ;
				}
			}

			OnHoldInner() ;

			if( m_Click == true && m_ClickCountTime != Time.frameCount )
			{
				m_Click  = false ;
			}
		}

		#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( m_RemoveCanvasGroup == true )
			{
				RemoveCanvasGroup() ;
				m_RemoveCanvasGroup = false ;
			}
		
			if( m_RemoveEventTrigger == true )
			{
				RemoveEventTrigger() ;
				m_RemoveEventTrigger = false ;
			}
		
			if( m_RemoveInteraction == true )
			{
				RemoveInteraction() ;
				m_RemoveInteraction = false ;
			}
		
			if( m_RemoveInteractionForScrollView == true )
			{
				RemoveInteractionForScrollView() ;
				m_RemoveInteractionForScrollView = false ;
			}
		
			if( m_RemoveTransition == true )
			{
				RemoveTransition() ;
				m_RemoveTransition = false ;
			}
		
			if( m_RemoveTransitionForScrollView == true )
			{
				RemoveTransitionForScrollView() ;
				m_RemoveTransitionForScrollView = false ;
			}
		
			if( m_RemoveMask == true )
			{
				RemoveMask() ;
				m_RemoveMask = false ;
			}
		
			if( m_RemoveRectMask2D == true )
			{
				RemoveRectMask2D() ;
				m_RemoveRectMask2D = false ;
			}
		
			if( m_RemoveAlphaMaskWindow == true )
			{
				RemoveAlphaMaskWindow() ;
				m_RemoveAlphaMaskWindow = false ;
			}
		
			if( m_RemoveAlphaMaskTarget == true )
			{
				RemoveAlphaMaskTarget() ;
				m_RemoveAlphaMaskTarget = false ;
			}

			if( m_RemoveContentSizeFitter == true )
			{
				RemoveContentSizeFitter() ;
				m_RemoveContentSizeFitter = false ;
			}

			if( m_RemoveShadow == true )
			{
				RemoveShadow() ;
				m_RemoveShadow = false ;
			}

			if( m_RemoveOutline == true )
			{
				RemoveOutline() ;
				m_RemoveOutline = false ;
			}

			if( m_RemoveGradient == true )
			{
				RemoveGradient() ;
				m_RemoveGradient = false ;
			}

			if( m_RemoveInversion == true )
			{
				RemoveInversion() ;
				m_RemoveInversion = false ;
			}

			if( m_RemoveToggleGroup == true )
			{
				RemoveToggleGroup() ;
				m_RemoveToggleGroup = false ;
			}

			if( m_RemoveToggle == true )
			{
				RemoveToggle() ;
				m_RemoveToggle = false ;
			}


			if( string.IsNullOrEmpty( m_RemoveTweenIdentity ) == false && m_RemoveTweenInstance != 0 )
			{
				RemoveTween( m_RemoveTweenIdentity, m_RemoveTweenInstance ) ;
				m_RemoveTweenIdentity = null ;
				m_RemoveTweenInstance = 0 ;
			}

			if( string.IsNullOrEmpty( m_RemoveFlipperIdentity ) == false && m_RemoveFlipperInstance != 0 )
			{
				RemoveFlipper( m_RemoveFlipperIdentity, m_RemoveFlipperInstance ) ;
				m_RemoveFlipperIdentity = null ;
				m_RemoveFlipperInstance = 0 ;
			}
		}
		#endif


		virtual protected void OnUpdate(){}

		void LateUpdate()
		{
			OnLateUpdate() ;

			if( m_MaterialType == MaterialTypes.Interpolation )
			{
				ProcessInterpolation() ;
			}
			else
			if( m_MaterialType == MaterialTypes.Mosaic )
			{
				ProcessMosaic() ;
			}
		}

		virtual protected void OnLateUpdate(){}

		//------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalPosition = Vector3.zero ;
		public  Vector3   LocalPosition
		{
			get
			{
				return m_LocalPosition ;
			}
			set
			{
				m_LocalPosition = value ;
			}
		}

		public Vector3 Position
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalPosition ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.anchoredPosition3D ;
				}
			}
			set
			{
				m_LocalPosition = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchoredPosition3D = value ;
			}
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( Vector2 position )
		{
			Position = new Vector3( position.x, position.y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPosition( float x, float y )
		{
			Position = new Vector3( x, y, Position.z ) ;
		}
		
		/// <summary>
		/// Ｘ座標(ショートカット)
		/// </summary>
		public float Px
		{
			get
			{
				return Position.x ;
			}
			set
			{
				Position = new Vector3( value, Position.y, Position.z ) ;
			}
		}

		public float Rx
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.x ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( value, GetRectTransform().anchoredPosition3D.y, GetRectTransform().anchoredPosition3D.z ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// Ｙ座標(ショートカット)
		/// </summary>
		public float Py
		{
			get
			{
				return Position.y ;
			}
			set
			{
				Position = new Vector3( Position.x, value, Position.z ) ;
			}
		}

		public float Ry
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.y ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( GetRectTransform().anchoredPosition3D.x, value, GetRectTransform().anchoredPosition3D.z ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}
		
	
		/// <summary>
		/// Ｚ座標(ショートカット)
		/// </summary>
		public float Pz
		{
			get
			{
				return Position.z ;
			}
			set
			{
				Position = new Vector3( Position.x, Position.y, value ) ;
			}
		}
		
		public float Rz
		{
			get
			{
				return GetRectTransform().anchoredPosition3D.z ;
			}
			set
			{
				GetRectTransform().anchoredPosition3D = new Vector3( GetRectTransform().anchoredPosition3D.x, GetRectTransform().anchoredPosition3D.y, value ) ;
				m_LocalPosition = GetRectTransform().anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// サイズ(ショートカット)
		/// </summary>
		virtual public Vector2 Size
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				Vector2 size = rectTransform.sizeDelta ;

				// Ｘのチェック
				if( GetRectTransform().anchorMin.x != GetRectTransform().anchorMax.x )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> hierarchyRects = new List<RectTransform>()
					{
						GetRectTransform()
					} ;

					Transform parentRect = GetRectTransform().parent ;
					Canvas canvas ;
					Vector2 canvasSize ;
					RectTransform targetRect ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parentRect != null )
						{
							canvas = parentRect.GetComponent<Canvas>() ;
							if( canvas != null )
							{
								// キャンバスに到達した
								canvasSize = GetCanvasSize() ;	// 正確な値を取得する
								delta = canvasSize.x ;
								break ;
							}
							else
							{
								targetRect = parentRect.GetComponent<RectTransform>() ;
								if( targetRect != null )
								{
									if( targetRect.anchorMin.x == targetRect.anchorMax.x  )
									{
										// 発見
										delta = targetRect.sizeDelta.x ;
										break ;
									}
									else
									{
										hierarchyRects.Add( targetRect ) ;
									}
								}
								parentRect = parentRect.parent ;
							}
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = hierarchyRects.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( hierarchyRects[ i ].anchorMax.x - hierarchyRects[ i ].anchorMin.x ) ;
							delta += hierarchyRects[ i ].sizeDelta.x ;
						}

						size.x = delta ;
					}
				}

				// Ｙのチェック
				if( GetRectTransform().anchorMin.y != GetRectTransform().anchorMax.y )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> hierarchyRects = new List<RectTransform>()
					{
						GetRectTransform()
					} ;

					Transform parentRect = GetRectTransform().parent ;
					Canvas canvas ;
					Vector2 canvasSize ;
					RectTransform targetRect ;
					float delta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( parentRect != null )
						{
							canvas = parentRect.GetComponent<Canvas>() ;
							if( canvas != null )
							{
								// キャンバスに到達した
								canvasSize = GetCanvasSize() ;	// 正確な値を取得する
								delta = canvasSize.y ;
								break ;
							}
							else
							{
								targetRect = parentRect.GetComponent<RectTransform>() ;
								if( targetRect != null )
								{
									if( targetRect.anchorMin.y == targetRect.anchorMax.y )
									{
										// 発見
										delta = targetRect.sizeDelta.y ;
										break ;
									}
									else
									{
										hierarchyRects.Add( targetRect ) ;
									}
								}
								parentRect = parentRect.parent ;
							}
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( delta >   0 )
					{
						// マージン分を引く
						for( int i  = hierarchyRects.Count - 1 ; i >= 0 ; i -- )
						{
							delta *= ( hierarchyRects[ i ].anchorMax.y - hierarchyRects[ i ].anchorMin.y ) ;
							delta += hierarchyRects[ i ].sizeDelta.y ;
						}

						size.y = delta ;
					}
				}

				return size ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}

				Vector2 size = rectTransform.sizeDelta ;
				if( rectTransform.anchorMin.x == rectTransform.anchorMax.x )
				{
					size.x = value.x ;
				}
				if( rectTransform.anchorMin.y == rectTransform.anchorMax.y )
				{
					size.y = value.y ;
				}
				rectTransform.sizeDelta = size ;
			}
		}

		/// <summary>
		/// 現在のピボットから矩形の中心への相対的な距離
		/// </summary>
		public Vector3 Center
		{
			get
			{
				Vector2 size = Size ;
				Vector2 pivot = Pivot ;

				return new Vector3( size.x * ( 0.5f - pivot.x ), size.y * ( 0.5f - pivot.y ), 0 ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する
		/// </summary>
		public Vector2 PositionInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return Vector2.zero ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;

				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1  = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1  = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}

				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				return new Vector2( px, py ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する
		/// </summary>
		public Rect RectInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;

				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

//					Debug.Log( rt.name ) ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}
				
				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				pw *= GetRectTransform().localScale.x ;
				ph *= GetRectTransform().localScale.y ;

				px -= ( pw * Pivot.x ) ;
				py -= ( ph * Pivot.y ) ;

				return new Rect( px, py, pw, ph ) ;
//				return new Rect( new Vector2( px, py ), new Vector2( pw, ph ) ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する
		/// </summary>
		public Rect ViewInCanvas
		{
			get
			{
				// 親サイズ
				Vector2 ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				List<RectTransform> hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				Transform t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				for( i  =  0 ; i <  64 ; i ++ )
				{
					if( t != null )
					{
						if( t.GetComponent<Canvas>() == null )
						{
							rt = t.GetComponent<RectTransform>() ;
							if( rt != null )
							{
								hierarchyRects.Add( rt ) ;
							}
						}
						else
						{
							break ;	// 終了
						}
					}
					else
					{
						break ;	// 終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;
//				float px1 = pw ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;
//				float py1 = ph ;


				l = hierarchyRects.Count ;
//				Debug.LogWarning( "階層の数:" + l ) ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

//					Debug.Log( rt.name ) ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;	// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

//						Debug.Log( "親のX:" + px0 + " ～ " + px1 + " / " + pw ) ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
						// 中心位置
//						Debug.Log( "親のX:" + px0 + " ～ " + px1 ) ;
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;
//					px1 = px0 + pw ;

//					Debug.Log( "x:" + px ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;	// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + ph ) ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) /* - ( ( rt.sizeDelta.x * rt.pivot.x ) - rt.anchoredPosition.x ) */ ;
					}
					else
					{
//						Debug.Log( "親のY:" + py0 + " ～ " + py1 + " / " + py ) ;

						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
//					py1 = py0 + ph ;

//					Debug.Log( "y:" + py ) ;
				}
				
				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				pw *= GetRectTransform().localScale.x ;
				ph *= GetRectTransform().localScale.y ;

				px -= ( pw * Pivot.x ) ;
				py -= ( ph * Pivot.y ) ;
				
				//---------------------------------------------------------

				float vx = ( px / ps.x ) * 2.0f ;
				float vy = ( py / ps.y ) * 2.0f ;
				float vw = ( pw / ps.x ) * 2.0f ;
				float vh = ( ph / ps.y ) * 2.0f ;

				return new Rect( vx, vy, vw, vh ) ;
//				return new Rect( new Vector2( vx, vy ), new Vector2( vw, vh ) ) ;
			}
		}


		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="tSize"></param>
		public void SetSize( Vector2 size )
		{
			Size = size ;
		}

		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetSize( float w, float h )
		{
			Size = new Vector2( w, h ) ;
		}

		/// <summary>
		/// 横幅(ショートカット)
		/// </summary>
		public float Width
		{
			get
			{
				return Size.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform != null )
				{
					SetSize( value, rectTransform.sizeDelta.y ) ;
				}
			}
		}
		
		/// <summary>
		/// 縦幅(ショートカット)
		/// </summary>
		public float Height
		{
			get
			{
				return Size.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform != null )
				{
					SetSize( rectTransform.sizeDelta.x, value ) ;
				}
			}
		}
		
		/// <summary>
		/// テキスト自体の横幅
		/// </summary>
		public float TextWidth
		{
			get
			{
				if( this is UIText )
				{
					UIText text = this as UIText ;
					return text.TextSize.x ;
				}
				else
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.TextSize.x ;
				}
				else
				if( this is UITextMesh )
				{
					UITextMesh text = this as UITextMesh ;
					return text.TextSize.x ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// テキスト自体の縦幅
		/// </summary>
		public float TextHeight
		{
			get
			{
				if( this is UIText )
				{
					UIText text = this as UIText ;
					return text.TextSize.y ;
				}
				else
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.TextSize.y ;
				}
				else
				if( this is UITextMesh )
				{
					UITextMesh text = this as UITextMesh ;
					return text.TextSize.y ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// テキスト自体の最大の縦幅
		/// </summary>
		public float FullTextWidth
		{
			get
			{
				if( this is UIRichText )
				{
					UIRichText text = this as UIRichText ;
					return text.FullHeight ;
				}
				return 0 ;	
			}
		}

		/// <summary>
		/// アンカー最少値(ショートカット)
		/// </summary>
		public Vector2 AnchorMin
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				return rectTransform.anchorMin ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = value ;
			}
		}
		
		/// <summary>
		/// アンカー最少値を設定
		/// </summary>
		/// <param name="tAnchorMin"></param>
		public void SetAnchorMin( Vector2 anchorMin )
		{
			AnchorMin = anchorMin ;
		}
		
		/// <summary>
		/// アンカー最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMin( float x, float y )
		{
			AnchorMin = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// アンカーＸ最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMinX( float x )
		{
			AnchorMin = new Vector2( x, AnchorMin.y ) ;
		}
		
		/// <summary>
		/// アンカーＹ最小値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMinY( float y )
		{
			AnchorMin = new Vector2( AnchorMin.x, y ) ;
		}
	
		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public Vector2 AnchorMax
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return Vector2.zero ;
				}
				return rectTransform.anchorMax ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = value ;
			}
		}
		
		/// <summary>
		/// アンカー最大値を設定
		/// </summary>
		/// <param name="tAnchorMax"></param>
		public void SetAnchorMax( Vector2 anchorMax )
		{
			AnchorMax = anchorMax ;
		}
		
		/// <summary>
		/// アンカー最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMax( float x, float y )
		{
			AnchorMax = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// アンカーＸ最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMaxX( float x )
		{
			AnchorMax = new Vector2( x, AnchorMax.y ) ;
		}
		
		/// <summary>
		/// アンカーＹ最大値を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchorMaxY( float y )
		{
			AnchorMax = new Vector2( AnchorMax.x, y ) ;
		}
		
		/// <summary>
		/// アンカー最小値・最大値を設定
		/// </summary>
		/// <param name="tAnchorMin"></param>
		/// <param name="tAnchorMax"></param>
		public void SetAnchorMinAndMax( Vector2 anchorMin, Vector2 anchorMax )
		{
			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}
		
		/// <summary>
		/// アンカー最大値・最小値を設定
		/// </summary>
		/// <param name="tAnchorMinX"></param>
		/// <param name="tAnchorMinY"></param>
		/// <param name="tAnchorMaxX"></param>
		/// <param name="tAnchorMaxY"></param>
		public void SetAnchorMinAndMax( float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY )
		{
			AnchorMin = new Vector2( anchorMinX, anchorMinY ) ;
			AnchorMax = new Vector2( anchorMaxX, anchorMaxY ) ;
		}
		
		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMinX
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMin.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = new Vector2( value, rectTransform.anchorMin.y ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMinY
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMin.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMin = new Vector2( rectTransform.anchorMin.x, value ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxX
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMax.x ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = new Vector2( value, rectTransform.anchorMax.y ) ;
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxY
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return 0 ;
				}
				return rectTransform.anchorMax.y ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.anchorMax = new Vector2( rectTransform.anchorMax.x, value ) ;
			}
		}

		/// <summary>
		/// アンカーのＸ値を設定する
		/// </summary>
		/// <param name="tMinX"></param>
		/// <param name="tMaxX"></param>
		public void SetAnchorX( float minX, float maxX )
		{
			Vector2 anchorMin = AnchorMin ;
			Vector2 anchorMax = AnchorMax ;

			anchorMin.x = minX ;
			anchorMax.x = maxX ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}

		/// <summary>
		/// アンカーのＹ値を設定する
		/// </summary>
		/// <param name="tMinY"></param>
		/// <param name="tMaxY"></param>
		public void SetAnchorY( float minY, float maxY )
		{
			Vector2 anchorMin = AnchorMin ;
			Vector2 anchorMax = AnchorMax ;

			anchorMin.y = minY ;
			anchorMax.y = maxY ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}


		/// <summary>
		/// アンカーを位置から設定
		/// </summary>
		/// <param name="tAnchors"></param>
		public void SetAnchors( UIAnchors anchors )
		{
			switch( anchors )
			{
				case UIAnchors.LeftTop			: SetAnchorMinAndMax( 0.0f, 1.0f, 0.0f, 1.0f ) ; break ;
				case UIAnchors.CenterTop		: SetAnchorMinAndMax( 0.5f, 1.0f, 0.5f, 1.0f ) ; break ;
				case UIAnchors.RightTop			: SetAnchorMinAndMax( 1.0f, 1.0f, 1.0f, 1.0f ) ; break ;
				case UIAnchors.StretchTop		: SetAnchorMinAndMax( 0.0f, 1.0f, 1.0f, 1.0f ) ; break ;
			
				case UIAnchors.LeftMiddle		: SetAnchorMinAndMax( 0.0f, 0.5f, 0.0f, 0.5f ) ; break ;
				case UIAnchors.CenterMiddle		: SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; break ;
				case UIAnchors.RightMiddle		: SetAnchorMinAndMax( 1.0f, 0.5f, 1.0f, 0.5f ) ; break ;
				case UIAnchors.StretchMiddle	: SetAnchorMinAndMax( 0.0f, 0.5f, 1.0f, 0.5f ) ; break ;
			
				case UIAnchors.LeftBottom		: SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 0.0f ) ; break ;
				case UIAnchors.CenterBottom		: SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 0.0f ) ; break ;
				case UIAnchors.RightBottom		: SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 0.0f ) ; break ;
				case UIAnchors.StretchBottom	: SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 0.0f ) ; break ;
			
				case UIAnchors.LeftStretch		: SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 1.0f ) ; break ;
				case UIAnchors.CenterStretch	: SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 1.0f ) ; break ;
				case UIAnchors.RightStretch		: SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 1.0f ) ; break ;
				case UIAnchors.Stretch			: SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 1.0f ) ; break ;
			
				case UIAnchors.Center			: SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; break ;
			}
		}
		
		/// <summary>
		/// アンカーを左上に設定
		/// </summary>
		public void SetAnchorToLeftTop()		{ SetAnchorMinAndMax( 0.0f, 1.0f, 0.0f, 1.0f ) ; }

		/// <summary>
		/// アンカーを中上に設定
		/// </summary>
		public void SetAnchorToCenterTop()		{ SetAnchorMinAndMax( 0.5f, 1.0f, 0.5f, 1.0f ) ; }

		/// <summary>
		/// アンカーを右上に設定
		/// </summary>
		public void SetAnchorToRightTop()		{ SetAnchorMinAndMax( 1.0f, 1.0f, 1.0f, 1.0f ) ; }

		/// <summary>
		/// アンカーを全上に設定
		/// </summary>
		public void SetAnchorToStretchTop()		{ SetAnchorMinAndMax( 0.0f, 1.0f, 1.0f, 1.0f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左中に設定
		/// </summary>
		public void SetAnchorToLeftMiddle()		{ SetAnchorMinAndMax( 0.0f, 0.5f, 0.0f, 0.5f ) ; }

		/// <summary>
		/// アンカーを中中に設定
		/// </summary>
		public void SetAnchorToCenterMiddle()	{ SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; }

		/// <summary>
		/// アンカーを右中に設定
		/// </summary>
		public void SetAnchorToRightMiddle()	{ SetAnchorMinAndMax( 1.0f, 0.5f, 1.0f, 0.5f ) ; }

		/// <summary>
		/// アンカーを全中に設定
		/// </summary>
		public void SetAnchorToStretchMiddle()	{ SetAnchorMinAndMax( 0.0f, 0.5f, 1.0f, 0.5f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左下に設定
		/// </summary>
		public void SetAnchorToLeftBottom()		{ SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 0.0f ) ; }

		/// <summary>
		/// アンカーを中下に設定
		/// </summary>
		public void SetAnchorToCenterBottom()	{ SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 0.0f ) ; }

		/// <summary>
		/// アンカーを右下に設定
		/// </summary>
		public void SetAnchorToRightBottom()	{ SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 0.0f ) ; }

		/// <summary>
		/// アンカーを全下に設定
		/// </summary>
		public void SetAnchorToStretchBottom()	{ SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 0.0f ) ; SetMarginX( 0, 0 ) ; }
		
		/// <summary>
		/// アンカーを左全に設定
		/// </summary>
		public void SetAnchorToLeftStretch()	{ SetAnchorMinAndMax( 0.0f, 0.0f, 0.0f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを中全に設定
		/// </summary>
		public void SetAnchorToCenterStretch()	{ SetAnchorMinAndMax( 0.5f, 0.0f, 0.5f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを右全に設定
		/// </summary>
		public void SetAnchorToRightStretch()	{ SetAnchorMinAndMax( 1.0f, 0.0f, 1.0f, 1.0f ) ; SetMarginY( 0, 0 ) ; }

		/// <summary>
		/// アンカーを全全に設定
		/// </summary>
		public void SetAnchorToStretch()		{ SetAnchorMinAndMax( 0.0f, 0.0f, 1.0f, 1.0f ) ; SetMargin( 0, 0, 0, 0 ) ; }
		
		/// <summary>
		///  アンカーを中中に設定
		/// </summary>
		public void SetAnchorToCenter()			{ SetAnchorMinAndMax( 0.5f, 0.5f, 0.5f, 0.5f ) ; }

		/// <summary>
		/// マージン
		/// </summary>
		public RectOffset Margin
		{
			get
			{
				return GetMargin() ;
			}
			set
			{
				SetMargin( value ) ;
			}
		}

		/// <summary>
		/// マージンを取得
		/// </summary>
		/// <param name="tMargin"></param>
		public RectOffset GetMargin()
		{
			RectOffset margin = new RectOffset() ;

			GetMargin( out float left, out float right, out float top, out float bottom ) ;

			margin.left		= ( int )left ;
			margin.right	= ( int )right ;
			margin.top		= ( int )top ;
			margin.bottom	= ( int )bottom ;

			return margin ;
		}

		/// <summary>
		/// マージンを取得
		/// </summary>
		/// <param name="tLeft"></param>
		/// <param name="tRight"></param>
		/// <param name="tTop"></param>
		/// <param name="tBottom"></param>
		public void GetMargin( out float left, out float right, out float top, out float bottom )
		{
			left	= 0 ;
			right	= 0 ;

			top		= 0 ;
			bottom	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				if( rectTransform.anchorMin.x != rectTransform.anchorMax.x )
				{
					// 横方向はマージン設定が可能な状態
					float px = Pivot.x ;
					float x = rectTransform.anchoredPosition3D.x ;
					float w = rectTransform.sizeDelta.x ;

					right		= - x - ( w * ( 1 - px ) ) ;
					left		=   x - ( w *       px )   ;
				}

				if( rectTransform.anchorMin.y != rectTransform.anchorMax.y )
				{
					// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
					float py = Pivot.y ;
					float y = rectTransform.anchoredPosition3D.y ;
					float h = rectTransform.sizeDelta.y ;

					top			= - y - ( h * ( 1 - py ) ) ;
					bottom		=   y - ( h *       py )   ;
				}
			}
		}

		/// <summary>
		/// 横方向のマージンを取得
		/// </summary>
		/// <param name="tLeft"></param>
		/// <param name="tRight"></param>
		public void GetMarginX( out float left, out float right )
		{
			left	= 0 ;
			right	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.x != rectTransform.anchorMax.x )
			{
				// 横方向はマージン設定が可能な状態
				float px = Pivot.x ;
				float x = rectTransform.anchoredPosition3D.x ;
				float w = rectTransform.sizeDelta.x ;

				right		= - x - ( w * ( 1 - px ) ) ;
				left		=   x - ( w *       px )   ;
			}
		}

		/// <summary>
		/// 縦方向のマージンを取得
		/// </summary>
		/// <param name="tTop"></param>
		/// <param name="tBottom"></param>
		public void GetMarginY( out float top, out float bottom )
		{
			top		= 0 ;
			bottom	= 0 ;

			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.y != rectTransform.anchorMax.y )
			{
				// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
				float py = Pivot.y ;
				float y = rectTransform.anchoredPosition3D.y ;
				float h = rectTransform.sizeDelta.y ;

				top			= - y - ( h * ( 1 - py ) ) ;
				bottom		=   y - ( h *       py )   ;
			}
		}

		/// <summary>
		/// マージンを設定
		/// </summary>
		/// <param name="tMargin"></param>
		public void SetMargin( RectOffset margin )
		{
			if( margin == null )
			{
				return ;
			}

			SetMargin( margin.left, margin.right, margin.top, margin.bottom ) ;
		}

		/// <summary>
		/// マージンを設定
		/// </summary>
		/// <param name="tLeft"></param>
		/// <param name="tRight"></param>
		/// <param name="tTop"></param>
		/// <param name="tBottom"></param>
		public void SetMargin( float left, float right, float top, float bottom )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null )
			{
				float x, w ;
				float px = Pivot.x ;

				if( rectTransform.anchorMin.x != rectTransform.anchorMax.x )
				{
					// 横方向はマージン設定が可能な状態
					x = ( left * ( 1.0f - px ) ) - ( right * px ) ;
					w = - left - right ;
				}
				else
				{
					x = rectTransform.anchoredPosition3D.x ;
					w = rectTransform.sizeDelta.y ;
				}

				float y, h ;
				float py = Pivot.y ;

				if( rectTransform.anchorMin.y != rectTransform.anchorMax.y )
				{
					// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
					y = ( ( bottom * ( 1.0f - py ) ) - ( top * py ) ) ;
					h = - bottom - top ;
				}
				else
				{
					y = rectTransform.anchoredPosition3D.y ;
					h = rectTransform.sizeDelta.y ;
				}

				rectTransform.anchoredPosition3D = new Vector3( x, y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( w, h ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// 横方向のマージンを設定
		/// </summary>
		/// <param name="tLeft"></param>
		/// <param name="tRight"></param>
		public void SetMarginX( float left, float right )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.x != rectTransform.anchorMax.x )
			{
				// 横方向はマージン設定が可能な状態
				float px = Pivot.x ;
				float x = ( left * ( 1.0f - px ) ) - ( right * px ) ;
				float w = - left - right ;

				rectTransform.anchoredPosition3D = new Vector3( x, rectTransform.anchoredPosition3D.y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( w, rectTransform.sizeDelta.y ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
		
		/// <summary>
		/// 縦方向のマージンを設定
		/// </summary>
		/// <param name="tTop"></param>
		/// <param name="tBottom"></param>
		public void SetMarginY( float top, float bottom )
		{
			RectTransform rectTransform = GetRectTransform() ;
			if( rectTransform != null && rectTransform.anchorMin.y != rectTransform.anchorMax.y )
			{
				// 縦方向はマージン設定が可能な状態(座標系が Top=Right Bottom=Left なのに注意)
				float py = Pivot.y ;
				float y = ( ( bottom * ( 1.0f - py ) ) - ( top * py ) ) ;
				float h = - bottom - top ;

				rectTransform.anchoredPosition3D = new Vector3( rectTransform.anchoredPosition3D.x, y, rectTransform.anchoredPosition3D.z ) ;
				rectTransform.sizeDelta = new Vector2( rectTransform.sizeDelta.x, h ) ;

				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}
	
		/// <summary>
		/// ピボット(ショートカト)
		/// </summary>
		public Vector2 Pivot
		{
			get
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return new Vector2( 0.5f, 0.5f ) ;
				}
				return rectTransform.pivot ;
			}
			set
			{
				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.pivot = value ;
			}
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPivot( float x, float y, bool correct = false )
		{
			if( correct == true )
			{
				// 表示位置は変化させない

				Vector2 pivot = Pivot ;
				
				float w = this.Width ;
				float dx0 = w * pivot.x ;
				float dx1 = w * x ;

				this.Rx += ( dx1 - dx0 ) ;

				float h = this.Height ;
				float dy0 = h * pivot.y ;
				float dy1 = h * y ;

				this.Ry += ( dy1 - dy0 ) ;
			}

			Pivot = new Vector2( x, y ) ;
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="tPivot"></param>
		public void SetPivot( Vector2 pivot, bool correct = false )
		{
			SetPivot( pivot.x, pivot.y, correct ) ;
		}
		
		/// <summary>
		/// 横方向のピボットを設定
		/// </summary>
		/// <param name="x"></param>
		public void SetPivotX( float x, bool correct = false )
		{
			SetPivot( x, Pivot.y, correct ) ;
		}

		/// <summary>
		/// 縦方向のピボットを設定
		/// </summary>
		/// <param name="x"></param>
		public void SetPivotY( float y, bool correct = false )
		{
			SetPivot( Pivot.x, y, correct ) ;
		}

		/// <summary>
		/// ピボットを位置から設定
		/// </summary>
		/// <param name="tPivots"></param>
		public void SetPivot( UIPivots pivots, bool correct = false )
		{
			switch( pivots )
			{
				case UIPivots.LeftTop		: SetPivot( 0.0f, 1.0f, correct ) ; break ;
				case UIPivots.CenterTop		: SetPivot( 0.5f, 1.0f, correct ) ; break ;
				case UIPivots.RightTop		: SetPivot( 1.0f, 1.0f, correct ) ; break ;
			
				case UIPivots.LeftMiddle	: SetPivot( 0.0f, 0.5f, correct ) ; break ;
				case UIPivots.CenterMiddle	: SetPivot( 0.5f, 0.5f, correct ) ; break ;
				case UIPivots.RightMiddle	: SetPivot( 1.0f, 0.5f, correct ) ; break ;
			
				case UIPivots.LeftBottom	: SetPivot( 0.0f, 0.0f, correct ) ; break ;
				case UIPivots.CenterBottom	: SetPivot( 0.5f, 0.0f, correct ) ; break ;
				case UIPivots.RightBottom	: SetPivot( 1.0f, 0.0f, correct ) ; break ;
			
				case UIPivots.Center		: SetPivot( 0.5f, 0.5f, correct ) ; break ;
			}
		}
		
		/// <summary>
		/// ピボットを左上に設定
		/// </summary>
		public void SetPivotToLeftTop( bool correct		= false ){ SetPivot( 0.0f, 1.0f, correct ) ; }

		/// <summary>
		/// ピボットを中上に設定
		/// </summary>
		public void SetPivotToCenterTop( bool correct		= false ){ SetPivot( 0.5f, 1.0f, correct ) ; }

		/// <summary>
		/// ピボットを右上に設定
		/// </summary>
		public void SetPivotToRightTop( bool correct		= false ){ SetPivot( 1.0f, 1.0f, correct ) ; }
		
		/// <summary>
		/// ピボットを左中に設定
		/// </summary>
		public void SetPivotToLeftMiddle( bool correct		= false ){ SetPivot( 0.0f, 0.5f, correct ) ; }

		/// <summary>
		/// ピボットを中中に設定
		/// </summary>
		public void SetPivotToCenterMiddle( bool correct	= false ){ SetPivot( 0.5f, 0.5f, correct ) ; }

		/// <summary>
		/// ピボットを右中に設定
		/// </summary>
		public void SetPivotToRightMiddle( bool correct	= false ){ SetPivot( 1.0f, 0.5f, correct ) ; }
		
		/// <summary>
		/// ピボットを左下に設定
		/// </summary>
		public void SetPivotToLeftBottom( bool correct		= false ){ SetPivot( 0.0f, 0.0f, correct ) ; }

		/// <summary>
		/// ピボットを中下に設定
		/// </summary>
		public void SetPivotToCenterBottom( bool correct	= false ){ SetPivot( 0.5f, 0.0f, correct ) ; }

		/// <summary>
		/// ピボットを右下に設定
		/// </summary>
		public void SetPivotToRightBottom( bool correct	= false ){ SetPivot( 1.0f, 0.0f, correct ) ; }
		
		/// <summary>
		/// ピボットを中中に設定
		/// </summary>
		public void SetPivotToCenter( bool correct			= false ){ SetPivot( 0.5f, 0.5f, correct ) ; }

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalRotation = Vector3.zero ;
		public  Vector3   LocalRotation
		{
			get
			{
				return m_LocalRotation ;
			}
			set
			{
				m_LocalRotation = value ;
			}
		}

		/// <summary>
		/// ローテーション(ショートカット)
		/// </summary>
		public Vector3 Rotation
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.localEulerAngles = value ;
			}
		}

		public float Pitch
		{
			get
			{
				return Rotation.x ;
			}
			set
			{
				Rotation = new Vector3( value, Rotation.y, Rotation.z ) ;
			}
		}

		public float Yaw
		{
			get
			{
				return Rotation.y ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, value, Rotation.z ) ;
			}
		}

		public float Roll
		{
			get
			{
				return Rotation.z ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, Rotation.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_LocalScale = Vector3.one ;
		public  Vector3   LocalScale
		{
			get
			{
				return m_LocalScale ;
			}
			set
			{
				m_LocalScale = value ;
			}
		}

		/// <summary>
		/// スケール(ショートカット)
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalScale ;
				}
				else
				{
					RectTransform rectTransform = GetRectTransform() ;
					if( rectTransform == null )
					{
						return Vector3.zero ;
					}
					return rectTransform.localScale ;
				}
			}
			set
			{
				m_LocalScale = value ;

				RectTransform rectTransform = GetRectTransform() ;
				if( rectTransform == null )
				{
					return ;
				}
				rectTransform.localScale = value ;
			}
		}
		
		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="s"></param>
		public void SetScale( float s )
		{
			Scale = new Vector3( s, s, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y )
		{
			Scale = new Vector3( x, y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y,float z )
		{
			Scale = new Vector3( x, y, z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector2 scale )
		{
			Scale = new Vector3( scale.x, scale.y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector3 scale )
		{
			Scale = scale ;
		}

		//-----------------------------------

		[SerializeField][HideInInspector]
		private float m_LocalAlpha = 1.0f ;
		public  float   LocalAlpha
		{
			get
			{
				return m_LocalAlpha ;
			}
			set
			{
				m_LocalAlpha = value ;
			}
		}

		/// <summary>
		/// α値
		/// </summary>
		public float Alpha
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalAlpha ;
				}
				else
				{
					CanvasGroup canvasGroup = GetCanvasGroup() ;
					if( canvasGroup == null )
					{
						return 1.0f ;
					}
					return canvasGroup.alpha ;
				}
			}
			set
			{
				m_LocalAlpha = value ;

				CanvasGroup canvasGroup = GetCanvasGroup() ;
				if( canvasGroup == null )
				{
					return ;
				}

				canvasGroup.alpha = value * ( m_Visible == true ? 1 : 0 ) ;

				if( canvasGroup.alpha <  disableRaycastUnderAlpha )
				{
					canvasGroup.blocksRaycasts = false ;	// 無効
				}
				else
				{
					canvasGroup.blocksRaycasts = true ;	// 有効
				}
			}
		}
		
		//----------------------------------------------------

		/// <summary>
		/// マテリアルタイプ
		/// </summary>
		public enum MaterialTypes
		{
			Default = 0,
			Grayscale = 1,
			Interpolation = 2,
			Mosaic = 3,
			Blur = 4,
		}

		[SerializeField][HideInInspector]
		private MaterialTypes m_MaterialType = MaterialTypes.Default ;

		/// <summary>
		/// マテリアルタイプ
		/// </summary>
		public  MaterialTypes  MaterialType
		{
			get
			{
				return m_MaterialType ;
			}
			set
			{
				if( m_MaterialType != value )
				{
					m_MaterialType  = value ;

					Graphic graphic = GetGraphic() ;
	
					if( graphic != null )
					{
						graphic.material = null ;
						graphic.GraphicUpdateComplete() ;
					}

					if( m_ActiveMaterial != null )
					{
						DestroyImmediate( m_ActiveMaterial ) ;
						m_ActiveMaterial = null ;
					}

					if( m_MaterialType != MaterialTypes.Default )
					{
						if( m_MaterialType == MaterialTypes.Grayscale )
						{
							m_ActiveMaterial = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-Grayscale" ) ) ;
						}
						else
						if( m_MaterialType == MaterialTypes.Interpolation )
						{
							m_ActiveMaterial = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-Interpolation" ) ) ;
						}
						else
						if( m_MaterialType == MaterialTypes.Mosaic )
						{
							m_ActiveMaterial = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-Mosaic" ) ) ;
						}
//						else
//						if( m_MaterialType == MaterialType.Blur )
//						{
//							m_ActiveMaterial = Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-Mosaic" ) ) ;
//						}

						if( graphic != null && m_ActiveMaterial != null )
						{
							graphic.material = m_ActiveMaterial ;
							graphic.GraphicUpdateComplete() ;
						}

						if( m_MaterialType == MaterialTypes.Interpolation )
						{
							ProcessInterpolation() ;
						}
						else
						if( m_MaterialType == MaterialTypes.Mosaic )
						{
							ProcessMosaic() ;
						}
//						else
//						if( m_MaterialType == MaterialType.Blur )
//						{
//							materialValue = m_MaterialValue ;
//						}
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private Material m_ActiveMaterial = null ;

		//-----------------------------------

		[SerializeField][HideInInspector]
		private float m_InterpolationValue = 1.0f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  InterpolationValue
		{
			get
			{
				return m_InterpolationValue ;
			}
			set
			{
				m_InterpolationValue = value ;
				ProcessInterpolation() ;
			}
		}

		[SerializeField][HideInInspector]
		private Color m_InterpolationColor = Color.white ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  Color InterpolationColor
		{
			get
			{
				return m_InterpolationColor ;
			}
			set
			{
				m_InterpolationColor = value ;
				ProcessInterpolation() ;
			}
		}

		private bool ProcessInterpolation()
		{
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Interpolation || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			graphic.materialForRendering.SetFloat( "_InterpolationValue", m_InterpolationValue ) ;
			graphic.materialForRendering.SetColor( "_InterpolationColor", m_InterpolationColor ) ;

			return true ;
		}
		
		//-----------------------------------

		[SerializeField][HideInInspector]
		private float m_MosaicIntensity = 0.5f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  MosaicIntensity
		{
			get
			{
				return m_MosaicIntensity ;
			}
			set
			{
				m_MosaicIntensity = value ;
				ProcessMosaic() ;
			}
		}

		[SerializeField][HideInInspector]
		private bool  m_MosaicSquareization = false ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  bool  MosaicSquareization
		{
			get
			{
				return m_MosaicSquareization ;
			}
			set
			{
				m_MosaicSquareization = value ;
				ProcessMosaic() ;
			}
		}

		//---------------

		private bool ProcessMosaic()
		{
			Graphic graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Mosaic || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			float intensity = 1.0f - m_MosaicIntensity ;
			intensity *= intensity ;

#if false
			float w = this.Width ;
			float h = this.Height ;

			float sw, sh ;
			float cw, ch ;
				
			if( w >= h )
			{
				// 横の方が長いので横を基準とする

				if( m_MosaicIntensity == 0 )
				{
					// モザイク無し
					sw = w ;
					cw = 0 ;

					sh = h ;
					ch = 0 ;
				}
				else
				{
					// モザイク有り
					if( w <  1 )
					{
						w  = 1 ;
					}
					sw = ( int )( ( ( int )w - 1 ) * intensity + 1 ) ;
					cw = 0.5f / sw ;

					if( m_MosaicSquareization == false )
					{
						// 正方形補正無し
						sh = sw ;
						ch = cw ;
					}
					else
					{
						// 正方形補正有り
						if( h <  1 )
						{
							h  = 1 ;
						}
						sh = ( int )( ( ( int )h - 1 ) * intensity + 1 ) ;
						ch = 0.5f / sh ;
					}
				}
			}
			else
			{
				// 縦の方が長いので縦を基準とする

				if( m_MosaicIntensity == 0 )
				{
					// モザイク無し
					sh = h ;
					ch = 0 ;

					sw = w ;
					cw = 0 ;
				}
				else
				{
					// モザイク有り
					if( h <  1 )
					{
						h  = 1 ;
					}
					sh = ( int )( ( ( int )h - 1 ) * intensity + 1 ) ;
					ch = 0.5f / sh ;

					if( m_MosaicSquareization == false )
					{
						// 正方形補正無し
						sw = sh ;
						cw = ch ;
					}
					else
					{
						// 正方形補正有り
						if( w <  1 )
						{
							w  = 1 ;
						}
						sw = ( int )( ( ( int )w - 1 ) * intensity + 1 ) ;
						cw = 0.5f / sw ;
					}
				}
			}

			Debug.LogWarning( "モザイク:" + new Vector4( sw, sh, cw, ch ) ) ;
#endif
			graphic.materialForRendering.SetFloat( "_MosaicIntensity", m_MosaicIntensity ) ;

			return true ;
		}


		//-----------------------------------

		[SerializeField][HideInInspector]
		private float m_MaterialValue = 1.0f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float MaterialValue
		{
			get
			{
				return m_MaterialValue ;
			}
			set
			{
				m_MaterialValue = value ;

				if( m_ActiveMaterial == null )
				{
					return ;
				}

				Graphic graphic = GetGraphic() ;

				if( m_MaterialType == MaterialTypes.Mosaic )
				{
					if( graphic != null )
					{
						graphic.materialForRendering.SetFloat( "_Range", m_MaterialValue * m_MaterialValue * 64.0f + 6.0f ) ;
					}
//					mActiveMaterial.SetFloat( "_Range", mMaterialValue * mMaterialValue * 64.0f + 6.0f ) ;
				}
				else
				if( m_MaterialType == MaterialTypes.Blur )
				{
					if( graphic != null )
					{
						graphic.materialForRendering.SetFloat( "_Range", m_MaterialValue * m_MaterialValue * 64.0f + 6.0f ) ;
					}
//					mActiveMaterial.SetFloat( "_Range", mMaterialValue ) ;				
				}
			}
		}

		//----------------------------------------------------

		//-------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveTweenIdentity = null ;

		public  string  RemoveTweenIdentity
		{
			set
			{
				m_RemoveTweenIdentity = value ;
			}
		}

		private int    m_RemoveTweenInstance = 0 ;

		public  int     RemoveTweenInstance
		{
			set
			{
				m_RemoveTweenInstance = value ;
			}
		}

#endif
		
		/// <summary>
		/// Tween の追加
		/// </summary>
		/// <param name="tIdentity"></param>
		public UITween AddTween( string identity )
		{
			UITween tween = gameObject.AddComponent<UITween>() ;
			tween.Identity = identity ;

			return tween ;
		}
		
		/// <summary>
		/// Tween の削除
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tInstance"></param>
		public void RemoveTween( string identity, int instance = 0 )
		{
			UITween[] tweens = GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return ;
			}
			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && tweens[ i ].Identity == identity ) || ( instance != 0 && tweens[ i ].Identity == identity && tweens[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tweens[ i ] ) ;
			}
			else
			{
				Destroy( tweens[ i ] ) ;
			}
		}

		/// <summary>
		/// Tween の取得
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public UITween GetTween( string identity )
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweens[ i ].Identity == identity )
				{
					return tweens[ i ] ;
				}
			}

			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,UITween> GetTweenAll()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,UITween> targets = new Dictionary<string, UITween>() ;

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( tweens[ i ].Identity ) == false )
				{
					if( targets.ContainsKey( tweens[ i ].Identity ) == false )
					{
						targets.Add( tweens[ i ].Identity, tweens[ i ] ) ;
					}
				}
			}

			if( targets.Count == 0 )
			{
				return null ;
			}

			return targets ;
		}

		/// <summary>
		/// Tween の Delay と Duration を設定
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public bool SetTweenTime( string identity, float delay = -1, float duration = -1 )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Delay		= delay ;
			tween.Duration	= duration ;
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// 終了を待つ機構無しに再生する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <param name="tOffset"></param>
		/// <returns></returns>
		public bool PlayTweenDirect( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			tween.Play( delay, duration, offset, onFinishedAction ) ;

			return true ;
		}
		
		/// <summary>
		/// 非アクティブ状態の時のみ再生する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public MovableState PlayTweenIfHiding( string identity, float delay = -1, float duration = -1 )
		{
			return PlayTween( identity, delay, duration, 0, true, false ) ;
		}

		/// <summary>
		/// 再生終了と同時に非アクティブ状態にする
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public MovableState PlayTweenAndHide( string identity, float delay = -1, float duration = -1 )
		{
			return PlayTween( identity, delay, duration, 0, false, true ) ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public MovableState PlayTween( string identity, float delay = -1, float duration = -1, float offset = 0, bool ifHiding = false, bool autoHide = false )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return null ;
			}

			if( ifHiding == true && ( gameObject.activeSelf == true && m_Visible == true ) )
			{
				return null ;
			}

			if( autoHide == true && gameObject.activeSelf == false )
			{
				return null ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return null ;
			}

			MovableState tState = new MovableState() ;
			StartCoroutine( PlayTweenAsync_Private( tween, delay, duration, offset, autoHide, tState ) ) ;
			return tState ;
		}

		public IEnumerator PlayTweenAsync_Private( UITween tween, float delay, float duration, float offset, bool autoHide, MovableState state )
		{
			// 同じトゥイーンを多重実行出来ないようにする
			if( tween.IsRunning == true || tween.IsPlaying == true )
			{
//				tTween.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( tween.IsRunning == true ) | ( tween.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			tween.Play( delay, duration, offset ) ;

			yield return new WaitWhile( () => ( tween.IsRunning == true || tween.IsPlaying == true ) ) ;
			
			state.IsDone = true ;

			if( autoHide == true )
			{
				gameObject.SetActive( false ) ;
			}
		}
		
		/// <summary>
		/// 指定した Tween の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsTweenPlaying( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return true ;// 実行中
			}
			
			return false ;	
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlaying
		{
			get
			{
				UITween[] tweens = gameObject.GetComponents<UITween>() ;
				if( tweens == null || tweens.Length == 0 )
				{
					return false ;
				}

				int i, l = tweens.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tweens[ i ].enabled == true && (  tweens[ i ].IsRunning == true || tweens[ i ].IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// Tween の一時停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool PauseTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Pause() ;
			return true ;
		}

		/// <summary>
		/// Tween の再開
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool UnpauseTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Unpause() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool StopTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Stop() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止と状態のリセット
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool StopAndResetTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.StopAndReset() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool FinishTween( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Finish() ;
			return true ;
		}

		/// <summary>
		/// 全ての Tween の停止
		/// </summary>
		public bool StopTweenAll()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
				{
					tween.Stop() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool StopAndResetTweenAll()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.StopAndReset() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool FinishTweenAll()
		{
			UITween[] tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.Finish() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を取得する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public float GetTweenProcessTime( string identity )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return 0 ;
			}

			return tween.ProcessTime ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を設定する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tTime"></param>
		public bool SetTweenProcessTime( string identity, float time )
		{
			UITween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.ProcessTime = time ;

			return true ;
		}

		//-------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveFlipperIdentity = null ;

		public  string  RemoveFlipperIdentity
		{
			set
			{
				m_RemoveFlipperIdentity = value ;
			}
		}

		private int    m_RemoveFlipperInstance = 0 ;

		public  int     RemoveFlipperInstance
		{
			set
			{
				m_RemoveFlipperInstance = value ;
			}
		}

#endif

		/// <summary>
		/// Flipper の追加
		/// </summary>
		/// <param name="tIdentity"></param>
		public UIFlipper AddFlipper( string identity )
		{
			UIFlipper flipper = gameObject.AddComponent<UIFlipper>() ;
			flipper.identity = identity ;

			return flipper ;
		}
		
		/// <summary>
		/// Flipper の削除
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tInstance"></param>
		public void RemoveFlipper( string identity, int instance = 0 )
		{
			UIFlipper[] flippers = GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return ;
			}

			int i, l = flippers.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && flippers[ i ].identity == identity ) || ( instance != 0 && flippers[ i ].identity == identity && flippers[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( flippers[ i ] ) ;
			}
			else
			{
				Destroy( flippers[ i ] ) ;
			}
		}

		/// <summary>
		/// Flipper の取得
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public UIFlipper GetFlipper( string identity )
		{
			UIFlipper[] flippers = gameObject.GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return null ;
			}

			foreach( var flipper in flippers )
			{
				if( flipper.identity == identity )
				{
					return flipper ;
				}
			}

			return null ;
		}

		/// <summary>
		/// Flipper の再生
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tTimeScale"></param>
		/// <returns></returns>
		public bool PlayFlipperDirect( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1, Action<string,UIFlipper> onFinishedAction = null )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			flipper.Play( destroyAtEnd, speed, delay, onFinishedAction ) ;

			return true ;
		}

		/// <summary>
		///  Flipper の再生(コルーチン)
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public MovableState PlayFlipper( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1 )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return null ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return null ;
			}

			MovableState state = new MovableState() ;
			StartCoroutine( PlayFlipperAsync_Private( flipper, destroyAtEnd, speed, delay, state ) ) ;
			return state ;
		}

		public IEnumerator PlayFlipperAsync_Private( UIFlipper flipper, bool destroyAtEnd, float speed, float delay, MovableState state )
		{
			// 同じフリッパーを多重実行出来ないようにする
			if( flipper.isRunning == true || flipper.isPlaying == true )
			{
//				tFlipper.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( flipper.isRunning == true ) | ( flipper.isPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			flipper.Play( destroyAtEnd, speed, delay ) ;

			yield return new WaitWhile( () => ( flipper.isRunning == true || flipper.isPlaying == true ) ) ;

			state.IsDone = true ;
		}
		
		/// <summary>
		/// 指定した Flipper の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsFlipperPlaying( string identity )
		{
			UIFlipper flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.enabled == true && ( flipper.isRunning == true || flipper.isPlaying == true ) )
			{
				return true ;	// 実行中
			}
			
			return false ;
		}
		
		/// <summary>
		/// いずれかの Flipper の実行中の有無
		/// </summary>
		public bool IsAnyFlipperPlaying
		{
			get
			{
				UIFlipper[] flippers = gameObject.GetComponents<UIFlipper>() ;
				if( flippers == null || flippers.Length == 0 )
				{
					return false ;
				}

				foreach( var flipper in flippers )
				{
					if( flipper.enabled == true && ( flipper.isRunning == true || flipper.isPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		//-------------------------------------------------------
		
		/// <summary>
		/// 親 View を取得する
		/// </summary>
		/// <returns></returns>
		public UIView GetParentView()
		{
			if( transform.parent != null )
			{
				return transform.parent.gameObject.GetComponent<UIView>() ;
			}
			return null ;
		}

		/// <summary>
		/// 親 Canvas を取得する(自身を含む)
		/// </summary>
		/// <returns></returns>
		public Canvas GetParentCanvas()
		{
			int i, l = 64 ;

			Canvas canvas ;

			Transform t = gameObject.transform ;
			for( i  =  0 ; i <  l ; i ++ )
			{
				canvas = t.gameObject.GetComponent<Canvas>() ;
				if( canvas != null )
				{
					return canvas ;
				}
			
				t = t.parent ;
				if( t == null )
				{
					break ;
				}
			}

			return null ;
		}


		/// <summary>
		/// 親 Canvas の設定仮想解像度を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetParentCanvasScalerSize()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return Vector2.zero ;
			}

			CanvasScaler canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>() ;
			if( canvasScaler == null )
			{
				return Vector2.zero ;
			}

			return canvasScaler.referenceResolution ;
		}

		/// <summary>
		/// 親 Canvas の実仮想解像度を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCanvasSize()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return new Vector2( Screen.width, Screen.height ) ;
			}

			if( Application.isPlaying == false )
			{
				RectTransform rt = canvas.gameObject.GetComponent<RectTransform>() ;
				if( rt == null )
				{
					return  new Vector2( Screen.width, Screen.height ) ;
				}
	
				return rt.sizeDelta ;
			}

			CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ;
			if( scaler == null )
			{
				return new Vector2( Screen.width, Screen.height ) ;
			}

			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize )
			{
				return new Vector2( Screen.width / scaler.scaleFactor, Screen.height / scaler.scaleFactor ) ;
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize )
			{
				float rw = scaler.referenceResolution.x ;
				float rh = scaler.referenceResolution.y ;

				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight )
				{
					float mf = scaler.matchWidthOrHeight ;

					float wa0 = ( float )Screen.width / ( float )Screen.height ;
					float wa1 = rw / rh ;
					float wa = Mathf.Lerp( wa0, wa1, mf ) ;

					float w  = rw * wa0 / wa ;

					float ha0 = rh / rw ;
					float ha1 = ( float )Screen.height / ( float )Screen.width ;
					float ha = Mathf.Lerp( ha0, ha1, mf ) ;

					float h  = rh * ha1 / ha ;

					return new Vector2( w, h ) ;
				}
				else
				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand )
				{
					float w, h ;

					if( Screen.width >= Screen.height )
					{
						// 実スクリーンは横長
						float sa = ( float )Screen.width / ( float )Screen.height ;
						float ra = rw / rh ;

						if( ra >= sa )
						{
							// 横が１倍
							w = rw ;
							h = rh * ra / sa ;
						}
						else
						{
							// 縦が１倍
							h = rh ;
							w = rw * sa / ra ;
						}
					}
					else
					{
						// 実スクリーンは縦長
						float sa = ( float )Screen.height / ( float )Screen.width ;
						float ra = rh / rw ;

						if( ra >= sa )
						{
							// 縦が１倍
							h = rh ;
							w = rw * ra / sa ;
						}
						else
						{
							// 横が１倍
							w = rw ;
							h = rh * sa / ra ;
						}
					}

					return new Vector2( w, h ) ;
				}
				else
				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Shrink )
				{
					float w, h ;

					if( Screen.width >= Screen.height )
					{
						// 実スクリーンは横長
						float sa = ( float )Screen.width / ( float )Screen.height ;
						float ra = rw / rh ;

						if( ra >= sa )
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * ( float )Screen.width / ( float )Screen.height ;
						}
						else
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * ( float )Screen.height / ( float )Screen.width ;
						}
					}
					else
					{
						// 実スクリーンは縦長
						float sa = ( float )Screen.height / ( float )Screen.width ;
						float ra = rh / rw ;

						if( ra >= sa )
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * ( float )Screen.height / ( float )Screen.width ;
						}
						else
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * ( float )Screen.width / ( float )Screen.height ;
						}
					}

					return new Vector2( w, h ) ;
				}
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize )
			{
				RectTransform rt = canvas.gameObject.GetComponent<RectTransform>() ;
				if( rt == null )
				{
					return  new Vector2( Screen.width, Screen.height ) ;
				}
	
				return rt.sizeDelta ;
			}

			return new Vector2( Screen.width, Screen.height ) ;


//			RectTransform tRectTransform = tCanvas.gameObject.GetComponent<RectTransform>() ;
//			if( tRectTransform == null )
//			{
//				return Vector2.zero ;
//			}
//
//			return tRectTransform.sizeDelta ;
		}

		/// <summary>
		/// 親キャンバスの基準となる長さを取得する
		/// </summary>
		/// <param name="tRatio"></param>
		/// <returns></returns>
		public float GetCanvasLength( float ratio = 1 )
		{
			Vector2 size = GetParentCanvasScalerSize() ;

			if( size.x >= size.y )
			{
				return size.x * ratio ;
			}
			else
			{
				return size.y * ratio ;
			}
		}

		/// <summary>
		/// 親 Canvas に設定されているワールドカメラを取得する
		/// </summary>
		/// <returns></returns>
		public Camera GetCanvasCamera()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return null ;
			}

			return canvas.worldCamera ;
		}

		/// <summary>
		/// Canvas 描画対象の Layer を取得する
		/// </summary>
		/// <returns></returns>
		public uint GetCanvasTargetLayer()
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return 0 ;	// 不明
			}

			if( canvas.worldCamera != null )
			{
				return ( uint )canvas.worldCamera.cullingMask ;
			}
			else
			{
				return ( uint )( 1 << canvas.gameObject.layer ) ;
			}
		}
		
		/// <summary>
		/// Canvas の描画対象 Layer の内で最も最初のものを取得する
		/// </summary>
		/// <returns></returns>
		public int GetCanvasTargetLayerOfFirst()
		{
			uint layer = GetCanvasTargetLayer() ;
			if( layer == 0 )
			{
				return 5 ;
			}
		
			int i, l = 32 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( layer & ( uint )( 1 << i ) ) != 0 )
				{
					return i ;
				}
			}
		
			return 5 ;
		}
		
		/// <summary>
		/// 属するキャンバスのオーバーレイ指定状態
		/// </summary>
		public bool IsCanvasOverlay
		{
			get
			{
				int i, l = 64 ;

				UICanvas canvas = null ;

				Transform t = gameObject.transform ;
				for( i  =  0 ; i <  l ; i ++ )
				{
					canvas = t.gameObject.GetComponent<UICanvas>() ;
					if( canvas != null )
					{
						break ;
					}
			
					t = t.parent ;
					if( t == null )
					{
						break ;
					}
				}
				
				if( canvas == null )
				{
					return false ;
				}

				return canvas.isOverlay ;
			}
		}

		//---------------------------------------------------------------

		/// <summary>
		/// View を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddView<T>() where T : UIView
		{
			return AddView<T>( "" ) ;
		}

		/// <summary>
		/// View を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tName"></param>
		/// <param name="tOption"></param>
		/// <returns></returns>
		public T AddView<T>( string viewName, string option = "" ) where T : UIView
		{
			// クラスの名前を取得する
			if( string.IsNullOrEmpty( viewName ) == true )
			{
				viewName = typeof( T ).ToString() ;

				int i ;

				i = viewName.IndexOf( "." ) ;
				if( i >= 0 )
				{
					viewName = viewName.Substring( i ) ;
				}

				i = viewName.IndexOf( "UI" ) ;
				if( i >= 0 )
				{
					viewName = viewName.Substring( i + 2, viewName.Length - ( i + 2 ) ) ;
				}
			}
		
			GameObject go = new GameObject( viewName, typeof( RectTransform ) ) ;
		
			if( go == null )
			{
				// 失敗
				return default ;
			}
		
			// 最初に親を設定してしまう
			go.transform.SetParent( gameObject.transform, false ) ;
		
			// コンポーネントをアタッチする
			T component = go.AddComponent<T>() ;
		
			if( component == null )
			{
				// 失敗
				Destroy( go ) ;
				return default ;
			}
			
			// AddView からの場合は　SetDefault を実行する
			component.SetDefault( option ) ;

			return component ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 空の GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public GameObject AddObject( string viewName, Transform t = null, int layer = -1 )
		{
			GameObject go = new GameObject( viewName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( t == null )
			{
				t = transform ;
			}

			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			return go ;
		}

		/// <summary>
		/// 指定のコンポーネントをアタッチしたの GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public T AddObject<T>( string viewName, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			GameObject go = new GameObject( viewName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( t == null )
			{
				t = transform ;
			}

			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			T component = go.AddComponent<T>() ;
			return component ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public GameObject AddPrefab( string path, Transform t = null, int layer = -1 )
		{
			GameObject go = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( go == null )
			{
				return null ;
			}

			go = Instantiate( go ) ;
		
			AddPrefab( go, t, layer ) ;
		
			return go ;
		}
	
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public GameObject AddPrefab( GameObject prefab, Transform t = null, int layer = -1 )
		{
			if( prefab == null )
			{
				return null ;
			}
			
			if( t == null )
			{
				t = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			return go ;
		}

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public T AddPrefab<T>( string path, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			GameObject prefab = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( prefab == null )
			{
				return null ;
			}

			return AddPrefab<T>( prefab, t, layer ) ;
		}
		
		/// <summary>
		/// プレハブからインスタンスを生成し自身の子とする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParentName"></param>
		/// <returns></returns>
		public T AddPrefabOnChild<T>( GameObject prefab, string parentName = null, int layer = -1 ) where T : UnityEngine.Component
		{
			Transform t = null ;
			if( string.IsNullOrEmpty( parentName ) == false )
			{
				if( transform.name.ToLower() == parentName.ToLower() )
				{
					t = transform ;
				}
				else
				{
					t = GetTransformByName( transform, parentName ) ;
				}
			}

			return AddPrefab<T>( prefab, t, layer ) ;
		}

		/// <summary>
		/// 自身に含まれる指定した名前のトランスフォームを検索する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Transform GetTransformByName( string viewName, bool isContains = false )
		{
			if( string.IsNullOrEmpty( viewName ) == true )
			{
				return null ;
			}

			return GetTransformByName( transform, viewName, isContains ) ;
		}

		// 自身に含まれる指定した名前のトランスフォームを検索する
		private Transform GetTransformByName( Transform t, string viewName, bool isContains = false )
		{
			viewName = viewName.ToLower() ;

			Transform child ;
			string childViewName ;
			bool result ;

			int i, l = t.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				child = t.GetChild( i ) ;

				childViewName = child.name.ToLower() ;

				result = false ;
				if( isContains == false && childViewName == viewName )
				{
					result = true ;
				}
				else
				if( isContains == true && childViewName.Contains( viewName ) == true )
				{
					result = true ;
				}

				if( result == true )
				{
					// 発見
					return child ;
				}
				else
				{
					if( child.childCount >  0 )
					{
						child = GetTransformByName( child, viewName ) ;
						if( child != null )
						{
							// 発見
							return child ;
						}
					}
				}
			}

			// 発見出来ず
			return null ;
		}

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public T AddPrefab<T>( GameObject prefab, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			if( prefab == null )
			{
				return default ;
			}
			
			if( t == null )
			{
				t = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( t, false ) ;

			if( layer >= -1 && layer <= 31 )
			{
				if( layer == -1 )
				{
					layer = t.gameObject.layer ;
				}
				SetLayer( go, layer ) ;
			}

			T tComponent = go.GetComponent<T>() ;
			return tComponent ;
		}

		/// <summary>
		/// 指定したゲームオブジェクトを自身の子にする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParent"></param>
		/// <returns></returns>
		public GameObject SetPrefab( GameObject go, Transform parent = null )
		{
			if( go == null )
			{
				return null ;
			}
		
			if( parent == null )
			{
				parent = transform ;
			}

			go.transform.SetParent( parent, false ) ;
			SetLayer( go, gameObject.layer ) ;

			return go ;
		}
		
		/// <summary>
		/// Layer を設定する
		/// </summary>
		/// <param name="tGameObject"></param>
		/// <param name="tLayer"></param>
		private void SetLayer( GameObject go, int layer )
		{
			go.layer = layer ;
			foreach( Transform t in go.transform )
			{
				SetLayer( t.gameObject, layer ) ;
			}
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Component を追加する(ショートカット)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T : UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}
		
		/// <summary>
		/// ARGB 32 ビットから Color を返す
		/// </summary>
		/// <param name="tColor"></param>
		/// <returns></returns>
		public static Color32 ARGB( uint color )
		{
			return new Color32( ( byte )( ( color >> 16 ) & 0xFF ), ( byte )( ( color >>  8 ) & 0xFF ), ( byte )( ( color & 0xFF ) ), ( byte )( ( color >> 24 ) & 0xFF ) ) ;
		}
	
		//-------------------------------------------------
	
		// Interface
		
		//----------

		// キャッシュ
		private Camera m_Camera = null ;

		/// <summary>
		/// Camera(ショートカット)
		/// </summary>
		virtual public Camera GetCamera()
		{
			if( m_Camera == null )
			{
				m_Camera = gameObject.GetComponent<Camera>() ;
			}
			return m_Camera ;
		}

		//----------

		// キャッシュ
		private RectTransform m_RectTransform ;

		/// <summary>
		/// RectTransform(ショートカット)
		/// </summary>
		virtual public RectTransform GetRectTransform()
		{
			if( m_RectTransform != null )
			{
				return m_RectTransform ;
			}
			m_RectTransform = gameObject.GetComponent<RectTransform>() ;
			return m_RectTransform ;
		}
	
		//----------

		// キャッシュ
		private Canvas m_Canvas = null ;

		/// <summary>
		/// Canvas(ショートカット)
		/// </summary>
		virtual public Canvas GetCanvas()
		{
			if( m_Canvas == null )
			{
				m_Canvas = gameObject.GetComponent<Canvas>() ;
			}
			return m_Canvas ;
		}
	
		//----------

		// キャッシュ
		private CanvasRenderer m_CanvasRenderer = null ;

		/// <summary>
		/// CanvasRenderer(ショートカット)
		/// </summary>
		virtual public CanvasRenderer GetCanvasRenderer()
		{
			if( m_CanvasRenderer == null )
			{
				m_CanvasRenderer = gameObject.GetComponent<CanvasRenderer>() ;
			}
			return m_CanvasRenderer ;
		}

		//----------

		// キャッシュ
		private CanvasScaler m_CanvasScaler = null ;

		/// <summary>
		/// CanvasScaler(ショートカット)
		/// </summary>
		virtual public CanvasScaler GetCanvasScaler()
		{
			if( m_CanvasScaler == null )
			{
				m_CanvasScaler = gameObject.GetComponent<CanvasScaler>() ;
			}
			return m_CanvasScaler ;
		}

		//----------

		// キャッシュ
		private CanvasGroup m_CanvasGroup = null ;

		/// <summary>
		/// CanvasGroup(ショートカット)
		/// </summary>
		virtual public CanvasGroup GetCanvasGroup()
		{
			if( m_CanvasGroup == null )
			{
				try
				{
					m_CanvasGroup = gameObject.GetComponent<CanvasGroup>() ;
				}
				catch( Exception e )
				{
					Debug.LogError( "Error:" + e.Message + " " + transform.parent.name ) ;
				}
			}
			return m_CanvasGroup ;
		}

		/// <summary>
		/// CanvasGroup の有無
		/// </summary>
		public bool IsCanvasGroup
		{
			get
			{
				if( GetCanvasGroup() == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddCanvasGroup() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveCanvasGroup() ;
					}
					else
					{
						m_RemoveCanvasGroup = true ;
					}

#else

					RemoveCanvasGroup() ;

#endif
				}
			}
		}

		/// <summary>
		/// CanvasGroup の追加
		/// </summary>
		public void AddCanvasGroup()
		{
			if( GetCanvasGroup() != null )
			{
				return ;
			}
		
			CanvasGroup canvasGroup ;
		
			canvasGroup = gameObject.AddComponent<CanvasGroup>() ;
			canvasGroup.alpha= 1.0f ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveCanvasGroup = false ;
#endif
		
		/// <summary>
		/// CanvasGroup の削除
		/// </summary>
		public void RemoveCanvasGroup()
		{
			CanvasGroup canvasGroup = GetCanvasGroup() ;
			if( canvasGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( canvasGroup ) ;
			}
			else
			{
				Destroy( canvasGroup ) ;
			}

			m_CanvasGroup = null ;
		}

		//----------

		// キャッシュ
		private GraphicRaycasterWrapper m_GraphicRaycaster = null ;

		/// <summary>
		/// GraphicRaycaster(ショートカット)
		/// </summary>
		virtual public GraphicRaycasterWrapper GetGraphicRaycaster()
		{
			if( m_GraphicRaycaster == null )
			{
				m_GraphicRaycaster = gameObject.GetComponent<GraphicRaycasterWrapper>() ;
			}
			return m_GraphicRaycaster ;
		}

		//----------

		// キャッシュ
		private Graphic m_Graphic = null ;

		/// <summary>
		/// Graphic(ショートカット)
		/// </summary>
		virtual public Graphic GetGraphic()
		{
			if( m_Graphic == null )
			{
				m_Graphic = gameObject.GetComponent<Graphic>() ;
			}
			return m_Graphic ;
		}

		//----------

		// キャッシュ	
		protected EventTrigger m_EventTrigger = null ;

		/// <summary>
		/// EventTrigger(ショートカット)
		/// </summary>
		virtual public EventTrigger _eventTrigger
		{
			get
			{
				if( m_EventTrigger == null )
				{
					m_EventTrigger = gameObject.GetComponent<EventTrigger>() ;
				}
				return m_EventTrigger ;
			}
		}

		/// <summary>
		/// EventTrigger の有無
		/// </summary>
		public bool isEventTrigger
		{
			get
			{
				if( _eventTrigger == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddEventTrigger() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveEventTrigger() ;
					}
					else
					{
						m_RemoveEventTrigger = true ;
					}

#else

					RemoveEventTrigger() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// EventTrigger の追加
		/// </summary>
		public void AddEventTrigger()
		{
			if( _eventTrigger != null )
			{
				return ;
			}
		
			m_EventTrigger = gameObject.AddComponent<EventTrigger>() ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveEventTrigger = false ;
#endif

		/// <summary>
		/// EventTrigger の削除
		/// </summary>
		public void RemoveEventTrigger()
		{
			EventTrigger tEventTrigger = _eventTrigger ;
			if( tEventTrigger == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tEventTrigger ) ;
			}
			else
			{
				Destroy( tEventTrigger ) ;
			}

			m_EventTrigger = null ;
		}

		//----------

		// キャッシュ
		protected UIInteraction m_Interaction = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		virtual public UIInteraction _interaction
		{
			get
			{
				if( m_Interaction == null )
				{
					m_Interaction = gameObject.GetComponent<UIInteraction>() ;
				}
				return m_Interaction ;
			}
		}
		
		/// <summary>
		/// Interaction の有無
		/// </summary>
		public bool isInteraction
		{
			get
			{
				if( _interaction == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddInteraction() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInteraction() ;
					}
					else
					{
						m_RemoveInteraction = true ;
					}

#else

					RemoveInteraction() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// Interaction の追加
		/// </summary>
		public void AddInteraction()
		{
			if( _interaction != null )
			{
				return ;
			}
			m_Interaction = gameObject.AddComponent<UIInteraction>() ;
			AddInteractionCallback() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInteraction = false ;
#endif

		/// <summary>
		/// Interaction の削除
		/// </summary>
		public void RemoveInteraction()
		{
			UIInteraction tInteraction = _interaction ;
			if( tInteraction == null )
			{
				return ;
			}
		
//			RemoveInteractionCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tInteraction ) ;
			}
			else
			{
				Destroy( tInteraction ) ;
			}

			m_Interaction = null ;

			m_Hover = false ;	// 消しておかないと Hover で悪さする
			m_Press = false ;
		}

		//----------

		// キャッシュ
		protected UIInteractionForScrollView m_InteractionForScrollView = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		virtual public UIInteractionForScrollView _interactionForScrollView
		{
			get
			{
				if( m_InteractionForScrollView == null )
				{
					m_InteractionForScrollView = gameObject.GetComponent<UIInteractionForScrollView>() ;
				}
				return m_InteractionForScrollView ;
			}
		}
		
		/// <summary>
		/// InteractionWithoutDrag の有無
		/// </summary>
		public bool isInteractionForScrollView
		{
			get
			{
				if( _interactionForScrollView == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddInteractionForScrollView() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInteractionForScrollView() ;
					}
					else
					{
						m_RemoveInteractionForScrollView = true ;
					}

#else

					RemoveInteractionForScrollView() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// InteractionWithoutDrag の追加
		/// </summary>
		public void AddInteractionForScrollView()
		{
			if( _interactionForScrollView != null )
			{
				return ;
			}
			m_InteractionForScrollView = gameObject.AddComponent<UIInteractionForScrollView>() ;
			AddInteractionForScrollViewCallback() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInteractionForScrollView = false ;
#endif

		/// <summary>
		/// Interaction の削除
		/// </summary>
		public void RemoveInteractionForScrollView()
		{
			UIInteractionForScrollView tInteractionForScrollView = _interactionForScrollView ;
			if( tInteractionForScrollView == null )
			{
				return ;
			}
		
			RemoveInteractionForScrollViewCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tInteractionForScrollView ) ;
			}
			else
			{
				Destroy( tInteractionForScrollView ) ;
			}

			m_InteractionForScrollView = null ;

			m_Hover = false ;	// 消しておかないと Hover で悪さする
			m_Press = false ;
		}

		//----------

		// キャッシュ
		protected UITransition m_Transition = null ;

		/// <summary>
		/// Transition(ショートカット)
		/// </summary>
		virtual public UITransition _transition
		{
			get
			{
				if( m_Transition == null )
				{
					m_Transition = gameObject.GetComponent<UITransition>() ;
				}
				return m_Transition ;
			}
		}
		
		/// <summary>
		/// Transition の有無
		/// </summary>
		public bool isTransition
		{
			get
			{
				if( _transition == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddTransition() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveTransition() ;
					}
					else
					{
						m_RemoveTransition = true ;
					}

#else

					RemoveTransition() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// Transition の追加
		/// </summary>
		public void AddTransition()
		{
			if( _transition != null )
			{
				return ;
			}
		
			m_Transition = gameObject.AddComponent<UITransition>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveTransition = false ;
#endif

		/// <summary>
		/// Transition の削除
		/// </summary>
		public void RemoveTransition()
		{
			UITransition tTransition = _transition ;
			if( tTransition == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tTransition ) ;
			}
			else
			{
				Destroy( tTransition ) ;
			}

			m_Transition = null ;
		}

		//----------

		// キャッシュ
		protected UITransitionForScrollView m_TransitionForScrollView = null ;

		/// <summary>
		/// TransitionForScrollView(ショートカット)
		/// </summary>
		virtual public UITransitionForScrollView _transitionForScrollView
		{
			get
			{
				if( m_TransitionForScrollView == null )
				{
					m_TransitionForScrollView = gameObject.GetComponent<UITransitionForScrollView>() ;
				}
				return m_TransitionForScrollView ;
			}
		}
		
		/// <summary>
		/// TransitionForScrollView の有無
		/// </summary>
		public bool isTransitionForScrollView
		{
			get
			{
				if( _transitionForScrollView == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddTransitionForScrollView() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveTransitionForScrollView() ;
					}
					else
					{
						m_RemoveTransitionForScrollView = true ;
					}

#else

					RemoveTransitionForScrollView() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// TransitionForScrollView の追加
		/// </summary>
		public void AddTransitionForScrollView()
		{
			if( _transitionForScrollView != null )
			{
				return ;
			}
		
			m_TransitionForScrollView = gameObject.AddComponent<UITransitionForScrollView>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveTransitionForScrollView = false ;
#endif

		/// <summary>
		/// TransitionForScrollView の削除
		/// </summary>
		public void RemoveTransitionForScrollView()
		{
			UITransitionForScrollView tTransitionForScrollView = _transitionForScrollView ;
			if( tTransitionForScrollView == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tTransitionForScrollView ) ;
			}
			else
			{
				Destroy( tTransitionForScrollView ) ;
			}

			m_TransitionForScrollView = null ;
		}

		//----------
		
		// キャッシュ
		private RawImage m_RawImage = null ;

		/// <summary>
		/// RawImage(ショートカット)
		/// </summary>
		virtual public RawImage _rawImage
		{
			get
			{
				if( m_RawImage == null )
				{
					m_RawImage = gameObject.GetComponent<RawImage>() ;
				}
				return m_RawImage ;
			}
		}
	
		//----------

		// キャッシュ
		private Image m_Image = null ;

		/// <summary>
		/// Image(ショートカット)
		/// </summary>
		virtual public Image _image
		{
			get
			{
				if( m_Image == null )
				{
					m_Image = gameObject.GetComponent<Image>() ;
				}
				return m_Image ;
			}
		}
		
		//----------

		// キャッシュ
		private Button m_Button = null ;

		/// <summary>
		/// Button(ショートカット)
		/// </summary>
		virtual public Button _button
		{
			get
			{
				if( m_Button == null )
				{
					m_Button = gameObject.GetComponent<Button>() ;
				}
				return m_Button ;
			}
		}
	
		//----------
		
		// キャッシュ
		private Text m_Text = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		virtual public Text _text
		{
			get
			{
				if( m_Text == null )
				{
					m_Text = gameObject.GetComponent<Text>() ;
				}
				return m_Text ;
			}
		}
	
		//----------
		
		// キャッシュ
		private RichText m_RichText = null ;

		/// <summary>
		/// RichText(ショートカット)
		/// </summary>
		virtual public RichText _richText
		{
			get
			{
				if( m_RichText == null )
				{
					m_RichText = gameObject.GetComponent<RichText>() ;
				}
				return m_RichText ;
			}
		}

		//----------
//#if TextMeshPro
		// キャッシュ
		private TextMeshProUGUI m_TextMesh = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		virtual public TextMeshProUGUI _textMesh
		{
			get
			{
				if( m_TextMesh == null )
				{
					m_TextMesh = gameObject.GetComponent<TextMeshProUGUI>() ;
				}
				return m_TextMesh ;
			}
		}
//#endif
		//----------

		// キャッシュ
		private ContentSizeFitter m_ContentSizeFitter = null ;

		/// <summary>
		/// ContentSizeFitter(ショートカット)
		/// </summary>
		virtual public ContentSizeFitter _contentSizeFitter
		{
			get
			{
				if( m_ContentSizeFitter == null )
				{
					m_ContentSizeFitter = gameObject.GetComponent<ContentSizeFitter>() ;
				}
				return m_ContentSizeFitter ;
			}
		}
		
		/// <summary>
		/// ContentSizeFitter の有無
		/// </summary>
		public bool isContentSizeFitter
		{
			get
			{
				if( _contentSizeFitter == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddContentSizeFitter() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveContentSizeFitter() ;
					}
					else
					{
						m_RemoveContentSizeFitter = true ;
					}

#else

					RemoveContentSizeFitter() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// ContentSizeFitter の追加
		/// </summary>
		public void AddContentSizeFitter()
		{
			if( _contentSizeFitter != null )
			{
				return ;
			}
		
			ContentSizeFitter tContentSizeFitter ;
		
			tContentSizeFitter = gameObject.AddComponent<ContentSizeFitter>() ;
			tContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize ;
			tContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveContentSizeFitter = false ;
#endif

		/// <summary>
		/// ContentSizeFitter の削除
		/// </summary>
		public void RemoveContentSizeFitter()
		{
			ContentSizeFitter tContentSizeFitter = _contentSizeFitter ;
			if( tContentSizeFitter == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tContentSizeFitter ) ;
			}
			else
			{
				Destroy( tContentSizeFitter ) ;
			}

			m_ContentSizeFitter = null ;
		}
		
		//----------
		
		// キャッシュ
		private Slider m_Slider = null ;

		/// <summary>
		/// Slider(ショートカット)
		/// </summary>
		virtual public Slider _slider
		{
			get
			{
				if( m_Slider == null )
				{
					m_Slider = gameObject.GetComponent<Slider>() ;
				}
				return m_Slider ;
			}
		}
	
		//----------
		
		// キャッシュ
		private ScrollbarWrapper m_Scrollbar = null ;

		/// <summary>
		/// Scrollbar(ショートカット)
		/// </summary>
		virtual public Scrollbar _scrollbar
		{
			get
			{
				if( m_Scrollbar == null )
				{
					m_Scrollbar = gameObject.GetComponent<ScrollbarWrapper>() ;
				}
				return m_Scrollbar ;
			}
		}
	
		//----------

		// キャッシュ
		private ToggleGroup m_ToggleGroup = null ;
		private UIToggleGroup m_UIToggleGroup = null ;

		/// <summary>
		/// ToggleGroup(ショートカット)
		/// </summary>
		virtual public ToggleGroup _toggleGroup
		{
			get
			{
				if( m_ToggleGroup == null )
				{
					m_ToggleGroup = gameObject.GetComponent<ToggleGroup>() ;
				}
				return m_ToggleGroup ;
			}
		}
		
		/// <summary>
		/// ToggleGroup の有無
		/// </summary>
		public bool isToggleGroup
		{
			get
			{
				if( _toggleGroup == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddToggleGroup() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveToggleGroup() ;
					}
					else
					{
						m_RemoveToggleGroup = true ;
					}

#else

					RemoveToggleGroup() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// ToggleGroup の追加
		/// </summary>
		public void AddToggleGroup()
		{
			if( _toggleGroup != null )
			{
				return ;
			}

			ToggleGroup toggleGroup ;
		
			toggleGroup = gameObject.AddComponent<ToggleGroup>() ;
			toggleGroup.allowSwitchOff = false ;

			m_UIToggleGroup = new UIToggleGroup( toggleGroup ) ;
		}

#if UNITY_EDITOR
		private bool m_RemoveToggleGroup = false ;
#endif

		/// <summary>
		/// ToggleGroup の削除
		/// </summary>
		public void RemoveToggleGroup()
		{
			ToggleGroup toggleGroup = _toggleGroup ;
			if( toggleGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( toggleGroup ) ;
			}
			else
			{
				Destroy( toggleGroup ) ;
			}

			m_ToggleGroup = null ;
			m_UIToggleGroup = null ;
		}

		public UIToggleGroup GetToggleGroup()
		{
			if( m_UIToggleGroup == null )
			{
				if( _toggleGroup == null )
				{
					return null ;
				}

				m_UIToggleGroup = new UIToggleGroup( _toggleGroup ) ;
			}

			return m_UIToggleGroup ;
		}

		//----------

		// キャッシュ
		private Toggle m_Toggle = null ;

		/// <summary>
		/// Toggle(ショートカット)
		/// </summary>
		virtual public Toggle _toggle
		{
			get
			{
				if( m_Toggle == null )
				{
					m_Toggle = gameObject.GetComponent<Toggle>() ;
				}
				return m_Toggle ;
			}
		}
		
		/// <summary>
		/// Toggle の有無
		/// </summary>
		public bool isToggle
		{
			get
			{
				if( _toggle == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddToggle() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveToggle() ;
					}
					else
					{
						m_RemoveToggle = true ;
					}

#else

					RemoveToggle() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// Toggle の追加
		/// </summary>
		public void AddToggle()
		{
			if( _toggle != null )
			{
				return ;
			}

			Toggle toggle ;
		
			toggle = gameObject.AddComponent<Toggle>() ;

			// 初期設定を行う
			ToggleGroup toggleGroup = GetComponentInParent<ToggleGroup>() ;
			if( toggleGroup != null )
			{
				toggle.group = toggleGroup ;
			}
		}

#if UNITY_EDITOR
		private bool m_RemoveToggle = false ;
#endif

		/// <summary>
		/// Toggle の削除
		/// </summary>
		public void RemoveToggle()
		{
			Toggle toggle = _toggle ;
			if( toggle == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( toggle ) ;
			}
			else
			{
				Destroy( toggle ) ;
			}

			m_Toggle = null ;
		}

		//----------

		// キャッシュ
		private ScrollRectWrapper m_ScrollRect = null ;

		/// <summary>
		/// ScrollRect(ショートカット)
		/// </summary>
		virtual public ScrollRectWrapper _scrollRect
		{
			get
			{
				if( m_ScrollRect == null )
				{
					m_ScrollRect = gameObject.GetComponent<ScrollRectWrapper>() ;
				}
				return m_ScrollRect ;
			}
		}

		//----------

		// キャッシュ
		private Dropdown m_Dropdown ;

		/// <summary>
		/// Dropdown(ショートカット)
		/// </summary>
		virtual public Dropdown _dropdown
		{
			get
			{
				if( m_Dropdown == null )
				{
					m_Dropdown = gameObject.GetComponent<Dropdown>() ;
				}
				return m_Dropdown ;
			}
		}
		
		//----------

		// キャッシュ
		private Mask m_Mask = null ;

		/// <summary>
		/// Mask(ショートカット)
		/// </summary>
		virtual public Mask _mask
		{
			get
			{
				if( m_Mask == null )
				{
					m_Mask = gameObject.GetComponent<Mask>() ;
				}
				return m_Mask ;
			}
		}
		
		/// <summary>
		/// Mask の有無
		/// </summary>
		public bool isMask
		{
			get
			{
				if( _mask == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddMask() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveMask() ;
					}
					else
					{
						m_RemoveMask = true ;
					}

#else

					RemoveMask() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// Mask の追加
		/// </summary>
		public void AddMask()
		{
			if( _mask != null )
			{
				return ;
			}
		
			Mask tMask ;
		
			tMask = gameObject.AddComponent<Mask>() ;
			tMask.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveMask = false ;
#endif

		/// <summary>
		/// Mask の削除
		/// </summary>
		public void RemoveMask()
		{
			Mask tMask = _mask ;
			if( tMask == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tMask ) ;
			}
			else
			{
				Destroy( tMask ) ;
			}

			tMask = null ;
		}

		//----------

		// キャッシュ
		private RectMask2D m_RectMask2D = null ;

		/// <summary>
		/// RectMask2D(ショートカット)
		/// </summary>
		virtual public RectMask2D _rectMask2D
		{
			get
			{
				if( m_RectMask2D == null )
				{
					m_RectMask2D = gameObject.GetComponent<RectMask2D>() ;
				}
				return m_RectMask2D ;
			}
		}
		
		/// <summary>
		/// RectMask2D の有無
		/// </summary>
		public bool isRectMask2D
		{
			get
			{
				if( _rectMask2D == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddRectMask2D() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveRectMask2D() ;
					}
					else
					{
						m_RemoveRectMask2D = true ;
					}

#else

					RemoveRectMask2D() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// RectMask2D の追加
		/// </summary>
		public void AddRectMask2D()
		{
			if( _rectMask2D != null )
			{
				return ;
			}

			gameObject.AddComponent<RectMask2D>() ;

//			RectMask2D tRectMask2D ;
		
//			tRectMask2D = gameObject.AddComponent<RectMask2D>() ;
//			tRectMask2D.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveRectMask2D = false ;
#endif

		/// <summary>
		/// RectMask2D の削除
		/// </summary>
		public void RemoveRectMask2D()
		{
			RectMask2D tRectMask2D = _rectMask2D ;
			if( tRectMask2D == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tRectMask2D ) ;
			}
			else
			{
				Destroy( tRectMask2D ) ;
			}

			tRectMask2D = null ;
		}

		//----------

		// キャッシュ
		private UIAlphaMaskWindow m_AlphaMaskWindow = null ;

		/// <summary>
		/// AlphaMaskWindow(ショートカット)
		/// </summary>
		virtual public UIAlphaMaskWindow _alphaMaskWindow
		{
			get
			{
				if( m_AlphaMaskWindow == null )
				{
					m_AlphaMaskWindow = gameObject.GetComponent<UIAlphaMaskWindow>() ;
				}
				return m_AlphaMaskWindow ;
			}
		}
		
		/// <summary>
		/// AlphaMaskWindow の有無
		/// </summary>
		public bool isAlphaMaskWindow
		{
			get
			{
				if( _alphaMaskWindow == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddAlphaMaskWindow() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveAlphaMaskWindow() ;
					}
					else
					{
						m_RemoveAlphaMaskWindow = true ;
					}

#else

					RemoveAlphaMaskWindow() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// AlphaMaskWindow の追加
		/// </summary>
		public void AddAlphaMaskWindow()
		{
			if( _alphaMaskWindow != null )
			{
				return ;
			}
		
//			UIAlphaMaskWindow tAlphaMaskWindow ;
		
//			tAlphaMaskWindow = gameObject.AddComponent<UIAlphaMaskWindow>() ;
			gameObject.AddComponent<UIAlphaMaskWindow>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAlphaMaskWindow = false ;
#endif

		/// <summary>
		/// AlphaMaskWindow の削除
		/// </summary>
		public void RemoveAlphaMaskWindow()
		{
			UIAlphaMaskWindow tAlphaMaskWindow = _alphaMaskWindow ;
			if( tAlphaMaskWindow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tAlphaMaskWindow ) ;
			}
			else
			{
				Destroy( tAlphaMaskWindow ) ;
			}

			tAlphaMaskWindow = null ;
		}

		//----------

		// キャッシュ
		private UIAlphaMaskTarget m_AlphaMaskTarget = null ;

		/// <summary>
		/// AlphaMaskTarget(ショートカット)
		/// </summary>
		virtual public UIAlphaMaskTarget _alphaMaskTarget
		{
			get
			{
				if( m_AlphaMaskTarget == null )
				{
					m_AlphaMaskTarget = gameObject.GetComponent<UIAlphaMaskTarget>() ;
				}
				return m_AlphaMaskTarget ;
			}
		}
		
		/// <summary>
		/// AlphaMaskTarget の有無
		/// </summary>
		public bool isAlphaMaskTarget
		{
			get
			{
				if( _alphaMaskTarget == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddAlphaMaskTarget() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveAlphaMaskTarget() ;
					}
					else
					{
						m_RemoveAlphaMaskTarget = true ;
					}

#else

					RemoveAlphaMaskTarget() ;

#endif
				}
			}
		}
		
		/// <summary>
		/// AlphaMaskTarget の追加
		/// </summary>
		public void AddAlphaMaskTarget()
		{
			if( _alphaMaskTarget != null )
			{
				return ;
			}
		
//			UIAlphaMaskTarget tAlphaMaskTarget ;
		
//			tAlphaMaskTarget = gameObject.AddComponent<UIAlphaMaskTarget>() ;
			gameObject.AddComponent<UIAlphaMaskTarget>() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAlphaMaskTarget = false ;
#endif

		/// <summary>
		/// AlphaMaskTarget の削除
		/// </summary>
		public void RemoveAlphaMaskTarget()
		{
			UIAlphaMaskTarget tAlphaMaskTarget = _alphaMaskTarget ;
			if( tAlphaMaskTarget == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tAlphaMaskTarget ) ;
			}
			else
			{
				Destroy( tAlphaMaskTarget ) ;
			}

			tAlphaMaskTarget = null ;
		}

		//----------

		/// <summary>
		/// グラフィックコンポーネントがアタッチされているかどうか
		/// </summary>
		public bool isGraphic
		{
			get
			{
				if( GetComponent<Graphic>() == null )
				{
					return false ;
				}

				return true ;
			}
		}


		//----------
		
		// キャッシュ
		private HorizontalLayoutGroup m_HorizontalLayoutGroup = null ;

		/// <summary>
		/// HorizontalLayoutGroup(ショートカット)
		/// </summary>
		virtual public HorizontalLayoutGroup _horizontalLayoutGroup
		{
			get
			{
				if( m_HorizontalLayoutGroup == null )
				{
					m_HorizontalLayoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>() ;
				}
				return m_HorizontalLayoutGroup ;
			}
		}
	
		//----------
		
		// キャッシュ
		private VerticalLayoutGroup m_VerticalLayoutGroup = null ;

		/// <summary>
		/// VerticalLayoutGroup(ショートカット)
		/// </summary>
		virtual public VerticalLayoutGroup _verticalLayoutGroup
		{
			get
			{
				if( m_VerticalLayoutGroup == null )
				{
					m_VerticalLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>() ;
				}
				return m_VerticalLayoutGroup ;
			}
		}
	
		//----------
		
		// キャッシュ
		private LayoutElement m_LayoutElement = null ;

		/// <summary>
		/// LayoutElement(ショートカット)
		/// </summary>
		virtual public LayoutElement _layoutElement
		{
			get
			{
				if( m_LayoutElement == null )
				{
					m_LayoutElement = gameObject.GetComponent<LayoutElement>() ;
				}
				return m_LayoutElement ;
			}
		}
	
		//----------
		
		// キャッシュ
		private InputFieldPlus m_InputField = null ;

		/// <summary>
		/// InputField(ショートカット)
		/// </summary>
 		virtual public InputFieldPlus _inputField
		{
			get
			{
				if( m_InputField == null )
				{
					m_InputField = gameObject.GetComponent<InputFieldPlus>() ;
				}
				return m_InputField ;
			}
		}
	
		//----------
		
		// キャッシュ
		private Shadow m_Shadow = null ;

		/// <summary>
		/// Shadow(ショートカット)
		/// </summary>
		virtual public Shadow _shadow
		{
			get
			{
				if( m_Shadow == null )
				{
					m_Shadow = gameObject.GetComponent<Shadow>() ;
				}
				return m_Shadow ;
			}
		}
		
		/// <summary>
		/// Shadow の有無
		/// </summary>
		public bool isShadow
		{
			get
			{
				Shadow tShadow = _shadow ;
				if( tShadow == null )
				{
					return false ;
				}
				else
				{
					if( tShadow is Shadow == true && tShadow is Outline == false )
					{
						return true ;
					}
					else
					{
						return false ;
					}
				}
			}
			set
			{
				if( value == true )
				{
					AddShadow() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveShadow() ;
					}
					else
					{
						m_RemoveShadow = true ;
					}

#else

					RemoveShadow() ;

#endif
				}
			}
		}

		/// <summary>
		/// Shadow の追加
		/// </summary>
		public void AddShadow()
		{
			if( _shadow != null )
			{
				return ;
			}
		
			Shadow tShadow ;
		
			tShadow = gameObject.AddComponent<Shadow>() ;
			tShadow.effectColor = ARGB( 0xFF000000 ) ;
			tShadow.effectDistance = new Vector2(  1, -1 ) ;
			tShadow.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveShadow = false ;
#endif

		/// <summary>
		/// Shadow の削除
		/// </summary>
		public void RemoveShadow()
		{
			Shadow tShadow = _shadow ;
			if( tShadow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tShadow ) ;
			}
			else
			{
				Destroy( tShadow ) ;
			}

			m_Shadow = null ;
		}
		
		//----------
		
		// キャッシュ
		private Outline m_Outline = null ;

		/// <summary>
		/// Outline(ショートカット)
		/// </summary>
		virtual public Outline _outline
		{
			get
			{
				if( m_Outline == null )
				{
					m_Outline = gameObject.GetComponent<Outline>() ;
				}
				return m_Outline ;
			}
		}

		/// <summary>
		/// Outline の有無
		/// </summary>
		public bool isOutline
		{
			get
			{
				if( _outline == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddOutline() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveOutline() ;
					}
					else
					{
						m_RemoveOutline = true ;
					}
#else
					RemoveOutline() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Outline の追加
		/// </summary>
		public void AddOutline()
		{
			if( _outline != null )
			{
				return ;
			}
		
			Outline tOutline ;
		
			tOutline = gameObject.AddComponent<Outline>() ;
			tOutline.effectColor = ARGB( 0xFF000000 ) ;
			tOutline.effectDistance = new Vector2(  1, -1 ) ;
			tOutline.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveOutline = false ;
#endif

		/// <summary>
		/// Outline の削除
		/// </summary>
		public void RemoveOutline()
		{
			Outline tOutline = _outline ;
			if( tOutline == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tOutline ) ;
			}
			else
			{
				Destroy( tOutline ) ;
			}

			m_Outline = null ;
		}

		//----------
		
		// キャッシュ
		private UIGradient m_Gradient = null ;

		/// <summary>
		/// Gradient(ショートカット)
		/// </summary>
		virtual public UIGradient _gradient
		{
			get
			{
				if( m_Gradient == null )
				{
					m_Gradient = gameObject.GetComponent<UIGradient>() ;
				}
				return m_Gradient ;
			}
		}
		
		/// <summary>
		/// Gradient の有無
		/// </summary>
		public bool isGradient
		{
			get
			{
				if( _gradient == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddGradient() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveGradient() ;
					}
					else
					{
						m_RemoveGradient = true ;
					}
#else
					RemoveGradient() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Gradient の追加
		/// </summary>
		public void AddGradient()
		{
			if( _gradient != null )
			{
				return ;
			}
		
			UIGradient tGradient ;
		
			tGradient = gameObject.AddComponent<UIGradient>() ;
			tGradient.DirectionType = UIGradient.DirectionTypes.Vertical ;
			tGradient.Top    = ARGB( 0xFFFFFFFF ) ;
			tGradient.Bottom = ARGB( 0xFF3F3F3F ) ;
		}

#if UNITY_EDITOR
		private bool m_RemoveGradient = false ;
#endif

		/// <summary>
		/// Gradient の削除
		/// </summary>
		public void RemoveGradient()
		{
			UIGradient tGradient = _gradient ;
			if( tGradient == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tGradient ) ;
			}
			else
			{
				Destroy( tGradient ) ;
			}

			tGradient = null ;
		}

		//----------
		
		// キャッシュ
		private UIInversion m_Inversion = null ;

		/// <summary>
		/// Inversion(ショートカット)
		/// </summary>
		virtual public UIInversion _inversion
		{
			get
			{
				if( m_Inversion == null )
				{
					m_Inversion = gameObject.GetComponent<UIInversion>() ;
				}
				return m_Inversion ;
			}
		}
		
		/// <summary>
		/// Inversion の有無
		/// </summary>
		public bool isInversion
		{
			get
			{
				if( _inversion == null )
				{
					return false ;
				}
				else
				{
					return true ;
				}
			}
			set
			{
				if( value == true )
				{
					AddInversion() ;
				}
				else
				{
#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveInversion() ;
					}
					else
					{
						m_RemoveInversion = true ;
					}
#else
					RemoveInversion() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Inversion の追加
		/// </summary>
		public void AddInversion()
		{
			if( _inversion != null )
			{
				return ;
			}
		
			UIInversion tInversion ;
		
			tInversion = gameObject.AddComponent<UIInversion>() ;
			tInversion.direction = UIInversion.Direction.None ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInversion = false ;
#endif

		/// <summary>
		/// Inversion の削除
		/// </summary>
		public void RemoveInversion()
		{
			UIInversion tInversion = _inversion ;
			if( tInversion == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tInversion ) ;
			}
			else
			{
				Destroy( tInversion ) ;
			}

			tInversion = null ;
		}

		//----------
		
		// キャッシュ
		private ImageNumber m_ImageNumber = null ;

		/// <summary>
		/// ImageNumber(ショートカット)
		/// </summary>
		virtual public ImageNumber _imageNumber
		{
			get
			{
				if( m_ImageNumber == null )
				{
					m_ImageNumber = gameObject.GetComponent<ImageNumber>() ;
				}
				return m_ImageNumber ;
			}
		}

		//----------
		
		// キャッシュ
		private GridMap m_GridMap = null ;

		/// <summary>
		/// GridMap(ショートカット)
		/// </summary>
		virtual public GridMap _gridMap
		{
			get
			{
				if( m_GridMap == null )
				{
					m_GridMap = gameObject.GetComponent<GridMap>() ;
				}
				return m_GridMap ;
			}
		}
		
		//----------
		
		// キャッシュ
		private ComplexRectangle m_ComplexRectangle = null ;

		/// <summary>
		/// ComplexRectangle(ショートカット)
		/// </summary>
		virtual public ComplexRectangle _complexRectangle
		{
			get
			{
				if( m_ComplexRectangle == null )
				{
					m_ComplexRectangle = gameObject.GetComponent<ComplexRectangle>() ;
				}
				return m_ComplexRectangle ;
			}
		}

		//----------
		
		// キャッシュ
		private Line m_Line = null ;

		/// <summary>
		/// Line(ショートカット)
		/// </summary>
		virtual public Line _line
		{
			get
			{
				if( m_Line == null )
				{
					m_Line = gameObject.GetComponent<Line>() ;
				}
				return m_Line ;
			}
		}

		//----------
		
		// キャッシュ
		private Circle m_Circle = null ;

		/// <summary>
		/// Circle(ショートカット)
		/// </summary>
		virtual public Circle _circle
		{
			get
			{
				if( m_Circle == null )
				{
					m_Circle = gameObject.GetComponent<Circle>() ;
				}
				return m_Circle ;
			}
		}

		//----------
		
		// キャッシュ
		private Arc m_Arc = null ;

		/// <summary>
		/// Arc(ショートカット)
		/// </summary>
		virtual public Arc _arc
		{
			get
			{
				if( m_Arc == null )
				{
					m_Arc = gameObject.GetComponent<Arc>() ;
				}
				return m_Arc ;
			}
		}


		//----------------------------------------------------------------
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="tIdentity">T identity.</param>
		public UIView FindView( string identity )
		{
			if( Identity == identity )
			{
				// 自身がそうだった
				return this ;
			}
		
			return FindView_Private( gameObject, identity ) ;
		}
	
		private UIView FindView_Private( GameObject go, string identity )
		{
		
			// 直の子供を確認する
			UIView tView ;
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				tView = go.transform.GetChild( i ).gameObject.GetComponent<UIView>() ;
				if( tView != null )
				{
					if( tView.Identity == identity )
					{
						// 発見
						return tView ;
					}
				}
			}
		
			// 直の子供を再帰的に検査する
			for( i  = 0 ; i <  c ; i ++ )
			{
				tView = FindView_Private( go.transform.GetChild( i ).gameObject, identity ) ;
				if( tView != null )
				{
					// 発見
					return tView ;
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="tIdentity">T identity.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindView<T>( string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			if( Identity == identity )
			{
				// 自身がそうだった
				component = gameObject.GetComponent<T>() ;
				return component ;
			}
		
			component = FindView_Private<T>( gameObject, identity ) ;
			return component ;
		}
	
		private T FindView_Private<T>( GameObject go, string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			// 直の子供を確認する
			UIView tView ;
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				tView = go.transform.GetChild( i ).gameObject.GetComponent<UIView>() ;
				if( tView != null )
				{
					if( tView.Identity == identity )
					{
						// 発見
						component = tView.gameObject.GetComponent<T>() ;
						return component ;
					}
				}
			}
		
			// 直の子供を再帰的に検査する
			for( i  = 0 ; i <  c ; i ++ )
			{
				component = FindView_Private<T>( go.transform.GetChild( i ).gameObject, identity ) ;
				if( component != null )
				{
					// 発見
					return component ;
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の GameObject を取得する
		/// </summary>
		/// <returns>The game object.</returns>
		/// <param name="tName">T name.</param>
		public GameObject FindNode( string tName, bool tContain = false )
		{
			if( name == tName )
			{
				// 自身がそうだった
				return gameObject ;
			}
		
			return FindNode_Private( gameObject, tName, tContain ) ;
		}
	
		private GameObject FindNode_Private( GameObject tGameObject, string tName, bool tContain )
		{
			tName = tName.ToLower() ;

			Transform tChild ;
			string tChildName ;
			bool tResult ;

			GameObject tTargetGameObject ;

			// 直の子供を再帰的に検査する
			int i, c = tGameObject.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				tChild =  tGameObject.transform.GetChild( i ) ;
				tChildName = tChild.name.ToLower() ;
				tResult = false ;
				if( tContain == false && tChildName == tName )
				{
					tResult = true ;
				}
				else
				if( tContain == true && tChildName.Contains( tName ) == true )
				{
					tResult = true ;
				}

				if( tResult == true )
				{
					// 発見
					tTargetGameObject = tChild.gameObject ;
					if( tTargetGameObject != null )
					{
						// 発見
						return tTargetGameObject ;
					}
				}
			
				if( tChild.childCount >  0 )
				{
					tTargetGameObject = FindNode_Private( tChild.gameObject, tName, tContain ) ;
					if( tTargetGameObject != null )
					{
						// 発見
						return tTargetGameObject ;
					}
				}
			}

			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の GameObject 内の Component を取得する
		/// </summary>
		/// <returns>The game object.</returns>
		/// <param name="tName">T name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindNode<T>( string tName, bool tContain = false ) where T : UnityEngine.Component
		{
			T tComponent ;
		
			if( name == tName )
			{
				// 自身がそうだった
				tComponent = gameObject.GetComponent<T>() ;
				if( tComponent != null )
				{
					return tComponent ;
				}
			}

			return FindNode_Private<T>( gameObject, tName, tContain ) ;
		}
	
		private T FindNode_Private<T>( GameObject tGameObject, string tName, bool tContain )
		{
			tName = tName.ToLower() ;

			Transform tChild ;
			string tChildName ;
			bool tResult ;

			T tComponent ;
		
			// 直の子供を再帰的に検査する
			int i, c = tGameObject.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				tChild = tGameObject.transform.GetChild( i ) ;
				tChildName = tChild.name.ToLower() ;
				tResult = false ;

				if( tContain == false && tChildName == tName )
				{
					tResult = true ;
				}
				else
				if( tContain == true && tChildName.Contains( tName ) == true )
				{
					tResult = true ;
				}
				
				if( tResult == true )
				{
					// 発見
					tComponent = tChild.GetComponent<T>() ;
					if( tComponent != null )
					{
						return tComponent ;
					}
				}

				if( tChild.childCount >  0 )
				{
					tComponent = FindNode_Private<T>( tChild.gameObject, tName, tContain ) ;
					if( tComponent != null )
					{
						// 発見
						return tComponent ;
					}
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return  default( T ) ;
		}
	
	
		//--------------------------------------------------------------------

		// ライブラリで持つ基本的なインタラクションイベントを登録する

		// 通常用のコールバックを登録する
		private bool AddInteractionCallback()
		{
			UIInteraction tInteraction = _interaction ;
			if( tInteraction == null )
			{
				return false ;
			}

			RemoveInteractionCallback()	;	// 多重登録にならないように削除しておく
			
			tInteraction.onPointerEnter	+= OnPointerEnterBasic	;
			tInteraction.onPointerExit	+= OnPointerExitBasic	;
			tInteraction.onPointerDown	+= OnPointerDownBasic	;
			tInteraction.onPointerUp	+= OnPointerUpBasic		;
			tInteraction.onPointerClick	+= OnPointerClickBasic	;
			tInteraction.onDrag			+= OnDragBasic			;

			return true ;
		}

		// 通常用のコールバックを解除する
		private bool RemoveInteractionCallback()
		{
			UIInteraction tInteraction = _interaction ;
			if( tInteraction == null )
			{
				return false ;
			}

			tInteraction.onPointerEnter	-= OnPointerEnterBasic	;
			tInteraction.onPointerExit	-= OnPointerExitBasic	;
			tInteraction.onPointerDown	-= OnPointerDownBasic	;
			tInteraction.onPointerUp	-= OnPointerUpBasic		;
			tInteraction.onPointerClick	-= OnPointerClickBasic	;
			tInteraction.onDrag			-= OnDragBasic			;

			return true ;
		}

		// スクロールビュー用のコールバックを登録する
		private bool AddInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView tInteractionForScrollView = _interactionForScrollView ;
			if( tInteractionForScrollView == null )
			{
				return false ;
			}

			RemoveInteractionForScrollViewCallback()	;	// 多重登録にならないように削除しておく
			
			tInteractionForScrollView.onPointerEnter	+= OnPointerEnterBasic	;
			tInteractionForScrollView.onPointerExit		+= OnPointerExitBasic	;
			tInteractionForScrollView.onPointerDown		+= OnPointerDownBasic	;
			tInteractionForScrollView.onPointerUp		+= OnPointerUpBasic		;
			tInteractionForScrollView.onPointerClick	+= OnPointerClickBasic	;
//			tInteractionForScrollView.onDrag			+= OnDragBasic			;

			return true ;
		}


		// スクロールビュー用のコールバックを解除する
		private bool RemoveInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView tInteractionForScrollView = _interactionForScrollView ;
			if( tInteractionForScrollView == null )
			{
				return false ;
			}

			tInteractionForScrollView.onPointerEnter	-= OnPointerEnterBasic	;
			tInteractionForScrollView.onPointerExit		-= OnPointerExitBasic	;
			tInteractionForScrollView.onPointerDown		-= OnPointerDownBasic	;
			tInteractionForScrollView.onPointerUp		-= OnPointerUpBasic		;
			tInteractionForScrollView.onPointerClick	-= OnPointerClickBasic	;
//			tInteractionForScrollView.onDrag			-= OnDragBasic			;
			
			return true ;
		}

		//--------------------------------------------------------------------

		protected bool m_Hover = false ;

		/// <summary>
		/// ホバー状態
		/// </summary>
		public  bool  isHover
		{
			get
			{
				return m_Hover ;
			}
		}

		protected bool m_Press = false ;

		/// <summary>
		/// プレス状態
		/// </summary>
		public  bool  isPress
		{
			get
			{
				if( m_Press == true && m_PressInvalidTime >  0 )
				{
					if( m_PressCountTime == Time.frameCount )
					{
						// 一番最初だけは有効にする
						return true ;
					}
					else
					if( ( Time.realtimeSinceStartup - m_PressStartTime ) <  m_PressInvalidTime )
					{
						return false ;
					}
				}

				return m_Press ;
			}
		}

		private float m_PressInvalidTime = 0 ;
		private int   m_PressCountTime = 0 ;
		private float m_PressStartTime = 0 ;

		public float pressInvalidTime
		{
			get
			{
				return m_PressInvalidTime ;
			}
			set
			{
				m_PressInvalidTime = value ;
			}
		}

		protected bool m_Click = false ;

		public bool isClick
		{
			get
			{
				return m_Click ;
			}
		}

		protected int m_ClickCountTime = 0 ;

		protected bool			m_ClickState				= false ;
		protected int			m_ClickPointerId			= -1 ;
		protected bool			m_ClickInsideFlag			= false ;
		
		protected bool			m_SmartClickState			= false ;
		protected int			m_SmartClickPointerId		= -1 ;
		protected bool			m_SmartClickInsideFlag		= false ;
		protected Vector2		m_SmartClickBasePosition	= Vector2.zero ;
		protected int			m_SmartClickCount			= 0 ;
		protected float			m_SmartClickBaseTime		= 0 ;
		protected IEnumerator	m_SmartClickCheckCoroutine	= null ;

		protected float			m_SmartClickLimitTime		= 0.5f ;
		protected float			m_SmartClickAllowTime		= 0.25f ;	// シングルクリック終了後にこの時間以内に新しいクリックが開始されたらダブルクリック判定が始まる
		protected float			m_SmartClickLimitDistance	= 30.0f ;

		protected float			m_HoldStartTime				= 0 ;
		protected float			m_HoldLimitTime				= 0.75f ;	// ホールドと判定する時間
		protected bool			m_HoldEnabled				= false ;

		public enum PointerState
		{
			None = 0,
			Start = 1,
			Moving = 2,
			End = 3,
		}

		/// <summary>
		/// ドラッグ状態
		/// </summary>
		/// <param name="tData"></param>
		
		public PointerState dragState
		{
			get
			{
				return m_DragState ;
			}
		}

		protected PointerState	m_DragState				= PointerState.None ;
		protected int			m_DragPointerId			= -1 ;
		protected Vector2		m_DragBasePosition		= Vector2.zero ;


		protected bool			m_FlickState			= false ;
		protected int			m_FlickPointerId		= -1 ;
		protected float			m_FlickAllowTime		=  0.5f ;
		protected float			m_FlickThresholdRatio	= 60.0f / 960.0f ;
		protected Vector2		m_FlickBasePosition		= Vector2.zero ;
		protected float			m_FlickBaseTime			= 0 ;
		protected bool			m_FlickCheck			= false ;
		protected float			m_FlickLastTime			= 0 ;

		public class TouchState
		{
			public int			index ;
			public int			identity ;
			public Vector2		position ;
			public PointerState	state ;

			public TouchState( int tIndex, int tIdentity, Vector2 tPosition, PointerState tState )
			{
				index		= tIndex ;
				identity	= tIdentity ;
				position	= tPosition ;
				state		= tState ;
			}
		}

		protected TouchState[]		m_TouchState			= new TouchState[ 10 ] ;
		protected List<TouchState>	m_TouchStateExchange	= new List<TouchState>() ;

		protected bool		m_FromScrollView = false ;
		protected float		m_InteractionLimit = 8.0f ;
		protected Vector2	m_InteractionLimit_StartPoint ;

		//--------------------------------------------------------------------

		// Enter
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerEnterBasic( PointerEventData tPointer, bool tFromScrollView )
		{
//			if( name == "0" )
//			{
//				Debug.LogWarning( "------>OnPointerEnterBasic : " + name + " p " + m_Press + " d " + m_DragState ) ;
//			}

			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;

			m_FromScrollView = tFromScrollView ;

			if( m_Hover == false )
			{
				// 初めて入った
				m_Hover = true ;
				OnHoverInner( PointerState.Start, tPosition ) ;

				// クリック処理
				if( m_ClickState == true && m_ClickPointerId == tIdentity )
				{
					// 中に入ったので以後は有効クリック扱いとなる
					m_ClickInsideFlag = true ;
				}
			}
			else
			{
				// ２回目以降
				if( m_Press == false && m_DragState == PointerState.None && UIEventSystem.ProcessType == StandaloneInputModuleWrapper.ProcessType.Custom )
				{
					OnHoverInner( PointerState.Moving, tPosition ) ;
				}
			}
		}

		// Exit
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerExitBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;

			m_FromScrollView = tFromScrollView ;

			m_Hover = false ;
			OnHoverInner( PointerState.End, tPosition ) ;

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == tIdentity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_ClickInsideFlag = false ;
				}
//			}

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
//			{
				if( m_SmartClickState == true && m_SmartClickPointerId == tIdentity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_SmartClickInsideFlag = false ;
				}
//			}

			// ホールド判定終了
			CancelHold() ;
		}

		// Down
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerDownBasic( PointerEventData tPointer, bool tFromScrollView )
		{
//			if( name == "0" )
//			{
//				Debug.LogWarning( "-----OnPointerDownBasic : name = " + name ) ;
//			}

			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;

			m_FromScrollView = tFromScrollView ;
			if( m_FromScrollView == true )
			{
				m_InteractionLimit_StartPoint = tPointer.position ;
			}

//			if( name == "0" )
//			{
//				Debug.LogWarning( "m_Press を true" ) ;
//			}
			m_Press = true ;
			m_PressCountTime = Time.frameCount ;
			m_PressStartTime = Time.realtimeSinceStartup ;
			OnPressInner( m_Press ) ;

			//----------------------------------------------------------

			// ホールド確認
//			if( ( onHoldAction != null || onHoldDelegate != null ) && m_HoldStartTime == 0 )
			if( m_HoldStartTime == 0 )
			{
				m_HoldStartTime = Time.realtimeSinceStartup ;
			}

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				m_ClickState = true ;
				m_ClickPointerId = tIdentity ;
				m_ClickInsideFlag = true ;
//			}



			// スマートクリック処理
//			if( onSmartClickDelegate != null )
///			{
				if( m_SmartClickState == false )
				{
					// １回目のクリック
					m_SmartClickState = true ;
					m_SmartClickPointerId = tIdentity ;
					m_SmartClickInsideFlag = true ;

					m_SmartClickBasePosition = tPosition ;
					m_SmartClickCount = 1 ;
					m_SmartClickBaseTime = Time.realtimeSinceStartup ;

					m_SmartClickCheckCoroutine = null ;
				}
				else
				{
					// ２回目のクリック
					if( m_SmartClickCheckCoroutine != null )
					{
						// コルーチンを停止させる
						StopCoroutine( m_SmartClickCheckCoroutine ) ;
						m_SmartClickCheckCoroutine = null ;
					}

					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - tPosition ).magnitude <= m_SmartClickLimitDistance ) )
					{
						// ダブルクリック判定に入る
						m_SmartClickPointerId = tIdentity ;	// 識別子を新しくする(１回目と２回目では異なる可能性がある)

						m_SmartClickInsideFlag = true ;	// 重要:タッチパネルの実機の場合は Up すると Exit も発行されてしまうためもう一度有効扱いにする必要がある

						m_SmartClickCount = 2 ;	
						m_SmartClickBaseTime = Time.realtimeSinceStartup ;
					}
					else
					{
						m_SmartClickState = false ;
					}
				}
//			}


			//----------------------------------------------------------

			// スクロールビュー上では以下は無視する
			if( tFromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( onDragDelegate != null )
			{
//				if( m_DragState == PointerState.None )
//				{
					m_DragState = PointerState.Start ;

					m_DragPointerId = tIdentity ;
					m_DragBasePosition = tPosition ;
	
					OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
//				}
			}

			// フリック処理
			if( onFlickAction != null || onFlickDelegate != null )
			{
				if( m_FlickState == false )
				{
					m_FlickState = true ;
	
					m_FlickPointerId = tIdentity ;
					m_FlickBasePosition = tPosition ;

					m_FlickCheck = false ;
				}
			}

			// タッチ処理
			if( onTouchAction != null || onTouchDelegate != null )
			{
				// 既に登録されているか判定する
				int i = 0, l = m_TouchState.Length ;
				int e = -1 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_TouchState[ i ] != null )
					{
						if( m_TouchState[ i ].identity == tIdentity )
						{
							break ;	// 既に登録済みのポイント(ありえない)
						}
					}
					else
					{
						if( e <  0 )
						{
							e = i ;
						}
					}
				}

				if( i >= l )
				{
					if( e >= 0 && e <  l )
					{
						// 新規登録
						m_TouchState[ e ] = new TouchState( e, tIdentity, tPosition, PointerState.Start ) ;
					}
				}
				else
				{
					// 既存更新(ありえない)
					m_TouchState[ i ].position = tPosition ;
					m_TouchState[ i ].state = PointerState.Start ;
				}

				// コールバック発行
				OnTouchInner( m_TouchState ) ;
			}
		}

		// Up
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnPointerUpBasic( PointerEventData tPointer, bool tFromScrollView )
		{
//			if( name == "0" )
//			{
//				Debug.LogWarning( "-----OnPointerUpBasic : name = " + name ) ;
//			}

			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;

			m_FromScrollView = tFromScrollView ;

			m_Press = false ;
			m_PressCountTime = 0 ;
			m_PressStartTime = 0 ;
			OnPressInner( m_Press ) ;

			//----------------------------------------------------------

			// スクロールビュー上では以下は無視する
			if( tFromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// スクロールビューでなければ処理する
			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == tIdentity )
				{
					if( m_ClickInsideFlag == true )
					{
						if( m_HoldEnabled == false )
						{
							// クリックとみなす
							OnClickInner() ;
						}
					}
				}

				m_ClickState = false ;
//			}

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
//			{
				if( m_SmartClickCheckCoroutine != null )
				{
					// コルーチンを停止させる(基本的にここに来る事はありえない＝保険)
					StopCoroutine( m_SmartClickCheckCoroutine ) ;
					m_SmartClickCheckCoroutine = null ;
				}

				if( m_SmartClickState == true && m_SmartClickPointerId == tIdentity )
				{
					if( m_SmartClickInsideFlag == true )
					{
						if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - tPosition ).magnitude <= m_SmartClickLimitDistance ) )
						{
							float tTime = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

							if( m_SmartClickCount == 1 )
							{
								// シングルクリック判定
								if( m_SmartClickLimitTime <= 0 )
								{
									// 常にシングルクリック
									if( m_HoldEnabled == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, tPosition ) ;
									}

									m_SmartClickState = false ;
								}
								else
								if( tTime <  m_SmartClickLimitTime )
								{
									// 一定時間以内に離していないと無効
									if( m_SmartClickAllowTime <= 0 )
									{
										// シングルクリック決定
										if( m_HoldEnabled == false )
										{
											OnSmartClickInner( 1, m_SmartClickBasePosition, tPosition ) ;
										}

										m_SmartClickState = false ;
									}
									else
									{
										// シングルクリックかダブルクリックかを判定するルーチンを起動する
										if( m_HoldEnabled == false )
										{
											SingleClickCheck( tPosition, tPointer.position ) ;
										}
									}
								}
								else
								{
									m_SmartClickState = false ;
								}
							}
							else
							if( m_SmartClickCount == 2 )
							{
								// ダブルクリック判定
	
								if( tTime <  m_SmartClickLimitTime )
								{
									// 一定時間以内に離していないと無効
	
									// ダブルクリック決定
									
									if( m_HoldEnabled == false )
									{
										OnSmartClickInner( 2, m_SmartClickBasePosition, tPosition ) ;
									}
	
									m_SmartClickState = false ;
								}
								else
								{
									m_SmartClickState = false ;
								}
							}
						}
						else
						{
							m_SmartClickState = false ;
						}
					}
					else
					{
						// 外に出たので無効クリック扱いとなる
						m_SmartClickState = false ;
					}
				}
//			}

			//----------------------------------------------------------

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( onDragDelegate != null )
			{
				if( ( m_DragState == PointerState.Start || m_DragState == PointerState.Moving ) && m_DragPointerId == tIdentity )
				{
					m_DragState = PointerState.None ;
	
					OnDragInner( PointerState.End, m_DragBasePosition, tPosition ) ;
				}
			}

			// フリック処理
			if( onFlickAction != null || onFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == tIdentity )
				{
					m_FlickState = false ;
	
					if( m_FlickCheck == true )
					{
						// １つ前のドラッグ位置と時間で最後に静止していたか判定する
						float tLastTime = Time.realtimeSinceStartup - m_FlickLastTime ;

						if( tLastTime <  0.1f )
						{
							// 基準位置からの移動量と時間で判定する
							Vector2 tValue = tPosition - m_FlickBasePosition ;
							float tTime = Time.realtimeSinceStartup - m_FlickBaseTime ;
	
							if( tTime <  m_FlickAllowTime && tValue.magnitude >  ( GetCanvasLength() * m_FlickThresholdRatio ) )
							{
								// フリック有効
								OnFlickInner( tValue, m_FlickBasePosition ) ;
							}
						}
					}
				}
			}

			// タッチ処理
			if( onTouchAction != null || onTouchDelegate != null )
			{
				// 既に登録されているか判定する
				int i = 0, l = m_TouchState.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_TouchState[ i ] != null )
					{
						if( m_TouchState[ i ].identity == tIdentity )
						{
							break ;
						}
					}
				}

				if( i <  l )
				{
					// 既存破棄
					m_TouchState[ i ].position = tPosition ;
					m_TouchState[ i ].state = PointerState.End ;

					// コールバック発行
					OnTouchInner( m_TouchState ) ;

					m_TouchState[ i ] = null ;
				}
			}

			// ホールド判定終了
			CancelHold() ;
		}

		// スクロールビュー用のクリック判定
		virtual protected void OnPointerClickBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			if( tFromScrollView == false )
			{
				// スクロールビュー上で無い場合は以下は無視する
				return ;
			}

			// ちなみに
			// ScrollView 上のボタンが押しにくい＝少し動いただけで離された状態になりクリックができないのは、
			// EventSystem の DragThreshold の値で改善できる(デフォルトは 10)
			// この値を大きくすればその範囲内の移動なら離されたと判定はされない
			// この値を超えてしまうと PointerUp のコールバックが発生する(ただしスクロール方向と違う方向でも発生する)
			// PointerUp のコールバックが発生すると PointerClick のコールバックも発生しなくなる

			//----------------------------------------------------------

			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;

			m_FromScrollView = tFromScrollView ;

			//----------------------------------------------------------
			// Click

			if( m_ClickState == true && m_ClickPointerId == tIdentity )
			{
				if( m_ClickInsideFlag == true )
				{
					if( m_HoldEnabled == false )
					{
						// クリックとみなす

						OnClickInner() ;
					}
				}
			}

			m_ClickState = false ;

			//----------------------------------------------------------
			// SmartClick

			if( m_SmartClickCheckCoroutine != null )
			{
				// コルーチンを停止させる(基本的にここに来る事はありえない＝保険)
				StopCoroutine( m_SmartClickCheckCoroutine ) ;
				m_SmartClickCheckCoroutine = null ;
			}

			if( m_SmartClickState == true && m_SmartClickPointerId == tIdentity )
			{
				if( m_SmartClickInsideFlag == true )
				{
					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - tPosition ).magnitude <= m_SmartClickLimitDistance ) )
					{
						float tTime = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

						if( m_SmartClickCount == 1 )
						{
							// シングルクリック判定
							if( m_SmartClickLimitTime <= 0 )
							{
								// 常にシングルクリック
									
								if( m_HoldEnabled == false )
								{
									OnSmartClickInner( 1, m_SmartClickBasePosition, tPosition ) ;
								}

								m_SmartClickState = false ;
							}
							else
							if( tTime <  m_SmartClickLimitTime )
							{
								// 一定時間以内に離していないと無効
								if( m_SmartClickAllowTime <= 0 )
								{
									// シングルクリック決定

									if( m_HoldEnabled == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, tPosition ) ;
									}

									m_SmartClickState = false ;
								}
								else
								{
									// シングルクリックかダブルクリックかを判定するルーチンを起動する
									if( m_HoldEnabled == false )
									{
										SingleClickCheck( tPosition, tPointer.position ) ;
									}
								}
							}
							else
							{
								m_SmartClickState = false ;
							}
						}
						else
						if( m_SmartClickCount == 2 )
						{
							// ダブルクリック判定
	
							if( tTime <  m_SmartClickLimitTime )
							{
								// 一定時間以内に離していないと無効
	
								// ダブルクリック決定
									
								if( m_HoldEnabled == false )
								{
									OnSmartClickInner( 2, m_SmartClickBasePosition, tPosition ) ;
								}
	
								m_SmartClickState = false ;
							}
							else
							{
								m_SmartClickState = false ;
							}
						}
					}
					else
					{
						m_SmartClickState = false ;
					}
				}
				else
				{
					// 外に出たので無効クリック扱いとなる
					m_SmartClickState = false ;
				}
			}

			//----------------------------------------------------------

			// ホールド判定終了
			CancelHold() ;
		}


		// Drag
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		virtual protected void OnDragBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			int tIdentity = tPointer.pointerId ;
			Vector2 tPosition = GetLocalPosition( tPointer ) ;
			
			m_FromScrollView = tFromScrollView ;
			if( ( m_FromScrollView == true && Vector2.Distance( m_InteractionLimit_StartPoint, tPointer.position ) >= m_InteractionLimit ) )
			{
				m_ClickState = false ;			// クリックキャンセル	
				m_SmartClickState = false ;	// スマートクリックキャンセル

				// ホールドをキャンセルする
				CancelHold() ;
			}

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( onDragDelegate != null )
			{
				if( ( m_DragState == PointerState.Start || m_DragState == PointerState.Moving ) && m_DragPointerId == tIdentity )
				{
					m_DragState = PointerState.Moving ;
	
					OnDragInner( PointerState.Moving, m_DragBasePosition, tPosition ) ;
				}
			}

			// フリック処理
			if( onFlickAction != null || onFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == tIdentity )
				{
					if( m_FlickCheck == false )
					{
						Vector2 tValue = tPosition - m_FlickBasePosition ;
	
						if( tValue.magnitude >   ( GetCanvasLength() * m_FlickThresholdRatio ) )
						{
							// フリック計測開始
							m_FlickCheck = true ;
							m_FlickBaseTime = Time.realtimeSinceStartup ;

							m_FlickLastTime = m_FlickBaseTime ;
						}
						else
						{
							// 基準の位置を更新
							m_FlickBasePosition = tPosition ;
						}
					}
					else
					{
						// フリック判定チェック中のドラッグは最後の状態を保存しておく

						// １フレーム前の時間を保存しておく
						m_FlickLastTime = Time.realtimeSinceStartup ;
					}
				}
			}

			// タッチ処理
			if( onTouchAction != null || onTouchDelegate != null )
			{
				// 既に登録されているか判定する
				int i = 0, l = m_TouchState.Length ;
				int e = -1 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_TouchState[ i ] != null )
					{
						if( m_TouchState[ i ].identity == tIdentity )
						{
							break ;
						}
					}
					else
					{
						if( e <  0 )
						{
							e  = i ;
						}
					}
				}

				if( i >= l )
				{
					if( e >= 0 && e <  l )
					{
						// 新規登録(ありえない)
						m_TouchState[ e ] = new TouchState( e, tIdentity, tPosition, PointerState.Moving ) ;
					}
				}
				else
				{
					// 既存更新
					m_TouchState[ i ].position = tPosition ;
					m_TouchState[ i ].state = PointerState.Moving ;
				}

				// コールバック発行
				OnTouchInner( m_TouchState ) ;
			}
		}

		// ホールドをキャンセルする
		private void CancelHold()
		{
			if( onHoldAction != null || onHoldDelegate != null )
			{
				if( m_HoldEnabled == false && m_HoldStartTime >  0 )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					// キャンセルを通知する
					onHoldAction?.Invoke( identity, this, 0 ) ;
					onHoldDelegate?.Invoke( identity, this, 0 ) ;
				}
			}

			m_HoldEnabled = false ;
			m_HoldStartTime = 0 ;
		}

		//-----------------------------------

		private bool	m_SingleClickCheck = false ;
		private Vector2 m_SingleClickCheck_Position ;
		private Vector2 m_SingleClickCheck_GlobalPosition ;
		private float	m_SingleClickCheck_BaseTime ;
		private float   m_SingleClickCheck_TickTime ;
		
		// シングルクリックかダブルクリックかの判定用のコルーチン(ScrollView内でコルーチンを使用するのは危険なので＝アイテムの作り直し問題・Updateで処理するようにする)
		private void SingleClickCheck( Vector2 tPosition, Vector2 tGlobalPosition )
		{
			m_SingleClickCheck					= true ;

			m_SingleClickCheck_Position			= tPosition ;
			m_SingleClickCheck_GlobalPosition	= tGlobalPosition ;
			
			m_SingleClickCheck_BaseTime			= Time.realtimeSinceStartup ;
			m_SingleClickCheck_TickTime			= 0 ;
		}

		private void SingleClickCheckProcess()
		{
			if( m_SingleClickCheck == false )
			{
				return ;
			}

			//----------------------------------------------------------

			m_SingleClickCheck_TickTime = m_SingleClickCheck_TickTime + (  Time.realtimeSinceStartup - m_SingleClickCheck_BaseTime ) ;
			if( m_SingleClickCheck_TickTime <  m_SmartClickAllowTime )
			{
				return ;
			}

			//----------------------------------------------------------

			if( m_HoldEnabled == false )
			{
				if( m_FromScrollView == false || ( m_FromScrollView == true && Vector2.Distance( m_InteractionLimit_StartPoint, m_SingleClickCheck_GlobalPosition ) <  m_InteractionLimit ) )
				{
					// シングルクリックとみなす
					OnSmartClickInner( 1, m_SmartClickBasePosition, m_SingleClickCheck_Position ) ;
				}
			}

			m_SmartClickState = false ;

			m_SingleClickCheck = false ;
		}

		//--------------------------------------------------------------------

		// Hover

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクション(マウス用)
		/// </summary>
		public Action<string, UIView, PointerState, Vector2> onHoverAction ;
	
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートの定義(マウス用)
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tState">状態(true=入った・false=離れた)</param>
		public delegate void OnHoverDelegate( string tIdentity, UIView tView, PointerState tState, Vector2 tMovePosition ) ;
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲート(マウス用)
		/// </summary>
		public OnHoverDelegate onHoverDelegate ;

		// 内部リスナー
		private void OnHoverInner( PointerState tState, Vector2 tMovePosition )
		{
			if( onHoverAction != null || onHoverDelegate != null) 
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onHoverAction?.Invoke( identity, this, tState, tMovePosition ) ;
				onHoverDelegate?.Invoke( identity, this, tState, tMovePosition ) ;
			}
		}
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクションを設定する → OnHover( string tIdentity, UIView tView, bool tState )
		/// </summary>
		/// <param name="tOnHover">アクションメソッド</param>
		public void SetOnHover( Action<string, UIView, PointerState, Vector2> tOnHoverAction )
		{
			onHoverAction = tOnHoverAction ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを追加する → OnHover( string tIdentity, UIView tView, bool tState )
		/// </summary>
		/// <param name="tOnHOverelegate">デリゲートメソッド</param>
		public void AddOnHover( OnHoverDelegate tOnHoverDelegate )
		{
			onHoverDelegate += tOnHoverDelegate ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnHoverDelegate">デリゲートメソッド</param>
		public void RemoveOnHover( OnHoverDelegate tOnHoverDelegate )
		{
			onHoverDelegate -= tOnHoverDelegate ;
		}

		//----------------------

		// Press

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, bool> onPressAction ;
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tState">状態(true=プレス・false=リリース)</param>
		public delegate void OnPressDelegate( string tIdentity, UIView tView, bool tState ) ;

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲート
		/// </summary>
		public OnPressDelegate onPressDelegate ;

		// 内部リスナー
		private void OnPressInner( bool state )
		{
			m_WaitForPress = true ;

			if( onPressAction != null || onPressDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onPressAction?.Invoke( identity, this, state ) ;
				onPressDelegate?.Invoke( identity, this, state ) ;
			}
		}
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクションを設定する → OnPress( string tIdentity, UIView tView, bool tState )
		/// </summary>
		/// <param name="tOnPress">アクションメソッド</param>
		public void SetOnPress( Action<string, UIView, bool> tOnPressAction )
		{
			onPressAction = tOnPressAction ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを追加する → OnPress( string tIdentity, UIView tView, bool tState )
		/// </summary>
		/// <param name="tOnPressDelegate">デリゲートメソッド</param>
		public void AddOnPress( OnPressDelegate tOnPressDelegate )
		{
			onPressDelegate += tOnPressDelegate ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnPressDelegate">デリゲートメソッド</param>
		public void RemoveOnPress( OnPressDelegate tOnPressDelegate )
		{
			onPressDelegate -= tOnPressDelegate ;
		}

		//----------------------

		// Click

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView> onClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		public delegate void OnClickDelegate( string tIdentity, UIView tView ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnClickDelegate onClickDelegate ;

		/// <summary>
		/// クリックを強制的に実行する
		/// </summary>
		public void ExecuteClick()
		{
			OnClickInner() ;
		}

		// 内部リスナー
		private void OnClickInner()
		{
			m_Click = true ;
			m_ClickCountTime = Time.frameCount ;

			m_WaitForClick = true ;

			if( onClickAction != null || onClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onClickAction?.Invoke( identity, this ) ;
				onClickDelegate?.Invoke( identity, this ) ;
			}
		}
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるアクションを設定する → OnClick( string tIdentity, UIView tView )
		/// </summary>
		/// <param name="tOnClick">アクションメソッド</param>
		public void SetOnClick( Action<string, UIView> tOnClickAction )
		{
			onClickAction = tOnClickAction ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnClick( string tIdentity, UIView tView )
		/// </summary>
		/// <param name="tOnClickDelegate">デリゲートメソッド</param>
		public void AddOnClick( OnClickDelegate tOnClickDelegate )
		{
			onClickDelegate += tOnClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnClickDelegate">デリゲートメソッド</param>
		public void RemoveOnClick( OnClickDelegate tOnClickDelegate )
		{
			onClickDelegate -= tOnClickDelegate ;
		}

		//----------------------

		// SmartClick

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tCount">クリック種別(1=シングル・2=ダブル)</param>
		/// <param name="tBasePosition">最初のカーソル座標</param>
		/// <param name="tMovePosition">最後のカーソル座標</param>
		public delegate void OnSmartClickDelegate( string tIdentity, UIView tView, int tCount, Vector2 tBasePosition, Vector2 tMovePosition ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnSmartClickDelegate onSmartClickDelegate ;
	
		// 内部リスナー
		private void OnSmartClickInner( int tCount, Vector2 tBasePosition, Vector2 tMovePosition )
		{
			if( onSmartClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onSmartClickDelegate( identity, this, tCount, tBasePosition, tMovePosition ) ;
			}
		}
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string tIdentity, UIView tView, int tCount, Vector2 tBasePosition, Vector2 tMovePosition )
		/// </summary>
		/// <param name="tOnSmartClickDelegate">デリゲートメソッド</param>
		public void SetOnSmartClick( OnSmartClickDelegate tOnSmartClickDelegate )
		{
			onSmartClickDelegate  = tOnSmartClickDelegate ;
		}
		
		/// <summary>
		/// スマートマリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="tLimitTime">シングルクリックと判定される押してから離すまでの時間(秒)</param>
		/// <param name="tAllowTime">ダブルクリックと判定される離してから押すまでの時間(秒)</param>
		/// <param name="tLimitDistance">クリックと判定される限界の移動量</param>
		public void SetSmartClickParameter(  float limitTime, float allowTime, float limitDistance )
		{
			if( limitTime >= 0 )
			{
				m_SmartClickLimitTime = limitTime ;
			}
			if( allowTime >= 0 )
			{
				m_SmartClickAllowTime = allowTime ;
			}
			if( limitDistance >= 0 )
			{
				m_SmartClickLimitDistance = limitDistance ;	// 最初に押した位置からどの程度の距離まで有効か
			}
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string tIdentity, UIView tView, int tCount, Vector2 tBasePosition, Vector2 tMovePosition )
		/// </summary>
		/// <param name="tOnSmartClickDelegate">デリゲートメソッド</param>
		public void AddOnSmartClick( OnSmartClickDelegate tOnSmartClickDelegate )
		{
			onSmartClickDelegate += tOnSmartClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnSmartClickDelegate">デリゲートメソッド</param>
		public void RemoveOnSmartClick( OnSmartClickDelegate tOnSmartClickDelegate )
		{
			onSmartClickDelegate -= tOnSmartClickDelegate ;
		}
		
		//----------------------

		// Hold		

		// Click

		/// <summary>
		/// ビューをホールドした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, float> onHoldAction ;
		
		/// <summary>
		/// ビューをホールドした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		public delegate void OnHoldDelegate( string tIdentity, UIView tView, float tState ) ;

		/// <summary>
		/// ビューをホールドした際に呼び出されるデリゲート
		/// </summary>
		public OnHoldDelegate onHoldDelegate ;

		// 内部リスナー
		private void OnHoldInner()
		{
			if( onHoldAction != null || onHoldDelegate != null )
			{
				if( m_HoldEnabled == false && m_HoldStartTime >  0 )
				{
					float tTime = Time.realtimeSinceStartup - m_HoldStartTime ;

					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					float state ;
					if( tTime >= m_HoldLimitTime )
					{
						state = 1 ;
						m_HoldEnabled = true ;
						m_HoldStartTime = 0 ;
					}
					else
					{
						state = tTime / m_HoldLimitTime ;
					}

					if( state >  0 )
					{
						onHoldAction?.Invoke( identity, this, state ) ;
						onHoldDelegate?.Invoke( identity, this, state ) ;
					}

					if( state == 1 )
					{
						UITransition tTransition = GetComponent<UITransition>() ;
						if( tTransition != null )
						{
							tTransition.OnPointerUp( null ) ;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// ビューをホールドした際に呼び出されるアクションを設定する → OnClick( string tIdentity, UIView tView )
		/// </summary>
		/// <param name="tOnHold">アクションメソッド</param>
		public void SetOnHold( Action<string, UIView, float> tOnHoldAction, float tHoldLimitTime = 0.75f )
		{
			onHoldAction = tOnHoldAction ;
			if( tHoldLimitTime >  0 )
			{
				m_HoldLimitTime = tHoldLimitTime ;
			}
		}

		/// <summary>
		/// ビューをホールドした際に呼び出されるデリゲートを追加する → OnClick( string tIdentity, UIView tView )
		/// </summary>
		/// <param name="tOnHoldDelegate">デリゲートメソッド</param>
		public void AddOnHold( OnHoldDelegate tOnHoldDelegate, float tHoldLimitTime = 0.75f )
		{
			onHoldDelegate += tOnHoldDelegate ;
			if( tHoldLimitTime >  0 )
			{
				m_HoldLimitTime = tHoldLimitTime ;
			}
		}

		/// <summary>
		/// ビューをホールドした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnHoldDelegate">デリゲートメソッド</param>
		public void RemoveOnHold( OnHoldDelegate tOnHoldDelegate )
		{
			onHoldDelegate -= tOnHoldDelegate ;
		}

		//----------------------

		// Drag

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tState">ドラッグの状態</param>
		/// <param name="tBasePosition">ドラッグ開始座標</param>
		/// <param name="tMovePosition">ドラッグ現在座標</param>
		public delegate void OnDragDelegate( string tIdentity, UIView tView, PointerState tState, Vector2 tBasePosition, Vector2 tMovePosition ) ;

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲート
		/// </summary>
		public OnDragDelegate onDragDelegate ;
	
		// 内部リスナー
		private void OnDragInner( PointerState tState, Vector2 tBasePosition, Vector2 tMovePosition )
		{
			if( onDragDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onDragDelegate( identity, this, tState, tBasePosition, tMovePosition ) ;
			}
		}
		
		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string tIdentity, UIView tView, PointerState tState, Vector2 tBasePosition, Vector2 tMovePosition )
		/// </summary>
		/// <param name="tOnDragDelegate"></param>
		public void SetOnDrag( OnDragDelegate tOnDragDelegate )
		{
			onDragDelegate  = tOnDragDelegate ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string tIdentity, UIView tView, PointerState tState, Vector2 tBasePosition, Vector2 tMovePosition )
		/// </summary>
		/// <param name="tOnDragDelegate"></param>
		public void AddOnDrag( OnDragDelegate tOnDragDelegate )
		{
			onDragDelegate += tOnDragDelegate ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnDragDelegate"></param>
		public void RemoveOnDrag( OnDragDelegate tOnDragDelegate )
		{
			onDragDelegate -= tOnDragDelegate ;
		}

		//----------------------

		// Flick

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, Vector2, Vector2> onFlickAction ;
		
		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tDistance">フリック移動量</param>
		/// <param name="tBasePosition">フリック開始座標</param>
		public delegate void OnFlickDelegate( string tIdentity, UIView tView, Vector2 tDistance, Vector2 tBasePosition ) ;

		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲート
		/// </summary>
		public OnFlickDelegate onFlickDelegate ;

		// 内部リスナー
		private void OnFlickInner( Vector2 tDistance, Vector2 tBasePosition )
		{
			if( onFlickAction != null || onFlickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onFlickAction?.Invoke( identity, this, tDistance, tBasePosition ) ;
				onFlickDelegate?.Invoke( identity, this, tDistance, tBasePosition ) ;
			}
		}
		
		/// <summary>
		/// フリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="tAllowTime">フリックと判定されるため押してから離すまでの最大時間(秒)</param>
		/// <param name="tMinimumDostance">フリックと判定されるための押してから離すまでの最小移動距離</param>
		public void SetOnFlickParameter( float tAllowTime, float tThresholdRatio = 60.0f / 960.0f )
		{
			m_FlickAllowTime		= tAllowTime ;
			m_FlickThresholdRatio	= tThresholdRatio ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを設定する → OnFlick( string tIdentity, UIView tView, Vector2 tDistance, Vector2 tBasePosition )
		/// </summary>
		/// <param name="tOnFlickAction">アクションメソッド</param>
		public void SetOnFlick( Action<string, UIView, Vector2, Vector2> tOnFlickAction )
		{
			onFlickAction			= tOnFlickAction ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを追加する → OnFlick( string tIdentity, UIView tView, Vector2 tDistance, Vector2 tBasePosition )
		/// </summary>
		/// <param name="tOnFlickDelegate">デリゲートメソッド</param>
		public void AddOnFlick( OnFlickDelegate tOnFlickDelegate )
		{
			onFlickDelegate += tOnFlickDelegate ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを削除する
		/// </summary>
		/// <param name="tOnFlickDelegate">デリゲートメソッド</param>
		public void RemoveOnFlick( OnFlickDelegate tOnFlickDelegate )
		{
			onFlickDelegate -= tOnFlickDelegate ;
		}

		//----------------------

		// Touch

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, TouchState[]> onTouchAction ;
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tTouchState">タッチ情報が格納された配列</param>
		public delegate void OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState ) ;	

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnTouchDelegate onTouchDelegate ;

		// 内部リスナー
		private void OnTouchInner( TouchState[] tTouchState )
		{
			if( onTouchAction != null || onTouchDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				// 存在するもののみ抽出する
				m_TouchStateExchange.Clear() ;
				int i, l = tTouchState.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tTouchState[ i ] != null )
					{
						m_TouchStateExchange.Add( tTouchState[ i ] ) ;
					}
				}

				onTouchAction?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
				onTouchDelegate?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
			}
		}
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクションを設定する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="tOnTouchAction">アクションメソッド</param>
		public void SetOnTouch( Action<string, UIView, TouchState[]> tOnTouchAction )
		{
			onTouchAction = tOnTouchAction ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="tOnTouchDelegate">デリゲートメソッド</param>
		public void AddOnTouch( OnTouchDelegate tOnTouchDelegate )
		{
			onTouchDelegate += tOnTouchDelegate ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnTouchDelegate">デリゲートメソッド</param>
		public void RemoveOnTouch( OnTouchDelegate tOnTouchDelegate )
		{
			onTouchDelegate -= tOnTouchDelegate ;
		}

		//--------------------------------------------------------------------

		private Dictionary<EventTriggerType, Action<string,UIView,EventTriggerType>> m_EventTriggerCallbackList = new Dictionary<EventTriggerType, Action<string, UIView, EventTriggerType>>() ;
		private Dictionary<EventTriggerType, EventTrigger.Entry>                     m_EventTriggerEntryList    = new Dictionary<EventTriggerType, EventTrigger.Entry>() ;

		/// <summary>
		/// EventTrigger のコールバックメソッドを登録する
		/// </summary>
		/// <param name="tCallback"></param>
		/// <param name="tTypeArray"></param>
		/// <returns></returns>
		public bool AddEventTriggerCallback( Action<string,UIView,EventTriggerType> tCallback, params EventTriggerType[] tTypeArray )
		{
			if( tCallback == null || tTypeArray == null || tTypeArray.Length == 0 )
			{
				// 引数が不正
#if UNITY_EDITOR
				Debug.LogWarning( name + " : " + "Bad parameter" ) ;
#endif
				return false ;
			}

			EventTrigger tEventTrigger = _eventTrigger ;
			if( tEventTrigger == null )
			{
				// イベントトリガーがアタッチされていない
				Debug.LogWarning( name + " : " + "Event Trigger not attached." ) ;
				return false ;
			}

			if( tEventTrigger.triggers == null )
			{
				tEventTrigger.triggers = new List<EventTrigger.Entry>() ;
			}
		
			int i ;
			EventTriggerType tType ;
			EventTrigger.Entry tEntry ;

			for( i  = 0 ; i <  tTypeArray.Length ; i ++ )
			{
				// 既に登録されている場合は上書きになる
				tType = tTypeArray[ i ] ;
				if( m_EventTriggerCallbackList.ContainsKey( tType ) == false )
				{
					// 新規登録
					m_EventTriggerCallbackList.Add( tType, tCallback ) ;
				}
				else
				{
					// 上書登録
					m_EventTriggerCallbackList[ tType ] = tCallback ;
				}

				// エントリーが既に登録されているかを確認する
				if( m_EventTriggerEntryList.ContainsKey( tType ) == true )
				{
					// 既に登録されている
					tEntry = m_EventTriggerEntryList[ tType ] ;

					if( tEventTrigger.triggers.Contains( tEntry ) == true )
					{
						// 登録済みなので一度破棄しておく
						tEventTrigger.triggers.Remove( tEntry ) ;
					}
				}
				else
				{
					// 登録されていないので新規にエントリーを生成する
					tEntry = new EventTrigger.Entry() ;
					tEntry.eventID = tType ;
					UnityAction<BaseEventData> tAction = null ;
					switch( tType )
					{
						case EventTriggerType.PointerEnter				: tAction = OnPointerEnterInner				; break ;
						case EventTriggerType.PointerExit				: tAction = OnPointerExitInner				; break ;
						case EventTriggerType.PointerDown				: tAction = OnPointerDownInner				; break ;
						case EventTriggerType.PointerUp					: tAction = OnPointerUpInner				; break ;
						case EventTriggerType.PointerClick				: tAction = OnPointerClickInner				; break ;
						case EventTriggerType.Drag						: tAction = OnDragInner						; break ;
						case EventTriggerType.Drop						: tAction = OnDropInner						; break ;
						case EventTriggerType.Scroll					: tAction = OnScrollInner					; break ;
						case EventTriggerType.UpdateSelected			: tAction = OnUpdateSelectedInner			; break ;
						case EventTriggerType.Select					: tAction = OnSelectInner					; break ;
						case EventTriggerType.Deselect					: tAction = OnDeselectInner					; break ;
						case EventTriggerType.Move						: tAction = OnMoveInner						; break ;
						case EventTriggerType.InitializePotentialDrag	: tAction = OnInitializePotentialDragInner	; break ;
						case EventTriggerType.BeginDrag					: tAction = OnBeginDragInner				; break ;
						case EventTriggerType.EndDrag					: tAction = OnEndDragInner					; break ;
						case EventTriggerType.Submit					: tAction = OnSubmitInner					; break ;
						case EventTriggerType.Cancel					: tAction = OnCancelInner					; break ;
					}
					tEntry.callback.AddListener( tAction ) ;

					m_EventTriggerEntryList.Add( tType, tEntry ) ;
				}

				// 改めてエントリーを登録する
				tEventTrigger.triggers.Add( tEntry ) ;
			}

			return true ;
		}

		private void OnPointerEnterInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerEnter, tData ) ;
		}

		private void OnPointerExitInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerExit, tData ) ;
		}

		private void OnPointerDownInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerDown, tData ) ;
		}

		private void OnPointerUpInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerUp, tData ) ;
		}

		private void OnPointerClickInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerClick, tData ) ;
		}

		private void OnDragInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drag, tData ) ;
		}

		private void OnDropInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drop, tData ) ;
		}

		private void OnScrollInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Scroll, tData ) ;
		}

		private void OnUpdateSelectedInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.UpdateSelected, tData ) ;
		}

		private void OnSelectInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Select, tData ) ;
		}

		private void OnDeselectInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Deselect, tData ) ;
		}

		private void OnMoveInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Move, tData ) ;
		}

		private void OnInitializePotentialDragInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.InitializePotentialDrag, tData ) ;
		}

		private void OnBeginDragInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.BeginDrag, tData ) ;
		}

		private void OnEndDragInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.EndDrag, tData ) ;
		}

		private void OnSubmitInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Submit, tData ) ;
		}

		private void OnCancelInner( BaseEventData tData )
		{
			InvokeEventTriggerCallback( EventTriggerType.Cancel, tData ) ;
		}
	
		private void InvokeEventTriggerCallback( EventTriggerType tType, BaseEventData tData )
		{
			if( m_EventTriggerCallbackList.ContainsKey( tType ) == false )
			{
				// コールバックが登録されていない
				return ;
			}
		
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			PointerEventData tPointer = tData as PointerEventData ;
			pointerId = tPointer.pointerId ;
			pointerPosition = GetLocalPosition( tPointer ) ;

			m_EventTriggerCallbackList[ tType ]( identity, this, tType ) ;
		}

		public int pointerId = -1 ;
		public Vector2 pointerPosition = Vector2.zero ;

		/// <summary>
		/// Pointer が Collider の内側にあるか判定する
		/// </summary>
		public bool isPointerInside
		{
			get
			{
				float x = pointerPosition.x ;
				float y = pointerPosition.y ;

				float w = this.Width ;
				float h = this.Height ;

				Vector2 p = Pivot ;

	//			Debug.Log( "x:"+ x + " y:" + y + " w:" + w + " h:" + h ) ;

				float tXMin = - ( w * p.x ) ;
				float tXMax = w * ( 1.0f - p.x ) ;
				float tYMin = - ( h * p.y ) ;
				float tYMax = h * ( 1.0f - p.y ) ;

	//			Debug.Log( "xMin:"+ tXMin + " xMax:" + tXMax + " yMin:" + tYMin + " yMax:" + tYMax ) ;

				if( x <  tXMin || x >  tXMax || y <  tYMin || y >  tYMax )
				{
					return false ;	// 外
				}

	//			Debug.Log( "入ってます" ) ;
				return true ;	// 中
			}
		}

		// そのＵＩ上の座標を取得する
		protected Vector2 GetLocalPosition( PointerEventData tPointer )
		{
			return GetLocalPosition( tPointer.position ) ;
		}

		//---------------------------------------------------------------------

#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_IOS )
#else
		private bool	m_SingleTouchState = false ;
		private int		m_SingleTouchFingerId = 0 ;
#endif

		/// <summary>
		/// レイキャストのプロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPosition"></param>
		/// <returns></returns>
		public int GetSinglePointer( ref Vector2 rPointer )
		{
			int tButton = 0 ;
			Vector2 tPointer = Vector2.zero ;

#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_IOS )

			int i ;
			for( i  = 0 ; i <= 2 ; i ++ )
			{
				if( Input.GetMouseButton( i ) == true )
				{
					tButton = tButton | ( 1 << i ) ;
				}
			}
			
			tPointer =  Input.mousePosition ;

#else

			if( Input.touchCount == 1 )
			{
				// 押された
				Touch tTouch = Input.GetTouch( 0 ) ;

				if( m_SingleTouchState == false )
				{
					m_SingleTouchState = true ;
					m_SingleTouchFingerId = tTouch.fingerId ;

					tPointer = tTouch.position ;

					tButton = 1 ;
				}
				else
				{
					if( m_SingleTouchFingerId == tTouch.fingerId )
					{
						tPointer = tTouch.position ;

						tButton = 1 ;
					}
					else
					{
						// ここに来ることは基本時にありえない
					}
				}
			}
			else
			if( Input.touchCount >= 2 )
			{
				// 複数押された
				tButton = 0 ;
			}
			else
			{
				// 離された
				m_SingleTouchState = false ;

				tButton = 0 ;
			}

#endif

			if( tButton != 0 )
			{
				rPointer = GetLocalPosition( tPointer ) ;
			}
			
			return tButton ;
		}

#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_IOS )
#else

		private bool[]	m_MultiTouchState    = new bool[ 10 ] ;
		private int[]	m_MultiTouchFingerId = new int[ 10 ] ;

#endif


		/// <summary>
		/// レイキャストのプロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPointer"></param>
		/// <returns></returns>
		public int GetMultiPointer( Vector2[] rPointer )
		{
			int i, l ;
			int tButton = 0 ;
			bool[] tEntry = new bool[ 10 ] ;
			Vector2[] tPointer = new Vector2[ 10 ] ;

#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_IOS )

			Vector2 tMousePosition = Input.mousePosition ;

			for( i  = 0 ; i <= 2 ; i ++ )
			{
				if( Input.GetMouseButton( i ) == true )
				{
					tButton = tButton | ( 1 << i ) ;
					tEntry[ i ] = true ;
					tPointer[ i ] = tMousePosition ;
				}
			}
			
#else
			int j, c, e ;

			l = m_MultiTouchState.Length ;

			c = Input.touchCount ;
			if( c >  0 )
			{
				for( i  = 0 ; i <  c ; i ++ )
				{
					// ＩＤが同じものを検査して存在するなら上書きする
					// ＩＤが同じものが存在しないなら新規に追加する

					Touch tTouch = Input.GetTouch( i ) ;
					int tFingerId = tTouch.fingerId ;
					Vector2 tPosition = tTouch.position ;

					e = -1 ;
					for( j  = 0 ; j <  l ; j ++ )
					{
						if( m_MultiTouchState[ j ] == true )
						{
							if( m_MultiTouchFingerId[ j ] == tFingerId )
							{
								// 既に登録済み
								tEntry[ j ] = true ;
								tPointer[ j ] = tPosition ;
								tButton = tButton | ( 1 << j ) ;
								break ;
							}
						}
						else
						{
							// 空いているスロットを発見した
							if( e <  0 )
							{
								e  = j ;
							}
						}	
					}

					if( j >= l && e <  l )
					{
						// 新規登録
						m_MultiTouchState[ e ] = true ;
						m_MultiTouchFingerId[ e ] = tFingerId ;

						tEntry[ e ] = true ;
						tPointer[ e ] = tPosition ;
						tButton = tButton | ( 1 << e ) ;
					}
				}
			}

			// 新規登録または上書更新が無かったスロットを解放する
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tEntry[ i ] == false )
				{
					m_MultiTouchState[ i ] = false ;
				}
			}


#endif

			if( tButton != 0 )
			{
				if( rPointer != null && rPointer.Length >  0 )
				{
					l = tEntry.Length ;
					if( rPointer.Length <  l )
					{
						l  = rPointer.Length ;
					}

					for( i  = 0 ; i <  l ; i ++ )
					{
						if( tEntry[ i ] == true )
						{
							rPointer[ i ] = GetLocalPosition( tPointer[ i ] ) ;
						}
					}
				}
			}
			
			return tButton ;
		}

		// そのＵＩ上の座標を取得する
		protected Vector2 GetLocalPosition( Vector2 tPosition )
		{
			Canvas tCanvas = GetParentCanvas() ;
			if( tCanvas != null )
			{
				if( tCanvas.renderMode == RenderMode.ScreenSpaceOverlay )
				{
					tPosition = transform.InverseTransformPoint( tPosition ) ;
				}
				else
				if( tCanvas.renderMode == RenderMode.ScreenSpaceCamera || tCanvas.renderMode == RenderMode.WorldSpace )
				{
					if( tCanvas.worldCamera != null && tCanvas.worldCamera.isActiveAndEnabled == true )
					{
						Vector2 tCanvasSize = GetCanvasSize() ;

						tPosition = tCanvas.worldCamera.ScreenToWorldPoint( tPosition ) ;

						tPosition.x = tPosition.x - transform.position.x ;
						tPosition.y = tPosition.y - transform.position.y ;

						if( tCanvas.worldCamera.orthographic == true )
						{
							float tHeight = tCanvas.worldCamera.orthographicSize ;
							float k = ( tCanvasSize.y * 0.5f ) / tHeight ;
							tPosition.x = tPosition.x * k ;
							tPosition.y = tPosition.y * k ;
						}
					}
					else
					{
						Vector2 tCanvasSize = GetCanvasSize() ;

						// キャンバス上の座標に変換する
						tPosition.x = ( ( tPosition.x / ( float )Screen.width  ) - 0.5f ) * tCanvasSize.x ;
						tPosition.y = ( ( tPosition.y / ( float )Screen.height ) - 0.5f ) * tCanvasSize.y ;

						Vector2 tCenter = PositionInCanvas ;

						tPosition = tPosition - tCenter ;
					}
				}

				return tPosition ;
			}

			return Vector2.zero ;
		}

		/// <summary>
		/// マウスのホイール値を取得する
		/// </summary>
		/// <returns></returns>
		public float GetWheelDistance()
		{
			 return Input.GetAxis( "Mouse ScrollWheel" ) ;
		}


		/// <summary>
		/// ピンチの距離を取得する
		/// </summary>
		/// <returns></returns>
		public float GetPinchDistance()
		{
			Vector2[] tMultiPointer = { Vector2.zero, Vector2.zero } ;

			int tButton = GetMultiPointer( tMultiPointer ) ;
			if( tButton == 3 )
			{
				// 最初の２点限定
				return ( tMultiPointer[ 1 ] - tMultiPointer[ 0 ] ).magnitude ;
			}

			return 0 ;
		}


		//--------------------------------------------------------------------

		/// <summary>
		/// GameObject を　Active にする(ショートカット)
		/// </summary>
		/// <param name="tState"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// GameObject の Active の有無(ショートカット)
		/// </summary>
		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="tFlag"></param>
		public void SetParent( Transform tParent, bool tFlag )
		{
			transform.SetParent( tParent, tFlag ) ;
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="tFlag"></param>
		public void SetParent( UIView parentView, bool flag )
		{
			transform.SetParent( parentView.transform, flag ) ;
		}

		/// <summary>
		/// UI の表示順番(ショートカット)
		/// </summary>
		public int siblingIndex
		{
			get
			{
				return transform.GetSiblingIndex() ;
			}
			set
			{
				transform.SetSiblingIndex( value ) ;
			}
		}

		/// <summary>
		/// UI の表示順番を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSiblingIndex()
		{
			return siblingIndex ;
		}

		/// <summary>
		/// UI の表示順番を設定する
		/// </summary>
		/// <param name="tIndex"></param>
		public void SetSiblingIndex( int tIndex )
		{
			siblingIndex = tIndex ;
		}

		/// <summary>
		/// UI の表示を最も奥にする(ショートカット)
		/// </summary>
		public void SetAsFirstSibling()
		{
			transform.SetAsFirstSibling() ;
		}

		/// <summary>
		/// UI の表示を最も手前にする(ショートカット)
		/// </summary>
		public void SetAsLastSibling()
		{
			transform.SetAsLastSibling() ;
		}

		/// <summary>
		/// Z 値に応じて表示順番を並び替える
		/// </summary>
		/// <param name="tReverse"></param>
		public void SortChildByZ( bool tReverse = false )
		{
			// 直接の子をＺ値に従ってソートする

			// 方法としては、最もＺが大きいものから順に手前に設定していく。

			int i, j, l = transform.childCount ;
			Transform[] t = new Transform[ l ] ;
			Transform s ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				t[ i ] = transform.GetChild( i ) ;
			}

			if( tReverse == false )
			{
				// 昇順（値が小さいものを奥に）
				for( i  = 0 ; i <  ( l - 1 ) ; i ++  )
				{
					for( j  = ( i + 1 ) ; j <  l ; j ++ )
					{
						if( t[ j ].localPosition.z <  t[ i ].localPosition.z )
						{
							// 入れ替え
							s = t[ i ] ;
							t[ i ] = t[ j ] ;
							t[ j ] = s ;
						}
					}
				}
			}
			else
			{
				// 降順（値が大きいものを奥に）
				for( i  = 0 ; i <  ( l - 1 ) ; i ++  )
				{
					for( j  = ( i + 1 ) ; j <  l ; j ++ )
					{
						if( t[ j ].localPosition.z >  t[ i ].localPosition.z )
						{
							// 入れ替え
							s = t[ i ] ;
							t[ i ] = t[ j ] ;
							t[ j ] = s ;
						}
					}
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				// 配列の最初のものから順に最前面に（最終的に最背面になる）
				t[ i ].SetAsLastSibling() ;
			}
		}

		/// <summary>
		/// ゲームオブジェクトを複製し指定のコンポーネントを取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Clone<T>() where T : UnityEngine.Component
		{
			GameObject tGameObject = Instantiate( gameObject ) ;
			tGameObject.transform.SetParent( transform.parent, false ) ;
			return tGameObject.GetComponent<T>() ;
		} 
		
		/// <summary>
		/// 指定したビューからの相対的な距離を取得する
		/// </summary>
		/// <param name="tBase"></param>
		/// <returns></returns>
		public Vector2 GetDistance( UIView tBase )
		{
			Vector2 t1 = transform.position ;
			Vector2 t0 = tBase.transform.position ;

			return t1 - t0 ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(X)を取得する
		/// </summary>
		/// <param name="tBase"></param>
		/// <returns></returns>
		public float GetDistanceX( UIView tBase )
		{
			return GetDistance( tBase ).x ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(Y)を取得する
		/// </summary>
		/// <param name="tBase"></param>
		/// <returns></returns>
		public float GetDistanceY( UIView tBase )
		{
			return GetDistance( tBase ).y ;
		}

		//-------------------------------------------------------------------
		// プレス待ち

		private bool m_WaitForPress = false ;

		public MovableState WaitForPress()
		{
			if( m_WaitForPress == true )
			{
				return null ;
			}

			m_WaitForPress = false ;
			MovableState tState = new MovableState() ;
			StartCoroutine( WaitForPress_Private( tState ) ) ;

			return tState ;
		}

		private IEnumerator WaitForPress_Private( MovableState tState )
		{
			yield return new WaitWhile( () => m_WaitForPress == false ) ;

			m_WaitForPress = false ;
			tState.IsDone = true ;
		}

		//-------------------------------------------------------------------
		// クリック待ち

		private bool m_WaitForClick = false ;

		public MovableState WaitForClick()
		{
			if( m_WaitForClick == true )
			{
				return null ;
			}

			m_WaitForClick = false ;
			MovableState tState = new MovableState() ;
			StartCoroutine( WaitForClick_Private( tState ) ) ;

			return tState ;
		}

		private IEnumerator WaitForClick_Private( MovableState tState )
		{
			yield return new WaitWhile( () => m_WaitForClick == false ) ;

			m_WaitForClick = false ;
			tState.IsDone = true ;
		}
	}
}

// メモ
// iTween でわかりやすい
// http://d.hatena.ne.jp/nakamura001/20121127/1354021902

