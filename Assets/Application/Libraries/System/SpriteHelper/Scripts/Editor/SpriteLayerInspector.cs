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
	[CustomEditor( typeof( SpriteLayer ) )]
	public class SpriteLayerInspector : SpriteTransformInspector
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
			var component = target as SpriteLayer ;

			// トランスフォーム部分を描画する
			DrawTransform( component, true ) ;

			//------------------------------------------------------------------------------------------

			// 区切り線
			DrawSeparater() ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Viewport Size Type (H)", "水平方向のビューポートサイズのタイプです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var horizontalViewportSizeType = ( ViewportSizeTypes )EditorGUILayout.EnumPopup( component.HorizontalViewportSizeType, GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 160f ) ) ;
				if( component.HorizontalViewportSizeType != horizontalViewportSizeType )
				{
					Undo.RecordObject( component, "SpriteLayer : Viewport Size Type (H) Change" ) ;	// アンドウバッファに登録
					component.HorizontalViewportSizeType  = horizontalViewportSizeType ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.HorizontalViewportSizeType != ViewportSizeTypes.Stretch )
			{
				// Position And Size
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Position And Size", "Ｘ位置と横幅です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
					var viewportPositionX = EditorGUILayout.FloatField( component.ViewportPositionX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportPositionX != viewportPositionX )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Position X Change" ) ;	// アンドウバッファに登録
						component.ViewportPositionX  = viewportPositionX ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
					var viewportSizeX = EditorGUILayout.FloatField( component.ViewportSizeX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportSizeX != viewportSizeX )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Size X Change" ) ;	// アンドウバッファに登録
						component.ViewportSizeX = viewportSizeX ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				// Margin LR
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Margin", "左右のマージンです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "L", GUILayout.Width( 16f ) ) ;
					var viewportMarginL = EditorGUILayout.FloatField( component.ViewportMarginL, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportMarginL != viewportMarginL )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Margin L Change" ) ;	// アンドウバッファに登録
						component.ViewportMarginL  = viewportMarginL ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "R", GUILayout.Width( 16f ) ) ;
					var viewportMarginR  = EditorGUILayout.FloatField( component.ViewportMarginR, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportMarginR != viewportMarginR )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Margin R Change" ) ;	// アンドウバッファに登録
						component.ViewportMarginR  = viewportMarginR ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Viewport Size Type (V)", "垂直方向のビューポートサイズのタイプです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var verticalViewportSizeType = ( ViewportSizeTypes )EditorGUILayout.EnumPopup( component.VerticalViewportSizeType, GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 160f ) ) ;
				if( component.VerticalViewportSizeType != verticalViewportSizeType )
				{
					Undo.RecordObject( component, "SpriteLayer : Viewport Size Type (V) Change" ) ;	// アンドウバッファに登録
					component.VerticalViewportSizeType  = verticalViewportSizeType ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.VerticalViewportSizeType != ViewportSizeTypes.Stretch )
			{
				// Position And Size
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Position And Size", "Ｙ位置と縦幅です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
					var viewportPositionY = EditorGUILayout.FloatField( component.ViewportPositionY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportPositionY != viewportPositionY )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Position Y Change" ) ;	// アンドウバッファに登録
						component.ViewportPositionY  = viewportPositionY ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
					var viewportSizeY = EditorGUILayout.FloatField( component.ViewportSizeY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportSizeY != viewportSizeY )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Size Y Change" ) ;	// アンドウバッファに登録
						component.ViewportSizeY = viewportSizeY ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				// Margin LR
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Margin", "上下のマージンです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "T", GUILayout.Width( 16f ) ) ;
					var viewportMarginT = EditorGUILayout.FloatField( component.ViewportMarginT, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportMarginT != viewportMarginT )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Margin T Change" ) ;	// アンドウバッファに登録
						component.ViewportMarginT  = viewportMarginT ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "B", GUILayout.Width( 16f ) ) ;
					var viewportMarginB = EditorGUILayout.FloatField( component.ViewportMarginB, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ViewportMarginB != viewportMarginB )
					{
						Undo.RecordObject( component, "SpriteLayer : Viewport Margin B Change" ) ;	// アンドウバッファに登録
						component.ViewportMarginB  = viewportMarginB ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUI.enabled = false ;

			// Offset
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Offset", "オフセットです(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ViewportOffsetX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ViewportOffsetY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			
			// DeltSize
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Delta Size", "実サイズです(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ViewportDeltaSizeX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ViewportDeltaSizeY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUI.enabled = true ;

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース
/*
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Map", "マップのノードです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var map = EditorGUILayout.ObjectField( component.Map, typeof( SpriteTransform ), true ) as SpriteTransform ;
				if( component.Map != map )
				{
					Undo.RecordObject( component, "SpriteLayer : Map Change" ) ;	// アンドウバッファに登録
					component.Map  = map ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.Map != null )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Map Position", "マップの表示位置です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
					var mapPositionX = EditorGUILayout.FloatField( component.MapPositionX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapPositionX != mapPositionX )
					{
						Undo.RecordObject( component, "SpriteLayer : Map Position X Change" ) ;	// アンドウバッファに登録
						component.MapPositionX = mapPositionX ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
					var mapPositionY = EditorGUILayout.FloatField( component.MapPositionY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapPositionY != mapPositionY )
					{
						Undo.RecordObject( component, "SpriteLayer : Map Position Y Change" ) ;	// アンドウバッファに登録
						component.MapPositionY = mapPositionY ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------------------------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Map Chip Size", "マップチップのサイズです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
					var mapChipSizeX = EditorGUILayout.FloatField( component.MapChipSizeX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapChipSizeX != mapChipSizeX )
					{
						Undo.RecordObject( component, "SpriteLayer : Map Chip Size X Change" ) ;	// アンドウバッファに登録
						component.MapChipSizeX = mapChipSizeX ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
					var mapChipSizeY = EditorGUILayout.FloatField( component.MapChipSizeY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapChipSizeY != mapChipSizeY )
					{
						Undo.RecordObject( component, "SpriteLayer : Map Chip Size Y Change" ) ;	// アンドウバッファに登録
						component.MapChipSizeY = mapChipSizeY ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Map Chip Offset", "マップチップのオフセットです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
					var mapChipOffsetX = EditorGUILayout.FloatField( component.MapChipOffsetX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapChipOffsetX != mapChipOffsetX )
					{
						Undo.RecordObject( component, "SpriteLayer : Map ChipOffset X Change" ) ;	// アンドウバッファに登録
						component.MapChipOffsetX = mapChipOffsetX ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
					var mapChipOffsetY = EditorGUILayout.FloatField( component.MapChipOffsetY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MapChipOffsetY != mapChipOffsetY )
					{
						Undo.RecordObject( component, "SpriteLayer : Map Chip Offset Y Change" ) ;	// アンドウバッファに登録
						component.MapChipOffsetY = mapChipOffsetY ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------------------------------

				if( component.MapSizeX >  0 && component.MapSizeY >  0 )
				{
					EditorGUILayout.Separator() ;   // 少し区切りスペース

					GUI.enabled = false ;

					// MapSize
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( new GUIContent( "Map Size", "マップサイズです(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

						GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
						EditorGUILayout.FloatField( component.MapSizeX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

						GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

						GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
						EditorGUILayout.FloatField( component.MapSizeY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					// MapArea
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( new GUIContent( "Map Area", "現在のセーフエリア[Screen座標系]です(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

						GUILayout.Label( "L", GUILayout.Width( 12f ) ) ;
						EditorGUILayout.FloatField( component.MapAreaL, GUILayout.MinWidth( 20f ), GUILayout.MaxWidth( 32f ) ) ;

						GUILayout.Label( "R", GUILayout.Width( 12f ) ) ;
						EditorGUILayout.FloatField( component.MapAreaR, GUILayout.MinWidth( 20f ), GUILayout.MaxWidth( 32f ) ) ;

						GUILayout.Label( "T", GUILayout.Width( 12f ) ) ;
						EditorGUILayout.FloatField( component.MapAreaT, GUILayout.MinWidth( 20f ), GUILayout.MaxWidth( 32f ) ) ;

						GUILayout.Label( "B", GUILayout.Width( 12f ) ) ;
						EditorGUILayout.FloatField( component.MapAreaB, GUILayout.MinWidth( 20f ), GUILayout.MaxWidth( 32f ) ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUI.enabled = false ;
				}
			}
*/
			//----------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//----------------------------------------------------------
			// レイヤー一覧

			var layers = serializedObject.FindProperty( "m_Layers" ) ;
			EditorGUILayout.PropertyField( layers ) ;


			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}


		//-------------------------------------------------------------------------------------------

		// 個々のレイヤー情報表示
		[CustomPropertyDrawer( typeof( SpriteLayer.LayerDescriptor ), true )]
		public class LayerDrawer : PropertyDrawer
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
//				var layersProperty = property.FindPropertyRelative( "m_Layers" ) ;
//				float height = EditorGUI.GetPropertyHeight( layersProperty ) ;

//				return base.GetPropertyHeight( property, label ) + EditorGUIUtility.standardVerticalSpacing + LineHeight * 1 + height + EditorGUIUtility.standardVerticalSpacing ;

				int lineCount = 0 ;

				var layerProperty = property.FindPropertyRelative( "LayerTransform" ) ;
				if( layerProperty.objectReferenceValue != null )
				{
					lineCount += 4 ;
				}

				return base.GetPropertyHeight( property, label ) + EditorGUIUtility.standardVerticalSpacing + LineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing ;
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

				var spriteLayer = property.serializedObject.targetObject as SpriteLayer ;

				//---------------------------------
				// MapChipSize

				var layerTransformProperty = property.FindPropertyRelative( "LayerTransform" ) ;

				var layerRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

				string layerTransformLabel = "LayerTransform" ; // animationNameProperty.displayName ;
				var layerTransformObject = EditorGUI.ObjectField( layerRect, layerTransformLabel, layerTransformProperty.objectReferenceValue, typeof( SpriteTransform ), true ) ;
				if( layerTransformProperty.objectReferenceValue != layerTransformObject )
				{
					Undo.RecordObject( spriteLayer, "SpriteLayer : LayerTransform : Change" ) ;	// アンドウバッファに登録
					layerTransformProperty.objectReferenceValue = layerTransformObject ;
					EditorUtility.SetDirty( spriteLayer ) ;
				}

				y += LineHeight ;

				//---------------------------------------------------------

				if( layerTransformProperty.objectReferenceValue != null )
				{
					//---------------------------------
					// MapPosition

					// PropertyDrawer の対象しているオブジェクトは property.boxedValue で取得する
					var layerObject = property.boxedValue as SpriteLayer.LayerDescriptor ;

					var mapPositionRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

					string mapPositionLabel = "MapPosition" ; // animationNameProperty.displayName ;
					var mapPosition = EditorGUI.Vector2Field( mapPositionRect, mapPositionLabel, layerObject.MapPosition ) ;
					if( layerObject.MapPosition != mapPosition )
					{
						Undo.RecordObject( spriteLayer, "SpriteLayer : Map Position : Change" ) ;	// アンドウバッファに登録
						layerObject.MapPosition  = mapPosition ;
						EditorUtility.SetDirty( spriteLayer ) ;
					}

					y += LineHeight ;

					//---------------------------------
					// MapPositionRatio

					var mapPositionRatioProperty = property.FindPropertyRelative( "MapPositionRatio" ) ;

					var mapPositionRatioRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

					string mapPositionRatioLabel = "MapPositionRatio" ; // animationNameProperty.displayName ;
					var mapPositionRatio = EditorGUI.Vector2Field( mapPositionRatioRect, mapPositionRatioLabel, mapPositionRatioProperty.vector2Value ) ;
					if( mapPositionRatioProperty.vector2Value != mapPositionRatio )
					{
						Undo.RecordObject( spriteLayer, "SpriteLayer : MapPositionRatio : Change" ) ;	// アンドウバッファに登録
						mapPositionRatioProperty.vector2Value = mapPositionRatio ;
						EditorUtility.SetDirty( spriteLayer ) ;
					}

					y += LineHeight ;

					//---------------------------------
					// MapChipSize

					var mapChipSizeProperty = property.FindPropertyRelative( "MapChipSize" ) ;

					var mapChipSizeRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

					string mapChipSizeLabel = "MapChipSize" ; // animationNameProperty.displayName ;
					var mapChipSize = EditorGUI.Vector2Field( mapChipSizeRect, mapChipSizeLabel, mapChipSizeProperty.vector2Value ) ;
					if( mapChipSizeProperty.vector2Value != mapChipSize )
					{
						Undo.RecordObject( spriteLayer, "SpriteLayer : Map Chip Size : Change" ) ;	// アンドウバッファに登録
						mapChipSizeProperty.vector2Value = mapChipSize ;
						EditorUtility.SetDirty( spriteLayer ) ;
					}

					y += LineHeight ;

					//---------------------------------
					// MapChipOffset

					var mapChipOffsetProperty = property.FindPropertyRelative( "MapChipOffset" ) ;

					var mapChipOffsetRect = new Rect( x, y, w, EditorGUIUtility.singleLineHeight ) ;

					string mapChipOffsetLabel = "MapChipOffset" ; // animationNameProperty.displayName ;
					var mapChipOffset = EditorGUI.Vector2Field( mapChipOffsetRect, mapChipOffsetLabel, mapChipOffsetProperty.vector2Value ) ;
					if( mapChipOffsetProperty.vector2Value != mapChipOffset )
					{
						Undo.RecordObject( spriteLayer, "SpriteLayer : Map Chip Offset : Change" ) ;	// アンドウバッファに登録
						mapChipOffsetProperty.vector2Value = mapChipOffset ;
						EditorUtility.SetDirty( spriteLayer ) ;
					}

					y += LineHeight ;
				}

				//---------------------------------
				// Frames
#if false
				var layersProperty = property.FindPropertyRelative( "Layers" ) ;
				float layersHeight = EditorGUI.GetPropertyHeight( layersProperty ) ;

				var framesRect = new Rect( x + ( w * 0.0f ), y, w * 1.0f, layersHeight ) ;

				EditorGUI.PropertyField( framesRect, layersProperty ) ;
#endif
				//---------------------------------------------------------

				// インデントを元通りに戻します
//				EditorGUI.indentLevel = indent ;

				EditorGUI.EndProperty() ;
			}
		}
	}
}
#endif
