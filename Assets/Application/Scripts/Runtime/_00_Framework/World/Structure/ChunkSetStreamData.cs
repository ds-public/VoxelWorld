using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace DBS.World
{
	/// <summary>
	/// チャンクセットのストリーム
	/// </summary>
	public class ChunkSetStreamData
	{
		private byte[]	m_Data ;

		/// <summary>
		/// データ
		/// </summary>
		public byte[]	  Data	=> m_Data ;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="size"></param>
		public ChunkSetStreamData( int size )
		{
			m_Data		= new byte[ size ] ;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="data"></param>
		public ChunkSetStreamData( byte[] data )
		{
			m_Data		= data ;
		}

		/// <summary>
		/// 値を設定する
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public short GetShort( int offset )
		{
			return ( short )( m_Data[ offset ] | ( m_Data[ offset + 1 ] << 8 ) ) ;
		}

		/// <summary>
		/// 値を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="value"></param>
		public void SetShort( int offset, short value )
		{
			m_Data[ offset     ] = ( byte )(   value        & 0xFF ) ;
			m_Data[ offset + 1 ] = ( byte )( ( value >> 8 ) & 0xFF ) ;
		}
	}
}
