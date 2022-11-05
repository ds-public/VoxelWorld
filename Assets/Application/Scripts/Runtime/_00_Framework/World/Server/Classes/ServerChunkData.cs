using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

// 参考
// https://minecraft-ja.gamepedia.com/%E3%83%81%E3%83%A3%E3%83%B3%E3%82%AF
// https://minecraft-ja.gamepedia.com/Chunk%E3%83%95%E3%82%A9%E3%83%BC%E3%83%9E%E3%83%83%E3%83%88

using DSW.World ;

namespace DSW.WorldServerClasses
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
		private ChunkSetStreamData	m_ChunkSetStream ;

		// チャンクセットストリーム内のこのチャンクの場所
		private int					m_Offset ;

		// 空ではないブロックの数
		private int					m_SolidBlockCount ;

		/// <summary>
		/// 空ではないブロックの数
		/// </summary>
		public	int					  SolidBlockCount	=> m_SolidBlockCount ;

		//-----------------------------------------------------------

		/// <summary>
		/// チャンクセットのストリームを設定する
		/// </summary>
		/// <param name="data"></param>
		public ServerChunkData( ServerChunkSetData owner, ChunkSetStreamData chunkSetStream, int offset, bool isEmpty )
		{
			m_Owner				= owner ;

			m_ChunkSetStream	= chunkSetStream ;
			m_Offset			= offset ;

			//----------------------------------

			m_SolidBlockCount = 0 ;

			if( isEmpty == false )
			{
				// 空ではないブロックの数をカウントする
				int i ;
				for( i  =    0 ; i <  4096 ; i ++ )
				{
					if( m_ChunkSetStream.GetShort( offset ) != 0 )
					{
						m_SolidBlockCount ++ ;
					}

					offset += 2 ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ブロックを設定する(デフォルト地形生成用)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <param name="v"></param>
		public void SetBlockFirst( int blx, int blz, int bly, short bi )
		{
			m_ChunkSetStream.SetShort( m_Offset + ( ( ( bly << 8 ) + ( blz << 4 ) + blx ) << 1 ), bi ) ;

			m_SolidBlockCount ++ ;
		}

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

			short bio = m_ChunkSetStream.GetShort( offset ) ;
			if( bio != bi )
			{
				m_ChunkSetStream.SetShort( offset, bi ) ;

				if( bio == 0 && bi != 0 )
				{
					m_SolidBlockCount ++ ;
				}
				else
				if( bio != 0 && bi == 0 )
				{
					m_SolidBlockCount -- ;
				}

				// チャンクに変更があった
				m_Owner.IsDirty	= true ;
			}
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
			return m_ChunkSetStream.GetShort( m_Offset + ( ( ( bly << 8 ) + ( blz << 4 ) + blx ) << 1 ) ) ;
		}
	}
}
