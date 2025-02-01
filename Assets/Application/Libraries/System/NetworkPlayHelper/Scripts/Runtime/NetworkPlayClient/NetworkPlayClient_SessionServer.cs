#if UNITY_2019_4_OR_NEWER
#define UNITY
#endif

using System ;
using System.Collections.Generic ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// NetworkPlay 機能のクライアント側の管理用クラス
	/// </summary>
	public partial class NetworkPlayClient
	{
		/// <summary>
		/// セッションサーバーのアドレス
		/// </summary>
		public string	SessionServerAddress
		{
			get
			{
				return m_NetworkPlayClientAdapter.SessionServerAddress ;
			}
		}

		/// <summary>
		/// セッションサーバーのポート
		/// </summary>
		public int		SessionServerPort
		{
			get
			{
				return m_NetworkPlayClientAdapter.SessionServerPort ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッション識別子
		/// </summary>
		public string		SessionId
		{
			get
			{
				return m_NetworkPlayClientAdapter.SessionId ;
			}
		}

		/// <summary>
		/// 最大人数
		/// </summary>
		public int			MaxPlayers
		{
			get
			{
				return m_NetworkPlayClientAdapter.MaxPlayers ;
			}
		}

		//-----------------------------------

		/// <summary>
		/// セッションの管理方法の種別
		/// </summary>
		public SessionManagementTypes ManagementType
		{
			get
			{
				return m_NetworkPlayClientAdapter.ManagementType ;
			}
		}

		/// <summary>
		/// セッションでＵＤＰ通信を有効にするかどうか
		/// </summary>
		public bool UdpEnabled
		{
			get
			{
				return m_NetworkPlayClientAdapter.UdpEnabled ;
			}
		}

		/// <summary>
		/// セッションのＵＤＰ通信で誤り補正を有効にするかどうか
		/// </summary>
		public bool UdpCorrectionEnabled
		{
			get
			{
				return m_NetworkPlayClientAdapter.UdpCorrectionEnabled ;
			}
		}

		//-------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// データの送信受信の準備が整っているかどうか
		/// </summary>
		public bool Ready
		{
			get
			{
				return m_NetworkPlayClientAdapter.Ready ;
			}
		}

		/// <summary>
		/// 自身がホストであるかどうか
		/// </summary>
		public bool IsHost
		{
			get
			{
				return m_NetworkPlayClientAdapter.IsHost ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルループバックを有効にするかどうか
		/// </summary>
		public bool		LocalLoopbackEnabled
		{
			get
			{
				return m_NetworkPlayClientAdapter.LocalLoopbackEnabled ;
			}
			set
			{
				m_NetworkPlayClientAdapter.LocalLoopbackEnabled = value ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// プレイヤー名
		/// </summary>
		public string	PlayerName
		{
			get
			{
				return m_NetworkPlayClientAdapter.PlayerName ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 全てのセッションプレイヤーを取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer[] GetSessionPlayers()
		{
			return m_NetworkPlayClientAdapter.GetSessionPlayers() ;
		}

		/// <summary>
		/// セッションのホストプレイヤーを取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer GetSessionHostPlayer()
		{
			return m_NetworkPlayClientAdapter.GetSessionHostPlayer() ;
		}

		/// <summary>
		/// セッションプレイヤーを取得する
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public SessionPlayer GetSessionPlayer( string userId )
		{
			return m_NetworkPlayClientAdapter.GetSessionPlayer( userId ) ;
		}

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType )
		{
			m_NetworkPlayClientAdapter.SetReceivingCallbackType( receivingCallbackType ) ;
		}

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue()
		{
			return m_NetworkPlayClientAdapter.Dequeue() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// データを送信する(外部からのアクセス)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public bool Send
		(
			byte[] data,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
		{
			return m_NetworkPlayClientAdapter.Send( data, destinationType, destinationUserIds ) ;
		}

		/// <summary>
		/// データを送信する(外部からのアクセス)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public bool Send
		(
			PacketTypes packetType,
			byte[] data,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
		{
			return m_NetworkPlayClientAdapter.Send( packetType, data, destinationType, destinationUserIds ) ;
		}

		/// <summary>
		/// 対象プレイヤーをキックする(ホストのみ可能)
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool Kick( string userId )
		{
			return m_NetworkPlayClientAdapter.Kick( userId ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションサーバーに接続された際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onConnected"></param>
		public void SetOnConnected( Action onConnected )
		{
			m_NetworkPlayClientAdapter.SetOnConnected( onConnected ) ;
		}

		/// <summary>
		/// フレーム受信時に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnReceived( Action<byte[],SourceTypes,string> onReceived )
		{
			m_NetworkPlayClientAdapter.SetOnReceived( onReceived ) ;
		}

		/// <summary>
		/// フレーム受信時に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnPlyerChanged( Action<SessionPlayer> onPlayerJoined, Action<SessionPlayer> onPlayerLeft )
		{
			m_NetworkPlayClientAdapter.SetOnPlyerChanged( onPlayerJoined, onPlayerLeft ) ;
		}

		/// <summary>
		/// セッションサーバーから切断された際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onDisconnected"></param>
		public void SetOnDisconnected( Action onDisconnected )
		{
			m_NetworkPlayClientAdapter.SetOnDisconnected( onDisconnected ) ;
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションから離脱する
		/// </summary>
		public void LeaveFromSession()
		{
			m_NetworkPlayClientAdapter.LeaveFromSession() ;
		}
	}
}
