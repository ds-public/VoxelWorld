using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using StorageHelper ;

using uGUIHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// ムービー表示クラス Version 2017/08/13 0
	/// </summary>
	public class Movie : MonoBehaviour
	{
		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public static bool isPlaying
		{
			get
			{
				Movie tMovie = GameObject.FindObjectOfType<Movie>() ;
				if( tMovie == null )
				{
					return false ;
				}

				return tMovie.m_IsPlaying ;
			}
		}

		// 再生中かどうか
		[SerializeField]
		private bool m_IsPlaying = false ;

		// 対象のパス
//		[SerializeField][NonSerialized]
//		private string m_Path = "" ;

		// 中断出来るかどうか
//		[SerializeField][NonSerialized]
//		private bool m_IsCancelOnInput = false ;

		// 終了時のコールバックメソッド
		private Action m_OnFinished ;

		/// <summary>
		/// ムービーを再生する
		/// </summary>
		/// <param name="tURL">表示するサイトのＵＲＬ</param>
		/// <param name="tCallback">サイトで入力あった場合に呼び出されるコールバックメソッド(ウェブビューのゲームオブジェクト・サイトからの入力文字列)</param>
		/// <returns>ウェブビューのゲームオブジェクト</returns>
		public static bool Play( string tPath, bool tIsCancelOnInput = true, Action tOnFinished = null )
		{
			if( string.IsNullOrEmpty( tPath ) == true )
			{
				return false ;
			}

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

			bool tResult = false ;

			if( tPath.IndexOf( "StreamingAssets://" ) == 0 )
			{
				// StreamingAssets から再生する
				tResult = StorageAccessor.PlayMovieFromStreamingAssets( tPath.Replace( "StreamingAssets://", "" ), tIsCancelOnInput ) ;
			}
			else
			{
				// ストレージから再生する
				tResult = StorageAccessor.PlayMovie( tPath, tIsCancelOnInput ) ;
			}

			if( tResult == true )
			{
				if( tOnFinished != null )
				{
					tOnFinished() ;
				}
				
				return true ;
			}
#endif

			Movie tMovie = GameObject.FindObjectOfType<Movie>() ;
			if( tMovie != null )
			{
				return false ;
			}

			// 新規にムービービューを生成する
			tMovie = ( new GameObject( "Movie" ) ).AddComponent<Movie>() ;

			tMovie.m_IsPlaying = true ;
//			tMovie.m_Path = tPath ;
//			tMovie.m_IsCancelOnInput = tIsCancelOnInput ;
			tMovie.m_OnFinished = tOnFinished ;

			UICanvas tCanvas = UICanvas.CreateWithCamera( tMovie.transform, 540, 960 ) ;

			Camera tCamera = tCanvas.WorldCamera ;
			tCamera.depth = 99 ;
			tCamera.clearFlags = CameraClearFlags.Nothing ;


			UIImage tScreen = tCanvas.AddView<UIImage>( "Movie Screen" ) ;
			tScreen.SetAnchorToStretch() ;
			tScreen.isEventTrigger = true ;
			tScreen.isInteraction = true ;

			tScreen.Color = new Color( 0.0f, 0.0f, 0.0f, 0.5f ) ;
			tScreen.SetOnClick( tMovie.OnClick ) ;

			UIText tMessage = tScreen.AddView<UIText>() ;
			tMessage.SetAnchorToCenter() ;
			tMessage.Color = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
			tMessage.isOutline = true ;
			tMessage.FontSize = 30 ;
			tMessage.Alignment = TextAnchor.MiddleCenter ;

			tMessage.Text = "ダミーのムービービューです\n閉じるには画面をクリックしてください" ;

			return true ;
		}

		// ダミーのスクリーンがクリックされた
		private void OnClick( string tIdentity, UIView tView )
		{
			m_IsPlaying = false ;
			if( m_OnFinished != null )
			{
				m_OnFinished() ;
			}

			GameObject.Destroy( tView.transform.parent.parent.gameObject ) ;
		}
	}
}