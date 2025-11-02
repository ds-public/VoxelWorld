using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using System.Net ;
using System.Net.Sockets ;

using UnityEngine ;

using SocketHelper ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 標準のアダプター
	/// </summary>
	public partial class DefaultNetworkPlayClientAdapter : INetworkPlayClientAdapter
	{
		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string		CommunicationServer_Address	=> m_CommunicationServer_Address ;

		// コミュニケーションサーバーのアドレス
		private string		m_CommunicationServer_Address ;

		/// <summary>
		/// コミュニケーションサーバーのＴＣＰポート
		/// </summary>
		public ushort		CommunicationServer_TcpPort		=> m_CommunicationServer_TcpPort ;

		// コミュニケーションサーバーのＴＣＰポート
		private ushort		m_CommunicationServer_TcpPort ;

		// コミュニケーションサーバーのＴＣＰエンドポイント
		private IPEndPoint	m_CommunicationServer_TcpEndPoint ;

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
				m_CommunicationServer_TcpEndPoint,
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

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// グルーピングサービスを開始する
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
		public async Task<StartGroupingService_Response> StartGroupingServiceAsync
		(
			Dictionary<string, string>	parameters,
			Action<byte[]>				onMessageReceived,
			CancellationToken			cancellationToken	
		)
		{
			//----------------------------------------------------------

			if( m_GroupingServerProcessor.IsConnected == true )
			{
				// 既に接続済みになっている
				return new
				(
					ResponseCodes.Succeeded, string.Empty,
					GroupingServer_Address, GroupingServer_TcpPort
				) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new StartGroupingService_RequestPacket
			(
			) ;

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new StartGroupingService_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new
				(
					ResponseCodes.BadResponse, "[Client] 受信データに問題があります",
					null, 0
				) ;
			}

			// レスポンスの値を取り出す
			m_GroupingServerProcessor.Address			= responseContent.GroupingServer_Address ;
			m_GroupingServerProcessor.TcpPort			= responseContent.GroupingServer_TcpPort ;

			Debug.Log( "<color=#7FFFFF>グルーピングサーバーのエンドポイント = " + GroupingServer_Address + ":" + GroupingServer_TcpPort + "</color>" ) ;

			//---------------------------------------------------------

			// グルーピングサーバー接続前に汎用通知メッセージの受信コールバックを設定しておく
			m_GroupingServerProcessor.SetOnReceived( onMessageReceived ) ;

			//----------------------------------------------------------
			// ※共通化したいが呼び出し元とコードがほとんど変わらないので共通化は断念

			bool isCanceled = false ;

			try
			{
				// グルーピングサーバーへＴＣＰ接続を行う
				( responseCode, errorMessage ) = await m_GroupingServerProcessor.Connect
				(
					GroupingServer_Address,
					GroupingServer_TcpPort,
					parameters,
					cancellationToken,
					m_OwnerCancellationToken
				) ;
			}
			catch( Exception e )
			{
				responseCode = ResponseCodes.ConnectionFailed ;
				errorMessage = "グルーピングサーバーに接続できない\n" + e.Message ;

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

			if( responseCode != ResponseCodes.Succeeded )
			{
				// グルーピングサーバーへの接続に失敗した
				await m_GroupingServerProcessor.StopServiceAsync( m_OwnerCancellationToken ) ;	// こちらが失敗しても結果は無視する

				return new
				(
					responseCode, errorMessage,
					null, 0
				) ;
			}

			//----------------------------------------------------------

			try
			{
				// 接続完了のコールバックを呼ぶ
				m_GroupingServerProcessor.CallOnConnected() ;
			}
			catch( Exception )
			{
				throw ;
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				ResponseCodes.Succeeded, string.Empty,
				GroupingServer_Address, GroupingServer_TcpPort
			) ;
		}

		/// <summary>
		/// グルーピングサービスを終了する
		/// </summary>
		/// <returns></returns>
		public bool StopGroupingService()
		{
			m_GroupingServerProcessor.StopService() ;
			return true ;
		}

		//-------------------------------------------------------------------------------------------

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
			string					    description,
			string					    password,
			int						    maxPlayers,
			SessionScopeTypes		    scopeType,
			SessionManagementTypes	    managementType,
			bool					    udpEnabled,
			bool					    udpCorrectionEnabled,
			Dictionary<string,string>   parameters,
			string					    playerName,
			CancellationToken		    cancellationToken	
		)
		{
			if( string.IsNullOrEmpty( m_ApplicationId ) == true  )
			{
				// 無効なアプリケーション識別子
				return new
				(
					ResponseCodes.InvalidApplicationIdentifier, "[Client] 無効なアプリケーション識別子です"
				) ;
			}

			//----------------------------------------------------------

			if( IsSessionJoined == true )
			{
				// 既にセッションに参加している(NetworkPlayClientインスタンス１つにつき参加できるセッションは１つまで)
				return new
				(
					ResponseCodes.AlreadyJoinedSession, "[Client] 既にセッションに参加しています"
				) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new CreateSession_RequestPacket
			(
				m_ApplicationId,
				description,
				password,
				maxPlayers,
				scopeType,
				managementType,
				udpEnabled,
				udpCorrectionEnabled,
				parameters,
				playerName
			) ;

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage
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
					ResponseCodes.BadResponse, "[Client] 受信データに問題があります"
				) ;
			}

			//----------------------------------------------------------

			// ExchangeServer へ接続を試みる
			( responseCode, errorMessage ) = await ConnectToExchangeServerAsync
			(
				responseContent.SessionId,

				responseContent.MaxPlayers,
				responseContent.ScopeType,
				responseContent.ManagementType,
				responseContent.UdpEnabled,
				responseContent.UdpCorrectionEnabled,
				responseContent.Parameters,
				responseContent.ProcessorEnabled,

				responseContent.GetSessionPlayers(),

				responseContent.ExchangeServer_Address,
				responseContent.ExchangeServer_TcpPort,
				responseContent.ExchangeServer_UdpPort,

				cancellationToken
			) ;

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage
				) ;
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				SessionId,
				MaxPlayers, ManagementType,
				UdpEnabled, UdpCorrectionEnabled,
				responseContent.Parameters,
				responseContent.ProcessorEnabled,
				responseContent.GetSessionPlayers(),
				ExchangeServer_Address, ExchangeServer_TcpPort, ExchangeServer_UdpPort
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
			ulong   sessionId,
			string  password,
			string  playerName,
			CancellationToken cancellationToken	
		)
		{
			if( IsSessionJoined == true )
			{
				// 既にセッションに参加している(NetworkPlayClientインスタンス１つにつき参加できるセッションは１つまで)
				return new
				(
					ResponseCodes.AlreadyJoinedSession, "[Client] 既にセッションに参加しています"
				) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new JoinToSession_RequestPacket
			(
				( uint )sessionId,
				password,
				playerName
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage
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
					ResponseCodes.BadResponse, "[Client] 受信データに問題があります"
				) ;
			}

			//----------------------------------------------------------

			// ExchangeServer へ接続を試みる
			( responseCode, errorMessage ) = await ConnectToExchangeServerAsync
			(
				( uint )sessionId,

				responseContent.MaxPlayers,
				responseContent.ScopeType,
				responseContent.ManagementType,
				responseContent.UdpEnabled,
				responseContent.UdpCorrectionEnabled,
				responseContent.Parameters,
				responseContent.ProcessorEnabled,

				responseContent.GetSessionPlayers(),

				responseContent.ExchangeServer_Address,
				responseContent.ExchangeServer_TcpPort,
				responseContent.ExchangeServer_UdpPort,

				cancellationToken
			) ;

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new
				(
					responseCode, errorMessage
				) ;
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				SessionId,
				MaxPlayers, ManagementType, 
				UdpEnabled, UdpCorrectionEnabled,
				responseContent.Parameters,
				responseContent.ProcessorEnabled,
				responseContent.GetSessionPlayers(),
				ExchangeServer_Address, ExchangeServer_TcpPort, ExchangeServer_UdpPort
			) ;
		}

		/// <summary>
		/// ExchangeServer へ接続する
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name=""></param>
		/// <returns></returns>
		/// <exception cref="OperationCanceledException"></exception>
		public async Task<( ResponseCodes, string )> ConnectToExchangeServerAsync
		(
			uint					    sessionId,

			ushort					    maxPlayers,
			SessionScopeTypes		    scopeType,
			SessionManagementTypes	    managementType,
			bool					    udpEnabled,
			bool					    udpCorrectionEnabled,
			Dictionary<string,string>   parameters,
			bool					    processorEnabled,

			List<SessionPlayer>		    sessionPlayers,

			string					    exchangeServer_Address,
			ushort					    exchangeServer_TcpPort,
			ushort					    exchangeServer_UdpPort,

			CancellationToken		    cancellationToken
		)
		{
			// レスポンスの値を取り出す
			m_ExchangeServerProcessor.SessionId				= sessionId ;

			m_ExchangeServerProcessor.MaxPlayers			= maxPlayers ;

			m_ExchangeServerProcessor.SessionScopeType		= scopeType ;
			m_ExchangeServerProcessor.ManagementType		= managementType ;

			m_ExchangeServerProcessor.UdpEnabled			= udpEnabled ;
			m_ExchangeServerProcessor.UdpCorrectionEnabled	= udpCorrectionEnabled ;

			m_ExchangeServerProcessor.Parameters            = parameters ;

			Debug.Log( "<color=#FFFF00>------------ＵＤＰが使えるかどうか : " + UdpEnabled + "</color>" ) ;

			m_ExchangeServerProcessor.Address				= exchangeServer_Address ;
			m_ExchangeServerProcessor.TcpPort				= exchangeServer_TcpPort ;
			m_ExchangeServerProcessor.UdpPort				= exchangeServer_UdpPort ;

			if( UdpEnabled == true )
			{
				// ＵＤＰ用のエンドポイントを作成する
				if( CreateUdpEndPoint( m_ExchangeServerProcessor.Address, m_ExchangeServerProcessor.UdpPort ) == false )
				{
					// 失敗(データ異常)
					return new
					(
						ResponseCodes.BadResponse, "[Client] ＵＤＰのエンドポイントが生成できません\n" + ExchangeServer_Address + ":" + ExchangeServer_UdpPort
					) ;
				}
			}

			Debug.Log( "<color=#7FFFFF>エクスチェンジサーバーのエンドポイント = " + ExchangeServer_Address + ":" + ExchangeServer_TcpPort + "," + ExchangeServer_UdpPort + "</color>" ) ;

			//----------------------------------------------------------
			// ※共通化したいが呼び出し元とコードがほとんど変わらないので共通化は断念

			ResponseCodes	responseCode ;
			string			errorMessage ;


			bool isCanceled = false ;

			try
			{
				// エクスチェンジサーバーへＴＣＰ接続を行う
				( responseCode, errorMessage ) = await m_ExchangeServerProcessor.Connect
				(
					ExchangeServer_Address,
					ExchangeServer_TcpPort,
					cancellationToken,
					m_OwnerCancellationToken
				) ;
			}
			catch( Exception e )
			{
				responseCode = ResponseCodes.ConnectionFailed ;
				errorMessage = "[Client] エクスチェンジサーバーに接続できない\n" + e.Message ;

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

			if( responseCode != ResponseCodes.Succeeded )
			{
				// セッションサーバーへの接続に失敗した
				if( m_ExchangeServerProcessor != null )
				{
					// ExchangeServerProcessor が破棄されている可能性がある
					await m_ExchangeServerProcessor.LeaveFromSessionAsync() ;	// こちらが失敗しても結果は無視する
				}

				return new
				(
					responseCode, errorMessage
				) ;
			}

			//----------------------------------------------------------

			try
			{
				// 接続完了のコールバックを呼ぶ
				m_ExchangeServerProcessor.CallOnConnected() ;
			}
			catch( Exception )
			{
				throw ;
			}

			//----------------------------------------------------------
			// セッションプロセッサーを Host で生成する場合はここで設定する(Host の場合は生成済みのものが設定されるため)
			// ※ホストでなくても一律実行する

			if( ManagementType == SessionManagementTypes.HostManagement )
			{
				if( IsHost == true )
				{
					// ホストであるため
					processorEnabled = WakeupSessionProcessor() ;
	
					if( processorEnabled == true )
					{
						m_ExchangeServerProcessor.CallOnActive_ForSessionProcessor
						(
							sessionPlayers
						) ;
					}
				}
				else
				{
					// メンバー
					WakeupSessionProcessor() ;
				}
			}

			return ( ResponseCodes.Succeeded, string.Empty ) ;
		}

		// ＵＤＰ用のエンドポイントを生成する
		private bool CreateUdpEndPoint( string address, int port )
		{
			// ＵＤＰ用のエンドポイントを作成する
			if( IPAddress.TryParse( address, out IPAddress ipAddress ) == false )
			{
				// 単純に変換できない
				var ipAddresses = Dns.GetHostAddresses( address ) ;
				if( ipAddresses != null && ipAddresses.Length >  0 )
				{
					foreach( var _ in ipAddresses )
					{
						if( _.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
						{
							ipAddress = _ ;
							break ;
						}
					}
				}
			}
				
			if( ipAddress != null && ipAddress.GetAddressBytes() != null )
			{
				m_ExchangeServerProcessor.UdpEndPoint = new IPEndPoint( ipAddress, port ) ;
				return true ;
			}
			else
			{
				return false ;
			}
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
			uint	offset,
			uint	length,
			CancellationToken cancellationToken
		)
		{
			if( string.IsNullOrEmpty( m_ApplicationId ) == true  )
			{
				// 無効なアプリケーション識別子
				return new ( ResponseCodes.InvalidApplicationIdentifier, "[Client] 無効なアプリケーション識別子です", null, 0 ) ;
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
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new GetSessions_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります", null, 0 ) ;
			}

			var sessions	= responseContent.Sessions ;
			var count		= responseContent.Count ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, sessions, count ) ;
		}

		/// <summary>
		/// フレンド情報取得を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<GetFriends_Response> GetFriendsAsync
		(
			ushort		offset = 0,
			ushort		length = 0,
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
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new GetFriends_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります", null, 0 ) ;
			}

			var friends		= responseContent.Friends ;
			var count		= responseContent.Count ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, friends, count ) ;
		}


		//-----------------------------------------------------------


		/// <summary>
		/// セッションのスコープタイプを設定する(セッションに参加済み且つホストである場合のみ使用可能)
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="scopeType"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<SetSessionScopeType_Response> SetSessionScopeTypeAsync
		(
			SessionScopeTypes		scopeType,
			CancellationToken		cancellationToken	
		)
		{
			if( string.IsNullOrEmpty( m_ApplicationId ) == true )
			{
				return new ( ResponseCodes.InvalidApplicationIdentifier, "アプリケーション識別子が設定されていません" ) ;
			}

			if( IsSessionJoined == false )
			{
				return new ( ResponseCodes.InvalidSessionIdentifier, "セッションに参加していません" ) ;
			}

			//----------------------------------

			// リクエストコンテント部
			var requestContent = new SetSessionScopeType_RequestPacket
			(
				( uint )SessionId,
				scopeType
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new SetSessionScopeType_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります" ) ;
			}

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty ) ;
		}

		//-------------------------------------------------------------------------------------------
		// ユーザー情報の更新

		public async Task<UpdateUserName_Response> UpdateUserNameAsync
		(
			string userName,
			CancellationToken cancellationToken
		)
		{
			// リクエストコンテント部
			var requestContent = new UpdateUserName_RequestPacket
			(
				userName
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
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
			var responseContent = new UpdateUserName_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります", null ) ;
			}

			// 実際に設定されたユーザー名をローカルにも反映させる
			m_UserName	= responseContent.UserName ;	// 実際に設定されたユーザー名(小文字が大文字に変わるケースなども想定)

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, m_UserName ) ;
		}

		//-------------------------------------------------------------------------------------------
		// デバッグ機能

		/// <summary>
		/// ユーザー情報群を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<GetUsers_Response> GetUsersAsync
		(
			ulong offset,
			ulong length,
			CancellationToken cancellationToken
		)
		{
			// リクエストコンテント部
			var requestContent = new GetUsers_RequestPacket
			(
				offset,
				length
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new GetUsers_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります", null, 0 ) ;
			}

			var users	= responseContent.Users ;
			var count	= responseContent.Count ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, users, count ) ;
		}

		/// <summary>
		/// フレンド設定を実行する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<SetFriend_Response> SetFriendAsync
		(
			string	userId,
			bool	isFriend,
			CancellationToken cancellationToken
		)
		{
			// リクエストコンテント部
			var requestContent = new SetFriend_RequestPacket
			(
				userId,
				isFriend
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_C_Async
			(
				m_CommunicationServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new SetFriend_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "[Client] 受信データに問題があります" ) ;
			}

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty ) ;
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
		/// グルーピングサービス開始の要求パケット
		/// </summary>
		public class StartGroupingService_RequestPacket : RequestPacketBase
		{
			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StartGroupingService_RequestPacket
			(
			)
			{
				RequestType		= RequestTypes.StartGroupingService ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// グルーピングサービス開始の応答パケット
		/// </summary>
		public class StartGroupingService_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// グルーピングサーバーのアドレス
			/// </summary>
			public string					GroupingServer_Address { get ; private set ; }

			/// <summary>
			/// グルーピングサーバーのＴＣＰポート
			/// </summary>
			public ushort					GroupingServer_TcpPort { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StartGroupingService_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				//---------------------------------
				// デコード

				try
				{
					GroupingServer_Address	= GetString() ;
					GroupingServer_TcpPort	= GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------
				// バリデーションチェック

				if
				(
					string.IsNullOrEmpty( GroupingServer_Address ) == true ||
					GroupingServer_TcpPort == 0
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
#if MATCHING_SYSTEM_OLD_VERSION
		/// <summary>
		/// セッションへのマッチング開始の要求パケット(旧版)
		/// </summary>
		public class StartMatchingToSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アプリケーション識別子
			/// </summary>
			public string					    ApplicationId           { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの説明文
			/// </summary>
			public string					    Description             { get ; private set ; }

			/// <summary>
			/// セッションのパスワード
			/// </summary>
			public string					    Password                { get ; private set ; }

			/// <summary>
			/// セッションの最大参加可能人数
			/// </summary>
			public ushort					    MaxPlayers              { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの情報公開範囲
			/// </summary>
			public SessionScopeTypes		    ScopeType               { get ; private set ; }

			/// <summary>
			/// セッションの処理種別
			/// </summary>
			public SessionManagementTypes	    ManagementType          { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションでＵＤＰ通信を使用可能にするかどうか
			/// </summary>
			public bool						    UdpEnabled              { get ; private set ; }

			/// <summary>
			/// セッションでＵＤＰ通信時の誤り補正を有効にするかどうか
			/// </summary>
			public bool						    UdpCorrectionEnabled    { get ; private set ; }

			//-----

			/// <summary>
			/// セッション固有パラメータ
			/// </summary>
			public Dictionary<string,string>    Parameters              { get ; private set ; }


			//--------------

			/// <summary>
			/// セッション中のユーザー名に上書きする名前(null は無効)
			/// </summary>
			public string					    PlayerName              { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StartMatchingToSession_RequestPacket
			(
				string					    applicationId,
				string					    description,
				string					    password,
				ushort					    maxPlayers,
				SessionScopeTypes		    scopeType,
				SessionManagementTypes	    managementType,
				bool					    udpEnabled,
				bool					    udpCorrectionEnabled,
				Dictionary<string,string>   parameters,
				string					    playerName
			)
			{
				RequestType		= RequestTypes.StartMatchingToSession ;

				//-------------

				ApplicationId			= applicationId ;

				Description				= description ;
				Password				= password ;
				MaxPlayers				= maxPlayers ;

				ScopeType				= scopeType ;
				ManagementType			= managementType ;
				UdpEnabled				= udpEnabled ;
				UdpCorrectionEnabled	= udpCorrectionEnabled ;

				Parameters              = parameters ;

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
				PutString( Password ) ;
				PutUShort( MaxPlayers ) ;

				PutByte( ( byte )ScopeType ) ;
				PutByte( ( byte )ManagementType ) ;
				PutBool( UdpEnabled ) ;
				PutBool( UdpCorrectionEnabled ) ;

				if( Parameters == null || Parameters.Count == 0 )
				{
					PutByte( 0 ) ;
				}
				else
				{
					PutByte( ( byte )Parameters.Count ) ;
					foreach( ( var key, var value ) in Parameters )
					{
						PutString( key ) ;
						PutString( value ) ;
					}
				}

				PutString( PlayerName ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッションへのマッチング開始の応答パケット
		/// </summary>
		public class StartMatchingToSession_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// マッチング識別子
			/// </summary>
			public ulong					MatchingId { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StartMatchingToSession_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				//---------------------------------
				// デシリアライズ

				try
				{
					MatchingId				= GetULong() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------
				// バリデーションチェック

				if
				(
					MatchingId == 0
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
		/// セッションへのマッチング実行の要求パケット
		/// </summary>
		public class ExecuteMatchingToSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// マッチング識別子
			/// </summary>
			public ulong					MatchingId { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public ExecuteMatchingToSession_RequestPacket
			(
				ulong					matchingId
			)
			{
				RequestType		= RequestTypes.ExecuteMatchingToSession ;

				//-------------

				MatchingId				= matchingId ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutULong( MatchingId ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッションへのマッチング実行の応答パケット
		/// </summary>
		public class ExecuteMatchingToSession_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// マッチング識別子
			/// </summary>
			public ulong					MatchingId { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public ExecuteMatchingToSession_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				//---------------------------------
				// デシリアライズ

				try
				{
					MatchingId				= GetULong() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------
				// バリデーションチェック

				if
				(
					MatchingId == 0
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
		/// セッションへのマッチング中断の要求パケット
		/// </summary>
		public class StopMatchingToSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// マッチング識別子
			/// </summary>
			public ulong					MatchingId { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StopMatchingToSession_RequestPacket
			(
				ulong			matchingId
			)
			{
				RequestType		= RequestTypes.StopMatchingToSession ;

				//-------------

				MatchingId		= matchingId ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutULong( MatchingId ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッションへのマッチング中断の応答パケット
		/// </summary>
		public class StopMatchingToSession_ResponsePacket : ResponsePacketBase
		{
			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public StopMatchingToSession_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
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
#endif
		//-----------------------------------

		/// <summary>
		/// セッション生成の要求パケット
		/// </summary>
		public class CreateSession_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アプリケーション識別子
			/// </summary>
			public string					    ApplicationId           { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの説明文
			/// </summary>
			public string					    Description             { get ; private set ; }

			/// <summary>
			/// セッションのパスワード
			/// </summary>
			public string					    Password                { get ; private set ; }

			/// <summary>
			/// セッションの最大参加可能人数
			/// </summary>
			public int						    MaxPlayers              { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションの情報公開範囲
			/// </summary>
			public SessionScopeTypes		    ScopeType               { get ; private set ; }

			/// <summary>
			/// セッションの処理種別
			/// </summary>
			public SessionManagementTypes	    ManagementType          { get ; private set ; }

			//--------------

			/// <summary>
			/// セッションでＵＤＰ通信を使用可能にするかどうか
			/// </summary>
			public bool						    UdpEnabled              { get ; private set ; }

			/// <summary>
			/// セッションでＵＤＰ通信時の誤り補正を有効にするかどうか
			/// </summary>
			public bool						    UdpCorrectionEnabled    { get ; private set ; }

			//--------------

			/// <summary>
			/// セッション固有パラメータ
			/// </summary>
			public Dictionary<string,string>    Parameters              { get ; private set ; }

			//--------------

			/// <summary>
			/// セッション中のユーザー名に上書きする名前(null は無効)
			/// </summary>
			public string					    PlayerName              { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateSession_RequestPacket
			(
				string					    applicationId,
				string					    description,
				string					    password,
				int						    maxPlayers,
				SessionScopeTypes		    scopeType,
				SessionManagementTypes	    managementType,
				bool					    udpEnabled,
				bool					    udpCorrectionEnabled,
				Dictionary<string,string>   parameters,
				string					    playerName
			)
			{
				RequestType		= RequestTypes.CreateSession ;

				//-------------

				ApplicationId			= applicationId ;

				Description				= description ;
				Password				= password ;
				MaxPlayers				= maxPlayers ;

				ScopeType				= scopeType ;
				ManagementType			= managementType ;
				UdpEnabled				= udpEnabled ;
				UdpCorrectionEnabled	= udpCorrectionEnabled ;

				Parameters              = parameters ;

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
				PutString( Password ) ;
				PutUShort( ( ushort )MaxPlayers ) ;

				PutByte( ( byte )ScopeType ) ;
				PutByte( ( byte )ManagementType ) ;

				PutBool( UdpEnabled ) ;
				PutBool( UdpCorrectionEnabled ) ;

				if( Parameters == null || Parameters.Count == 0 )
				{
					PutByte( 0 ) ;
				}
				else
				{
					PutByte( ( byte )Parameters.Count ) ;

					foreach( ( var key, var value ) in Parameters )
					{
						PutString( key ) ;
						PutString( value ) ;
					}
				}

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
			public uint		    			        SessionId		        { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// 最大プレイヤー数
			/// </summary>
			public ushort					        MaxPlayers		        { get ; private set ; }

			//--------------

			/// <summary>
			/// スコープタイプ
			/// </summary>
			public SessionScopeTypes		        ScopeType		        { get ; private set ; }

			/// <summary>
			/// セッションの管理タイプ
			/// </summary>
			public SessionManagementTypes	        ManagementType	        { get ; private set ; }


			//--------------

			/// <summary>
			/// ＵＤＰを使用できるかどうか
			/// </summary>
			public bool						        UdpEnabled		        { get ; private set ; }

			/// <summary>
			/// ＵＤＰの誤り補正を行うかどうか
			/// </summary>
			public bool						        UdpCorrectionEnabled    { get ; private set ; }

			//--------------

			/// <summary>
			/// セッション固有パラメータ
			/// </summary>
			public Dictionary<string,string>        Parameters              { get ; private set ; }

			//--------------

			/// <summary>
			/// サーバーのセッションプロセッサーが使用可能かどうか
			/// </summary>
			public bool						        ProcessorEnabled        { get ; private set ; }


			//----------------------------------

			/// <summary>
			/// セッションに参加中のメンバー情報
			/// </summary>
			public List<ResponseSessionPlayerData>	SessionPlayers          { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// エクスチェンジサーバーのアドレス
			/// </summary>
			public string					        ExchangeServer_Address  { get ; private set ; }

			/// <summary>
			/// エクスチェンジサーバーのＴＣＰポート番号
			/// </summary>
			public ushort					        ExchangeServer_TcpPort  { get ; private set ; }

			/// <summary>
			/// エクスチェンジサーバーのＵＤＰポート番号
			/// </summary>
			public ushort					        ExchangeServer_UdpPort  { get ; private set ; }

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
				int i, l ;

				//---------------------------------
				// デシリアライズ

				try
				{
					//------------
					// Session

					SessionId				= GetUInt() ;

					MaxPlayers				= GetUShort() ;

					ScopeType				= ( SessionScopeTypes )GetByte() ;
					ManagementType			= ( SessionManagementTypes )GetByte() ;

					UdpEnabled				= GetBool() ;
					UdpCorrectionEnabled	= GetBool() ;

					Parameters = new () ;

					l = GetByte() ;
					if( l >  0 )
					{
						for( i  = 0 ; i <  l ; i ++ )
						{
							string key      = GetString() ;
							string value    = GetString() ;

							Parameters.Add( key, value ) ;
						}
					}

					ProcessorEnabled		= GetBool() ;

					//------------
					// SessionPlayers

					SessionPlayers = new () ;

					l = GetVUShort() ;
					if( l >  0 )
					{
						ResponseSessionPlayerData sessionPlayer ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							sessionPlayer = new ResponseSessionPlayerData() ;
							sessionPlayer.Decode( m_Data, ref m_Pointer ) ;
							SessionPlayers.Add( sessionPlayer ) ;
						}
					}

					//------------
					// ExchangeServer EndPoint

					ExchangeServer_Address	= GetString() ;
					ExchangeServer_TcpPort	= GetUShort() ;
					ExchangeServer_UdpPort	= GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------
				// バリデーションチェック

				if
				(
					MaxPlayers == 0 ||
					( ScopeType != SessionScopeTypes.Public && ScopeType != SessionScopeTypes.Private ) ||
					( ManagementType != SessionManagementTypes.HostManagement && ManagementType != SessionManagementTypes.ServerManagement ) ||
					string.IsNullOrEmpty( ExchangeServer_Address ) == true ||
					ExchangeServer_TcpPort == 0
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
			public List<SessionPlayer> GetSessionPlayers()
			{
				var sessionPlayers = new List<SessionPlayer>() ;

				if( SessionPlayers != null && SessionPlayers.Count >  0 )
				{
					foreach( var sessionPlayer in SessionPlayers )
					{
						sessionPlayers.Add( new SessionPlayer
						(
							sessionPlayer.UserId,
							sessionPlayer.UserName,
							sessionPlayer.IsGuest,
							sessionPlayer.IsHost,
							sessionPlayer.Parameters
						) ) ;
					}
				}

				return sessionPlayers ;
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
			public uint			    SessionId { get ; private set ; }

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
				uint    sessionId,
				string  password,
				string  playerName
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

				PutUInt( SessionId ) ;
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

			//----------------------------------

			/// <summary>
			/// 最大プレイヤー数
			/// </summary>
			public ushort							MaxPlayers		        { get ; private set ; }

			//--------------

			/// <summary>
			/// スコープタイプ
			/// </summary>
			public SessionScopeTypes				ScopeType		        { get ; private set ; }

			/// <summary>
			/// セッションの処理方法
			/// </summary
			public SessionManagementTypes			ManagementType	        { get ; private set ; }

			//--------------

			/// <summary>
			/// ＵＤＰを使用できるかどうか
			/// </summary>
			public bool								UdpEnabled		        { get ; private set ; }

			/// <summary>
			/// ＵＤＰの誤り補正を行うかどうか
			/// </summary>
			public bool								UdpCorrectionEnabled    { get ; private set ; }

			//--------------

			/// <summary>
			/// セッション固有パラメータ
			/// </summary>
			public Dictionary<string,string>        Parameters              { get ; private set ; }        

			//--------------

			/// <summary>
			/// セッションプロセッサーが有効かどうか
			/// </summary>
			public bool								ProcessorEnabled        { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// セッションに参加中のメンバー情報
			/// </summary>
			public List<ResponseSessionPlayerData>	SessionPlayers          { get ; private set ; }

			//----------------------------------

			/// <summary>
			/// エクスチェンジサーバーのアドレス
			/// </summary>
			public string							ExchangeServer_Address  { get ; private set ; }

			/// <summary>
			/// エクスチェンジサーバーのＴＣＰポート番号
			/// </summary>
			public ushort							ExchangeServer_TcpPort  { get ; private set ; }

			/// <summary>
			/// エクスチェンジサーバーのＵＤＰポート番号
			/// </summary>
			public ushort							ExchangeServer_UdpPort  { get ; private set ; }

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
				int i, l ;

				try
				{
					//------------
					// Session

					MaxPlayers				= GetUShort() ;

					ScopeType				= ( SessionScopeTypes )GetByte() ;
					ManagementType			= ( SessionManagementTypes )GetByte() ;
					UdpEnabled				= GetBool() ;
					UdpCorrectionEnabled	= GetBool() ;

					Parameters              = new () ;

					l = GetByte() ;
					if( l >  0 )
					{
						for( i  = 0 ; i <  l ; i ++ )
						{
							string key      = GetString() ;
							string value    = GetString() ;
							Parameters.Add( key, value ) ;
						}
					}

					ProcessorEnabled		= GetBool() ;

					//------------
					// SessionPlayers

					SessionPlayers = new () ;

					l = GetVUShort() ;
					if( l >  0 )
					{
						ResponseSessionPlayerData sessionPlayer ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							sessionPlayer = new ResponseSessionPlayerData() ;
							sessionPlayer.Decode( m_Data, ref m_Pointer ) ;
							SessionPlayers.Add( sessionPlayer ) ;
						}
					}

					//------------
					// ExchangeServer EndPoint

					ExchangeServer_Address	= GetString() ;
					ExchangeServer_TcpPort	= GetUShort() ;
					ExchangeServer_UdpPort	= GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------

				// バリデーションチェック
				if
				(
					string.IsNullOrEmpty( ExchangeServer_Address ) == true ||
					ExchangeServer_TcpPort == 0
				)
				{
					// 失敗
					return false ;
				}

				//---------------------------------

				// 成功
				return true ;
			}

			/// <summary>
			/// 外部向けのプレイヤー情報群を取得する
			/// </summary>
			/// <returns></returns>
			public List<SessionPlayer> GetSessionPlayers()
			{
				var sessionPlayers = new List<SessionPlayer>() ;

				if( SessionPlayers != null && SessionPlayers.Count >  0 )
				{
					foreach( var sessionPlayer in SessionPlayers )
					{
						sessionPlayers.Add( new SessionPlayer
						(
							sessionPlayer.UserId,
							sessionPlayer.UserName,
							sessionPlayer.IsGuest,
							sessionPlayer.IsHost,
							sessionPlayer.Parameters
						) ) ;
					}
				}

				return sessionPlayers ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッション情報群取得要求
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
			public uint				Offset { get ; private set ; }

			/// <summary>
			/// 取得数
			/// </summary>
			public uint				Length { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetSessions_RequestPacket
			(
				string	applicationId,
				uint	offset,
				uint	length
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
				PutUInt( Offset ) ;
				PutUInt( Length ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッション情報群取得の応答パケット
		/// </summary>
		public class GetSessions_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// セッション情報
			/// </summary>
			public List<Session>	Sessions	{ get ; private set ; }

			/// <summary>
			/// 最大セッション数
			/// </summary>
			public uint				Count		{ get ; private set ; }

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
				//---------------------------------
				// デコード

				try
				{
					var sessions = new List<Session>() ;

					uint i, l = GetVUInt() ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						var sessionId			= GetUInt() ;
						var passwordRequired	= GetBool() ;
						var nowPlayers			= GetUShort() ;
						var maxPlayers			= GetUShort() ;
						var description			= GetString() ;

						sessions.Add( new
						(
							sessionId:sessionId,
							description:description,
							maxPlayers:maxPlayers,
							passwordRequired:passwordRequired,
							nowPlayers:nowPlayers
						) ) ;
					}

					Sessions	= sessions ;

					Count		= GetUInt() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//---------------------------------
				// バリデーションチェック

				if( Count <  0 )
				{
					return false ;
				}

				//---------------------------------

				// 成功
				return true ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// フレンド情報群取得要求
		/// </summary>
		public class GetFriends_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// 取得開始位置
			/// </summary>
			public ushort		Offset { get ; private set ; }

			/// <summary>
			/// 取得数
			/// </summary>
			public ushort		Length { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetFriends_RequestPacket
			(
				ushort			offset,
				ushort			length
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

				PutUShort( Offset ) ;
				PutUShort( Length ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// フレンド情報群取得の応答パケット
		/// </summary>
		public class GetFriends_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// フレンド情報群
			/// </summary>
			public List<ResponseFriendData>	Friends { get ; private set ; }

			/// <summary>
			/// フレンド総数
			/// </summary>
			public ushort					Count	{ get ; private set ; }

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

					ResponseFriendData friend ;

					ushort i, l = GetVUShort() ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						friend = new ResponseFriendData() ;
						friend.Decode( m_Data, ref m_Pointer ) ;
						friends.Add( friend ) ;
					}

					Friends = friends ;

					Count = GetUShort() ;
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
		/// セッションのスコープタイプの変更の要求パケット
		/// </summary>
		public class SetSessionScopeType_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// セッション識別子
			/// </summary>
			public uint	    			SessionId { get ; private set ; }

			/// <summary>
			/// スコープ種別
			/// </summary>
			public SessionScopeTypes	ScopeType { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public SetSessionScopeType_RequestPacket
			(
				uint				sessionId,
				SessionScopeTypes	scopeType
			)
			{
				RequestType		= RequestTypes.SetSessionScopeType ;

				//-------------

				SessionId		= sessionId ;
				ScopeType		= scopeType ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutUInt( SessionId ) ;
				PutByte( ( byte )ScopeType ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// セッションのスコープタイプの変更の応答パケット
		/// </summary>
		public class SetSessionScopeType_ResponsePacket : ResponsePacketBase
		{
			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public SetSessionScopeType_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
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
		// ユーザー情報の更新

		/// <summary>
		/// ユーザー名更新の要求パケット
		/// </summary>
		public class UpdateUserName_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// 新しいユーザー名
			/// </summary>
			public string			UserName { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public UpdateUserName_RequestPacket
			(
				string	userName
			)
			{
				RequestType		= RequestTypes.UpdateUserName ;

				//-------------

				UserName		= userName ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( UserName ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// ユーザー名更新の応答パケット
		/// </summary>
		public class UpdateUserName_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// 実際に設定されたユーザー名
			/// </summary>
			public string					UserName	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public UpdateUserName_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserName = GetString() ;
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
		// デバッグ用

		/// <summary>
		/// ユーザー情報群取得要求
		/// </summary>
		public class GetUsers_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// 取得開始位置
			/// </summary>
			public ulong			Offset { get ; private set ; }

			/// <summary>
			/// 取得数
			/// </summary>
			public ulong			Length { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetUsers_RequestPacket
			(
				ulong	offset,
				ulong	length
			)
			{
				RequestType		= RequestTypes.GetUsers ;

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

				PutULong( Offset ) ;
				PutULong( Length ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// ユーザー情報群取得の応答パケット
		/// </summary>
		public class GetUsers_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// フレンド情報群
			/// </summary>
			public List<ResponseUserData>	Users { get ; private set ; }

			/// <summary>
			/// フレンド総数
			/// </summary>
			public ulong					Count	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetUsers_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					var users = new List<ResponseUserData>() ;

					ResponseUserData user ;

					ulong i, l = GetVULong() ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						user = new ResponseUserData() ;
						user.Decode( m_Data, ref m_Pointer ) ;
						users.Add( user ) ;
					}

					Users = users ;

					Count = GetULong() ;
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


		/// <summary>
		/// フレンド設定の要求パケット
		/// </summary>
		public class SetFriend_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// 状態
			/// </summary>
			public bool				IsFriend	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public SetFriend_RequestPacket
			(
				string	userId,
				bool	isFriend
			)
			{
				RequestType		= RequestTypes.SetFriend ;

				//-------------

				UserId			= userId ;
				IsFriend		= isFriend ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString ( UserId ) ;
				PutBool( IsFriend ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// フレンド設定の応答パケット
		/// </summary>
		public class SetFriend_ResponsePacket : ResponsePacketBase
		{
			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public SetFriend_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				// 成功
				return true ;
			}
		}




		//-------------------------------------------------------------------------------------------

		// 汎用ＡＰＩコール
		private async Task<( ResponseCodes, string, byte[] )> CallWebApi_C_Async
		(
			IPEndPoint endPoint,
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
					return ( ResponseCodes.BadRequest, "[CommunicationServer] リクエスト情報に誤りがあります(1)", null ) ;
				}
			}
			else
			{
				// 失敗
				return ( ResponseCodes.BadRequest, "[CommunicationServer] リクエスト情報に誤りがあります(2)", null ) ;
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
				m_ClientCancellationTokenSource.Token
			) ;

			//----------------------------------------------------------

			// サーバーに接続を試みる
			var result = await socketClient.ConnectAsync( endPoint, null, cancellationToken ) ;
			if( result == false )
			{
				socketClient.Dispose() ;

				return ( ResponseCodes.ConnectionFailed, "[CommunicationServer] サーバーに接続できません\n" + endPoint.ToString(), null ) ;
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
				return ( ResponseCodes.RequestFailed, "[CommunicationServer] サーバーと通信出来ません(1)\n" + endPoint.ToString(), null ) ;
			}

			//----------------------------------------------------------
			// 受信を待つ

			bool isReceived = false ;
			ReadOnlyMemory<byte> receivedData = default ;

			// 受信コールバック
			void OnTcpReceievedHandler( ReadOnlyMemory<byte> data )
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
				return ( ResponseCodes.RequestFailed, "[CommunicationServer] サーバーと通信出来ません(2)\n" + endPoint.ToString(), null ) ;
			}

			//--------------

			if( receivedData.IsEmpty == true || receivedData.Length <  2 )
			{
				// 失敗(データ異常)

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( ResponseCodes.BadResponse, "[CommunicationServer] 受信データに問題があります(0)", null ) ;
			}

			//----------------------------------------------------------

			int offset = 0 ;

			// レスポンスコードを取得する
			ResponseCodes responseCode ;

			try
			{
				responseCode = ( ResponseCodes )DataFormat.GetUShort( receivedData.Span, ref offset ) ;
			}
			catch( Exception )
			{
				// 失敗(データ異常)

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( ResponseCodes.BadResponse, "[CommunicationServer] 受信データに問題があります(1)", null ) ;
			}

			//----------------------------------------------------------

			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				// エラーメッセージを取得する
				string errorMessage ;

				try
				{
					errorMessage = DataFormat.GetString( receivedData.Span, ref offset ) ;

					Debug.Log( "受信したエラーメッセージ : " + errorMessage ) ;
				}
				catch( Exception )
				{
					// 失敗(データ異常)

					// 切断
//					await m_SocketClient.DisconnectAsync() ;
					socketClient.Disconnect( false ) ;
					socketClient.Dispose() ;

					return ( ResponseCodes.BadResponse, "[CommunicationServer] 受信データに問題があります(2)", null ) ;
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
					responseContentData = m_Crypter.DecryptAes( receivedData.Span, offset, length ) ;
				}
				catch( Exception )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "[CommunicationServer] 受信データに問題があります(3)", null ) ;
				}

				if( responseContentData == null || responseContentData.Length == 0 )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "[CommunicationServer] 受信データに問題があります(4)", null ) ;
				}
			}

			return ( ResponseCodes.Succeeded, string.Empty, responseContentData ) ;
		}
	}
}
