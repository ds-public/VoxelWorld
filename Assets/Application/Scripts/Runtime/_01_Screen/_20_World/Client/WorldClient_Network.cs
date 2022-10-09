using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;


using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

using DBS.World.Packet ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(ウェブソケット)
	/// </summary>
	public partial class WorldClient
	{
		// クライアント用の WebSocket
		private ExWebSocket	m_WebSocket ;

		/// <summary>
		/// 切断されたかどうか
		/// </summary>
		public  bool		  IsDisconnected	=> m_IsDisconnected ;
		// 切断されたかどうか
		private bool		m_IsDisconnected ;

		//-------------------------------------------------------------------------------------------

		// クライアントの処理を開始する
		private void StartWebSocketClient()
		{
			// メインスレッドのコンテキストを取得する
			m_MainThreadContext = SynchronizationContext.Current ;

			// チャンク展開を中断するキャンセルトークンを生成する
			m_CancellationSource = new CancellationTokenSource() ;

			//----------------------------------------------------------

			string serverAddress	= PlayerData.ServerAddress ;
			int    serverPortNumber	= PlayerData.ServerPortNumber ;

			Debug.Log( "<color=#00FF00>[CLIENT] Address = " + serverAddress + " PortNumber = " + serverPortNumber + " のサーバーに接続します</color>" ) ;

			// ソケットを生成する
			m_WebSocket = new ExWebSocket
			(
				m_MainThreadContext,

				// メインスレッド
				OnConnected_Main,
				OnReceivedData_Main,
				OnReceivedText_Main,
				OnDisconnected_Main,
				OnError_Main

				// サブスレッド
			) ;

			//----------------------------------

			// 接続できなかったもしくは切断されたかどうか
			m_IsDisconnected = false ;

			// 非同期で接続を試みる
			m_WebSocket.Connect( serverAddress, serverPortNumber, false ) ;
		}

		// クライアントの処理を終了する
		private void EndWebSocketClient()
		{
			// 切断する
			if( m_WebSocket != null )
			{
				if( m_WebSocket.IsConnecting == true )
				{
					m_WebSocket.Disconnect() ;
				}

				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}

			//----------------------------------

			// チャンク展開を中断するキャンセルトークンを破棄する
			if( m_CancellationSource != null )
			{
				m_CancellationSource.Cancel() ;

				m_CancellationSource.Dispose() ;
				m_CancellationSource  = null ;
			}

			// メインスレッドのコンテキストを消去する
			m_MainThreadContext = null ;
		}

		//-----------------------------------------------------------
#region WebSocket_Callback_On_MainThread
		// 接続(サーバーアドレスとポート番号が欲しい
		private void OnConnected_Main( string serverAddress, int serverPortNumber )
		{
		}

		// 受信
		private void OnReceivedData_Main( byte[] data )
		{
			WS_ProcessReceive( data ) ;
		}

		// 受信
		private void OnReceivedText_Main( string text )
		{
		}

		// 切断
		private void OnDisconnected_Main( int code, string reason )
		{
			// 接続できなかった・サーバーから切断された
			m_IsDisconnected = true ;

			if( m_WebSocket != null )
			{
				if( code == 1006 )
				{
					// 接続できなかった
				}
				else
				{
					// サーバーから切断された
				}

				//---------------------------------

				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		// 異常
		private void OnError_Main( string message )
		{
			Debug.LogWarning( "[CLIENT] Error : " + message ) ;
			AddLog( "通信エラー:" + message ) ;
		}
#endregion
		//-------------------------------------------------------------------------------------------

		// 受信処理
		private void WS_ProcessReceive( byte[] data )
		{
			var response = DataPacker.Deserialize<ServerResponse>( data, false, Settings.DataTypes.MessagePack ) ;
			if( response == null || response.Signature != "VWPD" )
			{
				// 異常発生
				Debug.LogWarning( "[SERVER] 通信パケット異常:シグネチャが認識できません:" + data.Length ) ;
				return ;
			}

			//----------------------------------------------------------

			// エラーチェック
			
			//----------------------------------------------------------

			// コマンド種別
			var commandType = response.CommandType ;

			if( commandType == CommandTypes.Login )
			{
				// Login(レスポンス)
				WS_OnReceived_Response_Login( response.Data ) ;
			}
			if( commandType == CommandTypes.Login_Other )
			{
				// Join(レスポンス:他のプレイヤー限定)
				WS_OnReceived_Response_Login_Other( response.Data ) ;
			}
			else
			if( commandType == CommandTypes.SetPlayerTransform_Other )
			{
				// SetTransform(レスポンス:他のプレイヤー限定)
				WS_OnReceived_Response_SetPlayerTransform_Other( response.Data ) ;
			}
			else
			if( commandType == CommandTypes.LoadWorldChunkSet )
			{
				// LoadChunkSet(レスポンス)
				WS_OnReceived_Response_LoadWorldChunkSet( response.Data ) ;
			}
			else
			if( commandType == CommandTypes.FreeWorldChunkSet )
			{
				// FreeChunkSet(クライアントにこの通知が来る事は無い
			}
			else
			if( commandType == CommandTypes.SetWorldBlock_Other )
			{
				// SetBlock(他のプレイヤーによる操作による通知
				WS_OnReceived_Response_SetWorldBlock_Other( response.Data ) ;
			}
			else
			if( commandType == CommandTypes.Logout_Other )
			{
				// Logout_Other
				WS_OnReceived_Response_Logout_Other( response.Data ) ;
			}
		}
	}
}
