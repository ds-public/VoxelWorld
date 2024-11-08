using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
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
	/// スプライト制御クラス  Version 2024/08/25
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public partial class SpriteTransform : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteTransform", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteTransform" )]					// ポップアップメニューから
		public static void CreateSpriteEmpty()
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

			Undo.RecordObject( go, "Add a child SpriteTransform" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteTransform" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteTransform>() ;

			// 一番上に移動させる
			while( ComponentUtility.MoveComponentUp( component ) ){}

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

		[SerializeField][HideInInspector]
		protected Vector2 m_Offset = Vector2.zero ;

		/// <summary>
		/// オフセット
		/// </summary>
		public virtual Vector2 Offset
		{
			get
			{
				return m_Offset ;
			}
			protected set
			{
				m_Offset = value ;
			}
		}

		/// <summary>
		/// オフセットを取得する
		/// </summary>
		/// <returns></returns>
		public virtual Vector2 GetOffset() => m_Offset ;
		
		//-----

		[SerializeField][HideInInspector]
		protected Vector2 m_DeltaSize = Vector2.one ;

		/// <summary>
		/// 実サイズ
		/// </summary>
		public virtual Vector2 DeltaSize
		{
			get
			{
				return m_DeltaSize ;
			}
			protected set
			{
				m_DeltaSize = value ;
			}
		}

		/// <summary>
		/// 実サイズを取得する
		/// </summary>
		/// <returns></returns>
		public virtual Vector2 GetDeltaSize() => m_DeltaSize ;

		//-------------------------------------------------------------------------------------------
		// 拡張プロパティ

		[SerializeField][HideInInspector]
		protected HorizontalAnchorTypes	m_HorizontalAnchorType = HorizontalAnchorTypes.Center ;

		/// <summary>
		/// 水平方向のアンカータイプ
		/// </summary>
		public virtual HorizontalAnchorTypes HorizontalAnchorType
		{
			get
			{
				return m_HorizontalAnchorType ;
			}
			set
			{
				if( m_HorizontalAnchorType != value )
				{
					m_HorizontalAnchorType  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}


		[SerializeField][HideInInspector]
		protected float m_AnchorPositionX = 0 ;

		protected float m_LocalPositionX ;

		/// <summary>
		/// 水平方向の位置
		/// </summary>
		public virtual float AnchorPositionX
		{
			get
			{
				return m_AnchorPositionX ;
			}
			set
			{
				if( m_LocalPositionX != transform.localPosition.x )
				{
					// AnchorPosition を先に更新する必要がある
					UpdateSpriteTransformAnchorPosition() ;
				}

				if( m_AnchorPositionX != value )
				{
					// 高速版
					if( m_HorizontalAnchorType != HorizontalAnchorTypes.Stretch )
					{
						float deltaPositionX = value - m_AnchorPositionX ;

						var p = transform.localPosition ;
						p.x += deltaPositionX ;
						transform.localPosition = p ;
					}

					m_AnchorPositionX  = value ;

//					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// 水平方向の位置
		/// </summary>
		public virtual float PositionX
		{
			get
			{
				return m_AnchorPositionX ;
			}
			set
			{
				if( m_LocalPositionX != transform.localPosition.x )
				{
					// AnchorPosition を先に更新する必要がある
					UpdateSpriteTransformAnchorPosition() ;
				}

				if( m_AnchorPositionX != value )
				{
					// 高速版
					if( m_HorizontalAnchorType != HorizontalAnchorTypes.Stretch )
					{
						float deltaPositionX = value - m_AnchorPositionX ;

						var p = transform.localPosition ;
						p.x += deltaPositionX ;
						transform.localPosition = p ;
					}

					m_AnchorPositionX  = value ;

//					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// Ｘ座標を設定する
		/// </summary>
		/// <param name="x"></param>
		public virtual void SetPositionX( float x )
		{
			AnchorPositionX = x ;
		}

		[SerializeField][HideInInspector]
		protected float m_Width = 1 ;

		/// <summary>
		/// 水平方向の幅
		/// </summary>
		public virtual float Width
		{
			get
			{
				return m_Width ;
			}
			set
			{
				if( m_Width != value )
				{
					m_Width  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_MarginL = 0 ;

		/// <summary>
		/// 水平方向のマージン(左)
		/// </summary>
		public virtual float MarginL
		{
			get
			{
				return m_MarginL ;
			}
			set
			{
				if( m_MarginL != value )
				{
					m_MarginL  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_MarginR = 0 ;

		/// <summary>
		/// 水平方向のマージン(右)
		/// </summary>
		public virtual float MarginR
		{
			get
			{
				return m_MarginR ;
			}
			set
			{
				if( m_MarginR != value )
				{
					m_MarginR  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		//-----------------------------------

		[SerializeField][HideInInspector]
		protected VerticalAnchorTypes	m_VerticalAnchorType = VerticalAnchorTypes.Middle ;

		/// <summary>
		/// 垂直方向のアンカータイプ
		/// </summary>
		public virtual VerticalAnchorTypes VerticalAnchorType
		{
			get
			{
				return m_VerticalAnchorType ;
			}
			set
			{
				if( m_VerticalAnchorType != value )
				{
					m_VerticalAnchorType  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_AnchorPositionY = 0 ;

		protected float m_LocalPositionY ;


		/// <summary>
		/// 垂直方向の位置
		/// </summary>
		public virtual float AnchorPositionY
		{
			get
			{
				return m_AnchorPositionY ;
			}
			set
			{
				if( m_LocalPositionY != transform.localPosition.y )
				{
					// AnchorPosition を先に更新する必要がある
					UpdateSpriteTransformAnchorPosition() ;
				}

				if( m_AnchorPositionY != value )
				{
					// 高速版
					if( m_VerticalAnchorType != VerticalAnchorTypes.Stretch )
					{
						float deltaPositionY = value - m_AnchorPositionY ;

						var p = transform.localPosition ;
						p.y += deltaPositionY ;
						transform.localPosition = p ;
					}

					m_AnchorPositionY  = value ;

//					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// 垂直方向の位置
		/// </summary>
		public virtual float PositionY
		{
			get
			{
				return m_AnchorPositionY ;
			}
			set
			{
				if( m_LocalPositionY != transform.localPosition.y )
				{
					// AnchorPosition を先に更新する必要がある
					UpdateSpriteTransformAnchorPosition() ;
				}

				if( m_AnchorPositionY != value )
				{
					// 高速版
					if( m_VerticalAnchorType != VerticalAnchorTypes.Stretch )
					{
						float deltaPositionY = value - m_AnchorPositionY ;

						var p = transform.localPosition ;
						p.y += deltaPositionY ;
						transform.localPosition = p ;
					}

					m_AnchorPositionY  = value ;

//					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// Ｙ座標を設定する
		/// </summary>
		/// <param name="y"></param>
		public virtual void SetPositionY( float y )
		{
			AnchorPositionY = y ;
		}

		[SerializeField][HideInInspector]
		protected float m_Height = 1 ;

		/// <summary>
		/// 垂直方向の幅
		/// </summary>
		public virtual float Height
		{
			get
			{
				return m_Height ;
			}
			set
			{
				if( m_Height != value )
				{
					m_Height  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_MarginT = 0 ;

		/// <summary>
		/// 垂直方向のマージン(上)
		/// </summary>
		public virtual float MarginT
		{
			get
			{
				return m_MarginT ;
			}
			set
			{
				if( m_MarginT != value )
				{
					m_MarginT  = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_MarginB = 0 ;

		/// <summary>
		/// 垂直方向のマージン(下)
		/// </summary>
		public virtual float MarginB
		{
			get
			{
				return m_MarginB ;
			}
			set
			{
				if( m_MarginB != value )
				{
					m_MarginB  = value ;

					UpdateSpriteTransform() ;
				}
			}

		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected Vector2 m_Pivot = Vector2.zero ;

		/// <summary>
		/// ピボット(オフセットの代わりに使用可能) ※-0.5～+0.5
		/// </summary>
		public virtual Vector2 Pivot
		{
			get
			{
				return m_Pivot ;
			}
			set
			{
				if( m_Pivot.Equals( value ) == false )
				{
					m_Pivot = value ;

					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// ピボットを取得する
		/// </summary>
		public  virtual Vector2 GetPivot()
			=> m_Pivot ;

		/// <summary>
		/// ピボットを設定する
		/// </summary>
		/// <param name="pivot"></param>
		public virtual void SetPivot( Vector2 pivot )
		{
			Pivot = pivot ;
		}

		//-------------------------------------------------------------------------------------------

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

		// キャッシュした最初に見つかった親
		[SerializeField][HideInInspector]
		private SpriteTransform	m_ParentSpriteTransform	= null ;


		// 親の SpriteTransform の情報を取得する
		protected bool GetParentSpriteTransform( out Vector2 offset, out Vector2 deltaSize )
		{
			offset		= Vector2.zero ;
			deltaSize	= Vector2.zero ;

			// 親の矩形領域を取得する

			//----------------------------------------------------------

			// ヒエラルキーの構成に変化があったかどうか確認する

			UpdateActiveHierarchy() ;

			if( CompareHierarchy() == true && m_ParentSpriteTransform != null )
			{
				// ヒエラルキーの構成に変化は無く親トランスフォームのキャッシュが存在する
				offset		= m_ParentSpriteTransform.Offset ;
				deltaSize	= m_ParentSpriteTransform.DeltaSize ;
				return true ;
			}

			UpdateCachedHierarchy() ;

			//----------------------------------------------------------

			// 親の SpriteTransform を取得し直す
			if( transform.parent != null )
			{
				m_ParentSpriteTransform = transform.parent.GetComponentInParent<SpriteTransform>( true ) ;
				if( m_ParentSpriteTransform != null )
				{
					offset		= m_ParentSpriteTransform.Offset ;
					deltaSize	= m_ParentSpriteTransform.DeltaSize ;
					return true ;
				}
			}

			//----------------------------------------------------------

			Debug.LogWarning( "親が発見出来ず : " + name ) ;

			// 発生出来ず
			return false ;
		}

		// 矩形領域情報(Position・Offset・DeltaSize)を更新する
		protected void UpdateSpriteTransform()
		{
			GetParentSpriteTransform( out var parentOffset, out var parentDeltaSize ) ;

			//----------------------------------
			// 横方向の処理

			float positionX		= 0 ;
			float offsetX		= 0 ;
			float deltaSizeX	= 1 ;

			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Center )
			{
				// 親の中心を基準とする

				positionX	= parentOffset.x + m_AnchorPositionX ; 
				offsetX		= - m_Width * m_Pivot.x ;
				deltaSizeX	= m_Width ;
			}
			else
			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Left )
			{
				// 0 の時 parentDeltaSize * -0.5f
				// Pvoit = -0.5f だと +0.5f * parentDeltaSize になっているので 0
				// Pivot = +0.5f だと -0.5f * parentDeltaSize になっているのて - parentDeltaSize

				//  0.0 → -0.5
				// +0.1 → -0.4
				// +0.5 →  0.0
				// +0.4 → -0.1
				// -0.5 → -1.0

				positionX	= parentOffset.x - ( parentDeltaSize.x * 0.5f ) + m_AnchorPositionX ;
				offsetX		= - m_Width * m_Pivot.x ;
				deltaSizeX	= m_Width ;
			}
			else
			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Right )
			{
				// 0 の時 parentDeltaSize * +0.5f
				// Pvoit = +0.5f だと -0.5f * parentDeltaSize になっているので 0
				// Pivot = -0.5f だと +0.5f * parentDeltaSize になっているのて + parentDeltaSize

				//  0.0 → +0.5
				// -0.1 → +0.4
				// -0.5 →  0.0
				// -0.4 → +0.1
				// +0.5 → +1.0

				positionX	= parentOffset.x + ( parentDeltaSize.x * 0.5f ) + m_AnchorPositionX ;
				offsetX		= - m_Width * m_Pivot.x ;
				deltaSizeX	= m_Width ;
			}
			else
			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Stretch )
			{
				// Stretch

				deltaSizeX		= parentDeltaSize.x - m_MarginL - m_MarginR ;
				offsetX			= - deltaSizeX * m_Pivot.x ;
				positionX		= parentOffset.x - offsetX + ( m_MarginL - m_MarginR ) * 0.5f ;
			}

			//--------------

			float positionY		= 0 ;
			float offsetY		= 0 ;
			float deltaSizeY	= 1 ;

			if( m_VerticalAnchorType == VerticalAnchorTypes.Middle )
			{
				// 親の中心を基準とする

				positionY	= parentOffset.y + m_AnchorPositionY ; 
				offsetY		= - m_Height * m_Pivot.y ;
				deltaSizeY	= m_Height ;
			}
			else
			if( m_VerticalAnchorType == VerticalAnchorTypes.Bottom )
			{
				// 0 の時 parentDeltaSize * -0.5f
				// Pvoit = -0.5f だと +0.5f * parentDeltaSize になっているので 0
				// Pivot = +0.5f だと -0.5f * parentDeltaSize になっているのて - parentDeltaSize

				//  0.0 → -0.5
				// +0.1 → -0.4
				// +0.5 →  0.0
				// +0.4 → -0.1
				// -0.5 → -1.0

				positionY	= parentOffset.y - ( parentDeltaSize.y * 0.5f ) + m_AnchorPositionY ;
				offsetY		= - m_Height * m_Pivot.y ;
				deltaSizeY	= m_Height ;
			}
			else
			if( m_VerticalAnchorType == VerticalAnchorTypes.Top )
			{
				// 0 の時 parentDeltaSize * +0.5f
				// Pvoit = +0.5f だと -0.5f * parentDeltaSize になっているので 0
				// Pivot = -0.5f だと +0.5f * parentDeltaSize になっているのて + parentDeltaSize

				//  0.0 → +0.5
				// -0.1 → +0.4
				// -0.5 →  0.0
				// -0.4 → +0.1
				// +0.5 → +1.0

				positionY	= parentOffset.y + ( parentDeltaSize.y * 0.5f ) + m_AnchorPositionY ;
				offsetY		= - m_Height * m_Pivot.y ;
				deltaSizeY	= m_Height ;
			}
			else
			if( m_VerticalAnchorType == VerticalAnchorTypes.Stretch )
			{
				// Stretch

				deltaSizeY		= parentDeltaSize.y - m_MarginB - m_MarginT ;
				offsetY			= - deltaSizeY * m_Pivot.y ;
				positionY		= parentOffset.y - offsetY + ( m_MarginB - m_MarginT ) * 0.5f ;
			}

			//----------------------------------

			var offset		= new Vector2( offsetX, offsetY ) ;
			var deltaSize	= new Vector2( deltaSizeX, deltaSizeY ) ;

			bool isModified = ( m_Offset.Equals( offset ) == false ) || ( m_DeltaSize.Equals( offset ) == false ) ;

			SetTransformPosition( positionX, positionY ) ;			// メッシュの作り直しは発生しない
			Offset		= offset ;									// override した継承側のプロパティを呼ぶので変化が場合はメッシュの作り直しが発生する事がある
			DeltaSize	= deltaSize ;								// override した継承側のプロパティを呼ぶので変化が場合はメッシュの作り直しが発生する事がある

			//----------------------------------

//			Debug.Log( "UpdateSpriteTransform が呼ばれた : " + name ) ;

			if( isModified == true )
			{
				// Offset か DeltaSize に変化があった場合は子全てで UpdateSpriteTransform() を実行する必要がある
				ApplyToChildren() ;
			}
		}

		// 矩形領域情報(Position・Offset・DeltaSize)を更新する
		protected void UpdateSpriteTransformPosition()
		{
			GetParentSpriteTransform( out var parentOffset, out var parentDeltaSize ) ;

			//----------------------------------
			// 横方向の処理

			float positionX		= 0 ;

			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Center )
			{
				// 親の中心を基準とする

				positionX	= parentOffset.x + m_AnchorPositionX ; 
			}
			else
			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Left )
			{
				positionX	= parentOffset.x - ( parentDeltaSize.x * 0.5f ) + m_AnchorPositionX ;
			}
			else
			if( m_HorizontalAnchorType == HorizontalAnchorTypes.Right )
			{
				positionX	= parentOffset.x + ( parentDeltaSize.x * 0.5f ) + m_AnchorPositionX ;
			}

			//--------------

			float positionY		= 0 ;

			if( m_VerticalAnchorType == VerticalAnchorTypes.Middle )
			{
				// 親の中心を基準とする

				positionY	= parentOffset.y + m_AnchorPositionY ; 
			}
			else
			if( m_VerticalAnchorType == VerticalAnchorTypes.Bottom )
			{
				positionY	= parentOffset.y - ( parentDeltaSize.y * 0.5f ) + m_AnchorPositionY ;
			}
			else
			if( m_VerticalAnchorType == VerticalAnchorTypes.Top )
			{
				positionY	= parentOffset.y + ( parentDeltaSize.y * 0.5f ) + m_AnchorPositionY ;
			}

			//----------------------------------

			SetTransformPosition( positionX, positionY ) ;			// メッシュの作り直しは発生しない
		}

		// 矩形領域情報(Position・Offset・DeltaSize)を更新する
		protected void UpdateSpriteTransformAnchorPosition()
		{
			GetParentSpriteTransform( out var parentOffset, out var parentDeltaSize ) ;

			//----------------------------------
			// 横方向の処理

			if( m_LocalPositionX != transform.localPosition.x )
			{
				float positionX	 = transform.localPosition.x ;

				if( m_HorizontalAnchorType == HorizontalAnchorTypes.Center )
				{
					// 親の中心を基準とする

					m_AnchorPositionX = positionX - parentOffset.x ;
				}
				else
				if( m_HorizontalAnchorType == HorizontalAnchorTypes.Left )
				{
					m_AnchorPositionX = positionX - parentOffset.x + ( parentDeltaSize.x * 0.5f ) ;
				}
				else
				if( m_HorizontalAnchorType == HorizontalAnchorTypes.Right )
				{
					m_AnchorPositionX = positionX - parentOffset.x - ( parentDeltaSize.x * 0.5f ) ;
				}

				m_LocalPositionX = transform.localPosition.x ;
			}

			//--------------

			if( m_LocalPositionY != transform.localPosition.y )
			{
				float positionY	= transform.localPosition.y ;

				if( m_VerticalAnchorType == VerticalAnchorTypes.Middle )
				{
					// 親の中心を基準とする

					m_AnchorPositionY = positionY - parentOffset.y ;
				}
				else
				if( m_VerticalAnchorType == VerticalAnchorTypes.Bottom )
				{
					m_AnchorPositionY = positionY - parentOffset.y + ( parentDeltaSize.y * 0.5f ) ;
				}
				else
				if( m_VerticalAnchorType == VerticalAnchorTypes.Top )
				{
					m_AnchorPositionY = positionY - parentOffset.y - ( parentDeltaSize.y * 0.5f ) ;
				}

				m_LocalPositionY = transform.localPosition.y ;
			}
		}

		// 位置を設定する
		protected void SetTransformPosition( float x, float y )
		{
			transform.localPosition = new Vector3( x, y, transform.localPosition.z ) ;
		}

		// 直接の子
		private readonly List<SpriteTransform> m_ChildSpriteTransforms = new () ;

		// Offset または DeltaSize に変化があった事を子に通知する
		protected void ApplyToChildren()
		{
			m_ChildSpriteTransforms.Clear() ;

			int i, l = transform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var child = transform.GetChild( i ) ;

				if( child.TryGetComponent<SpriteTransform>( out var spriteTransform ) == true )
				{
					m_ChildSpriteTransforms.Add( spriteTransform ) ;
				}
				else
				{
					// 直接の子には SpriteTransform が存在しない
					CollectSpriteTransformFromChildren( child, m_ChildSpriteTransforms ) ;
				}

				// SendMessage() メソッドは、送信対象に対象メソッドが存在しない場合は、エラーが発生する。
				// オプションでエラー無視( SendMessageOptions.DontRequireReceiver )にしても、Awake 以下のタイミングだとワーニングが発生する。
//				child.SendMessage( "UpdateSpriteTransform", SendMessageOptions.DontRequireReceiver ) ;
			}

			//----------------------------------

			foreach( var childSpriteTransform in m_ChildSpriteTransforms )
			{
//				Debug.Log( "子に変化を通知する : " + childSpriteTransform.gameObject.name ) ;
				childSpriteTransform.UpdateSpriteTransform() ;
			}
		}

		// コンポーネントが見つからなかった子からさらにその子を検索する
		private void CollectSpriteTransformFromChildren( Transform root, List<SpriteTransform> childSpriteTransforms )
		{
			int i, l = root.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var child = root.GetChild( i ) ;

				if( child.TryGetComponent<SpriteTransform>( out var spriteTransform ) == true )
				{
					childSpriteTransforms.Add( spriteTransform ) ;
				}
				else
				{
					// SpriteTransform が存在しない
					// 再帰的に更に検索する
					CollectSpriteTransformFromChildren( child, m_ChildSpriteTransforms ) ;
				}

				// SendMessage() メソッドは、送信対象に対象メソッドが存在しない場合は、エラーが発生する。
				// オプションでエラー無視( SendMessageOptions.DontRequireReceiver )にしても、Awake 以下のタイミングだとワーニングが発生する。
//				child.SendMessage( "UpdateSpriteTransform", SendMessageOptions.DontRequireReceiver ) ;
			}

			// 全ての子にコンポーネントが有るか全ての子にコンポーネントが無いかで検索が終了する
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アンカーポジション
		/// </summary>
		public virtual Vector2 AnchorPosition
		{
			get
			{
				return new ( m_AnchorPositionX, m_AnchorPositionY ) ;
			}
			set
			{
				AnchorPositionX = value.x ;
				AnchorPositionY = value.y ;
			}
		}

		/// <summary>
		/// アンカーポジョンを取得する
		/// </summary>
		/// <returns></returns>
		public virtual Vector2 GetAnchorPosition()
			=> new ( m_AnchorPositionX, m_AnchorPositionY ) ;

		/// <summary>
		/// アンカーポジョンを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual void SetAnchorPosition( float x, float y )
		{
			AnchorPositionX = x ;
			AnchorPositionY = y ;
		}

		/// <summary>
		/// アンカーポジョンを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual void SetAnchorPosition( float x, float y, float z )
		{
			AnchorPositionX = x ;
			AnchorPositionY = y ;

			var t = transform.localPosition ;
			t.z = z ;
			transform.localPosition = t ;
		}

		//-----

		/// <summary>
		/// アンカーポジション
		/// </summary>
		public virtual Vector2 Position
		{
			get
			{
				return new ( m_AnchorPositionX, m_AnchorPositionY ) ;
			}
			set
			{
				AnchorPositionX = value.x ;
				AnchorPositionY = value.y ;
			}
		}

		/// <summary>
		/// アンカーポジョンを取得する
		/// </summary>
		/// <returns></returns>
		public virtual Vector2 GetPosition()
			=> new ( m_AnchorPositionX, m_AnchorPositionY ) ;


		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="p"></param>
		public virtual void SetPosition( Vector2 p )
		{
			AnchorPositionX = p.x ;
			AnchorPositionY = p.y ;
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual void SetPosition( float x, float y )
		{
			AnchorPositionX = x ;
			AnchorPositionY = y ;
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public virtual void SetPosition( float x, float y, float z )
		{
			AnchorPositionX = x ;
			AnchorPositionY = y ;

			var t = transform.localPosition ;
			t.z = z ;
			transform.localPosition = t ;
		}

		//---------------

		/// <summary>
		/// 奥行き(小さい方が手前に表示される)
		/// </summary>
		public float ZIndex
		{
			get
			{
				return transform.localPosition.z ;
			}
			set
			{
				var p = transform.localPosition ;
				p.z = value ;
				transform.localPosition = p ;
			}
		}

		/// <summary>
		/// Ｚ座標を設定する
		/// </summary>
		/// <param name="z"></param>
		public virtual void SetPositionZ( float z )
		{
			var p = transform.localPosition ;
			p.z = z ;
			transform.localPosition = p ;
		}

		//---------------

		/// <summary>
		/// サイズ
		/// </summary>
		public virtual Vector2   Size
		{
			get
			{
				return new Vector2( m_Width, m_Height ) ;
			}
			set
			{
				if( m_Width != value.x || m_Height != value.y )
				{
					m_Width  = value.x ;
					m_Height = value.y ;

					UpdateSpriteTransform() ;
				}
			}
		}

		/// <summary>
		/// サイズを取得する
		/// </summary>
		/// <returns></returns>
		public virtual Vector2 GetSize()
			=> new ( m_Width, m_Height ) ;


		/// <summary>
		/// サイズを設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual void SetSize( float x, float y )
		{	
			if( m_Width != x || m_Height != y )
			{
				m_Width  = x ;
				m_Height = y ;

				UpdateSpriteTransform() ;
			}
		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected Vector3 m_RotationEularAngles = Vector3.zero ;

		/// <summary>
		/// Ｘ軸回転
		/// </summary>
		public float Pitch
		{
			get
			{
				return m_RotationEularAngles.x ;
			}
			set
			{
				m_RotationEularAngles.x = value ;
				transform.localRotation = Quaternion.Euler( m_RotationEularAngles ) ;
			}
		}

		/// <summary>
		/// Ｙ軸回転
		/// </summary>
		public float Yaw
		{
			get
			{
				return m_RotationEularAngles.y ;
			}
			set
			{
				m_RotationEularAngles.y = value ;
				transform.localRotation = Quaternion.Euler( m_RotationEularAngles ) ;
			}
		}

		/// <summary>
		/// Ｚ軸回転
		/// </summary>
		public float Roll
		{
			get
			{
				return m_RotationEularAngles.z ;
			}
			set
			{
				m_RotationEularAngles.z = value ;
				transform.localRotation = Quaternion.Euler( m_RotationEularAngles ) ;
			}
		}

		/// <summary>
		/// 縮尺
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return transform.localScale ;
			}
			set
			{
				transform.localScale = new Vector3( value.x, value.y, transform.localScale.z ) ;
			}
		}

		/// <summary>
		/// 縮尺を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetScale( float x, float y )
		{
			transform.localScale = new Vector3( x, y, transform.localScale.z ) ;
		}

		//-------------------------------------------------------------------------------------------

		internal virtual void OnEnable()
		{
			transform.hideFlags = HideFlags.NotEditable ;
		}

		internal virtual void OnDisable()
		{
			transform.hideFlags = HideFlags.None ;
		}

		internal virtual void LateUpdate()
		{
//			Debug.Log( "LateUpdate : " + name ) ;
			if( transform.localPosition.x != m_LocalPositionX || transform.localPosition.y != m_LocalPositionY )
			{
				// AnchorPosition を先に更新する必要がある
				UpdateSpriteTransformAnchorPosition() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// レイヤーを設定する
		/// </summary>
		/// <param name="layer"></param>
		public void SetLayer( int layer )
		{
			gameObject.layer = layer ;
		}

		/// <summary>
		/// インスタンスを追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prefab"></param>
		/// <returns></returns>
		public T AddPrefab<T>( GameObject prefab ) where T : UnityEngine.Component
		{
			var go = Instantiate( prefab ) ;
			go.name = prefab.name + "(Clone)" ;

			go.transform.SetParent( transform, false ) ;

			return go.GetComponent<T>() ;			
		}

		//--------------------------------------------------------------------------------------------

		// 属しているスプライトスクリーン
		private SpriteCanvas		m_CachedSpriteCanvas ;

		/// <summary>
		/// 扱う準備が整っているかどうか
		/// </summary>
		public bool IsReady
		{
			get
			{
				return GetSpriteCanvas() != null ;
			}
		}

		/// <summary>
		/// スプライトスクリーンを取得する
		/// </summary>
		/// <returns></returns>
		public SpriteCanvas GetSpriteCanvas()
		{
			if( m_CachedSpriteCanvas == null )
			{
				m_CachedSpriteCanvas = transform.GetComponentInParent<SpriteCanvas>() ;
				if( m_CachedSpriteCanvas == null )
				{
					Debug.LogWarning( $"Not found SpriteScreen component. [{Path}]" ) ;
					return null ;
				}
			}

			return m_CachedSpriteCanvas ;
		}

		/// <summary>
		/// スプライトスクリーンのサイズ(解像度)
		/// </summary>
		public  Vector2		CanvasSize => GetCanvasSize() ;

		/// <summary>
		/// スプライトスクリーンのサイズ(解像度)を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCanvasSize()
		{
			var spriteCanvas = GetSpriteCanvas() ;
			if( spriteCanvas == null )
			{
				return Vector2.zero ;
			}

			return spriteCanvas.GetDeltaSize() ;
		}
	}
}
