#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIToggle のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIToggle ) ) ]
	public class UIToggleInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIToggle view = target as UIToggle ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UITextMesh labelMesh = EditorGUILayout.ObjectField( "LabelMesh", view.LabelMesh, typeof( UITextMesh ), true ) as UITextMesh ;
			if( labelMesh != view.LabelMesh )
			{
				Undo.RecordObject( view, "UIToggle : Label Mesh Change" ) ;	// アンドウバッファに登録
				view.LabelMesh = labelMesh ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage cursor = EditorGUILayout.ObjectField( "Cursor", view.Cursor, typeof( UIImage ), true ) as UIImage ;
			if( cursor != view.Cursor )
			{
				Undo.RecordObject( view, "UIToggle : Cursor Change" ) ;	// アンドウバッファに登録
				view.Cursor = cursor ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		}
	}
}

#endif
