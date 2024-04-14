using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;

using DSW.UI ;


namespace DSW.Screens.LobbyClasses.UI
{
	/// <summary>
	/// アドレス入力ダイアログのクラス
	/// </summary>
	public class AddressInputDialog : SceneDialogBase
	{
		//-------------------------------------------------------------------------------------------
		// このダイアログ固有のＵＩの定義

		[SerializeField]
		protected UIInputField	m_InputField_ServerAddress ;

		//-------------------------------------------------------------------------------------------

		// 閉じるボタンが押されたかどうか
		private bool			m_IsClosed ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns></returns>
		public async UniTask<string> Open()
		{
			//------------------------------------------------------------------------------------------
			// 固有ダイアログの処理

			string endPoint = string.Empty ;

			//----------------------------------

			// アドレスを入力させる
			string endPoint_Saved = string.Empty ;

			string key = "EndPoint" ;
			if( Preference.HasKey( key ) == true )
			{
				endPoint_Saved = Preference.GetValue<string>( key ) ;
			}
			else
			{
#if UNITY_EDITOR
				endPoint_Saved = "localhost" ;
#endif
			}

			//--------------

			m_InputField_ServerAddress.Text = endPoint_Saved ;

			m_InputField_ServerAddress.SetOnValueChanged( ( string identity, UIInputField view, string text ) =>
			{
				m_CloseButton.Interactable = ( string.IsNullOrEmpty( text ) == false ) ;
			} ) ;

			m_InputField_ServerAddress.SetOnEndEdit( ( string identity, UIInputField view, string text ) =>
			{
				if( string.IsNullOrEmpty( text ) == false )
				{
					endPoint = text ;
				}
			} ) ;

			m_CloseButton.Interactable = ( string.IsNullOrEmpty( m_InputField_ServerAddress.Text  ) == false ) ;

			//------------------------------------------------------------------------------------------

			// 閉じるボタンが押されたかどうかのフラグをクリアする
			m_IsClosed = false ;

			// ダイアログを開く
			await OpenBase( null, () =>
			{
				m_IsClosed = true ;	// 閉じるボタンがされた
			} ) ;

			// 何らかのアクション(閉じるボタンなど)が実行されるのを待つ
			await WaitUntil( () => ( m_IsClosed == true ) ) ;

			//------------------------------------------------------------------------------------------

			// ダイアログが閉じられた
			endPoint = m_InputField_ServerAddress.Text ;

			// プリファレンスに記録する
			if( string.IsNullOrEmpty( endPoint ) == false )
			{
				Preference.SetValue( key, endPoint ) ;
				Preference.Save() ;
			}

			return endPoint ;
		}
	}
}
