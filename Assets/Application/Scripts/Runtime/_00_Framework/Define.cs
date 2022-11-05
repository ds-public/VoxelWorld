using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DSW
{
	/// <summary>
	/// 共通定義クラス Version 2022/09/22 0
	/// </summary>
	public class Define
	{
		/// <summary>
		/// システムバージョン
		/// </summary>
		public static string SystemVersion
		{
			get
			{
				var settings = ApplicationManager.LoadSettings() ;
				return settings.SystemVersionName ;
			}
		}

		/// <summary>
		/// プラッフォーム名を取得する
		/// </summary>
		public static int PlatformCode
		{
			get
			{
				// 毎回Google先生に聞くのは面倒なのでリンクを貼っておく
				// https://docs.unity3d.com/ja/2018.4/Manual/PlatformDependentCompilation.html

#if UNITY_STANDALONE_WIN
				return 10 ;
#elif UNITY_STANDALONE_OSX
				return 11 ;
#elif UNITY_ANDROID
				return  1 ;
#elif UNITY_IOS || UNITY_IPHONE
				return  2 ;
#else
				return  0 ;
#endif
			}
		}

		/// <summary>
		/// プラッフォーム名を取得する
		/// </summary>
		public static string PlatformName
		{
			get
			{
				// 毎回Google先生に聞くのは面倒なのでリンクを貼っておく
				// https://docs.unity3d.com/ja/2018.4/Manual/PlatformDependentCompilation.html

#if UNITY_STANDALONE_WIN
				return "Windows" ;
#elif UNITY_STANDALONE_OSX
				return "OSX" ;
#elif UNITY_ANDROID
				return "Android" ;
#elif UNITY_IOS || UNITY_IPHONE
				return "iOS" ;
#else
				return "Unknown" ;
#endif
			}
		}

		/// <summary>
		/// 言語コード名
		/// </summary>
		public static string LanguageCodeName
		{
			get
			{
				// 多言語対応するまでは全て ja
				if( Application.systemLanguage == SystemLanguage.Japanese )
				{
					return "ja" ;
				}
				else
				if( Application.systemLanguage == SystemLanguage.Korean )
				{
					return "kr" ;
				}
				else
				if( Application.systemLanguage == SystemLanguage.Chinese )
				{
					return "cn" ;
				}
				else
				if( Application.systemLanguage == SystemLanguage.ChineseSimplified )
				{
					return "cs" ;
				}
				else
				if( Application.systemLanguage == SystemLanguage.ChineseTraditional )
				{
					return "ct" ;
				}
				else
				if( Application.systemLanguage == SystemLanguage.English )
				{
					return "en" ;
				}

				return "ja" ;	// デフォルト
			}
		}

		/// <summary>
		/// 一意のデバイス識別子
		/// </summary>
		/// <returns></returns>
		public static string DeviceKey
		{
			get
			{
				// https://docs.unity3d.com/ja/2018.4/ScriptReference/SystemInfo-deviceUniqueIdentifier.html
				return SystemInfo.deviceUniqueIdentifier ;
			}
		}

		/// <summary>
		/// デバイス情報
		/// </summary>
		public static string DeviceName
		{
			get
			{
				return SystemInfo.deviceModel + " " + SystemInfo.deviceName + " " + SystemInfo.deviceType ;
			}
		}

		/// <summary>
		/// OS名とバージョン
		/// </summary>
		/// <returns></returns>
		public static string OS
		{
			get
			{
				// https://docs.unity3d.com/ja/2019.4/ScriptReference/SystemInfo-operatingSystem.html
				return SystemInfo.operatingSystem ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 通信用暗号化キー
		/// </summary>
		public const string AESKey			= "MWxUqvsnJTwW7JtA/kZnqGGZny1/jXtSUbO/tA1ycXY=" ;

		/// <summary>
		/// 通信用暗号化ベクター
		/// </summary>
		public const string AESVector		= "Svs/zz9nLDSDTmLJ9OplgA==" ;

		/// <summary>
		/// 暗号化キー
		/// </summary>
		public const string CryptoKey		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;

		/// <summary>
		/// 暗号化ベクター
		/// </summary>
		public const string CryptoVector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;

		/// <summary>
		/// プレイヤーデータの保存フォルダ
		/// </summary>
		public const string Folder			= "preference/" ;

		//-----------------------------------------------------------

		/// <summary>
		/// 各種情報の暗号化を有効化するか
		/// </summary>
		public static bool SecurityEnabled
		{
			get
			{
				bool securityEnabled = false ;
				var settings = ApplicationManager.LoadSettings() ;
				if( settings != null )
				{
					securityEnabled = settings.SecurityEnabled ;
				}

#if !UNITY_EDITOR && DEVELOPMENT_BUILD
				securityEnabled = true ;
#endif
				return securityEnabled ;
			}
		}

		/// <summary>
		/// 開発モードかどうか
		/// </summary>
		public static bool DevelopmentMode
		{
			get
			{
				bool developmentMode = false ;
				var settings = ApplicationManager.LoadSettings() ;
				if( settings != null )
				{
					var endPoint = settings.WebAPI_DefaultEndPoint ;
					if( endPoint != Settings.EndPointNames.Staging && endPoint != Settings.EndPointNames.Release )
					{
						developmentMode = settings.DevelopmentMode ;
					}
				}

//				return false ;	// デバッグ
//				return true ;	// デバッグ
				return developmentMode ;
			}
		}

		/// <summary>
		/// デフォルトのエンドポイント
		/// </summary>
		public static Settings.EndPointNames DefaultEndPoint
		{
			get
			{
				var settings = ApplicationManager.LoadSettings() ;
				if( settings == null )
				{
					return Settings.EndPointNames.Development ;
				}
				return settings.WebAPI_DefaultEndPoint ;
			}
		}

#if false
		/// <summary>
		/// 現在のエンドポイント
		/// </summary>
		public static Settings.EndPointNames CurrentEndPoint
		{
			get
			{
				var settings = ApplicationManager.LoadSettings() ;
				if( settings == null )
				{
					return Settings.EndPointNames.Any ;
				}

				string endPoint = PlayerData.EndPoint ;

				var endPoints = settings.WebAPI_EndPoints ;

				int i, l = endPoints.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( endPoints[ i ].Path == endPoint )
					{
						break ;
					}
				}

				if( i >= l )
				{
					// 不明
					return Settings.EndPointNames.Any ;
				}

				return  endPoints[ i ].Name ;
			}
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 通信エラーダイアログ内のタイトルへボタンの文言
		/// </summary>
		public const string GOTO_TITLE	= "タイトルへ" ;

		/// <summary>
		/// 通信エラーダイアログ内の再実行ボタンの文言
		/// </summary>
		public const string RETRY		= "リトライ" ;

		/// <summary>
		/// 通信エラーダイアログ内の再起動ボタンの文言
		/// </summary>
		public const string REBOOT		= "再起動" ;

		//-----------------------------------------------------------

		/// <summary>
		/// マスターデータバージョン
		/// </summary>
		public static int	MasterDataVersion = 0 ;

		/// <summary>
		/// マスターデータのキー
		/// </summary>
		public static string MasterDataKey ;


		//-------------------------------------------------------------------------------------------
	}
}
