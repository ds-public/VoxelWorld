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
using System.Runtime.CompilerServices ;

using System.Net ;
using System.Net.Sockets ;

#if UNITY
using UnityEngine ;
#endif


namespace SocketHelper
{
	//--------------------------------------------------------------------------------------------
	// ■以下は個々のクライアントの制御クラス

	/// <summary>
	/// 各接続クライアントを管理するクラス
	/// </summary>
	public class ClientHandler
	{
		/// <summary>
		/// クライアントハンドラーの識別子
		/// </summary>
		public long													Id{ get ; private set ; }

		// ＴＣＰソケットのインスタンス
		private Socket												m_SocketTcp ;

		// ＴＣＰ受信時のコールバック
		private readonly Action<ClientHandler,ReadOnlyMemory<byte>>	m_OnTcpReceived ;

		// ＴＣＰ切断時のコールバック
		private readonly Action<ClientHandler>						m_OnTcpDisconnected ;

		// オーナーのキャンセレーショントークン
		private readonly CancellationToken							m_OwnerCancellationToken ;

		// メインスレッドのコンテキスト
		private readonly SynchronizationContext						m_MainThreadContext ;

		//-----------------------------------------------------------

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

		//-----------------------------------------------------------

		// ＴＣＰ送信用パケット群
		private readonly List<byte[]>								m_TcpSendPackets ;

		// ＴＣＰ送信用バッファ
		private readonly byte[]										m_TcpSendBuffer ;

		// ＴＣＰ受信用バッファ
		private readonly byte[]										m_TcpReceiveBuffer ;

		//-----------------------------------------------------------

		// 切断処理が既に行われているか
		private bool												m_IsClosed ;

		/// <summary>
		/// 既にクライアントハンドラーは閉じられているか
		/// </summary>
		public bool	IsClosed => m_IsClosed ;


		//-----------------------------------------------------------
		// 複製したエンドポイント情報

		/// <summary>
		/// ＩＰｖ４アドレス
		/// </summary>
		public  IPAddress AddressV4 => m_AddressV4 ;
		private readonly IPAddress									m_AddressV4 ;

		/// <summary>
		/// ＩＰｖ６アドレス
		/// </summary>
		public  IPAddress AddressV6 => m_AddressV6 ;
		private readonly IPAddress									m_AddressV6 ;

		/// <summary>
		/// ポート番号
		/// </summary>
		public  int Port => m_Port ;
		private readonly int										m_Port ;

		/// <summary>
		/// エンドポイント
		/// </summary>
		public EndPoint EndPoint => m_EndPoint ;
		private readonly	EndPoint	m_EndPoint ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="socket"></param>
		public ClientHandler
		(
			long										id,
			Socket										socketTcp,
			Action<ClientHandler,ReadOnlyMemory<byte>>	onTcpReceived,
			Action<ClientHandler>						onTcpDisconnected,
			int											maxTcpPacketSize,
			CancellationToken							ownerCancellationToken,
			SynchronizationContext						mainThreadContext
		)
		{
			// 識別子を保持する
			Id											= id ;

			// ソケットを保持する
			m_SocketTcp									= socketTcp ;

			//----------------------------------

			m_OnTcpReceived								= onTcpReceived ;
			m_OnTcpDisconnected							= onTcpDisconnected ;

			m_MaxTcpPacketSize							= maxTcpPacketSize ;

			m_OwnerCancellationToken					= ownerCancellationToken ;

			m_MainThreadContext							= mainThreadContext ;

			//----------------------------------

			// 送信パケット群
			m_TcpSendPackets = new() ;

			// 送信バッファはひとまず最大６４ＫＢ
			m_TcpSendBuffer = new byte[ 4 + 65536 ] ;

			// 受信バッファはひとまず最大６４ＫＢ
			m_TcpReceiveBuffer = new byte[ 65536 ] ;

			//----------------------------------

			// 切断処理が既に行われているか
			m_IsClosed = false ;

			//----------------------------------------------------------
			// 切断後もエンドポイントの情報が取れるように内容を複製する

			var ipEndPoint = ( IPEndPoint )socketTcp.RemoteEndPoint ;

			m_EndPoint = new IPEndPoint( ipEndPoint.Address, ipEndPoint.Port ) ;

			m_AddressV4		= ipEndPoint.Address.MapToIPv4() ;
			m_AddressV6		= ipEndPoint.Address.MapToIPv6() ;
			m_Port			= ipEndPoint.Port ;

			//----------------------------------------------------------
			// KeepAlive を有効化する

			// 参考 https://devlights.hatenablog.com/entry/2023/06/28/073000
			m_SocketTcp.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true ) ;
#if !UNITY
			m_SocketTcp.SetSocketOption( SocketOptionLevel.Tcp,    SocketOptionName.TcpKeepAliveTime, 10 ) ;	// 受信までの待機時間
			m_SocketTcp.SetSocketOption( SocketOptionLevel.Tcp,    SocketOptionName.TcpKeepAliveInterval, 5 ) ;	// 送信間隔
#endif
		}

		/// <summary>
		/// 処理を開始する
		/// </summary>
		public void Start()
		{
			// ＴＣＰ送信パケットバッファをクリアする
			m_TcpSendPackets.Clear() ;

			//----------------------------------

			// ＴＣＰ受信を処理する
			StartTcpReceive() ;
		}

		//-------------------------------------------------------------------------------------------
		// ＴＣＰの受信処理

		private readonly int	m_TcpReceiveHeaderSize = 8 ;
		private readonly byte[]	m_TcpReceiveHeaderData = new byte[ 8 ] ;
		private int				m_TcpReceiveHeaderStep = 0 ;

		private int				m_TcpReceivePacketSize = 0 ;
		private byte[]			m_TcpReceivePacketData = null ;
		private int				m_TcpReceivePacketStep = 0 ;

		// ＴＣＰの受信を開始する
		private void StartTcpReceive()
		{
			// 受信しうる最大サイズでバッファを確保する
			m_TcpReceivePacketData = new byte[ m_MaxTcpPacketSize ] ;

			// 最初の受信待ちを呼ぶ
			m_SocketTcp.BeginReceive( m_TcpReceiveBuffer, 0, m_TcpReceiveBuffer.Length, SocketFlags.None, StartReceiveTcp_Callback, m_SocketTcp ) ;
		}

		// データを受信した際に呼び出される(サブスレッドである事に注意)
		private void StartReceiveTcp_Callback( IAsyncResult ar )
		{
			if( ProcessReceiveTcp( ar ) == false )
			{
				// 切断された
				OnTcpDisconnectedFromClient() ;
			}
		}

		private bool ProcessReceiveTcp( IAsyncResult ar )
		{
			var socketTcp = ( Socket )ar.AsyncState ;

//			Debug.Log( "------> StartTcpReceive_Callback : " + m_EndPoint.ToString(), Color.red ) ;

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
						Debug.Log( "[TCP]受信待ち中に０バイト受信が発生した : EndPoint = " + m_EndPoint.ToString() ) ;

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
						if( m_TcpReceivePacketSize == 0 )
						{
							// パケットサイズが不明

							while( m_TcpReceivePacketSize == 0 && receiveStep <  receiveSize )
							{
								// ヘッダ部分の固定長のデータを取得する
								
								// Math.Min() はメモリを食う場合がある
								requiredSize = GetMinimumValue( m_TcpReceiveHeaderSize - m_TcpReceiveHeaderStep, receiveSize - receiveStep ) ;

								Buffer.BlockCopy( m_TcpReceiveBuffer, receiveStep, m_TcpReceiveHeaderData, m_TcpReceiveHeaderStep, requiredSize ) ;

								receiveStep += requiredSize ;
								m_TcpReceiveHeaderStep  += requiredSize ;

								if( m_TcpReceiveHeaderStep == m_TcpReceiveHeaderSize )
								{
									// サイズ分が溜まった

									for( xor_index  = 0 ;  xor_index <  m_TcpReceiveHeaderSize ; xor_index ++ )
									{
										m_TcpReceiveHeaderData[ xor_index ] ^= xor ;
									}

									crc = ( uint )(
										  m_TcpReceiveHeaderData[ 4 ]         |
										( m_TcpReceiveHeaderData[ 5 ] <<  8 ) |
										( m_TcpReceiveHeaderData[ 6 ] << 16 ) |
										( m_TcpReceiveHeaderData[ 7 ] << 24 ) ) ;

									if( crc != GetCRC32( m_TcpReceiveHeaderData, 0, 4 ) )
									{
										// サイズに異常が見られる

										// 不正アクセスなので強制切断
										return false ;
									}

									m_TcpReceivePacketSize =
										  m_TcpReceiveHeaderData[ 0 ]         |
										( m_TcpReceiveHeaderData[ 1 ] <<  8 ) |
										( m_TcpReceiveHeaderData[ 2 ] << 16 ) |
										( m_TcpReceiveHeaderData[ 3 ] << 24 ) ;

									if( m_TcpReceivePacketSize >  m_MaxTcpPacketSize )
									{
										// 最大サイズを超えている

										// 不正アクセスなので強制切断
										return false ;
									}

									if( m_TcpReceivePacketSize >  0 )
									{
										m_TcpReceivePacketStep = 0 ;
									}
									else
									{
										// もう一度取り直し(ワーニングは出した方が良い)
										m_TcpReceiveHeaderStep = 0 ;
									}
								}
							}
						}

						if( m_TcpReceivePacketSize >  0 && m_TcpReceivePacketStep <  m_TcpReceivePacketSize && receiveStep <  receiveSize )
						{
							// パケットサイズが確定

							// データ部をコピーする

							// Math.Min() はメモリを食う場合がある
							requiredSize = GetMinimumValue( m_TcpReceivePacketSize - m_TcpReceivePacketStep, receiveSize - receiveStep ) ;

							Buffer.BlockCopy( m_TcpReceiveBuffer, receiveStep, m_TcpReceivePacketData, m_TcpReceivePacketStep, requiredSize ) ;

							receiveStep += requiredSize ;
							m_TcpReceivePacketStep  += requiredSize ;

							if( m_TcpReceivePacketStep == m_TcpReceivePacketSize )
							{
//								Debug.Log( "受信パケット完成 EndPoint = " + m_EndPoint.ToString() + " [ " + m_ReceivePacketSize + " ]" ) ;
#if !UNITY
								// コールバックを呼ぶ
								m_OnTcpReceived?.Invoke( new ReadOnlyMemory<byte>( m_TcpReceivePacketData, 0, m_TcpReceivePacketSize ) ) ;
#else
								// コールバックを呼ぶ
								if( m_OnTcpReceived != null )
								{
									if( m_MainThreadContext != null )
									{
										// メインスレッド限定あり呼び出し
										if( SynchronizationContext.Current == m_MainThreadContext )
										{
											m_OnTcpReceived( this, new ReadOnlyMemory<byte>( m_TcpReceivePacketData, 0, m_TcpReceivePacketSize ) ) ;
										}
										else
										{
											// Post 内部が実行されるタイミングは現在のスレッドとコ異なるため受信データの複製(独立化)が必要
											byte[] data = new byte[ m_TcpReceivePacketSize ] ;
											Buffer.BlockCopy( m_TcpReceivePacketData, 0, data, 0, m_TcpReceivePacketSize ) ;

											m_MainThreadContext.Post( ( _ ) =>
											{
												m_OnTcpReceived( this, new ReadOnlyMemory<byte>( data ) ) ;
											}, null ) ;
										}
									}
									else
									{
										// メインスレッド限定なし呼び出し
										m_OnTcpReceived( this, new ReadOnlyMemory<byte>( m_TcpReceivePacketData, 0, m_TcpReceivePacketSize ) ) ;
									}
								}
#endif
								// パケットサイズを０に初期化
								m_TcpReceivePacketSize = 0 ;
								m_TcpReceiveHeaderStep = 0 ;
							}
						}
					}

					// 今回受信したデータは全て処理した

					// 再び受信待ちを呼ぶ
					socketTcp.BeginReceive( m_TcpReceiveBuffer, 0, m_TcpReceiveBuffer.Length, SocketFlags.None, StartReceiveTcp_Callback, socketTcp ) ;
				}
			}
			catch( Exception e )
			{
				Debug.Log( "[TCP]受信コールバック中の例外発生(EndReceive) : " + e.Message ) ;
				return false ;
			}

			// 通信継続
			return true ;
		}

		// 小さい方の値を取得する
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private int GetMinimumValue( int value0, int value1 )
		{
			return value0 <= value1 ? value0 : value1 ;
		}

		//-----------------------------------

		// 送信時のスレッド間の排他制御(複数のスレッドから同時参照があるので排他制御が必要)

		private readonly object m_TcpSendLockObject = new () ;

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
			lock( m_TcpSendLockObject )
			{
				if( m_IsTcpSendRunning == false )
				{
					// 送信中ではない

					m_IsTcpSendRunning = true ;	// 通信中に移行する

					// トータルの送信データサイズ
					int requiredSize = EncodeSendData( tcpPacket, m_TcpSendBuffer ) ;

					m_SocketTcp.BeginSend( m_TcpSendBuffer, 0, requiredSize, SocketFlags.None, SendTdp_Callback, m_SocketTcp ) ;
				}
				else
				{
					// 送信中である

					// 送信バッファ群に積む
					m_TcpSendPackets.Add( tcpPacket ) ;
				}
			}

			return true ;
		}

		// 送信終了時に呼び出される(別スレッドである事に注意する)
		private void SendTdp_Callback( IAsyncResult ar )
		{
			if( ProcessSendTcp( ar ) == false )
			{
				// 切断された
				OnTcpDisconnectedFromClient() ;
			}
		}

		private bool ProcessSendTcp( IAsyncResult ar )
		{
			var socketTcp = ( Socket )ar.AsyncState ;

			try
			{
				if( socketTcp.Connected == true )
				{
					int sendSize = m_SocketTcp.EndSend( ar ) ;
					if( sendSize >  0 )
					{
						// サブスレッドの排他制御
						lock( m_TcpSendLockObject )
						{
							if( m_TcpSendPackets.Count >  0 )
							{
								// パケットバッファにパケットが溜まっている

								// パケットを取り出す
								var tcpPacket = m_TcpSendPackets[ 0 ] ;
								m_TcpSendPackets.RemoveAt( 0 ) ;

								//------------------------------

								// トータルの送信データサイズ
								int requiredSize = EncodeSendData( tcpPacket, m_TcpSendBuffer ) ;

								// 再び送信を実行する
								socketTcp.BeginSend( m_TcpSendBuffer, 0, requiredSize, SocketFlags.None, SendTdp_Callback, socketTcp ) ;
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
			Buffer.BlockCopy( tcpPacket, 0, tcpBuffer, 8, packetSize ) ;

			return 8 + packetSize ;
		}

		//-------------------------------------------------------------------------------------------

		private readonly object   m_DisconnectedTcpLockObject = new () ;

		// クライアントよって切断された際に呼び出される
		private void OnTcpDisconnectedFromClient()
		{
			Debug.Log( "クライアントから切断されたと認識 : " + m_EndPoint.ToString() ) ;

			// 切断の排他制御(複数のスレッドから実行される可能性がある)
			lock( m_DisconnectedTcpLockObject )
			{
				if( m_SocketTcp != null )
				{
					m_SocketTcp.Close() ;
					m_SocketTcp.Dispose() ;
					m_SocketTcp = null ;
				}

				//----------------------------------------------------------

				if( m_IsClosed == false )
				{
					m_IsClosed  = true ;

					// 切断コールバック(※ServerSocket 側でメインスレッド限定化処理が施されるためメインスレッド限定化処理をここで行う必要は無い)
					m_OnTcpDisconnected?.Invoke( this ) ;
				}
			}
		}

		/// <summary>
		/// 切断を実行する
		/// </summary>
		public void Disconnect( bool callbakEnabled )
		{
			// 切断の排他制御(複数のスレッドから実行される可能性がある)
			lock( m_DisconnectedTcpLockObject )
			{
				if( m_SocketTcp != null )
				{
					Debug.Log( "能動的にサーバーと接続しているソケットを切断する" ) ;

					if( m_SocketTcp.Connected == true )
					{
						// 接続しているならブロッキングせずに動作する
						m_SocketTcp.Shutdown( SocketShutdown.Both ) ;
					}

					Debug.Log( "ソケットをクローズする : callbackEnabled = " + callbakEnabled ) ;

					m_SocketTcp.Close() ;
					m_SocketTcp.Dispose() ;
					m_SocketTcp = null ;
				}

				//----------------------------------------------------------

				if( m_IsClosed == false )
				{
					m_IsClosed  = true ;

					// 切断コールバックはデフォルトでは呼ばれないようにする
					if( callbakEnabled == true )
					{
						Debug.Log( "[ClientHandler] OnTcpDisconnected コールバックを呼ぶ : " + m_OnTcpDisconnected ) ;

						// 切断コールバック(※ServerSocket 側でメインスレッド限定化処理が施されるためメインスレッド限定化処理をここで行う必要は無い)
						m_OnTcpDisconnected?.Invoke( this ) ;
					}
				}
			}
		}

		/// <summary>
		/// 破棄する
		/// </summary>
		public void Dispose()
		{
			// 念の為切断も実行する(保険)　※既に切断済みであれば何もしない
			Disconnect( false ) ;
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
	}
}
