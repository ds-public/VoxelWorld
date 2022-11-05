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

namespace DSW.World
{
	/// <summary>
	/// 縦方向の６４チャンクを１つにまとめたデータ(この単位で読み書きされる)
	/// </summary>
	public class ClientChunkSetData
	{
		// このチャンクセット共通のデータ

		/// <summary>
		/// チャンク単位のＸ座標
		/// </summary>
		public int X ;

		/// <summary>
		/// チャンク単位のＺ座標
		/// </summary>
		public int Z ;

		/// <summary>
		/// チャンクセットの識別子
		/// </summary>
		public int CsId
		{
			get
			{
				return ( int )( Z << 12 ) | ( int )X ;
			}
		}


		// チャンクセットのストリーム
		private ChunkSetStreamData m_ChunkSetStream ;


		/// <summary>
		/// チャンク配列
		/// </summary>
		public ClientChunkData[] Chunks = new ClientChunkData[ 64 ] ;	// ６４個のチャンク配列(縦1024ブロック)

		// このチャンクセットが現在使用中かどうか
		public bool IsUsing ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="packer"></param>
		public ClientChunkSetData( int csId, byte[] compressedData )
		{
			//----------------------------------------------------------

			X =   csId         & 0x0FFF ;
			Z = ( csId >> 12 ) & 0x0FFF ;

			//----------------------------------

			// 圧縮データを伸長する
			GZipReader gzr = new GZipReader( compressedData ) ;

			// ８バイト＝６４ビットの６４チャンクの有効フラグを取得する
			byte[] work = gzr.Get( 8 ) ;

			ulong flags = ( ulong )(
				( work[ 0 ] <<  0 ) |
				( work[ 1 ] <<  8 ) |
				( work[ 2 ] << 16 ) |
				( work[ 3 ] << 24 ) |
				( work[ 4 ] << 32 ) |
				( work[ 5 ] << 40 ) |
				( work[ 6 ] << 48 ) |
				( work[ 7 ] << 56 ) ) ;


			byte[] chunkSetStream = new byte[ Chunks.Length * 8192 ] ;	// 512KB

			int cy ;
			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				if( ( flags & ( ulong )( 1 << cy ) ) != 0 )
				{
					// このチャンクのデータは存在する
					gzr.Get( chunkSetStream, cy * 8192, 8192 ) ;
				}
			}

			// 伸長終了
			gzr.Close() ;

			//----------------------------------

			// チャンクセットのストリーム
			m_ChunkSetStream = new ChunkSetStreamData( chunkSetStream ) ;

			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				Chunks[ cy ] = new ClientChunkData( X, Z, cy, m_ChunkSetStream, cy * 8192, isEmpty:( ( flags & ( ulong )( 1 << cy ) ) == 0 ) ) ;
			}

			//----------------------------------

			// 使用中のフラグを立てる
			IsUsing = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// チャンクセットが維持領域になった際に属するチャンク群の左右前後上下すべてにチャンクが存在するか確認し無い場合は外側のフェース情報を削る
		/// </summary>
		/// <param name="owner"></param>
		public void CheckChunks( WorldClient owner )
		{
			int cy, cl = Chunks.Length ;

			for( cy  = 0 ; cy <  cl ; cy ++ )
			{
				if( Chunks[ cy ].CanCreate( owner ) == false )
				{
					// 左右前後上下のチャンクが不明になりチャンクのメッシュモデルの状態が不確定になった

					// 外側のフェース情報を削る
					Chunks[ cy ].RemoveOuterBlockFaces() ;
				}
			}
		}
	}
}
