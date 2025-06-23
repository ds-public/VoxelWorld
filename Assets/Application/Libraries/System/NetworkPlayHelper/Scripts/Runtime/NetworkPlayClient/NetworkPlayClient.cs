using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;



namespace NetworkPlayHelper
{
	/// <summary>
	/// NetworkPlay 機能のクライアント側の管理用クラス Version 2025/06/18
	/// </summary>
	public partial class NetworkPlayClient
	{
		//-------------------------------------------------------------------------------------------

		// オーナーのキャンセレーショントークン(このキャンセルで全てのタスクがキャンセルされる)
		private CancellationToken					    m_OwnerCancellationToken ;

		// アダプターのインスタンス
		private INetworkPlayClientAdapter			   m_NetworkPlayClientAdapter ;

		// デフォルトアダプタかどうか
		private bool								  m_IsDefaultAdapter ;

		/// <summary>
		/// デフォルトアダプタであるかどうか
		/// </summary>
		public bool IsDefaultAdapter
		{
			get
			{
				return m_IsDefaultAdapter ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ownerCancellationToken"></param>
		public NetworkPlayClient( CancellationToken ownerCancellationToken )
		{
			// オーナーキャンセレーショントークンを記録
			m_OwnerCancellationToken	= ownerCancellationToken ;

			//----------------------------------

			// デフォルトのネットワーククライアントアダプターをカレントのアダプターとして設定する
			m_NetworkPlayClientAdapter = new DefaultNetworkPlayClientAdapter( m_OwnerCancellationToken ) ;
			m_IsDefaultAdapter = true ;
		}

		/// <summary>
		/// ネットワークプレイクライアントアダプターを上書きする
		/// </summary>
		/// <param name="adapter"></param>
		public void OverwriteAdapter( INetworkPlayClientAdapter networkPlayClientAdapter )
		{
			if( networkPlayClientAdapter == null )
			{
				// null はデフォルトに戻す

				if( m_IsDefaultAdapter == false )
				{
					// 現在のアダプターがデフォルトのもので無い場合のみ処理する

					// 既存のもの(デフォルトでないアダプター)を破棄する
					m_NetworkPlayClientAdapter?.Dispose() ;

					// デフォルトのネットワーククライアントアダプターをカレントのアダプターとして設定する
					m_NetworkPlayClientAdapter = new DefaultNetworkPlayClientAdapter( m_OwnerCancellationToken ) ;
					m_IsDefaultAdapter = true ;
				}

				return ;
			}

			//-------------------------------------------------

			// 既存のものを破棄する
			m_NetworkPlayClientAdapter?.Dispose() ;

			// 上書き
			m_NetworkPlayClientAdapter = networkPlayClientAdapter ;
			m_IsDefaultAdapter = false ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションプロセッサーを設定する
		/// </summary>
		/// <param name="sessionProcessorType"></param>
		public void SetSessionProcessor( ISessionProcessor sessionProcessor )
		{
			m_NetworkPlayClientAdapter.SetSessionProcessor( sessionProcessor ) ;
		}

		/// <summary>
		/// アプリケーション識別子を設定する
		/// </summary>
		/// <param name="applicationId"></param>
		public void SetApplicationId( string applicationId )
		{
			m_NetworkPlayClientAdapter.SetApplicationId( applicationId ) ;
		}

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public string ApplicationId
		{
			get
			{
				return m_NetworkPlayClientAdapter.ApplicationId ;
			}
		}

		/// <summary>
		/// ユーザー識別子(確認専用)
		/// </summary>
		public string	UserId
		{
			get
			{
				return m_NetworkPlayClientAdapter.UserId ;
			}
		}

		/// <summary>
		/// ユーザー識別子(確認専用)
		/// </summary>
		public string	UserName
		{
			get
			{
				return m_NetworkPlayClientAdapter.UserName ;
			}
		}

		/// <summary>
		/// アクセストークン(確認専用)
		/// </summary>
		public string AccessToken
		{
			get
			{
				return m_NetworkPlayClientAdapter.AccessToken ;
			}
		}

		/// <summary>
		/// アクセストークンの有効期限(確認専用)
		/// </summary>
		public long AccessLimit
		{
			get
			{
				return m_NetworkPlayClientAdapter.AccessLimit ;
			}
		}

		/// <summary>
		/// 共通鍵(確認専用)
		/// </summary>
		public byte[] CommonKey
		{
			get
			{
				return m_NetworkPlayClientAdapter.CommonKey ;
			}
		}

		/// <summary>
		/// ログインしているかどうか
		/// </summary>
		public bool IsLogin
		{
			get
			{
				return m_NetworkPlayClientAdapter.IsLogin ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 破棄を実行する
		/// </summary>
		public void Dispose()
		{
			m_NetworkPlayClientAdapter.Dispose() ;
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// セッション情報
	/// </summary>
	public class Session
	{
		/// <summary>
		/// セッション識別子
		/// </summary>
		public ulong	SessionId { get ; private set ; }

		/// <summary>
		/// セッションの説明文
		/// </summary>
		public string	Description { get ; private set ; }

		/// <summary>
		/// 最大の人数
		/// </summary>
		public int		MaxPlayers { get ; private set ; }

		/// <summary>
		/// セッションパスワードが必要かどうか
		/// </summary>
		public bool		PasswordRequired { get ; private set ; }

		/// <summary>
		/// 現在の人数
		/// </summary>
		public int		NowPlayers { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Session()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Session
		(
			ulong   sessionId,
			string  description,
			int     maxPlayers,
			bool    passwordRequired,
			int     nowPlayers
		)
		{
			SessionId			= sessionId ;
			Description			= description ;
			MaxPlayers			= maxPlayers ;
			PasswordRequired	= passwordRequired ;
			NowPlayers			= nowPlayers ;
		}
	}

	/// <summary>
	/// セッションのプレイヤー情報
	/// </summary>
	public class SessionPlayer
	{
		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId { get ; private set ; }

		/// <summary>
		/// ユーザー名またはセッションプレイヤー名
		/// </summary>
		public string	Name { get ; private set ; }

		/// <summary>
		/// ゲストであるかどうか
		/// </summary>
		public bool		IsGuest { get ; private set ; }

		/// <summary>
		/// セッションのホストであるかどうか
		/// </summary>
		public bool		IsHost { get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="userName"></param>
		/// <param name="isHost"></param>
		public SessionPlayer( string userId, string name, bool isGuest, bool isHost )
		{
			UserId		= userId ;
			Name		= name ;
			IsGuest		= isGuest ;
			IsHost		= isHost ;
		}
	}
}
