#nullable enable

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8618
#pragma warning disable CS8625

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using System.Net ;
using System.Net.Sockets ;
using System.Text ;

using UnityEngine ;

namespace DSW
{
	/// <summary>
	/// クライアントからの応答に答える Version 2024/03/31
	/// </summary>
	public class ClientResponder
	{
		// サーバーソケット(UDP)
		private UdpClient			m_UdpServer ;

		// クライアントに送るパケットデータ(固定内容)
		private readonly byte[]		m_PacketData ;		

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="serverPort"></param>
		/// <param name="serverName"></param>
		public ClientResponder( int serverDetectorPort, int serverPort, string serverName = null )
		{
			if( string.IsNullOrEmpty( serverName ) == true )
			{
				// 最も文字列が少ないアドレスを選択する(IPv4)
				int length = 0x7FFFFF ;

				// クライアント名が省略された場合はクライアントのＩＰアドレスを名前とする
				var addressNames = GetLocalAddress() ;
				if( addressNames != null && addressNames.Length >  0 )
				{
					foreach( var addressName in addressNames )
					{
						if( addressName.Length <  length )
						{
							serverName = addressName ;
							length = addressName.Length ;
						}
					}
				}

				if( string.IsNullOrEmpty( serverName ) == true )
				{
					// アドレスが全く取得出来なかった場合の名前
					serverName = "Unknown" ;
				}
			}

			//----------------------------------

			// クライアントに返信するパケット情報

			Debug.Log( "ServerPort : " + serverPort ) ;
			Debug.Log( "ServerName ; " + serverName ) ;

			// 送信する情報を生成する
			var data = new List<byte>() ;
			PutInt32( serverPort, in data ) ;
			PutString( serverName, in data ) ;

			m_PacketData = data.ToArray() ;

			//----------------------------------------------------------

			Debug.Log( "ServerDetectorPort : " + serverDetectorPort ) ;

			// ＵＤＰサーバーを生成する
			m_UdpServer = new UdpClient( serverDetectorPort ) ;
		}

		//-----------------------------------------------------------

		// ローカルのＩＰアドレスを取得する
		private static string[] GetLocalAddress()
		{
			string hostName = Dns.GetHostName() ;

			IPAddress[] addresses = Dns.GetHostAddresses( hostName ) ;

			if( addresses == null || addresses.Length == 0 )
			{
				return null ;
			}

			var addressNames = new List<string>() ;

			foreach( var address in addresses )
			{
				addressNames.Add( address.ToString() ) ;
			}

			return addressNames.ToArray() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 待ち受けを開始する
		/// </summary>
		public void Run()
		{
			if( m_UdpServer == null )
			{
				return ;
			}

			//----------------------------------

			Debug.Log( "[ClientResponder] Run" ) ;

			// 待ち受けを開始する
			m_UdpServer.BeginReceive( OnReceived, m_UdpServer ) ;
		}

		// パケットを受信したら呼び出される
		private void OnReceived( System.IAsyncResult result )
		{
			UdpClient udpServer = ( UdpClient )result.AsyncState ;
			IPEndPoint endPoint = null ;

			// データを取得する
			byte[] data = udpServer.EndReceive( result, ref endPoint ) ;

			Debug.Log( "[ClientResponder] サーバー探知パケット受信 : " + endPoint.Address.ToString() ) ;

			if( data != null && data.Length >  0 )
			{
				int		offset		= 0 ;
				string	clientName	= GetString( in data, ref offset ) ;
				int		clientPort	= endPoint.Port ;

				// クライアントからの要求を受信した

				Debug.Log( "[ClientResponder] 探知実行クライアントの名前 : " + clientName ) ;
				Debug.Log( "[ClientResponder] 探知実行クライアントの待ち受けポート番号 ] " + clientPort ) ;

				Debug.Log( "[ClientResponder] 探知実行クライアントに応答する : " + endPoint.Address.ToString() ) ;

				endPoint.Port = clientPort ;

				// サーバーの情報を返信する
				udpServer.Send( m_PacketData, m_PacketData.Length, endPoint ) ;
			}

			//----------------------------------------------------------

			// 待ち受けを開始する
			m_UdpServer.BeginReceive( OnReceived, m_UdpServer ) ;
		}

		/// <summary>
		/// 終了する
		/// </summary>
		public void Dispose()
		{
			Debug.Log( "[ClientResponder] Dispose" ) ;

			if( m_UdpServer != null )
			{
				m_UdpServer.Close() ;
				m_UdpServer.Dispose() ;

				m_UdpServer  = null ;
			}
		}

		//-------------------------------------------------------------------------------------------
#if false
		// ０８ビット整数値を格納する
		private static void PutByte( byte value, in List<byte> data )
		{
			data.Add( value ) ;
		}

		// ０８ビット整数値を取得する
		private static byte GetByte( int byte[] data, ref int offset )
		{
			byte value = data[ offset ] ;

			offset ++ ;

			return value ;
		}
#endif
		// １６ビット整数値を格納する
		private static void PutInt16( short value, in List<byte> data )
		{
			data.Add( ( byte )value ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
		}

		// １６ビット整数値を取得する
		private static int GetInt16( in byte[] data, ref int offset )
		{
			Int16 value = ( Int16 )
			(
				data[ offset ] |
				( ( Int16 )data[ offset + 1 ] <<  8 )
			) ;

			offset += 2 ;

			return value ;
		}

		// ３２ビット整数値を取得する
		private static void PutInt32( int value, in List<byte> data )
		{
			data.Add( ( byte )value ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
		}

		// ３２ビット整数値を取得する
		private static int GetInt32( in byte[] data, ref int offset )
		{
			Int32 value = ( Int32 )
			(
				data[ offset ] |
				( ( Int32 )data[ offset + 1 ] <<  8 ) |
				( ( Int32 )data[ offset + 2 ] << 16 ) |
				( ( Int32 )data[ offset + 3 ] << 24 )
			) ;

			offset += 4 ;

			return value ;
		}

		// 文字列を格納する
		private static void PutString( string value, in List<byte> data )
		{
			int length = 0 ;
			byte[] codes = null ;
			if( string.IsNullOrEmpty( value ) == false )
			{
				codes = Encoding.UTF8.GetBytes( value ) ;
				length = codes.Length ;
			}

			PutInt16( ( Int16 )length, in data ) ;

			if( length >  0 && codes != null )
			{
				data.AddRange( codes ) ;
			}
		}

		// 文字列を取得する
		private static string GetString( in byte[] data, ref int offset )
		{
			int length = GetInt16( data, ref offset ) ;
			if( length <= 0 )
			{
				return string.Empty ;
			}

			string value = Encoding.UTF8.GetString( data, offset, length ) ;

			offset += length ;

			return value ;
		}
	}
}
