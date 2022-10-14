using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.World
{
	/// <summary>
	/// クライアント:チャンク処理関係
	/// </summary>
	public partial class WorldClient
	{
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 現在位置に応じたチャンクの展開と除去を行う
		/// </summary>
		private void UpdateChunks()
		{
			// 現在の位置から周囲を展開する

			float px = m_PlayerActor.GetCamera().transform.position.x ;
			float pz = m_PlayerActor.GetCamera().transform.position.z ;

			// チャンク単位の座標を求める
			int cpx = ( int )( px / 16 ) ;
			int cpz = ( int )( pz / 16 ) ;

			// 現在位置のチャンクセット識別子
			int center_CsId = ( cpz << 12 ) | cpx ;

			if( m_Center_CsId != center_CsId )
			{
				// チャンクが変わった場合のみ展開・破棄が必要になる

				List<ChunkSetPriority> loadTargetChunkSets = new List<ChunkSetPriority>() ;

				// 一旦全ての使用中クラグをクリアする
				foreach( var activeChunkSet in m_ActiveChunkSets )
				{
					activeChunkSet.Value.IsUsing = false ;
				}

				// 展開が必要なチャンクセット識別子群
				var ltCsIds = GetTargetChunkSetIds( px, pz, WorldSettings.DISPLAY_CHUNK_RANGE ) ;

				// パフォーマンスモニタリング(読込チャンクセット数)
				m_P_ChunkSet_L.Value = ltCsIds.Count ;

				// 維持が必要なチャンクセット識別子群
				var ktCsIds = GetTargetChunkSetIds( px, pz, WorldSettings.DISPLAY_CHUNK_RANGE + 2 ) ;

				// パフォーマンスモニタリング(維持チャンクセット数)
				m_P_ChunkSet_K.Value = ktCsIds.Count - ltCsIds.Count ;

				foreach( var tCsId in ktCsIds )
				{
					if( ltCsIds.Contains( tCsId ) == true )
					{
						// 展開が必要な範囲
						if( m_ActiveChunkSets.ContainsKey( tCsId ) == true )
						{
							// 既に存在しているので継続して使用する
							m_ActiveChunkSets[ tCsId ].IsUsing = true ;
						}
						else
						{

							// 存在しない場合に展開が必要なチャンクセットの範囲
							loadTargetChunkSets.Add( new ChunkSetPriority( tCsId ) ) ;
						}
					}
					else
					{
						// 維持が必要な範囲
						if( m_ActiveChunkSets.ContainsKey( tCsId ) == true )
						{
							// 既に存在しているので継続して使用する
							m_ActiveChunkSets[ tCsId ].IsUsing = true ;

							// 属しているチャンクが左右上下前後の６方向にチャンクが存在しているか確認する
							m_ActiveChunkSets[ tCsId ].CheckChunks( this ) ;
						}
					}
				}

				//---------------------------------
				// 展開

				if( loadTargetChunkSets.Count >  0 )
				{
//					Debug.Log( "<color=#FFFF00>[CLIENT] ロード対象チャンクセット数 = " + loadTargetChunkSets.Count + "</color>" ) ;

					// 展開が必要なチャンクセットを視点から距離が近い順に並び替える
					loadTargetChunkSets = loadTargetChunkSets.OrderBy( _ => _.GetDistance( px, pz ) ).ToList() ;

					// サーバーにチャンクセットの転送要求を出す
					foreach( var loadTargetChunkSet in loadTargetChunkSets )
					{
						WS_Send_Request_LoadWorldChunkSet( loadTargetChunkSet.CsId ) ;
					}
				}

				//-------------
				// 破棄
				
				// 破棄する対象のチャンクセット識別子を取得する
				List<ClientChunkSetData> freeTargetChunkSets =
					m_ActiveChunkSets.Values.Where( _ => _.IsUsing == false ).ToList() ;

				if( freeTargetChunkSets != null && freeTargetChunkSets.Count >  0 )
				{
					// 展開中のチャンクで使用しないものを破棄する
					foreach( var freeTargetChunkSet in freeTargetChunkSets )
					{
						// サーバーにチャンクセットを破棄する事を通知する
						WS_Send_Request_FreeWorldChunkSet( freeTargetChunkSet.CsId ) ;

						// チャンクセットをクライアントから削除する
						FreeChunkSet( freeTargetChunkSet ) ;
					}
				}

				//---------------------------------

				// 現在位置のチャンクセット識別子を更新する
				m_Center_CsId  = center_CsId ;
			}

			//------------------------------------------------------------------------------------------

			// チャンクのメッシュを生成する(移動による作り直しは想定していないので生成済みのメッシュは更新しない)
//			t = Time.realtimeSinceStartup ;
			int c = BuildAllChunks() ;
//			if( c >  0 )
//			{
//				Debug.LogWarning( "新規展開チャンク(作)　個数:" + c + "　時間:" + ( Time.realtimeSinceStartup - t ) ) ;
//			}

			//------------------------------------------------------------------------------------------

			// パフォーマンスモニタリング(展開チャンク数)
			m_P_Chunk_O.Value = m_ActiveChunks.Count ;

			// パフォーマンスモニタリング(有効チャンク数)
			m_P_Chunk_E.Value = m_ActiveChunks.Count( _ => _.Model != null ) ;
		}

		/// <summary>
		/// チャンクセットのロード順番付け用
		/// </summary>
		public class ChunkSetPriority
		{
			public int X ;
			public int Z ;

			public int CsId => ( ( Z << 12 ) | X ) ;

			public ChunkSetPriority( int csId )
			{
				X =   csId         & 0x0FFF ;
				Z = ( csId >> 12 ) & 0x0FFF ;
			}


			/// <summary>
			/// 入力されたワールド座標からチャンクの中心のワールド座標までの距離を取得する(２次元)
			/// </summary>
			/// <param name="px"></param>
			/// <param name="pz"></param>
			/// <param name="py"></param>
			/// <returns></returns>
			public float GetDistance( float px, float pz )
			{
				// チャンクの中心のワールド座標
				float cx = ( X * 16.0f ) + 8.0f ;
				float cz = ( Z * 16.0f ) + 8.0f ;

				float dx = cx - px ;
				float dz = cz - pz ;

				return Mathf.Sqrt( dx * dx + dz * dz ) ;
			}
		}

		// 指定の円形の範囲内に含まれるチャンクセットの識別子群を取得する
		private List<int> GetTargetChunkSetIds( float x, float z, int chunkSetRange )
		{
			// チャンク単位の座標を求める
			int cpx = ( int )( x / 16 ) ;
			int cpz = ( int )( z / 16 ) ;

			int radius = chunkSetRange ;	// 表示距離と予備距離

			int cx, cz ;
			int cdx, cdz ;
			float ex ;
			int cx0, cx1 ;

			int xMin = WorldSettings.CHUNK_SET_X_MIN ;
			int xMax = WorldSettings.CHUNK_SET_X_MAX ;

			int zMin = WorldSettings.CHUNK_SET_Z_MIN ;
			int zMax = WorldSettings.CHUNK_SET_Z_MAX ;

			List<int> csIds = new List<int>() ;

			//----------------------------------

			cx0 = cpx - radius ;
			cx1 = cpx + radius ; 

			if( cx0 <  xMin )
			{
				cx0  = xMin ;
			}
			if( cx1 >  xMax )
			{
				cx1  = xMax ;
			}

			// 真横を追加する
			cz = cpz ;
			for( cx  = cx0 ; cx <= cx1 ; cx ++ )
			{
				csIds.Add( ( cz << 12 ) | cx ) ;
			}

			//----------------------------------
			// Ｚ方向にスライドしながら追加する

			for( cdz  = 1 ; cdz <= radius ; cdz ++ )
			{
				ex = Mathf.Sqrt( ( radius * radius ) - ( cdz * cdz ) ) ;
				cdx = ( int )( ex + 0.4f ) ;	// 四捨五入

				cx0 = cpx - cdx ;
				cx1 = cpx + cdx ;

				// Ｘ方向の範囲限定
				if( cx0 <  xMin )
				{
					cx0  = xMin ;
				}
				if( cx1 >  xMax )
				{
					cx1  = xMax ;
				}

				// Ｚの負方向
				cz = cpz - cdz ;
				if( cz >= zMin )
				{
					// 有効
					for( cx  = cx0 ; cx <= cx1 ; cx ++ )
					{
						csIds.Add( ( cz << 12 ) | cx ) ;
					}
				}

				// Ｚの正方向
				cz = cpz + cdz ;
				if( cz <= zMax )
				{
					// 有効
					for( cx  = cx0 ; cx <= cx1 ; cx ++ )
					{
						csIds.Add( ( cz << 12 ) | cx ) ;
					}
				}
			}

			return csIds ;
		}

		//-------------------------------------------------------------------------------------------

		// チャンクセットを追加する
		private void AddChunkSet( ClientChunkSetData chunkSet )
		{
			if( m_ActiveChunkSets.ContainsKey( chunkSet.CsId ) == true )
			{
				// 異常
				Debug.LogWarning( "異常発生:既にチャンクセットが登録済み:" + chunkSet.CsId ) ;
			}

			// チャンクセットをハッシュに追加する
			m_ActiveChunkSets.Add( chunkSet.CsId, chunkSet ) ;

			//--------------

			// 個々のチャンクをハッシュに追加する
			int cy, cId ;
			for( cy  =  0 ; cy <  chunkSet.Chunks.Length ; cy ++ )
			{
				cId = ( cy << 24 ) | ( chunkSet.Z << 12 ) | chunkSet.X ;

				if( m_ActiveChunks.ContainsKey( cId ) == true )
				{
					Debug.Log( "異常発生" ) ;
				}

				m_ActiveChunks.Add( cId, chunkSet.Chunks[ cy ] ) ;
			}
		}

		//-----------------------------------------------------------

		// チャンクセットを削除する
		private void FreeChunkSet( ClientChunkSetData chunkSet )
		{
			// 個々のチャンクをハッシュから削除する
			int cy, cId ;
			for( cy  =  0 ; cy <  chunkSet.Chunks.Length ; cy ++ )
			{
				cId = ( cy << 24 ) | ( chunkSet.Z << 12 ) | chunkSet.X ;

				if( m_ActiveChunks.ContainsKey( cId ) == false )
				{
					Debug.LogWarning( "異常発生" ) ;
				}

				// メッシュを破棄する
				m_ActiveChunks[ cId ].CleanupModel() ;

				m_ActiveChunks.Remove( cId ) ;
			}

			//--------------

			if( m_ActiveChunkSets.ContainsKey( chunkSet.CsId ) == false )
			{
				Debug.LogWarning( "異常発生" ) ;
			}

			// チャンクセットをハッシュから削除する
			m_ActiveChunkSets.Remove( chunkSet.CsId ) ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 指定の範囲のチャンクがロードされているか確認する(最初に行動が可能状態かどうかの確認用)
		private bool IsChunkSetsLoaded( int bx, int bz, int chunkSetRange )
		{
			int cx = ( bx >> 4 ) ;
			int cz = ( bz >> 4 ) ;

			int xMin = WorldSettings.CHUNK_SET_X_MIN ;
			int cx0 = cx - chunkSetRange ;
			if( cx0 <  xMin )
			{
				cx0  = xMin ;
			}

			int xMax = WorldSettings.CHUNK_SET_X_MAX ;
			int cx1 = cx + chunkSetRange ;
			if( cx1 >  xMax )
			{
				cx1  = xMax ;
			}

			int zMin = WorldSettings.CHUNK_SET_Z_MIN ;
			int cz0 = cz - chunkSetRange ;
			if( cz0 <  zMin )
			{
				cz0  = zMin ;
			}

			int zMax = WorldSettings.CHUNK_SET_Z_MAX ;
			int cz1 = cz + chunkSetRange ;
			if( cz1 >  zMax )
			{
				cz1  = zMax ;
			}

			int csId ;
			for( cz  = cz0 ; cz <= cz1 ; cz ++ )
			{
				for( cx  = cx0 ; cx <= cx1 ; cx ++ )
				{
					csId = ( cz << 12 ) | cx ;

					if( m_ActiveChunkSets.ContainsKey( csId ) == false )
					{
						// 指定の範囲でまだロード出来ていないチャンクセットがある
						return false ;
					}
				}
			}

			// 指定の範囲のチャンクセットは全てロード済みになっている
			return true ;
		}
	}
}
