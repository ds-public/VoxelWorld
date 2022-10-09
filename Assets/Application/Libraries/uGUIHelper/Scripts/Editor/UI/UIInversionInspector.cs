#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageInversion のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInversion ) ) ]
	public class UIInversionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIInversion view = target as UIInversion ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// バリュータイプ
			UIInversion.DirectionTypes direction = ( UIInversion.DirectionTypes )EditorGUILayout.EnumPopup( "Direction",  view.DirectionType ) ;
			if( direction != view.DirectionType )
			{
				Undo.RecordObject( view, "UIInversion : Direction Change" ) ;	// アンドウバッファに登録
				view.DirectionType = direction ;
				view.Refresh() ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif
