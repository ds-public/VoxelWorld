using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace DBS
{
	/// <summary>
	/// 多目的パラメータ保存用データクラス Version 2017/08/13 0
	/// </summary>
	[Serializable]
	public class Preference : PreferenceBase, ISerializationCallbackReceiver
	{
		/// <summary>
		/// 定義されたキー名の一覧
		/// </summary>
		public static class Key
		{
			public const string MasterDataHash	= "MasterDataHash" ;
		}


		//-------------------------------------------------------------------------------

		/// <summary>
		/// キーとバリュー(直接アクセス) ※インデクサを使う事を推奨
		/// </summary>
		public Dictionary<string,string> hash = new Dictionary<string,string>() ;

		// シリアライズ時のキー展開用
		[SerializeField]
		private string[]	m_Key ;

		// シリアライズ時のバリュー展開用
		[SerializeField]
		private string[]	m_Value ;

		//-------------------------------------------------------------------------------

		/// <summary>
		/// ショートカットアクセス
		/// </summary>
		public static Preference Data
		{
			get
			{
				if( PreferenceManager.Instance == null )
				{
					return null ;
				}
				return PreferenceManager.Instance.preference ;
			}
		}

		/// <summary>
		/// ストレージにセーブする
		/// </summary>
		/// <returns></returns>
		public static bool Save()
		{
			return PreferenceManager.Instance.preference.SaveToStorage() ;
		}

		/// <summary>
		/// ストレージからロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			return PreferenceManager.Instance.preference.LoadFromStorage() ;
		}

		/// <summary>
		/// インデクサでの直接アクセス(内部的には全て文字列として保存されている)
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public string this[ string tKey ]
		{
			get
			{
				if( string.IsNullOrEmpty( tKey ) == true )
				{
					return null ;
				}

				if( hash.ContainsKey( tKey ) == false )
				{
					return null ;
				}

				return hash[ tKey ] ;
			}
			set
			{
				if( string.IsNullOrEmpty( tKey ) == true )
				{
					return ;
				}

				if( hash.ContainsKey( tKey ) == false )
				{
					hash.Add( tKey, value ) ;
				}
				else
				{
					hash[ tKey ] = value ;
				}
			}
		}

		/// <summary>
		/// インデクサでの直接アクセス(内部的には全て文字列として保存されている)
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tType"></param>
		/// <returns></returns>
		public System.Object this[ string tKey, Type tType ]
		{
			get
			{
				if( string.IsNullOrEmpty( tKey ) == true )
				{
					return null ;
				}

				if( hash.ContainsKey( tKey ) == false )
				{
					return null ;
				}

				if( tType == typeof( bool ) )
				{
					bool.TryParse( hash[ tKey ], out bool tValue ) ;
					return tValue ;
				}
				else
				if( tType == typeof( int ) )
				{
					if( int.TryParse( hash[ tKey ], out int tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				else
				if( tType == typeof( uint ) )
				{
					if( uint.TryParse( hash[ tKey ], out uint tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				else
				if( tType == typeof( long ) )
				{
					if( long.TryParse( hash[ tKey ], out long tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				else
				if( tType == typeof( ulong ) )
				{
					if( ulong.TryParse( hash[ tKey ], out ulong tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				else
				if( tType == typeof( float ) )
				{
					if( float.TryParse( hash[ tKey ], out float tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				else
				if( tType == typeof( double ) )
				{
					if( double.TryParse( hash[ tKey ], out double tValue ) == false )
					{
						tValue = 0 ;
					}
					return tValue ;
				}
				
				return hash[ tKey ] ;
			}
			set
			{
				if( string.IsNullOrEmpty( tKey ) == true )
				{
					return ;
				}

				if( hash.ContainsKey( tKey ) == false )
				{
					hash.Add( tKey, value.ToString() ) ;
				}
				else
				{
					hash[ tKey ] = value.ToString() ;
				}
			}
		}

		/// <summary>
		/// 全てのキーとバリューを消去する
		/// </summary>
		public void Clear()
		{
			hash.Clear() ;
		}

		/// <summary>
		/// キーとバリューを追加または更新する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void Add( string tKey, System.Object tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tValue.ToString() ) ;
			}
			else
			{
				hash[ tKey ] = tValue.ToString() ;
			}
		}

		/// <summary>
		/// キーとバリューを追加または更新する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void Add( string tKey, string tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tValue ) ;
			}
			else
			{
				hash[ tKey ] = tValue ;
			}
		}

		//-------------------------------------------------------------------------------

		/// <summary>
		/// 値を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetValue( string tKey, System.Object tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// ブール値を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetBoolean( string tKey, bool tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// ブール値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public bool GetBoolean( string tKey, bool tDefault = false )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( bool.TryParse( tValue, out bool tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 整数を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetInt( string tKey, int tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 整数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public int GetInt( string tKey, int tDefault = 0 )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( int.TryParse( tValue, out int tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 整数を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetUint( string tKey, uint tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 整数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public uint GetUint( string tKey, uint tDefault = 0 )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( uint.TryParse( tValue, out uint tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 整数を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetLong( string tKey, long tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 整数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public long GetLong( string tKey, long tDefault = 0 )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( long.TryParse( tValue, out long tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 整数を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetUlong( string tKey, ulong tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 整数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public ulong GetUlong( string tKey, ulong tDefault = 0 )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( ulong.TryParse( tValue, out ulong tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 浮動小数値を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetFloat( string tKey, float tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 浮動小数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public float GetFloat( string tKey, float tDefault = 0.0f )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( float.TryParse( tValue, out float tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 浮動小数値を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetDouble( string tKey, double tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue.ToString() ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 浮動小数値を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public double GetDouble( string tKey, double tDefault = 0.0f )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			string tValue = hash[ tKey ] ;

			if( double.TryParse( tValue, out double tResult ) == false )
			{
				return tDefault ;
			}
			return tResult ;
		}

		/// <summary>
		/// 文字列を設定する
		/// </summary>
		/// <param name="tKey"></param>
		/// <param name="tValue"></param>
		public void SetString( string tKey, String tValue )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			string tResult = tValue ;
			if( hash.ContainsKey( tKey ) == false )
			{
				hash.Add( tKey, tResult ) ;
			}
			else
			{
				hash[ tKey ] = tResult ;
			}
		}

		/// <summary>
		/// 文字列を取得する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public string GetString( string tKey, string tDefault = "" )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return tDefault ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return tDefault ;
			}
			
			return hash[ tKey ] ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// キーが存在するか確認する
		/// </summary>
		/// <param name="tKey"></param>
		/// <returns></returns>
		public bool HasKey( string tKey )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return false ;
			}

			return hash.ContainsKey( tKey ) ;
		}

		/// <summary>
		/// キーと対応するバリューを削除する
		/// </summary>
		/// <param name="tKey"></param>
		public void DeleteKey( string tKey )
		{
			if( string.IsNullOrEmpty( tKey ) == true )
			{
				return ;
			}

			if( hash.ContainsKey( tKey ) == false )
			{
				return ;
			}

			hash.Remove( tKey ) ;
		}

		/// <summary>
		/// 全てのキーと対応するバリューを削除する
		/// </summary>
		public void DeleteAll()
		{
			hash.Clear() ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Preference()
		{
			path = "preference.bin" ;	// パスを設定
		}

		/// <summary>
		/// シリアライズ前に呼び出される
		/// </summary>
		public void OnBeforeSerialize()
		{
			//シリアライズする際にkeyとvalueをリストに展開
			int i, l = hash.Keys.Count ;

			if( l >  0 )
			{
				m_Key	= new string[ l ] ;
				m_Value	= new string[ l ] ;
	
				hash.Keys.CopyTo( m_Key, 0 ) ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Value[ i ]	= hash[ m_Key[ i ] ] ;
				}
			}
			else
			{
				m_Key	= null ;
				m_Value	= null ;
			}
		}

		/// <summary>
		/// デシリアライズ後に呼び出される
		/// </summary>
		public void OnAfterDeserialize()
		{
			if( m_Key != null && m_Key.Length >  0 && m_Value != null && m_Value.Length >  0 && m_Key.Length == m_Value.Length )
			{
				int i, l = m_Key.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					hash.Add( m_Key[ i ], m_Value[ i ] ) ;
				}
			}
			
			m_Key	= null ;
			m_Value	= null ;
		}
	}
}
