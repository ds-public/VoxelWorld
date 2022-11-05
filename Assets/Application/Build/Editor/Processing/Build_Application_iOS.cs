#if UNITY_IOS || UNITY_IPHONE
#define BUILD_IOS
#endif

// デバッグ
//#define BUILD_IOS

//-------------------------------------

using System.IO ;

using UnityEditor ;

using UnityEngine ;
#if BUILD_IOS
#endif

using DSW ;

/// <summary>
/// アプリケーションのバッチビルド用クラス(iOS用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 処理

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Release/BuildOnly", priority = 10)]
	internal static bool Execute_iOS_Release_BuildOnly()
	{
		return Execute_iOS_Release( false ) ;
	}

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Release/BuildAndRun", priority = 11)]
	internal static bool Execute_iOS_Release_BuildAndRun()
	{
		return Execute_iOS_Release( true ) ;
	}

	/// <summary>
	/// iOS用(Release)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
		{
			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ; // ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Release,
			m_Identifier_iOS_Release,
			options,
			false
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Staging/BuildOnly", priority = 20)]
	internal static bool Execute_iOS_Staging_BuildOnly()
	{
		return Execute_iOS_Staging( false ) ;
	}

	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Staging/BuildAndRun", priority = 21)]
	internal static bool Execute_iOS_Staging_BuildAndRun()
	{
		return Execute_iOS_Staging( true ) ;
	}

	/// <summary>
	/// iOS用(Staging)
	/// </summary>
	/// <param name="andRun"></param>
	/// <param name="isDevelopment">開発用に近しいものにするのか</param>
	/// <returns></returns>
	internal static bool Execute_iOS_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
		{
			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ; // ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Staging,
			m_Identifier_iOS_Staging,
			options,
			true
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30)]
	internal static bool Execute_iOS_Development_NotDebug_BuildOnly()
	{
		return Execute_iOS_Development( true, false ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31)]
	internal static bool Execute_iOS_Development_NotDebug_BuildAndRun()
	{
		return Execute_iOS_Development( true, true ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development/BuildOnly", priority = 32)]
	internal static bool Execute_iOS_Development_BuildOnly()
	{
		return Execute_iOS_Development( false, false ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Development/BuildAndRun", priority = 33)]
	internal static bool Execute_iOS_Development_BuildAndRun()
	{
		return Execute_iOS_Development( false, true ) ;
	}

	/// <summary>
	/// iOS用(Development)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
		{
			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options ;

		if( notDebug == true )
		{
			options = BuildOptions.None ;
		}
		else
		{
			options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		}

		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Development,
			m_Identifier_iOS_Development,
			options,
			true
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Profiler/BuildOnly", priority = 40)]
	internal static bool Execute_iOS_Profiler_BuildOnly()
	{
		return Execute_iOS_Profiler( false ) ;
	}

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/IL2CPP/Profiler/BuildAndRun", priority = 41)]
	internal static bool Execute_iOS_Profiler_BuildAndRun()
	{
		return Execute_iOS_Profiler( true ) ;
	}

	/// <summary>
	/// iOS用(Profiler)
	/// </summary>
	/// <param name="andRun"></param>
	/// <returns></returns>
	private static bool Execute_iOS_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( BuildTargetGroup.iOS ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
		{
			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ; // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options	= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;

		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

#if BUILD_IOS
		bool result = Execute_iOS_General
		(
			m_ProductName_iOS_Profiler,
			m_Identifier_iOS_Profiler,
			options,
			true
		) ;
#else
		bool result = false ;
#endif

		PopSetting() ; // 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// iOS用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/Application/iOS/Switch IL2CPP", priority = 80)]
	internal static bool Execute_iOS_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "iOS を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}

	//-------------------------------------------------------------------------------------------------
#if BUILD_IOS
	/// <summary>
	/// iOSビルド設定をしてビルドを行います
	/// </summary>
	/// <param name="productName"></param>
	/// <param name="identifier"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	private static bool Execute_iOS_General
	(
		string        productName,
		string        identifier,
		BuildOptions  options,
		bool          liappIsOff
	)
	{
		string      path   = m_Path_iOS ;
		BuildTarget target = BuildTarget.iOS ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------
		// 固有の設定

		// Librariesフォルダはシンボリックリンクにする (ビルド時間短縮に期待)
		// https://docs.unity3d.com/2020.3/Documentation/ScriptReference/BuildOptions.SymlinkLibraries.html
		options |= BuildOptions.SymlinkLibraries ;

		PlayerSettings.productName = productName ;
		PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.iOS, identifier ) ;

		PlayerSettings.bundleVersion = VersionName_iOS ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラス(PlayerSettings.iOS)はコンパイルエラーになるのでプリプロセッサで抑制すること
		string oldAppleDeveloperTeamID           = PlayerSettings.iOS.appleDeveloperTeamID ;
		string oldiOSManualProvisioningProfileID = PlayerSettings.iOS.iOSManualProvisioningProfileID ;
		ProvisioningProfileType oldiOSManualProvisioningProfileType =
			PlayerSettings.iOS.iOSManualProvisioningProfileType ;

		PlayerSettings.iOS.buildNumber            = VersionCode_iOS.ToString() ; // ひとまず ProjectSettings のものをそのまま使用する
		PlayerSettings.iOS.applicationDisplayName = productName ;
		PlayerSettings.iOS.appleDeveloperTeamID   = null ;
		PlayerSettings.iOS.appleEnableAutomaticSigning           = false ;
		PlayerSettings.iOS.iOSManualProvisioningProfileID        = null ;
		PlayerSettings.iOS.iOSManualProvisioningProfileType      = ProvisioningProfileType.Automatic ;

		//-----------------------------------------------------------

		// ビルドを実行する
		bool result = Process
		(
			path,
			target,
			options,
			liappIsOff
		) ;

		// ビルドターゲットを復帰する
		ChangeBuildTarget( activeBuildTarget, "Revert" ) ;

		// Gitにコミットすべきない情報を元に戻す
		PlayerSettings.iOS.appleDeveloperTeamID             = oldAppleDeveloperTeamID ;
		PlayerSettings.iOS.iOSManualProvisioningProfileID   = oldiOSManualProvisioningProfileID ;
		PlayerSettings.iOS.iOSManualProvisioningProfileType = oldiOSManualProvisioningProfileType ;

		// 処理時間計測終了
		StopClock() ;

		Debug.Log( "-------> Target : " + target + " " + productName ) ;

		return result ;
	}
#endif
}
