using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using MathHelper ;

namespace DSW.Screens.LobbyClasses.UI
{
	public class ModeSettingPanel : ExMonoBehaviour
	{
		private UIView		m_View = null ;
		public  UIView		  View
		{
			get
			{
				if( m_View == null )
				{
					m_View  = GetComponent<UIView>() ;
				}
				return m_View ;
			}
		}

		//-----------------------------------------------------------

		[Header( "ゲームモードの選択タブ" )]

		[SerializeField]
		protected UIButtonGroup					m_TabButtonBase ;

		[Serializable]
		public class TabStructure
		{
			public UIButton						Button ;
		}

		[SerializeField]
		protected TabStructure[]				m_Tabs = new TabStructure[ 2 ] ;

		[SerializeField]
		protected Material						m_TabLabelStyle_Normal ;

		[SerializeField]
		protected Material						m_TabLabelStyle_Active ;


		[Header( "プレイヤー設定" )]

		[SerializeField]
		protected UIInputField					m_PlayerName_InputField ;

		[Serializable]
		public class ColorSlot
		{
			public UIImage		Sample ;
			public UIImage		Cursor ;
		}

		[SerializeField]
		protected ColorSlot[]	m_ColorSlots = new ColorSlot[ 4 ] ;

		[Header( "サーバー設定" )]

		[SerializeField]
		protected UITextMesh					m_ServerAddress_Label ;

		[SerializeField]
		protected UIInputField					m_ServerAddress_InputField ;

		[SerializeField]
		protected UIButton						m_ServerSearchButton ;

		[SerializeField]
		protected UITextMesh					m_ServerPortNumber_Label ;

		[SerializeField]
		protected UIInputField					m_ServerPortNumber_InputField ;


		[Header( "アクション" )]

		[SerializeField]
		protected UIButton						m_StartButton ;

		//-------------------------------------------------------------------------------------------

		private Lobby	m_Owner ;


		private int		m_TabIndex				= -1 ;

		private int		m_SystemServerPortNumber ;


		private string	m_PlayerName			= string.Empty ;
		private byte	m_ColorType				=  0 ;
		private string	m_ServerAddress			= string.Empty ;
		private int		m_ServerPortNumber		= -1 ;

		private string	m_AlertMessage			= string.Empty ;

		private bool	m_IsStart				= false ;

		//-------------------------------------------------------------------------------------------

		private static	Color32	m_PositiveColor	= new ( 255, 255, 255, 255 ) ;
		private static	Color32	m_NegativeColor	= new ( 143, 143, 143, 255 ) ;


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 準備を行う
		/// </summary>
		/// <returns></returns>
		public void Prepare( Lobby owner )
		{
			// オーナーを保存
			m_Owner = owner ;

			//----------------------------------------------------------

			// タブ操作
			m_TabButtonBase.SetOnValueChanged( ( string identity, UIButton button, bool option ) =>
			{
				if( option == true )
				{
					SE.Play( SE.Selection ) ;
				}

				// タブ内のボタンが押されたら呼び出される
				UpdateTabButtons( button ) ;
			} ) ;

			//----------------------------------------------------------

			int i, l ;
			string key ;


			//----------------------------------------------------------
			// プレイヤー情報

			//--------------
			// プレイモード

			key = "PlayMode" ;
			PlayerData.PlayModes playMode = Preference.GetValue<PlayerData.PlayModes>( key, PlayerData.PlayModes.Single ) ;

			//--------------
			// プレイヤー名

			// デフォルトのプレイヤー名を設定する
			string defaultPlayerName ;
			defaultPlayerName = "ステーブ" ;
			if( Random_XorShift.Get(  0, 99 ) <  25 )
			{
				defaultPlayerName = "アリックス" ;
			}

			key = "PlayerName" ;
			if( Preference.HasKey( key ) == true )
			{
				m_PlayerName = Preference.GetValue<string>( key ) ;
			}
			else
			{
				m_PlayerName = defaultPlayerName ;
			}

			m_PlayerName_InputField.Text = m_PlayerName ;
			m_PlayerName_InputField.SetOnValueChanged( ( string identity, UIInputField inputField, string text ) =>
			{
				m_PlayerName = text ;
				UpdateStartButton() ;
			} ) ;
			m_PlayerName_InputField.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				m_PlayerName = text ;
				UpdateStartButton() ;
			} ) ;

			//--------------
			// 色選択

			key = "ColorType" ;
			if( Preference.HasKey( key ) == true )
			{
				m_ColorType = Preference.GetValue<byte>( key ) ;
			}

			l = m_ColorSlots.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var slot = m_ColorSlots[ i ] ;

				slot.Sample.Color = WorldSettings.PlayerActorColors[ i ] ;

				slot.Sample.IsInteraction = true ;
				slot.Sample.RaycastTarget = true ;
				slot.Sample.SetOnClick( OnColorSeleced ) ;
			}

			void OnColorSeleced( string identity, UIView view )
			{
				SE.Play( SE.Selection ) ;

				l = m_ColorSlots.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					var slot = m_ColorSlots[ i ] ;
					if( view == slot.Sample )
					{
						m_ColorType = ( byte )i ;
						break ;
					}
				}
				UpdateColorSlots() ;
			}

			UpdateColorSlots() ;

			//----------------------------------------------------------
			// サーバー情報

			// 注意:UIInputField.SetOnValueChanged は、.Text に設定しても呼ばれる。

			//--------------
			// サーバーアドレス

			key = "ServerAddress" ;
			if( Preference.HasKey( key ) == true )
			{
				m_ServerAddress = Preference.GetValue<string>( key ) ;
			}

			m_ServerAddress_InputField.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				m_ServerAddress = text ;

//				UpdateServerAddress() ;
				UpdateStartButton() ;
			} ) ;

			UpdateServerAddress() ;

			//--------------
			// サーバーポート番号
			var settings = ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				m_SystemServerPortNumber = settings.ServerPortNumber ;
			}
			m_ServerPortNumber = m_SystemServerPortNumber ;

			key = "ServerPortNumber" ;
			if( Preference.HasKey( key ) == true )
			{
				m_ServerPortNumber = Preference.GetValue<int>( key ) ;
			}
			
			m_ServerPortNumber_InputField.SetOnEndEdit( ( string identity, UIInputField inputField, string text ) =>
			{
				if( int.TryParse( text, out int serverPortNumber ) == false )
				{
					serverPortNumber = -1 ;
				}
				if( serverPortNumber >= 65536 )
				{
					serverPortNumber = -1 ;
				}
				m_ServerPortNumber = serverPortNumber ;

				UpdateServerPortNumber() ;
				UpdateStartButton() ;
			} ) ;

			UpdateServerPortNumber() ;

			//----------------------------------------------------------
			// サーバー検索へ

			m_ServerSearchButton.SetOnSimpleClick( () =>
			{
				OpenAddressSelectionDialog( m_ServerPortNumber ).Forget() ;
			} ) ;

			//----------------------------------------------------------

			int tabIndex = 0 ;
			if( playMode == PlayerData.PlayModes.Single )
			{
				tabIndex = 0 ;
			}
			else
			if( playMode == PlayerData.PlayModes.Multi )
			{
				tabIndex = 1 ;
			}

			// ゲームモードの現在のボタンを選択状態にする
			m_TabButtonBase.SetState( m_Tabs[ tabIndex ].Button, true, false ) ;

			//----------------------------------------------------------
			// ゲーム開始ボタン

			m_IsStart = false ;

			m_StartButton.SetOnSimpleClick( () =>
			{
				SE.Play( SE.Decision ) ;

				m_IsStart = true ;
			} ) ;

			m_StartButton.EnableFakeInvalidation( () =>
			{
				Toast.Show( m_AlertMessage ) ;
			} ) ;

			UpdateStartButton() ;
		}

		// タブ切り替えを表示に反映する
		private void UpdateTabButtons( UIButton selectedButton )
		{
			int i, l = m_Tabs.Length ;
			int tabIndex  = -1 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Tabs[ i ] != null && m_Tabs[ i ].Button != null )
				{
					var tab = m_Tabs[ i ] ;

					if( tab.Button == selectedButton )
					{
						// 選択済対象
						if( m_TabLabelStyle_Active != null )
						{
							tab.Button.SetLabelMaterial( m_TabLabelStyle_Active ) ;
						}
						else
						{
							// 保険
							tab.Button.SetLabelColor( 0xFFFFFFFF ) ;
						}

						tabIndex = i ;
					}
					else
					{
						// 非選択対象

						if( m_TabLabelStyle_Normal != null )
						{
							tab.Button.SetLabelMaterial( m_TabLabelStyle_Normal ) ;
						}
						else
						{
							// 保険
							tab.Button.SetLabelColor( 0xFF5A2F0A ) ;
						}
					}
				}
			}

			if( m_TabIndex != tabIndex )
			{
				m_TabIndex  = tabIndex ;

				Refresh() ;
			}
		}

		// 表示を更新する
		private void Refresh()
		{
			UpdateServerAddress() ;
			UpdateServerPortNumber() ;
			UpdateStartButton() ;
		}

		// 色選択ＵＩの表示を更新する
		private void UpdateColorSlots()
		{
			int i, l = m_ColorSlots.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				var slot = m_ColorSlots[ i ] ;

				slot.Cursor.SetActive( i == m_ColorType ) ;
			}
		}

		// サーバーアドレス設定ＵＩの表示を更新する
		private void UpdateServerAddress()
		{
			if( m_TabIndex == 0 )
			{
				// シングル
				m_ServerAddress_Label.Color = m_NegativeColor ;
				m_ServerAddress_InputField.Text = "localhost" ;
				m_ServerAddress_InputField.Interactable = false ;
				m_ServerSearchButton.Interactable = false ;
			}
			else
			if( m_TabIndex == 1 )
			{
				// マルチ
				m_ServerAddress_Label.Color = m_PositiveColor ;
				m_ServerAddress_InputField.Text = m_ServerAddress ;
				m_ServerAddress_InputField.Interactable = true ;
				m_ServerSearchButton.Interactable = true ;
			}
		}

		// サーバーポート番号設定ＵＩの表示を更新する
		private void UpdateServerPortNumber()
		{
//			if( m_TabIndex == 0 )
//			{
//				// シングル
//				m_ServerPortNumber_Label.Color = m_NegativeColor ;
//				m_ServerPortNumber_InputField.Text = m_SystemServerPortNumber.ToString() ;
//				m_ServerPortNumber_InputField.Interactable = false ;
//			}
//			else
//			if( m_TabIndex == 1 )
//			{
				// マルチ
				m_ServerPortNumber_Label.Color = m_PositiveColor ;
				if( m_ServerPortNumber >= 0 )
				{
					m_ServerPortNumber_InputField.Text = m_ServerPortNumber.ToString() ;
				}
				else
				{
					m_ServerPortNumber_InputField.Text = string.Empty ;
				}
				m_ServerPortNumber_InputField.Interactable = true ;
//			}
		}


		// ゲーム開始ボタンの表示を更新する
		private void UpdateStartButton()
		{
			bool isReady = true ;

			if( isReady == true && string.IsNullOrEmpty( m_PlayerName ) == true )
			{
				isReady = false ;
				m_AlertMessage = "プレイヤー名が入力されていません" ;
			}

			if( m_TabIndex == 1 )
			{
				if( isReady == true && string.IsNullOrEmpty( m_ServerAddress ) == true )
				{
					isReady = false ;
					m_AlertMessage = "サーバーアドレスが入力されていません" ;
				}
			}

			if( isReady == true && m_ServerPortNumber <  0 )
			{
				isReady = false ;
				m_AlertMessage = "サーバーポート番号が入力されていません" ;
			}

			m_StartButton.Interactable = isReady ;
		}

		// サーバー検索ダイアログを開く
		private async UniTask OpenAddressSelectionDialog( int serverPort )
		{
			string endPoint = await m_Owner.DialogController.AddressSelectionDialog.Open() ;
			if( string.IsNullOrEmpty( endPoint ) == false )
			{
				int i = endPoint.IndexOf( ':' ) ;
				if( i >= 0 )
				{
					// アドレスとポート番号を分離する
					m_ServerAddress = endPoint[ ..i ] ;
					m_ServerAddress_InputField.Text = m_ServerAddress ;

					i ++ ;
					string serverPortNumberName = endPoint[ i.. ] ;
					if( int.TryParse( serverPortNumberName, out int serverPortNumber ) == true )
					{
						m_ServerPortNumber = serverPortNumber ;
						m_ServerPortNumber_InputField.Text = serverPortNumber.ToString() ;
					}
				}
				else
				{
					// アドレスを更新する
					m_ServerAddress = endPoint ;
					m_ServerAddress_InputField.Text = m_ServerAddress ;
				}

				m_StartButton.Interactable = true ;
			}
		}


		//-----------------------------------------------------------

		/// <summary>
		/// フェードイン
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeIn()
		{
			if( View.ActiveSelf == true )
			{
				return ;
			}

			await When( View.PlayTween( "FadeIn" ) ) ;
		}

		/// <summary>
		///  フェードアウト
		/// </summary>
		/// <returns></returns>
		public async UniTask FadeOut()
		{
			if( View.ActiveSelf == false )
			{
				return ;
			}

			await When( View.PlayTweenAndHide( "FadeOut" ) ) ;
		}

		/// <summary>
		/// 開始ボタンが押されるのを待つ
		/// </summary>
		/// <returns></returns>
		public async UniTask<( PlayerData.PlayModes, string, byte, string, int )> WaitFor()
		{
			await WaitUntil( () => m_IsStart == true ) ;

			//----------------------------------

			PlayerData.PlayModes playMode	= PlayerData.PlayModes.Single ;

			if( m_TabIndex == 0 )
			{
				// シングル
				playMode			= PlayerData.PlayModes.Single ;
			}
			else
			if( m_TabIndex == 1 )
			{
				// マルチ
				playMode			= PlayerData.PlayModes.Multi ;
			}

			//----------------------------------
			// 設定値を保存する

			string key ;
			bool isSet = false ;

			key = "PlayMode" ;
			Preference.SetValue( key, playMode ) ;

			if( string.IsNullOrEmpty( m_PlayerName ) == false )
			{
				key = "PlayerName" ;
				Preference.SetValue( key, m_PlayerName ) ;
				isSet = true ;
			}

			if( m_ColorType >= 0 )
			{
				key = "ColorType" ;
				Preference.SetValue( key, m_ColorType ) ;
				isSet = true ;
			}

			if( string.IsNullOrEmpty( m_ServerAddress ) == false )
			{
				key = "ServerAddress" ;
				Preference.SetValue( key, m_ServerAddress ) ;
				isSet = true ;
			}

			if( m_ServerPortNumber >= 0 )
			{
				key = "ServerPortNumber" ;
				Preference.SetValue( key, m_ServerPortNumber ) ;
				isSet = true ;
			}

			//--------------

			if( isSet == true )
			{
				Preference.Save() ;
			}

			//----------------------------------------------------------
			// ワールドに受け渡す

			string	serverAddress			= string.Empty ;
			int		serverPortNumber		= 0 ;

			if( playMode == PlayerData.PlayModes.Single )
			{
				// シングル
				serverAddress		= "localhost" ;
				serverPortNumber	= m_ServerPortNumber ;
			}
			else
			if( playMode == PlayerData.PlayModes.Multi )
			{
				// マルチ
				serverAddress		= m_ServerAddress ;
				serverPortNumber	= m_ServerPortNumber ;
			}

			return ( playMode, m_PlayerName, m_ColorType, serverAddress, serverPortNumber ) ;
		}
	}
}

