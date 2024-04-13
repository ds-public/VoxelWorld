#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIScrollView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIScrollView ) ) ]
	public class UIScrollViewInspector : UIImageInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIScrollView view = target as UIScrollView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// スクロール方向
			UIScrollView.DirectionTypes directionType = ( UIScrollView.DirectionTypes )EditorGUILayout.EnumPopup( "DirectionType",  view.DirectionType ) ;
			if( directionType != view.DirectionType )
			{
				Undo.RecordObject( view, "UIScrollView : DirectionType Change" ) ;	// アンドウバッファに登録
				view.DirectionType = directionType ;
				EditorUtility.SetDirty( view ) ;
			}

			// スクロールバーの有無
			DrawScrollbar( view ) ;
		}

		// スクロールバーの有無を表示
		protected void DrawScrollbar( UIScrollView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// 横スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var isHorizontalScrollbar = EditorGUILayout.Toggle( view.IsHorizontalScrollber, GUILayout.Width( 16f ) ) ;
				if( isHorizontalScrollbar != view.IsHorizontalScrollber )
				{
					Undo.RecordObject( view, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					view.IsHorizontalScrollber = isHorizontalScrollbar ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Horizontal Scrollbar", GUILayout.Width( 120f ) ) ;

				// 横位置
				var horizontalScrollbarPosition = ( UIScrollView.HorizontalScrollbarPositionTypes )EditorGUILayout.EnumPopup( "",  view.HorizontalScrollbarPositionType, GUILayout.Width( 80f ) ) ;
				if( horizontalScrollbarPosition != view.HorizontalScrollbarPositionType )
				{
					Undo.RecordObject( view, "UIScrollView : Horizontal Scrollbar Position Change" ) ;	// アンドウバッファに登録
					view.HorizontalScrollbarPositionType = horizontalScrollbarPosition ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			var horizontalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", view.HorizontalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( horizontalScrollbarElastic != view.HorizontalScrollbarElastic )
			{
				Undo.RecordObject( view, "UIScrollView : Horizontal Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				view.HorizontalScrollbarElastic = horizontalScrollbarElastic ;
				EditorUtility.SetDirty( view ) ;
			}

			// 縦スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var isVerticalScrollbar = EditorGUILayout.Toggle( view.IsVerticalScrollber, GUILayout.Width( 16f ) ) ;
				if( isVerticalScrollbar != view.IsVerticalScrollber )
				{
					Undo.RecordObject( view, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					view.IsVerticalScrollber = isVerticalScrollbar ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Vertical Scrollbar", GUILayout.Width( 120f ) ) ;

				// 縦位置
				var verticalScrollbarPosition = ( UIScrollView.VerticalScrollbarPositionTypes )EditorGUILayout.EnumPopup( "",  view.VerticalScrollbarPositionType, GUILayout.Width( 80f ) ) ;
				if( verticalScrollbarPosition != view.VerticalScrollbarPositionType )
				{
					Undo.RecordObject( view, "UIScrollView : Vertical Scrollbar Position Change" ) ;	// アンドウバッファに登録
					view.VerticalScrollbarPositionType = verticalScrollbarPosition ;
					EditorUtility.SetDirty( view ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			var verticalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", view.VerticalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( verticalScrollbarElastic != view.VerticalScrollbarElastic )
			{
				Undo.RecordObject( view, "UIScrollView : Vertical Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				view.VerticalScrollbarElastic = verticalScrollbarElastic ;
				EditorUtility.SetDirty( view ) ;
			}

			//----------------------------------------------------------

			bool isWarning ;

			isWarning = false ;
			if( view.HorizontalScrollbarElastic != null && view.HorizontalScrollbarElastic.ScrollViewElastic == null )
			{
				isWarning = true ;
			}
			if( view.VerticalScrollbarElastic != null && view.VerticalScrollbarElastic.ScrollViewElastic == null )
			{
				isWarning = true ;
			}
			if( isWarning == true )
			{
				EditorGUILayout.HelpBox( GetMessage( "SetScrollView" ), MessageType.Warning, true ) ;
			}

			if( view.TryGetComponent<ScrollRectWrapper>( out var scroll) == true )
			{
				isWarning = false ;
				if( view.HorizontalScrollbarElastic != null && scroll.horizontalScrollbar!= null )
				{
					isWarning = true ;
				}
				if( view.VerticalScrollbarElastic != null && scroll.verticalScrollbar != null )
				{
					isWarning = true ;
				}
				if( isWarning == true )
				{
					EditorGUILayout.HelpBox( GetMessage( "ClearScrollBar" ), MessageType.Warning, true ) ;
				}
			}

			//----------------------------------------------------------

			if( view.IsHorizontalScrollber == true || view.HorizontalScrollbarElastic != null || view.IsVerticalScrollber == true || view.VerticalScrollbarElastic != null )
			{
				// スクロールバーのフェード処理
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					var scrollbarFadeEnebled = EditorGUILayout.Toggle( view.ScrollbarFadeEnabled, GUILayout.Width( 16f ) ) ;
					if( scrollbarFadeEnebled != view.ScrollbarFadeEnabled )
					{
						Undo.RecordObject( view, "UIScrollView : Scrollbar Fade Enabled Change" ) ;	// アンドウバッファに登録
						view.ScrollbarFadeEnabled = scrollbarFadeEnebled ;
						EditorUtility.SetDirty( view ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Scrollbar Fade Enabled" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				if( view.ScrollbarFadeEnabled == true )
				{
//					if( view.IsInteraction == false && view.IsInteractionForScrollView == false )
//					{
//						EditorGUILayout.HelpBox( GetMessage( "InteractionNone" ), MessageType.Warning, true ) ;
//					}

//					if( view._scrollRect.verticalScrollbarVisibility != UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide )
//					{
//						EditorGUILayout.HelpBox( GetMessage( "SetAutoHide" ), MessageType.Warning, true ) ;
//					}

	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０

					var scrollbarFadeInDuration = EditorGUILayout.FloatField( " Duration In", view.ScrollbarFadeInDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( scrollbarFadeInDuration != view.ScrollbarFadeInDuration )
					{
						Undo.RecordObject( view, "UIScrollView : Scrollbar Fade In Duration" ) ;	// アンドウバッファに登録
						view.ScrollbarFadeInDuration = scrollbarFadeInDuration ;
						EditorUtility.SetDirty( view ) ;
					}

					var scrollbarFadeHoldDuration = EditorGUILayout.FloatField( " Duration Hold", view.ScrollbarFadeHoldDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( scrollbarFadeHoldDuration != view.ScrollbarFadeHoldDuration )
					{
						Undo.RecordObject( view, "UIScrollView : Scrollbar Fade Hold Duration" ) ;	// アンドウバッファに登録
						view.ScrollbarFadeHoldDuration = scrollbarFadeHoldDuration ;
						EditorUtility.SetDirty( view ) ;
					}

					var scrollbarFadeOutDuration = EditorGUILayout.FloatField( " Duration Out", view.ScrollbarFadeOutDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( scrollbarFadeOutDuration != view.ScrollbarFadeOutDuration )
					{
						Undo.RecordObject( view, "UIScrollView : Scrollbar Fade Out Duration" ) ;	// アンドウバッファに登録
						view.ScrollbarFadeOutDuration = scrollbarFadeOutDuration ;
						EditorUtility.SetDirty( view ) ;
					}



	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０


					if( view.HorizontalScrollbar != null )
					{
						if( view.HorizontalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "HSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
					if( view.VerticalScrollbar != null )
					{
						if( view.VerticalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "VSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					var hidingScrollbarIfContentFew = EditorGUILayout.Toggle( view.HidingScrollbarIfContentFew, GUILayout.Width( 16f ) ) ;
					if( hidingScrollbarIfContentFew != view.HidingScrollbarIfContentFew )
					{
						Undo.RecordObject( view, "UIScrollView : Hiding Scrollbar If Content Few" ) ;	// アンドウバッファに登録
						view.HidingScrollbarIfContentFew = hidingScrollbarIfContentFew ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( " Hiding Scrollbar If Content Few" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					var invalidateScrollIfContentFew = EditorGUILayout.Toggle( view.InvalidateScrollIfContentFew, GUILayout.Width( 16f ) ) ;
					if( invalidateScrollIfContentFew != view.InvalidateScrollIfContentFew )
					{
						Undo.RecordObject( view, "UIScrollView : Invalidate Scroll If Content Few" ) ;	// アンドウバッファに登録
						view.InvalidateScrollIfContentFew = invalidateScrollIfContentFew ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( " Invalidate Scroll If Content Few" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		private readonly  Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "SetScrollView",			"Elastic を 完全に有効にするには Scrollbar に ScrollView を設定する必要があります" },
			{ "ClearScrollBar",			"Elastic を 完全に有効にするには ScrollRect の Scrollbar を消去する必要があります" },
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスまたは UIInteractionForScrollView クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "SetScrollView",			"In order to fully enable Elastic you need to set ScrollView on Scrollbar" },
			{ "ClearScrollBar",			"In order to fully enable Elastic, you need to clear the Scrollbar of ScrollRect" },
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' or 'UIInteractionForScrollView' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
		} ;

		private string GetMessage( string label )
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
