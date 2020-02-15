using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIInputField のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInputField ) ) ]
	public class UIInputFieldInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIInputField tTarget = target as UIInputField ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			FontFilter tFontFilter = EditorGUILayout.ObjectField( "Font Filter", tTarget.FontFilter, typeof( FontFilter ), false ) as FontFilter ;
			if( tFontFilter != tTarget.FontFilter )
			{
				Undo.RecordObject( tTarget, "UIInputField : Font Filter Change " ) ;	// アンドウバッファに登録
				tTarget.FontFilter = tFontFilter ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			string tFontAlternateCodeOld = "" + tTarget.FontAlternateCode ;
			string tFontAlternateCodeNew = EditorGUILayout.TextField( "Font Alternate Code", tFontAlternateCodeOld ) ;
			if( string.IsNullOrEmpty( tFontAlternateCodeNew ) == false && ( tFontAlternateCodeNew[ 0 ] != tFontAlternateCodeOld[ 0 ] ) )
			{
				Undo.RecordObject( tTarget, "UIInputField : Font Alternate Code Change " ) ;	// アンドウバッファに登録
				tTarget.FontAlternateCode = tFontAlternateCodeNew[ 0 ] ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

