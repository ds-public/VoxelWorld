using System ;
using System.IO ;
using System.Collections.Generic ;
using UnityEditor ;

// Assembly に Unity.2D.Sprite.Editor が必要
using UnityEditor.U2D.Sprites ;

using UnityEngine ;


/// <summary>
/// ゲームオブジェクトカウンター
/// </summary>
namespace Tools.ForGameObject
{
	/// <summary>
	/// ゲームオブジェクトカウンター(エディター用) Version 2024/04/22 0
	/// </summary>
	public class GameObjectCounter : EditorWindow
	{
		[ MenuItem( "Tools/Game Object Counter(GameObject 数の確認)" ) ]
		internal static void OpenWindow()
		{
			EditorWindow.GetWindow<GameObjectCounter>( false, "Game Object Counter", true ) ;
		}
	
		//----------------------------------------------------------

		private Vector2 m_Scroll ;


		// レイアウトを描画する
		internal void OnGUI()
		{
			if( Selection.gameObjects != null && Selection.gameObjects.Length >  0 )
			{
				( var count_a, var count_d ) = GetCount( Selection.gameObjects ) ;

				int count_t = count_a + count_d ;

				GUI.contentColor = new Color32( 255, 255,   0, 255 ) ;
				GUILayout.Label( GetMessage( "Selected GameObject" ), GUILayout.Width( 200f ) ) ;
				GUI.contentColor = Color.white ;

				//---------------------------------

				// スクロールビューで表示する
				m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
				{
					// 表示が必要な箇所だけ表示する
					GUI.color = new Color32( 255, 127,   0, 255 ) ;
					foreach( var go in Selection.gameObjects )
					{
						// アセット情報
						GUILayout.BeginHorizontal() ;
						{
							// 横一列
							EditorGUILayout.TextField( GetHierarchyPath( go ) ) ;
						}
						GUILayout.EndHorizontal() ;
					}
					GUI.color = Color.white ;
				}
				GUILayout.EndScrollView() ;

				//---------------------------------

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.contentColor = new Color32(   0, 255,   0, 255 ) ;
					GUILayout.Label( GetMessage( "Total count" ), GUILayout.Width( 80f ) ) ;
					GUI.contentColor = Color.white ;

					GUILayout.FlexibleSpace() ;	// ＵＩ自体の右寄せ
					EditorGUILayout.IntField( count_t, GUILayout.Width( 100f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.contentColor = new Color32(   0, 255, 255, 255 ) ;
					GUILayout.Label( GetMessage( "Active count" ), GUILayout.Width( 80f ) ) ;
					GUI.contentColor = Color.white ;

					GUILayout.FlexibleSpace() ;	// ＵＩ自体の右寄せ
					EditorGUILayout.IntField( count_a, GUILayout.Width( 100f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.contentColor = new Color32( 255,   0,   0, 255 ) ;
					GUILayout.Label( GetMessage( "Deactive count" ), GUILayout.Width( 80f ) ) ;
					GUI.contentColor = Color.white ;

					GUILayout.FlexibleSpace() ;	// ＵＩ自体の右寄せ
					EditorGUILayout.IntField( count_d, GUILayout.Width( 100f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				EditorGUILayout.HelpBox( GetMessage( "Please select GameObject(s)" ), MessageType.Info ) ;
			}
		}

		// ヒエラルキーのパスを取得する
		private string GetHierarchyPath( GameObject go )
		{
			string path = string.Empty ;

			Transform node = go.transform ;

			do
			{
				if( string.IsNullOrEmpty( path ) == true )
				{
					path = node.name ;
				}
				else
				{
					path = $"{node.name}/{path}" ;
				}

				node = node.parent ;
			}
			while( node != null ) ;

			return path ;
		}

		internal void OnSelectionChange() 
		{
			Repaint() ;
		}

		private ( int, int ) GetCount( GameObject[] targets )
		{
			int count_a = 0 ;
			int count_d = 0 ;

			var checkedTargets = new List<GameObject>() ;

			int i, l = targets.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var target = targets[ i ] ;

				if( checkedTargets.Contains( target ) == false )
				{
					if( target.activeSelf == true )
					{
						count_a ++ ;
					}
					else
					{
						count_d ++ ;
					}

					int ci, cl = target.transform.childCount ;
					if( cl >   0 )
					{
						for( ci  = 0 ; ci <  cl ; ci ++ )
						{
							GetCount( target.transform.GetChild( ci ).gameObject, ref count_a, ref count_d, checkedTargets ) ;
						}
					}

					checkedTargets.Add( target ) ;
				}
			}

			return ( count_a, count_d ) ;
		}

		private void GetCount( GameObject target, ref int count_a, ref int count_d, List<GameObject> checkedTargts )
		{
			if( checkedTargts.Contains( target ) == false )
			{
				if( target.activeSelf == true )
				{
					count_a ++ ;
				}
				else
				{
					count_d ++ ;
				}

				int ci, cl = target.transform.childCount ;
				if( cl >   0 )
				{
					for( ci  = 0 ; ci <  cl ; ci ++ )
					{
						GetCount( target.transform.GetChild( ci ).gameObject, ref count_a, ref count_d, checkedTargts ) ;
					}
				}

				checkedTargts.Add( target ) ;
			}
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "Please select GameObject(s)",	"Please select GameObject(s)." },
			{ "Selected GameObject",			"Selected GameObject" },
			{ "Total count",					"Total count" },
			{ "Active count",					"Active count" },
			{ "Deactive count",					"Deactive count" },
		} ;
		private readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "Please select GameObject(s)",	"GameObject を選択してください(複数可)" },
			{ "Selected GameObject",			"選択中の GameObject" },
			{ "Total count",					"合計数" },
			{ "Active count",					"有効数" },
			{ "Deactive count",					"無効数" },
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
	}
}

