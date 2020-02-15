using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// プリファレンス系情報を保持するクラス Version 2017/08/13 0
	/// </summary>
	public class PreferenceManager : SingletonManagerBase<PreferenceManager>
	{
		//---------------------------------------------------------------------------

		// カテゴリごとに分かれたプリファレンス系データ
		
		/// <summary>
		/// 多目的データ(いずれ無くなる可能性大)
		/// </summary>
		public Preference preference = new Preference() ;

		/// <summary>
		/// コンフィグデータ(ローカルに保存しなければならないものなので数は少ないはず)
		/// </summary>
		public Configuration configuration = new Configuration() ;

		//---------------------------------------------------------------------------

		new protected void Awake()
		{
			base.Awake() ;

			// プリファレンスをストレージから読み出す
			preference.LoadFromStorage() ;

			// コンフィギュレーションをストレージから読み出す
			configuration.LoadFromStorage() ;
		}
		
		//---------------------------------------------------------------------------

		/// <summary>
		/// 全てのプリファレンス系データをセーブする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Save_Private() ;
		}

		// 全てのプリファレンス系データをセーブする
		private bool Save_Private()
		{
			preference.SaveToStorage() ;

			configuration.SaveToStorage() ;

			return true ;
		}

		/// <summary>
		/// 全てのプリファレンス系データをロードする
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.Load_Private() ;
		}

		// 全てのプリファレンス系データをロードする
		private bool Load_Private()
		{
			preference.LoadFromStorage() ;

			configuration.LoadFromStorage() ;

			return true ;
		}

		//---------------------------------------------------------------------------

	}

}

