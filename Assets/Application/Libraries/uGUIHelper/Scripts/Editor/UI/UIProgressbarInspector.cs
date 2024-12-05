#if UNITY_EDITOR

using System.Collections.Generic ;

using UnityEngine ;
using UnityEditor ;


namespace uGUIHelper
{
	/// <summary>
	/// UIProgressbar のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIProgressbar ) ) ]
	public class UIProgressbarInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIProgressbar view = target as UIProgressbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// スライダーでアルファをコントロール出来るようにする
			float value = EditorGUILayout.Slider( "Value", view.Value, 0.0f, 1.0f ) ;
			if( value != view.Value )
			{
				Undo.RecordObject( target, "UIProgressbar : Value Change" ) ;	// アンドウバッファに登録
				view.Value = value ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// 値の反転
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var isReverse = EditorGUILayout.Toggle( view.IsReverse, GUILayout.Width( 16f ) ) ;
				if( view.IsReverse != isReverse )
				{
					Undo.RecordObject( view, "UIProgressbar : IsReverse Change" ) ;	// アンドウバッファに登録
					view.IsReverse  = isReverse ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "IsReverse", GUILayout.Width( 120f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			// 即値
			float number = EditorGUILayout.FloatField( "Number",  view.Number ) ;
			if( number != view.Number )
			{
				Undo.RecordObject( target, "UIProgressbar : Number Change" ) ;	// アンドウバッファに登録
				view.Number = number ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 形状タイプ
			var shapeType = ( UIProgressbar.ShapeTypes )EditorGUILayout.EnumPopup( "Shape Type",  view.ShapeType ) ;
			if( view.ShapeType != shapeType )
			{
				Undo.RecordObject( view, "UIProgressbar : ShapeType Change" ) ;	// アンドウバッファに登録
				view.ShapeType = shapeType ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.ShapeType == UIProgressbar.ShapeTypes.Rectangle )
			{
				// バーの表示タイプ
				var displayType = ( UIProgressbar.DisplayTypes )EditorGUILayout.EnumPopup( "Display Type",  view.DisplayType ) ;
				if( displayType != view.DisplayType )
				{
					Undo.RecordObject( view, "UIProgressbar : Display Type Change" ) ;	// アンドウバッファに登録
					view.DisplayType = displayType ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			else
			{
				// リングの反転
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					var thumbClockwise = EditorGUILayout.Toggle( view.ThumbClockwise, GUILayout.Width( 16f ) ) ;
					if( view.ThumbClockwise != thumbClockwise )
					{
						Undo.RecordObject( view, "UIProgressbar : ThumbClockwise Change" ) ;	// アンドウバッファに登録
						view.ThumbClockwise  = thumbClockwise ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "ThumbClockwise", GUILayout.Width( 120f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}


			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			var scope = EditorGUILayout.ObjectField( "Scope", view.Scope, typeof( UIImage ), true ) as UIImage ;
			if( scope != view.Scope )
			{
				Undo.RecordObject( view, "UIProgressbar : Scope Change" ) ;	// アンドウバッファに登録
				view.Scope = scope ;
				EditorUtility.SetDirty( view ) ;
			}

			var thumb = EditorGUILayout.ObjectField( "Thumb", view.Thumb, typeof( UIImage ), true ) as UIImage ;
			if( thumb != view.Thumb )
			{
				Undo.RecordObject( target, "UIProgressbar : Thumb Change" ) ;	// アンドウバッファに登録
				view.Thumb = thumb ;
				EditorUtility.SetDirty( view ) ;
			}

			var labelMesh = EditorGUILayout.ObjectField( "Label", view.LabelMesh, typeof( UINumberMesh ), true ) as UINumberMesh ;
			if( labelMesh != view.LabelMesh )
			{
				Undo.RecordObject( view, "UIProgressbar : LabelMesh Change" ) ;	// アンドウバッファに登録
				view.LabelMesh = labelMesh ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
