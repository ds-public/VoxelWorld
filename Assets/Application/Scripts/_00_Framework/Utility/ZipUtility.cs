using System ;
using System.IO ;
using System.IO.Compression ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace DBS
{
	public class ZipUtility
	{
		/// <summary>
		/// 圧縮する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress( byte[] data )
		{
			int n ;
			
			byte[] buffer = new byte[ 1024 * 1024 ] ;

			// 入力ストリーム
			MemoryStream iStream = new MemoryStream( data ) ;

			// 出力ストリーム
			MemoryStream oStream = new MemoryStream() ;

			// 圧縮ストリーム
			GZipStream cStream = new GZipStream( oStream, CompressionMode.Compress ) ;

			while( ( n = iStream.Read( buffer, 0, buffer.Length ) ) >  0 )
			{
				cStream.Write( buffer, 0, n ) ;
			}

			cStream.Close() ;

			data = oStream.ToArray() ;

			oStream.Close() ;
			
			iStream.Close() ;

			return data ;
		}

		/// <summary>
		/// 伸長する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data )
		{
			int n ;

			byte[] buffer = new byte[ 1024 * 1024 ] ;	// 1Kbytesずつ処理する

			// 入力ストリーム
			MemoryStream iStream = new MemoryStream( data );

			// 出力ストリーム
			MemoryStream oStream = new MemoryStream() ;

			// 解凍ストリーム
			GZipStream dStream = new GZipStream( iStream, CompressionMode.Decompress ) ;

			while ( ( n = dStream.Read( buffer, 0, buffer.Length ) ) >  0 )
			{
				oStream.Write( buffer, 0, n ) ;
			}

			dStream.Close() ;
			
			data = oStream.ToArray() ;

			oStream.Close() ;

 			iStream.Close() ;

			return data ;
		}
	}
}

