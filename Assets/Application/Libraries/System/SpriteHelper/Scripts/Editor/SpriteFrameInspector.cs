#if UNITY_EDITOR

using System.Collections.Generic ;
using System.Linq ;
using System.Text.RegularExpressions ;
using System.Reflection ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;
using UnityEditor ;


namespace SpriteHelper
{
	public static class SerializedPropertyExtensions
	{
		/// <summary>
		/// リストの要素Indexを返す
		/// </summary>
		public static int GetArrayElementIndex( this SerializedProperty property )
		{
			// プロパティがリストのインデックスであれば、パスは(変数名).Array.data[(インデックス)] 
			// となるため、この文字列からインデックスを取得する

			// リストの要素であるか判定する
			var match = Regex.Match( property.propertyPath, "^([a-zA-Z0-9_]*).Array.data\\[([0-9]*)\\]$" ) ;
			if( match.Success == false )
			{
				return -1 ;
			}

			// Indexを抜き出す
			var splitPath = property.propertyPath.Split( '.' ) ;
			var regax = new Regex( @"[^0-9]" ) ;
			if( int.TryParse( regax.Replace( splitPath[ ^1 ], "" ), out var index ) == false )
			{
				return -1 ;
			}

			return index ;
		}
	}


	[ CustomEditor( typeof( SpriteFrame ), true ) ]
	public class SpriteFrameInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript() ;

			// ボールド
//			var boldStyle = new GUIStyle( GUI.skin.label )
//			{
//				fontStyle = FontStyle.Bold
//			} ;

			if( target.GetType() != typeof( SpriteFrame ) )
			{
				// デフォルトの描画
				DrawDefaultInspector() ;

				DrawSeparater() ;
			}

			//----------------------------------------------------------

			// ターゲットのインスタンス
			var component = target as SpriteFrame ;

			//----------------------------------

			DrawCollider( component ) ;

			DrawAnimator( component ) ;

			//----------------------------------------------------------

			serializedObject.ApplyModifiedProperties() ;
		}

		//-------------------------------------------------------------------------------------------
		// Collider

		private int		m_ColliderIndex			= 0 ;
		private bool	m_ColliderRemoveAready	= false ;

		private bool	m_Tips_Foldout			= false ;

		protected void DrawCollider( SpriteFrame component )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			var colliderTypeNames = new string[]
			{
				"None",
				"BoxCollider2D",
				"CircleCollider2D",
				"CapsuleCollider2D",
				"PolygonCollider2D",
				"EdgeCollider2D",
				"CompositeCollider2D",
				"CustomCollider2D",
			} ;

			var collider = component.CCollider ;

			if( collider == null )
			{
				// コライダーは無し

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUILayout.Label( new GUIContent( "Collider2D", "<color=#00FFFF>Collider2D</color>コンポーネントの追加または削除を行います" ), GUILayout.Width( 80f ) ) ;

					m_ColliderIndex = EditorGUILayout.Popup( "", m_ColliderIndex, colliderTypeNames, GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;	// フィールド名有りタイプ

					if( m_ColliderIndex >  0 )
					{
						bool isAdd = false ;

						GUI.backgroundColor = Color.cyan ;
						if( GUILayout.Button( new GUIContent( "Add", "<color=#00FFFF>Collider</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します" ), GUILayout.Width( 60f ) ) == true )
						{
							isAdd = true ;
						}
						GUI.backgroundColor = Color.white ;

						if( isAdd == true )
						{
							// Collider を追加する

							switch( m_ColliderIndex )
							{
								case 1 : component.AddCollider<BoxCollider2D>()			; break ;
								case 2 : component.AddCollider<CircleCollider2D>()		; break ;
								case 3 : component.AddCollider<CapsuleCollider2D>()		; break ;
								case 4 : component.AddCollider<PolygonCollider2D>()		; break ;
								case 5 : component.AddCollider<EdgeCollider2D>()		; break ;
								case 6 : component.AddCollider<CompositeCollider2D>()	; break ;
								case 7 : component.AddCollider<CustomCollider2D>()		; break ;
							}
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
			else
			{
				// コライダーは有り

				if( m_ColliderRemoveAready == false )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						GUILayout.Label( new GUIContent( "Collider2D", "<color=#00FFFF>Collider2D</color>コンポーネントの追加または削除を行います" ), GUILayout.Width( 80f ) ) ;

						if( collider is BoxCollider2D		){ m_ColliderIndex = 1 ; }
						if( collider is CircleCollider2D	){ m_ColliderIndex = 2 ; }
						if( collider is CapsuleCollider2D	){ m_ColliderIndex = 3 ; }
						if( collider is PolygonCollider2D	){ m_ColliderIndex = 4 ; }
						if( collider is EdgeCollider2D		){ m_ColliderIndex = 5 ; }
						if( collider is CompositeCollider2D	){ m_ColliderIndex = 6 ; }
						if( collider is CustomCollider2D	){ m_ColliderIndex = 7 ; }

						EditorGUILayout.TextField( "", colliderTypeNames[ m_ColliderIndex ], GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 144f ) ) ;

						bool isRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を赤に
						if( GUILayout.Button( new GUIContent( "Remove", "<color=#00FFFF>Collider</color>コンポーネントを\nこの<color=#00FF00>GameObjectから削除</color>します" ), GUILayout.Width( 60f ) ) == true )
						{
							isRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を白に

						if( isRemove == true )
						{
							// 削除確認へ
							m_ColliderRemoveAready = true ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
				else
				{
					// 実際の破棄の確認と実行
					var message = GetMessage( "RemoveColliderOK?" ).Replace( "%1", colliderTypeNames[ m_ColliderIndex ] ) ;
					GUILayout.Label( message ) ;

					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						GUI.backgroundColor = Color.red ;
						if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
						{
							// 本当に削除する
							Undo.RecordObject( component, $"[SpriteController] {colliderTypeNames[ m_ColliderIndex ]} Remove" ) ;	// アンドウバッファに登録
							component.RemoveCollider() ;
							EditorUtility.SetDirty( component ) ;
							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

							m_ColliderRemoveAready = false ;
						}
						GUI.backgroundColor = Color.white ;
						if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
						{
							m_ColliderRemoveAready = false ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}

				//-----------------------------------------------------------------------------------------

				// コライダーの自動調整
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "", GUILayout.Width( 8f ) ) ;
					bool colliderAdjustment = EditorGUILayout.Toggle( component.ColliderAdjustment, GUILayout.Width( 16f ) ) ;
					if( colliderAdjustment != component.ColliderAdjustment )
					{
						Undo.RecordObject( component, "SpriteFrame : Collider Adjustment Change" ) ;	// アンドウバッファに登録
						component.ColliderAdjustment = colliderAdjustment ;
						EditorUtility.SetDirty( component ) ;
					}
					GUILayout.Label( new GUIContent( "Collider Adjustment", "コライダーのサイズをメッシュのサイズに自動的に合わせるかどうか" ) ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( component.ColliderAdjustment == true )
				{
					// Margin
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						GUILayout.Label( new GUIContent( "　Margin", "マージンです(非反転基準)" ), GUILayout.MinWidth( 40f ), GUILayout.MaxWidth( 80f ) ) ;

						GUILayout.Label( "L", GUILayout.Width( 12f ) ) ;
						var colliderMarginL = EditorGUILayout.FloatField( component.ColliderMarginL, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 32f ) ) ;
						if( component.ColliderMarginL != colliderMarginL )
						{
							Undo.RecordObject( component, "SpriteFrame : Colider Margin L Change" ) ;	// アンドウバッファに登録
							component.ColliderMarginL  = colliderMarginL ;
							EditorUtility.SetDirty( component ) ;
						}

						GUILayout.Label( "R", GUILayout.Width( 12f ) ) ;
						var colliderMarginR  = EditorGUILayout.FloatField( component.ColliderMarginR, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 32f ) ) ;
						if( component.ColliderMarginR != colliderMarginR )
						{
							Undo.RecordObject( component, "SpriteFrame : Collider Margin R Change" ) ;	// アンドウバッファに登録
							component.ColliderMarginR  = colliderMarginR ;
							EditorUtility.SetDirty( component ) ;
						}

						GUILayout.Label( "T", GUILayout.Width( 12f ) ) ;
						var colliderMarginT  = EditorGUILayout.FloatField( component.ColliderMarginT, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 32f ) ) ;
						if( component.ColliderMarginT != colliderMarginT )
						{
							Undo.RecordObject( component, "SpriteFrame : Collider Margin T Change" ) ;	// アンドウバッファに登録
							component.ColliderMarginT  = colliderMarginT ;
							EditorUtility.SetDirty( component ) ;
						}

						GUILayout.Label( "B", GUILayout.Width( 12f ) ) ;
						var colliderMarginB  = EditorGUILayout.FloatField( component.ColliderMarginB, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 32f ) ) ;
						if( component.ColliderMarginB != colliderMarginB )
						{
							Undo.RecordObject( component, "SpriteFrame : Collider Margin B Change" ) ;	// アンドウバッファに登録
							component.ColliderMarginB  = colliderMarginB ;
							EditorUtility.SetDirty( component ) ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					if( m_ColliderIndex == 5 )	// EdgeCollider 限定
					{
						// Edge Width
						GUILayout.BeginHorizontal() ;	// 横並び
						{
							GUILayout.Label( new GUIContent( "　Edge Width", "表示の優先順位(小さい方が手前に表示される)" ), GUILayout.MinWidth( 64f ), GUILayout.MaxWidth( 96f ) ) ;

							var colliderEgdeWidth = EditorGUILayout.FloatField( component.ColliderEdgeWidth, GUILayout.MinWidth( 16f ), GUILayout.MaxWidth( 48f ) ) ;
							if( component.ColliderEdgeWidth != colliderEgdeWidth )
							{
								Undo.RecordObject( component, "SpriteFrame : Collider Edge Width Change" ) ;	// アンドウバッファに登録
								component.ColliderEdgeWidth  = colliderEgdeWidth ;
								EditorUtility.SetDirty( component ) ;
							}
						}
						GUILayout.EndHorizontal() ;		// 横並び終了
					}
				}

				//-----------------------------------------------------------------------------------------
				// 忘れやすい点の補足メモ

				m_Tips_Foldout = EditorGUILayout.Foldout( m_Tips_Foldout, "Tips" ) ;
  
				if( m_Tips_Foldout == true )
				{
					EditorGUILayout.HelpBox( "衝突◯ 接触✕ : Collider の IsTrigger = false\n衝突✕ 接触◯ : Collider の IsTrigger = true", MessageType.Info, true ) ;
					EditorGUILayout.HelpBox( "物理移動◯ : Rigidbody の BodyType = Dynamic\n物理移動✕ : Rigidbody の BodyType = Static", MessageType.Info, true ) ;
					EditorGUILayout.HelpBox( "自由落下◯ : Rigidbody の GravityScale > 0\n自由落下✕ : Rigidbody の GravityScale = 0", MessageType.Info, true ) ;
					EditorGUILayout.HelpBox( "Ｚ軸回転◯ : Rigidbody の FreezeRotation = false\nＺ軸回転✕ : Rigidbody の FreezeRotation = true", MessageType.Info, true ) ;
					EditorGUILayout.HelpBox( "垂直移動時の側壁の引っ掛かり無効化 :\nPhysicsMaterial を設定(Friction = 0・Bounciness = 0)", MessageType.Info, true ) ;
					EditorGUILayout.HelpBox( "水平移動時の地面の引っ掛かり無効化 :\nEdgeCollider を使用(EdgeWidth > 0)", MessageType.Info, true ) ;
				}
			}
		}

		//--------------------------------------------------------------------------
		// Animator

		// アニメーターの生成破棄チェックボックスを描画する
		protected void DrawAnimator( SpriteFrame controller )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool isAnimator = EditorGUILayout.Toggle( controller.IsAnimator, GUILayout.Width( 16f ) ) ;
				if( isAnimator != controller.IsAnimator )
				{
					Undo.RecordObject( controller, "[SpriteController] Animator Change" ) ;	// アンドウバッファに登録
					controller.IsAnimator = isAnimator ;
					EditorUtility.SetDirty( controller ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( new GUIContent( "Animator", "<color=#00FFFF>Animator</color>コンポーネントを\nこの<color=#00FF00>GameObjectに追加</color>します\n<color=#00FFFF>PlayAnimator</color>メソッドを実行する際に必要になります" ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}

		//--------------------------------------------------------------------------

		protected static readonly Dictionary<string,string> m_Japanese_Message = new ()
		{
			{ "RemoveTweenOK?",		"Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?",	"Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone",	"EventTrigger クラスが必要です" },
			{ "InputIdentity",		"識別子を入力してください" },

			{ "RemoveColliderOK?",	"[ %1 ] を削除してもよろしいですか？" },
		} ;
		protected static readonly Dictionary<string,string> m_English_Message = new ()
		{
			{ "RemoveTweenOK?",		"It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?",	"It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone",	"'EventTrigger' is necessary." },
			{ "InputIdentity",		"Input identity !" },

			{ "RemoveColliderOK?",   "It does really may be to remove %1 ?" },
		} ;

		protected static string GetMessage( string label )
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

		//-------------------------------------------------------------------------------------------

		// 区切り線
		protected void DrawSeparater()
		{
			EditorGUILayout.Space( 8 ) ;	// 少し区切りスペース

			var rect = GUILayoutUtility.GetRect( Screen.width, 2f ) ;

			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 0, rect.width - 0, 1 ), Color.white ) ;
			EditorGUI.DrawRect( new Rect( rect.x + 0, rect.y + 1, rect.width - 0, 1 ), Color.black ) ;

			EditorGUILayout.Space( 8 ) ;	// 少し区切りスペース
		}
	}
}

#endif

