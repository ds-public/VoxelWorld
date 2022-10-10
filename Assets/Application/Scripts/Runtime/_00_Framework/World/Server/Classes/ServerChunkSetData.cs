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
	/// 縦方向の６４チャンクを１つにまとめたデータ(この単位で読み書きされる)
	/// </summary>
	public class ServerChunkSetData
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

		/// <summary>
		/// チャンク配列
		/// </summary>
		public ServerChunkData[] Chunks = new ServerChunkData[ 64 ] ;	// ６４個のチャンク配列(縦1024ブロック)

		//-------------------------------------------------------------------------------------------

		// 最新の圧縮状態のデータ化を行っているかどうか
		private bool	m_IsCompressed	= false ;

		// チャンク内の状態に変化が生じたかどうか
		private bool	m_IsDirty		= true ;

		/// <summary>
		/// チャンク内の状態に変化が生じたかどうか
		/// </summary>
		public	bool	  IsDirty
		{
			get
			{
				return m_IsDirty ;
			}
			set
			{
				m_IsDirty = value ;
			}
		}

		// 圧縮されたチャンクセットデータ
		private byte[]		m_CompressedData ;

		// 伸長されたチャンクセットデータ
		private Packer		m_ChunkSetStream ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 空状態のデータを設定する
		/// </summary>
		public void CreateDefault()
		{
			m_CompressedData	= null ;

			m_ChunkSetStream	= new Packer( new byte[ Chunks.Length * 8192 ] ) ;	// 512KB

			//----------------------------------------------------------
			// デフォルトのチャンクセット状態を生成する

			int[,] heightMap = new int[ 16, 16 ] ;	// x z

			float cbx = X * 16 ;
			float cbz = Z * 16 ;

			int maxHeight = -1 ;

			float noiseValue ;

			int blx, bly, blz ;
			for( blz  = 0 ; blz <= 15 ; blz ++ )
			{
				for( blx  = 0 ; blx <= 15 ; blx ++ )
				{
					noiseValue = PerlinNoise.GetValue( ( float )( cbx + blx ) / 16, ( float )( cbz + blz ) / 16 ) ;
					bly = ( int )( ( noiseValue * 8 ) + ( 512 ) ) ;

					heightMap[ blx, blz ] = bly ;

					if( bly >  maxHeight )
					{
						maxHeight = bly ;
					}
				}
			}

			//-------------------------

			int by0, by1 ;

			int cy, hy ;
			for( cy  =  0 ; cy <  Chunks.Length ; cy ++ )
			{
				Chunks[ cy ] = new ServerChunkData( this, m_ChunkSetStream, cy * 8192 ) ;
				
				//---------------------------------

				by0 = cy * 16 ;
				by1 = by0 + 15 ;

				if( by0 >  maxHeight )
				{
					// 全てが空になるチャンク
					continue ;	// それ以上上のチャンクは存在しない
				}

				//---------------------------------

				for( blz  = 0 ; blz <= 15 ; blz ++ )
				{
					for( blx  =  0 ; blx <= 15 ; blx ++ )
					{
						hy = heightMap[ blx, blz ] ;

						if( hy <  by0 )
						{
							// ここのブロックは全て無し(チャンクのインスタンスは null)
							continue ;
						}
						else
						if( hy >  by1 )
						{
							// ここのブロックは全て有り
							hy = 15 ;
						}
						else
						{
							// ここのブロックは一部有り
							hy -= by0 ;
						}

						for( bly  =  0 ; bly <= hy ; bly ++ )
						{
							Chunks[ cy ].SetBlockFirst( blx, blz, bly, 1 ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			m_IsCompressed	= false ;
			m_IsDirty		= false ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ストレージからロードしたデータを設定する
		/// </summary>
		/// <param name="data"></param>
		public void SetCompressedData( byte[] data )
		{
			// 圧縮状態のデータを保持する
			m_CompressedData = data ;

			// 伸長状態のデータを生成する
			Deflate() ;
		}

		/// <summary>
		/// ストレージからロードしたデータを展開する
		/// </summary>
		/// <returns></returns>
		public void Deflate()
		{
			//----------------------------------------------------------

			var packer = new Packer( m_CompressedData ) ;

			// 空読み
			int csId = packer.GetInt() ;

			// 圧縮状態のデータサイズ
			int size = packer.GetInt() ;

			// 圧縮状態のデータを伸長する
			byte[] decompressData = GZip.Decompress( packer.Data, packer.Offest, size ) ;

			//----------------------------------

			m_ChunkSetStream = new Packer( decompressData ) ;

			// チャンク単位でGZIP圧縮をかける(既に圧縮済みならそのままコピーする:無圧縮で8192バイト)
			int cy ;
			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				Chunks[ cy ] = new ServerChunkData( this, m_ChunkSetStream, cy * 8192  ) ;	// １チャンクあたり８ＫＢ
			}

			//----------------------------------------------------------

			// 最新のチャンクセットで圧縮チャックセットデータを生成している
			m_IsDirty = false ;
		}

		/// <summary>
		/// 圧縮状態データを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] Inflate()
		{
			if( m_IsDirty == false && m_IsCompressed == true )
			{
				return m_CompressedData ;	// 既に圧縮済みのチャンクセットデータが生成されている
			}

			//----------------------------------------------------------

			var packer = new Packer() ;

			// チャンクセット識別子を格納する
			packer.SetInt( CsId ) ;

			//----------------------------------

			// 圧縮する
			byte[] compressedData = GZip.Compress( m_ChunkSetStream.Data ) ;
			packer.SetInt( compressedData.Length ) ;
			packer.SetBytes( compressedData ) ;

			m_CompressedData = packer.Data ;

			// 圧縮状態のデータを生成した
			m_IsCompressed = true ;

			//----------------------------------------------------------

			// 最新のチャンクセットで圧縮チャックセットデータを生成している
			m_IsDirty = false ;

			return m_CompressedData ;
		}

		//-----------------------------------------------------------



		//-------------------------------------------------------------------------------------------

		// 現在のクライアントからの参照状態
		private readonly List<string>	m_ClientIds = new List<string>() ;

		/// <summary>
		/// クライアント識別子を参照リストに追加する
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public int AddCliendId( string clientId )
		{
			if( m_ClientIds.Contains( clientId ) == false )
			{
				m_ClientIds.Add( clientId ) ;
			}

			return m_ClientIds.Count ;
		}

		/// <summary>
		/// クライアント識別子を参照リストから削除する
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public int RemoveClientId( string clientId )
		{
			if( m_ClientIds.Contains( clientId ) == true )
			{
				m_ClientIds.Remove( clientId ) ;
			}

			return m_ClientIds.Count ;
		}

		/// <summary>
		/// 参照リストに指定のクライアント識別子が含まれているか確認する
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public bool ContainsClientId( string clientId )
		{
			return m_ClientIds.Contains( clientId ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// チャンクセットに変化があった事を設定する
		/// </summary>
		public void SetDirty()
		{
			m_IsDirty = true ;
		}


	}
}
