using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBS
{
	public class Settings : ScriptableObject
	{
		public enum LoadFrom
		{
			Storage,
			Resource,
		}

		// ストレージに探しに行って無ければＣＳＶから読み出す
		public LoadFrom UserDataLocation = LoadFrom.Storage ;


		public enum PlatformType
		{
			Default,	// 現在のプラットフォームに従う
			Mobile,		// デバッグ用に強制的にモバイルモードにする
		}

		public PlatformType SelectedPlatformType = PlatformType.Default ;

/*		public enum EndPoint
		{
			Development,
			Unstable,
			Check,
			Staging,
			Release,
		}

		[Header("接続先のURL")]
		public EndPoint endPoint = EndPoint.Development ;

		//-----------------------------------------------------------

		[SerializeField]
		private string m_Url = "" ;

		public string url
		{
			get
			{
				if( string.IsNullOrEmpty( m_Url ) == false )
				{
					return m_Url ;
				}

				switch( endPoint )
				{
					case EndPoint.Development		: return Define.server_Development	;
					case EndPoint.Unstable			: return Define.server_Unstable		;
					case EndPoint.Check				: return Define.server_Check		;
					case EndPoint.Staging			: return Define.server_Staging		;
					case EndPoint.Release			: return Define.server_Release		;
				}

				return Define.server_Development ;
			}
			set
			{
				m_Url = value ;
			}
		}*/


	}
}
