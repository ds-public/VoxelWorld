#nullable enable
#pragma warning disable CA1822
#pragma warning disable CS0030
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
#pragma warning disable IDE0301
#pragma warning disable IDE0305


using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;
using System.Security.Cryptography ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// データフォーマット操作クラス
	/// </summary>
	public class DataFormat
	{
		/// <summary>
		/// バイト配列を１６進数文字列にしたものを取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string ByteToString( List<byte> data, int offset = 0, int length = 0 )
		{
			return ByteToString( data.ToArray(), offset, length ) ;
		}

		/// <summary>
		/// バイト配列を１６進数文字列にしたものを取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string ByteToString( byte[] data, int offset = 0, int length = 0 )
		{
			if( data == null || data.Length == 0 || offset >= data.Length )
			{
				return string.Empty ;
			}

			if( length <= 0 )
			{
				length  = data.Length ;
			}

			if( ( offset + length ) > data.Length )
			{
				length = ( data.Length - offset ) ;
			}

			string s = data[ offset ].ToString( "X2" ) ;
			int i, l = length ;
			for( i  = 1 ; i <  l ; i ++ )
			{
				s += " " + data[ offset + i ].ToString( "X2" ) ;
			}

			return s ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// Bool 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static bool GetBool( byte[] data, ref int offset )
		{
			if( offset >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			bool value = ( data[ offset ] != 0 ) ;

			offset ++ ;

			return value ;
		}

		/// <summary>
		/// 列挙子を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static T GetEnum<T>( byte[] data, ref int offset ) where T : Enum
		{
			try
			{
				Type enumType = typeof( T ) ;

				switch( Type.GetTypeCode( enumType ) )
				{

					case TypeCode.Byte :
						byte byteValue = GetByte( data, ref offset ) ;
						if( Enum.IsDefined( enumType, byteValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, byteValue ) ;

					case TypeCode.SByte :
						sbyte sbyteValue = GetSByte( data, ref offset ) ;
						if( Enum.IsDefined( enumType, sbyteValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, sbyteValue ) ;

					case TypeCode.Int16 :
						short shortValue = GetShort( data, ref offset ) ;
						if( Enum.IsDefined( enumType, shortValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, shortValue ) ;

					case TypeCode.UInt16 :
						ushort ushortValue = GetUShort( data, ref offset ) ;
						if( Enum.IsDefined( enumType, ushortValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, ushortValue ) ;

					case TypeCode.Int32 :
						int intValue = GetInt( data, ref offset ) ;
						if( Enum.IsDefined( enumType, intValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, intValue ) ;

					case TypeCode.UInt32 :
						uint uintValue = GetUInt( data, ref offset ) ;
						if( Enum.IsDefined( enumType, uintValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, uintValue ) ;

					case TypeCode.Int64 :
						long longValue = GetLong( data, ref offset ) ;
						if( Enum.IsDefined( enumType, longValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, longValue ) ;

					case TypeCode.UInt64 :
						ulong ulongValue = GetULong( data, ref offset ) ;
						if( Enum.IsDefined( enumType, ulongValue ) == false )
						{
							throw new Exception() ;
						}
						return ( T )Enum.ToObject( enumType, ulongValue ) ;

					default :
						throw new Exception() ;
				}

			}
			catch( Exception )
			{
				throw ;
			}
		}

		/// <summary>
		/// Byte 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static byte GetByte( byte[] data, ref int offset )
		{
			if( offset >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			byte value = data[ offset ] ;

			offset ++ ;

			return value ;
		}

		/// <summary>
		/// Byte 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static sbyte GetSByte( byte[] data, ref int offset )
		{
			if( offset >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			sbyte value = ( sbyte )data[ offset ] ;

			offset ++ ;

			return value ;
		}

		/// <summary>
		/// 符号あり Short 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static short GetShort( byte[] data, ref int offset )
		{
			if( ( offset + 2 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			int value =
				  data[ offset     ]         |
				( data[ offset + 1 ] <<  8 ) ;

			offset += 2 ;

			return ( short )value ;
		}

		/// <summary>
		/// 符号なし Short 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static ushort GetUShort( ReadOnlySpan<byte> data, ref int offset )
		{
			if( ( offset + 2 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			int value =
				  data[ offset     ]         |
				( data[ offset + 1 ] <<  8 ) ;

			offset += 2 ;

			return ( ushort )value ;
		}

		/// <summary>
		/// 符号なし Short 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static ushort GetVUShort( byte[] data, ref int offset )
		{
			if( offset >= data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			int value = data[ offset ] ;
			offset ++ ;

			if( ( value & 0x80 ) != 0 )
			{
				// 上位バイトあり

				if( offset >= data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				value = ( value & 0x7F ) | ( data[ offset ] << 7 ) ;
				offset ++ ;
			}

			return ( ushort )value ;
		}

		/// <summary>
		/// 符号あり Int 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static int GetInt( byte[] data, ref int offset )
		{
			if( ( offset + 4 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			int value =
				  data[ offset     ]         |
				( data[ offset + 1 ] <<  8 ) |
				( data[ offset + 2 ] << 16 ) |
				( data[ offset + 3 ] << 24 ) ;

			offset += 4 ;

			return value ;
		}

		/// <summary>
		/// 符号なし Int 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static uint GetUInt( byte[] data, ref int offset )
		{
			if( ( offset + 4 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			int value =
				  data[ offset     ]         |
				( data[ offset + 1 ] <<  8 ) |
				( data[ offset + 2 ] << 16 ) |
				( data[ offset + 3 ] << 24 ) ;

			offset += 4 ;

			return ( uint )value ;
		}

		/// <summary>
		/// 符号あり Long 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static long GetLong( byte[] data, ref int offset )
		{
			if( ( offset + 8 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  ( long )data[ offset     ]         |
				( ( long )data[ offset + 1 ] <<  8 ) |
				( ( long )data[ offset + 2 ] << 16 ) |
				( ( long )data[ offset + 3 ] << 24 ) |
				( ( long )data[ offset + 4 ] << 32 ) |
				( ( long )data[ offset + 5 ] << 40 ) |
				( ( long )data[ offset + 6 ] << 48 ) |
				( ( long )data[ offset + 7 ] << 56 ) ;

			offset += 8 ;

			return value ;
		}

		/// <summary>
		/// 符号なし Long 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static ulong GetULong( byte[] data, ref int offset )
		{
			if( ( offset + 8 ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			ulong value =
				  ( ulong )data[ offset     ]         |
				( ( ulong )data[ offset + 1 ] <<  8 ) |
				( ( ulong )data[ offset + 2 ] << 16 ) |
				( ( ulong )data[ offset + 3 ] << 24 ) |
				( ( ulong )data[ offset + 4 ] << 32 ) |
				( ( ulong )data[ offset + 5 ] << 40 ) |
				( ( ulong )data[ offset + 6 ] << 48 ) |
				( ( ulong )data[ offset + 7 ] << 56 ) ;

			offset += 8 ;

			return value ;
		}

		/// <summary>
		/// 文字列を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static string GetString( ReadOnlySpan<byte> data, ref int offset )
		{
			if( offset >= data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			byte size0 = data[ offset ] ;
			offset ++ ;

			if( size0 == 0 )
			{
				return string.Empty ;
			}

			//----------------------------------

			int size ;

			if( size0 <  128 )
			{
				// 1 byte ～ 127 byte

				size = size0 ;
			}
			else
			{
				// 128 byte ～

				if( offset >= data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				byte size1 = data[ offset ] ;
				offset ++ ;

				if( size1 <  128 )
				{
					size = ( size0 & 0x7F ) | ( size1 <<  7 ) ;
				}
				else
				{
					// 32768(32KB) ～

					if( offset >= data.Length )
					{
						throw new Exception( "データサイズ異常" ) ;
					}

					byte size2 = data[ offset ] ;
					offset ++ ;

					if( size2 <  128 )
					{
						size = ( size0 & 0x7F ) | ( ( size1 & 0x7F ) <<  7 ) | ( size2 << 14 ) ;
					}
					else
					{
						// 4194304(4MB) ～ 536870912(512MB)

						byte size3 = data[ offset ] ;
						offset ++ ;

						size = ( size0 & 0x7F ) | ( ( size1 & 0x7F ) <<  7 ) | ( ( size2 & 0x7F ) << 14 ) | ( size3 << 21 ) ;
					}
				}
			}

			if( ( offset + size ) >  data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			string text = Encoding.UTF8.GetString( data.ToArray(), offset, size ) ;

			offset += size ;

			return text ;
		}

		/// <summary>
		/// バイト配列を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] GetByteArray( byte[] data, ref int offset )
		{
			if( offset >= data.Length )
			{
				throw new Exception( "データサイズ異常[0] offest = " + offset + " length = " + data.Length ) ;
			}

			byte size0 = data[ offset ] ;
			offset ++ ;

			if( size0 == 0 )
			{
				// 空配列
				return Array.Empty<byte>() ;
			}

			//----------------------------------

			int size ;

			if( size0 <  128 )
			{
				// 1 byte ～ 127 byte

				size = size0 ;
			}
			else
			{
				// 128 byte ～

				if( offset >= data.Length )
				{
					throw new Exception( "データサイズ異常[1] offset = " + offset + " length = " + data.Length ) ;
				}

				byte size1 = data[ offset ] ;
				offset ++ ;

				if( size1 <  128 )
				{
					// ～ 16383 byte (16KB)

					size = ( size0 & 0x7F ) | ( size1 <<  7 ) ;
				}
				else
				{
					// 16384 (16KB) ～

					if( offset >= data.Length )
					{
						throw new Exception( "データサイズ異常[2] offset = " + offset + " length = " + data.Length ) ;
					}

					byte size2 = data[ offset ] ;
					offset ++ ;

					if( size2 <  128 )
					{
						// ～ 2097151 byte (2MB)
						size = ( size0 & 0x7F ) | ( ( size1 & 0x7F ) <<  7 ) | ( size2 << 14 ) ;
					}
					else
					{
						// 2097152 byte (2MB) ～ 536870911 byte (512MB)

						byte size3 = data[ offset ] ;
						offset ++ ;

						size = ( size0 & 0x7F ) | ( ( size1 & 0x7F ) <<  7 ) | ( ( size2 & 0x7F ) << 14 ) | ( size3 << 21 ) ;
					}
				}
			}

			if( ( offset + size ) >  data.Length )
			{
				throw new Exception( "データサイズ異常[3] size = " + size + " offset = " + offset + " length = " + data.Length ) ;
			}

			byte[] byteArray = new byte[ size ] ;

			Array.Copy( data, offset, byteArray, 0, size ) ;

			offset += size ;

			return byteArray ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// Bool 値を格納する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="value"></param>
		public static void PutBool( List<byte> data, bool value )
		{
			data.Add( value == false ? ( byte )0 : ( byte )1 ) ;
		}

		/// <summary>
		/// 列挙子を格納する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static void PutEnum<T>( List<byte> data, T value ) where T : Enum
		{
			switch( Type.GetTypeCode( typeof( T ) ) )
			{
				case TypeCode.Byte :
					PutByte( data, ( byte )( ( object )value ) ) ;
					return ;

				case TypeCode.SByte :
					PutSByte( data, ( sbyte )( ( object )value ) ) ;
					return ;

				case TypeCode.Int16 :
					PutShort( data, ( short )( ( object )value ) ) ;
					return ;

				case TypeCode.UInt16 :
					PutUShort( data, ( ushort )( ( object )value ) ) ;
					return ;

				case TypeCode.Int32 :
					PutInt( data, ( int )( ( object )value ) ) ;
					return ;

				case TypeCode.UInt32 :
					PutUInt( data, ( uint )( ( object )value ) ) ;
					return ;

				case TypeCode.Int64 :
					PutLong( data, ( long )( ( object )value ) ) ;
					return ;

				case TypeCode.UInt64 :
					PutULong( data, ( ulong )( ( object )value ) ) ;
					return ;
			}
		}

		/// <summary>
		/// 符号なし Byte 値を格納する
		/// </summary>
		/// <param name="value"></param>
		public static void PutByte( List<byte> data, byte value )
		{
			data.Add( value ) ;
		}

		/// <summary>
		/// 符号あり Byte 値を格納する
		/// </summary>
		/// <param name="value"></param>
		public static void PutSByte( List<byte> data, sbyte value )
		{
			data.Add( ( byte )value ) ;
		}

		/// <summary>
		/// 符号あり Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutShort( List<byte> data, short value )
		{
			data.Add( ( byte )  value          ) ;
			data.Add( ( byte )( value >>  8  ) ) ;
		}

		/// <summary>
		/// 符号なし Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutUShort( List<byte> data, ushort value )
		{
			data.Add( ( byte )  value          ) ;
			data.Add( ( byte )( value >>  8  ) ) ;
		}

		/// <summary>
		/// 可変長の符号なし Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutVUShort( List<byte> data, ushort value )
		{
			if( value <  128 )
			{
				// 1 byte
				data.Add( ( byte )value ) ;
			}
			else
			{
				// 2 byte
				data.Add( ( byte )( ( value & 0x7F ) | 0x80 ) ) ;
				data.Add( ( byte )(   value >>  7  ) ) ;
			}
		}

		/// <summary>
		/// 符号あり Int 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutInt( List<byte> data, int value )
		{
			data.Add( ( byte )  value         ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
		}

		/// <summary>
		/// 符号なし Int 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutUInt( List<byte> data, uint value )
		{
			data.Add( ( byte )  value         ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
		}

		/// <summary>
		/// 符号あり Long 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutLong( List<byte> data, long value )
		{
			data.Add( ( byte )  value         ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
			data.Add( ( byte )( value >> 32 ) ) ;
			data.Add( ( byte )( value >> 40 ) ) ;
			data.Add( ( byte )( value >> 48 ) ) ;
			data.Add( ( byte )( value >> 56 ) ) ;
		}

		/// <summary>
		/// 符号なし Long 値を追加する
		/// </summary>
		/// <param name="value"></param>
		public static void PutULong( List<byte> data, ulong value )
		{
			data.Add( ( byte )  value         ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
			data.Add( ( byte )( value >> 32 ) ) ;
			data.Add( ( byte )( value >> 40 ) ) ;
			data.Add( ( byte )( value >> 48 ) ) ;
			data.Add( ( byte )( value >> 56 ) ) ;
		}

		/// <summary>
		/// 文字列を格納する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="text"></param>
		public static void PutString( List<byte> data, string text )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				// null か 空文字
				data.Add( 0 ) ;
			}
			else
			{
				byte[] buffer = Encoding.UTF8.GetBytes( text ) ;

				// 文字列のサイズ値は可変長

				int size = buffer.Length ;

				if( size <  128 )
				{
					// 最大 128 - 1 文字まで
					data.Add( ( byte )size ) ;
				}
				else
				{
					if( size <  32768 )
					{
						// 最大 32767(32KB) - 1 文字まで
						data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     size >>  7                   ) ) ;
					}
					else
					{
						if( size <  4194304 )
						{
							// 最大 4194303(4MB) - 1 文字まで
							data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
							data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
							data.Add( ( byte )(     size >> 14                   ) ) ;
						}
						else
						{
							// 最大 536870911(512MB) - 1 文字まで
							data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
							data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
							data.Add( ( byte )( ( ( size >> 14 ) & 0x7F ) | 0x80 ) ) ;
							data.Add( ( byte )(     size >> 21                   ) ) ;
						}
					}
				}

				//----------------------------------------------------------

				data.AddRange( buffer ) ;
			}
		}


		/// <summary>
		/// Byte 配列を追加する
		/// </summary>
		/// <param name="data"></param>
		public static void PutByteArray( List<byte> data, byte[] byteArray )
		{
			if( byteArray == null || byteArray.Length == 0 )
			{
				data.Add( 0 ) ;
				return ;
			}

			//----------------------------------

			int size = byteArray.Length ;

			if( size <  128 )
			{
				// 1 byte ～ 127 byte
				data.Add( ( byte )size ) ;
			}
			else
			{
				// 128 byte ～
				if( size <  16384 )
				{
					// ～ 16383 byte (16KB)
					data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
					data.Add( ( byte )(     size >>  7                   ) ) ;
				}
				else
				{
					// 16384 byte (16KB) ～
					if( size <  2097152 )
					{
						// ～ 2097151 byte (2MB)
						data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     size >> 14                   ) ) ;
					}
					else
					{
						// 2097152 byte (2MB) ～ 536870911 byte (512MB)
						data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >> 14 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     size >> 21                   ) ) ;
					}
				}
			}

			//----------------------------------------------------------

			data.AddRange( byteArray ) ;
		}

		/// <summary>
		/// Byte 配列を追加する
		/// </summary>
		/// <param name="data"></param>
		public static void PutByteArray( List<byte> data, List<byte> byteArray )
		{
			if( byteArray == null || byteArray.Count == 0 )
			{
				data.Add( 0 ) ;
				return ;
			}

			//----------------------------------

			int size = byteArray.Count ;

			if( size <  128 )
			{
				// 1 byte ～ 127 byte
				data.Add( ( byte )size ) ;
			}
			else
			{
				// 128 byte ～
				if( size <  16384 )
				{
					// ～ 16383 byte (16KB)
					data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
					data.Add( ( byte )(     size >>  7                   ) ) ;
				}
				else
				{
					// 16384 byte (16KB) ～
					if( size <  2097152 )
					{
						// ～ 2097151 byte (2MB)
						data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     size >> 14                   ) ) ;
					}
					else
					{
						// 2097152 byte (2MB) ～ 536870911 byte (512MB)
						data.Add( ( byte )( (   size         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( size >> 14 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     size >> 21                   ) ) ;
					}
				}
			}

			//----------------------------------------------------------

			data.AddRange( byteArray ) ;
		}


		/// <summary>
		/// Byte 配列を追加する
		/// </summary>
		/// <param name="data"></param>
		public static void PutByteArray( List<byte> data, byte[] byteArray, int offset, int length )
		{
			if( byteArray == null || byteArray.Length == 0 || length == 0 )
			{
				// 空配列
				data.Add( 0 ) ;
				return ;
			}

			//-------------------------

			if( length <  128 )
			{
				// 1 byte ～ 127 byte
				data.Add( ( byte )length ) ;
			}
			else
			{
				// 128 byte ～
				if( length <  16384 )
				{
					// ～ 16383 byte (16KB)
					data.Add( ( byte )( (   length         & 0x7F ) | 0x80 ) ) ;
					data.Add( ( byte )(     length >>  7                   ) ) ;
				}
				else
				{
					// 16384 byte (16KB) ～
					if( length <  2097152 )
					{
						// ～ 2097151 byte (2MB)
						data.Add( ( byte )( (   length         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( length >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     length >> 14                   ) ) ;
					}
					else
					{
						// 2097152 byte (2MB) ～ 536870911 byte (512MB)
						data.Add( ( byte )( (   length         & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( length >>  7 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )( ( ( length >> 14 ) & 0x7F ) | 0x80 ) ) ;
						data.Add( ( byte )(     length >> 21                   ) ) ;
					}
				}
			}

			//----------------------------------------------------------

			if( offset == 0 && length == byteArray.Length )
			{
				data.AddRange( byteArray ) ;
			}
			else
			{
				var buffer = new byte[ length ] ;
				Array.Copy( byteArray, offset, buffer, 0, length ) ;

				data.AddRange( buffer ) ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------
	// パケットの種別

	/// <summary>
	/// パケットの種別
	/// </summary>
	public enum PacketTypes
	{
		/// <summary>
		/// ＴＣＰ
		/// </summary>
		TCP	= 1,

		/// <summary>
		/// ＵＤＰ
		/// </summary>
		UDP = 2,
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// リクエスト種別
	/// </summary>
	public enum RequestTypes : byte
	{
		/// <summary>
		/// 公開鍵を取得する
		/// </summary>
		GetPublicKey                =   1,

		/// <summary>
		/// 任意データの送受信(無認証可能)
		/// </summary>
		GetStatus					= 10,

		/// <summary>
		/// ログイン要求(無認証可能)
		/// </summary>
		Login						= 11,

		/// <summary>
		/// ログアウト要求
		/// </summary>
		Logout						= 12,

		/// <summary>
		/// リフレッシュ要求
		/// </summary>
		Refresh						= 13,

		/// <summary>
		/// ゲストアカウント生成(無認証可能)
		/// </summary>
		CreateGuestAccount			= 14,

		/// <summary>
		/// アカウント生成(無認証可能)
		/// </summary>
		CreateAccount				=  15,

		/// <summary>
		/// プラットフォームアカウント生成(無認証可能)
		/// </summary>
		CreatePlatformAccount		=  16,

		/// <summary>
		/// プラットフォームアカウント引継(無認証可能)
		/// </summary>
		TakeOverPlatformAccount		=  17,

		/// <summary>
		/// アカウント生成またはログイン(無認証可能)
		/// </summary>
		CreateAccountOrLogin		=  18,

		//-----------------------------------------------------------

		/// <summary>
		/// 任意機能の実行
		/// </summary>
		CallFunction				= 20,

		/// <summary>
		/// セッション生成
		/// </summary>
		CreateSession				= 21,

		/// <summary>
		/// セッション参加
		/// </summary>
		JoinToSession				= 22,

		/// <summary>
		/// セッション離脱
		/// </summary>
		LeaveFromSession			= 23,

		/// <summary>
		/// セッション情報取得
		/// </summary>
		GetSessions					= 24,

		/// <summary>
		/// フレンド情報取得
		/// </summary>
		GetFriends					= 25,

		/// <summary>
		/// セッションのスコープタイプの変更
		/// </summary>
		SetSessionScopeType			= 26,

		//---------------

		/// <summary>
		/// ユーザー名の更新
		/// </summary>
		UpdateUserName				=  32,

		//---------------

		/// <summary>
		/// ユーザー情報群取得(デバッグ)
		/// </summary>
		GetUsers					=  64,

		/// <summary>
		/// フレンド設定(デバッグ)
		/// </summary>
		SetFriend					=  65,

		//-----------------------------------------------------------

		/// <summary>
		/// グルーピングサービスを開始する
		/// </summary>
		StartGroupingService		=  96,

		/// <summary>
		/// セッションへのマッチングを開始する
		/// </summary>
		StartMatchingToSession		=  97,

		/// <summary>
		/// セッションへのマッチングを実行する
		/// </summary>
		ExecuteMatchingToSession	=  98,

		/// <summary>
		/// セッションへのマッチングを中断する
		/// </summary>
		StopMatchingToSession		=  99,
	}

	/// <summary>
	/// ネットワーク対象の種別
	/// </summary>
	public enum NetworkTargetTypes
	{
		/// <summary>
		/// クライアント対象
		/// </summary>
		Client = 0,

		/// <summary>
		/// サーバー対象
		/// </summary>
		Server = 1,
	}

	/// <summary>
	/// 通信コマンド種別
	/// </summary>
	public enum CommandTypes : byte
	{
		/// <summary>
		/// セッションにバインドする(Client→Server)
		/// </summary>
		BindClientToSessionPlayer			=  10,

		/// <summary>
		/// バインドが完了した応答(Server→Client)
		/// </summary>
		BindClientToSessionPlayerComplated	=  11,

		/// <summary>
		/// フレーム通信が可能になった事サーバーからクライアントに通知する(Server→Client)
		/// </summary>
		ServerReady							=  12,

		/// <summary>
		/// フレーム通信が可能になった事を理解した旨をクライアントからサーバーに通知する(Client→Server)
		/// </summary>
		ClientReady							=  13,

		/// <summary>
		/// 属しているセッションでアラートが発生した
		/// </summary>
		SessionAlert						=  15,

		//-----------------------------------

		/// <summary>
		/// セッションにプレイヤーが参加(Server→Client)
		/// </summary>
		JoinedPlayer						=  30,

		/// <summary>
		/// セッションからプレイヤー離脱参加(Server→Client)
		/// </summary>
		LeftPlayer							=  40,

		//-----------------------------------

		/// <summary>
		/// フレーム
		/// </summary>
		Frame								=  50,

		/// <summary>
		/// フレームの再送要求
		/// </summary>
		Retransmission						=  51,

		/// <summary>
		/// 再送フレーム
		/// </summary>
		RetransmissionFrame					=  52,

		//-----------------------------------

		/// <summary>
		/// キック
		/// </summary>
		Kick								=  70,

		//-----------------------------------

		/// <summary>
		/// 接続維持のパケット(Client→Server)
		/// </summary>
		KeepAlive							=  90,

		/// <summary>
		/// 往復時間計測
		/// </summary>
		Ping								=  92,

		//-----------------------------------

		/// <summary>
		/// 通知サービスとのバインド確立要求
		/// </summary>
		BindClientToUser					= 120,

		/// <summary>
		/// 通信サービスとのバインド確立応答
		/// </summary>
		BindClientToUserComplated			= 121,

		/// <summary>
		/// 任意の通知メッセージ
		/// </summary>
		NotificationMessage					= 128,

		//---------------
		// グルーピング関連

		/// <summary>
		/// 指定したユーザー識別子群の情報を取得する
		/// </summary>
		GroupingAction						= 130,
	}

	/// <summary>
	/// グルーピングアクションの種別
	/// </summary>
	public enum GroupingActionTypes : byte
	{
		//---------------
		// 非同期グルーピングアクション系(待機が必要)


		/// <summary>
		/// グルーピングサービスの固有パラメータを設定する
		/// </summary>
		SetGroupingServiceParameter             =  10,

		/// <summary>
		/// 関連するユーザー情報を確認する
		/// </summary>
		CheckRelatedUsers						=  16,


		/// <summary>
		/// グループへの招待を付与する　※リーダー限定行動
		/// </summary>
		AffordGroupInvitation					=  20,

		/// <summary>
		/// グループへの招待を取消する　※リーダー限定行動
		/// </summary>
		CancelGroupInvitation					=  21,

		/// <summary>
		/// グループへの招待を了承する
		/// </summary>
		AcceptGroupInvitation					=  24,

		/// <summary>
		/// グループへの招待を拒否する
		/// </summary>
		RejectGroupInvitation					=  25,


		/// <summary>
		/// グループ情報群を取得する
		/// </summary>
		GetGroups                               =  27,

		/// <summary>
		/// グループへ参加する
		/// </summary>
		JoinToGroup								=  28,

		/// <summary>
		/// グループ固有パラメータを更新する
		/// </summary>
		SetGroupParameter						=  29,

		/// <summary>
		/// グループ内の固有パラメータを更新する
		/// </summary>
		SetGroupMemberParameter					=  30,

		/// <summary>
		/// グループの準備可能状態を設定する
		/// </summary>
		SetGroupMemberReady						=  31,

		/// <summary>
		/// グループから離脱する
		/// </summary>
		LeaveFromGroup							=  32,

		/// <summary>
		/// グループから排除する
		/// </summary>
		RejectGroupMember						=  34,


		/// <summary>
		/// グループを生成する　※フリーユーザー限定行動
		/// </summary>
		CreateGroup								=  36,

		/// <summary>
		/// グループを確定してマッチグを開始する
		/// </summary>
		StartSoloMatching						=  40,

		/// <summary>
		/// グループを確定してマッチグを開始する
		/// </summary>
		StartGroupMatching						=  41,

		/// <summary>
		/// マッチングを取消する
		/// </summary>
		StopMatching							=  43,

		/// <summary>
		/// グループを解散する　※リーダー限定行動　※現在未対応(LeaveFromGroupに内包されている)
		/// </summary>
		DeleteGroup								=  48,

		//---------------
		// プッシュ通知系

		/// <summary>
		/// グループへの招待を受け取った　※招待の受け取り側
		/// </summary>
		GroupInvitationReceived					=  60,

		/// <summary>
		/// グループへの招待が取り消された　※招待の受け取り側
		/// </summary>
		GroupInvitationCanceled					=  61,

		/// <summary>
		/// グループへの招待が承諾された　※オーナー側
		/// </summary>
		GroupInvitationAccepted					=  62,

		/// <summary>
		/// グループへの招待が拒否された　※オーナー側
		/// </summary>
		GroupInvitationRejected					=  63,


		/// <summary>
		/// Smallグループが生成された
		/// </summary>
		SmallGroupCreated						=  70,

		/// <summary>
		/// グループにメンバーが参加した
		/// </summary>
		GroupMemberJoined						=  72,


		/// <summary>
		/// グループメンバーの状態(固有パラメータ・準備完了)が更新された
		/// </summary>
		GroupParameterUpdated					=  73,

		/// <summary>
		/// グループメンバーの状態(固有パラメータ・準備完了)が更新された
		/// </summary>
		GroupMemberParameterUpdated				=  74,

		/// <summary>
		/// グループメンバーの状態(固有パラメータ・準備完了)が更新された
		/// </summary>
		GroupMemberReadyUpdated					=  75,


		/// <summary>
		/// グループメンバーが離脱した
		/// </summary>
		GroupMemberLeft							=  76,

		/// <summary>
		/// グループメンバーが排除された
		/// </summary>
		GroupMemberRejected						=  77,

		/// <summary>
		/// 参加中のグループが解散した
		/// </summary>
		GroupDeleted							=  78,


		/// <summary>
		/// マッチングが開始された
		/// </summary>
		MatchingStarted							=  80,

		/// <summary>
		/// グループマッチングに成功した(セッションプレイヤー全員を対象)
		/// </summary>
		MatchingCompleted						=  81,

		/// <summary>
		/// マッチングが中止された(Smallグループの場合は全員準備完了状態を解除する)
		/// </summary>
		MatchingCanceled						=  83,
	}

	/// <summary>
	/// グルーピングアクションのレスポンスコード種別(サイズがバイトである点に注意)
	/// </summary>
	public enum GroupingActionResponseCodes : byte
	{
		/// <summary>
		/// 成功した
		/// </summary>
		Succeeded					= 0,

		/// <summary>
		/// エラーが発生した(これは抽象的なもので後で細かくコードを分けるかもしれない)
		/// </summary>
		Error						=  1,

		/// <summary>
		/// 対象は既にオフライン状態になっている
		/// </summary>
		AlreadyTargetOffline		=  2,

		/// <summary>
		/// 対象は既にグループに参加してしまっている
		/// </summary>
		AlreadyTargetGroupJoined	=  3,

		/// <summary>
		/// アクセストークン異常
		/// </summary>
		AccessTokenFailed           =  9,

		/// <summary>
		/// パスワードが合わない
		/// </summary>
		PasswordFailed				= 10,

		/// <summary>
		/// メンバー数が最大に到達している
		/// </summary>
		IsMaxMembers                = 11,

		/// <summary>
		/// グループが見つからない
		/// </summary>
		GroupUnknown				= 99,
	}

	/// <summary>
	/// グループの種別
	/// </summary>
	public enum GroupTypes
	{
		/// <summary>
		/// グループではない
		/// </summary>
		None	= 0,

		/// <summary>
		/// 最大４人の小規模グループ
		/// </summary>
		Small	= 1,

		/// <summary>
		/// 最大８人の大規模グループ
		/// </summary>
		Large	= 2,
	}

	/// <summary>
	/// マッチングの取消が行われた理由
	/// </summary>
	public enum MatchingCancellationResons
	{
		/// <summary>
		/// 意味無し
		/// </summary>
		None,

		/// <summary>
		/// 取消要求が行われた
		/// </summary>
		Requested,

		/// <summary>
		/// マッチング要求ユーザーまたはマッチング要求グループ内のユーザーに切断があった
		/// </summary>
		Dicconnected,

		/// <summary>
		/// 何等かの理由でマッチング開始に失敗した(エラーメッセージ参照)
		/// </summary>
		Failed,

		/// <summary>
		/// タイムアウト(フレックスマッチのみ)
		/// </summary>
		Timeout,
	}

	/// <summary>
	/// 関連性のあるユーザー情報(基本的にはフレンドしかいないはず)
	/// </summary>
	public class RelatedUserData
	{
		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string		UserId ;

		/// <summary>
		/// 名前(ゲーム用の名前に上書きの可能性あり:UserNameとは限らない)
		/// </summary>
		public string		UserName ;

		/// <summary>
		/// オンライン中であるか
		/// </summary>
		public bool			IsOnline ;

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public string		ApplicationId ;

		/// <summary>
		/// 参加中のグループ種別
		/// </summary>
		public GroupTypes	GroupType ;

		/// <summary>
		/// 招待中(Small:オーナーでなくても可能・Large:自身がオーナーに限る)
		/// </summary>
		public bool			IsInviting ;

		/// <summary>
		/// 多目的パラメータ
		/// </summary>
		public Dictionary<string, string> Parameters ;

        /// <summary>
        /// セッション識別子(０以外でセッション参加中)
        /// </summary>
        public uint         SessionId ;
	}


	/// <summary>
	/// グループ招待情報(招待を受けた側のみ使用する)
	/// </summary>
	public class GroupInvitationData
	{
		/// <summary>
		/// 招待を送ってきたユーザーのユーザー識別子
		/// </summary>
		public string						UserId ;

		/// <summary>
		/// 招待を送ってきたユーザーの名前(ゲーム用の名前に上書きの可能性あり:UserNameとは限らない)
		/// </summary>
		public string						UserName ;

		/// <summary>
		/// 招待を送ってきたユーザーの多目的パラメータ
		/// </summary>
		public Dictionary<string,string>	Parameters ;

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public string						ApplicationId ;

		/// <summary>
		/// 招待の種別(Noneはありえない)
		/// </summary>
		public GroupTypes					GroupType ;
	}

	/// <summary>
	/// グループメンバー情報
	/// </summary>
	public class GroupMemberData
	{
		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string						UserId ;

		/// <summary>
		/// 名前(ゲーム用の名前に上書きの可能性あり:UserNameとは限らない)
		/// </summary>
		public string						UserName ;

		/// <summary>
		/// 準備完了中かどうか
		/// </summary>
		public bool							IsReady ;

		/// <summary>
		/// 多目的パラメータ
		/// </summary>
		public Dictionary<string, string>	Parameters ;

		/// <summary>
		/// グループのリーダーであるかどうか
		/// </summary>
		public bool							IsLeader ;


		//-----------------------------------

		/// <summary>
		/// 複製する
		/// </summary>
		/// <returns></returns>
		public GroupMemberData Clone()
		{
			var groupMember = new GroupMemberData()
			{
				UserId		= this.UserId,
				UserName	= this.UserName,
				IsReady		= this.IsReady,
				Parameters	= new (),
				IsLeader	= this.IsLeader
			} ;

			if( this.Parameters != null )
			{
				foreach( ( var key, var value ) in this.Parameters )
				{
					groupMember.Parameters.Add( key, value ) ;
				}
			}

			return groupMember ;
		}
	}

	/// <summary>
	/// グループ招待の応答結果
	/// </summary>
	public enum GroupInvitationResponseTypes
	{
		/// <summary>
		/// 承諾された
		/// </summary>
		Accepted,

		/// <summary>
		/// 拒否された
		/// </summary>
		Rejected,
	}

	/// <summary>
	/// グループの状況
	/// </summary>
	public enum GroupStatus
	{
		/// <summary>
		/// グループが生成された
		/// </summary>
		Created,

		/// <summary>
		/// 固有パラメータが更新された
		/// </summary>
		ParameterUpdated,

		/// <summary>
		/// メンバーが追加された
		/// </summary>
		MemberJoined,

		/// <summary>
		/// メンバーに離脱された
		/// </summary>
		MemberLeft,

		/// <summary>
		/// メンバーが更新された
		/// </summary>
		MemberUpdated,

		/// <summary>
		/// 自身がグループから排除された
		/// </summary>
		Rejected,

		/// <summary>
		/// グループが削除された
		/// </summary>
		Deleted,
	}


	/// <summary>
	/// マッチングの状況
	/// </summary>
	public enum MatchingStatus
	{
		/// <summary>
		/// マッチングが開始された
		/// </summary>
		Started,

		/// <summary>
		/// マッチングが完了した
		/// </summary>
		Completed,

		/// <summary>
		/// マッチングが取消された
		/// </summary>
		Canceled,

		/// <summary>
		/// マッチング中の切断が検出された
		/// </summary>
		Disconnected,

		/// <summary>
		/// マッチングに失敗した
		/// </summary>
		Failed,

		/// <summary>
		/// タイムアウトした(フレックスマッチのみ)
		/// </summary>
		Timeout,

		/// <summary>
		/// 取消はできない
		/// </summary>
		NoCancellation,
	}

	//------------------------------------------------------------

	/// <summary>
	/// セッションの管理タイプ
	/// </summary>
	public enum SessionManagementTypes : byte
	{
		/// <summary>
		/// ホストによる管理
		/// </summary>
		HostManagement = 1,

		/// <summary>
		/// サーバーによる管理
		/// </summary>
		ServerManagement = 2,
	}

	/// <summary>
	/// セッション情報の公開範囲
	/// </summary>
	public enum SessionScopeTypes : byte
	{
		/// <summary>
		/// 公開
		/// </summary>
		Public	= 1,

		/// <summary>
		/// 非公開
		/// </summary>
		Private = 2,
	}

	/// <summary>
	/// セッションのアラート
	/// </summary>
	public enum SessionAlertTypes : byte
	{
		/// <summary>
		/// セッションが開始する前にセッションプレイヤーの切断があった
		/// </summary>
		SessionPlayerDisconnectedBeforeSessionStarting,
	}

	/// <summary>
	/// 受信コールバックタイプ
	/// </summary>
	public enum ReceivingCallbackTypes
	{
		/// <summary>
		/// 受動的
		/// </summary>
		Passive	= 0,

		/// <summary>
		/// 能動的
		/// </summary>
		Active	= 1,
	}

	/// <summary>
	/// 送信先の種別
	/// </summary>
	public enum DestinationTypes : byte
	{
		/// <summary>
		/// セッション内の全プレイヤー対象
		/// </summary>
		Broadcast	= 0,

		/// <summary>
		/// セッション内の複数のプレイヤー対象
		/// </summary>
		Multicast	= 1,

		/// <summary>
		/// セッション内の単体のプレイヤー対象
		/// </summary>
		Unicast		= 2 ,

		/// <summary>
		/// ホストとなっているプレイヤー対象
		/// </summary>
		ToHost		= 3,

		/// <summary>
		/// サーバー対象(クライアントサーバーモードのみ有効でクライアントサーバーモードでない場合はホスト対象扱いとなる)
		/// </summary>
		ToServer	= 4,
	}

	/// <summary>
	/// 送信元の種別
	/// </summary>
	public enum SourceTypes : byte
	{
		/// <summary>
		/// サーバーを送信元としている
		/// </summary>
		FromServer = 0,

		/// <summary>
		/// クライアントを送信元としている
		/// </summary>
		FromClient = 1,
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// 各種応答パケットの基底クラス
	/// </summary>
	public class ResponsePacketBase
	{
		protected byte[]	m_Data ;

		protected int		m_Pointer ;

		//-----------------------------------

		/// <summary>
		/// リクエスト種別
		/// </summary>
		public RequestTypes		RequestType ;

		/// <summary>
		/// レスポンスコード
		/// </summary>
		public ResponseCodes	ResponseCode ;

		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string			ErrorMessage ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public ResponsePacketBase( byte[] data, int pointer = 0 )
		{
			m_Data = data ;

			m_Pointer = pointer ;
		}

		/// <summary>
		/// 現在のポインターの位置から Boolean 値を取得する
		/// </summary>
		/// <returns></returns>
		protected bool GetBool()
		{
			if( m_Pointer >= m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			byte value = m_Data[ m_Pointer ] ;

			m_Pointer ++ ;

			return value != 0 ;
		}

		/// <summary>
		/// 現在のポインターの位置からバイト値を取得する
		/// </summary>
		/// <returns></returns>
		protected byte GetByte()
		{
			if( m_Pointer >= m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			byte value = m_Data[ m_Pointer ] ;

			m_Pointer ++ ;

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号あり Short 値を取得する
		/// </summary>
		/// <returns></returns>
		protected short GetShort()
		{
			if( ( m_Pointer + 2 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  m_Data[ m_Pointer     ]         |
				( m_Data[ m_Pointer + 1 ] <<  8 ) ;

			m_Pointer += 2 ;

			return ( short )value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号なし Short 値を取得する
		/// </summary>
		/// <returns></returns>
		protected ushort GetUShort()
		{
			if( ( m_Pointer + 2 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  m_Data[ m_Pointer     ]         |
				( m_Data[ m_Pointer + 1 ] <<  8 ) ;

			m_Pointer += 2 ;

			return ( ushort )value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号なし Short 値を取得する
		/// </summary>
		/// <returns></returns>
		protected ushort GetVUShort()
		{
			if( m_Pointer >=  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			ushort value, data ;
			int shift = 0 ;

			data = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;

			if( data <  128 )
			{
				// 0
				value = data ;
			}
			else
			{
				// 0
				value = ( ushort )( data & 0x7F ) ;
				shift += 7 ;

				if( m_Pointer >=  m_Data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				data = m_Data[ m_Pointer ] ;
				m_Pointer ++ ;

				// 1
				value |= ( ushort )( data << shift ) ;
			}

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号あり Int 値を取得する
		/// </summary>
		/// <returns></returns>
		protected int GetInt()
		{
			if( ( m_Pointer + 4 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  m_Data[ m_Pointer     ]         |
				( m_Data[ m_Pointer + 1 ] <<  8 ) |
				( m_Data[ m_Pointer + 2 ] << 16 ) |
				( m_Data[ m_Pointer + 3 ] << 24 ) ;

			m_Pointer += 4 ;

			return ( int )value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号あり Int 値を取得する
		/// </summary>
		/// <returns></returns>
		protected uint GetUInt()
		{
			if( ( m_Pointer + 4 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  m_Data[ m_Pointer     ]         |
				( m_Data[ m_Pointer + 1 ] <<  8 ) |
				( m_Data[ m_Pointer + 2 ] << 16 ) |
				( m_Data[ m_Pointer + 3 ] << 24 ) ;

			m_Pointer += 4 ;

			return ( uint )value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号なし Int 値を取得する
		/// </summary>
		/// <returns></returns>
		protected uint GetVUInt()
		{
			if( m_Pointer >=  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			uint value, data ;
			int shift = 0 ;

			data = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;

			if( data <  128 )
			{
				// 0
				value = data ;
			}
			else
			{
				// 0
				value = ( ushort )( data & 0x7F ) ;
				shift += 7 ;

				if( m_Pointer >=  m_Data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				data = m_Data[ m_Pointer ] ;
				m_Pointer ++ ;

				if( data <  128 )
				{
					// 1
					value |= ( data << shift ) ;
				}
				else
				{
					// 1
					value |= ( ( data & 0x7F ) << shift ) ;
					shift += 7 ;

					if( m_Pointer >=  m_Data.Length )
					{
						throw new Exception( "データサイズ異常" ) ;
					}

					data = m_Data[ m_Pointer ] ;
					m_Pointer ++ ;

					if( data <  128 )
					{
						// 2
						value |= ( data << shift ) ;
					}
					else
					{
						// 2
						value |= ( ( data & 0x7F ) << shift ) ;
						shift += 7 ;

						data = m_Data[ m_Pointer ] ;
						m_Pointer ++ ;

						// 3
						value |= ( data << shift ) ;
					}
				}
			}

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号あり Long 値を取得する
		/// </summary>
		/// <returns></returns>
		protected long GetLong()
		{
			if( ( m_Pointer + 8 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			long value =
				  ( long )m_Data[ m_Pointer     ]         |
				( ( long )m_Data[ m_Pointer + 1 ] <<  8 ) |
				( ( long )m_Data[ m_Pointer + 2 ] << 16 ) |
				( ( long )m_Data[ m_Pointer + 3 ] << 24 ) |
				( ( long )m_Data[ m_Pointer + 4 ] << 32 ) |
				( ( long )m_Data[ m_Pointer + 5 ] << 40 ) |
				( ( long )m_Data[ m_Pointer + 6 ] << 48 ) |
				( ( long )m_Data[ m_Pointer + 7 ] << 56 ) ;

			m_Pointer += 8 ;

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号あり Long 値を取得する
		/// </summary>
		/// <returns></returns>
		protected ulong GetULong()
		{
			if( ( m_Pointer + 8 ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			ulong value =
				  ( ulong )m_Data[ m_Pointer     ]         |
				( ( ulong )m_Data[ m_Pointer + 1 ] <<  8 ) |
				( ( ulong )m_Data[ m_Pointer + 2 ] << 16 ) |
				( ( ulong )m_Data[ m_Pointer + 3 ] << 24 ) |
				( ( ulong )m_Data[ m_Pointer + 4 ] << 32 ) |
				( ( ulong )m_Data[ m_Pointer + 5 ] << 40 ) |
				( ( ulong )m_Data[ m_Pointer + 6 ] << 48 ) |
				( ( ulong )m_Data[ m_Pointer + 7 ] << 56 ) ;

			m_Pointer += 8 ;

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から符号なし Long 値を取得する
		/// </summary>
		/// <returns></returns>
		protected ulong GetVULong()
		{
			if( m_Pointer >=  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			ulong value, data ;
			int shift = 0 ;

			data = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;

			if( data <  128 )
			{
				// 0
				value = data ;
			}
			else
			{
				// 0
				value = ( ushort )( data & 0x7F ) ;
				shift += 7 ;

				if( m_Pointer >=  m_Data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				data = m_Data[ m_Pointer ] ;
				m_Pointer ++ ;

				if( data <  128 )
				{
					// 1
					value |= ( data << shift ) ;
				}
				else
				{
					// 1
					value |= ( ( data & 0x7F ) << shift ) ;
					shift += 7 ;

					if( m_Pointer >=  m_Data.Length )
					{
						throw new Exception( "データサイズ異常" ) ;
					}

					data = m_Data[ m_Pointer ] ;
					m_Pointer ++ ;

					if( data <  128 )
					{
						// 2
						value |= ( data << shift ) ;
					}
					else
					{
						// 2
						value |= ( ( data & 0x7F ) << shift ) ;
						shift += 7 ;

						if( m_Pointer >=  m_Data.Length )
						{
							throw new Exception( "データサイズ異常" ) ;
						}

						data = m_Data[ m_Pointer ] ;
						m_Pointer ++ ;

						if( data <  128 )
						{
							// 3
							value |= ( data << shift ) ;
						}
						else
						{
							// 3
							value |= ( ( data & 0x7F ) << shift ) ;
							shift += 7 ;

							if( m_Pointer >=  m_Data.Length )
							{
								throw new Exception( "データサイズ異常" ) ;
							}

							data = m_Data[ m_Pointer ] ;
							m_Pointer ++ ;

							if( data <  128 )
							{
								// 4
								value |= ( data << shift ) ;
							}
							else
							{
								// 4
								value |= ( ( data & 0x7F ) << shift ) ;
								shift += 7 ;

								if( m_Pointer >=  m_Data.Length )
								{
									throw new Exception( "データサイズ異常" ) ;
								}

								data = m_Data[ m_Pointer ] ;
								m_Pointer ++ ;

								if( data <  128 )
								{
									// 5
									value |= ( data << shift ) ;
								}
								else
								{
									// 5
									value |= ( ( data & 0x7F ) << shift ) ;
									shift += 7 ;

									if( m_Pointer >=  m_Data.Length )
									{
										throw new Exception( "データサイズ異常" ) ;
									}

									data = m_Data[ m_Pointer ] ;
									m_Pointer ++ ;

									if( data <  128 )
									{
										// 6
										value |= ( data << shift ) ;
									}
									else
									{
										// 6
										value |= ( ( data & 0x7F ) << shift ) ;
										shift += 7 ;

										data = m_Data[ m_Pointer ] ;
										m_Pointer ++ ;

										// 7
										value |= ( data << shift ) ;
									}
								}
							}
						}
					}
				}
			}

			return value ;
		}

		/// <summary>
		/// 現在のポインターの位置から Byte 配列を取得する
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		protected byte[] GetByteArray()
		{
			if( m_Pointer >= m_Data.Length )
			{
				throw new Exception( "データサイズ異常[0] offset = " + m_Pointer + " length = " + m_Data.Length ) ;
			}

			byte lSize = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;

			if( lSize == 0 )
			{
				return Array.Empty<byte>() ;
			}

			//----------------------------------

			int size ;

			if( ( lSize & 0x80 ) == 0x00 )
			{
				// 1 byte ～ 127 byte

				size = lSize ;
			}
			else
			{
				// 128 byte ～ 32767 byte (32KB)

				if( m_Pointer >= m_Data.Length )
				{
					throw new Exception( "データサイズ異常[1] offset = " + m_Pointer + " length = " + m_Data.Length ) ;
				}

				byte hSize = m_Data[ m_Pointer ] ;
				m_Pointer ++ ;

				size = ( lSize & 0x7F ) | ( hSize << 7 ) ;
			}

			if( ( m_Pointer + size ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常[2] size = " + size + " offset = " + m_Pointer + " length = " + m_Data.Length ) ;
			}

			byte[] data = new byte[ size ] ;
			Array.Copy( m_Data, m_Pointer, data, 0, size ) ;

			m_Pointer += size ;

			return data ;
		}

		/// <summary>
		/// 現在のポインターの位置から文字列を取得する
		/// </summary>
		/// <returns></returns>
		protected string GetString()
		{
			if( m_Pointer >= m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			byte lSize = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;

			if( lSize == 0 )
			{
				return string.Empty ;
			}

			//----------------------------------

			int size ;

			if( ( lSize & 0x80 ) == 0x00 )
			{
				// 1 byte ～ 127 byte

				size = lSize ;
			}
			else
			{
				// 128 byte ～

				if( m_Pointer >= m_Data.Length )
				{
					throw new Exception( "データサイズ異常" ) ;
				}

				byte hSize = m_Data[ m_Pointer ] ;
				m_Pointer ++ ;

				size = ( lSize & 0x7F ) | ( hSize << 7 ) ;
			}

			if( ( m_Pointer + size ) >  m_Data.Length )
			{
				throw new Exception( "データサイズ異常" ) ;
			}

			string text = Encoding.UTF8.GetString( m_Data, m_Pointer, size ) ;

			m_Pointer += size ;

			return text ;
		}
	}

	/// <summary>
	/// 各種要求パケットの基底クラス
	/// </summary>
	public class RequestPacketBase
	{
		// シリアライズ状態のバイト配列
		protected readonly List<byte>	m_Data ;

		/// <summary>
		/// リクエスト種別
		/// </summary>
		public RequestTypes	RequestType ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public RequestPacketBase()
		{
			m_Data = new List<byte>() ;
		}

		/// <summary>
		/// Bool 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutBool( bool value )
		{
			m_Data.Add( value == false ? ( byte )0 : ( byte )1 ) ;
		}

		/// <summary>
		/// Byte 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutByte( byte value )
		{
			m_Data.Add( value ) ;
		}

		/// <summary>
		/// 符号あり Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutShort( short value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
		}

		/// <summary>
		/// 符号なし Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutUShort( ushort value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
		}

		/// <summary>
		/// 可変長符号なし Short 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutVUShort( ushort value )
		{
			if( value <  128 )
			{
				// 1 byte
				m_Data.Add( ( byte )value ) ;
			}
			else
			{
				// 2 byte
				m_Data.Add( ( byte )( ( value & 0x7F ) | 0x80 ) ) ;
				m_Data.Add( ( byte )(   value >>  7  ) ) ;
			}
		}

		/// <summary>
		/// 符号あり Int 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutInt( int value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) ) ) ;
		}

		/// <summary>
		/// 符号なし Int 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutUInt( uint value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) ) ) ;
		}

		/// <summary>
		/// 符号あり Long 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutLong( long value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 32 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 40 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 48 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 56 ) ) ) ;
		}

		/// <summary>
		/// 符号なし Long 値を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutULong( ulong value )
		{
			m_Data.Add( ( byte )(   value         ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 32 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 40 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 48 ) ) ) ;
			m_Data.Add( ( byte )( ( value >> 56 ) ) ) ;
		}

		/// <summary>
		/// Byte 配列を追加する
		/// </summary>
		/// <param name="data"></param>
		protected void PutByteArray( byte[] data )
		{
			if( data == null || data.Length == 0 )
			{
				// 空配列
				m_Data.Add( 0 ) ;
				return ;
			}

			//-------------------------

			int size = data.Length ;

			if( size <  128 )
			{
				// 1 byte ～ 127 byte

				m_Data.Add( ( byte )size ) ;
			}
			else
			{
				// 128 byte ～ 32767 byte (32KB)
				m_Data.Add( ( byte )( ( size & 0x7F ) | 0x80 ) ) ;
				m_Data.Add( ( byte )( ( size >>   7 ) ) ) ;
			}

			//----------------------------------------------------------

			m_Data.AddRange( data ) ;
		}

		/// <summary>
		/// 文字列を追加する
		/// </summary>
		/// <param name="value"></param>
		protected void PutString( string text )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				// null か 空文字
				m_Data.Add( 0 ) ;
			}
			else
			{
				byte[] data = Encoding.UTF8.GetBytes( text ) ;

				// 文字列のサイズ値は可変長

				int size = data.Length ;

				if( size <  128 )
				{
					m_Data.Add( ( byte )size ) ;
				}
				else
				{
					// 最大 32767 文字まで
					m_Data.Add( ( byte )( ( size & 0x7F ) | 0x80 ) ) ;
					m_Data.Add( ( byte )( ( size >>   7 ) ) ) ;
				}

				//----------------------------------------------------------

				m_Data.AddRange( data ) ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------
	// レスポンスコード

	/// <summary>
	/// レスポンスコード
	/// </summary>
	public enum ResponseCodes
	{
		/// <summary>
		/// 成功した
		/// </summary>
		Succeeded = 0,

		/// <summary>
		/// エラーが発生した(これは抽象的なもので後で細かくコードを分けるかもしれない)
		/// </summary>
		Error = 50000,

		/// <summary>
		/// 接続失敗
		/// </summary>
		ConnectionFailed = 50001,

		/// <summary>
		/// 要求異常
		/// </summary>
		BadRequest = 50002,

		/// <summary>
		/// 要求失敗
		/// </summary>
		RequestFailed = 50003,

		/// <summary>
		/// 応答異常
		/// </summary>
		BadResponse = 50004,

		//-----------------------------------

		/// <summary>
		/// 既にログイン状態になっている(多重ログイン禁止)
		/// </summary>
		AlreadyLogined          = 50020,

		/// <summary>
		/// 有効なアクセストークンではない
		/// </summary>
		AccessTokenFailed       = 50030,

		/// <summary>
		/// データベースアクセスで問題が生じた
		/// </summary>
		DatabaseFailed			= 50040,

		/// <summary>
		/// ゲストアカウントの作成に失敗した
		/// </summary>
		CreateGuestCountFailed = 50110,

		/// <summary>
		/// ユーザー識別子が見つからない
		/// </summary>
		InvalidUserId = 50120,

		/// <summary>
		/// パスワードが一致しない
		/// </summary>
		InvalidPassword = 50121,

		//-----------------------------------

		/// <summary>
		/// 無効なアプリケーション識別子
		/// </summary>
		InvalidApplicationIdentifier = 50207,

		/// <summary>
		/// 無効なセッション識別子
		/// </summary>
		InvalidSessionIdentifier = 50208,

		/// <summary>
		/// 既にセッションに参加している
		/// </summary>
		AlreadyJoinedSession = 50209,

		/// <summary>
		/// セッションサーバーに接続できない
		/// </summary>
		CouldNotConnectToSessionServer = 50210,
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// セッションに参加中のプレイヤー情報
	/// </summary>
	public class ResponseSessionPlayerData
	{
		/// <summary>
		/// ユーザー識別子
		/// </summary>
		public string	UserId ;

		/// <summary>
		/// ユーザー名
		/// </summary>
		public string	UserName ;

		/// <summary>
		/// ゲストアカウントであるかどうか
		/// </summary>
		public bool		IsGuest ;

		/// <summary>
		/// ホストであるかどうか
		/// </summary>
		public bool		IsHost ;

		//---------------

		public Dictionary<string,string>	Parameters ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseSessionPlayerData()
		{
		}

#if false
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="userName"></param>
		/// <param name="isHost"></param>
		public ResponseSessionPlayerData
		(
			string	userId,
			string	userName,
			bool	isGuest,
			bool	isHost
		)
		{
			UserId		= userId ;
			UserName	= userName ;
			IsGuest		= isGuest ;
			IsHost		= isHost ;
		}

		/// <summary>
		/// 値を格納する
		/// </summary>
		/// <param name="data"></param>
		public void Encode( List<byte> data )
		{
			DataFormat.PutString( data, UserId ) ;
			DataFormat.PutString( data, UserName ) ;
			DataFormat.PutBool( data, IsGuest ) ;
			DataFormat.PutBool( data, IsHost ) ;
		}
#endif
		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public void Decode( byte[] data, ref int offset )
		{
			UserId		= DataFormat.GetString( data, ref offset ) ;
			UserName	= DataFormat.GetString( data, ref offset ) ;
			IsGuest		= DataFormat.GetBool( data, ref offset ) ;
			IsHost		= DataFormat.GetBool( data, ref offset ) ;

			Parameters = new () ;

			int pi, pl = DataFormat.GetByte( data, ref offset ) ;
			string key, value ;
			if( pl >  0 )
			{
				for( pi  = 0 ; pi <  pl ; pi ++ )
				{
					key		= DataFormat.GetString( data, ref offset ) ;
					value	= DataFormat.GetString( data, ref offset ) ;

					Parameters.Add( key, value ) ;
				}
			}
		}
	}

#if false
	/// <summary>
	/// セッション情報
	/// </summary>
	public class ResponseSessionData
	{
		/// <summary>
		/// セッション識別子
		/// </summary>
		public string	SessionId ;

		/// <summary>
		/// セッションパスワードが必要かどうか
		/// </summary>
		public bool		PasswordRequired ;

		/// <summary>
		/// 現在の人数
		/// </summary>
		public int		NowPlayers ;

		/// <summary>
		/// 最大の人数
		/// </summary>
		public int		MaxPlayers ;

		/// <summary>
		/// セッションの説明文
		/// </summary>
		public string	Description ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseSessionData()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseSessionData
		(
			string	sessionId,
			bool	passwordRequired,
			int		nowPlayers,
			int		maxPlayers,
			string	description
		)
		{
			SessionId			= sessionId ;
			PasswordRequired	= passwordRequired ;
			NowPlayers			= nowPlayers ;
			MaxPlayers			= maxPlayers ;
			Description			= description ;
		}
		
		/// <summary>
		/// 値を格納する
		/// </summary>
		/// <param name="data"></param>
		public void Encode( List<byte> data )
		{
			DataFormat.PutString( data, SessionId ) ;
			DataFormat.PutBool( data, PasswordRequired ) ;
			DataFormat.PutUShort( data, ( ushort )NowPlayers ) ;
			DataFormat.PutUShort( data, ( ushort )MaxPlayers ) ;
			DataFormat.PutString( data, Description ) ;
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public void Decode( byte[] data, ref int offset )
		{
			SessionId			= DataFormat.GetString( data, ref offset ) ;
			PasswordRequired	= DataFormat.GetBool( data, ref offset ) ;
			NowPlayers			= DataFormat.GetUShort( data, ref offset ) ;
			MaxPlayers			= DataFormat.GetUShort( data, ref offset ) ;
			Description			= DataFormat.GetString( data, ref offset ) ;
		}
	}
#endif
	/// <summary>
	/// フレンド情報
	/// </summary>
	public class ResponseFriendData
	{
		/// <summary>
		/// フレンドのユーザー識別子
		/// </summary>
		public string	UserId ;

		/// <summary>
		/// フレンドのユーザー名
		/// </summary>
		public string	UserName ;

		/// <summary>
		/// 参加中のセッション情報
		/// </summary>
		public class SessionData
		{
			/// <summary>
			/// プレイ中のアプリケーション識別子
			/// </summary>
			public string	ApplicationId ;

			/// <summary>
			/// プレイ中のアプリケーション名
			/// </summary>
			public string	ApplicationName ;

			/// <summary>
			/// 参加中のセッション識別子
			/// </summary>
			public string	SessionId ;
		}

		public List<SessionData> Sessions ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseFriendData()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseFriendData
		(
			string	userId,
			string	userName,
			List<SessionData> sessions
		)
		{
			UserId			= userId ;
			UserName		= userName ;
			Sessions		= sessions ;
		}

		/// <summary>
		/// 値を格納する
		/// </summary>
		/// <param name="data"></param>
		public void Encode( List<byte> data )
		{
			DataFormat.PutString( data, UserId ) ;
			DataFormat.PutString( data, UserName ) ;

			if( Sessions == null || Sessions.Count == 0 )
			{
				// 参加中のセッションは無し
				DataFormat.PutByte( data, 0 ) ;
			}
			else
			{
				// 参加中のセッションは有り

				// 数(最大でも２つまでなので Byte 固定で良し)
				DataFormat.PutByte( data, ( byte )Sessions.Count  ) ;

				foreach( var session in Sessions )
				{
					DataFormat.PutString( data, session.ApplicationId ) ;
					DataFormat.PutString( data, session.ApplicationName ) ;
					DataFormat.PutString( data, session.SessionId ) ;
				}
			}
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public void Decode( byte[] data, ref int offset )
		{
			UserId				= DataFormat.GetString( data, ref offset ) ;
			UserName			= DataFormat.GetString( data, ref offset ) ;

			Sessions = new List<SessionData>() ;

			int i, l = DataFormat.GetVUShort( data, ref offset ) ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Sessions.Add( new SessionData()
				{
					ApplicationId		= DataFormat.GetString( data, ref offset ),
					ApplicationName		= DataFormat.GetString( data, ref offset ),
					SessionId			= DataFormat.GetString( data, ref offset )
				} ) ;
			}
		}
	}

	//------------------------------------------------------------

	/// <summary>
	/// マッチング結果
	/// </summary>
	public class MatchingToSessionResult
	{
		/// <summary>
		/// マッチング結果
		/// </summary>
		public ResponseCodes			    ResponseCode			{ get ; private set ; }

		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string					    ErrorMessage			{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// マッチング識別子
		/// </summary>
		public ulong					    MatchingId				{ get ; private set ; }

		/// <summary>
		/// アプリケーション識別子
		/// </summary>
		public string					    ApplicationId			{ get ; private set ; }

		//---------------

		/// <summary>
		/// セッション識別子
		/// </summary>
		public uint						    SessionId				{ get ; private set ; }

		/// <summary>
		/// セッションのスコープタイプ
		/// </summary>
		public SessionScopeTypes		    ScopeType				{ get ; private set ; }

		/// <summary>
		/// セッション説明
		/// </summary>
		public string					    Description				{ get ; private set ; }

		/// <summary>
		/// セッションの最大人数
		/// </summary>
		public ushort					    MaxPlayers				{ get ; private set ; }

		/// <summary>
		/// セッションの管理方法
		/// </summary>
		public SessionManagementTypes	    ManagementType			{ get ; private set ; }

		/// <summary>
		/// ＵＤＰ通信が使用可能かどうか
		/// </summary>
		public bool						    UdpEnabled				{ get ; private set ; }

		/// <summary>
		/// ＵＤＰの再送補正が有効かどうか
		/// </summary>
		public bool						    UdpCorrectionEnabled	{ get ; private set ; }

		/// <summary>
		/// セッションの固有パラメータ
		/// </summary>
		public Dictionary<string,string>    Parameters              { get ; private set ; }

		/// <summary>
		/// サーバー側のセッションプロセッサーが有効になっているかどうか
		/// </summary>
		public bool						    ProcessorEnabled		{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// セッションのプレイユー群
		/// </summary>
		public List<SessionPlayer>		    SessionPlayers			{ get ; private set ; }

		//-----------------------------------

		/// <summary>
		/// ExchangeServer のアドレス
		/// </summary>
		public string					    ExchangeServer_Address	{ get ; private set ; }

		/// <summary>
		/// ExchangeServer のＴＣＰポート
		/// </summary>
		public ushort					    ExchangeServer_TcpPort	{ get ; private set ; }

		/// <summary>
		/// ExchangeServer のＵＤＰポート
		/// </summary>
		public ushort					    ExchangeServer_UdpPort	{ get ; private set ; }

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ(デフォルト)
		/// </summary>
		public MatchingToSessionResult()
		{
		}


		/// <summary>
		/// コンストラクタ(失敗)
		/// </summary>
		public MatchingToSessionResult
		(
			ResponseCodes	responseCode,
			string			errorMessage,

			ulong			matchingId,
			string			applicationId
		)
		{
			ResponseCode	= responseCode ;
			ErrorMessage	= errorMessage ;
			MatchingId		= matchingId ;
			ApplicationId	= applicationId ;
		}
		
		/// <summary>
		/// 成功の場合のデコード
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public bool DecodeSuccessful( byte[] data, ref int offset )
		{
			int i, l ;

			ResponseCode = ResponseCodes.Succeeded ;

			try
			{
				//-------------
				// Session

				MatchingId				= DataFormat.GetULong( data, ref offset ) ;
				
				ApplicationId			= DataFormat.GetString( data, ref offset ) ;

				//-------------

				SessionId				= DataFormat.GetUInt( data, ref offset ) ;
				Description				= DataFormat.GetString( data, ref offset ) ;
				MaxPlayers				= DataFormat.GetUShort( data, ref offset ) ;
				ScopeType				= ( SessionScopeTypes )DataFormat.GetByte( data, ref offset ) ;
				ManagementType			= ( SessionManagementTypes )DataFormat.GetByte( data, ref offset ) ;
				UdpEnabled				= DataFormat.GetBool( data, ref offset ) ;
				UdpCorrectionEnabled	= DataFormat.GetBool( data, ref offset ) ;

				Parameters              = new () ;
				l = DataFormat.GetByte( data, ref offset ) ;
				if( l >  0 )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						string key      = DataFormat.GetString( data, ref offset ) ;
						string value    = DataFormat.GetString( data, ref offset ) ;
						if( Parameters.ContainsKey( key ) == false )
						{
							Parameters.Add( key, value ) ;
						}
					}
				}

				ProcessorEnabled		= DataFormat.GetBool( data, ref offset ) ;

				//-------------
				// SessionPlayers

				SessionPlayers = new () ;

				l = DataFormat.GetVUShort( data, ref offset ) ;
				if( l >  0 )
				{
					string						userId ;
					string						userName ;
					bool						isGuest ;
					bool						isHost ;

					Dictionary<string, string>	sessionPlayerParameters ;
					int pi, pl ;
					string key, value ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						userId		= DataFormat.GetString( data, ref offset ) ;
						userName	= DataFormat.GetString( data, ref offset ) ;
						isGuest		= DataFormat.GetBool( data, ref offset ) ;
						isHost		= DataFormat.GetBool( data, ref offset ) ;

						sessionPlayerParameters = new () ;

						pl = DataFormat.GetByte( data, ref offset ) ;
						if( pl >  0 )
						{
							for( pi  = 0 ; pi <  pl ; pi ++ )
							{
								key		= DataFormat.GetString( data, ref offset ) ;
								value	= DataFormat.GetString( data, ref offset ) ;

								sessionPlayerParameters.Add( key, value ) ;
							}
						}

						SessionPlayers.Add( new
						(
							userId,
							userName,
							isGuest,
							isHost,
							sessionPlayerParameters
						) ) ;
					}
				}

				//-------------
				// ExchangeServer EndPoint

				ExchangeServer_Address	= DataFormat.GetString( data, ref offset ) ;
				ExchangeServer_TcpPort	= DataFormat.GetUShort( data, ref offset ) ;
				ExchangeServer_UdpPort	= DataFormat.GetUShort( data, ref offset ) ;
			}
			catch( Exception )
			{
				// データ異常
				ResponseCode = ResponseCodes.Error ;
				ErrorMessage = "マッチング成功通知のデータに異常がある" ;

				Debug.LogWarning( "マッチング失敗通知のデータに異常がある" ) ;
				return false ;
			}

			return true ;
		}
	
		/// <summary>
		/// 失敗の場合のデコード
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public bool DecodeFailed( byte[] data, ref int offset )
		{
			ResponseCode = ResponseCodes.Error ;

			try
			{
				MatchingId		= DataFormat.GetULong( data, ref offset ) ;
				ApplicationId	= DataFormat.GetString( data, ref offset ) ;
				ErrorMessage	= DataFormat.GetString( data, ref offset ) ;
			}
			catch( Exception )
			{
				// データ異常
				Debug.LogWarning( "マッチング失敗通知のデータに異常がある" ) ;
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// ステータスを更新する
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="rrormessage"></param>
		public void UpdateStatus( ResponseCodes responseCode, string rrormessage )
		{
			ResponseCode = responseCode ;
			ErrorMessage = rrormessage ;
		}
	}



	/// <summary>
	/// ユーザー情報
	/// </summary>
	public class ResponseUserData
	{
		/// <summary>
		/// フレンドのユーザー識別子
		/// </summary>
		public string	UserId ;

		/// <summary>
		/// フレンドのユーザー名
		/// </summary>
		public string	UserName ;

		/// <summary>
		/// ゲストアカウントかどうか
		/// </summary>
		public bool		IsGuest ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseUserData()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ResponseUserData
		(
			string	userId,
			string	userName,
			bool	isGuest
		)
		{
			UserId			= userId ;
			UserName		= userName ;
			IsGuest			= isGuest ;
		}

		/// <summary>
		/// 値を格納する
		/// </summary>
		/// <param name="data"></param>
		public void Encode( List<byte> data )
		{
			DataFormat.PutString( data, UserId ) ;
			DataFormat.PutString( data, UserName ) ;
			DataFormat.PutBool( data, IsGuest ) ;
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <param name="data"></param>
		/// <param name="posinter"></param>
		public void Decode( byte[] data, ref int offset )
		{
			UserId				= DataFormat.GetString( data, ref offset ) ;
			UserName			= DataFormat.GetString( data, ref offset ) ;
			IsGuest				= DataFormat.GetBool( data, ref offset ) ;
		}
	}





	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// 上りフレームデータ
	/// </summary>
	public class UpstreamFrameData
	{
		/// <summary>
		/// シーケンス番号
		/// </summary>
		public ushort			Sequence { get ; private set ; }

		/// <summary>
		/// 通常アップか転送アップか
		/// </summary>
		public bool				IsTransfer { get ; private set ; }

		/// <summary>
		/// 送信先の種別
		/// </summary>
		public DestinationTypes DestinationType { get; private set ; }

		/// <summary>
		/// 送信先のユーザー識別子(Multicast と Unicast 以外は null)
		/// </summary>
		public string[]			DestinationUserIds { get; private set ; }

		/// <summary>
		/// 送信元の種別
		/// </summary>
		public SourceTypes		SourceType { get; private set ; }

		/// <summary>
		/// 送信元のユーザー識別子(Multicast と Unicast 以外は null)
		/// </summary>
		public string			SourceUserId { get; private set ; }

		/// <summary>
		/// データ
		/// </summary>
		public byte[]			Data { get ; private set ; }

		/// <summary>
		/// フレームが生成された時間
		/// </summary>
		public long				NowTicks { get; private set ; }

		//-----------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="destinationType"></param>
		/// <param name="userIds"></param>
		/// <param name="data"></param>
		public UpstreamFrameData
		(
			ushort				sequence,
			bool				isTransfer,
			DestinationTypes	destinationType,
			string[]			destinationUserIds,
			SourceTypes			sourceType,
			string				sourceUserId,
			ReadOnlySpan<byte>	data
		)
		{
			Sequence			= sequence ;
			IsTransfer			= isTransfer ;
			DestinationType		= destinationType ;
			DestinationUserIds	= destinationUserIds ;
			SourceType			= sourceType ;
			SourceUserId		= sourceUserId ;

			Data				= data.ToArray() ;
			
			NowTicks			= Timer.NowTicks ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="destinationType"></param>
		/// <param name="userIds"></param>
		/// <param name="data"></param>
		public UpstreamFrameData
		(
			ushort				sequence,
			bool				isTransfer,
			DestinationTypes	destinationType,
			string[]			destinationUserIds,
			SourceTypes			sourceType,
			string				sourceUserId,
			byte[]				data
		)
		{
			Sequence			= sequence ;
			IsTransfer			= isTransfer ;
			DestinationType		= destinationType ;
			DestinationUserIds	= destinationUserIds ;
			SourceType			= sourceType ;
			SourceUserId		= sourceUserId ;

			Data				= data ;

			NowTicks			= Timer.NowTicks ;
		}

		/// <summary>
		/// 送信用にエンコード
		/// </summary>
		/// <param name="data"></param>
		public void Encode( List<byte> data )
		{
			// シーケンス番号
			DataFormat.PutUShort( data, Sequence ) ;

			//----------------------------------

			// アップ種別
			DataFormat.PutBool( data, IsTransfer ) ;

			// 送信先タイプ
			DataFormat.PutByte( data, ( byte )DestinationType ) ;

			if( IsTransfer == false )
			{
				// 通常アップ

				// 送信先
				if( DestinationType == DestinationTypes.Multicast )
				{
					// Multicast
					DataFormat.PutVUShort( data, ( ushort )DestinationUserIds.Length ) ;
					foreach( var destinationUserId in DestinationUserIds )
					{
						DataFormat.PutString( data, destinationUserId ) ;
					}
				}
				else
				if( DestinationType == DestinationTypes.Unicast )
				{
					DataFormat.PutString( data, DestinationUserIds[ 0 ] ) ;
				}
			}
			else
			{
				// 転送アップ
				DataFormat.PutString( data, DestinationUserIds[ 0 ] ) ;

				// 送信元種別
				DataFormat.PutByte( data, ( byte )SourceType ) ;

				if( SourceType == SourceTypes.FromClient )
				{
					// 送信元のユーザー識別子
					DataFormat.PutString( data, SourceUserId ) ;
				}
			}

			// データ
			DataFormat.PutByteArray( data, Data ) ;
		}
/*
		/// <summary>
		/// 送信用にエンコード
		/// </summary>
		/// <param name="data"></param>
		public byte[] Encode()
		{
			var data = new List<byte>() ;

			// シーケンス番号
			DataFormat.PutUShort( data, Sequence ) ;

			// 送信先タイプ
			DataFormat.PutByte( data, ( byte )DestinationType ) ;

			// 送信先
			if( DestinationType == DestinationTypes.Multicast )
			{
				// Multicast
				DataFormat.PutVUShort( data, ( ushort )UserIds.Length ) ;
				foreach( var userId in UserIds )
				{
					DataFormat.PutString( data, userId ) ;
				}
			}
			else
			if( DestinationType == DestinationTypes.Unicast )
			{
				DataFormat.PutString( data, UserIds[ 0 ] ) ;
			}

			// データ
			DataFormat.PutByteArray( data, Data ) ;

			return data.ToArray() ;
		}*/
	}

	/// <summary>
	/// 下りフレームデータ
	/// </summary>
	public class DownstreamFrameData
	{
		/// <summary>
		/// シーケンス番号
		/// </summary>
		public ushort				Sequence { get ; private set ; }

		/// <summary>
		/// 送信動作種別
		/// </summary>
		public bool					IsTransfer { get ; private set ; }

		/// <summary>
		/// 送信先の種別
		/// </summary>
		public DestinationTypes		DestinationType { get ; private set ; }

		/// <summary>
		/// 送信先の識別子群(Multicast と Unicast 以外は null)
		/// </summary>
		public string[]				DestinationUserIds { get ; private set ; }

		/// <summary>
		/// 送信元の種別
		/// </summary>
		public SourceTypes			SourceType { get; private set ; }

		/// <summary>
		/// 送信元のユーザー識別子(SourceTypes.Client の場合のみ有効)
		/// </summary>
		public string				SourceUserId { get; private set ; }

		/// <summary>
		/// データ
		/// </summary>
		public byte[]				Data { get ; private set ; }

		/// <summary
		/// ＴＣＰとＵＤＰのどちらでフレームを受信したか(ホストの転送アップ時に使用する)
		/// </summary>
		public PacketTypes			PacketType { get ; private set ; }

		/// <summary>
		/// フレームが生成された時間
		/// </summary>
		public long					NowTicks { get; private set ; }

		//-----------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="destinationType"></param>
		/// <param name="userIds"></param>
		/// <param name="data"></param>
		public DownstreamFrameData
		(
			ushort				sequence,
			bool				isTransfer,
			DestinationTypes	destinationType,
			string[]			destinationUserIds,
			SourceTypes			sourceType,
			string				sourceUserId,
			ReadOnlySpan<byte>	data,

			PacketTypes			packetType
		)
		{
			Sequence			= sequence ;
			IsTransfer			= isTransfer ;
			DestinationType		= destinationType ;
			DestinationUserIds	= destinationUserIds ;
			SourceType			= sourceType ;
			SourceUserId		= sourceUserId ;

			Data				= data.ToArray() ;

			PacketType			= packetType ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="destinationType"></param>
		/// <param name="userIds"></param>
		/// <param name="data"></param>
		public DownstreamFrameData
		(
			ushort				sequence,
			bool				isTransfer,
			DestinationTypes	destinationType,
			string[]			destinationUserIds,
			SourceTypes			sourceType,
			string				sourceUserId,
			byte[]				data,

			PacketTypes			packetType
		)
		{
			Sequence			= sequence ;
			IsTransfer			= isTransfer ;
			DestinationType		= destinationType ;
			DestinationUserIds	= destinationUserIds ;
			SourceType			= sourceType ;
			SourceUserId		= sourceUserId ;
			Data				= data ;

			PacketType			= packetType ;
		}
	}

	/// <summary>
	/// セッションプロセッサーの受信中のフレームデータ
	/// </summary>
	public class SessionProcessorReceivingFrameData
	{
		/// <summary>
		/// データ
		/// </summary>
		public byte[]			Data { get ; private set ; }

		/// <summary>
		/// 送信元のユーザー識別子(SourceType が FromServer の場合は null)
		/// </summary>
		public string			SourceUserId { get ; private set ; }

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="destinationType"></param>
		/// <param name="userIds"></param>
		/// <param name="data"></param>
		public SessionProcessorReceivingFrameData
		(
			byte[]				data,
			string				sourceUserId
		)
		{
			Data					= data ;
			SourceUserId			= sourceUserId ;
		}
	}

}
