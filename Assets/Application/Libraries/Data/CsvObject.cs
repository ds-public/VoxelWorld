#if UNITY_2019_4_OR_NEWER
#define UNITY
#endif

//#define IMMUTABLE_ENABLED

// null 許容を有効化しワーニングを抑制する
#nullable enable
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625

//-------------------------------------

using System ;
using System.Collections ;
using System.Collections.Generic ;
#if IMMUTABLE_ENABLED
using System.Collections.Immutable ;
#endif
using System.Reflection ;
using System.Text ;
using System.Security.Cryptography ;
using System.Linq ;
using System.Runtime.Serialization ;
using System.Runtime.CompilerServices ;

#if UNITY
	using UnityEngine ;
#endif


namespace CsvHelper
{
	/// <summary>
	/// CSVデータクラス Version 2023/10/09
	/// </summary>
	public class CsvObject
	{
		/// <summary>
		/// オリジナルのデータ
		/// </summary>
		public byte[] Data { get ; private set ; }

		/// <summary>
		/// オリジナルのデータのサイズ
		/// </summary>
		public int Size
		{
			get
			{
				if( Data == null )
				{
					return 0 ;
				}
				return Data.Length ;
			}
		}

		/// <summary>
		/// オリジナルのテキスト
		/// </summary>
		public string Text
		{
			get
			{
				if( m_Text == null )
				{
					if( Data == null || Data.Length == 0 )
					{
						m_Text = string.Empty ;
					}
					else
					{
						m_Text = Encoding.UTF8.GetString( Data ) ;
					}
				}
				return m_Text ;
			}
		}

		private string m_Text ;

		/// <summary>
		/// ハッシュ
		/// </summary>
		public string Hash
		{
			get
			{
				if( m_HashWord == null )
				{
					if( m_HashCode != null && m_HashCode.Length >  0 )
					{
						m_HashWord = BitConverter.ToString( m_HashCode ).ToLower().Replace( "-", "" ) ;
					}
				}
				return m_HashWord ;
			}
		}

		/// <summary>
		/// ハッシュを取得する
		/// </summary>
		/// <returns></returns>
		public string GetHash()	=> Hash ;

		/// <summary>
		/// ハッシュを取得する
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public string GetHash( out byte[] code )
		{
			code = m_HashCode ;
			return Hash ;
		}

		/// <summary>
		/// ハッシュ(数値版)
		/// </summary>
		private readonly byte[] m_HashCode ;

		private string m_HashWord ;

		//-----------------------------------------------------------

		/// <summary>
		/// セルの情報
		/// </summary>
		public class Cell
		{
			// セルの値
			public string Value ;

			/// <summary>
			/// コンストラクタ(デフォルト)
			/// </summary>
			public Cell()
			{
				Value	= string.Empty ;
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="value"></param>
			public Cell( System.Object value )
			{
				if( value == null )
				{
					Value = string.Empty ;
				}
				else
				{
					if( value is string s )
					{
						Value = s ;
					}
					else
					{
						Value = value.ToString() ;
					}
				}
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// Boolean 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Boolean ToBoolean( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return false ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					if( ulongValue == 0 )
					{
						return false ;
					}
					else
					{
						return true ;
					}
				}

				if( Double.TryParse( text, out double doubleValue ) == true )
				{
					if( doubleValue == 0 )
					{
						return false ;
					}
					else
					{
						return true ;
					}
				}

				text = text.ToLower() ;
				if( text == "false" )
				{
					return false ;
				}

				return true ;
			}

			/// <summary>
			/// bool 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public bool ToBool( string defaultValue = null )	=> ToBoolean( defaultValue ) ;

			/// <summary>
			/// Byte 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Byte ToByte( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Byte )ulongValue ;
				}

				if( Byte.TryParse( text, out Byte value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// SByte 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public SByte ToSByte( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( SByte )ulongValue ;
				}

				if( SByte.TryParse( text, out SByte value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// Char 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Char ToChar( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return ( Char )0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Char )ulongValue ;
				}

				if( Char.TryParse( text, out Char value ) == false )
				{
					value = ( Char )0 ;
				}

				return value ;
			}

			/// <summary>
			/// Int16 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int16 ToInt16( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int16 )ulongValue ;
				}

				if( Int16.TryParse( text, out Int16 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// short 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public short ToShort( string defaultValue = null)	=> ToInt16( defaultValue ) ;

			/// <summary>
			/// UInt16 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt16 ToUInt16( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( UInt16 )ulongValue ;
				}

				if( UInt16.TryParse( text, out UInt16 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// ushort 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ushort ToUShort( string defaultValue = null )	=> ToUInt16( defaultValue ) ;

			/// <summary>
			/// Int32 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int32 ToInt32( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int32 )ulongValue ;
				}

				if( Int32.TryParse( text, out Int32 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// int 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public int ToInt( string defaultValue = null )	=> ToInt32( defaultValue ) ;

			/// <summary>
			/// UInt32 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt32 ToUInt32( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( UInt32 )ulongValue ;
				}

				if( UInt32.TryParse( text, out UInt32 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// uint 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public uint ToUInt( string defaultValue = null )	=> ToUInt32( defaultValue ) ;

			/// <summary>
			/// Int64 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int64 ToInt64( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int64 )ulongValue ;
				}

				if( Int64.TryParse( text, out Int64 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// long 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public long ToLong( string defaultValue = null )	=> ToInt64( defaultValue ) ;

			/// <summary>
			/// UInt64 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt64 ToUInt64( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( UInt64.TryParse( text, out UInt64 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// ulong 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ulong ToULong( string defaultValue = null )	=> ToUInt64( defaultValue ) ;

			/// <summary>
			/// Single 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Single ToSingle( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Single.TryParse( text, out Single value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// float 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public float ToFloat( string defaultValue = null )	=> ToSingle( defaultValue ) ;

			/// <summary>
			/// Double 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Double ToDouble( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Double.TryParse( text, out Double value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// Decimal 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Decimal ToDecimal( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return 0 ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Decimal.TryParse( text, out Decimal value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// String 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public override String ToString()
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true )
				{
					return string.Empty ;
				}

				return text ;
			}

			/// <summary>
			/// DateTime 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public DateTime ToDateTime( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return DateTime.Now ;
				}

				if( DateTime.TryParse( text, out DateTime value ) == false )
				{
					value = DateTime.Now ;
				}

				return value ;
			}

			/// <summary>
			/// DateTimeOffset 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public DateTimeOffset ToDateTimeOffset( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return DateTimeOffset.Now ;
				}

				if( DateTimeOffset.TryParse( text, out DateTimeOffset value ) == false )
				{
					value = DateTimeOffset.Now ;
				}

				return value ;
			}

			/// <summary>
			/// TimeSpan 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public TimeSpan ToTimeSpan( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return TimeSpan.Zero ;
				}

				if( TimeSpan.TryParse( text, out TimeSpan value ) == false )
				{
					value = TimeSpan.Zero ;
				}

				return value ;
			}

			/// <summary>
			/// Guid 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public Guid ToGuid( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return Guid.Empty ;
				}

				if( Guid.TryParse( text, out Guid value ) == false )
				{
					value = Guid.Empty ;
				}

				return value ;
			}

			/// <summary>
			/// 列挙子 型でセルの値を取得する
			/// </summary>
			/// <param name="enumType"></param>
			/// <returns></returns>
			public T ToEnum<T>( string defaultValue = null ) where T :System.Enum
			{
				return ( T )ToEnum( typeof( T ), defaultValue ) ;
			}

			/// <summary>
			/// 列挙子 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <returns></returns>
			public System.Object ToEnum( Type enumType, string defaultValue = null  )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return default ;
				}

				text = GetRelationalValue( text ) ;

				if( string.IsNullOrEmpty( text ) == true )
				{
					return default ;
				}

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return Enum.ToObject( enumType, ulongValue ) ;
				}

				// 数値のケース
				TypeCode typeCode = Type.GetTypeCode( enumType ) ;
				switch( typeCode )
				{
					case TypeCode.Byte		:
					{
						if( Byte.TryParse( text, out Byte value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.SByte		:
					{
						if( SByte.TryParse( text, out SByte value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int16		:
					{
						if( Int16.TryParse( text, out Int16 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt16	:
					{
						if( UInt16.TryParse( text, out UInt16 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int32		:
					{
						if( Int32.TryParse( text, out Int32 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt32	:
					{
						if( UInt32.TryParse( text, out UInt32 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int64		:
					{
						if( Int64.TryParse( text, out Int64 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt64	:
					{
						if( UInt64.TryParse( text, out UInt64 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
				}

				// 文字のケース
				// ignoreCase : 大文字と小文字の区別が無い場合は true
				if( Enum.TryParse( enumType, text, ignoreCase:false, out System.Object enumValue ) == true )
				{
					return enumValue ;
				}

				return default ;
			}

			//----------------------------------

			/// <summary>
			/// Boolean? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Boolean? ToBooleanN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					if( ulongValue == 0 )
					{
						return false ;
					}
					else
					{
						return true ;
					}
				}

				if( Double.TryParse( text, out double doubleValue ) == true )
				{
					if( doubleValue == 0 )
					{
						return false ;
					}
					else
					{
						return true ;
					}
				}

				text = text.ToLower() ;
				if( text == "false" )
				{
					return false ;
				}

				return true ;
			}

			/// <summary>
			/// bool? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public bool? ToBoolN( string defaultValue = null )	=> ToBooleanN( defaultValue ) ;

			/// <summary>
			/// Byte? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Byte? ToByteN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Byte )ulongValue ;
				}

				if( Byte.TryParse( text, out Byte value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// SByte? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public SByte? ToSByteN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( SByte )ulongValue ;
				}

				if( SByte.TryParse( text, out SByte value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// Char? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Char? ToCharN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Char )ulongValue ;
				}

				if( Char.TryParse( text, out Char value ) == false )
				{
					value = ( Char )0 ;
				}

				return value ;
			}

			/// <summary>
			/// Int16? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int16? ToInt16N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int16 )ulongValue ;
				}

				if( Int16.TryParse( text, out Int16 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// short? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public short? ToShortN( string defaultValue = null )		=> ToInt16N( defaultValue ) ;

			/// <summary>
			/// UInt16? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt16? ToUInt16N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( UInt16 )ulongValue ;
				}

				if( UInt16.TryParse( text, out UInt16 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// ushort? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ushort? ToUShortN( string defaultValue = null )		=> ToUInt16N( defaultValue ) ;

			/// <summary>
			/// Int32 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int32? ToInt32N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int32 )ulongValue ;
				}

				if( Int32.TryParse( text, out Int32 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// int? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public int? ToIntN( string defaultValue = null )		=> ToInt32N( defaultValue ) ;

			/// <summary>
			/// UInt32? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt32? ToUInt32N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( UInt32 )ulongValue ;
				}

				if( UInt32.TryParse( text, out UInt32 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// uint? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public uint? ToUIntN( string defaultValue = null )		=> ToUInt32N( defaultValue ) ;

			/// <summary>
			/// Int64? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int64? ToInt64N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ( Int64 )ulongValue ;
				}

				if( Int64.TryParse( text, out Int64 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// long? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public long? ToLongN( string defaultValue = null )		=> ToInt64N( defaultValue ) ;

			/// <summary>
			/// UInt64? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt64? ToUInt64N( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( UInt64.TryParse( text, out UInt64 value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// ulong? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ulong? ToULongN( string defaultValue = null )	=> ToUInt64N( defaultValue ) ;

			/// <summary>
			/// Single 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Single? ToSingleN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Single.TryParse( text, out Single value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// float? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public float? ToFloatN( string defaultValue = null )	=> ToSingleN( defaultValue ) ;

			/// <summary>
			/// Double? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Double? ToDoubleN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Double.TryParse( text, out Double value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// Decimal? 型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Decimal? ToDecimalN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return ulongValue ;
				}

				if( Decimal.TryParse( text, out Decimal value ) == false )
				{
					value = 0 ;
				}

				return value ;
			}

			/// <summary>
			/// DateTime? 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public DateTime? ToDateTimeN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( DateTime.TryParse( text, out DateTime value ) == false )
				{
					value = DateTime.Now ;
				}

				return value ;
			}

			/// <summary>
			/// DateTimeOffset? 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public DateTimeOffset? ToDateTimeOffsetN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( DateTimeOffset.TryParse( text, out DateTimeOffset value ) == false )
				{
					value = DateTimeOffset.Now ;
				}

				return value ;
			}

			/// <summary>
			/// TimeSpan? 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public TimeSpan? ToTimeSpanN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( TimeSpan.TryParse( text, out TimeSpan value ) == false )
				{
					value = TimeSpan.Zero ;
				}

				return value ;
			}

			/// <summary>
			/// Guid? 型でセルの値を取得する
			/// </summary>
			/// <param name="row"></param>
			/// <param name="column"></param>
			/// <returns></returns>
			public Guid? ToGuidN( string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( Guid.TryParse( text, out Guid value ) == false )
				{
					value = Guid.Empty ;
				}

				return value ;
			}

			/// <summary>
			/// 列挙子 型でセルの値を取得する
			/// </summary>
			/// <param name="enumType"></param>
			/// <returns></returns>
			public T? ToEnumN<T>( string defaultValue = null ) where T : struct, System.Enum
			{
				return ( T? )ToEnumN( typeof( T ), defaultValue ) ;
			}

			/// <summary>
			/// 列挙子? 型でセルの値を取得する
			/// </summary>
			/// <param name="enumType"></param>
			/// <returns></returns>
			public System.Object ToEnumN( Type enumType, string defaultValue = null )
			{
				var text = Value ;

				if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
				{
					// デフォルト値を使用する
					text = defaultValue ;
				}

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				text = GetRelationalValue( text ) ;

				if( string.IsNullOrEmpty( text ) == true )
				{
					return null ;
				}

				if( TryHexStringToValue( text, out ulong ulongValue ) == true )
				{
					return Enum.ToObject( enumType, ulongValue ) ;
				}

				// 数値のケース
				TypeCode typeCode = Type.GetTypeCode( enumType ) ;
				switch( typeCode )
				{
					case TypeCode.Byte		:
					{
						if( Byte.TryParse( text, out Byte value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.SByte		:
					{
						if( SByte.TryParse( text, out SByte value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int16		:
					{
						if( Int16.TryParse( text, out Int16 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt16	:
					{
						if( UInt16.TryParse( text, out UInt16 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int32		:
					{
						if( Int32.TryParse( text, out Int32 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt32	:
					{
						if( UInt32.TryParse( text, out UInt32 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.Int64		:
					{
						if( Int64.TryParse( text, out Int64 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
					case TypeCode.UInt64	:
					{
						if( UInt64.TryParse( text, out UInt64 value ) == true )
						{
							return Enum.ToObject( enumType, value ) ;
						}
					}
					break ;
				}

				// 文字のケース
				// ignoreCase : 大文字と小文字の区別が無い場合は true
				if( Enum.TryParse( enumType, text, ignoreCase:false, out System.Object enumValue ) == true )
				{
					return enumValue ;
				}

				return null ;
			}

			//----------------------------------

			/// <summary>
			/// Boolean 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Boolean[] ToBooleanArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Boolean[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = false ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							if( ulongValue == 0 )
							{
								values[ i ] = false ;
							}
							else
							{
								values[ i ] = true ;
							}
							continue ;
						}

						if( Double.TryParse( text, out double doubleValue ) == true )
						{
							if( doubleValue == 0 )
							{
								values[ i ] = false ;
							}
							else
							{
								values[ i ] = true ;
							}
							continue ;
						}

						text = text.ToLower() ;
						if( text == "false" )
						{
							values[ i ] = false ;
						}
						else
						{
							values[ i ] = true ;
						}
					}
				}

				return values ;
			}

			/// <summary>
			/// bool 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public bool[] ToBoolArray( string defaultValue = null )	=> ToBooleanArray( defaultValue ) ;

			/// <summary>
			/// Byte 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Byte[] ToByteArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Byte[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Byte )ulongValue ;
							continue ;
						}

						if( Byte.TryParse( text, out Byte value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// SByte 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public SByte[] ToSByteArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new SByte[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( SByte )ulongValue ;
							continue ;
						}

						if( SByte.TryParse( text, out SByte value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Char 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Char[] ToCharArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Char[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = ( char )0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Char )ulongValue ;
							continue ;
						}

						if( Char.TryParse( text, out Char value ) == false )
						{
							value = ( char )0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Int16 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int16[] ToInt16Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int16[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int16 )ulongValue ;
							continue ;
						}

						if( Int16.TryParse( text, out Int16 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// short 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public short[] ToShortArray( string defaultValue = null )	=> ToInt16Array( defaultValue ) ;

			/// <summary>
			/// UInt16 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt16[] ToUInt16Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt16[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( UInt16 )ulongValue ;
							continue ;
						}

						if( UInt16.TryParse( text, out UInt16 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// ushort 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ushort[] ToUShortArray( string defaultValue = null )	=> ToUInt16Array( defaultValue ) ;

			/// <summary>
			/// Int32 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int32[] ToInt32Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int32[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int32 )ulongValue ;
							continue ;
						}

						if( Int32.TryParse( text, out Int32 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// int 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public int[] ToIntArray( string defaultValue = null )	=> ToInt32Array( defaultValue ) ;

			/// <summary>
			/// UInt32 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt32[] ToUInt32Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt32[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( UInt32 )ulongValue ;
							continue ;
						}

						if( UInt32.TryParse( text, out UInt32 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// uint 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public uint[] ToUIntArray( string defaultValue = null )	=> ToUInt32Array( defaultValue ) ;

			/// <summary>
			/// Int64 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int64[] ToInt64Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int64[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int64 )ulongValue ;
							continue ;
						}

						if( Int64.TryParse( text, out Int64 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// long 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public long[] ToLongArray( string defaultValue = null )	=> ToInt64Array( defaultValue ) ;

			/// <summary>
			/// UInt64 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt64[] ToUInt64Array( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt64[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( UInt64.TryParse( text, out UInt64 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// ulong 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ulong[] ToULongArray( string defaultValue = null )	=> ToUInt64Array( defaultValue ) ;

			/// <summary>
			/// Single 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Single[] ToSingleArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Single[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Single.TryParse( text, out Single value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// float 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public float[] ToFloatArray( string defaultValue = null )	=> ToFloatArray( defaultValue ) ;

			/// <summary>
			/// Double 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Double[] ToDoubleArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Double[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Double.TryParse( text, out Double value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Decimal 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Decimal[] ToDecimalArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Decimal[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = 0 ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Decimal.TryParse( text, out Decimal value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// String 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public String[] ToStringArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new String[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = string.Empty ;
					}
					else
					{
						values[ i ] = text ;
					}
				}

				return values ;
			}

			/// <summary>
			/// DateTime 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public DateTime[] ToDateTimeArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new DateTime[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = DateTime.Now ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( DateTime.TryParse( text, out DateTime value ) == false )
						{
							value = DateTime.Now ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// DateTimeOffset 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public DateTimeOffset[] ToDateTimeOffsetArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new DateTimeOffset[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = DateTimeOffset.Now ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( DateTimeOffset.TryParse( text, out DateTimeOffset value ) == false )
						{
							value = DateTimeOffset.Now ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// TimeSpan 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public TimeSpan[] ToTimeSpanArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new TimeSpan[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = TimeSpan.Zero ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TimeSpan.TryParse( text, out TimeSpan value ) == false )
						{
							value = TimeSpan.Zero ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Guid 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Guid[] ToGuidArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Guid[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = Guid.Empty ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( Guid.TryParse( text, out Guid value ) == false )
						{
							value = Guid.Empty ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// 列挙子 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public T[] ToEnumArray<T>( string defaultValue = null ) where T : System.Enum
			{
				var values = ToEnumArray( typeof( T ), defaultValue ) ;
				if( values == null || values.Length == 0 )
				{
					return null ;
				}

				int i, l = values.Length ;
				T[] enums = new T[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					enums[ i ] = ( T )values[ i ] ;
				}

				return enums ;
			}

			/// <summary>
			/// 列挙子 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public System.Object[] ToEnumArray( Type enumType,  string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new System.Object[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = default ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( string.IsNullOrEmpty( text ) == true )
						{
							values[ i ] = default ;
						}
						else
						{
							if( TryHexStringToValue( text, out ulong ulongValue ) == true )
							{
								values[ i ] =  Enum.ToObject( enumType, ulongValue ) ;
								continue ;
							}

							// 数値のケース
							TypeCode typeCode = Type.GetTypeCode( enumType ) ;
							switch( typeCode )
							{
								case TypeCode.Byte		:
								{
									if( Byte.TryParse( text, out Byte value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.SByte		:
								{
									if( SByte.TryParse( text, out SByte value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int16		:
								{
									if( Int16.TryParse( text, out Int16 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt16	:
								{
									if( UInt16.TryParse( text, out UInt16 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int32		:
								{
									if( Int32.TryParse( text, out Int32 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt32	:
								{
									if( UInt32.TryParse( text, out UInt32 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int64		:
								{
									if( Int64.TryParse( text, out Int64 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt64	:
								{
									if( UInt64.TryParse( text, out UInt64 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
							}

							if( values[ i ] == null )
							{
								// 文字のケース
								// ignoreCase : 大文字と小文字の区別が無い場合は true
								if( Enum.TryParse( enumType, text, ignoreCase:false, out System.Object enumValue ) == true )
								{
									values[ i ] = enumValue ;
								}
							}
						}
					}
				}

				return values ;
			}

			//--------------

			/// <summary>
			/// Boolean? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Boolean?[] ToBooleanNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Boolean?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							if( ulongValue == 0 )
							{
								values[ i ] = false ;
							}
							else
							{
								values[ i ] = true ;
							}

							continue ;
						}

						if( Double.TryParse( text, out double doubleValue ) == true )
						{
							if( doubleValue == 0 )
							{
								values[ i ] = false ;
							}
							else
							{
								values[ i ] = true ;
							}

							continue ;
						}

						text = text.ToLower() ;
						if( text == "false" )
						{
							values[ i ] = false ;
						}
						else
						{
							values[ i ] = true ;
						}
					}
				}

				return values ;
			}

			/// <summary>
			/// bool? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public bool?[] ToBoolNArray( string defaultValue = null )	=> ToBooleanNArray( defaultValue ) ;

			/// <summary>
			/// Byte? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Byte?[] ToByteNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Byte?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Byte )ulongValue ;
							continue ;
						}

						if( Byte.TryParse( text, out Byte value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// SByte? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public SByte?[] ToSByteNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new SByte?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( SByte )ulongValue ;
							continue ;
						}

						if( SByte.TryParse( text, out SByte value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Char? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Char?[] ToCharNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Char?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Char )ulongValue ;
							continue ;
						}

						if( Char.TryParse( text, out Char value ) == false )
						{
							value = ( char )0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Int16 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int16?[] ToInt16NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int16?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int16 )ulongValue ;
							continue ;
						}

						if( Int16.TryParse( text, out Int16 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// short? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public short?[] ToShortNArray( string defaultValue = null )	=> ToInt16NArray( defaultValue ) ;

			/// <summary>
			/// UInt16? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt16?[] ToUInt16NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt16?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( UInt16 )ulongValue ;
							continue ;
						}

						if( UInt16.TryParse( text, out UInt16 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// ushort? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ushort?[] ToUShortNArray( string defaultValue = null )	=> ToUInt16NArray( defaultValue ) ;

			/// <summary>
			/// Int32? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int32?[] ToInt32NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int32?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int32 )ulongValue ;
							continue ;
						}

						if( Int32.TryParse( text, out Int32 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// int? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public int?[] ToIntNArray( string defaultValue = null )	=> ToInt32NArray( defaultValue ) ;

			/// <summary>
			/// UInt32? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt32?[] ToUInt32NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt32?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( UInt32 )ulongValue ;
							continue ;
						}

						if( UInt32.TryParse( text, out UInt32 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// uint? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public uint?[] ToUIntNArray( string defaultValue = null )	=> ToUInt32NArray( defaultValue ) ;

			/// <summary>
			/// Int64? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Int64?[] ToInt64NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Int64?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ( Int64 )ulongValue ;
							continue ;
						}

						if( Int64.TryParse( text, out Int64 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// long? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public long?[] ToLongNArray( string defaultValue = null )	=> ToInt64NArray( defaultValue ) ;

			/// <summary>
			/// UInt64? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public UInt64?[] ToUInt64NArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new UInt64?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( UInt64.TryParse( text, out UInt64 value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// ulong? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public ulong?[] ToULongNArray( string defaultValue = null )	=> ToUInt64NArray( defaultValue ) ;

			/// <summary>
			/// Single? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Single?[] ToSingleNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Single?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Single.TryParse( text, out Single value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// float? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public float?[] ToFloatNArray( string defaultValue = null )	=> ToFloatNArray( defaultValue ) ;

			/// <summary>
			/// Double? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Double?[] ToDoubleNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Double?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Double.TryParse( text, out Double value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Decimal? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Decimal?[] ToDecimalNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Decimal?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TryHexStringToValue( text, out ulong ulongValue ) == true )
						{
							values[ i ] = ulongValue ;
							continue ;
						}

						if( Decimal.TryParse( text, out Decimal value ) == false )
						{
							value = 0 ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// DateTime? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public DateTime?[] ToDateTimeNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new DateTime?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( DateTime.TryParse( text, out DateTime value ) == false )
						{
							value = DateTime.Now ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// DateTimeOffset? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public DateTimeOffset?[] ToDateTimeOffsetNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new DateTimeOffset?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( DateTimeOffset.TryParse( text, out DateTimeOffset value ) == false )
						{
							value = DateTimeOffset.Now ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// TimeSpan? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public TimeSpan?[] ToTimeSpanNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new TimeSpan?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( TimeSpan.TryParse( text, out TimeSpan value ) == false )
						{
							value = TimeSpan.Zero ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// Guid? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public Guid?[] ToGuidNArray( string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new Guid?[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( Guid.TryParse( text, out Guid value ) == false )
						{
							value = Guid.Empty ;
						}
						values[ i ] = value ;
					}
				}

				return values ;
			}

			/// <summary>
			/// 列挙子? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public T?[] ToEnumNArray<T>( string defaultValue = null ) where T : struct, System.Enum
			{
				var values = ToEnumNArray( typeof( T ), defaultValue ) ;
				if( values == null || values.Length == 0 )
				{
					return null ;
				}

				int i, l = values.Length ;
				T?[] enums = new T?[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					enums[ i ] = ( T? )values[ i ] ;
				}

				return enums ;
			}

			/// <summary>
			/// 列挙子? 配列型でセルの値を取得する
			/// </summary>
			/// <returns></returns>
			public System.Object[] ToEnumNArray( Type enumType, string defaultValue = null )
			{
				var texts = GetArrayValues() ;
				if( texts == null || texts.Length == 0 )
				{
					return null ;
				}

				//---------------------------------

				var values = new System.Object[ texts.Length ] ;

				int i, l = texts.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var text = texts[ i ] ;

					if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
					{
						// デフォルト値を使用する
						text = defaultValue ;
					}

					if( string.IsNullOrEmpty( text ) == true )
					{
						values[ i ] = null ;
					}
					else
					{
						text = GetRelationalValue( text ) ;

						if( string.IsNullOrEmpty( text ) == true )
						{
							values[ i ] = null ;
						}
						else
						{
							if( TryHexStringToValue( text, out ulong ulongValue ) == true )
							{
								values[ i ] = Enum.ToObject( enumType, ulongValue ) ;
								continue ;
							}

							// 数値のケース
							TypeCode typeCode = Type.GetTypeCode( enumType ) ;
							switch( typeCode )
							{
								case TypeCode.Byte		:
								{
									if( Byte.TryParse( text, out Byte value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.SByte		:
								{
									if( SByte.TryParse( text, out SByte value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int16		:
								{
									if( Int16.TryParse( text, out Int16 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt16	:
								{
									if( UInt16.TryParse( text, out UInt16 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int32		:
								{
									if( Int32.TryParse( text, out Int32 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt32	:
								{
									if( UInt32.TryParse( text, out UInt32 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.Int64		:
								{
									if( Int64.TryParse( text, out Int64 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
								case TypeCode.UInt64	:
								{
									if( UInt64.TryParse( text, out UInt64 value ) == true )
									{
										values[ i ] = Enum.ToObject( enumType, value ) ;
									}
								}
								break ;
							}

							if( values[ i ] == null )
							{
								// 文字のケース
								// ignoreCase : 大文字と小文字の区別が無い場合は true
								if( Enum.TryParse( enumType, text, ignoreCase:false, out System.Object enumValue ) == true )
								{
									values[ i ] = enumValue ;
								}
							}
						}
					}
				}

				return values ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// 要素を分解して返す
			/// </summary>
			/// <returns></returns>
			public string[] GetArrayValues()
			{
				return Utility.GetArrayValues( Value ) ;
			}

			//------------------------------------------------------------------------------------------

			// |値|名前 形式の文字列から値部分を取り出す
			private static string GetRelationalValue( string value )
			{
				if( string.IsNullOrEmpty( value ) == true )
				{
					return value ;
				}

				if( value[ 0 ] != '|' )
				{
					return value ;
				}

				string relationalValue = value[ 1.. ] ;

				int i = relationalValue.IndexOf( '|' ) ;
				if( i <  0 )
				{
					return value ;
				}

				return relationalValue[ 0..i ] ;
			}
#if false
			// １６進数文字列を符号あり６４ビット値に変換する
			private static bool TryHexStringToValue( string hexString, out long value )
			{
				value = 0 ;

				if( string.IsNullOrEmpty( hexString ) == true )
				{
					// 不可
					return false ;
				}

				hexString = hexString.ToLower() ;

				string code = null ;
				if( hexString.Length >= 2 && hexString[ .. 2 ] == "0x" )
				{
					code = hexString[ 2.. ] ;
				}

				if( string.IsNullOrEmpty( code ) == true )
				{
					return false ;
				}

				//---------------------------------

				long unit = 1 ;
				char c ;

				int i, l = code.Length ;
				for( i  = l - 1 ; i >= 0 ; i -- )
				{
					c = code[ i ] ;
					if( c >= '0' && c <= '9' )
					{
						value += ( long )( c - '0' ) * unit ;
					}
					else
					if( c >= 'a' && c <= 'f' )
					{
						value += ( long )( c - 'a' + 10 ) * unit ;
					}
					else
					{
						// 不可
						return false ;
					}

					unit *= 16 ;
				}

				return true ;
			}
#endif
			// １６進数文字列を符号なし６４ビット値に変換する
			private static bool TryHexStringToValue( string hexString, out ulong value )
			{
				value = 0 ;

				if( string.IsNullOrEmpty( hexString ) == true )
				{
					// 不可
					return false ;
				}

				hexString = hexString.ToLower() ;

				string code = null ;
				if( hexString.Length >= 2 && hexString[ .. 2 ] == "0x" )
				{
					code = hexString[ 2.. ] ;
				}

				if( string.IsNullOrEmpty( code ) == true )
				{
					return false ;
				}

				//---------------------------------

				ulong unit = 1 ;
				char c ;

				int i, l = code.Length ;
				for( i  = l - 1 ; i >= 0 ; i -- )
				{
					c = code[ i ] ;
					if( c >= '0' && c <= '9' )
					{
						value += ( ulong )( c - '0' ) * unit ;
					}
					else
					if( c >= 'a' && c <= 'f' )
					{
						value += ( ulong )( c - 'a' + 10 ) * unit ;
					}
					else
					{
						// 不可
						return false ;
					}

					unit *= 16 ;
				}

				return true ;
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// 値は空であるかどうか
			/// </summary>
			public bool IsEmpty
			{
				get
				{
					return string.IsNullOrEmpty( Value ) ;
				}
			}

			/// <summary>
			/// 値はアレイ型であるかどうか
			/// </summary>
			public bool IsArray
			{
				get
				{
					// パースしてアレイ型かどうか確認する

					// 前後のスペースは削除されている

					if( string.IsNullOrEmpty( Value ) == true )
					{
						return false ;
					}

					// 最初に " があるかないかで処理が変わる
					// ※この " は、CSV のフォーマットの " では無い事に注意する

					// " が無い場合は、" と , はエスケープする必要がある
					// , がある場合は、必ず前後に " が存在する

					// 文字列にスペースを加えたい場合は前後に " を記述する
					// " あいうえお ", " かきくけこ ", " さしすせそ "

					// 文字列に ,(カンマ) を含めたいケース
					// "あ,い,う,え,お"
					// 　または
					// あ\,い\,う\,え\,お

					// 文字列に "(ダブルクォート) を含めたいケース
					// "あ\"い\"う\"え\"お"
					// 　または
					// あ\"い\"う\"え\"お

					if( Value[ 0 ] != '"' )
					{
						// 最初にダブルクォート無し

						bool isEscape = false ;
						char c ;
						int i, l = Value.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							c = Value[ i ] ;

							if( isEscape == false )
							{
								// エスケープ中ではない
								if( c == ',' || c == '&' )
								{
									// 要素は２つ以上の要素がある
									return true ;
								}
								else
								if( c == '\\' )
								{
									// エスケープが出現した
									isEscape = true ;
								}
							}
							else
							{
								// エスケープ中である
								isEscape = false ;	// エスケープを解除する
							}
						}
					}
					else
					{
						// 最初にダブルクォート有り
						bool isEscape = false ;
						char c ;
						int i, l = Value.Length ;
						for( i  = 1 ; i <  l ; i ++ )
						{
							c = Value[ i ] ;

							if( isEscape == false )
							{
								// エスケープ中ではない
								if( c == '"' )
								{
									// 文字列の終了を検出した
									break ;
								}
								else
								if( c == '\\' )
								{
									// エスケープが出現した
									isEscape = true ;
								}
							}
							else
							{
								// エスケープ中である
								isEscape = false ;	// エスケープを解除する
							}
						}

						i ++ ;
						if( i <  l )
						{
							// 最初の終わりの " の後にまだ文字が存在する
							c = Value[ i ] ;
							if( c == ',' || c == '&' )
							{
								// 要素は２つ以上の要素がある
								return true ;
							}
						}
					}

					// 要素は１つしかない
					return false ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------------------------------
		
		private List<List<Cell>> m_Cells ;

		/// <summary>
		/// インデクサ
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Cell this[ int row, int column ]
		{
			get
			{
				if( m_Cells == null )
				{
					return new Cell() ;
				}

				//---------------------------------

				int r = row - 1 ;
				if( r <  0 || r >= m_Cells.Count )
				{
					return new Cell() ;
				}

				if( m_Cells[ r ] == null )
				{
					return new Cell() ;
				}

				int c = column - 1 ;
				if( r <  0 || c <  0 || c >= m_Cells[ r ].Count )
				{
					return new Cell() ;
				}

				return m_Cells[ r ][ c ] ;
			}
		}

		/// <summary>
		/// 行数
		/// </summary>
		public int Row
		{
			get
			{
				if( m_Cells == null )
				{
					return 0 ;
				}
				return m_Cells.Count ;
			}
		}

		/// <summary>
		/// 列数
		/// </summary>
		public int Column
		{
			get
			{
				if( m_Cells == null )
				{
					return 0 ;
				}

				int r, c = 0 ;
				for( r  = 0 ; r <  m_Cells.Count ; r ++ )
				{
					if( m_Cells[ r ] != null )
					{
						if( m_Cells[ r ].Count >  c )
						{
							c  = m_Cells[ r ].Count ;
						}
					}
				}

				return c ;
			}
		}

		public int GetLength( int row )
		{
			if( m_Cells == null )
			{
				return 0 ;
			}

			//---------------------------------

			int r = row - 1 ;
			if( r <  0 || r >= m_Cells.Count )
			{
				return 0 ;
			}

			if( m_Cells[ r ] == null )
			{
				return 0 ;
			}

			return m_Cells[ r ].Count ;

		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ(空)
		/// </summary>
		public CsvObject()
		{
		}

		/// <summary>
		/// コンストラクタ(数値版)
		/// </summary>
		/// <param name="csvText"></param>
		/// <param name="separaterCode"></param>
		public CsvObject( byte[] data, char separaterCode = ',' )
		{
			if( data == null || data.Length == 0 )
			{
				return ;
			}

			string text = Encoding.UTF8.GetString( data ) ;

			//----------------------------------

			Data = data ;

			m_HashCode = ComputeHash( data ) ;

			Parse( text, separaterCode ) ;
		}
		/// <summary>
		/// コンストラクタ(文字版)
		/// </summary>
		/// <param name="csvText"></param>
		/// <param name="separaterCode"></param>
		public CsvObject( string text, char separaterCode = ',' )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return ;
			}

			byte[] data = Encoding.UTF8.GetBytes( text ) ;

			//----------------------------------

			Data = data ;

			m_HashCode = ComputeHash( data ) ;

			Parse( text, separaterCode ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="csvText"></param>
		/// <param name="separaterCode"></param>
		public void Parse( string text, char separaterCode = ',' )
		{
			//----------------------------------------------------------
		
			// まずコメント部分を削る
		
			int o, p, L, i ;
			char c ;
		
			//-------------------------------------------------------------
			// 改行を \n に統一する（" 内外関係無し）
		
			L = text.Length ;
		
			char[] ca = new char[ L ] ;
		
			p = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( c == 0x0D )
				{
					// CR
					if( ( i + 1 ) <  L )
					{
						if( text[ i + 1 ] == 0x0A )
						{
							// 改行
							ca[ p ] = '\n' ;
							p ++ ;
						
							i ++ ;	// １文字スキップ
						}
					}
				}
				else
				{
					ca[ p ] = c ;
					p ++ ;
				}
			}
			
			text = new string( ca, 0, p ) ;

			L = text.Length ;

			//------------------------------------------------------------------------------------------
		
			// この段階で CR は存在しない
		
			// ダブルクォートの外のカンマまたはタブの数をカウントする
			int row = 0 ;
			int column = 0, maxColumn = 0 ;
		
			int inRow, inCell ;	// セルの内側か外側か（デフォルトは外側）
			bool isDouble = false ;
			bool isEscape = false ;
		
			inRow  = 0 ;
			inCell = 0 ;

			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( inRow == 0 )
				{
					// 行開始
					column = 0 ;
					inCell = 0 ;	// セルの外側

					inRow  = 1 ;	// 行の処理内
				}
			
				if( inCell == 0 )
				{
					// セルの外側

					if( c == separaterCode )
					{
						// 区切り記号
						column ++ ;
					}
					else
					if( c == '\n' )
					{
						// 改行
						if( column >  maxColumn )
						{
							maxColumn = column ;
						}
						column = 0 ;

						row ++ ;	// 行増加

						inRow = 0 ;	// 行の処理外
					}
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						isDouble = true ;	// ダブルクォート中

						inCell = 1 ;		// セル内の状態へ移行
						isEscape = false ;
					}
					else
					if( c != ' ' )
					{
						// そのまま文字列(スペースは無視する)
						isDouble = false ;	// 非ダブルクォート

						inCell = 1 ;		// セル内の状態へ移行
					}
				}
				else
				if( inCell == 1 )
				{
					// セルの内側

					if( isDouble == true )
					{
						// ダブルクォート中の文字列
						if( isEscape == false )
						{
							// エスケープ中でない

							if( c == '"' )
							{
								if( ( i + 1 <  L ) &&  text[ i + 1 ] == '"' )
								{
									// まだ次の文字があり次も " だったらエスケープ
									isEscape = true ;
								}
								else
								{
									// そうでなければ終了
									column ++ ;

									inCell = 2 ;	// ダブルクォート後の区切り記号か改行待ちへ
								}
							}
						}
						else
						{
							// エスケープ中である

							// エスケープを解除
							isEscape = false ;
						}
					}
					else
					{
						// 非ダブルクォートの文字

						if( c == separaterCode || c == '\n' )
						{
							// 区切り記号または改行
							column ++ ;

							inCell = 0 ;

							if( c == '\n' )
							{
								if( column >  maxColumn )
								{
									maxColumn = column ;
								}
								column = 0 ;

								//-------

								row ++ ;

								inRow = 0 ;
							}
						}
					}
				}
				else
				if( inCell == 2 )
				{
					// ダブルクォート終了後のセルの外側

					if( c == separaterCode || c == '\n' )
					{
						// 区切り記号または改行

						inCell = 0 ;
					
						if( c == '\n' )
						{
							if( column >  maxColumn )
							{
								maxColumn = column ;
							}
							column = 0 ;

							//----------

							row ++ ;

							inRow = 0 ;
						}
					}
				}
			}
		
			if( inRow == 1 )
			{
				// 中途半端に終わった
				if( column >  maxColumn )
				{
					maxColumn = column ;
				}

				row ++ ;
			}
		
			//--------------------------
			
			// 配列を確保する
			int lr, lc ;

			m_Cells = new List<List<Cell>>( row ) ;

			for( lr  = 0 ; lr <  row ; lr ++ )
			{
				var cells = new List<Cell>( maxColumn ) ;
				for( lc  = 0 ; lc <  maxColumn ; lc ++ )
				{
					cells.Add( new Cell() ) ;
				}
				m_Cells.Add( cells ) ;
			}

			//-----------------------------------------------------		
			// 実際に値を格納する
		
			row = 0 ;
		
			o = -1 ;
		
			inRow  = 0 ;
			inCell = 0 ;

			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( inRow == 0 )
				{
					// 行開始
					column = 0 ;
					inCell = 0 ;

					inRow = 1 ;
				}
			
				if( inCell == 0 )
				{
					// セルの外側

					if( c == separaterCode )
					{
						// 区切り記号
						m_Cells[ row ][ column ] = new Cell() ;
						o = i + 1 ;

						column ++ ;
					}
					else
					if( c == '\n' )
					{
						// 改行
						o = i + 1 ;

						column = 0 ;
					
						//-------

						row ++ ;	// 行増加

						inRow = 0 ;	// 行の処理外へ
					}	
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						o = i + 1 ;			// ダブルクォート内の文字列の開始位置

						isDouble = true ;	// ダブルクォート中

						inCell = 1 ;		// セル内の状態へ移行
						isEscape = false ;
					}
					else
					if( c != ' ' )
					{
						// そのまま文字列(スペースは無視する)
						o = i ;				// 文字列の開始位置

						isDouble = false ;	// 非ダブルクォート

						inCell = 1 ;		// セル内の状態へ移行
					}
				}
				else
				if( inCell == 1 )
				{
					// セルの内側

					if( isDouble == true )
					{
						// ダブルクォート中の文字列
						if( isEscape == false )
						{
							// エスケープ中ではない

							if( c == '"' )
							{
								if( ( i + 1 <  L ) &&  text[ i + 1 ] == '"' )
								{
									// まだ次の文字があり次も " だったらエスケープ
									isEscape = true ;
								}
								else
								{
									// そうでなければ終了
									m_Cells[ row ][ column ] = new Cell( text[ o..i ].Replace( "\"\"", "\"" ) ) ;	// ２つ繋がるダブルクォートはエスケープされたものなので１つにする
									o = i + 2 ;

									column ++ ;

									inCell =  2 ;	// ダブルクォート後の区切り記号か改行待ちへ
								}
							}
						}
						else
						{
							// エスケープ中である

							// エスケープを解除
							isEscape = false ;
						}
					}
					else
					{
						// 非ダブルクォートの文字
						if( c == separaterCode || c == '\n' )
						{
							// 区切り記号または改行
							m_Cells[ row ][ column ] = new Cell( text[ o..i ].TrimEnd( ' ' ) ) ;
							o = i + 1 ;

							column ++ ;

							inCell = 0 ;

							if( c == '\n' )
							{
								column = 0 ;

								//-------

								row ++ ;

								inRow = 0 ;
							}
						}
					}
				}
				else
				if( inCell == 2 )
				{
					// ダブルクォート終了後のセルの外側

					if( c == separaterCode || c == '\n' )
					{
						// 区切り記号または改行

						inCell = 0 ;
					
						if( c == '\n' )
						{
							column = 0 ;

							//----------

							row ++ ;

							inRow = 0 ;
						}
					}
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// Boolean 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Boolean GetBoolean( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBoolean( defaultValue ) ;

		/// <summary>
		/// bool 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public bool GetBool( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBoolean( defaultValue ) ;

		/// <summary>
		/// Byte 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Byte GetByte( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToByte( defaultValue ) ;

		/// <summary>
		/// SByte 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public SByte GetSByte( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSByte( defaultValue ) ;

		/// <summary>
		/// Char 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Char GetChar( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToChar( defaultValue ) ;

		/// <summary>
		/// Int16 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int16 GetInt16( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt16( defaultValue ) ;

		/// <summary>
		/// short 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public short GetShort( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt16( defaultValue ) ;

		/// <summary>
		/// UInt16 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt16 GetUInt16( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt16( defaultValue ) ;

		/// <summary>
		/// ushort 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ushort GetUShort( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt16( defaultValue ) ;

		/// <summary>
		/// Int32 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int32 GetInt32( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt32( defaultValue ) ;

		/// <summary>
		/// Int 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public int GetInt( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt32( defaultValue ) ;

		/// <summary>
		/// Int32 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt32 GetUInt32( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt32( defaultValue ) ;

		/// <summary>
		/// UInt 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public uint GetUInt( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt32( defaultValue ) ;

		/// <summary>
		/// Int64 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int64 GetInt64( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt64( defaultValue ) ;

		/// <summary>
		/// Long 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public long GetLong( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt64( defaultValue ) ;

		/// <summary>
		/// UInt64 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt64 GetUInt64( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt64( defaultValue ) ;

		/// <summary>
		/// ulong 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ulong GetULong( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt64( defaultValue ) ;

		/// <summary>
		/// Single 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Single GetSingle( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSingle( defaultValue ) ;

		/// <summary>
		/// float 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public float GetFloat( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSingle( defaultValue ) ;

		/// <summary>
		/// Double 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Double GetDouble( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDouble( defaultValue ) ;

		/// <summary>
		/// Decimal 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Decimal GetDecimal( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDecimal( defaultValue ) ;

		/// <summary>
		/// String 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public String GetString( int row, int column, string defaultValue = null )
		{
			var text = this[ row, column ].ToString() ;

			if( string.IsNullOrEmpty( text ) == true && string.IsNullOrEmpty( defaultValue ) == false )
			{
				// デフォルト値を使用する
				text = defaultValue ;
			}

			return text ;
		}

		/// <summary>
		/// DateTime 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTime GetDateTime( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTime( defaultValue ) ;

		/// <summary>
		/// DateTimeOffset 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTimeOffset GetDateTimeOffset( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeOffset( defaultValue ) ;

		/// <summary>
		/// TimeSpan 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public TimeSpan GetTimeSpan( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToTimeSpan( defaultValue ) ;

		/// <summary>
		/// Guid 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Guid GetGuid( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToGuid( defaultValue ) ;

		/// <summary>
		/// 列挙子 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public T GetEnum<T>( int row, int column, string defaultValue = null ) where T :System.Enum
			=> this[ row, column ].ToEnum<T>( defaultValue ) ;

		/// <summary>
		/// 列挙子 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public System.Object GetEnum( int row, int column, Type enumType, string defaultValue = null )
			=> this[ row, column ].ToEnum( enumType, defaultValue ) ;

		//-----------------------------------------------------------

		/// <summary>
		/// Boolean? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Boolean? GetBooleanN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBooleanN( defaultValue ) ;

		/// <summary>
		/// bool? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public bool? GetBoolN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBoolN( defaultValue ) ;

		/// <summary>
		/// Byte? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Byte? GetByteN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToByteN( defaultValue ) ;

		/// <summary>
		/// SByte? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public SByte? GetSByteN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSByteN( defaultValue ) ;

		/// <summary>
		/// Char? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Char? GetCharN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToCharN( defaultValue ) ;

		/// <summary>
		/// Int16? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int16? GetInt16N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt16N( defaultValue ) ;

		/// <summary>
		/// short? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public short? GetShortN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToShortN( defaultValue ) ;

		/// <summary>
		/// UInt16? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt16? GetUInt16N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt16N( defaultValue ) ;

		/// <summary>
		/// ushort? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ushort? GetUShortN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUShortN( defaultValue ) ;

		/// <summary>
		/// Int32? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int32? GetInt32N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt32N( defaultValue ) ;

		/// <summary>
		/// int? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public int? GetIntN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToIntN( defaultValue ) ;

		/// <summary>
		/// UInt32? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt32? GetUInt32N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt32N( defaultValue ) ;

		/// <summary>
		/// uint? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public uint? GetUIntN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUIntN( defaultValue ) ;

		/// <summary>
		/// Int64? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int64? GetInt64N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt64N( defaultValue ) ;

		/// <summary>
		/// long? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public long? GetLongN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToLongN( defaultValue ) ;

		/// <summary>
		/// UInt64? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt64? GetUInt64N( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt64N( defaultValue ) ;

		/// <summary>
		/// ulong? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ulong? GetULongN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToULongN( defaultValue ) ;

		/// <summary>
		/// Single? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Single? GetSingleN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSingleN( defaultValue ) ;

		/// <summary>
		/// float? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public float? GetFloatN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToFloatN( defaultValue ) ;

		/// <summary>
		/// Double? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Double? GetDoubleN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDoubleN( defaultValue ) ;

		/// <summary>
		/// Decimal? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Decimal? GetDecimalN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDecimalN( defaultValue ) ;

		/// <summary>
		/// DateTime? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTime? GetDateTimeN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeN( defaultValue ) ;

		/// <summary>
		/// DateTimeOffset? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTimeOffset? GetDateTimeOffsetN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeOffsetN( defaultValue ) ;

		/// <summary>
		/// TimeSpan? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public TimeSpan? GetTimeSpanN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToTimeSpanN( defaultValue ) ;

		/// <summary>
		/// Guid? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Guid? GetGuidN( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToGuidN( defaultValue ) ;

		/// <summary>
		/// 列挙子? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public T? GetEnumN<T>( int row, int column, string defaultValue = null ) where T : struct, System.Enum
			=> this[ row, column ].ToEnumN<T>( defaultValue ) ;

		/// <summary>
		/// 列挙子? 型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public System.Object GetEnumN( int row, int column, Type enumType, string defaultValue = null )
			=> this[ row, column ].ToEnumN( enumType, defaultValue ) ;

		//-----------------------------------

		/// <summary>
		/// Boolean 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Boolean[] GetBooleanArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBooleanArray( defaultValue ) ;

		/// <summary>
		/// bool 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public bool[] GetBoolArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBoolArray( defaultValue ) ;

		/// <summary>
		/// Byte 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Byte[] GetByteArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToByteArray( defaultValue ) ;

		/// <summary>
		/// SByte 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public SByte[] GetSByteArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSByteArray( defaultValue ) ;

		/// <summary>
		/// Char 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Char[] GetCharArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToCharArray( defaultValue ) ;

		/// <summary>
		/// Int16 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int16[] GetInt16Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt16Array( defaultValue ) ;

		/// <summary>
		/// short 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public short[] GetShortArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToShortArray( defaultValue ) ;

		/// <summary>
		/// UInt16 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt16[] GetUInt16Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt16Array( defaultValue ) ;

		/// <summary>
		/// ushort 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ushort[] GetUShortArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUShortArray( defaultValue ) ;

		/// <summary>
		/// Int32 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int32[] GetInt32Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt32Array( defaultValue ) ;

		/// <summary>
		/// int 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public int[] GetIntArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToIntArray( defaultValue ) ;

		/// <summary>
		/// UInt32 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt32[] GetUInt32Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt32Array( defaultValue ) ;

		/// <summary>
		/// uint 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public uint[] GetUIntArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUIntArray( defaultValue ) ;

		/// <summary>
		/// Int64 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int64[] GetInt64Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt64Array( defaultValue ) ;

		/// <summary>
		/// long 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public long[] GetLongArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToLongArray( defaultValue ) ;

		/// <summary>
		/// UInt64 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt64[] GetUInt64Array( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt64Array( defaultValue ) ;

		/// <summary>
		/// ulong 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ulong[] GetULongArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToULongArray( defaultValue ) ;

		/// <summary>
		/// Single 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Single[] GetSingleArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSingleArray( defaultValue ) ;

		/// <summary>
		/// Float 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public float[] GetFloatArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToFloatArray( defaultValue ) ;

		/// <summary>
		/// Double 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Double[] GetDoubleArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDoubleArray( defaultValue ) ;

		/// <summary>
		/// Decimal 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Decimal[] GetDecimalArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDecimalArray( defaultValue ) ;

		/// <summary>
		/// String 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public String[] GetStringArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToStringArray( defaultValue ) ;

		/// <summary>
		/// DateTime 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTime[] GetDateTimeArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeArray( defaultValue ) ;

		/// <summary>
		/// DateTimeOffset 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTimeOffset[] GetDateTimeOffsetArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeOffsetArray( defaultValue ) ;

		/// <summary>
		/// TimeSpan 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public TimeSpan[] GetTimeSpanArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToTimeSpanArray( defaultValue ) ;

		/// <summary>
		/// Guid 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Guid[] GetGuidArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToGuidArray( defaultValue ) ;

		/// <summary>
		/// Enum 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public T[] GetEnumArray<T>( int row, int column, string defaultValue = null ) where T : System.Enum
			=> this[ row, column ].ToEnumArray<T>( defaultValue ) ;

		/// <summary>
		/// Enum 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public System.Object[] GetEnumArray( int row, int column, Type enumType, string defaultValue = null )
			=> this[ row, column ].ToEnumArray( enumType, defaultValue ) ;

		//---------------

		/// <summary>
		/// Boolean? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Boolean?[] GetBooleanNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBooleanNArray( defaultValue ) ;

		/// <summary>
		/// bool? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public bool?[] GetBoolNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToBoolNArray( defaultValue ) ;

		/// <summary>
		/// Byte? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Byte?[] GetByteNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToByteNArray( defaultValue ) ;

		/// <summary>
		/// SByte? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public SByte?[] GetSByteNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSByteNArray( defaultValue ) ;

		/// <summary>
		/// Char? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Char?[] GetCharNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToCharNArray( defaultValue ) ;

		/// <summary>
		/// Int16? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int16?[] GetInt16NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt16NArray( defaultValue ) ;

		/// <summary>
		/// short? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public short?[] GetShortNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToShortNArray( defaultValue ) ;

		/// <summary>
		/// UInt16? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt16?[] GetUInt16NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt16NArray( defaultValue ) ;

		/// <summary>
		/// ushort? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ushort?[] GetUShortNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUShortNArray( defaultValue ) ;

		/// <summary>
		/// Int32? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int32?[] GetInt32NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt32NArray( defaultValue ) ;

		/// <summary>
		/// int? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public int?[] GetIntNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToIntNArray( defaultValue ) ;

		/// <summary>
		/// UInt32? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt32?[] GetUInt32NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt32NArray( defaultValue ) ;

		/// <summary>
		/// uint? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public uint?[] GetUIntNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUIntNArray( defaultValue ) ;

		/// <summary>
		/// Int64? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Int64?[] GetInt64NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToInt64NArray( defaultValue ) ;

		/// <summary>
		/// long? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public long?[] GetLongNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToLongNArray( defaultValue ) ;

		/// <summary>
		/// UInt64? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public UInt64?[] GetUInt64NArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToUInt64NArray( defaultValue ) ;

		/// <summary>
		/// ulong? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public ulong?[] GetULongNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToULongNArray( defaultValue ) ;

		/// <summary>
		/// Single? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Single?[] GetSingleNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToSingleNArray( defaultValue ) ;

		/// <summary>
		/// Float? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public float?[] GetFloatNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToFloatNArray( defaultValue ) ;

		/// <summary>
		/// Double? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Double?[] GetDoubleNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDoubleNArray( defaultValue ) ;

		/// <summary>
		/// Decimal? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Decimal?[] GetDecimalNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDecimalNArray( defaultValue ) ;

		/// <summary>
		/// DateTime? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTime?[] GetDateTimeNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeNArray( defaultValue ) ;

		/// <summary>
		/// DateTimeOffset? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public DateTimeOffset?[] GetDateTimeOffsetNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToDateTimeOffsetNArray( defaultValue ) ;

		/// <summary>
		/// TimeSpan? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public TimeSpan?[] GetTimeSpanNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToTimeSpanNArray( defaultValue ) ;

		/// <summary>
		/// Guid? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public Guid?[] GetGuidNArray( int row, int column, string defaultValue = null )
			=> this[ row, column ].ToGuidNArray( defaultValue ) ;

		/// <summary>
		/// Enum? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public T?[] GetEnumNArray<T>( int row, int column, string defaultValue = null ) where T : struct, System.Enum 
			=> this[ row, column ].ToEnumNArray<T>( defaultValue ) ;

		/// <summary>
		/// Enum? 配列型でセルの値を取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public System.Object[] GetEnumNArray( int row, int column, Type enumType, string defaultValue = null )
			=> this[ row, column ].ToEnumNArray( enumType, defaultValue ) ;


		//-----------------------------------

		/// <summary>
		/// １カラム配列として値を分解して文字列として取得する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public string[] GetArrayValues( int row, int column )
			=> this[ row, column ].GetArrayValues() ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 新規に１行を追加する
		/// </summary>
		/// <param name="values"></param>
		public void AddRow( params System.Object[] values )
		{
			m_Cells ??= new List<List<Cell>>() ;

			//----------------------------------

			var cells = new List<Cell>() ;

			if( values != null && values.Length >  0 )
			{
				foreach( var value in values )
				{
					cells.Add( new Cell( value ) ) ;
				}
			}

			//----------------------------------

			m_Cells.Add( cells ) ;
		}

		/// <summary>
		/// １行を設定する
		/// </summary>
		/// <param name="values"></param>
		public void SetRow( int row, params System.Object[] values )
		{
			if( row <= 0 )
			{
				return ;
			}

			//----------------------------------------------------------

			m_Cells ??= new List<List<Cell>>() ;

			//----------------------------------

			int i, l ;

			var cells = new List<Cell>() ;

			if( values != null && values.Length >  0 )
			{
				foreach( var value in values )
				{
					cells.Add( new Cell( value ) ) ;
				}
			}

			//----------------------------------

			if( m_Cells.Count >= row )
			{
				// 指定の行は既にある

				m_Cells[ row - 1 ] = cells ;
			}
			else
			{
				// 行で足りない分を加える

				int r = row - 1 ;
				l = m_Cells.Count ;

				if( l <  r )
				{
					for( i  = 0 ; i <  ( r - l ) ; i ++ )
					{
						m_Cells.Add( new List<Cell>() ) ;
					}
				}

				m_Cells.Add( cells ) ;
			}
		}

		/// <summary>
		/// 列を追加する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="values"></param>
		public void AddColumns( int row, params System.Object[] values )
		{
			if( row <= 0 || values == null || values.Length == 0 )
			{
				return ;
			}

			//----------------------------------------------------------

			m_Cells ??= new List<List<Cell>>() ;

			//----------------------------------

			int i, l ;

			List<Cell> cells ;

			if( m_Cells.Count >= row )
			{
				// 指定の行は既にある

				cells = m_Cells[ row - 1 ] ;
			}
			else
			{
				// 行で足りない分を加える

				int r = row - 1 ;
				l = m_Cells.Count ;

				if( l <  r )
				{
					for( i  = 0 ; i <  ( r - l ) ; i ++ )
					{
						m_Cells.Add( new List<Cell>() ) ;
					}
				}

				cells = new List<Cell>() ;
				m_Cells.Add( cells ) ;
			}

			//----------------------------------

			// 値を追加していく

			l = values.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( values[ i ] == null )
				{
					cells.Add( new Cell() ) ;
				}
				else
				{
					cells.Add( new Cell( values[ i ] ) ) ;
				}
			}
		}

		/// <summary>
		/// 列を設定する
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="values"></param>
		public void SetColumns( int row, int column, params System.Object[] values )
		{
			if( row <= 0 || column <= 0 || values == null || values.Length == 0 )
			{
				return ;
			}

			//----------------------------------------------------------

			m_Cells ??= new List<List<Cell>>() ;

			//----------------------------------

			int i, l ;

			List<Cell> cells ;

			if( m_Cells.Count >= row )
			{
				// 指定の行は既にある

				cells = m_Cells[ row - 1 ] ;
			}
			else
			{
				// 行で足りない分を加える

				int r = row - 1 ;
				l = m_Cells.Count ;

				if( l <  r )
				{
					for( i  = 0 ; i <  ( r - l ) ; i ++ )
					{
						m_Cells.Add( new List<Cell>() ) ;
					}
				}

				cells = new List<Cell>() ;
				m_Cells.Add( cells ) ;
			}

			//----------------------------------

			l = column - 1 + values.Length ;

			if( cells.Count <  l )
			{
				// 列で足りない分を加える

				l -= cells.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					cells.Add( new Cell() ) ;
				}
			}

			//----------------------------------

			// 値を設定していく
			int c = column - 1 ;

			l = values.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( values[ i ] == null )
				{
					cells[ c ].Value = string.Empty ;
				}
				else
				{
					cells[ c ].Value = values[ i ].ToString() ;
				}
				c ++ ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// リフレクションのバインドフラグ
		private static readonly BindingFlags m_BindingFlags = ( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ;

		// バッキングフィールドかどうか判定する
		public static bool IsBackingField( FieldInfo field )
		{
			return field.IsDefined( typeof( CompilerGeneratedAttribute ), false ) ;
		}

		/// <summary>
		// メンバー情報を取得する
		private static List<MemberDefinition> GetMemberDefinitions( Type objectType )
		{
			// 対象メンバー情報
			var members = new List<MemberDefinition>() ;

			MemberInfo[] memberInfos = objectType.GetMembers( m_BindingFlags ) ;

			Type			type ;

			FieldInfo		field ;
			PropertyInfo	property ;

			bool isRemainGetter ;
//			bool isPublicGetter ;
			bool isRemainSetter ;

			MethodInfo getter ;
			MethodInfo setter ;

			// メンバーごとに処理を行う
			foreach( MemberInfo memberInfo in memberInfos )
			{
				type		= null ;

				field		= null ;
				property	= null ;

				if( memberInfo.MemberType == MemberTypes.Field )
				{
					// このメンバーはフィールド
					field = objectType.GetField( memberInfo.Name, m_BindingFlags ) ;

					if( IsBackingField( field ) == false )
					{
						// バッキングフィードでなければ使用可能

						// 無効指定
						bool isNonSerialized = ( field.GetCustomAttribute<System.NonSerializedAttribute>() != null ) ;
						if( isNonSerialized == false )
						{
							// フィールドは常に有効なメンバー
							type = field.FieldType ;
						}
					}
				}
				else
				if( memberInfo.MemberType == MemberTypes.Property )
				{
					// このメンバーはプロパティ


					property = objectType.GetProperty( memberInfo.Name, m_BindingFlags ) ;

					// getter が public かどうか

					isRemainGetter = false ;
//					isPublicGetter = false ;
					isRemainSetter = false ;

					// Getter の確認
					getter = property.GetMethod ;
					if( getter != null && property.CanRead == true )
					{
						isRemainGetter = true ;
//						isRemainGetter = getter.IsPublic ;
					}

					setter = property.SetMethod ;
					if( setter != null && property.CanWrite == true )
					{
						isRemainSetter = true ;
					}

					//------------

					// アトリビュートの記述が有るなら対象メンバーとするには Getter と Setter の存在が必要
					// アトリビュートの指定が無いなら対象メンバーとするには Public の Getter と Setter の存在が必要
					if( isRemainGetter == true && isRemainSetter == true )
//					if( isPublicGetter == true && isRemainSetter == true )
					{
						// 無効指定
						bool isNonSerialized = ( property.GetCustomAttribute<System.NonSerializedAttribute>() != null ) ;
						if( isNonSerialized == false )
						{
							// 有効なメンバー
							type = property.PropertyType ;
						}
					}
				}

				//---------------------------------

				if( type != null )
				{
					var originalType = type ;

					bool isNullable											= false ;
					bool isList												= false ;
					MemberDefinition.ListConversionTypes listConversionType	= MemberDefinition.ListConversionTypes.None ;
					bool isEnum												= false ;

					Type elementType	= null ;

					//--------------------------------

					if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
					{
						// IsNullable
						isNullable	= true ;

						type = Nullable.GetUnderlyingType( type ) ;
					}

					if( type.IsGenericType == true )
					{
						isList = false ;

						var genericType = type.GetGenericTypeDefinition() ;
						if
						( 
							genericType == typeof( List<>					) ||
							genericType == typeof( IList<>					) ||
							genericType == typeof( IReadOnlyList<>			) ||
							genericType == typeof( IReadOnlyCollection<>	) ||
							genericType == typeof( ICollection<>			)
						)
						{
							// List 系
							isList				= true ;
							listConversionType	= MemberDefinition.ListConversionTypes.List ;
						}
						else
						if
						(
							genericType == typeof( HashSet<>				) ||
							genericType == typeof( ISet<>					)
						)
						{
							// HashSet 系
							isList				= true ;
							listConversionType	= MemberDefinition.ListConversionTypes.HashSet ;

							PrintWarning( $"Object type '{objectType}'. Member name '{memberInfo.Name}'. This type '{genericType}' does not work in the IL2CPP build environment." ) ;
						}
						//-------------------------------------------------------
						// Immutable パッケージの追加が必要
#if IMMUTABLE_ENABLED
						else
						if
						(
							genericType == typeof( ImmutableList<>	  	    ) ||
							genericType == typeof( IImmutableList<>			)
						)
						{
							// ImmutableList 系
							isList				= true ;
							listConversionType	= MemberDefinition.ListConversionTypes.ImmutableList ;

							PrintWarning( $"Object type '{objectType}'. Member name '{memberInfo.Name}'. This type '{genericType}' does not work in the IL2CPP build environment." ) ;
						}
						else
						if
						(
							genericType == typeof( ImmutableHashSet<>		) ||
							genericType == typeof( IImmutableSet<>			)
						)
						{
							// ImmutableHashSet 系
							isList				= true ;
							listConversionType	= MemberDefinition.ListConversionTypes.ImmutableHashSet ;

							PrintWarning( $"Object type '{objectType}'. Member name '{memberInfo.Name}'. This type '{genericType}' does not work in the IL2CPP build environment." ) ;
						}
#endif
						//-------------------------------------------------------

						if( isList == true )
						{
							isNullable	= false ;

							var types = type.GenericTypeArguments ;
							if( types == null || types.Length != 1 )
							{
								// 複数のジェネリックの場合はスルーされる
								throw new Exception( message:"Only one argument of list type is valid." ) ;
							}

							type = types[ 0 ] ;
							elementType = type ;
						}
					}
					else
					if( type.IsArray == true )
					{
						// Array
						isList				= true ;
						listConversionType	= MemberDefinition.ListConversionTypes.Array ;

						isNullable	= false ;

						type = type.GetElementType() ;
						elementType = type ;
					}

					//--------------------------------

					if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
					{
						// IsNullable
						isNullable	= true ;

						type = Nullable.GetUnderlyingType( type ) ;
					}

					if( type.IsEnum == true )
					{
						// Enum
						isEnum		= true ;
					}

					//--------------------------------

					bool availableType = false ;
					
					var typeCode = Type.GetTypeCode( type ) ;

					switch( typeCode )
					{
						case TypeCode.Boolean	:
						case TypeCode.Byte		:
						case TypeCode.SByte		:
						case TypeCode.Char		:
						case TypeCode.Int16		:
						case TypeCode.UInt16	:
						case TypeCode.Int32		:
						case TypeCode.UInt32	:
						case TypeCode.Int64		:
						case TypeCode.UInt64	:
						case TypeCode.Single	:
						case TypeCode.Double	:
						case TypeCode.Decimal	:
						case TypeCode.String	:
						case TypeCode.DateTime	:
							availableType = true ;
						break ;
						case TypeCode.Object :
							if
							(
								type == typeof( DateTimeOffset )	||
								type == typeof( TimeSpan )			||
								type == typeof( Guid )
							)
							{
								availableType = true ;
							}
						break ;
					}

					// 例外を出さずに無視するケースが存在するため type が null になる可能性がある(null チェックが必要)
					// フィールドとプロパティだけで見たら対象となっている(まだ対象して確定した訳ではない)

					if( availableType == true )
					{
//						Debug.Log( "<color=#00FFFF>登録メンバー名 : " + memberInfo.Name + "</color>" ) ;

						var member = new MemberDefinition()
						{
							// 識別名
							Name				= memberInfo.Name,

							Field				= field,
							Property			= property,

							Type				= originalType,

							IsList				= isList,
							IsEnum				= isEnum,

							IsNullable			= isNullable,

							ElementType			= elementType,

							PrimitiveType		= type,
							PrimitiveTypeCode	= typeCode,
						} ;

						// メンバーの値の設定を行うコールバックを設定する
						member.SetValueCallback() ;

						if( isList == true )
						{
							member.ConversionListCallback( listConversionType ) ;
						}

						// メンバーを追加する
						members.Add( member ) ;
					}
					else
					{
						// 非対応の型は特に例外をスローさせたりはせず無視する
						// class interface struct
//						throw new Exception( message:"Member is unsupported type. : Name = " + memberInfo.Name + " Type = " + type + " / ObjectType = " + objectType.Name ) ;
					}
				}
			}

			//----------------------------------------------------------

			if( members.Count == 0 )
			{
				// 対象のメンバーが存在しない
				throw new Exception( message:"Member is empty. : " + objectType.Name ) ;
			}

			return members ;
		}

		//-------------------------------------------------------------------------------------------

		// メンバー情報に対応するカラム情報を適合させる
		private void FetchColumns( List<MemberDefinition> members, int nameRow, int dataColumn_Start )
		{
			// 列数
			int columnCount = this.Column ;

			int columnIndex ;

			// 有効な列
			var columnIndices = new List<int>() ;

			for( columnIndex = dataColumn_Start ; columnIndex <= columnCount ; columnIndex ++ )
			{
				string name = this[ nameRow, columnIndex ].ToString() ;
				if( string.IsNullOrEmpty( name ) == false )
				{
					if( name[ 0 ] != '#' )
					{
						columnIndices.Add( columnIndex ) ;
					}
				}
			}

			if( columnIndices.Count == 0 )
			{
				// 格納する値が存在しない
				throw new Exception( message:"Column is empty." ) ;
			}

			//----------------------------------

			int i, l = columnIndices.Count, p ;

			foreach( var member in members )
			{
				p = 0 ;
				while( p <  l )
				{
					// メンバー名と一致するか検査する
					for( i  = p ; i <  l ; i ++ )
					{
						columnIndex = columnIndices[ i ] ;

						string name = this[ nameRow, columnIndex ].ToString() ;

						// 名前の末尾に[]が付いていたら削る
						name = ReduceArrayElement( name ) ;

						// オリジナル名
						if( member.Name == name )
						{
							// 一致した
							break ;
						}

						// アッパーキャメル名
						string upperCamelName = SnakeToCamel( name, isLower:false ) ;
						if( member.Name == upperCamelName )
						{
							// 一致した
							break ;
						}

						// ローワーキャメル名
						string lowerCamelName = SnakeToCamel( name, isLower:true  ) ;
						if( member.Name == lowerCamelName )
						{
							// 一致した
							break ;
						}
					}

					if( i <  l )
					{
						// 格納対象となるカラムが見つかった
						member.ColumnIndices ??= new List<int>() ;
						member.ColumnIndices.Add( columnIndex ) ;

						p = i + 1 ;
					}
					else
					{
						p = i ;
					}
				}
			}
		}

		// 有効な行を取得する
		public List<int> GetAvailableRows( List<MemberDefinition> members, int dataRow_Start )
		{
			int columnIndex ;

			string check ;
			bool isAvailable ;
			bool exist ;

			int rowIndex ;

			int startRow	= dataRow_Start ;
			int endRow		= this.Row ;

			var rowIndices = new List<int>() ;

			for( rowIndex  = startRow ; rowIndex <= endRow ; rowIndex ++ )
			{
				isAvailable	= true ;
				exist		= false ;

				foreach( var member in members )
				{
					if( member.ColumnIndices == null || member.ColumnIndices.Count == 0 )
					{
						// 基本的にありえないが保険
						continue ;
					}

					//--------------------------------

					// 文字列以外の行で検査する
					if( member.PrimitiveTypeCode != TypeCode.String )
					{
						// 格納すべき値がある
						columnIndex = member.ColumnIndices[ 0 ] ;

						check = this[ rowIndex, columnIndex ].ToString() ;

						if( string.IsNullOrEmpty( check ) == false )
						{
							// 文字列は存在する
							if( check[ 0 ] == '#' )
							{
								// この行は無効となる
								isAvailable = false ;

								// 次のロウ(行)へ
								break ;
							}
						}
					}
					else
					{
						columnIndex = member.ColumnIndices[ 0 ] ;

						if( string.IsNullOrEmpty( this[ rowIndex, columnIndex ].Value ) == false )
						{
							// 文字列カラムに何か文字列が入っている
							exist = true ;

							// 次のロウ(行)へ
							break ;
						}
					}

					if( exist == false )
					{
						if
						(
							member.PrimitiveTypeCode != TypeCode.String &&
							member.IsNullable == false
						)
						{
							// String でも Nullable でもないカラムが全て空白であった場合、
							// それらは 0 (default 値) とはせず、レコード自体を無効とする
							// しかし String が空で無い場合は有効とする

							int i ;
							for( i  = 0 ; i <  member.ColumnIndices.Count ; i ++ )
							{
								columnIndex = member.ColumnIndices[ 0 ] ;

								check = this[ rowIndex, columnIndex ].ToString() ;

								if( string.IsNullOrEmpty( check ) == false )
								{
									// 空ではないセルを発見した
									exist = true ;

									// 次のロウ(行)へ
									break ;
								}
							}
						}
					}
				}

				if( isAvailable == true && exist == false )
				{
					// String でも Nullable でもないカラムが、全て空文字だった
					isAvailable  = false ;
				}

				if( isAvailable == true )
				{
					// この行は有効
					rowIndices.Add( rowIndex ) ;
				}
			}

			return rowIndices ;
		}

		//-----------------------------------------------------------

		/// 指定したクラスにデシリアライズする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		public T Deserialize<T>( int nameRow, int dataRow, int validationRow, int dataColumn_Start = 1 ) where T : class
		{
			var records = Deserialize( typeof( T ), nameRow, dataRow, validationRow, dataColumn_Start ) ;
			if( records == null )
			{
				return default ;
			}

			return records as T ;
		}

		/// <summary>
		/// 指定したクラスのリストにデシリアライズする
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="nameRow"></param>
		/// <param name="dataRow_Start"></param>
		/// <param name="validationRow"></param>
		/// <param name="dataColumn_Start"></param>
		/// <returns></returns>
		public IList<System.Object> DeserializeToList( Type objectType, int nameRow, int dataRow_Start, int validationRow, int dataColumn_Start = 1 )
		{
			// リスト
			var listObject = ( IList )Deserialize( objectType, true, nameRow, dataRow_Start, validationRow, dataColumn_Start ) ;

			return listObject.Cast<System.Object>().ToList() ;
		}

		/// <summary>
		/// 指定したクラスにデシリアライズする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		private System.Object Deserialize( Type objectType, int nameRow, int dataRow_Start, int validationRow, int dataColumn_Start = 1 )
		{
			return Deserialize( objectType, false, nameRow, dataRow_Start, validationRow, dataColumn_Start ) ;
		}

		/// <summary>
		/// 指定したクラスにデシリアライズする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		private System.Object Deserialize( Type objectType, bool isOverrideList, int nameRow, int dataRow_Start, int validationRow, int dataColumn_Start = 1 )
		{
			// 型を解析する
			Type type = objectType ;

			bool isArray		= false ;
			bool isList			= false ;

			Type elementType	= null ;

			if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// IsNullable
				type = Nullable.GetUnderlyingType( type ) ;
			}

			//----------------------------------------------------------

			if( isOverrideList == false )
			{
				if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( List<> ) )
				{
					// List
					isList		= true ;

					var types = type.GenericTypeArguments ;
					if( types == null || types.Length != 1 )
					{
						// 複数のジェネリックの場合はスルーされる
						throw new Exception( message:"Only one argument of list type is valid." ) ;
					}

					type = types[ 0 ] ;
					elementType = type ;
				}
				else
				if( type.IsArray == true )
				{
					// Array
					isArray		= true ;

					type = type.GetElementType() ;
					elementType = type ;
				}
			}
			else
			{
				isList = true ;

				elementType = type ;

				objectType = typeof( List<System.Object> ) ;
			}

			//----------------------------------

			if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// IsNullable
				type = Nullable.GetUnderlyingType( type ) ;
			}

			if( Type.GetTypeCode( type ) != TypeCode.Object )
			{
				// オブジェクト以外は不可
				return null ;
			}

			//----------------------------------------------------------

			// メンバー情報を取得する
			var members = GetMemberDefinitions( type ) ;

			// 各メンバーに対応するカラムを適合させる
			FetchColumns( members, nameRow, dataColumn_Start ) ;

			// 有効なデータ行を取得する
			var rowIndices = GetAvailableRows( members, dataRow_Start ) ;

			//----------------------------------------------------------

			int rowIndex ;
			int columnIndex, ri, rl, ei, el ;

			//----------------------------------------------------------
			// バリデーションによるデフォルト値があれば反映させる

			if( validationRow >  0 )
			{
				rowIndex = validationRow ;

				foreach( var member in members )
				{
					if( member.ColumnIndices != null )
					{
						// 格納すべき値がある

						string validation = string.Empty ;

						if( member.IsList == false )
						{
							// アレイ・リストでない
							columnIndex = member.ColumnIndices[ 0 ] ;

							validation = this.GetString( rowIndex, columnIndex ) ;
						}
						else
						{
							// アレイまたはリストである
							el = member.ColumnIndices.Count ;

							for( ei  = 0 ; ei <  el ; ei ++ )
							{
								columnIndex = member.ColumnIndices[ ei ] ;

								string text = this.GetString( rowIndex, columnIndex ) ;
								if( string.IsNullOrEmpty( text ) == false )
								{
									validation = text ;
								}
							}
						}

						//-------------------------------

						if( string.IsNullOrEmpty( validation ) == false )
						{
							// バリデーションの記述が存在する

							string defaultValue = string.Empty ;

							var rules = Utility.GetArrayValues( validation ) ;
							if( rules != null && rules.Length >  0 )
							{
								foreach( var rule in rules )
								{
									var key = rule.ToLower() ;

									if
									(
										key.Contains( '~' ) == false &&
										key.IndexOf( "Master.".ToLower() ) != 0 &&
										key.IndexOf( "MasterData".ToLower() ) != 0 &&
										key.IndexOf( "Player.".ToLower() ) != 0 &&
										key.IndexOf( "PlayerData".ToLower() ) != 0
									)
									{
										// 有効なデフォルト値指定とみなす
										defaultValue = rule ;
										break ;
									}
								}
							}

							if( string.IsNullOrEmpty( defaultValue ) == false )
							{
								member.DefaultValue = defaultValue ;
							}
						}
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 実際にリフレクションによって値を格納していく

			rl = rowIndices.Count ;

			System.Object records = null ;

			Action<int,System.Object> AddRecord = null ;
			Array array	= null ;
			IList list	= null ;

			if( isArray == true )
			{
				// アレイ
				records = Array.CreateInstance( elementType, rl ) ;
				array = records as System.Array ;

				AddRecord = ( int index, System.Object record ) =>
				{
					array.SetValue( record, index ) ;
				} ;
			}
			else
			if( isList == true )
			{
				// リスト
				records = Activator.CreateInstance( objectType ) ;
				list = records as IList ;

				AddRecord = ( int index, System.Object record ) =>
				{
					list.Add( record ) ;
				} ;
			}

			//----------------------------------

			for( ri  = 0 ; ri <  rl ; ri ++ )
			{
				rowIndex = rowIndices[ ri ] ;

				// １レコードを生成する
//				var record = Activator.CreateInstance( type ) ;
				var record = FormatterServices.GetUninitializedObject( type ) ;

				// １レコードを設定する
				foreach( var member in members )
				{
					if( member.ColumnIndices != null )
					{
						// 格納すべき値がある

						// デフォルト値指定があれば使用する
						string defautValue = member.DefaultValue ;

						if( member.IsList == false )
						{
							// アレイ・リストでない
							columnIndex = member.ColumnIndices[ 0 ] ;
							System.Object value = null ;

							if( member.IsEnum == false )
							{
								// 列挙子でない
								if( member.IsNullable == false )
								{
									// Null許容型でない
									switch( member.PrimitiveTypeCode )
									{
										case TypeCode.Boolean	: value = this.GetBoolean( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Byte		: value = this.GetByte( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.SByte		: value = this.GetSByte( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Char		: value = this.GetChar( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int16		: value = this.GetInt16( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt16	: value = this.GetUInt16( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int32		: value = this.GetInt32( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt32	: value = this.GetUInt32( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int64		: value = this.GetInt64( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt64	: value = this.GetUInt64( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Single	: value = this.GetSingle( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Double	: value = this.GetDouble( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Decimal	: value = this.GetDecimal( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.DateTime	: value = this.GetDateTime( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Object :
											if( member.PrimitiveType == typeof( DateTimeOffset ) )
											{
												value = this.GetDateTimeOffset( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( TimeSpan ) )
											{
												value = this.GetTimeSpan( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( Guid ) )
											{
												value = this.GetGuid( rowIndex, columnIndex, defautValue ) ;
											}
										break ;
									}
								}
								else
								{
									// Null許容型である
									switch( member.PrimitiveTypeCode )
									{
										case TypeCode.Boolean	: value = this.GetBooleanN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Byte		: value = this.GetByteN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.SByte		: value = this.GetSByteN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Char		: value = this.GetCharN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int16		: value = this.GetInt16N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt16	: value = this.GetUInt16N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int32		: value = this.GetInt32N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt32	: value = this.GetUInt32N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int64		: value = this.GetInt64N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt64	: value = this.GetUInt64N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Single	: value = this.GetSingleN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Double	: value = this.GetDoubleN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Decimal	: value = this.GetDecimalN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.DateTime	: value = this.GetDateTimeN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Object :
											if( member.PrimitiveType == typeof( DateTimeOffset ) )
											{
												value = this.GetDateTimeOffsetN( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( TimeSpan ) )
											{
												value = this.GetTimeSpanN( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( Guid ) )
											{
												value = this.GetGuidN( rowIndex, columnIndex, defautValue ) ;
											}
										break ;
									}
								}
							}
							else
							{
								// 列挙子である
								if( member.IsNullable == false )
								{
									// Null許容型でない
									value = this.GetEnum( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
								}
								else
								{
									// Null許容型である
									value = this.GetEnumN( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
								}
							}

							// メンバーの値を格納する
							member.SetValue( record, value ) ;
						}
						else
						{
							// リストである
							el = member.ColumnIndices.Count ;

							IList elements ;
							if( member.Type == typeof( List<> ) || member.Type == typeof( IList<> ) )
							{
								// IL2CPP でも動作する
								elements = Activator.CreateInstance( member.Type ) as IList ;
							}
							else
							{
								// IL2CPP では動作しない
								Type listType = typeof( List<> ).MakeGenericType( member.ElementType ) ;
								elements = Activator.CreateInstance( listType ) as IList ;
							}

							for( ei  = 0 ; ei <  el ; ei ++ )
							{
								columnIndex = member.ColumnIndices[ ei ] ;

								if( this[ rowIndex, columnIndex ].IsArray == false )
								{
									// データそのものは配列型ではない

									if( this[ rowIndex, columnIndex ].IsEmpty == false )
									{
										System.Object value = null ;

										if( member.IsEnum == false )
										{
											// 列挙子でない
											if( member.IsNullable == false )
											{
												// Null許容型でない
												switch( member.PrimitiveTypeCode )
												{
													case TypeCode.Boolean	: value = this.GetBoolean( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Byte		: value = this.GetByte( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.SByte		: value = this.GetSByte( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Char		: value = this.GetChar( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int16		: value = this.GetInt16( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt16	: value = this.GetUInt16( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int32		: value = this.GetInt32( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt32	: value = this.GetUInt32( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int64		: value = this.GetInt64( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt64	: value = this.GetUInt64( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Single	: value = this.GetSingle( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Double	: value = this.GetDouble( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Decimal	: value = this.GetDecimal( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.DateTime	: value = this.GetDateTime( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Object :
														if( member.PrimitiveType == typeof( DateTimeOffset ) )
														{
															value = this.GetDateTimeOffset( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( TimeSpan ) )
														{
															value = this.GetTimeSpan( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( Guid ) )
														{
															value = this.GetGuid( rowIndex, columnIndex, defautValue ) ;
														}
													break ;
												}
											}
											else
											{
												// Null許容型である
												switch( member.PrimitiveTypeCode )
												{
													case TypeCode.Boolean	: value = this.GetBooleanN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Byte		: value = this.GetByteN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.SByte		: value = this.GetSByteN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Char		: value = this.GetCharN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int16		: value = this.GetInt16N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt16	: value = this.GetUInt16N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int32		: value = this.GetInt32N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt32	: value = this.GetUInt32N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int64		: value = this.GetInt64N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt64	: value = this.GetUInt64N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Single	: value = this.GetSingleN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Double	: value = this.GetDoubleN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Decimal	: value = this.GetDecimalN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.DateTime	: value = this.GetDateTimeN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Object :
														if( member.PrimitiveType == typeof( DateTimeOffset ) )
														{
															value = this.GetDateTimeOffsetN( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( TimeSpan ) )
														{
															value = this.GetTimeSpanN( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( Guid ) )
														{
															value = this.GetGuidN( rowIndex, columnIndex, defautValue ) ;
														}
													break ;
												}
											}
										}
										else
										{
											// 列挙子である
											if( member.IsNullable == false )
											{
												// Null許容型でない
												value = this.GetEnum( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
											}
											else
											{
												// Null許容型である
												value = this.GetEnumN( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
											}
										}

										elements.Add( value ) ;
									}
								}
								else
								{
									// データそのものが配列型である

									if( member.IsEnum == false )
									{
										// 列挙子でない
										if( member.IsNullable == false )
										{
											// Null許容型でない
											switch( member.PrimitiveTypeCode )
											{
												case TypeCode.Boolean	: Put( this.GetBooleanArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Byte		: Put( this.GetByteArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.SByte		: Put( this.GetSByteArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Char		: Put( this.GetCharArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Int16		: Put( this.GetInt16Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt16	: Put( this.GetUInt16Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int32		: Put( this.GetInt32Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt32	: Put( this.GetUInt32Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int64		: Put( this.GetInt64Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt64	: Put( this.GetUInt64Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Single	: Put( this.GetSingleArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Double	: Put( this.GetDoubleArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Decimal	: Put( this.GetDecimalArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.String	: Put( this.GetStringArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.DateTime	: Put( this.GetDateTimeArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Object :
													if( member.PrimitiveType == typeof( DateTimeOffset ) )
													{
														Put( this.GetDateTimeOffsetArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( TimeSpan ) )
													{
														Put( this.GetTimeSpanArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( Guid ) )
													{
														Put( this.GetGuidArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
												break ;
											}
										}
										else
										{
											// Null許容型である
											switch( member.PrimitiveTypeCode )
											{
												case TypeCode.Boolean	: Put( this.GetBooleanNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Byte		: Put( this.GetByteNArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.SByte		: Put( this.GetSByteNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Char		: Put( this.GetCharNArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Int16		: Put( this.GetInt16NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt16	: Put( this.GetUInt16NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int32		: Put( this.GetInt32NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt32	: Put( this.GetUInt32NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int64		: Put( this.GetInt64NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt64	: Put( this.GetUInt64NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Single	: Put( this.GetSingleNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Double	: Put( this.GetDoubleNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Decimal	: Put( this.GetDecimalNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.String	: Put( this.GetStringArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.DateTime	: Put( this.GetDateTimeNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Object :
													if( member.PrimitiveType == typeof( DateTimeOffset ) )
													{
														Put( this.GetDateTimeOffsetNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( TimeSpan ) )
													{
														Put( this.GetTimeSpanNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( Guid ) )
													{
														Put( this.GetGuidNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
												break ;
											}
										}
									}
									else
									{
										// 列挙子である
										if( member.IsNullable == false )
										{
											// Null許容型でない
											Put( this.GetEnumArray( rowIndex, columnIndex, member.PrimitiveType, defautValue ), elements ) ;
										}
										else
										{
											// Null許容型である
											Put( this.GetEnumNArray( rowIndex, columnIndex, member.PrimitiveType, defautValue ), elements ) ;
										}
									}
								}
							}

							// メンバーの値を格納する
							member.SetValue( record, member.ConversionList( elements ) ) ;
						}
					}
				}

				//---------------------------------

				// レコードを追加する
				if( AddRecord == null )
				{
					// １レコードのみ
					return record ;
				}
				else
				{
					// アレイ・リスト
					AddRecord( ri, record ) ;
				}
			}

			// アレイ・リスト
			return records  ;
		}

		//-------------------------------------------------------------------------------------------

		// メンバー情報に対応するカラム情報を適合させる
		private void FetchRows( List<MemberDefinition> members, int nameColumn, int dataRow_Start )
		{
			// 行数
			int rowCount = this.Row ;

			int rowIndex ;

			// 有効な行
			var rowIndices = new List<int>() ;

			for( rowIndex = dataRow_Start ; rowIndex <= rowCount ; rowIndex ++ )
			{
				string name = this[ rowIndex, nameColumn ].ToString() ;
				if( string.IsNullOrEmpty( name ) == false )
				{
					if( name[ 0 ] != '#' )
					{
						rowIndices.Add( rowIndex ) ;
					}
				}
			}

			if( rowIndices.Count == 0 )
			{
				// 格納する値が存在しない
				throw new Exception( message:"Row is empty." ) ;
			}

			//----------------------------------

			int i, l = rowIndices.Count, p ;

			foreach( var member in members )
			{
				p = 0 ;
				while( p <  l )
				{
					// メンバー名と一致するか検査する
					for( i  = p ; i <  l ; i ++ )
					{
						rowIndex = rowIndices[ i ] ;

						string name = this[ rowIndex, nameColumn ].ToString() ;

						// 名前の末尾に[]が付いていたら削る
						name = ReduceArrayElement( name ) ;

						// オリジナル名
						if( member.Name == name )
						{
							// 一致した
							break ;
						}

						// アッパーキャメル名
						string upperCamelName = SnakeToCamel( name, isLower:false ) ;
						if( member.Name == upperCamelName )
						{
							// 一致した
							break ;
						}

						// ローワーキャメル名
						string lowerCamelName = SnakeToCamel( name, isLower:true  ) ;
						if( member.Name == lowerCamelName )
						{
							// 一致した
							break ;
						}
					}

					if( i <  l )
					{
						// 格納対象となるカラムが見つかった
						member.ColumnIndices ??= new List<int>() ;
						member.ColumnIndices.Add( rowIndex ) ;

						p = i + 1 ;
					}
					else
					{
						p = i ;
					}
				}
			}
		}

		// 有効な列を取得する
		public List<int> GetAvailableColumns( List<MemberDefinition> members, int dataColumn_Start, int dataColumn_End )
		{
			int rowIndex ;

			string check ;
			bool isAvailable ;
			bool exist ;

			int columnIndex ;

			int startColumn	= dataColumn_Start ;
			int endColumn	= dataColumn_End ;

			var columnIndices = new List<int>() ;

			for( columnIndex  = startColumn ; columnIndex <= endColumn ; columnIndex ++ )
			{
				isAvailable	= true ;
				exist		= false ;

				foreach( var member in members )
				{
					// 文字列以外の行で検査する
					if( member.PrimitiveTypeCode != TypeCode.String && member.ColumnIndices != null )
					{
						// 格納すべき値がある
						rowIndex = member.ColumnIndices[ 0 ] ;

						check = this[ rowIndex, columnIndex ].ToString() ;

						if( string.IsNullOrEmpty( check ) == false )
						{
							// 文字列は存在する
							if( check[ 0 ] == '#' )
							{
								// この行は無効となる
								isAvailable = false ;
								break ;
							}
						}
					}

					if( exist == false )
					{
						if
						(
							member.PrimitiveTypeCode != TypeCode.String &&
							member.IsNullable == false &&
							member.ColumnIndices != null && member.ColumnIndices.Count >  0
						)
						{
							// String でも Nullable でもないカラムが全て空白であった場合、
							// それらは 0 (default 値) とはせず、レコード自体を無効とする

							int i ;
							for( i  = 0 ; i <  member.ColumnIndices.Count ; i ++ )
							{
								rowIndex = member.ColumnIndices[ 0 ] ;

								check = this[ rowIndex, columnIndex ].ToString() ;

								if( string.IsNullOrEmpty( check ) == false )
								{
									// 空ではないセルを発見した
									exist = true ;

									// 次のカラムへ
									break ;
								}
							}
						}
					}
				}

				if( isAvailable == true && exist == false )
				{
					// String でも Nullable でもないカラムが、全て空文字だった
					isAvailable  = false ;
				}

				if( isAvailable == true )
				{
					// この列は有効
					columnIndices.Add( columnIndex ) ;
				}
			}

			return columnIndices ;
		}

		//-----------------------------------------------------------

		/// 指定したクラスのリストにデシリアライズする(横方向)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		public T Deserialize<T>( int nameColumn, int dataColumn_Start, int dataColumn_End, int validationColumn, int dataRow_Start ) where T : class
		{
			var records = Deserialize( typeof( T ), nameColumn, dataColumn_Start, dataColumn_End, validationColumn, dataRow_Start ) ;
			if( records == null )
			{
				return default ;
			}

			return records as T ;
		}

		/// <summary>
		/// 指定したクラスのリストにデシリアライズする
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="nameRow"></param>
		/// <param name="dataRow_Start"></param>
		/// <param name="validationRow"></param>
		/// <param name="dataColumn_Start"></param>
		/// <returns></returns>
		public IList<System.Object> DeserializeToList( Type objectType, int nameColumn, int dataColumn_Start, int dataColumn_End, int validationColumn, int dataRow_Start  )
		{
			// リスト
			var listObject = ( IList )Deserialize( objectType, true, nameColumn, dataColumn_Start, dataColumn_End, validationColumn, dataRow_Start ) ;

			return listObject.Cast<System.Object>().ToList() ;
		}

		/// <summary>
		/// 指定したクラスのリストにデシリアライズする(横方向)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		public System.Object Deserialize( Type objectType, int nameColumn, int dataColumn_Start, int dataColumn_End, int validationColumn, int dataRow_Start )
		{
			return Deserialize( objectType, false, nameColumn, dataColumn_Start, dataColumn_End, validationColumn, dataRow_Start ) ;
		}

		/// <summary>
		/// 指定したクラスのリストにデシリアライズする(横方向)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nameRow"></param>
		/// <param name="dataRow"></param>
		/// <returns></returns>
		private System.Object Deserialize( Type objectType, bool isOverrideList, int nameColumn, int dataColumn_Start, int dataColumn_End, int validationColumn, int dataRow_Start )
		{
			// 型を解析する
			Type type = objectType ;

			bool isArray		= false ;
			bool isList			= false ;

			Type elementType	= null ;

			if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// IsNullable
				type = Nullable.GetUnderlyingType( type ) ;
			}

			//----------------------------------------------------------

			if( isOverrideList == false )
			{
				if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( List<> ) )
				{
					// List
					isList		= true ;

					var types = type.GenericTypeArguments ;
					if( types == null || types.Length != 1 )
					{
						// 複数のジェネリックの場合はスルーされる
						throw new Exception( message:"Only one argument of list type is valid." ) ;
					}

					type = types[ 0 ] ;
					elementType = type ;
				}
				else
				if( type.IsArray == true )
				{
					// Array
					isArray		= true ;

					type = type.GetElementType() ;
					elementType = type ;
				}
			}
			else
			{
				isList = true ;

				elementType = type ;

				objectType = typeof( List<System.Object> ) ;
			}

			//----------------------------------

			if( type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
			{
				// IsNullable
				type = Nullable.GetUnderlyingType( type ) ;
			}

			if( Type.GetTypeCode( type ) != TypeCode.Object )
			{
				// オブジェクト以外は不可
				return null ;
			}

			//----------------------------------------------------------

			// メンバー情報を取得する
			var members = GetMemberDefinitions( type ) ;

			// 各メンバーに対応するカラムを適合させる
			FetchRows( members, nameColumn, dataRow_Start ) ;

			// 有効なデータ列を取得する
			var columnIndices = GetAvailableColumns( members, dataColumn_Start, dataColumn_End ) ;

			//----------------------------------------------------------
			// 有効な行を検査する

			int columnIndex ;
			int rowIndex, ci, cl, ei, el ;

			//----------------------------------------------------------
			// バリデーションによるデフォルト値があれば反映させる

			if( validationColumn >  0 )
			{
				columnIndex = validationColumn ;

				foreach( var member in members )
				{
					if( member.ColumnIndices != null )
					{
						// 格納すべき値がある

						string validation = string.Empty ;

						if( member.IsList == false )
						{
							// リストでない
							rowIndex = member.ColumnIndices[ 0 ] ;

							validation = this.GetString( rowIndex, columnIndex ) ;
						}
						else
						{
							// リストである
							el = member.ColumnIndices.Count ;

							for( ei  = 0 ; ei <  el ; ei ++ )
							{
								rowIndex = member.ColumnIndices[ ei ] ;

								string text = this.GetString( rowIndex, columnIndex ) ;
								if( string.IsNullOrEmpty( text ) == false )
								{
									validation = text ;
								}
							}
						}

						//-------------------------------

						if( string.IsNullOrEmpty( validation ) == false )
						{
							// バリデーションの記述が存在する

							string defaultValue = string.Empty ;

							var rules = Utility.GetArrayValues( validation ) ;
							if( rules != null && rules.Length >  0 )
							{
								foreach( var rule in rules )
								{
									var key = rule.ToLower() ;

									if
									(
										key.Contains( '~' ) == false &&
										key.IndexOf( "Master.".ToLower() ) != 0 &&
										key.IndexOf( "MasterData".ToLower() ) != 0 &&
										key.IndexOf( "Player.".ToLower() ) != 0 &&
										key.IndexOf( "PlayerData".ToLower() ) != 0
									)
									{
										// 有効なデフォルト値指定とみなす
										defaultValue = rule ;
										break ;
									}
								}
							}

							if( string.IsNullOrEmpty( defaultValue ) == false )
							{
								member.DefaultValue = defaultValue ;
							}
						}
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 実際にリフレクションによって値を格納していく

			cl = columnIndices.Count ;

			System.Object records = null ;

			Action<int,System.Object> AddRecord = null ;
			Array array	= null ;
			IList list	= null ;

			if( isArray == true )
			{
				// アレイ
				records = Array.CreateInstance( elementType, cl ) ;
				array = records as System.Array ;

				AddRecord = ( int index, System.Object record ) =>
				{
					array.SetValue( record, index ) ;
				} ;
			}
			else
			if( isList == true )
			{
				// リスト
				records = Activator.CreateInstance( objectType ) ;
				list = records as IList ;

				AddRecord = ( int index, System.Object record ) =>
				{
					list.Add( record ) ;
				} ;
			}

			//----------------------------------

			for( ci  = 0 ; ci <  cl ; ci ++ )
			{
				columnIndex = columnIndices[ ci ] ;

				// １レコードを生成する
//				var record = Activator.CreateInstance( type ) ;
				var record = FormatterServices.GetUninitializedObject( type ) ;

				// １レコードを設定する
				foreach( var member in members )
				{
					if( member.ColumnIndices != null )
					{
						// 格納すべき値がある

						// デフォルト値指定があれば使用する
						string defautValue = member.DefaultValue ;

						if( member.IsList == false )
						{
							// リストでない
							rowIndex = member.ColumnIndices[ 0 ] ;
							System.Object value = null ;

							if( member.IsEnum == false )
							{
								// 列挙子でない
								if( member.IsNullable == false )
								{
									// Null許容型でない
									switch( member.PrimitiveTypeCode )
									{
										case TypeCode.Boolean	: value = this.GetBoolean( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Byte		: value = this.GetByte( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.SByte		: value = this.GetSByte( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Char		: value = this.GetChar( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int16		: value = this.GetInt16( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt16	: value = this.GetUInt16( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int32		: value = this.GetInt32( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt32	: value = this.GetUInt32( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int64		: value = this.GetInt64( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt64	: value = this.GetUInt64( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Single	: value = this.GetSingle( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Double	: value = this.GetDouble( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Decimal	: value = this.GetDecimal( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.DateTime	: value = this.GetDateTime( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Object :
											if( member.PrimitiveType == typeof( DateTimeOffset ) )
											{
												value = this.GetDateTimeOffset( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( TimeSpan ) )
											{
												value = this.GetTimeSpan( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( Guid ) )
											{
												value = this.GetGuid( rowIndex, columnIndex, defautValue ) ;
											}
										break ;
									}
								}
								else
								{
									// Null許容型である
									switch( member.PrimitiveTypeCode )
									{
										case TypeCode.Boolean	: value = this.GetBooleanN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Byte		: value = this.GetByteN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.SByte		: value = this.GetSByteN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Char		: value = this.GetCharN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int16		: value = this.GetInt16N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt16	: value = this.GetUInt16N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int32		: value = this.GetInt32N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt32	: value = this.GetUInt32N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Int64		: value = this.GetInt64N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.UInt64	: value = this.GetUInt64N( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Single	: value = this.GetSingleN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Double	: value = this.GetDoubleN( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.Decimal	: value = this.GetDecimalN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
										case TypeCode.DateTime	: value = this.GetDateTimeN( rowIndex, columnIndex, defautValue )	; break ;
										case TypeCode.Object :
											if( member.PrimitiveType == typeof( DateTimeOffset ) )
											{
												value = this.GetDateTimeOffsetN( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( TimeSpan ) )
											{
												value = this.GetTimeSpanN( rowIndex, columnIndex, defautValue ) ;
											}
											else
											if( member.PrimitiveType == typeof( Guid ) )
											{
												value = this.GetGuidN( rowIndex, columnIndex, defautValue ) ;
											}
										break ;
									}
								}
							}
							else
							{
								// 列挙子である
								if( member.IsNullable == false )
								{
									// Null許容型でない
									value = this.GetEnum( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
								}
								else
								{
									// Null許容型である
									value = this.GetEnumN( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
								}
							}

							// メンバーの値を格納する
							member.SetValue( record, value ) ;
						}
						else
						{
							// リストである
							el = member.ColumnIndices.Count ;

							IList elements ;
							if( member.Type == typeof( List<> ) || member.Type == typeof( IList<> ) )
							{
								// IL2CPP でも動作する
								elements = Activator.CreateInstance( member.Type ) as IList ;
							}
							else
							{
								// IL2CPP では動作しない
								Type listType = typeof( List<> ).MakeGenericType( member.ElementType ) ;
								elements = Activator.CreateInstance( listType ) as IList ;
							}

							for( ei  = 0 ; ei <  el ; ei ++ )
							{
								rowIndex = member.ColumnIndices[ ei ] ;

								if( this[ rowIndex, columnIndex ].IsArray == false )
								{
									// データそのものは配列型ではない

									if( this[ rowIndex, columnIndex ].IsEmpty == false )
									{
										System.Object value = null ;

										if( member.IsEnum == false )
										{
											// 列挙子でない
											if( member.IsNullable == false )
											{
												// Null許容型でない
												switch( member.PrimitiveTypeCode )
												{
													case TypeCode.Boolean	: value = this.GetBoolean( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Byte		: value = this.GetByte( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.SByte		: value = this.GetSByte( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Char		: value = this.GetChar( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int16		: value = this.GetInt16( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt16	: value = this.GetUInt16( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int32		: value = this.GetInt32( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt32	: value = this.GetUInt32( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int64		: value = this.GetInt64( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt64	: value = this.GetUInt64( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Single	: value = this.GetSingle( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Double	: value = this.GetDouble( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Decimal	: value = this.GetDecimal( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.DateTime	: value = this.GetDateTime( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Object :
														if( member.PrimitiveType == typeof( DateTimeOffset ) )
														{
															value = this.GetDateTimeOffset( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( TimeSpan ) )
														{
															value = this.GetTimeSpan( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( Guid ) )
														{
															value = this.GetGuid( rowIndex, columnIndex, defautValue ) ;
														}
													break ;
												}
											}
											else
											{
												// Null許容型である
												switch( member.PrimitiveTypeCode )
												{
													case TypeCode.Boolean	: value = this.GetBooleanN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Byte		: value = this.GetByteN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.SByte		: value = this.GetSByteN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Char		: value = this.GetCharN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int16		: value = this.GetInt16N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt16	: value = this.GetUInt16N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int32		: value = this.GetInt32N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt32	: value = this.GetUInt32N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Int64		: value = this.GetInt64N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.UInt64	: value = this.GetUInt64N( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Single	: value = this.GetSingleN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Double	: value = this.GetDoubleN( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.Decimal	: value = this.GetDecimalN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.String	: value = this.GetString( rowIndex, columnIndex, defautValue )		; break ;
													case TypeCode.DateTime	: value = this.GetDateTimeN( rowIndex, columnIndex, defautValue )	; break ;
													case TypeCode.Object :
														if( member.PrimitiveType == typeof( DateTimeOffset ) )
														{
															value = this.GetDateTimeOffsetN( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( TimeSpan ) )
														{
															value = this.GetTimeSpanN( rowIndex, columnIndex, defautValue ) ;
														}
														else
														if( member.PrimitiveType == typeof( Guid ) )
														{
															value = this.GetGuidN( rowIndex, columnIndex, defautValue ) ;
														}
													break ;
												}
											}
										}
										else
										{
											// 列挙子である
											if( member.IsNullable == false )
											{
												// Null許容型でない
												value = this.GetEnum( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
											}
											else
											{
												// Null許容型である
												value = this.GetEnumN( rowIndex, columnIndex, member.PrimitiveType, defautValue ) ;
											}
										}

										elements.Add( value ) ;
									}
								}
								else
								{
									// データそのものが配列型である

									if( member.IsEnum == false )
									{
										// 列挙子でない
										if( member.IsNullable == false )
										{
											// Null許容型でない
											switch( member.PrimitiveTypeCode )
											{
												case TypeCode.Boolean	: Put( this.GetBooleanArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Byte		: Put( this.GetByteArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.SByte		: Put( this.GetSByteArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Char		: Put( this.GetCharArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Int16		: Put( this.GetInt16Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt16	: Put( this.GetUInt16Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int32		: Put( this.GetInt32Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt32	: Put( this.GetUInt32Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int64		: Put( this.GetInt64Array( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.UInt64	: Put( this.GetUInt64Array( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Single	: Put( this.GetSingleArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Double	: Put( this.GetDoubleArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Decimal	: Put( this.GetDecimalArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.String	: Put( this.GetStringArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.DateTime	: Put( this.GetDateTimeArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Object :
													if( member.PrimitiveType == typeof( DateTimeOffset ) )
													{
														Put( this.GetDateTimeOffsetArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( TimeSpan ) )
													{
														Put( this.GetTimeSpanArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( Guid ) )
													{
														Put( this.GetGuidArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
												break ;
											}
										}
										else
										{
											// Null許容型である
											switch( member.PrimitiveTypeCode )
											{
												case TypeCode.Boolean	: Put( this.GetBooleanNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Byte		: Put( this.GetByteNArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.SByte		: Put( this.GetSByteNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Char		: Put( this.GetCharNArray( rowIndex, columnIndex, defautValue ), elements )		; break ;
												case TypeCode.Int16		: Put( this.GetInt16NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt16	: Put( this.GetUInt16NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int32		: Put( this.GetInt32NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt32	: Put( this.GetUInt32NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Int64		: Put( this.GetInt64NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.UInt64	: Put( this.GetUInt64NArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Single	: Put( this.GetSingleNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Double	: Put( this.GetDoubleNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Decimal	: Put( this.GetDecimalNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.String	: Put( this.GetStringArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.DateTime	: Put( this.GetDateTimeNArray( rowIndex, columnIndex, defautValue ), elements )	; break ;
												case TypeCode.Object :
													if( member.PrimitiveType == typeof( DateTimeOffset ) )
													{
														Put( this.GetDateTimeOffsetNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( TimeSpan ) )
													{
														Put( this.GetTimeSpanNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
													else
													if( member.PrimitiveType == typeof( Guid ) )
													{
														Put( this.GetGuidNArray( rowIndex, columnIndex, defautValue ), elements ) ;
													}
												break ;
											}
										}
									}
									else
									{
										// 列挙子である
										if( member.IsNullable == false )
										{
											// Null許容型でない
											Put( this.GetEnumArray( rowIndex, columnIndex, member.PrimitiveType, defautValue ), elements ) ;
										}
										else
										{
											// Null許容型である
											Put( this.GetEnumNArray( rowIndex, columnIndex, member.PrimitiveType, defautValue ), elements ) ;
										}
									}
								}
							}

							// メンバーの値を格納する
							member.SetValue( record, member.ConversionList( elements ) ) ;
						}
					}
				}

				//---------------------------------

				// レコードを追加する
				if( AddRecord == null )
				{
					// １レコードのみ
					return record ;
				}
				else
				{
					// アレイ・リスト
					AddRecord( ci, record ) ;
				}
			}

			// アレイ・リスト
			return records  ;
		}

		//-----------------------------------------------------------

		// 任意の型の配列を System.Object の配列にキャスト格納する
		private static void Put<T>( T[] values, IList elements )
		{
			if( values == null || values.Length == 0 )
			{
				return ;
			}

			//----------------------------------

			foreach( var value in values )
			{
				elements.Add( value ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストを取得する
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.ToString( ',', "\n" ) ;
		}

		/// <summary>
		/// ＣＳＶテキストを取得する
		/// </summary>
		/// <returns></returns>
		public string ToString( char separatorCode, string returnCode )
		{
			if( m_Cells == null || m_Cells.Count == 0 )
			{
				return string.Empty ;
			}

			//----------------------------------------------------------

			var sb = new ExStringBuilder() ;

			int rowIndex, columnIndex ;

			// 実際のデータ化
			for( rowIndex  = 1 ; rowIndex <= Row ; rowIndex ++ )
			{
				for( columnIndex  = 1 ; columnIndex <= Column ; columnIndex ++ )
				{
					sb += Escape( this[ rowIndex, columnIndex ].Value, separatorCode, returnCode ) ;

					if( columnIndex <  Column  )
					{
						sb += separatorCode ;
					}
				}

				sb += returnCode ;	// 改行を環境ごとのものにする(C#での\nはLF=x0A)
			}

			return sb.ToString() ;
		}

		// 値を必要に応じてエスケープする
		// 区切り記号が入ると "..." 囲いが追加される
		// 改行が入ると "..." 囲いが追加される
		// " が入ると "..." 囲いが追加され " は "" にエスケープされる
		private static string Escape( string text, char sepataterCode, string returnCode )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return string.Empty ;
			}

			// 改行コード
			int ri, rl = returnCode.Length ;

			int i, l = text.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( text[ i ] == '"' )
				{
					// "
					break ;
				}
				else
				if( text[ i ] == sepataterCode )
				{
					// 区切記号判定
					break ;	// エスケープが必要
				}
				else
				if( i <= ( l - rl ) )
				{
					// 改行記号判定
					for( ri  = 0 ; ri <  rl ; ri ++ )
					{
						if( text[ i + ri ] != returnCode[ ri ] )
						{
							break ;
						}
					}

					if( ri == rl )
					{
						// 改行にヒットした
						break ;
					}
				}
			}

			if( i <  l )
			{
				// エスケープが必要

				var sb = new ExStringBuilder() ;
				sb += "\"" ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( text[ i ] == '"' )
					{
						// ２つになる
						sb += "\"\"" ;
					}
					else
					{
						sb += text[ i ] ;
					}
				}
				sb += "\"" ;

				text = sb.ToString() ;
			}

			return text ;
		}

		//-------------------------------------------------------------------------------------------

		// 名前の末尾に [] が付いてたら削る
		private static string ReduceArrayElement( string name )
		{
			if( string.IsNullOrEmpty( name ) == true )
			{
				// 無効な文字列
				return name ;
			}

			//---------------------------------

			if( name[ ^1 ] == ']' )
			{
				int i = name.IndexOf( "[" ) ;
				if( i >= 0 )
				{
					// カッコがある
					name = name[ 0..i ] ;
				}
			}

			return name ;
		}

		// 名前がスネーク表記であればキャメル表記に変えたものを取得する
		private static string SnakeToCamel( string name, bool isLower )
		{
			if( string.IsNullOrEmpty( name ) == true )
			{
				return string.Empty ;
			}

			// 一旦全て小文字にする
			name = name.ToLower() ;

			int i, l = name.Length ;
			char[] ca = new char[ l ] ;
			char c ;

			int p = 0 ;

			bool isSecondary = false ;
			bool isUnderLine = false ;

			int upper = ( int )( 'A' - 'a' ) ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = name[ i ] ;

				if( isUnderLine == false )
				{
					// １つ前がアンダーラインではない

					if( c == '_' )
					{
						if( isSecondary == false )
						{
							// 最初がアンダーラインの場合は出力する

							ca[ p ] = c ;
							p ++ ;

							isSecondary = true ;
						}
						else
						if( i <  ( l - 1 ) )
						{
							char nc = name[ i + 1 ] ;
							if( ( nc >= 'a' && nc <= 'z' ) == false )
							{
								// 次の文字はアルファベットではないのでアンダーラインは出力する
								ca[ p ] = c ;
								p ++ ;
							}
						}

						isUnderLine = true ;
					}
					else
					{
						if( isSecondary == false )
						{
							// 一番最初のコード
							if( c >= 'a' && c <= 'z' && isLower == false )
							{
								// アッパー化する
								c = ( char )( ( int )c + upper ) ;
							}

							isSecondary = true ;
						}

						ca[ p ] = c ;
						p ++ ;
					}
				}
				else
				{
					// １つ前がアンダーラインである

					if( c == '_' )
					{
						ca[ p ] = c ;	// そのままアンダーラインを出力する( __ => _ )
						p ++ ;
					}
					else
					{
						// アンダーライン直後の英文字
						if( c >= 'a' && c <= 'z' )
						{
							// アッパー化する
							c = ( char )( ( int )c + upper ) ;
						}

						ca[ p ] = c ;
						p ++ ;

						isUnderLine = false ;	// アンダーライン解除
					}
				}
			}
					
			return new string( ca, 0, p ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 対象メンバーの管理用のクラス
		public class MemberDefinition
		{
			/// <summary>
			/// 識別名
			/// </summary>
			public string			Name ;

			//----------------------------------------------------------

			/// <summary>
			/// タイプ(Array Nullable も含まれるオリジナルの型)
			/// </summary>
			public Type				Type ;

			/// <summary>
			/// フィールドの場合のインスタンス
			/// </summary>
			public FieldInfo		Field ;

			/// <summary>
			/// プロパティの場合のインスタンス
			/// </summary>
			public PropertyInfo		Property ;

			//--------------

			/// <summary>
			/// リストかどうか
			/// </summary>
			public bool				IsList ;

			//----------------------------------------------------------

			/// <summary>
			/// 列挙子かどうか
			/// </summary>
			public bool				IsEnum ;

			/// <summary>
			/// 要素タイプ
			/// </summary>
			public Type				ElementType ;

			/// <summary>
			/// Null許容型かどうか
			/// </summary>
			public bool				IsNullable ;

			/// <summary>
			/// List Array Nullable を除外した型
			/// </summary>
			public Type				PrimitiveType ;

			/// <summary>
			/// List Array Nullable を除外した型のコード
			/// </summary>
			public TypeCode			PrimitiveTypeCode ;

			//----------------------------------------------------------

			/// <summary>
			/// 格納対象カラムのインデックス値
			/// </summary>
			public List<int>		ColumnIndices ;

			//----------------------------------------------------------

			/// <summary>
			/// デフォルト値指定がある場合
			/// </summary>
			public string			DefaultValue ;


			//----------------------------------------------------------

			/// <summary>
			/// メンバーへの値の設定
			/// </summary>
			public Action<System.Object,System.Object> SetValue ;

			// フィールドに値を設定する
			private void SetValueToField( System.Object target, System.Object value )
			{
				Field.SetValue( target, value ) ;
			}

			// プロパティに値を設定する
			private void SetValueToProperty( System.Object target, System.Object value )
			{
				Property.SetValue( target, value ) ;
			}

			/// <summary>
			/// メンバーへの値を設定するコールバックを設定する
			/// </summary>
			public void SetValueCallback()
			{
				if( Field != null )
				{
					SetValue = SetValueToField ;
				}
				else
				if( Property != null )
				{
					SetValue = SetValueToProperty ;
				}
			}

			//--------------

			/// <summary>
			/// リストの変換処理
			/// </summary>
			public Func<IList,System.Object> ConversionList ;

			/// <summary>
			/// リストの変換タイプ
			/// </summary>
			public enum ListConversionTypes
			{
				None,
				List,
				HashSet,
				ImmutableList,
				ImmutableHashSet,
				Array,
			}

			/// <summary>
			/// リストの変換方式を設定する
			/// </summary>
			/// <param name="listConversionType"></param>
			public void ConversionListCallback( ListConversionTypes listConversionType )
			{
				switch( listConversionType )
				{
					case ListConversionTypes.List :
						ConversionList = ConversionListToList ;
					break ;

					case ListConversionTypes.HashSet :
						ConversionList = ConversionListToHashSet ;
					break ;

					//--------------------------------------------------------
					// Immutable パッケージの追加が必要
#if IMMUTABLE_ENABLED
					case ListConversionTypes.ImmutableList :
						ConversionList = ConversionListToImmutableList ;
					break ;

					case ListConversionTypes.ImmutableHashSet :
						ConversionList = ConversionListToImmutableHashSet ;
					break ;
#endif
					//--------------------------------------------------------

					case ListConversionTypes.Array :
						ConversionList = ConversionListToArray ;
					break ;
				}
			}

			// List
			private System.Object ConversionListToList( IList list )
			{
				// そのまま使用できる
				return list ;
			}

			// HashSet
			private System.Object ConversionListToHashSet( IList list )
			{
				var hashSetType= typeof( HashSet<> ).MakeGenericType( ElementType ) ;
				var hashSet = Activator.CreateInstance( hashSetType ) ;
				var setValueMethod = hashSetType.GetMethod( "UnionWith" ) ;

				setValueMethod.Invoke( hashSet, new[]{ list } ) ;

				return hashSet ;
			}

			//----------------------------------------------------------
			// Immutable パッケージの追加が必要
#if IMMUTABLE_ENABLED
			// ImmutableList
			private System.Object ConversionListToImmutableList( IList list )
			{
				var listType = typeof( ImmutableList<> ).MakeGenericType( ElementType ) ;
				var listEmptyMethod = listType.GetField( "Empty", BindingFlags.Static | BindingFlags.Public ) ;
				var listEmpty = listEmptyMethod.GetValue( null ) ;
				var setValueMethod = listType.GetMethod( "Union" ) ;

				return setValueMethod.Invoke( listEmpty, new[]{ list } ) ;
			}

			// ImmutableHashSet
			private System.Object ConversionListToImmutableHashSet( IList list )
			{
				var hashSetType = typeof( ImmutableHashSet<> ).MakeGenericType( ElementType ) ;
				var hashSetEmptyMethod = hashSetType.GetField( "Empty", BindingFlags.Static | BindingFlags.Public ) ;
				var hashSetEmpty = hashSetEmptyMethod.GetValue( null ) ;
				var setValueMethod = hashSetType.GetMethod( "Union" ) ;

				return setValueMethod.Invoke( hashSetEmpty, new[]{ list } ) ;
			}
#endif
			//----------------------------------------------------------

			// Array
			private System.Object ConversionListToArray( IList list )
			{
				// リストをアレイにコンバートする
				int ai, al = list.Count ;
				var array = Array.CreateInstance( ElementType, al ) ;
				for( ai  = 0 ; ai <  al ; ai ++ )
				{
					array.SetValue( list[ ai ], ai ) ;
				}
				return array ;
			}
		}

		//-------------------------------------------------------------------------------------------

		public class Utility
		{
			/// <summary>
			/// 要素を分解して返す
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static string[] GetArrayValues( string value )
			{
				if( string.IsNullOrEmpty( value ) == true )
				{
					return null ;
				}

				//---------------------------------

				var values = new List<string>() ;

				// 最初に " があるかないかで処理が変わる
				// ※この " は、CSV のフォーマットの " では無い事に注意する

				// " が無い場合は、" と , はエスケープする必要がある
				// , がある場合は、必ず前後に " が存在する

				// 文字列にスペースを加えたい場合は前後に " を記述する
				// " あいうえお ", " かきくけこ ", " さしすせそ "

				// 文字列に ,(カンマ) を含めたいケース
				// "あ,い,う,え,お"
				// 　または
				// あ\,い\,う\,え\,お

				// 文字列に "(ダブルクォート) を含めたいケース
				// "あ\"い\"う\"え\"お"
				// 　または
				// あ\"い\"う\"え\"お

				var	sb = new ExStringBuilder() ;

				bool isEntity = false ;
				bool isDouble = false ;
				bool isInside = false ;
				bool isEscape = false ;
				char c ;

				int i, l = value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ i ] ;

					if( isEntity == false )
					{
						// 要素の外側
						if( c == '"' )
						{
							// ダブルクォートで始まる文字列(文字の内部にスペース・タブ・リターンが含まれるケース)
							isEntity = true ;
							isDouble = true ;
							isInside = true ;
							isEscape = false ;

							sb.Clear() ;
						}
						else
						if( c != ' '&& c != '\t' && c != '\n' )
						{
							// ダブルクォートでは無い文字で始まる文字列(スペース・タブ・リターンではない)
							isEntity = true ;
							isDouble = false ;
							isEscape = false ;

							sb.Clear() ;

							// 最初の文字
							sb += c ;
						}
						else
						if( c == ',' || c == '&' )
						{
							// 空文字で次の文字列に進む
							values.Add( string.Empty ) ;
						}
					}
					else
					{
						// 要素の内側

						if( isDouble == true )
						{
							if( isInside == true )
							{
								// ダブルクォートで始まる文字列
								if( isEscape == false )
								{
									// エスケープ中ではない

									if( c == '\\' )
									{
										// エスケープになる
										isEscape = true ;
									}
									else
									if( c == '"' )
									{
										// 文字列終了

										values.Add( sb.ToString() ) ;

										// カンマ出現待ち
										isInside = false ;
									}
									else
									{
										// 文字を追加する
										sb += c ;
									}
								}
								else
								{
									// エスケープ中である

									// 文字を追加する
									sb += c ;	// 文字列を追加する

									// エスケープを解除する
									isEscape = false ;
								}
							}
							else
							{
								if( c == ',' || c == '&' )
								{
									// 次の要素へ
									isEntity = false ;
								}
								else
								if( c != ' '&& c != '\t' && c != '\n' )
								{
									// 不正な文字列
									isEntity = false ;
									break ;
								}

								// スペース・タブ・リターンは無視される
							}
						}
						else
						{
							// ダブルクォートでは無い文字で始まる文字列
							if( isEscape == false )
							{
								// エスケープ中ではない

								if( c == '\\' )
								{
									// エスケープになる
									isEscape = true ;
								}
								else
								if( c == ',' || c == '&' )
								{
									// 文字列終了

									string entity = sb.ToString() ;

									// 末尾のスペース・タブ・リターンがあれば削除する
									entity = entity.TrimEnd( ' ', '\t', '\n' ) ;

									values.Add( entity ) ;

									// 要素終了
									isEntity = false ;
								}
								else
								{
									// 文字を追加する
									sb += c ;
								}
							}
							else
							{
								// エスケープ中である

								// 文字を追加する
								sb += c ;	// 文字列を追加する

								// エスケープを解除する
								isEscape = false ;
							}
						}
					}
				}

				//--------------------------------

				if( isEntity == true )
				{
					if( isDouble == false )
					{
						// ダブルクォートでは無い文字で始まる文字列で終わった

						string entity = sb.ToString() ;

						// 末尾のスペース・タブ・リターンがあれば削除する
						entity = entity.TrimEnd( ' ', '\t', '\n' ) ;

						values.Add( entity ) ;
					}
				}

				return values.ToArray() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		private static readonly HashAlgorithm m_HashAlgorithm = SHA256.Create() ;

		// ハッシュ値を取得する
		private static byte[] ComputeHash( byte[] data )
		{
			// Ｅｘｃｅｌはロックされるのできちんとファイルオープンで読む必要がある
			try
			{
				return m_HashAlgorithm.ComputeHash( data ) ;
			}
			catch( Exception e )
			{
				Console.WriteLine( e.ToString() ) ;
				throw ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// StringBuilder 機能拡張ラッパークラス(メソッド拡張ではない)
		/// </summary>
		public class ExStringBuilder
		{
			private readonly StringBuilder m_StringBuilder ;

			public ExStringBuilder()
			{
				m_StringBuilder			= new StringBuilder() ;
			}

			public int Length
			{
				get
				{
					return m_StringBuilder.Length ;
				}
			}

			public int Count
			{
				get
				{
					return m_StringBuilder.Length ;
				}
			}

			public void Clear()
			{
				m_StringBuilder.Clear() ;
			}

			public override string ToString()
			{
				return m_StringBuilder.ToString() ;
			}
		
			public void Append( string s )
			{
				m_StringBuilder.Append( s ) ;
			}

			// これを使いたいがためにラッパークラス化
			public static ExStringBuilder operator + ( ExStringBuilder sb, string s )
			{
				sb.Append( s ) ;
				return sb ;
			}

			// これを使いたいがためにラッパークラス化
			public static ExStringBuilder operator + ( ExStringBuilder sb, char c )
			{
				sb.Append( c.ToString() ) ;
				return sb ;
			}
		}

		//-------------------------------------------------------------------------------------------
#if false
		// コンソールに出力
		private static void Print( string message )
		{
#if UNITY
			UnityEngine.Debug.Log( $"[CsvObject] {message}" ) ;	
#else
			Console.WriteLine( $"[CsvObject] {message}" ) ;
#endif
		}
#endif
		// 警告を出力する
		private static void PrintWarning( string message )
		{
#if UNITY
			// Unity 限定でワーニングを出力する
			if( Application.isPlaying == true )
			{
				UnityEngine.Debug.LogWarning( $"[CsvObject] {message}" ) ;
			}
#endif
		}
	}
}
