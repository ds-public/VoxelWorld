using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// 仮想ゲームパッドボタンクラス(複合ＵＩ)
	/// </summary>
	public class UIPadButton : UIImage
	{
		/// <summary>
		/// コリジョン形状
		/// </summary>
		public enum CollisionShapeTypes
		{
			Rectangle	= 0,
			Circle		= 1,
		}

		[SerializeField][HideInInspector]
		protected	CollisionShapeTypes	m_CollisionShapeType = CollisionShapeTypes.Rectangle ;

		/// <summary>
		/// コリジョン形状
		/// </summary>
		public		CollisionShapeTypes	  CollisionShapeType
		{
			get
			{
				return m_CollisionShapeType ;
			}
			set
			{
				m_CollisionShapeType = value ;
			}
		}

		[SerializeField][HideInInspector]
		protected	float				m_CollisionVolumeRatio	= 1.0f ;

		/// <summary>
		/// 円形コリジョンの場合のボリューム比率
		/// </summary>
		public		float				  CollisionVolumeRatio
		{
			get
			{
				return m_CollisionVolumeRatio ;
			}
			set
			{
				m_CollisionVolumeRatio = value ;
			}
		}


		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		protected	float		m_PadButtonRepeatPressStartingTime = 0.75f ;

		/// <summary>
		/// リピートの開始時間
		/// </summary>
		public		float		  RepeatPressStartingTime
		{
			get
			{
				return m_PadButtonRepeatPressStartingTime ;
			}
			set
			{
				m_PadButtonRepeatPressStartingTime = value ;
				m_RepeatPressStartingTime = value ;
			}
		}

		[SerializeField][HideInInspector]
		protected	float		m_PadButtonRepeatPressIntervalTime = 0.25f ;

		/// <summary>
		/// リピートの継続時間
		/// </summary>
		public		float		  RepeatPressIntervalTime
		{
			get
			{
				return m_PadButtonRepeatPressIntervalTime ;
			}
			set
			{
				m_PadButtonRepeatPressIntervalTime = value ;
				m_RepeatPressIntervalTime = value ;
			}
		}

		[SerializeField][HideInInspector]
		protected	float		m_PadButtonLongPressDecisionTime = 0.75f ;

		/// <summary>
		/// 長押しの有効時間
		/// </summary>
		public		float		  LongPressDecisionTime
		{
			get
			{
				return m_PadButtonLongPressDecisionTime ;
			}
			set
			{
				m_PadButtonLongPressDecisionTime = value ;
				m_LongPressDecisionTime = value ;
			}
		}

		[SerializeField][HideInInspector]
		protected	float		m_PadButtonLongPressLimitDistance = 0.0f ;

		/// <summary>
		/// 長押しの有効範囲(０以下で無制限)
		/// </summary>
		public		float		  LongPressLimitDistance
		{
			get
			{
				return m_PadButtonLongPressLimitDistance ;
			}
			set
			{
				m_PadButtonLongPressLimitDistance = value ;
				m_LongPressLimitDistance = value ;
			}
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		protected override void OnBuild( string option = "" )
		{
			SetSize( 96, 96 ) ;

			SetPivot( 0.5f, 0.5f ) ;

			Image image = CImage ;
			image.color = new Color32( 255, 255, 255, 255 ) ;

			IsCanvasGroup	= true ;
			RaycastTarget	= true ;	
			IsInteraction	= true ;
			IsTransition	= true ;

			AutoPivotToCenter = true ;

			SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/PadButton_Set" ) ;

			image.sprite = spriteSet.GetSprite( "B1_A" ) ;

			//----------------------------------
		}

		//---------------------------------------------

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// パッドボタン扱いで複数ドラッグを可能にする
				m_IsPadButtonEnabled = true ;

				if( m_CollisionShapeType == CollisionShapeTypes.Rectangle )
				{
					// コリジョン形状は四角(デフォルト)
					m_CollisionRadiusRatio = 0f ;
				}
				else
				{
					// コリジョン形状は円形(デフォルト)
					m_CollisionRadiusRatio = m_CollisionVolumeRatio ;
				}

				// ピボットを強制的に中心にする(ただし位置は変化させない)
				SetPivot( 0.5f, 0.5f, true ) ;

				//---------------------------------------------------------

				m_RepeatPressStartingTime	= m_PadButtonRepeatPressStartingTime ;
				m_RepeatPressIntervalTime	= m_PadButtonRepeatPressIntervalTime ;

				m_LongPressDecisionTime		= m_PadButtonLongPressDecisionTime ;
				m_LongPressLimitDistance    = m_PadButtonLongPressLimitDistance ;
			}
		}
	}
}
