using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using UnityEditor ;
using UnityEngine ;


namespace AssetSettings
{
	/// <summary>
	/// アセットパスが一致したら処理をするクラス Version 2024/08/07
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class ImportDispatcher<T> where T : AssetImporter
	{
		public				Regex[]			Paths { get ; }
		private	readonly	Func<T,bool>	m_Method ;
		private	readonly	Func<T,bool>	m_MethodForAll ;

		/// <summary>
		///     一致させたいパスパターンと、一致した際に動作させるメソッドを初期化します
		/// </summary>
		/// <param name="pathPatterns"></param>
		/// <param name="method"></param>
		public ImportDispatcher( Regex[] pathPatterns, Func<T,bool> method, Func<T,bool> methodForAll = null )
		{
			Paths			= pathPatterns ??	throw new ArgumentNullException( nameof( pathPatterns ) ) ;
			m_Method		= method ;
			m_MethodForAll	= methodForAll ;
		}

		public ImportDispatcher( string[] pathPatterns, Func<T,bool> method, Func<T,bool> methodForAll = null )
		{
			if( pathPatterns == null || pathPatterns.Length == 0 )
			{
				throw new ArgumentNullException( nameof( pathPatterns ) ) ;
			}

			int i, l = pathPatterns.Length ;
			Paths = new Regex[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Paths[ i ] = new Regex( pathPatterns[ i ] ) ;
			}

			m_Method		= method ;
			m_MethodForAll	= methodForAll ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// パスの条件に一致しているのか
		/// </summary>
		/// <param name="assetImporter"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool MatchAny( T assetImporter )
		{
			if( assetImporter == null )
			{
				throw new ArgumentNullException( nameof( assetImporter ) ) ;
			}

//			if( assetImporter.importSettingsMissing == true )
//			{
//				// meta ファイルがまだ作られていない(初回インポート) ※ワーニングを出すまでもないので一旦コメントアウト
//				Debug.LogWarning( $"{ assetImporter.assetPath } importSettings is missing." ) ;
//			}

			var assetPath = assetImporter.assetPath.Replace( '\\', '/' ) ;

			return Paths.Any( value => value.IsMatch( assetPath ) ) ;
		}

		/// <summary>
		/// パスの条件に一致しているのか
		/// </summary>
		/// <param name="assetImporter"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool MatchAny( string assetPath )
		{
			return Paths.Any( value => value.IsMatch( assetPath ) ) ;
		}

		/// <summary>
		/// パス群のいずれかの先頭部分にマッチすれば設定を行う
		/// </summary>
		/// <param name="forAssetImporter"></param>
		/// <returns></returns>
		public bool SetupAny( T assetImporter )
		{
			if( MatchAny( assetImporter ) == false )
			{
				return false ;
			}

			if( m_Method == null )
			{
				return false ;
			}

			// いずれかのパスの先頭にマッチする
			return m_Method( assetImporter ) ;
		}

		/// <summary>
		/// パス群のいずれかの先頭部分にマッチすれば設定を行う
		/// </summary>
		/// <param name="forAssetImporter"></param>
		/// <returns></returns>
		public bool SetupAny( string assetPath )
		{
			if( MatchAny( assetPath ) == false )
			{
				return false ;
			}

			if( m_Method == null )
			{
				return false ;
			}

			AssetImporter assetImporter = AssetImporter.GetAtPath( assetPath ) ;
			if( assetImporter == null )
			{
				return false ;
			}

			// いずれかのパスの先頭にマッチする
			return m_Method( assetImporter as T ) ;
		}

		/// <summary>
		/// 設定を行う
		/// </summary>
		/// <param name="forAssetImporter"></param>
		/// <returns></returns>
		public bool Setup( T assetImporter )
		{
			if( m_Method == null )
			{
				return false ;
			}

			return m_Method( assetImporter ) ;
		}

		/// <summary>
		/// 設定を行う
		/// </summary>
		/// <param name="forAssetImporter"></param>
		/// <returns></returns>
		public bool Setup( string assetPath )
		{
			if( m_Method == null )
			{
				return false ;
			}

			AssetImporter assetImporter = AssetImporter.GetAtPath( assetPath ) ;
			if( assetImporter == null )
			{
				return false ;
			}

			return m_Method( assetImporter as T ) ;
		}

		/// <summary>
		/// 対象全てに対して処理を施す
		/// </summary>
		/// <returns></returns>
		public bool SetupAll( List<string> targetPaths = null )
		{
			if( m_MethodForAll == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			string message =  "処理は長時間かかる可能性があります\n本当に実行してよろしいですか？" ;

			string targetFilePath	= null ;
			string targetFolderPath = null ;

			if( targetPaths == null || targetPaths.Count <= 1 )
			{
				// 全体または１つが対象

				string targetPath = null ;
				if( targetPaths.Count == 1 )
				{
					targetPath = targetPaths[ 0 ] ;
				}

				if( string.IsNullOrEmpty( targetPath ) == false )
				{
					// パスの指定がある
					targetPath = targetPath.Replace( '\\', '/' ) ;
					if( File.Exists( targetPath ) == true )
					{
						// 単体ファイルを指定している
						targetFilePath = targetPath ;
					}
					else
					if( Directory.Exists( targetPath ) == true )
					{
						// 単体フォルダを指定している
						targetFolderPath = targetPath ;
					}

					if( string.IsNullOrEmpty( targetFilePath ) == false )
					{
	//					if( MatchAny( targetFilePath ) == true )
	//					{
							message += "\n[対象ファイル]\n" + targetFilePath ;
	//					}
					}
					else
					if( string.IsNullOrEmpty( targetFolderPath ) == false )
					{
	//					if( MatchAny( targetFolderPath ) == true )
	//					{
							message += "\n[対象フォルダ]\n" + targetFolderPath ;
	//					}
					}
				}
			}
			else
			{
				// 複数を選択している

				message += "\n[対象ファイル・フォルダ]\n" + targetPaths.Count ;
			}

			if( EditorUtility.DisplayDialog( "アセット再設定の実行確認", message, "はい", "いいえ" ) == false )
			{
				return false ;
			}

			//----------------------------------------------------------

			bool isDirty = false ;
			int count = 0 ;

			try
			{
				// バッチ処理中を設定する
				RootAssetPostprocessor.isBatching = true ;

				// プログラム変更の反映禁止
				EditorApplication.LockReloadAssemblies() ;

				// バッチ処理開始
				AssetDatabase.StartAssetEditing() ;

				//---------------------------------------------------------

				if( targetPaths == null || targetPaths.Count <= 1 )
				{
					if( targetFilePath == null && targetFolderPath == null )
					{
						// 全体対象
						foreach( var path in Paths )
						{
							string folderPath = path.ToString().Replace( '\\', '/' ) ;
							int p = folderPath.LastIndexOf( '/' ) ;
							if( p >= 0 )
							{
								folderPath = folderPath[ ..p ] ;
								ProcessFolder( folderPath ) ;
							}
						}
					}
					else
					if( string.IsNullOrEmpty( targetFilePath ) == false )
					{
//						Debug.Log( "対象ファイル : " + targetFilePath ) ;

						// ファイル指定
						ProcessFile( targetFilePath ) ;
					}
					else
					if( string.IsNullOrEmpty( targetFolderPath ) == false )
					{
						// フォルダ指定
						ProcessFolder( targetFolderPath ) ;
					}
				}
				else
				{
					// 複数対象
					foreach( var targetPath in targetPaths )
					{
						string path = targetPath.Replace( '\\', '/' ) ;

						if( File.Exists( path ) == true )
						{
							// ファイル指定
							ProcessFile( path ) ;
						}
						else
						if( Directory.Exists( path ) == true )
						{
							// フォルダ指定
							ProcessFolder( path ) ;
						}
					}
				}

				//-------------

				// 指定したファイル内の対象ファイルを処理する
				void ProcessFile( string filePath )
				{
					if( File.Exists( filePath ) == true )
					{
						string path = filePath ;

						bool isTarget = true ;
						int invalidIndex = path.IndexOf( "/#" ) ;
						if( invalidIndex >= 0 )
						{
							// 処理無効フォルダ内のファイルである可能性がある
							int validIndex = path.IndexOf( "/#@" ) ;
							if( validIndex <  0 || invalidIndex <  validIndex )
							{
								// /# しかない・/# の方が /#@ より先にある
								isTarget = false ;	// このパスを無効とする
							}
						}

						if( isTarget == true )
						{
							T assetImporter = AssetImporter.GetAtPath( path ) as T  ;
							if( assetImporter != null && m_MethodForAll( assetImporter ) == true )
							{
								isDirty = true ;
								count ++ ;
							}
						}
					}
					else
					{
						Debug.LogWarning( "Not found file : " + filePath ) ;
					}
				}

				// 指定したフォルダ内の対象ファイルを処理する
				void ProcessFolder( string folderPath )
				{
					if( Directory.Exists( folderPath ) == true )
					{
						string[] assetFileFullPaths = Directory.GetFiles( folderPath, "*", SearchOption.AllDirectories ) ;
						if( assetFileFullPaths != null && assetFileFullPaths.Length >  0 )
						{
							int i, l = assetFileFullPaths.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								string assetFileFullPath = assetFileFullPaths[ i ] ;
//								Debug.Log( "TargetPath:" + targetPath ) ;
								assetFileFullPath = assetFileFullPath.Replace( '\\', '/' ) ;

								bool isTarget = true ;
								int invalidIndex = assetFileFullPath.IndexOf( "/#" ) ;
								if( invalidIndex >= 0 )
								{
									// 処理無効フォルダ内のファイルである可能性がある
									int validIndex = assetFileFullPath.IndexOf( "/#@" ) ;
									if( validIndex <  0 || invalidIndex <  validIndex )
									{
										// /# しかない・/# の方が /#@ より先にある
										isTarget = false ;	// このパスを無効とする
									}
								}

								if( isTarget == true )
								{
									T assetImporter = AssetImporter.GetAtPath( assetFileFullPath ) as T  ;
									if( assetImporter != null && m_MethodForAll( assetImporter ) == true )
									{
										isDirty = true ;
										count ++ ;
									}
								}
							}
						}
					}
					else
					{
						Debug.LogWarning( "Not found folder : " + folderPath ) ;
					}
				}
			}
			catch( Exception e )
			{
				Debug.LogException( e ) ;
			}
			finally
			{
				float time = Time.realtimeSinceStartup ;

				// バッチ処理終了
				AssetDatabase.StopAssetEditing() ;

				Debug.Log( "<color=#00FFFF>Processint Time : " + GetTimeName( Time.realtimeSinceStartup - time ) + " ( " + count + " )</color>" ) ;

				// 保存が必要な場合のみファイルに保存する
				if( isDirty == true )
				{
					Debug.Log( "<color=#FF7F00>[SaveAssets]</color>" ) ;
					AssetDatabase.SaveAssets() ;
				}

				// アセットの状態を更新する
				AssetDatabase.Refresh() ;

				// プログラム変更の反映許可
				EditorApplication.UnlockReloadAssemblies() ;

				// バッチ処理中を解除する
				RootAssetPostprocessor.isBatching = false ;
			}

			EditorUtility.DisplayDialog( "アセットの再設定", "正常に終了しました", "とじる" ) ;

			return isDirty ;
		}

		// 時間名を取得する
		private string GetTimeName( float t )
		{
			int s = ( int )( t * 10 ) ;

			int h = 0 ;
			if( s >  36000 )
			{
				h = s / 36000 ;
				s %=    36000 ;
			}

			int m = 0 ;
			if( s >  600 )
			{
				m = s / 600 ;
				s %=    600 ;
			}

			string n = string.Empty ;
			if( h >  0 )
			{
				n += h + "時間" ;
			}

			if( m >  0 )
			{
				n += m + "分" ;
			}

			if( h >  0 || m >  0 )
			{
				n += ( s / 10 ) + "秒" ;
			}
			else
			{
				n += ( ( float )s / 10 ) + "秒" ;
			}

			return n ;
		}
	}

	/// <summary>
	/// <see cref="ImportDispatcher{T}" /> の配列版拡張メソッド
	/// </summary>
	internal static class ImportDispatcherArrayExtensions
	{
		/// <summary>
		///  <see cref="ImportDispatcher{T}" />配列内で、いずれかの条件が一致しているのか
		/// </summary>
		/// <param name="self"></param>
		/// <param name="assetImporter"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool MatchAny<T>( this ImportDispatcher<T>[] self, T assetImporter ) where T : AssetImporter
		{
			if( self == null )
			{
				throw new ArgumentNullException( nameof( self ) ) ;
			}

			if( assetImporter == null )
			{
				throw new ArgumentNullException( nameof( assetImporter ) ) ;
			}

			return self.Any( dispatcher => dispatcher.MatchAny( assetImporter ) ) ;
		}

		/// <summary>
		///  <see cref="ImportDispatcher{T}" />配列内で、いずれかの条件が一致しているのか
		/// </summary>
		/// <param name="self"></param>
		/// <param name="assetImporter"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool MatchAny<T>( this ImportDispatcher<T>[] self, string assetPath ) where T : AssetImporter
		{
			if( self == null )
			{
				throw new ArgumentNullException( nameof( self ) ) ;
			}

			return self.Any( dispatcher => dispatcher.MatchAny( assetPath ) ) ;
		}

		/// <summary>
		/// <see cref="ImportDispatcher{T}" />配列内で、完全に一致したら処理をします
		/// </summary>
		/// <param name="self"></param>
		/// <param name="assetImporter"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool SetupAny<T>( this ImportDispatcher<T>[] self, T assetImporter ) where T : AssetImporter
		{
			if( self == null )
			{
				throw new ArgumentNullException( nameof( self ) ) ;
			}

			if( assetImporter == null )
			{
				throw new ArgumentNullException( nameof( assetImporter ) ) ;
			}

			var dispatcher = self.FirstOrDefault( value => value.MatchAny( assetImporter ) ) ;
			if( dispatcher == null )
			{
				return false ;	// Not match
			}

			dispatcher.Setup( assetImporter ) ;

			return true ;
		}
	}
}
