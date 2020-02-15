using UnityEngine ;
using System ;
using System.Collections ;

namespace DBS
{
	/// <summary>
	/// コンフィグ系データ
	/// </summary>
	[Serializable]
	public class Configuration : PreferenceBase
	{
		/// <summary>
		/// ＢＧＭボリューム
		/// </summary>
		public float _bgmVolume		= 1.0f ;

		/// <summary>
		/// ＢＧＭボリューム(ショートカットアクセス)
		/// </summary>
		public static float BgmVolume
		{
			get
			{
				return PreferenceManager.Instance.configuration._bgmVolume ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
			set
			{
				PreferenceManager.Instance.configuration._bgmVolume = value ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
		}

		/// <summary>
		/// ＳＥボリューム
		/// </summary>
		public float _seVolume		= 1.0f ;

		/// <summary>
		/// ＳＥボリューム(ショートカットアクセス)
		/// </summary>
		public static float SeVolume
		{
			get
			{
				return PreferenceManager.Instance.configuration._seVolume ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
			set
			{
				PreferenceManager.Instance.configuration._seVolume = value ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
		}

		/// <summary>
		/// Ｖｏｉｃｅボリューム
		/// </summary>
		public float _voiceVolume	= 1.0f ;

		// Ｖｏｉｃｅボリューム(ショートカットアクセス)
		public static float VoiceVolume
		{
			get
			{
				return PreferenceManager.Instance.configuration._voiceVolume ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
			set
			{
				PreferenceManager.Instance.configuration._voiceVolume = value ;	// 記述をミスると永久再帰ループに陥るので注意すること
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンフィグデータをストレージに書き込む(ショートカットアクセス)
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save()
		{
			return PreferenceManager.Instance.configuration.SaveToStorage() ;
		}

		/// <summary>
		/// コンフィグデータをストレージから読み出す(ショートカットアクセス)
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load()
		{
			return PreferenceManager.Instance.configuration.LoadFromStorage() ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Configuration()
		{
			path = "configuration.bin" ;	// 保存ファイル名を設定
		}
	}
}