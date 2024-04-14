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
	[ CustomEditor( typeof( SoftMesh3D ) ) ]
	public class SoftMesh3DInspector : Editor
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
			var component = target as SoftMesh3D ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// タイプ
			var shapeType = ( SoftMesh3D.ShapeTypes )EditorGUILayout.EnumPopup( "Shape Type",  component.ShapeType ) ;
			if( component.ShapeType != shapeType )
			{
				Undo.RecordObject( component, "SoftMesh3D : Shape Type Change" ) ;	// アンドウバッファに登録
				component.ShapeType = shapeType ;
				EditorUtility.SetDirty( component ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//----------------------------------

			// オフセット
			var offset3D_Old = new Vector3( component.Offset.x, component.Offset.y, component.Offset.z ) ;
			var offset3D_New = EditorGUILayout.Vector3Field( "Offset", offset3D_Old ) ;
			if( offset3D_New.Equals( offset3D_Old ) == false )
			{
				Undo.RecordObject( component, "SoftMesh3D : Offset Change" ) ;	// アンドウバッファに登録
				component.Offset = new Vector3( offset3D_New.x, offset3D_New.y, offset3D_New.z ) ;
				EditorUtility.SetDirty( component ) ;
			}
	
			// サイズ
			var size3D_Old = new Vector3( component.Size.x, component.Size.y, component.Size.z ) ;
			var size3D_New = EditorGUILayout.Vector3Field( "Size", size3D_Old ) ;
			if( size3D_New.Equals( size3D_Old ) == false )
			{
				Undo.RecordObject( component, "SoftMesh3D : Size Change" ) ;	// アンドウバッファに登録
				component.Size = new Vector3( size3D_New.x, size3D_New.y, size3D_New.z ) ;
				EditorUtility.SetDirty( component ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプリット
			if( component.ShapeType != SoftMesh3D.ShapeTypes.Cube )
			{
				var split = EditorGUILayout.IntSlider( "Split", component.Split, 0, 5 ) ;
				if( component.Split != split )
				{
					Undo.RecordObject( component, "SoftMesh3D : Split Change" ) ;	// アンドウバッファに登録
					component.Split = split ;
					EditorUtility.SetDirty( component ) ;
				}
			}

			// 頂点のカラー
			var basisColor = Color.white ;
			basisColor.r = component.VertexColor.r ;
			basisColor.g = component.VertexColor.g ;
			basisColor.b = component.VertexColor.b ;
			basisColor.a = component.VertexColor.a ;
			basisColor = EditorGUILayout.ColorField( "Vertex Color", basisColor ) ;
			if
			(
				basisColor.r != component.VertexColor.r ||
				basisColor.g != component.VertexColor.g ||
				basisColor.b != component.VertexColor.b ||
				basisColor.a != component.VertexColor.a
			)
			{
				Undo.RecordObject( component, "SoftMesh3D : VertexColor Change" ) ;	// アンドウバッファに登録
				component.VertexColor = basisColor ;
				EditorUtility.SetDirty( component ) ;
			}

			// 軸タイプ
			var directionType = ( SoftMesh3D.DirectionTypes )EditorGUILayout.EnumPopup( "Direction Type",  component.DirectionType ) ;
			if( component.DirectionType != directionType )
			{
				Undo.RecordObject( component, "SoftMesh3D : Direction Type Change" ) ;	// アンドウバッファに登録
				component.DirectionType = directionType ;
				EditorUtility.SetDirty( component ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isDirectionInverse = EditorGUILayout.Toggle( component.IsDirectionInverse, GUILayout.Width( 16f ) ) ;
				if( component.IsDirectionInverse != isDirectionInverse )
				{
					Undo.RecordObject( component, "SoftMesh3D : IsDirectionInverse Change" ) ;	// アンドウバッファに登録
					component.IsDirectionInverse = isDirectionInverse ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "Is Direction Inverse", "上下方向を反転するかどうか" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// テクスチャ
			var texture = EditorGUILayout.ObjectField( "Texture", component.Texture, typeof( Texture ), false ) as Texture ;
			if( component.Texture != texture )
			{
				Undo.RecordObject( component, "SofrMesh3D : Texture Change" ) ;	// アンドウバッファに登録
				component.Texture  = texture ;
				EditorUtility.SetDirty( component ) ;
			}

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

			//-----------------------------------------------------------

			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//-----------------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isSpriteOverwrite = EditorGUILayout.Toggle( component.IsSpriteOverwrite, GUILayout.Width( 16f ) ) ;
				if( component.IsSpriteOverwrite != isSpriteOverwrite )
				{
					Undo.RecordObject( component, "SoftMesh3D : IsSpriteOverwrite Change" ) ;	// アンドウバッファに登録
					component.IsSpriteOverwrite = isSpriteOverwrite ;
					EditorUtility.SetDirty( component ) ;

					// 後で必要かどうか確認する
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Is Sprite Overwrite", "形状ごとの固有のスプライトを表示します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.IsSpriteOverwrite == true )
			{
				EditorGUILayout.Separator() ;   // 少し区切りスペース

				if( component.ShapeType == SoftMesh3D.ShapeTypes.Cube )
				{
					// Cube

					// 1
					var spriteF1 = EditorGUILayout.ObjectField( "Sprite(1)", component.SpriteF1, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF1 != spriteF1 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF1 Change" ) ;	// アンドウバッファに登録
						component.SpriteF1  = spriteF1 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF1 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF1.rect.width} x {component.SpriteF1.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}

					// 2
					var spriteF2 = EditorGUILayout.ObjectField( "Sprite(2)", component.SpriteF2, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF2 != spriteF2 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF2 Change" ) ;	// アンドウバッファに登録
						component.SpriteF2  = spriteF2 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF2 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF2.rect.width} x {component.SpriteF2.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}

					// 3
					var spriteF3 = EditorGUILayout.ObjectField( "Sprite(3)", component.SpriteF3, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF3 != spriteF3 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF3 Change" ) ;	// アンドウバッファに登録
						component.SpriteF3  = spriteF3 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF3 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF3.rect.width} x {component.SpriteF3.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}

					// 4
					var spriteF4 = EditorGUILayout.ObjectField( "Sprite(4)", component.SpriteF4, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF4 != spriteF4 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF4 Change" ) ;	// アンドウバッファに登録
						component.SpriteF4  = spriteF4 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF4 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF4.rect.width} x {component.SpriteF4.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}

					// 5
					var spriteF5 = EditorGUILayout.ObjectField( "Sprite(5)", component.SpriteF5, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF5 != spriteF5 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF5 Change" ) ;	// アンドウバッファに登録
						component.SpriteF5  = spriteF5 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF5 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF5.rect.width} x {component.SpriteF5.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}

					// 6
					var spriteF6 = EditorGUILayout.ObjectField( "Sprite(6)", component.SpriteF6, typeof( Sprite ), false ) as Sprite ;
					if( component.SpriteF6 != spriteF6 )
					{
						Undo.RecordObject( component, "SofrMesh3D : SpriteF6 Change" ) ;	// アンドウバッファに登録
						component.SpriteF6  = spriteF6 ;
						EditorUtility.SetDirty( component ) ;
					}

					if( component.SpriteF6 != null )
					{
						// サイズ
						EditorGUILayout.BeginHorizontal() ;
						{
							GUILayout.FlexibleSpace() ;
							GUILayout.Label( $"{component.SpriteF6.rect.width} x {component.SpriteF6.rect.height}" ) ;
						}
						EditorGUILayout.EndHorizontal() ;
					}
				}
				else
				if( component.ShapeType == SoftMesh3D.ShapeTypes.Sphere || component.ShapeType == SoftMesh3D.ShapeTypes.Capsule || component.ShapeType == SoftMesh3D.ShapeTypes.Cylinder || component.ShapeType == SoftMesh3D.ShapeTypes.Cone )
				{
					if( component.ShapeType == SoftMesh3D.ShapeTypes.Capsule || component.ShapeType == SoftMesh3D.ShapeTypes.Cylinder || component.ShapeType == SoftMesh3D.ShapeTypes.Cone )
					{
						// T
						var spriteT = EditorGUILayout.ObjectField( "Sprite(T)", component.SpriteT, typeof( Sprite ), false ) as Sprite ;
						if( component.SpriteT != spriteT )
						{
							Undo.RecordObject( component, "SofrMesh3D : SpriteT Change" ) ;	// アンドウバッファに登録
							component.SpriteT  = spriteT ;
							EditorUtility.SetDirty( component ) ;
						}

						if( component.SpriteT != null )
						{
							// サイズ
							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.FlexibleSpace() ;
								GUILayout.Label( $"{component.SpriteT.rect.width} x {component.SpriteT.rect.height}" ) ;
							}
							EditorGUILayout.EndHorizontal() ;
						}
					}

					if( component.ShapeType == SoftMesh3D.ShapeTypes.Sphere || component.ShapeType == SoftMesh3D.ShapeTypes.Capsule || component.ShapeType == SoftMesh3D.ShapeTypes.Cylinder )
					{
						// M
						var spriteM = EditorGUILayout.ObjectField( "Sprite(M)", component.SpriteM, typeof( Sprite ), false ) as Sprite ;
						if( component.SpriteM != spriteM )
						{
							Undo.RecordObject( component, "SofrMesh3D : SpriteM Change" ) ;	// アンドウバッファに登録
							component.SpriteM  = spriteM ;
							EditorUtility.SetDirty( component ) ;
						}

						if( component.SpriteM != null )
						{
							// サイズ
							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.FlexibleSpace() ;
								GUILayout.Label( $"{component.SpriteM.rect.width} x {component.SpriteM.rect.height}" ) ;
							}
							EditorGUILayout.EndHorizontal() ;
						}
					}

					if( component.ShapeType == SoftMesh3D.ShapeTypes.Capsule || component.ShapeType == SoftMesh3D.ShapeTypes.Cylinder || component.ShapeType == SoftMesh3D.ShapeTypes.Cone )
					{
						// B
						var spriteB = EditorGUILayout.ObjectField( "Sprite(B)", component.SpriteB, typeof( Sprite ), false ) as Sprite ;
						if( component.SpriteB != spriteB )
						{
							Undo.RecordObject( component, "SofrMesh3D : SpriteB Change" ) ;	// アンドウバッファに登録
							component.SpriteB  = spriteB ;
							EditorUtility.SetDirty( component ) ;
						}

						if( component.SpriteB != null )
						{
							// サイズ
							EditorGUILayout.BeginHorizontal() ;
							{
								GUILayout.FlexibleSpace() ;
								GUILayout.Label( $"{component.SpriteB.rect.width} x {component.SpriteB.rect.height}" ) ;
							}
							EditorGUILayout.EndHorizontal() ;
						}
					}
				}
			}

			//------------------------------------------------------------------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// コライダー
			DrawCollider( component ) ;

			// リジッドボディ
			DrawRigidbody( component ) ;

			// アニメーター
			DrawAnimator( component ) ;
		}

		//-------------------------------------------------------------------------------------------
		// Material

		protected void DrawMaterial( SoftMesh3D component )
		{
			// マテリアル
			var material = EditorGUILayout.ObjectField( "Maretial", component.Material, typeof( Material ), false ) as Material ;
			if( material != component.Material )
			{
				Undo.RecordObject( component, "SoftMesh3D : Material Change" ) ;	// アンドウバッファに登録
				component.Material = material ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Color

		protected void DrawColor( SoftMesh3D component )
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
				Undo.RecordObject( component, "SoftMesh3D : Color Change" ) ;	// アンドウバッファに登録
				component.Color = color ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-----------------------------------------------------------
		// Sprite

		protected void DrawSprite( SoftMesh3D component )
		{
			var sprite = EditorGUILayout.ObjectField( "Sprite", component.Sprite, typeof( Sprite ), false ) as Sprite ;
			if( component.Sprite != sprite )
			{
				Undo.RecordObject( component, "SofrMesh3D : Sprite Change" ) ;	// アンドウバッファに登録
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
		protected void DrawAtlas( SoftMesh3D component )
		{
			// スプライトアトラス
			SpriteAtlas spriteAtlas = EditorGUILayout.ObjectField( new GUIContent( "Sprite Atlas", "<color=#00FFFF>SpriteAtlas</color>アセットを設定します\nランタイム実行中、<color=#00FFFF>SetSpriteInAtlas</color>メソッドを使用する事により\n表示する<color=#00FFFF>Spriteを動的に切り替える</color>事が出来ます" ), component.SpriteAtlas, typeof( SpriteAtlas ), false ) as SpriteAtlas ;
			if( spriteAtlas != component.SpriteAtlas )
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
		private void RefreshSpriteSet( SoftMesh3D component, Texture atlasTexture )
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
					component.SpriteSet.ClearSprites() ;
					component.SpriteSet.SetSprites( targetSprites.ToArray() ) ;
				}
				else
				{
					// 存在しないのでクリアする
					component.SpriteSet?.ClearSprites() ;
				}

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

		protected void DrawCollider( SoftMesh3D component )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCollider = EditorGUILayout.Toggle( component.IsCollider, GUILayout.Width( 16f ) ) ;
				if( component.IsCollider != isCollider )
				{
					Undo.RecordObject( component, "SoftMesh3D : Collider Change" ) ;	// アンドウバッファに登録
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
						Undo.RecordObject( component, "SoftMesh3D : Collider Adjustment Change" ) ;	// アンドウバッファに登録
						component.ColliderAdjustment = colliderAdjustment ;
						EditorUtility.SetDirty( component ) ;
					}
					GUILayout.Label( new GUIContent( "Collider Adjustment", "コライダーのサイズをメッシュのサイズに自動的に合わせるかどうか" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------
		// Rigidbody

		// リジッドボディの生成破棄チェックボックスを描画する
		protected void DrawRigidbody( SoftMesh3D component )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isRigidbody = EditorGUILayout.Toggle( component.IsRigidbody, GUILayout.Width( 16f ) ) ;
				if( component.IsRigidbody != isRigidbody )
				{
					Undo.RecordObject( component, "SoftMesh3D : Rigidbody Change" ) ;	// アンドウバッファに登録
					component.IsAnimator = isRigidbody ;
					EditorUtility.SetDirty( component ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Rigidbody", "<color=#00FFFF>Rigidbody</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		//--------------------------------------------------------------------------
		// Animator

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( SoftMesh3D component )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( component.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( component.IsAnimator != isAnimator )
				{
					Undo.RecordObject( component, "SoftMesh3D : Animator Change" ) ;	// アンドウバッファに登録
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
