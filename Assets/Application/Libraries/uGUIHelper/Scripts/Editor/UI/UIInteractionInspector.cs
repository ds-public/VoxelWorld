#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageInversion のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInteraction ) ) ]
	public class UIInteractionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIInteraction interaction = target as UIInteraction ;

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
