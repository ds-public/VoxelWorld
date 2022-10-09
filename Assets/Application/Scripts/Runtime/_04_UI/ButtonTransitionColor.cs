using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace DBS.UI
{
	/// <summary>
	/// ButtonTransition 用の共通 ColorTint クラス
	/// </summary>
	[CreateAssetMenu( fileName = "ButtonTransitionColor", menuName = "ScriptableObject/DBS/ButtonTransitionColor" )]
	public class ButtonTransitionColor : ScriptableObject
	{
		/// <summary>
		/// ノーマル状態の色
		/// </summary>
		public		Color32	  Normal		=> m_Normal ;

		[SerializeField]
		protected	Color32	m_Normal		= new Color32( 255, 255, 255, 255 ) ;


		/// <summary>
		/// ハイライト状態の色
		/// </summary>
		public		Color32	  Highlighted	=> m_Highlighted ;

		[SerializeField]
		protected	Color32	m_Highlighted	= new Color32( 247, 247, 247, 255 ) ;


		/// <summary>
		/// プレス状態の色
		/// </summary>
		public		Color32	  Pressed		=> m_Pressed ;

		[SerializeField]
		protected	Color32	m_Pressed		= new Color32( 199, 199, 199, 255 ) ;


		/// <summary>
		/// セレクト状態の色
		/// </summary>
		public		Color32	  Selected		=> m_Selected ;

		[SerializeField]
		protected	Color32	m_Selected		= new Color32( 247, 247, 247, 255 ) ;


		/// <summary>
		/// ディスエーブル状態の色
		/// </summary>
		public		Color32	  Disabled		=> m_Disabled ;

		[SerializeField]
		protected	Color32	m_Disabled		= new Color32( 143, 143, 143, 255 ) ;


		/// <summary>
		/// フェードの変化時間
		/// </summary>
		public		float	  FadeDuration	=> m_FadeDuration ;

		[SerializeField]
		protected	float	m_FadeDuration	= 0.1f ;

		//-------------------------------------------------------------------------------------------
	}
}
