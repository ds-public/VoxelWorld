//#if false

#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Line のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Line ) ) ]
	public class LineInspector : MaskableGraphicWrapperInspector
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
			Line view = target as Line ;
		
			// Graphic の基本情報を描画する
			DrawBasis( view ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプライト
			Sprite sprite = EditorGUILayout.ObjectField( "Sprite", view.sprite, typeof( Sprite ), false ) as Sprite ;
			if( sprite != view.sprite )
			{
				Undo.RecordObject( view, "Line : Sprite Change" ) ;	// アンドウバッファに登録
				view.sprite = sprite ;
				EditorUtility.SetDirty( view ) ;
			}

			// 最初の太さ
			float startWidth = EditorGUILayout.FloatField( "Start Width", view.startWidth ) ;
			if( startWidth != view.startWidth && startWidth >  0 )
			{
				Undo.RecordObject( view, "Line : Start Width Change" ) ;	// アンドウバッファに登録
				view.startWidth = startWidth ;
				EditorUtility.SetDirty( view ) ;
			}

			// 最後の太さ
			float endWidth = EditorGUILayout.FloatField( "End Width", view.endWidth ) ;
			if( endWidth != view.endWidth && endWidth >  0 )
			{
				Undo.RecordObject( view, "Line : Start Width Change" ) ;	// アンドウバッファに登録
				view.endWidth = endWidth ;
				EditorUtility.SetDirty( view ) ;
			}

			// 最初の色
			Color startColor = new Color( view.startColor.r, view.startColor.g, view.startColor.b, view.startColor.a ) ;
			startColor = EditorGUILayout.ColorField( "Start Color", startColor ) ;
			if( startColor.r != view.startColor.r || startColor.g != view.startColor.g || startColor.b != view.startColor.b || startColor.a != view.startColor.a )
			{
				Undo.RecordObject( view, "Line : Start Color Change" ) ;	// アンドウバッファに登録
				view.startColor = startColor ;
				EditorUtility.SetDirty( view ) ;
			}

			// 最後の色
			Color endColor = new Color( view.endColor.r, view.endColor.g, view.endColor.b, view.endColor.a ) ;
			endColor = EditorGUILayout.ColorField( "End Color", endColor ) ;
			if( endColor.r != view.endColor.r || endColor.g != view.endColor.g || endColor.b != view.endColor.b || endColor.a != view.endColor.a )
			{
				Undo.RecordObject( view, "Line : End Color Change" ) ;	// アンドウバッファに登録
				view.endColor = endColor ;
				EditorUtility.SetDirty( view ) ;
			}

			// オフセット
			Vector2 offset = EditorGUILayout.Vector2Field( "Offset", view.offset ) ;
			if( offset.Equals( view.offset ) == false )
			{
				Undo.RecordObject( view, "Line : Offset Change" ) ;	// アンドウバッファに登録
				view.offset = offset ;
				EditorUtility.SetDirty( view ) ;
			}

			// データ配列
			SerializedObject so = new SerializedObject( view ) ;
			so.Update() ;
			SerializedProperty sp = so.FindProperty( "vertices" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp, new GUIContent( "Vertices" ) ) ;
			}
//			if( so.hasModifiedProperties == true )
//			{
				so.ApplyModifiedProperties() ;
//			}


			// ポジションタイプ
			Line.PositionType positionType = ( Line.PositionType )EditorGUILayout.EnumPopup( "Position Type",  view.positionType ) ;
			if( positionType != view.positionType )
			{
				Undo.RecordObject( view, "Line : Position Type Change" ) ;	// アンドウバッファに登録
				view.positionType = positionType ;
				EditorUtility.SetDirty( view ) ;
			}
		}
	}
}

#endif

//#endif
