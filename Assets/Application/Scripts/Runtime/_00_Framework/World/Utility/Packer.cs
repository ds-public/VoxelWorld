using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;

using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

namespace DBS.World
{
	// リトルエンディアン

	/// <summary>
	/// データのバイナリデータ管理クラス
	/// </summary>
	public class Packer
	{
		private List<byte>	m_Data ;
		private int			m_Offset ;

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		public Packer()
		{
			m_Data = new List<byte>() ;
			m_Offset = 0 ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public Packer( List<byte> data )
		{
			m_Data = data ;
			m_Offset = 0 ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public Packer( List<byte> data, int offset )
		{
			m_Data		= data ;
			m_Offset	= offset ;
		}
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public Packer( byte[] data )
		{
			m_Data = data.ToList() ;
			m_Offset = 0 ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public Packer( byte[] data, int offset )
		{
			m_Data		= data.ToList() ;
			m_Offset	= offset ;
		}


		/// <summary>
		/// データを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] GetData()
		{
			return m_Data.ToArray() ;
		}

		/// <summary>
		/// データ
		/// </summary>
		public byte[] Data	=> m_Data.ToArray() ;

		/// <summary>
		/// オフセット
		/// </summary>
		public int Offest	=> m_Offset ;

		/// <summary>
		/// オフセット位置を増加させる
		/// </summary>
		/// <param name="size"></param>
		public void Skip( int size )
		{
			m_Offset += size ;
		}


		//-----------------------------------

		/// <summary>
		/// バイトデータを入力する
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetByte( byte value )
		{
			m_Data.Add( value ) ;

			return m_Data.Count ;
		}

		/// <summary>
		/// バイトデータ配列を入力する
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public int SetBytes( byte[] values )
		{
			m_Data.AddRange( values ) ;

			return m_Data.Count ;
		}

		/// <summary>
		/// バイトデータを出力する
		/// </summary>
		/// <returns></returns>
		public byte GetByte()
		{
			byte value = m_Data[ m_Offset ] ;
			m_Offset ++ ;

			return value ;
		}

		/// <summary>
		/// バイトデータ配列を出力する
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public byte[] GetBytes( int size )
		{
			byte[] data = new byte[ size ] ;

			Array.Copy( m_Data.ToArray(), m_Offset, data, 0, size ) ;

			m_Offset += size ;

			return data ;
		}

		/// <summary>
		/// ショートインテジャーデータを入力する
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetShort( short value )
		{
			m_Data.Add( ( byte )(   value         & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) & 0xFF ) ) ;

			return m_Data.Count ;
		}

		/// <summary>
		/// ショートインテジャーデータを出力する
		/// </summary>
		/// <returns></returns>
		public short GetShort()
		{
			short value = ( short )(
				( short )( m_Data[ m_Offset + 1 ] <<  8 ) |
				( short )( m_Data[ m_Offset     ]       )
			) ;
			m_Offset += 2 ;

			return value ;
		}

		/// <summary>
		/// インテジャーデータを入力する
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetInt( int value )
		{
			m_Data.Add( ( byte )(   value         & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) & 0xFF ) ) ;

			return m_Data.Count ;
		}

		/// <summary>
		/// インテジャーデータを出力する
		/// </summary>
		/// <returns></returns>
		public int GetInt()
		{
			int value = ( int )(
				( int )( m_Data[ m_Offset + 3 ] << 24 ) |
				( int )( m_Data[ m_Offset + 2 ] << 16 ) |
				( int )( m_Data[ m_Offset + 1 ] <<  8 ) |
				( int )( m_Data[ m_Offset     ]       )
			) ;
			m_Offset += 4 ;

			return value ;
		}


		/// <summary>
		/// ロングインテジャーデータを入力する
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetLong( long value )
		{
			m_Data.Add( ( byte )(   value         & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >>  8 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 16 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 24 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 32 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 40 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 48 ) & 0xFF ) ) ;
			m_Data.Add( ( byte )( ( value >> 56 ) & 0xFF ) ) ;

			return m_Data.Count ;
		}

		/// <summary>
		/// ロングインテジャーデータを出力する
		/// </summary>
		/// <returns></returns>
		public long GetLong()
		{
			long value = ( long )(
				( long )( m_Data[ m_Offset + 7 ] << 56 ) |
				( long )( m_Data[ m_Offset + 6 ] << 48 ) |
				( long )( m_Data[ m_Offset + 5 ] << 40 ) |
				( long )( m_Data[ m_Offset + 4 ] << 32 ) |
				( long )( m_Data[ m_Offset + 3 ] << 24 ) |
				( long )( m_Data[ m_Offset + 2 ] << 16 ) |
				( long )( m_Data[ m_Offset + 1 ] <<  8 ) |
				( long )( m_Data[ m_Offset     ]       )
			) ;
			m_Offset += 8 ;

			return value ;
		}

		/// <summary>
		/// 浮動小数値を入力する
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetFloat( float value )
		{
			m_Data.AddRange( BitConverter.GetBytes( value ) ) ;
			return m_Data.Count ;
		}

		/// <summary>
		/// 浮動小数値を出力する
		/// </summary>
		/// <returns></returns>
		public float GetFloat()
		{
			float value = BitConverter.ToSingle( m_Data.ToArray(), m_Offset ) ;
			m_Offset += 4 ;

			return value ;
		}

		/// <summary>
		/// 文字列を追加する(ただし２５５文字まで)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int SetString( string value, bool ignoreSize = false )
		{
			if( string.IsNullOrEmpty( value ) == true )
			{
				m_Data.Add( 0 ) ;
			}
			else
			{
				byte[] data = Encoding.UTF8.GetBytes( value ) ;
				int length = data.Length ;
				if( length >  255 )
				{
					length  = 255 ;
				}

				// サイズを有効にするかどうか
				if( ignoreSize == false )
				{
					m_Data.Add( ( byte )length ) ;
				}

				m_Data.AddRange( data, 0, length ) ;
			}

			return m_Data.Count ;
		}

		/// <summary>
		/// 文字列を取得する
		/// </summary>
		/// <returns></returns>
		public string GetString( int size = 0 )
		{
			string value ;

			if( size <= 0 )
			{
				// サイズ指定無し
				size = m_Data[ m_Offset ] ;
				m_Offset ++ ;

				value = Encoding.UTF8.GetString( m_Data.ToArray(), m_Offset, size ) ;
				m_Offset += size ;
			}
			else
			{
				// サイズ指定あり
				value = Encoding.UTF8.GetString( m_Data.ToArray(), m_Offset, size ) ;
				m_Offset += size ;
			}

			return value ;
		}
	}
}
