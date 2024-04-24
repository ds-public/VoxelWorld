using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

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
		//-------------------------------------------------------------------------------------------
		// Unity Standard

		//-----------------------------------------------------------

		// バッファハンドラー(共通)
		class DownloadHandler_Common : DownloadHandlerScript
		{
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="buffer"></param>
			public DownloadHandler_Common( byte[] buffer ) : base( buffer ){}

			/// <summary>
			/// ダウンロード中のサイズ
			/// </summary>
			public virtual long DownloadedBytes{ get{ return 0 ; } }

			/// <summary>
			/// ストレージに書き込み中かどうか
			/// </summary>
			public virtual bool IsWriting{ get{ return false ; } }

			/// <summary>
			/// ＣＲＣ３２値を取得する
			/// </summary>
			/// <returns></returns>
			public virtual uint	CRC32{ get{ return 0 ; } }

			/// <summary>
			/// 閉じる
			/// </summary>
			public virtual void Close(){}
		}

		//-------------------------------------------------------------------------------------------

		// バッファハンドラー(一括用)
		class DownloadHandler_LargeReceiveBuffer : DownloadHandler_Common
		{
			private readonly string			m_Url ;
			private readonly long			m_Size ;
			private readonly bool			m_CRC32_Processing ;

			private long					m_Offset ;
			private long					m_Length ;

			private uint					m_CRC32 ;

			private byte[]					m_Data ;

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="buffer"></param>
			public DownloadHandler_LargeReceiveBuffer( string url, long size, bool crc, byte[] buffer ) : base( buffer )
			{
				m_Url				= url ;
				m_Size				= size ;
				m_CRC32_Processing	= crc ;		// ＣＲＣの計算を行うかどうか

				m_Offset			= 0 ;
				m_Length			= 0 ;

				m_CRC32				= CRC32_MASK ;

				//---------------------------------

				if( m_Size >  0 )
				{
					// 予めサイズがかわっている
					m_Data		= new byte[ m_Size ] ;
				}
				else
				{
					m_Data		= new byte[ 0 ] ;
				}
			}

			//----------------------------------------------------------

			// ダウンロードサイズの通知があった際に呼び出される
			protected override void ReceiveContentLengthHeader( ulong contentLength )
			{
				// ファイルサイズを保存する(呼ばれない事もある)
				m_Length = ( long )contentLength ;
			}

			// 受信した際に呼び出される
			protected override bool ReceiveData( byte[] data, int length )
			{
				// 一度の受信データ(ただしコンストラクタで設定したバッファサイズを超える事はない)

				if( length >  0 )
				{
					if( m_Size >  0 )
					{
						// 予めサイズがわかっている

						if( ( m_Offset + length ) >  m_Size )
						{
							// オーバーしました
							Debug.LogWarning( "Downdowd size is over : " + ( m_Offset + length ) + " > " + m_Size + "\n" + m_Url ) ;

							length = ( int )( m_Size - m_Offset ) ;
						}

						if( length >  0 )
						{
							Array.Copy( data, 0, m_Data, m_Offset, length ) ;
						}
					}
					else
					{
						// 予めサイズがわかっていない

						Array.Resize( ref m_Data, ( int )( m_Offset + length ) ) ;
						Array.Copy( data, 0, m_Data, m_Offset, length ) ;
					}

					m_Offset += length ;

					if( m_CRC32_Processing == true )
					{
						// ＣＲＣ値を更新する
						m_CRC32 = GetCRC32( m_CRC32, data, length ) ;
					}
				}

				// ダウンロード継続
				return true ;
			}

			// ダウンロード進行度を取得する
			protected override float GetProgress()
			{
				if( m_Length == 0 )
				{
					return 0.0f ;
				}

				return ( float )( ( double )m_Offset / ( double )m_Length ) ;
			}

			// 受信が完了した際に呼び出される
			protected override void CompleteContent()
			{
//				Debug.Log( "ダウンロード完了:" + m_Url + "\n" + m_Offset + " / " + m_Length ) ;
				Close() ;
			}

			// 受信したデータを取得する
			protected override byte[] GetData()
			{
				if( m_Offset == 0 )
				{
					// データは受信できなかった
					return null ;
				}

				return m_Data ;
			}

			//------------------------------------------------------------------------------------------
			// 拡張

			/// <summary>
			/// ダウンロードしたサイズ
			/// </summary>
			public override long DownloadedBytes	=> m_Offset ;

			/// <summary>
			/// ＣＲＣ値を取得する
			/// </summary>
			/// <returns></returns>
			public override uint CRC32				=> m_CRC32 ;

			//------------------------------------------------------------------------------------------
			// 閉じる

			/// <summary>
			/// ファイルストリーム操作を終了する
			/// </summary>
			public override void Close()
			{
				if( m_CRC32_Processing == true )
				{
					// 最後のＸＯＲ
					m_CRC32 = GetCRC32( m_CRC32 ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// 固定バッファハンドラー(分割用)
		class DownloadHandler_SmallReceiveBuffer : DownloadHandler_Common
		{
			private byte[]					m_ReceiveBuffer ;

			private readonly string			m_Path ;
			private FileStream				m_File ;

			private long					m_Offset ;
			private long					m_Length ;

			private uint					m_CRC32 ;

			private AssetBundleManager		m_Instance ;

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="buffer"></param>
			public DownloadHandler_SmallReceiveBuffer( string path, byte[] buffer, AssetBundleManager instance ) : base( buffer )
			{
				m_ReceiveBuffer	= buffer ;

				m_Path		= path ;
				m_File		= StorageAccessor_Open( m_Path, StorageAccessor.FileOperationTypes.CreateAndWrite, true ) ;

				m_Offset	= 0 ;
				m_Length	= 0 ;

				m_CRC32		= CRC32_MASK ;

				//---------------------------------

				m_Instance	= instance ;

				// 緊急停止時のコールバックハンドラの登録を実行する
				m_Instance.AddOnQuitCallback( OnQuit ) ;
			}

			//----------------------------------------------------------

			// ダウンロードサイズの通知があった際に呼び出される
			protected override void ReceiveContentLengthHeader( ulong contentLength )
			{
				// ファイルサイズを保存する(呼ばれない事もある)
				m_Length = ( long )contentLength ;
			}

			// 受信した際に呼び出される
			protected override bool ReceiveData( byte[] data, int length )
			{
				if( m_File != null )
				{
					if( length >  0 )
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
				}
				else
				{
					// 異常が発生したのでダウンロードを停止させる
					return false ;
				}

				// ダウンロード継続
				return true ;
			}

			// ダウンロード進行度を取得する
			protected override float GetProgress()
			{
				if( m_Length == 0 )
				{
					return 0.0f ;
				}

				return ( float )( ( double )m_Offset / ( double )m_Length ) ;
			}

			// 受信が完了した際に呼び出される
			protected override void CompleteContent()
			{
				Close() ;
			}

			//------------------------------------------------------------------------------------------
			// 拡張

			/// <summary>
			/// ダウンロードしたサイズ
			/// </summary>
			public override long DownloadedBytes	=> m_Offset ;

			/// <summary>
			/// ＣＲＣ値を取得する
			/// </summary>
			/// <returns></returns>
			public override uint CRC32				=> m_CRC32 ;

			//------------------------------------------------------------------------------------------
			// 閉じる

			/// <summary>
			/// ファイルストリーム操作を終了する
			/// </summary>
			public override void Close()
			{
				// 開いているファイルストリームのハンドルを閉じる
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;

					// 最後のＸＯＲ
					m_CRC32 = GetCRC32( m_CRC32 ) ;
				}

				if( m_Instance != null )
				{
					// 緊急停止時のコールバックハンドラの登録を解除する
					m_Instance.RemoveOnQuitCallback( OnQuit ) ;
					m_Instance = null ;
				}
			}

			// 緊急停止時のコールバックハンドラ
			private void OnQuit()
			{
				// 開いているファイルストリームのハンドルを閉じる
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;
				}

				if( m_ReceiveBuffer != null )
				{
					m_Instance.FreeLargeReceiveBuffer( m_ReceiveBuffer ) ;
					m_ReceiveBuffer  = null ;
				}

				// 緊急停止時のコールバックハンドラの登録の解除はは不要
				m_Instance = null ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 固定バッファハンドラー(分割用)
		class DownloadHandler_SmallReceiveBufferAsync : DownloadHandler_Common
		{
			private byte[]					m_ReceiveBuffer ;

			private readonly string			m_Path ;
			private FileStream				m_File ;

			private long					m_Offset ;
			private long					m_Length ;

			private uint					m_CRC32 ;

			private AssetBundleManager		m_Instance ;

			//----------------------------------------------------------

			public enum BufferTypes
			{
				Static,
				Dynaminc,
			}

			public class WritingBuffer
			{
				public byte[]			Data ;
				public long				Size ;
				public BufferTypes		Type ;
			}

			private bool					m_IsWriting ;

			private WritingBuffer			m_WritingBuffer ;

			private List<WritingBuffer>		m_WritingBuffers ;
			private bool					m_WriteResult ;

			//----------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="path"></param>
			/// <param name="buffer"></param>
			public DownloadHandler_SmallReceiveBufferAsync( string path, byte[] buffer, AssetBundleManager instance ) : base( buffer )
			{
				m_ReceiveBuffer	= buffer ;

				m_Path		= path ;
				m_File		= StorageAccessor_Open( m_Path, StorageAccessor.FileOperationTypes.CreateAndWrite, true ) ;

				m_Offset	= 0 ;
				m_Length	= 0 ;

				m_CRC32		= CRC32_MASK ;

				//---------------------------------

				m_Instance	= instance ;

				// 緊急停止時のコールバックハンドラの登録を実行する
				m_Instance.AddOnQuitCallback( OnQuit ) ;

				//---------------------------------------------------------

				m_IsWriting		= false ;	// ストレージへの非同期書き込みを行っている最中かどうか
				m_WriteResult	= true ;	// ストレージへの非同期書き込み結果(１度でも失敗したらこんダウンロードは失敗とみなす)
			}

			//----------------------------------------------------------

			// ダウンロードサイズの通知があった際に呼び出される
			protected override void ReceiveContentLengthHeader( ulong contentLength )
			{
				// ファイルサイズを保存する(呼ばれない事もある)
				m_Length = ( long )contentLength ;
			}

			// 受信した際に呼び出される
			protected override bool ReceiveData( byte[] data, int length )
			{
				if( m_File != null || m_WriteResult == false )
				{
					if( length >  0 )
					{
						if( m_IsWriting == false )
						{
							// 書き込み中ではない

							// バッファをコピーする
							m_WritingBuffer = new WritingBuffer() ;

							if( length <= m_Instance.m_SmallReceiveBufferSize )
							{
								// サイズが小さいのでスタティックバッファを使用する
								m_WritingBuffer.Data = m_Instance.KeepSmallReceiveBuffer() ;
								m_WritingBuffer.Size = length ;
								m_WritingBuffer.Type = BufferTypes.Static ;
							}
							else
							{
								// サイズが大きいのでダイナミックバッファを使用する
								m_WritingBuffer.Data = new byte[ length ] ;
								m_WritingBuffer.Size = length ;
								m_WritingBuffer.Type = BufferTypes.Dynaminc ;
							}

							Array.Copy( data, 0, m_WritingBuffer.Data, 0, length ) ;

							//------------------------------

							// 非同期でストレージに保存する
							m_Instance.StartCoroutine( StorageAccessor_WriteAsync
							(
								m_File,
								m_WritingBuffer.Data,
								0,
								m_WritingBuffer.Size,
								m_Offset,
								OnWritingResult,
								m_Instance.m_WritingCancellationSource.Token
							) ) ;

							// 書き込み中とする
							m_IsWriting = true ;
//							Debug.Log( "<color=#FF7F00>Path = " + m_Path + "\n１回目のデータサイズ = " + m_Instance.GetSizeName( m_WritingBuffer.Size ) + "</color>" ) ;
						}
						else
						{
							// 書き込み中である

							// バッファをコピーする
							var writingBuffer = new WritingBuffer() ;

							if( length <= m_Instance.m_SmallReceiveBufferSize )
							{
								// サイズが小さいのでスタティックバッファを使用する
								writingBuffer.Data = m_Instance.KeepSmallReceiveBuffer() ;
								writingBuffer.Size = length ;
								writingBuffer.Type = BufferTypes.Static ;
							}
							else
							{
								// サイズが大きいのでダイナミックバッファを使用する
								writingBuffer.Data = new byte[ length ] ;
								writingBuffer.Size = length ;
								writingBuffer.Type = BufferTypes.Dynaminc ;

//								Debug.Log( "<color=#00FF00>ダイナミックバッファ確保:" + m_Instance.GetSizeName( m_WritingBuffer.Size ) + " > " + m_Instance.m_SmallReceiveBufferSize + "</color>" ) ;
							}

							Array.Copy( data, 0, writingBuffer.Data, 0, length ) ;

							//------------------------------

							// バッファスタックが生成されていなければ生成する
							m_WritingBuffers ??= new () ;

							// バッファスタックに貯める
							m_WritingBuffers.Add( writingBuffer ) ;

//							Debug.Log( "<color=#FF7F00>Path = " + m_Path + "\n１回で書き込みきれない = " + m_WritingBuffers.Count + " データサイズ = " + m_Instance.GetSizeName( m_WritingBuffer.Size ) + "</color>" ) ;
						}
					}
				}
				else
				{
					// 異常が発生したのでダウンロードを停止させる
					return false ;
				}

				// ダウンロード継続
				return true ;
			}

			// ストレージへの非同期書き込みが終わったら呼び出される
			private void OnWritingResult( bool result )
			{
				// ストレージへの非同期書き込み結果を保存する
				m_WriteResult	= result ;

				if( m_WriteResult == true )
				{
					// 非同期書き込み成功

					// 次の書き込み位置へ
					m_Offset += m_WritingBuffer.Size ;

					// ＣＲＣ値を更新する
					m_CRC32 = GetCRC32( m_CRC32, m_WritingBuffer.Data, m_WritingBuffer.Size ) ;

					//--------------------------------
					// カレントバッファを破棄する

					if( m_WritingBuffer.Type == BufferTypes.Static )
					{
						m_Instance.FreeSmallReceiveBuffer( m_WritingBuffer.Data ) ;
					}

					//--------------------------------

					if( m_WritingBuffers != null && m_WritingBuffers.Count >  0 )
					{
						// 書き込み待ちのデータがある

						//-------------------------------------------------------
						// 次のデータを書き込む

						m_WritingBuffer = m_WritingBuffers[ 0 ] ;
						m_WritingBuffers.RemoveAt( 0 ) ;

						//-------------------------------

						// 次の待機中のバッファを非同期でストレージに保存する
						m_Instance.StartCoroutine( StorageAccessor_WriteAsync
						(
							m_File,
							m_WritingBuffer.Data,
							0,
							m_WritingBuffer.Size,
							m_Offset,
							OnWritingResult,
							m_Instance.m_WritingCancellationSource.Token
						) ) ;

						// 引き続き書き込み中の状態を継続する(m_IsWriting = true)
					}
					else
					{
						// 書き込み待ちのデータがない

						//--------------------------------
						// カレントバッファを破棄する

						if( m_WritingBuffer.Type == BufferTypes.Static )
						{
							m_Instance.FreeSmallReceiveBuffer( m_WritingBuffer.Data ) ;
						}

						m_WritingBuffer	= null ;

						// 書き込みは終了している
						m_IsWriting		= false ;
					}
				}
				else
				{
					// 非同期書き込み失敗

					// 書き込みは終了状態にする(以後は書き込まない)
					CleanupWritingBuffers() ;
					m_IsWriting = false ;
				}
			}

			// ダウンロード進行度を取得する
			protected override float GetProgress()
			{
				if( m_Length == 0 )
				{
					return 0.0f ;
				}

				return ( float )( ( double )m_Offset / ( double )m_Length ) ;
			}

			// 受信が完了した際に呼び出される
			protected override void CompleteContent()
			{
				if( m_IsWriting == false && m_WritingBuffer == null && ( m_WritingBuffers == null || ( m_WritingBuffers != null && m_WritingBuffers.Count == 0 ) ) )
				{
					// ストレージへの書き込みも終了しているようならファイルストリームのハンドルも閉じてしまって良い
					Close() ;
				}
			}

			//------------------------------------------------------------------------------------------
			// 拡張

			/// <summary>
			/// ダウンロードしたサイズ
			/// </summary>
			public override long DownloadedBytes	=> m_Offset ;

			/// <summary>
			/// ストレージへの非同期書き込みか行われている最中かどうか
			/// </summary>
			public override bool IsWriting			=> m_IsWriting ;

			/// <summary>
			/// ＣＲＣ値を取得する
			/// </summary>
			public override uint CRC32				=> m_CRC32 ;

			//------------------------------------------------------------------------------------------
			// 閉じる

			/// <summary>
			/// ファイルストリーム操作を終了する
			/// </summary>
			public override void Close()
			{
				if( m_IsWriting == true )
				{
					// まだ非同期書き込みが行われている最中である
					Debug.LogWarning( "Buffer is writing. path = " + m_Path ) ;

					//--------------------------------

					// 永久待機になってしまう可能性があるのでフラグは終了済みにする
					CleanupWritingBuffers() ;
					m_IsWriting = false ;
				}

				//---------------------------------------------------------

				// 開いているファイルストリームのハンドルを閉じる
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;

					// 最後のＸＯＲ
					m_CRC32 = GetCRC32( m_CRC32 ) ;
				}

				if( m_Instance != null )
				{
					// 緊急停止時のコールバックハンドラの登録を解除する
					m_Instance.RemoveOnQuitCallback( OnQuit ) ;
					m_Instance = null ;
				}
			}

			// 緊急停止時のコールバックハンドラ
			private void OnQuit()
			{
				// 非同期書き込みはキャンセルされたはずなのでフラグも終了済みにする
				CleanupWritingBuffers() ;
				m_IsWriting = false ;

				// 開いているファイルストリームのハンドルを閉じる
				if( m_File != null )
				{
					StorageAccessor_Close( m_Path, m_File ) ;
					m_File  = null ;
				}

				if( m_ReceiveBuffer != null )
				{
					m_Instance.FreeLargeReceiveBuffer( m_ReceiveBuffer ) ;
					m_ReceiveBuffer  = null ;
				}

				// 緊急停止時のコールバックハンドラの登録の解除はは不要
				m_Instance = null ;
			}

			// バッファを解放する
			private void CleanupWritingBuffers()
			{
				if( m_WritingBuffer.Type == BufferTypes.Static )
				{
					m_Instance.FreeSmallReceiveBuffer( m_WritingBuffer.Data ) ;
				}

				m_WritingBuffer	= null ;

				if( m_WritingBuffers != null )
				{
					foreach( var writingBuffer in m_WritingBuffers )
					{
						if( writingBuffer.Type == BufferTypes.Static )
						{
							m_Instance.FreeSmallReceiveBuffer( m_WritingBuffer.Data ) ;
						}
					}

					m_WritingBuffers = null ;
				}
			}
		}
		
		//-------------------------------------------------------------------------------------------
		// マニフェスト単位での処理
		public partial class ManifestInfo
		{
#if false
			// ダウンロード対象ファイルの拡張子を取得する
			private string GetExtension( string url )
			{
				if( string.IsNullOrEmpty( url ) == true )
				{
					// 無し
					return string.Empty ;
				}

				int p ;

				p = url.IndexOf( '?' ) ;
				if( p >= 0 )
				{
					// パラメータがあればカットする
					url = url.Substring( 0, p ) ;
				}

				p = url.LastIndexOf( '.' ) ;
				if( p <  0 )
				{
					return string.Empty ;
				}

				//---------------------------------

				p ++ ;

				int l = url.Length ;
				if( p >= l )
				{
					return string.Empty ;
				}

				string extension = url.Substring( p, l - p ) ;

				return extension ;
			}

			// Content-Type 群
			private static Dictionary<string,string> m_ContentTypes = new Dictionary<string, string>()
			{
				{ "txt",	"text/plain"			},
				{ "csv",	"text/csv"				},
				{ "json",	"application/json"		},
			} ;
#endif
			//------------------------------------------------------------------------------------------

			// ダウンロードを実行する
			private IEnumerator DownloadFromRemote( string url, long fileSize, string storagePath, uint fileCrc, Action<DownloadStates,byte[],float,long,string,int> onProgress, AssetBundleManager instance )
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

				var header = new Dictionary<string, string>()
				{
					// バイト配列通信限定
					{  "Content-Type", "application/octet-stream" }
				} ;

				//---------------------------------------------------------

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
					var keys = new string[ l ] ;
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
				byte[] receiveBuffer					= null ;
				DownloadHandler_Common downloadHandler	= null ;

				if( string.IsNullOrEmpty( storagePath ) == true )
				{
					// 直接ストレージへ保存しないケース(一括)
					// ダウンロードしきった後にまとめて保存する

					www = UnityWebRequest.Get( url ) ;

					//--------------------------------
#if false
					www = new UnityWebRequest( url, "GET" ) ;

					// 受信バッファを取得する(分割用のバッファを使用する)
					receiveBuffer = instance.KeepLargeReceiveBuffer() ;

					// ダウンロードハンドラーを生成する
					downloadHandler = new DownloadHandler_LargeReceiveBuffer( url, fileSize, fileCrc != 0, receiveBuffer ) ;

					// ダウンロードハンドラーを設定する
					www.downloadHandler = downloadHandler ;
#endif
				}
				else
				{
					// 直接ストレージへ保存するケース(分割)
					// ダウンロードしながら保存していく

					www = new ( url, "GET" ) ;

					// 受信バッファを取得する(分割用のバッファを使用する)
					receiveBuffer = instance.KeepSmallReceiveBuffer() ;

					// ダウンロードハンドラーを生成する
					downloadHandler = new DownloadHandler_SmallReceiveBuffer( storagePath, receiveBuffer, instance ) ;

					// ダウンロードハンドラーを設定する
					www.downloadHandler = downloadHandler ;
				}

				//---------------------------------------------------------

				// ヘッダを設定
				if( header.Count >  0 )
				{
					int i, l = header.Count ;
					var keys = new string[ l ] ;
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

						if( string.IsNullOrEmpty( storagePath ) == true )
						{
							// 直接ストレージへ保存しないケース(一括)
							progress		= www.downloadProgress ;
//							downloadedSize	= downloadHandler.DownloadedBytes ;
							downloadedSize	= ( long )www.downloadedBytes ;	// デフォルトハンドラーの場合
						}
						else
						{
							// 直接ストレージへ保存するケース(分割)
							progress		= www.downloadProgress ;
							downloadedSize	= downloadHandler.DownloadedBytes ;
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

				if( string.IsNullOrEmpty( storagePath ) == true )
				{
					// 直接ストレージへ保存しないケース(一括)

					// ファイルストリームの操作を終了する
//					if( downloadHandler != null )
//					{
						downloadHandler?.Close() ;
//					}

					//--------------------------------

					bool isCompleted = false ;

					if( www.isDone == true )
					{
						// 通信終了

						if( fileCrc != 0 )
						{
							// ＣＲＣ値を取得する(必ず Close が実行された後の値を使用する事)
							uint crc = downloadHandler.CRC32 ;

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

						if( isCompleted == true )
						{
							// 成功
							data	= www.downloadHandler.data ;
						}
					}
				}
				else
				{
					// 直接ストレージへ保存するケース(分割)

					if( downloadHandler != null )
					{
						//--------------------------------------------------------

						// 通信が終わってもまだストレージへ書き込み中の可能性のがあるのでそうであれば書き込みが終了するのを待つ
						if( downloadHandler.IsWriting == true )
						{
							while( true )
							{
								if( downloadHandler.IsWriting == false )
								{
									// ストレージへの書き込みが終了した
									break ;
								}

								// １フレーム待つ
								yield return null ;
							}
						}
					
						//--------------------------------------------------------

						// ファイルストリームの操作を終了する
						downloadHandler.Close() ;
					}

					//--------------------------------

					bool isCompleted = false ;

					if( www.isDone == true )
					{
						// 通信終了

						if( fileCrc != 0 )
						{
							// ＣＲＣ値を取得する(必ず Close が実行された後の値を使用する事)
							uint crc = downloadHandler.CRC32 ;

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
						if( StorageAccessor_Exists( storagePath ) == StorageAccessor.TargetTypes.File )
						{
							StorageAccessor_Remove( storagePath ) ;
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

					if( string.IsNullOrEmpty( storagePath ) == true )
					{
						// 直接ストレージへ保存しないケース(一括)
						instance.FreeLargeReceiveBuffer( receiveBuffer ) ;
					}
					else
					{
						// 直接ストレージへ保存するケース(分割)
						instance.FreeSmallReceiveBuffer( receiveBuffer ) ;
					}

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
	}
}
