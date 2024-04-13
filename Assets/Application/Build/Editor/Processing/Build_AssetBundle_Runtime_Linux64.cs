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
    /// Linux用
    /// </summary>
    /// <returns></returns>
    [MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/Linux64/Internal", priority = 2 )]
    internal static bool Execute_StreamingAssets_Runtime_Linux64_Internal()
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_StreamingAssetsListFilePath_Default_Internal,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_StreamingAssets_Linux64_Internal
            )
        } ;

        BuildTarget buildTarget				= BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
    }

    //--------------------------------------------------------------------------------------------
    // Linux - Deveopment

    /// <summary>
    /// Linux用
    /// </summary>
    /// <returns></returns>
    [MenuItem( "Build/AssetBundle/StreamingAssets/Runtime/Linux64/Default", priority = 2 )]
    internal static bool Execute_StreamingAssets_Runtime_Linux64()
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Development,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_StreamingAssets_Linux64_Default
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_StreamingAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget				= BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, false, false ) ;
    }

    //------------------------------------------------------------

    [MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Linux64/Development", priority = 2 )]
    internal static bool Execute_RemoteAssets_Runtime_Linux64_Development()
    {
        return Execute_RemoteAssets_Runtime_Linux64_Development_Private( false ) ;
    }

    [MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Linux64/Development - Link", priority = 2 )]
    internal static bool Execute_RemoteAssets_Runtime_Linux64_Development_Link()
    {
        return Execute_RemoteAssets_Runtime_Linux64_Development_Private( true ) ;
    }

    private static bool Execute_RemoteAssets_Runtime_Linux64_Development_Private( bool makeLink )
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Development,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_RemoteAssets_Linux64_Default_Development
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget				= BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
    }

    //--------------------------------------------------------------------------------------------
    // Linux - Release

    [MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Linux64/Release", priority = 2 )]
    internal static bool Execute_RemoteAssets_Runtime_Linux64_Release()
    {
        return Execute_RemoteAssets_Runtime_Linux64_Release_Private( false ) ;
    }

    [MenuItem( "Build/AssetBundle/RemoteAssets/Runtime/Linux64/Release - Link", priority = 2 )]
    internal static bool Execute_RemoteAssets_Runtime_Linux64_Release_Link()
    {
        return Execute_RemoteAssets_Runtime_Linux64_Release_Private( true ) ;
    }

    private static bool Execute_RemoteAssets_Runtime_Linux64_Release_Private( bool makeLink )
    {
        var targets = new ( string, string, string )[]
        {
            // Default
            (
                m_RemoteAssetsListFilePath_Default_Release,
                m_AssetsRootFolderPath_Default,
                m_AssetBundleRootFolderPath_RemoteAssets_Linux64_Default_Release
            )
            // External
//			(
//				m_RemoteAssetsListFilePath_External,
//				m_AssetsRootFolderPath_External,
//				m_AssetBundleRootFolderPath_RemoteAssets_Linux_External
//			)
        } ;

        BuildTarget buildTarget				= BuildTarget.StandaloneLinux64 ;

        return ProcessRemoteAssets( targets, buildTarget, makeLink, false ) ;
    }
}
