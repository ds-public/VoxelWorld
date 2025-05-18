#nullable enable
#pragma warning disable CA1822
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable IDE0028
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable IDE0305

using System ;
using System.Collections.Generic ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// セッションの操作機能
	/// </summary>
	public interface ISessionFunction
	{
		/// <summary>
		/// セッション内の全プレイヤー情報を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer[] GetSessionPlayers() ;

		/// <summary>
		/// セッション内のホストプレイヤーの情報を取得する
		/// </summary>
		/// <returns></returns>
		public SessionPlayer GetSessionHostPlayer() ;

		/// <summary>
		/// セッション内の指定したプレイヤーの情報を取得する
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public SessionPlayer GetSessionPlayer( string userId  ) ;

		//-----------------------------------

		/// <summary>
		/// 受信コールバックが能動的コールバックに設定されている場合にデータを受信済みならコールバックを発生させる
		/// </summary>
		/// <returns></returns>
		public int Dequeue() ;

		//-----------------------------------------------------------

		/// <summary>
		/// データを送信する
		/// </summary>
		/// <param name="destinationType"></param>
		/// <param name="destinationUserIds"></param>
		/// <param name="data"></param>
		public bool Send( byte[] data, DestinationTypes destinationType = DestinationTypes.Broadcast, params string[] destinationUserIds ) ;

		/// <summary>
		/// データを送信する
		/// </summary>
		/// <param name="destinationType"></param>
		/// <param name="destinationUserIds"></param>
		/// <param name="data"></param>
		public bool Send( byte[] data, PacketTypes packetType, DestinationTypes destinationType = DestinationTypes.Broadcast, params string[] destinationUserIds ) ;

		/// <summary>
		/// 指定したユーザー識別子のプレイヤーをセッションからキックする
		/// </summary>
		/// <param name="userIds"></param>
		public bool Kick( string userId ) ;
	}

	/// <summary>
	/// セッションサーバーの独自処理用インターフェース
	/// </summary>
	public interface ISessionProcessor
	{
		/// <summary>
		/// セッションが生成された際に呼び出される
		/// </summary>
		/// <param name="players"></param>
		public void OnCreated( ISessionFunction function ) ;

		/// <summary>
		/// セッションが実際に有効化された際に呼び出される
		/// </summary>
		/// <param name="sessionPlayers"></param>
		public void OnActive( SessionPlayer[] sessionPlayers ) ;

		/// <summary>
		/// セッションが破棄された際に呼び出される
		/// </summary>
		public void OnDeleted() ;

		/// <summary>
		/// セッションにプレイヤーが参加した際に呼び出される
		/// </summary>
		/// <param name="player"></param>
		public void OnPlayerJoined( SessionPlayer sessionPlayer ) ;

		/// <summary>
		/// セッションからプレイヤーが離脱した際に呼び出される
		/// </summary>
		/// <param name="player"></param>
		public void OnPlayerLeft( SessionPlayer sessionPlayer ) ;

		/// <summary>
		/// ホストとなっているプレイヤーが変化した際に呼び出される(ホスト制御だと呼ばれない)
		/// </summary>
		/// <param name="hostUserId"></param>
		public void OnHostPlayerChanged( string hostUserId ) ;

		/// <summary>
		/// データを受信した際に呼び出される
		/// </summary>
		/// <param name="data"></param>
		/// <param name="sourceUserId"></param>
		public void OnReceived( byte[] data, string sourceUserId ) ;
	}
}
