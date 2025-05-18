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
		public string CommunicationServerAddress
		{
			get
			{
				return m_NetworkPlayClientAdapter.CommunicationServerAddress ;
			}
		}

		/// <summary>
		/// コミュニケーションサーバーのポート(確認専用)
		/// </summary>
		public int CommunicationServerPort
		{
			get
			{
				return m_NetworkPlayClientAdapter.CommunicationServerPort ;
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
		/// セッション生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateSession_Response> CreateSessionAsync
		(
			string description						= "MyRoom",
			int maxPlayers							= 4,
			string password							= null,
			SessionScopeTypes scopeType				= SessionScopeTypes.Public,
			SessionManagementTypes managementType	= SessionManagementTypes.HostManagement,
			bool udpEnabled							= true,
			bool udpCorrectionEnabled				= true,
			string playerName						= null,
			CancellationToken cancellationToken		= default
		)
		{
			return m_NetworkPlayClientAdapter.CreateSessionAsync
			(
				description,
				maxPlayers,
				password,
				scopeType,
				managementType,
				udpEnabled,
				udpCorrectionEnabled,
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
			int offset = 0,
			int length = 0,
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
			int offset = 0,
			int length = 0,
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
	}
}
