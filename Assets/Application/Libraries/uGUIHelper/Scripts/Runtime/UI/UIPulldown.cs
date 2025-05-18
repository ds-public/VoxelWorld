using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using TMPro ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent( typeof( Button ) )]
	public class UIPulldown : UIButton
	{
		// 選択中のアイテム関係

		// テキスト
		[SerializeField]
		protected UITextMesh		m_CaptionText ;

		/// <summary>
		/// テキスト
		/// </summary>
		public UITextMesh CaptionText
		{
			get
			{
				return m_CaptionText ;
			}
			set
			{
				m_CaptionText = value ;
			}
		}

		// イメージ
		[SerializeField]
		protected UIImage		m_CaptionImage ;

		/// <summary>
		/// イメージ
		/// </summary>
		public UIImage			  CaptionImage
		{
			get
			{
				return m_CaptionImage ;
			}
			set
			{
				m_CaptionImage = value ;
			}
		}

		// リスト部分
		[SerializeField]
		protected UIListView		m_Template ;

		/// <summary>
		/// リスト部分
		/// </summary>
		public UIListView			  Template
		{
			get
			{
				return m_Template ;
			}
			set
			{
				m_Template = value ;
			}
		}

		// 複数選択を可能にするかどうか
		[SerializeField]
		protected bool				m_MultiSelect ;

		/// <summary>
		/// 複数選択を可能にするかどうか
		/// </summary>
		public bool					  MultiSelect
		{
			get
			{
				return m_MultiSelect ;
			}
			set
			{
				if( m_MultiSelect != value )
				{
					m_MultiSelect = value ;

					Refresh() ;
				}
			}
		}

		// 単一選択の選択中のインデックス値
		[SerializeField]
		protected int				m_Value ;

		/// <summary>
		/// 単一選択の選択中のインデックス値
		/// </summary>
		public int					  Value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				if( m_Value != value )
				{
					m_Value  = value ;

					Refresh() ;
				}
			}
		}

		// 複数選択の選択中のインデックス値
		[SerializeField]
		protected List<int>			m_Values ;

		/// <summary>
		/// 複数選択の選択中のインデックス値
		/// </summary>
		public List<int>			  Values
		{
			get
			{
				return m_Values ;
			}
			set
			{
				if( m_Values != value )
				{
					m_Values  = value ;

					Refresh() ;
				}
			}
		}

		// 全て非選択の文言
		[SerializeField]
		protected string			m_NothingText		= "Nothing" ;

		/// <summary>
		/// 全て非選択の文言
		/// </summary>
		public string				  NothingText
		{
			get
			{
				return m_NothingText ;
			}
			set
			{
				if( m_NothingText != value )
				{
					m_NothingText  = value ;

					Refresh() ;
				}
			}
		}

		// 全て選択時の文言
		[SerializeField]
		protected string			m_EverythingText	= "Everything" ;

		/// <summary>
		/// 全て選択時の文言
		/// </summary>
		public string				  EverythingText
		{
			get
			{
				return m_EverythingText ;
			}
			set
			{
				if( m_EverythingText != value )
				{
					m_EverythingText  = value ;

					Refresh() ;
				}
			}
		}

		// 複数選択時の文言
		[SerializeField]
		protected string			m_MixedText			= "Mixed..." ;

		/// <summary>
		/// 複数選択時の文言
		/// </summary>
		public string				  MixedText
		{
			get
			{
				return m_MixedText ;
			}
			set
			{
				if( m_MixedText != value )
				{
					m_MixedText  = value ;

					Refresh() ;
				}
			}
		}

		// リスト部分の表示・隠蔽の速度係数
		[SerializeField]
		protected float				m_AlphaFadeSpeed = 1.0f ;

		public float				  AlphaFadeSpeed
		{
			get
			{
				return m_AlphaFadeSpeed ;
			}
			set
			{
				m_AlphaFadeSpeed = value ;
			}
		}

		// リスト部分の表示・隠蔽の時間(秒)
		[SerializeField]
		protected float				m_AlphaFadeDuration = 0.15f ;

		/// <summary>
		/// リスト部分の表示・隠蔽の時間(秒)
		/// </summary>
		public float				AlphaFadeDuration
		{
			get
			{
				return m_AlphaFadeDuration ;
			}
			set
			{
				m_AlphaFadeDuration = value ;
			}
		}

		/// <summary>
		/// リスト部分のプルダウンに対しての表示位置
		/// </summary>
		public enum AnchorTypes
		{
			/// <summary>
			/// 表示位置は自動選択
			/// </summary>
			Auto,

			/// <summary>
			/// 下部に固定表示
			/// </summary>
			Lower,

			/// <summary>
			/// 上部に固定表示
			/// </summary>
			Upper,
		}

		// リスト部分のプルダウンに対しての表示位置
		[SerializeField]
		protected AnchorTypes		m_AnchorType = AnchorTypes.Auto ;

		/// <summary>
		/// リスト部分のプルダウンに対しての表示位置
		/// </summary>
		public AnchorTypes			  AnchorType
		{
			get
			{
				return m_AnchorType ;
			}
			set
			{
				if( m_AnchorType != value )
				{
					m_AnchorType  = value ;
				}
			}
		}

		[SerializeField]
		protected int	m_AdditionalCanvasSortOrder = 50 ;

		/// <summary>
		/// プルダウン用のキャンパスのソートオーダーの補正値
		/// </summary>
		public int AdditionalCanvasSortOrder
		{
			get
			{
				return m_AdditionalCanvasSortOrder ;
			}
			set
			{
				m_AdditionalCanvasSortOrder = value ;
			}
		}

		[SerializeField]
		protected Color	m_MaskColor = new Color32(   0,   0,   0,   8 ) ;

		/// <summary>
		/// プルダウン用のマスクカラー
		/// </summary>
		public Color MaskColor
		{
			get
			{
				return m_MaskColor ;
			}
			set
			{
				m_MaskColor = value ;
			}
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プルダウン用のアイテム
		/// </summary>
		[Serializable]
		public class PulldownItem
		{
			/// <summary>
			/// テキスト
			/// </summary>
			public string	Text ;

			/// <summary>
			/// イメージ
			/// </summary>
			public Sprite	Image ;
		}

		// プルダウン用のアイテム
		[SerializeField]
		protected List<PulldownItem> m_PulldownItems ;

		//-------------------------------------------------------------------------------------------

		// フォーカスを処理するかどうか
		[SerializeField]
		protected bool				m_FocusProcessingEnabled = false ;

		/// <summary>
		/// フォーカスを処理するかどうか
		/// </summary>
		public bool					  FocusProcessingEnabled
		{
			get
			{
				return m_FocusProcessingEnabled ;
			}
			set
			{
				if( m_FocusProcessingEnabled != value )
				{
					m_FocusProcessingEnabled  = value ;

					if( Application.isPlaying == false || m_Pulldown == null )
					{
						return ;
					}

					if( m_FocusProcessingEnabled == false )
					{
						UpdatePulldown() ;
					}
					else
					{
						if( m_FocusIndex <  0 )
						{
							m_FocusIndex  = m_Value ;
						}
						if( m_FocusIndex <  0 )
						{
							m_FocusIndex  = 0 ;

							m_Pulldown.SetContentIndex( m_FocusIndex ) ;
						}
						else
						{
							m_Pulldown.SetFocusIndex( m_FocusIndex ) ;
						}
					}
				}
			}
		}

		// フォーカスインデックス
		[SerializeField]
		protected int				m_FocusIndex = -1 ;

		/// <summary>
		/// フォーカスインデックス
		/// </summary>
		public int					  FocusIndex
		{
			get
			{
				return m_FocusIndex ;
			}
			set
			{
				if( m_FocusIndex != value )
				{
					m_FocusIndex  = value ;

					if( Application.isPlaying == false || m_Pulldown == null )
					{
						return ;
					}

					if( m_FocusIndex >= 0 && m_FocusIndex <  m_Pulldown.ItemCount )
					{
						m_Pulldown.SetFocusIndex( m_FocusIndex ) ;
					}

					UpdatePulldown() ;
				}
			}
		}

		/// <summary>
		/// プルダウンが表示されているかどうか
		/// </summary>
		public bool IsPulldownOpened
		{
			get
			{
				return ( m_Pulldown != null ) ;
			}
		}


		//-------------------------------------------------------------------------------------------

		// 基本的なプルダウンの縦幅
		protected float					            m_BasePulldownHeight ;

		// 実際の表示用のアイテム
		protected List<PulldownItem>	            m_DisplayItems ;

		// プルダウン用のキャンバス
		protected UICanvas				            m_PulldownCanvas ;

		// プルダウン用のマスク
		protected UIImage				            m_PulldownMask ;

		// プルダウン
		protected UIListView			            m_Pulldown ;

		// コールバック
		protected Action<UIPulldown>                m_OnPulldownOpened ;

		protected Action<InputTypes,InputTypes>     m_OnInputTypeChanged ;

		protected Action<UIPulldown>                m_OnPulldownUpdate ;

		protected Action<UIPulldown>                m_OnPulldownClosed ;

		protected bool                              m_IsPulldownReady ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		protected override void OnBuild( string option = "" )
		{
			var button = CButton != null ? CButton : gameObject.AddComponent<Button>() ;
			if( button == null )
			{
				// 異常
				return ;
			}

			var image  = CImage ;

			//------------------------------------------

			var size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				SetSize( size.y * 0.28f, size.y * 0.05f ) ;
			}

			var colorBlock = button.colors ;
			colorBlock.fadeDuration = 0.1f ;
			button.colors = colorBlock ;

			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			image.color = Color.white ;
			image.type = Image.Type.Sliced ;

			//----------------------------------
			// インタラクション用意

			button.interactable = true ;
			image.raycastTarget = true ;

			//-------------------------------------------------

			// Label
			var captionText = AddView<UITextMesh>( "Label" ) ;
			captionText.Text = "Element 0" ;
			captionText.SetAnchorToStretch() ;
			captionText.SetMargin( 24, 24,  6,  6 ) ;
			captionText.Alignment = TextAlignmentOptions.MidlineLeft ;
			captionText.Color = new Color32(  50,  50,  50, 255 ) ;

			captionText.AutoSizeFitting = false ;
			captionText.HorizontalOverflow = false ;

			m_CaptionText = captionText ;


			// Arrow
			var arrow = AddView<UIImage>( "Arrow" ) ;
			arrow.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultArrowDown" ) ;
			arrow.Color = Color.white ;
			arrow.Type = Image.Type.Sliced ;

			arrow.SetAnchorToRightMiddle() ;
			arrow.SetPosition( -40,   0 ) ;
			float s = this.Height * 0.6f ;
			arrow.SetSize( s, s ) ;


			//-------------------------------------------------

			// ListView
			var listView = AddView<UIListView>( "Template", "VS" ) ;
			listView.SetAnchorToStretchBottom() ;
			listView.SetPosition(  0,  2 ) ;
			listView.SetSize(   0, this.Height * 5 ) ;
			listView.SetPivot( 0.5f, 1.0f ) ;

			listView.Viewport.IsMask = false ;
			listView.Viewport.IsRectMask2D = true ;
			listView.Viewport.IsAlphaMaskWindow = false ;

			listView.Content.Height = this.Height ;


			listView.IsCanvasGroup = true ;

			var fadeIn = listView.AddTween( "FadeIn" ) ;
			fadeIn.Duration = m_AlphaFadeDuration ;
			fadeIn.AlphaEnabled = true ;
			fadeIn.AlphaFrom = 0 ;
			fadeIn.AlphaTo = 1 ;

			var fadeOut = listView.AddTween( "FadeOut" ) ;
			fadeOut.Duration = m_AlphaFadeDuration ;
			fadeOut.AlphaEnabled = true ;
			fadeOut.AlphaFrom = 1 ;
			fadeOut.AlphaTo = 0 ;

			// 最後に隠蔽
			listView.SetActive( false ) ;

			m_Template = listView ;

			//----------------------------------
			// アイテム編集

			var item = listView.Item ;
			var pulldownItem = item.AddComponent<UIPulldownItem>() ;

			// Highlight
			var highlight = item.AddView<UIImage>( "Highlight" ) ;
			highlight.SetAnchorToStretch() ;
			highlight.Color = new Color32( 224, 224, 224, 255 ) ;

			if( IsCanvasOverlay == true )
			{
				highlight.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
			}

			pulldownItem.Highlight = highlight ;

			// PadCursor
			var padCursor = item.AddView<UIImage>( "PadCursor" ) ;
			padCursor.SetAnchorToStretch() ;
			padCursor.Color = new Color32(   0, 255, 255, 255 ) ;
			padCursor.SetActive( false ) ;

			if( IsCanvasOverlay == true )
			{
				padCursor.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
			}

			pulldownItem.PadCursor = padCursor ;

			// Checkmark
			var checkmark = item.AddView<UIToggle>( "Checkmark", "Simple" ) ;
			checkmark.SetAnchorToLeftMiddle() ;
			checkmark.SetPivot( 0.0f, 0.5f ) ;
			checkmark.SetPosition( item.Height * 0.25f, 0 ) ;
			checkmark.SetSize( item.Height * 0.75f, item.Height * 0.75f ) ;
			checkmark.Background.Enabled = false ;
			
			pulldownItem.Checkmark = checkmark ;

			// CaptionText
			var itemCaptionText = item.AddView<UITextMesh>( "Label" ) ;
			itemCaptionText.Text = "Element 0" ;
			itemCaptionText.SetAnchorToStretch() ;
			itemCaptionText.SetMargin( item.Height * 1.25f, item.Height,  0,  0 ) ;
			itemCaptionText.Alignment = TextAlignmentOptions.MidlineLeft ;
			itemCaptionText.Color = new Color32(  50,  50,  50, 255 ) ;
			
			itemCaptionText.AutoSizeFitting = false ;
			itemCaptionText.HorizontalOverflow = false ;

			pulldownItem.CaptionText = itemCaptionText ;

			//-------------------------------------------------

			ResetRectTransform() ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				if( m_Template != null )
				{
					m_BasePulldownHeight = m_Template.Height ;

					m_Template.SetActive( false ) ;
				}

				AddOnButtonClick( OnPulldownOpen ) ;
			}
		}


		protected override void OnEnable()
		{
			base.OnEnable() ;

			UIEventSystem.AddOnInputTypeChanged( OnInputTypeChanged ) ;
		}

		protected override void OnDisable()
		{
			base.OnDisable() ;

			UIEventSystem.RemoveOnInputTypeChanged( OnInputTypeChanged ) ;
		}

		protected void OnInputTypeChanged( InputTypes basisInputType, InputTypes extraInputType )
		{
			m_OnInputTypeChanged?.Invoke( basisInputType, extraInputType ) ;

			UpdatePulldown() ;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( m_PulldownCanvas != null && m_IsPulldownReady == true )
			{
				m_OnPulldownUpdate?.Invoke( this ) ;
			}
		}

		// プルダウンボタンがタップされた際に呼び出される
		protected virtual void OnPulldownOpen( string identity, UIButton button )
		{
			if( m_Template != null )
			{
				if( m_Pulldown == null )
				{
					Open() ;
				}
				else
				{
					Close() ;
				}
			}
		}

		// リストのアイテムが選択された際に呼び出される
		protected virtual void OnItemSelected( int index )
		{
			Decision( index ) ;
		}

		protected virtual void OnItemEntering( int index )
		{
			if( m_FocusProcessingEnabled == true )
			{
				if( UIEventSystem.InputType == InputTypes.Pointer )
				{
					m_FocusIndex = index ;
				}
			}
		}

		/// <summary>
		/// 決定アクションを行う
		/// </summary>
		/// <param name="index"></param>
		public void Decision( int index )
		{
			if( m_MultiSelect == false )
			{
				// 単一選択
				m_Value = index ;

				UpdateCaption() ;

				if( m_Pulldown != null )
				{
					Close() ;
				}

				OnValueChangedInner( m_Value ) ;
			}
			else
			{
				// 複数選択
				if( index == 0 )
				{
					m_Values = null ;
				}
				else
				if( index == 1 )
				{
					if( m_PulldownItems != null && m_PulldownItems.Count >  0 )
					{
						m_Values = new List<int>() ;

						int i, l = m_PulldownItems.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							m_Values.Add( i ) ;
						}
					}
				}
				else
				if( index >= 2 )
				{
					int pulldownIndex = index - 2 ;
					if( pulldownIndex <  m_PulldownItems.Count )
					{
						if( m_Values != null )
						{
							if( m_Values.Contains( pulldownIndex ) == true )
							{
								m_Values.Remove( pulldownIndex ) ;
								if( m_Values.Count == 0 )
								{
									m_Values = null ;
								}
							}
							else
							{
								m_Values.Add( pulldownIndex ) ;
							}
						}
						else
						{
							m_Values ??= new () ;
							m_Values.Add( pulldownIndex ) ;
						}

						if( m_Values != null )
						{
							m_Values = m_Values.OrderBy( _ => _ ).ToList() ;
						}
					}
				}

				UpdateCaption() ;

				if( m_Pulldown != null )
				{
					m_Pulldown.Restore() ;
				}

				OnValuesChangedInner( m_Values?.ToArray() ) ;
			}
		}

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 入力タイプが変化した際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChanged"></param>
		public void SetOnInputTypeChanged( Action<InputTypes,InputTypes> onInputTypeChanged )
		{
			m_OnInputTypeChanged    = onInputTypeChanged ;
		}

		/// <summary>
		/// プルダウンリストが開かれた際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onPulldownOpened"></param>
		public void SetOnPulldownOpened( Action<UIPulldown> onPulldownOpened )
		{
			m_OnPulldownOpened      = onPulldownOpened ;
		}

		/// <summary>
		/// プルダウンリストが表示されている際に毎フレーム呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onInputTypeChanged"></param>
		public void SetOnPulldownUpdate( Action<UIPulldown> onPulldownUpdate )
		{
			m_OnPulldownUpdate      = onPulldownUpdate ;
		}

		/// <summary>
		/// プルダウンリストが閉じられた際に呼び出すコールバックを設定する
		/// </summary>
		/// <param name="onPulldownClose"></param>
		public void SetOnPulldownClosed( Action<UIPulldown> onPulldownClosed )
		{
			m_OnPulldownClosed      = onPulldownClosed ;
		}

		/// <summary>
		/// コールバック群をまとめて設定する
		/// </summary>
		/// <param name="onInputTypeChanged"></param>
		/// <param name="onPulldownOpened"></param>
		/// <param name="onPulldownUpdate"></param>
		/// <param name="onPulldownClosed"></param>
		public void SetCallbacks
		(
			Action<InputTypes,InputTypes> onInputTypeChanged,
			Action<UIPulldown> onPulldownOpened,
			Action<UIPulldown> onPulldownUpdate,
			Action<UIPulldown> onPulldownClosed
		)
		{
			m_OnInputTypeChanged    = onInputTypeChanged ;
			m_OnPulldownOpened      = onPulldownOpened ;
			m_OnPulldownUpdate      = onPulldownUpdate ;
			m_OnPulldownClosed      = onPulldownClosed ;
		}

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 項目を表示する
		/// </summary>
		/// <returns></returns>
		public void Open()
		{
			if( Application.isPlaying == false )
			{
				return ;
			}

			if( m_Template != null && m_PulldownCanvas == null )
			{
				CreatePulldown() ;

				if( m_PulldownCanvas != null )
				{
					m_PulldownCanvas.Alpha = 1.0f ;
					m_PulldownCanvas.TimeScale = m_AlphaFadeSpeed ;
					m_PulldownCanvas.PlayTween( "FadeIn", onFinishedAction: ( string identity, UITween tween ) =>
					{
						m_PulldownCanvas.TimeScale = 1 ;

						m_IsPulldownReady = true ;

						m_OnPulldownOpened?.Invoke( this ) ;
					} ) ;
				}
			}
		}

		/// <summary>
		/// 項目を隠蔽する
		/// </summary>
		/// <returns></returns>
		public void Close()
		{
			if( Application.isPlaying == false )
			{
				return ;
			}

			if( m_PulldownCanvas != null )
			{
				Destroy( m_PulldownCanvas.gameObject ) ;

				m_PulldownCanvas	= null ;
				m_PulldownMask		= null ;
				m_Pulldown			= null ;

				m_OnPulldownClosed?.Invoke( this ) ;

				m_IsPulldownReady   = false ;
			}
		}

		// キャプションを更新する
		protected void UpdateCaption()
		{
			if( m_MultiSelect == false )
			{
				// 単一選択

				if( m_Value >= 0 && m_Value <  m_PulldownItems.Count )
				{
					if( m_CaptionText != null )
					{
						if( string.IsNullOrEmpty( m_PulldownItems[ m_Value ].Text ) == false )
						{
							m_CaptionText.SetActive( true ) ;
							m_CaptionText.Text = m_PulldownItems[ m_Value ].Text ;
						}
						else
						{
							m_CaptionText.SetActive( true ) ;
							m_CaptionText.Text = "Unknown" ;
						}
					}

					if( m_CaptionImage != null )
					{
						if( m_PulldownItems[ m_Value ].Image != null )
						{
							m_CaptionImage.SetActive( true ) ;
							m_CaptionImage.Sprite = m_PulldownItems[ m_Value ].Image ;
						}
						else
						{
							m_CaptionImage.SetActive( false ) ;
						}
					}
				}
				else
				{
					if( m_CaptionText != null )
					{
						m_CaptionText.SetActive( true ) ;
						m_CaptionText.Text = "Unknown" ;
					}

					if( m_CaptionImage != null )
					{
						m_CaptionImage.SetActive( false ) ;
					}
				}
			}
			else
			{
				// 複数選択

				if( m_PulldownItems != null && m_PulldownItems.Count >  0 )
				{
					if( m_Values == null || m_Values.Count == 0 )
					{
						if( m_CaptionText != null )
						{
							m_CaptionText.SetActive( true ) ;
							m_CaptionText.Text = m_NothingText ;
						}
					}
					else
					if( m_Values.Count == m_PulldownItems.Count )
					{
						if( m_CaptionText != null )
						{
							m_CaptionText.SetActive( true ) ;
							m_CaptionText.Text = m_EverythingText ;
						}
					}
					else
					{
						if( m_CaptionText != null )
						{
							m_CaptionText.SetActive( true ) ;
							m_CaptionText.Text = m_MixedText ;
						}
					}
				}
				else
				{
					if( m_CaptionText != null )
					{
						m_CaptionText.SetActive( true ) ;
						m_CaptionText.Text = m_NothingText ;
					}
				}

				if( m_CaptionImage != null )
				{
					m_CaptionImage.SetActive( false ) ;
				}
			}
		}


		// プルダウンの表示位置を設定する
		protected void CreatePulldown()
		{
			var ownerCanvas = GetParentCanvas() ;
			ownerCanvas.TryGetComponent<CanvasScaler>( out var ownerCanvasScaler ) ;

			m_PulldownCanvas = UICanvas.Create
			(
				ownerCanvasScaler.referenceResolution.x,
				ownerCanvasScaler.referenceResolution.y
			) ;
			m_PulldownCanvas.name = "Blocker" ;

			m_PulldownCanvas.SetAnchorToStretch() ;
			m_PulldownCanvas.GetCanvas().overrideSorting = true ;
			m_PulldownCanvas.SetPositionZ( 0 ) ;

			var scaler = m_PulldownCanvas.GetCanvasScaler() ;
			scaler.screenMatchMode = ownerCanvasScaler.screenMatchMode ;

			m_PulldownCanvas.SortingOrder = ownerCanvas.sortingOrder + m_AdditionalCanvasSortOrder ;

			m_PulldownCanvas.IsCanvasGroup = true ;
			m_PulldownCanvas.SetActive( false ) ;

			var fadeIn = m_PulldownCanvas.AddTween( "FadeIn" ) ;
			fadeIn.Duration = m_AlphaFadeDuration ;
			fadeIn.AlphaEnabled = true ;
			fadeIn.AlphaFrom = 0 ;
			fadeIn.AlphaTo = 1 ;

			var fadeOut = m_PulldownCanvas.AddTween( "FadeOut" ) ;
			fadeOut.Duration = m_AlphaFadeDuration ;
			fadeOut.AlphaEnabled = true ;
			fadeOut.AlphaFrom = 1 ;
			fadeOut.AlphaTo = 0 ;

			//----------------------------------

			m_PulldownMask = m_PulldownCanvas.AddView<UIImage>( "Mask" ) ;
			m_PulldownMask.SetActive( true ) ;
			m_PulldownMask.Color = m_MaskColor;
			m_PulldownMask.SetAnchorToStretch() ;
			m_PulldownMask.SetPivot( 0.5f, 0.5f ) ;

			m_PulldownMask.RaycastTarget = true ;
			m_PulldownMask.IsInteraction = true ;
			m_PulldownMask.SetOnSimplePress( ( bool isPress ) =>
			{
				if( isPress == true )
				{
					Close() ;
				}
			} ) ;

			//----------------------------------

			var ownerCanvasSize = GetCanvasSize() ;

			float canvasYMin = ownerCanvasSize.y * -0.5f ;
			float canvasYMax = ownerCanvasSize.y * +0.5f ;

			AnchorTypes anchorType = m_AnchorType ;

			// ボタン部分の領域を取得する
			var br = RectInCanvas ;

			float yMin = br.yMin ;
			float yMax = br.yMax ;

			var tr = m_Template.RectInCanvas ;

			float width = tr.width ;
			float x     = ( tr.xMin + tr.xMax ) * 0.5f ;

			// 実縦幅を取得する
			float height = GetPulldownHeight() ;

			if( anchorType == AnchorTypes.Auto )
			{
				if( ( yMin - height ) <  canvasYMin )
				{
					// Upper に変更
					anchorType = AnchorTypes.Upper ;
				}
				else
				if( ( yMax + height ) >  canvasYMax )
				{
					// Lower に変更
					anchorType = AnchorTypes.Lower ;
				}
				else
				{
					// デフォルトでは下に表示
					anchorType = AnchorTypes.Lower ;
				}
			}

			m_Pulldown = m_PulldownCanvas.AddPrefab<UIListView>( m_Template.gameObject ) ;
			m_Pulldown.name = "Template" ;
			m_Pulldown.SetActive( true ) ;

			m_Pulldown.SetAnchorToCenter() ;

			if( anchorType == AnchorTypes.Lower )
			{
				// 下に表示

				float y = br.yMin ;

				m_Pulldown.SetPivot( 0.5f, 1.0f ) ;
				m_Pulldown.SetPosition( x, y ) ;
			}
			else
			if( anchorType == AnchorTypes.Upper )
			{
				// 上に表示

				float y = br.yMax ;

				m_Pulldown.SetPivot( 0.5f, 0.0f ) ;
				m_Pulldown.SetPosition( x, y ) ;
			}

			m_Pulldown.Width  = width ;
			m_Pulldown.Height = height ;

			//----------------------------------

			UpdatePulldown() ;

			if( m_MultiSelect == false )
			{
				// 単一選択

				if( m_Value <  0 || m_Value >= m_PulldownItems.Count )
				{
					m_Value  = 0 ;
					m_FocusIndex = m_Value ;

					m_Pulldown.SetContentIndex( m_Value ) ;
				}
				else
				{
					m_FocusIndex = m_Value ;

					m_Pulldown.SetContentIndex( m_Value ) ;
				}
			}
			else
			{
				// 複数選択

				m_FocusIndex = 0 ;

				// 必ずトップを表示
				m_Pulldown.SetContentIndex( 0 ) ;
			}
		}

		// プルダウンを更新する
		protected virtual void UpdatePulldown()
		{
			if( Application.isPlaying == false )
			{
				return ;
			}

			if( m_Pulldown == null )
			{
				return ;
			}

			//----------------------------------

			if( m_MultiSelect == false )
			{
				// 単一選択

				if( m_PulldownItems != null && m_PulldownItems.Count >  0 )
				{
					m_Pulldown.SetOnItemUpdated<UIPulldownItem>( ( string identity, UIListView listView, int index, Component component ) =>
					{
						if( component != null )
						{
							var itemView = component as UIPulldownItem ;

							bool isCheck = ( index == m_Value ) ;

							itemView.SetStyle
							(
								index,
								m_PulldownItems[ index ],
								UIEventSystem.InputType == InputTypes.Pointer,
								UIEventSystem.InputType == InputTypes.GamePad && m_FocusProcessingEnabled == true && index == m_FocusIndex,
								isCheck,
								OnItemSelected, OnItemEntering
							) ;
						}

						return 0 ;
					} ) ;

					if( m_Pulldown.ItemCount != m_PulldownItems.Count )
					{
						m_Pulldown.ItemCount  = m_PulldownItems.Count ;
						m_Pulldown.Refresh() ;
					}
					else
					{
						m_Pulldown.Restore() ;
					}

					m_Pulldown.Height = GetPulldownHeight() ;
				}
				else
				{
					m_Pulldown.ItemCount = 0 ;
				}
			}
			else
			{
				// 複数選択

				if( m_DisplayItems == null )
				{
					m_DisplayItems = new () ;
				}
				else
				{
					m_DisplayItems.Clear() ;
				}

				m_DisplayItems.Add( new ()
				{
					Text = NothingText,
				} ) ;

				m_DisplayItems.Add( new ()
				{
					Text = EverythingText
				} ) ;

				if( m_PulldownItems != null && m_PulldownItems.Count >  0 )
				{
					foreach( var pulldownItem in m_PulldownItems )
					{
						m_DisplayItems.Add( new ()
						{
							Text	= pulldownItem.Text,
							Image	= pulldownItem.Image,
						} ) ;
					}
				}

				m_Pulldown.SetOnItemUpdated<UIPulldownItem>( ( string identity, UIListView listView, int index, Component component ) =>
				{
					if( component != null )
					{
						var itemView = component as UIPulldownItem ;

						bool isCheck = false ;

						if( index == 0 )
						{
							isCheck = ( m_Values == null || m_Values.Count == 0 ) ;
						}
						else
						if( index == 1 && m_Values != null && m_Values.Count >  0 )
						{
							isCheck = ( m_Values.Count == m_PulldownItems.Count ) ;
						}
						else
						if( index >= 2 && m_Values != null && m_Values.Count >  0 )
						{
							isCheck = m_Values.Contains( index - 2 ) ;
						}

						itemView.SetStyle
						(
							index,
							m_DisplayItems[ index ],
							UIEventSystem.InputType == InputTypes.Pointer,
							UIEventSystem.InputType == InputTypes.GamePad && m_FocusProcessingEnabled == true && index == m_FocusIndex,
							isCheck,
							OnItemSelected,
							OnItemEntering
						) ;
					}

					return 0 ;
				} ) ;

				if( m_Pulldown.ItemCount != m_DisplayItems.Count )
				{
					m_Pulldown.ItemCount  = m_DisplayItems.Count ;
					m_Pulldown.Refresh() ;
				}
				else
				{
					m_Pulldown.Restore() ;
				}

				m_Pulldown.Height = GetPulldownHeight() ;
			}
		}

		/// <summary>
		/// 表示を更新する
		/// </summary>
		public void Refresh()
		{
			UpdateCaption() ;

			if( m_Pulldown != null )
			{
				UpdatePulldown() ;
			}
		}

		// プルダウンの実縦幅を取得する
		protected float GetPulldownHeight()
		{
			if( m_Template == null )
			{
				return 0 ;
			}

			// ※ Content サイズは、Viewport サイズより小さい場合は Viewport サイズと同じになってしまう。
			float itemHeight = m_Template.Item.Height ;
			float viewHeight = m_PulldownItems.Count * itemHeight ;

			// ビューポートのマージンがあれば加える
			m_Template.Viewport.GetMarginY( out float tm, out float bm ) ;
			viewHeight += ( tm + bm ) ;

			return Mathf.Min( viewHeight, m_BasePulldownHeight ) ;
		}


		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 選択項目を設定する
		/// </summary>
		public void SetValue( int value, bool isCallbackEnabled = true )
		{
			// 単一選択

			m_Value = value ;

			Refresh() ;

			//----------------------------------

			if( isCallbackEnabled == true )
			{
				OnValueChangedInner( m_Value ) ;
			}
		}

		/// <summary>
		/// 選択項目を設定する
		/// </summary>
		public void SetValues( int[] values, bool isCallbackEnabled = true )
		{
			// 単一選択

			if( m_Values == null )
			{
				m_Value  = new () ;
			}
			else
			{
				m_Values.Clear() ;
			}

			m_Values.AddRange( values ) ;

			Refresh() ;

			//----------------------------------

			if( isCallbackEnabled == true )
			{
				OnValuesChangedInner( m_Values?.ToArray() ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// フォーカスインデックスを設定する
		/// </summary>
		/// <returns></returns>
		public void SetFocusIndex( int index )
		{
			FocusIndex = index ;
		}

		/// <summary>
		/// フォーカスインデックスを移動する
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="isPage"></param>
		public void MoveFocusIndex( int direction, bool isPage = false )
		{
			if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
			{
				return ;	// 移動不可
			}

			int count ;

			if( m_MultiSelect == false )
			{
				// 単一選択
				count = m_PulldownItems.Count ;
			}
			else
			{
				// 複数選択
				count = 2 + m_PulldownItems.Count ;
			}

			if ( isPage == false )
			{
				// アイテム単位

				int focusIndex = m_FocusIndex ;

				if( focusIndex <  0 )
				{
					focusIndex  = 0 ;
				}
				else
				if( focusIndex >= count )
				{
					focusIndex  = count - 1 ;
				}
				else
				{
					focusIndex = ( focusIndex + direction + count ) % count ;
				}

				FocusIndex = focusIndex ;
			}
			else
			{
				// ページ単位

				if( m_Template != null )
				{
					float itemSize = m_Template.DefaultItemSize ;
					float viewSize = Mathf.Min( m_Template.ViewSize, itemSize * count ) ;

					int pageItemCount = ( int )( viewSize / itemSize ) ;

					int focusIndex = m_FocusIndex ;

					if( focusIndex <  0 )
					{
						focusIndex  = 0 ;
					}
					else
					if( focusIndex >= count )
					{
						focusIndex  = count - 1 ;
					}
					else
					{
						if( focusIndex == 0 && direction <  0 )
						{
							focusIndex  = count - 1 ;
						}
						else
						if( focusIndex == ( count - 1 ) && direction >  0 )
						{
							focusIndex  = 0 ;
						}
						else
						{
							focusIndex += ( pageItemCount * direction ) ;

							if( focusIndex <  0 )
							{
								focusIndex  = 0 ;
							}
							else
							if( focusIndex >= count )
							{
								focusIndex  = count - 1 ;
							}
						}
					}

					FocusIndex = focusIndex ;
				}
			}
		}

		//-------------------------------------------------------------------------------------
		// 単一選択

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIPulldown, int> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="value">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIPulldown view, int value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIPulldown, int> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}

		// 内部リスナー登実行
		private void OnValueChangedInner( int value )
		{
			if( OnValueChangedAction != null || OnValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValueChangedAction?.Invoke( identity, this, value ) ;
				OnValueChangedDelegate?.Invoke( identity, this, value ) ;
			}
		}

		//-------------------------------------------------------------------------------------
		// 複数選択

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIPulldown, int[]> OnValuesChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="identity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="view">ビューのインスタンス</param>
		/// <param name="value">変化後の値</param>
		public delegate void OnValuesChanged( string identity, UIPulldown view, int[] values ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValuesChanged OnValuesChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="onValueChangedAction">アクションメソッド</param>
		public void SetOnValuesChanged( Action<string, UIPulldown, int[]> onValuesChangedAction )
		{
			OnValuesChangedAction = onValuesChangedAction ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValuesChanged( OnValuesChanged onValuesChangedDelegate )
		{
			OnValuesChangedDelegate += onValuesChangedDelegate ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="onValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValuesChanged( OnValuesChanged onValuesChangedDelegate )
		{
			OnValuesChangedDelegate -= onValuesChangedDelegate ;
		}

		// 内部リスナー実行
		private void OnValuesChangedInner( int[] values )
		{
			if( OnValuesChangedAction != null || OnValuesChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValuesChangedAction?.Invoke( identity, this, values ) ;
				OnValuesChangedDelegate?.Invoke( identity, this, values ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// オプションデータをまとめて設定する
		/// </summary>
		/// <param name="dataTexts"></param>
		public void Set( string[] dataTexts, int initailValue = -1 )
		{
			var items = new List<PulldownItem>() ;

			if( dataTexts != null && dataTexts.Length >   0 )
			{
				foreach( var dataText in dataTexts )
				{
					items.Add( new PulldownItem()
					{
						Text	= dataText,
						Image	= null
					} ) ;
				}
			}

			m_PulldownItems = items ;

			m_Value = initailValue ;

			Refresh() ;
		}

		/// <summary>
		/// オプションデータを追加する
		/// </summary>
		/// <param name="dataText"></param>
		/// <returns></returns>
		public void Add( string dataText )
		{
			m_PulldownItems ??= new List<PulldownItem>() ;

			m_PulldownItems.Add( new PulldownItem()
			{
				Text = dataText,
			} ) ;

			Refresh() ;
		}

		/// <summary>
		/// オプションデータを挿入する
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dataText"></param>
		/// <returns></returns>
		public void Insert( int index, string dataText )
		{
			m_PulldownItems ??= new List<PulldownItem>() ;

			m_PulldownItems.Insert( index, new PulldownItem()
			{
				Text = dataText,
			} ) ;

			Refresh() ;
		}

		/// <summary>
		/// オプションデータを削除する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public void RemoveAt( int index )
		{
			if( m_PulldownItems != null && m_PulldownItems.Count >  index )
			{
				m_PulldownItems.RemoveAt( index ) ;

				Refresh() ;
			}
		}

		/// <summary>
		/// 現在のカーソル位置の項目名
		/// </summary>
		public string DataText
		{
			get
			{
				if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
				{
					return string.Empty ;
				}

				if( m_Value <  0 || m_Value >  m_PulldownItems.Count )
				{
					return string.Empty ;
				}

				return m_PulldownItems[ m_Value ].Text ;
			}
			set
			{
				if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
				{
					return ;
				}

				if( m_Value <  0 || m_Value >  m_PulldownItems.Count )
				{
					return ;
				}

				m_PulldownItems[ m_Value ].Text = value ;

				Refresh() ;
			}
		}

		/// <summary>
		/// 項目一覧を取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetDataTexts()
		{
			if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
			{
				return null ;
			}

			return m_PulldownItems.Select( _ => _.Text ).ToArray() ;
		}

		/// <summary>
		/// インデクサ(項目一覧へのショートカットアクセス)
		/// </summary>
		public string this[ int index ]
		{
			get
			{
				if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
				{
					return null ;
				}

				if( index <  0 || index >  m_PulldownItems.Count )
				{
					return string.Empty ;
				}

				return m_PulldownItems[ index ].Text ;
			}
			set
			{
				if( m_PulldownItems == null || m_PulldownItems.Count == 0 )
				{
					return ;
				}

				if( index <  0 || index >  m_PulldownItems.Count )
				{
					return ;
				}

				m_PulldownItems[ index ].Text = value ;

				Refresh() ;
			}
		}
	}
}
