using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UITween のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITween ) ) ]
	public class UITweenInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UITween tTarget = target as UITween ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string identity = EditorGUILayout.TextField( "Identity",  tTarget.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != tTarget.Identity )
			{
				Undo.RecordObject( tTarget, "UITween : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.Identity = identity ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float delay = EditorGUILayout.FloatField( "Delay", tTarget.Delay ) ;
			if( delay != tTarget.Delay )
			{
				Undo.RecordObject( tTarget, "UITween : Delay Change" ) ;	// アンドウバッファに登録
				tTarget.Delay = delay ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// デュアレーション
			float duration = EditorGUILayout.FloatField( "Duration", tTarget.Duration ) ;
			if( duration != tTarget.Duration )
			{
				Undo.RecordObject( tTarget, "UITween : Duration Change" ) ;	// アンドウバッファに登録
				tTarget.Duration = duration ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ワイドモードを有効にする
			bool tWideMode = EditorGUIUtility.wideMode ;
			EditorGUIUtility.wideMode = true ;


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.PositionEnabled == false )
				{
					GUILayout.Label( "Position Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.PositionFoldOut = EditorGUILayout.Foldout( tTarget.PositionFoldOut, "Position Enabled" ) ;
				}

				bool tPositionEnabled = EditorGUILayout.Toggle( tTarget.PositionEnabled, GUILayout.Width( 24f ) ) ;
				if( tPositionEnabled != tTarget.PositionEnabled )
				{
					Undo.RecordObject( tTarget, "UITween : Position Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.PositionEnabled = tPositionEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.PositionEnabled == true && tTarget.PositionFoldOut == true )
			{
				// ポジション

				Vector3 tPositionFrom = EditorGUILayout.Vector3Field( " From",  tTarget.PositionFrom /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionFrom != tTarget.PositionFrom )
				{
					Undo.RecordObject( tTarget, "UITween : Position From Change" ) ;	// アンドウバッファに登録
					tTarget.PositionFrom = tPositionFrom ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				Vector3 tPositionTo = EditorGUILayout.Vector3Field( " To",  tTarget.PositionTo /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionTo != tTarget.PositionTo )
				{
					Undo.RecordObject( tTarget, "UITween : Position To Change" ) ;	// アンドウバッファに登録
					tTarget.PositionTo = tPositionTo ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// プロセスタイプ
				UITween.ProcessTypes positionProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.PositionProcessType ) ;
				if( positionProcessType != tTarget.PositionProcessType )
				{
					Undo.RecordObject( tTarget, "UITween : Position Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.PositionProcessType = positionProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				if( tTarget.PositionProcessType == UITween.ProcessTypes.Ease )
				{
					// イーズタイプ
					UITween.EaseTypes positionEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.PositionEaseType ) ;
					if( positionEaseType != tTarget.PositionEaseType )
					{
						Undo.RecordObject( tTarget, "UITween : Position Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.PositionEaseType = positionEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				else
				if( tTarget.PositionProcessType == UITween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve( tTarget.PositionAnimationCurve.keys ) ;
					tTarget.PositionAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.PositionProcessType, tTarget.PositionEaseType, tTarget.PositionAnimationCurve ) ;
				}

				if( tTarget.GetComponent<RectTransform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
				}
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.RotationEnabled == false )
				{
					GUILayout.Label( "Rotation Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.RotationFoldOut = EditorGUILayout.Foldout( tTarget.RotationFoldOut, "Rotation Enabled" ) ;
				}

				bool tRotationEnabled = EditorGUILayout.Toggle( tTarget.RotationEnabled, GUILayout.Width( 24f ) ) ;
				if( tRotationEnabled != tTarget.RotationEnabled )
				{
					Undo.RecordObject( tTarget, "UITween : Rotation Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.RotationEnabled = tRotationEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.RotationEnabled == true && tTarget.RotationFoldOut == true )
			{
				// ローテーション

				Vector3 tRotationFrom = EditorGUILayout.Vector3Field( " From",  tTarget.RotationFrom ) ;
				if( tRotationFrom != tTarget.RotationFrom )
				{
					Undo.RecordObject( tTarget, "UITween : Rotation From Change" ) ;	// アンドウバッファに登録
					tTarget.RotationFrom = tRotationFrom ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				Vector3 tRotationTo = EditorGUILayout.Vector3Field( " To",  tTarget.RotationTo ) ;
				if( tRotationTo != tTarget.RotationTo )
				{
					Undo.RecordObject( tTarget, "UITween : Rotation To Change" ) ;	// アンドウバッファに登録
					tTarget.RotationTo = tRotationTo ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// プロセスタイプ
				UITween.ProcessTypes rotationProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.RotationProcessType ) ;
				if( rotationProcessType != tTarget.RotationProcessType )
				{
					Undo.RecordObject( tTarget, "UITween : Rotation Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.RotationProcessType = rotationProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				if( tTarget.RotationProcessType == UITween.ProcessTypes.Ease )
				{
					// イーズタイプ
					UITween.EaseTypes rotationEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.RotationEaseType ) ;
					if( rotationEaseType != tTarget.RotationEaseType )
					{
						Undo.RecordObject( tTarget, "UITween : Rotation Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.RotationEaseType = rotationEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				else
				if( tTarget.RotationProcessType == UITween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.RotationAnimationCurve.keys ) ;
					tTarget.RotationAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.RotationProcessType, tTarget.RotationEaseType, tTarget.RotationAnimationCurve ) ;
				}

				if( tTarget.GetComponent<RectTransform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
				}
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.ScaleEnabled == false )
				{
					GUILayout.Label( "Scale Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.ScaleFoldOut = EditorGUILayout.Foldout( tTarget.ScaleFoldOut, "Scale Enabled" ) ;
				}

				bool tScaleEnabled = EditorGUILayout.Toggle( tTarget.ScaleEnabled, GUILayout.Width( 24f ) ) ;
				if( tScaleEnabled != tTarget.ScaleEnabled )
				{
					Undo.RecordObject( tTarget, "UITween : Scale Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleEnabled = tScaleEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.ScaleEnabled == true && tTarget.ScaleFoldOut == true )
			{
				// スケール

				Vector3 tScaleFrom = EditorGUILayout.Vector3Field( " From",  tTarget.ScaleFrom ) ;
				if( tScaleFrom != tTarget.ScaleFrom )
				{
					Undo.RecordObject( tTarget, "UITween : Scale From Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleFrom = tScaleFrom ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				Vector3 tScaleTo = EditorGUILayout.Vector3Field( " To",  tTarget.ScaleTo ) ;
				if( tScaleTo != tTarget.ScaleTo )
				{
					Undo.RecordObject( tTarget, "UITween : Scale To Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleTo = tScaleTo ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// プロセスタイプ
				UITween.ProcessTypes scaleProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.ScaleProcessType ) ;
				if( scaleProcessType != tTarget.ScaleProcessType )
				{
					Undo.RecordObject( tTarget, "UITween : Scale Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleProcessType = scaleProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				if( tTarget.ScaleProcessType == UITween.ProcessTypes.Ease )
				{
					// イーズタイプ
					UITween.EaseTypes scaleEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.ScaleEaseType ) ;
					if( scaleEaseType != tTarget.ScaleEaseType )
					{
						Undo.RecordObject( tTarget, "UITween : Scale Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.ScaleEaseType = scaleEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				else
				if( tTarget.ScaleProcessType == UITween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.ScaleAnimationCurve.keys ) ;
					tTarget.ScaleAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.ScaleProcessType, tTarget.ScaleEaseType, tTarget.ScaleAnimationCurve ) ;
				}

				if( tTarget.GetComponent<RectTransform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "RectTransformNone" ), MessageType.Warning, true ) ;		
				}
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.AlphaEnabled == false )
				{
					GUILayout.Label( "Alpha Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.AlphaFoldOut = EditorGUILayout.Foldout( tTarget.AlphaFoldOut, "Alpha Enabled" ) ;
				}

				bool tAlphaEnabled = EditorGUILayout.Toggle( tTarget.AlphaEnabled, GUILayout.Width( 24f ) ) ;
				if( tAlphaEnabled != tTarget.AlphaEnabled )
				{
					Undo.RecordObject( tTarget, "UITween : Alpha Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.AlphaEnabled = tAlphaEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.AlphaEnabled == true && tTarget.AlphaFoldOut == true )
			{
				// アルファ

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( " From", GUILayout.Width( 40f ) ) ;

					float tAlphaFrom = EditorGUILayout.Slider( tTarget.AlphaFrom, 0, 1 ) ;
					if( tAlphaFrom != tTarget.AlphaFrom )
					{
						Undo.RecordObject( tTarget, "UITween : Alpha From Change" ) ;	// アンドウバッファに登録
						tTarget.AlphaFrom = tAlphaFrom ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( " To", GUILayout.Width( 40f ) ) ;

					float tAlphaTo = EditorGUILayout.Slider( tTarget.AlphaTo, 0, 1 ) ;
					if( tAlphaTo != tTarget.AlphaTo )
					{
						Undo.RecordObject( tTarget, "UITween : Alpha To Change" ) ;	// アンドウバッファに登録
						tTarget.AlphaTo = tAlphaTo ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// プロセスタイプ
				UITween.ProcessTypes alphaProcessType = ( UITween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.AlphaProcessType ) ;
				if( alphaProcessType != tTarget.AlphaProcessType )
				{
					Undo.RecordObject( tTarget, "UITween : Alpha Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.AlphaProcessType = alphaProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				if( tTarget.AlphaProcessType == UITween.ProcessTypes.Ease )
				{
					// イーズタイプ
					UITween.EaseTypes alphaEaseType = ( UITween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.AlphaEaseType ) ;
					if( alphaEaseType != tTarget.AlphaEaseType )
					{
						Undo.RecordObject( tTarget, "UITween : Alpha Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.AlphaEaseType = alphaEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				else
				if( tTarget.AlphaProcessType == UITween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve animationCurve = new AnimationCurve( tTarget.AlphaAnimationCurve.keys ) ;
					tTarget.AlphaAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.AlphaProcessType, tTarget.AlphaEaseType, tTarget.AlphaAnimationCurve ) ;
				}

				if( tTarget.GetComponent<CanvasGroup>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "CanvasGroupNone" ), MessageType.Warning, true ) ;		
				}
			}

		
			// ワイドモードを元に戻す
			EditorGUIUtility.wideMode = tWideMode ;


			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//--------------------------------------------------------------------

			if( tTarget.enabled == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// チェック
					GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

					bool isChecker = EditorGUILayout.Toggle( tTarget.IsChecker ) ;
					if( isChecker != tTarget.IsChecker )
					{
						if( isChecker == true )
						{
							UITween[] tweens = tTarget.gameObject.GetComponents<UITween>() ;
							if( tweens != null && tweens.Length >  0 )
							{
								for( int i  = 0 ; i <  tweens.Length ; i ++ )
								{
									if( tweens[ i ] != tTarget )
									{
										if( tweens[ i ].IsChecker == true )
										{
											tweens[ i ].IsChecker  = false ;
										}
									}
								}
							}
						}

						Undo.RecordObject( tTarget, "UITween : Checker Change" ) ;	// アンドウバッファに登録
						tTarget.IsChecker = isChecker ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTarget.IsChecker == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						float checkFactor = EditorGUILayout.Slider( tTarget.CheckFactor, 0, 1 ) ;
						if( checkFactor != tTarget.CheckFactor )
						{
							tTarget.CheckFactor = checkFactor ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// バリュータイプ
			UITween.ValueTypes valueType = ( UITween.ValueTypes )EditorGUILayout.EnumPopup( "ValueType",  tTarget.ValueType ) ;
			if( valueType != tTarget.ValueType )
			{
				Undo.RecordObject( tTarget, "UITween : Value Type Change" ) ;	// アンドウバッファに登録
				tTarget.ValueType = valueType ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool loop = EditorGUILayout.Toggle( tTarget.Loop ) ;
				if( loop != tTarget.Loop )
				{
					Undo.RecordObject( tTarget, "UITween : Loop Change" ) ;	// アンドウバッファに登録
					tTarget.Loop = loop ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.Loop == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// リバース
					GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

					bool reverse = EditorGUILayout.Toggle( tTarget.Reverse ) ;
					if( reverse != tTarget.Reverse )
					{
						Undo.RecordObject( tTarget, "UITween : Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.Reverse = reverse ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool ignoreTimeScale = EditorGUILayout.Toggle( tTarget.IgnoreTimeScale ) ;
				if( ignoreTimeScale != tTarget.IgnoreTimeScale )
				{
					Undo.RecordObject( tTarget, "UITween : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					tTarget.IgnoreTimeScale = ignoreTimeScale ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool playOnAwake = EditorGUILayout.Toggle( tTarget.PlayOnAwake ) ;
				if( playOnAwake != tTarget.PlayOnAwake )
				{
					Undo.RecordObject( tTarget, "UITween : Play On Awake Change" ) ;	// アンドウバッファに登録
					tTarget.PlayOnAwake = playOnAwake ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool destroyAtEnd = EditorGUILayout.Toggle( tTarget.DestroyAtEnd ) ;
				if( destroyAtEnd != tTarget.DestroyAtEnd )
				{
					Undo.RecordObject( tTarget, "UITween : Destroy At End Change" ) ;	// アンドウバッファに登録
					tTarget.DestroyAtEnd = destroyAtEnd ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Interaction Disable In Playing", GUILayout.Width( 180f ) ) ;

				bool interactionDisableInPlaying = EditorGUILayout.Toggle( tTarget.InteractionDisableInPlaying ) ;
				if( interactionDisableInPlaying != tTarget.InteractionDisableInPlaying )
				{
					Undo.RecordObject( tTarget, "UITween : Interaction Disable In Playing Change" ) ;	// アンドウバッファに登録
					tTarget.InteractionDisableInPlaying = interactionDisableInPlaying ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.InteractionDisableInPlaying == true )
			{
				if( tTarget.GetComponent<CanvasGroup>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "CanvasGroupNone" ), MessageType.Warning, true ) ;		
				}
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( tTarget.IsPlaying ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;   // 少し区切りスペース


			// デリゲートの設定状況
			SerializedObject tSO = new SerializedObject( tTarget ) ;

			SerializedProperty tSP = tSO.FindProperty( "onFinished" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP ) ;
			}
			tSO.ApplyModifiedProperties() ;

		}

		// 曲線を描画する
		private void DrawCurve( UITween tTarget, float checkFactor, UITween.ProcessTypes processType, UITween.EaseTypes easeType, AnimationCurve animationCurve )
		{
			Rect rect = GUILayoutUtility.GetRect( Screen.width - 160, 102f ) ;
		
			float x = 0 ;
			x = ( rect.width - 102f ) * 0.5f ;
			if( x <  0 )
			{
				x  = 0 ;
			}
			rect.x = x ;
			rect.width = 102f ;
		
			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 0, rect.width - 0, rect.height - 0 ), new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ) ;
			EditorGUI.DrawRect( new Rect( rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2 ), new Color( 0.2f, 0.2f, 0.2f, 1.0f ) ) ;

			DrawLine(   0,  25, 99,  25, 0xFF7F7F7F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(   0,  74, 99,  74, 0xFF7F7F7F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(  50,  99, 50,   0, 0xFF4F4F4F, rect.x + 1.0f, rect.y + 1.0f ) ;
			DrawLine(   0,  49, 99,  49, 0xFF4F4F4F, rect.x + 1.0f, rect.y + 1.0f ) ;

			int px = 0, py = 0 ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px < 100 ; px ++  )
			{
				py = ( int )UITween.GetValue(   0,  50, ( float )px * 0.01f, processType, easeType, animationCurve ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 74 - oy ) / 1 ) + 0, px, ( ( 74 - py ) / 1 ) + 0, 0xFF00FF00, rect.x + 1.0f, rect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}

			px = ( int )( ( 100.0f * checkFactor ) + 0.5f ) ;
			DrawLine( px, 99, px,  0, 0xFFFF0000, rect.x + 1.0f, rect.y + 1.0f ) ;
		}


	//	private AnimationCurve mCurve  = AnimationCurve.EaseInOut( 0, 0, 1, 1 ) ;

		// 直線を描画する
		private void DrawLine( int x0, int y0, int x1, int y1, uint tColor, float tScreenX, float tScreenY )
		{
			int dx = x1 - x0 ;
			int dy = y1 - y0 ;

			int sx = 0 ;
			if( dx <  0 )
			{
				dx  = - dx ;
				sx  = -1 ;
			}
			else
			if( dx >  0 )
			{
				sx  =  1 ;
			}

			int sy = 0 ;
			if( dy <  0 )
			{
				dy  = - dy ;
				sy  = -1 ;
			}
			else
			if( dy >  0 )
			{
				sy  =  1 ;
			}

			dx ++ ;
			dy ++ ;

			Color32 tC = new Color32( ( byte )( ( tColor >> 16 ) & 0xFF ), ( byte )( ( tColor >>  8 ) & 0xFF ),  ( byte )( ( tColor >>   0 ) & 0xFF ), ( byte )( ( tColor >> 24 ) & 0xFF ) ) ;
			Rect tR = new Rect( 0, 0, 1, 1 ) ;

			int lx, ly ;
			int px, py ;
			int cx, cy ;

			px = x0 ;
			py = y0 ;

			if( dx == 1 && dy == 1 )
			{
				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx >  1 && dy == 1 )
			{
				if( x1 <  x0 )
				{
					px = x1 ;
				}

				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				tR.width = dx ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx == 1 && dy >  1 )
			{
				if( y1 <  y0 )
				{
					py = y1 ;
				}

				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				tR.height = dy ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx >= dy )
			{
				cy = 0 ;
				for( lx  = 0 ; lx <  dx ; lx ++ )
				{
					tR.x = ( float )px + tScreenX ;
					tR.y = ( float )py + tScreenY ;
					EditorGUI.DrawRect( tR, tC ) ;

					cy = cy + dy ;
					if( cy >= dx )
					{
						cy = cy - dx ;
						py = py + sy ;
					}

					px = px + sx ;
				}
			}
			else
			{
				cx = 0 ;
				for( ly  = 0 ; ly <  dy ; ly ++ )
				{
					tR.x = ( float )px + tScreenX ;
					tR.y = ( float )py + tScreenY ;
					EditorGUI.DrawRect( tR, tC ) ;

					cx = cx + dx ;
					if( cx >= dy )
					{
						cx = cx - dy ;
						px = px + sx ;
					}

					py = py + sy ;
				}
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "RectTransformNone", "RectRansform クラスが必要です" },
			{ "CanvasGroupNone",   "CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "RectTransformNone", "'RectTransorm' is necessary." },
			{ "CanvasGroupNone",   "'CanvasGroup' is necessary." },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}
	}
}


