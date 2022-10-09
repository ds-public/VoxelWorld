#if UNITY_EDITOR

using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace TransformHelper
{
	[ CustomEditor( typeof( SoftTransformTween ) ) ]
	public class SoftTransformTweenInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			SoftTransformTween tTarget = target as SoftTransformTween ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity",  tTarget.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity != tTarget.Identity )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.Identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		
			// ディレイ
			float tDelay = EditorGUILayout.FloatField( "Delay",  tTarget.Delay ) ;
			if( tDelay != tTarget.Delay )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Delay Change" ) ;	// アンドウバッファに登録
				tTarget.Delay = tDelay ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// デュアレーション
			float tDuration = EditorGUILayout.FloatField( "Duration",  tTarget.Duration ) ;
			if( tDuration != tTarget.Duration )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Duration Change" ) ;	// アンドウバッファに登録
				tTarget.Duration = tDuration ;
				EditorUtility.SetDirty( tTarget ) ;
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
					Undo.RecordObject( tTarget, "SoftTransformTween : Position Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.PositionEnabled = tPositionEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.PositionEnabled == true && tTarget.PositionFoldOut == true )
			{
				// ポジション

				Vector3 tPositionFrom = EditorGUILayout.Vector3Field( " From",  tTarget.PositionFrom /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionFrom != tTarget.PositionFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position From Change" ) ;	// アンドウバッファに登録
					tTarget.PositionFrom = tPositionFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tPositionTo = EditorGUILayout.Vector3Field( " To",  tTarget.PositionTo /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionTo != tTarget.PositionTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position To Change" ) ;	// アンドウバッファに登録
					tTarget.PositionTo = tPositionTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessTypes tPositionProcessType = ( SoftTransformTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.PositionProcessType ) ;
				if( tPositionProcessType != tTarget.PositionProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.PositionProcessType = tPositionProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.PositionProcessType == SoftTransformTween.ProcessTypes.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseTypes tPositionEaseType = ( SoftTransformTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.PositionEaseType ) ;
					if( tPositionEaseType != tTarget.PositionEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Position Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.PositionEaseType = tPositionEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.PositionProcessType == SoftTransformTween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve animationCurve = new AnimationCurve(  tTarget.PositionAnimationCurve.keys ) ;
					tTarget.PositionAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", animationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.PositionProcessType, tTarget.PositionEaseType, tTarget.PositionAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
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
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.RotationEnabled = tRotationEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.RotationEnabled == true && tTarget.RotationFoldOut == true )
			{
				// ローテーション

				Vector3 tRotationFrom = EditorGUILayout.Vector3Field( " From",  tTarget.RotationFrom ) ;
				if( tRotationFrom != tTarget.RotationFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation From Change" ) ;	// アンドウバッファに登録
					tTarget.RotationFrom = tRotationFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tRotationTo = EditorGUILayout.Vector3Field( " To",  tTarget.RotationTo ) ;
				if( tRotationTo != tTarget.RotationTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation To Change" ) ;	// アンドウバッファに登録
					tTarget.RotationTo = tRotationTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessTypes tRotationProcessType = ( SoftTransformTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.RotationProcessType ) ;
				if( tRotationProcessType != tTarget.RotationProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.RotationProcessType = tRotationProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.RotationProcessType == SoftTransformTween.ProcessTypes.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseTypes tRotationEaseType = ( SoftTransformTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.RotationEaseType ) ;
					if( tRotationEaseType != tTarget.RotationEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.RotationEaseType = tRotationEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.RotationProcessType == SoftTransformTween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.RotationAnimationCurve.keys ) ;
					tTarget.RotationAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.RotationProcessType, tTarget.RotationEaseType, tTarget.RotationAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
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
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleEnabled = tScaleEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.ScaleEnabled == true && tTarget.ScaleFoldOut == true )
			{
				// スケール

				Vector3 tScaleFrom = EditorGUILayout.Vector3Field( " From",  tTarget.ScaleFrom ) ;
				if( tScaleFrom != tTarget.ScaleFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale From Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleFrom = tScaleFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tScaleTo = EditorGUILayout.Vector3Field( " To",  tTarget.ScaleTo ) ;
				if( tScaleTo != tTarget.ScaleTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale To Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleTo = tScaleTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessTypes tScaleProcessType = ( SoftTransformTween.ProcessTypes )EditorGUILayout.EnumPopup( " Process Type",  tTarget.ScaleProcessType ) ;
				if( tScaleProcessType != tTarget.ScaleProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.ScaleProcessType = tScaleProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.ScaleProcessType == SoftTransformTween.ProcessTypes.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseTypes tScaleEaseType = ( SoftTransformTween.EaseTypes )EditorGUILayout.EnumPopup( " EaseType",  tTarget.ScaleEaseType ) ;
					if( tScaleEaseType != tTarget.ScaleEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Scale Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.ScaleEaseType = tScaleEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.ScaleProcessType == SoftTransformTween.ProcessTypes.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.ScaleAnimationCurve.keys ) ;
					tTarget.ScaleAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.IsChecker == true )
				{
					DrawCurve( tTarget, tTarget.CheckFactor, tTarget.ScaleProcessType, tTarget.ScaleEaseType, tTarget.ScaleAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
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

					bool tIsChecker = EditorGUILayout.Toggle( tTarget.IsChecker ) ;
					if( tIsChecker != tTarget.IsChecker )
					{
						if( tIsChecker == true )
						{
							SoftTransformTween[] tTweenList = tTarget.gameObject.GetComponents<SoftTransformTween>() ;
							if( tTweenList != null && tTweenList.Length >  0 )
							{
								for( int i  = 0 ; i <  tTweenList.Length ; i ++ )
								{
									if( tTweenList[ i ] != tTarget )
									{
										if( tTweenList[ i ].IsChecker == true )
										{
											tTweenList[ i ].IsChecker  = false ;
										}
									}
								}
							}
						}

						Undo.RecordObject( tTarget, "SoftTransformTween : Checker Change" ) ;	// アンドウバッファに登録
						tTarget.IsChecker = tIsChecker ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTarget.IsChecker == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						float tCheckFactor = EditorGUILayout.Slider( tTarget.CheckFactor, 0, 1 ) ;
						if( tCheckFactor != tTarget.CheckFactor )
						{
							tTarget.CheckFactor = tCheckFactor ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// バリュータイプ
			SoftTransformTween.ValueTypes tValueType = ( SoftTransformTween.ValueTypes )EditorGUILayout.EnumPopup( "ValueType",  tTarget.ValueType ) ;
			if( tValueType != tTarget.ValueType )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Value Type Change" ) ;	// アンドウバッファに登録
				tTarget.ValueType = tValueType ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool tLoop = EditorGUILayout.Toggle( tTarget.Loop ) ;
				if( tLoop != tTarget.Loop )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Loop Change" ) ;	// アンドウバッファに登録
					tTarget.Loop = tLoop ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.Loop == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// リバース
					GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

					bool tReverse = EditorGUILayout.Toggle( tTarget.Reverse ) ;
					if( tReverse != tTarget.Reverse )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.Reverse = tReverse ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool tIgnoreTimeScale = EditorGUILayout.Toggle( tTarget.IgnoreTimeScale ) ;
				if( tIgnoreTimeScale != tTarget.IgnoreTimeScale )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					tTarget.IgnoreTimeScale = tIgnoreTimeScale ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool tPlayOnAwake = EditorGUILayout.Toggle( tTarget.PlayOnAwake ) ;
				if( tPlayOnAwake != tTarget.PlayOnAwake )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Play On Awake Change" ) ;	// アンドウバッファに登録
					tTarget.PlayOnAwake = tPlayOnAwake ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
//				// プレイオンアウェイク
//				GUILayout.Label( "Interaction Disable In Playing", GUILayout.Width( 180f ) ) ;
//
//				bool tInteractionDisableInPlaying = EditorGUILayout.Toggle( tTarget.interactionDisableInPlaying ) ;
//				if( tInteractionDisableInPlaying != tTarget.interactionDisableInPlaying )
//				{
//					Undo.RecordObject( tTarget, "SoftTransformTween : Interaction Disable In Playing Change" ) ;	// アンドウバッファに登録
//					tTarget.interactionDisableInPlaying = tInteractionDisableInPlaying ;
//					EditorUtility.SetDirty( tTarget ) ;
//				}
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了

//			if( tTarget.interactionDisableInPlaying == true )
//			{
//			}

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
		private void DrawCurve( SoftTransformTween tTarget, float tCheckFactor, SoftTransformTween.ProcessTypes tProcessType, SoftTransformTween.EaseTypes tEaseType, AnimationCurve tAnimationCurve )
		{
			Rect tRect = GUILayoutUtility.GetRect( Screen.width - 64, 102f ) ;
		
			float x ;
			x = ( tRect.width - 52f ) * 0.5f ;
			if( x <  0 )
			{
				x  = 0 ;
			}
			tRect.x = x ;
			tRect.width = 52f ;
		
			EditorGUI.DrawRect( new Rect( tRect.x + 0, tRect.y + 0, tRect.width - 0, tRect.height - 0 ), new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ) ;
			EditorGUI.DrawRect( new Rect( tRect.x + 1, tRect.y + 1, tRect.width - 2, tRect.height - 2 ), new Color( 0.2f, 0.2f, 0.2f, 1.0f ) ) ;

			DrawLine(   0,  25, 49,  25, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  74, 49,  74, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(  25,  99, 25,   0, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  49, 49,  49, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;

			int px, py ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px <  50 ; px ++  )
			{
				py = ( int )SoftTransformTween.GetValue(   0,  50, ( float )px * 0.02f, tProcessType, tEaseType, tAnimationCurve ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 74 - oy ) / 1 ) + 0, px, ( ( 74 - py ) / 1 ) + 0, 0xFF00FF00, tRect.x + 1.0f, tRect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}

			px = ( int )( ( 50.0f * tCheckFactor ) + 0.5f ) ;
			DrawLine( px, 99, px,  0, 0xFFFF0000, tRect.x + 1.0f, tRect.y + 1.0f ) ;
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
					tR.x = ( float )px + tScreenX ;
					tR.y = ( float )py + tScreenY ;
					EditorGUI.DrawRect( tR, tC ) ;

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

		private readonly Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "TransformNone",		"Transform クラスが必要です" },
		} ;
		private readonly Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "TransformNone",		"'Transorm' is necessary." },
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

#endif
