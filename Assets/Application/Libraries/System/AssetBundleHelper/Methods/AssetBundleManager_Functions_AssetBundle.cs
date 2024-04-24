using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Playables ;
using UnityEngine.U2D ;
using UnityEngine.Video ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditor.SceneManagement ;
#endif


/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager
	{
		// AssetBundleManager :: Asset

#if UNITY_EDITOR

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private ( string, Type )[] GetLocalAllAssetPaths( string localAssetsRootPath, string folderPath, bool isOriginal )
		{
			folderPath = $"{localAssetsRootPath}{folderPath}" ;
			if( folderPath[ ^1 ] != '/' )
			{
				folderPath += "/" ;
			}

			var allAssetPaths = new List<( string, Type )>() ;

			GetLocalAllAssetPaths_Recursion( folderPath, folderPath, ref allAssetPaths, isOriginal ) ;

			return allAssetPaths.ToArray() ;
		}

		// 再帰
		private void GetLocalAllAssetPaths_Recursion( string folderPath, string currentFolderPath, ref List<(string,Type)> allAssetPaths, bool isOriginal )
		{
			int i, l ;

			//--------------

			var paths = Directory.GetFiles( currentFolderPath ) ;
			if( paths != null && paths.Length >  0 )
			{
				// アセットバンドル部のパスと拡張子を削除する
				string path, extension ;
				int i0, i1 ;

				l = paths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = paths[ i ].Replace( "\\", "/" ) ;
					path = path.Replace( folderPath, "" ) ;

					// 実環境に合わせるために拡張子を全て小文字にする
					if( isOriginal == false )
					{
						path = path.ToLower() ;
					}

					i0 = path.LastIndexOf( '/' ) ;
					i1 = path.LastIndexOf( '.' ) ;
					if( i1 <= i0 )
					{
						// 拡張子なし
						allAssetPaths.Add( ( path, typeof( System.Object ) ) ) ;
					}
					else
					{
						// 拡張子あり
						extension = path[ i1.. ] ;
						if( extension.Contains( "meta" ) == false )
						{
							path = path[ ..i1 ] ;
							allAssetPaths.Add( ( path, GetTypeFromExtension( extension ) ) ) ;
						}
					}
				}
			}

			// サブフォルダを検索する
			var folderPaths = Directory.GetDirectories( currentFolderPath ) ;
			if( folderPaths != null && folderPaths.Length >  0 )
			{
				l = folderPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					GetLocalAllAssetPaths_Recursion( folderPath, folderPaths[ i ].Replace( "\\", "/" ), ref allAssetPaths, isOriginal ) ;
				}
			}
		}

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string[] GetLocalAllAssetPaths( string localAssetsRootPath, string folderPath, Type type, bool isOriginal )
		{
			folderPath = $"{localAssetsRootPath}{folderPath}" ;
			if( folderPath[ ^1 ] != '/' )
			{
				folderPath += "/" ;
			}

			if( type == null )
			{
				// 指定なし
				type = typeof( UnityEngine.Object ) ;
			}

			var allAssetPaths = new List<string>() ;

			GetLocalAllAssetPaths_Recursion( folderPath, folderPath, type, ref allAssetPaths, isOriginal ) ;

			return allAssetPaths.ToArray() ;
		}

		// 再帰
		private void GetLocalAllAssetPaths_Recursion( string folderPath, string currentFolderPath, Type type, ref List<string> allAssetPaths, bool isOriginal )
		{
			int i, l ;

			//--------------

			var paths = Directory.GetFiles( currentFolderPath ) ;
			if( paths != null && paths.Length >  0 )
			{
				// アセットバンドル部のパスと拡張子を削除する
				string path, extension ;
				int i0, i1 ;

				l = paths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					path = paths[ i ].Replace( "\\", "/" ) ;
					path = path.Replace( folderPath, "" ) ;

					// 実環境に合わせるために拡張子を全て小文字にする
					if( isOriginal == false )
					{
						path = path.ToLower() ;
					}

					i0 = path.LastIndexOf( '/' ) ;
					i1 = path.LastIndexOf( '.' ) ;
					if( i1 <= i0 )
					{
						// 拡張子なし
//						allAssetPaths.Add( ( path, typeof( System.Object ) ) ) ;
					}
					else
					{
						// 拡張子あり
						extension = path[ i1.. ] ;
						if( extension.Contains( "meta" ) == false )
						{
							if( GetTypeFromExtension( extension ) == type )
							{
								path = path[ ..i1 ] ;
								allAssetPaths.Add( path ) ;
							}
						}
					}
				}
			}

			// サブフォルダを検索
			var folderPaths = Directory.GetDirectories( currentFolderPath ) ;
			if( folderPaths != null && folderPaths.Length >  0 )
			{
				l = folderPaths.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					GetLocalAllAssetPaths_Recursion( folderPath, folderPaths[ i ].Replace( "\\", "/" ), type, ref allAssetPaths, isOriginal ) ;
				}
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスからアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private UnityEngine.Object LoadLocalAsset( string localAssetsRootPath, string path, Type type )
		{
			path = $"{localAssetsRootPath}{path}" ;
			
			// 最初はそのままロードを試みる
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath( path, type ) ;
			if( asset != null )
			{
				// 成功したら終了
				return asset ;
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				List<string> extensions ;

				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般タイプ
					extensions = m_TypeToExtension[ type ] ;
				}
				else
				{
					// 不明タイプ
					extensions = m_UnknownTypeToExtension ;
				}

				foreach( string extension in extensions )
				{
					asset = AssetDatabase.LoadAssetAtPath( path + extension, type ) ;
					if( asset != null )
					{
						return asset ;
					}
				}

//				Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
				return null ;
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスからアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private UnityEngine.Object[] LoadLocalAllAssets( string localAssetsRootPath, string folderPath, Type type )
		{
			folderPath = $"{localAssetsRootPath}{folderPath}" ;
			
			if( Directory.Exists( folderPath ) == false )
			{
				return null ;
			}

			if( type == null )
			{
				// 指定なし
				type = typeof( UnityEngine.Object ) ;
			}

			var temporaryAssets = new List<UnityEngine.Object>() ;

			// 再帰でアセットを取得する
			LoadLocalAllAssets_Recursion( folderPath, type, ref temporaryAssets ) ;

			if( temporaryAssets.Count == 0 )
			{
				return null ;
			}

			return temporaryAssets.ToArray() ;
		}

		// 再帰でサブフォルダのアセットも取得する
		private void LoadLocalAllAssets_Recursion( string currentPath, Type type, ref List<UnityEngine.Object> temporaryAssets )
		{
			var filePaths = Directory.GetFiles( currentPath ) ;
			if( filePaths != null && filePaths.Length >  0 )
			{
				foreach( var filePath in filePaths )
				{
					UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath( filePath, type ) ;
					if( asset != null )
					{
						temporaryAssets.Add( asset ) ;
					}
				}
			}

			var folderPaths = Directory.GetDirectories( currentPath ) ;
			if( folderPaths != null && folderPaths.Length >  0 )
			{
				foreach( var folderPath in folderPaths )
				{
					LoadLocalAllAssets_Recursion( folderPath, type, ref temporaryAssets ) ;
				}
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスからサブアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private UnityEngine.Object LoadLocalSubAsset( string localAssetsRootPath, string path, string subAssetName, Type type )
		{
			path = $"{localAssetsRootPath}{path}" ;
			
			// 最初はそのままロードを試みる
			var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path ) ;
			if( assets != null && assets.Length >  0 )
			{
				// 成功したら終了
				var asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
				return ( asset != null && asset.GetType() == type ) ? asset : null ;
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				List<string> extensions ;

				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般タイプ
					extensions = m_TypeToExtension[ type ] ;
				}
				else
				{
					// 不明タイプ
					extensions = m_UnknownTypeToExtension ;
				}

				foreach( string extension in extensions )
				{
					assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + extension ) ;
					if( assets != null && assets.Length >  0 )
					{
						// 成功したら終了
						var asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
						return ( asset != null && asset.GetType() == type ) ? asset : null ;
					}
				}

				Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
				return null ;
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスから全てのサブアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		private UnityEngine.Object[] LoadLocalAllSubAssets( string localAssetsRootPath, string path, Type type )
		{
			path = $"{localAssetsRootPath}{path}" ;
			
			// 最初はそのままロードを試みる
			var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path ) ;
			if( assets != null && assets.Length >  0 )
			{
				// 成功したら終了
				assets = assets.Where( _ => _.GetType() == type ).ToArray() ;
				if( assets != null && assets.Length >  0 )
				{
					return assets ;
				}
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				List<string> extensions ;

				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般タイプ
					extensions = m_TypeToExtension[ type ] ;
				}
				else
				{
					// 不明タイプ
					extensions = m_UnknownTypeToExtension ;
				}

				foreach( string extension in extensions )
				{
					assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + extension ) ;
					if( assets != null && assets.Length >  0 )
					{
						// 成功したら終了
						assets = assets.Where( _ => _.GetType() == type ).ToArray() ;
						if( assets != null && assets.Length >  0 )
						{
							return assets ;
						}
					}
				}

				Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
				return null ;
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットパスからシーンをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private IEnumerator OpenLocalSceneAsync( string localAssetsRootPath, string path, string sceneName, Type type, UnityEngine.SceneManagement.LoadSceneMode mode, Action<string> onError )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				onError?.Invoke( "Bad scene name." ) ;
				yield break ;
			}
			
			//----------------------------------------------------------

			if( type != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindAnyObjectByType( type ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------

			path = $"{localAssetsRootPath}{path}" ;

			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				path += ".unity" ;
			}

			EditorSceneManager.LoadSceneInPlayMode( path, new UnityEngine.SceneManagement.LoadSceneParameters( mode ) ) ;
			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.IsValid() == false )
			{
				onError?.Invoke( "Could not load." ) ;
				yield break ;
			}
		}

		//---------------

		/// <summary>
		/// ファイルまたはフォルダが存在するか確認する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool ContainsLocalAsset( string localAssetsRootPath, string path )
		{
			path = $"{localAssetsRootPath}{path}" ;

			bool result = ( Directory.Exists( path ) == true ) | ( File.Exists( path ) ) ;
			if( result == true )
			{
				return true ;
			}

			//----------------------------------
			// アセットバンドルが１ファイルから構成されている場合はフォルダではヒットしないためファイル名部分が一致するものを検索する

			int i = path.LastIndexOf( '/' ) ;
			if( i <  0 )
			{
				// フォルダ部分が無いため一致は無い
				return false ;
			}

			string folderPath = path[ ..i ] ;
			if( string.IsNullOrEmpty( folderPath ) == true )
			{
				// フォルダ部分が無いため一致は無い
				return false ;
			}

			if( Directory.Exists( folderPath ) == false )
			{
				// フォルダが存在しない
				return false ;
			}

			var filePaths = Directory.GetFiles( folderPath ) ;
			if( filePaths == null || filePaths.Length == 0 )
			{
				// フォルダ内にファイルが無いため一致は無い
				return false ;
			}

			foreach( var filePath in filePaths )
			{
				if( filePath.Replace( '\\', '/' ).IndexOf( path ) == 0 )
				{
					// 一致有り
					return true ;
				}
			}

			// 一致無し
			return false ;
		}

		/// <summary>
		/// ローカルアセットの環境パスを取得する
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string GetLocalAssetFilePath( string localAssetsRootPath, string path )
		{
			path = $"{localAssetsRootPath}{path}" ;

			if( File.Exists( path ) == false )
			{
				return null ;
			}

			return Directory.GetCurrentDirectory().Replace( "\\", "/" ) + "/" + path ;
		}

#endif

		// 一般タイプに対する拡張子
		internal protected readonly Dictionary<Type,List<string>> m_TypeToExtension = new ()
		{
			{ typeof( Sprite ),						new (){ ".png", ".jpg", ".tga", ".gif", ".bmp", ".tiff",											} },
			{ typeof( GameObject ),					new (){ ".prefab", ".asset", ".fbx", ".dae", ".obj", ".max", ".blend", 								} },
			{ typeof( AudioClip ),					new (){ ".wav", ".ogg", ".mp3", ".aif", ".aiff", ".xm", ".mod", ".it", ".s3m",						} },
			{ typeof( TextAsset ),					new (){ ".txt", ".json", ".bytes", ".csv", ".html", ".xml",  ".yml", ".htm", ".fnt"					} },
			{ typeof( Texture2D ),					new (){ ".png", ".jpg", ".tga", ".psd", ".gif", ".bmp", ".tif", ".tiff", ".iff", ".pict"			} },
			{ typeof( Texture ),					new (){ ".png", ".jpg", ".tga", ".psd", ".gif", ".bmp", ".tif", ".tiff", ".iff", ".pict", ".exr"	} },
			{ typeof( AnimationClip ),				new (){ ".anim",																					} },
			{ typeof( Font ),						new (){ ".ttf", ".otf", ".dfont", 																	} },
			{ typeof( Material ),					new (){ ".mat", ".material",																		} },
			{ typeof( Cubemap ),					new (){ ".hdr", ".cubemap",																			} },
			{ typeof( RuntimeAnimatorController ),	new (){ ".controller",																				} },
			{ typeof( AnimatorOverrideController ),	new (){ ".overrideController",																		} },
			{ typeof( Mesh ),						new (){ ".fbx", ".dae", ".obj", ".max", ".blend", 													} },
			{ typeof( Shader ),						new (){ ".shader", 																					} },
			{ typeof( PhysicMaterial ),				new (){ ".physicmaterial", 																			} },
			{ typeof( AvatarMask ),					new (){ ".mask", 																					} },
			{ typeof( Playable ),					new (){ ".playable", 																				} },
			{ typeof( SpriteAtlas ),				new (){ ".spriteatlas", 																			} },
			{ typeof( VideoClip ),					new (){ ".mp4", ".mov", ".asf", ".avi", ".mpg", ".mpeg"												} },
			{ typeof( ScriptableObject ),			new (){ ".asset"																					} },
		} ;

		internal protected Type GetTypeFromExtension( string extension )
		{
			if( string.IsNullOrEmpty( extension ) == true )
			{
				return typeof( System.Object ) ;
			}

			foreach( var item in m_TypeToExtension )
			{
				if( item.Value.Contains( extension ) == true )
				{
					return item.Key ;
				}
			}

			// 不明
			return typeof( System.Object ) ;
		}
		
		// 不明タイプに対する拡張子
		internal protected readonly List<string> m_UnknownTypeToExtension = new ()
		{
			".asset", ".prefab"
		} ;

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// ローカルアセットパスへ変換を行う
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		private string ToLocal( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return path ;
			}

			int i = path.IndexOf( m_ManifestSeparator ) ;
			if( i >= 0 )
			{
				// マニフェスト部分は削る
				i ++ ;
				path = path[ i.. ] ;
			}

			// アセットバンドル部とアセット部の境界を削る
			path = path.Replace( "//", "/" ) ;

			return path ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static ( string, Type )[] GetAllAssetPaths( string path, bool isOriginal = false )
		{
			if( m_Instance == null )
			{
				return default ;
			}
			return m_Instance.GetAllAssetPaths_Private( path, isOriginal ) ;
		}

		// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)
		private ( string, Type )[] GetAllAssetPaths_Private( string path, bool isOriginal )
		{
			// ※isOriginal はネイティブのパスかどうか

			( string, Type )[] allAssetPaths = null ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						allAssetPaths = GetLocalAllAssetPaths( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), isOriginal ) ;
					}
#endif
					if( allAssetPaths == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						allAssetPaths = m_ManifestHash[ manifestName ].GetAllAssetPaths
						(
							assetBundlePath,
							this
						) ;
					}
				}
			}

			if( allAssetPaths == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			return allAssetPaths ;
		}

		//---------------

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request GetAllAssetPathsAsync( string path, Action<( string, Type )[]> onLoaded = null, bool keep = false, bool isOriginal = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.GetAllAssetPathsAsync_Private( path, onLoaded, keep, isOriginal, request ) ) ;
			return request ;
		}

		// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		private IEnumerator GetAllAssetPathsAsync_Private( string path, Action<( string, Type )[]> onLoaded, bool keep, bool isOriginal, Request request )
		{
			// ※isOriginal はネイティブパスにするかどうか

			( string, Type )[] allAssetPaths = null ;
			string error = null ;

			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						allAssetPaths = GetLocalAllAssetPaths( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), isOriginal ) ;
					}
#endif
					if( allAssetPaths == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].GetAllAssetPaths_Coroutine
						(
							assetBundlePath,
							( _ ) => { allAssetPaths = _ ; }, ( _ ) => { error = _ ; },
							request,
							this
						) ) ;
					}
				}
			}

			if( allAssetPaths == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			request.Asset = allAssetPaths ;
			request.IsDone = true ;
			onLoaded?.Invoke( allAssetPaths ) ;

			m_AsyncProcessingCount -- ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static string[] GetAllAssetPaths( string path, Type type, bool isOriginal = false )
		{
			if( m_Instance == null )
			{
				return default ;
			}
			return m_Instance.GetAllAssetPaths_Private( path, type, isOriginal ) ;
		}

		// アセットを取得する(同期版)
		private string[] GetAllAssetPaths_Private( string path, Type type, bool isOriginal )
		{
			// ※isOriginal はネイティブパスにするかどうか

			string[] allAssetPaths = null ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true  )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						allAssetPaths = GetLocalAllAssetPaths( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), type, isOriginal ) ;
					}
#endif
					if( allAssetPaths == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						allAssetPaths = m_ManifestHash[ manifestName ].GetAllAssetPaths
						(
							assetBundlePath, type,
							this
						) ;
					}
				}
			}

			if( allAssetPaths == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			return allAssetPaths ;
		}

		//---------------

		/// <summary>
		/// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request GetAllAssetPathsAsync( string path, Type type, Action<string[]> onLoaded = null, bool keep = false, bool isOriginal = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.GetAllAssetPathsAsync_Private( path, type, onLoaded, keep, isOriginal, request ) ) ;
			return request ;
		}

		// アセットバンドルに含まれる全てのアセットのパスを取得する(非同期版)
		private IEnumerator GetAllAssetPathsAsync_Private( string path, Type type, Action<string[]> onLoaded, bool keep, bool isOriginal, Request request )
		{
			// ※isOriginal はネイティブパスにするかどうか

			string[] allAssetPaths = null ;
			string error = null ;

			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						allAssetPaths = GetLocalAllAssetPaths( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), type, isOriginal ) ;
					}
#endif
					if( allAssetPaths == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].GetAllAssetPaths_Coroutine
						(
							assetBundlePath, type,
							( _ ) => { allAssetPaths = _ ; }, ( _ ) => { error = _ ; },
							request,
							this
						) ) ;
					}
				}
			}

			if( allAssetPaths == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			request.Asset = allAssetPaths ;
			request.IsDone = true ;
			onLoaded?.Invoke( allAssetPaths ) ;

			m_AsyncProcessingCount -- ;
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// アセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T LoadAsset<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				return default ;
			}
			return m_Instance.LoadAsset_Private( path, typeof( T ), cachingType ) as T ;
		}

		/// <summary>
		/// アセットを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object LoadAsset( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.LoadAsset_Private( path, type, cachingType ) ;
		}

		// アセットを取得する(同期版)
		private UnityEngine.Object LoadAsset_Private( string path, Type type, CachingTypes cachingType )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = $"{path}:{type}" ;

			//--------------

			// キャッシュにあればそれを返す
			if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				// キャッシュされているインスタンスを返す(参照カウントも増加する)
				return m_ResourceCache[ resourceCachePath ].Load() ;
			}

			//------------------------------------------------

			UnityEngine.Object						asset				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false &&  m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						asset = LoadLocalAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), type ) ;
					}
#endif
					if( asset == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						( asset, assetBundleCache ) = m_ManifestHash[ manifestName ].LoadAsset
						(
							assetBundlePath, assetPath, type,
							this
						) ;
					}
				}
			}

			if( asset == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				// アセット(リソース)キャッシュの追加
				AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
			}

			//------------------------------------------------

			return asset ;
		}

		//---------------

		/// <summary>
		/// アセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAssetAsync<T>( string path, Action<T> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetAsync_Private( path, typeof( T ), ( UnityEngine.Object asset ) => { onLoaded?.Invoke( asset as T ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		/// <summary>
		/// アセットを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAssetAsync( string path, Type type, Action<UnityEngine.Object> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetAsync_Private( path, type, onLoaded, cachingType, keep, request ) ) ;
			return request ;
		}
		
		// アセットを取得する(非同期版)
		private IEnumerator LoadAssetAsync_Private( string path, Type type, Action<UnityEngine.Object> onLoaded, CachingTypes cachingType, bool keep, Request request )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = $"{path}:{type}" ;

			//------------------------------------------------

			UnityEngine.Object asset = null ;
			string error = null ;

			// キャッシュにあればそれを返す
			if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				// キャッシュされているインスタンスを返す(参照カウントも増加する)
				asset = m_ResourceCache[ resourceCachePath ].Load() ;

				request.Asset = asset ;
				onLoaded?.Invoke( asset ) ;
				request.IsDone = true ;

				yield break ;
			}

			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			ManifestInfo.AssetBundleCacheElement assetBundleCache = null ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						asset = LoadLocalAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ), type ) ;
					}
#endif
					if( asset == null &&  string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAsset_Coroutine
						(
							assetBundlePath, assetPath, type,
							( _1, _2 ) => { asset = _1 ; assetBundleCache = _2 ; }, ( _ ) => { error = _ ; },
							request,
							this
						) ) ;
					}
				}
			}

			if( asset == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				// アセット(リソース)キャッシュの追加
				AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
			}

			//------------------------------------------------

			request.Asset	= asset ;
			request.IsDone	= true ;
			onLoaded?.Invoke( asset ) ;

			m_AsyncProcessingCount -- ;
		}
		
		//---------------------------------------------------------------------------

		// AssetBundleManager :: AllAssets

		/// <summary>
		/// 全てのアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T[] LoadAllAssets<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				return null ;
			}

			// 配列のジェネリックキャストは出来ないので単体にばらしてキャストする
			var temporaryAssets = m_Instance.LoadAllAssets_Private( path, typeof( T ), cachingType ) ;
			if( temporaryAssets == null || temporaryAssets.Length == 0 )
			{
				return null ;
			}

			var assets = new T[ temporaryAssets.Length ] ;
			for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
			{
				assets[ i ] = temporaryAssets[ i ] as T ;
			}

			return assets ;
		}

		/// <summary>
		/// 全てのサブアセットを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object[] LoadAllAssets( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.LoadAllAssets_Private( path, type, cachingType ) ;
		}

		// アセットバンドル内の指定の型の全てのサブアセットを直接取得する(同期版)
		private UnityEngine.Object[] LoadAllAssets_Private( string path, Type type, CachingTypes cachingType )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			string resourceCachePath ;
			
			//------------------------------------------------

			string localAssetPath = ToLocal( path ) ;

			UnityEngine.Object[]					assets				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;	// 複数であってもキャッシュ情報インスタンスは１つ(ただしカウントはアセット数分増えている)

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					UnityEngine.Object[] temporaryAssets ;

					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						temporaryAssets = LoadLocalAllAssets( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, type ) ;
						if( temporaryAssets != null && temporaryAssets.Length >  0 )
						{
							for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
							{
								resourceCachePath = $"{path}/{temporaryAssets[ i ].name}:{temporaryAssets[ i ].GetType()}" ;
								if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
								{
									// キャッシュされているインスタンスを返す(参照カウントも増加する)
									temporaryAssets[ i ] = m_ResourceCache[ resourceCachePath ].Load() ;
								}
							}
							assets = temporaryAssets ;
						}
					}
#endif
					if( ( assets == null || assets.Length == 0 ) && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						( assets, assetBundleCache ) = m_ManifestHash[ manifestName ].LoadAllAssets
						(
							assetBundlePath, type,
							localAssetPath,
							this
						) ;
					}
				}
			}

			if( assets == null || assets.Length == 0 )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				foreach( var asset in assets )
				{
					resourceCachePath = $"{path}/{asset.name}:{asset.GetType()}" ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						// アセット(リソース)キャッシュの追加
						AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
					}
				}
			}

			//------------------------------------------------

			return assets ;
		}

		//---------------

		/// <summary>
		/// 全てのアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAllAssetsAsync<T>( string path, Action<T[]> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;

			m_Instance.StartCoroutine( m_Instance.LoadAllAssetsAsync_Private
			(
				path, typeof( T ),
				( UnityEngine.Object[] temporaryAssets ) =>
				{
					if( onLoaded != null && temporaryAssets != null && temporaryAssets.Length >  0 )
					{
						var assets = new T[ temporaryAssets.Length ] ;
						for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
						{
							assets[ i ] = temporaryAssets[ i ] as T ;
						}
						onLoaded( assets ) ;
					}
				},
				cachingType, keep, request
			) ) ;

			return request ;
		}

		/// <summary>
		/// 全てのアセットを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAllAssetsAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAllAssetsAsync_Private( path, type, ( UnityEngine.Object[] assets ) => { onLoaded?.Invoke( assets ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		// アセットに含まれる全てのサブアセットを取得する(非同期版)
		private IEnumerator LoadAllAssetsAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingTypes cachingType, bool keep, Request request )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			string resourceCachePath ;
			
			//------------------------------------------------

			string localAssetPath = ToLocal( path ) ;

			m_AsyncProcessingCount ++ ;

			UnityEngine.Object[]					assets				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			string error = string.Empty ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					UnityEngine.Object[] temporaryAssets ;

					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						temporaryAssets = LoadLocalAllAssets( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, type ) ;
						if( temporaryAssets != null && temporaryAssets.Length >  0 )
						{
							for( int  i = 0 ; i <  temporaryAssets.Length ; i ++ )
							{
								resourceCachePath = $"{path}/{temporaryAssets[ i ].name}:{temporaryAssets[ i ].GetType()}" ;
								if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
								{
									// キャッシュされているインスタンスを返す(参照カウントも増加する)
									temporaryAssets[ i ] = m_ResourceCache[ resourceCachePath ].Load() ;
								}
							}
							assets = temporaryAssets ;
						}
					}
#endif
					if( ( assets == null || assets.Length == 0 ) &&  string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAllAssets_Coroutine
						(
							assetBundlePath, type,
							( _1, _2 ) => { assets = _1 ; assetBundleCache = _2 ; }, ( _ ) => { error = _ ; },
							request,
							localAssetPath,
							this
						) ) ;
					}
				}
			}

			if( assets == null || assets.Length == 0 )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				// 参照カウントを増加させる

				foreach( var asset in assets )
				{
					resourceCachePath = $"{path}/{asset.name}:{asset.GetType()}" ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						// アセット(リソース)キャッシュの追加
						AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
					}
				}
			}

			//------------------------------------------------

			request.Assets = assets ;
			request.IsDone = true ;
			onLoaded?.Invoke( assets ) ;

			m_AsyncProcessingCount -- ;
		}

		//---------------------------------------------------------------------------

		// AssetBundleManager :: SubAsset

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T LoadSubAsset<T>( string path, string subAssetName, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				return default ;
			}
			return m_Instance.LoadSubAsset_Private( path, subAssetName, typeof( T ), cachingType ) as T ;
		}

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object LoadSubAsset( string path, string subAssetName, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.LoadSubAsset_Private( path, subAssetName, type, cachingType ) ;
		}

		// アセットに含まれるサブアセットを取得する(同期版)
		private UnityEngine.Object LoadSubAsset_Private( string path, string subAssetName, Type type, CachingTypes cachingType )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = $"{path}/{subAssetName}:{type}" ;

			//--------------

			// キャッシュにあればそれを返す
			if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				// キャッシュされているインスタンスを返す(参照カウントも増加する)
				return m_ResourceCache[ resourceCachePath ].Load() ;
			}

			//------------------------------------------------

			string localAssetPath = ToLocal( path ) ;

			UnityEngine.Object						asset				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(同期)
						asset = LoadLocalSubAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, subAssetName, type ) ;
					}
#endif
					if( asset == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						( asset, assetBundleCache ) = m_ManifestHash[ manifestName ].LoadSubAsset
						(
							assetBundlePath, assetPath, subAssetName, type,
							localAssetPath,
							this
						) ;
					}
				}
			}

			if( asset == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				// アセット(リソース)キャッシュの追加
				AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
			}

			//------------------------------------------------

			return asset ;
		}

		//---------------

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadSubAssetAsync<T>( string path, string subAssetName, Action<T> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadSubAssetAsync_Private( path, subAssetName, typeof( T ), ( UnityEngine.Object asset ) => { onLoaded?.Invoke( asset as T ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadSubAssetAsync( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadSubAssetAsync_Private( path, subAssetName, type, onLoaded, cachingType, keep, request ) ) ;
			return request ;
		}
		
		// アセットに含まれるサブアセットを取得する(非同期版)
		private IEnumerator LoadSubAssetAsync_Private( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded, CachingTypes cachingType, bool keep, Request request )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = $"{path}/{subAssetName}:{type}" ;

			//------------------------------------------------

			UnityEngine.Object asset = null ;

			// キャッシュにあればそれを返す
			if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				// キャッシュされているインスタンスを返す(参照カウントも増加する)
				asset = m_ResourceCache[ resourceCachePath ].Load() ;

				request.Asset = asset ;
				request.IsDone = true ;
				onLoaded?.Invoke( asset ) ;

				yield break ;
			}

			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			string localAssetPath = ToLocal( path ) ;

			ManifestInfo.AssetBundleCacheElement assetBundleCache = null ;
			string error = string.Empty ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						asset = LoadLocalSubAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, subAssetName, type ) ;
					}
#endif
					if( asset == null && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadSubAsset_Coroutine
						(
							assetBundlePath, assetPath, subAssetName, type,
							( _1, _2 ) => { asset = _1 ; assetBundleCache = _2 ; }, ( _ ) => { error = _ ; },
							request,
							localAssetPath,
							this
						) ) ;
					}
				}
			}

			if( asset == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				// アセット(リソース)キャッシュの追加
				AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
			}

			//------------------------------------------------

			request.Asset = asset ;
			request.IsDone = true ;
			onLoaded?.Invoke( asset ) ;

			m_AsyncProcessingCount -- ;
		}

		//---------------------------------------------------------------------------

		// AssetBundleManager :: AllSubAssets

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static T[] LoadAllSubAssets<T>( string path, CachingTypes cachingType = CachingTypes.None ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				return null ;
			}

			// ジェネリック配列へのキャストは出来ないので配列の個々単位でにキャストする必要がある
			var temporaryAssets = m_Instance.LoadAllSubAssets_Private( path, typeof( T ), cachingType ) ;
			if( temporaryAssets == null || temporaryAssets.Length == 0 )
			{
				return null ;
			}

			var assets = new T[ temporaryAssets.Length ] ;
			for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
			{
				assets[ i ] = temporaryAssets[ i ] as T ;
			}
			return assets ;
		}

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="cachingType"></param>
		/// <returns></returns>
		public static UnityEngine.Object[] LoadAllSubAssets( string path, Type type, CachingTypes cachingType = CachingTypes.None )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.LoadAllSubAssets_Private( path, type, cachingType ) ;
		}

		// アセットバンドル内の指定の型の全てのサブアセットを直接取得する(同期版)
		private UnityEngine.Object[] LoadAllSubAssets_Private( string path, Type type, CachingTypes cachingType )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			string resourceCachePath ;
			
			//------------------------------------------------

			UnityEngine.Object[]					assets				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			string localAssetPath = ToLocal( path ) ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					UnityEngine.Object[] temporaryAssets ;

					if( m_UseLocalAssets == true && ( assets == null || assets.Length == 0 ) )
					{
						// LocalAssets からロードを試みる(同期)
						temporaryAssets = LoadLocalAllSubAssets( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, type ) ;
						if( temporaryAssets != null && temporaryAssets.Length >  0 )
						{
							for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
							{
								resourceCachePath = $"{path}/{temporaryAssets[ i ].name}:{type}" ;
								if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
								{
									// キャッシュされているインスタンスを返す(参照カウントも増加する)
									temporaryAssets[ i ] = m_ResourceCache[ resourceCachePath ].Load() ;
								}
							}
							assets = temporaryAssets ;
						}
					}
#endif
					if( ( assets == null || assets.Length == 0 ) && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(同期)
						( assets, assetBundleCache ) = m_ManifestHash[ manifestName ].LoadAllSubAssets
						(
							assetBundlePath, assetPath, type,
							localAssetPath,
							this
						) ;
					}
				}
			}

			if( assets == null || assets.Length == 0 )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				foreach( var asset in assets )
				{
					resourceCachePath = $"{path}/{asset.name}:{type}" ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						// アセット(リソース)キャッシュの追加
						AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
					}
				}
			}

			//------------------------------------------------

			return assets ;
		}

		//---------------

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAllSubAssetsAsync<T>( string path, Action<T[]> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAllSubAssetsAsync_Private
			(
				path, typeof( T ),
				( UnityEngine.Object[] temporaryAssets ) =>
				{
					if( onLoaded != null && temporaryAssets != null && temporaryAssets.Length >  0 )
					{
						var assets = new T[ temporaryAssets.Length ] ;
						for( int i  = 0 ; i <  temporaryAssets.Length ; i ++ )
						{
							assets[ i ] = temporaryAssets[ i ] as T ;
						}
						onLoaded( assets ) ;
					}
				},
				cachingType, keep, request
			) ) ;
			return request ;
		}

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="cachingType"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadAllSubAssetsAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, CachingTypes cachingType = CachingTypes.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAllSubAssetsAsync_Private( path, type, ( UnityEngine.Object[] assets ) => { onLoaded?.Invoke( assets ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		// アセットに含まれる全てのサブアセットを取得する(非同期版)
		private IEnumerator LoadAllSubAssetsAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingTypes cachingType, bool keep, Request request )
		{
			bool cachingEnabled = ( cachingType != CachingTypes.None ) ;

			//------------------------------------------------

			string resourceCachePath ;
			
			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			UnityEngine.Object[]					assets				= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ; 
			string error = string.Empty ;

			string localAssetPath = ToLocal( path ) ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					UnityEngine.Object[] temporaryAssets ;

					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						temporaryAssets = LoadLocalAllSubAssets( m_ManifestHash[ manifestName ].LocalAssetsRootPath, localAssetPath, type ) ;
						if( temporaryAssets != null && temporaryAssets.Length >  0 )
						{
							for( int  i = 0 ; i <  temporaryAssets.Length ; i ++ )
							{
								resourceCachePath = $"{path}/{temporaryAssets[ i ].name}:{type}" ;
								if( m_ResourceCache.ContainsKey( resourceCachePath ) == true )
								{
									// キャッシュされているインスタンスを返す(参照カウントも増加する)
									temporaryAssets[ i ] = m_ResourceCache[ resourceCachePath ].Load() ;
								}
							}
							assets = temporaryAssets ;
						}
					}
#endif
					if( ( assets == null || assets.Length == 0 ) && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAllSubAssets_Coroutine
						(
							assetBundlePath, assetPath, type,
							( _1, _2 ) => { assets = _1 ; assetBundleCache = _2 ; }, ( _ ) => { error = _ ; },
							request,
							localAssetPath,
							this
						) ) ;
					}
				}
			}

			if( assets == null || assets.Length == 0 )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( cachingEnabled == true )
			{
				foreach( var asset in assets )
				{
					resourceCachePath = $"{path}/{asset.name}:{type}" ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						// アセット(リソース)キャッシュの追加
						AddResourceCache( resourceCachePath, asset, assetBundleCache ) ;
					}
				}
			}

			//------------------------------------------------

			request.Assets = assets ;
			request.IsDone = true ;
			onLoaded?.Invoke( assets ) ;

			m_AsyncProcessingCount -- ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセット(リソース)を破棄する
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		public static bool FreeAsset( UnityEngine.Object asset )
		{
			if( m_Instance == null || asset == null )
			{
				return false ;
			}
			return m_Instance.FreeAsset_Private( asset ) ;
		}

		// アセット(リソース)を破棄する
		private bool FreeAsset_Private( UnityEngine.Object asset )
		{
			if( m_ResourceCacheDetector.ContainsKey( asset ) == false )
			{
				// キャッシユされているものではない
				return false ;
			}

			//----------------------------------

			var resourceCache = m_ResourceCacheDetector[ asset ] ;

			// アセット(リソース)の参照カウントを減少させる
			if( resourceCache.Free() == true )
			{
				// アセット(リソース)の参照カウントが０になったのでアセット(リソース)のキャッシュを削除する
				RemoveResourceCache( resourceCache.Path ) ;
			}

			return true ;
		}
		
		//---------------------------------------------------------------------------

		// AssetBundleManager :: Scene

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync( string path, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync<T>( string path, Action<T[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, typeof( T ), ( UnityEngine.Object[] targets ) => { onLoaded?.Invoke( targets as T[] ) ; }, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request AddSceneAsync( string path, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request AddSceneAsync<T>( string path, Action<T[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, typeof( T ), ( UnityEngine.Object[] targets ) => { onLoaded?.Invoke( targets as T[] ) ; }, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="onLoaded"></param>
		/// <param name="targetName"></param>
		/// <param name="sceneName"></param>
		/// <param name="keep"></param>
		/// <returns></returns>
		public static Request AddSceneAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}
		
		//-----------------------------------------------------------

		// シーンを展開または加算する(非同期版)
		private IEnumerator LoadOrAddSceneAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, string sceneName, bool keep, UnityEngine.SceneManagement.LoadSceneMode mode, Request request )
		{
			// 名前の指定が無ければファイル名から生成する
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				int p = path.LastIndexOf( "/" ) ;
				if( p <  0 )
				{
					sceneName = path ;
				}
				else
				{
					p ++ ;
					sceneName = path[ p.. ] ;
					if( string.IsNullOrEmpty( sceneName ) == true )
					{
						request.Error = "Bad scene name." ;
						yield break ;	// シーン名が不明
					}
				}
			}

			//------------------------------------------------

			m_AsyncProcessingCount ++ ;

			bool			result = false ;

			AssetBundle								assetBundle			= null ;
			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			UnityEngine.Object[] targets = null ;
			string error = string.Empty ;

			// アセットバンドルからロードを試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からロードを試みる(非同期)
						error = string.Empty ;
						yield return StartCoroutine( OpenLocalSceneAsync( m_ManifestHash[ manifestName ].LocalAssetsRootPath, path, sceneName, type, mode, ( _ ) => { error = _ ; } ) ) ;
						result = string.IsNullOrEmpty( error ) ;
						if( result == true )
						{
							yield return StartCoroutine( WaitSceneAsync_Private( sceneName, type, ( _ ) => { targets = _ ; }, targetName, ( _ ) => { error = _ ; } ) ) ;
							result = string.IsNullOrEmpty( error ) ;
						}
					}
#endif
					if( result == false && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からロードを試みる(非同期)
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAssetBundle_Coroutine
						(
							assetBundlePath,
							( _1, _2 ) => { assetBundle = _1 ; assetBundleCache = _2 ; }, ( _ ) => { error = _ ; },
							request,
							this
						) ) ;

						if( assetBundle != null )
						{
							if( assetBundle.isStreamedSceneAssetBundle == true )
							{
								// SceneのAssetBundle
								error = string.Empty ;
								yield return StartCoroutine( OpenSceneAsync_Private( sceneName, type, mode, ( _ ) => { error = _ ; } ) ) ;
								result = string.IsNullOrEmpty( error ) ;
								if( result == true )
								{
									yield return StartCoroutine( WaitSceneAsync_Private( sceneName, type, ( _ ) => { targets = _ ; }, targetName, ( _ ) => { error = _ ; } ) ) ;
									result = string.IsNullOrEmpty( error ) ;
								}
							}

							if( result == false )
							{
								// 成功の場合は自動破棄リストに追加する(Unload(false))
								m_ManifestHash[ manifestName ].RemoveAssetBundleCacheForced( assetBundlePath, false ) ;
							}
							else
							{
								// 失敗(isStreamedSceneAssetBundle==trueでなければSceneのAssetBundleではない)
								assetBundle.Unload( true ) ;
							}
						}
					}
				}
			}

			if( result == false )
			{
				// 失敗
				if( string.IsNullOrEmpty( error ) == true )
				{
					error = "Could not load." ;
				}
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			//------------------------------------------------

			// シーンに関しては原則破棄後に Resources.UnloadUnusedAssets() が実行されるため AssetBundle をキャッシュに貯めるという事はしない

			request.Assets = targets ;
			request.IsDone = true ;
			onLoaded?.Invoke( targets ) ;

			m_AsyncProcessingCount -- ;
		}

		private IEnumerator OpenSceneAsync_Private( string sceneName, Type type, UnityEngine.SceneManagement.LoadSceneMode mode, Action<string> onError )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				onError?.Invoke( "Bad scene name." ) ;
				yield break ;
			}
			
			//----------------------------------------------------------

			if( type != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindAnyObjectByType( type ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------
			
			// リモート
//			if( instance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//			{
//				// 非同期(低速)
//				yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( tName, tMode ) ;
//			}
//			else
//			{
				// 同期(高速)　※同期メソッドを使っても実質非同期
				UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName, mode ) ;
//			}
		}

		// シーンをロードまたは加算する(非同期版)
		private IEnumerator WaitSceneAsync_Private( string sceneName, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, Action<string> onError )
		{
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			
			if( scene.IsValid() == false )
			{
				onError?.Invoke( "Scene is invalid" ) ;
				yield break ;
			}

			// シーンの展開が完了するのを待つ
			yield return new WaitWhile( () => scene.isLoaded == false ) ;

			if( type != null && onLoaded != null )
			{
				GetInstance_Private( scene, type, onLoaded, targetName ) ;
			}
		}

		//---------------------------

		private void GetInstance_Private( UnityEngine.SceneManagement.Scene scene, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName )
		{
			// 指定の型のコンポーネントを探してインスタンスを取得する
			var fullTargets = new List<UnityEngine.Object>() ;

			var gos = scene.GetRootGameObjects() ;
			if( gos != null && gos.Length >  0 )
			{
				UnityEngine.Object[] components ;
				foreach( var go in gos )
				{
					components = go.GetComponentsInChildren( type, true ) ;
					if( components != null && components.Length >  0 )
					{
						foreach( var component in components )
						{
							fullTargets.Add( component ) ;
						}
					}
				}
			}

			if( fullTargets.Count >  0 )
			{
				UnityEngine.Object[] temporaryTargets = null ;

				// 該当のコンポーネントが見つかった
				if( string.IsNullOrEmpty( targetName ) == false )
				{
					// 名前によるフィルタ有り
					var filteredTargets = new List<UnityEngine.Object>() ;
					foreach( var target in fullTargets )
					{
						if( target.name == targetName )
						{
							filteredTargets.Add( target ) ;
						}
					}

					if( filteredTargets.Count >  0 )
					{
						temporaryTargets = filteredTargets.ToArray() ;
					} 
				}
				else
				{
					// 名前によるフィルタ無し
					temporaryTargets = fullTargets.ToArray() ;
				}

				if( temporaryTargets != null && temporaryTargets.Length >  0 )
				{
					onLoaded?.Invoke( temporaryTargets ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		// AssetBundleManager :: AssetBundle

		/// <summary>
		/// アセットバンドルを展開する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="retain"></param>
		/// <returns></returns>
		public static AssetBundle LoadAssetBundle( string path )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.LoadAssetBundle_Private( path ) ;
		}

		// アセットバンドルを展開する(同期版)
		private AssetBundle LoadAssetBundle_Private( string path )
		{
			// 注意：アセットバンドル自体は原則キャッシュに溜まりシーンの切替時に強制的に破棄される
			// 　　　NoCaching にした場合のみキャッシュに貯めるという事はせず解放の責任がエンドユーザーに任される
			// 　　　キャッシュにためた場合はリリースメソッドにより参照カウントを下げて破棄する事が可能
			// 　　　特例としてこのリリースメソッドのみ参照カウントが０になった際にアセットの強制破棄を行うかどうかも選択する事が出来る

			AssetBundle								assetBundle			= null ;
//			ManifestInfo.AssetBundleCacheElement	assetBundleCache	= null ;

			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						( assetBundle, _ ) = m_ManifestHash[ manifestName ].LoadAssetBundle
						(
							assetBundlePath,
							m_Instance
						) ;
					}
				}
			}

			return assetBundle ;
		}

		//-----------------------------------

		/// <summary>
		/// アセットバンドルを展開する(非同期版)　※必ず自前で Unload を行わなければならない
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="onLoaded">アセットバンドルのインスタンスを取得するコールバック</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAssetBundleAsync( string path )
		{
			// 必ず自前で Unload を行わなければならない
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			var request = new Request( m_Instance ) ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetBundleAsync_Private( path, request ) ) ;
			return request ;
		}

		// アセットバンドルを展開する(非同期版)
		private IEnumerator LoadAssetBundleAsync_Private( string path, Request request )
		{
			m_AsyncProcessingCount ++ ;

			// アセットバンドルを取得する
			string error = string.Empty ;

			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAssetBundle_Coroutine
						(
							assetBundlePath,
							null, ( _ ) => { error = _ ; },
							request,
							this
						) ) ;
					}
				}
			}

			if( string.IsNullOrEmpty( error ) == false )
			{
				request.Error = error ;

				m_AsyncProcessingCount -- ;

				yield break ;
			}

			request.IsDone = true ;

			// アセットバンドルに関しては強制的にキャッシュに貯める

			m_AsyncProcessingCount -- ;
		}

		//-----------------------------------

		// 将来的に引数にインスタンス版を用意する可能性が高い

		/// <summary>
		/// アセットバンドルを破棄する(同期版)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="retain"></param>
		/// <returns></returns>
		public static bool FreeAssetBundle( string path )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.FreeAssetBundle_Private( path ) ;
		}

		// アセットバンドルを破棄する(同期版)
		private bool FreeAssetBundle_Private( string path )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].RemoveAssetBundleCacheForced( assetBundlePath, true ) ;
					}
				}
			}

			// 失敗
			return false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 通常のメモリ展開アセットバンドルの破棄でアセットバンドルが破棄されないようにするかどうかの設定を行う
		/// </summary>
		/// <param name="isRetain"></param>
		/// <returns></returns>
		public static bool SetAssetBundleRetaining( string path, bool isRetain, bool withAssets = true )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			return m_Instance.SetAssetBundleRetaining_Private( path, isRetain, withAssets ) ;
		}

		// 通常のメモリ展開アセットバンドルの破棄でアセットバンドルが破棄されないようにするかどうかの設定を行う
		private bool SetAssetBundleRetaining_Private( string path, bool isRetain, bool withAssets )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].SetAssetBundleRetaining( assetBundlePath, isRetain, withAssets ) ;
					}
				}
			}

			// 失敗
			return false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルをストレージキャッシュから削除する
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static bool DeleteAssetBundleFromStorageCache( string path )
		{
			// 簡略化すると Unity でエラーが出る(C#のバージョン的に未対応)
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.DeleteAssetBundleFromStorageCache_Private( path ) ;
		}

		// アセットバンドルをキャッシュから削除する
		private bool DeleteAssetBundleFromStorageCache_Private( string path )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].DeleteAssetBundleFromStorageCache( assetBundlePath, this ) ;
					}
				}
			}
			return false ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// アセットバンドルが管理対象に含まれているか確認する
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <returns></returns>
		public static bool Contains( string path )
		{
			// 簡略化すると Unity でエラーが出る(C#のバージョン的に未対応)
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Contains_Private( path ) ;
		}
		
		// アセットバンドルが管理対象に含まれているか確認する
		private bool Contains_Private( string path )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					// LocalAssets
					if( m_UseLocalAssets == true )
					{
						if( ContainsLocalAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ) ) == true )
						{
							return true ;
						}
					}
#endif
					// LocalAssetBundle StreamingAssets RemoteAssetBundle
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true)
					{
						return m_ManifestHash[ manifestName ].Contains( assetBundlePath ) ;
					}
				}
			}

			return false ;
		}
		
		/// <summary>
		/// アセットバンドルの存在を確認する
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <returns></returns>
		public static bool Exists( string path )
		{
			// 簡略化すると Unity でエラーが出る(C#のバージョン的に未対応)
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Exists_Private( path ) ;
		}
		
		// アセットバンドルの存在を確認する
		private bool Exists_Private( string path )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					// LocalAssets
					if( m_UseLocalAssets == true )
					{
						if( ContainsLocalAsset( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ) ) == true )
						{
							return true ;
						}
					}
#endif
					// LocalAssetBundle StreamingAssets RemoteAssetBundle
 					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].Exists( assetBundlePath ) ;
					}
				}
			}
			return false ;
		}

		/// <summary>
		/// アセットバンドルのサイズを取得する
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <returns></returns>
		public static long GetSize( string path )
		{
			// 簡略化すると Unity でエラーが出る(C#のバージョン的に未対応)
			if( m_Instance == null )
			{
				return -1 ;
			}
			
			return m_Instance.GetSize_Private( path ) ;
		}
		
		// アセットバンドルのサイズを取得する
		private long GetSize_Private( string path )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].GetSize( assetBundlePath ) ;
					}
				}
			}
			return -1 ;
		}

		/// <summary>
		/// 指定のアセットバンドルのキャッシュ内での動作を設定する(キャッシュオーバー時に維持するかどうか)
		/// </summary>
		/// <param name="path">アセットバンドルのパス</param>
		/// <param name="keep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>結果(true=成功・失敗)</returns>
		public static bool SetKeepFlag( string path, bool keep )
		{
			// 簡略化すると Unity でエラーが出る(C#のバージョン的に未対応)
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetKeepFlag_Private( path, keep ) ;
		}

		// 指定のアセットバンドルのキャッシュ内での動作を設定する
		private bool SetKeepFlag_Private( string path, bool keep )
		{
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					if( string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						return m_ManifestHash[ manifestName ].SetKeepFlag( assetBundlePath, keep ) ;
					}
				}
			}
			return false ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 非アセットバンドルのファイルの直接パスを取得する(実際にファイルが存在しないと取得できない)
		/// </summary>
		/// <param name="assetBundlePath"></param>
		/// <returns></returns>
		public static string GetAssetNativePath( string path )
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.GetAssetNativePath_Private( path ) ;
		}

		private string GetAssetNativePath_Private( string path )
		{
			string filePath = null ;

			// マニフェストからファイルパス取得を試みる
			if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundlePath, out string assetPath ) == true )
			{
				if( string.IsNullOrEmpty( manifestName ) == false && m_ManifestHash.ContainsKey( manifestName ) == true )
				{
#if UNITY_EDITOR
					if( m_UseLocalAssets == true )
					{
						// LocalAssets からパス取得を試みる(同期)
						filePath = GetLocalAssetFilePath( m_ManifestHash[ manifestName ].LocalAssetsRootPath, ToLocal( path ) ) ;
					}
#endif
					if( string.IsNullOrEmpty( filePath ) == true && string.IsNullOrEmpty( assetBundlePath ) == false && m_ManifestHash[ manifestName ].CorrectPath( ref assetBundlePath, ref assetPath ) == true )
					{
						// LocalAssetBundle StreamingAssets RemoteAssetBundle からパス取得を試みる(同期)
						filePath = m_ManifestHash[ manifestName ].GetAssetFilePath( assetBundlePath, m_Instance ) ;
					}
				}
			}

			return filePath ;
		}

		//-----------------------------------------------------------
	}
}
