using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;

using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using InputHelper ;

using MathHelper ;

using StorageHelper ;

using DSW.WorldServerClasses ;

namespace DSW.World
{
	/// <summary>
	/// サーバー(プレイヤー管理)
	/// </summary>
	public partial class WorldServer
	{
		//-------------------------------------------------------------------------------------------

		// プレイヤー情報群をロードする
		private WorldPlayerData LoadPlayer( string id )
		{
			string path = m_PlayerRootPath + id ;

			if( StorageAccessor.Exists( path ) != StorageAccessor.Target.File )
			{
				// プレイヤーデータは存在しない
				return null ;
			}

			//----------------------------------------------------------

			byte[] data = StorageAccessor.Load( path ) ;
			if( data == null || data.Length == 0 )
			{
				// 失敗
				return null ;
			}

			var player = DataPacker.Deserialize<WorldPlayerData>( data, false, Settings.DataTypes.Json ) ;
			if( player == null )
			{
				// 失敗
				return null ;
			}

			// ロードに成功したのでアクティブなプレイヤーに追加
			m_Players.Add( player.Id, player ) ;

			// 成功
			return player ;
		}

		// プレイヤーデータをセーブする
		private bool SavePlayer( string id )
		{
			if( m_Players.ContainsKey( id ) == false )
			{
				// 失敗
				return false ;
			}

			return SavePlayer( m_Players[ id ] ) ;
		}

		// プレイヤーデータをセーブする
		private bool SavePlayer( WorldPlayerData player )
		{
			if( player == null )
			{
				// 念のため null チェック
				return false ;
			}

			//----------------------------------

			var data = DataPacker.Serialize( player, false, Settings.DataTypes.Json ) ;
			if( data == null || data.Length == 0 )
			{
				// 失敗
				return false ;
			}

			//----------------------------------------------------------

			string path = m_PlayerRootPath + player.Id ;

			// セーブを実行する
			var result = StorageAccessor.Save( path, data, makeFolder:true ) ;

			if( result == true )
			{
				Debug.Log( "<color=#00FFFF>[SERVER] プレイヤー(PID:" + player.Id + ")を保存しました</color>" ) ;
			}

			return result ;
		}

		// 現在のアクティブなプレイヤーを全てセーブする
		private void SaveAllPlayers( bool withCleanup )
		{
			Debug.Log( "<color=#00FFFF>[SERVER] Save All Players : " + m_Players.Count + "</color>" ) ;

			if( m_Players == null || m_Players.Count == 0 )
			{
				return ;
			}

			//----------------------------------------------------------

			foreach( var player in m_Players.Values )
			{
				if( SavePlayer( player ) == false )
				{
					Debug.LogWarning( "<color=#00FFFF>[SERVER] Player Save Faild : Id = " + player.Id + "</color>" ) ;
				}
			}

			//----------------------------------------------------------

			if( withCleanup == true )
			{
				// 保存と同時に消去する
				m_Players.Clear() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// プレイヤーを取得する(ログイン時用)
		private WorldPlayerData GetPlayerById( string id, string playerName, byte colorType )
		{
			if( string.IsNullOrEmpty( id ) == true )
			{
				// 始めてのログイン
				return null ;
			}

			//----------------------------------
			// 念のためアクティブなプレイヤーを確認する

			WorldPlayerData player = null ;
			if( m_Players.ContainsKey( id ) == true )
			{
				player = m_Players[ id ] ;
			}

			if( player != null )
			{
				// 既にアクティブなプレイヤーになっている(トラブル発生時以外はあまり考えられない)

				// 設定を上書きする
				player.Name			= playerName ;
				player.ColorType	= colorType ;

				return player ;
			}

			//----------------------------------------------------------
			// ストレージに既にプレイヤーデータが存在しているか確認

			player = LoadPlayer( id ) ;

			if( player != null )
			{
				// 設定を上書きする
				player.Name			= playerName ;
				player.ColorType	= colorType ;
			}

			// ロードできたプレイヤーを返す(null でプレイヤーデータが存在しない)
			return player ;
		}

		// 新しいプレイヤーを生成する
		private WorldPlayerData CreateNewPlayer( string playerName, byte colorType )
		{
			Guid guid = Guid.NewGuid() ;
			
			var player = new WorldPlayerData() ;

			//----------------------------------

			// プレイヤー識別子を生成する
			player.Id = guid.ToString( "N" ) ;

			player.Name			= playerName ;
			player.ColorType	= colorType ;

			player.Position = new Vector3( 16.5f, 720, 27.5f ) ;
			player.Direction = new Vector3( 0, 0, 1 ) ;

			//----------------------------------------------------------

			// アクティブプレイヤーに追加する
			m_Players.Add( player.Id, player ) ;

			// ストレージに保存しておく
			if( SavePlayer( player ) == false )
			{
				Debug.LogWarning( "<color=#FF7F00>[SERVER] Player Save Faild : Id = " + player.Id + "</color>" ) ;
			}

			return player ;
		}

		//---------------

		// 既にログイン済み想定でアクティプなプレイヤー群からプレイヤーを取得する
		private WorldPlayerData GetPlayerById( string id )
		{
			if( m_Players.ContainsKey( id ) == false )
			{
				return null ;
			}

			return m_Players[ id ] ;
		}

		//-----------------------------------------------------------

		// プレイヤーを削除する
		private void DeletePlayer( WorldPlayerData player )
		{
			// 削除する
			m_Players.Remove( player.Id ) ;

			//----------------------------------

			// 保存する
			SavePlayer( player ) ;
		}
	}
}
