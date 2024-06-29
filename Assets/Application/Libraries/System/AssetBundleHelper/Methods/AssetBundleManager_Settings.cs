using System ;
using System.Collections.Generic ;
using System.IO ;
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
		/// ダウンロードしたアセットバンドルファイルを保存するルートフォルダ名
		/// </summary>
		public static string DataPath
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DataPath ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DataPath = value ;
				}
			}
		}

		[SerializeField,Header( "ストレージ内の保存対象パス" )]
		private string m_DataPath = "AssetBundleCache" ;

		//---------------

		/// <summary>
		/// 保存されたアセットバンドルファイルのパスをハッシュ化して隠蔽するかどうか
		/// </summary>
		public static bool SecurityEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_SecurityEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_SecurityEnabled = value ;
				}
			}
		}

		[SerializeField,Header( "各種情報を暗号化するかどうか" )]
		private bool m_SecurityEnabled = false ;

		//---------------

		/// <summary>
		/// 非同期版のロードを行う際に通信以外処理を全て同期で行うかどうか(展開速度は上がるが別のコルーチンの呼び出し頻度が下がる)
		/// </summary>
		public static bool FastLoadEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_FastLoadEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_FastLoadEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header( "非同期ロード実行時に通信以外を全て同期で行うかどうか" )]
		private bool m_FastLoadEnabled = true ;

		//---------------

		/// <summary>
		/// ストレージへの書き込みを非同期で行うかどうか
		/// </summary>
		public static bool AsynchronousWritingEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_AsynchronousWritingEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_AsynchronousWritingEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header( "ストレージへの書き込みを非同期で行うかどうか" )]
		private bool m_AsynchronousWritingEnabled = true ;

		//---------------

		/// <summary>
		/// ストレージへの書き込みをダウンロードしながら直接行う事を全マニフェストで許可するか
		/// </summary>
		public static bool DirectSaveEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_DirectSaveEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DirectSaveEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header( "ストレージへの書き込みをダウンロードしながら直接行う事を全マニフェストで許可するか" )]
		private bool m_DirectSaveEnabled = true ;

		//---------------

		/// <summary>
		/// アセットバンドルマネージャ起動と同時にマニフェストをロードするかどうかを示す
		/// </summary>
		public static bool LoadManifestOnAwake
		{
			get
			{
				return m_Instance != null && m_Instance.m_LoadManifestOnAwake ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LoadManifestOnAwake = value ;
				}
			}
		}
		
		[SerializeField,Header( "AssetBundleManager起動と同時に自動的にマニフェストのダウンロードを行うかどうか" )]
		private bool m_LoadManifestOnAwake = false ;


		//-----------------------------------

		/// <summary>
		/// 通信プロトコルが HTTP/1.1 の場合の最大並列ダウンロード数
		/// </summary>
		public static int MaxParallelOfHttp1
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}
				return m_Instance.m_MaxParallelOfHttp1 ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_MaxParallelOfHttp1 = value ;
				}
			}
		}
		
		[SerializeField,Header( "通信プロトコルが HTTP/1.1 の場合の最大並列ダウンロード数" )]
		private int m_MaxParallelOfHttp1 =  6 ;

		/// <summary>
		/// 通信プロトコルが HTTP/2.0 の場合の最大並列ダウンロード数
		/// </summary>
		public static int MaxParallelOfHttp2
		{
			get
			{
				if( m_Instance == null )
				{
					return 0 ;
				}
				return m_Instance.m_MaxParallelOfHttp2 ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_MaxParallelOfHttp2 = value ;
				}
			}
		}
		
		[SerializeField,Header( "通信プロトコルが HTTP/2.0 の場合の最大並列ダウンロード数" )]
		private int m_MaxParallelOfHttp2 = 24 ;

		//-----------------------------------

		/// <summary>
		/// ローカルアセットを使うかどうか(デバッグ用)
		/// </summary>
		public static bool UseLocalAssets
		{
			get
			{
#if UNITY_EDITOR
				return m_Instance != null && m_Instance.m_UseLocalAssets ;
#else
				return false ;
#endif
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseLocalAssets = value ;
				}
			}
		}
		
		[SerializeField,Header( "ローカルアセットを使用するかどうか(Eitor専用)" )]
		private bool m_UseLocalAssets = true ;

		//-----------------------------------

		/// <summary>
		/// マニフェストとパスの区切り記号
		/// </summary>
		public static string ManifestSeparator
		{
			get
			{
				return m_Instance != null ? m_Instance.m_ManifestSeparator : null ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_ManifestSeparator = value ;
				}
			}
		}
		
		[SerializeField,Header( "マニフェストとパスの区切り記号" )]
		private string m_ManifestSeparator = "|" ;

		//-----------------------------------

		/// <summary>
		/// デフォルトのマニフェスト(無指定の場合はパスの最初のフォルダ名がマニフェスト名とみなされる)
		/// </summary>
		public static string DefaultManifestName
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DefaultManifestName ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DefaultManifestName = value ;
				}
			}
		}
		
		[SerializeField,Header( "デフォルトのマニフェスト名(マニフェスト名省略時の対象)" )]
		private string m_DefaultManifestName = "" ;

		//-----------------------------------------------------------------
		
		/// <summary>
		/// 全マニフェスト情報
		/// </summary>
		[SerializeField,Header("【マニフェスト情報】")]
		private List<ManifestInfo> m_ManifestInfo = null ;


		//-------------------------------------------------------------------------------------------

		// 初期の受信バッファサイズ(一括)
		private const int m_DefaultLargeReceiveBufferSize = 1024 * 1024 * 16 ;

		// 最小の受信バッファサイズ(一括)
		private const int m_MinimumLargeReceiveBufferSize = 1024 * 1024 *  4 ;

		/// <summary>
		/// 受信バッファのサイズ(一括)[並列それぞれのサイズ]
		/// </summary>
		public static int LargeReceiveBufferSize
		{
			get
			{
				return m_Instance == null ? 0 : m_Instance.m_LargeReceiveBufferSize ;
			}
			set
			{
				if( m_Instance != null )
				{
					int largeReceiveBufferSize = value ;
					if( largeReceiveBufferSize <  m_MinimumLargeReceiveBufferSize )
					{
						largeReceiveBufferSize  = m_MinimumLargeReceiveBufferSize ;
					}

					m_Instance.m_LargeReceiveBufferSize = largeReceiveBufferSize ;
				}
			}
		}

		[SerializeField,Header( "受信バッファのサイズ(一括)[並列それぞれのサイズ]" )]
		private int m_LargeReceiveBufferSize = m_DefaultLargeReceiveBufferSize ;

		//---------------

		// 初期の受信バッファサイズ(分割)
		private const int m_DefaultSmallReceiveBufferSize = 1024 * 1024 *  1 ;

		// 最小の受信バッファサイズ(分割)
		private const int m_MinimumSmallReceiveBufferSize = 1024 ;

		/// <summary>
		/// 受信バッファのサイズ(分割)[並列それぞれのサイズ]
		/// </summary>
		public static int SmallReceiveBufferSize
		{
			get
			{
				return m_Instance == null ? 0 : m_Instance.m_SmallReceiveBufferSize ;
			}
			set
			{
				if( m_Instance != null )
				{
					int smallReceiveBufferSize = value ;
					if( smallReceiveBufferSize <  m_MinimumSmallReceiveBufferSize )
					{
						smallReceiveBufferSize  = m_MinimumSmallReceiveBufferSize ;
					}

					m_Instance.m_SmallReceiveBufferSize = smallReceiveBufferSize ;
				}
			}
		}

		[SerializeField,Header( "受信バッファのサイズ(分割)[並列それぞれのサイズ]" )]
		private int m_SmallReceiveBufferSize = m_DefaultSmallReceiveBufferSize ;

		//-----------------------------------

		// 受信バッファ構造体
		public class ReceiveBufferStructure
		{
			public bool		IsUsing ;
			public byte[]	ReceiveBuffer ;
		}

		//-----------------------------------
		// 大きいバッファ

		private List<ReceiveBufferStructure>	m_LargeReceiveBufferCache ;
		private List<ReceiveBufferStructure>	m_LargeRemoveTargets ;

		/// <summary>
		/// 受信バッファ(大)キャッシュを生成する
		/// </summary>
		public void CreateLargeReceiveBufferCache()
		{
			m_LargeReceiveBufferCache	= new () ;
			m_LargeRemoveTargets		= new () ;
		}

		/// <summary>
		/// 受信バッファ(大)キャッシュを破棄する
		/// </summary>
		public void DeleteLargeReceiveBufferCache()
		{
			m_LargeRemoveTargets		= null ;
			m_LargeReceiveBufferCache	= null ;
		}

		/// <summary>
		/// 使用していない受信バッファ(大)を破棄する
		/// </summary>
		/// <param name="isForce"></param>
		public void ClearLargeReceiveBufferCache( bool isForce = false )
		{
			if( m_LargeReceiveBufferCache == null || m_LargeReceiveBufferCache.Count == 0 )
			{
				// 不要
				return ;
			}

			if( isForce == true )
			{
				// 全て強制破棄
				m_LargeReceiveBufferCache.Clear() ;
				return ;
			}

			//----------------------------------

			int i, l = m_LargeReceiveBufferCache.Count ;

			// 破棄対象
			m_LargeRemoveTargets.Clear() ;

			//----------------------------------

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_LargeReceiveBufferCache[ i ].IsUsing == false )
				{
					// 破棄対象に追加する
					m_LargeRemoveTargets.Add( m_LargeReceiveBufferCache[ i ] ) ;
				}
			}

			//----------------------------------

			if( m_LargeRemoveTargets.Count >  0 )
			{
				// 破棄対象が存在するため破棄する
				foreach( var removeTarget in m_LargeRemoveTargets )
				{
					m_LargeReceiveBufferCache.Remove( removeTarget ) ;
				}

				m_LargeRemoveTargets.Clear() ;
			}
		}

		/// <summary>
		/// 受信バッファ(大)を生成または取得する
		/// </summary>
		/// <returns></returns>
		public byte[] KeepLargeReceiveBuffer()
		{
			int i, l = m_LargeReceiveBufferCache.Count ;

			// 破棄対象
			m_LargeRemoveTargets.Clear() ;

			//----------------------------------

			byte[] receiveBuffer = null ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_LargeReceiveBufferCache[ i ].IsUsing == false )
				{
					// 空いているバッファがある
					if( m_LargeReceiveBufferCache[ i ].ReceiveBuffer != null && m_LargeReceiveBufferCache[ i ].ReceiveBuffer.Length != m_LargeReceiveBufferSize )
					{
						// サイズ的に設定の基準を満たす
						m_LargeReceiveBufferCache[ i ].IsUsing	= true ;	// 確保状態とする
						receiveBuffer							= m_LargeReceiveBufferCache[ i ].ReceiveBuffer ;
						break ;
					}
					else
					{
						// サイズ的に設定の基準を満たさないので破棄する
						m_LargeReceiveBufferCache[ i ].ReceiveBuffer = null ;

						// 破棄対象に追加する
						m_LargeRemoveTargets.Add( m_LargeReceiveBufferCache[ i ] ) ;
					}
				}
			}

			//----------------------------------

			if( m_LargeRemoveTargets.Count >  0 )
			{
				// 破棄対象が存在するため破棄する
				foreach( var removeTarget in m_LargeRemoveTargets )
				{
					m_LargeReceiveBufferCache.Remove( removeTarget ) ;
				}

				m_LargeRemoveTargets.Clear() ;
			}

			//----------------------------------

			if( receiveBuffer == null )
			{
				// 確保できなかったので新たに生成する
				receiveBuffer = new byte[ m_LargeReceiveBufferSize ] ;

				m_LargeReceiveBufferCache.Add( new ()
				{
					IsUsing			= true,
					ReceiveBuffer	= receiveBuffer
				} ) ;
			}

			//----------------------------------------------------------
#if UNITY_EDITOR
			if( receiveBuffer == null )
			{
				Debug.LogWarning( "Large receive buffer could not allocated." ) ;
			}
#endif
			return receiveBuffer ;
		}

		/// <summary>
		/// 受信バッファ(大)を解放する
		/// </summary>
		/// <param name="receiveBuffer"></param>
		public void FreeLargeReceiveBuffer( byte[] receiveBuffer )
		{
			int i, l = m_LargeReceiveBufferCache.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_LargeReceiveBufferCache[ i ].ReceiveBuffer == receiveBuffer )
				{
					m_LargeReceiveBufferCache[ i ].IsUsing = false ;	// 解放状態とする
					break ;
				}
			}
		}

		/// <summary>
		/// 使用中のバッファ(大)の数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetUsingLargeReceiveBufferCount()
		{
			int i, l = m_LargeReceiveBufferCache.Count ;
			int count = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_LargeReceiveBufferCache[ i ].IsUsing == true )
				{
					count ++ ;
				}
			}

			return count ;
		}

		//-----------------------------------
		// 小さいバッファ

		private List<ReceiveBufferStructure>	m_SmallReceiveBufferCache ;
		private List<ReceiveBufferStructure>	m_SmallRemoveTargets ;

		/// <summary>
		/// 受信バッファ(小)キャッシュを生成する
		/// </summary>
		public void CreateSmallReceiveBufferCache()
		{
			m_SmallReceiveBufferCache	= new () ;
			m_SmallRemoveTargets		= new () ;
		}

		/// <summary>
		/// 受信バッファ(小)キャッシュを破棄する
		/// </summary>
		public void DeleteSmallReceiveBufferCache()
		{
			m_SmallRemoveTargets		= null ;
			m_SmallReceiveBufferCache	= null ;
		}

		/// <summary>
		/// 使用していない受信バッファ(小)を破棄する
		/// </summary>
		/// <param name="isForce"></param>
		public void ClearSmallReceiveBufferCache( bool isForce = false )
		{
			if( m_SmallReceiveBufferCache == null || m_SmallReceiveBufferCache.Count == 0 )
			{
				// 不要
				return ;
			}

			if( isForce == true )
			{
				// 全て強制破棄
				m_SmallReceiveBufferCache.Clear() ;
				return ;
			}

			//----------------------------------

			int i, l = m_SmallReceiveBufferCache.Count ;

			// 破棄対象
			m_SmallRemoveTargets.Clear() ;

			//----------------------------------

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SmallReceiveBufferCache[ i ].IsUsing == false )
				{
					// 破棄対象に追加する
					m_SmallRemoveTargets.Add( m_SmallReceiveBufferCache[ i ] ) ;
				}
			}

			//----------------------------------

			if( m_SmallRemoveTargets.Count >  0 )
			{
				// 破棄対象が存在するため破棄する
				foreach( var removeTarget in m_SmallRemoveTargets )
				{
					m_SmallReceiveBufferCache.Remove( removeTarget ) ;
				}

				m_SmallRemoveTargets.Clear() ;
			}
		}

		/// <summary>
		/// 受信バッファ(小)を生成または取得する
		/// </summary>
		/// <returns></returns>
		public byte[] KeepSmallReceiveBuffer()
		{
			int i, l = m_SmallReceiveBufferCache.Count ;

			// 破棄対象
			m_SmallRemoveTargets.Clear() ;

			//----------------------------------

			byte[] receiveBuffer = null ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SmallReceiveBufferCache[ i ].IsUsing == false )
				{
					// 空いているバッファがある
					if( m_SmallReceiveBufferCache[ i ].ReceiveBuffer != null && m_SmallReceiveBufferCache[ i ].ReceiveBuffer.Length != m_SmallReceiveBufferSize )
					{
						// サイズ的に設定の基準を満たす
						m_SmallReceiveBufferCache[ i ].IsUsing	= true ;	// 確保状態とする
						receiveBuffer							= m_SmallReceiveBufferCache[ i ].ReceiveBuffer ;
						break ;
					}
					else
					{
						// サイズ的に設定の基準を満たさないので破棄する
						m_SmallReceiveBufferCache[ i ].ReceiveBuffer = null ;

						// 破棄対象に追加する
						m_SmallRemoveTargets.Add( m_SmallReceiveBufferCache[ i ] ) ;
					}
				}
			}

			//----------------------------------

			if( m_SmallRemoveTargets.Count >  0 )
			{
				// 破棄対象が存在するため破棄する
				foreach( var removeTarget in m_SmallRemoveTargets )
				{
					m_SmallReceiveBufferCache.Remove( removeTarget ) ;
				}

				m_SmallRemoveTargets.Clear() ;
			}

			//----------------------------------

			if( receiveBuffer == null )
			{
				// 確保できなかったので新たに生成する
				receiveBuffer = new byte[ m_SmallReceiveBufferSize ] ;

				m_SmallReceiveBufferCache.Add( new ()
				{
					IsUsing			= true,
					ReceiveBuffer	= receiveBuffer
				} ) ;
			}

			//----------------------------------------------------------
#if UNITY_EDITOR
			if( receiveBuffer == null )
			{
				Debug.LogWarning( "Small receive buffer could not allocated." ) ;
			}
#endif
			return receiveBuffer ;
		}

		/// <summary>
		/// 受信バッファ(小)を解放する
		/// </summary>
		/// <param name="receiveBuffer"></param>
		public void FreeSmallReceiveBuffer( byte[] receiveBuffer )
		{
			int i, l = m_SmallReceiveBufferCache.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SmallReceiveBufferCache[ i ].ReceiveBuffer == receiveBuffer )
				{
					m_SmallReceiveBufferCache[ i ].IsUsing = false ;	// 解放状態とする
					break ;
				}
			}
		}

		/// <summary>
		/// 使用中のバッファ(小)の数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetUsingSmallReceiveBufferCount()
		{
			int i, l = m_SmallReceiveBufferCache.Count ;
			int count = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SmallReceiveBufferCache[ i ].IsUsing == true )
				{
					count ++ ;
				}
			}

			return count ;
		}

		//-------------------------------------------------------------------------------------------

		// 緊急停止の読んで欲しいコールバックの登録
		private readonly List<Action> m_OnQuitCallbacks = new () ;

		/// <summary>
		///  AssetBundleManager 停止時に呼んで欲しいコールバックを登録する
		/// </summary>
		/// <param name="onQuit"></param>
		public void AddOnQuitCallback( Action onQuit )
		{
			m_OnQuitCallbacks.Add( onQuit ) ;
		}

		/// <summary>
		///  AssetBundleManager 停止時に呼んで欲しいコールバックを解除する
		/// </summary>
		/// <param name="onQuit"></param>
		public void RemoveOnQuitCallback( Action onQuit )
		{
			m_OnQuitCallbacks.Remove( onQuit ) ;
		}

		// 終了時に呼んで欲しいコールバックを呼び出す
		private void CallOnQuitCallbacks()
		{
			if( m_OnQuitCallbacks != null && m_OnQuitCallbacks.Count >  0 )
			{
				foreach( var onQuit in m_OnQuitCallbacks )
				{
					onQuit?.Invoke() ;
				}

				m_OnQuitCallbacks.Clear() ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ダウンロード時に追加したい HTTP ヘッダ情報

		private readonly Dictionary<string,string>	m_ConstantHeaders = new () ;

#if UNITY_EDITOR
		/// <summary>
		/// 任意追加のヘッダー
		/// </summary>
		[Serializable]
		public class ConstantHeader
		{
			public string Key ;
			public string Value ;
		}

		private readonly List<ConstantHeader>	m_ConstantHeaers_Monitor = new () ;
#endif

		/// <summary>
		/// HTTP ヘッダに設定する情報を追加する
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool AddHeader( string key, string value )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.AddHeader_Private( key, value ) ;
		}

		// HTTP ヘッダに設定する情報を追加する
		private bool AddHeader_Private( string key, string value )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_ConstantHeaders.ContainsKey( key ) == false )
			{
				// 新規
				if( string.IsNullOrEmpty( value ) == false )
				{
					// 追加
					m_ConstantHeaders.Add( key, value ) ;
#if UNITY_EDITOR

					m_ConstantHeaers_Monitor.Add( new (){ Key = key, Value = value } ) ;
#endif
				}
			}
			else
			{
				// 既存
				if( string.IsNullOrEmpty( value ) == false )
				{
					// 上書
					m_ConstantHeaders[ key ] = value ;
#if UNITY_EDITOR
					var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
					if( record != null )
					{
						record.Value = value ;
					}
#endif
				}
				else
				{
					// 削除
					m_ConstantHeaders.Remove( key ) ;

#if UNITY_EDITOR
					var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
					if( record != null )
					{
						m_ConstantHeaers_Monitor.Remove( record ) ;
					}
#endif
				}
			}

			return true ;
		}

		/// <summary>
		/// HTTP ヘッダに設定する情報を削除する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool RemoveHeader( string key )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.RemoveHeader_Private( key ) ;
		}

		// HTTP ヘッダに設定する情報を削除する
		private bool RemoveHeader_Private( string key )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				return false ;
			}

			if( m_ConstantHeaders.ContainsKey( key ) == true )
			{
				// 既存

				// 削除
				m_ConstantHeaders.Remove( key ) ;

#if UNITY_EDITOR
				var record = m_ConstantHeaers_Monitor.FirstOrDefault( _ => _.Key == key ) ;
				if( record != null )
				{
					m_ConstantHeaers_Monitor.Remove( record ) ;
				}
#endif
			}

			return true ;
		}

		/// <summary>
		/// HTTP ヘッダに設定する情報を削除する
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool RemoveAllHeaders()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.RemoveAllHeaders_Private() ;
		}

		// HTTP ヘッダに設定する情報を削除する
		private bool RemoveAllHeaders_Private()
		{
			m_ConstantHeaders.Clear() ;
#if UNITY_EDITOR
			m_ConstantHeaers_Monitor.Clear() ;
#endif
			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// ローカルアセットを使うかどうか(デバッグ用)
		/// </summary>
		public static bool LogEnabled
		{
			get
			{
				return m_Instance != null && m_Instance.m_LogEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LogEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header( "ログを出力するかどうか" )]
		private bool m_LogEnabled = true ;

		//-----------------------------------------------------------

		/// <summary>
		/// 処理を実行中かどうか
		/// </summary>
		[SerializeField][NonSerialized]
		private int m_AsyncProcessingCount ;


		//-------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
			/// <summary>
			/// デバッグ用のキャッシュ中のリソースの表示リスト
			/// </summary>
			[SerializeField,Header( "【リソースキャッシュ情報】" )]
			private List<ResourceCacheElement>	m_ResourceCacheViewer = null ;
#endif

		//-------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR

		// 使用するアセットバンドルを記録する
		private bool	m_IsRecording ;

		/// <summary>
		/// 使用するアセットバンドルの情報
		/// </summary>
		[Serializable]
		public class UsingAssetBundleInfo
		{
			/// <summary>
			/// マニフェスト名
			/// </summary>
			public string	ManifestName ;

			/// <summary>
			/// アセットバンドルのパス
			/// </summary>
			public string	AssetBundlePath ;

			/// <summary>
			/// ファイルサイズ
			/// </summary>
			public long		AssetBundleSize ;

			/// <summary>
			/// 参照された回数
			/// </summary>
			public int		ReferencedCount ;
		}

		[SerializeField][Header( "使用対象のアセットバンドル記録(レコーディング用)" )]
		private List<UsingAssetBundleInfo>								m_UsingAssetBundles		 = new () ;
		private Dictionary<( string, string ), UsingAssetBundleInfo>	m_UsingAssetBundles_Hash = new () ;

		/// <summary>
		/// 現在使用するアセットバンドル情報を記録中かどうか
		/// </summary>
		public static bool IsRecording
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsRecording ;
			}
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を開始する
		/// </summary>
		/// <returns></returns>
		public static bool StartRecording()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StartRecording_Private() ;
		}

		// 使用するアセットバンドルの記録を開始する
		private bool StartRecording_Private()
		{
			if( m_IsRecording == true )
			{
				Debug.Log( "<color=#00FF7F>[AssetBundleManager] Already started recording</color>" ) ;
			}

			//----------------------------------------------------------

			m_IsRecording = true ;

			m_UsingAssetBundles.Clear() ;
			m_UsingAssetBundles_Hash.Clear() ;

			Debug.Log( "<color=#00FF7F>[AssetBundleManager] StartRecording()</color>" ) ;

			return true ;
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を終了する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string[] StopRecording( string path, string platformName, string separator = "," )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.StopRecording_Private( path, platformName, separator ) ;
		}

		// 使用するアセットバンドルの記録を終了する
		private string[] StopRecording_Private( string path, string platformName, string separator )
		{
			if( m_IsRecording == false )
			{
				Debug.LogWarning( "<color=#00FF7F>[AssetBundleManager] Not start recording</color>" ) ;
				return null ;
			}

			//----------------------------------------------------------

			m_IsRecording = false ;

			//----------------------------------------------------------

			if( m_UsingAssetBundles.Count >  0 )
			{
				// 記録された情報がある

				// 情報をソートする
				m_UsingAssetBundles = m_UsingAssetBundles.OrderBy( _ => _.ManifestName ).ThenBy( _ => _.AssetBundlePath ).ToList() ;

				//----------------------------------------------------------
				// ファイルを書き出す

				long totalSize = 0 ;

				foreach( var record in m_UsingAssetBundles )
				{
					totalSize += record.AssetBundleSize ;
				}

				if( string.IsNullOrEmpty( separator ) == true )
				{
					separator = "," ;
				}

				//--------------

				string text = string.Empty ;

				// Platform
				text += $"# Platform = {platformName}\n" ;

				// TotalFile
				text += $"# TotalFile = {m_UsingAssetBundles.Count}\n" ;

				// TotalSize
				text += $"# TotalSize = {totalSize} byte" ;
				if( totalSize >= 1024 )
				{
					text += $" ( {GetSizeName( totalSize )} )" ;
				}
				text += "\n" ;

				foreach( var record in m_UsingAssetBundles )
				{
					text += $"{record.ManifestName}{separator}{record.AssetBundlePath}{separator}{record.AssetBundleSize}{separator}{record.ReferencedCount}\n" ;
				}

				// 保存
				File.WriteAllText( path, text ) ;

				//--------------------------------------------------------------------------
				// プラットフォーム関係無しのパス情報のみを返す

				var assetBundlePaths = new List<string>() ;

				foreach( var record in m_UsingAssetBundles )
				{
					assetBundlePaths.Add( $"{record.ManifestName}/{record.AssetBundlePath}" ) ;
				}

				//----------------------------------

				Debug.Log( "<color=#00FF7F>[AssetBundleManager] StopRecording() : TotalFile = " + m_UsingAssetBundles.Count + " TotalSize = " + GetSizeName( totalSize ) + " > " + path + "</color>" ) ;

				m_UsingAssetBundles.Clear() ;
				m_UsingAssetBundles_Hash.Clear() ;

				//----------------------------------------------------------

				return assetBundlePaths.ToArray() ;
			}
			else
			{
				// 記録された情報がない

				Debug.Log( "<color=#00FF7F>[AssetBundleManager] StopRecording() : Record is empty.</color>" ) ;

				return null ;
			}
		}

		/// <summary>
		/// 使用するアセットバンドルの記録を中断する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool CancelRecording()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.CancelRecording_Private() ;
			return true ;
		}

		// 使用するアセットバンドルの記録を中断する
		private bool CancelRecording_Private()
		{
			if( m_IsRecording == false )
			{
				return true ;
			}

			//----------------------------------

			m_IsRecording  = false ;

			m_UsingAssetBundles.Clear() ;
			m_UsingAssetBundles_Hash.Clear() ;

			Debug.Log( "<color=#00FF7F>[AssetBundleManager] Recording canceled</color>" ) ;

			return true ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// 使用するアセットバンドル情報を記録する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="assetBundlePath"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static bool RecordUsingAssetBundle( string manifestName, string assetBundlePath, int assetBundleSize )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.RecordUsingAssetBundle_Private( manifestName, assetBundlePath, assetBundleSize ) ;

			return true ;
		}
#endif
		// 使用するアセットバンドル情報を記録する
		private void RecordUsingAssetBundle_Private( string manifestName, string assetBundlePath, long assetBundleSize )
		{
			var key = ( manifestName, assetBundlePath ) ;

			// 既に記録済みか判定する
			if( m_UsingAssetBundles_Hash.ContainsKey( key ) == false )
			{
				// 記録はされていない
				var record = new UsingAssetBundleInfo()
				{
					ManifestName	= manifestName,
					AssetBundlePath	= assetBundlePath,
					AssetBundleSize	= assetBundleSize,
					ReferencedCount	= 1
				} ;

				m_UsingAssetBundles.Add( record ) ;
				m_UsingAssetBundles_Hash.Add( key, record ) ;
			}
			else
			{
				// 記録はされている
				m_UsingAssetBundles_Hash[ key ].ReferencedCount ++ ;	// 参照数のみ増加させる
			}
		}
#endif
	}
}
