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
    /// <summary>
    /// DedicatedServer(Linux)用
    /// </summary>
    /// <returns></returns>
    [MenuItem( "Build/AssetBundle/StreamingAssets/DedicatedServer/Linux64/Internal", priority = 2 )]
    internal static bool Execute_StreamingAssets_DedicatedServer_Linux64_Internal()
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_StreamingAssetsListFilePath_Default_Internal,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Linux64_Internal
            )
        } ;

        BuildTarget buildTarget = BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, false, true ) ;
    }

    //--------------------------------------------------------------------------------------------
    // DedicatedServer(Linux) - Deveopment

    /// <summary>
    /// DedicatedServer(Linux)用
    /// </summary>
    /// <returns></returns>
    [MenuItem( "Build/AssetBundle/StreamingAssets/DedicatedServer/Linux64/Default", priority = 2 )]
    internal static bool Execute_StreamingAssets_DedicatedServer_Linux64()
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Development,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_StreamingAssets_DedicatedServer_Linux64_Default
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget = BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, false, true ) ;
    }

    //------------------------------------------------------------

    [MenuItem( "Build/AssetBundle/RemoteAssets/DedicatedServer/Linux64/Development", priority = 2 )]
    internal static bool Execute_RemoteAssets_DedicatedServer_Linux64()
    {
        return Execute_RemoteAssets_DedicatedServer_Linux64_Private( false ) ;
    }

    [MenuItem( "Build/AssetBundle/RemoteAssets/DedicatedServer/Linux64/Development - Link", priority = 2 )]
    internal static bool Execute_RemoteAssets_DedicatedServer_Linux64_Link()
    {
        return Execute_RemoteAssets_DedicatedServer_Linux64_Private( true ) ;
    }

    private static bool Execute_RemoteAssets_DedicatedServer_Linux64_Private( bool makeLink )
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Development,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Linux64_Default_Development
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget = BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, makeLink, true ) ;
    }

    //--------------------------------------------------------------------------------------------
    // DedicatedServer(Linux) - Release

    [MenuItem( "Build/AssetBundle/RemoteAssets/DedicatedServer/Linux64/Release", priority = 2 )]
    internal static bool Execute_RemoteAssets_DedicatedServer_Linux64_Release()
    {
        return Execute_RemoteAssets_DedicatedServer_Linux64_Release_Private( false ) ;
    }

    [MenuItem( "Build/AssetBundle/RemoteAssets/DedicatedServer/Linux64/Release - Link", priority = 2 )]
    internal static bool Execute_RemoteAssets_DedicatedServer_Linux64_Release_Link()
    {
        return Execute_RemoteAssets_DedicatedServer_Linux64_Release_Private( true ) ;
    }

    private static bool Execute_RemoteAssets_DedicatedServer_Linux64_Release_Private( bool makeLink )
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Release,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_RemoteAssets_DedicatedServer_Linux64_Default_Release
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget = BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, makeLink, true ) ;
    }
}
