using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using InputHelper ;

using MathHelper ;

using StorageHelper ;

using DBS.WorldServerClasses ;

namespace DBS.World
{
	/// <summary>
	/// サーバー(ストレージ操作)
	/// </summary>
	public partial class WorldServer
	{
		//-------------------------------------------------------------------------------------------

		private string m_CSAT_Path = m_WorldRootPath + "csat.bin" ;

		/// <summary>
		/// チャンクセット全体のストレージ上の管理テーブル
		/// </summary>
		public class ChunkSetAllocation
		{
			public	int		CsId ;

			public	int		BlockOffset ;
			public	int		BlockLength ;

			public	bool	IsEmpty ;
		}

		private List<ChunkSetAllocation>				m_ChunkSetAllocations		= new List<ChunkSetAllocation>() ;
		private Dictionary<int,ChunkSetAllocation>		m_ChunkSetExistAllocations	= new Dictionary<int, ChunkSetAllocation>() ;
		private List<ChunkSetAllocation>				m_ChunkSetEmptyAllocations	= new List<ChunkSetAllocation>() ;


		private int	m_TotalLength ;

		private const int m_DataBlockSize = 4096 ;	// 4KB が最小単位


		//-----------------------------------------------------------

		private string m_CSDB_Path = m_WorldRootPath + "csdb.bin" ;

		private FileStream m_CSDB ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// チャンクセットアロケーションテーブルのロード
		/// </summary>
		private void LoadChunkSetAllocations()
		{
			m_ChunkSetExistAllocations.Clear() ;
			m_ChunkSetEmptyAllocations.Clear() ;

			//----------------------------------

			// int   : 24bit CsId(Z<<12|X)
			//         7bit 使用データブロック数(1～127) ※1データブロックは4KB
			//         1bit 無効化データブロックフラグ(1で無効)

			int offset = 0 ;

			if( StorageAccessor.Exists( m_CSAT_Path ) == StorageAccessor.Target.File )
			{
				byte[] data = StorageAccessor.Load( m_CSAT_Path ) ;

				Packer packer = new Packer( data ) ;

				// 現在のデータブロック数
				int i, l = packer.GetInt() ;

				ChunkSetAllocation record ;
				int value ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					record = new ChunkSetAllocation() ;

					value = packer.GetInt() ;

					record.CsId = value & 0x00FFFFFF ;

					record.BlockOffset	= offset ;
					record.BlockLength	= ( value >> 24 ) % 0x7F ;

					record.IsEmpty		= ( ( value & 0x80000000 ) != 0 ) ;

					//--------------------------------

					m_ChunkSetAllocations.Add( record ) ;

					if( record.IsEmpty == false )
					{
						// 有効データブロック
						m_ChunkSetExistAllocations.Add( record.CsId, record ) ;
					}
					else
					{
						// 無効データブロック
						m_ChunkSetEmptyAllocations.Add( record ) ;
					}

					offset += record.BlockLength ;
				}
			}

			// 総データブロック数
			m_TotalLength = offset ;

			//----------------------------------------------------------
			// データブロックを開く

			m_CSDB = StorageAccessor.Open( m_CSDB_Path, StorageAccessor.FileOperationTypes.WriteAndRead ) ;
		}

		/// <summary>
		/// チャンクセットアロケーションテーブルのセーブ
		/// </summary>
		private void SaveChunkSetAllocations( bool withClenup )
		{
			//----------------------------------------------------------

			// ここはいずれメモリへ全展開では無い形に変える

			Packer packer = new Packer() ;

			// 現在のデータブロック数
			int i, l = m_ChunkSetAllocations.Count ;

			packer.SetInt( l ) ;

			ChunkSetAllocation record ;
			int value ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				record = m_ChunkSetAllocations[ i ] ;

				value = record.CsId ;
				value |= ( int )( ( record.BlockLength & 0x7F ) << 24 ) ;

				if( record.IsEmpty == true )
				{
					value = ( int )( ( uint )value | ( uint )0x80000000 ) ;
				}

				packer.SetInt( value ) ;
			}

			byte[] data = packer.Data ;

			StorageAccessor.Save( m_CSAT_Path, data ) ;

			//------------------------------------------------------------------------------------------

			if( withClenup == true )
			{
				if( m_CSDB != null )
				{
					// チャンクセット読み書きのファイルハンドルを閉じる
					StorageAccessor.Close( m_CSDB_Path, m_CSDB ) ;
					m_CSDB = null ;
				}

				// 同時にクリアする
				m_ChunkSetAllocations.Clear() ;

				m_ChunkSetEmptyAllocations.Clear() ;
				m_ChunkSetExistAllocations.Clear() ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// データブロックの操作

		// チャンクセットデータをセーブする
		private void SaveDataBlocks( int csId, byte[] data )
		{
			if( m_CSDB == null )
			{
				// 準備が出来ていない
				return ;
			}

			//----------------------------------------------------------

			// 実データサイズ
			int size = data.Length ;

//			Debug.Log( "サイズ:" + size ) ;

			// データブロック単位でのサイズ
			int blockLength = size / m_DataBlockSize ;
			if( ( size % m_DataBlockSize ) >  0 )
			{
				blockLength ++ ;
			}

			//--------------

			bool isSaved = false ;

			// 既に保存済みか確認する
			if( m_ChunkSetExistAllocations.ContainsKey( csId ) == true )
			{
				// 既に保存した事がある

				var record = m_ChunkSetExistAllocations[ csId ] ;

				if( blockLength <= record.BlockLength )
				{
					// 現在の使用データプロック数をオーバーしないのでそのまま上書きする

					StorageAccessor.Seek( m_CSDB, record.BlockOffset * m_DataBlockSize, SeekOrigin.Begin ) ;

					var header = new Packer() ;
					header.SetInt( size ) ;

					StorageAccessor.Write( m_CSDB, header.Data ) ;	// 最初の４バイトは実サイズ
					StorageAccessor.Write( m_CSDB, data ) ;

					isSaved = true ;
				}
				else
				{
					// 現在の使用データプロック数をオーバーするので末尾に追加する

					// 無効なチャンクセット識別子にする
					record.CsId		= 0x08000000 ;

					// 空きデータブロックになってしまう
					record.IsEmpty = true ;
				}
			}

			//----------------------------------

			if( isSaved == false )
			{
				// 末尾に書き込む

				//---------------------------------
				// アロケーションを追加する

				var record = new ChunkSetAllocation()
				{
					CsId		= csId,
					BlockOffset	= m_TotalLength,
					BlockLength	= blockLength,
					IsEmpty		= false
				} ;

				m_ChunkSetAllocations.Add( record ) ;
				m_ChunkSetExistAllocations.Add( csId, record ) ;

				//---------------------------------
				// データブロックを書き込む

				StorageAccessor.Seek( m_CSDB, m_TotalLength * m_DataBlockSize, SeekOrigin.Begin ) ;

				var header = new Packer() ;
				header.SetInt( size ) ;

				StorageAccessor.Write( m_CSDB, header.Data ) ;	// 最初の４バイトは実サイズ
				StorageAccessor.Write( m_CSDB, data ) ;

				//---------------------------------

				// トータル使用データブロック数を増やす
				m_TotalLength += blockLength ;

				//---------------------------------------------------------
				// アロケーションが書き換わったのでこちらも保存する

//				SaveChunkSetAllocations() ;
			}
		}

		// チャンクセットデータをロードする
		private byte[] LoadDataBlocks( int csId )
		{
			if( m_CSDB == null )
			{
				// 準備が出来ていない
				return null ;
			}

			//----------------------------------------------------------

			// 既に保存済みか確認する
			if( m_ChunkSetExistAllocations.ContainsKey( csId ) == false )
			{
				// 保存されていない(普通にここには来る)
				return null ;
			}

			//------------------------------------------------------------------------------------------

			var record = m_ChunkSetExistAllocations[ csId ] ;

			//---------------------------------
			// データブロックを読み込む

			StorageAccessor.Seek( m_CSDB, record.BlockOffset * m_DataBlockSize, SeekOrigin.Begin ) ;

			// サイズを読み込む
			byte[] work = new byte[ 4 ] ;

			StorageAccessor.Read( m_CSDB, work ) ;

			var header = new Packer( work ) ;
			int size = header.GetInt() ;

			// データを読み込む
			byte[] data = new byte[ size ] ;

			StorageAccessor.Read( m_CSDB, data ) ;

			//---------------------------------

			return data ;
		}

	}
}
