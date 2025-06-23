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
		/// アカウントサーバーの情報を設定する
		/// </summary>
		/// <param name="SetLoginServer"></param>
		/// <param name="loginServerPort"></param>
		public void SetAccountServer( string accountServerAddress, int accountServerPort )
		{
			m_NetworkPlayClientAdapter.SetAccountServer( accountServerAddress, accountServerPort ) ;
		}

		/// <summary>
		/// アカウントサーバーのアドレス
		/// </summary>
		public string	AccountServerAddress
		{
			get
			{
				return m_NetworkPlayClientAdapter.AccountServerAddress ;
			}
		}

		/// <summary>
		/// アカウントサーバーのポート
		/// </summary>
		public int		AccountServerTcpPort
		{
			get
			{
				return m_NetworkPlayClientAdapter.AccountServerTcpPort ;
			}
		}

		/// <summary>
		/// サーバーの公開鍵を設定する(設定してもしなくてもどちらでも良い)
		/// </summary>
		/// <param name="serverPublicKey"></param>
		public void SetServerPublicKey( string serverPublicKey )
		{
			m_NetworkPlayClientAdapter.SetServerPublicKey( serverPublicKey ) ;
		}

		/// <summary>
		/// クライアントの公開鍵・秘密鍵を設定する(ログイン前に必要：設定しなくても動作するがクライアントごとに異なるものを設定する事を強く推奨)
		/// </summary>
		/// <param name="publicKey"></param>
		public void SetClientKeys( string publicKey, string secretKey )
		{
			m_NetworkPlayClientAdapter.SetClientKeys( publicKey, secretKey ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アカウントサーバーに対し任意データの送受信を実行する
		/// </summary>
		/// <returns></returns>
		public Task<GetStatus_Response> GetStatusAsync
		(
			byte[] data,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.GetStatusAsync
			(
				data,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しログインを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Login_Response> LoginAsync
		(
			string userId,
			string password,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.LoginAsync
			(
				userId,
				password,
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する
		/// </summary>
		/// <returns></returns>
		public Task<WebApiResponseBase> LogoutAsync
		(
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.LogoutAsync
			(
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しリフレッシュを実行する
		/// </summary>
		/// <returns></returns>
		public Task<Refresh_Response> RefreshAsync
		(
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.RefreshAsync
			(
				cancellationToken
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しゲストアカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public Task<CreateGuestAccount_Response> CreateGuestAccountAsync
		(
			string userName,
			CancellationToken cancellationToken = default
		)
		{
			return m_NetworkPlayClientAdapter.CreateGuestAccountAsync
			(
				userName,
				cancellationToken
			) ;
		}
	}
}
