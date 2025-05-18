using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.UI ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// ゲームパッドのフォーカスコントロールのクラス
	/// </summary>
	public class UIPadFocusController : UIImage
	{
		/// <summary>
		/// 入力要素
		/// </summary>
		[Serializable]
		public class InputElement
		{
			/// <summary>
			/// 識別名
			/// </summary>
			public string Identity ;

			/// <summary>
			/// カーソル(設定しておくと自動的に表示のオンオフを行ってくれる)
			/// </summary>
			public UIView   Cursor ;

			/// <summary>
			/// ←が押された際に呼び出される(nullを返すと遷移しない)
			/// </summary>
			public Func<string[]> OnMoveL ;

			/// <summary>
			/// →が押された際に呼び出される(nullを返すと遷移しない)
			/// </summary>
			public Func<string[]> OnMoveR ;

			/// <summary>
			/// ↑が押された際に呼び出される(nullを返すと遷移しない)
			/// </summary>
			public Func<string[]> OnMoveU ;

			/// <summary>
			/// ↓が押された際に呼び出される(nullを返すと遷移しない)
			/// </summary>
			public Func<string[]> OnMoveD ;

			/// <summary>
			/// 何等かの動きがあった際に呼び出される
			/// </summary>
			public Action         OnMove ;

			/// <summary>
			/// フォーカス状態が変化した際に呼び出すコールバック
			/// </summary>
			public Action<bool> OnFocusChanged ;

			/// <summary>
			/// 決定を押した際に呼び出すコールバック
			/// </summary>
			public Action<bool> OnDecision ;

			//-------------------------------------------------

			// 最後の←側の識別名
			[NonSerialized]
			public string      IdentityFromL ;

			// 最後の→側の識別名
			[NonSerialized]
			public string      IdentityFromR ;

			// 最後の↑側の識別名
			[NonSerialized]
			public string      IdentityFromU ;

			// 最後の↓側の識別名
			[NonSerialized]
			public string      IdentityFromD ;
		}

		// 入力要素
		[SerializeField]
		private List<InputElement>  m_InputElements ;

		// 有効化されている(フォーカスを得ている)入力要素
		[SerializeField]
		private string  m_CurrentInputElementIdentity ;

		//-----------------------------------------------------
		// 全体用のコールバック

		private Func<string,string[]> m_OnMoveL ;
		private Func<string,string[]> m_OnMoveR ;
		private Func<string,string[]> m_OnMoveU ;
		private Func<string,string[]> m_OnMoveD ;

		private Action<string,bool>   m_OnFocusChanged ;

		// 決定ボタンの識別子
		private int[] m_DecisionButtonIdentities = { GamePad.B1 } ;

		// 決定キーの識別子群
		private KeyCodes[] m_DecisionButtonKeyCodes = { KeyCodes.Z, KeyCodes.Return } ;

		// 決定ボタンの入力があった際に呼ばれるコールバック
		private Action<string,bool>   m_OnDecision ;

		// 決定ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool m_IsDecisionButtonPressing = false ;

		// キャンセルボタンの識別子
		private int[] m_CancelButtonIdentities = { GamePad.B2 } ;

		// キャンセルキーの識別子群
		private KeyCodes[] m_CancelButtonKeyCodes = { KeyCodes.X, KeyCodes.Escape } ;

		// キャンセルボタンの入力があった際に呼ばれるコールバック
		private Func<string,string>   m_OnCancel ;

		// 取消ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool m_IsCancelButtonPressing = false ;

		// 拡張ボタンの識別子群
		private int[] m_ExtraButtonIdentities = null ;

		// 拡張キーの識別子群
		private KeyCodes[][] m_ExtraButtonKeyCodes = null ;

		// 拡張ボタンの入力があった際に呼ばれるコールバック
		private Action<int,string>      m_OnExtraButton ; 

		// 拡張ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool[] m_IsExtraButtonPressing = null ;

		//-----------------------------

		// 基本アクシスのスティック識別番号群(カーソル移動に関係あり)
		private int[] m_BasisAxisNumbers = { 0 } ;

		// 基本アクシスのキーコード群(カーソル移動に関係あり)
		private KeyCodes[][]    m_BasisAxisKeyCodes =
		{
			new []{ KeyCodes.LeftArrow, KeyCodes.RightArrow, KeyCodes.UpArrow, KeyCodes.DownArrow },
		} ;

		// 拡張アクシスの識別番号群(カーソル移動に関係なし)
		private int[]                           m_ExtraAxisNumbers = null ;

		private KeyCodes[][]                    m_ExtraAxisKeyCodes = null ;

		// 拡張アクシスの入力があった際に呼ばれるコールバック
		private Action<int,Vector2,string>      m_OnExtraAxis ;

		//-----------------------------

		// 入力モードが切り替わった際に呼び出されるコールバック
		private Action<InputTypes,InputTypes>   m_OnInputTypeChanged ;

		// 現在の入力モード
		private InputTypes                      m_BasisInputType ;

		// 現在の入力モード
		private InputTypes                      m_ExtraInputType ;

		//-----------------------------------------------------

		// 現在ゲームパッドの入力が有効な状態になっているかどうか(実際の入力ではなくこのクラス内での入力)
		private bool m_IsFocusEnabled ;

		/// <summary>
		/// フォーカスを得いている状態かどうか
		/// </summary>
		public bool IsFocusEnabled => m_IsFocusEnabled ;


		// 現在のポインターの位置
		private Vector2 m_CurrentPointerPosition ;


		private bool    m_Ready ;

		//-------------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		protected override void OnBuild( string option = "" )
		{
			var image = CImage != null ? CImage : gameObject.AddComponent<Image>() ;
			if( image == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/GamePad" ) ;
			image.color = Color.white ;
			image.type = Image.Type.Simple ;

			ResetRectTransform() ;
			
			SetAnchorToLeftBottom() ;
			SetPivot( 0, 0 ) ;
			SetSize( 64, 64 ) ;
			SetPosition( 16, 16 ) ;

			//----------------------------------

			if( IsCanvasOverlay == true )
			{
				image.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Normal" ) ;
			}
		}

		// 透明色
		private static Color m_Transparency = new ( 0, 0, 0, 0 ) ;

		// 開始時に呼び出される
		protected override void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				// 自身を見えないようにする
				Color          = m_Transparency ;
				EffectiveColor = m_Transparency ;

				RaycastTarget               = false ;  // 通常はレイキャストに反応しないようにする
				IsForceRaycastTargetEnabled = true ;   // レイキャストの判定自体は有効化する

				m_CurrentPointerPosition = Mouse.Position ;
			}
		}

		// 破棄時に呼び出される
		protected override void OnDestroy()
		{
			base.OnDestroy() ;
		}

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 入力要素を設定する
		/// </summary>
		public void SetInputElements
		(
			List<InputElement> inputElements,
			string currentInputElementIdentity,
			Func<string,string[]> onMoveL = null,
			Func<string,string[]> onMoveR = null,
			Func<string,string[]> onMoveU = null,
			Func<string,string[]> onMoveD = null,
			Action<string,bool> onFocusChanged = null,
			Action<string,bool> onDecision = null,
			Func<string,string> onCancel = null,
			Action<InputTypes,InputTypes> onInputTypeChanged = null,
			InputTypes basisInputType = InputTypes.Unknown,
			InputTypes extraInputType = InputTypes.Unknown
		)
		{
			if( basisInputType != InputTypes.Unknown )
			{
				m_BasisInputType = basisInputType ;
			}
			else
			{
				m_BasisInputType = UIEventSystem.BasisInputType ;
			}
			if( extraInputType != InputTypes.Unknown )
			{
				m_ExtraInputType = extraInputType ;
			}
			else
			{
				m_ExtraInputType = UIEventSystem.ExtraInputType ;
			}

			//-------------------------------------------------

			if( m_InputElements != null && m_InputElements.Count > 0 )
			{
				// 古い入力要素が設定されていたらカーソルを全て消去する

				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false );

				UpdateCursors();
			}

			//-------------------------------------------------

			m_InputElements               = inputElements ;
			m_CurrentInputElementIdentity = currentInputElementIdentity ;

			m_OnMoveL        = onMoveL ;
			m_OnMoveR        = onMoveR ;
			m_OnMoveU        = onMoveU ;
			m_OnMoveD        = onMoveD ;
			m_OnFocusChanged = onFocusChanged ;
			m_OnDecision     = onDecision ;
			m_OnCancel       = onCancel ;

			m_OnInputTypeChanged = onInputTypeChanged ;

			if( m_Ready == false )
			{
				m_IsFocusEnabled = ( m_BasisInputType == InputTypes.Keyboard || m_BasisInputType == InputTypes.GamePad ) ;

				m_Ready = true ;
			}

			if( m_IsFocusEnabled == true )
			{
				// フォーカスを得る
				CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;
			}

			UpdateCursors() ;

			// 設定直後のコール
			m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
		}

		/// <summary>
		/// アクティブな入力要素を設定する
		/// </summary>
		/// <param name="currentInputElementIdentity"></param>
		public void SetCurrentInputElement( string currentInputElementIdentity )
		{
			if( m_CurrentInputElementIdentity != currentInputElementIdentity )
			{
				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

				m_CurrentInputElementIdentity = currentInputElementIdentity ;

				// フォーカスを得る
				CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

				UpdateCursors() ;
			}
		}

		/// <summary>
		/// アクティブになっている入力要素の識別子を取得する
		/// </summary>
		/// <returns></returns>
		public string GetCurrentInputElementIdentity()
		{
			return m_CurrentInputElementIdentity ;
		}

		//-----------------------------------------------------

		/// <summary>
		/// 基本入力ゲームパッドを設定する
		/// </summary>
		/// <param name="decisionButtonIdentity"></param>
		/// <param name="cancalButtonIdentity"></param>
		/// <param name="availableAxisNumbers"></param>
		public void SetBasisConfiguration
		(
			int[] decisionButtonIdentities, int[] cancalButtonIdentities, int[] basisAxisNumbers,
			KeyCodes[] decisionButtonKeyCodes, KeyCodes[] cancalButtonKeyCodes, KeyCodes[][] basisAxisKeyCodes
		)
		{
			m_DecisionButtonIdentities  = decisionButtonIdentities ;
			m_CancelButtonIdentities    = cancalButtonIdentities ;

			m_BasisAxisNumbers          = basisAxisNumbers ;

			//-------------------------

			m_DecisionButtonKeyCodes    = decisionButtonKeyCodes ;
			m_CancelButtonKeyCodes      = cancalButtonKeyCodes ;

			m_BasisAxisKeyCodes         = basisAxisKeyCodes ;
		}

		/// <summary>
		/// 基本入力ゲームパッドを設定する
		/// </summary>
		/// <param name="decisionButtonIdentity"></param>
		/// <param name="cancalButtonIdentity"></param>
		/// <param name="availableAxisNumbers"></param>
		public void SetBasisConfiguration
		(
			int[] decisionButtonIdentities, int[] cancalButtonIdentities, int[] basisAxisNumbers
		)
		{
			m_DecisionButtonIdentities  = decisionButtonIdentities ;
			m_CancelButtonIdentities    = cancalButtonIdentities ;

			m_BasisAxisNumbers          = basisAxisNumbers ;

			//-------------------------

			m_DecisionButtonKeyCodes    = null ;
			m_CancelButtonKeyCodes      = null ;

			m_BasisAxisKeyCodes         = null ;
		}

		//---------

		/// <summary>
		/// 拡張入力ゲームパッドボタンを設定する
		/// </summary>
		/// <param name="extraActionButtonIdentities"></param>
		/// <param name="onExtraAction"></param>
		public void SetExtraButtonConfiguration
		(
			int[] extraButtonIdentities,
			KeyCodes[][] extraButtonKeyCodes,
			Action<int,string> onExtraButton
		)
		{
			m_ExtraButtonIdentities = extraButtonIdentities ;
			m_ExtraButtonKeyCodes   = extraButtonKeyCodes ;
			m_OnExtraButton         = onExtraButton ;

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				m_IsExtraButtonPressing = new bool[ m_ExtraButtonIdentities.Length ] ;
			}
			else
			{
				m_IsExtraButtonPressing = null ;
			}
		}

		/// <summary>
		/// 拡張入力ゲームパッドボタンを設定する
		/// </summary>
		/// <param name="extraActionButtonIdentities"></param>
		/// <param name="onExtraAction"></param>
		public void SetExtraButtonConfiguration
		(
			int[] extraButtonIdentities,
			Action<int,string> onExtraButton
		)
		{
			m_ExtraButtonIdentities = extraButtonIdentities ;
			m_ExtraButtonKeyCodes   = null ;
			m_OnExtraButton         = onExtraButton ;

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				m_IsExtraButtonPressing = new bool[ m_ExtraButtonIdentities.Length ] ;
			}
			else
			{
				m_IsExtraButtonPressing = null ;
			}
		}

		/// <summary>
		/// 拡張入力ゲームパッドアクシスを設定する
		/// </summary>
		/// <param name="extraAxisNumbers"></param>
		/// <param name="onExtraAxis"></param>
		public void SetExtraAxisConfiguration
		(
			int[] extraAxisNumbers,
			KeyCodes[][] extraAxisKeyCodes,
			Action<int,Vector2,string> onExtraAxis
		)
		{
			m_ExtraAxisNumbers      = extraAxisNumbers ;
			m_ExtraAxisKeyCodes     = extraAxisKeyCodes ;
			m_OnExtraAxis           = onExtraAxis ;
		}

		/// <summary>
		/// 拡張入力ゲームパッドアクシスを設定する
		/// </summary>
		/// <param name="extraAxisNumbers"></param>
		/// <param name="onExtraAxis"></param>
		public void SetExtraAxisConfiguration
		(
			int[] extraAxisNumbers,
			Action<int,Vector2,string> onExtraAxis
		)
		{
			m_ExtraAxisNumbers      = extraAxisNumbers ;
			m_ExtraAxisKeyCodes     = null ;
			m_OnExtraAxis           = onExtraAxis ;
		}

		//-------------------------------------------------------------------------------------

		// カーソルの表示を更新する
		private bool UpdateCursors()
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 無効
				return false ;
			}

			//---------------------------------------------------------------------------------

			// 基本はすべて非表示
			foreach( var inputElement in m_InputElements )
			{
				if( inputElement != null && inputElement.Cursor != null )
				{
					inputElement.Cursor.SetActive( false ) ;
				}
			}

			//---------------------------------------------------------------------------------

			if( string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == false )
			{
				// フォーカスを得ているもの
				var inputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( inputElement != null )
				{
					if( inputElement.Cursor != null && m_IsFocusEnabled == true )
					{
						// カーソル表示
						inputElement.Cursor.SetActive( true ) ;
					}

					return true ;
				}
			}

			return false ;
		}


		// ゲームパッドによる入力があったかどうか判定する
		private bool IsGamePadInput( bool hasBasis, bool hasExtra )
		{
			//---------------------------------------------------------------------------------
			// 基本操作

			if( hasBasis == true )
			{
				// 決定キー
				if( m_DecisionButtonIdentities != null && m_DecisionButtonIdentities.Length >  0 )
				{
					foreach( var decisionButtonIdentity in m_DecisionButtonIdentities )
					{
						if( GamePad.GetButton( decisionButtonIdentity ) == true )
						{
							return true ;
						}
					}
				}

				//-----

				// 取消キー
				if( m_CancelButtonIdentities != null && m_CancelButtonIdentities.Length >  0 )
				{
					foreach( var cancelButtonIdentity in m_CancelButtonIdentities )
					{
						if( GamePad.GetButton( cancelButtonIdentity ) == true )
						{
							return true ;
						}
					}
				}

				//-------------------------

				// 方向キー
				if( m_BasisAxisNumbers != null && m_BasisAxisNumbers.Length >  0 )
				{
					foreach( var basisAxisNumber in m_BasisAxisNumbers )
					{
						var axis = GamePad.GetAxis( basisAxisNumber ) ;
						axis.x = axis.x <  0 ? - axis.x : axis.x ;
						axis.y = axis.y <  0 ? - axis.y : axis.y ;

						if( axis.x >  0.1f || axis.y >  0.1f )
						{
							return true ;
						}
					}
				}
			}

			//---------------------------------------------------------------------------------
			// 拡張操作

			if( hasExtra == true )
			{
				if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
				{
					foreach( var extraButtonIdentity in m_ExtraButtonIdentities )
					{
						if( GamePad.GetButton( extraButtonIdentity ) == true )
						{
							return true ;
						}
					}
				}

				if( m_ExtraAxisNumbers != null && m_ExtraAxisNumbers.Length >  0 )
				{
					foreach( var extraAxisNumber in m_ExtraAxisNumbers )
					{
						var axis = GamePad.GetAxis( extraAxisNumber ) ;
						axis.x = axis.x <  0 ? - axis.x : axis.x ;
						axis.y = axis.y <  0 ? - axis.y : axis.y ;

						if( axis.x >  0.1f || axis.y >  0.1f )
						{
							return true ;
						}
					}
				}
			}

			//---------------------------------------------------------------------------------

			return false ;
		}

		// キーボードによる入力があったかどうか判定する
		private bool IsKeyboardInput( bool hasBasis, bool hasExtra )
		{
			//---------------------------------------------------------------------------------
			// 基本操作

			if( hasBasis == true )
			{
				// 決定キー
				if( m_DecisionButtonKeyCodes != null && m_DecisionButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_DecisionButtonKeyCodes )
					{
						if( Keyboard.GetKey( keyCode ) == true )
						{
							return true ;
						}
					}
				}

				//-----

				// 取消キー
				if( m_CancelButtonKeyCodes != null && m_CancelButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_CancelButtonKeyCodes )
					{
						if( Keyboard.GetKey( keyCode ) == true )
						{
							return true ;
						}
					}
				}

				//-------------------------

				// 方向キー
				if( m_BasisAxisKeyCodes != null && m_BasisAxisKeyCodes.Length >  0 )
				{
					foreach( var basisAxiskeyCodes in m_BasisAxisKeyCodes )
					{
						if( basisAxiskeyCodes != null && basisAxiskeyCodes.Length == 4 )
						{
							if( Keyboard.GetKey( basisAxiskeyCodes[ 0 ] ) == true )
							{
								// →
								return true ;
							}
							if( Keyboard.GetKey( basisAxiskeyCodes[ 1 ] ) == true )
							{
								// ←
								return true ;
							}
							if( Keyboard.GetKey( basisAxiskeyCodes[ 2 ] ) == true )
							{
								// ↑
								return true ;
							}
							if( Keyboard.GetKey( basisAxiskeyCodes[ 3 ] ) == true )
							{
								// ↓
								return true ;
							}
						}
					}
				}
			}

			//---------------------------------------------------------------------------------
			// 拡張操作

			if( hasExtra == true )
			{
				int i, l ;

				if( m_ExtraButtonKeyCodes != null && m_ExtraButtonKeyCodes.Length >  0 )
				{
					l = m_ExtraButtonKeyCodes.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						var extraButtonKeyCodes = m_ExtraButtonKeyCodes[ i ] ;
						if( extraButtonKeyCodes != null && extraButtonKeyCodes.Length >  0 )
						{
							foreach( var keyCode in extraButtonKeyCodes )
							{
								if( Keyboard.GetKey( keyCode ) == true )
								{
									return true ;
								}
							}
						}
					}
				}

				if( m_ExtraAxisKeyCodes != null && m_ExtraAxisKeyCodes.Length >  0 )
				{
					l = m_ExtraAxisKeyCodes.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						var extraAxisKeyCodes = m_ExtraAxisKeyCodes[ i ] ;
						if( extraAxisKeyCodes != null && extraAxisKeyCodes.Length == 4 )
						{
							if( Keyboard.GetKey( extraAxisKeyCodes[ 0 ] ) == true )
							{
								// →
								return true ;
							}
							if( Keyboard.GetKey( extraAxisKeyCodes[ 1 ] ) == true )
							{
								// ←
								return true ;
							}
							if( Keyboard.GetKey( extraAxisKeyCodes[ 2 ] ) == true )
							{
								// ↑
								return true ;
							}
							if( Keyboard.GetKey( extraAxisKeyCodes[ 3 ] ) == true )
							{
								// ↓
								return true ;
							}
						}
					}
				}
			}

			//---------------------------------------------------------------------------------

			return false ;
		}

		/// <summary>
		/// 毎フレーム呼び出される
		/// </summary>
		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == false )
			{
				return ;
			}

			//---------------------------------------------------------------------------------

//			if( m_InputElements == null || m_InputElements.Count == 0 )
//			{
//				// アクティブになっている入力要素が存在しない場合は無効
//				return ;
//			}

			if( m_IsFocusEnabled == true )
			{
				// フォーカスが有効になっている

				Vector2 pointerPosition = Mouse.Position ;
				if
				(
					( ( m_CurrentPointerPosition.x != pointerPosition.x || m_CurrentPointerPosition .y != pointerPosition.y ) && Mouse.GetButton( 0 ) == false && Mouse.GetButton( 1 ) == false && Mouse.GetButton( 2 ) == false ) ||
					Mouse.GetButtonUp( 0 ) == true || Mouse.GetButtonUp( 1 ) == true || Mouse.GetButtonUp( 2 ) == true
				)
				{
					// フォーカスを解除する
					m_IsFocusEnabled = false ;

					// フォーカスを失う
					CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

					UpdateCursors() ;

					// マウスカーソルは表示する
					Cursor.visible = true ;

					// ポインターモード
					m_BasisInputType = InputTypes.Pointer ;
					m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;

					//-----------------------------------------

					m_IsDecisionButtonPressing = false ;
					m_IsCancelButtonPressing = false ;

					if( m_IsExtraButtonPressing != null && m_IsExtraButtonPressing.Length >  0 )
					{
						int i, l = m_IsExtraButtonPressing.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							m_IsExtraButtonPressing[ i ] = false ;
						}
					}

					return ;
				}
			}

			//-------------------------------------------------

			if( IsRaycastAvailable() == false )
			{
				// レイキャストが通る状態でなければ処理しない
				m_IsDecisionButtonPressing = false ;
				return ;
			}

			//-------------------------------------------------

			// 拡張ボタン・拡張アクシスは、フォーカスの有無に関係なく反応する

			// 拡張アクシスを処理する(基本方向キーやボタンと同時押し可能)
			ProcessExtraAxis() ;

			// 拡張ボタンを処理する
			ProcessExtraButton() ;

			//---------------------------------------------------------------------------------

			if( m_IsFocusEnabled == false )
			{
				// フォーカスが有効になっていない(ポインター)

				bool isGamePadInput  = IsGamePadInput( true, false ) ;
				bool isKeyboardInput = IsKeyboardInput( true, false ) ;

				if( isGamePadInput == true || isKeyboardInput == true )
				{
					// 入力が有効になる
					m_IsFocusEnabled = true ;

					// フォーカスを得る
					CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

					UpdateCursors() ;

					// マウスカーソルは隠蔽する
					Cursor.visible = false ;

					// キーボードまたはゲームパッド
					if( isKeyboardInput == false )
					{
						// ゲームパッド入力
						m_BasisInputType = InputTypes.GamePad ;
						m_ExtraInputType = InputTypes.GamePad ;
						m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
					}
					else
					{
						// キーボード入力
						m_BasisInputType = InputTypes.Keyboard ;
						m_ExtraInputType = InputTypes.Keyboard ;
						m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
					}

					// 最初の入力は無視する
					m_CurrentPointerPosition = Mouse.Position ;
				}
				else
				{
					if( m_ExtraInputType == InputTypes.GamePad )
					{
						if( IsKeyboardInput( false, true ) == true )
						{
							// 拡張操作がキーボード入力モードに変化する
							m_ExtraInputType = InputTypes.Keyboard ;
							m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
						}
					}
					else
					if( m_ExtraInputType == InputTypes.Keyboard )
					{
						if( IsGamePadInput( false, true ) == true )
						{
							// 拡張操作がゲームパッド入力モードに変化する
							m_ExtraInputType = InputTypes.GamePad ;
							m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
						}
					}

					// マウスの戻る判定
					if( Mouse.GetButtonDown( 1 ) == true )
					{
						m_OnCancel?.Invoke( m_CurrentInputElementIdentity ) ;
					}
				}
				return ;
			}
			else
			{
				// フォーカスが有効になっている(ゲームパッドまたはキーボード)

				if( m_BasisInputType == InputTypes.GamePad )
				{
					if( IsKeyboardInput( true, true ) == true )
					{
						// キーボード入力モードに変更
						m_BasisInputType = InputTypes.Keyboard ;
						m_ExtraInputType = InputTypes.Keyboard ;
						m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
//						return ;
				   }
				}
				else
				if( m_BasisInputType == InputTypes.Keyboard )
				{
					if( IsGamePadInput( true, true ) == true )
					{
						// ゲームパッド入力モードに変更
						m_BasisInputType = InputTypes.GamePad ;
						m_ExtraInputType = InputTypes.GamePad ;
						m_OnInputTypeChanged?.Invoke( m_BasisInputType, m_ExtraInputType ) ;
//						return ;
					}
				}
			}

			//-------------------------------------------------

			// 基本アクシスを処理する
			if( ProcessBasisAxis() == false )
			{
				// 基本ボタンを処理する
				ProcessBasisButton();
			}
		}

		// 基本アクシスを処理する
		protected bool ProcessBasisAxis()
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 要素が存在しない
				return false ;
			}

			Vector2 basisAxis = GetBasisAxisRepeat() ;

			if( basisAxis.x == 0 && basisAxis.y == 0 )
			{
				// 入力なし
				return false ;
			}

			//---------------------------------------------------------------------------------

			// 現在アクティブになっている入力要素を取得する
			var currentInputElementOld = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
			if( currentInputElementOld == null )
			{
				Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity ) ;
				return false ;
			}

			int inputFlags = 0 ;
			if( basisAxis.x <  0 )
			{
				// ←
				inputFlags |= 1 ;
			}
			if( basisAxis.y <  0 )
			{
				// ↓
				inputFlags |= 2 ;
			}
			if( basisAxis.x >  0 )
			{
				// →
				inputFlags |= 4 ;
			}
			if( basisAxis.y >  0 )
			{
				// ↑
				inputFlags |= 8 ;
			}

			if( inputFlags == 1 )
			{
				// ←
				if( currentInputElementOld.OnMoveL != null || m_OnMoveL != null )
				{
					// ←への遷移先が存在する可能性がある
					string[] identities = null ;

					if( currentInputElementOld.OnMoveL != null )
					{
						identities = currentInputElementOld.OnMoveL() ;
					}
					if( m_OnMoveL != null )
					{
						identities = m_OnMoveL( currentInputElementOld.Identity ) ;
					}
					
					if( identities != null && identities.Length >  0 )
					{
						currentInputElementOld.OnMove?.Invoke() ;

						string identityToL = null ;

						if( identities.Length >  1 && string.IsNullOrEmpty( currentInputElementOld.IdentityFromL ) == false )
						{
							// 過去に←側から遷移してきた事がある
							if( identities.Contains( currentInputElementOld.IdentityFromL ) == true )
							{
								identityToL = currentInputElementOld.IdentityFromL ;
							}
							else
							{
								// 履歴に異常が生じている
								Debug.LogWarning( "[PadFocusController][Move L] 遷移先の識別名に過去の遷移名が見つからない = " + currentInputElementOld.IdentityFromL + " ← " + currentInputElementOld.Identity ) ;

								// 最初のものをデフォルト遷移先として採用する
								identityToL = identities[ 0 ] ;
							}
						}
						else
						{
							// 過去に←側から遷移してきた事がない

							// 最初のものをデフォルト遷移先として採用する
							identityToL = identities[ 0 ] ;
						}

						// 遷移先の入力要素を取得する
						var currentInputElementNew = m_InputElements.FirstOrDefault( _ => _.Identity == identityToL ) ;
						if( currentInputElementNew == null )
						{
							Debug.LogWarning( "[PadFocusController][Move L] 識別名が不正です = " + identityToL + " ← " + currentInputElementOld.Identity ) ;
							return false ;
						}

						// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
						currentInputElementNew.IdentityFromR = currentInputElementOld.Identity ;

						// アクティブな入力要素を遷移させる
						m_CurrentInputElementIdentity = currentInputElementNew.Identity ;

						// フォーカスを失う
						CallOnFocusChanged( currentInputElementOld.Identity, false ) ;

						// フォーカスを得る
						CallOnFocusChanged( currentInputElementNew.Identity, true ) ;

						// カーソルの表示を更新する
						UpdateCursors() ;
					}
				}
			}

			if( inputFlags == 2 )
			{
				// ↓
				if( currentInputElementOld.OnMoveD != null || m_OnMoveD != null )
				{
					// ↓への遷移先が存在する可能性がある
					string[] identities = null ;

					if( currentInputElementOld.OnMoveD != null )
					{
						identities = currentInputElementOld.OnMoveD() ;
					}
					if( m_OnMoveD != null )
					{
						identities = m_OnMoveD( currentInputElementOld.Identity ) ;
					}
					
					if( identities != null && identities.Length >  0 )
					{
						currentInputElementOld.OnMove?.Invoke() ;

						string identityToD = null ;

						if( identities.Length >  1 && string.IsNullOrEmpty( currentInputElementOld.IdentityFromD ) == false )
						{
							// 過去に↓側から遷移してきた事がある
							if( identities.Contains( currentInputElementOld.IdentityFromD ) == true )
							{
								identityToD = currentInputElementOld.IdentityFromD ;
							}
							else
							{
								// 履歴に異常が生じている
								Debug.LogWarning( "[PadFocusController][Mode D] 遷移先の識別名に過去の遷移名が見つからない = " + currentInputElementOld.IdentityFromD + " ← " + currentInputElementOld.Identity ) ;

								// 最初のものをデフォルト遷移先として採用する
								identityToD = identities[ 0 ] ;
							}
						}
						else
						{
							// 過去に←側から遷移してきた事がない

							// 最初のものをデフォルト遷移先として採用する
							identityToD = identities[ 0 ] ;
						}

						// 遷移先の入力要素を取得する
						var currentInputElementNew = m_InputElements.FirstOrDefault( _ => _.Identity == identityToD ) ;
						if( currentInputElementNew == null )
						{
							Debug.LogWarning( "[PadFocusController][Move D] 移動先の識別名が不正です = " + identityToD + " ← " + currentInputElementOld.Identity ) ;
							return false ;
						}

						// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
						currentInputElementNew.IdentityFromU = currentInputElementOld.Identity ;

						// アクティブな入力要素を遷移させる
						m_CurrentInputElementIdentity = currentInputElementNew.Identity ;

						// フォーカスを失う
						CallOnFocusChanged( currentInputElementOld.Identity, false ) ;

						// フォーカスを得る
						CallOnFocusChanged( currentInputElementNew.Identity, true ) ;

						// カーソルの表示を更新する
						UpdateCursors() ;

					}
				}
			}

			if( inputFlags == 4 )
			{
				// →
				if( currentInputElementOld.OnMoveR != null || m_OnMoveR != null )
				{
					// ←への遷移先が存在する可能性がある
					string[] identities = null ;

					if( currentInputElementOld.OnMoveR != null )
					{
						identities = currentInputElementOld.OnMoveR() ;
					}
					if( m_OnMoveR != null )
					{
						identities = m_OnMoveR( currentInputElementOld.Identity ) ;
					}
					
					if( identities != null && identities.Length >  0 )
					{
						currentInputElementOld.OnMove?.Invoke() ;

						string identityToR = null ;

						if( identities.Length >  1 && string.IsNullOrEmpty( currentInputElementOld.IdentityFromR ) == false )
						{
							// 過去に←側から遷移してきた事がある
							if( identities.Contains( currentInputElementOld.IdentityFromR ) == true )
							{
								identityToR = currentInputElementOld.IdentityFromR ;
							}
							else
							{
								// 履歴に異常が生じている
								Debug.LogWarning( "[PadFocusController][Move R] 遷移先の識別名に過去の遷移名が見つからない = " + currentInputElementOld.IdentityFromR + " ← " + currentInputElementOld.Identity ) ;

								// 最初のものをデフォルト遷移先として採用する
								identityToR = identities[ 0 ] ;
							}
						}
						else
						{
							// 過去に←側から遷移してきた事がない

							// 最初のものをデフォルト遷移先として採用する
							identityToR = identities[ 0 ] ;
						}

						// 遷移先の入力要素を取得する
						var currentInputElementNew = m_InputElements.FirstOrDefault( _ => _.Identity == identityToR ) ;
						if( currentInputElementNew == null )
						{
							Debug.LogWarning( "[PadFocusController][Move R] 移動先の識別名が不正です = " + identityToR + " ← " + currentInputElementOld.Identity ) ;
							return false ;
						}

						// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
						currentInputElementNew.IdentityFromL = currentInputElementOld.Identity ;

						// アクティブな入力要素を遷移させる
						m_CurrentInputElementIdentity = currentInputElementNew.Identity ;

						// フォーカスを失う
						CallOnFocusChanged( currentInputElementOld.Identity, false ) ;

						// フォーカスを得る
						CallOnFocusChanged( currentInputElementNew.Identity, true ) ;

						// カーソルの表示を更新する
						UpdateCursors() ;
					}
				}
			}

			if( inputFlags == 8 )
			{
				// ↑
				if( currentInputElementOld.OnMoveU != null || m_OnMoveU != null )
				{
					// ↑への遷移先が存在する可能性がある
					string[] identities = null ;

					if( currentInputElementOld.OnMoveU != null )
					{
						identities = currentInputElementOld.OnMoveU() ;
					}
					if( m_OnMoveU != null )
					{
						identities = m_OnMoveU( currentInputElementOld.Identity ) ;
					}
					
					if( identities != null && identities.Length >  0 )
					{
						currentInputElementOld.OnMove?.Invoke() ;

						string identityToU = null ;

						if( identities.Length >  1 && string.IsNullOrEmpty( currentInputElementOld.IdentityFromU ) == false )
						{
							// 過去に↑側から遷移してきた事がある
							if( identities.Contains( currentInputElementOld.IdentityFromU ) == true )
							{
								identityToU = currentInputElementOld.IdentityFromU ;
							}
							else
							{
								// 履歴に異常が生じている
								Debug.LogWarning( "[PadFocusController][Move U] 遷移先の識別名に過去の遷移名が見つからない = " + currentInputElementOld.IdentityFromU + " ← " + currentInputElementOld.Identity ) ;

								// 最初のものをデフォルト遷移先として採用する
								identityToU = identities[ 0 ] ;
							}
						}
						else
						{
							// 過去に←側から遷移してきた事がない

							// 最初のものをデフォルト遷移先として採用する
							identityToU = identities[ 0 ] ;
						}

						// 遷移先の入力要素を取得する
						var currentInputElementNew = m_InputElements.FirstOrDefault( _ => _.Identity == identityToU ) ;
						if( currentInputElementNew == null )
						{
							Debug.LogWarning( "[PadFocusController][Move U] 移動先の識別名が不正です = " + identityToU + " ← " + currentInputElementOld.Identity) ;
							return false ;
						}

						// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
						currentInputElementNew.IdentityFromD = currentInputElementOld.Identity ;

						// アクティブな入力要素を遷移させる
						m_CurrentInputElementIdentity = currentInputElementNew.Identity ;

						// フォーカスを失う
						CallOnFocusChanged( currentInputElementOld.Identity, false ) ;

						// フォーカスを得る
						CallOnFocusChanged( currentInputElementNew.Identity, true ) ;

						// カーソルの表示を更新する
						UpdateCursors() ;
					}
				}
			}

			return true ;
		}

		// 基本アクシスを取得する(リピートあり)
		protected Vector2 GetBasisAxisRepeat()
		{
			Vector2 fixedAxis = Vector2.zero ;

			float v ;

			if( m_BasisAxisNumbers != null && m_BasisAxisNumbers.Length >  0 )
			{
				foreach( var axisNumber in m_BasisAxisNumbers )
				{
					var axis = GamePad.GetAxisRepeat( axisNumber ) ;

					// Ｘ入力
					v = axis.x <  0 ? - axis.x : axis.x ;
					if( fixedAxis.x == 0 && v >  0.1f )
					{
						fixedAxis.x = axis.x ;
					}

					// Ｙ入力
					v = axis.y <  0 ? - axis.y : axis.y ;
					if( fixedAxis.y == 0 && v >  0.1f )
					{
						fixedAxis.y = axis.y ;
					}
				}
			}

			if( m_BasisAxisKeyCodes != null && m_BasisAxisKeyCodes.Length >  0 )
			{
				foreach( var basisAxiskeyCodes in m_BasisAxisKeyCodes )
				{
					if( basisAxiskeyCodes != null && basisAxiskeyCodes.Length == 4 )
					{
						if( fixedAxis.x == 0 )
						{
							if( Keyboard.GetKeyRepeat( basisAxiskeyCodes[ 0 ] ) == true )
							{
								// →
								fixedAxis.x += 1 ;
							}
							if( Keyboard.GetKeyRepeat( basisAxiskeyCodes[ 1 ] ) == true )
							{
								// ←
								fixedAxis.x -= 1 ;
							}
						}
						if( fixedAxis.y == 0 )
						{
							if( Keyboard.GetKeyRepeat( basisAxiskeyCodes[ 2 ] ) == true )
							{
								// ↑
								fixedAxis.y += 1 ;
							}
							if( Keyboard.GetKeyRepeat( basisAxiskeyCodes[ 3 ] ) == true )
							{
								// ↓
								fixedAxis.y -= 1 ;
							}
						}
					}
				}
			}

			return fixedAxis ;
		}

		// 拡張アクシスを処理する
		protected bool ProcessExtraAxis()
		{
			bool inputFlags = false ;
			float x, y ;

			if( m_ExtraAxisNumbers != null && m_ExtraAxisNumbers.Length >  0 )
			{
				// 個別に処理する
				foreach( var axisNumber in m_ExtraAxisNumbers )
				{
					if( axisNumber >= 0 )
					{
						// ゲームパッド
						var axis = GamePad.GetAxisRepeat( axisNumber ) ;

						x = axis.x <  0 ? - axis.x : axis.x ;
						y = axis.y <  0 ? - axis.y : axis.y ;

						// キーボード
						if( m_ExtraAxisKeyCodes != null && axisNumber <  m_ExtraAxisKeyCodes.Length )
						{
							var extraAxisKeyCodes = m_ExtraAxisKeyCodes[ axisNumber ] ;
							if( extraAxisKeyCodes != null && extraAxisKeyCodes.Length == 4 )
							{
								if( x == 0 )
								{
									if( Keyboard.GetKeyRepeat( extraAxisKeyCodes[ 0 ] ) == true )
									{
										// →
										x += 1 ;
									}
									if( Keyboard.GetKeyRepeat( extraAxisKeyCodes[ 1 ] ) == true )
									{
										// ←
										x -= 1 ;
									}
								}
								if( y == 0 )
								{
									if( Keyboard.GetKeyRepeat( extraAxisKeyCodes[ 2 ] ) == true )
									{
										// ↑
										y += 1 ;
									}
									if( Keyboard.GetKeyRepeat( extraAxisKeyCodes[ 3 ] ) == true )
									{
										// ↓
										y -= 1 ;
									}
								}
							}
						}

						if( x >  0.1f || y >  0.1f )
						{
							inputFlags = true ;

							m_OnExtraAxis?.Invoke( axisNumber, axis, m_CurrentInputElementIdentity ) ;
						}
					}
				}
			}

			return inputFlags ;
		}

		//-------------------------------------------------------------------------------------

		// 基本ボタンを処理する
		protected void ProcessBasisButton()
		{
			int inputFlags = 0 ;

			bool isDecisionButtonUp = false ;

			//-------------------------------------------------

			// 決定ボタン
			if( m_IsDecisionButtonPressing == false )
			{
				// ゲームパッド
				if( m_DecisionButtonIdentities != null && m_DecisionButtonIdentities.Length >  0 )
				{
					foreach( var decisionButtonIdentity in m_DecisionButtonIdentities )
					{
						// GamePad.GetButton( ... ) にしてはならない
						if( GamePad.GetButtonDown( decisionButtonIdentity ) == true )
						{
							m_IsDecisionButtonPressing = true ;
							inputFlags |= 1 ;
							break ;
						}
					}
				}

				// キーボード
				if( m_DecisionButtonKeyCodes != null && m_DecisionButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_DecisionButtonKeyCodes )
					{
						// Keyboard.GetKey( ... ) にしてはならない
						if( Keyboard.GetKeyDown( keyCode ) == true )
						{
							m_IsDecisionButtonPressing = true ;
							inputFlags |= 1 ;
							break ;
						}
					}
				}
			}
			else
			{
				bool isPressing = false ;

				// ゲームパッド
				if( m_DecisionButtonIdentities != null && m_DecisionButtonIdentities.Length >  0 )
				{
					foreach( var decisionButtonIdentity in m_DecisionButtonIdentities )
					{
						if( GamePad.GetButton( decisionButtonIdentity ) == true )
						{
							isPressing = true ;
							break ;
						}
					}
				}

				// キーボード
				if( m_DecisionButtonKeyCodes != null && m_DecisionButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_DecisionButtonKeyCodes )
					{
						if( Keyboard.GetKey( keyCode ) == true )
						{
							isPressing = true ;
							break ;
						}
					}
				}

				if( isPressing == false )
				{
					m_IsDecisionButtonPressing = false ;
					isDecisionButtonUp = true ;
				}
			}

			//-------------------------------------------------

			// 取消ボタン
			if( m_IsCancelButtonPressing == false )
			{
				// ゲームパッド
				if( m_CancelButtonIdentities != null && m_CancelButtonIdentities.Length >  0 )
				{
					foreach( var cancelButtonIdentity in m_CancelButtonIdentities )
					{
						// GamePad.GetButton( ... ) にしてはならない
						if( GamePad.GetButtonDown( cancelButtonIdentity ) == true )
						{
							m_IsCancelButtonPressing = true ;
							inputFlags |= 2 ;
						}
					}
				}

				// キーボード
				if( m_CancelButtonKeyCodes != null && m_CancelButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_CancelButtonKeyCodes )
					{
						// Keyboard.GetKey( ... ) にしてはならない
						if( Keyboard.GetKeyDown( keyCode ) == true )
						{
							m_IsCancelButtonPressing = true ;
							inputFlags |= 2 ;
							break ;
						}
					}
				}
			}
			else
			{
				bool isPressing = false ;

				// ゲームパッド
				if( m_CancelButtonIdentities != null && m_CancelButtonIdentities.Length >  0 )
				{
					foreach( var cancelButtonIdentity in m_CancelButtonIdentities )
					{
						if( GamePad.GetButton( cancelButtonIdentity ) == true )
						{
							isPressing = true ;
							break ;
						}
					}
				}

				// キーボード
				if( m_CancelButtonKeyCodes != null && m_CancelButtonKeyCodes.Length >  0 )
				{
					foreach( var keyCode in m_CancelButtonKeyCodes )
					{
						if( Keyboard.GetKey( keyCode ) == true )
						{
							isPressing = true ;
							break ;
						}
					}
				}

				if( isPressing == false )
				{
					m_IsCancelButtonPressing = false ;
				}
			}

			//-------------------------------------------------
			// 決定ボタン

			if( inputFlags == 1 )
			{
				// 現在アクティブになっている入力要素を取得する
				var currentInputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( currentInputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity ) ;
					return ;
				}

				// 決定ボタンのコールバック呼び出し
				currentInputElement.OnDecision?.Invoke( true ) ;
				m_OnDecision?.Invoke( currentInputElement.Identity, true ) ;
			}

			if( isDecisionButtonUp == true )
			{
				// 現在アクティブになっている入力要素を取得する
				var currentInputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( currentInputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity ) ;
					return ;
				}

				// 決定ボタンのコールバック呼び出し
				currentInputElement.OnDecision?.Invoke( false ) ;
				m_OnDecision?.Invoke( currentInputElement.Identity, false ) ;
			}

			//-------------------------------------------------
			// 取消ボタン

			if( inputFlags == 2 )
			{
				string currentInputElementIdentity = m_OnCancel?.Invoke( m_CurrentInputElementIdentity ) ;

				if( string.IsNullOrEmpty( currentInputElementIdentity ) == false )
				{
					// アクティブな対象となる入力要素を変更する

					if( m_CurrentInputElementIdentity != currentInputElementIdentity )
					{
						// フォーカスを失う
						CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

						m_CurrentInputElementIdentity = currentInputElementIdentity ;

						// フォーカスを得る
						CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

						//-------------
						
						UpdateCursors() ;
					}
				}
			}
		}

		// 拡張ボタンを処理する
		protected void ProcessExtraButton()
		{
			int inputFlags = 0 ;

			//-------------------------------------------------

			// 拡張ボタン
			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				int i, l = m_ExtraButtonIdentities.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// ゲームパッド
					if( m_IsExtraButtonPressing[ i ] == false )
					{
						var extraButtonIdentity = m_ExtraButtonIdentities[ i ] ;
						if( extraButtonIdentity != 0 )
						{
							if( GamePad.GetButtonDown( extraButtonIdentity ) == true )
							{
								 m_IsExtraButtonPressing[ i ] = true ;
								inputFlags |= ( 4 << i ) ;
							}
						}

						// キーボード
						if( m_ExtraButtonKeyCodes != null && i <  m_ExtraButtonKeyCodes.Length )
						{
							var extraButtonKeyCodes = m_ExtraButtonKeyCodes[ i ] ;
							if( extraButtonKeyCodes != null && extraButtonKeyCodes.Length >  0 )
							{
								foreach( var keyCode in extraButtonKeyCodes )
								{
									// Keyboard.GetKey( ... ) にしてはならない
									if( Keyboard.GetKeyDown( keyCode ) == true )
									{
										m_IsExtraButtonPressing[ i ] = true ;
										inputFlags |= ( 4 << i ) ;
										break ;
									}
								}
							}
						}
					}
					else
					{
						bool isPressing = false ;

						// ゲームパッド
						var extraButtonIdentity = m_ExtraButtonIdentities[ i ] ;
						if( extraButtonIdentity != 0 )
						{
							if( GamePad.GetButton( extraButtonIdentity ) == true )
							{
								isPressing = true ;
							}
						}

						// キーボード
						if( m_ExtraButtonKeyCodes != null && i <  m_ExtraButtonKeyCodes.Length )
						{
							var extraButtonKeyCodes = m_ExtraButtonKeyCodes[ i ] ;
							if( extraButtonKeyCodes != null && extraButtonKeyCodes.Length >  0 )
							{
								foreach( var keyCode in extraButtonKeyCodes )
								{
									if( Keyboard.GetKey( keyCode ) == true )
									{
										isPressing = true ;
										break ;
									}
								}
							}
						}

						if( isPressing == false )
						{
							m_IsExtraButtonPressing[ i ] = false ;
						}
					}
				}

				//---------------------------------------------

				// 決定ボタンとキャンセルボタンのフラグを削除
				int extraInputFlags = inputFlags >> 2 ;

				int index = -1, count = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( ( extraInputFlags & ( 1 << i ) ) != 0 )
					{
						index = i ;
						count ++ ;
					}
				}

				if( index >= 0 && count == 1 )
				{
					// 同時押しは不可
					m_OnExtraButton?.Invoke( m_ExtraButtonIdentities[ index ], m_CurrentInputElementIdentity ) ;
				}
			}
		}

		// フォーカスを得たか失ったの場合のコールバックを呼び出す
		protected void CallOnFocusChanged( string identity, bool isFocus )
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 要素が存在しない
				return ;
			}

			if( string.IsNullOrEmpty( identity ) == false )
			{
				var inputElement = m_InputElements.FirstOrDefault( _ => _.Identity == identity ) ;
				if( inputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + identity ) ;
					return ;
				}

				inputElement.OnFocusChanged?.Invoke( isFocus ) ;
				m_OnFocusChanged?.Invoke( inputElement.Identity, isFocus ) ;
			}
		}

		/// <summary>
		/// 入力要素のカーソルを取得する
		/// </summary>
		/// <param name="identity"></param>
		/// <returns></returns>
		public UIView GetCursor( string identity )
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 要素が存在しない
				return null ;
			}

			var inputElement = m_InputElements.FirstOrDefault( _ => _.Identity == identity ) ;
			if( inputElement == null )
			{
				return null ;
			}

			return inputElement.Cursor ;
		}

		/// <summary>
		/// 入力要素の移動履歴を消去する
		/// </summary>
		/// <param name="identitis"></param>
		public void ClearHistory( params string[] identities )
		{
			if( identities == null || identities.Length == 0 || m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 要素が存在しない
				return ;
			}

			foreach( var identity in identities )
			{
				var inputElement = m_InputElements.FirstOrDefault( _ => _.Identity == identity ) ;
				if( inputElement == null )
				{
					Debug.LogWarning( "[PadFocusController][Clear] 指定された入力要素の識別子が見つかりません = " + identity ) ;
				}
				else
				{
					inputElement.IdentityFromL = null ;
					inputElement.IdentityFromD = null ;
					inputElement.IdentityFromR = null ;
					inputElement.IdentityFromU = null ;
				}
			}
		}
	}
}
