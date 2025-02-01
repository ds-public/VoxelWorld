using System ;
using System.Collections.Generic ;
using System.Linq ;

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
		/// <summary>
		/// アカウントサーバーの情報を設定する
		/// </summary>
		/// <param name="SetLoginServer"></param>
		/// <param name="loginServerPort"></param>
		public void SetAccountServer( string accountServerAddress, int accountServerPort )
		{
			m_AccountServerAddress	= accountServerAddress ;
			m_AccountServerPort		= accountServerPort ;
		}

		// アカウントサーバーのアドレス
		private string						m_AccountServerAddress ;

		/// <summary>
		/// アカウントサーバーのアドレス
		/// </summary>
		public string	AccountServerAddress => m_AccountServerAddress ;

		// アカウントサーバーのポート
		private int							m_AccountServerPort ;

		/// <summary>
		/// アカウントサーバーのポート
		/// </summary>
		public int		AccountServerPort	=> m_AccountServerPort ;

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
				m_AccountServerAddress,
				m_AccountServerPort,
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
		/// アカウントサーバーに対しログインを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<Login_Response> LoginAsync
		(
			string userId,
			string password,
			CancellationToken cancellationToken = default
		)
		{
			// リクエストコンテント部
			var requestContent = new Login_RequestPacket
			(
				userId,
				password,
				m_ClientPublicKey
			) ;

			//----------------------------------------------------------

			// 共通処理部(ＷｅｂＡｐｉ)
			( var responseCode, var errorMessage, var responseContentData ) = await CallWebApi_A_Async
			(
				m_AccountServerAddress,
				m_AccountServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, null, 0, null, null, 0 ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new Login_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, 0, null, null, 0 ) ;
			}

			// ユーザー識別子とパスワードも記録しておく
			m_UserId						= userId ;
//			m_Password						= password ;

			// レスポンスの値を取り出す
			m_UserName						= responseContent.UserName ;
			m_AccessToken					= responseContent.AccessToken ;
			m_AccessLimit					= responseContent.AccessLimit ;
			m_CommonKey						= responseContent.CommonKey ;
			m_CommunicationServerAddress	= responseContent.CommunicationServerAddress ;
			m_CommunicationServerPort		= responseContent.CommunicationServerPort ;

			// 共通鍵の暗号器を生成する
			m_Crypter = Security.CreateCrypter( m_CommonKey ) ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, m_UserName, m_AccessToken, m_AccessLimit, m_CommonKey, m_CommunicationServerAddress, m_CommunicationServerPort ) ;
		}

		//-----------------------------------

		/// <summary>
		/// アカウントサーバーに対しログアウトを実行する
		/// </summary>
		/// <returns></returns>
		public async Task<WebApiResponseBase> LogoutAsync
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
				m_AccountServerAddress,
				m_AccountServerPort,
				requestContent.Encode(), cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage ) ;
			}

			//----------------------------------------------------------
			// 情報をクリアする

			m_AccessToken					= null ;
			m_AccessLimit					= 0 ;
			m_CommonKey						= null ;
			m_CommunicationServerAddress	= null ;
			m_CommunicationServerPort		= 0 ;

			m_UserId						= null ;
//			m_Password						= null ;

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
				m_AccountServerAddress,
				m_AccountServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗
				return new ( responseCode, errorMessage, null, null, 0, null, null, 0 ) ;
			}

			// レスポンスコンテント部
			var responseContent = new Refresh_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null, 0, null, null, 0 ) ;
			}

			// レスポンスの値を取り出す
			m_UserName						= responseContent.UserName ;
			m_AccessToken					= responseContent.AccessToken ;
			m_AccessLimit					= responseContent.AccessLimit ;
			m_CommonKey						= responseContent.CommonKey ;
			m_CommunicationServerAddress	= responseContent.CommunicationServerAddress ;
			m_CommunicationServerPort		= responseContent.CommunicationServerPort ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, m_UserName, m_AccessToken, m_AccessLimit, m_CommonKey, m_CommunicationServerAddress, m_CommunicationServerPort ) ;
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
				m_AccountServerAddress,
				m_AccountServerPort,
				requestContent.Encode(),
				cancellationToken
			) ;
			if( responseCode != ResponseCodes.Succeeded )
			{
				// 失敗

				return new ( responseCode, errorMessage, null, null ) ;
			}

			//----------------------------------------------------------

			// レスポンスコンテント部
			var responseContent = new CreateGuestAccount_ResponsePacket( responseContentData, 0 ) ;
			if( responseContent.Decode() == false )
			{
				// 失敗(データ異常)
				return new ( ResponseCodes.BadResponse, "受信データに問題があります", null, null ) ;
			}

			// レスポンスの値を取り出す
			string userId					= responseContent.UserId ;
			string password					= responseContent.Password ;

			//----------------------------------------------------------

			// 成功
			return new ( responseCode, string.Empty, userId, password ) ;
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

		/// <summary>
		/// ログインの要求パケット
		/// </summary>
		public class Login_RequestPacket : RequestPacketBase
		{
			/// <summary>
			/// ユーザー識別子
			/// </summary>
			public string			UserId { get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password { get ; private set ; }

			/// <summary>
			/// 応答用のクライアントの公開鍵
			/// </summary>
			public string			PublicKey { get ; private set ; }

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="data"></param>
			public Login_RequestPacket
			(
				string userId,
				string password,
				string publicKey
			)
			{
				RequestType		= RequestTypes.Login ;

				//-------------

				UserId			= userId ;
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

				PutString( UserId ) ;
				PutString( Password ) ;
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
			public string			CommunicationServerAddress { get ; private set ; }

			/// <summary>
			/// コミュニケーションサーバーのポート
			/// </summary>
			public int				CommunicationServerPort { get ; private set ; }

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
					CommunicationServerAddress	= GetString() ;
					CommunicationServerPort		= GetUShort() ;
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
					string.IsNullOrEmpty( CommunicationServerAddress ) == true ||
					CommunicationServerPort == 0
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
					UserName					= GetString() ;
					AccessToken					= GetString() ;
					AccessLimit					= GetLong() ;
					CommonKey					= GetByteArray() ;
					CommunicationServerAddress	= GetString() ;
					CommunicationServerPort		= GetUShort() ;
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
					string.IsNullOrEmpty( CommunicationServerAddress ) == true ||
					CommunicationServerPort == 0
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
			public string			UserId { get ; private set ; }

			/// <summary>
			/// パスワード
			/// </summary>
			public string			Password { get ; private set ; }

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

		// 汎用ＡＰＩコール
		private async Task<( ResponseCodes, string, byte[] )> CallWebApi_A_Async
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

			// コンテント
			if( requestContentData != null &&  requestContentData.Length >  0 )
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
				return ( ResponseCodes.RequestFailed, "ログインサーバーと通信出来ません", null ) ;
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
					responseContentData = Security.DecryptBySecretKey( receivedData, offset, length, m_ClientSecretKey ) ;
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
