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

	// StandaloneOSX

	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_Internal
		= "Assets/StreamingAssets/dbs/OSX/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_Default
		= "Assets/StreamingAssets/dbs/OSX/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Development
		= "AssetBundle/OSX/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Release
		= "AssetBundle_Release/OSX/Default" ;

//	private const string m_AssetBundleRootFolderPath_StreamingAssets_OSX_External
//		= "Assets/StreamingAssets/dbs/OSX/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_OSX_External
//		= "AssetBundle/OSX/External" ;

	//--------------------------------------------------------------------------------------------
	// Internal

	/// <summary>
	/// OSX用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/OSX/Internal", priority = 2)]
	internal static bool Execute_StreamingAssets_OSX_Internal()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_OSX_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneOSX ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// OSX - Development

	/// <summary>
	/// OSX用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/OSX/Default", priority = 2)]
	internal static bool Execute_StreamingAssets_OSX()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_OSX_Default
			)
			// External
//			(
///				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_OSX_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneOSX ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//------------------------------------------------------------

	[MenuItem("Build/AssetBundle/RemoteAssets/OSX/Development", priority = 2)]
	internal static bool Execute_RemoteAssets_OSX()
	{
		return Execute_RemoteAssets_OSX_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/OSX/Development - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_OSX_Link()
	{
		return Execute_RemoteAssets_OSX_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_OSX_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Development
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_OSX_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneOSX ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}

	//--------------------------------------------------------------------------------------------
	// OSX - Release

	[MenuItem("Build/AssetBundle/RemoteAssets/OSX/Release", priority = 2)]
	internal static bool Execute_RemoteAssets_Release_OSX()
	{
		return Execute_RemoteAssets_OSX_Release_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/OSX/Release - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_OSX_Release_Link()
	{
		return Execute_RemoteAssets_OSX_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_OSX_Release_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Release,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_OSX_Default_Release
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_OSX_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneOSX ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}


}
