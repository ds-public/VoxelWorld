//#define UseBestHTTP

using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

using StorageHelper ;

#if UseBestHTTP
using BestHTTP ;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Tls ;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509 ;
#endif

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
#if !UseBestHTTP
		//-------------------------------------------------------------------------------------------
		// Unity Standard

		//-----------------------------------------------------------

		// 固定バッファハンドラー
		class FileDownloadHandler : DownloadHandlerScript
		{
			private readonly string			m_Path ;
			private FileStream				m_File ;

			private int						m_Offset ;
			private int						m_Length ;

			private uint					m_CRC32 ;

			private AssetBundleManager		m_Instance ;

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="buffer"></param>
			public FileDownloadHandler( string path, byte[] buffer, AssetBundleManager instance ) : base( buffer )
			{
				m_Path		= path ;
				m_File		= StorageAccessor_Open( m_Path, StorageAccessor.FileOperationTypes.CreateAndWrite, true ) ;

				m_Offset	= 0 ;
				m_Length	= 0 ;

				m_CRC32		= CRC32_MASK ;

				//---------------------------------

				m_Instance	= instance ;

				m_Instance.AddOnQuitCallback( OnQuit ) ;
			}

			// 終了時に呼び出して欲しいコールバック
			private void OnQuit()
			{
				// 開いているファイルストリームのハンドルを閉じる
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;
				}

				// Remove は不要
				m_Instance = null ;
			}

			//----------------------------------------------------------

			// ダウンロードサイズの通知があった際に呼び出される
			protected override void ReceiveContentLengthHeader( ulong contentLength )
			{
				// ファイルサイズを保存する
				m_Length = ( int )contentLength ;
			}

			// 受信した際に呼び出される
			protected override bool ReceiveData( byte[] data, int length )
			{
				if( m_File != null && length >  0 )
				{
					// ダウンロード逐次保存ではメインスレッド同期で処理する(メインスレッドのパフォーマンス的には問題なさそう:サブスレッドでも処理出来なくはなさそうだが処理がかなり複雑になる上に別の部分でパフォーマンスを下げそうなのでひとまず現状の形でいく)
					if( StorageAccessor_Write( m_File, data, 0, length ) == true )
					{
						// ↑Seek は無しでも末尾に追加してくれてはいるようだ
						m_Offset += length ;

						// ＣＲＣ値を更新する
						m_CRC32 = GetCRC32( m_CRC32, data, length ) ;
					}
				}

				return true ;
			}

			// ダウンロード進行度を取得する
			protected override float GetProgress()
			{
				if( m_Length == 0 )
				{
					return 0.0f ;
				}

				return ( float )m_Offset / ( float )m_Length ;
			}

			// 受信が完了した際に呼び出される
			protected override void CompleteContent()
			{
				Close() ;
			}

			//------------------------------------------------------------------------------------------
			// 拡張

			/// <summary>
			/// ＣＲＣ値を取得する
			/// </summary>
			/// <returns></returns>
			public uint GetCRC()		=> m_CRC32 ;

			/// <summary>
			/// ダウンロードしたサイズ
			/// </summary>
			public long downloadedBytes	=> m_Offset ;

			//------------------------------------------------------------------------------------------
			// 閉じる

			/// <summary>
			/// ファイルストリーム操作を終了する
			/// </summary>
			public void Close()
			{
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;

					// 最後のＸＯＲ
					m_CRC32 = GetCRC32( m_CRC32 ) ;
				}

				if( m_Instance != null )
				{
					m_Instance.RemoveOnQuitCallback( OnQuit ) ;
					m_Instance = null ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// マニフェスト単位での処理
		public partial class ManifestInfo
		{
			// ダウンロードを実行する
			private IEnumerator DownloadFromRemote( string url, string storagePath, uint fileCrc, Action<DownloadStates,byte[],float,long,string,int> onProgress, AssetBundleManager instance )
			{
				DownloadStates	state	= DownloadStates.Processing ;
				byte[]	data			= null ;
				float	progress		= 0 ;
				long	downloadedSize	= 0 ;
				string	error			= null ;
				int		version			= 0 ;

				//---------------------------------
				// ダウンロード実行

				//---------------------------------------------------------
				// HTTP ヘッダーの設定

				Dictionary<string,string> header = new Dictionary<string, string>()
				{
					// バイト配列通信限定
					{  "Content-Type", "application/octet-stream" }
				} ;

				if( instance.m_ConstantHeaders.Count >  0 )
				{
					foreach( var constantHeader in instance.m_ConstantHeaders )
					{
						if( header.ContainsKey( constantHeader.Key ) == false )
						{
							// 追加
							header.Add( constantHeader.Key, constantHeader.Value ) ;
						}
						else
						{
							// 上書
							header[ constantHeader.Key ] = constantHeader.Value ;
						}
					}
				}

				//---------------------------------

				// ヘッダー値のエスケープ(ヘッダも文字列もＵＲＬエンコードする必要がある)
				if( header.Count >  0 )
				{
					int i, l = header.Count ;
					string[] keys = new string[ l ] ;
					header.Keys.CopyTo( keys, 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( header[ keys[ i ] ] == null )
						{
							// Null はサーバーで問題を引き起こす可能性があるため空文字に変える
							header[ keys[ i ] ] = string.Empty ;
						}

						header[ keys[ i ] ] = instance.EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;

	//					Debug.LogWarning( "HN:" + keys[ i ] + " HV:" + header[ keys[ i ] ] ) ;
	//					header[ keys[ i ] ] = EscapeHttpHeaderValue( header[ keys[ i ] ] ) ;
					}
				}

				//-----------------------------------------------------------------------------------------
				// 通信リクエスト生成

				UnityWebRequest www ;

				// 受信バッファを生成または取得する
				byte[] receiveBuffer				= null ;
				FileDownloadHandler downloadHandler	= null ;

				if( string.IsNullOrEmpty( storagePath ) == false )
				{
					// 直接ストレージへ保存するケース
					www = new UnityWebRequest( url, "GET" ) ;

					// 受信バッファを取得する
					receiveBuffer = instance.KeepReceiveBuffer() ;

					// ダウンロードハンドラーを生成する
					downloadHandler = new FileDownloadHandler( storagePath, receiveBuffer, instance ) ;

					// ダウンロードハンドラーを設定する
					www.downloadHandler = downloadHandler ;
				}
				else
				{
					// 直接ストレージへ保存しないケース
					www = UnityWebRequest.Get( url ) ;
				}

				//---------------------------------------------------------

				// ヘッダを設定
				if( header.Count >  0 )
				{
					int i, l = header.Count ;
					string[] keys = new string[ l ] ;
					header.Keys.CopyTo( keys, 0 ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
//						Debug.Log( "<color=#FF3FFF>[AssetBundleManager] HTTP Header : Key = " + keys[ i ] + " Value = " + header[ keys[ i ] ] + "</color>" ) ;
						www.SetRequestHeader( keys[ i ], header[ keys[ i ] ] ) ;
					}
				}

				//---------------------------------------------------------

				// 通信リクエスト実行
				www.SendWebRequest() ;

				while( true )
				{
					if( IsNetworkError( www ) == false )
					{
						// エラーは起きていない

						if( string.IsNullOrEmpty( storagePath ) == false )
						{
							// 直接ストレージへ保存するケース
							progress		= www.downloadProgress ;
							downloadedSize	= downloadHandler.downloadedBytes ;
						}
						else
						{
							// 直接ストレージへ保存しないケース
							progress		= www.downloadProgress ;
							downloadedSize	= ( long )www.downloadedBytes ;
						}

						// 途中のコールバック
						onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;

						if( www.isDone == true )
						{
							// 成功
							state	= DownloadStates.Successed ;
							error	= string.Empty ;
							version	= 1 ;

							break ;
						}
					}
					else
					{
						// エラー発生
						state	= DownloadStates.Failed ;
						error	= www.error ;
						version	= 1 ;

						break ;
					}

					yield return null ;
				}

				//---------------------------------------------------------

				if( string.IsNullOrEmpty( storagePath ) == false )
				{
					// 直接ストレージへ保存するケース

					downloadHandler.Close() ;	// ファイルストリームの操作を終了する(念のため保険)

					bool isCompleted = false ;

					if( www.isDone == true )
					{
						// 成功

						if( fileCrc != 0 )
						{
							// ＣＲＣ値を取得する(必ず Close が実行された後の値を使用する事)
							uint crc = downloadHandler.GetCRC() ;

							if( crc == fileCrc )
							{
								isCompleted = true ;
							}
							else
							{
								state = DownloadStates.Failed ;
								error = "Bad Crc" ;
								version	= 1 ;
							}
						}
						else
						{
							isCompleted = true ;
						}
					}
					
					if( isCompleted == false )
					{
						// 失敗

						// ゴミファイルがあれば削除する
						if( StorageAccessor_Exists( storagePath ) == StorageAccessor.Target.File )
						{
							StorageAccessor_Remove( storagePath ) ;
						}
					}
				}
				else
				{
					// 直接ストレージへ保存しないケース
					if( www.isDone == true )
					{
						// 成功
						data	= www.downloadHandler.data ;

						if( fileCrc != 0 )
						{
							// ＣＲＣ値を取得する
							uint crc = GetCRC32( data ) ;

							if( crc != fileCrc )
							{
								state	= DownloadStates.Failed ;
								error = "Bad Crc" ;
								version	= 1 ;
							}
						}
					}
				}

				//---------------------------------------------------------
				// 破棄

				www.Dispose() ;

				if( downloadHandler != null )
				{
					// ダウンロードハンドラーを破棄する
					downloadHandler.Dispose() ;
					downloadHandler  = null ;
				}

				if( receiveBuffer != null )
				{
					// 受信バッファを解放する
					instance.FreeReceiveBuffer( receiveBuffer ) ;
					receiveBuffer  = null ;
				}

				//---------------------------------------------------------

				// 最後のコールバック(ＣＲＣ値も返す)
				onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
			}

			// プロトコルの細かい設定を行う
			private void SetHttpSettings()
			{
				// HTTP/1.1

				// 何もしない
			}
		}

		//---------------------------------------------------------------------------
#else
		//-------------------------------------------------------------------------------------------
		// BestHTTP

		/// <summary>
		///  セキュア認証を通すためのクラス
		/// </summary>
		class CustomVerifier : ICertificateVerifyer
		{
			public bool IsValid( Uri serverUri, X509CertificateStructure[] certs )
			{
				return true ;
			}
		}

		// マニフェスト単位での処理
		public partial class ManifestInfo
		{
			// BestHTTP を用いたダウンロード
			private IEnumerator DownloadFromRemote( string path, Action<DownloadStates,byte[],float,long,string,int> onProgress )
			{
				DownloadStates	state	= DownloadStates.Processing ;
				byte[]	data			= null ;
				float	progress		= 0 ;
				long	downloadedSize	= 0 ;
				string	error			= null ;
				int		version			= 0 ;

				//----------------------------------
				// コールバック

				void OnDownloadProgress( HTTPRequest request, long offset, long length )
				{
					progress		= ( offset / ( float )length ) ;
					downloadedSize	= offset ;

					onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
				}

				void OnRequestFinished( HTTPRequest request, HTTPResponse response )
				{
//					Debug.Log( "HTTP/" + response.VersionMajor + "." + response.VersionMinor ) ;
					version = response.VersionMajor ;

					if( response.IsSuccess == true )
					{
						state	= DownloadStates.Successed ;
						data	= response.Data ;
					}
					else
					{
						state	= DownloadStates.Failed ;
						error	= response.Message ;
					}
				}

				//----------------------------------
				// ダウンロード実行

				var request = new HTTPRequest( new Uri( path ), OnRequestFinished ) ;

				// セキュア通信の認証
				request.CustomCertificateVerifyer = new CustomVerifier() ;
				request.UseAlternateSSL = true ;

				request.OnDownloadProgress = OnDownloadProgress ;

				request.Send() ;

				// 終了を待つ
				while( true )
				{
					if( state != DownloadStates.Processing )
					{
						break ;
					}

					yield return null ;
				}

				// 最後のコールバック
				onProgress?.Invoke( state, data, progress, downloadedSize, error, version ) ;
			}

			// プロトコルの細かい設定を行う
			private void SetHttpSettings()
			{
				// HTTP/2.0

				HTTPManager.HTTP2Settings.MaxConcurrentStreams = 64 ;
				HTTPManager.HTTP2Settings.InitialStreamWindowSize = 10 * 1024 * 1024;
				HTTPManager.HTTP2Settings.InitialConnectionWindowSize = HTTPManager.HTTP2Settings.MaxConcurrentStreams * 1024 * 1024 ;
				HTTPManager.HTTP2Settings.MaxFrameSize = 1 * 1024 * 1024 ;
				HTTPManager.HTTP2Settings.MaxIdleTime = TimeSpan.FromSeconds( 120 ) ;

				HTTPManager.HTTP2Settings.WebSocketOverHTTP2Settings.EnableWebSocketOverHTTP2 = true ;
				HTTPManager.HTTP2Settings.WebSocketOverHTTP2Settings.EnableImplementationFallback = true ;
			}
		}
#endif

		//-------------------------------------------------------------------------------------------

#if false
		/// <summary>
		/// タグで指定したアセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundlesWithTagAsync( string tag, bool keep = false, Action<int,int> onProgress = null )
		{
			return DownloadAssetBundlesWithTagsAsync( new string[]{ tag }, keep, onProgress ) ;
		}
		public static Request DownloadAssetBundlesWithTagsAsync( string[] tags, bool keep = false, Action<int,int> onProgress = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundlesWithTagsAsync_Private( m_Instance.m_DefaultManifestName, tags, keep, onProgress, request ) ) ;
			return request ;
		}

		/// <summary>
		/// タグで指定したアセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundlesWithTagsAsync( string manifestName, string tag, bool keep = false, Action<int,int> onProgress = null )
		{
			return DownloadAssetBundlesWithTagsAsync( manifestName, new string[]{ tag }, keep, onProgress ) ;
		}
		public static Request DownloadAssetBundlesWithTagsAsync( string manifestName, string[] tags, bool keep = false, Action<int,int> onProgress = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundlesWithTagsAsync_Private( manifestName, tags, keep, onProgress, request ) ) ;
			return request ;
		}

		// タグで指定したアセットバンドルのダウンロードを行う
		private IEnumerator DownloadAssetBundlesWithTagsAsync_Private( string manifestName, string[] tags, bool keep, Action<int,int> onProgress, Request request )
		{
			if( tags == null || tags.Length == 0 )
			{
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Invalid tags." ;
				}
				yield break ;
			}

			//--------------------------

			bool isCompleted = false ;
			string error = string.Empty ;

			if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true  )
			{
				yield return StartCoroutine( m_ManifestHash[ manifestName ].DownloadAssetBundlesWithTags_Coroutine
				(
					tags,
					keep,
					onProgress,
					request,
					this
				) ) ;
			}

			if( isCompleted == false )
			{
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;
				yield break ;
			}

			request.IsDone = true ;
		}
#endif

	}
}
