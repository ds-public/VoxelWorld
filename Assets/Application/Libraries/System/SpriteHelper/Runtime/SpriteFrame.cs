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
	/// スプライト制御クラス  Version 2024/08/16
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( SpriteTransform ) )]
	public partial class SpriteFrame : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteFrame", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteFrame" )]					// ポップアップメニューから
		public static void CreateSpriteFrame()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child SpriteFrame" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteFrame" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteFrame>() ;
			component.SetDefault() ;	// 初期状態に設定する

			// 一番上に移動させる
			while( ComponentUtility.MoveComponentUp( component ) ){}

			if( component.TryGetComponent<SpriteTransform>( out var spriteTransform ) == true )
			{
				// SpriteDrawer を一番上に移動させる
				while( ComponentUtility.MoveComponentUp( spriteTransform ) ){}
			}

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
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public virtual void SetDefault()
		{
			// 予めキャッシュしておく

			_ = CachedSpriteCanvas ;
			_ = CachedSpriteTransform ;
		}

		//-------------------------------------------------------------------------------------------

		// 属しているスプライトスクリーン
		[SerializeField]
		protected SpriteCanvas			m_CachedSpriteCanvas ;

		// 同じ GameObject に付いているSpriteTransform
		[SerializeField]
		protected SpriteTransform		m_CachedSpriteTransform ;

		//--------------------------------------------------------------------------------------------

		// ヒエラルキーの構造
		[SerializeField][HideInInspector]
		private int[]			m_ActiveHierarchy = new int[ 64 ] ;
		[SerializeField][HideInInspector]
		private int				m_ActiveHierarchy_Count ;
		[SerializeField][HideInInspector]
		private int[]			m_CachedHierarchy = new int[ 64 ] ;
		[SerializeField][HideInInspector]
		private int				m_CachedHierarchy_Count ;

		// ヒエラルキーを更新する
		private void UpdateActiveHierarchy()
		{
			m_ActiveHierarchy_Count = 0 ;

			Transform t = transform ;
			while( t != null )
			{
				m_ActiveHierarchy[ m_ActiveHierarchy_Count ] = t.GetInstanceID() ;
				m_ActiveHierarchy_Count ++ ;
				t = t.parent ;
			}
		}

		// ヒエラルキーを更新する
		private void UpdateCachedHierarchy()
		{
			Array.Copy( m_ActiveHierarchy, m_CachedHierarchy, m_ActiveHierarchy_Count ) ;
			m_CachedHierarchy_Count = m_ActiveHierarchy_Count ;
		}

		// ヒエラルキーを比較する
		private bool CompareHierarchy()
		{
			if( m_CachedHierarchy_Count != m_ActiveHierarchy_Count )
			{
				// 異なる
				return false ;
			}

			int i, l = m_CachedHierarchy_Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CachedHierarchy[ i ] != m_ActiveHierarchy[ i ] )
				{
					// 異なる
					return false ;
				}
			}

			// 等しい
			return true ;
		}

		/// <summary>
		/// キャッシュされたスプライトキャンバス
		/// </summary>
		public SpriteCanvas		CachedSpriteCanvas
		{
			get
			{
				// ヒエラルキーの構成に変化があったかどうか確認する

				UpdateActiveHierarchy() ;

				if( CompareHierarchy() == true && m_CachedSpriteCanvas != null )
				{
					// ヒエラルキーの構成に変化は無く親キャンバスのキャッシュが存在する
					return m_CachedSpriteCanvas ;
				}

				UpdateCachedHierarchy() ;

				//---------------------------------------------------------

				// キャンバスを取得し直す
				m_CachedSpriteCanvas = GetComponentInParent<SpriteCanvas>( true ) ;

				return m_CachedSpriteCanvas ;
			}
		}

		/// <summary>
		/// キャッシュされたスプライトキャンバス
		/// </summary>
		public SpriteCanvas		ParentCanvas => CachedSpriteCanvas ; 


		/// <summary>
		/// キャッシュされたスプライトトランスフォーム
		/// </summary>
		public SpriteTransform	CachedSpriteTransform
		{
			get
			{
				if( m_CachedSpriteTransform == null )
				{
					TryGetComponent<SpriteTransform>( out m_CachedSpriteTransform ) ;
				}
				return m_CachedSpriteTransform ;
			}
		}

		/// <summary>
		/// キャッシュされたスプライトトランスフォーム
		/// </summary>
		public SpriteTransform CST => CachedSpriteTransform ;

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

		/// <summary>
		/// アクティブ状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// アクティブ状態
		/// </summary>
		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		//-------------------------------------------------------------------------------------------


		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPosition( float x, float y )
		{
			CST.SetPosition( x, y ) ;
		}


		/// <summary>
		/// Ｘ方向の移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void MoveX( float velocity, bool multiplyTime = true )
		{
			CST.PositionX += ( velocity * ( multiplyTime == true ? Time.deltaTime : 1 ) ) ;

			// ※Update から呼び出す場合に multiplyTime が true であれば velocity は 1 秒あたりの移動量となる
			// ※FixedUpdate から呼び出す場合は multiplyTime は false にして velocity は FixedUpdate 呼び出し間隔想定にする
		}

		/// <summary>
		/// Ｙ方向の移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void MoveY( float velocity, bool multiplyTime = true )
		{
			CST.PositionY += ( velocity * ( multiplyTime == true ? Time.deltaTime : 1 ) ) ;

			// ※Update から呼び出す場合に multiplyTime が true であれば velocity は 1 秒あたりの移動量となる
			// ※FixedUpdate から呼び出す場合は multiplyTime は false にして velocity は FixedUpdate 呼び出し間隔想定にする
		}

		/// <summary>
		/// 移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void Move( float velocityX, float velocityY, bool multiplyTime = true )
		{
			float deltaTime = multiplyTime == true ? Time.deltaTime : 1 ;

			CST.PositionX += ( velocityX * deltaTime ) ;
			CST.PositionY += ( velocityY * deltaTime ) ;

			// ※Update から呼び出す場合に multiplyTime が true であれば velocity は 1 秒あたりの移動量となる
			// ※FixedUpdate から呼び出す場合は multiplyTime は false にして velocity は FixedUpdate 呼び出し間隔想定にする
		}

		/// <summary>
		/// 移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void Move( Vector2 velocity, bool multiplyTime = true )
		{
			float deltaTime = multiplyTime == true ? Time.deltaTime : 1 ;

			CST.PositionX += ( velocity.x * deltaTime ) ;
			CST.PositionY += ( velocity.y * deltaTime ) ;

			// ※Update から呼び出す場合に multiplyTime が true であれば velocity は 1 秒あたりの移動量となる
			// ※FixedUpdate から呼び出す場合は multiplyTime は false にして velocity は FixedUpdate 呼び出し間隔想定にする
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Ｘ方向の移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void SetVelocityX( float velocity )
		{
			if( CRigidbody == null )
			{
				return ;
			}

//			rigidbody.AddForceX( velocity, ForceMode2D.Impulse ) ;
			m_Rigidbody.velocityX = velocity ;
		}

		/// <summary>
		/// Ｘ方向の移動を行う(multiplyTime が有効になるのは Update から呼び出した場合のみ)
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="multiplyTime"></param>
		public void SetVelocityY( float velocity )
		{
			if( CRigidbody == null )
			{
				return ;
			}

//			rigidbody.AddForceY( velocity, ForceMode2D.Impulse ) ;
			m_Rigidbody.velocityY = velocity ;
		}

		//-----------------------------------

		/// <summary>
		/// コライダーのトリガー
		/// </summary>
		public bool IsTrigger
		{
			get
			{
				if( CCollider == null )
				{
					return false ;
				}
				return m_Collider.isTrigger ;
			}
			set
			{
				if( CCollider == null )
				{
					return ;
				}
				m_Collider.isTrigger = value ;
			}
		}

		//---------------

		/// <summary>
		/// ボディタイプ
		/// </summary>
		public RigidbodyType2D BodyType
		{
			get
			{
				if( CRigidbody == null )
				{
					return RigidbodyType2D.Dynamic ;
				}
				return m_Rigidbody.bodyType ;
			}
			set
			{
				if( CRigidbody == null )
				{
					return ;
				}
				m_Rigidbody.bodyType = value ;
			}
		}

		/// <summary>
		/// リジッドボディのシミュレーティッド
		/// </summary>
		public bool Similated
		{
			get
			{
				if( CRigidbody == null )
				{
					return false ;
				}
				return m_Rigidbody.simulated ;
			}
			set
			{
				if( CRigidbody == null )
				{
					return ;
				}

				m_Rigidbody.simulated = value ;
			}
		}

		/// <summary>
		/// リジッドボディのグラビティスケール
		/// </summary>
		public float GravityScale
		{
			get
			{
				if( CRigidbody == null )
				{
					return 0 ;
				}
				return m_Rigidbody.gravityScale ;
			}
			set
			{
				if( CRigidbody == null )
				{
					return ;
				}
				m_Rigidbody.gravityScale = value ;
			}
		}

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector2 m_Offset ;

		[SerializeField][HideInInspector]
		private Vector2 m_Size ;

		// 強制更新フラグ
		protected bool m_ForceRefresh ;

		internal virtual void OnEnable()
		{
			m_ForceRefresh = true ;
		}

		// 毎フレーム呼び出される
		internal virtual void Update()
		{
			// コライダーの更新
			if( CST != null && m_ColliderAdjustment == true )
			{
				if( CST.Offset.Equals( m_Offset ) == false || CST.DeltaSize.Equals( m_Size ) == false || m_ForceRefresh == true )
				{
					m_IsColliderDirty = true ;

					m_Offset = CST.Offset ;
					m_Size	 = CST.DeltaSize ;

					m_ForceRefresh = false ;
				}

				if( m_IsColliderDirty == true )
				{
					AdjustCollider() ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// コライダー２Ｄキャッシュ
		[SerializeField][HideInInspector]
		protected Collider2D m_Collider ;

		[SerializeField][HideInInspector]
		protected Rigidbody2D m_Rigidbody ;

		/// <summary>
		/// Collider2D(ショートカット)
		/// </summary>
		public virtual Collider2D CCollider
		{
			get
			{
				if( m_Collider == null )
				{
					gameObject.TryGetComponent<Collider2D>( out m_Collider ) ;
				}
				return m_Collider ;
			}
		}

		/// <summary>
		/// Collider2D(ショートカット)
		/// </summary>
		public virtual Rigidbody2D CRigidbody
		{
			get
			{
				if( m_Rigidbody == null )
				{
					gameObject.TryGetComponent<Rigidbody2D>( out m_Rigidbody ) ;
				}
				return m_Rigidbody ;
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
			if( CCollider == null )
			{
				if( TryGetComponent<Collider2D>( out m_Collider ) == false )
				{
					m_Collider = gameObject.AddComponent<T>() ;
					m_Collider.enabled		= true ;
					m_Collider.isTrigger	= true ;
				}
			}

			//----------------------------------

			if( m_Rigidbody == null )
			{
				if( TryGetComponent<Rigidbody2D>( out m_Rigidbody ) == false )
				{
					m_Rigidbody = gameObject.AddComponent<Rigidbody2D>() ;
					m_Rigidbody.gravityScale = 0 ;
				}
			}

			//----------------------------------

			m_IsColliderDirty = true ;
		}

		/// <summary>
		/// Collider の削除
		/// </summary>
		public void RemoveCollider()
		{
			if( m_Rigidbody != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Rigidbody ) ;
				}
				else
				{
					Destroy( m_Rigidbody ) ;
				}

				m_Rigidbody = null ;
			}

			//----------------------------------------------------------

			if( CCollider != null )
			{
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Collider ) ;
				}
				else
				{
					Destroy( m_Collider ) ;
				}

				m_Collider = null ;
			}
		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected float m_ColliderMarginL = 0 ;

		/// <summary>
		/// コライダーのマージン(左)　※非反転時
		/// </summary>
		public float ColliderMarginL
		{
			get
			{
				return m_ColliderMarginL ;
			}
			set
			{
				if( m_ColliderMarginL != value )
				{
					m_ColliderMarginL  = value ;

					m_IsColliderDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ColliderMarginR = 0 ;

		/// <summary>
		/// コライダーのマージン(右)　※非反転時
		/// </summary>
		public float ColliderMarginR
		{
			get
			{
				return m_ColliderMarginR ;
			}
			set
			{
				if( m_ColliderMarginR != value )
				{
					m_ColliderMarginR  = value ;

					m_IsColliderDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ColliderMarginT = 0 ;

		/// <summary>
		/// コライダーのマージン(上)　※非反転時
		/// </summary>
		public float ColliderMarginT
		{
			get
			{
				return m_ColliderMarginT ;
			}
			set
			{
				if( m_ColliderMarginT != value )
				{
					m_ColliderMarginT  = value ;

					m_IsColliderDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ColliderMarginB = 0 ;

		/// <summary>
		/// コライダーのマージン(下)　※非反転時
		/// </summary>
		public float ColliderMarginB
		{
			get
			{
				return m_ColliderMarginB ;
			}
			set
			{
				if( m_ColliderMarginB != value )
				{
					m_ColliderMarginB  = value ;

					m_IsColliderDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ColliderEdgeWidth = 1 ;

		/// <summary>
		/// エッジコライダーの角の幅
		/// </summary>
		public float ColliderEdgeWidth
		{
			get
			{
				return m_ColliderEdgeWidth ;
			}
			set
			{
				if( m_ColliderEdgeWidth != value )
				{
					m_ColliderEdgeWidth  = value ;

					m_IsColliderDirty = true ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		
		// キャッシュ
		[SerializeField][HideInInspector]
		protected Animator m_Animator = null ;

		/// <summary>
		/// Animator(ショートカット)
		/// </summary>
		public virtual Animator CAnimator
		{
			get
			{
				if( m_Animator == null )
				{
					gameObject.TryGetComponent<Animator>( out m_Animator ) ;
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
					RemoveAnimator() ;
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
				
			m_Animator = gameObject.AddComponent<Animator>() ;
			m_Animator.speed = 1 ;
		}

		/// <summary>
		/// Animator の削除
		/// </summary>
		public void RemoveAnimator()
		{
			if( CAnimator == null )
			{
				return ;
			}
		
			if( Application.isPlaying == false )
			{
				DestroyImmediate( m_Animator ) ;
			}
			else
			{
				Destroy( m_Animator ) ;
			}

			m_Animator = null ;
		}

		protected bool m_IsColliderDirty = true ;

		/// <summary>
		/// コライダーの自動調整
		/// </summary>
		[ SerializeField ][ HideInInspector ]
		protected bool m_ColliderAdjustment = true ;

		/// <summary>
		/// コライダーの自動調整
		/// </summary>
		public    bool	 ColliderAdjustment
		{
			get
			{
				return m_ColliderAdjustment ;
			}
			set
			{
				if( m_ColliderAdjustment != value )
				{
					m_ColliderAdjustment	= value ;

					if( m_ColliderAdjustment == true )
					{
						m_IsColliderDirty	= true ;
					}
				}
			}
		}

		/// <summary>
		/// コライダーの位置と大きさをメッシュと同じに合わせる
		/// </summary>
		public virtual void AdjustCollider()
		{
			if( CST == null )
			{
				return ;
			}

			//----------------------------------------------------------

			var offset = CST.Offset ;
			var size   = CST.DeltaSize ;

			if( m_Collider is BoxCollider2D )
			{
				var collider2D = m_Collider as BoxCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.size		= new ( size.x - ml - mr, size.y - mb - mt ) ;
			}
			else
			if( m_Collider is CircleCollider2D )
			{
				var collider2D = m_Collider as CircleCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.radius	= Mathf.Min( size.x - ml - mr, size.y - mb - mt ) * 0.5f ;
			}
			else
			if( m_Collider is CapsuleCollider2D )
			{
				var collider2D = m_Collider as CapsuleCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				collider2D.offset	= new ( offset.x + mx, offset.y + my ) ;
				collider2D.size		= new ( size.x - ml - mr, size.y - mb - mt ) ;
			}
			else
			if( m_Collider is EdgeCollider2D )
			{
				var collider2D = m_Collider as EdgeCollider2D ;

				float mx, my ;
				float ml, mr, mt, mb ;

				ml = m_ColliderMarginL ;
				mr = m_ColliderMarginR ;

				mt = m_ColliderMarginT ;
				mb = m_ColliderMarginB ;

				mx = ( ml - mr ) * 0.5f ;
				my = ( mb - mt ) * 0.5f ;

				float ox = offset.x + mx,    oy = offset.y + my ;
				float sx = size.x - ml - mr, sy = size.y - mb - mt ; 

				float hx = sx * 0.5f ;
				float hy = sy * 0.5f ;

				float x0 = ox - hx ;
				float x1 = ox + hx ;
				float y0 = oy - hy ;
				float y1 = oy + hy ;

				float ew = m_ColliderEdgeWidth ;
				if( ew <= 0 )
				{
					ew  = 0.1f ;
				}

				collider2D.points = new Vector2[]
				{
					new ( x0 + ew, y0 ),
					new ( x0, y0 + ew ),

					new ( x0, y1 - ew ),
					new ( x0 + ew, y1 ),

					new ( x1 - ew, y1 ),
					new ( x1, y1 - ew ),

					new ( x1, y0 + ew ),
					new ( x1 - ew, y0 ),

					new ( x0 + ew, y0 ),	// 最初に戻る
				} ;
			}

			m_IsColliderDirty = false ;
		}
	}
}
