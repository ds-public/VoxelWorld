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
	//--------------------------------------------------------------------------------------------
	// Internal

	/// <summary>
	/// OSX用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/OSX/Internal", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_OSX_Internal()
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_OSX_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.StandaloneOSX ;

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// OSX - Development

	/// <summary>
	/// OSX用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/OSX/Default", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_OSX()
	{
		var targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//------------------------------------------------------------

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/OSX/Development", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_OSX_Development()
	{
		return Execute_RemoteAssets_Runtime_OSX_Development_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/OSX/Development - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_OSX_Development_Link()
	{
		return Execute_RemoteAssets_Runtime_OSX_Development_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_OSX_Development_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// OSX - Release

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/OSX/Release", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_OSX_Release()
	{
		return Execute_RemoteAssets_Runtime_OSX_Release_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/OSX/Release - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_OSX_Release_Link()
	{
		return Execute_RemoteAssets_Runtime_OSX_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_OSX_Release_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}
}
