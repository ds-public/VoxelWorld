using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// ネットワークＡＰＩのショートカットクラス
	/// </summary>
	public class NetAPI
	{
		/// <summary>
		/// オプション動作定義クラス
		/// </summary>
		public class Option
		{
			public int      dialogHoldTime      = -1 ;
			public bool     skipErrorHandling   = false ;   // エラーが発生してもエラー処理をスキップするかの有無
			public int      retryCount          = 0 ;       // 通信エラー時のリトライ回数
			public byte[]   key                 = null ;    // 暗号化などのキー
			public float    timeout             = -1f ;     // 受信タイムアウトの時間
			public float    connectTimeout      = -1f ;     // 接続タイムアウトの時間
			public bool     noUpdateMasterData  = false ;   // マスターデータの更新を行うかの有無

			/// <summary>
			/// デフォルト設定のオプションオブジェクトを取得する
			/// </summary>
			public static Option DefaultOption
			{
				get
				{
					if( defaultOption == null )
					{
						defaultOption = new Option();
					}
					return defaultOption;
				}
			}
			private static Option defaultOption = null ;
		}

		//---------------------------------------------------------------------

		// カテゴリごとの通信ＡＰＩのスタティックインスタンスを記述する

/*		public static NetAPIs.Sync sync						= new NetAPIs.Sync() ;				// Sync カテゴリの通信ＡＰＩのスタティックインスタンス
		public static NetAPIs.Auth auth						= new NetAPIs.Auth() ;				// Auth カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.User user						= new NetAPIs.User() ;				// User カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.LoginBonus loginBonus			= new NetAPIs.LoginBonus() ;		// LoginBonus カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.MemberStory memberStory		= new NetAPIs.MemberStory() ;

		public static NetAPIs.ActiveTeamId	activeTeamId	= new NetAPIs.ActiveTeamId() ;

		public static NetAPIs.Team team						= new NetAPIs.Team() ;

		public static NetAPIs.Unit unit						= new NetAPIs.Unit() ;				// Unit カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.Item item						= new NetAPIs.Item() ;				// Item カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.Equipment equipment			= new NetAPIs.Equipment() ;			// Equipment カテゴリの通信ＡＰＩのスタティックインスタンス

		public static NetAPIs.Sphere sphere					= new NetAPIs.Sphere() ;

		public static NetAPIs.Quest quest					= new NetAPIs.Quest() ;

		public static NetAPIs.Gift gift						= new NetAPIs.Gift() ;

		public static NetAPIs.Gacha gacha					= new NetAPIs.Gacha() ;

		public static NetAPIs.Friend friend					= new NetAPIs.Friend() ;

		public static NetAPIs.GameDebug gameDebug			= new NetAPIs.GameDebug() ;
*/

		//-----------------------------------------------------------

//		public static NetAPIs.Download	download			= new NetAPIs.Download() ;

		//---------------------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( isDone == false && string.IsNullOrEmpty( error ) == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool isDone = false ;

			/// <summary>
			/// ステータス
			/// </summary>
			public int	status = 0 ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string error = "" ;

			/// <summary>
			/// レスポンスデータ
			/// </summary>
			public System.Object data = null ;

			/// <summary>
			/// 指定の型でレスポンスデータを取得する(直接 .data でも良い)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public T GetResponse<T>() where T : class
			{
				if( data == null )
				{
					return null ;
				}

				return data as T ;
			}

			/// <summary>
			/// ダウンロード済みのサイズ
			/// </summary>
			public int    offset ;

			/// <summary>
			/// ダウンロードされるサイズ
			/// </summary>
			public int    length ;

			/// <summary>
			/// ダウンロード状況
			/// </summary>
			public float progress
			{
				get
				{
					if( m_Progress == 0 )
					{
						if( length <= 0 )
						{
							return 0 ;
						}
	
						return ( float )offset / ( float )length ;
					}
					else
					{
						return m_Progress ;
					}
				}
				set
				{
					m_Progress = value ;
				}
			}

			private float m_Progress = 0 ;
		}
	}
}
