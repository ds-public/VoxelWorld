#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UISlider のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISlider ) ) ]
	public class UISliderInspector : UIViewInspector
	{
		protected override  void DrawInspectorGUI()
		{
			UISlider view = target as UISlider ;

            //-------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			UIImage cursor = EditorGUILayout.ObjectField( "Cursor", view.Cursor, typeof( UIImage ), true ) as UIImage ;
			if( cursor != view.Cursor )
			{
				Undo.RecordObject( view, "UISlider : Cursor Change" ) ;	// アンドウバッファに登録
				view.Cursor = cursor ;
				EditorUtility.SetDirty( view ) ;
			}

			if( view.Cursor != null && view.Cursor == view.PointerCursor )
			{
				EditorGUILayout.HelpBox( GetMessage( "SameCursor" ), MessageType.Info, true ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		}

		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "SameCursor",			    "カーソルが重複しています。場合によっては意図しない挙動となる可能性があるため、異なるものを設定する事を推奨します。" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "SameCursor",			    "The cursors are duplicated. This may lead to unintended behavior in some cases, so it is recommended to set different cursors." },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ] ;
			}
		}


	}   // class
}   // namespace

#endif
