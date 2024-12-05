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


		[SerializeField]
		private string  m_DefaultInputElementIdentity ;

		// 有効化されている(フォーカスを得ている)入力要素
		[SerializeField]
		private string  m_CurrentInputElementIdentity ;

		[SerializeField]
		private bool    m_IsCurrentInputElementClear ;

		[SerializeField]
		private string  m_HistoryInputElementIdentiry ;

		//-----------------------------------------------------
		// 全体用のコールバック

		private Func<string,string[]> m_OnMoveL ;
		private Func<string,string[]> m_OnMoveR ;
		private Func<string,string[]> m_OnMoveU ;
		private Func<string,string[]> m_OnMoveD ;

		private Action<string,bool>   m_OnFocusChanged ;

		// 決定ボタンの識別子
		private int m_DecisionButtonIdentity = GamePad.B1 ;

		// 決定ボタンの入力があった際に呼ばれるコールバック
		private Action<string,bool>   m_OnDecision ;

		// 決定ボタンは、押した際と離した際に、個別にコールバックを呼ぶため、その状態判定用の値
		private bool m_IsDecisionButtonPressing = false ;

		// キャンセルボタンの識別子
		private int m_CancelButtonIdentity = GamePad.B2 ;

		// キャンセルボタンの入力があった際に呼ばれるコールバック
		private Func<string,string>   m_OnCancel ;

		// 拡張ボタンの識別子群
		private int[] m_ExtraButtonIdentities = null ;

		// 拡張ボタンの入力があった際に呼ばれるコールバック
		private Action<int,string>    m_OnExtraButton ; 

		//-----------------------------

		// 基本アクシスの識別番号群(カーソル移動に関係あり)
		private int[] m_BasisAxisNumbers = { 0 } ;

		// 拡張アクシスの識別番号群(カーソル移動に関係なし
		private int[] m_ExtraAxisNumbers = null ;

		// 拡張アクシスの入力があった際に呼ばれるコールバック
		private Action<int,Vector2,string>  m_OnExtraAxis ;

		// 入力モードが切り替わった際に呼び出されるコールバック
		private Action<InputTypes>    m_OnInputTypeChanged ;

		//-----------------------------------------------------

		//-----------------------------------------------------

		// 現在ゲームパッドの入力が有効な状態になっているかどうか(実際の入力ではなくこのクラス内での入力)
		private bool m_IsGamePadMode ;

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

				// 入力方法の変化の監視対象に追加
				UIEventSystem.AddOnInputTypeChanged( OnInputTypeChanged ) ;
			}
		}

		// 破棄時に呼び出される
		protected override void OnDestroy()
		{
			base.OnDestroy() ;

			if( Application.isPlaying == true )
			{
				// 入力方法の変化の監視対象から削除
				UIEventSystem.RemoveOnInputTypeChanged( OnInputTypeChanged ) ;
			}
		}

		// 入力方法が変化したら呼び出されるコールバックメソッド
		protected void OnInputTypeChanged( InputTypes inputType )
		{
			if( inputType == InputTypes.Pointer )
			{
//              Debug.Log( "<color=#FFFF00>[PadCursorController]Pointer Mode !</color>" ) ;

				// ゲームパッドモードを失う
				m_IsGamePadMode = false ;

				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

				if( m_IsCurrentInputElementClear == true )
				{
					m_HistoryInputElementIdentiry = m_CurrentInputElementIdentity ;
					m_CurrentInputElementIdentity = null ;
				}

				UpdateCursors() ;

				m_OnInputTypeChanged?.Invoke( InputTypes.Pointer ) ;

				m_IsDecisionButtonPressing = false ;
			}
			else
			if( inputType == InputTypes.GamePad )
			{
//              Debug.Log( "<color=#FFFF00>[PadCursorController]GamePad Mode !</color>" ) ;

				if( UIEventSystem.InputProcessingType == InputProcessingTypes.Switching )
				{
					// 特定の入力のケースのみ入力制限を解除する

					var button = GetButton() ;
					var axis   = GetBasisAxis() ;
					if
					(
						button != 0 ||
						Mathf.Abs( axis.x ) >  0.1f || Mathf.Abs( axis.y ) >  0.1f
					)
					{
						// 入力が有効になる
						m_IsGamePadMode = true ;

						if( string.IsNullOrEmpty( m_HistoryInputElementIdentiry ) == true )
						{
							m_CurrentInputElementIdentity = m_DefaultInputElementIdentity ;
						}
						else
						{
							m_CurrentInputElementIdentity = m_HistoryInputElementIdentiry ;
						}

						// フォーカスを得る
						CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

						UpdateCursors() ;
					}
				}
				else
				{
					// フォーカスを得る
					CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

					UpdateCursors() ;
				}

				m_OnInputTypeChanged?.Invoke( InputTypes.GamePad ) ;
			}
		}

		// ボタン入力を取得する
		protected int GetButton()
		{
			int buttonFlags = 0 ;

			if( m_DecisionButtonIdentity != 0 )
			{
				if( GamePad.GetButton( m_DecisionButtonIdentity ) == true )
				{
					buttonFlags |= 1 ;
				}
			}

			if( m_CancelButtonIdentity != 0 )
			{
				if( GamePad.GetButton( m_CancelButtonIdentity   ) == true )
				{
					buttonFlags |= 2 ;
				}
			}

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				int i, l = m_ExtraButtonIdentities.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var extraActionButtonIdentity = m_ExtraButtonIdentities[ i ] ;
					if( extraActionButtonIdentity != 0 )
					{
						if( GamePad.GetButton( extraActionButtonIdentity ) == true )
						{
							buttonFlags |= ( 4 << i ) ;
						}
					}
				}
			}

			return buttonFlags ;
		}

		// 方向入力を取得する
		protected Vector2 GetBasisAxis()
		{
			Vector2 fixedAxis = Vector2.zero ;

			foreach( var axisNumber in m_BasisAxisNumbers )
			{
				var axis = GamePad.GetAxis( axisNumber ) ;

				// Ｘ入力
				if( fixedAxis.x == 0 && Mathf.Abs( axis.x ) >  0.1f )
				{
					fixedAxis.x = axis.x ;
				}

				// Ｙ入力
				if( fixedAxis.y == 0 && Mathf.Abs( axis.y ) >  0.1f )
				{
					fixedAxis.y = axis.y ;
				}
			}

			return fixedAxis ;
		}

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 入力要素を設定する
		/// </summary>
		public void SetInputElements
		(
			List<InputElement> inputElements,
			string currentInputElementIdentity,
			string defaultInputElementIdentity = null,
			Func<string,string[]> onMoveL = null,
			Func<string,string[]> onMoveR = null,
			Func<string,string[]> onMoveU = null,
			Func<string,string[]> onMoveD = null,
			Action<string,bool> onFocusChanged = null,
			Action<string,bool> onDecision = null,
			Func<string,string> onCancel = null,
			Action<InputTypes> onInputTypeChanged = null
		)
		{
			if( m_InputElements != null && m_InputElements.Count >  0 )
			{
				// 古い入力要素が設定されていたらカーソルを全て消去する

				// フォーカスを失う
				CallOnFocusChanged( m_CurrentInputElementIdentity, false ) ;

				m_CurrentInputElementIdentity = null ;

				UpdateCursors() ;
			}

			//-------------------------------------------------

			m_InputElements               = inputElements ;
			m_CurrentInputElementIdentity = currentInputElementIdentity ;
			m_DefaultInputElementIdentity = defaultInputElementIdentity ;

			m_IsCurrentInputElementClear  = string.IsNullOrEmpty( m_CurrentInputElementIdentity ) ;

			if( string.IsNullOrEmpty( m_DefaultInputElementIdentity ) == true && string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == false )
			{
				m_DefaultInputElementIdentity = m_CurrentInputElementIdentity ;
			}

			m_HistoryInputElementIdentiry = null ;

			m_OnMoveL        = onMoveL ;
			m_OnMoveR        = onMoveR ;
			m_OnMoveU        = onMoveU ;
			m_OnMoveD        = onMoveD ;
			m_OnFocusChanged = onFocusChanged ;
			m_OnDecision     = onDecision ;
			m_OnCancel       = onCancel ;

			m_OnInputTypeChanged = onInputTypeChanged ;

			m_IsGamePadMode = ( string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == false ) && ( UIEventSystem.InputType == InputTypes.GamePad ) ;

			if( m_IsGamePadMode == true )
			{
				// フォーカスを得る
				CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;
			}

			UpdateCursors() ;

			// 設定直後のコール
			m_OnInputTypeChanged?.Invoke( UIEventSystem.InputType ) ;
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
				m_HistoryInputElementIdentiry = currentInputElementIdentity ;

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

		/// <summary>
		/// 基本入力ボタンを設定する
		/// </summary>
		/// <param name="decisionButtonIdentity"></param>
		/// <param name="cancalButtonIdentity"></param>
		/// <param name="availableAxisNumbers"></param>
		public void SetBasisConfiguration( int decisionButtonIdentity, int cancalButtonIdentity, params int[] basisAxisNumbers )
		{
			m_DecisionButtonIdentity = decisionButtonIdentity ;
			m_CancelButtonIdentity   = cancalButtonIdentity ;

			m_BasisAxisNumbers       = basisAxisNumbers ;
		}

		/// <summary>
		/// 拡張入力ボタンを設定する
		/// </summary>
		/// <param name="extraActionButtonIdentities"></param>
		/// <param name="onExtraAction"></param>
		public void SetExtraButtonConfiguration( int[] extraButtonIdentities, Action<int,string> onExtraButton )
		{
			m_ExtraButtonIdentities = extraButtonIdentities ;
			m_OnExtraButton         = onExtraButton ;
		}

		public void SetExtraAxisConfiguration( int[] extraAxisNumbers, Action<int,Vector2,string> onExtraAxis )
		{
			m_ExtraAxisNumbers      = extraAxisNumbers ;
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

			if( UIEventSystem.InputType == InputTypes.Pointer )
			{
				// パッド操作用のカーソルは表示しない
				return false ;
			}

			//---------------------------------------------------------------------------------

			if( string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == false )
			{
				// フォーカスを得ているもの
				var inputElement = m_InputElements.FirstOrDefault( _ => _.Identity == m_CurrentInputElementIdentity ) ;
				if( inputElement != null )
				{
					if( inputElement.Cursor != null )
					{
						// カーソル表示
						inputElement.Cursor.SetActive( true ) ;
					}

					return true ;
				}
			}

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

			if( UIEventSystem.InputType == InputTypes.Pointer )
			{
				// ポインターモードでは処理しない
				return ;
			}

			if( IsRaycastAvailable() == false )
			{
				// レイキャストが通る状態でなければ処理しない
				m_IsDecisionButtonPressing = false ;
				return ;
			}

			if( m_InputElements == null || m_InputElements.Count == 0 )
			{
				// アクティブになっている入力要素が存在しない場合は無効
				return ;
			}

			//---------------------------------------------------------------------------------

			if( m_IsGamePadMode == false )
			{
				if( UIEventSystem.InputProcessingType == InputProcessingTypes.Parallel )
				{
					// パラレルモードの場合は最初の入力は無視する

					// すべての入力
					int buttonAll = GamePad.GetButtonAll() ;
					var axis_0 = GamePad.GetAxis( 0 ) ;
					var axis_1 = GamePad.GetAxis( 1 ) ;
					var axis_2 = GamePad.GetAxis( 2 ) ;

					// ドリフト対策
					float ax0 = Mathf.Abs( axis_0.x ) ;
					float ay0 = Mathf.Abs( axis_0.y ) ;
					float ax1 = Mathf.Abs( axis_1.x ) ;
					float ay1 = Mathf.Abs( axis_1.y ) ;
					float ax2 = Mathf.Abs( axis_2.x ) ;
					float ay2 = Mathf.Abs( axis_2.y ) ;
					float ax = Mathf.Max( ax0, ax1, ax2 ) ;
					float ay = Mathf.Max( ay0, ay1, ay2 ) ;

					if( buttonAll != 0 || ax >  0.1f || ay >  0.1f )
					{
						if( string.IsNullOrEmpty( m_CurrentInputElementIdentity ) == true )
						{
							// カレントが空の場合は特定の入力でなければ入力が有効にならない

							var button = GetButton() ;
							var axis   = GetBasisAxis() ;
							if
							(
								button != 0 ||
								Mathf.Abs( axis.x ) >  0.1f || Mathf.Abs( axis.y ) >  0.1f
							)
							{
								// 入力が有効になる
								m_IsGamePadMode = true ;

								if( string.IsNullOrEmpty( m_HistoryInputElementIdentiry ) == true )
								{
									m_CurrentInputElementIdentity = m_DefaultInputElementIdentity ;
								}
								else
								{
									m_CurrentInputElementIdentity = m_HistoryInputElementIdentiry ;
								}

								// フォーカスを得る
								CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

								UpdateCursors() ;
							}

							// 最初の入力は無視する
							return ;
						}
						else
						{
							// 入力が有効になる
							m_IsGamePadMode = true ;

							// フォーカスを得る
							CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

							UpdateCursors() ;

							// 最初の入力は無視する
							return ;
						}
					}
				}
				else
				{
					// スイッチモード

					if( m_IsGamePadMode == false )
					{
						var button = GetButton() ;
						var axis   = GetBasisAxis() ;
						if
						(
							button != 0 ||
							Mathf.Abs( axis.x ) >  0.1f || Mathf.Abs( axis.y ) >  0.1f
						)
						{
							// 入力が有効になる
							m_IsGamePadMode = true ;

							if( string.IsNullOrEmpty( m_HistoryInputElementIdentiry ) == true )
							{
								m_CurrentInputElementIdentity = m_DefaultInputElementIdentity ;
							}
							else
							{
								m_CurrentInputElementIdentity = m_HistoryInputElementIdentiry ;
							}

							// フォーカスを得る
							CallOnFocusChanged( m_CurrentInputElementIdentity, true ) ;

							UpdateCursors() ;

							// 実際の反応は無視する
							return ;
						}
					}
				}
			}

			//-------------------------------------------------

			// 基本方向キーを処理する
			bool isBasisAxisInput = ProcessBasisAxis() ;

			// 拡張方向キーを処理する(基本方向キーやボタンと同時押し可能)
			ProcessExtraAxis() ;

			if( isBasisAxisInput == false )
			{
				// ボタンを処理する
				ProcessButton() ;
			}
		}

		// 基本方向キーの入力を処理する
		protected bool ProcessBasisAxis()
		{
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

		// 基本方向キーの入力を取得する(リピートあり)
		protected Vector2 GetBasisAxisRepeat()
		{
			Vector2 fixedAxis = Vector2.zero ;

			foreach( var axisNumber in m_BasisAxisNumbers )
			{
				var axis = GamePad.GetAxisRepeat( axisNumber ) ;

				// Ｘ入力
				if( fixedAxis.x == 0 && Mathf.Abs( axis.x ) >  0.1f )
				{
					fixedAxis.x = axis.x ;
				}

				// Ｙ入力
				if( fixedAxis.y == 0 && Mathf.Abs( axis.y ) >  0.1f )
				{
					fixedAxis.y = axis.y ;
				}
			}

			return fixedAxis ;
		}

		// 拡張方向キーの入力を処理する
		protected bool ProcessExtraAxis()
		{
			if( m_ExtraAxisNumbers == null || m_ExtraAxisNumbers.Length == 0 )
			{
				return false ;
			}

			//-------------------------------------------------

			bool inputFlags = false ;

			// 個別に処理する
			foreach( var axisNumber in m_ExtraAxisNumbers )
			{
				if( axisNumber >= 0 )
				{
					var axis = GamePad.GetAxisRepeat( axisNumber ) ;

					if( Mathf.Abs( axis.x ) >  0.1f || Mathf.Abs( axis.y ) >  0.1f )
					{
						inputFlags = true ;

						m_OnExtraAxis?.Invoke( axisNumber, axis, m_CurrentInputElementIdentity ) ;
					}
				}
			}

			return inputFlags ;
		}

		//-------------------------------------------------------------------------------------

		// 決定ボタンの入力を処理する
		protected void ProcessButton()
		{
			int inputFlags = 0 ;

			bool isDecisionButtonUp = false ;

			// 決定ボタン
			if( m_DecisionButtonIdentity != 0 )
			{
				if( m_IsDecisionButtonPressing == false )
				{
					// GamePad.GetButton( ... ) にしてはならない
					if( GamePad.GetButtonDown( m_DecisionButtonIdentity ) == true )
					{
						m_IsDecisionButtonPressing = true ;
						inputFlags |= 1 ;
					}
				}
				else
				{
					if( GamePad.GetButtonUp( m_DecisionButtonIdentity ) == true )
					{
						m_IsDecisionButtonPressing = false ;
						isDecisionButtonUp = true ;
					}
				}
			}

			// キャンセルボタン
			if( m_CancelButtonIdentity != 0 )
			{
				if( GamePad.GetButtonDown( m_CancelButtonIdentity ) == true )
				{
					inputFlags |= 2 ;
				}
			}

			// 拡張ボタン
			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				int i, l = m_ExtraButtonIdentities.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var extraActionButtonIdentity = m_ExtraButtonIdentities[ i ] ;
					if( extraActionButtonIdentity != 0 )
					{
						if( GamePad.GetButtonDown( extraActionButtonIdentity ) == true )
						{
							inputFlags |= ( 4 << i ) ;
						}
					}
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
			// キャンセルボタン

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

			//---------------------------------------------------------------------------------
			// 拡張ボタン

			if( m_ExtraButtonIdentities != null && m_ExtraButtonIdentities.Length >  0 )
			{
				// 決定ボタンとキャンセルボタンのフラグを削除
				int extraInputFlags = inputFlags >> 2 ;

				int index = -1, count = 0 ;

				int i, l = m_ExtraButtonIdentities.Length ;
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
