using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;


namespace GREE
{
	/// <summary>
	/// WebViewObject のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( WebViewObject ) ) ]
	public class WebViewObjectInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
			DrawDefaultInspector() ;
			
			//--------------------------------------------
		
			// ターゲットのインスタンス
			WebViewObject tTarget = target as WebViewObject ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//-----------------------

			if( GUILayout.Button( "コールバック呼び出し(デバッグ用)" ) == true )
			{
				tTarget.Call() ;
			}
		}
	}
}