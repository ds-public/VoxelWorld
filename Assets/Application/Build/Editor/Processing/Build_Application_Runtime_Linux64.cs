using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;
using UnityEngine.Rendering ;

using UnityEditor ;
using UnityEditor.Build ;
using UnityEditor.Build.Reporting ;

using DSW ;


/// <summary>
/// アプリケーションのバッチビルド用クラス(Linux64用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 処理

	//--------------------------------------------------------------------------------------------
	// for Mono

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Release/BuildOnly", priority = 10 )]
	internal static bool Execute_Runtime_Linux64_Mono_Release_BuildOnly()
	{
		return Execute_Runtime_Linux64_Mono_Release( false ) ;
	}

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Release/BuildAndRun", priority = 11 )]
	internal static bool Execute_Runtime_Linux64_Mono_Release_BuildAndRun()
	{
		return Execute_Runtime_Linux64_Mono_Release( true ) ;
	}

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	internal static bool Execute_Runtime_Linux64_Mono_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Release,
			m_Identifier_Linux64_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Staging/BuildOnly", priority = 20 )]
	internal static bool Execute_Runtime_Linux64_Mono_Staging_BuildOnly()
	{
		return Execute_Runtime_Linux64_Mono_Staging( false ) ;
	}

	/// <summary>
	/// Linux64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Staging/BuildAndRun", priority = 21 )]
	internal static bool Execute_Runtime_Linux64_Mono_Staging_BuildAndRun()
	{
		return Execute_Runtime_Linux64_Mono_Staging( true ) ;
	}

	/// <summary>
	/// Linux64用(Staging)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Runtime_Linux64_Mono_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Staging,
			m_Identifier_Linux64_Staging,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Development(NotDebug)/BuildOnly", priority = 30 )]
	internal static bool Execute_Runtime_Linux64_Mono_Development_NotDebug_BuildOnly()
	{
		return Execute_Runtime_Linux64_Mono_Development( true, false ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Development(NotDebug)/BuildAndRun", priority = 31 )]
	internal static bool Execute_Runtime_Linux64_Mono_Development_NotDebug_BuildAndRun()
	{
		return Execute_Runtime_Linux64_Mono_Development( true, true ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Development/BuildOnly", priority = 32 )]
	internal static bool Execute_Runtime_Linux64_Mono_Development_BuildOnly()
	{
		return Execute_Runtime_Linux64_Mono_Development( false, false ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Development/BuildAndRun", priority = 33 )]
	internal static bool Execute_Runtime_Linux64_Mono_Development_BuildAndRun()
	{
		return Execute_Runtime_Linux64_Mono_Development( false, true ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Runtime_Linux64_Mono_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options	;

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

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Development,
			m_Identifier_Linux64_Development,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Profiler/BuildOnly", priority = 40 )]
	internal static bool Execute_Runtime_Linux64_Mono_Profiler_BuildOnly()
	{
		return Execute_Runtime_Linux64_Mono_Profiler( false ) ;
	}

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Mono/Profiler/BuildAndRun", priority = 41 )]
	internal static bool Execute_Runtime_Linux64_Mono_Profiler_BuildAndRun()
	{
		return Execute_Runtime_Linux64_Mono_Profiler( true ) ;
	}

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Runtime_Linux64_Mono_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Profiler,
			m_Identifier_Linux64_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// for IL2CPP

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Release/BuildOnly", priority = 10 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Release_BuildOnly()
	{
		return Execute_Runtime_Linux64_IL2CPP_Release( false ) ;
	}

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Release/BuildAndRun", priority = 11 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Release_BuildAndRun()
	{
		return Execute_Runtime_Linux64_IL2CPP_Release( true ) ;
	}

	/// <summary>
	/// Linux64用(Release)
	/// </summary>
	/// <returns></returns>
	internal static bool Execute_Runtime_Linux64_IL2CPP_Release( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

//		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
//		{
//			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
//			return false ;
//		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Release,
			m_Identifier_Linux64_Release,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Staging/BuildOnly", priority = 20 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Staging_BuildOnly()
	{
		return Execute_Runtime_Linux64_IL2CPP_Staging( false ) ;
	}

	/// <summary>
	/// Linux64用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Staging/BuildAndRun", priority = 21 )]
	internal static bool Execute_Linux64_IL2CPP_Staging_BuildAndRun()
	{
		return Execute_Runtime_Linux64_IL2CPP_Staging( true ) ;
	}

	// Linux64 - Staging
	private static bool Execute_Runtime_Linux64_IL2CPP_Staging( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

//		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
//		{
//			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
//			return false ;
//		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.None ;	// ひとまずビルド速度優先と自動実行
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Staging,
			m_Identifier_Linux64_Staging,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Development(NotDebug)/BuildOnly", priority = 30 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Development_NotDebug_BuildOnly()
	{
		return Execute_Runtime_Linux64_IL2CPP_Development( true, false ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Development(NotDebug)/BuildAndRun", priority = 31 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Development_NotDebug_BuildAndRun()
	{
		return Execute_Runtime_Linux64_IL2CPP_Development( true, true ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Development/BuildOnly", priority = 32 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Development_BuildOnly()
	{
		return Execute_Runtime_Linux64_IL2CPP_Development( false, false ) ;
	}

	/// <summary>
	/// Linux64用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Development/BuildAndRun", priority = 33 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Development_BuildAndRun()
	{
		return Execute_Runtime_Linux64_IL2CPP_Development( false, true ) ;
	}

	// Linux64 - Development
	private static bool Execute_Runtime_Linux64_IL2CPP_Development( bool notDebug, bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

//		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
//		{
//			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
//			return false ;
//		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options	;

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

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Development,
			m_Identifier_Linux64_Development,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		//-----------------------------------

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Profiler/BuildOnly", priority = 40 )]
	internal static bool Execute_Runtime_Linux64_IL2CPP_Profiler_BuildOnly()
	{
		return Execute_Runtime_Linux64_IL2CPP_Profiler( false ) ;
	}

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/IL2CPP/Profiler/BuildAndRun", priority = 41 )]
	internal static bool Execute_Linux64_IL2CPP_Profiler_BuildAndRun()
	{
		return Execute_Runtime_Linux64_IL2CPP_Profiler( true ) ;
	}

	/// <summary>
	/// Linux64用(Profiler)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_Runtime_Linux64_IL2CPP_Profiler( bool andRun )
	{
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Standalone ) ;
		if( settings != ScriptingImplementation.IL2CPP )
		{
			EditorUtility.DisplayDialog( "注意", "IL2CPP が有効化されてません", "閉じる" ) ;
			return false ;
		}

//		if( File.Exists( m_MessagePack_Resolver_Path ) == false )
//		{
//			EditorUtility.DisplayDialog( "注意", "MessagePack の IL2CPP 用コードが生成されてません", "閉じる" ) ;
//			return false ;
//		}

		//-----------------------------------

		PushSetting() ;	// 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Development ) ;
		SetDevelopmentMode( true ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions	options		= BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.EnableDeepProfilingSupport | BuildOptions.AllowDebugging | BuildOptions.WaitForPlayerConnection ;
		if( andRun == true )
		{
			options |= BuildOptions.AutoRunPlayer ;
		}

		bool result = Execute_Runtime_Linux64_General
		(
			m_ProductName_Linux64_Profiler,
			m_Identifier_Linux64_Profiler,
			options
		) ;

		PopSetting() ;	// 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// Linux64用でMonoを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Switch Mono", priority = 80 )]
	internal static bool Execute_Runtime_Linux64_Switch_Mono()
	{
		// Mono を有効化する
		PlayerSettings.SetScriptingBackend( NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX or Linux64 を Mono でビルドします", "閉じる" ) ;

		return true ;
	}

	/// Linux64用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/Runtime/Linux64/Switch IL2CPP", priority = 81 )]
	internal static bool Execute_Runtime_Linux64_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX or Linux64 を IL2CPP でビルドします", "閉じる" ) ;

		return true ;
	}

	//------------------------------------------------------------

	// Linux64 共通
	private static bool Execute_Runtime_Linux64_General( string productName, string identifier, BuildOptions options )
	{
		BuildTarget		target		= BuildTarget.StandaloneLinux64 ;
		string			path		= m_Path_Linux64 ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------

		// 固有の設定
		PlayerSettings.productName					= productName ;
		PlayerSettings.SetApplicationIdentifier( NamedBuildTarget.Standalone, identifier ) ;

		PlayerSettings.bundleVersion				= VersionName_Linux64 ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_LINUX
#if false
		DisableUnityAudio( false ) ;	// CriWareに無効化される対策
#endif
#endif
		//-----------------------------------------------------------

		// ビルドを実行する
		bool result = Process( path, target, options ) ;

		// ビルドターゲットを復帰する
		ChangeBuildTarget( activeBuildTarget, "Revert" ) ;

		// 処理時間計測終了
		StopClock() ;

		Debug.Log( "-------> Target : " + target + " " + productName ) ;

		return result ;
	}

	//------------------------------------
	// 固有の設定群

	// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_LINUX
#if false
	// Unityオーディオの再生の有無を設定する
	private static void DisableUnityAudio( bool state )
	{
		string				path		= "ProjectSettings/AudioManager.asset";
		UnityEngine.Object	manager		= AssetDatabase.LoadAllAssetsAtPath( path ).FirstOrDefault() ;
		SerializedObject	target		= new SerializedObject( manager ) ;
		SerializedProperty	property	= target.FindProperty( "m_DisableAudio" ) ;

		property.boolValue = state ;
		target.ApplyModifiedProperties() ;
	}
#endif
#endif
}
