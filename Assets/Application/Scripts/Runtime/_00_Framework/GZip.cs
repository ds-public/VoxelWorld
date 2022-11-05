using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using ICSharpCode.SharpZipLib.GZip ;

namespace DSW
{
	/// <summary>
	/// ＧＺｉｐクラス Version 2022/10/11 0
	/// </summary>
	public class GZip : ExMonoBehaviour
	{
		private static GZip	m_Instance ;
		internal void Awake()
		{
			m_Instance = this ;
		}
		internal void OnDestroy()
		{
			m_Instance = null ;
		}

		//-----------------------------------

		/// <summary>
		/// 伸長・圧縮のバッファサイズ
		/// </summary>
		public static int BufferSize = 4096 ;


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列を伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data, int length )
		{
			if( data == null || length <= 0 ||  length >  data.Length )
			{
				// 不可
				return null ;
			}

			MemoryStream mis = new MemoryStream( data, 0, length ) ;
	
			return Decompress_Private( mis ) ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列を伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data, int offset, int length )
		{
			if( data == null || offset <   0 || length <= 0 || offset >= data.Length || ( offset + length ) >  data.Length )
			{
				// 不可
				return null ;
			}

			MemoryStream mis = new MemoryStream( data, offset, length ) ;
	
			return Decompress_Private( mis ) ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列を伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( byte[] data )
		{
			if( data == null || data.Length == 0 )
			{
				// 不可
				return null ;
			}

			MemoryStream mis = new MemoryStream( data ) ;
	
			return Decompress_Private( mis ) ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮された環境パスのファイルを伸張する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] Decompress( string path )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return Decompress_Private( fis ) ;
			}
			return null ;
		}

		private static byte[] Decompress_Private( Stream cis )
		{
			byte[] result ;

			GZipInputStream gis = new GZipInputStream( cis ) ;

			MemoryStream mos ;

			//----------------------------------

			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			mos = new MemoryStream() ;

			do
			{
				length = gis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					mos.Write( buffer, 0, length ) ;
				}
			}
			while( length >  0 ) ;

			result = mos.ToArray() ;

			mos.Close() ;

			gis.Close() ;

			cis.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮されたバイト配列から任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( byte[] data, Action<byte[]> onAction = null )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				
				return await DecompressAsync_Private( mis, onAction ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮された環境パスのファィルから任意のファイルのバイト配列を取得する(非同期)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> DecompressAsync( string path, Action<byte[]> onAction = null )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await DecompressAsync_Private( fis,onAction ) ;
			}
			return null ;
		}

		private static async UniTask<byte[]> DecompressAsync_Private( Stream cis, Action<byte[]> onAction )
		{
			byte[] result ;

			GZipInputStream gis = new GZipInputStream( cis ) ;

			MemoryStream mos ;

			//----------------------------------

			byte[] buffer = new byte[ BufferSize ] ;
			int length ;

			mos = new MemoryStream() ;

			do
			{
				length = gis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					mos.Write( buffer, 0, length ) ;
					await m_Instance.Yield() ;
				}
			}
			while( length >  0 ) ;

			result = mos.ToArray() ;

			mos.Close() ;

			gis.Close() ;

			cis.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;

				return result ;
			}
			else
			{
				Debug.LogError( "Decompress Failed" ) ;
				return null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列をバイト配列から生成する(同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Compress( byte[] data, int level = 3 )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
	
				return Compress_Private( mis, level ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列を環境パスのファイルから生成する(同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static byte[] Compress( string path, int level = 3 )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return Compress_Private( fis, level ) ;
			}
			return null ;
		}

		private static byte[] Compress_Private( Stream cis, int level )
		{
			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			GZipOutputStream gos = new GZipOutputStream( mos ) ;

			gos.SetLevel( level ) ;

			//----------------------------------------------------------

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			do
			{
				length = cis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					gos.Write( buffer, 0, length ) ;
				}
			}
			while( length >  0 ) ;

			//----------------------------------------------------------

			gos.Finish() ;
			gos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			return result ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列をバイト配列から生成する(非同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> CompressAsync( byte[] data, Action<byte[]> onAction = null, int level = 3 )
		{
			if( data != null && data.Length >  0 )
			{
				MemoryStream mis = new MemoryStream( data ) ;
				
				return await CompressAsync_Private( mis, onAction, level ) ;
			}
			return null ;
		}

		/// <summary>
		/// ＧＺｉｐ形式で圧縮したバイナリ配列を環境パスのファイルから生成する(非同期)
		/// </summary>
		/// <param name="password"></param>
		/// <param name="sources"></param>
		/// <returns></returns>
		public static async UniTask<byte[]> CompressAsync( string path, Action<byte[]> onAction = null, int level = 3 )
		{
			FileStream fis = File.OpenRead( path ) ;
			if( fis != null )
			{
				return await CompressAsync_Private( fis, onAction, level ) ;
			}
			return null ;
		}

		private static async UniTask<byte[]> CompressAsync_Private( Stream cis, Action<byte[]> onAction, int level )
		{
			byte[] result ;

			MemoryStream mos = new MemoryStream() ;
			GZipOutputStream gos = new GZipOutputStream( mos ) ;

			gos.SetLevel( level ) ;

			//----------------------------------------------------------

			byte[] buffer = new byte[ BufferSize ] ;	// ファイル用のバッファ
			int length ;

			do
			{
				length = cis.Read( buffer, 0, buffer.Length ) ;
				if( length >  0 )
				{
					gos.Write( buffer, 0, length ) ;
					await m_Instance.Yield() ;
				}
			}
			while( length >  0 ) ;

			//----------------------------------------------------------

			gos.Finish() ;
			gos.Close() ;

			result = mos.ToArray() ;

			mos.Close() ;

			if( result != null )
			{
				onAction?.Invoke( result ) ;
				return result ;
			}
			else
			{
				Debug.LogError( "Compress Failed" ) ;
				return null ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------------------------------
	// より柔軟な伸長と圧縮

	/// <summary>
	/// より柔軟な伸長クラス
	/// </summary>
	public class GZipReader
	{
		//-------------------------------------------------------------------------------------------
		// より柔軟な伸長

		private Stream				m_Cis ;

		private GZipInputStream		m_Gis ;
		private	MemoryStream		m_Mos ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public GZipReader( byte[] data )
		{
			if( data == null || data.Length == 0 )
			{
				// 不可
				return ;
			}

			m_Cis = new MemoryStream( data ) ;
			Prepare() ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public GZipReader( byte[] data, int length )
		{
			if( data == null || length <= 0 ||  length >  data.Length )
			{
				// 不可
				return ;
			}

			m_Cis = new MemoryStream( data, 0, length ) ;
			Prepare() ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public GZipReader( byte[] data, int offset, int length )
		{
			if( data == null || offset <   0 || length <= 0 || offset >= data.Length || ( offset + length ) >  data.Length )
			{
				// 不可
				return ;
			}

			m_Cis = new MemoryStream( data, offset, length ) ;
			Prepare() ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public GZipReader( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return ;

			}

			m_Cis = File.OpenRead( path ) ;
			Prepare() ;
		}

		private void Prepare()
		{
			if( m_Cis == null )
			{
				return ;
			}

			m_Gis = new GZipInputStream( m_Cis ) ;
			m_Mos = new MemoryStream() ;
		}

		/// <summary>
		/// 全てのストリームを閉じる
		/// </summary>
		/// <returns></returns>
		public void Close()
		{
			if( m_Mos != null )
			{
				m_Mos.Close() ;
				m_Mos  = null ;
			}

			if( m_Gis != null )
			{
				m_Gis.Close() ;
				m_Gis  = null ;
			}

			if( m_Cis != null )
			{
				m_Cis.Close() ;
				m_Cis  = null ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したサイズ分の伸長を行いデータを取得する
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public byte[] Get( int size )
		{
			if( size <= 0 )
			{
				return null ;
			}

			byte[] data = new byte[ size ] ;

			Get( data ) ;

			return data ;
		}

		/// <summary>
		/// 予め確保されたバッファに伸長したデータを格納する
		/// </summary>
		/// <param name="readBuffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public int Get( byte[] readBuffer )
		{
			if( readBuffer == null )
			{
				return -1 ;
			}
			return Get( readBuffer, 0, readBuffer.Length ) ;
		}

		/// <summary>
		/// 予め確保されたバッファに伸長したデータを格納する
		/// </summary>
		/// <param name="readBuffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public int Get( byte[] readBuffer, int length )
		{
			if( readBuffer == null )
			{
				return -1 ;
			}
			return Get( readBuffer, 0, length ) ;
		}

		/// <summary>
		/// 予め確保されたバッファに伸長したデータを格納する
		/// </summary>
		/// <param name="readBuffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public int Get( byte[] readBuffer, int offset, int length )
		{
			if( readBuffer == null || readBuffer.Length == 0 )
			{
				return -1 ;
			}

			if( offset <  0 || offset >= readBuffer.Length )
			{
				return -1 ;
			}

			if( length <= 0 || length >  readBuffer.Length )
			{
				length  = readBuffer.Length ;
			}

			if( ( offset + length ) >  readBuffer.Length )
			{
				length = readBuffer.Length - offset ;
			}
		
			//----------------------------------------------------------

			if( m_Gis == null )
			{
				return -1 ;
			}

			//----------------------------------

			int size ;
			int step = 0 ;

			do
			{
				size = m_Gis.Read( readBuffer, offset + step, length - step ) ;
				if( size >  0 )
				{
					step += size ;
					if( step >= length )
					{
						break ;	// 規定のサイズを伸長した
					}
				}
			}
			while( size >  0 ) ;

			// 実際に伸長できたサイズを返す
			return step ;
		}


		/// <summary>
		/// 指定のサイズ分伸長したデータを取得する
		/// </summary>
		/// <param name="readBuffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public byte[] Read( int length = 0, int bufferSize = 4096 )
		{
			if( bufferSize <=    0 )
			{
				bufferSize  = 4096 ;
			}

			// 一時バッファ確保
			byte[] buffer = new byte[ bufferSize ] ;

			int size ;
			int step = 0 ;

			int stepLength ;

			do
			{
				if( length >  0 )
				{
					stepLength = length - step ;
					if( stepLength <= 0 )
					{
						break ;
					}
					
					if( stepLength >  buffer.Length )
					{
						stepLength  = buffer.Length ;
					}
				}
				else
				{
					stepLength  = bufferSize ;
				}

				size = m_Gis.Read( buffer, 0, stepLength ) ;
				if( size >  0 )
				{
					step += size ;
					m_Mos.Write( buffer, 0, size ) ;
				}
			}
			while( size >  0 ) ;

			// length と 配列サイズが同じになるとは限らない事に注意する
			return m_Mos.ToArray() ;
		}
	}

	/// <summary>
	/// より柔軟な圧縮クラス
	/// </summary>
	public class GZipWriter
	{
		//-------------------------------------------------------------------------------------------
		// より柔軟な圧縮

		private MemoryStream		m_Mos ;
		private GZipOutputStream	m_Gos ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public GZipWriter( int level = 3 )
		{
			m_Mos = new MemoryStream() ;
			m_Gos = new GZipOutputStream( m_Mos ) ;

			m_Gos.SetLevel( level ) ;
		}

		/// <summary>
		/// 圧縮対象データを追加する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Set( byte[] data )
		{
			if( data == null )
			{
				return false ;
			}
			return Set( data, 0, data.Length ) ;
		}

		/// <summary>
		/// 圧縮対象データを追加する
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Set( byte[] data, int length )
		{
			if( data == null )
			{
				return false ;
			}
			return Set( data, 0, length ) ;
		}

		/// <summary>
		/// 圧縮対象データを追加する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public bool Set( byte[] data, int offset, int length )
		{
			if( data == null || data.Length == 0 )
			{
				return false ;
			}

			if( offset <  0 || offset >= data.Length )
			{
				return false ;
			}

			if( length <= 0 || length >  data.Length )
			{
				length  = data.Length ;
			}

			if( ( offset + length ) >  data.Length )
			{
				length = data.Length - offset ;
			}
		
			//----------------------------------------------------------

			if( m_Gos == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			m_Gos.Write( data, offset, length ) ;

			return true ;
		}

		/// <summary>
		/// 圧縮を終了し圧縮データを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] Close()
		{
			byte[] compressedData = null ;

			if( m_Gos != null )
			{
				m_Gos.Finish() ;

				m_Gos.Close() ;
				m_Gos  = null ;
			}

			if( m_Mos != null )
			{
				compressedData = m_Mos.ToArray() ;

				m_Mos.Close() ;
				m_Mos  = null ;
			}

			return compressedData ;
		}
	}

}
