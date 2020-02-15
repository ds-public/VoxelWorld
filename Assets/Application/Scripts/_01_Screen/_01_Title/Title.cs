using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DBS.nScreen.nTitle
{
	public class Title : MonoBehaviour
	{
		[SerializeField]
		protected UIImage		m_Screen ;

		//-------------------------------------------------------------------------------------------

		void Awake()
		{
			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;
		}

		IEnumerator Start()
		{
			// ApplicationManager の準備が整うのを待つ
			if( ApplicationManager.IsInitialized == false )
			{
				yield return new WaitWhile( () => ApplicationManager.IsInitialized == false ) ;
			}
			
			if( ScreenManager.IsProcessing == false )
			{
				// いきなりこのシーンを開いたケース(デバッグ動作)
				yield return ScreenManager.SetupAsync( Scene.Screen.Title ) ;
			}

			//----------------------------------------------------------

			m_Screen.isInteraction = true ;
			m_Screen.SetOnClick( OnClick ) ;

			//----------------------------------------------------------

			// フェードインを許可する
			Scene.Ready = true ;
			ScreenManager.Ready = true ;

			//----------------------------------------------------------

			// フェード完了を待つ
			yield return new WaitWhile( () => ( Scene.IsFading == true || ScreenManager.IsProcessing == true ) ) ;

			//----------------------------------------------------------

			// ＢＧＭ再生
//			BGM.PlayMain( BGM.Title ) ;
		}

		private void OnClick( string tIdentity, UIView tView )
		{
			Scene.LoadWithFade( Scene.Screen.World ) ;
		}
	}
}
