#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System.Collections ;

[ CustomEditor( typeof( SoftMesh ) ) ]
public class SoftMeshInspector : Editor
{
	private bool	m_UVA_Show = false ;
	private bool[]	m_UVI_Show = null ;

	/// <summary>
	/// スンスペクター描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		// とりあえずデフォルト
		DrawDefaultInspector() ;
		
		//--------------------------------------------
		
		// ターゲットのインスタンス
		SoftMesh view = target as SoftMesh ;
		
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		// タイプ
		SoftMesh.ShapeTypes shapeType = ( SoftMesh.ShapeTypes )EditorGUILayout.EnumPopup( "Shape Type",  view.shapeType ) ;
		if( shapeType != view.shapeType )
		{
			Undo.RecordObject( view, "SoftMesh : Shape Type Change" ) ;	// アンドウバッファに登録
			view.shapeType = shapeType ;
			EditorUtility.SetDirty( view ) ;
		}

		if( view.shapeType == SoftMesh.ShapeTypes.Capsule || view.shapeType == SoftMesh.ShapeTypes.Cylinder )
		{
			// カプセル・シリンダーの場合の方向
			SoftMesh.Directions direction = ( SoftMesh.Directions )EditorGUILayout.EnumPopup( "Direction", view.Direction ) ;
			if( direction != view.Direction )
			{
				Undo.RecordObject( view, "SoftMesh : Direction Change" ) ;	// アンドウバッファに登録
				view.Direction = direction ;
				EditorUtility.SetDirty( view ) ;
			}
		}
		else
		if( view.shapeType == SoftMesh.ShapeTypes.Plane )
		{
			// プレーンの場合の方向
			SoftMesh.PlaneDirections planeDirection = ( SoftMesh.PlaneDirections )EditorGUILayout.EnumPopup( "Plane Direction",  view.PlaneDirection ) ;
			if( planeDirection != view.PlaneDirection )
			{
				Undo.RecordObject( view, "SoftMesh : Plane Direction Change" ) ;	// アンドウバッファに登録
				view.PlaneDirection = planeDirection ;
				EditorUtility.SetDirty( view ) ;
			}
		}

		if( view.shapeType == SoftMesh.ShapeTypes.Cube || view.shapeType == SoftMesh.ShapeTypes.Sphere || view.shapeType == SoftMesh.ShapeTypes.Capsule || view.shapeType == SoftMesh.ShapeTypes.Cylinder || view.shapeType == SoftMesh.ShapeTypes.Plane )
		{
			// ３Ｄ系

			// オフセット
			Vector3 offset = EditorGUILayout.Vector3Field( "Offset", view.Offset ) ;
			if( offset != view.Offset )
			{
				Undo.RecordObject( view, "SoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				view.Offset = offset ;
				EditorUtility.SetDirty( view ) ;
			}
	
			// サイズ
			Vector3 size = EditorGUILayout.Vector3Field( "Size", view.Size ) ;
			if( size != view.Size )
			{
				Undo.RecordObject( view, "SoftMesh : Size Change" ) ;	// アンドウバッファに登録
				view.Size = size ;
				EditorUtility.SetDirty( view ) ;
			}
		}
		else
		{
			// ２Ｄ系

			// オフセット
			Vector2 offset2D_Old = new Vector2( view.Offset.x, view.Offset.y ) ;
			Vector2 offset2D_New = EditorGUILayout.Vector2Field( "Offset", offset2D_Old ) ;
			if( offset2D_New != offset2D_Old )
			{
				Undo.RecordObject( view, "SoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				view.Offset = new Vector3( offset2D_New.x, offset2D_New.y, view.Offset.z ) ;
				EditorUtility.SetDirty( view ) ;
			}
	
			// サイズ
			Vector2 size2D_Old = new Vector2( view.Size.x, view.Size.y ) ;
			Vector2 size2D_New = EditorGUILayout.Vector2Field( "Size", size2D_Old ) ;
			if( size2D_New != size2D_Old )
			{
				Undo.RecordObject( view, "SoftMesh : Size Change" ) ;	// アンドウバッファに登録
				view.Size = new Vector2( size2D_New.x, size2D_New.y ) ;
				EditorUtility.SetDirty( view ) ;
			}
		}

		// カラー
		Color vertexColor = Color.white ;
		vertexColor.r = view.VertexColor.r ;
		vertexColor.g = view.VertexColor.g ;
		vertexColor.b = view.VertexColor.b ;
		vertexColor.a = view.VertexColor.a ;
		vertexColor = EditorGUILayout.ColorField( "Vertex Color", vertexColor ) ;
//		Debug.LogWarning( "O:" + tTarget.vertexColor + " N:" + tVertexColor ) ;
		if
		(
			vertexColor.r != view.VertexColor.r ||
			vertexColor.g != view.VertexColor.g ||
			vertexColor.b != view.VertexColor.b ||
			vertexColor.a != view.VertexColor.a
		)
		{
//			Debug.LogWarning( "変化した" ) ;
			Undo.RecordObject( view, "SoftMesh : Vertex Color Change" ) ;	// アンドウバッファに登録
			view.VertexColor = vertexColor ;
			EditorUtility.SetDirty( view ) ;
		}

		// ＵＶ
		if( view.shapeType == SoftMesh.ShapeTypes.Cube || view.shapeType == SoftMesh.ShapeTypes.Plane || view.shapeType == SoftMesh.ShapeTypes.Box2D )
		{
			Rect[] uv = view.UV ;
			if( uv != null && uv.Length >  0 )
			{
				m_UVA_Show = EditorGUILayout.Foldout( m_UVA_Show, "UV" ) ;
				if( m_UVA_Show == true )
				{
					Rect uv_Old ;
					Rect uv_New ;

					int i, l = uv.Length ;
					if( m_UVI_Show == null )
					{
						m_UVI_Show = new bool[ l ] ;
					}

					if( view.shapeType == SoftMesh.ShapeTypes.Plane || view.shapeType == SoftMesh.ShapeTypes.Box2D )
					{
						if( l >  1 )
						{
							l  = 1 ;
						}
					}

					for( i  = 0 ; i <  l ; i ++ )
					{
						uv_Old = new Rect( uv[ i ].x, uv[ i ].y, uv[ i ].width, uv[ i ].height ) ;
						uv_New = new Rect( uv[ i ].x, uv[ i ].y, uv[ i ].width, uv[ i ].height ) ;

						m_UVI_Show[ i ] = EditorGUILayout.Foldout( m_UVI_Show[ i ], " Index " + i ) ;
						if( m_UVI_Show[ i ] == true )
						{
							uv_New = EditorGUILayout.RectField( uv_New ) ;
							if( uv_Old.Equals( uv_New ) == false )
							{
								Undo.RecordObject( view, "SoftMesh : UV Change" ) ;	// アンドウバッファに登録
								view.SetUV( i, uv_New ) ;
								EditorUtility.SetDirty( view ) ;
							}
						}
					}
				}
			}
		}


		// スプリット
		int split = EditorGUILayout.IntSlider( "Split", view.Split, 0, 5 ) ;
		if( split != view.Split )
		{
			Undo.RecordObject( view, "SoftMesh : Split Change" ) ;	// アンドウバッファに登録
			view.Split = split ;
			EditorUtility.SetDirty( view ) ;
		}

		if( view.shapeType == SoftMesh.ShapeTypes.Plane || view.shapeType == SoftMesh.ShapeTypes.Box2D )
		{
			// タイリング
			bool tiling = EditorGUILayout.Toggle( "Tiling", view.Tiling ) ;
			if( tiling != view.Tiling )
			{
				Undo.RecordObject( view, "SoftMesh : Tiling Change" ) ;	// アンドウバッファに登録
				view.Tiling = tiling ;
				EditorUtility.SetDirty( view ) ;
			}
		}

		//-----------------------------------------------------------

		EditorGUILayout.Separator() ;   // 少し区切りスペース

		// マテリアル
		Material material = EditorGUILayout.ObjectField( "Maretial", view.Material, typeof( Material ), false ) as Material ;
		if( material != view.Material )
		{
			Undo.RecordObject( view, "SoftMesh : Material Change" ) ;	// アンドウバッファに登録
			view.Material = material ;
			EditorUtility.SetDirty( view ) ;
		}

		// テクスチャ
		Texture2D texture = EditorGUILayout.ObjectField( "Texture", view.Texture, typeof( Texture2D ), false ) as Texture2D ;
		if( texture != view.Texture )
		{
			Undo.RecordObject( view, "SoftMesh : Texture Change" ) ;	// アンドウバッファに登録
			view.Texture = texture ;
			EditorUtility.SetDirty( view ) ;
		}

		// コライダー
		bool isCollider = EditorGUILayout.Toggle( "Collider", view.IsCollider ) ;
		if( isCollider != view.IsCollider )
		{
			Undo.RecordObject( view, "SoftMesh : Collider Change" ) ;	// アンドウバッファに登録
			view.IsCollider = isCollider ;
			EditorUtility.SetDirty( view ) ;
		}

		if( view.IsCollider == true )
		{
			// コライダーの自動調整
			bool colliderAdjustment = EditorGUILayout.Toggle( "Collider Adjustment", view.ColliderAdjustment ) ;
			if( colliderAdjustment != view.ColliderAdjustment )
			{
				Undo.RecordObject( view, "SoftMesh : Collider Adjustment Change" ) ;	// アンドウバッファに登録
				view.ColliderAdjustment = colliderAdjustment ;
				EditorUtility.SetDirty( view ) ;
			}
		}

		// リジッドボディ
		bool isRigidbody = EditorGUILayout.Toggle( "Rigidbody", view.IsRigidbody ) ;
		if( isRigidbody != view.IsRigidbody )
		{
			Undo.RecordObject( view, "SoftMesh : Rigidbody Change" ) ;	// アンドウバッファに登録
			view.IsRigidbody = isRigidbody ;
			EditorUtility.SetDirty( view ) ;
		}
/*
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		GUI.backgroundColor = Color.cyan ;
		if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
		{
			tTarget.Refresh() ;
		}
		GUI.backgroundColor = Color.white ;
*/
	}
}

#endif
