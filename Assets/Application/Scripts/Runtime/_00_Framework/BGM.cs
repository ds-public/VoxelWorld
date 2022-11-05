#define USE_UNITY_AUDIO

#if USE_UNITY_AUDIO
using System ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

// 要 AudioHelper パッケージ
using AudioHelper ;

namespace DSW
{
	/// <summary>
	/// ＢＧＭクラス Version 2022/10/01 0
	/// </summary>
	public class BGM : ExMonoBehaviour
	{
		private static BGM	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		// ＢＧＭ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)

		public const string	m_InternalPath = "Internal|Sounds/BGM/" ;

		public const string None			= null ;

		public const string Title			= m_InternalPath + "001_Title" ;
		public const string Lobby			= "008_Lobby" ;
		public const string World			= "010_World" ;

		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path			= "Sounds/BGM/" ;

		//-----------------------------------------------------------

		private const string TagName		= "BGM" ;

		//-----------------------------------------------------------

		// メインＢＧＭのオーディオチャンネルインスタンスを保持する
		private static string	m_MainBGM_RequestPath		= string.Empty ;
		private static string	m_MainBGM_Path				= string.Empty ;
		private static float	m_MainBGM_Volume			=  1 ;
		private static float	m_MainBGM_Pan				=  0 ;

		private static int		m_MainBGM_PlayId			= -1 ;

		private static string	m_MainBGM_Reserved_Path		=  string.Empty ;
		private static float	m_MainBGM_Reserved_Volume	= 1 ;
		private static float	m_MainBGM_Reserved_Pan		= 0 ;

		//-----------------------------------------------------------

		/// <summary>
		/// ワーク変数初期化
		/// </summary>
		public static void Initialize()
		{
			m_MainBGM_RequestPath		= string.Empty ;
			m_MainBGM_Path				= string.Empty ;

			m_MainBGM_Volume			=  1 ;
			m_MainBGM_Pan				=  0 ;

			m_MainBGM_PlayId			= -1 ;

			m_MainBGM_Reserved_Path		=  string.Empty ;
			m_MainBGM_Reserved_Volume	= 1 ;
			m_MainBGM_Reserved_Pan		= 0 ;
		}

		/// <summary>
		/// 現在再生中のＢＧＭのパスを取得する
		/// </summary>
		/// <returns></returns>
		public static string GetCurrent()
		{
			return m_MainBGM_Path ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// タイトル前から必要なアセットをロードする
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadInternalAsync()
		{
			// アセットバンドルの強制ダウンロードを行う(失敗してもダイアログは出さない)
			if( Asset.Exists( BGM.Title ) == false )
			{
				await Asset.DownloadAssetBundleAsync( BGM.Title, true ) ;
			}
		}

		// パスの保険
		private static string CorrectPath( string path )
		{
			// ＢＧＭはファイル単位でアセットバンドル化するため以下まような保険は不要
//			// 保険をかける
//			if( path.Contains( "//" ) == false )
//			{
//				// アセットバンドルのパス指定が無い
//				int p = path.LastIndexOf( '/' ) ;
//				if( p >= 0 )
//				{
//					path = path.Substring( 0, p ) + "//" + path.Substring( p + 1, path.Length - ( p + 1 ) ) ;
//				}
//			}

			if( path.IndexOf( m_InternalPath ) <  0 )
			{
				path = m_Path + path ;
			}

			return path ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMain( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false )
		{
			string originPath = path ;

			if( m_MainBGM_RequestPath == originPath )
			{
				return true ;	// 既に再生リクエスト中
			}

			m_MainBGM_RequestPath = originPath ;

			//----------------------------------

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( originPath == m_MainBGM_Path )
				{
					m_MainBGM_RequestPath = string.Empty ;
					return true ;
				}
			}

			//----------------------------------

			path = CorrectPath( path ) ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = Asset.Load<AudioClip>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				return false ;
			}

			//----------------------------------------------------------

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				m_MainBGM_RequestPath = string.Empty ;
				return false ;
			}

			m_MainBGM_Path		= originPath ;
			m_MainBGM_Volume	= volume ;
			m_MainBGM_Pan		= pan ;

			m_MainBGM_PlayId	= playId ;

			m_MainBGM_RequestPath = string.Empty ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayMainAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true, bool restart = false )
		{
			string originPath = path ;

			if( m_MainBGM_RequestPath == originPath )
			{
				return -1 ;	// 既に再生リクエスト中
			}

			m_MainBGM_RequestPath = originPath ;

			//----------------------------------

			if( restart == false )
			{
				// 既に同じ曲が鳴っていたらスルーする
				if( originPath == m_MainBGM_Path )
				{
					m_MainBGM_RequestPath = string.Empty ;
					return m_MainBGM_PlayId ;
				}
			}

			//----------------------------------

			path = CorrectPath( path ) ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			//----------------------------------------------------------

			// ＢＧＭを再生する
			int playId ;
			
			if( fade <= 0 )
			{
				// フェードなし再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.Stop( m_MainBGM_PlayId ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
			}
			else
			{
				// フェードあり再生
				if( m_MainBGM_PlayId >= 0 && AudioManager.IsPlaying( m_MainBGM_PlayId ) == true )
				{
					AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
					m_MainBGM_Path		= string.Empty ;
					m_MainBGM_PlayId	= -1 ;
				}

				// 再生する
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				m_MainBGM_RequestPath = string.Empty ;
				return -1 ;
			}

			m_MainBGM_Path		= originPath ;
			m_MainBGM_Volume	= volume ;
			m_MainBGM_Pan		= pan ;

			m_MainBGM_PlayId	= playId ;

			m_MainBGM_RequestPath = string.Empty ;
			
			// 成功
			return m_MainBGM_PlayId ;
		}

		/// <summary>
		/// メインＢＧＭを退避する
		/// </summary>
		/// <returns></returns>
		public static bool ReserveMainBGM()
		{
			if( m_MainBGM_PlayId <  0 )
			{
				Debug.Log( "<color=#FFFF00>退避するBGMは無い</color>" ) ;

				m_MainBGM_Reserved_Path		= string.Empty ;
				m_MainBGM_Reserved_Volume	= 1 ;
				m_MainBGM_Reserved_Pan		= 0 ;

				return false ;
			}

			m_MainBGM_Reserved_Path		= m_MainBGM_Path ;
			m_MainBGM_Reserved_Volume	= m_MainBGM_Volume ;
			m_MainBGM_Reserved_Pan		= m_MainBGM_Pan ;

			return true ;
		}

		/// <summary>
		/// 退避中のメインＢＧＭを復帰させる
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> RestoreMainBGM( float fadeTime = 0 )
		{
			if( string.IsNullOrEmpty( m_MainBGM_Reserved_Path ) == true )
			{
				Debug.Log( "<color=#FFFF00>復帰するBGMは無い</color>" ) ;
				return false ;
			}

			int playId = await PlayMainAsync( m_MainBGM_Reserved_Path, fadeTime, m_MainBGM_Reserved_Volume, m_MainBGM_Reserved_Pan, true ) ;

			ClearReservedMainBGM() ;

			return ( playId != -1 ) ;
		}

		/// <summary>
		/// 退避中のメインＢＧＭ情報を消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearReservedMainBGM()
		{
			if( string.IsNullOrEmpty( m_MainBGM_Reserved_Path ) == true )
			{
				return false ;
			}

			m_MainBGM_Reserved_Path		= null ;
			m_MainBGM_Reserved_Volume	= 1 ;
			m_MainBGM_Reserved_Pan		= 0 ;

			return true ;
		}

		/// <summary>
		/// 現在再生中のメインＢＧＭと退避中のメインＢＧＭが同じか判定する
		/// </summary>
		/// <returns></returns>
		public static bool IsSameReservedMainBGM()
		{
			return m_MainBGM_Path == m_MainBGM_Reserved_Path ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// メインＢＧＭを停止する
		/// </summary>
		/// <param name="fade">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopMain( float fade = 0 )
		{
			m_MainBGM_RequestPath = string.Empty ;

			if( m_MainBGM_PlayId <  0 )
			{
				return false ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
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

			m_MainBGM_Path		= string.Empty ;
			m_MainBGM_PlayId	= -1 ;

			return true ;
		}

		/// <summary>
		/// ＢＧＭが停止するまで待つ
		/// </summary>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async UniTask<int> StopMainAsync( float fade = 0 )
		{
			m_MainBGM_RequestPath = string.Empty ;

			if( m_MainBGM_PlayId <  0 )
			{
				return -1 ;	// 元々鳴っていない
			}

			if( AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;
				return -1 ;	// 元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( m_MainBGM_PlayId ) ;

				int mainBGM_PlayId	= m_MainBGM_PlayId ;
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;

				return mainBGM_PlayId ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( m_MainBGM_PlayId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( m_MainBGM_PlayId ) == true ) ;

				int mainBGM_PlayId	= m_MainBGM_PlayId ;
				m_MainBGM_Path		= string.Empty ;
				m_MainBGM_PlayId	= -1 ;

				return mainBGM_PlayId ;
			}
		}

		/// <summary>
		/// メインＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="audioClipName ">曲の名前(再生中の曲の種類を限定したい場合は指定する)</param>
		/// <returns>再生状況(true=再生中・false=停止中)</returns>
		public static bool IsPlayingMain( string path = null )
		{
			// 何かの曲は再生中か
			if( m_MainBGM_PlayId <  0 || AudioManager.IsPlaying( m_MainBGM_PlayId ) == false )
			{
				// 何も再生されていない
				return false ;
			}

			//----------------------------------------------------------
			
			// 何かしらの曲は鳴っている
			if( string.IsNullOrEmpty( path ) == false )
			{
				// 曲の名前の指定がある

				if( m_MainBGM_Path != path )
				{
					// 指定した曲は鳴っていない
					return false ;
				}
			}

			return true ;
		}

#if false
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
#endif
		//-----------------------------------------------------------

		/// <summary>
		/// ＢＧＭを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子</returns>
		public static int Play( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = Asset.Load<AudioClip>( path, Asset.CachingTypes.None ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			//----------------------------------

			int playId ;

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			return playId ;
		}

		/// <summary>
		/// ＢＧＭを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="fade">フェード時間(秒)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayAsync( string path, float fade = 0, float volume = 1.0f, float pan = 0, bool loop = true )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			int playId ;

			// 再生中以外のものは破棄されて構わないので一切キャッシュには積まない
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			if( fade <= 0 )
			{
				// フェードなし再生
				playId = AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
			}
			else
			{
				// フェードあり再生
				playId = AudioManager.PlayFade( audioClip, fade, loop, volume, pan, 0, TagName ) ;
			}

			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＢＧＭを停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade">フェード時間(秒)</param>
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
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="fade"></param>
		/// <returns></returns>
		public static async UniTask<bool> StopAsync( int playId, float fade = 0 )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId ) == false )
			{
				return false ;	// 識別子が不正か元々鳴っていない
			}

			if( fade <= 0 )
			{
				// フェードなし停止
				AudioManager.Stop( playId ) ;
			}
			else
			{
				// フェードあり停止
				AudioManager.StopFade( playId, fade ) ;
				
				await m_Instance.WaitWhile( () => AudioManager.IsPlaying( playId ) == true ) ;
			}

			return true ;
		}

		/// <summary>
		/// 識別子で指定するＢＧＭが再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
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
		/// ベースボリュームを設定する
		/// </summary>
		/// <param name="baseVolume"></param>
		public static void SetBaseVolume( float baseVolume )
		{
			AudioManager.SetBaseVolume( TagName, baseVolume ) ;
		}
	}
}
#endif
