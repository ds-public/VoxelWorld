using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Video ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

namespace DSW.Screens.DownloadingClasses.UI
{
	/// <summary>
	/// ムービー表示パネル
	/// </summary>
	public partial class MoviePanel : ExMonoBehaviour
	{
		private UIView				m_View ;

		/// <summary>
		/// パネルのビューを取得する
		/// </summary>
		public	UIView				  View
		{
			get
			{
				if( m_View == null )
				{
					m_View  = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}

		[SerializeField]
		protected UIButton		m_SkipButton ;

		[SerializeField]
		protected UIImage		m_Mask ;

		[SerializeField]
		protected UIView		m_Blocker ;

		//-----------------------------------

		[Header( "ムービー関連" )]

		[SerializeField]
		protected VideoPlayer	m_VideoPlayer ;

		[SerializeField]
		protected AudioSource	m_AudioSource ;

		// スクリーン(全画面)
		[SerializeField]
		protected UIRawImage	m_RenderImage ;

		//-----------------------------------------------------------

		[Header( "動的変化情報" )]

		// ムービーを再生中かどうか
		[SerializeField]
		protected bool	m_IsPlaying = false ;

		/// <summary>
		/// ムービーを再生中かどうか
		/// </summary>
		public	bool	  IsPlaying	=> m_IsPlaying ;

		// ムービーを一時停止中かどうか
		[SerializeField]
		protected bool	m_IsPausing = false ;

		/// <summary>
		/// ムービーを一時停止中かどうか
		/// </summary>
		public	bool	  IsPausing	=> m_IsPausing ;

		//-----------------------------------------------------------

		// キャンセル要求を出した際に呼ばれるコールバック(trueを返すと動画が終了する)
		private Func<UniTask<bool>> m_OnPaused ;

		// 終了時のコールバックメソッド
		private Action<bool> m_OnFinished ;

		// 再生用のレンダーテクスチャ
		private RenderTexture m_RenderTexture ;

		//-----------------------------------------------------------

		internal void Awake()
		{
			m_SkipButton.SetActive( false ) ;

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
		}


		/// <summary>
		/// ムービーを再生する
		/// </summary>
		/// <returns></returns>
		public async UniTask Play( string path, Action onCompleted = null )
		{
			if( m_IsPlaying == true )
			{
				return ;	// 多重再生禁止
			}

			m_IsPlaying = true ;

			//----------------------------------------------------------

			m_Mask.SetActive( true ) ;
			m_Blocker.SetActive( true ) ;

			//----------------------------------------------------------

			// スキップボタンのコールバックを設定する
			m_SkipButton.SetActive( false ) ;
			m_SkipButton.SetOnSimpleClick( () =>
			{
				SE.Play( SE.Cancel ) ;

				if( m_IsPlaying == true )
				{
					// ムービーをスキップする
					Stop() ;
				}

				m_Blocker.SetActive( true ) ;
			} ) ;

			//----------------------------------

			// ムービー有効化
//			m_MoviePlayer.SetActive( true ) ;

			// ロードする
//			await m_MoviePlayer.SetupFromAssetBundle( path, this.GetCancellationTokenOnDestroy() ) ;

			//----------------------------------
			// ダウンロード中のムービーはスキップできない
#if false
			// 画面がタップされたらムービーを一時停止してスキップするか確認ダイアログを出す
			m_MoviePlayer.SetOnSimpleClick( () =>
			{
				PauseMovie().Forget() ;
			} ) ;
#endif
			//----------------------------------

			m_Blocker.SetActive( false ) ;
			m_Mask.SetActive( false ) ;

			// // ムービー再生

//			Debug.Log( "<color=#FFFF00>ムービー再生を開始する</color>" ) ;

			// アセットバンドル版
			var clip = await Asset.LoadAsync<VideoClip>( path ) ;
			if( clip != null )
			{
				await PlayAsync( clip ) ;
			}
			else
			{
				Debug.LogWarning( "Can not load movie : Path = " + path ) ;
			}
#if false
			// オリジナルソース版
			await PlayAsync( Asset.GetNativePath( path ), 640, 360, loop:false ) ;
#endif
//			Debug.Log( "<color=#FFFF00>ムービー再生が終了した</color>" ) ;

			//----------------------------------

			// 再生終了時に呼ぶコールバック
			onCompleted?.Invoke() ;

			// 再生終了
			m_IsPlaying = false ;

			m_Blocker.SetActive( false ) ;
		}

		//-------------------------------------------------------------------------------------------
		// ダウンロード中のムービーはスキップできない
#if false
		// ポーズとスキップ確認
		private async UniTask PauseMovie()
		{
			// ムービーの一時停止
			m_MoviePlayer.PauseMovie() ;

			int index = await Dialog.Open
			(
				"スキップ",
				"ムービーをスキップしますか？",
				new string[]
				{
					"はい",
					"いいえ",
				}
			) ;

			if( index == 0 )
			{
				m_Blocker.SetActive( true ) ;
				
				// スキップ
				m_MoviePlayer.StopMoviePlaying() ;
			}
			else
			{
				// ムービー再生の再開
				m_MoviePlayer.ResumeMovie() ;
			}
		}
#endif

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スキップボタンを表示する
		/// </summary>
		/// <returns></returns>
		public async UniTask ShowSkipButton()
		{
			if( m_SkipButton.ActiveSelf == true )
			{
				return ;
			}

			await When( m_SkipButton.PlayTween( "FadeIn" ) ) ;
		}

		/// <summary>
		/// スキップボタンを消去する
		/// </summary>
		/// <returns></returns>
		public async UniTask HideSkipButton()
		{
			if( m_SkipButton.ActiveSelf == false )
			{
				return ;
			}

			await When( m_SkipButton.PlayTweenAndHide( "FadeOut" ) ) ;
		}


		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			if( View.ActiveSelf == true )
			{
				return ;
			}

			await When( View.PlayTween( "FadeIn" ) ) ;
		}

		/// <summary>
		/// フェードアウト
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			await When( View.PlayTweenAndHide( "FadeOut" ) ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ムービーを再生する(同期)
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="onPaused"></param>
		/// <param name="onFinished"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public bool Play( VideoClip clip, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( clip == null )
			{
				return false ;
			}

			return Play_Private( clip, null, ( int )clip.width, ( int )clip.height, onPaused, onFinished, loop ) ;
		}

		public bool Play( string path, int width, int height, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			return Play_Private( null, path, width, height, onPaused, onFinished, loop ) ;
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
		public async UniTask<bool> PlayAsync( VideoClip clip, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			if( clip == null )
			{
				return false ;
			}

			return await PlayAsync_Private( clip, null, ( int )clip.width, ( int )clip.height, onPaused, onFinished, loop ) ;
		}

		public async UniTask<bool> PlayAsync( string path, int width, int height, Func<UniTask<bool>> onPaused = null, Action<bool> onFinished = null, bool loop = false )
		{
			return await PlayAsync_Private( null, path, width, height, onPaused, onFinished, loop ) ;
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
				m_RenderImage.Texture = null ;

				DestroyImmediate( m_RenderTexture ) ;
				m_RenderTexture = null ;

				m_RenderTexture = new RenderTexture( width, height, 32, RenderTextureFormat.ARGB32 )
				{
					antiAliasing = 1,	// 無しにしないと表示されない(最低値は 1 )
					useMipMap = false
				} ;
				m_RenderTexture.Create() ;

				m_VideoPlayer.targetTexture	= m_RenderTexture ;
				m_RenderImage.Texture		= m_RenderTexture ;
			}

			if( m_RenderImage != null )
			{
				m_RenderImage.RaycastTarget = ( onPaused != null ) ;
				m_RenderImage.IsInteraction = ( onPaused != null ) ;
				if( onPaused!= null )
				{
					m_RenderImage.SetOnClick( OnClick ) ;
				}
				else
				{
					m_RenderImage.SetOnClick( null ) ;
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

//				gameObject.SetActive( false ) ;
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
		public bool Stop()
		{
			return Stop_Private() ;
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

//			gameObject.SetActive( false ) ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// ダミーのスクリーンがクリックされた
		private void OnClick( string identity, UIView view )
		{
			ProcessPausing().Forget() ;
		}

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
