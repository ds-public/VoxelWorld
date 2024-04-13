using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEditor ;
using UnityEngine ;


/// <summary>
/// アプリケーションのバッチビルド用クラス:設定
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	//------------------------------------
	// Windows64用

	private const string	m_Path_Windows64								= "Runtime/Windows64/dbs.exe" ;

	private static string	VersionName_Windows64
	{
		get
		{
			( string versionName, _ ) = GetRuntimeVersion( RuntimePlatformTypes.Windows64 ) ;
			return versionName ;
		}
	}

	private const string	m_ProductName_Windows64_Release					= "DBS" ;
	private const string	m_ProductName_Windows64_Staging					= "DBS(S)" ;
	private const string	m_ProductName_Windows64_Development				= "DBS(D)" ;
	private const string	m_ProductName_Windows64_Profiler				= "DBS(P)" ;

	private const string	m_Identifier_Windows64_Release					= "com.dbs" ;
	private const string	m_Identifier_Windows64_Staging					= "com.dbs" ;
	private const string	m_Identifier_Windows64_Development				= "com.dbs" ;
	private const string	m_Identifier_Windows64_Profiler					= "com.dbs" ;

	//------------------------------------
	// OSX用

	private const string	m_Path_OSX										= "Runtime/OSX/dbs.app" ;

	private static string	VersionName_OSX
	{
		get
		{
			( string versionName, _ ) = GetRuntimeVersion( RuntimePlatformTypes.OSX ) ;
			return versionName ;
		}
	}

	private const string	m_ProductName_OSX_Release						= "DBS" ;
	private const string	m_ProductName_OSX_Staging						= "DBS(S)" ;
	private const string	m_ProductName_OSX_Development					= "DBS(D)" ;
	private const string	m_ProductName_OSX_Profiler						= "DBS(P)" ;

	private const string	m_Identifier_OSX_Release						= "com.dbs" ;
	private const string	m_Identifier_OSX_Staging						= "com.dbs" ;
	private const string	m_Identifier_OSX_Development					= "com.dbs" ;
	private const string	m_Identifier_OSX_Profiler						= "com.dbs" ;

	private const string	m_DevelopmentTeam_OSX							= "ABCDEFGHIJ" ;
	private const string	m_CodeSignIdentity_OSX							= "iPhone Distribution: DBS Co.,Ltd." ;
	private const string	m_ProvisioningProfileSpecifier_OSX				= "DBS_Enterprise" ;

	//------------------------------------
	// Android用

	// デフォルトアプリケーション出力パス
	private const string	m_Path_Android									= "Runtime/Android/dbs.apk" ;

	// アプリケーション出力パス
	private static string GetExportFilePathForAndroid()
	{
		var appName = Path.GetFileNameWithoutExtension( m_Path_Android ) ;

		string folderName = string.Empty ;
		int i = m_Path_Android.LastIndexOf( '/' ) ;
		if( i >= 0 )
		{
			i ++ ;
			folderName = m_Path_Android[ ..i ] ;
		}

		return folderName + ( EditorUserBuildSettings.buildAppBundle ? appName + ".aab" : appName + ".apk" ) ;
	}

	// バージョンネーム
	private static string	VersionName_Android
	{
		get
		{
			( string versionName, _ ) = GetRuntimeVersion( RuntimePlatformTypes.Android ) ;
			return versionName ;
		}
	}

	// バージョンコード
	private static int VersionCode_Android
	{
		get
		{
			( _, int versionCode ) = GetRuntimeVersion( RuntimePlatformTypes.Android ) ;
			return versionCode ;
		}
	}

	//----------------

	private const string	m_ProductName_Android_Release					= "DBS" ;
	private const string	m_ProductName_Android_Review					= "DBS(R)" ;
	private const string	m_ProductName_Android_Staging					= "DBS(S)" ;
	private const string	m_ProductName_Android_Development				= "DBS(D)" ;
	private const string	m_ProductName_Android_Profiler					= "DBS(P)" ;

	private const string	m_Identifier_Android_Release					= "com.dbs" ;
	private const string	m_Identifier_Android_Review						= "com.dbs" ;
	private const string	m_Identifier_Android_Staging					= "com.dbs" ;
	private const string	m_Identifier_Android_Development				= "com.dbs" ;
	private const string	m_Identifier_Android_Profiler					= "com.dbs" ;

	//----------------

	private const string	m_KeyStorePath_Android							= "Build/Android/dbs.keystore" ;
	private const string	m_KeyStorePassword_Android						= "dbs_2024" ;
	private const string	m_KeyStoreAlias_Android							= "dbs_alias" ;
	private const string	m_KeyStoreAliasPassword_Android					= "dbs_2024" ;

	//------------------------------------
	// iOS用

	private const string m_Path_iOS											= "Runtime/iOS/Xcode" ;

	private static string VersionName_iOS
	{
		get
		{
			( string versionName, _ ) = GetRuntimeVersion( RuntimePlatformTypes.iOS ) ;
			return versionName ;
		}
	}

	private static int VersionCode_iOS
	{
		get
		{
			( _, int versionCode ) = GetRuntimeVersion( RuntimePlatformTypes.iOS ) ;
			return versionCode ;
		}
	}

	private const string m_ProductName_iOS_Release		= "DBS" ;
	private const string m_ProductName_iOS_Review		= "DBS_R" ;
	private const string m_ProductName_iOS_Staging		= "DBS_S" ;
	private const string m_ProductName_iOS_Development	= "DBS_D" ;
	private const string m_ProductName_iOS_Profiler		= "DBS_P" ;

	//TODO: デフォルトのIdentifier
	private const string m_Identifier_iOS_Release		= "com.dbs" ;
	private const string m_Identifier_iOS_Review		= "com.dbs" ;
	private const string m_Identifier_iOS_Staging		= "com.dbs" ;
	private const string m_Identifier_iOS_Development	= "com.dbs" ;
	private const string m_Identifier_iOS_Profiler		= "com.dbs" ;

	//------------------------------------
	// Linux64用

	private const string	m_Path_Linux64								    = "Runtime/Linus64/dbs" ;

	private static string	VersionName_Linux64
	{
		get
		{
			( string versionName, _ ) = GetRuntimeVersion( RuntimePlatformTypes.Linux64 ) ;
			return versionName ;
		}
	}

	private const string	m_ProductName_Linux64_Release					= "DBS" ;
	private const string	m_ProductName_Linux64_Staging					= "DBS(S)" ;
	private const string	m_ProductName_Linux64_Development				= "DBS(D)" ;
	private const string	m_ProductName_Linux64_Profiler				    = "DBS(P)" ;

	private const string	m_Identifier_Linux64_Release					= "com.dbs" ;
	private const string	m_Identifier_Linux64_Staging					= "com.dbs" ;
	private const string	m_Identifier_Linux64_Development				= "com.dbs" ;
	private const string	m_Identifier_Linux64_Profiler					= "com.dbs" ;


	//------------------------------------
	// Windows64 の DedicatedServer用

	private const string m_Path_DedicatedServer_Windows64					= "DedicatedServer/Windows64/dbs.exe" ;

	private static string VersionName_DedicatedServer_Windows64
	{
		get
		{
			( string versionName, _) = GetDedicatedServerVersion( DedicatedServerPlatformTypes.Windows64 ) ;
			return versionName ;
		}
	}

	private const string m_ProductName_DedicatedServer_Windows64_Release        = "DBS" ;
	private const string m_ProductName_DedicatedServer_Windows64_Staging        = "DBS(S)" ;
	private const string m_ProductName_DedicatedServer_Windows64_Development    = "DBS(D)" ;

	private const string m_Identifier_DedicatedServer_Windows64_Release         = "com.dbs" ;
	private const string m_Identifier_DedicatedServer_Windows64_Staging         = "com.dbs" ;
	private const string m_Identifier_DedicatedServer_Windows64_Development     = "com.dbs" ;


	//------------------------------------
	// Linux64 の DedicatedServer用

	private const string m_Path_DedicatedServer_Linux64						= "DedicatedServer/Linux64/dbs" ;

	private static string VersionName_DedicatedServer_Linux64
	{
		get
		{
			( string versionName, _ ) = GetDedicatedServerVersion( DedicatedServerPlatformTypes.Linux64 ) ;
			return versionName ;
		}
	}

	private const string m_ProductName_DedicatedServer_Linux64_Release        = "DBS" ;
	private const string m_ProductName_DedicatedServer_Linux64_Staging        = "DBS(S)" ;
	private const string m_ProductName_DedicatedServer_Linux64_Development    = "DBS(D)" ;

	private const string m_Identifier_DedicatedServer_Linux64_Release         = "com.dbs" ;
	private const string m_Identifier_DedicatedServer_Linux64_Staging         = "com.dbs" ;
	private const string m_Identifier_DedicatedServer_Linux64_Development     = "com.dbs" ;
}
