using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;


// Version 2022/10/01 0 

namespace TransformHelper
{
	[ ExecuteInEditMode ]
	[RequireComponent(typeof(Transform))]

	/// <summary>
	/// トランスフォームをもっと楽に扱うためのコンポーネント(メソッド拡張は他人とかち合う可能性があるので使わない)
	/// </summary>
	public class SoftTransform : MonoBehaviour
	{
		/// <summary>
		/// 識別子
		/// </summary>
		public string	Identity ;

		internal void Awake()
		{
			// Tween などで使用する基準情報を保存する(コンポーネントの実行順が不確定なので Awake で必ず実行すること)
			SetLocalState() ;
		}

		/// <summary>
		/// 回転をリセットする
		/// </summary>
		public void ResetRotation()
		{
			transform.localRotation = new Quaternion() ;
			LocalRotation =  transform.localEulerAngles ;
		}

		/// <summary>
		/// 縮尺をリセットする
		/// </summary>
		public void ResetScale()
		{
			transform.localScale = Vector3.one ;
			LocalScale    =  transform.localScale ;
		}

		// Tween などで使用する基準情報を保存する
		protected void SetLocalState()
		{
			LocalPosition =  transform.localPosition ;
			LocalRotation =  transform.localEulerAngles ;
			LocalScale    =  transform.localScale ;
		}
		
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
					return transform.localPosition ;
				}
			}
			set
			{
				m_LocalPosition = value ;
				transform.localPosition = value ;
			}
		}

		public Vector3 WorldPosition
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return transform.position ;
				}
				else
				{
					return transform.position ;
				}
			}
			set
			{
				transform.position = value ;
				m_LocalPosition = transform.localPosition ;
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
		/// <param name="tPosition"></param>
		public void SetPosition( Vector3 position )
		{
			Position = new Vector3( position.x, position.y, position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( float x, float y )
		{
			Position = new Vector3( x, y, Position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( float x, float y, float z )
		{
			Position = new Vector3( x, y, z ) ;
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
				if( Position.x != value )
				{
					Position = new Vector3( value, Position.y, Position.z ) ;
				}
			}
		}

		/// <summary>
		/// Ｘ座標(ショートカット)
		/// </summary>
		public float RPx
		{
			get
			{
				return transform.localPosition.x ;
			}
			set
			{
				transform.localPosition = new Vector3( value, transform.localPosition.y, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition ;
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
				if( Position.y != value )
				{
					Position = new Vector3( Position.x, value, Position.z ) ;
				}
			}
		}

		/// <summary>
		/// Ｙ座標(ショートカット)
		/// </summary>
		public float RPy
		{
			get
			{
				return transform.localPosition.y ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, value, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition;
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
				if( Position.z != value )
				{
					Position = new Vector3( Position.x, Position.y, value ) ;
				}
			}
		}

		public float RPz
		{
			get
			{
				return transform.localPosition.z ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, value ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return transform.forward ;
			}
			set
			{
				transform.forward = value ;
			}
		}

		public Vector3 Right
		{
			get
			{
				return transform.right ;
			}
			set
			{
				transform.right = value ;
			}
		}

		public Vector3 Up
		{
			get
			{
				return transform.up ;
			}
			set
			{
				transform.up = value ;
			}
		}

		//-----------------------------------------------------------

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
#if false
		public Quaternion Rotation
		{
			get
			{
				return transform.localRotation ;
			}
			set
			{
				transform.localRotation = value ;
			}
		}
#endif

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
					return transform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;
				transform.localEulerAngles = value ;
			}
		}

		public Vector3 EulerAngles
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					return transform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;
				transform.localEulerAngles = value ;
			}
		}

		public float Pitch
		{
			get
			{
				return EulerAngles.x ;
			}
			set
			{
				if( EulerAngles.x != value )
				{
					EulerAngles = new Vector3( value, EulerAngles.y, EulerAngles.z ) ;
				}
			}
		}


		public float Yaw
		{
			get
			{
				return EulerAngles.y ;
			}
			set
			{
				if( EulerAngles.y != value )
				{
					EulerAngles = new Vector3( EulerAngles.x, value, EulerAngles.z ) ;
				}
			}
		}

		public float Roll
		{
			get
			{
				return EulerAngles.z ;
			}
			set
			{
				if( EulerAngles.z != value )
				{
					EulerAngles = new Vector3( EulerAngles.x, EulerAngles.y, value ) ;
				}
			}
		}

		/// <summary>
		/// 視線と頭上から姿勢を設定する
		/// </summary>
		/// <param name="foward"></param>
		/// <param name="up"></param>
		public void SetDirection( Vector3 foward, Vector3 up )
		{
			foward.Normalize() ;
			up.Normalize() ;

			transform.forward	= foward ;
			transform.up		= up ;

			transform.right		= Vector3.Cross( up, foward ) ;
		}

		//-----------------------------------------------------------

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
					return transform.localScale ;
				}
			}
			set
			{
				m_LocalScale = value ;
				transform.localScale = value ;
			}
		}

		/// <summary>
		/// スケールをＸＹＺ一括で設定する
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( float scale )
		{
			Scale = new Vector3( scale, scale, scale ) ;
		}
		
		/// <summary>
		/// スケールをＸＹＺ一括で設定する
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector3 scale )
		{
			Scale = scale ;
		}
		
		//-------------------------------------------------------------------------------------------

		private Camera m_ChildCamera ;
		public  Camera   ChildCamera
		{
			get
			{
				if( m_ChildCamera != null )
				{
					return m_ChildCamera ;
				}
				m_ChildCamera = GetComponentInChildren<Camera>() ;
				return m_ChildCamera ;
			}
		}

		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		private Camera m_Camera = null ;
		public  Camera   Camera
		{
			get
			{
				if( m_Camera != null )
				{
					return m_Camera ;
				}
				m_Camera = GetComponent<Camera>() ;
				return m_Camera ;
			}
		}

		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定の親からの相対的な位置を取得する
		/// </summary>
		/// <param name="tParent"></param>
		/// <returns></returns>
		public Vector3 GetRelativePosition( SoftTransform parent )
		{
			return transform.position - parent.transform.position ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 空の GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public GameObject AddObject( string targetName, Transform targetTransform = null, int targetLayer = -1 )
		{
			GameObject go = new GameObject( targetName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( targetTransform == null )
			{
				targetTransform = transform ;
			}

			go.transform.SetParent( targetTransform, false ) ;

			if( targetLayer >= -1 && targetLayer <= 31 )
			{
				if( targetLayer == -1 )
				{
					targetLayer = targetTransform.gameObject.layer ;
				}
				SetLayer( go, targetLayer ) ;
			}

			return go ;
		}

		/// <summary>
		/// 指定のコンポーネントをアタッチしたの GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public T AddObject<T>( string targetName, Transform targetTransform = null, int targetLayer = -1 ) where T : UnityEngine.Component
		{
			GameObject go = new GameObject( targetName ) ;
			go.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			go.transform.localRotation = Quaternion.identity ;
			go.transform.localScale = Vector3.one ;

			if( targetTransform == null )
			{
				targetTransform = transform ;
			}

			go.transform.SetParent( targetTransform, false ) ;

			if( targetLayer >= -1 && targetLayer <= 31 )
			{
				if( targetLayer == -1 )
				{
					targetLayer = targetTransform.gameObject.layer ;
				}
				SetLayer( go, targetLayer ) ;
			}

			T component = go.AddComponent<T>() ;
			return component ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public GameObject AddPrefab( string path, Transform targetTransform = null, int targetLayer = -1 )
		{
			GameObject prefab = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( prefab == null )
			{
				return null ;
			}

			GameObject go = Instantiate( prefab ) ;
		
			AddPrefab( go, targetTransform, targetLayer ) ;
		
			return go ;
		}
	
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public GameObject AddPrefab( GameObject prefab, Transform targetTransform = null, int targetLayer = -1 )
		{
			if( prefab == null )
			{
				return null ;
			}
			
			if( targetTransform == null )
			{
				targetTransform = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( targetTransform, false ) ;

			if( targetLayer >= -1 && targetLayer <= 31 )
			{
				if( targetLayer == -1 )
				{
					targetLayer = targetTransform.gameObject.layer ;
				}
				SetLayer( go, targetLayer ) ;
			}

			return go ;
		}
		
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public T AddPrefab<T>( string path, Transform targetTransform = null, int targetLayer = -1 ) where T : UnityEngine.Component
		{
			GameObject prefab = Resources.Load( path, typeof( GameObject ) ) as GameObject ;
			if( prefab == null )
			{
				return null ;
			}

			return AddPrefab<T>( prefab, targetTransform, targetLayer ) ;
		}
		

		/// <summary>
		/// プレハブからインスタンスを生成し自身の子とする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParentName"></param>
		/// <returns></returns>
		public T AddPrefabOnChild<T>( GameObject prefab, string parentName = null, int targetLayer = -1 ) where T : UnityEngine.Component
		{
			Transform targetTransform = null ;
			if( string.IsNullOrEmpty( parentName ) == false )
			{
				if( transform.name.ToLower() == parentName.ToLower() )
				{
					targetTransform = transform ;
				}
				else
				{
					targetTransform = GetTransformByName( transform, parentName ) ;
				}
			}

			return AddPrefab<T>( prefab, targetTransform, targetLayer ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 自身に含まれる指定した名前のトランスフォームを検索する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Transform GetTransformByName( string targetName, bool isContain = false )
		{
			if( string.IsNullOrEmpty( targetName ) == true )
			{
				return null ;
			}

			return GetTransformByName( transform, targetName, isContain ) ;
		}

		// 自身に含まれる指定した名前のトランスフォームを検索する
		private Transform GetTransformByName( Transform targetTransform, string targetName, bool isContain = false )
		{
			targetName = targetName.ToLower() ;

			Transform childTransform ;
			string childName ;
			bool result ;

			int i, l = targetTransform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				childTransform = targetTransform.GetChild( i ) ;
				childName = childTransform.name.ToLower() ;
//				Debug.LogWarning( "n:[" + tChildName + "] : [" + tName +"]" ) ;
				result = false ;
				if( isContain == false && childName == targetName )
				{
					result = true ;
				}
				else
				if( isContain == true && childName.Contains( targetName ) == true )
				{
					result = true ;
				}

				if( result == true )
				{
					// 発見
					return childTransform ;
				}

				if( childTransform.childCount >  0 )
				{
					childTransform = GetTransformByName( childTransform, targetName ) ;
					if( childTransform != null )
					{
						// 発見
						return childTransform ;
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
		public T AddPrefab<T>( GameObject prefab, Transform targetTransform = null, int targetLayer = -1 ) where T : UnityEngine.Component
		{
			if( prefab == null )
			{
				return default ;
			}
			
			if( targetTransform == null )
			{
				targetTransform = transform ;
			}

			GameObject go = ( GameObject )GameObject.Instantiate( prefab ) ;
			if( go == null )
			{
				return null ;
			}
		
			go.transform.SetParent( targetTransform, false ) ;

			if( targetLayer >= -1 && targetLayer <= 31 )
			{
				if( targetLayer == -1 )
				{
					targetLayer = targetTransform.gameObject.layer ;
				}
				SetLayer( go, targetLayer ) ;
			}

			T component = go.GetComponent<T>() ;
			return component ;
		}

	

		/// <summary>
		/// 指定したゲームオブジェクトを自身の子にする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParent"></param>
		/// <returns></returns>
		public GameObject SetPrefab( GameObject go, Transform parentTransform = null )
		{
			if( go == null )
			{
				return null ;
			}
		
			if( parentTransform == null )
			{
				parentTransform = transform ;
			}

			go.transform.SetParent( parentTransform, false ) ;
			SetLayer( go, gameObject.layer ) ;

			return go ;
		}
	

		/// <summary>
		/// 指定のゲームオブジェクトを子も含めて指定のレイヤーに設定する
		/// </summary>
		/// <param name="tLayer"></param>
		public void SetLayer( int targetLayer )
		{
			SetLayer( gameObject, targetLayer ) ;
		}


		/// <summary>
		/// 指定のゲームオブジェクトを子も含めて指定のレイヤーに設定する
		/// </summary>
		/// <param name="tGameObject"></param>
		/// <param name="tLayer"></param>
		public static void SetLayer( GameObject go, int targetLayer )
		{
			go.layer = targetLayer ;

			int i, l = go.transform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				SetLayer( go.transform.GetChild( i ).gameObject, targetLayer ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// MonoBehaviour 直下には AddComponent が存在しないためバイパスメソッドを追加
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
	
		//-------------------------------------------------------------------------------------------

		internal void Update()
		{
#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tweenChecker = false ;
				SoftTransformTween[] tweenList = GetComponents<SoftTransformTween>() ;
				if( tweenList != null && tweenList.Length >  0 )
				{
					for( int i  = 0 ; i <  tweenList.Length ; i++ )
					{
						if( tweenList[ i ].IsChecker == true )
						{
							tweenChecker = true ;
							break ;
						}
					}
				}

				if( tweenChecker == false )
				{
					// ３つの値が異なっていれば更新する
					if( transform != null )
					{
						if( m_LocalPosition != transform.localPosition )
						{
							m_LocalPosition  = transform.localPosition ;
						}
						if( m_LocalRotation != transform.localEulerAngles )
						{
							m_LocalRotation  = transform.localEulerAngles ;
						}
						if( m_LocalScale != transform.localScale )
						{
							m_LocalScale  = transform.localScale ;
						}
					}
				}
			}

			RemoveComponents() ;

#endif
		}

#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( string.IsNullOrEmpty( m_RemoveTweenIdentity ) == false && m_RemoveTweenInstance != 0 )
			{
				RemoveTween( m_RemoveTweenIdentity, m_RemoveTweenInstance ) ;
				m_RemoveTweenIdentity = null ;
				m_RemoveTweenInstance = 0 ;
			}
		}
#endif

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
		public SoftTransformTween AddTween( string identity )
		{
			SoftTransformTween tween = gameObject.AddComponent<SoftTransformTween>() ;
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
			SoftTransformTween[] tweenList = GetComponents<SoftTransformTween>() ;
			if( tweenList == null || tweenList.Length == 0 )
			{
				return ;
			}
			int i, l = tweenList.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && tweenList[ i ].Identity == identity ) || ( instance != 0 && tweenList[ i ].Identity == identity && tweenList[ i ].GetInstanceID() == instance ) )
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
				DestroyImmediate( tweenList[ i ] ) ;
			}
			else
			{
				Destroy( tweenList[ i ] ) ;
			}
		}

		/// <summary>
		/// Tween の取得
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public SoftTransformTween GetTween( string identity )
		{
			SoftTransformTween[] tweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tweenArray == null || tweenArray.Length == 0 )
			{
				return null ;
			}

			int i, l = tweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweenArray[ i ].Identity == identity )
				{
					return tweenArray[ i ] ;
				}
			}

			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,SoftTransformTween> GetTweenAll()
		{
			SoftTransformTween[] tweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tweenArray == null || tweenArray.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,SoftTransformTween> list = new Dictionary<string, SoftTransformTween>() ;

			int i, l = tweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( tweenArray[ i ].Identity ) == false )
				{
					if( list.ContainsKey( tweenArray[ i ].Identity ) == false )
					{
						list.Add( tweenArray[ i ].Identity, tweenArray[ i ] ) ;
					}
				}
			}

			if( list.Count == 0 )
			{
				return null ;
			}

			return list ;
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
			SoftTransformTween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Delay		= delay ;
			tween.Duration	= duration ;

			return true ;
		}

		/// <summary>
		/// Tween の再生
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public bool PlayTween( string identity, float delay = -1, float duration = -1 )
		{
			SoftTransformTween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( tween.gameObject.activeSelf == false )
			{
				tween.gameObject.SetActive( true ) ;
			}

			tween.Play( delay, duration ) ;

			return true ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator PlayTween_Coroutine( string identity, float delay = -1, float duration = -1 )
		{
			SoftTransformTween tween = GetTween( identity ) ;
			if( tween == null )
			{
				yield break ;
			}

			if( tween.gameObject.activeSelf == false )
			{
				tween.gameObject.SetActive( true ) ;
			}

			yield return StartCoroutine( tween.Play_Coroutine( delay, duration ) ) ;
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlaying
		{
			get
			{
				SoftTransformTween[] tweenArray = gameObject.GetComponents<SoftTransformTween>() ;
				if( tweenArray == null || tweenArray.Length == 0 )
				{
					return false ;
				}

				int i, l = tweenArray.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tweenArray[ i ].enabled == true && ( tweenArray[ i ].IsRunning == true || tweenArray[ i ].IsPlaying == true ) )
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
			SoftTransformTween tween = GetTween( identity ) ;
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
			SoftTransformTween tween = GetTween( identity ) ;
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
			SoftTransformTween tween = GetTween( identity ) ;
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
			SoftTransformTween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.StopAndReset() ;

			return true ;
		}

		/// <summary>
		/// 全ての Tween の停止
		/// </summary>
		public bool StopTweenAll()
		{
			SoftTransformTween[] tweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tweenArray == null || tweenArray.Length == 0 )
			{
				return false ;
			}

			int i, l = tweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweenArray[ i ].enabled == true && ( tweenArray[ i ].IsRunning == true || tweenArray[ i ].IsPlaying == true ) )
				{
					tweenArray[ i ].Stop() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool StopAndResetTweenAll()
		{
			SoftTransformTween[] tweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tweenArray == null || tweenArray.Length == 0 )
			{
				return false ;
			}

			int i, l = tweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweenArray[ i ].enabled == true )
				{
					tweenArray[ i ].StopAndReset() ;
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
			SoftTransformTween tween = GetTween( identity ) ;
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
			SoftTransformTween tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.ProcessTime = time ;

			return true ;
		}

		//-------------------------------------------

	}
}

