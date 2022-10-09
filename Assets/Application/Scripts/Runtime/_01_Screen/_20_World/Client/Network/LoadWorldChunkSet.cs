using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using WebSocketSharp ;
using WebSocketSharp.Net ;


using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

using DBS.World.Packet ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(ウェブソケット)
	/// </summary>
	public partial class WorldClient
	{
		// 通信:チャンクセット展開の要求
		private bool WS_Send_Request_LoadWorldChunkSet( int csId )
		{
			if( m_WebSocket == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			// ロードリクエストが出されていたら無視する
			if( m_ChunkSetRequests.Contains( csId ) == true )
			{
				return false ;
			}

			m_ChunkSetRequests.Add( csId ) ;

			//----------------------------------------------------------

			// パケットを生成する
			var request = Packet.ClientRequestTypes.LoadWorldChunkSet.Encode
			(
				csId
			) ;

			// サーバーにパケットを送信する
			m_WebSocket.Send( request ) ;

			return true ;
		}

		//-----------------------------------

		// 圧縮されたチャンク情報を受信したら呼び出される
		private void WS_OnReceived_Response_LoadWorldChunkSet( byte[] data )
		{
			var context = Packet.ServerResponseTypes.LoadWorldChunkSet.Decode( data ) ;
			if( context == null )
			{
				return ;
			}

			//----------------------------------------------------------
			// レスポンスパラメータの展開

			int		csId		= context.CsId ;
			byte[]	chunkSet	= context.ChunkSet ;

			//----------------------------------------------------------
			// クライアント側の処理

//			Debug.Log( "[CLIENT]チャンクセット受信:CsId = " + csId.ToString( "X4" ) + " Size = " + chunkSet.Length ) ;

			if( m_ChunkSetRequests.Contains( csId ) == false )
			{
				// 既にリクエストが取り消されている
				return ;
			}

			//--------------
			// リクエストは有効

			// ※現在はメインスレッド

			// サブスレッドでチャンクの展開を行う(メインスレッドを専有すると重くなるため)
			Task<bool> task = Task.Run( () => OpenChunkSet_Task( csId, chunkSet, m_MainThreadContext, m_CancellationSource.Token ) ) ; 

			// task.Result で戻り値が得られるがここでは特に意味はない
		}

		// サブスレッドでチャンクセットを展開する
		private bool OpenChunkSet_Task( int csId, byte[] data, SynchronizationContext mainThreadContext, CancellationToken cancellationToken )
		{
			if( cancellationToken.IsCancellationRequested == true )
			{
//				Debug.Log( "<color=#FF0000>キャンセルされた</color>" ) ;

				// 例外(キャンセル)をスローする
//				cancellationToken.ThrowIfCancellationRequested() ;
				return false ;
			}

			//----------------------------------

			// サブスレッドでは使えない可能性が高い
//			float t = Time.realtimeSinceStartup ;

			// チャンクセットを展開(生成)する(そこそこ重い処理なのでサブスレッドで処理する)
			var chunkSet = new ClientChunkSetData( csId, data ) ;

//			Debug.Log( "チャンク(" + csId.ToString( "X4" ) + ")展開:" + ( Time.realtimeSinceStartup - t ) + "秒" ) ;

			//--------------

			// チャンクセットが展開完了したらメインスレッドでアクティブチャンクセット群に登録する
			if( mainThreadContext != null )
			{
				mainThreadContext.Post( _ =>
				{
					// メインスレッドで実行する(処理の直列化のため)

					// アクティブチャンクセット群にチャンクセットを追加する
					AddChunkSet( chunkSet ) ;

					// リクエストを削除する(この場所で消さないとサブスレッドで処理中に新たなリクエストが出されてしまう)
					m_ChunkSetRequests.Remove( csId ) ;
				}, null ) ;
			}

			//----------------------------------

			return true ;
		}



	}
}
