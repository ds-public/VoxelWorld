using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Security.Cryptography ;
using System.Text ;
using System.Linq ;

/// <summary>
/// シンプルアセットバンドルビルダーパッケージ
/// </summary>
namespace SimpleAssetBundleBuilder
{
	/// <summary>
	/// アセットバンドルビルダークラス(エディター用) Version 2019/09/11 0
	/// </summary>
	public class SimpleAssetBundleBuilder : EditorWindow
	{
		[ MenuItem( "Tools/Simple AssetBundle Builder" ) ]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleAssetBundleBuilder>( false, "AB Builder", true ) ;
		}

		//------------------------------------------------------------

		// アセットバンドル化するファイル単位の情報
		[System.Serializable]
		class AssetBundleFile
		{
			[System.Serializable]
			public class AssetFile
			{
				public string	AssetPath ;
				public int		AssetType ;	// 0だと直接含まれるアセットを表す

				public AssetFile( string assetPath, int assetType )
				{
					AssetPath = assetPath ;
					AssetType = assetType ;
				}
			}

			public string			AssetBundlePath ;						// 書き出すアセットバンドルのパス
			public List<AssetFile>	AssetFiles = new List<AssetFile>() ;	// 含めるアセットのパスのリスト

			public int				SourceType ;							// 0=ファイル　1=フォルダ

			public string[]			Tags ;									// アセットバンドルごとのタグ

			public void AddAssetFile( string assetPath, int assetType )
			{
				AssetFiles.Add( new AssetFile( assetPath, assetType ) ) ;
			}

			// 依存関係のあるアセットも対象に追加する
			public int CollectDependencies()
			{
				// 注意：必ず基本アセットを全て追加した後にこのメソッドを呼び出す事

				if( AssetFiles.Count == 0 )
				{
					return 0 ;
				}

				List<string> assetPaths = new List<string>() ;

				foreach( var assetFile in AssetFiles )
				{
					// 依存関係にあるアセットを検出する
					string[] checkPaths = AssetDatabase.GetDependencies( assetFile.AssetPath ) ;
					if( checkPaths != null && checkPaths.Length >  0 )
					{
						foreach( var checkPath in checkPaths )
						{
							if( AssetFiles.Any( _ => _.AssetPath == checkPath ) == false )
							{
								// 同アセットバンドルに含まれないものに限定する
								string extension = Path.GetExtension( checkPath ) ;
								if( extension != "cs" && extension != "js" )
								{
									// ソースファイル以外に限定する
									assetPaths.Add( checkPath ) ;
								}
							}
						}
					}
				}
				
				if( assetPaths.Count >  0 )
				{
					foreach( var assetPath in assetPaths )
					{
						AddAssetFile( assetPath, 1 ) ;	// 依存系のアセットのパスを追加する
					}
				}

				return assetPaths.Count ;
			}
		}

		//------------------------------------------------------------


		private string						m_ResourceListFilePath			= "" ;
		private string						m_ResourceRootFolderPath		= "" ;						// リソースルートパス
		private string						m_AssetBundleRootFolderPath		= "" ;						// アセットバンドルルートパス

		private BuildTarget					m_BuildTarget					= BuildTarget.Android ;		// ターゲットプラットフォーム
		private bool						m_ChunkBasedCompression			= true ;					// チャンクベースのフォーマットにするか
		private bool						m_ForceRebuildAssetBundle		= false ;					// 強制的に再生成するか
		private bool						m_IgnoreTypeTreeChanges			= false ;					// タイプツリーが変わっても無視(ソースコードを変更しても変化したとはみなされない)
		private bool						m_DisableWriteTypeTree			= false ;					// タイプツリー自体を削除(サイズ削減用途として)

		private bool						m_CollectDependencies			= false ;					// 全てのアセットバンドルでそのアセットバンドルで使用するアセットを全て含ませる

		private bool						m_GenerateCRCFile				= true ;					// アセットバンドルファイルのＣＲＣファイルを出力する

		//--------------------------------------------------
	
	
		private AssetBundleFile[]			m_AssetBundleFiles = null ;
	
		//--------------------------------------------------
	
		private bool m_Clean				= true ;
	
		private Vector2 m_Scroll			= Vector2.zero ;
	
		private bool m_Refresh				= true ;
	
		private bool m_ShowResourceElements	= false ;
	
	
		//-----------------------------------------------------------------
		
		// 描画
		void OnGUI()
		{
			GUILayout.Space( 6f ) ;
		
			string path ;
		
			// リスト更新フラグ
			m_Refresh = false ;

			//-------------------------------------------------------------
		
			EditorGUILayout.HelpBox( GetMessage( "SelectResourcePath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Resource List File Path", GUILayout.Width( 200f ) ) == true )
				{
					m_Refresh = true ;

					m_ResourceListFilePath = string.Empty ;
					m_ResourceRootFolderPath = string.Empty ;
					m_AssetBundleFiles = null ;

					if( Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( File.Exists( path ) == true )
						{
							// ファイルを指定
								
							TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( path ) ;
							if( textAsset != null && string.IsNullOrEmpty( textAsset.text ) == false )
							{
								m_ResourceListFilePath = path ;

								path = path.Replace( "\\", "/" ) ;
							
								// 最後のフォルダ区切り位置を取得する
								int s = path.LastIndexOf( '.' ) ;
								if( s >= 0 )
								{
									path = path.Substring( 0, s ) ;
								}
							
								// 最後のフォルダ区切り位置を取得する
								s = path.LastIndexOf( '/' ) ;
								if( s >= 0 )
								{
									path = path.Substring( 0, s ) ;
								}
								
								// ファイルかどうか判別するには System.IO.File.Exists
								m_ResourceRootFolderPath = path + "/" ;
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;
			
				//---------------------------------------------------------
			
				// ルートフォルダ
				EditorGUILayout.TextField( m_ResourceListFilePath ) ;
			}
			GUILayout.EndHorizontal() ;
			
			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false )
			{
				GUILayout.BeginHorizontal() ;
				{
					// ルートフォルダも表示する
					GUILayout.Label( "     Resource Root Folder Path     ", GUILayout.Width( 200f ) ) ;
					GUI.color = Color.yellow ;
					GUILayout.Label( m_ResourceRootFolderPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;
			}

			if( string.IsNullOrEmpty( m_ResourceListFilePath ) == false && string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false )
			{
				// 更新
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					m_Refresh = true ;	// 対象更新
				}
				GUI.backgroundColor = Color.white ;
			}

			//-------------------------------------------------------------

			GUILayout.Space( 12f ) ;
		
			//-------------------------------------------------------------
		
			EditorGUILayout.HelpBox( GetMessage( "SelectAssetBundlePath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 1, 0.5f, 0, 1 ) ;
				if( GUILayout.Button( "AssetBundle Root Folder Path", GUILayout.Width( 220f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AssetBundleRootFolderPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( System.IO.Directory.Exists( path ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							path = path.Replace( "\\", "/" ) ;
						}
						else
						{
							// ファイルを指定しています
							path = path.Replace( "\\", "/" ) ;
						
							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int s = path.LastIndexOf( '.' ) ;
							if( s >= 0 )
							{
								path = path.Substring( 0, s ) ;
							}
						
							// 最後のフォルダ区切り位置を取得する
							s = path.LastIndexOf( '/' ) ;
							if( s >= 0 )
							{
								path = path.Substring( 0, s ) ;
							}
						}
					
						m_AssetBundleRootFolderPath = path + "/" ;
					}

					// プラットフォーム自動設定
					path = m_AssetBundleRootFolderPath ;
					if( string.IsNullOrEmpty( path ) == false )
					{
						string[] folderNameElements = path.Split( '/' ) ;
						string smallFolderName ;
						if( folderNameElements != null && folderNameElements.Length >= 2 )
						{
							for( int i  = 0 ; i <  ( folderNameElements.Length - 1 ) ; i ++ )
							{
								if( string.IsNullOrEmpty( folderNameElements[ i ] ) == false )
								{
									smallFolderName = folderNameElements[ i ].ToLower() ;

									if( smallFolderName == "windows" )
									{
										m_BuildTarget = BuildTarget.StandaloneWindows ;
										break ;
									}
									else
									if( smallFolderName == "android" )
									{
										m_BuildTarget = BuildTarget.Android ;
										break ;
									}
									else
									if( smallFolderName == "ios" || smallFolderName == "iphone" )
									{
										m_BuildTarget = BuildTarget.iOS ;
									}
								}
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;
			
				// 保存パス
				if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
				{
					GUI.color = Color.yellow ;
					GUILayout.Label( "Select AssetBundle Root Folder Path." ) ;
					GUI.color = Color.white ;
				}
				else
				{
					m_AssetBundleRootFolderPath = EditorGUILayout.TextField( m_AssetBundleRootFolderPath ) ;
				}
			}
			GUILayout.EndHorizontal() ;
		
			//-----------------------------------------------------
		
			// ターゲットプラットフォームと圧縮指定
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( "Build Target", GUILayout.Width( 80 ) ) ;	// null でないなら 74
			
				BuildTarget buildTarget = ( BuildTarget )EditorGUILayout.EnumPopup( m_BuildTarget ) ;
				if( buildTarget != m_BuildTarget )
				{
					m_BuildTarget  = buildTarget ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool chunkBasedCompression = EditorGUILayout.Toggle( m_ChunkBasedCompression, GUILayout.Width( 10f ) ) ;
				if( chunkBasedCompression != m_ChunkBasedCompression )
				{
					m_ChunkBasedCompression = chunkBasedCompression ;
				}
				GUILayout.Label( "Chunk Based Compression", GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool forceRebuildAssetBundle = EditorGUILayout.Toggle( m_ForceRebuildAssetBundle, GUILayout.Width( 10f ) ) ;
				if( forceRebuildAssetBundle != m_ForceRebuildAssetBundle )
				{
					m_ForceRebuildAssetBundle = forceRebuildAssetBundle ;
				}
				GUILayout.Label( "Force Rebuild AssetBundle", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool ignoreTypeTreeChanges = EditorGUILayout.Toggle( m_IgnoreTypeTreeChanges, GUILayout.Width( 10f ) ) ;
				if( ignoreTypeTreeChanges != m_IgnoreTypeTreeChanges )
				{
					m_IgnoreTypeTreeChanges = ignoreTypeTreeChanges ;
					if( m_IgnoreTypeTreeChanges == true )
					{
						m_DisableWriteTypeTree = false ;
					}
				}
				GUILayout.Label( "Ignore Type Tree Changes",  GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool disableWriteTypeTree = EditorGUILayout.Toggle( m_DisableWriteTypeTree, GUILayout.Width( 10f ) ) ;
				if( disableWriteTypeTree != m_DisableWriteTypeTree )
				{
					m_DisableWriteTypeTree = disableWriteTypeTree ;
					if( m_DisableWriteTypeTree == true )
					{
						m_IgnoreTypeTreeChanges = false ;
					}
				}
				GUILayout.Label( "Disable Write TypeTree", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Space(  6f ) ;
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool collectDependencies = EditorGUILayout.Toggle( m_CollectDependencies, GUILayout.Width( 10f ) ) ;
				if( collectDependencies != m_CollectDependencies )
				{
					m_CollectDependencies = collectDependencies ;
					m_Refresh = true ;	// リスト更新
				}
				GUI.color = Color.yellow ;
				GUILayout.Label( "Collect Dependencies ( Legacy Type )", GUILayout.Width( 240f ) ) ;
				GUI.color = Color.white ;

				GUILayout.Label( " " ) ;

				bool generateCRCFile = EditorGUILayout.Toggle( m_GenerateCRCFile, GUILayout.Width( 10f ) ) ;
				if( generateCRCFile != m_GenerateCRCFile )
				{
					m_GenerateCRCFile = generateCRCFile ;
				}
				GUILayout.Label( "Generate CRC File", GUILayout.Width( 160f ) ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//-----------------------------------------------------
		
			GUILayout.Space( 24f ) ;

			//-------------------------------------------------------------
		
			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false && m_ResourceRootFolderPath == m_AssetBundleRootFolderPath )
			{
				// 同じパスを指定するのはダメ
				EditorGUILayout.HelpBox( GetMessage( "SamePath" ), MessageType.Warning ) ;
			
				return ;
			}
		
			//-------------------------------------------------------------
		
			// ここからが表示と出力

			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == true )
			{
				return ;
			}
		
			//----------------------------------

			//　アップデートフラグを更新する
			if( m_Refresh == true )
			{
				m_Refresh = false ;
			
				// アセットバンドル情報を読み出す
				m_AssetBundleFiles = GetAssetBundleFiles() ;
			}
		
			// アセットバンドル化対象リストを表示する
			if( m_AssetBundleFiles == null || m_AssetBundleFiles.Length == 0 )
			{
				return ;
			}

			// トータルのアセットバンドル数とリソース数を計算する
			int acount = m_AssetBundleFiles.Length ;
			int rcount = 0 ;
			foreach( var assetBundleFile in m_AssetBundleFiles )
			{
				rcount += assetBundleFile.AssetFiles.Count ;
			}
			
			//---------------------------------------------------------
			
			// 生成ボタン（Create , Create And Replace , Replace
			bool execute = false ;
			
			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == false && Directory.Exists( m_AssetBundleRootFolderPath ) == true )
			{
				// 更新または新規作成対象が存在する
				GUILayout.BeginHorizontal() ;
				{
					// 生成
					GUI.backgroundColor = new Color( 0, 1, 0, 1 ) ;
					if( GUILayout.Button( "Create Or Update" ) == true )
					{
						execute = true ;
					}
					GUI.backgroundColor = Color.white ;
				
					// 同時にリソースリスト外のファイル・フォルダを削除する
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
					m_Clean = EditorGUILayout.Toggle( m_Clean, GUILayout.Width( 10f ) ) ;
					GUILayout.Label( "Clean", GUILayout.Width( 40 ) ) ;
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
				}
				GUILayout.EndHorizontal() ;
			}

			//---------------------------------------------------------
			
			// リストを表示する
			
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				m_ShowResourceElements = EditorGUILayout.Toggle( m_ShowResourceElements, GUILayout.Width( 10f ) ) ;
				GUILayout.Label( "Show Resource Elements" ) ;	// null でないなら
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
				
			GUILayout.Space(  6f ) ;
			
			GUILayout.Label( "Asset Bundle : " + acount + "  from Resource : " + rcount ) ;
			
			string aformat = "{0,0:d" + acount.ToString ().Length +"}" ;
			string rformat = "{0,0:d" + rcount.ToString ().Length +"}" ;
			// 0 無しは "{0," + tNumber.Length + "}"
			
			//-------------------------------------------------
		
			Color c0 = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;
			Color c1 = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
			
			// スクロールビューで表示する
			m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
			{
				// 表示が必要な箇所だけ表示する
				int aline = 0 ;
				int rline = 0 ;
				foreach( var assetBundleFile in m_AssetBundleFiles )
				{
					// アセット情報
					GUILayout.BeginHorizontal() ;
					{
						// 横一列
						GUI.color = c0 ;

						GUILayout.Label( string.Format( aformat, aline ) + " : ", GUILayout.Width( 40f ) ) ;
						string ac ;
					
						if( m_CollectDependencies == false )
						{
							ac = " [ " + assetBundleFile.AssetFiles.Count +" ]" ;
						}
						else
						{
							int[] st = { 0, 0 } ;

							if( assetBundleFile.AssetFiles.Count >  0 )
							{
								foreach( var assetFile in assetBundleFile.AssetFiles )
								{
									st[ assetFile.AssetType ] ++ ;
								}
							}

							ac = " [ " + st[ 0 ] + " + " + st[ 1 ] +" ]" ;
						}

						string assetBundlePath = assetBundleFile.AssetBundlePath ;
						if( assetBundleFile.SourceType == 1 )
						{
							assetBundlePath += "/" ;
						}
						
						GUILayout.Label( assetBundlePath + ac ) ;

						GUI.color = Color.white ;
					}
					GUILayout.EndHorizontal() ;
						
					if( m_ShowResourceElements == true )
					{
						if( assetBundleFile.AssetFiles.Count >  0 )
						{
							GUI.color = c1 ;
							foreach( var assetFile in assetBundleFile.AssetFiles )
							{
								GUILayout.BeginHorizontal() ;
								{
									// 横一列
									if( m_CollectDependencies == true )
									{
										if( assetFile.AssetType == 0 )
										{
											GUI.color = Color.white ;
										}
										else
										{
											GUI.color = Color.yellow ;
										}
									}

									GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
									GUILayout.Label( string.Format( rformat, rline ) + " : ", GUILayout.Width( 40f ) ) ;
									GUILayout.Label( assetFile.AssetPath ) ;
									rline ++ ;
								}
								GUILayout.EndHorizontal() ;
							}
							GUI.color = Color.white ;
						}
					}
					aline ++ ;
				}
			}
			GUILayout.EndScrollView() ;
			
			//-------------------------------------------------
			
			if( execute == true )
			{
				// アセットバンドル群を生成する
				if( m_AssetBundleRootFolderPath == "Assets/" )
				{
					execute = EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "RootPath" ).Replace( "%1", m_AssetBundleRootFolderPath ), GetMessage( "Yes" ), GetMessage( "No" ) ) ;
				}
				
				if( execute == true )
				{
					CreateAssetBundleAll() ;	// 表示と状態が変わっている可能性があるのでリストは作り直す
					
					// 表示を更新
					m_Refresh = true ;
					Repaint() ;
					
					// 結果のアラートダイアログを表示する
					EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "Succeed" ), GetMessage( "OK" ) ) ;
				}
			}

			// EditorUserBuildSettings.activeBuildTarget
		}

		// 選択中のファイルが変更された際に呼び出される
		void OnSelectionChange()
		{
			Repaint() ;
		}

		//-----------------------------------------------------------------------------------------------------

		// アセットバンドルの生成リストを取得する
		private AssetBundleFile[] GetAssetBundleFiles()
		{
			//-------------------------------------------------------------
			
			if( File.Exists( m_ResourceListFilePath ) == false )
			{
				Debug.Log( "[Log]Error : File not found !! : " + m_ResourceListFilePath ) ;
				return null ;
			}

			// リストファイルを読み出す
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( m_ResourceListFilePath ) ;
			string text = textAsset.text ;

			if( string.IsNullOrEmpty( text ) == true )
			{
				Debug.Log( "[Log]Error : Bad list file !! : " + m_ResourceListFilePath ) ;
				return null ;
			}
			
			string[] elements = text.Split( '\n', ( char )0x0D, ( char )0x0A ) ;
			if( elements == null || elements.Length == 0 )
			{
				return null ;
			}
			
			//-------------------------------------------------------------

			List<AssetBundleFile> assetBundleFiles = new List<AssetBundleFile>() ;
			
			string		path ;
			string[]	tags ;

			string[] wildPaths ;

			foreach( var element in elements )
			{
				path = element ;

				path = path.TrimEnd( '\n', ( char )0x0D, ( char )0x0A ) ;
				path = path.Trim( ' ' ) ;	// 前後のスペースを削除する

				tags = null ;

				if( path.IndexOf( ',' ) >= 0 )
				{
					// タグの指定あり
					string[] words = path.Split( ',' ) ;
					path = words[ 0 ] ;
					if( words.Length >= 2 && string.IsNullOrEmpty( words[ 1 ] ) == false )
					{
						// タグあり
						if( words[ 1 ].IndexOf( ' ' ) <  0 )
						{
							tags = new string[]{ words[ 1 ] } ;	// タグは単一
						}
						else
						{
							tags = words[ 1 ].Split( ' ' ) ;	// タグは複数
						}
					}
				}

				if( string.IsNullOrEmpty( path ) == false )
				{
					path = GetLowerPath( path, out bool wildCard, out bool folderOnly ) ;
					if( path != null )
					{
						// 有効なパス指定
						wildPaths = GetUpperPath( path ) ;
						if( wildPaths != null && wildPaths.Length >  0 )
						{
							foreach( var wildPath in wildPaths )
							{
								// 生成するアセットバンドル情報を追加する
								AddAssetBundleFile( wildPath, m_ResourceRootFolderPath + wildPath, wildCard, folderOnly, tags, ref assetBundleFiles ) ;
							}
						}
					}
				}
			}

			//-------------------------------------------------------------

			if( assetBundleFiles.Count == 0 )
			{
				return null ;
			}
		
			//-----------------------------------------------------
		
			return assetBundleFiles.ToArray() ;
		}

		// 生成するアセットバンドル情報を追加する
		private void AddAssetBundleFile( string path, string resourcePath, bool wildCard, bool folderOnly, string[] tags, ref List<AssetBundleFile> assetBundleFiles )
		{
			int i, l, p ;

			string parentPath, assetName ;
			string[] targetPaths ;

			if( wildCard == false )
			{
				// 単体

				// １つ親のフォルダを取得する
				p = resourcePath.LastIndexOf( '/' ) ;
				if( p <  0 )
				{
					// ありえない
					return ;
				}

				parentPath = resourcePath.Substring( 0, p ) ;
				if( parentPath.Length <  0 )
				{
					// ありえない
					return ;
				}

				// 親フォルダ内の全てのフォルダまたはファイルのパスを取得する
				if( folderOnly == false && Directory.Exists( parentPath ) == true )
				{
					targetPaths = Directory.GetFiles( parentPath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							// 拡張子は関係無く最初にヒットしたものを対象とする(基本的に同名のフォルダとファイルを同一フォルダ内に置いてはならない)

							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;
							if( targetPaths[ i ].Contains( path ) == true )
							{
								// 対象はフォルダまたはファイル
								if( CheckType( targetPaths[ i ] ) == true )
								{
									// 決定(単独ファイル)

									AssetBundleFile assetBundleFile = new AssetBundleFile()
									{
										AssetBundlePath = path,	// 出力パス(相対)
										Tags = tags		// タグ
									} ;
														
									// コードで対象指定：単独ファイルのケース
									assetBundleFile.AddAssetFile( targetPaths[ i ], 0 ) ;
									
									assetBundleFile.SourceType = 0 ;	// 単独ファイル

									if( m_CollectDependencies == true )
									{
										// 依存対象のアセットも内包対象に追加する
										assetBundleFile.CollectDependencies() ;
									}

									// リストに加える
									assetBundleFiles.Add( assetBundleFile ) ;

									// 終了
									return ;
								}
							}
						}
					}
				}

				// フォルダ
				if( Directory.Exists( resourcePath ) == true )
				{
					AssetBundleFile assetBundleFile = new AssetBundleFile()
					{
						AssetBundlePath	= path,		// 出力パス
						Tags			= tags		// タグ
					} ;
							
					// 再帰的に素材ファイルを加える
					AddAssetBundleFile( assetBundleFile, resourcePath ) ;
					
					if( assetBundleFile.AssetFiles.Count >  0 )
					{
						assetBundleFile.SourceType = 1 ;	// 複数ファイル

						if( m_CollectDependencies == true )
						{
							// 依存対象のアセットも内包対象に追加する
							assetBundleFile.CollectDependencies() ;
						}

						// リストに加える
						assetBundleFiles.Add( assetBundleFile ) ;

						// 終了
						return ;
					}
				}
			}
			else
			{
				// 複数

				if( Directory.Exists( resourcePath ) == false )
				{
					return ;
				}

				if( folderOnly == false )
				{
					// ファイル
					targetPaths = Directory.GetFiles( resourcePath ) ;
					if( targetPaths != null && targetPaths.Length >  0 )
					{
						l = targetPaths.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;

							// 対象はファイル
							if( CheckType( targetPaths[ i ] ) == true )
							{
								// 決定(単独ファイル)

								AssetBundleFile assetBundleFile = new AssetBundleFile()
								{
									Tags = tags		// タグ
								} ;
								
								assetName = targetPaths[ i ] ;
								p = assetName.LastIndexOf( '/' ) ;
								if( p >= 0 )
								{
									p ++ ;
									assetName = assetName.Substring( p, assetName.Length - p ) ;
								}
								p = assetName.IndexOf( '.' ) ;
								if( p >= 0 )
								{
									assetName = assetName.Substring( 0, p ) ;
								}

								assetBundleFile.AssetBundlePath	= path + "/" + assetName ;	// 出力パス(相対)
						
								// コードで対象指定：単独ファイルのケース
								assetBundleFile.AddAssetFile( targetPaths[ i ], 0 ) ;
						
								assetBundleFile.SourceType = 0 ;	// 単独ファイル

								if( m_CollectDependencies == true )
								{
									// 依存対象のアセットも内包対象に追加する
									assetBundleFile.CollectDependencies() ;
								}

								// リストに加える
								assetBundleFiles.Add( assetBundleFile ) ;
							}
						}
					}
				}

				// フォルダ
				targetPaths = Directory.GetDirectories( resourcePath ) ;
				if( targetPaths != null && targetPaths.Length >  0 )
				{
					l = targetPaths.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						targetPaths[ i ] = targetPaths[ i ].Replace( "\\", "/" ) ;

						AssetBundleFile assetBundleFile = new AssetBundleFile()
						{
							Tags = tags		// タグ
						} ;
															
						assetName = targetPaths[ i ] ;
						p = assetName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							p ++ ;
							assetName = assetName.Substring( p, assetName.Length - p ) ;
						}

						assetBundleFile.AssetBundlePath = path + "/" + assetName ;	// 出力パス
							
						// 再帰的に素材ファイルを加える
						AddAssetBundleFile( assetBundleFile, targetPaths[ i ] ) ;
					
						if( assetBundleFile.AssetFiles.Count >  0 )
						{
							assetBundleFile.SourceType = 1 ;	// 複数ファイル

							if( m_CollectDependencies == true )
							{
								// 依存対象のアセットも内包対象に追加する
								assetBundleFile.CollectDependencies() ;
							}

							// リストに加える
							assetBundleFiles.Add( assetBundleFile ) ;
						}
					}
				}
			}
		}

		// アセットバンドルの要素をリストに追加していく（再帰版）
		private void AddAssetBundleFile( AssetBundleFile assetBundleFile, string currentPath )
		{
			if( Directory.Exists( currentPath ) == false )
			{
				return ;
			}
		
			//-----------------------------------------------------
		
			// フォルダ
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( int i  = 0 ; i <  da.Length ; i ++ )
				{
					// サブフォルダを検査
					da[ i ] = da[ i ].Replace( "\\", "/" ) ;
					AddAssetBundleFile( assetBundleFile, da[ i ] + "/" ) ;	// 再帰版
				}
			}
		
			// ファイル
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				for( int i  = 0 ; i <  fa.Length ; i ++ )
				{
					// 対象化コードで反転無効化（は止める）
					fa[ i ] = fa[ i ].Replace( "\\", "/" ) ;
					if( CheckType( fa[ i ] ) == true )
					{
						// コードで対象指定：複数ファイルのケース
						assetBundleFile.AddAssetFile( fa[ i ], 0 ) ;
					}
				}
			}
		}

		// パスを解析して最終的なターゲットパスを取得する
		private string GetLowerPath( string path, out bool wildCard, out bool folderOnly )
		{
			wildCard = false ;
			folderOnly = false ;

			path = path.Replace( "**", "*" ) ;
			path = path.Replace( "**", "*" ) ;

			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 先頭がビックリマークなら除外する
			if( path[ 0 ] == '!' )
			{
				// 不可
				return null ;
			}

			// 先頭にスラッシュが付いていれば外す
			path = path.TrimStart( '/' ) ;
			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にスラッシュが付いていればフォルダ限定
			if( path[ path.Length - 1 ] == '/' )
			{
				folderOnly = true ;	// フォルダ限定
				path = path.TrimEnd( '/' ) ;
			}

			if( path.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にアスタリスクが付いていれば複数対象
			if( path[ path.Length - 1 ] == '*' )
			{
				wildCard = true ;	// 対象は親フォルダ内の全て
				path = path.TrimEnd( '*' ) ;
			}

			// 最後にスラッシュになってしまうようなら除外する
			if( path.Length >= 1 )
			{
				if( path[ path.Length - 1 ] == '/' )
				{
					path = path.Trim( '/' ) ;
				}
			}

			// パスが空文字の場合もありえる
			return path ;
		}

		private string[] GetUpperPath( string path )
		{
			if( string.IsNullOrEmpty( path ) == true || path.Contains( "*" ) == false )
			{
				return new string[]{ path } ;
			}

			// ワイルドカード部分を展開して全て個別のパスにする

			List<string> stackedPaths = new List<string>() ;

			// 一時的に最後にスラッシュを付ける
			path += "/" ;

			string currentPath = string.Empty ;

			// 再帰メソッドを呼ぶ
			GetUpperPath( path, currentPath, ref stackedPaths ) ;

			if( stackedPaths.Count == 0 )
			{
				// ワイルドカード内で有効なパスは存在しない
				return null ;
			}

			return stackedPaths.ToArray() ;
		}

		// 再帰処理側
		private void GetUpperPath( string path, string currentPath, ref List<string> stackedPaths )
		{
			int p ;
			string token,  fixedPath ;

			string[]		folderPaths ;
			List<string>	assetPaths = new List<string>() ;

			//----------------------------------

			p = path.IndexOf( "/" ) ;

			// 最初のスラッシュまで切り出し(絶対に０より大きい値になる
			token = path.Substring( 0, p ) ;

			p ++ ;
			path = path.Substring( p, path.Length - p ) ;

			if( token != "*" )
			{
				// 固定
				fixedPath = currentPath ;
				if( string.IsNullOrEmpty( fixedPath ) == false )
				{
					fixedPath += "/" ;
				}
				fixedPath += token ;

				if( string.IsNullOrEmpty( path ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					GetUpperPath( path, fixedPath, ref stackedPaths ) ;
				}
				else
				{
					// 最終的なパスが決定した
					stackedPaths.Add( fixedPath ) ;
				}
			}
			else
			{
				// 可変

				if( Directory.Exists( m_ResourceRootFolderPath + currentPath ) == false )
				{
					// フォルダが存在しない
					return ;
				}

				assetPaths.Clear() ;
				folderPaths = Directory.GetDirectories( m_ResourceRootFolderPath + currentPath ) ;
				if( folderPaths != null && folderPaths.Length >  0 )
				{
					foreach( var folderPath in folderPaths )
					{
						string assetPath = folderPath.Replace( "\\", "/" ) ;
						if( Directory.Exists( assetPath ) == true )
						{
							// フォルダ
							assetPaths.Add( assetPath.Replace( m_ResourceRootFolderPath + currentPath + "/", "" ) ) ;
						}
					}
				}

				if( assetPaths.Count == 0 )
				{
					// この枝は打ち止め
					return ;
				}

				if( string.IsNullOrEmpty( path ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					foreach( var assetPath in assetPaths )
					{
						fixedPath = currentPath ;
						if( string.IsNullOrEmpty( fixedPath ) == false )
						{
							fixedPath += "/" ;
						}
						fixedPath += assetPath ;

						GetUpperPath( path, fixedPath, ref stackedPaths ) ;
					}
				}
				else
				{
					// 最終的なパスが決定した
					foreach( var assetPath in assetPaths )
					{
						fixedPath = currentPath ;
						if( string.IsNullOrEmpty( fixedPath ) == false )
						{
							fixedPath += "/" ;
						}
						fixedPath += assetPath ;

						// 追加
						stackedPaths.Add( fixedPath ) ;
					}
				}
			}
		}

		//-----------------------------------------------------------
	
		// 拡張子をチェックして有効なファイルかどうか判別する
		private bool CheckType( string path )
		{
			int i = path.LastIndexOf( '.' ) ;
			if( i >= 0 )
			{
				// 拡張子あり
				string extension = path.Substring( i + 1, path.Length - ( i + 1 ) ) ;
			
				if( string.IsNullOrEmpty( extension ) == false )
				{
					if( extension != "meta" )
					{
						return true ;	// meta 以外はＯＫ
					}
				}
			}
		
			return false ;
		}
	
		//-----------------------------------------------------------------

		// 必要なアセットバンドルを全て生成する
		private void CreateAssetBundleAll()
		{
			// コンソールから呼ばれた場合
			AssetBundleFile[] assetBundleFiles = GetAssetBundleFiles() ;

			if( assetBundleFiles != null && assetBundleFiles.Length >  0 )
			{
				CreateAssetBundleAll( assetBundleFiles ) ;
			}
		}

		// 必要なアセットバンドルを全て生成する
		private void CreateAssetBundleAll( AssetBundleFile[] assetBundleFiles )
		{
			if( assetBundleFiles == null || assetBundleFiles.Length == 0 )
			{
				return ;
			}
	
			int i, l ;
			bool result = false ;
			string[] assetBundleNames = null ;
			
			//-----------------------------------------------------------------------------
			
			// アセットバンドルファイルの階層が浅い方から順にビルドするようにソートする(小さい値の方が先)

			//-----------------------------------------------------------------------------
		
			// 保存先ルートフォルダ
			string assetBundleRootFolderPath = m_AssetBundleRootFolderPath ;
			l = assetBundleRootFolderPath.Length ;
			if( assetBundleRootFolderPath[ l - 1 ] == '/' )
			{
				assetBundleRootFolderPath = assetBundleRootFolderPath.Substring( 0, l - 1 ) ;
			}
			
			if( Directory.Exists( assetBundleRootFolderPath ) == false )
			{
				Debug.Log( "[Log]Output folder is not found :" + assetBundleRootFolderPath ) ;
				return ;
			}

			//----------------------------------

			BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle ;

			if( m_ChunkBasedCompression == true )
			{
				options |= BuildAssetBundleOptions.ChunkBasedCompression ;
			}

			if( m_ForceRebuildAssetBundle == true )
			{
				options |= BuildAssetBundleOptions.ForceRebuildAssetBundle ;
			}

			if( m_IgnoreTypeTreeChanges == true && m_DisableWriteTypeTree == false )
			{
				options |= BuildAssetBundleOptions.IgnoreTypeTreeChanges ;
			}

			if( m_DisableWriteTypeTree == true && m_IgnoreTypeTreeChanges == false  )
			{
				options |= BuildAssetBundleOptions.DisableWriteTypeTree ;
			}

			//---------------------------------------------------------

			if( m_CollectDependencies == false )
			{
				// 新版(依存アセット除外あり)
				l = assetBundleFiles.Length ;

				// ここからが新版のメイン生成処理
				AssetBundleBuild[] map = new AssetBundleBuild[ l ] ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					PostAssetBundleFile( assetBundleFiles[ i ], ref map[ i ] ) ;
				}

				//--------------------

				AssetBundleManifest manifest ;

				// アセットバンドルの生成
				manifest = BuildPipeline.BuildAssetBundles
				(
					assetBundleRootFolderPath,
					map,
					options,
					m_BuildTarget
				) ;
	
				if( manifest != null )
				{
					result = true ;
					assetBundleNames = manifest.GetAllAssetBundles() ;
				}
			}
			else
			{
				// 旧版(依存アセット除外なし)
				l = assetBundleFiles.Length ;

				// ここからが新版のメイン生成処理
				AssetBundleBuild[] map = new AssetBundleBuild[ 1 ] ;
			
				AssetBundleManifest manifest ;

				string[] nameArray ;
				List<string> nameList = new List<string>() ;
				List<string> hashList = new List<string>() ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					PostAssetBundleFile( assetBundleFiles[ i ], ref map[ 0 ] ) ;

					// アセットバンドルの生成
					manifest = BuildPipeline.BuildAssetBundles
					(
						assetBundleRootFolderPath,
						map,
						options,
						m_BuildTarget
					) ;
	
					if( manifest == null )
					{
						break ;
					}

					nameArray = manifest.GetAllAssetBundles() ;
					if( nameArray != null && nameArray.Length >  0 )
					{
						nameList.Add( nameArray[ 0 ] ) ;	// 常に１つのはず
						hashList.Add( manifest.GetAssetBundleHash( nameArray[ 0 ] ).ToString() ) ; 
					}
				}

				if( i >= l )
				{
					result = true ;
					assetBundleNames = nameList.ToArray() ;

					// ハッシュリストを生成して保存する
					l = nameList.Count ;
					string text = string.Empty ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						text += ( nameList[ i ] + "," + hashList[ i ] + "\n" ) ;
					}

					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".list", text ) ;
				}
			}

			//-------------------------------------------------------------------------------

			// ＣＲＣファイルを出力する
			if( m_GenerateCRCFile == true )
			{
//				Debug.LogWarning( "保存先ルートフォルダ:" + m_AssetBundleRootFolderPath ) ;
//				Debug.LogWarning( "保存マニフェスト名:" + GetAssetBundleRootName() ) ;

				byte[] data ;
				int size ;
				uint crc ;
				string text = string.Empty ;

				foreach( var assetBundleFile in assetBundleFiles )
				{
					data = File.ReadAllBytes( m_AssetBundleRootFolderPath + assetBundleFile.AssetBundlePath ) ;
					if( data != null && data.Length >  0 )
					{
						size  = data.Length ;
						crc   = GetCRC32( data ) ;

						text += ( assetBundleFile.AssetBundlePath + "," + size + "," + crc ) ;
						if( assetBundleFile.Tags != null && assetBundleFile.Tags.Length >  0 )
						{
							text += "," ;
							for( i  = 0 ; i <  assetBundleFile.Tags.Length ; i ++ )
							{
								text += assetBundleFile.Tags[ i ] ;
								if( i <  ( assetBundleFile.Tags.Length - 1 ) )
								{
									text += " " ;	// タグの区切り記号はスペース
								}
							}
						}
						text += "\n" ;
					}

//					Debug.LogWarning( "アセットバンドルファイルのパス:" + tAssetBundleFileList[ i ].name ) ;
				}

				if( string.IsNullOrEmpty( text ) == false )
				{
					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".crc", text ) ;
				}
			}

			//-------------------------------------------------------------------------------

			if( m_Clean == true && result == true && assetBundleNames != null && assetBundleNames.Length >  0 )
			{
				// 余計なファイルを削除する
				CleanAssetBundle( assetBundleNames ) ;
			}
		
			AssetDatabase.Refresh() ;
		}

		// アセットバンドルの情報を格納する
		private void PostAssetBundleFile( AssetBundleFile assetBundleFile, ref AssetBundleBuild map )
		{
			string assetBundlePath = assetBundleFile.AssetBundlePath ;

			map.assetBundleName = assetBundlePath ;
			map.assetBundleVariant = string.Empty ;
		
//			Debug.LogWarning( "Map assetBundleName:" + tMap.assetBundleName ) ;
			
			List<string> assetPaths = new List<string>() ;
		
			foreach( var assetFile in assetBundleFile.AssetFiles )
			{
				// このアセット自体は確実にアセットバンドルに含まれる
				if( assetFile.AssetType == 0 )
				{
					assetPaths.Add( assetFile.AssetPath ) ;
//					Debug.LogWarning( "含まれるリソース:" + tAssetBundleFile.assetFile[ i ].path ) ;

					// 依存関係にあるアセットを表示する
//					DebugPrintDependencies( tAssetBundleFile.assetFile[ i ].path ) ;
				}
			}
		
			map.assetNames = assetPaths.ToArray() ;
		}

		private const uint CRC32_MASK = 0xffffffff ;
	
		// ＣＲＣ値を取得する
		public static uint GetCRC32( byte[] data )
		{
			uint value = CRC32_MASK  ;
			
			foreach( var code in data )
			{
				value = m_CRC32_Table[ ( value ^ code ) & 0xFF ] ^ ( value >> 8 ) ;
			}
		
			return value ^ CRC32_MASK ;
		}
	
		private readonly static uint[] m_CRC32_Table = new uint[]
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
			0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
			0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
			0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
			0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
			0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
			0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
			0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
			0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
			0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
			0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
			0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
			0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
			0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
			0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
			0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
			0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
			0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
			0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
			0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
			0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
			0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
			0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
			0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
			0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
			0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
			0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
			0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
			0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
			0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
			0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
			0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
			0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
			0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
			0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
			0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
			0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
			0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
			0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
			0x2d02ef8d
		} ;

		//-----------------------------------------------------------------

		// 出力パスのフォルダ名を取得する
		private string GetAssetBundleRootName()
		{
			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
			{
				return string.Empty ;
			}

			// 出力パスのアセットバンドル
			string path = m_AssetBundleRootFolderPath ;
			if( path[ path.Length - 1 ] == '/' )
			{
				path = path.Substring( 0, path.Length - 1 ) ;
			}
			int i = path.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				path = path.Substring( i + 1, path.Length - ( i + 1 ) ) ;
			}

			return path ;
		}

		// 不要になったアセットバンドルファイルを削除する
		private void CleanAssetBundle( string[] assetBundleNames )
		{
			List<string> list = new List<string>() ;

			//-------------------------------------------------

			// 削除対象から除外するファイル名を登録する

			// 出力パスのアセットバンドル
			string path = GetAssetBundleRootName() ;

			// シングルマニフェストアセットバンドル
			if( m_CollectDependencies == false )
			{
				// 依存ありタイプ
				list.Add( m_AssetBundleRootFolderPath + path ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".meta" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".manifest" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".manifest.meta" ) ;
			}
			else
			{
				// 依存なしタイプ
				list.Add( m_AssetBundleRootFolderPath + path + ".list" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".list.meta" ) ;
			}

			if( m_GenerateCRCFile == true )
			{
				list.Add( m_AssetBundleRootFolderPath + path + ".crc" ) ;
				list.Add( m_AssetBundleRootFolderPath + path + ".crc.meta" ) ;
			}

			// 各アセットバンドル
			if( assetBundleNames != null && assetBundleNames.Length >  0 )
			{
				foreach( var assetBundleName in assetBundleNames )
				{
					list.Add( m_AssetBundleRootFolderPath + assetBundleName ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".meta" ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".manifest" ) ;
					list.Add( m_AssetBundleRootFolderPath + assetBundleName + ".manifest.meta" ) ;
				}
			}

			//---------------------------------------------------------

			// 再帰を使って不要になったアセットバンドルファイルを全て削除する
			CleanAssetBundle( list, m_AssetBundleRootFolderPath ) ;
		}

		// 不要になったアセットバンドルファイルを削除(再帰)
		private int CleanAssetBundle( List<string> list, string currentPath )
		{
			int i ;
			string path ;
		
			//-----------------------------------------------------
		
			if( Directory.Exists( currentPath ) == false )
			{
				return 0 ;
			}
		
			//-----------------------------------------------------
		
			int c = 0 ;	// フォルダ・ファイルを残すカウント
			int d = 0 ;	// フォルダ・ファイルを消すカウント
		
			// フォルダ
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( i  = 0 ; i <  da.Length ; i ++ )
				{
					// サブフォルダを検査
					path = da[ i ] + "/" ;
					if( CleanAssetBundle( list, path ) == 0 )
					{
						// このサブフォルダは残す
						Debug.LogWarning( "削除対象:" + path ) ;
						System.IO.Directory.Delete( path, true ) ;

						d ++ ;
					}
					else
					{
						// フォルダのメタファイルを削除対象から除外する
						if( path[ path.Length - 1 ] == '/' )
						{
							path = path.Substring( 0, path.Length - 1 ) ;
						}
						path += ".meta" ;

						if( list.Contains( path ) == false )
						{
							list.Add( path ) ;
						}

						c ++ ;
					}
				}
			}
		
			// ファイル
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				for( i  = 0 ; i <  fa.Length ; i ++ )
				{
					if( list.Contains( fa[ i ] ) == false )
					{
						// 削除対象

//						Debug.LogWarning( "削除対象:" + tFA[ i ] ) ;
						File.Delete( fa[ i ] ) ;

						d ++ ;
					}
					else
					{
						c ++ ;
					}
				}
			}
		
			Debug.Log( "Deleted Count : " + currentPath + " = " + d ) ;
		
			return c ;
		}
	
		// 依存するアセットをコンソールに表示する
		private void DebugPrintDependencies( string path )
		{
			// 依存関係にあるアセットを検出する
			string[] dependenciesPaths = AssetDatabase.GetDependencies( path ) ;
			if( dependenciesPaths!= null && dependenciesPaths.Length >  0 )
			{
				foreach( var dependenciesPath in dependenciesPaths )
				{
					Debug.LogWarning( "依存:" + dependenciesPaths ) ;
				}
			}
		}
		
		public void OnPostprocessAssetbundleNameChanged( string assetPath, string previousAssetBundleName, string newAssetBundleName )
		{
			Debug.Log( "Asset " + assetPath + " has been moved from assetBundle " + previousAssetBundleName + " to assetBundle " + newAssetBundleName + "." ) ;
		}

		//----------------------------------------------------------------------------------------------
		
		private static readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SelectResourcePath",		"AssetBundle化したいファイル一覧が記述されたリストファイルを設定してください" },
			{ "SelectAssetBundlePath",	"生成したAssetBundleを格納するフォルダを設定してください" },
			{ "SelectAllResource",		"AssetBundle化対象はプロジェクト全体のAssetLabel入力済みファイルとなります" },
			{ "SamePath",				"ResourceフォルダとAssetBundleフォルダに同じものは指定できません" },
			{ "RootPath",				"プロジェクトのルートフォルダ\n\n%1\n\nにAssetBundleを生成します\n\n本当によろしいですか？" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"はい" },
			{ "No",						"いいえ" },
			{ "OK",						"閉じる" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "SelectResourcePath",		"Please set up a list file that lists the files you want AssetBundle." },
			{ "SelectAssetBundlePath",	"Please set the folder in which to store the generated AssetBundle." },
			{ "SelectAllResource",		"AssetBundle target will be AssetLabel entered file of the entire project." },
			{ "SamePath",				"The same thing can not be specified in the Resource folder and AssetBundle folder." },
			{ "RootPath",				"Asset Bundle Root Path is \n\n '%1'\n\nReally ?" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"All Succeed !!" },
			{ "No",						"No" },
			{ "OK",						"OK" },
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

		//----------------------------------------------------------------------------

		// コマンドラインからの実行可能版
		public static bool BatchBuild()
		{
			string path = GetConfigurationFilePtah() ;
			if( string.IsNullOrEmpty( path ) == true )
			{
				Debug.Log( "Error : Bad Configuration File !!" ) ;
				return false ;
			}
			
			return BatchBuild( path ) ;
		}
	
		public static bool BatchBuild( string path )
		{
			SimpleAssetBundleBuilder sabb = ScriptableObject.CreateInstance<SimpleAssetBundleBuilder>() ;
		
			if( sabb.LoadConfiguration( path ) == false )
			{
				DestroyImmediate( sabb ) ;
			
				Debug.Log( "Error : Bad Configuration File !!" ) ;
			
				return false ;
			}
		
			//-----------------------------------------
			
			// アセットバンドルをビルドする
			sabb.CreateAssetBundleAll() ;
		
			// 最後にオブジェクトを破棄する
			DestroyImmediate( sabb ) ;
			
			return true ;
		}
		
		// コンフィギュレーションファイルのパスを取得する
		private static string GetConfigurationFilePtah()
		{
			string[] args = System.Environment.GetCommandLineArgs() ;
			if( args == null || args.Length == 0 )
			{
				return null ;
			}
		
			string path = "" ;
		
			int i, l = args.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( args[ i ].ToLower() == "-setting" && ( i + 1 ) <  l )
				{
					// 発見した
					path = args[ i + 1 ] ;
					break ;
				}
			}
		
			if( i >= l || string.IsNullOrEmpty( path ) == true )
			{
				return null ;
			}
		
	//		if( tPath[ 0 ] == '/' )
	//		{
	//			l = tPath.Length ;
	//			tPath = tPath.Substring( 1, l - 1 ) ;
	//		}
	//		
	//		tPath = "Assets/" + tPath ;
			
			return path ;
		}
	
	
		// コンフィグ情報を読み出す
		private bool LoadConfiguration( string path )
		{
	//		Debug.Log( "ConfigPath:" + tPath ) ;
		
			if( File.Exists( path ) == false )
			{
				return false ;
			}
		
			string code = File.ReadAllText( path ) ;
		
			if( string.IsNullOrEmpty( code ) == true )
			{
				return false ;
			}
		
	//		TextAsset tText = AssetDatabase.LoadAssetAtPath( tPath, typeof( TextAsset ) ) as TextAsset ;
	//		if( tText == null )
	//		{
	//			return false ;
	//		}
	//		
	//		if( string.IsNullOrEmpty( tText.text ) == true )
	//		{
	//			return true ;
	//		}
	//		
	//		string tCode = tText.text ;
		
			//-------------------------------------------------
		
			string[] line = code.Split( '\n' ) ;
			int i, l = line.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				line[ i ] = line[ i ].Replace( " ", "" ) ;
				line[ i ] = line[ i ].Replace( "\n", "" ) ;
				line[ i ] = line[ i ].Replace( "\t", "" ) ;
				line[ i ] = line[ i ].Replace( "\r", "" ) ;	// 超重要
				string[] data = line[ i ].Split( '=' ) ;
			
				if( data.Length == 2 )
				{
					string label = data[ 0 ].ToLower() ;
					string value = data[ 1 ] ;
				
					if( label == "ResourceListFilePath".ToLower() )
					{
						m_ResourceListFilePath = CollectFilePath( value ) ;
						Debug.Log( "[Log]ResourceListFilePath:" + m_ResourceListFilePath ) ;
					}

					if( label == "ResourceRootFolderPath".ToLower() )
					{
						m_ResourceRootFolderPath = CollectFolderPath( value ) ;
						Debug.Log( "[Log]ResourceRootFolderPath:" + m_ResourceRootFolderPath ) ;
					}

					if( label == "AssetBundleRootFolderPath".ToLower() )
					{
						m_AssetBundleRootFolderPath = CollectFolderPath( value ) ;
						Debug.Log( "[Log]m_AssetBundleRootFolderPath:" + m_AssetBundleRootFolderPath ) ;
					}

					if( label == "BuildTarget".ToLower() )
					{
						m_BuildTarget = GetBuildTarget( value ) ;
					}

					if( label == "ChunkBasedCompression".ToLower() )
					{
						m_ChunkBasedCompression = GetBoolean( value ) ;
					}

					if( label == "ForceRebuildAssetBundle".ToLower() )
					{
						m_ForceRebuildAssetBundle = GetBoolean( value ) ;
					}

					if( label == "IgnoreTypeTreeChanges".ToLower() )
					{
						m_IgnoreTypeTreeChanges = GetBoolean( value ) ;
					}

					if( label == "DisableWriteTypeTree".ToLower() )
					{
						m_DisableWriteTypeTree = GetBoolean( value ) ;
					}

					if( label == "CollectDependencies".ToLower() )
					{
						m_CollectDependencies = GetBoolean( value ) ;
					}
				}
			}
		
			return true ;
		}
	
		// パスの整形を行う
		private string CollectFolderPath( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return path ;
			}
		
			path = path.Replace( "\\", "/" ) ;
		
			if( path[ 0 ] == '/' )
			{
				path = path.Substring( 1, path.Length - 1 ) ;
			}
		
			if( path.Length <  1 )
			{
				return path ;
			}
		
			if( path[ path.Length - 1 ] != '/' )
			{
				path += "/" ;
			}
		
			return path ;
		}
	
		// パスの整形を行う
		private string CollectFilePath( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return path ;
			}
		
			path = path.Replace( "\\", "/" ) ;
		
			if( path[ 0 ] == '/' )
			{
				path = path.Substring( 1, path.Length - 1 ) ;
			}
		
			return path ;
		}

		// ブーリアン結果を取得する
		private bool GetBoolean( string value )
		{
			value = value.ToLower() ;
		
			bool boolean = false ;
		
			if( value == "false".ToLower() )
			{
				boolean = false ;
			}
			else
			if( value == "true".ToLower() )
			{
				boolean = true ;
			}
		
			return boolean ;
		}

		// ビルドターゲットを取得する
		private BuildTarget GetBuildTarget( string value )
		{
			value = value.ToLower() ;
		
			BuildTarget buildTarget = BuildTarget.Android ;
		
			if( value == "StandaloneOSXUniversal".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneOSX ;
			}
			else
			if( value == "StandaloneWindows".ToLower() || value == "Windows".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneWindows ;
			}
			else
			if( value == "iPhone".ToLower() || value == "iOS".ToLower() )
			{
				buildTarget = BuildTarget.iOS ;
			}
			else
			if( value == "Android".ToLower() )
			{
				buildTarget = BuildTarget.Android ;
			}
			else
			if( value == "StandaloneWindows64".ToLower() || value == "Windows64".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneWindows64 ;
			}
			else
			if( value == "StandaloneLinux64".ToLower() )
			{
				buildTarget = BuildTarget.StandaloneLinux64 ;
			}
		
			return buildTarget ;
		}
	}
}

