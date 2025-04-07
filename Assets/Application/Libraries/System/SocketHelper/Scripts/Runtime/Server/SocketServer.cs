#if UNITY_2019_4_OR_NEWER
#define UNITY
#endif

#nullable enable
#pragma warning disable CA1822
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable IDE0028
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable IDE0300
#pragma warning disable IDE0305


using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading ;
using System.Threading.Tasks ;

using System.Net ;
using System.Net.Sockets ;
using System.Net.NetworkInformation ;

#if UNITY
using UnityEngine ;
#endif


namespace SocketHelper
{
	/// <summary>
	/// SocketServer Version 2025/04/07
	/// </summary>
	public partial class SocketServer
	{
		private Socket												m_ServerSocketTcp ;
		
		private UdpClient											m_ServerSocketUdp ;

		private readonly Action<ClientHandler>						m_OnTcpAccepted ;

		private readonly Action<ClientHandler,byte[]>				m_OnTcpReceived ;

		private readonly Action<ClientHandler>						m_OnTcpDisconnected ;

		private readonly Action<byte[],string,int>					m_OnUdpReceived ;


		private readonly CancellationToken							m_OwnerCancellationToken ;

		private readonly SynchronizationContext						m_MainThreadContext ;

		private	long												m_ClientIdentity ;

		// クライアントハンドラー保持
		private readonly List<ClientHandler>						m_ClientHandlers ;

		private readonly object										m_ClientHandlersLockObject ;

		private CancellationTokenSource								m_MainCancellationTokenSource ;

		//-----------------------------------

		/// <summary>
		/// ＴＣＰの最大パケットサイズ
		/// </summary>
		public  int MaxTcpPacketSize
		{
			get
			{
				return m_MaxTcpPacketSize ;
			}
			set
			{
				m_MaxTcpPacketSize = value ;
				if( m_MaxTcpPacketSize <  256 )
				{
					m_MaxTcpPacketSize  = 256 ;
				}
			}
		}
		private int		m_MaxTcpPacketSize = 65536 ;

		//-----------------------------------

		// ＴＣＰの接続待ち受け用のアドレス
		private string	m_Address ;

		/// <summary>
		/// ＴＣＰの接続待ち受け用のアドレス
		/// </summary>
		public string	Address => m_Address ;

		// ＴＣＰの接続待ち受け用のポート
		private int		m_Port ;

		/// <summary>
		/// ＴＣＰの接続待ち受け用のポート
		/// </summary>
		public int		Port => m_Port ;

		//-----------------------------------------------------------

		/// <summary>
		/// ＵＤＰパケットクラス
		/// </summary>
		public class UdpPacket
		{
			public byte[]	Data ;
			public string	Address ;
			public int		Port ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			/// <param name="ipAddress"></param>
			/// <param name="portNumber"></param>
			public UdpPacket( byte[] data, string address, int port )
			{
				Data	= data ;
				Address	= address ;
				Port	= port ;
			}
		}

		// 送信用パケット群
		private List<UdpPacket>				m_SendUdpPackets ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		public SocketServer()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ownerCancallationToken"></param>
		public SocketServer
		(
			Action<ClientHandler> onTcpAccepted,
			Action<ClientHandler,byte[]> onTcpReceived,
			Action<ClientHandler> onTcpDisconnected,
			Action<byte[],string,int> onUdpReceived,
			CancellationToken ownerCancellationToken,
			SynchronizationContext mainThreadContext = null
		)
		{
			m_OnTcpAccepted				= onTcpAccepted ;
			m_OnTcpReceived				= onTcpReceived ;
			m_OnTcpDisconnected			= onTcpDisconnected ;
			m_OnUdpReceived				= onUdpReceived ;

			m_OwnerCancellationToken	= ownerCancellationToken ;

			m_MainThreadContext			= mainThreadContext ;

			//----------------------------------

			m_ClientIdentity			= 1 ;

			m_ClientHandlers			= new () ;
			m_ClientHandlersLockObject	= new () ;
		}

		/// <summary>
		/// 実行する
		/// </summary>
		/// <returns></returns>
		public bool Start( string address, int tcpPort, int udpPort )
		{
			if( m_ServerSocketTcp != null )
			{
				return false ;
			}

			// ＴＣＰ接続待ち受け用のソケット
			m_ServerSocketTcp = new ( SocketType.Stream, ProtocolType.Tcp ) ;

//			Debug.Log( "------------ポート番号 : " + port ) ;


			if( udpPort >  0 )
			{
				// ＵＤＰ通信用ソケット
				m_ServerSocketUdp = new ( udpPort ) ;

				// ＵＤＰ送信パケットバッファ
				m_SendUdpPackets = new() ;
			}

			//----------------------------------------------------------
			// タスクキャンセル用のトークンを生成する

			if( m_MainCancellationTokenSource != null )
			{
				// 既に生成済みなら破棄する(保険)
				m_MainCancellationTokenSource.Cancel() ;
				m_MainCancellationTokenSource.Dispose() ;
			}

			if( m_OwnerCancellationToken == default )
			{
				m_MainCancellationTokenSource = new () ;
			}
			else
			{
				m_MainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken ) ;
			}

			//----------------------------------------------------------

//			Debug.Log( "接続待ちに移行する : " + address + " : " + port, Color.green ) ;

			// ＴＣＰの接続待受を開始する
			StartAcceptTcp( address, tcpPort ) ;

			if( udpPort >  0 )
			{
				// ＵＤＰの受信を開始する
				StartReceiveUdp() ;
			}

			return true ;
		}

		// 接続の待ち受けを行う
		private void StartAcceptTcp( string address, int port )
		{
			IPEndPoint ipEndPoint ;

			if( string.IsNullOrEmpty( address ) == false )
			{
				ipEndPoint = new IPEndPoint( IPAddress.Parse( address ), port ) ;
			}
			else
			{
				ipEndPoint = new IPEndPoint( IPAddress.Any, port ) ;
			}

			//----------------------------------
			// 結果的にバインドするエンドポイント情報をモニタリング用に別途保持しておく

			m_Address	= ipEndPoint.Address.ToString() ;
			m_Port		= ipEndPoint.Port ;

			//----------------------------------------------------------

			m_ServerSocketTcp.Bind( ipEndPoint ) ;
			m_ServerSocketTcp.Listen( 16 ) ;

			string path = ipEndPoint.ToString() ;
//			Debug.Log( "[SocketServer] TCP 接続待ち : " + path, Color.red ) ;

			//----------------------------------------------------------

			// 最初の接続受付開始
			m_ServerSocketTcp.BeginAccept( StartAcceptTcp_Callback, m_ServerSocketTcp ) ;
		}

		private void StartAcceptTcp_Callback( IAsyncResult ar )
		{
			var serverSocketTcp = ( Socket )ar.AsyncState ;

			try
			{
				var socketTcp = serverSocketTcp.EndAccept( ar ) ;

				var ipEndPoint = ( IPEndPoint )socketTcp.RemoteEndPoint ;
				var endPoint = new IPEndPoint( ipEndPoint.Address, ipEndPoint.Port ) ;

				Debug.Log( "==============================================" ) ;
				Debug.Log( "[TCP] クライアントから接続要求あり EndPoint = " + endPoint.ToString() ) ;

				// 接続コールバックを呼ぶ
				OnTcpAccepted( socketTcp ) ;

				//-----------------------------

				// 再び接続受付を呼ぶ
				serverSocketTcp.BeginAccept( StartAcceptTcp_Callback, serverSocketTcp ) ;
			}
			catch( Exception )
			{
				// ソケットがクローズされている可能性大
				return ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// ＵＤＰの受信を処理する(新)
		private void StartReceiveUdp()
		{
			// 最初の受信受付開始
			m_ServerSocketUdp.BeginReceive( StartReceiveUdp_Callback, m_ServerSocketUdp ) ;
		}

		// 受信した際に呼び出されるコールバック(サブスレッドである事に注意する)
		private void StartReceiveUdp_Callback( IAsyncResult ar )
		{
			var serverSocketUdp = ( UdpClient )ar.AsyncState ;

			// データを取得する
			try
			{
				IPEndPoint ipEndPoint = null ;
				var udpPacket = serverSocketUdp.EndReceive( ar, ref ipEndPoint ) ;
				if( udpPacket != null && udpPacket.Length >  0 )
				{
#if !UNITY
					// コールバックを呼ぶ
					m_OnUdpReceived?.Invoke( udpPacket, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
#else
					// コールバックを呼ぶ
					if( m_OnUdpReceived != null )
					{
						if( m_MainThreadContext != null )
						{
							// メインスレッド限定あり呼び出し
							if( SynchronizationContext.Current == m_MainThreadContext )
							{
								// パケットが完成した
								m_OnUdpReceived( udpPacket, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
							}
							else
							{
								m_MainThreadContext.Post( ( _ ) =>
								{
									// パケットが完成した
									m_OnUdpReceived( udpPacket, ipEndPoint.Address.ToString(), ipEndPoint.Port  ) ;
								}, null ) ;
							}
						}
						else
						{
							// メインスレッド限定なし呼び出し
							m_OnUdpReceived( udpPacket, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
						}
					}
#endif
					//-----------------------------
					// 再び受信監視処理を呼ぶ
					serverSocketUdp.BeginReceive( StartReceiveUdp_Callback, serverSocketUdp ) ;
				}
				else
				{
					Debug.Log( "[警告]UDPの受信が止まってしまった" ) ;
				}
			}
			catch( Exception ex )
			{
				Debug.Log( "[UDP] 受信で何らかの異常発生 : " + ex.Message ) ;
				return ;
			}
		}

		//-----------------------------------

		// 送信時のスレッド間の排他制御(複数のスレッドから同時参照があるので排他制御が必要)

		private readonly object m_SendUdpLockObject = new () ;

		// 送信中かどうかのフラグ
		private bool m_IsUdpSendRunning = false ;

		/// <summary>
		/// ＵＤＰパケットを送信する
		/// </summary>
		/// <param name="data"></param>
		public bool SendUdp( byte[] data, string address, int port )
		{
			if( data == null || data.Length <= 0 )
			{
				return false ;
			}

			if( m_ServerSocketUdp == null )
			{
				return false ;
			}

			//----------------------------------

			// サブスレッドの排他制御
			lock( m_SendUdpLockObject )
			{
				if( m_IsUdpSendRunning == false )
				{
					// 送信中ではない

					m_IsUdpSendRunning = true ;	// 通信中に移行する

					m_ServerSocketUdp.BeginSend( data, data.Length, address, port, SendUdp_Callback, m_ServerSocketUdp ) ;
				}
				else
				{
					// 送信中である

					// 送信バッファ群に積む
					m_SendUdpPackets.Add( new UdpPacket( data, address, port ) ) ;
				}
			}

			return true ;
		}

		// 送信終了時に呼び出される(別スレッドである事に注意する)
		private void SendUdp_Callback( IAsyncResult ar )
		{
			var serverSocketUdp = ( UdpClient )ar.AsyncState ;

			try
			{
				int sendSize = serverSocketUdp.EndSend( ar ) ;
				if( sendSize >  0 )
				{
					// サブスレッドの排他制御
					lock( m_SendUdpLockObject )
					{
						// 送信バッファに次以降のＵＤＰパケットが溜まっている場合は引き続きそれらを送信する
						if( m_SendUdpPackets.Count >  0 )
						{
							// パケットを取り出す
							var udpPacket = m_SendUdpPackets[ 0 ] ;
							m_SendUdpPackets.RemoveAt( 0 ) ;

							//------------------------------

							var udpData = udpPacket.Data ;

							serverSocketUdp.BeginSend( udpData, udpData.Length, udpPacket.Address, udpPacket.Port, SendUdp_Callback, serverSocketUdp ) ;
						}
						else
						{
							// 送信中ではなくなった
							m_IsUdpSendRunning = false ;
						}
					}
				}
				else
				{
					// 問題発生
				}
			}
			catch( Exception e )
			{
				// 問題発生
				Debug.Log( "UDP 送信で例外発生 : " + e.Message ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// SocketServer の内部コールバック群

		// 新しい接続があった場合に呼び出される
		private void OnTcpAccepted( Socket socket )
		{
			var clientHandler = new ClientHandler
			(
				m_ClientIdentity,
				socket,
				m_OnTcpReceived,
				OnTcpDisconnected,
				m_MaxTcpPacketSize,
				m_MainCancellationTokenSource.Token,
				m_MainThreadContext
			) ;

			// 要素数が変化する際は排他処理を行う
			lock( m_ClientHandlersLockObject )
			{
				// クライアントハンドラーに割り振る識別子を変化させる
				m_ClientIdentity ++ ;

				// 新しいクライアントハンドラーを追加する
				m_ClientHandlers.Add( clientHandler ) ;
			}

			//----------------------------------

			// コールバックを呼ぶ

			// ※各クライアントの Start を呼ぶより先に呼ぶこと
			// 　でないとクライアントがこのコールバックを呼ぶ前に切断を実行している場合
			// 　既に無効になったソケットを接続コールバックに受け渡す事になる
#if !UNITY
			m_OnTcpAccepted?.Invoke( clientHandler ) ;
#else
			// コールバックを呼ぶ
			if( m_OnTcpAccepted != null )
			{
				if( m_MainThreadContext != null )
				{
					// メインスレッド限定あり呼び出し
					if( SynchronizationContext.Current == m_MainThreadContext )
					{
						// パケットが完成した
						m_OnTcpAccepted( clientHandler ) ;
					}
					else
					{
						m_MainThreadContext.Post( ( _ ) =>
						{
							// パケットが完成した
							m_OnTcpAccepted( clientHandler ) ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッド限定なし呼び出し
					m_OnTcpAccepted( clientHandler ) ;
				}
			}
#endif
			//----------------------------------

			// 新しいクライアントハンドラーの処理を開始する
			clientHandler.Start() ;
		}

		// クライアントから切断があった際に呼び出される
		private void OnTcpDisconnected( ClientHandler clientHandler )
		{
			// 要素数が変化する際は排他処理を行う
			lock( m_ClientHandlersLockObject )
			{
				m_ClientHandlers?.Remove( clientHandler ) ;
			}

			//----------------------------------
			// コールバックを呼ぶ

#if !UNITY
			m_OnTcpDisconnected?.Invoke( clientHandler ) ;
#else
			// コールバックを呼ぶ
			if( m_OnTcpDisconnected != null )
			{
				if( m_MainThreadContext != null )
				{
					// メインスレッド限定あり呼び出し
					if( SynchronizationContext.Current == m_MainThreadContext )
					{
						// パケットが完成した
						m_OnTcpDisconnected( clientHandler ) ;
					}
					else
					{
						m_MainThreadContext.Post( ( _ ) =>
						{
							// パケットが完成した
							m_OnTcpDisconnected( clientHandler ) ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッド限定なし呼び出し
					m_OnTcpDisconnected( clientHandler ) ;
				}
			}
#endif
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// クラアント全てを破棄し待受を停止する
		/// </summary>
		public void Stop()
		{
			// 要素数が変化する際は排他処理を行う
			lock( m_ClientHandlersLockObject )
			{
				// 全クライアントとの切断
				foreach( var clientHandler in m_ClientHandlers )
				{
					clientHandler.Dispose() ;
				}

				m_ClientHandlers.Clear() ;
			}

			// タスク全てを停止する
			if( m_MainCancellationTokenSource != null )
			{
				if( m_MainCancellationTokenSource.IsCancellationRequested == false )
				{
					m_MainCancellationTokenSource.Cancel() ;
				}

				m_MainCancellationTokenSource.Dispose() ;
				m_MainCancellationTokenSource = null ;
			}

			// ＵＤＰソケットを破棄する
			if( m_ServerSocketUdp != null )
			{
				m_ServerSocketUdp.Dispose() ;
				m_ServerSocketUdp = null ;
			}

			// ＴＣＰソケットを破棄する
			if( m_ServerSocketTcp != null )
			{
				m_ServerSocketTcp.Dispose() ;
				m_ServerSocketTcp = null ;
			}
		}

		/// <summary>
		/// 全ての内部処理を終了させる
		/// </summary>
		public void Dispose()
		{
			// 念の為停止も実行する(保険)　※既に切断済みであれば何もしない
			Stop() ;

			//----------------------------------

//			Debug.Log( "SocketServer 終了", Color.red ) ;
		}


		//-------------------------------------------------------------------------------------------
		// Utilities

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
