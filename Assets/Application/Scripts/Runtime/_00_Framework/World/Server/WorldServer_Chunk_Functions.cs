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
	public partial class WorldServer
	{

		/// <summary>
		/// 指定の絶対位置のブロックを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private bool SetBlock( int bx, int bz, int by, int bi )
		{
			// チャンクセット識別子
			int csId = ( int )( ( bz & 0xFFF0 ) <<  8 ) | ( int )( ( bx & 0xFFF0 ) >> 4 ) ;

			if( m_ActiveChunkSets.ContainsKey( csId ) == false )
			{
				// 指定の位置に対応するチャンクは生成されていない
				Debug.LogError( "[エラー]指定位置に対応するチャンクは生成されていない: cx = " + ( bx >> 4 ) + " cz = " + ( bz >> 4 ) ) ;
				return false ;
			}

			//----------------------------------------------------------

			// 対象のチャンクセット
			var chunkSet = m_ActiveChunkSets[ csId ] ;

			// 縦は64チャンクある
			int cy = ( by & 0x03F0 ) >> 4 ;

			if( bi != 0 )
			{
				if( chunkSet.Chunks[ cy ] == null )
				{
					// これまで完全な空白チャンクだった
					chunkSet.Chunks[ cy ] = new ServerChunkData() ;
				}

				chunkSet.Chunks[ cy ].SetBlock( bx & 0x0F, bz & 0x0F, by & 0x0F, ( short )bi ) ;
			}
			else
			{
				if( chunkSet.Chunks[ cy ] != null )
				{
					chunkSet.Chunks[ cy ].SetBlock( bx & 0x0F, bz & 0x0F, by & 0x0F, ( short )bi ) ;

					// 完全に空白になったか確認する
					int lx, lz, ly, c = 0 ;
					for( ly  = 0 ; ly <= 15 ; ly ++ )
					{
						for( lz  = 0 ; lz <= 15 ; lz ++ )
						{
							for( lx  = 0 ; lx <= 15 ; lx ++ )
							{
								if( chunkSet.Chunks[ cy ].GetBlock( lx, lz, ly ) != 0 )
								{
									c ++ ;
								}
							}
						}
					}

					if( c == 0 )
					{
						// 完全空白化
						chunkSet.Chunks[ cy ] = null ;
					}
				}
			}

			// チャンクの内容に変化があった
			chunkSet.SetDirty() ;

			return true ;	// 成功
		}
	}
}
