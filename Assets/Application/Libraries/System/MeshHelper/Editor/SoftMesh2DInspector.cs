#if UNITY_EDITOR

using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace MeshHelper
{
	[ CustomEditor( typeof( SoftMesh2D ) ) ]
	public class SoftMesh2DInspector : Editor
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
			var component = target as SoftMesh2D ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// タイプ
			var shapeType = ( SoftMesh2D.ShapeTypes )EditorGUILayout.EnumPopup( "Shape Type",  component.ShapeType ) ;
			if( shapeType != component.ShapeType )
			{
				Undo.RecordObject( component, "SoftMesh2D : Shape Type Change" ) ;	// アンドウバッファに登録
				component.ShapeType = shapeType ;
				EditorUtility.SetDirty( component ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//----------------------------------

			// オフセット
			var offset2D_Old = new Vector2( component.Offset.x, component.Offset.y ) ;
			var offset2D_New = EditorGUILayout.Vector2Field( "Offset", offset2D_Old ) ;
			if( offset2D_New.Equals( offset2D_Old ) == false )
			{
				Undo.RecordObject( component, "SoftMesh2D : Offset Change" ) ;	// アンドウバッファに登録
				component.Offset = new Vector2( offset2D_New.x, offset2D_New.y ) ;
				EditorUtility.SetDirty( component ) ;
			}
	
			// サイズ
			var size2D_Old = new Vector2( component.Size.x, component.Size.y ) ;
			var size2D_New = EditorGUILayout.Vector2Field( "Size", size2D_Old ) ;
			if( size2D_New.Equals( size2D_Old ) == false )
			{
				Undo.RecordObject( component, "SoftMesh2D : Size Change" ) ;	// アンドウバッファに登録
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
					Undo.RecordObject( component, "SoftMesh2D : FlipX Change" ) ;	// アンドウバッファに登録
					component.FlipX = flipX ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "FlipX", "横方向を反転表示します" ), GUILayout.Width( 64f ) ) ;

				bool flipY = EditorGUILayout.Toggle( component.FlipY, GUILayout.Width( 16f ) ) ;
				if( component.FlipY != flipY )
				{
					Undo.RecordObject( component, "SoftMesh2D : FlipY Change" ) ;	// アンドウバッファに登録
					component.FlipY = flipY ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "FlipY", "縦方向を反転表示します" ), GUILayout.Width( 64f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			// 線の太さ(０で塗りつぶし)
			float lineWidth = EditorGUILayout.Slider( "LineWidth", component.LineWidth,  0, 16 ) ;
			if( component.LineWidth != lineWidth )
			{
				Undo.RecordObject( component, "SoftMesh2D : LineWidth Change" ) ;	// アンドウバッファに登録
				component.LineWidth = lineWidth ;
				EditorUtility.SetDirty( component ) ;
			}

			if( component.LineWidth <= 0 )
			{
				if( component.ShapeType == SoftMesh2D.ShapeTypes.Rectangle )
				{
					if( component.ImageType == SoftMesh2D.ImageTypes.Simple || component.Tiling == true )
					{
						// 分割数(四角専用)
						int gridX = EditorGUILayout.IntField( "GridX", component.GridX ) ;
						if( component.GridX != gridX )
						{
							Undo.RecordObject( component, "SoftMesh2D : GridX Change" ) ;	// アンドウバッファに登録
							component.GridX = gridX ;
							EditorUtility.SetDirty( component ) ;
						}

						int gridY = EditorGUILayout.IntField( "GridY", component.GridY ) ;
						if( component.GridY != gridY )
						{
							Undo.RecordObject( component, "SoftMesh2D : GridY Change" ) ;	// アンドウバッファに登録
							component.GridY = gridY ;
							EditorUtility.SetDirty( component ) ;
						}
					}
				}
			}

			if( component.ShapeType == SoftMesh2D.ShapeTypes.Circle )
			{
				// 分割数(円専用)
				int split = EditorGUILayout.IntSlider( "Split", component.Split, 0, 5 ) ;
				if( component.Split != split )
				{
					Undo.RecordObject( component, "SoftMesh2D : Split Change" ) ;	// アンドウバッファに登録
					component.Split = split ;
					EditorUtility.SetDirty( component ) ;
				}
			}

			// 基本のカラー
			var basisColor = Color.white ;
			basisColor.r = component.BasisColor.r ;
			basisColor.g = component.BasisColor.g ;
			basisColor.b = component.BasisColor.b ;
			basisColor.a = component.BasisColor.a ;
			basisColor = EditorGUILayout.ColorField( "Basis Color (Vertex)", basisColor ) ;
			if
			(
				basisColor.r != component.BasisColor.r ||
				basisColor.g != component.BasisColor.g ||
				basisColor.b != component.BasisColor.b ||
				basisColor.a != component.BasisColor.a
			)
			{
				Undo.RecordObject( component, "SoftMesh2D : BasisColor Change" ) ;	// アンドウバッファに登録
				component.BasisColor = basisColor ;
				EditorUtility.SetDirty( component ) ;
			}

			// 内側のカラー
			var innerColor = Color.white ;
			innerColor.r = component.InnerColor.r ;
			innerColor.g = component.InnerColor.g ;
			innerColor.b = component.InnerColor.b ;
			innerColor.a = component.InnerColor.a ;
			innerColor = EditorGUILayout.ColorField( "Inner Color (Vertex)", innerColor ) ;
			if
			(
				innerColor.r != component.InnerColor.r ||
				innerColor.g != component.InnerColor.g ||
				innerColor.b != component.InnerColor.b ||
				innerColor.a != component.InnerColor.a
			)
			{
				Undo.RecordObject( component, "SoftMesh2D : InnerColor Change" ) ;	// アンドウバッファに登録
				component.InnerColor = innerColor ;
				EditorUtility.SetDirty( component ) ;
			}

			// 外側のカラー
			var outerColor = Color.white ;
			outerColor.r = component.OuterColor.r ;
			outerColor.g = component.OuterColor.g ;
			outerColor.b = component.OuterColor.b ;
			outerColor.a = component.OuterColor.a ;
			outerColor = EditorGUILayout.ColorField( "Outer Color (Vertex)", outerColor ) ;
			if
			(
				outerColor.r != component.OuterColor.r ||
				outerColor.g != component.OuterColor.g ||
				outerColor.b != component.OuterColor.b ||
				outerColor.a != component.OuterColor.a
			)
			{
				Undo.RecordObject( component, "SoftMesh2D : OuterColor Change" ) ;	// アンドウバッファに登録
				component.OuterColor = outerColor ;
				EditorUtility.SetDirty( component ) ;
			}

			// 四角限定機能
			if( component.ShapeType == SoftMesh2D.ShapeTypes.Rectangle && component.LineWidth == 0 )
			{
				// 表示タイプ
				var imageType = ( SoftMesh2D.ImageTypes )EditorGUILayout.EnumPopup( "Image Type",  component.ImageType ) ;
				if( component.ImageType != imageType )
				{
					Undo.RecordObject( component, "SoftMesh2D : Image Type Change" ) ;	// アンドウバッファに登録
					component.ImageType  = imageType ;
					EditorUtility.SetDirty( component ) ;
				}

				// 表示タイプがスライス限定のプロパティ
				if( component.ImageType == SoftMesh2D.ImageTypes.Sliced )
				{
					// ボーダーサイズ
					var borderSize_Old = new Vector2( component.BorderSize.x, component.BorderSize.y ) ;
					var borderSize_New = EditorGUILayout.Vector2Field( "Border Size", borderSize_Old ) ;
					if( borderSize_New.Equals( borderSize_Old ) == false )
					{
						Undo.RecordObject( component, "SoftMesh2D : Border Size Change" ) ;	// アンドウバッファに登録
						component.BorderSize = new Vector2( borderSize_New.x, borderSize_New.y ) ;
						EditorUtility.SetDirty( component ) ;
					}

					// センター表示
					var fillCenter = EditorGUILayout.Toggle( "Fill Center", component.FillCenter ) ;
					if( component.FillCenter != fillCenter )
					{
						Undo.RecordObject( component, "SoftMesh2D : Fill Center Change" ) ;	// アンドウバッファに登録
						component.FillCenter  = fillCenter ;
						EditorUtility.SetDirty( component ) ;
					}
				}

				// タイリング
				var tiling = EditorGUILayout.Toggle( "Tiling", component.Tiling ) ;
				if( component.Tiling != tiling )
				{
					Undo.RecordObject( component, "SoftMesh2D : Tiling Change" ) ;	// アンドウバッファに登録
					component.Tiling  = tiling ;
					EditorUtility.SetDirty( component ) ;
				}
			}

			// 円限定機能
			if( component.ShapeType == SoftMesh2D.ShapeTypes.Circle && component.LineWidth == 0 )
			{
				// 効果タイプ
				var decalType = ( SoftMesh2D.DecalTypes )EditorGUILayout.EnumPopup( "Decal Type",  component.DecalType ) ;
				if( component.DecalType != decalType )
				{
					Undo.RecordObject( component, "SoftMesh2D : Decal Type Change" ) ;	// アンドウバッファに登録
					component.DecalType  = decalType ;
					EditorUtility.SetDirty( component ) ;
				}
			}

			//----------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 軸タイプ
			var directionType = ( SoftMesh2D.DirectionTypes )EditorGUILayout.EnumPopup( "Direction Type",  component.DirectionType ) ;
			if( component.DirectionType != directionType )
			{
				Undo.RecordObject( component, "SoftMesh2D : Direction Type Change" ) ;	// アンドウバッファに登録
				component.DirectionType = directionType ;
				EditorUtility.SetDirty( component ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isDirectionInverse = EditorGUILayout.Toggle( component.IsDirectionInverse, GUILayout.Width( 16f ) ) ;
				if( component.IsDirectionInverse != isDirectionInverse )
				{
					Undo.RecordObject( component, "SoftMesh2D : IsDirectionInverse Change" ) ;	// アンドウバッファに登録
					component.IsDirectionInverse = isDirectionInverse ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "Is Direction Inverse", "前方方向を反転するかどうか" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			// テクスチャ
			var texture = EditorGUILayout.ObjectField( "Texture", component.Texture, typeof( Texture ), false ) as Texture ;
			if( component.Texture != texture )
			{
				Undo.RecordObject( component, "SofrMesh2D : Texture Change" ) ;	// アンドウバッファに登録
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
			DrawColor( component ) ;

			// スプライト
			DrawSprite( component ) ;

			//----------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// アトラス関連
			DrawAtlas( component ) ;

			//------------------------------------------------------------------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// コライダー
			DrawCollider( component ) ;

			// アニメーター
			DrawAnimator( component ) ;
		}

		//-------------------------------------------------------------------------------------------
		// Material

		protected void DrawMaterial( SoftMesh2D component )
		{
			// マテリアル
			var material = EditorGUILayout.ObjectField( "Maretial", component.Material, typeof( Material ), false ) as Material ;
			if( material != component.Material )
			{
				Undo.RecordObject( component, "SoftMesh2D : Material Change" ) ;	// アンドウバッファに登録
				component.Material = material ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Color

		protected void DrawColor( SoftMesh2D component )
		{
			// カラー
			var color = Color.white ;
			color.r = component.Color.r ;
			color.g = component.Color.g ;
			color.b = component.Color.b ;
			color.a = component.Color.a ;
			color = EditorGUILayout.ColorField( "Color", color ) ;
			if
			(
				color.r != component.Color.r ||
				color.g != component.Color.g ||
				color.b != component.Color.b ||
				color.a != component.Color.a
			)
			{
				Undo.RecordObject( component, "SoftMesh2D : Color Change" ) ;	// アンドウバッファに登録
				component.Color = color ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-----------------------------------------------------------
		// Sprite

		protected void DrawSprite( SoftMesh2D component )
		{
			var sprite = EditorGUILayout.ObjectField( "Sprite", component.Sprite, typeof( Sprite ), false ) as Sprite ;
			if( component.Sprite != sprite )
			{
				Undo.RecordObject( component, "SofrMesh2D : Sprite Change" ) ;	// アンドウバッファに登録
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
		protected void DrawAtlas( SoftMesh2D component )
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
						Undo.RecordObject( component, "SoftMesh2D : Sprite Change" ) ;	// アンドウバッファに登録
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
		private void RefreshSpriteSet( SoftMesh2D component, Texture atlasTexture )
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

		//--------------------------------------------------------------------------
		// Collider

		protected void DrawCollider( SoftMesh2D component )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCollider = EditorGUILayout.Toggle( component.IsCollider, GUILayout.Width( 16f ) ) ;
				if( component.IsCollider != isCollider )
				{
					Undo.RecordObject( component, "SoftMesh2D : Collider Change" ) ;	// アンドウバッファに登録
					component.IsCollider = isCollider ;
					EditorUtility.SetDirty( component ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Collider", "<color=#00FFFF>Collider</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.IsCollider == true )
			{
				// コライダーの自動調整
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( " ", GUILayout.Width( 16f ) ) ;
					bool colliderAdjustment = EditorGUILayout.Toggle( component.ColliderAdjustment, GUILayout.Width( 16f ) ) ;
					if( colliderAdjustment != component.ColliderAdjustment )
					{
						Undo.RecordObject( component, "SoftMesh2D : Collider Adjustment Change" ) ;	// アンドウバッファに登録
						component.ColliderAdjustment = colliderAdjustment ;
						EditorUtility.SetDirty( component ) ;
					}
					GUILayout.Label( new GUIContent( "Collider Adjustment", "コライダーのサイズをメッシュのサイズに自動的に合わせるかどうか" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------
		// Animator

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( SoftMesh2D component )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( component.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( component.IsAnimator !=	isAnimator )
				{
					Undo.RecordObject( component, "SoftMesh2D : Animator Change" ) ;	// アンドウバッファに登録
					component.IsAnimator = isAnimator ;
					EditorUtility.SetDirty( component ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Animator", "<color=#00FFFF>Animator</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n<color=#00FFFF>PlayAnimator</color>メソッドを実行する際に必要になります" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
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
