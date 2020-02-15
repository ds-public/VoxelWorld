using UnityEngine ;
using System.Collections ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// フェードクラス(画面全体のフェード演出に使用する) Version 2019/09/18 0
	/// </summary>
	public class Fade : MonoBehaviour
	{
		// シングルトンインスタンス
		private static Fade m_Instance ; 

		/// <summary>
		/// フェードクラスのインスタンス
		/// </summary>
		public  static Fade   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		// キャンバス部分のインスタンス
		[SerializeField]
		protected UICanvas m_Canvas ;

		// フェード部分のインスタンス
		[SerializeField]
		protected UIImage	m_Fade ;

		/// <summary>
		/// デフォルトの色
		/// </summary>
		public Color DefaultColor = new Color( 0, 0, 0, 1 ) ;

		/// <summary>
		/// デフォルトの遅延時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float DefaultDelay ;

		/// <summary>
		/// デフォルトの実行時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float DefaultDuration = -1 ;

		//-----------------------------------------------------------

		/// <summary>
		/// ＶＲ対応用にキャンバスを取得できるようにする
		/// </summary>
		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		//---------------------------------------------------------------------------

		private bool m_Showing ;

		/// <summary>
		/// 表示状態
		/// </summary>
		public static bool IsShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Showing ;
			}
		}

		// 実行状態
		private bool m_Playing ;

		/// <summary>
		/// 実行状態
		/// </summary>
		public static bool IsPlaying
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Playing ;
			}
		}

		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// フェードの状態クラス
		/// </summary>
		public class State : CustomYieldInstruction
		{
			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone ;
		}
		
		//---------------------------------------------------------------------------
		
		void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。
		}
		
		//-----------------------------------

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void Show( uint aarrggbb = 0x00000000 )
		{
			Color32 color= new Color32
			(
				( byte )( ( aarrggbb >> 16 ) & 0xFF ),
				( byte )( ( aarrggbb >>  8 ) & 0xFF ),
				( byte )( ( aarrggbb >>  0 ) & 0xFF ),
				( byte )( ( aarrggbb >> 24 ) & 0xFF )
			) ;

			Show( color ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void Show( Color color )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Show_Private( color ) ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void Show_Private( Color color )
		{
			if( m_Fade == null )
			{
				return ;
			}

			UITween tween = m_Fade.GetTween( "FadeOut" ) ;
			if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return ;	// 放っておいても表示になるので無視する
			}

			m_Fade.StopTweenAll() ;

			if( color.a <= 0 )
			{
				color = DefaultColor ;
			}

			m_Fade.Color = color ;

			m_Fade.Alpha = 1 ;

			gameObject.SetActive( true ) ;

			m_Showing = true ;
		}

		/// <summary>
		/// フェードを非表示にする
		/// </summary>
		public static void Hide()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Hide_Private() ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void Hide_Private()
		{
			if( m_Fade == null )
			{
				return ;
			}

			UITween tween = m_Fade.GetTween( "FadeIn" ) ;
			if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return ;	// 放っておいても非表示になるので無視する
			}

			m_Fade.StopTweenAll() ;

			gameObject.SetActive( false ) ;

			m_Showing = false ;
		}

		//---------------

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		public static Fade.State In( float delay = 0, float duration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			State state = new State() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.In_Private( delay, duration, state ) ) ;

			return state ;
		}

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator In_Private( float delay, float duration, State state )
		{
			if( m_Fade == null )
			{
				state.IsDone = true ;
				yield break ;
			}

			m_Playing = true ;

			m_Fade.Color = DefaultColor ;

			if( delay <  0 )
			{
				delay  = DefaultDelay ;
			}

			if( duration <  0 )
			{
				duration = DefaultDuration ;
			}

			m_Fade.PlayTween( "FadeIn", delay, duration ) ;

			yield return new WaitWhile( () => m_Fade.IsAnyTweenPlaying == true ) ;

			gameObject.SetActive( false ) ;

			m_Playing = false ;

			state.IsDone = true ;

			m_Showing = false ;
		}

		//---------------

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Fade.State Out( float delay = 0, float duration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			State state = new State() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.Out_Private( delay, duration, state ) ) ;

			return state ;
		}

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator Out_Private( float delay, float duration, State state )
		{
			if( m_Fade == null )
			{
				state.IsDone = true ;
				yield break ;
			}

			m_Playing = true ;

			m_Fade.Color = DefaultColor ;

			if( delay <  0 )
			{
				delay  = DefaultDelay ;
			}

			if( duration <  0 )
			{
				duration = DefaultDuration ;
			}

			m_Fade.PlayTween( "FadeOut", delay, duration ) ;

			yield return new WaitWhile( () => m_Fade.IsAnyTweenPlaying == true ) ;

			m_Playing = false ;

			state.IsDone = true ;

			m_Showing = true ;
		}
		
		//---------------

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void SetDefaultColor( uint aarrggbb )
		{
			Color32 color = new Color32
			(
				( byte )( ( aarrggbb >> 16 ) & 0xFF ),
				( byte )( ( aarrggbb >>  8 ) & 0xFF ),
				( byte )( ( aarrggbb >>  0 ) & 0xFF ),
				( byte )( ( aarrggbb >> 24 ) & 0xFF )
			) ;

			SetDefaultColor( color ) ;
		}

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tColor">色</param>
		public static void SetDefaultColor( Color color )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultColor = color ;
		}

		/// <summary>
		/// フェードのデフォルトの遅延時間を設定する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		public static void SetDefaultDelay( float delay )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultDelay = delay ;
		}

		/// <summary>
		/// フェードのデフォルトの実行時間を設定する
		/// </summary>
		/// <param name="tDuration">実行時間(秒)</param>
		public static void SetDefaultDuration( float duration )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.DefaultDuration = duration ;
		}
	}
}
