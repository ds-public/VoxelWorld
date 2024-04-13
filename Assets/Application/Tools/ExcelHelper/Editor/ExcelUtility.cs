//#define ENV
//#define CRLF
#define LF

#pragma warning disable IDE0038

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
	/// エクセルの読み書き
	/// </summary>
	public class ExcelUtility
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

		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイル内の指定のシートをＣＳＶテキストとして読み出す
		/// </summary>
		/// <param name="tPath">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="tSeetName">シート名</param>
		/// <param name="tSplitCode">区切り記号</param>
		/// <returns>ＣＳＶテキスト</returns>
		public static string LoadText( string path, string sheetName, char separatorCode = ',' )
		{
			FileStream fs = null ;
			IWorkbook book = null ;

			if( File.Exists( path ) ==  false )
			{
				return null ;
			}

			try
			{
				fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

				if( path.EndsWith( ".xls" ) == true )
				{
					book = new HSSFWorkbook( fs ) ;
				}
				else
				if( path.EndsWith( ".xlsx" ) == true )
				{
					book = new XSSFWorkbook( fs ) ;
				}
				else
				{
					throw new Exception( "Bad File Type" ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e.Message ) ;
				book = null ;
			}
			finally
			{
				if( fs != null )
				{
					fs.Close() ;
					fs.Dispose() ;
				}
			}

			if( book == null )
			{
				return null ;
			}

			//----------------------------------------------------------

			// シートの数を取得する
			int numberOfSheets = book.NumberOfSheets ;
 
			sheetName = sheetName.ToLower() ;

			ISheet sheet = null ;
			
			int sheetIndex ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					if( sheet.SheetName.ToLower() == sheetName )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				return null ;	// 指定の名前のシートが見つからない
			}

			return GetText( sheet, separatorCode ) ;
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

		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイル内の指定のシートを行列の配列として読み出す
		/// </summary>
		/// <param name="path">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="sheetName">シート名</param>
		/// <returns>ＣＳＶテキスト</returns>
		public static System.Object[,] LoadMatrix( string path, string sheetName )
		{
			FileStream fs = null ;
			IWorkbook book = null ;

			if( File.Exists( path ) ==  false )
			{
				return null ;
			}

			try
			{
				fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

				if( path.EndsWith( ".xls" ) == true )
				{
					book = new HSSFWorkbook( fs ) ;
				}
				else
				if( path.EndsWith( ".xlsx" ) == true )
				{
					book = new XSSFWorkbook( fs ) ;
				}
				else
				{
					throw new Exception( "Bad File Type" ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e.Message ) ;
				book = null ;
			}
			finally
			{
				if( fs != null )
				{
					fs.Close() ;
					fs.Dispose() ;
				}
			}

			if( book == null )
			{
				return null ;
			}

			//----------------------------------------------------------

			// シートの数を取得する
			int numberOfSheets = book.NumberOfSheets ;
 
			sheetName = sheetName.ToLower() ;

			ISheet sheet = null ;
			
			int sheetIndex ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					if( sheet.SheetName.ToLower() == sheetName )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				return null ;	// 指定の名前のシートが見つからない
			}

			return GetMatrix( sheet ) ;
		}

		// 行列を取得する
		private static System.Object[,] GetMatrix( ISheet sheet )
		{
			int lastRowNumber = sheet.LastRowNum ;
			if( lastRowNumber <  0 )
			{
				return null ;
			}


			IRow row ;
			int lastCellNumber ;
			ICell cell ;

			int rowIndex, cellIndex ;

			// 最大の列数を取得する
			lastCellNumber = -1 ;

			for( rowIndex  = 0 ; rowIndex <= lastRowNumber ; rowIndex ++ )
			{
				row = sheet.GetRow( rowIndex ) ;
				if( row != null )
				{
					if( row.LastCellNum >  lastCellNumber )
					{
						lastCellNumber  = row.LastCellNum ;
					}
				}
			}

			if( lastCellNumber <  0 )
			{
				return null ;
			}

			System.Object[,] matrix = new System.Object[ lastRowNumber + 1, lastCellNumber + 1 ] ;

			for( rowIndex  = 0 ; rowIndex <= lastRowNumber ; rowIndex ++ )
			{
				row = sheet.GetRow( rowIndex ) ;
				if( row != null )
				{
					lastCellNumber = row.LastCellNum ;
					if( lastCellNumber >= 0 )
					{
						for( cellIndex  = 0 ; cellIndex <= lastCellNumber ; cellIndex ++ )
						{
							cell = row.GetCell( cellIndex ) ;
							if( cell != null )
							{
								if( cell.CellType == CellType.Blank )
								{
									matrix[ rowIndex, cellIndex ] = string.Empty ;
								}
								else
								if( cell.CellType == CellType.Boolean )
								{
									matrix[ rowIndex, cellIndex ] = cell.BooleanCellValue ;
								}
								else
								if( cell.CellType == CellType.Numeric )
								{
									matrix[ rowIndex, cellIndex ] = cell.NumericCellValue ;
								}
								else
								{
									matrix[ rowIndex, cellIndex ] = cell.StringCellValue ;
								}
							}
						}
					}
				}
			}

			return matrix  ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイルにテキスト形式の行列情報を書き込む
		/// </summary>
		/// <param name="path">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="sheetName">シート名</param>
		/// <param name="text">テキスト形式の行列情報</param>
		/// <param name="create">ファイルまたはシートが存在しない場合は自動的に生成するかどうか(falseの場合は該当するファイルがシートが存在しない場合はエラーとなる)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveText( string path, string sheetName, string text, bool create = true )
		{
			FileStream fs = null ;
			IWorkbook book = null ;

			bool result ;

			//----------------------------------------------------------

			// ファイルの取得・生成を行う
			if( File.Exists( path ) == true )
			{
				// ファイルが存在する

				try
				{
					// 読み出す
					fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

					if( path.EndsWith( ".xls" ) == true )
					{
						book = new HSSFWorkbook( fs ) ;
					}
					else
					if( path.EndsWith( ".xlsx" ) == true )
					{
						book = new XSSFWorkbook( fs ) ;
					}
					else
					{
						throw new Exception( "Bad File Type" ) ;
					}
				}
				catch( Exception e )
				{
					Debug.LogWarning( "Error:" + e.Message ) ;
					book = null ;
				}
				finally
				{
					if( fs != null )
					{
						fs.Close() ;
						fs.Dispose() ;
						fs = null ;
					}
				}

				if( book == null )
				{
					return false ;	// ファイルフォーマットに異常がある
				}
			}
			else
			{
				// ファイルが存在しない
				if( create == false )
				{
					// エラー
					return false ;
				}

				// 新たに生成する

				if( path.EndsWith( ".xls" ) == true )
				{
					book = new HSSFWorkbook() ;
				}
				else
				if( path.EndsWith( ".xlsx" ) == true )
				{
					book = new XSSFWorkbook() ;
				}
				else
				{
					return false ;	// フォーマットが不明
				}
			}
			
			//----------------------------------------------------------

			// シートの追加・更新を行う
			if( SetText( book, sheetName, text, create ) == false )
			{
				return false ;	// 失敗
			}

			//----------------------------------------------------------

			// シートを追加・更新したブックを保存する
			result = true ;

			try
			{
				fs = new FileStream( path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite ) ;

				book.Write( fs ) ;
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e .Message ) ;
				result = false ;
			}
			finally
			{
				if( fs != null )
				{
					fs.Close() ;
					fs.Dispose() ;
				}
			}

			AssetDatabase.Refresh() ;

			return result ;
		}

		// テキスト形式の行列情報を書き込む
		private static bool SetText( IWorkbook book, string sheetName, string text, bool create )
		{
			// シート名の小文字大文字を無視するためあえてめんどくさい方法を使う

			// シートの数を取得する
			int numberOfSheets = book.NumberOfSheets ;
 
			string sheetNameChecker = sheetName.ToLower() ;

			ISheet sheet = null ;
			
			int sheetIndex ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					if( sheet.SheetName.ToLower() == sheetNameChecker )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				// シートは見つからなかった
				if( create == false )
				{
					return false ;	// 失敗
				}

				// シートを追加する
				sheet = book.CreateSheet( sheetName ) ;
			}

			//----------------------------------------------------------

			// テキストを２次元配列に変換する
			string[][] dimension = GetDimension( text ) ;
			if( dimension == null )
			{
				return false ;	// 失敗
			}


			int x, y ;
			IRow row ;
			ICell cell ;

			for( y  = 0 ; y <  dimension.Length ; y ++ )
			{
				row = sheet.GetRow( y ) ;

				// 行を新たに生成する
				row ??= sheet.CreateRow( y ) ;

				for( x  = 0 ; x <  dimension[ y ].Length ; x ++ )
				{
					cell = row.GetCell( x ) ;

					// 列を新たに生成する
					cell ??= row.CreateCell( x ) ;

					if( string.IsNullOrEmpty( dimension[ y ][ x ] ) == false )
					{
//						cell.SetCellType( CellType.String ) ;
						cell.SetCellValue( ( string )dimension[ y ][ x ] ) ;
					}
					else
					{
//						cell.SetCellType( CellType.Blank ) ;
						cell.SetCellValue( "" ) ;
					}
				}

				// 余計な列を削る
				if( row.LastCellNum >  ( dimension[ y ].Length - 1 ) )
				{
					for( x  = dimension[ y ].Length ; x <= row.LastCellNum ; x ++ )
					{
						cell = row.GetCell( x ) ;
						if( cell != null )
						{
							row.RemoveCell( cell ) ;
						}
					}
				}
			}

			// 余計な行を削る
			if( sheet.LastRowNum >  ( dimension.Length - 1 ) )
			{
				for( y  = dimension.Length ; y <= sheet.LastRowNum ; y ++ )
				{
					row =  sheet.GetRow( y ) ;
					if( row != null )
					{
						sheet.RemoveRow( row ) ;
					}
				}
			}

			return true ;
		}


		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイルにオブジェクト配列形式の行列情報を書き込む
		/// </summary>
		/// <param name="path">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="sheetName">シート名</param>
		/// <param name="matrix">オブジェクト配列形式の行列情報</param>
		/// <param name="create">ファイルまたはシートが存在しない場合は自動的に生成するかどうか(falseの場合は該当するファイルがシートが存在しない場合はエラーとなる)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveMatrix( string path, string sheetName, System.Object[,] matrix, bool create = true )
		{
			FileStream fs = null ;
			IWorkbook book = null ;

			bool result ;

			//----------------------------------------------------------

			// ファイルの取得・生成を行う
			if( File.Exists( path ) == true )
			{
				// ファイルが存在する

				// 読み出す
				try
				{
					fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

					if( path.EndsWith( ".xls" ) == true )
					{
						book = new HSSFWorkbook( fs ) ;
					}
					else
					if( path.EndsWith( ".xlsx" ) == true )
					{
						book = new XSSFWorkbook( fs ) ;
					}
					else
					{
						throw new Exception( "Bad File Type" ) ;
					}
				}
				catch( Exception e )
				{
					Debug.LogError( "Error:" + e.Message ) ;
					book = null ;
				}
				finally
				{
					if( fs != null )
					{
						fs.Close() ;
						fs.Dispose() ;
					}
				}

				if( book == null )
				{
					return false ;	// ファイルフォーマットに異常がある
				}
			}
			else
			{
				// ファイルが存在しない
				if( create == false )
				{
					// エラー
					return false ;
				}

				// 新たに生成する

				if( path.EndsWith( ".xls" ) == true )
				{
					book = new HSSFWorkbook() ;
				}
				else
				if( path.EndsWith( ".xlsx" ) == true )
				{
					book = new XSSFWorkbook() ;
				}
				else
				{
					return false ;	// フォーマットが不明
				}
			}
			
			//----------------------------------------------------------

			// シートの追加・更新を行う
			if( SetMatrix( book, sheetName, matrix, create ) == false )
			{
				return false ;	// 失敗
			}

			//----------------------------------------------------------

			// シートを追加・更新したブックを保存する
			fs = null ;
			result = true ;

			try
			{
				fs = new FileStream( path, FileMode.OpenOrCreate, FileAccess.Write ) ;
				book.Write( fs ) ;
			}
			catch( Exception e )
			{
				Debug.LogError( "Error:" + e .Message ) ;
				result = false ;
			}
			finally
			{
				if( fs != null )
				{
					fs.Close() ;
					fs.Dispose() ;
				}
			}

			//----------------------------------------------------------

			if( result == true )
			{
				AssetDatabase.Refresh() ;
			}

			return result ;
		}

		// オブジェクト配列形式の行列情報を書き込む
		private static bool SetMatrix( IWorkbook book, string sheetName, System.Object[,] matrix, bool create )
		{
			// シート名の小文字大文字を無視するためあえてめんどくさい方法を使う

			// シートの数を取得する
			int numberOfSheets = book.NumberOfSheets ;
 
			string sheetNameChecker = sheetName.ToLower() ;

			ISheet sheet = null ;
			
			int sheetIndex ;
			for( sheetIndex = 0 ; sheetIndex <  numberOfSheets ; sheetIndex ++ )
			{
				// シート情報を取得する
				sheet = book.GetSheetAt( sheetIndex ) ;
				if( sheet != null )
				{
					if( sheet.SheetName.ToLower() == sheetNameChecker )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( sheetIndex >= numberOfSheets )
			{
				// シートは見つからなかった
				if( create == false )
				{
					return false ;	// 失敗
				}

				// シートを追加する
				sheet = book.CreateSheet( sheetName ) ;
			}

			//----------------------------------------------------------

			int x, y ;
			IRow row ;
			ICell cell ;
			object value ;

			for( y  = 0 ; y <  matrix.GetLength( 0 ) ; y ++ )
			{
				row = sheet.GetRow( y ) ;

				// 行を新たに生成する
				row ??= sheet.CreateRow( y ) ;

				for( x  = 0 ; x <  matrix.GetLength( 1 ) ; x ++ )
				{
					cell = row.GetCell( x ) ;

					// 列を新たに生成する
					cell ??= row.CreateCell( x ) ;

					value = matrix[ y, x ] ;
					if( value != null )
					{
						if( value is bool )
						{
//							cell.SetCellType( CellType.Boolean ) ;
							cell.SetCellValue( ( bool )value ) ;
						}
						else
						if( value is int )
						{
//							cell.SetCellType( CellType.Formula ) ;
							cell.SetCellValue( ( int )value ) ;
						}
						else
						if( value is uint )
						{
//							cell.SetCellType( CellType.Formula ) ;
							cell.SetCellValue( ( uint )value ) ;
						}
						else
						if( value is long )
						{
//							cell.SetCellType( CellType.Numeric ) ;
							cell.SetCellValue( ( long )value ) ;
						}
						else
						if( value is ulong )
						{
//							cell.SetCellType( CellType.Numeric ) ;
							cell.SetCellValue( ( ulong )value ) ;
						}
						else
						if( value is float )
						{
//							cell.SetCellType( CellType.Numeric ) ;
							cell.SetCellValue( ( float )value ) ;
						}
						else
						if( value is double )
						{
//							cell.SetCellType( CellType.Numeric ) ;
							cell.SetCellValue( ( double )value ) ;
						}
						else
						{
//							cell.SetCellType( CellType.String ) ;
							cell.SetCellValue( ( string )value ) ;
						}
					}
					else
					{
//						cell.SetCellType( CellType.Blank ) ;
						cell.SetCellValue( "" ) ;
					}
				}

				// 余計な列を削る
				if( row.LastCellNum >  ( matrix.GetLength( 1 ) - 1 ) )
				{
					for( x  = matrix.GetLength( 1 ) ; x <= row.LastCellNum ; x ++ )
					{
						cell = row.GetCell( x ) ;
						if( cell != null )
						{
							row.RemoveCell( cell ) ;
						}
					}
				}
			}

			// 余計な行を削る
			if( sheet.LastRowNum >  ( matrix.GetLength( 0 ) - 1 ) )
			{
				for( y  = matrix.GetLength( 0 ) ; y <= sheet.LastRowNum ; y ++ )
				{
					row =  sheet.GetRow( y ) ;
					if( row != null )
					{
						sheet.RemoveRow( row ) ;
					}
				}
			}

			return true ;
		}
		
		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストの各要素ををパースして格納する
		/// </summary>
		/// <param name="text">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <returns>パース結果(true=成功・false=失敗)</returns>
		private static string[][] GetDimension( string text )
		{
			if( string.IsNullOrEmpty( text ) == true )
			{
				return null ;
			}
		
			// まずコメント部分を削る

			int o, p, L, i, j, k, m, r ;
			char c ;
		
			//-------------------------------------------------------------
		
			// ダブルクォート内にある改行・タブを置き換える
		
			L = text.Length ;
		
			char[] ca = new char[ L ] ;
		
			// 改行を \n に統一する（" 内外関係無し）
			p = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( c == 13 )
				{
					// CR
					if( ( i + 1 ) <  L )
					{
						if( text[ i + 1 ] == 10 )
						{
							// 改行
							ca[ p ] = '\n' ;
							p ++ ;
						
							i ++ ;	// １文字スキップ
						}
					}
				}
				else
				{
					ca[ p ] = c ;
					p ++ ;
				}
			}
		
			text = new string( ca, 0, p ) ;
		
			L = text.Length ;
		
			// この段階で CR は存在しない
		
			// ダブルクォートの外のカンマまたはタブの数をカウントする
			int lc = 0 ;
		
			int f = 0 ;	// ダブルクォートの内側か外側か（デフォルトは外側）
			bool d = false ;
			bool escape = false ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						lc ++ ;	// 行増加
						r = 0 ;
					}
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						f = 1 ;	// 状態を反転させる
						d = true ;	// ダブルクォート系
						escape = false ;
					}
					else
					{
						// そのまま文字列
						f = 1 ;
						d = false ;	// 非ダブルクォート系
						escape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '"' )
								{
									// 終了
									f = 2 ;
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '\t' || c == ',' || c == '\n' )
								{
									// 終了
									if( c == '\t' || c == ',' )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || c == ',' || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
			if( r == 1 )
			{
				// 中途半端に終わった
				lc ++ ;
			}
		
			//--------------------------
		
			// 配列を確保する
			string[][] value = new string[ lc ][] ;

			//-----------------------------------------------------
		
			// 実際に値を取得する
			var wordList = new List<string>() ;
		
			lc = 0 ;
		
			c = ' ' ;
			o = 0 ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = text[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					wordList.Clear() ;
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						value[ lc ] = wordList.ToArray() ;
					
						lc ++ ;	// 行増加
						r = 0 ;
					}	
					else
					if( c == '\t' || c == ',' )
					{
						// 区切り
						wordList.Add( "" ) ;
					}
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						o = i + 1 ;
						f = 1 ;	// 状態を反転させる
						d = true ;	// ダブルクォート系
						escape = false ;
					}
					else
					{
						// そのまま文字列
						o = i ;
						f = 1 ;
						d = false ;	// 非ダブルクォート系
						escape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '"' )
								{
									// 終了
									if( i >  o )
									{
										wordList.Add( text[ o..i ] ) ;
									}
									else
									{
										wordList.Add( "" ) ;
									}
									f = 2 ;
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							escape = true ;
						}
						else
						{
							if( escape == false )
							{
								if( c == '\t' || c == ',' || c == '\n' )
								{
									// 終了
									if( i >  o )
									{
										// 末尾のスペースは削除する
										for( k  = i - 1 ; k >= o ; k -- )
										{
											if( text[ k ] != ' ' )
											{
												break ;
											}
										}
										if( k >= o )
										{
											wordList.Add( text.Substring( o, k - o + 1 ) ) ;
										}
										else
										{
											wordList.Add( "" ) ;
										}
									}
									else
									{
										if( c != '\n' )
										{
											wordList.Add( "" ) ;
										}
									}
								
									if( c == '\t' || c == ',' )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										value[ lc ] = wordList.ToArray() ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								escape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || c == ',' || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							value[ lc ] = wordList.ToArray() ;
						
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
		
			if( r == 1 )
			{
				// 中途半端に終わった
				if( d == false )
				{
					if( i >  o )
					{
						// 末尾のスペースは削除する
						for( k  = i - 1 ; k >= o ; k -- )
						{
							if( text[ k ] != ' ' )
							{
								break ;
							}
						}
						if( k >= o )
						{
							wordList.Add( text.Substring( o, k - o + 1 ) ) ;
						}
						else
						{
							wordList.Add( "" ) ;
						}
					}
					else
					{
						if( c != '\n' )
						{
							wordList.Add( "" ) ;
						}
					}
				}
			
				value[ lc ] = wordList.ToArray() ;
			
				lc ++ ;
			}

			//-------------------------------------------------------------

			// 最後の部分の完全に値が無い部分を切り捨てる

			p = lc ;
			for( i  = ( lc - 1 ) ; i >= 0 ; i -- )
			{
				m = value[ i ].Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( string.IsNullOrEmpty( value[ i ][ j ] ) == false )
					{
						break ;
					}
				}
				
				if( j <  m )
				{
					// 現在の行まで残す
					p = i + 1 ;
					break ;
				}
			}
			
			
			if( p <  lc )
			{
				// 切り詰める
				string[][] reduceValue = new string[ p ][] ;
				
				for( i  = 0 ; i <  p ; i ++ )
				{
					m = value[ i ].Length ;
					reduceValue[ i ] = new string[ m ] ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						reduceValue[ i ][ j ] = value[ i ][ j ] ;
					}
				}
				
				value = reduceValue ;
			}
		
			//-------------------------------------------------------------
		
			return value ;
		}
	}
}
