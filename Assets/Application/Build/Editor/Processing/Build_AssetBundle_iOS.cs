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
	//------------------------------------

	// iOS

	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_Internal
		= "Assets/StreamingAssets/dbs/iOS/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_Default
		= "Assets/StreamingAssets/dbs/iOS/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Development
		= "AssetBundle/iOS/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_Default_Release
		= "AssetBundle_Release/iOS/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_iOS_External
//		= "Assets/StreamingAssets/dbs/iOS/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_iOS_External
//		= "AssetBundle/iOS/External" ;

	//--------------------------------------------------------------------------------------------
	// Internal

	/// <summary>
	/// iOS用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/iOS/Internal", priority = 2)]
	internal static bool Execute_StreamingAssets_iOS_Internal()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_iOS_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.iOS ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// iOS - Development

	/// <summary>
	/// iOS用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/iOS/Default", priority = 2)]
	internal static bool Execute_StreamingAssets_iOS()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//------------------------------------

	[MenuItem("Build/AssetBundle/RemoteAssets/iOS/Development", priority = 2)]
	internal static bool Execute_RemoteAssets_iOS()
	{
		return Execute_RemoteAssets_iOS_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/iOS/Development - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_iOS_Link()
	{
		return Execute_RemoteAssets_iOS_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_iOS_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}

	//--------------------------------------------------------------------------------------------
	// iOS - Release

	[MenuItem("Build/AssetBundle/RemoteAssets/iOS/Release", priority = 2)]
	internal static bool Execute_RemoteAssets_iOS_Release()
	{
		return Execute_RemoteAssets_iOS_Release_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/iOS/Release - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_iOS_Release_Link()
	{
		return Execute_RemoteAssets_iOS_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_iOS_Release_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}
}
