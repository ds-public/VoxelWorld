#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace DSW.Screens.nBoot
{
	/// <summary>
	/// Boot のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Boot ) ) ]
	public class BootInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
			DrawDefaultInspector() ;

			// ターゲットのインスタンス
			Boot boot = target as Boot ;
			
			GUI.backgroundColor = Color.cyan ;
			if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
			{
				Undo.RecordObject( boot, "Boot : Refresh" ) ;	// アンドウバッファに登録
				boot.Refresh() ;
				EditorUtility.SetDirty( boot ) ;
			}
			GUI.backgroundColor = Color.white ;
		}
	}
}

#endif

