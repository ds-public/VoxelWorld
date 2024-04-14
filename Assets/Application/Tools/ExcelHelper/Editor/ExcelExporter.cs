//#define ENV
//#define CRLF
#define LF

using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEditor ;

using NPOI.HSSF.UserModel ;
using NPOI.XSSF.UserModel ;
using NPOI.SS.UserModel ;

namespace Tools.ForExcel
{
	/// <summary>
	/// Excel Exporter Version 2023/08/18 0
	/// </summary>
	public class ExcelExporter : AssetPostprocessor
	{
#if ENV
		private static readonly string m_ReturnCode = Environment.NewLine ;
#endif
#if CRLF
		private static readonly string m_ReturnCode = "\x0D\x0A" ;
#endif
#if LF
		private static readonly string m_ReturnCode = "\x0A" ;
#endif

		//------------------------------------

		// 出力先
		private const string m_SymbolSgheetName = "@Export" ;

		// 拡張子
		private const string m_Extension = ".txt" ;

		//------------------------------------

		/// <summary>
		/// シート情報
		/// </summary>
		public class Sheet
		{
			/// <summary>
			/// シート名
			/// </summary>
			public string Name ;

			/// <summary>
			/// テキスト
			/// </summary>
			public string Text ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ファイルがインポートされると呼び出される
		/// </summary>
		/// <param name="importedAssets"></param>
		/// <param name="deletedAssets"></param>
		/// <param name="movedAssets"></param>
		/// <param name="movedFromAssetPaths"></param>
		public static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{
			char separatorCode = ',' ;

			foreach( string importedAsset in importedAssets )
			{
				if( importedAsset.EndsWith( ".xls" ) == true )
				{
					ReadXLS( importedAsset, separatorCode ) ;
				}
				else
				if( importedAsset.EndsWith( ".xlsx" ) == true )
				{
					ReadXLSX( importedAsset, separatorCode ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// シートをＣＳＶ形式で保存する
		private static void Save( string path, IWorkbook book, List<Sheet> sheets )
		{
			var overwritePath = new Dictionary<string, string>() ;

			// 出力先パス情報を取得する
			string rootPath = GetPath( path, book, ref overwritePath ) ;

			if( string.IsNullOrEmpty( rootPath ) == true )
			{
				return ;	// 出力先情報を持ったシートが存在しない
			}

			//----------------------------------

			string symbolSheetName = m_SymbolSgheetName.ToLower() ;

			int folderIndex, totalExecute = 0 ;
			string folderName ;

			string name, text ;

			foreach( var sheet in sheets )
			{
				name = sheet.Name ;
				text = sheet.Text ;

				//---------------------------------

				if( string.IsNullOrEmpty( name ) == false && string.IsNullOrEmpty( text ) == false && name.ToLower() != symbolSheetName )
				{
					if( overwritePath.ContainsKey( name ) == false || ( overwritePath.ContainsKey( name ) == true && string.IsNullOrEmpty( overwritePath[ name ] ) == true ) )
					{
						path = rootPath + name ;

						if( name.IndexOf( '.' ) <  0 )
						{
							path += m_Extension ;
						}
					}
					else
					{
						path = overwritePath[ name ] ;
						if( path.Length >  0 && path[ ^1 ] != '/' )
						{
							path = rootPath + path ;
						}
					}

					// パスにフォルダが含まれているかチェックする
					folderIndex = path.LastIndexOf( '/' ) ;
					if( folderIndex >= 0 )
					{
						// フォルダが含まれている
						folderName = path[ ..folderIndex ] ;

						if( Directory.Exists( folderName ) == false )
						{
							// フォルダを生成する(多階層をまとめて生成出来る)
							Directory.CreateDirectory( folderName ) ;
						}
					}

					// 文字列をバイナリ化する(UTF-8N)
					byte[] data = Encoding.UTF8.GetBytes( text ) ;

					if( data != null && data.Length >= 3 )
					{
						if( data[ 0 ] == ( byte )0xEF && data[ 1 ] == ( byte )0xBB && data[ 2 ] == ( byte )0xBF )
						{
							// 先頭にＢＯＭが付与されていたら削る
							int l = data.Length - 3 ;
							byte[] work = new byte[ l ] ;
							if( l  >  0 )
							{
								Array.Copy( data, 3, work, 0, l ) ;
							}
							data = work ;
						}
					}

					bool execute = true ;

					if( File.Exists( path ) == true )
					{
						// 既にファイルが存在する場合は読み出して比較する
						byte[] work = File.ReadAllBytes( path ) ;
						if( work != null && data.Length == work.Length )
						{
							int i, l = work.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( data[ i ] != work[ i ] )
								{
									break ;
								}
							}
							if( i >= l )
							{
								execute = false ;	// 完全に同一ファイルなので書き込みを行わない
							}
						}
					}

					if( execute == true )
					{
						File.WriteAllBytes( path, data ) ;
						Debug.Log( "Sheet Exported : " + name +  " --> " + path ) ;
						totalExecute ++ ;
					}
				}
			}
		}

		// 出力先情報を取得する
		private static string GetPath( string path, IWorkbook book, ref Dictionary<string,string> overwritePath )
		{
			string symbolSheetName = m_SymbolSgheetName.ToLower() ;
			string sheetName ;

			//----------------------------------------------------------

			int numberOfSheets = book.NumberOfSheets ;
 
			ISheet sheet = null ;
			
			int sheetIndex ;

			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				
				sheetName = "" ;
				if( string.IsNullOrEmpty( sheet.SheetName ) == false )
				{
					sheetName = sheet.SheetName.ToLower() ;
				}

				if( sheetName == symbolSheetName )
				{
					break ;
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				return null ;	// 必要なシート情報が取得出来ず
			}

			//----------------------------------

			// デフォルトの出力パス
			string rootPath = path ;
			if( rootPath.Length >  0 && rootPath[ 0 ] == '/' )
			{
				// 先頭にスラッシュがあれば削除する
				rootPath = rootPath.Remove( 0, 1 ) ;
			}

			int index = rootPath.LastIndexOf( '/' ) ;
			if( index >= 0 )
			{
				rootPath = rootPath[ ..( index + 1 ) ] ;
			}

			//-------------------------------------------------

			// そのシートの最終行
			int lastRowNumber = sheet.LastRowNum ;

			IRow row ;
			int lastCellNumber ;
			ICell cell ;

			if( lastRowNumber >= 0 )
			{
				row = sheet.GetRow( 0 ) ;
				if( row != null )
				{
					lastCellNumber = row.LastCellNum ;
					if( lastCellNumber >= 0 )
					{
						cell = row.GetCell( 0 ) ;
						if( cell != null )
						{
							// ルートパスの指定があった
							rootPath = cell.ToString() ;

							if( rootPath.Length >  0 && rootPath[ 0 ] == '/' )
							{
								// 先頭にスラッシュがあれば削除する
								rootPath = rootPath.Remove( 0, 1 ) ;
							}
						}
					}
				}
			}

			int length = rootPath.Length ;
			if( length >  0 )
			{
				if( rootPath[ length - 1 ] != '/' )
				{
					rootPath += "/" ;
				}
			}
			else
			{
				rootPath = "/" ;
			}

			// 相対パス指定の場合はソースファイルのパスに連結する
			if( rootPath.IndexOf( "./" ) == 0 || rootPath.IndexOf( "../" ) == 0 )
			{
				string basePath = path ;
				int p = basePath.LastIndexOf( '/' ) ;
				if( p >= 0 )
				{
					basePath = basePath[ ..p ] ;
				}

				if( rootPath.IndexOf( "./" ) == 0 )
				{
					rootPath = basePath + rootPath[ 1.. ] ;
				}
				else
				if( rootPath.IndexOf( "../" ) == 0 )
				{
					p = basePath.LastIndexOf( '/' ) ;
					if( p >= 0 )
					{
						basePath = basePath[ ..p ] ;
					}
					rootPath = basePath + rootPath[ 2.. ] ;
				}
			}

			if( lastRowNumber >= 1 )
			{
				// 特殊パス指定あり
				string specificName, specificPath ;

				int rowIndex ;
				for( rowIndex  = 1 ; rowIndex <= lastRowNumber ; rowIndex ++ )
				{
					row = sheet.GetRow( rowIndex ) ;
					if( row != null )
					{
						lastCellNumber = row.LastCellNum ;
						if( lastCellNumber >= 1 )
						{
							specificName = "" ;
							cell = row.GetCell( 0 ) ;
							if( cell != null )
							{
								specificName = cell.ToString() ;
							}

							specificPath = "" ;
							cell = row.GetCell( 1 ) ;
							if( cell != null )
							{
								specificPath = cell.ToString() ;
								length = specificPath.Length ;
								if( length >  0 && specificPath[ length - 1 ] == '/' )
								{
									if( string.IsNullOrEmpty( specificName ) == false )
									{
										specificPath += specificName ;
									}

									if( specificName.IndexOf( '.' ) <  0 )
									{
										specificPath += m_Extension ;

									}
								}
							}

							if( string.IsNullOrEmpty( specificName ) == false && string.IsNullOrEmpty( specificPath ) == false )
							{
								if( overwritePath.ContainsKey( specificName ) == false )
								{
									overwritePath.Add( specificName, specificPath ) ;
								}
							}
						}
					}
				}
			}

			return rootPath ;
		}

		//-------------------------------------------------------------------------------------------

		// .xls タイプのファイルを開く
		private static ( List<Sheet>, string ) ReadXLS( string bookFilePath, char separatorCode )
		{
			int i = bookFilePath.LastIndexOf( '/' ) + 1 ;
			if( bookFilePath.IndexOf( "~$" ) == i )
			{
				return ( null, string.Empty ) ;
			}
			
			//----------------------------------------------------------

			FileStream fs = null ;

			try
			{
				fs = new( bookFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;
			}
			catch( DirectoryNotFoundException )
			{
				return ( null, "File not found. : " + bookFilePath ) ;
			}
			catch( FileNotFoundException )
			{
				return ( null, "File not found. : " + bookFilePath ) ;
			}
			catch( IOException )
			{
				if( File.Exists ( bookFilePath ) == false )
				{
					return ( null, "File not found. : " + bookFilePath ) ;
				}
			}
			catch( Exception )
			{
				return ( null, "File not found. : " + bookFilePath ) ;
			}

			//----------------------------------

			List<Sheet> sheets = null ;

			if( fs != null )
			{
				var book = new HSSFWorkbook( fs ) ;
				sheets = ReadBook( book, separatorCode ) ;
				if( sheets != null )
				{
					Save( bookFilePath, book, sheets ) ;
				}

				fs.Close() ;
			}

			return ( sheets, string.Empty ) ;
		}

		// .xlsx タイプのファイルを開く
		private static ( List<Sheet>, string ) ReadXLSX( string bookFilePath, char separatorCode )
		{
			int i = bookFilePath.LastIndexOf( '/' ) + 1 ;
			if( bookFilePath.IndexOf( "~$" ) == i )
			{
				return ( null, string.Empty ) ;
			}

			//----------------------------------------------------------

			FileStream fs = null ;

			try
			{
				fs = new( bookFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;
			}
			catch( DirectoryNotFoundException )
			{
				return ( null, "File not found. : " + bookFilePath ) ;
			}
			catch( FileNotFoundException )
			{
				return ( null, "File not found. : " + bookFilePath ) ;
			}
			catch( IOException )
			{
				if( File.Exists ( bookFilePath ) == false )
				{
					return ( null, "File not found. : " + bookFilePath ) ;
				}
			}
			catch( Exception )
			{
				return ( null, "File is locking. : " + bookFilePath ) ;
			}

			//----------------------------------

			List<Sheet> sheets = null ;

			if( fs != null )
			{
				var book = new XSSFWorkbook( fs ) ;
				sheets = ReadBook( book, separatorCode ) ;
				if( sheets != null )
				{
					Save( bookFilePath, book, sheets ) ;
				}

				fs.Close() ;
			}

			return ( sheets, string.Empty ) ;
		}

		//-------------------------------------------------------------------------------------

		// 各シートを展開・保存する
		private static List<Sheet> ReadBook( IWorkbook book, char separatorCode )
		{
			// シートの数
			int numberOfSheets = book.NumberOfSheets ;
			if( numberOfSheets == 0 )
			{
				// シートが無い
				return null ;
			}

			//----------------------------------------------------------
			
			var sheets = new List<Sheet>() ;

			ISheet sheet ;
			
			string name, text ;

			int sheetIndex ;

			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					name = sheet.SheetName ;

					if( string.IsNullOrEmpty( name ) == false )
					{
						if( name[ 0 ] != '@' && name[ 0 ] != '#' )
						{
							// 設定シートではない・無効シートではない

							text = GetText( sheet, separatorCode ) ;

							sheets.Add( new Sheet(){ Name = name, Text = text } ) ;
						}
					}
				}
			}

			return sheets ;
		}

		// テキストを取得する
		private static string GetText( ISheet sheet, char separatorCode )
		{
			int lastRowNumber = sheet.LastRowNum ;
			if( lastRowNumber <  0 )
			{
				return string.Empty ;
			}

			//-----------------------------------------------------------

			IRow row ;
			int lastCellNumber ;
			ICell cell ;

			string word ;
			var rows = new List<List<string>>() ;
			List<string> columns ;

			int rowIndex, columnIndex, maxColumn = 0 ;
			bool isAdded ;

			for( rowIndex  = 0 ; rowIndex <= lastRowNumber ; rowIndex ++ )
			{
				// 行ループ
				isAdded = false ;
				row = sheet.GetRow( rowIndex ) ;
				if( row != null )
				{
					lastCellNumber = row.LastCellNum ;
					if( lastCellNumber >= 0 )
					{
						columns = new List<string>() ;

						for( columnIndex  = 0 ; columnIndex <= lastCellNumber ; columnIndex ++ )
						{
							// 列ループ
							cell = row.GetCell( columnIndex ) ;
							if( cell != null )
							{
//								Debug.Log( "Type:" + cell.CellType.ToString() + " " + cell.ToString() ) ;
								if( cell.CellType == CellType.Formula )
								{
									if( cell.CachedFormulaResultType == CellType.Boolean )
									{
										word = cell.BooleanCellValue.ToString() ;
									}
									else
									if( cell.CachedFormulaResultType == CellType.Numeric )
									{
										word = cell.NumericCellValue.ToString() ;
									}
									else
									{
										word = cell.StringCellValue ;
									}
								}
								else
								{
									word = cell.ToString() ;
								}
								if( word == null )
								{
									word = string.Empty ;
								}
								else
								{
									// Environment.NewLine は Windows 環境は 0x0D 0x0A Machintosh 環境は 0x0A だが、Machintosh 環境でも Excel は 0x0D 0x0A を出力する
									word = word.Replace( "\x0D\x0A", m_ReturnCode ) ;
								}
								columns.Add( word ) ;
							}
							else
							{
								columns.Add( string.Empty ) ;
							}
						}

						// 後ろの空白を全て削る
						while( columns.Count >  0 )
						{
							int lastIndex = columns.Count - 1 ;
							if( string.IsNullOrEmpty( columns[ lastIndex ] ) == true )
							{
								columns.RemoveAt( lastIndex ) ;
							}
							else
							{
								break ;
							}
						}

						if( columns.Count >  0 )
						{
							rows.Add( columns ) ;
							isAdded = true ;

							if( columns.Count >  maxColumn )
							{
								maxColumn  = columns.Count ;	// 最も列数が多い行に全体の列数を合わせる
							}
						}
					}
				}

				if( isAdded == false )
				{
					rows.Add( null ) ;
				}
			}

			// 末尾から空行を削る
			while( rows.Count >  0 )
			{
				int lastIndex = rows.Count - 1 ;
				if( rows[ lastIndex ] == null )
				{
					rows.RemoveAt( lastIndex ) ;
				}
				else
				{
					break ;
				}
			}

			//-----------------------------------------------------------

			var sb = new ExStringBuilder() ;

			string returnCode = m_ReturnCode.ToString() ;

			// 実際のデータ化
			for( rowIndex  = 0 ; rowIndex <  rows.Count ; rowIndex ++ )
			{
				columns = rows[ rowIndex ] ;

				if( columns != null )
				{
					for( columnIndex  = 0 ; columnIndex <  maxColumn ; columnIndex ++ )
					{
						if( columnIndex <  columns.Count )
						{
							sb += Escape( columns[ columnIndex ], separatorCode ) ;
						}
						else
						{
							sb += string.Empty ;
						}

						if( columnIndex <  ( maxColumn - 1 ) )
						{
							sb += separatorCode ;
						}
					}
				}
				else
				{
					// 一切の要素が存在しない行も出力する(Excel の Csv 保存と同じ形式に合わせる)
					for( columnIndex  = 0 ; columnIndex <  maxColumn ; columnIndex ++ )
					{
						if( columnIndex <  ( maxColumn - 1 ) )
						{
							sb += separatorCode ;
						}
					}
				}

				sb += returnCode ;	// 改行を環境ごとのものにする(C#での\nはLF=x0A)
			}

			return sb.ToString() ;
		}

		// 値を必要に応じてエスケープする
		// 区切り記号が入ると "..." 囲いが追加される
		// 改行が入ると "..." 囲いが追加される
		// " が入ると "..." 囲いが追加され " は "" にエスケープされる
		private static string Escape( string text, char sepataterCode )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return string.Empty ;
			}

			// 改行コード
			var returnCode = m_ReturnCode.ToCharArray() ;
			int ri, rl = returnCode.Length ;

			int i, l = text.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( text[ i ] == '"' )
				{
					// "
					break ;
				}
				else
				if( text[ i ] == sepataterCode )
				{
					// 区切記号判定
					break ;	// エスケープが必要
				}
				else
				if( i <= ( l - rl ) )
				{
					// 改行記号判定
					for( ri  = 0 ; ri <  rl ; ri ++ )
					{
						if( text[ i + ri ] != returnCode[ ri ] )
						{
							break ;
						}
					}

					if( ri == rl )
					{
						// 改行にヒットした
						break ;
					}
				}
			}

			if( i <  l )
			{
				// エスケープが必要

				var sb = new ExStringBuilder() ;
				sb += "\"" ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( text[ i ] == '"' )
					{
						// ２つになる
						sb += "\"\"" ;
					}
					else
					{
						sb += text[ i ] ;
					}
				}
				sb += "\"" ;

				text = sb.ToString() ;
			}

			return text ;
		}

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// StringBuilder 機能拡張ラッパークラス(メソッド拡張ではない)
		/// </summary>
		public class ExStringBuilder
		{
			private readonly StringBuilder m_StringBuilder ;

			public ExStringBuilder()
			{
				m_StringBuilder			= new StringBuilder() ;
			}

			public int Length
			{
				get
				{
					return m_StringBuilder.Length ;
				}
			}

			public int Count
			{
				get
				{
					return m_StringBuilder.Length ;
				}
			}

			public void Clear()
			{
				m_StringBuilder.Clear() ;
			}

			public override string ToString()
			{
				return m_StringBuilder.ToString() ;
			}
		
			public void Append( string s )
			{
				m_StringBuilder.Append( s ) ;
			}

			// これを使いたいがためにラッパークラス化
			public static ExStringBuilder operator + ( ExStringBuilder sb, string s )
			{
				sb.Append( s ) ;
				return sb ;
			}

			// これを使いたいがためにラッパークラス化
			public static ExStringBuilder operator + ( ExStringBuilder sb, char c )
			{
				sb.Append( c.ToString() ) ;
				return sb ;
			}
		}
	}
}
