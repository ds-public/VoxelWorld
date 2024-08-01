using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
//	[RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
	[RequireComponent(typeof(ScrollRectWrapper))]
	[DefaultExecutionOrder( 2 )]	// ScrollRect の表示位置変化後にアイテム展開を行いたいのでわずかに遅らせる
	public class UIListView : UIScrollView
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIListView/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIListView[] views = UIEditorUtility.FindComponents<UIListView>
			(
				"Assets/Application",
				( _ ) =>
				{
					_.m_SnapThreshold = _.snapThreshold ;
					_.m_SnapTime = _.snapTime ;

					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIListViewの数:" + c ) ;
		}
#endif
*/


		/// <summary>
		/// Item(テンプレート)のインスタンス
		/// </summary>
		public UIView Item
		{
			get
			{
				return m_Item ;
			}
			set
			{
				if( m_Item != value )
				{
					m_Item  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected UIView	m_Item ;



		/// <summary>
		/// Itemが空の時に表示する表示物のインスタンス
		/// </summary>
		public UIView ItemEmptyPresentment
		{
			get
			{
				return m_ItemEmptyPresentment ;
			}
			set
			{
				if( m_ItemEmptyPresentment != value )
				{
					m_ItemEmptyPresentment  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		protected UIView	m_ItemEmptyPresentment ;

		/// <summary>
		/// リストビュー表示に使用するアイテム数
		/// </summary>
		public int		WorkingItemCount
		{
			get
			{
				return m_WorkingItemCount ;
			}
			set
			{
				if( m_WorkingItemCount != value && value >=  4 )
				{
					m_WorkingItemCount  = value ;
				}
				m_ItemListDirty = true ;	// 設定したら値が同じであっても必ず更新する
			}
		}

		[SerializeField][HideInInspector]
		private int		m_WorkingItemCount = 10 ;

		/// <summary>
		/// ビューポートの外側部分の表示も有効にするかどうか
		/// </summary>
		public	bool	MarginEnabled
		{
			get
			{
				return m_MarginEnabled ;
			}
			set
			{
				if( m_MarginEnabled != value )
				{
					m_MarginEnabled  = value ;
					m_ItemListDirty  = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_MarginEnabled = false ;


		/// <summary>
		/// スクロールは有限か無限か
		/// </summary>
		public bool		Infinity
		{
			get
			{
				return m_Infinity ;
			}
			set
			{
				if( m_Infinity != value )
				{
					m_Infinity  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_Infinity = false ;	// 有限

		/// <summary>
		/// 有限の場合のアイテムの最大数
		/// </summary>
		public int		ItemCount
		{
			get
			{
				return m_ItemCount ;
			}
			set
			{
				m_ItemCount  = value ;
				m_ItemListDirty = true ;
			}
		}

		[SerializeField][HideInInspector]
		private int		m_ItemCount = 20 ;

		/// <summary>
		/// スナップの有無
		/// </summary>
		public bool		Snap
		{
			get
			{
				return m_Snap ;
			}
			set
			{
				if( value != m_Snap )
				{
					m_Snap = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_Snap = false ;

		/// <summary>
		/// スナップの実行判定速度
		/// </summary>
		public float	SnapThreshold{ get{ return m_SnapThreshold ; } set{ m_SnapThreshold = value ; } }

		[SerializeField][HideInInspector]
		private float	m_SnapThreshold = 100.0f ;


		/// <summary>
		/// スナップの速度
		/// </summary>
		public float	SnapTime{ get{ return m_SnapTime ; } set{ m_SnapTime = value ; } }

		[SerializeField][HideInInspector]
		private float	m_SnapTime = 0.25f ;


		public enum SnapAnchorTypes
		{
			Haed = 0,
			Tail = 1,
		}

		/// <summary>
		/// スナップの位置
		/// </summary>
		public SnapAnchorTypes SnapAnchorType
		{
			get
			{
				return m_SnapAnchorType ;
			}
			set
			{
				if( m_SnapAnchorType != value )
				{
					m_SnapAnchorType  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private SnapAnchorTypes	m_SnapAnchorType = SnapAnchorTypes.Haed ;

		//------------------------------------------------

		//-----------------------------------------------------------
		// リストビュー複製時に再びStart()が呼ばれたタイミングでリストが更新されないようシリアライズ対象にする

		// 内部処理変数系

		[Serializable]
		public class ItemData
		{
			public int						Index ;
			public UIView					View ;
			public UnityEngine.Component	Code ;
			public float					Size ;

			public ItemData( int index, UIView view, UnityEngine.Component code, float size )
			{
				Index	= index ;
				View	= view ;
				Code	= code ;
				Size	= size ;
			}
		}

		/// <summary>
		/// リストビューアイテムの処理コンポーネントタイプ
		/// </summary>
		public Type ItemComponentType
		{
			get
			{
				if( m_ItemComponentType != null )
				{
					return m_ItemComponentType ;
				}

				if( m_ItemComponent != null )
				{
					return m_ItemComponent.GetType() ;
				}

				return null ;
			}
			set
			{
				m_ItemComponentType	= value ;
				m_ItemComponent		= null ;
			}
		}

		private Type	m_ItemComponentType = null ;

		/// <summary>
		/// リストビューアイテムの処理コンポーネントタイプ
		/// </summary>
		public MonoBehaviour ItemComponent
		{
			get
			{
				return m_ItemComponent ;
			}
			set
			{
				m_ItemComponent = value ;
			}
		}
		
		/// <summary>
		/// リストビューアイテムの処理コンポーネントを取得する
		/// </summary>
		public T GetItemComponent<T>() where T : MonoBehaviour
		{
			return m_Item == null ? null : m_Item.GetComponent<T>() ;
		}

		[SerializeField][HideInInspector]
		protected MonoBehaviour m_ItemComponent ;

		// アイテムリスト
		[SerializeField][HideInInspector]
		private List<ItemData> m_ItemList ;

		[SerializeField][HideInInspector]
		private int		m_CurrentItemIndex = 0 ;

		[SerializeField][HideInInspector]
		private float	m_ItemUpperPosition = 0 ;

		[SerializeField][HideInInspector]
		private float	m_ItemLowerPosition = 0 ;

		[SerializeField][HideInInspector]
		private bool	m_ItemListDirty = true ;

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private float	m_CanvasLength = 0 ;

		private float?	m_DirtyPosition		= null ;
		private int?	m_DirtyIndex		= null ;
		private int?	m_DirtyItemCount	= null ;

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		override protected void OnBuild( string option = "" )
		{
			ScrollRectWrapper scrollRect = CScrollRect != null ? CScrollRect : gameObject.AddComponent<ScrollRectWrapper>() ;

			Image image = CImage ;

			//-------------------------------------

			BuildTypes buildType = BuildTypes.Unknown ;
			DirectionTypes directionType = DirectionTypes.Both ;

			if( option.ToLower() == "h" )
			{
				buildType = BuildTypes.ListView ;
				directionType = DirectionTypes.Horizontal ;
			}
			else
			if( option.ToLower() == "v" )
			{
				buildType = BuildTypes.ListView ;
				directionType = DirectionTypes.Vertical ;
			}

			m_BuildType		= buildType ;		// 後から変更は出来ない
			m_DirectionType	= directionType ;	// 後から変更は出来ない

			// 基本的な大きさを設定
			float s = 100.0f ;
			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				if( size.x <= size.y )
				{
					s = size.x ;
				}
				else
				{
					s = size.y ;
				}
				s *= 0.5f ;
			}
				
			ResetRectTransform() ;

			// 方向を設定
			if( directionType == DirectionTypes.Horizontal )
			{
				scrollRect.horizontal = true ;
				scrollRect.vertical   = false ;
				SetSize( s, s * 0.75f ) ;
			}
			else
			if( directionType == DirectionTypes.Vertical )
			{
				scrollRect.horizontal = false ;
				scrollRect.vertical   = true ;
				SetSize( s * 0.75f, s ) ;
			}

			m_MarginEnabled = false ;


			// Mask 等を設定する Viewport を設定(スクロールバーは表示したいので ScrollRect と Mask は別の階層に分ける)
			m_Viewport = AddView<UIImage>( "Viewport" ) ;
			m_Viewport.SetAnchorToStretch() ;
			m_Viewport.SetMargin( 0, 0, 0, 0 ) ;
			m_Viewport.SetPivot( 0, 1 ) ;
			m_Viewport.Pz = -1 ;
			scrollRect.viewport = m_Viewport.GetRectTransform() ;

			// マスクは CanvasRenderer と 何等かの表示を伴うコンポートと一緒でなければ使用できない
//			Mask mask = m_Viewport.gameObject.AddComponent<Mask>() ;
//			mask.showMaskGraphic = false ;
			m_Viewport.gameObject.AddComponent<RectMask2D>() ;
			m_Viewport.IsAlphaMaskWindow = true ;
//			m_Viewport.color = new Color( 0, 0, 0, 0 ) ;
			m_Viewport.CImage.enabled = false ;

			if( IsCanvasOverlay == true )
			{
				m_Viewport.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
			
			// Content を追加する
			var content = CreateContent( m_Viewport, directionType ) ;
			if( content != null )
			{
				scrollRect.content = content.GetRectTransform() ;
				m_Content = content ;
				
				// Item(テンプレート)を追加する
				m_Item = CreateTemplateItem( content, directionType ) ;

				content.Pz = -1 ;
			}

			// 自身の Image
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			image.color = new Color32( 255, 255, 255,  63 ) ;
			image.type = Image.Type.Sliced ;

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
		}

		// デフォルトの Content を生成する
		private UIView CreateContent( UIView parent, DirectionTypes directionType )
		{
			UIView content ;

			content = parent.AddView<UIView>( "Content" ) ;

			if( directionType == DirectionTypes.Horizontal )
			{
				// 横スクロール
				content.SetAnchorToLeftStretch() ;
				content.SetPivot( 0.0f, 0.5f ) ;
			}

			if( directionType == DirectionTypes.Vertical  )
			{
				// 縦スクロール
				content.SetAnchorToStretchTop() ;
				content.SetPivot( 0.5f, 1.0f ) ;
			}

			return content ;
		}

		// テンプレートのアイテムを生成する
		private UIView CreateTemplateItem( UIView parent, DirectionTypes directionType )
		{
			UIImage item = parent.AddView<UIImage>( "Item(Template)" ) ;

			// Image
			item.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			item.Color = Color.white ;
			item.Type = Image.Type.Sliced ;

			// 横スクロール
			if( directionType == DirectionTypes.Horizontal )
			{
				item.SetAnchorToLeftStretch() ;
				item.SetPivot( 0.0f, 0.5f ) ;
				item.Width = this.Width * 0.2f ;
			}

			// 縦スクロール
			if( directionType == DirectionTypes.Vertical  )
			{
				item.SetAnchorToStretchTop() ;
				item.SetPivot( 0.5f, 1.0f ) ;
				item.Height = this.Height * 0.2f ;
			}

			if( IsCanvasOverlay == true )
			{
				item.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return item ;
		}

		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( Application.isPlaying == true )
			{
				m_ItemList ??= new List<ItemData>() ;

				// テンプレートのアイテムを無効化する(Awakeのタイミングで行わないとまずい)
				if( m_Item != null )
				{
					m_Item.SetActive( false ) ;
				}
			}
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			// ContentSize を設定後に呼ぶこと
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// キャンバスの基本サイズを取得する
				m_CanvasLength = GetCanvasLength() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}

//				CScrollRect.enabled = true ;
//				CScrollRect.velocity = Vector2.zero ;
			}
		}

		/// <summary>
		/// 派生クラスの Update
		/// </summary>
		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				// キャンバスの基本サイズを取得する
				m_CanvasLength = GetCanvasLength() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}
			}
		}

		// リストにアップデートをかける
		private void UpdateItemList()
		{
			if( m_DirtyIndex != null )
			{
				SetContentIndex( m_DirtyIndex.Value, m_DirtyItemCount.Value, true ) ;
			}
			else
			if( m_DirtyPosition != null )
			{
				SetContentPosition( m_DirtyPosition.Value, m_DirtyItemCount.Value, true ) ;
			}
			else
			{
				// リフレッシュだけは例外的に即時更新も可能にする
				SetContentPosition( base.ContentPosition ) ;
			}

			//--------------------------------

			// アイテムが空の時の表示物を表示する
			if( m_ItemEmptyPresentment != null )
			{
				m_ItemEmptyPresentment.SetActive( m_ItemCount <= 0 ) ;
			}
		}

		protected override void OnLateUpdate()
		{
			if( Application.isPlaying == true )
			{
				ProcessSnap() ;

				ProcessItem() ;

				// このメソッドの最後に呼ぶこと
				base.OnLateUpdate() ;
			}
		}

		// スナップ処理中かどうか
		protected override bool IsSnapping()
		{
			if( m_Snap == false || m_Snap == true && m_Snapping <  0 )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// コンテントの現在位置を取得する
		/// </summary>
		public override float ContentPosition
		{
			get
			{
				return base.ContentPosition ;
			}
			set
			{
				// 横スクロール(強制設定)
				SetContentPosition( value ) ;
			}
		}

		/// <summary>
		/// コンテントの現在位置を取得する
		/// </summary>
		protected override float ContentPositionPure
		{
			get
			{
				return base.ContentPosition ;
			}
			set
			{
				// 横スクロール(強制設定)
				base .ContentPosition = value ;

				// このフレームではスナップ処理を行わない
				m_Snapping = 0 ;
			}
		}

		/// <summary>
		/// 現在のコンテントの位置を取得する
		/// </summary>
		/// <returns></returns>
		public float GetContentPosition()
		{
			return base.ContentPosition ;
		}

		/// <summary>
		/// 現在のコンテントの取りうる範囲内での位置をシユ得する
		/// </summary>
		/// <returns></returns>
		public float GetContentPositionOfValidRange()
		{
			float contentPosition = base.ContentPosition ;

			if( contentPosition <  0 || ContentSize <= ViewSize )
			{
				contentPosition  = 0 ;
			}
			else
			if( ( ContentSize - contentPosition ) <  ViewSize )
			{
				contentPosition = ContentSize - ViewSize ;
			}

			return contentPosition ;
		}

		/// <summary>
		/// 値で指定した位置に座標を設定する
		/// </summary>
		/// <param name="position"></param>
		public float SetContentPosition( float position, int itemCount = -1, bool isUpdating = true )
		{
			// 注意：こちらでは MarginEnabled は考慮しない。LateUpdate の ProcessItem() で自動補完に任せる
			if( isUpdating == false )
			{
				m_DirtyIndex		= null ;
				m_DirtyPosition		= position ;
				m_DirtyItemCount	= itemCount ;

				m_ItemListDirty		= true ;

				return 0 ;
			}

			m_DirtyIndex			= null ;
			m_DirtyPosition			= null ;
			m_DirtyItemCount		= null ;

			//----------------------------------------------------------

			float basePosition = position ;

			if( m_Infinity == false && basePosition <  0 )
			{
				// 有限スクロールの場合は負値は指定できない
				basePosition = 0 ;
			}

			int index = -1 ;

			int i, l ;

			// この座標がどこに含まれるか検査する
			if( basePosition >= 0 )
			{
				// 正値
				if( Infinity == false )
				{
					// 有限
					if( itemCount >= 0 )
					{
						l = itemCount ;
					}
					else
					{
						l = m_ItemCount ;
					}
				}
				else
				{
					// 無限
					l =  65536 ;
				}

				float p0 = 0 ;
				float p1 ;

				if( m_Snap == true && m_SnapAnchorType == SnapAnchorTypes.Tail )
				{
					// 最後基準の場合は例外的な処理を行う
					basePosition += ViewSize ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						p1 = p0 + OnItemUpdatedInner( i, null, null ) ;
	
						if( basePosition >= p0 && basePosition <  p1 )
						{
							// ここに確定
							index = i ;
							break ;
						}

						p0 = p1 ;
					}

					if( i >= l )
					{
						// 該当無しの場合は最後とする
						index = l - 1 ;
					}
				}
				else
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						p1 = p0 + OnItemUpdatedInner( i, null, null ) ;
	
						if( basePosition >= p0 && basePosition <  p1 )
						{
							// ここに確定
							index = i ;
							break ;
						}

						p0 = p1 ;
					}

					if( i >= l )
					{
						// 該当無しの場合は最後とする
						index = l - 1 ;
					}
				}
			}
			else
			{
				// 負値(無限限定)

				l = -65536 ;

				float p0 ;
				float p1 = 0 ;
				for( i  = -1 ; i >  ( l - 1 ) ; i -- )
				{
					p0 = p1 - OnItemUpdatedInner( i, null, null ) ;

					if( basePosition >= p0 && basePosition <  p1 )
					{
						// ここに確定
						index = i ;
						break ;
					}

					p1 = p0 ;
				}
			}

			// 指定のインデックスに飛ばす
			SetContentIndex( index, itemCount ) ;

			if( Infinity == false )
			{
				// 有限の場合の座標補正を行う

				float max = ContentSize - ViewSize ;
				if( max <  0 )
				{
					max  = 0 ;
				}

				if( position <  0 )
				{
					position  = 0 ;
				}
				else
				if( position >  max )
				{
					position  = max ;
				}
			}

			base.ContentPosition = position ;

			// 重要
			m_Snapping = 0 ;	// スナップ処理中であれば一旦キャンセルする(+1=スナップ処理中・0=現フレームではスナップを処理しない・-1=スナップ処理は終了している)

			if( m_Snap == true )
			{
				float snapToPosition = GetSnapPosition() ;
				if( base.ContentPosition == snapToPosition )
				{
					m_Snapping = -1 ;
				}
			}

			m_ContentDirty = true ;

			return ContentSize ;
		}

		/// <summary>
		/// リストビューアイテムの処理コンポーネントの型を設定する
		/// </summary>
		/// <param name="itemComponentType"></param>
		public void SetItemComponentType( Type itemComponentType )
		{
			ItemComponentType = itemComponentType ;
		}

		/// <summary>
		/// 指定したインデックスを基準としてアイテム群を表示させる
		/// </summary>
		/// <param name="index"></param>
		/// <param name="itemCount"></param>
		/// <param name="isUpdating"></param>
		/// <returns></returns>
		public float SetContentIndex( int index, int itemCount = -1, bool isUpdating = true )
		{
			// 注意：こちらでは MarginEnabled は考慮しない。LateUpdate の ProcessItem() で自動補完に任せる
			if( isUpdating == false )
			{
				m_DirtyIndex		= index ;
				m_DirtyPosition		= null ;
				m_DirtyItemCount	= itemCount ;

				m_ItemListDirty		= true ;

				return 0 ;
			}

			m_DirtyIndex			= null ;
			m_DirtyPosition			= null ;
			m_DirtyItemCount		= null ;

			m_ItemListDirty  = false ;

			//----------------------------------------------------------

			// スクロール中であれば止める
			CScrollRect.velocity = Vector2.zero ;

			if( Content == null || m_Item == null )
			{
				return 0 ;
			}

//			Debug.LogWarning( "アイテムリストを更新:" + name ) ;
//			float tt = Time.realtimeSinceStartup ;

			if( itemCount <  0 )
			{
				// アイテム数は継続する
				itemCount  = m_ItemCount ;
			}
			else
			if( m_ItemCount != itemCount )
			{
				// アイテム数も変化する
				m_ItemCount  = itemCount ;
			}
			
			if( Infinity == false )
			{
				// 有限スクロール
				if( index <  0 || index >= itemCount )
				{
					index  = itemCount - 1 ;	// 最後の位置にする
				}
			}

			//------------------------------------------------------------------

			int i, l ;
#if false
			if( m_ItemList.Count >  0 )
			{
				// 既に展開済みのアイテムを全て削除する
				l = m_ItemList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_ItemList[ i ].view != null )
					{
						DestroyImmediate( m_ItemList[ i ].view.gameObject ) ;
					}
				}

				m_ItemList.Clear() ;
			}
#endif
			//----------------------------------------------------------

			// コンテントサイズは最初に設定しておく必要がある

			if( Infinity == false )
			{
				// 有限

				// コンテントのサイズを計算する
				float contentSize = 0 ;
				for( i  = 0 ; i <  itemCount ; i ++ )
				{
					// 生成コールバックの呼び出し
					contentSize += OnItemUpdatedInner( i, null, null ) ;
				}

				if( contentSize <  ViewSize )
				{
					contentSize  = ViewSize ;
				}

				ContentSize = contentSize ;

				if( CScrollRect.movementType == ScrollRect.MovementType.Unrestricted )
				{
					CScrollRect.movementType  = ScrollRect.MovementType.Elastic ;		// バウンドする
				}
			}
			else
			{
				// 無限

				// コンテントのサイズ
				ContentSize = ViewSize ;	// 無限スクロールの場合はバウンドしないのでこの値自体はどうでもよい
				
				// スクロールタイプを設定
				CScrollRect.movementType = ScrollRect.MovementType.Unrestricted ;	// バウンドしない
			}

			//------------------------------------------------------------------

			float viewOffset = 0 ;
			float itemOffset = 0 ;
			float snapOffset = 0 ;	// スナップ有効且つアンカーが最後の場合のズレ補正値

			if( ( m_Infinity == false && ContentSize >  ViewSize ) || m_Infinity == true )
			{
				if( m_Snap == true && m_SnapAnchorType == SnapAnchorTypes.Tail )
				{
					// 最後基準である場合はインデックスは画面最後のものとみなしオフセット値の開始値もずれる

					if( m_Infinity == false )
					{
						// 有限
						l = 0 ;
					}
					else
					{
						// 無限
						l = -65536 ;
					}

					float contentSize = 0 ;
					for( i  = index ; i >= l ; i -- )
					{
						contentSize += OnItemUpdatedInner( i, null, null ) ;
						if( contentSize >= ViewSize )
						{
							break ;	// 足りた
						}
					}

					if( contentSize <  ViewSize )
					{
						// 足りない(足りない状況が起こり得るのは有限の場合のみ
						for( i  = ( index + 1 ) ; i <  m_ItemCount ; i ++ )
						{
							contentSize += OnItemUpdatedInner( i, null, null ) ;
							if( contentSize >=  ViewSize )
							{
								break ;	// 足りた
							}
						}

						index = 0 ;	// インデックスは０始まりで確定
					}
					else
					{
						index = i ;
					}

					snapOffset = contentSize - ViewSize ;	// ズレ
				}

				// オフセット値を調整する
				if( index >  0 )
				{
					// 正方向
					for( i  = 0 ; i <  index ; i ++ )
					{
						// 幅だけ取得する
						itemOffset += OnItemUpdatedInner( i, null, null ) ;
					}
				}
				else
				if( index <  0 )
				{
					// 負方向
					for( i  = -1 ; i >  ( index - 1 ) ; i -- )
					{
						// 幅だけ取得する
						itemOffset -= OnItemUpdatedInner( i, null, null ) ;
					}
				}

				viewOffset = itemOffset ;

				// 有限の場合は最後の方に行き過ぎると表示がおかしくなるのでスナップで限界までに留める
				if( Infinity == false )
				{
					// 有限
					if( ( ContentSize - itemOffset ) <  ViewSize )
					{
						// 下に空白が出来てしまうので余力が出来るまでインデックスを引き下げる
						for( i  = index - 1 ; i >= 0 ; i -- )
						{
							itemOffset -= OnItemUpdatedInner( i, null, null ) ;

							if( ( ContentSize - itemOffset ) >= ViewSize )
							{
								break ;	// 余力が出来た
							}
						}

						index = i ;

						viewOffset = ContentSize - ViewSize ;
					}
				}
			}
			else
			{
				// contentSize が viewSize より小さい場合はインデックスは０に固定される
				index = 0 ;
			}

			//----------------------------------------------------------

			m_CurrentItemIndex = index ;

			base.ContentPosition = viewOffset + snapOffset ;

			// 重要
			m_Snapping = 0 ;		// スナップ処理中であれば一旦キャンセルする(+1=スナップ処理中・0=現フレームではスナップを処理しない・-1=スナップ処理は終了している)

			//---------------------------------------------

			m_ItemUpperPosition = itemOffset ;

			UIView					itemView ;
			UnityEngine.Component	itemCode ;
			float					itemSize ;

			// アイテムを展開する
			l = m_WorkingItemCount ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				itemCode = null ;

				if( i >= m_ItemList.Count )
				{
					// 複製(一瞬親無しになってワーニングを引き起こす事があるので注意)
					itemView = GameObject.Instantiate( m_Item, Content.transform ) as UIView ;

					// 親を設定
//					itemView.SetParent( Content, false ) ;
				}
				else
				{
					// 継続
					itemView = m_ItemList[ i ].View ;

					itemCode = m_ItemList[ i ].Code ;
				}

				if( Infinity == true || ( Infinity == false && ( index + i ) <  itemCount ) )
				{
					// 名前を設定
					itemView.name = ( index + i ).ToString() ;

					// アイテムをアクティブ化
					itemView.SetActive( true ) ;

					// 位置を設定
					SetItemPosition( itemView, itemOffset ) ;

					if( itemCode == null && ItemComponentType != null )
					{
						// コンポーネントの指定があり且つコンポーネントのインスタンスが未取得なら取得する
						itemCode = itemView.GetComponent( ItemComponentType ) ;
					}

					// 生成コールバックの呼び出し
					itemSize = OnItemUpdatedInner( index + i, itemView.gameObject, itemCode ) ;

					// 縦幅を設定
					SetItemSize( itemView, itemSize ) ;

					// オフセット増加
					itemOffset += itemSize ;

					// アイテムリストに追加
					if( i >= m_ItemList.Count )
					{
						m_ItemList.Add( new ItemData( index + i, itemView, itemCode, itemSize ) ) ;
					}
					else
					{
						m_ItemList[ i ].Index	= index + i ;
						m_ItemList[ i ].Code	= itemCode ;	// タイミングによっては null の可能性もあるためここでも保存する
						m_ItemList[ i ].Size	= itemSize ;
					}
				}
				else
				{
					// 有限且つインデックスが最大アイテム数を超える場合はここにくる

					// 見えないダミーを登録しておく(必ず DisplayItemCount 分展開する必要がある)

					// 名前を設定
					itemView.name = $"hide {i}" ;

					// アイテムを非アクティブ化
					itemView.SetActive( false ) ;

					if( itemCode == null && ItemComponentType != null )
					{
						// コンポーネントの指定があり且つコンポーネントのインスタンスが未取得なら取得する
						itemCode = itemView.GetComponent( ItemComponentType ) ;
					}

					// ダミーサイズ
					itemSize = DefaultItemSize ;	// 幅が不確定な値になってしまうのでマージン値をダミー値としてセットする

					// オフセット増加
					itemOffset += itemSize ;

					if( i >= m_ItemList.Count )
					{
						// アイテムリストに追加
						m_ItemList.Add( new ItemData( -1, itemView, itemCode, itemSize ) ) ;
					}
					else
					{
						m_ItemList[ i ].Index	= -1 ;
						m_ItemList[ i ].Code	= itemCode ;	// タイミングによっては null の可能性もあるためここでも保存する
						m_ItemList[ i ].Size	= itemSize ;
					}
				}
			}

			m_ItemLowerPosition = itemOffset ;

//			Debug.LogWarning( "------>計測時間: [ " + ( Time.realtimeSinceStartup - tt ) + " ]" ) ;

			return ContentSize ;
		}

		/// <summary>
		/// 展開中のアイテムを取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T GetWorkingItem<T>( int index ) where T : Component
		{
			if( m_ItemList == null || m_ItemList.Count == 0 || index <  0 )
			{
				return null ;
			}

			if( ItemComponentType != typeof( T ) )
			{
				return null ;
			}

			//----------------------------------------------------------

			int i, l = m_ItemList.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_ItemList[ i ].Index == index )
				{
					// 発見
					if( m_ItemList[ i ].Code != null )
					{
						return m_ItemList[ i ].Code as T ;
					}
				}
			}

			return null ;
		}

		/// <summary>
		/// 展開中のアイテム群を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T[] GetWorkingItems<T>( bool visibleOnly = true ) where T : Component
		{
			if( m_ItemList == null || m_ItemList.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			var items = new List<( int, T )>() ;

			int i, l = m_ItemList.Count ;

			if( ItemComponentType == typeof( T ) )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 発見
					if( visibleOnly == false || ( visibleOnly == true && m_ItemList[ i ].View.ActiveSelf == true ) )
					{
						if( m_ItemList[ i ].Code != null )
						{
							items.Add( ( m_ItemList[ i ].Index, m_ItemList[ i ].Code as T ) ) ;
						}
					}
				}
			}
			else
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 発見
					if( visibleOnly == false || ( visibleOnly == true && m_ItemList[ i ].View.ActiveSelf == true ) )
					{
						if( m_ItemList[ i ].View.TryGetComponent<T>( out var component ) == true )
						{
							items.Add( ( m_ItemList[ i ].Index, component ) ) ;
						}
					}
				}
			}

			if( items.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------
			// インデックスでソートする

			var sortedItems = new List<T>() ;

			( int, T ) item_a ;
			( int, T ) item_b ;

			int a, b ;
			l = items.Count ;
			for( a  = 0 ; a <  ( l - 1 ) ; a ++ )
			{
				for( b  = ( a + 1 ) ; b <  l ; b ++ )
				{
					item_a = items[ a ] ;
					item_b = items[ b ] ;

					if( item_b.Item1 <  item_a.Item1 )
					{
						items[ a ] = item_b ;
						items[ b ] = item_a ;
					}
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				sortedItems.Add( items[ i ].Item2 ) ;
			}

			//----------------------------------------------------------

			return sortedItems.ToArray() ;
		}


		/// <summary>
		/// 展開中のアイテムを取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public GameObject GetWorkingItem( int index )
		{
			if( m_ItemList == null || m_ItemList.Count == 0 || index <  0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			int i, l = m_ItemList.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_ItemList[ i ].Index == index )
				{
					// 発見
					if( m_ItemList[ i ].Code != null )
					{
						return m_ItemList[ i ].Code.gameObject ;
					}
				}
			}

			return null ;
		}

		/// <summary>
		/// 展開中のアイテム群を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public GameObject[] GetWorkingItems( bool visibleOnly = true )
		{
			if( m_ItemList == null || m_ItemList.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			var items = new List<( int, GameObject )>() ;

			int i, l = m_ItemList.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				// 発見
				if( m_ItemList[ i ].Code != null )
				{
					if( visibleOnly == false || ( visibleOnly == true && m_ItemList[ i ].View.ActiveSelf == true ) )
					{
						items.Add( ( m_ItemList[ i ].Index, m_ItemList[ i ].Code.gameObject ) ) ;
					}
				}
			}

			if( items.Count == 0 )
			{
				return null ;
			}

			//----------------------------------------------------------
			// インデックスでソートする

			var sortedItems = new List<GameObject>() ;

			( int, GameObject ) item_a ;
			( int, GameObject ) item_b ;

			int a, b ;
			l = items.Count ;
			for( a  = 0 ; a <  ( l - 1 ) ; a ++ )
			{
				for( b  = ( a + 1 ) ; b <  l ; b ++ )
				{
					item_a = items[ a ] ;
					item_b = items[ b ] ;

					if( item_b.Item1 <  item_a.Item1 )
					{
						items[ a ] = item_b ;
						items[ b ] = item_a ;
					}
				}
			}

			for( i  = 0 ; i <  l ; i ++ )
			{
				sortedItems.Add( items[ i ].Item2 ) ;
			}

			//----------------------------------------------------------

			return sortedItems.ToArray() ;
		}

		//-------------------------------------------------------------------------------------------

		// インデックスをポジションに変換する
		private float ConvertIndexToPosition_Private( int index )
		{
			//----------------------------------------------------------

			// コンテントサイズは最初に設定しておく必要がある

			int i, l ;
			int itemCount = ItemCount ;
			float contentSize = 0 ;
			
			if( Infinity == false )
			{
				// 有限

				// コンテントのサイズを計算する
				for( i  = 0 ; i < itemCount ; i ++ )
				{
					// 生成コールバックの呼び出し
					contentSize += OnItemUpdatedInner( i, null, null ) ;
				}

				if( contentSize <  ViewSize )
				{
					contentSize  = ViewSize ;
				}
			}
			else
			{
				// 無限

				// コンテントのサイズ
				contentSize = ViewSize ;	// 無限スクロールの場合はバウンドしないのでこの値自体はどうでもよい
			}

			//------------------------------------------------------------------

			float itemOffset = 0 ;
			float snapOffset = 0 ;	// スナップ有効且つアンカーが最後の場合のズレ補正値

			if( ( m_Infinity == false && contentSize >  ViewSize ) || m_Infinity == true )
			{
				if( m_Snap == true && m_SnapAnchorType == SnapAnchorTypes.Tail )
				{
					// 最後基準である場合はインデックスは画面最後のものとみなしオフセット値の開始値もずれる

					if( m_Infinity == false )
					{
						// 有限
						l = 0 ;
					}
					else
					{
						// 無限
						l = -65536 ;
					}

					contentSize = 0 ;
					for( i  = index ; i >= l ; i -- )
					{
						contentSize += OnItemUpdatedInner( i, null, null ) ;
						if( contentSize >= ViewSize )
						{
							break ;	// 足りた
						}
					}

					if( contentSize <  ViewSize )
					{
						// 足りない(足りない状況が起こり得るのは有限の場合のみ
						for( i  = ( index + 1 ) ; i <  m_ItemCount ; i ++ )
						{
							contentSize += OnItemUpdatedInner( i, null, null ) ;
							if( contentSize >=  ViewSize )
							{
								break ;	// 足りた
							}
						}

						index = 0 ;	// インデックスは０始まりで確定
					}
					else
					{
						index = i ;
					}

					snapOffset = contentSize - ViewSize ;	// ズレ
				}

				// オフセット値を調整する
				if( index >  0 )
				{
					// 正方向
					for( i  = 0 ; i <  index ; i ++ )
					{
						// 幅だけ取得する
						itemOffset += OnItemUpdatedInner( i, null, null ) ;
					}
				}
				else
				if( index <  0 )
				{
					// 負方向
					for( i  = -1 ; i >  ( index - 1 ) ; i -- )
					{
						// 幅だけ取得する
						itemOffset -= OnItemUpdatedInner( i, null, null ) ;
					}
				}

				// 有限の場合は最後の方に行き過ぎると表示がおかしくなるのでスナップで限界までに留める
				if( Infinity == false )
				{
					// 有限

					// 下方向にオーバーする場合の補正をかける
					if( ( itemOffset + ViewSize ) >  ContentSize )
					{
						itemOffset = ContentSize - ViewSize ;
					}
				}
			}

			//----------------------------------------------------------

			return itemOffset + snapOffset ;
		}

		/// <summary>
		/// インデックスをポジションに変換する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float ConvertIndexToPosition( int index )
		{
			int i ;

			//----------------------------------------------------------

			float itemUpperPosition = 0 ;

			// オフセット値を調整する
			if( index >  0 )
			{
				// 正方向
				for( i  = 0 ; i <  index ; i ++ )
				{
					// 幅だけ取得する
					itemUpperPosition += OnItemUpdatedInner( i, null, null ) ;
				}
			}
			else
			if( index <  0 )
			{
				// 負方向(無限の場合のみ)
				for( i  = -1 ; i >  ( index - 1 ) ; i -- )
				{
					// 幅だけ取得する
					itemUpperPosition -= OnItemUpdatedInner( i, null, null ) ;
				}
			}

			//----------------------------------------------------------

			return itemUpperPosition ;
		}

		/// <summary>
		/// インデックスをポジション範囲に変換する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ( float, float ) ConvertIndexToPositionRange( int index )
		{
			int i ;

			//----------------------------------------------------------

			float itemUpperPosition = 0 ;

			// オフセット値を調整する
			if( index >  0 )
			{
				// 正方向
				for( i  = 0 ; i <  index ; i ++ )
				{
					// 幅だけ取得する
					itemUpperPosition += OnItemUpdatedInner( i, null, null ) ;
				}
			}
			else
			if( index <  0 )
			{
				// 負方向(無限の場合のみ)
				for( i  = -1 ; i >  ( index - 1 ) ; i -- )
				{
					// 幅だけ取得する
					itemUpperPosition -= OnItemUpdatedInner( i, null, null ) ;
				}
			}

			//----------------------------------------------------------

			float itemLowerPosition = itemUpperPosition + OnItemUpdatedInner( index, null, null ) ;	// 最後に自身の幅を加算する

			return ( itemUpperPosition, itemLowerPosition ) ;
		}

		/// <summary>
		/// ポジションをインデックスに変換する
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public int ConvertPositionToIndex( float position, bool isLower )
		{
			int i, l ;

			//----------------------------------------------------------

			if( position >= 0 )
			{
				// 正方向

				if( Infinity == false )
				{
					// 有限
					l = ItemCount ;
				}
				else
				{
					// 無限
					l = int.MaxValue ;
				}

				float itemUpperPosition = 0 ;
				float itemLowerPosition = 0 ;

				if( isLower == false )
				{
					// Upper を基準
					for( i  = 0 ; i <  l ; i ++ )
					{
						itemLowerPosition += OnItemUpdatedInner( i, null, null ) ;

						if( itemUpperPosition <= position && position <  itemLowerPosition )
						{
							// 確定
							return i ;
						}

						itemUpperPosition = itemLowerPosition ;
					}
				}
				else
				{
					// Lower を基準
					for( i  = 0 ; i <  l ; i ++ )
					{
						itemLowerPosition += OnItemUpdatedInner( i, null, null ) ;

						if( itemUpperPosition <  position && position <= itemLowerPosition )
						{
							// 確定
							return i ;
						}

						itemUpperPosition = itemLowerPosition ;
					}
				}
			}
			else
			{
				// 負方向(無限の場合のみ)

				if( Infinity == false )
				{
					// 負は不可
					return -1 ;
				}
				else
				{
					l = - int.MaxValue ;
				}

				float itemUpperPosition = 0 ;
				float itemLowerPosition = 0 ;

				if( isLower == false )
				{
					// Upper を基準
					for( i  = -1 ; i >  l ; i -- )
					{
						// 幅だけ取得する
						itemUpperPosition -= OnItemUpdatedInner( i, null, null ) ;

						if( itemUpperPosition <= position && position <  itemLowerPosition )
						{
							// 確定
							return i ;
						}

						itemLowerPosition = itemUpperPosition ;
					}
				}
				else
				{
					// Lower を基準
					for( i  = -1 ; i >  l ; i -- )
					{
						// 幅だけ取得する
						itemUpperPosition -= OnItemUpdatedInner( i, null, null ) ;

						if( itemUpperPosition <  position && position <= itemLowerPosition )
						{
							// 確定
							return i ;
						}

						itemLowerPosition = itemUpperPosition ;
					}
				}
			}

			// 該当を発見出来ず
			return -1 ;
		}

		//-----------------------------------------------------------

		private readonly List<ItemData> m_ItemChecker = new () ;

		// 更新する(LateUpdate から呼ばれてる)
		private void ProcessItem()
		{
			if( m_WorkingItemCount <= 0 || m_ItemCount <= 0 )
			{
				return ;
			}

			if( m_Infinity == false && ContentSize <= ViewSize )
			{
				return ;
			}

			//----------------------------------------

			ItemData item ;
			UIView itemView ;
			UnityEngine.Component itemCode ;
			float itemSize ;

			//----------------------------------------------------------

			m_ItemChecker.Clear() ;
			bool workingItemFew = false ;

			var vr = new Rect() ;
			var sr = new Rect() ;

			if( m_MarginEnabled == true )
			{
				// マージンが有効である場合、ListView と Viewport の間にもアイテムを表示する

				vr = m_Viewport.RectInCanvas ;
				sr = RectInCanvas ;
			}

			//----------------------------------------------------------

			float lowerPosition = base.ContentPosition + ViewSize ;

			if( m_MarginEnabled == true )
			{
				// マージンが有効である場合、ListView と Viewport の間にもアイテムを表示する

				float lowerMargin = 0 ;

				if( DirectionType == DirectionTypes.Horizontal )
				{
					// 右
					lowerMargin =   ( sr.xMax - vr.xMax ) ;
				}
				else
				if( DirectionType == DirectionTypes.Vertical )
				{
					// 下
					lowerMargin = - ( sr.yMin - vr.yMin ) ;
				}

				if( lowerMargin >  0 )
				{
					// マージンが存在する
					lowerPosition += lowerMargin ;
				}
			}

			// インデックスは↓正に進む＝コンテントは↑負に進む(上のを下に持っていく)
			while( m_ItemLowerPosition <  lowerPosition )
			{
				// 最初のアイテムを最後のアイテムに移動させる

				item = m_ItemList[ 0 ] ;
				m_ItemList.RemoveAt( 0 ) ;
				m_ItemList.Add( item ) ;

				m_ItemChecker.Add( item ) ;	// チェッカー

				itemView = item.View ;
				itemCode = item.Code ;
				if( itemCode == null && ItemComponentType != null )
				{
					// ItemComponentType を設定するタイミングによっては itemCode が null の場合がありえる 
					itemCode = itemView.GetComponent( ItemComponentType ) ;
					item.Code = itemCode ;
				}
				itemSize = item.Size ;

				m_ItemUpperPosition += itemSize ;

				int index = m_CurrentItemIndex + m_WorkingItemCount ;
				if( m_Infinity == false )
				{
					// 有限
					if( index <  0 || index >= m_ItemCount )
					{
						// リミットを超えた分は非アクティブにする
//						Debug.LogWarning( "↓リミット超えてます:" + index ) ;
						itemView.SetActive( false ) ;

						// アイテムの名前を更新
						itemView.name = $"hide {index}" ;

						// ダミーのサイズ
						itemSize = DefaultItemSize ;	// 幅が不確定な値になってしまうのでマージン値をダミー値としてセットする
					}
					else
					{
						// 生成時のコールバック呼び出し
						itemView.SetActive( true ) ;

						// アイテムの名前を更新
						itemView.name = index.ToString() ;

						// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
						itemSize = OnItemUpdatedInner( index, itemView.gameObject, itemCode ) ;
					}
				}
				else
				{
					// 無限

					// アイテムの名前を更新
					itemView.name = index.ToString() ;

					// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
					itemSize = OnItemUpdatedInner( index, itemView.gameObject, itemCode ) ;
				}

				item.Size  = itemSize ;	// 位置変動したアイテムの縦幅をきちんと更新する
				item.Index = index ;

//				Debug.LogWarning( "↓位置:" + index + " " + m_ItemTailPosition ) ;
				SetItemPosition( itemView, m_ItemLowerPosition ) ;
				if( itemSize >  0 )
				{
					SetItemSize( itemView, itemSize ) ;
				}
				m_ItemLowerPosition += itemSize ;	// 後に座標を更新する

				// 最初に設定したコンテントサイズより現在の末尾の位置の方が大きい場合にコンテントサイズを更新する(保険)
				if( m_Infinity == false && index == ( m_ItemCount - 1 ) && m_ItemLowerPosition >  ContentSize )
				{
					// しかし完全な解決にはならない(
//					Debug.Log( "オーバーしました:" + m_ItemTailPosition + " " + ContentSize ) ;
					ContentSize = m_ItemLowerPosition ;
				}

				m_CurrentItemIndex ++ ;

				m_ContentDirty = true ;
			}

			//----------------------------------------------------------

			float upperPosition = base.ContentPosition ;

			if( m_MarginEnabled == true )
			{
				// マージンが有効である場合、ListView と Viewport の間にもアイテムを表示する

				float upperMargin = 0 ;

				if( DirectionType == DirectionTypes.Horizontal )
				{
					// 左
					upperMargin = - ( sr.xMin - vr.xMin ) ;
				}
				else
				if( DirectionType == DirectionTypes.Vertical )
				{
					// 上
					upperMargin =   ( sr.yMax - vr.yMax ) ;
				}

				if( upperMargin >  0 )
				{
					// マージンが存在する
					upperPosition -= upperMargin ;
				}
			}

			// インデックスは↑負に進む＝コンテントは↓正に進む((下のを上に持っていく)
			while( m_ItemUpperPosition >  upperPosition )
			{
				// 最後のアイテムを最初に付け直す
				int lastIndex = WorkingItemCount - 1 ; 

				item = m_ItemList[ lastIndex ] ;
				m_ItemList.RemoveAt( lastIndex ) ;
				m_ItemList.Insert( 0, item ) ;

				if( m_ItemChecker.Contains( item ) == true )
				{
					workingItemFew = true ;
				}

				itemView = item.View ;
				itemCode = item.Code ;
				if( itemCode == null && ItemComponentType != null )
				{
					// ItemComponentType を設定するタイミングによっては itemCode が null の場合がありえる 
					itemCode = itemView.GetComponent( ItemComponentType ) ;
					item.Code = itemCode ;
				}
				itemSize = item.Size ;

				m_ItemLowerPosition -= itemSize ;

				m_CurrentItemIndex -- ;

				int index = m_CurrentItemIndex ;
//				Debug.LogWarning( "展開:" + tIndex ) ;
				if( m_Infinity == false )
				{
					// 有限

					if( index <  0 || index >= m_ItemCount )
					{
						// リミットを超えた分は非アクティブにする
						itemView.SetActive( false ) ;
//						Debug.LogWarning( "↑リミット超えてます:" + tIndex ) ;

						// アイテムの名前を更新
						itemView.name = $"hide{index}" ;

						// ダミーのサイズ
						itemSize = DefaultItemSize ;	// 幅が不確定な値になってしまうのでマージン値をダミー値としてセットする
					}
					else
					{
						// 生成時のコールバック呼び出し
						itemView.SetActive( true ) ;

						// アイテムの名前を更新
						itemView.name = index.ToString() ;

						// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
						itemSize = OnItemUpdatedInner( m_CurrentItemIndex, itemView.gameObject, itemCode ) ;
					}
				}
				else
				{
					// 無限

					// アイテムの名前を更新
					itemView.name = index.ToString() ;

					// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
					itemSize = OnItemUpdatedInner( m_CurrentItemIndex, itemView.gameObject, itemCode ) ;
				}

				item.Size  = itemSize ;	// 位置変動したアイテムの縦幅をきちんと更新する
				item.Index = index ;

				m_ItemUpperPosition -= itemSize ;	// 先に座標を更新する

//				Debug.LogWarning( "↑位置:" + tIndex + " " + m_ItemHeadPosition ) ;
				SetItemPosition( itemView, m_ItemUpperPosition ) ;
				if( itemSize >  0 )
				{
					SetItemSize( itemView, itemSize ) ;
				}

				m_ContentDirty = true ;
			}

			//----------------------------------------------------------

			m_ItemChecker.Clear() ;

			if( workingItemFew == true )
			{
				Debug.LogWarning( "[UIListView] : WorkingItem is few -> " + m_ItemList.Count + " < " + Path ) ;
			}
		}

		//-----------------------------------------------------------

		private int   m_Snapping = 0 ;
		private float m_SnapFromPosition ;
		private float m_SnapToPosition ;
		private float m_SnapBaseTime ;

		// スナップを処理する
		private void ProcessSnap()
		{
			if( Snap == false )
			{
				return ;
			}

			if( m_Infinity == false && ContentSize <= ViewSize )
			{
				// スナップ無効
				return ;
			}

//			Debug.LogWarning( "リミット:" +  ( m_CanvasLength * 100f / 960f ) ) ;


			Vector2 wheel = InputAdapter.UIEventSystem.MouseScrollDelta ;

			if( CScrollRect.velocity.magnitude >  ( m_SnapThreshold * m_CanvasLength / 960f ) || CScrollRect.IsDrag == true || wheel.x != 0 || wheel.y != 0 )
			{
				m_Snapping = 0 ;

				// スナップ処理は行わない
				return ;
			}


//			Debug.LogWarning( "スナップ実行:" + CScrollRect.velocity.magnitude + " " + CScrollRect.velocity + " " + CScrollRect.isDrag ) ;

			// 強制的にフリックによるスクロールを停止させる
			CScrollRect.velocity = Vector2.zero ;

			if( m_Snapping <  0 )
			{
				// スナップ処理は実行していないがホイール操作でベロシティ０で位置ずれが発生する可能性がある
				float snapFromPosition = base.ContentPosition ;	// スナップ前の位置
				float snapToPosition = GetSnapPosition() ;	// スナップ後の位置

				if( snapFromPosition != snapToPosition )
				{
					// 位置ずれが発生している
					m_Snapping = 0 ;
				}
			}

			if( m_Snapping == 0 )
			{
				// スナップ初期化

				// 現在位置を保存

				m_SnapFromPosition = base.ContentPosition ;	// スナップ前の位置
				m_SnapToPosition = GetSnapPosition() ;	// スナップ後の位置

				if( m_SnapTime <= 0 )
				{
					// 一瞬でスナップを終了させる
					base.ContentPosition = m_SnapToPosition ;
					m_Snapping = -1 ;	// スナップ終了
					return ;
				}

				m_SnapBaseTime = Time.realtimeSinceStartup ;

				// スナップ処理を実際に有効化する
				m_Snapping = 1 ;
			}

			if( m_Snapping == 1 )
			{
				// スナップ処理中
				float time = Time.realtimeSinceStartup - m_SnapBaseTime ;

				float factor = time / m_SnapTime ;
				if( factor >  1 )
				{
					factor  = 1 ;
				}

				float delta = m_SnapToPosition - m_SnapFromPosition ;
				delta = UITween.GetValue( 0, delta, factor, UITween.ProcessTypes.Ease, UITween.EaseTypes.EaseOutQuad ) ;
				
				base.ContentPosition = m_SnapFromPosition + delta ;
				if( factor >= 1 )
				{
					base.ContentPosition = m_SnapToPosition ;
					m_Snapping = -1 ;	// スナップ終了
				}
			}
		}

		// 現在の位置に対するスナップ後の位置を取得する
		private float GetSnapPosition()
		{
			float snapToPosition ;

			float snapFromPosition = base.ContentPosition ;

			int i ;

			if( m_SnapAnchorType == SnapAnchorTypes.Haed )
			{
				// スナップは最初基準

				float snapBasePosition = snapFromPosition ;

				if( Infinity == false )
				{
					// バウンドの完全戻りを待たずにスナップ処理が走り始めるので tSnapBasePosition がバウンド中状態の座標で + viewSize すると contentSize をオーバーしてしまいガクガク状態のバグが発生するのできちんと最大値を contentSize にする
					if( snapBasePosition <  0 )
					{
						snapBasePosition  = 0 ;
					}
				}

				float nearestPosition	= m_ItemUpperPosition ;
				float position			= m_ItemUpperPosition ;

				float minimumDistance = Mathf.Abs( position - snapBasePosition ) ;
				float distance ;
				for( i  = 0 ; i <  m_ItemList.Count ; i ++ )
				{
					position += m_ItemList[ i ].Size ;

					// ブレブレチェック
					if( Infinity == false && ( ContentSize - position ) <  ViewSize )
					{
						// １つ前の位置で決定
						break ;
					}

					distance = Mathf.Abs( position - snapBasePosition ) ;
					if( distance <  minimumDistance )
					{
						// 更新
						nearestPosition = position ;
						minimumDistance = distance ;
					}
				}
				
				snapToPosition = nearestPosition ;
			}
			else
			{
				// スナップは最後基準

				float snapBasePosition = snapFromPosition + ViewSize ;

				if( Infinity == false )
				{
					// バウンドの完全戻りを待たずにスナップ処理が走り始めるので tSnapBasePosition がバウンド中状態の座標で + viewSize すると contentSize をオーバーしてしまいガクガク状態のバグが発生するのできちんと最大値を contentSize にする
					if( snapBasePosition >  ContentSize )
					{
						snapBasePosition  = ContentSize ;
					}
				}

				float nearestPosition	= m_ItemUpperPosition ;
				float position			= m_ItemUpperPosition + m_ItemList[ 0 ].Size ;

				float minimumDistance = Mathf.Abs( position - snapBasePosition ) ;
				float distance ;
				for( i  = 1 ; i <  m_ItemList.Count ; i ++ )
				{
					position += m_ItemList[ i ].Size ;

					if( ( ( position >= snapBasePosition ) || ( ( position <  snapBasePosition ) && position >= ViewSize ) ) )
					{
						// 更新
						distance = Mathf.Abs( position - snapBasePosition ) ;
						if( distance <  minimumDistance )
						{
							nearestPosition = position ;
							minimumDistance = distance ;
						}
					}
				}
					
				snapToPosition = nearestPosition - ViewSize ;
			}

			return snapToPosition ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 基本的な設定を一括して行うメソッド(余計なリストの作り直しを省くためのもの)
		/// </summary>
		/// <param name="index"></param>
		/// <param name="itemCount"></param>
		/// <param name="onItemUpdated"></param>
		/// <param name="snap"></param>
		public void Setup( int index, int itemCount, Func<string,UIListView,int,GameObject,float> onItemUpdated, bool snap = false, bool infinity = false )
		{
			OnItemUpdatedAction	= onItemUpdated ;
			m_Snap				= snap ;
			m_Infinity			= infinity ;

			SetContentIndex( index, itemCount ) ;
		}

		/// <summary>
		/// 位置やアイテム数は変えずに表示を更新する
		/// </summary>
		public void Refresh( bool now = true )
		{
			if( now == true )
			{
				UpdateItemList() ;	// 強制更新
			}
			else
			{
				m_ItemListDirty = true ;	// Start か Update のタイミングで実行する
			}
		}

		/// <summary>
		/// 現在の位置・個数・縦幅を変更しない想定で表示の更新を行う(個々の縦幅を変更した場合の動作は保証されない)
		/// </summary>
		public void Restore()
		{
			if( m_ItemList == null || m_ItemList.Count == 0 )
			{
//				Debug.LogWarning( "Not fount item" ) ;
//				return ;
				Refresh() ;
			}

			//----------------------------------

			// アイテムを展開する
			ItemData item ;

			int i, l = m_WorkingItemCount ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				item = m_ItemList[ i ] ;
				if( item != null )
				{
					if( item.View.ActiveSelf == true )
					{
						// アクティブなもののみに表示更新要求を送る
						OnItemUpdatedInner( item.Index, item.View.gameObject, item.Code ) ;
					}
				}
				else
				{
					Debug.LogWarning( "Not fount item : index = " + i ) ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		// アイテムの幅を取得する
		public float DefaultItemSize
		{
			get
			{
				if( m_Item != null )
				{
					// 横スクロール
					if( DirectionType == DirectionTypes.Horizontal )
					{
						return m_Item.Width ;
					}
	
					// 縦スクロール
					if( DirectionType == DirectionTypes.Vertical )
					{
						return m_Item.Height ;
					}
				}
				
				return 0 ;
			}
		}

		// アイテムのポジションを設定する
		private void SetItemPosition( UIView view, float position )
		{
			// 横スクロール
			if( DirectionType == DirectionTypes.Horizontal )
			{
				view.Px =   position ;
			}

			// 縦スクロール
			if( DirectionType == DirectionTypes.Vertical )
			{
				view.Py = - position ;
			}
		}

		// アイテムのサイズを設定する
		private void SetItemSize( UIView view, float size )
		{
			// 横スクロール
			if( DirectionType == DirectionTypes.Horizontal )
			{
				view.Width = size ;
			}

			// 縦スクロール
			if( DirectionType == DirectionTypes.Vertical )
			{
				view.Height = size ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 表示上の最初の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えていない場合でも該当する)　※現在の状態限定
		/// </summary>
		/// <returns></returns>
		public int ItemUpperIndex
		{
			get
			{
				( var viewUpperPosition, _ ) = GetViewPosition() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
					UpdateItemList() ;
				}

				//---------------------------------

				float itemUpperPosition = m_ItemUpperPosition ;
				float itemLowerPosition ;

				int index = m_CurrentItemIndex ;

				int i, l = m_ItemList.Count ;

				int upperIndex = -1 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					itemLowerPosition = itemUpperPosition + m_ItemList[ i ].Size ;

					if( viewUpperPosition <  itemLowerPosition )
					{
						if( m_ItemList[ i ].View.ActiveSelf == false )
						{
							// 非表示に到達したら終了
							break ;
						}

						if( itemUpperPosition <= viewUpperPosition && viewUpperPosition <  itemLowerPosition )
						{
							// ここ決定
							upperIndex = index ;
							break ;
						}
					}

					itemUpperPosition = itemLowerPosition ;

					index ++ ;
				}

				return upperIndex ;	// 該当無し
			}
		}

		/// <summary>
		/// 表示上の最初の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えていない場合でも該当する)
		/// </summary>
		/// <returns></returns>
		public int GetItemUpperIndex( float viewUpperPosition )
		{
			int i, l = ItemCount ;

			float itemUpperPosition = 0 ;
			float itemLowerPosition ;

			int itemIndex = -1 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				itemLowerPosition = itemUpperPosition + OnItemUpdatedInner( i, null, null ) ; 

				if( itemUpperPosition <= viewUpperPosition && viewUpperPosition < itemLowerPosition )
				{
					// ここに決定(最初に条件を満たしたアイテム
					itemIndex = i ;
					break ;
				}

				itemUpperPosition = itemLowerPosition ;
			}

			return itemIndex ;
		}

		/// <summary>
		/// 表示上の最後の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えていない場合でも該当する)　※現在の状態限定
		/// </summary>
		/// <returns></returns>
		public int ItemLowerIndex
		{
			get
			{
				( var viewUpperPosition, var viewLowerPosition ) = GetViewPosition() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
					UpdateItemList() ;
				}

				//---------------------------------

				float itemUpperPosition = m_ItemUpperPosition ;
				float itemLowerPosition ;

				int index = m_CurrentItemIndex ;

				int i, l = m_ItemList.Count ;

				int itemIndex = -1 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					itemLowerPosition = itemUpperPosition + m_ItemList[ i ].Size ;

					if( viewUpperPosition <  itemLowerPosition )
					{
						if( m_ItemList[ i ].View.ActiveSelf == false )
						{
							// 非表示に到達したら終了
							break ;
						}

						if( itemUpperPosition <  viewLowerPosition && viewLowerPosition <= itemLowerPosition )
						{
							// ここ決定
							itemIndex = index ;
							break ;
						}
					}

					itemUpperPosition = itemLowerPosition ;

					index ++ ;
				}

				return itemIndex ;	// 該当無し
			}
		}

		/// <summary>
		/// 表示上の最後の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えていない場合でも該当する)
		/// </summary>
		/// <returns></returns>
		public int GetItemLowerIndex( float viewLowerPosition )
		{
			int i, l = ItemCount ;

			float itemUpperPosition = 0 ;
			float itemLowerPosition ;

			int itemIndex = -1 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				itemLowerPosition = itemUpperPosition + OnItemUpdatedInner( i, null, null ) ; 

				if( itemUpperPosition <  viewLowerPosition && viewLowerPosition <= itemLowerPosition )
				{
					// ここに決定(最初に条件を満たしたアイテム
					itemIndex = i ;
					break ;
				}

				itemUpperPosition = itemLowerPosition ;
			}

			return itemIndex ;
		}

		//---------------

		/// <summary>
		/// 表示上の最初の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えている場合のみ該当する)　※現在の状態限定
		/// </summary>
		/// <returns></returns>
		public int ItemUpperInsideIndex
		{
			get
			{
				( var viewUpperPosition, _ ) = GetViewPosition() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
					UpdateItemList() ;
				}

				//---------------------------------


				float itemUpperPosition = m_ItemUpperPosition ;
				float itemLowerPosition ;

				int index = m_CurrentItemIndex ;

				int i, l = m_ItemList.Count ;

				int itemIndex = -1 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					itemLowerPosition = itemUpperPosition + m_ItemList[ i ].Size ;

					if( viewUpperPosition <  itemLowerPosition )
					{
						if( m_ItemList[ i ].View.ActiveSelf == false )
						{
							// 非表示に到達したら終了
							break ;
						}

						if( viewUpperPosition <= itemUpperPosition )
						{
							// ここ決定(最初に条件を満たしたアイテム)
							itemIndex = index ;
							break ;
						}
					}

					itemUpperPosition = itemLowerPosition ;

					index ++ ;
				}

				return itemIndex ;	// 該当無し
			}
		}

		/// <summary>
		/// 表示上の最初の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えている場合のみ該当する)
		/// </summary>
		/// <returns></returns>
		public int GetItemUpperInsideIndex( float viewUpperPosition )
		{
			int i, l = ItemCount ;

			float itemUpperPosition = 0 ;

			int itemIndex = -1 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( viewUpperPosition <= itemUpperPosition )
				{
					// ここに決定(最初に条件を満たしたアイテム
					itemIndex = i ;
					break ;
				}

				itemUpperPosition += OnItemUpdatedInner( i, null, null ) ;
			}

			return itemIndex ;	// 該当無し
		}

		/// <summary>
		/// 表示上の最後の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えている場合のみ該当する)
		/// </summary>
		/// <returns></returns>
		public int ItemLowerInsideIndex
		{
			get
			{
				( var viewUpperPosition, var viewLowerPosition ) = GetViewPosition() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
					UpdateItemList() ;
				}

				//---------------------------------

				float itemUpperPosition = m_ItemUpperPosition ;
				float itemLowerPositin ;

				int index = m_CurrentItemIndex ;

				int i, l = m_ItemList.Count ;

				int itemIndex = -1 ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					itemLowerPositin = itemUpperPosition + m_ItemList[ i ].Size ;

					if( viewUpperPosition <  itemLowerPositin )
					{
						if( m_ItemList[ i ].View.ActiveSelf == false )
						{
							// 非表示に到達したら終了
							break ;
						}

						if( itemLowerPositin <= viewLowerPosition )
						{
							// 候補
							itemIndex = index ;
						}
						else
						{
							// 溢れたら最後のものとする
							break ;
						}
					}

					itemUpperPosition = itemLowerPositin ;

					index ++ ;
				}

				return itemIndex ;	// 該当無し
			}
		}

		/// <summary>
		/// 表示上の最後の位置にあるアイテムのインデックスを取得する(アイテムが完全に見えている場合のみ該当する)
		/// </summary>
		/// <returns></returns>
		public int GetItemLowerInsideIndex( float viewLowerPosition )
		{
			int i, l = ItemCount ;

			float itemLowerPosition = 0 ;

			int itemIndex = -1 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				itemLowerPosition += OnItemUpdatedInner( i, null, null ) ;

				if( itemLowerPosition <= viewLowerPosition )
				{
					// 候補
					itemIndex = i ;
				}
				else
				{
					// 溢れたら最後のものとする
					break ;
				}
			}

			return itemIndex ;	// 該当無し
		}

		//---------------

		// コンテンツ座標上のビューポートの表示位置を取得する
		private ( float, float ) GetViewPosition()
		{
			float contentPosition	= base.ContentPosition ;
			float contentSize		= base.ContentSize ;
			float viewSize			= ViewSize ;

			float upperViewPosition = contentPosition ;
			if( Infinity == false )
			{
				// 有限の場合は０で制限をかける(ScrollRect は誤差でマイナス値になっている場合がある)
				if( upperViewPosition <  0 )
				{
					upperViewPosition  = 0 ;
				}
			}

			float lowerViewPosition = upperViewPosition + viewSize ;
			if( Infinity == false )
			{
				// 有限の場合はコンテントサイズで制限をかける(ScrollRect は誤差でコンテントサイズを超える値になっている場合がある)
				if( lowerViewPosition >  contentSize )
				{
					lowerViewPosition  = contentSize ;
					upperViewPosition  = lowerViewPosition - viewSize ;
					if( upperViewPosition <  0 )
					{
						upperViewPosition  = 0 ;
					}
				}
			}

			return ( upperViewPosition, lowerViewPosition ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したインデックスの位置にフォーカスする
		/// </summary>
		/// <param name="index"></param>
		public void SetFocusIndex( int index )
		{
			( float upperItemPosition, float lowerItemPosition ) = ConvertIndexToPositionRange( index ) ;

			( float upperViewPosition, float lowerViewPosition ) = GetViewPosition() ;

			// 上に移動か下に移動か

			if( upperItemPosition <  upperViewPosition )
			{
				// 上に移動する
				SetContentPosition( upperItemPosition ) ;
			}
			else
			if( lowerViewPosition <  lowerItemPosition )
			{
				// 下に移動する
				float contentPosition = lowerItemPosition - ViewSize ;
				if( contentPosition <  0 )
				{
					contentPosition  = 0 ;
				}

				SetContentPosition( contentPosition ) ;
			}
			else
			{
				// 移動はしない
				Restore() ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// スクロールが可能かどうか
		/// </summary>
		public bool IsScrollable
		{
			get
			{
				if( m_Infinity == false && ContentSize <= ViewSize )
				{
					return false ;
				}

				return true ;
			}
		}

		//-----------------------------------------------------------
		
		// 内部リスナー
		private float OnItemUpdatedInner( int index, GameObject item, UnityEngine.Component itemCode )
		{
			float itemSize = 0 ;

			if( OnItemUpdatedAction != null || OnItemUpdatedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( OnItemUpdatedAction != null )
				{
					itemSize = OnItemUpdatedAction( identity, this, index, item ) ;
				}
				
				if( OnItemUpdatedDelegate != null )
				{
					itemSize = OnItemUpdatedDelegate( identity, this, index, item ) ;
				}
			}

			if( OnItemClassUpdatedAction != null || OnItemClassUpdatedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( OnItemClassUpdatedAction != null )
				{
					itemSize = OnItemClassUpdatedAction( identity, this, index, itemCode ) ;
				}

				if( OnItemClassUpdatedDelegate != null )
				{
					itemSize = OnItemClassUpdatedDelegate( identity, this, index, itemCode ) ;
				}
			}

			if( itemSize <= 0 )
			{
				return DefaultItemSize ;
			}

			return itemSize ;
		}
		
		//-----------------------------------------------------------
		// GameObject

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクション
		/// </summary>
		Func<string, UIListView, int, GameObject, float> OnItemUpdatedAction ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated( Func<string, UIListView, int, GameObject, float> onItemUpdatedAction )
		{
			OnItemUpdatedAction  = onItemUpdatedAction ;
			
			m_ItemListDirty = true ;
		}

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated( int itemCount, Func<string, UIListView, int, GameObject, float> onItemUpdatedAction )
		{
			m_ItemCount				= itemCount ;
			OnItemUpdatedAction		= onItemUpdatedAction ;
			
			m_ItemListDirty = true ;
		}
	

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="index">アイテムのインデックス番号</param>
		/// <param name="item">アイテムのゲームオブジェクトのインスタンス</param>
		public delegate float OnItemUpdated( string identity, UIListView view, int index, GameObject item ) ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲート
		/// </summary>
		public OnItemUpdated OnItemUpdatedDelegate ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onItemUpdatedDelegate">デリゲートメソッド</param>
		public void AddOnItemUpdated( OnItemUpdated onItemUpdatedDelegate )
		{
			OnItemUpdatedDelegate += onItemUpdatedDelegate ;

			m_ItemListDirty = true ;
		}
		
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onItemUpdatedDelegate">デリゲートメソッド</param>
		public void RemoveOnItemUpdated( OnItemUpdated onItemUpdatedDelegate )
		{
			OnItemUpdatedDelegate -= onItemUpdatedDelegate ;

			m_ItemListDirty = true ;
		}

		//-----------------------------------------------------------
		// Class

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクション
		/// </summary>
		protected Func<string, UIListView, int, UnityEngine.Component, float> OnItemClassUpdatedAction ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated( Func<string, UIListView, int, UnityEngine.Component, float> onItemClassUpdatedAction )
		{
			OnItemClassUpdatedAction	= onItemClassUpdatedAction ;

			m_ItemListDirty = true ;
		}

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated( int itemCount, Func<string, UIListView, int, UnityEngine.Component, float> onItemClassUpdatedAction )
		{
			m_ItemCount					= itemCount ;
			OnItemClassUpdatedAction	= onItemClassUpdatedAction ;

			m_ItemListDirty = true ;
		}

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated<T>( Func<string, UIListView, int, UnityEngine.Component, float> onItemClassUpdatedAction ) where T : UnityEngine.Component
		{
			ItemComponentType = typeof( T ) ;

			OnItemClassUpdatedAction	= onItemClassUpdatedAction ;

			m_ItemListDirty = true ;
		}

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated<T>( int itemCount, Func<string, UIListView, int, UnityEngine.Component, float> onItemClassUpdatedAction ) where T : UnityEngine.Component
		{
			ItemComponentType = typeof( T ) ;

			m_ItemCount					= itemCount ;
			OnItemClassUpdatedAction	= onItemClassUpdatedAction ;
			
			m_ItemListDirty = true ;
		}

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="index">アイテムのインデックス番号</param>
		/// <param name="item">アイテムのゲームオブジェクトのインスタンス</param>
		public delegate float OnItemClassUpdated( string identity, UIListView view, int index, UnityEngine.Component item ) ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲート
		/// </summary>
		public OnItemClassUpdated OnItemClassUpdatedDelegate ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onItemUpdatedDelegate">デリゲートメソッド</param>
		public void AddOnItemUpdated<T>( OnItemClassUpdated onItemClassUpdatedDelegate ) where T : UnityEngine.Component
		{
			ItemComponentType = typeof( T ) ;

			OnItemClassUpdatedDelegate += onItemClassUpdatedDelegate ;

			m_ItemListDirty = true ;
		}
		
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onItemUpdatedDelegate">デリゲートメソッド</param>
		public void RemoveOnItemUpdated<T>( OnItemClassUpdated onItemClassUpdatedDelegate ) where T : UnityEngine.Component
		{
			ItemComponentType = typeof( T ) ;

			OnItemClassUpdatedDelegate -= onItemClassUpdatedDelegate ;

			m_ItemListDirty = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のインデックスまで移動させる
		/// </summary>
		/// <param name="contentPosition"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public AsyncState MoveToIndex( int contentIndex, float duration, UITween.EaseTypes easeType = UITween.EaseTypes.EaseOutQuad )
		{
			if( duration <= 0 )
			{
				return null ;
			}

			float contentPosition = ConvertIndexToPosition_Private( contentIndex ) ;

			var state = new AsyncState( this ) ;
			StartCoroutine( MoveToPosition_Private( contentPosition, duration, easeType, state ) ) ;
			return state ;
		}

		/// <summary>
		/// 指定のインデックスまで移動させます
		/// </summary>
		/// <param name="contentIndex">移動したいインデックス + 追加する割合</param>
		/// <param name="duration"></param>
		/// <returns></returns>
		/// <remarks>
		/// <para><paramref name="contentIndex"/>に小数点以下がある場合、小数点以下を幅に対する割合とみなして計算します</para>
		/// </remarks>
		public AsyncState MoveToIndex( float contentIndex, float duration, UITween.EaseTypes easeType = UITween.EaseTypes.EaseOutQuad )
		{
			if( duration <= 0 )
			{
				return null ;
			}

			// 最低限移動させるインデックス
			var index = ( int )contentIndex ;

			// 小数点以下の取得
			var afPoint = contentIndex - index ;

			// どの地点まで移動させるのか？
			float contentPosition = ConvertIndexToPosition_Private( index ) ;
			contentPosition	+= ( DefaultItemSize * afPoint ) ;

			var state = new AsyncState( this ) ;
			StartCoroutine( MoveToPosition_Private( contentPosition, duration, easeType, state ) ) ;
			return state ;
		}

		//-------------------------------------------------------------------------------------------

		private PointerEventData	m_LVRH_EventData	= null ;
		private List<RaycastResult>	m_LVRH_Results		= null ;

		/// <summary>
		/// ポインターが上にあるリストビュー内のアイテムのインデックスを取得する
		/// </summary>
		/// <returns></returns>
		public int GetIndexByRaycastHit()
		{
			if( EventSystem.current == null )
			{
				// まだ準備が整っていない
				return -1 ;
			}

			//----------------------------------------------------------
			Vector2 position ;

#if UNITY_EDITOR || UNITY_STANDALONE

			position = InputAdapter.UIEventSystem.MousePosition ;

#elif !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )

			if( Input.touchCount == 1 )
			{
				position = Input.touches[ 0 ].position ;
			}
			else
			{
				return -1 ;
			}

#else
			return -1 ;
#endif

			if( m_ItemList == null || m_ItemList.Count == 0 )
			{
				return -1 ;
			}


			m_LVRH_EventData	??= new PointerEventData( EventSystem.current ) ;
			m_LVRH_Results		??= new List<RaycastResult>() ;

			// スクリーン座標からRayを飛ばす

			m_LVRH_EventData.position = position ;
			m_LVRH_Results.Clear() ;

			// レイキャストで該当するＵＩを探す
			EventSystem.current.RaycastAll( m_LVRH_EventData, m_LVRH_Results ) ;

			if( m_LVRH_Results.Count >= 1 )
			{
				GameObject target = m_LVRH_Results[ 0 ].gameObject ;

				int i, l = m_ItemList.Count ;

				while( true )
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_ItemList[ i ].View.gameObject == target )
						{
							// 発見しました
							return m_ItemList[ i ].Index ;
						}
					}

					if( target.transform.parent != null )
					{
						target = target.transform.parent.gameObject ;
					}
					else
					{
						break ;
					}
				}
			}

			return -1 ;
		}

		//-------------------------------------------------------------------------------------------
		// ゲームパッド対応処理

		/// <summary>
		/// 選択状態のインデックス番号(ゲームパッドでの制御用)
		/// </summary>
		public int SelectedIndex
		{
			get
			{
				return m_SelectedIndex ;
			}
			set
			{
				m_SelectedIndex = value ;
			}
		}

		protected	int m_SelectedIndex			= -1 ;
		private		int m_SelectedIndex_Keep	= -1 ;

		/// <summary>
		/// フォーカスのオン・オフを行う(ゲームパッドでの制御用)
		/// </summary>
		/// <param name="state"></param>
		public void SetPadFocus( bool state )
		{
			if( m_ItemCount <= 0 )
			{
				// 意味無し
				return ;
			}

			//----------------------------------

			if( state == true )
			{
				// フォーカスをオンにする

				int itemUpperIndideIndex = ItemUpperInsideIndex ;
				int itemLowerIndideIndex = ItemLowerInsideIndex ;

				bool isLower = false ;

				if( m_SelectedIndex_Keep >= 0 )
				{
					// 以前に設定された選択状態のインデックス番号が存在する
					if( m_SelectedIndex_Keep <  itemUpperIndideIndex )
					{
						// 表示範囲外になってしまうため無効
						m_SelectedIndex_Keep = -1 ;
						isLower = false ;
					}
					if( m_SelectedIndex_Keep >  itemLowerIndideIndex )
					{
						// 表示範囲外になってしまうため無効
						m_SelectedIndex_Keep = -1 ;
						isLower = true ;
					}
				}

				if( m_SelectedIndex_Keep >= 0 )
				{
					// 以前に設定された選択状態のインデックス番号は現時点でも有効である
					m_SelectedIndex = m_SelectedIndex_Keep ;
					m_SelectedIndex_Keep = -1 ;
				}
				else
				{
					// 以前に設定された選択状態のインデックス番号は現時点では無効である
					if( isLower == false )
					{
						m_SelectedIndex = itemUpperIndideIndex ;
					}
					else
					{
						m_SelectedIndex = itemLowerIndideIndex ;
					}
				}

				// 表示を更新する
				Restore() ;
			}
			else
			{
				// フォーカスをオフにする
				if( m_SelectedIndex >= 0 )
				{
					m_SelectedIndex_Keep = m_SelectedIndex ;

					m_SelectedIndex = -1 ;

					// 表示を更新する
					Restore() ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// パッドアクションの種別
		/// </summary>
		public enum PadActions
		{
			/// <summary>
			/// 何もしない
			/// </summary>
			None,


			/// <summary>
			/// １つ次のアイテムを選択状態にする
			/// </summary>
			NextItem,

			/// <summary>
			/// １つ前のアイテムを選択状態にする
			/// </summary>
			BackItem,


			/// <summary>
			/// １つ次のページを表示する
			/// </summary>
			NextPage,

			/// <summary>
			/// １つ前のページを表示する
			/// </summary>
			BackPage,


			/// <summary>
			/// 最後のアイテムを選択状態にする
			/// </summary>
			NextAll,

			/// <summary>
			/// 最初のアイテムを選択状態にする
			/// </summary>
			BackAll,
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// パッドアクションを実行する
		/// </summary>
		/// <param name="selectedIndex"></param>
		/// <param name=""></param>
		public void ProcessPadAction( PadActions padAction )
		{
			if( TryGetComponent<UIPadAdapter>( out var padAdapter ) == true )
			{
				if( padAdapter.Focus == false )
				{
					// 無効
					return ;
				}
			}

			//----------------------------------

			if( m_ItemCount <= 0 || m_SelectedIndex <  0 || m_SelectedIndex >= m_ItemCount )
			{
				// 選択対象インデックスが無効
				return ;
			}

			//----------------------------------------------------------

			int l = m_ItemCount ;

			if( padAction == PadActions.NextItem )
			{
				// １つ次のアイテムへ

				if( m_SelectedIndex <  ( l - 1 ) )
				{
					// １つ次に
					m_SelectedIndex ++ ;
				}
				else
				{
					// 一番最初に
					m_SelectedIndex = 0 ;
				}

				// 表示を更新する
				SetFocusIndex( m_SelectedIndex ) ;
			}
			else
			if( padAction == PadActions.BackItem )
			{
				// １つ前のアイテムへ

				if( m_SelectedIndex >  0 )
				{
					// １つ前に
					m_SelectedIndex -- ;
				}
				else
				{
					// 一番最後に
					m_SelectedIndex = l - 1 ;
				}

				// 表示を更新する
				SetFocusIndex( m_SelectedIndex ) ;
			}
			else
			if( padAction == PadActions.NextPage )
			{
				// １つ次のページへ

				float contentSize	= ContentSize ; 
				float viewSize		= ViewSize ;

				if( contentSize >  viewSize )
				{
					// 次のページへ進めるのはコンテントサイズがビューサイズより大きい場合のみ

					float contentPosition = ContentPosition ;

					float viewUpperPosition = contentPosition ;
					float viewLowerPosition = viewUpperPosition + viewSize ;

					float itemUpperPosition ;
					float itemLowerPosition ;

					if( ( m_Infinity == false && viewLowerPosition <  contentSize ) || m_Infinity == true )	// 無限の場合はこの判定は無い(無限に前のページに戻れる)
					{
						// まだ次のページへ進める余地がある

						// 現在の選択インデックスからポジション(上)を取得する
						( itemUpperPosition, _ ) = ConvertIndexToPositionRange( m_SelectedIndex ) ;
						float itemPositionOffset	= itemUpperPosition - contentPosition ;

						//---------
						// 次のページの位置へ

						viewUpperPosition += viewSize ;
						viewLowerPosition += viewSize ;

						if( viewLowerPosition >  contentSize )
						{
							// オーバーする
							viewLowerPosition  = contentSize ;
							viewUpperPosition  = viewLowerPosition - viewSize ;
						}

						itemUpperPosition = viewUpperPosition + itemPositionOffset ;

						// インデックスを逆算する
						int changedSelectedIndex = ConvertPositionToIndex( itemUpperPosition, false ) ;

						( itemUpperPosition, itemLowerPosition ) = ConvertIndexToPositionRange( changedSelectedIndex ) ;
						if( itemUpperPosition <  viewUpperPosition )
						{
							// 見切れるので補正が必要
							changedSelectedIndex = GetItemUpperInsideIndex( viewUpperPosition ) ;
						}
						else
						if( viewLowerPosition <  itemLowerPosition )
						{
							// 見切れるので補正が必要
							changedSelectedIndex = GetItemLowerInsideIndex( viewLowerPosition ) ;
						}

						m_SelectedIndex = changedSelectedIndex ;

						// 表示を更新する
						SetContentPosition( viewUpperPosition ) ;
					}
					else
					{
						// 有限且つ既に最後のページが表示されている(無限の場合はここには来ない)

						int itemUpperIndex = 0 ;
						int itemLowerIndex = m_ItemCount - 1 ;

						if( m_SelectedIndex <  itemLowerIndex )
						{
							// 最後のアイテムへ
							m_SelectedIndex  = itemLowerIndex ;

							// 表示を更新する
							Restore() ;
						}
						else
						{
							// 最初のアイテムへ
							m_SelectedIndex = itemUpperIndex ;

							// 表示を更新する
							SetFocusIndex( m_SelectedIndex ) ;
						}
					}
				}
				else
				{
					// ページ切り替えは出来ない

					int itemUpperIndex = 0 ;
					int itemLowerIndex = m_ItemCount - 1 ;

					if( m_SelectedIndex <  itemLowerIndex )
					{
						// 最後のアイテムへ
						m_SelectedIndex  = itemLowerIndex ;

						// 表示を更新する
						Restore() ;
					}
					else
					{
						// 最初のアイテムへ
						m_SelectedIndex = itemUpperIndex ;

						// 表示を更新する
						SetFocusIndex( m_SelectedIndex ) ;
					}
				}
			}
			else
			if( padAction == PadActions.BackPage )
			{
				// １つ前のページへ

				float contentSize	= ContentSize ; 
				float viewSize		= ViewSize ;

				if( contentSize >  viewSize )
				{
					// 前のページへ戻れるのはコンテントサイズがビューサイズより大きい場合のみ

					float contentPosition = ContentPosition ;

					float viewUpperPosition = contentPosition ;
					float viewLowerPosition = viewUpperPosition + viewSize ;

					float itemUpperPosition ;
					float itemLowerPosition ;

					if( ( m_Infinity == false && viewUpperPosition >  0 ) || m_Infinity == true )	// 無限の場合はこの判定は無い(無限に前のページに戻れる)
					{
						// まだ前のページへ戻れる余地がある

						// 現在の選択インデックスからポジション(上)を取得する
						( _, itemLowerPosition ) = ConvertIndexToPositionRange( m_SelectedIndex ) ;
						float itemPositionOffset	= itemLowerPosition - contentPosition ;

						//---------
						// 前のページの位置へ

						viewUpperPosition -= viewSize ;
						viewLowerPosition -= viewSize ;

						if( viewUpperPosition <  0 )
						{
							// オーバーする
							viewUpperPosition  = 0 ;
							viewLowerPosition  = viewUpperPosition + viewSize ;
						}

						itemLowerPosition = viewUpperPosition + itemPositionOffset ;

						// インデックスを逆算する
						int changedSelectedIndex = ConvertPositionToIndex( itemLowerPosition, true ) ;

						( itemUpperPosition, itemLowerPosition ) = ConvertIndexToPositionRange( changedSelectedIndex ) ;
						if( itemUpperPosition <  viewUpperPosition )
						{
							// 見切れるので補正が必要
							changedSelectedIndex = GetItemUpperInsideIndex( viewUpperPosition ) ;
						}
						else
						if( viewLowerPosition <  itemLowerPosition )
						{
							// 見切れるので補正が必要
							changedSelectedIndex = GetItemLowerInsideIndex( viewLowerPosition ) ;
						}

						m_SelectedIndex = changedSelectedIndex ;

						// 表示を更新する
						SetContentPosition( viewUpperPosition ) ;
					}
					else
					{
						// 有限且つ既に最初のページが表示されている(無限の場合はここには来ない)

						int itemUpperIndex = 0 ;
						int itemLowerIndex = m_ItemCount - 1 ;

						if( m_SelectedIndex >  itemUpperIndex )
						{
							// 最初のアイテムへ
							m_SelectedIndex  = itemUpperIndex ;

							// 表示を更新する
							Restore() ;
						}
						else
						{
							// 最後のアイテムへ
							m_SelectedIndex = itemLowerIndex ;

							// 表示を更新する
							SetFocusIndex( m_SelectedIndex ) ;
						}
					}
				}
				else
				{
					// ページの切り替えは出来ない

					int itemUpperIndex = 0 ;
					int itemLowerIndex = m_ItemCount - 1 ;

					if( m_SelectedIndex >  itemUpperIndex )
					{
						// 最初のアイテムへ
						m_SelectedIndex  = itemUpperIndex ;

						// 表示を更新する
						Restore() ;
					}
					else
					{
						// 最後のアイテムへ
						m_SelectedIndex = itemLowerIndex ;

						// 表示を更新する
						SetFocusIndex( m_SelectedIndex ) ;
					}
				}
			}
			else
			if( padAction == PadActions.NextAll )
			{
				// 最後へ
				m_SelectedIndex = m_ItemCount - 1 ;

				// 表示を更新する
				SetFocusIndex( m_SelectedIndex ) ;
			}
			else
			if( padAction == PadActions.BackAll )
			{
				// 最初へ
				m_SelectedIndex = 0 ;

				// 表示を更新する
				SetFocusIndex( m_SelectedIndex ) ;
			}
		}
	}
}
