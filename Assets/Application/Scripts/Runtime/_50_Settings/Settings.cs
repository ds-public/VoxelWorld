using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace DBS
{
	/// <summary>
	/// 起動時の各種設定クラス Version 2022/10/02
	/// </summary>
	[CreateAssetMenu( fileName = "Settings", menuName = "ScriptableObject/DBS/Settings" )]
	public class Settings : ScriptableObject
	{
		[Header( "システムバージョン名" )]
		public string	SystemVersionName = "0.2.1" ;

		[Header( "リビジョン[Android] (システムバージョン名が変ったら0にリセットする事)" )]
		public int		Revision_Android = 0 ;

		[Header( "リビジョン[iOS] (システムバージョン名が変ったら0にリセットする事)" )]
		public int		Revision_iOS = 0 ;

		[Header( "ビルドバージョン(自動設定)" )]
		public string	BuildVersion ;

		/// <summary>
		/// リビジョン
		/// </summary>
		public int		Revision
		{
			get
			{
#if UNITY_IOS
				return Revision_iOS ;
#else
				return Revision_Android ;
#endif
			}
		}

		/// <summary>
		/// プラットフォームを自動判断してクライアントバージョンを返す
		/// </summary>
		public string   ClientVersionName
		{
			get
			{
#if UNITY_IOS
				return SystemVersionName ;	// iOS は、メジャーバージョン(4桁).マイナーバージョン(2桁).パッチバージョン(2桁)でなくてはならないルールがあるためシステムバージョンそのもののを返す
#else
				return SystemVersionName + "." + Revision.ToString() ;	// Android はバージョン表記の形式は自由
#endif
			}
		}

		[Header( "クライアントバージョン値 (1 以上且つリリース後にクライアントに変更があったら +1 すること)" )]
		public int		ClientVersionCode = 1 ;	// 1 以上の値にすること

		//-----------------------------------------------------------

		[Header( "フルスクリーン指定(STANDALONEのみ有効)")]
		public bool		IsFullScreen		= false ;

		[Header( "仮想解像度" )]
		public float	BasicWidth			=  960 ;
		public float	BasicHeight			=  540 ;

		[Header( "最大解像度" )]
		public float	LimitWidth			= 1280 ;
		public float	LimitHeight			=  720 ;

		[Header( "初期フレームレート" )]
		public int		FrameRate_Rendering = 60 ;
		public int		FrameRate_FixedTime = 60 ;

		/// <summary>
		/// 垂直同期の動作タイプ
		/// </summary>
		public enum VsyncTypes
		{
			/// <summary>
			/// 無効
			/// </summary>
			Invalid = 0,

			/// <summary>
			/// 等倍
			/// </summary>
			Default = 1,

			/// <summary>
			/// ２分の１
			/// </summary>
			Divide2 = 2,

			/// <summary>
			/// ３分の１
			/// </summary>
			Divide3 = 3,

			/// <summary>
			/// ４分の１
			/// </summary>
			Divide4 = 4,
		}

		[Header( "垂直同期(STANDALONE以外では無効)" )]
		public VsyncTypes	VsyncType = VsyncTypes.Invalid ;

		/// <summary>
		/// アセットバンドルのプラットフォームをビルドプラットフォームと強制的に違うものにしてデバッグするためのパラメータ
		/// </summary>
		public enum AssetBundlePlatformTypes
		{
			BuildPlatform,	// アセットバンドルのプラットフォームはビルドプラットフォームと同じ
			Windows,		// アセットバンドルのプラットフォームを強制的にWindowsにする
			OSX,			// アセットバンドルのプラットフォームを強制的にOSXにする
			Android,		// アセットバンドルのプラットフォームを強制的にAndroidにする
			iOS,			// アセットバンドルのプラットフォームを強制的にiOSにする
		}

		/// <summary>
		/// アセットバンドルのプラットフォームをビルドプラットフォームと強制的に違うものにしてデバッグするためのパラメータ
		/// </summary>
		[Header( "ダウンロードするアセットバンドルプラットフォーム" )]
		public AssetBundlePlatformTypes AssetBundlePlatformType = AssetBundlePlatformTypes.BuildPlatform ;

		/// <summary>
		/// ローカルアセットを使用するかどうか
		/// </summary>
		[Header( "ローカルのアセットを使用するかどうか" )]
		public bool UseLocalAssets = true ;

		/// <summary>
		/// ローカルアセットバンドルを使用するかどうか
		/// </summary>
		[Header( "ローカルのアセットバンドルを使用するかどうか" )]
		public bool UseLocalAssetBundle = true ;

		/// <summary>
		/// ストリーミングアセット(内のアセットバンドル)を使用するかどうか
		/// </summary>
		[Header( "ストリーミングアセットのアセットバンドルを使用するかどうか" )]
		public bool UseStreamingAssets = true ;

		/// <summary>
		/// リモートアセット(アセットバンドル)を使用するかどうか
		/// </summary>
		[Header( "リモートのアセットバンドルを使用するかどうか" )]
		public bool UseRemoteAssetBundle = true ;

		/// <summary>
		/// 並列ダウンロードを使用するかどうか
		/// </summary>
		[Header( "並列ダウンロードを使用するかどうか)" )]
		public bool	UseParallelDownload = true ;

		/// <summary>
		/// 並列ダウンロード時の最大数
		/// </summary>
		[Header( "並列ダウンロード時の最大数)" )]
		public int MaxParallelDownloadCount = 6 ;

		/// <summary>
		/// リモートアセットバンドルのエンドポイントを強制指定する
		/// </summary>
		[Header( "リモートアセットバンドルのエンドポイントを強制指定する" )]
		public string RemoteAssetBundlePath = string.Empty ;	// 言語とプラットフォームも指定する必要がある

		/// <summary>
		/// アセットバンドルの全体のバージョン値(デバッグ用)の値が異なる場合にアセットバンドルを全消去するかどうか
		/// </summary>
		[Header( "アセットバンドルが古い場合に全消去を行うかどうか" )]
		public bool EnableAssetBundleCleaning = true ;

		/// <summary>
		/// アセットバンドルの全体のバージョン値(デバッグ用)
		/// </summary>
		[Header( "クライアントアセットバンドルバージョン" )]
		public int	AssetBundleVersion = 0 ;


		//-----------------------------------------------------

		/// <summary>
		/// ダミーのPlayerDataをどこから読み出すか
		/// </summary>
		public enum PlayerDataLoadProcessingTypes
		{
			FinalSaveDataLoading,	// 最後にセーブされたデータをロードする
			AlwaysInitialization,	// 常に初期化する
		}

		// ストレージに探しに行って無ければＣＳＶから読み出す
		[Header( "ダミーのプレイヤーデータの読み出し場所" )]
		public PlayerDataLoadProcessingTypes PlayerDataLoadProcessingType = PlayerDataLoadProcessingTypes.FinalSaveDataLoading ;

		//-----------------------------------

		/// <summary>
		/// ＦＰＳの表示
		/// </summary>
		[Header( "ＦＰＳを表示するかどうか" )]
		public bool FPS = true ;


		//-----------------------------------------------------------

		/// <summary>
		/// ＰＣ環境上でモバイル端末の動作を確認するためのフラグ
		/// </summary>
		public enum PlatformType
		{
			Default,	// 現在のプラットフォームの動作に従う
			Mobile,		// デバッグ用に強制的にモバイルモードの動作にする
		}

		[Header( "モバイル端末エミュレーションを強制的に行うかどうか" )]
		public PlatformType SelectedPlatformType = PlatformType.Default ;

		/// <summary>
		/// 例外が発生した時に例外ダイアログを表示する
		/// </summary>
		[Header( "例外発生時にダイアログを表示するかどうか(実機限定)" )]
		public bool EnableExceptionDialog = true ;

		/// <summary>
		/// データの形式
		/// </summary>
		public enum DataTypes
		{
			MessagePack,
			Json,
		}

		[Header( "通信データの形式" )]
		public DataTypes	DataType = DataTypes.MessagePack ;

		/// <summary>
		/// エンドポイント
		/// </summary>
		public enum EndPointNames
		{
			Experiment	=  0,
			Development =  1,
			Staging		=  2,
			Release		=  3,

			Any			= 99,
		}

		/// <summary>
		/// エンドポイントの情報
		/// </summary>
		[Serializable]
		public class EndPointData
		{
			public EndPointNames	Name ;
			public string			DisplayName ;
			public string			Path ;
			public string			Description ;
		}

		private static Dictionary<EndPointNames,string> m_EndPointNames = new Dictionary<EndPointNames, string>()
		{
			{ EndPointNames.Experiment,		"実験サーバー"			},
			{ EndPointNames.Development,	"開発サーバー"			},
			{ EndPointNames.Staging,		"監修サーバー"			},
			{ EndPointNames.Release,		"本番サーバー"			},
		} ;


		[Header( "WebAPIの通信先(エンドポイント)リスト" )] //[NonSerialized]
		public EndPointData[] WebAPI_EndPoints = new EndPointData[]
		{
			new EndPointData()
			{
				Name		= EndPointNames.Experiment,
				DisplayName = m_EndPointNames[ EndPointNames.Experiment		], Path = "https://api-experiment-dbs.net/api/",
				Description = "実験用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Development,
				DisplayName = m_EndPointNames[ EndPointNames.Development	], Path = "https://api-development-dbs.net/api/",
				Description = "開発用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Staging,
				DisplayName = m_EndPointNames[ EndPointNames.Staging		], Path = "https://api-staging-dbs.net/api/",
				Description = "確認用の環境です"
			},
			new EndPointData()
			{
				Name		= EndPointNames.Release,
				DisplayName = m_EndPointNames[ EndPointNames.Release		], Path = "https://api-release-dbs.net/api/",
				Description = "公開用の環境です"
			},
		} ;

		[Header( "デフォルトのWebAPIの通信先(エンドポイント)" )]
		public EndPointNames WebAPI_DefaultEndPoint = EndPointNames.Development ;	// バッチビルド時に書き換える

		[Header( "各種デバッグ用の機能を有効にするか(ただし開発環境でのみ有効)" )]
		public bool DevelopmentMode = true ;	// バッチビルド時に書き換える

		//-------------------------------------------------------------------------------------------

		[Header( "各種情報の暗号化を行う(実機は常に有効)" )]
		public bool	SecurityEnabled = true ;

		//-------------------------------------------------------------------------------------------
		// CriWare 関連

//		[Header("CriWare - 最大のファイルバインド可能数")]
//		public int		CriWare_MaxMaxNumberOfBinders	= 24 ;

//		[Header("CriWare - サウンド(Atom)の復号化キー")]
//		public string	CriWare_EncryptKey				= "607582128852343681" ;

//		[Header("CriWare - サウンド(Atom) の復号化有効")]
//		public bool		CriWare_AtomDecryptEnabled		= true ;

//		[Header("CriWare - ムービー(Mana)の復号化ファイルパス")]
//		public string	CriWare_AuthFilePath			= "Internal|Sounds_ADX2/dbs_movie" ;

//		[Header("CriWare - ムービー(Mana) の復号化有効")]
//		public bool		CriWare_ManaDecryptEnabled		= true ;

		//-------------------------------------------------------------------------------------------

		[Header( "サーバーのポート番号" )]
		public int	ServerPortNumber = 32760 ;
	}
}

