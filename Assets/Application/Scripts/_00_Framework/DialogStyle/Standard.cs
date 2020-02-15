using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using InputHelper ;

namespace DBS.DialogStyle
{
	/// <summary>
	/// スタンダードタイプのダイアログ
	/// </summary>
	public class Standard : DialogStyleBase
	{
		// タイトル
		[SerializeField]
		private UITextMesh	m_Title = null ;

		/// <summary>
		/// タイトル
		/// </summary>
		public UITextMesh Title
		{
			get
			{
				return m_Title ;
			}
		}

		//---------------

		// メッセージ
		[SerializeField]
		private UITextMesh	m_Message = null ;

		/// <summary>
		/// メッセージ
		/// </summary>
		public UITextMesh Message
		{
			get
			{
				return m_Message ;
			}
		}


		// ボタン
		[SerializeField]
		private UIView		m_ClosingButton = null ;

		[SerializeField]
		private UIView		m_SelectionButton = null ;

		//-----------------------------------------------------------
		
		/// <summary>
		/// 選択式ボタンのラベル
		/// </summary>
		public string[] SelectionButtonLabels = null ;

		/// <summary>
		/// 選択式ボタンの初期のインデックス
		/// </summary>
		public int		SelectionButtonIndex = 0 ;


		/// <summary>
		/// コールバック用のメソッド
		/// </summary>
		public Action<Standard,string,int> Callback = null ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる
		/// </summary>
		public bool AutoClose = false ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる際に呼ぶコールバック用のメソッド
		/// </summary>
		public Action<int> AutoCloseCallback = null ;
		
		/// <summary>
		/// ダイアログの表示状態
		/// </summary>
		public Dialog.State	state = null ;

		//-------------------------------------------------------------------------------------------

		private bool	m_Closing = false ;

		public bool		Focus = true ;

		private ClosingButtonExecutor	m_ClosingButtonExecutor		= null ;
		private SelectionButtonExecutor	m_SelectionButtonExecutor	= null ;


		//-------------------------------------------------------------------------------------------

		private void Awake()
		{
			m_ClosingButton.SetActive( false ) ;
			m_SelectionButton.SetActive( false ) ;
		}

		/// <summary>
		/// ＵＩの状態を完成させる
		/// </summary>
		override public void Commit()
		{
			if( m_Dirty == false )
			{
				return ;	// 既に実行済み
			}

			float h =   0 ;	// 縦幅

			h += 16 ;	// 上マージン

			if( m_Title != null && m_Title.ActiveSelf == true && string.IsNullOrEmpty( m_Title.Text ) == false )
			{
				// タイトルの表示がある
				h += m_Title.Height ;
				h += 8 ;
			}

			if( m_Message != null && m_Message.ActiveSelf == true && string.IsNullOrEmpty( m_Message.Text ) == false )
			{
				// メッセージの表示がある
				m_Message.Py = - h - ( m_Message.TextHeight * ( 1.0f - m_Message.Pivot.y ) ) ;
				
				h = h + m_Message.TextHeight ;
			}
			
			//----------------------------------------------------------
			// ボタンの設定

			Focus = true ;

			if( SelectionButtonLabels == null || SelectionButtonLabels.Length == 0 )
			{
				// 閉じるボタンのみ

				// 下マージンを加える
				h += 28 ;	// ClosingButton

				m_ClosingButton.SetActive( true ) ;
				m_SelectionButton.SetActive( false ) ;

				//---------------------------------

				m_ClosingButtonExecutor = new ClosingButtonExecutor() ;
				m_ClosingButtonExecutor.Commit( m_ClosingButton, OnAction ) ;
			}
			else
			{
				// 選択ボタン
				
				// 下マージンを加える
				h += 60 ;	// ClosingButton

				m_SelectionButton.SetActive( true ) ;
				m_ClosingButton.SetActive( false ) ;

				//---------------------------------

				m_SelectionButtonExecutor = new SelectionButtonExecutor() ;
				m_SelectionButtonExecutor.Commit( SelectionButtonLabels, SelectionButtonIndex, m_SelectionButton, OnAction ) ;
			}
			
			//----------------------------------------------------------

			// ウィンドウの縦幅決定
			m_Window.Height = h ;
			
			//----------------------------------------------------------

			m_Closing = false ;

			//----------------------------------------------------------

			m_Dirty = false ;	// 完了
		}

		/// <summary>
		/// ダイアログが表示されたら呼び出される
		/// </summary>
		override protected void Visible()
		{
			if( SelectionButtonLabels == null || SelectionButtonLabels.Length == 0 )
			{
				// ボタンなし
				if( m_ClosingButtonExecutor != null )
				{
					// Update の方が Commit より早いので null チェック必須
					m_ClosingButtonExecutor.Visible() ;
				}
			}
			else
			{
				// 複数ボタン
				if( m_SelectionButtonExecutor != null )
				{
					// Update の方が Commit より早いので null チェック必須
					m_SelectionButtonExecutor.Visible() ;
				}
			}
		}


		// GamePad 系の入力処理を行う
		void Update()
		{
			if( Focus == false )
			{
				// フォーカスが無い時は処理しない
				return ;
			}

			//----------------------------------------------------------

			if( SelectionButtonLabels == null || SelectionButtonLabels.Length == 0 )
			{
				if( m_ClosingButtonExecutor != null )
				{
					// Update の方が Commit より早いので null チェック必須
					m_ClosingButtonExecutor.Update() ;
				}
			}
			else
			{
				if( m_SelectionButtonExecutor != null )
				{
					// Update の方が Commit より早いので null チェック必須
					m_SelectionButtonExecutor.Update() ;
				}
			}
		}

		// ボタンが押された際に呼び出されるローカルのコールバックメソッド
		private void OnAction( int identity, string se )
		{
			if( m_Closing == true )
			{
				return ;
			}

			//----------------------------------

			// 音を出すかどうかは後で改めて考える
			if( string.IsNullOrEmpty( se ) == false )
			{
				SE.Play( se ) ;
			}
					
			// フォーカスを無効化
			Focus = false ;

			//----------------------------------

			if( Callback != null )
			{
				Callback( this, "clicked", identity ) ;
			}

			if( AutoClose == true )
			{
				m_Closing = true ;
				Close( identity ) ;
			}
		}

		// ダイアログをフエードアウト効果付きで非表示にして最後にクローズ時のデリゲードを呼び出す
		override protected IEnumerator FadeOut()
		{
			yield return StartCoroutine( base.FadeOut() ) ;

			// 最後にコールバックを呼び出して複製された自身も破棄する
			if( Callback != null )
			{
				Callback( this, "closed", m_Result ) ;	// 最後に閉じられた事を通知する
			}

			if( AutoClose == true && AutoCloseCallback != null )
			{
				AutoCloseCallback( m_Result ) ;
			}

			if( state != null )
			{
				state.Index = m_Result ;
				state.Result = m_Result ;
				state.IsDone = true ;
			}

			Destroy( gameObject ) ;
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// ボタン無しでの処理
		/// </summary>
		public class ClosingButtonExecutor
		{
			private	UIView		m_Pointer ;
			private	UIView		m_GamePad ;

			private	float		m_ModeFactor ;

			private	bool		m_IsHover ;

			//----------------------------------

			private	UIView		m_PointerButton ;
			private	UIImage		m_PointerCursor ;

			private	UIImage		m_GamePadIcon ;
	
			//----------------------------------

			private Action<int,string>	m_OnAction ;

			//----------------------------------------------------------

			public void Commit( UIView closingButton, Action<int,string> onAction )
			{
				m_OnAction = onAction ;
				
				//---------------------------------

				m_Pointer =  closingButton.FindNode<UIView>( "Pointer" ) ;
				m_Pointer.IsCanvasGroup = true ;
			
				m_PointerButton	= closingButton.FindNode<UIView>( "PointerButton" ) ;
				m_PointerButton.isInteraction = true ;
				m_PointerCursor	= closingButton.FindNode<UIImage>( "PointerCursor" ) ;

				//-------------

				m_GamePad =  closingButton.FindNode<UIView>( "GamePad" ) ;
				m_GamePad.IsCanvasGroup = true ;

				m_GamePadIcon	= closingButton.FindNode<UIImage>( "GamePadIcon" ) ;

				//---------------------------------

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード
					m_Pointer.SetActive( true ) ;
					m_GamePad.SetActive( false ) ;

					m_ModeFactor = 0 ;

					m_PointerCursor.SetActive( false ) ;
					m_IsHover = false ;
				}
				else
				{
					// GamePad モード
					m_Pointer.SetActive( false ) ;
					m_GamePad.SetActive( true ) ;

					m_PointerCursor.SetActive( false ) ;	// Tween は Absolute にすること
					m_IsHover = false ;

					m_ModeFactor = 1 ;

					m_GamePadIcon.Alpha = 1 ;
				}

				//----------------------------------

				m_Pointer.Alpha = 1 ;	// 保険
				m_GamePad.Alpha = 1 ;	// 保険

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer
					m_Pointer.SetActive( true  ) ;
					m_GamePad.SetActive( false ) ;

					m_ModeFactor = 0 ;
				}
				else
				{
					// GamePad
					m_Pointer.SetActive( false ) ;
					m_GamePad.SetActive( true  ) ;

					m_ModeFactor = 1 ;
				}
			}

			public void Visible()
			{
				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード
					if( m_PointerButton.isHover == true )
					{
						m_PointerCursor.SetActive( true ) ;
						m_IsHover = true ;
					}
					else
					{
						m_PointerCursor.SetActive( false ) ;
						m_IsHover = false ;
					}
				}
				else
				{
					// GamePad モード
					m_GamePadIcon.PlayTweenDirect( "Move" ) ;
				}
			}

			public void Update()
			{
				//----------------------------------------------------------
				// 表示の切り替わり

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer

					// カーソルの処理
					if( m_IsHover == false && m_PointerButton.isHover == true )
					{
						m_PointerCursor.SetActive( true ) ;
						m_IsHover = true ;
					}
					else
					if( m_IsHover == true  && m_PointerButton.isHover == false )
					{
						m_PointerCursor.SetActive( false ) ;
						m_IsHover = false ;
					}

					//--------------------------------
					
					if( m_ModeFactor == 1 )
					{
						// 切り替わり開始
						m_GamePadIcon.StopTween( "Move" ) ;
					}

					if( m_ModeFactor >  0 )
					{
						m_ModeFactor -= ( Time.deltaTime / 0.1f ) ;
						if( m_ModeFactor <= 0 )
						{
							// 切り替わり終了
							m_ModeFactor  = 0 ;

							m_GamePadIcon.Alpha = 1 ;
						}
					}
				}
				else
				{
					// GamePad
					if( m_ModeFactor == 0 )
					{
						// 切り替わり開始
						m_GamePadIcon.Alpha = 1 ;
					}

					if( m_ModeFactor <  1 )
					{
						m_ModeFactor += ( Time.deltaTime / 0.1f ) ;
						if( m_ModeFactor >= 1 )
						{
							// 切り替わり終了
							m_ModeFactor  = 1 ;

							m_GamePadIcon.PlayTweenDirect( "Move" ) ;
						}
					}
				}

				if( m_ModeFactor <  1 )
				{
					m_Pointer.SetActive( true ) ;
					m_Pointer.Alpha = ( 1 - m_ModeFactor ) ;
				}
				else
				{
					m_Pointer.SetActive( false ) ;
					m_Pointer.Alpha = 1 ;
				}

				if( m_ModeFactor >  0 )
				{
					m_GamePad.SetActive( true ) ;
					m_GamePad.Alpha = m_ModeFactor ;
				}
				else
				{
					m_GamePad.SetActive( false ) ;
					m_GamePad.Alpha = 1 ;
				}
			
				//----------------------------------

				if( m_ModeFactor >  0 && m_ModeFactor <  1 )
				{
					// 切り替わり最中は反応させない
					return ;
				}

				//----------------------------------------------------------
				// 入力判定

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード

					// Press
					if( m_IsHover == true )
					{
						if( m_PointerButton.isPress == true )
						{
							// 押された
						
							m_PointerCursor.SetActive( false ) ;
	
							m_OnAction( -1, SE.Click ) ;
							
							m_IsHover = false ;
						}
					}
				}
				else
				if( InputManager.InputType == InputManager.InputTypes.GamePad && InputManager.InputEnabled == true )
				{
					// GamePad モード

					if( GamePad.GetSmartButton( GamePad.B1 ) == true )
					{
						// 押された
						m_GamePadIcon.StopTween( "Move" ) ;

						m_OnAction( -1, SE.Click ) ;

						m_GamePadIcon.SetActive( false ) ;
					}
					else
					if( GamePad.GetSmartButton( GamePad.B2 ) == true )
					{
						m_GamePadIcon.StopTween( "Move" ) ;

						m_OnAction( -1, SE.Cancel ) ;

						m_GamePadIcon.SetActive( false ) ;
					}
				}
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// 選択ボタンでの処理
		/// </summary>
		public class SelectionButtonExecutor
		{
			public class ButtonClone
			{
				public	UIView		PointerButton ;
				public	UIImage		PointerCursor ;
				public	UITextMesh	PointerLabel ;

				public	UIImage		GamePadCursor ;
				public	UITextMesh	GamePadLabel ;

				public void SetPointer( UIView clone, string label )
				{
					PointerButton	= clone.FindNode<UIView>( "PointerButton" ) ;
					PointerButton.isInteraction = true ;
					PointerCursor	= clone.FindNode<UIImage>( "PointerCursor" ) ;
					PointerLabel	= clone.FindNode<UITextMesh>( "PointerLabel" ) ;
					PointerLabel.Text = label ;
				}

				public void SetGamePad( UIView clone, string label )
				{
					GamePadCursor	= clone.FindNode<UIImage>( "GamePadCursor" ) ;
					GamePadLabel	= clone.FindNode<UITextMesh>( "GamePadLabel" ) ;
					GamePadLabel.Text = label ;
				}
			}

			//----------------------------------

			private	UIView			m_Pointer = null ;
			private UIView			m_GamePad ;
			
			private float			m_ModeFactor ;
			
			private ButtonClone[]	m_Button = null ;
			
			//----------------------------------

			private	Action<int,string>			m_OnAction = null ;

			private int							m_PointerCursorIndex = -1 ;
			private int							m_GamePadCursorIndex =  0 ;


			//----------------------------------------------------------

			public void Commit( string[] selectionButtonLabels, int selectionButtonIndex, UIView selectionButton, Action<int,string> onAction )
			{
				m_OnAction = onAction ;

				//---------------------------------

				m_Pointer =	selectionButton.FindNode<UIView>( "Pointer" ) ;
				m_Pointer.IsCanvasGroup = true ;

				m_GamePad = selectionButton.FindNode<UIView>( "GamePad" ) ;
				m_GamePad.IsCanvasGroup = true ;
				
				//---------------------------------

				int i, l = selectionButtonLabels.Length ;

				m_Button = new ButtonClone[ l ] ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Button[ i ] = new ButtonClone() ;
				}

				float w, b ;
				UIView template ;
				UIView clone ;

				//---------------------------------
				// Pointer

				template = m_Pointer.FindNode<UIView>( "Template" ) ; 
				template.SetActive( false ) ;

				w = template.Width + ( 8 * 2 ) ;
				b = w * l * -0.5f ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					clone = m_Pointer.AddPrefab<UIView>( template.gameObject ) ;
					clone.SetActive( true ) ;

					clone.Px = b + ( w * i ) + ( w * 0.5f ) ;

					m_Button[ i ].SetPointer( clone, selectionButtonLabels[ i ] ) ;
				}

				//---------------------------------
				// GamePad

				template = m_GamePad.FindNode<UIView>( "Template" ) ; 
				template.SetActive( false ) ;

				w = template.Width + ( 8 * 2 ) ;
				b = w * l * -0.5f ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					clone = m_GamePad.AddPrefab<UIView>( template.gameObject ) ;
					clone.SetActive( true ) ;

					clone.Px = b + ( w * i ) + ( w * 0.5f ) ;

					m_Button[ i ].SetGamePad( clone, selectionButtonLabels[ i ] ) ;
				}

				//---------------------------------

				if( selectionButtonIndex >= 0 && selectionButtonIndex <  l )
				{
					m_GamePadCursorIndex = selectionButtonIndex ;
				}

				//---------------------------------------------------------

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード
					m_Pointer.SetActive( true ) ;
					m_GamePad.SetActive( false ) ;

					m_ModeFactor = 0 ;

					m_PointerCursorIndex = -1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						m_Button[ i ].PointerCursor.SetActive( false ) ;
					}
				}
				else
				{
					// GamePad モード
					m_Pointer.SetActive( false ) ;
					m_GamePad.SetActive( true ) ;

					m_ModeFactor = 1 ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						m_Button[ i ].PointerCursor.SetActive( false ) ;	// Tween は Absolute にすること

						m_Button[ i ].GamePadCursor.SetActive( false ) ;
						m_Button[ i ].GamePadCursor.Alpha = 1 ;
					}
				}

				//----------------------------------

				m_Pointer.Alpha = 1 ;	// 保険
				m_GamePad.Alpha = 1 ;	// 保険

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer
					m_Pointer.SetActive( true  ) ;
					m_GamePad.SetActive( false ) ;

					m_ModeFactor = 0 ;
				}
				else
				{
					// GamePad
					m_Pointer.SetActive( false ) ;
					m_GamePad.SetActive( true  ) ;

					m_ModeFactor = 1 ;
				}
			}

			/// <summary>
			/// ダイアログが完全に表示された直後に呼び出される
			/// </summary>
			public void Visible()
			{
				int i, l = m_Button.Length ;

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード

					m_PointerCursorIndex = -1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_Button[ i ].PointerButton.isHover == true )
						{
							m_Button[ i ].PointerCursor.SetActive( true ) ;
							m_PointerCursorIndex = i ;
						}
					}

					if( m_PointerCursorIndex >= 0 )
					{
						// GamePad のインデックスも更新する
						m_GamePadCursorIndex = m_PointerCursorIndex ;
					}
				}
				else
				{
					// GamePad モード
					m_Button[ m_GamePadCursorIndex ].GamePadCursor.PlayTweenDirect( "Move" ) ;
				}
			}

			/// <summary>
			/// 更新処理
			/// </summary>
			public void Update()
			{
				int i, l = m_Button.Length ;

				//----------------------------------------------------------
				// 表示の切り替わり

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer

					// カーソルの処理
					m_PointerCursorIndex = -1 ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( m_Button[ i ].PointerButton.isHover == true )
						{
							m_Button[ i ].PointerCursor.SetActive( true ) ;

							m_PointerCursorIndex = i ;
						}
						else
						{
							m_Button[ i ].PointerCursor.SetActive( false ) ;
						}
					}

					if( m_PointerCursorIndex >= 0 )
					{
						// GamePad のカーソルインデックスも更新する
						m_GamePadCursorIndex = m_PointerCursorIndex ;
					}

					//--------------------------------

					if( m_ModeFactor == 1 )
					{
						// 切り替わり開始
						m_Button[ m_GamePadCursorIndex ].GamePadCursor.StopTween( "Move" ) ;
					}

					if( m_ModeFactor >  0 )
					{
						m_ModeFactor -= ( Time.deltaTime / 0.1f ) ;
						if( m_ModeFactor <= 0 )
						{
							// 切り替わり終了
							m_ModeFactor  = 0 ;

							m_Button[ m_GamePadCursorIndex ].GamePadCursor.Alpha = 1 ;
						}
					}
				}
				else
				{
					// GamePad
					if( m_ModeFactor == 0 )
					{
						// 切り替わり開始
						for( i  = 0 ; i <  l ; i ++ )
						{
							m_Button[ i ].PointerCursor.SetActive( false ) ;	// Tween は Absolute にすること

							if( i == m_GamePadCursorIndex )
							{
								m_Button[ i ].GamePadCursor.Alpha = 1 ;
								m_Button[ i ].GamePadCursor.SetActive( true ) ;
							}
							else
							{
								m_Button[ i ].GamePadCursor.SetActive( false ) ;
								m_Button[ i ].GamePadCursor.Alpha = 1 ;
							}
						}
					}

					if( m_ModeFactor <  1 )
					{
						m_ModeFactor += ( Time.deltaTime / 0.1f ) ;
						if( m_ModeFactor >= 1 )
						{
							// 切り替わり終了
							m_ModeFactor  = 1 ;

							m_Button[ m_GamePadCursorIndex ].GamePadCursor.PlayTweenDirect( "Move" ) ;
						}
					}
				}

				if( m_ModeFactor <  1 )
				{
					m_Pointer.SetActive( true ) ;
					m_Pointer.Alpha = ( 1 - m_ModeFactor ) ;
				}
				else
				{
					m_Pointer.SetActive( false ) ;
					m_Pointer.Alpha = 1 ;
				}

				if( m_ModeFactor >  0 )
				{
					m_GamePad.SetActive( true ) ;
					m_GamePad.Alpha = m_ModeFactor ;
				}
				else
				{
					m_GamePad.SetActive( false ) ;
					m_GamePad.Alpha = 1 ;
				}
			
				//----------------------------------

				if( m_ModeFactor >  0 && m_ModeFactor <  1 )
				{
					// 切り替わり最中は反応させない
					return ;
				}

				//----------------------------------------------------------
				// 入力判定

				if( InputManager.InputType == InputManager.InputTypes.Pointer )
				{
					// Pointer モード

					// Press
					if( m_PointerCursorIndex >= 0 )
					{
						if( m_Button[ m_PointerCursorIndex ].PointerButton.isPress == true )
						{
							// 押された
						
							m_Button[ m_PointerCursorIndex ].PointerCursor.SetActive( false ) ;
	
							m_OnAction( m_PointerCursorIndex, SE.Click ) ;

							m_PointerCursorIndex = -1 ;
						}
					}
				}
				else
				if( InputManager.InputType == InputManager.InputTypes.GamePad && InputManager.InputEnabled == true )
				{
					// GamePad モード

					// カーソル移動
					Vector2 axis = GamePad.GetSmartAxis( 0 ) ;

					int gamePadCursorIndex = m_GamePadCursorIndex ;
					if( axis.x <  0 )
					{
						// 左へ
						gamePadCursorIndex = ( gamePadCursorIndex - 1 + l ) % l ;
					}
					else
					if( axis.x >  0 )
					{
						// 右へ
						gamePadCursorIndex = ( gamePadCursorIndex + 1 + l ) % l ;
					}

					bool cancel = GamePad.GetSmartButton( GamePad.B2 ) ;
					bool jump = false ;

					if( cancel == true && gamePadCursorIndex != ( l - 1 ) )
					{
						// 強制的に最後のボタンへ移動させる
						gamePadCursorIndex = l - 1 ;
						jump = true ;
					}

					// カーソルの表示位置を更新する
					if( gamePadCursorIndex != m_GamePadCursorIndex )
					{
						// カーソルの位置が変わった
						m_Button[ m_GamePadCursorIndex ].GamePadCursor.StopTween( "Move" ) ;

						m_Button[ m_GamePadCursorIndex ].GamePadCursor.SetActive( false ) ;
						m_Button[ m_GamePadCursorIndex ].GamePadCursor.Alpha = 1 ;

						m_GamePadCursorIndex = gamePadCursorIndex ;

						m_Button[ m_GamePadCursorIndex ].GamePadCursor.Alpha = 1 ;
						m_Button[ m_GamePadCursorIndex ].GamePadCursor.SetActive( true ) ;

						m_Button[ m_GamePadCursorIndex ].GamePadCursor.PlayTweenDirect( "Move" ) ;

						if( jump == false )
						{
							SE.Play( SE.Select ) ;
						}
					}

					if( jump == true )
					{
						// 最後の項目へのジャンプが発生したので決定チェックは行わない
						SE.Play( SE.Cancel ) ;

						return ;
					}

					if( GamePad.GetSmartButton( GamePad.B1 ) == true )
					{
						// 押された
						m_OnAction( m_GamePadCursorIndex, SE.Click ) ;
					}
					else
					if( cancel == true && m_GamePadCursorIndex == ( l - 1 ) )
					{
						m_OnAction( m_GamePadCursorIndex, SE.Cancel ) ;
					}
				}
			}


			//----------------------------------------------------------

		}
	}
}
