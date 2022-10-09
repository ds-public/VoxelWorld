using System ;
using System.Collections.Generic ;
using System.IO ;

using UnityEngine ;
using UnityEditor ;

using Tools.ForAssetBundle ;	// AssetBundleBuilder のパッケージ

/// <summary>
/// アセットバンドルのバッチビルド用クラス Version 2022/09/23
/// </summary>
public partial class Build_AssetBundle
{
	//--------------------------------------------------------------------------------------------
	// 共通設定

	// Common Internal
	private const string m_StreamingAssetsListFilePath_Common_Internal		= "Assets/Application/AssetBundle/list_local_common.txt" ;


	//------------------------------------
	// Source


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
	// Common

	private const string m_AssetBundleRootFolderPath_StreamingAssets_Common_Internal
		= "Assets/StreamingAssets/dbs/Common/Internal" ;

	//------------------------------------------------------------
	// Remote Only

	// link.xml
	private const string m_LinkFilePath			= "Assets/link.xml" ;


	//--------------------------------------------------------------------------------------------

	[MenuItem("Build/AssetBundle/StreamingAssets/Common", priority = 2)]
	internal static bool Execute_StreamingAssets_Common()
	{
		string listFilePath					= m_StreamingAssetsListFilePath_Common_Internal ;
		string assetsRootPath				= m_AssetsRootFolderPath_Default ;
		string assetBundleRootFolderPath	= m_AssetBundleRootFolderPath_StreamingAssets_Common_Internal ;
		BuildTarget buildTarget				= BuildTarget.NoTarget ;

		return ProcessStreamingAssets( listFilePath, assetsRootPath, assetBundleRootFolderPath, buildTarget ) ;
	}

	//------------------------------------------------------------

	// AssetBundleのビルド実行
	private static bool ProcessStreamingAssets( string listFilePath, string assetsRootPath, string assetBundleRootFolderPath, BuildTarget buildTarget )
	{
		// 処理時間計測開始
		StartClock() ;

		// 注意：AssetBundleのビルドでは目的のプラットフォームに切り替える必要は無い(以前は必要だったが現在は必要無くなった)
		bool result = SimpleAssetBundleBuilder.Build( listFilePath, assetsRootPath, assetBundleRootFolderPath, buildTarget, generateLinkFile:false ) ;

		// 処理時間計測終了
		StopClock() ;

#if UNITY_EDITOR
		// ビルド結果のダイアログを表示する
		string message = result ? "成功しました" : "失敗しました" ;
		EditorUtility.DisplayDialog( "Build Asset Bundle", message, "閉じる" ) ;
#endif
		Debug.Log( "-------> Target : " + buildTarget ) ;

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	// AssetBundleのビルド実行
	private static bool ProcessRemoteAssets( ( string, string, string )[] targets, BuildTarget buildTarget, bool makeLink )
	{
		// 処理時間計測開始
		StartClock() ;

		bool result = true ;

		string listFilePath ;
		string assetRootPath ;
		string assetBundleRootPath ;

		List<( string, string)> linkTargets = new List<( string, string )>() ;

		int i, l = targets.Length ;
		for( i  = 0 ; i <  l ; i ++ )
		{
			if( result == true )
			{
				listFilePath		= targets[ i ].Item1 ;
				assetRootPath		= targets[ i ].Item2 ;
				assetBundleRootPath	= targets[ i ].Item3 ;

				linkTargets.Add( ( listFilePath, assetRootPath ) ) ;

				// 注意：AssetBundleのビルドでは目的のプラットフォームに切り替える必要は無い(以前は必要だったが現在は必要無くなった)
				if
				(
					SimpleAssetBundleBuilder.Build
					(
						listFilePath,
						assetRootPath,
						assetBundleRootPath,
						buildTarget,
						generateLinkFile: false,
						strictMode: false
					)
					== false
				)
				{
					result = false ;
				}
			}
		}

		if( result == true && makeLink == true )
		{
			// link.xml を出力する

			// 注意：AssetBundleのビルドでは目的のプラットフォームに切り替える必要は無い(以前は必要だったが現在は必要無くなった)
			result = SimpleAssetBundleBuilder.MakeLinkXmlFile( linkTargets.ToArray(), m_LinkFilePath ) ;
		}

		// 処理時間計測終了
		StopClock() ;

		Debug.Log( "<color=#FFFFFF>=======================================</color>" ) ;
#if UNITY_EDITOR
		// ビルド結果のダイアログを表示する
		string message = result ? "成功しました" : "失敗しました" ;
		EditorUtility.DisplayDialog( "Build Asset Bundle", message, "閉じる" ) ;
#endif
		Debug.Log( "<color=#7FFF7F>-------> Target : " + buildTarget + "</color>" ) ;

		if( Application.isBatchMode && !result )
		{
			// バッチモードで失敗した場合、エラーで終了させる
			EditorApplication.Exit( 1 );
		}

		return result ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// ビルドタイプ
	/// </summary>
	public enum BuildTypes
	{
		StreamingAssets	= 0,
		RemoteAssets	= 1,
	}

	// 外部からはこれを叩いてアプリをビルドする。パラメータはコマンドライン引数から渡す。
	public static void Execute()
	{
		BuildTypes		build			= BuildTypes.RemoteAssets ;
		BuildTarget		platform		= BuildTarget.StandaloneWindows ;

		//-----------------------------------

		string[] args = Environment.GetCommandLineArgs() ;

		for( int i  = 0 ; i <  args.Length ; i ++ )
		{
			Debug.Log( args[ i ] ) ;
			switch( args[ i ] )
			{
				//---------------------------------

				// Windows用
				case "--platform-windows64" :
				case "--platform-windows" :
					platform	= BuildTarget.StandaloneWindows ;
				break ;

				// OSX用
				case "--platform-osx" :
				case "--platform-machintosh" :
					platform	= BuildTarget.StandaloneOSX ;
				break ;

				// Android用
				case "--platform-android" :
					platform	= BuildTarget.Android ;
				break ;

				// iOS用
				case "--platform-ios" :
					platform	= BuildTarget.iOS ;
				break ;

				//---------------------------------

				// StreamingAssets版
				case "--build-streaming" :
					build = BuildTypes.StreamingAssets ;
				break ;

				// RemoteAssets版
				case "--build-remote" :
					build = BuildTypes.RemoteAssets ;
				break ;

				//---------------------------------
			}
		}

		//-----------------------------------------------------------

		Debug.Log( "=================================================" ) ;
		Debug.Log( "------- Build    : " + build.ToString() ) ;
		Debug.Log( "------- Platform : " + platform.ToString() ) ;
		Debug.Log( "=================================================" ) ;

		// プラットフォームごとのビルドを実行する
		bool result = false ;

		switch( build )
		{
			case BuildTypes.StreamingAssets	:
				switch( platform )
				{
					case BuildTarget.StandaloneWindows	: result = Execute_StreamingAssets_Windows()	; break ;
					case BuildTarget.StandaloneOSX		: result = Execute_StreamingAssets_OSX()		; break ;
					case BuildTarget.Android			: result = Execute_StreamingAssets_Android()	; break ;
					case BuildTarget.iOS				: result = Execute_StreamingAssets_iOS()		; break ;
				}
			break ;

			case BuildTypes.RemoteAssets	:
				switch( platform )
				{
					case BuildTarget.StandaloneWindows	: result = Execute_RemoteAssets_Windows()		; break ;
					case BuildTarget.StandaloneOSX		: result = Execute_RemoteAssets_OSX()			; break ;
					case BuildTarget.Android			: result = Execute_RemoteAssets_Android()		; break ;
					case BuildTarget.iOS				: result = Execute_RemoteAssets_iOS()			; break ;
				}
			break ;
		}

		//-----------------------------------------------------------

		if( result == true )
		{
			// ビルド成功
			EditorApplication.Exit( 0 ) ;
		}
		else
		{
			// ビルド失敗
			EditorApplication.Exit( 1 ) ;
		}
	}

	//--------------------------------------------------------------------------------------------
	// link.xml(コマンドラインからも可能)

	[MenuItem("Build/AssetBundle/RemoteAssets/Make link.xml", priority = 2)]
	internal static bool MakeLinkXmlFile()
	{
		// 処理時間計測開始
		StartClock() ;

		( string, string )[] targets =
		{
			( m_RemoteAssetsListFilePath_Default_Development,	m_AssetsRootFolderPath_Default ),
		} ;

		// 注意：AssetBundleのビルドでは目的のプラットフォームに切り替える必要は無い(以前は必要だったが現在は必要無くなった)
		bool result = SimpleAssetBundleBuilder.MakeLinkXmlFile( targets, m_LinkFilePath ) ;

		// 処理時間計測終了
		StopClock() ;

#if UNITY_EDITOR
		// ビルド結果のダイアログを表示する
		string message = result ? "成功しました" : "失敗しました" ;
		EditorUtility.DisplayDialog( "Make Link File", message, "閉じる" ) ;
#endif
		return result ;
	}

	//--------------------------------------------------------------------------------------------
	// チュートリアル用のアセットバンドルリストファイルをコピーする

	[MenuItem("Build/AssetBundle/Copy AssetBundlePaths For Tutorial", priority = 2)]
	internal static bool CopyAssetBundlePathsForTutorial()
	{
		bool result = false ;

		// 処理時間計測開始
		StartClock() ;

		string sPath = "AssetBundlePaths_Tutorial.txt" ;
		string dPath = "Assets/Application/Resources/AssetBundlePaths_Tutorial.txt" ;

		if( File.Exists( sPath ) == false )
		{
			Debug.LogWarning( "File not found : " + sPath ) ;
		}
		else
		{
			string text = File.ReadAllText( sPath ) ;
			if( string.IsNullOrEmpty( text ) == true )
			{
				Debug.LogWarning( "File is empty : " + sPath ) ;
			}
			else
			{
				// ファイルのコピーを実行する
				File.WriteAllText( dPath, text ) ;

				// 再インポート
				AssetDatabase.ImportAsset
				(
					path: dPath,
					options: ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport
				) ;

				result = true ;
			}
		}

		// 処理時間計測終了
		StopClock() ;

#if UNITY_EDITOR
		// ビルド結果のダイアログを表示する
		string message = result ? "成功しました" : "失敗しました" ;
		EditorUtility.DisplayDialog( "Copy AssetBundlePaths For Tutorial", message, "閉じる" ) ;
#endif
		return result ;
	}



	//--------------------------------------------------------------------------------------------

	// 1970年01月01日 00時00分00秒 からの経過秒数を計算するためのエポック時間
	private static readonly DateTime m_UNIX_EPOCH = new DateTime( 1970, 1, 1, 0, 0, 0, 0 ) ;

	private static long m_ProcessingTime ;

	// 時間計測を開始する
	private static void StartClock()
	{
		DateTime dt = DateTime.Now ;
		TimeSpan time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		m_ProcessingTime = ( long )time.TotalSeconds ;
	}

	// 時間計測を終了する
	private static void StopClock()
	{
		DateTime dt = DateTime.Now ;
		TimeSpan time = dt.ToUniversalTime() - m_UNIX_EPOCH ;
		long processtingTime = ( long )time.TotalSeconds - m_ProcessingTime ;

		long hour   = ( processtingTime / 3600L ) ;
		processtingTime %= 3600 ;
		long minute = ( processtingTime /   60L ) ;
		processtingTime %=   60 ;
		long second =   processtingTime ;

		Debug.Log( "<color=#FFFFFF>=======================================</color>" ) ;
		Debug.Log( "<color=#00FFFF>Processing Time -> " + hour.ToString( "D2" ) + ":" + minute.ToString( "D2" ) + ":" + second.ToString( "D2" ) + "</color>" ) ;
	}
}
