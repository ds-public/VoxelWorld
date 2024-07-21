#if UNITY_EDITOR

using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[ CustomEditor( typeof( SpriteActor ), true ) ]
	public class SpriteActorInspector : SpriteImageInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// ボールド
//			var boldStyle = new GUIStyle( GUI.skin.label )
//			{
//				fontStyle = FontStyle.Bold
//			} ;

			if( target.GetType() != typeof( SpriteActor ) )
			{
				// デフォルトの描画
				DrawDefaultInspector() ;


//				GUILayout.Label( "-----------------", boldStyle ) ;
				DrawSeparater() ;
			}

			//----------------------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteImage ;

			//----------------------------------

			DrawAtlas( component ) ;

			if( component.SpriteAtlas != null || component.SpriteSet != null )
			{
				DrawFlipper( component ) ;
			}

			DrawCollider( component ) ;

			DrawAnimator( component ) ;

			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}
	}
}
#endif

