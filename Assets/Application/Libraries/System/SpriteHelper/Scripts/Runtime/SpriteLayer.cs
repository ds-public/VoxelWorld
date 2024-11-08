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
	/// スプライト制御クラス  Version 2024/08/17
	/// </summary>
//	[ExecuteAlways]
	[DisallowMultipleComponent]
	public partial class SpriteLayer : SpriteTransform
	{
#if UNITY_EDITOR
		/// <summary>
		/// SpriteScreen を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteLayer", false, 22 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteLayer" )]					// ポップアップメニューから
		public static void CreateSpriteLayer()
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

			Undo.RecordObject( go, "Add a child SpriteLayer" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteLayer" ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteLayer>() ;
			component.SetDefault() ;	// 初期状態に設定する

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
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public void SetDefault()
		{
		}

		//-------------------------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		protected ViewportSizeTypes m_HorizontalViewportSizeType = ViewportSizeTypes.Fixed ;

		/// <summary>
		/// ビューポートのサイズタイプ(横方向)
		/// </summary>
		public ViewportSizeTypes HorizontalViewportSizeType
		{
			get
			{
				return m_HorizontalViewportSizeType ;
			}
			set
			{
				if( m_HorizontalViewportSizeType != value )
				{
					m_HorizontalViewportSizeType  = value ;

					// 更新
					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportPositionX = 0 ;

		/// <summary>
		/// 水平方向のビューポートの位置(固定)
		/// </summary>
		public float ViewportPositionX
		{
			get
			{
				return m_ViewportPositionX ;
			}
			set
			{
				if( m_ViewportPositionX != value )
				{
					m_ViewportPositionX  = value ;

					UpdateViewportSize() ;
				}
			}
		}


		[SerializeField][HideInInspector]
		protected float m_ViewportSizeX = 16 ;

		/// <summary>
		/// 水平方向のビューポートサイズ(固定)
		/// </summary>
		public float ViewportSizeX
		{
			get
			{
				return m_ViewportSizeX ;
			}
			set
			{
				if( m_ViewportSizeX != value )
				{
					m_ViewportSizeX  = value ;

					UpdateViewportSize() ;
				}
			}
		}

		//---------------

		[SerializeField][HideInInspector]
		protected ViewportSizeTypes m_VerticalViewportSizeType = ViewportSizeTypes.Fixed ;

		/// <summary>
		/// ビューポートのサイズタイプ(縦方向)
		/// </summary>
		public ViewportSizeTypes VerticalViewportSizeType
		{
			get
			{
				return m_VerticalViewportSizeType ;
			}
			set
			{
				if( m_VerticalViewportSizeType != value )
				{
					m_VerticalViewportSizeType  = value ;

					// 更新
					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportPositionY = 0 ;

		/// <summary>
		/// 垂直方向のビューポートの位置(固定)
		/// </summary>
		public float ViewportPositionY
		{
			get
			{
				return m_ViewportPositionY ;
			}
			set
			{
				if( m_ViewportPositionY != value )
				{
					m_ViewportPositionY  = value ;

					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportSizeY = 16 ;

		/// <summary>
		/// 垂直方向のビューポートサイズ(固定)
		/// </summary>
		public float ViewportSizeY
		{
			get
			{
				return m_ViewportSizeY ;
			}
			set
			{
				if( m_ViewportSizeX != value )
				{
					m_ViewportSizeY  = value ;

					UpdateViewportSize() ;
				}
			}
		}


		/// <summary>
		/// 表示クリップ領域のサイズを設定する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetViewportSize( float w, float h )
		{
			m_ViewportSizeX = w ;
			m_ViewportSizeY = h ;

			UpdateViewportSize() ;
		}

		//-----------------------------------

		[SerializeField][HideInInspector]
		protected float m_ViewportMarginL = 0 ;

		/// <summary>
		/// ビューポートのマージン(左)
		/// </summary>
		public float ViewportMarginL
		{
			get
			{
				return m_ViewportMarginL ;
			}
			set
			{
				if( m_ViewportMarginL != value )
				{
					m_ViewportMarginL = value ;

					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportMarginR = 0 ;

		/// <summary>
		/// ビューポートのマージン(右)
		/// </summary>
		public float ViewportMarginR
		{
			get
			{
				return m_ViewportMarginR ;
			}
			set
			{
				if( m_ViewportMarginR != value )
				{
					m_ViewportMarginR = value ;

					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportMarginT = 0 ;

		/// <summary>
		/// ビューポートのマージン(上)
		/// </summary>
		public float ViewportMarginT
		{
			get
			{
				return m_ViewportMarginT ;
			}
			set
			{
				if( m_ViewportMarginT != value )
				{
					m_ViewportMarginT = value ;

					UpdateViewportSize() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportMarginB = 0 ;

		/// <summary>
		/// ビューポートのマージン(下)
		/// </summary>
		public float ViewportMarginB
		{
			get
			{
				return m_ViewportMarginB ;
			}
			set
			{
				if( m_ViewportMarginB != value )
				{
					m_ViewportMarginB = value ;

					UpdateViewportSize() ;
				}
			}
		}

		// 実ピューポートサイズを更新する
		protected void UpdateViewportSize()
		{
			if( m_HorizontalViewportSizeType == ViewportSizeTypes.Fixed )
			{
				m_ViewportOffsetX		= m_ViewportPositionX ;
				m_ViewportDeltaSizeX	= m_ViewportSizeX ;
			}
			else
			if( m_HorizontalViewportSizeType == ViewportSizeTypes.Stretch )
			{
				m_ViewportOffsetX		= ( m_ViewportMarginL - m_ViewportMarginR ) * 0.5f ;
				m_ViewportDeltaSizeX	= m_ViewportSizeX - m_ViewportMarginL - m_ViewportMarginR ;
			}

			if( m_VerticalViewportSizeType == ViewportSizeTypes.Fixed )
			{
				m_ViewportOffsetY		= m_ViewportPositionY ;
				m_ViewportDeltaSizeY	= m_ViewportSizeY ;
			}
			else
			if( m_VerticalViewportSizeType == ViewportSizeTypes.Stretch )
			{
				m_ViewportOffsetY		= ( m_ViewportMarginB - m_ViewportMarginT ) * 0.5f ;
				m_ViewportDeltaSizeY	= m_ViewportSizeY - m_ViewportMarginB - m_ViewportMarginT ;
			}

			//----------------------------------------------------------
			// 範囲内外の変化を通知する

			UpdateAllViewClips() ;
		}

		[SerializeField][HideInInspector]
		protected float m_ViewportOffsetX ;

		public float ViewportOffsetX => m_ViewportOffsetX ;

		[SerializeField][HideInInspector]
		protected float m_ViewportOffsetY ;

		public float ViewportOffsetY => m_ViewportOffsetY ;

		[SerializeField][HideInInspector]
		protected float m_ViewportDeltaSizeX ;

		public float ViewportDeltaSizeX => m_ViewportDeltaSizeX ;

		[SerializeField][HideInInspector]
		protected float m_ViewportDeltaSizeY ;

		public float ViewportDeltaSizeY => m_ViewportDeltaSizeY ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マップチップ情報
		/// </summary>
		[Serializable]
		public class MapChip
		{
			public System.Object	Data ;
			public GameObject		Node ;
		}


		[Serializable]
		public class LayerDescriptor
		{
			public SpriteTransform LayerTransform ;

			/// <summary>
			/// マップ位置
			/// </summary>
			public Vector2 MapPosition
			{
				get
				{
					if( LayerTransform == null )
					{
						return Vector2.zero ;
					}

					return - LayerTransform.AnchorPosition ;
				}
				set
				{
					if( LayerTransform == null )
					{
						return ;
					}
					LayerTransform.AnchorPosition = - ( value * MapPositionRatio ) ;
				}
			}

			/// <summary>
			/// マップ位置の係数
			/// </summary>
			public Vector2 MapPositionRatio = Vector2.one ;

			/// <summary>
			/// マップチップのサイズ
			/// </summary>
			public Vector2 MapChipSize ;

			/// <summary>
			/// マップチップのオフセット
			/// </summary>
			public Vector2 MapChipOffset ;

			//----------------------------------------------------------

			// マップ
			public Dictionary<( int X, int Y ),MapChip> MapData = new () ;

			public int MapSizeX ;
			public int MapSizeY ;

			public int MapAreaL ;
			public int MapAreaR ;
			public int MapAreaT ;
			public int MapAreaB ;
		}

		[SerializeField][HideInInspector]
		protected List<LayerDescriptor> m_Layers ;


		/// <summary>
		/// マップ(レイヤー)の表示位置を取得する
		/// </summary>
		/// <param name="layerIndex"></param>
		/// <returns></returns>
		public Vector2 GetMapPosition( int layerIndex )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return Vector2.zero ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			return layer.MapPosition ;
		}

		/// <summary>
		/// 表示クリップ領域の中心座標を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetMapPosition( int layerIndex, float x, float y )
		{
			SetMapPosition( layerIndex, new Vector2( x, y ) ) ;
		}

		/// <summary>
		/// 表示クリップ領域の中心座標を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetMapPosition( int layerIndex, Vector2 mapPosition )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			var lt = layer.LayerTransform ;

			lt.HorizontalAnchorType	= HorizontalAnchorTypes.Center ;
			lt.VerticalAnchorType	= VerticalAnchorTypes.Middle ;

			layer.MapPosition = mapPosition * layer.MapPositionRatio ;

			//----------------------------------------------------------
			// 範囲内外の変化を通知する

			UpdateViewClip( layerIndex ) ;
		}

		/// <summary>
		/// 全てのマップ位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetAllMapPositions( float x, float y )
		{
			if( m_Layers == null || m_Layers.Count <= 0 )
			{
				return ;
			}

			//----------------------------------

			int layerIndex = 0, layerCount = m_Layers.Count ;
			for( layerIndex  = 0 ; layerIndex <  layerCount ; layerIndex ++ )
			{
				SetMapPosition( layerIndex, new Vector2( x, y ) ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// マップチップのサイズを設定する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetMapChipSize( int layerIndex, float w, float h )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			layer.MapChipSize = new Vector2( w, h ) ;

			//----------------------------------------------------------
			// 範囲内外の変化を通知する

			UpdateViewClip( layerIndex ) ;
		}

		/// <summary>
		/// マップチップのサイズを設定する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetMapChipOffset( int layerIndex, float x, float y )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			layer.MapChipOffset = new Vector2( x, y ) ;

			//----------------------------------------------------------
			// 範囲内外の変化を通知する

			UpdateViewClip( layerIndex ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// マップデータを設定する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="map"></param>
		public void SetMapData<T>( int layerIndex, Dictionary<( int X, int Y ),T> mapChips )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			int ml = +1000000, mr = -1000000, mb = +1000000, mt = -1000000 ;

			foreach( var mapChip in mapChips )
			{
				var gridKey = mapChip.Key ;

				if( layer.MapData.ContainsKey( gridKey ) == false )
				{
					// 新規追加
					layer.MapData.Add( gridKey, new MapChip(){ Data = mapChip.Value } ) ;
				}
				else
				{
					// 既存更新
					layer.MapData[ gridKey ].Data = mapChip.Value ;
				}

				int x = gridKey.X ;
				int y = gridKey.Y ;

				if( x <  ml )
				{
					ml  = x ;
				}
				if( x >  mr )
				{
					mr  = x ;
				}
				if( y <  mb )
				{
					mb  = y ;
				}
				if( y >  mt )
				{
					mt  = y ;
				}
			}

			layer.MapSizeX = mr - ml + 1 ;
			layer.MapSizeY = mt - mb + 1 ;

			layer.MapAreaL = ml ;
			layer.MapAreaR = mr ;
			layer.MapAreaB = mb ;
			layer.MapAreaT = mt ;

			UpdateViewClip( layerIndex ) ;
		}

		/// <summary>
		/// マップデータを設定する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="map"></param>
		public void SetMapData<T>( int layerIndex, T[,] mapChips )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			int lx = mapChips.GetLength( 0 ) ;
			int ly = mapChips.GetLength( 1 ) ;

			int gx, gy ;

			int ml = +1000000, mr = -1000000, mb = +1000000, mt = -1000000 ;

			for( gy  = 0 ; gy <  ly ; gy ++ )
			{
				for( gx  = 0 ; gx <  lx ; gx ++ )
				{
					var gridKey = ( gx, gy ) ;

					if( layer.MapData.ContainsKey( gridKey ) == false )
					{
						// 新規追加
						layer.MapData.Add( gridKey, new MapChip(){ Data = mapChips[ gy, gx ] } ) ;
					}
					else
					{
						// 既存更新
						layer.MapData[ gridKey ].Data = mapChips[ gy, gx ] ;
					}


					int x = gx ;
					int y = gy ;

					if( x <  ml )
					{
						ml  = x ;
					}
					if( x >  mr )
					{
						mr  = x ;
					}
					if( y <  mb )
					{
						mb  = y ;
					}
					if( y >  mt )
					{
						mt  = y ;
					}
				}
			}

			layer.MapSizeX = mr - ml + 1 ;
			layer.MapSizeY = mt - mb + 1 ;

			layer.MapAreaL = ml ;
			layer.MapAreaR = mr ;
			layer.MapAreaB = mb ;
			layer.MapAreaT = mt ;

			UpdateViewClip( layerIndex ) ;
		}

		/// <summary>
		/// 移動させる
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Move( int layerIndex, float x, float y )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

//			var layer = m_Layers[ layerIndex ] ;

			//----------------------------------

			var mapPosition = GetMapPosition( layerIndex ) ;

			mapPosition.x += ( x * Time.deltaTime ) ;
			mapPosition.y += ( y * Time.deltaTime ) ;

			SetMapPosition( layerIndex, mapPosition ) ;

			UpdateViewClip( layerIndex ) ;
		}

/*
		internal override void LateUpdate()
		{
			base.LateUpdate() ;

			UpdateViewClip() ;
		}
*/
		//-------------------------------------------------------------------------------------------

		protected Func<SpriteLayer,int,int,int,System.Object,GameObject> m_OnViewportInner ;

		/// <summary>
		/// 新しいマップチップの展開が必要な際に呼び出されるコールバックを登録する
		/// </summary>
		/// <param name="onViewClipEnter"></param>
		public void SetOnViewportInner( Func<SpriteLayer,int,int,int,System.Object,GameObject> onViewportInner )
		{
			m_OnViewportInner = onViewportInner ;
		}

		protected Func<SpriteLayer,int,int,int,System.Object,MapChipDestructionTypes> m_OnViewportOuter ;

		/// <summary>
		/// 新しいマップチップが破棄された際に呼び出されるコールバックを登録する
		/// </summary>
		/// <param name="onViewClipEnter"></param>
		public void SetOnViewportOuter( Func<SpriteLayer,int,int,int,System.Object,MapChipDestructionTypes> onViewportOuter )
		{
			m_OnViewportOuter = onViewportOuter ;
		}

		protected Action<SpriteLayer,int,float,float,float,float> m_OnViewportMoved ;

		/// <summary>
		/// 表示クリップ領域が変化した際に呼び出されるコールバックを登録する
		/// </summary>
		/// <param name="onViewClipMoved"></param>
		public void SetOnViewportMoved( Action<SpriteLayer,int,float,float,float,float> onViewClipMoved )
		{
			m_OnViewportMoved = onViewClipMoved ;
		}

		//-------------------------------------------------------------------------------------------

		// 全ての表示クリップを更新する
		protected void UpdateAllViewClips()
		{
			if( m_Layers == null || m_Layers.Count == 0 )
			{
				return ;
			}

			int i, l = m_Layers.Count ;
			for( i = 0 ; i <  l ; i ++ )
			{
				UpdateViewClip( i ) ;
			}
		}

		// 表示クリップ領域を更新する
		protected void UpdateViewClip( int layerIndex )
		{
			if( m_Layers == null || m_Layers.Count <= layerIndex || m_Layers[ layerIndex ] == null )
			{
				return ;
			}

			var layer = m_Layers[ layerIndex ] ;

			var mapPosition = layer.MapPosition ;

			//----------------------------------

			// 表示クリップ領域の範囲を算出する

			float vcX = mapPosition.x ;
			float vcY = mapPosition.y ;

			float vcW = m_ViewportDeltaSizeX * 0.5f ;
			float vcH = m_ViewportDeltaSizeY * 0.5f ;

			float vcMinX = vcX - vcW + m_ViewportOffsetX ;
			float vcMaxX = vcX + vcW + m_ViewportOffsetX ;

			float vcMinY = vcY - vcH + m_ViewportOffsetY ;
			float vcMaxY = vcY + vcH + m_ViewportOffsetY ;

			//----------------------------------

			// 一部でも表示クリップ領域に含まれるマップチップは、新規生成か現状維持

			// 完全に表示クリップ外になってしまったマップチップは、破棄

			float mcMinX, mcMaxX, mcMinY, mcMaxY ;

			float mcSizeW = layer.MapChipSize.x ;
			float mcSizeH = layer.MapChipSize.y ;

			float mcHalfSizeW = mcSizeW * 0.5f ;
			float mcHalfSizeH = mcSizeH * 0.5f ;

			float mcOffsetX = layer.MapChipOffset.x ;
			float mcOffsetY = layer.MapChipOffset.y ;

			int gridX, gridY ;

			float mcCenterX, mcCenterY ;


			foreach( var mapChip in layer.MapData )
			{
				gridX = mapChip.Key.X ;
				gridY = mapChip.Key.Y ;

				mcCenterX = gridX * mcSizeW + mcOffsetX ;
				mcCenterY = gridY * mcSizeH + mcOffsetY ;

				mcMinX = mcCenterX - mcHalfSizeW ;
				mcMaxX = mcCenterX + mcHalfSizeW ;

				mcMinY = mcCenterY - mcHalfSizeH ;
				mcMaxY = mcCenterY + mcHalfSizeH ;

				//---------------------------------

				if( mcMaxX <= vcMinX || vcMaxX <= mcMinX || mcMaxY <= vcMinY || vcMaxY <= mcMinY )
				{
					// 外に出たので破棄する必要がある

					if( mapChip.Value.Node != null )
					{
						// 破棄
						MapChipDestructionTypes mapChipDestructionType = MapChipDestructionTypes.Destroy ;

						if( m_OnViewportOuter != null )
						{
							mapChipDestructionType = m_OnViewportOuter( this, layerIndex, gridX, gridY, mapChip.Value.Data ) ;
						}

						if( mapChipDestructionType == MapChipDestructionTypes.Destroy )
						{
							Destroy( mapChip.Value.Node ) ;
							mapChip.Value.Node = null ;
						}
						else
						{
							mapChip.Value.Node.SetActive( false ) ;
						}
					}
				}
				else
				{
					// 全てまたは一部が表示クリップ領域内にあるので生成か維持

					if( mapChip.Value.Node == null )
					{
						// 生成

						mapChip.Value.Node = m_OnViewportInner?.Invoke( this, layerIndex, gridX, gridY, mapChip.Value.Data ) ;

						if( mapChip.Value.Node != null )
						{
							mapChip.Value.Node.transform.SetParent( layer.LayerTransform.transform, false ) ;

							float x = gridX * mcSizeW + mcOffsetX ;
							float y = gridY * mcSizeH + mcOffsetY ;
							mapChip.Value.Node.transform.localPosition = new Vector2( x, y ) ;
							mapChip.Value.Node.SetActive( true ) ;
						}			
					}
					else
					{
						mapChip.Value.Node.SetActive( true ) ;
					}
				}
			}

			// エンティティの生成と破棄
			m_OnViewportMoved?.Invoke( this, layerIndex, vcX, vcY, vcW, vcH ) ;
		}
	}
}
