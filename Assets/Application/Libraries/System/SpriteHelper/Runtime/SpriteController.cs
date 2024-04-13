using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2023/12/16
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( SpriteRenderer ) )]
	public partial class SpriteController : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/Sprite", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a Sprite" )]					// ポップアップメニューから
		public static void CreateSpriteController()
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child Sprite" ) ;	// アンドウバッファに登録

			var child = new GameObject( "Sprite" ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteController>() ;
			component.SetDefault( true ) ;	// 初期状態に設定する

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ヒエラルキーでの階層パス名を取得する
		/// </summary>
		public string Path
		{
			get
			{
				string path = name ;

				var t = transform.parent ;
				while( t != null )
				{
					path = $"{t.name}/{path}" ;
					t = t.parent ;
				}
				return path ;
			}
		}

		/// <summary>
		/// Component を追加する(ショートカット)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T : UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動くものの表示確認用クラス
		/// </summary>
		public class AsyncState : CustomYieldInstruction
		{
			private readonly MonoBehaviour m_Owner = default ;
			public AsyncState( MonoBehaviour owner )
			{
				// 自身が削除された際にコルーチンの終了待ちをブレイクする施策
				m_Owner = owner ;
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && string.IsNullOrEmpty( Error ) == true && m_Owner != null && m_Owner.gameObject.activeInHierarchy == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 終了したかどうか
			/// </summary>
			public bool IsDone = false ;

			/// <summary>
			/// エラーが発生したかどうか
			/// </summary>
			public string	Error = string.Empty ;

			/// <summary>
			/// 多目的保存値
			/// </summary>
			public System.Object	option ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public void SetDefault( bool useSample = false )
		{
			var spriteRenderer = CSpriteRenderer ;
			if( spriteRenderer == null )
			{
				return ; 
			}

			//----------------------------------

			if( useSample == false )
			{
				// サンプルは設定しない
				return ;
			}

			var sprites = Resources.LoadAll<Sprite>( "SpriteHelper/Textures/Sample" ) ;
			if( sprites != null && sprites.Length >  0 )
			{
				spriteRenderer.sprite = sprites[ 0 ] ;

				SetSprites( sprites ) ;
			}
		}


		//-------------------------------------------------------------------------------------------

		// インスタンスキャッシュ
		private SpriteRenderer	m_SpriteRenderer ;

		/// <summary>
		/// キャッシュされた SpriteRenderer を取得する
		/// </summary>
		public	SpriteRenderer	CSpriteRenderer
		{
			get
			{
				if( m_SpriteRenderer == null )
				{
					TryGetComponent<SpriteRenderer>( out m_SpriteRenderer ) ;
				}
				return m_SpriteRenderer ;
			}
		}

		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// 動作有無のショートカット
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.enabled ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.enabled = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		// SpriteAtlas 限定
		
		// スプライトアトラス(Unity標準機能)
		[SerializeField][HideInInspector]
		private SpriteAtlas m_SpriteAtlas = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  SpriteAtlas  SpriteAtlas
		{
			get
			{
				return m_SpriteAtlas ;
			}
			set
			{
				if( m_SpriteAtlas != value )
				{
					// アトラス内スプライトのキャッシュをクリアする
					CleanupAtlasSprites() ;

					m_SpriteAtlas  = value ;
				}
			}
		}

		// SpriteAtlas から取得した Sprite は Destroy() が必要であるためキャッシュする
		private Dictionary<string,Sprite> m_SpritesInAtlas ;

		//-------------------------------------------------------------------------------------------
		// SpriteSet 限定

		[SerializeField]
		private MultiModeSprite m_SpriteSet = null ;

		/// <summary>
		/// スプライトセットのインスタンス
		/// </summary>
		public  MultiModeSprite  SpriteSet
		{
			get
			{
				return m_SpriteSet ;
			}
			set
			{
				if( m_SpriteSet != value )
				{
					m_SpriteSet  = value ;
				}
			}
		}

		/// <summary>
		/// アトラススプライトの要素となるスプライト群を設定する
		/// </summary>
		/// <param name="sprites"></param>
		/// <returns></returns>
		public bool SetSprites( Sprite[] sprites )
		{
			if( sprites == null || sprites.Length == 0 )
			{
				return false ;
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet == null )
			{
				m_SpriteSet = new MultiModeSprite() ;
			}
			else
			{
				m_SpriteSet.ClearSprites() ;
			}

			m_SpriteSet.SetSprites( sprites ) ;

			//----------------------------------------------------------

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		// アトラス内のスプライトをキャッシュにためつつ取得する
		private Sprite GetSpriteInAtlasFromCache( string spriteName )
		{
			Sprite sprite ;

			if( m_SpritesInAtlas != null )
			{
				if( m_SpritesInAtlas.Count >  0 )
				{
					if( m_SpritesInAtlas.ContainsKey( spriteName ) == true )
					{
						// 既にキャッシュに存在する
						return m_SpritesInAtlas[ spriteName ] ;
					}
				}
			}

			//----------------------------------

			// 実際のアトラスに存在するか確認する
			sprite = m_SpriteAtlas.GetSprite( spriteName ) ;
			if( sprite != null )
			{
				// GetSprite()で取得したSpriteオブジェクトの名前は「"～(Clone)"」のように
				// なっているため、「"(Clone)"」が付かない名前に上書き
				sprite.name = spriteName ;

				// キャッシュを生成する
				m_SpritesInAtlas ??= new Dictionary<string, Sprite>() ;

				// 存在するのでキャッシュに貯める
				m_SpritesInAtlas.Add( spriteName, sprite ) ;
			}

			return sprite ;
		}
		
		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="resize">画像のサイズに合わせてリサイズを行うかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string spriteName, bool resize = false )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					Sprite = sprite ;

//					if( resize == true )
//					{
//						SetNativeSize() ;
//					}

					return true ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					Sprite = sprite ;

//					if( resize == true )
//					{
//						SetNativeSize() ;
//					}

					return true ;
				}
			}

			//----------------------------------------------------------

			return false ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return sprite ;
				}
			}

			//----------------------------------------------------------

			return null ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.width ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string spriteName )
		{
			//----------------------------------------------------------
			// SpriteAtlas

			if( m_SpriteAtlas != null )
			{
				Sprite sprite = GetSpriteInAtlasFromCache( spriteName ) ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------
			// SpriteSet

			if( m_SpriteSet != null )
			{
				Sprite sprite = m_SpriteSet[ spriteName ] ;
				if( sprite != null )
				{
					return ( int )sprite.rect.height ;
				}
			}

			//----------------------------------------------------------

			return 0 ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="color"></param>
		public void SetColor( uint color )
		{
			byte r = ( byte )( ( color >> 16 ) & 0xFF ) ;
			byte g = ( byte )( ( color >>  8 ) & 0xFF ) ;
			byte b = ( byte )( ( color       ) & 0xFF ) ;
			byte a = ( byte )( ( color >> 24 ) & 0xFF ) ;

			Color = new Color32( r, g, b, a ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.sprite : null ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.sprite = value ;
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.color : Color.white ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.color = value ;
				}
			}
		}

		/// <summary>
		/// 左右反転
		/// </summary>
		public bool FlipX
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.flipX ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.flipX = value ;
				}
			}
		}

		/// <summary>
		/// 上下反転
		/// </summary>
		public bool FlipY
		{
			get
			{
				return CSpriteRenderer != null && CSpriteRenderer.flipY ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.flipY = value ;
				}
			}
		}

		/// <summary>
		/// 描画モード
		/// </summary>
		public SpriteDrawMode DrawMode
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.drawMode : SpriteDrawMode.Simple ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.drawMode = value ;
				}
			}
		}

		/// <summary>
		/// 横幅
		/// </summary>
		public float BoundsWidth
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.bounds.size.x : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					var bounds = CSpriteRenderer.bounds ;

					bounds.size = new Vector2( value,  bounds.size.y ) ;

					CSpriteRenderer.bounds = bounds ;
				}
			}
		}

		/// <summary>
		/// 縦幅
		/// </summary>
		public float BoundsHeight
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.bounds.size.y : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					var bounds = CSpriteRenderer.bounds ;

					bounds.size = new Vector2( bounds.size.x, value ) ;

					CSpriteRenderer.bounds = bounds ;
				}
			}
		}


		/// <summary>
		/// 横幅
		/// </summary>
		public float Width
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.size.x : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.size = new Vector2( value,  CSpriteRenderer.size.y ) ;
				}
			}
		}

		/// <summary>
		/// 縦幅
		/// </summary>
		public float Height
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.size.y : 0 ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.size = new Vector2( CSpriteRenderer.size.x, value ) ;
				}
			}
		}

		/// <summary>
		/// マスクインタラクション(スプライトの形にくり抜き制御・スプライトの内側か外側)
		/// </summary>
		public SpriteMaskInteraction MaskInteraction
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.maskInteraction : SpriteMaskInteraction.None ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.maskInteraction = value ;
				}
			}
		}

		/// <summary>
		/// ソートポイント(スプライト同士が同じ SortingLayer OrderInLayer であった時に、どこを基準にソートするか : Center = Center の Y 値が小さい方が手前・Pivot = Pivot の Y 値が小さい方が手前)
		/// </summary>
		public SpriteSortPoint SortPoint
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.spriteSortPoint : SpriteSortPoint.Center ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					 CSpriteRenderer.spriteSortPoint = value ;
				}
			}
		}

		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				return CSpriteRenderer != null ? CSpriteRenderer.material : null ;
			}
			set
			{
				if( CSpriteRenderer != null )
				{
					CSpriteRenderer.material = value ;
				}
			}
		}

		/// <summary>
		/// ソーティングレイヤー名
		/// </summary>
		public string SortingLayerName
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return null ;
				}

				return CSpriteRenderer.sortingLayerName ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.name == value )
					{
						// 発見(設定可能)
						CSpriteRenderer.sortingLayerName = value ;
					}
				}
			}
		}

		/// <summary>
		/// ソーティングレイヤー値
		/// </summary>
		public int SortingLayer
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return -1 ;
				}

				string sortingLayerName = CSpriteRenderer.sortingLayerName ;

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.name == sortingLayerName )
					{
						// 発見
						return layer.value ;
					}
				}

				// 発見出来ず
				return -1 ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				string sortingLayerName = string.Empty ;

				foreach( var layer in  UnityEngine.SortingLayer.layers )
				{
					if( layer.value == value )
					{
						// 発見
						sortingLayerName = layer.name ;
					}
				}

				if( string.IsNullOrEmpty( sortingLayerName ) == true )
				{
					// 発見できず
					return ;
				}

				CSpriteRenderer.sortingLayerName = sortingLayerName ;
			}
		}

		/// <summary>
		/// ソーティングレイヤー(オーダーインレイヤーと同じ)
		/// </summary>
		public int SortingOrder
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 0 ;
				}

				return CSpriteRenderer.sortingOrder ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				CSpriteRenderer.sortingOrder = value ;
			}
		}

		/// <summary>
		/// オーダーインレイヤー
		/// </summary>
		public int OrderInLayer
		{
			get
			{
				if( CSpriteRenderer == null )
				{
					return 0 ;
				}

				return CSpriteRenderer.sortingOrder ;
			}
			set
			{
				if( CSpriteRenderer == null )
				{
					return ;
				}

				CSpriteRenderer.sortingOrder = value ;
			}
		}

		//-----------------------------------------------------
/*
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		protected override void OnBuild( string option = "" )
		{
			Image image = CImage != null ? CImage : gameObject.AddComponent<Image>() ;
			if( image == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			if( option.ToLower() == "panel" )
			{
				// Panel
				image.color = new Color32( 255, 255, 255, 100 ) ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			
				SetAnchorToStretch() ;
//				SetSize( 0, 0 ) ;
			}
			else
			{
				// Default
//				image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
				image.color = Color.white ;
				image.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			}

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			image.raycastTarget = false ;
		}
*/
		/// <summary>
		/// リソースからスプライトをロードする
		/// </summary>
		/// <param name="path">リソースのパス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool LoadSpriteFromResources( string path )
		{
			Sprite sprite = Resources.Load<Sprite>( path ) ;
			if( sprite == null )
			{
				return false ;
			}

			Sprite = sprite ;

			return true ;
		}

		/// <summary>
		/// スプライトを設定する
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		public void SetSprite( Sprite sprite, bool resize = false )
		{
			if( resize == false )
			{
				Sprite = sprite ;
			}
			else
			{
//				SetSpriteAndResize( sprite ) ;
			}
		}
/*
		/// <summary>
		/// スプライトを設定しスプライトのサイズでリサイズする
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		public void SetSpriteAndResize( Sprite sprite )
		{
			Sprite = sprite ;

			if( sprite != null )
			{
				Size = sprite.rect.size ;
			}
		}

		/// <summary>
		/// スプライトを設定し任意のサイズにリサイズする
		/// </summary>
		/// <param name="sprite">スプライトのインスタンス</param>
		/// <param name="size">リサイズ後のサイズ</param>
		public void SetSpriteAndResize( Sprite sprite, Vector2 size )
		{
			Sprite = sprite ;
			Size   = size ;
		}
*/


		//-----------------------------------------------------------

		internal void OnEnable()
		{
		}

		internal void OnDisable()
		{
		}


		internal void Update()
		{
//			Debug.Log( "サイズ : " + CSpriteRenderer.size ) ;

#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tweenChecker = false ;
				var tweens = GetComponents<SpriteTween>() ;
				if( tweens != null && tweens.Length >  0 )
				{
					for( int i  = 0 ; i <  tweens.Length ; i++ )
					{
						if( tweens[ i ].IsChecker == true )
						{
							tweenChecker = true ;
							break ;
						}
					}
				}

				if( tweenChecker == false )
				{
					// ３つの値が異なっていれば更新する
					if( m_LocalPosition != transform.localPosition )
					{
						m_LocalPosition  = transform.localPosition ;
					}
					if( m_LocalRotation != transform.localEulerAngles )
					{
						m_LocalRotation  = transform.localEulerAngles ;
					}
					if( m_LocalScale != transform.localScale )
					{
						m_LocalScale  = transform.localScale ;
					}

					var renderer = CSpriteRenderer ;
					if( renderer != null )
					{
						if( m_LocalAlpha != renderer.color.a )
						{
							m_LocalAlpha  = renderer.color.a ;
						}
					}
				}
			}

			RemoveComponents() ;
#endif
			//----------------------------------------------------------

			// 将来的に連結・多関節キャラクターを扱うのであれば考える
/*
			// 子への色反映(UIButton の場合は別処理を行うのでここでは処理しない)
			if( m_IsApplyColorToChildren == true && m_EffectiveColorReplacing == false )
			{
				ApplyColorToChidren( m_EffectiveColor, true ) ;
			}
*/
		}

#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( m_RemoveCollider == true )
			{
				RemoveCollider() ;
				m_RemoveCollider = false ;
			}

			if( m_RemoveAnimator == true )
			{
				RemoveAnimator() ;
				m_RemoveAnimator = false ;
			}

			if( string.IsNullOrEmpty( m_RemoveTweenIdentity ) == false && m_RemoveTweenInstance != 0 )
			{
				RemoveTween( m_RemoveTweenIdentity, m_RemoveTweenInstance ) ;
				m_RemoveTweenIdentity = null ;
				m_RemoveTweenInstance = 0 ;
			}

			if( string.IsNullOrEmpty( m_RemoveFlipperIdentity ) == false && m_RemoveFlipperInstance != 0 )
			{
				RemoveFlipper( m_RemoveFlipperIdentity, m_RemoveFlipperInstance ) ;
				m_RemoveFlipperIdentity = null ;
				m_RemoveFlipperInstance = 0 ;
			}
		}
#endif







		internal void OnDestroy()
		{
			// アトラス内スプライトのキャッシュをクリアする
			CleanupAtlasSprites() ;
		}

		// キャッシュされたアトラス内スプライト群を破棄する
		private void CleanupAtlasSprites()
		{
			if( m_SpritesInAtlas != null )
			{
				if( m_SpritesInAtlas.Count >  0 )
				{
					if( Application.isPlaying == true )
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								Destroy( sprite.Value ) ;
							}
						}
					}
					else
					{
						foreach( var sprite in m_SpritesInAtlas )
						{
							if( sprite.Value != null )
							{
								DestroyImmediate( sprite.Value ) ;
							}
						}
					}

					m_SpritesInAtlas.Clear() ;
				}

				m_SpritesInAtlas = null ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// サブコンポーネント

		//-------------------------------------------------------------------------------------------

		// コライダー２Ｄキャッシュ
		protected Collider2D m_Collider ;

		/// <summary>
		/// Collider2D(ショートカット)
		/// </summary>
		public virtual Collider2D CCollider
		{
			get
			{
				if( m_Collider == null )
				{
					m_Collider = gameObject.GetComponent<Collider2D>() ;
				}
				return m_Collider ;
			}
		}
		
		/// <summary>
		/// Collider2D の有無
		/// </summary>
		public bool IsCollider
		{
			get
			{
				return ( CCollider != null ) ;
			}
		}
		
		/// <summary>
		/// Collider の追加
		/// </summary>
		public void AddCollider<T>() where T : Collider2D
		{
			if( CCollider != null )
			{
				return ;
			}
		
			T collider ;
		
			collider = gameObject.AddComponent<T>() ;
			collider.enabled = true ;
		}

#if UNITY_EDITOR
		private bool m_RemoveCollider = false ;
#endif

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
			var collider = CCollider ;
			if( collider == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( collider ) ;
			}
			else
			{
				Destroy( collider ) ;
			}
		}


		//----------
		
		// キャッシュ
		private Animator m_Animator = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		public virtual Animator CAnimator
		{
			get
			{
				if( m_Animator == null )
				{
					m_Animator = gameObject.GetComponent<Animator>() ;
				}
				return m_Animator ;
			}
		}
		
		/// <summary>
		/// Animator の有無
		/// </summary>
		public bool IsAnimator
		{
			get
			{
				return ( CAnimator != null ) ;
			}
			set
			{
				if( value == true )
				{
					AddAnimator() ;
				}
				else
				{
#if UNITY_EDITOR
					if( Application.isPlaying == true )
					{
						RemoveAnimator() ;
					}
					else
					{
						m_RemoveAnimator = true ;
					}
#else
					RemoveAnimator() ;
#endif
				}
			}
		}
		
		/// <summary>
		/// Animator の追加
		/// </summary>
		public void AddAnimator()
		{
			if( CAnimator != null )
			{
				return ;
			}
		
			Animator animator ;
		
			animator = gameObject.AddComponent<Animator>() ;
			animator.speed = 1 ;
		}

#if UNITY_EDITOR
		private bool m_RemoveAnimator = false ;
#endif

		/// <summary>
		/// Animator の削除
		/// </summary>
		public void RemoveAnimator()
		{
			var animator = CAnimator ;
			if( animator == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( animator ) ;
			}
			else
			{
				Destroy( animator ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveTweenIdentity = null ;

		public  string  RemoveTweenIdentity
		{
			set
			{
				m_RemoveTweenIdentity = value ;
			}
		}

		private int    m_RemoveTweenInstance = 0 ;

		public  int     RemoveTweenInstance
		{
			set
			{
				m_RemoveTweenInstance = value ;
			}
		}

#endif
		
		/// <summary>
		/// Tween の追加
		/// </summary>
		/// <param name="identity"></param>
		public SpriteTween AddTween( string identity )
		{
			var tween = gameObject.AddComponent<SpriteTween>() ;
			tween.Identity = identity ;

			return tween ;
		}
		
		/// <summary>
		/// Tween の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveTween( string identity, int instance = 0 )
		{
			var tweens = GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return ;
			}
			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && tweens[ i ].Identity == identity ) || ( instance != 0 && tweens[ i ].Identity == identity && tweens[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "[Tween] Not found this identity -> " + identity ) ;
#endif
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tweens[ i ] ) ;
			}
			else
			{
				Destroy( tweens[ i ] ) ;
			}
		}

		/// <summary>
		/// Tween の取得
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public SpriteTween GetTween( string identity )
		{
			var tweens = gameObject.GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tweens[ i ].Identity == identity )
				{
					return tweens[ i ] ;
				}
			}

#if UNITY_EDITOR
			Debug.LogWarning( "[Tween] Not found this identity -> " + identity + " / "+ name ) ;
#endif
			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,SpriteTween> GetAllTweens()
		{
			var tweens = gameObject.GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return null ;
			}

			var targets = new Dictionary<string, SpriteTween>() ;

			int i, l = tweens.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( tweens[ i ].Identity ) == false )
				{
					if( targets.ContainsKey( tweens[ i ].Identity ) == false )
					{
						targets.Add( tweens[ i ].Identity, tweens[ i ] ) ;
					}
				}
			}

			if( targets.Count == 0 )
			{
				return null ;
			}

			return targets ;
		}

		/// <summary>
		/// Tween の Delay と Duration を設定
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool SetTweenTime( string identity, float delay = -1, float duration = -1 )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Delay		= delay ;
			tween.Duration	= duration ;
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// 終了を待つ機構無しに再生する
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool PlayTweenDirect( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,SpriteTween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			return true ;
		}
		
		/// <summary>
		/// 非アクティブ状態の時のみ再生する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenIfHiding( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,SpriteTween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, true, false ) ;
		}

		/// <summary>
		/// 再生終了と同時に非アクティブ状態にする
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public AsyncState PlayTweenAndHide( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,SpriteTween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0 )
		{
			return PlayTween( identity, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, false, true ) ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayTween( string identity, float delay = -1, float duration = -1, float offset = 0, Action<string,SpriteTween> onFinishedAction = null, float additionalDelay = 0, float additionalDuration = 0, bool ifHiding = false, bool autoHide = false )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				Debug.LogWarning( "Not found identity of tween : " + identity + " / " + Path ) ;
				return null ;
			}

			if( ifHiding == true && ( gameObject.activeSelf == true && m_Visible == true ) )
			{
				return new AsyncState( this ){ IsDone = true } ;
			}

			if( autoHide == true && gameObject.activeSelf == false )
			{
				return new AsyncState( this ){ IsDone = true } ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( m_Visible == false )
			{
				Show() ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return new AsyncState( this ){ IsDone = true } ;
			}

			var state = new AsyncState( this ) ;
			StartCoroutine( PlayTweenAsync_Private( tween, delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration, autoHide, state ) ) ;
			return state ;
		}

		public IEnumerator PlayTweenAsync_Private( SpriteTween tween, float delay, float duration, float offset, Action<string,SpriteTween> onFinishedAction, float additionalDelay, float additionalDuration, bool autoHide, AsyncState state )
		{
			// 同じトゥイーンを多重実行出来ないようにする
			if( tween.IsRunning == true || tween.IsPlaying == true )
			{
//				tween.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( tween.IsRunning == true ) | ( tween.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			var destroyAtEnd = tween.DestroyAtEnd ;
			tween.DestroyAtEnd = false ;

			tween.Play( delay, duration, offset, onFinishedAction, additionalDelay, additionalDuration ) ;

			yield return new WaitWhile( () => ( tween.IsRunning == true || tween.IsPlaying == true ) ) ;
			
			state.IsDone = true ;

			if( autoHide == true )
			{
				gameObject.SetActive( false ) ;
			}

			if( destroyAtEnd == true )
			{
				Destroy( tween.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Tween の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsTweenPlaying( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
			{
				return true ;// 実行中
			}
			
			return false ;	
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlaying
		{
			get
			{
				var tweens = gameObject.GetComponents<SpriteTween>() ;
				if( tweens == null || tweens.Length == 0 )
				{
					return false ;
				}

				int i, l = tweens.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tweens[ i ].enabled == true && (  tweens[ i ].IsRunning == true || tweens[ i ].IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool IsAnyTweenPlayingInParents
		{
			get
			{
				if( IsAnyTweenPlaying == true )
				{
					return true ;
				}

				// 親も含めてトゥイーンが動作中か確認する
				var t = transform.parent ;
				while( t != null )
				{
					if( t.TryGetComponent<SpriteController>( out var view ) == true )
					{
						if( view.IsAnyTweenPlaying == true )
						{
							return true ;
						}
					}
					t = t.parent ;
				}

				return false ;
			}
		}

		/// <summary>
		/// Tween の一時停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool PauseTween( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Pause() ;
			return true ;
		}

		/// <summary>
		/// Tween の再開
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool UnpauseTween( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Unpause() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopTween( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Stop() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止と状態のリセット
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopAndResetTween( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.StopAndReset() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool FinishTween( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.Finish() ;
			return true ;
		}

		/// <summary>
		/// 全ての Tween の停止
		/// </summary>
		public bool StopAllTweens()
		{
			var tweens = gameObject.GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true && ( tween.IsRunning == true || tween.IsPlaying == true ) )
				{
					tween.Stop() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool StopAndResetAllTweens()
		{
			var tweens = gameObject.GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.StopAndReset() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool FinishAllTweens()
		{
			var tweens = gameObject.GetComponents<SpriteTween>() ;
			if( tweens == null || tweens.Length == 0 )
			{
				return false ;
			}

			foreach( var tween in tweens )
			{
				if( tween.enabled == true )
				{
					tween.Finish() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を取得する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public float GetTweenProcessTime( string identity )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return 0 ;
			}

			return tween.ProcessTime ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を設定する
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="time"></param>
		public bool SetTweenProcessTime( string identity, float time )
		{
			var tween = GetTween( identity ) ;
			if( tween == null )
			{
				return false ;
			}

			tween.ProcessTime = time ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

#if UNITY_EDITOR
		
		private string m_RemoveFlipperIdentity = null ;

		public  string  RemoveFlipperIdentity
		{
			set
			{
				m_RemoveFlipperIdentity = value ;
			}
		}

		private int    m_RemoveFlipperInstance = 0 ;

		public  int     RemoveFlipperInstance
		{
			set
			{
				m_RemoveFlipperInstance = value ;
			}
		}

#endif

		/// <summary>
		/// Flipper の追加
		/// </summary>
		/// <param name="identity"></param>
		public SpriteFlipper AddFlipper( string identity )
		{
			var flipper = gameObject.AddComponent<SpriteFlipper>() ;
			flipper.Identity = identity ;

			return flipper ;
		}
		
		/// <summary>
		/// Flipper の削除
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="instance"></param>
		public void RemoveFlipper( string identity, int instance = 0 )
		{
			var flippers = GetComponents<SpriteFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return ;
			}

			int i, l = flippers.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( instance == 0 && flippers[ i ].Identity == identity ) || ( instance != 0 && flippers[ i ].Identity == identity && flippers[ i ].GetInstanceID() == instance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity + " / " + name ) ;
#endif
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( flippers[ i ] ) ;
			}
			else
			{
				Destroy( flippers[ i ] ) ;
			}
		}

		/// <summary>
		/// Flipper の取得
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public SpriteFlipper GetFlipper( string identity )
		{
			var flippers = gameObject.GetComponents<SpriteFlipper>() ;
			if( flippers == null || flippers.Length == 0 )
			{
				return null ;
			}

			foreach( var flipper in flippers )
			{
				if( flipper.Identity == identity )
				{
					return flipper ;
				}
			}

#if UNITY_EDITOR
				Debug.LogWarning( "[Flipper] Not found this identity -> " + identity ) ;
#endif
			return null ;
		}

		/// <summary>
		/// Flipper の再生
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="timeScale"></param>
		/// <returns></returns>
		public bool PlayFlipperDirect( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1, Action<string,SpriteFlipper> onFinishedAction = null )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			// アクティブになったタイミングで実行するので親が非アクティブであっても実行自体は行う
//			if( gameObject.activeInHierarchy == false )
//			{
//				// 親が非アクティブならコルーチンは実行できないので終了
//				return true ;
//			}

			flipper.Play( destroyAtEnd, speed, delay, onFinishedAction ) ;

			return true ;
		}

		/// <summary>
		///  Flipper の再生(コルーチン)
		/// </summary>
		/// <param name="identity"></param>
		/// <param name="delay"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState PlayFlipper( string identity, bool destroyAtEnd = false, float speed = 0, float delay = -1 )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				Debug.LogWarning( "Not found identity of flipper : " + identity ) ;
				return null ;
			}

			if( flipper.gameObject.activeSelf == false )
			{
				flipper.gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親が非アクティブならコルーチンは実行できないので終了
				return new AsyncState( this ){ IsDone = true } ;
			}

			var state = new AsyncState( this ) ;
			StartCoroutine( PlayFlipperAsync_Private( flipper, destroyAtEnd, speed, delay, state ) ) ;
			return state ;
		}

		public IEnumerator PlayFlipperAsync_Private( SpriteFlipper flipper, bool destroyAtEnd, float speed, float delay, AsyncState state )
		{
			// 同じフリッパーを多重実行出来ないようにする
			if( flipper.IsRunning == true || flipper.IsPlaying == true )
			{
//				flipper.Stop() ;	// ストップを実行してはならない。古い実行の方で停止されるのを待つ
				yield return new WaitWhile( () => ( ( flipper.IsRunning == true ) | ( flipper.IsPlaying == true ) ) ) ;
			}

			//----------------------------------------------------------

			flipper.Play( false, speed, delay ) ;

			yield return new WaitWhile( () => ( flipper.IsRunning == true || flipper.IsPlaying == true ) ) ;

			state.IsDone = true ;

			if( destroyAtEnd == true )
			{
				Destroy( flipper.gameObject ) ;
			}
		}
		
		/// <summary>
		/// 指定した Flipper の実行中り有無
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool IsFlipperPlaying( string identity )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
			{
				return true ;	// 実行中
			}
			
			return false ;
		}
		
		/// <summary>
		/// いずれかの Flipper の実行中の有無
		/// </summary>
		public bool IsAnyFlipperPlaying
		{
			get
			{
				var flippers = gameObject.GetComponents<SpriteFlipper>() ;
				if( flippers == null || flippers.Length == 0 )
				{
					return false ;
				}

				foreach( var flipper in flippers )
				{
					if( flipper.enabled == true && ( flipper.IsRunning == true || flipper.IsPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// Flipper の完全停止
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public bool StopFlipper( string identity )
		{
			var flipper = GetFlipper( identity ) ;
			if( flipper == null )
			{
				return false ;
			}

			flipper.Stop() ;
			return true ;
		}



		//-------------------------------------------------------------------------------------------
		// Transform 関係


		[SerializeField][HideInInspector]
		private Vector3 m_LocalPosition = Vector3.zero ;

		/// <summary>
		/// 位置(ローカルキャッシュ)
		/// </summary>
		public  Vector3   LocalPosition
		{
			get
			{
				return m_LocalPosition ;
			}
			set
			{
				m_LocalPosition = value ;
			}
		}

		/// <summary>
		/// 位置(ローカル)
		/// </summary>
		public Vector3 Position
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalPosition ;
				}
				else
				{
					return transform.localPosition ;
				}
			}
			set
			{
				m_LocalPosition = value ;
				transform.localPosition = value ;
			}
		}

		/// <summary>
		/// 位置を設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPosition( Vector2 position )
		{
			Position = new Vector3( position.x, position.y, Position.z ) ;
		}

		/// <summary>
		/// 位置を設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPosition( float x, float y )
		{
			Position = new Vector3( x, y, Position.z ) ;
		}

		/// <summary>
		/// 位置Ｘを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionX( float x )
		{
			Position = new Vector3( x, Position.y, Position.z ) ;
		}

		/// <summary>
		/// 位置Ｘ
		/// </summary>
		public float PositionX
		{
			get
			{
				return transform.localPosition.x ;
			}
			set
			{
				transform.localPosition = new Vector3( value, transform.localPosition.y, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}


		/// <summary>
		/// 位置Ｙを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionY( float y )
		{
			Position = new Vector3( Position.x, y, Position.z ) ;
		}

		/// <summary>
		/// 位置Ｙ
		/// </summary>
		public float PositionY
		{
			get
			{
				return transform.localPosition.y ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, value, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}


		/// <summary>
		/// 位置Ｚを設定
		/// </summary>
		/// <param name="position"></param>
		public void SetPositionZ( float z )
		{
			Position = new Vector3( Position.x, Position.y, z ) ;
		}
		
		/// <summary>
		/// 位置Ｚ
		/// </summary>
		public float PositionZ
		{
			get
			{
				return transform.localPosition.z ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, value ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}


		/// <summary>
		/// 基準ポジションを更新する
		/// </summary>
		public void RefreshPosition()
		{
			m_LocalPosition = transform.localPosition ;
		}

		/// <summary>
		/// サイズ(ショートカット)　※
		/// </summary>
		public virtual Vector2 Size
		{
			get
			{
				var spriteRenderer = CSpriteRenderer ;
				if( spriteRenderer != null )
				{
					return spriteRenderer.size ;
				}
				return Vector2.zero ;
			}
			set
			{
				var spriteRenderer = CSpriteRenderer ;
				if( spriteRenderer != null )
				{
					spriteRenderer.size = value ;
				}
			}
		}

		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="size"></param>
		public void SetSize( Vector2 size )
		{
			Size = size ;
		}

		/// <summary>
		/// サイズを設定
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetSize( float w, float h )
		{
			Size = new Vector2( w, h ) ;
		}

		//-----------------------------------

		protected Vector2 m_Anchor ;

		/// <summary>
		/// アンカー値(ショートカット)
		/// </summary>
		public Vector2 Anchor
		{
			get
			{
				return m_Anchor ;
			}
			set
			{
				m_Anchor = value ;
			}
		}
		
		/// <summary>
		/// アンカー最少値を設定
		/// </summary>
		/// <param name="anchorMin"></param>
		public void SetAnchor( Vector2 anchor )
		{
			Anchor = anchor ;
		}
		
		/// <summary>
		/// アンカーの値を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAnchor( float x, float y )
		{
			Anchor = new Vector2( x, y ) ;
		}

		/// <summary>
		/// アンカーＸの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorX( float x )
		{
			Anchor = new Vector2( x, m_Anchor.y ) ;
		}

		/// <summary>
		/// アンカーＹの値を設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetAnchorY( float y )
		{
			Anchor = new Vector2( m_Anchor.x, y ) ;
		}

		//-------------------------------------------------------------------------------------------
		// Rotation

		[SerializeField][HideInInspector]
		private Vector3 m_LocalRotation = Vector3.zero ;
		public  Vector3   LocalRotation
		{
			get
			{
				return m_LocalRotation ;
			}
			set
			{
				m_LocalRotation = value ;
			}
		}

		/// <summary>
		/// ２Ｄでの回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( float axisZ )
		{
			Roll = axisZ ;
		}

		/// <summary>
		/// ３軸での回転角度を設定する
		/// </summary>
		/// <param name="axisZ"></param>
		public void SetRotation( Vector2 value )
		{
			Rotation = value ;
		}

		/// <summary>
		/// ローテーション(ショートカット)
		/// </summary>
		public Vector3 Rotation
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					return transform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;
				transform.localEulerAngles = value ;
			}
		}

		public float Pitch
		{
			get
			{
				return Rotation.x ;
			}
			set
			{
				Rotation = new Vector3( value, Rotation.y, Rotation.z ) ;
			}
		}

		public float Yaw
		{
			get
			{
				return Rotation.y ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, value, Rotation.z ) ;
			}
		}

		public float Roll
		{
			get
			{
				return Rotation.z ;
			}
			set
			{
				Rotation = new Vector3( Rotation.x, Rotation.y, value ) ;
			}
		}

		/// <summary>
		/// 基準ローテーションを更新する
		/// </summary>
		public void RefreshRotation()
		{
			m_LocalRotation = transform.localEulerAngles ;
		}

		//---------------
		// Scale

		[SerializeField][HideInInspector]
		private Vector3 m_LocalScale = Vector3.one ;
		public  Vector3   LocalScale
		{
			get
			{
				return m_LocalScale ;
			}
			set
			{
				m_LocalScale = value ;
			}
		}

		/// <summary>
		/// スケール(ショートカット)
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalScale ;
				}
				else
				{
					return transform.localScale ;
				}
			}
			set
			{
				m_LocalScale = value ;
				transform.localScale = value ;
			}
		}
		
		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="s"></param>
		public void SetScale( float s )
		{
			Scale = new Vector3( s, s, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y )
		{
			Scale = new Vector3( x, y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y, float z )
		{
			Scale = new Vector3( x, y, z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector2 scale )
		{
			Scale = new Vector3( scale.x, scale.y, Scale.z ) ;
		}

		/// <summary>
		/// スケールを設定
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector3 scale )
		{
			Scale = scale ;
		}

		/// <summary>
		/// 基準スケールを更新する
		/// </summary>
		public void RefreshScale()
		{
			m_LocalScale = transform.localScale ;
		}

		//-----------------------------------
		// Alpha

		[SerializeField][HideInInspector]
		private float m_LocalAlpha = 1.0f ;
		public  float   LocalAlpha
		{
			get
			{
				return m_LocalAlpha ;
			}
			set
			{
				m_LocalAlpha = value ;
			}
		}

		/// <summary>
		/// アルファ値を設定する
		/// </summary>
		/// <param name="alpha"></param>
		public void SetAlpha( float alpha )
		{
			Alpha = alpha ;
		}

		/// <summary>
		/// α値
		/// </summary>
		public float Alpha
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalAlpha ;
				}
				else
				{
					var renderer = CSpriteRenderer ;
					if( renderer != null )
					{
						return renderer.color.a ;
					}
					else
					{
						return 0 ;
					}
				}
			}
			set
			{
				m_LocalAlpha = value ;

				var renderer = CSpriteRenderer ;
				if( renderer != null )
				{
					Color color = renderer.color ;
					color.a = value ;
					renderer.color = color ;
				}
			}
		}


		//-------------------------------------------------------------------------------------------

		// タイムスケール
		[SerializeField]
		protected float m_TimeScale = 1.0f ;

		/// <summary>
		/// タイムスケール
		/// </summary>
		public float TimeScale
		{
			get
			{
				return m_TimeScale ;
			}
			set
			{
				if( m_TimeScale != value )
				{
					m_TimeScale  = value ;

					// 注意 : １度でも Animator を使った事が無いと m_Animator にはインスタンスが記録されない(キャッシュ)
					if( m_Animator != null )
					{
						m_Animator.speed = m_TimeScale ;
					}

					OnTimeScaleChanged( m_TimeScale ) ;
				}
			}
		}

		/// <summary>
		/// タイムスケールが変更された際に呼び出される
		/// </summary>
		/// <param name="timeScale"></param>
		protected virtual void OnTimeScaleChanged( float timeScale ){}

		//-----------------------------------

		// 画面表示状態
		private bool m_Visible = true ;	// 自身を含めた子を画面に表示するか

		/// <summary>
		/// 画面表示状態
		/// </summary>
		public bool Visible
		{
			get
			{
				return m_Visible ;
			}
		}

		/// <summary>
		/// 表示状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetVisible( bool state )
		{
			if( state == false )
			{
				Hide() ;
			}
			else
			{
				Show() ;
			}
		}

		/// <summary>
		/// 自身を含めて子を非表示にする
		/// </summary>
		public void Hide()
		{
			m_Visible = false ;

			Alpha = m_LocalAlpha ;
		}

		public void Show()
		{
			m_Visible = true ;

			Alpha = m_LocalAlpha ;
		}


	}
}

