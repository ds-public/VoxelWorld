using System ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEditor ;

using Tools.ForAssetBundle ;	// AssetBundleBuilder のパッケージ

/// <summary>
/// アセットバンドルのバッチビルド用クラス
/// </summary>
public partial class Build_AssetBundle
{
	//------------------------------------------------------------

	// StandaloneWindows

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows_Internal
		= "Assets/StreamingAssets/dbs/Windows/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows_Default
		= "Assets/StreamingAssets/dbs/Windows/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows_Default_Development
		= "AssetBundle/Windows/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows_Default_Release
		= "AssetBundle_Release/Windows/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_Windows_External
//		= "Assets/StreamingAssets/dbs/Windows/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_Windows_External
//		= "AssetBundle/Windows/External" ;

	//--------------------------------------------------------------------------------------------
	// Internal

	/// <summary>
	/// Windows用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/Windows/Internal", priority = 2)]
	internal static bool Execute_StreamingAssets_Windows_Internal()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_Windows_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneWindows ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Windows - Deveopment

	/// <summary>
	/// Windows用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/Windows/Default", priority = 2)]
	internal static bool Execute_StreamingAssets_Windows()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_Windows_Default
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_Windows_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneWindows ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//------------------------------------------------------------

	[MenuItem("Build/AssetBundle/RemoteAssets/Windows/Development", priority = 2)]
	internal static bool Execute_RemoteAssets_Windows()
	{
		return Execute_RemoteAssets_Windows_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/Windows/Development - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_Windows_Link()
	{
		return Execute_RemoteAssets_Windows_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Windows_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_Windows_Default_Development
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Windows_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneWindows ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Windows - Release

	[MenuItem("Build/AssetBundle/RemoteAssets/Windows/Release", priority = 2)]
	internal static bool Execute_RemoteAssets_Windows_Release()
	{
		return Execute_RemoteAssets_Windows_Release_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/Windows/Release - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_Windows_Release_Link()
	{
		return Execute_RemoteAssets_Windows_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Windows_Release_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Release,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_Windows_Default_Release
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Windows_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneWindows ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}
}
