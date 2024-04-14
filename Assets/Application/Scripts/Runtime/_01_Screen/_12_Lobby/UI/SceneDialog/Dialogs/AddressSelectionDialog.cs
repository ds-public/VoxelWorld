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
	/// アドレス選択ダイアログのクラス
	/// </summary>
	public class AddressSelectionDialog : SceneDialogBase
	{
		//-------------------------------------------------------------------------------------------
		// このダイアログ固有のＵＩの定義

		[SerializeField]
		protected UIListView	m_Selector ;

		//-------------------------------------------------------------------------------------------

		// 閉じるボタンが押されたかどうか
		private bool			m_IsClosed ;

		// サーバー情報
		private List<ServerDetactor.ServerEntity>	m_ServerEntities ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <returns></returns>
		public async UniTask<string> Open()
		{
			//------------------------------------------------------------------------------------------
			// 固有ダイアログの処理

			// サーバー捜査開始
			ServerDetactor.ServerDitectorPort = 55555 ;
			ServerDetactor.Resume() ;

			// サーバー情報が更新されたら呼ばれるコールバックを登録する
			ServerDetactor.AddCallback( OnUpdateServerEntities ) ;

			//----------------------------------------------------------

			string endPoint = string.Empty ;

			//----------------------------------

			// サーバー情報のリスト更新コールバックを設定する
			m_Selector.SetOnItemUpdated<AddressSelectionDialog_ListViewItem>( ( string identity, UIListView view, int index, Component component ) =>
			{
				if( component != null )
				{
					var viewItem = component as AddressSelectionDialog_ListViewItem ;

					var serverEntity = m_ServerEntities[ index ] ;

					string serverName		= serverEntity.Name ;
					string serverAddress	= serverEntity.IpAddress + " : " + serverEntity.Port.ToString() ;

					// リストビューアイテムのスタイルを設定する
					viewItem.SetStyle( serverName, serverAddress, index, ( int selectedIndex ) =>
					{
						endPoint = m_ServerEntities[ selectedIndex ].IpAddress + ":" + m_ServerEntities[ selectedIndex ].Port.ToString() ;
					} ) ;
				}

				return 0 ;
			} ) ;
			m_Selector.ItemCount = 0 ;

			//------------------------------------------------------------------------------------------

			// 閉じるボタンが押されたかどうかのフラグをクリアする
			m_IsClosed = false ;

			// ダイアログを開く
			await OpenBase( null, () =>
			{
				m_IsClosed = true ;	// 閉じるボタンがされた
			} ) ;

			// 何らかのアクション(閉じるボタンなど)が実行されるのを待つ
			await WaitUntil( () => ( m_IsClosed == true || string.IsNullOrEmpty( endPoint ) == false ) ) ;

			if( m_IsClosed == false )
			{
				// ダイアログを閉じる
				await Close() ;
			}

			//------------------------------------------------------------------------------------------

			// サーバー情報が更新されたら呼ばれるコールバックを解除する
			ServerDetactor.RemoveCallback( OnUpdateServerEntities ) ;

			// サーバー捜査終了
			ServerDetactor.Suspend() ;

			return endPoint ;
		}

		// サーバー情報が更新されたら呼ばれるコールバック
		private void OnUpdateServerEntities( List<ServerDetactor.ServerEntity> serverEntities )
		{
			m_ServerEntities = serverEntities ;

			if( serverEntities != null )
			{
				m_Selector.ItemCount = m_ServerEntities.Count ;
			}
			else
			{
				m_Selector.ItemCount = 0 ;
			}
		}
	}
}
