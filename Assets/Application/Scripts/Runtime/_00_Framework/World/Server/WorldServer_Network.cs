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
using uGUIHelper ;
using TransformHelper ;

using DBS.WorldServerClasses ;

using DBS.World.Packet ;

namespace DBS.World
{
	/// <summary>
	/// サーバー(ウェブソケット)
	/// </summary>
	public partial class WorldServer
	{
		// WebSocketServer のインスタンス
		private WebSocketServer m_WebSocketServer ;

		// クライアントの制御用インスタンス群を保持する
		private readonly Dictionary<string,ActiveClient> m_ActiveClients = new Dictionary<string,ActiveClient>() ;

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
		private ResultCodes CreateWebSocketServer()
		{
			// WebSocket 準備
			int serverPortNumber	= PlayerData.ServerPortNumber ;

			// ポート番号の使用状況を確認する
			if( ExWebSocket.IsTcpPortNumberUsing( serverPortNumber ) == true )
			{
				// 指定したポート番号は既に使用されている
				return ResultCodes.PortNumberAlreadyInUse ;
			}

			m_WebSocketServer = new WebSocketServer( serverPortNumber ) ;

			Debug.Log( "<color=#00FFFF>[SERVER] PortNumber = " + serverPortNumber + " でサーバーを起動します</color>" ) ;

			//----------------------------------------------------------
			// WebSocketServer 準備

			// メインスレッドのコンテキストを取得する
			m_MainThreadContext = SynchronizationContext.Current ;

			m_WebSocketServer.AddWebSocketService<ActiveClient>( "/", ( ActiveClient client ) =>
			{
				if( client != null )
				{
					// メインスレッドで呼び出してくれるイベントコールバック群を登録する
					client.SetEventCallbacks
					(
						m_MainThreadContext,

						// メインスレッド
						OnConnected_Main,
						OnReceivedData_Main,
						OnReceivedText_Main,
						OnDisconnected_Main,

						// サブスレッド
						OnConnected,
						OnReceivedData,
						OnReceivedText,
						OnDisconnected
					) ;

					// このタイミングではまだ client.ID は不明である事に注意する
				}
			} ) ;

			// サーバー開始
			m_WebSocketServer.Start() ;

			Debug.Log( "<color=#00FFFF>[SERVER ] WebSocketServer 開始</color>" ) ;

			return ResultCodes.Successful ;
		}

		// サーバーの処理を終了する
		private void DeleteWebSocketServer()
		{
			// WebSocketServer をシャットダウンする
			if( m_WebSocketServer != null )
			{
				m_WebSocketServer.Stop() ;
				m_WebSocketServer  = null ;
			}
		}

		//-----------------------------------------------------------
		// メインスレッド用
#region WebSocket_Callback_On_MainThread
		private void OnConnected_Main( ActiveClient client )
		{
			// 接続(string client.ID)
			Debug.Log( "<color=#00FFFF>[SERVER] クライアント(CID:" + client.ID + ")が接続しました</color>" ) ;

			// 接続はメインスレッドで処理する
			m_ActiveClients.Add( client.ID, client ) ;
		}

		private void OnReceivedData_Main( ActiveClient client, byte[] data )
		{
			// バイナリ受信
		}

		private void OnReceivedText_Main( ActiveClient client, string text )
		{
			// テキスト受信
		}

		private void OnDisconnected_Main( ActiveClient client )
		{
			// 接続(string client.ID)
			Debug.Log( "<color=#00FFFF>[SERVER] クライアント(CID:" + client.ID + ")が切断しました</color>" ) ;

			// 切断はメインスレッドで処理する
			WS_OnDisconnected( client ) ;
		}
#endregion
		//---------------
		// サブスレッド用
#region WebSocket_Callback_On_SubThread
		private bool m_IsReceiving = false ;

		// 接続された際に呼び出される
		private void OnConnected( ActiveClient client )
		{
			// 接続(string client.ID)
		}


		// 受信した際に呼び出される(バイナリ)
		private void OnReceivedData( ActiveClient client, byte[] data )
		{
			// バイナリ受信
			m_IsReceiving = true ;
			WS_ProcessReceive( client, data ) ;
			m_IsReceiving = false ;
		}

		// 受信した際に呼び出される(テキスト)
		private void OnReceivedText( ActiveClient client, string text )
		{
			// テキスト受信
		}

		// 切断した際に呼び出される
		private void OnDisconnected( ActiveClient client )
		{
			// 切断(string client.ID)
		}
#endregion
		//-------------------------------------------------------------------------------------------

		// 受信処理
		private void WS_ProcessReceive( ActiveClient client, byte[] data )
		{
			var request = DataPacker.Deserialize<ClientRequest>( data, false, Settings.DataTypes.MessagePack ) ;
			if( request == null || request.Signature != "VWPD" )
			{
				// 異常発生
				Debug.LogWarning( "[SERVER] 通信パケット異常:シグネチャが認識できません:" + data.Length ) ;
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
