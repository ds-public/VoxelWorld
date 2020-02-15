using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	[Serializable]
	public class ScreenAttribute
	{
		public string	AttributeName ;
		public string	ScreenName ;
		public string[]	LayoutNames ;
		public string	CategoryName ;

		public bool		State ;
		public bool		Level ;
		public string	Title ;
		public string	Description ;
		public bool		Back ;
		public bool		Home ;

		public string	BgmName ;

		public int		Type ;

		public ScreenAttribute( string attributeName, string screenName, string[] layoutNames, string categoryName, bool state, bool level, string title, string description, bool back, bool home, string bgmName, int type )
		{
			AttributeName	= attributeName ;	// アトリビュート名 
			ScreenName		= screenName ;		// スクリーンファイル名
			LayoutNames		= layoutNames ;		// レイアウトファイル名
			CategoryName	= categoryName ;	// 黒フェードを実行するかどうかの判定

			State			= state ;			// プレイヤー状態の表示の有無
			Level			= level ;			// プレイヤー強度の表示の有無
			Title			= title ;			// タイトル表示名
			Description		= description ;	// タイトル説明文
			Back			= back ;			// バックボタンの有無
			Home			= home ;			// ホームボタンの有無

			BgmName			= bgmName ;		// ＢＧＭ名

			Type			= type ;			// シーンのタイプ(0=シンプル・1=SceneManagerのフェード機構利用)
		}

		//-------------------------------------------------------------------------------------------

		public class Name
		{
			//----------------------------------------------------------
			// Common

			public const string UnitEquipment						= "UnitEquipment" ;


			//----------------------------------------------------------
			// Organize

			public const string	Organize_UnitEquipment				= "Organize_UnitEquipment" ;

			//----------------------------------------------------------
			// Quest

			public const string Quest_UnitEquipment					= "Quest_UnitEquipment" ;
		}
		
		/// <summary>
		/// アトリビート名からスクリーンアトリビュートデータを取得する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public static ScreenAttribute GetByAttributeName( string attributeName )
		{
			int i, l = Data.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Data[ i ].AttributeName == attributeName )
				{
					return Data[ i ] ;
				}
			}

			return null ;
		}

		
		/// <summary>
		/// スクリーン名からスクリーンアトリビュートデータを取得する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public static ScreenAttribute GetByScreenName( string screenName )
		{
			int i, l = Data.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Data[ i ].ScreenName == screenName )
				{
					return Data[ i ] ;
				}
			}

			return null ;
		}


		/// <summary>
		/// スクリーンの構成情報
		/// </summary>
		public static ScreenAttribute[] Data =
		{
			new ScreenAttribute( Scene.Screen.Title,						Scene.Screen.Title,
				null,
				"Title",		false,	false,	"",		"",
				false,	false,	null,			-1 ),

			new ScreenAttribute( Scene.Screen.World,						Scene.Screen.World,
				null,
				"World",		false,	false,	"",		"",
				false,	false,	null,			-1 ),


			//----------------------------------------------------------
			// Template

			new ScreenAttribute( Scene.Screen.Template,							Scene.Screen.Template,	
				null,
				"Template",			true,	false,	"テンプレート",				"テンプレートです",
				true,	true,	BGM.Home,		1 ),

		} ;
	}
}
