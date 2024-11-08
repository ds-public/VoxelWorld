#if UNITY_EDITOR

using System.Collections.Generic ;

using UnityEngine ;
using UnityEditor ;


namespace uGUIHelper
{
	/// <summary>
	/// UIDropdown のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIPulldown ) ) ]
	public class UIPulldownInspector : UIViewInspector
	{
		protected override void DrawInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			var view = target as UIPulldown ;
			
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			//-------------------------------------------------------------------

			var captionText = EditorGUILayout.ObjectField( "CaptionText", view.CaptionText, typeof( UITextMesh ), true ) as UITextMesh ;
			if( view.CaptionText != captionText )
			{
				Undo.RecordObject( view, "UIPulldown : CaptionText Change" ) ;	// アンドウバッファに登録
				view.CaptionText  = captionText ;
				EditorUtility.SetDirty( view ) ;
			}

			var captionImage = EditorGUILayout.ObjectField( "CaptionImage", view.CaptionImage, typeof( UIImage ), true ) as UIImage ;
			if( view.CaptionImage != captionImage )
			{
				Undo.RecordObject( view, "UIPulldown : CaptionImage Change" ) ;	// アンドウバッファに登録
				view.CaptionImage  = captionImage ;
				EditorUtility.SetDirty( view ) ;
			}

			var template = EditorGUILayout.ObjectField( "Template", view.Template, typeof( UIListView ), true ) as UIListView ;
			if( view.Template != template )
			{
				Undo.RecordObject( view, "UIPulldown : Template Change" ) ;	// アンドウバッファに登録
				view.Template  = template ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var multiSelect = EditorGUILayout.Toggle( view.MultiSelect, GUILayout.Width( 16f ) ) ;
				if( view.MultiSelect != multiSelect )
				{
					Undo.RecordObject( view, "UIPulldown : MultiSelect Change" ) ;	// アンドウバッファに登録
					view.MultiSelect  = multiSelect ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "MultiSelect" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.MultiSelect == false )
			{
				// 単一選択
				var value = EditorGUILayout.IntField( "Value", view.Value ) ;
				if( view.Value != value )
				{
					Undo.RecordObject( view, "UIPulldown : Value Duration" ) ;	// アンドウバッファに登録
					view.Value  = value ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			else
			{
				// 複数選択
				var values = serializedObject.FindProperty( "m_Values" ) ;
				EditorGUILayout.PropertyField( values ) ;

				var nothingText = EditorGUILayout.TextField( "NothingText", view.NothingText ) ;
				if( view.NothingText != nothingText )
				{
					Undo.RecordObject( view, "UIPulldown : NothingText Change" ) ;	// アンドウバッファに登録
					view.NothingText  = nothingText ;
					EditorUtility.SetDirty( view ) ;
				}

				var everythingText = EditorGUILayout.TextField( "EverythingText", view.EverythingText ) ;
				if( view.EverythingText != everythingText )
				{
					Undo.RecordObject( view, "UIPulldown : EverythingText Change" ) ;	// アンドウバッファに登録
					view.EverythingText  = everythingText ;
					EditorUtility.SetDirty( view ) ;
				}

				var mixedText = EditorGUILayout.TextField( "MixedText", view.MixedText ) ;
				if( view.MixedText != mixedText )
				{
					Undo.RecordObject( view, "UIPulldown : MixedText Change" ) ;	// アンドウバッファに登録
					view.MixedText  = mixedText ;
					EditorUtility.SetDirty( view ) ;
				}
			}

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			var alphaFadeSpeed = EditorGUILayout.Slider( "AlphaFadeSpeed", view.AlphaFadeSpeed, 0.1f, 10.0f ) ;
			if( view.AlphaFadeSpeed != alphaFadeSpeed )
			{
				Undo.RecordObject( view, "UIPulldown : AlphaFadeSpeed Change" ) ;	// アンドウバッファに登録
				view.AlphaFadeSpeed  = alphaFadeSpeed ;
				EditorUtility.SetDirty( view ) ;
			}

			var alphaFadeDuration = EditorGUILayout.FloatField( "AlphaFadeDuration", view.AlphaFadeDuration ) ;
			if( view.AlphaFadeDuration != alphaFadeDuration )
			{
				Undo.RecordObject( view, "UIPulldown : AlphaFadeDuration Change" ) ;	// アンドウバッファに登録
				view.AlphaFadeDuration  = alphaFadeDuration ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			var anchorType = ( UIPulldown.AnchorTypes )EditorGUILayout.EnumPopup( "AnchorType",  view.AnchorType, GUILayout.Width( 240f ) ) ;
			if( view.AnchorType != anchorType )
			{
				Undo.RecordObject( view, "UIPulldown : AnchorType Change" ) ;	// アンドウバッファに登録
				view.AnchorType  = anchorType ;
				EditorUtility.SetDirty( view ) ;
			}

			var additionalCanvasSortOrder = EditorGUILayout.IntField( "AdditionalCanvasSortOrder", view.AdditionalCanvasSortOrder ) ;
			if( view.AdditionalCanvasSortOrder != additionalCanvasSortOrder )
			{
				Undo.RecordObject( view, "UIPulldown : AdditionalCanvasSortOrder Change" ) ;	// アンドウバッファに登録
				view.AdditionalCanvasSortOrder  = additionalCanvasSortOrder ;
				EditorUtility.SetDirty( view ) ;
			}

			var maskColorOld = view.MaskColor ;
			var maskColorNew = EditorGUILayout.ColorField( "MaskColor", maskColorOld ) ;
			if( maskColorOld.Equals( maskColorNew ) == false )
			{
				Undo.RecordObject( view, "UIPulldown : MaskColor Change" ) ;	// アンドウバッファに登録
				view.MaskColor  = maskColorNew ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			var pulldownItems = serializedObject.FindProperty( "m_PulldownItems" ) ;
			EditorGUILayout.PropertyField( pulldownItems ) ;

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// フォーカスプロセッシングイネーブルド
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var focusProcessingEnabled = EditorGUILayout.Toggle( view.FocusProcessingEnabled, GUILayout.Width( 16f ) ) ;
				if( view.FocusProcessingEnabled != focusProcessingEnabled )
				{
					Undo.RecordObject( view, "UIPulldown : FocusProcessingEnabled Change" ) ;	// アンドウバッファに登録
					view.FocusProcessingEnabled  = focusProcessingEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "FocusProcessingEnabled" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			// フォーカスインデックス
			var focusIndex = EditorGUILayout.IntField( "FocusIndex", view.FocusIndex ) ;
			if( view.FocusIndex != focusIndex )
			{
				Undo.RecordObject( view, "UIPulldown : FocusIndex Duration" ) ;	// アンドウバッファに登録
				view.FocusIndex  = focusIndex ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}
	}
}

#endif
