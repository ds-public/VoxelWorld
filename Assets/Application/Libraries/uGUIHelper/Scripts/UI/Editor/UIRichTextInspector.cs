using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIRichText ) ) ]
	public class UIRichTextInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIRichText view = target as UIRichText ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool autoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UIRichText : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}

			string localizeKey = EditorGUILayout.TextField( "Localize Key", view.LocalizeKey ) ;
			if( localizeKey != view.LocalizeKey )
			{
				Undo.RecordObject( view, "UIRichText : Localize Key Change" ) ;	// アンドウバッファに登録
				view.LocalizeKey = localizeKey ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}
