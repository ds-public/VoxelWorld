using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

using uGUIHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// ローカイズ情報を保持するクラス Version 2020/02/06 0
	/// </summary>
	public class LocalizeManager : SingletonManagerBase<LocalizeManager>
	{
		//---------------------------------------------------------------------------

		private readonly Dictionary<string,string>	m_StringHash = new Dictionary<string, string>() ;

#if UNITY_EDITOR

		[Serializable]
		public class StringKeyAndValue
		{
			public string Key ;
			public string Value ;
		}


		[SerializeField]
		private readonly List<StringKeyAndValue>	m_StringList = new List<StringKeyAndValue>() ;

#endif

		public enum ErrorProcessTypes
		{
			Defualt			= 0,
			Key				= 1,	// 該当のキー文字列が見つからない場合はキー文字列をそのまま返す
			DefaultValue	= 2,	// 該当のキー文字列が見つからない場合はデフォルト文字列を返す。デフォルト文字列が空の場合はキー文字列を返す。
			WarningValue	= 3,	// 該当のキー文字列が見つからない場合は装飾したデフォルト文字列を返す。デフォルト文字列が空の場合はキー文字列を返す。
		}

		[SerializeField]
		private ErrorProcessTypes	m_ErrorProcessType ;

		/// <summary>
		/// 表示タイプを設定する
		/// </summary>
		public static ErrorProcessTypes	ErrorProcessType
		{
			get
			{
				if( m_Instance == null )
				{
					return ErrorProcessTypes.Defualt ;
				}
				return m_Instance.m_ErrorProcessType ;
			}
			set
			{
				if( m_Instance == null || value == ErrorProcessTypes.Defualt )
				{
					return ;
				}
				m_Instance.m_ErrorProcessType = value ;
			}
		}

		//---------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			UILocalize.SetOnProcess( OnProcessForUI ) ;
		}
		
		/// <summary>
		/// ＵＩ用のローカライズ文字列変換を行う
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private string OnProcessForUI( string key )
		{
			return TEXT_Private( key, "", ErrorProcessTypes.Key ) ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// ローカライズ変換後の文字列を取得する(メソッド名の短縮バージョン)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <param name="errorProcessType"></param>
		/// <returns></returns>
		public static string TEXT( string key, string defaultValue = "", ErrorProcessTypes errorProcessType = ErrorProcessTypes.Defualt )
		{
			if( m_Instance == null )
			{
				return "[LocalizeManager is not create]" ;
			}

			return m_Instance.TEXT_Private( key, defaultValue, errorProcessType ) ;
		}

		private string TEXT_Private( string key, string defaultValue, ErrorProcessTypes errorProcessType )
		{
			if( string.IsNullOrEmpty( key ) == true )
			{
				if( string.IsNullOrEmpty( defaultValue ) == true )
				{
					return "[Unknown String Key And Value]" ;
				}
				else
				{
					return defaultValue ;
				}
			}

			if( m_StringHash.ContainsKey( key ) == false )
			{
				// キーに該当するバリューが存在しない
				ErrorProcessTypes activeErrorProcessType = m_ErrorProcessType ;
				if( errorProcessType != ErrorProcessTypes.Defualt )
				{
					activeErrorProcessType = errorProcessType ;
				}

				if( string.IsNullOrEmpty( defaultValue ) == false )
				{
					if( activeErrorProcessType == ErrorProcessTypes.DefaultValue )
					{
						return defaultValue ;
					}
					else
					if( activeErrorProcessType == ErrorProcessTypes.WarningValue )
					{
						return "[Unknown String Key] " + defaultValue ;
					}
				}

				return key ;
			}

			return m_StringHash[ key ] ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// 全てのプリファレンス系データをロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private() ;
		}

		// 全てのプリファレンス系データをロードする
		private bool Load_Private()
		{
			m_StringHash.Add( "hoge", "ほげ" ) ;

#if UNITY_EDITOR
			foreach( var s in m_StringHash )
			{
				m_StringList.Add( new StringKeyAndValue(){ Key = s.Key, Value = s.Value } ) ;
			}
#endif
			return true ;
		}

		//---------------------------------------------------------------------------

	}

}


