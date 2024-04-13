using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// Flipper コンポーネントクラス
	/// </summary>
	[ ExecuteInEditMode ]
	// ＵＩの簡易アニメーション用のコンポーネント
	public class SpriteFlipper : MonoBehaviour
	{
		// 識別名
		[SerializeField]
		protected string m_Identity ;
		public    string   Identity{ get{ return m_Identity ; } set{ m_Identity = value ; } }

		// 開始までにかかる時間
		[SerializeField]
		protected float m_Delay ;
		public    float   Delay{ get{ return m_Delay ; } set{ m_Delay = value ; } }

		//------------------------------------------------------

		[SerializeField][HideInInspector]
		private SpriteFlipperAnimation	m_SpriteAnimation ;	// スプライトアニメーション
		public  SpriteFlipperAnimation    SpriteAnimation
		{
			get
			{
				return m_SpriteAnimation ;
			}
			set
			{
				if( m_SpriteAnimation != value )
				{
					m_SpriteAnimation  = value ;

					Image image = GetComponent<Image>() ;

					if( m_SpriteAnimation != null )
					{
						if( image != null )
						{
							image.sprite = m_SpriteAnimation.GetSpriteOfFrame( 0 ) ;
						}

						m_Begin = 0 ;
						m_End   = m_SpriteAnimation.Length - 1 ;
					}
					else
					{
						if( image != null )
						{
							image.sprite = null ;
						}

						m_Begin = 0 ;
						m_End   = 0 ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_Begin = 0 ;			// 開始フレームのインデックス番号
		public  int   Begin
		{
			get
			{
				return m_Begin ;
			}
			set
			{
				if( m_Begin != value && m_End != value && m_SpriteAnimation != null )
				{
					if( m_Begin >= 0 && m_Begin <  m_SpriteAnimation.Length )
					{
						m_Begin = value ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_End = 0 ;				// 終了フレームのインデックス番号
		public  int   End
		{
			get
			{
				return m_End ;
			}
			set
			{
				if( m_End != value && m_Begin != value && m_SpriteAnimation != null )
				{
					if( m_End >= 0 && m_End <  m_SpriteAnimation.Length )
					{
						m_End = value ;
					}
				}
			}
		}

		private int m_Current = 0 ;				// カレントフレーム
		public  int   Current
		{
			get
			{
				return m_Current ;
			}
			set
			{
				if( m_Current != value && m_SpriteAnimation != null )
				{
					if( m_Current >= 0 && m_Current <  m_SpriteAnimation.Length )
					{
						m_Current = value ;
					}
				}
			}
		}
		
		/// <summary>
		/// 逆再生(しない)
		/// </summary>
		[SerializeField]
		protected bool m_Back ;
		public    bool   Back{ get{ return m_Back ; } set{ m_Back = value ; } }


		/// <summary>
		/// ループ(しない)
		/// </summary>
		[SerializeField]
		protected bool m_Loop ;
		public    bool   Loop{ get{ return m_Loop ; } set{ m_Loop = value ; } }


		/// <summary>
		/// ループ後の反転(しない)
		/// </summary>
		[SerializeField]
		protected bool m_Reverse ;
		public    bool   Reverse{ get{ return m_Reverse ; } set{ m_Reverse = value ; } }


		/// <summary>
		/// スピード倍率
		/// </summary>
		[SerializeField]
		protected float m_Speed = 1.0f ;
		public    float   Speed{ get{ return m_Speed ; } set{ m_Speed = value ; } }



		/// <summary>
		/// タイムスケールを無視するかどうか（しない）
		/// </summary>
		[SerializeField]
		protected bool m_IgnoreTimeScale ;
		public    bool   IgnoreTimeScale{ get{ return m_IgnoreTimeScale ; } set{ m_IgnoreTimeScale = value ; } }


		/// <summary>
		/// 起動と同時に実行するかどうか（する）
		/// </summary>
		[SerializeField]
		protected bool m_PlayOnAwake = true ;
		public    bool   PlayOnAwake{ get{ return m_PlayOnAwake ; } set{ m_PlayOnAwake = value ; } }


		/// <summary>
		/// 終了と同時にゲームオブジェクトを破棄するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_DestroyAtEnd ;
		public    bool   DestroyAtEnd{ get{ return m_DestroyAtEnd ; } set{ m_DestroyAtEnd = value ; } }


		/// <summary>
		/// イメージの大きさを各フレームのサイズに合わせるかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AutoResize = true ;
		public    bool   AutoResize{ get{ return m_AutoResize ; } set{ m_AutoResize = value ; } }

		//------------------------------------------------------

		private int m_Count = 0 ;

		private SpriteRenderer		m_Renderer ;
//		private Transform		m_Transform ;

		private SpriteController	m_Controller ;
		public  SpriteController	  Controller
		{
			get
			{
				return m_Controller ;
			}
		}
		
		private float   m_Time ;
		private float	m_BaseTime ;

		private enum State
		{
			Delay = 0,
			Play  = 1,
			Finished = 2,
		}

		private State m_State = State.Delay ;

		// 実行状況
		private bool m_Running = false ;
		public  bool IsRunning{ get{ return m_Running ; } }

		// 再生状況
		private bool m_Playing = false ;
		public  bool IsPlaying{ get{ return m_Playing ; } }
	
		//--------------------------------------------------------

		public UnityEvent OnFinished = new () ;

		private Action<string, SpriteFlipper> m_OnFinishedAction ;
		public  Action<string, SpriteFlipper>   OnFinishedAction
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
		public void SetOnFinished( Action<string, SpriteFlipper> onFinishAction )
		{
			m_OnFinishedAction = onFinishAction ;
		}


		//--------------------------------------------------------

		//--------------------------------------------------------

		internal void  OnDisable()
		{
			// 無効化した際にチェッカーが開いていれば強制的に閉じる
			m_IsChecker = false ;
		}

		internal void Awake()
		{
			if( Application.isPlaying == true && m_IsChecker == true )
			{
				LoadState() ;
			}
		}
	
		internal void Start()
		{
			if( Application.isPlaying == true && m_PlayOnAwake == true )
			{
				// カスタムリスナー登録
				OnFinished.AddListener( OnFinishedInner ) ;
			
				Play() ;
			}
		}
	
		//---------------------------------------------

		// 内部リスナー
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
	
		/// <summary>
		/// 再生する(コルーチン)
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator Play_Coroutine( bool destroyAtEnd = false, float speed = 0, float delay = -1 )
		{
			Play( destroyAtEnd, speed, delay ) ;

			while( m_Playing == true )
			{
				yield return null ;
			}
		}

		/// <summary>
		/// 再生する
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tSpeed"></param>
		public void Play( bool destroyAtEnd = false, float speed = 0, float delay = -1, Action<string,SpriteFlipper> onFinishedAction = null )
		{
			if( m_SpriteAnimation == null )
			{
				return ;	// 再生出来ない
			}

			enabled = true ;

			if( onFinishedAction != null )
			{
				// 既に設定されている可能性がある
				m_OnFinishedAction = onFinishedAction ;
			}

			if( m_Running == true )
			{
				// 既に再生中
				if( m_Playing == true )
				{
					return ;
				}
				else
				{
					// リスタート
					m_BaseTime = Time.realtimeSinceStartup ;

					m_Playing = true ;
					return ;
				}
			}

			//-------------------------------------------


			if( delay >= 0 )
			{
				m_Delay = delay ;
			}

			if( speed >  0 )
			{
				m_Speed = speed ;
			}

			if( destroyAtEnd == true )
			{
				m_DestroyAtEnd = true ;
			}

			if( m_Renderer == null )
			{
				m_Renderer  = gameObject.GetComponent<SpriteRenderer>() ;
			}

			if( m_Renderer != null && m_Delay >  0 )
			{
				m_Renderer.enabled = false ;
			}

			//-------------------------------

			m_Controller = gameObject.GetComponent<SpriteController>() ;

			//-------------------------------

			m_State = State.Delay ;

			m_Current = m_Begin ;

			m_Time = 0 ;
			m_Count = 0 ;



			Modify( m_Current ) ;	// 初期位置へ移動させる

			m_BaseTime = Time.realtimeSinceStartup ;

			m_Running = true ;
			m_Playing = true ;
		}

		/// <summary>
		/// 一時停止する
		/// </summary>
		public void Pause()
		{
			if( m_Running == true )
			{
				m_Playing = false ;
			}
		}

		/// <summary>
		/// 完全停止する
		/// </summary>
		public void Stop()
		{
			if( m_Running == true )
			{
				m_Playing = false ;
				m_Running = false ;
			}
		}

		internal void Update()
		{
			if( m_Running == true && m_Playing == true )
			{
				m_Time += GetDeltaTime() ;
			
				if( m_State == State.Delay )
				{
					if( m_Renderer != null && m_Renderer.enabled == true )
					{
						m_Renderer.enabled = false ;
					}

					// ディレイはタイムスケール値を無視する(ただし Time.timeScale は除く)
					if( m_Time <  m_Delay )
					{
						return ;
					}
					else
					{
						m_State = State.Play ;
//						m_Time = m_Time - tDelay ;
						m_Time = 0 ;
					}
				}

				if( m_State == State.Play )
				{
					if( m_Renderer != null && m_Renderer.enabled == false )
					{
						m_Renderer.enabled = true ;
					}

					if( m_Time <  m_SpriteAnimation[ m_Current ].Duration )
					{
						return ;
					}
				
					while( true )
					{
						m_Time -= m_SpriteAnimation[ m_Current ].Duration ;

						bool back = m_Back ;
						if( m_Loop == true && m_Reverse == true )
						{
							if( ( m_Count % 1 ) == 1 )
							{
								back = ! back ;
							}
						}

						if( back == false )
						{
							m_Current = ( m_Current + 1 ) % m_SpriteAnimation.Length ;
						}
						else
						{
							m_Current = ( m_Current - 1 + m_SpriteAnimation.Length ) % m_SpriteAnimation.Length ;
						}

						if( m_Current == m_End )
						{
							// 終了
							if( m_Loop == false )
							{
								m_State = State.Finished ;
								break ;
							}
							else
							{
								m_Count ++ ;
							}
						}

						if( m_Time <  m_SpriteAnimation[ m_Current ].Duration )
						{
							break ;
						}
					}

					Modify( m_Current ) ;
				}
				else
				if( m_State == State.Finished )
				{
					// 最終フレーム

					if( m_Time <  m_SpriteAnimation[ m_Current ].Duration )
					{
						return ;
					}
				
					// 完全終了
					m_Playing = false ;
					m_Running = false ;

					// コールバック呼び出し
					OnFinished.Invoke() ;

					if( m_DestroyAtEnd == true )
					{
						Destroy( gameObject ) ;
					}
				}
			}
		}

		private void Modify( int frameIndex )
		{
			if( m_SpriteAnimation != null && m_Renderer != null )
			{
				// 指定したフレームのスプライトを表示する
				m_Renderer.sprite = SpriteAnimation.GetSpriteOfFrame( frameIndex ) ;

				// コリジョンを自動追従させるかもしれない
//				if( m_AutoResize == true )
//				{
//					if( m_Renderer.sprite != null  )
//					{
//						m_Renderer.sizeDelta = new Vector2( m_Image.sprite.rect.width, m_Image.sprite.rect.height ) ;
//					}
//				}
			}
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			float baseTime = Time.realtimeSinceStartup ;
			float deltaTime = baseTime - m_BaseTime ;

			m_BaseTime = baseTime ;

			// 独自のスピードを乗算する
			deltaTime = deltaTime * m_Speed * m_SpriteAnimation.TimeScale ;

			if( m_IgnoreTimeScale == false )
			{
				// タイムスケールを無視しない
				deltaTime *= Time.timeScale ;
			}

			return deltaTime ;
		}

		//--------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Sprite m_InitialSprite ;

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
						SetCheckFactor( m_CheckFactor ) ;
					}
					else
					{
						LoadState() ;
					}
				}
			}
		}

		// ただの位置表示用
		[SerializeField][HideInInspector]
		private int m_CheckFactor = 0 ;
		public  int   CheckFactor
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
						SetCheckFactor( m_CheckFactor ) ;
					}
				}
			}
		}

		// モーションチェック専用で現在の状態を退避する
		public void SaveState()
		{
			if( TryGetComponent<SpriteRenderer>( out var renderer ) == true )
			{
				m_InitialSprite = renderer.sprite ;
			}
		}

		// モーションチェック専用で現在の状態を復帰する
		private void LoadState()
		{
			if( TryGetComponent<SpriteRenderer>( out var renderer ) == true )
			{
				renderer.sprite = m_InitialSprite ;
			}
		}

		// デバッグ用
		private void SetCheckFactor( int checkFactor )
		{
//			m_Renderer = gameObject.GetComponent<SpriteRenderer>() ;

			//-----------------

			Modify( checkFactor ) ;

//			m_Renderer.GraphicUpdateComplete() ;
		}
	}
}

