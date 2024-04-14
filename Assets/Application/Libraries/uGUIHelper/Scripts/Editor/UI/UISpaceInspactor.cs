#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;


namespace uGUIHelper
{
	/// <summary>
	/// UICanvas のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISpace ) ) ]
	public class UISpaceInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UISpace view = target as UISpace ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			GUI.backgroundColor = Color.green ;
			Transform worldRoot = EditorGUILayout.ObjectField( "World Root", view.WorldRoot, typeof( Transform ), true ) as Transform ;
			GUI.backgroundColor = Color.white ;
			if( worldRoot != view.WorldRoot )
			{
				Undo.RecordObject( view, "UISpace : World Root Change" ) ;	// アンドウバッファに登録
				view.WorldRoot = worldRoot ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.WorldRoot != null )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// 実行時のレイヤーの強制設定
					bool isForceWorldLayer = EditorGUILayout.Toggle( view.IsForceWorldLayer, GUILayout.Width( 16f ) ) ;
					if( isForceWorldLayer != view.IsForceWorldLayer )
					{
						Undo.RecordObject( view, "UISpace : IsForceWorldLayer Change" ) ;	// アンドウバッファに登録
						view.IsForceWorldLayer = isForceWorldLayer ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Is Force World Layer" ) ;

					//-------------

					if( view.IsForceWorldLayer == true )
					{
						string[] layerNames = new string[ 32 ] ;
						int layer ;
						for( layer  = 0 ; layer <  layerNames.Length ; layer ++ )
						{
							layerNames[ layer ] = layer.ToString( "D2" ) + " : " + LayerMask.LayerToName( layer ) ;
						}

						int targetLayer = EditorGUILayout.Popup( view.WorldLayer, layerNames ) ;
						if( targetLayer != view.WorldLayer )
						{
							Undo.RecordObject( view, "UISpace : World Layer Change" ) ;	// アンドウバッファに登録
							view.WorldLayer = targetLayer ;
							EditorUtility.SetDirty( view ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUI.backgroundColor = Color.magenta ;
			Camera targetCamera = EditorGUILayout.ObjectField( "Target Camera", view.TargetCamera, typeof( Camera ), true ) as Camera ;
			GUI.backgroundColor = Color.white ;
			if( targetCamera != view.TargetCamera )
			{
				Undo.RecordObject( view, "UISpace : Target Camera Change" ) ;	// アンドウバッファに登録
				view.TargetCamera = targetCamera ;
				EditorUtility.SetDirty( view ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( view.TargetCamera != null )
			{
				Vector2 cameraOffset = EditorGUILayout.Vector2Field( "Camera Offset", view.CameraOffset ) ;
				if( cameraOffset.Equals( view.CameraOffset ) == false )
				{
					Undo.RecordObject( view, "UISpace : Camera Offset Change" ) ;	// アンドウバッファに登録
					view.CameraOffset = cameraOffset ;
					EditorUtility.SetDirty( view ) ;
				}
			}


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool flexibleFieldOfView = EditorGUILayout.Toggle( view.FlexibleFieldOfView, GUILayout.Width( 16f ) ) ;
				if( flexibleFieldOfView != view.FlexibleFieldOfView )
				{
					Undo.RecordObject( view, "UISpace : Flexible Field Of View Change" ) ;	// アンドウバッファに登録
					view.FlexibleFieldOfView = flexibleFieldOfView ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Flexible Field Of View" ) ;

				if( view.FlexibleFieldOfView == true )
				{
					float basisHeight = EditorGUILayout.FloatField( "Basis Height", view.BasisHeight ) ;
					if( basisHeight != view.BasisHeight )
					{
						Undo.RecordObject( view, "UISpace : Basis Height Change" ) ;	// アンドウバッファに登録
						view.BasisHeight = basisHeight ;
						EditorUtility.SetDirty( view ) ;
					}
				}

			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool renderTextureEnabled = EditorGUILayout.Toggle( view.RenderTextureEnabled, GUILayout.Width( 16f ) ) ;
				if( renderTextureEnabled != view.RenderTextureEnabled )
				{
					Undo.RecordObject( view, "UISpace : Render Texture Enabled Change" ) ;	// アンドウバッファに登録
					view.RenderTextureEnabled = renderTextureEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Render Texture Enabled" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			if( view.RenderTextureEnabled == true )
			{
				// 設定されているイメージ
				GUI.contentColor = Color.cyan ;	// ボタンの下地を緑に
				EditorGUILayout.ObjectField( "Raw Image", view.Image, typeof( UIRawImage ), false ) ;
				GUI.contentColor = Color.white ;	// ボタンの下地を緑に

				// 強制更新
				GUI.backgroundColor = Color.cyan ;	// ボタンの下地を緑に
				if( GUILayout.Button( "Force Render", GUILayout.Width( 140f ) ) == true )
				{
					view.Render() ;
				}
				GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に


				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isMask = EditorGUILayout.Toggle( view.ImageMask, GUILayout.Width( 16f ) ) ;
					if( isMask != view.ImageMask )
					{
						Undo.RecordObject( view, "UISpace : Mask Change" ) ;	// アンドウバッファに登録
						view.ImageMask = isMask ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Mask" ) ;

					bool isInversion = EditorGUILayout.Toggle( view.ImageInversion, GUILayout.Width( 16f ) ) ;
					if( isInversion != view.ImageInversion )
					{
						Undo.RecordObject( view, "UISpace : Image Inversion Change" ) ;	// アンドウバッファに登録
						view.ImageInversion = isInversion ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Inversion" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isShadow = EditorGUILayout.Toggle( view.ImageShadow, GUILayout.Width( 16f ) ) ;
					if( isShadow != view.IsShadow )
					{
						Undo.RecordObject( view, "UISpace : Shadow Change" ) ;	// アンドウバッファに登録
						view.ImageShadow = isShadow ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Shadow" ) ;

					bool isOutline = EditorGUILayout.Toggle( view.ImageOutline, GUILayout.Width( 16f ) ) ;
					if( isOutline != view.ImageOutline )
					{
						Undo.RecordObject( view, "UISpace : Outline Change" ) ;	// アンドウバッファに登録
						view.ImageOutline = isOutline ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Outline" ) ;

					bool isGradient = EditorGUILayout.Toggle( view.ImageGradient, GUILayout.Width( 16f ) ) ;
					if( isGradient != view.ImageGradient )
					{
						Undo.RecordObject( view, "UISpace : Gradient Change" ) ;	// アンドウバッファに登録
						view.ImageGradient = isGradient ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Gradient" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;
			}

		}
	}
}

#endif
