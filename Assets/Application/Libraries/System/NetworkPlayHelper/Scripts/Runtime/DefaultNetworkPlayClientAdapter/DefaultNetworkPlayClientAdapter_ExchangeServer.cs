using System ;
using System.Collections.Generic ;
using System.Linq ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using SocketHelper ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 標準のアダプター
	/// </summary>
	public partial class DefaultNetworkPlayClientAdapter : INetworkPlayClientAdapter
	{
		// エクスチェンジサーバーのアドレス
		private string		m_ExchangeServerAddress ;

		/// <summary>
		/// エクスチェンジサーバーのアドレス
		/// </summary>
		public	string		ExchangeServerAddress	=> m_ExchangeServerAddress ;

		// エクスチェンジサーバーのＴＣＰポート
		private int			m_ExchangeServerTcpPort ;

		/// <summary>
		/// エクスチェンジサーバーのＴＣＰポート
		/// </summary>
		public	int			ExchangeServerTcpPort	=> m_ExchangeServerTcpPort ;

		// エクスチェンジサーバーのＵＤＰポート
		private int			m_ExchangeServerUdpPort ;

		/// <summary>
		/// エクスチェンジサーバーのＵＤＰポート
		/// </summary>
		public	int			ExchangeServerUdpPort	=> m_ExchangeServerUdpPort ;

		//-----------------------------------

		// セッションに加わっているかどうか
		private bool        m_IsSessionJoined ;

		/// <summary>
		/// セッションに加わっているかどうか
		/// </summary>
		public bool     IsSessionJoined
		{
			get
			{
				return m_IsSessionJoined ;
			}
		}

		// セッション識別子
		private uint		m_SessionId ;

		/// <summary>
		/// セッション識別子(Abstruct)
		/// </summary>
		public ulong	SessionId
		{
			get
			{
				return m_SessionId ;
			}
		}

		// セッションの最大プレイヤー数
		private int			m_MaxPlayers ;

		/// <summary>
		/// セッションの最大プレイヤー数
		/// </summary>
		public int		MaxPlayers
		{
			get
			{
				return m_MaxPlayers ;
			}
		}

		//-----------------------------------------------------------

		// セッションの管理方法(ホスト集中かサーバー集中か)
		private SessionManagementTypes	m_ManagementType ;

		/// <summary>
		/// セッションの管理方法[ホスト集中かサーバー集中か](Abstruct)
		/// </summary>
		public SessionManagementTypes ManagementType
		{
			get
			{
				return m_ManagementType ;
			}
		}

		// セッション内通信でＵＤＰを有効にするかどうか
		private bool					m_UdpEnabled ;

		/// <summary>
		/// セッション内通信でＵＤＰを有効にするかどうか(Abstruct)
		/// </summary>
		public bool UdpEnabled
		{
			get
			{
				return m_UdpEnabled ;
			}
		}

		// セッション内通信でＵＤＰの補正機能を有効にするかどうか
		private bool					m_UdpCorrectionEnabled ;

		/// <summary>
		/// セッション内通信でＵＤＰの補正機能を有効にするかどうか(Abstruct)
		/// </summary>
		public bool UdpCorrectionEnabled
		{
			get
			{
				return m_UdpCorrectionEnabled ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// セッション通信用のソケットクライアント
		private SocketClient			m_RealTimeSocketClient ;

		// セッションサーバーと通信時のタスクキャンセル用
		private CancellationTokenSource m_CancellationTokenSource_ForExchangeServer ;


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// クライアントの状態
		/// </summary>
		public enum ClientPhases
		{
			/// <summary>
			/// 初期状態
			/// </summary>
			None			= 0,

			/// <summary>
			/// バインド要求中
			/// </summary>
			RequestBinding	= 1,

			/// <summary>
			/// 接続状態継続中
			/// </summary>
			Connecting		= 2,

			/// <summary>
			/// フレームの通信可能な状態
			/// </summary>
			Ready			= 3,

			/// <summary>
			/// 切断状態継続中
			/// </summary>
			Disconnecting	= 9,
		}

		// クライアントの状態
		private ClientPhases	m_ClientPhase = ClientPhases.None ;

		/// <summary>
		/// セッションプレイヤー情報
		/// </summary>
		public class SessionPlayerData
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string	UserId { get ; private set ; }

			/// <summary>
			/// ユーザー名
			/// </summary>
			public string	Name { get ; private set ; }

			/// <summary>
			/// ゲストアカウントであるかどうか
			/// </summary>
			public bool		IsGuest { get ; private set ; }

			/// <summary>
			/// セッションのホストかどうか
			/// </summary>
			public bool		IsHost ;

			//----------------------------------

			/// <summary>
			/// バイト配列から情報をデコードする
			/// </summary>
			/// <param name="data"></param>
			/// <param name="offset"></param>
			public void Decode( byte[] data, ref int offset )
			{
				try
				{
					UserId		= DataFormat.GetString( data, ref offset ) ;
					Name		= DataFormat.GetString( data, ref offset ) ;
					IsGuest		= DataFormat.GetBool( data, ref offset ) ;
					IsHost		= DataFormat.GetBool( data, ref offset ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// セッション内のプレイヤー情報群
		private List<SessionPlayerData>				m_SessionPlayers ;

		//-----------------------------------------------------------

		// 受信コールバックタイプ
		private ReceivingCallbackTypes				m_ReceivingCallbackType = ReceivingCallbackTypes.Passive ;

		/// <summary>
		/// 受信フレーム
		/// </summary>
		public class ActiveFrame
		{
			public byte[]		Data ;
			public SourceTypes	SourceType ;
			public string		SourceUserId ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			/// <param name="sourceTypes"></param>
			/// <param name="sourceUserId"></param>
			public ActiveFrame( byte[] data, SourceTypes sourceTypes, string sourceUserId )
			{
				Data			= data ;
				SourceType		= sourceTypes ;
				SourceUserId	= sourceUserId ;
			}
		}

		// 受信フレーム群
		private readonly List<ActiveFrame>			m_ActiveFrames = new () ;

		// 受信フレーム群の排他制御用オブジェクト
		private readonly object						m_ActiveFrames_LockObject = new () ;

		//---------------

		// 受信コールバックタイプ
		private ReceivingCallbackTypes				m_ReceivingCallbackType_ForSessionProcessor = ReceivingCallbackTypes.Passive ;



		/// <summary>
		/// 受信フレーム
		/// </summary>
		public class ActiveFrame_ForSessionProcessor
		{
			public byte[]		Data ;
			public string		SourceUserId ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			/// <param name="sourceTypes"></param>
			/// <param name="sourceUserId"></param>
			public ActiveFrame_ForSessionProcessor( byte[] data, string sourceUserId )
			{
				Data			= data ;
				SourceUserId	= sourceUserId ;
			}
		}

		// 受信フレーム群
		private readonly List<ActiveFrame_ForSessionProcessor>		m_ActiveFrames_ForSessionProcessor = new () ;

		// 受信フレーム群の排他制御用オブジェクト
		private readonly object										m_ActiveFrames_ForSessionProcessor_LockObject = new () ;
		
		//-----------------------------------

		// タイマー
		private long								m_LastSendTime ;

		//-----------------------------------

		// 送信済みフレーム(バックアップ)
		private List<UpstreamFrameData>				m_SendingFrames ;

		// 送信フレームのシーケンス番号
		private ushort								m_SendingFrameSequence ;

		//----------------------------------

		// 受信済みフレーム
		private List<DownstreamFrameData>			m_ReceivingFrames ;

		// 受信処理中のフレームの処理可能フレームシーケンス番号
		private ushort								m_ReceivingFrameSequence ;

		// 並び変わりが発生しているか
		private bool								m_Reordering ;

		// 既に再送要求を送っているか
		private bool								m_RetransmissionSending ;

		// 再送要求を送っているフレームのシーケンス
		private ushort								m_RetransmissionSendingFrameSequence ;

		// 再送要求までの計測開始時間
		private long								m_RetransmissionBaseTime ; 

		//-----------------------------------------------------------
		// 受信時に呼び出すコールバック

		// 接続した際に呼び出される
		private Action								m_OnConnected ;

		// 通常のフレームを受信した際に呼び出されるコールバック
		private Action<byte[],SourceTypes,string>	m_OnReceived ;

		// プレイヤーが参加した際に呼び出される
		private Action<SessionPlayer>				m_OnPlayerJoined ;

		// プレイヤーが離脱した際に呼び出される
		private Action<SessionPlayer>				m_OnPlayerLeft ;

		// 切断された際に呼び出される
		private Action								m_OnDisconnected ;

		//-----------------------------------------------------------

		/// <summary>
		/// フレームの通信準備が整っているかどうか
		/// </summary>
		public bool Ready
		{
			get
			{
				return ( m_ClientPhase == ClientPhases.Ready ) ;
			}
		}

		/// <summary>
		/// 自身がホストであるかどうか
		/// </summary>
		public bool IsHost
		{
			get
			{
				if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
				{
					// 現状判定不可能な状況
					return false ;
				}

				var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.IsHost == true ) ;
				if( sessionPlayer == null )
				{
					// ホストとなっているプレイヤーが存在しない(異常)
					Debug.LogWarning( "セッションのホストとなっているプレイヤーが存在いない" ) ;
					return false ;
				}

				// 自身がホストであるかどうか
				return ( sessionPlayer.UserId == m_UserId ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// KeepAlive を使用するかどうか
		/// </summary>
		public bool			UseKeepAlive = true ;


		//-----------------------------------------------------------

		/// <summary>
		/// Ping 計測を行うかどうか
		/// </summary>
		public bool			UsePing { get; set ; } = true ;

		/// <summary>
		/// Ping のパケットタイプ(セッションが TCP のみの場合は UDP は指定できない)
		/// </summary>
		public PacketTypes	PingPacketType { get ; set ; } = PacketTypes.UDP ;

		//---------------

		/// <summary>
		/// サーバー宛のPing最小値[ms]
		/// </summary>
		public long		PingToServer_Min { get ; private set ; }

		/// <summary>
		/// サーバー宛のPing平均値[ms]
		/// </summary>
		public long		PingToServer_Avarage
		{
			get
			{
				if( m_PingToServer_Count <= 0 )
				{
					return 0 ;
				}

				return ( long )( ( ( double )m_PingToServer_Total / ( double )m_PingToServer_Count ) + 0.5 ) ;
			}
		}

		// サーバー宛のPingの平均値を出すためのトータル
		private long	m_PingToServer_Total ;

		// サーバー宛のPingの平均値を出すためのカウント
		private long	m_PingToServer_Count ;

		/// <summary>
		/// サーバー宛のPing最終値[ms]
		/// </summary>
		public long		PingToServer { get ; private set ; }

		/// <summary>
		/// サーバー宛のPing最大値[ms]
		/// </summary>
		public long		PingToServer_Max  { get ; private set ; }

		//---------------

		/// <summary>
		/// ホスト宛のPing最小値[ms]
		/// </summary>
		public long		PingToHost_Min { get ; private set ; }

		/// <summary>
		/// ホスト宛のPing平均値[ms]
		/// </summary>
		public long		PingToHost_Avarage
		{
			get
			{
				if( m_PingToHost_Count <= 0 )
				{
					return 0 ;
				}

				return ( long )( ( ( double )m_PingToHost_Total / ( double )m_PingToHost_Count ) + 0.5 ) ;
			}
		}

		// ホスト宛のPingの平均値を出すためのトータル
		private long	m_PingToHost_Total ;

		// ホスト宛のPingの平均値を出すためのカウント
		private long	m_PingToHost_Count ;

		/// <summary>
		/// ホスト宛のPing最終値[ms]
		/// </summary>
		public long		PingToHost { get ; private set ; }

		/// <summary>
		/// サーバー宛のPing最大値[ms]
		/// </summary>
		public long		PingToHost_Max  { get ; private set ; }


		//-----------------------------------------------------------

		// ローカルループバックを有効にするかどうか
		private bool m_LocalLoopbackEnabled = true ;

		/// <summary>
		/// ローカルループバックを有効にするかどうか
		/// </summary>
		public bool		LocalLoopbackEnabled
		{
			get
			{
				return m_LocalLoopbackEnabled ;
			}
			set
			{
				m_LocalLoopbackEnabled = value ;
			}
		}

		/// <summary>
		/// プレイヤー名
		/// </summary>
		public string	PlayerName
		{
			get
			{
				if( m_IsSessionJoined == false )
				{
					return UserName ;
				}
				else
				{
					var sessionPlayer = GetSessionPlayer( null ) ;
					if( sessionPlayer != null )
					{
						return sessionPlayer.Name ;
					}
					else
					{
						return "Unknown" ;
					}
				}
			}
		}


		//-------------------------------------------------------------------------------------------
		// インターフェースのメソッド群(実装必須)

		/// <summary>
		/// セッション内プレイヤー情報群を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer[] GetSessionPlayers()
		{
			if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
			{
				return null ;
			}

			var sessionPlayers = new List<SessionPlayer>() ;

			foreach( var sessionPlayer in m_SessionPlayers )
			{
				sessionPlayers.Add( new SessionPlayer( sessionPlayer.UserId, sessionPlayer.Name, sessionPlayer.IsGuest, sessionPlayer.IsHost ) ) ;
			}

			return sessionPlayers.ToArray() ;
		}

		/// <summary>
		/// セッション内のホストプレイヤーの情報を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer GetSessionHostPlayer()
		{
			if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
			{
				return null ;
			}

			var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.IsHost == true ) ;
			if( sessionPlayer == null )
			{
				return null ;
			}

			return new ( sessionPlayer.UserId, sessionPlayer.Name, sessionPlayer.IsGuest, sessionPlayer.IsHost ) ;
		}

		/// <summary>
		/// 指定したユーザー識別子のセッション内プレイヤー情報を取得する
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public SessionPlayer GetSessionPlayer( string userId )
		{
			if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
			{
				return null ;
			}

			if( string.IsNullOrEmpty( userId ) == true )
			{
				userId = UserId ;
			}

			var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.UserId == userId ) ;
			if( sessionPlayer == null )
			{
				return null ;
			}

			return new ( sessionPlayer.UserId, sessionPlayer.Name, sessionPlayer.IsGuest, sessionPlayer.IsHost ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext )
		{
			m_ReceivingCallbackType	= receivingCallbackType ;
			m_MainThreadContext		= mainThreadContext ;
		}

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue()
		{
			byte[] data ;
			SourceTypes sourceType ;
			string sourceUserId ;
			int count ;

			//----------------------------------------------------------

			// 排他制御
			lock( m_ActiveFrames_LockObject )
			{
				if( m_ActiveFrames.Count == 0 )
				{
					return 0 ;
				}

				//-------------

				// 取り出してコールバックを実行する
				var activeFrame = m_ActiveFrames[ 0 ] ;
				m_ActiveFrames.RemoveAt( 0 ) ;

				//-------------

				data			= activeFrame.Data ;
				sourceType		= activeFrame.SourceType ;
				sourceUserId	= activeFrame.SourceUserId ;

				count			= m_ActiveFrames.Count ;
			}

			//----------------------------------

			var context = SynchronizationContext.Current ;

			if( m_MainThreadContext != null )
			{
				// メインスレッドによる縛り：あり

				if( context == m_MainThreadContext )
				{
					// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//					Debug.Log( "<color=#FFFF00>Dequeue はメインスレッドで呼ばれた</color>" ) ;
					CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>Dequeue はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし
//				Debug.Log( "<color=#FFFF00>Dequeue はスレッド縛りなしで呼ばれた</color>" ) ;
				CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
			}

			void CallOnReceivedInMainThred( byte[] data, SourceTypes sourceType, string sourceUserId )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_OnReceived?.Invoke( data, sourceType, sourceUserId ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}

			//----------------------------------------------------------

			// 残りのキューに蓄積されたフレームの数を返す
			return count ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType_ForSessionProcessor( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext )
		{
			m_ReceivingCallbackType_ForSessionProcessor = receivingCallbackType ;
			m_MainThreadContext_ForSessionProcessor		= mainThreadContext ;
		}

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue_ForSessionProcessor()
		{
			byte[] data ;
			string sourceUserId ;
			int count ;

			//----------------------------------------------------------

			// 排他制御
			lock( m_ActiveFrames_ForSessionProcessor_LockObject )
			{
				if( m_ActiveFrames_ForSessionProcessor.Count == 0 )
				{
					return 0 ;
				}

				//-------------

				// 取り出してコールバックを実行する
				var activeFrame = m_ActiveFrames_ForSessionProcessor[ 0 ] ;
				m_ActiveFrames_ForSessionProcessor.RemoveAt( 0 ) ;

				//-------------

				data			= activeFrame.Data ;
				sourceUserId	= activeFrame.SourceUserId ;

				count			= m_ActiveFrames_ForSessionProcessor.Count ;
			}

			//----------------------------------

			if( m_MainThreadContext_ForSessionProcessor != null )
			{
				// メインスレッドの縛り：あり

				if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
				{
					// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//					Debug.Log( "<color=#FFFF00>Dequeue_ForSessionProcessor はメインスレッドで呼ばれた</color>" ) ;
					CallOnReceivedInSessionProcessorInMainThread( data, sourceUserId ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>Dequeue_ForSessionProcessor はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnReceivedInSessionProcessorInMainThread( data, sourceUserId ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドの縛り：なし

				CallOnReceivedInSessionProcessorInMainThread( data, sourceUserId ) ;
			}

			void CallOnReceivedInSessionProcessorInMainThread( byte[] data, string sourceUserId )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_SessionProcessor?.OnReceived( data, sourceUserId ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}

			//----------------------------------------------------------

			// 残りのキューに蓄積されたフレームの数を返す
			return count ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// データを送信する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="destinationType"></param>
		/// <param name="destinationUserIds"></param>
		/// <returns></returns>
		public bool Send
		(
			byte[] data,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
		{
			// パケットタイプ
			var packetType = m_UdpEnabled == false ? PacketTypes.TCP : PacketTypes.UDP ;

			return Send( data, packetType, destinationType, destinationUserIds ) ;
		}

		/// <summary>
		/// データを送信する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="destinationType"></param>
		/// <param name="destinationUserIds"></param>
		/// <returns></returns>
		public bool Send
		(
			byte[] data,
			PacketTypes packetType,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
		{
//			Debug.Log( "<color=#FF7F00>送信するパケットのタイプ = " + packetType + " 送信先タイプ = " + destinationType + "</color>" ) ;

			if( m_RealTimeSocketClient == null )
			{
				// サーバーに接続されていない
				Debug.LogWarning( "サーバーに接続されていない状態です" ) ;
				return false ;
			}

			if( m_ClientPhase != ClientPhases.Ready )
			{
				// 送信可能な状態になっていない
				return false ;
			}

			if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
			{
				// 送信可能な対象が存在しない
				return false ;
			}

			//----------------------------------------------------------

			if( data == null || data.Length == 0 )
			{
				return false ;
			}

			//----------------------------------------------------------

			// パケットタイプ
			if( packetType == PacketTypes.UDP && m_UdpEnabled == false )
			{
				// ＵＤＰは使用不可
				packetType = PacketTypes.TCP ;
			}

			//----------------------------------------------------------
			// ローカルループバック処理

			if( m_LocalLoopbackEnabled == true )
			{
				// サーバーの負荷を減らすためにローカルで可能な通信はローカルで行い余計な通信を行わないようにする

				if( destinationType == DestinationTypes.Broadcast )
				{
					if( m_ManagementType == SessionManagementTypes.ServerManagement )
					{
						// サーバー管理

						if( m_SessionPlayers.Count == 1 )
						{
							// セッション内にはプレイヤーが１人しかいない

							// セッション内の唯一のプレイヤーが自分自身か確認する
							if( m_SessionPlayers[ 0 ].UserId != m_UserId )
							{
								// 自分自身では無い状態は異常
								return false ;
							}

//							Debug.Log( "<color=#FFFF00>[Broadcast] 完全なローカルループバックを行う</color>" ) ;
							CallOnReceived( data, SourceTypes.FromClient, m_UserId ) ;

							return true ;
						}
					}
					else
					{
						// ホスト管理
						if( IsHost == true )
						{
							// 自身がホストである場合は最初から転送アップフレームを送信する
							return SendReceivingFrame_FromSessionProcessor_Private
							(
								packetType,
								data,
								destinationType,
								null,
								SourceTypes.FromClient,
								m_UserId
							) ;
						}
					}
				}
				else
				if( destinationType == DestinationTypes.Multicast )
				{
					if( m_ManagementType == SessionManagementTypes.ServerManagement )
					{
						// サーバー管理

						// 送信先の指定の確認
						if( destinationUserIds == null || destinationUserIds.Length == 0 )
						{
							// 送信先の指定が異常
							return false ;
						}

						if( destinationUserIds.Length == 1 )
						{
							// 送信先が１人だけ

							if( destinationUserIds[ 0 ] == m_UserId )
							{
//								Debug.Log( "<color=#FFFF00>[Multicast] 完全なローカルループバックを行う</color>" ) ;
								CallOnReceived( data, SourceTypes.FromClient, m_UserId ) ;

								return true ;
							}
						}
					}
					else
					{
						// ホスト管理
						if( IsHost == true )
						{
							// 自身がホストである場合は最初から転送アップフレームを送信する
							return SendReceivingFrame_FromSessionProcessor_Private
							(
								packetType,
								data,
								destinationType,
								destinationUserIds,
								SourceTypes.FromClient,
								m_UserId
							) ;
						}
					}
				}
				else
				if( destinationType == DestinationTypes.Unicast )
				{
					// 送信先の指定の確認
					if( destinationUserIds == null || destinationUserIds.Length == 0 )
					{
						// 送信先の指定が異常
						return false ;
					}

					// 送信先は１人だけのはず(複数指定してもエラーにはしない)

					if( destinationUserIds[ 0 ] == m_UserId )
					{
//						Debug.Log( "<color=#FFFF00>[Unicast] 完全なローカルループバックを行う</color>" ) ;
						CallOnReceived( data, SourceTypes.FromClient, m_UserId ) ;

						return true ;
					}
				}
				else
				if( destinationType == DestinationTypes.ToHost )
				{
					// ホストを限定対象としたフレーム

					if( IsHost == true )
					{
						// 自身はホスト

//						Debug.Log( "<color=#FFFF00>[ToHost] 完全なローカルループバックを行う</color>" ) ;
						CallOnReceived( data, SourceTypes.FromClient, m_UserId ) ;

						return true ;
					}
				}
				else
				if( destinationType == DestinationTypes.ToServer )
				{
					if( m_ManagementType == SessionManagementTypes.HostManagement && IsHost == true )
					{
						// ホスト管理かつ自身はホスト

//						Debug.Log( "<color=#FFFF00>[ToServer] 完全なローカルループバックを行う</color>" ) ;

						if( m_SessionProcessor != null )
						{
							// カスタムセッションプロセッサーが設定されている
							CallOnReceived_ForSessionProcessor( data, m_UserId ) ;
						}

						return true ;
					}
				}
			}

			//----------------------------------------------------------
			// フレームデータを生成・送信・蓄積する

			// フレームのバックアップ用バッファへの蓄積とフレームの再送要求がかち合うので排他制御が必要
			// 自身の送信とホスト管理としての送信が存在するのでそちらの排他制御も必要
			lock( m_SendingFrameLockObject )
			{
				//----------------------------------
				// リオーダーのテスト
/*
				// 意図的にシーケンスの順番を入れ替える
				ushort sequence = m_SendingFrameSequence ;

				if( sequence == 2 )
				{
					sequence  = 4 ;
				}
				else
				if( sequence == 3 )
				{
					sequence  = 5 ;
				}
				else
				if( sequence == 4 )
				{
					sequence  = 3 ;
				}
				else
				if( sequence == 5 )
				{
					sequence  = 2 ;
				}

				var frame = new UpstreamFrameData
				(
					sequence,
					false,
					destinationType,
					destinationUserIds,
					SourceTypes.FromClient,	// 通常アップでは意味無し
					null,					// 通常アップでは意味無し
					data
				) ;
*/
				//----------------------------------

				// 通常アップの上りフレームを生成する
				var frame = new UpstreamFrameData
				(
					( packetType == PacketTypes.UDP && m_UdpCorrectionEnabled == true ) ? m_SendingFrameSequence : ( ushort )0,
					false,	// 通常アップ(ここでは通常アップ以外はありえない)
					destinationType,
					destinationUserIds,
					SourceTypes.FromClient,	// 通常アップでは意味無し
					null,					// 通常アップでは意味無し
					data
				) ;

				// 特定のフレームのみロスト扱いにする(デバッグ)
/*				if( m_SendingFrameSequence != 2 && m_SendingFrameSequence != 3 )
				{*/
					// フレームを送信する
					SendFrame_Private( frame, packetType ) ;
/*				}*/

//				Debug.Log( "<color=#7FFF7F>フレーム送信 シーケンス = " + m_SendingFrameSequence + "</color>" ) ;

				//--------------------------------------------------------------------------

				if( packetType == PacketTypes.UDP && m_UdpCorrectionEnabled == true )
				{
					// フレームをバックアップ用送信バッファに貯める
					AddSendingFrame( frame ) ;

					//----------------------------------------------------------

					// シーケンス番号増加
					m_SendingFrameSequence ++ ;
				}

				//----------------------------------------------------------

				// 最後に送信時間を更新する
				m_LastSendTime = Timer.NowTicks ;
			}

			//--------------------------------------------------------------------------

			// 成功
			return true ;
		}

		/// <summary>
		/// 対象プレイヤーをキックする(ホストのみ可能)
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool Kick( string userId )
		{
			if( IsHost == false )
			{
				// ホスト以外は使用不可
				return false ;
			}

			if( m_RealTimeSocketClient == null )
			{
				// サーバーに接続されていない
				Debug.LogWarning( "サーバーに接続されていない状態です" ) ;
				return false ;
			}

			if( m_ClientPhase != ClientPhases.Ready )
			{
				// 送信可能な状態になっていない
				return false ;
			}

			if( m_SessionPlayers == null || m_SessionPlayers.Count == 0 )
			{
				// 送信可能な対象が存在しない
				return false ;
			}

			//----------------------------------------------------------

			SendKick( userId ) ;

			return true ;
		}

		//---------------

		/// <summary>
		/// 自身がセッションに参加した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onDisconnected"></param>
		public void SetOnConnected( Action onConnected )
		{
			m_OnConnected = onConnected ;
		}

		/// <summary>
		/// データを受信した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnReceived
		(
			Action<byte[],SourceTypes,string> onReceived
		)
		{
			m_OnReceived		= onReceived ;
		}

		/// <summary>
		/// セッション内プレイヤーの参加と離脱が行われた際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPlayerJoined"></param>
		/// <param name="onPlayerLeft"></param>
		public void SetOnPlyerChanged
		(
			Action<SessionPlayer> onPlayerJoined,
			Action<SessionPlayer> onPlayerLeft
		)
		{
			m_OnPlayerJoined	= onPlayerJoined ;
			m_OnPlayerLeft		= onPlayerLeft ;
		}

		/// <summary>
		/// 自身がセッションから離脱した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onDisconnected"></param>
		public void SetOnDisconnected( Action onDisconnected )
		{
			m_OnDisconnected = onDisconnected ;
		}

		//---------------

		/// <summary>
		/// セッションから離脱する
		/// </summary>
		public void LeaveFromSession()
		{
			if( m_IsSessionJoined == false )
			{
				return ;
			}

			if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
			{
				Debug.Log( "クライアントによる明示的切断(1)" ) ;
				m_ClientPhase  = ClientPhases.Disconnecting ;
			}
		}


		//-------------------------------------------------------------------------------------------------------------------

		// リアルタイム通信用のソケットクライアントを生成する
		private void CreateRealTimeSocketClient( CancellationToken ownerCancellationToken )
		{
			if( m_RealTimeSocketClient != null )
			{
				// 既に生成済み
				return ;
			}

			//----------------------------------------------------------
			// タスク中断用のキャンセレーショントークン生成

			if( ownerCancellationToken == default )
			{
				m_CancellationTokenSource_ForExchangeServer = new CancellationTokenSource() ;
			}
			else
			{
				m_CancellationTokenSource_ForExchangeServer = CancellationTokenSource.CreateLinkedTokenSource( ownerCancellationToken ) ;
			}

			//----------------------------------------------------------

			// リアルタイム通信用のクライアントソケット生成
			m_RealTimeSocketClient = new SocketClient
			(
				OnTcpReceived_FromSessonServer,
				OnTcpDeiconnected_FromSessionServer,
				OnUdpReceived_FromSessionServer,
				m_MaxTcpPacketSize,
				m_CancellationTokenSource_ForExchangeServer.Token
			) ;

			//----------------------------------------------------------

			// クライアントの状態を初期状態に初期化
			m_ClientPhase = ClientPhases.None ;

			// セッション内のプレイヤー情報群
			m_SessionPlayers = new () ;

			// 送信済みフレームのバックアップ
			m_SendingFrames = new () ;

			// 受信済みフレーム
			m_ReceivingFrames = new () ;

			// 受信中のフレームの処理可能シーケンス番号
			m_ReceivingFrameSequence = 0 ;

			// 並び代わりが発生しているか
			m_Reordering = false ;

			// 既に再送要求を送っているか
			m_RetransmissionSending = false ;
		}

		// エンスチェンジサーバーにＴＣＰで接続する
		private async Task<bool> ConnectToExchangeServer
		(
			string exchangeServerAddress,
			int exchangeServerPort,
			CancellationToken cancellationToken
		)
		{
			if( m_RealTimeSocketClient == null )
			{
				// リアルタイム通信用のソケットクライアントが生成されていない
				return false ;
			}

			//----------------------------------------------------------
			// Ping 計測用の値を初期化する

			PingToServer_Min		= 0 ;
			m_PingToServer_Total	= 0 ;
			m_PingToServer_Count	= 0 ;
			PingToServer_Max		= 0 ;

			PingToHost_Min			= 0 ;
			m_PingToHost_Total		= 0 ;
			m_PingToHost_Count		= 0 ;
			PingToHost_Max			= 0 ;

			//----------------------------------------------------------
			// タスク中断用のキャンセレーショントークン生成

			CancellationTokenSource cancellationTokenSource ;
			bool isCanceled ;

			//----------------------------------------------------------
			// エクスチェンジサーバーへの接続を行う

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForExchangeServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForExchangeServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			try
			{
				// エクスチェンジサーバーへＴＣＰ接続を行う(接続実行と受信開始)
				await m_RealTimeSocketClient.ConnectAsync( exchangeServerAddress, exchangeServerPort, null, cancellationTokenSource.Token ) ;
			}
			catch( Exception e )
			{
				if( e is OperationCanceledException )
				{
					isCanceled = true ;
				}
				else
				{
					Debug.LogError( e.Message ) ;
				}
			}

			// 新規にキャンセレーショントークンソースを生成してれば破棄する
			if( cancellationToken != default )
			{
				cancellationTokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// キャンセルされていたらキャンセル例外を発行する
				throw new OperationCanceledException() ;
			}

			Debug.Log( "<color=#FFFF00>無事に ExchangeServer に接続 : クライアント側のポート番号 = " + m_RealTimeSocketClient.GetTcpPort() + "</color>" ) ;

			//----------------------------------------------------------

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForExchangeServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForExchangeServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			//------------------------------------------------------------------------------------------

//			Debug.Log( "-------状態 : " + m_ClientPhase ) ;

			// バインド要求を出している最中(送信前に状態を変える事)
			m_ClientPhase = ClientPhases.RequestBinding ;

			// クライアントとセッションプレイヤーのバインド要求を送る
			SendBindClientToSessionPlayer() ;

			while( m_RealTimeSocketClient != null )
			{
				if( m_RealTimeSocketClient.GetSendingTcpPacketCount() == 0 )
				{
					// 送信完了
					break ;
				}

				if( cancellationTokenSource.IsCancellationRequested == true )
				{
					isCanceled = true ;
					break ;
				}

				await Task.Yield() ;
			}

			// 新規にキャンセレーショントークンソースを生成してれば破棄する
			if( cancellationToken != default )
			{
				cancellationTokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// キャンセルされていたらキャンセル例外を発行する
				throw new OperationCanceledException() ;
			}

			Debug.Log( "<color=#FFFF00>ExchangeServer にバインド要求を送信した</color>" ) ;

			//----------------------------------------------------------
			// 応答としてのセッションプレイヤー情報受信を待つ

			//----------------------------------------------------------

			// バインドが完了するまで待機する

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForExchangeServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForExchangeServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForExchangeServer != null )
			{
				if( m_CancellationTokenSource_ForExchangeServer.IsCancellationRequested == true )
				{
					isCanceled = true ;
					break ;
				}

				if( m_ClientPhase != ClientPhases.RequestBinding )
				{
					// 待機終了
					break ;
				}

				await Task.Yield() ;
			}

			// 新規にキャンセレーショントークンソースを生成してれば破棄する
			if( cancellationToken != default )
			{
				cancellationTokenSource.Dispose() ;
			}

			if( m_ClientPhase == ClientPhases.Disconnecting )
			{
				// バインド失敗
				m_ClientPhase = ClientPhases.None ;

				m_SessionPlayers = null ;

				DeleteRealTimeSocketClient() ;

				m_SessionId         = 0 ;
				m_IsSessionJoined   = false ;
			}

			if( isCanceled == true )
			{
				// キャンセルされていたらキャンセル例外を発行する
				throw new OperationCanceledException() ;
			}

			if( m_ClientPhase == ClientPhases.Disconnecting )
			{
				// 失敗
				return false ;
			}

			Debug.Log( "<color=#FFFF00>ExchangeServer にバインド完了 : m_ClientPhase = " + m_ClientPhase + " m_UdpEnabled = " + m_UdpEnabled + "</color>" ) ;

			//------------------------------------------------------------------------------------------
			// ＵＤＰが有効であるなら KeepAlive を一定時間毎に送りつつ Ready を受信するのを待つ

			// 既に Ready になっていたらスキップする(ＵＤＰを使用しないケースではありえる)
			if( m_ClientPhase == ClientPhases.Connecting )
			{
				long baseTicks = 0 ;

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource_ForExchangeServer ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForExchangeServer.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForExchangeServer != null )
				{
					if( m_CancellationTokenSource_ForExchangeServer.IsCancellationRequested == true )
					{
						isCanceled = true ;
						break ;
					}

					if( m_UdpEnabled == true )
					{
						// ＵＤＰが有効

						if( ( Timer.NowTicks - baseTicks ) >  250 )
						{
//							Debug.Log( "<color=#00FFFF>UDP で KeepAlive を送信</color>" ) ;
							// ＵＤＰが有効である場合はルートを確立するため一定期間おきにＵＤＰのＫｅｅｐＡｌｉｖｅを送信する
							SendKeepAlive( PacketTypes.UDP ) ;

							baseTicks = Timer.NowTicks ;
						}
					}

					//---------------------------------

					if( m_ClientPhase != ClientPhases.Connecting )
					{
						// 待機終了
						break ;
					}

					await Task.Yield() ;
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( m_ClientPhase != ClientPhases.Ready )
				{
					// バインド失敗
					m_ClientPhase = ClientPhases.None ;

					m_SessionPlayers = null ;

					DeleteRealTimeSocketClient() ;

					m_SessionId         = 0 ;
					m_IsSessionJoined   = false ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				if( m_ClientPhase != ClientPhases.Ready )
				{
					// 失敗
					return false ;
				}
			}

			//----------------------------------
			// 最後にサーバーに準備完了を通知する

			SendClientReady() ;

			//----------------------------------

			Debug.Log( "<color=#FFFF00>フレーム通信が可能な状態になった</color>" ) ;

			//------------------------------------------------------------------------------------------

			// データパケットのシーケンス番号初期化
			m_SendingFrameSequence = 0 ;

			// 監視タスクを起動する
			_ = ProcessSession() ;

			// 接続成功
			return true ;
		}

		// セッションの接続状況監視用の非同期タスク
		private async Task ProcessSession()
		{
			bool isCanceled ;

			PacketTypes packetType = m_UdpEnabled == false ? PacketTypes.TCP : PacketTypes.UDP ;

			isCanceled = false ;

			long nowTicks ;
			long pingSendTime_ToServer	= Timer.NowTicks + 1000 ;	// 早すぎると初動の負荷あまり正しい値にならないためある程度時間経過してから計測を開始する
			long pingSendTime_ToHost	= Timer.NowTicks + 2000 ;	// 早すぎると初動の負荷あまり正しい値にならないためある程度時間経過してから計測を開始する

			while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForExchangeServer != null )
			{
				if( m_CancellationTokenSource_ForExchangeServer.IsCancellationRequested == true )
				{
					// タスクがキャンセルされた
					isCanceled = true ;
					break ;
				}

				if( m_ClientPhase == ClientPhases.Ready )
				{
					// 接続状態を継続している

					nowTicks = Timer.NowTicks ;

					if( UseKeepAlive == true )
					{
						// KeepAlive を使用する

						if( ( nowTicks - m_LastSendTime ) >= 5000 )
						{
							// ５秒経過

							// KeepAlive を送信する
							SendKeepAlive( packetType ) ;
						}
					}

					if( UsePing == true )
					{
						// Ping 計測を行う

						if( ( nowTicks - pingSendTime_ToServer ) >= 5000 )
						{
							// ５秒経過
							pingSendTime_ToServer = nowTicks ;

							// Ping(サーバーまでの往復時間)
							SendPing( false, nowTicks, false, null, packetType ) ;
						}

						if( ( nowTicks - pingSendTime_ToHost ) >= 5000 )
						{
							// ５秒経過
							pingSendTime_ToHost = nowTicks ;

							// Ping(ホストまでの往復時間)　※クライアントの識別子はホストに送信する際にサーバーで設定してくれる
							SendPing( true, nowTicks, true, null, packetType ) ;
						}
					}
				}

				if( m_ClientPhase == ClientPhases.Disconnecting )
				{
					// ソケットが切断状態になっている
					Debug.Log( "ソケットの切断要求が出されている" ) ;
					break ;
				}

				// 少し待つ
				await Task.Yield() ;
			}

			//----------------------------------------------------------

			if( m_ManagementType == SessionManagementTypes.HostManagement )
			{
				// セッションはホスト管理になっている
				if( IsHost == true )
				{
					if( m_SessionPlayers.Count == 1 )
					{
						var sessionPlayer = m_SessionPlayers[ 0 ] ;

						CallOnPlayerLeft_ForSessionProcessor( new
						(
							sessionPlayer.UserId,
							PlayerName,
							sessionPlayer.IsGuest,
							sessionPlayer.IsHost
						) ) ;

						// 自身が最後の１人のホストである場合はセッションプロセッサーにも通知
						CallOnDeleted_ForSessionProcessor() ;
					}
				}
			}

			//----------------------------------------------------------
			// 後始末を行う

			m_SessionPlayers = null ;
			m_ClientPhase = ClientPhases.None ;

			DeleteRealTimeSocketClient() ;

			m_SessionId         = 0 ;
			m_IsSessionJoined   = false ;

			//----------------------------------

			if( isCanceled == true )
			{
				// キャンセルされていたらキャンセル例外を発行する
				throw new OperationCanceledException() ;
			}

			//----------------------------------
			// 順番に注意(タスクキャンセルならコールバックは呼び出さない)

			if( m_OnDisconnected != null )
			{
				CallOnDisconnected() ;
			}
		}

		//-----------------------------------------------------------

		// リアルタイム通信用のソケットクライアントを破棄する
		private void DeleteRealTimeSocketClient()
		{
			// タスクをキャンセルする
			if( m_CancellationTokenSource_ForExchangeServer != null )
			{
				if( m_CancellationTokenSource_ForExchangeServer.IsCancellationRequested == false )
				{
					m_CancellationTokenSource_ForExchangeServer.Cancel() ;
				}

				m_CancellationTokenSource_ForExchangeServer.Dispose() ;
				m_CancellationTokenSource_ForExchangeServer = null ;
			}

			// リアルタイム通信用のソケットクライアントを破棄する
			if( m_RealTimeSocketClient != null )
			{
				m_RealTimeSocketClient.Dispose() ;
				m_RealTimeSocketClient = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションから離脱する(離脱を完了するまで待つ)
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task LeaveFromSessionAsync( CancellationToken cancellationToken = default )
		{
			if( m_IsSessionJoined == false )
			{
				return ;
			}

			if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
			{
				Debug.Log( "クライアントによる明示的切断(2)" ) ;
				m_ClientPhase  = ClientPhases.Disconnecting ;
			}
			else
			{
				return ;
			}

			//----------------------------------

			CancellationTokenSource cancellationTokenSource ;

			//----------------------------------------------------------
			// セッションサーバーへからの離脱を行う

			if( cancellationToken == default )
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken ) ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken, cancellationToken ) ;
			}

			while( cancellationTokenSource.IsCancellationRequested == false )
			{
				if( m_IsSessionJoined == false )
				{
					break ;
				}

				await Task.Yield() ;
			}

			if( cancellationTokenSource.IsCancellationRequested == false )
			{
				cancellationTokenSource.Cancel() ;
			}

			cancellationTokenSource.Dispose() ;
		}

		//-------------------------------------------------------------------------------------------
		// SocketHelper 関連のコールバック

		// ＴＣＰパケットを受信した際に呼び出されるコールバック
		private void OnTcpReceived_FromSessonServer( ReadOnlyMemory<byte> data )
		{
			if( m_ClientPhase != ClientPhases.RequestBinding && m_ClientPhase != ClientPhases.Connecting && m_ClientPhase != ClientPhases.Ready )
			{
				// 異常な状態でコールバックが呼ばれたので無視する
				Debug.Log( "ＴＣＰ受信時のフェーズ異常による切断 : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			// 全体で復号化する
			byte[] commandData = m_Crypter.DecryptXor( data.Span ) ;

			if( commandData == null || commandData.Length <  1 )
			{
				// 不正レスポンス(結果として切断する)
				Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(1) : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			var commandType = ( CommandTypes )commandData[ 0 ] ;

//			Debug.Log( "<color=#3F7FFF>-------------->[TCP] コマンド受信 = " + commandType + " 現在のフェーズ : " + m_ClientPhase + "</color>" ) ;

			if( m_ClientPhase == ClientPhases.RequestBinding )
			{
				// まだバインドされていない

				if( commandType == CommandTypes.BindClientToSessionPlayerComplated )
				{
					// バインド完了通知
					try
					{
						int offset = 1 ;

						// セッション内のプレイヤー情報群を取得する
						m_SessionPlayers.Clear() ;

						SessionPlayerData sessionPlayer ;

						// 現在セッション内で有効なプレイヤー数を取得
						int index, count = DataFormat.GetVUShort( commandData, ref offset ) ;
						if( count >  0 )
						{
							// 現在セッション内で有効なプレイヤーの情報(識別子・名前・ホストかどうか)を取得
							for( index  = 0 ; index <  count ; index ++ )
							{
								sessionPlayer = new SessionPlayerData() ;
								sessionPlayer.Decode( commandData, ref offset ) ;

								m_SessionPlayers.Add( sessionPlayer ) ;
							}
						}

						long pingTicks = DataFormat.GetLong( commandData, ref offset ) ;

						Debug.Log( "<color=#3FFF3F>------バインド実行時の往復時間 " + ( Timer.NowTicks - pingTicks ) + " ms</color>" ) ;
					}
					catch( Exception )
					{
						// データ異常
						Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(2) : " + m_ClientPhase ) ;
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}
					//--------------------------------------------------------

					// 接続が確立された状態に以降する
					m_ClientPhase = ClientPhases.Connecting ;

					// タイマー計測開始(初期化)
					m_LastSendTime = Timer.NowTicks	;	// これはローカルのタイマーを使用する

					Debug.Log( "<color=#FFFF00>[受信] セッションプレイヤーとのバインド状態に移行した</color>" ) ;

					// 成功
					return ;
				}
			}

			if( m_ClientPhase == ClientPhases.Connecting )
			{
				if( commandType == CommandTypes.ServerReady )
				{
					// フレーム通信可能な状態に移行する

					// フレーム通信が可能な状態に移行する
					m_ClientPhase = ClientPhases.Ready ;

					Debug.Log( "<color=#FFFF00>[受信] フレーム通信可能な状態に移行した</color>" ) ;

					// 成功
					return ;
				}
			}

			if( m_ClientPhase == ClientPhases.Connecting || m_ClientPhase == ClientPhases.Ready )
			{
				// 既にバインド済み

				if( commandType == CommandTypes.JoinedPlayer )
				{
					// 新しいプレイヤーがセッションに参加した(バインド済みとなった)

					try
					{
						int offset = 1 ;

						var joinedSessionPlayer = new SessionPlayerData() ;

						joinedSessionPlayer.Decode( commandData, ref offset ) ;

//						Debug.Log( "<color=#FF00FF>新たに加わったプレイヤー : " + joinedSessionPlayer.UserName + " : " + joinedSessionPlayer.IsHost + "</color>" ) ;

						m_SessionPlayers.Add( joinedSessionPlayer ) ;

						//-------------------------------

						// 参加したプレイヤー情報(公開用)
						var sessionPlayer = new SessionPlayer
						(
							joinedSessionPlayer.UserId,
							joinedSessionPlayer.Name,
							joinedSessionPlayer.IsGuest,
							joinedSessionPlayer.IsHost
						) ;

						// コールバック処理内で例外が発生しても全体の処理を止めないために try ～ catch で実行する

						//------------------------------

						// コールバックを呼ぶ
						CallOnPlayerJoined( sessionPlayer ) ;

						//-------------------------------

						// セッションプロセッサーにも通知する
						if( m_ManagementType == SessionManagementTypes.HostManagement )
						{
							// セッション管理はホストになっている
							if( IsHost == true )
							{
								// ホストのみ処理する
								CallOnPlayerJoined_ForSessionProcessor( sessionPlayer ) ;
							}
						}

						//-------------------------------

						// 成功
						return ;
					}
					catch( Exception )
					{
						// データ異常
						Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(3) : " + m_ClientPhase ) ;
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}
				}
				else
				if( commandType == CommandTypes.LeftPlayer )
				{
					// 既存のプレイヤーがセッションから離脱した

					try
					{
						int offset = 1 ;

						var leftSessionPlayer = new SessionPlayerData() ;	// 将来的には識別子のみにしても良い
						leftSessionPlayer.Decode( commandData, ref offset ) ;

						string userId = leftSessionPlayer.UserId ;

						//-------------------------------

						// 新しいホストのユーザー識別子
						string hostUserId = DataFormat.GetString( commandData, ref offset ) ;

						//-------------------------------------------------------

						// 処理前の自身のホスト状況
						bool isHost = IsHost ;

						// 削除対象のプレイヤーを探す
						var removingSessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.UserId == userId ) ;
						if( removingSessionPlayer != null )
						{
							// プレイヤーを削除する
							m_SessionPlayers.Remove( removingSessionPlayer ) ;
							
							// ホストが変更された可能性があるためホストフラグを更新する
							foreach( var sessionHostPlayer in m_SessionPlayers )
							{
								sessionHostPlayer.IsHost = ( sessionHostPlayer.UserId == hostUserId ) ;
							}

							//-------------------------------

							// 離脱したプレイヤー情報(公開用)
							var sessionPlayer = new SessionPlayer
							(
								removingSessionPlayer.UserId,
								removingSessionPlayer.Name,
								removingSessionPlayer.IsGuest,
								removingSessionPlayer.IsHost
							) ;

							// コールバック処理内で例外が発生しても全体の処理を止めないために try ～ catch で実行する

							//------------------------------

							// 実際にプレイヤー情報をリストから除去した後にコールバックを呼ぶ
							CallOnPlayerLeft( sessionPlayer ) ;

							//-------------------------------

							// セッションプロセッサーにも通知する
							if( m_ManagementType == SessionManagementTypes.HostManagement )
							{
								// セッション管理はホストになっている
								if( IsHost == true )
								{
									// ホストのみ処理する
									if( isHost == false )
									{
										// このタイミングで新たにホストになった
										CallOnActive_ForSessionProcessor( GetSessionPlayers() ) ;
									}
									else
									{
										// 既に自身はホストになっている
										CallOnPlayerLeft_ForSessionProcessor( sessionPlayer ) ;
									}
								}
							}
						}

						// 成功
						return ;
					}
					catch( Exception )
					{
						// データ異常
						Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(4) : " + m_ClientPhase ) ;
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}
				}
				else
				if( commandType == CommandTypes.Ping )
				{
					// Ping

					int offset = 1 ;

					// ＴＣＰとＵＤＰの共通のピング受信処理
					OnPingReceived( commandData, ref offset, PacketTypes.TCP ) ;

					// 成功
					return ;
				}
			}

			if( m_ClientPhase == ClientPhases.Ready )
			{
				// フレームの通信が可能な状態

				if( commandType == CommandTypes.Frame )
				{
					// フレームを受信

					int offset = 1 ;

					// ＴＣＰとＵＤＰの共通のフレーム受信処理
					OnFrameReceived( commandData, ref offset, PacketTypes.TCP, false ) ;

					return ;
				}
				else
				if( commandType == CommandTypes.Retransmission )
				{
					// フレーの再送要求を受信(TCP)

					Debug.Log( "<color=#FF3F00>ＵＤＰリオーダー発生による再送要求を受信した(サーバーに届いていない)</color>" ) ;

					ushort sequence ;

					try
					{
						int offset = 1 ;

						// 再送要求の対象となるフレームのシーケンス
						sequence = DataFormat.GetUShort( commandData, ref offset ) ;

						Debug.Log( "<color=#FF7F00>→ サーバーからの再送要求対象のシーケンス番号 : " + sequence + "</color>" ) ;
					}
					catch( Exception )
					{
						// データ異常
						Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(5) : " + m_ClientPhase ) ;
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}

					//--------------------------------

					// 送信済みフレーム内から対象のシーケンスのフレームを取得する
					var frame = GetSendingFrame( sequence ) ;
					if( frame != null )
					{
						// 再送要求に応答できるので対象のシーケンスのフレームを再送信する

						// フレームを再送信する
						SendRetransmissionFrame_Private( frame, PacketTypes.TCP ) ;	// ＴＣＰ固定

						Debug.Log( "<color=#FFBFDF> >>> サーバーの要求するフレームを再送信 シーケンス番号 = " + sequence + "</color>" ) ;

						// 成功
						return ;
					}
					else
					{
						// 既にバックアップされた送信フレームから消失している
						Debug.Log( "<color=#FF7F00>---------->再送要求に該当するシーケンスのフレームを送信バッファから発見出来なかった シーケンス = " + sequence + "</color>" ) ;
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}
				}
				else
				if( commandType == CommandTypes.RetransmissionFrame )
				{
					// 再送フレームを受信

					int offset = 1 ;

					// ＴＣＰとＵＤＰの共通のフレーム受信処理
					OnFrameReceived( commandData, ref offset, PacketTypes.TCP, true ) ;

					// 成功
					return ;
				}
			}

			//----------------------------------------------------------

			// ここに来るのはいずれのコマンドにも該当しなかったという事なので不正アクセス
			Debug.LogWarning( "ＴＣＰ受信時にいずれのコマンドに該当しなかった : Phase = " + m_ClientPhase + " Command = " + commandType ) ;
			m_ClientPhase = ClientPhases.Disconnecting ;
		}

		// ＴＣＰが切断された際に呼び出されるコールバック
		private void OnTcpDeiconnected_FromSessionServer()
		{
			// 切断状態
			Debug.LogWarning( "ＴＣＰ接続がサーバーから切断された : " + m_ClientPhase ) ;
			m_ClientPhase = ClientPhases.Disconnecting ;
		}

		// ＵＤＰパケットを受信した際に呼び出されるコールバック
		private void OnUdpReceived_FromSessionServer( ReadOnlyMemory<byte> data, string serverAddress, int serverPort )
		{
			if( m_ClientPhase != ClientPhases.Ready )
			{
				// 異常な状態でコールバックが呼ばれたので無視する
				Debug.LogWarning( "ＵＤＰ受信時のフェーズ異常による切断 : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			// 全体で復号化する
			byte[] commandData = m_Crypter.DecryptXor( data.Span ) ;

			if( commandData == null || commandData.Length <  1 )
			{
				// 不正レスポンス(結果として切断する)
				Debug.LogWarning( "ＵＤＰ受信時のデータ異常による切断(1) : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			var commandType = ( CommandTypes )commandData[ 0 ] ;

			if( commandType == CommandTypes.Frame )
			{
				int offset = 1 ;

				// ＴＣＰとＵＤＰの共通のフレーム受信処理
				OnFrameReceived( commandData, ref offset, PacketTypes.UDP, false ) ;
			}
			else
			if( commandType == CommandTypes.Ping )
			{
				int offset = 1 ;

				// ＴＣＰとＵＤＰの共通のピング受信処理
				OnPingReceived( commandData, ref offset, PacketTypes.UDP ) ;
			}
			else
			{
				// ＵＤＰではフレーム以外の受信を許容しない

				// 異常な状態でコールバックが呼ばれたので無視する
				Debug.LogWarning( "ＵＤＰ受信時のデータ異常による切断(2) : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// NetworkPlay 関連のコールバック

		// フレーム受信時のＴＣＰとＵＤＰの共通処理(どちらで受信したかわかるようにする)
		private void OnFrameReceived
		(
			byte[] commandData,
			ref int offset,

			PacketTypes packetType,
			bool isRetransmissionFrame
		)
		{
			// ※packetType は、ホストが転送アップする場合にのみ必要
			//----------------------------------------------------------

			// シーンス番号
			ushort sequence ;

			// 送信動作種別
			bool	isTransfer ;

			// 送信元の種別
			DestinationTypes destinationType ;

			// 送信元のユーザー識別子群
			string[] destinationUserIds = null ;

			// 送信元の種別
			SourceTypes sourceType ;

			// 送信元のユーザー識別子
			string sourceUserId = null ;

			// データ
			byte[] data ;

			try
			{
				// シーケンス番号
				sequence = DataFormat.GetUShort( commandData, ref offset ) ;

				// 送信動作種別
				isTransfer = DataFormat.GetBool( commandData, ref offset ) ;

				if( isTransfer == true )
				{
					// 転送ダウン
					if( m_ManagementType != SessionManagementTypes.HostManagement )
					{
						// ホスト管理モードでなければ転送ダウンフレームを受信するのは異常であるた無視する
						Debug.LogWarning( "ホスト管理モードでないにも関わらずが転送ダウンフレームを受信した" ) ;
						return ;
					}

					if( IsHost == false )
					{
						// 転送ダウンを処理可能なのはホストのみであるためホストでない場合は不正データが送られてきたとみなして無視する
						Debug.LogWarning( "ホストでないにも関わらず転送ダウンフレームを受信した" ) ;
						return ;
					}
				}

				// 送信元の種別
//				destinationType = DataFormat.GetEnum<DestinationTypes>( commandData, ref offset ) ;

				byte destinationTypeCode = DataFormat.GetByte( commandData, ref offset ) ;
//				if( Enum.IsDefined( typeof( DestinationTypes ), destinationTypeCode ) == true )
				if( destinationTypeCode >= 0 && destinationTypeCode <= 4 )
				{
					destinationType = ( DestinationTypes )destinationTypeCode ;
				}
				else
				{
					// データ異常
					Debug.LogWarning( "送信先種別が異常なダウンフレームを受信した" ) ;
					return ;					
				}

				if( isTransfer == false )
				{
					// 通常ダウン
				}
				else
				{
					// 転送ダウン
					if( destinationType == DestinationTypes.Broadcast )
					{
						// 特に処理する必要は無い
					}
					else
					if( destinationType == DestinationTypes.Multicast )
					{
						// 宛先が正しいか判定を行う(途中改竄を警戒)

						int i, l = DataFormat.GetVUShort( commandData, ref offset ) ;
						if( l >= 1 && l <= m_SessionPlayers.Count )
						{
							destinationUserIds = new string[ l ] ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								destinationUserIds[ i ] = DataFormat.GetString( commandData, ref offset ) ;
							}
						}
						else
						{
							// データ異常
							Debug.LogWarning( "[" + m_UserId + "] 転送ダウンフレームのマルチキャスト送信先が異常 : " + l ) ;
							return ;					
						}
					}
					else
					if( destinationType == DestinationTypes.ToServer )
					{
						// ここに来る事はありえない
					}
					else
					{
						// 異常なデータ(転送ダウンは Broadcast と Multicast 以外はありえない)
						Debug.LogWarning( "転送ダウンフレームにおいて異常な送信先を検出した : " + destinationType ) ;
						return ;
					}
				}

				// 送信元の種別
//				sourceType = DataFormat.GetEnum<SourceTypes>( commandData, ref offset ) ;

				byte sourceTypeCode = DataFormat.GetByte( commandData,ref offset ) ;
//				if( Enum.IsDefined( typeof( SourceTypes ), sourceTypeCode ) == true )
				if( sourceTypeCode >= 0 && sourceTypeCode <= 1 )
				{
					sourceType = ( SourceTypes )sourceTypeCode ;
				}
				else
				{
					// データ異常
					Debug.LogWarning( "送信元種別が異常なダウンフレームを受信した" ) ;
					return ;					
				}

				if( sourceType == SourceTypes.FromClient )
				{
					// 送信元のユーザー識別子
					sourceUserId = DataFormat.GetString( commandData, ref offset ) ;

					if( string.IsNullOrEmpty( sourceUserId ) == true )
					{
						// データ異常
						Debug.LogWarning( "[ " + m_UserId + " ]送信元のユーザー識別子が異常なダウンフレームを受信した SourceUserId is null" ) ;
						return ;
					}
				}

				data = DataFormat.GetByteArray( commandData, ref offset ) ;
				if( data == null || data.Length == 0 )
				{
					// データ異常
					Debug.LogWarning( "送信データが異常なダウンフレームを受信した" ) ;
					return ;
				}
			}
			catch( Exception )
			{
				// データ異常
				Debug.LogWarning( "送信データに何らかの異常が見られるダウンフレームを受信した" ) ;
				return ;
			}

			//----------------------------------

			// 受信したフレームは一応問題なしと判断する
			// バッファに貯めつつ問題がなければ受信コールバックを呼び出す
			ProcessReceivingFrames
			(
				sequence,
				isTransfer,
				destinationType,
				destinationUserIds,
				sourceType,
				sourceUserId,
				data,

				packetType,	// ホストの転送アップでＴＣＰとＵＤＰのどちらの方法を使用するか選択の必要があるため必要
				isRetransmissionFrame
			) ;
		}

		// ピング受信時のＴＣＰとＵＤＰの共通処理(どちらで受信したかわかるようにする)
		private void OnPingReceived
		(
			byte[] commandData,
			ref int offset,

			PacketTypes packetType
		)
		{
			try
			{
				bool isHost = DataFormat.GetBool( commandData, ref offset ) ;

				// 現在セッション内で有効なプレイヤー数を取得
				long oldTicks = DataFormat.GetLong( commandData, ref offset ) ;

				if( isHost == false )
				{
					// サーバーまでの往復時間
					PingToServer = Timer.NowTicks - oldTicks ;

					if( m_PingToServer_Count == 0 )
					{
						PingToServer_Min = PingToServer ;
						PingToServer_Max = PingToServer ;
					}
					else
					if( m_PingToServer_Count < 1000000000 )
					{
						if( PingToServer <  PingToServer_Min )
						{
							PingToServer_Min = PingToServer ;
						}
						if( PingToServer >  PingToServer_Max )
						{
							PingToServer_Max = PingToServer ;
						}
					}
					else
					{
						// 一旦リセットする

						PingToServer_Min = PingToServer ;
						PingToServer_Max = PingToServer ;

						m_PingToServer_Count = 0 ;
						m_PingToServer_Total = 0 ;
					}

					m_PingToServer_Count ++ ;
					m_PingToServer_Total += PingToServer ;

//					Debug.Log( "<color=#FFFF00>-----------> [PING] サーバーまでの往復時間 : " + PingToServer + " ms : PacketType = " + packetType + "</color>" ) ;
				}
				else
				{
					// ホストまでの往復時間

					// 送信動作種別
					bool isTransfer = DataFormat.GetBool( commandData, ref offset ) ;

					if( isTransfer == false )
					{
						// ホストからの往復 Ping を受信した

						// サーバーまでの往復時間
						PingToHost = Timer.NowTicks - oldTicks ;

						if( m_PingToHost_Count == 0 )
						{
							PingToHost_Min = PingToHost ;
							PingToHost_Max = PingToHost ;
						}
						else
						if( m_PingToHost_Count < 1000000000 )
						{
							if( PingToHost <  PingToHost_Min )
							{
								PingToHost_Min = PingToHost ;
							}
							if( PingToHost >  PingToHost_Max )
							{
								PingToHost_Max = PingToHost ;
							}
						}
						else
						{
							// 一旦リセットする

							PingToHost_Min = PingToHost ;
							PingToHost_Max = PingToHost ;

							m_PingToHost_Count = 0 ;
							m_PingToHost_Total = 0 ;
						}

						m_PingToHost_Count ++ ;
						m_PingToHost_Total += PingToHost ;

//						Debug.Log( "<color=#FFFF80>-----------> [PING] ホストまでの往復時間 : " + PingToHost + " ms : PacketType = " + packetType + "</color>" ) ;
					}
					else
					{
						if( IsHost == false )
						{
							// ホストでは無いので無視する
							Debug.LogWarning( "ホストで無いにも関わらずホスト宛の Ping を受信した" ) ;
							return ;
						}

						//-------------------------------

						// Ping 計測の要求を出したクライアントに送り返す

						string sourceUserId = DataFormat.GetString( commandData, ref offset ) ;
						if( string.IsNullOrEmpty( sourceUserId ) == true )
						{
							Debug.LogWarning( "Ping 計測元の識別子が不明" ) ;
							return ;
						}

						// クライアントへ返信
						SendPing( true, oldTicks, false, sourceUserId, packetType ) ;
					}
				}
			}
			catch( Exception )
			{
				// エラーが発生したら無視する
				Debug.LogWarning( "Ping の受信データに異常を確認" ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// コマンドを送信する
		private void SendCommand( CommandTypes commandType, Action<List<byte>> onDataAdditional, PacketTypes packetType )
		{
			var command = new List<byte>() ;

			//----------------------------------
			// 平文部分

			// シグネチャ
			command.AddRange( m_Signature ) ;

			// バージョンコード
			command.Add( ( byte )(   m_VersionCode         & 0xFF ) ) ; 
			command.Add( ( byte )( ( m_VersionCode >>  8 ) & 0xFF ) ) ; 
			command.Add( ( byte )( ( m_VersionCode >> 16 ) & 0xFF ) ) ; 
			command.Add( ( byte )( ( m_VersionCode >> 24 ) & 0xFF ) ) ; 

			// 通信区分
			command.Add( ( byte )NetworkTargetTypes.Client ) ;

			//----------------------------------

			// アクセストークン
			DataFormat.PutString( command, m_AccessToken ) ;

			//----------------------------------
			// 暗号部分

			var commandContentData = new List<byte>() ;

			// セッション識別子
			DataFormat.PutUInt( commandContentData, m_SessionId ) ;

			// コマンド種別
			DataFormat.PutByte( commandContentData, ( byte )commandType ) ;

			// データ追加のコールバック
			onDataAdditional?.Invoke( commandContentData ) ;

			// 暗号化
			byte[] encryptedData = m_Crypter.EncryptXor( commandContentData.ToArray() ) ;

			command.AddRange( encryptedData ) ;

			byte[] commandData = command.ToArray() ;

			//----------------------------------------------------------

			if( packetType == PacketTypes.TCP )
			{
				// ＴＣＰでパケットを送信する
				m_RealTimeSocketClient.SendTcp( commandData ) ;
			}
			else
			{
				// ＵＤＰでパケットを送信する(宛先を後できちんと設定する[IPv6]にも対応が必要)
				m_RealTimeSocketClient.SendUdp( commandData, m_ExchangeServerAddress, m_ExchangeServerUdpPort ) ;
			}

			// 最後に送信した時間を更新する
			m_LastSendTime = Timer.NowTicks ;
		}

		// セッションへのバインド要求を送信する
		private void SendBindClientToSessionPlayer()
		{
			// TCP
			SendCommand
			(
				CommandTypes.BindClientToSessionPlayer,
				( List<byte> commandContentData ) =>
				{
					// おおよその応答までの時間計測のため現在の時間を格納する
					DataFormat.PutLong( commandContentData, Timer.NowTicks ) ;
				},
				PacketTypes.TCP
			) ;
		}

		// セッションへの完全接続理解を送信する
		private void SendClientReady()
		{
			// TCP
			SendCommand
			(
				CommandTypes.ClientReady,
				null,
				PacketTypes.TCP
			) ;
		}


		// フレームを送信する
		private void SendFrame_Private( UpstreamFrameData frame, PacketTypes packetType )
		{
			// フレームを送信する
			SendCommand
			(
				CommandTypes.Frame,
				( List<byte> commandContentData ) =>
				{
					frame.Encode( commandContentData ) ;
				},
				packetType
			) ;
		}

		// 再送フレームを送信する
		private void SendRetransmissionFrame_Private( UpstreamFrameData frame, PacketTypes packetType )
		{
			// 再送フレームを送信する
			SendCommand
			(
				CommandTypes.RetransmissionFrame,
				( List<byte> commandContentData ) =>
				{
					frame.Encode( commandContentData ) ;
				},
				packetType
			) ;
		}

		//-----------------------------------------------------------

		// 送信バッファの排他制御用のオブジェクト
		private readonly object	m_SendingFrameLockObject = new () ;

		// 送信済みフレームのバックアップ(再送要求用)　※ＵＤＰが有効な時のみ使用
		// このメソッドの呼び出し元で排他制御が行われている
		private void AddSendingFrame( UpstreamFrameData frame )
		{
			// バックアップに送信済みフレームを追加
			m_SendingFrames.Add( frame ) ;

			//----------------------------------

			long limitTicks = Timer.NowTicks - 5000 ;	// ひとまず５秒前(後で時間は任意の値に変えられるようにする)

			// 一定時間以上経過したバックアップフレームは削除する
			while( m_SendingFrames.Count >  0 )
			{
				if( m_SendingFrames[ 0 ].NowTicks <   limitTicks )
				{
					// 削除対象
					m_SendingFrames.RemoveAt( 0 ) ;
				}
				else
				{
					// 削除不可
					break ;
				}
			}
		}

		// 再送要求の対象となっているシーケンスのフレームを取得する
		private UpstreamFrameData GetSendingFrame( ushort sequence )
		{
			// フレームのバックアップ用バッファへの蓄積とフレームの再送要求がかち合うので排他制御が必要
			lock( m_SendingFrameLockObject )
			{
				if( m_SendingFrames == null || m_SendingFrames.Count == 0 )
				{
					// 不可
					return null ;
				}

				//----------------------------------
				// 新しい方から検査する

				for( int i  = m_SendingFrames.Count - 1 ; i >= 0 ; i -- )
				{
					if( m_SendingFrames[ i ].Sequence == sequence )
					{
						// 該当を発見した
						return m_SendingFrames[ i ] ;
					}
				}

				// 該当を発見できなかった
				return null ;
			}
		}


		//------------------------------------------------------------------------------------------

		// 再送要求の排他制御用のオブジェクト
		private readonly object	m_RetransmissionTaskLockObject = new () ;

		// ※m_ReceivingFrames への追加と削除はこのメソッド内でりみ行われるためスレッド間の排他制御は不要

		// シーケンス的に正常なフレームを取得する(サブスレッド実行)
		private void ProcessReceivingFrames
		(
			ushort				sequence,
			bool				isTransfer,
			DestinationTypes	destinationType,
			string[]			destinationUserIds,
			SourceTypes			sourceType,
			string				sourceUserId,
			byte[]				data,

			PacketTypes			packetType,	// ＴＣＰとＵＤＰのどちらで受信したか
			bool				isRetransmissionFrame
		)
		{
			// 新しく受信したフレーム
			var frame = new DownstreamFrameData
			(
				sequence,
				isTransfer,
				destinationType,
				destinationUserIds,
				sourceType,
				sourceUserId,
				data,

				packetType
			) ;

//			Debug.Log( "<color=#00FFFF>isRetransmissionFrame = " + isRetransmissionFrame + "</color>" ) ;

			// TCP の場合はそのまま受信処理を行う
			// ただし TCP でも再送フレームの場合はフレーム順番確認処理へ流す
			if( ( packetType == PacketTypes.TCP && isRetransmissionFrame == false ) || m_UdpCorrectionEnabled == false )
			{
				// 補正不要なのでそのままフレームを対象プレイヤーに受け渡す
				SendReceivingFrame( frame ) ;
				return ;
			}

			//-----------------------------------------------------------------------------------------
			// 補正確認

			// 処理対象のシーケンスと受信したシーケンスを比較する
			ushort delta = ( ushort )( frame.Sequence - m_ReceivingFrameSequence ) ;
			if( delta >= 0x8000 )
			{
				// 過去のシーケンスのフレームなので読み捨て(既にこのシーケンスのフレームは処理されている)
				return ;
			}

			if( delta == 0 )
			{
				// 必要としているシーケンスのフレームを受信した

				// 必要としているシーケンスのフレームを受信した
				lock( m_RetransmissionTaskLockObject )
				{
					// 受信したフレームを対象プレイヤーに受け渡す
					SendReceivingFrame( frame ) ;

					//--------------------------------

					if( m_RetransmissionSending == true && m_RetransmissionSendingFrameSequence == m_ReceivingFrameSequence )
					{
						// 再送要求を送っていたシーケンスのフレームを受信したので再送要求の送信中の状態を解除する

						m_RetransmissionSending		= false ;
						m_RetransmissionBaseTime	= Timer.NowTicks ;
					}

					//--------------------------------

					// 次のシーケンスへ
					m_ReceivingFrameSequence ++ ;

					//--------------------------------

					if( m_ReceivingFrames.Count == 0 )
					{
						// 終了(ＴＣＰは常にここに来るはず)

						// フレームの入れ替わり状態は発生していない状態なので入れ替わりフラグを解消する
						m_Reordering = false ;

						// 再送要求のタスクを終了させる判定基準時間を設定
						m_RetransmissionBaseTime = Timer.NowTicks ;

						// 終了
						return ;
					}

					//--------------------------------

					int index = 0 ;

					// 溜まっているバッファのシーケンス検査と適切な処理
					while( index <  m_ReceivingFrames.Count )
					{
						delta = ( ushort )( m_ReceivingFrames[ index ].Sequence - m_ReceivingFrameSequence ) ;
						if( delta >= 0x8000 )
						{
							// 古いシーケンスのフレームなので読み捨て(既にこのシーケンスのフレームは処理されている)
							m_ReceivingFrames.RemoveAt( index ) ;
						}
						else
						if( delta == 0 )
						{
							// 処理対象のシーケンス
							frame = m_ReceivingFrames[ index ] ;
							m_ReceivingFrames.RemoveAt( index ) ;

							// 検査位置をリセット(最初に戻す)
							index = 0 ;

							// 受信したフレームを対象プレイヤーに受け渡す
							SendReceivingFrame( frame ) ;

							// 次のシーケンスへ
							m_ReceivingFrameSequence ++ ;
						}
						else
						{
							// 未来のシーケンスを検出(入れ替わり状態は発生している)
							index ++ ;
						}
					}

					//---------------------------------

					if( m_ReceivingFrames.Count == 0 )
					{
						// フレームの入れ替わり状態は発生していない状態なので入れ替わりフラグを解消する
						m_Reordering = false ;

						// 再送要求のタスクを終了させる判定基準時間を設定
						m_RetransmissionBaseTime = Timer.NowTicks ;

						// 終了
						return ;
					}
				}
			}
			else
			{
				// 受信したのは処理対象ではない未来のシーケンスのフレーム(ＴＣＰではここに来る事は無いはず)

				// 受信バッファの最後に詰む
				m_ReceivingFrames.Add( frame ) ;
			}

			//---------------------------------

			if( m_Reordering == false )
			{
				// フレームの入れ替わり状態が発生しているフラグをオンにする(一定時間内に受信するフレームで解消しないようであれば再送要求を出す)
				m_Reordering = true ;

				//------------

				// 再送要求のタスクを開始させる
				StartRetransmissionProcessing() ;
			}
		}

		// 再送要求を処理するタスクのインスタンス
		private Task			m_RetransmissionTask ;

		// 再送要求のタスク生成と実行
		private void StartRetransmissionProcessing()
		{
			lock( m_RetransmissionTaskLockObject )
			{
				if( m_RetransmissionTask == null )
				{
					// タスクが生成・実行されていない場合のみ状態を初期化する

					// 再送要求を出していない状態にする(元々 false のはず)
					m_RetransmissionSending = false ;

					// 現在再送要求を出していない場合は基準時間を設定する(再送要求は多重に出す事はできない)
					m_RetransmissionBaseTime = Timer.NowTicks ;

					// タスクの生成と実行
					m_RetransmissionTask = Task.Run( () => ProcessRetransmession() ) ;
				}
			}
		}

		// タスク：再送要求の確認と実行を行う
		private async Task ProcessRetransmession()
		{
			long nowTicks ;

			while( true )
			{
				// 現在時間を取得
				nowTicks = Timer.NowTicks ;

				if( m_Reordering == false )
				{
					// 順番入れ替わり状態が解消されていても１秒はタスクを維持する(スレッドの生成と破棄にはそれなりの負荷が伴うため)

					lock( m_RetransmissionTaskLockObject )
					{
						if( ( nowTicks - m_RetransmissionBaseTime ) >= 1000 )
						{
							// タスク(スレッド)終了
							m_RetransmissionTask = null ;
							return ;
						}
					}
				}
				else
				{
					// 順番入れ替わり状態は依然継続中

					lock( m_RetransmissionTaskLockObject )
					{
						if( m_RetransmissionSending == false )
						{
							// 再送要求はまだ送信していない

							if( ( nowTicks - m_RetransmissionBaseTime ) >= 100 )
							{
								// 時間をオーバーしたので再送要求を出す(ＴＣＰでの再送要求)
								SendRetransmission( m_ReceivingFrameSequence ) ;

								//--------

								// 再送要求を送信した
								m_RetransmissionSending = true ;

								// 再送要求中のフレームのシーケンス
								m_RetransmissionSendingFrameSequence = m_ReceivingFrameSequence ;

								// 順番入れ替わりの解消までのタイムリミットの基準時間を設定する
								m_RetransmissionBaseTime = Timer.NowTicks ;
							}
						}
						else
						{
							// 既に再送要求を送信している
							if( ( nowTicks - m_RetransmissionBaseTime ) >= 1000 )
							{
								// 入れ替わり解消までの猶予時間を超過したのでこのセッションプレイヤーは回復不可能とみなし切断する
								Debug.LogWarning( "ＵＤＰ再送データ受け取りまでの限界時間を超過した事による切断 : " + m_ClientPhase ) ;
								m_ClientPhase = ClientPhases.Disconnecting ;
								return ;
							}
						}
					}
				}

				// タスクの中断対応(スレッドの負荷を軽くするために１０ミリ秒はスリープする)
				await Task.Delay( 10, m_CancellationTokenSource_ForExchangeServer.Token ) ;
			}
		}

		//------------------------------------------------------------------------------------------

		// 下りフレームの再送要求を出す
		private void SendRetransmission( ushort sequence )
		{
			SendCommand
			(
				CommandTypes.Retransmission,
				( List<byte> commandContentData ) =>
				{
					// 再送要求対象となるフレームのシーケンス
					DataFormat.PutUShort( commandContentData, sequence ) ;
				},
				PacketTypes.TCP
			) ;
		}

		// 完全に処理可能なフレームを受信した際の処理を行う
		private void SendReceivingFrame( DownstreamFrameData frame )
		{
			// アプリケーション側に受信したフレームをコールバックする

			if( frame.IsTransfer == false )
			{
				// 通常ダウン(終点)

				if( frame.DestinationType != DestinationTypes.ToServer )
				{
					// 不特定多数を対象
					CallOnReceived(  frame.Data, frame.SourceType, frame.SourceUserId  ) ;
				}
				else
				{
					Debug.LogWarning( "最終的なフレームの到着先であるにも関わらず DestinationType が ToServer になっている" ) ;
				}
			}
			else
			{
				// 転送ダウン

				if( frame.DestinationType == DestinationTypes.Broadcast || frame.DestinationType == DestinationTypes.Multicast )
				{
					// ※現在は廃止されたのでここに来る事は無い

					// 転送アップフレームを送信する
					SendReceivingFrame_FromSessionProcessor_Private
					(
						frame.PacketType,
						frame.Data,
						frame.DestinationType,
						frame.DestinationUserIds,
						frame.SourceType,
						frame.SourceUserId
					) ;
				}
				else
				if( frame.DestinationType == DestinationTypes.ToServer )
				{
					// セッションプロセッサーが有効であれば受け渡す
					if( m_SessionProcessor != null )
					{
						CallOnReceived_ForSessionProcessor( frame.Data, frame.SourceUserId ) ;
					}
				}
			}
		}

		// 転送アップフレームを送信する(セッションプロセッサー専用)
		private bool SendReceivingFrame_FromSessionProcessor
		(
			PacketTypes packetType,
			byte[] data,
			DestinationTypes destinationType,
			string[] destinationUserIds,
			SourceTypes sourceType,
			string sourceUserId
		)
		{
			return SendReceivingFrame_FromSessionProcessor_Private
			(
				packetType,
				data,
				destinationType,
				destinationUserIds,
				sourceType,
				sourceUserId
			) ;
		}

		// 転送アップフレームを送信する
		public bool SendReceivingFrame_FromSessionProcessor_Private
		(
			PacketTypes packetType,
			byte[] data,
			DestinationTypes destinationType,
			string[] destinationUserIds,
			SourceTypes sourceType,
			string sourceUserId
		)
		{
			List<SessionPlayerData> sessionPlayers = null ;

			if( destinationType == DestinationTypes.Broadcast )
			{
				// セッション内の有効な全プレイヤーを対象とした送信
				sessionPlayers = m_SessionPlayers ;
			}
			else
			if( destinationType == DestinationTypes.Multicast )
			{
				// セッション内の有効な指定したプレイヤーを対象とした送信

				sessionPlayers = new () ;

				foreach( var destinationUserId in destinationUserIds )
				{
					var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.UserId == destinationUserId ) ;
					if( sessionPlayer != null )
					{
						sessionPlayers.Add( sessionPlayer ) ;
					}
				}
			}
			else
			if( destinationType == DestinationTypes.Unicast )
			{
				sessionPlayers = new () ;

				var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.UserId == destinationUserIds[ 0 ] ) ;
				if( sessionPlayer != null )
				{
					sessionPlayers.Add( sessionPlayer ) ;
				}
			}
			else
			if( destinationType == DestinationTypes.ToHost )
			{
				sessionPlayers = new () ;

				var sessionPlayer = m_SessionPlayers.FirstOrDefault( _ => _.UserId == m_UserId ) ;
				if( sessionPlayer != null )
				{
					sessionPlayers.Add( sessionPlayer ) ;
				}
			}
			else
			{
				// サーバー指定は不可
				return false ;
			}

			if( sessionPlayers == null || sessionPlayers.Count == 0 )
			{
				Debug.LogWarning( "転送可能な対象が存在しない" ) ;
				return false ;
			}

			//---------------------------------------------------------
			// 転送アップ

			// 転送アップフレームを対象のプレイヤーそれぞれにユニキャストする

			bool isMyself = false ;
			foreach( var sessionPlayer in sessionPlayers )
			{
				if( sessionPlayer.UserId == m_UserId )
				{
					// 自身に対しては最後にループバック
					isMyself = true ;
				}
				else
				{
					// フレームのバックアップ用バッファへの蓄積とフレームの再送要求がかち合うので排他制御が必要
					// 自身の送信とホスト管理としての送信が存在するのでそちらの排他制御も必要
					lock( m_SendingFrameLockObject )
					{
						// 転送アップの上りフレームを生成する
						var relayFrame = new UpstreamFrameData
						(
							m_SendingFrameSequence,
							true,	// 転送アップ
							destinationType,
							new string[]{ sessionPlayer.UserId },
							sourceType,
							sourceUserId,
							data
						) ;

						SendFrame_Private( relayFrame, packetType ) ;

						//----------------------------------------------------------

						if( m_UdpEnabled == true && m_UdpCorrectionEnabled == true )
						{
							// フレームをバックアップ用送信バッファに貯める
							AddSendingFrame( relayFrame ) ;
						}

						//----------------------------------------------------------

						if( packetType == PacketTypes.UDP )
						{
							// 転送アップフレームのユニキャスト対象毎に送信シーケンスは増加させる
							m_SendingFrameSequence ++ ;
						}
					}
				}
			}

			if( isMyself == true )
			{
				// ローカルループバック(Broadcast と Multicast のみここに来るため通常の受信のみ処理すれば良い)

//				Debug.Log( "<color=#7FFF7F>--------->[SessionServer] ホストが宛ての送信なのでローカルループバックを行うしかない : 送信先タイプ = " + destinationType + " 送信元タイプ = " + sourceType + " 送信元ユーザー識別子 = " + sourceUserId + "</color>" ) ;

				// 不特定対象向けフレーム(→フレームはここが終点)
				CallOnReceived( data, sourceType, sourceUserId ) ;

				// ※ループバックの場合は実際は通信を行っていないため m_LastSendTime を更新してはならない
			}

			//----------------------------------------------------------

			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// クライアント用のコールバックヘルパー

		// 受信コールバックのヘルパー
		private void CallOnPlayerJoined( SessionPlayer sessionPlayer )
		{
			if( m_MainThreadContext != null )
			{
				// メインスレッドによる縛り：あり

				if( SynchronizationContext.Current == m_MainThreadContext )
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerJoined はメインスレッドで呼ばれた</color>" ) ;
					CallOnPlayerJoined_Inner( sessionPlayer ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerJoined はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnPlayerJoined_Inner( sessionPlayer ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnPlayerJoined_Inner( sessionPlayer ) ;
			}

			void CallOnPlayerJoined_Inner( SessionPlayer sessionPlayer )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_OnPlayerJoined?.Invoke( sessionPlayer );
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnPlayerLeft( SessionPlayer sessionPlayer )
		{
			if( m_MainThreadContext != null )
			{
				// メインスレッドによる縛り：あり

				if( SynchronizationContext.Current == m_MainThreadContext )
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerLeft はメインスレッドで呼ばれた</color>" ) ;
					CallOnPlayerLeft_Inner( sessionPlayer ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerLeft はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnPlayerLeft_Inner( sessionPlayer ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnPlayerLeft_Inner( sessionPlayer ) ;
			}

			void CallOnPlayerLeft_Inner( SessionPlayer sessionPlayer )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_OnPlayerLeft?.Invoke( sessionPlayer ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnDisconnected()
		{
			if( m_MainThreadContext != null )
			{
				// メインスレッドによる縛り：あり

				if( SynchronizationContext.Current == m_MainThreadContext )
				{
//					Debug.Log( "<color=#FFFF00>CallOnDisconnected はメインスレッドで呼ばれた</color>" ) ;
					CallOnDisconnected_Inner() ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnDisconnected はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnDisconnected_Inner() ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnDisconnected_Inner() ;
			}

			void CallOnDisconnected_Inner()
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_OnDisconnected?.Invoke() ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnReceived( byte[] data, SourceTypes sourceType, string sourceUserId )
		{
			if( m_ReceivingCallbackType == ReceivingCallbackTypes.Passive )
			{
				// 受信コールバックは受動的に処理する

				if( m_MainThreadContext != null )
				{
					// メインスレッドによる縛り：あり

					if( SynchronizationContext.Current == m_MainThreadContext )
					{
//						Debug.Log( "<color=#00FFFF>[NetworkPlay] CallOnReceived はメインスレッドで呼ばれた</color>" ) ;
						CallOnReceived_Inner( data, sourceType, sourceUserId ) ;
					}
					else
					{
	//					Debug.Log( "<color=#00FF00>[NetworkPlay] CallOnReceived  はサブスレッドで呼ばれた</color>" ) ;
						m_MainThreadContext.Post( ( _ ) =>
						{
							// メインスレッドのタイミングで受信処理を実行する)
							CallOnReceived_Inner( data, sourceType, sourceUserId ) ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッドによる縛り：なし

					CallOnReceived_Inner( data, sourceType, sourceUserId ) ;
				}

				void CallOnReceived_Inner( byte[] data, SourceTypes sourceType, string sourceUserId )
				{
					try
					{
						// 不特定対象向けフレーム(→フレームはここが終点)
						m_OnReceived?.Invoke( data, sourceType, sourceUserId ) ;
					}
					catch( Exception )
					{
						throw ;
					}
				}
			}
			else
			if( m_ReceivingCallbackType == ReceivingCallbackTypes.Active )
			{
				// 受信コールバックは能動的に処理する

				lock( m_ActiveFrames_LockObject )
				{
					m_ActiveFrames.Add( new ActiveFrame( data, sourceType, sourceUserId ) ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// セッションプロセッサー用のコールバックヘルパー

		// 受信コールバックのヘルパー
		private void CallOnActive_ForSessionProcessor( SessionPlayer[] sessionPlayers )
		{
			if( m_MainThreadContext_ForSessionProcessor != null )
			{
				// メインスレッドによる縛り：あり
								
				if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
				{
//					Debug.Log( "<color=#FFFF00>CallOnActiveInSessionProcessor はメインスレッドで呼ばれた</color>" ) ;
					CallOnActive_ForSessionProcessor_Inner( sessionPlayers ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnActiveInSessionProcessor はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnActive_ForSessionProcessor_Inner( sessionPlayers ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnActive_ForSessionProcessor_Inner( sessionPlayers ) ;
			}

			void CallOnActive_ForSessionProcessor_Inner( SessionPlayer[] sessionPlayers )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_SessionProcessor?.OnActive( sessionPlayers ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnDeleted_ForSessionProcessor()
		{
			if( m_MainThreadContext_ForSessionProcessor != null )
			{
				// メインスレッドによる縛り：あり
								
				if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
				{
//					Debug.Log( "<color=#FFFF00>CallOnDeletedInSessionProcessor はメインスレッドで呼ばれた</color>" ) ;
					CallOnDeleted_ForSessionProcessor_Inner() ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnDeletedInSessionProcessor はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnDeleted_ForSessionProcessor_Inner() ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnDeleted_ForSessionProcessor_Inner() ;
			}

			void CallOnDeleted_ForSessionProcessor_Inner()
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_SessionProcessor?.OnDeleted() ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnPlayerJoined_ForSessionProcessor( SessionPlayer sessionPlayer )
		{
			if( m_MainThreadContext_ForSessionProcessor != null )
			{
				// メインスレッドによる縛り：あり
								
				if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerJoinedInSessionProcessor はメインスレッドで呼ばれた</color>" ) ;
					CallOnPlayerJoined_ForSessionProcessor_Inner( sessionPlayer ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerJoinedInSessionProcessor はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnPlayerJoined_ForSessionProcessor_Inner( sessionPlayer ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnPlayerJoined_ForSessionProcessor_Inner( sessionPlayer ) ;
			}

			void CallOnPlayerJoined_ForSessionProcessor_Inner( SessionPlayer sessionPlayer )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_SessionProcessor?.OnPlayerJoined( sessionPlayer ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		// 受信コールバックのヘルパー
		private void CallOnPlayerLeft_ForSessionProcessor( SessionPlayer sessionPlayer )
		{
			if( m_MainThreadContext_ForSessionProcessor != null )
			{
				// メインスレッドによる縛り：あり
								
				if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerLeftInSessionProcessor はメインスレッドで呼ばれた</color>" ) ;
					CallOnPlayerLeft_ForSessionProcessor_Inner( sessionPlayer ) ;
				}
				else
				{
//					Debug.Log( "<color=#FFFF00>CallOnPlayerLeftInSessionProcessor はサブスレッドで呼ばれた</color>" ) ;
					m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnPlayerLeft_ForSessionProcessor_Inner( sessionPlayer ) ;
					}, null ) ;
				}
			}
			else
			{
				// メインスレッドによる縛り：なし

				CallOnPlayerLeft_ForSessionProcessor_Inner( sessionPlayer ) ;
			}

			void CallOnPlayerLeft_ForSessionProcessor_Inner( SessionPlayer sessionPlayer )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_SessionProcessor?.OnPlayerLeft( sessionPlayer ) ;
				}
				catch( Exception )
				{
					throw ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// セッションプロセッサー受信処理のスレッド排他制御用のオブジェクト
		private readonly object	m_SessionProcessorLockObject = new () ;

		// セッションプロセッサー宛のフレームを受信処理中かどうか
		private bool			m_SessionProcessorReceiving ;

		// セッションプロセッサー宛のフレームのバッファ(すぐに処理できない場合はバッファに貯める)
		private readonly List<SessionProcessorReceivingFrameData>	m_SessionProcessorReceivingFrames = new () ;


		// 受信コールバック(セッションプロセッサー用)
		private void CallOnReceived_ForSessionProcessor( byte[] data, string sourceUserId )
		{
			lock( m_SessionProcessorLockObject )
			{
				// セッションプロセッサーの受信処理はサブスレッドで行う
				// バッファの増減があるためスレッドの排他制御を行う

				if( m_SessionProcessorReceiving == false )
				{
					// セッションプロセッサーの受信処理中ではない

					m_SessionProcessorReceiving  = true ;	// 受信中に移行する

					// サブスレッドで送信を試みる
					BeginSessionProcessorReceive
					(
						data,
						sourceUserId,
						SessionProcessorReceive_Callback
					) ;
				}
				else
				{
					// 送信中である

					// 送信バッファ群に積む
					m_SessionProcessorReceivingFrames.Add
					(
						new SessionProcessorReceivingFrameData
						(
							data,
							sourceUserId
						)
					) ;
				}
			}
		}

		// セッションプロセッサーの受信が終了した後に呼び出される(サブスレッド)
		private void SessionProcessorReceive_Callback()
		{
			// サブスレッドの排他制御
			lock( m_SessionProcessorLockObject )
			{
				if( m_SessionProcessorReceivingFrames.Count >  0 )
				{
					// フレームバッファにフレームが溜まっている

					// フレームを取り出す
					var frame = m_SessionProcessorReceivingFrames[ 0 ] ;
					m_SessionProcessorReceivingFrames.RemoveAt( 0 ) ;

					//------------------------------

					// 再び受信を実行する
					BeginSessionProcessorReceive
					(
						frame.Data,
						frame.SourceUserId,
						SessionProcessorReceive_Callback
					) ;
				}
				else
				{
					// 受信中ではなくなった
					m_SessionProcessorReceiving = false ;
				}
			}
		}

		// セッションプロセッサーの受信を処理する
		private bool BeginSessionProcessorReceive
		(
			byte[] data,
			string sourceUserId,
			Action onFinished
		)
		{
			// サブスレッドで送信する
			Task task = Task.Run( () => SessionProcessorReceiveAsync
			(
				data,
				sourceUserId,
				onFinished
			) ) ;
			if( task.IsFaulted == true || task.IsCanceled == true )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// セッションプロセッサーの受信を処理する(注意：戻り値は使用していない)
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="packetType"></param>
		/// <returns></returns>
		public void SessionProcessorReceiveAsync
		(
			byte[] data,
			string sourceUserId,
			Action onFinished
		)
		{
			if( m_ReceivingCallbackType_ForSessionProcessor == ReceivingCallbackTypes.Passive )
			{
				// 受信コールバックは受動的に処理する

				if( m_MainThreadContext_ForSessionProcessor != null )
				{
					// メインスレッドによる縛り：あり
								
					if( SynchronizationContext.Current == m_MainThreadContext_ForSessionProcessor )
					{
						Debug.Log( "<color=#00FFFF>CallOnReceivedInSessionProcessor は[スレッドの縛り：あり] - メインスレッドで呼ばれた</color>" ) ;
						CallOnReceived_ForSessionProcessor_Inner( data, sourceUserId ) ;
					}
					else
					{
						Debug.Log( "<color=#00FF00>CallOnReceivedInSessionProcessor は[スレッドの縛り：あり] - サブスレッドで呼ばれた</color>" ) ;
						m_MainThreadContext_ForSessionProcessor.Post( ( _ ) =>
						{
							// メインスレッドのタイミングで受信処理を実行する)
							CallOnReceived_ForSessionProcessor_Inner( data, sourceUserId ) ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッド縛り：なし

					Debug.Log( "<color=#00FF00>CallOnReceivedInSessionProcessor は[スレッドの縛り：なし]で呼ばれた</color>" ) ;
					CallOnReceived_ForSessionProcessor_Inner( data, sourceUserId ) ;
				}

				void CallOnReceived_ForSessionProcessor_Inner( byte[] data, string sourceUserId )
				{
					try
					{
						// 不特定対象向けフレーム(→フレームはここが終点)
						m_SessionProcessor.OnReceived( data, sourceUserId ) ;
					}
					catch( Exception )
					{
						throw ;
					}
				}
			}
			else
			if( m_ReceivingCallbackType_ForSessionProcessor == ReceivingCallbackTypes.Active )
			{
				// 受信コールバックは能動的に処理する

				lock( m_ActiveFrames_ForSessionProcessor_LockObject )
				{
					m_ActiveFrames_ForSessionProcessor.Add( new ActiveFrame_ForSessionProcessor( data, sourceUserId ) ) ;
				}
			}

			//----------------------------------

			if( m_OwnerCancellationToken != null && m_OwnerCancellationToken.IsCancellationRequested == true )
			{
				throw new OperationCanceledException() ;
			}

			//----------------------------------

			// 終了のコールバックを呼ぶ
			onFinished?.Invoke() ;
		}

		//-------------------------------------------------------------------------------------------

		// Kick パケットを送る
		private void SendKick( string userId )
		{
			SendCommand
			(
				CommandTypes.Kick,
				( List<byte> commandContentData ) =>
				{
					// 再送要求対象となるフレームのシーケンス
					DataFormat.PutString( commandContentData, userId ) ;
				},
				PacketTypes.TCP
			) ;
		}

		// KeepAlive パケットを送る
		// 送るタイミングは、 LastUpdateTime から５秒経過していたら
		private void SendKeepAlive( PacketTypes packetType )
		{
//			Debug.Log( "<color=#FF7FFF>KeepAlive の送信起点 : Ticks = " + ticks + " PacketType = " + packetType + "</color>" ) ;

			SendCommand
			(
				CommandTypes.KeepAlive,
				null,
				packetType
			) ;

			//----------------------------------------------------------

//			Debug.Log( "<color=#7FFF7F>KeepAlive 送信 [ PacketType = " + packetType + " ] </color>" ) ;
		}

		// Ping を送信する
		private void SendPing( bool isHost, long ticks, bool isTransfer, string sourceUserId, PacketTypes packetType )
		{
			if( packetType == PacketTypes.UDP && PingPacketType == PacketTypes.TCP )
			{
				// 強制的に Ping は TCP で送受信する
				packetType = PacketTypes.TCP ;
			}

			//----------------------------------------------------------

			SendCommand
			(
				CommandTypes.Ping,
				( List<byte> commandContentData ) =>
				{
					// サーバーまでの往復かホストまでの往復かを格納する
					DataFormat.PutBool( commandContentData, isHost ) ;

					// おおよその応答までの時間計測のため現在の時間を格納する
					DataFormat.PutLong( commandContentData, ticks ) ;

					if( isHost == true )
					{
						// ホストへの往復の Ping を計測する

						// isTransfer : true = ホスト宛(送信)・false = クライアント宛(返信)
						DataFormat.PutBool( commandContentData, isTransfer ) ;

						if( isTransfer == false )
						{
							// クライアントへの返信であるためクライアントの識別子を格納する必要がある
							DataFormat.PutString( commandContentData, sourceUserId ) ;
						}
					}
				},
				packetType
			) ;
		}

		//-------------------------------------------------------------------------------------------
		// セッションプロセッサー用の基本機能提供

		/// <summary>
		/// アクティブセッションに対して外部からの操作を提供するクラス
		/// </summary>
		public class SessionFunction : ISessionFunction
		{
			private readonly DefaultNetworkPlayClientAdapter m_Adapter ;

			//-----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="session"></param>
			public SessionFunction( DefaultNetworkPlayClientAdapter adapter )
			{
				m_Adapter = adapter ;
			}

			//-----------------------------------------------------------

			/// <summary>
			/// セッション内の全プレイヤー情報を取得する
			/// </summary>
			/// <returns></returns>
			public SessionPlayer[] GetSessionPlayers()
			{
				return m_Adapter.GetSessionPlayers() ;
			}

			/// <summary>
			/// セッション内のホストプレイヤーの情報を取得する
			/// </summary>
			/// <returns></returns>
			public SessionPlayer GetSessionHostPlayer()
			{
				return m_Adapter.GetSessionHostPlayer() ;
			}

			/// <summary>
			/// セッション内の指定したプレイヤーの情報を取得する
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public SessionPlayer GetSessionPlayer( string userId  )
			{
				return m_Adapter.GetSessionPlayer( userId ) ;
			}

			//----------------------------------

			/// <summary>
			/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
			/// </summary>
			/// <returns></returns>
			public int Dequeue()
			{
				return m_Adapter.Dequeue_ForSessionProcessor() ;
			}

			//-----------------------------------------------------------

			/// <summary>
			/// データを送信する
			/// </summary>
			/// <param name="destinationType"></param>
			/// <param name="destinationUserIds"></param>
			/// <param name="data"></param>
			public bool Send
			(
				byte[] data,
				DestinationTypes destinationType = DestinationTypes.Broadcast,
				params string[] destinationUserIds
			)
			{
				var packetType = m_Adapter.UdpEnabled == false ? PacketTypes.TCP : PacketTypes.UDP ;
				return Send( data, packetType, destinationType, destinationUserIds ) ;
			}

			/// <summary>
			/// データを送信する
			/// </summary>
			/// <param name="destinationType"></param>
			/// <param name="destinationUserIds"></param>
			/// <param name="data"></param>
			public bool Send
			(
				byte[] data,
				PacketTypes packetType,
				DestinationTypes destinationType = DestinationTypes.Broadcast,
				params string[] destinationUserIds
			)
			{
				// サーバーからの送信アクションを実行する
				if( destinationType == DestinationTypes.Multicast || destinationType == DestinationTypes.Unicast )
				{
					if( destinationUserIds == null ||  destinationUserIds.Length == 0 )
					{
						// 不可
						return false ;
					}
				}
				else
				if( destinationType == DestinationTypes.ToServer )
				{
					// 不可
					return false ;
				}

				return m_Adapter.SendReceivingFrame_FromSessionProcessor
				(
					packetType,
					data,
					destinationType,
					destinationUserIds,
					SourceTypes.FromServer,
					null
				) ;
			}

			/// <summary>
			/// 指定したユーザー識別子のプレイヤーをセッションからキックする
			/// </summary>
			/// <param name="userIds"></param>
			public bool Kick( string userId )
			{
				return m_Adapter.Kick( userId ) ;
			}
		}
	}
}
