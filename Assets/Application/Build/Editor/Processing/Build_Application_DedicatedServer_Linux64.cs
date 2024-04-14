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
/// アプリケーションのバッチビルド用クラス(DedicatedServer用)
/// </summary>
public partial class Build_Application
{
	//--------------------------------------------------------------------------------------------
	// 処理

	/// <summary>
	/// Dedicated ServerのLinux64用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Mono/Release", priority = 10 )]
	internal static bool Execute_DedicatedServer_Linux64_Mono_Release()
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;  // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ;   // ひとまずビルド速度優先

		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Release,
			m_Identifier_DedicatedServer_Linux64_Release,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Mono/Staging", priority = 20 )]
	private static bool Execute_DedicatedServer_Linux64_Mono_Staging()
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;  // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ;   // ひとまずビルド速度優先

		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Staging,
			m_Identifier_DedicatedServer_Linux64_Staging,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Mono/Development(NotDebug)", priority = 30 )]
	internal static bool Execute_DedicatedServer_Linux64_Mono_Development_NotDebug()
	{
		return Execute_DedicatedServer_Linux64_Mono_Development( true ) ;
	}

	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Mono/Development", priority = 31 )]
	internal static bool Execute_DedicatedServer_Linux64_Mono_Development()
	{
		return Execute_DedicatedServer_Linux64_Mono_Development( false ) ;
	}


	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_DedicatedServer_Linux64_Mono_Development( bool notDebug )
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
		if( settings != ScriptingImplementation.Mono2x )
		{
			EditorUtility.DisplayDialog( "注意", "Mono が有効化されてません", "閉じる" ) ;
			return false ;
		}

		//-----------------------------------

		PushSetting() ;  // 設定の退避

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


		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Development,
			m_Identifier_DedicatedServer_Linux64_Development,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// for IL2CPP

	/// <summary>
	/// Linux用(Release)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/IL2CPP/Release", priority = 10 )]
	internal static bool Execute_DedicatedServer_Linux64_IL2CPP_Release()
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
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

		PushSetting() ;  // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Release ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ;   // ひとまずビルド速度優先

		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Release,
			m_Identifier_DedicatedServer_Linux64_Release,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux用(Staging)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/IL2CPP/Staging", priority = 20 )]
	private static bool Executee_DedicatedServer_Linux64_IL2CPP_Staging()
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
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

		PushSetting() ;  // 設定の退避

		SetDefaultEndPoint( Settings.EndPointNames.Staging ) ;
		SetDevelopmentMode( false ) ;
		SetRevision( Execute_GitInfo() ) ;

		BuildOptions options = BuildOptions.None ;   // ひとまずビルド速度優先と自動実行

		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Staging,
			m_Identifier_DedicatedServer_Linux64_Staging,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		return result ;
	}

	//----------------

	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/IL2CPP/Development(NotDebug)", priority = 30 )]
	internal static bool Execute_DedicatedServer_Linux64_IL2CPP_Development_NotDebug()
	{
		return Execute_DedicatedServer_Linux64_IL2CPP_Development( true ) ;
	}

	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/IL2CPP/Development", priority = 31 )]
	internal static bool Execute_DedicatedServer_Linux64_IL2CPP_Development_BuildOnly()
	{
		return Execute_DedicatedServer_Linux64_IL2CPP_Development( false ) ;
	}

	/// <summary>
	/// Linux用(Development)
	/// </summary>
	/// <returns></returns>
	private static bool Execute_DedicatedServer_Linux64_IL2CPP_Development( bool notDebug )
	{
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server ;
		var settings = PlayerSettings.GetScriptingBackend( NamedBuildTarget.Server ) ;
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

		PushSetting() ;  // 設定の退避

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

		bool result = Execute_DedicatedServer_Linux64_General
		(
			m_ProductName_DedicatedServer_Linux64_Development,
			m_Identifier_DedicatedServer_Linux64_Development,
			options
		) ;

		PopSetting() ;   // 設定の復帰

		//-----------------------------------

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// Linux用でMonoを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Switch Mono", priority = 80 )]
	internal static bool Execute_DedicatedServer_Linux64_Switch_Mono()
	{
		// Mono を有効化する
		PlayerSettings.SetScriptingBackend( NamedBuildTarget.Server, ScriptingImplementation.Mono2x ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog("ビルドモード変更", "Windows64 or OSX or Linux64 を Mono でビルドします", "閉じる" ) ;

		return true ;
	}

	/// Linux用でIL2CPPを有効化する
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/Application/DedicatedServer/Linux64/Switch IL2CPP", priority = 81 )]
	internal static bool Execute_DedicatedServer_Linux64_Switch_IL2CPP()
	{
		// IL2CPP を有効化する
		PlayerSettings.SetScriptingBackend( NamedBuildTarget.Server, ScriptingImplementation.IL2CPP ) ;

		AssetDatabase.SaveAssets() ;

		EditorUtility.DisplayDialog( "ビルドモード変更", "Windows64 or OSX or Linux64 を IL2CPPでビルドします", "閉じる" ) ;

		return true ;
	}

	//------------------------------------------------------------

	// Linux 共通
	private static bool Execute_DedicatedServer_Linux64_General( string productName, string identifier, BuildOptions options )
	{
		BuildTarget target = BuildTarget.StandaloneLinux64 ;
		string path = m_Path_DedicatedServer_Linux64 ;

		// 処理時間計測開始
		StartClock() ;

		// ビルドターゲットを変更する
		BuildTarget activeBuildTarget = ChangeBuildTarget( target, "Change" ) ;

		//-----------------------------------------------------------

		// 固有の設定
		PlayerSettings.productName = productName ;
		PlayerSettings.SetApplicationIdentifier( NamedBuildTarget.Server, identifier ) ;

		PlayerSettings.bundleVersion = VersionName_DedicatedServer_Linux64 ;

		// ビルドプラットフォームのモジュールをインストールしていないと固有クラスはコンパイルエラーになるのでプリプロセッサで抑制すること
#if UNITY_STANDALONE_WIN
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
}
