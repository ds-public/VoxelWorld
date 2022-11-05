using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using JsonHelper ;

namespace DSW
{
	// 参考
	// https://github.com/neuecc/MessagePack-CSharp

	/// <summary>
	/// データ処理のユーティリティ Version 2022/09/19 0
	/// </summary>
	public class DataPacker
	{
#if false
		// データタイプを取得する
		private static Settings.DataTypes GetDataType()
		{
			Settings.DataTypes dataType = Settings.DataTypes.MessagePack ;

			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				dataType = settings.DataType ;
			}

			return dataType ;
		}
#endif

		/// <summary>
		/// オブジェクトをシリアライズする(引数のクラスタイプ指定だとIL2CPPで動かないのでジェネリックによるクラス指定メソッドを使用すること)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="type"></param>
		/// <param name="isCompression"></param>
		/// <returns></returns>
		public static byte[] Serialize<T>( T dataObject, bool isCompression = false, Settings.DataTypes dataType = Settings.DataTypes.MessagePack )
		{
			if( dataObject == null )
			{
				return null ;
			}

//			var dataType = GetDataType() ;

			byte[] dataBuffer = null ;

			if( dataType == Settings.DataTypes.MessagePack )
			{
				// MessagePack 版(属性が必要)

				//---------------------------------------------------------
				// オプション設定

				MessagePackSerializerOptions options = MessagePackSerializer.DefaultOptions ;

				// 圧縮に対応
				options.WithCompression( isCompression ? MessagePackCompression.Lz4BlockArray : MessagePackCompression.None ) ;

				//---------------------------------------------------------

				try
				{
					dataBuffer = MessagePackSerializer.Serialize( dataObject, options ) ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "[MessagePack - Serialize] 例外発生 : " + e.Message ) ;
					Debug.LogWarning( "[Object]" + dataObject ) ;
				}
			}
			else
			if( dataType == Settings.DataTypes.Json )
			{
				// Json 版(属性が必要)
				string text = JsonUtility.ToJson( dataObject ) ;

//				Debug.Log( "<color=#00FF00>リクエスト:" + text + "</color>" ) ;

				dataBuffer = Encoding.UTF8.GetBytes( text ) ;

				if( isCompression == true )
				{
					dataBuffer = GZip.Compress( dataBuffer ) ;
				}
			}
			else
			{
				// 失敗
				return null ;
			}

			return dataBuffer ;
		}

		/// <summary>
		/// オブジェクトをデシリアライズする(引数のクラスタイプ指定だとIL2CPPで動かないのでジェネリックによるクラス指定メソッドを使用すること)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dadaBuffer"></param>
		/// <param name="isDecompression"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T Deserialize<T>( byte[] dataBuffer, bool isCompression = false, Settings.DataTypes dataType = Settings.DataTypes.MessagePack )
		{
			if( dataBuffer.IsNullOrEmpty() == true )
			{
				return default ;
			}

//			var dataType = GetDataType() ;

			T dataObject = default ;

			if( dataType == Settings.DataTypes.MessagePack )
			{
				// MessagePack 版(属性が必要)

				//---------------------------------------------------------
				// オプション設定

				MessagePackSerializerOptions options = MessagePackSerializer.DefaultOptions ;

				// 圧縮に対応
				options.WithCompression( isCompression ? MessagePackCompression.Lz4BlockArray : MessagePackCompression.None ) ;

				//---------------------------------------------------------

				try
				{
					dataObject = MessagePackSerializer.Deserialize<T>( dataBuffer, options ) ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "[MessagePack - Deserialize] 例外発生 : " + e.Message ) ;
					Debug.LogWarning( "Data Size : " + dataBuffer.Length ) ;
				}
			}
			else
			if( dataType == Settings.DataTypes.Json )
			{
				// Json 版(属性が必要)
				if( isCompression == true )
				{
					dataBuffer = GZip.Decompress( dataBuffer ) ;
				}

				string text = Encoding.UTF8.GetString( dataBuffer ) ;

//				Debug.Log( "<color=#00FF00>レスポンス:" + text + "</color>" ) ;

				dataObject = JsonUtility.FromJson<T>( text ) ;
			}
			else
			{
				// 失敗
				return default ;
			}

			return dataObject ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 単独系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static T LoadObjectFromJson<T>( string path ) where T : class
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
//				Debug.LogError( "データ異常:" + path ) ;
				return null ;
			}

			JsonObject jo = new JsonObject( ta.text ) ;

//			Debug.LogWarning( "Json:\n" + jo.ToString( "\t" ) ) ;
			return JsonUtility.FromJson<T>( jo.ToString() ) ;
		}
		
		/// <summary>
		/// 単独系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static async UniTask<T> LoadObjectFromJsonAsync<T>( string path, Action<T> onAction = null ) where T : class
		{
			TextAsset ta = await Asset.LoadAsync<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
				Debug.LogWarning( "データ異常 : " + path ) ;
				return null ;
			}

			JsonObject jo = new JsonObject( ta.text ) ;

//			Debug.LogWarning( "Json:\n" + jo.ToString( "\t" ) ) ;
			T result = JsonUtility.FromJson<T>( jo.ToString() ) ;
			onAction?.Invoke( result ) ;
			return result ;
		}
		
		/// <summary>
		/// 配列系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<T> LoadArrayFromJson<T>( string path ) where T : class
		{
			TextAsset ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
//				Debug.LogError( "データ異常:" + path ) ;
				return null ;
			}

			JsonArray ja = new JsonArray( ta.text ) ;
			int i, l = ja.Length ;

			List<T> list = new List<T>() ;

			T o ;
			for( i  = 0 ; i <  l ; i ++ )
			{
//				Debug.LogWarning( "Json:\n" + ( ja[ i ] as JsonObject ).ToString( "\t" ) ) ;
				o = JsonUtility.FromJson<T>( ( ja[ i ] as JsonObject ).ToString() ) ;
				list.Add( o ) ;
			}

			return list ;
		}

		/// <summary>
		/// 配列系のテストデータを展開する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public static async UniTask<List<T>> LoadArrayFromJsonAsync<T>( string path, Action<List<T>> onAction = null ) where T : class
		{
			TextAsset ta = await Asset.LoadAsync<TextAsset>( path ) ;
			if( ta == null || string.IsNullOrEmpty( ta.text ) == true )
			{
				Debug.LogWarning( "データ異常 : " + path ) ;
				return null ;
			}

			JsonArray ja = new JsonArray( ta.text ) ;
			int i, l = ja.Length ;

			List<T> list = new List<T>() ;

			T o ;
			for( i  = 0 ; i <  l ; i ++ )
			{
//				Debug.LogWarning( "Json:\n" + ( ja[ i ] as JsonObject ).ToString( "\t" ) ) ;
				o = JsonUtility.FromJson<T>( ( ja[ i ] as JsonObject ).ToString() ) ;
				list.Add( o ) ;
			}

			onAction?.Invoke( list ) ;
			return list ;
		}
	}
}
