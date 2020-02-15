using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// 共通定義クラス Version 2017/08/13 0
	/// </summary>
	public class Define
	{
		/// <summary>
		/// 暗号化キー
		/// </summary>
		public const string cryptoKey		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;

		/// <summary>
		/// 暗号化ベクター
		/// </summary>
		public const string cryptoVector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;

		/// <summary>
		/// プレイヤーデータの保存フォルダ
		/// </summary>
		public const string folder			= "preference/" ;




		/// <summary>
        /// EC2開発環境アドレス
		/// </summary>
        public const string ec2_dev_url         = "http://moe-dev.ibrains.co.jp/";

		/// <summary>
        /// 中村氏EC2開発環境アドレス
		/// </summary>
        public const string ec2_nakamura_url    = "http://moe-nakamura.ibrains.co.jp/";

		/// <summary>
        /// 辻野氏EC2開発環境アドレス
		/// </summary>
        public const string ec2_tsujino_url		= "http://moe-tsujino.ibrains.co.jp/";

		/// <summary>
		/// 本田氏EC2開発環境アドレス
		/// </summary>
        public const string ec2_honda_url       = "http://moe-honda.ibrains.co.jp/";

		/// <summary>
		/// プランナー確認用アドレス
		/// </summary>
		public const string ec2_check_url		= "http://moe-check.ibrains.co.jp/";

		/// <summary>
		/// AGS確認用環境アドレス.
		/// </summary>
		public const string ec2_staging_url		= "http://moe-staging.ibrains.co.jp/";

		/// <summary>
		/// UnStable開発環境アドレス.
		/// </summary>
		public const string ec2_unstable_url	= "http://moe-unstable.ibrains.co.jp/";


        /// <summary>
        /// 通信エラーダイアログの汎用タイトル文言
        /// </summary>
        public const string general_error_title = "通信状況の確認";
        /// <summary>
        /// 通信エラーダイアログの汎用説明文言
        /// </summary>
        public const string general_error_description = "通信状況の良い場所に移動するか、しばらく時間をおいてからお試しください。";
        /// <summary>
        /// 通信エラー400ダイアログの説明文言
        /// </summary>
        public const string error_400_description = "通信に失敗したため\nタイトルへ戻ります。";

        /// <summary>
        /// 通信エラー401ダイアログのタイトル文言
        /// </summary>
        public const string error_401_title = "セッションが切れました";
        /// <summary>
        /// 通信エラー401ダイアログの説明文言
        /// </summary>
        public const string error_401_description = "セッションが切れたため\nタイトルへ戻ります。";

        /// <summary>
        /// 通信エラー406ダイアログのタイトル文言
        /// </summary>
        public const string error_406_title = "アプリの更新";
        /// <summary>
        /// 通信エラー406ダイアログの説明文言
        /// </summary>
        public const string error_406_description = "新しいバージョンのアプリがあるので\n更新をしてください。";

        /// <summary>
        /// メンテナンス(通信エラー502)ダイアログのタイトル文言
        /// </summary>
        public const string maintenance_title = "メンテナンス中";
        /// <summary>
        /// メンテナンス(通信エラー502)の説明文言
        /// </summary>
        public const string maintenance_description = "しばらく時間をおいてから\nお試しください。";

        /// <summary>
        /// 通信エラーダイアログ内の更新ボタンの文言
        /// </summary>
        public const string update = "更新";
        /// <summary>
        /// 通信エラーダイアログ内のタイトルへボタンの文言
        /// </summary>
        public const string goto_title = "タイトルへ";
        /// <summary>
        /// 通信エラーダイアログ内の再実行ボタンの文言
        /// </summary>
        public const string retry = "再実行";
        /// <summary>
        /// 通信エラーダイアログ内の再起動ボタンの文言
        /// </summary>
        public const string reboot = "再起動";


		/// <summary>
		/// プラッフォーム名を取得する
		/// </summary>
		public static string platformName
		{
			get
			{
				string tPlatformName = "PC" ;

#if UNITY_IOS || UNITY_IPHONE
				tPlatformName = "iOS" ;
#elif UNITY_ANDROID
				tPlatformName = "Android" ;
#endif

				return tPlatformName ;
			}
		}

		/// <summary>
		/// プラットフォーム番号を返す(通信で使用している)
		/// </summary>
		public static int platformNumber
		{
			get
			{
				return 0 ;
			}
		}

		/// <summary>
		/// アセットバンドル用のプラッフォーム名を取得する
		/// </summary>
		public static string assetBundlePlatformName
		{
			get
			{
				string tPlatformName = "Windows" ;


#if UNITY_ANDROID
				tPlatformName = "Android" ;
#elif UNITY_IOS || UNITY_IPHONE
				tPlatformName = "iOS" ;
#endif

				return tPlatformName ;
			}
		}

		public const string CLIENT_VERSION = "0.0.0" ;

		public static byte[]	PW = { 0 } ;
		public const int		PW_XOR = 0 ;

		public const string	RSA_MODULES = "" ;
		public const string	RSA_EXPONENT = "" ;


	}
}
