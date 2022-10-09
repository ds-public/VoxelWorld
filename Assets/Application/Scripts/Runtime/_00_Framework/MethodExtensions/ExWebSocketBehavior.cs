using System ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;
using WebSocketSharp.Server ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// WebSocket まのサーバー側のクライアント管理用クラスのラッパー Version 2022/09/27
	/// </summary>
	public class ExWebSocketBehavior<T> : WebSocketBehavior where T : WebSocketBehavior
	{
		// メインスレッドのコンテキスト
		private SynchronizationContext	m_MainThreadContext ;

		//-----------------------------------
		// メインスレッド用

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnConnected_Main ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,byte[]>	m_OnReceivedData_Main ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,string>	m_OnReceivedText_Main ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnDisconnected_Main ;

		//-----------------------------------
		// 通信スレッド用

		// 新しい接続があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnConnected ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,byte[]>	m_OnReceivedData ;

		// クライアントからの情報を受信した際にサーバーに伝えるコールバック
		private Action<T,string>	m_OnReceivedText ;

		// クライアントからの切断があった際にサーバーに伝えるコールバック
		private Action<T>			m_OnDisconnected ;
		
		//------------------------------------------------------------------------------------------

		/// <summary>
		/// イベントコールバックを設定する
		/// </summary>
		/// <param name="server"></param>
		public void SetEventCallbacks
		(
			SynchronizationContext mainThreadContent,

			Action<T> onConnected_Main,
			Action<T,byte[]> onReceivedData_Main,
			Action<T,string> onReceivedText_Main,
			Action<T> onDisconnected_Main,

			Action<T> onConnected,
			Action<T,byte[]> onReceivedData,
			Action<T,string> onReceivedText,
			Action<T> onDisconnected
		)
		{
			m_MainThreadContext		= mainThreadContent ;

			m_OnConnected_Main		= onConnected_Main ;
			m_OnReceivedData_Main	= onReceivedData_Main ;
			m_OnReceivedText_Main	= onReceivedText_Main ;
			m_OnDisconnected_Main	= onDisconnected_Main ;

			m_OnConnected			= onConnected ;
			m_OnReceivedData		= onReceivedData ;
			m_OnReceivedText		= onReceivedText ;
			m_OnDisconnected		= onDisconnected ;
		}

		//----------------------------------------------------------

		// 誰かがログインしてきたときに呼ばれるメソッド
		protected override void OnOpen()
		{
			// 通信スレッドでコールバックを実行する(継承クラス内部)
			OnConnected() ;

			// 通信スレッドでコールバックを実行する
			m_OnConnected?.Invoke( this as T ) ;

			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドでコールバックを実行する
					m_OnConnected_Main?.Invoke( this as T ) ;
				}, null ) ;
			}
		}
		protected virtual void OnConnected(){}

		//---------------

		// 誰かがメッセージを送信してきたときに呼ばれるメソッド
		protected override void OnMessage( MessageEventArgs e )
		{
			if( e.IsBinary == true && e.RawData != null )
			{
				// 通信スレッドでコールバックを実行する(継承クラス内部)
				OnReceivedData( e.RawData ) ;

				// 通信スレッドでコールバックを実行する
				m_OnReceivedData?.Invoke( this as T, e.RawData ) ;

				if( m_MainThreadContext != null )
				{
					m_MainThreadContext.Post( _ =>
					{
						// メインスレッドでコールバックを実行する
						m_OnReceivedData_Main?.Invoke( this as T, e.RawData ) ;
					}, null ) ;
				}
			}
			else
			if( e.IsText == true && e.Data != null )
			{
				// 通信スレッドでコールバックを実行する(継承クラス内部)
				OnReceivedText( e.Data ) ;

				// 通信スレッドでコールバックを実行する
				m_OnReceivedText?.Invoke( this as T, e.Data ) ;

				if( m_MainThreadContext != null )
				{
					m_MainThreadContext.Post( _ =>
					{
						// メインスレッドでコールバックを実行する
						m_OnReceivedText_Main?.Invoke( this as T, e.Data ) ;
					}, null ) ;
				}
			}
		}
		protected virtual void OnReceivedData( byte[] data ){}
		protected virtual void OnReceivedText( string text ){}

		//---------------

		// 誰かがログアウトしてきたときに呼ばれるメソッド
		protected override void OnClose( CloseEventArgs e )
		{
			// 通信スレッドでコールバックを実行する(継承クラス内部)
			OnDisconnected() ;

			// 通信スレッドでコールバックを実行する
			m_OnDisconnected?.Invoke( this as T ) ;

			if( m_MainThreadContext != null )
			{
				m_MainThreadContext.Post( _ =>
				{
					// メインスレッドでコールバックを実行する
					m_OnDisconnected_Main?.Invoke( this as T ) ;
				}, null ) ;
			}
		}
		protected virtual void OnDisconnected(){}

		//-----------------------------------------------------------

		/// <summary>
		/// データを送信する
		/// </summary>
		/// <param name="data"></param>
		public void SendData( byte[] data )
		{
			base.Send( data ) ;
		}

		/// <summary>
		/// テキストを送信する
		/// </summary>
		/// <param name="text"></param>
		public void SendText( string text )
		{
			base.Send( text ) ;
		}

		/// <summary>
		/// 全クライアントにまとめて送信する
		/// </summary>
		/// <param name="text"></param>
		public void BroadcastData( byte[] data )
		{
			Sessions.Broadcast( data ) ;
		}

		/// <summary>
		/// 全クライアントにまとめて送信する
		/// </summary>
		/// <param name="text"></param>
		public void BroadcastText( string text )
		{
			Sessions.Broadcast( text ) ;
		}
	}
}
