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
	/// iOS用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/iOS/Internal", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_iOS_Internal()
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_iOS_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.iOS ;

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// iOS - Development

	/// <summary>
	/// iOS用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/iOS/Default", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_iOS()
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_iOS_Default
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_iOS_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.iOS ;

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//------------------------------------

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/iOS/Development", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_iOS_Development()
	{
		return Execute_RemoteAssets_Runtime_iOS_Development_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/iOS/Development - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_iOS_Development_Link()
	{
		return Execute_RemoteAssets_Runtime_iOS_Development_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_iOS_Development_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Development
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_iOS_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.iOS ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// iOS - Release

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/iOS/Release", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_iOS_Release()
	{
		return Execute_RemoteAssets_Runtime_iOS_Release_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/iOS/Release - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_iOS_Release_Link()
	{
		return Execute_RemoteAssets_Runtime_iOS_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_iOS_Release_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Release,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Release
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_iOS_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.iOS ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}
}
