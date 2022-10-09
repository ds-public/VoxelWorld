using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;


using uGUIHelper ;
using AssetBundleHelper ;

namespace DBS.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新)
	/// </summary>
	public partial class Downloading
	{
		/// <summary>
		/// 更新が必要なアセットバンドルのパスを取得する
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string,AssetBundleManager.DownloadEntity> GetTargetAssetBundlePaths( bool ignorePlayerData )
		{
			// 事前ダウンロードを行うアセットバンドルパスを追加する
			Dictionary<string,AssetBundleManager.DownloadEntity> targetAssetBundlePaths = new Dictionary<string, AssetBundleManager.DownloadEntity>() ;

			//------------------------------------------------------------------------------------------

			bool tutorialCompleted = true ;
//			if( ignorePlayerData == false )
//			{
//				tutorialCompleted = Tutorial.IsCompleted ;
//			}

			string text = null ;
			string filePath = "AssetBundlePaths_Tutorial" ;
			var ta = Resources.Load<TextAsset>( filePath ) ;
			if( ta != null )
			{
				text = ta.text ;
			}

			//----------------------------------

			if( tutorialCompleted == false && string.IsNullOrEmpty( text ) == false )
			{
				// チュートリアル限定のアセットバンドルファイルのみを対象とする
				string[] paths = text.Split( '\n' ) ;
				if( paths != null && paths.Length >  3 )
				{
					string fullPath ;
					string path ;

					// 最初の３行はダウンロードする必要の無い情報

					int i, l = paths.Length, p ;
					for( i  = 3 ; i <  l ; i ++ )
					{
						path = paths[ i ] ;

						p = path.IndexOf( "/" ) ;	// 最初のスラッシュまでがマニフェスト名
						if( p >= 0 )
						{
							fullPath = path.Substring( 0, p ) + "|" + path.Substring( p + 1, path.Length - ( p + 1 ) ) ;

							if(	AssetBundleManager.Contains( fullPath ) == true && AssetBundleManager.Exists( fullPath ) == false )
							{
								// 管理対象に含まれ且つ更新が必要なもののみ追加する
								targetAssetBundlePaths.Add
								(
									fullPath,
									new AssetBundleManager.DownloadEntity()
									{
										Path = fullPath,
										Keep = true			// 基本的に全てのアセットバンドルを保持するので true = キャッシュ溢れで削除はしない で良い
									}
								) ;
							}
						}
					}
				}
			}
			else
			{
				// 全てのアセットバンドルファイルを対象とする

				// 対象はデフォルトマニフェスト(Default) ※ひとまずこれのみで良い
				string manifestName = "Default" ;
				string[] paths        =  AssetBundleManager.GetAllAssetBundlePaths( manifestName, true ) ;
				if( paths != null && paths.Length >  0 )
				{
					string fullPath ;
					foreach( var path in paths )
					{
						fullPath = manifestName + "|" + path ;
						targetAssetBundlePaths.Add
						(
							fullPath,
							new AssetBundleManager.DownloadEntity()
							{
								Path = fullPath,
								Keep = true			// 基本的に全てのアセットバンドルを保持するので true = キャッシュ溢れで削除はしない で良い
							}
						) ;
					}
				}
			}

			//----------------------------------------------------------

			return targetAssetBundlePaths ;
		}
		//-------------------------------------------------------------------------------------------
	}
}
