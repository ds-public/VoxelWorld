using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace DSW
{
	/// <summary>
	/// 起動時の各種設定クラス Version 2022/10/08
	/// </summary>
	[CreateAssetMenu( fileName = "WorldSettingsFile", menuName = "ScriptableObject/DBS/WorldSettingsFile" )]
	public class WorldSettingsFile : ScriptableObject
	{
		[Header( "表示範囲半径(チャンク単位)" ) ]
		public int DISPLAY_CHUNK_RANGE	= 12 ;

		//-----------------------------------

		[Header( "ワールドＸ最小値" )]
		public int WORLD_X_MIN		=     0 ;

		[Header( "ワールドＸ最大値" )]
		public int WORLD_X_MAX		= 65536 ;

		[Header( "ワールドＺ最小値" )]
		public int WORLD_Z_MIN		=	  0 ;

		[Header( "ワールドＺ最大値" )]
		public int WORLD_Z_MAX		= 65536 ;

		[Header( "ワールドＹ最小値" )]
		public int WORLD_Y_MIN		=     0 ;

		[Header( "ワールドＹ最大値" )]
		public int WORLD_Y_MAX		=  1024 ;

		//---------------

		[Header( "チャンクＸ最小値" )]
		public int CHUNK_SET_X_MIN	=    0 ;

		[Header( "チャンクＸ最大値" )]
		public int CHUNK_SET_X_MAX	= 4095 ;

		[Header( "チャンクＺ最小値" )]
		public int CHUNK_SET_Z_MIN	=    0 ;

		[Header( "チャンクＺ最大値" )]
		public int CHUNK_SET_Z_MAX	= 4095 ;

		[Header( "チャンクＹ最小値" )]
		public int CHUNK_Y_MIN		=    0 ;

		[Header( "チャンクＹ最大値" )]
		public int CHUNK_Y_MAX		=   63 ;

		//---------------

		[Header( "プレイヤーアクター色" )]
		public Color32[] PlayerActorColors = new Color32[]
		{
			new Color32( 255,   0,   0, 255 ),
			new Color32(   0,   0, 255, 255 ),
			new Color32( 255, 255,   0, 255 ),
			new Color32(   0, 255,   0, 255 ),
		} ;
	}

	//--------------------------------------------------------------------------------------------
	// 以下ショートカットアクセス

	public class WorldSettings
	{
		private static WorldSettingsFile m_File ;

		private const string m_Path = "ScriptableObjects/WorldSettingsFile" ;

		/// <summary>
		/// ロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			m_File = Resources.Load<WorldSettingsFile>( m_Path ) ;

			return ( m_File != null ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 表示範囲半径(チャンク単位)
		/// </summary>
		public static int		DISPLAY_CHUNK_RANGE	=> m_File.DISPLAY_CHUNK_RANGE ;

		//---------------

		/// <summary>
		/// ワールドＸ最小値
		/// </summary>
		public static int		WORLD_X_MIN			=> m_File.CHUNK_SET_X_MIN ;

		/// <summary>
		/// ワールドＸ最大値
		/// </summary>
		public static int		WORLD_X_MAX			=> m_File.WORLD_X_MAX ;

		/// <summary>
		/// ワールドＺ最小値
		/// </summary>
		public static int		WORLD_Z_MIN			=> m_File.WORLD_Z_MIN ;

		/// <summary>
		/// ワールドＺ最大値
		/// </summary>
		public static int		WORLD_Z_MAX			=> m_File.WORLD_Z_MAX ;

		/// <summary>
		/// ワールドＹ最小値
		/// </summary>
		public static int		WORLD_Y_MIN			=> m_File.WORLD_Y_MIN ;

		/// <summary>
		/// ワールドＹ最大値
		/// </summary>
		public static int		WORLD_Y_MAX			=> m_File.WORLD_Y_MAX ;

		//---------------

		/// <summary>
		/// チャンクＸ最小値
		/// </summary>
		public static int		CHUNK_SET_X_MIN		=> m_File.CHUNK_SET_X_MIN ;

		/// <summary>
		/// チャンクＸ最大値
		/// </summary>
		public static int		CHUNK_SET_X_MAX		=> m_File.CHUNK_SET_X_MAX ;

		/// <summary>
		/// チャンクＺ最小値
		/// </summary>
		public static int		CHUNK_SET_Z_MIN		=> m_File.CHUNK_SET_Z_MIN ;

		/// <summary>
		/// チャンクＺ最大値
		/// </summary>
		public static int		CHUNK_SET_Z_MAX		=> m_File.CHUNK_SET_Z_MAX ;

		/// <summary>
		/// チャンクＹ最小値
		/// </summary>
		public static int		CHUNK_Y_MIN			=> m_File.CHUNK_Y_MIN ;

		/// <summary>
		/// チャンクＹ最大値
		/// </summary>
		public static int		CHUNK_Y_MAX			=> m_File.CHUNK_Y_MAX ;

		//---------------

		/// <summary>
		/// プレイヤーアクター色
		/// </summary>
		public static Color32[]	PlayerActorColors	=> m_File.PlayerActorColors ;

		//-------------------------------------------------------------------------------------------

	}


}
