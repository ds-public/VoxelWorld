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
	[CustomEditor( typeof( SpriteMapDrawer ) )]
	public class SpriteMapDrawerInspector : SpriteTransformInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			var component = target as SpriteMapDrawer ;

			// トランスフォーム部分を描画する
			DrawTransform( component, true ) ;

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

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// Grid
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Grid", "矩形の分割量です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				var gridX = EditorGUILayout.IntField( component.GridX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.GridX != gridX )
				{
					Undo.RecordObject( component, "SpriteDrawer : Grid X Change" ) ;	// アンドウバッファに登録
					component.GridX = gridX ;
					EditorUtility.SetDirty( component ) ;
				}

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				var gridY = EditorGUILayout.IntField( component.GridY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.GridY != gridY )
				{
					Undo.RecordObject( component, "SpriteDrawer : Grid Y Change" ) ;	// アンドウバッファに登録
					component.GridY = gridY ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//-----------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			var cells = serializedObject.FindProperty( "m_Cells" ) ;
			EditorGUILayout.PropertyField( cells ) ;

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

			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}

		//-------------------------------------------------------------------------------------------
		// Material

		protected void DrawMaterial( SpriteMapDrawer component )
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

		protected void DrawMaterialColor( SpriteMapDrawer component )
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

		protected void DrawSprite( SpriteMapDrawer component )
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
		protected void DrawAtlas( SpriteMapDrawer component )
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
		private void RefreshSpriteSet( SpriteMapDrawer component, Texture atlasTexture )
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


		// 個々セル情報表示
		[CustomPropertyDrawer( typeof( SpriteMapDrawer.Cell ), true )]
		public class CellDrawer : PropertyDrawer
		{
			private float LineHeight { get { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing ; } }

			/// <summary>
			/// プロパティの高さを取得する(カスタムによって高さが変わるなら必須)
			/// </summary>
			/// <param name="property"></param>
			/// <param name="label"></param>
			/// <returns></returns>
			public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
			{
				return base.GetPropertyHeight( property, label ) + EditorGUIUtility.standardVerticalSpacing + LineHeight * 3 + EditorGUIUtility.standardVerticalSpacing ;
			}


			/// <summary>
			/// 指定された矩形内のプロパティを描画する
			/// </summary>
			/// <param name="position"></param>
			/// <param name="property"></param>
			/// <param name="label"></param>
			public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
			{
				EditorGUI.BeginProperty( position, label, property ) ;

				// ラベルを描画
				position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label ) ;

				// 子のフィールドをインデントしない 
//				var indent = EditorGUI.indentLevel ;
//				EditorGUI.indentLevel = 0 ;

				float x = position.x ;
				float y = position.y ;
				float w = position.width ;

				var spriteMapDrawer = property.serializedObject.targetObject as SpriteMapDrawer ;

				//---------------------------------
				// Index

				var indexProperty = property.FindPropertyRelative( "Index" ) ;

				var indexRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string indexLabel = "Index" ; // animationNameProperty.displayName ;
				var index = EditorGUI.IntField( indexRect, indexLabel, indexProperty.intValue ) ;
				if( indexProperty.intValue != index )
				{
					Undo.RecordObject( spriteMapDrawer, "[SpriteMapDrawer] Cell Index : Change" ) ;	// アンドウバッファに登録
					indexProperty.intValue = index ;
					EditorUtility.SetDirty( spriteMapDrawer ) ;
				}

				y += LineHeight ;

				//---------------------------------
				// Color

				var colorProperty = property.FindPropertyRelative( "Color" ) ;

				var colorRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string colorLabel = "Color" ; // animationNameProperty.displayName ;
				var colorOld = colorProperty.colorValue ;
				var colorNew = colorProperty.colorValue ;
				colorNew = EditorGUI.ColorField( colorRect, colorLabel, colorNew ) ;
				if( colorNew.Equals( colorOld ) == false )
				{
					Undo.RecordObject( spriteMapDrawer, "[SpriteMapDrawer] Cell Color : Change" ) ;	// アンドウバッファに登録
					colorProperty.colorValue = colorNew ;
					EditorUtility.SetDirty( spriteMapDrawer ) ;
				}

				y += LineHeight ;

				//---------------------------------
				// FlipX

				var flipXProperty = property.FindPropertyRelative( "FlipX" ) ;

				var flipXRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string flipXLabel = "FlipX" ; // animationNameProperty.displayName ;
				var flipX = EditorGUI.Toggle( flipXRect, flipXLabel, flipXProperty.boolValue ) ;
				if( flipXProperty.boolValue != flipX )
				{
					Undo.RecordObject( spriteMapDrawer, "[SpriteMapDrawer] Cell FlipX : Change" ) ;	// アンドウバッファに登録
					flipXProperty.boolValue = flipX ;
					EditorUtility.SetDirty( spriteMapDrawer ) ;
				}

				y += LineHeight ;

				//---------------------------------
				// FlipY

				var flipYProperty = property.FindPropertyRelative( "FlipY" ) ;

				var flipYRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string flipYLabel = "FlipY" ; // animationNameProperty.displayName ;
				var flipY = EditorGUI.Toggle( flipYRect, flipYLabel, flipYProperty.boolValue ) ;
				if( flipYProperty.boolValue != flipY )
				{
					Undo.RecordObject( spriteMapDrawer, "[SpriteMapDrawer] Cell FlipY : Change" ) ;	// アンドウバッファに登録
					flipYProperty.boolValue = flipY ;
					EditorUtility.SetDirty( spriteMapDrawer ) ;
				}

				y += LineHeight ;

				//---------------------------------------------------------

				// インデントを元通りに戻します
//				EditorGUI.indentLevel = indent ;

				EditorGUI.EndProperty() ;
			}
		}


	}
}
#endif
