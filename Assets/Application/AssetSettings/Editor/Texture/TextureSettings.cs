using System ;
using System.Linq ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Reflection ;

using UnityEngine ;
using UnityEngine.U2D ;

using UnityEditor ;
using UnityEditor.U2D ;


namespace AssetSettings
{
	/// <summary>
	/// Texture の設定 Version 2024/08/07
	/// </summary>
	public class TextureSettings : ImportProcessor
	{
		// フォルダ無指定時の対象フォルダ
		private static readonly string[] m_Paths =
		{
			@"Assets/Application/AssetBundle/Textures/*",
			@"Assets/Application/ReferencedAssets/Textures/*",
		} ;

		// Textureインポート用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_TextureDispatcher
			= new
			(
				m_Paths,
				ReplaceTextureSettings,
				null
			) ;

		// SpriteAtlasインポート用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_SpriteAtlasDispatcher
			= new
			(
				m_Paths,
				ReplaceSpriteAtlasSettings,
				null
			) ;

		//---------------

		// バッチ処理用のディスパッチャー
		private static readonly ImportDispatcher<AssetImporter> m_BatchDispatcher
			= new
			(
				m_Paths,
				null,
				ReplaceBatchSettings
			) ;

		// ※テクスチャとスプライトアトラスは、処理は別に分ける。(スプライトアトラスは、明示的なインポートが不要であるため。)

		//-------------------------------------------------------------------------------------------

		// メニューから全て設定し直す
		[ MenuItem( "AssetSettings/Texture - Reimport" ) ]
		internal static void ReimportAllAssets()
		{
			var targetPaths = new List<string>() ;

			if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				// １つだけ選択（複数選択には対応していない：フォルダかファイル）
				targetPaths.Add( AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ).Replace( '\\', '/' ) ) ;
			}
			else
			if( Selection.objects != null && Selection.objects.Length >  1 )
			{
				// 複数対象
				foreach( var activeObject in Selection.objects )
				{
					targetPaths.Add( AssetDatabase.GetAssetPath( activeObject.GetInstanceID() ).Replace( '\\', '/' ) ) ;
				}
			}

			m_BatchDispatcher.SetupAll( targetPaths ) ;
		}

		//-------------------------------------------------------------------------------------------
		// コールバックメソッド群(オーバーライド)

		/// <summary>
		/// Texture がインポートされる前に呼び出されます(このメソッドを継承して処理を追加するのが最も負荷が高い)
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public override void OnPreprocessTexture( AssetPostprocessor assetPostprocessor )
		{
			AssetImporter assetImporter = assetPostprocessor.assetImporter ;
			if( assetImporter.importSettingsMissing == false )
			{
				// 既にインポート済み(metaファイルが作られている)は無視する
//				Debug.Log( "既にインポートされている:" + assetImporter.assetPath ) ;
				return ;
			}

//			Debug.Log( "<color=#00FFFF>[インポートされていないので処理する]:" + assetImporter.assetPath + "</color>" ) ;

			m_TextureDispatcher.SetupAny( assetImporter ) ;
		}

		// 注意:
		// SpriteAtlas を新規作成しても OnPreprocessAsset は呼ばれない

		/// <summary>
		/// Asset が追加された後に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public override bool OnAssetImported( string assetPath )
		{
			return m_SpriteAtlasDispatcher.SetupAny( assetPath ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Texture の設定を行う(インポート用)
		/// </summary>
		private static bool ReplaceTextureSettings( AssetImporter assetImporter )
		{
			if( assetImporter is TextureImporter )
			{
				// テクスチャの設定を行う
				ProcessTextureSettings( assetImporter as TextureImporter ) ;
			}

			// Save は必要ない
			return false ;
		}

		/// <summary>
		/// SpriteAtlas の設定を行う(インポート用)
		/// </summary>
		/// <param name="assetImporter"></param>
		/// <returns></returns>
		private static bool ReplaceSpriteAtlasSettings( AssetImporter assetImporter )
		{
			string assetPath = assetImporter.assetPath.Replace( '\\', '/' ) ;

			int version ;
			string extension = Path.GetExtension( assetPath ) ;
			if( extension == ".spriteatlas" )
			{
				version = 1 ;
			}
			else
			if( extension == ".spriteatlasv2" )
			{
				version = 2 ;
			}
			else
			{
				return false ;	// スプライトアトラス以外は無視する
			}

			SpriteAtlas spriteAltas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>( assetPath ) ;
			if( spriteAltas == null )
			{
//				Debug.Log( "アセットのロードができない" ) ;
				return false ;	// スプライトアトラスではない
			}

			if( ProcessSpriteAtlasSettings( spriteAltas, version ) == true )
			{
//				Debug.Log( "------>スプライトアトラスを処理した" ) ;
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// Texture の設定を行う(バッチ専用)
		/// </summary>
		private static bool ReplaceBatchSettings( AssetImporter assetImporter )
		{
			if( assetImporter is TextureImporter )
			{
				// テクスチャの設定を行う
				if( ProcessTextureSettings( assetImporter as TextureImporter ) == true )
				{
					Debug.Log( "<color=#FFFF00>[Texture Importing] " + assetImporter.assetPath + "</color>" ) ;
					AssetDatabase.ImportAsset( assetImporter.assetPath ) ;

					return true ;
				}

				return false ;	// Save は不要
			}
			else
			{
				// その他(スプライトアトラス)の設定を行う
				return ReplaceSpriteAtlasSettings( assetImporter ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// テクスチャサイズ最大(リミッター)
		/// </summary>
		private const int m_MaxTextureSize = 2048 ;


		//-------------------------------------------------------------------------------------------
		// 共通の設定項目

		//------------------------------------------------------------
		// Texture(Sprite) 関連

		private const int							m_Texture_SpritePixelsPerUnit			= 100 ;
		private const bool							m_Texture_IsReadable					= false ;
		private const bool							m_Texture_MipmapEnabled					= false ;
		private const FilterMode					m_Texture_FilterMode					= FilterMode.Bilinear ;
		private const TextureImporterCompression	m_Texture_TextureCompression			= TextureImporterCompression.Compressed ;
		private const bool							m_Texture_CrunchedCompression			= true ;

		// Standalone
		private const bool							m_Texture_Overridden_Standalone			= true ;
		private const TextureImporterFormat			m_Texture_TextureFormat_Standalone		= TextureImporterFormat.DXT5Crunched ;

		// Android
		private const bool							m_Texture_Overridden_Android			= true ;
		private const TextureImporterFormat			m_Texture_TextureFormat_Android_1		= TextureImporterFormat.ASTC_6x6 ;
		private const TextureImporterFormat			m_Texture_TextureFormat_Android_2		= TextureImporterFormat.ETC2_RGBA8Crunched ;

		// iPhone
		private const bool							m_Texture_Overridden_iPhone				= true ;
		private const TextureImporterFormat			m_Texture_TextureFormat_iPhone			= TextureImporterFormat.ASTC_6x6 ;

		//------------------------------------------------------------
		// SpriteAtlas 関連

		private const bool							m_SpriteAtlas_EnableRotation			= false ;
		private const bool							m_SpriteAtlas_EnableTightPacking		= false ;
		private const int							m_SpriteAtlas_Padding					= 2 ;
		private const bool							m_SpriteAtlas_GenerateMipMaps			= false ;
		private const FilterMode					m_SpriteAtlas_FilterMode				= FilterMode.Point ;
		private const TextureImporterCompression	m_SpriteAtlas_TextureCompression		= TextureImporterCompression.Uncompressed ;
		private const int							m_SpriteAtlas_CompressionQuality		= 0 ;
		private const bool							m_SpriteAtlas_CrunchedCompression		= false ;

		// Default
		private const bool							m_SpriteAtlas_Overridden_Default		= true ;
		private const TextureImporterFormat			m_SpriteAtlas_TextureFormat_Default		= TextureImporterFormat.Automatic ;

		// Standalone
		private const bool							m_SpriteAtlas_Overridden_Standalone		= true ;
		private const TextureImporterFormat			m_SpriteAtlas_TextureFormat_Standalone	= TextureImporterFormat.DXT5Crunched ;

		// Android
		private const bool							m_SpriteAtlas_Overridden_Android		= true ;
		private const TextureImporterFormat			m_SpriteAtlas_TextureFormat_Android		= TextureImporterFormat.ETC2_RGBA8Crunched ;

		// iPhone
		private const bool							m_SpriteAtlas_Overridden_iPhone			= true ;
		private const TextureImporterFormat			m_SpriteAtlas_TextureFormat_iPhone		= TextureImporterFormat.ASTC_6x6 ;

		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// Textures の設定を行う
		/// </summary>
		private static bool ProcessTextureSettings( TextureImporter textureImporter )
		{
			// 再設定を行ったかどうか
			bool isDirty = false ;

			//------------------------------------------------------------------------------------------
			// 再設定必要かどうかを確認しつつ必要であれば再設定を行う

            bool isNotSprite = false ;

			// タイプ
			if( textureImporter.textureType != TextureImporterType.Sprite )
			{
				textureImporter.textureType  = TextureImporterType.Sprite ;
				isDirty = true ;

                isNotSprite = true ;
			}

            // モード
            // TextureType が Texture を SPrite に変えた際に、
            // SpriteImportModer はデフォルトで Multiple になってしまってウザいので、
            // その場合は Single にする。
			if( isNotSprite == true )
			{
				textureImporter.spriteImportMode  = SpriteImportMode.Single ;
				isDirty = true ;
			}

			// ピクセパーユニット
			if( textureImporter.spritePixelsPerUnit != m_Texture_SpritePixelsPerUnit )
			{
				textureImporter.spritePixelsPerUnit  = m_Texture_SpritePixelsPerUnit ;
				isDirty = true ;
			}

			// 読み書き許可
			if( textureImporter.isReadable != m_Texture_IsReadable )
			{
				textureImporter.isReadable  = m_Texture_IsReadable ;
				isDirty = true ;
			}

			// ミップマップ
			if( textureImporter.mipmapEnabled != m_Texture_MipmapEnabled )
			{
				textureImporter.mipmapEnabled  = m_Texture_MipmapEnabled ;
				isDirty = true ;
			}

			// フィルタ
			if( textureImporter.filterMode != m_Texture_FilterMode )
			{
				textureImporter.filterMode  = m_Texture_FilterMode ;
				isDirty = true ;
			}

			//---------------------------------
			// 各プラットフォームごとの共通設定

			// 最大テクスチャサイズ
			int maxTextureSize = textureImporter.maxTextureSize ;
			if( maxTextureSize >  m_MaxTextureSize )
			{
				maxTextureSize  = m_MaxTextureSize ;
			}
			if( textureImporter.maxTextureSize != maxTextureSize )
			{
				textureImporter.maxTextureSize  = maxTextureSize ;
				isDirty = true ;
			}

			//==========================================================
			// パスを見て設定の種類を変える

			int settingType = 0 ;

			string path = textureImporter.assetPath.Replace( "\\", "/" ) ;
			if( path.IndexOf( "/@" ) >= 0 || path.IndexOf( "/#@" ) >= 0 )
			{
				// 無圧縮設定にする
				// ※ /# の場合はここには来ない

				settingType = 1 ;
			}

			//==========================================================

			// 圧縮
			if( settingType == 0 )
			{
				// 通常設定

				// 圧縮
				if( textureImporter.textureCompression != m_Texture_TextureCompression )
				{
					textureImporter.textureCompression  = m_Texture_TextureCompression ;
					isDirty = true ;
				}

				// さらに圧縮
				if( textureImporter.crunchedCompression != m_Texture_CrunchedCompression )
				{
					textureImporter.crunchedCompression  = m_Texture_CrunchedCompression ;
					isDirty = true ;
				}
			}
			else
			{
				// 特殊設定(強制非圧縮)

				// 圧縮
				if( textureImporter.textureCompression != TextureImporterCompression.Uncompressed )
				{
					textureImporter.textureCompression  = TextureImporterCompression.Uncompressed ;
					isDirty = true ;
				}

				// さらに圧縮
				if( textureImporter.crunchedCompression != false )
				{
					textureImporter.crunchedCompression  = false ;
					isDirty = true ;
				}
			}

			//----------------------------------
			// 各プラットフォームごとの個別設定

			// 注意：
			// overridden は、全てのプラットフォームで true にすべし。
			// false になっているものが 1 つでもあると、
			// Inspector のプラッフォームタブでプラットフォームを切り替えた際に、
			// Dirty 状態([Apply]ボタンと[Revert]ボタンが有効化)されてしまう。
			// また、
			// 対象フォルダ内の全対象ファイルをインポート(再設定)し直す際は、
			// Inspector に対象のファイルを表示した状態にしてはならない。
			// 表示されていたファイルが Dirty 状態になってしまう。

			TextureImporterPlatformSettings platformSettings ;
			string platfornName ;

			// Standalone
			platfornName = "Standalone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName, m_Texture_Overridden_Standalone, textureImporter.maxTextureSize, m_Texture_TextureFormat_Standalone, settingType ) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// Android

			// テクスチャのサイズにより圧縮フォーマットを切り替える
			TextureImporterFormat androidTextureFormat =  m_Texture_TextureFormat_Android_1 ;

//			Texture2D texture = AssetDatabase.LoadAssetAtPath( textureImporter.assetPath, typeof( Texture2D ) ) as Texture2D ;

			// 外法(リフレクションを使いインポーターからテクスチャのサイズを取得する)
			object[] size = new object[ 2 ]{ 0, 0 } ;
			var method = typeof( TextureImporter ).GetMethod( "GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance ) ;
			method.Invoke( textureImporter, size ) ;
			if( ( ( int )size[ 0 ] & 3 ) == 0 && ( ( int )size[ 1 ] & 3 ) == 0 )
			{
				// サイズは４の倍数なのでクランチＥＴＣ２が使用できる
				androidTextureFormat =  m_Texture_TextureFormat_Android_2 ;
			}

			// Android
			platfornName = "Android" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName,  m_Texture_Overridden_Android, textureImporter.maxTextureSize, androidTextureFormat, settingType ) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// iOS
			platfornName = "iPhone" ;
			platformSettings = textureImporter.GetPlatformTextureSettings( platfornName ) ;
			if( SetPlatformSettings( platformSettings, platfornName,  m_Texture_Overridden_iPhone, textureImporter.maxTextureSize, m_Texture_TextureFormat_iPhone, settingType ) == true )
			{
				textureImporter.SetPlatformTextureSettings( platformSettings ) ;
				isDirty = true ;
			}

			// 再設定が行われたかどうかを返す
			return isDirty ;
		}

		// プラットフォームごとの確認と設定
		private static bool SetPlatformSettings
		(
			TextureImporterPlatformSettings platformSettings,
			string platfornName,
			bool overridden,
			int maxTextureSize,
			TextureImporterFormat textureFormat,
			int settingType
		)
		{
			bool isUpdate = false ;

			if( platformSettings != null )
			{
				// 引数で受け取るテクスチャサイズ設定が、全体で守らなくてはならないサイズを上回らないこと
				// maxTextureSize = platformSettings.maxTextureSize ;
				if( maxTextureSize >  m_MaxTextureSize )
				{
					maxTextureSize  = m_MaxTextureSize ;
				}

				if( settingType == 0 )
				{
					// 通常設定
					if
					(
						platformSettings.overridden			!= overridden								||
						platformSettings.maxTextureSize		!= maxTextureSize							||
						platformSettings.format				!= textureFormat							||
						platformSettings.textureCompression	!= m_Texture_TextureCompression				||
						platformSettings.resizeAlgorithm	!= TextureResizeAlgorithm.Mitchell
					)
					{
						isUpdate = true ;
					}
				}
				else
				{
					// 特殊設定(強制無圧縮)
					if
					(
						platformSettings.overridden			!= overridden								||
						platformSettings.maxTextureSize		!= maxTextureSize							||
						platformSettings.format				!= textureFormat							||
						platformSettings.textureCompression	!= TextureImporterCompression.Uncompressed	||
						platformSettings.resizeAlgorithm	!= TextureResizeAlgorithm.Mitchell
					)
					{
						isUpdate = true ;
					}
				}
			}
			else
			{
				isUpdate = true ;
			}

			if( isUpdate == true )
			{
				if( settingType == 0 )
				{
					// 通常設定
					platformSettings.name				= platfornName									;
					platformSettings.overridden			= overridden									;
					platformSettings.maxTextureSize		= maxTextureSize								;
					platformSettings.format				= textureFormat									;
					platformSettings.textureCompression	= m_Texture_TextureCompression					;
					platformSettings.resizeAlgorithm	= TextureResizeAlgorithm.Mitchell				;
				}
				else
				{
					// 特殊設定(強制無圧縮)
					platformSettings.name				= platfornName									;
					platformSettings.overridden			= overridden									;
					platformSettings.maxTextureSize		= maxTextureSize								;
					platformSettings.format				= textureFormat									;
					platformSettings.textureCompression	= TextureImporterCompression.Uncompressed		;
					platformSettings.resizeAlgorithm	= TextureResizeAlgorithm.Mitchell				;
				}
			}

			return isUpdate ;
		}


		// https://tetsujp84.hatenablog.com/entry/2019/05/05/050828

		/// <summary>
		/// SpriteAtlas の設定を行う(バージョン１とバージョン２をそれぞれ行う必要がある)
		/// </summary>
		private static bool ProcessSpriteAtlasSettings( SpriteAtlas spriteAtlas, int version )
		{
			var serializedObject = new SerializedObject( spriteAtlas ) ;
			var editorData = serializedObject.FindProperty( "m_EditorData" ) ;

			string path = AssetDatabase.GetAssetPath( spriteAtlas ) ;

			bool isDirty = false ;

			//----------------------------------

			// packing設定も適用
			var packingSetting = editorData.FindPropertyRelative( "packingSettings" ) ;

			if( CheckPropertyOfBool( packingSetting, "enableRotation", m_SpriteAtlas_EnableRotation ) == false )
			{
				StorePropertyOfBool( packingSetting, "enableRotation", m_SpriteAtlas_EnableRotation ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfBool( packingSetting, "enableTightPacking", m_SpriteAtlas_EnableTightPacking ) == false )
			{
				StorePropertyOfBool( packingSetting, "enableTightPacking", m_SpriteAtlas_EnableTightPacking ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfInt( packingSetting, "padding", m_SpriteAtlas_Padding ) == false )
			{
				StorePropertyOfInt( packingSetting, "padding", m_SpriteAtlas_Padding ) ;
				isDirty = true ;
			}

			//--------------

			if( version >= 2 )
			{
				// バージョン２対応

				var spriteAtlasImporter = ( SpriteAtlasImporter )AssetImporter.GetAtPath( path ) ;

				var packingSettingsV2 = spriteAtlasImporter.packingSettings ;

				bool isChanged = false ;

				if( packingSettingsV2.enableRotation != m_SpriteAtlas_EnableRotation )
				{
					packingSettingsV2.enableRotation  = m_SpriteAtlas_EnableRotation ;
					isChanged = true ;
				}

				if( packingSettingsV2.enableTightPacking != m_SpriteAtlas_EnableTightPacking )
				{
					packingSettingsV2.enableTightPacking  = m_SpriteAtlas_EnableTightPacking ;
					isChanged = true ;
				}

				if( packingSettingsV2.padding != m_SpriteAtlas_Padding )
				{
					packingSettingsV2.padding  = m_SpriteAtlas_Padding ;
					isChanged = true ;
				}

				if( isChanged == true )
				{
					spriteAtlasImporter.packingSettings = packingSettingsV2 ;
					isDirty = true ;
				}
			}

			//----------------------------------
			// 基本設定
			var textureSettings = editorData.FindPropertyRelative( "textureSettings" ) ;

			if( CheckPropertyOfInt( textureSettings, "maxTextureSize", m_MaxTextureSize ) == false )
			{
				StorePropertyOfInt( textureSettings, "maxTextureSize", m_MaxTextureSize ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfBool( textureSettings, "generateMipMaps", m_SpriteAtlas_GenerateMipMaps ) == false )
			{
				StorePropertyOfBool( textureSettings, "generateMipMaps", m_SpriteAtlas_GenerateMipMaps ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfInt( textureSettings, "filterMode", ( int )m_SpriteAtlas_FilterMode ) == false )
			{
				StorePropertyOfInt( textureSettings, "filterMode", ( int )m_SpriteAtlas_FilterMode ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfBool( textureSettings, "textureCompression", ( m_SpriteAtlas_TextureCompression != TextureImporterCompression.Uncompressed ) ) == false )
			{
				StorePropertyOfBool( textureSettings, "textureCompression", ( m_SpriteAtlas_TextureCompression != TextureImporterCompression.Uncompressed ) ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfInt( textureSettings, "compressionQuality", m_SpriteAtlas_CompressionQuality ) == false )
			{
				StorePropertyOfInt( textureSettings, "compressionQuality", m_SpriteAtlas_CompressionQuality ) ;
				isDirty = true ;
			}

			if( CheckPropertyOfBool( textureSettings, "crunchedCompression", m_SpriteAtlas_CrunchedCompression ) == false )
			{
				StorePropertyOfBool( textureSettings, "crunchedCompression", m_SpriteAtlas_CrunchedCompression ) ;
				isDirty = true ;
			}

			//--------------

			if( version >= 2 )
			{
				// バージョン２対応

				var spriteAtlasImporter = ( SpriteAtlasImporter )AssetImporter.GetAtPath( path ) ;

				var textureSettingsV2 = spriteAtlasImporter.textureSettings ;

				bool isChanged = false ;

				if( textureSettingsV2.generateMipMaps != m_SpriteAtlas_GenerateMipMaps )
				{
					textureSettingsV2.generateMipMaps  = m_SpriteAtlas_GenerateMipMaps ;
					isChanged = true ;
				}

				if( textureSettingsV2.filterMode != m_SpriteAtlas_FilterMode )
				{
					textureSettingsV2.filterMode  = m_SpriteAtlas_FilterMode ;
					isChanged = true ;
				}

				if( isChanged == true )
				{
					spriteAtlasImporter.textureSettings = textureSettingsV2 ;
					isDirty = true ;
				}
			}

			//----------------------------------------------------------
			// platform 設定

			var platformSettings = editorData.FindPropertyRelative( "platformSettings" ) ;

			SerializedProperty settings ;

			if( platformSettings.arraySize == 4 )
			{
				// プラットフォームの数は４つ

				// ※：デフォルトのプラットォーム識別名は Default ではなく DefaultTexturePlatform である事に注意する
				settings = platformSettings.GetArrayElementAtIndex( 0 ) ;
				if( CheckPlatformSettingsV1( settings, "DefaultTexturePlatform", m_SpriteAtlas_Overridden_Default,		( int )m_SpriteAtlas_TextureFormat_Default		) == false )
				{
					StorePlatformSettingsV1( settings, "DefaultTexturePlatform", m_SpriteAtlas_Overridden_Default,		( int )m_SpriteAtlas_TextureFormat_Default		) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 1 ) ;
				if( CheckPlatformSettingsV1( settings, "Standalone",			m_SpriteAtlas_Overridden_Standalone,	( int )m_SpriteAtlas_TextureFormat_Standalone	) == false )
				{
					StorePlatformSettingsV1( settings, "Standalone",			m_SpriteAtlas_Overridden_Standalone,	( int )m_SpriteAtlas_TextureFormat_Standalone	) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 2 ) ;
				if( CheckPlatformSettingsV1( settings, "Android",				m_SpriteAtlas_Overridden_Android,		( int )m_SpriteAtlas_TextureFormat_Android		) == false )
				{
					StorePlatformSettingsV1( settings, "Android",				m_SpriteAtlas_Overridden_Android,		( int )m_SpriteAtlas_TextureFormat_Android		) ;
					isDirty = true ;
				}

				settings = platformSettings.GetArrayElementAtIndex( 3 ) ;
				if( CheckPlatformSettingsV1( settings, "iPhone",				m_SpriteAtlas_Overridden_iPhone,		( int )m_SpriteAtlas_TextureFormat_iPhone		) == false )
				{
					StorePlatformSettingsV1( settings, "iPhone",				m_SpriteAtlas_Overridden_iPhone,		( int )m_SpriteAtlas_TextureFormat_iPhone		) ;
					isDirty = true ;
				}
			}
			else
			{
				// プラットフォームの数は４つ以外(４つにする)

				platformSettings.arraySize = 4 ;

				// ※：デフォルトのプラットォーム識別名は Default ではなく DefaultTexturePlatform である事に注意する
				settings = platformSettings.GetArrayElementAtIndex( 0 ) ;
				StorePlatformSettingsV1( settings, "DefaultTexturePlatform",	m_SpriteAtlas_Overridden_Default,		( int )m_SpriteAtlas_TextureFormat_Default		) ;

				settings = platformSettings.GetArrayElementAtIndex( 1 ) ;
				StorePlatformSettingsV1( settings, "Standalone",				m_SpriteAtlas_Overridden_Standalone,	( int )m_SpriteAtlas_TextureFormat_Standalone	) ;

				settings = platformSettings.GetArrayElementAtIndex( 2 ) ;
				StorePlatformSettingsV1( settings, "Android",					m_SpriteAtlas_Overridden_Android,		( int )m_SpriteAtlas_TextureFormat_Android		) ;

				settings = platformSettings.GetArrayElementAtIndex( 3 ) ;
				StorePlatformSettingsV1( settings, "iPhone",					m_SpriteAtlas_Overridden_iPhone,		( int )m_SpriteAtlas_TextureFormat_iPhone		) ;

				isDirty = true ;
			}

			if( version >= 2 )
			{
				// バージョン２対応

				var spriteAtlasImporter = ( SpriteAtlasImporter )AssetImporter.GetAtPath( path ) ;

				TextureImporterPlatformSettings platformSettingsV2 ;

				// ※：デフォルトのプラットォーム識別名は Default ではなく DefaultTexturePlatform である事に注意する
				platformSettingsV2 = spriteAtlasImporter.GetPlatformSettings( "DefaultTexturePlatform" ) ;
				if( platformSettingsV2 != null )
				{
					if( CheckPlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Default,		m_SpriteAtlas_TextureFormat_Default ) == false )
					{
						StorePlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Default,		m_SpriteAtlas_TextureFormat_Default ) ;
						spriteAtlasImporter.SetPlatformSettings( platformSettingsV2 ) ;
					}
				}

				platformSettingsV2 = spriteAtlasImporter.GetPlatformSettings( "Standalone" ) ;
				if( platformSettingsV2 != null )
				{
					if( CheckPlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Standalone,	m_SpriteAtlas_TextureFormat_Standalone ) == false )
					{
						StorePlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Standalone,	m_SpriteAtlas_TextureFormat_Standalone ) ;
						spriteAtlasImporter.SetPlatformSettings( platformSettingsV2 ) ;
					}
				}

				platformSettingsV2 = spriteAtlasImporter.GetPlatformSettings( "Android" ) ;
				if( platformSettingsV2 != null )
				{
					if( CheckPlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Android,		m_SpriteAtlas_TextureFormat_Android ) == false )
					{
						StorePlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Android,		m_SpriteAtlas_TextureFormat_Android ) ;
						spriteAtlasImporter.SetPlatformSettings( platformSettingsV2 ) ;
					}
				}

				platformSettingsV2 = spriteAtlasImporter.GetPlatformSettings( "iPhone" ) ;
				if( platformSettingsV2 != null )
				{
					if( CheckPlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Android,		m_SpriteAtlas_TextureFormat_iPhone ) == false )
					{
						StorePlatformSettingsV2( platformSettingsV2, m_SpriteAtlas_Overridden_Android,		m_SpriteAtlas_TextureFormat_iPhone ) ;
						spriteAtlasImporter.SetPlatformSettings( platformSettingsV2 ) ;
					}
				}
			}

			//------------------------------------------------------------------------------------------

			if( isDirty == true )
			{
				// 上記の変更を適用
				serializedObject.ApplyModifiedProperties() ;

				if( version >= 2 )
				{
					// バージョン２対応

					AssetDatabase.WriteImportSettingsIfDirty( path ) ;
				}
			}

			//------------------------------------------------------------------------------------------
			// Version 1 用

			// プロパティの値が合っているかチェックする
			bool CheckPropertyOfBool( SerializedProperty property, string key, bool value )
			{
				return ( property.FindPropertyRelative( key ).boolValue	== value ) ;
			}

			// プロパティの値を設定する
			void StorePropertyOfBool( SerializedProperty property, string key, bool value )
			{
				property.FindPropertyRelative( key ).boolValue	= value ;
			}

			// プロパティの値が合っているかチェックする
			bool CheckPropertyOfInt( SerializedProperty property, string key, int value )
			{
				return ( property.FindPropertyRelative( key ).intValue	== value ) ;
			}

			// プロパティの値を設定する
			void StorePropertyOfInt( SerializedProperty property, string key, int value )
			{
				property.FindPropertyRelative( key ).intValue	= value ;
			}

			//----------------------------------------------------------

			// プラットフォームの設定を確認する
			bool CheckPlatformSettingsV1( SerializedProperty property, string platform, bool overridden, int textureFormat )
			{
				if( property.FindPropertyRelative( "m_BuildTarget" ).stringValue != platform )
				{
					return false ;
				}

				//-------------

				if( property.FindPropertyRelative( "m_Overridden" ).boolValue != overridden )
				{
					return false ;
				}

				//-------------

				if( property.FindPropertyRelative( "m_MaxTextureSize" ).intValue != m_MaxTextureSize )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_TextureFormat" ).intValue	!= textureFormat )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_TextureCompression" ).boolValue != ( m_SpriteAtlas_TextureCompression != TextureImporterCompression.Uncompressed ) )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_CompressionQuality" ).intValue != m_SpriteAtlas_CompressionQuality )
				{
					return false ;
				}

				if( property.FindPropertyRelative( "m_CrunchedCompression" ).boolValue != m_SpriteAtlas_CrunchedCompression )
				{
					return false ;
				}

				return true ;
			}

			// プラットフォームの設定を更新する
			void StorePlatformSettingsV1( SerializedProperty property, string platform, bool overridden, int textureFormat )
			{
				property.FindPropertyRelative( "m_BuildTarget" ).stringValue				= platform ;

				property.FindPropertyRelative( "m_Overridden" ).boolValue					= overridden ;

				property.FindPropertyRelative( "m_MaxTextureSize" ).intValue				= m_MaxTextureSize ;
				property.FindPropertyRelative( "m_TextureFormat" ).intValue					= textureFormat ;
				property.FindPropertyRelative( "m_TextureCompression" ).boolValue			= ( m_SpriteAtlas_TextureCompression != TextureImporterCompression.Uncompressed ) ;
				property.FindPropertyRelative( "m_CompressionQuality" ).intValue			= m_SpriteAtlas_CompressionQuality ;
				property.FindPropertyRelative( "m_CrunchedCompression" ).boolValue			= m_SpriteAtlas_CrunchedCompression ;
			}

			//-----------------------------------------------------------------------------------------
			// Version 2 用

			bool CheckPlatformSettingsV2( TextureImporterPlatformSettings settings, bool overridden, TextureImporterFormat textureFormat )
			{
				if( settings.overridden != overridden )
				{
					return false ;
				}

				//-------------

				if( settings.maxTextureSize != m_MaxTextureSize )
				{
					return false ;
				}

				if( settings.format != textureFormat )
				{
					return false ;
				}

				if( settings.textureCompression != m_SpriteAtlas_TextureCompression )
				{
					return false ;
				}

				if( settings.compressionQuality != m_SpriteAtlas_CompressionQuality )
				{
					return false ;
				}

				if( settings.crunchedCompression != m_SpriteAtlas_CrunchedCompression )
				{
					return false ;
				}

				return true ;
			}

			// プラットフォームの設定を更新する
			void StorePlatformSettingsV2( TextureImporterPlatformSettings settings, bool overridden, TextureImporterFormat textureFormat )
			{
				settings.overridden = overridden ;

				//-------------

				settings.maxTextureSize			= m_MaxTextureSize ;
				settings.format					= textureFormat ;
				settings.textureCompression		= m_SpriteAtlas_TextureCompression ;
				settings.compressionQuality		= m_SpriteAtlas_CompressionQuality ;
				settings.crunchedCompression	= m_SpriteAtlas_CrunchedCompression ;
			}

			//-----------------------------------------------------------------------------------------

			return isDirty ;
		}
	}
}
