using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Security.Cryptography ;
using System.Runtime.Serialization.Formatters.Binary ;
using UnityEngine ;
using UnityEngine.Networking ;

/// <summary>
/// ストレージヘルパーパッケージ
/// </summary>
namespace StorageHelper
{
	/// <summary>
	/// ストレージアクセサクラス Version 2019/09/10 0
	/// </summary>
	public class StorageAccessor
	{
		// 初期化キー
//		private const string m_Key    = "lkirwf897+22#bbtrm8814z5qq=498j5" ;	// RM  用 32 byte
	
		// 初期化ベクタ
//		private const string m_Vector = "741952hheeyy66#cs!9hjv887mxx7@8y" ;	// 16 byte

		public enum Target
		{
			Unknown	= -1,
			None	=  0,
			File	=  1,
			Folder	=  2,
		}


#if UNITY_EDITOR || UNITY_STANDALONE
	
		// デバッグ用のテンポラリデータフォルダ
		public const string TemporaryDataFoler = "/TemporaryDataFolder" ;
	
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
	
		private static string CreateTemporaryDataFolder()
		{
			string path = Directory.GetCurrentDirectory().Replace( "\\", "/" ) + TemporaryDataFoler ;
		
			if( Directory.Exists( path ) == false )
			{
				// テンポラリが無いので生成する
				Directory.CreateDirectory( path ) ;
			}

			return path ;
		}
	
#endif


		/// <summary>
		/// データフォルダのパス
		/// </summary>
		public static string Path
		{
			get
			{
				// あえてメソッド化しているのは、引数で挙動を変える可能性もあるため。
				string path = "" ;
		
#if UNITY_EDITOR || UNITY_STANDALONE
				path = CreateTemporaryDataFolder() ;
#else
				path = Application.persistentDataPath ;
#endif
			
				return path + "/" ;
			}
		}
		
		public static void Setup()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			CreateTemporaryDataFolder() ;
#endif
		}

		/// <summary>
		/// ２つのパスを結合したものを返す
		/// </summary>
		/// <param name="tPath_0"></param>
		/// <param name="tPath_1"></param>
		/// <returns></returns>
		public static string Combine( string path_0, string path_1 )
		{
			if( string.IsNullOrEmpty( path_1 ) == false )
			{
				if( path_1[ 0 ] == '!' || path_1[ 0 ] == '@' )
				{
					// 絶対パス
					return path_1.Substring( 1, path_1.Length - 1 ) ;
				}
			}
			
			if( string.IsNullOrEmpty( path_0 ) == true )
			{
				return path_1 ;
			}

			path_0 = path_0.Replace( "\\", "/" ) ;

			if( string.IsNullOrEmpty( path_1 ) == true )
			{
				return path_0 ;
			}

			path_1 = path_1.Replace( "\\", "/" ) ;

			int l0 = path_0.Length ;
			if( path_0[ l0 - 1 ] == '/' )
			{
				path_0 = path_0.Substring( 0, l0 - 1 ) ;
			}

			int l1 = path_1.Length ;
			if( path_1[ 0 ] == '/' )
			{
				path_1 = path_1.Substring( 1, l1 - 1 ) ;
			}

			return path_0 + "/" + path_1 ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージにバイト配列を書き込む
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tData">バイト配列</param>
		/// <param name="tMakeFolder">フォルダが存在しない場合に生成するかどうか(生成しない場合はエラーとなる)</param>
		/// <param name="tKey">暗号化キー(null で暗号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で暗号化は行わない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save( string path, byte[] data, bool makeFolder = false, string key = null, string vector = null )
		{
			if( string.IsNullOrEmpty( path ) == true || data == null )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Save Error : Path = " + path + " Data = " + data ) ;
				#endif
				return false ;
			}

			if( string.IsNullOrEmpty( key ) == false && string.IsNullOrEmpty( vector ) == false )
			{
				// 暗号化する
				data = Encrypt( data, key, vector ) ;
			}

			string fullPath = Combine( Path, path ) ;

			// 名前にフォルダが含まれているかチェックする
			int i = fullPath.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				// フォルダが含まれている
				string folderName = fullPath.Substring( 0, i ) ;

				if( Directory.Exists( folderName ) == false )
				{
					if( makeFolder == false )
					{
						// セーブ出来ません
						return false ;
					}
					else
					{
						// フォルダを生成する(多階層をまとめて生成出来る)
						Directory.CreateDirectory( folderName ) ;
						
						// Apple 審査のリジェクト回避用コード
						#if !UNITY_EDITOR &&( UNITY_IOS || UNITY_IPHONE )
							UnityEngine.iOS.Device.SetNoBackupFlag( folderName ) ;
						#endif
					}
				}
			}

			File.WriteAllBytes( fullPath, data ) ;

			// Apple 審査のリジェクト回避用コード
			#if !UNITY_EDITOR &&( UNITY_IOS || UNITY_IPHONE )
				UnityEngine.iOS.Device.SetNoBackupFlag( fullPath ) ;
			#endif

			return true ;
		}

		/// <summary>
		/// ローカルストレージに文字列を書き込む
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tText">文字列</param>
		/// <param name="tMakeFolder">フォルダが存在しない場合に生成するかどうか(生成しない場合はエラーとなる)</param>
		/// <param name="tKey">暗号化キー(null で暗号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で暗号化は行わない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveText( string path, string text, bool makeFolder = false, string key = null, string vector = null )
		{
			if( string.IsNullOrEmpty( path ) == true || string.IsNullOrEmpty( text ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Text File Save Error : Path = " + path + " Text = " + text ) ;
				#endif
				return false ;
			}
		
			byte[] data = Encoding.UTF8.GetBytes( text ) ;

			return Save( path, data, makeFolder, key, vector ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージからバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で複合化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で複合化は行わない)</param>
		/// <returns>バイト配列(null で失敗)</returns>
		public static byte[] Load( string path, string key = null, string vector = null )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			string fullPath = Combine( Path, path ) ;

			if( File.Exists( fullPath ) == false )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			byte[] data = File.ReadAllBytes( fullPath ) ;
			if( data == null )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			if( string.IsNullOrEmpty( key ) == false && string.IsNullOrEmpty( vector ) == false )
			{
				// 復号化する
				data = Decrypt( data, key, vector ) ;
			}

			return data ;
		}

		/// <summary>
		/// ローカルストレージからバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で複合化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で複合化は行わない)</param>
		/// <returns>バイト配列(null で失敗)</returns>
		public static byte[] Load( string path, int offset, int length )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			string fullPath = Combine( Path, path ) ;
			if( File.Exists( fullPath ) == false )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			int size = GetSize( path ) ;
			if( size <= 0 || offset <  0 || offset >= size )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			if( ( offset + length ) >  size )
			{
				length = size - offset ;
			}

			FileStream stream = File.OpenRead( fullPath ) ;
			if( stream == null )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			stream.Seek( offset, SeekOrigin.Begin ) ;

			byte[] data = new byte[ length ] ;
			size = stream.Read( data, 0, length ) ;

			if( size != length )
			{
				stream.Close() ;

				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Path = " + path ) ;
				#endif
				return null ;
			}

			stream.Close() ;

			return data ;
		}

		/// <summary>
		/// ローカルストレージから文字列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>文字列(null で失敗)</returns>
		public static string LoadText( string path, string key = null, string vector = null )
		{
			byte[] data = Load( path, key, vector ) ;
			if( data == null )
			{
				return null ;
			}

			if( data.Length >= 3 )
			{
				if( data[ 0 ] == 0xEF && data[ 1 ] == 0xBB && data[ 2 ] == 0xBF )
				{
					// BOM 突き
					return Encoding.UTF8.GetString( data, 3, data.Length - 3 ) ;
				}
			}

			return Encoding.UTF8.GetString( data ) ;
		}

		//-----------------------------------------------------------
#if false
		/// <summary>
		/// オブジェクトをバイナリデータ化して返す
		/// </summary>
		/// <param name="tObject"></param>
		/// <returns></returns>
		public static byte[] GetBinary<T>( T tObject ) where T : class
		{
			// iOSでは下記設定を行わないとエラーになる
#if UNITY_IOS || UNITY_IPHONE
			Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" ) ;
#endif

			BinaryFormatter tBF = new BinaryFormatter() ;
			MemoryStream tMS = new MemoryStream() ;

			tBF.Serialize( tMS, tObject ) ;
			return tMS.GetBuffer() ;
		}

		public static T SetBinary<T>( T tOverwriteObject, byte[] tData ) where T : class 
		{
			// iOSでは下記設定を行わないとエラーになる
#if UNITY_IOS || UNITY_IPHONE
			Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" ) ;
#endif

			BinaryFormatter tBF = new BinaryFormatter() ;
			MemoryStream tMS = new MemoryStream( tData ) ;

			T tObject = tBF.Deserialize( tMS ) as T ;

			if( tOverwriteObject != null && tObject != null )
			{
				tOverwriteObject  = tObject ;
			}

			return tObject ;
		}
#endif
		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージからテクスチャを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>テクスチャのインスタンス(null で失敗)</returns>
		public static Texture2D LoadTexture( string path, string key = null, string vector = null )
		{
			byte[] data = Load( path, key, vector ) ;
			if( data == null )
			{
				return null ;
			}
		
			// イメージデータは取得出来た
			Texture2D texture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;	// MipMap を事前に切るのがミソ
			texture.LoadImage( data ) ;
		
	//		Debug.Log( "ミップマップカウント:" + tTexture.mipmapCount ) ;	// これが１になってなっていればＯＫ
		
			return texture ;
		}

		/// <summary>
		/// ローカルストレージからオーディオクリップを読み出す(これは使い物にはならない)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tOutput">オーディオクリップのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tStream">ストリーミングを有効にするかどうか</param>
		/// <param name="tAudioType">ファイルの種別(wav ogg mp3)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAudioClip( string path, Action<AudioClip> onLoaded, AudioType audioType = AudioType.WAV )
		{
			if( Exists( path ) != Target.File )
			{
				yield break ;	// ファイルが存在しない
			}

			string fullPath = Combine( Path, path ) ;

			string extension = fullPath.ToLower() ;
			if( extension.IndexOf( "wav" ) >= 0 )
			{
				audioType = AudioType.WAV ;
			}
			else
			if( extension.IndexOf( "ogg" ) >= 0 )
			{
				audioType = AudioType.OGGVORBIS ;
			}
			else
			if( extension.IndexOf( "mp3" ) >= 0 )
			{
				audioType = AudioType.MPEG ;
			}

			UnityWebRequest	www = UnityWebRequestMultimedia.GetAudioClip( "file://" + fullPath, audioType ) ;
			yield return www.SendWebRequest() ;
			if( www.isHttpError || www.isNetworkError )
			{
				yield break ;	// エラー
			}
			
			AudioClip audioClip = DownloadHandlerAudioClip.GetContent( www ) ;
			www.Dispose() ;

			onLoaded?.Invoke( audioClip ) ;
		}

		/// <summary>
		/// ローカルストレージからアセットバンドルを読み出す(同期版)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>アセットバンドルのインスタンス(null で失敗)</returns>
		public static AssetBundle LoadAssetBundle( string path, string key = null, string vector = null )
		{
			if( string.IsNullOrEmpty( key ) == true && string.IsNullOrEmpty( vector ) == true )
			{
				if( Exists( path ) != Target.File )
				{
					return null ;	// ファイルが存在しない
				}

				string fullPath = Combine( Path, path ) ;

				// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
//				Debug.LogWarning( "Path:" + tPath ) ;
				return AssetBundle.LoadFromFile( fullPath ) ;
			}
			
			byte[] data = Load( path, key, vector ) ;
			if( data == null )
			{
				return null ;
			}
			
			// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
			return AssetBundle.LoadFromMemory( data ) ;
		}

		/// <summary>
		/// ローカルストレージからアセットバンドルを読み出す(非同期版)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAssetBundleAsync( string path, Action<AssetBundle> onLoaded, string key = null, string vector = null )
		{
			AssetBundleCreateRequest request ;

			if( string.IsNullOrEmpty( key ) == true && string.IsNullOrEmpty( vector ) == true )
			{
				if( Exists( path ) != Target.File )
				{
					yield break ;	// ファイルが存在しない
				}

				string fullPath = Combine( Path, path ) ;

				// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
				request = AssetBundle.LoadFromFileAsync( fullPath ) ;
				yield return request ;

				if( request.isDone == false )
				{
					yield break ;
				}

				onLoaded?.Invoke( request.assetBundle ) ;
				if( onLoaded == null && request.assetBundle != null )
				{
					request.assetBundle.Unload( true ) ;
				}		

				yield break ;
			}

			//----------------------------------------------------------

			// ここはいずれストリーミングに変えるかもしれない

			byte[] data = Load( path, key, vector ) ;
			if( data == null )
			{
				yield break ;
			}
		
			// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
			request = AssetBundle.LoadFromMemoryAsync( data ) ;
			yield return request ;

			if( request.isDone == false )
			{
				yield break ;
			}

			onLoaded?.Invoke( request.assetBundle ) ;
			if( onLoaded == null && request.assetBundle != null )
			{
				request.assetBundle.Unload( true ) ;
			}		
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージに保存されたムービーファイルをネイティブプレイヤーで再生する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tIsCancelOnInput">タップで再生を中止させられるようにするかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMovie( string path, bool isCancelOnInput )
		{
			if( Exists( path ) != Target.File )
			{
				return false ;	// ファイルが存在しない
			}

			bool result = true ;

			string fullPath = Combine( Path, path ) ;

#if UNITY_EDITOR || UNITY_STANDALONE
			Application.OpenURL( "file://" + path ) ;
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = Handheld.PlayFullScreenMovie( fullPath, Color.black, ( isCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;
#elif !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )
			result = Handheld.PlayFullScreenMovie( "file://" + fullPath, Color.black, ( isCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;
#endif
			return result ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したファイルのサイズを取得する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <returns>ファイルのサイズ(-1 でファイルが存在しない)</returns>
		public static int GetSize( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
#if UNITY_EDITOR
				Debug.LogError( "Storage Exist Error : Path = " + path ) ;
#endif
				return -1 ;
			}

			string fullPath = Combine( Path, path ) ;
		
			if( File.Exists( fullPath ) == false )
			{
				return -1 ;
			}
		
			FileInfo info = new FileInfo( fullPath ) ;
		
			return ( int )info.Length ;
		}
	
		/// <summary>
		/// 指定したフォルダに内包されるファイル名の一覧を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>ファイル名の一覧が格納された文字列の配列</returns>
		public static string[] GetFiles( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return null ;
			}

			string fullPath = Combine( Path, path ) ;

			if( Directory.Exists( fullPath ) == false )
			{
				// 対象はフォルダではない
				return null ;
			}

			return Directory.GetFiles( fullPath ) ;
		}

		/// <summary>
		/// 指定したフォルダに内包されるフォルダ名の一覧を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>フォルダ名の一覧が格納された文字列の配列</returns>
		public static string[] GetFolders( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
				return null ;
			}

			string fullPath = Combine( Path, path ) ;

			if( Directory.Exists( fullPath ) == false )
			{
				// 対象はフォルダではない
				return null ;
			}

			return Directory.GetDirectories( fullPath ) ;
		}

		/// <summary>
		/// 指定したフォルダに内包されるファイルおよびフォルダの数を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>ファイルおよびフォルダの数</returns>
		public static int GetCount( string path )
		{
			string[] fileList   = GetFiles( path ) ;
			string[] folderList = GetFolders( path ) ;

			int count = 0 ;

			if( fileList != null )
			{
				count += fileList.Length ;
			}

			if( folderList != null )
			{
				count += folderList.Length ;
			}

			return count ;
		}

		/// <summary>
		/// 指定のファイルまたはフォルダが存在するか確認する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <returns>結果(-1=名前が不正・0=存在しない・1=ファイルが存在する・2=フォルダが存在する)</returns>
		public static Target Exists( string path )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
#if UNITY_EDITOR
				Debug.LogError( "Storage Exist Error : Path = " + path ) ;
#endif
				return Target.None ;
			}

			string fullPath = Combine( Path, path ) ;

			if( File.Exists( fullPath ) == true )
			{
				return Target.File ;
			}
			else
			if( Directory.Exists( fullPath ) == true )
			{
				return Target.Folder ;
			}

			return Target.None ;
		}

		/// <summary>
		/// 指定のファイルまたはフォルダを移動する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <param name="tAbsolute">削除対象がフォルダの場合に内部にファイルまたはフォルダが存在していても強制的に削除するかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Move( string path_0, string path_1 )
		{
			if( string.IsNullOrEmpty( path_0 ) == true || string.IsNullOrEmpty( path_1 ) )
			{
#if UNITY_EDITOR
				Debug.LogError( "Storage Move Error" ) ;
#endif
				return false ;
			}

			if( path_0.Equals( path_1 ) == true )
			{
				// パスが同じ
				return true ;
			}

			string fullPath_0 = Combine( Path, path_0 ) ;
			string fullPath_1 = Combine( Path, path_1 ) ;
			
			if( File.Exists( fullPath_0 ) == false && Directory.Exists( fullPath_0 ) == false )
			{
				// 移動元のファイルかフォルダ存在しない
				return false ;
			}

			if( File.Exists( fullPath_1 ) == true || Directory.Exists( fullPath_1 ) == true )
			{
				// 移動先にファイルかフォルダが存在する
				return false ;
			}

			if( File.Exists( fullPath_0 ) == true )
			{
				// 移動元はファイル
				File.Move( fullPath_0, fullPath_1 ) ;
			}
			else
			if( Directory.Exists( fullPath_0 ) == true )
			{
				// 移動元はフォルダ
				Directory.Move( fullPath_0, fullPath_1 ) ;
			}
			
			// Apple 審査のリジェクト回避用コード
#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )
				UnityEngine.iOS.Device.SetNoBackupFlag( fullPath_1 ) ;
#endif
			return true ;
		}

		/// <summary>
		/// 指定のファイルまたはフォルダを削除する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <param name="tAbsolute">削除対象がフォルダの場合に内部にファイルまたはフォルダが存在していても強制的に削除するかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Remove( string path, bool absolute = false )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
#if UNITY_EDITOR
				Debug.LogError( "Storage Remove Error : Path = " + path ) ;
#endif
				return false ;
			}

			string fullPath = Combine( Path, path ) ;
		
			if( File.Exists( fullPath ) == true )
			{
				// ファイルが存在する
				File.Delete( fullPath ) ;

				return true ;
			}
			else
			if( Directory.Exists( fullPath ) == true )
			{
				// ディレクトリが存在する
				
				if( absolute == false )
				{
					// 内包するファイルとフォルダの数を確認する
					if( GetCount( path ) >  0 )
					{
						return false ;	// 削除は出来ない
					}
					Directory.Delete( fullPath ) ;

					return true ;
				}
				else
				{
					// 強制削除可能

					// 再帰的に内包するファイルとフォルダを全て削除する
					Directory.Delete( fullPath, true ) ;

					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 指定したフォルダ以下の内包されるファイルとフォルダが存在しないフォルダを全て削除する
		/// </summary>
		/// <param name="tName">フォルダの名前(相対パス)</param>
		public static void RemoveAllEmptyFolders( string path = "" )
		{
			path = path ?? string.Empty ;
			RemoveEmptyFolderAllLoop( Combine( Path, path ) ) ;	// ルートフォルダは残す
		}

		// 指定したフォルダ以下の内包されるファイルとフォルダが存在しないフォルダを全て削除する
		private static bool RemoveEmptyFolderAllLoop( string currentPath )
		{
			string fullPath ;
		
			//-----------------------------------------------------
		
			if( Directory.Exists( currentPath ) == false )
			{
				return false ;
			}
		
			//-----------------------------------------------------
		
			// ファイル
			int fc = 0 ;
			string[] fa = Directory.GetFiles( currentPath ) ;
			if( fa != null && fa.Length >  0 )
			{
				fc = fa.Length ;
			}

			// フォルダ
			int dc = 0 ;
			string[] da = Directory.GetDirectories( currentPath ) ;
			if( da != null && da.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				foreach( var ds in da )
				{
					fullPath = ds + "/" ;
					if( RemoveEmptyFolderAllLoop( fullPath ) == false )
					{
						// このフォルダは削除してはいけない

						dc ++ ;	// 残っているフォルダ増加
					}
					else
					{
						// このフォルダは削除して良い
						Directory.Delete( fullPath, true ) ;
					}
				}
			}

			if( fc >  0 || dc >  0 )
			{
				// このフォルダは削除してはいけない
				return false ;
			}
			else
			{
				// このフォルダは削除して良い
				return true ;
			}
		}

		//------------------------------------------------------------------------------

		/// <summary>
		/// バイト配列を暗号化する
		/// </summary>
		/// <param name="tOriginalData">暗号化前のバイト配列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>暗号化後のバイト配列</returns>
		public static byte[] Encrypt( byte[] originalData, string key, string vector )
		{
			// オリジナルのサイズがわからなくなるので保存する
			byte[] data = new byte[ 4 + originalData.Length ] ;
			long size = originalData.Length ;
		
			data[ 0 ] = ( byte )( ( size >>  0 ) & 0xFF ) ;
			data[ 1 ] = ( byte )( ( size >>  8 ) & 0xFF ) ;
			data[ 2 ] = ( byte )( ( size >> 16 ) & 0xFF ) ;
			data[ 3 ] = ( byte )( ( size >> 24 ) & 0xFF ) ;
	
			System.Array.Copy( originalData, 0, data, 4, size ) ;
		
			//-----------------------------------------------------
		
			// 暗号化用の種別オブジェクト生成
	//		TripleDESCryptoServiceProvider tKind = new TripleDESCryptoServiceProvider() ;

			RijndaelManaged kind = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize   = 256,
				BlockSize = 256
			} ;

			//-----------------------------------------------------
		
			// 暗号用のキー情報をセットする
			byte[] aKey    = Encoding.UTF8.GetBytes( key    ) ;
			byte[] aVector = Encoding.UTF8.GetBytes( vector ) ;
		
			ICryptoTransform encryptor = kind.CreateEncryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			MemoryStream memoryStream = new MemoryStream() ;
		
			// 暗号化
			CryptoStream cryptoStream = new CryptoStream( memoryStream, encryptor, CryptoStreamMode.Write ) ;
		
 			cryptoStream.Write( data, 0, data.Length ) ;
			cryptoStream.FlushFinalBlock() ;
		
			cryptoStream.Close() ;
		
			byte[] cryptoData = memoryStream.ToArray() ;
		
 			memoryStream.Close() ;
		
			//-----------------------------------------------------
		
			encryptor.Dispose() ;
		
			kind.Clear() ;
			kind.Dispose() ;
		
			//-----------------------------------------------------
		
			return cryptoData ;
		}

		/// <summary>
		/// 文字列を暗号化する
		/// </summary>
		/// <param name="tText">暗号化前の文字列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>暗号化後の文字列</returns>
		public static string Encrypt( string text, string key, string vector )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return null ;
			}
		
			byte[] originalData = Encoding.UTF8.GetBytes( text ) ;
			byte[] cryptoData = Encrypt( originalData, key, vector ) ;
		
			return Convert.ToBase64String( cryptoData ) ;
		}

		/// <summary>
		/// バイト配列を復号化する
		/// </summary>
		/// <param name="tCryptoData">暗号化されたバイト配列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>復号化されたバイト配列</returns>
		public static byte[] Decrypt( byte[] cryptoData, string key, string vector )
		{
			// 暗号化用の種別オブジェクト生成
	//		TripleDESCryptoServiceProvider tKind = new TripleDESCryptoServiceProvider() ;
		
			RijndaelManaged kind = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize   = 256,
				BlockSize = 256
			} ;

			//-----------------------------------------------------
		
			// 暗号用のキー情報をセットする
			byte[] aKey    = Encoding.UTF8.GetBytes( key    ) ;
			byte[] aVector = Encoding.UTF8.GetBytes( vector ) ;
		
			ICryptoTransform decryptor = kind.CreateDecryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			byte[] data = new byte[ cryptoData.Length ] ;
		
			//-----------------------------------------------------
		
			MemoryStream memoryStream = new MemoryStream( cryptoData ) ;
		
			// 復号化
			CryptoStream cryptoStream = new CryptoStream( memoryStream, decryptor, CryptoStreamMode.Read ) ;
		
			cryptoStream.Read( data, 0, data.Length ) ;
			cryptoStream.Close() ;
		
			memoryStream.Close() ;
 		
			//-----------------------------------------------------
		
			decryptor.Dispose() ;
		
			kind.Clear() ;
			kind.Dispose() ;
		
			//-----------------------------------------------------
		
			long size = ( ( long )data[ 0 ] <<  0 ) | ( ( long )data[ 1 ] <<  8 ) | ( ( long )data[ 2 ] << 16 ) | ( ( long )data[ 3 ] ) ;
		
			byte[] originalData = new byte[ size ] ;
			System.Array.Copy( data, 4, originalData, 0, size ) ;
		
			return originalData ;
		}
	
		/// <summary>
		/// 文字列を復号化する
		/// </summary>
		/// <param name="tText">暗号化された文字列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>復号化された文字列</returns>
		public static string Decrypt( string text, string key, string vector )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return null ;
			}

			byte[] cryptoData ;
			try
			{
				cryptoData = Convert.FromBase64String( text ) ;
			}
			catch( System.FormatException )
			{
				Debug.LogWarning( "データが壊れています" ) ;
				return string.Empty ;
			}
			byte[] originalData = Decrypt( cryptoData, key, vector ) ;
		
			return Encoding.UTF8.GetString( originalData ) ;
		}

		//-------------------------------------------------------
		
		/// <summary>
		/// バイト配列からＭＤ５のハッシュコード文字列を取得する
		/// </summary>
		/// <param name="tData">ハッシュコードを取得する対象のバイト配列</param>
		/// <returns>ハッシュコード文字列</returns>
		public static string GetMD5Hash( byte[] data )
		{
			// MD5CryptoServiceProviderオブジェクトを作成
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider() ;
		
			// ハッシュ値を計算する
			byte[] hash = md5.ComputeHash( data ) ;
		
			// リソースを解放する
			md5.Clear() ;
			md5.Dispose() ;
		
			// byte型配列を16進数の文字列に変換
			System.Text.StringBuilder result = new System.Text.StringBuilder() ;
			foreach( byte code in hash )
			{
				result.Append( code.ToString( "x2" ) ) ;
			}
		
			return result.ToString() ;
		}

		/// <summary>
		/// 文字列からＭＤ５のハッシュコード文字列を取得する
		/// </summary>
		/// <param name="tText">ハッシュコードを取得する対象の文字列</param>
		/// <returns>ハッシュコード文字列</returns>
		public static string GetMD5Hash( string text )
		{
			// 文字列をbyte型配列に変換する
			return GetMD5Hash( System.Text.Encoding.UTF8.GetBytes( text ) ) ;
		}
		
		//-------------------------------------------------------

		/// <summary>
		/// StreamingAssets からバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rData">バイト配列を格納する要素数１以上のバイト配列の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadFromStreamingAssets( string path, Action<byte[]> onLoaded, string key = null, string vector = null )
		{
			if( string.IsNullOrEmpty( path ) == true )
			{
#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Name = " + path ) ;
#endif
				yield break ;
			}


			byte[] data = null ;

#if UNITY_ANDROID && !UNITY_EDITOR

			path = Application.streamingAssetsPath + "/" + path ;
			UnityWebRequest request = UnityWebRequest.Get( path ) ;
			request.SendWebRequest() ;
			yield return request ;

			if( request.isDone == true && string.IsNullOrEmpty( request.error ) == true )
			{
				data = request.downloadHandler.data ;
			}

			request.Dispose() ;

#else
		
			path = Application.streamingAssetsPath + "/" + path ;

			if( File.Exists( path ) == true )
			{
				data = File.ReadAllBytes( path ) ;
			}

#endif

			if( data == null )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : path = " + path ) ;
#endif
				yield break ;
			}

			if( string.IsNullOrEmpty( key ) == false && string.IsNullOrEmpty( vector ) == false )
			{
				// 復号化する
				data = Encrypt( data, key, vector ) ;
			}

			onLoaded?.Invoke( data ) ;
		}

		/// <summary>
		/// StreamingAssets から文字列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rText">文字列を格納する要素数１以上の文字列の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadTextFromStreamingAssets( string path, Action<string> onLoaded, string key = null, string vector = null  )
		{
			byte[] data = null ;

			yield return LoadFromStreamingAssets( path, ( _ ) => { data = _ ; }, key, vector ) ;

			if( data != null && data.Length >  0 )
			{
				string text = Encoding.UTF8.GetString( data ) ;

				onLoaded?.Invoke( text ) ;
			}
		}

		/// <summary>
		/// StreamingAssets からテクスチャを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rTexture">テクスチャのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadTextureFromStreamingAssets( string path, Action<Texture2D> onLoaded, string key = null, string vector = null )
		{
			byte[] data = null ;

			yield return LoadFromStreamingAssets( path, ( _ ) => { data = _ ; }, key, vector ) ;

			if( data != null && data.Length >  0 )
			{
				// イメージデータは取得出来た
				Texture2D texture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;	// MipMap を事前に切るのがミソ
				texture.LoadImage( data ) ;

				onLoaded?.Invoke( texture ) ;
			}
		}

		/// <summary>
		/// StreamingAssets からアセットバンドルを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAssetBundleFromStreamingAssets( string path, Action<AssetBundle> onLoaded, string key = null, string vector = null )
		{
			AssetBundle assetBundle = null ;

			if( string.IsNullOrEmpty( key ) == true && string.IsNullOrEmpty( vector ) == true )
			{
#if UNITY_ANDROID && !UNITY_EDITOR

				path = Application.streamingAssetsPath + "/" + path ;
				UnityWebRequest request = UnityWebRequest.Get( path ) ;
				request.SendWebRequest() ;
				yield return request ;

				if( request.isDone == true && string.IsNullOrEmpty( request.error ) == true )
				{
					if( request.downloadHandler.data != null && request.downloadHandler.data.Length >  0 )
					{
						assetBundle = AssetBundle.LoadFromMemory( request.downloadHandler.data ) ;
					}
				}

				request.Dispose() ;

#else

				path = Application.streamingAssetsPath + "/" + path ;
				if( File.Exists( path ) == true )
				{
					// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
					assetBundle = AssetBundle.LoadFromFile( path ) ;
				}

#endif

				if( assetBundle != null )
				{
					onLoaded?.Invoke( assetBundle ) ;
				}

				yield break ;
			}


			byte[] data = null ;

			yield return LoadFromStreamingAssets( path, ( _ ) => { data = _ ; }, key, vector ) ;
		
			if( data != null && data.Length >  0 )
			{
				// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
				assetBundle = AssetBundle.LoadFromMemory( data ) ;

				onLoaded?.Invoke( assetBundle ) ;
			}
		}


		/// <summary>
		/// StreamingAssets に存在するムービーファイルをネイティブプレイヤーで再生する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tIsCancelOnInput">タップで再生を中止させられるようにするかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMovieFromStreamingAssets( string path, bool isCancelOnInput )
		{
			bool result = true ;
		
#if !UNITY_EDITOR && UNITY_ANDROID

			result = Handheld.PlayFullScreenMovie( path, Color.black, ( isCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;

#elif !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )

			result = Handheld.PlayFullScreenMovie( path, Color.black, ( isCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;

#endif

			return result ;
		}


		//---------------------------------------------------------------------------

		/// <summary>
		/// 固有識別子を取得する(取得毎に値が変わる可能性があるため最初に保存したものを使いまわす)
		/// </summary>
		/// <returns></returns>
		public static string GetUUID( string path = "UUID" )
		{
			string key		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;
			string vector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;

			string uuid = LoadText( path, key, vector ) ;
			if( string.IsNullOrEmpty( uuid ) == true )
			{
				// 生成する
				uuid = SystemInfo.deviceUniqueIdentifier ;
				SaveText( path, uuid, false, key, vector ) ;
			}

			return uuid ;
		}
	}
}

