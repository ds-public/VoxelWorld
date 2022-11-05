using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;
using UnityEngine.Video ;

using uGUIHelper ;

namespace DSW
{
	/// <summary>
	/// ムービー表示クラス Version 2022/09/21 0
	/// </summary>
	public class Movie : ExMonoBehaviour
	{
		// シングルトンインスタンス
		private static Movie m_Instance ; 

		/// <summary>
		/// フェードクラスのインスタンス
		/// </summary>
		public  static Movie   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------
		
		[SerializeField]
		protected VideoPlayer	m_VideoPlayer ;

		[SerializeField]
		protected AudioSource	m_AudioSource ;

		//-----------------------------------------------------------

		// キャンバス部分のインスタンス
		[SerializeField]
		protected UICanvas		m_Canvas ;

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

		/// <summary>
		/// キャンバスの表示順位を設定する
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public static bool SetCanvasOrder( int order )
		{
			if( m_Instance == null || m_Instance.m_Canvas == null )
			{
				return false ;
			}

			m_Instance.m_Canvas.SortingOrder = order ;

			return true ;
		}

		//-----------------------------------------------------------

		// スクリーン(全画面)
		[SerializeField]
		protected UIRawImage	m_Screen ;

		//-----------------------------------------------------------

		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public static bool IsPlaying
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsPlaying ;
			}
		}

		// 再生中かどうか
		[SerializeField]
		protected bool m_IsPlaying ;


		/// <summary>
		/// 一時停止中かどうか
		/// </summary>
		public static bool IsPausing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsPlaying ;
			}
		}

		// 一時停止中かどうか
		[SerializeField]
		protected bool m_IsPausing ;

		//-----------------------------------------------------------

		// キャンセル要求を出した際に呼ばれるコールバック(trueを返すと動画が終了する)
		private Func<UniTask<bool>> m_OnPaused ;

		// 終了時のコールバックメソッド
		private Action<bool> m_OnFinished ;

		// 再生用のレンダーテクスチャ
		private RenderTexture m_RenderTexture ;

		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			if( m_VideoPlayer == null )
			{
				m_VideoPlayer = GetComponent<VideoPlayer>() ;
				if( m_VideoPlayer == null )
				{
					m_VideoPlayer = gameObject.AddComponent<VideoPlayer>() ;
				}
			}

			if( m_AudioSource == null )
			{
				m_AudioSource = GetComponent<AudioSource>() ;
				if( m_AudioSource == null )
				{
					m_AudioSource = gameObject.AddComponent<AudioSource>() ;
				}
			}

			//----------------------------------------------------------

			m_VideoPlayer.playOnAwake		= false ;
			m_VideoPlayer.renderMode		= VideoRenderMode.RenderTexture ;
			m_VideoPlayer.aspectRatio		= VideoAspectRatio.FitInside ;
			m_VideoPlayer.audioOutputMode	= VideoAudioOutputMode.AudioSource ;
			m_VideoPlayer.SetTargetAudioSource( 0, m_AudioSource ) ;

			//----------------------------------------------------------

			// キャンバスの解像度を設定する
			float width  =  960 ;
			float height =  540 ;

			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				width  = settings.BasicWidth ;
				height = settings.BasicHeight ;
			}

			SetCanvasResolution( width, height ) ;
		}
		
		//-----------------------------------

		/// <summary>
		/// ムービーを再生する(同期)
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="onPaused"></param>
		/// <param name="onFinished"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public static bool Play( VideoClip clip, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( m_Instance == null || clip == null )
			{
				return false ;
			}

			m_Instance.gameObject.SetActive( true ) ;

			return m_Instance.Play_Private( clip, null, ( int )clip.width, ( int )clip.height, onPaused, onFinished, loop ) ;
		}

		public static bool Play( string path, int width, int height, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.gameObject.SetActive( true ) ;

			return m_Instance.Play_Private( null, path, width, height, onPaused, onFinished, loop ) ;
		}

		private bool Play_Private( VideoClip clip, string path, int width, int height, Func<UniTask<bool>> onPaused, Action<bool> onFinished, bool loop )
		{
			if( clip != null )
			{
				m_VideoPlayer.clip = clip ;
			}
			else
			if( string.IsNullOrEmpty( path ) == false )
			{
				m_VideoPlayer.url = path ;
			}
			else
			{
				m_Instance.gameObject.SetActive( false ) ;

				return false ;
			}

			//----------------------------------

			Process( width, height, onPaused, onFinished, loop ) ;

			return true ;
		}

		/// <summary>
		/// ムービーを再生する(非同期)
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="onPaused"></param>
		/// <param name="onFinished"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public static async UniTask<bool> PlayAsync( VideoClip clip, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( m_Instance == null || clip == null )
			{
				return false ;
			}

			m_Instance.gameObject.SetActive( true ) ;

			return await m_Instance.PlayAsync_Private( clip, null, ( int )clip.width, ( int )clip.height, onPaused, onFinished, loop ) ;
		}

		public static async UniTask<bool> PlayAsync( string path, int width, int height, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.gameObject.SetActive( true ) ;

			return await m_Instance.PlayAsync_Private( null, path, width, height, onPaused, onFinished, loop ) ;
		}

		private async UniTask<bool> PlayAsync_Private( VideoClip clip, string path, int width, int height, Func<UniTask<bool>> onPaused, Action<bool> onFinished, bool loop )
		{
			if( clip != null )
			{
				m_VideoPlayer.clip = clip ;
			}
			else
			if( string.IsNullOrEmpty( path ) == false )
			{
				m_VideoPlayer.url = path ;
			}
			else
			{
				m_Instance.gameObject.SetActive( false ) ;

				Debug.LogWarning( "Unknown source" ) ;
				return false ;
			}

			//----------------------------------

			Process( width, height, onPaused, onFinished, loop ) ;

			// ワンショット再生の場合のみ終了を待つ
			if( loop == false )
			{
				await WaitWhile( () => m_IsPlaying == true ) ;
			}

			return true ;
		}

		private void Process( int width, int height, Func<UniTask<bool>> onPaused, Action<bool> onFinished, bool loop )
		{
			m_VideoPlayer.isLooping = loop ;

			m_OnPaused		= onPaused ;
			m_OnFinished	= onFinished ;

			//----------------------------------
			// RenderTexture の設定

			if( m_RenderTexture == null || ( m_RenderTexture != null && ( m_RenderTexture.width != width || m_RenderTexture.height != height ) ) )
			{
				// レンダーテクスチャを作り直す
				m_VideoPlayer.targetTexture = null ;
				m_Screen.Texture = null ;

				DestroyImmediate( m_RenderTexture ) ;
				m_RenderTexture = null ;

				m_RenderTexture = new RenderTexture( width, height, 32, RenderTextureFormat.ARGB32 )
				{
					antiAliasing = 1,	// 無しにしないと表示されない(最低値は 1 )
					useMipMap = false
				} ;
				m_RenderTexture.Create() ;

				m_VideoPlayer.targetTexture	= m_RenderTexture ;
				m_Screen.Texture			= m_RenderTexture ;
			}

			if( m_Screen != null )
			{
				m_Screen.RaycastTarget = ( onPaused != null ) ;
				m_Screen.IsInteraction = ( onPaused != null ) ;
				if( onPaused!= null )
				{
					m_Screen.SetOnClick( OnClick ) ;
				}
				else
				{
					m_Screen.SetOnClick( null ) ;
				}
			}

			//----------------------------------

			// ネイティブ用のコールバックを設定する
			m_VideoPlayer.loopPointReached -= OnFinished ;
			m_VideoPlayer.loopPointReached += OnFinished ;

			// 再生する
			m_VideoPlayer.Play() ;

			// 再生中とする
			m_IsPlaying = true ;
			m_IsPausing = false ;
		}

		// 再生終了時に呼び出される
		private void OnFinished( VideoPlayer videoPlayer )
		{
			m_OnFinished?.Invoke( videoPlayer.isLooping ) ;

			if( videoPlayer.isLooping == false )
			{
				videoPlayer.loopPointReached -= OnFinished ;

				m_IsPlaying = false ;
				m_IsPausing = false ;

				gameObject.SetActive( false ) ;
			}
		}

		/// <summary>
		/// 停止させる
		/// </summary>
		/// <param name="path"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="onPaused"></param>
		/// <param name="onFinished"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public static bool Stop()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Stop_Private() ;
		}

		private bool Stop_Private()
		{
			if( m_VideoPlayer == null )
			{
				return false ;
			}

			// 終了時のコールバックを呼ばないようにする
			m_VideoPlayer.loopPointReached -= OnFinished ;

			// 停止
			m_VideoPlayer.Stop() ;

			//----------------------------------

			// 終了通知
			m_OnFinished?.Invoke( false ) ;

			m_IsPlaying = false ;
			m_IsPausing = false ;

			gameObject.SetActive( false ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// ダミーのスクリーンがクリックされた
		private void OnClick( string identity, UIView view )
		{
			ProcessPausing().Forget() ;
		}

		// 一時停止処理
		private async UniTask ProcessPausing()
		{
			if( m_OnPaused != null )
			{
				// 一時停止とその後の行動選択が設定されている

				// 一時停止実行
				m_VideoPlayer.Pause() ;
				m_IsPausing = true ;

				bool isUnpause = await m_OnPaused() ;

				if( isUnpause == true )
				{
					// 再開
					m_VideoPlayer.Play() ;
					m_IsPausing = false ;
				}
				else
				{
					// 完全停止
					Stop_Private() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// ポーズ処理のサンプル
#if false
		// ポーズ用コールバック
		private async UniTask<bool> OnPaused()
		{
			int index = await Dialog.Open( "確認", "一時停止中", new string[]{ "再開", "終了" } ) ;
			return ( index == 0 ) ;	// true で再開・false で終了
		}
#endif
	}
}
