using System ;
using System.Collections.Generic ;

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
		// コミュニケーションサーバーのアドレス
		private string	m_CommunicationServerAddress ;

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string	CommunicationServerAddress	=> m_CommunicationServerAddress ;

		// コミュニケーションサーバーのポート
		private int		m_CommunicationServerPort ;

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public int		CommunicationServerPort		=> m_CommunicationServerPort ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 任意の機能を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<CallFunction_Response> CallFunctionAsync
		(
			byte[] data,
			CancellationToken cancellationToken = default
		)
		{
			if( string.IsNullOrEmpty( ApplicationId ) == true )
			{
				return new ( ResponseCodes.InvalidApplicationIdentifier, "アプリケーション識別子が設定されていません", null ) ;
			}

			//----------------------------------

			// リクエストコンテント部
			var requestContent = new CallFunction_RequestPacket
			(
				m_ApplicationId,
				data
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServerAddress,
				m_CommunicationServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				Debug.Log( "-----------------エラー発生 : Code = " + responseCode + " ErrorMessahe = " + errorMessage ) ;

				// 失敗
				return new ( responseCode, errorMessage, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CallFunction_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			data = responseContent.Data ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, data ) ;
		}

		/// <summary>
		/// セッションを生成する
		/// </summary>
		/// <param name="sessionProcessionType"></param>
		/// <param name="password"></param>
		/// <param name="sessionScopeType"></param>
		/// <param name="maxPlayers"></param>
		/// <param name="sessionDescription"></param>
		/// <param name="udpEnabled"></param>
		/// <param name="udpCorrectionEnabled"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<CreateSession_Response> CreateSessionAsync
		(
			string					description,
			int						maxPlayers,
			string					password,
			SessionScopeTypes		scopeType,
			SessionManagementTypes	managementType,
			bool					udpEnabled,
			bool					udpCorrectionEnabled,
			string					playerName,
			CancellationToken		cancellationToken	
		)
		{
			if( string.IsNullOrEmpty( m_ApplicationId ) == true  )
			{
				// 無効なアプリケーション識別子
				return new
				(
					ResponseCodes.InvalidApplicationIdentifier, "無効なアプリケーション識別子です",
					null,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			if( string.IsNullOrEmpty( m_SessionId ) == false )
			{
				// 既にセッションに参加している(NetworkPlayClientインスタンス１つにつき参加できるセッションは１つまで)
				return new
				(
					ResponseCodes.AlreadyJoinedSession, "既にセッションに参加しています",
					null,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new CreateSession_RequestPacket
			(
				m_ApplicationId,
				description,
				maxPlayers,
				password,
				scopeType,
				managementType,
				udpEnabled,
				udpCorrectionEnabled,
				playerName
			) ;

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServerAddress,
				m_CommunicationServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage,
					null,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreateSession_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new
				(
					ResponseCodes.BadResponse, "受信データに問題があります",
					null,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0
				) ;
			}

			// レスポンスの値を取り出す
			m_SessionId				= responseContent.SessionId ;

			m_MaxPlayers			= responseContent.MaxPlayers ;

			m_ManagementType		= responseContent.ManagementType ;
			m_UdpEnabled			= responseContent.UdpEnabled ;
			m_UdpCorrectionEnabled	= responseContent.UdpCorrectionEnabled ;

			// サーバーのセッションプロセッサーが使用可能かどうか
			bool processorEnabled	= responseContent.ProcessorEnabled ;		// SessionProcessor を Server で生成する場合

			m_SessionServerAddress	= responseContent.SessionServerAddress ;
			m_SessionServerPort		= responseContent.SessionServerPort ;

			//----------------------------------------------------------
			// ※共通化したいが呼び出し元とコードがほとんど変わらないので共通化は断念

			// SessionServer と通信するリアルタイム通信用ソケットクライアントを生成する
			CreateRealTimeSocketClient( m_OwnerCancellationToken ) ; 

			bool isConnected = false ;
			bool isCanceled = false ;

			try
			{
				// セッションサーバーへＴＣＰ接続を行う
				isConnected = await ConnectToSessionServer( m_SessionServerAddress, m_SessionServerPort, cancellationToken ) ;
			}
			catch( Exception e )
			{
				if( e is OperationCanceledException )
				{
					// キャンセルされた
					isCanceled = true ;
				}
			}

			if( isCanceled == true )
			{
				// タスクがキャンセルされた
				throw new OperationCanceledException() ;
			}

			if( isConnected == false )
			{
				// セッションサーバーへの接続に失敗した
				await LeaveFromSessionAsync() ;	// こちらが失敗しても結果は無視する

				return new
				(
					ResponseCodes.CouldNotConnectToSessionServer, "セッションサーバーに接続できません",
					null,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			try
			{
				// 接続完了のコールバックを呼ぶ
				m_OnConnected?.Invoke() ;
			}
			catch( Exception )
			{
				throw ;
			}

			//----------------------------------------------------------
			// セッションプロセッサーを Host で生成する場合はここで設定する(Host の場合は生成済みのものが設定されるため)
			// ※ホストでなくても一律実行する

			if( m_ManagementType == SessionManagementTypes.HostManagement )
			{
				// ホストであるため
				processorEnabled = WakeupSessionProcessor() ;

				if( processorEnabled == true )
				{
					CallOnActiveInSessionProcessor( new SessionPlayer[]{ new ( UserId, PlayerName, false, true ) } ) ;
				}
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				m_SessionId,
				m_MaxPlayers, m_ManagementType,
				m_UdpEnabled, m_UdpCorrectionEnabled, processorEnabled,
				m_SessionServerAddress, m_SessionServerPort
			) ;
		}

		/// <summary>
		/// セッションに参加する
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="sessionPassword"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<JoinToSession_Response> JoinToSessionAsync
		(
			string sessionId,
			string password,
			string playerName,
			CancellationToken cancellationToken	
		)
		{
			if( string.IsNullOrEmpty( sessionId ) == true  )
			{
				// 無効なセッション識別子
				return new
				(
					ResponseCodes.InvalidSessionIdentifier, "無効なセッション識別子です",
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0,
					null
				) ;
			}

			if( string.IsNullOrEmpty( m_SessionId ) == false )
			{
				// 既にセッションに参加している(NetworkPlayClientインスタンス１つにつき参加できるセッションは１つまで)
				return new
				(
					ResponseCodes.AlreadyJoinedSession, "既にセッションに参加しています",
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0,
					null
				) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new JoinToSession_RequestPacket
			(
				sessionId,
				password,
				playerName
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServerAddress,
				m_CommunicationServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage,
					0, SessionManagementTypes.HostManagement,
					false, false, false,
					null, 0,
					null
				) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new JoinToSession_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new
				(
					ResponseCodes.BadResponse, "受信データに問題があります",
					0, SessionManagementTypes.HostManagement, 
					false, false, false,
					null, 0,
					null
				) ;
			}

			// 一旦保持しておく
			m_SessionId						= sessionId ;

			m_MaxPlayers					= responseContent.MaxPlayers ;
			m_ManagementType				= responseContent.ManagementType ;

			// レスポンスの値を取り出す
			m_UdpEnabled					= responseContent.UdpEnabled ;
			m_UdpCorrectionEnabled			= responseContent.UdpCorrectionEnabled ;

			// ホストまたはサーバーのセッションプロセッサーが使用可能な状態かどうか
			bool processorEnabled			= responseContent.ProcessorEnabled ;

			m_SessionServerAddress			= responseContent.SessionServerAddress ;
			m_SessionServerPort				= responseContent.SessionServerPort ;

			// セッションに参加中のプレイヤー群
			var sessionPlayers = responseContent.GetSessionPlayers() ;

			//----------------------------------------------------------
			// ※共通化したいが呼び出し元とコードがほとんど変わらないので共通化は断念

			// SessionServer と通信するリアルタイム通信用ソケットクライアントを生成する
			CreateRealTimeSocketClient( m_OwnerCancellationToken ) ; 

			bool isConnected = false ;
			bool isCanceled = false ;

			try
			{
				// セッションサーバーへＴＣＰ接続を行う
				isConnected = await ConnectToSessionServer( m_SessionServerAddress, m_SessionServerPort, cancellationToken ) ;
			}
			catch( Exception e )
			{
				if( e is OperationCanceledException )
				{
					// キャンセルされた
					isCanceled = true ;
				}
			}

			if( isCanceled == true )
			{
				// タスクがキャンセルされた
				throw new OperationCanceledException() ;
			}

			if( isConnected == false )
			{
				// セッションサーバーへの接続に失敗した

				// 以降の処理が失敗してもローカルのセッション識別子はクリアする
				m_SessionId = null ;

				return new
				(
					ResponseCodes.CouldNotConnectToSessionServer, "セッションサーバーに接続できません",
					m_MaxPlayers, SessionManagementTypes.HostManagement, 
					m_UdpEnabled, m_UdpCorrectionEnabled, processorEnabled,
					m_SessionServerAddress, m_SessionServerPort, sessionPlayers
				) ;
			}

			//----------------------------------------------------------

			try
			{
				// 接続完了のコールバックを呼ぶ
				m_OnConnected?.Invoke() ;
			}
			catch( Exception )
			{
				throw ;
			}

			//----------------------------------------------------------
			// セッションプロセッサーを Host で生成する場合はここで設定する(Host の場合は生成済みのものが設定されるため)
			// ※ホストでなくても一律実行する

			if( m_ManagementType == SessionManagementTypes.HostManagement )
			{
				WakeupSessionProcessor() ;
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				m_MaxPlayers, m_ManagementType, 
				m_UdpEnabled, m_UdpCorrectionEnabled, processorEnabled,
				m_SessionServerAddress, m_SessionServerPort,
				sessionPlayers
			) ;
		}

		// ホスト用のセッションプロセッサーを起動する
		private bool WakeupSessionProcessor()
		{
			if( m_SessionProcessor != null )
			{
				// ホストのセッションプロセッサーは使用できる

				// セッション生成コールバックを呼び出す
				m_SessionProcessor.OnCreated( new SessionFunction( this ) ) ;

				return true ;
			}
			else
			{
				// ホストのセッションプロセッサーは使用できない
				return false ;
			}
		}

		/// <summary>
		/// セッション情報を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<GetSessions_Response> GetSessionsAsync
		(
			int offset,
			int length,
			CancellationToken cancellationToken
		)
		{
			if( string.IsNullOrEmpty( m_ApplicationId ) == true  )
			{
				// 無効なアプリケーション識別子
				return new ( ResponseCodes.InvalidApplicationIdentifier, "無効なアプリケーション識別子です", null ) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new GetSessions_RequestPacket
			(
				m_ApplicationId,
				offset,
				length
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServerAddress,
				m_CommunicationServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new GetSessions_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			var sessions = responseContent.Sessions ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, sessions ) ;
		}

		/// <summary>
		/// フレンド情報取得を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<GetFriends_Response> GetFriendsAsync
		(
			int offset = 0,
			int length = 0,
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new GetFriends_RequestPacket
			(
				offset,
				length
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServerAddress,
				m_CommunicationServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				Debug.Log( "-----------------エラー発生 : Code = " + responseCode + " ErrorMessahe = " + errorMessage ) ;

				// 失敗
				return new ( responseCode, errorMessage, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new GetFriends_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			var friends = responseContent.Friends ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, friends ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 任意機能の実行の要求パケット
		/// </summary>
		public class CallFunction_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アプリケーション識別子
			/// </summary>
			public string			ApplicationId { get ; private set ; }

			/// <summary>
			/// 任意リクエストデータ
			/// </summary>
			public byte[]			Data { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CallFunction_RequestPacket
			(
				string	applicationId,
				byte[]	data
			)
			{
				RequestType		= RequestTypes.CallFunction ;

				//-------------

				ApplicationId	= applicationId ;
				Data			= data ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( ApplicationId ) ;
				PutByteArray( Data ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// 任意機能の実行の応答パケット
		/// </summary>
		public class CallFunction_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// 任意レスポンスデータ
			/// </summary>
			public byte[]	Data { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CallFunction_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					Data = GetByteArray() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッション生成の要求パケット
		/// </summary>
		public class CreateSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アプリケーション識別子
			/// </summary>
			public string					ApplicationId { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの説明文
			/// </summary>
			public string					Description { get ; private set ; }

			/// <summary>
			/// セッションの最大参加可能人数
			/// </summary>
			public int						MaxPlayers { get ; private set ; }

			/// <summary>
			/// セッションのパスワード
			/// </summary>
			public string					Password { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの情報公開範囲
			/// </summary>
			public SessionScopeTypes		ScopeType { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの処理種別
			/// </summary>
			public SessionManagementTypes	ManagementType { get ; private set ; }

			/// <summary>
			/// セッションでＵＤＰ通信を使用可能にするかどうか
			/// </summary>
			public bool						UdpEnabled { get ; private set ; }

			/// <summary>
			/// セッションでＵＤＰ通信時の誤り補正を有効にするかどうか
			/// </summary>
			public bool						UdpCorrectionEnabled { get ; private set ; }

			//--------------

			/// <summary>
			/// セッション中のユーザー名に上書きする名前(null は無効)
			/// </summary>
			public string					PlayerName { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateSession_RequestPacket
			(
				string					applicationId,
				string					description,
				int						maxPlayers,
				string					password,
				SessionScopeTypes		scopeType,
				SessionManagementTypes	managementType,
				bool					udpEnabled,
				bool					udpCorrectionEnabled,
				string					playerName
			)
			{
				RequestType		= RequestTypes.CreateSession ;

				//-------------

				ApplicationId			= applicationId ;

				Description				= description ;
				MaxPlayers				= maxPlayers ;
				Password				= password ;

				ScopeType				= scopeType ;

				ManagementType			= managementType ;
				UdpEnabled				= udpEnabled ;
				UdpCorrectionEnabled	= udpCorrectionEnabled ;

				PlayerName				= playerName ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( ApplicationId ) ;

				PutString( Description ) ;
				PutUShort( ( ushort )MaxPlayers ) ;
				PutString( Password ) ;

				PutByte( ( byte )ScopeType ) ;

				PutByte( ( byte )ManagementType ) ;
				PutBool( UdpEnabled ) ;
				PutBool( UdpCorrectionEnabled ) ;

				PutString( PlayerName ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッション生成の応答パケット
		/// </summary>
		public class CreateSession_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// セッション識別子
			/// </summary>
			public string					SessionId { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// 最大プレイヤー数
			/// </summary>
			public int						MaxPlayers { get ; private set ; }

			/// <summary>
			/// セッションの管理タイプ
			/// </summary>
			public SessionManagementTypes	ManagementType { get ; private set ; }


			//--------------

			/// <summary>
			/// ＵＤＰを使用できるかどうか
			/// </summary>
			public bool						UdpEnabled { get ; private set ; }

			/// <summary>
			/// ＵＤＰの誤り補正を行うかどうか
			/// </summary>
			public bool						UdpCorrectionEnabled { get ; private set ; }

			//--------------

			/// <summary>
			/// サーバーのセッションプロセッサーが使用可能かどうか
			/// </summary>
			public bool						ProcessorEnabled { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// セッションサーバーのアドレス
			/// </summary>
			public string					SessionServerAddress { get ; private set ; }

			/// <summary>
			/// セッションサーバーのポート番号
			/// </summary>
			public int						SessionServerPort { get ; private set ; }
			
			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateSession_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					SessionId				= GetString() ;

					MaxPlayers				= GetUShort() ;
					ManagementType			= ( SessionManagementTypes )GetByte() ;

					UdpEnabled				= GetBool() ;
					UdpCorrectionEnabled	= GetBool() ;
					ProcessorEnabled		= GetBool() ;

					SessionServerAddress	= GetString() ;
					SessionServerPort		= GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( SessionId ) == true ||
					MaxPlayers == 0 ||
					( ManagementType != SessionManagementTypes.HostManagement && ManagementType != SessionManagementTypes.ServerManagement ) ||
					string.IsNullOrEmpty( SessionServerAddress ) == true ||
					SessionServerPort == 0
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッション参加要求
		/// </summary>
		public class JoinToSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// セッション識別子
			/// </summary>
			public string			SessionId { get ; private set ; }

			/// <summary>
			/// セッションパスワード
			/// </summary>
			public string			Password { get ; private set ; }

			/// <summary>
			/// セッション中のユーザー名に上書きする名前(null は無効)
			/// </summary>
			public string			PlayerName { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public JoinToSession_RequestPacket
			(
				string sessionId,
				string password,
				string playerName
			)
			{
				RequestType		= RequestTypes.JoinToSession ;

				//-------------

				SessionId	= sessionId ;
				Password	= password ;
				PlayerName	= playerName ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( SessionId ) ;
				PutString( Password ) ;
				PutString( PlayerName ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッション参加の応答パケット
		/// </summary>
		public class JoinToSession_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// 最大プレイヤー数
			/// </summary>
			public int								MaxPlayers { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの処理方法
			/// </summary
			public SessionManagementTypes			ManagementType { get ; private set ; }

			/// <summary>
			/// ＵＤＰを使用できるかどうか
			/// </summary>
			public bool								UdpEnabled { get ; private set ; }

			/// <summary>
			/// ＵＤＰの誤り補正を行うかどうか
			/// </summary>
			public bool								UdpCorrectionEnabled { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションプロセッサーが有効かどうか
			/// </summary>
			public bool								ProcessorEnabled { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// セッションサーバーのアドレス
			/// </summary>
			public string							SessionServerAddress { get ; private set ; }

			/// <summary>
			/// セッションサーバーのポート番号
			/// </summary>
			public int								SessionServerPort { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// セッションに参加中のメンバー情報
			/// </summary>
			public List<ResponseSessionPlayerData>	SessionPlayers { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public JoinToSession_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					MaxPlayers				= GetUShort() ;

					ManagementType			= ( SessionManagementTypes )GetByte() ;
					UdpEnabled				= GetBool() ;
					UdpCorrectionEnabled	= GetBool() ;

					ProcessorEnabled		= GetBool() ;

					SessionServerAddress	= GetString() ;
					SessionServerPort		= GetUShort() ;

					int i, l = GetUShort() ;

					if( l >  0 )
					{
						SessionPlayers = new List<ResponseSessionPlayerData>() ;

						ResponseSessionPlayerData sessionPlayer ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							sessionPlayer = new ResponseSessionPlayerData() ;
							sessionPlayer.Decode( m_Data, ref m_Pointer ) ;
							SessionPlayers.Add( sessionPlayer ) ;
						}
					}

				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( SessionServerAddress ) == true ||
					SessionServerPort == 0
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}

			/// <summary>
			/// 外部向けのプレイヤー情報群を取得する
			/// </summary>
			/// <returns></returns>
			public SessionPlayer[] GetSessionPlayers()
			{
				var sessionPlayers = new List<SessionPlayer>() ;

				if( SessionPlayers != null && SessionPlayers.Count >  0 )
				{
					foreach( var sessionPlayer in SessionPlayers )
					{
						sessionPlayers.Add( new SessionPlayer
						(
							sessionPlayer.UserId, sessionPlayer.UserName, sessionPlayer.IsGuest, sessionPlayer.IsHost )
						) ;
					}
				}

				return sessionPlayers.ToArray()	 ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッション情報取得要求
		/// </summary>
		public class GetSessions_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アプリケーション識別子
			/// </summary>
			public string			ApplicationId { get ; private set ; }

			/// <summary>
			/// 取得開始位置
			/// </summary>
			public int				Offset { get ; private set ; }

			/// <summary>
			/// 取得数
			/// </summary>
			public int				Length { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetSessions_RequestPacket
			(
				string applicationId,
				int    offset,
				int    length
			)
			{
				RequestType		= RequestTypes.GetSessions ;

				//-------------

				ApplicationId	= applicationId ;
				Offset			= offset ;
				Length			= length ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( ApplicationId ) ;
				PutUShort( ( ushort )Offset ) ;
				PutUShort( ( ushort )Length ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッション情報取得の応答パケット
		/// </summary>
		public class GetSessions_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// セッション情報
			/// </summary>
			public Session[]	Sessions { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetSessions_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					var sessions = new List<Session>() ;

					int i, l = GetUShort() ;
					if( l >  0 )
					{
						ResponseSessionData session ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							session = new ResponseSessionData() ;
							session.Decode( m_Data, ref m_Pointer ) ;
							sessions.Add
							(
								new Session
								(
									session.SessionId,
									session.Description,
									session.MaxPlayers,
									session.PasswordRequired,
									session.NowPlayers
								)
							) ;
						}
					}

					Sessions = sessions.ToArray() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// フレンド情報取得要求
		/// </summary>
		public class GetFriends_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// 取得開始位置
			/// </summary>
			public int				Offset { get ; private set ; }

			/// <summary>
			/// 取得数
			/// </summary>
			public int				Length { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetFriends_RequestPacket
			(
				int    offset,
				int    length
			)
			{
				RequestType		= RequestTypes.GetFriends ;

				//-------------

				Offset			= offset ;
				Length			= length ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutUShort( ( ushort )Offset ) ;
				PutUShort( ( ushort )Length ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// フレンド情報取得の応答パケット
		/// </summary>
		public class GetFriends_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// フレンド情報
			/// </summary>
			public ResponseFriendData[]	Friends { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetFriends_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					var friends = new List<ResponseFriendData>() ;

					int i, l = GetUShort() ;
					if( l >  0 )
					{
						ResponseFriendData friend ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							friend = new ResponseFriendData() ;
							friend.Decode( m_Data, ref m_Pointer ) ;
							friends.Add( friend ) ;
						}
					}

					Friends = friends.ToArray() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 汎用ＡＰＩコール
		private async Task<( ResponseCodes, string, byte[] )> CallWebApi_C_Async
		(
			string address,
			int port,
			byte[] requestContentData,
			CancellationToken cancellationToken = default
		)
		{
			//----------------------------------------------------------
			// リクエスト情報を生成する

			var request = new List<byte>() ;

			// シグネチャ
			request.AddRange( m_Signature ) ;

			// バージョンコード
			request.Add( ( byte )(   m_VersionCode         & 0xFF ) ) ; 
			request.Add( ( byte )( ( m_VersionCode >>  8 ) & 0xFF ) ) ; 
			request.Add( ( byte )( ( m_VersionCode >> 16 ) & 0xFF ) ) ; 
			request.Add( ( byte )( ( m_VersionCode >> 24 ) & 0xFF ) ) ; 

			// アクセストークンを格納する
			DataFormat.PutString( request, m_AccessToken ) ;

			// コンテント
			if( requestContentData != null &&  requestContentData.Length >  0 )
			{
				try
				{
					// 共通鍵による暗号化を行う
//					byte[] encryptedData = Security.EncryptByCommonKey( requestContentData, m_CommonKey ) ;
					byte[] encryptedData = m_Crypter.EncryptAes( requestContentData ) ;
					request.AddRange( encryptedData ) ;
				}
				catch( Exception )
				{
					// 失敗
					return ( ResponseCodes.BadRequest, "リクエスト情報に誤りがあります", null ) ;
				}
			}
			else
			{
				// 失敗
				return ( ResponseCodes.BadRequest, "リクエスト情報に誤りがあります", null ) ;
			}

			//----------------------------------------------------------
			// 接続を行う

			// ソケット生成

			var mainContext = SynchronizationContext.Current ;

			// ソケットクライアントを生成する
			var socketClient = new SocketClient
			(
				OnTcpReceievedHandler,
				null,
				null,
				m_MaxTcpPacketSize,
				m_ClientCancellationTokenSource.Token,
				mainContext
			) ;

			//----------------------------------------------------------

			// サーバーに接続を試みる
			var result = await socketClient.ConnectAsync( address, port, null, cancellationToken ) ;
			if( result == false )
			{
				socketClient.Dispose() ;

				return ( ResponseCodes.ConnectionFailed, "サーバーに接続できません", null ) ;
			}

			//----------------------------------------------------------
			// 送信を行う

			// 完全に送信が終わるのを待つ
			if( await socketClient.SendTcpAsync( request.ToArray(), null, cancellationToken ) == false )
			{
				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				// 失敗
				return ( ResponseCodes.RequestFailed, "サーバーと通信出来ません", null ) ;
			}

			//----------------------------------------------------------
			// 受信を待つ

			bool isReceived = false ;
			byte[] receivedData = null ;

			// 受信コールバック
			void OnTcpReceievedHandler( byte[] data )
			{
				isReceived = true ;
				receivedData = data ;
			}

			//----------------------------------

			CancellationTokenSource cancellationTokenSource ;

			// ※ m_TcpCancellationTokenSource には必ず値が設定されている
			if( cancellationToken == default )
			{
				cancellationTokenSource = m_ClientCancellationTokenSource ;
			}
			else
			{
				cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_ClientCancellationTokenSource.Token, cancellationToken ) ;
			}

			//--------------

			// 一時的に生成したキャンセレーショントークンソースをきちんと破棄するためにめんどくさい処理が必要

			bool isCanceled	= false ;

			while( socketClient.IsConnected == true )
			{
				if( isReceived == true )
				{
					// 受信した
					break ;
				}

				if( cancellationTokenSource.IsCancellationRequested == true )
				{
					// これらは中断扱いとする
					isCanceled = true ;
					break ;
				}

				await Task.Yield() ;
			}

			//--------------

			if( cancellationToken != default )
			{
				// 一時的に生成したものなので破棄する
				cancellationTokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は切断した上で例外を投げる

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				throw new OperationCanceledException() ;
			}

			if( socketClient.IsConnected == false )
			{
				socketClient.Dispose() ;

				// 切断された可能性がある
				return ( ResponseCodes.RequestFailed, "サーバーと通信出来ません", null ) ;
			}

			//--------------

			if( receivedData == null || receivedData.Length <  2 )
			{
				// 失敗(データ異常)

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			//----------------------------------------------------------

			int offset = 0 ;

			// レスポンスコードを取得する
			ResponseCodes responseCode ;

			try
			{
				responseCode = ( ResponseCodes )DataFormat.GetUShort( receivedData, ref offset ) ;
			}
			catch( Exception )
			{
				// 失敗(データ異常)

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			//----------------------------------------------------------

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				// エラーメッセージを取得する
				string errorMessage ;

				try
				{
					errorMessage = DataFormat.GetString( receivedData, ref offset ) ;

					Debug.Log( "受信したエラーメッセージ : " + errorMessage ) ;
				}
				catch( Exception )
				{
					// 失敗(データ異常)

					// 切断
//					await m_SocketClient.DisconnectAsync() ;
					socketClient.Disconnect( false ) ;
					socketClient.Dispose() ;

					return ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
				}

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( responseCode, errorMessage, null ) ;			
			}

			// 通信終了

			// 切断
//			await m_SocketClient.DisconnectAsync() ;
			socketClient.Disconnect( false ) ;
			socketClient.Dispose() ;

			//--------------------------------------------------------------------------
			// 成功

			byte[] responseContentData = null ;

			int length = receivedData.Length - offset ;

			if( length >  0 )
			{
				// レスポンスコンテントが存在する

				// レスポンスコンテントの復号化
				try
				{
//					responseContentData = Security.DecryptByCommonKey( receivedData, offset, length, m_CommonKey ) ;
					responseContentData = m_Crypter.DecryptAes( receivedData, offset, length ) ;
				}
				catch( Exception )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
				}

				if( responseContentData == null || responseContentData.Length == 0 )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
				}
			}

			return ( ResponseCodes.Succeeded, string.Empty, responseContentData ) ;
		}
	}
}
