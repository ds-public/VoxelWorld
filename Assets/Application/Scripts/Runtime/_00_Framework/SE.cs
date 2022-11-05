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
	///ＳＥクラス Version 2022/09/23 0
	/// </summary>
	public class SE : ExMonoBehaviour
	{
		private static SE	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------------------------------

		// ＳＥ名称の一覧(文字列からマスターのＩＤ値などに変わる可能性もある)

		public const string	m_InternalPath = "Internal|Sounds/SE/" ;

		public const string None			= null ;

		public const string Start			= m_InternalPath + "System//001_Start" ;
		public const string Click			= m_InternalPath + "System//002_Click" ;
		public const string Decision		= m_InternalPath + "System//002_Click" ;
		public const string Cancel			= m_InternalPath + "System//003_Cancel" ;

		public const string Hit				= m_InternalPath + "System//005_Hit" ;
		public const string Parry			= m_InternalPath + "System//006_Parry" ;
		public const string Move			= m_InternalPath + "System//007_Move" ;
		public const string Broken			= m_InternalPath + "System//008_Broken" ;
		public const string Magic			= m_InternalPath + "System//009_Magic" ;

		public const string Bomb			= m_InternalPath + "System//011_Bomb" ;
		public const string Recover			= m_InternalPath + "System//012_Recover" ;
		public const string Healing			= m_InternalPath + "System//013_Healing" ;

		public const string Tap				= m_InternalPath + "System//024_Tap" ;
		public const string Select			= m_InternalPath + "System//025_Select" ;
		public const string Selection		= m_InternalPath + "System//025_Select" ;

		public const string Encount			= m_InternalPath + "Syetem//030_Encount" ;

		//-----------------------------------------------------------

		// 基本パス(環境に合わせて書き換える事)
		private const string m_Path			= "Sounds/SE/" ;

		//-----------------------------------------------------------

		private const string TagName		= "SE" ;

		//-----------------------------------------------------------

		/// <summary>
		/// タイトル前から必要なアセットをロードする
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadInternalAsync()
		{
			string path = m_InternalPath + "System" ;

			if( Asset.Exists( path ) == false )
			{
				// アセットバンドルの強制ダウンロードを行う(失敗してもダイアログは出さない)
				await Asset.DownloadAssetBundleAsync( path, true ) ;
			}
		}

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

			if( path.IndexOf( m_InternalPath ) <  0 )
			{
				path = m_Path + path ;
			}

			return path ;
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＳＥを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>発音毎に割り当てられるユニークな識別子(-1で失敗)</returns>
		public static int Play( string path, float volume = 1.0f, float pan = 0, bool loop = false )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}

			// 再生する
			return AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
		}

		/// <summary>
		/// ＳＥを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <param name="pan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="loop">ループ(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> PlayAsync( string path, float volume = 1.0f, float pan = 0, bool loop = false )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			int playId ;

			if( Asset.Exists( path ) == true )
			{
				// 既にあるなら同期で高速再生
				playId = Play( path, volume, pan, loop ) ;
				return playId ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			// 再生する
			playId = AudioManager.Play( audioClip, loop, volume, pan, 0, TagName ) ;
			if( playId <  0 )
			{
				// 失敗
				Debug.LogWarning( "Could not play : " + path ) ;
				return -1 ;
			}

			// 成功
			return playId ;
		}

		/// <summary>
		/// ３Ｄ空間想定でＳＥを再生する(同期版:ファイルが存在しない場合は失敗)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		public static int Play3D( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = Asset.Load<AudioClip>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				return -1 ;
			}
			
			// 再生する
			return AudioManager.Play3D( audioClip, position, listener, scale, volume, TagName ) ;
		}
		
		/// <summary>
		/// ３Ｄ空間想定でＳＥを再生する(非同期版:ファイルが存在しない場合はダウンロードを試みる)
		/// </summary>
		/// <param name="path">ファイル名</param>
		/// <param name="position">音源の位置(ワールド座標系)</param>
		/// <param name="listener">リスナーのトランスフォーム</param>
		/// <param name="scale">距離係数(リスナーから音源までの距離にこの係数を掛け合わせたものが最終的な距離になる)</param>
		/// <param name="volume">ボリューム係数(0～1)</param>
		/// <returns>列挙子</returns>
		public static async UniTask<int> Play3DAsync( string path, Vector3 position, Transform listener = null, float scale = 1, float volume = 1.0f )
		{
			path = CorrectPath( path ) ;

			//----------------------------------

			int playId ;

			if( Asset.Exists( path ) == true )
			{
				// 既にあるなら高速再生
				return Play3D( path, position, listener, scale, volume ) ;
			}

			// 複数を１つのアセットバンドルにまとめており同じシーンの中で何度も再生されるケースがあるのでリソース・アセットバンドル両方にキャッシュする
			AudioClip audioClip = await Asset.LoadAsync<AudioClip>( path, Asset.CachingTypes.Same ) ;
			if( audioClip == null )
			{
				// 失敗
				Debug.LogWarning( "Could not load : " + path ) ;
				return -1 ;
			}

			// 再生する
			playId = AudioManager.Play3D( audioClip, position, listener, scale, volume, TagName ) ;
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
		/// ＳＥを停止する
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
		/// ＳＥが停止するまで待つ
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
		/// 識別子で指定するＳＥが再生中か確認する(一時停止中は停止扱いになる)
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int playId )
		{
			if( playId <  0 || AudioManager.IsPlaying( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// ＳＥを一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int playId )
		{
			return AudioManager.Pause( playId ) ;
		}

		/// <summary>
		/// ＳＥを一時停止する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int playId )
		{
			return AudioManager.Unpause( playId ) ;
		}

		/// <summary>
		/// 識別子で指定するＳＥが再生中か確認する
		/// </summary>
		/// <param name="playId">発音毎に割り当てられるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int playId )
		{
			if( playId <  0 || AudioManager.IsPausing( playId )== false )
			{
				return false ;	// 識別子が不正か鳴っていない
			}

			// 再生中
			return true ;
		}

		/// <summary>
		/// 全ＳＥを完全停止する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return AudioManager.StopAll( TagName ) ;
		}

		/// <summary>
		/// 全ＳＥを一時停止する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return AudioManager.PauseAll( TagName ) ;
		}

		/// <summary>
		/// 全ＳＥを一時再開する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return AudioManager.UnpauseAll( TagName ) ;
		}

		//-------------------------------------------------------------------------------------------

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
