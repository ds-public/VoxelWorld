using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;

using MathHelper ;

using DBS.WorldServerClasses ;

/// <summary>
/// パッケージ
/// </summary>
namespace DBS.World
{
	/// <summary>
	/// ワールドを管理するクラス Version 2022/10/03 0
	/// </summary>
	public partial class WorldServer : ExMonoBehaviour
	{
		private static WorldServer	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}

		/// <summary>
		/// インスタンス
		/// </summary>
		public static WorldServer	  Instance => m_Instance ;

		//-----------------------------------

		private const string m_WorldRootPath = "Server/Worlds/01/" ;

		//-----------------------------------------------------------
		// Player

		// ルートパス
		private const string m_PlayerRootPath = m_WorldRootPath + "Players/" ;

		// ロード中のプレイヤー情報
		private readonly Dictionary<string,WorldPlayerData>		m_Players = new Dictionary<string, WorldPlayerData>() ;

		//-----------------------------------------------------------
		// Chunk

		// 展開中のチャンクセット群
		private readonly Dictionary<int,ServerChunkSetData>	m_ActiveChunkSets = new Dictionary<int,ServerChunkSetData>() ;

		//-----------------------------------------------------------

		// サーバーが稼働中かどうか
		private bool	m_IsRunning ;

		// サーバーの準備が整っているかどうか
		private bool	m_IsReady ;

		//-----------------------------------------------------------

		/// <summary>
		/// 起動する
		/// </summary>
		/// <returns></returns>
		public static UniTask<WorldServer.ResultCodes> Play()
		{
			if( m_Instance == null )
			{
				return default ;
			}

			return m_Instance.Play_Private() ;
		}

		// 起動する
		private async UniTask<WorldServer.ResultCodes> Play_Private()
		{
			m_IsRunning	= true ;
			m_IsReady	= false ;

			//----------------------------------------------------------

			// サーバー処理を開始する
			var resultCode = CreateWebSocketServer() ;
			if( resultCode != ResultCodes.Successful )
			{
				return resultCode ;
			}

			//------------------------------------------------------------------------------------------

			// クライアント情報を消去する
			m_ActiveClients.Clear() ;

			// チャンクセットアロケーションテーブルをロードする
			LoadChunkSetAllocations() ;

			// パーリンノイズのシード値を設定する
			PerlinNoise.Initialize( 13212 ) ;

			//--------------

			m_IsReady = true ;

			//----------------------------------------------------------

			await Yield() ;

			// 成功
			return WorldServer.ResultCodes.Successful ;
		}

		/// <summary>
		/// 停止する
		/// </summary>
		/// <returns></returns>
		public static bool Stop()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Stop_Private() ;
		}

		// 停止する
		private bool Stop_Private()
		{
			if( m_IsRunning == false )
			{
				// サーバーは動作していない
				return true ;
			}

			//----------------------------------------------------------

			bool isServer = ( m_WebSocketServer != null ) ;

			Debug.Log( "<color=#00FFFF>[SERVER] シャットダウンを行います(サーバーの起動状態 = " + isServer + ")</color>" ) ;

			//----------------------------------------------------------
#if UNITY_EDITOR
			if( m_IsReceiving == true )
			{
				Debug.Log( "受信処理状況(前):" + m_IsReceiving ) ;
			}
#endif           
			// サーバー処理を終了する
			DeleteWebSocketServer() ;
#if UNITY_EDITOR
			if( m_IsReceiving == true )
			{
				Debug.Log( "受信処理状況(後):" + m_IsReceiving ) ;
			}
#endif
			//------------------------------------------------------------------------------------------

			if( m_IsReady == false )
			{
				// 何も処理が行われていない
				m_IsRunning = false ;
				return false ;
			}

			//----------------------------------

			Debug.Log( "<color=#00FFFF>[SERVER] 各種情報を保存します</color>" ) ;

			// 全てのチャンクセットを解放(保存)する
			FreeAllChunkSets() ;

			// チャンクセットアロケーションを保存する
			SaveChunkSetAllocations( withClenup:true ) ;

			// プレイヤー情報をセーブする
			SaveAllPlayers( withCleanup:true ) ;

			// クライアント情報を消去する
			m_ActiveClients.Clear() ;

			//----------------------------------

			m_IsReady	= false ;
			m_IsRunning	= false ;

			Debug.Log( "<color=#00FFFF>[SERVER] サーバーのシャットダウンが完了しました</color>" ) ;

			// 完了
			return true ;
		}

		// 破棄される際に呼び出される
		internal void OnDestroy()
		{
			Stop_Private() ;

			// 自身のインスタンスを初期化する
			m_Instance = null ;

		}
	}
}
