using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;


using Cysharp.Threading.Tasks ;

using SocketHelper ;


using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

using DSW.WorldServerClasses ;

using DSW.World.Packet ;


namespace DSW.World
{
	/// <summary>
	/// サーバー(ウェブソケット)
	/// </summary>
	public partial class WorldServer
	{
		// SocketServer のインスタンス
		private SocketServer	m_SocketServer ;

		// クライアントの制御用インスタンス群を保持する
		private readonly Dictionary<ClientHandler,ActiveClient> m_ActiveClients = new () ;

		// メインスレッドのコンテキスト
		private SynchronizationContext	m_MainThreadContext ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// サーバー使用処理時の結果
		/// </summary>
		public enum ResultCodes
		{
			Successful,
			PortNumberAlreadyInUse,
		}

		// サーバーの処理を開始する
		private ResultCodes CreateSocketServer( CancellationToken cancellationToken )
		{
			// WebSocket 準備
			int serverPort	= PlayerData.ServerPort ;

			// ポート番号の使用状況を確認する(後で修正) ※SocketHelper に取り込む
			if( SocketServer.IsTcpPortNumberUsing( serverPort ) == true )
			{
				// 指定したポート番号は既に使用されている
				return ResultCodes.PortNumberAlreadyInUse ;
			}

			Debug.Log( "<color=#00FFFF>[SERVER] Port = " + serverPort + " でサーバーを起動します</color>" ) ;

			//----------------------------------------------------------
			// WebSocketServer 準備

			// メインスレッドのコンテキストを取得する
			m_MainThreadContext = SynchronizationContext.Current ;

			m_SocketServer = new SocketServer
			(
				// 以下のコールバックを設定せよ
				OnTcpAccepted,
				OnTcpReceived,
				OnTcpDisconnected,
				null,
				cancellationToken
			) ;

			// サーバー開始
			m_SocketServer.Start( null, serverPort, 0 ) ;

			Debug.Log( "<color=#00FFFF>[SERVER] SocketServer 開始</color>" ) ;

			return ResultCodes.Successful ;
		}

		// サーバーの処理を終了する
		private void DeleteWebSocketServer()
		{
			// SocketServer をシャットダウンする
			if( m_SocketServer != null )
			{
				m_SocketServer.Stop() ;
				m_SocketServer.Dispose() ;
				m_SocketServer = null ;
			}
		}

		//-----------------------------------------------------------

		private bool m_IsReceiving = false ;

		// 接続があった
		private void OnTcpAccepted( ClientHandler client )
		{
			// 接続(string client.ID)
			Debug.Log( "<color=#00FFFF>[SERVER] クライアント(" + client.EndPoint + ")が接続しました</color>" ) ;

			// 接続はメインスレッドで処理する

			m_ActiveClients.Add( client, new ActiveClient(){ Client = client } ) ;
		}

		// 受信があった
		private void OnTcpReceived( ClientHandler client, ReadOnlyMemory<byte> data )
		{
			// バイナリ受信
			m_IsReceiving = true ;
			WS_ProcessReceive( m_ActiveClients[ client ], data ) ;
			m_IsReceiving = false ;
		}

		// 切断があった
		private void OnTcpDisconnected( ClientHandler client )
		{
			// 切断はメインスレッドで処理する
			WS_OnDisconnected( m_ActiveClients[ client ] ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 受信処理
		private void WS_ProcessReceive( ActiveClient client, ReadOnlyMemory<byte> memory )
		{
			var request = DataPacker.Deserialize<ClientRequest>( memory, false, Settings.DataTypes.MessagePack ) ;
			if( request == null || request.Signature != "VWPD" )
			{
				// 異常発生
				Debug.LogWarning( "[SERVER] 通信パケット異常:シグネチャが認識できません:" + memory.Length ) ;
				return ;
			}

			//----------------------------------------------------------

			// コマンド種別
			var commandType = request.CommandType ;

			// コマンドごとの処理の分岐
			if( commandType == CommandTypes.Login )
			{
				// ログインの要求を受けた
				WS_OnReceived_Request_Login( client, request.Data ) ;
			}
			else
			if( commandType == CommandTypes.SetPlayerTransform )
			{
				// プレイヤーの位置と方向を設定する
				WS_OnReceived_Request_SetPlayerTransform( client, request.Data ) ;
			}
			else
			if( commandType == CommandTypes.LoadWorldChunkSet )
			{
				// チャンクセット展開の要求を受けた
				WS_OnReceived_Request_LoadWorldChunkSet( client, request.Data ) ;
			}
			else
			if( commandType == CommandTypes.FreeWorldChunkSet )
			{
				// チャンクセット解放の要求を受けた
				WS_OnReceived_Request_FreeWorldChunkSet( client, request.Data ) ;
			}
			else
			if( commandType == CommandTypes.SetWorldBlock )
			{
				// ブロック設定の要求を受けた
				WS_OnReveived_Request_SetWorldBlock( client, request.Data ) ;
			}
		}
	}
}
