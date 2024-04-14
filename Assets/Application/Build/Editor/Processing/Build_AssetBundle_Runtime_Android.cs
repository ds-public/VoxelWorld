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
	/// Android用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/Android/Internal", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_Android_Internal()
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_Android_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.Android ;

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Android - Development

	/// <summary>
	/// Android用
	/// </summary>
	/// <returns></returns>
	[MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/Android/Default", priority = 2 )]
	internal static bool Execute_StreamingAssets_Runtime_Android()
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_Android_Default
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_Android_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.Android ;

		return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
	}

	//------------------------------------

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Android/Default/Development", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_Android_Development()
	{
		return Execute_RemoteAssets_Runtime_Android_Development_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Android/Default/Development - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_Android_Development_Link()
	{
		return Execute_RemoteAssets_Runtime_Android_Development_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_Android_Development_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Development,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Development
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Android_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.Android ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Android - Release

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Android/Default/Release", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_Android_Release()
	{
		return Execute_RemoteAssets_Runtime_Android_Release_Private( false ) ;
	}

	[MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Android/Default/Release - Link", priority = 2 )]
	internal static bool Execute_RemoteAssets_Runtime_Android_Release_Link()
	{
		return Execute_RemoteAssets_Runtime_Android_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Runtime_Android_Release_Private( bool makeLink )
	{
		var targets = new ( string, string, string )[]
		{
			// Default
			(
				m_RemoteAssetsListFilePath_Default_Release,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Release
			)
			// External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Android_External
//			)
		} ;

		BuildTarget buildTarget				= BuildTarget.Android ;

		return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
	}
}
