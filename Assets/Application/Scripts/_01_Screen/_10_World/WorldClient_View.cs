using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// クライアント(ビュー)
	/// </summary>
	public partial class WorldClient : MonoBehaviour
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
				RenderSettings.fogMode = FogMode.Linear ;
				RenderSettings.fogDensity = 1.0f ;
				RenderSettings.fogStartDistance = 0 ;
				RenderSettings.fogEndDistance = distance ;
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

			Camera fpsCamera = m_Camera ;
			if( fpsCamera == null )
			{
				return ;
			}

			// 視錐台をセットアップする
			m_ViewVolume.Setup( fpsCamera, cameraPosition ) ;

			foreach( var activeChunk in ActiveChunks )
			{
				if( activeChunk.Value.Model != null )
				{
					activeChunk.Value.Model.SetActive( m_ViewVolume.IsVisible( activeChunk.Value.BoundingBox ) ) ;
				}
			}

//			Debug.LogWarning( "表示対象チャンク数:" + c ) ;
		}
	}
}