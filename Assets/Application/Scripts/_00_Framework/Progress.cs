using UnityEngine ;
using System.Collections ;

// 要 uGUIHelper パッケージ
using uGUIHelper ;

namespace DBS
{
	/// <summary>
	/// プログレスクラス(待ち演出などに使用する) Version 2019/09/18 0
	/// </summary>
	public class Progress : MonoBehaviour
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

		//---------------------------------------------------------------------------

		// フェード部分のインスタンス
		[SerializeField]
		protected UIImage	m_Fade ;

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
#if UNITY_EDITOR
				Debug.LogError( "Progress is not create !" ) ;
#endif
					return false ;
				}

				return m_Instance.m_On ;
			}

			set
			{
				if( m_Instance == null )
				{
#if UNITY_EDITOR
				Debug.LogError( "Progress is not create !" ) ;
#endif
					return ;
				}

				m_Instance.m_On = value ;
			}
		}

		// プログレスを消去中かどうか(コルーチンを使用しているので連続実行を抑制する必要がある)
		private IEnumerator m_HidingCoroutine ;

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

				if( m_Instance.m_Fade.IsAnyTweenPlaying == true || m_Instance.m_HidingCoroutine != null )
				{
					return true ;	// 表示中
				}

				return false ;
			}
		}

		//---------------------------------------------------------------------------

		void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		public static bool Show()
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

			m_Instance.Show_Private() ;

			return true ;
		}

		// プログレスを表示する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void Show_Private()
		{
			UITween tween ;

			if( m_HidingCoroutine != null )
			{
				StopCoroutine( m_HidingCoroutine ) ;
				m_HidingCoroutine = null ;

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

			// 改めてフェードイン再生
			m_Fade.PlayTween( "FadeIn" ) ;
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

			if( gameObject.activeSelf == false || m_HidingCoroutine != null )
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

			m_HidingCoroutine = HidingCoroutine_Private() ;
			StartCoroutine( m_HidingCoroutine ) ;
		}

		// プログレスを消去する(実装部・コルーチン)
		private IEnumerator HidingCoroutine_Private()
		{
			m_Fade.PlayTween( "FadeOut" ) ;
			yield return new WaitWhile( () => m_Instance.m_Fade.IsAnyTweenPlaying == true ) ;	// フェードアウトが完了するのを待つ

			gameObject.SetActive( false ) ;
			m_HidingCoroutine = null ;	// 非表示終了
		}

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		/// <returns></returns>
		public static bool On()
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

			m_Instance.Show_Private() ;

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

			m_Instance.m_On = false ;

			return true ;
		}
	}
}
