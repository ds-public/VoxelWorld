using System ;
using UnityEngine ;
using UnityEngine.EventSystems ;


namespace ScreenSizeHelper
{
	/// <summary>
	/// スクリーンのサイズ調整クラス Version 2024/02/22 0
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( RectTransform ) )]
	[DefaultExecutionOrder( -5 )]	// 通常よりも早く Update をコールさせる(ただし SafeAreaCorrector より後)
	public class ScreenSizeFitterBase : UIBehaviour
	{
		[Header( "自動サイズ調整を無効化するか(あらゆる処理の無効化)" )]

		[SerializeField]
		protected bool					m_IgnoreSizeFitting = false ;


		[Header( "セーフエリア対応が必要かどうか" )]

		[SerializeField]
		protected bool					m_SafeAreaEnabled = true ;


		//---------------------------------------------------------------------------
		// 以下はモニタリング用

		[Header( "親のサイズ ※ReadOnly" )]

		// 直親(Canvas または SafeAreaCorrector)の RectTransform
		[SerializeField]
		protected RectTransform			m_ParentRectTransform ;

		// 直親(Canvas または SafeAreaCorrector)の横幅
		[SerializeField]
		protected float					m_ParentWidth ;

		// 直親Canvas または SafeAreaCorrector)の縦幅
		[SerializeField]
		protected float					m_ParentHeight ;


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


		//-----------------------------------
		// 状態変化確認用

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

		//-----------------------------------

		// 自身の RectTransform
		protected RectTransform			m_RectTransform ;

		// 自身の RectTransform の更新が必要かどうか
		protected bool					m_IsDirty = false ;

#if UNITY_EDITOR
		// RectTransform のトラッカー(操作無効化)
		private DrivenRectTransformTracker m_Tracker ;
#endif
		//-------------------------------------------------------------------------------------------

		protected override void Awake()
		{
			base.Awake() ;

			//----------------------------------
			// 解像度情報を取得・設定する

			// 基準解像度
			m_BasicWidth	= 1080 ;
			m_BasicHeight	= 1920 ;

			// 最大解像度
			m_LimitWidth	= 1440 ;
			m_LimitHeight	= 2560 ;

			// 継承クラスの Awake() 呼び出し
			OnAwake() ;

			//----------------------------------------------------------
			// 自身の RectTransform を取得しておく

			if( transform is RectTransform )
			{
				m_RectTransform = transform as RectTransform ;
			}

#if UNITY_EDITOR
			if( m_RectTransform == null )
			{
				Debug.LogWarning( "[ScreenSizeFitter] Not found rectTransform." ) ;
			}
#endif
			//----------------------------------------------------------
			// 直親の Canvas を取得しておく(使用する訳けではない)

			if( Application.isPlaying == true )
			{
				// 実行時のみ処理する

				if( m_SafeAreaEnabled == true )
				{
					// セーフエリア対応は有効

					Canvas canvas = null ;
					var t = transform.parent ;

					while( canvas == null && t != null )
					{
						if( t.TryGetComponent<Canvas>( out canvas ) == false )
						{
							// 直親の Canvas は発見出来ず
							t = t.parent ;
						}
					}

					if( canvas != null )
					{
						// 直親の Canvas に SafeAreaCorrector が付いていなければ付ける
						if( canvas.gameObject.TryGetComponent<SafeAreaCorrector>( out var safeAreaCorrector ) == false )
						{
							safeAreaCorrector = canvas.gameObject.AddComponent<SafeAreaCorrector>() ;
						}
						else
						{
							// 元々付いていても無効化されている可能性は０では無いので念のため
							safeAreaCorrector.enabled = true ;
						}

						// 解像度を設定する
						safeAreaCorrector.SetResolution( m_BasicWidth, m_BasicHeight, m_LimitHeight, m_LimitHeight, GetSafeArea ) ;
					}
	#if UNITY_EDITOR
					else
					{
						Debug.LogWarning( "[ScreenSizeFitter] Not found canvas." ) ;
					}
	#endif
				}
				else
				{
					// セーフエリア対応は無効

					Canvas canvas = null ;
					var t = transform.parent ;

					while( canvas == null && t != null )
					{
						if( t.TryGetComponent<Canvas>( out canvas ) == false )
						{
							// 直親の Canvas は発見出来ず
							t = t.parent ;
						}
					}

					if( canvas != null )
					{
						// セーフエリア対応を行わない
						if( canvas.gameObject.TryGetComponent<SafeAreaCorrector>( out var safeAreaCorrector ) == true )
						{
							// 無効化
							safeAreaCorrector.enabled = false ;
						}
					}
				}
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

		protected override void Start()
		{
			base.Start() ;

			if( m_IgnoreSizeFitting == false )
			{
				Refresh() ;
			}
		}

		/// <summary>
		/// コンポーネントが Enabled の時に毎フレーム呼び出される(通常の MonoBehaviour の Update より少し早い)
		/// </summary>
		internal void Update()
		{
			// OnRectTransformDimensionsChange の挙動が不安定過ぎて信用できないため毎フレーム変化をチェックする
			//----------------------------------------------------------

			if( m_IgnoreSizeFitting == false )
			{
				CheckUpdate() ;

				// 必要に応じて自身の RectTransform を更新する
				if( m_IsDirty == true )
				{
					Refresh() ;
				}
			}
		}

		private void CheckUpdate()
		{
			if( m_ParentRectTransform == null || ( m_ParentRectTransform != null && m_ParentRectTransform != transform.parent ) )
			{
				// 更新が必要

				if( transform.parent != null && transform.parent is RectTransform )
				{
					m_ParentRectTransform = transform.parent as RectTransform ;
					m_IsDirty = true ;
				}
			}

			if( m_ParentRectTransform != null )
			{
				if( m_ParentWidth != m_ParentRectTransform.rect.width || m_ParentHeight != m_ParentRectTransform.rect.height )
				{
					m_IsDirty = true ;
				}
			}

			//-------------

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

		// 表示更新する
		private void Refresh()
		{
			if( m_RectTransform == null || m_ParentRectTransform == null )
			{
				// まさか取れない事は無いと思うが
				return ;
			}

			//------------------------------------------------------------------------------------------

			// 保持している直親の (Canvas・SafeAreaCorrector) のサイズを更新する
			m_ParentWidth  = m_ParentRectTransform.rect.width ;
			m_ParentHeight = m_ParentRectTransform.rect.height ;

			//----

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

			// ここは固定設定(Anchor=Center・Pivot=Center)
			m_RectTransform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			m_RectTransform.localScale	= Vector3.one ;
			m_RectTransform.anchorMin	= new Vector2( 0.5f, 0.5f ) ;
			m_RectTransform.anchorMax	= new Vector2( 0.5f, 0.5f ) ;
			m_RectTransform.pivot		= new Vector2( 0.5f, 0.5f ) ;

			//----------------------------------------------------------

			float width, height ;

			//------------------------------------------------------------------------------------------

			if( m_ParentWidth >= m_ParentHeight )
			{
				// 画面は横長
				float parentAspect = m_ParentWidth  / m_ParentHeight ;
				float basicAspect  = m_BasicWidth   / m_BasicHeight  ;

				if( parentAspect >  basicAspect )
				{
					// より横長
					float w = m_ParentWidth  ;

					if( w >  m_LimitWidth  )
					{
						w  = m_LimitWidth  ;
					}

					width	= w ;
					height	= m_BasicHeight ;
				}
				else
				if( parentAspect <  basicAspect )
				{
					// 基準より少ない横長
					float h = m_ParentHeight ;

					if( h >  m_LimitHeight )
					{
						h  = m_LimitHeight ;
					}

					width	= m_BasicWidth ;
					height	= h ;
				}
				else
				{
					// 丁度
					width	= m_BasicWidth ;
					height	= m_BasicHeight ;
				}
			}
			else
			{
				// 画面は縦長
				float parentAspect = m_ParentHeight / m_ParentWidth ;
				float basicAspect  = m_BasicHeight  / m_BasicWidth  ;

				if( parentAspect >  basicAspect )
				{
					// より縦長
					float h = m_ParentHeight ;

					if( h >  m_LimitHeight )
					{
						h  = m_LimitHeight ;
					}

					height	= h ;
					width	= m_BasicWidth  ;
				}
				else
				if( parentAspect <  basicAspect )
				{
					// 基準より少ない縦長
					float w = m_ParentWidth ;

					if( w >  m_LimitWidth  )
					{
						w  = m_LimitWidth  ;
					}

					height = m_BasicHeight ;
					width	= w ;
				}
				else
				{
					// 丁度
					height	= m_BasicHeight ;
					width	= m_BasicWidth  ;
				}
			}

			//----------------------------------------------------------

			// サイズ
			m_RectTransform.sizeDelta			= new Vector2( width, height ) ;

			// 中心位置(常に親の中心でＯＫ
			m_RectTransform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;

			// スケール(常に１倍でＯＫ)
			m_RectTransform.localScale			= Vector3.one ;

			// 強制更新
			m_RectTransform.ForceUpdateRectTransforms() ;

			//------------------------------------------------------------------------------------------

			// 更新終了
			m_IsDirty = false ;
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

#if UNITY_EDITOR
		// RectTransform のトラッカーを設定する
		private void SetRectTransformTracker( bool isSet )
		{
			if( m_RectTransform != null )
			{
				if( isSet == true )
				{
					m_Tracker.Add
					(
						this, m_RectTransform,
						DrivenTransformProperties.AnchoredPosition3D	|
						DrivenTransformProperties.SizeDelta				|
						DrivenTransformProperties.Anchors
					) ;
				}
				else
				{
					m_Tracker.Clear() ;
				}
			}
		}
#endif

		/// <summary>
		/// コンポーネントが Enabled になる際に呼び出される
		/// </summary>
		protected override void OnEnable()
		{
#if UNITY_EDITOR
			if( m_IgnoreSizeFitting == false )
			{
				// RectTransform のトラッカー(操作無効化)を有効
				SetRectTransformTracker( true ) ;
			}
#endif
			m_IsDirty = true ;
		}

		/// <summary>
		/// コンポーネントが Disabled になる際に呼び出される
		/// </summary>
		protected override void OnDisable()
		{
#if UNITY_EDITOR
			if( m_IgnoreSizeFitting == false )
			{
				// RectTransform のトラッカー(操作無効化)を無効
				SetRectTransformTracker( false ) ;
			}
#endif
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

			if( m_IgnoreSizeFitting == false )
			{
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

#if UNITY_EDITOR
		/// <summary>
		/// (Inspectorで)プロパティの編集が行われた際に呼び出される ※m_SafeAreaEnabled の変化の即時反映
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate() ;

			if( m_IgnoreSizeFitting == false )
			{
				// RectTransform のトラッカー(操作無効化)を有効
				SetRectTransformTracker( true ) ;
			}
			else
			{
				SetRectTransformTracker( false ) ;
			}
		}
#endif
	}
}
