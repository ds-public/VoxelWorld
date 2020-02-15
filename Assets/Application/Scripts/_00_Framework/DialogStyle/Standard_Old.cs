/*
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DBS.DialogStyle
{
	/// <summary>
	/// スタンダードタイプのダイアログ
	/// </summary>
	public class Standard_Old : DialogStyleBase
	{
		// タイトル
		[SerializeField]
		private UIRichText	m_Title = null ;

		/// <summary>
		/// タイトル
		/// </summary>
		public UIRichText title
		{
			get
			{
				return m_Title ;
			}
		}

		/// <summary>
		/// タイトル部分の縦幅
		/// </summary>
		public float spaceTitle = 72 ;


		/// <summary>
		/// タイトル下のスペース
		/// </summary>
		public float spaceUnderTitle = 8 ;

		//---------------

		// メッセージ
		[SerializeField]
		private UIRichText	m_Message = null ;

		/// <summary>
		/// メッセージ
		/// </summary>
		public UIRichText message
		{
			get
			{
				return m_Message ;
			}
		}

		/// <summary>
		/// メッセージ下のスペース
		/// </summary>
		public float spaceUnderMessage = 20 ;

		//---------------

		// ボタンのテンプレート
		[SerializeField]
		private UIButton	m_TemplateButton = null ;

		// 複製ボタン
		private UIButton[]	m_CloneButton = null ;

		// ボタンの数とラベル
		private string[]	m_ButtonLabel = null ;

		/// <summary>
		/// ボタンラベル
		/// </summary>
		public string[] buttonLabel
		{
			get
			{
				return m_ButtonLabel ;
			}
			set
			{
				m_ButtonLabel = value ;
			}
		}

		/// <summary>
		/// ボタン間のスペース
		/// </summary>
		public float spaceBetweenButton = 12 ;

		//-----------------------------------------------------------
		
		/// <summary>
		/// コールバック用のメソッド
		/// </summary>
		public Action<Standard_Old,string,string> callback = null ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる
		/// </summary>
		public bool autoClose = false ;

		/// <summary>
		/// ボタンを押したら自動的にウィンドウを閉じる際に呼ぶコールバック用のメソッド
		/// </summary>
		public Action<string> autoCloseCallback = null ;
		
		/// <summary>
		/// ダイアログの表示状態
		/// </summary>
		public Dialog.State	state = null ;

		/// <summary>
		/// 外側にも反応するかどうか(ただしボタンが１つの場合のみ)
		/// </summary>
		public bool outsideEnabled = true ;
		
		// 自動的にダイアログを閉じるまでの時間
		public float displayTime = 0 ;

		//-------------------------------------------------------------------------------------------

		private float	m_DisplayTime = 0 ;

		private bool	m_Closing = false ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各種準備を行う
		/// </summary>
//		override public void Prepare( int tPriority )
//		{
//			base.Prepare( tPriority ) ;
//		}

		/// <summary>
		/// ＵＩの状態を完成させる
		/// </summary>
		override public void Commit()
		{
			if( m_Dirty == false )
			{
				return ;	// 既に実行済み
			}

			float tH =   0 ;	// 縦幅
			float tW = 320 ;	// 横幅

			tH = tH + padding.top ;

			if( m_Title != null && m_Title.length >  0 )
			{
				m_Window.SetSpriteInAtlas( "Window", false ) ;

//				m_Title._y = - tH - ( m_Title._th * ( 1.0f - m_Title.pivot.y ) ) ;
//
				if( m_Title._tw >  tW )
				{
					tW = m_Title._tw ;
				}

//				tH = tH + m_Title._th ;
				tH = tH + spaceTitle ;

				if( ( m_Message != null && m_Message.length >  0 || ( m_ButtonLabel != null && m_ButtonLabel.Length >  0 ) ) )
				{
					tH = tH + spaceUnderTitle ;
				}
			}
			else
			{
				m_Window.SetSpriteInAtlas( "Window_NoTitle", false ) ;

				m_Title.SetActive( false ) ;
			}

			if( m_Message != null && m_Message.length >  0 )
			{
				m_Message._y = - tH - ( m_Message._th * ( 1.0f - m_Message.pivot.y ) ) ;
				
				if( m_Message._tw >  tW )
				{
					tW = m_Message._tw ;
				}

				tH = tH + m_Message._th ;

				if( m_ButtonLabel != null && m_ButtonLabel.Length >  0 )
				{
					tH = tH + spaceUnderMessage ;
				}
			}
			
			if( m_ButtonLabel != null && m_ButtonLabel.Length >  0 )
			{
				int i, l = m_ButtonLabel.Length ;

				m_CloneButton = new UIButton[ l ] ;

				m_TemplateButton.SetActive( false ) ;


				float tBW = 0 ;
				float tBH = 0 ;

				// 各ボタンの位置をサイズを決定する

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButton[ i ] = m_Window.AddPrefab<UIButton>( m_TemplateButton.gameObject ) ;

                    // ボタンの数によってボタンの画像を変更する
                    m_CloneButton[ i ].SetSpriteInAtlas( i == 0 && l <= 2 ? "CancelButton" : "DecideButton", false ) ;
					
					string[] tText = m_ButtonLabel[ i ].Split( ':' ) ;
					if( tText.Length >  0 )
					{
						m_CloneButton[ i ].name = tText[ 0 ] ;
						m_CloneButton[ i ].identity = i.ToString() ;
						m_CloneButton[ i ].label.text = tText[ 0 ] ;
					}
					if( tText.Length >  1 )
					{
						m_CloneButton[ i ].identity = tText[ 0 ] ;
						m_CloneButton[ i ].label.text = tText[ 1 ] ;
					}
					m_CloneButton[ i ].SetActive( true ) ;
					m_CloneButton[ i ].SetOnButtonClick( OnButtonClick ) ;

					//--------------------------------------------------------

					ResizeButton( m_CloneButton[ i ] ) ;

					tBW = tBW + m_CloneButton[ i ]._w ;
					if( i <  ( l - 1 ) )
					{
						tBW = tBW + spaceBetweenButton ;
					}

					if( m_CloneButton[ i ]._h >  tBH )
					{
						tBH  = m_CloneButton[ i ]._h ;
					}
				}

				float bx = - ( tBW * 0.5f ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_CloneButton[ i ]._y = - tH - ( tBH * 0.5f ) ;

					m_CloneButton[ i ]._x = bx + ( m_CloneButton[ i ]._w * 0.5f ) ;

					bx = bx + m_CloneButton[ i ]._w + spaceBetweenButton ;
				}

				if( tBW >  tW )
				{
					tW  = tBW ;
				}

				tH = tH + tBH ;

				// 特殊キャンセル処理
				if( m_ButtonLabel.Length == 1 && outsideEnabled == true )
				{
					if( m_Fade != null )
					{
						m_Fade.isEventTrigger = true ;
						m_Fade.isInteraction = true ;
						m_Fade.SetOnClick( OnOutside ) ;	// 外側タップの反応
					}
				}
			}
			else
			{
				m_TemplateButton.SetActive( false ) ;
			}

			tW = tW + padding.horizontal ;
			tH = tH + padding.bottom ;

			if( tW <  m_Window._w )
			{
				tW  = m_Window._w ;
			}

			m_Window.SetSize( tW, tH ) ;
			
			//----------------------------------------------------------

			// 表示時間
			m_DisplayTime = displayTime ;
			m_Closing = false ;

			//----------------------------------------------------------

			m_Dirty = false ;	// 完了
		}

		// ボタンが押された際に呼び出されるローカルのコールバックメソッド
		private void OnButtonClick( string tIdentity, UIButton tButton )
		{
			if( m_Closing == true )
			{
				return ;
			}

			if( callback != null )
			{
				callback( this, "clicked", tIdentity ) ;
			}

			if( autoClose == true )
			{
				m_Closing = true ;
				Close( tIdentity ) ;
			}
		}

		// 外側に触れた時の対応
		private void OnOutside( string tIdentity, UIView tView )
		{
			if( m_CloneButton != null && m_CloneButton.Length == 1 )
			{
				OnButtonClick( m_CloneButton[ 0 ].identity, m_CloneButton[ 0 ] ) ;
			}
		}

		void Update()
		{
			if( m_DisplayTime >  0 && m_Closing == false )
			{
				m_DisplayTime -= Time.fixedDeltaTime ;
				if( m_DisplayTime <= 0 )
				{
					// ダイアログを閉じる
					m_Closing = true ;
					Close( "" ) ;
				}
			}
		}

		// ダイアログをフォードアウト効果付きで非表示にして最後にクローズ時のデリゲードを呼び出す
		override protected IEnumerator FadeOut()
		{
			yield return StartCoroutine( base.FadeOut() ) ;

			// 最後にコールバックを呼び出して複製された自身も破棄する
			if( callback != null )
			{
				callback( this, "closed", m_Result ) ;	// 最後に閉じられた事を通知する
			}

			if( autoClose == true && autoCloseCallback != null )
			{
				autoCloseCallback( m_Result ) ;
			}

			if( state != null )
			{
				state.index = m_Result ;
				state.isDone = true ;
			}

			Destroy( gameObject ) ;
		}


		public void SetTextColor( Color c )
		{
			// 全てのテキストのデフォルトカラーを設定する
			if( m_Title != null )
			{
				m_Title.color = c ;
			}

			if( m_Message != null )
			{
				m_Message.color = c ;
			}

			if( m_CloneButton != null && m_CloneButton.Length >  0 )
			{
				int i, l = m_CloneButton.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_CloneButton[ i ].label != null )
					{
						m_CloneButton[ i ].label.color = c ;
					}
				}
			}
		}
	}
}
*/
