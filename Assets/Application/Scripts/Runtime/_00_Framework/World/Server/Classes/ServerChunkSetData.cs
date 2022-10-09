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

		// チャンク内の状態に変化が生じたかどうか
		private bool	m_IsDirty = true ;

		/// <summary>
		/// チャンク内の状態に変化が生じたかどうか
		/// </summary>
		public	bool	  IsDirty	=> m_IsDirty ;

		// 圧縮されたデータ
		private byte[]	m_PackedData ;

		/// <summary>
		/// ストレージからロードしたデータを設定する
		/// </summary>
		/// <param name="data"></param>
		public void SetPackedData( byte[] data )
		{
			m_PackedData = data ;

			Unpack() ;
		}


		// 現在のクライアントからの参照状態
		private List<string>	m_CliendIds = new List<string>() ;

		/// <summary>
		/// クライアント識別子を参照リストに追加する
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public int AddCliendId( string clientId )
		{
			if( m_CliendIds.Contains( clientId ) == false )
			{
				m_CliendIds.Add( clientId ) ;
			}

			return m_CliendIds.Count ;
		}

		/// <summary>
		/// クライアント識別子を参照リストから削除する
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public int RemoveClientId( string clientId )
		{
			if( m_CliendIds.Contains( clientId ) == true )
			{
				m_CliendIds.Remove( clientId ) ;
			}

			return m_CliendIds.Count ;
		}




		//-----------------------------------------------------------

		/// <summary>
		/// チャンクセットに変化があった事を設定する
		/// </summary>
		public void SetDirty()
		{
			m_IsDirty = true ;
		}

		/// <summary>
		/// パックしたデータを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] Pack()
		{
			if( m_IsDirty == false && m_PackedData != null )
			{
				return m_PackedData ;	// 既に圧縮済みのチャンクセットデータが生成されている
			}

			//----------------------------------------------------------

			var packer = new Packer() ;

			// チャンクセット識別子を格納する
			packer.SetInt( CsId ) ;

			//----------------------------------

			// 全体でGZIP圧縮をかける

			var chunks = new Packer() ;

			// チャンク単位でGZIP圧縮をかける(既に圧縮済みならそのままコピーする:無圧縮で8192バイト)
			int cy ;
			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				if( Chunks[ cy ] == null )
				{
					// 空のチャンク
					chunks.SetShort(    0 ) ;
				}
				else
				{
					// 存在チャンク
					chunks.SetShort(    1 ) ;
					chunks.SetBytes( Chunks[ cy ].Pack() ) ;
				}
			}

			byte[] compressedData = GZip.Compress( chunks.GetData() ) ;
			packer.SetInt( compressedData.Length ) ;
			packer.SetBytes( compressedData ) ;

			m_PackedData = packer.GetData() ;

			//----------------------------------------------------------

			// 最新のチャンクセットで圧縮チャックセットデータを生成している
			m_IsDirty = false ;

			return m_PackedData ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ストレージからロードしたデータを展開する
		/// </summary>
		/// <returns></returns>
		public void Unpack()
		{
			//----------------------------------------------------------

			var packer = new Packer( m_PackedData ) ;

			// 空読み
			int csId = packer.GetInt() ;

			int size = packer.GetInt() ;

			byte[] decompreddedData = GZip.Decompress( packer.Data, packer.Offest, size ) ;

			//----------------------------------

			var chunks = new Packer( decompreddedData ) ;

			// チャンク単位でGZIP圧縮をかける(既に圧縮済みならそのままコピーする:無圧縮で8192バイト)
			int cy ;
			for( cy  = 0 ; cy <  Chunks.Length ; cy ++ )
			{
				size = chunks.GetShort() ;
				if( size == 0 )
				{
					// 空のチャンク
					Chunks[ cy ] = null ;
				}
				else
				{
					// 存在チャンク
					Chunks[ cy ] = new ServerChunkData() ;

					Chunks[ cy ].SetPackData( chunks.GetBytes( 8192 ) ) ;
					Chunks[ cy ].Unpack() ;
				}
			}

			//----------------------------------------------------------

			// 最新のチャンクセットで圧縮チャックセットデータを生成している
			m_IsDirty = false ;
		}

	}
}
