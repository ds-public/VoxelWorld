using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;
using UnityEditor ;

using Tools.ForAssetBundle ;	// AssetBundleBuilder のパッケージ

/// <summary>
/// アセットバンドルのバッチビルド用クラス(設定) Version 2024/04/13
/// </summary>
public partial class Build_AssetBundle
{
	//--------------------------------------------------------------------------------------------
	// 共通設定

	// Common Internal

	private const string m_StreamingAssetsListFilePath_Common_Internal		= "Assets/Application/AssetBundle/list_local_common.txt" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Common_Internal
		= "Assets/StreamingAssets/dbs/Common/Internal" ;


	//-----------------------------------------------------------------------------------------------------------------
	// プラットフォーム共通設定

	//----------------
	// Default

	// Assets RootFolderPath Default
	private const string m_AssetsRootFolderPath_Default						= "Assets/Application/AssetBundle" ;

	//----------------
	// Internal

	// RemoteAssets ListFilePath Default
	private const string m_StreamingAssetsListFilePath_Default_Internal		= "Assets/Application/AssetBundle/list_local_internal.txt" ;

	//----------------
	// Development

	// RemoteAssets ListFilePath Default
	private const string m_RemoteAssetsListFilePath_Default_Development		= "Assets/Application/AssetBundle/list_remote.txt" ;

	//----------------
	// Release

	// RemoteAssets ListFilePath Default
	private const string m_RemoteAssetsListFilePath_Default_Release			= "Assets/Application/AssetBundle/list_remote_release.txt" ;

	//----------------

	// Assets RootFolderPath External
//	private const string m_AssetsRootFolderPath_External					= "Assets/Application/AssetBundle_Application" ;

	//------------------------------------------------------------
	// Remote Only

	// link.xml
	private const string m_LinkFilePath			= "Assets/link.xml" ;

	//-----------------------------------------------------------------------------------------------------------------
	// プラットフォーム個別設定

	//------------------------------------------------------------
	// Standalone(Windows64) 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows64_Internal
		= "Assets/StreamingAssets/dbs/Runtime/Windows64/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows64_Default
		= "Assets/StreamingAssets/dbs/Runtime/Windows64/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows64_Default_Development
		= "AssetBundle/Runtime/Windows64/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows64_Default_Release
		= "AssetBundle_Release/Runtime/Windows64/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows64_External
//		= "Assets/StreamingAssets/dbs/Runtime/Windows64/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows64_External
//		= "AssetBundle/Runtime/Windows64/External" ;


	//------------------------------------------------------------
	// Standalone(OSX) 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_Internal
		= "Assets/StreamingAssets/dbs/Runtime/OSX/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_Default
		= "Assets/StreamingAssets/dbs/Runtime/OSX/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Development
		= "AssetBundle/Runtime/OSX/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Release
		= "AssetBundle_Release/Runtime/OSX/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_External
//		= "Assets/StreamingAssets/dbs/Runtime/OSX/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_External
//		= "AssetBundle/Runtime/OSX/External" ;


	//------------------------------------------------------------
	// Android 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_Internal
		= "Assets/StreamingAssets/dbs/Runtime/Android/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_Default
		= "Assets/StreamingAssets/dbs/Runtime/Android/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Development
		= "AssetBundle/Runtime/Android/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Release
		= "AssetBundle_Release/Runtime/Android/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_External
//		= "Assets/StreamingAssets/dbs/Runtime/Android/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_External
//		= "AssetBundle/Runtime/Android/External" ;


	//------------------------------------------------------------
	// iOS 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_Internal
		= "Assets/StreamingAssets/dbs/Runtime/iOS/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_Default
		= "Assets/StreamingAssets/dbs/Runtime/iOS/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Development
		= "AssetBundle/Runtime/iOS/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Release
		= "AssetBundle_Release/Runtime/iOS/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_External
//		= "Assets/StreamingAssets/dbs/Runtime/iOS/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_External
//		= "AssetBundle/Runtime/iOS/External" ;


	//------------------------------------------------------------
	// Standalone(Linux) 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Linux64_Internal
		= "Assets/StreamingAssets/dbs/Runtime/Linux64/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Linux64_Default
		= "Assets/StreamingAssets/dbs/Runtime/Linux64/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Linux64_Default_Development
		= "AssetBundle/Runtime/Linux64/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Linux64_Default_Release
		= "AssetBundle_Release/Runtime/Linux64/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_Linux64_External
//		= "Assets/StreamingAssets/dbs/Runtime/Linux64/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_Linux64_External
//		= "AssetBundle/Runtime/Linux64/External" ;


	//------------------------------------------------------------
	// DedicatedServer(Windows64) 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Windows64_Internal
		= "Assets/StreamingAssets/dbs/DedicatedServer/Windows64/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Windows64_Default
		= "Assets/StreamingAssets/dbs/DedicatedServer/Windows64/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Windows64_Default_Development
		= "AssetBundle/DedicatedServer/Windows64/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Windows64_Default_Release
		= "AssetBundle_Release/DedicatedServer/Windows64/Default" ;


	//------------------------------------------------------------
	// DedicatedServer(Linux64) 用

	//-------------
	// StreamingAssets 用の出力先

	private const string m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Linux64_Internal
		= "Assets/StreamingAssets/dbs/DedicatedServer/Linux64/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Linux64_Default
		= "Assets/StreamingAssets/dbs/DedicatedServer/Linux64/Default" ;

	//-------------
	// Local & Remote 用の出力先

	private const string m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Linux64_Default_Development
		= "AssetBundle/DedicatedServer/Linux64/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Linux64_Default_Release
		= "AssetBundle_Release/DedicatedServer/Linux64/Default" ;
}
