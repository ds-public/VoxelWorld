using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEditor ;
using UnityEditor.Build.Reporting ;

using UnityEngine ;

using DSW ;


/// <summary>
/// アプリケーションのバッチビルド用クラス Version 2024/03/31
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	// 設定ファイルのパス
	public readonly static string	m_SettingsPath			= "ScriptableObjects/Settings" ;

	// 有効なシーンファイルのパスのフィルタ
	private readonly static string	m_SceneFileFilter		= "Assets/Application/Scenes" ;

	//------------------------------------

	// MessagePack の IL2CPP 用の自動生成コード(リゾルバ)のパス
//	private const string	m_MessagePack_Resolver_Path		= "Assets/Application/Scripts/Runtime/_00_Framework/MessagePackHelper/Generated/MessagePack_Resolvers_GeneratedResolver.cs" ;

	//----------------------------------------------------------------------------

	/// <summary>
	/// ランタイムのプラットフォームタイプ
	/// </summary>
	public enum RuntimePlatformTypes
	{
		Windows64,
		OSX,
		Android,
		iOS,
		Linux64,
	}

	/// <summary>
	/// デディケーティッドサーバーのプラットフォームタイプ
	/// </summary>
	public enum DedicatedServerPlatformTypes
	{
		Windows64,
		OSX,
		Android,
		iOS,
		Linux64,
	}

	/// <summary>
	/// バージョン値を取得する
	/// </summary>
	/// <returns></returns>
	private static ( string, int ) GetRuntimeVersion( RuntimePlatformTypes platformType )
	{
		string versionName = "0.0.0" ;
		int    versionCode = 0 ;

		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			if( platformType == RuntimePlatformTypes.Android )
			{
				// Android
				versionName = settings.ClientVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			if( platformType == RuntimePlatformTypes.iOS )
			{
				// iOS
				versionName = settings.ClientVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			{
				// Other
				versionName = settings.SystemVersionName ;
			}
		}

		return ( versionName, versionCode ) ;
	}

	/// <summary>
	/// バージョン値を取得する
	/// </summary>
	/// <returns></returns>
	private static ( string, int ) GetDedicatedServerVersion( DedicatedServerPlatformTypes platformType )
	{
		string versionName = "0.0.0" ;
		int    versionCode = 0 ;

		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			if( platformType == DedicatedServerPlatformTypes.Android )
			{
				// Android
				versionName = settings.ClientVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			if( platformType == DedicatedServerPlatformTypes.iOS )
			{
				// iOS
				versionName = settings.ClientVersionName ;
				versionCode = settings.ClientVersionCode ;
			}
			else
			{
				// Other
				versionName = settings.SystemVersionName ;
			}
		}

		return ( versionName, versionCode ) ;
	}

	//--------------------------------------------------------------------------------------------

	// 変更する可能性のある設定項目
	private static string					m_PlayerSettings_productName ;
	private static string					m_PlayerSettings_bundleVersion ;
	private static Settings.EndPointNames	m_EndPoint ;
	private static bool						m_DevelopmentMode ;
	private static string					m_BuildVersion ;

	// 変更する可能性のある設定を退避する
	private static void PushSetting()
	{
		m_PlayerSettings_productName	= PlayerSettings.productName ;
		m_PlayerSettings_bundleVersion	= PlayerSettings.bundleVersion ;

		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			m_EndPoint				= settings.EndPoint ;
			m_DevelopmentMode		= settings.DevelopmentMode ;
			m_BuildVersion			= settings.BuildVersion ;
		}
	}

	// デフォルトのエンドポイントを設定する
	private static void SetDefaultEndPoint( Settings.EndPointNames endPointName )
	{
		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.EndPoint = endPointName ;

			EditorUtility.SetDirty( settings ) ;	// 重要

			// SaveAssets → Refresh の順でなければダメらしい(逆でもいける事もあるが)
			AssetDatabase.SaveAssets() ;

			settings = Resources.Load<Settings>( m_SettingsPath ) ;
			Debug.Log( "[デフォルトのエンドポイント] " + settings.EndPoint ) ;
		}
	}

	// 各種デバッグ用の機能を有効にするかを設定する
	private static void SetDevelopmentMode( bool developmentMode )
	{
		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.DevelopmentMode = developmentMode ;

			EditorUtility.SetDirty( settings ) ; // 重要
			AssetDatabase.SaveAssetIfDirty( settings ) ;

			settings = Resources.Load<Settings>( m_SettingsPath ) ;
			Debug.Log( "[各種デバッグ用の機能を有効にするか] " + settings.DevelopmentMode ) ;
		}
	}

	// ビルドバージョンを設定する
	private static void SetRevision( string buildVersion )
	{
		if( string.IsNullOrEmpty( buildVersion ) == true )
		{
			return ;
		}

		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.BuildVersion = buildVersion ;

			EditorUtility.SetDirty( settings ) ; // 重要
			AssetDatabase.SaveAssetIfDirty( settings ) ;

			settings = Resources.Load<Settings>( m_SettingsPath ) ;
			Debug.Log( "[自動リビジョン] " + settings.BuildVersion ) ;
		}
	}

	// 変更する可能性のある設定を復帰する
	private static void PopSetting()
	{
		Debug.Log( "[設定を復帰させる]" ) ;

		PlayerSettings.productName		= m_PlayerSettings_productName ;
		PlayerSettings.bundleVersion	= m_PlayerSettings_bundleVersion ;

		var settings = Resources.Load<Settings>( m_SettingsPath ) ;
		if( settings != null )
		{
			settings.EndPoint				= m_EndPoint ;
			settings.DevelopmentMode		= m_DevelopmentMode ;
			settings.BuildVersion			= m_BuildVersion ;

			EditorUtility.SetDirty( settings ) ;	// 重要
		}

		AssetDatabase.SaveAssets() ;
	}


	//--------------------------------------------------------------------------------------------
	// 処理

	/// <summary>
	/// ビルドタイプ
	/// </summary>
	public enum BuildTypes
	{
		Release		=  0,
		Staging		=  1,
		Development	=  2,
		Profiler	=  3,
		Unknown		= -1,
	}

	// 外部からはこれを叩いてアプリをビルドする。パラメータはコマンドライン引数から渡す。
	public static void Execute()
	{
		BuildTypes		build			= BuildTypes.Unknown ;
		BuildTarget		platform		= BuildTarget.NoTarget ;

		//-----------------------------------

		string[] args = Environment.GetCommandLineArgs() ;

		for( int i  = 0 ; i <  args.Length ; i ++ )
		{
			Debug.Log( args[ i ] ) ;
			switch( args[ i ] )
			{
				//---------------------------------

				// Windows用
				case "--platform-windows64" :
				case "--platform-windows" :
					platform	= BuildTarget.StandaloneWindows64 ;
				break ;

				// OSX用
				case "--platform-osx" :
				case "--platform-machintosh" :
					platform	= BuildTarget.StandaloneOSX ;
				break ;

				// Android用
				case "--platform-android" :
					platform	= BuildTarget.Android ;
				break ;

				// iOS用
				case "--platform-ios" :
					platform	= BuildTarget.iOS ;
				break ;

				// Linux用
				case "--platform-linux64" :
				case "--platform-linux" :
					platform	= BuildTarget.StandaloneLinux64 ;
				break ;

				//---------------------------------

				// Release版
				case "--build-release" :
					build = BuildTypes.Release ;
				break ;

				// Staging版
				case "--build-staging" :
					build = BuildTypes.Staging ;
				break ;

				// Development版
				case "--build-development" :
					build = BuildTypes.Development ;
				break ;

				// Profiler版
				case "--build-profiler" :
					build = BuildTypes.Profiler ;
				break ;

				//---------------------------------
			}
		}

		//-----------------------------------------------------------

		Debug.Log( "=================================================" ) ;
		Debug.Log( "------- Platform : " + platform.ToString() ) ;
		Debug.Log( "------- Build    : " + build.ToString() ) ;
		Debug.Log( "=================================================" ) ;

		// プラットフォームごとのビルドを実行する
		bool result = false ;

		switch( platform )
		{
			case BuildTarget.StandaloneWindows64	:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Runtime_Windows64_Mono_Release_BuildOnly()		; break ;
					case BuildTypes.Staging		: result = Execute_Runtime_Windows64_Mono_Staging_BuildOnly()		; break ;
					case BuildTypes.Development	: result = Execute_Runtime_Windows64_Mono_Development_BuildOnly()	; break ;
					case BuildTypes.Profiler	: result = Execute_Runtime_Windows64_Mono_Profiler_BuildOnly()		; break ;
				}
			break ;

			case BuildTarget.StandaloneOSX			:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Runtime_OSX_Mono_Release_BuildOnly()				; break ;
					case BuildTypes.Staging		: result = Execute_Runtime_OSX_Mono_Staging_BuildOnly()				; break ;
					case BuildTypes.Development	: result = Execute_Runtime_OSX_Mono_Development_BuildOnly()			; break ;
					case BuildTypes.Profiler	: result = Execute_Runtime_OSX_Mono_Profiler_BuildOnly()			; break ;
				}
			break ;

			case BuildTarget.Android				:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Runtime_Android_Mono_Release_BuildOnly()			; break ;
					case BuildTypes.Staging		: result = Execute_Runtime_Android_Mono_Staging_BuildOnly()			; break ;
					case BuildTypes.Development : result = Execute_Runtime_Android_Mono_Development_BuildOnly()		; break ;
					case BuildTypes.Profiler	: result = Execute_Runtime_Android_Mono_Profiler_BuildOnly()		; break ;
				}
			break ;

			case BuildTarget.iOS					:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Runtime_iOS_IL2CPP_Release_BuildOnly()			; break ;
					case BuildTypes.Staging		: result = Execute_Runtime_iOS_IL2CPP_Staging_BuildOnly()			; break ;
					case BuildTypes.Development : result = Execute_Runtime_iOS_IL2CPP_Development_BuildOnly()		; break ;
					case BuildTypes.Profiler	: result = Execute_Runtime_iOS_IL2CPP_Profiler_BuildOnly()			; break ;
				}
			break ;

			case BuildTarget.StandaloneLinux64	:
				switch( build )
				{
					case BuildTypes.Release		: result = Execute_Runtime_Linux64_Mono_Release_BuildOnly()			; break ;
					case BuildTypes.Staging		: result = Execute_Runtime_Linux64_Mono_Staging_BuildOnly()			; break ;
					case BuildTypes.Development	: result = Execute_Runtime_Linux64_Mono_Development_BuildOnly()		; break ;
					case BuildTypes.Profiler	: result = Execute_Runtime_Linux64_Mono_Profiler_BuildOnly()		; break ;
				}
			break ;

		}

		//-----------------------------------------------------------

		if( result == true )
		{
			// ビルド成功
			EditorApplication.Exit( 0 ) ;
		}
		else
		{
			// ビルド失敗
			EditorApplication.Exit( 1 ) ;
		}
	}

	//--------------------------------------------------------------------------------------------

	// ビルドターゲットとビルドターゲットグループの関係
	private static readonly Dictionary<BuildTarget,BuildTargetGroup> m_BuildTargetGroups = new ()
	{
		{ BuildTarget.StandaloneWindows,		BuildTargetGroup.Standalone	},
		{ BuildTarget.StandaloneWindows64,		BuildTargetGroup.Standalone	},
		{ BuildTarget.StandaloneOSX,			BuildTargetGroup.Standalone	},
		{ BuildTarget.Android,					BuildTargetGroup.Android	},
		{ BuildTarget.iOS,						BuildTargetGroup.iOS		},
	} ;

	// ビルドターゲットを変更する
	private static BuildTarget ChangeBuildTarget( BuildTarget target, string state )
	{
		// 現在のビルドターゲットを取得する
		BuildTarget activeBuildTarget =	EditorUserBuildSettings.activeBuildTarget ;
		if( target != activeBuildTarget )
		{
			if( state == "Change" )
			{
				string s = "[Bad build target] " + target + "\n" ;

				// 動的な変更はプリプロセッサに対応出来ないためワーニングを表示する(一応ビルドは出来るようにしておく)
				s += "================================" + "\n" ;
				s += "The specified build target is different from the current build target." + "\n" ;
				s += "[Specified build target] " + target + "\n" ;
				s += "[Current build target] " + activeBuildTarget + "\n" ;
				s += "" + "\n" ;
				s += "[Case] UnityEditor" + "\n" ;
				s += "Please change the current build target from the menu below ..." + "\n" ;
				s += " File -> Build Settings -> Platform" + "\n" ;
				s += " Set to [ " + target + " ]" + "\n" ;
				s += "" + "\n" ;
				s += "[Case] Commandline build" + "\n" ;
				s += "Please add the following options ..." + "\n" ;

				if( target == BuildTarget.StandaloneWindows64 )
				{
					s += " -buildTarget win64" + "\n" ;
				}
				else
				if( target == BuildTarget.StandaloneOSX )
				{
					s += " -buildTarget osx" + "\n" ;
				}
				else
				if( target == BuildTarget.Android )
				{
					s += " -buildTarget android" + "\n" ;
				}
				else
				if( target == BuildTarget.iOS )
				{
					s += " -buildTarget ios" + "\n" ;
				}

				s += "================================" + "\n" ;

				Debug.LogWarning( s ) ;
			}

			// ビルドターゲットが現在のビルドターゲットと異なる場合にビルドターゲットを切り替える
			EditorUserBuildSettings.SwitchActiveBuildTarget( m_BuildTargetGroups[ target ], target ) ;
			Debug.Log( "[" + state + "Platform] " + activeBuildTarget + " -> " + target ) ;
		}

		return activeBuildTarget ;
	}

	//--------------------------------------------------------------------------------------------

	// 実際にクライアントをビルドする処理
	private static bool Process
	(
		string			path,
		BuildTarget		target,
		BuildOptions	options
	)
	{
		//-----------------------------------------------------------

		// シェーダー内で使用しないキーワードの削除
		StripShaderValiant() ;

		//-----------------------------------

		// 有効なシーンを取得する
		string[] scenes = GetAvailableScenes() ;
		if( scenes == null || scenes.Length == 0 )
		{
			Debug.Log( "Build ERROR : Target scene could not found." ) ;
			return false ;
		}

		//-----------------------------------

		// ビルド実行
		BuildReport report = BuildPipeline.BuildPlayer( scenes, path, target, options ) ;
		BuildSummary summary = report.summary ;

		bool result = false ;

		if( summary.result == BuildResult.Failed	){ Debug.LogWarning( "Build failed"		) ; }
		else
		if( summary.result == BuildResult.Cancelled	){ Debug.LogWarning( "Build canceled"	) ; }
		else
		if( summary.result == BuildResult.Unknown	){ Debug.LogWarning( "Build unknown"	) ;	}
		else
		if( summary.result == BuildResult.Succeeded	)
		{
			// 成功(コンソールの Clear In Build にチェックが入っているとビルド前のコンソールは全て消去されてしまうのでビルド後のタイミングでログを表示する方が安全
			result = true ;

			// 対象のシーン
			string s = "Build Target Scene -> " + scenes.Length + "\n" ;
			foreach( var scene in scenes )
			{
				s += " + " + scene + "\n" ;
			}
			Debug.Log( s ) ;

			// 出力されたファイルのサイズ
			ulong size = 0 ;
			if( target == BuildTarget.iOS )
			{
				Debug.Log( "Build succeeded -> " + path + " | Total Size = " + GetSizeName( summary.totalSize ) ) ;
			}
			else
			{
				var fi = new FileInfo( path ) ;
				if( fi != null )
				{
					size = ( ulong )fi.Length ;
				}
				Debug.Log( "Build succeeded -> " + path + " : File Size = " + GetSizeName( size ) + " | Total Size = " + GetSizeName( summary.totalSize ) ) ;
			}
		}

		//-----------------------------------------------------------

		// 結果を返す
		return result ;
	}

	//----------------------------------------------------------------------------
	// プラットフォーム共通の処理メソッド群

	// 有効なシーンのパスのみ取得する
	private static string[] GetAvailableScenes()
	{
		var scenes = EditorBuildSettings.scenes ;

		var targetScenePaths = new List<string>() ;

		// 実際に存在するシーンファイルのみ対象とする
		string path ;
		string filterPath = m_SceneFileFilter ;

		if( string.IsNullOrEmpty( filterPath ) == false )
		{
			filterPath = filterPath.Replace( "\\", "/" ) ;
			filterPath = filterPath.TrimStart( '/' ) ;
		}

		bool isEnable ;
		foreach( var scene in scenes )
		{
			if( scene.enabled == true && string.IsNullOrEmpty( scene.path ) == false && File.Exists( scene.path ) == true )
			{
				isEnable = true ;

				if( string.IsNullOrEmpty( filterPath ) == false )
				{
					// シーンファイルのパスのフィルタが有効
					path = scene.path ;
					path = path.Replace( "\\", "/" ) ;
					path = path.TrimStart( '/' ) ;

					if( path.IndexOf( filterPath ) != 0 )
					{
						isEnable = false ;
					}
				}

				if( isEnable == true )
				{
					targetScenePaths.Add( scene.path ) ;
				}
			}
		}

		if( targetScenePaths.Count == 0 )
		{
			Debug.LogWarning( "Not Found Build Target Scene." ) ;
			return null ;
		}

		return targetScenePaths.ToArray() ;
	}

	// ファイルサイズを見やすい形に変える
	private static string GetSizeName( ulong size )
	{
		string sizeName = "Value Overflow" ;

		if( size <  1024L )
		{
			sizeName = size + " byte" ;
		}
		else
		if( size <  ( 1024L * 1024L ) )
		{
			sizeName = ( size / 1024L ) + " KB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
		{
			double value = ( double )size / ( double )( 1024L * 1024L * 1024L ) ;
			value = ( double )( ( int )( value * 1000 ) ) / 1000 ;	// 少数までわかるようにする
			sizeName = value + " GB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L ) ) + " TB" ;
		}
		else
		if( size <  ( 1024L * 1024L * 1024L * 1024L * 1024L * 1024L ) )
		{
			sizeName = ( size / ( 1024L * 1024L * 1024L * 1024L * 1024L ) ) + " PB" ;
		}

		return sizeName ;
	}

	/// <summary>
	/// シェーダー内で使用しないキーワードの削除
	/// </summary>
	private static void StripShaderValiant()
	{
	}

	//--------------------------------------------------------------------------------------------

	// 注意
	//   Time.realtimeSinceStartup は、Editor モードでは動作しない。
	//   Editor モードでの時間計測には DateTime を使用する必要がある。

	// 1970年01月01日 00時00分00秒 からの経過秒数を計算するためのエポック時間
	private static readonly DateTime m_UNIX_EPOCH = new ( 1970, 1, 1, 0, 0, 0, 0 ) ;

	private static long m_ProcessingTime ;

	// 時間計測を開始する
	private static void StartClock()
	{
		var dt = DateTime.Now ;
		var time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		m_ProcessingTime = ( long )time.TotalSeconds ;
	}

	// 時間計測を終了する
	private static void StopClock()
	{
		var dt = DateTime.Now ;
		var time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		long processtingTime = ( long )time.TotalSeconds - m_ProcessingTime ;

		long hour   = ( processtingTime / 3600L ) ;
		processtingTime %= 3600 ;
		long minute = ( processtingTime /   60L ) ;
		processtingTime %=   60 ;
		long second =   processtingTime ;

		Debug.Log( "Processing Time -> " + hour.ToString( "D2" ) + ":" + minute.ToString( "D2" ) + ":" + second.ToString( "D2" ) ) ;
	}


	//--------------------------------------------------------------------------------------------
	// Git の情報を得る

	/// <summary>
	/// Android用(Release) - Mono
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/GitInfo", priority = 20 )]
	internal static string Execute_GitInfo()
	{
		string activeBranchName = string.Empty ;
		string lastCommitCode = string.Empty ;

		DateTime dt ;
		string date ;

		//-----------------------------------------------------------
#if false

		string path ;
		string text ;
		string[] lines ;
		string[] words ;
		int i, l ;

		//-----------------------------------------------------------

		string branchName = string.Empty ;
		path = ".git/HEAD" ;

		text = File.ReadAllText( path ) ;
		if( string.IsNullOrEmpty( text ) == false )
		{
			// 改行統一
			text = text.Replace( "\n", "\x0A" ) ;
			text = text.Replace( "\x0D\x0A", "\x0A" ) ;
			text = text.Replace( "\x0D", "\x0A" ) ;

			// 改行除去
			text = text.Replace( "\x0A", "" ) ;

			text = text.Replace( '\t', ' ' ) ;
			words = text.Split( ' ' ) ;

			if( words != null && words.Length >= 2 )
			{
				branchName = words[ 1 ] ;
			}

			// ローカルのアクティブなブランチの名前を取得する
			activeBranchName = branchName.Replace( "refs/heads/", "" ) ;

//			Debug.Log( "<color=#00FF00>BranchName = " + branchName + "</color>" ) ;
		}
		else
		{
			Debug.LogWarning( "Could not load file : Path = " + path ) ;
			return string.Empty ;
		}

		//-----------------------------------------------------------
		// 最終コミット日時

		string lastCommitLine = string.Empty ;

		path = ".git/logs/HEAD" ;
//		path = ".git/logs/refs/remotes/origin/HEAD" ;

		text = File.ReadAllText( path ) ;
		if( string.IsNullOrEmpty( text ) == false )
		{
			// 改行統一
			text = text.Replace( "\n", "\x0A" ) ;
			text = text.Replace( "\x0D\x0A", "\x0A" ) ;
			text = text.Replace( "\x0D", "\x0A" ) ;

			lines = text.Split( '\x0A' ) ;
			if( lines != null && lines.Length >= 1 )
			{
				// 最後から検索して有効な行を探す

				// リモートブランチのタイムスタンプを見るべきかどうか
				bool isRemote = false ;

				l = lines.Length ;

				for( i  = l - 1 ; i >= 0 ; i -- )
				{
					text = lines[ i ] ;

					// 改行除去(実際は不要)
					text = text.Replace( "\0x0A", "" ) ;

					if( string.IsNullOrEmpty( text ) == false )
					{
						// 有効な行を発見

						text = text.Replace( '\t', ' ' ) ;
						words = text.Split( ' ' ) ;

						if( words != null && words.Length >= 8 )
						{
							if( words[  6 ].IndexOf( "commit:" ) == 0 )
							{
								// 確定
								lastCommitLine = lines[ i ] ;
								break ;
							}
							else
							if( words[  6 ].IndexOf( "checkout:" ) == 0 && words[  7 ].IndexOf( "moving" ) == 0 )
							{
								// アクティブなブランチを切り替える
								if( words[ 11 ] == activeBranchName )
								{
									activeBranchName = words[  9 ] ;
								}
							}
							else
							if( words[  6 ].IndexOf( "pull" ) == 0 )
							{
								// リモートブランチ確定
								isRemote = true ;
								break ;
							}
						}
					}
				}

				//---------------------------------------------------------

				if( isRemote == true )
				{
					// リモートブランチの最終コミット情報を見る必要がある

					path = ".git/logs/refs/remotes/origin/" + activeBranchName ;

					text = File.ReadAllText( path ) ;
					if( string.IsNullOrEmpty( text ) == false )
					{
						// 改行統一
						text = text.Replace( "\n", "\x0A" ) ;
						text = text.Replace( "\x0D\x0A", "\x0A" ) ;
						text = text.Replace( "\x0D", "\x0A" ) ;

						lines = text.Split( '\x0A' ) ;
						if( lines != null && lines.Length >= 1 )
						{
							// 最後から検索して有効な行を探す

							l = lines.Length ;
							for( i  = l - 1 ; i >= 0 ; i -- )
							{
								text = lines[ i ] ;

								// 改行除去(実際は不要)
								text = text.Replace( "\0x0A", "" ) ;

								if( string.IsNullOrEmpty( text ) == false )
								{
									// 有効な行を発見
									lastCommitLine = text ;

									// リモートの場合は最初にヒットした有効な行で良い(pull fetch 関係なし)
									break ;
								}
							}
						}
					}
				}

//				Debug.Log( "ListCommitLine:" + lastCommitLine ) ;

				//---------------------------------------------------------

				if( string.IsNullOrEmpty( lastCommitLine ) == false )
				{
					lastCommitLine = lastCommitLine.Replace( '\t', ' ' ) ;
					words = lastCommitLine.Split( ' ' ) ;

					if( isRemote == false )
					{
						// ローカルブランチ(日時を使用する)
						if( words != null && words.Length >= 6 )
						{
#if false
							// 最終コミット日時を使うバージョン
							string tick_string = words[ 4 ] ;
							string zisa_string = words[ 5 ] ;

							if( long.TryParse( tick_string, out long tick ) == false )
							{
								tick = 0 ;
							}

							if( long.TryParse( zisa_string, out long zisa ) == false )
							{
								zisa = 0 ;
							}

							if( tick != 0 )
							{
								if( zisa != 0 )
								{
									// 補正を加える
									long zisa_hour   = ( long )( zisa / 100 ) ;
									long zisa_minute = ( long )( zisa % 100 ) ;

									tick += ( ( zisa_hour * 3600 ) + ( zisa_minute * 60 ) ) ;
								}

								dt = ToDateTime( tick ) ;

								date =
									dt.Year.ToString( "D4" ) + "/" + dt.Month.ToString( "D2" ) + "/" + dt.Day.ToString( "D2" ) + " " +
									dt.Hour.ToString( "D2" ) + ":" + dt.Minute.ToString( "D2" ) + ":" + dt.Second.ToString( "D2" ) ;

								Debug.Log( "<color=#00FF00>LastCommitDate = " + date + "</color>" ) ;

								lastCommitCode =
									dt.Year.ToString( "D4" ) + dt.Month.ToString( "D2" ) + dt.Day.ToString( "D2" ) +
									dt.Hour.ToString( "D2" ) + dt.Minute.ToString( "D2" ) + dt.Second.ToString( "D2" ) ;
							}
#endif
							// ローカルブランチのハッシュを使用する
							lastCommitCode = words[ 1 ].Substring( 0, 14 ) ;
						}
					}
					else
					{
						// リモートブランチのハッシュを使用する
						if( words != null && words.Length >= 2 )
						{
							lastCommitCode = words[ 1 ].Substring( 0, 14 ) ;
						}
					}

					Debug.Log( "<color=#FF7F>ActiveBranchName = " + activeBranchName + "</color>" ) ;
					Debug.Log( "<color=#00FF00>LastCommitCode = " + lastCommitCode + "</color>" ) ;
				}
			}
		}
		else
		{
			Debug.LogWarning( "Could not load file : Path = " + path ) ;
		}
#endif
		//-----------------------------------------------------------

		string result = string.Empty ;

		if( string.IsNullOrEmpty( activeBranchName ) == false && string.IsNullOrEmpty( lastCommitCode ) == false )
		{
			result = lastCommitCode + "[" + activeBranchName[ 0..2 ] + "]" ;
		}
		else
		{
			result = "--------------[-]" ;
		}

		// 現在の日時
		dt = ToDateTime() ;
		date =
			dt.Year.ToString( "D4" ) + "/" + dt.Month.ToString( "D2" ) + "/" + dt.Day.ToString( "D2" ) + " " +
			dt.Hour.ToString( "D2" ) + ":" + dt.Minute.ToString( "D2" ) + ":" + dt.Second.ToString( "D2" ) ;

		result += " " + date ;

		Debug.Log( "<color=#00FFFF>result = " + result + "</color>" ) ;

		return result ;
	}

	/// <summary>
	/// Unixエポックからの日時に変換する
	/// </summary>
	/// <param name="tick"></param>
	/// <returns></returns>
	private static DateTime ToDateTime( long tickTime = 0 )
	{
		if( tickTime <= 0 )
		{
			return DateTime.Now ;
		}

		// 1970/01/01 を加算する
		var dateTime = m_UNIX_EPOCH.AddSeconds( tickTime ) ;

		// タイムゾーンの補正を加算する(一旦不要)
//		TimeZone zone = TimeZone.CurrentTimeZone ;
//		TimeSpan offset = zone.GetUtcOffset( DateTime.Now ) ;
//		dateTime += offset ;

		return dateTime ;
	}
}
