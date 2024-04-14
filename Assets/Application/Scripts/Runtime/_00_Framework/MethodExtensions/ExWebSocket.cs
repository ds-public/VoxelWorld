using System ;
using System.Collections.Generic ;

using System.Net.NetworkInformation ;
using System.Net ;

using System.Threading ;
using System.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

namespace DSW
{
	/// <summary>
	/// WebSocket のサーバー側のクライアント管理用クラスのラッパー Version 2022/10/03
	/// </summary>
	public class ExWebSocket
	{
		// メインスレッドのコンテキスト
		private SynchronizationContext	m_MainThreadContext ;

		//-----------------------------------
		// メインスレッド用コールドッグ

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action<string,int>		m_OnConnected_Main ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<byte[]>			m_OnReceivedData_Main ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<string>			m_OnReceivedText_Main ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<int,string>		m_OnDisconnected_Main ;

		// 異常があった際にサーバーに伝えるコールバック
		private Action<string>			m_OnError_Main ;

		//-----------------------------------
		// 通信スレッド用コールドッグ

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action<string,int>		m_OnConnected ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<byte[]>			m_OnReceivedData ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<string>			m_OnReceivedText ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<int,string>		m_OnDisconnected ;

		// 異常があった際にサーバーに伝えるコールバック
		private Action<string>			m_OnError ;


		//-----------------------------------------------------------

		private WebSocket				m_WebSocket ;

		private string					m_ServerAddress ;
		private int						m_ServerPortNumber ;

		private bool					m_IsConnecting ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="context"></param>
		/// <param name="onOpen"></param>
		/// <param name="onData"></param>
		/// <param name="onText"></param>
		/// <param name="onClose"></param>
		public ExWebSocket
		(
			SynchronizationContext mainThreadContext	= null,

			Action<string,int> onConnected_Main			= null,
			Action<byte[]> onReceivedData_Main			= null,
			Action<string> onReceivedText_Main			= null,
			Action<int,string> onDisconnected_Main		= null,
			Action<string> onError_Main					= null,

			Action<string,int> onConnected				= null,
			Action<byte[]> onReceivedData				= null,
			Action<string> onReceivedText				= null,
			Action<int,string> onDisconnected			= null,
			Action<string> onError						= null
		)
		{
			m_MainThreadContext		= mainThreadContext ;

			m_OnConnected_Main		= onConnected_Main ;
			m_OnReceivedData_Main	= onReceivedData_Main ;
			m_OnReceivedText_Main	= onReceivedText_Main ;
			m_OnDisconnected_Main	= onDisconnected_Main ;
			m_OnError_Main			= onError_Main ;

			m_OnConnected			= onConnected ;
			m_OnReceivedData		= onReceivedData ;
			m_OnReceivedText		= onReceivedText ;
			m_OnDisconnected		= onDisconnected ;
			m_OnError				= onError ;

			//----------------------------------

			m_IsConnecting = false ;
		}

		/// <summary>
		/// 接続
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="serverPortNumber"></param>
		public void Connect( string serverAddress, int serverPortNumber, bool isBlocking = true )
		{
			// 設定を保存する
			m_ServerAddress		= serverAddress ;
			m_ServerPortNumber	= serverPortNumber ; 

			//----------------------------------

			string url = "ws://" + serverAddress + ":" + serverPortNumber.ToString() + "/" ;

			Debug.Log( "<color=#00FF00>[CLIENT] Connect -> 接続先 " + url + "</color>" ) ;

			m_WebSocket = new WebSocket( url ) ;

			// コールバックを設定する
			m_WebSocket.OnOpen		+= OnOpen ;
			m_WebSocket.OnMessage	+= OnMessage ;
			m_WebSocket.OnClose		+= OnClose ;
			m_WebSocket.OnError		+= OnError ;

			if( isBlocking == true )
			{
				// 同期接続
				m_WebSocket.Connect() ;
			}
			else
			{
				// 非同期接続
				m_WebSocket.ConnectAsync() ;
			}
		}

		//-----------------------------------------------------------

		// 接続された際に呼び出される
		private void OnOpen( object sender, EventArgs e )
		{
			// 接続状態
			m_IsConnecting = true ;

			//----------------------------------

			// 通信スレッドのコールバックを呼ぶ
			m_OnConnected?.Invoke( m_ServerAddress, m_ServerPortNumber ) ;	

			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドのコールバックを呼ぶ
					m_OnConnected_Main?.Invoke( m_ServerAddress, m_ServerPortNumber ) ;
				},
				null ) ;
			}
		}

		// 受信した際に呼び出される
		private void OnMessage( object sender, MessageEventArgs e )
		{
			// 通信スレッドのコールバックを呼ぶ
			if( e.IsBinary == true && e.RawData != null )
			{
				m_OnReceivedData?.Invoke( e.RawData ) ;
			}
			if( e.IsText == true && e.Data != null )
			{
				m_OnReceivedText?.Invoke( e.Data ) ;
			}

			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドのコールバックを呼ぶ
					if( e.IsBinary == true && e.RawData != null )
					{
						m_OnReceivedData_Main?.Invoke( e.RawData ) ;
					}
					if( e.IsText == true && e.Data != null )
					{
						m_OnReceivedText_Main?.Invoke( e.Data ) ;
					}
				},
				null ) ;
			}
		}

		// 切断された際に呼び出される
		private void OnClose( object sender, CloseEventArgs e )
		{
			// 切断状態
			m_IsConnecting = false ;

			//----------------------------------

			// 通信スレッドのコールバックを呼ぶ
			m_OnDisconnected?.Invoke(  e.Code, e.Reason ) ;

			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドのコールバックを呼ぶ
					m_OnDisconnected_Main?.Invoke( e.Code, e.Reason ) ;
				},
				null ) ;
			}

			//----------------------------------

			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		// 異常が発生した際に呼び出される
		private void OnError(  object sender, ErrorEventArgs e )
		{
			// 通信スレッドのコールバックを呼ぶ
			m_OnError?.Invoke( e.Message ) ;
			
			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドのコールバックを呼ぶ
					m_OnError_Main?.Invoke( e.Message ) ;
				},
				null ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 送信する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Send( byte[] data )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			m_WebSocket.Send( data ) ;

			return true ;
		}

		/// <summary>
		/// 送信する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Send( string text )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			m_WebSocket.Send( text ) ;

			return true ;
		}

		/// <summary>
		/// 切断する
		/// </summary>
		public void Disconnect()
		{
			// 接続状態
			m_IsConnecting = false ;

			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		/// <summary>
		/// 切断する
		/// </summary>
		public void Close()
		{
			// 接続状態
			m_IsConnecting = false ;

			if( m_WebSocket != null )
			{
				m_WebSocket.Close() ;
				m_WebSocket = null ;
			}
		}

		/// <summary>
		/// 接続状態
		/// </summary>
		public bool IsConnecting	=> m_IsConnecting ;

		//-------------------------------------------------------------------------------------------
		// オプション機能

		/// <summary>
		/// 指定のポート番号がＴＣＰで使用されているか確認する
		/// </summary>
		/// <param name="portNumber"></param>
		/// <returns></returns>
		public static bool IsTcpPortNumberUsing( int portNumber )
		{
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties() ;
			IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners() ;

			foreach( IPEndPoint endPoint in endPoints )
			{
				if( endPoint.Port == portNumber )
				{
					// 使用中
					return true ;
				}
			}

			// 未使用
			return false ;
		}

		/// <summary>
		/// 指定のポート番号がＵＤＰで使用されているか確認する
		/// </summary>
		/// <param name="portNumber"></param>
		/// <returns></returns>
		public static bool IsUdpPortNumberUsing( int portNumber )
		{
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties() ;
			IPEndPoint[] endPoints = ipGlobalProperties.GetActiveUdpListeners() ;

			foreach( IPEndPoint endPoint in endPoints )
			{
				if( endPoint.Port == portNumber )
				{
					// 使用中
					return true ;
				}
			}

			// 未使用
			return false ;
		}
	}
}
