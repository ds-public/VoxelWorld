using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

// 参考
// https://minecraft-ja.gamepedia.com/%E3%83%81%E3%83%A3%E3%83%B3%E3%82%AF
// https://minecraft-ja.gamepedia.com/Chunk%E3%83%95%E3%82%A9%E3%83%BC%E3%83%9E%E3%83%83%E3%83%88

using DBS.World ;

namespace DBS.WorldServerClasses
{
	/// <summary>
	/// ストレージ・サーバーメモリ上でのチャンクデータ
	/// </summary>
	public class ServerChunkData
	{
		// ブロック情報
		private short[,,]	m_Blocks = new short[ 16, 16, 16 ] ;	// x z y

		/// <summary>
		/// ブロック情報
		/// </summary>
		public short[,,]	  Blocks	=> m_Blocks ;

		//-------------------------------------------------------------------------------------------
		// Server 限定

		// チャンク内の状態に変化が生じたかどうか
		private bool		m_IsDirty = true ;

		// 圧縮されたチャンクデータ
		private byte[]		m_PackedData ;

		//-----------------------------------------------------------

		/// <summary>
		/// パックしたデータを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] Pack()
		{
			if( m_IsDirty == false && m_PackedData != null )
			{
				// 変化なし
				return m_PackedData ;
			}

			//----------------------------------------------------------

			var blocks = new Packer() ;

			//--------------

			int bx, bz, by ;

//			short delta = 0 ;
//			short value ;

			for( by  =  0 ; by <  16 ; by ++ )
			{
				for( bz  =  0 ; bz <  16 ; bz ++ )
				{
					for( bx  =  0 ; bx <  16 ; bx ++ )
					{
//						value = m_Blocks[ bx, bz, by ] ;
//						blocks.SetShort( ( short )( value - delta ) ) ;	// 差分を格納する
//						delta = value ;

						blocks.SetShort(  m_Blocks[ bx, bz, by ] ) ;	// 差分を格納する
					}
				}
			}

			//----------------------------------------------------------

			m_PackedData = blocks.GetData() ;

			//----------------------------------------------------------

			m_IsDirty = false ;

			return m_PackedData ;
		}

		/// <summary>
		/// パック済みデータを設定する
		/// </summary>
		/// <param name="data"></param>
		public void SetPackData( byte[] data )
		{
			m_PackedData = data ;
		}

		/// <summary>
		/// パック済みデータを展開する
		/// </summary>
		public void Unpack()
		{
			var blocks = new Packer( m_PackedData ) ;

			//--------------

			int bx, bz, by ;

			for( by  =  0 ; by <  16 ; by ++ )
			{
				for( bz  =  0 ; bz <  16 ; bz ++ )
				{
					for( bx  =  0 ; bx <  16 ; bx ++ )
					{
						m_Blocks[ bx, bz, by ] = blocks.GetShort() ;
					}
				}
			}

			//----------------------------------------------------------

			m_IsDirty = false ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ブロックを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="v"></param>
		public void SetBlock( int blx, int blz, int bly, short bi )
		{
			if( m_Blocks[ blx, blz, bly ] != bi )
			{
				m_Blocks[ blx, blz, bly ]  = bi ;

				// チャンクに変更があった
				m_IsDirty	= true ;
			}
		}

		/// <summary>
		/// ブロックを取得する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public short GetBlock( int x, int z, int y )
		{
			return m_Blocks[ x, z, y ] ;
		}


	}
}
