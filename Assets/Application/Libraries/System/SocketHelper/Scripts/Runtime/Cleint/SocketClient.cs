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
#pragma warning disable IDE0031
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable IDE0300
#pragma warning disable IDE0305


using System ;
using System.Collections.Generic ;

using System.Net.NetworkInformation ;
using System.Net ;

using System.Threading ;
using System.Threading.Tasks ;

// using System.Net ;
using System.Net.Sockets ;

#if UNITY
using UnityEngine ;
#endif
//using Debug = DebugHelper.Debug ;	// Debug クラスは UnityEngine 側ではなく DebugHelper 側を優先させる


namespace SocketHelper
{
	/// <summary>
	/// Socket のクライアント側の管理用クラス Version 2025/04/06
	/// </summary>
	public class SocketClient
	{
		// サーバーアドレス
		private string													m_ServerAddress ;

		// サーバーポート
		private int														m_ServerPort ;

		// ソケットのインスタンス
		private Socket													m_SocketTcp ;

		// ソケットのインスタンス
		private UdpClient												m_SocketUdp ;

		// 受信時のコールバック
		private readonly Action<byte[]>									m_OnTcpReceived ;

		// 切断時のコールバック
		private readonly Action											m_OnTcpDisconnected ;

		// 受信時のコールバック
		private readonly Action<byte[],string,int>						m_OnUdpReceived ;

		// ＴＣＰの最大パケットサイズ
		private readonly int											m_MaxTcpPacketSize ;


		// インスタンスを所持しているオーナーのキャンセルトークン
		private readonly CancellationToken								m_OwnerCancellationToken ;

		// 受信用バッファ
		private readonly byte[]											m_ReceiveBuffer ;

		// 送信用バッファ
		private readonly byte[]											m_SendBuffer ;

		// 送信用パケット群
		private readonly List<byte[]>									m_SendTcpPackets ;

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
				Address	= address;
				Port	= port ;
			}
		}

		// 送信用パケット群
		private readonly List<UdpPacket>								m_SendUdpPackets ;

		// 切断処理が既に行われているか
		private bool													m_IsClosed ;

		//-----------------------------------------------------------

		// メインスレッドのコンテキスト(同期を取るためのもの)
		private readonly SynchronizationContext							m_MainThreadContext ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SocketClient
		(
			Action<byte[]>				onTcpReceived,
			Action						onTcpDisconnected,
			Action<byte[],string,int>	onUdpReceived,
			int							maxTcpPacketSize,
			CancellationToken			ownerCancellationToken,

			// メインスレッドの同期を取るためのコンテキスト
			SynchronizationContext		mainThreadContext			= null
		)
		{
			if( onUdpReceived != null )
			{
				// ソケットを生成する
				m_SocketUdp = new UdpClient( 0 ) ;
			}

			//----------------------------------

			m_OnTcpReceived				= onTcpReceived ;
			m_OnTcpDisconnected			= onTcpDisconnected ;

			m_OnUdpReceived				= onUdpReceived ;

			m_MaxTcpPacketSize			= maxTcpPacketSize ;

			//----------------------------------

			m_OwnerCancellationToken	= ownerCancellationToken ;

			// メインスレッドの同期を取るためのコンテキスト
			m_MainThreadContext			= mainThreadContext ;

			//----------------------------------

			// 受信バッファはひとまず最大６４ＫＢ
			m_ReceiveBuffer = new byte[ 65536 ] ;

			// 受信バッファはひとまず最大６４ＫＢ
			m_SendBuffer = new byte[ 4 + 65536 ] ;

			// 送信パケット群
			m_SendTcpPackets = new() ;

			// 送信パケット群
			m_SendUdpPackets = new() ;

			// 切断処理が既に行われているか
			m_IsClosed = false ;

			//----------------------------------
			// UDP 用の CancellationTokenSource

			if( onUdpReceived != null )
			{
				// ＵＤＰの受信を開始する
				StartReceiveUdp() ;
			}
		}

		/// <summary>
		/// サーバーへ接続を行う
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="serverPort"></param>
		/// <param name="onCnnected"></param>
		public void Connect( string serverAddress, int serverPort, Action<bool> onTcpConnected = null )
		{
			_ = ConnectAsync( serverAddress, serverPort, onTcpConnected ) ;
		}

		/// <summary>
		/// サーバーへ接続を行う
		/// </summary>
		/// <param name="serverAddress"></param>
		/// <param name="serverPortNumber"></param>
		public async Task<bool> ConnectAsync( string serverAddress, int serverPort, Action<bool> onTcpConnected = null, CancellationToken cancellationToken = default )
		{
			if( m_SocketTcp != null )
			{
				// 既に Dispose() が行われてソケットが破棄されている
				Debug.Log( "-----------[TCP 接続] 既に接続済み" ) ;
				return false ;
			}

			if( string.IsNullOrEmpty( serverAddress ) == true || serverPort == 0 )
			{
				// サーバーアドレスかサーバーポートが不正
				return false ;
			}

			//----------------------------------------------------------
			// タスクキャンセル用のトークンを生成する

			// ソケットを生成する
			m_SocketTcp = new Socket( SocketType.Stream, ProtocolType.Tcp ) ;

			//----------------------------------------------------------

			string url = serverAddress + ":" + serverPort.ToString() ;

			Debug.Log( "<color=#00FF00>[CLIENT] Connect -> 接続先 " + url + "</color>" ) ;

			// 同期接続

			// デバッグ用に記録しておく
			m_ServerAddress = serverAddress ;
			m_ServerPort	= serverPort ;

			if( serverAddress == "localhost" )
			{
				// Socket は localhost を理解出来ないため変換が必要
				serverAddress  = "127.0.0.1" ;
			}

			// 接続実行
			bool isConnectRunning = true ;
			m_SocketTcp.BeginConnect( serverAddress, serverPort, ( IAsyncResult ar ) =>
			{
				isConnectRunning = false ;
				if( m_SocketTcp == null )
				{
					// 既にソケットが破棄されている
					return ;
				}
				m_SocketTcp.EndConnect( ar ) ;
			}, null ) ;

			//----------------------------------------------------------

			CancellationTokenSource cancellationTokenSource = null ;
			CancellationToken activeCancellationToken = default ;

			if( m_OwnerCancellationToken != default && cancellationToken == default )
			{
				activeCancellationToken = m_OwnerCancellationToken ;
			}
			else
			if( m_OwnerCancellationToken == default && cancellationToken != default )
			{
				activeCancellationToken = cancellationToken ;
			}
			else
			if( m_OwnerCancellationToken != default && cancellationToken != default )
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken, cancellationToken ) ;
				activeCancellationToken = cancellationTokenSource.Token ;
			}

			bool isCanceled = false ;

			// 接続完了を待つ
			while( m_SocketTcp != null )
			{
				if( activeCancellationToken != default && activeCancellationToken.IsCancellationRequested == true )
				{
					// ConnectAsync のみの停止要求が出された
					isCanceled = true ;
					break ;
				}

				if( isConnectRunning == false )
				{
					// 接続プロセス終了
					break ;
				}

				await Task.Yield() ;
			}

			if( cancellationTokenSource != null )
			{
				cancellationTokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}

			if( m_SocketTcp == null )
			{
				// 切断された可能性がある
				onTcpConnected?.Invoke( false ) ;
				return false ;
			}

			//----------------------------------

			if( m_SocketTcp.Connected == false )
			{
				Debug.Log( "<color=#FFFF00>直接的な結果として接続に失敗した</color>" ) ;

				Disconnect( false ) ;

				// 接続出来なかった(例外が発生したか中断された)
				onTcpConnected?.Invoke( false ) ;
				return false ;
			}


			Debug.Log( "<color=#FFFF00>接続自体は成功 : " + serverAddress + " : " + serverPort + "</color>" ) ;

			// 接続時のコールバックを呼ぶ
			onTcpConnected?.Invoke( true ) ;

			//----------------------------------------------------------

			// 処理を開始する(必ず一番最後に呼ぶ)

			Start() ;

			return true ;
		}

		/// <summary>
		/// ＴＣＰで接続中かどうか
		/// </summary>
		public bool IsConnected
		{
			get
			{
				return m_SocketTcp != null && m_SocketTcp.Connected == true ;
			}
		}

		/// <summary>
		/// 処理を開始する
		/// </summary>
		private void Start()
		{
			// 送信パケットバッファをクリアする
			m_SendTcpPackets.Clear() ;

			//----------------------------------

			// ＴＣＰの受信待ちを開始する
			StartTcpReceive() ;
		}


		/// <summary>
		/// ＴＣＰのローカル接続ポート番号を取得する
		/// </summary>
		/// <returns></returns>
		public int GetTcpPort()
		{
			if( m_SocketTcp == null )
			{
				// 接続していない
				return 0 ;
			}

			var ipEndPoint = ( IPEndPoint )m_SocketTcp.LocalEndPoint ;

			return ipEndPoint.Port ;
		}

		/// <summary>
		/// ＵＤＰのローカル接続ポート番号を取得する
		/// </summary>
		/// <returns></returns>
		public int GetUdpPort()
		{
			if( m_SocketUdp == null )
			{
				// 生成していない
				return 0 ;
			}

			var ipEndPoint = ( IPEndPoint )m_SocketUdp.Client.LocalEndPoint ;

			return ipEndPoint.Port ;
		}

		//-------------------------------------------------------------------------------------------

		// 参考
		// https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
		// https://csharp.nomux2.net/socket-client/

		// ＴＣＰの受信を開始する
		private void StartTcpReceive()
		{
			// 最初の受信待ちを呼ぶ
			m_SocketTcp.BeginReceive( m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, StartReceiveTcp_Callback, m_SocketTcp ) ;
		}

		private readonly int	m_ReceiveHeaderSize = 8 ;
		private readonly byte[]	m_ReceiveHeaderData = new byte[ 8 ] ;
		private int				m_ReceiveHeaderStep = 0 ;

		private int				m_ReceivePacketSize = 0 ;
		private byte[]			m_ReceivePacketData = null ;
		private int				m_ReceivePacketStep = 0 ;

		// データを受信した際に呼び出される(サブスレッドである事に注意)
		private void StartReceiveTcp_Callback( IAsyncResult ar )
		{
			if( ProcessReceiveTcp( ar ) == false )
			{
				// 切断された
				OnTcpDisconnectedFromServer() ;
			}
		}

		private bool ProcessReceiveTcp( IAsyncResult ar )
		{
			var socketTcp = ( Socket )ar.AsyncState ;

			int	receiveSize = 0 ;
			int	receiveStep = 0 ;

			// ここの try ～ catch は必須(無いと m_Socket 切断後に isDisconnected の値設定に到達できない)
			try
			{
				if( socketTcp.Connected == true )
				{
					receiveSize = socketTcp.EndReceive( ar ) ;
					if( receiveSize == 0 )
					{
						Debug.Log( "[TCP] 受信待ち中に０バイト受信が発生した" ) ;

						return false ;
					}

					//----------------------------------

					int		requiredSize ;

					uint	crc ;
					byte	xor = 0xAA ;
					int		xor_index ;

					// 現在のフレームで受信したデータを全て処理しきるまで繰り返し処理する
					while( receiveStep <  receiveSize )
					{
						if( m_ReceivePacketSize == 0 )
						{
							// パケットサイズが不明

							while( m_ReceivePacketSize == 0 && receiveStep <  receiveSize )
							{
								// ヘッダ部分の固定長のデータを取得する
								
								requiredSize = Math.Min( m_ReceiveHeaderSize - m_ReceiveHeaderStep, receiveSize - receiveStep ) ;

								Array.Copy( m_ReceiveBuffer, receiveStep, m_ReceiveHeaderData, m_ReceiveHeaderStep, requiredSize ) ;
								receiveStep += requiredSize ;
								m_ReceiveHeaderStep  += requiredSize ;

								if( m_ReceiveHeaderStep == m_ReceiveHeaderSize )
								{
									// サイズ分が溜まった

									for( xor_index  = 0 ;  xor_index <  m_ReceiveHeaderSize ; xor_index ++ )
									{
										m_ReceiveHeaderData[ xor_index ] ^= xor ;
									}

									crc = ( uint )(
										  m_ReceiveHeaderData[ 4 ]         |
										( m_ReceiveHeaderData[ 5 ] <<  8 ) |
										( m_ReceiveHeaderData[ 6 ] << 16 ) |
										( m_ReceiveHeaderData[ 7 ] << 24 ) ) ;

									if( crc != GetCRC32( m_ReceiveHeaderData, 0, 4 ) )
									{
										// サイズに異常が見られる

										// 不正アクセスなので強制切断
										return false ;
									}

									m_ReceivePacketSize =
										  m_ReceiveHeaderData[ 0 ]         |
										( m_ReceiveHeaderData[ 1 ] <<  8 ) |
										( m_ReceiveHeaderData[ 2 ] << 16 ) |
										( m_ReceiveHeaderData[ 3 ] << 24 ) ;

									if( m_ReceivePacketSize >  m_MaxTcpPacketSize )
									{
										// 最大サイズを超えている

										// 不正アクセスなので強制切断
										return false ;
									}

									if( m_ReceivePacketSize >  0 )
									{
										m_ReceivePacketData = new byte[ m_ReceivePacketSize ] ;
										m_ReceivePacketStep = 0 ;
									}
									else
									{
										// もう一度取り直し(ワーニングは出した方が良い)
										m_ReceiveHeaderStep = 0 ;
									}
								}
							}
						}

						if( m_ReceivePacketSize >  0 && m_ReceivePacketStep <  m_ReceivePacketSize && receiveStep <  receiveSize )
						{
							// パケットサイズが確定

							// データ部をコピーする

							requiredSize = Math.Min( m_ReceivePacketSize - m_ReceivePacketStep, receiveSize - receiveStep ) ;
							Array.Copy( m_ReceiveBuffer, receiveStep, m_ReceivePacketData, m_ReceivePacketStep, requiredSize ) ;
							receiveStep += requiredSize ;
							m_ReceivePacketStep  += requiredSize ;

							if( m_ReceivePacketStep == m_ReceivePacketSize )
							{
		//						Debug.Log( "受信パケット完成 [ " + m_ReceivePacketSize + " ]" ) ;

								// コピーしておかないと次の受信で上書きされてしまう
								byte[] tcpPacket = new byte[ m_ReceivePacketSize ] ;
								Array.Copy( m_ReceivePacketData, tcpPacket, m_ReceivePacketSize ) ;

								//-------------------------------

								// コールバックを呼ぶ
								if( m_OnTcpReceived != null )
								{
									if( m_MainThreadContext != null )
									{
										if( SynchronizationContext.Current == m_MainThreadContext )
										{
											// パケットが完成した
											m_OnTcpReceived( tcpPacket ) ;
										}
										else
										{
											m_MainThreadContext.Post( ( _ ) =>
											{
												// パケットが完成した
												m_OnTcpReceived( tcpPacket ) ;
											}, null ) ;
										}
									}
									else
									{
										// パケットが完成した
										m_OnTcpReceived( tcpPacket ) ;
									}
								}

								//-------------------------------

								// パケットサイズを０に初期化
								m_ReceivePacketSize = 0 ;
								m_ReceiveHeaderStep = 0 ;
							}
						}
					}

					// 今回受信したデータは全て処理した

					// 再び受信待ちを呼ぶ
					socketTcp.BeginReceive( m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, StartReceiveTcp_Callback, socketTcp ) ;
				}
			}
			catch( Exception e )
			{
				Debug.Log( e.Message ) ;
				return false ;
			}

			// 通信継続
			return true ;
		}

		//-----------------------------------

		// 送信時のスレッド間の排他制御(複数のスレッドから同時参照があるので排他制御が必要)
		private readonly object m_SendTcpLockObject = new () ;

		// 送信中かどうかのフラグ
		private bool m_IsTcpSendRunning = false ;

		/// <summary>
		/// 送信する
		/// </summary>
		/// <param name="data"></param>
		public bool SendTcp( byte[] tcpPacket )
		{
			// 内容をコピーして送信バッファに積む

			if( tcpPacket == null || tcpPacket.Length <= 0 )
			{
				return false ;
			}

			if( m_SocketTcp == null || m_SocketTcp.Connected == false )
			{
				return false ;
			}

			//----------------------------------

			// サブスレッドの排他制御
			lock( m_SendTcpLockObject )
			{
				if( m_IsTcpSendRunning == false )
				{
					// 送信中ではない

					m_IsTcpSendRunning = true ;	// 通信中に移行する

					// トータルの送信データサイズ
					int requiredSize = EncodeSendData( tcpPacket, m_SendBuffer ) ;

					m_SocketTcp.BeginSend( m_SendBuffer, 0, requiredSize, SocketFlags.None, SendTcp_Callback, m_SocketTcp ) ;
				}
				else
				{
					// 送信中である

					// 送信バッファ群に積む
					m_SendTcpPackets.Add( tcpPacket ) ;
				}
			}

			return true ;
		}

		// 送信終了時に呼び出される(別スレッドである事に注意する)
		private void SendTcp_Callback( IAsyncResult ar )
		{
			if( ProcessSendTcp( ar ) == false )
			{
				// 切断された
				OnTcpDisconnectedFromServer() ;
			}
		}

		private bool ProcessSendTcp( IAsyncResult ar )
		{
			var socketTcp = ( Socket )ar.AsyncState ;

			try
			{
				if( socketTcp.Connected == true )
				{
					int sendSize = socketTcp.EndSend( ar ) ;
					if( sendSize >  0 )
					{
						// サブスレッドの排他制御
						lock( m_SendTcpLockObject )
						{
							if( m_SendTcpPackets.Count >  0 )
							{
								// パケットバッファにパケットが溜まっている

								// パケットを取り出す
								var tcpPacket = m_SendTcpPackets[ 0 ] ;
								m_SendTcpPackets.RemoveAt( 0 ) ;

								//------------------------------

								// トータルの送信データサイズ
								int requiredSize = EncodeSendData( tcpPacket, m_SendBuffer ) ;

								// 送信を実行する
								socketTcp.BeginSend( m_SendBuffer, 0, requiredSize, SocketFlags.None, SendTcp_Callback, socketTcp ) ;
							}
							else
							{
								// 送信中ではなくなった
								m_IsTcpSendRunning = false ;
							}
						}
					}
					else
					{
						return false ;
					}
				}
				else
				{
					return false ;
				}
			}
			catch( Exception e )
			{
				Debug.Log( "ＴＣＰの送信で例外発生 " + e.Message ) ;
				return false ;
			}

			// 通信継続
			return true ;
		}

		private int EncodeSendData( byte[] tcpPacket, byte[] tcpBuffer )
		{
			// サイズ部を追加
			int packetSize = tcpPacket.Length ;

			tcpBuffer[ 0 ] = ( byte )( packetSize       ) ;
			tcpBuffer[ 1 ] = ( byte )( packetSize >>  8 ) ;
			tcpBuffer[ 2 ] = ( byte )( packetSize >> 16 ) ;
			tcpBuffer[ 3 ] = ( byte )( packetSize >> 24 ) ;

			uint crc = GetCRC32( tcpBuffer, 0, 4 ) ;

			tcpBuffer[ 4 ] = ( byte )( crc       ) ;
			tcpBuffer[ 5 ] = ( byte )( crc >>  8 ) ;
			tcpBuffer[ 6 ] = ( byte )( crc >> 16 ) ;
			tcpBuffer[ 7 ] = ( byte )( crc >> 24 ) ;

			byte xor = 0xAA ;
			int xor_index ;

			for( xor_index  = 0 ;  xor_index <  8 ; xor_index ++ )
			{
				tcpBuffer[ xor_index ] ^= xor ;
			}

			//------------

			// データ部を追加
			Array.Copy( tcpPacket, 0, tcpBuffer, 8, packetSize ) ;

			return 8 + packetSize ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// パケットを送信し且つ全ての送信が完了するのを待つ(送信バッファが完全に空になるのを待つ)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public async Task<bool> SendTcpAsync( byte[] data, Action<bool> onSendTcp = null, CancellationToken cancellationToken = default )
		{
			var result = SendTcp( data ) ;
			if( result == false )
			{
				// 失敗
				onSendTcp?.Invoke( false ) ;
				return false ;
			}

			//----------------------------------
			// バッファに積む事自体は成功したので終了を待つ

			CancellationTokenSource cancellationTokenSource = null ;
			CancellationToken activeCancellationToken = default ;

			if( m_OwnerCancellationToken != default && cancellationToken == default )
			{
				activeCancellationToken = m_OwnerCancellationToken ;
			}
			else
			if( m_OwnerCancellationToken == default && cancellationToken != default )
			{
				activeCancellationToken = cancellationToken ;
			}
			else
			if( m_OwnerCancellationToken != default && cancellationToken != default )
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken, cancellationToken ) ;
				activeCancellationToken = cancellationTokenSource.Token ;
			}

			bool isCanceled	= false ;

			while( m_SocketTcp != null )
			{
				if( activeCancellationToken != default && activeCancellationToken.IsCancellationRequested == true )
				{
					isCanceled = true ;
					break ;
				}

				if( m_SendTcpPackets.Count == 0 )
				{
					// 送信パケットバッファが空になった
					break ;
				}

				// わずかにディレイを入れる
				await Task.Yield() ;
			}

			if( cancellationTokenSource != null )
			{
				cancellationTokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}

			if( m_SocketTcp == null )
			{
				// 切断された可能性がある
				onSendTcp?.Invoke( false ) ;
				return false ;
			}

			//----------------------------------

			// 成功
			onSendTcp?.Invoke( true ) ;
			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// ＵＤＰの受信を処理する(新)
		private void StartReceiveUdp()
		{
			// 最初の受信受付開始
			m_SocketUdp.BeginReceive( StartReceiveUdp_Callback, m_SocketUdp ) ;
		}

		// 受信した際に呼び出されるコールバック(サブスレッドである事に注意する)
		private void StartReceiveUdp_Callback( IAsyncResult ar )
		{
			var socketUdp = ( UdpClient )ar.AsyncState ;

			// データを取得する
			try
			{
				IPEndPoint ipEndPoint = null ;
				var packetData = socketUdp.EndReceive( ar, ref ipEndPoint ) ;
				if( packetData != null && packetData.Length >  0 )
				{
//					Debug.Log( "<color=#FF7F00>----------[UDP] データ受信 : データサイズ = " + packetData.Length + "</color>" ) ;

					//--------------------------------

					// コールバックを呼ぶ
					if( m_OnUdpReceived != null )
					{
						if( m_MainThreadContext != null )
						{
							if( SynchronizationContext.Current == m_MainThreadContext )
							{
								// パケットが完成した
								m_OnUdpReceived( packetData, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
							}
							else
							{
								m_MainThreadContext.Post( ( _ ) =>
								{
									// パケットが完成した
									m_OnUdpReceived( packetData, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
								}, null ) ;
							}
						}
						else
						{
							// パケットが完成した
							m_OnUdpReceived( packetData, ipEndPoint.Address.ToString(), ipEndPoint.Port ) ;
						}
					}

					//--------------------------------

					// 再び受信監視処理を呼ぶ
					socketUdp.BeginReceive( StartReceiveUdp_Callback, socketUdp ) ;
				}
			}
			catch( Exception e )
			{
				Debug.Log( "<color=#FF00FF>[UDP] 受信で何らかの異常発生 : " + e.Message + "</color>" ) ;
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

			if( m_SocketUdp == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// サブスレッドの排他制御
			lock( m_SendUdpLockObject )
			{
#if UNITY_EDITOR
				if( data.Length >  65500 )
				{
					Debug.LogWarning( $"<color=#FFFF00>ＵＤＰのデータサイズが 65500 バイトを超えています( {data.Length} ) 正常に送信されない可能性があります</color>" ) ;
				}
#endif
				//---------------------------------

				if( m_IsUdpSendRunning == false )
				{
					// 送信中ではない

					m_IsUdpSendRunning = true ;	// 通信中に移行する

					m_SocketUdp.BeginSend( data, data.Length, address, port, SendUdp_Callback, m_SocketUdp ) ;
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
			var socketUdp = ( UdpClient )ar.AsyncState ;

			try
			{
				int sendSize = socketUdp.EndSend( ar ) ;
				if( sendSize > 0 )
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

	//						Debug.Log( "<color=#FF7F00>再び送信 : " + udpData.Length + "</color>" ) ;
							socketUdp.BeginSend( udpData, udpData.Length, udpPacket.Address, udpPacket.Port, SendUdp_Callback, socketUdp ) ;
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
				Debug.Log( "<color=#FF0000>UDP 送信で例外発生 : " + e.Message + "</color>" ) ;
			}
		}

		/// <summary>
		/// 送信処理中のパケット数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSendingTcpPacketCount()
		{
			return m_SendTcpPackets.Count ;
		}

		//-----------------------------------------------------------

		// ＴＣＰソケットの排他制御用のオブジェクト
		private readonly object		m_DisconnectedTcpLockObject = new () ;

		// サーバーよって切断された際に呼び出される
		private void OnTcpDisconnectedFromServer()
		{
			lock( m_DisconnectedTcpLockObject )
			{
				if( m_IsClosed == false )
				{
					m_IsClosed  = true ;

					// ブロッキングメソッドなのでコールしてはならない
//					m_Socket.Shutdown( SocketShutdown.Both ) ;

					// タイミングによっては先に破棄されている可能性があるため null チェックは必要
					if( m_SocketTcp != null )
					{
						m_SocketTcp.Close() ;
						m_SocketTcp.Dispose() ;
						m_SocketTcp = null ;
					}

					//-------------------------------

					// コールバックを呼ぶ
					if( m_OnTcpDisconnected != null )
					{
						if( m_MainThreadContext != null )
						{
							if( SynchronizationContext.Current == m_MainThreadContext )
							{
								// 切断コールバックは自発的な切断では呼ばれないようにする
								m_OnTcpDisconnected() ;
							}
							else
							{
								m_MainThreadContext.Post( ( _ ) =>
								{
									// 切断コールバックは自発的な切断では呼ばれないようにする
									m_OnTcpDisconnected() ;
								}, null ) ;
							}
						}
						else
						{
							// 切断コールバックは自発的な切断では呼ばれないようにする
							m_OnTcpDisconnected() ;
						}
					}
				}
			}
		}

		/// <summary>
		/// 切断を実行する
		/// </summary>
		public void Disconnect( bool callbackEnabled )
		{
			// 切断の排他制御(複数のスレッドから実行される可能性がある)
			lock( m_DisconnectedTcpLockObject )
			{
				// 切断なので UdpSocket については処理不要

				if( m_SocketTcp != null )
				{
					Debug.Log( "<color=#FF7FFF>能動的にサーバーと接続しているソケットを切断する " + m_ServerAddress + " : " + m_ServerPort + "</color>" ) ;

					if( m_SocketTcp.Connected == true )
					{
						// ブロッキングに注意
						m_SocketTcp.Shutdown( SocketShutdown.Both ) ;

						// 送受信中のデータがあっても強制的に切断する
	//					m_SocketTcp.Disconnect( true ) ;
					}

					m_SocketTcp.Close() ;
					m_SocketTcp.Dispose() ;
					m_SocketTcp = null ;

					if( callbackEnabled == true )
					{
						//-------------------------------

						// コールバックを呼ぶ
						if( m_OnTcpDisconnected != null )
						{
							if( m_MainThreadContext != null )
							{
								if( SynchronizationContext.Current == m_MainThreadContext )
								{
									// 切断コールバックは自発的な切断では呼ばれないようにする
									m_OnTcpDisconnected() ;
								}
								else
								{
									m_MainThreadContext.Post( ( _ ) =>
									{
										// 切断コールバックは自発的な切断では呼ばれないようにする
										m_OnTcpDisconnected() ;
									}, null ) ;
								}
							}
							else
							{
								// 切断コールバックは自発的な切断では呼ばれないようにする
								m_OnTcpDisconnected() ;
							}
						}

						//-------------------------------
					}
				}
			}
		}

		/// <summary>
		/// 切断を実行する
		/// </summary>
		public async Task DisconnectAsync( bool callbackEnabled, CancellationToken cancellationToken = default )
		{
			bool isCanceled	= false ;

			// 送信を実行する
			bool isDisconectRunning = false ;

			// タスクキャンセル用のトークンソース
			CancellationTokenSource cancellationTokenSource = null ;

			//----------------------------------------------------------

			// 切断の排他制御(複数のスレッドから実行される可能性がある)
			lock( m_DisconnectedTcpLockObject )
			{
				// 切断なので UdpSocket については処理不要

				if( m_SocketTcp != null )
				{
					Debug.Log( "<color=#FF7FFF>能動的にサーバーと接続しているソケットを切断する</color>" ) ;
					// 切断の排他制御(複数のスレッドから実行される可能性がある)

					if( m_SocketTcp.Connected == true )
					{
						// ブロッキングなので非推奨
	//					m_SocketTcp.Shutdown( SocketShutdown.Both ) ;

						// 送受信中のデータがあっても強制的に切断する
	//					m_SocketTcp.Disconnect( true ) ;


						//----------------------------------
						// バッファに積む事自体は成功したので終了を待つ

						// ※ m_TcpCancellationTokenSource には必ず値が設定されている
						if( m_OwnerCancellationToken != default && cancellationToken != default )
						{
							cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken, cancellationToken ) ;
						}
						else
						if( m_OwnerCancellationToken != default && cancellationToken == default )
						{
							cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken ) ;
						}
						else
						if( m_OwnerCancellationToken == default && cancellationToken != default )
						{
							cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) ;
						}	

						//--------------

						// 一時的に生成したキャンセレーショントークンソースをきちんと破棄するためにめんどくさい処理が必要

						// 切断を実行する
						isDisconectRunning = true ;

						m_SocketTcp.BeginDisconnect( false, ( IAsyncResult ar ) =>
						{
							// パケットのストリームの送信完了
							isDisconectRunning = false ;

							if( m_SocketTcp == null )
							{
								// 既に破棄されている
								return ;
							}

							try
							{
								m_SocketTcp.EndDisconnect( ar ) ;

								Debug.Log( "<color=#FF00FF>-----------------きちんと切断された</color>" ) ;
							}
							catch( Exception e )
							{
								Debug.Log( "<color=#FF00FF>TCP切断で例外発生 : " + e.Message + "</color>" ) ;
							}
						}, null ) ;
					}
				}
			}

			//-----------------------------------------------------------------------------------------

			// 切断完了待機
			while( m_SocketTcp != null )
			{
				if( cancellationTokenSource != default && cancellationTokenSource.IsCancellationRequested == true )
				{
					// キャンセルトークンによる停止要求が出された
					isCanceled = true ;
					break ;
				}

				if( isDisconectRunning == false )
				{
					// 切断プロセス終了
					break ;
				}

				// わずかにディレイを入れる
				await Task.Yield() ;
			}

			//----------------------------------

			lock( m_DisconnectedTcpLockObject )
			{
				if( m_SocketTcp != null )
				{
					m_SocketTcp.Close() ;
					m_SocketTcp.Dispose() ;
					m_SocketTcp = null ;
				}

				if( cancellationTokenSource != null )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( isCanceled == true )
				{
					// 中断された場合は例外を投げる
					throw new OperationCanceledException() ;
				}

				if( callbackEnabled == true )
				{
					// 切断時のコールバックを呼ぶ

					//-------------------------------

					// コールバックを呼ぶ
					if( m_OnTcpDisconnected != null )
					{
						if( m_MainThreadContext != null )
						{
							if( SynchronizationContext.Current == m_MainThreadContext )
							{
								// 切断コールバックは自発的な切断では呼ばれないようにする
								m_OnTcpDisconnected() ;
							}
							else
							{
								m_MainThreadContext.Post( ( _ ) =>
								{
									// 切断コールバックは自発的な切断では呼ばれないようにする
									m_OnTcpDisconnected() ;
								}, null ) ;
							}
						}
						else
						{
							// 切断コールバックは自発的な切断では呼ばれないようにする
							m_OnTcpDisconnected() ;
						}
					}

					//-------------------------------
				}
			}
		}

		/// <summary>
		/// 破棄する(再利用出来ない)
		/// </summary>
		public void Dispose()
		{
			// 念の為切断も実行する(保険)　※既に切断済みであれば何もしない
			Disconnect( false ) ;

			//----------------------------------

			if( m_SocketUdp != null )
			{
				Debug.Log( "<color=#00FF00>[UDP] ソケットをクローズする</color>" ) ;

				m_SocketUdp.Close() ;
				m_SocketUdp.Dispose() ;
				m_SocketUdp = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＲＣを取得する(継続)
		/// </summary>
		/// <param name="crc"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static uint GetCRC32( byte[] data, int offset, int length )
		{
			int index ;
			int limit = offset + length ;

			uint crc = CRC32_MASK ;

			for( index  = offset ; index <  limit ; index ++ )
			{
				crc = m_CRC32_Table[ ( crc ^ data[ index ] ) & 0xFF ] ^ ( crc >> 8 ) ;
			}

			return crc ;
		}

		//-----------------------------------

		public const uint CRC32_MASK = 0xffffffff ;

		// ＣＲＣテーブル
		private readonly static uint[] m_CRC32_Table = new uint[]
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
			0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
			0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
			0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
			0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
			0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
			0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
			0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
			0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
			0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
			0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
			0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
			0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
			0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
			0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
			0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
			0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
			0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
			0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
			0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
			0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
			0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
			0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
			0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
			0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
			0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
			0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
			0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
			0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
			0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
			0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
			0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
			0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
			0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
			0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
			0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
			0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
			0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
			0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
			0x2d02ef8d
		} ;

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

		/// <summary>
		/// 接続中のアドレスを取得する
		/// </summary>
		/// <returns></returns>
		public string GetConnectingIpAddress()
		{
			string ipAddress = string.Empty ;

			foreach( var ni in NetworkInterface.GetAllNetworkInterfaces() )
			{
				if( ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet )
				{
					foreach( var ip in ni.GetIPProperties().UnicastAddresses )
					{
						if( ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
						{
							ipAddress = ip.Address.ToString() ;
							break ;
						}
					}
				}

				if( string.IsNullOrEmpty( ipAddress ) == false )
				{
					break ;
				}
			}

			return ipAddress ;
		}
	}

	public static class AsyncExtensions
	{
		public static async Task<T> WithCancellation<T>( this Task<T> task, CancellationToken cancellationToken )
		{
			var tcs = new TaskCompletionSource<bool>() ;
			using( cancellationToken.Register( s => ( ( TaskCompletionSource<bool> )s ).TrySetResult( true ), tcs ) )
			{
				if( task != await Task.WhenAny( task, tcs.Task ) )
				{
					throw new OperationCanceledException( cancellationToken ) ;
				}
			}

			return task.Result ;
		}
	}
}
