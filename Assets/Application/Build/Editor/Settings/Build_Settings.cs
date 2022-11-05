using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEditor ;
using UnityEngine ;

using DSW ;

/// <summary>
/// アプリケーションのバッチビルド用クラス:設定
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 設定

	//------------------------------------
	// Windows64用

	private const string	m_Path_Windows64								= "DBS/dbs.exe" ;

	private static string	VersionName_Windows64
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.Windows64 ) ;
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

	private const string	m_Path_OSX										= "DBS/dbs.app" ;

	private static string	VersionName_OSX
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.OSX ) ;
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

	private const string	m_Path_Android									= "dbs.apk" ;

	private static string	VersionName_Android
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.Android ) ;
			return versionName ;
		}
	}
	private static int VersionCode_Android
	{
		get
		{
			( _, int versionCode ) = GetVersion( PlatformTypes.Android ) ;
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
	private const string	m_KeyStorePassword_Android						= "dbs_2020" ;
	private const string	m_KeyStoreAlias_Android							= "dbs_alias" ;
	private const string	m_KeyStoreAliasPassword_Android					= "dbs_2020" ;

	//------------------------------------
	// iOS用

	private const string m_Path_iOS = "Xcode" ;

	private static string VersionName_iOS
	{
		get
		{
			( string versionName, _ ) = GetVersion( PlatformTypes.iOS ) ;
			return versionName ;
		}
	}

	private static int VersionCode_iOS
	{
		get
		{
			( _, int versionCode ) = GetVersion( PlatformTypes.iOS ) ;
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




}
