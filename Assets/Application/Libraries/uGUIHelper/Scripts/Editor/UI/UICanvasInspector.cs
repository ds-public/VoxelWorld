#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;

namespace uGUIHelper
{
	/// <summary>
	/// UICanvas のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UICanvas ) ) ]
	public class UICanvasInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			UICanvas view = target as UICanvas ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( view ) ;
		}

		// キャンバスグループの設定項目を描画する
		private void DrawCanvasGroup( UICanvas view )
		{
			// キャンバスグループを有効にするかどうか

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isOverlay = EditorGUILayout.Toggle( view.IsOverlay, GUILayout.Width( 16f ) ) ;
				if( isOverlay != view.IsOverlay )
				{
					Undo.RecordObject( view, "UICanvas : Is Overlay Change" ) ;	// アンドウバッファに登録
					view.IsOverlay = isOverlay ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Overlay" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCanvasGroup = EditorGUILayout.Toggle( view.IsCanvasGroup, GUILayout.Width( 16f ) ) ;
				if( isCanvasGroup != view.IsCanvasGroup )
				{
					Undo.RecordObject( view, "UICanvas : Canvas Group Change" ) ;	// アンドウバッファに登録
					view.IsCanvasGroup = isCanvasGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Canvas Group" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.IsCanvasGroup == true )
			{
				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float alpha = EditorGUILayout.Slider( "Alpha", view.GetCanvasGroup().alpha, 0.0f, 1.0f ) ;
				if( alpha != view.GetCanvasGroup().alpha )
				{
					Undo.RecordObject( view, "UIView : Alpha Change" ) ;	// アンドウバッファに登録
					view.GetCanvasGroup().alpha = alpha ;
					EditorUtility.SetDirty( view ) ;
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float disableRaycastUnderAlpha = EditorGUILayout.Slider( "Disable Raycast Under Alpha", view.DisableRaycastUnderAlpha, 0.0f, 1.0f ) ;
				if( disableRaycastUnderAlpha != view.DisableRaycastUnderAlpha )
				{
					Undo.RecordObject( view, "UIView : Disable Raycast Under Alpha Change" ) ;	// アンドウバッファに登録
					view.DisableRaycastUnderAlpha = disableRaycastUnderAlpha ;
					EditorUtility.SetDirty( view ) ;
				}
			}
		}
	}
}

#endif
