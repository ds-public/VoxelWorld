using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:InputField クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent(typeof(UnityEngine.UI.InputFieldPlus))]	
	public class UIInputField : UIImage
	{
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool Interactable
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.interactable ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.interactable = value ;
			}
		}

		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string Text
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return "" ;
				}

				return inputField.text ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.text = value ;
			}
		}
		
		/// <summary>
		/// キャラクターリミット(ショートカット)
		/// </summary>
		public int CharacterLimit
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.characterLimit ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.characterLimit = value ;
			}
		}
	
		/// <summary>
		/// コンテントタイプ(ショートカット)
		/// </summary>
		public InputFieldPlus.ContentTypes ContentType
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return InputFieldPlus.ContentTypes.Standard ;
				}
				return inputField.contentType ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}

				if( inputField.contentType != value )
				{
					inputField.contentType = value ;

					string text = inputField.text ;
					inputField.text = "" ;
					inputField.text = text ;
				}
			}
		}

		/// <summary>
		/// ラインタイプ(ショートカット)
		/// </summary>
		public InputFieldPlus.LineTypes LineType
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return InputFieldPlus.LineTypes.SingleLine ;
				}
				return inputField.lineType ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.lineType = value ;
			}
		}

		/// <summary>
		/// テキストコンポーネント(ショートカット)
		/// </summary>
		public UIText TextComponent
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return null ;
				}
				return inputField.textComponent.GetComponent<UIText>() ;
			}
		}

		/// <summary>
		/// プレースホルダー(ショートカット)
		/// </summary>
		public UIText Placeholder
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return null ;
				}
				return inputField.placeholder.GetComponent<UIText>() ;
			}
		}

		/// <summary>
		/// キャレットブリンクレート(ショートカット)
		/// </summary>
		public float CaretBlinkRate
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.caretBlinkRate ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretBlinkRate = value ;
			}
		}

		/// <summary>
		/// キャレットウィドス(ショートカット)
		/// </summary>
		public int CaretWidth
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return 0 ;
				}
				return inputField.caretWidth ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretWidth = value ;
			}
		}

		/// <summary>
		/// カスタムキャレットカラー(ショートカット)
		/// </summary>
		public bool CustomCaretColor
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.customCaretColor ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.customCaretColor = value ;
			}
		}

		/// <summary>
		/// キャレットカラー(ショートカット)
		/// </summary>
		public Color CaretColor
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return Color.black ;
				}
				return inputField.caretColor ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.caretColor = value ;
			}
		}

		/// <summary>
		/// セレクションカラー(ショートカット)
		/// </summary>
		public Color SelectionColor
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return Color.black ;
				}
				return inputField.selectionColor;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.selectionColor = value ;
			}
		}

		/// <summary>
		/// ハイドモバイルインプット(ショートカット)
		/// </summary>
		public bool HideMobileInput
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.shouldHideMobileInput ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.shouldHideMobileInput = value ;
			}
		}


		/// <summary>
		/// リードオンリー(ショートカット)
		/// </summary>
		public bool ReadOnly
		{
			get
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return false ;
				}
				return inputField.readOnly ;
			}
			set
			{
				InputFieldPlus inputField = _inputField ;
				if( inputField == null )
				{
					return ;
				}
				inputField.readOnly = value ;
			}
		}

		/// <summary>
		/// フォントサイズを設定する
		/// </summary>
		/// <param name="tFontSize"></param>
		public void SetFontSize( int fontSize )
		{
			if( TextComponent != null )
			{
				TextComponent.FontSize = fontSize ;
			}

			if( Placeholder != null )
			{
				Placeholder.FontSize = fontSize ;
			}
		}

		[SerializeField]
		protected FontFilter	m_FontFilter = null ;
		public FontFilter		  FontFilter{ get{ return m_FontFilter ; } set{ m_FontFilter = value ; } }

		[SerializeField]
		protected char			m_FontAlternateCode = ( char )0 ;
		public char				  FontAlternateCode{ get{ return m_FontAlternateCode ; } set{ m_FontAlternateCode = value ; } }

		//-----------------------------------

		/// <summary>
		/// フォーカスを持たせる
		/// </summary>
		public bool Activate()
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField == null )
			{
				return false ;
			}
			inputField.ActivateInputField() ;

			return true ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			InputFieldPlus inputField = _inputField ;

			if( inputField == null )
			{
				inputField = gameObject.AddComponent<InputFieldPlus>() ;
			}
			if( inputField == null )
			{
				// 異常
				return ;
			}

			Image image = _image ;
			if( image != null )
			{
				inputField.targetGraphic = image ;
			}

			//-------------------------------
			
			bool isMultiLine = false ;
			if( string.IsNullOrEmpty( option ) == false && option.ToLower() == "multiline" )
			{
				// マルチ
				isMultiLine = true ;
			}


			Vector2 size = GetCanvasSize() ;

			int fontSize = 16 ;
			if( size.x >  0 && size.y >  0 )
			{
				if( isMultiLine == false )
				{
					// シングル
					SetSize( size.y * 0.5f, size.y * 0.1f ) ;
				}
				else
				{
					// マルチ
					SetSize( size.y * 0.5f, size.y * 0.5f ) ;
				}

				fontSize = ( int )( size.y * 0.1f * 0.6f ) ;
			}
				
			// Image
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			image.type = Image.Type.Sliced ;
				
			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
				
			// Text
			UIText textComponent = AddView<UIText>( "Text", "SIMPLE" ) ;
			textComponent.isContentSizeFitter = false ;
			textComponent.FontSize = fontSize ;
			textComponent.SupportRichText = false ;
			textComponent.Color = new Color32(  50,  50,  50,  255 ) ;
			textComponent.SetAnchorToStretch() ;
			textComponent.SetMargin( 12, 12, 12, 12 ) ;
//			text.position = new Vector2( 0, -2 ) ;
//			text.SetSize( -24, -28 ) ;
//			text.resizeTextForBestFit = true ;
			inputField.textComponent = textComponent._text ;
			if( isMultiLine == false )
			{
				textComponent.Alignment = TextAnchor.MiddleLeft ;
			}
			else
			{
				textComponent.Alignment = TextAnchor.UpperLeft ;
			}
				
			if( IsCanvasOverlay == true )
			{
				textComponent.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			// TextColorModifier
			textComponent.AddComponent<TextColorModifier>() ;

			// Placeholder
			UIText placeholder = AddView<UIText>( "Placeholder", "SIMPLE" ) ;
			placeholder.FontSize = fontSize ;
			placeholder.FontStyle = FontStyle.Italic ;
			placeholder.Text = "Enter text..." ;
			placeholder.Color = new Color32(  50,  50,  50, 128 ) ;
			placeholder.SetAnchorToStretch() ;
			placeholder.SetMargin( 12, 12, 12, 12 ) ;
//			placeholder.position = new Vector2( 0, -2 ) ;
//			placeholder.SetSize( -24, -28 ) ;
//			placeholder.resizeTextForBestFit = true ;
			inputField.placeholder = placeholder._text ;
			if( isMultiLine == false )
			{
				placeholder.Alignment = TextAnchor.MiddleLeft ;
			}
			else
			{
				placeholder.Alignment = TextAnchor.UpperLeft ;
			}

			if( IsCanvasOverlay == true )
			{
				placeholder.Material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}


			if( isMultiLine == true )
			{
				// マルチラインで生成する
				inputField.lineType = InputFieldPlus.LineTypes.MultiLineNewline ;
				inputField.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap ;
			}

			inputField.caretWidth = 4 ;
			inputField.customCaretColor = true ;
			inputField.caretColor = Color.blue ;

			//----------------------------------------------------------

			FontFilter	fontFilter = null ;
			char		fontAlternateCode = '？' ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings ds = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( ds != null )
				{
					fontFilter			= ds.fontFilter ;
					fontAlternateCode	= ds.fontAlternateCode ;
				}
			}
			
#endif
			if( fontFilter == null )
			{

			}
			else
			{
				m_FontFilter = fontFilter ;
			}

			if( fontAlternateCode == 0 )
			{
				m_FontAlternateCode = '？' ;
			}
			else
			{
				m_FontAlternateCode = fontAlternateCode ;
			}
		}

		private UITextColor m_TextColor = null ;

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _inputField != null )
				{
					_inputField.onValueChanged.AddListener( OnValueChangedInner ) ;
					_inputField.onEndEdit.AddListener( OnEndEditInner ) ;

					if( _inputField.textComponent != null )
					{
						m_TextColor = _inputField.textComponent.GetComponent<UITextColor>() ;
						if( m_TextColor == null )
						{
							m_TextColor = _inputField.textComponent.gameObject.AddComponent<UITextColor>() ;
						}
					}
				}
			}
		}

		// キャレットのＶＲモード対応
		private LayoutElement m_LE = null ;

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			if( Application.isPlaying == true )
			{
				// キャレットが表示されなくなるバグ対策
				if( _inputField != null && m_LE == null )
				{
					m_LE =	_inputField.GetComponentInChildren<LayoutElement>() ;
					if( m_LE != null )
					{
						CanvasRenderer cr0 = m_LE.GetComponent<CanvasRenderer>() ;
						CanvasRenderer cr1 = _inputField.GetComponent<CanvasRenderer>() ;
						if( cr0 != null && cr1 != null )
						{
							cr0.SetMaterial( cr1.GetMaterial(), 0 ) ;
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIView view, string value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIInputField, string> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}
		
		// 内部リスナー
		private void OnValueChangedInner( string value )
		{
//			Debug.LogWarning( "状態変化:" + tValue + " : " + Input.compositionString + " : " + Input.inputString ) ;

			if( m_FontFilter != null && m_FontAlternateCode != 0 && string.IsNullOrEmpty( value ) == false )
			{
				// 内部の文字を検査して表示できない文字が入っていたら強制的に補正する
				int i, l = value.Length ;
				char[] s = new char[ l ] ;
				int c ;
				bool dirty = false ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ i ] ;

					if( _inputField.lineType == InputFieldPlus.LineTypes.MultiLineNewline && ( c == 0x0D || c == 0x0A ) )
					{
						// 改行コードは許容する
						s[ i ] = value[ i ] ;
						dirty = true ;
					}
					else
					{
						if( ( m_FontFilter.flag[ c >> 3 ] & ( 1 << ( c & 0x0007 ) ) ) == 0 )
						{
							// この文字は置き換える必要がある
							s[ i ] = m_FontAlternateCode ;
						}
						else
						{
							s[ i ] = value[ i ] ;
							dirty = true ;
						}
					}
				}

				if( dirty == true )
				{
					_inputField.text = new string( s ) ;
				}
			}

			//----------------------------------------------------------

			//----------------------------------------------------------

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
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangeListener( UnityEngine.Events.UnityAction<string> onValueChanged )
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.AddListener( onValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangeListener( UnityEngine.Events.UnityAction<string> onValueChanged )
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangeAllListeners()
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onValueChanged.RemoveAllListeners() ;
			}
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnEndEditAction ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnEndEdit( string identity, UIInputField view, string value ) ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲート
		/// </summary>
		public OnEndEdit OnEndEditDelegate ;

		/// <summary>
		/// 入力が終了した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnEndEditAction">アクションメソッド</param>
		public void SetOnEndEdit( Action<string, UIInputField, string> onEndEditAction )
		{
			OnEndEditAction = onEndEditAction ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnEndEditDelegate">デリゲートメソッド</param>
		public void AddOnEndEdit( OnEndEdit onEndEditDelegate )
		{
			OnEndEditDelegate += onEndEditDelegate ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnEndEditDelegate">デリゲートメソッド</param>
		public void RemoveOnEndEdit( OnEndEdit onEndEditDelegate )
		{
			OnEndEditDelegate -= onEndEditDelegate ;
		}
		
		//---------------------------

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> OnEnterKeyPressedAction ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnEnterKeyPressed( string identity, UIInputField view, string value ) ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲート
		/// </summary>
		public OnEnterKeyPressed OnEnterKeyPressedDelegate ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを設定する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyAction">アクションメソッド</param>
		public void SetOnEnterKeyPressed( Action<string, UIInputField, string> onEnterKeyPressedAction )
		{
			OnEnterKeyPressedAction = onEnterKeyPressedAction ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを追加する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyDelegate">デリゲートメソッド</param>
		public void AddOnEnterKeyPressed( OnEnterKeyPressed onEnterKeyPressedDelegate )
		{
			OnEnterKeyPressedDelegate += onEnterKeyPressedDelegate ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを削除する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyDelegate">デリゲートメソッド</param>
		public void RemoveOnEnterKeyPressed( OnEnterKeyPressed onEnterKeyPressedDelegate )
		{
			OnEnterKeyPressedDelegate -= onEnterKeyPressedDelegate ;
		}

		//---------------------------
		
		// 内部リスナー
		private void OnEndEditInner( string value )
		{
			if( OnEndEditAction != null || OnEndEditDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnEndEditAction?.Invoke( identity, this, value ) ;
				OnEndEditDelegate?.Invoke( identity, this, value ) ;
			}

			if( Input.GetKey( KeyCode.Return ) == true )
			{
				if( OnEnterKeyPressedAction != null || OnEnterKeyPressedDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					OnEnterKeyPressedAction?.Invoke( identity, this, value ) ;
					OnEnterKeyPressedDelegate?.Invoke( identity, this, value ) ;
				}
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnEndEdit">リスナーメソッド</param>
		public void AddOnEndEditListener( UnityEngine.Events.UnityAction<string> onEndEdit )
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.AddListener( onEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnEndEdit">リスナーメソッド</param>
		public void RemoveOnEndEditListener( UnityEngine.Events.UnityAction<string> onEndEdit )
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.RemoveListener( onEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnEndEditAllListeners()
		{
			InputFieldPlus inputField = _inputField ;
			if( inputField != null )
			{
				inputField.onEndEdit.RemoveAllListeners() ;
			}
		}
	}
}

