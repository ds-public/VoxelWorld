using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(ビュー)
	/// </summary>
	public partial class WorldClient
	{
		/// <summary>
		/// フォグを設定する
		/// </summary>
		/// <param name="state"></param>
		/// <param name="distance"></param>
		private void SetFog( bool state, float distance )
		{
			if( state == true )
			{
				RenderSettings.fog = true ;
				RenderSettings.fogColor = new Color32(   0, 255, 255, 255 ) ;

				// ＰＣだと線形フォグが効かない
//				RenderSettings.fogMode = FogMode.Linear ;
//				RenderSettings.fogDensity = 1.0f ;
//				RenderSettings.fogStartDistance = 0 ;
//				RenderSettings.fogEndDistance = distance ;

				RenderSettings.fogMode = FogMode.ExponentialSquared ;
				RenderSettings.fogDensity = 0.01f ;
			}
			else
			{
				RenderSettings.fog = false ;
			}
		}

		/// <summary>
		/// オクルージョンカリングのテストを行う(見えないチャンク単位のメッシュを非表示にする)
		/// </summary>
		/// <param name="cameraPosition"></param>
		private void OcclusionCulling( Vector3 cameraPosition )
		{
			// 5 x 5 に展開する

			//---------------------------------------------------------

			// 各チャンクが視錐台に含まれるか確認する

			Camera fpsCamera = m_PlayerActor.GetCamera() ;
			if( fpsCamera == null )
			{
				return ;
			}

			// 視錐台をセットアップする
			m_ViewVolume.Setup( fpsCamera, cameraPosition ) ;

			int c = 0 ;

			if( m_ViewVolumeCenterOnly == false )
			{
				// 通常表示
				foreach( var activeChunk in m_ActiveChunks )
				{
					if( activeChunk.Value.Model != null )
					{
						activeChunk.Value.Model.SetActive( m_ViewVolume.IsVisible( activeChunk.Value.BoundingBox ) ) ;
						if( activeChunk.Value.Model.activeSelf == true )
						{
							c ++ ;
						}
					}
				}
			}
			else
			{
				// 限定表示
				foreach( var activeChunk in m_ActiveChunks )
				{
					if( activeChunk.Value.Model != null )
					{
						activeChunk.Value.Model.SetActive( m_ViewVolume.IsVisible( activeChunk.Value.BoundingBox ) && activeChunk.Value.CsId == m_Center_CsId ) ;
						if( activeChunk.Value.Model.activeSelf == true )
						{
							c ++ ;
						}
					}
				}
			}

			// パフォーマンスモニタリング(表示チャンク数)
			m_P_Chunk_V.Value = c ;
//			Debug.Log( "表示対象チャンク数:" + c ) ;
		}
	}
}
