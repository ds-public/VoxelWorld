using System.Collections ;
using System.Collections.Generic ;
using System.IO ;

using UnityEditor ;
using UnityEngine ;

using DBS ;

/// <summary>
/// プレイヤーデータを生成する Version 2022/09/19
/// </summary>
public class Build_PlayerData
{
	private const string m_TargetRootPath	= "Assets/Application/Database/PlayerData" ;
	private const string m_OutputPath		= "Assets/StreamingAssets/dbs/PlayerData.bin" ;

	[MenuItem("Build/Database/PlayerData", priority = 0)]
	internal static void Process()
	{
		List<( string, object)> entities = new List<(string, object)>() ;

		string targetRootPath = m_TargetRootPath.Replace( "\\", "/" ).TrimEnd( '/' ) ;

		Add( targetRootPath, targetRootPath, ref entities ) ;

		string password = null ;

		var data = Zip.Compress( entities.ToArray(), password ) ;
		if( data != null && data.Length >  0 )
		{
			// 名前にフォルダが含まれているかチェックする
			int i = m_OutputPath.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				// フォルダが含まれている
				string folderName = m_OutputPath.Substring( 0, i ) ;

				if( Directory.Exists( folderName ) == false )
				{
					// フォルダを生成する(多階層をまとめて生成出来る)
					Directory.CreateDirectory( folderName ) ;
				}
			}

			File.WriteAllBytes( m_OutputPath, data ) ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			Debug.Log( "<color=#00FF00>[Build PlayerData] Completed ! : Size = " + data.Length + "</color>" ) ;
		}
	}

	// 再帰的にファイルを追加する
	private static void Add( string path, string targetRootPath, ref List<( string, object )> entities )
	{
		path = path.Replace( "\\", "/" ) ;
		path = path.TrimEnd( '/' ) ;

		int i, l ;

		var directories = Directory.GetDirectories( path ) ;
		if( directories != null && directories.Length >  0 )
		{
			l = directories.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Add( directories[ i ], targetRootPath, ref entities ) ;
			}
		}

		var files = Directory.GetFiles( path ) ;
		if( files != null && files.Length >  0 )
		{
			l = files.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var extension = Path.GetExtension( files[ i ] ) ;
				extension = extension.TrimStart( '.' ) ;
				if( extension.IndexOf( "txt" ) >= 0 || extension.IndexOf( "csv" ) >= 0 || extension.IndexOf( "json" ) >= 0 )
				{
					var data = File.ReadAllBytes( files[ i ] ) ;
					if( data != null && data.Length >  0 )
					{
						var name = files[ i ].Replace( "\\", "/" ).Replace( targetRootPath + "/", string.Empty ) ;
						name = name.Replace( "." + extension, string.Empty ) ;

						entities.Add( ( name, data ) ) ;
					}
				}
			}
		}
	}
}
