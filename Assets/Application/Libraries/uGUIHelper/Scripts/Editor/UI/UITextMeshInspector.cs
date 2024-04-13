#if UNITY_EDITOR

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
			
			bool autoSizeFitting = EditorGUILayout.Toggle( new GUIContent( "Auto Size Fitting", "RectTransformのサイズを強制的にテキストのサイズに合わせます\n<color=#FFFF00>Wrapping(テキスト自動折返し)を使用する際は無効にして下さい</color>" ), view.AutoSizeFitting ) ;
			if( autoSizeFitting != view.AutoSizeFitting )
			{
				Undo.RecordObject( view, "UITextMesh : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				view.AutoSizeFitting = autoSizeFitting ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.AutoSizeFitting == false )
			{
				float autoCharacterSpacing = EditorGUILayout.FloatField( "Auto Chatacter Spacing", view.AutoCharacterSpacing ) ;
				if( autoCharacterSpacing != view.AutoCharacterSpacing )
				{
					Undo.RecordObject( view, "UITextMesh : Auto Character Spacing Change" ) ;	// アンドウバッファに登録
					view.AutoCharacterSpacing = autoCharacterSpacing ;
					EditorUtility.SetDirty( view ) ;
				}
			}

			string localizationKey = EditorGUILayout.TextField( "Localization Key", view.LocalizationKey ) ;
			if( localizationKey != view.LocalizationKey )
			{
				Undo.RecordObject( view, "UITextMesh : Localization Key Change" ) ;	// アンドウバッファに登録
				view.LocalizationKey = localizationKey ;
				EditorUtility.SetDirty( view ) ;
			}

			// 全角
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool zenkaku = EditorGUILayout.Toggle(view.Zenkaku, GUILayout.Width( 16f ) ) ;
				if( zenkaku != view.Zenkaku )
				{
					Undo.RecordObject( view, "UITextMesh : Zenkaku Change" ) ;	// アンドウバッファに登録
					view.Zenkaku = zenkaku ;
					EditorUtility.SetDirty(view ) ;
				}
				GUILayout.Label( new GUIContent( "Zenkaku", "文字列を強制的に全角にします" ), GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ルビ
			bool overrideEnabled = EditorGUILayout.Toggle( "Override", view.OverrideEnabled ) ;
			if( overrideEnabled != view.OverrideEnabled )
			{
				Undo.RecordObject( view, "UITextMesh : OverrideEnabled Change" ) ;	// アンドウバッファに登録
				view.OverrideEnabled = overrideEnabled ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.OverrideEnabled == true )
			{
				string overrideText = view.OverrideText ;

				EditorGUILayout.HelpBox( "Exsample:\n<ruby=きょう>今日</ruby>は<ruby=よ>良</ruby>い\n<ruby=てんき>天気</ruby>ですね", MessageType.Info, true ) ;

				SerializedProperty	m_OverrideText = serializedObject.FindProperty( "m_OverrideText" ) ;
				EditorGUILayout.PropertyField( m_OverrideText ) ;

				serializedObject.ApplyModifiedProperties() ;

				if( overrideText != view.OverrideText )
				{
					view.ApplyOverrideText() ;
				}

				float rubyScale = EditorGUILayout.Slider( view.RubyScale, 0.1f, 1 ) ;
				if( rubyScale != view.RubyScale )
				{
					Undo.RecordObject( view, "UITextMesh : RubyScale Change" ) ;	// アンドウバッファに登録
					view.RubyScale = rubyScale ;
					EditorUtility.SetDirty( view ) ;
				}
			}

			//----------------------------------------------------------
			
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

#endif
