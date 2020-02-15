using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIButton のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIButton ) ) ]
	public class UIButtonInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIButton tTarget = target as UIButton ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// アトラススプライトの表示
			DrawAtlas( tTarget ) ;

			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIText tLabel = EditorGUILayout.ObjectField( "Label", tTarget.Label, typeof( UIText ), true ) as UIText ;
			if( tLabel != tTarget.Label )
			{
				Undo.RecordObject( tTarget, "UIButton : Label Change" ) ;	// アンドウバッファに登録
				tTarget.Label = tLabel ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			UIRichText tRichLabel = EditorGUILayout.ObjectField( "RichLabel", tTarget.RichLabel, typeof( UIRichText ), true ) as UIRichText ;
			if( tRichLabel != tTarget.RichLabel )
			{
				Undo.RecordObject( tTarget, "UIButton : Rich Label Change" ) ;	// アンドウバッファに登録
				tTarget.RichLabel = tRichLabel ;
				EditorUtility.SetDirty( tTarget ) ;
			}

//#if TextMeshPro
			UITextMesh tLabelMesh = EditorGUILayout.ObjectField( "LabelMesh", tTarget.LabelMesh, typeof( UITextMesh ), true ) as UITextMesh ;
			if( tLabelMesh != tTarget.LabelMesh )
			{
				Undo.RecordObject( tTarget, "UIButton : Label Mesh Change" ) ;	// アンドウバッファに登録
				tTarget.LabelMesh = tLabelMesh ;
				EditorUtility.SetDirty( tTarget ) ;
			}
//#endif

			UIImage tDisableMask = EditorGUILayout.ObjectField( "DisableMask", tTarget.DisableMask, typeof( UIImage), true ) as UIImage ;
			if( tDisableMask != tTarget.DisableMask )
			{
				Undo.RecordObject( tTarget, "UIButton : Disable Mask Change" ) ;	// アンドウバッファに登録
				tTarget.DisableMask = tDisableMask ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tClickTransitionEnabled = EditorGUILayout.Toggle( "Click Transition Enabled", tTarget.ClickTransitionEnabled ) ;
			if( tClickTransitionEnabled != tTarget.ClickTransitionEnabled )
			{
				Undo.RecordObject( tTarget, "UIButton : Click Transition Enabled Change" ) ;	// アンドウバッファに登録
				tTarget.ClickTransitionEnabled = tClickTransitionEnabled ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tWaitForTransition = EditorGUILayout.Toggle( "Wait For Transition", tTarget.WaitForTransition ) ;
			if( tWaitForTransition != tTarget.WaitForTransition )
			{
				Undo.RecordObject( tTarget, "UIButton : Wait For Transition Change" ) ;	// アンドウバッファに登録
				tTarget.WaitForTransition = tWaitForTransition ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tColorTransmission = EditorGUILayout.Toggle( "Color Transmission", tTarget.ColorTransmission ) ;
			if( tColorTransmission != tTarget.ColorTransmission )
			{
				Undo.RecordObject( tTarget, "UIButton : Color Transmission Change" ) ;	// アンドウバッファに登録
				tTarget.ColorTransmission = tColorTransmission ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			bool tSetPivotToCenter = EditorGUILayout.Toggle( "Set Pivot To Center", tTarget.AutoPivotToCenter ) ;
			if( tSetPivotToCenter != tTarget.AutoPivotToCenter )
			{
				Undo.RecordObject( tTarget, "UIButton : Set Pivot To Center Change" ) ;	// アンドウバッファに登録
				tTarget.AutoPivotToCenter = tSetPivotToCenter ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

