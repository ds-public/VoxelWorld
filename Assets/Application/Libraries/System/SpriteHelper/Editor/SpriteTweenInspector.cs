#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace SpriteHelper
{
	/// <summary>
	/// UITween のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( SpriteTween ) ) ]
	public class SpriteTweenInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteTween ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			var identity = EditorGUILayout.TextField( "Identity",  component.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != component.Identity )
			{
				Undo.RecordObject( component, "[SpriteTween] Identity Change" ) ;	// アンドウバッファに登録
				component.Identity = identity ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float delay = EditorGUILayout.FloatField( "Delay", component.Delay ) ;
			if( delay != component.Delay )
			{
				Undo.RecordObject( component, "[SpriteTween] Delay Change" ) ;	// アンドウバッファに登録
				component.Delay = delay ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( component.AnimationClip == null )
			{
				// デュアレーション
				float duration = EditorGUILayout.FloatField( "Duration", component.Duration ) ;
				if( duration != component.Duration )
				{
					Undo.RecordObject( component, "UITween : Duration Change" ) ;	// アンドウバッファに登録
					component.Duration = duration ;
					EditorUtility.SetDirty( component ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ワイドモードを有効にする
			bool wideMode = EditorGUIUtility.wideMode ;
			EditorGUIUtility.wideMode = true ;

			if( component.AnimationClip == null )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( component.PositionEnabled == false )
					{
						GUILayout.Label( "Position Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						component.PositionFoldOut = EditorGUILayout.Foldout( component.PositionFoldOut, "Position Enabled" ) ;
					}

					bool positionEnabled = EditorGUILayout.Toggle( component.PositionEnabled, GUILayout.Width( 24f ) ) ;
					if( positionEnabled != component.PositionEnabled )
					{
						Undo.RecordObject( component, "[SpriteTween] Position Enabled Change" ) ;	// アンドウバッファに登録
						component.PositionEnabled = positionEnabled ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.PositionEnabled == true && component.PositionFoldOut == true )
				{
					// ポジション

					var positionFrom = EditorGUILayout.Vector3Field( " From",  component.PositionFrom /*, GUILayout.MaxWidth( 100f ) */ ) ;
					if( positionFrom != component.PositionFrom )
					{
						Undo.RecordObject( component, "[SpriteTween] Position From Change" ) ;	// アンドウバッファに登録
						component.PositionFrom = positionFrom ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					var positionTo = EditorGUILayout.Vector3Field( " To",  component.PositionTo /*, GUILayout.MaxWidth( 100f ) */ ) ;
					if( positionTo != component.PositionTo )
					{
						Undo.RecordObject( component, "UITween : Position To Change" ) ;	// アンドウバッファに登録
						component.PositionTo = positionTo ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					SpriteTween.ProcessTypes positionProcessType = ( SpriteTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  component.PositionProcessType ) ;
					if( positionProcessType != component.PositionProcessType )
					{
						Undo.RecordObject( component, "[SpriteTween] Position Process Type Change" ) ;	// アンドウバッファに登録
						component.PositionProcessType = positionProcessType ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( component.PositionProcessType == SpriteTween.ProcessTypes.Ease )
					{
						// イーズタイプ
						SpriteTween.EaseTypes positionEaseType = ( SpriteTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  component.PositionEaseType ) ;
						if( positionEaseType != component.PositionEaseType )
						{
							Undo.RecordObject( component, "[SpriteTween] Position Ease Type Change" ) ;	// アンドウバッファに登録
							component.PositionEaseType = positionEaseType ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( component.PositionProcessType == SpriteTween.ProcessTypes.AnimationCurve )
					{
						var animationCurve = new AnimationCurve( component.PositionAnimationCurve.keys ) ;
						component.PositionAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( component.IsChecker == true )
					{
						DrawCurve( component, component.CheckFactor, component.PositionProcessType, component.PositionEaseType, component.PositionAnimationCurve ) ;
					}

					if( component.GetComponent<Transform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( component.RotationEnabled == false )
					{
						GUILayout.Label( "Rotation Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						component.RotationFoldOut = EditorGUILayout.Foldout( component.RotationFoldOut, "Rotation Enabled" ) ;
					}

					bool rotationEnabled = EditorGUILayout.Toggle( component.RotationEnabled, GUILayout.Width( 24f ) ) ;
					if( rotationEnabled != component.RotationEnabled )
					{
						Undo.RecordObject( component, "[SpriteTween] Rotation Enabled Change" ) ;	// アンドウバッファに登録
						component.RotationEnabled = rotationEnabled ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.RotationEnabled == true && component.RotationFoldOut == true )
				{
					// ローテーション

					var rotationFrom = EditorGUILayout.Vector3Field( " From", component.RotationFrom ) ;
					if( rotationFrom != component.RotationFrom )
					{
						Undo.RecordObject( component, "[SpriteTween] Rotation From Change" ) ;	// アンドウバッファに登録
						component.RotationFrom = rotationFrom ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					var rotationTo = EditorGUILayout.Vector3Field( " To", component.RotationTo ) ;
					if( rotationTo != component.RotationTo )
					{
						Undo.RecordObject( component, "[SpriteTween] Rotation To Change" ) ;	// アンドウバッファに登録
						component.RotationTo = rotationTo ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					SpriteTween.ProcessTypes rotationProcessType = ( SpriteTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  component.RotationProcessType ) ;
					if( rotationProcessType != component.RotationProcessType )
					{
						Undo.RecordObject( component, "[SpriteTween] Rotation Process Type Change" ) ;	// アンドウバッファに登録
						component.RotationProcessType = rotationProcessType ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( component.RotationProcessType == SpriteTween.ProcessTypes.Ease )
					{
						// イーズタイプ
						SpriteTween.EaseTypes rotationEaseType = ( SpriteTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  component.RotationEaseType ) ;
						if( rotationEaseType != component.RotationEaseType )
						{
							Undo.RecordObject( component, "[SpriteTween] Rotation Ease Type Change" ) ;	// アンドウバッファに登録
							component.RotationEaseType = rotationEaseType ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( component.RotationProcessType == SpriteTween.ProcessTypes.AnimationCurve )
					{
						var animationCurve = new AnimationCurve( component.RotationAnimationCurve.keys ) ;
						component.RotationAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( component.IsChecker == true )
					{
						DrawCurve( component, component.CheckFactor, component.RotationProcessType, component.RotationEaseType, component.RotationAnimationCurve ) ;
					}

					if( component.GetComponent<Transform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( component.ScaleEnabled == false )
					{
						GUILayout.Label( "Scale Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						component.ScaleFoldOut = EditorGUILayout.Foldout( component.ScaleFoldOut, "Scale Enabled" ) ;
					}

					bool scaleEnabled = EditorGUILayout.Toggle( component.ScaleEnabled, GUILayout.Width( 24f ) ) ;
					if( scaleEnabled != component.ScaleEnabled )
					{
						Undo.RecordObject( component, "[SpriteTween] Scale Enabled Change" ) ;	// アンドウバッファに登録
						component.ScaleEnabled = scaleEnabled ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.ScaleEnabled == true && component.ScaleFoldOut == true )
				{
					// スケール

					var scaleFrom = EditorGUILayout.Vector3Field( " From", component.ScaleFrom ) ;
					if( scaleFrom != component.ScaleFrom )
					{
						Undo.RecordObject( component, "[SpriteTween] Scale From Change" ) ;	// アンドウバッファに登録
						component.ScaleFrom = scaleFrom ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					var scaleTo = EditorGUILayout.Vector3Field( " To", component.ScaleTo ) ;
					if( scaleTo != component.ScaleTo )
					{
						Undo.RecordObject( component, "[SpriteTween] Scale To Change" ) ;	// アンドウバッファに登録
						component.ScaleTo = scaleTo ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					// プロセスタイプ
					SpriteTween.ProcessTypes scaleProcessType = ( SpriteTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type", component.ScaleProcessType ) ;
					if( scaleProcessType != component.ScaleProcessType )
					{
						Undo.RecordObject( component, "[SpriteTween] Scale Process Type Change" ) ;	// アンドウバッファに登録
						component.ScaleProcessType = scaleProcessType ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( component.ScaleProcessType == SpriteTween.ProcessTypes.Ease )
					{
						// イーズタイプ
						SpriteTween.EaseTypes scaleEaseType = ( SpriteTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType", component.ScaleEaseType ) ;
						if( scaleEaseType != component.ScaleEaseType )
						{
							Undo.RecordObject( component, "[SpriteTween] Scale Ease Type Change" ) ;	// アンドウバッファに登録
							component.ScaleEaseType = scaleEaseType ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( component.ScaleProcessType == SpriteTween.ProcessTypes.AnimationCurve )
					{
						var animationCurve = new AnimationCurve( component.ScaleAnimationCurve.keys ) ;
						component.ScaleAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( component.IsChecker == true )
					{
						DrawCurve( component, component.CheckFactor, component.ScaleProcessType, component.ScaleEaseType, component.ScaleAnimationCurve ) ;
					}

					if( component.GetComponent<Transform>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
					}
				}

				//--------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					if( component.AlphaEnabled == false )
					{
						GUILayout.Label( "Alpha Enabled" /*, GUILayout.Width( 116f ) */ ) ;
					}
					else
					{
						component.AlphaFoldOut = EditorGUILayout.Foldout( component.AlphaFoldOut, "Alpha Enabled" ) ;
					}

					bool alphaEnabled = EditorGUILayout.Toggle( component.AlphaEnabled, GUILayout.Width( 24f ) ) ;
					if( alphaEnabled != component.AlphaEnabled )
					{
						Undo.RecordObject( component, "[SpriteTween] Alpha Enabled Change" ) ;	// アンドウバッファに登録
						component.AlphaEnabled = alphaEnabled ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.AlphaEnabled == true && component.AlphaFoldOut == true )
				{
					// アルファ

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( " From", GUILayout.Width( 40f ) ) ;

						float alphaFrom = EditorGUILayout.Slider( component.AlphaFrom, 0, 1 ) ;
						if( alphaFrom != component.AlphaFrom )
						{
							Undo.RecordObject( component, "[SpriteTween] Alpha From Change" ) ;	// アンドウバッファに登録
							component.AlphaFrom = alphaFrom ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( " To", GUILayout.Width( 40f ) ) ;

						float alphaTo = EditorGUILayout.Slider( component.AlphaTo, 0, 1 ) ;
						if( alphaTo != component.AlphaTo )
						{
							Undo.RecordObject( component, "[SpriteTween] Alpha To Change" ) ;	// アンドウバッファに登録
							component.AlphaTo = alphaTo ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					// プロセスタイプ
					SpriteTween.ProcessTypes alphaProcessType = ( SpriteTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type", component.AlphaProcessType ) ;
					if( alphaProcessType != component.AlphaProcessType )
					{
						Undo.RecordObject( component, "[SpriteTween] Alpha Process Type Change" ) ;	// アンドウバッファに登録
						component.AlphaProcessType = alphaProcessType ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}

					if( component.AlphaProcessType == SpriteTween.ProcessTypes.Ease )
					{
						// イーズタイプ
						SpriteTween.EaseTypes alphaEaseType = ( SpriteTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType", component.AlphaEaseType ) ;
						if( alphaEaseType != component.AlphaEaseType )
						{
							Undo.RecordObject( component, "[SpriteTween] Alpha Ease Type Change" ) ;	// アンドウバッファに登録
							component.AlphaEaseType = alphaEaseType ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					else
					if( component.AlphaProcessType == SpriteTween.ProcessTypes.AnimationCurve )
					{
						var animationCurve = new AnimationCurve( component.AlphaAnimationCurve.keys ) ;
						component.AlphaAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve /*, GUILayout.Width( 170f ), GUILayout.Height( 52f )*/ ) ;
					}

					if( component.IsChecker == true )
					{
						DrawCurve( component, component.CheckFactor, component.AlphaProcessType, component.AlphaEaseType, component.AlphaAnimationCurve ) ;
					}

					if( component.GetComponent<SpriteRenderer>() == null )
					{
						EditorGUILayout.HelpBox( GetMessage( "SpriteRendererNone" ), MessageType.Warning, true ) ;		
					}
				}

				EditorGUILayout.Separator() ;	// 少し区切りスペース
			}

			//----------------------------------------------------------

			// AnimationClip
			AnimationClip animationClip = EditorGUILayout.ObjectField( "Animation Clip", component.AnimationClip, typeof( AnimationClip ), false ) as AnimationClip ;
			if( animationClip != component.AnimationClip )
			{
				Undo.RecordObject( component, "[SpriteTween] AnimationClip Change" ) ;	// アンドウバッファに登録
				component.AnimationClip = animationClip ;
				EditorUtility.SetDirty( component ) ;
			}

			//----------------------------------------------------------

			// ワイドモードを元に戻す
			EditorGUIUtility.wideMode = wideMode ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//--------------------------------------------------------------------

			if( component.AnimationClip == null )
			{
				if( component.enabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						// チェック
						GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

						bool isChecker = EditorGUILayout.Toggle( component.IsChecker ) ;
						if( isChecker != component.IsChecker )
						{
							if( isChecker == true )
							{
								var tweens = component.gameObject.GetComponents<SpriteTween>() ;
								if( tweens != null && tweens.Length >  0 )
								{
									for( int i  = 0 ; i <  tweens.Length ; i ++ )
									{
										if( tweens[ i ] != component )
										{
											if( tweens[ i ].IsChecker == true )
											{
												tweens[ i ].IsChecker  = false ;
											}
										}
									}
								}
							}

							Undo.RecordObject( component, "[SpriteTween] Checker Change" ) ;	// アンドウバッファに登録
							component.IsChecker = isChecker ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					if( component.IsChecker == true )
					{
						GUILayout.BeginHorizontal() ;	// 横並び
						{
							float checkFactor = EditorGUILayout.Slider( component.CheckFactor, 0, 1 ) ;
							if( checkFactor != component.CheckFactor )
							{
								component.CheckFactor = checkFactor ;
							}
						}
						GUILayout.EndHorizontal() ;		// 横並び終了
					}
				}

				//--------------------------------------------------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// バリュータイプ
				SpriteTween.ValueTypes valueType = ( SpriteTween.ValueTypes )EditorGUILayout.EnumPopup( "ValueType", component.ValueType ) ;
				if( valueType != component.ValueType )
				{
					Undo.RecordObject( component, "[SpriteTween] Value Type Change" ) ;	// アンドウバッファに登録
					component.ValueType = valueType ;
					EditorUtility.SetDirty( component ) ;
	//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// ループ
					GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

					bool loop = EditorGUILayout.Toggle( component.Loop ) ;
					if( loop != component.Loop )
					{
						Undo.RecordObject( component, "[SpriteTween] Loop Change" ) ;	// アンドウバッファに登録
						component.Loop = loop ;
						EditorUtility.SetDirty( component ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.Loop == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						// リバース
						GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

						bool reverse = EditorGUILayout.Toggle( component.Reverse ) ;
						if( reverse != component.Reverse )
						{
							Undo.RecordObject( component, "[SpriteTween] Reverse Change" ) ;	// アンドウバッファに登録
							component.Reverse = reverse ;
							EditorUtility.SetDirty( component ) ;
	//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// イグノアタイムスケール
					GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

					bool ignoreTimeScale = EditorGUILayout.Toggle( component.IgnoreTimeScale ) ;
					if( ignoreTimeScale != component.IgnoreTimeScale )
					{
						Undo.RecordObject( component, "[SpriteTween] Ignore Time Scale Change" ) ;	// アンドウバッファに登録
						component.IgnoreTimeScale = ignoreTimeScale ;
						EditorUtility.SetDirty( component ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool playOnAwake = EditorGUILayout.Toggle( component.PlayOnAwake ) ;
				if( playOnAwake != component.PlayOnAwake )
				{
					Undo.RecordObject( component, "[SpriteTween] Play On Awake Change" ) ;	// アンドウバッファに登録
					component.PlayOnAwake = playOnAwake ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool destroyAtEnd = EditorGUILayout.Toggle( component.DestroyAtEnd ) ;
				if( destroyAtEnd != component.DestroyAtEnd )
				{
					Undo.RecordObject( component, "[SpriteTween] Destroy At End Change" ) ;	// アンドウバッファに登録
					component.DestroyAtEnd = destroyAtEnd ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( component.IsPlaying ) ;
			}
			GUILayout.EndHorizontal() ;     // 横並び終了

			//----------------------------------
#if false
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// デリゲートの設定状況
			SerializedObject so = new SerializedObject( view ) ;

			SerializedProperty sp = so.FindProperty( "OnFinishedDelegate" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp ) ;
			}
			so.ApplyModifiedProperties() ;
#endif
		}

		// 曲線を描画する
		private void DrawCurve( SpriteTween view, float checkFactor, SpriteTween.ProcessTypes processType, SpriteTween.EaseTypes easeType, AnimationCurve animationCurve )
		{
			var rect = GUILayoutUtility.GetRect( Screen.width - 160, 102f ) ;
		
			float x ;
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

			int px, py ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px < 100 ; px ++  )
			{
				py = ( int )SpriteTween.GetValue(   0,  50, ( float )px * 0.01f, processType, easeType, animationCurve ) ;

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
		private void DrawLine( int x0, int y0, int x1, int y1, uint color, float screenX, float screenY )
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

			var c = new Color32( ( byte )( ( color >> 16 ) & 0xFF ), ( byte )( ( color >>  8 ) & 0xFF ),  ( byte )( ( color >>   0 ) & 0xFF ), ( byte )( ( color >> 24 ) & 0xFF ) ) ;
			var r = new Rect( 0, 0, 1, 1 ) ;

			int lx, ly ;
			int px, py ;
			int cx, cy ;

			px = x0 ;
			py = y0 ;

			if( dx == 1 && dy == 1 )
			{
				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx >  1 && dy == 1 )
			{
				if( x1 <  x0 )
				{
					px = x1 ;
				}

				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				r.width = dx ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx == 1 && dy >  1 )
			{
				if( y1 <  y0 )
				{
					py = y1 ;
				}

				r.x = ( float )px + screenX ;
				r.y = ( float )py + screenY ;
				r.height = dy ;
				EditorGUI.DrawRect( r, c ) ;
			}
			else
			if( dx >= dy )
			{
				cy = 0 ;
				for( lx  = 0 ; lx <  dx ; lx ++ )
				{
					r.x = ( float )px + screenX ;
					r.y = ( float )py + screenY ;
					EditorGUI.DrawRect( r, c ) ;

					cy += dy ;
					if( cy >= dx )
					{
						cy -= dx ;
						py += sy ;
					}

					px += sx ;
				}
			}
			else
			{
				cx = 0 ;
				for( ly  = 0 ; ly <  dy ; ly ++ )
				{
					r.x = ( float )px + screenX ;
					r.y = ( float )py + screenY ;
					EditorGUI.DrawRect( r, c ) ;

					cx += dx ;
					if( cx >= dy )
					{
						cx -= dy ;
						px += sx ;
					}

					py += sy ;
				}
			}
		}

		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "TransformNone",			"Tansform クラスが必要です" },
			{ "SpriteRendererNone",		"SpriteRenderer クラスが必要です" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "TransformNone",			"'Tansform' is necessary." },
			{ "SpriteRendererNone",		"'SpriteRenderer' is necessary." },
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
