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
		private bool SetBlock( int bx, int bz, int by, short bi )
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

			chunkSet.Chunks[ cy ].SetBlock( bx & 0x0F, bz & 0x0F, by & 0x0F, bi ) ;

			return true ;	// 成功
		}
	}
}
