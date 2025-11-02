using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 基本通信機能の定義インターフェース Version 2025/10/29
	/// </summary>
	public interface INetworkPlayClientAdapter
	{
		//-------------------------------------------------------------------------------------------
		// 全体共通関係

		/// <summary>
		/// セッションプロセッサーを設定する
		/// </summary>
		/// <param name="sessionProcessor"></param>
		public void SetSessionProcessor( ISessionProcessor sessionProcessor ) ;

		/// <summary>
		/// アプリケーション識別子を設定する
		/// </summary>
		/// <param name="applicationId"></param>
		public void SetApplicationId( string applicationId ) ;

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public string	ApplicationId { get ; }

		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId { get ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string	Password { get ; }

		/// <summary>
		/// ユーザー名
		/// </summary>
		public string	UserName { get ; }

		/// <summary>
		/// アクセストークン
		/// </summary>
		public string	AccessToken { get ; }

		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		public long		AccessLimit {  get ; }

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]	CommonKey { get ; }

		/// <summary>
		/// ログイン済みかどうか
		/// </summary>
		public bool		IsLogin { get ; }

		//-----------------------------------
		// パフォーマンス計測用の機能

		/// <summary>
		/// 往復時間の計測を行うかどうか
		/// </summary>
		public bool			UsePing { get ; set ; }

		/// <summary>
		/// 往復時間計測のパケットタイプ
		/// </summary>
		public PacketTypes	PingPacketType { get ; set ; }

		//-----

		/// <summary>
		/// サーバー宛の Ping の往復時間[最小]
		/// </summary>
		public long		PingToServer_Min { get ; }

		/// <summary>
		/// サーバー宛の Ping の往復時間[平均]
		/// </summary>
		public long		PingToServer_Avarage { get ; }

		/// <summary>
		/// サーバー宛の Ping の往復時間[最新]
		/// </summary>
		public long		PingToServer { get ; }

		/// <summary>
		/// サーバー宛の Ping の往復時間[最大]
		/// </summary>
		public long		PingToServer_Max { get ; }

		//-----

		/// <summary>
		/// ホスト宛の Ping の往復時間[最小]
		/// </summary>
		public long		PingToHost_Min { get ; }

		/// <summary>
		/// ホスト宛の Ping の往復時間[平均]
		/// </summary>
		public long		PingToHost_Avarage { get ; }

		/// <summary>
		/// ホスト宛の Ping の往復時間[最新]
		/// </summary>
		public long		PingToHost { get ; }

		/// <summary>
		/// ホスト宛の Ping の往復時間[最大]
		/// </summary>
		public long		PingToHost_Max { get ; }

		//-------------------------------------------------------------------------------------------
		// アカウント関連

		/// <summary>
		/// アカウントサーバーの情報を設定する
		/// </summary>
		/// <param name="loginServerAddress"></param>
		/// <param name="loginServerPort"></param>
		public bool SetAccountServer( string accountServerAddress, int accountServerTcpPort ) ;

		/// <summary>
		/// アカウントサーバーのアドレス
		/// </summary>
		public string	AccountServer_Address { get ; }

		/// <summary>
		/// アカウントサーバーのポート
		/// </summary>
		public int		AccountServer_TcpPort { get ; }


		/// <summary>
		/// サーバーの公開鍵を設定する(設定してもしなくてもどちらでも良い)
		/// </summary>
		/// <param name="serverPublicKey"></param>
		public void SetServerPublicKey( string serverPublicKey ) ;

		/// <summary>
		/// クライアントの公開鍵・秘密鍵を設定する(ログイン前に必要：設定しなくても動作するがクライアントごとに異なるものを設定する事を強く推奨)
		/// </summary>
		/// <param name="publicKey"></param>
		public void SetClientKeys( string publicKey, string secretKey ) ;

		//---------------

		/// <summary>
		/// アカウントサーバーと任意データの送受信を行う
		/// </summary>
		/// <param name="data"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetStatus_Response> GetStatusAsync
		(
			byte[] data,
			CancellationToken cancellationToken = default
		) ;

		/// <summary>
		/// アカウントサーバーに対しゲストアカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateGuestAccount_Response> CreateGuestAccountAsync
		(
			string userName,
			CancellationToken cancellationToken = default
		) ;

		/// <summary>
		/// アカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateAccount_Response> CreateAccountAsync
		(
			string	            userId,		// 空文字可能
			string	            password,	// 空文字可能
			string	            userName,	// 空文字可能
			CancellationToken   cancellationToken = default
		) ;

		/// <param name="userName"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<CreatePlatformAccount_Response> CreatePlatformAccountAsync
		(
			string				platformUserId,
			int                 platformCode,
			string              password,
			string				userName,
			CancellationToken	cancellationToken = default
		) ;

		/// <summary>
		/// カウントを引継する(プラットフォームに紐づけ)
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TakeOverPlatformAccount_Response> TakeOverPlatformAccountAsync
		(
			string				platformUserId,
			int                 platformCode,
			string              userId,
			string              password,
			CancellationToken	cancellationToken = default
		) ;

		/// <summary>
		/// アカウント生成またはログインを実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateAccountOrLogin_Response> CreateAccountOrLoginAsync
		(
			string                      userId,
			string                      userName,
			bool                        isGroupingServiceEnabled,
			Dictionary<string,string>   parameters          = null,
			Action<byte[]>              onMessageReceived   = null,
			CancellationToken           cancellationToken = default
		) ;

		/// <summary>
		/// アカウントサーバーに対しログインを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Login_Response> LoginAsync
		(
			string                      userId,
			string                      password,
			bool                        isGroupingServiceEnabled,
			Dictionary<string,string>   parameters          = null,
			Action<byte[]>              onMessageReceived   = null,
			CancellationToken           cancellationToken = default
		) ;

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する(クライアントのみ情報を消去する)
		/// </summary>
		/// <returns></returns>
		public void Logout
		(
		) ;

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Logout_Response> LogoutAsync
		(
			CancellationToken cancellationToken = default
		) ;

		/// <summary>
		/// アカウントサーバーに対しリフレッシュを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Refresh_Response> RefreshAsync
		(
			CancellationToken cancellationToken = default
		) ;


		//-------------------------------------------------------------------------------------------
		// コミュニケーション関連

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string	CommunicationServer_Address { get ; }

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public ushort	CommunicationServer_TcpPort { get ; }

		/// <summary>
		/// 任意機能を実行する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<CallFunction_Response> CallFunctionAsync
		(
			byte[] data,
			CancellationToken cancellationToken
		) ;

		/// <summary>
		/// グルーピングサービスを開始する
		/// </summary>
		/// <param name="onMessageReceived"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<StartGroupingService_Response> StartGroupingServiceAsync
		(
			Dictionary<string,string>	parameters,
			Action<byte[]>				onMessageReceived,
			CancellationToken			cancellationToken	
		) ;

		/// <summary>
		/// グルーピングサービスを終了する
		/// </summary>
		/// <returns></returns>
		public bool StopGroupingService() ;

		/// <summary>
		/// グルーピングサービスの固有パラメータを設定する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<SetGroupingServiceParameter_Response> SetGroupingServiceParameterAsync
		(
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken	
		) ;

		/// <summary>
		/// グルーピングサーバーに接続中かどうか
		/// </summary>
		public bool IsGroupingServerConnected { get ; }

#if MATCHING_SYSTEM_OLD_VERSION
		/// <summary>
		/// セッションへのマッチングを開始する(旧版)
		/// </summary>
		/// <param name="onSessionJoined"></param>
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
			Action<MatchingToSessionResult>	onMatchinToSession,
			CancellationToken				cancellationToken
		) ;

		/// <summary>
		/// セッションへのマッチングを中断する(旧版)
		/// </summary>
		/// <returns></returns>
		public Task<StopMatchingToSession_Response> StopMatchingToSessionAsync
		(
			CancellationToken			cancellationToken	
		) ;
#endif
		//-----------------------------------------------------------
		// グループ関係

		/// <summary>
		/// 関連性のあるユーザー情報群
		/// </summary>
		public List<RelatedUserData>		RelatedUsers		{ get ; }

		/// <summary>
		/// グループ招待情報群
		/// </summary>
		public List<GroupInvitationData>	GroupInvitations	{ get ; }

		/// <summary>
		/// グループメンバー情報群
		/// </summary>
		public List<GroupMemberData>		GroupMembers		{ get ; }

		//---------------

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType			{ get ; }

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId				{ get ; }

		/// <summary>
		/// グループのリーダーであるかどうか
		/// </summary>
		public bool							IsGroupLeader		{ get ; }

		/// <summary>
		/// グループの準備完了を実行中か
		/// </summary>
		public bool							IsGroupReady		{ get ; }


		//-----------------------------------

		/// <summary>
		/// フレンド群の状態を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetFriendsForGrouping_Response> GetFriendsForGroupingAsync
		(
			ushort offset,
			ushort length,
			CancellationToken cancellationToken
		) ;

		//-----------------------------------
		// コールバック管理

		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated ) ;

		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated ) ;

		/// <summary>
		/// グループ招待に応答があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded ) ;

		/// <summary>
		/// グループ招待に応答があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded ) ;

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated ) ;

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated ) ;

		/// <summary>
		/// グループパラータの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated ) ;

		/// <summary>
		/// グループパラメータの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated ) ;

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated ) ;

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated ) ;

		/// <summary>
		///マッチングが開始された際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated ) ;

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated ) ;

		/// <summary>
		/// マッチング中かどうか
		/// </summary>
		public bool	IsMatchingRunning { get ; }

		/// <summary>
		/// マッチングの取消が可能かどうか
		/// </summary>
		public bool IsMatchingStoppable { get ; }

		//-----------------------------------

		/// <summary>
		/// グループ招待を実行する
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<AffordGroupInvitation_Response> AffordGroupInvitationAsync
		(
			string						applicationId,		// アプリケーション識別子
			string						userId,				// 招待対象のユーザー識別子
			Dictionary<string, string>	parameters,			// 招待者の固有パラメータ
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループ招待を取消する
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<CancelGroupInvitation_Response> CancelGroupInvitationAsync
		(
			string userId,							// 招待対象のユーザー識別子
			CancellationToken cancellationToken
		) ;

		/// <summary>
		/// グループ招待を承諾する　※メンバー限定行動
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<AcceptGroupInvitation_Response> AcceptGroupInvitationAsync
		(
			string						userId,							// 招待対象のユーザー識別子
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループへの招待を拒否する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<RejectGroupInvitation_Response> RejectGroupInvitationAsync
		(
			string						userId,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループへ参加する　※フリーユーザー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<JoinToGroup_Response> JoinToGroupAsync
		(
			string						userId,
			uint						groupId,
			string						password,
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループ固有パラメータを設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<SetGroupParameter_Response> SetGroupParameterAsync
		(
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループのメンバー固有パラメータを設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<SetGroupMemberParameter_Response> SetGroupMemberParameterAsync
		(
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループのメンバー準備可能を設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<SetGroupMemberReady_Response> SetGroupMemberReadyAsync
		(
			bool                        isReady,
			Dictionary<string,string>   parameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループから離脱(グループを解散)する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<LeaveFromGroup_Response> LeaveFromGroupAsync
		(
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// メンバーをグループから排除する　※リーダー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<RejectGroupMember_Response> RejectGroupMemberAsync
		(
			string						userId,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループを生成する　※フリーユーザー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<CreateGroup_Response> CreateGroupAsync
		(
			string						applicationId,		// アプリケーション識別子
			GroupTypes					groupType,
			string						password,
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// ソロマッチングを開始する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<StartSoloMatching_Response> StartSoloMatchingAsync
		(
			string						applicationId,		// アプリケーション識別子
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// グループマッチングを開始する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<StartGroupMatching_Response> StartGroupMatchingAsync
		(
			Dictionary<string,string>	groupParameters,
			CancellationToken			cancellationToken
		) ;

		/// <summary>
		/// マッチングを取消する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public Task<StopMatching_Response> StopMatchingAsync
		(
			CancellationToken			cancellationToken
		) ;

		//-----------------------------------------------------------


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
		public Task<CreateSession_Response> CreateSessionAsync
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
		) ;

		/// <summary>
		/// セッションに参加する
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="sessionPassword"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<JoinToSession_Response> JoinToSessionAsync
		(
			ulong					    sessionId,
			string					    password,
			string					    playerName,
			CancellationToken		    cancellationToken	
		) ;

		/// <summary>
		/// セッション情報を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetSessions_Response> GetSessionsAsync
		(
			uint	offset,
			uint	length,
			CancellationToken cancellationToken
		) ;

		/// <summary>
		/// フレンド情報を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetFriends_Response> GetFriendsAsync
		(
			ushort	offset,
			ushort	length,
			CancellationToken cancellationToken
		) ;

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
		) ;

		//-----------------------------------
		// ユーザー情報の更新

		/// <summary>
		/// ユーザー名を更新する
		/// </summary>
		/// <returns></returns>
		public Task<UpdateUserName_Response> UpdateUserNameAsync
		(
			string	userName,
			CancellationToken cancellationToken = default
		) ;

		//-----------------------------------
		// デバッグ機能


		/// <summary>
		/// ユーザー情報を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<GetUsers_Response> GetUsersAsync
		(
			ulong	offset,
			ulong	length,
			CancellationToken cancellationToken
		) ;

		/// <summary>
		/// ユーザーをフレンド登録するまたはフレンド解除する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<SetFriend_Response> SetFriendAsync
		(
			string	friendUserId,
			bool	isFriend,
			CancellationToken cancellationToken
		) ;

		//-------------------------------------------------------------------------------------------
		// エクスチェンジ関連

		/// <summary>
		/// エクスチェンジサーバーのアドレス
		/// </summary>
		public string					ExchangeServer_Address { get ; }

		/// <summary>
		/// エクスチェンジサーバーのＴＣＰポート
		/// </summary>
		public ushort					ExchangeServer_TcpPort { get ; }

		/// <summary>
		/// エクスチェンジサーバーのＵＤＰポート
		/// </summary>
		public ushort					ExchangeServer_UdpPort { get ; }

		/// <summary>
		/// セッションに参加中かどうか
		/// </summary>
		public bool                     IsSessionJoined { get ; }

		/// <summary>
		/// セッション識別子
		/// </summary>
		public ulong					SessionId { get ; }

		/// <summary>
		/// セッションのスコープタイプ
		/// </summary>
		public SessionScopeTypes		SessionScopeType { get ; }


		/// <summary>
		/// セッションの最大人数
		/// </summary>
		public int						MaxPlayers { get ; }

		/// <summary>
		/// セッションの管理方法
		/// </summary>
		public SessionManagementTypes	ManagementType { get ; }

		/// <summary>
		/// ＵＤＰ通信が有効になっているかどうか
		/// </summary>
		public bool						UdpEnabled { get ; }

		/// <summary>
		/// ＵＤＰ通信でリオーダー＆ロストが発生した際の補正が有効になっているかどうか
		/// </summary>
		public bool						UdpCorrectionEnabled { get ; }

		/// <summary>
		/// データの送信受信の準備が整っているかどうか
		/// </summary>
		public bool						Ready { get ; }

		/// <summary>
		/// セッションのホストであるかどうか
		/// </summary>
		public bool						IsHost { get ; }

		/// <summary>
		/// ローカルループバックが有効かどうか
		/// </summary>
		public bool						LocalLoopbackEnabled { get ; set ; }

		/// <summary>
		/// プレイヤー名
		/// </summary>
		public string					PlayerName { get ; }

		//---------------

		/// <summary>
		/// セッションの固有パラメータ群を取得する
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,string>    GetSessionParameters() ;

		/// <summary>
		/// セッション内プレイヤー情報群を取得する
		/// </summary>
		/// <returns></returns>
		public List<SessionPlayer>		    GetSessionPlayers() ;

		/// <summary>
		/// セッション内のホストプレイヤーの情報を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer                GetSessionHostPlayer() ;

		/// <summary>
		/// 指定したユーザー識別子のセッション内プレイヤー情報を取得する
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public SessionPlayer GetSessionPlayer( string userId ) ;

		//-----------------------------------

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext ) ;

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue() ;

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType_ForSessionProcessor( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext ) ;

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue_ForSessionProcessor() ;

		//-----------------------------------------------------------

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
		) ;

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
			PacketTypes packetTypes,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		) ;

		/// <summary>
		/// 対象プレイヤーをキックする(ホストのみ可能)
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool Kick( string userId ) ;

		//---------------

		/// <summary>
		/// 自身がセッションに参加した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onConnected"></param>
		public void SetOnConnected( Action onConnected ) ;

		/// <summary>
		/// データを受信した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnReceived
		(
			Action<byte[],SourceTypes,string> onReceived
		) ;

		/// <summary>
		/// セッション内プレイヤーの参加と離脱が行われた際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onPlayerJoined"></param>
		/// <param name="onPlayerLeft"></param>
		public void SetOnPlyerChanged
		(
			Action<SessionPlayer> onPlayerJoined,
			Action<SessionPlayer> onPlayerLeft
		) ;

		/// <summary>
		/// 自身がセッションから離脱した際に呼び出されるコールバックを設定する
		/// </summary>
		/// <param name="onDisconnected"></param>
		public void SetOnDisconnected( Action onDisconnected ) ;

		//---------------

		/// <summary>
		/// セッションから離脱する
		/// </summary>
		public void LeaveFromSession() ;

		//---------------

		/// <summary>
		/// セッションのアラート群
		/// </summary>
		public List<SessionAlertTypes>	SessionAlerts { get ; }

		/// セッションアラート受信コールバックを追加する
		/// </summary>
		/// <param name="m_OnSessionAlertReceived"></param>
		public void AddOnSessionAlertReceived( Action<SessionAlertTypes> onSessionAlertReceived ) ;

		/// <summary>
		/// セッションアラート受信コールバックを削除する
		/// </summary>
		/// <param name="m_OnSessionAlertReceived"></param>
		public void RemoveOnSessionAlertReceived( Action<SessionAlertTypes> onSessionAlertReceived ) ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 破棄を行う
		/// </summary>
		public void Dispose() ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// 任意データの送受信のレスポンス
	/// </summary>
	public class GetStatus_Response : WebApiResponseBase
	{
		/// <summary>
		/// 任意データ
		/// </summary>
		public byte[]			Data { get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public GetStatus_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			byte[] data
		) : base( responseCode, errorMessage )
		{
			Data						= data ;
		}
	}

	/// <summary>
	/// ゲストアカウント生成のレスポンス
	/// </summary>
	public class CreateGuestAccount_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId		{ get ; private set ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string	Password	{ get ; private set ; }

		/// <summary>
		/// 実際に設定されたユーザー名
		/// </summary>
		public string	UserName	{  get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateGuestAccount_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			string userId,
			string password,
			string userName
		) : base( responseCode, errorMessage )
		{
			UserId		= userId ;
			Password	= password ;
			UserName	= userName ;
		}
	}

	/// <summary>
	/// アカウント生成のレスポンス
	/// </summary>
	public class CreateAccount_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId		{ get ; private set ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string	Password	{ get ; private set ; }

		/// <summary>
		/// 実際に設定されたユーザー名
		/// </summary>
		public string	UserName	{  get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateAccount_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			string userId,
			string password,
			string userName
		) : base( responseCode, errorMessage )
		{
			UserId		= userId ;
			Password	= password ;
			UserName	= userName ;
		}
	}

	/// <summary>
	/// プラットフォームアカウント生成のレスポンス
	/// </summary>
	public class CreatePlatformAccount_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId		{ get ; private set ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string	Password	{ get ; private set ; }

		/// <summary>
		/// 実際に設定されたユーザー名
		/// </summary>
		public string	UserName	{  get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreatePlatformAccount_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			string userId,
			string password,
			string userName
		) : base( responseCode, errorMessage )
		{
			UserId		= userId ;
			Password	= password ;
			UserName	= userName ;
		}
	}

	/// <summary>
	/// プラットフォームアカウント引継のレスポンス
	/// </summary>
	public class TakeOverPlatformAccount_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId		{ get ; private set ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string	Password	{ get ; private set ; }

		/// <summary>
		/// 実際に設定されたユーザー名
		/// </summary>
		public string	UserName	{  get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public TakeOverPlatformAccount_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			string userId,
			string password,
			string userName
		) : base( responseCode, errorMessage )
		{
			UserId		= userId ;
			Password	= password ;
			UserName	= userName ;
		}
	}

	/// <summary>
	/// アカウント生成またはログインのレスポンス
	/// </summary>
	public class CreateAccountOrLogin_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// アクセストークン
		/// </summary>
		public string	AccessToken					{ get ; private set ; }

		/// <summary>
		/// アクセスリミット
		/// </summary>
		public long		AccessLimit					{ get ; private set ; }

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]	CommonKey					{  get ; private set ; }


		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string	CommunicationServer_Address	{ get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのＴＣＰポート
		/// </summary>
		public ushort	CommunicationServer_TcpPort	{ get ; private set ; }

		/// <summary>
		/// グルーピングサーバーのアドレス
		/// </summary>
		public string   GroupingServer_Address      { get ; private set ; }

		/// <summary>
		/// グルーピングサーバーのＴＣＰポート
		/// </summary>
		public ushort   GroupingServer_TcpPort      { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateAccountOrLogin_Response
		(
			ResponseCodes responseCode,
			string errorMessage,

			string	accessToken,
			long	accessLimit,
			byte[]	commonKey,

			string	communicationServer_Address,
			ushort	communicationServer_TcpPort,
			string  groupingServer_Address,
			ushort  groupingServer_TcpPort

		) : base( responseCode, errorMessage )
		{
			AccessToken					= accessToken ;
			AccessLimit					= accessLimit ;
			CommonKey					= commonKey ;

			CommunicationServer_Address	= communicationServer_Address ;
			CommunicationServer_TcpPort	= communicationServer_TcpPort ;
			GroupingServer_Address      = groupingServer_Address ;
			GroupingServer_TcpPort      = groupingServer_TcpPort ;
		}
	}

	/// <summary>
	/// ログインのレスポンス
	/// </summary>
	public class Login_Response : WebApiResponseBase
	{
		/// <summary>
		/// ユーザー名
		/// </summary>
		public string	UserName { get ; private set ; }

		/// <summary>
		/// アクセストークン
		/// </summary>
		public string	AccessToken { get ; private set ; }

		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		public long		AccessLimit { get ; private set ; }

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]	CommonKey { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string	CommunicationServer_Address { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public ushort	CommunicationServer_TcpPort { get ; private set ; }

		/// <summary>
		/// グルーピングサーバーのアドレス
		/// </summary>
		public string   GroupingServer_Address      { get ; private set ; }

		/// <summary>
		/// グルーピングサーバーのＴＣＰポート
		/// </summary>
		public ushort   GroupingServer_TcpPort      { get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public Login_Response
		(
			ResponseCodes responseCode,
			string	errorMessage,
			string	userName,
			string	accessToken,
			long	accessLimit,
			byte[]	commonKey,
			string	communicationServer_Address,
			ushort	communicationServer_TcpPort,
			string  groupingServer_Address,
			ushort  groupingServer_TcpPort
		) : base( responseCode, errorMessage )
		{
			UserName					= userName ;
			AccessToken					= accessToken ;
			AccessLimit					= accessLimit ;
			CommonKey					= commonKey ;

			CommunicationServer_Address	= communicationServer_Address ;
			CommunicationServer_TcpPort	= communicationServer_TcpPort ;
			GroupingServer_Address      = groupingServer_Address ;
			GroupingServer_TcpPort      = groupingServer_TcpPort ;
		}
	}

	/// <summary>
	/// ログアウトのレスポンス
	/// </summary>
	public class Logout_Response : WebApiResponseBase
	{
		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public Logout_Response
		(
			ResponseCodes responseCode,
			string	errorMessage
		) : base( responseCode, errorMessage )
		{
		}
	}

	/// <summary>
	/// リフレッシュのレスポンス
	/// </summary>
	public class Refresh_Response : WebApiResponseBase
	{
		//-----------------------------------------------------------
		// 固有パラメータ

		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		public long				AccessLimit { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public Refresh_Response
		(
			ResponseCodes responseCode,
			string	errorMessage
		) : base( responseCode, errorMessage )
		{
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public Refresh_Response
		(
			long	accessLimit
		) : base( ResponseCodes.Succeeded, string.Empty )
		{
			AccessLimit					= accessLimit ;
		}
	}


	//--------------------------------------------------------------------------------------------
	// レスポンス情報定義


	/// <summary>
	/// グルーピングサービサービス開始のレスポンス
	/// </summary>
	public class StartGroupingService_Response : WebApiResponseBase
	{
		/// <summary>
		/// グルーピングサーバーのアドレス
		/// </summary>
		public string					GroupingServer_Address	{ get ; private set ; }

		/// <summary>
		/// グルーピングサーバーのＴＣＰポート
		/// </summary>
		public ushort					GroupingServer_TcpPort	{ get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public StartGroupingService_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage,

			string					groupingServer_Address,
			ushort					groupingServer_TcpPort

		) : base( responseCode, errorMessage )
		{
			GroupingServer_Address	= groupingServer_Address ;
			GroupingServer_TcpPort	= groupingServer_TcpPort ;
		}
	}

	/// <summary>
	/// グルーピングサービサービス固有パラメータの設定のレスポンス
	/// </summary>
	public class SetGroupingServiceParameter_Response : WebApiResponseBase
	{
		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public SetGroupingServiceParameter_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage

		) : base( responseCode, errorMessage )
		{
		}
	}

	//------------------------------------

	/// <summary>
	/// セッション生成のレスポンス
	/// </summary>
	public class CreateSession_Response : WebApiResponseBase
	{
		/// <summary>
		/// セッション識別子
		/// </summary>
		public ulong					    SessionId				{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// セッションの最大参加可能人数
		/// </summary>
		public int						    MaxPlayers				{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// セッションの管理タイプ
		/// </summary>
		public SessionManagementTypes	    ManagementType			{ get ; private set ; }

		//---------------

		/// <summary>
		/// ＵＤＰを使用できるかどうか
		/// </summary>
		public bool						    UdpEnabled				{ get ; private set ; }

		/// <summary>
		/// ＵＤＰの誤り補正を行うかどうか
		/// </summary>
		public bool						    UdpCorrectionEnabled    { get ; private set ; }

		//---------

		/// <summary>
		/// セッション固有パラメータ
		/// </summary>
		public Dictionary<string,string>    Parameters              {  get ; private set ; }

		//---------------

		/// <summary>
		/// セッションプロセッサーが使用可能かどうか
		/// </summary>
		public bool						    ProcessorEnabled		{ get ; private set ; }

		//---------------

		/// <summary>
		/// セッションに参加中のメンバー情報
		/// </summary>
		public List<SessionPlayer>		    SessionPlayers			{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// エクスチェンジサーバーのアドレス
		/// </summary>
		public string					    ExchangeServer_Address	{ get ; private set ; }

		/// <summary>
		/// エクスチェンジサーバーのＴＣＰポート番号
		/// </summary>
		public ushort					    ExchangeServer_TcpPort	{ get ; private set ; }

		/// <summary>
		/// エクスチェンジサーバーのＵＤＰポート番号
		/// </summary>
		public ushort					    ExchangeServer_UdpPort	{ get ; private set ; }


		//----------------------------------------------------------


		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateSession_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage
		) : base( responseCode, errorMessage )
		{
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateSession_Response
		(
			ResponseCodes			    responseCode,
			string					    errorMessage,
			ulong					    sessionId,
			int						    maxPlayers,
			SessionManagementTypes	    managementType,
			bool					    udpEnabled,
			bool					    udpCorrectionEnabled,
			Dictionary<string,string>   parameters,
			bool					    processorEnabled,

			List<SessionPlayer>		    sessionPlayers,

			string					    exchangeServer_Address,
			ushort					    exchangeServer_TcpPort,
			ushort					    exchangeServer_UdpPort

		) : base( responseCode, errorMessage )
		{
			SessionId				= sessionId ;

			MaxPlayers				= maxPlayers ;
			ManagementType			= managementType ;
			UdpEnabled				= udpEnabled ;
			UdpCorrectionEnabled	= udpCorrectionEnabled ;
			Parameters              = parameters ;
			ProcessorEnabled		= processorEnabled ;

			SessionPlayers			= sessionPlayers ;

			ExchangeServer_Address	= exchangeServer_Address ;
			ExchangeServer_TcpPort	= exchangeServer_TcpPort ;
			ExchangeServer_UdpPort	= exchangeServer_UdpPort ;
		}
	}

	/// <summary>
	/// セッション参加のレスポンス
	/// </summary>
	public class JoinToSession_Response : WebApiResponseBase
	{
		/// <summary>
		/// セッション識別子
		/// </summary>
		public ulong					    SessionId				{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// 最大プレイヤー数
		/// </summary>
		public int						    MaxPlayers              { get ; private set ; }

		//---------------

		/// <summary>
		/// セッションの管理方法
		/// </summary>
		public SessionManagementTypes	    ManagementType          { get ; private set ; }

		/// <summary>
		/// ＵＤＰを使用できるかどうか
		/// </summary>
		public bool						    UdpEnabled              { get ; private set ; }

		/// <summary>
		/// ＵＤＰの誤り補正を行うかどうか
		/// </summary>
		public bool						    UdpCorrectionEnabled    { get ; private set ; }


		//---------

		/// <summary>
		/// セッション固有パラメータ
		/// </summary>
		public Dictionary<string,string>    Parameters              {  get ; private set ; }

		/// <summary>
		/// セッションプロセッサーが有効な状態かどうか
		/// </summary>
		public bool						    ProcessorEnabled        { get ; private set ; }


		//---------------

		/// <summary>
		/// セッションに参加中のメンバー情報
		/// </summary>
		public List<SessionPlayer>		    SessionPlayers          { get ; private set ; }

		//---------------

		/// <summary>
		/// エクスチェンジサーバーのアドレス
		/// </summary>
		public string					    ExchangeServer_Address  { get ; private set ; }

		/// <summary>
		/// エクスチェンジサーバーのＴＣＰポート番号
		/// </summary>
		public ushort					    ExchangeServer_TcpPort  { get ; private set ; }

		/// <summary>
		/// エクスチェンジサーバーのＵＤＰポート番号
		/// </summary>
		public ushort					    ExchangeServer_UdpPort  { get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public JoinToSession_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage
		) : base ( responseCode, errorMessage )
		{
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public JoinToSession_Response
		(
			ResponseCodes			    responseCode,
			string					    errorMessage,
			ulong					    sessionId,
			int						    maxPlayers,
			SessionManagementTypes	    managementType,
			bool					    udpEnabled,
			bool					    udpCorrectionEnabled,
			Dictionary<string,string>   parameters,
			bool					    processorEnabled,

			List<SessionPlayer>		    sessionPlayers,

			string					    exchangeServer_Address,
			ushort					    exchangeServer_TcpPort,
			ushort					    exchangeServer_UdpPort

		) : base( responseCode, errorMessage )
		{
			SessionId				= sessionId ;

			MaxPlayers				= maxPlayers ;
			ManagementType			= managementType ;
			UdpEnabled				= udpEnabled ;
			UdpCorrectionEnabled	= udpCorrectionEnabled ;
			Parameters              = parameters ;
			ProcessorEnabled		= processorEnabled ;

			SessionPlayers			= sessionPlayers ;

			ExchangeServer_Address	= exchangeServer_Address ;
			ExchangeServer_TcpPort	= exchangeServer_TcpPort ;
			ExchangeServer_UdpPort	= exchangeServer_UdpPort ;
		}
	}

	/// <summary>
	/// セッション情報取得のレスポンス
	/// </summary>
	public class GetSessions_Response : WebApiResponseBase
	{
		/// <summary>
		/// セッション情報
		/// </summary>
		public List<Session>			Sessions	{ get ; private set ; }

		/// <summary>
		/// セッション総数
		/// </summary>
		public uint						Count		{ get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public GetSessions_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,
			List<Session>				sessions,
			uint						count

		) : base( responseCode, errorMessage )
		{
			Sessions					= sessions ;
			Count						= count ;
		}
	}

	/// <summary>
	/// フレンド情報取得のレスポンス
	/// </summary>
	public class GetFriends_Response : WebApiResponseBase
	{
		/// <summary>
		/// フレンド情報
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
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public GetFriends_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,
			List<ResponseFriendData>	friends,
			ushort						count

		) : base( responseCode, errorMessage )
		{
			Friends						= friends ;
			Count						= count ;
		}
	}

	/// <summary>
	/// セッションのスコープタイプの設定のレスポンス
	/// </summary>
	public class SetSessionScopeType_Response : WebApiResponseBase
	{
		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public SetSessionScopeType_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage
		) : base( responseCode, errorMessage )
		{
		}
	}

	/// <summary>
	/// ユーザー名更新のレスポンス
	/// </summary>
	public class UpdateUserName_Response : WebApiResponseBase
	{
		/// <summary>
		/// 実際に設定されたユーザー名
		/// </summary>
		public string						UserName					{ get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public UpdateUserName_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,

			string						userName

		) : base( responseCode, errorMessage )
		{
			UserName					= userName ;
		}
	}

	/// <summary>
	/// ユーザー情報群取得のレスポンス
	/// </summary>
	public class GetUsers_Response : WebApiResponseBase
	{
		/// <summary>
		/// ユーザー情報群
		/// </summary>
		public List<ResponseUserData>		Users					{ get ; private set ; }

		/// <summary>
		/// ユーザー総数
		/// </summary>
		public ulong						Count					{ get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public GetUsers_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,

			List<ResponseUserData>		users,
			ulong						count

		) : base( responseCode, errorMessage )
		{
			Users						= users ;
			Count						= count ;
		}
	}

	/// <summary>
	/// フレンド設定のレスポンス
	/// </summary>
	public class SetFriend_Response : WebApiResponseBase
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public SetFriend_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage

		) : base( responseCode, errorMessage )
		{
		}
	}



	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// セッションへのマッチング開始のレスポンス
	/// </summary>
	public class StartMatchingToSession_Response : WebApiResponseBase
	{
		/// <summary>
		/// マッチング識別子
		/// </summary>
		public ulong						MatchingId				{ get ; private set ; }

		public MatchingToSessionResult		Result					{ get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public StartMatchingToSession_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,

			ulong						matchingId,
			MatchingToSessionResult		result
		) : base( responseCode, errorMessage )
		{
			MatchingId	= matchingId ;
			Result		= result ;
		}
	}

	/// <summary>
	/// セッションへのマッチング中断のレスポンス
	/// </summary>
	public class StopMatchingToSession_Response : WebApiResponseBase
	{
		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public StopMatchingToSession_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage
		) : base( responseCode, errorMessage )
		{
		}
	}


	//------------------------------------------------------------

	/// <summary>
	/// 任意機能の実行のレスポンス
	/// </summary>
	public class CallFunction_Response : WebApiResponseBase
	{
		/// <summary>
		/// 任意レスポンスデータ
		/// </summary>
		public byte[]	Data { get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CallFunction_Response
		(
			ResponseCodes				responseCode,
			string						errorMessage,
			byte[]						data
		) : base( responseCode, errorMessage )
		{
			Data			= data ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// WebAPi 系のレスポンスの基底クラス
	/// </summary>
	public class WebApiResponseBase
	{
		/// <summary>
		/// レスポンスコード
		/// </summary>
		public ResponseCodes ResponseCode { get ; private set ; }

		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string ErrorMessage { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessgae"></param>
		public WebApiResponseBase( ResponseCodes responseCode, string errorMessgae )
		{
			ResponseCode = responseCode ;
			ErrorMessage = errorMessgae ;
		}
	}

	//--------------------------------------------------------------------------------------------
	// Group 関係

	/// <summary>
	/// 関連性のあるユーザー情報群取得のレスポンス
	/// </summary>
	public class GetFriendsForGrouping_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		public List<RelatedUserData>		RelatedUsers	{ get ; private set ; }

		public int							Count			{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public GetFriendsForGrouping_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;

			RelatedUsers	= null ;
			Count			= 0 ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public GetFriendsForGrouping_Response
		(
			List<RelatedUserData>		relatedUsers,
			int							count
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;

			RelatedUsers	= relatedUsers ;
			Count			= count ;
		}
	}

	/// <summary>
	/// グループ招待の送付のレスポンス
	/// </summary>
	public class AffordGroupInvitation_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public AffordGroupInvitation_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public AffordGroupInvitation_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループ招待の取消のレスポンス
	/// </summary>
	public class CancelGroupInvitation_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public CancelGroupInvitation_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public CancelGroupInvitation_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループ招待の承諾のレスポンス
	/// </summary>
	public class AcceptGroupInvitation_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType		{ get ; private set ; }

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId			{ get ; private set ; }

		/// <summary>
		/// グループメンバー群
		/// </summary>
		public List<GroupMemberData>		GroudMembers	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public AcceptGroupInvitation_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public AcceptGroupInvitation_Response
		(
			GroupTypes				groupType,
			uint					groupId,

			List<GroupMemberData>	groudMembers
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;

			GroupType		= groupType ;
			GroupId			= groupId ;
			GroudMembers	= groudMembers ;
		}
	}

	/// <summary>
	/// グループ招待の拒否のレスポンス
	/// </summary>
	public class RejectGroupInvitation_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public RejectGroupInvitation_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public RejectGroupInvitation_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループ参加のレスポンス
	/// </summary>
	public class JoinToGroup_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType		{ get ; private set ; }

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId			{ get ; private set ; }

		/// <summary>
		/// グループメンバー群
		/// </summary>
		public List<GroupMemberData>		GroudMembers	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public JoinToGroup_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public JoinToGroup_Response
		(
			GroupTypes				groupType,
			uint					groupId,

			List<GroupMemberData>	groudMembers
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;

			GroupType		= groupType ;
			GroupId			= groupId ;
			GroudMembers	= groudMembers ;
		}
	}

	/// <summary>
	/// グループ固有パラメータを設定するのレスポンス　※メンバー限定行動
	/// </summary>
	public class SetGroupParameter_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public SetGroupParameter_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public SetGroupParameter_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループのメンバー固有パラメータを設定するのレスポンス　※メンバー限定行動
	/// </summary>
	public class SetGroupMemberParameter_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public SetGroupMemberParameter_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public SetGroupMemberParameter_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループのメンバー準備可能を設定するのレスポンス　※メンバー限定行動
	/// </summary>
	public class SetGroupMemberReady_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public SetGroupMemberReady_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public SetGroupMemberReady_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループ離脱(解散)のレスポンス
	/// </summary>
	public class LeaveFromGroup_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public LeaveFromGroup_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public LeaveFromGroup_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// メンバー排除のレスポンス
	/// </summary>
	public class RejectGroupMember_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public RejectGroupMember_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public RejectGroupMember_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループ作成のレスポンス
	/// </summary>
	public class CreateGroup_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType		{ get ; private set ; }

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId			{ get ; private set ; }

		/// <summary>
		/// グループメンバー群
		/// </summary>
		public List<GroupMemberData>		GroudMembers	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public CreateGroup_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public CreateGroup_Response
		(
			GroupTypes				groupType,
			uint					groupId,

			List<GroupMemberData>	groudMembers
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;

			GroupType		= groupType ;
			GroupId			= groupId ;
			GroudMembers	= groudMembers ;
		}
	}

	/// <summary>
	/// ソロのマッチング開始のレスポンス
	/// </summary>
	public class StartSoloMatching_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public StartSoloMatching_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public StartSoloMatching_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// グループのマッチング開始のレスポンス
	/// </summary>
	public class StartGroupMatching_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public StartGroupMatching_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public StartGroupMatching_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}

	/// <summary>
	/// マッチング中止のレスポンス
	/// </summary>
	public class StopMatching_Response
	{
		public GroupingActionResponseCodes	ResponseCode	{ get ; private set ; }
		public string						ErrorMessage	{ get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		public StopMatching_Response
		(
			GroupingActionResponseCodes	responseCode,
			string						errorMessage
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
		}

		/// <summary>
		/// コンストラクタ(成功)
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="relatedUsers"></param>
		public StopMatching_Response
		(
		)
		{
			ResponseCode	= GroupingActionResponseCodes.Succeeded ;
			ErrorMessage	= string.Empty ;
		}
	}


}
