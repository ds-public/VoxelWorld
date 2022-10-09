using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace TransformHelper
{
	[ ExecuteInEditMode ]
	public class SoftTransformTween : MonoBehaviour
	{
		/// <summary>
		/// 識別名
		/// </summary>
		public string Identity ;

		/// <summary>
		/// 遅延時間(秒)
		/// </summary>
		public float Delay = 0 ;

		/// <summary>
		/// 実行時間(秒)
		/// </summary>
		public float Duration = 1 ;

		/// <summary>
		/// 動作対象種別の定義
		/// </summary>
		public enum MotionTypes
		{
			Position = 0,
			Rotation = 1,
			Scale    = 2,
			alpha    = 3,
		}

		[SerializeField][HideInInspector]
		private MotionTypes m_MotionType = MotionTypes.Position ;

		/// <summary>
		/// 動作対象種別
		/// </summary>
		public  MotionTypes   MotionType
		{
			get
			{
				return m_MotionType ;
			}
			set
			{
				if( m_MotionType != value )
				{
					m_MotionType  = value ;
					RefreshChecker() ;
				}
			}
		}
		
		/// <summary>
		/// カーブの処理方法種別の定義
		/// </summary>
		public enum ProcessTypes
		{
			Ease = 0,
			AnimationCurve = 1,
		}

		/// <summary>
		/// イーズの種別の定義
		/// </summary>
		public enum EaseTypes
		{
			EaseInQuad,
			EaseOutQuad,
			EaseInOutQuad,
			EaseInCubic,
			EaseOutCubic,
			EaseInOutCubic,
			EaseInQuart,
			EaseOutQuart,
			EaseInOutQuart,
			EaseInQuint,
			EaseOutQuint,
			EaseInOutQuint,
			EaseInSine,
			EaseOutSine,
			EaseInOutSine,
			EaseInExpo,
			EaseOutExpo,
			EaseInOutExpo,
			EaseInCirc,
			EaseOutCirc,
			EaseInOutCirc,
			Linear,
			Spring,
			EaseInBounce,
			EaseOutBounce,
			EaseInOutBounce,
			EaseInBack,
			EaseOutBack,
			EaseInOutBack,
			EaseInElastic,
			EaseOutElastic,
			EaseInOutElastic,
	//		Punch
		}
		
		//-----------------------------------------------------------

		// ポジション

		[SerializeField][HideInInspector]
		private bool m_PositionEnabled = false ;

		/// <summary>
		/// 位置の変化を有効にするかどうか
		/// </summary>
		public  bool  PositionEnabled
		{
			get
			{
				return m_PositionEnabled ;
			}
			set
			{
				if( m_PositionEnabled != value )
				{
					m_PositionEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionFrom = Vector3.zero ;

		/// <summary>
		/// 開始位置
		/// </summary>
		public  Vector3  PositionFrom
		{
			get
			{
				return m_PositionFrom ;
			}
			set
			{
				if( m_PositionFrom != value )
				{
					m_PositionFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始位置Ｘ
		/// </summary>
		public float PositionFromX
		{
			get
			{
				return m_PositionFrom.x ;
			}
			set
			{
				PositionFrom = new Vector2( value, m_PositionFrom.y ) ;
			}
		}

		/// <summary>
		/// 開始位置Ｙ
		/// </summary>
		public float PositionFromY
		{
			get
			{
				return m_PositionFrom.y ;
			}
			set
			{
				PositionFrom = new Vector2( m_PositionFrom.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionTo   = Vector3.zero ;

		/// <summary>
		/// 終了位置
		/// </summary>
		public  Vector3  PositionTo
		{
			get
			{
				return m_PositionTo ;
			}
			set
			{
				if( m_PositionTo != value )
				{
					m_PositionTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了位置Ｘ
		/// </summary>
		public float PositionToX
		{
			get
			{
				return m_PositionTo.x ;
			}
			set
			{
				PositionTo = new Vector2( value, m_PositionTo.y ) ;
			}
		}

		/// <summary>
		/// 終了位置Ｙ
		/// </summary>
		public float PositionToY
		{
			get
			{
				return m_PositionTo.y ;
			}
			set
			{
				PositionTo = new Vector2( m_PositionTo.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_PositionProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 位置のカーブの処理方法種別
		/// </summary>
		public  ProcessTypes  PositionProcessType
		{
			get
			{
				return m_PositionProcessType ;
			}
			set
			{
				if( m_PositionProcessType != value )
				{
					m_PositionProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_PositionEaseType = EaseTypes.Linear ;

		/// <summary>
		/// 位置のイーズの種別
		/// </summary>
		public  EaseTypes  PositionEaseType
		{
			get
			{
				return m_PositionEaseType ;
			}
			set
			{
				if( m_PositionEaseType != value )
				{
					m_PositionEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_PositionAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 位置のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  PositionAnimationCurve
		{
			get
			{
				return m_PositionAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_PositionAnimationCurve, value ) == true )
				{
					m_PositionAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_PositionFoldOut = true ;

		/// <summary>
		/// 位置のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  PositionFoldOut
		{
			get
			{
				return m_PositionFoldOut ;
			}
			set
			{
				m_PositionFoldOut = value ;
			}
		}

		//-----------------------------------------------------------

		// ローテーション

		[SerializeField][HideInInspector]
		private bool m_RotationEnabled = false ;

		/// <summary>
		/// 角度の変化を有効にするかどうか
		/// </summary>
		public  bool  RotationEnabled
		{
			get
			{
				return m_RotationEnabled ;
			}
			set
			{
				if( m_RotationEnabled != value )
				{
					m_RotationEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationFrom = Vector3.zero ;

		/// <summary>
		/// 開始角度
		/// </summary>
		public  Vector3  RotationFrom
		{
			get
			{
				return m_RotationFrom ;
			}
			set
			{
				if( m_RotationFrom != value )
				{
					m_RotationFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始角度Ｘ
		/// </summary>
		public float RotationFromX
		{
			get
			{
				return m_RotationFrom.x ;
			}
			set
			{
				RotationFrom = new Vector3( value, m_RotationFrom.y, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｙ
		/// </summary>
		public float RotationFromY
		{
			get
			{
				return m_RotationFrom.y ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationFrom.x, value, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｚ
		/// </summary>
		public float RotationFromZ
		{
			get
			{
				return m_RotationFrom.z ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationFrom.x, m_RotationFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationTo   = Vector3.zero ;

		/// <summary>
		/// 終了角度
		/// </summary>
		public  Vector3  RotationTo
		{
			get
			{
				return m_RotationTo ;
			}
			set
			{
				if( m_RotationTo != value )
				{
					m_RotationTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了角度Ｘ
		/// </summary>
		public float RotationToX
		{
			get
			{
				return m_RotationTo.x ;
			}
			set
			{
				RotationTo = new Vector3( value, m_RotationTo.y, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｙ
		/// </summary>
		public float RotationToY
		{
			get
			{
				return m_RotationTo.y ;
			}
			set
			{
				RotationTo = new Vector3( m_RotationTo.x, value, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｚ
		/// </summary>
		public float RotationToZ
		{
			get
			{
				return m_RotationTo.z ;
			}
			set
			{
				RotationFrom = new Vector3( m_RotationTo.x, m_RotationTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_RotationProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 角度のカーブの処理種別
		/// </summary>
		public  ProcessTypes  RotationProcessType
		{
			get
			{
				return m_RotationProcessType ;
			}
			set
			{
				if( m_RotationProcessType != value )
				{
					m_RotationProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_RotationEaseType = EaseTypes.Linear ;

		/// <summary>
		/// 角度のイーズの種別
		/// </summary>
		public  EaseTypes  RotationEaseType
		{
			get
			{
				return m_RotationEaseType ;
			}
			set
			{
				if( m_RotationEaseType != value )
				{
					m_RotationEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_RotationAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 角度のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  RotationAnimationCurve
		{
			get
			{
				return m_RotationAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_RotationAnimationCurve, value ) == true )
				{
					m_RotationAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_RotationFoldOut = true ;

		/// <summary>
		/// 角度のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  RotationFoldOut
		{
			get
			{
				return m_RotationFoldOut ;
			}
			set
			{
				m_RotationFoldOut = value ;
			}
		}

		//-------------------------------------------

		// スケール

		[SerializeField][HideInInspector]
		private bool m_ScaleEnabled = false ;

		/// <summary>
		/// 縮尺の変化を有効にするかどうか
		/// </summary>
		public  bool  ScaleEnabled
		{
			get
			{
				return m_ScaleEnabled ;
			}
			set
			{
				if( m_ScaleEnabled != value )
				{
					m_ScaleEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleFrom    = Vector3.one ;

		/// <summary>
		/// 開始縮尺
		/// </summary>
		public  Vector3  ScaleFrom
		{
			get
			{
				return m_ScaleFrom ;
			}
			set
			{
				if( m_ScaleFrom != value )
				{
					m_ScaleFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始縮尺Ｘ
		/// </summary>
		public float ScaleFromX
		{
			get
			{
				return m_ScaleFrom.x ;
			}
			set
			{
				ScaleFrom = new Vector3( value, m_ScaleFrom.y, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｙ
		/// </summary>
		public float ScaleFromY
		{
			get
			{
				return m_ScaleFrom.y ;
			}
			set
			{
				ScaleFrom = new Vector3( m_ScaleFrom.x, value, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｚ
		/// </summary>
		public float ScaleFromZ
		{
			get
			{
				return m_ScaleFrom.z ;
			}
			set
			{
				ScaleFrom = new Vector3( m_ScaleFrom.x, m_ScaleFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleTo      = Vector3.one ;

		/// <summary>
		/// 終了縮尺
		/// </summary>
		public  Vector3  ScaleTo
		{
			get
			{
				return m_ScaleTo ;
			}
			set
			{
				if( m_ScaleTo != value )
				{
					m_ScaleTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了縮尺Ｘ
		/// </summary>
		public float ScaleToX
		{
			get
			{
				return m_ScaleTo.x ;
			}
			set
			{
				ScaleTo = new Vector3( value, m_ScaleTo.y, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｙ
		/// </summary>
		public float ScaleToY
		{
			get
			{
				return m_ScaleTo.y ;
			}
			set
			{
				ScaleTo = new Vector3( m_ScaleTo.x, value, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｚ
		/// </summary>
		public float ScaleToZ
		{
			get
			{
				return m_ScaleTo.z ;
			}
			set
			{
				ScaleTo = new Vector3( m_ScaleTo.x, m_ScaleTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessTypes m_ScaleProcessType = ProcessTypes.Ease ;

		/// <summary>
		/// 縮尺のカーブの処理種別
		/// </summary>
		public  ProcessTypes  ScaleProcessType
		{
			get
			{
				return m_ScaleProcessType ;
			}
			set
			{
				if( m_ScaleProcessType != value )
				{
					m_ScaleProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseTypes m_ScaleEaseType = EaseTypes.Linear ;

		/// <summary>
		/// 縮尺のイーズの種別
		/// </summary>
		public  EaseTypes  ScaleEaseType
		{
			get
			{
				return m_ScaleEaseType ;
			}
			set
			{
				if( m_ScaleEaseType != value )
				{
					m_ScaleEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_ScaleAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 縮尺のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  ScaleAnimationCurve
		{
			get
			{
				return m_ScaleAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_ScaleAnimationCurve, value ) == true )
				{
					m_ScaleAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_ScaleFoldOut = true ;

		/// <summary>
		/// 縮尺のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  ScaleFoldOut
		{
			get
			{
				return m_ScaleFoldOut ;
			}
			set
			{
				m_ScaleFoldOut = value ;
			}
		}

		//-------------------------------------------

		[SerializeField][HideInInspector]
		private ProcessTypes m_ProcessType = ProcessTypes.Ease ;
		public  ProcessTypes   ProcessType
		{
			get
			{
				return m_ProcessType ;
			}
			set
			{
				if( m_ProcessType != value )
				{
					m_ProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}


		[SerializeField][HideInInspector]
		private EaseTypes m_EaseType = EaseTypes.Linear ;
		public  EaseTypes   EaseType
		{
			get
			{
				return m_EaseType ;
			}
			set
			{
				if( m_EaseType != value )
				{
					m_EaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブバージョン
		[SerializeField][HideInInspector]
		private AnimationCurve m_AnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;
		public  AnimationCurve   AnimationCurve
		{
			get
			{
				return m_AnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_AnimationCurve, value ) == true )
				{
					m_AnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブに変化があったか確認する
		private bool IsModifyAnamationCurve( AnimationCurve a1, AnimationCurve a2 )
		{
			if( a1.length != a2.length || a1.postWrapMode != a2.postWrapMode || a1.preWrapMode != a2.preWrapMode )
			{
				return true ;
			}

			int i, l = a1.length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Keyframe k1 = a1[ i ] ;	// 最終キー
				Keyframe k2 = a2[ i ] ;	// 最終キー

				if( k1.time != k2.time || k1.value != k2.value || k1.inTangent != k2.inTangent || k1.outTangent != k2.outTangent )
				{
					return true ;
				}
			}

			return false ;
		}



		// 相対位置か絶対位置か（相対位置）
		public enum ValueTypes
		{
			Relative = 0,
			Absolute = 1,
			RelativeAndUpdate = 2,
		}

		[SerializeField][HideInInspector]
		private ValueTypes m_ValueType = ValueTypes.Relative ;
		public  ValueTypes   ValueType
		{
			get
			{
				return m_ValueType ;
			}
			set
			{
				if( m_ValueType != value )
				{
					m_ValueType  = value ;
					RefreshChecker() ;
				}
			}
		}

		//------------------------------------------------------

		/// <summary>
		/// ループさせるか
		/// </summary>
		public bool Loop = false ;

		/// <summary>
		/// ループ場合に終了時に逆再生するか
		/// </summary>
		public bool Reverse = true ;

		// タイムスケールを無視するかどうか（する）
		public bool IgnoreTimeScale = true ;

		/// <summary>
		/// インスタンス生成と同時に実行されるようにするか
		/// </summary>
		public bool PlayOnAwake = true ;

		/// <summary>
		/// 処理中は入力を受け付けないようにするか
		/// </summary>
//		public bool InteractionDisableInPlaying = true ;

		//--------------------------------------------------------

		private Transform		m_Transform ;

		private Vector3			m_BasePosition ;
		private Vector3			m_BaseRotation ;
		private Vector3			m_BaseScale ;

		private SoftTransform	m_Base ;

		private float			m_Time ;
		private float			m_BaseTime ;

		// 実行状況
		protected bool			m_IsRunning ;
		public  bool			IsRunning{ get{ return m_IsRunning ; } }

		// 再生状況
		protected bool			m_IsPlaying ;
		public  bool			IsPlaying{ get{ return m_IsPlaying ; } }
	

		private bool			m_Busy = false ;

		//--------------------------------------------------------

		public UnityEvent OnFinished = new UnityEvent() ;

		private Action<string, SoftTransformTween> m_OnFinishedAction ;
		public  Action<string, SoftTransformTween>   OnFinishedAction
		{
			get
			{
				return m_OnFinishedAction ;
			}
			set
			{
				m_OnFinishedAction = value ;
			}
		}
		public void SetOnFinished( Action<string, SoftTransformTween> onFinishAction )
		{
			m_OnFinishedAction = onFinishAction ;
		}


		//--------------------------------------------------------

		internal void OnDisable()
		{
			// 無効化した際にチェッカーが開いていれば強制的に閉じる
			IsChecker = false ;
		}

		internal void Awake()
		{
			if( Application.isPlaying == true && IsChecker == true )
			{
				LoadState() ;
			}		
		}
	
		private bool	 m_IsStarting = false ;

		private float	m_DelayStack	= -1 ;
		private float	m_DurationStack	= -1 ;
		private float	m_OffsetStack	=  0 ; 

		internal void Start()
		{
			// カスタムリスナー登録
			OnFinished.AddListener( OnFinishedInner ) ;

			m_IsStarting = true ;	// スタートが実行された

			if( Application.isPlaying == true && ( PlayOnAwake == true || m_IsRunning == true ) )
			{
				// Start() の前に　Play が実行されている可能性もある
				Play( m_DelayStack, m_DurationStack, m_OffsetStack ) ;
			}
		}
	
		//---------------------------------------------

		// 内部リスナー登録
		private void OnFinishedInner()
		{
			m_OnFinishedAction?.Invoke( Identity, this ) ;
		}
	
		/// <summary>
		/// 再生終了時に呼ばれるリスナーを登録する
		/// </summary>
		/// <param name="tOnFinished"></param>
		public void AddOnFinishedListener( UnityEngine.Events.UnityAction onFinished )
		{
			OnFinished.AddListener( onFinished ) ;
		}
		
		/// <summary>
		/// 再生終了時に呼ばれリスナーを全て削除する
		/// </summary>
		public void RemoveOnFinishedAllListeners()
		{
			OnFinished.RemoveAllListeners() ;
		}
	
		//---------------------------------------------
	
//		private bool m_KeepInteractionDisableProcass = false ; 

//		private void DisableInteraction()
//		{
//			// 自身が処理を担当する
//			m_KeepInteractionDisableProcass = true ;
//		}

//		private void EnableInteraction()
//		{
//			// 自身は処理の担当ではない
//			if( m_KeepInteractionDisableProcass == false )
//			{
//				return ;
//			}
//
//			// 処理終了
//			m_KeepInteractionDisableProcass = false ;
//		}


		//---------------------------------------------

		/// <summary>
		/// 再生する(コルーチン)
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator Play_Coroutine( float delay = -1, float duration = -1 )
		{
			Play( delay, duration ) ;

			while( m_IsRunning == true || m_IsPlaying == true )
			{
				yield return null ;
			}
		}


		/// <summary>
		/// 再生する
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		public void Play( float delay = -1, float duration = -1, float offset = 0  )
		{
			enabled = true ;

			if( m_IsStarting == false )
			{
				// スタート前なのでためるだけ
				m_DelayStack	= delay ;
				m_DurationStack	= duration ;
				m_OffsetStack	= offset ;

				m_IsRunning = true ;

				return ;
			}

			//-------------------------------------------

			if( delay >= 0 )
			{
				Delay = delay ;
			}

			if( duration >= 0 )
			{
				Duration = duration ;
			}

			m_Transform	= gameObject.GetComponent<Transform>() ;
			if( m_Transform != null )
			{
				m_BasePosition		= m_Transform.localPosition ;
				m_BaseRotation		= m_Transform.localEulerAngles ;
				m_BaseScale			= m_Transform.localScale ;
			}

//			if( interactionDisableInPlaying == true )
//			{
//				DisableInteraction() ;
//			}

			//--------------------------------------------------

			m_Base = gameObject.GetComponent<SoftTransform>() ;
			if( m_Base != null )
			{
				m_BasePosition = m_Base.LocalPosition ;
				m_BaseRotation = m_Base.LocalRotation ;
				m_BaseScale    = m_Base.LocalScale ;
			}

			//-------------------------------

			m_Time = offset ;
			SetState( m_Time ) ;	// 初期位置へ移動させる

			m_BaseTime = Time.realtimeSinceStartup ;

			m_IsRunning = true ;
			m_IsPlaying = true ;

			m_Busy = true ;
		}

		/// <summary>
		/// 状態を再生前に戻す
		/// </summary>
		public void Revert()
		{
			if( m_Busy == false )
			{
				return ;
			}

			m_IsRunning = false ;
			m_IsPlaying = false ;

			m_Transform	= gameObject.GetComponent<Transform>() ;
			if( m_Transform != null )
			{
				m_Transform.localPosition		= m_BasePosition ;
				m_Transform.localEulerAngles	= m_BaseRotation ;
				m_Transform.localScale			= m_BaseScale ;
			}

			m_Busy = false ;
		}



		/// <summary>
		/// 一時停止する
		/// </summary>
		public void Pause()
		{
			if( m_IsRunning == true )
			{
				m_IsPlaying = false ;
			}
		}

		/// <summary>
		/// 再開する
		/// </summary>
		public void Unpause()
		{
			if( m_IsRunning == true )
			{
				m_IsPlaying = true ;
			}
		}

		/// <summary>
		/// 完全停止する
		/// </summary>
		public void Stop()
		{
			if( m_IsRunning == true )
			{
//				if( interactionDisableInPlaying == true )
//				{
//					EnableInteraction() ;
//				}

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}
		}

		/// <summary>
		/// 完全に停止させ状態を元に戻す
		/// </summary>
		public void StopAndReset()
		{
			if( m_IsRunning == true )
			{
//				if( interactionDisableInPlaying == true )
//				{
//					EnableInteraction() ;
//				}

				m_IsPlaying = false ;
				m_IsRunning = false ;
			}

			Revert() ;
		}

		/// <summary>
		/// 強制的に最後の状態にする
		/// </summary>
		public void Finish()
		{
			// 強制的に最後の状態にする(ただしループになっているものは無効)
			if( Loop == true )
			{
				return ;
			}

			if( m_IsRunning == true && m_IsPlaying == true )
			{
				Modify( 1, true ) ;

				// 終了

				m_IsPlaying = false ;
				m_IsRunning = false ;

				// コールバック呼び出し
				OnFinished.Invoke() ;
			}
			else
			{
				Modify( 1, true ) ;
			}
		}

		internal void Update()
		{
			if( m_IsRunning == true && m_IsPlaying == true )
			{
				m_Time += GetDeltaTime() ;

				if( SetState( m_Time ) == true )
				{
					// 終了
//					if( interactionDisableInPlaying == true )
//					{
//						EnableInteraction() ;
//					}

					m_IsPlaying = false ;
					m_IsRunning = false ;

					// コールバック呼び出し
					OnFinished.Invoke() ;
				}
			}
		}

		// 現在の経過時間から状態を設定する
		private bool SetState( float time )
		{
			float duration = 0 ;
			if( m_ProcessType == ProcessTypes.Ease )
			{
				duration = Duration ;
			}
			else
			if( m_ProcessType == ProcessTypes.AnimationCurve )
			{
				int l = m_AnimationCurve.length ;
				Keyframe keyFrame = m_AnimationCurve[ l - 1 ] ;	// 最終キー
				duration = keyFrame.time ;
			}

			if( duration <= 0 )
			{
				return false ;
			}

			//---------------------------------------------

			if( time <  Delay )
			{
				Modify( 0, false ) ;
				return false ;
			}

			time = m_Time - Delay ;

			if( m_Base != null )
			{
				m_BasePosition = m_Base.LocalPosition ;
				m_BaseRotation = m_Base.LocalRotation ;
				m_BaseScale    = m_Base.LocalScale ;
			}

			int count ;
			float factor ;

			if( Loop == false )
			{
				// ループ無し

				factor  = time / duration ;
				if( factor >= 1 )
				{
					factor  = 1 ;
				}

				Modify( factor, factor >= 1 ) ;

				if( factor >= 1 )
				{
					return true ;
				}
			}
			else
			{
				// ループ有り
				
				count  = ( int )( time / duration ) ;
				factor = ( time % duration ) / duration ;

				if( Reverse == true && ( count & 1 ) == 1 )
				{
					factor = 1.0f - factor ;
				}

				Modify( factor, false ) ;
			}

			return false ;
		}


		/// <summary>
		/// 実行開始からの経過時間
		/// </summary>
		public float ProcessTime
		{
			get
			{
				return m_Time ;
			}
			set
			{
				m_Time = value ;

				// ベースタイムをリセットしておかないと次のフレームで大きく飛んでしまう
				m_BaseTime = Time.realtimeSinceStartup ;

				if( m_IsRunning == true )
				{
					SetState( m_Time ) ;
				}
			}
		}

		private void Modify( float factor, bool finished )
		{
			if( m_Transform != null )
			{
				if( m_PositionEnabled == true )
				{
					Vector3 delta = GetValue( m_PositionFrom, m_PositionTo, factor, m_PositionProcessType, m_PositionEaseType, m_PositionAnimationCurve ) ;

					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate )
					{
						m_Transform.localPosition = m_BasePosition + delta ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_Base != null )
						{
							m_Base.LocalPosition = m_BasePosition + delta ;
						}
					}
					else
					{
						m_Transform.localPosition = delta ;
					}
				}

				if( m_RotationEnabled == true )
				{
					Vector3 delta = GetValue( m_RotationFrom, m_RotationTo, factor, m_RotationProcessType, m_RotationEaseType, m_RotationAnimationCurve ) ;
			
					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate )
					{
						m_Transform.localEulerAngles = m_BaseRotation + delta ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_Base != null )
						{
							m_Base.LocalRotation = m_BaseRotation + delta ;
						}
					}
					else
					{
						m_Transform.localEulerAngles = delta ;
					}
				}

				if( m_ScaleEnabled == true )
				{
					Vector3 delta = GetValue( m_ScaleFrom, m_ScaleTo, factor, m_ScaleProcessType, m_ScaleEaseType, m_ScaleAnimationCurve ) ;

					if( m_ValueType == ValueTypes.Relative || m_ValueType == ValueTypes.RelativeAndUpdate  )
					{
						m_Transform.localScale = new Vector3( m_BaseScale.x * delta.x, m_BaseScale.y * delta.y, m_BaseScale.z * delta.z ) ;

						if( finished == true && m_ValueType == ValueTypes.RelativeAndUpdate && m_Base != null )
						{
							m_Base.LocalScale = new Vector3( m_BaseScale.x * delta.x, m_BaseScale.y * delta.y, m_BaseScale.z * delta.z ) ;
						}
					}
					else
					{
						m_Transform.localScale = delta ;
					}
				}
			}
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			float baseTime = Time.realtimeSinceStartup ;
			float deltaTime = baseTime - m_BaseTime ;

			m_BaseTime = baseTime ;

			if( IgnoreTimeScale == false )
			{
				// タイムスケールを無視しない
				deltaTime *= Time.timeScale ;
			}

			return deltaTime ;
		}


		//--------------------------------------------------------

		// Vector3 の変化中の値を取得
		public static Vector3 GetValue( Vector3 start, Vector3 end, float factor, ProcessTypes processType, EaseTypes easeType, AnimationCurve animationCurve = null )
		{
			float x = GetValue( start.x, end.x, factor, processType, easeType, animationCurve ) ;
			float y = GetValue( start.y, end.y, factor, processType, easeType, animationCurve ) ;
			float z = GetValue( start.z, end.z, factor, processType, easeType, animationCurve ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public static float GetValue( float start, float end, float factor, ProcessTypes processType, EaseTypes easeType, AnimationCurve animationCurve = null )
		{
			float value = 0 ;

			if( processType == ProcessTypes.Ease )
			{
				switch( easeType )
				{
					case EaseTypes.EaseInQuad		: value = EaseInQuad(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutQuad		: value = EaseOutQuad(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutQuad	: value = EaseInOutQuad(	start, end, factor ) ; break ;
					case EaseTypes.EaseInCubic		: value = EaseInCubic(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutCubic		: value = EaseOutCubic(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutCubic	: value = EaseInOutCubic(	start, end, factor ) ; break ;
					case EaseTypes.EaseInQuart		: value = EaseInQuart(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutQuart		: value = EaseOutQuart(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutQuart	: value = EaseInOutQuart(	start, end, factor ) ; break ;
					case EaseTypes.EaseInQuint		: value = EaseInQuint(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutQuint		: value = EaseOutQuint(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutQuint	: value = EaseInOutQuint(	start, end, factor ) ; break ;
					case EaseTypes.EaseInSine		: value = EaseInSine(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutSine		: value = EaseOutSine(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutSine	: value = EaseInOutSine(	start, end, factor ) ; break ;
					case EaseTypes.EaseInExpo		: value = EaseInExpo(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutExpo		: value = EaseOutExpo(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutExpo	: value = EaseInOutExpo(	start, end, factor ) ; break ;
					case EaseTypes.EaseInCirc		: value = EaseInCirc(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutCirc		: value = EaseOutCirc(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutCirc	: value = EaseInOutCirc(	start, end, factor ) ; break ;
					case EaseTypes.Linear			: value = Linear(			start, end, factor ) ; break ;
					case EaseTypes.Spring			: value = Spring(			start, end, factor ) ; break ;
					case EaseTypes.EaseInBounce		: value = EaseInBounce(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutBounce	: value = EaseOutBounce(	start, end, factor ) ; break ;
					case EaseTypes.EaseInOutBounce	: value = EaseInOutBounce(	start, end, factor ) ; break ;
					case EaseTypes.EaseInBack		: value = EaseInBack(		start, end, factor ) ; break ;
					case EaseTypes.EaseOutBack		: value = EaseOutBack(		start, end, factor ) ; break ;
					case EaseTypes.EaseInOutBack	: value = EaseInOutBack(	start, end, factor ) ; break ;
					case EaseTypes.EaseInElastic	: value = EaseInElastic(	start, end, factor ) ; break ;
					case EaseTypes.EaseOutElastic	: value = EaseOutElastic(	start, end, factor ) ; break ;
					case EaseTypes.EaseInOutElastic	: value = EaseInOutElastic(	start, end, factor ) ; break ;
				}
			}
			else
			if( processType == ProcessTypes.AnimationCurve )
			{
				if( animationCurve != null )
				{
					int l = animationCurve.length ;
					Keyframe keyFrame = animationCurve[ l - 1 ] ;	// 最終キー
					float duration = keyFrame.time ;
	
					value = animationCurve.Evaluate( duration * factor ) ;
				}

				value = Mathf.Lerp( start, end, value ) ;
			}

			return value ;
		}


		//------------------------

		private static float EaseInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private static float EaseOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private static float EaseInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private static float EaseInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private static float EaseOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private static float EaseInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private static float EaseInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private static float EaseOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private static float EaseInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private static float EaseInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private static float EaseOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private static float EaseInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private static float EaseInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private static float EaseOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private static float EaseInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private static float EaseInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private static float EaseOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private static float EaseInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private static float EaseInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private static float EaseOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private static float EaseInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private static float Linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private static float Spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private static float EaseInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - EaseOutBounce( 0, end, d - value ) + start ;
		}
	
		private static float EaseOutBounce( float start, float end, float value )
		{
			value /= 1f ;
			end -= start ;
			if( value <  ( 1 / 2.75f ) )
			{
				return end * ( 7.5625f * value * value ) + start ;
			}
			else
			if( value <  ( 2 / 2.75f ) )
			{
				value -= ( 1.5f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .75f ) + start ;
			}
			else
			if( value <  (  2.5  / 2.75 ) )
			{
				value -= ( 2.25f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .9375f ) + start ;
			}
			else
			{
				value -= ( 2.625f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .984375f ) + start ;
			}
		}

		private static float EaseInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return EaseInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return EaseOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private static float EaseInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private static float EaseOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float EaseInOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value /= 0.5f ;
			if( ( value ) <  1 )
			{
				s *= ( 1.525f ) ;
				return end * 0.5f * ( value * value * ( ( ( s ) + 1 ) * value - s ) ) + start ;
			}
			value -= 2 ;
			s *= ( 1.525f ) ;
			return end * 0.5f * ( ( value ) * value * ( ( ( s ) + 1 ) * value + s ) + 2 ) + start ;
		}

		private static float EaseInElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return - ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
		}		

		private static float EaseOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p * 0.25f ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return ( a * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start ) ;
		}		

		private static float EaseInOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d * 0.5f ) == 2 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			if( value <  1 ) return - 0.5f * ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
			return a * Mathf.Pow( 2, - 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start ;
		}
#if false
		private static float Punch( float amplitude, float value )
		{
			float s ;
			if( value == 0 )
			{
				return 0 ;
			}
			else
			if( value == 1 )
			{
				return 0 ;
			}
			float period = 1 * 0.3f ;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 ) ;
			return ( amplitude * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * 1 - s ) * ( 2 * Mathf.PI ) / period ) ) ;
		}

		private static float Clerp( float start, float end, float value )
		{
			float min = 0.0f ;
			float max = 360.0f ;
			float half = Mathf.Abs( ( max - min ) * 0.5f ) ;
			float retval ;
			float diff ;
			if( ( end - start ) <  - half )
			{
				diff =   ( ( max - start ) + end   ) * value ;
				retval = start + diff ;
			}
			else
			if( ( end - start ) >    half )
			{
				diff = - ( ( max - end   ) + start ) * value ;
				retval = start + diff ;
			}
			else retval = start + ( end - start ) * value ;
			return retval ;
		}
#endif
		//--------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3	m_InitialPosition ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialRotation ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialScale ;

		//-------------------------------------------------

		[SerializeField][HideInInspector]
		private bool m_IsChecker = false ;
		public  bool   IsChecker
		{
			get
			{
				return m_IsChecker ;
			}
			set
			{
				if( m_IsChecker != value )
				{
					m_IsChecker = value ;
					if( m_IsChecker == true )
					{
						SaveState() ;
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
					}
					else
					{
						LoadState() ;
					}
				}
			}
		}

		private void RefreshChecker()
		{
			if( Application.isPlaying == true )
			{
				return ;	// 実行中は無効
			}

			if( m_IsChecker == true )
			{
				LoadState() ;
				SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
			}
		}
	
		// ただの位置表示用
		[SerializeField][HideInInspector]
		private float m_CheckFactor = 0 ;
		public  float   CheckFactor
		{
			get
			{
				return m_CheckFactor ;
			}
			set
			{
				if( m_CheckFactor != value )
				{
					m_CheckFactor = value ;
					if( m_IsChecker == true )
					{
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
					}
				}
			}
		}

		// モーションチェック専用で現在の状態を退避する
		public void SaveState()
		{
			Transform targetTransform = gameObject.GetComponent<Transform>() ;
			if( targetTransform != null )
			{
				m_InitialPosition = targetTransform.localPosition ;
				m_InitialRotation = targetTransform.localEulerAngles ;
				m_InitialScale    = targetTransform.localScale ;
			}
		}

		// モーションチェック専用で現在の状態を復帰する
		private void LoadState()
		{
			Transform targetTransform = gameObject.GetComponent<Transform>() ;
			if( targetTransform != null )
			{
				targetTransform.localPosition	 = m_InitialPosition ;
				targetTransform.localEulerAngles = m_InitialRotation ;
				targetTransform.localScale       = m_InitialScale ;
			}
		}

		// デバッグ用
		private void SetCheckFactor( float checkFactor, Vector3 initialPosition, Vector3 initialRotation, Vector3 initialScale )
		{
			m_Transform	= gameObject.GetComponent<Transform>() ;

			//-----------------

			m_BasePosition		= initialPosition ;
			m_BaseRotation		= initialRotation ;
			m_BaseScale			= initialScale ;

			//-----------------

			Modify( checkFactor, false ) ;
		}
	}
}
