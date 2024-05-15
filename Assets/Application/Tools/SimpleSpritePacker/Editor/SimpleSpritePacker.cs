using System ;
using System.IO ;
using System.Collections.Generic ;
using UnityEditor ;

// Assembly に Unity.2D.Sprite.Editor が必要
using UnityEditor.U2D.Sprites ;

using UnityEngine ;


/// <summary>
/// シンプルスプライトパッカーパッケージ
/// </summary>
namespace Tools.ForSprite
{
	/// <summary>
	/// スプライトパッカークラス(エディター用) Version 2024/05/08 0
	/// </summary>
	public class SimpleSpritePacker : EditorWindow
	{
		[ MenuItem( "Tools/Simple Sprite Packer(アトラス画像作成)" ) ]
		internal static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleSpritePacker>( false, "Sprite Packer", true ) ;
		}

		// 要素がどのように変化するか
		public enum SpriteAction
		{
			None,		// 要素維持
			Add,		// 要素追加
			Update,		// 要素更新
			Delete,		// 要素削除
		} ;

		// 要素の情報
		public class SpriteElement
		{
			public	Texture2D		Texture ;	// 追加または更新が行われ場合の別テクスチャファイルのインスタンス
			public	SpriteRect		SpriteRect ;
			public	SpriteAction	Action ;
			public	int				Type ;      // 要素となる画像がテクスチャ・シングルタイプスプライト(0)なのかマルチプルタイプスプライト(1)なのか

			public	TextureSettings	TextureSettings ;
//			public TextureImporterCompression	Format ;
//			public TextureImporterNPOTScale		NPOTscale ;
		} ;

		private string m_AtlasPath = "Assets/" ;
		private string m_AtlasName = "A New Atlas" ;
		private string m_AtlasFullPath = "" ;

		private Dictionary<string,SpriteElement> m_SpriteElementHash = new () ;
	
		private int	m_Padding = 2 ;
		private int	m_MaxTextureSize = 2048 ;

		private readonly static string[] m_MaxTextureSizeLabels = new string[]{   "64",  "128",  "256",  "512", "1024", "2048", "4096", "8192" } ;
		private readonly static int[]    m_MaxTextureSizeValues = new    int[]{    64,    128,    256,    512,   1024,   2048,   4096,   8192  } ;

		private bool m_OutputSourceTexture = false ;
		private string m_OutputSourceTexturePath = "" ;

		private Vector2 m_ScrollPosition = Vector2.zero ;
	
		//----------------------------------------------------------

		// レイアウトを描画する
		internal void OnGUI()
		{
			Texture2D atlas = null ;
			bool isSetPath = false ;
			bool isAtlas = false ;

			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Atlas Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AtlasPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( Directory.Exists( path ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							path = path.Replace( "\\", "/" ) ;
							m_AtlasPath = path + "/" ;
						}
						else
						{
							// ファイルを指定しています
							path = path.Replace( "\\", "/" ) ;
							m_AtlasPath = path ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int index = m_AtlasPath.LastIndexOf( '/' ) ;
							if( index >= 0 )
							{
								m_AtlasPath = m_AtlasPath[ ..index ] + "/" ;
							}

							if( path.Length >  4 )
							{
								if( path[ ( path.Length - 4 ).. ] == ".png" )
								{
									path = path[ ( index + 1 ).. ] ;
									m_AtlasName = path[ ..( path.Length - 4 ) ] ;

									isSetPath = true ;	// パスを設定した
								}
							}
						}
					}
				}
			
				// 保存パス
				m_AtlasPath = EditorGUILayout.TextField( m_AtlasPath ) ;
			
				// 名前
				m_AtlasName = EditorGUILayout.TextField( m_AtlasName ) ;
			}
			GUILayout.EndHorizontal() ;

			// 指定しているパスのファイルがアトラスならばインスタンスを取得する
			string atlasFullPath = m_AtlasPath + m_AtlasName + ".png" ;
			if( string.IsNullOrEmpty( atlasFullPath ) == false )
			{
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasFullPath ) as TextureImporter ;
				if( textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite )
				{
					atlas = AssetDatabase.LoadAssetAtPath<Texture2D>( atlasFullPath ) ;
					isAtlas = ( textureImporter.spriteImportMode == SpriteImportMode.Multiple ) ;
				}
			}

			GUILayout.Space( 6f ) ;

			//-----------------------------------------------------
		
			// アトラステクスチャ情報の表示を行う
			if( atlas != null )
			{
				GUILayout.BeginHorizontal() ;
				{
					GUILayout.Label( "Path", GUILayout.Width( 36f ) ) ;
					GUI.color = Color.cyan ;
					GUILayout.Label( atlasFullPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;

				// アトラスが選択されている時のみ表示編集が可能となる
				GUILayout.BeginHorizontal() ;
				{
					if( GUILayout.Button( "Sprite", GUILayout.Width( 76f ) ) )
					{
						Selection.activeObject = atlas ;
					}
					GUILayout.Label( " " + atlas.width + "x" + atlas.height ) ;
				}
				GUILayout.EndHorizontal() ;

				if( isSetPath == true && isAtlas == true )
				{
					// 最大テクスチャサイズもアトラスのサイズに合わせる(ただしパスをセットした直後だけ)
					int w = atlas.width ;
					int h = atlas.height ;
					int size = Math.Max( w, h ) ;

					if( size >  m_MaxTextureSize )
					{
						// 現在設定中のサイズが既存アトラスのサイズより小さかったら既存アトラスのサイズが収まるように調整する
						int i, l = m_MaxTextureSizeValues.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( size <= m_MaxTextureSizeValues[ i ] )
							{
								break ;
							}
						}
						if( i >= l )
						{
							i  = l - 1 ;
						}
						m_MaxTextureSize = m_MaxTextureSizeValues[ i ] ;
					}
				}
			}

			//------------------------------------------

			bool clear = false ;
			if( m_AtlasFullPath != atlasFullPath )
			{
				m_AtlasFullPath  = atlasFullPath ;
				clear  = true ;
			}

			// リストを更新する
			UpdateList( atlas, clear ) ;

			//-------------------------------------------------

			bool execute  = false ;

			if( m_SpriteElementHash.Count >  0 )
			{
				// マルチタイプのスプライトで要素が１以上存在する

				GUILayout.BeginHorizontal() ;	// 横一列開始
				{
					GUILayout.Label( "Padding", GUILayout.Width( 56f ) ) ;
					m_Padding = Mathf.Clamp( EditorGUILayout.IntField( m_Padding, GUILayout.Width( 50f ) ),  0, 10 ) ;

					GUILayout.Label( "", GUILayout.Width( 16f ) ) ;

					GUILayout.Label( "MaxTextureSize", GUILayout.Width( 104f ) ) ;
					m_MaxTextureSize = EditorGUILayout.IntPopup( m_MaxTextureSize, m_MaxTextureSizeLabels, m_MaxTextureSizeValues ) ;
				}
				GUILayout.EndHorizontal() ;		// 横一列終了


				GUILayout.BeginHorizontal() ;
				{
					if( atlas == null )
					{
						// 新規作成
						// 新規作成で生成可能
						GUI.backgroundColor = Color.green ;
						execute = GUILayout.Button( "Create" ) ;
						GUI.backgroundColor = Color.white ;
					}
					else
					{
						// 維持更新
						GUI.backgroundColor = Color.cyan ;
						execute = GUILayout.Button( "Update" ) ;		// 更新または追加
						GUI.backgroundColor = Color.white ;
					}
				}
				GUILayout.EndHorizontal() ;
				

				// テクスチャ分解出力
				if( atlas != null )
				{
					// テクスチャ分解出力パス
					GUILayout.BeginHorizontal() ;
					{
						EditorGUIUtility.labelWidth = 140f ;
						EditorGUIUtility.fieldWidth =  40f ;
						m_OutputSourceTexture = EditorGUILayout.Toggle( "Output Divided Sprite", m_OutputSourceTexture ) ;
						EditorGUIUtility.labelWidth =  80f ;
						EditorGUIUtility.fieldWidth =  50f ;
					}
					GUILayout.EndHorizontal() ;
			
					if( m_OutputSourceTexture == true )
					{
						// 保存先のパスの設定
						GUILayout.BeginHorizontal() ;
						{
							// 保存パスを選択する
							if( GUILayout.Button( "Output Path", GUILayout.Width( 100f ) ) == true )
							{
								if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
								{
									// ルート
									m_OutputSourceTexturePath = "Assets/" ;
								}
								else
								if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
								{
									string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
									if( Directory.Exists( path ) == true )
									{
										// フォルダを指定しています
								
										// ファイルかどうか判別するには System.IO.File.Exists
								
										// 有効なフォルダ
										path = path.Replace( "\\", "/" ) ;
										m_OutputSourceTexturePath = path + "/" ;
									}
									else
									{
										// ファイルを指定しています
										path = path.Replace( "\\", "/" ) ;
								
										// 拡張子を見てアセットバンドルであればファイル名まで置き変える
										// ただしこれを読み出して含まれるファイルの解析などは行わない
										// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
								
										// 最後のフォルダ区切り位置を取得する
										int index = path.LastIndexOf( '/' ) ;
								
										m_OutputSourceTexturePath = path[ ..index ] + "/" ;
									}
								}
							}
					
							// 保存パス
							m_OutputSourceTexturePath = EditorGUILayout.TextField( m_OutputSourceTexturePath ) ;
					
							if( string.IsNullOrEmpty( m_OutputSourceTexturePath ) == false && atlas != null )
							{
								if( Directory.Exists( m_OutputSourceTexturePath ) == true )
								{
									GUI.backgroundColor = Color.red ;
									if( GUILayout.Button( "Execute", GUILayout.Width( 80f ) ) == true )
									{
										OutputDividedSprite( atlas, m_OutputSourceTexturePath ) ;
									}
									GUI.backgroundColor = Color.white ;
								}
							}
						}
						GUILayout.EndHorizontal() ;
					}
				}
			}

			//------------------------------------------
		
			// 素材情報のリストを表示する
			if( m_SpriteElementHash.Count >  0 )
			{
				EditorGUILayout.Separator() ;
			
				EditorGUILayout.LabelField( "Target (" + m_SpriteElementHash.Count + ")" ) ;

				GUILayout.BeginVertical() ;
				{
					m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
					{
						foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
						{
							string spriteName = spriteElementKeyValue.Key ;
							SpriteElement spriteElement = spriteElementKeyValue.Value ;
						
							GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
							{
								GUILayout.Label( spriteName, GUILayout.Height( 20f ) ) ;
							
								switch( spriteElement.Action )
								{
									// 通常状態
									case SpriteAction.None :
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											spriteElement.Action = SpriteAction.Delete ;	// 破棄対象にする
										}
									break ;

									// 追加対象
									case SpriteAction.Add :
										GUI.backgroundColor = Color.green ;
										GUILayout.Box( "Add", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 更新対象
									case SpriteAction.Update :
										GUI.backgroundColor = Color.yellow ;
										GUILayout.Box( "Update", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 削除対象
									case SpriteAction.Delete :
										GUI.backgroundColor = Color.red ;
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											spriteElement.Action = SpriteAction.None ;
										}
										GUI.backgroundColor = Color.white ;
									break ;
								}
							}
							GUILayout.EndHorizontal() ;		// 横一列終了
						}
					}
					GUILayout.EndScrollView() ;
				}
				GUILayout.EndVertical() ;
			}
			else
			{
				EditorGUILayout.HelpBox( GetMessage( "SelectTexture" ), MessageType.Info ) ;
			}

			//-------------------------------------------

			if( execute == true )
			{
				// アトラスを生成する
				BuildAtlas( atlas ) ;
			}
		}

		internal void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//---------------------------------------------------------------

		// 素材リストの情報を生成する
		private void UpdateList( Texture2D atlas, bool clear )
		{
			if( clear == true )
			{
				m_SpriteElementHash.Clear() ;
			}
		
			var spriteElementHash = new Dictionary<string,SpriteElement>() ;

			if( atlas != null )
			{
				// 既にある場合はアトラス内のスプライト情報を展開する
				string atlasPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;
			
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasPath ) as TextureImporter ;

				var factory = new SpriteDataProviderFactories() ;
				factory.Init() ;
				var dataProvider = factory.GetSpriteEditorDataProviderFromObject( textureImporter ) ;
				dataProvider.InitSpriteEditorDataProvider() ;

				var spriteRects = dataProvider.GetSpriteRects() ;

				foreach( var spriteRect in spriteRects )
				{
					string spriteName = spriteRect.name ;

					if( m_SpriteElementHash.TryGetValue( spriteName, out SpriteElement spriteElement ) )
					{
						// 既にリストに登録済みの情報
						spriteElement.Texture = null ;

						spriteElement.SpriteRect = spriteRect ;

						if( spriteElement.Action == SpriteAction.Delete )
						{
							spriteElement.Action  = SpriteAction.Delete ;
						}
						else
						{
							spriteElement.Action  = SpriteAction.None ;
						}
						spriteElement.Type = 0 ;
					}
					else
					{
						// リストに存在しない情報
						spriteElement = new SpriteElement()
						{
							Texture = null,
							SpriteRect = spriteRect,
							Action = SpriteAction.None,
							Type = 0
						} ;
					}

					spriteElementHash.Add( spriteName, spriteElement ) ;
				}
			}

			// 選択中の素材を追加する
			foreach( UnityEngine.Object target in Selection.objects )
			{
				Texture2D texture = target as Texture2D ;
			
				if( texture == null || texture == atlas )
				{
					continue ;
				}

				//-----------------------------------------

				SpriteElement spriteElement = null ;
			
				string spriteName ;

				// 素材となる画像がスプライト（アトラス）かそれ以外（テクスチャ）かで処理が異なる
				string texturePath = AssetDatabase.GetAssetPath( texture.GetInstanceID() ) ;
				TextureImporter textureImporter = AssetImporter.GetAtPath( texturePath ) as TextureImporter ;
				if( textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite )
				{
					// スプライト扱い
					if( textureImporter.spriteImportMode == SpriteImportMode.Single )
					{
						// シングルタイプ
						spriteName = texture.name ;

						if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
						{
							// 既に存在するのでアップデートになる
							spriteElement.Texture = texture ;
						
							spriteElement.SpriteRect.border	= textureImporter.spriteBorder ;
							spriteElement.SpriteRect.pivot	= textureImporter.spritePivot ;
						
							spriteElement.Action = SpriteAction.Update ;
							spriteElement.Type = 0 ;
						}
						else
						{
							// 存在しないため追加となる
							spriteElement = new ()
							{
								Texture = texture,
								SpriteRect = new SpriteRect()
								{
									name	= spriteName,
									border	= textureImporter.spriteBorder,
									pivot	= textureImporter.spritePivot
								},
								Action = SpriteAction.Add,
								Type = 0
							} ;

							spriteElementHash.Add( spriteName, spriteElement ) ;
						}
					}
					else
					if( textureImporter.spriteImportMode == SpriteImportMode.Multiple )
					{
						// マルチプルタイプ

						var factory = new SpriteDataProviderFactories() ;
						factory.Init();
						var dataProvider = factory.GetSpriteEditorDataProviderFromObject( textureImporter ) ;
						dataProvider.InitSpriteEditorDataProvider() ;
						var spriteRects = dataProvider.GetSpriteRects() ;

						foreach( var spriteRect in spriteRects )
						{
							spriteName = spriteRect.name ;
						
							if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
							{
								// 既に存在するのでアップデートになる
								spriteElement.Texture = texture ;
							
								spriteElement.SpriteRect = spriteRect ;

								spriteElement.Action = SpriteAction.Update ;
								spriteElement.Type = 1 ;
							}
							else
							{
								// 存在しないため追加となる
								spriteElement = new ()
								{
									Texture = texture,
									SpriteRect = spriteRect,
									Action = SpriteAction.Add,
									Type = 1
								} ;

								spriteElementHash.Add( spriteName, spriteElement ) ;
							}
						}
					}
				}
				else
				{
					// テクスチャ扱い
					spriteName = texture.name ;
				
					if( spriteElementHash.TryGetValue( spriteName, out spriteElement ) )
					{
						// 既に存在するのでアップデートになる
						spriteElement.Texture = texture ;
					
						spriteElement.Action = SpriteAction.Update ;
						spriteElement.Type = 0 ;
					}
					else
					{
						// 存在しないため追加となる
						spriteElement = new ()
						{
							Texture = texture,
							SpriteRect = new ()
							{
								name = spriteName
							},
							Action = SpriteAction.Add,
							Type = 0
						} ;

						spriteElementHash.Add( spriteName, spriteElement ) ;
					}
				}
			}

			m_SpriteElementHash = spriteElementHash ;
		}

		// アトラステクスチャの更新を実行する
		private void BuildAtlas( Texture2D atlas )
		{
			var texture = new Texture2D( 1, 1, TextureFormat.ARGB32, false ) ;
		
			var spriteRects = new List<SpriteRect>() ;
			var spriteTextures = new List<Texture2D>() ;
			var deletingSpriteNames = new List<string>() ;
		
			string path ;

			// 元の状態を保存する(バックアップが必要な項目)
			TextureSettings	atlasSettings = null ;

			if( atlas != null )
			{
				// 既に作成済みのアトラスの更新
				path = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;

				atlasSettings = GetOrSetTextureSettings( path, null ) ;	// 書き込み属性を有効にする
			
				atlas = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
			}

			int pn = 0 ;
			int pm = m_SpriteElementHash.Count ;

			foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
			{
				// プログレスバーを表示
				EditorUtility.DisplayProgressBar
				(
					"Loading ...",
					string.Format( "{0}/{1}", pn + 1, pm ),
					( float )( pn + 1 ) / ( float )pm
				) ;
				pn ++ ;

				//---------------------------------------------------------

				string spriteName = spriteElementKeyValue.Key ;
				SpriteElement spriteElement = spriteElementKeyValue.Value ;

				Texture2D elementTexture ;
				int x, y, w, h ;

				switch( spriteElement.Action )
				{
					// アトラススプライトに内包される領域
					case SpriteAction.None :
						spriteRects.Add( spriteElement.SpriteRect ) ;
					
						x = ( int )spriteElement.SpriteRect.rect.x ;
						y = ( int )spriteElement.SpriteRect.rect.y ;
						w = ( int )spriteElement.SpriteRect.rect.width ;
						h = ( int )spriteElement.SpriteRect.rect.height ;

						elementTexture = new ( w, h, TextureFormat.ARGB32, false ) ;
						elementTexture.SetPixels32( GetPixels32( atlas, x, y, w, h ), 0 ) ;
						elementTexture.Apply() ;

						spriteTextures.Add( elementTexture ) ;
					break ;

					// 新規追加
					case SpriteAction.Add :
						spriteRects.Add( spriteElement.SpriteRect ) ;
					
						path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
						
						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						spriteElement.TextureSettings = GetOrSetTextureSettings( path, null ) ;
					
						if( spriteElement.Type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							spriteElement.Texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
							spriteTextures.Add( spriteElement.Texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )spriteElement.SpriteRect.rect.x ;
							y = ( int )spriteElement.SpriteRect.rect.y ;
							w = ( int )spriteElement.SpriteRect.rect.width ;
							h = ( int )spriteElement.SpriteRect.rect.height ;
						
							elementTexture = new ( w, h, TextureFormat.ARGB32, false ) ;
							elementTexture.SetPixels32( GetPixels32( spriteElement.Texture, x, y, w, h ) ) ;
							elementTexture.Apply() ;
					
							spriteTextures.Add( elementTexture ) ;
						}
					break ;

					// 領域更新
					case SpriteAction.Update :
						spriteRects.Add( spriteElement.SpriteRect ) ;
				
						path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;

						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						spriteElement.TextureSettings = GetOrSetTextureSettings( path, null ) ;
					
						if( spriteElement.Type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							spriteElement.Texture = AssetDatabase.LoadAssetAtPath( path, typeof( Texture2D ) ) as Texture2D ;
							spriteTextures.Add( spriteElement.Texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )spriteElement.SpriteRect.rect.x ;
							y = ( int )spriteElement.SpriteRect.rect.y ;
							w = ( int )spriteElement.SpriteRect.rect.width ;
							h = ( int )spriteElement.SpriteRect.rect.height ;
						
							elementTexture = new ( w, h, TextureFormat.ARGB32, false ) ;
							elementTexture.SetPixels32( GetPixels32( spriteElement.Texture, x, y, w, h ) ) ;
							elementTexture.Apply() ;
						
							spriteTextures.Add( elementTexture ) ;
						}
					break ;

					// 領域削除
					case SpriteAction.Delete :
						deletingSpriteNames.Add( spriteName ) ;
					break ;
				}

				elementTexture = null ;
			}

			// プログレスバーを消す
			EditorUtility.ClearProgressBar() ;

			//--------------------------------------------------------------------------

			if( spriteRects.Count >  0 && spriteTextures.Count >  0 )
			{
				int maxSize = Mathf.Min( SystemInfo.maxTextureSize, m_MaxTextureSize ) ;

				int padding = m_Padding ;
				if( spriteTextures.Count == 1 )
				{
					// 要素が１つだけの場合はパディングは強制的に０にする
					padding = 0 ;
				}

				// パッキングを行う
				Rect[] rects = texture.PackTextures( spriteTextures.ToArray(), padding, maxSize ) ;



				pn = 0 ;
				pm = m_SpriteElementHash.Count ;

				// 後始末
				foreach( KeyValuePair<string,SpriteElement> spriteElementKeyValue in m_SpriteElementHash )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"Restoring ...",
						string.Format( "{0}/{1}", pn + 1, pm ),
						( float )( pn + 1 ) / ( float )pm
					) ;
					pn ++ ;

					SpriteElement spriteElement = spriteElementKeyValue.Value ;
				
					switch( spriteElement.Action )
					{
						// 新規追加の後始末
						case SpriteAction.Add :
							path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
							GetOrSetTextureSettings( path, spriteElement.TextureSettings ) ;	// 設定を元に戻す
						
							spriteElement.Texture = null ;
						
							spriteElement.Action = SpriteAction.None ;
						break ;

						// 領域更新の後始末
						case SpriteAction.Update :
							path = AssetDatabase.GetAssetPath( spriteElement.Texture.GetInstanceID() ) ;
							GetOrSetTextureSettings( path, spriteElement.TextureSettings ) ;
						
							spriteElement.Texture = null ;
						
							spriteElement.Action = SpriteAction.None ;
						break ;
					}
				}

				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

				//---------------------------------------------------

				// 出来上がったテクスチャに無駄が無いか確認し無駄があれば除去する

				int i, l ;

				float tw = texture.width ;
				float th = texture.height ;

				l = spriteRects.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					Rect rect = rects[ i ] ;
					rect.x      *= tw ;
					rect.y      *= th ;
					rect.width  *= tw ;
					rect.height *= th ;
					rects[ i ] = rect ;
				}

				// ソースのリージョンを検査して無駄が無いか確認する
				float xMin = tw ;
				float yMin = th ;
				float xMax = 0 ;
				float yMax = 0 ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( rects[ i ].xMin <  xMin )
					{
						xMin  = rects[ i ].xMin ;
					}
				
					if( rects[ i ].yMin <  yMin )
					{
						yMin  = rects[ i ].yMin ;
					}
				
					if( rects[ i ].xMax >  xMax )
					{
						xMax  = rects[ i ].xMax ;
					}
				
					if( rects[ i ].yMax >  yMax )
					{
						yMax  = rects[ i ].yMax ;
					}
				}

				xMin -= padding ;
				if( xMin <  0 )
				{
					xMin  = 0 ;
				}
				yMin -= padding ;
				if( yMin <  0 )
				{
					yMin  = 0 ;
				}
				xMax += padding ;
				if( xMax >  tw )
				{
					xMax  = tw ;
				}
				yMax += padding ;
				if( yMax >  th )
				{
					yMax  = th ;
				}
				int pw = ( int )( xMax - xMin ) ;
				int ph = ( int )( yMax - yMin ) ;

				int rw = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rw >= pw )
					{
						break ;
					}
					rw <<= 1 ;
				}

				int rh = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rh >= ph )
					{
						break ;
					}
					rh <<= 1 ;
				}

				if( rw <= ( int )( tw / 2 ) || rh <= ( int )( th / 2 ) )
				{
					// 無駄がある

//					Debug.LogWarning( "無駄がある:" + rw + " / " + tw + " " + rh + " / " + th ) ;

					int rx = ( int )xMin ;
					int ry = ( int )yMin ;

//					Debug.LogWarning( "取得範囲:" + rx + " " + ry + " " + rw + " " + rh ) ;

					var reduceTexture = new Texture2D( rw, rh, TextureFormat.ARGB32, false ) ;
					reduceTexture.SetPixels32( GetPixels32( texture, rx, ry, rw, rh ) ) ;
					reduceTexture.Apply() ;
				
					texture = reduceTexture ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						Rect rect = rects[ i ] ;
						rect.x      -= rx ;
						rect.y      -= ry ;
						rects[ i ] = rect ;
					}
				}
			
				//---------------------------------------------------

				string atlasFullPath ;
				if( atlas == null )
				{
					// 生成
					atlasFullPath = m_AtlasFullPath ;
				}
				else
				{
					// 更新
					atlasFullPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;
				}

				// ＰＮＧファイル化を行う
				byte[] data = texture.EncodeToPNG() ;
				File.WriteAllBytes( atlasFullPath, data ) ;
			
				AssetDatabase.SaveAssets() ;
				if( atlas == null )
				{
					AssetDatabase.Refresh() ;
				}
			
				TextureImporter textureImporter = AssetImporter.GetAtPath( atlasFullPath ) as TextureImporter ;

				// 既存の場合は以前の設定を引き継がせる
				if( atlas == null )
				{
					// 新規作成の場合に設定する
					atlasSettings = new TextureSettings() ;	// デフォルトはスプライト用の設定

					ApplyTextureSettings( textureImporter, atlasSettings ) ;

					textureImporter.spriteImportMode	= SpriteImportMode.Multiple ;
				}
				else
				{
					// 既存更新の場合は反映する
					ApplyTextureSettings( textureImporter, atlasSettings ) ;
				}

				// 警告を表示するかどうか
				bool isWarning = false ;

				// スプライト群の領域情報を更新する
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( rects[ i ].width <  spriteRects[ i ].rect.width || rects[ i ].height <  spriteRects[ i ].rect.height )
					{
						// 領域が元のものより小さくなっている
						isWarning = true ;
					}

					spriteRects[ i ].rect = rects[ i ] ;
				}

				//---------------------------------------------------------
				// マルチタイプの各スプライトの領域情報を設定する

				var factory = new SpriteDataProviderFactories() ;
				factory.Init() ;
				var dataProvider = factory.GetSpriteEditorDataProviderFromObject( textureImporter ) ;
				dataProvider.InitSpriteEditorDataProvider() ;

				dataProvider.SetSpriteRects( spriteRects.ToArray() ) ;
				dataProvider.Apply() ;

				//---------------------------------------------------------

				// 設定を上書き保存する
				textureImporter.SaveAndReimport() ;

				// アトラステクスチャを再ロードしておく(リストの表示更新用)
				atlas = AssetDatabase.LoadAssetAtPath( atlasFullPath, typeof( Texture2D ) ) as Texture2D ;

				spriteRects.Clear() ;
				spriteTextures.Clear() ;

				Resources.UnloadUnusedAssets() ;

				//---------------------------------------------------------

				if( isWarning == true )
				{
					// クオリティが落ちている可能性があるため警告用のダイアログを表示する
					EditorUtility.DisplayDialog( GetMessage( "Warning" ), GetMessage( "WarningMessage" ), GetMessage( "Close" ) ) ;
				}
			}
			else
			{
				// 要素が全て無くなるためファイル自体を削除する
				AssetDatabase.DeleteAsset( m_AtlasFullPath ) ;
				atlas = null ;
			}

			//----------------------------------------------------------
			// アセットデータベースの更新

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			//----------------------------------------------------------

			// 削除されたスプライトの情報を消去する
			foreach( string deletingSpriteName in deletingSpriteNames )
			{
				m_SpriteElementHash.Remove( deletingSpriteName ) ;
			}

			// リストの表示を更新する
			UpdateList( atlas, true ) ;
		}

#if false
		private Texture2D Decompress( Texture2D sourceTexture )
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary
			(
				sourceTexture.width,
				sourceTexture.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear
			) ;

			Graphics.Blit( sourceTexture, renderTexture ) ;

			RenderTexture previous = RenderTexture.active ;
			RenderTexture.active = renderTexture ;

			var readableTexture = new Texture2D( sourceTexture.width, sourceTexture.height ) ;
			readableTexture.ReadPixels( new Rect( 0, 0, renderTexture.width, renderTexture.height ), 0, 0 ) ;
			readableTexture.Apply() ;

			RenderTexture.active = previous ;
			RenderTexture.ReleaseTemporary( renderTexture ) ;

			return readableTexture ;
		}
#endif
		// 指定した領域のピクセル情報を取得する
		private Color32[] GetPixels32( Texture2D texture, int x, int y, int w, int h )
		{
			// 一度全ピクセルを取得する
			var fullPixels32 = texture.GetPixels32( 0 ) ;

			var pixels32 = new Color32[ w * h ] ;

			int o0, o1, o2 ;
			int lx, ly ;

			int tw = texture.width ;

			o0 = y * tw + x ;
			o2 = 0 ;
			for( ly  = 0 ; ly <  h ; ly ++ )
			{
				o1 = o0 ;
				for( lx  = 0 ; lx <  w ; lx ++ )
				{
					pixels32[ o2 ] = fullPixels32[ o1 ] ;
					o1 ++ ;
					o2 ++ ;
				}
				o0 += tw ;
			}

			return pixels32 ;
		}

		// 個々の要素をシングルタイプスプライトして書き出す
		private void OutputDividedSprite( Texture2D atlas, string folderPath )
		{
			if( string.IsNullOrEmpty( folderPath ) == true )
			{
				return ;
			}

			if( Directory.Exists( folderPath ) == false )
			{
				return ;
			}
			
			int l = folderPath.Length ;
			if( folderPath[ l - 1 ] != '/' )
			{
				folderPath += '/' ;
			}

			//----------------------------------------------------------

			// アトラススプライトを読み出し可能状態にする
			string atlasPath = AssetDatabase.GetAssetPath( atlas.GetInstanceID() ) ;

			// 元の状態を保存する
			TextureSettings atlasSettings ;

			atlasSettings =	GetOrSetTextureSettings( atlasPath, null ) ;	// 読み込み属性を有効にする
		
			atlas = AssetDatabase.LoadAssetAtPath( atlasPath, typeof( Texture2D ) ) as Texture2D ;

			TextureImporter atlasTextureImporter = AssetImporter.GetAtPath( atlasPath ) as TextureImporter ;
			int x, y, w, h ;

			Texture2D elementTexture ;
			string elementPath ;
			TextureImporter elementTextureImporter ;

//			TextureSettings elementSettings = new TextureSettings() ;

			try
			{
//				EditorApplication.LockReloadAssemblies() ;
//				AssetDatabase.StartAssetEditing() ;


				var factory = new SpriteDataProviderFactories() ;
				factory.Init();
				var dataProvider = factory.GetSpriteEditorDataProviderFromObject( atlasTextureImporter ) ;
				dataProvider.InitSpriteEditorDataProvider() ;

				var spriteRects = dataProvider.GetSpriteRects() ;

				int pn = 0 ;
				int pm = spriteRects.Length ;

				foreach( var spriteRect in spriteRects )
				{
					// プログレスバーを表示
					EditorUtility.DisplayProgressBar
					(
						"Saving ...",
						string.Format( "{0}/{1}", pn + 1, pm ),
						( float )( pn + 1 ) / ( float )pm
					) ;
					pn ++ ;

					x = ( int )spriteRect.rect.x ;
					y = ( int )spriteRect.rect.y ;
					w = ( int )spriteRect.rect.width ;
					h = ( int )spriteRect.rect.height ;
			
					elementTexture = new ( w, h, TextureFormat.ARGB32, false ) ;
					elementTexture.SetPixels32( GetPixels32( atlas, x, y, w, h ) ) ;
					elementTexture.Apply() ;

					elementPath = folderPath + spriteRect.name + ".png" ;

					// テクスチャをＰＮＧ画像として保存する
					byte[] data = elementTexture.EncodeToPNG() ;
					File.WriteAllBytes( elementPath, data ) ;
					data = null ;

					AssetDatabase.SaveAssets() ;
					AssetDatabase.Refresh() ;

					// 新規生成

					elementTextureImporter = AssetImporter.GetAtPath( elementPath ) as TextureImporter ;

					ApplyTextureSettings( elementTextureImporter, atlasSettings ) ;

					elementTextureImporter.spriteImportMode		= SpriteImportMode.Single ;

					elementTextureImporter.spriteBorder			= spriteRect.border ;
					elementTextureImporter.spritePivot			= spriteRect.pivot ;

					elementTextureImporter.SaveAndReimport() ;

					elementTexture = null ;
				}
			}
			finally
			{
				// プログレスバーを消す
				EditorUtility.ClearProgressBar() ;

//				AssetDatabase.StopAssetEditing() ;
//				EditorApplication.UnlockReloadAssemblies() ;
			}

			// 後始末
			GetOrSetTextureSettings( atlasPath, atlasSettings ) ;	// 読み込み属性を無効にする

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;
		}

		//------------------------------------------------------------

		/// <summary>
		/// テクスチャの設定
		/// </summary>
		public class TextureSettings
		{
			// General
			public	TextureImporterType				TextureType				= TextureImporterType.Sprite ;
			public	TextureImporterNPOTScale		NPOTScale				= TextureImporterNPOTScale.None ;
			public	bool							IsReadable				= false ;
			public	bool							MipmapEnabled			= false ;
			public	bool							AlphaIsTransparency		= true ;
			public	TextureWrapMode					WrapMode				= TextureWrapMode.Clamp ;
			public	FilterMode						FilterMode				= FilterMode.Bilinear ;

			// Default
			public class DefaultSettings
			{
				public	int							MaxTextureSize		= 2048 ;
				public	TextureImporterCompression	TextureCompression	= TextureImporterCompression.Compressed ;
				public	int							CompressionQuality	= 100 ;
			}
			public	DefaultSettings					Default		= new () ;

			public class PlatformSettings
			{
				public	bool						Overridden			= false ;
				public	int							MaxTextureSize		= 2048 ;
				public	TextureImporterFormat		Format				= TextureImporterFormat.Automatic ;
				public	TextureImporterCompression	TextureCompression	= TextureImporterCompression.Compressed ;
			}

			// Standalone
			public	PlatformSettings				Standalone	= new () ;

			// Android
			public	PlatformSettings				Android		= new () ;

			// iOS
			public	PlatformSettings				iOS			= new () ;
		}

		// テクスチャの設定を取得または設定する
		private TextureSettings GetOrSetTextureSettings( string path, TextureSettings textureSettings )
		{
			var textureImporter = AssetImporter.GetAtPath( path ) as TextureImporter ;

			TextureImporterPlatformSettings ps ;

			if( textureSettings == null )
			{
				// 取得しつつ無圧縮状態にする

				textureSettings = new ()
				{
					// General
					TextureType						= textureImporter.textureType,
					NPOTScale						= textureImporter.npotScale,
					IsReadable						= textureImporter.isReadable,
					MipmapEnabled					= textureImporter.mipmapEnabled,
					AlphaIsTransparency				= textureImporter.alphaIsTransparency,
					WrapMode						= textureImporter.wrapMode,
					FilterMode						= textureImporter.filterMode
				} ;

				// Default
				textureSettings.Default.MaxTextureSize			= textureImporter.maxTextureSize ;
				textureSettings.Default.TextureCompression		= textureImporter.textureCompression ;
				textureSettings.Default.CompressionQuality		= textureImporter.compressionQuality ;

				// Standalone
				ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
				textureSettings.Standalone.Overridden			= ps.overridden ;
				textureSettings.Standalone.MaxTextureSize		= ps.maxTextureSize ;
				textureSettings.Standalone.Format				= ps.format ;
				textureSettings.Standalone.TextureCompression	= ps.textureCompression ;

				// Android
				ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
				textureSettings.Android.Overridden				= ps.overridden ;
				textureSettings.Android.MaxTextureSize			= ps.maxTextureSize ;
				textureSettings.Android.Format					= ps.format ;
				textureSettings.Android.TextureCompression		= ps.textureCompression ;

				// iOS
				ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
				textureSettings.iOS.Overridden					= ps.overridden ;
				textureSettings.iOS.MaxTextureSize				= ps.maxTextureSize ;
				textureSettings.iOS.Format						= ps.format ;
				textureSettings.iOS.TextureCompression			= ps.textureCompression ;

				//---------------------------------------------------------
				// 無圧縮

				// General
				textureImporter.textureType						= TextureImporterType.Sprite ;				//RGB
				textureImporter.npotScale						= TextureImporterNPOTScale.None ;				//RGB
				textureImporter.isReadable						= true ;									// 読み出し可能
				textureImporter.mipmapEnabled					= false ;
				textureSettings.AlphaIsTransparency				= true ;
				textureSettings.WrapMode						= TextureWrapMode.Clamp ;
				textureSettings.FilterMode						= FilterMode.Point ;

				// Default
				textureImporter.maxTextureSize					= 8192 ;									// 縮められないように 2048 最大で
				textureImporter.textureCompression				= TextureImporterCompression.Uncompressed ;	// 無圧縮
				textureImporter.compressionQuality				= 100 ;										// 無圧縮

				// Standalone
				ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
				ps.overridden									= true ;
				ps.maxTextureSize								= textureImporter.maxTextureSize ;
				ps.format										= TextureImporterFormat.Automatic ;
				ps.textureCompression							= textureImporter.textureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// Android
				ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
				ps.overridden									= true ;
				ps.maxTextureSize								= textureImporter.maxTextureSize ;
				ps.format										= TextureImporterFormat.Automatic ;
				ps.textureCompression							= textureImporter.textureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// iOS
				ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
				ps.overridden									= true ;
				ps.maxTextureSize								= textureImporter.maxTextureSize ;
				ps.format										= TextureImporterFormat.Automatic ;
				ps.textureCompression							= textureImporter.textureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				//---------------------------------------------------------
				// 反映

				textureImporter.SaveAndReimport() ;
			}
			else
			{
				// 設定

				// General
				textureImporter.textureType						= textureSettings.TextureType ;
				textureImporter.npotScale						= textureSettings.NPOTScale ;
				textureImporter.isReadable						= textureSettings.IsReadable ;
				textureImporter.mipmapEnabled					= textureSettings.MipmapEnabled ;
				textureImporter.alphaIsTransparency				= textureSettings.AlphaIsTransparency ;
				textureImporter.wrapMode						= textureSettings.WrapMode ;
				textureImporter.filterMode						= textureSettings.FilterMode ;

				// Default
				textureImporter.maxTextureSize					= textureSettings.Default.MaxTextureSize ;
				textureImporter.textureCompression				= textureSettings.Default.TextureCompression ;
				textureImporter.compressionQuality				= textureSettings.Default.CompressionQuality ;

				// Standalone
				ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
				ps.overridden									= textureSettings.Standalone.Overridden ;
				ps.maxTextureSize								= textureSettings.Standalone.MaxTextureSize ;
				ps.format										= textureSettings.Standalone.Format ;
				ps.textureCompression							= textureSettings.Standalone.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// Android
				ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
				ps.overridden									= textureSettings.Android.Overridden ;
				ps.maxTextureSize								= textureSettings.Android.MaxTextureSize ;
				ps.format										= textureSettings.Android.Format ;
				ps.textureCompression							= textureSettings.Android.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				// iOS
				ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
				ps.overridden									= textureSettings.iOS.Overridden ;
				ps.maxTextureSize								= textureSettings.iOS.MaxTextureSize ;
				ps.format										= textureSettings.iOS.Format ;
				ps.textureCompression							= textureSettings.iOS.TextureCompression ;
				textureImporter.SetPlatformTextureSettings( ps ) ;

				//---------------------------------------------------------
				// 反映

				textureImporter.SaveAndReimport() ;
			}

			return textureSettings ;
		}

		// インポーターに設定を反映させる
		private void ApplyTextureSettings( TextureImporter textureImporter, TextureSettings textureSettings )
		{
			TextureImporterPlatformSettings ps ;

			// General
			textureImporter.textureType						= textureSettings.TextureType ;
			textureImporter.npotScale						= textureSettings.NPOTScale ;
			textureImporter.isReadable						= textureSettings.IsReadable ;
			textureImporter.mipmapEnabled					= textureSettings.MipmapEnabled ;

			// Default
			textureImporter.maxTextureSize					= textureSettings.Default.MaxTextureSize ;
			textureImporter.textureCompression				= textureSettings.Default.TextureCompression ;
			textureImporter.compressionQuality				= textureSettings.Default.CompressionQuality ;

			// Standalone
			ps												= textureImporter.GetPlatformTextureSettings( "Standalone" ) ;
			ps.overridden									= textureSettings.Standalone.Overridden ;
			ps.maxTextureSize								= textureSettings.Standalone.MaxTextureSize ;
			ps.format										= textureSettings.Standalone.Format ;
			ps.textureCompression							= textureSettings.Standalone.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// Android
			ps												= textureImporter.GetPlatformTextureSettings( "Android" ) ;
			ps.overridden									= textureSettings.Android.Overridden ;
			ps.maxTextureSize								= textureSettings.Android.MaxTextureSize ;
			ps.format										= textureSettings.Android.Format ;
			ps.textureCompression							= textureSettings.Android.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;

			// iOS
			ps												= textureImporter.GetPlatformTextureSettings( "iPhone" ) ;
			ps.overridden									= textureSettings.iOS.Overridden ;
			ps.maxTextureSize								= textureSettings.iOS.MaxTextureSize ;
			ps.format										= textureSettings.iOS.Format ;
			ps.textureCompression							= textureSettings.iOS.TextureCompression ;
			textureImporter.SetPlatformTextureSettings( ps ) ;
		}

		//--------------------------------------------------------------------------

		private readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "SelectTexture",	"パック対象にしたいテクスチャを選択してください(複数可)" },
			{ "Warning",		"注意" },
			{ "WarningMessage",	"スプライトが縮小されている可能性があります\nMaxTextureSizeを大きくして\nUpdateを実行する事をお勧めします" },
			{ "Close",			"閉じる" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "SelectTexture",	"Please select one or more textures in the Project View window." },
			{ "Warning",		"Warning" },
			{ "WarningMessage",	"The sprite may have been shrunk.\nWe recommend increasing MaxTextureSize and running Update." },
			{ "Close",			"Close" },
		} ;

		private string GetMessage( string label )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( label ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ label ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( label ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ label ] ;
			}
		}
	}
}

