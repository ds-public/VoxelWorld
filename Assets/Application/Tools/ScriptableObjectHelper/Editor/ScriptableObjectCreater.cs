using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;



namespace ScriptableObjectHelper
{
	/// <summary>
	/// スクリプタブルオブジェクト生成ツールクラス Version 2016/11/18 0
	/// </summary>
	public class ScriptableObjectCreater : EditorWindow
	{
		[ MenuItem( "Tools/ScriptableObject Creater" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<ScriptableObjectCreater>( false, "ScriptableObject Creater", true ) ;
		}

		private bool m_FullTypeName = false ;
		private int m_Index = 0 ;

		private string	m_OutputPath = "Assets/" ;
		private string	m_OutputName = "" ;

		private const string m_Extension = "asset" ;

		// レイアウトを描画する
		private void OnGUI()
		{
			int i, l, p ;

			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Output Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_OutputPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						string tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( System.IO.Directory.Exists( tPath ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							tPath = tPath.Replace( "\\", "/" ) ;
							m_OutputPath = tPath + "/" ;
						}
						else
						{
							// ファイルを指定しています
							tPath = tPath.Replace( "\\", "/" ) ;
							m_OutputPath = tPath ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							i = m_OutputPath.LastIndexOf( '/' ) ;
							if( i >= 0 )
							{
								m_OutputPath = m_OutputPath.Substring( 0, i ) + "/" ;
							}
						}
					}
				}
			
				// 出力パス
				m_OutputPath = EditorGUILayout.TextField( m_OutputPath ) ;

				// 出力名前
				m_OutputName = EditorGUILayout.TextField( m_OutputName, GUILayout.Width( 160f ) ) ;

				// 拡張子
				EditorGUILayout.LabelField( "." + m_Extension, GUILayout.Width( 45f ) ) ;
			}
			GUILayout.EndHorizontal() ;

			GUILayout.Space( 6f ) ;

			//----------------------------------------------------------

			Type[] tTypes = GetScriptableObjectClassTypes() ;
			if( tTypes == null )
			{
				EditorGUILayout.HelpBox( "ScriptableObject継承クラスがプロジェクトに存在しません", MessageType.Warning ) ;
				return ;
			}

			// 名前を全て表示するかどうか
			m_FullTypeName = EditorGUILayout.Toggle( "Full Type Name", m_FullTypeName ) ;

			l = tTypes.Length ;
			string[] tTypeNames = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tTypeNames[ i ] = tTypes[ i ].ToString() ;
				if( m_FullTypeName == false )
				{
					p = tTypeNames[ i ].LastIndexOf( "." ) ;
					if( p >= 0 )
					{
						tTypeNames[ i ] = tTypeNames[ i ].Substring( p + 1, tTypeNames[ i ].Length - ( p + 1 ) ) ;
					}
				}
			}

			int tIndex = EditorGUILayout.Popup( "Type", m_Index, tTypeNames ) ;  // フィールド名有りタイプ
			if( tIndex != m_Index )
			{
				m_Index = tIndex ;

				if( string.IsNullOrEmpty( m_OutputName ) == true )
				{
					m_OutputName = GetShortName( tTypes[ m_Index ].ToString() ) ;
				}
			}

			if( m_FullTypeName == false )
			{
				GUILayout.BeginHorizontal() ;
				{	
					EditorGUILayout.LabelField( "Type(Full)", GUILayout.Width( 148f ) ) ;
					GUI.color = Color.yellow ;
					EditorGUILayout.LabelField( tTypes[ m_Index ].ToString() ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;
			}

			if( string.IsNullOrEmpty( m_OutputPath ) == false && Directory.Exists( m_OutputPath ) == true && string.IsNullOrEmpty( m_OutputName ) == false )
			{
				string tPath = m_OutputPath ;
				if( tPath[ tPath.Length - 1 ] != '/' )
				{
					tPath = tPath + "/" ;
 				}

				string tPathName = tPath + m_OutputName +"." + m_Extension ;
				if( File.Exists( tPathName ) == true )
				{
					EditorGUILayout.HelpBox( "既に同名のファイルが存在します", MessageType.Warning ) ;
				}
				else
				{
					GUI.backgroundColor = Color.green ;
					if( GUILayout.Button( "Create" ) == true )
					{
						// 生成
						ScriptableObject tScriptableObject = ScriptableObject.CreateInstance( tTypes[ m_Index ] )  ;
						tScriptableObject.name = GetShortName( tTypes[ m_Index ].ToString() ) ;
			
						AssetDatabase.CreateAsset( tScriptableObject, tPathName ) ;
						AssetDatabase.Refresh() ;
			
						Selection.activeObject = tScriptableObject ;
					}
				}
			}
		}

		// 短縮名を取得する
		private string GetShortName( string tName )
		{
			int p = tName.LastIndexOf( "." ) ;
			if( p >= 0 )
			{
				tName = tName.Substring( p + 1, tName.Length - ( p + 1 ) ) ;
			}

			return tName ;
		}

		//-----------------------------------------------------------

		private Dictionary<string, List<Type>> m_ClassTypes ;
		private MonoScript[] m_MonoScripts ;

		/// <summary>
		/// プロジェクト内に存在する全スクリプトファイル
		/// </summary>
		private MonoScript[] _MonoScripts
		{
			get
			{
				return m_MonoScripts ?? ( m_MonoScripts = Resources.FindObjectsOfTypeAll<MonoScript>() ) ;
			}
		}
/*
		/// <summary>
		/// クラス名からタイプを取得する
		/// </summary>
		public Type GetClassType( string className )
		{
			if( m_ClassTypes == null )
			{
				// Dictionary作成
				m_ClassTypes = new Dictionary<string, List<Type>>() ;
				foreach( Type tType in GetAllTypes() )
				{
					if( !m_ClassTypes.ContainsKey( tType.Name ) )
					{
						m_ClassTypes.Add( tType.Name, new List<Type>() ) ;
					}
					m_ClassTypes[ tType.Name ].Add( tType ) ;
				}
			}

			if( m_ClassTypes.ContainsKey( className ) ) // クラスが存在
			{
				return m_ClassTypes[ className ][ 0 ] ;
			}
			else
			{
				// クラスが存在しない場合
				return null ;
			}
		}*/

		private Type[] GetScriptableObjectClassTypes()
		{
			List<Type> tList = new List<Type>() ;

			foreach( Type tType in GetAllTypes() )
			{
				if( tType.IsSubclassOf( typeof( ScriptableObject ) ) )
				{
					tList.Add( tType ) ;
				}
			}

			if( tList.Count == 0 )
			{
				return null ;
			}

			return tList.ToArray() ;
		}




		/// <summary>
		/// 全てのクラスタイプを取得
		/// </summary>
		private IEnumerable<Type> GetAllTypes()
		{
			// Unity標準のクラスタイプ
			IEnumerable<Type> tBuitinTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany( asm => asm.GetTypes() )
			.Where( type => type != null && !string.IsNullOrEmpty( type.Namespace ) )
			.Where( type => type.Namespace.Contains( "UnityEngine" ) ) ;

			// 自作のクラスタイプ
			IEnumerable<Type> tCustomTypes = _MonoScripts
			.Where( script => script != null )
			.Select( script => script.GetClass() )
			.Where( classType => classType != null )
			.Where( classType => classType.Module.Name == "Assembly-CSharp.dll" ) ;
			
			return tBuitinTypes.Concat( tCustomTypes ).Distinct() ;
		}
	}
}

