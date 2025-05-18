using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

using SocketHelper ;

using DSW.World.Packet ;



namespace DSW.World
{
	/// <summary>
	/// クライアント(ウェブソケット)
	/// </summary>
	public partial class WorldClient
	{
		// SocketClient のインスタンス
		private SocketClient m_SocketClient ;


		/// <summary>
		/// 切断されたかどうか
		/// </summary>
		public  bool		  IsDisconnected	=> m_IsDisconnected ;
		// 切断されたかどうか
		private bool		m_IsDisconnected ;


		/// <summary>
		/// エラーコード
		/// </summary>
		public	int			ErrorCode => m_ErrorCode ;
		private int			m_ErrorCode ;


		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string		ErrorMessage	=> m_ErrorMessage ;
		private string		m_ErrorMessage ;


		//-------------------------------------------------------------------------------------------

		// クライアントの処理を開始する
		private void StartSocketClient()
		{
			// メインスレッドのコンテキストを取得する
			m_MainThreadContext = SynchronizationContext.Current ;

			// チャンク展開を中断するキャンセルトークンを生成する
			m_CancellationSource = new CancellationTokenSource() ;

			//----------------------------------------------------------

			string serverAddress	= PlayerData.ServerAddress ;
			int    serverPort		= PlayerData.ServerPort ;

			Debug.Log( "<color=#00FF00>[CLIENT] Address = " + serverAddress + " Port = " + serverPort + " のサーバーに接続します</color>" ) ;

			// SocketClient トを生成する
			m_SocketClient = new SocketClient
			(
				OnTcpReceived,
				OnTcpDisconnected,
				null,
				512 * 1024,
				m_CancellationSource.Token,
				m_MainThreadContext
			) ;

			//----------------------------------

			// 接続できなかったもしくは切断されたかどうか
			m_IsDisconnected = false ;

			m_ErrorCode		= 0 ;
			m_ErrorMessage	= string.Empty ;

			// 接続
			m_SocketClient.Connect( serverAddress, serverPort, OnTcpConnected ) ;
		}

		// クライアントの処理を終了する
		private void CloseSocketClient()
		{
			// チャンク展開を中断するキャンセルトークンを破棄する
			if( m_CancellationSource != null )
			{
				m_CancellationSource.Cancel() ;

				m_CancellationSource.Dispose() ;
				m_CancellationSource  = null ;
			}

			// 切断する
			if( m_SocketClient != null )
			{
				m_SocketClient.Disconnect( false ) ;
				m_SocketClient.Dispose() ;
				m_SocketClient = null ;
			}

			// メインスレッドのコンテキストを消去する
			m_MainThreadContext = null ;
		}

		//-----------------------------------------------------------
#region WebSocket_Callback_On_MainThread

		// 接続(サーバーアドレスとポート番号が欲しい
		private void OnTcpConnected( bool isSucceeded )
		{
		}

		// 受信
		private void OnTcpReceived( ReadOnlyMemory<byte> data )
		{
			WS_ProcessReceive( data ) ;
		}

		// 切断
		private void OnTcpDisconnected()
		{
			Debug.Log( $"<color=#FF7F00>サーバーから切断された</color>" ) ;

			// 接続できなかった・サーバーから切断された
			m_IsDisconnected = true ;

			if( m_SocketClient != null )
			{
				m_SocketClient.Dispose() ;
				m_SocketClient = null ;
			}
		}

#endregion
		//-------------------------------------------------------------------------------------------

		// 受信処理
		private void WS_ProcessReceive( ReadOnlyMemory<byte> memory )
		{
			var response = DataPacker.Deserialize<ServerResponse>( memory, false, Settings.DataTypes.MessagePack ) ;
			if( response == null || response.Signature != "VWPD" )
			{
				// 異常発生
				Debug.LogWarning( "[SERVER] 通信パケット異常:シグネチャが認識できません:" + memory.Length ) ;
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
