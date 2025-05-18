#nullable enable
#pragma warning disable CA1822
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable IDE0028
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable IDE0300
#pragma warning disable IDE0305


using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;
using System.Security.Cryptography ;



namespace NetworkPlayHelper
{
	/// <summary>
	/// セキュリティ関連クラス Version 2025/05/15
	/// </summary>
	public class Security
	{
		//-------------------------------------------------------------------------------------------------------------
		// ■公開鍵による暗号化と復号化

		/// <summary>
		/// ＲＳＡを用いた公開鍵と秘密鍵を生成する
		/// </summary>
		/// <returns></returns>
		public static ( string, string ) CreatePublicKey()
		{
			// ＲＳＡオブジェクトを生成
			var rsa = new RSACryptoServiceProvider( 2048 ) ;

			// 公開鍵をＸＭＬ形式で取得
			string publicKey = rsa.ToXmlString( false ) ;

			// 秘密鍵をＸＭＬ形式で取得
			string secretKey = rsa.ToXmlString( true ) ;

			// ＲＳＡオブジェクトを破棄
			rsa.Dispose() ;

			return ( publicKey, secretKey ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 公開鍵を用いて暗号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] EncryptByPublicKey( ReadOnlySpan<byte> data, string publicKey )
		{
			return EncryptByPublicKey( data, 0, data.Length, publicKey ) ;
		}

		/// <summary>
		/// 公開鍵を用いて暗号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] EncryptByPublicKey( ReadOnlySpan<byte> data, int length, string publicKey )
		{
			return EncryptByPublicKey( data, 0, length, publicKey ) ;
		}

		/// <summary>
		/// 公開鍵を用いて暗号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] EncryptByPublicKey( ReadOnlySpan<byte> data, int offset, int length, string publicKey )
		{
			// ＲＳＡオブジェクトを生成
			var rsa = new RSACryptoServiceProvider( 2048 ) ;

			try
			{
				// 公開鍵を指定
				rsa.FromXmlString( publicKey ) ;
			}
			catch( CryptographicException )
			{
				rsa.Dispose() ;

				// 異常発生
				throw ;
			}

			//-------------------------

			byte[] i_buffer = new byte[ 214 ] ;
			byte[] o_buffer ;

			var encryptedData = new List<byte>() ;

			// 元のサイズを格納
			int size = length ;

			byte xor = 0xAA ;

			encryptedData.Add( ( byte )( (   size         & 0xFF ) ^ xor ) ) ;
			encryptedData.Add( ( byte )( ( ( size >>  8 ) & 0xFF ) ^ xor ) ) ;
			encryptedData.Add( ( byte )( ( ( size >> 16 ) & 0xFF ) ^ xor ) ) ;
			encryptedData.Add( ( byte )( ( ( size >> 24 ) & 0xFF ) ^ xor ) ) ;

			int window = i_buffer.Length ;
			int o, l, w, i ;

			byte[] source = data.ToArray() ;

			l = offset + length ;
			for( o  = offset ; o <  l ; o += window )
			{
				w = Math.Min( window, l - o ) ;
				Array.Copy( source, o, i_buffer, 0, w ) ;

				if( w <  window )
				{
					// ゼロパディング
					for( i  = w ; i <  window ; i ++ )
					{
						i_buffer[ i ] = 0 ;
					}
				}

				try
				{
					// 暗号化を実行
					o_buffer = rsa.Encrypt( i_buffer, RSAEncryptionPadding.OaepSHA1 ) ;
				}
				catch( CryptographicException )
				{
					// 異常発生
					rsa.Dispose() ;

					throw ;
				}

				encryptedData.AddRange( o_buffer ) ;
			}

			//-------------------------

			// ＲＳＡオブジェクトを破棄
			rsa.Dispose() ;

			return encryptedData.ToArray() ;
		}

		/// <summary>
		/// 秘密鍵を用いて復号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] DecryptBySecretKey( ReadOnlySpan<byte> data, string secretKey )
		{
			return DecryptBySecretKey( data, 0, data.Length, secretKey ) ;
		}

		/// <summary>
		/// 秘密鍵を用いて復号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] DecryptBySecretKey( ReadOnlySpan<byte> data, int length, string secretKey )
		{
			return DecryptBySecretKey( data, 0, length, secretKey ) ;
		}

		/// <summary>
		/// 秘密鍵を用いて復号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static byte[] DecryptBySecretKey( ReadOnlySpan<byte> data, int offset, int lenth, string secretKey )
		{
			// ＲＳＡオブジェクトを生成
			var rsa = new RSACryptoServiceProvider( 2048 ) ;

			try
			{
				// 秘密鍵を指定
				rsa.FromXmlString( secretKey ) ;
			}
			catch( CryptographicException )
			{
				rsa.Dispose() ;

				// 異常発生
				throw ;
			}

			//-------------------------

			byte[] i_buffer = new byte[ 256 ] ;
			byte[] o_buffer ;

			var decryptedData = new List<byte>() ;

			int window = i_buffer.Length ;
			int o, l, w, i ;

			byte[] source = data.ToArray() ;

			l = offset + lenth ;
			for( o  = offset + 4 ; o <  l ; o += window )
			{
				w = Math.Min( window, l - o ) ;
				Array.Copy( source, o, i_buffer, 0, w ) ;

				if( w <  window )
				{
					// ゼロパディング
					for( i  = w ; i <  window ; i ++ )
					{
						i_buffer[ i ] = 0 ;
					}
				}

				try
				{
					// 複合化を実行
					o_buffer = rsa.Decrypt( i_buffer, RSAEncryptionPadding.OaepSHA1 ) ;
				}
				catch( CryptographicException )
				{
					// 異常発生
					rsa.Dispose() ;

					throw ;
				}

				decryptedData.AddRange( o_buffer ) ;
			}

			// ＲＳＡオブジェクトを破棄
			rsa.Dispose() ;

			byte xor = 0xAA ;

			// 元のサイズを取得
			int size =
				(   data[ offset + 0 ] ^ xor         ) |
				( ( data[ offset + 1 ] ^ xor ) <<  8 ) |
				( ( data[ offset + 2 ] ^ xor ) << 16 ) |
				( ( data[ offset + 3 ] ^ xor ) << 24 ) ;

			if( decryptedData.Count <  size )
			{
				// サイズ異常
				throw new Exception( "Decrypt failed." ) ;
			}

			return decryptedData.GetRange( 0, size ).ToArray() ;
		}

		//-------------------------------------------------------------------------------------------------------------
		// ■共通鍵による暗号化と復号化(後で高速版に改修する)　※且つスレッドセーフも考慮

		/// <summary>
		/// AES128 で暗号化する(バイナリ→バイナリ)　パスワード版
		/// </summary>
		/// <param name="data"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] EncryptByCommonKey( byte[] data, string password )
		{
			if( string.IsNullOrEmpty( password ) == true )
			{
				throw new Exception( "Password is empty." ) ;
			}

			//----------------------------------

			 byte[] hash = GetHashFromText( password ) ;

			return EncryptByCommonKey( data, hash ) ;
		}

		/// <summary>
		/// AES128 で暗号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static byte[] EncryptByCommonKey( byte[] data, byte[] hash )
		{
			return EncryptByCommonKey( data, 0, data.Length, hash ) ;
		}

		/// <summary>
		/// AES128 で暗号化する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static byte[] EncryptByCommonKey( byte[] data, int offset, int length, byte[] hash )
		{
			// キーとベクターに問題があれば補正する
			( var key, var vector ) = GetKeyAndVector( hash ) ;

			//----------------------------------

			var aes = Aes.Create() ;
			{
				aes.BlockSize = 16 * 8 ;			// 128 bit
				aes.KeySize = key.Length * 8 ;
				aes.Mode = CipherMode.CBC ;
				aes.Padding = PaddingMode.PKCS7 ;
				aes.Key = key ;
				aes.IV = vector ;	// GenerateIV() を使ってはダメ。毎回値が変わる。
			} ;

			//----------------------------------

			ICryptoTransform encryptor = aes.CreateEncryptor( aes.Key, aes.IV ) ;

				// 高速化：ここ以外を呼び出し元に持っていく
			byte[] encodedData = encryptor.TransformFinalBlock( data, offset, length ) ;

			encryptor.Dispose() ;
			aes.Dispose() ;

			return encodedData ;
		}

		/// <summary>
		/// AES128 で復号化する(バイナリ→バイナリ)　パスワード版
		/// </summary>
		/// <param name="data"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] DecryptByCommonKey( byte[] data, string password )
		{
			if( string.IsNullOrEmpty( password ) == true )
			{
				throw new Exception( "Password is empty." ) ;
			}

			//----------------------------------

			byte[] hash = GetHashFromText( password ) ;

			return DecryptByCommonKey( data, hash ) ;
		}

		/// <summary>
		/// AES128 で復号化する(バイナリ→バイナリ)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static byte[] DecryptByCommonKey( byte[] data, byte[] hash )
		{
			return DecryptByCommonKey( data, 0, data.Length, hash ) ;
		}

		/// <summary>
		/// AES128 で復号化する(バイナリ→バイナリ)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static byte[] DecryptByCommonKey( byte[] data, int offset, int length, byte[] hash )
		{
			// キーとベクターに問題があれば補正する
			( var key, var vector ) = GetKeyAndVector( hash ) ;

			//----------------------------------

			var aes = Aes.Create() ;
			aes.BlockSize = 16 * 8 ;			// 128 bit
			aes.KeySize = key.Length * 8 ;
			aes.Mode = CipherMode.CBC ;
			aes.Padding = PaddingMode.PKCS7 ;
			aes.Key = key ;
			aes.IV  = vector ;	// GenerateIV() を使ってはダメ。毎回値が変わる。

			//----------------------------------

			ICryptoTransform decryptor = aes.CreateDecryptor( aes.Key, aes.IV ) ;

			byte[] decodedData ;

			try
			{
				// 高速化：ここ以外を呼び出し元に持っていく
				decodedData = decryptor.TransformFinalBlock( data, offset, length ) ;
			}
			catch( CryptographicException )
			{
				decodedData = null ;
			}

			decryptor.Dispose() ;
			aes.Dispose() ;

			return decodedData ;
		}

		//-----------------------------------------------------------
		// オーバーヘッドを減らすため暗号化オブジェクトを事前生成する方法

		/// <summary>
		/// 暗号器
		/// </summary>
		public class Crypter
		{
			private ulong[] m_Seeds ;

			private Aes m_AES ;

			//----------------------------------

			/// <summary>
			/// 暗号器を生成する
			/// </summary>
			/// <param name="key"></param>
			/// <param name="vector"></param>
			public void Create( byte[] hash, byte[] key, byte[] vector )
			{
				m_Seeds = new ulong[ 4 ] ;

				m_Seeds[ 0 ] =
					( ulong )hash[  0 ]       |
					( ulong )hash[  1 ] <<  8 |
					( ulong )hash[  2 ] << 16 |
					( ulong )hash[  3 ] << 24 |
					( ulong )hash[  4 ] << 32 |
					( ulong )hash[  5 ] << 40 |
					( ulong )hash[  6 ] << 48 |
					( ulong )hash[  7 ] << 56 ;

				m_Seeds[ 1 ] =
					( ulong )hash[  8 ]       |
					( ulong )hash[  9 ] <<  8 |
					( ulong )hash[ 10 ] << 16 |
					( ulong )hash[ 11 ] << 24 |
					( ulong )hash[ 12 ] << 32 |
					( ulong )hash[ 13 ] << 40 |
					( ulong )hash[ 14 ] << 48 |
					( ulong )hash[ 15 ] << 56 ;

				m_Seeds[ 2 ] =
					( ulong )hash[ 16 ]       |
					( ulong )hash[ 17 ] <<  8 |
					( ulong )hash[ 18 ] << 16 |
					( ulong )hash[ 19 ] << 24 |
					( ulong )hash[ 20 ] << 32 |
					( ulong )hash[ 21 ] << 40 |
					( ulong )hash[ 22 ] << 48 |
					( ulong )hash[ 23 ] << 56 ;

				m_Seeds[ 3 ] =
					( ulong )hash[ 24 ]       |
					( ulong )hash[ 25 ] <<  8 |
					( ulong )hash[ 26 ] << 16 |
					( ulong )hash[ 27 ] << 24 |
					( ulong )hash[ 28 ] << 32 |
					( ulong )hash[ 29 ] << 40 |
					( ulong )hash[ 30 ] << 48 |
					( ulong )hash[ 31 ] << 56 ;

				//-------------

				m_AES = Aes.Create() ;
				{
					m_AES.BlockSize = 16 * 8 ;			// 128 bit
					m_AES.KeySize = key.Length * 8 ;
					m_AES.Mode = CipherMode.CBC ;
					m_AES.Padding = PaddingMode.PKCS7 ;
					m_AES.Key = key ;
					m_AES.IV = vector ;	// GenerateIV() を使ってはダメ。毎回値が変わる。
				}
			}

			/// <summary>
			/// 破棄する
			/// </summary>
			public void Dispose()
			{
				if( m_AES != null )
				{
					m_AES.Dispose() ;
					m_AES = null ;
				}
			}

			//----------------------------------------------------------

			/// <summary>
			/// 暗号化を行う
			/// </summary>
			/// <param name="data"></param>
			/// <returns></returns>
			public byte[] EncryptXor( byte[] data, int offset = 0, int length = 0 )
			{
				if( data == null )
				{
					return null ;
				}

				if( offset >= data.Length )
				{
					return null ;
				}

				if( length <= 0 )
				{
					length  = data.Length ;
				}

				if( ( offset + length ) >  data.Length )
				{
					length  = ( data.Length - offset ) ;
				}

				//---------------------------------------------------------

				ulong x = m_Seeds[ 0 ] ;
				ulong y = m_Seeds[ 1 ] ;
				ulong z = m_Seeds[ 2 ] ;
				ulong w = m_Seeds[ 3 ] ;
				ulong t ;

				int index ;
				byte code ;

				byte[] encryptedData = new byte[ length + 4 ] ;

				// データ部分の暗号化
				for( index  = 0 ; index <  length ; index ++ )
				{
					code = data[ offset + index ] ;

					//------------

					t = ( x ^ ( x << 11 ) ) ;
					x = y ; y = z ; z = w ;
					w = w ^ ( w >> 19 ) ^ ( t ^ ( t >> 8 ) ) ;

					encryptedData[ index ] = ( byte )( code ^ ( byte )( w & 0xFF ) ) ;
				}

				// 末尾にＣＲＣを付与する
				uint crc32 = GetCRC32( data, offset, length ) ;

				encryptedData[ length + 0 ] = ( byte )(   crc32         & 0xFF ) ;
				encryptedData[ length + 1 ] = ( byte )( ( crc32 >>  8 ) & 0xFF ) ;
				encryptedData[ length + 2 ] = ( byte )( ( crc32 >> 16 ) & 0xFF ) ;
				encryptedData[ length + 3 ] = ( byte )( ( crc32 >> 24 ) & 0xFF ) ;

				// ＣＲＣ部分の暗号化
				for( index  = length ; index <  ( length + 4 ) ; index ++ )
				{
					code = encryptedData[ index ] ;

					//------------

					t = ( x ^ ( x << 11 ) ) ;
					x = y ; y = z ; z = w ;
					w = w ^ ( w >> 19 ) ^ ( t ^ ( t >> 8 ) ) ;

					encryptedData[ index ] = ( byte )( code ^ ( byte )( w & 0xFF ) ) ;
				}

				return encryptedData ;
			}

			/// <summary>
			/// 復号化を行う
			/// </summary>
			/// <param name="data"></param>
			/// <param name="offset"></param>
			/// <param name="length"></param>
			/// <returns></returns>
			public byte[] DecryptXor( ReadOnlySpan<byte> data, int offset = 0, int length = 0 )
			{
				if( data.IsEmpty == true )
				{
					return null ;
				}

				if( offset >= data.Length )
				{
					return null ;
				}

				if( length <= 0 )
				{
					length  = data.Length ;
				}

				if( ( offset + length ) >  data.Length )
				{
					length  = ( data.Length - offset ) ;
				}

				if( length <  5 )
				{
					// ＣＲＣ部分を含めてサイズが異常
					return null ;
				}

				length -= 4 ;

				//---------------------------------------------------------

				ulong x = m_Seeds[ 0 ] ;
				ulong y = m_Seeds[ 1 ] ;
				ulong z = m_Seeds[ 2 ] ;
				ulong w = m_Seeds[ 3 ] ;
				ulong t ;

				int index ;
				byte code ;

				byte[] decryptedData = new byte[ length ] ;

				// データ部分
				for( index  = 0 ; index <  length ; index ++ )
				{
					code = data[ offset + index ] ;

					//------------

					t = ( x ^ ( x << 11 ) ) ;
					x = y ; y = z ; z = w ;
					w = w ^ ( w >> 19 ) ^ ( t ^ ( t >> 8 ) ) ;

					decryptedData[ index ] = ( byte )( code ^ ( byte )( w & 0xFF ) ) ;
				}

				// ＣＲＣ部分
				uint crc32 = 0 ;
				int shift  = 0 ;

				for( index  = length ; index <  ( length + 4 ) ; index ++ )
				{
					code = data[ offset + index ] ;

					//------------

					t = ( x ^ ( x << 11 ) ) ;
					x = y ; y = z ; z = w ;
					w = w ^ ( w >> 19 ) ^ ( t ^ ( t >> 8 ) ) ;

					crc32 += ( ( uint )( code ^ ( byte )( w & 0xFF ) ) << shift ) ;
					shift += 8 ;
				}

				//---------------------------------

				// ＣＲＣの確認
				if( crc32 != GetCRC32( decryptedData, 0, length ) )
				{
					// ＣＲＣが合わない
					return null ;
				}

				return decryptedData ;
			}

			/// <summary>
			/// 暗号化を行う
			/// </summary>
			/// <param name="data"></param>
			/// <returns></returns>
			public byte[] EncryptAes( byte[] data, int offset = 0, int length = 0 )
			{
				if( data == null )
				{
					return null ;
				}

				if( offset >= data.Length )
				{
					return null ;
				}

				if( length <= 0 )
				{
					length  = data.Length ;
				}

				if( ( offset + length ) >  data.Length )
				{
					length  = ( data.Length - offset ) ;
				}

				var encryptor = m_AES.CreateEncryptor( m_AES.Key, m_AES.IV ) ;

				byte[] encryptedData = encryptor.TransformFinalBlock( data, offset, length ) ;

				encryptor.Dispose() ;

				return encryptedData ;
			}

			/// <summary>
			/// 復号化を行う
			/// </summary>
			/// <param name="data"></param>
			/// <param name="offset"></param>
			/// <param name="length"></param>
			/// <returns></returns>
			public byte[] DecryptAes( ReadOnlySpan<byte> data, int offset = 0, int length = 0 )
			{
				if( data == null )
				{
					return null ;
				}

				if( offset >= data.Length )
				{
					return null ;
				}

				if( length <= 0 )
				{
					length  = data.Length ;
				}

				if( ( offset + length ) >  data.Length )
				{
					length  = ( data.Length - offset ) ;
				}

				var decryptor = m_AES.CreateDecryptor( m_AES.Key, m_AES.IV ) ;

				byte[] decryptedData ;

				try
				{
					decryptedData = decryptor.TransformFinalBlock( data.ToArray(), offset, length ) ;
				}
				catch( CryptographicException )
				{
					decryptedData = null ;
				}

				decryptor.Dispose() ;

				return decryptedData ;
			}
		}

		/// <summary>
		/// 暗号器を生成する
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		public static Crypter CreateCrypter( byte[] hash )
		{
			// キーとベクターに問題があれば補正する
			( byte[] key, byte[] vector ) = GetKeyAndVector( hash ) ;

			//----------------------------------

			var crypter = new Crypter() ;
			crypter.Create( hash, key, vector ) ;

			//----------------------------------
			return crypter ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 通信用暗号化キー(省略時に使用)
		/// </summary>
		public static readonly byte[] AESKey = { 0x51, 0x66, 0x54, 0x6A, 0x57, 0x6E, 0x5A, 0x72, 0x34, 0x75, 0x37, 0x77, 0x21, 0x7A, 0x25, 0x43 };

		/// <summary>
		/// 通信用暗号化ベクター(省略時に使用)
		/// </summary>
		public static readonly byte[] AESVector = { 0x79, 0x2F, 0x42, 0x3F, 0x45, 0x28, 0x48, 0x2B, 0x4D, 0x62, 0x51, 0x65, 0x54, 0x68, 0x57, 0x6D };

		// テキストからキーとベクターを取得する
		private static byte[] GetHashFromText( string text )
		{
			byte[] hash = Encoding.UTF8.GetBytes( text ) ;

			if( hash.Length >= 32 )
			{
				// ３２文字以上なら最初の３２文字を使用する
				hash = Slice( hash,  0, 32 ) ;
			}
			else
			{
				// ３２文字未満ならハッシュ化して最初の３２文字を使用する
				hash = Encoding.UTF8.GetBytes( GetHash( hash, 0, hash.Length, HashTypes.SHA256 ) ) ;
				hash = Slice( hash,  0, 32 ) ;
			}

			return hash ;
		}

		// キーとベクターに問題があれば補正する
		private static ( byte[] key, byte[] vector ) GetKeyAndVector( byte[] hash )
		{
			if( hash == null || hash.Length == 0 )
			{
				return ( AESKey, AESVector ) ;
			}

			//----------------------------------

			byte[] key, vector ;

			int length = 16, size ;

			key = new byte[ length ] ;
			size = Math.Min( length, hash.Length ) ;
			Array.Copy( hash, 0, key, 0, size ) ;

			vector = new byte[ length ] ;
			if( hash.Length >  length )
			{
				size = hash.Length - length ;
				size = Math.Min( length, size ) ;
				Array.Copy( hash, length, vector, 0, size ) ;
			}
			else
			{
				size = Math.Min( length, hash.Length ) ;
				Array.Copy( hash, 0, vector, 0, size ) ;
			}

			return ( key, vector ) ;
		}

		//-------------------------------------------------------------------------------------------------------------------
		// その他

		/// <summary>
		/// ＡＳＣＩＩコード部(0x28～0x5B・0x5D～0x7E)を shift ずつずらした文字列を取得する
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string ShiftAscii( string text, int shift = 8 )
		{
			if( string. IsNullOrEmpty( text ) == true )
			{
				return text ;
			}

			//----------------------------------------------------------

			if( shift <  -16 )
			{
				shift  =  -16 ;
			}
			else
			if( shift >   16 )
			{
				shift  =  16 ;
			}

			char[] codes = text.ToCharArray() ;
			int    code ;

			int i, l = codes.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				code = codes[ i ] ;

				if( code >= 0x28 && code <= 0x5B )
				{
					code += shift ;

					if( code <  0x28 )
					{
						code  = 0x5B + ( code - 0x28 ) ;
					}
					else
					if( code >  0x5B )
					{
						code  = 0x28 + ( code - 0x5B ) ;
					}
				}
				else
				if( code >= 0x5D && code <= 0x7E )
				{
					code += shift ;

					if( code <  0x5D )
					{
						code  = 0x7E + ( code - 0x5D ) ;
					}
					else
					if( code >  0x7E )
					{
						code  = 0x5D + ( code - 0x7E ) ;
					}
				}

				codes[ i ] = ( char )code ;
			}

			return new string( codes ) ;
		}

		//-------------------------------------------------------------------------------------------------------------------
		// ■ハッシュ関連

		/// <summary>
		/// ハッシュのタイプ
		/// </summary>
		public enum HashTypes
		{
			MD5,
			SHA1,
			SHA256,
			SHA384,
			SHA512,
		}

		//---------------

		/// <summary>
		/// ハッシュコードを計算する(テキスト)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="hashType"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] GetHashCode( string text, HashTypes hashType = HashTypes.SHA256 )
		{
			if( text == null )
			{
				throw new Exception( "Text is null." ) ;
			}

			var data = Encoding.UTF8.GetBytes( text ) ;
			return GetHashCode( data, 0, data.Length, hashType ) ;
		}

		/// <summary>
		/// ハッシュコードを計算する(バイナリ)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="hashType"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] GetHashCode( byte[] data, int offset = 0, int length = 0, HashTypes hashType = HashTypes.SHA256 )
		{
			data = Slice( data, offset, length ) ;
			if( data == null )
			{
				throw new Exception( "Data is null." ) ;
			}

			//--------------

			byte[] hash ;
			switch( hashType )
			{
				case HashTypes.MD5 :
					{
						var hashGenerator = MD5.Create() ;
						hash = hashGenerator.ComputeHash( data ) ;
						hashGenerator.Dispose() ;
					}
				break ;
				case HashTypes.SHA1 :
					{
						var hashGenerator = SHA1.Create() ;
						hash = hashGenerator.ComputeHash( data ) ;
						hashGenerator.Dispose() ;
					}
				break ;
				case HashTypes.SHA256 :
					{
						var hashGenerator = SHA256.Create() ;
						hash = hashGenerator.ComputeHash( data ) ;
						hashGenerator.Dispose() ;
					}
				break ;
				case HashTypes.SHA384 :
					{
						var hashGenerator = SHA384.Create() ;
						hash = hashGenerator.ComputeHash( data ) ;
						hashGenerator.Dispose() ;
					}
				break ;
				case HashTypes.SHA512 :
					{
						var hashGenerator = SHA512.Create() ;
						hash = hashGenerator.ComputeHash( data ) ;
						hashGenerator.Dispose() ;
					}
				break ;
				default :
					throw new Exception( "Unknown HashType" ) ;
			}

			//--------------

			return hash ;
		}

		/// <summary>
		/// ハッシュコードを計算する(テキスト)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="hashType"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string GetHash( string text, HashTypes hashType = HashTypes.SHA256 )
		{
			if( text == null )
			{
				throw new Exception( "Text is null." ) ;
			}

			var data = Encoding.UTF8.GetBytes( text ) ;
			return GetHash( data, 0, data.Length, hashType ) ;
		}

		/// <summary>
		/// ハッシュコードを計算する(バイナリ)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="hashType"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string GetHash( byte[] data, int offset = 0, int length = 0, HashTypes hashType = HashTypes.SHA256 )
		{
			var hash = GetHashCode( data, offset, length, hashType ) ;

			//--------------

			var sb = new StringBuilder() ;
			foreach( var code in hash )
			{
				sb.Append( code.ToString( "x2" ) ) ;
			}

			return sb.ToString() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＲＣを取得する(継続)
		/// </summary>
		/// <param name="crc"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static uint GetCRC32( byte[] data, int offset, int length )
		{
			int index ;
			int limit = offset + length ;

			uint crc = CRC32_MASK ;

			for( index  = offset ; index <  limit ; index ++ )
			{
				crc = m_CRC32_Table[ ( crc ^ data[ index ] ) & 0xFF ] ^ ( crc >> 8 ) ;
			}

			return crc ;
		}

		//-----------------------------------

		public const uint CRC32_MASK = 0xffffffff ;

		// ＣＲＣテーブル
		private readonly static uint[] m_CRC32_Table = new uint[]
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
			0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
			0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
			0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
			0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
			0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
			0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
			0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
			0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
			0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
			0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
			0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
			0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
			0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
			0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
			0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
			0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
			0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
			0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
			0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
			0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
			0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
			0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
			0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
			0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
			0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
			0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
			0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
			0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
			0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
			0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
			0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
			0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
			0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
			0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
			0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
			0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
			0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
			0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
			0x2d02ef8d
		} ;

		//-------------------------------------------------------------------------------------------

		// バイト配列の椎した範囲を取り出して新しいバイト配列として取得する
		private static byte[] Slice( byte[] data, int offset, int length )
		{
			if( data == null || data.Length == 0 )
			{
				return data ;
			}

			if( offset <  0 )
			{
				offset  = 0 ;
			}

			if( offset >= data.Length )
			{
				offset  = data.Length - 1 ;
			}

			if( length <= 0 )
			{
				length  = data.Length ;
			}

			if( ( offset + length ) >  data.Length )
			{
				length = data.Length - offset ;
			}

			if( offset != 0 || length != data.Length )
			{
				data = data.AsSpan( offset, length ).ToArray() ;
			}

			return data ;
		}

	}
}
