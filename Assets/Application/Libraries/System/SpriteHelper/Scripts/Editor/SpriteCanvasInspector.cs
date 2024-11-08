#if UNITY_EDITOR

using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	[ CustomEditor( typeof( SpriteCanvas ), true ) ]
	public class SpriteCanvasInspector : SpriteTransformInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// ターゲットのインスタンス
			var component = target as SpriteCanvas ;


			//----------------------------------------------------------

			// ボールド
			var boldStyle = new GUIStyle( GUI.skin.label )
			{
				fontStyle = FontStyle.Bold
			} ;

			//----------------------------------------------------------

			// 対象のカメラ
			GUILayout.Label( "Sprite Canvas 表示用のカメラ", boldStyle ) ;
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Sprite Camera", "<color=#00FFFF>Camera</color>アセットを設定します" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;
				var spriteCamera = EditorGUILayout.ObjectField( component.SpriteCamera, typeof( Camera ), true ) as Camera ;
				if( component.SpriteCamera != spriteCamera )
				{
					Undo.RecordObject( component, "SpriteCanvas : Sprite Camera Change" ) ;	// アンドウバッファに登録
					component.SpriteCamera  = spriteCamera ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// セーフエリア対応が必要かどうか
			GUILayout.Label( "セーフエリア対応が必要かどうか", boldStyle ) ;
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Safe Area Enabled", "<color=#00FFFF>SafeAreaEnabled</color>の有効・無効を設定します" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f )  ) ;
				var safeAreaEnabled = EditorGUILayout.Toggle( component.SafeAreaEnabled, GUILayout.Width( 24f ) ) ;
				if( component.SafeAreaEnabled != safeAreaEnabled )
				{
					Undo.RecordObject( component, "SpriteCanvas : Safe Area Enabled Change" ) ;	// アンドウバッファに登録
					component.SafeAreaEnabled  = safeAreaEnabled ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ビューポートの表示位置設定
			GUILayout.Label( "基準解像度", boldStyle ) ;

			// Basic Resolution
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Basic Resolution", "基準解像度です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
				var basicWidth = EditorGUILayout.FloatField( component.BasicWidth, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.BasicWidth != basicWidth )
				{
					Undo.RecordObject( component, "SpriteCanvas : Basic Width Change" ) ;	// アンドウバッファに登録
					component.BasicWidth  = basicWidth ;
					EditorUtility.SetDirty( component ) ;
				}

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
				var basicHeight = EditorGUILayout.FloatField( component.BasicHeight, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.BasicHeight != basicHeight )
				{
					Undo.RecordObject( component, "SpriteCanvas : Basic Height Change" ) ;	// アンドウバッファに登録
					component.BasicHeight = basicHeight ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ビューポートの表示位置設定
			GUILayout.Label( "最大解像度", boldStyle ) ;

			// Limit Resolution
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Limit Resolution", "最大解像度です" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
				var limitWidth = EditorGUILayout.FloatField( component.LimitWidth, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.LimitWidth != limitWidth )
				{
					Undo.RecordObject( component, "SpriteCanvas : Limit Width Change" ) ;	// アンドウバッファに登録
					component.LimitWidth  =limitWidth ;
					EditorUtility.SetDirty( component ) ;
				}

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
				var limitHeight = EditorGUILayout.FloatField( component.LimitHeight, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
				if( component.LimitHeight != limitHeight )
				{
					Undo.RecordObject( component, "SpriteCanvas : Limit Height Change" ) ;	// アンドウバッファに登録
					component.LimitHeight = limitHeight ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ビューポートの表示位置設定
			GUILayout.Label( "ビューポートの表示位置設定", boldStyle ) ;

			//--------------

			// トランスフォームの表示
			DrawTransform( component, false ) ;

			EditorGUILayout.HelpBox( "ピボットは DeltaSize の領域で計算されます\n-0.5 ～ +0.5", MessageType.Info, true ) ;

			//----------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// プロジェクションサイズに反映するかどうか
			GUILayout.Label( "プロジェクションサイズに反映するかどうか", boldStyle ) ;
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Projection Size Adjustment", "<color=#00FFFF>ProjectionSizeAutomaticAdjustment</color>の有効・無効を設定します" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 160f )  ) ;
				var projectionSizeAdjustment = EditorGUILayout.Toggle( component.ProjectionSizeAdjustment, GUILayout.Width( 24f ) ) ;
				if( component.ProjectionSizeAdjustment != projectionSizeAdjustment )
				{
					Undo.RecordObject( component, "SpriteCanvas : Projection Size Adjustment : Change" ) ;	// アンドウバッファに登録
					component.ProjectionSizeAdjustment  = projectionSizeAdjustment ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//------------------------------------------------------------------------------------------
			// 以下は ReadOnly 系

			EditorGUILayout.Space( 16f );	// 少し区切りスペース

			GUI.enabled = false ;

			// ビューポートの表示位置設定
			GUILayout.Label( "現在の画面の解像度 ※ReadOnly", boldStyle ) ;

			// Screen Resolution
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Screen Resolution", "現在の画面の解像度です(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "W", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ScreenWidth, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;

				GUILayout.Label( "", GUILayout.Width( 2f ) ) ;

				GUILayout.Label( "H", GUILayout.Width( 16f ) ) ;
				EditorGUILayout.FloatField( component.ScreenHeight, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//--------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ビューポートの表示位置設定
			GUILayout.Label( "現在のセーフエリア[Screen座標系] ※ReadOnly", boldStyle ) ;

			// Screen Safe Area
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Screen Safe Are", "現在のセーフエリア[Screen座標系]です(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "L", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaSL, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "R", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaSR, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "T", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaST, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "B", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaSB, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//--------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ビューポートの表示位置設定
			GUILayout.Label( "現在のセーフエリア[Canvas座標系] ※ReadOnly", boldStyle ) ;

			// Screen Safe Area
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( new GUIContent( "Canvas Safe Are", "現在のセーフエリア[Canvas座標系]です(読み取り専用)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

				GUILayout.Label( "L", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaCL, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "R", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaCR, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "T", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaCT, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;

				GUILayout.Label( "B", GUILayout.Width( 12f ) ) ;
				EditorGUILayout.FloatField( component.SafeAreaCB, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 24f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//--------------

			GUI.enabled = true ;
		}
	}
}

#endif
