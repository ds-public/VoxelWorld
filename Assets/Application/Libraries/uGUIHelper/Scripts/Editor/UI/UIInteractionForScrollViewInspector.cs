#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageInversion のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInteractionForScrollView ) ) ]
	public class UIInteractionForScrollViewInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIInteractionForScrollView interaction = target as UIInteractionForScrollView ;

			UIView view = interaction.GetComponent<UIView>() ;
			if( view != null )
			{
				EditorGUILayout.Separator() ;   // 少し区切りスペース

				// クリックの排他制御
				bool clickExclusionEnabled = EditorGUILayout.Toggle( "Click Exclusion Enabled", view.ClickExclusionEnabled ) ;
				if( clickExclusionEnabled != view.ClickExclusionEnabled )
				{
					Undo.RecordObject( view, "UIInteraction : Click Exclusion Enabled Change" ) ;	// アンドウバッファに登録
					view.ClickExclusionEnabled = clickExclusionEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
			}
		}
	}
}

#endif
