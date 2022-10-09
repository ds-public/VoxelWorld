using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using SceneManagementHelper ;

using DBS.World ;

namespace DBS.Screens
{
	/// <summary>
	/// 起動直後のダウンロード処理(マスターデータ・アセットバンドルの更新)
	/// </summary>
	public partial class Downloading
	{
		/// <summary>
		/// 最初から展開しておく必要のアセットを展開する(外部[Layout Dialog]から呼ばれる可能性があるので static メソッド)
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadConstantAsset( int phase, ExMonoBehaviour _ )
		{
			// 設定ファイルを読み出す
			var settings = ApplicationManager.LoadSettings() ;

			//--------------------------------------------------------------------------
			// Pahse 1

			if( ( phase & 1 ) != 0 )
			{
				Debug.Log( "<color=#00FF00>[Downloading] Phase 1 Excution</color>" ) ;

				//----------------------------------------------------------
				// Internal

				// タイトルＢＧＭのロードを行いキャッシュにためておく(※Androidの場合はダイレクトアクセスが出来ないのでストレージへのコピーが必要)
				await BGM.LoadInternalAsync() ;

				// システムＳＥのロードを行いキャッシュにためておく(※Androidの場合はダイレクトアクセスが出来ないのでストレージへのコピーが必要)
				await SE.LoadInternalAsync() ;

				// メインＢＧＭの状態を初期化する
				BGM.Initialize() ;
			}

			//--------------------------------------------------------------------------
			// Pahse 2

			if( ( phase & 2 ) != 0 )
			{
				Debug.Log( "<color=#00FF00>[Downloading] Phase 2 Excution</color>" ) ;

				// タイトルＢＧＭのロードを行いキャッシュにためておく(※Androidの場合はダイレクトアクセスが出来ないのでストレージへのコピーが必要)
				await BGM.LoadInternalAsync() ;

				// システムＳＥのロードを行いキャッシュにためておく(※Androidの場合はダイレクトアクセスが出来ないのでストレージへのコピーが必要)
				await SE.LoadInternalAsync() ;

				// メインＢＧＭの状態を初期化する
				BGM.Initialize() ;

				//---------------------------------------------------------
				// アセットバンドル側にあるシーンを展開できる状態にする

				//---------------------------------------------------------

				// ワールドサーバーシーンをロードする
				await LoadWorldServerScene( _ ) ;
			}
		}

		// ワールドサーバーシーンをロードする
		private static async UniTask LoadWorldServerScene( ExMonoBehaviour owner )
		{
			if( WorldServer.Instance != null )
			{
				return ;
			}

			//----------------------------------------------------------

			// イベント用のシーンをロードする
			WorldServer worldServer= null ;
			await EnhancedSceneManager.AddAsync<WorldServer>( "WorldServer", ( _ ) => { worldServer = _[ 0 ] ; } ) ;
			if( worldServer != null )
			{
				DontDestroyOnLoad( worldServer.gameObject ) ;	// DontDestroyOnLoad は、完全にロードが終わった後に実行しないと、目的のコンポーネントのインスタンスが取得出来なくなってしまう。

				worldServer.gameObject.SetActive( false ) ;
			}
		}

	}
}
