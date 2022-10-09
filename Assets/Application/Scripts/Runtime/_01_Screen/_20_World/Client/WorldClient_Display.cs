using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.SceneManagement ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

using DBS.World ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(メイン)
	/// </summary>
	public partial class WorldClient
	{
		/// <summary>
		/// ガイドメッセージの表示設定を行う
		/// </summary>
		public void SetGuideMessage()
		{
			if( m_Focus == false )
			{
				// ガイドメッセージのアニメーションを行う
				m_GuideMessage.SetActive( true ) ;
				_ = m_GuideMessage.PlayTween( "Move" ) ;

				Ripple.Off() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// 全てを表示にする
		private void SetVisible()
		{
			m_PlayingLayer.SetActive( m_Focus ) ;
			m_PausingLayer.SetActive( false ) ;
			m_DisplayLayer.SetActive( true ) ;
		}

		// アイテムショートカットを設定する
		private void PrepareActiveItemSlots()
		{
			int i, l = m_ActiveItemSlots.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ActiveItemSlots[ i ].Setup( i, OnSelectedBlockIndex ) ;
			}

			// 初期値
			m_ActiveItemSlotIndex = 0 ;
			m_SelectedBlockIndex = 1 ;
			
			RefreshActiveItemSlots( m_ActiveItemSlotIndex ) ;
		}

		/// <summary>
		/// アイテムショートカットを選択した
		/// </summary>
		/// <param name="selectedBlockIndex"></param>
		private void OnSelectedBlockIndex( int itemShortCutIndex )
		{
			if( m_ActiveItemSlotIndex != itemShortCutIndex )
			{
				m_ActiveItemSlotIndex  = itemShortCutIndex ;

				// 表示を更新する
				RefreshActiveItemSlots( m_ActiveItemSlotIndex ) ;

				//---------------------------------
				// 選択中のブロックインデックスを更新する(仮)
				m_SelectedBlockIndex = 1 + m_ActiveItemSlotIndex ;
			}
		}

		/// <summary>
		/// アイテムショートカットの表示を更新する
		/// </summary>
		private void RefreshActiveItemSlots( int selectedItemShortCutIndex )
		{
			int i, l = m_ActiveItemSlots.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ActiveItemSlots[ i ].SetCursor( i == selectedItemShortCutIndex ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// プレイヤーのネームプレートの表示を更新する
		private void UpdatePlayerNamePlates()
		{
			if( m_ClientPlayers != null && m_ClientPlayers.Count >  0 )
			{
				Camera playerCamera = m_PlayerActor.GetCamera() ;

				foreach( var clientPlayer in m_ClientPlayers.Values )
				{
					if( clientPlayer.ClientId != m_ClientId )
					{
						// 自分では無い
						clientPlayer.UpdateNamePlatePosition( m_NamePlateRoot, playerCamera ) ;
					}
				}
			}
		}
	}
}
