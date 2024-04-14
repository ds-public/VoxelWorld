#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIPadButton ) ) ]
	public class UIPadButtonInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			var view = target as UIPadButton ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//------------------------------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlas( view ) ;

			// Flipper の追加と削除
			DrawFlipper( view ) ;

			// マテリアル選択
			DrawMaterial( view ) ;

			//------------------------------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			EditorGUIUtility.labelWidth = 200f ;

			// コリジョン形状タイプ
			var collisionShapeType = ( UIPadButton.CollisionShapeTypes )EditorGUILayout.EnumPopup( "Collision Shape Type",  view.CollisionShapeType ) ;
			if( collisionShapeType != view.CollisionShapeType )
			{
				Undo.RecordObject( view, "UIPadButton : Collision Shape Type Change" ) ;	// アンドウバッファに登録
				view.CollisionShapeType = collisionShapeType ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.CollisionShapeType == UIPadButton.CollisionShapeTypes.Circle )
			{
				// コリジョン形状に円形指定の場合はボリュームを調整できる

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Collision Volume Ratio", GUILayout.Width( 200f ) ) ;

					float collisionVolumeRatio = EditorGUILayout.Slider( view.CollisionVolumeRatio, 0.1f, 5.0f ) ;
					if( collisionVolumeRatio != view.CollisionVolumeRatio )
					{
						Undo.RecordObject( view, "UIPadButton : Collision Volume Ratio Change" ) ;	// アンドウバッファに登録
						view.CollisionVolumeRatio = collisionVolumeRatio ;
						EditorUtility.SetDirty( view ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//----------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Repeat Press Starting Time", GUILayout.Width( 200f ) ) ;

				float repeatPressStartingTime = EditorGUILayout.Slider( view.RepeatPressStartingTime, 0.1f, 10.0f ) ;
				if( repeatPressStartingTime != view.RepeatPressStartingTime )
				{
					Undo.RecordObject( view, "UIPadButton : Repeat Press Starting Time Change" ) ;	// アンドウバッファに登録
					view.RepeatPressStartingTime = repeatPressStartingTime ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Repeat Press Interval Time", GUILayout.Width( 200f ) ) ;

				float repeatPressIntervalTime = EditorGUILayout.Slider( view.RepeatPressIntervalTime, 0.1f, 10.0f ) ;
				if( repeatPressIntervalTime != view.RepeatPressIntervalTime )
				{
					Undo.RecordObject( view, "UIPadButton : Repeat Interval Time Change" ) ;	// アンドウバッファに登録
					view.RepeatPressIntervalTime = repeatPressIntervalTime ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Long Press Decision Time", GUILayout.Width( 200f ) ) ;

				float longPressDecisionTime = EditorGUILayout.Slider( view.LongPressDecisionTime, 0.1f, 10.0f ) ;
				if( longPressDecisionTime != view.LongPressDecisionTime )
				{
					Undo.RecordObject( view, "UIPadButton : Long Press Decision Time Change" ) ;	// アンドウバッファに登録
					view.LongPressDecisionTime = longPressDecisionTime ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		}
	}
}

#endif
