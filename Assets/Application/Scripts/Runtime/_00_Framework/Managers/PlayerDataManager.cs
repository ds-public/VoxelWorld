using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

//using __p = DSW.PlayerData ;

namespace DSW
{
	/// <summary>
	/// ユーザー系の情報を保持したり処理したりするクラス Version 2021/03/21 0
	/// </summary>
	public class PlayerDataManager : SingletonManagerBase<PlayerDataManager>
	{
		//---------------------------------------------------------------------------
		// モニター用

#if UNITY_EDITOR

		[SerializeField]
		protected string						m_EndPoint ;

		[SerializeField]
		protected long							m_MasterDataVersion ;

		[SerializeField]
		protected string						m_MasterDataPath ;

		[SerializeField]
		protected string						m_MasterDataKey ;

		[SerializeField]
		protected string						m_AssetBundlePath ;

		[SerializeField]
		protected int							m_PlayerId ;

		//-----------------------------------

#endif
		//---------------------------------------------------------------------------

		/// <summary>
		/// ログインする(プレイヤー未登録で利用規約に同意させるのたタイトル画面側で行う)
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> Login()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return await m_Instance.Login_Private() ;
		}

		private async UniTask<bool> Login_Private()
		{
#if false
			// 既にプレイヤー登録済みかを確認する
			string pk = "PlayerId - " + WebAPIManager.EndPoint ;
			int playerId = 0 ;
			if( Preference.HasKey( pk ) == true )
			{
				// 既に登録済み
				playerId = Preference.GetValue<int>( pk ) ;
				Debug.Log( "Load Player Id : " + playerId ) ;
			}

			//----------------------------------------------------------

			// Http Header で使用するので事前に設定しておく(いずれ不要になる)
			PlayerData.PlayerId = playerId ;

			int httpStatus = default ;
			string errorMessage = default ;

			var rd = await WebAPI.User.Login( playerId, ( r1, r2, r3 ) => { httpStatus = r1 ; errorMessage = r2 ; }  ) ;
			Debug.Log( "ResponseCode : " + rd.ResponseCode ) ;
			Debug.Log( "PlayerId : " + rd.PlayerId ) ;

			if( rd.playerId == 0 )
			{
				// プレイヤー登録またはログイン失敗(基本的にはここには来ない)
				await Dialog.Open( "エラー", errorMessage, new string[]{ "閉じる" } ) ;
				return false ;
			}

			if( playerId == 0 && rd.PlayerId >  0 )
			{
				// プレイヤー登録が行われたのでプレイヤー識別子を保存する
				playerId = rd.PlayerId ;

				Preference.SetValue<int>( pk, playerId ) ;
				Preference.Save() ;
				Debug.Log( "Save Player Id : " + playerId ) ;
			}

			// プレイヤー識別子をオンメモリに保存する
			PlayerData.PlayerId = playerId ;

			//----------------------------------------------------------
			// モニタリング

#if UNITY_EDITOR

			m_PlayerId			= PlayerData.PlayerId ;

			UpdatePlayerDataMonitor() ;

#endif
			//----------------------------------------------------------

			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				if( settings.UseBootSettings == true )
				{
					// プレイヤー識別子をプロファイル画面に設定する
					Profile.ShowPlayerId( playerId ) ;
				}
			}

			// 基本情報をチート画面に設定する
			Cheat.SetInformation( playerId, Define.SystemVersion, PlayerData.MasterDataVersion ) ;

			//----------------------------------------------------------

			Debug.Log( "[ログイン成功] PlayerId = " + PlayerData.PlayerId ) ;
#endif
//------------------------------------------------------------------------------------------
#if false
			Settings settings = ApplicationManager.LoadSettings() ;

			// PlayerData を展開する
			if( settings.PlayerDataLoadProcessingType == Settings.PlayerDataLoadProcessingTypes.AlwaysInitialization )
			{
				// 初期化
				if( PlayerDataManager.LoadSystem( true ) == false )
				{
					// 失敗
					return false ;
				}

				// ブート・タイトル以外なのでプレイヤーデータをロードする
				await PlayerDataManager.LoadMamoryAsync( -1 ) ;
			}
			else
			{
				// ロード
				if( PlayerDataManager.LoadSystem( false )== false )
				{
					// 失敗
					return false ;
				}

				// ブート・タイトル以外なのでプレイヤーデータをロードする
				await PlayerDataManager.LoadMamoryAsync( PlayerData.System.LastSelectedIndex ) ;
			}
#endif
			//------------------------------------------------------------------------------------------

			await Yield() ;

			// 成功
			return true ;
		}

		/// <summary>
		/// ログアウトする
		/// </summary>
		public static void LogOut()
		{
//			PlayerData.Clear() ;
		}

		//---------------------------------------------------------------------------

#if UNITY_EDITOR

		/// <summary>
		/// マスターデータの情報をモニター用にセットする
		/// </summary>
		/// <param name="masterDataVersion"></param>
		/// <param name="masterDataKey"></param>
		public static void SetMasterDataInfomation( string endPoint, long masterDataVersion, string masterDataPath, string masterDataKey, string assetBundlePath )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_EndPoint			= endPoint ;
			m_Instance.m_MasterDataVersion	= masterDataVersion ;
			m_Instance.m_MasterDataPath		= masterDataPath ;
			m_Instance.m_MasterDataKey		= masterDataKey ;
			m_Instance.m_AssetBundlePath	= assetBundlePath ;
		}

		/// <summary>
		/// ダウンロードシーンでアセットバンドルパスをデバッグ機能で書き換える可能性があるのでそれの対応用
		/// </summary>
		/// <param name="assetBundlePath"></param>
		public static void SetAssetBundlePath( string assetBundlePath )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_AssetBundlePath	= assetBundlePath ;
		}

		/// <summary>
		/// モニター用のプレイヤーデータを設定する
		/// </summary>
		public static void UpdatePlayerDataMonitor()
		{
			if( m_Instance == null )
			{
				return ;
			}

//			m_Instance.m_System = PlayerData.System ;	// デバッグモニタリング
//			m_Instance.m_Memory = PlayerData.Memory ;	// デバッグモニタリング
		}

#endif

		//---------------------------------------------------------------------------

		/// <summary>
		/// システム部をロードする
		/// </summary>
		/// <param name="isInitialization"></param>
		/// <returns></returns>
		public static bool LoadSystem( bool isInitialization )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.LoadSystem_Private( isInitialization ) ;
		}

		private bool LoadSystem_Private( bool isInitialization )
		{
//			if( __p.SystemData.Load( isInitialization ) == false )
//			{
//				return false ;
//			}
#if UNITY_EDITOR
//			m_System = PlayerData.System ;  // デバッグモニタリング
#endif
			return true ;
		}

		/// <summary>
		/// メモリー部をロードする
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static async UniTask<bool> LoadMamoryAsync( int index )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return await m_Instance.LoadMemoryAsync_Private( index ) ;
		}

		private async UniTask<bool> LoadMemoryAsync_Private( int index )
		{
//			if( await __p.MemoryData.LoadAsync( index ) == false )
//			{
//				return false ;
//			}
#if UNITY_EDITOR
//			m_Memory = PlayerData.Memory ;  // デバッグモニタリング
#endif
			await Yield() ;
			return true ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// セーブする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save( int index = -1 )
		{
//			if( __p.System.SaveToStorage( index ) == false )
//			{
//				return false ;
//			}

//			if( __p.Memory.SaveToStorage( index ) == false )
//			{
//				return false ;
//			}

			return true ;
		}
		
		//---------------------------------------------------------------------------
	}
}

