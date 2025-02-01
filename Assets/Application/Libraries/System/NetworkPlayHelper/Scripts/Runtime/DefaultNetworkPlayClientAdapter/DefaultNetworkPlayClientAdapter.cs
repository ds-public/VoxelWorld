using System ;
using System.Collections.Generic ;

using System.Threading ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 標準のアダプター
	/// </summary>
	public partial class DefaultNetworkPlayClientAdapter : INetworkPlayClientAdapter
	{
		// カスタムセッションプロセッサー
		private ISessionProcessor		m_SessionProcessor ;

		/// <summary>
		/// セッションプロセッサーを設定する(Abstruct)
		/// </summary>
		/// <param name="sessionProcessor"></param>
		public void SetSessionProcessor( ISessionProcessor sessionProcessor )
		{
			m_SessionProcessor = sessionProcessor ;
		}

		/// <summary>
		/// アプリケーション識別子を設定する
		/// </summary>
		/// <param name="applicationId"></param>
		public void SetApplicationId( string applicationId )
		{
			m_ApplicationId = applicationId ;
		}

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		private string	m_ApplicationId ;

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public	string	ApplicationId	=> m_ApplicationId ;

		//-------------------------------------------------------------------------------------------

		// ユーザー識別子
		private string	m_UserId ;

		/// <summary>
		/// ユーザー識別子(確認専用)
		/// </summary>
		public string	UserId => m_UserId ;


		// ユーザー名
		private string	m_UserName ;

		/// <summary>
		/// ユーザー名
		/// </summary>
		public string UserName	=> m_UserName ;


		// アクセストークン
		private string	m_AccessToken ;

		/// <summary>
		/// アクセストークン(確認専用)
		/// </summary>
		public string	AccessToken => m_AccessToken ;


		/// <summary>
		/// アクセストークンの有効期限
		/// </summary>
		protected long	m_AccessLimit ;

		/// <summary>
		/// アクセストークンの有効期限(確認専用)
		/// </summary>
		public long		AccessLimit => m_AccessLimit ;


		// ログアウト用に共通鍵は保持しておく
		private byte[]	m_CommonKey ;

		/// <summary>
		/// 共通鍵
		/// </summary>
		public byte[]	CommonKey => m_CommonKey ;

		/// <summary>
		/// ログイン済みかどうか
		/// </summary>
		public bool		IsLogin
		{
			get
			{
				return ( string.IsNullOrEmpty( m_AccessToken ) == false ) ;
			}
		}

		//---------------

		// 共通鍵の暗号器
		private Security.Crypter				m_Crypter ;

		//-----------------------------------

		// ＴＣＰの最大パケットサイズ
		private int m_MaxTcpPacketSize = 65536 ;

		/// <summary>
		/// ＴＣＰの最大パケットサイズを設定する
		/// </summary>
		/// <param name="maxTcpPacketSize"></param>
		public void SetMaxTcpPacketSize( int maxTcpPacketSize )
		{
			m_MaxTcpPacketSize = maxTcpPacketSize ;
		}

		//-----------------------------------------------------------

		// メインスレッドのコンテキスト
		private SynchronizationContext			m_MainContext ;

		// オーナーのキャンセレーショントークン
		private CancellationToken				m_OwnerCancellationToken ;

		/// <summary>
		/// 自身のキャンセレーショントークンソース
		/// </summary>
		protected CancellationTokenSource		m_ClientCancellationTokenSource ;

		//-------------------------------------------------------------------------------------------

		// シグネチャ
		private readonly byte[]					m_Signature = new byte[]{ ( byte )'N', ( byte )'P', ( byte )'H', ( byte )'P' } ;

		// バージョンコード
		private readonly int					m_VersionCode = 2 ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ownerCancellationToken"></param>
		public DefaultNetworkPlayClientAdapter( SynchronizationContext mainContext, CancellationToken ownerCancellationToken )
		{
			// メインスレッドのコンテキストを記録
			m_MainContext				= mainContext ;

			// オーナーのキャンセレーショントークンを記録
			m_OwnerCancellationToken	= ownerCancellationToken ;

			if( m_OwnerCancellationToken == default )
			{
				m_ClientCancellationTokenSource = new () ;
			}
			else
			{
				m_ClientCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_OwnerCancellationToken ) ;
			}
		}

		/// <summary>
		/// 破棄
		/// </summary>
		public void Dispose()
		{
			if( m_ClientCancellationTokenSource != null )
			{
				if( m_ClientCancellationTokenSource.IsCancellationRequested == false )
				{
					m_ClientCancellationTokenSource.Cancel() ;
				}

				m_ClientCancellationTokenSource.Dispose() ;
				m_ClientCancellationTokenSource = null ;
			}

			//----------------------------------

			m_ActiveFrames.Clear() ;
			m_ActiveFrames_ForSessionProcessor.Clear() ;

			DeleteRealTimeSocketClient() ;

			if( m_Crypter != null )
			{
				m_Crypter.Dispose() ;
				m_Crypter = null ;
			}

			m_OwnerCancellationToken = default ;
		}
	}
}
