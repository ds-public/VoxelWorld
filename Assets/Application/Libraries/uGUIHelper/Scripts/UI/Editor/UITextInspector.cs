using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIText ) ) ]
	public class UITextInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIText view = target as UIText ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool autoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UIText : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}

			string localizeKey = EditorGUILayout.TextField( "Localize Key", view.LocalizeKey ) ;
			if( localizeKey != view.LocalizeKey )
			{
				Undo.RecordObject( view, "UIText : Localize Key Change" ) ;	// アンドウバッファに登録
				view.LocalizeKey = localizeKey ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}
