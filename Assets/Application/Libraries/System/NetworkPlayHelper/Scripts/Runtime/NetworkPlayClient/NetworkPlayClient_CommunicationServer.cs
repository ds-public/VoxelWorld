using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// NetworkPlay 機能のクライアント側の管理用クラス
	/// </summary>
	public partial class NetworkPlayClient
	{
		/// <summary>
		/// コミュニケーションサーバーのアドレス(確認専用)　※グローバル
		/// </summary>
		public string CommunicationServer_Address
		{
			get
			{
				return m_NetworkPlayClientAdapter.CommunicationServer_Address ;
			}
		}

		/// <summary>
		/// コミュニケーションサーバーのポート(確認専用)
		/// </summary>
		public ushort CommunicationServer_TcpPort
		{
			get
			{
				return m_NetworkPlayClientAdapter.CommunicationServer_TcpPort ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 任意機能を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CallFunction_Response> CallFunctionAsync
		(
			byte[] data,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.CallFunctionAsync
			(
				data,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// グルーピングサービスを開始する
		/// </summary>
		/// <param name="onGeneralMessageReceived"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<StartGroupingService_Response> StartGroupingServiceAsync
		(
			Dictionary<string,string>	parameters,
			Action<byte[]>				onMessageReceived,
			CancellationToken			cancellationToken	
		)
		{
			return m_NetworkPlayClientAdapter.StartGroupingServiceAsync
			(
				parameters,
				onMessageReceived,
				cancellationToken
			) ;
		}

		/// <summary>
		/// グルーピングサービスを終了する
		/// </summary>
		/// <returns></returns>
		public bool StopGroupingService()
		{
			return m_NetworkPlayClientAdapter.StopGroupingService() ;
		}

		/// <summary>
		/// グルーピングサービスの固有パラメータを設定する
		/// </summary>
		/// <param name="onGeneralMessageReceived"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<SetGroupingServiceParameter_Response> SetGroupingServiceParameterAsync
		(
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken	
		)
		{
			return m_NetworkPlayClientAdapter.SetGroupingServiceParameterAsync
			(
				parameters,
				cancellationToken
			) ;
		}

		/// <summary>
		/// グルーピングサーバーに接続中かどうか
		/// </summary>
		public bool IsGroupingServerConnected
		{
			get
			{
				return m_NetworkPlayClientAdapter.IsGroupingServerConnected ;
			}
		}

		//-----------------------------------
#if MATCHING_SYSTEM_OLD_VERSION
		/// <summary>
		/// セッションへのマッチング要求を開始する(旧版)
		/// </summary>
		/// <param name="description"></param>
		/// <param name="maxPlayers"></param>
		/// <param name="password"></param>
		/// <param name="scopeType"></param>
		/// <param name="managementType"></param>
		/// <param name="udpEnabled"></param>
		/// <param name="udpCorrectionEnabled"></param>
		/// <param name="playerName"></param>
		/// <param name="onMatchingToSession"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<StartMatchingToSession_Response> StartMatchingToSessionAsync
		(
			string							description,
			string							password,
			ushort							maxPlayers,
			SessionScopeTypes				scopeType,
			SessionManagementTypes			managementType,
			bool							udpEnabled,
			bool							udpCorrectionEnabled,
			Dictionary<string,string>       parameters,
			string							playerName,
			Action<MatchingToSessionResult>	onMatchingToSession,
			CancellationToken				cancellationToken
		)
		{
			return m_NetworkPlayClientAdapter.StartMatchingToSessionAsync
			(
				description,
				password,
				maxPlayers,
				scopeType,
				managementType,
				udpEnabled,
				udpCorrectionEnabled,
				parameters,
				playerName,
				onMatchingToSession,
				cancellationToken
			) ;
		}

		/// <summary>
		/// セッションへのマッチング要求を停止する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<StopMatchingToSession_Response> StopMatchingToSessionAsync
		(
			CancellationToken				cancellationToken	
		)
		{
			return m_NetworkPlayClientAdapter.StopMatchingToSessionAsync
			(
				cancellationToken
			) ;
		}
#endif
		//-----------------------------------

		/// <summary>
		/// セッション生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateSession_Response> CreateSessionAsync
		(
			string                      description				= "MyRoom",
			string                      password				= null,
			int                         maxPlayers				= 4,
			SessionScopeTypes           scopeType				= SessionScopeTypes.Public,
			SessionManagementTypes      managementType	        = SessionManagementTypes.HostManagement,
			bool                        udpEnabled				= true,
			bool                        udpCorrectionEnabled	= true,
			Dictionary<string,string>   parameters              = null,

			string                      playerName				= null,
			CancellationToken           cancellationToken		= default
		)
		{
			return m_NetworkPlayClientAdapter.CreateSessionAsync
			(
				description,
				password,
				maxPlayers,
				scopeType,
				managementType,
				udpEnabled,
				udpCorrectionEnabled,
				parameters,
				playerName,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// セッション参加を実行する
		/// </summary>
		/// <returns></returns>
		public Task<JoinToSession_Response> JoinToSessionAsync
		(
			ulong   sessionId,
			string  password = null,
			string  playerName = null,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.JoinToSessionAsync
			(
				sessionId,
				password,
				playerName,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// セッション情報取得を実行する
		/// </summary>
		/// <returns></returns>
		public Task<GetSessions_Response> GetSessionsAsync
		(
			uint	offset = 0,
			uint	length = 0,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.GetSessionsAsync
			(
				offset,
				length,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// フレンド情報取得を実行する
		/// </summary>
		/// <returns></returns>
		public Task<GetFriends_Response> GetFriendsAsync
		(
			ushort	offset = 0,
			ushort	length = 0,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.GetFriendsAsync
			(
				offset,
				length,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// セッションのスコープタイプを設定する(セッションに参加済み且つホストである場合のみ使用可能)
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="scopeType"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<SetSessionScopeType_Response> SetSessionScopeTypeAsync
		(
			SessionScopeTypes		scopeType,
			CancellationToken		cancellationToken	
		)
		{
			return m_NetworkPlayClientAdapter.SetSessionScopeTypeAsync
			(
				scopeType,
				cancellationToken
			) ;
		}

		//-----------------------------------------------------------
		// ユーザー情報の更新

		/// <summary>
		/// ユーザー名を更新する
		/// </summary>
		/// <returns></returns>
		public Task<UpdateUserName_Response> UpdateUserNameAsync
		(
			string	userName,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.UpdateUserNameAsync
			(
				userName,
				cancellationToken
			) ;
		}

		//-----------------------------------------------------------
		// デバッグ機能

		//-----------------------------------

		/// <summary>
		/// ユーザー情報群の取得を実行する
		/// </summary>
		/// <returns></returns>
		public Task<GetUsers_Response> GetUsersAsync
		(
			ulong	offset = 0,
			ulong	length = 0,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.GetUsersAsync
			(
				offset,
				length,
				cancellationToken
			) ;
		}

		/// <summary>
		/// フレンド設定を実行する
		/// </summary>
		/// <returns></returns>
		public Task<SetFriend_Response> SetFriendAsync
		(
			string	userId,
			bool	isFriend,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.SetFriendAsync
			(
				userId,
				isFriend,
				cancellationToken
			) ;
		}

		//-------------------------------------------------------------------------------------------

	}
}
