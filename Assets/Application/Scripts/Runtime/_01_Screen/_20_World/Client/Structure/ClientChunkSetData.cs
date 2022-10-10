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

namespace DBS.World
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
		private Packer m_ChunkSetStream ;


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
		public ClientChunkSetData( int csId, byte[] data )
		{
			Packer packer = new Packer( data ) ;

			//----------------------------------------------------------

			X =   csId         & 0x0FFF ;
			Z = ( csId >> 12 ) & 0x0FFF ;

			//----------------------------------

			// 最初の４バイトはチャンクセット識別子
			if( csId != packer.GetInt() )
			{
				Debug.LogWarning( "[CLIENT]チャンクセット識別子に異常あり" ) ;
				return ;
			}

			//----------------------------------

			// 全体でGZIP圧縮がかかっている
			int size = packer.GetInt() ;

			// 圧縮データを伸長する
			byte[] decompressedData = GZip.Decompress( packer.Data, packer.Offest, size ) ;

			// チャンクセットのストリーム
			m_ChunkSetStream = new Packer( decompressedData ) ;

			int cy ;
			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				Chunks[ cy ] = new ClientChunkData( X, Z, cy, m_ChunkSetStream, cy * 8192 ) ;
/*


				size = chunks.GetShort() ;
				if( size >  0 )
				{
					// データが存在する
					Chunks[ cy ] = new ClientChunkData( X, Z, cy, chunks, size ) ;
				}
				else
				{
					// データが存在しない
					Chunks[ cy ] = new ClientChunkData( X, Z, cy, null, 0 ) ;
				}*/
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
