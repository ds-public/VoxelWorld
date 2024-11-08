#if UNITY_EDITOR

using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using System.Reflection ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[ CustomEditor( typeof( SpriteImage ), true ) ]
	public class SpriteImageInspector : SpriteFrameInspector
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

				DrawSeparater() ;
			}

			//----------------------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteImage ;

			//----------------------------------

			DrawColor( component ) ;

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
		protected void DrawColor( SpriteImage component )
		{
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

		//-------------------------------------------------------------------------------------------

		// フリッパーアニメーション
		protected void DrawFlipper( SpriteImage component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			DrawSeparater() ;

			//------------------------------------------------------------------------------------------

			List<string> animationNames ;

			if( component.Animations != null && component.Animations.Count >  0 )
			{
				animationNames = component.Animations.Where( _ => string.IsNullOrEmpty( _.Name ) == false ).Select( _ => _.Name ).ToList() ;
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
			int indexNew = EditorGUILayout.Popup( "Default Animation Name", indexOld, animationNames.ToArray() ) ;
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

			if( Application.isPlaying == false )
			{
				bool isImport = false ;
				bool isExport = false ;

				EditorGUILayout.Separator() ;	// 少し区切りスペース
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUI.backgroundColor = Color.cyan ;	// ボタンの下地を緑に
					if( GUILayout.Button( new GUIContent( "Import", "アニメーション情報を Json テキストファイルから取り込みます" ), GUILayout.Width( 60f ) ) == true )
					{
						isImport = true ;
					}
					GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

					if( component.Animations != null && component.Animations.Count >  0 )
					{
						GUI.backgroundColor = Color.green ;	// ボタンの下地を緑に
						if( GUILayout.Button( new GUIContent( "Export", "アニメーション情報を Json テキストファイルに書き出します" ), GUILayout.Width( 60f ) ) == true )
						{
							isExport = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//--------------------------------------------------------

				if( isImport == true )
				{
					// エクスポート処理へ
					var path = EditorUtility.OpenFilePanel( "Load Animaions", "Assets", "json" ) ;
					if( string.IsNullOrEmpty( path ) == false )
					{
						var json = File.ReadAllText( path ) ;
						if( string .IsNullOrEmpty( json ) == false )
						{
							if( component.ConvertAnimationsFromJson( json ) == true )
							{
								EditorUtility.DisplayDialog( "Save Animations", "アニメーションを Json テキストから読み込みました", "OK" ) ;
							}
							else
							{
								EditorUtility.DisplayDialog( "Save Animations", "Json テキストの読み込みに失敗しました(変換失敗)", "OK" ) ;
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Save Animations", "Json テキストの読み込みに失敗しました(読込失敗)", "OK" ) ;
						}
					}
				}

				if( isExport == true )
				{
					// エクスポート処理へ
					var path = EditorUtility.SaveFilePanel( "Save Animaions", "Assets", $"{component.name}_Animations.json", "json" ) ;
					if( string.IsNullOrEmpty( path ) == false )
					{
						var json = component.ConvertAnimationsToJson() ;
						if( string.IsNullOrEmpty( json ) == false )
						{
							File.WriteAllText( path, json ) ;

							EditorUtility.DisplayDialog( "Save Animations", "アニメーションを Json テキストで書き出しました", "OK" ) ;
						}
						else
						{
							EditorUtility.DisplayDialog( "Save Animations", "Json テキストの書き出しに失敗しました", "OK" ) ;
						}
					}
				}
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

				return base.GetPropertyHeight( property, label ) + EditorGUIUtility.standardVerticalSpacing + LineHeight * 1 + height + EditorGUIUtility.standardVerticalSpacing ;
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
				// AnimationName

				var animationNameProperty = property.FindPropertyRelative( "Name" ) ;

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
				// AnimationSpeed

				var animationSpeedProperty = property.FindPropertyRelative( "Speed" ) ;

				var animationSpeedRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string animationSpeedLabel = "Speed" ; // animationNameProperty.displayName ;
				var animationSpeed = EditorGUI.Slider( animationSpeedRect, animationSpeedLabel, animationSpeedProperty.floatValue, 0, 10 ) ;
				if( animationSpeedProperty.floatValue != animationSpeed )
				{
					Undo.RecordObject( spriteImage, "[SpriteImage] Animation Speed : Change" ) ;	// アンドウバッファに登録
					animationSpeedProperty.floatValue = animationSpeed ;
					EditorUtility.SetDirty( spriteImage ) ;
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
	}
}

#endif

