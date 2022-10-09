#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIImage ) ) ]
	public class UIImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIImage view = target as UIImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlas( view ) ;

			// Flipper の追加と削除
			DrawFlipper( view ) ;

			// マテリアル選択
			DrawMaterial( view ) ;

			//----------------------------------------------------------

		}
	}
}

#endif
