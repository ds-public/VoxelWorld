#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIRawImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIRawImage ) ) ]
	public class UIRawImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIRawImage view = target as UIRawImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// マテリアル選択
			DrawMaterial( view ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var blurIntensity = ( UIRawImage.BlurIntensities )EditorGUILayout.EnumPopup( "Blur Intensity", view.BlurIntensity ) ;
				if( blurIntensity != view.BlurIntensity )
				{
					Undo.RecordObject( view, "UIRawImage : Blur Intensity Change" ) ;	// アンドウバッファに登録
					view.BlurIntensity = blurIntensity ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.BlurIntensity == UIRawImage.BlurIntensities.None )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool autoCreateDrawableTexture = EditorGUILayout.Toggle( view.AutoCreateDrawableTexture, GUILayout.Width( 16f ) ) ;
					if( autoCreateDrawableTexture != view.AutoCreateDrawableTexture )
					{
						Undo.RecordObject( view, "UIRawImage : Auto Create DrawableTexture Change" ) ;	// アンドウバッファに登録
						view.AutoCreateDrawableTexture = autoCreateDrawableTexture ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Auto Create Drawable Texture" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isFlipVertical = EditorGUILayout.Toggle( view.IsFlipVertical, GUILayout.Width( 16f ) ) ;
					if( isFlipVertical != view.IsFlipVertical )
					{
						Undo.RecordObject( view, "UIRawImage : Is Flip Vertical Change" ) ;	// アンドウバッファに登録
						view.IsFlipVertical = isFlipVertical ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Is Flip Vertical" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}
	}
}

#endif
