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
				return m_ActiveChunkSets[ csId ].Inflate() ;
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
			return m_ActiveChunkSets[ csId ].Inflate() ;
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
				chunkSet.SetCompressedData( data ) ;

				return chunkSet ;
			}

			//----------------------------------------------------------
			// 生成

			// 空状態で初期化する
			chunkSet.CreateDefault() ;

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
						Debug.Log( "<color=#FFFF00>[SERVER]チャンクセットが破棄される際にストレージに保存する:" + csId.ToString( "X4" ) + "</color>" ) ;
						SaveDataBlocks( csId, m_ActiveChunkSets[ csId ].Inflate() ) ;
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
							SaveDataBlocks( activeChunkSet.CsId, activeChunkSet.Inflate() ) ;
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
						SaveDataBlocks( activeChunkSet.CsId, activeChunkSet.Inflate() ) ;
					}
				}

				m_ActiveChunkSets.Clear() ;
			}

			Debug.Log( "<color=#00FFFF>[SERVER] 残存チャンクセット数 = " + m_ActiveChunkSets.Count + "</color>" ) ;
		}
	}
}
