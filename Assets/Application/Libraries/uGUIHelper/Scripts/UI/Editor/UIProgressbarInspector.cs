using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

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
			UIProgressbar tTarget = target as UIProgressbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// スライダーでアルファをコントロール出来るようにする
			float tValue = EditorGUILayout.Slider( "Value", tTarget.Value, 0.0f, 1.0f ) ;
			if( tValue != tTarget.Value )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Value Change" ) ;	// アンドウバッファに登録
				tTarget.Value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// 即値
			float tNumber = EditorGUILayout.FloatField( "Number",  tTarget.Number ) ;
			if( tNumber != tTarget.Number )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Number Change" ) ;	// アンドウバッファに登録
				tTarget.Number = tNumber ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 頂点密度
			UIProgressbar.DisplayTypes tDisplayType = ( UIProgressbar.DisplayTypes )EditorGUILayout.EnumPopup( "Display Type",  tTarget.DisplayType ) ;
			if( tDisplayType != tTarget.DisplayType )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Display Type Change" ) ;	// アンドウバッファに登録
				tTarget.DisplayType = tDisplayType ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}



			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIImage tScope= EditorGUILayout.ObjectField( "Scope", tTarget.Scope, typeof( UIImage ), true ) as UIImage ;
			if( tScope != tTarget.Scope )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Scope Change" ) ;	// アンドウバッファに登録
				tTarget.Scope = tScope ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UIImage tThumb = EditorGUILayout.ObjectField( "Thumb", tTarget.Thumb, typeof( UIImage ), true ) as UIImage ;
			if( tThumb != tTarget.Thumb )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Thumb Change" ) ;	// アンドウバッファに登録
				tTarget.Thumb = tThumb ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UINumber tLabel = EditorGUILayout.ObjectField( "Label", tTarget.Label, typeof( UINumber ), true ) as UINumber ;
			if( tLabel != tTarget.Label )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Label Change" ) ;	// アンドウバッファに登録
				tTarget.Label = tLabel ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}
	}
}

