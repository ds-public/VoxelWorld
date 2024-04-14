using System ;
using System.Collections.Generic ;
using System.Threading ;
using UnityEngine ;

using Cysharp.Threading.Tasks ;
using MathHelper ;

using uGUIHelper ;

namespace DSW.UI
{
	/// <summary>
	/// シーン内部ダイアログの制御クラス(シーン独立ダイアログではない) Version 2023/07/30
	/// </summary>
	public abstract class SceneDialogControllerBase : ExMonoBehaviour
	{
		[Header( "シーン固有ダイアログの共通部分" )]

		[SerializeField]
		protected UICanvas							m_DialogCanvas ;

		[SerializeField]
		protected UIImage							m_Mask ;

		//-------------------------------------------------------------------------------------------

		// ダイアログ群を保持する
		protected	List<SceneDialogBase>			m_SceneDialogs ;

		//-------------------------------------------------------------------------------------------

		internal void Awake()
		{
			// ダイアログ群の土台のキャンバスを非アクティブにしておく
			m_DialogCanvas.SetActive( false ) ;

			//----------------------------------
			// ダイアログ群の設定を行う

			m_SceneDialogs = new List<SceneDialogBase>() ;

			// 継承クラスでダイアログ群を登録してもらう
			RegisterDialog( in m_SceneDialogs ) ;

			// ダイアログ群を非表示にしてオーナーのコントローラーを登録する
			foreach( var sceneDialog in m_SceneDialogs )
			{
				sceneDialog.gameObject.SetActive( false ) ;
				sceneDialog.SetOwner( this ) ;
			}
		}

		/// <summary>
		/// ダイアログ群を登録する
		/// </summary>
		/// <param name="dialogs"></param>
		protected abstract void RegisterDialog( in List<SceneDialogBase> dialogs ) ;

		//-----------------------------------------------------------

		/// <summary>
		/// 開いているダイアログの数を取得する
		/// </summary>
		/// <returns></returns>
		public	int GetOpeningCount()
		{
			int count = 0 ;


			foreach( var sceneDialog in m_SceneDialogs )
			{
				if( sceneDialog.IsOpening == true )
				{
					count ++ ; 
				}
			}

			return count ;
		}

		/// <summary>
		/// コントローラーを開始する
		/// </summary>
		public bool StartupController()
		{
			if( m_DialogCanvas.ActiveSelf == false )
			{
				int count = GetOpeningCount() ;
				if( count == 0 )
				{
					// ダイアログコントローラーを有効化する
					m_DialogCanvas.SetActive( true ) ;

					return true ;	// 有効になった
				}
			}

			return false ;
		}

		/// <summary>
		/// コントローラーを終了する
		/// </summary>
		public void CleanupController()
		{
			if( m_DialogCanvas.ActiveSelf == true )
			{
				int count = GetOpeningCount() ;
				if( count == 0 )
				{
					// 開いているダイアログが無くなれば全体も閉じる
					m_DialogCanvas.SetActive( false ) ;
				}
			}
		}

		/// <summary>
		/// マスクをフェードインさせる
		/// </summary>
		public void FadeInMask()
		{
			if( GetOpeningCount() >  1 )
			{
				return ;
			}

			m_Mask.PlayTween( "FadeIn" ) ;
		}

		/// <summary>
		/// マスクをフェードアウトさせる
		/// </summary>
		public void FadeOutMask()
		{
			if( GetOpeningCount() >  1 )
			{
				return ;
			}

			m_Mask.PlayTweenAndHide( "FadeOut" ) ;
		}

		/// <summary>
		/// 表示する
		/// </summary>
		public void Show()
		{
			m_DialogCanvas.SetActive( true ) ;
		}

		/// <summary>
		/// 隠蔽する
		/// </summary>
		public void Hide()
		{
			m_DialogCanvas.SetActive( false ) ;
		}
	}
}


