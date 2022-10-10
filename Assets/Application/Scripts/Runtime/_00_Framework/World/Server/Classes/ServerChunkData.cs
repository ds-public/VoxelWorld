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
		//-------------------------------------------------------------------------------------------
		// Server 限定

		// 属しているチャンクセット
		private ServerChunkSetData	m_Owner ;

		// チャンクセットのストリーム
		private Packer				m_ChunkSetStream ;

		// チャンクセットストリーム内のこのチャンクの場所
		private int					m_Offset ;

		//-----------------------------------------------------------

		/// <summary>
		/// チャンクセットのストリームを設定する
		/// </summary>
		/// <param name="data"></param>
		public ServerChunkData( ServerChunkSetData owner, Packer chunkSetStream, int offset )
		{
			m_Owner				= owner ;

			m_ChunkSetStream	= chunkSetStream ;
			m_Offset			= offset ;
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
			int offset = m_Offset + ( ( ( bly << 8 ) + ( blz << 4 ) + blx ) << 1 ) ;

			if( m_ChunkSetStream.GetShortFirst( offset ) != bi )
			{
				m_ChunkSetStream.SetShortFirst( bi, offset ) ;

				// チャンクに変更があった
				m_Owner.IsDirty	= true ;
			}
		}

		/// <summary>
		/// ブロックを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="v"></param>
		public void SetBlockFirst( int blx, int blz, int bly, short bi )
		{
			m_ChunkSetStream.SetShortFirst( bi, m_Offset + ( ( ( bly << 8 ) + ( blz << 4 ) + blx ) << 1 ) ) ;
		}

		/// <summary>
		/// ブロックを取得する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public short GetBlock( int blx, int blz, int bly )
		{
			return m_ChunkSetStream.GetShortFirst( m_Offset + ( ( ( bly << 8 ) + ( blz << 4 ) + blx ) << 1 ) ) ;
		}


	}
}
