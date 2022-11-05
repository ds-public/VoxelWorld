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

using DSW.World.Packet ;

namespace DSW.World
{
	/// <summary>
	/// クライアント(サブスレッド処理)
	/// </summary>
	public partial class WorldClient
	{
		// チャンクセットを展開する
		private void OpenChunkSet( int csId, byte[] data )
		{
			// サブスレッドでチャンクの展開を行う(メインスレッドを専有すると重くなるため)
			Task<bool> task = Task.Run( () => OpenChunkSet_Task( csId, data, m_MainThreadContext, m_CancellationSource.Token ) ) ; 
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
