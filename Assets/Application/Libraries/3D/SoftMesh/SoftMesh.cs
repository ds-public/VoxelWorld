using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

[ RequireComponent( typeof( MeshRenderer ) ) ]
[ RequireComponent( typeof( MeshFilter ) ) ]

[ ExecuteInEditMode ]


// Last Update 2022/10/05 0

/// <summary>
/// 任意のメッシュ形状を選択出来るメッシュ生成クラス
/// </summary>
public class SoftMesh : MonoBehaviour
{
	//------------------------------------------------------------

	// private

	// メッシュレンダラー
	[ SerializeField  ][ HideInInspector ]
	private MeshRenderer	m_MeshRenderer ;

	// メッシュフィルター
	[ SerializeField ][ HideInInspector ]
	private MeshFilter		m_MeshFilter ;

	private Mesh m_Mesh = null ;

	//------------------------------------------------------------
	
	// protected & public

	/// <summary>
	/// メッシュ形状
	/// </summary>
	public enum ShapeTypes
	{
		Cube		= 1,
		Sphere		= 2,
		Capsule		= 3,
		Cylinder	= 4,
		Plane		= 5,
		Box2D		= 6,
		Circle2D	= 7,
//		Polygon2D	= 8,
	}

	/// <summary>
	/// メッシュ形状がカプセル・シリンダーの場合の方向
	/// </summary>
	public enum Directions
	{
		X_Axis = 0,
		Y_Axis = 1,
		Z_Axis = 2,
	}

	/// <summary>
	/// メッシュの形状がプレーンの場合の配置
	/// </summary>
	public enum PlaneDirections
	{
		Front  = 1,
		Back   = 2,
		Left   = 3,
		Right  = 4,
		Top    = 5,
		Bottom = 6,
	}

	//--------------------------------------------------------------------------------------------

	[ SerializeField ][ HideInInspector ]
	protected ShapeTypes m_ShapeType = ShapeTypes.Cube ;

	public    ShapeTypes  shapeType
	{
		get
		{
			return m_ShapeType ;
		}
		set
		{
			if( m_ShapeType != value )
			{
				m_ShapeType  = value ;
				Refresh() ;

				if( m_Collider == true || m_Collider2D == true )
				{
					#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveCollider() ;
						AddCollider() ;
					}
					else
					{
						m_RemoveCollider = true ;
						m_AddCollider = true ;
					}

					#else

					RemoveCollider() ;
					AddCollider() ;

					#endif
				}

				if( m_Rigidbody == true || m_Rigidbody2D == true )
				{
					#if UNITY_EDITOR

					if( Application.isPlaying == true )
					{
						RemoveRigidbody() ;
						AddRigidbody() ;
					}
					else
					{
						m_RemoveRigidbody = true ;
						m_AddRigidbody = true ;
					}

					#else

					RemoveRigidbody() ;
					AddRigidbody() ;

					#endif
				}
			}
		}
	}

	[ SerializeField ][ HideInInspector ]
	protected  Directions m_Direction = Directions.Y_Axis ;

	public     Directions   Direction
	{
		get
		{
			return m_Direction ;
		}
		set
		{
			if( m_Direction != value )
			{
				m_Direction  = value ;
				Refresh() ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}

	[ SerializeField ][ HideInInspector ]
	protected  PlaneDirections m_PlaneDirection = PlaneDirections.Front ;

	public     PlaneDirections   PlaneDirection
	{
		get
		{
			return m_PlaneDirection ;
		}
		set
		{
			if( m_PlaneDirection != value )
			{
				m_PlaneDirection  = value ;
				Refresh() ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}

	//------------------------------

	// 以下は共通

	/// <summary>
	/// オフセット
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected Vector3 m_Offset = Vector3.zero ;

	public    Vector3   Offset
	{
		get
		{
			return m_Offset ;
		}
		set
		{
			if( m_Offset.Equals( value ) == false )
			{
				m_Offset = value ;
				Refresh() ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}	

	/// <summary>
	/// サイズ
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected Vector3 m_Size = Vector3.one ;

	public    Vector3   Size
	{
		get
		{
			return m_Size ;
		}
		set
		{
			if( m_Size.Equals( value ) == false )
			{
				m_Size = value ;
				Refresh() ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}	

	/// <summary>
	/// 頂点カラー
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected Color m_VertexColor = Color.white ;

	public    Color  VertexColor
	{
		get
		{
			return m_VertexColor ;
		}
		set
		{
			if
			(
				m_VertexColor.r != value.r ||
				m_VertexColor.g != value.g ||
				m_VertexColor.b != value.b ||
				m_VertexColor.a != value.a
			)
			{
				m_VertexColor.r  = value.r ;
				m_VertexColor.g  = value.g ;
				m_VertexColor.b  = value.b ;
				m_VertexColor.a  = value.a ;
				Refresh() ;
			}
		}
	}

	/// <summary>
	/// テクスチャ座標
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected Rect[] m_UV = new Rect[]
	{ new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ) } ;

	public    Rect[]   UV
	{
		get
		{
			return m_UV ;
		}
	}

	/// <summary>
	/// 複数のＵＶをまとめて設定する
	/// </summary>
	/// <param name="tUVs"></param>
	public void SetUV( params Rect[] uvs )
	{
		if( uvs == null || uvs.Length == 0 )
		{
			return ;
		}

		int i, l = uvs.Length ;

		for( i  = 0 ; i <  l ; i ++ )
		{
			SetUV( i, uvs[ i ] ) ;
		}
	}

	/// <summary>
	/// インデックスを指定してＵＶを設定する
	/// </summary>
	/// <param name="tIndex"></param>
	/// <param name="tUV"></param>
	/// <returns></returns>
	public bool SetUV( int index, Rect uv )
	{
		if( m_UV == null || m_UV.Length == 0 )
		{
			return false ;
		}

		if( index <  0 || index >= m_UV.Length )
		{
			return false ;
		}

		if( m_UV[ index ].Equals( uv ) == false )
		{
			m_UV[ index ].x			= uv.x ;
			m_UV[ index ].y			= uv.y ;
			m_UV[ index ].width		= uv.width ;
			m_UV[ index ].height	= uv.height ;
	
			Refresh() ;
		}

		return true ;
	}

	/// <summary>
	/// 分割数
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected int m_Split = 0 ;

	public    int  Split
	{
		get
		{
			return m_Split ;
		}
		set
		{
			if( m_Split != value )
			{
				m_Split  = value ;
				Refresh() ;
			}
		}
	}

	/// <summary>
	/// テクスチャのタイリング
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected bool m_Tiling = false ;
	
	public    bool   Tiling
	{
		get
		{
			return m_Tiling ;
		}
		set
		{
			if( m_Tiling != value )
			{
				m_Tiling  = value ;
				Refresh() ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// マテリアル(ショートカット)
	/// </summary>
	public Material	Material
	{
		get
		{
			if( m_MeshRenderer == null )
			{
				return null ;
			}

			if( Application.isPlaying == false )
			{
				return m_MeshRenderer.sharedMaterial ;
			}
			else
			{
				return m_MeshRenderer.material ;
			}
		}
		set
		{
			if( m_MeshRenderer == null )
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial != value )
				{
					if( m_MeshRenderer.sharedMaterial != null )
					{
						// テクスチャの受け渡し
						Texture tTexture = m_MeshRenderer.sharedMaterial.mainTexture ;
	
						m_MeshRenderer.sharedMaterial = value ;
	
						if( m_MeshRenderer.sharedMaterial != null && m_MeshRenderer.sharedMaterial.mainTexture == null )
						{
							m_MeshRenderer.sharedMaterial.mainTexture = tTexture ;
						}
					}
					else
					{
						m_MeshRenderer.sharedMaterial = value ;
					}
				}
			}
			else
			{
				if( m_MeshRenderer.material != value )
				{
					if( m_MeshRenderer.material != null )
					{
						// テクスチャの受け渡し
						Texture tTexture = m_MeshRenderer.material.mainTexture ;
	
						m_MeshRenderer.material = value ;
	
						if( m_MeshRenderer.material != null && m_MeshRenderer.material.mainTexture == null )
						{
							m_MeshRenderer.material.mainTexture = tTexture ;
						}
					}
					else
					{
						m_MeshRenderer.sharedMaterial = value ;
					}
				}
			}
		}
	}


	[SerializeField][HideInInspector]
	private Texture	m_Texture = null ;

	/// <summary>
	/// テクスチャ(ショートカット)
	/// </summary>
	public    Texture	 Texture
	{
		get
		{
			return m_Texture ;
		}
		set
		{
			m_Texture = value ;

			if( m_MeshRenderer == null ) 
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.sharedMaterial = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.sharedMaterial != null )
				{
					if( m_MeshRenderer.sharedMaterial.mainTexture != m_Texture )
					{
						m_MeshRenderer.sharedMaterial.mainTexture  = m_Texture ;
					}
				}
			}
			else
			{
				if( m_MeshRenderer.material == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.material = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.material != null )
				{
					if( m_MeshRenderer.material.mainTexture != m_Texture )
					{
						m_MeshRenderer.material.mainTexture  = m_Texture ;
					}
				}
			}
		}
	}

	//--------------------------------------------------------------------------------------------

	// キャッシュ
	private Collider	m_Collider = null ;
	private Collider2D	m_Collider2D = null ;

	/// <summary>
	/// コライダーが存在するかの判定を行う
	/// </summary>
	public bool IsCollider
	{
		get
		{
			if( m_Collider == null && m_Collider2D == null )
			{
				return false ;
			}
			else
			{
				return true ;
			}
		}
		set
		{
			if( value == true )
			{
				AddCollider() ;
			}
			else
			{
				#if UNITY_EDITOR

				if( Application.isPlaying == true )
				{
					RemoveCollider() ;
				}
				else
				{
					m_RemoveCollider = true ;
				}

				#else

				RemoveCollider() ;

				#endif
			}
		}
	}

	/// <summary>
	/// Collider の追加
	/// </summary>
	public void AddCollider()
	{
		if( shapeType == ShapeTypes.Cube )
		{
			if( m_Collider != null )
			{
				return ;
			}
			
			m_Collider = gameObject.AddComponent<BoxCollider>() ;
		}
		else
		if( shapeType == ShapeTypes.Sphere )
		{
			if( m_Collider != null )
			{
				return ;
			}
			
			m_Collider = gameObject.AddComponent<SphereCollider>() ;
		}
		else
		if( shapeType == ShapeTypes.Capsule )
		{
			if( m_Collider != null )
			{
				return ;
			}
			
			m_Collider = gameObject.AddComponent<CapsuleCollider>() ;
		}
		else
		if( shapeType == ShapeTypes.Cylinder )
		{
			if( m_Collider != null )
			{
				return ;
			}
			
			m_Collider = gameObject.AddComponent<CapsuleCollider>() ;
		}
		else
		if( shapeType == ShapeTypes.Plane )
		{
			if( m_Collider != null )
			{
				return ;
			}
			
			m_Collider = gameObject.AddComponent<BoxCollider>() ;
		}
		else
		if( shapeType == ShapeTypes.Box2D )
		{
			if( m_Collider2D != null )
			{
				return ;
			}
			
			m_Collider2D = gameObject.AddComponent<BoxCollider2D>() ;
		}
		else
		if( shapeType == ShapeTypes.Circle2D )
		{
			if( m_Collider2D != null )
			{
				return ;
			}
			
			m_Collider2D = gameObject.AddComponent<CircleCollider2D>() ;
		}
//		else
//		if( mShapeType == ShapeType.Polygon2D )
//		{
//			if( _collider2D != null )
//			{
//				return ;
//			}
//			
//			mCollider2D = gameObject.AddComponent<PolygonCollider2D>() ;
//		}

		AdjustCollider() ;
	}

	#if UNITY_EDITOR
	private bool m_RemoveCollider	= false ;
	private bool m_AddCollider		= false ;
	#endif

	/// <summary>
	/// Collider の削除
	/// </summary>
	public void RemoveCollider()
	{
		if( m_Collider != null )
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

		if( m_Collider2D != null )
		{
			if( Application.isPlaying == false )
			{
				DestroyImmediate( m_Collider2D ) ;
			}
			else
			{
				Destroy( m_Collider2D ) ;
			}
	
			m_Collider2D = null ;
		}
	}

	/// <summary>
	/// コライダーの自動調整
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected bool m_ColliderAdjustment = true ;

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
				m_ColliderAdjustment  = value ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}

	/// <summary>
	/// コライダーの位置と大きさをメッシュと同じに合わせる
	/// </summary>
	public void AdjustCollider()
	{
		if( shapeType == ShapeTypes.Cube )
		{
			if( m_Collider == null )
			{
				return ;
			}

			BoxCollider collider = m_Collider as BoxCollider ;
			collider.center	= Offset ;
			collider.size	= Size ; 
		}
		else
		if( shapeType == ShapeTypes.Sphere )
		{
			if( m_Collider == null )
			{
				return ;
			}

			SphereCollider collider = m_Collider as SphereCollider ;
			collider.center	= Offset ;

			float r = 0 ;
			if( Size.x >  r )
			{
				r = Size.x ;
			}
			if( Size.y >  r )
			{
				r = Size.y ;
			}
			if( Size.z >  r )
			{
				r = Size.z ;
			}

			collider.radius = r * 0.5f ;
		}
		else
		if( shapeType == ShapeTypes.Capsule )
		{
			if( m_Collider == null )
			{
				return ;
			}

			CapsuleCollider collider = m_Collider as CapsuleCollider  ;
			collider.center	= Offset ;

			float r = 0 ;
			if( Size.x >  r )
			{
				r = Size.x ;
			}
			if( Size.z >  r )
			{
				r = Size.z ;
			}

			collider.radius = r * 0.5f ;
			collider.height = Size.y ;
			collider.direction = ( int )Direction ;
		}
		else
		if( shapeType == ShapeTypes.Cylinder )
		{
			if( m_Collider == null )
			{
				return ;
			}

			CapsuleCollider collider = m_Collider as CapsuleCollider ;
			collider.center	= Offset ;

			float r = 0 ;
			if( Size.x >  r )
			{
				r = Size.x ;
			}
			if( Size.z >  r )
			{
				r = Size.z ;
			}

			collider.radius = r * 0.5f ;
			collider.height = Size.y ;
			collider.direction = ( int )Direction ;
		}
		else
		if( shapeType == ShapeTypes.Plane )
		{
			if( m_Collider == null )
			{
				return ;
			}

			BoxCollider collider = m_Collider as BoxCollider ;
			collider.center	= Offset ;

			Vector3 size = Size ;
			if( PlaneDirection == PlaneDirections.Front || PlaneDirection == PlaneDirections.Back   )
			{
				size.z = 0 ;
			}
			else
			if( PlaneDirection == PlaneDirections.Left  || PlaneDirection == PlaneDirections.Right  )
			{
				size.x = 0 ;
			}
			else
			if( PlaneDirection == PlaneDirections.Top   || PlaneDirection == PlaneDirections.Bottom )
			{
				size.y = 0 ;
			}

			collider.size = size ;
		}
		else
		if( shapeType == ShapeTypes.Box2D )
		{
			if( m_Collider2D == null )
			{
				return ;
			}

			BoxCollider2D collider2D = m_Collider2D as BoxCollider2D ;
			collider2D.offset = new Vector2( Offset.x, Offset.y ) ;

			collider2D.size = new Vector2( Size.x, Size.y ) ;
		}
		else
		if( shapeType == ShapeTypes.Circle2D )
		{
			if( m_Collider2D == null )
			{
				return ;
			}

			CircleCollider2D collider2D = m_Collider2D as CircleCollider2D ;
			collider2D.offset = new Vector2( Offset.x, Offset.y ) ;

			float r = 0 ;
			if( Size.x >  r )
			{
				r = Size.x ;
			}
			if( Size.y >  r )
			{
				r = Size.y ;
			}

			collider2D.radius = r * 0.5f ;
		}
//		else
//		if( mShapeType == ShapeType.Polygon2D )
//		{
//			if( _collider2D == null )
//			{
//				return ;
//			}
//
//			PolygonCollider2D tCollider2D = _collider2D as PolygonCollider2D ;
//			tCollider2D.offset = new Vector2( mOffset.x, mOffset.y ) ;
//		}
	}

	//---------------------------------------------------------------

	// キャッシュ
	private Rigidbody	m_Rigidbody		= null ;
	private Rigidbody2D	m_Rigidbody2D	= null ;

	/// <summary>
	/// Rigidbody が存在するかの判定を行う
	/// </summary>
	public bool IsRigidbody
	{
		get
		{
			if( m_Rigidbody == null && m_Rigidbody2D == null )
			{
				return false ;
			}
			else
			{
				return true ;
			}
		}
		set
		{
			if( value == true )
			{
				AddRigidbody() ;
			}
			else
			{
				#if UNITY_EDITOR

				if( Application.isPlaying == true )
				{
					RemoveRigidbody() ;
				}
				else
				{
					m_RemoveRigidbody = true ;
				}

				#else

				RemoveRigidbody() ;

				#endif
			}
		}
	}

	/// <summary>
	/// Rigidbody の追加
	/// </summary>
	public void AddRigidbody()
	{
		if( shapeType == ShapeTypes.Cube || shapeType == ShapeTypes.Sphere || shapeType == ShapeTypes.Capsule || shapeType == ShapeTypes.Cylinder || shapeType == ShapeTypes.Plane )
		{
			if( m_Rigidbody != null )
			{
				return ;
			}
			
			m_Rigidbody = gameObject.AddComponent<Rigidbody>() ;
		}
		else
		if( shapeType == ShapeTypes.Box2D || shapeType == ShapeTypes.Circle2D )
		{
			if( m_Rigidbody2D != null )
			{
				return ;
			}
			
			m_Rigidbody2D = gameObject.AddComponent<Rigidbody2D>() ;
		}
	}

	#if UNITY_EDITOR
	private bool m_RemoveRigidbody	= false ;
	private bool m_AddRigidbody		= false ;
	#endif

	/// <summary>
	/// Rigidbody の削除
	/// </summary>
	public void RemoveRigidbody()
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

		if( m_Rigidbody2D != null )
		{
			if( Application.isPlaying == false )
			{
				DestroyImmediate( m_Rigidbody2D ) ;
			}
			else
			{
				Destroy( m_Rigidbody2D ) ;
			}
	
			m_Rigidbody2D = null ;
		}
	}

	//---------------------------------------------------------------

	void Awake()
	{
		m_MeshRenderer	= GetComponent<MeshRenderer>() ;

		UpdateTexture() ;

		m_MeshFilter	= GetComponent<MeshFilter>() ;
		if( m_MeshFilter != null )
		{
			m_Mesh = new Mesh() ;
			if( Application.isPlaying == false )
			{
				m_MeshFilter.sharedMesh = m_Mesh ;
			}
			else
			{
				m_MeshFilter.mesh = m_Mesh ;
			}
		}

		Refresh() ;
	}

	void Start()
	{
		UpdateTexture() ;
	}

	private void UpdateTexture()
	{
		if( m_MeshRenderer != null )
		{
			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.sharedMaterial = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.sharedMaterial != null && m_Texture != null )
				{
					m_MeshRenderer.sharedMaterial.mainTexture = m_Texture ;
				}
			}
			else
			{
				if( m_MeshRenderer.material == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.material = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.material != null && m_Texture != null )
				{
					m_MeshRenderer.material.mainTexture = m_Texture ;
				}
			}
		}
	}

	void Update()
	{
		if( m_UV == null || m_UV.Length == 0 )
		{
			m_UV = new Rect[]
			{ new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ) } ;
		}

		//-----------------------------------

		#if UNITY_EDITOR

		// 生成と削除の順番が重要
		// 必ず
		// Ｒ削除→Ｃ削除→Ｃ生成→Ｒ生成
		// とすること
		
		if( m_RemoveRigidbody == true )
		{
			RemoveRigidbody() ;
			m_RemoveRigidbody  = false ;
		}

		if( m_RemoveCollider == true )
		{
			RemoveCollider() ;
			m_RemoveCollider  = false ;
		}

		if( m_AddCollider == true )
		{
			AddCollider() ;
			m_AddCollider  = false ;
		}

		if( m_AddRigidbody == true )
		{
			AddRigidbody() ;
			m_AddRigidbody  = false ;
		}

		#endif
	}

	void OnDestroy()
	{
		if( m_MeshFilter != null )
		{
			if( Application.isPlaying == false )
			{
				m_MeshFilter.sharedMesh = null ;
			}
			else
			{
				m_MeshFilter.mesh = null ;
			}
			m_MeshFilter = null ;
		}

		if( m_Mesh != null )
		{
			DestroyImmediate( m_Mesh ) ;
			m_Mesh = null ;
		}

		if( m_MeshRenderer != null )
		{
			if( Application.isPlaying == false )
			{
				m_MeshRenderer.sharedMaterial = null ;
			}
			else
			{
				m_MeshRenderer.material = null ;
			}
		}
	}

	//-------------------------------------------------------------------------

	/// <summary>
	/// メッシュをクリアする
	/// </summary>
	public void Clear()
	{
		if( m_MeshFilter == null )
		{
			return ;
		}

		if( Application.isPlaying == false )
		{
			if( m_MeshFilter.sharedMesh == null )
			{
				return ;
			}

			m_MeshFilter.sharedMesh.Clear() ;
		}
		else
		{
			if( m_MeshFilter.mesh == null )
			{
				return ;
			}

			m_MeshFilter.mesh.Clear() ;
		}
	}

	/// <summary>
	/// 更新する
	/// </summary>
	public void Refresh()
	{
		switch( shapeType )
		{
			case ShapeTypes.Cube		: CreateCube()		; break ;
			case ShapeTypes.Sphere		: CreateSphere()	; break ;
			case ShapeTypes.Capsule		: CreateCapsule()	; break ;
			case ShapeTypes.Cylinder	: CreateCylinder()	; break ;
			case ShapeTypes.Plane		: CreatePlane()		; break ;
			case ShapeTypes.Box2D		: CreateBox2D()		; break ;
			case ShapeTypes.Circle2D	: CreateCircle2D()	; break ;
		}
	}


	/// <summary>
	/// メッシュを更新する
	/// </summary>
	/// <param name="aVD"></param>
	/// <param name="aCD"></param>
	/// <param name="aTD"></param>
	/// <param name="aID"></param>
	public void Build( string modelName, Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI )
	{
		if( m_MeshFilter == null )
		{
			return ;
		}

		if( m_MeshFilter.sharedMesh == null )
		{
			return ;
		}

		m_MeshFilter.sharedMesh.name = modelName ;

		m_MeshFilter.sharedMesh.Clear() ;	// 新たに設定する場合は必ずクリアが必要（クリアしないと設定中の要素数の不整合がおきてエラーになる）
		m_MeshFilter.sharedMesh.vertices  = aV ;
		if( aN != null )
		{
			m_MeshFilter.sharedMesh.normals   = aN ;
		}
		m_MeshFilter.sharedMesh.colors    = aC ;
		m_MeshFilter.sharedMesh.uv        = aT ;
		m_MeshFilter.sharedMesh.triangles = aI ;
		
		if( aN == null )
		{
			m_MeshFilter.sharedMesh.RecalculateNormals() ;
		}
		m_MeshFilter.sharedMesh.RecalculateBounds() ;
	}

	//--------------------------------------------------------------------------------------------

	private void CreateCube()
	{
		CreateCube( m_Offset.x, m_Offset.y, m_Offset.z, m_Size.x, m_Size.y, m_Size.z, m_VertexColor, m_UV, m_Split ) ;
	}

	/// <summary>
	/// キューブを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="uv"></param>
	public void CreateCube( float px, float py, float pz, float sx, float sy, float sz, Color c, Rect[] uv = null, int tSplit = 0 )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Offset		= new Vector3( px, py, pz ) ;
		m_Size			= new Vector3( sx, sy, sz ) ;
		m_UV			= uv ;
		m_VertexColor	= c ;
		m_Split			= tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float xMin = - sx * 0.5f, xMax = sx * 0.5f, yMin = - sy * 0.5f, yMax = sy * 0.5f, zMin = - sz * 0.5f, zMax = sz * 0.5f ;
		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;

		Vector3[,] v = new Vector3[ 6, 4 ]
		{
			{ new Vector3( xMin, yMin, zMin ), new Vector3( xMin, yMax, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMin, zMin ) },
            { new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMax, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMax, zMin ) },
			{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMin, zMin ) },
			{ new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMin, zMax ) },
			{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMin, zMin ), new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMin, zMax ) },
			{ new Vector3( xMax, yMin, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMin, zMax ) },
		} ;

		Vector3[] n = new Vector3[]
		{
			new Vector3(  0,  0, -1 ),
			new Vector3(  0,  1,  0 ),
			new Vector3( -1,  0,  0 ),
			new Vector3(  1,  0,  0 ),
			new Vector3(  0, -1,  0 ),
			new Vector3(  0,  0,  1 ),
		} ;

		int l = v.GetLength( 0 ) ;

		int ly, lx ;

		int s, s1 = 1 << tSplit, s2 = 2 << tSplit ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}


		int f ;
		for( f  = 0 ; f <  l ; f ++ )
		{
			float vx0 = v[ f, 0 ].x ;
			float vy0 = v[ f, 0 ].y ;
			float vz0 = v[ f, 0 ].z ;

			float vx1 = v[ f, 1 ].x ;
			float vy1 = v[ f, 1 ].y ;
			float vz1 = v[ f, 1 ].z ;

			float vx2 = v[ f, 2 ].x ;
			float vy2 = v[ f, 2 ].y ;
			float vz2 = v[ f, 2 ].z ;

			float vx3 = v[ f, 3 ].x ;
			float vy3 = v[ f, 3 ].y ;
			float vz3 = v[ f, 3 ].z ;


			float tx0, ty0 ;
			float tx1, ty1 ;
			float tx2, ty2 ;
			float tx3, ty3 ;

			if( uv != null && uv.Length >= 6 )
			{
				tx0 = uv[ f ].x ;
				ty0 = uv[ f ].y ;
				tx1 = uv[ f ].x ;
				ty1 = uv[ f ].y + uv[ f ].height ;
				tx2 = uv[ f ].x + uv[ f ].width ;
				ty2 = uv[ f ].y + uv[ f ].height ;
				tx3 = uv[ f ].x + uv[ f ].width ;
				ty3 = uv[ f ].y ;
			}
			else
			{
				tx0 = 0 ;
				ty0 = 0 ;
				tx1 = 0 ;
				ty1 = 1 ;
				tx2 = 1 ;
				ty2 = 1 ;
				tx3 = 1 ;
				ty3 = 0 ;
			}

			for( lx  = 0 ; lx <  s2 ; lx ++ )
			{
				// V
				float vlx0_dx = vx3 - vx0 ;
				float vlx0_dy = vy3 - vy0 ;
				float vlx0_dz = vz3 - vz0 ;

				float vlx1_dx = vx2 - vx1 ;
				float vlx1_dy = vy2 - vy1 ;
				float vlx1_dz = vz2 - vz1 ;

				float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
				float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
				float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

				float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
				float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
				float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

				float vly_dx = vlx1_x - vlx0_x ;
				float vly_dy = vlx1_y - vlx0_y ;
				float vly_dz = vlx1_z - vlx0_z ;

				for( ly  = 0 ; ly <  s2 ; ly ++  )
				{
					float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
					float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
					float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

					tV[ lx ][ ly ] = new Vector3( vx, vy, vz ) ;
				}

				// T
				float tlx0_dx = tx3 - tx0 ;
				float tlx0_dy = ty3 - ty0 ;

				float tlx1_dx = tx2 - tx1 ;
				float tlx1_dy = ty2 - ty1 ;

				float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
				float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

				float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
				float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

				float tly_dx = tlx1_x - tlx0_x ;
				float tly_dy = tlx1_y - tlx0_y ;

				for( ly  = 0 ; ly <  s2 ; ly ++  )
				{
					float tx = tlx0_x + ( tly_dx * ly / s1 ) ;
					float ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
					tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
				}
			}

			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		Build( "Cube", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateSphere()
	{
		CreateSphere( m_Offset.x, m_Offset.y, m_Offset.z, m_Size.x, m_Size.y, m_Size.z, m_VertexColor, m_Split ) ;
	}
	
	/// <summary>
	/// スフィアを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateSphere( float px, float py, float pz, float sx, float sy, float sz, Color c, int split = 0 )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Offset		= new Vector3( px, py, pz ) ;
		m_Size			= new Vector3( sx, sy, sz ) ;
		m_VertexColor	= c ;
		m_Split			= split ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int l ;
		int s1, s2, s4 ;
		int ly ;
		float a, r ;
		
		s1 = 1 << split ;
		s2 = 2 << split ;
		s4 = 4 << split ;


		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;

		float vx, vy, vz ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s2 ) ;
		int yp = s2 ;


		// 一番上の頂点
		vx = 0 ;
		vy =   1.0f ;
		vz = 0 ;
		ty = tdh * yp ; yp -- ;
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * i + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o + i          ) ;
			aI.Add( o + i + 1 + s4 ) ;
			aI.Add( o + i     + s4 ) ;
		}
		o = o + s4 ;

		// 赤道から上の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  = Mathf.Sin( a ) ;
			vy = Mathf.Cos( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				vx = - Mathf.Cos( a ) * r ;
//				vz = - Mathf.Sin( a ) * r ;
				vx = - Mathf.Sin( a ) * r ;
				vz =   Mathf.Cos( a ) * r ;

				aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
				aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道の頂点
		vy = 0 ;
		ty = tdh * yp ; yp -- ;
		for( i  = 0 ; i <= s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
//			vx = - Mathf.Cos( a ) ;
//			vz = - Mathf.Sin( a ) ;
			vx = - Mathf.Sin( a ) ;
			vz =   Mathf.Cos( a ) ;

			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cb, cg, ca ) ) ;

			tx = tdw * i ;
			aT.Add( new Vector2( tx, ty ) ) ;

			if( i <  s4 )
			{
				if( split == 0 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
				else
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
		}
		o = o + s4 + 1 ;

		// 赤道から下の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  =   Mathf.Cos( a ) ;
			vy = - Mathf.Sin( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				vx = - Mathf.Cos( a ) * r ;
//				vz = - Mathf.Sin( a ) * r ;
				vx = - Mathf.Sin( a ) * r ;
				vz =   Mathf.Cos( a ) * r ;

				aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
				aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  ( s1 - 1 ) )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}
		
		// 一番下の頂点
		vx = 0 ;
		vy = - 1.0f ;
		vz = 0 ;
		ty = tdh * yp ; yp -- ;
		for( l  = 0 ; l <  s4 ; l ++ )
		{
			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * l + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;
		}
		
		//-----------------------------------------------------------

		Build( "Sphere", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateCapsule()
	{
		CreateCapsule( m_Direction, m_Offset.x, m_Offset.y, m_Offset.z, m_Size.x, m_Size.y, m_Size.z, m_VertexColor, m_Split ) ;
	}

	/// <summary>
	/// カプセルを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCapsule( Directions direction, float px, float py, float pz, float sx, float sy, float sz, Color c, int split = 0 )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Direction		= direction ;
		m_Offset		= new Vector3( px, py, pz ) ;
		m_Size			= new Vector3( sx, sy, sz ) ;
		m_VertexColor	= c ;
		m_Split			= split ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int l ;
		int s1, s2, s4 ;
		int ly ;
		float a, r ;

		float vx, vy, vz, vs ;
		float nx, ny, nz, ns ;

		s1 = 1 << split ;
		s2 = 2 << split ;
		s4 = 4 << split ;


		float hsx = sx * 0.50f ;
		float hsy = sy * 0.50f ;
		float hsz = sz * 0.50f ;

		float qsy = sy * 0.25f ;

		float x, y, z ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s4 ) ;
		int yp = s4 ;


		// 一番上の頂点
		x = 0 ;
		y =   1.0f ;
		z = 0 ;
		ty = tdh * yp ; yp -- ;

		for( i  = 0 ; i <  s4 ; i ++ )
		{
			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx = x ;
			ny = y ;
			nz = z ;

			if( direction == Directions.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( direction == Directions.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * i + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o + i          ) ;
			aI.Add( o + i + 1 + s4 ) ;
			aI.Add( o + i     + s4 ) ;
		}
		o = o + s4 ;

		// 赤道から上の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  = Mathf.Sin( a ) ;
			y = Mathf.Cos( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) * r ;
//				z = - Mathf.Sin( a ) * r ;
				x = - Mathf.Sin( a ) * r ;
				z =   Mathf.Cos( a ) * r ;

				vx = hsx * x ;
				vy = qsy * y + qsy ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( direction == Directions.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( direction == Directions.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道の頂点
		for( ly  = 0 ; ly <= s2 ; ly ++ )
		{
			y = 0.25f - ( 0.5f * ly / s2 ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) ;
//				z = - Mathf.Sin( a ) ;
				x = - Mathf.Sin( a ) ;
				z =   Mathf.Cos( a ) ;

				vx = hsx * x ;
				vy = sy  * y ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( direction == Directions.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( direction == Directions.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cb, cg, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  s2 )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						if( split == 0 )
						{
							aI.Add( o + i              ) ;
							aI.Add( o + i + 1          ) ;
							aI.Add( o + i     + s4 + 1 ) ;
						}
						else
						{
							aI.Add( o + i              ) ;
							aI.Add( o + i + 1          ) ;
							aI.Add( o + i     + s4 + 1 ) ;

							aI.Add( o + i + 1          ) ;
							aI.Add( o + i + 1 + s4 + 1 ) ;
							aI.Add( o + i     + s4 + 1 ) ;
						}
					}
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道から下の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  =   Mathf.Cos( a ) ;
			y  = - Mathf.Sin( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) * r ;
//				z = - Mathf.Sin( a ) * r ;
				x = - Mathf.Sin( a ) * r ;
				z =   Mathf.Cos( a ) * r ;

				vx = hsx * x ;
				vy = qsy * y - qsy ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( direction == Directions.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( direction == Directions.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  ( s1 - 1 ) )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}
		
		// 一番下の頂点
		x = 0 ;
		y = - 1.0f ;
		z = 0 ;
		ty = tdh * yp ; yp -- ;
		for( l  = 0 ; l <  s4 ; l ++ )
		{
			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx = x ;
			ny = y ;
			nz = z ;

			if( direction == Directions.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( direction == Directions.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = tdw * l + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;
		}
		
		//-----------------------------------------------------------

		Build( "Sphere", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//-------------------------------------------------------------------------

	private void CreateCylinder()
	{
		CreateCylinder( m_Direction, m_Offset.x, m_Offset.y, m_Offset.z, m_Size.x, m_Size.y, m_Size.z, m_VertexColor, m_Split ) ;
	}

	/// <summary>
	/// シリンダーを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCylinder( Directions direction, float px, float py, float pz, float sx, float sy, float sz, Color c, int split = 0 )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Direction		= direction ;
		m_Offset		= new Vector3( px, py, pz ) ;
		m_Size			= new Vector3( sx, sy, sz ) ;
		m_VertexColor	= c ;
		m_Split			= split ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int s1, s4 ;
		int ly ;
		float a ;
		
		float vx, vy, vz, vs ;
		float nx, ny, nz, ns ;
		
		s1 = 1 << split ;
		s4 = 4 << split ;


		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;

		float x, y, z ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s1 ) ;

		// 一番上の頂点
		x =  0 ;
		y =  1 ;
		z =  0 ;

		// 上の中心

		vx = hsx * x ;
		vy = hsy * y ;
		vz = hsz * z ;

		nx =  0 ;
		ny =  1 ;
		nz =  0 ;

		if( direction == Directions.X_Axis )
		{
			// ＸとＹを入れ替え
			vs = - vx ;
			vx =   vy ;
			vy =   vs ;

			ns = - nx ;
			nx =   ny ;
			ny =   ns ;
		}
		else
		if( direction == Directions.Z_Axis )
		{
			// ＺとＹを入れ替え
			vs = - vz ;
			vz =   vy ;
			vy =   vs ;

			ns = - nz ;
			nz =   ny ;
			ny =   ns ;
		}

		aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
		aN.Add( new Vector3( nx, ny, nz ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 上の外周
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
			x = - Mathf.Sin( a ) ;
			z =   Mathf.Cos( a ) ;

			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx =  0 ;
			ny =  1 ;
			nz =  0 ;

			if( direction == Directions.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( direction == Directions.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = (   x * 0.5f ) + 0.5f ;
			ty = ( - z * 0.5f ) + 0.5f ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o                      ) ;
			aI.Add( o + 1 + ( i + 1 ) % s4 ) ;
			aI.Add( o + 1 +   i            ) ;
		}
		o = o + 1 + s4 ;

		// 中
		for( ly  = 0 ; ly <= s1 ; ly ++ )
		{
			y = 1.0f - ( 2.0f / ( float )s1 ) * ly ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
				x = - Mathf.Sin( a ) ;
				z =   Mathf.Cos( a ) ;

				vx = hsx * x ;
				vy = hsy * y ;
				vz = hsz * z ;

				nx =  x ;
				ny =  0 ;
				nz =  z ;

				if( direction == Directions.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( direction == Directions.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;
	
				tx = tdw * i ;
				ty = 1.0f - ( tdh * ly ) ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  s1 )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}

		// 一番上の頂点
		x =  0 ;
		y = -1 ;
		z =  0 ;
		
		// 下の中心
		vx = hsx * x ;
		vy = hsy * y ;
		vz = hsz * z ;

		nx =  0 ;
		ny = -1 ;
		nz =  0 ;

		if( direction == Directions.X_Axis )
		{
			// ＸとＹを入れ替え
			vs = - vx ;
			vx =   vy ;
			vy =   vs ;

			ns = - nx ;
			nx =   ny ;
			ny =   ns ;
		}
		else
		if( direction == Directions.Z_Axis )
		{
			// ＺとＹを入れ替え
			vs = - vz ;
			vz =   vy ;
			vy =   vs ;

			ns = - nz ;
			nz =   ny ;
			ny =   ns ;
		}

		aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
		aN.Add( new Vector3( nx, ny, nz ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 上の外周
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
			x = - Mathf.Sin( a ) ;
			z =   Mathf.Cos( a ) ;

			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx =  0 ;
			ny = -1 ;
			nz =  0 ;

			if( direction == Directions.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( direction == Directions.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = (   x * 0.5f ) + 0.5f ;
			ty = ( - z * 0.5f ) + 0.5f ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o                      ) ;
			aI.Add( o + 1 +   i            ) ;
			aI.Add( o + 1 + ( i + 1 ) % s4 ) ;
		}
		o = o + 1 + s4 ;

		//-----------------------------------------------------------

		Build( "Cylinder", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//-------------------------------------------------------------------------

	private void CreatePlane()
	{
		Rect tUV = new Rect( 0, 0, 1, 1 ) ;

		if( m_UV != null && m_UV.Length >  0 )
		{
			tUV = m_UV[ 0 ] ;
		}

		CreatePlane( m_PlaneDirection, m_Offset.x, m_Offset.y, m_Offset.z, m_Size.x, m_Size.y, m_Size.z, m_VertexColor, tUV, m_Split, m_Tiling ) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreatePlane( PlaneDirections planeDirection,  float px, float py, float pz, float sx, float sy, float sz, Color c, Rect uv, int split = 0, bool tiling = false )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_PlaneDirection	= planeDirection ;
		m_Offset			= new Vector3( px, py, pz ) ;
		m_Size				= new Vector3( sx, sy, sz ) ;
		if( m_UV != null && m_UV.Length >  0 )
		{
			m_UV[ 0 ]		= uv ;
		}
		m_VertexColor		= c ;
		m_Split				= split ;
		m_Tiling			= tiling ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;
		
		// ４つの頂点
		float vx0, vy0, vz0 ;
		float vx1, vy1, vz1 ;
		float vx2, vy2, vz2 ;
		float vx3, vy3, vz3 ;

		float nx, ny, nz ;

		float tx0, ty0 ;
		float tx1, ty1 ;
		float tx2, ty2 ;
		float tx3, ty3 ;

		float tx, ty ;

		int lx, ly ;

		int s, s1 = 1 << split, s2 = 2 << split ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}

		//-----------------------------------------------------------

		if( planeDirection == PlaneDirections.Front )
		{
			// 前
			vx0 = -1 ; vy0 = -1 ; vz0 =  0 ;
			vx1 = -1 ; vy1 =  1 ; vz1 =  0 ;
			vx2 =  1 ; vy2 =  1 ; vz2 =  0 ;
			vx3 =  1 ; vy3 = -1 ; vz3 =  0 ;

			nx =  0 ;
			ny =  0 ;
			nz = -1 ;
		}
		else
		if( planeDirection == PlaneDirections.Back )
		{
			// 後
			vx0 =  1 ; vy0 = -1 ; vz0 =  0 ;
			vx1 =  1 ; vy1 =  1 ; vz1 =  0 ;
			vx2 = -1 ; vy2 =  1 ; vz2 =  0 ;
			vx3 = -1 ; vy3 = -1 ; vz3 =  0 ;

			nx =  0 ;
			ny =  0 ;
			nz =  1 ;
		}
		else
		if( planeDirection == PlaneDirections.Left )
		{
			// 左
			vx0 =  0 ; vy0 = -1 ; vz0 = -1 ;
			vx1 =  0 ; vy1 =  1 ; vz1 = -1 ;
			vx2 =  0 ; vy2 =  1 ; vz2 =  1 ;
			vx3 =  0 ; vy3 = -1 ; vz3 =  1 ;

			nx =  1 ;
			ny =  0 ;
			nz =  0 ;
		}
		else
		if( planeDirection == PlaneDirections.Right )
		{
			// 右
			vx0 =  0 ; vy0 = -1 ; vz0 =  1 ;
			vx1 =  0 ; vy1 =  1 ; vz1 =  1 ;
			vx2 =  0 ; vy2 =  1 ; vz2 = -1 ;
			vx3 =  0 ; vy3 = -1 ; vz3 = -1 ;

			nx = -1 ;
			ny =  0 ;
			nz =  0 ;
		}
		else
		if( planeDirection == PlaneDirections.Top )
		{
			// 上
			vx0 = -1 ; vy0 =  0 ; vz0 =  1 ;
			vx1 = -1 ; vy1 =  0 ; vz1 = -1 ;
			vx2 =  1 ; vy2 =  0 ; vz2 = -1 ;
			vx3 =  1 ; vy3 =  0 ; vz3 =  1 ;

			nx =  0 ;
			ny = -1 ;
			nz =  0 ;
		}
		else
		if( planeDirection == PlaneDirections.Bottom )
		{
			// 下
			vx0 = -1 ; vy0 =  0 ; vz0 = -1 ;
			vx1 = -1 ; vy1 =  0 ; vz1 =  1 ;
			vx2 =  1 ; vy2 =  0 ; vz2 =  1 ;
			vx3 =  1 ; vy3 =  0 ; vz3 = -1 ;

			nx =  0 ;
			ny =  1 ;
			nz =  0 ;
		}
		else
		{
			return ;
		}

//		if( tUV != null )
//		{
			tx0 = uv.x ;
			ty0 = uv.y ;
			tx1 = uv.x ;
			ty1 = uv.y + uv.height ;
			tx2 = uv.x + uv.width ;
			ty2 = uv.y + uv.height ;
			tx3 = uv.x + uv.width ;
			ty3 = uv.y ;
//		}
//		else
//		{
//			tx0 = 0 ;
//			ty0 = 0 ;
//			tx1 = 0 ;
//			ty1 = 1 ;
//			tx2 = 1 ;
//			ty2 = 1 ;
//			tx3 = 1 ;
//			ty3 = 0 ;
//		}
		
		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;
		
		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// V
			float vlx0_dx = vx3 - vx0 ;
			float vlx0_dy = vy3 - vy0 ;
			float vlx0_dz = vz3 - vz0 ;

			float vlx1_dx = vx2 - vx1 ;
			float vlx1_dy = vy2 - vy1 ;
			float vlx1_dz = vz2 - vz1 ;

			float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
			float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
			float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

			float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
			float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
			float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

			float vly_dx = vlx1_x - vlx0_x ;
			float vly_dy = vlx1_y - vlx0_y ;
			float vly_dz = vlx1_z - vlx0_z ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
				float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
				float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

				tV[ lx ][ ly ] = new Vector3( vx * hsx, vy * hsy, vz * hsz ) ;
			}
		}

		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// T
			float tlx0_dx = tx3 - tx0 ;
			float tlx0_dy = ty3 - ty0 ;

			float tlx1_dx = tx2 - tx1 ;
			float tlx1_dy = ty2 - ty1 ;

			float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
			float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

			float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
			float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

			float tly_dx = tlx1_x - tlx0_x ;
			float tly_dy = tlx1_y - tlx0_y ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				tx = tlx0_x + ( tly_dx * ly / s1 ) ;
				ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
				tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
			}
		}

		if( tiling == false )
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}
		else
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx0 ;
					y = tx0 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx1 ;
					y = ty1 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx2 ;
					y = ty2 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx3 ;
					y = ty3 ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		//-----------------------------------------------------------

		Build( "Plane", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}
	
	//-------------------------------------------------------------------------

	private void CreateBox2D()
	{
		Rect tUV = new Rect( 0, 0, 1, 1 ) ;

		if( m_UV != null && m_UV.Length >  0 )
		{
			tUV = m_UV[ 0 ] ;
		}

		CreateBox2D( m_Offset.x, m_Offset.y, m_Size.x, m_Size.y, m_VertexColor, tUV, m_Split, m_Tiling ) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateBox2D( float px, float py, float sx, float sy, Color c, Rect uv, int split = 0, bool tiling = false )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Offset		= new Vector3( px, py, Offset.z ) ;
		m_Size			= new Vector3( sx, sy, Size.z ) ;
		if( m_UV != null && m_UV.Length >  0 )
		{
			m_UV[ 0 ]	= uv ;
		}
		m_VertexColor	= c ;
		m_Split			= split ;
		m_Tiling		= tiling ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float pz = 0 ;

		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;
		
		// ４つの頂点
		float vx0, vy0, vz0 ;
		float vx1, vy1, vz1 ;
		float vx2, vy2, vz2 ;
		float vx3, vy3, vz3 ;

		float nx, ny, nz ;

		float tx0, ty0 ;
		float tx1, ty1 ;
		float tx2, ty2 ;
		float tx3, ty3 ;

		float tx, ty ;

		int lx, ly ;

		int s, s1 = 1 << split, s2 = 2 << split ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}

		//-----------------------------------------------------------

		// 前
		vx0 = -1 ; vy0 = -1 ; vz0 =  0 ;
		vx1 = -1 ; vy1 =  1 ; vz1 =  0 ;
		vx2 =  1 ; vy2 =  1 ; vz2 =  0 ;
		vx3 =  1 ; vy3 = -1 ; vz3 =  0 ;

		nx =  0 ;
		ny =  0 ;
		nz = -1 ;

//		if( tUV != null )
//		{
			tx0 = uv.x ;
			ty0 = uv.y ;
			tx1 = uv.x ;
			ty1 = uv.y + uv.height ;
			tx2 = uv.x + uv.width ;
			ty2 = uv.y + uv.height ;
			tx3 = uv.x + uv.width ;
			ty3 = uv.y ;
//		}
//		else
//		{
//			tx0 = 0 ;
//			ty0 = 0 ;
//			tx1 = 0 ;
//			ty1 = 1 ;
//			tx2 = 1 ;
//			ty2 = 1 ;
//			tx3 = 1 ;
//			ty3 = 0 ;
//		}
		
		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		
		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// V
			float vlx0_dx = vx3 - vx0 ;
			float vlx0_dy = vy3 - vy0 ;
			float vlx0_dz = vz3 - vz0 ;

			float vlx1_dx = vx2 - vx1 ;
			float vlx1_dy = vy2 - vy1 ;
			float vlx1_dz = vz2 - vz1 ;

			float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
			float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
			float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

			float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
			float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
			float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

			float vly_dx = vlx1_x - vlx0_x ;
			float vly_dy = vlx1_y - vlx0_y ;
			float vly_dz = vlx1_z - vlx0_z ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
				float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
				float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

				tV[ lx ][ ly ] = new Vector3( vx * hsx, vy * hsy, vz ) ;
			}
		}

		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// T
			float tlx0_dx = tx3 - tx0 ;
			float tlx0_dy = ty3 - ty0 ;

			float tlx1_dx = tx2 - tx1 ;
			float tlx1_dy = ty2 - ty1 ;

			float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
			float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

			float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
			float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

			float tly_dx = tlx1_x - tlx0_x ;
			float tly_dy = tlx1_y - tlx0_y ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				tx = tlx0_x + ( tly_dx * ly / s1 ) ;
				ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
				tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
			}
		}

		if( tiling == false )
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}
		else
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx0 ;
					y = tx0 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx1 ;
					y = ty1 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx2 ;
					y = ty2 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx3 ;
					y = ty3 ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		//-----------------------------------------------------------

		Build( "Box2D", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}
	
	//-------------------------------------------------------------------------

	private void CreateCircle2D()
	{
		CreateCircle2D( m_Offset.x, m_Offset.y, m_Size.x, m_Size.y, m_VertexColor, m_Split ) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCircle2D( float px, float py, float sx, float sy, Color c, int split = 0 )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
		m_Offset		= new Vector3( px, py, Offset.z ) ;
		m_Size			= new Vector3( sx, sy, Size.z ) ;
		m_VertexColor	= c ;
		m_Split			= split ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0 ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;

		// 中心頂点
		aV.Add( new Vector3( px, py,  0 ) ) ;
		aN.Add( new Vector3(  0,  0, -1 ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 分割数
		int s = 4 << split ;

		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;

		float rx, ry, ra ;

		int i ;

		for( i  = 0 ; i <= s ; i ++ )
		{
			ra = 2.0f * Mathf.PI * ( float )i / ( float )s ;
			rx = Mathf.Cos( ra ) ;
			ry = Mathf.Sin( ra ) ;

			aV.Add( new Vector3( rx * hsx + px, ry * hsy + py,  0 ) ) ;
			aN.Add( new Vector3(  0,  0, -1 ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			aT.Add( new Vector2( 0.5f + rx * 0.5f, 0.5f + ry * 0.5f ) ) ;
		}

		o = 1 ;
		for( i  = 0 ; i <  s ; i ++ )
		{
			aI.Add( 0 ) ;
			aI.Add( o + 1 ) ;
			aI.Add( o ) ;

			o ++ ;
		}

		//-----------------------------------------------------------

		Build( "Circle2D", aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}
}
