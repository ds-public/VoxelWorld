#if UNITY_EDITOR

using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;

using TMPro ;


namespace uGUIHelper
{
	/// <summary>
	/// UIView のインスペクタークラス(引数の true は継承クラスでも有効にするかどうか)
	/// </summary>
	[ CustomEditor( typeof( UIView ), true ) ]
	public class UIViewInspector : Editor
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
			UIView view = target as UIView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//-----------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			GUI.backgroundColor = Color.cyan ;
			string identity = EditorGUILayout.TextField( new GUIContent( "Identity", "任意で指定する、このビューの<color=#00FF00>識別名</color>です\n省略すると<color=#00FFFF>gameObject.name</color>が使用されます" ), view.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != view.Identity )
			{
				Undo.RecordObject( view, "UIView : Identity Change" ) ;	// アンドウバッファに登録
				view.Identity = identity ;
				EditorUtility.SetDirty( view ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// タイムスケール
			float timeScale = EditorGUILayout.FloatField( new GUIContent( "TimeScale", "<color=#00FFFF>PlayTween</color>メソッドや<color=#00FFFF>PlayAnimator</color>メソッドの\n<color=#00FF00>再生速度に乗算</color>されます" ), view.TimeScale ) ;
			if( timeScale != view.TimeScale )
			{
				Undo.RecordObject( view, "UIView : TimeScale Change" ) ;	// アンドウバッファに登録
				view.TimeScale = timeScale ;
				EditorUtility.SetDirty( view ) ;
			}

			//------------------------------------------

			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( view ) ;

			// イベントトリガーを有効にするかどうか
//			DrawEventTrigger( view ) ;

			//----------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool setPivotToCenter = EditorGUILayout.Toggle( view.AutoPivotToCenter, GUILayout.Width( 16f ) ) ;
				if( setPivotToCenter != view.AutoPivotToCenter )
				{
					Undo.RecordObject( view, "UIView : Set Pivot To Center Change" ) ;	// アンドウバッファに登録
					view.AutoPivotToCenter = setPivotToCenter ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Set Pivot To Center", "<color=#00FFFF>ランタイム実行時</color>に\nピボットを強制的に中心(0.5,0.5)に変更します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------

			// インタラクション
			DrawInteraction( view ) ;

			// トランジション
			DrawTransition( view ) ;

			// Tween の追加と削除
			DrawTween( view ) ;

			//------------------------------------------

			DrawInspectorGUI() ;

			//------------------------------------------

			// エフェクトカテゴリ
			DrawEffect( view ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//------------------------------------------

			// トグルカテゴリ
			DrawToggleCategory( view ) ;

			// レクトマスト２Ｄ
			DrawRectMask2D( view ) ;

			// アルファマスク
			DrawAlphaMask( view ) ;

			// コンテンツサイズフィッター
			DrawContentSizeFitter( view ) ;



			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 子の色
			DrawChildrenColor( view ) ;

			if( view.TryGetComponent<Graphic>( out _ ) == true )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// バックキー
				DrawBackKey( view ) ;

				// パッドアダプター
				DrawPadAdapter( view ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// アニメーター
			DrawAnimator( view ) ;

			if( serializedObject.hasModifiedProperties )
			{
				serializedObject.ApplyModifiedProperties() ;
			}
		}

		// 派生クラスの個々のＧＵＩを描画する
		virtual protected void DrawInspectorGUI(){}

		// キャンバスグループの設定項目を描画する
		protected void DrawCanvasGroup( UIView view )
		{
			// キャンバスグループを有効にするかどうか

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// Image が無ければ CanvasRender の追加・削除ができる UI を追加する
			if
			(
				view.GetComponent<Image>() == null				&&
				view.GetComponent<TextMeshProUGUI>() == null	&&
				view.GetComponent<ImageNumber>() == null		&&
				view.GetComponent<Circle>() == null				&&
				view.GetComponent<Line>() == null				&&
				view.GetComponent<Arc>() == null				&&
				view.GetComponent<GridMap>() == null			&&
				view.GetComponent<ComplexRectangle>() == null
			)
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isCanvasRenderer = EditorGUILayout.Toggle( view.IsCanvasRenderer, GUILayout.Width( 16f ) ) ;
					if( isCanvasRenderer != view.IsCanvasRenderer )
					{
						Undo.RecordObject( view, "UIView : Canvas Renderer Change" ) ;	// アンドウバッファに登録
						view.IsCanvasRenderer = isCanvasRenderer ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "CanvasRenderer" ) ;

					bool isGraphicEmpty = EditorGUILayout.Toggle( view.IsGraphicEmpty, GUILayout.Width( 16f ) ) ;
					if( isGraphicEmpty != view.IsGraphicEmpty )
					{
						Undo.RecordObject( view, "UIView : Graphic Empty Change" ) ;	// アンドウバッファに登録
						view.IsGraphicEmpty = isGraphicEmpty ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "GraphicEmpty" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

//				EditorGUILayout.Separator() ;	// 少し区切りスペース
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCanvasGroup = EditorGUILayout.Toggle( view.IsCanvasGroup, GUILayout.Width( 16f ) ) ;
				if( isCanvasGroup != view.IsCanvasGroup )
				{
					Undo.RecordObject( view, "UIView : Canvas Group Change" ) ;	// アンドウバッファに登録
					view.IsCanvasGroup = isCanvasGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "CanvasGroup", "<color=#00FFFF>CanvasGroup</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( view.IsCanvasGroup == true )
			{
				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float alpha = EditorGUILayout.Slider( "Alpha", view.GetCanvasGroup().alpha, 0.0f, 1.0f ) ;
				if( alpha != view.GetCanvasGroup().alpha )
				{
					Undo.RecordObject( view, "UIView : Alpha Change" ) ;	// アンドウバッファに登録
					view.GetCanvasGroup().alpha = alpha ;
					EditorUtility.SetDirty( view ) ;
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float tDisableRaycastUnderAlpha = EditorGUILayout.Slider( "Disable Raycast Under Alpha", view.DisableRaycastUnderAlpha, 0.0f, 1.0f ) ;
				if( tDisableRaycastUnderAlpha != view.DisableRaycastUnderAlpha )
				{
					Undo.RecordObject( view, "UIView : Disable Raycast Under Alpha Change" ) ;	// アンドウバッファに登録
					view.DisableRaycastUnderAlpha = tDisableRaycastUnderAlpha ;
					EditorUtility.SetDirty( view ) ;
				}
			}
		}

		// イベントトリガーの生成破棄チェックボックスを描画する
		protected void DrawEventTrigger( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isEventTrigger = EditorGUILayout.Toggle( view.IsEventTrigger, GUILayout.Width( 16f ) ) ;
				if( isEventTrigger != view.IsEventTrigger )
				{
					Undo.RecordObject( view, "UIView : EventTrigger Change" ) ;	// アンドウバッファに登録
					view.IsEventTrigger = isEventTrigger ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "EventTrigger" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// インタラクションの生成破棄チェックボックスを描画する
		protected void DrawInteraction( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isInteraction = EditorGUILayout.Toggle( view.IsInteraction, GUILayout.Width( 16f ) ) ;
				if( isInteraction != view.IsInteraction )
				{
					Undo.RecordObject( view, "UIView : Interaction Change" ) ;	// アンドウバッファに登録
					view.IsInteraction = isInteraction ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Interaction", "<color=#00FFFF>UIInteraction</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n各種インタラクションの<color=#00FFFF>コールバックを受け取れる</color>ようになります\n<color=#00FF00>OnClick OnSmartClick OnSimpleClick OnPress OnSimplePress OnDrag OnFlick ...</color>\n<color=#FFFF00>※ RaycastTarget が有効になっている必要があります</color>" ) ) ;

				//-------------

				bool isInteractionForScrollView = EditorGUILayout.Toggle( view.IsInteractionForScrollView, GUILayout.Width( 16f ) ) ;
				if( isInteractionForScrollView != view.IsInteractionForScrollView )
				{
					Undo.RecordObject( view, "UIView : Interaction For ScrollView Change" ) ;	// アンドウバッファに登録
					view.IsInteractionForScrollView = isInteractionForScrollView ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Interaction For ScrollView", "<color=#00FFFF>InteractionForScrollView</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n各種インタラクションの<color=#00FFFF>コールバックを受け取れる</color>ようになります\n<color=#00FF00>OnClick OnSmartClick OnSimpleClick OnPress OnSimplePress OnDrag OnFlick ...</color>\nScrollViewやListViewの項目内で使用する事が推奨されます\nインタラクションの反応を良くします\n<color=#FFFF00>※ RaycastTarget が有効になっている必要があります</color>" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// トランジションの生成破棄チェックボックスを描画する
		protected void DrawTransition( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isTransition = EditorGUILayout.Toggle( view.IsTransition, GUILayout.Width( 16f ) ) ;
				if( isTransition != view.IsTransition )
				{
					Undo.RecordObject( view, "UIView : Transition Change" ) ;	// アンドウバッファに登録
					view.IsTransition = isTransition ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Transition", "<color=#00FFFF>UITransition</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\nこのビューに対してインタラクションを行った際に\n<color=#00FFFF>スケーリングアニメーション</color>を追加します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// レクトマスク２Ｄの生成破棄チェックボックスを描画する
		protected void DrawRectMask2D( UIView view )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isRectMask2D = EditorGUILayout.Toggle( view.IsRectMask2D, GUILayout.Width( 16f ) ) ;
				if( isRectMask2D != view.IsRectMask2D )
				{
					Undo.RecordObject( view, "UIView : RectMask2D Change" ) ;	// アンドウバッファに登録
					view.IsRectMask2D = isRectMask2D ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "RectMask2D", "<color=#00FFFF>RectMask2D</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// アルファマスクの生成破棄チェックボックスを描画する
		protected void DrawAlphaMask( UIView view )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAlphaMaskWindow = EditorGUILayout.Toggle( view.IsAlphaMaskWindow, GUILayout.Width( 16f ) ) ;
				if( isAlphaMaskWindow != view.IsAlphaMaskWindow )
				{
					Undo.RecordObject( view, "UIView : AlphaMaskWindow Change" ) ;	// アンドウバッファに登録
					view.IsAlphaMaskWindow = isAlphaMaskWindow ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskWindow" ) ;

				bool isAlphaMaskTarget = EditorGUILayout.Toggle( view.IsAlphaMaskTarget, GUILayout.Width( 16f ) ) ;
				if( isAlphaMaskTarget != view.IsAlphaMaskTarget )
				{
					Undo.RecordObject( view, "UIView : AlphaMaskTarget Change" ) ;	// アンドウバッファに登録
					view.IsAlphaMaskTarget = isAlphaMaskTarget ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskTarget" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// コンテントサイズフィッターの生成破棄チェックボックスを描画する
		protected void DrawContentSizeFitter( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isHorizontalLayoutGroup = EditorGUILayout.Toggle( view.IsHorizontalLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isHorizontalLayoutGroup != view.IsHorizontalLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : HorizontalLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsHorizontalLayoutGroup = isHorizontalLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "HorizontalLayoutGroup", "<color=#00FFFF>HorizontalLayoutGroup</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;

				bool isVerticalLayoutGroup = EditorGUILayout.Toggle( view.IsVerticalLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isVerticalLayoutGroup != view.IsVerticalLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : VerticalLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsVerticalLayoutGroup = isVerticalLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "VerticalLayoutGroup", "<color=#00FFFF>VerticalLayoutGroup</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isGridLayoutGroup = EditorGUILayout.Toggle( view.IsGridLayoutGroup, GUILayout.Width( 16f ) ) ;
				if( isGridLayoutGroup != view.IsGridLayoutGroup )
				{
					Undo.RecordObject( view, "UIView : GridLayoutGroup Change" ) ;	// アンドウバッファに登録
					view.IsGridLayoutGroup = isGridLayoutGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "GridLayoutGroup", "<color=#00FFFF>GridLayoutGroup</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isContentSizeFitter = EditorGUILayout.Toggle( view.IsContentSizeFitter, GUILayout.Width( 16f ) ) ;
				if( isContentSizeFitter != view.IsContentSizeFitter )
				{
					Undo.RecordObject( view, "UIView : ContentSizeFitter Change" ) ;	// アンドウバッファに登録
					view.IsContentSizeFitter = isContentSizeFitter ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "ContentSizeFitter", "<color=#00FFFF>ContentSizeFitter</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;

				bool isLayoutElement = EditorGUILayout.Toggle( view.IsLayoutElement, GUILayout.Width( 16f ) ) ;
				if( isLayoutElement != view.IsLayoutElement )
				{
					Undo.RecordObject( view, "UIView : LayoutElement Change" ) ;	// アンドウバッファに登録
					view.IsLayoutElement = isLayoutElement ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "LayoutElement", "<color=#00FFFF>LayoutElement</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// トグルグループの生成破棄チェックボックスを描画する
		protected void DrawToggleCategory( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isToggleGroup = EditorGUILayout.Toggle( view.IsToggleGroup, GUILayout.Width( 16f ) ) ;
				if( isToggleGroup != view.IsToggleGroup )
				{
					Undo.RecordObject( view, "UIView : ToggleGroup Change" ) ;	// アンドウバッファに登録
					view.IsToggleGroup = isToggleGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ToggleGroup" ) ;

				bool isButtonGroup = EditorGUILayout.Toggle( view.IsButtonGroup, GUILayout.Width( 16f ) ) ;
				if( isButtonGroup != view.IsButtonGroup )
				{
					Undo.RecordObject( view, "UIView : ButtonGroup Change" ) ;	// アンドウバッファに登録
					view.IsButtonGroup = isButtonGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ButtonGroup" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// パッドアダプターの生成破棄チェックボックスを描画する
		protected void DrawPadAdapter( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isPadAdapter = EditorGUILayout.Toggle( view.IsPadAdapter, GUILayout.Width( 16f ) ) ;
				if( isPadAdapter != view.IsPadAdapter )
				{
					Undo.RecordObject( view, "UIView : PadAdapter Change" ) ;	// アンドウバッファに登録
					view.IsPadAdapter = isPadAdapter ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "PadAdapter", "<color=#00FFFF>UIPadAdapter</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\nこのビューに対してゲームパッドによるインタラクションを行った際\n<color=#00FFFF>コールバックを受け取れる</color>ようになります\n<color=#00FF00>OnPadValueChanged OnPadDown OnPadUp OnPadButtonChanged OnPadButtonDown OnPadButtonUp OnPadAxisChanged OnPadAxisDown OnPadAxisUp</color>" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			if( view.IsPadAdapter == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( " ", GUILayout.Width( 16f ) ) ;

					bool padAutoFocusEnabled = EditorGUILayout.Toggle( view.PadAutoFocusEnabled, GUILayout.Width( 16f ) ) ;
					if( padAutoFocusEnabled != view.PadAutoFocusEnabled )
					{
						Undo.RecordObject( view, "UIView : Pad Auto Focus Enabled Change" ) ;	// アンドウバッファに登録
						view.PadAutoFocusEnabled = padAutoFocusEnabled ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Auto Focus Enabled" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( view.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( isAnimator != view.IsAnimator )
				{
					Undo.RecordObject( view, "UIView : Animator Change" ) ;	// アンドウバッファに登録
					view.IsAnimator = isAnimator ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Animator", "<color=#00FFFF>Animator</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n<color=#00FFFF>PlayAnimator</color>メソッドを実行する際に必要になります" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		//---------------------------------------------------------------------------

		// Tween の追加と削除
		private string m_AddTweenIdentity = "" ;
		private int    m_RemoveTweenIndex = 0 ;
		private int    m_RemoveTweenIndexAnswer = -1 ;

		protected void DrawTween( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			var tweens = view.GetComponents<UITween>() ;

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
							var tween = view.AddComponent<UITween>() ;
							tween.Identity = m_AddTweenIdentity ;

							// 追加後の全ての Tween を取得する
							var temporaryTweens = view.gameObject.GetComponents<UITween>() ;
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
						Undo.RecordObject( view, "UIView : Tween Remove" ) ;	// アンドウバッファに登録
						view.RemoveTweenIdentity = tweens[ m_RemoveTweenIndexAnswer ].Identity ;
						view.RemoveTweenInstance = tweens[ m_RemoveTweenIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( view ) ;
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

		// Filipper の追加と削除
		private string m_AddFlipperIdentity = "" ;
		private int    m_RemoveFlipperIndex = 0 ;
		private int    m_RemoveFlipperIndexAnswer = -1 ;

		protected void DrawFlipper( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			var flippers = view.GetComponents<UIFlipper>() ;

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
							var flipper = view.AddComponent<UIFlipper>() ;
							flipper.Identity = m_AddFlipperIdentity ;

							var existingFlippers = view.gameObject.GetComponents<UIFlipper>() ;
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
						Undo.RecordObject( view, "UIView : Flipper Remove" ) ;	// アンドウバッファに登録
						view.RemoveFlipperIdentity = flippers[ m_RemoveFlipperIndexAnswer ].Identity ;
						view.RemoveFlipperInstance = flippers[ m_RemoveFlipperIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( view ) ;
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

		// エフェクト系のコンポーネントを追加するかどうか
		protected void DrawEffect( UIView view )
		{
			if( view.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool isMask = EditorGUILayout.Toggle( view.IsMask, GUILayout.Width( 16f ) ) ;
					if( isMask != view.IsMask )
					{
						Undo.RecordObject( view, "UIView : Mask Change" ) ;	// アンドウバッファに登録
						view.IsMask = isMask ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( new GUIContent( "Mask", "<color=#00FFFF>Mask</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ) ) ;

					bool isInversion = EditorGUILayout.Toggle( view.IsInversion, GUILayout.Width( 16f ) ) ;
					if( isInversion != view.IsInversion )
					{
						Undo.RecordObject( view, "UIView : Inversion Change" ) ;	// アンドウバッファに登録
						view.IsInversion = isInversion ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( new GUIContent( "Inversion", "<color=#00FFFF>UIInversion</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\nこのビューで表示される<color=#00FFFF>スプライト・テクスチャ</color>を\n<color=#00FF00>左右上下の反転</color>や<color=#00FF00>90度単位の回転</color>を行って表示します" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				//-----------------------------------------------------------------------------------------

				bool isShow = true ;

				if( view.GetComponent<TextMeshProUGUI>() != null )
				{
					isShow = false ;
				}

				if( isShow == true )
				{
					EditorGUIUtility.labelWidth =  60f ;
					EditorGUIUtility.fieldWidth =  40f ;

					GUILayout.BeginHorizontal() ;	// 横並び
					{
		//				GUILayout.Label( "UI Effect" ) ;

						bool isShadow = EditorGUILayout.Toggle( view.IsShadow, GUILayout.Width( 16f ) ) ;
						if( isShadow != view.IsShadow )
						{
							Undo.RecordObject( view, "UIView : Shadow Change" ) ;	// アンドウバッファに登録
							view.IsShadow = isShadow ;
							EditorUtility.SetDirty( view ) ;
						}
						GUILayout.Label( "Shadow" ) ;

						bool isOutline = EditorGUILayout.Toggle( view.IsOutline, GUILayout.Width( 16f ) ) ;
						if( isOutline != view.IsOutline )
						{
							Undo.RecordObject( view, "UIView : Outline Change" ) ;	// アンドウバッファに登録
							view.IsOutline = isOutline ;
							EditorUtility.SetDirty( view ) ;
						}
						GUILayout.Label( "Outline" ) ;

						bool isGradient = EditorGUILayout.Toggle( view.IsGradient, GUILayout.Width( 16f ) ) ;
						if( isGradient != view.IsGradient )
						{
							Undo.RecordObject( view, "UIView : Gradient Change" ) ;	// アンドウバッファに登録
							view.IsGradient = isGradient ;
							EditorUtility.SetDirty( view ) ;
						}
						GUILayout.Label( "Gradient" ) ;
					}
					GUILayout.EndHorizontal() ;     // 横並び終了
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;
			}
		}

		// マテリアルの描画
		protected void DrawMaterial( UIView view )
		{
			if( view.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// タイプ
				UIView.MaterialTypes materialType = ( UIView.MaterialTypes )EditorGUILayout.EnumPopup( "Material Type",  view.MaterialType ) ;
				if( materialType != view.MaterialType )
				{
					Undo.RecordObject( view, "UIView : Material Type Change" ) ;	// アンドウバッファに登録
					view.MaterialType = materialType ;
					EditorUtility.SetDirty( view ) ;
				}

				// Sepia
				if( view.MaterialType == UIView.MaterialTypes.Sepia )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float sepiaDark = EditorGUILayout.Slider( "Sepia Dark", view.SepiaDark, 0.0f, 0.2f ) ;
					if( sepiaDark != view.SepiaDark )
					{
						Undo.RecordObject( view, "UIView : Sepia Dark Change" ) ;	// アンドウバッファに登録
						view.SepiaDark = sepiaDark ;
						EditorUtility.SetDirty( view ) ;
					}

					float sepiaStrength = EditorGUILayout.Slider( "Sepia Strength", view.SepiaStrength, 0.05f, 0.15f ) ;
					if( sepiaStrength != view.SepiaStrength )
					{
						Undo.RecordObject( view, "UIView : Sepia Strength Change" ) ;	// アンドウバッファに登録
						view.SepiaStrength = sepiaStrength ;
						EditorUtility.SetDirty( view ) ;
					}

					float sepiaInterpolation = EditorGUILayout.Slider( "Sepia Intepolation", view.SepiaInterpolation, 0f, 1f ) ;
					if( sepiaInterpolation != view.SepiaInterpolation )
					{
						Undo.RecordObject( view, "UIView : Sepia Interpolation Change" ) ;	// アンドウバッファに登録
						view.SepiaInterpolation = sepiaInterpolation ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				// Interpolation
				if( view.MaterialType == UIView.MaterialTypes.Interpolation )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float interpolationValue = EditorGUILayout.Slider( "Interpolation Value", view.InterpolationValue, 0.0f, 1.0f ) ;
					if( interpolationValue != view.InterpolationValue )
					{
						Undo.RecordObject( view, "UIView : Interpolation Value Change" ) ;	// アンドウバッファに登録
						view.InterpolationValue = interpolationValue ;
						EditorUtility.SetDirty( view ) ;
					}

					var interpolationColor = new Color( view.InterpolationColor.r, view.InterpolationColor.g, view.InterpolationColor.b, view.InterpolationColor.a ) ;
					interpolationColor = EditorGUILayout.ColorField( "Interpolation Color", interpolationColor ) ;
					if( interpolationColor.Equals( view.InterpolationColor ) == false )
					{
						Undo.RecordObject( view, "UIView : Interpolation Color Change" ) ;	// アンドウバッファに登録
						view.InterpolationColor = interpolationColor ;
						EditorUtility.SetDirty( view ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				// Mosaic
				if( view.MaterialType == UIView.MaterialTypes.Mosaic )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float mosaicIntensity = EditorGUILayout.Slider( "Mosaic Intensity", view.MosaicIntensity, 0.0f, 1.0f ) ;
					if( mosaicIntensity != view.MosaicIntensity )
					{
						Undo.RecordObject( view, "UIView : Mosaic Intensity Change" ) ;	// アンドウバッファに登録
						view.MosaicIntensity = mosaicIntensity ;
						EditorUtility.SetDirty( view ) ;
					}

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool mosaicSquareization = EditorGUILayout.Toggle( view.MosaicSquareization, GUILayout.Width( 16f ) ) ;
						if( mosaicSquareization != view.MosaicSquareization )
						{
							Undo.RecordObject( view, "UIView : Mosaic Squareization Change" ) ;	// アンドウバッファに登録
							view.MosaicSquareization = mosaicSquareization ;
							EditorUtility.SetDirty( view ) ;
						}
						GUILayout.Label( "Mosaic Squareization" ) ;
					}
					GUILayout.EndHorizontal() ;     // 横並び終了


					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
		}

		// 子の色
		protected void DrawChildrenColor( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isApplyColorToChildren = EditorGUILayout.Toggle( view.IsApplyColorToChildren, GUILayout.Width( 16f ) ) ;
				if( isApplyColorToChildren != view.IsApplyColorToChildren )
				{
					Undo.RecordObject( view, "UIView : Is Apply Color To Children Change" ) ;	// アンドウバッファに登録
					view.IsApplyColorToChildren = isApplyColorToChildren ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Is Apply Color To Children", "このビューでのランタイム実行中の<color=#00FFFF>カラー変化を子ビューにも反映</color>させます\n<color=#FFFF00>例としては、ボタン押下時やインタラクション無効化時のカラー変化です</color>" ) ) ;
			}
			GUILayout.EndHorizontal() ;     // 横並び終了

			if( view.IsApplyColorToChildren == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool effeciveColorReplcaing = EditorGUILayout.Toggle( view.EffectiveColorReplacing, GUILayout.Width( 16f ) ) ;
					if( effeciveColorReplcaing != view.EffectiveColorReplacing )
					{
						Undo.RecordObject( view, "UIView : Effective Color Replacing Change" ) ;	// アンドウバッファに登録
						view.EffectiveColorReplacing = effeciveColorReplcaing ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Effective Color Replacing" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				if( view.EffectiveColorReplacing == false )
				{
					// 影響色
					var effectiveColor = new Color( view.EffectiveColor.r, view.EffectiveColor.g, view.EffectiveColor.b, view.EffectiveColor.a ) ;
					effectiveColor = EditorGUILayout.ColorField( "Effective Color", effectiveColor ) ;
					if( effectiveColor.Equals( view.EffectiveColor ) == false )
					{
						Undo.RecordObject( view, "UIView : Effective Color Change" ) ;	// アンドウバッファに登録
						view.EffectiveColor = effectiveColor ;
						EditorUtility.SetDirty( view ) ;
					}
				}
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool ignoreParentEffectiveColor = EditorGUILayout.Toggle( view.IgnoreParentEffectiveColor, GUILayout.Width( 16f ) ) ;
				if( ignoreParentEffectiveColor != view.IgnoreParentEffectiveColor )
				{
					Undo.RecordObject( view, "UIView : Ignore Parent Effective Color Change" ) ;	// アンドウバッファに登録
					view.IgnoreParentEffectiveColor = ignoreParentEffectiveColor ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( "Ignore Parent Effective Color" ) ;
			}
			GUILayout.EndHorizontal() ;     // 横並び終了

		}

		// バックキー
		protected void DrawBackKey( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool backKeyEnabled = EditorGUILayout.Toggle( view.BackKeyEnabled, GUILayout.Width( 16f ) ) ;
				if( backKeyEnabled != view.BackKeyEnabled )
				{
					Undo.RecordObject( view, "UIImage : Back Key Enabled Change" ) ;	// アンドウバッファに登録
					view.BackKeyEnabled = backKeyEnabled ;
					EditorUtility.SetDirty( view ) ;
				}
				GUILayout.Label( new GUIContent( "Back Key Enabled", "<color=#00FFFF>Androidバックキー(ESCキー)</color>が押された際に\n<color=#00FF00>OnClick OnSimpleClick</color>コールバックを呼び出すようにします\n<color=#FFFF00>※UIInteractionコンポーネント及びRaycastTargetの有効化が必要です</color>" ) ) ;

				if( view.BackKeyEnabled == true )
				{
					bool isBackKeyIgnoreRaycastTarget = EditorGUILayout.Toggle( view.IsBackKeyIgnoreRaycastTarget, GUILayout.Width( 16f ) ) ;
					if( isBackKeyIgnoreRaycastTarget != view.IsBackKeyIgnoreRaycastTarget )
					{
						Undo.RecordObject( view, "UIImage : Is Back Key Ignore RaycastTarget Change" ) ;	// アンドウバッファに登録
						view.IsBackKeyIgnoreRaycastTarget = isBackKeyIgnoreRaycastTarget ;
						EditorUtility.SetDirty( view ) ;
					}
					GUILayout.Label( "Ignore RaycastTarget" ) ;
				}
			}
			GUILayout.EndHorizontal() ;     // 横並び終了
		}

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( UIView view )
		{
			UIImage image ;
			if( view is UIImage )
			{
				image = view as UIImage ;
			}
			else
			{
				return ;
			}

			//----------------------------------------------------------

			// スプライトアトラス
			SpriteAtlas spriteAtlas = EditorGUILayout.ObjectField( new GUIContent( "Sprite Atlas", "<color=#00FFFF>SpriteAtlas</color>アセットを設定します\nランタイム実行中、<color=#00FFFF>SetSpriteInAtlas</color>メソッドを使用する事により\n表示する<color=#00FFFF>Spriteを動的に切り替える</color>事が出来ます" ), image.SpriteAtlas, typeof( SpriteAtlas ), false ) as SpriteAtlas ;
			if( spriteAtlas != image.SpriteAtlas )
			{
				Undo.RecordObject( image, "Sprite Atlas : Change" ) ;	// アンドウバッファに登録

				// SpriteAtlas 側を設定する
				image.SpriteAtlas = spriteAtlas ;

				// SpriteSet 側を消去する
				image.SpriteSet = null ;

				if( image.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					Sprite[] sprites = GetSprites( image.SpriteAtlas ) ;
					if( sprites != null && sprites.Length >  0 )
					{
						image.Sprite = sprites[ 0 ] ;
					}
				}

				EditorUtility.SetDirty( image ) ;
			}

			if( image.SpriteAtlas != null )
			{
				// スプライトアトラスのテクスチャ(表示のみ)
//				EditorGUILayout.ObjectField( "Sprite Atlas Texture", image.SpriteAtlasTexture, typeof( Texture2D ), true ) ;

				//---------------------------------

				Sprite[] sprites = GetSprites( image.SpriteAtlas ) ;
				if( sprites != null && sprites.Length >  0 )
				{
					int i, l = sprites.Length ;
					var spriteNames = new List<string>() ;
					foreach( var sprite in sprites )
					{
						spriteNames.Add( sprite.name ) ;
					}

					string currentSpriteName = null ;
					if( image.Sprite != null )
					{
						currentSpriteName = image.Sprite.name ;
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
						Undo.RecordObject( image, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
						image.Sprite = sprites[ index - indexMove ] ;
						EditorUtility.SetDirty( image ) ;
					}
				}

				// 確認用
				EditorGUILayout.ObjectField( " ", image.Sprite, typeof( Sprite ), false ) ;
			}

			//----------------------------------------------------------
			// 以下はレガシー

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			Texture spriteSetTextureActive = null ;
			if( image.SpriteSet != null )
			{
				spriteSetTextureActive= image.SpriteSet.Texture ;
			}

			Texture spriteSetTextureChange = EditorGUILayout.ObjectField( "Sprite Set", spriteSetTextureActive, typeof( Texture ), false ) as Texture ;
			if( spriteSetTextureChange != spriteSetTextureActive )
			{
				Undo.RecordObject( image, "SpriteSet Texture : Change" ) ;	// アンドウバッファに登録

				// SpriteSet 側を設定する
				RefreshSpriteSet( image, spriteSetTextureChange ) ;

				// SpriteAtlas 側を消去する
				image.SpriteAtlas = null ;

				if( image.Sprite == null )
				{
					// スプライトが設定されていなければデフォルトを設定する
					Sprite[] sprites = image.SpriteSet.GetSprites() ;
					if( sprites != null && sprites.Length >  0 )
					{
						image.Sprite = sprites[ 0 ] ;
					}
				}
				EditorUtility.SetDirty( image ) ;
			}

			if( image.SpriteSet != null )
			{
				spriteSetTextureActive = image.SpriteSet.Texture ;
			}

			if( spriteSetTextureActive != null )
			{
				//-----------------------------------------------------

				// 一覧から選択出来るようにする

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload Sprites In SpriteSet", GUILayout.Width( 240f ) ) == true || ( image.SpriteSet.IsAvailable == false && image.SpriteSet.Texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					if( image.SpriteSet.IsAvailable == false && image.SpriteSet.Texture != null && Application.isPlaying == false )
					{
						Debug.LogWarning( "SpriteSet に内包される Sprites を自動的に更新:" + image.SpriteSet.Texture.name ) ;
					}

					RefreshSpriteSet( image, image.SpriteSet.Texture ) ;

					EditorUtility.SetDirty( image ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				string[] spriteNames = image.SpriteSet.GetSpriteNames() ;
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
					if( image.Sprite != null )
					{
						currentSpriteName = image.Sprite.name ;
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
						Undo.RecordObject( image, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
						image.SetSpriteInAtlas( spriteNames[ index ] ) ;
						EditorUtility.SetDirty( image ) ;
					}

					// 確認用
					EditorGUILayout.ObjectField( " ", image.Sprite, typeof( Sprite ), false ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------------------------------

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
		private void RefreshSpriteSet( UIImage image, Texture atlasTexture )
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
					image.SpriteSet.ClearSprites() ;
					image.SpriteSet.SetSprites( targetSprites.ToArray() ) ;
				}
				else
				{
					// 存在しないのでクリアする
					image.SpriteSet?.ClearSprites() ;
				}

				// SpriteAtlas 側を消去する
				image.SpriteAtlas = null ;
			}
			else
			{
				image.SpriteSet?.ClearSprites() ;
			}
		}

		//--------------------------------------------------------------------------------

		// 整数タイプの４次元値の表示と選択
		private Vector4 Float4Field( string prefix, float prefixLength, string l0, string l1, string l2, string l3, float captionLength, Vector4 vi )
		{
			var vo = new Vector4() ;

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( string.IsNullOrEmpty( prefix ) )
				{
					if( prefixLength >  0 )
					{
						GUILayout.Space( prefixLength ) ;        // null なら 82
					}
				}
				else
				{
					GUILayout.Label( prefix, GUILayout.Width( prefixLength ) ) ;	// null でないなら 74
				}

				EditorGUIUtility.labelWidth = captionLength ;
				EditorGUIUtility.fieldWidth =  50f ;

				vo.x = ( float )EditorGUILayout.FloatField( l0, ( int )vi.x, GUILayout.MinWidth( 10f ) ) ;
				vo.y = ( float )EditorGUILayout.FloatField( l1, ( int )vi.y, GUILayout.MinWidth( 10f ) ) ;
				vo.z = ( float )EditorGUILayout.FloatField( l2, ( int )vi.z, GUILayout.MinWidth( 10f ) ) ;
				vo.w = ( float )EditorGUILayout.FloatField( l3, ( int )vi.w, GUILayout.MinWidth( 10f ) ) ;

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  50f ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			return vo ;
		}

		protected Color CloneColor( Color c1 )
		{
			return new Color( c1.r, c1.g, c1.b, c1.a ) ;
		}

		protected bool CheckColor( Color c1, Color c2 )
		{
			if( c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a )
			{
				return false ;
			}

			return true ;
		}

		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "RemoveTweenOK?",   "Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?", "Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
			{ "InputIdentity",   "識別子を入力してください" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "RemoveTweenOK?",   "It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?", "It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
			{ "InputIdentity",   "Input identity !" },
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
