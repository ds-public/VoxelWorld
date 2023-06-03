using System ;
using System.Linq ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text.RegularExpressions ;
using System.Reflection ;

using UnityEngine ;
using UnityEditor ;

using UnityEngine.U2D ;

namespace AssetSettings
{
	/// <summary>
	/// Audio の設定 Version 2023/06/03
	/// </summary>
	public class AudioSettings : ImportProcessor
	{
		// フォルダ無指定時の対象フォルダ
		private static readonly string[] m_Paths =
		{
			@"Assets/Application/AssetBundle/Sounds/*",
		} ;

		// AudioClip インポート用のディスパッチャー
		private static readonly ImportDispatcher<AudioImporter> m_AudioDispatcher
			= new ImportDispatcher<AudioImporter>
			(
				m_Paths,
				ReplaceAudioSettings,	// ファイル単位の処理
				null
			) ;

		//---------------

		// バッチ処理用のディスパッチャー
		private static readonly ImportDispatcher<AudioImporter> m_BatchDispatcher
			= new ImportDispatcher<AudioImporter>
			(
				m_Paths,
				null,
				ReplaceBatchSettings	// 全てのファイルの処理
			) ;

		//-------------------------------------------------------------------------------------------

		// メニューから全て設定し直す
		[ MenuItem( "AssetSettings/Audio - Reimport" ) ]
		internal static void ReimportAllAssets()
		{
			string targetPath = null ;
			if( Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				// １つだけ選択（複数選択には対応していない：フォルダかファイル）
				targetPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ).Replace( '\\', '/' ) ;
			}

			m_BatchDispatcher.SetupAll( targetPath ) ;
		}

		//-------------------------------------------------------------------------------------------
		// コールバックメソッド群(オーバーライド)

		/// <summary>
		/// オーディオクリップがインポートされる前に呼び出されます
		/// </summary>
		/// <param name="assetPostprocessor"></param>
		public override void OnPreprocessAudio( AssetPostprocessor assetPostprocessor )
		{
			AssetImporter assetImporter = assetPostprocessor.assetImporter ;
			if( assetImporter.importSettingsMissing == false )
			{
				// 既にインポート済み(metaファイルが作られている)は無視する(でないと Inspector で設定を変更するたびに Importer が動作してしまう)
//				Debug.Log( "既にインポートされている:" + assetImporter.assetPath ) ;
				return ;
			}

			if( assetImporter is AudioImporter )
			{
				// 指定のファイルのみ設定する
				m_AudioDispatcher.SetupAny( assetImporter as AudioImporter ) ;
			}
		}

		/// <summary>
		/// オーディオクリップが移動した際も処理を施す
		/// </summary>
		/// <param name="importedAssets"></param>
		/// <param name="deletedAssets"></param>
		/// <param name="movedAssets"></param>
		/// <param name="movedFromAssetPaths"></param>
		public override bool OnAssetMoved( string pathTo, string pathFrom )
		{
			AssetImporter assetImporter = AssetImporter.GetAtPath( pathTo ) ;
			if( assetImporter is AudioImporter )
			{
				return m_AudioDispatcher.SetupAny( assetImporter as AudioImporter ) ;
			}

			return false ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// AudioClip の設定を行う(インポート用)
		/// </summary>
		private static bool ReplaceAudioSettings( AudioImporter assetImporter )
		{
			return ProcessAudioSettings( assetImporter ) ;
		}

		/// <summary>
		/// AudioClip の設定を行う(バッチ専用)
		/// </summary>
		private static bool ReplaceBatchSettings( AudioImporter assetImporter )
		{
			// AudioClip の設定を行う
			if( ProcessAudioSettings( assetImporter as AudioImporter ) == true )
			{
				Debug.Log( "<color=#FFFF00>[AudioClip Importing] " + assetImporter.assetPath + "</color>" ) ;
				AssetDatabase.ImportAsset( assetImporter.assetPath ) ;

				return true ;
			}
			else
			{
				return false ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// AudioClip の設定を行う
		/// </summary>
		private static bool ProcessAudioSettings( AudioImporter audioImporter )
		{
			// 再設定を行ったかどうか
			bool isDirty = false ;

			if( m_Dispatchers == null || m_Dispatchers.Length == 0 )
			{
				return isDirty ;
			}

			// インポート先のパスによって設定する内容を変える
			bool isProcessing = false ;
			foreach( var dispatcher in m_Dispatchers )
			{
				( isProcessing, isDirty ) = dispatcher.Match( audioImporter ) ;

				if( isProcessing == true )
				{
					break ;	// 最初に該当したパスの設定のみ行う
				}
			}

			if( isProcessing == false )
			{
				// いずれにも該当しなかったのでデフォルト設定を行う
				isDirty = SetDefaultSetting( audioImporter ) ;
			}

			return isDirty ;
		}

		//===========================================================================================
		// AudioClip のパスに応じて設定を変える

		/// <summary>
		/// パスと設定
		/// </summary>
		public class Dispatcher
		{
			public string[]					Paths ;		// パス群
			public Func<AudioImporter,bool>	Method ;	// 設定メソッド

			/// <summary>
			/// パス群のいずれかの先頭部分にマッチすれば設定を行う
			/// </summary>
			/// <param name="ti"></param>
			/// <returns></returns>
			public ( bool, bool ) Match( AudioImporter audioImporter )
			{
				string path = audioImporter.assetPath.Replace( "\\", "/" ) ;

				if( Paths.Any( _ => path.IndexOf( _ ) == 0 ) )
				{
					// いずれかのパスの先頭にマッチする
					if( Method != null )
					{
						return ( true, Method( audioImporter ) ) ;
					}
				}

				return ( false, false ) ;
			}
		}

		// ※パスと設定を追加したい場合は、設定メソッドを用意し、パスと設定のセットをここに追加してください。

		// パスと設定の一覧
		private static readonly Dispatcher[] m_Dispatchers = new Dispatcher[]
		{
			// BGM
			new Dispatcher()
			{
				// 対象パス
				Paths = new string[]
				{
					"Assets/Application/AssetBundle/Sounds/BGM",		// アセットバンドル内のオーディオクリップ
				},
				// 設定メソッド
				Method = SetBgmSetting,									// ＢＧＭ用の設定にする
			},

			// Jingle
			new Dispatcher()
			{
				// 対象パス
				Paths = new string[]
				{
					"Assets/Application/AssetBundle/Sounds/Jingle",		// アセットバンドル内のオーディオクリップ
				},
				// 設定メソッド
				Method = SetJingleSetting,								// Ｊｉｎｇｌｅ用の設定にする
			},

			// SE
			new Dispatcher()
			{
				// 対象パス
				Paths = new string[]
				{
					"Assets/Application/AssetBundle/Sounds/SE",			// アセットバンドル内のオーディオクリップ
				},
				// 設定メソッド
				Method = SetSeSetting,									// ＳＥ用の設定にする
			},

			// Voice
			new Dispatcher()
			{
				// 対象パス
				Paths = new string[]
				{
					"Assets/Application/AssetBundle/Sounds/Voice",		// アセットバンドル内のオーディオクリップ
				},
				// 設定メソッド
				Method = SetVoiceSetting,								// Ｖｏｉｃｅ用の設定にする
			},
		} ;

		//-----------------------------------

		/// <summary>
		/// 該当が無い場合のデフォルト設定を行う
		/// </summary>
		private static bool SetDefaultSetting( AudioImporter audioImporter )
		{
			return SetSetting( audioImporter, AudioClipLoadType.CompressedInMemory ) ;
		}

		/// <summary>
		/// ＢＧＭ用の設定を行う
		/// </summary>
		private static bool SetBgmSetting( AudioImporter audioImporter )
		{
			return SetSetting( audioImporter, AudioClipLoadType.CompressedInMemory ) ;
		}

		/// <summary>
		/// Ｊｉｎｇｌｅ用の設定を行う
		/// </summary>
		private static bool SetJingleSetting( AudioImporter audioImporter )
		{
			return SetSetting( audioImporter, AudioClipLoadType.CompressedInMemory ) ;
		}

		/// <summary>
		/// ＳＥ用の設定を行う
		/// </summary>
		private static bool SetSeSetting( AudioImporter audioImporter )
		{
			return SetSetting( audioImporter, AudioClipLoadType.DecompressOnLoad ) ;
		}

		/// <summary>
		/// Ｖｏｉｃｅ用の設定を行う
		/// </summary>
		private static bool SetVoiceSetting( AudioImporter audioImporter )
		{
			return SetSetting( audioImporter, AudioClipLoadType.CompressedInMemory ) ;
		}

		//-----------------------------------

		private static bool SetSetting( AudioImporter audioImporter, AudioClipLoadType loadType )
		{
			// 再設定の必要があるか・再設定が実際に行われたか
			bool isDirty = false ;

			//----------------------------------
			// 共通

			AudioImporterSampleSettings ai_Default = audioImporter.defaultSampleSettings ;
			if( ai_Default.loadType != loadType )
			{
				ai_Default.loadType  = loadType ;
				isDirty = true ;
			}
			if( ai_Default.compressionFormat != AudioCompressionFormat.Vorbis )
			{
				ai_Default.compressionFormat  = AudioCompressionFormat.Vorbis ;
				isDirty = true ;
			}
			if( ai_Default.quality != 1.0f )
			{
				ai_Default.quality = 1.0f ;
				isDirty = true ;
			}
			audioImporter.defaultSampleSettings = ai_Default ;

			//----------------------------------
			// プラットフォームごと

			// Standalone
			AudioImporterSampleSettings ai_PC = audioImporter.GetOverrideSampleSettings( "Standalone" ) ;
			if( ai_PC.loadType != loadType )
			{
				ai_PC.loadType  = loadType ;
				isDirty = true ;
			}
			if( ai_PC.compressionFormat != AudioCompressionFormat.Vorbis )
			{
				ai_PC.compressionFormat  = AudioCompressionFormat.Vorbis ;
				isDirty = true ;
			}
			if( ai_PC.quality != 1.0f )
			{
				ai_PC.quality  = 1.0f ;
				isDirty = true ;
			}
			audioImporter.SetOverrideSampleSettings( "Standalone", ai_PC ) ;

			// Android
			AudioImporterSampleSettings ai_android = audioImporter.GetOverrideSampleSettings( "Android" ) ;
			if( ai_android.loadType != loadType )
			{
				ai_android.loadType  = loadType ;
				isDirty = true ;
			}
			if( ai_android.compressionFormat != AudioCompressionFormat.Vorbis )
			{
				ai_android.compressionFormat  = AudioCompressionFormat.Vorbis ;
				isDirty = true ;
			}
			if( ai_android.quality != 1.0f )
			{
				ai_android.quality  = 1.0f ;
				isDirty = true ;
			}
			audioImporter.SetOverrideSampleSettings( "Android", ai_android ) ;

			// iOS
			AudioImporterSampleSettings ai_iOS = audioImporter.GetOverrideSampleSettings( "iOS" ) ;
			if( ai_iOS.loadType != loadType )
			{
				ai_iOS.loadType  = loadType ;
				isDirty = true ;
			}
			if( ai_iOS.compressionFormat != AudioCompressionFormat.MP3 )
			{
				ai_iOS.compressionFormat = AudioCompressionFormat.MP3 ;
				isDirty = true ;
			}
			if( ai_iOS.quality != 1.0f )
			{
				ai_iOS.quality = 1.0f ;
				isDirty = true ;
			}
			audioImporter.SetOverrideSampleSettings( "iOS", ai_iOS ) ;

			return isDirty ;
		}

	}
}
