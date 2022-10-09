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

	// Android

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_Internal
		= "Assets/StreamingAssets/dbs/Android/Internal" ;

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_Default
		= "Assets/StreamingAssets/dbs/Android/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Development
		= "AssetBundle/Android/Default" ;

	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_Default_Release
		= "AssetBundle_Release/Android/Default" ;


//	private const string m_AssetBundleRootFolderPath_StreamingAssets_Android_External
//		= "Assets/StreamingAssets/dbs/Android/External" ;

//	private const string m_AssetBundleRootFolderPath_RemoteAssets_Android_External
//		= "AssetBundle/Android/External" ;


	//--------------------------------------------------------------------------------------------
	// Internal

	/// <summary>
	/// Android用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/Android/Internal", priority = 2)]
	internal static bool Execute_StreamingAssets_Android_Internal()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
		{
			// Default
			(
				m_StreamingAssetsListFilePath_Default_Internal,
				m_AssetsRootFolderPath_Default,
				m_AssetBundleRootFolderPath_StreamingAssets_Android_Internal
			)
		} ;

		BuildTarget buildTarget				= BuildTarget.Android ;

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Android - Development

	/// <summary>
	/// Android用
	/// </summary>
	/// <returns></returns>
	[MenuItem("Build/AssetBundle/StreamingAssets/Android/Default", priority = 2)]
	internal static bool Execute_StreamingAssets_Android()
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, false ) ;
	}

	//------------------------------------

	[MenuItem("Build/AssetBundle/RemoteAssets/Android/Default/Development", priority = 2)]
	internal static bool Execute_RemoteAssets_Android()
	{
		return Execute_RemoteAssets_Android_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/Android/Default/Development - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_Android_Link()
	{
		return Execute_RemoteAssets_Android_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Android_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}

	//--------------------------------------------------------------------------------------------
	// Android - Release

	[MenuItem("Build/AssetBundle/RemoteAssets/Android/Default/Release", priority = 2)]
	internal static bool Execute_RemoteAssets_Android_Release()
	{
		return Execute_RemoteAssets_Android_Release_Private( false ) ;
	}

	[MenuItem("Build/AssetBundle/RemoteAssets/Android/Default/Release - Link", priority = 2)]
	internal static bool Execute_RemoteAssets_Android_Release_Link()
	{
		return Execute_RemoteAssets_Android_Release_Private( true ) ;
	}

	private static bool Execute_RemoteAssets_Android_Release_Private( bool makeLink )
	{
		( string, string, string )[] targets = new ( string, string, string )[]
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

		return ProcessRemoteAssets( targets, buildTarget, makeLink ) ;
	}
}
