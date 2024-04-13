using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Net ;
using System.Net.Sockets ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using System.Text ;


namespace DSW
{
	/// <summary>
	/// サーバーを検知するためのクラス Version 2024/04/13
	/// </summary>
	public class ServerDetactor : ExMonoBehaviour
	{
		// 自身のシングルトンインスタンス
		private static ServerDetactor m_Instance ;

		//-----------------------------------------------------------

		[SerializeField]
		protected int		m_ServerDirectorPort ;

		[SerializeField]
		protected int		m_ClientPort ;

		[SerializeField]
		protected string	m_ClientName ;

		[SerializeField]
		protected float		m_IntervalTime ;

		[SerializeField]
		protected bool		m_RunInBackground ;

		//---------------

		[Serializable]
		public class ServerEntity
		{
			public string	IpAddress ;

			public int		Port ;
			public string	Name ;

			public float	LastUpdateTime ;
		}

		[SerializeField]
		protected List<ServerEntity> m_ServerEntities ;

		/// <summary>
		/// 検知されたサーバー情報
		/// </summary>
		public List<ServerEntity> ServerEntities => m_ServerEntities ;


		[SerializeField]
		private bool		m_IsSuspending = false ;

		//-----------------------------------------------------------

		// クライアントソケット(UDP)
		private UdpClient m_UdpClient ;

		// Stopwatch(UnityEngine.Time の代わり。サブスレッド内で UnityEngine.Time を使うと死ぬ) 
		private System.Diagnostics.Stopwatch m_Stopwatch ;

		// サーバーに応答要求パケットを出すまでの残り時間を計る
		private float m_Timer ;

		// タイムオーバーで不要になるサーバー情報対象
		private readonly List<ServerEntity>	m_InvalidServerEntitis = new () ;

		// サーバーに送るパケットデータ(固定内容)
		private byte[] m_PacketData ;

		// コールバック登録
		protected Action<List<ServerEntity>> m_Callback ;

		private bool m_IsRunning = false ;

		/// <summary>
		/// 処理を実行中かどうか
		/// </summary>
		public static bool IsRunning
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsRunning ;
			}
		}

		/// <summary>
		/// サーバーのポート番号
		/// </summary>
		public static int ServerDitectorPort
		{
			get
			{
				if( m_Instance == null )
				{
					return -1 ;
				}

				return m_Instance.m_ServerDirectorPort ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_ServerDirectorPort = value ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// サーバー検知機構を生成する
		/// </summary>
		/// <param name="serverPort"></param>
		/// <param name="parent"></param>
		public static void Create( int serverDitectorPort, string clientName = null, float intervalTime = 3, bool runInBackground = true, Transform parent = null, bool isSuspending = false )
		{
			if( m_Instance != null )
			{
				// 既に起動中
				return ;
			}

			//----------------------------------

			// GameObject 生成
			var go = new GameObject( "ServerDetector" ) ;

			if( parent == null )
			{
				// 親の指定は無し

				// 常駐する
				DontDestroyOnLoad( go ) ;
			}
			else
			{
				// 親の指定は有り

				// 親を設定する
				go.transform.SetParent( parent, false ) ;
#if UNITY_EDITOR
				// 親が常駐状態でなければ警告を出す
				if( parent.gameObject.scene.name != "DontDestroyOnLoad" )
				{
					Debug.LogWarning( "Parent gameObject is not DontDestroyOnLoad." ) ;
				}
#endif
			}

			// インスタンス生成
			m_Instance = go.AddComponent<ServerDetactor>() ;

			//----------------------------------
			// 各種設定情報を保存する

			if( string.IsNullOrEmpty( clientName ) == true )
			{
				// 最も文字列が少ないアドレスを選択する(IPv4)
				int length = 0x7FFFFF ;

				// クライアント名が省略された場合はクライアントのＩＰアドレスを名前とする
				var addressNames = GetLocalAddress() ;
				if( addressNames != null && addressNames.Length >  0 )
				{
					foreach( var addressName in addressNames )
					{
						if( addressName.Length <  length )
						{
							clientName = addressName ;
							length = addressName.Length ;
						}
					}
				}

				if( string.IsNullOrEmpty( clientName ) == true )
				{
					// アドレスが全く取得出来なかった場合の名前
					clientName = "Unknown" ;
				}
			}

			// サーバー検出パケットの発行は、最低 6 秒は空ける
			if( intervalTime <  1 )
			{
				intervalTime  = 1 ;
			}

			//--------------

			m_Instance.m_ServerDirectorPort	= serverDitectorPort ;

			m_Instance.m_ClientName			= clientName ;
			m_Instance.m_IntervalTime		= intervalTime ;

			m_Instance.m_RunInBackground	= runInBackground ;

			m_Instance.m_IsSuspending		= isSuspending ;
		}

		/// <summary>
		/// 一時停止
		/// </summary>
		public static void Suspend()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Suspend_Private() ;
		}

		/// <summary>
		/// 再開
		/// </summary>
		public static void Resume()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.Resume_Private() ;
		}

		/// <summary>
		/// 破棄する
		/// </summary>
		public static void Delete()
		{
			if( m_Instance == null )
			{
				return ;
			}

			GameObject.Destroy( m_Instance.gameObject ) ;
		}

		//---------------

		/// <summary>
		/// 一時停止中かどうか
		/// </summary>
		public static bool IsSuspending
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_IsSuspending ;
			}
		}

		//-----------------------------------------------------------


		// 待ち受けを起動する
		internal void Start()
		{
			// 送信用のパケット情報
			var data = new List<byte>() ;
			PutString( m_ClientName, in data ) ;

			m_PacketData = data.ToArray() ;

			//--------------

			m_ServerEntities = new List<ServerEntity>() ;

			// ストップウォッチ作成(UnityEngine.Time はスレッドセーフではない)
			m_Stopwatch = new System.Diagnostics.Stopwatch() ;
			m_Stopwatch.Start() ;

			// 最初は待ち時間無しでサーバー検出パケットを発行する
			m_Timer = 0 ;

			//----------------------------------

			// ＵＤＰクライアントを生成する
			m_UdpClient = new UdpClient( 0 )	// 0 = クライアントのポート番号は自動割り当て
			{
				// ブロードキャスト有効化
				EnableBroadcast = true
			} ;

			// 自動的に割り当てられたクライアントのポート番号
			m_ClientPort = ( ( IPEndPoint )m_UdpClient.Client.LocalEndPoint ).Port ;

			//----------------------------------

			// 処理開始
			m_IsRunning = true ;

			// 待ち受けを開始する
			m_UdpClient.BeginReceive( OnReceived, m_UdpClient ) ;
		}

		// パケットを受信したら呼び出される(サブスレッドで実行されている事に注意:UnityEngineパッケージの機能を使うと死ぬ)
		private void OnReceived( System.IAsyncResult result )
		{
			if( m_IsRunning == false )
			{
				// 処理は既に終了している
				return ;
			}

			//----------------------------------

			UdpClient udpClient = ( UdpClient )result.AsyncState ;
			IPEndPoint endPoint = null ;

			// データを取得する
			byte[] data = udpClient.EndReceive( result, ref endPoint ) ;

//			Debug.Log( "サーバーからの応答があった : " + endPoint.Address.ToString() + " " + endPoint.Port.ToString() ) ;

			if( data != null && data.Length >  0 )
			{
				// サーバーからの応答を受信した

				//-------------

				// サーバーの情報をアンパッキング(展開)する
				int		offset		= 0 ;
				int		serverPort	= GetInt32( in data, ref offset ) ;
				string	serverName	= GetString( in data, ref offset ) ;

				//-------------

				// サーバーの情報を保存する
				string ipAddress = endPoint.Address.ToString() ;

				float lastUpdateTime = GetTime() ;

				var serverEntity = m_ServerEntities.FirstOrDefault( _ => _.IpAddress == ipAddress ) ;

				if( serverEntity == null )
				{
					// 初めて登録されるサーバー(情報を追加する)
					m_ServerEntities.Add( new ServerEntity()
					{
						IpAddress				= ipAddress,

						Port					= serverPort,
						Name					= serverName,

						LastUpdateTime			= lastUpdateTime
					} ) ;
				}
				else
				{
					// 既に登録済みのサーバー(情報を更新する)
					serverEntity.Port			= serverPort ;
					serverEntity.Name			= serverName ;

					serverEntity.LastUpdateTime	= lastUpdateTime ;
				}
			}

			//----------------------------------------------------------

			// 待ち受けを開始する
			m_UdpClient.BeginReceive( OnReceived, m_UdpClient ) ;
		}

		/// <summary>
		/// 毎フレーム呼ばれる
		/// </summary>
		internal void Update()
		{
			if( m_UdpClient != null && m_IsSuspending == false )
			{
				UpdateServerEntities() ;
			}
		}

		// サーバー情報をリフレッシュする
		private void UpdateServerEntities()
		{
			float lastUpdateTime = GetTime() ;
			float limitTime = m_IntervalTime * 10 ;
			float deltaTime ;

			// インターバルタイムの 10 倍の時間応答が無いサーバーの情報を消去する
			if( m_ServerEntities != null && m_ServerEntities.Count >  0 )
			{
				// 次に有効なサーバー情報
				m_InvalidServerEntitis.Clear() ;

				foreach( var serverEntity in m_ServerEntities )
				{
					deltaTime = lastUpdateTime - serverEntity.LastUpdateTime ;
					if( deltaTime >  limitTime )
					{
						// サーバー情報として有効な時間は超過したのでこのサーバー情報は削除対象とする
						m_InvalidServerEntitis.Add( serverEntity ) ;
					}
				}

				if( m_InvalidServerEntitis.Count >  0 )
				{
					// 削除対象が存在する
					m_ServerEntities.RemoveRange( m_InvalidServerEntitis ) ;
				}
			}

			//----------------------------------
			// コールバックが登録されていたら呼び出す

			m_Callback?.Invoke( m_ServerEntities ) ;

			//----------------------------------

			if( m_Timer == 0 || ( lastUpdateTime - m_Timer ) >= m_IntervalTime )
			{
				// パケットを発行する

				m_Timer = lastUpdateTime ;	// 次の発行はインターバルタイム経過後

				m_UdpClient.Send( m_PacketData, m_PacketData.Length, new IPEndPoint( IPAddress.Broadcast, m_ServerDirectorPort ) ) ;
//				Debug.Log( "パケット送信 : " + m_PacketData.Length + " ( " + m_ServerPort + " )" ) ;
			}
		}

		// 一時停止
		private void Suspend_Private()
		{
			// 一時停止
			m_IsSuspending = true ;
		}

		// 再開
		private void Resume_Private()
		{
			// 再開
			m_IsSuspending = false ;
		}

		/// <summary>
		/// フォーカスの状態が変化したら呼び出される
		/// </summary>
		/// <param name="focus"></param>
		internal void OnApplicationFocus( bool focus )
		{
			if( m_RunInBackground == false )
			{
				if( focus == false )
				{
					// フォーカスを失った
					Suspend_Private() ;
				}
				else
				{
					// フォーカスを得た
					Resume_Private() ;
				}
			}
		}

		/// <summary>
		/// コンポーネントが破棄される際に呼び出される
		/// </summary>
		internal void OnDestroy()
		{
			if( m_Instance != this )
			{
				return ;
			}

			// 処理終了
			m_IsRunning = false ;

			//----------------------------------

			if( m_UdpClient != null )
			{
				m_UdpClient.Close() ;
				m_UdpClient.Dispose() ;

				m_UdpClient = null ;
			}

			m_Instance = null ;
		}

		//-----------------------------------------------------------

		// ローカルのＩＰアドレスを取得する
		private static string[] GetLocalAddress()
		{
			string hostName = Dns.GetHostName() ;

			IPAddress[] addresses = Dns.GetHostAddresses( hostName ) ;

			if( addresses == null || addresses.Length == 0 )
			{
				return null ;
			}

			var addressNames = new List<string>() ;

			foreach( var address in addresses )
			{
				addressNames.Add( address.ToString() ) ;
			}

			return addressNames.ToArray() ;
		}

		// 経過時間を取得する
		private float GetTime()
		{
			if( m_Stopwatch == null )
			{
				return 0.0f ;
			}

			return ( float )m_Stopwatch.ElapsedMilliseconds / 1000.0f ;
		}

		//-------------------------------------------------------------------------------------------
		// コールバック登録と解除

		/// <summary>
		/// サーバー情報が更新されたら呼ばれるコールバックを登録する
		/// </summary>
		/// <param name="callback"></param>
		public static void AddCallback( Action<List<ServerEntity>> callback )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.AddCallback_Private( callback ) ;
		}

		// サーバー情報が更新されたら呼ばれるコールバックを登録する
		private void AddCallback_Private( Action<List<ServerEntity>> callback )
		{
			m_Callback -= callback ;
			m_Callback += callback ;
		}

		/// <summary>
		/// サーバー情報が更新されたら呼ばれるコールバックを解除する
		/// </summary>
		/// <param name="callback"></param>
		public static void RemoveCallback( Action<List<ServerEntity>> callback )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.RemoveCallback_Private( callback ) ;
		}

		// サーバー情報が更新されたら呼ばれるコールバックを解除する
		private void RemoveCallback_Private( Action<List<ServerEntity>> callback )
		{
			m_Callback -= callback ;
		}

		//-------------------------------------------------------------------------------------------
#if false
		// ０８ビット整数値を格納する
		private static void PutByte( byte value, in List<byte> data )
		{
			data.Add( value ) ;
		}

		// ０８ビット整数値を取得する
		private static byte GetByte( int byte[] data, ref int offset )
		{
			byte value = data[ offset ] ;

			offset ++ ;

			return value ;
		}
#endif
		// １６ビット整数値を格納する
		private static void PutInt16( short value, in List<byte> data )
		{
			data.Add( ( byte )value ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
		}

		// １６ビット整数値を取得する
		private static int GetInt16( in byte[] data, ref int offset )
		{
			Int16 value = ( Int16 )
			(
				data[ offset ] |
				( ( Int16 )data[ offset + 1 ] <<  8 )
			) ;

			offset += 2 ;

			return value ;
		}

		// ３２ビット整数値を取得する
		private static void PutInt32( int value, in List<byte> data )
		{
			data.Add( ( byte )value ) ;
			data.Add( ( byte )( value >>  8 ) ) ;
			data.Add( ( byte )( value >> 16 ) ) ;
			data.Add( ( byte )( value >> 24 ) ) ;
		}

		// ３２ビット整数値を取得する
		private static int GetInt32( in byte[] data, ref int offset )
		{
			Int32 value = ( Int32 )
			(
				data[ offset ] |
				( ( Int32 )data[ offset + 1 ] <<  8 ) |
				( ( Int32 )data[ offset + 2 ] << 16 ) |
				( ( Int32 )data[ offset + 3 ] << 24 )
			) ;

			offset += 4 ;

			return value ;
		}

		// 文字列を格納する
		private static void PutString( string value, in List<byte> data )
		{
			int length = 0 ;
			byte[] codes = null ;
			if( string.IsNullOrEmpty( value ) == false )
			{
				codes = Encoding.UTF8.GetBytes( value ) ;
				length = codes.Length ;
			}

			PutInt16( ( Int16 )length, in data ) ;

			if( length >  0 && codes != null )
			{
				data.AddRange( codes ) ;
			}
		}

		// 文字列を取得する
		private static string GetString( in byte[] data, ref int offset )
		{
			int length = GetInt16( data, ref offset ) ;
			if( length <= 0 )
			{
				return string.Empty ;
			}

			string value = Encoding.UTF8.GetString( data, offset, length ) ;

			offset += length ;

			return value ;
		}
	}
}
