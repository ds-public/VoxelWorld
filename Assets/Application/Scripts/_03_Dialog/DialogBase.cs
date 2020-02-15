using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using uGUIHelper ;
using InputHelper ;

namespace DBS.nDialog
{
	/// <summary>
	/// ダイアログ基底クラス
	/// </summary>
	public class DialogBase : MonoBehaviour
	{
		[SerializeField]
		protected Camera			m_BottomCamera ;

		[SerializeField]
		protected UIImage			m_Fade ;

		[SerializeField]
		protected UIImage			m_Window ;

		// ボタン
		[SerializeField]
		protected UIView			m_ClosingButton ;

		[SerializeField]
		protected UIView			m_SelectionButton ;
		
		//-----------------------------------------------------------

		/// <summary>
		/// 選択式ボタンのラベル
		/// </summary>
		public string[] SelectionButtonLabels ;

		/// <summary>
		/// 選択式ボタンの初期のインデックス
		/// </summary>
		public int		SelectionButtonIndex ;

		private ClosingButtonExecutor	m_ClosingButtonExecutor	;
		private SelectionButtonExecutor	m_SelectionButtonExecutor ;


		//-------------------------------------------------------------------------------------------

		private string				m_DialogSceneName	= "" ; 
		private Dialog.State		m_DialogStatus ;

		// フォーカス状態
		public bool Focus			= true ;
		
		//-------------------------------------------------------------------------------------------

		virtual protected string SetDialogSceneName(){ return "" ; }
		
		void Awake()
		{
			// 初期状態では非表示にしておく
			if( m_Fade != null )
			{
				m_Fade.SetActive( false ) ;
			}

			if( m_Window != null )
			{
				m_Window.SetActive( false ) ;
			}
			
			if( m_ClosingButton != null )
			{
				m_ClosingButton.SetActive( false ) ;
			}

			if( m_SelectionButton != null )
			{
				m_SelectionButton.SetActive( false ) ;
			}

			//----------------------------------------------------------

			m_DialogSceneName = SetDialogSceneName() ;

			// カメラは Awake() で設定しないと一瞬表示されてしまうので注意
			if( m_BottomCamera != null )
			{
				if( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == m_DialogSceneName )
				{
					// デバッグ
					m_BottomCamera.clearFlags = CameraClearFlags.Skybox ;
				}
				else
				{
					// リリース
					m_BottomCamera.clearFlags = CameraClearFlags.Nothing ;
				}
			}

			//----------------------------------------------------------

			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;
		}

		// 呼ばれる順番は、Awake -> Start -> Setup

		IEnumerator Start()
		{
			// ApplicationManager の準備が整うのを待つ
			if( ApplicationManager.IsInitialized == false )
			{
				yield return new WaitWhile( () => ApplicationManager.IsInitialized == false ) ;
			}

			//----------------------------------------------------------

#if UNITY_EDITOR
			if( Scene.Name == m_DialogSceneName )
			{
				// デバッグ
				yield return StartCoroutine( RunDebug() ) ;
			}
#endif
		}

		virtual protected IEnumerator RunDebug(){ yield return null ; }

		//-----------------------------------------------------------

		private Action<int>		m_OnButtonAction ;
		
		// ボタンを使用する
		protected void UseStandardButton( string[] selectionButtonLabels = null, int selectionButtonIndex = 0, Action<int> onButtonAction = null )
		{
			// ボタン情報を保存する
			SelectionButtonLabels	= selectionButtonLabels ;
			SelectionButtonIndex	= selectionButtonIndex ;
			m_OnButtonAction		= onButtonAction ;

			if( m_ClosingButton != null && SelectionButtonLabels.IsNullOrEmpty() == true )
			{
				// 閉じるボタンのみ

				m_ClosingButton.SetActive( true ) ;

				if( m_SelectionButton != null )
				{
					m_SelectionButton.SetActive( false ) ;
				}

				//---------------------------------

				m_ClosingButtonExecutor = new ClosingButtonExecutor() ;
				m_ClosingButtonExecutor.Commit( m_ClosingButton, OnStandardButtonAction ) ;
			}
			else
			if( m_SelectionButton != null && SelectionButtonLabels.IsNullOrEmpty() == false )
			{
				// 選択ボタン
				
				m_SelectionButton.SetActive( true ) ;

				if( m_ClosingButton != null )
				{
					m_ClosingButton.SetActive( false ) ;
				}

				//---------------------------------

				m_SelectionButtonExecutor = new SelectionButtonExecutor() ;
				m_SelectionButtonExecutor.Commit( SelectionButtonLabels, SelectionButtonIndex, m_SelectionButton, OnStandardButtonAction ) ;
			}
		}
		
		// ボタンが押された際に呼び出されるローカルのコールバックメソッド
		private void OnStandardButtonAction( int index, string se )
		{
			if( Focus == false )
			{
				// フォーカスを失っている場合はボタンを押しても無効
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

			// コールバック呼び出し
			m_OnButtonAction?.Invoke( index ) ;
		}

		//-------------------------------------------------------------------------------------------

		protected Dialog.State SetDialogStatus()
		{
			return m_DialogStatus = new Dialog.State() ;
		}

		protected Dialog.State GetDialogStatus()
		{
			return m_DialogStatus ;
		}

		protected UIImage GetFade()
		{
			return m_Fade ;
		}

		protected UIImage GetWindow()
		{
			return m_Window ;
		}
		
		/// <summary>
		/// ダイアログを開く(ダイアログが閉じられるまでコルーチンは終了しない)
		/// </summary>
		/// <returns></returns>
		protected Dialog.State OpenBase( Action tFinishAction = null, IEnumerator tWaitingAction = null )
		{
			gameObject.SetActive( true ) ;

			if( m_Fade != null )
			{
				m_Fade.SetActive( true ) ;
				m_Fade.PlayTween( "FadeIn" ) ;
			}

			if( m_Window != null )
			{
				m_Window.SetActive( true ) ;
				m_Window.PlayTween( "FadeIn" ) ;
			}

			StartCoroutine( OpenBase_Private( tFinishAction, tWaitingAction ) ) ;

			return m_DialogStatus = new Dialog.State() ;
		}

		private IEnumerator OpenBase_Private( Action tFinishAction, IEnumerator tWaitingAction )
		{
			if( m_Fade != null || m_Window != null )
			{
				while
				(
					( m_Fade != null && m_Fade.IsAnyTweenPlaying == true ) ||
					( m_Window != null && m_Window.IsAnyTweenPlaying == true )
				)
				{
					yield return null ;
				}
			}

			if( tWaitingAction != null )
			{
				yield return StartCoroutine( tWaitingAction ) ;
			}

			//----------------------------------

			// ダイアログが開ききった

			// 完了ボタン
			if( m_ClosingButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須
				m_ClosingButtonExecutor.Visible() ;
			}

			// 選択ボタン
			if( m_SelectionButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須

				m_SelectionButtonExecutor.Visible() ;
			}

			//----------------------------------

			tFinishAction?.Invoke() ;

			//-----------------------------------

			// フォーカスを得る
			Focus = true ;
		}
		
		/// <summary>
		/// ダイアログを開く(ダイアログが開いた時点でコルーチンが終了するため後で明示的に閉じなければならない)
		/// </summary>
		/// <returns></returns>
		protected Dialog.State ShowBase( Action tFinishAction = null, IEnumerator tWaitingAction = null )
		{
			if( m_Fade != null )
			{
				m_Fade.SetActive( true ) ;
				m_Fade.PlayTween( "FadeIn" ) ;
			}
			
			if( m_Window != null )
			{
				m_Window.SetActive( true ) ;
				m_Window.PlayTween( "FadeIn" ) ;
			}

			Dialog.State tDialogState = new Dialog.State() ;

			StartCoroutine( ShowBase_Private( tFinishAction, tWaitingAction, tDialogState ) ) ;

			return tDialogState ;
		}

		private IEnumerator ShowBase_Private( Action tFinishAction, IEnumerator tWaitingAction, Dialog.State tDialogState )
		{
			if( m_Fade != null || m_Window != null )
			{
				while
				(
					( m_Fade != null && m_Fade.IsAnyTweenPlaying == true ) ||
					( m_Window != null && m_Window.IsAnyTweenPlaying == true )
				)
				{
					yield return null ;
				}
			}

			if( tWaitingAction != null )
			{
				yield return StartCoroutine( tWaitingAction ) ;
			}

			//----------------------------------

			// ダイアログが開ききった

			// 完了ボタン
			if( m_ClosingButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須
				m_ClosingButtonExecutor.Visible() ;
			}

			// 選択ボタン
			if( m_SelectionButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須
				m_SelectionButtonExecutor.Visible() ;
			}

			//----------------------------------

			tFinishAction?.Invoke() ;

			//-----------------------------------

			// フォーカスを得る
			Focus = true ;

			//-----------------------------------

			if( tDialogState != null )
			{
				tDialogState.IsDone = true ;
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

			// 終了ボタン
			if( m_ClosingButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須
				m_ClosingButtonExecutor.Update() ;
			}

			// 選択ボタン
			if( m_SelectionButtonExecutor != null )
			{
				// Update の方が Commit より早いので null チェック必須
				m_SelectionButtonExecutor.Update() ;
			}

			//----------------------------------------------------------

			// 毎フレーム更新処理を呼び出す
			OnUpdate() ;
		}

		virtual protected void OnUpdate(){}

		// 毎フレームの最後に呼ばれる
		void LateUpdate()
		{
			OnLateUpdate() ;
		}

		virtual protected void OnLateUpdate(){ }
		
		/// <summary>
		/// ダイアログを閉じる
		/// </summary>
		/// <returns>The base.</returns>
		/// <param name="tFinishAction">T finish action.</param>
		/// <param name="tWaitingAction">T waiting action.</param>
		protected IEnumerator CloseBase( Action tFinishAction = null, IEnumerator tWaitingAction = null )
		{
			if( m_Fade != null )
			{
				m_Fade.PlayTween( "FadeOut" ) ;
			}
			if( m_Window != null )
			{
				m_Window.PlayTween( "FadeOut" ) ;
			}

			if( m_Fade != null || m_Window != null )
			{
				while
				(
					( m_Fade != null && m_Fade.IsAnyTweenPlaying == true ) ||
					( m_Window != null && m_Window.IsAnyTweenPlaying == true )
				)
				{
					yield return null ;
				}
			}

			if( tWaitingAction != null )
			{
				yield return StartCoroutine( tWaitingAction ) ;
			}

			tFinishAction?.Invoke() ;

			if( m_DialogStatus != null )
			{
				m_DialogStatus.IsDone = true ;
				m_DialogStatus  = null ;	// ダイアログ内のインスタンス参照は消去する
			}
			
			if( gameObject.scene.name == m_DialogSceneName )
			{
				// 非常駐型
				Scene.Remove( m_DialogSceneName ) ;
			}
			else
			{
				// 常駐型
				gameObject.SetActive( false ) ;
			}
		}

		/// <summary>
		/// 結果を１つ設定する
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		protected bool SetResult( object result )
		{
			if( m_DialogStatus == null )
			{
				return false ;
			}

			m_DialogStatus.Result = result ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

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
					
					if( m_ModeFactor >= 1 )
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
					if( m_ModeFactor <= 0 )
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

			private	UIView			m_Pointer ;
			private UIView			m_GamePad ;
			
			private float			m_ModeFactor ;
			
			private ButtonClone[]	m_Button ;
			
			//----------------------------------

			private	Action<int,string>		m_OnAction ;

			private int						m_PointerCursorIndex = -1 ;
			private int						m_GamePadCursorIndex ;


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

					if( m_ModeFactor >= 1 )
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
					if( m_ModeFactor <= 0 )
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
		}
	}
}

