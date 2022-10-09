using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;
using System.Linq ;

using Cysharp.Threading.Tasks ;
using Cysharp.Threading.Tasks.CompilerServices;

using UnityEngine ;

using CSVHelper ;
using StorageHelper ;

//using DBS.MasterData ;

namespace DBS
{
	/// <summary>
	/// マスターデータの管理を行うクラス Version 2022/09/19 0
	/// </summary>
	public class MasterDataManager : SingletonManagerBase<MasterDataManager>
	{
		//---------------------------------------------------------------------------
		// モニター用

#if UNITY_EDITOR

#if false
		[SerializeField]
		protected List<PlayerClassData>			m_PlayerClassData ;

		[SerializeField]
		protected List<PlayerExperienceData>	m_PlayerExperienceData ;

		[SerializeField]
		protected List<EnemyUnitData>			m_EnemyUnitData ;

		[SerializeField]
		protected List<EnemyTeamData>			m_EnemyTeamData ;

		[SerializeField]
		protected List<ItemData>				m_ItemData ;

		[SerializeField]
		protected List<EquipmentData>			m_EquipmentData ;

		[SerializeField]
		protected List<GoodsData>				m_GoodsData ;

		[SerializeField]
		protected List<SkillData>				m_SkillData ;

		[SerializeField]
		protected List<InfluenceData>			m_InfluenceData ;

		[SerializeField]
		protected List<EffectData>				m_EffectData ;
#endif

#endif

		//---------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶからマスターデータを展開する(同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<T> LoadFromCsv<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn ) where T : class, new()
		{
			var data = Zip.Decompress( m_MasterDataFile, path ) ;
			if( data == null || data.Length == 0 )
			{
				return null ;
			}

			var text = Encoding.UTF8.GetString( data ) ;
			if( string.IsNullOrEmpty( text ) == true )
			{
				return null ;
			}

//			TextAsset ta = Asset.Load<TextAsset>( path ) ;
//			if( ta != null )
//			{
				List<T> list = new List<T>() ;
				if( CSVObject.Load<T>( text, ref list, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) == true )
				{
					// 展開成功
					return list ;
				}
//			}
			return null ;
		}
#if false
		/// <summary>
		/// ＣＳＶからマスターデータを展開する(非同期)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onAction"></param>
		/// <returns></returns>
		public static async UniTask<List<T>> LoadFromCsvAsync<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn, Action<List<T>> onAction ) where T : class, new()
		{
			return await m_Instance.LoadFromCsvAsync_Private<T>( path, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn, onAction ) ;
		}

		private async UniTask<List<T>> LoadFromCsvAsync_Private<T>( string path, int startColumn, int startRow, int nameRow, int typeRow, int dataRow, int keyColumn, Action<List<T>> onAction ) where T : class, new()
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				ta = await Asset.LoadAsync<TextAsset>( path ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				List<T> list = new List<T>() ;
				if( CSVObject.Load<T>( ta.text, ref list, startColumn, startRow, nameRow, typeRow, dataRow, keyColumn ) == true )
				{
					// 展開成功
					onAction?.Invoke( list ) ;
					return list ;
				}
			}

			Debug.LogWarning( "Failed to load = " + typeof( T ).ToString() ) ;
			return null ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------

		private static byte[] m_MasterDataFile ;

		/// <summary>
		/// マスターデータをダウンロードする
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> DownloadAsync( bool useProgress = false, Action<float> onProgress = null )
		{
			return await m_Instance.DownloadAsync_Private( useProgress, onProgress ) ;
		}

		private async UniTask<bool> DownloadAsync_Private( bool usePrpgress, Action<float> onProgress )
		{
			bool isDone = false ;
			string errorMessage = string.Empty ;

			ApplicationManager.Instance.StartCoroutine(	StorageAccessor.LoadFromStreamingAssetsAsync( "dbs/MasterData.bin", ( byte[] data, string error ) =>
			{
				m_MasterDataFile = data ;
				errorMessage = error ;
				isDone = true ;
			} ) ) ;

			await WaitUntil( () => ( isDone == true ) ) ;

			if( m_MasterDataFile == null || string.IsNullOrEmpty( errorMessage ) == false )
			{
				Debug.LogWarning( "MasterData Load Failure : " + errorMessage ) ;
				return false ;
			}

			Debug.Log( "<color=#00FFFF>MasterData Load OK : Size = " + m_MasterDataFile.Length + "</color>" ) ;

			//----------------------------------------------------------
#if false
			string key = "MasterDataVersion" ;
			string masterDataFileName = "MasterData" ;

			// マスターデータのバージョンを取得する(取得できればマスターデータが存在する)
			long masterDataVersion = 0 ;
			if( Preference.HasKey( key ) == true )
			{
				masterDataVersion = Preference.GetValue<long>( key ) ;
			}
			if( StorageAccessor.GetSize( masterDataFileName ) <= 0 )
			{
				// ファイルが存在しないためバージョンを強制的に 0 にする
				masterDataVersion = 0 ;
			}

			// 現在ストレージに保存されているマスターデータのバージョンをオンメモリにも保存する(あまり意味は無い)
			PlayerData.MasterDataVersion = masterDataVersion ;

			//----------------------------------------------------------

			// バージョンチェック
			int httpStatus		= default ;
			string errorMessage	= default ;
			WebAPIs.Maintenance.CheckVersion_Response response = default ;

			while( true )
			{
				httpStatus		= default ;
				errorMessage	= default ;
				response = await WebAPI.Maintenance.CheckVersion( ( r1, r2, r3 ) => { httpStatus = r1 ; errorMessage = r2 ; }, isCheckVersion:false ) ;

				// 通信に成功しない限りここにはこない

				Debug.Log( "ResponseCode : " + response.ResponseCode ) ;

				if( response.ResponseCode == Enums.ResponseCodes.Redirect )
				{
					// クライアントのシステムバージョンがサーバー環境よりも新しいので別のサーバー環境にリダイレクトする(Apple審査用)
					string redirectEndPoint = response.EndPoint ;
					if( redirectEndPoint.IsNullOrEmpty() == true )
					{
						// クライアントバージョンが古い
						await Dialog.Open( "注意", "リダイレクト用のエンドポイントが異常です", new string[]{ "再起動" } ) ;
						ApplicationManager.DownloadingState = 0 ;	// ダウンロードプロセスはリセットする

						// リブートを実行する
						ApplicationManager.Reboot() ;
						return false ;
					}

					Debug.LogWarning( "<リダイレクトを行う> : " + redirectEndPoint ) ;

					// リダイレクト用のエンドポイントを設定して再度エンドポイントにアクセスする
					PlayerData.EndPoint = redirectEndPoint ;
					WebAPIManager.EndPoint = PlayerData.EndPoint ;
				}
				else
				{
					// 通常のエラーチェックフェーズへ
					break ;
				}
			}

			//----------------------------------------------------------

			// クライアントバージョンの確認
			if( response.ResponseCode == Enums.ResponseCodes.SystemVersionIsOld )
			{
				// クライアントバージョンが古い
				await Dialog.Open( "注意", "クライアントバージョンが古いです", new string[]{ "ストアへ移動" } ) ;

				// アプリのダウンロードサイトへ誘導
				Debug.Log( "StoreURL : " + response.StoreUrl ) ;
				ApplicationManager.OpenURL( response.StoreUrl ) ;

				return false ;
			}

			//----------------------------------------------------------

			string assetBundlePath = response.AssetBundlePath ;
			if( string.IsNullOrEmpty( assetBundlePath ) == false )
			{
				// 最後にスラッシュがあれば削る
				assetBundlePath = assetBundlePath.TrimEnd( '/' ) ;
			}

			Debug.Log( "MasterDataVersion : " + response.MasterDataVersion ) ;
			Debug.Log( "MasterDataPath : " + response.MasterDataPath ) ;
			Debug.Log( "MasterDataKey : " + response.MasterDataKey ) ;
			Debug.Log( "AssetBundlePath : " + assetBundlePath ) ;

			//----------------------------------

			// マスターデータのバージョンをオンメモリにも保存する
			PlayerData.MasterDataVersion	= response.MasterDataVersion ;

			// マスターデータのキーをオンメモリにのみ保存する(アプリ起動中のみ有効)
			PlayerData.MasterDataKey		= response.MasterDataKey ;

			// アセットバンドルパスをオンメモリのみに保存する(アプリ起動中のみ有効)
			PlayerData.AssetBundlePath		= assetBundlePath ;

			//----------------------------------

			byte[] masterData ;

			// マスターデータバージョンの確認
			if( response.ResponseCode == Enums.ResponseCodes.MasterVersionNotEqual )
			{
				// マスターデータの更新が必要

				// マスターデータのダウンロードを行う(失敗したらリブートになる)
				masterData = await Download.ToBytes
				(
					response.MasterDataPath,
					onProgress : ( int offset, int length ) =>
					{
						if( length >  0 )
						{
							onProgress?.Invoke( ( float )offset / ( float )length ) ;
						}
					},
					useProgress: false,
					title:"マスターデータのダウンロード",
					message:"マスタデータのダウンロードに\n失敗しました"
				) ;

				// 成功した場合のみここに来る
				Debug.Log( "----->マスターデータのサイズ:" + masterData.Length ) ;

				// マスターデータを保存する
				bool result = false ;
				string keyAlternane    = PlayerData.KeyAlternate ;
				string vectorAlternane = PlayerData.VectorAlternate ;

#if UNITY_EDITOR
				// UnityEditor では暗号化しない
				keyAlternane    = null ;
				vectorAlternane = null ;
#endif
				// Key と Vector は 16文字である必要がある
				if( StorageAccessor.Save( masterDataFileName, masterData, false, keyAlternane, vectorAlternane ) == true )
				{
					// マスターデータのバージョンを保存する
					Preference.SetValue<long>( key, response.MasterDataVersion ) ;
					if( Preference.Save() == true )
					{
						result = true ;	// マスターデータの保存に成功した
					}
				}

				if( result == false )
				{
					await Dialog.Open( "マスターデータの保存", "マスターデータの保存に失敗しました\n\n再起動します", new string[]{ "再起動" } ) ;
					ApplicationManager.DownloadingState = 0 ;	// ダウンロードプロセスはリセットする

					// リブートを実行する
					ApplicationManager.Reboot() ;
					return false ;
				}
			}
			else
			{
				Debug.Log( "< マスターデータの更新不要 >" ) ;
			}

			//----------------------------------------------------------

			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				if( settings.UseBootSettings == true )
				{
					string endPoint = WebAPIManager.EndPoint ;

					// サーバー環境の表示を設定する
					SetEnvironment( endPoint ) ;

					// プレイヤー識別子を設定する
					SetPlayerId( endPoint ) ;
				}
			}

#if UNITY_EDITOR

			// モニター用にエンドポイントとマスターデータバージョンとマスターデータパスとマスターデータキーを設定する
			PlayerDataManager.SetMasterDataInfomation( WebAPIManager.EndPoint, response.MasterDataVersion, response.MasterDataPath, response.MasterDataKey, assetBundlePath ) ;

#endif
			//----------------------------------------------------------
#endif
			onProgress?.Invoke( 1 ) ;

			await Yield() ;

			// 成功
			return true ;
		}

		/// <summary>
		/// 接続サーバー環境の情報を設定する
		/// </summary>
		private void SetEnvironment( string endPoint )
		{
#if false
			var settings = ApplicationManager.LoadSettings() ;
			if( settings == null )
			{
				return ;
			}

			var endPoints = settings.WebAPI_EndPoints ;

			List<string> endPointNames = new List<string>(){ "任意サーバー" } ;
			endPointNames.AddRange( endPoints.Select( _ => _.Name ) ) ;

			int endPointIndex = GetIndex( endPoint ) ;

			Profile.ShowEnvironment( endPointNames[ endPointIndex ] ) ;

			//---------------------------------
			// インナーメソッド：インデックスを検査する
			int GetIndex( string path )
			{
				int p = 0 ;

				int i, l = endPoints.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( path == endPoints[ i ].Path )
					{
						p = 1 + i ;	// 発見
						break ;
					}
				}

				return p ;
			}
#endif
		}

		/// <summary>
		/// ローカルに保存されたプレイヤー識別子の情報を設定する
		/// </summary>
		/// <param name="endPoint"></param>
		private void SetPlayerId( string endPoint )
		{
#if false
			string pk = "PlayerId - " + endPoint ;
			int playerId = 0 ;
			if( Preference.HasKey( pk ) == true )
			{
				// 既に登録済み
				playerId = Preference.GetValue<int>( pk ) ;
			}

			Profile.ShowPlayerId( playerId ) ;
#endif
		}

		//-------------------------------------------------------------------------------------------
#if false
		/// <summary>
		/// マスターデータをメモリに展開する
		/// </summary>
		/// <returns></returns>
		public static async UniTask<bool> LoadAsync( Action<float> onProgress = null )
		{
			return await m_Instance.LoadAsync_Private( onProgress ) ;
		}

		private async UniTask<bool> LoadAsync_Private( Action<float> onProgress )
		{
			// マスターデータを展開する
			string keyAlternane    = PlayerData.KeyAlternate ;
			string vectorAlternane = PlayerData.VectorAlternate ;
#if UNITY_EDITOR
			// UnityEditor では暗号化しない
			keyAlternane    = null ;
			vectorAlternane = null ;
#endif
			// Key と Vector は 16文字である必要がある
			byte[] masterData = StorageAccessor.Load( "MasterData", keyAlternane, vectorAlternane ) ;
			if( masterData == null )
			{
				// 失敗(ありえない)
				return false ;
			}

			( string, long )[] files = Zip.GetFiles( masterData, PlayerData.MasterDataKey ) ;
			if( files == null || files.Length == 0 )
			{
				// 失敗(ありえない)
				return false ;
			}

			//----------------------------------------------------------

			int index, count = files.Length ;

#if UNITY_EDITOR
			string lm = "-----マスターデータの展開状況↓: 対象ファイル数[ " + count + " ]" ;
#endif
			for( index  = 0 ; index <  count ; index ++ )
			{
				var file = files[ index ] ;
				( string fileName, long fileSize ) = ( file.Item1, file.Item2 ) ;
//				Debug.Log( "ファイル:" + fileName + " " + fileSize ) ;

				if( m_MasterDataFiles.ContainsKey( fileName ) == true )
				{
					if( DecompressAndDeserialize( masterData, fileName, PlayerData.MasterDataKey, m_MasterDataFiles[ fileName ] ) == true )
					{
						// マスターデータの展開成功
#if UNITY_EDITOR
						lm += "\n" + "成功 -> " + fileName ;
#endif
						onProgress?.Invoke( ( float )( index + 1 ) / ( float )count ) ;
					}
					else
					{
						await Dialog.Open( "Error", "マスターデータデシリアライズに失敗しました\n" + fileName, new string[]{ "閉じる" } ) ;
					}
				}
				else
				{
					await Dialog.Open( "Error", "マスターデータに対応するデシリアライザが\n登録されていません\n" + fileName, new string[]{ "閉じる" } ) ;
				}

//				await WaitForSeconds( 1 ) ;
			}

#if UNITY_EDITOR
			Debug.Log( lm ) ;
#endif

#if false
			int i, l ;

			Debug.Log( "-----モンスターデータのレコード数:" + MasterData.MonsterData.Records.Count ) ;

			l = MasterData.MonsterData.Records.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Debug.LogWarning( "名前:[" + i + "] " + MasterData.MonsterData.Records[ i ].Name ) ;
			}
#endif
			//------------------------------------------------------------------------------------------

			// インデックス生成が必要なテーブルはインデックスの生成を行う

//			MonsterData.CreateIndices() ;	// 使用されなくなる

			MonsterBaseData.CreateIndices() ;
			BloodBaseData.CreateIndices() ;

//			MonsterSkillData.CreateIndices() ;

			AccessoryData.CreateIndices() ;

			SkillBaseData.CreateIndices() ;

			StatusEffectData.CreateIndices() ;

			MonsterLifespanData.CreateIndices() ;

			FeedData.CreateIndices() ;

			WorkingData.CreateIndices() ;
			TrainingData.CreateIndices() ;

			TournamentData.CreateIndices() ;
			TournamentScheduleData.CreateIndices() ;

			MonsterNpcData.CreateIndices() ;

			ItemData.CreateIndices() ;

			ReleaseTimeData.CreateIndices() ;
			ReleaseSpanData.CreateIndices() ;

			//------------------------------------------------------------------------------------------
			// 以下はローカルのデバッグ用(強制的にローカルのマスターデータを上書きする)

			//----------------------------------------------------------
			// モニター用

#if UNITY_EDITOR
			SetMonitor() ;
#endif
			// 成功
			return true ;
		}

		// マスターデータをデシリアライズする
		private bool DecompressAndDeserialize( byte[] masterData, string fileName, string masterDataKey, Func<byte[],bool> onDecompressed )
		{
			byte[] data = Zip.Decompress( masterData, fileName, masterDataKey ) ;
			if( data == null || data.Length == 0 )
			{
				return false ;	// 伸長失敗
			}

			if( onDecompressed == null )
			{
				return false ;	// 伸長失敗
			}

			return onDecompressed( data ) ;
		}
#endif
		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// 全てのマスターデータをロードする
		/// </summary>
		/// <returns></returns>
		public static async UniTask LoadAsync()
		{
			await m_Instance.LoadAsync_Private() ;
		}

		private async UniTask LoadAsync_Private()
		{
			//----------------------------------------------------------
			// 個々のテーブルを展開する
#if false
			PlayerClassData.Load( PlayerClassData.Path ) ;
			PlayerClassData.CreateIndices() ;

			PlayerExperienceData.Load( PlayerExperienceData.Path ) ;
			PlayerExperienceData.CreateIndices() ;
			PlayerExperienceData.Prepare() ;

			EnemyUnitData.Load( EnemyUnitData.Path ) ;
			EnemyUnitData.CreateIndices() ;

			EnemyTeamData.Load( EnemyTeamData.Path ) ;
			EnemyTeamData.CreateIndices() ;

			ItemData.Load( ItemData.Path ) ;
			ItemData.CreateIndices() ;

			EquipmentData.Load( EquipmentData.Path ) ;
			EquipmentData.CreateIndices() ;

			GoodsData.Load( GoodsData.Path ) ;
			GoodsData.CreateIndices() ;
			GoodsData.Prepare() ;

			SkillData.Load( SkillData.Path ) ;
			SkillData.CreateIndices() ;

			InfluenceData.Load( InfluenceData.Path ) ;
			InfluenceData.CreateIndices() ;

			EffectData.Load( EffectData.Path ) ;
			EffectData.CreateIndices() ;

			UndefinedEquipmentNameData.Load( UndefinedEquipmentNameData.Path ) ;
			UndefinedEquipmentNameData.CreateIndices() ;

			//----------------------------------------------------------

			// 特殊なマスターデータの読み出し
			ActionPatternData.LoadAll( m_MasterDataFile ) ;
#endif
			//----------------------------------------------------------
			// デバッグモニター用
#if UNITY_EDITOR

#if false
			m_PlayerClassData		= PlayerClassData.Records ;

			m_PlayerExperienceData	= PlayerExperienceData.Records ;

			m_EnemyUnitData			= EnemyUnitData.Records ;

			m_EnemyTeamData			= EnemyTeamData.Records ;

			m_ItemData				= ItemData.Records ;

			m_EquipmentData			= EquipmentData.Records ;

			m_GoodsData				= GoodsData.Records ;

			m_SkillData				= SkillData.Records ;

			m_InfluenceData			= InfluenceData.Records ;

			m_EffectData			= EffectData.Records ;
#endif

#endif
			//--------------------------------------------------------------------------
			// ワークデータを展開する(後でマスターデータに移行する)

			//--------------------------------------------------------------------------

			await Yield() ;
		}
	}
}
