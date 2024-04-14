#if UNITY_EDITOR

using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;


namespace SpriteHelper
{
	/// <summary>
	/// UIFlipper のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( SpriteFlipper ) ) ]
	public class SpriteFlipperInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteFlipper ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			var identity = EditorGUILayout.TextField( "Identity",  component.Identity ) ;
			GUI.backgroundColor = Color.white ;
			if( identity != component.Identity )
			{
				Undo.RecordObject( component, "[SpriteFlipper] Identity Change" ) ;	// アンドウバッファに登録
				component.Identity = identity ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float delay = EditorGUILayout.FloatField( "Delay",  component.Delay ) ;
			if( delay != component.Delay )
			{
				Undo.RecordObject( component, "[SpriteFlipper] Delay Change" ) ;	// アンドウバッファに登録
				component.Delay = delay ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			if( component.GetComponent<SpriteRenderer>() == null )
			{
				EditorGUILayout.HelpBox( GetMessage( "SpriteRendererNone" ), MessageType.Warning, true ) ;		
			}

			// ワイドモードを有効にする
	//		bool tWideMode = EditorGUIUtility.wideMode ;
	//		EditorGUIUtility.wideMode = true ;


			//--------------------------------------------------------------------

			// 一番肝心なスプライトアニメーションファイル
			SpriteFlipperAnimation spriteAnimation = EditorGUILayout.ObjectField( "Sprite Animation", component.SpriteAnimation, typeof( SpriteFlipperAnimation ), false ) as SpriteFlipperAnimation ;
			if( spriteAnimation != component.SpriteAnimation )
			{
				Undo.RecordObject( component, "[SpriteFlipper] Sprite Animation Change " ) ;	// アンドウバッファに登録
				component.SpriteAnimation = spriteAnimation ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( component.enabled == true && component.SpriteAnimation != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// チェック
					GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

					bool isChecker = EditorGUILayout.Toggle( component.IsChecker ) ;
					if( isChecker != component.IsChecker )
					{
						if( isChecker == true )
						{
							var flippers = component.gameObject.GetComponents<SpriteFlipper>() ;
							if( flippers != null && flippers.Length >  0 )
							{
								for( int i  = 0 ; i <  flippers.Length ; i ++ )
								{
									if( flippers[ i ] != component )
									{
										if( flippers[ i ].IsChecker == true )
										{
											flippers[ i ].IsChecker  = false ;
										}
									}
								}
							}
						}


						Undo.RecordObject( component, "[SpriteFlipper] Checker Change" ) ;	// アンドウバッファに登録
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
						int checkFactor = EditorGUILayout.IntSlider( component.CheckFactor, 0, component.SpriteAnimation.Length - 1 ) ;
						if( checkFactor != component.CheckFactor )
						{
							Undo.RecordObject( component, "[SpriteFlipper] Check Factor Change " ) ;	// アンドウバッファに登録
							component.CheckFactor = checkFactor ;
							EditorUtility.SetDirty( component ) ;
//							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

	/*						var renderer = component.GetComponent<SpriteRenderer>() ;
							if( renderer != null )
							{
								EditorUtility.SetDirty( renderer ) ;
							}*/
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}


				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// Begin
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Begin", GUILayout.Width( 60f ) ) ;
					int begin = EditorGUILayout.IntSlider( component.Begin, 0, component.SpriteAnimation.Length - 1 ) ;
					if( begin != component.Begin )
					{
						Undo.RecordObject( component, "[SpriteFlipper] Begin Change " ) ;	// アンドウバッファに登録
						component.Begin = begin ;
						EditorUtility.SetDirty( component ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// end
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "End", GUILayout.Width( 60f ) ) ;
					int end = EditorGUILayout.IntSlider( component.End, 0, component.SpriteAnimation.Length - 1 ) ;
					if( end != component.End )
					{
						Undo.RecordObject( component, "[SpriteFlipper] End Change " ) ;	// アンドウバッファに登録
						component.End = end ;
						EditorUtility.SetDirty( component ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// バック
				GUILayout.Label( "Back", GUILayout.Width( 116f ) ) ;

				bool back = EditorGUILayout.Toggle( component.Back ) ;
				if( back != component.Back )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Back Change" ) ;	// アンドウバッファに登録
					component.Back = back ;
					EditorUtility.SetDirty( component ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool loop = EditorGUILayout.Toggle( component.Loop ) ;
				if( loop != component.Loop )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Loop Change" ) ;	// アンドウバッファに登録
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
						Undo.RecordObject( component, "[SpriteFlipper] Reverse Change" ) ;	// アンドウバッファに登録
						component.Reverse = reverse ;
						EditorUtility.SetDirty( component ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			// スピード
			float speed = EditorGUILayout.FloatField( "Speed",  component.Speed ) ;
			if( speed != component.Speed )
			{
				Undo.RecordObject( component, "[SpriteFlipper] Speed Change" ) ;	// アンドウバッファに登録
				component.Speed = speed ;
				EditorUtility.SetDirty( component ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool ignoreTimeScale = EditorGUILayout.Toggle( component.IgnoreTimeScale ) ;
				if( ignoreTimeScale != component.IgnoreTimeScale )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					component.IgnoreTimeScale = ignoreTimeScale ;
					EditorUtility.SetDirty( component ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool playOnAwake = EditorGUILayout.Toggle( component.PlayOnAwake ) ;
				if( playOnAwake != component.PlayOnAwake )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Play On Awake Change" ) ;	// アンドウバッファに登録
					component.PlayOnAwake = playOnAwake ;
					EditorUtility.SetDirty( component ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;     // 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool destroyAtEnd = EditorGUILayout.Toggle( component.DestroyAtEnd ) ;
				if( destroyAtEnd != component.DestroyAtEnd )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Destroy At End Change" ) ;	// アンドウバッファに登録
					component.DestroyAtEnd = destroyAtEnd ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Auto Resize", GUILayout.Width( 116f ) ) ;

				bool autoResize = EditorGUILayout.Toggle( component.AutoResize ) ;
				if( autoResize != component.AutoResize )
				{
					Undo.RecordObject( component, "[SpriteFlipper] Auto Resize Change" ) ;	// アンドウバッファに登録
					component.AutoResize = autoResize ;
					EditorUtility.SetDirty( component ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( component.IsPlaying ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;   // 少し区切りスペース


			// デリゲートの設定状況
			var so = new SerializedObject( component ) ;

			var sp = so.FindProperty( "onFinished" ) ;
			if( sp != null )
			{
				EditorGUILayout.PropertyField( sp ) ;
			}
			so.ApplyModifiedProperties() ;
		}


		//--------------------------------------------------------------------------

		private static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "SpriteRendererNone", "SpriteRenderer クラスが必要です" },
		} ;
		private static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "SpriteRendererNone", "'SpriteRenderer' is necessary." },
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
