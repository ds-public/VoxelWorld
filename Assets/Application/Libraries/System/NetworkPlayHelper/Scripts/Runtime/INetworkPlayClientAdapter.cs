using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 基本通信機能の定義インターフェース
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

		//-------------------------------------------------------------------------------------------
		// アカウント関連

		/// <summary>
		/// アカウントサーバーの情報を設定する
		/// </summary>
		/// <param name="loginServerAddress"></param>
		/// <param name="loginServerPort"></param>
		public void SetAccountServer( string accountServerAddress, int accountServerPort ) ;

		/// <summary>
		/// アカウントサーバーのアドレス
		/// </summary>
		public string	AccountServerAddress { get ; }

		/// <summary>
		/// アカウントサーバーのポート
		/// </summary>
		public int		AccountServerPort { get ; }


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
		/// アカウントサーバーに対しログインを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Login_Response> LoginAsync
		(
			string userId,
			string password,
			CancellationToken cancellationToken = default
		) ;

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する
		/// </summary>
		/// <returns></returns>
		public Task<WebApiResponseBase> LogoutAsync
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

		/// <summary>
		/// アカウントサーバーに対しゲストアカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateGuestAccount_Response> CreateGuestAccountAsync
		(
			string userName,
			CancellationToken cancellationToken = default
		) ;

		//-------------------------------------------------------------------------------------------
		// コミュニケーション関連

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string	CommunicationServerAddress { get ; }

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public int		CommunicationServerPort { get ; }

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
			string					description,
			int						maxPlayers,
			string					password,
			SessionScopeTypes		sessionScopeType,
			SessionManagementTypes	sessionManagementType,
			bool					udpEnabled,
			bool					udpCorrectionEnabled,
			string					playerName,
			CancellationToken		cancellationToken	
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
			string					sessionId,
			string					password,
			string					playerName,
			CancellationToken		cancellationToken	
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
			int offset,
			int length,
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
			int offset,
			int length,
			CancellationToken cancellationToken
		) ;

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

		//-------------------------------------------------------------------------------------------
		// セッション関連

		/// <summary>
		/// セッションサーバーのアドレス
		/// </summary>
		public string					SessionServerAddress { get ; }

		/// <summary>
		/// セッションサーバーのポート
		/// </summary>
		public int						SessionServerPort { get ; }


		/// <summary>
		/// セッション識別子
		/// </summary>
		public string					SessionId { get ; }

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
		/// セッション内プレイヤー情報群を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer[]	GetSessionPlayers() ;

		/// <summary>
		/// セッション内のホストプレイヤーの情報を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer GetSessionHostPlayer() ;

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
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType ) ;

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue() ;

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
			PacketTypes packetTypes,
			byte[] data,
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
	/// ログインのレスポンス
	/// </summary>
	public class Login_Response : WebApiResponseBase
	{
		/// <summary>
		/// ユーザー名
		/// </summary>
		public string			UserName { get ; private set ; }

		/// <summary>
		/// アクセストークン
		/// </summary>
		public string			AccessToken { get ; private set ; }

		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		public long				AccessLimit { get ; private set ; }

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]			CommonKey { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string			CommunicationServerAddress { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public int				CommunicationServerPort { get ; private set ; }

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
			string errorMessage,
			string userName,
			string accessToken,
			long   accessLimit,
			byte[] commonKey,
			string communicationServerAddress,
			int    communicationServerPort
		) : base( responseCode, errorMessage )
		{
			UserName					= userName ;
			AccessToken					= accessToken ;
			AccessLimit					= accessLimit ;
			CommonKey					= commonKey ;
			CommunicationServerAddress	= communicationServerAddress ;
			CommunicationServerPort		= communicationServerPort ;
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
		/// ユーザー名
		/// </summary>
		public string			UserName { get ; private set ; }

		/// <summary>
		/// アクセストークン
		/// </summary>
		public string			AccessToken { get ; private set ; }

		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		public long				AccessLimit { get ; private set ; }

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]			CommonKey { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのアドレス
		/// </summary>
		public string			CommunicationServerAddress { get ; private set ; }

		/// <summary>
		/// コミュニケーションサーバーのポート
		/// </summary>
		public int				CommunicationServerPort { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public Refresh_Response
		(
			ResponseCodes responseCode,
			string errorMessage,
			string userName,
			string accessToken,
			long   accessLimit,
			byte[] commonKey,
			string communicationServerAddress,
			int    communicationServerPort
		) : base( responseCode, errorMessage )
		{
			UserName					= userName ;
			AccessToken					= accessToken ;
			AccessLimit					= accessLimit ;
			CommonKey					= commonKey ;
			CommunicationServerAddress	= communicationServerAddress ;
			CommunicationServerPort		= communicationServerPort ;
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
		public string UserId { get ; private set ; }

		/// <summary>
		/// パスワード
		/// </summary>
		public string Password { get ; private set ; }

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
			string password
		) : base( responseCode, errorMessage )
		{
			UserId		= userId ;
			Password	= password ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// セッション生成のレスポンス
	/// </summary>
	public class CreateSession_Response : WebApiResponseBase
	{
		/// <summary>
		/// セッション識別子
		/// </summary>
		public string					SessionId { get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// セッションの最大参加可能人数
		/// </summary>
		public int						MaxPlayers { get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// セッションの管理タイプ
		/// </summary>
		public SessionManagementTypes	ManagementType { get ; private set ; }

		//---------------

		/// <summary>
		/// ＵＤＰを使用できるかどうか
		/// </summary>
		public bool						UdpEnabled { get ; private set ; }

		/// <summary>
		/// ＵＤＰの誤り補正を行うかどうか
		/// </summary>
		public bool						UdpCorrectionEnabled { get ; private set ; }

		//---------------

		/// <summary>
		/// セッションプロセッサーが使用可能かどうか
		/// </summary>
		public bool						ProcessorEnabled { get ; private set ; }

		//-----------------------------------

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
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public CreateSession_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage,
			string					sessionId,
			int						maxPlayers,
			SessionManagementTypes	managementType,
			bool					udpEnabled,
			bool					udpCorrectionEnabled,
			bool					processorEnabled,
			string					sessionServerAddress,
			int						sessionServerPort
		) : base( responseCode, errorMessage )
		{
			SessionId				= sessionId ;

			MaxPlayers				= maxPlayers ;

			ManagementType			= managementType ;
			
			UdpEnabled				= udpEnabled ;
			UdpCorrectionEnabled	= udpCorrectionEnabled ;

			ProcessorEnabled		= processorEnabled ;

			SessionServerAddress	= sessionServerAddress ;
			SessionServerPort		= sessionServerPort ;
		}
	}

	/// <summary>
	/// セッション参加のレスポンス
	/// </summary>
	public class JoinToSession_Response : WebApiResponseBase
	{
		/// <summary>
		/// 最大プレイヤー数
		/// </summary>
		public int						MaxPlayers { get ; private set ; }

		//---------------

		/// <summary>
		/// セッションの管理方法
		/// </summary>
		public SessionManagementTypes	ManagementType { get ; private set ; }

		/// <summary>
		/// ＵＤＰを使用できるかどうか
		/// </summary>
		public bool						UdpEnabled { get ; private set ; }

		/// <summary>
		/// ＵＤＰの誤り補正を行うかどうか
		/// </summary>
		public bool						UdpCorrectionEnabled { get ; private set ; }

		/// <summary>
		/// セッションプロセッサーが有効な状態かどうか
		/// </summary>
		public bool						ProcessorEnabled { get ; private set ; }

		//---------------

		/// <summary>
		/// セッションサーバーのアドレス
		/// </summary>
		public string					SessionServerAddress { get ; private set ; }

		/// <summary>
		/// セッションサーバーのポート番号
		/// </summary>
		public int						SessionServerPort { get ; private set ; }

		//---------------

		/// <summary>
		/// セッションに参加中のメンバー情報
		/// </summary>
		public SessionPlayer[]			SessionPlayers { get ; private set ; }

		//----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="errorMessage"></param>
		/// <param name="UserId"></param>
		/// <param name="Password"></param>
		public JoinToSession_Response
		(
			ResponseCodes			responseCode,
			string					errorMessage,
			int						maxPlayers,
			SessionManagementTypes	managementType,
			bool					udpEnabled,
			bool					udpCorrectionEnabled,
			bool					processorEnabled,
			string					sessionServerAddress,
			int						sessionServerPort,
			SessionPlayer[]			sessionPlayers
		) : base( responseCode, errorMessage )
		{
			MaxPlayers				= maxPlayers ;
			ManagementType			= managementType ;
			UdpEnabled				= udpEnabled ;
			UdpCorrectionEnabled	= udpCorrectionEnabled ;
			ProcessorEnabled		= processorEnabled ;
			SessionServerAddress	= sessionServerAddress ;
			SessionServerPort		= sessionServerPort ;
			SessionPlayers			= sessionPlayers ;
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
		public Session[]	Sessions { get ; private set ; }

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
			Session[]					sessions
			) : base( responseCode, errorMessage )
		{
			Sessions			= sessions ;
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
		public ResponseFriendData[]	Friends { get ; private set ; }

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
			ResponseFriendData[]		friends
			) : base( responseCode, errorMessage )
		{
			Friends			= friends ;
		}
	}

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

	//------------------------------------

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
}
