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
	///  Ｖｏｉｃｅクラス Version 2019/09/13 0
	/// </summary>
	public class Voice
	{
		// Ｖｏｉｃｅ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)

		/// <summary>
		/// Attack
		/// </summary>
		public const string Attack		= "Attack" ;

		/// <summary>
		/// Finisher
		/// </summary>
		public const string Finisher	= "Finisher" ;

		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path = "Sounds/Voice" ;

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
					if( IsDone == true || string.IsNullOrEmpty( Error ) == true )
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
			public string Error = string.Empty ;

			/// <summary>
			/// 再生識別子
			/// </summary>
			public int PlayId = -1 ;
		}

		//-----------------------------------------------------------
		
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
		/// Ｖｏｉｃｅを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子(-1で失敗)</returns>
		public static int Play( string path, float volume = 1.0f, float pan = 0, bool loop = false, float delay = 0.0f )
		{
			path = CorrectPath( path ) ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingType.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}

			// 再生する
			return AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay, "Voice" ) ;
		}
		
		/// <summary>
		/// Ｖｏｉｃｅを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tLoop">ループ(true=する・false=しない)</param>
		/// <param name="tDelay">再生開始遅延時間(秒)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>列挙子</returns>
		public static Request PlayAsync( string path, Action<int> onLoaded = null, float volume = 1.0f, float pan = 0, bool loop = false, float delay = 0.0f )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( PlayAsync_Private( path, onLoaded, volume, pan, loop, delay, request ) ) ;
			return request ;
		}

		private static IEnumerator PlayAsync_Private( string path, Action<int> onLoaded, float volume, float pan, bool loop, float delay, Request request )
		{
			path = CorrectPath( path ) ;

			int playId ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら高速再生
				playId = Play( path, volume, pan, loop, delay ) ;
				request.PlayId = playId ;
				request.IsDone = true ;
				onLoaded?.Invoke( playId ) ;
				yield break ;
			}

			AudioClip audioClip = null ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			yield return Asset.LoadAsync<AudioClip>( m_Path + "/" + path, ( _ ) => { audioClip = _ ; }, Asset.CachingType.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				request.Error = "Could not load." ;
				yield break ;
			}

			// 再生する
			playId = AudioManager.Play( audioClip, loop, volume * GetVolume(), pan, 0, delay, "Voice" ) ;
			if( playId <  0 )
			{
				// 失敗
				request.Error = "Could not play." ;
				yield break ;
			}

			// 成功
			request.PlayId = playId ;
			request.IsDone = true ;
			onLoaded?.Invoke( playId ) ;
		}

		//-------------------------------------------------------------------------------------------
		
		/// <summary>
		/// ３Ｄ空間想定でＶｏｉｃｅを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tPosition">音源の位置(ワールド座標系)</param>
		/// <param name="tListener">リスナーのトランスフォーム</param>
		/// <param name="tScale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		public static bool Play3D( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			path = CorrectPath( path ) ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( m_Path + "/" + path, Asset.CachingType.Same ) ;
			if( audioClip == null )
			{
				return false ;
			}

			// 再生する
			return AudioManager.Play3D( audioClip, position, listener, scale, volume * GetVolume() ) ;
		}

		/// <summary>
		/// ３Ｄ空間想定でＶｏｉｃｅを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="tName">ファイル名</param>
		/// <param name="tPosition">音源の位置(ワールド座標系)</param>
		/// <param name="tListener">リスナーのトランスフォーム</param>
		/// <param name="tScale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="tVolume">ボリューム係数(0～1)</param>
		/// <param name="tCaching">キャッシュにためるかどうか(true=ためる・false=ためない)</param>
		/// <returns>列挙子</returns>
		public static Request Play3DAsync( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			Request request = new Request() ;
			ApplicationManager.Instance.StartCoroutine( Play3DAsync_Private( path, position, listener, scale, volume, request ) ) ;
			return request ;
		}

		private static IEnumerator Play3DAsync_Private( string path, Vector3 position, Transform listener, float scale, float volume, Request request )
		{
			path = CorrectPath( path ) ;

			if( Asset.Exists( m_Path + "/" + path ) == true )
			{
				// 既にあるなら高速再生
				Play3D( path, position, listener, scale, volume ) ;
				request.IsDone = true ;
				yield break ;
			}

			AudioClip audioClip = null ;

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			yield return Asset.LoadAsync<AudioClip>( m_Path + "/" + path, ( _ ) => { audioClip = _ ; }, Asset.CachingType.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				request.Error = "Could not load." ;
				yield break ;
			}

			// 再生する
			if( AudioManager.Play3D( audioClip, position, listener, scale, volume * GetVolume() ) == false )
			{
				// 失敗
				request.Error = "Could not play." ;
				yield break ;
			}

			// 成功
			request.IsDone = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｖｏｉｃｅを停止する
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
		/// Ｖｏｉｃｅが停止するまで待つ
		/// </summary>
		/// <param name="tFade"></param>
		/// <returns></returns>
		public static Request StopAsync( int playId, float fade = 0 )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return null ;	// 不正な識別子か元々鳴っていない
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
		/// 識別子で指定するＶｏｉｃｅが再生中か確認する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsRunning( int playId )
		{
			if( playId <  0 || AudioManager.IsRunning( playId ) == false )
			{
				return false ;
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 識別子で指定するＶｏｉｃｅが再生中か確認する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return false ;
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// Ｖｏｉｃｅを一時停止する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			return AudioManager.Pause( playId ) ;
		}

		/// <summary>
		/// Ｖｏｉｃｅを一時停止する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			return AudioManager.Unpause( playId ) ;
		}

		/// <summary>
		/// 識別子で指定するＶｏｉｃｅが再生中か確認する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( playId <  0 || AudioManager.IsPausing( playId )== false )
			{
				return false ;
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 全Ｖｏｉｃｅを完全停止する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return AudioManager.StopAll( "Voice" ) ;
		}

		/// <summary>
		/// 全Ｖｏｉｃｅを一時停止する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return AudioManager.PauseAll( "Voice" ) ;
		}

		/// <summary>
		/// 全Ｖｏｉｃｅを一時再開する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tFade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return AudioManager.UnpauseAll( "Voice" ) ;
		}

		//-------------------------------------------------------------------------------------------

		// コンフィグのボリューム値を取得する
		private static float GetVolume()
		{
//			return Player.voiceVolume ;
			return 1 ;
		}
	}
}
