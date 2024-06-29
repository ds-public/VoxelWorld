using System ;
using System.Collections ;
using System.IO ;
using System.Text ;
using System.Security.Cryptography ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using StorageHelper ;


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
		// ハッシュ生成インスタンス
		//		private static MD5CryptoServiceProvider mHashGenerator = new MD5CryptoServiceProvider() ;
		private static readonly HMACSHA256 m_HashGenerator = new ( new byte[]{ 0, 1, 2, 3 } ) ;	// コンストラクタに適当なキー値を入れる事(でないと毎回ランダムになってしまう)

		// ハッシュコードを計算する
		private static string GetHash( string fileName )
		{
			if( string.IsNullOrEmpty( fileName ) == true )
			{
				return string.Empty ;
			}

			var data = Encoding.UTF8.GetBytes( fileName ) ;
			return GetHash( data ) ;
		}

		// ハッシュコードを計算する
		private static string GetHash( byte[] data )
		{
			var hash = m_HashGenerator.ComputeHash( data ) ;

			string text = string.Empty ;
			foreach( var code in hash )
			{
				text += code.ToString( "x2" ) ;
			}

			return text ;
		}

		private static string GetFullPath( string fileName )
		{
			string path = m_Instance.m_DataPath ;

			// 難読化
			if( m_Instance.m_SecurityEnabled == true )
			{
				fileName = GetHash( fileName ) ;
			}

			if( string.IsNullOrEmpty( fileName ) == false )
			{
				path = $"{path}/{fileName}" ;
			}

			return path ;
		}

		private const string m_Key    = "lkirwf897+22#bbtrm8814z5qq=498j5" ;	// RM  用 32 byte
	
		// 初期化ベクタ
		private const string m_Vector = "741952hheeyy66#cs!9hjv887mxx7@8y" ;    // 16 byte


#if UNITY_EDITOR
		// ローカルからのデータの読み出し
		private static byte[] File_Load( string fullPath )
		{
			// 基本的に素のデータは暗号化しない
			if( File.Exists( fullPath ) == false )
			{
				return null ;
			}
			return File.ReadAllBytes( fullPath ) ;
		}

		private static string File_LoadText( string fullPath )
		{
			var data = File_Load( fullPath ) ;
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
#endif

		// ローカルストレージからのテキストの読み出し
		private static string StorageAccessor_LoadText( string fileName, string key = null, string vector = null )
		{
			// セキュリティが有効であれば復号化する
			if( m_Instance.m_SecurityEnabled == true )
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					key	= m_Key ;
				}
				if( string.IsNullOrEmpty( vector ) == true )
				{
					vector	= m_Vector ;
				}
			}
			return StorageAccessor.LoadText( GetFullPath( fileName ), key, vector ) ;
		}

		// ローカルストレージへテキストの書き込み
		private static bool StorageAccessor_SaveText( string fileName, string text, bool makeFolder = false, string key = null, string vector = null )
		{
			// セキュリティが有効であれば暗号化する
			if( m_Instance.m_SecurityEnabled == true )
			{
				if( string.IsNullOrEmpty( key ) == true )
				{
					key	= m_Key ;
				}
				if( string.IsNullOrEmpty( vector ) == true )
				{
					vector	= m_Vector ;
				}
			}

			return StorageAccessor.SaveText( GetFullPath( fileName ), text, makeFolder, key, vector ) ;
		}

		// ローカルストレージへのバイナリの書き込み(同期)
		private static bool StorageAccessor_Save( string fileName, byte[] data, bool makeFolder = true, string key = null, string vector = null )
		{
			return StorageAccessor.Save( GetFullPath( fileName ), data, makeFolder, key, vector ) ;
		}

		// ローカルストレージへのバイナリの書き込み(非同期)
		private static IEnumerator StorageAccessor_SaveAsync( string fileName, byte[] data, bool makeFolder = true, string key = null, string vector = null, Action<float> onProgress = null, Action<bool> onResult = null, CancellationToken cancellationToken = default )
		{
			return StorageAccessor.SaveAsync( GetFullPath( fileName ), data, makeFolder, key, vector, onProgress, onResult, cancellationToken ) ;
		}


		// ローカルストレージのストリーム操作開始
		private static FileStream StorageAccessor_Open( string fileName, StorageAccessor.FileOperationTypes fileOperationType = StorageAccessor.FileOperationTypes.CreateAndWrite, bool makeFolder = false )
		{
			return StorageAccessor.Open( GetFullPath( fileName ), fileOperationType, makeFolder ) ;
		}

		// ローカルストレージのストリームへの書き込み(同期)
		private static bool StorageAccessor_Write( FileStream file, byte[] data, long offset = 0, long length = 0 )
		{
			return StorageAccessor.Write( file, data, offset, length ) ;
		}

		// ローカルストレージのストリームへの書き込み(非同期)
		private static IEnumerator StorageAccessor_WriteAsync( FileStream file, byte[] data, long offset = 0, long length = 0, long seekPosition = -1, Action<bool> onResult = null, CancellationToken token = default )
		{
			return StorageAccessor.WriteAsync( file, data, offset, length, seekPosition, onResult, token ) ;
		}

		// ローカルストレージのストリーム操作終了
		private static bool StorageAccessor_Close( string fileName, FileStream file )
		{
			return StorageAccessor.Close( GetFullPath( fileName ), file ) ;
		}

		// ローカルストレージでのパスの取得
		private static string StorageAccessor_GetPath( string fileName )
		{
			return StorageAccessor.GetPath( GetFullPath( fileName ) ) ;
		}

		// StreamingAssestsでのパスの取得
		private static string StorageAccessor_GetPathFromStreamingAssets( string path )
		{
			return StorageAccessor.GetPathInStreamingAssets( path ) ;
		}


		// ローカルストレージへのファイルの存在確認
		private static StorageAccessor.TargetTypes StorageAccessor_Exists( string fileName )
		{
			return StorageAccessor.Exists( GetFullPath( fileName ) ) ;
		}

		// StreamingAssetsのファイルの存在確認(ダイレクトアクセスが可能な環境のみ有効)
		private static bool StorageAccesor_ExistsInStreamingAssets( string path )
		{
			return StorageAccessor.ExistsInStreamingAssets( path ) ;
		}

		// ローカルストレージからのファイルサイズの取得
		private static long StorageAccessor_GetSize( string fileName )
		{
			return StorageAccessor.GetSize( GetFullPath( fileName ) ) ;
		}

		// ローカルストレージへのファイルの削除
		private static bool StorageAccessor_Remove( string fileName, bool absolute = false )
		{
			return StorageAccessor.Remove( GetFullPath( fileName ), absolute ) ;
		}

		// ローカルストレージでの空フォルダの削除
		private static void StorageAccessor_RemoveAllEmptyFolders( string fileName = "" )
		{
			StorageAccessor.RemoveAllEmptyFolders( GetFullPath( fileName ) ) ;
		}

		// ローカルストレージからのアセットバンドルの取得(同期版)
		private static AssetBundle StorageAccessor_LoadAssetBundle( string fileName, string key = null, string vector = null )
		{
			return StorageAccessor.LoadAssetBundle( GetFullPath( fileName ), key, vector ) ;
		}

#if false
		// ローカルストレージからのアセットバンドルの取得(非同期版)
		private static IEnumerator StorageAccessor_LoadAssetBundleAsync( string fileName, Action<AssetBundle> onLoaded, string key = null, string vector = null )
		{
			return StorageAccessor.LoadAssetBundleAsync( GetFullPath( fileName ), onLoaded, key, vector ) ;
		}
#endif

		// ストリーミングアセッツからのアセットバンドルの取得(同期版)
		private static AssetBundle StorageAccessor_LoadAssetBundleFromStreamingAssets( string path, string key = null, string vector = null )
		{
			return StorageAccessor.LoadAssetBundleFromStreamingAssets( path, key, vector ) ;
		}
	}
}
