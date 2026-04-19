using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.SceneManagement;

using TMPro ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	public class UILocalization
	{
		/// <summary>
		/// ローカライズ対応処理
		/// </summary>
		private static Func<string,string> m_OnProcess ;

		private static readonly Dictionary<Action<string>,string> m_Requests = new () ;

		/// <summary>
		/// ローカライズのリクエストを登録する
		/// </summary>
		/// <param name="onLocalized"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool AddRequest( Action<string> onLocalized, string key )
		{
			if( m_OnProcess != null )
			{
				// 既にローカライズ機構の準備が整っている
				onLocalized( m_OnProcess( key ) ) ;
				return true ;
			}

			// まだ準備が整っていないので一旦キューに貯める
			m_Requests.Add( onLocalized, key ) ;

			return false ;
		}

		/// <summary>
		/// ローカライズのリクエストを削除する
		/// </summary>
		/// <param name="onLocalized"></param>
		public static void RemoveRequest( Action<string> onLocalized )
		{
			if( m_Requests.ContainsKey( onLocalized ) == true )
			{
				m_Requests.Remove( onLocalized ) ;
			}
		}

		/// <summary>
		/// ローカライズの処理機構をセットする
		/// </summary>
		/// <param name="onProcess"></param>
		public static void SetOnProcess( Func<string,string> onProcess )
		{
			m_OnProcess = onProcess ;
			if( m_OnProcess != null )
			{
				// 既にリクエストが溜まっていれば処理する
				if( m_Requests.Count >  0 )
				{
					foreach( var request in m_Requests )
					{
						request.Key( m_OnProcess( request.Value ) ) ;
					}
					m_Requests.Clear() ;
				}
			}
		}

		/// <summary>
		/// 言語を設定する
		/// </summary>
		/// <param name="languageCodeName"></param>
		public static void Refresh()
		{
			// 現在のヒエラルキーに存在する UITextMesh UIImage に言語が切り替わった事を通知する

			// UITextMesh UINumberMesh のテキストを更新する
			var texts = GetAllComponentsInHierarchy<UITextMesh>() ;    // UITextMesh UINumberMesh をすべて取得する
			if( texts != null && texts.Count >  0 )
			{
				// 言語が切り替わった事を通知する
				foreach( var text in texts )
				{
					text.OnLanguageChanged() ;
				}
			}

			// UIImage のイメージを更新する
			var images = GetAllComponentsInHierarchy<UIImage>() ;    // UImage UIButton をすべて取得する
			if( images != null && images.Count >  0 )
			{
				foreach( var image in images )
				{
					image.OnLanguageChanged() ;
				}
			}
		}

		private static List<T> GetAllComponentsInHierarchy<T>() where T : Component
		{
			var components = new List<T>() ;
		
			// 現在の全アクティブシーンのルートオブジェクトを取得
			int sceneIndex ;
			for( sceneIndex = 0 ; sceneIndex <  SceneManager.sceneCount ; sceneIndex ++ )
			{
				var scene = SceneManager.GetSceneAt( sceneIndex ) ;
				if( scene.isLoaded == true )
				{
					var rootObjects = scene.GetRootGameObjects() ;
					foreach( var rootObject in rootObjects )
					{
						// 第2引数を true にすることで非アクティブも対象にする
						components.AddRange( rootObject.GetComponentsInChildren<T>( true ) ) ;
					}
				}
			}

			return components ;
		}


	}
}
