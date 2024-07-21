#if UNITY_EDITOR

using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[CustomEditor( typeof( SpriteDrawer ) )]
	public class SpriteDrawerInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			var component = target as SpriteDrawer ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//----------------------------------

			// オフセット
			var offset2D_Old = new Vector2( component.Offset.x, component.Offset.y ) ;
			var offset2D_New = EditorGUILayout.Vector2Field( "Offset", offset2D_Old ) ;
			if( offset2D_New.Equals( offset2D_Old ) == false )
			{
				Undo.RecordObject( component, "SpriteDrawer : Offset Change" ) ;	// アンドウバッファに登録
				component.Offset = new Vector2( offset2D_New.x, offset2D_New.y ) ;
				EditorUtility.SetDirty( component ) ;
			}
	
			// サイズ
			var size2D_Old = new Vector2( component.Size.x, component.Size.y ) ;
			var size2D_New = EditorGUILayout.Vector2Field( "Size", size2D_Old ) ;
			if( size2D_New.Equals( size2D_Old ) == false )
			{
				Undo.RecordObject( component, "SpriteDrawer : Size Change" ) ;	// アンドウバッファに登録
				component.Size = new Vector2( size2D_New.x, size2D_New.y ) ;
				EditorUtility.SetDirty( component ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//----------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool flipX = EditorGUILayout.Toggle( component.FlipX, GUILayout.Width( 16f ) ) ;
				if( component.FlipX != flipX )
				{
					Undo.RecordObject( component, "SpriteDrawer : FlipX Change" ) ;	// アンドウバッファに登録
					component.FlipX = flipX ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "FlipX", "横方向を反転表示します" ), GUILayout.Width( 64f ) ) ;

				bool flipY = EditorGUILayout.Toggle( component.FlipY, GUILayout.Width( 16f ) ) ;
				if( component.FlipY != flipY )
				{
					Undo.RecordObject( component, "SpriteDrawer : FlipY Change" ) ;	// アンドウバッファに登録
					component.FlipY = flipY ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "FlipY", "縦方向を反転表示します" ), GUILayout.Width( 64f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			// 基本のカラー
			var vertexColor = Color.white ;
			vertexColor.r = component.VertexColor.r ;
			vertexColor.g = component.VertexColor.g ;
			vertexColor.b = component.VertexColor.b ;
			vertexColor.a = component.VertexColor.a ;
			vertexColor = EditorGUILayout.ColorField( "Vertex Color", vertexColor ) ;
			if
			(
				vertexColor.r != component.VertexColor.r ||
				vertexColor.g != component.VertexColor.g ||
				vertexColor.b != component.VertexColor.b ||
				vertexColor.a != component.VertexColor.a
			)
			{
				Undo.RecordObject( component, "SpriteDrawer : VertexColor Change" ) ;	// アンドウバッファに登録
				component.VertexColor = vertexColor ;
				EditorUtility.SetDirty( component ) ;
			}

			// テクスチャ
			var texture = EditorGUILayout.ObjectField( "Texture", component.Texture, typeof( Texture ), false ) as Texture ;
			if( component.Texture != texture )
			{
				Undo.RecordObject( component, "SpriteDrawer : Texture Change" ) ;	// アンドウバッファに登録
				component.Texture  = texture ;
				EditorUtility.SetDirty( component ) ;
			}

			//-----------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//------------------------------------------------------------------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// マテリアル
			DrawMaterial( component ) ;

			// カラー
			DrawMaterialColor( component ) ;

			// スプライト
			DrawSprite( component ) ;

			//----------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// アトラス関連
			DrawAtlas( component ) ;
		}

		//-------------------------------------------------------------------------------------------
		// Material

		protected void DrawMaterial( SpriteDrawer component )
		{
			// マテリアル
			var material = EditorGUILayout.ObjectField( "Maretial", component.Material, typeof( Material ), false ) as Material ;
			if( material != component.Material )
			{
				Undo.RecordObject( component, "SpriteDrawer : Material Change" ) ;	// アンドウバッファに登録
				component.Material = material ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Color

		protected void DrawMaterialColor( SpriteDrawer component )
		{
			// カラー
			var materialColor = Color.white ;
			materialColor.r = component.MaterialColor.r ;
			materialColor.g = component.MaterialColor.g ;
			materialColor.b = component.MaterialColor.b ;
			materialColor.a = component.MaterialColor.a ;
			materialColor = EditorGUILayout.ColorField( "Material Color", materialColor ) ;
			if
			(
				materialColor.r != component.MaterialColor.r ||
				materialColor.g != component.MaterialColor.g ||
				materialColor.b != component.MaterialColor.b ||
				materialColor.a != component.MaterialColor.a
			)
			{
				Undo.RecordObject( component, "SpriteDrawer : MaterialColor Change" ) ;	// アンドウバッファに登録
				component.MaterialColor = materialColor ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-----------------------------------------------------------
		// Sprite

		protected void DrawSprite( SpriteDrawer component )
		{
			var sprite = EditorGUILayout.ObjectField( "Sprite", component.Sprite, typeof( Sprite ), false ) as Sprite ;
			if( component.Sprite != sprite )
			{
				Undo.RecordObject( component, "SpriteDrawer : Sprite Change" ) ;	// アンドウバッファに登録
				component.Sprite  = sprite ;
				EditorUtility.SetDirty( component ) ;
			}

			if( component.Sprite != null )
			{
				// サイズ
				EditorGUILayout.BeginHorizontal() ;
				{
					GUILayout.FlexibleSpace() ;
					GUILayout.Label( $"{component.Sprite.rect.width} x {component.Sprite.rect.height}" ) ;
				}
				EditorGUILayout.EndHorizontal() ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Atlas

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( SpriteDrawer component )
		{
			// スプライトアトラス
			SpriteAtlas spriteAtlas = EditorGUILayout.ObjectField( new GUIContent( "Sprite Atlas", "<color=#00FFFF>SpriteAtlas</color>アセットを設定します\nランタイム実行中、<color=#00FFFF>SetSpriteInAtlas</color>メソッドを使用する事により\n表示する<color=#00FFFF>Spriteを動的に切り替える</color>事が出来ます" ), component.SpriteAtlas, typeof( SpriteAtlas ), false ) as SpriteAtlas ;
			if( component.SpriteAtlas != spriteAtlas)
			{
				Undo.RecordObject( component, "[SpriteController] Sprite Atlas : Change" ) ;	// アンドウバッファに登録

				// SpriteAtlas 側を設定する
				component.SpriteAtlas = spriteAtlas ;

				// SpriteSet 側を消去する
				component.SpriteSet = null ;

				if( component.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					var sprites = GetSprites( component.SpriteAtlas ) ;
					if( sprites != null && sprites.Length >  0 )
					{
						component.Sprite = sprites[ 0 ] ;
					}
				}

				EditorUtility.SetDirty( component ) ;
			}

			if( component.SpriteAtlas != null )
			{
				// スプライトアトラスのテクスチャ(表示のみ)
//				EditorGUILayout.ObjectField( "Sprite Atlas Texture", image.SpriteAtlasTexture, typeof( Texture2D ), true ) ;

				//---------------------------------

				var sprites = GetSprites( component.SpriteAtlas ) ;
				if( sprites != null && sprites.Length >  0 )
				{
					int i, l = sprites.Length ;
					var spriteNames = new List<string>() ;
					foreach( var sprite in sprites )
					{
						spriteNames.Add( sprite.name ) ;
					}

					string currentSpriteName = null ;
					if( component.Sprite != null )
					{
						currentSpriteName = component.Sprite.name ;
					}

					int indexBase = -1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( spriteNames[ i ] == currentSpriteName )
						{
							indexBase = i ;
							break ;
						}
					}

					int indexMove = 0 ;
					if( indexBase <  0 )
					{
						spriteNames.Insert( 0, "Unknown" ) ;
						indexBase = 0 ;
						indexMove = 1 ;
					}

					// フレーム番号
					int index = EditorGUILayout.Popup( "Selected Sprite", indexBase, spriteNames.ToArray() ) ;
					if( index != indexBase )
					{
						Undo.RecordObject( component, "[SpriteController] Sprite : Change" ) ;	// アンドウバッファに登録
						component.Sprite = sprites[ index - indexMove ] ;
						EditorUtility.SetDirty( component ) ;
					}
				}

				// 確認用
				EditorGUILayout.ObjectField( " ", component.Sprite, typeof( Sprite ), false ) ;

				if( component.Sprite != null )
				{
					// サイズ
					EditorGUILayout.BeginHorizontal() ;
					{
						GUILayout.FlexibleSpace() ;
						GUILayout.Label( $"{component.Sprite.rect.width} x {component.Sprite.rect.height}" ) ;
					}
					EditorGUILayout.EndHorizontal() ;
				}
			}

			//----------------------------------------------------------
			// 以下はレガシー

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//----------------------------------

			Texture spriteSetTextureActive = null ;
			if( component.SpriteSet != null )
			{
				spriteSetTextureActive = component.SpriteSet.Texture ;
			}

			Texture spriteSetTextureChange = EditorGUILayout.ObjectField( "Sprite Set", spriteSetTextureActive, typeof( Texture ), false ) as Texture ;
			if( spriteSetTextureChange != spriteSetTextureActive )
			{
				Undo.RecordObject( component, "[SpriteController] SpriteSet Texture : Change" ) ;	// アンドウバッファに登録

				// SpriteSet 側を設定する
				RefreshSpriteSet( component, spriteSetTextureChange ) ;

				// SpriteAtlas 側を消去する
				component.SpriteAtlas = null ;

				if( component.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					var sprites = component.SpriteSet.GetSprites() ;
					if( sprites != null && sprites.Length >  0 )
					{
						component.Sprite = sprites[ 0 ] ;
					}
				}
				EditorUtility.SetDirty( component ) ;
			}

			if( component.SpriteSet != null )
			{
				spriteSetTextureActive = component.SpriteSet.Texture ;

				if( spriteSetTextureActive != null )
				{
					// サイズ
					EditorGUILayout.BeginHorizontal() ;
					{
						GUILayout.FlexibleSpace() ;
						GUILayout.Label( $"{spriteSetTextureActive.width} x {spriteSetTextureActive.height}" ) ;
					}
					EditorGUILayout.EndHorizontal() ;
				}
			}

			if( spriteSetTextureActive != null )
			{
				//-----------------------------------------------------

				// 一覧から選択出来るようにする

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload Sprites In SpriteSet", GUILayout.Width( 240f ) ) == true || ( component.SpriteSet.IsAvailable == false && component.SpriteSet.Texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					if( component.SpriteSet.IsAvailable == false && component.SpriteSet.Texture != null && Application.isPlaying == false )
					{
						Debug.LogWarning( "SpriteSet に内包される Sprites を自動的に更新:" + component.SpriteSet.Texture.name ) ;
					}

					RefreshSpriteSet( component, component.SpriteSet.Texture ) ;

					EditorUtility.SetDirty( component ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				var spriteNames = component.SpriteSet.GetSpriteNames() ;
				if( spriteNames != null && spriteNames.Length >  0 )
				{
					// ソートする
					var sortedSpriteNames = new List<string>() ;

					int i, l = spriteNames.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						sortedSpriteNames.Add( spriteNames[ i ] ) ;
					}
					sortedSpriteNames.Sort() ;
					spriteNames = sortedSpriteNames.ToArray() ;

					string currentSpriteName = null ;
					if( component.Sprite != null )
					{
						currentSpriteName = component.Sprite.name ;
					}

					int indexBase = -1 ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						if( spriteNames[ i ] == currentSpriteName )
						{
							indexBase = i ;
							break ;
						}
					}

					if( indexBase <  0 )
					{
						var temporarySpriteNames = new List<string>()
						{
							"Unknown"
						} ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							temporarySpriteNames.Add( spriteNames[ i ] ) ;
						}

						spriteNames = temporarySpriteNames.ToArray() ;

						indexBase = 0 ;
					}

					// フレーム番号
					int index = EditorGUILayout.Popup( "Selected Sprite", indexBase, spriteNames ) ;
					if( index != indexBase )
					{
						Undo.RecordObject( component, "SpriteDrawer : Sprite Change" ) ;	// アンドウバッファに登録
						component.SetSpriteInAtlas( spriteNames[ index ] ) ;
						EditorUtility.SetDirty( component ) ;
					}

					// 確認用
					EditorGUILayout.ObjectField( " ", component.Sprite, typeof( Sprite ), false ) ;

					if( component.Sprite != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.Sprite.rect.width} x {component.Sprite.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}
				}
			}
		}

		//---------------

		/// <summary>
		/// エディター専用のスプライトアトラスからオリジナルパーツスプライトのインスタンスを取得する
		/// </summary>
		/// <param name="spriteAtlaa"></param>
		/// <returns></returns>
		private Sprite[] GetSprites( SpriteAtlas spriteAtlas )
		{
			var so = new SerializedObject( spriteAtlas ) ;
			if( so == null )
			{
				return null ;
			}

			//----------------------------------

			var sprites = new List<Sprite>() ;

			// VSの軽度ワーニングが煩わしいので using は使わず Dispose() を使用 
			var property = so.GetIterator() ;
			while( property != null )
			{
				// 有効な参照のみピックアップする
				if
				(
					( property.propertyType						== SerializedPropertyType.ObjectReference	) &&
					( property.objectReferenceValue				!= null										) &&
					( property.objectReferenceInstanceIDValue	!= 0										)
				)
				{
					if( property.propertyPath.IndexOf( "m_PackedSprites.Array.data" ) == 0 && property.type == "PPtr<Sprite>" )
					{
						// オリジナルパーツスプライトへの直接参照を発見した
						sprites.Add( property.objectReferenceValue as Sprite ) ;
					}
				}

				if( property.Next( true ) == false )
				{
					break ;
				}
			}
			so.Dispose() ;

			if( sprites.Count == 0 )
			{
				return null ;
			}

			// ソート
			sprites.Sort( ( a, b ) => string.Compare( a.name, b.name ) ) ;

			return sprites.ToArray() ;
		}

		// スプライトセット情報を更新する
		private void RefreshSpriteSet( SpriteDrawer component, Texture atlasTexture )
		{
			var targetSprites = new List<Sprite>() ;

			if( atlasTexture != null )
			{
				string path = AssetDatabase.GetAssetPath( atlasTexture.GetInstanceID() ) ;

				// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
				var allSprites = AssetDatabase.LoadAllAssetsAtPath( path ) ;

				if( allSprites != null && allSprites.Length >  0 )
				{
					int i, l = allSprites.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( allSprites[ i ] is Sprite )
						{
							targetSprites.Add( allSprites[ i ] as Sprite ) ;
						}
					}
				}

				if( targetSprites.Count >  0 )
				{
					// 存在するので更新する
					component.SpriteSet ??= new SpriteSet() ;

					component.SpriteSet.ClearSprites() ;
					component.SpriteSet.SetSprites( targetSprites.ToArray() ) ;
				}
				else
				{
					// 存在しないのでクリアする
					component.SpriteSet?.ClearSprites() ;
				}

				// 選択中のスプライトは一旦消去する
				component.Sprite = null ;

				// SpriteAtlas 側を消去する
				component.SpriteAtlas = null ;
			}
			else
			{
				component.SpriteSet?.ClearSprites() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 区切り線
		protected void DrawSeparater()
		{
			EditorGUILayout.Space( 8 ) ;	// 少し区切りスペース

			var rect = GUILayoutUtility.GetRect( Screen.width, 2f ) ;

			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 0, rect.width - 0, 1 ), Color.white ) ;
			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 1, rect.width - 0, 1 ), Color.black ) ;

			EditorGUILayout.Space( 8 ) ;	// 少し区切りスペース
		}

	}
}
#endif
