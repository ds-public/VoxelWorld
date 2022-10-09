using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

namespace VoxelHelper
{
	/// <summary>
	/// ピクセルデータからボクセルモデルを生成する Version 2020/09/01
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]

	[ExecuteInEditMode]

	public class VoxelMesh : MonoBehaviour
	{
		[SerializeField][HideInInspector]
		private  MeshRenderer	m_MeshRenderer ;
		internal MeshRenderer	  MeshRenderer
		{
			get
			{
				if( m_MeshRenderer != null )
				{
					return m_MeshRenderer ;
				}
				m_MeshRenderer = GetComponent<MeshRenderer>() ;
				return m_MeshRenderer ;
			}
		}

		[SerializeField][HideInInspector]
		private  MeshFilter	m_MeshFilter ;
		internal MeshFilter	  MeshFilter
		{
			get
			{
				if( m_MeshFilter != null )
				{
					return m_MeshFilter ;
				}
				m_MeshFilter = GetComponent<MeshFilter>() ;
				return m_MeshFilter ;
			}
		}


		[SerializeField][HideInInspector]
		private Vector3 m_Offset = Vector3.zero ;
		public  Vector3   Offset
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
					UpdateMesh() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_VoxelScale = 1.0f ;
		public  float   VoxelScale
		{
			get
			{
				return m_VoxelScale ;
			}
			set
			{
				if( m_VoxelScale != value )
				{
					m_VoxelScale = value ;
					UpdateMesh() ;
				}
			}
		}	




		[SerializeField][HideInInspector]
		private Material m_Material = null ;
		public  Material   Material
		{
			get
			{
				return m_Material ;
			}
			set
			{
				if( m_Material != value )
				{
					if( m_MeshRenderer != null )
					{
						Texture texture = null ;

						if( m_MeshRenderer.sharedMaterial != null )
						{
							texture = m_MeshRenderer.sharedMaterial.mainTexture ;
						}

						m_Material = value ;
						m_MeshRenderer.sharedMaterial = m_Material ;

						if( m_MeshRenderer.sharedMaterial == null )
						{
							return ;
						}

						if( m_MeshRenderer.sharedMaterial != null && texture != null && texture == m_Texture )
						{
							m_MeshRenderer.sharedMaterial.mainTexture = texture ;
						}
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private Texture m_Texture = null ;
		public  Texture	  Texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				if( m_Texture != value )
				{
					m_Texture = value ;

					if( m_MeshRenderer == null || m_MeshRenderer.sharedMaterial == null )
					{
						return ;
					}
	
					if( m_MeshRenderer.sharedMaterial.mainTexture != m_Texture )
					{
						m_MeshRenderer.sharedMaterial.mainTexture  = m_Texture ;
					}
				}
			}
		}




		[SerializeField][HideInInspector]
		private Texture2D m_VoxelTexture = null ;
		public  Texture2D   VoxelTexture
		{
			get
			{
				return m_VoxelTexture ;
			}
			set
			{
				if( m_VoxelTexture != value )
				{
					m_VoxelTexture = value ;
					UpdateMesh() ;
				}
			}
		}


		[SerializeField][HideInInspector]
		private bool m_VoxelTextureReadable = false ;
		public  bool   VoxelTextureReadable
		{
			get
			{
				return m_VoxelTextureReadable ;
			}
			set
			{
				if( m_VoxelTextureReadable != value )
				{
					m_VoxelTextureReadable  = value ;
	//				UpdateMesh() ;
				}
			}
		}



		void Awake()
		{
			m_MeshRenderer	= GetComponent<MeshRenderer>() ;
			if( m_MeshRenderer != null )
			{
				if( m_MeshRenderer.sharedMaterial == null )
				{
					if( m_Material == null )
					{
						m_Material = new Material( Shader.Find( "Standard" ) ) ;
					}

					m_MeshRenderer.sharedMaterial = m_Material ;

					if( m_Texture != null )
					{
						m_MeshRenderer.sharedMaterial.mainTexture  = m_Texture ;
					}
				}
			}

			m_MeshFilter		= GetComponent<MeshFilter>() ;
			if( m_MeshFilter != null )
			{
				if( m_MeshFilter.sharedMesh == null )
				{
					m_MeshFilter.sharedMesh = new Mesh() ;	
				}
				else
				{
					m_MeshFilter.sharedMesh.Clear() ;
				}
			}

			UpdateMesh() ;
		}

		//-------------------------------------------------------------------------

		/// <summary>
		/// メッシュをクリアする
		/// </summary>
		public void Clear()
		{
			MeshFilter meshFilter = MeshFilter ;
			if( meshFilter == null )
			{
				return ;
			}

			if( meshFilter.sharedMesh == null )
			{
				return ;
			}

			meshFilter.sharedMesh.Clear() ;
		}

		private void UpdateMesh()
		{
			CreateVoxelMesh() ;
		}



		/// <summary>
		/// メッシュを更新する
		/// </summary>
		/// <param name="aVD"></param>
		/// <param name="aCD"></param>
		/// <param name="aTD"></param>
		/// <param name="aID"></param>
		public void Build( string tName, Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI )
		{
			MeshFilter meshFilter = MeshFilter ;
			if( meshFilter == null )
			{
				return ;
			}

			if( meshFilter.sharedMesh == null )
			{
				return ;
			}

			meshFilter.sharedMesh.name = tName ;

			meshFilter.sharedMesh.Clear() ;	// 新たに設定する場合は必ずクリアが必要（クリアしないと設定中の要素数の不整合がおきてエラーになる）
			meshFilter.sharedMesh.vertices  = aV ;
			if( aN != null )
			{
				meshFilter.sharedMesh.normals   = aN ;
			}
			meshFilter.sharedMesh.colors    = aC ;
			meshFilter.sharedMesh.uv        = aT ;
			meshFilter.sharedMesh.triangles = aI ;
		
			if( aN == null )
			{
				meshFilter.sharedMesh.RecalculateNormals() ;
			}
			meshFilter.sharedMesh.RecalculateBounds() ;
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
		public void CreateVoxelMesh()
		{
			if( m_VoxelTexture == null || m_VoxelTextureReadable == false )
			{
				Clear() ;
				return ;
			}

			//----------------------------------------------

			int w = m_VoxelTexture.width ;
			int h = m_VoxelTexture.height ;

			Color32[] cd = m_VoxelTexture.GetPixels32( 0 ) ;

			List<Vector3> aV = new List<Vector3>() ;
			List<Color>   aC = new List<Color>() ;
			List<Vector2> aT = new List<Vector2>() ;
			List<int>     aI = new List<int>() ;
		
			float sx = - ( float )w * 0.5f + 0.5f ;
			float sy =   ( float )h        + 0.5f ;

			Color32 c ;
			int x, y, i, o = 0 ;
			float x0, y0, x1, y1,z0, z1 ;
			float vr = 0.5f * m_VoxelScale ;

			for( y  = 0 ; y <  h ; y ++ )
			{
				for( x   = 0 ; x <  w ; x ++ )
				{
					i = ( h - 1 - y ) * w + x ;
					c = cd[ i ] ;
					if( c.a != 0 )
					{
						x0 = sx + x - vr + m_Offset.x ;
						x1 = sx + x + vr + m_Offset.x ;
						y0 = sy - y + vr + m_Offset.y ;
						y1 = sy - y - vr + m_Offset.y ;
						z0 =        - vr + m_Offset.z ;
						z1 =          vr + m_Offset.z ;

						// 表
						aV.Add( new Vector3( x0, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x1, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 1 ) ) ;

						aV.Add( new Vector3( x0, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aV.Add( new Vector3( x1, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;

						// 裏
						aV.Add( new Vector3( x1, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aV.Add( new Vector3( x0, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x1, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aV.Add( new Vector3( x0, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;

						// 左
						aV.Add( new Vector3( x0, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x0, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 1 ) ) ;

						aV.Add( new Vector3( x0, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aV.Add( new Vector3( x0, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;

						// 右
						aV.Add( new Vector3( x1, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x1, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 1 ) ) ;

						aV.Add( new Vector3( x1, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aV.Add( new Vector3( x1, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;

						// 上
						aV.Add( new Vector3( x0, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x1, y0, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 1 ) ) ;

						aV.Add( new Vector3( x0, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aV.Add( new Vector3( x1, y0, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;

						// 下
						aV.Add( new Vector3( x0, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 1 ) ) ;

						aV.Add( new Vector3( x1, y1, z0 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 1 ) ) ;

						aV.Add( new Vector3( x0, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 0, 0 ) ) ;

						aV.Add( new Vector3( x1, y1, z1 ) ) ;
						aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
						aT.Add( new Vector2( 1, 0 ) ) ;

						aI.Add( o + 0 ) ;
						aI.Add( o + 1 ) ;
						aI.Add( o + 2 ) ;

						aI.Add( o + 1 ) ;
						aI.Add( o + 3 ) ;
						aI.Add( o + 2 ) ;

						o += 4 ;
					}
				}
			}


			Build( "VoxelMesh", aV.ToArray(), null, aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
		}
	}
}
