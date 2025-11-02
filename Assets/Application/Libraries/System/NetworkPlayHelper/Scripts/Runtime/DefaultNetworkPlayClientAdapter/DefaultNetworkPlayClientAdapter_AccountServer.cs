using System ;
using System.Collections.Generic ;
using System.Linq ;

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
		/// アカウントサーバーの情報を設定する
		/// </summary>
		/// <param name="SetLoginServer"></param>
		/// <param name="loginServerPort"></param>
		public bool SetAccountServer( string accountServer_Address, int accountServer_TcpPort )
		{
			m_AccountServer_Address	= accountServer_Address ;
			m_AccountServer_TcpPort	= accountServer_TcpPort ;

			if( IPAddress.TryParse( accountServer_Address, out IPAddress ipAddress ) == false )
			{
				var ipAddresses = Dns.GetHostAddresses( accountServer_Address ) ;
				if( ipAddresses != null && ipAddresses.Length >  0 )
				{
					foreach( var _ in ipAddresses )
					{
						if( _.AddressFamily == AddressFamily.InterNetwork )
						{
							ipAddress = _ ;
							break ;
						}
					}
				}
			}

			if( ipAddress != null && ipAddress.GetAddressBytes() != null )
			{
				m_AccountServer_TcpEndPoint = new IPEndPoint( ipAddress, accountServer_TcpPort ) ;

				return true ;
			}
			else
			{
				return false ;
			}
		}

		/// <summary>
		/// アカウントサーバーのアドレス
		/// </summary>
		public string	AccountServer_Address => m_AccountServer_Address ;

		// アカウントサーバーのアドレス
		private string							m_AccountServer_Address ;

		/// <summary>
		/// アカウントサーバーのＴＣＰポート
		/// </summary>
		public int		AccountServer_TcpPort	=> m_AccountServer_TcpPort ;

		// アカウントサーバーのＴＣＰポート
		private int								m_AccountServer_TcpPort ;

		// アカウントサーバーのＴＣＰエンドポイント
		private IPEndPoint						m_AccountServer_TcpEndPoint ;

		//-------------------------------------------------------------------------------------------

		protected string					m_ServerPublicKey = string.Empty ;

		/// <summary>
		/// 公開鍵を取得する
		/// </summary>
		/// <returns></returns>
		protected string GetServerPublicKey()
		{
			if( string.IsNullOrEmpty( m_ServerPublicKey ) == false )
			{
				return m_ServerPublicKey ;
			}

			return Security.ShiftAscii( m_ServerPublicKey_Default, -8 ) ;
		}

		/// <summary>
		/// 公開鍵(ログインサーバー)
		/// </summary>
		protected string					m_ServerPublicKey_Default = "DZ[ISm`+it}mFDUwl}t}{F`WYSpI@=.WQ~}Jl)tpOww~M[iZ~LyPZVj*x,.O7SiAkMzuS/;w;NwX~[zU_,iL{_lRTNr9IZkNJU@Wv~|`swjK/o|m+uIP{WXXLkR8Ttl3{rjvU;|Ir^.OUX=At:zk@J8[RSR??kz.iyWs;N?Y?3ilZrWmynWXX+nl_Um@3`w[.QL,7N>/o*K+8sAipn[/Rpu=?W<n_-j{[SQ_ar^8y`v~^vKIZw{M*Kl/>kZoI8{l*7uty:way~^9x/qU/YT`W`8naZ^{TJ},UJmX,?*3ARo}A~sP<iv+kU:-N~^woLk-txr`r|JV9StV^nNitqu[iVAMW?I|[Xq;.uUY^WMK,lzYEED7Uwl}t}{FDM_xwvmv|FIYIJD7M_xwvmv|FD7Z[ISm`+it}mF" ;

		/// <summary>
		/// 公開鍵
		/// </summary>
		protected string					m_ClientPublicKey = "<RSAKeyValue><Modulus>ky0gvetIlm/IvLfGPY+iaQ3pjfh784LRB5m45eEVnd24Rjv6MUzSpc6t74sqYZryd9Cop75xiQ4pCOzDtd5EHg/uaNNpjpb/YjMh220AY1DkutdgxHnQ/Mh1hjlpcP2FprV5T6tTpPQcdojZ8tky+wOPO/UPFk9cL6FCLdnlQw60Eb8AyehERMk8aRkWixBnD/YDKWS6twkXHZ90cvVt7fQt6s4vIPThjHMMOHLYd7CjRQNfhzLPDlTVQFFaQZ9LQOo6ZviGMO8Wusu0N92K+z1vTpb86JzTm6dLLzlujZUiU+FjhsAlYA7vAxyLF7In6cCK+HnP9L9Os83U6s+g/Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>" ;

		/// <summary>
		/// 秘密鍵
		/// </summary>
		protected string					m_ClientSecretKey = "<RSAKeyValue><Modulus>ky0gvetIlm/IvLfGPY+iaQ3pjfh784LRB5m45eEVnd24Rjv6MUzSpc6t74sqYZryd9Cop75xiQ4pCOzDtd5EHg/uaNNpjpb/YjMh220AY1DkutdgxHnQ/Mh1hjlpcP2FprV5T6tTpPQcdojZ8tky+wOPO/UPFk9cL6FCLdnlQw60Eb8AyehERMk8aRkWixBnD/YDKWS6twkXHZ90cvVt7fQt6s4vIPThjHMMOHLYd7CjRQNfhzLPDlTVQFFaQZ9LQOo6ZviGMO8Wusu0N92K+z1vTpb86JzTm6dLLzlujZUiU+FjhsAlYA7vAxyLF7In6cCK+HnP9L9Os83U6s+g/Q==</Modulus><Exponent>AQAB</Exponent><P>+Ko+f/MLRHHwc4NfKvBrMgFOHtuJYpRQE4iWoNMQuuL05hbq8Axk80K0Syhnt8q/ARZtB/CZpXxaqMZEtisiCcjwZgYCwe/h46F67UY3CtG3+Pk/10jNZQQC2evLvmADyA1BLa2aHJrsjAysIv9ThBH5K35AE3iIXxzLSyssnRc=</P><Q>l4SB0GgeEPhOMYTlKw5wktCRbx6kDCPQL6J+wti4VzcaPlTpbfgZ6GurWPZzJtqHwvRzFQ3YJQHkk+BDmHxsLnS7N3KWaykyB/SKCC3aa6u2f9qw2lDWb+00D4CyP8pVUlwee7GhwrbfDxWqrLNGBQXNQs2T+Zvy/WN4cpl4xws=</Q><DP>tnKK2uwjhzumNcrdB69Qp2bnv6JKYgb53esowaU7MDQXhb6o8CnX49g8WqyxtNtQW2bt5pZ01UOxbQXUImjxV4aUQ/cDDPKJpa+0duU+u3R2bHnMipPDB+vyf5wPaIYgICcBfJdUbMqK5pLhtefqigt565x9PQwB2u9Qhb2OxU8=</DP><DQ>SjtVDjG0aUP9qy0cyZdtd8BPQE2WuYviNzQ5PmTHC5One9pF8uaWatQ1QoSbrfFqig0RRMNfneHrhrdc5pwutCPkhSnSn/Wy2UrpRVCRriaWZtVRx+PK61MfKmk26yHJ42vWU2uXgLnvVoia8blzGIrbIVtun9/TkGjnXd3q2jk=</DQ><InverseQ>Bd+1lfE4hNhxxfgue+Zo/D8vxBVE7Pf0rSrOF55Ls4IqtnRmUt2Hl8Y/6aoWNlqFZGyGPFuILlLjazYvAAA/nU6B8U7pc5ptdepyO0qdeHq2VmS0jC+lUsJ6SHURkVKnjoidQgMmqgxyP5KToT2Y6YKvTWHEIGKUbsxzQlj50Tc=</InverseQ><D>EMQxWLF3IXw2mBkvgk/cpq8pDj1ikYkzmvQONlIADm310jp+9CLWVIFJG2L1Pw1R1gh1TjbJ5F0ym4uteAiMJgiWlmaPPelBysQcdUzjoGzUwdxLb9aY0lNb4CCmPHMFSMqfuU3BR6dvnqlUeu/3eNUc+i2evGHqFJsAWQzbq1nR3sg4/UTPHSJwwF08Wjycxwnz658XD0Ei9Bx6cNZC187d2BNJmT5wyK3ZTCEeGrRo88yUhdPHW0q6ibp3keOkENiPNDC0i50yOwxRKykSpIpxVeOf7K09BdmrqW2FDikGldaViG7vNCzDX3XnNUsqvK0Lmzd/DeFvtCNlI7noGQ==</D></RSAKeyValue>" ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// サーバーの公開鍵を設定する(設定してもしなくてもどちらでも良い)
		/// </summary>
		/// <param name="serverPublicKey"></param>
		public void SetServerPublicKey( string serverPublicKey )
		{
			m_ServerPublicKey = serverPublicKey ;
		}

		/// <summary>
		/// クライアントの公開鍵・秘密鍵を設定する(ログイン前に必要：設定しなくても動作するがクライアントごとに異なるものを設定する事を強く推奨)
		/// </summary>
		/// <param name="publicKey"></param>
		public void SetClientKeys( string publicKey, string secretKey )
		{
			m_ClientPublicKey = publicKey ;
			m_ClientSecretKey = secretKey ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アカウントサーバーに対し任意データの送受信を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<GetStatus_Response> GetStatusAsync
		(
			byte[] data,
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new GetStatus_RequestPacket
			(
				data,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
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
			var responseContent = new GetStatus_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null ) ;
			}

			// レスポンスの値を取り出す
			data							= responseContent.Data ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, data ) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しゲストアカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<CreateGuestAccount_Response> CreateGuestAccountAsync
		(
			string userName,
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new CreateGuestAccount_RequestPacket
			(
				userName,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				return new ( responseCode, errorMessage, null, null, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreateGuestAccount_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, null ) ;
			}

			// レスポンスの値を取り出す
			string userId					= responseContent.UserId ;
			string password					= responseContent.Password ;
			userName						= responseContent.UserName ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, userId, password, userName ) ;
		}

		/// <summary>
		/// アカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<CreateAccount_Response> CreateAccountAsync
		(
			string	userId,		// 空文字可能
			string	password,	// 空文字可能
			string	userName,	// 空文字可能
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new CreateAccount_RequestPacket
			(
				userId,
				password,
				userName,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				return new ( responseCode, errorMessage, null, null, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreateAccount_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, null ) ;
			}

			// レスポンスの値を取り出す
			userId					= responseContent.UserId ;
			password				= responseContent.Password ;
			userName				= responseContent.UserName ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, userId, password, userName ) ;
		}

		/// <summary>
		/// プラットフォームアカウント生成を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<CreatePlatformAccount_Response> CreatePlatformAccountAsync
		(
			string	            platformUserId,
			int                 platformCode,
			string	            password,	    // 空文字可能
			string	            userName,	    // 空文字可能
			CancellationToken   cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new CreatePlatformAccount_RequestPacket
			(
				platformUserId,
				platformCode,
				password,
				userName,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, null, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreatePlatformAccount_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, null ) ;
			}

			// レスポンスの値を取り出す
			string userId			= responseContent.UserId ;
			password				= responseContent.Password ;
			userName				= responseContent.UserName ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, userId, password, userName ) ;
		}

		/// <summary>
		/// プラットフォームアカウント引継を実行する
		/// </summary>
		/// <returns></returns>
		public async Task<TakeOverPlatformAccount_Response> TakeOverPlatformAccountAsync
		(
			string	            platformUserId,
			int                 platformCode,
			string              userId,
			string	            password,
			CancellationToken   cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new TakeOverPlatformAccount_RequestPacket
			(
				platformUserId,
				platformCode,
				userId,
				password,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, null, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new TakeOverPlatformAccount_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, null ) ;
			}

			// レスポンスの値を取り出す
			userId                  = responseContent.UserId ;
			password                = responseContent.Password ;
			string userName			= responseContent.UserName ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, userId, password, userName ) ;
		}

		/// <summary>
		/// アカウント生成またはログインを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<CreateAccountOrLogin_Response> CreateAccountOrLoginAsync
		(
			string	                    userId,
			string	                    userName,
			bool                        isGroupingServiceEnabled,
			Dictionary<string,string>   parameters          = null,
			Action<byte[]>              onMessageReceived   = null,
			CancellationToken           cancellationToken = default
		)
		{
			if( string.IsNullOrEmpty( userId ) == true )
			{
				return new ( ResponseCodes.BadResponse, "ユーザー識別子が異常です", null, 0, null, null, 0, null, 0 ) ;
			}

			//----------------------------------

			// リクエストコンテント部
			var requestContent = new CreateAccountOrLogin_RequestPacket
			(
				userId,
				userName,
				isGroupingServiceEnabled,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				return new ( responseCode, errorMessage, null, 0, null, null, 0, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreateAccountOrLogin_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, 0, null, null, 0, null, 0 ) ;
			}

			// ユーザー識別子を記録しておく
			m_UserId						= userId ;

			// レスポンスの値を取り出す
			m_AccessToken					= responseContent.AccessToken ;
			m_AccessLimit					= responseContent.AccessLimit ;
			m_CommonKey						= responseContent.CommonKey ;

			// CommunicationServer のエンドポイントを取り出す
			m_CommunicationServer_Address	= responseContent.CommunicationServer_Address ;
			m_CommunicationServer_TcpPort	= responseContent.CommunicationServer_TcpPort ;

			if( IPAddress.TryParse( m_CommunicationServer_Address, out IPAddress ipAddress ) == false )
			{
				var ipAddresses = Dns.GetHostAddresses( m_CommunicationServer_Address ) ;
				if( ipAddresses != null && ipAddresses.Length >  0 )
				{
					foreach( var _ in ipAddresses )
					{
						// IPv4
						if( _.AddressFamily == AddressFamily.InterNetwork )
						{
							ipAddress = _ ;
							break ;
						}
					}
				}
			}

			if( ipAddress != null && ipAddress.GetAddressBytes() != null )
			{
				m_CommunicationServer_TcpEndPoint = new IPEndPoint( ipAddress, m_CommunicationServer_TcpPort ) ;
			}
			else
			{
				return new
				(
					ResponseCodes.BadResponse,
					"コミュニケーションサーバーのエンドポイントが異常です\n" + m_CommunicationServer_Address + ":" + m_CommunicationServer_TcpPort,
					null, 0, null, null, 0, null, 0
				) ;
			}

			//----------------------------------

			// 共通鍵の暗号器を生成する
			if( m_Crypter != null )
			{
				m_Crypter.Dispose() ;
				m_Crypter = null ;
			}
			m_Crypter = Security.CreateCrypter( m_CommonKey ) ;

			//----------------------------------

			Debug.Log( "<color=#FFFF7F>コミュニケーションサーバーのエンドポイント = " + m_CommunicationServer_TcpEndPoint.ToString() + "</color>" ) ;

			//----------------------------------------------------------
			// グルーピングサービスの利用

			if( isGroupingServiceEnabled == true )
			{
				m_GroupingServerProcessor.Address = responseContent.GroupingServer_Address ;
				m_GroupingServerProcessor.TcpPort = responseContent.GroupingServer_TcpPort ;

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
						null, 0, null, null, 0, null, 0
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
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				m_AccessToken, m_AccessLimit, m_CommonKey,
				m_CommunicationServer_Address, m_CommunicationServer_TcpPort,
				m_GroupingServerProcessor.Address, m_GroupingServerProcessor.TcpPort
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しログインを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<Login_Response> LoginAsync
		(
			string                      userId,
			string                      password,
			bool                        isGroupingServiceEnabled,
			Dictionary<string,string>   parameters          = null,
			Action<byte[]>              onMessageReceived   = null,
			CancellationToken           cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new Login_RequestPacket
			(
				userId,
				password,
				isGroupingServiceEnabled,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, null, 0, null, null, 0, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new Login_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new
				(
					ResponseCodes.BadResponse,
					"受信データに問題があります",
					null, null, 0, null, null, 0, null, 0
				) ;
			}

			// ユーザー識別子を記録しておく
			m_UserId						= userId ;
			m_Password                      = password ;

			// レスポンスの値を取り出す
			m_UserName						= responseContent.UserName ;
			m_AccessToken					= responseContent.AccessToken ;
			m_AccessLimit					= responseContent.AccessLimit ;
			m_CommonKey						= responseContent.CommonKey ;

			// CommunicationServer のエンドポイントを取り出す
			m_CommunicationServer_Address	= responseContent.CommunicationServer_Address ;
			m_CommunicationServer_TcpPort	= responseContent.CommunicationServer_TcpPort ;

			if( IPAddress.TryParse( m_CommunicationServer_Address, out IPAddress ipAddress ) == false )
			{
				var ipAddresses = Dns.GetHostAddresses( m_CommunicationServer_Address ) ;
				if( ipAddresses != null && ipAddresses.Length >  0 )
				{
					foreach( var _ in ipAddresses )
					{
						// IPv4
						if( _.AddressFamily == AddressFamily.InterNetwork )
						{
							ipAddress = _ ;
							break ;
						}
					}
				}
			}

			if( ipAddress != null && ipAddress.GetAddressBytes() != null )
			{
				m_CommunicationServer_TcpEndPoint = new IPEndPoint( ipAddress, m_CommunicationServer_TcpPort ) ;
			}
			else
			{
				return new
				(
					ResponseCodes.BadResponse,
					"コミュニケーションサーバーのエンドポイントが異常です\n" + m_CommunicationServer_Address + ":" + m_CommunicationServer_TcpPort,
					null, null, 0, null, null, 0, null, 0
				) ;
			}

			//----------------------------------

			// 共通鍵の暗号器を生成する
			if( m_Crypter != null )
			{
				m_Crypter.Dispose() ;
				m_Crypter = null ;
			}
			m_Crypter = Security.CreateCrypter( m_CommonKey ) ;

			//----------------------------------

			Debug.Log( "<color=#FFFF7F>コミュニケーションサーバーのエンドポイント = " + m_CommunicationServer_TcpEndPoint.ToString() + "</color>" ) ;

			//----------------------------------------------------------
			// グルーピングサービスの利用

			if( isGroupingServiceEnabled == true )
			{
				m_GroupingServerProcessor.Address = responseContent.GroupingServer_Address ;
				m_GroupingServerProcessor.TcpPort = responseContent.GroupingServer_TcpPort ;

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
						null, null, 0, null, null, 0, null, 0
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
			}

			//----------------------------------------------------------

			// 成功
			return new
			(
				responseCode, string.Empty,
				m_UserName, m_AccessToken, m_AccessLimit, m_CommonKey,
				m_CommunicationServer_Address, m_CommunicationServer_TcpPort,
				m_GroupingServerProcessor.Address, m_GroupingServerProcessor.TcpPort
			) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する(クライアントのみ情報を消去する)
		/// </summary>
		/// <returns></returns>
		public void Logout
		(
		)
		{
			if( string.IsNullOrEmpty( m_UserId ) == true || string.IsNullOrEmpty( m_AccessToken ) == true )
			{
				return ;
			}

			//----------------------------------------------------------

			// セッション参加中であれば切断
			m_ExchangeServerProcessor.CloseSession() ;

			// グルーピングサービス利用中であれば切断
			m_GroupingServerProcessor.CloseService() ;

			//----------------------------------------------------------
			// 情報をクリアする

			m_AccessToken						= null ;
			m_AccessLimit						= 0 ;
			m_CommonKey							= null ;
			m_CommunicationServer_Address		= null ;
			m_CommunicationServer_TcpPort		= 0 ;
			m_CommunicationServer_TcpEndPoint	= null ;

			m_UserId							= null ;
			m_Password							= null ;

			// 暗号器を破棄する
			if( m_Crypter != null )
			{
				m_Crypter.Dispose() ;
				m_Crypter = null ;
			}
		}

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<Logout_Response> LogoutAsync
		(
			CancellationToken cancellationToken = default
		)
		{
			if( string.IsNullOrEmpty( m_UserId ) == true || string.IsNullOrEmpty( m_AccessToken ) == true )
			{
				return new ( ResponseCodes.Error, "ログインを行っていません" ) ;
			}

			//----------------------------------------------------------

			// リクエストコンテント部
			var requestContent = new Logout_RequestPacket
			(
				m_AccessToken,
				m_CommonKey,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var _ ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
				requestContent.Encode(), cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage ) ;
			}

			//----------------------------------------------------------
			// 情報をクリアする

			m_AccessToken						= null ;
			m_AccessLimit						= 0 ;
			m_CommonKey							= null ;
			m_CommunicationServer_Address		= null ;
			m_CommunicationServer_TcpPort		= 0 ;
			m_CommunicationServer_TcpEndPoint	= null ;

			m_UserId							= null ;
//			m_Password							= null ;

			// 暗号器を破棄する
			if( m_Crypter != null )
			{
				m_Crypter.Dispose() ;
				m_Crypter = null ;
			}

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty ) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しリフレッシュを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<Refresh_Response> RefreshAsync
		(
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new Refresh_RequestPacket
			(
				m_AccessToken,
				m_CommonKey,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServer_TcpEndPoint,
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

			// レスポンスコンテント部
			var responseContent = new Refresh_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new
				(
					ResponseCodes.BadResponse, "受信データに問題があります"
				) ;
			}

			// レスポンスの値を取り出す
			m_AccessLimit	= responseContent.AccessLimit ;

			//----------------------------------------------------------

			// 成功
			return new
			(
				m_AccessLimit
			) ;
		}

		//-------------------------------------------------------------------------------------------
		// パケットデータ定義

		/// <summary>
		/// 任意データの送受信の要求パケット
		/// </summary>
		public class GetStatus_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// 任意データ
			/// </summary>
			public byte[]			Data { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetStatus_RequestPacket
			(
				byte[] data,
				string publicKey
			)
			{
				RequestType		= RequestTypes.GetStatus ;

				//-------------

				Data			= data ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutByteArray( Data ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// 任意データの送受信の応答パケット
		/// </summary>
		public class GetStatus_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// 任意データ
			/// </summary>
			public byte[]			Data { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public GetStatus_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					Data						= GetByteArray() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					Data == null || Data.Length == 0
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

		//-----------------------------------

		/// <summary>
		/// ゲストアカウント生成要求
		/// </summary>
		public class CreateGuestAccount_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー名
			/// </summary>
			public string			UserName { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateGuestAccount_RequestPacket
			(
				string userName,
				string publicKey
			)
			{
				RequestType		= RequestTypes.CreateGuestAccount ;

				//-------------

				UserName		= userName ;
				PublicKey		= publicKey ;
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
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// ゲストアカウント応答パケット
		/// </summary>
		public class CreateGuestAccount_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// ユーザー識別子(ゲスト)
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// 実際に設定されたユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateGuestAccount_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserId						= GetString() ;
					Password					= GetString() ;
					UserName					= GetString() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( UserId ) == true ||
					string.IsNullOrEmpty( Password ) == true
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		/// <summary>
		/// アカウント生成の要求パケット
		/// </summary>
		public class CreateAccount_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// ユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateAccount_RequestPacket
			(
				string	userId,
				string	password,
				string	userName,
				string	publicKey
			)
			{
				RequestType		= RequestTypes.CreateAccount ;

				//-------------

				UserId			= userId ;
				Password		= password ;
				UserName		= userName ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( UserId ) ;
				PutString( Password ) ;
				PutString( UserName ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// アカウント作成の応答パケット
		/// </summary>
		public class CreateAccount_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// ユーザー識別子(ゲスト)
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// 実際に設定されたユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateAccount_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserId						= GetString() ;
					Password					= GetString() ;
					UserName					= GetString() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( UserId ) == true ||
					string.IsNullOrEmpty( Password ) == true
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		/// <summary>
		/// プラットフォームアカウント生成の要求パケット
		/// </summary>
		public class CreatePlatformAccount_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// プラットフォームユーザー識別子
			/// </summary>
			public string			PlatformUserId		{ get ; private set ; }

			/// <summary>
			/// プラットフォームコード
			/// </summary>
			public int              PlatformCode        { get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// ユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreatePlatformAccount_RequestPacket
			(
				string	platformUserId,
				int     platformCode,
				string	password,
				string	userName,
				string	publicKey
			)
			{
				RequestType		= RequestTypes.CreatePlatformAccount ;

				//-------------

				PlatformUserId	= platformUserId ;
				PlatformCode    = platformCode ;
				Password		= password ;
				UserName		= userName ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( PlatformUserId ) ;
				PutInt( PlatformCode ) ;
				PutString( Password ) ;
				PutString( UserName ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// プラットフォームアカウント作成の応答パケット
		/// </summary>
		public class CreatePlatformAccount_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// ユーザー識別子(ゲスト)
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// 実際に設定されたユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreatePlatformAccount_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserId						= GetString() ;
					Password					= GetString() ;
					UserName					= GetString() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( UserId ) == true      ||
					string.IsNullOrEmpty( Password ) == true    ||
					string.IsNullOrEmpty( UserName ) == true
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		/// <summary>
		/// プラットフォームアカウント引継の要求パケット
		/// </summary>
		public class TakeOverPlatformAccount_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// プラットフォームユーザー識別子
			/// </summary>
			public string			PlatformUserId		{ get ; private set ; }

			/// <summary>
			/// プラットフォームコード
			/// </summary>
			public int              PlatformCode        { get ; private set ; }

			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId	    { get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public TakeOverPlatformAccount_RequestPacket
			(
				string	platformUserId,
				int     platformCode,
				string  userId,
				string	password,
				string	publicKey
			)
			{
				RequestType		= RequestTypes.TakeOverPlatformAccount ;

				//-------------

				PlatformUserId	= platformUserId ;
				PlatformCode    = platformCode ;
				UserId          = userId ;
				Password		= password ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( PlatformUserId ) ;
				PutInt( PlatformCode ) ;
				PutString( UserId ) ;
				PutString( Password ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// プラットフォームアカウント引継の応答パケット
		/// </summary>
		public class TakeOverPlatformAccount_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// ユーザー識別子(ゲスト)
			/// </summary>
			public string			UserId		{ get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password	{ get ; private set ; }

			/// <summary>
			/// 実際に設定されたユーザー名
			/// </summary>
			public string			UserName	{ get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public TakeOverPlatformAccount_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserId						= GetString() ;
					Password					= GetString() ;
					UserName					= GetString() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( UserId ) == true      ||
					string.IsNullOrEmpty( Password ) == true    ||
					string.IsNullOrEmpty( UserName ) == true
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		/// <summary>
		/// アカウント生成またはログインの要求パケット
		/// </summary>
		public class CreateAccountOrLogin_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId		                { get ; private set ; }

			/// <summary>
			/// ユーザー名
			/// </summary>
			public string			UserName	                { get ; private set ; }

			/// <summary>
			/// グルーピングサービスの確認と開始を行うかどうか
			/// </summary>
			public bool             IsGroupingServiceEnabled    { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey	                { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateAccountOrLogin_RequestPacket
			(
				string	userId,
				string	userName,
				bool    isGroupingServiceEnabled,
				string	publicKey
			)
			{
				RequestType		= RequestTypes.CreateAccountOrLogin ;

				//-------------

				UserId			            = userId ;
				UserName		            = userName ;
				IsGroupingServiceEnabled    = isGroupingServiceEnabled ;
				PublicKey		            = publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( UserId ) ;
				PutString( UserName ) ;
				PutBool( IsGroupingServiceEnabled ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// アカウントの作成またはログインの応答パケット
		/// </summary>
		public class CreateAccountOrLogin_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// アクセストークン
			/// </summary>
			public string			AccessToken					{ get ; private set ; }

			/// <summary>
			/// アクセスリミット
			/// </summary>
			public long				AccessLimit					{ get ; private set ; }

			/// <summary>
			/// 共通鍵
			/// </summary>
			public byte[]			CommonKey					{ get ; private set ; }

			/// <summary>
			/// コミュニケーションサーバーのアドレス
			/// </summary>
			public string			CommunicationServer_Address	{ get ; private set ; }

			/// <summary>
			/// コミュニケーションサーバーのＴＣＰポート
			/// </summary>
			public ushort			CommunicationServer_TcpPort	{ get ; private set ; }

			/// <summary>
			/// グルーピングサーバーのアドレス
			/// </summary>
			public string           GroupingServer_Address      { get ; private set ; }

			/// <summary>
			/// グルーピングサーバーのＴＣＰポート
			/// </summary>
			public ushort           GroupingServer_TcpPort      { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public CreateAccountOrLogin_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				//-------------
				// デコード

				try
				{
					AccessToken					= GetString() ;
					AccessLimit					= GetLong() ;
					CommonKey					= GetByteArray() ;

					CommunicationServer_Address = GetString() ;
					CommunicationServer_TcpPort	= GetUShort() ;
					GroupingServer_Address      = GetString() ;
					GroupingServer_TcpPort      = GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				//-------------
				// バリデーションチェック

				if
				(
					string.IsNullOrEmpty( AccessToken ) == true ||
					CommonKey == null | CommonKey.Length == 0 ||
					string.IsNullOrEmpty( CommunicationServer_Address ) == true ||
					CommunicationServer_TcpPort == 0
				)
				{
					// 失敗
					return false ;
				}

				// 成功
				return true ;
			}
		}

		/// <summary>
		/// ログインの要求パケット
		/// </summary>
		public class Login_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId                      { get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password                    { get ; private set ; }

			/// <summary>
			/// グルーピングサービスの確認と開始を行うかどうか
			/// </summary>
			public bool             IsGroupingServiceEnabled    { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey                   { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Login_RequestPacket
			(
				string  userId,
				string  password,
				bool    isGroupingServiceEnabled,
				string  publicKey
			)
			{
				RequestType		= RequestTypes.Login ;

				//-------------

				UserId			            = userId ;
				Password		            = password ;
				IsGroupingServiceEnabled    = isGroupingServiceEnabled ;
				PublicKey		            = publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( UserId ) ;
				PutString( Password ) ;
				PutBool( IsGroupingServiceEnabled ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// ログインの応答パケット
		/// </summary>
		public class Login_ResponsePacket : ResponsePacketBase
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
			public string			CommunicationServer_Address { get ; private set ; }

			/// <summary>
			/// コミュニケーションサーバーのポート
			/// </summary>
			public ushort			CommunicationServer_TcpPort { get ; private set ; }

			/// <summary>
			/// グルーピングサーバーのアドレス
			/// </summary>
			public string           GroupingServer_Address      { get ; private set ; }

			/// <summary>
			/// グルーピングサーバーのＴＣＰポート
			/// </summary>
			public ushort           GroupingServer_TcpPort      { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Login_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					UserName					= GetString() ;
					AccessToken					= GetString() ;
					AccessLimit					= GetLong() ;
					CommonKey					= GetByteArray() ;

					CommunicationServer_Address	= GetString() ;
					CommunicationServer_TcpPort	= GetUShort() ;
					GroupingServer_Address      = GetString() ;
					GroupingServer_TcpPort      = GetUShort() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					string.IsNullOrEmpty( AccessToken ) == true ||
					AccessLimit == 0 ||
					CommonKey == null || CommonKey.Length == 0 ||
					string.IsNullOrEmpty( CommunicationServer_Address ) == true ||
					CommunicationServer_TcpPort == 0
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
		/// ログアウト要求
		/// </summary>
		public class Logout_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アクセストークン
			/// </summary>
			public string			AccessToken { get ; private set ; }

			/// <summary>
			/// 共通鍵
			/// </summary>
			public byte[]			CommonKey { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Logout_RequestPacket
			(
				string accessToken,
				byte[] commonKey,
				string publicKey
			)
			{
				RequestType		= RequestTypes.Logout ;

				//-------------

				AccessToken		= accessToken ;
				CommonKey		= commonKey ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( AccessToken ) ;
				PutByteArray( CommonKey ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// リフレッシュ要求
		/// </summary>
		public class Refresh_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// アクセストークン
			/// </summary>
			public string			AccessToken { get ; private set ; }

			/// <summary>
			/// 共通鍵
			/// </summary>
			public byte[]			CommonKey { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Refresh_RequestPacket
			(
				string accessToken,
				byte[] commonKey,
				string publicKey
			)
			{
				RequestType		= RequestTypes.Refresh ;

				//-------------

				AccessToken		= accessToken ;
				CommonKey		= commonKey ;
				PublicKey		= publicKey ;
			}

			/// <summary>
			/// エンコード
			/// </summary>
			/// <returns></returns>
			public byte[] Encode()
			{
				PutByte( ( byte )RequestType ) ;

				//------------

				PutString( AccessToken ) ;
				PutByteArray( CommonKey ) ;
				PutString( PublicKey ) ;

				//---------------------------------

				return m_Data.ToArray() ;
			}
		}

		/// <summary>
		/// リフレッシュ応答パケット
		/// </summary>
		public class Refresh_ResponsePacket : ResponsePacketBase
		{
			/// <summary>
			/// アクセストークンの有効期限
			/// </summary>
			public long				AccessLimit { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Refresh_ResponsePacket( byte[] data, int pointer ) : base( data, pointer ){}

			/// <summary>
			/// デコード
			/// </summary>
			/// <returns></returns>
			public bool Decode()
			{
				try
				{
					AccessLimit					= GetLong() ;
				}
				catch( Exception )
				{
					// 失敗
					return false ;
				}

				if
				(
					AccessLimit == 0
				)
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
		private async Task<( ResponseCodes, string, byte[] )> CallWebApi_A_Async
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

			// コンテント
			if( requestContentData != null && requestContentData.Length >  0 )
			{
				try
				{
					// サーバー側の公開鍵による暗号化を行う
					byte[] encryptedData = Security.EncryptByPublicKey( requestContentData, GetServerPublicKey() ) ;
					request.AddRange( encryptedData ) ;
				}
				catch( Exception )
				{
					// 失敗
					return ( ResponseCodes.BadRequest, "[AccountServer] リクエスト情報に誤りがあります", null ) ;
				}
			}
			else
			{
				// 失敗
				return ( ResponseCodes.BadRequest, "[AccountServer] リクエスト情報に誤りがあります", null ) ;
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

				return ( ResponseCodes.ConnectionFailed, "[AccountServer] サーバーに接続できません\n" + endPoint.ToString(), null ) ;
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
				return ( ResponseCodes.RequestFailed, "[AccountServer] サーバーと通信出来ません\n" + endPoint.ToString(), null ) ;
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
				// 中断された場合は例外を投げる

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
				return ( ResponseCodes.RequestFailed, "[AccountServer] サーバーと通信出来ません\n" + endPoint.ToString(), null ) ;
			}

			//--------------

			if( receivedData.IsEmpty == true || receivedData.Length <  2 )
			{
				// 失敗(データ異常)

				// 切断
//				await m_SocketClient.DisconnectAsync() ;
				socketClient.Disconnect( false ) ;
				socketClient.Dispose() ;

				return ( ResponseCodes.BadResponse, "[AccountServer] 受信データに問題があります(0)", null ) ;
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

				return ( ResponseCodes.BadResponse, "[AccountServer] 受信データに問題があります(1)", null ) ;
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

					return ( ResponseCodes.BadResponse, "[AccountServer] 受信データに問題があります(2)", null ) ;
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
					responseContentData = Security.DecryptBySecretKey( receivedData.Span, offset, length, m_ClientSecretKey ) ;
				}
				catch( Exception )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "[AccountServer] 受信データに問題があります(3)", null ) ;
				}

				if( responseContentData == null || responseContentData.Length == 0 )
				{
					// 失敗(データ異常)
					return ( ResponseCodes.BadResponse, "[AccountServer] 受信データに問題があります(4)", null ) ;
				}
			}

			return ( ResponseCodes.Succeeded, string.Empty, responseContentData ) ;
		}
	}
}
