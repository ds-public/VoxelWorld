#if UNITY_2019_4_OR_NEWER
#define UNITY
#endif

using System ;
using System.Collections.Generic ;

using System.Threading ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// NetworkPlay 機能のクライアント側の管理用クラス
	/// </summary>
	public partial class NetworkPlayClient
	{
		/// <summary>
		/// エクスチェンジサーバーのアドレス
		/// </summary>
		public string	ExchangeServer_Address
			=> m_NetworkPlayClientAdapter.ExchangeServer_Address ;

		/// <summary>
		/// エクスチェンジサーバーのＴＣＰポート
		/// </summary>
		public ushort	ExchangeServer_TcpPort
			=> m_NetworkPlayClientAdapter.ExchangeServer_TcpPort ;

		/// <summary>
		/// エクスチェンジサーバーのＵＤＰポート
		/// </summary>
		public ushort	ExchangeServer_UdpPort
			=> m_NetworkPlayClientAdapter.ExchangeServer_UdpPort ;

		//-----------------------------------

		/// <summary>
		/// セッションに参加しているかどうか
		/// </summary>
		public bool		IsSessionJoined
			=> m_NetworkPlayClientAdapter.IsSessionJoined ;

		/// <summary>
		/// セッション識別子
		/// </summary>
		public ulong		SessionId
			=> m_NetworkPlayClientAdapter.SessionId ;

		/// <summary>
		/// セッションのスコープタイプ
		/// </summary>
		public SessionScopeTypes		SessionScopeType
			=> m_NetworkPlayClientAdapter.SessionScopeType ;

		/// <summary>
		/// 最大人数
		/// </summary>
		public int			MaxPlayers
			=> m_NetworkPlayClientAdapter.MaxPlayers ;

		//-----------------------------------

		/// <summary>
		/// セッションの管理方法の種別
		/// </summary>
		public SessionManagementTypes ManagementType
			=> m_NetworkPlayClientAdapter.ManagementType ;

		/// <summary>
		/// セッションでＵＤＰ通信を有効にするかどうか
		/// </summary>
		public bool UdpEnabled
			=> m_NetworkPlayClientAdapter.UdpEnabled ;

		/// <summary>
		/// セッションのＵＤＰ通信で誤り補正を有効にするかどうか
		/// </summary>
		public bool UdpCorrectionEnabled
			=> m_NetworkPlayClientAdapter.UdpCorrectionEnabled ;

		//-------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// データの送信受信の準備が整っているかどうか
		/// </summary>
		public bool Ready
			=> m_NetworkPlayClientAdapter.Ready ;

		/// <summary>
		/// 自身がホストであるかどうか
		/// </summary>
		public bool IsHost
			=> m_NetworkPlayClientAdapter.IsHost ;

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
			=> m_NetworkPlayClientAdapter.PlayerName ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッション固有パラメータ群を取得する
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,string> GetSessionParameters()
			=> m_NetworkPlayClientAdapter.GetSessionParameters() ;

		/// <summary>
		/// 全てのセッションプレイヤーを取得する
		/// </summary>
		/// <returns></returns>
		public List<SessionPlayer> GetSessionPlayers()
			=> m_NetworkPlayClientAdapter.GetSessionPlayers() ;

		/// <summary>
		/// セッションのホストプレイヤーを取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer GetSessionHostPlayer()
			=> m_NetworkPlayClientAdapter.GetSessionHostPlayer() ;

		/// <summary>
		/// セッションプレイヤーを取得する
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public SessionPlayer GetSessionPlayer( string userId )
			=> m_NetworkPlayClientAdapter.GetSessionPlayer( userId ) ;

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext )
			=> m_NetworkPlayClientAdapter.SetReceivingCallbackType( receivingCallbackType, mainThreadContext ) ;

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue()
			=> m_NetworkPlayClientAdapter.Dequeue() ;

		/// <summary>
		/// 受信コールバックタイプを設定する(受動的か能動的か)
		/// </summary>
		/// <param name="receivingCallbackType"></param>
		public void SetReceivingCallbackType_ForSessionProcessor( ReceivingCallbackTypes receivingCallbackType, SynchronizationContext mainThreadContext )
			=> m_NetworkPlayClientAdapter.SetReceivingCallbackType_ForSessionProcessor( receivingCallbackType, mainThreadContext ) ;

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue_ForSessionProcessor()
			=> m_NetworkPlayClientAdapter.Dequeue_ForSessionProcessor() ;

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
			byte[] data,
			PacketTypes packetType,
			DestinationTypes destinationType = DestinationTypes.Broadcast,
			params string[] destinationUserIds	// 設定が必要なのは Multicast と Unicast のケース
		)
			=> m_NetworkPlayClientAdapter.Send( data, packetType, destinationType, destinationUserIds ) ;

		/// <summary>
		/// 対象プレイヤーをキックする(ホストのみ可能)
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public bool Kick( string userId )
			=> m_NetworkPlayClientAdapter.Kick( userId ) ;

		//-------------------------------------------------------------------------------------------
		// パフォーマンス計測機能

		/// <summary>
		/// 往復時間の計測を行うかどうか
		/// </summary>
		public bool UsePing
		{
			get
			{
				return m_NetworkPlayClientAdapter.UsePing ;
			}
			set
			{
				m_NetworkPlayClientAdapter.UsePing = value ;
			}
		}

		/// <summary>
		/// 往復時間計測のパケットタイプ
		/// </summary>
		public PacketTypes PingPacketType
		{
			get
			{
				return m_NetworkPlayClientAdapter.PingPacketType ;
			}
			set
			{
				m_NetworkPlayClientAdapter.PingPacketType = value ;
			}
		}

		//---------------

		/// <summary>
		/// サーバー宛の往復時間[最小]
		/// </summary>
		public long PingToServer_Min
			=> m_NetworkPlayClientAdapter.PingToServer_Min ;

		/// <summary>
		/// サーバー宛の往復時間[平均]
		/// </summary>
		public long PingToServer_Avarage
			=> m_NetworkPlayClientAdapter.PingToServer_Avarage ;

		/// <summary>
		/// サーバー宛の往復時間[最新]
		/// </summary>
		public long PingToServer
			=> m_NetworkPlayClientAdapter.PingToServer ;

		/// <summary>
		/// サーバー宛の往復時間[最大]
		/// </summary>
		public long PingToServer_Max
			=> m_NetworkPlayClientAdapter.PingToServer_Max ;

		//-----

		/// <summary>
		/// ホスト宛の往復時間[最小]
		/// </summary>
		public long PingToHost_Min
			=> m_NetworkPlayClientAdapter.PingToHost_Min ;

		/// <summary>
		/// ホスト宛の往復時間[平均]
		/// </summary>
		public long PingToHost_Avarage
			=> m_NetworkPlayClientAdapter.PingToHost_Avarage ;

		/// <summary>
		/// ホスト宛の往復時間[最新]
		/// </summary>
		public long PingToHost
			=> m_NetworkPlayClientAdapter.PingToHost ;

		/// <summary>
		/// ホスト宛の往復時間[最大]
		/// </summary>
		public long PingToHost_Max
			=> m_NetworkPlayClientAdapter.PingToHost_Max ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションサーバーに接続された際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onConnected"></param>
		public void SetOnConnected( Action onConnected )
			=> m_NetworkPlayClientAdapter.SetOnConnected( onConnected ) ;

		/// <summary>
		/// フレーム受信時に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnReceived( Action<byte[],SourceTypes,string> onReceived )
			=> m_NetworkPlayClientAdapter.SetOnReceived( onReceived ) ;

		/// <summary>
		/// フレーム受信時に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onReceived"></param>
		/// <param name="onReceivedToHost"></param>
		public void SetOnPlyerChanged( Action<SessionPlayer> onPlayerJoined, Action<SessionPlayer> onPlayerLeft )
			=> m_NetworkPlayClientAdapter.SetOnPlyerChanged( onPlayerJoined, onPlayerLeft ) ;

		/// <summary>
		/// セッションサーバーから切断された際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onDisconnected"></param>
		public void SetOnDisconnected( Action onDisconnected )
			=> m_NetworkPlayClientAdapter.SetOnDisconnected( onDisconnected ) ;

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションから離脱する
		/// </summary>
		public void LeaveFromSession()
			=> m_NetworkPlayClientAdapter.LeaveFromSession() ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// セッションのアラート群
		/// </summary>
		public List<SessionAlertTypes>	SessionAlerts
			=> m_NetworkPlayClientAdapter.SessionAlerts ;

		/// セッションアラート受信コールバックを追加する
		/// </summary>
		/// <param name="m_OnSessionAlertReceived"></param>
		public void AddOnSessionAlertReceived( Action<SessionAlertTypes> onSessionAlertReceived )
			=> m_NetworkPlayClientAdapter.AddOnSessionAlertReceived( onSessionAlertReceived ) ;

		/// <summary>
		/// セッションアラート受信コールバックを削除する
		/// </summary>
		/// <param name="m_OnSessionAlertReceived"></param>
		public void RemoveOnSessionAlertReceived( Action<SessionAlertTypes> onSessionAlertReceived )
			=> m_NetworkPlayClientAdapter.RemoveOnSessionAlertReceived( onSessionAlertReceived ) ;


	}
}
