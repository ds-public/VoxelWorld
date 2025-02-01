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
		// セッションサーバーのアドレス
		private string		m_SessionServerAddress ;

		/// <summary>
		/// セッションサーバーのアドレス
		/// </summary>
		public	string		SessionServerAddress	=> m_SessionServerAddress ;

		// セッションサーバーのポート
		private int			m_SessionServerPort ;

		/// <summary>
		/// セッションサーバーのポート
		/// </summary>
		public	int			SessionServerPort		=> m_SessionServerPort ;

		//-----------------------------------

		// セッション識別子
		private string		m_SessionId ;

		/// <summary>
		/// セッション識別子(Abstruct)
		/// </summary>
		public string	SessionId
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
		private CancellationTokenSource m_CancellationTokenSource_ForSessionServer ;


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
		public bool		UseKeepAlive = true ;

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
				if( string.IsNullOrEmpty( m_SessionId ) == true )
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
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType )
		{
			m_ReceivingCallbackType = receivingCallbackType ;
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

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
				}, null ) ;
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
		public void SetReceivingCallbackType_ForSessionProcessor( ReceivingCallbackTypes receivingCallbackType )
		{
			m_ReceivingCallbackType_ForSessionProcessor = receivingCallbackType ;
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

			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnReceivedInSessionProcessorInMainThred( data, sourceUserId ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnReceivedInSessionProcessorInMainThred( data, sourceUserId ) ;
				}, null ) ;
			}

			void CallOnReceivedInSessionProcessorInMainThred( byte[] data, string sourceUserId )
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

			return Send( packetType, data, destinationType, destinationUserIds ) ;
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
			PacketTypes packetType,
			byte[] data,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
		{
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
							return SendRelayFrame_Private
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
							return SendRelayFrame_Private
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

						// カスタムセッションプロセッサーが設定されている
						CallOnReceivedInSessionProcessor( data, m_UserId ) ;

						return true ;
					}
				}
			}

			//----------------------------------------------------------
			// フレームデータを生成する

			//----------------------------------
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

			var fakeFrame = new UpstreamFrameData
			(
				sequence,
				destinationType,
				userIds,
				data.Slice( offset, length )
			) ;
*/
			//----------------------------------

			// 通常アップの上りフレームを生成する
			var frame = new UpstreamFrameData
			(
				m_SendingFrameSequence,
				false,	// 通常アップ(ここでは通常アップ以外はありえない)
				destinationType,
				destinationUserIds,
				SourceTypes.FromClient,	// 通常アップでは意味無し
				null,					// 通常アップでは意味無し
				data
			) ;

			// 特定のフレームのみロスト扱いにする(デバッグ)
/*			if( m_SendingFrameSequence != 2 && m_SendingFrameSequence != 3 )
			{*/
				// フレームを送信する
				SendFrame_Private( frame, packetType ) ;
/*			}*/

//			Debug.Log( "<color=#7FFF7F>フレーム送信 シーケンス = " + m_SendingFrameSequence + "</color>" ) ;

			//--------------------------------------------------------------------------

			if( m_UdpEnabled == true && m_UdpCorrectionEnabled == true )
			{
				// フレームをバックアップ用送信バッファに貯める
				AddSendingFrame( frame ) ;
			}

			//----------------------------------------------------------

			// シーケンス番号増加
			m_SendingFrameSequence ++ ;

			//----------------------------------------------------------

			// 最後に送信時間を更新する
			m_LastSendTime = Timer.NowTicks ;

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
			if( string.IsNullOrEmpty( m_SessionId ) == true )
			{
				return ;
			}

			if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
			{
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
				m_CancellationTokenSource_ForSessionServer = new CancellationTokenSource() ;
			}
			else
			{
				m_CancellationTokenSource_ForSessionServer = CancellationTokenSource.CreateLinkedTokenSource( ownerCancellationToken ) ;
			}

			//----------------------------------------------------------

			// リアルタイム通信用のクライアントソケット生成
			m_RealTimeSocketClient = new SocketClient
			(
				OnTcpReceived_FromSessonServer,
				OnTcpDeiconnected_FromSessionServer,
				OnUdpReceived_FromSessionServer,
				m_MaxTcpPacketSize,
				m_CancellationTokenSource_ForSessionServer.Token
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

		// セッションサーバーにＴＣＰで接続する
		private async Task<bool> ConnectToSessionServer
		(
			string sessionServerAddress,
			int sessionServerPort,
			CancellationToken cancellationToken
		)
		{
			if( m_RealTimeSocketClient == null )
			{
				// リアルタイム通信用のソケットクライアントが生成されていない
				return false ;
			}

			//----------------------------------------------------------
			// タスク中断用のキャンセレーショントークン生成

			CancellationTokenSource cancellationTokenSource ;
			bool isCanceled ;

			//----------------------------------------------------------
			// セッションサーバーへの接続を行う

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForSessionServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForSessionServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			try
			{
				// セッションサーバーへＴＣＰ接続を行う(接続実行と受信開始)
				await m_RealTimeSocketClient.ConnectAsync( sessionServerAddress, sessionServerPort, null, cancellationTokenSource.Token ) ;
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

			Debug.Log( "<color=#FFFF00>無事に SessionServer に接続 : クライアント側のポート番号 = " + m_RealTimeSocketClient.GetTcpPort() + "</color>" ) ;

			//----------------------------------------------------------

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForSessionServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForSessionServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			//------------------------------------------------------------------------------------------

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

			Debug.Log( "<color=#FFFF00>SessionServer にバインド要求を送信した</color>" ) ;

			//----------------------------------------------------------
			// 応答としてのセッションプレイヤー情報受信を待つ

			// バインド要求を出している最中
			m_ClientPhase = ClientPhases.RequestBinding ;

			//----------------------------------------------------------

			// バインドが完了するまで待機する

			if( cancellationToken == default )
			{
				cancellationTokenSource = m_CancellationTokenSource_ForSessionServer ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForSessionServer.Token, cancellationToken ) ;
			}

			isCanceled = false ;

			while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForSessionServer != null )
			{
				if( m_CancellationTokenSource_ForSessionServer.IsCancellationRequested == true )
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

				m_SessionId = null ;
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

			Debug.Log( "<color=#FFFF00>SessionServer にバインド完了</color>" ) ;

			//------------------------------------------------------------------------------------------
			// ＵＤＰが有効であるなら KeepAlive を一定時間毎に送りつつ Ready を受信するのを待つ

			// 既に Ready になっていたらスキップする(ＵＤＰを使用しないケースではありえる)
			if( m_ClientPhase == ClientPhases.Connecting )
			{
				long baseTicks = 0 ;

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource_ForSessionServer ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource_ForSessionServer.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForSessionServer != null )
				{
					if( m_CancellationTokenSource_ForSessionServer.IsCancellationRequested == true )
					{
						isCanceled = true ;
						break ;
					}

					if( m_UdpEnabled == true )
					{
						// ＵＤＰが有効

						if( ( Timer.NowTicks - baseTicks ) >  250 )
						{
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

					m_SessionId = null ;
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

			while( m_RealTimeSocketClient != null && m_CancellationTokenSource_ForSessionServer != null )
			{
				if( m_CancellationTokenSource_ForSessionServer.IsCancellationRequested == true )
				{
					// タスクがキャンセルされた
					isCanceled = true ;
					break ;
				}

				if( m_ClientPhase == ClientPhases.Ready )
				{
					// 接続状態を継続している
					if( UseKeepAlive == true )
					{
						if( ( Timer.NowTicks - m_LastSendTime ) >= 5000 )
						{
							// ５秒経過
							SendKeepAlive( packetType ) ;
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

						CallOnPlayerLeftInSessionProcessor( new
						(
							sessionPlayer.UserId,
							PlayerName,
							sessionPlayer.IsGuest,
							sessionPlayer.IsHost
						) ) ;

						// 自身が最後の１人のホストである場合はセッションプロセッサーにも通知
						CallOnDeletedInSessionProcessor() ;
					}
				}
			}

			//----------------------------------------------------------
			// 後始末を行う

			m_SessionPlayers = null ;
			m_ClientPhase = ClientPhases.None ;

			DeleteRealTimeSocketClient() ;

			m_SessionId = null ;

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
			if( m_CancellationTokenSource_ForSessionServer != null )
			{
				if( m_CancellationTokenSource_ForSessionServer.IsCancellationRequested == false )
				{
					m_CancellationTokenSource_ForSessionServer.Cancel() ;
				}

				m_CancellationTokenSource_ForSessionServer.Dispose() ;
				m_CancellationTokenSource_ForSessionServer = null ;
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
			if( string.IsNullOrEmpty( m_SessionId ) == true )
			{
				return ;
			}

			if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
			{
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
				if( string.IsNullOrEmpty( m_SessionId ) == true )
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

		// ＴＣＰパケットを受信した際に呼び出されるコールバック
		private void OnTcpReceived_FromSessonServer( byte[] data )
		{
			if( m_ClientPhase != ClientPhases.RequestBinding && m_ClientPhase != ClientPhases.Connecting && m_ClientPhase != ClientPhases.Ready )
			{
				// 異常な状態でコールバックが呼ばれたので無視する
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			// 全体で復号化する
			byte[] commandData = m_Crypter.DecryptXor( data ) ;

			if( commandData == null || commandData.Length <  1 )
			{
				// 不正レスポンス(結果として切断する)
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			var commandType = ( CommandTypes )commandData[ 0 ] ;

//			Debug.Log( "<color=#FFFF00>--------------> コマンド受信 = " + commandType + "</color>" ) ;

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
							for( index  = 0 ; index < count ; index ++ )
							{
								sessionPlayer = new SessionPlayerData() ;
								sessionPlayer.Decode( commandData, ref offset ) ;

								m_SessionPlayers.Add( sessionPlayer ) ;
							}
						}
					}
					catch( Exception )
					{
						// データ異常
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
								CallOnPlayerJoinedInSessionProcessor( sessionPlayer ) ;
							}
						}

						//-------------------------------

						// 成功
						return ;
					}
					catch( Exception )
					{
						// データ異常
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
										CallOnActiveInSessionProcessor( GetSessionPlayers() ) ;
									}
									else
									{
										// 既に自身はホストになっている
										CallOnPlayerLeftInSessionProcessor( sessionPlayer ) ;
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
						m_ClientPhase = ClientPhases.Disconnecting ;

						// 失敗
						return ;
					}
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
					OnFrameReceived( commandData, ref offset, PacketTypes.TCP ) ;

					return ;
				}
				else
				if( commandType == CommandTypes.Retransmission )
				{
					// フレーの再送要求を受信(TCP)

					ushort sequence ;

					try
					{
						int offset = 1 ;

						// 再送要求の対象となるフレームのシーケンス
						sequence = DataFormat.GetUShort( commandData, ref offset ) ;

					}
					catch( Exception )
					{
						// データ異常
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
						SendFrame_Private( frame, PacketTypes.TCP ) ;	// ＴＣＰ固定

						Debug.Log( "<color=#FF7FAF>上りフレーム再送信 " + sequence + "</color>" ) ;

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
			}

			//----------------------------------------------------------

			// ここに来るのはいずれのコマンドにも該当しなかったという事なので不正アクセス
			m_ClientPhase = ClientPhases.Disconnecting ;
		}

		// ＴＣＰが切断された際に呼び出されるコールバック
		private void OnTcpDeiconnected_FromSessionServer()
		{
//			Debug.Log( "<color=#FF0000>セッションサーバーから切断された</color>" ) ;

			// 切断状態
			m_ClientPhase = ClientPhases.Disconnecting ;
		}

		// ＵＤＰパケットを受信した際に呼び出されるコールバック
		private void OnUdpReceived_FromSessionServer( byte[] data, string serverAddress, int serverPort )
		{
			if( m_ClientPhase != ClientPhases.Ready )
			{
				// 異常な状態でコールバックが呼ばれたので無視する
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			// 全体で復号化する
			byte[] commandData = m_Crypter.DecryptXor( data ) ;

			if( commandData == null || commandData.Length <  1 )
			{
				// 不正レスポンス(結果として切断する)
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			var commandType = ( CommandTypes )commandData[ 0 ] ;

			if( commandType != CommandTypes.Frame )
			{
				// ＵＤＰではフレーム以外の受信を許容しない

				// 異常な状態でコールバックが呼ばれたので無視する
				m_ClientPhase = ClientPhases.Disconnecting ;
				return ;
			}

			//----------------------------------------------------------

			int offset = 1 ;

			// ＴＣＰとＵＤＰの共通のフレーム受信処理
			OnFrameReceived( commandData, ref offset, PacketTypes.UDP ) ;
		}

		// フレーム受信時のＴＣＰとＵＤＰの共通処理(どちらで受信したかわかるようにする)
		private void OnFrameReceived( byte[] commandData, ref int offset, PacketTypes packetType )
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
					}
					else
					if( destinationType == DestinationTypes.Multicast )
					{
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

			//--------------------------------
			// 受信したフレームは一応問題なしと判断する

			// バッファに貯めつつ問題がなければ受信コールバックを呼び出す

			// １ループにつき１フレームのみ処理する(同一描画フレーム内で複数の通信フレームを処理させない)
			ProcessReceivingFrames
			(
				sequence,
				isTransfer,
				destinationType,
				destinationUserIds,
				sourceType,
				sourceUserId,
				data,

				packetType	// ホストの転送アップでＴＣＰとＵＤＰのどちらの方法を使用するか選択の必要があるため必要
			) ;
		}

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

			// アクセストークン
			DataFormat.PutString( command, m_AccessToken ) ;

			//----------------------------------
			// 暗号部分

			var commandContentData = new List<byte>() ;

			// セッション識別子
			DataFormat.PutString( commandContentData, m_SessionId ) ;

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
				m_RealTimeSocketClient.SendUdp( commandData, m_SessionServerAddress, m_SessionServerPort ) ;
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

		//-----------------------------------------------------------

		// 送信バッファの排他制御用のオブジェクト
		private readonly object	m_SendingFrameLockObject = new () ;

		// 送信済みフレームのバックアップ(再送要求用)　※ＵＤＰが有効な時のみ使用
		private void AddSendingFrame( UpstreamFrameData frame )
		{
			// フレームのバックアップ用バッファへの蓄積とフレームの再送要求がかち合うので排他制御が必要
			lock( m_SendingFrameLockObject )
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

			PacketTypes			packetType	// ＴＣＰとＵＤＰのどちらで受信したか
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

			if( m_UdpCorrectionEnabled == false )
			{
				// 補正不要なのでそのままフレームを対象プレイヤーに受け渡す
				PostReceivingFrame( frame ) ;
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
					PostReceivingFrame( frame ) ;

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
							PostReceivingFrame( frame ) ;

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
								m_ClientPhase = ClientPhases.Disconnecting ;
								return ;
							}
						}
					}
				}

				// タスクの中断対応(スレッドの負荷を軽くするために１０ミリ秒はスリープする)
				await Task.Delay( 10, m_CancellationTokenSource_ForSessionServer.Token ) ;
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
		private void PostReceivingFrame( DownstreamFrameData frame )
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
			}
			else
			{
				// 転送ダウン

				if( frame.DestinationType == DestinationTypes.Broadcast || frame.DestinationType == DestinationTypes.Multicast )
				{
					// 転送アップフレームを送信する
					SendRelayFrame_Private
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
					CallOnReceivedInSessionProcessor( frame.Data, frame.SourceUserId ) ;
				}
			}
		}

		// 転送アップフレームを送信する
		private bool SendRelayFrame
		(
			PacketTypes packetType,
			byte[] data,
			DestinationTypes destinationType,
			string[] destinationUserIds,
			SourceTypes sourceType,
			string sourceUserId
		)
		{
			return SendRelayFrame_Private
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
		public bool SendRelayFrame_Private
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

			foreach( var sessionPlayer in sessionPlayers )
			{
				if( sessionPlayer.UserId != m_UserId )
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

					// 転送アップフレームのユニキャスト対象毎に送信シーケンスは増加させる
					m_SendingFrameSequence ++ ;
				}
				else
				{
					// ローカルループバック(Broadcast と Multicast のみここに来るため通常の受信のみ処理すれば良い)

//					Debug.Log( "<color=#7FFF7F>--------->[SessionServer] ホストが宛ての送信なのでローカルループバックを行うしかない : 送信先タイプ = " + destinationType + " 送信元タイプ = " + sourceType + " 送信元ユーザー識別子 = " + sourceUserId + "</color>" ) ;

					// 不特定対象向けフレーム(→フレームはここが終点)
					CallOnReceived( data, sourceType, sourceUserId ) ;

					// ※ループバックの場合は実際は通信を行っていないため m_LastSendTime を更新してはならない
				}
			}

			//----------------------------------------------------------

			return true ;
		}

		//-------------------------------------------------------------------------------------------
		// クライアント用のコールバックヘルパー

		// 受信コールバックのヘルパー
		private void CallOnPlayerJoined( SessionPlayer sessionPlayer )
		{
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnPlayerJoinedInMainThred( sessionPlayer ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnPlayerJoinedInMainThred( sessionPlayer ) ;
				}, null ) ;
			}

			void CallOnPlayerJoinedInMainThred( SessionPlayer sessionPlayer )
			{
				try
				{
					// 不特定対象向けフレーム(→フレームはここが終点)
					m_OnPlayerJoined?.Invoke( sessionPlayer ) ;
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
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnPlayerLeftInMainThred( sessionPlayer ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnPlayerLeftInMainThred( sessionPlayer ) ;
				}, null ) ;
			}

			void CallOnPlayerLeftInMainThred( SessionPlayer sessionPlayer )
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
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnDisconnectedInMainThred() ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnDisconnectedInMainThred() ;
				}, null ) ;
			}

			void CallOnDisconnectedInMainThred()
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

				var context = SynchronizationContext.Current ;

				if( m_MainContext == null || context == m_MainContext )
				{
					// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

	//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
					CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
				}
				else
				{
	//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
					m_MainContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnReceivedInMainThred( data, sourceType, sourceUserId ) ;
					}, null ) ;
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
		private void CallOnActiveInSessionProcessor( SessionPlayer[] sessionPlayers )
		{
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnActiveInSessionProcessorInMainThred( sessionPlayers ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnActiveInSessionProcessorInMainThred( sessionPlayers ) ;
				}, null ) ;
			}

			void CallOnActiveInSessionProcessorInMainThred( SessionPlayer[] sessionPlayers )
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
		private void CallOnDeletedInSessionProcessor()
		{
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnDeletedInSessionProcessorInMainThred() ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnDeletedInSessionProcessorInMainThred() ;
				}, null ) ;
			}

			void CallOnDeletedInSessionProcessorInMainThred()
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
		private void CallOnPlayerJoinedInSessionProcessor( SessionPlayer sessionPlayer )
		{
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnPlayerJoinedInSessionProcessorInMainThred( sessionPlayer ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnPlayerJoinedInSessionProcessorInMainThred( sessionPlayer ) ;
				}, null ) ;
			}

			void CallOnPlayerJoinedInSessionProcessorInMainThred( SessionPlayer sessionPlayer )
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
		private void CallOnPlayerLeftInSessionProcessor( SessionPlayer sessionPlayer )
		{
			var context = SynchronizationContext.Current ;

			if( m_MainContext == null || context == m_MainContext )
			{
				// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
				CallOnPlayerLeftInSessionProcessorInMainThred( sessionPlayer ) ;
			}
			else
			{
//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
				m_MainContext.Post( ( _ ) =>
				{
					// メインスレッドのタイミングで受信処理を実行する)
					CallOnPlayerLeftInSessionProcessorInMainThred( sessionPlayer ) ;
				}, null ) ;
			}

			void CallOnPlayerLeftInSessionProcessorInMainThred( SessionPlayer sessionPlayer )
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

		// 受信コールバックのヘルパー
		private void CallOnReceivedInSessionProcessor( byte[] data, string sourceUserId )
		{
			if( m_ReceivingCallbackType_ForSessionProcessor == ReceivingCallbackTypes.Passive )
			{
				// 受信コールバックは受動的に処理する

				var context = SynchronizationContext.Current ;

				if( m_MainContext == null || context == m_MainContext )
				{
					// メインスレッド実行の指定が無いかメインスレッドで呼び出されれている

	//				Debug.Log( "<color=#FFFF00>DrawLines はメインスレッドで呼ばれた</color>" ) ;
					CallOnReceivedInSessionProcessorInMainThred( data, sourceUserId ) ;
				}
				else
				{
	//				Debug.Log( "<color=#FFFF00>DrawLines はサブスレッドで呼ばれた</color>" ) ;
					m_MainContext.Post( ( _ ) =>
					{
						// メインスレッドのタイミングで受信処理を実行する)
						CallOnReceivedInSessionProcessorInMainThred( data, sourceUserId ) ;
					}, null ) ;
				}

				void CallOnReceivedInSessionProcessorInMainThred( byte[] data, string sourceUserId )
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
			/// 受信コールバックタイプを設定する(受動的か能動的か)
			/// </summary>
			/// <param name="receivingCallbackType"></param>
			public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType )
			{
				m_Adapter.SetReceivingCallbackType_ForSessionProcessor( receivingCallbackType ) ;
			}

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
				return Send( packetType, data, destinationType, destinationUserIds ) ;
			}

			/// <summary>
			/// データを送信する
			/// </summary>
			/// <param name="destinationType"></param>
			/// <param name="destinationUserIds"></param>
			/// <param name="data"></param>
			public bool Send
			(
				PacketTypes packetType,
				byte[] data,
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

				return m_Adapter.SendRelayFrame
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
