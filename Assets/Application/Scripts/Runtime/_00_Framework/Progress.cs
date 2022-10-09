using System.Threading ;
using System.Collections ;

using UnityEngine ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;

using Cysharp.Threading.Tasks ;

namespace DBS
{
	/// <summary>
	/// プログレスクラス(待ち演出などに使用する) Version 2022/09/27 0
	/// </summary>
	public class Progress : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Progress m_Instance ;

		/// <summary>
		/// プログレスクラスのインスタンス
		/// </summary>
		public  static Progress   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

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

		/// <summary>
		/// キャンバスの仮想解像度を設定する
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static bool SetCanvasResolution( float width, float height )
		{
			if( m_Instance == null || m_Instance.m_Canvas == null )
			{
				return false ;
			}

			m_Instance.m_Canvas.SetResolution( width, height, true ) ;

			return true ;
		}

		//---------------------------------------------------------------------------

		// フェード部分のインスタンス
		[SerializeField]
		protected UIImage		m_Fade ;

		[SerializeField]
		protected UIImage		m_Animation ;

		[SerializeField]
		protected UITextMesh	m_Message ;

		//---------------------------------------------------------------------------

		// プログレス継続中のフラグ
		private bool m_On ;

		/// <summary>
		/// プログレス継続中のフラグ
		/// </summary>
		public static bool IsOn
		{
			get
			{
				if( m_Instance == null )
				{
					Debug.LogError( "Progress is not create !" ) ;
					return false ;
				}

				return m_Instance.m_On ;
			}

			set
			{
				if( m_Instance == null )
				{
					Debug.LogError( "Progress is not create !" ) ;
					return ;
				}

				m_Instance.m_On = value ;
			}
		}

		// プログレスを消去中かどうか(コルーチンを使用しているので連続実行を抑制する必要がある)
		private IEnumerator m_ActiveTask ;
		// ※必ずしもタスクが優れている訳ではない
		// 　UnityEditor が終了しても動き続けるのは場合によっては致命的な問題になる


		/// <summary>
		/// プログレスを表示中かどうか
		/// </summary>
		/// <returns></returns>
		public static bool IsShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				if( m_Instance.m_Fade.IsAnyTweenPlaying == true || m_Instance.m_ActiveTask != null )
				{
					return true ;	// 表示中
				}

				return false ;
			}
		}

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			float width  =  960 ;
			float height =  540 ;

			Settings settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			SetCanvasResolution( width, height ) ;

			m_Message.SetActive( false ) ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		public static bool Show( string message = null )
		{
			if( m_Instance == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress is not create !" ) ;
#endif
				return false ;
			}

			if( m_Instance.m_Fade == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress'fade is not create !" ) ;
#endif
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.Show_Private( message ) ;

			return true ;
		}

		// プログレスを表示する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void Show_Private( string message )
		{
			UITween tween ;

			if( m_ActiveTask != null )
			{
				StopCoroutine( m_ActiveTask ) ;
				m_ActiveTask = null ;

				if( m_Instance.gameObject.activeSelf == true )
				{
					tween = m_Fade.GetTween( "FadeOut" ) ;
					if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
					{
						// 非表示中なので終了させる
						tween.Finish() ;
					}
				}
			}

			if( m_Instance.gameObject.activeSelf == false )
			{
				m_Instance.gameObject.SetActive( true ) ;
			}

			tween = m_Fade.GetTween( "FadeIn" ) ;
			if( tween != null && tween.IsPlaying == true )
			{
				return ;	// 既に表示最中
			}

			// メッセージの有無
			if( string.IsNullOrEmpty( message ) == true )
			{
				m_Message.SetActive( false ) ;
			}
			else
			{
				m_Message.Text = message ;
				m_Message.SetActive( true ) ;
			}

			// 改めてフェードイン再生
			m_Fade.PlayTween( "FadeIn" ) ;

			// アニメーション再生
			m_Animation.PlayFlipper( "Move" ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		public static bool Hide()
		{
			if( m_Instance == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress is not create !" ) ;
#endif
				return false ;
			}

			if( m_Instance.m_Fade == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress'fade is not create !" ) ;
#endif
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.Hide_Private() ;

			return true ;
		}

		// プログレスを消去する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void Hide_Private()
		{
			UITween tween ;

			if( gameObject.activeSelf == false || m_ActiveTask != null )
			{
				// 既に非表示中
				return ;
			}
			
			tween = m_Fade.GetTween( "FadeIn" ) ;
			if( tween != null && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				// 表示中なので終了させる
				tween.Finish() ;
			}

			m_ActiveTask = HidingCoroutine_Private() ;
			StartCoroutine( m_ActiveTask ) ;
		}

		// プログレスを消去する(実装部・コルーチン)
		private IEnumerator HidingCoroutine_Private()
		{
			yield return m_Fade.PlayTween( "FadeOut" ) ;

			// アニメーション停止
			m_Animation.StopFlipper( "Move" ) ;

			gameObject.SetActive( false ) ;
			m_ActiveTask = null ;	// 非表示終了
		}

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		/// <returns></returns>
		public static bool On( string message = null )
		{
			if( m_Instance == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress is not create !" ) ;
#endif
				return false ;
			}

			if( m_Instance.m_Fade == null )
			{
#if UNITY_EDITOR
				Debug.LogError( "Progress'fade is not create !" ) ;
#endif
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.m_On = true ;

			m_Instance.Show_Private( message ) ;

			return true ;
		}

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		/// <returns></returns>
		public static bool Off()
		{
			if( m_Instance == null )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "Progress is not create !" ) ;
#endif
				return false ;
			}
			
			if( m_Instance.m_Fade == null )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "Progress'fade is not create !" ) ;
#endif
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.Hide_Private() ;

			m_Instance.m_On = false ;

			return true ;
		}

		/// <summary>
		/// プログレスを消去する(終了を待てる)
		/// </summary>
		/// <returns></returns>
		public static async UniTask OffAsync()
		{
			if( Off() == false )
			{
				return ;
			}

			if( m_Instance.gameObject.activeSelf == false )
			{
				return ;
			}

			//----------------------------------

			await m_Instance.WaitWhile( () => m_Instance.gameObject.activeSelf == true ) ;
		}
	}
}
