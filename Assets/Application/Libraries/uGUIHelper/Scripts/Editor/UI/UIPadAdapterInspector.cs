#if UNITY_EDITOR

using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;


namespace uGUIHelper
{
	/// <summary>
	/// UIPadAdapter のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIPadAdapter ) ) ]
	public class UIPadAdapterInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			var padAdapter = target as UIPadAdapter ;

//			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//------------------------------------------------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool focus = EditorGUILayout.Toggle( padAdapter.Focus, GUILayout.Width( 16f ) ) ;
				if( padAdapter.Focus != focus )
				{
					Undo.RecordObject( padAdapter, "UIPadAdapter : Focus Change" ) ;	// アンドウバッファに登録
					padAdapter.Focus = focus ;
					EditorUtility.SetDirty( padAdapter ) ;
				}
				GUILayout.Label( "Focus", GUILayout.Width( 64f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了		

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//----------------------------------------------------------

			var padPlayerTargetNames = new List<string>() ;

			int selectedIndex = -1 ;
			int index = 0 ;
			foreach( var padPlayerTarget in Enum.GetValues( typeof( PadPlayerTargets ) ).Cast<PadPlayerTargets>() )
			{
				padPlayerTargetNames.Add( padPlayerTarget.ToString() ) ;
				if( padAdapter.PadPlayerTarget == padPlayerTarget )
				{
					selectedIndex = index ;
				}
				index ++ ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				GUILayout.Label( "Pad Player Target" ) ;
				EditorGUILayout.Popup( "", selectedIndex, padPlayerTargetNames.ToArray(), GUILayout.Width( 120f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			//------------------------------------------------------------------------------------------

			if( padAdapter.Foldout == false )
			{
				GUI.contentColor = Color.magenta ;
			}
			else
			{
				GUI.contentColor = Color.white ;
			}
			padAdapter.Foldout = EditorGUILayout.Foldout( padAdapter.Foldout, "Input Targets" ) ;
			GUI.contentColor = Color.white ;
			if( padAdapter.Foldout == true )
			{
				// 折りたたみ対応

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.cyan ;
				GUILayout.Label( "Button : Base" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool b1 = EditorGUILayout.Toggle( padAdapter.B1, GUILayout.Width( 16f ) ) ;
					if( padAdapter.B1 != b1 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : B1 Change" ) ;	// アンドウバッファに登録
						padAdapter.B1 = b1 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "B1", GUILayout.Width( 32f ) ) ;

					bool b2 = EditorGUILayout.Toggle( padAdapter.B2, GUILayout.Width( 16f ) ) ;
					if( padAdapter.B2 != b2 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : B2 Change" ) ;	// アンドウバッファに登録
						padAdapter.B2 = b2 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "B2", GUILayout.Width( 32f ) ) ;

					bool b3 = EditorGUILayout.Toggle( padAdapter.B3, GUILayout.Width( 16f ) ) ;
					if( padAdapter.B3 != b3 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : B3 Change" ) ;	// アンドウバッファに登録
						padAdapter.B3 = b3 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "B3", GUILayout.Width( 32f ) ) ;

					bool b4 = EditorGUILayout.Toggle( padAdapter.B4, GUILayout.Width( 16f ) ) ;
					if( padAdapter.B4 != b4 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : B4 Change" ) ;	// アンドウバッファに登録
						padAdapter.B4 = b4 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "B4", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//----------------------------------

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.cyan ;
				GUILayout.Label( "Button : Side" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool r1 = EditorGUILayout.Toggle( padAdapter.R1, GUILayout.Width( 16f ) ) ;
					if( padAdapter.R1 != r1 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : R1 Change" ) ;	// アンドウバッファに登録
						padAdapter.R1 = r1 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "R1", GUILayout.Width( 32f ) ) ;

					bool l1 = EditorGUILayout.Toggle( padAdapter.L1, GUILayout.Width( 16f ) ) ;
					if( padAdapter.L1 != l1 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : L1 Change" ) ;	// アンドウバッファに登録
						padAdapter.L1 = l1 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "L1", GUILayout.Width( 32f ) ) ;


					bool r2 = EditorGUILayout.Toggle( padAdapter.R2, GUILayout.Width( 16f ) ) ;
					if( padAdapter.R2 != r2 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : R2 Change" ) ;	// アンドウバッファに登録
						padAdapter.R2 = r2 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "R2", GUILayout.Width( 32f ) ) ;

					bool l2 = EditorGUILayout.Toggle( padAdapter.L2, GUILayout.Width( 16f ) ) ;
					if( padAdapter.L2 != l2 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : L2 Change" ) ;	// アンドウバッファに登録
						padAdapter.L2 = l2 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "L2", GUILayout.Width( 32f ) ) ;


					bool r3 = EditorGUILayout.Toggle( padAdapter.R3, GUILayout.Width( 16f ) ) ;
					if( padAdapter.R3 != r3 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : R3 Change" ) ;	// アンドウバッファに登録
						padAdapter.R3 = r3 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "R3", GUILayout.Width( 32f ) ) ;

					bool l3 = EditorGUILayout.Toggle( padAdapter.L3, GUILayout.Width( 16f ) ) ;
					if( padAdapter.L3 != l3 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : L3 Change" ) ;	// アンドウバッファに登録
						padAdapter.L3 = l3 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "L3", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//----------------------------------

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.cyan ;
				GUILayout.Label( "Button : Optional" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool o1 = EditorGUILayout.Toggle( padAdapter.O1, GUILayout.Width( 16f ) ) ;
					if( padAdapter.O1 != o1 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : O1 Change" ) ;	// アンドウバッファに登録
						padAdapter.O1 = o1 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "O1", GUILayout.Width( 32f ) ) ;

					bool o2 = EditorGUILayout.Toggle( padAdapter.O2, GUILayout.Width( 16f ) ) ;
					if( padAdapter.O2 != o2 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : O2 Change" ) ;	// アンドウバッファに登録
						padAdapter.O2 = o2 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "O2", GUILayout.Width( 32f ) ) ;

					bool o3 = EditorGUILayout.Toggle( padAdapter.O3, GUILayout.Width( 16f ) ) ;
					if( padAdapter.O3 != o3 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : O3 Change" ) ;	// アンドウバッファに登録
						padAdapter.O3 = o3 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "O3", GUILayout.Width( 32f ) ) ;

					bool o4 = EditorGUILayout.Toggle( padAdapter.O4, GUILayout.Width( 16f ) ) ;
					if( padAdapter.O4 != o4 )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : O4 Change" ) ;	// アンドウバッファに登録
						padAdapter.O4 = o4 ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "O4", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//-------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool buttonRepeatPressEnabled = EditorGUILayout.Toggle( padAdapter.ButtonRepeatPressEnabled, GUILayout.Width( 16f ) ) ;
					if( padAdapter.ButtonRepeatPressEnabled != buttonRepeatPressEnabled )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : Button Repeat Press Enabled Change" ) ;	// アンドウバッファに登録
						padAdapter.ButtonRepeatPressEnabled = buttonRepeatPressEnabled ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "Button Repeat Press Enabled", GUILayout.Width( 240f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( padAdapter.ButtonRepeatPressEnabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Button Repeat Starting Time", GUILayout.Width( 240f ) ) ;

						float buttonRepeatStartingTime = EditorGUILayout.Slider( padAdapter.ButtonRepeatPressStartingTime, 0.01f, 10.0f ) ;
						if( padAdapter.ButtonRepeatPressStartingTime != buttonRepeatStartingTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Button Repeat Starting Time Change" ) ;	// アンドウバッファに登録
							padAdapter.ButtonRepeatPressStartingTime = buttonRepeatStartingTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Button Repeat Interval Time", GUILayout.Width( 240f ) ) ;

						float buttonRepeatIntervalTime = EditorGUILayout.Slider( padAdapter.ButtonRepeatPressIntervalTime, 0.01f, 10.0f ) ;
						if( padAdapter.ButtonRepeatPressIntervalTime != buttonRepeatIntervalTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Button Repeat Interval Time Change" ) ;	// アンドウバッファに登録
							padAdapter.ButtonRepeatPressIntervalTime = buttonRepeatIntervalTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool buttonLongPressEnabled = EditorGUILayout.Toggle( padAdapter.ButtonLongPressEnabled, GUILayout.Width( 16f ) ) ;
					if( padAdapter.ButtonLongPressEnabled != buttonLongPressEnabled )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : Button Long Press Enabled Change" ) ;	// アンドウバッファに登録
						padAdapter.ButtonLongPressEnabled = buttonLongPressEnabled ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "Button Long Press Enabled", GUILayout.Width( 240f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( padAdapter.ButtonLongPressEnabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Button Long Press Decision Time", GUILayout.Width( 240f ) ) ;

						float buttonLongPressDecisionTime = EditorGUILayout.Slider( padAdapter.ButtonLongPressDecisionTime, 0.1f, 10.0f ) ;
						if( padAdapter.ButtonLongPressDecisionTime != buttonLongPressDecisionTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Button Long Press Decision Time Change" ) ;	// アンドウバッファに登録
							padAdapter.ButtonLongPressDecisionTime = buttonLongPressDecisionTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				//----------------------------------

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.green ;
				GUILayout.Label( "Axis : D-Pad" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool dp_r = EditorGUILayout.Toggle( padAdapter.DP_R, GUILayout.Width( 16f ) ) ;
					if( padAdapter.DP_R != dp_r )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : DP(R) Change" ) ;	// アンドウバッファに登録
						padAdapter.DP_R = dp_r ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "R", GUILayout.Width( 32f ) ) ;

					bool dp_l = EditorGUILayout.Toggle( padAdapter.DP_L, GUILayout.Width( 16f ) ) ;
					if( padAdapter.DP_L != dp_l )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : DP(L) Change" ) ;	// アンドウバッファに登録
						padAdapter.DP_L = dp_l ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "L", GUILayout.Width( 32f ) ) ;

					bool dp_u = EditorGUILayout.Toggle( padAdapter.DP_U, GUILayout.Width( 16f ) ) ;
					if( padAdapter.DP_U != dp_u )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : DP(U) Change" ) ;	// アンドウバッファに登録
						padAdapter.DP_U = dp_u ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "U", GUILayout.Width( 32f ) ) ;

					bool dp_d = EditorGUILayout.Toggle( padAdapter.DP_D, GUILayout.Width( 16f ) ) ;
					if( padAdapter.DP_D != dp_d )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : DP(D) Change" ) ;	// アンドウバッファに登録
						padAdapter.DP_D = dp_d ;
						EditorUtility.SetDirty( padAdapter ) ;
					}
					GUILayout.Label( "D", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//----------------------------------

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.green ;
				GUILayout.Label( "Axis : L-Stick" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool ls_r = EditorGUILayout.Toggle( padAdapter.LS_R, GUILayout.Width( 16f ) ) ;
					if( padAdapter.LS_R != ls_r )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : LS(R) Change" ) ;	// アンドウバッファに登録
						padAdapter.LS_R = ls_r ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "R", GUILayout.Width( 32f ) ) ;

					bool ls_l = EditorGUILayout.Toggle( padAdapter.LS_L, GUILayout.Width( 16f ) ) ;
					if( padAdapter.LS_L != ls_l )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : LS(L) Change" ) ;	// アンドウバッファに登録
						padAdapter.LS_L = ls_l ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "L", GUILayout.Width( 32f ) ) ;

					bool ls_u = EditorGUILayout.Toggle( padAdapter.LS_U, GUILayout.Width( 16f ) ) ;
					if( padAdapter.LS_U != ls_u )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : LS(U) Change" ) ;	// アンドウバッファに登録
						padAdapter.LS_U = ls_u ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "U", GUILayout.Width( 32f ) ) ;

					bool ls_d = EditorGUILayout.Toggle( padAdapter.LS_D, GUILayout.Width( 16f ) ) ;
					if( padAdapter.LS_D != ls_d )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : LS(D) Change" ) ;	// アンドウバッファに登録
						padAdapter.LS_D = ls_d ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "D", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//----------------------------------

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.green ;
				GUILayout.Label( "Axis : R-Stick" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool rs_r = EditorGUILayout.Toggle( padAdapter.RS_R, GUILayout.Width( 16f ) ) ;
					if( padAdapter.RS_R != rs_r )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : RS(R) Change" ) ;	// アンドウバッファに登録
						padAdapter.RS_R = rs_r ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "R", GUILayout.Width( 32f ) ) ;

					bool rs_l = EditorGUILayout.Toggle( padAdapter.RS_L, GUILayout.Width( 16f ) ) ;
					if( padAdapter.RS_L != rs_l )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : RS(L) Change" ) ;	// アンドウバッファに登録
						padAdapter.RS_L = rs_l ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "L", GUILayout.Width( 32f ) ) ;

					bool rs_u = EditorGUILayout.Toggle( padAdapter.RS_U, GUILayout.Width( 16f ) ) ;
					if( padAdapter.RS_U != rs_u )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : RS(U) Change" ) ;	// アンドウバッファに登録
						padAdapter.RS_U = rs_u ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "U", GUILayout.Width( 32f ) ) ;

					bool rs_d = EditorGUILayout.Toggle( padAdapter.RS_D, GUILayout.Width( 16f ) ) ;
					if( padAdapter.RS_D != rs_d )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : RS(D) Change" ) ;	// アンドウバッファに登録
						padAdapter.RS_D = rs_d ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "D", GUILayout.Width( 32f ) ) ;

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//-------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool axisRepeatPressEnabled = EditorGUILayout.Toggle( padAdapter.AxisRepeatPressEnabled, GUILayout.Width( 16f ) ) ;
					if( padAdapter.AxisRepeatPressEnabled != axisRepeatPressEnabled )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : Axis Repeat Press Enabled Change" ) ;	// アンドウバッファに登録
						padAdapter.AxisRepeatPressEnabled = axisRepeatPressEnabled ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Axis Repeat Press Enabled", GUILayout.Width( 240f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( padAdapter.AxisRepeatPressEnabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Axis Repeat Starting Time", GUILayout.Width( 240f ) ) ;

						float axisRepeatStartingTime = EditorGUILayout.Slider( padAdapter.AxisRepeatPressStartingTime, 0.01f, 10.0f ) ;
						if( padAdapter.AxisRepeatPressStartingTime != axisRepeatStartingTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Axis Repeat Starting Time Change" ) ;	// アンドウバッファに登録
							padAdapter.AxisRepeatPressStartingTime = axisRepeatStartingTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Axis Repeat Interval Time", GUILayout.Width( 240f ) ) ;

						float axisRepeatIntervalTime = EditorGUILayout.Slider( padAdapter.AxisRepeatPressIntervalTime, 0.01f, 10.0f ) ;
						if( padAdapter.AxisRepeatPressIntervalTime != axisRepeatIntervalTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Axis Repeat Interval Time Change" ) ;	// アンドウバッファに登録
							padAdapter.AxisRepeatPressIntervalTime = axisRepeatIntervalTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool axisLongPressEnabled = EditorGUILayout.Toggle( padAdapter.AxisLongPressEnabled, GUILayout.Width( 16f ) ) ;
					if( padAdapter.AxisLongPressEnabled != axisLongPressEnabled )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : Axis Long Press Enabled Change" ) ;	// アンドウバッファに登録
						padAdapter.AxisLongPressEnabled = axisLongPressEnabled ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Axis Long Press Enabled", GUILayout.Width( 240f ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( padAdapter.AxisLongPressEnabled == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( "Axis Long Press Decision Time", GUILayout.Width( 240f ) ) ;

						float axisLongPressDecisionTime = EditorGUILayout.Slider( padAdapter.AxisLongPressDecisionTime, 0.1f, 10.0f ) ;
						if( padAdapter.AxisLongPressDecisionTime != axisLongPressDecisionTime )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : Axis Long Press Decision Time Change" ) ;	// アンドウバッファに登録
							padAdapter.AxisLongPressDecisionTime = axisLongPressDecisionTime ;
							EditorUtility.SetDirty( padAdapter ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				EditorGUILayout.Separator() ;	// 少し区切りスペース
			}

			//----------------------------------

			bool isView = false ;
			if( padAdapter.TryGetComponent<UIView>( out var _ ) == true )
			{
				isView = true ;
			}

			bool isGraphic = false ;
			if( padAdapter.TryGetComponent<Graphic>( out var _ ) == true )
			{
				isGraphic = true ;
			}

			if( isView == true && isGraphic == true )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUI.contentColor = Color.yellow ;
				GUILayout.Label( "Execution" ) ;
				GUI.contentColor = Color.white ;
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool toPress = EditorGUILayout.Toggle( padAdapter.ToPress, GUILayout.Width( 16f ) ) ;
					if( padAdapter.ToPress != toPress )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : ToPress Change" ) ;	// アンドウバッファに登録
						padAdapter.ToPress = toPress ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "ToPress", GUILayout.Width( 64f ) ) ;

					bool toClick = EditorGUILayout.Toggle( padAdapter.ToClick, GUILayout.Width( 16f ) ) ;
					if( padAdapter.ToClick != toClick )
					{
						Undo.RecordObject( padAdapter, "UIPadAdapter : ToClick Change" ) ;	// アンドウバッファに登録
						padAdapter.ToClick = toClick ;
						EditorUtility.SetDirty( padAdapter ) ;
	//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "ToClick", GUILayout.Width( 64f ) ) ;

					if( padAdapter.ToClick == true )
					{
						bool toClickOnDown = EditorGUILayout.Toggle( padAdapter.ToClickOnDown, GUILayout.Width( 16f ) ) ;
						if( padAdapter.ToClickOnDown != toClickOnDown )
						{
							Undo.RecordObject( padAdapter, "UIPadAdapter : ToClickOnDown Change" ) ;	// アンドウバッファに登録
							padAdapter.ToClickOnDown = toClickOnDown ;
							EditorUtility.SetDirty( padAdapter ) ;
		//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
						}
						GUILayout.Label( "OnDown", GUILayout.Width( 64f ) ) ;
					}

					GUILayout.FlexibleSpace() ;	// 左寄せ
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//----------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				var surplusInputEnabled = EditorGUILayout.Toggle( padAdapter.SurplusInputEnabled, GUILayout.Width( 16f ) ) ;
				if( padAdapter.SurplusInputEnabled != surplusInputEnabled )
				{
					Undo.RecordObject( padAdapter, "UIPadAdapter : Surplus Input Enabled Change" ) ;	// アンドウバッファに登録
					padAdapter.SurplusInputEnabled  = surplusInputEnabled ;
					EditorUtility.SetDirty( padAdapter ) ;
				}
				GUILayout.Label( new GUIContent( "Surplus Input Enabled", "レイキャストブロック後の最後の入力を継続させます" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

		}
	}
}

#endif
