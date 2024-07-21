#if UNITY_EDITOR

using System.Collections.Generic ;
using System.Linq ;
using System.Text.RegularExpressions ;
using System.Reflection ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	public static class SerializedPropertyExtensions
	{
		/// <summary>
		/// リストの要素Indexを返す
		/// </summary>
		public static int GetArrayElementIndex( this SerializedProperty property )
		{
			// プロパティがリストのインデックスであれば、パスは(変数名).Array.data[(インデックス)] 
			// となるため、この文字列からインデックスを取得する

			// リストの要素であるか判定する
			var match = Regex.Match( property.propertyPath, "^([a-zA-Z0-9_]*).Array.data\\[([0-9]*)\\]$" ) ;
			if( match.Success == false )
			{
				return -1 ;
			}

			// Indexを抜き出す
			var splitPath = property.propertyPath.Split( '.' ) ;
			var regax = new Regex( @"[^0-9]" ) ;
			if( int.TryParse( regax.Replace( splitPath[ ^1 ], "" ), out var index ) == false )
			{
				return -1 ;
			}

			return index ;
		}
	}


	[ CustomEditor( typeof( SpriteImage ), true ) ]
	public class SpriteImageInspector : Editor
	{

		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// ボールド
//			var boldStyle = new GUIStyle( GUI.skin.label )
//			{
//				fontStyle = FontStyle.Bold
//			} ;

			if( target.GetType() != typeof( SpriteImage ) )
			{
				// デフォルトの描画
				DrawDefaultInspector() ;


//				GUILayout.Label( "-----------------", boldStyle ) ;
				DrawSeparater() ;
			}

			//----------------------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteImage ;

			//----------------------------------

			DrawAtlas( component ) ;

			if( component.SpriteAtlas != null || component.SpriteSet != null )
			{
				DrawFlipper( component ) ;
			}

			DrawCollider( component ) ;

			DrawAnimator( component ) ;

			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}

		//-------------------------------------------------------------------------------------------
		// Atlas

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( SpriteImage component )
		{
			// スプライトアトラス
			SpriteAtlas spriteAtlas = EditorGUILayout.ObjectField( new GUIContent( "Sprite Atlas", "<color=#00FFFF>SpriteAtlas</color>アセットを設定します\nランタイム実行中、<color=#00FFFF>SetSpriteInAtlas</color>メソッドを使用する事により\n表示する<color=#00FFFF>Spriteを動的に切り替える</color>事が出来ます" ), component.SpriteAtlas, typeof( SpriteAtlas ), false ) as SpriteAtlas ;
			if( spriteAtlas != component.SpriteAtlas )
			{
				Undo.RecordObject( component, "[SpriteImage] Sprite Atlas : Change" ) ;	// アンドウバッファに登録

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
						component.Width  = component.Sprite.rect.width ;
						component.Height = component.Sprite.rect.height ;
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
						Undo.RecordObject( component, "[SpriteImage] Sprite : Change" ) ;	// アンドウバッファに登録
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
				Undo.RecordObject( component, "[SpriteImage] SpriteSet Texture : Change" ) ;	// アンドウバッファに登録

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
						component.Width  = component.Sprite.rect.width ;
						component.Height = component.Sprite.rect.height ;
					}
				}
				EditorUtility.SetDirty( component ) ;
			}

			if( component.SpriteSet != null )
			{
				spriteSetTextureActive = component.SpriteSet.Texture ;

				// サイズ
				if( spriteSetTextureActive != null )
				{
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
						Undo.RecordObject( component, "[SpriteImage] Sprite : Change" ) ;	// アンドウバッファに登録
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

			//------------------------------------------------------------------------------------------
			// Interpolation 関係

			// 変化値
			float interpolationValue = EditorGUILayout.Slider( "Interpolation Value", component.InterpolationValue, 0, 1 ) ;
			if( component.InterpolationValue != interpolationValue )
			{
				Undo.RecordObject( component, "[SpriteImage] Interpolation Value : Change" ) ;	// アンドウバッファに登録
				component.InterpolationValue  = interpolationValue ;
				EditorUtility.SetDirty( component ) ;
			}

			// 変化色
			var interpolationColor = Color.white ;
			interpolationColor.r = component.InterpolationColor.r ;
			interpolationColor.g = component.InterpolationColor.g ;
			interpolationColor.b = component.InterpolationColor.b ;
			interpolationColor.a = component.InterpolationColor.a ;
			interpolationColor = EditorGUILayout.ColorField( "Interpolation Color", interpolationColor ) ;
			if
			(
				interpolationColor.r != component.InterpolationColor.r ||
				interpolationColor.g != component.InterpolationColor.g ||
				interpolationColor.b != component.InterpolationColor.b ||
				interpolationColor.a != component.InterpolationColor.a
			)
			{
				Undo.RecordObject( component, "[SpriteImage] Interpolation Color : Change" ) ;	// アンドウバッファに登録
				component.InterpolationColor = interpolationColor ;
				EditorUtility.SetDirty( component ) ;
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
			if( spriteAtlas == null )
			{
				return null ;
			}

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
		private void RefreshSpriteSet( SpriteImage component, Texture atlasTexture )
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

		protected void DrawFlipper( SpriteImage component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			DrawSeparater() ;

			//------------------------------------------------------------------------------------------

			List<string> animationNames ;

			if( component.Animations != null && component.Animations.Count >  0 )
			{
				animationNames = component.Animations.Where( _ => string.IsNullOrEmpty( _.AnimationName ) == false ).Select( _ => _.AnimationName ).ToList() ;
			}
			else
			{
				animationNames = new List<string>() ;
			}

			if( animationNames.Count == 0 )
			{
				animationNames.Insert( 0, "None" ) ;
			}
			else
			{
				if( string.IsNullOrEmpty( component.PlayingAnimationName ) == true || component.PlayingAnimationName == "None" )
				{
					// 未設定の場合は最初のアニメーション名を自動で設定する
					component.PlayingAnimationName = animationNames[ 0 ] ;
				}
			}

			int indexOld = 0 ;
			int index = animationNames.IndexOf( component.PlayingAnimationName ) ;
			if( index >= 0 )
			{
				indexOld = index ;
			}

			// アニメーション選択
			int indexNew = EditorGUILayout.Popup( "Animation Name", indexOld, animationNames.ToArray() ) ;
			if( indexOld != indexNew )
			{
				Undo.RecordObject( component, "[SpriteImage] Animation Name : Change" ) ;	// アンドウバッファに登録
				component.PlayingAnimationName = animationNames[ indexNew ] ;
				EditorUtility.SetDirty( component ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isLooping = EditorGUILayout.Toggle( component.IsAnimationLooping, GUILayout.Width( 16f ) ) ;
				if( component.IsAnimationLooping != isLooping )
				{
					Undo.RecordObject( component, "[SpriteImage] Is Animation Looping : Change" ) ;	// アンドウバッファに登録
					component.IsAnimationLooping = isLooping ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "Is Animation Looping", "ループさせるかどうか" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			// 変化値
			float animationSpeed = EditorGUILayout.Slider( "Animtion Speed", component.AnimationSpeed, 0.1f, 10.0f ) ;
			if( component.AnimationSpeed != animationSpeed )
			{
				Undo.RecordObject( component, "[SpriteImage] Animation Speed : Change" ) ;	// アンドウバッファに登録
				component.AnimationSpeed  = animationSpeed ;
				EditorUtility.SetDirty( component ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool playOnAwake = EditorGUILayout.Toggle( component.AnimationPlayOnAwake, GUILayout.Width( 16f ) ) ;
				if( component.AnimationPlayOnAwake != playOnAwake )
				{
					Undo.RecordObject( component, "[SpriteImage] Animation Play On Awake : Change" ) ;	// アンドウバッファに登録
					component.AnimationPlayOnAwake = playOnAwake ;
					EditorUtility.SetDirty( component ) ;
				}
				GUILayout.Label( new GUIContent( "Animation Play On Awake", "自動でアニメーションを再生させるかどうか" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------
			// アニメーション一覧

			var animations = serializedObject.FindProperty( "m_Animations" ) ;
			EditorGUILayout.PropertyField( animations ) ;

			//----------------------------------------------------------

			if( Application.isPlaying == true )
			{
				// 再生中かどうか(ランタイム実行中のみ有効)
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isAnimationPlaying = EditorGUILayout.Toggle( component.IsAnimationPlaying, GUILayout.Width( 16f ) ) ;
					if( component.IsAnimationPlaying != isAnimationPlaying )
					{
						component.IsAnimationPlaying  = isAnimationPlaying ;
					}
					
					GUILayout.Label( new GUIContent( "Is Animation Playing", "アニメーションが再生中かどうか" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//------------------------------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			DrawSeparater() ;
		}

//#if false
		// 個々のアニメーション情報表示
		[CustomPropertyDrawer( typeof( SpriteImage.AnimationDescriptor ), true )]
		public class AnimationDrawer : PropertyDrawer
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
				// フレームリスト部分の縦幅は可変
				var framesProperty = property.FindPropertyRelative( "Frames" ) ;
				float height = EditorGUI.GetPropertyHeight( framesProperty ) ;

				return base.GetPropertyHeight( property, label ) + EditorGUIUtility.standardVerticalSpacing + LineHeight * 0 + height + EditorGUIUtility.standardVerticalSpacing ;
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
//				position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label ) ;

				// 子のフィールドをインデントしない 
//				var indent = EditorGUI.indentLevel ;
//				EditorGUI.indentLevel = 0 ;

				float x = position.x ;
				float y = position.y ;
				float w = position.width ;

				var spriteImage = property.serializedObject.targetObject as SpriteImage ;
#if false
				SpriteImage.AnimationDescriptor animation = null ;
				int index = property.GetArrayElementIndex() ;
				if( index >= 0 && spriteImage.Animations != null && spriteImage.Animations.Count >  index )
				{
					animation = spriteImage.Animations[ index ] ;
				}
#endif
				//---------------------------------
				// AnimationtName

				var animationNameProperty = property.FindPropertyRelative( "AnimationName" ) ;

				var animationNameRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string animationNameLabel = "Name" ; // animationNameProperty.displayName ;
				var animationName = EditorGUI.TextField( animationNameRect, animationNameLabel, animationNameProperty.stringValue ) ;
				if( animationNameProperty.stringValue != animationName )
				{
					if( string.IsNullOrEmpty( animationName ) == false )
					{
						Undo.RecordObject( spriteImage, "[SpriteImage] Animation Name : Change" ) ;	// アンドウバッファに登録
						animationNameProperty.stringValue = animationName ;
						EditorUtility.SetDirty( spriteImage ) ;
					}
				}

				y += LineHeight ;

				//---------------------------------
				// FrameDuartion
#if false
				y += LineHeight ;

				var frameDurationProperty = property.FindPropertyRelative( "FrameDuration" ) ;

				var frameDurationRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				var frameDuration = EditorGUI.FloatField( frameDurationRect, frameDurationProperty.displayName, frameDurationProperty.floatValue ) ;
				if( frameDurationProperty.floatValue != frameDuration )
				{
					if( frameDuration >  0 )
					{
						Undo.RecordObject( spriteImage, "[SpriteImage] Frame Duration : Change" ) ;	// アンドウバッファに登録
						frameDurationProperty.floatValue = frameDuration ;
						EditorUtility.SetDirty( spriteImage ) ;
					}
				}
#endif
				//---------------------------------
				// Frames

				var framesProperty = property.FindPropertyRelative( "Frames" ) ;
				float framesHeight = EditorGUI.GetPropertyHeight( framesProperty ) ;

				var framesRect = new Rect( x + ( w * 0.0f ), y, w * 1.0f, framesHeight ) ;

				EditorGUI.PropertyField( framesRect, framesProperty ) ;

				//---------------------------------------------------------

				// インデントを元通りに戻します
//				EditorGUI.indentLevel = indent ;

				EditorGUI.EndProperty() ;
			}

#if false
			private bool CheckAnimationName( List<SpriteImage.AnimationDescriptor> animations, SpriteImage.AnimationDescriptor animation, string requestAnimationName )
			{
				if( animations == null || animations.Count <= 1 )
				{
					return true ;
				}

				var animationNames = animations.Where( _ => ( _ != animation ) ).Select( _ => _.AnimationName ).ToList() ;
				if( animationNames.Count == 0 )
				{
					return true ;
				}
				
				//---------------------------------

				foreach( var animationName in animationNames )
				{
					if( animationName == requestAnimationName )
					{
						// 名称が重複している
						return false ;
					}
				}

				return true ;
			}

			// アニメーション名が被る場合は適切に変更して返す
			private string CorrectAnimationName( List<SpriteImage.AnimationDescriptor> animations, SpriteImage.AnimationDescriptor animation, string requestAnimationName )
			{
				if( animations == null || animations.Count <= 1 )
				{
					return requestAnimationName ;
				}

				var animationNames = animations.Where( _ => ( _ != animation ) ).Select( _ => _.AnimationName ).ToList() ;
				if( animationNames.Count == 0 )
				{
					return requestAnimationName ;
				}
				
				//---------------------------------

				bool isDuplication ;
				var regax = new Regex( @"[^0-9]" ) ;

				int limitCount = 0 ;

				do
				{
					isDuplication = false ;
					foreach( var animationName in animationNames )
					{
						if( animationName == requestAnimationName )
						{
							// 名称が重複してしまう
							isDuplication = true ;
							break ;
						}
					}

					if( isDuplication == true )
					{
						// 名称を変更する

						var match = Regex.Match( requestAnimationName, "^([a-zA-Z0-9_]*)\\(([0-9]*)\\)$" ) ;	// 最後がカッコ番号になっているかどうか
						if( match.Success == true )
						{
							int i = requestAnimationName.LastIndexOf( '(' ) ;
							string number = requestAnimationName[ i.. ] ;
							requestAnimationName = requestAnimationName[ ..i ] ;
							int.TryParse( regax.Replace( number, "" ), out var count ) ;
							count ++ ;
							requestAnimationName = $"{requestAnimationName}({count})" ;
						}
						else
						{
							requestAnimationName += "(1)" ;
						}
					}

					limitCount ++ ;
					if( limitCount >= 1000 )
					{
						break ;
					}
				}
				while( isDuplication ) ;	// 重複が起きなくなったら抜ける

				//---------------------------------

				return requestAnimationName ;
			}
#endif
		}


		// 個々のアニメーション情報表示
		[CustomPropertyDrawer( typeof( SpriteImage.AnimationDescriptor.FrameDescriptor ), true )]
		public class AnimationFrameDrawer : PropertyDrawer
		{
			/// プロパティの高さを取得する。カスタムによって高さが変わるなら必須
			public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
			{
				return base.GetPropertyHeight( property, label ) ;
			}

			// 指定された矩形内のプロパティを描画
			public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
			{
//				position.x =+ ( position.width * 0.2f ) ;
//				position.width *= 0.8f ;

				EditorGUI.BeginProperty( position, label, property ) ;

				// ラベルを描画
//				position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label ) ;

				// 子のフィールドをインデントしない 
//				var indent = EditorGUI.indentLevel ;
//				EditorGUI.indentLevel = 0 ;

				float x = position.x ;
				float y = position.y ;
				float w = position.width ;
				float h = position.height ;

				var spriteImage = property.serializedObject.targetObject as SpriteImage ;

				// 各長さ
				float textureWidth = h ;

				float cw = w - textureWidth ;

				float spriteNameWidth = cw * 0.6f ;
				float spaceWidth = cw * 0.05f ;
				float durationWidth = cw * 0.3f ;

				//---------------------------------
				// SpriteName

				var spriteName = property.FindPropertyRelative( "SpriteName" ).stringValue ;

				var names = spriteImage.GetSpriteNames() ;
				if( names == null || names.Length == 0 )
				{
					names = new string[]{ "Unknown" } ;
				}
				var spriteNames = names.ToList() ;

				int indexOld = 0 ;
				int index = spriteNames.IndexOf( spriteName ) ;
				if( index >= 0 )
				{
					indexOld = index ;
				}

				//---------------------------------
				// スプライ群が存在する状態で空文字は許容しない(重要:初期状態では一見スプライト識別名が設定されているようで実際は空文字になっている)
				if( string.IsNullOrEmpty( spriteName ) == true )
				{
					if( names != null && names.Length >  0 )
					{
						property.FindPropertyRelative( "SpriteName" ).stringValue = names[ 0 ] ;
					}
				}
				//---------------------------------

				var spriteNameRect = new Rect( x, y, spriteNameWidth, h ) ;

				int indexNew = EditorGUI.Popup( spriteNameRect, indexOld, spriteNames.ToArray() ) ;
				if( indexOld != indexNew )
				{
					Undo.RecordObject( spriteImage, "[SpriteImage] SpriteName : Change" ) ;	// アンドウバッファに登録
					property.FindPropertyRelative( "SpriteName" ).stringValue = spriteNames[ indexNew ] ;
					EditorUtility.SetDirty( spriteImage ) ;
				}

				x += spriteNameWidth ;
				x += spaceWidth ;

				//---------------------------------
				// Sprite(Texture)

				Sprite sprite = spriteImage.GetSpriteInAtlas( spriteNames[ indexNew ] ) ;
				if( sprite != null )
				{
					var textureRect = new Rect( x, y - EditorGUIUtility.standardVerticalSpacing * 0.5f, textureWidth, textureWidth ) ;
					DrawPreviewTexture( textureRect, sprite ) ;
				}

				x += textureWidth ;
				x += spaceWidth ;

				//---------------------------------
				// Duration

				var durationProperty = property.FindPropertyRelative( "Duration" ) ;

				if( durationProperty.floatValue <= 0 )
				{
					durationProperty.floatValue  = 0.1f ;
				}

				var durationRect = new Rect( x, y, durationWidth, h ) ;

				var duration = EditorGUI.FloatField( durationRect, durationProperty.floatValue ) ;
				if( durationProperty.floatValue != duration )
				{
					if( duration <= 0 )
					{
						duration  = 0.1f ;
					}

					Undo.RecordObject( spriteImage, "[SpriteImage] Duration : Change" ) ;	// アンドウバッファに登録
					durationProperty.floatValue = duration ;
					EditorUtility.SetDirty( spriteImage ) ;
				}

//				x += durationWidth ;
//				x += spaceWidth ;

				//---------------------------------------------------------

				// インデントを元通りに戻します
//				EditorGUI.indentLevel = indent ;

				EditorGUI.EndProperty() ;
			}

			// シンプルなテクスチャを描画する
			private void DrawPreviewTexture( Rect position, Sprite sprite )
			{
				var fullSize = new Vector2( sprite.texture.width, sprite.texture.height ) ;
				var size = new Vector2( sprite.textureRect.width, sprite.textureRect.height ) ;
 
				Rect coords = sprite.textureRect ;
				coords.x      /= fullSize.x ;
				coords.width  /= fullSize.x ;
				coords.y      /= fullSize.y ;
				coords.height /= fullSize.y ;
 
				Vector2 ratio ;
				ratio.x = position.width  / size.x ;
				ratio.y = position.height / size.y ;
				float minRatio = Mathf.Min( ratio.x, ratio.y ) ;
 
				Vector2 center  = position.center ;
				position.width  = size.x * minRatio ;
				position.height = size.y * minRatio ;
				position.center = center ;
 
				GUI.DrawTextureWithTexCoords( position, sprite.texture, coords ) ;
			}
		}
//#endif
		//-------------------------------------------------------------------------------------------
		// Collider

		private int		m_ColliderIndex			= 0 ;
		private bool	m_ColliderRemoveAready	= false ;

		protected void DrawCollider( SpriteImage component )
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

				// コライダーの自動調整
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( " ", GUILayout.Width( 16f ) ) ;
					bool colliderAdjustment = EditorGUILayout.Toggle( component.ColliderAdjustment, GUILayout.Width( 16f ) ) ;
					if( colliderAdjustment != component.ColliderAdjustment )
					{
						Undo.RecordObject( component, "SpriteImage : Collider Adjustment Change" ) ;	// アンドウバッファに登録
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
		protected void DrawAnimator( SpriteImage controller )
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

