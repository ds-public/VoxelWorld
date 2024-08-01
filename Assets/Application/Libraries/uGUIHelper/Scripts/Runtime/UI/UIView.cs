using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditor.SceneManagement ;
#endif


namespace uGUIHelper
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( RectTransform ) )]
	
	/// <summary>
	/// uGUIの使い勝手を向上させるヘルパークラス(基底クラス)
	/// </summary>
	public class UIView : UIBehaviour
	{
		public const string Version = "Version 2024/07/31 0" ;

		// ソースコード
		// https://bitbucket.org/Unity-Technologies/ui/src/2019.1/


		// SerializeField のリネーム方法
		// http://docs.unity3d.com/jp/current/ScriptReference/Serialization.FormerlySerializedAsAttribute.html
		// https://qiita.com/iwashihead/items/ab1b4f0363e07b32eaef

/*
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
*/


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
		/// 任意のオブジェクトを保持する(実行時のみ有効)
		/// </summary>
		[NonSerialized]
		public System.Object AnyObject ;

		/// <summary>
		/// キャンバスグループのアルファ値が指定値未満の場合はレイキャストを無効化する
		/// </summary>
		[SerializeField]
		protected float m_DisableRaycastUnderAlpha = 0.0f ;
		public float DisableRaycastUnderAlpha{ get{ return m_DisableRaycastUnderAlpha ; } set{ m_DisableRaycastUnderAlpha = value ; } }

		//-----------------------------------

		/// <summary>
		/// 子オブジェクトへの色の適用
		/// </summary>
		public    bool			  IsApplyColorToChildren{ get{ return m_IsApplyColorToChildren ; } set{ m_IsApplyColorToChildren = value ; } }

		[SerializeField]
		protected bool			m_IsApplyColorToChildren = false ;

		/// <summary>
		/// 親オブジェクトからの色の適用の無視
		/// </summary>
		public	  bool			  IgnoreParentEffectiveColor{ get{ return m_IgnoreParentEffectiveColor ; } set{ m_IgnoreParentEffectiveColor = value ; } }

		[SerializeField]
		protected bool			m_IgnoreParentEffectiveColor = false ;


		/// <summary>
		/// 子を含めた乗算色
		/// </summary>
		public	  Color			  EffectiveColor
		{
			get
			{
				return m_EffectiveColor ;
			}
			set
			{
				// 変化の判定をしてはいけない(非アクティブ→アクティブの際に色が変わっていなくて１フレーム反映されない事がある)

				m_EffectiveColor = value ;

				if( m_IsApplyColorToChildren == true && this.GetType() != typeof( UIButton ) )
				{
					ApplyColorToChidren( m_EffectiveColor, true ) ;
				}
			}
		}

		/// <summary>
		/// ボタンの状態により自動的に効果色を置き換えるかどうか
		/// </summary>
		public		bool			  EffectiveColorReplacing{ get{ return m_EffectiveColorReplacing ; } set{ m_EffectiveColorReplacing = value ; } }
		[SerializeField]
		protected	bool			m_EffectiveColorReplacing = true ;

		[SerializeField]
		protected Color			m_EffectiveColor = Color.white ;

		protected bool			m_RefreshChildrenColor = true ;
		protected Color			m_PreviousColor ;

		//-------------------------------------------------------------------------------------------

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
		/// <param name="state"></param>
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

			m_RefreshChildrenColor	= true ;	// 子への影響色は再び設定が必要

			m_HoverAtFirst			= false ;	// 非アクティブになったら一度ホバーフラグはクリアする

			m_SingleClickCheck		= false ;	// スマートクリックのシングルクリックを計測していたなら無効化

			if( m_ActiveAnimations != null )
			{
				StopAllAnimators() ;
				m_ActiveAnimations.Clear() ;	// コルーチンが止まるので実行中のアニメーションはすべて停止する
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動くものの表示確認用クラス
		/// </summary>
		public class AsyncState : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public AsyncState( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && string.IsNullOrEmpty( Error ) == true && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
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

			/// <summary>
			/// 多目的保存値
			/// </summary>
			public System.Object	option ;
		}

		//-------------------------------------------------------------------------------------------
	
		protected override void Awake()
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

			// オーバーライドマテリアルを設定する(プレハブの場合は Awake で毎回設定しないと、プレハブオリジナルのものが設定されてしまう)
			var graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Default && m_ActiveMaterial == null )
			{
				m_ActiveMaterial = CreateCustomMaterial( m_MaterialType ) ;
			}

			if( graphic != null && m_ActiveMaterial != null )
			{
				graphic.material = m_ActiveMaterial ;
				graphic.GraphicUpdateComplete() ;
			}

			// 基本的なインタラクションイベントを登録する(非アクティブの場合はStartはアクティブになるまで呼ばれないためAwakeでないとマズい)
			if( Application.isPlaying == true )
			{
				var pivot = Pivot ;
				if( m_AutoPivotToCenter == true && ( pivot.x != 0.5f || pivot.y != 0.5f ) )
				{
					// ピボットを強制的に中心に補正する
					SetPivot( 0.5f, 0.5f, true ) ;	
				}

				//---------------------------------

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
			if( transform is RectTransform rectTransform )
			{
				rectTransform.anchoredPosition3D = Vector3.zero ;
				rectTransform.localScale = Vector3.one ;
				rectTransform.localRotation = Quaternion.identity ;
			}
		}

		protected virtual void OnAwake(){}

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
				var childObjects = new GameObject[ l ] ;
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
		protected virtual void OnBuild( string option = "" ){}

		protected override void Start()
		{
			base.Start() ;

			OnStart() ;
		}

		protected virtual void OnStart(){}

		internal void Update()
		{
#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tweenChecker = false ;
				var tweens = GetComponents<UITween>() ;
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
			//----------------------------------------------------------

			if( Application.isPlaying == true )
			{
				// バックキーの処理
				if( m_BackKeyEnabled == true )
				{
					if( InputAdapter.UIEventSystem.GetKeyDown( InputAdapter.KeyCodes.Escape ) == true )
					{
						ProcessBackKey() ;
					}
				}

				//----------------------------------------------------------

				SingleClickCheckProcess() ;

				if( OnHoverAction != null || OnHoverDelegate != null )
				{
					ProcessHover() ;
				}

				if( OnPinchAction != null || OnPinchDelegate != null || OnTouchAction != null || OnTouchDelegate != null )
				{
					ProcessMultiTouch() ;
				}

				if( OnRepeatPressAction != null || OnRepeatPressDelegate != null )
				{
					ProcessRepeatPress() ;
				}

				if( OnLongPressAction != null || OnLongPressDelegate != null )
				{
					ProcessLongPress() ;
				}

				if( OnWheelAction != null || OnWheelDelegate != null )
				{
					ProcessWheel() ;
				}

				if( m_Click == true && m_ClickCountTime != Time.frameCount )
				{
					m_Click  = false ;
				}
			}

			//----------------------------------------------------------

			// 子への色反映(UIButton の場合は別処理を行うのでここでは処理しない)
			if( m_IsApplyColorToChildren == true && m_EffectiveColorReplacing == false )
			{
				ApplyColorToChidren( m_EffectiveColor, true ) ;
			}

			//----------------------------------------------------------

			OnUpdate() ;

			//----------------------------------------------------------
		}

		/// <summary>
		/// 子のオブジェクトに親のオブジェクトの色を適用
		/// </summary>
		public void ApplyColorToChidren( Color color, bool withMyself = false )
		{
			if( m_RefreshChildrenColor == false && ( m_PreviousColor.r == color.r && m_PreviousColor.g == color.g && m_PreviousColor.b == color.b && m_PreviousColor.a == color.a ) && withMyself == false )
			{
				// 設定済みの色に変化無し
				return ;	// 何もしない
			}

			//----------------------------------

			// 子 GameObject 群の CanvasRenderer を取得する
			IEnumerable<CanvasRenderer> targets ;

			if( withMyself == false )
			{
				// 自身を含めない
				targets = GetComponentsInChildren<CanvasRenderer>().Where( _ => _.gameObject != gameObject ) ;
			}
			else
			{
				// 自身を含める
				targets = GetComponentsInChildren<CanvasRenderer>() ;
			}

			if( targets != null && targets.Any() == true )
			{
				foreach( var target in targets )
				{
					if( target.TryGetComponent<UIView>( out var view ) == false )
					{
						target.SetColor( color ) ;
					}
					else
					if( view.IgnoreParentEffectiveColor == false )
					{
						target.SetColor( color ) ;
					}
				} ;
			}

			//----------------------------------

			// 現在の色を保存する
			m_PreviousColor.r = color.r ;
			m_PreviousColor.g = color.g ;
			m_PreviousColor.b = color.b ;
			m_PreviousColor.a = color.a ;

			// 更新済み
			m_RefreshChildrenColor = false ;
		}

#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( m_RemoveCanvasRenderer == true )
			{
				RemoveCanvasRenderer() ;
				m_RemoveCanvasRenderer = false ;
			}
		
			if( m_RemoveGraphicEmpty == true )
			{
				RemoveGraphicEmpty() ;
				m_RemoveGraphicEmpty = false ;
			}

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

			if( m_RemoveHorizontalLayoutGroup == true )
			{
				RemoveHorizontalLayoutGroup() ;
				m_RemoveHorizontalLayoutGroup = false ;
			}

			if( m_RemoveVerticalLayoutGroup == true )
			{
				RemoveVerticalLayoutGroup() ;
				m_RemoveVerticalLayoutGroup = false ;
			}

			if( m_RemoveGridLayoutGroup == true )
			{
				RemoveGridLayoutGroup() ;
				m_RemoveGridLayoutGroup = false ;
			}

			if( m_RemoveContentSizeFitter == true )
			{
				RemoveContentSizeFitter() ;
				m_RemoveContentSizeFitter = false ;
			}

			if( m_RemoveLayoutElement == true )
			{
				RemoveLayoutElement() ;
				m_RemoveLayoutElement = false ;
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

			if( m_RemoveButtonGroup == true )
			{
				RemoveButtonGroup() ;
				m_RemoveButtonGroup = false ;
			}

			if( m_RemoveToggleGroup == true )
			{
				RemoveToggleGroup() ;
				m_RemoveToggleGroup = false ;
			}

			if( m_RemovePadAdapter == true )
			{
				RemovePadAdapter() ;
				m_RemovePadAdapter = false ;
			}

			if( m_RemoveAnimator == true )
			{
				RemoveAnimator() ;
				m_RemoveAnimator = false ;
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


		protected virtual void OnUpdate(){}

		internal void LateUpdate()
		{
			OnLateUpdate() ;

			if( m_MaterialType == MaterialTypes.Sepia )
			{
				ProcessSepia() ;
			}
			else
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

		protected virtual void OnLateUpdate(){}

		//------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalPosition = Vector3.zero ;

		/// <summary>
		/// 位置(ローカルキャッシュ)
		/// </summary>
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

		/// <summary>
		/// 位置(ローカル)
		/// </summary>
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
					if( transform is RectTransform rectTransform )
					{
						return rectTransform.anchoredPosition3D ;
					}
					return Vector3.zero ;
				}
			}
			set
			{
				m_LocalPosition = value ;

				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchoredPosition3D = value ;
				}
			}
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
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
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionX( float x )
		{
			Position = new Vector3( x, Position.y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionY( float y )
		{
			Position = new Vector3( Position.x, y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionZ( float z )
		{
			Position = new Vector3( Position.x, Position.y, z ) ;
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
		/// 基準ポジションを更新する
		/// </summary>
		public void RefreshPosition()
		{
			if( transform is RectTransform rectTransform )
			{
				m_LocalPosition = rectTransform.anchoredPosition3D ;
			}
		}

		/// <summary>
		/// サイズ(ショートカット)
		/// </summary>
		public virtual Vector2 Size
		{
			get
			{
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.rect.size ;
				}
				return Vector2.zero ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					var size = rectTransform.sizeDelta ;
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
		}

		/// <summary>
		/// 現在のピボットから矩形の中心への相対的な距離
		/// </summary>
		public Vector3 Center
		{
			get
			{
				var size	= Size ;
				var pivot	= Pivot ;

				return new Vector3( size.x * ( 0.5f - pivot.x ), size.y * ( 0.5f - pivot.y ), 0 ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する(ＵＩの中心)
		/// </summary>
		public Vector2 CenterPositionInCanvas
		{
			get
			{
				var position = PositionInCanvas ;

				float w = Width ;
				float h = Height ;
				float cx = position.x + ( 0.5f - Pivot.x ) * w ;
				float cy = position.y + ( 0.5f - Pivot.y ) * h ;

				return new Vector2( cx, cy ) ;
			}
		}

		/// <summary>
		/// キャンバス上での座標を取得する(ＵＩのピボット)
		/// </summary>
		public Vector2 PositionInCanvas
		{
			get
			{
				// 属するキャンバスのサイズ
				var ps = GetCanvasSize() ;

				var hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				var t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				while( t != null )
				{
					if( t.GetComponent<Canvas>() == null )
					{
						if( t is RectTransform )
						{
							hierarchyRects.Add( t as RectTransform ) ;
						}
					}
					else
					{
						break ;	// 属するキャンバスが見つかったので終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return Vector2.zero ;	// 異常
				}

				//---------------------------------

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;

				l = hierarchyRects.Count ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;		// 親の最小
						px1  = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) ;
					}
					else
					{
						// 中心位置
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;		// 親の最小
						py1  = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) ;
					}
					else
					{
						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
				}

				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				return new Vector2( px, py ) ;
			}
		}

		/// <summary>
		/// キャンバス上での領域を取得する(画面中心が原点[0,0]・領域は左下が基準位置[x,y]となる)
		/// </summary>
		public Rect RectInCanvas
		{
			get
			{
				// 属するキャンバスのサイズ
				var ps = GetCanvasSize() ;

//				Debug.LogWarning( "キャンバスの大きさ:" + ps ) ;

				var hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				var t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				while( t != null )
				{
					if( t.GetComponent<Canvas>() == null )
					{
						if( t is RectTransform )
						{
							hierarchyRects.Add( t as RectTransform ) ;
						}
					}
					else
					{
						break ;	// 属するキャンバスが見つかったので終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				//---------------------------------

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;

				l = hierarchyRects.Count ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;		// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) ;
					}
					else
					{
						// 中心位置
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;		// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) ;
					}
					else
					{
						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
				}
				
				// 画面の中心基準
				px -= ( ps.x * 0.5f ) ;
				py -= ( ps.y * 0.5f ) ;

				pw *= GetRectTransform().localScale.x ;
				ph *= GetRectTransform().localScale.y ;

				px -= ( pw * Pivot.x ) ;
				py -= ( ph * Pivot.y ) ;

				return new Rect( px, py, pw, ph ) ;
			}
		}

		/// <summary>
		/// キャンバス上でのビューポートを取得する(画面中心が原点[0,0]・値は -1 ～ +1 の範囲)
		/// </summary>
		public Rect ViewInCanvas
		{
			get
			{
				// 属するキャンバスのサイズ
				var ps = GetCanvasSize() ;

				var hierarchyRects = new List<RectTransform>() ;
				int i, l ;

				var t = transform ;
				RectTransform rt ;

				// まずはキャンバスを検出するまでリストに格納する
				while( t != null )
				{
					if( t.GetComponent<Canvas>() == null )
					{
						if( t is RectTransform )
						{
							hierarchyRects.Add( t as RectTransform ) ;
						}
					}
					else
					{
						break ;	// 属するキャンバスが見つかったので終了
					}

					t = t.parent ;
				}

				if( hierarchyRects.Count <= 0 )
				{
					return new Rect() ;	// 異常
				}

				//---------------------------------

				float pw = ps.x ;
				float ph = ps.y ;

				float px  = pw * 0.5f ;
				float px0 = 0, px1 ;

				float py  = ph * 0.5f ;
				float py0 = 0, py1 ;

				l = hierarchyRects.Count ;
				for( i  = ( l - 1 ) ; i >= 0 ; i -- )
				{
					rt = hierarchyRects[ i ] ;

					// X

					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.x != rt.anchorMax.x )
					{
						px0 += ( pw * rt.anchorMin.x ) ;		// 親の最小
						px1 = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
						// マージンの補正をかける
						px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
						px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

						pw = px1 - px0 ;

						// 中心位置
						px = px0 + ( pw * rt.pivot.x ) ;
					}
					else
					{
						// 中心位置
						px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

						pw = rt.sizeDelta.x ;
					}

					// 親の範囲更新
					px0 = px - ( pw * rt.pivot.x ) ;

					// Y
					// 自身の横幅(次の親の横幅)
					if( rt.anchorMin.y != rt.anchorMax.y )
					{
						py0 += ( ph * rt.anchorMin.y ) ;		// 親の最小
						py1 = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
						// マージンの補正をかける
						py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
						py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

						ph = py1 - py0 ;

						// 中心位置
						py = py0 + ( ph * rt.pivot.y ) ;
					}
					else
					{
						// 中心位置
						py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

						ph = rt.sizeDelta.y ;
					}

					// 親の範囲更新
					py0 = py - ( ph * rt.pivot.y ) ;
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
			}
		}

		/// <summary>
		/// 特定のコンポーネントのついた GameObject を親としてその親上での位置を取得する
		/// </summary>
		public Vector2 GetPositionIn<T>() where T : Component
		{
			int i, l ;

			//----------------------------------------------------------

			UIView screenView = null ;

			var hierarchyRects = new List<RectTransform>() ;

			var t = transform ;
			RectTransform rt ;

			// まずはスクリーンを検出するまでリストに格納する
			while( t != null )
			{
				if( t.GetComponent<T>() == null )
				{
					if( t is RectTransform )
					{
						hierarchyRects.Add( t as RectTransform ) ;
					}
				}
				else
				{
					screenView = t.GetComponent<UIView>() ;
					break ;	// 終了
				}

				t = t.parent ;
			}

			if( screenView == null )
			{
#if UNITY_EDITOR
				var isPrefabMode = PrefabStageUtility.GetCurrentPrefabStage() != null ;
				if( isPrefabMode == false )
				{
					// 異常
					Debug.LogWarning( "Not found parent : Component = <" + typeof( T ).ToString() + "> : Path = " + this.Path ) ;
				}
#endif
				// 指定のコンポーネントの付いたビューが見つからなかったのでキャンバス上の座標を返す
				return CenterPositionInCanvas ;
			}

			//------------------------------------------------------------------------------------------

			// 親サイズ
			var ps = screenView.Size ;

			float pw = ps.x ;
			float ph = ps.y ;

			float px  = pw * 0.5f ;
			float px0 = 0, px1 ;

			float py  = ph * 0.5f ;
			float py0 = 0, py1 ;

			l = hierarchyRects.Count ;
			for( i  = ( l - 1 ) ; i >= 0 ; i -- )
			{
				rt = hierarchyRects[ i ] ;

				// X

				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.x != rt.anchorMax.x )
				{
					px0 += ( pw * rt.anchorMin.x ) ;		// 親の最小
					px1  = px0 + ( pw * rt.anchorMax.x ) ;	// 親の最大
						
					// マージンの補正をかける
					px0 -= ( ( rt.sizeDelta.x *       rt.pivot.x   ) - rt.anchoredPosition.x ) ;
					px1 += ( ( rt.sizeDelta.x * ( 1 - rt.pivot.x ) ) + rt.anchoredPosition.x ) ;

					pw = px1 - px0 ;

					// 中心位置
					px = px0 + ( pw * rt.pivot.x ) ;
				}
				else
				{
					// 中心位置
					px = px0 + ( pw * rt.anchorMin.x ) + rt.anchoredPosition.x ;

					pw = rt.sizeDelta.x ;
				}

				// 親の範囲更新
				px0 = px - ( pw * rt.pivot.x ) ;

				// Y
				// 自身の横幅(次の親の横幅)
				if( rt.anchorMin.y != rt.anchorMax.y )
				{
					py0 += ( ph * rt.anchorMin.y ) ;		// 親の最小
					py1  = py0 + ( ph * rt.anchorMax.y ) ;	// 親の最大
						
					// マージンの補正をかける
					py0 -= ( ( rt.sizeDelta.y *       rt.pivot.y   ) - rt.anchoredPosition.y ) ;
					py1 += ( ( rt.sizeDelta.y * ( 1 - rt.pivot.y ) ) + rt.anchoredPosition.y ) ;

					ph = py1 - py0 ;

					// 中心位置
					py = py0 + ( ph * rt.pivot.y ) ;
				}
				else
				{
					// 中心位置
					py = py0 + ( ph * rt.anchorMin.y ) + rt.anchoredPosition.y ;

					ph = rt.sizeDelta.y ;
				}

				// 親の範囲更新
				py0 = py - ( ph * rt.pivot.y ) ;
			}

			// 画面の中心基準
			px -= ( ps.x * 0.5f ) ;
			py -= ( ps.y * 0.5f ) ;

			var position = new Vector2( px, py ) ;

			//----------------------------------------------------------

			// 中心位置に補正する
			var pivot = Pivot ;
			float w = Width ;
			float h = Height ;

			position.x += ( ( 0.5f - pivot.x ) * w ) ;
			position.y += ( ( 0.5f - pivot.y ) * h ) ;

			return position ;
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
				if( transform is RectTransform rectTransform )
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
				if( transform is RectTransform rectTransform )
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
					var text = this as UIText ;
					return text.TextSize.x ;
				}
				else
				if( this is UIRichText )
				{
					var text = this as UIRichText ;
					return text.TextSize.x ;
				}
				else
				if( this is UITextMesh )
				{
					var text = this as UITextMesh ;
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
					var text = this as UIText ;
					return text.TextSize.y ;
				}
				else
				if( this is UIRichText )
				{
					var text = this as UIRichText ;
					return text.TextSize.y ;
				}
				else
				if( this is UITextMesh )
				{
					var text = this as UITextMesh ;
					return text.TextSize.y ;
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
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMin ;
				}
				return Vector2.zero ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMin = value ;
				}
			}
		}
		
		/// <summary>
		/// アンカー最少値を設定
		/// </summary>
		/// <param name="anchorMin"></param>
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
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMax ;
				}
				return Vector2.zero ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMax = value ;
				}
			}
		}
		
		/// <summary>
		/// アンカー最大値を設定
		/// </summary>
		/// <param name="anchorMax"></param>
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
		/// <param name="anchorMin"></param>
		/// <param name="anchorMax"></param>
		public void SetAnchorMinAndMax( Vector2 anchorMin, Vector2 anchorMax, bool correct = false )
		{
			if( correct == true && transform.parent != null )
			{
				// 表示位置は変化させない

				if( transform.parent.TryGetComponent<UIView>( out var parentView ) == true )
				{
					float w = parentView.Width ;

					float ax0 = 0 ;
					if( AnchorMin.x == 0.0f && AnchorMax.x == 0.0f )
					{
						// 変化前のアンカーは左
						ax0 = 0.0f ;
					}
					else
					if( AnchorMin.x == 0.5f && AnchorMax.x == 0.5f )
					{
						// 変化前のアンカーは中
						ax0 = 0.5f ;
					}
					else
					if( AnchorMin.x == 1.0f && AnchorMax.x == 1.0f )
					{
						// 変化前のアンカーは右
						ax0 = 1.0f ;
					}

					float ax1 = 0 ;
					if( anchorMin.x == 0.0f && anchorMax.x == 0.0f )
					{
						// 変化後のアンカーは左
						ax1 = 0.0f ;
					}
					else
					if( anchorMin.x == 0.5f && anchorMax.x == 0.5f )
					{
						// 変化後のアンカーは中
						ax1 = 0.5f ;
					}
					else
					if( anchorMin.x == 1.0f && anchorMax.x == 1.0f )
					{
						// 変化後のアンカーは右
						ax1 = 1.0f ;
					}

					Rx -= ( ( ax1 - ax0 ) * w ) ;

					//---

					float h = parentView.Height ;

					float ay0 = 0 ;
					if( AnchorMin.y == 0.0f && AnchorMax.y == 0.0f )
					{
						// 変化前のアンカーは左
						ay0 = 0.0f ;
					}
					else
					if( AnchorMin.y == 0.5f && AnchorMax.y == 0.5f )
					{
						// 変化前のアンカーは中
						ay0 = 0.5f ;
					}
					else
					if( AnchorMin.y == 1.0f && AnchorMax.y == 1.0f )
					{
						// 変化前のアンカーは右
						ay0 = 1.0f ;
					}

					float ay1 = 0 ;
					if( anchorMin.y == 0.0f && anchorMax.y == 0.0f )
					{
						// 変化後のアンカーは左
						ay1 = 0.0f ;
					}
					else
					if( anchorMin.y == 0.5f && anchorMax.y == 0.5f )
					{
						// 変化後のアンカーは中
						ay1 = 0.5f ;
					}
					else
					if( anchorMin.y == 1.0f && anchorMax.y == 1.0f )
					{
						// 変化後のアンカーは右
						ay1 = 1.0f ;
					}

					Ry -= ( ( ay1 - ay0 ) * h ) ;
				}
			}

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}
		
		/// <summary>
		/// アンカー最大値・最小値を設定
		/// </summary>
		/// <param name="anchorMinX"></param>
		/// <param name="anchorMinY"></param>
		/// <param name="anchorMaxX"></param>
		/// <param name="anchorMaxY"></param>
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
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMin.x ;
				}
				return 0 ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMin = new Vector2( value, rectTransform.anchorMin.y ) ;
				}
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMinY
		{
			get
			{
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMin.y ;
				}
				return 0 ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMin = new Vector2( rectTransform.anchorMin.x, value ) ;
				}
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxX
		{
			get
			{
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMax.x ;
				}
				return 0 ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMax = new Vector2( value, rectTransform.anchorMax.y ) ;
				}
			}
		}

		/// <summary>
		/// アンカー最大値(ショートカット)
		/// </summary>
		public float AnchorMaxY
		{
			get
			{
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.anchorMax.y ;
				}
				return 0 ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.anchorMax = new Vector2( rectTransform.anchorMax.x, value ) ;
				}
			}
		}

		/// <summary>
		/// アンカーの値を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchor( float x, float y )
		{
			SetAnchorX( x ) ;
			SetAnchorY( y ) ;
		}

		/// <summary>
		/// アンカーＸの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorX( float x )
		{
			SetAnchorX( x, x ) ;
		}

		/// <summary>
		/// アンカーのＸ値を設定する
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="maxX"></param>
		public void SetAnchorX( float minX, float maxX )
		{
			var anchorMin = AnchorMin ;
			var anchorMax = AnchorMax ;

			anchorMin.x = minX ;
			anchorMax.x = maxX ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}

		/// <summary>
		/// アンカーＹの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorY( float y )
		{
			SetAnchorY( y, y ) ;
		}

		/// <summary>
		/// アンカーのＹ値を設定する
		/// </summary>
		/// <param name="minY"></param>
		/// <param name="maxY"></param>
		public void SetAnchorY( float minY, float maxY )
		{
			var anchorMin = AnchorMin ;
			var anchorMax = AnchorMax ;

			anchorMin.y = minY ;
			anchorMax.y = maxY ;

			AnchorMin = anchorMin ;
			AnchorMax = anchorMax ;
		}


		/// <summary>
		/// アンカーを位置から設定
		/// </summary>
		/// <param name="anchors"></param>
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
		/// <returns></returns>
		public RectOffset GetMargin()
		{
			var margin = new RectOffset() ;

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
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void GetMargin( out float left, out float right, out float top, out float bottom )
		{
			left	= 0 ;
			right	= 0 ;

			top		= 0 ;
			bottom	= 0 ;

			if( transform is RectTransform rectTransform )
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
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void GetMarginX( out float left, out float right )
		{
			left	= 0 ;
			right	= 0 ;

			if( transform is RectTransform rectTransform )
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
			}
		}

		/// <summary>
		/// 縦方向のマージンを取得
		/// </summary>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void GetMarginY( out float top, out float bottom )
		{
			top		= 0 ;
			bottom	= 0 ;

			if( transform is RectTransform rectTransform )
			{
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
		/// マージンを設定
		/// </summary>
		/// <param name="margin"></param>
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
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void SetMargin( float left, float right, float top, float bottom )
		{
			if( transform is RectTransform rectTransform )
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
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void SetMarginX( float left, float right )
		{
			if( transform is RectTransform rectTransform )
			{
				if( rectTransform.anchorMin.x != rectTransform.anchorMax.x )
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
		}
		
		/// <summary>
		/// 縦方向のマージンを設定
		/// </summary>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		public void SetMarginY( float top, float bottom )
		{
			if( transform is RectTransform rectTransform )
			{
				if( rectTransform.anchorMin.y != rectTransform.anchorMax.y )
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
		}
	
		/// <summary>
		/// ピボット(ショートカト)
		/// </summary>
		public Vector2 Pivot
		{
			get
			{
				if( transform is RectTransform rectTransform )
				{
					return rectTransform.pivot ;
				}
				return new Vector2( 0.5f, 0.5f ) ;
			}
			set
			{
				if( transform is RectTransform rectTransform )
				{
					rectTransform.pivot = value ;
				}
			}
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPivot( float x, float y, bool correct = false )
		{
			if( transform is not RectTransform )
			{
				return ;
			}

			var rt = transform as RectTransform ;

			//----------------------------------

			var localPosition = transform.localPosition ;
			var size = rt.rect.size ;

			//--------------

			var pivotOld = rt.pivot ;
			var pivotNew = rt.pivot ;

			pivotNew.x = x ;
			pivotNew.y = y ;

			rt.pivot = pivotNew ;

			if( correct == true )
			{
				// 表示位置は変化させない

				//---------------------------------

				// X
				localPosition.x += ( x - pivotOld.x ) * size.x ;

				// Y
				localPosition.y += ( y - pivotOld.y ) * size.y ;

				// 表示位置をピボット変更前と同じ位置に補正する
				transform.localPosition = localPosition ;
			}

			//----------------------------------

			m_LocalPosition = rt.anchoredPosition3D ;
		}
		
		/// <summary>
		/// ピボットを設定
		/// </summary>
		/// <param name="pivot"></param>
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
		/// <param name="pivots"></param>
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

		/// <summary>
		/// ピボットを自動的に実行時に中心にする
		/// </summary>
		public bool AutoPivotToCenter{ get{ return m_AutoPivotToCenter ; } set{ m_AutoPivotToCenter = value ; } }
		[SerializeField][Tooltip( "ランタイム実行時に\nピボットを強制的に中心(0.5,0.5)に変更します" )]
		protected bool m_AutoPivotToCenter = false ;

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
		/// ２Ｄでの回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( float axisZ )
		{
			Roll = axisZ ;
		}

		/// <summary>
		/// ３軸での回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( Vector2 value )
		{
			Rotation = value ;
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
					if( transform is RectTransform rectTransform )
					{
						return rectTransform.localEulerAngles ;
					}
					return Vector3.zero ;
				}
			}
			set
			{
				m_LocalRotation = value ;

				if( transform is RectTransform rectTransform )
				{
					rectTransform.localEulerAngles = value ;
				}
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

		/// <summary>
		/// 基準ローテーションを更新する
		/// </summary>
		public void RefreshRotation()
		{
			m_LocalRotation = GetRectTransform().localEulerAngles ;
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
					if( transform is RectTransform rectTransform )
					{
						return rectTransform.localScale ;
					}
					return Vector3.zero ;
				}
			}
			set
			{
				m_LocalScale = value ;

				if( transform is RectTransform rectTransform )
				{
					rectTransform.localScale = value ;
				}
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
		public void SetScale( float x, float y, float z )
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

		/// <summary>
		/// 基準スケールを更新する
		/// </summary>
		public void RefreshScale()
		{
			if( transform is RectTransform rectTransform )
			{
				m_LocalScale = rectTransform.localScale ;
			}
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
		/// アルファ値を設定する
		/// </summary>
		/// <param name="alpha"></param>
		public void SetAlpha( float alpha )
		{
			Alpha = alpha ;
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
					var canvasGroup = GetCanvasGroup() ;
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

				var canvasGroup = GetCanvasGroup() ;
				if( canvasGroup == null )
				{
					return ;
				}

				canvasGroup.alpha = value * ( m_Visible == true ? 1 : 0 ) ;

				if( canvasGroup.alpha <  m_DisableRaycastUnderAlpha )
				{
					canvasGroup.blocksRaycasts = false ;	// 無効
				}
				else
				{
					canvasGroup.blocksRaycasts = true ;	// 有効
				}
			}
		}


		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public virtual bool RaycastTarget
		{
			get
			{
				if( TryGetComponent<Graphic>( out var g ) == false )
				{
					return false ;
				}

				return g.raycastTarget ;
			}
			set
			{
				if( TryGetComponent<Graphic>( out var g ) == false )
				{
					return  ;
				}

				g.raycastTarget = value ;
			}
		}

		//----------------------------------------------------

		/// <summary>
		/// マテリアルタイプ
		/// </summary>
		public enum MaterialTypes
		{
			Default         = 0,
			Additional		= 1,
			Multiply        = 2,
			Grayscale		= 3,
			Sepia			= 4,
			Interpolation	= 5,
			Mosaic			= 6,
			Blur			= 7,
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
					var beforeMaterialType = m_MaterialType ;

					m_MaterialType  = value ;

					var graphic = GetGraphic() ;
	
					if( graphic != null )
					{
						graphic.material = null ;
						graphic.GraphicUpdateComplete() ;
					}

					if( m_ActiveMaterial != null )
					{
						bool isAsset =
						(
							beforeMaterialType == MaterialTypes.Default		||
							beforeMaterialType == MaterialTypes.Additional	||
							beforeMaterialType == MaterialTypes.Multiply	||
							beforeMaterialType == MaterialTypes.Grayscale
						) ;

						// マテリアルが直接アセットファイルのインスタンスを指している場合は破棄は実行しない
						if( isAsset == false )
						{
							if( Application.isPlaying == false )
							{
								DestroyImmediate( m_ActiveMaterial ) ;
							}
							else
							{
								Destroy( m_ActiveMaterial ) ;
							}
						}

						m_ActiveMaterial = null ;
					}

					if( m_MaterialType != MaterialTypes.Default )
					{
						m_ActiveMaterial = CreateCustomMaterial( m_MaterialType ) ;

						if( graphic != null && m_ActiveMaterial != null )
						{
							graphic.material = m_ActiveMaterial ;
							graphic.GraphicUpdateComplete() ;
						}

						if( m_MaterialType == MaterialTypes.Sepia )
						{
							ProcessSepia() ;
						}
						else
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
				}
			}
		}

		// カスタムマテリアルのインスタンスを取得する
		// 注意：Overlay バージョンのマテリアル＆シェーダーは、
		// 　　　Canvas の RenderMode を WorldSpace にして、Ｚソートを有効にした際に、
		// 　　　Ｚ値が同一で、後から描画されるＵＩがＺテストによって正しく描画されない問題を回避するためのものだと思われる。
		// 　　　※主にＶＲ系のＵＩを作る際に必要になったと朧気ながら記憶している。
		private Material CreateCustomMaterial( MaterialTypes materialType )
		{
			Material material = null ;

			string folderName = "Normal" ;
			string additionalName = string.Empty ;
			if( IsCanvasOverlay == true )
			{
				folderName     = "Overlay" ;
				additionalName = "Overlay-" ;
			}

			//-----------------------------------------------------------

			if( materialType == MaterialTypes.Additional )
			{
				// パラメータは無いためアセットファイル側のインスタンスを設定する(SharedMaterial)
				material = Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Additional" ) ;
			}
			else
			if( materialType == MaterialTypes.Multiply )
			{
				// パラメータは無いためアセットファイル側のインスタンスを設定する(SharedMaterial)
				material = Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Multiply" ) ;
			}
			else
			if( materialType == MaterialTypes.Grayscale )
			{
				// パラメータは無いためアセットファイル側のインスタンスを設定する(SharedMaterial)
				material = Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Grayscale" ) ;
			}
			else
			if( materialType == MaterialTypes.Sepia )
			{
				// パラメータが存在するため Instantiate が必要
				material = Instantiate( Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Sepia" ) ) ;
			}
			else
			if( materialType == MaterialTypes.Interpolation )
			{
				// パラメータが存在するため Instantiate が必要
				material = Instantiate( Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Interpolation" ) ) ;
			}
			else
			if( m_MaterialType == MaterialTypes.Mosaic )
			{
				// パラメータが存在するため Instantiate が必要
				material = Instantiate( Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Mosaic" ) ) ;
			}
			else
			if( materialType == MaterialTypes.Blur )
			{
				// パラメータが存在するため Instantiate が必要
				material = Instantiate( Resources.Load<Material>( $"uGUIHelper/Shaders/{folderName}/UI-{additionalName}Blur" ) ) ;
			}

			return material ;
		}

		// マテリアルはシリアライズ対象にならない
		private Material m_ActiveMaterial = null ;

		//-----------------------------------
		// セピア関係

		[SerializeField][HideInInspector]
		private float m_SepiaDark = 0.1f ;

		/// <summary>
		/// セピアの明暗度
		/// </summary>
		public  float  SepiaDark
		{
			get
			{
				return m_SepiaDark ;
			}
			set
			{
				m_SepiaDark = value ;
				ProcessSepia() ;
			}
		}


		[SerializeField][HideInInspector]
		private float m_SepiaStrength = 0.1f ;

		/// <summary>
		/// セピアの強度
		/// </summary>
		public  float  SepiaStrength
		{
			get
			{
				return m_SepiaStrength ;
			}
			set
			{
				m_SepiaStrength = value ;
				ProcessSepia() ;
			}
		}


		[SerializeField][HideInInspector]
		private float m_SepiaInterpolation = 1.0f ;

		/// <summary>
		/// マテリアルパラメータ値
		/// </summary>
		public  float  SepiaInterpolation
		{
			get
			{
				return m_SepiaInterpolation ;
			}
			set
			{
				m_SepiaInterpolation = value ;
				ProcessSepia() ;
			}
		}

		// セピアの反映
		private bool ProcessSepia()
		{
			var graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Sepia || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			graphic.materialForRendering.SetFloat( "_Dark",				m_SepiaDark ) ;
			graphic.materialForRendering.SetFloat( "_Strength",			m_SepiaStrength ) ;
			graphic.materialForRendering.SetFloat( "_Interpolation",	m_SepiaInterpolation ) ;

			return true ;
		}

		//-----------------------------------
		// インターポリューション関係

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

		// インターポレーションの反映
		private bool ProcessInterpolation()
		{
			var graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Interpolation || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			graphic.materialForRendering.SetFloat( "_InterpolationValue", m_InterpolationValue ) ;
			graphic.materialForRendering.SetColor( "_InterpolationColor", m_InterpolationColor ) ;

			return true ;
		}
		
		//-----------------------------------
		// モザイク関係

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
		/// モザイクのドットを正四角形にするかどうか
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

		// モザイク反映
		private bool ProcessMosaic()
		{
			var graphic = GetGraphic() ;

			if( m_MaterialType != MaterialTypes.Mosaic || graphic == null || m_ActiveMaterial == null )
			{
				return false ;
			}
			
			float intensity = 1.0f - m_MosaicIntensity ;
			intensity *= intensity ;

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

//			Debug.LogWarning( "モザイク:" + new Vector4( sw, sh, cw, ch ) ) ;
			graphic.materialForRendering.SetVector( "_Mosaic", new Vector4( sw, sh, cw, ch ) ) ;
//			graphic.materialForRendering.SetFloat( "_MosaicIntensity", m_MosaicIntensity ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// タイムスケール
		[SerializeField]
		protected float m_TimeScale = 1.0f ;

		/// <summary>
		/// タイムスケール
		/// </summary>
		public float TimeScale
		{
			get
			{
				return m_TimeScale ;
			}
			set
			{
				if( m_TimeScale != value )
				{
					m_TimeScale  = value ;

					// 注意 : １度でも Animator を使った事が無いと m_Animator にはインスタンスが記録されない(キャッシュ)
					if( m_Animator != null )
					{
						m_Animator.speed = m_TimeScale ;
					}

					OnTimeScaleChanged( m_TimeScale ) ;
				}
			}
		}

		/// <summary>
		/// タイムスケールが変更された際に呼び出される
		/// </summary>
		/// <param name="timeScale"></param>
		protected virtual void OnTimeScaleChanged( float timeScale ){}

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
		/// <param name="identity"></param>
		public UITween AddTween( string identity )
		{
			var tween = gameObject.AddComponent<UITween>() ;
			tween.Identity = identity ;

			return tween ;
		}
		
		/// <summary>
		/// Tween の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveTween( string identity, int instance = 0 )
		{
			var tweens = GetComponents<UITween>() ;
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
#if UNITY_EDITOR
				Debug.LogWarning( "[Tween] Not found this identity -> " + identity ) ;
#endif
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public UITween GetTween( string identity )
		{
			var tweens = gameObject.GetComponents<UITween>() ;
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

#if UNITY_EDITOR
			Debug.LogWarning( "[Tween] Not found this identity -> " + identity + " / "+ name ) ;
#endif
			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,UITween> GetAllTweens()
		{
			var tweens = gameObject.GetComponents<UITween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			var targets = new Dictionary<string, UITween>() ;

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
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool SetTweenTime( string identity, float delay = -1, float duration = -1 )
		{
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool PlayTweenDirect( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			var tween = GetTween( identity ) ;
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

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			return true ;
		}
		
		/// <summary>
		/// 非アクティブ状態の時のみ再生する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenIfHiding( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, true, false ) ;
		}

		/// <summary>
		/// 再生終了と同時に非アクティブ状態にする
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenAndHide( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, false, true ) ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayTween( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,UITween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0, bool ifHiding = false, bool autoHide = false )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				Debug.LogWarning( "Not found identity of tween : " + identity + " / " + Path ) ;
				return null ;
			}

			if( ifHiding == true && ( gameObject.activeSelf == true && m_Visible == true ) )
			{
				return new AsyncState( this ){ IsDone = true } ;
			}

			if( autoHide == true && gameObject.activeSelf == false )
			{
				return new AsyncState( this ){ IsDone = true } ;
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
				return new AsyncState( this ){ IsDone = true } ;
			}

			var state = new AsyncState( this ) ;
			StartCoroutine( PlayTweenAsync_Private( tween, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, autoHide, state ) ) ;
			return state ;
		}

		public IEnumerator PlayTweenAsync_Private( UITween tween, float delay, float duration, float offset, Action<string,UITween> onFinishedAction, float additionalDelay, float additionalDuration, bool autoHide, AsyncState state )
		{
			// 同じトゥイーンを多重実行出来ないようにする
			if( tween.IsRunning == true || tween.IsPlaying == true )
			{
//				tween.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( tween.IsRunning == true ) | ( tween.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			var destroyAtEnd = tween.DestroyAtEnd ;
			tween.DestroyAtEnd = false ;

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			yield return new WaitWhile( () => ( tween.IsRunning == true || tween.IsPlaying == true ) ) ;
			
			state.IsDone = true ;

			if( autoHide == true )
			{
				gameObject.SetActive( false ) ;
			}

			if( destroyAtEnd == true )
			{
				Destroy( tween.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Tween の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsTweenPlaying( string identity )
		{
			var tween = GetTween( identity ) ;
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
				var tweens = gameObject.GetComponents<UITween>() ;
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
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlayingInParents
		{
			get
			{
				if( IsAnyTweenPlaying == true )
				{
					return true ;
				}

				// 親も含めてトゥイーンが動作中か確認する
				var t = transform.parent ;
				while( t != null )
				{
					if( t.TryGetComponent<UIView>( out var view ) == true )
					{
						if( view.IsAnyTweenPlaying == true )
						{
							return true ;
						}
					}
					t = t.parent ;
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
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool UnpauseTween( string identity )
		{
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopTween( string identity )
		{
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopAndResetTween( string identity )
		{
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool FinishTween( string identity )
		{
			var tween = GetTween( identity ) ;
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
		public bool StopAllTweens()
		{
			var tweens = gameObject.GetComponents<UITween>() ;
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
		public bool StopAndResetAllTweens()
		{
			var tweens = gameObject.GetComponents<UITween>() ;
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
		public bool FinishAllTweens()
		{
			var tweens = gameObject.GetComponents<UITween>() ;
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public float GetTweenProcessTime( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return 0 ;
			}

			return tween.ProcessTime ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を設定する
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="time"></param>
		public bool SetTweenProcessTime( string identity, float time )
		{
			var tween = GetTween( identity ) ;
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
		/// <param name="identity"></param>
		public UIFlipper AddFlipper( string identity )
		{
			var flipper = gameObject.AddComponent<UIFlipper>() ;
			flipper.Identity = identity ;

			return flipper ;
		}
		
		/// <summary>
		/// Flipper の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveFlipper( string identity, int instance = 0 )
		{
			var flippers = GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return ;
			}

			int i, l = flippers.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && flippers[ i ].Identity == identity ) || ( instance != 0 && flippers[ i ].Identity == identity && flippers[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity + " / " + name ) ;
#endif
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
		/// <param name="identity"></param>
		/// <returns></returns>
		public UIFlipper GetFlipper( string identity )
		{
			var flippers = gameObject.GetComponents<UIFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return null ;
			}

			foreach( var flipper in flippers )
			{
				if( flipper.Identity == identity )
				{
					return flipper ;
				}
			}

#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity ) ;
#endif
			return null ;
		}

		/// <summary>
		/// Flipper の再生
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="timeScale"></param>
		/// <returns></returns>
		public bool PlayFlipperDirect( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1, Action<string,UIFlipper> onFinishedAction = null )
		{
			var flipper = GetFlipper( identity ) ;
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
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayFlipper( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1 )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				Debug.LogWarning( "Not found identity of flipper : " + identity ) ;
				return null ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return new AsyncState( this ){ IsDone = true } ;
			}

			var state = new AsyncState( this ) ;
			StartCoroutine( PlayFlipperAsync_Private( flipper, destroyAtEnd, speed, delay, state ) ) ;
			return state ;
		}

		public IEnumerator PlayFlipperAsync_Private( UIFlipper flipper, bool destroyAtEnd, float speed, float delay, AsyncState state )
		{
			// 同じフリッパーを多重実行出来ないようにする
			if( flipper.IsRunning == true || flipper.IsPlaying == true )
			{
//				flipper.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( flipper.IsRunning == true ) | ( flipper.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			flipper.Play( false, speed, delay ) ;

			yield return new WaitWhile( () => ( flipper.IsRunning == true || flipper.IsPlaying == true ) ) ;

			state.IsDone = true ;

			if( destroyAtEnd == true )
			{
				Destroy( flipper.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Flipper の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsFlipperPlaying( string identity )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
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
				var flippers = gameObject.GetComponents<UIFlipper>() ;
				if( flippers == null || flippers.Length == 0 )
				{
					return false ;
				}

				foreach( var flipper in flippers )
				{
					if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// Flipper の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopFlipper( string identity )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			flipper.Stop() ;
			return true ;
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
			return GetComponentInParent<Canvas>() ;
#if false

			int i, l = 64 ;

			Transform t = gameObject.transform ;
			for( i  =  0 ; i <  l ; i ++ )
			{
				if( t.gameObject.TryGetComponent<Canvas>( out var canvas ) == true )
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
#endif
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

			if( canvas.gameObject.TryGetComponent<CanvasScaler>( out var canvasScaler ) == false )
			{
				return Vector2.zero ;
			}

			return canvasScaler.referenceResolution ;
		}

		/// <summary>
		/// 親 Canvas の実仮想解像度を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCanvasSize( bool isReal = false )
		{
			Canvas canvas = GetParentCanvas() ;
			if( canvas == null )
			{
				return new Vector2( Screen.width, Screen.height ) ;
			}

			float sw = Screen.width ;
			float sh = Screen.height ;

			if( canvas.worldCamera != null && canvas.worldCamera.targetTexture != null )
			{
				sw = canvas.worldCamera.targetTexture.width ;
				sh = canvas.worldCamera.targetTexture.height ;
			}

			if( Application.isPlaying == false || isReal == true )
			{
				if( ( canvas.transform is RectTransform rt ) == false )
				{
					return  new Vector2( sw, sh ) ;
				}
	
				return rt.sizeDelta ;
			}

			if( canvas.TryGetComponent<CanvasScaler>( out var scaler ) == false )
			{
				return new Vector2( sw, sh ) ;
			}

			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize )
			{
				return new Vector2( sw / scaler.scaleFactor, sh / scaler.scaleFactor ) ;
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize )
			{
				float rw = scaler.referenceResolution.x ;
				float rh = scaler.referenceResolution.y ;

				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight )
				{
					float mf = scaler.matchWidthOrHeight ;

					float wa0 = sw / sh ;
					float wa1 = rw / rh ;
					float wa = Mathf.Lerp( wa0, wa1, mf ) ;

					float w  = rw * wa0 / wa ;

					float ha0 = rh / rw ;
					float ha1 = sw / sh ;
					float ha = Mathf.Lerp( ha0, ha1, mf ) ;

					float h  = rh * ha1 / ha ;

					return new Vector2( w, h ) ;
				}
				else
				if( scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand )
				{
					float w, h ;

					if( sw >= sh )
					{
						// 実スクリーンは横長
						float sa = sw / sh ;
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
						float sa = sh / sw ;
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

					if( sw >= sh )
					{
						// 実スクリーンは横長
						float sa = sw / sh ;
						float ra = rw / rh ;

						if( ra >= sa )
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * sw / sh ;
						}
						else
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * sh / sw ;
						}
					}
					else
					{
						// 実スクリーンは縦長
						float sa = sh / sw ;
						float ra = rh / rw ;

						if( ra >= sa )
						{
							// 仮想解像度の横をスクリーンのの横に合わせる
							w = rw ;
							h = rw * sh / sw ;
						}
						else
						{
							// 仮想解像度の縦をスクリーンのの横に合わせる
							h = rh ;
							w = rh * sw / sh ;
						}
					}

					return new Vector2( w, h ) ;
				}
			}
			else
			if( scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize )
			{
				if( ( canvas.transform is RectTransform rt ) == false )
				{
					return  new Vector2( sw, sh ) ;
				}
	
				return rt.sizeDelta ;
			}

			return new Vector2( sw, sh ) ;
		}

		/// <summary>
		/// 親キャンバスの基準となる長さを取得する
		/// </summary>
		/// <param name="ratio"></param>
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

				var t = gameObject.transform ;
				for( i  =  0 ; i <  l ; i ++ )
				{
					if( t.gameObject.TryGetComponent<UICanvas>( out canvas ) == true )
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

				return canvas.IsOverlay ;
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
			return AddView<T>( string.Empty ) ;
		}

		/// <summary>
		/// View を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="viewName"></param>
		/// <param name="option"></param>
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
					viewName = viewName[ i.. ] ;
				}

				i = viewName.IndexOf( "UI" ) ;
				if( i >= 0 )
				{
					viewName = viewName[ ( i + 2 ).. ] ;
				}
			}
		
			var go = new GameObject( viewName, typeof( RectTransform ) ) ;
		
			if( go == null )
			{
				// 失敗
				return default ;
			}
		
			// 最初に親を設定してしまう
			go.transform.SetParent( gameObject.transform, false ) ;
		
			// コンポーネントをアタッチする
			var component = go.AddComponent<T>() ;
		
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
		/// <param name="viewName"></param>
		/// <param name="t"></param>
		/// <param name="layer"></param>
		/// <returns></returns>
		public GameObject AddObject( string viewName, Transform t = null, int layer = -1 )
		{
			var go = new GameObject( viewName ) ;
			go.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
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
			var go = new GameObject( viewName ) ;
			go.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
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
		/// <param name="path"></param>
		/// <returns></returns>
		public GameObject AddPrefab( string path, Transform t = null, int layer = -1 )
		{
			var go = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
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
		/// <param name="prefab"></param>
		/// <param name="transform"></param>
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

			var go = ( GameObject )GameObject.Instantiate( prefab ) ;
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
		/// <param name="path"></param>
		/// <returns></returns>
		public T AddPrefab<T>( string path, Transform t = null, int layer = -1 ) where T : UnityEngine.Component
		{
			var prefab = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
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
		/// <param name="prefab"></param>
		/// <param name="parentName"></param>
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
		/// <param name="viewName"></param>
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
		/// <param name="prefab"></param>
		/// <param name="t"></param>
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

			var go = ( GameObject )GameObject.Instantiate( prefab ) ;
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

			T component = go.GetComponentInChildren<T>( true ) ;

			return component ;
		}

		/// <summary>
		/// 指定したゲームオブジェクトを自身の子にする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="go"></param>
		/// <param name="parent"></param>
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
		/// <param name="go"></param>
		/// <param name="layer"></param>
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
		/// <param name="color"></param>
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
		public virtual Camera GetCamera()
		{
			if( m_Camera == null )
			{
				 TryGetComponent<Camera>( out m_Camera ) ;
			}
			return m_Camera ;
		}

		//----------

		/// <summary>
		/// RectTransform(ショートカット)
		/// </summary>
		public virtual RectTransform GetRectTransform()
		{
			if( transform is RectTransform rectTransform )
			{
				return rectTransform ;
			}

			// RectTransform ではない
			return null ;
		}
	
		//----------

		// キャッシュ
		private Canvas m_Canvas = null ;

		/// <summary>
		/// Image(ショートカット)
		/// </summary>
		public virtual Canvas CCanvas
		{
			get
			{
				if( m_Canvas == null )
				{
					TryGetComponent<Canvas>( out m_Canvas ) ;
				}
				return m_Canvas ;
			}
		}

		/// <summary>
		/// Canvas(ショートカット)
		/// </summary>
		public virtual Canvas GetCanvas()
		{
			if( m_Canvas == null )
			{
				 TryGetComponent<Canvas>( out m_Canvas ) ;
			}
			return m_Canvas ;
		}
	
		//----------

		// キャッシュ
		private CanvasRenderer m_CanvasRenderer = null ;

		/// <summary>
		/// CanvasRenderer(ショートカット)
		/// </summary>
		public virtual CanvasRenderer GetCanvasRenderer()
		{
			if( m_CanvasRenderer == null )
			{
				TryGetComponent<CanvasRenderer>( out m_CanvasRenderer ) ;
			}
			return m_CanvasRenderer ;
		}

		/// <summary>
		/// CanvasRenderer の有無
		/// </summary>
		public bool IsCanvasRenderer
		{
			get
			{
				return ( GetCanvasRenderer() != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddCanvasRenderer() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveCanvasRenderer() ;
					}
					else
					{
						m_RemoveCanvasRenderer = true ;
					}
#else
					RemoveCanvasRenderer() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// CanvasRenderer の追加
		/// </summary>
		public void AddCanvasRenderer()
		{
			if( GetCanvasRenderer() != null )
			{
				return ;
			}
		
			m_CanvasRenderer = gameObject.AddComponent<CanvasRenderer>() ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveCanvasRenderer = false ;
#endif

		/// <summary>
		/// CanvasRenderer の削除
		/// </summary>
		public void RemoveCanvasRenderer()
		{
			var canvasRenderer = GetCanvasRenderer() ;
			if( canvasRenderer == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( canvasRenderer ) ;
			}
			else
			{
				Destroy( canvasRenderer ) ;
			}

			m_CanvasRenderer = null ;
		}

		//---------------

		// キャッシュ
		private GraphicEmpty m_GraphicEmpty = null ;

		/// <summary>
		/// GraphicEmpty(ショートカット)
		/// </summary>
		public virtual GraphicEmpty GetGraphicEmpty()
		{
			if( m_GraphicEmpty == null )
			{
				TryGetComponent<GraphicEmpty>( out m_GraphicEmpty ) ;
			}
			return m_GraphicEmpty ;
		}

		/// <summary>
		/// GraphicEmpty の有無
		/// </summary>
		public bool IsGraphicEmpty
		{
			get
			{
				return ( GetGraphicEmpty() != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddGraphicEmpty() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveGraphicEmpty() ;
					}
					else
					{
						m_RemoveGraphicEmpty = true ;
					}
#else
					RemoveGraphicEmpty() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// GraphicEmpty の追加
		/// </summary>
		public void AddGraphicEmpty()
		{
			if( GetGraphicEmpty() != null )
			{
				return ;
			}
		
			m_GraphicEmpty = gameObject.AddComponent<GraphicEmpty>() ;
			m_GraphicEmpty.raycastTarget = true ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveGraphicEmpty = false ;
#endif

		/// <summary>
		/// GraphicEmpty の削除
		/// </summary>
		public void RemoveGraphicEmpty()
		{
			var graphicEmpty = GetGraphicEmpty() ;
			if( graphicEmpty == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( graphicEmpty ) ;
			}
			else
			{
				Destroy( graphicEmpty ) ;
			}

			m_GraphicEmpty = null ;
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
				TryGetComponent<CanvasScaler>( out m_CanvasScaler ) ;
			}
			return m_CanvasScaler ;
		}

		//----------

		// キャッシュ
		private CanvasGroup m_CanvasGroup = null ;

		/// <summary>
		/// CanvasGroup(ショートカット)
		/// </summary>
		public virtual CanvasGroup GetCanvasGroup()
		{
			if( m_CanvasGroup == null )
			{
				TryGetComponent<CanvasGroup>( out m_CanvasGroup ) ;
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
				return ( GetCanvasGroup() != null ) ;
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
			var canvasGroup = GetCanvasGroup() ;
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
		public virtual GraphicRaycasterWrapper GetGraphicRaycaster()
		{
			if( m_GraphicRaycaster == null )
			{
				 TryGetComponent<GraphicRaycasterWrapper>( out m_GraphicRaycaster ) ;
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
				 TryGetComponent<Graphic>( out m_Graphic ) ;
			}
			return m_Graphic ;
		}

		//----------

		// キャッシュ	
		protected EventTrigger m_EventTrigger = null ;

		/// <summary>
		/// EventTrigger(ショートカット)
		/// </summary>
		public virtual EventTrigger CEventTrigger
		{
			get
			{
				if( m_EventTrigger == null )
				{
					 TryGetComponent<EventTrigger>( out m_EventTrigger ) ;
				}
				return m_EventTrigger ;
			}
		}

		/// <summary>
		/// EventTrigger の有無
		/// </summary>
		public bool IsEventTrigger
		{
			get
			{
				return ( CEventTrigger != null ) ;
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
			if( CEventTrigger != null )
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
			var eventTrigger = CEventTrigger ;
			if( eventTrigger == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( eventTrigger ) ;
			}
			else
			{
				Destroy( eventTrigger ) ;
			}

			m_EventTrigger = null ;
		}

		//----------

		// キャッシュ
		protected UIInteraction m_Interaction = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		public virtual UIInteraction CInteraction
		{
			get
			{
				if( m_Interaction == null )
				{
					TryGetComponent<UIInteraction>( out m_Interaction ) ;
				}
				return m_Interaction ;
			}
		}
		
		/// <summary>
		/// Interaction の有無
		/// </summary>
		public bool IsInteraction
		{
			get
			{
				return ( CInteraction != null ) ;
			}
			set
			{
				if( value == true )
				{
					if( IsInteractionForScrollView == true )
					{
						// スクロールビュー用のインタラクションが既に付いていたら通常のインタラクションは無効
						return ;
					}

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
			if( CInteraction != null )
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
			var interaction = CInteraction ;
			if( interaction == null )
			{
				return ;
			}
		
//			RemoveInteractionCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( interaction ) ;
			}
			else
			{
				Destroy( interaction ) ;
			}

			m_Interaction = null ;

			m_HoverAtFirst = false ;	// 消しておかないと Hover で悪さする
		}

		//----------

		// キャッシュ
		protected UIInteractionForScrollView m_InteractionForScrollView = null ;

		/// <summary>
		/// Interaction(ショートカット)
		/// </summary>
		public virtual UIInteractionForScrollView CInteractionForScrollView
		{
			get
			{
				if( m_InteractionForScrollView == null )
				{
					TryGetComponent<UIInteractionForScrollView>( out m_InteractionForScrollView ) ;
				}
				return m_InteractionForScrollView ;
			}
		}
		
		/// <summary>
		/// InteractionWithoutDrag の有無
		/// </summary>
		public bool IsInteractionForScrollView
		{
			get
			{
				return ( CInteractionForScrollView != null ) ;
			}
			set
			{
				if( value == true )
				{
					if( IsInteraction == true )
					{
						// 通常のインタラクションが付いていたら削除する
						IsInteraction  = false ; 
					}

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
			if( CInteractionForScrollView != null )
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
			var interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return ;
			}
		
			RemoveInteractionForScrollViewCallback() ;

			if( Application.isPlaying == false )
			{
				DestroyImmediate( interactionForScrollView ) ;
			}
			else
			{
				Destroy( interactionForScrollView ) ;
			}

			m_InteractionForScrollView = null ;

			m_HoverAtFirst = false ;	// 消しておかないと Hover で悪さする
		}

		//----------

		// キャッシュ
		protected UITransition m_Transition = null ;

		/// <summary>
		/// Transition(ショートカット)
		/// </summary>
		public virtual UITransition CTransition
		{
			get
			{
				if( m_Transition == null )
				{
					 TryGetComponent<UITransition>( out m_Transition ) ;
				}
				return m_Transition ;
			}
		}
		
		/// <summary>
		/// Transition の有無
		/// </summary>
		public bool IsTransition
		{
			get
			{
				return ( CTransition != null ) ;
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
			if( CTransition != null )
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
			var transition = CTransition ;
			if( transition == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( transition ) ;
			}
			else
			{
				Destroy( transition ) ;
			}

			m_Transition = null ;
		}

		//----------
		
		// キャッシュ
		private RawImage m_RawImage = null ;

		/// <summary>
		/// RawImage(ショートカット)
		/// </summary>
		public virtual RawImage CRawImage
		{
			get
			{
				if( m_RawImage == null )
				{
					TryGetComponent<RawImage>( out m_RawImage ) ;
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
		public virtual Image CImage
		{
			get
			{
				if( m_Image == null )
				{
					TryGetComponent<Image>( out m_Image ) ;
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
		public virtual Button CButton
		{
			get
			{
				if( m_Button == null )
				{
					TryGetComponent<Button>( out m_Button ) ;
				}
				return m_Button ;
			}
		}

		//----------

		// キャッシュ
		private UIButtonGroup m_ButtonGroup = null ;

		/// <summary>
		/// ButtonGroup(ショートカット)
		/// </summary>
		public virtual UIButtonGroup CButtonGroup
		{
			get
			{
				if( m_ButtonGroup == null )
				{
					TryGetComponent<UIButtonGroup>( out m_ButtonGroup ) ;
				}
				return m_ButtonGroup ;
			}
		}
		
		/// <summary>
		/// ButtonGroup の有無
		/// </summary>
		public bool IsButtonGroup
		{
			get
			{
				return ( CButtonGroup != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddButtonGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveButtonGroup() ;
					}
					else
					{
						m_RemoveButtonGroup = true ;
					}
#else
					RemoveButtonGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// ButtonGroup の追加
		/// </summary>
		public void AddButtonGroup()
		{
			if( CButtonGroup != null )
			{
				return ;
			}

			UIButtonGroup buttonGroup ;
		
			buttonGroup = gameObject.AddComponent<UIButtonGroup>() ;
			buttonGroup.AllowSwitchOff = false ;
		}

#if UNITY_EDITOR
		private bool m_RemoveButtonGroup = false ;
#endif

		/// <summary>
		/// ButtonGroup の削除
		/// </summary>
		public void RemoveButtonGroup()
		{
			var buttonGroup = CButtonGroup ;
			if( buttonGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( buttonGroup ) ;
			}
			else
			{
				Destroy( buttonGroup ) ;
			}

			m_ButtonGroup = null ;
		}

		//----------

		// キャッシュ
		private Toggle m_Toggle = null ;

		/// <summary>
		/// Toggle(ショートカット)
		/// </summary>
		public virtual Toggle CToggle
		{
			get
			{
				if( m_Toggle == null )
				{
					TryGetComponent<Toggle>( out m_Toggle ) ;
				}
				return m_Toggle ;
			}
		}

		//----------

		// キャッシュ
		private ToggleGroup m_ToggleGroup = null ;
		private UIToggleGroup m_UIToggleGroup = null ;

		/// <summary>
		/// ToggleGroup(ショートカット)
		/// </summary>
		public virtual ToggleGroup CToggleGroup
		{
			get
			{
				if( m_ToggleGroup == null )
				{
					TryGetComponent<ToggleGroup>( out m_ToggleGroup ) ;
				}
				return m_ToggleGroup ;
			}
		}
		
		/// <summary>
		/// ToggleGroup の有無
		/// </summary>
		public bool IsToggleGroup
		{
			get
			{
				return ( CToggleGroup != null ) ;
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
			if( CToggleGroup != null )
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
			var toggleGroup = CToggleGroup ;
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
				if( CToggleGroup == null )
				{
					return null ;
				}

				m_UIToggleGroup = new UIToggleGroup( CToggleGroup ) ;
			}

			return m_UIToggleGroup ;
		}

		//----------
		
		// キャッシュ
		private Slider m_Slider = null ;

		/// <summary>
		/// Slider(ショートカット)
		/// </summary>
		public virtual Slider CSlider
		{
			get
			{
				if( m_Slider == null )
				{
					TryGetComponent<Slider>( out m_Slider ) ;
				}
				return m_Slider ;
			}
		}
	
		//----------

		// キャッシュ
		private ScrollRectWrapper m_ScrollRect = null ;

		/// <summary>
		/// ScrollRect(ショートカット)
		/// </summary>
		public virtual ScrollRectWrapper CScrollRect
		{
			get
			{
				if( m_ScrollRect == null )
				{
					TryGetComponent<ScrollRectWrapper>( out m_ScrollRect ) ;
				}
				return m_ScrollRect ;
			}
		}

		//----------
		
		// キャッシュ
		private ScrollbarWrapper m_Scrollbar = null ;

		/// <summary>
		/// Scrollbar(ショートカット)
		/// </summary>
		public virtual Scrollbar CScrollbar
		{
			get
			{
				if( m_Scrollbar == null )
				{
					TryGetComponent<ScrollbarWrapper>( out m_Scrollbar ) ;
				}
				return m_Scrollbar ;
			}
		}
	
		//----------

		// キャッシュ
		private Dropdown m_Dropdown ;

		/// <summary>
		/// Dropdown(ショートカット)
		/// </summary>
		public virtual Dropdown CDropdown
		{
			get
			{
				if( m_Dropdown == null )
				{
					TryGetComponent<Dropdown>( out m_Dropdown ) ;
				}
				return m_Dropdown ;
			}
		}

		//----------

		// キャッシュ
		private TMP_Dropdown m_TMP_Dropdown ;

		/// <summary>
		/// TMP_Dropdown(ショートカット)
		/// </summary>
		public virtual TMP_Dropdown CTMP_Dropdown
		{
			get
			{
				if( m_TMP_Dropdown == null )
				{
					TryGetComponent<TMP_Dropdown>( out m_TMP_Dropdown ) ;
				}
				return m_TMP_Dropdown ;
			}
		}

		//----------
		
		// キャッシュ
		private Text m_Text = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		public virtual Text CText
		{
			get
			{
				if( m_Text == null )
				{
					TryGetComponent<Text>( out m_Text ) ;
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
		public virtual RichText CRichText
		{
			get
			{
				if( m_RichText == null )
				{
					 TryGetComponent<RichText>( out m_RichText ) ;
				}
				return m_RichText ;
			}
		}

		//----------

		// キャッシュ
		private TextMeshProUGUI m_TextMesh = null ;

		/// <summary>
		/// Text(ショートカット)
		/// </summary>
		public virtual TextMeshProUGUI CTextMesh
		{
			get
			{
				if( m_TextMesh == null )
				{
					TryGetComponent<TextMeshProUGUI>( out m_TextMesh ) ;
				}
				return m_TextMesh ;
			}
		}

		//----------

		// キャッシュ
		private HorizontalLayoutGroup m_HorizontalLayoutGroup = null ;

		/// <summary>
		/// HorizontalLayoutGroup(ショートカット)
		/// </summary>
		public virtual HorizontalLayoutGroup CHorizontalLayoutGroup
		{
			get
			{
				if( m_HorizontalLayoutGroup == null )
				{
					 TryGetComponent<HorizontalLayoutGroup>( out m_HorizontalLayoutGroup ) ;
				}
				return m_HorizontalLayoutGroup ;
			}
		}
		
		/// <summary>
		/// HorizontalLayoutGroup の有無
		/// </summary>
		public bool IsHorizontalLayoutGroup
		{
			get
			{
				return ( CHorizontalLayoutGroup != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddHorizontalLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveHorizontalLayoutGroup() ;
					}
					else
					{
						m_RemoveHorizontalLayoutGroup = true ;
					}
#else
					RemoveHorizontalLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// HorizontalLayoutGroup の追加
		/// </summary>
		public void AddHorizontalLayoutGroup()
		{
			if( CHorizontalLayoutGroup != null )
			{
				return ;
			}
		
			HorizontalLayoutGroup horizontalLayoutGroup ;
		
			horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>() ;
			horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
			horizontalLayoutGroup.childControlWidth			= false ;
			horizontalLayoutGroup.childControlHeight		= false ;
			horizontalLayoutGroup.childScaleWidth			= false ;
			horizontalLayoutGroup.childScaleHeight			= false ;
			horizontalLayoutGroup.childForceExpandWidth		= false ;
			horizontalLayoutGroup.childForceExpandHeight	= false ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveHorizontalLayoutGroup = false ;
#endif

		/// <summary>
		/// HorizontalLayoutGroup の削除
		/// </summary>
		public void RemoveHorizontalLayoutGroup()
		{
			var horizontalLayoutGroup = CHorizontalLayoutGroup ;
			if( horizontalLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( horizontalLayoutGroup ) ;
			}
			else
			{
				Destroy( horizontalLayoutGroup ) ;
			}

			m_HorizontalLayoutGroup = null ;
		}
		
		//----------

		// キャッシュ
		private VerticalLayoutGroup m_VerticalLayoutGroup = null ;

		/// <summary>
		/// VerticalLayoutGroup(ショートカット)
		/// </summary>
		public virtual VerticalLayoutGroup CVerticalLayoutGroup
		{
			get
			{
				if( m_VerticalLayoutGroup == null )
				{
					TryGetComponent<VerticalLayoutGroup>( out m_VerticalLayoutGroup ) ;
				}
				return m_VerticalLayoutGroup ;
			}
		}
		
		/// <summary>
		/// VerticalLayoutGroup の有無
		/// </summary>
		public bool IsVerticalLayoutGroup
		{
			get
			{
				return ( CVerticalLayoutGroup != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddVerticalLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveVerticalLayoutGroup() ;
					}
					else
					{
						m_RemoveVerticalLayoutGroup = true ;
					}
#else
					RemoveVerticalLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// VerticalLayoutGroup の追加
		/// </summary>
		public void AddVerticalLayoutGroup()
		{
			if( CVerticalLayoutGroup != null )
			{
				return ;
			}
		
			VerticalLayoutGroup verticalLayoutGroup ;
		
			verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>() ;
			verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
			verticalLayoutGroup.childControlWidth		= false ;
			verticalLayoutGroup.childControlHeight		= false ;
			verticalLayoutGroup.childScaleWidth			= false ;
			verticalLayoutGroup.childScaleHeight		= false ;
			verticalLayoutGroup.childForceExpandWidth	= false ;
			verticalLayoutGroup.childForceExpandHeight	= false ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveVerticalLayoutGroup = false ;
#endif

		/// <summary>
		/// VerticalLayoutGroup の削除
		/// </summary>
		public void RemoveVerticalLayoutGroup()
		{
			var verticalLayoutGroup = CVerticalLayoutGroup ;
			if( verticalLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( verticalLayoutGroup ) ;
			}
			else
			{
				Destroy( verticalLayoutGroup ) ;
			}

			m_VerticalLayoutGroup = null ;
		}

		//----------

		// キャッシュ
		private GridLayoutGroup m_GridLayoutGroup = null ;

		/// <summary>
		/// GridLayoutGroup(ショートカット)
		/// </summary>
		public virtual GridLayoutGroup CGridLayoutGroup
		{
			get
			{
				if( m_GridLayoutGroup == null )
				{
					 TryGetComponent<GridLayoutGroup>( out m_GridLayoutGroup ) ;
				}
				return m_GridLayoutGroup ;
			}
		}
		
		/// <summary>
		/// GridLayoutGroup の有無
		/// </summary>
		public bool IsGridLayoutGroup
		{
			get
			{
				return ( CGridLayoutGroup != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddGridLayoutGroup() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveGridLayoutGroup() ;
					}
					else
					{
						m_RemoveGridLayoutGroup = true ;
					}
#else
					RemoveGridLayoutGroup() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// GridLayoutGroup の追加
		/// </summary>
		public void AddGridLayoutGroup()
		{
			if( CGridLayoutGroup != null )
			{
				return ;
			}
		
			GridLayoutGroup gridLayoutGroup ;
		
			gridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>() ;
			gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveGridLayoutGroup = false ;
#endif

		/// <summary>
		/// GridLayoutGroup の削除
		/// </summary>
		public void RemoveGridLayoutGroup()
		{
			var gridLayoutGroup = CGridLayoutGroup ;
			if( gridLayoutGroup == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gridLayoutGroup ) ;
			}
			else
			{
				Destroy( gridLayoutGroup ) ;
			}

			m_GridLayoutGroup = null ;
		}

		//----------

		// キャッシュ
		private ContentSizeFitter m_ContentSizeFitter = null ;

		/// <summary>
		/// ContentSizeFitter(ショートカット)
		/// </summary>
		public virtual ContentSizeFitter CContentSizeFitter
		{
			get
			{
				if( m_ContentSizeFitter == null )
				{
					TryGetComponent<ContentSizeFitter>( out m_ContentSizeFitter ) ;
				}
				return m_ContentSizeFitter ;
			}
		}
		
		/// <summary>
		/// ContentSizeFitter の有無
		/// </summary>
		public bool IsContentSizeFitter
		{
			get
			{
				return ( CContentSizeFitter != null ) ;
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
			if( CContentSizeFitter != null )
			{
				return ;
			}
		
			ContentSizeFitter contentSizeFitter ;
		
			contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>() ;
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize ;
			contentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveContentSizeFitter = false ;
#endif

		/// <summary>
		/// ContentSizeFitter の削除
		/// </summary>
		public void RemoveContentSizeFitter()
		{
			var contentSizeFitter = CContentSizeFitter ;
			if( contentSizeFitter == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( contentSizeFitter ) ;
			}
			else
			{
				Destroy( contentSizeFitter ) ;
			}

			m_ContentSizeFitter = null ;
		}
		
		//----------
		
		// キャッシュ
		private LayoutElement m_LayoutElement = null ;

		/// <summary>
		/// LayoutElement(ショートカット)
		/// </summary>
		public virtual LayoutElement CLayoutElement
		{
			get
			{
				if( m_LayoutElement == null )
				{
					 TryGetComponent<LayoutElement>( out m_LayoutElement ) ;
				}
				return m_LayoutElement ;
			}
		}

		/// <summary>
		/// LayoutElement の有無
		/// </summary>
		public bool IsLayoutElement
		{
			get
			{
				return ( CLayoutElement != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddLayoutElement() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveLayoutElement() ;
					}
					else
					{
						m_RemoveLayoutElement = true ;
					}
#else
					RemoveLayoutElement() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// LayoutElement の追加
		/// </summary>
		public void AddLayoutElement()
		{
			if( CLayoutElement != null )
			{
				return ;
			}
		
			LayoutElement layoutElement ;
		
			layoutElement = gameObject.AddComponent<LayoutElement>() ;
			layoutElement.ignoreLayout		= false ;
			layoutElement.minWidth			= -1 ;
			layoutElement.minHeight			= -1 ;
			layoutElement.preferredWidth	= Width ;
			layoutElement.preferredHeight	= Height ;
			layoutElement.flexibleWidth		= -1 ;
			layoutElement.flexibleHeight	= -1 ;
			layoutElement.layoutPriority	=  1 ;
		}
		
#if UNITY_EDITOR
		private bool m_RemoveLayoutElement = false ;
#endif

		/// <summary>
		/// LayoutElement の削除
		/// </summary>
		public void RemoveLayoutElement()
		{
			var layoutElement = CLayoutElement ;
			if( layoutElement == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( layoutElement ) ;
			}
			else
			{
				Destroy( layoutElement ) ;
			}

			m_LayoutElement = null ;
		}
		
		//----------

		// キャッシュ
		private Mask m_Mask = null ;

		/// <summary>
		/// Mask(ショートカット)
		/// </summary>
		public virtual Mask CMask
		{
			get
			{
				if( m_Mask == null )
				{
					 TryGetComponent<Mask>( out m_Mask ) ;
				}
				return m_Mask ;
			}
		}
		
		/// <summary>
		/// Mask の有無
		/// </summary>
		public bool IsMask
		{
			get
			{
				return ( CMask != null ) ;
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
			if( CMask != null )
			{
				return ;
			}
		
			Mask mask ;
		
			mask = gameObject.AddComponent<Mask>() ;
			mask.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveMask = false ;
#endif

		/// <summary>
		/// Mask の削除
		/// </summary>
		public void RemoveMask()
		{
			var mask = CMask ;
			if( mask == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( mask ) ;
			}
			else
			{
				Destroy( mask ) ;
			}
		}

		//----------

		// キャッシュ
		private RectMask2D m_RectMask2D = null ;

		/// <summary>
		/// RectMask2D(ショートカット)
		/// </summary>
		public virtual RectMask2D CRectMask2D
		{
			get
			{
				if( m_RectMask2D == null )
				{
					TryGetComponent<RectMask2D>( out m_RectMask2D ) ;
				}
				return m_RectMask2D ;
			}
		}
		
		/// <summary>
		/// RectMask2D の有無
		/// </summary>
		public bool IsRectMask2D
		{
			get
			{
				return ( CRectMask2D != null ) ;
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
			if( CRectMask2D != null )
			{
				return ;
			}

			gameObject.AddComponent<RectMask2D>() ;

//			RectMask2D rectMask2D ;
		
//			rectMask2D = gameObject.AddComponent<RectMask2D>() ;
//			rectMask2D.showMaskGraphic = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveRectMask2D = false ;
#endif

		/// <summary>
		/// RectMask2D の削除
		/// </summary>
		public void RemoveRectMask2D()
		{
			var rectMask2D = CRectMask2D ;
			if( rectMask2D == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( rectMask2D ) ;
			}
			else
			{
				Destroy( rectMask2D ) ;
			}
		}

		//----------

		// キャッシュ
		private UIAlphaMaskWindow m_AlphaMaskWindow = null ;

		/// <summary>
		/// AlphaMaskWindow(ショートカット)
		/// </summary>
		public virtual UIAlphaMaskWindow CAlphaMaskWindow
		{
			get
			{
				if( m_AlphaMaskWindow == null )
				{
					TryGetComponent<UIAlphaMaskWindow>( out m_AlphaMaskWindow ) ;
				}
				return m_AlphaMaskWindow ;
			}
		}
		
		/// <summary>
		/// AlphaMaskWindow の有無
		/// </summary>
		public bool IsAlphaMaskWindow
		{
			get
			{
				return ( CAlphaMaskWindow != null ) ;
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
			if( CAlphaMaskWindow != null )
			{
				return ;
			}
		
//			UIAlphaMaskWindow alphaMaskWindow ;
		
//			alphaMaskWindow = gameObject.AddComponent<UIAlphaMaskWindow>() ;
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
			var alphaMaskWindow = CAlphaMaskWindow ;
			if( alphaMaskWindow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( alphaMaskWindow ) ;
			}
			else
			{
				Destroy( alphaMaskWindow ) ;
			}
		}

		//----------

		// キャッシュ
		private UIAlphaMaskTarget m_AlphaMaskTarget = null ;

		/// <summary>
		/// AlphaMaskTarget(ショートカット)
		/// </summary>
		public virtual UIAlphaMaskTarget CAlphaMaskTarget
		{
			get
			{
				if( m_AlphaMaskTarget == null )
				{
					TryGetComponent<UIAlphaMaskTarget>( out m_AlphaMaskTarget ) ;
				}
				return m_AlphaMaskTarget ;
			}
		}
		
		/// <summary>
		/// AlphaMaskTarget の有無
		/// </summary>
		public bool IsAlphaMaskTarget
		{
			get
			{
				return ( CAlphaMaskTarget != null ) ;
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
			if( CAlphaMaskTarget != null )
			{
				return ;
			}
		
			gameObject.AddComponent<UIAlphaMaskTarget>() ;
//			var alphaMaskTarget = gameObject.AddComponent<UIAlphaMaskTarget>() ;
//			alphaMaskTarget.RefreshAlphaMask() ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAlphaMaskTarget = false ;
#endif

		/// <summary>
		/// AlphaMaskTarget の削除
		/// </summary>
		public void RemoveAlphaMaskTarget()
		{
			var alphaMaskTarget = CAlphaMaskTarget ;
			if( alphaMaskTarget == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( alphaMaskTarget ) ;
			}
			else
			{
				Destroy( alphaMaskTarget ) ;
			}
		}

		//----------

		/// <summary>
		/// グラフィックコンポーネントがアタッチされているかどうか
		/// </summary>
		public bool IsGraphic
		{
			get
			{
				return TryGetComponent<Graphic>( out _ ) ;
			}
		}

		//----------
		
		// キャッシュ
		private InputFieldPlus m_InputField = null ;

		/// <summary>
		/// InputField(ショートカット)
		/// </summary>
		public virtual InputFieldPlus CInputField
		{
			get
			{
				if( m_InputField == null )
				{
					TryGetComponent<InputFieldPlus>( out m_InputField ) ;
				}
				return m_InputField ;
			}
		}
		
		//----------
		
		// キャッシュ
		private TMP_InputFieldPlus m_TMP_InputField = null ;

		/// <summary>
		/// InputField(ショートカット)
		/// </summary>
		public virtual TMP_InputFieldPlus CTMP_InputField
		{
			get
			{
				if( m_TMP_InputField == null )
				{
					TryGetComponent<TMP_InputFieldPlus>( out m_TMP_InputField ) ;
				}
				return m_TMP_InputField ;
			}
		}
	
		//----------
		
		// キャッシュ
		private Shadow m_Shadow = null ;

		/// <summary>
		/// Shadow(ショートカット)
		/// </summary>
		public virtual Shadow CShadow
		{
			get
			{
				if( m_Shadow == null )
				{
					TryGetComponent<Shadow>( out m_Shadow ) ;
				}
				return m_Shadow ;
			}
		}
		
		/// <summary>
		/// Shadow の有無
		/// </summary>
		public bool IsShadow
		{
			get
			{
				Shadow shadow = CShadow ;
				if( shadow != null == true && shadow is Outline == false )
				{
					return true ;
				}
				else
				{
					return false ;
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
			if( CShadow != null )
			{
				return ;
			}
		
			Shadow shadow ;
		
			shadow = gameObject.AddComponent<Shadow>() ;
			shadow.effectColor = ARGB( 0xFF000000 ) ;
			shadow.effectDistance = new Vector2(  1, -1 ) ;
			shadow.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveShadow = false ;
#endif

		/// <summary>
		/// Shadow の削除
		/// </summary>
		public void RemoveShadow()
		{
			var shadow = CShadow ;
			if( shadow == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( shadow ) ;
			}
			else
			{
				Destroy( shadow ) ;
			}

			m_Shadow = null ;
		}
		
		//----------
		
		// キャッシュ
		private Outline m_Outline = null ;

		/// <summary>
		/// Outline(ショートカット)
		/// </summary>
		public virtual Outline COutline
		{
			get
			{
				if( m_Outline == null )
				{
					TryGetComponent<Outline>( out m_Outline ) ;
				}
				return m_Outline ;
			}
		}

		/// <summary>
		/// Outline の有無
		/// </summary>
		public bool IsOutline
		{
			get
			{
				return ( COutline != null ) ;
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
			if( COutline != null )
			{
				return ;
			}
		
			Outline outline ;
		
			outline = gameObject.AddComponent<Outline>() ;
			outline.effectColor = ARGB( 0xFF000000 ) ;
			outline.effectDistance = new Vector2(  1, -1 ) ;
			outline.useGraphicAlpha = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveOutline = false ;
#endif

		/// <summary>
		/// Outline の削除
		/// </summary>
		public void RemoveOutline()
		{
			var outline = COutline ;
			if( outline == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( outline ) ;
			}
			else
			{
				Destroy( outline ) ;
			}

			m_Outline = null ;
		}

		//----------
		
		// キャッシュ
		private UIGradient m_Gradient = null ;

		/// <summary>
		/// Gradient(ショートカット)
		/// </summary>
		public virtual UIGradient CGradient
		{
			get
			{
				if( m_Gradient == null )
				{
					TryGetComponent<UIGradient>( out m_Gradient ) ;
				}
				return m_Gradient ;
			}
		}
		
		/// <summary>
		/// Gradient の有無
		/// </summary>
		public bool IsGradient
		{
			get
			{
				return ( CGradient != null ) ;
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
			if( CGradient != null )
			{
				return ;
			}
		
			UIGradient gradient ;
		
			gradient = gameObject.AddComponent<UIGradient>() ;
			gradient.DirectionType = UIGradient.DirectionTypes.Vertical ;
			gradient.Top    = ARGB( 0xFFFFFFFF ) ;
			gradient.Bottom = ARGB( 0xFF3F3F3F ) ;
		}

#if UNITY_EDITOR
		private bool m_RemoveGradient = false ;
#endif

		/// <summary>
		/// Gradient の削除
		/// </summary>
		public void RemoveGradient()
		{
			var gradient = CGradient ;
			if( gradient == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gradient ) ;
			}
			else
			{
				Destroy( gradient ) ;
			}
		}

		//----------
		
		// キャッシュ
		private UIInversion m_Inversion = null ;

		/// <summary>
		/// Inversion(ショートカット)
		/// </summary>
		public virtual UIInversion CInversion
		{
			get
			{
				if( m_Inversion == null )
				{
					TryGetComponent<UIInversion>( out m_Inversion ) ;
				}
				return m_Inversion ;
			}
		}
		
		/// <summary>
		/// Inversion の有無
		/// </summary>
		public bool IsInversion
		{
			get
			{
				return ( CInversion != null ) ;
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
			if( CInversion != null )
			{
				return ;
			}
		
			UIInversion inversion ;
		
			inversion = gameObject.AddComponent<UIInversion>() ;
			inversion.DirectionType = UIInversion.DirectionTypes.None ;
		}

#if UNITY_EDITOR
		private bool m_RemoveInversion = false ;
#endif

		/// <summary>
		/// Inversion の削除
		/// </summary>
		public void RemoveInversion()
		{
			var inversion = CInversion ;
			if( inversion == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( inversion ) ;
			}
			else
			{
				Destroy( inversion ) ;
			}
		}

		//----------
		
		// キャッシュ
		private ImageNumber m_ImageNumber = null ;

		/// <summary>
		/// ImageNumber(ショートカット)
		/// </summary>
		public virtual ImageNumber CImageNumber
		{
			get
			{
				if( m_ImageNumber == null )
				{
					TryGetComponent<ImageNumber>( out m_ImageNumber ) ;
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
		public virtual GridMap CGridMap
		{
			get
			{
				if( m_GridMap == null )
				{
					TryGetComponent<GridMap>( out m_GridMap ) ;
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
		public virtual ComplexRectangle CComplexRectangle
		{
			get
			{
				if( m_ComplexRectangle == null )
				{
					TryGetComponent<ComplexRectangle>( out m_ComplexRectangle ) ;
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
		public virtual Line CLine
		{
			get
			{
				if( m_Line == null )
				{
					TryGetComponent<Line>( out m_Line ) ;
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
		public virtual Circle CCircle
		{
			get
			{
				if( m_Circle == null )
				{
					TryGetComponent<Circle>( out m_Circle ) ;
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
		public virtual Arc CArc
		{
			get
			{
				if( m_Arc == null )
				{
					TryGetComponent<Arc>( out m_Arc ) ;
				}
				return m_Arc ;
			}
		}

		//----------
		
		// キャッシュ
		private UIPadAdapter m_PadAdapter = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		public virtual UIPadAdapter CPadAdapter
		{
			get
			{
				if( m_PadAdapter == null )
				{
					TryGetComponent<UIPadAdapter>( out m_PadAdapter ) ;
				}
				return m_PadAdapter ;
			}
		}
		
		/// <summary>
		/// PadAdapter の有無
		/// </summary>
		public bool IsPadAdapter
		{
			get
			{
				return ( CPadAdapter != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddPadAdapter() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemovePadAdapter() ;
					}
					else
					{
						m_RemovePadAdapter = true ;
					}
#else
					RemovePadAdapter() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// PadAdapter の追加
		/// </summary>
		public void AddPadAdapter()
		{
			if( CPadAdapter != null )
			{
				return ;
			}
		
			UIPadAdapter padAdapter ;
		
			padAdapter = gameObject.AddComponent<UIPadAdapter>() ;
			padAdapter.Focus = true ;
		}

#if UNITY_EDITOR
		private bool m_RemovePadAdapter = false ;
#endif

		/// <summary>
		/// PadAdapter の削除
		/// </summary>
		public void RemovePadAdapter()
		{
			var padAdapter = CPadAdapter ;
			if( padAdapter == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( padAdapter ) ;
			}
			else
			{
				Destroy( padAdapter ) ;
			}
		}

		//----------
		
		// キャッシュ
		private Animator m_Animator = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		public virtual Animator CAnimator
		{
			get
			{
				if( m_Animator == null )
				{
					TryGetComponent<Animator>( out m_Animator ) ;
				}
				return m_Animator ;
			}
		}
		
		/// <summary>
		/// Animator の有無
		/// </summary>
		public bool IsAnimator
		{
			get
			{
				return ( CAnimator != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddAnimator() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveAnimator() ;
					}
					else
					{
						m_RemoveAnimator = true ;
					}
#else
					RemoveAnimator() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Animator の追加
		/// </summary>
		public void AddAnimator()
		{
			if( CAnimator != null )
			{
				return ;
			}
		
			Animator animator ;
		
			animator = gameObject.AddComponent<Animator>() ;
			animator.speed = 1 ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAnimator = false ;
#endif

		/// <summary>
		/// Animator の削除
		/// </summary>
		public void RemoveAnimator()
		{
			var animator = CAnimator ;
			if( animator == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( animator ) ;
			}
			else
			{
				Destroy( animator ) ;
			}
		}

		//----------------------------------------------------------------
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="identity">T identity.</param>
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
			UIView view ;
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				if( go.transform.GetChild( i ).gameObject.TryGetComponent<UIView>( out view ) == true )
				{
					if( view.Identity == identity )
					{
						// 発見
						return view ;
					}
				}
			}
		
			// 直の子供を再帰的に検査する
			for( i  = 0 ; i <  c ; i ++ )
			{
				view = FindView_Private( go.transform.GetChild( i ).gameObject, identity ) ;
				if( view != null )
				{
					// 発見
					return view ;
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の View を取得する
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="identity">T identity.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindView<T>( string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			if( Identity == identity )
			{
				// 自身がそうだった
				TryGetComponent<T>( out component ) ;
				return component ;
			}
		
			component = FindView_Private<T>( gameObject, identity ) ;
			return component ;
		}
	
		private T FindView_Private<T>( GameObject go, string identity ) where T : UnityEngine.Component
		{
			T component ;
		
			// 直の子供を確認する
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				if( go.transform.GetChild( i ).gameObject.TryGetComponent<UIView>( out var view ) == true )
				{
					if( view.Identity == identity )
					{
						// 発見
						view.gameObject.TryGetComponent<T>( out component ) ;
						return component ;	// null でもとりあえず良し
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
		/// <param name="nodeName">T name.</param>
		public GameObject FindNode( string nodeName, bool isContain = false )
		{
			if( name == nodeName )
			{
				// 自身がそうだった
				return gameObject ;
			}
		
			return FindNode_Private( gameObject, nodeName, isContain ) ;
		}
	
		private GameObject FindNode_Private( GameObject go, string nodeName, bool isContain )
		{
			nodeName = nodeName.ToLower() ;

			Transform childNode ;
			string childNodeName ;
			bool result ;

			GameObject targetGameObject ;

			// 直の子供を再帰的に検査する
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				childNode = go.transform.GetChild( i ) ;
				childNodeName = childNode.name.ToLower() ;
				result = false ;
				if( isContain == false && childNodeName == nodeName )
				{
					result = true ;
				}
				else
				if( isContain == true && childNodeName.Contains( nodeName ) == true )
				{
					result = true ;
				}

				if( result == true )
				{
					// 発見
					targetGameObject = childNode.gameObject ;
					if( targetGameObject != null )
					{
						// 発見
						return targetGameObject ;
					}
				}
			
				if( childNode.childCount >  0 )
				{
					targetGameObject = FindNode_Private( childNode.gameObject, nodeName, isContain ) ;
					if( targetGameObject != null )
					{
						// 発見
						return targetGameObject ;
					}
				}
			}

			return null ;
		}
	
		/// <summary>
		/// 指定の識別子の GameObject 内の Component を取得する
		/// </summary>
		/// <returns>The game object.</returns>
		/// <param name="nodeName">T name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindNode<T>( string nodeName, bool isContain = false ) where T : UnityEngine.Component
		{
			if( name == nodeName )
			{
				// 自身がそうだった
				if( gameObject.TryGetComponent<T>( out var component ) == true )
				{
					return component ;
				}
			}

			return FindNode_Private<T>( gameObject, nodeName, isContain ) ;
		}
	
		private T FindNode_Private<T>( GameObject go, string nodeName, bool isContain ) where T : UnityEngine.Component
		{
			nodeName = nodeName.ToLower() ;

			Transform childNode ;
			string childNodeName ;
			bool result ;

			T component ;
		
			// 直の子供を再帰的に検査する
			int i, c = go.transform.childCount ;
		
			for( i  = 0 ; i <  c ; i ++ )
			{
				childNode = go.transform.GetChild( i ) ;
				childNodeName = childNode.name.ToLower() ;
				result = false ;

				if( isContain == false && childNodeName == nodeName )
				{
					result = true ;
				}
				else
				if( isContain == true && childNodeName.Contains( nodeName ) == true )
				{
					result = true ;
				}
				
				if( result == true )
				{
					// 発見
					if( childNode.TryGetComponent<T>( out component ) == true )
					{
						return component ;
					}
				}

				if( childNode.childCount >  0 )
				{
					component = FindNode_Private<T>( childNode.gameObject, nodeName, isContain ) ;
					if( component != null )
					{
						// 発見
						return component ;
					}
				}
			}
		
			// このゲームオブジェクト以下には該当する名前のゲームオブジェクトは発見できなかった
			return default ;
		}
	
		//--------------------------------------------------------------------

		// ライブラリで持つ基本的なインタラクションイベントを登録する

		// 無効なポインター識別子
		protected const int m_UnKnownCode = -1000000000 ;

		// 通常用のコールバックを登録する
		private bool AddInteractionCallback()
		{
			UIInteraction interaction = CInteraction ;
			if( interaction == null )
			{
				return false ;
			}

			RemoveInteractionCallback()	;	// 多重登録にならないように削除しておく
			
			interaction.onPointerEnter	+= OnPointerEnterBasic	;
			interaction.onPointerExit	+= OnPointerExitBasic	;
			interaction.onPointerDown	+= OnPointerDownBasic	;
			interaction.onPointerUp		+= OnPointerUpBasic		;
			interaction.onPointerClick	+= OnPointerClickBasic	;
			interaction.onDrag			+= OnDragBasic			;

			return true ;
		}

		// 通常用のコールバックを解除する
		private bool RemoveInteractionCallback()
		{
			UIInteraction interaction = CInteraction ;
			if( interaction == null )
			{
				return false ;
			}

			interaction.onPointerEnter	-= OnPointerEnterBasic	;
			interaction.onPointerExit	-= OnPointerExitBasic	;
			interaction.onPointerDown	-= OnPointerDownBasic	;
			interaction.onPointerUp		-= OnPointerUpBasic		;
			interaction.onPointerClick	-= OnPointerClickBasic	;
			interaction.onDrag			-= OnDragBasic			;

			return true ;
		}

		// スクロールビュー用のコールバックを登録する
		private bool AddInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return false ;
			}

			RemoveInteractionForScrollViewCallback()	;	// 多重登録にならないように削除しておく
			
			interactionForScrollView.onPointerEnter	+= OnPointerEnterBasic	;
			interactionForScrollView.onPointerExit	+= OnPointerExitBasic	;
			interactionForScrollView.onPointerDown	+= OnPointerDownBasic	;
			interactionForScrollView.onPointerUp	+= OnPointerUpBasic		;
			interactionForScrollView.onPointerClick	+= OnPointerClickBasic	;
//			interactionForScrollView.onDrag			+= OnDragBasic			;

			return true ;
		}


		// スクロールビュー用のコールバックを解除する
		private bool RemoveInteractionForScrollViewCallback()
		{
			UIInteractionForScrollView interactionForScrollView = CInteractionForScrollView ;
			if( interactionForScrollView == null )
			{
				return false ;
			}

			interactionForScrollView.onPointerEnter	-= OnPointerEnterBasic	;
			interactionForScrollView.onPointerExit	-= OnPointerExitBasic	;
			interactionForScrollView.onPointerDown	-= OnPointerDownBasic	;
			interactionForScrollView.onPointerUp	-= OnPointerUpBasic		;
			interactionForScrollView.onPointerClick	-= OnPointerClickBasic	;
//			interactionForScrollView.onDrag			-= OnDragBasic			;
			
			return true ;
		}

		//--------------------------------------------------------------------

		private bool	m_HoverAtFirst = false ;
		private Vector2	m_HoverPosition ;

		/// <summary>
		/// ホバー状態
		/// </summary>
		public  bool  IsHover
		{
			get
			{
				return InputAdapter.UIEventSystem.IsHovering( gameObject ) ;
			}
		}

		/// <summary>
		/// プレス状態
		/// </summary>
		public  bool  IsPress
		{
			get
			{
				bool isPress = InputAdapter.UIEventSystem.IsPressing( gameObject ) ;

				//---------------------------------------------------------
				// 同じＵＩに対するプレスの連続入力を禁止する対応が入っている場合の処理

				if( isPress == true && m_PressInvalidTime >  0 )
				{
					if( m_PressCountTime == Time.frameCount )
					{
						// 一番最初だけは有効にする
						return true ;
					}
					else
					if( ( Time.realtimeSinceStartup - m_PressStartTime ) <  m_PressInvalidTime )
					{
						// 指定時間経過までは無効扱いとする
						return false ;
					}
				}

				//---------------------------------------------------------

				return isPress ;
			}
		}

		private float m_PressInvalidTime = 0 ;
		private int   m_PressCountTime = 0 ;
		private float m_PressStartTime = 0 ;

		public float PressInvalidTime
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

		public bool IsClick
		{
			get
			{
				return m_Click ;
			}
		}

		protected int			m_ClickCountTime = 0 ;

		protected bool			m_ClickState						= false ;
		protected int			m_ClickPointerId					= m_UnKnownCode ;
		protected bool			m_ClickInsideFlag					= false ;
		
		protected bool			m_SmartClickState					= false ;
		protected int			m_SmartClickPointerId				= m_UnKnownCode ;
		protected bool			m_SmartClickInsideFlag				= false ;
		protected Vector2		m_SmartClickBasePosition			= Vector2.zero ;
		protected int			m_SmartClickCount					= 0 ;
		protected float			m_SmartClickBaseTime				= 0 ;

		protected float			m_SmartClickReleaseLimitTime		= 0.5f ;
		protected float			m_SmartClickSecondPressLimitTime	= 0.25f ;	// シングルクリック終了後にこの時間以内に新しいクリックが開始されたらダブルクリック判定が始まる
		protected float			m_SmartClickLimitDistance			= 30.0f ;

		protected float			m_LongPressDecisionTime				= 0.75f ;	// 長押しと判定する時間
		protected float         m_LongPressLimitDistance            = 0 ;       // 最初の位置からこれだ動いたら無効とする距離(半径)
		protected bool			m_LongPressExecuted					= false ;
		protected float			m_LongPressTimer					= 0 ;
		protected Vector2       m_LongPressBasePosition ;                       // 長押しを開始した際の位置

		protected float			m_RepeatPressStartingTime			= 0.75f ;
		protected float			m_RepeatPressIntervalTime			= 0.25f ;
		protected bool			m_RepeatPressState					= false ;
		protected int			m_RepeatPressCount					= 0 ;
		protected float			m_RepeatPressTimer					= 0 ;

		/// <summary>
		/// ドラッグ状態
		/// </summary>
		public enum PointerState
		{
			/// <summary>
			/// 無し
			/// </summary>
			None		= 0,

			/// <summary>
			/// 開始
			/// </summary>
			Start		= 1,

			/// <summary>
			/// 移動
			/// </summary>
			Moving		= 2,

			/// <summary>
			/// 終了
			/// </summary>
			End			= 3,
		}

		/// <summary>
		/// ドラッグ状態
		/// </summary>
		public PointerState DragState
		{
			get
			{
				return m_DragState ;
			}
		}

		protected PointerState	m_DragState						= PointerState.None ;
		protected int			m_DragPointerId					= m_UnKnownCode ;
		protected Vector2		m_DragBasePosition				= Vector2.zero ;


		protected bool			m_FlickState					= false ;
		protected int			m_FlickPointerId				= m_UnKnownCode ;
		protected float			m_FlickDecisionLimitTime		=  0.5f ;
		protected float			m_FlickDecisionDistance			=  60.0f ;
		protected Vector2		m_FlickBasePosition				= Vector2.zero ;
		protected float			m_FlickBaseTime					= 0 ;
		protected bool			m_FlickCheck					= false ;
		protected float			m_FlickLastTime					= 0 ;

		public class TouchState
		{
			public int			Index ;
			public int			Identity ;
			public Vector2		Position ;
			public PointerState	State ;

			public TouchState( int index, int identity, Vector2 position, PointerState state )
			{
				Index		= index ;
				Identity	= identity ;
				Position	= position ;
				State		= state ;
			}
		}

		protected TouchState[]		m_TouchState			= new TouchState[ 10 ] ;
		protected List<TouchState>	m_TouchStateExchange	= new () ;

		protected bool		m_FromScrollView = false ;
		protected float		m_InteractionLimit = 8.0f ;
		protected Vector2	m_InteractionLimit_StartPoint ;

		//--------------------------------------------------------------------

		// Enter
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		protected virtual void OnPointerEnterBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FF7F7F>OnPointerEnterBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			if( m_HoverAtFirst == false )
			{
				// 初めて入った
				m_HoverAtFirst	= true ;
				m_HoverPosition	= position ;

				OnHoverInner( PointerState.Start, position ) ;

				// クリック処理
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					// 中に入ったので以後は有効クリック扱いとなる
					m_ClickInsideFlag = true ;
				}
			}
			else
			{
				// ２回目以降
				if( InputAdapter.UIEventSystem.IsPressing( gameObject ) == false && m_DragState == PointerState.None )
				{
					if( m_HoverPosition.Equals( position ) == false )
					{
						m_HoverPosition = position ;

						OnHoverInner( PointerState.Moving, position ) ;
					}
				}
			}
		}

		// Exit
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		protected virtual void OnPointerExitBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#7FFFFF>OnPointerExitBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			m_HoverAtFirst = false ;
			OnHoverInner( PointerState.End, position ) ;

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_ClickInsideFlag = false ;
				}
//			}

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
//			{
				if( m_SmartClickState == true && m_SmartClickPointerId == identity )
				{
					// 外に出たので以後は無効クリック扱いとなる
					m_SmartClickInsideFlag = false ;
				}
//			}

			// ホールド判定終了
			CancelPress() ;
		}

		// Down
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		protected virtual void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FF7F00>OnPointerDownBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			// 円形であった場合の範囲内チェック
			if( CheckCollisionRadius( position ) == false )
			{
				return ;	// 不可
			}

			m_FromScrollView = fromScrollView ;
			if( m_FromScrollView == true )
			{
				m_InteractionLimit_StartPoint = pointer.position ;
			}

			m_PressCountTime = Time.frameCount ;
			m_PressStartTime = Time.realtimeSinceStartup ;
			OnPressInner( true ) ;

			//----------------------------------------------------------

			// リピートプレス確認
			if( ( OnRepeatPressAction != null || OnRepeatPressDelegate != null ) && m_RepeatPressCount == 0 )
			{
				m_RepeatPressState = true ;
				ProcessRepeatPress() ;
			}

			// ロングプレス確認
			if( ( OnLongPressAction != null || OnLongPressDelegate != null ) && m_LongPressTimer == 0 )
			{
				// 長押しを開始した時間を記録する
				m_LongPressExecuted	= false ;
				m_LongPressTimer    = Time.realtimeSinceStartup ;

				if( m_LongPressLimitDistance >  0 )
				{
					// 長押しを開始した位置を記録する
					( _, m_LongPressBasePosition ) = GetSinglePointerInCanvas() ;
				}
			}

			//------------------------------------------------------------------------------------------

			if( pointer.eligibleForClick == false )
			{
				// このプレスではクリックとドラッグの処理はできない
				return ;
			}

			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				m_ClickState = true ;
				m_ClickPointerId = identity ;
				m_ClickInsideFlag = true ;
//			}

			// 離れた時から計測を開始するので押した時は無効化
			m_SingleClickCheck = false ;

			// スマートクリック処理
//			if( onSmartClickDelegate != null )
///			{
				if( m_SmartClickState == false )
				{
					// １回目のクリック
					m_SmartClickState = true ;
					m_SmartClickPointerId = identity ;
					m_SmartClickInsideFlag = true ;

					m_SmartClickBasePosition = position ;
					m_SmartClickCount = 1 ;
					m_SmartClickBaseTime = Time.realtimeSinceStartup ;
				}
				else
				{
					// ２回目のクリック

					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
					{
						// ダブルクリック判定に入る
						m_SmartClickPointerId = identity ;	// 識別子を新しくする(１回目と２回目では異なる可能性がある)

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
			if( fromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ処理
			if( OnDragAction != null || OnDragDelegate != null )
			{
//				if( m_DragState == PointerState.None )
//				{
					m_DragPointerId = identity ;
					m_DragBasePosition = position ;
	
					m_DragState = PointerState.Start ;

					OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
//				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == false )
				{
					m_FlickState = true ;
	
					m_FlickPointerId = identity ;
					m_FlickBasePosition = position ;

					m_FlickCheck = false ;
				}
			}
		}

		// Up
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		protected virtual void OnPointerUpBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#00FFFF>OnPointerUpBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			m_FromScrollView = fromScrollView ;

			m_PressCountTime = 0 ;
			m_PressStartTime = 0 ;
			OnPressInner( false ) ;

			//----------------------------------------------------------

			// スクロールビュー上では以下は無視する
			if( fromScrollView == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// スクロールビューでなければ処理する
			// クリック処理
//			if( onClickAction != null || onClickDelegate != null )
//			{
				if( m_ClickState == true && m_ClickPointerId == identity )
				{
					if( m_ClickInsideFlag == true )
					{
						if( m_LongPressExecuted == false )
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
				if( m_SmartClickState == true && m_SmartClickPointerId == identity )
				{
					if( m_SmartClickInsideFlag == true )
					{
						if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
						{
							float time = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

							if( m_SmartClickCount == 1 )
							{
								// シングルクリック判定
								if( m_SmartClickReleaseLimitTime <= 0 )
								{
									// 常にシングルクリック
									if( m_LongPressExecuted == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
									}

									m_SmartClickState = false ;
								}
								else
								if( time <  m_SmartClickReleaseLimitTime )
								{
									// 一定時間以内に離していないと無効
									if( m_SmartClickSecondPressLimitTime <= 0 )
									{
										// シングルクリック決定
										if( m_LongPressExecuted == false )
										{
											OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
										}

										m_SmartClickState = false ;
									}
									else
									{
										// シングルクリックかダブルクリックかを判定するルーチンを起動する
										if( m_LongPressExecuted == false )
										{
											SingleClickCheck( position, pointer.position ) ;
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
	
								if( time <  m_SmartClickReleaseLimitTime )
								{
									// 一定時間以内に離していないと無効
	
									// ダブルクリック決定
									
									if( m_LongPressExecuted == false )
									{
										OnSmartClickInner( 2, m_SmartClickBasePosition, position ) ;
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
			if( OnDragAction != null || OnDragDelegate != null )
			{
				if( ( m_DragState == PointerState.Start || m_DragState == PointerState.Moving ) && m_DragPointerId == identity )
				{
					m_DragState = PointerState.None ;
	
					OnDragInner( PointerState.End, m_DragBasePosition, position ) ;
				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == identity )
				{
					m_FlickState = false ;
	
					if( m_FlickCheck == true )
					{
						// １つ前のドラッグ位置と時間で最後に静止していたか判定する
						float lastTime = Time.realtimeSinceStartup - m_FlickLastTime ;

						if( lastTime <  0.1f )
						{
							// 基準位置からの移動量と時間で判定する
							Vector2 value = position - m_FlickBasePosition ;
							float time = Time.realtimeSinceStartup - m_FlickBaseTime ;
	
							if( time <  m_FlickDecisionLimitTime && value.magnitude >  m_FlickDecisionDistance )
							{
								// フリック有効
								OnFlickInner( value, m_FlickBasePosition ) ;
							}
						}
					}
				}
			}

			// 長押し判定終了
			CancelPress() ;
		}

		// スクロールビュー用のクリック判定(通常のクリック判定には使用していない)
		protected virtual void OnPointerClickBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FFFF00>OnPointerClickBasic:" + name + "</color>" ) ;

			if( fromScrollView == false )
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

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			// 円形であった場合の範囲内チェック
			if( CheckCollisionRadius( position ) == false )
			{
				return ;	// 不可
			}

			m_FromScrollView = fromScrollView ;

			//----------------------------------------------------------
			// Click

			if( m_ClickState == true && m_ClickPointerId == identity )
			{
				if( m_ClickInsideFlag == true )
				{
					if( m_LongPressExecuted == false )
					{
						// クリックとみなす
						OnClickInner() ;
					}
				}
			}

			m_ClickState = false ;

			//----------------------------------------------------------
			// SmartClick

			if( m_SmartClickState == true && m_SmartClickPointerId == identity )
			{
				if( m_SmartClickInsideFlag == true )
				{
					if( m_SmartClickLimitDistance == 0 || ( m_SmartClickLimitDistance >  0 && ( m_SmartClickBasePosition - position ).magnitude <= m_SmartClickLimitDistance ) )
					{
						float time = Time.realtimeSinceStartup - m_SmartClickBaseTime ;

						if( m_SmartClickCount == 1 )
						{
							// シングルクリック判定
							if( m_SmartClickReleaseLimitTime <= 0 )
							{
								// 常にシングルクリック
									
								if( m_LongPressExecuted == false )
								{
									OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
								}

								m_SmartClickState = false ;
							}
							else
							if( time <  m_SmartClickReleaseLimitTime )
							{
								// 一定時間以内に離していないと無効
								if( m_SmartClickSecondPressLimitTime <= 0 )
								{
									// シングルクリック決定

									if( m_LongPressExecuted == false )
									{
										OnSmartClickInner( 1, m_SmartClickBasePosition, position ) ;
									}

									m_SmartClickState = false ;
								}
								else
								{
									// シングルクリックかダブルクリックかを判定するルーチンを起動する
									if( m_LongPressExecuted == false )
									{
										SingleClickCheck( position, pointer.position ) ;
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
	
							if( time <  m_SmartClickReleaseLimitTime )
							{
								// 一定時間以内に離していないと無効
	
								// ダブルクリック決定
									
								if( m_LongPressExecuted == false )
								{
									OnSmartClickInner( 2, m_SmartClickBasePosition, position ) ;
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
			CancelPress() ;
		}

		// パッドボタンが有効である場合無制限ドラッグが可能になる
		protected bool m_IsPadButtonEnabled = false ;

		// Drag
		// 他の処理が無効化されてしまうため、メソッド内で return を使ってはダメ。本当は処理をメソッド単位に分離した方が良いのだが。
		protected virtual void OnDragBasic( PointerEventData pointer, bool fromScrollView )
		{
//			Debug.Log( "<color=#FFFF00>OnPointerDragBasic:" + name + "</color>" ) ;

			int identity = pointer.pointerId ;
			Vector2 position = GetLocalPosition( pointer ) ;

			// 円形であった場合の範囲内チェック
			if( CheckCollisionRadius( position ) == false )
			{
				return ;	// 不可
			}

			m_FromScrollView = fromScrollView ;
			if( ( m_FromScrollView == true && Vector2.Distance( m_InteractionLimit_StartPoint, pointer.position ) >= m_InteractionLimit ) )
			{
				m_ClickState = false ;			// クリックキャンセル	
				m_SmartClickState = false ;	// スマートクリックキャンセル

				// 長押しをキャンセルする
				CancelPress() ;
			}

			// ドラッグ・フリック・タッチは処理が重いのでこの段階でデリゲートで抑制する

			// ドラッグ中の処理
			if( OnDragAction != null || OnDragDelegate != null )
			{
				if( m_DragPointerId == identity )
				{
					if( m_IsPadButtonEnabled == false )
					{
						// パッドボタンでは無いためシングルドラッグのみ許可される

						if( m_DragState == PointerState.Start || m_DragState == PointerState.Moving )
						{
							if( Input.touchCount == 1 || Input.touchCount == 0 )
							{
								// touchCount == 0  の場合は完全に PC(マウス) 環境
								m_DragState = PointerState.Moving ;
		
								OnDragInner( PointerState.Moving, m_DragBasePosition, position ) ;
							}
							else
							if( Input.touchCount >= 2 )
							{
								// ２点以上タッチされていたら一旦解除する
								m_DragState = PointerState.None ;
		
								OnDragInner( PointerState.End, m_DragBasePosition, position ) ;
							}
						}
						else
						if( m_DragState == PointerState.None )
						{
							// 最初が None 状態でこのメソッドが呼ばれる事は基本的に無い(ここは保険)

							if( Input.touchCount == 1 || Input.touchCount == 0 )
							{
								// touchCount == 0 の場合は完全に PC(マウス) 環境

								// １点タッチに戻ったので開始扱いとする
								m_DragBasePosition = position ;
	
								m_DragState = PointerState.Start ;

								OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
							}
						}
					}
					else
					{
						// パッドボタンであるため無宣言ドラッグを可能とする

						if( m_DragState == PointerState.Start || m_DragState == PointerState.Moving )
						{
							// touchCount == 0  の場合は完全に PC(マウス) 環境
							m_DragState = PointerState.Moving ;
		
							OnDragInner( PointerState.Moving, m_DragBasePosition, position ) ;
						}
						else
						if( m_DragState == PointerState.None )
						{
							// 最初が None 状態でこのメソッドが呼ばれる事は基本的に無い(ここは保険)

							// １点タッチに戻ったので開始扱いとする
							m_DragBasePosition = position ;
	
							m_DragState = PointerState.Start ;

							OnDragInner( PointerState.Start, m_DragBasePosition, m_DragBasePosition ) ;
						}
					}
				}
			}

			// フリック処理
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( m_FlickState == true && m_FlickPointerId == identity )
				{
					if( m_FlickCheck == false )
					{
						Vector2 value = position - m_FlickBasePosition ;
	
						if( value.magnitude >   m_FlickDecisionDistance )
						{
							// フリック計測開始
							m_FlickCheck = true ;
							m_FlickBaseTime = Time.realtimeSinceStartup ;

							m_FlickLastTime = m_FlickBaseTime ;
						}
						else
						{
							// 基準の位置を更新
							m_FlickBasePosition = position ;
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
		}

		// プレス終了処理
		private void CancelPress()
		{
			m_RepeatPressState			= false ;
			m_RepeatPressCount			= 0 ;
			m_RepeatPressTimer			= 0 ;

			m_LongPressExecuted			= false ;
			m_LongPressTimer			= 0 ;
		}

		//-------------------------------------------------------------------------------------------

		protected float m_CollisionRadiusRatio = 0 ;

		private bool CheckCollisionRadius( Vector2 position )
		{
			if( m_CollisionRadiusRatio <= 0 )
			{
				// 円形指定ではない
				return true ;
			}

			float sx = Width ;
			float sy = Height ;

			float px = Pivot.x ;
			float py = Pivot.y ;

			// 中心を基準として座標になるように補正
			float x = position.x + ( sx * ( px - 0.5f ) ) ;
			float y = position.y + ( sy * ( py - 0.5f ) ) ;

			float hx = sx * 0.5f ;
			float hy = sy * 0.5f ;

			var p = new Vector2( x / hx, y / hy ) ;
			float d = p.magnitude ;

			return ( d <  m_CollisionRadiusRatio ) ;
		}

		//-----------------------------------

		private bool	m_SingleClickCheck ;
		private Vector2 m_SingleClickCheck_Position ;
		private Vector2 m_SingleClickCheck_GlobalPosition ;
		private float	m_SingleClickCheck_BaseTime ;
		private float   m_SingleClickCheck_TickTime ;
		
		// シングルクリックかダブルクリックかの判定用のコルーチン(ScrollView内でコルーチンを使用するのは危険なので＝アイテムの作り直し問題・Updateで処理するようにする)
		private void SingleClickCheck( Vector2 position, Vector2 globalPosition )
		{
			m_SingleClickCheck					= true ;

			m_SingleClickCheck_Position			= position ;
			m_SingleClickCheck_GlobalPosition	= globalPosition ;
			
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

			m_SingleClickCheck_TickTime += ( Time.realtimeSinceStartup - m_SingleClickCheck_BaseTime ) ;
			if( m_SingleClickCheck_TickTime <  m_SmartClickSecondPressLimitTime )
			{
				return ;
			}

			//----------------------------------------------------------
			// ダブルクリックと判定するまでに押さなければならない時間をオーバーした

			if( m_LongPressExecuted == false )
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

		// Multi Touch

		/// <summary>
		/// 複数点タッチを処理する
		/// </summary>
		private void ProcessMultiTouch()
		{
			( int states, Vector2[] pointers ) = GetMultiPointer( true ) ;	// レイキャストのチェックを有効にする

			if( OnPinchAction != null || OnPinchDelegate != null )
			{
				// コールバック発行
				OnPinchInner( states, pointers ) ;
			}

			if( OnTouchAction != null || OnTouchDelegate != null )
			{
				// コールバック発行
				OnTouchInner( states, pointers ) ;
			}
		}

		//--------------------------------------------------------------------

		// Hover

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクション(マウス用)
		/// </summary>
		public Action<string, UIView, PointerState, Vector2> OnHoverAction ;
	
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートの定義(マウス用)
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">状態</param>
		/// <param name="movePosition">現在位置</param>
		public delegate void OnHover( string identity, UIView view, PointerState state, Vector2 movePosition ) ;
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲート(マウス用)
		/// </summary>
		public OnHover OnHoverDelegate ;

		// 内部プロセス
		private void ProcessHover()
		{
			if( m_HoverAtFirst == true )
			{
				// ホバー中である場合に位置が変わったらコールバックを呼ぶ

//				if( InputAdapter.UIEventSystem.IsPressing( gameObject ) == false && m_DragState == PointerState.None )
//				{
					Vector2 position = GetLocalPosition( InputAdapter.UIEventSystem.MousePosition ) ;

					if( m_HoverPosition.Equals( position ) == false )
					{
						m_HoverPosition = position ;

						OnHoverInner( PointerState.Moving, position ) ;
					}
//				}
			}
		}

		// 内部リスナー
		private void OnHoverInner( PointerState state, Vector2 movePosition )
		{
			if( OnHoverAction != null || OnHoverDelegate != null || OnSimpleHoverAction != null || OnSimpleHoverDelegate != null ) 
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnHoverAction?.Invoke( identity, this, state, movePosition ) ;
				OnHoverDelegate?.Invoke( identity, this, state, movePosition ) ;

				if( state != PointerState.None && state != PointerState.Moving )
				{
					bool isHover = ( state != PointerState.End ) ;

					OnSimpleHoverAction?.Invoke( isHover ) ;
					OnSimpleHoverDelegate?.Invoke( isHover ) ;
				}
			}
		}
		
		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるアクションを設定する → OnHover( string identity, UIView view, UIView.PointerState state, Vector2 movePosition )
		/// </summary>
		/// <param name="onHover">アクションメソッド</param>
		public void SetOnHover( Action<string, UIView, PointerState, Vector2> onHoverAction )
		{
			OnHoverAction = onHoverAction ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを追加する → OnHover( string identity, UIView view, UIView.PointerState state, Vector2 movePosition )
		/// </summary>
		/// <param name="onHOverelegate">デリゲートメソッド</param>
		public void AddOnHover( OnHover onHoverDelegate )
		{
			OnHoverDelegate += onHoverDelegate ;
		}

		/// <summary>
		/// ビューの範囲に入ったまたは出た際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onHoverDelegate">デリゲートメソッド</param>
		public void RemoveOnHover( OnHover onHoverDelegate )
		{
			OnHoverDelegate -= onHoverDelegate ;
		}

		//-----------

		/// <summary>
		/// ビューをホバーした際に呼び出されるアクション
		/// </summary>
		public Action<bool> OnSimpleHoverAction ;
		
		/// <summary>
		/// ビューをホバーした際に呼び出されるデリゲートの定義
		/// </summary>
		public delegate void OnSimpleHover( bool state ) ;

		/// <summary>
		/// ビューをホバーした際に呼び出されるデリゲート
		/// </summary>
		public OnSimpleHover OnSimpleHoverDelegate ;

		/// <summary>
		/// ビューをホバーした際に呼び出されるアクションを設定する → OnHover( bool state )
		/// </summary>
		public void SetOnSimpleHover( Action<bool> onSimpleHoverAction )
		{
			OnSimpleHoverAction = onSimpleHoverAction ;
		}

		/// <summary>
		/// ビューをホバーした際に呼び出されるデリゲートを追加する → OnPress( bool state )
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void AddOnSimpleHover( OnSimpleHover onSimpleHoverDelegate )
		{
			OnSimpleHoverDelegate += onSimpleHoverDelegate ;
		}

		/// <summary>
		/// ビューをホバーした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void RemoveOnSimpleHover( OnSimpleHover onSimpleHoverDelegate )
		{
			OnSimpleHoverDelegate -= onSimpleHoverDelegate ;
		}

		//----------------------
		// Press

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, bool> OnPressAction ;
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="isPressed">状態(true=プレス・false=リリース)</param>
		public delegate void OnPress( string identity, UIView view, bool isPressed ) ;

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲート
		/// </summary>
		public OnPress OnPressDelegate ;

		/// <summary>
		/// プレスを強制的に実行する
		/// </summary>
		public void ExecutePress( bool state )
		{
			OnPressInner( state ) ;
		}

		// 内部リスナー
		private void OnPressInner( bool state )
		{
			m_WaitForPress = true ;

			if( OnPressAction != null || OnPressDelegate != null || OnSimplePressAction != null || OnSimplePressDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnPressAction?.Invoke( identity, this, state ) ;
				OnPressDelegate?.Invoke( identity, this, state ) ;

				OnSimplePressAction?.Invoke( state ) ;
				OnSimplePressDelegate?.Invoke( state ) ;
			}
		}
		
		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるアクションを設定する → OnPress( string identity, UIView view, bool isPressed )
		/// </summary>
		/// <param name="onPress">アクションメソッド</param>
		public void SetOnPress( Action<string, UIView, bool> onPressAction )
		{
			OnPressAction = onPressAction ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを追加する → OnPress( string identity, UIView view, bool isPressed )
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void AddOnPress( OnPress onPressDelegate )
		{
			OnPressDelegate += onPressDelegate ;
		}

		/// <summary>
		/// ビューを押したまたは離した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void RemoveOnPress( OnPress onPressDelegate )
		{
			OnPressDelegate -= onPressDelegate ;
		}

		//-----------

		/// <summary>
		/// ビューをプレスした際に呼び出されるアクション
		/// </summary>
		public Action<bool> OnSimplePressAction ;
		
		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートの定義
		/// </summary>
		public delegate void OnSimplePress( bool state ) ;

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲート
		/// </summary>
		public OnSimplePress OnSimplePressDelegate ;

		/// <summary>
		/// ビューをプレスした際に呼び出されるアクションを設定する → OnPress( bool state )
		/// </summary>
		public void SetOnSimplePress( Action<bool> onSimplePressAction )
		{
			OnSimplePressAction = onSimplePressAction ;
		}

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートを追加する → OnPress( bool state )
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void AddOnSimplePress( OnSimplePress onSimplePressDelegate )
		{
			OnSimplePressDelegate += onSimplePressDelegate ;
		}

		/// <summary>
		/// ビューをプレスした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPressDelegate">デリゲートメソッド</param>
		public void RemoveOnSimplePress( OnSimplePress onSimplePressDelegate )
		{
			OnSimplePressDelegate -= onSimplePressDelegate ;
		}

		//----------------------
		// RepeatPress

		/// <summary>
		/// ビューを長押しした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, int> OnRepeatPressAction ;
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnRepeatPress( string identity, UIView view, int repeatCount ) ;

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲート
		/// </summary>
		public OnRepeatPress OnRepeatPressDelegate ;

		// 内部プロセス
		private void ProcessRepeatPress()
		{
			if( m_RepeatPressState == false )
			{
				return ;
			}

			//----------------------------------------------------------

			if( m_RepeatPressCount == 0 )
			{
				m_RepeatPressTimer = Time.realtimeSinceStartup ;

				OnRepeatPressInner( m_RepeatPressCount ) ;

				m_RepeatPressCount ++ ;	// リピート回数
			}
			else
			if( m_RepeatPressCount == 1 )
			{
				float deltaTime = Time.realtimeSinceStartup - m_RepeatPressTimer ;

				if( deltaTime >= m_RepeatPressStartingTime )
				{
					m_RepeatPressTimer = Time.realtimeSinceStartup ;

					OnRepeatPressInner( m_RepeatPressCount ) ;

					m_RepeatPressCount ++ ;	// リピート回数
				}
			}
			else
			if( m_RepeatPressCount >= 2 )
			{
				float deltaTime = Time.realtimeSinceStartup - m_RepeatPressTimer ;

				if( deltaTime >= m_RepeatPressIntervalTime )
				{
					m_RepeatPressTimer = Time.realtimeSinceStartup ;

					OnRepeatPressInner( m_RepeatPressCount ) ;

					m_RepeatPressCount ++ ;	// リピート回数
				}
			}
		}

		/// <summary>
		/// プレスを強制的に実行する
		/// </summary>
		public void ExecuteRepeatPress( int count )
		{
			OnRepeatPressInner( count ) ;
		}

		// 内部リスナー
		private void OnRepeatPressInner( int count )
		{
			// 識別子
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			// 最初のコールバック
			OnRepeatPressAction?.Invoke( identity, this, count ) ;
			OnRepeatPressDelegate?.Invoke( identity, this, count ) ;
		}
		
		/// <summary>
		/// ビューを押し続けた際に呼び出されるアクションを設定する → OnRepeatPress( string identity, UIView view )
		/// </summary>
		/// <param name="onRepeatPress">アクションメソッド</param>
		public void SetOnRepeatPress( Action<string, UIView, int> onRepeatPressAction, float repeatPressStartingTime = 0.75f, float repeatPressIntervalTime = 0.25f )
		{
			OnRepeatPressAction = onRepeatPressAction ;
			if( repeatPressStartingTime >  0 )
			{
				m_RepeatPressStartingTime = repeatPressStartingTime ;
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを追加する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onRepeatPressDelegate">デリゲートメソッド</param>
		public void AddOnRepeatPress( OnRepeatPress onRepeatPressDelegate, float repeatPressStartingTime = 0.75f, float repeatPressIntervalTime = 0.25f )
		{
			OnRepeatPressDelegate += onRepeatPressDelegate ;
			if( repeatPressStartingTime >  0 )
			{
				m_RepeatPressStartingTime = repeatPressStartingTime ;
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onRepeatPressDelegate">デリゲートメソッド</param>
		public void RemoveOnRepeatPress( OnRepeatPress onRepeatPressDelegate )
		{
			OnRepeatPressDelegate -= onRepeatPressDelegate ;
		}

		/// <summary>
		/// リピートの間隔時間を設定する
		/// </summary>
		/// <param name="repeatPressIntervalTime"></param>
		public void SetRepeatPressIntervalTime( float repeatPressIntervalTime )
		{
			if( repeatPressIntervalTime >  0 )
			{
				m_RepeatPressIntervalTime = repeatPressIntervalTime ;
			}
		}

		//----------------------
		// LongPress		

		/// <summary>
		/// ビューを長押しした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView> OnLongPressAction ;
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnLongPress( string identity, UIView view ) ;

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲート
		/// </summary>
		public OnLongPress OnLongPressDelegate ;

		// 内部リスナー(毎フレーム呼び出される)
		private void ProcessLongPress()
		{
			if( m_LongPressExecuted == false && m_LongPressTimer >  0 )
			{
				if( m_LongPressLimitDistance >  0 )
				{
					// 移動による無効化判定が有効になっている
					( _, var movePosition ) = GetSinglePointerInCanvas() ;

					float distance = ( m_LongPressBasePosition - movePosition ).magnitude ;

					if( distance >  m_LongPressLimitDistance )
					{
						// 以後はこの長押しは無効化
						m_LongPressExecuted = true ;
						m_LongPressTimer	= 0 ;
						return ;
					}
				}

				float deltaTime = Time.realtimeSinceStartup - m_LongPressTimer ;

				if( deltaTime >= m_LongPressDecisionTime )
				{
					m_LongPressExecuted = true ;	// 押し続けている扱いとする
					m_LongPressTimer	= 0 ;

					OnLongPressInner() ;
				}
			}
		}

		/// <summary>
		/// プレスを強制的に実行する
		/// </summary>
		public void ExecuteLongPress()
		{
			OnLongPressInner() ;
		}

		// 内部リスナー
		private void OnLongPressInner()
		{
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			// 最初に長押しと判定された時だけコールバックを呼ぶ
			OnLongPressAction?.Invoke( identity, this ) ;
			OnLongPressDelegate?.Invoke( identity, this ) ;

			if( TryGetComponent<UITransition>( out var transition ) == true )
			{
				transition.OnPointerUp( null ) ;
			}
		}
		
		/// <summary>
		/// ビューを長押しした際に呼び出されるアクションを設定する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onLongPress">アクションメソッド</param>
		public void SetOnLongPress( Action<string, UIView> onLongPressAction, float longPressDecisionTime = 0.75f, float longPressLimitDistance = 0 )
		{
			OnLongPressAction = onLongPressAction ;

			m_LongPressDecisionTime  = longPressDecisionTime ;
			m_LongPressLimitDistance = longPressLimitDistance ;
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを追加する → OnLongPress( string identity, UIView view )
		/// </summary>
		/// <param name="onLongPressDelegate">デリゲートメソッド</param>
		[Obsolete( "Not Use" )]
		public void AddOnLongPress( OnLongPress onLongPressDelegate, float longPressDecisionTime = 0.75f, float longPressLimitDistance = 16 )
		{
			OnLongPressDelegate += onLongPressDelegate ;

			// 最後ら設定したものが有効になってしまう(本当はこの処理はまずいので Add 系は削除すべきか)
			m_LongPressDecisionTime  = longPressDecisionTime ;
			m_LongPressLimitDistance = longPressLimitDistance ;
		}

		/// <summary>
		/// ビューを長押しした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onLongPressDelegate">デリゲートメソッド</param>
		[Obsolete( "Not Use" )]
		public void RemoveOnLongPress( OnLongPress onLongPressDelegate )
		{
			OnLongPressDelegate -= onLongPressDelegate ;
		}

		//----------------------
		// Click

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView> OnClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnClick( string identity, UIView view ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnClick OnClickDelegate ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクションを設定する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClick">アクションメソッド</param>
		public void SetOnClick( Action<string, UIView> onClickAction )
		{
			OnClickAction = onClickAction ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void AddOnClick( OnClick onClickDelegate )
		{
			OnClickDelegate += onClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void RemoveOnClick( OnClick onClickDelegate )
		{
			OnClickDelegate -= onClickDelegate ;
		}

		//-----------

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action OnSimpleClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnSimpleClick() ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnSimpleClick OnSimpleClickDelegate ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクションを設定する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClick">アクションメソッド</param>
		public void SetOnSimpleClick( Action onSimpleClickAction )
		{
			OnSimpleClickAction = onSimpleClickAction ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnClick( string identity, UIView view )
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void AddOnSimpleClick( OnSimpleClick onSimpleClickDelegate )
		{
			OnSimpleClickDelegate += onSimpleClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onClickDelegate">デリゲートメソッド</param>
		public void RemoveOnSimpleClick( OnSimpleClick onSimpleClickDelegate )
		{
			OnSimpleClickDelegate -= onSimpleClickDelegate ;
		}

		//-----------

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
			//----------------------------------
			// このクリックが有効か判定する

			if( CanClickExecution() == false )
			{
				// 無効
				return ;
			}

			//----------------------------------

			m_Click = true ;
			m_ClickCountTime = Time.frameCount ;

			m_WaitForClick = true ;

			if( gameObject.activeInHierarchy == false )
			{
				return ;	// アクティブな場合のみ有効(非アクティブになった際のリリースでクリック判定されてしまうのを防ぐ
			}

			if( OnClickAction != null || OnClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnClickAction?.Invoke( identity, this ) ;
				OnClickDelegate?.Invoke( identity, this ) ;
			}

			if( OnSimpleClickAction != null || OnSimpleClickDelegate != null )
			{
				OnSimpleClickAction?.Invoke() ;
				OnSimpleClickDelegate?.Invoke() ;
			}
		}
		
		//----------------------
		// SmartClick

		/// <summary>
		/// ビューをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, int, Vector2, Vector2> OnSmartClickAction ;
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="count">クリック種別(1=シングル・2=ダブル)</param>
		/// <param name="basePosition">最初のカーソル座標</param>
		/// <param name="movePosition">最後のカーソル座標</param>
		public delegate void OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition ) ;

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnSmartClick OnSmartClickDelegate ;
	
		// 内部リスナー
		private void OnSmartClickInner( int count, Vector2 basePosition, Vector2 movePosition )
		{
			if( gameObject.activeInHierarchy == false )
			{
				return ;	// アクティブな場合のみ有効(非アクティブになった際のリリースでクリック判定されてしまうのを防ぐ
			}

			if( OnSmartClickAction != null || OnSmartClickDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnSmartClickAction?.Invoke( identity, this, count, basePosition, movePosition ) ;
				OnSmartClickDelegate?.Invoke( identity, this, count, basePosition, movePosition ) ;
			}
		}
		
		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void SetOnSmartClick( Action<string, UIView, int, Vector2, Vector2> onSmartClickAction )
		{
			OnSmartClickAction  = onSmartClickAction ;
		}
		
		/// <summary>
		/// スマートクリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="releaseLimitTime">シングルクリックと判定される押してから離すまでの時間(秒)[デフォルト0.5秒]</param>
		/// <param name="secondPressLimitTime">ダブルクリックと判定される離してから押すまでの時間(秒)[デフォルト0.25秒]</param>
		/// <param name="limitDistance">クリックと判定される限界の移動量[デフォルト30]</param>
		public void SetSmartClickParameter(  float releaseLimitTime, float secondPressLimitTime, float limitDistance )
		{
			if( releaseLimitTime >= 0 )
			{
				m_SmartClickReleaseLimitTime = releaseLimitTime ;
			}
			if( secondPressLimitTime >= 0 )
			{
				m_SmartClickSecondPressLimitTime = secondPressLimitTime ;
			}
			if( limitDistance >= 0 )
			{
				m_SmartClickLimitDistance = limitDistance ;	// 最初に押した位置からどの程度の距離まで有効か
			}
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを追加する → OnSmartClick( string identity, UIView view, int count, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void AddOnSmartClick( OnSmartClick onSmartClickDelegate )
		{
			OnSmartClickDelegate += onSmartClickDelegate ;
		}

		/// <summary>
		/// ビューをクリックした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onSmartClickDelegate">デリゲートメソッド</param>
		public void RemoveOnSmartClick( OnSmartClick onSmartClickDelegate )
		{
			OnSmartClickDelegate -= onSmartClickDelegate ;
		}
		
		//----------------------
		// Drag

		/// <summary>
		/// ビューをドラッグした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, PointerState, Vector2, Vector2> OnDragAction ;

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="state">ドラッグの状態</param>
		/// <param name="basePosition">ドラッグ開始座標</param>
		/// <param name="movePosition">ドラッグ現在座標</param>
		public delegate void OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition ) ;

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲート
		/// </summary>
		public OnDrag OnDragDelegate ;
	
		// 内部リスナー
		private void OnDragInner( PointerState state, Vector2 basePosition, Vector2 movePosition )
		{
			if( OnDragAction != null || OnDragDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnDragAction?.Invoke( identity, this, state, basePosition, movePosition ) ;
				OnDragDelegate?.Invoke( identity, this, state, basePosition, movePosition ) ;
			}
		}
		
		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void SetOnDrag( Action<string, UIView, PointerState, Vector2, Vector2> onDragAction )
		{
			OnDragAction  = onDragAction ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを追加する → OnDrag( string identity, UIView view, PointerState state, Vector2 basePosition, Vector2 movePosition )
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void AddOnDrag( OnDrag onDragDelegate )
		{
			OnDragDelegate += onDragDelegate ;
		}

		/// <summary>
		/// ビュー上でドラッグした際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onDragDelegate"></param>
		public void RemoveOnDrag( OnDrag onDragDelegate )
		{
			OnDragDelegate -= onDragDelegate ;
		}

		//----------------------
		// Flick

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, Vector2, Vector2> OnFlickAction ;
		
		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="dstance">フリック移動量</param>
		/// <param name="basePosition">フリック開始座標</param>
		public delegate void OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition ) ;

		/// <summary>
		/// ビューをフリックした際に呼び出されるデリゲート
		/// </summary>
		public OnFlick OnFlickDelegate ;

		// 内部リスナー
		private void OnFlickInner( Vector2 distance, Vector2 basePosition )
		{
			if( OnFlickAction != null || OnFlickDelegate != null )
			{
				if( Input.touchCount >= 2 )
				{
					return ;	// ２点以上タッチされていたら無視する
				}

				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnFlickAction?.Invoke( identity, this, distance, basePosition ) ;
				OnFlickDelegate?.Invoke( identity, this, distance, basePosition ) ;
			}
		}
		
		/// <summary>
		/// フリックの判定用パラメータを設定する
		/// </summary>
		/// <param name="decisionLimitTime">フリックと判定されるため押してから離すまでの最大時間(秒)[デフォルト0.5秒]</param>
		/// <param name="decisionDistance">フリックと判定されるための押してから離すまでの最小移動距離割合[デフォルト60]</param>
		public void SetFlickParameter( float decisionLimitTime, float decisionDistance )
		{
			m_FlickDecisionLimitTime	= decisionLimitTime ;	// この時間以内に離さないとフリックとはみなされない
			m_FlickDecisionDistance		= decisionDistance ;	// この距離以上移動していないとフリックとはみなされない
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを設定する → OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition )
		/// </summary>
		/// <param name="onFlickAction">アクションメソッド</param>
		public void SetOnFlick( Action<string, UIView, Vector2, Vector2> onFlickAction )
		{
			OnFlickAction			= onFlickAction ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを追加する → OnFlick( string identity, UIView view, Vector2 distance, Vector2 basePosition )
		/// </summary>
		/// <param name="onFlickDelegate">デリゲートメソッド</param>
		public void AddOnFlick( OnFlick onFlickDelegate )
		{
			OnFlickDelegate += onFlickDelegate ;
		}

		/// <summary>
		/// ビューをフリックした際に呼び出されるアクションを削除する
		/// </summary>
		/// <param name="onFlickDelegate">デリゲートメソッド</param>
		public void RemoveOnFlick( OnFlick onFlickDelegate )
		{
			OnFlickDelegate -= onFlickDelegate ;
		}

		//----------------------
		// Pinch

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, PointerState, float, float, Vector2, Vector2> OnPinchAction ;
		
		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="touchState">タッチ情報が格納された配列</param>
		public delegate void OnPinch( string identity, UIView view, PointerState state, float ratio, float distance, Vector2 p0, Vector2 p1 ) ;

		/// <summary>
		/// ピンチ( ２箇所限定)があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnPinch OnPinchDelegate ;

		private int m_PinchStates = 0 ;
		private float m_PinchDistanceBase = 0 ;

		// 内部リスナー
		private void OnPinchInner( int states, Vector2[] pointers )
		{
			int i, l = pointers.Length ;
			int c0 = 0, c1 = 0 ;
			Vector2[] p = { Vector2.zero, Vector2.zero } ;

			// タッチ数を数える

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( m_PinchStates & ( 1 << i ) ) != 0 )
				{
					c0 ++ ;
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( states & ( 1 << i ) ) != 0 )
				{
					if( c1 <  2 )
					{
						p[ c1 ] = pointers[ i ] ;
					}

					c1 ++ ;
				}
			}

			m_PinchStates = states ;

			//----------------------------------

			PointerState state = PointerState.None ;

			if( c1 == 2 )
			{
				// ピンチ有効
				if( c0 != 2 || states != m_PinchStates )
				{
					// 新規
					state = PointerState.Start ;
				}
				else
				{
					// 継続
					state = PointerState.Moving ;
				}
			}
			else
			{
				// ピンチ無効
				if( c0 == 2 )
				{
					state = PointerState.End ;
				}
			}

			float ratio = 0 ;
			float distance = 0 ;

			if( state == PointerState.Start )
			{
				// 開始
				distance = ( p[ 1 ] - p[ 0 ] ).magnitude ;

				if( distance >  0 )
				{
					ratio = 1 ;
					m_PinchDistanceBase = distance ;
				}
				else
				{
					// 無効
					m_PinchDistanceBase = 0 ;
					state = PointerState.None ;
				}
			}
			else
			if( state == PointerState.Moving )
			{
				// 継続
				if( m_PinchDistanceBase >  0 )
				{
					distance = ( p[ 1 ] - p[ 0 ] ).magnitude ;
					ratio = distance / m_PinchDistanceBase ;
				}
				else
				{
					// 異常
					state = PointerState.None ;
				}
			}
			else
			if( state == PointerState.End )
			{
				// 終了
				if( m_PinchDistanceBase >  0 )
				{
					m_PinchDistanceBase = 0 ;
				}
				else
				{
					// 異常
					state = PointerState.None ;
				}
			}

			if( state != PointerState.None )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnPinchAction?.Invoke( identity, this, state, ratio, distance, p[ 0 ], p[ 1 ] ) ;
				OnPinchDelegate?.Invoke( identity, this, state, ratio, distance, p[ 0 ], p[ 1 ] ) ;
			}
		}
		
		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるアクションを設定する → OnPinchDelegate( string identity, UIView view, PointerState state, float ratio, float distance, Vector2 p0, Vector2 p1 )
		/// </summary>
		/// <param name="onTouchAction">アクションメソッド</param>
		public void SetOnPinch( Action<string, UIView, PointerState, float, float, Vector2, Vector2> onPinchAction )
		{
			OnPinchAction = onPinchAction ;
		}

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onPinchDelegate">デリゲートメソッド</param>
		public void AddOnPinch( OnPinch onPinchDelegate )
		{
			OnPinchDelegate += onPinchDelegate ;
		}

		/// <summary>
		/// ピンチ(２箇所限定)があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onPinchDelegate">デリゲートメソッド</param>
		public void RemoveOnPinch( OnPinch onPinchDelegate )
		{
			OnPinchDelegate -= onPinchDelegate ;
		}

		//----------------------
		// Touch

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクション
		/// </summary>
		public Action<string, UIView, TouchState[]> OnTouchAction ;
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="touchState">タッチ情報が格納された配列</param>
		public delegate void OnTouch( string identity, UIView view, TouchState[] touchState ) ;	

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnTouch OnTouchDelegate ;

		// 内部リスナー
		private void OnTouchInner( int states, Vector2[] pointers )
		{
			int i, l = pointers.Length ;
			for( i = 0 ; i < l ; i ++ )
			{
				if( ( states & ( 1 << i ) ) != 0 )
				{
					if( m_TouchState[ i ] == null )
					{
						// 押された
						m_TouchState[ i ] = new TouchState( i, i, pointers[ i ], PointerState.Start ) ;
					}
					else
					{
						// 押されている
						if( m_TouchState[ i ].State == PointerState.Start )
						{
							// 状態変更
							m_TouchState[ i ].State = PointerState.Moving ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.Moving )
						{
							// 位置更新
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.End )
						{
							// 状態変更
							m_TouchState[ i ].State = PointerState.Start ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
					}
				}
				else
				{
					if( m_TouchState[ i ] != null )
					{
						if( m_TouchState[ i ].State == PointerState.Start || m_TouchState[ i ].State == PointerState.Moving )
						{
							// 離された
							m_TouchState[ i ].State = PointerState.End ;
							m_TouchState[ i ].Position = pointers[ i ] ;
						}
						else
						if( m_TouchState[ i ].State == PointerState.End )
						{
							// 破棄
							m_TouchState[ i ] = null ;
						}
					}
				}
			}

			//----------------------------------------------------------

			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			// 存在するもののみ抽出する
			m_TouchStateExchange.Clear() ;
			l = m_TouchState.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_TouchState[ i ] != null )
				{
					m_TouchStateExchange.Add( m_TouchState[ i ] ) ;
				}
			}

			OnTouchAction?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
			OnTouchDelegate?.Invoke( identity, this, m_TouchStateExchange.ToArray() ) ;
		}
		
		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるアクションを設定する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onTouchAction">アクションメソッド</param>
		public void SetOnTouch( Action<string, UIView, TouchState[]> onTouchAction )
		{
			OnTouchAction = onTouchAction ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onTouchDelegate">デリゲートメソッド</param>
		public void AddOnTouch( OnTouch onTouchDelegate )
		{
			OnTouchDelegate += onTouchDelegate ;
		}

		/// <summary>
		/// タッチ(１箇所以上)があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onTouchDelegate">デリゲートメソッド</param>
		public void RemoveOnTouch( OnTouch onTouchDelegate )
		{
			OnTouchDelegate -= onTouchDelegate ;
		}

		//-------------------------------------------------------------------
		// Wheel

		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるアクション
		/// </summary>
		public Action<string,UIView,float> OnWheelAction ;
		
		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		public delegate void OnWheel( string identity, UIView view, float value ) ;

		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるデリゲートの定義
		/// </summary>
		public OnWheel OnWheelDelegate ;

		// 内部処理
		private void ProcessWheel()
		{
			if( IsHover == true )
			{
				var wheelValue = InputAdapter.UIEventSystem.MouseScrollDelta.y ;

				if( wheelValue != 0 )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					OnWheelAction?.Invoke( identity, this, wheelValue ) ;
					OnWheelDelegate?.Invoke( identity, this, wheelValue ) ;
				}
			}
		}
		
		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるアクションを設定する → OnPinchDelegate( string identity, UIView view, PointerState state, float ratio, float distance, Vector2 p0, Vector2 p1 )
		/// </summary>
		/// <param name="onWheelAction">アクションメソッド</param>
		public void SetOnWheel( Action<string, UIView, float> onWheelAction )
		{
			OnWheelAction = onWheelAction ;
		}

		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるデリゲートを追加する → OnTouchDelegate( string tIdentity, UIView tView, TouchState[] tTouchState )
		/// </summary>
		/// <param name="onWheelDelegate">デリゲートメソッド</param>
		public void AddOnWheel( OnWheel onWheelDelegate )
		{
			OnWheelDelegate += onWheelDelegate ;
		}

		/// <summary>
		/// ホイールの状態に変化があった際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onWheelDelegate">デリゲートメソッド</param>
		public void RemoveOnWheel( OnWheel onWheelDelegate )
		{
			OnWheelDelegate -= onWheelDelegate ;
		}

		//--------------------------------------------------------------------

		private readonly Dictionary<EventTriggerType, Action<string,UIView,EventTriggerType>> m_EventTriggerCallbackList = new () ;
		private readonly Dictionary<EventTriggerType, EventTrigger.Entry>                     m_EventTriggerEntryList    = new () ;

		/// <summary>
		/// EventTrigger のコールバックメソッドを登録する
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="typeArray"></param>
		/// <returns></returns>
		public bool AddEventTriggerCallback( Action<string,UIView,EventTriggerType> callback, params EventTriggerType[] typeArray )
		{
			if( callback == null || typeArray == null || typeArray.Length == 0 )
			{
				// 引数が不正
#if UNITY_EDITOR
				Debug.LogWarning( name + " : " + "Bad parameter" ) ;
#endif
				return false ;
			}

			
			EventTrigger eventTrigger = CEventTrigger ;
			if( eventTrigger == null )
			{
				// イベントトリガーがアタッチされていない
				Debug.LogWarning( name + " : " + "Event Trigger not attached." ) ;
				return false ;
			}

			eventTrigger.triggers ??= new List<EventTrigger.Entry>() ;
		
			int i ;
			EventTriggerType type ;
			EventTrigger.Entry entry ;

			for( i  = 0 ; i <  typeArray.Length ; i ++ )
			{
				// 既に登録されている場合は上書きになる
				type = typeArray[ i ] ;

				if( m_EventTriggerCallbackList.ContainsKey( type ) == false )
				{
					// 新規登録
					m_EventTriggerCallbackList.Add( type, callback ) ;
				}
				else
				{
					// 上書登録
					m_EventTriggerCallbackList[ type ] = callback ;
				}

				//-------------

				entry = null ;

				// エントリーが既に登録されているかを確認する
				if( m_EventTriggerEntryList.ContainsKey( type ) == true )
				{
					// 既に登録されている
					entry = m_EventTriggerEntryList[ type ] ;

					if( eventTrigger.triggers.Contains( entry ) == true )
					{
						// 登録済みなので一度破棄しておく
						eventTrigger.triggers.Remove( entry ) ;
					}
				}
				else
				{
					// 登録されていないので新規にエントリーを生成する
					UnityAction<BaseEventData> action = type switch
					{
						EventTriggerType.PointerEnter				=> OnPointerEnterInner,
						EventTriggerType.PointerExit				=> OnPointerExitInner,
						EventTriggerType.PointerDown				=> OnPointerDownInner,
						EventTriggerType.PointerUp					=> OnPointerUpInner,
						EventTriggerType.PointerClick				=> OnPointerClickInner,
						EventTriggerType.Drag						=> OnDragInner,
						EventTriggerType.Drop						=> OnDropInner,
						EventTriggerType.Scroll						=> OnScrollInner,
						EventTriggerType.UpdateSelected				=> OnUpdateSelectedInner,
						EventTriggerType.Select						=> OnSelectInner,
						EventTriggerType.Deselect					=> OnDeselectInner,
						EventTriggerType.Move						=> OnMoveInner,
						EventTriggerType.InitializePotentialDrag	=> OnInitializePotentialDragInner,
						EventTriggerType.BeginDrag					=> OnBeginDragInner,
						EventTriggerType.EndDrag					=> OnEndDragInner,
						EventTriggerType.Submit						=> OnSubmitInner,
						EventTriggerType.Cancel						=> OnCancelInner,
						_ => null,
					} ;

					if( action != null )
					{
						entry = new EventTrigger.Entry()
						{
							eventID = type
						} ;
						entry.callback.AddListener( action ) ;

						m_EventTriggerEntryList.Add( type, entry ) ;
					}
				}

				if( entry != null )
				{
					// 改めてエントリーを登録する
					eventTrigger.triggers.Add( entry ) ;
				}
			}
			
			return true ;
		}

		private void OnPointerEnterInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerEnter, data ) ;
		}

		private void OnPointerExitInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerExit, data ) ;
		}

		private void OnPointerDownInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerDown, data ) ;
		}

		private void OnPointerUpInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerUp, data ) ;
		}

		private void OnPointerClickInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.PointerClick, data ) ;
		}

		private void OnDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drag, data ) ;
		}

		private void OnDropInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Drop, data ) ;
		}

		private void OnScrollInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Scroll, data ) ;
		}

		private void OnUpdateSelectedInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.UpdateSelected, data ) ;
		}

		private void OnSelectInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Select,data ) ;
		}

		private void OnDeselectInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Deselect, data ) ;
		}

		private void OnMoveInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Move, data ) ;
		}

		private void OnInitializePotentialDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.InitializePotentialDrag, data ) ;
		}

		private void OnBeginDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.BeginDrag, data ) ;
		}

		private void OnEndDragInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.EndDrag, data ) ;
		}

		private void OnSubmitInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Submit, data ) ;
		}

		private void OnCancelInner( BaseEventData data )
		{
			InvokeEventTriggerCallback( EventTriggerType.Cancel, data ) ;
		}
	
		private void InvokeEventTriggerCallback( EventTriggerType type, BaseEventData data )
		{
			if( m_EventTriggerCallbackList.ContainsKey( type ) == false )
			{
				// コールバックが登録されていない
				return ;
			}
		
			string identity = Identity ;
			if( string.IsNullOrEmpty( identity ) == true )
			{
				identity = name ;
			}

			PointerEventData pointer = data as PointerEventData ;
			m_PointerId = pointer.pointerId ;
			m_PointerPosition = GetLocalPosition( pointer ) ;

			m_EventTriggerCallbackList[ type ]( identity, this, type ) ;
		}


		//-----------------------------------------------------------

		private int m_PointerId = -1 ;
		private Vector2 m_PointerPosition = Vector2.zero ;

		/// <summary>
		/// Pointer が Collider の内側にあるか判定する
		/// </summary>
		public bool IsPointerInside
		{
			get
			{
				Vector3 position ;

				if( m_PointerId == -1 )
				{
					position = InputAdapter.UIEventSystem.MousePosition ;
					position = GetLocalPosition( position ) ;
				}
				else
				{
					position = GetLocalPosition( m_PointerPosition ) ;
				}

				float x = position.x ;
				float y = position.y ;

				float w = this.Width ;
				float h = this.Height ;

				Vector2 p = Pivot ;

				float xMin = - ( w * p.x ) ;
				float xMax = w * ( 1.0f - p.x ) ;
				float yMin = - ( h * p.y ) ;
				float yMax = h * ( 1.0f - p.y ) ;

				if( x <  xMin || x >  xMax || y <  yMin || y >  yMax )
				{
					return false ;	// 外
				}

				return true ;	// 中
			}
		}

		// そのＵＩ上の座標を取得する
		protected Vector2 GetLocalPosition( PointerEventData pointer )
		{
			return GetLocalPosition( pointer.position ) ;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// 現在タッチされている数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetTouchCount()
		{
			int count = Input.touchCount ;

			if( count == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				int i ;
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( InputAdapter.UIEventSystem.GetMouseButton( i ) == true )
					{
						count = 1 ;
					}
				}
			}

			return count ;
		}


		private bool	m_SingleTouchState = false ;
		private int		m_SingleTouchFingerId = 0 ;

		/// <summary>
		/// レイキャストのブロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPosition"></param>
		/// <returns></returns>
		public ( int, Vector2 ) GetSinglePointer()
		{
			int i ;
			int state = 0 ;
			Vector2 pointer = Vector2.zero ;
			
			if( Input.touchCount == 1 )
			{
				// 押された
				Touch touch = Input.GetTouch( 0 ) ;

				if( m_SingleTouchState == false )
				{
					m_SingleTouchState = true ;
					m_SingleTouchFingerId = touch.fingerId ;

					pointer = touch.position ;

					state = 1 ;
				}
				else
				{
					if( m_SingleTouchFingerId == touch.fingerId )
					{
						pointer = touch.position ;

						state = 1 ;
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
				state = 0 ;
			}
			else
			{
				// 離された
				m_SingleTouchState = false ;

				state = 0 ;
			}

			//----------------------------------------------------------

			if( state == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( InputAdapter.UIEventSystem.GetMouseButton( i ) == true )
					{
						state |= ( 1 << i ) ;
					}
				}
				if( state != 0 )
				{
					pointer = InputAdapter.UIEventSystem.MousePosition ;
				}
			}

			//----------------------------------------------------------

			if( state != 0 )
			{
				// ローカル座標への変換を行う
				pointer = GetLocalPosition( pointer ) ;
			}

			return ( state, pointer ) ;
		}

		/// <summary>
		/// レイキャストのブロックキングに関わらず現在のタッチ情報を取得する(キャンバススケール)
		/// </summary>
		/// <param name="rPosition"></param>
		/// <returns></returns>
		public ( int, Vector2 ) GetSinglePointerInCanvas()
		{
			int i ;
			int state = 0 ;
			Vector2 pointer = Vector2.zero ;
			
			if( Input.touchCount == 1 )
			{
				// 押された
				Touch touch = Input.GetTouch( 0 ) ;

				if( m_SingleTouchState == false )
				{
					m_SingleTouchState = true ;
					m_SingleTouchFingerId = touch.fingerId ;

					pointer = touch.position ;

					state = 1 ;
				}
				else
				{
					if( m_SingleTouchFingerId == touch.fingerId )
					{
						pointer = touch.position ;

						state = 1 ;
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
				state = 0 ;
			}
			else
			{
				// 離された
				m_SingleTouchState = false ;

				state = 0 ;
			}

			//----------------------------------------------------------

			if( state == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( InputAdapter.UIEventSystem.GetMouseButton( i ) == true )
					{
						state |= ( 1 << i ) ;
					}
				}
				if( state != 0 )
				{
					pointer = InputAdapter.UIEventSystem.MousePosition ;
				}
			}

			//----------------------------------------------------------

			if( state != 0 )
			{
				// キャンパス座標への変換を行う

				var canvas = GetParentCanvas() ;
				if( canvas != null )
				{
					if( canvas.TryGetComponent<RectTransform>( out var rectTransform ) == true )
					{
						float sw = Screen.width  * 0.5f ;
						float sh = Screen.height * 0.5f ;
						float cw = rectTransform.sizeDelta.x * 0.5f ;
						float ch = rectTransform.sizeDelta.y * 0.5f ;

						pointer.x -= sw ;
						pointer.x *= ( cw / sw ) ;

						pointer.y -= sh ;
						pointer.y *= ( ch / sh ) ;
					}
					else
					{
						// キャンバスの RectTransform が見つからない場合は仕方がないのでローカル座標への変換を行う(基本ここに来る事はありえない)
						pointer = GetLocalPosition( pointer ) ;
					}
				}
				else
				{
					// キャンバスが見つからない場合は仕方がないのでローカル座標への変換を行う(基本ここに来る事はありえない)
					pointer = GetLocalPosition( pointer ) ;
				}
			}
		
			return ( state, pointer ) ;
		}


		private readonly bool[]	m_MultiTouchState    = new bool[ 10 ] ;
		private readonly int[]	m_MultiTouchFingerId = new int[ 10 ] ;

		private readonly bool[] m_MultiTouchEntries = new bool[ 10 ] ;
		private readonly Vector2[] m_MultiTouchPointers = new Vector2[ 10 ] ;

		/// <summary>
		/// レイキャストのブロックキングに関わらず現在のタッチ情報を取得する
		/// </summary>
		/// <param name="rPointer"></param>
		/// <returns></returns>
		public ( int, Vector2[] ) GetMultiPointer( bool isRaycastCheck = false )
		{
			int i, l ;
			int states = 0 ;

			int j, c, e ;

			l = m_MultiTouchState.Length ;

			c = Input.touchCount ;
			if( c >  0 )
			{
				for( i  = 0 ; i <  c ; i ++ )
				{
					// ＩＤが同じものを検査して存在するなら上書きする
					// ＩＤが同じものが存在しないなら新規に追加する

					var touch = Input.GetTouch( i ) ;
					var fingerId = touch.fingerId ;
					var position = touch.position ;

					bool isHit = true ;
					if( isRaycastCheck == true )
					{
						// このビューにレイキャストヒットしなければ無効
						if( IsRaycastHit( position ) == false )
						{
							isHit = false ;
						}
					}

					if( isHit == true )
					{
						e = -1 ;
						for( j  = 0 ; j <  l ; j ++ )
						{
							if( m_MultiTouchState[ j ] == true )
							{
								if( m_MultiTouchFingerId[ j ] == fingerId )
								{
									// 既に登録済み
									m_MultiTouchEntries[ j ] = true ;
									m_MultiTouchPointers[ j ] = position ;
									states |= ( 1 << j ) ;
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
							m_MultiTouchFingerId[ e ] = fingerId ;

							m_MultiTouchEntries[ e ] = true ;
							m_MultiTouchPointers[ e ] = position ;
							states |= ( 1 << e ) ;
						}
					}
				}
			}

			// 新規登録または上書更新が無かったスロットを解放する
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_MultiTouchEntries[ i ] == false )
				{
					m_MultiTouchState[ i ] = false ;
				}
			}

			if( states != 0 )
			{
				l = m_MultiTouchEntries.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_MultiTouchEntries[ i ] == true )
					{
						// 登録があったスロットのみローカル座標に変換する
						m_MultiTouchPointers[ i ] = GetLocalPosition( m_MultiTouchPointers[ i ] ) ;
					}
				}
			}

			//----------------------------------------------------------

			// マウスのダミー処理
			if( Input.touchCount == 0 )
			{
				// 本当にタッチが無い場合のみマウスの入力を使用する
				// 注意:タッチするとエミュレーション的に Input.GetMouseButton() が反応してしまう
				c = 0 ; j = -1 ;
				for( i  = 0 ; i <= 2 ; i ++ )
				{
					if( InputAdapter.UIEventSystem.GetMouseButton( i ) == true )
					{
						Vector2 position = InputAdapter.UIEventSystem.MousePosition ;

						bool isHit = true ;
						if( isRaycastCheck == true )
						{
							// このビューにレイキャストヒットしなければ無効
							if( IsRaycastHit( position ) == false )
							{
								isHit = false ;
							}
						}

						if( isHit == true )
						{
							states |= ( 1 << i ) ;
							m_MultiTouchEntries[ i ] = true ;
							m_MultiTouchPointers[ i ] = GetLocalPosition( position ) ;

							c ++ ;
							j = i ;
						}
					}
				}

				//-----------------------------------------------------------------------------------------
				// ピンチ動作のエミュレート(強制的に２点タッチに変える)

				// 注意:Unity Remote 5 を起動しているとキーボードの押下が取れない

#if UNITY_EDITOR
				if( c == 1 )
				{
					if( InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.LeftControl ) == true || InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.RightControl ) == true )
					{
						if( m_PinchEmulateMessage == false )
						{
							m_PinchEmulateMessage  = true ;
							Debug.Log( "<color=#00FF00>[UIView] CTRL + X => Pinch X | CTRL + Y => Pinch Y</color>" ) ;
						}
					}

					if( ( InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.LeftControl ) == true || InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.RightControl ) == true ) && InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.X ) == true )
					{
						j ++ ;
						states |= ( 1 << j ) ;
						m_MultiTouchPointers[ j ] = new Vector2( - m_MultiTouchPointers[ j - 1 ].x, m_MultiTouchPointers[ j - 1 ].y ) ;
					}
					if( ( InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.LeftControl ) == true || InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.RightControl ) == true ) && InputAdapter.UIEventSystem.GetKey( InputAdapter.KeyCodes.Y ) == true )
					{
						j ++ ;
						states |= ( 1 << j ) ;
						m_MultiTouchPointers[ j ] = new Vector2( m_MultiTouchPointers[ j - 1 ].x, - m_MultiTouchPointers[ j - 1 ].y ) ;
					}
				}
				else
				{
					m_PinchEmulateMessage = false ;
				}
#endif
			}

			//----------------------------------------------------------

			return ( states, m_MultiTouchPointers ) ;
		}

#if UNITY_EDITOR
		private bool m_PinchEmulateMessage = false ;
#endif

		/// <summary>
		/// スクリーン座標に該当するビュー上の座標を取得する
		/// </summary>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <returns></returns>
		public Vector2 GetLocalPosition( float screenX, float screenY )
		{
			return GetLocalPosition( new Vector2( screenX, screenY ) ) ;
		}

		/// <summary>
		/// スクリーン座標に該当するビュー上の座標を取得する
		/// </summary>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <returns></returns>
		public Vector2 GetLocalPosition( Vector2 screenPosition )
		{
			Vector2 viewPosition = screenPosition ;

			Canvas canvas = GetParentCanvas() ;
			if( canvas != null )
			{
				if( canvas.renderMode == RenderMode.ScreenSpaceOverlay )
				{
					viewPosition = transform.InverseTransformPoint( screenPosition ) ;
				}
				else
				if( canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace )
				{
					if( canvas.worldCamera != null && canvas.worldCamera.isActiveAndEnabled == true )
					{
						Vector2 canvasSize = GetCanvasSize() ;

						viewPosition = canvas.worldCamera.ScreenToWorldPoint( screenPosition ) ;

						viewPosition.x -= transform.position.x ;
						viewPosition.y -= transform.position.y ;

						if( canvas.worldCamera.orthographic == true )
						{
							float height = canvas.worldCamera.orthographicSize ;
							float k = ( canvasSize.y * 0.5f ) / height ;
							viewPosition.x *= k ;
							viewPosition.y *= k ;
						}
					}
					else
					{
						Vector2 canvasSize = GetCanvasSize() ;

						// キャンバス上の座標に変換する
						viewPosition.x = ( ( screenPosition.x / ( float )Screen.width  ) - 0.5f ) * canvasSize.x ;
						viewPosition.y = ( ( screenPosition.y / ( float )Screen.height ) - 0.5f ) * canvasSize.y ;

						Vector2 center = PositionInCanvas ;

						viewPosition -= center ;
					}
				}

				return viewPosition ;
			}

			return Vector2.zero ;
		}

		/// <summary>
		/// マウスのホイール値を取得する
		/// </summary>
		/// <returns></returns>
		public float GetWheelDistance()
		{
			 return InputAdapter.UIEventSystem.MouseScrollDelta.y ;
		}

		/// <summary>
		/// ピンチの距離を取得する
		/// </summary>
		/// <returns></returns>
		public float GetPinchDistance()
		{
			( int state, Vector2[] pointers ) = GetMultiPointer() ;
			if( state == 3 )
			{
				// 最初の２点限定
				return ( pointers[ 1 ] - pointers[ 0 ] ).magnitude ;
			}

			return 0 ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// バックキー連携
		/// </summary>
		[SerializeField]
		protected bool			m_BackKeyEnabled = false ;
		public    bool			  BackKeyEnabled
		{
			get
			{
				return m_BackKeyEnabled ;
			}
			set
			{
				m_BackKeyEnabled = value ;
			}
		}

		/// <summary>
		/// バックキー連携でレイキャストヒットを無視して反応するようにするかどうか
		/// </summary>
		[SerializeField]
		protected bool			m_IsBackKeyIgnoreRaycastTarget = false ;
		public    bool			  IsBackKeyIgnoreRaycastTarget
		{
			get
			{
				return m_IsBackKeyIgnoreRaycastTarget ;
			}
			set
			{
				m_IsBackKeyIgnoreRaycastTarget  = value ;
			}
		}

		// バックキーを処理する
		private void ProcessBackKey()
		{
			if( ActiveInHierarchy == true && IsAnyTweenPlayingInParents == false && ( ( IsBackKeyIgnoreRaycastTarget == false && Alpha >  0 ) || IsBackKeyIgnoreRaycastTarget == true ) && IsBackKeyAvailable() == true )
			{
				// バックキーは有効
				if( this is UIButton button )
				{
					if( button.Interactable == true )
					{
						button.ExecuteButtonClick() ;
					}
				}
				else
				if( this is UIImage image )
				{
					if( image.IsInteraction == true )
					{
						image.ExecuteClick() ;
					}
				}
			}
		}

		// バックキーが押せるか確認する
		private readonly PointerEventData		m_BK_EventDataCurrentPosition = new ( EventSystem.current ) ;
		private readonly List<RaycastResult>	m_BK_Results = new () ;

		// バックキーが現在有効な状態か確認する
		private bool IsBackKeyAvailable()
		{
			// バックキー対象のスクリーン座標を計算する
			( var backKeyPoints, var backKeyCenter ) = GetScreenArea( gameObject ) ;

			//----------------------------------

			// 一時的にレイキャストターゲットを有効化する(よって Graphic コンポーネント必須)
			bool raycastTarget = true ;
			if( IsBackKeyIgnoreRaycastTarget == true && RaycastTarget == false )
			{
				raycastTarget = RaycastTarget ;
				RaycastTarget = true ;
			}

			//----------------------------------------------------------

			bool isAvailable = false ;

			// レイキャストを実行しヒットする対象を検出する
			m_BK_EventDataCurrentPosition.position = backKeyCenter ;
			m_BK_Results.Clear() ;
			EventSystem.current.RaycastAll( m_BK_EventDataCurrentPosition, m_BK_Results ) ;

			// ヒットしない事は基本的にありえない
			foreach( var result in m_BK_Results )
			{
				if( result.gameObject == gameObject )
				{
					// 有効
					isAvailable = true ;
					break ;
				}
				else
				{
					// レイキャストヒット対象がバックキーそのものでなくても親にバックキーが含まれていたらスルーする
					if( IsContainParent( gameObject, result.gameObject ) == false )
					{
						// 親では無い
						( var blockerPoints, var blockerCenter ) = GetScreenArea( result.gameObject ) ;
						if( IsCompleteBlocking( backKeyPoints, blockerPoints ) == true )
						{
							// 無効
							isAvailable = false ;
							break ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// レイキャスト無効でもヒット判定を有効にしていた場合は設定を元に戻す
			if( IsBackKeyIgnoreRaycastTarget == true && raycastTarget == false )
			{
				RaycastTarget = raycastTarget ;
			}

			//----------------------------------

			// バックキーが有効か無効か返す
			return isAvailable ;
		}

		// スクリーン上の矩形範囲を取得する
		private ( Vector2[], Vector2 ) GetScreenArea( GameObject go )
		{
			if( ( go.transform is RectTransform rt ) == false )
			{
				// 取得出来ない
				throw new Exception( "Not foud rectTransform." ) ;
			}

			//----------------------------------

			// 横幅・縦幅
			float tw = rt.rect.width ;
			float th = rt.rect.height ;

			// レイキャストパディング
			Vector4 raycastPadding = Vector4.zero ;

			if( go.TryGetComponent<UnityEngine.UI.Image>( out var image ) == true )
			{
				raycastPadding = image.raycastPadding ;
			}

			float tx0 = ( tw * ( 0 - rt.pivot.x ) ) + raycastPadding.x ;	// x = left
			float ty0 = ( th * ( 0 - rt.pivot.y ) ) + raycastPadding.y ;	// y = bottom
			float tx1 = ( tw * ( 1 - rt.pivot.x ) ) - raycastPadding.z ;	// z = right
			float ty1 = ( th * ( 1 - rt.pivot.y ) ) - raycastPadding.w ;	// w = top

			// 角の座標(まだローカルの２次元)	※順番は右回りである事に注意(Ｚ型ではない)
			Vector2[] points = new Vector2[ 4 ]
			{
				new ( tx0, ty0 ),
				new ( tx1, ty0 ),
				new ( tx1, ty1 ),
				new ( tx0, ty1 ),
			} ;

			// ローカル座標をワールド座標に変換(ローカルのローテーションとスケールも反映)
			int i, l = points.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// ローテーションとスケールも考慮するため個別に分ける
				points[ i ] = rt.TransformPoint( points[ i ] ) ;
			}

			//----------------------------------

			Camera targetCamera ;

			var parentCanvas = rt.transform.GetComponentInParent<Canvas>() ;
			if( parentCanvas != null )
			{
				if( parentCanvas.worldCamera != null )
				{
					// Screen Space - Camera
					targetCamera = parentCanvas.worldCamera ;
				}
				else
				{
					// Screen Space - Overlay
					targetCamera = Camera.main ;
				}
			}
			else
			{
				throw new Exception( "Not foud canvas." ) ;
			}

			// スクリーン座標に変換する
			Vector2 center = Vector2.zero ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				points[ i ] = RectTransformUtility.WorldToScreenPoint( targetCamera, points[ i ] ) ;
				center += points[ i ] ;
			}

			center /= l ;

			return ( points, center ) ;
		}

		// ブロッカーの親にバックキーが踏まれているかどうか
		private bool IsContainParent( GameObject backKey, GameObject blocker )
		{
			while( blocker.transform.parent != null )
			{
				blocker = blocker.transform.parent.gameObject ;

				if( blocker == backKey )
				{
					// 含まれている
					return true ;
				}
			}

			// 含まれていない
			return false ;
		}

		// バックキーがブロッカーの内側に完全に隠されているか確認する
		private bool IsCompleteBlocking( Vector2[] backKeyPoints, Vector2[] blockerPoints )
		{
			int oi, ol = blockerPoints.Length ;
			int ii, il = backKeyPoints.Length ;

			for( oi = 0 ; oi <  ol ; oi ++ )
			{
				var op0 = blockerPoints[ oi ] ;
				var op1 = blockerPoints[ ( oi + 1 ) % ol ] ;

				for( ii = 0 ; ii <  il ; ii ++ )
				{
					var ip = backKeyPoints[ ii ] ;

					// 外積を用いて表裏判定を行う(Cross)
					var v0 = op1 - op0 ;
					var v1 = ip  - op0 ;
					var cross = ( v0.x * v1.y ) - ( v0.y * v1.x ) ;

					if( cross <  0 )
					{
						// 外側にある
						return false ;
					}
				}
			}

			// 全て内側にある
			return true ;
		}

		//--------------------------------------------------------------------

		/// <summary>
		/// GameObject を　Active にする(ショートカット)
		/// </summary>
		/// <param name="state"></param>
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
		/// GameObject の ActiveInHierarchy の有無(ショートカット)
		/// </summary>
		public bool ActiveInHierarchy
		{
			get
			{
				return gameObject.activeInHierarchy ;
			}
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="flag"></param>
		public void SetParent( Transform parent, bool flag )
		{
			transform.SetParent( parent, flag ) ;
		}

		/// <summary>
		/// 親を設定する(ショートカット)
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="flag"></param>
		public void SetParent( UIView parentView, bool flag )
		{
			transform.SetParent( parentView.transform, flag ) ;
		}

		/// <summary>
		/// UI の表示順番(ショートカット)
		/// </summary>
		public int SiblingIndex
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
			return SiblingIndex ;
		}

		/// <summary>
		/// UI の表示順番を設定する
		/// </summary>
		/// <param name="index"></param>
		public void SetSiblingIndex( int index )
		{
			SiblingIndex = index ;
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
		/// <param name="reverse"></param>
		public void SortChildByZ( bool reverse = false )
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

			if( reverse == false )
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
		/// ゲームオブジェクトの複製を行う(親や姿勢は引き継ぐ)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public GameObject Duplicate()
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone ;
		} 
		
		/// <summary>
		/// ゲームオブジェクトを複製し指定のコンポーネントを取得する(親や姿勢は引き継ぐ)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Duplicate<T>() where T : UnityEngine.Component
		{
			GameObject clone = Instantiate( gameObject, transform.parent ) ;
			return clone.GetComponent<T>() ;
		}

		/// <summary>
		/// ゲームオブジェクトを破棄する
		/// </summary>
		public void Destroy()
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				DestroyImmediate( gameObject ) ;
			}
			else
#endif
			{
				Destroy( gameObject ) ;
			}
		}

		/// <summary>
		/// 指定したビューからの相対的な距離を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public Vector2 GetDistance( UIView baseView )
		{
			Vector2 t1 = transform.position ;
			Vector2 t0 = baseView.transform.position ;

			return t1 - t0 ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(X)を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public float GetDistanceX( UIView baseView )
		{
			return GetDistance( baseView ).x ;
		}

		/// <summary>
		/// 指定したビューからの相対的な距離(Y)を取得する
		/// </summary>
		/// <param name="baseView"></param>
		/// <returns></returns>
		public float GetDistanceY( UIView baseView )
		{
			return GetDistance( baseView ).y ;
		}

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

		//-------------------------------------------------------------------
		// プレス待ち

		private bool m_WaitForPress = false ;

		public AsyncState WaitForPress()
		{
			if( m_WaitForPress == true )
			{
				return null ;
			}

			m_WaitForPress = false ;
			var state = new AsyncState( this ) ;
			StartCoroutine( WaitForPress_Private( state ) ) ;

			return state ;
		}

		private IEnumerator WaitForPress_Private( AsyncState state )
		{
			yield return new WaitWhile( () => m_WaitForPress == false ) ;

			m_WaitForPress = false ;
			state.IsDone = true ;
		}

		//-------------------------------------------------------------------
		// クリック待ち

		private bool m_WaitForClick = false ;

		public AsyncState WaitForClick()
		{
			if( m_WaitForClick == true )
			{
				return null ;
			}

			m_WaitForClick = false ;
			var state = new AsyncState( this ) ;
			StartCoroutine( WaitForClick_Private( state ) ) ;

			return state ;
		}

		private IEnumerator WaitForClick_Private( AsyncState state )
		{
			while( m_WaitForClick == false )
			{
				yield return null ;
			}

			m_WaitForClick = false ;
			state.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------
		// アニメーションの再生

		// アニメーション再生中の待機オブジェクト
//		private AsyncState m_ActiveAnimation = null ;

		// アニメーションの中断要求
//		private bool m_BreakAnimation = false ;

		public class ActiveAnimation
		{
			public AsyncState	State ;
			public bool			Break ;

			public string		Name ;		// ステート名
			public float		Duration ;	// 再生時間
			public bool			IsLoop ;	// ループするか
		}

		public class ActiveAnimatorState
		{
			public string		Name ;		// ステート名
			public float		Duration ;	// 再生時間
			public bool			IsLoop ;	// ループするか
		}

		// 実行中のアニメーション情報
		private Dictionary<int,ActiveAnimation>	m_ActiveAnimations ;

		// イベント
		private Action<string>	m_OnAnimationEvents ;

		/// <summary>
		/// アニメーションが再生中かどうか
		/// </summary>
		public bool IsPlayingAnimation( int layer = 0 )
		{
			if( m_ActiveAnimations == null )
			{
				return false ;
			}

			return m_ActiveAnimations.ContainsKey( layer ) ;
		}

		/// <summary>
		/// 全レイヤーの Animator を停止させる
		/// </summary>
		/// <returns></returns>
		public bool StopAllAnimators()
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component : " + Path ) ;
				return false ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "[StopAllAnimators] Animator Component is disabled : " + Path ) ;
//				return false ;
//			}

			//----------------------------------------------------------

			if( m_ActiveAnimations != null && m_ActiveAnimations.Count >  0 )
			{
				int[] layers = new int[ m_ActiveAnimations.Count ] ;
				m_ActiveAnimations.Keys.CopyTo( layers, 0 ) ;

				foreach( var layer in layers )
				{
					if( m_ActiveAnimations.ContainsKey( layer ) == true )
					{
						// 強制中断
						m_ActiveAnimations[ layer ].Break  = false ;

						m_ActiveAnimations[ layer ].State.IsDone = true ;
						m_ActiveAnimations[ layer ].State = null ;

						m_ActiveAnimations.Remove( layer ) ;
					}
				}
			}

			m_Animator.enabled = false ;

			//----------------------------------------------------------

			return true ;
		}


		/// <summary>
		/// Animator の停止
		/// </summary>
		/// <returns></returns>
		public bool StopAnimator( int layer = 0 )
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component : Path " + Path ) ;
				return false ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "[StopAnimator] Animator Component is disabled : " + Path ) ;
//				return false ;
//			}

			//----------------------------------------------------------

			if( m_ActiveAnimations != null )
			{
				if( m_ActiveAnimations.ContainsKey( layer ) == true )
				{
					// 強制中断
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;
				}
			}

//			m_Animator.Play( "Entry", layer ) ;
			m_Animator.enabled = false ;

			//----------------------------------------------------------

			return true ;
		}

		/// <summary>
		/// アクティブなアニメーターのステート情報を取得する
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public ActiveAnimatorState GetAnimatorState( int layer = 0 )
		{
			if( m_ActiveAnimations.ContainsKey( layer ) == false )
			{
				return null ;	// アクティブなアニメーターのステート情報は存在しない
			}

			var animation = m_ActiveAnimations[ layer ] ;

			var state = new ActiveAnimatorState()
			{
				Name		= animation.Name,
				Duration	= animation.Duration,
				IsLoop		= animation.IsLoop
			} ;

			return state ;
		}

		/// <summary>
		/// 現在のアニメーションはループするものか判定する
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		public bool IsAnimationLooping( int layer = 0 )
		{
			if( m_ActiveAnimations.ContainsKey( layer ) == false )
			{
				return false ;	// アクティブなアニメーターのステート情報は存在しない
			}

			var animation = m_ActiveAnimations[ layer ] ;
			return animation.IsLoop ;
		}

		/// <summary>
		/// アニメーターの速度を設定する
		/// </summary>
		/// <param name="speed"></param>
		/// <returns></returns>
		public bool SetAnimatorSpeed( float speed )
		{
			if( m_Animator == null )
			{
				return false ;
			}

			m_Animator.speed = speed ;

			return true ;
		}

		/// <summary>
		/// Animatorの再生
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public AsyncState PlayAnimator( string stateName, int layer = 0, float offset = 0, bool waitForFinish = true, Action<string> onEvent = null, Action onStarted = null, Action<bool> onFinished = null )
		{
			if( CAnimator == null )
			{
				Debug.LogWarning( "Not found Animator Component : [PlayAnimator] StateName = " + stateName + " Path = " + Path, this ) ;
				return null ;
			}

			if( CAnimator.runtimeAnimatorController == null )
			{
				// アニメーションコントローラが設定されていない
				onFinished?.Invoke( false ) ;

				Debug.LogWarning( "Not set AnimationController : [PlayAnimator] " + Path + " | StateName = " + stateName ) ;
				return null ;
			}

//			if( m_Animator.enabled == false )
//			{
//				Debug.LogWarning( "Animator Component is disabled : [PlayAnimator] " + stateName ) ;
//				return null ;
//			}
			m_Animator.enabled = true ;

			if( offset >= 1.0f )
			{
				Debug.LogWarning( "Bad offset : [PlayAnimator] " + stateName + " offset = " + offset ) ;
				return null ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				Debug.LogWarning( "activeInHierarchy is false : [PlayAnimator] " + stateName + " offset = " + offset + " Path = " + Path ) ;
				return null ;
			}

			m_ActiveAnimations ??= new Dictionary<int, ActiveAnimation>() ;

			//----------------------------------------------------------
			// １フレーム目からアニメーションの状態を反映させるため０フレームでアニメーションを実行する

			var state = new AsyncState( this ) ;
			StartCoroutine( PlayAnimator_Private( stateName, layer, offset, waitForFinish, onEvent, onStarted, onFinished, state ) ) ;
			return state ;
		}

		private IEnumerator PlayAnimator_Private( string stateName, int layer, float offset, bool waitForFinish, Action<string> onEvent, Action onStarted, Action<bool> onFinished, AsyncState state )
		{
			//------------------------------------------------------------------------------------------
			// 以前の再生の終了を待つ

			if( m_ActiveAnimations.ContainsKey( layer ) == true )
			{
//				Debug.Log( "----->別のアニメーションを再生中なのかもしれない:" + stateName + " -> " + m_ActiveAnimations[ layer ].Name + " " + m_ActiveAnimations[ layer ].IsLoop ) ;
				if( waitForFinish == false || m_ActiveAnimations[ layer ].IsLoop == true )
				{
					// 強制中断
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;
				}
				else
				{
					// 別のアニメーションを終了を待つ
					while( m_ActiveAnimations.ContainsKey( layer ) == true )
					{
						yield return null ;
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 新規に再生を行う

			//----------------------------------

//			Debug.Log( "アニメーション実行1:" + Path + " " + stateName ) ;

			// コールバックの設定は再生よりも前のタイミングで行う
			if( onEvent != null )
			{
				// 任意イベントのハンドラーを追加する
				m_OnAnimationEvents -= onEvent ;
				m_OnAnimationEvents += onEvent ;
			}

			// 新しいアニメショーンを再生する
			m_Animator.Play( stateName, layer, offset ) ;

			m_Animator.Update( 0 ) ;	// 強制的に設定したステートの開始状態にする(重要)

			AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;

			if( stateInfo.IsName( stateName ) == false )
			{
				// 再生できない

				// コールバックを除去する
				if( onEvent != null )
				{
					m_OnAnimationEvents -= onEvent ;
				}

				Debug.LogWarning( "[Animator] Can not play. StateName = " + stateName ) ;
				state.IsDone = true ;
				yield break ;
			}

			// 待機オブジェクトを保存する(全体で１つ)
			m_ActiveAnimations.Add( layer, new ActiveAnimation() ) ;
			m_ActiveAnimations[ layer ].State			= state ;
			m_ActiveAnimations[ layer ].State.option	= stateName ;
			m_ActiveAnimations[ layer ].Name			= stateName ;

			m_Animator.speed = m_TimeScale ;

			//------------------------------------------------------------------------------------------
			// 新規の再生が実際に始められたのを待つ(この中はおそらく実行されなくなったはず)

			// Update( 0 ) があれば以下は不要だが念の為
			if( stateInfo.IsName( stateName ) == false || stateInfo.normalizedTime >  1.0f )
			{
				Debug.Log( "<color=#FF00FF>[Animator]別のステートが実行中の可能性がある: 目的のステート = " + stateName + " 再生中になったか = " + stateInfo.IsName( stateName ) + " カレントのプログレス = " + stateInfo.normalizedTime + "</color>" ) ;

				// 前回のステート状態が残っている(Play実行直後ではまだ残っている)
				while( true )
				{
					if( m_ActiveAnimations[ layer ].Break == true )
					{
						// 中断
//						Debug.Log( "[中断0]" + stateName ) ;
						m_ActiveAnimations[ layer ].Break  = false ;

						m_ActiveAnimations[ layer ].State.IsDone = true ;
						m_ActiveAnimations[ layer ].State = null ;

						m_ActiveAnimations.Remove( layer ) ;
						yield break ;
					}

					// ステートを取り直す
					stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;
					if( stateInfo.IsName( stateName ) == true && stateInfo.normalizedTime <  1.0f )
					{
						// 新しいステートが開始された
						break ;
					}

					yield return null ;
				}
			}

			//--------------------------------------------------------------------------

			// 構造体なので毎フレーム取得して値を確認する必要がある
			stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;

			m_ActiveAnimations[ layer ].Duration	= stateInfo.length ;
			m_ActiveAnimations[ layer ].IsLoop		= stateInfo.loop ;

			// 開始コールバック
			onStarted?.Invoke() ;

//			Debug.Log( "アニメーション実行2:" + stateName + " " + stateInfo.IsName( stateName ) + " " + stateInfo.loop ) ;

			//------------------------------------------------------------------------------------------
			// 新規の再生の終了を待つ

			// ワンショットもループも関係なく終了待ちに入る
			while( true )
			{
//				Debug.Log( "ループ中:" + stateName + " " + m_ActiveAnimations[ layer ].IsLoop  ) ;

				if( m_ActiveAnimations == null || m_ActiveAnimations.Count == 0 )
				{
					// 別で強制停止させられた
//					Debug.Log( "[中断1]" + stateName ) ;

					if( onEvent != null )
					{
						// 任意イベントのハンドラーを削除する
						m_OnAnimationEvents -= onEvent ;
					}

					// 中断コールバック
					onFinished?.Invoke( false ) ;

					yield break ;
				}

				if( m_ActiveAnimations[ layer ].Break == true )
				{
					// 中断(ループの場合はここで中断を待つしか無い)
//					Debug.Log( "[中断2]" + stateName ) ;

					if( onEvent != null )
					{
						// 任意イベントのハンドラーを削除する
						m_OnAnimationEvents -= onEvent ;
					}

					// 中断コールバック
					onFinished?.Invoke( false ) ;

					//---------------------------------

					// 終了
					m_ActiveAnimations[ layer ].Break  = false ;

					m_ActiveAnimations[ layer ].State.IsDone = true ;	// コルーチン終了
					m_ActiveAnimations[ layer ].State = null ;

					m_ActiveAnimations.Remove( layer ) ;

					yield break ;
				}

				// ワンショットのみ終了時間をチェックする
				if( m_ActiveAnimations[ layer ].IsLoop == false )
				{
					// 構造体なので毎フレーム取得して値を確認する必要がある
					stateInfo = m_Animator.GetCurrentAnimatorStateInfo( layer ) ;
//					Debug.Log( "状態:" + stateInfo.normalizedTime ) ;
					if( stateInfo.normalizedTime >= 1.0f )
					{
						// 再生終了
						break ;
					}
				}

				yield return null ;
			}

//			Debug.Log( "アニメーション実行3:" + stateName ) ;

			if( onEvent != null )
			{
				// 任意イベントのハンドラーを削除する
				m_OnAnimationEvents -= onEvent ;
			}

			//---------------------------------

			// 終了
			m_ActiveAnimations[ layer ].Break  = false ;
	
			m_ActiveAnimations[ layer ].State.IsDone = true ;	// コルーチン終了
			m_ActiveAnimations[ layer ].State = null ;

			m_ActiveAnimations.Remove( layer ) ;

			//----------------------------------

			// 完了コールバック
			onFinished?.Invoke( true ) ;
		}

		/// <summary>
		/// アニメーションクリップのイベントハンドラー
		/// </summary>
		/// <param name="identity"></param>
		public void OnAnimationEvent( string identity )
		{
			m_OnAnimationEvents?.Invoke( identity ) ;
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		private PointerEventData	m_VRH_EventData		= null ;
		private List<RaycastResult>	m_VRH_Results		= null ;

		/// <summary>
		/// ポインターがこのビューにヒットするか判定する
		/// </summary>
		/// <returns></returns>
		public bool IsRaycastHit()
		{
			if( EventSystem.current == null )
			{
				// まだ準備が整っていない
				return false ;
			}

			//----------------------------------------------------------

			Vector2 position ;

#if UNITY_EDITOR || UNITY_STANDALONE

			position = InputAdapter.UIEventSystem.MousePosition ;

#elif !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)

			if( Input.touchCount == 1 )
			{
				position = Input.touches[ 0 ].position ;
			}
			else
			{
				return false ;
			}

#else
			return false ;
#endif
			return IsRaycastHit( position ) ;
		}

		/// <summary>
		/// 指定したスクリーン座標がこのビューにヒットするか判定する
		/// </summary>
		/// <returns></returns>
		public bool IsRaycastHit( float screenPositionX, float screenPositionY )
		{
			return IsRaycastHit( new Vector2( screenPositionX, screenPositionY ) ) ;
		}

		/// <summary>
		/// 指定したスクリーン座標がこのビューにヒットするか判定する
		/// </summary>
		/// <returns></returns>
		public bool IsRaycastHit( Vector2 screenPosition )
		{
			if( EventSystem.current == null )
			{
				// まだ準備が整っていない
				return false ;
			}

			//----------------------------------------------------------

			Vector2 position = screenPosition ;

			// スクリーン座標からRayを飛ばす

			m_VRH_EventData	??= new PointerEventData( EventSystem.current ) ;
			m_VRH_Results ??= new List<RaycastResult>() ;

			m_VRH_EventData.position = position ;
			m_VRH_Results.Clear() ;

			// レイキャストで該当するＵＩを探す
			EventSystem.current.RaycastAll( m_VRH_EventData, m_VRH_Results ) ;

			if( m_VRH_Results.Count >= 1 )
			{
				var target = m_VRH_Results[ 0 ].gameObject ;

				while( true )
				{
					if( gameObject == target )
					{
						// 発見しました
						return true ;
					}

					if( target.transform.parent != null )
					{
						target = target.transform.parent.gameObject ;
					}
					else
					{
						break ;
					}
				}
			}

			return false ;
		}

		//-------------------------------------------------------------------------------------------

		// 最後にクリックされたフレーム
		protected static int m_FrameCountOfLastClick = 0 ;

		protected static int m_InstanceIdOfLastClick = 0 ;

		[SerializeField]
		private	bool m_ClickExclusionEnabled = true ;

		/// <summary>
		/// クリックの排他制御
		/// </summary>
		public	bool   ClickExclusionEnabled
		{
			get
			{
				return m_ClickExclusionEnabled ;
			}
			set
			{
				m_ClickExclusionEnabled = value ;
			}
		}

		// クリックが有効になるか判定する
		protected bool CanClickExecution()
		{
			if( m_ClickExclusionEnabled == false )
			{
				// 排他制御は無効なのでクリック常に有効
				return true ;
			}

			// フレームカウントと比較して有効か判定する

			int frameCount = Time.frameCount ;

			if( m_FrameCountOfLastClick == 0 || frameCount >= ( m_FrameCountOfLastClick + 5 ) )
			{
				// 一定フレームが経過しているのでこのクリックは有効
				m_FrameCountOfLastClick = frameCount ;
				m_InstanceIdOfLastClick = gameObject.GetInstanceID() ;

				return true ;
			}

			if( frameCount == m_FrameCountOfLastClick && m_InstanceIdOfLastClick == gameObject.GetInstanceID() )
			{
				// 同じフレームに同じゲームオブジェクトからのクリック要求は有効(UIView.OnClick と UIButton.OnButtonClick など)
				return true ;
			}

			// 現在実行されるクリックは無効
			return false ;
		}

		//===================================================================================================================
		// ゲームパッド関係

		//-----------------------------------------------------------
		// Down 時の詳細な状態

		/// <summary>
		/// どのモードで動作した際に発せられたコールバックか
		/// </summary>
		public enum PressActionTypes
		{
			/// <summary>
			/// 通常
			/// </summary>
			NormalPress,

			/// <summary>
			/// 繰り返し
			/// </summary>
			RepeatPress,

			/// <summary>
			/// 長押し
			/// </summary>
			LongPress,
		}

		/// <summary>
		/// パッドのオートフォーカス処理の有効無効
		/// </summary>
		public bool PadAutoFocusEnabled
		{
			get
			{
				return m_PadAutoFocusEnabled ;
			}
			set
			{
				m_PadAutoFocusEnabled = value ;
			}
		}

		[SerializeField]
		protected bool m_PadAutoFocusEnabled = true ;

		//-----------------------------------------------------------
		// Down と Up を個別に分けたもの

		// ゲームパッドのボタン・アクシスが押された際に呼び出されるコールバック
		protected Action<int,Vector2[],Vector2> m_OnPadDown ;

		/// <summary>
		/// ゲームパッドのボタン・アクシスが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadDown"></param>
		public void SetOnPadDown( Action<int,Vector2[],Vector2> onPadDown )
		{
			m_OnPadDown = onPadDown ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		/// <param name="axisFlags"></param>
		/// <param name="margedAxisFlag"></param>
		public void CallOnPadDown( int buttonFlags, Vector2[] axisFlags, Vector2 margedAxisFlag )
		{
			m_OnPadDown?.Invoke( buttonFlags, axisFlags, margedAxisFlag ) ;
		}

		//---------------

		// ゲームパッドのボタン・アクシスが離された際に呼び出されるコールバック
		protected Action m_OnPadUp ;

		/// <summary>
		/// ゲームパッドのボタン・アクシスが離された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadUp"></param>
		public void SetOnPadUp( Action onPadUp )
		{
			m_OnPadUp = onPadUp ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		/// <param name="axisFlags"></param>
		/// <param name="margedAxisFlag"></param>
		public void CallOnPadUp()
		{
			m_OnPadUp?.Invoke() ;
		}

		//-----------------------------------

		// ゲームパッドのボタンが押された際に呼び出されるコールバック
		protected Action<int> m_OnPadButtonDown ;

		/// <summary>
		/// ゲームパッドのボタンが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadButtonDown"></param>
		public void SetOnPadButtonDown( Action<int> onPadButtonDown )
		{
			m_OnPadButtonDown = onPadButtonDown ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		public void CallOnPadButtonDown( int buttonFlags )
		{
			m_OnPadButtonDown?.Invoke( buttonFlags ) ;
		}

		//---------------

		// ゲームパッドのボタンが離された際に呼び出されるコールバック
		protected Action m_OnPadButtonUp ;

		/// <summary>
		/// ゲームパッドのボタンが離された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadButtonUp"></param>
		public void SetOnPadButtonUp( Action onPadButtonUp )
		{
			m_OnPadButtonUp = onPadButtonUp ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		public void CallOnPadButtonUp()
		{
			m_OnPadButtonUp?.Invoke() ;
		}

		//-----------------------------------

		// ゲームパッドのアクシスが押された際に呼び出されるコールバック
		protected Action<Vector2[],Vector2> m_OnPadAxisDown ;

		/// <summary>
		/// ゲームパッドのアクシスが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadAxisDown"></param>
		public void SetOnPadAxisDown( Action<Vector2[],Vector2> onPadAxisDown )
		{
			m_OnPadAxisDown = onPadAxisDown ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="axisFlags"></param>
		/// <param name="margedAxisFlag"></param>
		public void CallOnPadAxisDown( Vector2[] axisFlags, Vector2 margedAxisFlag )
		{
			m_OnPadAxisDown?.Invoke( axisFlags, margedAxisFlag ) ;
		}

		//---------------

		// ゲームパッドのアクシスが離された際に呼び出されるコールバック
		protected Action m_OnPadAxisUp ;

		/// <summary>
		/// ゲームパッドのアクシスが離された際に呼び出されるコールバックを設定する
		/// </summary>
		public void SetOnPadAxisUp( Action onPadAxisUp )
		{
			m_OnPadAxisUp = onPadAxisUp ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		public void CallOnPadAxisUp()
		{
			m_OnPadAxisUp?.Invoke() ;
		}

		//---------------------------------------------------------------------------
		// LongPress

		// ゲームパッドのボタン・アクシスが押された際に呼び出されるコールバック
		protected Action<int,Vector2[],Vector2> m_OnPadLongPress ;

		/// <summary>
		/// ゲームパッドのボタン・アクシスが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadDown"></param>
		public void SetOnPadLongPress( Action<int,Vector2[],Vector2> onPadLongPress )
		{
			m_OnPadLongPress = onPadLongPress ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		/// <param name="axisFlags"></param>
		/// <param name="margedAxisFlag"></param>
		public void CallOnPadLongPress( int buttonFlags, Vector2[] axisFlags, Vector2 margedAxisFlag )
		{
			m_OnPadLongPress?.Invoke( buttonFlags, axisFlags, margedAxisFlag ) ;
		}

		//---------------

		// ゲームパッドのボタンが押された際に呼び出されるコールバック
		protected Action<int> m_OnPadButtonLongPress ;

		/// <summary>
		/// ゲームパッドのボタンが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadButtonDown"></param>
		public void SetOnPadButtonLongPress( Action<int> onPadButtonLongPress )
		{
			m_OnPadButtonLongPress = onPadButtonLongPress ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="buttonFlags"></param>
		public void CallOnPadButtonLongPress( int buttonFlags )
		{
			m_OnPadButtonLongPress?.Invoke( buttonFlags ) ;
		}

		//-----

		// ゲームパッドのアクシスが押された際に呼び出されるコールバック
		protected Action<Vector2[],Vector2> m_OnPadAxisLongPress ;

		/// <summary>
		/// ゲームパッドのアクシスが押された際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPadAxisLongPress"></param>
		public void SetOnPadAxisLongPress( Action<Vector2[],Vector2> onPadAxisLongPress )
		{
			m_OnPadAxisLongPress = onPadAxisLongPress ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		/// <param name="axisFlags"></param>
		/// <param name="margedAxisFlag"></param>
		public void CallOnPadAxisLongPress( Vector2[] axisFlags, Vector2 margedAxisFlag )
		{
			m_OnPadAxisLongPress?.Invoke( axisFlags, margedAxisFlag ) ;
		}

		//---------------------------------------------------------------------------

		// ゲームパッドの入力の有効・無効が切り替わった際に呼び出されるコールバック
		protected Action<bool> m_OnPadInputStateChanged ;

		/// <summary>
		/// ゲームパッドの入力の有効・無効が切り替わった際に呼び出されるコールバックを登録する
		/// </summary>
		/// <param name="onPadInputStateChanged"></param>
		public void SetOnPadInputStateChanged( Action<bool> onPadInputStateChanged )
		{
			m_OnPadInputStateChanged = onPadInputStateChanged ;
		}

		/// <summary>
		/// UIPadAdapter からの呼び出し専用
		/// </summary>
		/// <param name="state"></param>
		public void CallOnPadInputStateChanged( bool state )
		{
			if( m_PadAutoFocusEnabled == true )
			{
				if( this is UIListView listView )
				{
					listView.SetPadFocus( state ) ;
				}
			}

			//----------------------------------

			m_OnPadInputStateChanged?.Invoke( state ) ;
		}
	}
}


// メモ
// iTween でわかりやすい
// http://d.hatena.ne.jp/nakamura001/20121127/1354021902

