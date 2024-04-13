using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:ライン
	/// </summary>
	public class Line : MaskableGraphicWrapper
	{
		/// <summary>
		/// スプライト
		/// </summary>
		[SerializeField]
		protected Sprite m_Sprite = null ;

		private Texture2D m_BlankTexture = null ;

		public  Sprite  sprite
		{
			get
			{
				return m_Sprite ;
			}
			set
			{
				if( m_Sprite != value )
				{
					m_Sprite  = value ;

					if( m_Sprite != null )
					{
						CanvasRenderer.SetTexture( m_Sprite.texture ) ;
					}
					else
					{
						if( m_BlankTexture == null )
						{
							m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
						}
						CanvasRenderer.SetTexture( m_BlankTexture ) ;
					}

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 最初のカラー
		/// </summary>
		[SerializeField]
		protected Color m_StartColor = Color.white ;

		public    Color  startColor
		{
			get
			{
				return m_StartColor ;
			}
			set
			{
				if( m_StartColor.Equals( value ) == false )
				{
					m_StartColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 最後のカラー
		/// </summary>
		[SerializeField]
		protected Color m_EndColor = Color.white ;

		public    Color  endColor
		{
			get
			{
				return m_EndColor ;
			}
			set
			{
				if( m_EndColor.Equals( value ) == false )
				{
					m_EndColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 最初の太さ
		/// </summary>
		[SerializeField]
		protected float m_StartWidth = 2 ;

		public    float  startWidth
		{
			get
			{
				return m_StartWidth ;
			}
			set
			{
				if( m_StartWidth != value )
				{
					m_StartWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 最後の太さ
		/// </summary>
		[SerializeField]
		protected float m_EndWidth =  2 ;

		public    float  endWidth
		{
			get
			{
				return m_EndWidth ;
			}
			set
			{
				if( m_EndWidth != value )
				{
					m_EndWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}
		

		/// <summary>
		/// オフセット
		/// </summary>
		[SerializeField]
		protected Vector2 m_Offset = Vector2.zero ;

		public  Vector2 offset
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

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 頂点配列
		/// </summary>
		public Vector2[] vertices = new Vector2[]{ Vector2.zero, Vector2.one } ;

		/// <summary>
		/// 座標の位置タイプ
		/// </summary>
		public enum PositionType
		{
			Relative = 0,
			Absolute = 1,
		}

		[SerializeField]
		protected PositionType m_PositionType = PositionType.Relative ;

		public  PositionType  positionType
		{
			get
			{
				return m_PositionType ;
			}
			set
			{
				if( m_PositionType != value )
				{
					m_PositionType  = value ;

					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField]
		protected float m_PreferredWidth = 0 ;

		/// <summary>
		/// 実際の横幅
		/// </summary>
		public float   preferredWidth
		{
			get
			{
				return m_PreferredWidth ;
			}
		}

		[SerializeField]
		protected float m_PreferredHeight = 0 ;
		
		/// <summary>
		/// 実際の縦幅
		/// </summary>
		public  float  preferredHeight
		{
			get
			{
				return m_PreferredHeight ;
			}
		}

		//----------------------------------------------------------

		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			helper.Clear() ;

			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------
			
			if( vertices == null || vertices.Length <= 1 )
			{
				return ;
			}
		
			int i, j, l = vertices.Length ;

			// 最初に全体の長さを計算する
			float length = 0 ;
			for( i  = 1 ; i <  l ; i ++ )
			{
				length += ( vertices[ i ] - vertices[ i - 1 ] ).magnitude ;
			}
		
			if( length == 0 )
			{
				return ;
			}

			float bt = 0 ;
			float bb = 0 ;

			if( m_Sprite != null )
			{
				bt = m_Sprite.border.y ;
				bb = m_Sprite.border.w ;
			}

			var aV = new List<Vector2>() ;		// ボーダーも含めた頂点
			var aC = new List<float>() ;		// 色比率
			var aT = new List<float>() ;		// テクスチャのＹ座標位置
		
			float bt_d = 0 ;
			if( ( vertices[ 1 ] - vertices[ 0 ] ).magnitude >  bt )
			{
				bt_d = bt ;
			}
			float bb_d = 0 ;
			if( ( vertices[ l - 1 ] - vertices[ l - 2 ] ).magnitude >  bb )
			{
				bb_d = bb ;
			}
			
			float height_d = 0 ;
			if( m_Sprite != null )
			{
				height_d = m_Sprite.rect.height - bt_d - bb_d ;
			}
			float length_d = length - bt_d - bb_d ;
			
			Vector2 p0 = Vector2.zero ;
			Vector2 p1 ;

			// メッシュを生成する前の事前頂点情報を生成する
			float c = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( i  == 0 )
				{
					if( m_PositionType == PositionType.Relative )
					{
						p0 += vertices[ i ] ;
					}
					else
					{
						p0  = vertices[ i ] ;
					}

					aV.Add( p0 ) ;
					aC.Add( 0 ) ;

					if( m_Sprite != null )
					{
						aT.Add( 0 ) ;
					}
				
					if( bt_d >  0 )
					{
						if( m_PositionType == PositionType.Relative )
						{
							p1 = p0 + vertices[ 1 ] ;
						}
						else
						{
							p1 = vertices[ 1 ] ;
						}

						if( p0.Equals( p1 ) == false )
						{
							aV.Add( p0 + bt_d * ( p1 - p0 ).normalized ) ;
							aC.Add( bt_d / length ) ;

							if( m_Sprite != null )
							{
								aT.Add( bt_d ) ;
							}
						}
					}
				}
				else
				{
					p1 = p0 ;
					
					if( m_PositionType == PositionType.Relative )
					{
						p0 += vertices[ i ] ;
					}
					else
					{
						p0  = vertices[ i ] ;
					}

					if( p0.Equals( p1 ) == false )
					{
						if( bb_d >  0 && i == ( l - 1 ) )
						{
							aV.Add( p0 + bb_d * ( p1 - p0 ).normalized ) ;
							aC.Add( ( length - bb_d ) / length ) ;

							if( m_Sprite != null )
							{
								aT.Add( m_Sprite.rect.height - bb_d ) ;
							}
						}
				
						aV.Add( p0 ) ;
				
						c += ( p0 - p1 ).magnitude ;
				
						aC.Add( ( float )c / ( float )length ) ;
					
						if( m_Sprite != null )
						{
							if( i <= ( l - 2 ) )
							{
								aT.Add( bt_d + ( height_d * ( c - bt_d ) / length_d ) ) ;
							}
							else
							{
								aT.Add( m_Sprite.rect.height ) ;
							}
						}
					}
				}
			}

			if( aV.Count <  2 )
			{
				return ;
			}

			//-----------------------------------------------------------

			// 実際に頂点バッファを生成する

			List<UIVertex>	bV = new () ;
			List<int>		bI = new () ;


			float th = 0 ;
			
			if( m_Sprite != null )
			{
				th = m_Sprite.texture.height ;
			}
		
			float ty ;
			Vector2 lt = Vector2.zero, rt = Vector2.zero ;
		
			Vector2 normal, normal_n, normal_c ;
			Vector2 width ;
		
			float r, w, x, y, k ;
		
			Vector2 lv, rv ;
		
			int vi, vc ;

			UIVertex v ;
		
			Color color ;

			Vector3 normalVector = new (  0,  0, -1 ) ;

			int index ;

			for( i  = 0 ; i <  ( aV.Count - 1 ) ; i ++ )
			{
				// オフセット
				vi = bV.Count ;			
				vc = 0 ;

				//---------------------------------

				// ベクトル
				normal = ( aV[ i + 1 ] - aV[ i ] ).normalized ;

				//--------------------------------------------------------
				// 始点

				index = i ;

				// 全体比率
				r = aC[ index ] ;	// 開始点を０～終了点を１

				// その位置での太さの半分
				w = ( ( m_EndWidth - m_StartWidth ) * r + m_StartWidth ) * 0.5f ;

				//-------------

				// 幅
				width.x = - normal.y * w ;
				width.y =   normal.x * w ;

				// 点
				x = aV[ index ].x ;
				y = aV[ index ].y ;
			
				// 左の点
				lv.x = x + width.x ;
				lv.y = y + width.y ;
				
				// 右の点
				rv.x = x - width.x ;
				rv.y = y - width.y ;

				//------------

				// 色
				color = Color.Lerp( m_StartColor, m_EndColor, r ) ;

				if( m_Sprite != null )
				{
					ty = 1.0f - ( ( m_Sprite.rect.yMin + r ) / th ) ;
				}
				else
				{
					ty = 0 ;
				}

				//-------------------------------------------

				// 左の点
				v = new ()
				{
					position	= new Vector3( lv.x + m_Offset.x, lv.y + m_Offset.y, 0 ),
					normal		= normalVector,
					color		= color
				} ;

				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( lt.x, ty ) ;
				}

				bV.Add( v ) ;
				vc ++ ;

				//-------------

				// 右の点
				v = new ()
				{
					position	= new Vector3( rv.x + m_Offset.x, rv.y + m_Offset.y, 0 ),
					normal		= normalVector,
					color		= color
				} ;
				
				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( rt.x, ty ) ;
				}

				bV.Add( v ) ;
				vc ++ ;

				//--------------------------------------------------------
				// 終点

				index = i + 1 ;

				// 全体比率
				r = aC[ index ] ;	// 開始点を０～終了点を１

				// その位置での太さの半分
				w = ( ( m_EndWidth - m_StartWidth ) * r + m_StartWidth ) * 0.5f ;

				//-------------

				// 幅
				width.x = - normal.y * w ;
				width.y =   normal.x * w ;

				// 点
				x = aV[ index ].x ;
				y = aV[ index ].y ;
			
				// 左の点
				lv.x = x + width.x ;
				lv.y = y + width.y ;
				
				// 右の点
				rv.x = x - width.x ;
				rv.y = y - width.y ;

				//------------

				// 色
				color = Color.Lerp( m_StartColor, m_EndColor, r ) ;

				if( m_Sprite != null )
				{
					ty = 1.0f - ( ( m_Sprite.rect.yMin + r ) / th ) ;
				}
				else
				{
					ty = 0 ;
				}

				//-------------------------------------------

				// 左の点
				v = new ()
				{
					position	= new Vector3( lv.x + m_Offset.x, lv.y + m_Offset.y, 0 ),
					normal		= normalVector,
					color		= color
				} ;

				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( lt.x, ty ) ;
				}

				bV.Add( v ) ;
				vc ++ ;
		
				//-------------

				// 右の点
				v = new ()
				{
					position	= new Vector3( rv.x + m_Offset.x, rv.y + m_Offset.y, 0 ),
					normal		= normalVector,
					color		= color
				} ;
				
				if( m_Sprite != null )
				{
					v.uv0 = new Vector2( rt.x, ty ) ;
				}

				bV.Add( v ) ;
				vc ++ ;

				//---------------------------------

				if( i <  ( aV.Count - 2 ) && w >= 2.0f )
				{
					// 連結ポイントのポリゴンが必要かどうか

					// 次の方向ベクトル
					normal_n = ( aV[ i + 2 ] - aV[ i + 1 ] ).normalized ;

					// 内積計算
					k = Vector2.Dot( normal, normal_n ) ;

					if( Mathf.Abs( k ) <  0.98f )
					{
						// 連結ポイントのポリゴンを追加する

						// 連結ポイントの方向
						normal_c = ( normal + normal_n ).normalized ; 

						k = Vector2.Dot( normal_n, normal_c ) ;
						float m = Mathf.Abs( k ) * w * 0.88f ;

						Vector2 lc = new ( x - normal_c.y * m, y + normal_c.x * m ) ;
						Vector2 rc = new ( x + normal_c.y * m, y - normal_c.x * m ) ;


						Vector2 el0 = lc + ( normal_c * m ) ;
						Vector2 er0 = rc + ( normal_c * m ) ;

						Vector2 el1 = lc - ( normal_c * m ) ;
						Vector2 er1 = rc - ( normal_c * m ) ;

						//-------------------------------
						// 各種情報は終点と同じ

						//-----------
						// 前方

						// 左の点
						v = new ()
						{
							position	= new Vector3( el0.x + m_Offset.x, el0.y + m_Offset.y, 0 ),
							normal		= normalVector,
							color		= color
						} ;

						if( m_Sprite != null )
						{
							v.uv0 = new Vector2( lt.x, ty ) ;
						}

						bV.Add( v ) ;
						vc ++ ;
		
						//-------------

						// 右の点
						v = new ()
						{
							position	= new Vector3( er0.x + m_Offset.x, er0.y + m_Offset.y, 0 ),
							normal		= normalVector,
							color		= color
						} ;
				
						if( m_Sprite != null )
						{
							v.uv0 = new Vector2( rt.x, ty ) ;
						}

						bV.Add( v ) ;
						vc ++ ;

						//-----------
						// 後方

						// 左の点
						v = new ()
						{
							position	= new Vector3( el1.x + m_Offset.x, el1.y + m_Offset.y, 0 ),
							normal		= normalVector,
							color		= color
						} ;

						if( m_Sprite != null )
						{
							v.uv0 = new Vector2( lt.x, ty ) ;
						}

						bV.Add( v ) ;
						vc ++ ;
		
						//-------------

						// 右の点
						v = new ()
						{
							position	= new Vector3( er1.x + m_Offset.x, er1.y + m_Offset.y, 0 ),
							normal		= normalVector,
							color		= color
						} ;
				
						if( m_Sprite != null )
						{
							v.uv0 = new Vector2( rt.x, ty ) ;
						}

						bV.Add( v ) ;
						vc ++ ;
					}
				}

				//---------------------------------------------------------

				// インデックス
				for( j  = 0 ; j <   vc ; j = j + 4 )
				{
					bI.Add( vi + j ) ;
					bI.Add( vi + j + 1 ) ;
					bI.Add( vi + j + 2 ) ;
					
					bI.Add( vi + j + 1 ) ;
					bI.Add( vi + j + 3 ) ;
					bI.Add( vi + j + 2 ) ;
				}
			}

			//----------------------------------------

			if( bV.Count >  0 && bI.Count >  0 )
			{
				// 頂点の最大最小からバウンディングボックスのサイズを算出する

				float xMin =   Mathf.Infinity ;
				float xMax = - Mathf.Infinity ;
				float yMin =   Mathf.Infinity ;
				float yMax = - Mathf.Infinity ;

				l = bV.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					x = bV[ i ].position.x ;
					y = bV[ i ].position.y ;
					if( x <  xMin )
					{
						xMin = x ;
					}
					if( x >  xMax )
					{
						xMax = x ;
					}
					if( y <  yMin )
					{
						yMin = y ;
					}
					if( y >  yMax )
					{
						yMax = y ;
					}
				}

				m_PreferredWidth  = xMax - xMin ;
				m_PreferredHeight = yMax - yMin ;

//				Debug.LogWarning( "w:" + mPreferredWidth + " h:" + mPreferredHeight ) ;

				helper.AddUIVertexStream( bV, bI ) ;
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			if( m_Sprite != null )
			{
				CanvasRenderer.SetTexture( m_Sprite.texture ) ;
			}
			else
			{
				if( m_BlankTexture == null )
				{
					m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
				}
				CanvasRenderer.SetTexture( m_BlankTexture ) ;
			}
		}
	}
}
