#if UNITY_EDITOR

using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[CustomEditor( typeof( SpriteTransform ), true )]
	public class SpriteTransformInspector : Editor
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
			var component = target as SpriteTransform ;

			// トランスフォーム部分を描画する
			DrawTransform( component, true ) ;
		}

		// トランスフォーム部分を描画する
		protected void DrawTransform( SpriteTransform component, bool isFull )
		{
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//------------------------------------------------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Horizontal Anchor Type", "水平方向のアンカーのタイプです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var horizontalAnchorType = ( HorizontalAnchorTypes )EditorGUILayout.EnumPopup( component.HorizontalAnchorType, GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 160f ) ) ;
				if( component.HorizontalAnchorType != horizontalAnchorType )
				{
					Undo.RecordObject( component, "SpriteDrawer : Horizontal Anchor Type Change" ) ;	// アンドウバッファに登録
					component.HorizontalAnchorType  = horizontalAnchorType ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.HorizontalAnchorType != HorizontalAnchorTypes.Stretch )
			{
				// Position And Size
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Position And Size", "Ｘ位置と横幅です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
					var anchorPositionX = EditorGUILayout.FloatField( component.AnchorPositionX, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.AnchorPositionX != anchorPositionX )
					{
						Undo.RecordObject( component, "SpriteDrawer : Anchor Position X Change" ) ;	// アンドウバッファに登録
						component.AnchorPositionX  = anchorPositionX ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
					var width = EditorGUILayout.FloatField( component.Width, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.Width != width  )
					{
						Undo.RecordObject( component, "SpriteDrawer : Width  Change" ) ;	// アンドウバッファに登録
						component.Width = width  ;
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
					var marginL = EditorGUILayout.FloatField( component.MarginL, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MarginL != marginL )
					{
						Undo.RecordObject( component, "SpriteDrawer : Margin L Change" ) ;	// アンドウバッファに登録
						component.MarginL  = marginL ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "R", GUILayout.Width( 16f ) ) ;
					var marginR  = EditorGUILayout.FloatField( component.MarginR, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MarginR != marginR )
					{
						Undo.RecordObject( component, "SpriteDrawer : Margin R Change" ) ;	// アンドウバッファに登録
						component.MarginR  = marginR ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}


			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Vertical Anchor Type", "垂直方向のアンカーのタイプです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var verticalAnchorType = ( VerticalAnchorTypes )EditorGUILayout.EnumPopup( component.VerticalAnchorType, GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 160f ) ) ;
				if( component.VerticalAnchorType != verticalAnchorType )
				{
					Undo.RecordObject( component, "SpriteDrawer : Vertical Anchor Type Change" ) ;	// アンドウバッファに登録
					component.VerticalAnchorType  = verticalAnchorType ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( component.VerticalAnchorType != VerticalAnchorTypes.Stretch )
			{
				// Position And Size
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Position And Size", "Ｙ位置と縦幅です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
					var anchorPositionY = EditorGUILayout.FloatField( component.AnchorPositionY, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.AnchorPositionY != anchorPositionY )
					{
						Undo.RecordObject( component, "SpriteDrawer : Anchor Position Y Change" ) ;	// アンドウバッファに登録
						component.AnchorPositionY  = anchorPositionY ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
					var height = EditorGUILayout.FloatField( component.Height, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.Height != height )
					{
						Undo.RecordObject( component, "SpriteDrawer : Height Change" ) ;	// アンドウバッファに登録
						component.Height = height ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				// Margin TB
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "　Margin", "上下のマージンです" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "T", GUILayout.Width( 16f ) ) ;
					var marginT = EditorGUILayout.FloatField( component.MarginT, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MarginT != marginT )
					{
						Undo.RecordObject( component, "SpriteDrawer : Margin T Change" ) ;	// アンドウバッファに登録
						component.MarginT  = marginT ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "B", GUILayout.Width( 16f ) ) ;
					var marginB = EditorGUILayout.FloatField( component.MarginB, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.MarginB != marginB )
					{
						Undo.RecordObject( component, "SpriteDrawer : Margin B Change" ) ;	// アンドウバッファに登録
						component.MarginB  = marginB ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// Pivot
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Pivot", "ピボット(中心)です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				var pivotX = EditorGUILayout.FloatField( component.Pivot.x, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.Pivot.x != pivotX )
				{
					Undo.RecordObject( component, "SpriteDrawer : Pivot X Change" ) ;	// アンドウバッファに登録
					component.Pivot = new Vector2( pivotX, component.Pivot.y ) ;
					EditorUtility.SetDirty( component ) ;
				}

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				var pivotY = EditorGUILayout.FloatField( component.Pivot.y, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.Pivot.y != pivotY )
				{
					Undo.RecordObject( component, "SpriteDrawer : Pivot Y Change" ) ;	// アンドウバッファに登録
					component.Pivot = new Vector2( component.Pivot.x, pivotY ) ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//------------------------------------------------------------------------------------------

			if( isFull == true )
			{
				EditorGUILayout.Separator() ;   // 少し区切りスペース

				// ZIndex
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "ZIndex", "表示の優先順位(小さい方が手前に表示される)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					var zIndex = EditorGUILayout.FloatField( component.ZIndex, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.ZIndex != zIndex )
					{
						Undo.RecordObject( component, "SpriteDrawer : ZIndex Change" ) ;	// アンドウバッファに登録
						component.ZIndex = zIndex ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------

				// Rotation
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "Rotation", "各軸での回転" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( new GUIContent( "X", "Pitch 回転" ), GUILayout.Width( 12f ) ) ;
					var pitch = EditorGUILayout.FloatField( component.Pitch, GUILayout.MinWidth( 8f ), GUILayout.MaxWidth( 30f ) ) ;
					if( component.Pitch != pitch )
					{
						Undo.RecordObject( component, "SpriteDrawer : Pitch Change" ) ;	// アンドウバッファに登録
						component.Pitch  = pitch ;
						EditorUtility.SetDirty( component ) ;
					}

//					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( new GUIContent( "Y", "Yaw 回転" ), GUILayout.Width( 12f ) ) ;
					var yaw = EditorGUILayout.FloatField( component.Yaw, GUILayout.MinWidth( 8f ), GUILayout.MaxWidth( 30f ) ) ;
					if( component.Yaw != yaw )
					{
						Undo.RecordObject( component, "SpriteDrawer : Yaw Change" ) ;	// アンドウバッファに登録
						component.Yaw  = yaw ;
						EditorUtility.SetDirty( component ) ;
					}

//					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( new GUIContent( "Z", "Roll 回転" ), GUILayout.Width( 12f ) ) ;
					var roll = EditorGUILayout.FloatField( component.Roll, GUILayout.MinWidth(  8f ), GUILayout.MaxWidth( 30f ) ) ;
					if( component.Roll != roll )
					{
						Undo.RecordObject( component, "SpriteDrawer : Roll Change" ) ;	// アンドウバッファに登録
						component.Roll  = roll ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//---------------------------------

				// Scale
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( new GUIContent( "Scale", "縮尺" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

					GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
					var scaleX = EditorGUILayout.FloatField( component.Scale.x, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.Scale.x != scaleX )
					{
						Undo.RecordObject( component, "SpriteDrawer : Scale X Change" ) ;	// アンドウバッファに登録
						component.Scale = new Vector2( scaleX, component.Scale.y ) ;
						EditorUtility.SetDirty( component ) ;
					}

					GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

					GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
					var scaleY = EditorGUILayout.FloatField( component.Scale.y, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
					if( component.Scale.y != scaleY )
					{
						Undo.RecordObject( component, "SpriteDrawer : Scale Y Change" ) ;	// アンドウバッファに登録
						component.Scale = new Vector2( component.Scale.x, scaleY ) ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//------------------------------------------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUI.enabled = false ;

			// Offset
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Offset", "オフセットです(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.Offset.x, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.Offset.y, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			
			// DeltSize
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Delta Size", "実サイズです(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "X", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.DeltaSize.x, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "Y", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.DeltaSize.y, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUI.enabled = true ;
			
			EditorGUILayout.Separator() ;   // 少し区切りスペース
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
