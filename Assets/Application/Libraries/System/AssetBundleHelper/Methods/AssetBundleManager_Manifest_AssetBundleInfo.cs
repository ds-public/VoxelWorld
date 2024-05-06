using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;


/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		public partial class ManifestInfo
		{
			// AssetBundleInfo

			/// <summary>
			/// どちらの格納場所が優先されるか
			/// </summary>
			public enum LocationPriorities
			{
				Storage,
				StreamingAssets,
			}

			/// <summary>
			/// アセットバンドル情報クラス
			/// </summary>
			[Serializable]
			public class AssetBundleInfo
			{
				/// <summary>
				/// マニフェスト内での相対パス
				/// </summary>
				public string	Path = String.Empty ;

				/// <summary>
				/// アセットバンドルファイルのサイズ
				/// </summary>
				public long		Size = 0 ;			// サイズ(処理の高速化のためにここに保持しておく)※キャッシュオーバーなどの際の処理に使用する

				/// <summary>
				/// ハッシュ値
				/// </summary>
				public string	Hash = String.Empty ;

				/// <summary>
				/// ＣＲＣ値(０で使用しない)
				/// </summary>
				public uint		Crc = 0 ;

				/// <summary>
				/// タグ
				/// </summary>
				public string[]	Tags = null ;

				//---------------------------------

				/// <summary>
				/// ファイルが正しい完全なものとして検証できているかどうか
				/// </summary>
				public bool		IsCompleted = false ;

				/// <summary>
				/// 最終更新日時
				/// </summary>
				public long		LastUpdateTime = 0 ;

				//---------------------------------

				/// <summary>
				/// 通常のメモリアセットバンドルキャッシュでは破棄されないようにするかどうか
				/// </summary>
				public bool		IsRetain = false ;

				/// <summary>
				/// キャッシュオーバーする際に破棄可能にするかどうかを示す
				/// </summary>
				public bool		IsKeep = false ;

				/// <summary>
				/// 更新が必要がどうかを示す
				/// </summary>
				public bool		UpdateRequired = true ;		// 更新が必要かどうか

				/// <summary>
				/// Storage と StreamingAssets でどちらを優先するか
				/// </summary>
				public LocationPriorities	LocationPriority = LocationPriorities.Storage ;

				/// <summary>
				/// ダウンロード中かどうか
				/// </summary>
				public bool		IsDownloading = false ;

				/// <summary>
				/// 非同期アクセス時の排他ロックフラグ
				/// </summary>
				public bool		Busy = false ;

				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="path">マニフェスト内での相対パス</param>
				/// <param name="hash">ハッシュ値</param>
				/// <param name="time">最終更新日時</param>
				public AssetBundleInfo( string path, long size, string hash, uint crc, string[] tags, long lastUpdateTime )
				{
					Path			= path ;
					Size			= size ;
					Hash			= hash ;
					Crc				= crc ;
					Tags			= tags ;
					LastUpdateTime	= lastUpdateTime ;
				}

				/// <summary>
				/// 指定したタグに該当するかどうか
				/// </summary>
				/// <param name="tag"></param>
				/// <returns></returns>
				public bool ContainsTag( string tag )
				{
					if( Tags == null || Tags.Length == 0 )
					{
						return false ;
					}

					return Tags.Contains( tag ) ;
				}

				//---------------------------------------------------------

				// AssetBundleInfo :: GetAllAssetPaths

				/// <summary>
				/// アセットバンドルに含まれる全アセットのパスを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="assetBundlePath"></param>
				/// <param name="assetName"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <returns></returns>
				internal protected ( string, Type )[] GetAllAssetPaths( AssetBundle assetBundle, string assetBundlePath, string localAssetsRootPath, AssetBundleManager instance )
				{
					string localAssetBundlePath = $"{localAssetsRootPath}{assetBundlePath}/" ;
					localAssetBundlePath = localAssetBundlePath.ToLower() ;

					var paths = assetBundle.GetAllAssetNames() ;
					if( paths == null || paths.Length == 0 )
					{
						return null ;
					}

					var allAssetPaths = new List<( string, Type )>() ;

					// アセットバンドル部のパスと拡張子を削除する
					int i, l = paths.Length ;
					string path, extension ;
					int i0, i1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						path = paths[ i ] ;
						path = path.Replace( localAssetBundlePath, "" ) ;

						i0 = path.LastIndexOf( '/' ) ;
						i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							allAssetPaths.Add( ( path, typeof( System.Object ) ) ) ;
						}
						else
						{
							// 拡張子あり
							extension = path[ i1.. ] ;
							path = path[ ..i1 ] ;

							allAssetPaths.Add( ( path, instance.GetTypeFromExtension( extension ) ) ) ;
						}
					}

					return allAssetPaths.ToArray() ;
				}

				/// <summary>
				/// アセットバンドルに含まれる全アセットのパスを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="assetBundlePath"></param>
				/// <param name="assetName"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <returns></returns>
				internal protected string[] GetAllAssetPaths( AssetBundle assetBundle, string assetBundlePath, Type type, string localAssetsRootPath, AssetBundleManager instance )
				{
					string localAssetBundlePath = $"{localAssetsRootPath}{assetBundlePath}/" ;
					localAssetBundlePath = localAssetBundlePath.ToLower() ;

					var paths = assetBundle.GetAllAssetNames() ;
					if( paths == null || paths.Length == 0 )
					{
						return null ;
					}

					var allAssetPaths = new List<string>() ;

					// アセットバンドル部のパスと拡張子を削除する
					int i, l = paths.Length ;
					string path, extension ;
					int i0, i1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						path = paths[ i ] ;
						path = path.Replace( localAssetBundlePath, "" ) ;

						i0 = path.LastIndexOf( '/' ) ;
						i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
//							allAssetPaths.Add( path ) ;
						}
						else
						{
							// 拡張子あり
							extension = path[ i1.. ] ;
							if( instance.GetTypeFromExtension( extension ) == type )
							{
								path = path[ ..i1 ] ;
								allAssetPaths.Add( path ) ;
							}
						}
					}

					return allAssetPaths.ToArray() ;
				}

				//---------------------------------------------------------

				// AssetBundleInfo :: Asset
				
				/// <summary>
				/// アセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="assetBundle">アセットバンドルのインスタンス</param>
				/// <param name="assetBundleName">アセットバンドル名</param>
				/// <param name="assetName">アセット名</param>
				/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadAsset
				(
					AssetBundle assetBundle,
					string assetBundlePath, string assetName, Type type, bool isSingle,
					string localAssetsRootPath,
					AssetBundleManager instance
				)
				{
					string path ;

					if( isSingle == false )
					{
						// 複数のアセットファイルが存在するタイプ

						// ひとまず assetName は空想定でやってみる
						if( string.IsNullOrEmpty( assetName ) == true )
						{
							// アセットバンドル＝単一アセットのケース
							path = ( $"{localAssetsRootPath}{assetBundlePath}" ) ;
						}
						else
						{
							// アセットバンドル＝複合アセットのケース
							path = ( $"{localAssetsRootPath}{assetBundlePath}/{assetName}" ) ;
						}
					}
					else
					{
						// 単体のアセットファイルが存在するタイプ
						int i = assetBundlePath.LastIndexOf( '/' ) ;
						if( i >= 0 )
						{
							// フォルダがある

							assetBundlePath = assetBundlePath[ ..i ] ;

							if( string.IsNullOrEmpty( assetName ) == true )
							{
								// アセットファイル名が空(基本的にありえない)
								path = ( $"{localAssetsRootPath}{assetBundlePath}" ) ;
							}
							else
							{
								path = ( $"{localAssetsRootPath}{assetBundlePath}/{assetName}" ) ;
							}
						}
						else
						{
							// フォルダがない

							if( string.IsNullOrEmpty( assetName ) == true )
							{
								// アセットファイル名が空(基本的にありえない)
								return null ;
							}
							else
							{
								// アセットバンドル＝複合アセットのケース
								path = ( $"{localAssetsRootPath}{assetName}" ) ;
							}
						}
					}

					// まずはそのままロードしてみる
					var asset = assetBundle.LoadAsset( path, type ) ;
					if( asset == null )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								asset = assetBundle.LoadAsset( path + extension, type ) ;
								if( asset != null )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}
					
					return asset ;
				}

				//-----------------------
				// AssetBundleInfo :: AllAssets

				/// <summary>
				/// 全てのアセットを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <param name="resourcePath"></param>
				/// <returns></returns>
				internal protected UnityEngine.Object[] LoadAllAssets( AssetBundle assetBundle, Type type, string localAssetsRootPath, string localAssetPath, AssetBundleManager instance )
				{
					var assets = assetBundle.LoadAllAssets() ;
					if( type != null && assets != null && assets.Length >  0 )
					{
						// タイプ指定があるならタイプで絞る
						var temporaryAssets = new List<UnityEngine.Object>() ;
						foreach( var asset in assets )
						{
							if( asset.GetType() == type )
							{
								temporaryAssets.Add( asset ) ;
							}
						}

						if( temporaryAssets.Count == 0 )
						{
							return null ;
						}

						assets = temporaryAssets.ToArray() ;
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = $"{localAssetsRootPath}{localAssetPath}/{assets[ i ].name}:{assets[ i ].GetType()}" ;
							if( instance.ResourceCache != null && instance.ResourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュされているインスタンスを返す(参照カウントも増加する)
								assets[ i ] = instance.ResourceCache[ resourceCachePath ].Load() ;
							}
//							else
//							{
//								assets[ i ] = ReplaceShader( assets[ i ], assets[ i ].GetType() ) ;
//							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
				
				//-----------------------

				// AssetBundleInfo :: SubAsset

				/// <summary>
				/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="assetBundle">アセットバンドルのインスタンス</param>
				/// <param name="assetBundleName">アセットバンドル名</param>
				/// <param name="assetName">アセット名</param>
				/// <param name="subName">サブアセット名</param>
				/// <param name="instance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="resourcePath">アセットのリソースパス</param>
				/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadSubAsset
				(
					AssetBundle assetBundle,
					string assetBundlePath, string assetName, string subAssetName, Type type, bool isSingle,
					string localAssetsRootPath, string localAssetPath,
					AssetBundleManager instance
				)
				{
					var subAssets = LoadAllSubAssets( assetBundle, assetBundlePath, assetName, type, isSingle, localAssetsRootPath, localAssetPath, instance ) ;
					if( subAssets == null || subAssets.Length == 0 )
					{
						return null ;
					}

					UnityEngine.Object asset = null ;

					foreach( var subAsset in subAssets )
					{
						if( subAsset.name == subAssetName && subAsset.GetType() == type )
						{
							asset = subAsset ;
							break ;
						}
					}

//					if( asset != null )
//					{
//						asset = ReplaceShader( asset, type ) ;
//					}

					return asset ;
				}

				//-----------------------

				// AssetBundleInfo :: AllSubAssets

				/// <summary>
				/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object[] LoadAllSubAssets
				(
					AssetBundle assetBundle,
					string assetBundlePath, string assetName, Type type, bool isSingle,
					string localAssetsRootPath, string localAssetPath,
					AssetBundleManager instance
				)
				{
					string path ;

					if( isSingle == false )
					{
						// 複数のアセットファイルが存在するタイプ

						// ひとまず assetName は空想定でやってみる
						if( string.IsNullOrEmpty( assetName ) == true )
						{
							// アセットバンドル＝単一アセットのケース
							path = ( $"{localAssetsRootPath}{assetBundlePath}" ) ;
						}
						else
						{
							// アセットバンドル＝複合アセットのケース
							path = ( $"{localAssetsRootPath}{assetBundlePath}/{assetName}" ) ;
						}
					}
					else
					{
						// 単体のアセットファイルが存在するタイプ
						int i = assetBundlePath.LastIndexOf( '/' ) ;
						if( i >= 0 )
						{
							// フォルダがある

							assetBundlePath = assetBundlePath[ ..i ] ;

							if( string.IsNullOrEmpty( assetName ) == true )
							{
								// アセットファイル名が空(基本的にありえない)
								path = ( $"{localAssetsRootPath}{assetBundlePath}" ) ;
							}
							else
							{
								path = ( $"{localAssetsRootPath}{assetBundlePath}/{assetName}" ) ;
							}
						}
						else
						{
							// フォルダがない

							if( string.IsNullOrEmpty( assetName ) == true )
							{
								// アセットファイル名が空(基本的にありえない)
								return null ;
							}
							else
							{
								// アセットバンドル＝複合アセットのケース
								path = ( $"{localAssetsRootPath}{assetName}" ) ;
							}
						}
					}

					var assets = assetBundle.LoadAssetWithSubAssets( path, type ) ;	// 注意：該当が無くても配列数０のインスタンスが返る
					if( assets == null || assets.Length == 0 )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								assets = assetBundle.LoadAssetWithSubAssets( $"{path}{extension}", type ) ;
								if( assets != null && assets.Length >  0 )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = $"{localAssetsRootPath}{localAssetPath}/{assets[ i ].name}:{type}" ;
							if( instance.ResourceCache != null && instance.ResourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュされているインスタンスを返す(参照カウントも増加する)
								assets[ i ] = instance.ResourceCache[ resourceCachePath ].Load() ;
							}
//							else
//							{
//								assets[ i ] = ReplaceShader( assets[ i ], type ) ;
//							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
			}
		}
	}
}
