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
			public Action<ButtonActionTypes> OnDecision ;

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

		//---------

		// 決定ボタンの識別子
		private int[] m_DecisionButtonIdentities = { GamePad.B1 } ;

		// 決定アクションの名前
		private string m_DecisionButtonActionName = null ;

		// 決定キーの識別子群
		private KeyCodes[] m_DecisionButtonKeyCodes = { KeyCodes.Z, KeyCodes.Return } ;

		// 決定ボタンの入力があった際に呼ばれるコールバック
		private Action<string,ButtonActionTypes>   m_OnDecision ;

		// 決定ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool m_IsDecisionButtonPressing = false ;

		/// <summary>
		/// 決定ボタンを押している最中かどうか
		/// </summary>
		public bool IsDecisionButtonPressing => m_IsDecisionButtonPressing ;

		// 決定ボタンの長押しが行われたかどうか
		private bool m_IsDecisionButtonLongPressed = false ;

		// 決定ボタンの長押し継続時間
		private float m_DecisionButtonLongPressingTime = 0 ;

		// 決定ボタンが押されたかどうか
		private bool m_IsDecisionButtonDown = false ;

		/// <summary>
		/// 決定ボタンが長押しを行っている時間
		/// </summary>
		public float DecisionButtonPressingTime
		{
			get
			{
				if( m_DecisionButtonLongPressingTime <= 0 )
				{
					return 0 ;
				}

				float deltaTime = m_DecisionButtonLongPressingTime ;

				if( deltaTime >  m_ButtonLongPressConfirmationTime )
				{
					deltaTime  = m_ButtonLongPressConfirmationTime ;
				}

				return deltaTime ;
			}
		}

		/// <summary>
		/// 決定ボタンが長押しと判定されるまでの時間比率
		/// </summary>
		public float DecisionButtonLongPressProgress
		{
			get
			{
				return DecisionButtonPressingTime / m_ButtonLongPressConfirmationTime ;
			}
		}

		// インスタンスが有効になった直後から決定ボタンが押されていた場合に一度解放するまで決定ボタンを無効にするフラグ
		private bool    m_IsInvalidDecisionButton ;

		//---------

		// 取消ボタンの識別子
		private int[] m_CancelButtonIdentities = { GamePad.B2 } ;

		// 取消アクションの名前
		private string m_CancelButtonActionName = null ;

		// 取消キーの識別子群
		private KeyCodes[] m_CancelButtonKeyCodes = null ;

		// 取消ボタンの入力があった際に呼ばれるコールバック
		private Func<string,ButtonActionTypes,string>   m_OnCancel ;

		// 取消ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool m_IsCancelButtonPressing = false ;

		/// <summary>
		/// 取消ボタンを押している最中かどうか
		/// </summary>
		public bool IsCancelButtonPressing => m_IsCancelButtonPressing ;

		// 取消ボタンの長押しが行われたかどうか
		private bool m_IsCancelButtonLongPressed = false ;

		// 取消ボタンの長押し継続時間
		private float m_CancelButtonLongPressingTime = 0 ;

		// 取消ボタンが押されたかどうか
		private bool m_IsCancelButtonDown = false ;

		/// <summary>
		/// 取消ボタンが長押しを行っている時間
		/// </summary>
		public float CancelButtonPressingTime
		{
			get
			{
				if( m_CancelButtonLongPressingTime <= 0 )
				{
					return 0 ;
				}

				float deltaTime = m_CancelButtonLongPressingTime ;

				if( deltaTime >  m_ButtonLongPressConfirmationTime )
				{
					deltaTime  = m_ButtonLongPressConfirmationTime ;
				}

				return deltaTime ;
			}
		}

		/// <summary>
		/// 取消ボタンが長押しと判定されるまでの時間比率
		/// </summary>
		public float CancelButtonLongPressProgress
		{
			get
			{
				return CancelButtonPressingTime / m_ButtonLongPressConfirmationTime ;
			}
		}

		// インスタンスが有効になった直後から取消ボタンが押されていた場合に一度解放するまで取消ボタンを無効にするフラグ
		private bool    m_IsInvalidCancelButton ;

		//---------

		// カーソルが押された際にボタンが押しっぱなしであった場合に入力を一時的に無効化する
		private bool  m_IsButtonBlocking = false ;

		// 長押しと判定されるまでの時間
		private float m_ButtonLongPressConfirmationTime = 0.75f ;

		/// <summary>
		/// 長押し判定時間
		/// </summary>
		public float ButtonLongPressConfirmationTime
		{
			get
			{
				return m_ButtonLongPressConfirmationTime ;
			}
			set
			{
				m_ButtonLongPressConfirmationTime = value ;
			}
		}

		//---------

		// 拡張ボタンの識別子群
		private int[] m_ExtraButtonIdentities = null ;

		// 拡張キーの識別子群
		private KeyCodes[][] m_ExtraButtonKeyCodeSets = null ;

		// 拡張ボタンアクションの識別子群
		private string[] m_ExtraButtonActionNames = null ;

		// 拡張ボタンの入力があった際に呼ばれるコールバック
		private Action<int,string>              m_OnExtraButton = null ; 

		// 拡張ボタンの入力があった際に呼ばれるコールバック
		private Action<string,string,object>    m_OnExtraButtonAction = null ;

		// コールバック呼び出し時に渡す任意オブジェクト
		private object                          m_ExtraButtonActionObject = null ;
		
		// 拡張ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool[] m_IsExtraButtonIdentities_Pressing = null ;

		// 拡張ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool[] m_IsExtraButtonActionNames_Pressing = null ;

		//-----------------------------

		// 基本アクシスのスティック識別番号群(カーソル移動に関係あり)
		private int[] m_BasisAxisNumbers = { 0 } ;

		// 基本アクシスアクションの名前
		private string m_BasisAxisActionName = null ;

		// 基本アクシスのキーコード群(カーソル移動に関係あり)
		private KeyCodes[][]    m_BasisAxisKeyCodeSets =
		{
			new []{ KeyCodes.RightArrow, KeyCodes.LeftArrow, KeyCodes.UpArrow, KeyCodes.DownArrow },
		} ;

		// 拡張アクシスの識別番号群(カーソル移動に関係なし)
		private int[]                                   m_ExtraAxisNumbers = null ;

		private KeyCodes[][]                            m_ExtraAxisKeyCodeSets = null ;

		// 拡張アクシスの識別番号群(カーソル移動に関係なし)
		private string[]                                m_ExtraAxisActionNames = null ;

		// 拡張アクシスの入力があった際に呼ばれるコールバック
		private Action<int,Vector2,string>              m_OnExtraAxis = null ;

		// 拡張アクシスの入力があった際に呼ばれるコールバック
		private Action<string,Vector2,string,object>    m_OnExtraAxisAction = null ;

		// コールバック呼び出し時に渡す任意オブジェクト
		private object                                  m_ExtraAxisActionObject = null ;

		//-----------------------------

		// 入力モードが切り替わった際に呼び出されるコールバック
		private Action<InputTypes>              m_OnInputTypeChanged ;

		// 毎フレーム呼び出すコールバック
		private Action<string>                  m_OnUpdate ;

		//---------

		// 現在の基本入力モード
		private InputTypes                      m_InputType ;

		/// <summary>
		/// 現在の基本入力モード
		/// </summary>
		public InputTypes                       InputType => m_InputType ;

		//-----------------------------------------------------

		// 現在ゲームパッドの入力が有効な状態になっているかどうか(実際の入力ではなくこのクラス内での入力)
		private bool m_IsFocusEnabled ;

		/// <summary>
		/// フォーカスを得いている状態かどうか
		/// </summary>
		public bool IsFocusEnabled => m_IsFocusEnabled ;

		// フォーカス状態へ切り替わっている最中かどうか
		private bool            m_IsFocusSwitching ;

		// 現在のポインターの位置
		private Vector2 m_CurrentPointerPosition ;


		private bool    m_Ready ;

		// 直前までの機能が有効であったかどうかのフラグ(レイキャストでブロッキングされていても無効と判定される)
		private bool    m_IsActivate ;

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
				Color                       = m_Transparency ;
				EffectiveColor              = m_Transparency ;

				RaycastTarget               = false ;  // 通常はレイキャストに反応しないようにする
				IsForceRaycastTargetEnabled = true ;   // レイキャストの判定自体は有効化する

				m_CurrentPointerPosition    = Mouse.Position ;

				m_IsActivate                = false ;
			}
		}

		// 無効時に呼び出される
		protected override void OnDisable()
		{
			base.OnDisable() ;

			// 無効時の処理を行う
			ProcessDisable() ;
		}   
			
		// 破棄時に呼び出される
		protected override void OnDestroy()
		{
			base.OnDestroy() ;
		}

		//-------------------------------------------------------------------------------------

		// 無効時の処理
		private void ProcessDisable()
		{
			if( m_IsActivate == true )
			{
				// 無効になった
				m_IsActivate = false ;

				m_IsDecisionButtonPressing  = false ;
				m_IsCancelButtonPressing    = false ;
			}
		}

        /// <summary>
        /// フォーカスを持っている入力要素をアップデートする
        /// </summary>
        public void UpdateCurrentInputElement()
        {
			if( string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == false )
            {
			    CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;
            }
        }

		//-------------------------------------------------------------------------------------

		// ボタンに対する動作
		public enum ButtonActionTypes
		{
			/// <summary>
			/// 押された
			/// </summary>
			Down,

			/// <summary>
			/// 離された
			/// </summary>
			Up,

			/// <summary>
			/// 長押しされた
			/// </summary>
			LongPressed,
		}

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
			Action<string,ButtonActionTypes> onDecision = null,
			Func<string,ButtonActionTypes,string> onCancel = null,
			Action<InputTypes> onInputTypeChanged = null,
			Action<string> onUpdate = null,
			InputTypes inputType = InputTypes.Unknown
		)
		{
			if( inputType != InputTypes.Unknown )
			{
				m_InputType = inputType ;
			}
			else
			{
				m_InputType = UIEventSystem.InputType ;
			}

			m_CurrentPointerPosition = Mouse.Position ;

			//-------------------------------------------------

			if( m_InputElements != null && m_InputElements.Count >  0 )
			{
				// 古い入力要素が設定されていたらカーソルを全て消去する

				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

				UpdateCursors() ;
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

			m_OnInputTypeChanged    = onInputTypeChanged ;

			m_OnUpdate              = onUpdate ;

			if( m_Ready == false )
			{
				m_IsFocusEnabled = ( m_InputType == InputTypes.Keyboard || m_InputType == InputTypes.GamePad ) ;

				m_Ready = true ;
			}

			// 一旦すべての要素からフォーカスを失わせる

			if( m_InputElements != null && m_InputElements.Count >  0 )
			{
				foreach( var element in m_InputElements )
				{
					// フォーカスを失う
					CallOnFocusChanged( element.Identity, false ) ;
				}
			}

			if( m_IsFocusEnabled == true )
			{
				// フォーカスを得る
				CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;
			}

			UpdateCursors() ;

			// 設定直後のコール
			m_OnInputTypeChanged?.Invoke( m_InputType ) ;
		}

		/// <summary>
		/// アクティブな入力要素を設定する
		/// </summary>
		/// <param name="currentInputElementIdentity"></param>
		public void SetCurrentInputElement( string currentInputElementIdentity )
		{
			if( m_InputType != InputTypes.Keyboard && m_InputType != InputTypes.GamePad )
			{
				// カーソル非表示中の設定はカレントのエレメント識別子を変更するだけとする
				m_CurrentInputElementIdentity = currentInputElementIdentity ;
				return ;
			}

			//-------------------------------------------------

			if( m_CurrentInputElementIdentity != currentInputElementIdentity )
			{
				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

				m_CurrentInputElementIdentity = currentInputElementIdentity ;

				// フォーカスを得る
				CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

				//---------------------

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
			KeyCodes[] decisionButtonKeyCodes, KeyCodes[] cancalButtonKeyCodes, KeyCodes[][] basisAxisKeyCodeSets
		)
		{
			m_DecisionButtonIdentities  = decisionButtonIdentities ;
			m_CancelButtonIdentities    = cancalButtonIdentities ;

			m_BasisAxisNumbers          = basisAxisNumbers ;

			//-------------------------

			m_DecisionButtonKeyCodes    = decisionButtonKeyCodes ;
			m_CancelButtonKeyCodes      = cancalButtonKeyCodes ;

			m_BasisAxisKeyCodeSets      = basisAxisKeyCodeSets ;
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

			m_BasisAxisKeyCodeSets      = null ;
		}

		/// <summary>
		/// 基本入力ゲームパッドを設定する(抽象化アクション指定)
		/// </summary>
		/// <param name="decisionButtonIdentity"></param>
		/// <param name="cancalButtonIdentity"></param>
		/// <param name="availableAxisNumbers"></param>
		public void SetBasisConfiguration
		(
			string decisionButtonActionName, string cancalButtonActionName, string basisAxisActionName
		)
		{
			m_DecisionButtonActionName  = decisionButtonActionName ;
			m_CancelButtonActionName    = cancalButtonActionName ;

			m_BasisAxisActionName       = basisAxisActionName ;
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
			KeyCodes[][] extraButtonKeyCodeSets,
			Action<int,string> onExtraButton
		)
		{
			m_ExtraButtonIdentities     = extraButtonIdentities ;
			m_ExtraButtonKeyCodeSets    = extraButtonKeyCodeSets ;
			m_OnExtraButton             = onExtraButton ;

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				m_IsExtraButtonIdentities_Pressing = new bool[ m_ExtraButtonIdentities.Length ] ;
			}
			else
			{
				m_IsExtraButtonIdentities_Pressing = null ;
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
			m_ExtraButtonIdentities     = extraButtonIdentities ;
			m_ExtraButtonKeyCodeSets    = null ;
			m_OnExtraButton             = onExtraButton ;

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				m_IsExtraButtonIdentities_Pressing = new bool[ m_ExtraButtonIdentities.Length ] ;
			}
			else
			{
				m_IsExtraButtonIdentities_Pressing = null ;
			}
		}

		/// <summary>
		/// 拡張入力ゲームパッドボタンを設定する(抽象化アクション指定)
		/// </summary>
		/// <param name="extraActionButtonIdentities"></param>
		/// <param name="onExtraAction"></param>
		public void SetExtraButtonConfiguration
		(
			string[] extraButtonActionNames,
			Action<string,string,object> onExtraButtonAction,
			object extraButtonActionObject
		)
		{
			m_ExtraButtonActionNames    = extraButtonActionNames ;
			m_OnExtraButtonAction       = onExtraButtonAction ;
			m_ExtraButtonActionObject   = extraButtonActionObject ;

			if( m_ExtraButtonActionNames != null && m_ExtraButtonActionNames.Length >  0 )
			{
				m_IsExtraButtonActionNames_Pressing = new bool[ m_ExtraButtonActionNames.Length ] ;
			}
			else
			{
				m_IsExtraButtonActionNames_Pressing = null ;
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
			KeyCodes[][] extraAxisKeyCodeSets,
			Action<int,Vector2,string> onExtraAxis
		)
		{
			m_ExtraAxisNumbers      = extraAxisNumbers ;
			m_ExtraAxisKeyCodeSets  = extraAxisKeyCodeSets ;
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
			m_ExtraAxisKeyCodeSets  = null ;
			m_OnExtraAxis           = onExtraAxis ;
		}

		/// <summary>
		/// 拡張入力ゲームパッドアクシスを設定する(抽象化アクション指定)
		/// </summary>
		/// <param name="extraAxisNumbers"></param>
		/// <param name="onExtraAxis"></param>
		public void SetExtraAxisConfiguration
		(
			string[] extraAxisActionNames,
			Action<string,Vector2,string,object> onExtraAxisAction,
			object extraAxisActionObject
		)
		{
			m_ExtraAxisActionNames  = extraAxisActionNames ;
			m_OnExtraAxisAction     = onExtraAxisAction ;
			m_ExtraAxisActionObject = extraAxisActionObject ;
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

		// キーボードによる入力があったかどうか判定する
		private bool IsKeyboardInput( bool isBasis )
		{
			//---------------------------------------------------------------------------------
			// 基本操作

			if( isBasis == true )
			{
				//---------------------------------------------
				// 固定キー

				if( Keyboard.GetKey( KeyCodes.Return ) == true )
				{
					return true ;
				}

//				if( Keyboard.GetKey( KeyCodes.Escape ) == true )
//				{
//					return true ;
//				}

				if( Keyboard.GetKey( KeyCodes.LeftArrow ) == true )
				{
					return true ;
				}

				if( Keyboard.GetKey( KeyCodes.RightArrow ) == true )
				{
					return true ;
				}

				if( Keyboard.GetKey( KeyCodes.UpArrow ) == true )
				{
					return true ;
				}

				if( Keyboard.GetKey( KeyCodes.DownArrow ) == true )
				{
					return true ;
				}

				//---------------------------------------------
				// 可変キー

				// 決定と方向に設定されているアクションも確認する

				// 決定のアクション
				if( string.IsNullOrEmpty( m_DecisionButtonActionName ) == false )
				{
					if( GamePad.AcquireButtonAction( m_DecisionButtonActionName, out var _, out var decisionButtonKeyCodes, out var _ ) == true )
					{
						// キーボード
						if( decisionButtonKeyCodes != null && decisionButtonKeyCodes.Length >  0 )
						{
							foreach( var decisionButtonKeyCode in decisionButtonKeyCodes )
							{
								if( Keyboard.GetKey( decisionButtonKeyCode ) == true )
								{
									return true ;
								}
							}
						}
					}
				}

				// 方向のアクション
				if( string.IsNullOrEmpty( m_BasisAxisActionName ) == false )
				{
					if( GamePad.AcquireAxisAction( m_BasisAxisActionName, out var _, out var basisAxisKeyCodeSets, out var _ ) == true )
					{
						// キーボード
						if( basisAxisKeyCodeSets != null && basisAxisKeyCodeSets.Length >  0 )
						{
							foreach( var basisAxisKeyCodeSet in basisAxisKeyCodeSets )
							{
								if( basisAxisKeyCodeSet != null && basisAxisKeyCodeSet.Length == 4 )
								{
									if( Keyboard.GetKey( basisAxisKeyCodeSet[ 0 ] ) == true )
									{
										// →
										return true ;
									}
									if( Keyboard.GetKey( basisAxisKeyCodeSet[ 1 ] ) == true )
									{
										// ←
										return true ;
									}

									if( Keyboard.GetKey( basisAxisKeyCodeSet[ 2 ] ) == true )
									{
										// ↑
										return true ;
									}
									if( Keyboard.GetKey( basisAxisKeyCodeSet[ 3 ] ) == true )
									{
										// ↓
										return true ;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if( Keyboard.IsAnyKeyDown() == true )
				{
					return true ;
				}
			}

			//---------------------------------------------------------------------------------

			// キーボードによる入力は無い
			return false ;
		}

		// ゲームパッドによる入力があったかどうか判定する
		private bool IsGamePadInput()
		{
			var buttons = GamePad.GetButtonAll() ;
			if( buttons != 0 )
			{
				return true ;
			}

			var dpad = GamePad.GetAxis( GamePad.DPad ) ;
			if( Mathf.Abs( dpad.x ) > 0.5f || Mathf.Abs( dpad.y ) >  0.5f )
			{
				return true ;
			}

			var rstick = GamePad.GetAxis( GamePad.RStick ) ;
			if( Mathf.Abs( rstick.x ) > 0.5f || Mathf.Abs( rstick.y ) >  0.5f )
			{
				return true ;
			}

			var lstick = GamePad.GetAxis( GamePad.LStick ) ;
			if( Mathf.Abs( lstick.x ) > 0.5f || Mathf.Abs( lstick.y ) >  0.5f )
			{
				return true ;
			}

			//---------------------------------------------------------------------------------

			// ゲームパッドによる入力は無い
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

			Vector2 pointerPosition = Mouse.Position ;

			if( m_IsFocusEnabled == true )
			{
				// フォーカスが有効になっている

				// マウス入力モードに切替わる判定

				if
				(
					( ( m_CurrentPointerPosition.x != pointerPosition.x || m_CurrentPointerPosition .y != pointerPosition.y ) && Mouse.GetButton( 0 ) == false && Mouse.GetButton( 1 ) == false && Mouse.GetButton( 2 ) == false ) ||
					Mouse.GetButtonUp( 0 ) == true || Mouse.GetButtonUp( 1 ) == true || Mouse.GetButtonUp( 2 ) == true
				)
				{
					// フォーカスを解除する
					m_IsFocusEnabled = false ;
					m_IsFocusSwitching = false ;

					//-----------------

					// フォーカスを失う
					CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

					UpdateCursors() ;

					// マウスカーソルは表示する
					Cursor.visible = true ;

					// ポインターモード
					m_InputType = InputTypes.Pointer ;
					m_OnInputTypeChanged?.Invoke( m_InputType ) ;

					//-----------------------------------------

					m_IsDecisionButtonPressing = false ;
					m_IsCancelButtonPressing = false ;

					if( m_IsExtraButtonIdentities_Pressing != null && m_IsExtraButtonIdentities_Pressing.Length >  0 )
					{
						int i, l = m_IsExtraButtonIdentities_Pressing.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							m_IsExtraButtonIdentities_Pressing[ i ] = false ;
						}
					}

					// マウス入力モードになった
					return ;
				}
			}

			//-------------------------------------------------

			bool isGamePadInput       = IsGamePadInput() ;
			bool isKeyboardInputBasis = IsKeyboardInput( true ) ;

			if( IsRaycastAvailable() == false )
			{
				// レイキャストが通る状態でなければ処理しない

				// 無効時の処理を行う
				ProcessDisable() ;

				//---------------------------------------------
				// フォーカスのフラグのみはチェックして更新する(でないとマウスーカーソルの表示状態とフォーカスの状態が合わなくなる)

				if( m_IsFocusEnabled == false )
				{
					// フォーカスが有効になっていない(ポインター)

					if( isGamePadInput == true || isKeyboardInputBasis == true )
					{
						// 入力が有効になる
						m_IsFocusEnabled = true ;

						// フォーカスを得る
						CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

						// カーソルの表示を更新する
						UpdateCursors() ;

						// キーボードまたはゲームパッド
						if( isGamePadInput == true )
						{
							// ゲームパッド入力
							m_InputType = InputTypes.GamePad ;
							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						}
						else
						{
							// キーボード入力
							m_InputType = InputTypes.Keyboard ;
							m_OnInputTypeChanged?.Invoke( m_InputType ) ;
						}

						m_CurrentPointerPosition = pointerPosition ;

						//-----------------------------------------

						if( m_InputType == InputTypes.GamePad || m_InputType == InputTypes.Keyboard )
						{
							// マウスカーソルは隠蔽する
							Cursor.visible = false ;
						}
					}
				}

				return ;
			}

			//------------------------------------------------------------------------------------------
			// レイキャストが通っているので有効

			if( m_IsActivate == false )
			{
				// 有効になった
				m_IsActivate  = true ;

				//---------------------------------------------
				// 有効になった直後から決定ボタンまたは取消ボタンが押されていたら一度解放されるまでは入力を無効化する

				m_IsInvalidDecisionButton   = ProcessDecisionButton() ;
				m_IsInvalidCancelButton     = ProcessCancelButton() ;
			}

			//-------------------------------------------------
			// 注意
			//
			// 基本入力
			// 　キーボード入力モード
			// 　　→切り替えに使用された入力は無効
			// 　ゲームパッド入力モード
			// 　　→切り替えに使用された入力は無効
			//
			// 拡張入力
			// 　キーボード入力モード
			// 　　→切り替えに使用された入力は有効
			// 　ゲームパッド入力モード
			// 　　→切り替えに使用された入力は無効
			// 
			//-------------------------------------------------

			if( m_IsFocusEnabled == false )
			{
				// フォーカスが有効になっていない(ポインター)
				// カーソル入力モードに切り替わるか判定

				if( isGamePadInput == true || isKeyboardInputBasis == true )
				{
					// 切り替わる

					// 入力が有効になる
					m_IsFocusEnabled = true ;

					// 切り替わり最中
					m_IsFocusSwitching = true ;

					// フォーカスを得る
					CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

					// カーソルの表示を更新する
					UpdateCursors() ;

					// キーボードまたはゲームパッド
					if( isGamePadInput == true )
					{
						// ゲームパッド入力
						m_InputType = InputTypes.GamePad ;
						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
					}
					else
					{
						// キーボード入力
						m_InputType = InputTypes.Keyboard ;
						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
					}

					// 現在のポインターの位置を記録しておく
					m_CurrentPointerPosition = pointerPosition ;

					// 決定ボタンと取消ボタンの状態を一時的に無効化する(押しっぱなしである場合に離されるまで無効化)
					m_IsButtonBlocking = true ;

					//-----------------------------------------

					if( m_InputType == InputTypes.GamePad || m_InputType == InputTypes.Keyboard )
					{
						// マウスカーソルは隠蔽する
						Cursor.visible = false ;
					}

					// 入力モード変更のトリガーとなった入力は無効化
					return ;
				}
				else
				{
					// 切り替わらない

					// 拡張アクシスを処理する(ポインターモード時でもキーボードは有効)
					ProcessExtraAxis( InputTypes.Keyboard ) ;

					// 拡張ボタンを処理する
					ProcessExtraButton( InputTypes.Keyboard ) ;

					//-----------------------------------------

					// マウスの戻る判定
					if( Mouse.GetButtonDown( 1 ) == true )
					{
						m_OnCancel?.Invoke( m_CurrentInputElementIdentity, ButtonActionTypes.Down ) ;
					}

					return ;
				}
			}
			else
			{
				// フォーカスが有効になっている(ゲームパッドまたはキーボード)

				if( m_IsFocusSwitching == true )
				{
					if( isGamePadInput == false && isKeyboardInputBasis == false )
					{
						// 完全に入力モードが切り替わったとみなす
						m_IsFocusSwitching = false ;
					}

					return ;
				}

				//---------------------------------------------

				if( m_InputType == InputTypes.GamePad )
				{
					if( IsKeyboardInput( false ) == true )
					{
						// ゲームパッド入力モードからキーボード入力モードに変更
						m_InputType = InputTypes.Keyboard ;
						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
				   }
				}
				else
				if( m_InputType == InputTypes.Keyboard )
				{
					if( IsGamePadInput() == true )
					{
						// キーボード入力モードからゲームパッド入力モードに変更
						m_InputType = InputTypes.GamePad ;
						m_OnInputTypeChanged?.Invoke( m_InputType ) ;
					}
				}
			}

			//------------------------------------------------------------------------------------------
			// 以下は完全のフォーカスを得ている場合のみ処理される

			// 基本アクシスを処理する
			if( ProcessBasisAxis( true ) == false )
			{
				// 基本ボタンを処理する
				ProcessBasisButton() ;
			}
			else
			{
				// 決定ボタンと取消ボタンの状態を一時的に無効化する(押しっぱなしである場合に離されるまで無効化)
				m_IsButtonBlocking = true ;
			}

			// 拡張アクシスを処理する
			ProcessExtraAxis( InputTypes.Unknown ) ;

			// 拡張ボタンを処理する
			ProcessExtraButton( InputTypes.Unknown ) ;

			//-------------------------------------------------

			// 毎フレーム呼び出し
			m_OnUpdate?.Invoke( m_CurrentInputElementIdentity ) ;
		}

		// 基本アクシスを処理する
		protected bool ProcessBasisAxis( bool isRepeat )
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// 要素が存在しない
				return false ;
			}

			if( UIEventSystem.IsInputFieldFocused() == true )
			{
				// InputField が入力状態になっている
				return false ;
			}

			//-------------------------

			Vector2 basisAxis ;

			if( isRepeat == true )
			{
				// リピートあり
				basisAxis = GetBasisAxisRepeat() ;
			}
			else
			{
				// リピートなし
				basisAxis = GetBasisAxisDown() ;
			}

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
								Debug.LogWarning( "[PadFocusController][Move L] 遷移先の識別名に過去の遷移名が見つからない : 過去の遷移先 = " + currentInputElementOld.IdentityFromL + " / 現在の位置 = " + currentInputElementOld.Identity ) ;

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

						if( currentInputElementNew.Identity != currentInputElementOld.Identity )
						{
							// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
							currentInputElementNew.IdentityFromR = currentInputElementOld.Identity ;

							// アクティブな入力要素を遷移させる
							m_CurrentInputElementIdentity = currentInputElementNew.Identity ;
						}

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
								Debug.LogWarning( "[PadFocusController][Move D] 遷移先の識別名に過去の遷移名が見つからない : 過去の遷移先 = " + currentInputElementOld.IdentityFromD + " / 現在の位置 = " + currentInputElementOld.Identity ) ;

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

						if( currentInputElementNew.Identity != currentInputElementOld.Identity )
						{
							// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
							currentInputElementNew.IdentityFromU = currentInputElementOld.Identity ;

							// アクティブな入力要素を遷移させる
							m_CurrentInputElementIdentity = currentInputElementNew.Identity ;
						}

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
								Debug.LogWarning( "[PadFocusController][Move R] 遷移先の識別名に過去の遷移名が見つからない : 過去の遷移先 = " + currentInputElementOld.IdentityFromR + " / 現在の位置 = " + currentInputElementOld.Identity ) ;

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

						if( currentInputElementNew.Identity != currentInputElementOld.Identity )
						{
							// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
							currentInputElementNew.IdentityFromL = currentInputElementOld.Identity ;

							// アクティブな入力要素を遷移させる
							m_CurrentInputElementIdentity = currentInputElementNew.Identity ;
						}

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
								Debug.LogWarning( "[PadFocusController][Move U] 遷移先の識別名に過去の遷移名が見つからない : 過去の遷移先 = " + currentInputElementOld.IdentityFromU + " / 現在の位置 = " + currentInputElementOld.Identity ) ;

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

						if( currentInputElementNew.Identity != currentInputElementOld.Identity )
						{
							// 遷移元の識別名を遷移先の入力要素の→側の入力要素の識別名に記録する
							currentInputElementNew.IdentityFromD = currentInputElementOld.Identity ;

							// アクティブな入力要素を遷移させる
							m_CurrentInputElementIdentity = currentInputElementNew.Identity ;
						}

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

		// 基本アクシスを取得する(リピートなし)
		protected Vector2 GetBasisAxisDown()
		{
			Vector2 fixedAxis = Vector2.zero ;

			float v ;

			// ゲームパッド
			if( m_BasisAxisNumbers != null && m_BasisAxisNumbers.Length >  0 )
			{
				foreach( var basisAxisNumber in m_BasisAxisNumbers )
				{
					var axis = GamePad.GetAxisDown( basisAxisNumber ) ;

					// Ｘ入力
					v = axis.x <  0 ? - axis.x : axis.x ;   // 傾きの閾値判定用
					if( fixedAxis.x == 0 && v >  0.1f )
					{
						fixedAxis.x = axis.x ;
					}

					// Ｙ入力
					v = axis.y <  0 ? - axis.y : axis.y ;   // 傾きの閾値判定用
					if( fixedAxis.y == 0 && v >  0.1f )
					{
						fixedAxis.y = axis.y ;
					}
				}
			}

			// キーボード
			if( m_BasisAxisKeyCodeSets != null && m_BasisAxisKeyCodeSets.Length >  0 )
			{
				foreach( var basisAxisKeyCodeSet in m_BasisAxisKeyCodeSets )
				{
					if( basisAxisKeyCodeSet != null && basisAxisKeyCodeSet.Length == 4 )
					{
						if( fixedAxis.x == 0 )
						{
							if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 0 ] ) == true )
							{
								// →
								fixedAxis.x += 1 ;
							}
							if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 1 ] ) == true )
							{
								// ←
								fixedAxis.x -= 1 ;
							}
						}

						if( fixedAxis.y == 0 )
						{
							if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 2 ] ) == true )
							{
								// ↑
								fixedAxis.y += 1 ;
							}
							if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 3 ] ) == true )
							{
								// ↓
								fixedAxis.y -= 1 ;
							}
						}
					}
				}
			}

			// アクション
			if( string.IsNullOrEmpty( m_BasisAxisActionName ) == false )
			{
				if( GamePad.AcquireAxisAction( m_BasisAxisActionName, out var basisAxisNumbers, out var basisAxisKeyCodeSets, out var _ ) == true )
				{
					// ゲームパッド
					if( basisAxisNumbers != null && basisAxisNumbers.Length >  0 )
					{
						foreach( var basisAxisNumber in basisAxisNumbers )
						{
							if( basisAxisNumber >= 0 )
							{
								var axis = GamePad.GetAxisDown( basisAxisNumber ) ;

								// Ｘ入力
								v = axis.x <  0 ? - axis.x : axis.x ;   // 傾きの閾値判定用
								if( fixedAxis.x == 0 && v >  0.1f )
								{
									fixedAxis.x = axis.x ;
								}

								// Ｙ入力
								v = axis.y <  0 ? - axis.y : axis.y ;   // 傾きの閾値判定用
								if( fixedAxis.y == 0 && v >  0.1f )
								{
									fixedAxis.y = axis.y ;
								}
							}
						}
					}

					// キーボード
					if( basisAxisKeyCodeSets != null && basisAxisKeyCodeSets.Length >  0 )
					{
						foreach( var basisAxisKeyCodeSet in basisAxisKeyCodeSets )
						{
							if( basisAxisKeyCodeSet != null && basisAxisKeyCodeSet.Length == 4 )
							{
								if( fixedAxis.x == 0 )
								{
									if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 0 ] ) == true )
									{
										// →
										fixedAxis.x += 1 ;
									}
									if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 1 ] ) == true )
									{
										// ←
										fixedAxis.x -= 1 ;
									}
								}

								if( fixedAxis.y == 0 )
								{
									if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 2 ] ) == true )
									{
										// ↑
										fixedAxis.y += 1 ;
									}
									if( Keyboard.GetKeyDown( basisAxisKeyCodeSet[ 3 ] ) == true )
									{
										// ↓
										fixedAxis.y -= 1 ;
									}
								}
							}
						}
					}
				}
			}

			return fixedAxis ;
		}


		// 基本アクシスを取得する(リピートあり)
		protected Vector2 GetBasisAxisRepeat()
		{
			Vector2 fixedAxis = Vector2.zero ;

			float v ;

			// ゲームパッド
			if( m_BasisAxisNumbers != null && m_BasisAxisNumbers.Length >  0 )
			{
				foreach( var basisAxisNumber in m_BasisAxisNumbers )
				{
					var axis = GamePad.GetAxisRepeat( basisAxisNumber ) ;

					// Ｘ入力
					v = axis.x <  0 ? - axis.x : axis.x ;   // 傾きの閾値判定用
					if( fixedAxis.x == 0 && v >  0.1f )
					{
						fixedAxis.x = axis.x ;
					}

					// Ｙ入力
					v = axis.y <  0 ? - axis.y : axis.y ;   // 傾きの閾値判定用
					if( fixedAxis.y == 0 && v >  0.1f )
					{
						fixedAxis.y = axis.y ;
					}
				}
			}

			// キーボード
			if( m_BasisAxisKeyCodeSets != null && m_BasisAxisKeyCodeSets.Length >  0 )
			{
				foreach( var basisAxisKeyCodeSet in m_BasisAxisKeyCodeSets )
				{
					if( basisAxisKeyCodeSet != null && basisAxisKeyCodeSet.Length == 4 )
					{
						if( fixedAxis.x == 0 )
						{
							if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 0 ] ) == true )
							{
								// →
								fixedAxis.x += 1 ;
							}
							if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 1 ] ) == true )
							{
								// ←
								fixedAxis.x -= 1 ;
							}
						}

						if( fixedAxis.y == 0 )
						{
							if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 2 ] ) == true )
							{
								// ↑
								fixedAxis.y += 1 ;
							}
							if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 3 ] ) == true )
							{
								// ↓
								fixedAxis.y -= 1 ;
							}
						}
					}
				}
			}

			// アクション
			if( string.IsNullOrEmpty( m_BasisAxisActionName ) == false )
			{
				if( GamePad.AcquireAxisAction( m_BasisAxisActionName, out var basisAxisNumbers, out var basisAxisKeyCodeSets, out var _ ) == true )
				{
					// ゲームパッド
					if( basisAxisNumbers != null && basisAxisNumbers.Length >  0 )
					{
						foreach( var basisAxisNumber in basisAxisNumbers )
						{
							if( basisAxisNumber >= 0 )
							{
								var axis = GamePad.GetAxisRepeat( basisAxisNumber ) ;

								// Ｘ入力
								v = axis.x <  0 ? - axis.x : axis.x ;   // 傾きの閾値判定用
								if( fixedAxis.x == 0 && v >  0.1f )
								{
									fixedAxis.x = axis.x ;
								}

								// Ｙ入力
								v = axis.y <  0 ? - axis.y : axis.y ;   // 傾きの閾値判定用
								if( fixedAxis.y == 0 && v >  0.1f )
								{
									fixedAxis.y = axis.y ;
								}
							}
						}
					}

					// キーボード
					if( basisAxisKeyCodeSets != null && basisAxisKeyCodeSets.Length >  0 )
					{
						foreach( var basisAxisKeyCodeSet in basisAxisKeyCodeSets )
						{
							if( basisAxisKeyCodeSet != null && basisAxisKeyCodeSet.Length == 4 )
							{
								if( fixedAxis.x == 0 )
								{
									if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 0 ] ) == true )
									{
										// →
										fixedAxis.x += 1 ;
									}
									if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 1 ] ) == true )
									{
										// ←
										fixedAxis.x -= 1 ;
									}
								}

								if( fixedAxis.y == 0 )
								{
									if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 2 ] ) == true )
									{
										// ↑
										fixedAxis.y += 1 ;
									}
									if( Keyboard.GetKeyRepeat( basisAxisKeyCodeSet[ 3 ] ) == true )
									{
										// ↓
										fixedAxis.y -= 1 ;
									}
								}
							}
						}
					}
				}
			}

			return fixedAxis ;
		}

		// 拡張アクシスを処理する
		protected bool ProcessExtraAxis( InputTypes inputType )
		{
			if( UIEventSystem.IsInputFieldFocused() == true )
			{
				// InputField が入力状態になっている
				return false ;
			}

			//-------------------------

			bool inputFlags = false ;

			// ゲームパッド＆キーボード
			if( m_ExtraAxisNumbers != null && m_ExtraAxisNumbers.Length >  0 )
			{
				Vector2 axis ;
				float vx, vy ;    // 傾きの絶対値

				// 個別に処理する
				foreach( var extraAxisNumber in m_ExtraAxisNumbers )
				{
					if( extraAxisNumber >= 0 )
					{
						axis = Vector2.zero ;
						vx = 0 ;
						vy = 0 ;

						if( inputType == InputTypes.Unknown || inputType == InputTypes.GamePad )
						{
							// ゲームパッド
							axis = GamePad.GetAxisRepeat( extraAxisNumber ) ;

							vx = Mathf.Abs( axis.x ) ;
							vy = Mathf.Abs( axis.y ) ;
						}

						if( inputType == InputTypes.Unknown || inputType == InputTypes.Keyboard )
						{
							// キーボード
							if( m_ExtraAxisKeyCodeSets != null && extraAxisNumber <  m_ExtraAxisKeyCodeSets.Length )
							{
								var extraAxisKeyCodeSet = m_ExtraAxisKeyCodeSets[ extraAxisNumber ] ;
								if( extraAxisKeyCodeSet != null && extraAxisKeyCodeSet.Length == 4 )
								{
									if( vx == 0 )
									{
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 0 ] ) == true )
										{
											// →
											axis.x = +1 ;
											vx += 1 ;
										}
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 1 ] ) == true )
										{
											// ←
											axis.x = -1 ;
											vx -= 1 ;
										}
										vx = vx < 0 ? - vx : vx ;
									}
									if( vy == 0 )
									{
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 2 ] ) == true )
										{
											// ↑
											axis.y = +1 ;
											vy += 1 ;
										}
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 3 ] ) == true )
										{
											// ↓
											axis.y = -1 ;
											vy -= 1 ;
										}
										vy = vy < 0 ? - vy : vy ;
									}
								}
							}
						}

						if( vx >  0.1f || vy >  0.1f )  // キャリブレーション閾値判定
						{
							inputFlags = true ;

							m_OnExtraAxis?.Invoke( extraAxisNumber, axis, m_CurrentInputElementIdentity ) ;
						}
					}
				}
			}

			// アクション
			if( m_ExtraAxisActionNames != null && m_ExtraAxisActionNames.Length >  0 )
			{
				Vector2 axis ;
				float vx, vy ;    // 傾きの絶対値

				foreach( var extraAxisActionName in m_ExtraAxisActionNames )
				{
					if( GamePad.AcquireAxisAction( extraAxisActionName, out var extraAxisNumbers, out var extraAxisKeyCodeSets, out var _ ) == true )
					{
						axis.x = 0 ;
						axis.y = 0 ;
						vx = 0 ;
						vy = 0 ;

						// ゲームパッド
						if( extraAxisNumbers != null && extraAxisNumbers.Length >  0 )
						{
							foreach( var extraAxisNumber in extraAxisNumbers )
							{
								if( extraAxisNumber >= 0 )
								{
									axis = GamePad.GetAxisRepeat( extraAxisNumber ) ;

									vx = axis.x <  0 ? - axis.x : axis.x ;
									vy = axis.y <  0 ? - axis.y : axis.y ;
								}
							}
						}

						// キーボード
						if( extraAxisKeyCodeSets != null && extraAxisKeyCodeSets.Length >  0 )
						{
							foreach( var extraAxisKeyCodeSet in extraAxisKeyCodeSets )
							{
								if( extraAxisKeyCodeSet != null && extraAxisKeyCodeSet.Length == 4 )
								{
									if( vx == 0 )
									{
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 0 ] ) == true )
										{
											// →
											axis.x = +1 ;
											vx += 1 ;
										}
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 1 ] ) == true )
										{
											// ←
											axis.x = -1 ;
											vx -= 1 ;
										}
										vx = vx <  0 ? - vx : vx ;
									}
									if( vy == 0 )
									{
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 2 ] ) == true )
										{
											// ↑
											axis.y = +1 ;
											vy += 1 ;
										}
										if( Keyboard.GetKeyRepeat( extraAxisKeyCodeSet[ 3 ] ) == true )
										{
											// ↓
											axis.y = -1 ;
											vy -= 1 ;
										}
										vy = vy < 0 ? - vy : vy ;
									}
								}
							}
						}

						if( vx >  0.1f || vy >  0.1f )  // キャリブレーション閾値判定
						{
							inputFlags = true ;

							m_OnExtraAxisAction?.Invoke( extraAxisActionName, axis, m_CurrentInputElementIdentity, m_ExtraAxisActionObject ) ;
						}
					}
				}
			}

			return inputFlags ;
		}

		//-------------------------------------------------------------------------------------

		// 決定ボタンを判定する
		protected bool ProcessDecisionButton()
		{
			if( UIEventSystem.IsInputFieldFocused() == true )
			{
				// InputField が入力状態になっている
				return false ;
			}

			//-------------------------

			bool isDecisionButtonPressing = false ;

			// ゲームパッド
			if( m_DecisionButtonIdentities != null && m_DecisionButtonIdentities.Length >  0 )
			{
				foreach( var decisionButtonIdentity in m_DecisionButtonIdentities )
				{
					if( GamePad.GetButton( decisionButtonIdentity ) == true )
					{
						isDecisionButtonPressing = true ;
						break ;
					}
				}
			}

			// キーボード
			if( m_DecisionButtonKeyCodes != null && m_DecisionButtonKeyCodes.Length >  0 )
			{
				foreach( var decisionButtonKeyCode in m_DecisionButtonKeyCodes )
				{
					if( Keyboard.GetKey( decisionButtonKeyCode ) == true )
					{
						isDecisionButtonPressing = true ;
						break ;
					}
				}
			}

			// アクション
			if( string.IsNullOrEmpty( m_DecisionButtonActionName ) == false )
			{
				if( GamePad.AcquireButtonAction( m_DecisionButtonActionName, out var decisionButtonIdentities, out var decisionButtonKeyCodes, out var decisionButtonMouseButtons ) == true )
				{
					// ゲームパッド
					if( decisionButtonIdentities != null && decisionButtonIdentities.Length >  0 )
					{
						foreach( var decisionButtonIdentity in decisionButtonIdentities )
						{
							if( GamePad.GetButton( decisionButtonIdentity ) == true )
							{
								isDecisionButtonPressing = true ;
								break ;
							}
						}
					}

					// キーボード
					if( decisionButtonKeyCodes != null && decisionButtonKeyCodes.Length >  0 )
					{
						foreach( var decisionButtonKeyCode in decisionButtonKeyCodes )
						{
							if( Keyboard.GetKey( decisionButtonKeyCode ) == true )
							{
								isDecisionButtonPressing = true ;
								break ;
							}
						}
					}
#if false
					// マウス
					if( decisionButtonMouseButtons != null && decisionButtonMouseButtons.Length >  0 )
					{
						foreach( var decisionButtonMouseButton in decisionButtonMouseButtons )
						{
							if( decisionButtonMouseButton >= 0 && Mouse.GetButton( decisionButtonMouseButton ) == true )
							{
								isDecisionButtonPressing = true ;
								break ;
							}
						}
					}
#endif
				}
			}

			return isDecisionButtonPressing ;
		}

		// 取消ボタンを判定する
		protected bool ProcessCancelButton()
		{
			if( m_InputType == InputTypes.Keyboard )
			{
				if( UIEventSystem.IsInputFieldFocused() == true )
				{
					// InputField が入力状態になっている
					return false ;
				}
			}

			//-------------------------

			bool isCancelButtonPressing = false ;

			// ゲームパッド
			if( m_CancelButtonIdentities != null && m_CancelButtonIdentities.Length >  0 )
			{
				foreach( var cancelButtonIdentity in m_CancelButtonIdentities )
				{
					if( GamePad.GetButton( cancelButtonIdentity ) == true )
					{
						isCancelButtonPressing = true ;
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
						isCancelButtonPressing = true ;
						break ;
					}
				}
			}

			// アクション
			if( string.IsNullOrEmpty( m_CancelButtonActionName ) == false )
			{
				if( GamePad.AcquireButtonAction( m_CancelButtonActionName, out var cancelButtonIdentities, out var cancelButtonKeyCodes, out var cancelButtonMouseButtons ) == true )
				{
					// ゲームパッド
					if( cancelButtonIdentities != null && cancelButtonIdentities.Length >  0 )
					{
						foreach( var cancelButtonIdentity in cancelButtonIdentities )
						{
							if( GamePad.GetButton( cancelButtonIdentity ) == true )
							{
								isCancelButtonPressing = true ;
								break ;
							}
						}
					}

					// キーボード
					if( cancelButtonKeyCodes != null && cancelButtonKeyCodes.Length >  0 )
					{
						foreach( var cancelButtonKeyCode in cancelButtonKeyCodes )
						{
							if( Keyboard.GetKey( cancelButtonKeyCode ) == true )
							{
								isCancelButtonPressing = true ;
								break ;
							}
						}
					}
#if false
					// マウス
					if( cancelButtonMouseButtons != null && cancelButtonMouseButtons.Length >  0 )
					{
						foreach( var cancelButtonMouseButton in cancelButtonMouseButtons )
						{
							if( cancelButtonMouseButton >= 0 && Mouse.GetButton( cancelButtonMouseButton ) == true )
							{
								isCancelButtonPressing = true ;
								break ;
							}
						}
					}
#endif
				}
			}

			return isCancelButtonPressing ;
		}

		// 基本ボタンを処理する
		protected void ProcessBasisButton()
		{
			// 決定ボタンの押下状態を取得
			bool isDecisionButtonPressing   = ProcessDecisionButton() ;

			//-------------------------------------------------

			// 取消ボタンの押下状態を取得
			bool isCancelButtonPressing     = ProcessCancelButton() ;

			//----------------------------------------------------------

			if( isDecisionButtonPressing == false && isCancelButtonPressing == false )
			{
				// 解放

				m_IsButtonBlocking          = false ;

				m_IsDecisionButtonPressing  = false ;
				m_IsCancelButtonPressing    = false ;

				m_IsInvalidDecisionButton   = false ;
				m_IsInvalidCancelButton     = false ;
			}

			//----------------------------------------------------------

			int inputFlags = 0 ;

			if( m_IsButtonBlocking == false )
			{
				//-------------------------
				// 決定ボタン

				if( m_IsInvalidDecisionButton == false )
				{
					// 最初から押しっぱなしを行っていて無効になっていなければ処理する

					if( isDecisionButtonPressing == true )
					{
						// 決定ボタンは押されている

						if( m_IsDecisionButtonPressing == false )
						{
							// 決定ボタンは押されていなかった

							// 決定ボタンを押された状態にする
							m_IsDecisionButtonPressing          = true ;

							m_IsDecisionButtonLongPressed       = false ;
							m_DecisionButtonLongPressingTime    = 0 ;

							inputFlags |= 0x01 ;   // 決定ボタンの Down
						}

						// 長押し判定
						if( m_IsDecisionButtonLongPressed == false )
						{
							m_DecisionButtonLongPressingTime += Time.fixedDeltaTime ;
							if( m_DecisionButtonLongPressingTime >  m_ButtonLongPressConfirmationTime )
							{
								m_IsDecisionButtonLongPressed = true ;

								inputFlags |= 0x04 ;   // 決定ボタンの LongPressed
							}
						}
					}
					else
					{
						// 決定ボタンは離されている

						if( m_IsDecisionButtonPressing == true )
						{
							// 決定ボタンが離された
							m_IsDecisionButtonPressing = false ;

							if( m_IsDecisionButtonDown == true )
							{
								m_IsDecisionButtonDown  = false ;

								inputFlags |= 0x02 ;   // 決定ボタンの Up
							}
						}
					}
				}

				//-------------------------
				// 取消ボタン

				if( m_IsInvalidCancelButton == false )
				{
					// 最初から押しっぱなしを行っていて無効になっていなければ処理する

					if( isCancelButtonPressing == true )
					{
						// 取消ボタンは押されている

						if( m_IsCancelButtonPressing == false )
						{
							// 取消ボタンは押されていなかった

							// 取消ボタンを押された状態にする
							m_IsCancelButtonPressing            = true ;

							m_IsCancelButtonLongPressed         = false ;
							m_CancelButtonLongPressingTime      = 0 ;

							m_IsCancelButtonDown                = true ;

							inputFlags |= 0x10 ;   // 取消ボタンの Down
						}

						// 長押し判定
						if( m_IsCancelButtonLongPressed == false )
						{
							m_CancelButtonLongPressingTime += Time.fixedDeltaTime ;
							if( m_CancelButtonLongPressingTime >  m_ButtonLongPressConfirmationTime )
							{
								m_IsCancelButtonLongPressed = true ;

								inputFlags |= 0x40 ;   // 取消ボタンの LongPressed
							}
						}
					}
					else
					{
						// 取消ボタンは離されている

						if( m_IsCancelButtonPressing == true )
						{
							// 取消ボタンが離された
							m_IsCancelButtonPressing = false ;

							if( m_IsCancelButtonDown == true )
							{
								m_IsCancelButtonDown  = false ;

								inputFlags |= 0x20 ;   // 取消ボタンの Up
							}
						}
					}
				}
			}

			//-------------------------------------------------
			// 決定ボタン

			if( inputFlags == 0x01 )
			{
				// 決定ボタン Down

				// 現在アクティブになっている入力要素を取得する
				var currentInputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( currentInputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity + "\n" + Path ) ;
					return ;
				}

				// 決定ボタンのコールバック呼び出し
				currentInputElement.OnDecision?.Invoke( ButtonActionTypes.Down ) ;
				m_OnDecision?.Invoke( currentInputElement.Identity, ButtonActionTypes.Down ) ;

				// 決定ボタン Up は発行される
				m_IsDecisionButtonDown              = true ;
			}
			else
			if( inputFlags == 0x02 )
			{
				// 決定ボタン Up

				// 現在アクティブになっている入力要素を取得する
				var currentInputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( currentInputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity ) ;
					return ;
				}

				// 決定ボタンのコールバック呼び出し
				currentInputElement.OnDecision?.Invoke( ButtonActionTypes.Up ) ;
				m_OnDecision?.Invoke( currentInputElement.Identity, ButtonActionTypes.Up ) ;
			}
			else
			if( inputFlags == 0x04 )
			{
				// 決定ボタン LongPressed

				// 現在アクティブになっている入力要素を取得する
				var currentInputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( currentInputElement == null )
				{
					Debug.LogWarning( "[PadFocusController] 識別名が不正です = " + m_CurrentInputElementIdentity ) ;
					return ;
				}

				// 決定ボタンのコールバック呼び出し
				currentInputElement.OnDecision?.Invoke( ButtonActionTypes.LongPressed ) ;
				m_OnDecision?.Invoke( currentInputElement.Identity, ButtonActionTypes.LongPressed ) ;
			}

			//-------------------------------------------------
			// 取消ボタン

			if( inputFlags == 0x10 )
			{
				// 取消ボタン Down

				string currentInputElementIdentity = m_OnCancel?.Invoke( m_CurrentInputElementIdentity, ButtonActionTypes.Down ) ;

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

				// 取消ボタン Up は発行される
				m_IsCancelButtonDown              = true ;
			}
			else
			if( inputFlags == 0x20 )
			{
				// 取消ボタン Up

				m_OnCancel?.Invoke( m_CurrentInputElementIdentity, ButtonActionTypes.Up ) ;
			}
			else
			if( inputFlags == 0x40 )
			{
				// 取消ボタン LongPressed

				m_OnCancel?.Invoke( m_CurrentInputElementIdentity, ButtonActionTypes.LongPressed ) ;
			}

			//-------------------------------------------------

		}

		// 拡張ボタンを処理する
		protected void ProcessExtraButton( InputTypes inputType )
		{
			if( UIEventSystem.IsInputFieldFocused() == true )
			{
				// InputField が入力状態になっている
				return ;
			}

			//-------------------------------------------------

			// 拡張ボタン
			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				int inputFlags = 0 ;

				int i, l = m_ExtraButtonIdentities.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_IsExtraButtonIdentities_Pressing[ i ] == false )
					{
						if( inputType == InputTypes.Unknown || inputType == InputTypes.GamePad )
						{
							// ゲームパッド
							var extraButtonIdentity = m_ExtraButtonIdentities[ i ] ;
							if( GamePad.GetButtonDown( extraButtonIdentity ) == true )
							{
								m_IsExtraButtonIdentities_Pressing[ i ] = true ;
								inputFlags |= ( 4 << i ) ;
							}
						}

						if( inputType == InputTypes.Unknown || inputType == InputTypes.Keyboard )
						{
							// キーボード
							if( m_ExtraButtonKeyCodeSets != null && i <  m_ExtraButtonKeyCodeSets.Length )
							{
								var extraButtonKeyCodeSet = m_ExtraButtonKeyCodeSets[ i ] ;
								if( extraButtonKeyCodeSet != null && extraButtonKeyCodeSet.Length >  0 )
								{
									foreach( var extraButtonKeyCode in extraButtonKeyCodeSet )
									{
										// Keyboard.GetKey( ... ) にしてはならない
										if( Keyboard.GetKeyDown( extraButtonKeyCode ) == true )
										{
											m_IsExtraButtonIdentities_Pressing[ i ] = true ;
											inputFlags |= ( 4 << i ) ;
											break ;
										}
									}
								}
							}
						}
					}
					else
					{
						bool isPressing = false ;

						if( inputType == InputTypes.Unknown || inputType == InputTypes.GamePad )
						{
							// ゲームパッド
							var extraButtonIdentity = m_ExtraButtonIdentities[ i ] ;
							if( GamePad.GetButton( extraButtonIdentity ) == true )
							{
								isPressing = true ;
							}
						}

						if( inputType == InputTypes.Unknown || inputType == InputTypes.Keyboard )
						{
							// キーボード
							if( m_ExtraButtonKeyCodeSets != null && i <  m_ExtraButtonKeyCodeSets.Length )
							{
								var extraButtonKeyCodeSet = m_ExtraButtonKeyCodeSets[ i ] ;
								if( extraButtonKeyCodeSet != null && extraButtonKeyCodeSet.Length >  0 )
								{
									foreach( var extraButtonKeyCode in extraButtonKeyCodeSet )
									{
										if( Keyboard.GetKey( extraButtonKeyCode ) == true )
										{
											isPressing = true ;
											break ;
										}
									}
								}
							}
						}

						if( isPressing == false )
						{
							m_IsExtraButtonIdentities_Pressing[ i ] = false ;
						}
					}
				}

				//---------------------------------------------

				// 決定ボタンと取消ボタンのフラグを削除
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

			//----------------------------------------------------------
			// 入力アクション

			if( m_ExtraButtonActionNames != null && m_ExtraButtonActionNames.Length >  0 )
			{
				int inputFlags = 0 ;

				int i, l = m_ExtraButtonActionNames.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_IsExtraButtonActionNames_Pressing[ i ] == false )
					{
						var extraButtonActionName = m_ExtraButtonActionNames[ i ] ;
						if( GamePad.AcquireButtonAction( extraButtonActionName, out var extraButtonIdentities, out var extraButtonKeyCodeSet, out var extraButtonMouseButtons ) == true )
						{
							if( inputType == InputTypes.Unknown || inputType == InputTypes.GamePad )
							{
								// ゲームパッド
								if( extraButtonIdentities != null && extraButtonIdentities.Length >  0 )
								{
									foreach( var extraButtonIdentity in extraButtonIdentities )
									{
										if( GamePad.GetButtonDown( extraButtonIdentity ) == true )
										{
											m_IsExtraButtonActionNames_Pressing[ i ] = true ;
											inputFlags |= ( 4 << i ) ;
										}
									}
								}
							}

							if( inputType == InputTypes.Unknown || inputType == InputTypes.Keyboard )
							{
								// キーボード
								if( extraButtonKeyCodeSet != null && extraButtonKeyCodeSet.Length >  0 )
								{
									foreach( var extraButtonKeyCode in extraButtonKeyCodeSet )
									{
										// Keyboard.GetKey( ... ) にしてはならない
										if( Keyboard.GetKeyDown( extraButtonKeyCode ) == true )
										{
											m_IsExtraButtonActionNames_Pressing[ i ] = true ;
											inputFlags |= ( 4 << i ) ;
											break ;
										}
									}
								}
#if false
								// マウス
								if( extraButtonMouseButtons != null && extraButtonMouseButtons.Length >  0 )
								{
									foreach( var extraButtonMouseButton in extraButtonMouseButtons )
									{
										// Mouse.GetButton( ... ) にしてはならない
										if( extraButtonMouseButton >= 0 && Mouse.GetButtonDown( extraButtonMouseButton ) == true )
										{
											m_IsExtraButtonActionNames_Pressing[ i ] = true ;
											inputFlags |= ( 4 << i ) ;
											break ;
										}
									}
								}
#endif
							}
						}
					}
					else
					{
						bool isPressing = false ;

						var extraButtonActionName = m_ExtraButtonActionNames[ i ] ;
						if( GamePad.AcquireButtonAction( extraButtonActionName, out var extraButtonIdentities, out var extraButtonKeyCodeSet, out var extraButtonMouseButtons ) == true )
						{
							if( inputType == InputTypes.Unknown || inputType == InputTypes.GamePad )
							{
								// ゲームパッド
								if( extraButtonIdentities != null && extraButtonIdentities.Length >  0 )
								{
									foreach( var extraButtonIdentity in extraButtonIdentities )
									{
										if( GamePad.GetButton( extraButtonIdentity ) == true )
										{
											isPressing = true ;
										}
									}
								}
							}

							if( inputType == InputTypes.Unknown || inputType == InputTypes.Keyboard )
							{
								// キーボード
								if( extraButtonKeyCodeSet != null && extraButtonKeyCodeSet.Length >  0 )
								{
									foreach( var extraButtonKeyCode in extraButtonKeyCodeSet )
									{
										if( Keyboard.GetKey( extraButtonKeyCode ) == true )
										{
											isPressing = true ;
											break ;
										}
									}
								}
#if false
								// マウス
								if( extraButtonMouseButtons != null && extraButtonMouseButtons.Length >  0 )
								{
									foreach( var extraButtonMouseButton in extraButtonMouseButtons )
									{
										if( extraButtonMouseButton >= 0 && Mouse.GetButton( extraButtonMouseButton ) == true )
										{
											isPressing = true ;
											break ;
										}
									}
								}
#endif
							}
						}

						if( isPressing == false )
						{
							m_IsExtraButtonActionNames_Pressing[ i ] = false ;
						}
					}
				}

				//---------------------------------------------

				// 決定ボタンと取消ボタンのフラグを削除
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
					m_OnExtraButtonAction?.Invoke( m_ExtraButtonActionNames[ index ], m_CurrentInputElementIdentity, m_ExtraButtonActionObject ) ;
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
		/// 入力要素の移動履歴を消去する(指定した要素のみ)
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


		/// <summary>
		/// 入力要素の移動履歴を消去する(全て)
		/// </summary>
		public void ClearHistory()
		{
			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				return ;
			}

			foreach( var inputElement in m_InputElements )
			{
				inputElement.IdentityFromL = null ;
				inputElement.IdentityFromR = null ;
				inputElement.IdentityFromU = null ;
				inputElement.IdentityFromD = null ;
			}
		}

	}   // class
}   // namespace
