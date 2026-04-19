#if UNITY_EDITOR

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
		protected override void DrawInspectorGUI()
		{
			var view = target as UIButton ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// アトラススプライトの表示
			DrawAtlas( view ) ;

			// マテリアル選択
			DrawMaterial( view ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIText label = EditorGUILayout.ObjectField( "Label", view.Label, typeof( UIText ), true ) as UIText ;
			if( label != view.Label )
			{
				Undo.RecordObject( view, "UIButton : Label Change" ) ;	// アンドウバッファに登録
				view.Label = label ;
				EditorUtility.SetDirty( view ) ;
			}

			UIRichText richLabel = EditorGUILayout.ObjectField( "RichLabel", view.RichLabel, typeof( UIRichText ), true ) as UIRichText ;
			if( richLabel != view.RichLabel )
			{
				Undo.RecordObject( view, "UIButton : Rich Label Change" ) ;	// アンドウバッファに登録
				view.RichLabel = richLabel ;
				EditorUtility.SetDirty( view ) ;
			}

			UITextMesh labelMesh = EditorGUILayout.ObjectField( "LabelMesh", view.LabelMesh, typeof( UITextMesh ), true ) as UITextMesh ;
			if( labelMesh != view.LabelMesh )
			{
				Undo.RecordObject( view, "UIButton : Label Mesh Change" ) ;	// アンドウバッファに登録
				view.LabelMesh = labelMesh ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage icon = EditorGUILayout.ObjectField( "Icon", view.Icon, typeof( UIImage ), true ) as UIImage ;
			if( icon != view.Icon )
			{
				Undo.RecordObject( view, "UIButton : Icon Change" ) ;	// アンドウバッファに登録
				view.Icon = icon ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage disableMask = EditorGUILayout.ObjectField( "DisableMask", view.DisableMask, typeof( UIImage ), true ) as UIImage ;
			if( disableMask != view.DisableMask )
			{
				Undo.RecordObject( view, "UIButton : Disable Mask Change" ) ;	// アンドウバッファに登録
				view.DisableMask = disableMask ;
				EditorUtility.SetDirty( view ) ;
			}

			UIImage cursor = EditorGUILayout.ObjectField( "Cursor", view.Cursor, typeof( UIImage ), true ) as UIImage ;
			if( cursor != view.Cursor )
			{
				Undo.RecordObject( view, "UIButton : Cursor Change" ) ;	// アンドウバッファに登録
				view.Cursor = cursor ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.Cursor != null && view.Cursor == view.PointerCursor )
			{
				EditorGUILayout.HelpBox( GetMessage( "SameCursor" ), MessageType.Info, true ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool clickTransitionEnabled = EditorGUILayout.Toggle( view.ClickTransitionEnabled, GUILayout.Width( 16f ) ) ;
				if( clickTransitionEnabled != view.ClickTransitionEnabled )
				{
					Undo.RecordObject( view, "UIButton : Click Transition Enabled Change" ) ;	// アンドウバッファに登録
					view.ClickTransitionEnabled = clickTransitionEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Click Transition Enabled", "<color=#00FFFF>ランタイム実行時</color>に\nクリックを行った場合にトランジションを実行するか設定します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool waitForTransition = EditorGUILayout.Toggle( view.WaitForTransition, GUILayout.Width( 16f ) ) ;
				if( waitForTransition != view.WaitForTransition )
				{
					Undo.RecordObject( view, "UIButton : Wait For Transition Change" ) ;	// アンドウバッファに登録
					view.WaitForTransition = waitForTransition ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Wait For Transition", "<color=#00FFFF>ランタイム実行時</color>に\nトランジションが終了するまで入力を禁止するかどうかを設定します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// クリックの排他制御
				bool clickExclusionEnabled = EditorGUILayout.Toggle( view.ClickExclusionEnabled, GUILayout.Width( 16f ) ) ;
				if( clickExclusionEnabled != view.ClickExclusionEnabled )
				{
					Undo.RecordObject( view, "UIButton : Click Exclusion Enabled Change" ) ;	// アンドウバッファに登録
					view.ClickExclusionEnabled = clickExclusionEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Click Exclusion Enabled", "<color=#00FFFF>ランタイム実行時</color>に\n同じボタンに対して同時に複数のクリックを実行できないようにします" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool setPivotToCenter = EditorGUILayout.Toggle( view.AutoPivotToCenter, GUILayout.Width( 16f ) ) ;
				if( setPivotToCenter != view.AutoPivotToCenter )
				{
					Undo.RecordObject( view, "UIButton : Set Pivot To Center Change" ) ;	// アンドウバッファに登録
					view.AutoPivotToCenter = setPivotToCenter ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Set Pivot To Center", "<color=#00FFFF>ランタイム実行時</color>に\nピボットを強制的に中心(0.5,0.5)に変更します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			UIButtonGroup targetButtonGroup = EditorGUILayout.ObjectField( "ButtonGroup", view.TargetButtonGroup, typeof( UIButtonGroup ), true ) as UIButtonGroup ;
			if( targetButtonGroup != view.TargetButtonGroup )
			{
				Undo.RecordObject( view, "UIButton : Target Button Group Change" ) ;	// アンドウバッファに登録
				view.TargetButtonGroup = targetButtonGroup ;
				EditorUtility.SetDirty( view ) ;
			}

		}

		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "SameCursor",			    "カーソルが重複しています。場合によっては意図しない挙動となる可能性があるため、異なるものを設定する事を推奨します。" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "SameCursor",			    "The cursors are duplicated. This may lead to unintended behavior in some cases, so it is recommended to set different cursors." },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ] ;
			}
		}

	}   // class
}   // namespace

#endif
