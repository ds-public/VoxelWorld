using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

// 要 AudioHelper パッケージ
using AudioHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// ＢＧＭクラス Version 2019/09/13 0
	/// </summary>
	public class BGM
	{
		// ＢＧＭ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)

		/// <summary>
		/// タイトル
		/// </summary>
		public const string Title	= "BGM001" ;

		/// <summary>
		/// ホーム
		/// </summary>
		public const string Home	= "BGM002" ;

		/// <summary>
		/// メンバー
		/// </summary>
		public const string Member	= "BGM002" ;

		/// <summary>
		/// 編成
		/// </summary>
		public const string Unit	= "BGM002" ;

		/// <summary>
		/// クエスト
		/// </summary>
		public const string Quest	= "BGM002" ;

		/// <summary>
		/// エリア
		/// </summary>
		public const string Area	= "BGM002" ;

		/// <summary>
		/// ガチャ
		/// </summary>
		public const string Gacha	= "BGM002" ;


		/// <summary>
		/// バトル
		/// </summary>
		public const string Battle_01	= "BGM_101_Battle_01" ;
		public const string Battle_02	= "BGM_102_Battle_02" ;
		public const string Battle_03	= "BGM_102_Battle_02" ;

		/// <summary>
		/// バトルリザルト
		/// </summary>
		public const string BattleResult = "BGM006" ;


		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path = "Sounds/BGM" ;

		//-----------------------------------------------------------

		/// <summary>
		/// 状態クラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == true || string.IsNullOrEmpty( Error ) == false )
					{
						return false ;   // 終了
					}
					return true ;    // 継続
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラー
			/// </summary>
			public string Error = String.Empty ;

			/// <summary>
			/// 再生識別子
			/// </summary>
			public int PlayId = -1 ;
		}

		//-----------------------------------------------------------

		// メインＢＧＭのオーディオチャンネルインスタンスを保持する
		private static int		m_MainBGM_PlayId	= -1 ;
		private static float	m_MainBGM_Volume	=  1 ;

		// パスの保険
		private static string CorrectPath( string path )
		{
			// 保険をかける
			if( path.Contains( "//" ) == false )
			{
				// アセットバンドルのパス指定が無い
				int p = path.LastIndexOf( '/' ) ;
				if( p >= 0 )
				{
					path = path.Substring( 0, p ) + "//" + path.Substring( p + 1, path.Length - ( p + 1 ) ) ;
				}
			}
			return path ;
		}
		
		/// <summary>
		/// メインＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMain( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float delay = 0.0f )
		{
			path = CorrectPath( path ) ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingType.None ) ;
			if( audioClip == null )
			{
				// 失敗
				return false ;
			}

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_PlayId = -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_PlayId = -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, delay ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				return false ;
			}

			m_MainBGM_PlayId	= playId ;
			m_MainBGM_Volume	= volume ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>列挙子</returns>
		public static Request PlayMainAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float delay = 0.0f )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( PlayMainAsync_Private( path, fade, volume, pan, loop, delay, request ) ) ;
			return request ;
		}

		private static IEnumerator PlayMainAsync_Private( string path, float fade, float volume, float pan, bool loop, float delay, Request request )
		{
			path = CorrectPath( path ) ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら同期で高速再生
				PlayMain( path, fade, volume, pan, loop, delay ) ;
				request.PlayId = m_MainBGM_PlayId ;
				request.IsDone = true ;
				yield break ;
			}

			AudioClip audioClip = null ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			yield return Asset.LoadAsync<AudioClip>( m_Path + "/" + path, ( _ ) => { audioClip = _ ; }, Asset.CachingType.None ) ;
			if( audioClip == null )
			{
				// 失敗
				request.Error = "Could not load." ;
				yield break ;
			}

			// ＢＧＭを再生する
			int playId ;
			
			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_PlayId = -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_PlayId = -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, delay ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				request.Error = "Could not play." ;
				yield break ;
			}

			m_MainBGM_PlayId	= playId ;
			m_MainBGM_Volume	= volume ;
			
			// 成功
			request.PlayId = m_MainBGM_PlayId ;
			request.IsDone = true ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを停止する
		/// </summary>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopMain( float fade = 0 )
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_PlayId = -1 ;
				return false ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
			}

			m_MainBGM_PlayId	= -1 ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="tFade"></param>
		/// <returns></returns>
		public static Request StopMainAsync( float fade = 0 )
		{
			if( m_MainBGM_PlayId <  0 )
			{
				return null ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_PlayId = -1 ;
				return null ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;

				Request request = new Request()
				{
					PlayId = m_MainBGM_PlayId,
					IsDone = true
				} ;
				return request ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
				
				Request request = new Request() ;
				ApplicationManager.Instance.StartCoroutine( StopMainAsync_Private( request ) ) ;
				return request ;
			}
		}

		private static IEnumerator StopMainAsync_Private( Request request )
		{
			yield return new WaitWhile( () => AudioManager.IsPlaying( m_MainBGM_PlayId ) == true ) ;

			request.PlayId = m_MainBGM_PlayId ;
			request.IsDone = true ;

			m_MainBGM_PlayId = -1 ;
		}

		/// <summary>
		/// メインＢＧＭのボリュームを設定する(オプションからリアルタイムに変更するケースで使用する value = GetVolume() )
		/// </summary>
		public static float Volume
		{
			set
			{
				if( m_MainBGM_PlayId >= 0 )
				{
					AudioManager.SetVolume( m_MainBGM_PlayId, m_MainBGM_Volume * value ) ;
				}
			}
		}
		
		/// <summary>
		/// メインＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="tName">曲の名前(再生中の曲の種類を限定したい場合は指定する)</param>
		/// <returns>再生状況(true=再生中・false=停止中)</returns>
		public static bool IsPlayingMain( string audioClipName = null )
		{
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				return false ;
			}

			// 何かしらの曲は鳴っている
			if( string.IsNullOrEmpty( audioClipName ) == false )
			{
				// 曲の名前の指定がある
				if( AudioManager.GetName( m_MainBGM_PlayId ) != audioClipName )
				{
					// 指定した曲は鳴っていない
					return false ;
				}
			}

			return true ;
		}

		/// <summary>
		/// 再生中のメインＢＧＭの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetMainName()
		{
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId )== false )
			{
				return string.Empty ;
			}

			return AudioManager.GetName( m_MainBGM_PlayId ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子</returns>
		public static int Play( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float delay = 0.0f )
		{
			path = CorrectPath( path ) ;

			AudioClip audioClip ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingType.None ) ;
			if( audioClip == null )
			{
				// 失敗
				return -1 ;
			}

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, delay ) ;
			}

			return playId ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="rPlayId">発音毎に割り当てられるユニークな識別子を格納する要素数１以上の配列</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>列挙子</returns>
		public static Request PlayAsync( string path, Action<int> onLoaded = null, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, float delay = 0.0f )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( PlayAsync_Private( path, onLoaded, fade, volume, pan, loop, delay, request ) ) ;
			return request ;
		}

		private static IEnumerator PlayAsync_Private( string path, Action<int> onLoaded, float fade, float volume, float pan, bool loop, float delay, Request request )
		{
			path = CorrectPath( path ) ;

			int playId ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら高速再生
				playId = Play( path, fade, volume, pan, loop, delay ) ;
				request.PlayId = playId ;
				request.IsDone = true ;
				onLoaded?.Invoke( playId ) ;
				yield break ;
			}

			AudioClip audioClip = null ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			yield return Asset.LoadAsync<AudioClip>( m_Path + "/" + path, ( _ ) => { audioClip = _ ; }, Asset.CachingType.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				request.Error = "Could not load" ;
				yield break ;
			}

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume * GetVolume(), pan, 0, delay ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				request.Error = "Could not play" ;
				yield break ;
			}

			// 成功
			request.PlayId = playId ;
			request.IsDone = true ;
			onLoaded?.Invoke( playId ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＢＧＭを停止する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int playId, float fade = 0 )
		{
			if( fade <= 0 )
			{
				// フェードなし停止
				return AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				return AudioManager.StopFade( playId, fade ) ;
			}
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="tFade"></param>
		/// <returns></returns>
		public static Request StopAsync( int playId, float fade = 0 )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return null ;	// 識別子が不正か元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( playId ) ;

				Request request = new Request()
				{
					PlayId = playId,
					IsDone = true
				} ;
				return request ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( playId, fade ) ;
				
				Request request = new Request() ;
				ApplicationManager.Instance.StartCoroutine( StopAsync_Private( playId, request ) ) ;
				return request ;
			}
		}

		private static IEnumerator StopAsync_Private( int playId, Request request )
		{
			yield return new WaitWhile( () => AudioManager.IsPlaying( playId ) == true ) ;

			request.PlayId = playId ;
			request.IsDone = true ;
		}

		/// <summary>
		/// 識別子で指定するＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;  // 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// コンフィグのボリューム値を取得する
		private static float GetVolume()
		{
//			return Player.bgmVolume ;
			return 1 ;
		}
	}
}
