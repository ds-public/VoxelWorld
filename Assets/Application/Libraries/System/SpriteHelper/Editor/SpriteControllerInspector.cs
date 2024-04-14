#if UNITY_EDITOR

using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[ CustomEditor( typeof( SpriteController ), true ) ]
	public class SpriteControllerInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// ターゲットのインスタンス
			var component = target as SpriteController ;

			//----------------------------------

			DrawSize( component ) ;

			DrawAtlas( component ) ;

			DrawCollider( component ) ;

			DrawTween( component ) ;

			DrawFlipper( component ) ;

			DrawAnimator( component ) ;
		}


		//-------------------------------------------------------------------------------------------
		// Size

		protected void DrawSize( SpriteController component )
		{
			var size = EditorGUILayout.Vector2Field( "Size", component.Size ) ;
			if( size != component.Size )
			{
				Undo.RecordObject( component, "[SpriteController] Size : Change" ) ;	// アンドウバッファに登録
				component.Size = size ;
				EditorUtility.SetDirty( component ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( SpriteController component )
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
						Undo.RecordObject( component, "[SpriteController] Sprite : Change" ) ;	// アンドウバッファに登録
						component.SetSpriteInAtlas( spriteNames[ index ] ) ;
						EditorUtility.SetDirty( component ) ;
					}

					// 確認用
					EditorGUILayout.ObjectField( " ", component.Sprite, typeof( Sprite ), false ) ;
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
		private void RefreshSpriteSet( SpriteController component, Texture atlasTexture )
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

		//-------------------------------------------------------------------------------------------
		// Collider

		private int		m_ColliderIndex			= 0 ;
		private bool	m_ColliderRemoveAready	= false ;

		protected void DrawCollider( SpriteController component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			var colliderTypeNames = new string[]
			{
				"None",
				"BoxCollider2D",
				"CircleCollider2D",
				"CapsuleCollider2D",
				"PolygonCollider2D",
				"EdgeCollider2D",
				"CompositeCollider2D",
				"CustomCollider2D",
			} ;

			var collider = component.CCollider ;

			if( collider == null )
			{
				// コライダーは無し

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUILayout.Label( new GUIContent( "Collider2D", "<color=#00FFFF>Collider2D</color>コンポーネントの追加または削除を行います" ), GUILayout.Width( 80f ) ) ;

					m_ColliderIndex = EditorGUILayout.Popup( "", m_ColliderIndex, colliderTypeNames, GUILayout.Width( 160f ) ) ;	// フィールド名有りタイプ

					if( m_ColliderIndex >  0 )
					{
						bool isAdd = false ;

						GUI.backgroundColor = Color.cyan ;
						if( GUILayout.Button( new GUIContent( "Add", "<color=#00FFFF>Collider</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ), GUILayout.Width( 60f ) ) == true )
						{
							isAdd = true ;
						}
						GUI.backgroundColor = Color.white ;

						if( isAdd == true )
						{
							// Collider を追加する

							switch( m_ColliderIndex )
							{
								case 1 : component.AddCollider<BoxCollider2D>()			; break ;
								case 2 : component.AddCollider<CircleCollider2D>()		; break ;
								case 3 : component.AddCollider<CapsuleCollider2D>()		; break ;
								case 4 : component.AddCollider<PolygonCollider2D>()		; break ;
								case 5 : component.AddCollider<EdgeCollider2D>()		; break ;
								case 6 : component.AddCollider<CompositeCollider2D>()	; break ;
								case 7 : component.AddCollider<CustomCollider2D>()		; break ;
							}
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				// コライダーは有り

				if( m_ColliderRemoveAready == false )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						GUILayout.Label( new GUIContent( "Collider2D", "<color=#00FFFF>Collider2D</color>コンポーネントの追加または削除を行います" ), GUILayout.Width( 80f ) ) ;

						if( collider is BoxCollider2D		){ m_ColliderIndex = 1 ; }
						if( collider is CircleCollider2D	){ m_ColliderIndex = 2 ; }
						if( collider is CapsuleCollider2D	){ m_ColliderIndex = 3 ; }
						if( collider is PolygonCollider2D	){ m_ColliderIndex = 4 ; }
						if( collider is EdgeCollider2D		){ m_ColliderIndex = 5 ; }
						if( collider is CompositeCollider2D	){ m_ColliderIndex = 6 ; }
						if( collider is CustomCollider2D	){ m_ColliderIndex = 7 ; }

						EditorGUILayout.TextField( "", colliderTypeNames[ m_ColliderIndex ], GUILayout.Width( 160f ) ) ;

						bool isRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( new GUIContent( "Remove", "<color=#00FFFF>Collider</color>コンポーネントを\nこの<color=#00FF00>GameObjectから削除</color>します" ), GUILayout.Width( 60f ) ) == true )
						{
							isRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( isRemove == true )
						{
							// 削除確認へ
							m_ColliderRemoveAready = true ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
				else
				{
					// 実際の破棄の確認と実行
					var message = GetMessage( "RemoveColliderOK?" ).Replace( "%1", colliderTypeNames[ m_ColliderIndex ] ) ;
					GUILayout.Label( message ) ;

					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						GUI.backgroundColor = Color.red ;
						if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
						{
							// 本当に削除する
							Undo.RecordObject( component, $"[SpriteController] {colliderTypeNames[ m_ColliderIndex ]} Remove" ) ;	// アンドウバッファに登録
							component.RemoveCollider() ;
							EditorUtility.SetDirty( component ) ;
							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

							m_ColliderRemoveAready = false ;
						}
						GUI.backgroundColor = Color.white ;
						if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
						{
							m_ColliderRemoveAready = false ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
		}


		//-------------------------------------------------------------------------------------------
		// Tween

		// Tween の追加と削除
		private string m_AddTweenIdentity = "" ;
		private int    m_RemoveTweenIndex = 0 ;
		private int    m_RemoveTweenIndexAnswer = -1 ;

		protected void DrawTween( SpriteController component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			var tweens = component.GetComponents<SpriteTween>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tweens.Length, j, c ;
			string identity ;
			var tweenIdentities = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tweenIdentities[ i ] = tweens[ i ].Identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				identity = tweenIdentities[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( tweenIdentities[ j ] == identity )
					{
						// 同じ名前を発見した
						c ++ ;
						tweenIdentities[ j ] = tweenIdentities[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( m_RemoveTweenIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool isAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( new GUIContent( "Add Tween", "<color=#00FFFF>UITween</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n<color=#00FFFF>PlayTween</color>メソッドにより、このビューのトゥイーンアニメーションを行う事が出来ます" ), GUILayout.Width( 140f ) ) == true )
					{
						isAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					m_AddTweenIdentity = EditorGUILayout.TextField( "", m_AddTweenIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( isAdd == true )
					{
						if( string.IsNullOrEmpty( m_AddTweenIdentity ) == false )
						{
							// Tween を追加する
							var tween = component.AddComponent<SpriteTween>() ;
							tween.Identity = m_AddTweenIdentity ;

							// 追加後の全ての Tween を取得する
							var temporaryTweens = component.gameObject.GetComponents<SpriteTween>() ;
							if( temporaryTweens != null && temporaryTweens.Length >  0 )
							{
								for( i  = 0 ; i <  temporaryTweens.Length ; i ++ )
								{
									if( temporaryTweens[ i ] != tween )
									{
										break ;
									}
								}
								if( i <  temporaryTweens.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									tween.enabled = false ;
									tween.PlayOnAwake = false ;
								}
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Add Tween", GetMessage( "InputIdentity" ), "Close" ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tweens != null && tweens.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool isRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Tween", GUILayout.Width( 140f ) ) == true )
						{
							isRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( m_RemoveTweenIndex >= tweenIdentities.Length )
						{
							m_RemoveTweenIndex  = tweenIdentities.Length - 1 ;
						}
						m_RemoveTweenIndex = EditorGUILayout.Popup( "", m_RemoveTweenIndex, tweenIdentities, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ

						if( isRemove == true )
						{
							// 削除する
							m_RemoveTweenIndexAnswer = m_RemoveTweenIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				var message = GetMessage( "RemoveTweenOK?" ).Replace( "%1", tweenIdentities[ m_RemoveTweenIndexAnswer ] ) ;
				GUILayout.Label( message ) ;
	//			GUILayout.Label( "It does really may be to remove tween '" + tTweenIdentityArray[ mRemoveTweenIndexAnswer ] + "' ?" ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( component, "[SpriteController] Tween Remove" ) ;	// アンドウバッファに登録
						component.RemoveTweenIdentity = tweens[ m_RemoveTweenIndexAnswer ].Identity ;
						component.RemoveTweenInstance = tweens[ m_RemoveTweenIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( component ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						m_RemoveTweenIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						m_RemoveTweenIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//-----------------------------------------------------------
		// Flipper

		// Filipper の追加と削除
		private string m_AddFlipperIdentity = "" ;
		private int    m_RemoveFlipperIndex = 0 ;
		private int    m_RemoveFlipperIndexAnswer = -1 ;

		protected void DrawFlipper( SpriteController component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			var flippers = component.GetComponents<SpriteFlipper>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = flippers.Length, j, c ;
			string identity ;
			var flipperIdentities = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				flipperIdentities[ i ] = flippers[ i ].Identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				identity = flipperIdentities[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( flipperIdentities[ j ] == identity )
					{
						// 同じ名前を発見した
						c ++ ;
						flipperIdentities[ j ] = flipperIdentities[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( m_RemoveFlipperIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool isAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Flipper", GUILayout.Width( 140f ) ) == true )
					{
						isAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					m_AddFlipperIdentity = EditorGUILayout.TextField( "", m_AddFlipperIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( isAdd == true )
					{
						if( string.IsNullOrEmpty( m_AddFlipperIdentity ) == false )
						{
							// Flipper を追加する
							var flipper = component.AddComponent<SpriteFlipper>() ;
							flipper.Identity = m_AddFlipperIdentity ;

							var existingFlippers = component.gameObject.GetComponents<SpriteFlipper>() ;
							if( existingFlippers != null && existingFlippers.Length >  0 )
							{
								for( i  = 0 ; i <  existingFlippers.Length ; i ++ )
								{
									if( existingFlippers[ i ] != flipper )
									{
										break ;
									}
								}
								if( i <  existingFlippers.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									flipper.enabled = false ;
									flipper.PlayOnAwake = false ;
								}
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Add Flipper", GetMessage( "InputIdentity" ), "Close" ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( flippers != null && flippers.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool isRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Flipper", GUILayout.Width( 140f ) ) == true )
						{
							isRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( m_RemoveFlipperIndex >= flipperIdentities.Length )
						{
							m_RemoveFlipperIndex  = flipperIdentities.Length - 1 ;
						}
						m_RemoveFlipperIndex = EditorGUILayout.Popup( "", m_RemoveFlipperIndex, flipperIdentities, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ

						if( isRemove == true )
						{
							// 削除する
							m_RemoveFlipperIndexAnswer = m_RemoveFlipperIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				var message = GetMessage( "RemoveFlipperOK?" ).Replace( "%1", flipperIdentities[ m_RemoveFlipperIndexAnswer ] ) ;
				GUILayout.Label( message ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( component, "[SpriteConroller] Flipper Remove" ) ;	// アンドウバッファに登録
						component.RemoveFlipperIdentity = flippers[ m_RemoveFlipperIndexAnswer ].Identity ;
						component.RemoveFlipperInstance = flippers[ m_RemoveFlipperIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( component ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						m_RemoveFlipperIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						m_RemoveFlipperIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}


		//--------------------------------------------------------------------------

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( SpriteController controller )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( controller.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( isAnimator != controller.IsAnimator )
				{
					Undo.RecordObject( controller, "[SpriteController] Animator Change" ) ;	// アンドウバッファに登録
					controller.IsAnimator = isAnimator ;
					EditorUtility.SetDirty( controller ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Animator", "<color=#00FFFF>Animator</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n<color=#00FFFF>PlayAnimator</color>メソッドを実行する際に必要になります" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "RemoveTweenOK?",		"Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?",	"Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone",	"EventTrigger クラスが必要です" },
			{ "InputIdentity",		"識別子を入力してください" },

			{ "RemoveColliderOK?",	"[ %1 ] を削除してもよろしいですか？" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "RemoveTweenOK?",		"It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?",	"It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone",	"'EventTrigger' is necessary." },
			{ "InputIdentity",		"Input identity !" },

			{ "RemoveColliderOK?",   "It does really may be to remove %1 ?" },
		} ;

		private static string GetMessage( string label )
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

#endif
