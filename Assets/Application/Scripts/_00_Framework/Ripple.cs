using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.XR ;

using uGUIHelper ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS
{
	/// <summary>
	/// 入力に対する反応演出のクラス Version 2019/10/01 0
	/// </summary>
	public class Ripple : MonoBehaviour
	{
		// シングルトンインスタンス
		private static Ripple m_Instance ;

		/// <summary>
		/// インスタンス
		/// </summary>
		public  static Ripple   Instance
		{
			get
			{
				return m_Instance ;
			}
		}

		//-------------------------------------

		[SerializeField]
		protected UICanvas	m_Canvas ;

		[SerializeField]
		protected UIImage	m_Screen ;

		[SerializeField]
		protected UICircle	m_Circle ;

		[SerializeField]
		protected UICircle	m_Star ;

		[SerializeField]
		protected UILine	m_Line ;

		//-----------------------------------------------------------

		public UICanvas TargetCanvas
		{
			get
			{
				return m_Canvas ;
			}
		}
		
		private bool m_IsOn = true ;

		//---------------------------------------------------------------------------

		void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			if( m_Circle != null )
			{
				m_Circle.SetActive( false ) ;
			}

			if( m_Star != null )
			{
				m_Star.SetActive( false ) ;
			}

			if( m_Line != null )
			{
				m_Line.SetActive( true ) ;
				m_Line.TrailEnabled = true ;	// トレイルモードを有効にする
			}
		}
			
		void Update()
		{
			if( XRSettings.enabled == false )
			{
				m_Canvas.SetActive( true ) ;
				ProcessMove() ;
			}
			else
			{
				m_Canvas.SetActive( false ) ;
			}
		}

		private int m_TouchState ;
		private Vector2 m_TouchPosition ;

		private float m_Interval ;

		private void ProcessMove()
		{
			Vector2 touchPosition = Vector2.zero ;
			int touchState = m_Screen.GetSinglePointer( ref touchPosition ) ;

			if( touchState == 1 && m_IsOn == true )
			{
				if( m_TouchState == 0 )
				{
					// 押された
					//-------------------------

					m_TouchState = touchState ;
					m_TouchPosition = touchPosition ;

					UICircle circle = m_Circle.Clone<UICircle>() ;

					// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
					circle.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
					circle.SetPosition( touchPosition ) ;	// 座標設定
					circle.SetActive( true ) ;		// 有効化

					//-------------------------

					m_Interval = Time.realtimeSinceStartup ;

					UICircle star = m_Star.Clone<UICircle>() ;

					// 方向ランダム指定
					int a = Random.Range(   0, 360 ) ;
					float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
					float d = Random.Range( 8.0f, 16.0f ) ;
					Vector3 v = Vector3.zero ;
					v.x = Mathf.Cos( r ) * d ;
					v.y = Mathf.Sin( r ) * d ;

					star.GetTween( "FadeOut" ).PositionTo = v ;

					// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
					star.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
					star.SetPosition( touchPosition ) ;	// 座標設定
					star.SetActive( true ) ;		// 有効化

					//-------------------------

					// 新規で押された場合は表示中の軌跡は全てクリアする
					m_Line.ClearTrailPosition() ;
					m_Line.AddTrailPosition( touchPosition ) ;
				}
				else
				if( m_TouchState == touchState )
				{
					// 移動した
					//-------------------------

					if( m_TouchPosition != touchPosition )
					{
						// きちんと移動していたら処理する
						m_TouchPosition = touchPosition ;
						if( ( Time.realtimeSinceStartup - m_Interval ) >  0.1f )
						{
							m_Interval = Time.realtimeSinceStartup ;

							UICircle star = m_Star.Clone<UICircle>() ;

							// 方向ランダム指定
							int a = Random.Range(   0, 360 ) ;
							float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
							float d = Random.Range( 8.0f, 16.0f ) ;
							Vector3 v = Vector3.zero ;
							v.x = Mathf.Cos( r ) * d ;
							v.y = Mathf.Sin( r ) * d ;

							star.GetTween( "FadeOut" ).PositionTo = v ;

							// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
							star.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
							star.SetPosition( touchPosition ) ;	// 座標設定
							star.SetActive( true ) ;       // 有効化
						}

						//-------------------------

						m_Line.AddTrailPosition( touchPosition ) ;
					}
				}
			}
			else
			{
				// 離された
				m_TouchState = 0 ;
			}
		}

		private void OnFinish( string identity, UITween tween )
		{
			// 終了したら自身を破棄する
			Destroy( tween.gameObject ) ;
		}

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void On()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_IsOn = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Off()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_IsOn = false ;
		}
	}
}
