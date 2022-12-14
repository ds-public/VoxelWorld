using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

//#if TextMeshPro
using TMPro ;
//#endif

namespace uGUIHelper
{
	/// <summary>
	/// UIView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIView ) ) ]
	public class UIViewInspector : Editor
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
			UIView tTarget = target as UIView ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-----------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity", tTarget.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity !=  tTarget.Identity )
			{
				Undo.RecordObject( tTarget, "UIView : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.Identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			//------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( tTarget ) ;
		
			// イベントトリガーを有効にするかどうか
			DrawEventTrigger( tTarget ) ;

			// インタラクション
			DrawInteraction( tTarget ) ;

			// トランジション
			DrawTransition( tTarget ) ;

			// Tween の追加と削除
			DrawTween( tTarget ) ;

			//------------------------------------------
		
			DrawInspectorGUI() ;

			//------------------------------------------
		
			DrawEffect( tTarget ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//------------------------------------------
		
			// トグルカテゴリ
			DrawToggleCategory( tTarget ) ;

			// レクトマスト２Ｄ
			DrawRectMask2D( tTarget ) ;

			// アルファマスク
			DrawAlphaMask( tTarget ) ;

			// コンテンツサイスフィッター
			DrawContentSizeFitter( tTarget ) ;
		}
	
		// 派生クラスの個々のＧＵＩを描画する
		virtual protected void DrawInspectorGUI(){}

		// キャンバスグループの設定項目を描画する
		protected void DrawCanvasGroup( UIView tTarget )
		{
			// キャンバスグループを有効にするかどうか

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isCanvasGroup = EditorGUILayout.Toggle( tTarget.IsCanvasGroup, GUILayout.Width( 16f ) ) ;
				if( isCanvasGroup != tTarget.IsCanvasGroup )
				{
					Undo.RecordObject( tTarget, "UIView : Canvas Group Change" ) ;	// アンドウバッファに登録
					tTarget.IsCanvasGroup = isCanvasGroup ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Canvas Group" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.IsCanvasGroup == true )
			{
				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float alpha = EditorGUILayout.Slider( "Alpha", tTarget.GetCanvasGroup().alpha, 0.0f, 1.0f ) ;
				if( alpha != tTarget.GetCanvasGroup().alpha )
				{
					Undo.RecordObject( tTarget, "UIView : Alpha Change" ) ;	// アンドウバッファに登録
					tTarget.GetCanvasGroup().alpha = alpha ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;

				// スライダーでアルファをコントロール出来るようにする
				float tDisableRaycastUnderAlpha = EditorGUILayout.Slider( "Disable Raycast Under Alpha", tTarget.disableRaycastUnderAlpha, 0.0f, 1.0f ) ;
				if( tDisableRaycastUnderAlpha != tTarget.disableRaycastUnderAlpha )
				{
					Undo.RecordObject( tTarget, "UIView : Disable Raycast Under Alpha Change" ) ;	// アンドウバッファに登録
					tTarget.disableRaycastUnderAlpha = tDisableRaycastUnderAlpha ;
					EditorUtility.SetDirty( tTarget ) ;
				}

			}
		}

		// イベントトリガーの生成破棄チェックボックスを描画する
		protected void DrawEventTrigger( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsEventTrigger = EditorGUILayout.Toggle( tTarget.isEventTrigger, GUILayout.Width( 16f ) ) ;
				if( tIsEventTrigger != tTarget.isEventTrigger )
				{
					Undo.RecordObject( tTarget, "UIView : EventTrigger Change" ) ;	// アンドウバッファに登録
					tTarget.isEventTrigger = tIsEventTrigger ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "EventTrigger" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// インタラクションの生成破棄チェックボックスを描画する
		protected void DrawInteraction( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsInteraction = EditorGUILayout.Toggle( tTarget.isInteraction, GUILayout.Width( 16f ) ) ;
				if( tIsInteraction != tTarget.isInteraction )
				{
					Undo.RecordObject( tTarget, "UIView : Interaction Change" ) ;	// アンドウバッファに登録
					tTarget.isInteraction = tIsInteraction ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Interaction" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsInteractionForScrollView = EditorGUILayout.Toggle( tTarget.isInteractionForScrollView, GUILayout.Width( 16f ) ) ;
				if( tIsInteractionForScrollView != tTarget.isInteractionForScrollView )
				{
					Undo.RecordObject( tTarget, "UIView : Interaction For ScrollView Change" ) ;	// アンドウバッファに登録
					tTarget.isInteractionForScrollView = tIsInteractionForScrollView ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Interaction For ScrollView" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

		}

		// トランジションの生成破棄チェックボックスを描画する
		protected void DrawTransition( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsTransition = EditorGUILayout.Toggle( tTarget.isTransition, GUILayout.Width( 16f ) ) ;
				if( tIsTransition != tTarget.isTransition )
				{
					Undo.RecordObject( tTarget, "UIView : Transition Change" ) ;	// アンドウバッファに登録
					tTarget.isTransition = tIsTransition ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Transition" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsTransitionForScrollView = EditorGUILayout.Toggle( tTarget.isTransitionForScrollView, GUILayout.Width( 16f ) ) ;
				if( tIsTransitionForScrollView != tTarget.isTransitionForScrollView )
				{
					Undo.RecordObject( tTarget, "UIView : Transition For ScrollView Change" ) ;	// アンドウバッファに登録
					tTarget.isTransitionForScrollView = tIsTransitionForScrollView ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Transition For ScrollView" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

		}

		// レクトマスク２Ｄの生成破棄チェックボックスを描画する
		protected void DrawRectMask2D( UIView tTarget )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsRectMask2D = EditorGUILayout.Toggle( tTarget.isRectMask2D, GUILayout.Width( 16f ) ) ;
				if( tIsRectMask2D != tTarget.isRectMask2D )
				{
					Undo.RecordObject( tTarget, "UIView : RectMask2D Change" ) ;	// アンドウバッファに登録
					tTarget.isRectMask2D = tIsRectMask2D ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "RectMask2D" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// アルファマスクの生成破棄チェックボックスを描画する
		protected void DrawAlphaMask( UIView tTarget )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsAlphaMaskWindow = EditorGUILayout.Toggle( tTarget.isAlphaMaskWindow, GUILayout.Width( 16f ) ) ;
				if( tIsAlphaMaskWindow != tTarget.isAlphaMaskWindow )
				{
					Undo.RecordObject( tTarget, "UIView : AlphaMaskWindow Change" ) ;	// アンドウバッファに登録
					tTarget.isAlphaMaskWindow = tIsAlphaMaskWindow ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskWindow" ) ;

				bool tIsAlphaMaskTarget = EditorGUILayout.Toggle( tTarget.isAlphaMaskTarget, GUILayout.Width( 16f ) ) ;
				if( tIsAlphaMaskTarget != tTarget.isAlphaMaskTarget )
				{
					Undo.RecordObject( tTarget, "UIView : AlphaMaskTarget Change" ) ;	// アンドウバッファに登録
					tTarget.isAlphaMaskTarget = tIsAlphaMaskTarget ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "AlphaMaskTarget" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// コンテントサイズフィッターの生成破棄チェックボックスを描画する
		protected void DrawContentSizeFitter( UIView tTarget )
		{
			// オブジェクトの維持フラグ
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsContentSizeFitter = EditorGUILayout.Toggle( tTarget.isContentSizeFitter, GUILayout.Width( 16f ) ) ;
				if( tIsContentSizeFitter != tTarget.isContentSizeFitter )
				{
					Undo.RecordObject( tTarget, "UIView : ContentSizeFitter Change" ) ;	// アンドウバッファに登録
					tTarget.isContentSizeFitter = tIsContentSizeFitter ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ContentSizeFillter" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		// トグルグループの生成破棄チェックボックスを描画する
		protected void DrawToggleCategory( UIView view )
		{
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isToggleGroup = EditorGUILayout.Toggle( view.isToggleGroup, GUILayout.Width( 16f ) ) ;
				if( isToggleGroup != view.isToggleGroup )
				{
					Undo.RecordObject( view, "UIView : ToggleGroup Change" ) ;	// アンドウバッファに登録
					view.isToggleGroup = isToggleGroup ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "ToggleGroup" ) ;

				bool isToggle = EditorGUILayout.Toggle( view.isToggle, GUILayout.Width( 16f ) ) ;
				if( isToggle != view.isToggle )
				{
					Undo.RecordObject( view, "UIView : Toggle Change" ) ;	// アンドウバッファに登録
					view.isToggle = isToggle ;
					EditorUtility.SetDirty( view ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Toggle" ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}



		// アニメーターを加えるかどうか


		// Tween の追加と削除
		private string mAddTweenIdentity = "" ;
		private int    mRemoveTweenIndex = 0 ;
		private int    mRemoveTweenIndexAnswer = -1 ;

		protected void DrawTween( UIView view )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			UITween[] tweens = view.GetComponents<UITween>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tweens.Length, j, c ;
			string identity ;
			string[] tweenIdentities = new string[ l ] ;
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

			if( mRemoveTweenIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool isAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Tween", GUILayout.Width( 140f ) ) == true )
					{
						isAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					mAddTweenIdentity = EditorGUILayout.TextField( "", mAddTweenIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( isAdd == true )
					{
						if( string.IsNullOrEmpty( mAddTweenIdentity ) == false )
						{
							// Tween を追加する
							UITween tween = view.AddComponent<UITween>() ;
							tween.Identity = mAddTweenIdentity ;
	
							// 追加後の全ての Tween を取得する
							UITween[] temporaryTweens = view.gameObject.GetComponents<UITween>() ;
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

						if( mRemoveTweenIndex >= tweenIdentities.Length )
						{
							mRemoveTweenIndex  = tweenIdentities.Length - 1 ;
						}
						mRemoveTweenIndex = EditorGUILayout.Popup( "", mRemoveTweenIndex, tweenIdentities, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
				
						if( isRemove == true )
						{
							// 削除する
							mRemoveTweenIndexAnswer = mRemoveTweenIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				string message = GetMessage( "RemoveTweenOK?" ).Replace( "%1", tweenIdentities[ mRemoveTweenIndexAnswer ] ) ;
				GUILayout.Label( message ) ;
	//			GUILayout.Label( "It does really may be to remove tween '" + tTweenIdentityArray[ mRemoveTweenIndexAnswer ] + "' ?" ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( view, "UIView : Tween Remove" ) ;	// アンドウバッファに登録
						view.RemoveTweenIdentity = tweens[ mRemoveTweenIndexAnswer ].Identity ;
						view.RemoveTweenInstance = tweens[ mRemoveTweenIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( view ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						mRemoveTweenIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						mRemoveTweenIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}



		// Filipper の追加と削除
		private string mAddFlipperIdentity = "" ;
		private int    mRemoveFlipperIndex = 0 ;
		private int    mRemoveFlipperIndexAnswer = -1 ;

		protected void DrawFlipper( UIView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			UIFlipper[] tFlipperArray = tTarget.GetComponents<UIFlipper>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tFlipperArray.Length, j, c ;
			string tIdentity ;
			string[] tFlipperIdentityArray = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tFlipperIdentityArray[ i ] = tFlipperArray[ i ].identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				tIdentity = tFlipperIdentityArray[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( tFlipperIdentityArray[ j ] == tIdentity )
					{
						// 同じ名前を発見した
						c ++ ;
						tFlipperIdentityArray[ j ] = tFlipperIdentityArray[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( mRemoveFlipperIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool tAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Flipper", GUILayout.Width( 140f ) ) == true )
					{
						tAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					mAddFlipperIdentity = EditorGUILayout.TextField( "", mAddFlipperIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( tAdd == true )
					{
						if( string.IsNullOrEmpty( mAddFlipperIdentity ) == false )
						{
							// Flipper を追加する
							UIFlipper tFlipper = tTarget.AddComponent<UIFlipper>() ;
							tFlipper.identity = mAddFlipperIdentity ;
	
							UIFlipper[] tFlipperList = tTarget.gameObject.GetComponents<UIFlipper>() ;
							if( tFlipperList != null && tFlipperList.Length >  0 )
							{
								for( i  = 0 ; i <  tFlipperList.Length ; i ++ )
								{
									if( tFlipperList[ i ] != tFlipper )
									{
										break ;
									}
								}
								if( i <  tFlipperList.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									tFlipper.enabled = false ;
									tFlipper.playOnAwake = false ;
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

				if( tFlipperArray != null && tFlipperArray.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool tRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Flipper", GUILayout.Width( 140f ) ) == true )
						{
							tRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( mRemoveFlipperIndex >= tFlipperIdentityArray.Length )
						{
							mRemoveFlipperIndex  = tFlipperIdentityArray.Length - 1 ;
						}
						mRemoveFlipperIndex = EditorGUILayout.Popup( "", mRemoveFlipperIndex, tFlipperIdentityArray, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
				
						if( tRemove == true )
						{
							// 削除する
							mRemoveFlipperIndexAnswer = mRemoveFlipperIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				string tMessage = GetMessage( "RemoveFlipperOK?" ).Replace( "%1", tFlipperIdentityArray[ mRemoveFlipperIndexAnswer ] ) ;
				GUILayout.Label( tMessage ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( tTarget, "UIView : Flipper Remove" ) ;	// アンドウバッファに登録
						tTarget.RemoveFlipperIdentity = tFlipperArray[ mRemoveFlipperIndexAnswer ].identity ;
						tTarget.RemoveFlipperInstance = tFlipperArray[ mRemoveFlipperIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( tTarget ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						mRemoveFlipperIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						mRemoveFlipperIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		// エフェクト系のコンポーネントを追加するかどうか
		protected void DrawEffect( UIView tTarget )
		{
			if( tTarget.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsMask = EditorGUILayout.Toggle( tTarget.isMask, GUILayout.Width( 16f ) ) ;
					if( tIsMask != tTarget.isMask )
					{
						Undo.RecordObject( tTarget, "UIView : Mask Change" ) ;	// アンドウバッファに登録
						tTarget.isMask = tIsMask ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Mask" ) ;

					bool tIsInversion = EditorGUILayout.Toggle( tTarget.isInversion, GUILayout.Width( 16f ) ) ;
					if( tIsInversion != tTarget.isInversion )
					{
						Undo.RecordObject( tTarget, "UIView : Inversion Change" ) ;	// アンドウバッファに登録
						tTarget.isInversion = tIsInversion ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Inversion" ) ;

				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				//-----------------------------------------------------------------------------------------

				bool tShow = true ;

//#if TextMeshPro
				if( tTarget.GetComponent<TextMeshProUGUI>() != null )
				{
					tShow = false ;
				}
//#endif

				if( tShow == true )
				{
					EditorGUIUtility.labelWidth =  60f ;
					EditorGUIUtility.fieldWidth =  40f ;

					GUILayout.BeginHorizontal() ;	// 横並び
					{
		//				GUILayout.Label( "UI Effect" ) ;

						bool tIsShadow = EditorGUILayout.Toggle( tTarget.isShadow, GUILayout.Width( 16f ) ) ;
						if( tIsShadow != tTarget.isShadow )
						{
							Undo.RecordObject( tTarget, "UIView : Shadow Change" ) ;	// アンドウバッファに登録
							tTarget.isShadow = tIsShadow ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Shadow" ) ;

						bool tIsOutline = EditorGUILayout.Toggle( tTarget.isOutline, GUILayout.Width( 16f ) ) ;
						if( tIsOutline != tTarget.isOutline )
						{
							Undo.RecordObject( tTarget, "UIView : Outline Change" ) ;	// アンドウバッファに登録
							tTarget.isOutline = tIsOutline ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Outline" ) ;


						bool tIsGradient = EditorGUILayout.Toggle( tTarget.isGradient, GUILayout.Width( 16f ) ) ;
						if( tIsGradient != tTarget.isGradient )
						{
							Undo.RecordObject( tTarget, "UIView : Gradient Change" ) ;	// アンドウバッファに登録
							tTarget.isGradient = tIsGradient ;
							EditorUtility.SetDirty( tTarget ) ;
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
		protected void DrawMaterial( UIView tTarget )
		{
			if( tTarget.GetCanvasRenderer() != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// タイプ
				UIView.MaterialTypes materialType = ( UIView.MaterialTypes )EditorGUILayout.EnumPopup( "Material Type",  tTarget.MaterialType ) ;
				if( materialType != tTarget.MaterialType )
				{
					Undo.RecordObject( tTarget, "UIView : Material Type Change" ) ;	// アンドウバッファに登録
					tTarget.MaterialType = materialType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.MaterialType == UIView.MaterialTypes.Interpolation )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float interpolationValue = EditorGUILayout.Slider( "Interpolation Value", tTarget.InterpolationValue, 0.0f, 1.0f ) ;
					if( interpolationValue != tTarget.InterpolationValue )
					{
						Undo.RecordObject( tTarget, "UIView : Interpolation Value Change" ) ;	// アンドウバッファに登録
						tTarget.InterpolationValue = interpolationValue ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					Color interpolationColor = new Color( tTarget.InterpolationColor.r, tTarget.InterpolationColor.g, tTarget.InterpolationColor.b, tTarget.InterpolationColor.a ) ;
					interpolationColor = EditorGUILayout.ColorField( "Interpolation Color", interpolationColor ) ;
					if( interpolationColor.Equals( tTarget.InterpolationColor ) == false )
					{
						Undo.RecordObject( tTarget, "UIView : Interpolation Color Change" ) ;	// アンドウバッファに登録
						tTarget.InterpolationColor = interpolationColor ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				if( tTarget.MaterialType == UIView.MaterialTypes.Mosaic )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float mosaicIntensity = EditorGUILayout.Slider( "Mosaic Intensity", tTarget.MosaicIntensity, 0.0f, 1.0f ) ;
					if( mosaicIntensity != tTarget.MosaicIntensity )
					{
						Undo.RecordObject( tTarget, "UIView : Mosaic Intensity Change" ) ;	// アンドウバッファに登録
						tTarget.MosaicIntensity = mosaicIntensity ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool mosaicSquareization = EditorGUILayout.Toggle( tTarget.MosaicSquareization, GUILayout.Width( 16f ) ) ;
						if( mosaicSquareization != tTarget.MosaicSquareization )
						{
							Undo.RecordObject( tTarget, "UIView : Mosaic Squareization Change" ) ;	// アンドウバッファに登録
							tTarget.MosaicSquareization = mosaicSquareization ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( "Mosaic Squareization" ) ;
					}
					GUILayout.EndHorizontal() ;     // 横並び終了


					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}

				if( tTarget.MaterialType == UIView.MaterialTypes.Blur )
				{
					EditorGUIUtility.labelWidth = 120f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float materialValue = EditorGUILayout.Slider( "Material Value", tTarget.MaterialValue, 0.0f, 1.0f ) ;
					if( materialValue != tTarget.MaterialValue )
					{
						Undo.RecordObject( tTarget, "UIView : Material Value Change" ) ;	// アンドウバッファに登録
						tTarget.MaterialValue = materialValue ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
		}

		// AtlasSprite の項目を描画する
		protected void DrawAtlas( UIView tView )
		{
			UIImage tTarget ;
			if( tView is UIImage )
			{
				tTarget = tView as UIImage ;
			}
			else
			{
				return ;
			}

			Texture atlasTextureBase = null ;
			if( tTarget.AtlasSprite != null )
			{
				atlasTextureBase = tTarget.AtlasSprite.texture ;
			}

			bool tAtlasTextureRefresh = false ;

			Texture atlasTexture = EditorGUILayout.ObjectField( "Atlas Texture", atlasTextureBase, typeof( Texture ), false ) as Texture ;
			if( atlasTexture != atlasTextureBase )
			{
				Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録

				RefreshAtlasSprite( tTarget, atlasTexture ) ;

				EditorUtility.SetDirty( tTarget ) ;

				tAtlasTextureRefresh = true ;
			}

			GUILayout.Label( "Atlas Path" ) ;
			GUILayout.BeginHorizontal() ;
			{
				GUI.color = Color.cyan ;
				GUILayout.Label( "Resources/" ) ;
				GUI.color = Color.white ;

				string atlasPath = EditorGUILayout.TextField( "", tTarget.AtlasSprite.path ) ;
				if( atlasPath != tTarget.AtlasSprite.path )
				{
					Undo.RecordObject( tTarget, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
					tTarget.AtlasSprite.path = atlasPath ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				atlasPath = "" ;
				if( tTarget.AtlasSprite.texture != null )
				{
					string path = AssetDatabase.GetAssetPath( tTarget.AtlasSprite.texture.GetInstanceID() ) ;
					if( System.IO.File.Exists( path ) == true )
					{
						string c = "/Resources/" ;
						int p = path.IndexOf( c ) ;
						if( p >= 0 )
						{
							// 有効なパス
							atlasPath = path.Substring( p + c.Length, path.Length - ( p + c.Length ) ) ;
							p = atlasPath.IndexOf( "." ) ;
							if( p >= 0 )
							{
								atlasPath = atlasPath.Substring( 0, p ) ;
							}
						}
					}
				}

				if( string.IsNullOrEmpty( atlasPath ) == false )
				{
					if( atlasPath != tTarget.AtlasSprite.path )
					{
						GUI.backgroundColor = Color.yellow ;
					}
					else
					{
						GUI.backgroundColor = Color.white ;
					}
					if( GUILayout.Button( "Set", GUILayout.Width( 50f ) ) == true || tAtlasTextureRefresh == true )
					{
						Undo.RecordObject( tTarget, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
						tTarget.AtlasSprite.path = atlasPath ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUI.backgroundColor = Color.white ;
				}
			}
			GUILayout.EndHorizontal() ;

			//-----------------------------------------------------
			
			// 一覧から選択出来るようにする


//			if( tTarget.atlasSprite != null && tTarget.atlasSprite.texture != null )
//			{
//				GUI.backgroundColor = Color.yellow ;
//				if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
//				{
//					Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録
//					RefreshAtlasSprite( tTarget, tTarget.atlasSprite.texture ) ;
//					EditorUtility.SetDirty( tTarget ) ;
//				}
//				GUI.backgroundColor = Color.white ;
//			}

			if( tTarget.AtlasSprite != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload", GUILayout.Width( 50f ) ) == true || ( tTarget.AtlasSprite.isAvailable == false && tTarget.AtlasSprite.texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					Debug.LogWarning( "Atlas を自動的に更新:" + tTarget.AtlasSprite.texture.name ) ;
					RefreshAtlasSprite( tTarget, tTarget.AtlasSprite.texture ) ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				string[] spriteNames = tTarget.AtlasSprite.GetNameList() ;
				if( spriteNames != null && spriteNames.Length >  0 )
				{
					// ソートする
					List<string> sortedSpriteNames = new List<string>() ;

					int i, l = spriteNames.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						sortedSpriteNames.Add( spriteNames[ i ] ) ;
					}
					sortedSpriteNames.Sort() ;
					spriteNames = sortedSpriteNames.ToArray() ;

					string currentSpriteName = null ;
					if( tTarget.Sprite != null )
					{
						currentSpriteName = tTarget.Sprite.name ;
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
						List<string> temporarySpriteNames = new List<string>()
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
						Undo.RecordObject( tTarget, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
						tTarget.SetSpriteInAtlas( spriteNames[ index ] ) ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					// 確認用
					Sprite tSprite = EditorGUILayout.ObjectField( "", tTarget.Sprite, typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) as Sprite ;
					if( tSprite != tTarget.Sprite )
					{
					}
				}
			}
		}

		private void RefreshAtlasSprite( UIImage tTarget, Texture atlasTexture )
		{
			List<Sprite> targetSprites = new List<Sprite>() ;

			if( atlasTexture != null )
			{
				string path = AssetDatabase.GetAssetPath( atlasTexture.GetInstanceID() ) ;

				// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
				UnityEngine.Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath( path ) ;

				if( allSprites != null && allSprites.Length >  0 )
				{
					int i, l = allSprites.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( allSprites[ i ] is UnityEngine.Sprite )
						{
							targetSprites.Add( allSprites[ i ] as UnityEngine.Sprite ) ;
						}
					}
				}

				if( targetSprites.Count >  0 )
				{
					// 存在するので更新する

//					UIAtlasSprite tAtlasSprite = UIAtlasSprite.Create() ;
					tTarget.AtlasSprite.Clear() ;
					tTarget.AtlasSprite.Set( targetSprites.ToArray() ) ;
//					tTarget.atlasSprite = tAtlasSprite ;
				}
				else
				{
					// 存在しないのでクリアする
					if(	tTarget.AtlasSprite != null )
					{
						tTarget.AtlasSprite.Clear() ;
					}
				}
			}
			else
			{
				if(	tTarget.AtlasSprite != null )
				{
					tTarget.AtlasSprite.Clear() ;
				}
			}
		}

		//--------------------------------------------------------------------------------

		// 整数タイプの４次元値の表示と選択
		private Vector4 Float4Field( string tPrefix, float tPrefixLength, string l0, string l1, string l2, string l3, float tCaptionLength, Vector4 vi )
		{
			Vector4 vo = new Vector4() ;
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( string.IsNullOrEmpty( tPrefix ) )
				{
					if( tPrefixLength >  0 )
					{
						GUILayout.Space( tPrefixLength ) ;        // null なら 82
					}
				}
				else
				{
					GUILayout.Label( tPrefix, GUILayout.Width( tPrefixLength ) ) ;	// null でないなら 74
				}
	
				EditorGUIUtility.labelWidth = tCaptionLength ;
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

		private readonly Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?", "Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
			{ "InputIdentity",   "識別子を入力してください" },
		} ;
		private readonly Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?", "It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
			{ "InputIdentity",   "Input identity !" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ tLabel ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ tLabel ] ;
			}
		}
	}
}

