using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using StorageHelper ;


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
			// 優先順位に応じたソース元のパスを取得する
			private string GetSelectedUri( AssetBundleInfo assetBundleInfo, LocationTypes locationType )
			{
				bool isDetected = false ;
				string url = string.Empty ;

				if
				(
					isDetected == false &&
					string.IsNullOrEmpty( StreamingAssetsRootPath ) == false &&
					(
						locationType == LocationTypes.StreamingAssets ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets )
					)
				)
				{
					// StreamingAssets が対象になっている
//					url = "StreamingAssets://" + Application.streamingAssetsPath + "/" + StreamingAssetsRootPath + assetBundleInfo.Path ;
					url = $"StreamingAssets://{StreamingAssetsRootPath}{assetBundleInfo.Path}" ;
					isDetected = true ;
				}

				//=================================================================================================================
				//-----------------------------------------------------------------------------------------------------------------
				// リモートのクラウドストレージに配置されたアセットバンドルをダウンロードする

				if
				(
					isDetected == false &&
					string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath  ) == true  &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{
					// ネットワークからダウンロードを試みる
					url = $"{RemoteAssetBundleRootPath}{assetBundleInfo.Path}" ;
					isDetected = true ;
				}

				//=================================================================================================================
				//-----------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
				// ローカルストレージにビルドされたアセットバンドルをロードする

				if
				(
					isDetected == false &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath ) == false &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{

					// ローカルファイル からダウンロードを試みる
					url = $"file://{LocalAssetBundleRootPath}{assetBundleInfo.Path}" ;
					isDetected = true ;
				}
#endif
				return url ;
			}

			// ローカルにアセットバンドルが存在しない場合にリモートから取得しローカルに保存する
			private IEnumerator LoadAssetBundleFromRemote_Coroutine
			(
				AssetBundleInfo assetBundleInfo, LocationTypes locationType,
				Action<float,float> onProgress, Action<string> onResult,
				bool isManifestSaving,
				Request request,
				AssetBundleManager instance
			)
			{
				// ダウンロード中の数をカウントする
				DownloadingCount ++ ;

				//---------------------------------

				if( request != null )
				{
					// アセットバンドルのファイルサイズ(ただし CRC ファイルがない場合は 0 にになっている)
					request.EntireDataSize	= assetBundleInfo.Size ;
					request.EntireFileCount	= 1 ;
				}

				// リモートからのダウンロードの際にストレージにダイレクトに保存するかどうか
				bool directSave				= false ;
				if( instance.m_DirectSaveEnabled == true && DirectSaveEnabled == true && assetBundleInfo.Size >  0 && assetBundleInfo.Size >  instance.m_LargeReceiveBufferSize )
//				if( DirectSaveEnabled == true && assetBundleInfo.Size >  0 )
				{
					// 一定のサイズ(受信バッファサイズ)を超えるものはダウンロードと同時にストレージに直接保存にする
					// そうでないもの(受信バッファサイズより小さいサイズのファイル)はダウンロードしきった後に保存する
					directSave = true ;		// ダウンロードしながら保存する
				}

				// ダイレクトセーブが行われたかどうか
				bool directSaveSuccessful	= false ;

				string path ;
				byte[] data	= null ;
				long size	= 0 ;

				bool isLoaded = false ;	// 多重実行されないための念のためのフラグ

				// アセットバンドルファイルの保存パス
				string storagePath = $"{StorageCacheRootPath}{ManifestName}/{assetBundleInfo.Path}" ;

				//-----------------------------------------------------------------------------------------
				// 予めサイズがわかっている場合にキャッシュを空ける

				if( assetBundleInfo.Size >  0 && CacheSize >  0 )
				{
					// キャッシュサイズ制限があるので足りなければ破棄可能で古いアセットバンドルを破棄する
					if( Cleanup( assetBundleInfo.Size ) == false )
					{
						// 空き領域が確保出来ない
						DownloadingCount -- ;	// ダウンロード中の数を減少させる

						Error = "Could not alocate space" ;
						onResult?.Invoke( Error ) ;    // 失敗

						yield break ;
					}
				}

				//=================================================================================================================
				//-----------------------------------------------------------------------------------------------------------------
				// StreamingAssets に配置されたアセットバンドルをロードする

				if
				(
					isLoaded == false &&
					string.IsNullOrEmpty( StreamingAssetsRootPath ) == false &&
					(
						locationType == LocationTypes.StreamingAssets ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.StreamingAssets )
					)
				)
				{
#if UNITY_EDITOR
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string logMessage = "<color=#FFFF00>[Download(StreamingAssets)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")</color>" ;
						logMessage += "\n" + StreamingAssetsRootPath + assetBundleInfo.Path ;
						Debug.Log( logMessage ) ;
					}
#endif
					//--------------------------------

					// StreamingAssets からダウンロードを試みる
					yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssetsAsync( $"{StreamingAssetsRootPath}{assetBundleInfo.Path}", ( _1, _2 ) => { data = _1 ; } ) ) ;
					if( data != null && data.Length >  0 )
					{
						onProgress?.Invoke( 1.0f, 0.0f ) ;

						if( request != null )
						{
							request.StoredDataSize = data.Length ;
							request.Progress = 1.0f ;
							request.StoredFileCount = 1 ;
						}
					}
					else
					{
						// 極めて危険な現象(ストレージからのロードが間に合っていない)
						Debug.LogWarning( "[危険]ストリーミングアセットからのロード失敗 : " + StreamingAssetsRootPath + assetBundleInfo.Path ) ;
					}

					//--------------------------------------------------------

					// ロード自体は終了
					isLoaded = true ;
				}

				//=================================================================================================================
				//-----------------------------------------------------------------------------------------------------------------
				// リモートのクラウドストレージに配置されたアセットバンドルをダウンロードする

				if
				(
					isLoaded == false &&
					string.IsNullOrEmpty( RemoteAssetBundleRootPath ) == false &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath  ) == true  &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{
#if UNITY_EDITOR
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string color = "FFFF00" ;
						string style = "" ;
						if( directSave == true )
						{
							color = "FFDF00" ;
							style = " [D]" ;
						}

						string logMessage = "<color=#" + color + ">[Download(Remote)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")" + style + "</color>" ;
						logMessage += "\n" + RemoteAssetBundleRootPath + assetBundleInfo.Path ;

						Debug.Log( logMessage ) ;
					}
#endif
					//----------------------------------------------------------------------------------------

					long fileSize = assetBundleInfo.Size ;

					if( directSave == true && fileSize >  0 && CacheSize >  0 )
					{
						// キャッシュサイズ制限があるので足りなければ破棄可能で古いアセットバンドルを破棄する
						if( Cleanup( fileSize ) == false )
						{
							// 空き領域が確保出来ない
							DownloadingCount -- ;	// ダウンロード中の数を減少させる

							Error = "Could not alocate space" ;
							onResult?.Invoke( Error ) ;    // 失敗

							yield break ;
						}

						// セーブはしないがこのファイル分の領域をキャッシュに確保する
						assetBundleInfo.IsDownloading = true ;	// まだストレージには保存されていないが空き容量はこのファイル分のサイズを減算して計算する
					}

					//----------------------------------------------------------------------------------------

					// ネットワークからダウンロードを試みる
					path = $"{RemoteAssetBundleRootPath}{assetBundleInfo.Path}?time={GetClientTime()}" ;

					yield return instance.StartCoroutine
					(
						DownloadFromRemote
						(
							path,
							fileSize,											// 基本的にサイズはわかっている
							directSave == false ? null	: storagePath,			// ダウンロードから直接保存
							directSave == false ? 0		: assetBundleInfo.Crc,	// ダウンロードから直接保存でなければＣＲＣのチェックも不要
							( DownloadStates state, byte[] downloadedData, float progress, long downloadedSize, string errorMessage, int version ) =>
							{
								Progress = progress ;
								onProgress?.Invoke( Progress, 0.0f ) ;

								if( request != null )
								{
									request.StoredDataSize	= downloadedSize ;
									request.Progress		= Progress ;
								}

								//-----------------------------

								if( state == DownloadStates.Successed )
								{
									// 成功
									if( request != null )
									{
										request.StoredFileCount = 1 ;
									}

									if( directSave == false )
									{
										data = downloadedData ;
									}
									else
									{
										// 最終的なサイズ
										size = downloadedSize ;
									}

									Error = string.Empty ;
								}
								else
								if( state == DownloadStates.Failed )
								{
									// 失敗
									Error = errorMessage ;
								}
							},
							instance
						)
					) ;

					//----------------------------------------------------------------------------------------

					if( directSave == true && fileSize >  0 && CacheSize >  0 )
					{
						// ダウンロード中のフラグをクリアする
						assetBundleInfo.IsDownloading = false ;
					}

					//----------------------------------------------------------------------------------------

					if( directSave == false )
					{
						// 直接保存ではない

						if( string.IsNullOrEmpty( Error ) == false )
						{
							// 失敗

							DownloadingCount -- ;	// ダウンロード中の数を減少させる

							onResult?.Invoke( Error ) ;    // 失敗

							yield break ;
						}
					}
					else
					{
						// 直接保存である

						if( string.IsNullOrEmpty( Error ) == false )
						{
							// 失敗

							// 中途半端にファイルが保存されていたら削除する(おそらく不要だが保険)
							if( StorageAccessor_Exists( storagePath ) == StorageAccessor.TargetTypes.File )
							{
								StorageAccessor_Remove( storagePath ) ;
							}

							DownloadingCount -- ;	// ダウンロード中の数を減少させる

							onResult?.Invoke( Error ) ;    // 失敗

							yield break ;
						}

	//					if( assetBundleInfo.Size >  0 && assetBundleInfo.Size != ( int )totalSize )
	//					{
	//						// サイズ異常(チェックはしない=どのみちＣＲＣで弾かれるため)
	//					}

						if( assetBundleInfo.Size == 0 )
						{
							// 基本的にここに来る事は無い
							assetBundleInfo.Size	= size ;		// ＣＲＣファイルが存在しない場合はここで初めてサイズが書き込まれる
						}

						// 直接保存に成功した
						directSaveSuccessful = true ;
					}

					//----------------------------------------------------------------------------------------

					// ロード自体は終了
					isLoaded = true ;
				}

				//=================================================================================================================
				//-----------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
				// ローカルストレージにビルドされたアセットバンドルをロードする

				if
				(
					isLoaded == false &&
					string.IsNullOrEmpty( LocalAssetBundleRootPath ) == false &&
					(
						locationType == LocationTypes.Storage ||
						( locationType == LocationTypes.StorageAndStreamingAssets && assetBundleInfo.LocationPriority == LocationPriorities.Storage )
					)
				)
				{
					// ログの出力
					if( AssetBundleManager.LogEnabled == true )
					{
						string logMessage = "<color=#FFFF00>[Download(Local)] " + assetBundleInfo.Path + " (" + instance.GetSizeName( assetBundleInfo.Size ) + ")</color>" ;
						logMessage += "\n" + LocalAssetBundleRootPath + assetBundleInfo.Path ;
						Debug.Log( logMessage ) ;
					}

					//--------------------------------

					// ローカルファイル からダウンロードを試みる
					data = File_Load( $"{LocalAssetBundleRootPath}{assetBundleInfo.Path}" ) ;
					if( data != null && data.Length >  0 )
					{
						if( request != null )
						{
							request.StoredDataSize = data.Length ;
							request.Progress = 1.0f ;
							request.StoredFileCount = 1 ;
						}
					}

					//--------------------------------------------------------

					// ロード自体は終了
					isLoaded = true ;
				}
#endif
				//=================================================================================================================

				if( directSaveSuccessful == false )
				{
					// ストレージへ直接保存は行われていない

					if( data == null )
					{
						// 失敗
						DownloadingCount -- ;	// ダウンロード中の数を減少させる

						Error = "Could not load data" ;
						onResult?.Invoke( Error ) ;    // 失敗

						yield break ;
					}

					//--------------------------------

					if( assetBundleInfo.Crc != 0 )
					{
						// ＣＲＣのチェックが必要
						uint crc = 0 ;

						if( data.Length <  ( 1024 * 10124 ) )
						{
							// 同スレッドでＣＲＣを計算する
							crc = GetCRC32( data ) ;
						}
						else
						{
							// 別スレッドでＣＲＣを計算する
							yield return instance.StartCoroutine( GetCRC32Async( data, _ => { crc = _ ; }, instance.m_WritingCancellationSource.Token ) ) ;
						}

						if( crc != assetBundleInfo.Crc )
						{
							// 失敗
							Debug.LogWarning( "[AssetBundle Downloading]Bad CRC : Path = " + RemoteAssetBundleRootPath + assetBundleInfo.Path + " Size = " + data.Length + " Crc[F] = " + crc + " ! Crc[A] = " + assetBundleInfo.Crc ) ;

							DownloadingCount -- ;	// ダウンロード中の数を減少させる

							Error = "Bad CRC" ;
							onResult?.Invoke( Error ) ;    // 失敗

							yield break ;
						}
					}

					//--------------------------------

					if( assetBundleInfo.Size == 0 && CacheSize >  0 )
					{
						// キャッシュサイズ制限があるので足りなければ破棄可能で古いアセットバンドルを破棄する
						if( Cleanup( data.Length ) == false )
						{
							// 空き領域が確保出来ない
							DownloadingCount -- ;	// ダウンロード中の数を減少させる

							Error = "Could not alocate space" ;
							onResult?.Invoke( Error ) ;    // 失敗

							yield break ;
						}
					}

					//------------------------------------------------------
					// 保存する

					// セーブはしないがこのファイル分の領域をキャッシュに確保する
					assetBundleInfo.IsDownloading = true ;	// まだストレージには保存されていないが空き容量はこのファイル分のサイズを減算して計算する

					// ダウンロード中のサイズもキャッシュ使用分に含めるためサイズを保存する
					assetBundleInfo.Size = data.Length ;	// 実際は既に同じ値が設定されているはずである

					//--------------------------------------------------------
					// ここは並列に実行される(ファイルサイズによって完了時間が異なるためマニフェスト更新時はキャッシュ整理時の実行順に合わせるようにする)

					bool saveResult = false ;

					if( instance.m_AsynchronousWritingEnabled == false )
					{
						// 同期保存
						saveResult = StorageAccessor_Save( storagePath, data, true ) ;
						if( saveResult == true )
						{
							onProgress?.Invoke( 1.0f, 1.0f ) ;
						}
					}
					else
					{
						// 非同期保存
						yield return instance.StartCoroutine
						(
							// 非同期でストレージに保存する
							StorageAccessor_SaveAsync
							(
								storagePath,
								data,
								true,
								null,
								null,
								( float writingProgress ) =>
								{
									onProgress?.Invoke( 1.0f, writingProgress ) ;
								},
								( bool _ ) => { saveResult = _ ; },
								instance.m_WritingCancellationSource.Token
							)
						) ;
					}

					//--------------------------------------------------------
					// ストレージへの保存完了

					// ダウンロード中のフラグをクリアする
					assetBundleInfo.IsDownloading = false ;

					//--------------------------------------------------------

					if( saveResult == false )
					{
						// 情報が保存出来ない

						DownloadingCount -- ;	// ダウンロード中の数を減少させる

						Error = "Could not save" ;
						onResult?.Invoke( Error ) ;    // 失敗

						yield break ;
					}
				}

				//=================================================================================================================
				// 全て成功

				assetBundleInfo.IsCompleted		= true ;			// ファイルは完全な状態で保存された

				assetBundleInfo.LastUpdateTime	= GetClientTime() ;	// タイムスタンプ更新

				assetBundleInfo.UpdateRequired	= false ;			// 更新は不要(最新の状態のものである)

				Modified = true ;									// アセットバンドルの状態に変化があった

				//--------------------------------
				// まとめてダウンロードの場合は１ファイル単位ではマニフェストのセーブは行わない

				if( isManifestSaving == true )
				{
					// マニフェストをセーブする(個別ダウンロードの場合)
					Save() ;	// アプリ終了時にもセーブは実行されるのでここでの失敗は無視する
				}

				//-----------------------------------------------------------------------------------------

				DownloadingCount -- ;	// ダウンロード中の数を減少させる

				// 成功
				onResult?.Invoke( string.Empty ) ;
			}			
		}
	}
}
