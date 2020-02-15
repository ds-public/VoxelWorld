//#if TextMeshPro

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITextMesh ) ) ]
	public class UITextMeshInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UITextMesh view = target as UITextMesh ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool autoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UITextMesh : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}

			string localizeKey = EditorGUILayout.TextField( "Localize Key", view.LocalizeKey ) ;
			if( localizeKey != view.LocalizeKey )
			{
				Undo.RecordObject( view, "UITextMesh : Localize Key Change" ) ;	// アンドウバッファに登録
				view.LocalizeKey = localizeKey ;
				EditorUtility.SetDirty( view ) ;
			}

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			bool isCustomized = EditorGUILayout.Toggle( "Customize", view.IsCustomized ) ;
			if( isCustomized != view.IsCustomized )
			{
				Undo.RecordObject( view, "UITextMesh : Customize Change" ) ;	// アンドウバッファに登録
				view.IsCustomized = isCustomized ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.IsCustomized == true )
			{
				Color outlineColor_Old = CloneColor( view.OutlineColor ) ;
				Color outlineColor_New = EditorGUILayout.ColorField( "Outline Color", outlineColor_Old ) ;
				if( CheckColor( outlineColor_Old, outlineColor_New ) == false )
				{
					Undo.RecordObject( view, "UITextMesh : Outline Color Change" ) ;	// アンドウバッファに登録
					view.OutlineColor = outlineColor_New ;
					EditorUtility.SetDirty( view ) ;
				}
			}

			bool raycastTarget = EditorGUILayout.Toggle( "Raycast Target", view.RaycastTarget ) ;
			if( raycastTarget !=view.RaycastTarget )
			{
				Undo.RecordObject( view, "UITextMesh : RaycastTarget Change" ) ;	// アンドウバッファに登録
				view.RaycastTarget = raycastTarget ;
				EditorUtility.SetDirty( view ) ;
			}

			//-------------------------------------------------------------------

		}
	}
}

//#endif
