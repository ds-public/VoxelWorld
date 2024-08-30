using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.XR ;

using uGUIHelper ;

namespace DSW
{
	/// <summary>
	/// 入力に対する反応演出のクラス Version 2024/08/15 0
	/// </summary>
	public class Ripple : ExMonoBehaviour
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

		/// <summary>
		/// ＶＲ対応用にキャンバスを取得できるようにする
		/// </summary>
		public UICanvas Canvas
		{
			get
			{
				return m_Canvas ;
			}
		}

		//-------------------------------------

		[Header( "タッチエフェクト関連" )]

		[SerializeField]
		protected UIView	m_Screen ;

		[SerializeField]
		protected UICircle	m_Circle ;

		[SerializeField]
		protected UICircle	m_Star ;

		[SerializeField]
		protected UILine	m_Line ;

		[Header( "ボタンエフェクト関連" )]

		[SerializeField]
		protected UIView	m_PositiveButtonEffect ;

		[SerializeField]
		protected UIView	m_NegativeButtonEffect ;

        // この値よりボタン幅(短い方)が小さい場合にボタンエフェクトに縮小のスケーリングがかかる
		[SerializeField]
		protected float		m_ScalingThreshold =  40.0f ;

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンエフェクトの種類
		/// </summary>
		public enum ButtonEffectTypes
		{
			None,
			Positive,
			Negative,
		}

		//-----------------------------------------------------------

		public UICanvas TargetCanvas
		{
			get
			{
				return m_Canvas ;
			}
		}
		
		private bool m_IsOn = true ;

		private bool m_Focus = true ;
		private int m_FocusWait = 1 ;

		/// <summary>
		/// インスタンスとして生成されたエフェクトの一覧
		/// </summary>
		private readonly List<GameObject> m_TouchEffects = new () ;


		//---------------------------------------------------------------------------

		internal void Awake()
		{
			m_Instance = this ;

			// そのシーンをシーンが切り替わっても永続的に残すようにする場合、そのシーン側で DontDestroyOnLoad を実行してはならない。
			// DontDestroyOnLoad は、呼び出し側のシーンで、そのシーンに対して実行すること。

			if( m_PositiveButtonEffect != null )
			{
				m_PositiveButtonEffect.SetActive( false ) ;
			}

			if( m_NegativeButtonEffect != null )
			{
				m_NegativeButtonEffect.SetActive( false ) ;
			}

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
			
		internal void Update()
		{
			if( m_Focus == false )
			{
				return ;
			}

			if( m_FocusWait >  0 )
			{
				// フォーカスを得た直後は取得できるマウスの座標がおかしいので１フレームだけ待つ
				m_FocusWait -- ;
				return ;
			}

			//----------------------------------

			// 削除されたものは除外する
			for( var i  = 0 ; i < m_TouchEffects.Count ; i ++ )
			{
				if( m_TouchEffects[ i ] == null )
				{
					m_TouchEffects.RemoveAt( i ) ;
				}
			}

			//----------------------------------

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

		// 軌跡を処理する
		private void ProcessMove()
		{
			( int touchState, Vector2 touchPosition ) = m_Screen.GetSinglePointer() ;

			if( touchState == 1 && m_IsOn == true )
			{
				if( m_TouchState == 0 )
				{
					// 押された
					//-------------------------

					m_TouchState = touchState ;
					m_TouchPosition = touchPosition ;

					UICircle circle = m_Circle.Duplicate<UICircle>() ;
					m_TouchEffects.Add( circle.gameObject ) ;

					// デリゲートのポインタは保持されないので複製したオブジェクトごとに設定してやる必要がある
					circle.GetTween( "FadeOut" ).SetOnFinished( OnFinish ) ;
					circle.SetPosition( touchPosition ) ;	// 座標設定
					circle.SetActive( true ) ;		// 有効化

					//-------------------------

					m_Interval = Time.realtimeSinceStartup ;

					UICircle star = m_Star.Duplicate<UICircle>() ;
					m_TouchEffects.Add( star.gameObject ) ;

					// 方向ランダム指定
					int a = UnityEngine.Random.Range(   0, 360 ) ;
					float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
					float d = UnityEngine.Random.Range(  8.0f, 16.0f ) ;
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

							UICircle star = m_Star.Duplicate<UICircle>() ;
							m_TouchEffects.Add( star.gameObject ) ;

							// 方向ランダム指定
							int a = UnityEngine.Random.Range(   0, 360 ) ;
							float r = 2.0f * Mathf.PI * ( float )a / 360.0f ;
							float d = UnityEngine.Random.Range(  8.0f, 16.0f ) ;
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
			// 登録も削除する
			m_TouchEffects.Remove( tween.gameObject ) ;

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

			m_Instance.m_TouchState = 0 ;
			m_Instance.m_Line.ClearTrailPosition() ;
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

			m_Instance.m_TouchState = 0 ;
			m_Instance.m_Line.ClearTrailPosition() ;

			//----------------------------------------------------------

			// インスタンス化したエフェクトも削除する
			foreach( var touchEffect in m_Instance.m_TouchEffects )
			{
				if( touchEffect != null )
				{
//					GameObject.Destroy( touchEffect ) ;
					GameObject.DestroyImmediate( touchEffect ) ;	// 即時消さなくてはならない(ブラー関係のため)
				}
			}
			m_Instance.m_TouchEffects.Clear() ;
		}

		internal void OnApplicationFocus( bool state )
		{
			m_Focus = state ;

			if( state == true )
			{
				m_TouchState = 0 ;
				m_Line.ClearTrailPosition() ;

				// フォーカスを得た直後は取得できるマウスの座標がおかしいので１フレームだけ待つ
				m_FocusWait = 1 ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ボタンエフェクトを再生する


		/// <summary>
		/// ボタンエフェクトを再生する
		/// </summary>
		/// <param name="isPositive"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public static void PlayButtonEffect( ButtonEffectTypes buttonEffectType, UIView view, Action onFinished = null )
		{
			if( m_Instance == null )
			{
				return ;
			}

			Vector2 position	= view.PositionInCanvas ;
			Vector2 size		= view.Size ;

			Vector2 pivot		= view.Pivot ;

			position.x += ( size.x * ( 0.5f - pivot.x ) ) ; 
			position.y += ( size.y * ( 0.5f - pivot.y ) ) ; 

			m_Instance.PlayButtonEffect_Private( buttonEffectType, position, size, onFinished ) ;
		}

		/// <summary>
		/// ボタンエフェクトを再生する
		/// </summary>
		/// <param name="isPositive"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public static void PlayButtonEffect( ButtonEffectTypes buttonEffectType, Vector2 position, Vector2 size, Action onFinished = null )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.PlayButtonEffect_Private( buttonEffectType, position, size, onFinished ) ;
		}

		private void PlayButtonEffect_Private( ButtonEffectTypes buttonEffectType, Vector2 position, Vector2 size, Action onFinished )
		{
			UIView target ;
			if( buttonEffectType == ButtonEffectTypes.Positive )
			{
				target = m_PositiveButtonEffect ;
			}
			else
			if( buttonEffectType == ButtonEffectTypes.Negative )
			{
				target = m_NegativeButtonEffect ;
			}
			else
			{
				return ;
			}

			target.StopAnimator() ;

			target.SetPosition( position ) ;

			//----------------------------------------------------------

			// 矩形(の辺)サイズが一定値より小さい場合はエフェクトに縮小補正をかける
			float scale = 1 ;
			float maxScale =  1.00f ;
			float minScale =  0.01f ;
			float mapSize = m_ScalingThreshold ;		// キャンバスのスケール次第で変化する
			float minSize = Mathf.Min( Mathf.Abs( size.x ), Mathf.Abs( size.y ) ) ;
			if( minSize <  mapSize )
			{
				if( minSize >  0 )
				{
					scale = ( maxScale - minScale ) * ( minSize / mapSize ) + minScale ;
				}
				else
				{
					scale = minScale ;  // 最小限界
				}
			}

			//----------------------------------------------------------

			target.SetSize( size / scale ) ;
			target.SetScale( scale ) ;

			target.SetActive( true ) ;
			target.PlayAnimator( "Play", onFinished:( bool state ) =>
			{
				target.SetActive( false ) ;
				onFinished?.Invoke() ;
			} ) ;
		}


		/// <summary>
		/// ボタンエフェクトを停止する(ブラー用)
		/// </summary>
		/// <param name="buttonEffectType"></param>
		public static void StopButtonEffect( ButtonEffectTypes buttonEffectType )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.StopButtonEffect_Private( buttonEffectType ) ;
		}

		private void StopButtonEffect_Private( ButtonEffectTypes buttonEffectType )
		{
			UIView target ;
			if( buttonEffectType == ButtonEffectTypes.Positive )
			{
				target = m_PositiveButtonEffect ;
			}
			else
			if( buttonEffectType == ButtonEffectTypes.Negative )
			{
				target = m_NegativeButtonEffect ;
			}
			else
			{
				return ;
			}

			target.StopAnimator() ;
			target.SetActive( false ) ;
		}
	}
}
