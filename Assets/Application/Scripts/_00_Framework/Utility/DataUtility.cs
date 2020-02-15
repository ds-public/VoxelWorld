using System ;
using System.IO ;
using System.IO.Compression ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using UnityEngine ;

namespace DBS
{
	public class DataUtility
	{
		/// <summary>
		/// オブジェクトをシリアライズする
		/// </summary>
		/// <param name="data"></param>
		/// <param name="type"></param>
		/// <param name="isCompression"></param>
		/// <returns></returns>
		public static byte[] Serialize<T>( T dataObject, bool isCompression = true, int type = 1 )
		{
			if( dataObject == null )
			{
				return null ;
			}

			byte[] dataBuffer = null ;

			if( type == 0 )
			{
				// Json 版(属性が必要)
				string text = JsonUtility.ToJson( dataObject ) ;
				dataBuffer = Encoding.UTF8.GetBytes( text ) ;

				if( isCompression == true )
				{
					dataBuffer = ZipUtility.Compress( dataBuffer ) ;
				}
			}
			else
			if( type == 1 )
			{
				// MessagePack 版(属性が必要)
				if( isCompression == false )
				{
					dataBuffer = MessagePackSerializer.Serialize( dataObject ) ;
				}
				else
				{
					dataBuffer = LZ4MessagePackSerializer.Serialize( dataObject ) ;
				}
			}
			else
			{
				// 失敗
				return null ;
			}

			return dataBuffer ;
		}

		/// <summary>
		/// オブジェクトをデシリアライズする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dadaBuffer"></param>
		/// <param name="isDecompression"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T Deserialize<T>( byte[] dataBuffer, bool isDecompression = true, int type = 1 )
		{
			if( dataBuffer.IsNullOrEmpty() == true )
			{
				return default( T ) ;
			}

			T dataObject = default( T );

			if( type == 0 )
			{
				// Json 版(属性が必要)
				if( isDecompression == true )
				{
					dataBuffer = ZipUtility.Decompress( dataBuffer ) ;
				}

				string text = Encoding.UTF8.GetString( dataBuffer ) ;
				dataObject = JsonUtility.FromJson<T>( text ) ;
			}
			else
			if( type == 1 )
			{
				// MessagePack 版(属性が必要)
				if( isDecompression == false )
				{
					dataObject = MessagePackSerializer.Deserialize<T>( dataBuffer ) ;
				}
				else
				{
					dataObject = LZ4MessagePackSerializer.Deserialize<T>( dataBuffer ) ;
				}
			}
			else
			{
				// 失敗
				return default( T ) ;
			}

			return dataObject ;
		}
	}
}
