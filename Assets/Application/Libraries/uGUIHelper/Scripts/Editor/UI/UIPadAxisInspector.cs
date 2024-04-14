#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIPadAxis のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIPadAxis ) ) ]
	public class UIPadAxisInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			var view = target as UIPadAxis ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			var frame = EditorGUILayout.ObjectField( new GUIContent( "Frame", "外側部分のインスタンス参照です" ), view.Frame, typeof( UIImage ), true ) as UIImage ;
			if( view.Frame != frame )
			{
				Undo.RecordObject( view, "UIPadAxis Frame : Change" ) ;	// アンドウバッファに登録
				view.Frame = frame ;
				EditorUtility.SetDirty( view ) ;
			}

			var thumb = EditorGUILayout.ObjectField( new GUIContent( "Thumb", "外側部分のインスタンス参照です" ), view.Thumb, typeof( UIImage ), true ) as UIImage ;
			if( view.Thumb != thumb )
			{
				Undo.RecordObject( view, "UIPadAxis Thumb : Change" ) ;	// アンドウバッファに登録
				view.Thumb = thumb ;
				EditorUtility.SetDirty( view ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var yAxisInversion = EditorGUILayout.Toggle( view.YAxisInversion, GUILayout.Width( 16f ) ) ;
				if( view.YAxisInversion != yAxisInversion )
				{
					Undo.RecordObject( view, "UIPadAxis : Y Axis Inversion Change" ) ;	// アンドウバッファに登録
					view.YAxisInversion = yAxisInversion ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Y Axis Inversion", "縦方向の取得値の符号を反転します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var horizontalFunctionStop = EditorGUILayout.Toggle( view.HorizontalFunctionStop, GUILayout.Width( 16f ) ) ;
				if( view.HorizontalFunctionStop != horizontalFunctionStop )
				{
					Undo.RecordObject( view, "UIPadAxis : Horizontal Function Stop Change" ) ;	// アンドウバッファに登録
					view.HorizontalFunctionStop = horizontalFunctionStop ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Horizontal Function Stop", "横方向の入力を無効化します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var verticalFunctionStop = EditorGUILayout.Toggle( view.VerticalFunctionStop, GUILayout.Width( 16f ) ) ;
				if( view.VerticalFunctionStop != verticalFunctionStop )
				{
					Undo.RecordObject( view, "UIPadAxis : Vertical Function Stop Change" ) ;	// アンドウバッファに登録
					view.VerticalFunctionStop = verticalFunctionStop ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Vertical Function Stop", "縦方向の入力を無効化します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var alwaysDisplay = EditorGUILayout.Toggle( view.AlwaysDisplay, GUILayout.Width( 16f ) ) ;
				if( view.AlwaysDisplay != alwaysDisplay )
				{
					Undo.RecordObject( view, "UIPadAxis : Always Display Change" ) ;	// アンドウバッファに登録
					view.AlwaysDisplay = alwaysDisplay ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Always Display", "常に表示します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.AlwaysDisplay == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					var interactionRangeEnabled = EditorGUILayout.Toggle( view.InteractionRangeEnabled, GUILayout.Width( 16f ) ) ;
					if( view.InteractionRangeEnabled != interactionRangeEnabled )
					{
						Undo.RecordObject( view, "UIPadAxis : Interaction Range Enabled Change" ) ;	// アンドウバッファに登録
						view.InteractionRangeEnabled = interactionRangeEnabled ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( new GUIContent( "Interaction Range Enabled", "タッチの有効範囲をビュー内に限定します" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var shapeType = ( UIPadAxis.ShapeTypes )EditorGUILayout.EnumPopup( new GUIContent( "Shape Type", "ビュー内のタッチ有効形状を設定します" ), view.ShapeType ) ;
				if( view.ShapeType != shapeType )
				{
					Undo.RecordObject( view, "UIPadAxis : ShapeType Change" ) ;	// アンドウバッファに登録
					view.ShapeType = shapeType ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var surplusInputEnabled = EditorGUILayout.Toggle( view.SurplusInputEnabled, GUILayout.Width( 16f ) ) ;
				if( view.SurplusInputEnabled != surplusInputEnabled )
				{
					Undo.RecordObject( view, "UIPadAxis : Surplus Input Enabled Change" ) ;	// アンドウバッファに登録
					view.SurplusInputEnabled  = surplusInputEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Surplus Input Enabled", "レイキャストブロック後の最後の入力を継続させます" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

#endif
