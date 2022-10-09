using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using MathHelper ;
using uGUIHelper ;
using TransformHelper ;

using DBS.WorldServerClasses ;

namespace DBS.World
{
	/// <summary>
	/// サーバー処理
	/// </summary>
	public partial class WorldServer
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// チャンクセットの取得または生成を行う
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="onLoaded"></param>
		public byte[] LoadChunkSet( int csId, string clientId )
		{
			if( m_ActiveChunkSets.ContainsKey( csId ) == true )
			{
				// クライアントの参照を追加する
				m_ActiveChunkSets[ csId ].AddCliendId( clientId ) ;

				// 既に生成済み
				// 変更が無ければパック圧縮済みのものを返す
				// 変更が有るようなら再度パック圧縮する
				return m_ActiveChunkSets[ csId ].Pack() ;
			}

			//------------------------------------------------------------------------------------------

			// 未生成のチャンクセットであるため新規に生成する
			var chunkSetData = LoadOrMakeChunkSet( csId ) ;

			// アクティブなチャンクセットに追加する
			m_ActiveChunkSets.Add( csId, chunkSetData ) ;

			// クライアントの参照を追加する
			m_ActiveChunkSets[ csId ].AddCliendId( clientId ) ;

			//------------------------------------------------------------------------------------------

			// パック圧縮したものを返す
			return m_ActiveChunkSets[ csId ].Pack() ;
		}


		/// <summary>
		/// 新しいチャンクセットデータを生成する
		/// </summary>
		/// <param name="csid"></param>
		/// <returns></returns>
		private ServerChunkSetData LoadOrMakeChunkSet( int csId )
		{
			int x = ( int )( ( csId       ) & 0x0FFF ) ;
			int z = ( int )( ( csId >> 12 ) & 0x0FFF ) ;
			
			ServerChunkSetData chunkSet = new ServerChunkSetData()
			{
				X = x,
				Z = z
			} ;

			//----------------------------------------------------------
			// 展開

			byte[] data = LoadDataBlocks( csId ) ;

			if( data != null )
			{
//				Debug.Log( "<color=#00FFFF>[SERVER]チャンクが展開される際にストレージから取得した:" + csId.ToString( "X4" ) + "</color>" ) ;

				// ロードできた
				chunkSet.SetPackedData( data ) ;
				chunkSet.Unpack() ;

				return chunkSet ;
			}

			//----------------------------------------------------------
			// 生成

			int[,] heightMap = new int[ 16, 16 ] ;	// x z

			float ox = x * 16 ;
			float oz = z * 16 ;

			int maxHeight = -1 ;

			float pn ;

			int bx, by, bz ;
			for( bz  = 0 ; bz <= 15 ; bz ++ )
			{
				for( bx  = 0 ; bx <= 15 ; bx ++ )
				{
					pn = PerlinNoise.GetValue( ( float )( ox + bx ) / 16, ( float )( oz + bz ) / 16 ) ;
					by = ( int )( ( pn * 8 ) + ( 512 ) ) ;

					heightMap[ bx, bz ] = by ;

					if( by >  maxHeight )
					{
						maxHeight = by ;
					}
				}
			}

			//-------------------------

			ServerChunkData chunkData ;

			int y0, y1 ;

			int h, hy ;
			for( h  =  0 ; h <  64 ; h ++ )
			{
				y0 =  h * 16 ;
				y1 = y0 + 15 ;

				if( y0 >  maxHeight )
				{
					// 全てが空になるチャンク
					break ;	// それ以上上のチャンクは存在しない
				}

				chunkData = new ServerChunkData() ;
				
				for( bz  = 0 ; bz <= 15 ; bz ++ )
				{
					for( bx  =  0 ; bx <= 15 ; bx ++ )
					{
						hy = heightMap[ bx, bz ] ;

						if( hy <  y0 )
						{
							// ここのブロックは全て無し(チャンクのインスタンスは null)
							continue ;
						}
						else
						if( hy >  y1 )
						{
							// ここのブロックは全て有り
							hy = 15 ;
						}
						else
						{
							// ここのブロックは一部有り
							hy -= y0 ;
						}

						for( by  =  0 ; by <= hy ; by ++ )
						{
							chunkData.SetBlock( bx, bz, by, 1 ) ;
						}
					}
				}

				chunkSet.Chunks[ h ] = chunkData ;
			}

			Debug.Log( "<color=#00DFFF>[SERVER] 完全新規にチャンク作成</color>" ) ;
			return chunkSet ;
		}

		//-------------------------------------------------------------------------------------------
		// チャンクを破棄する

		/// <summary>
		/// チャンクセットの破棄を行う
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="onLoaded"></param>
		private void FreeChunkSet( int csId, string clientId )
		{
			if( m_ActiveChunkSets != null && m_ActiveChunkSets.Count >  0 )
			{
				if( m_ActiveChunkSets.ContainsKey( csId ) == false )
				{
					// 既に破棄済み
					return ;
				}

				int referencedCount = m_ActiveChunkSets[ csId ].RemoveClientId( clientId ) ;
				if( referencedCount == 0 )
				{
					// チャンクセットへの参照が無くなったので破棄可能

					// 変更がかかっていたらストレージに保存する
					if( m_ChunkSetExistAllocations.ContainsKey( csId ) == false || m_ActiveChunkSets[ csId ].IsDirty == true )
					{
						Debug.Log( "<color=#FFFF00>[SERVER]チャンクが破棄される際にストレージに保存する:" + csId.ToString( "X4" ) + "</color>" ) ;
						SaveDataBlocks( csId, m_ActiveChunkSets[ csId ].Pack() ) ;
					}

					m_ActiveChunkSets.Remove( csId ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// 特定のクライアントがログアウトした際にそのクライアントが関係しているチャンクセットで破棄可能なものを破棄する
		private void FreeChunkSetsWithClientId( string clientId )
		{
			if( m_ActiveChunkSets != null && m_ActiveChunkSets.Count >  0 )
			{
				List<int> removeTargets = new List<int>() ;

				foreach( var activeChunkSet in m_ActiveChunkSets.Values )
				{
					int referencedCount = activeChunkSet.RemoveClientId( clientId ) ;
					if( referencedCount == 0 )
					{
						// チャンクセットへの参照が無くなったので破棄可能

						// 変更がかかっていたらストレージに保存する
						if( m_ChunkSetExistAllocations.ContainsKey( activeChunkSet.CsId ) == false || activeChunkSet.IsDirty == true )
						{
							Debug.Log( "<color=#FFFF00>[SERVER]チャンクが破棄される際にストレージに保存する:" + activeChunkSet.CsId.ToString( "X4" ) + "</color>" ) ;
							SaveDataBlocks( activeChunkSet.CsId, activeChunkSet.Pack() ) ;
						}

						removeTargets.Add( activeChunkSet.CsId ) ;
					}
				}

				if( removeTargets.Count >  0 )
				{
					// 削除対象になったチャンクセットを除外する
					m_ActiveChunkSets.RemoveRange( removeTargets ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// 全てチャンクセットを破棄する
		private void FreeAllChunkSets()
		{
			if( m_ActiveChunkSets != null && m_ActiveChunkSets.Count >  0 )
			{
				foreach( var activeChunkSet in m_ActiveChunkSets.Values )
				{
					if( m_ChunkSetExistAllocations.ContainsKey( activeChunkSet.CsId ) == false || activeChunkSet.IsDirty == true )
					{
						Debug.Log( "<color=#FFFF00>[SERVER]チャンクが破棄される際にストレージに保存する:" + activeChunkSet.CsId.ToString( "X4" ) + "</color>" ) ;
						SaveDataBlocks( activeChunkSet.CsId, activeChunkSet.Pack() ) ;
					}
				}

				m_ActiveChunkSets.Clear() ;
			}

			Debug.Log( "<color=#00FFFF>[SERVER] 残存チャンクセット数 = " + m_ActiveChunkSets.Count + "</color>" ) ;
		}
	}
}
