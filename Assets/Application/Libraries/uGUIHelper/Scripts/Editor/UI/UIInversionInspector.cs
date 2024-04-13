#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;

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

			// 反転方向タイプ
			UIInversion.DirectionTypes direction = ( UIInversion.DirectionTypes )EditorGUILayout.EnumPopup( "Direction",  view.DirectionType ) ;
			if( direction != view.DirectionType )
			{
				Undo.RecordObject( view, "UIInversion : Direction Type Change" ) ;	// アンドウバッファに登録
				view.DirectionType = direction ;
				EditorUtility.SetDirty( view ) ;
			}

			// 回転方向タイプ
			UIInversion.RotationTypes rotation = ( UIInversion.RotationTypes )EditorGUILayout.EnumPopup( "Rotation",  view.RotationType ) ;
			if( rotation != view.RotationType )
			{
				Undo.RecordObject( view, "UIInversion : Rptation Type Change" ) ;	// アンドウバッファに登録
				view.RotationType = rotation ;
				EditorUtility.SetDirty( view ) ;
			}

		}
	}
}

#endif
