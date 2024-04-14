using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// 仮想ゲームパッドアクシスクラス(複合ＵＩ)
	/// </summary>
	public class UIPadAxis : UIImage
	{
		// 自身
		private UIImage		m_View ;

		/// <summary>
		/// 枠
		/// </summary>
		[SerializeField]
		protected UIImage m_Frame ;
		/// <summary>
		/// 枠
		/// </summary>
		public    UIImage   Frame{ get{ return m_Frame ; } set{ m_Frame = value ; } }

		/// <summary>
		/// 棒
		/// </summary>
		[SerializeField]
		protected UIImage m_Thumb ;
		/// <summary>
		/// 棒
		/// </summary>
		public    UIImage   Thumb{ get{ return m_Thumb ; } set{ m_Thumb = value ; } }

		/// <summary>
		/// 値が変化した際に呼び出すコールバック→( string identity, UIPadAxis padAxis, float magnitude, Vector2 direction )
		/// </summary>
		public Action<string, UIPadAxis, float, Vector2> OnValueChangedAction ;

		/// <summary>
		/// 値が変化した際に呼び出すコールバック→( string identity, UIPadAxis padAxis, float magnitude, Vector2 direction )
		/// </summary>
		public void SetOnValueChanged( Action<string, UIPadAxis, float, Vector2> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}

		/// <summary>
		/// 押された時と離された時に呼び出すコールバック→( string identity, UIPadAxis padAxis, bool isPress, Vector2 position, float releaseTime )
		/// </summary>
		public Action<string, UIPadAxis, bool, Vector2, float> OnActivateAction ;

		/// <summary>
		/// 押された時と離された時に呼び出すコールバック→( string identity, UIPadAxis padAxis, bool isPress, Vector2 position, float releaseTime )
		/// </summary>
		public void SetOnActivate( Action<string, UIPadAxis, bool, Vector2, float> onActivateAction )
		{
			OnActivateAction = onActivateAction ;
		}

		/// <summary>
		/// Ｙ軸反転
		/// </summary>
		[SerializeField]
		protected bool m_YAxisInversion = false ;
		/// <summary>
		/// Ｙ軸反転
		/// </summary>
		public    bool   YAxisInversion
		{
			get{ return m_YAxisInversion ; }
			set{ m_YAxisInversion = value ; }
		}

		/// <summary>
		/// 横方向機能停止
		/// </summary>
		[SerializeField]
		protected bool m_HorizontalFunctionStop = false ;
		/// <summary>
		/// 横方向機能停止
		/// </summary>
		public    bool   HorizontalFunctionStop
		{
			get{ return m_HorizontalFunctionStop ; }
			set{ m_HorizontalFunctionStop = value ; }
		}

		/// <summary>
		/// 縦方向機能停止
		/// </summary>
		[SerializeField]
		protected bool m_VerticalFunctionStop = false ;
		/// <summary>
		/// 縦方向機能停止
		/// </summary>
		public    bool   VerticalFunctionStop
		{
			get{ return m_VerticalFunctionStop ; }
			set{ m_VerticalFunctionStop = value ; }
		}

		/// <summary>
		/// 位置の固定化と常時表示をにするかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AlwaysDisplay = false ;
		/// <summary>
		/// 位置の固定化と常時表示をにするかどうか
		/// </summary>
		public    bool   AlwaysDisplay
		{
			get{ return m_AlwaysDisplay ; }
			set{ m_AlwaysDisplay = value ; }
		}

		/// <summary>
		/// 位置の固定化と常時表示を有効にした場合にタッチ反応範囲を限定化するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_InteractionRangeEnabled = true ;
		/// <summary>
		/// 位置の固定化と常時表示を有効にした場合にタッチ反応範囲を限定化するかどうか
		/// </summary>
		public    bool   InteractionRangeEnabled
		{
			get{ return m_InteractionRangeEnabled ; }
			set{ m_InteractionRangeEnabled = value ; }
		}

		/// <summary>
		/// 形状
		/// </summary>
		public enum ShapeTypes
		{
			Circle,
			Rectangle,
		}

		/// <summary>
		/// 反応の形状
		/// </summary>
		[SerializeField]
		protected ShapeTypes m_ShapeType = ShapeTypes.Circle ;
		/// <summary>
		/// 反応の形状
		/// </summary>
		public    ShapeTypes   ShapeType
		{
			get{ return m_ShapeType ; }
			set{ m_ShapeType = value ; }
		}

		/// <summary>
		/// 余剰入力を有効にするかどうか(レイキャストブロックされても最後の入力は継続するかどうか)
		/// </summary>
		[SerializeField]
		protected bool m_SurplusInputEnabled = false ;
		/// <summary>
		/// 位置の固定化と常時表示をにするかどうか
		/// </summary>
		public    bool   SurplusInputEnabled
		{
			get{ return m_SurplusInputEnabled ; }
			set{ m_SurplusInputEnabled = value ; }
		}

		//-----------------------------------------------------------

		// OnAvtivateの時間計測用
		protected float m_BaseTime ;

		//-----------------------------------------------------------

		// タッチポイントのＩＤ
		private int		m_PadAxisPointerId = m_UnKnownCode ;
		private Vector2	m_BasePosition ;

		//-----------------------------------------------------------

		/// <summary>
		/// 傾き
		/// </summary>
		public  float   Magnitude	=> m_Magnitude ;
		private float m_Magnitude = 0 ;

		/// <summary>
		/// 方向(単位ベクトル)
		/// </summary>
		public  Vector2   Direction	=> m_Direction ;
		private Vector2 m_Direction ;

		/// <summary>
		/// 方向と傾き
		/// </summary>
		public Vector2 Axis	=> m_Magnitude * m_Direction ;

		/// <summary>
		/// 方向と傾き
		/// </summary>
		public Vector2 Velocity	=> m_Magnitude * m_Direction ;

		/// <summary>
		/// デジタルでの８方向の入力状態(-1～0～+1)
		/// </summary>
		public Vector2 DPad
		{
			get
			{
				var axis = Vector2.zero ;

				if( m_Magnitude == 0 || ( m_Direction.x == 0 && m_Direction.y == 0 ) )
				{
					return Vector2.zero ;
				}

				Vector2 direction = m_Direction.normalized ;

				float limit = 0.4226f ;	// 60 = 0.5f  45 = 0.7071f 

				if( direction.x <  ( - limit ) )
				{
					axis.x = -1 ;
				}
				else
				if( direction.x >  (   limit ) )
				{
					axis.x =  1 ;
				}

				if( direction.y <  ( - limit ) )
				{
					axis.y = -1 ;
				}
				else
				if( direction.y >  (   limit ) )
				{
					axis.y =  1 ;
				}

				return axis ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="option"></param>
		protected override void OnBuild( string option = "" )
		{
			SetAnchorToStretch() ;
			SetPivot( 0.5f, 0.5f ) ;

			Image image = CImage ;
			image.color = new Color32( 255, 255, 255, 32 ) ;

			IsCanvasGroup = true ;
			IsInteraction = true ;

			SpriteSet spriteSet = SpriteSet.Create( "uGUIHelper/Textures/UISimpleJoystick" ) ;

			m_Frame = AddView<UIImage>( "Frame" ) ;
			m_Frame.SetAnchorToCenter() ;
			m_Frame.Sprite = spriteSet[ "UISimpleJoystick_Frame_Type_0" ] ;
			m_Frame.SetSize( 128, 128 ) ;

			float thumbWidth = m_Frame.Width * 0.6f ;

			m_Thumb = m_Frame.AddView<UIImage>( "Thumb" ) ;
			m_Thumb.SetAnchorToCenter() ;
			m_Thumb.Sprite = spriteSet[ "UISimpleJoystick_Thumb_Type_0" ] ;
			m_Thumb.SetSize( thumbWidth, thumbWidth ) ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart() ;

			// 自身を取得する
			TryGetComponent<UIImage>( out m_View ) ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// パッドボタン扱いで複数ドラッグを可能にする
				m_IsPadButtonEnabled = true ;

				// ピボットを強制的に中心にする(ただし位置は変化させない)
				SetPivot( 0.5f, 0.5f, true ) ;

				// ピボットを強制的に中心にする(ただし位置は変化させない)
				m_Frame.SetPivot( 0.5f, 0.5f, true ) ;

				if( m_AlwaysDisplay == false )
				{
					m_Frame.SetActive( false ) ;
				}

				m_Thumb.SetPosition( 0, 0 ) ;
			}
		}

		//---------------------------------------------

		// Down
		protected override void OnPointerDownBasic( PointerEventData pointer, bool fromScrollView )
		{
			base.OnPointerDownBasic( pointer, fromScrollView ) ;

			m_BasePosition = GetLocalPosition( pointer ) ;

			m_Thumb.SetPosition( 0, 0 ) ;

			if( m_AlwaysDisplay == false )
			{
				// 押した位置に表示する
				m_Frame.SetPosition( m_BasePosition ) ;
				m_Frame.SetActive( true ) ;
			}
			else
			{
				// 常に表示する

				if( m_InteractionRangeEnabled == true )
				{
					// 押した位置が有効範囲内でなければならない
					Vector2 frameCenter = m_Frame.Position ;

					if( m_ShapeType == ShapeTypes.Circle )
					{
						// 円
						if( ( m_BasePosition - frameCenter ).magnitude >  ( m_Frame.Width * 0.5f ) )
						{
							// 範囲外なので無効
							m_PadAxisPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)
							return ;
						}
					}
					else
					{
						// 四角
						Vector2 tp = m_BasePosition - frameCenter ;
						float w = m_Frame.Width  * 0.5f ;
						float h = m_Frame.Height * 0.5f ;

						if( tp.x <  ( - w ) || tp.x >  ( + w ) || tp.y <  ( - h ) || tp.y >  ( + h ) )
						{
							// 範囲外なので無効
							m_PadAxisPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)
							return ;
						}
					}
				}
			}

			//----------------------------------------------------------

			m_PadAxisPointerId = pointer.pointerId ;

			m_Magnitude = 0 ;
			m_Direction = Vector2.zero ;
	
			// OnActivate用の時間計測開始
			m_BaseTime = Time.realtimeSinceStartup ;

			// コールバック呼び出し
			OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
			OnActivateAction?.Invoke( Identity, this, true, m_BasePosition, 0 ) ;
		}

		// Move
		protected override void OnDragBasic( PointerEventData pointerEvent, bool fromScrollView )
		{
			base.OnDragBasic( pointerEvent, fromScrollView ) ;

			if( pointerEvent.pointerId == m_PadAxisPointerId )
			{
				Vector2 position = GetLocalPosition( pointerEvent ) ;

				Vector2 deltaPosition = position - m_BasePosition ;

				if( m_ShapeType == ShapeTypes.Circle )
				{
					// 円
					float limit = ( m_Frame.Width - m_Thumb.Width ) * 0.5f ;

					// 傾き
					float magnitude = deltaPosition.magnitude ;
					if( magnitude >  limit )
					{
						magnitude  = limit ;
					}

					// 方向
					Vector2 direction = deltaPosition.normalized ;

					if( m_HorizontalFunctionStop == true )
					{
						direction.x = 0 ;
					}

					if( m_VerticalFunctionStop == true )
					{
						direction.y = 0 ;
					}

					// 表示位置設定
					m_Thumb.SetPosition( magnitude * direction ) ;

					// 正規化
					magnitude /= limit ;

					if( m_YAxisInversion == true )
					{
						direction.y = - direction.y ;
					}

					// 決定
					m_Magnitude = magnitude ;
					m_Direction = direction ;
				}
				else
				{
					// 四角

					float limitX = ( m_Frame.Width  - m_Thumb.Width  ) * 0.5f ;
					float limitY = ( m_Frame.Height - m_Thumb.Height ) * 0.5f ;

					// 傾き
					float magnitudeX = deltaPosition.x ;
					float magnitudeY = deltaPosition.y ;

					if( magnitudeX >  ( + limitX ) )
					{
						magnitudeX  = ( + limitX ) ;
					}
					if( magnitudeX <  ( - limitX ) )
					{
						magnitudeX  = ( - limitX ) ;
					}
					if( magnitudeY >  ( + limitY ) )
					{
						magnitudeY  = ( + limitY ) ;
					}
					if( magnitudeY <  ( - limitY ) )
					{
						magnitudeY  = ( - limitY ) ;
					}

					// 方向
					Vector2 direction = deltaPosition ;

					if( direction.x <  ( - limitX ) )
					{
						direction.x  = ( - limitX ) ;
					}
					if( direction.x >  ( + limitX ) )
					{
						direction.x  = ( + limitX ) ;
					}
					if( direction.y <  ( - limitY ) )
					{
						direction.y  = ( - limitY ) ;
					}
					if( direction.y >  ( + limitY ) )
					{
						direction.y  = ( + limitY ) ;
					}

					direction.Normalize() ;

					if( m_HorizontalFunctionStop == true )
					{
						direction.x = 0 ;
					}

					if( m_VerticalFunctionStop == true )
					{
						direction.y = 0 ;
					}

					m_Thumb.SetPosition( magnitudeX, magnitudeY ) ;

					// 正規化
					magnitudeX /= limitX ;
					magnitudeY /= limitY ;

					// 0～1.4
					float magnitude = Mathf.Sqrt( magnitudeX * magnitudeX + magnitudeY * magnitudeY ) ;

					if( m_YAxisInversion == true )
					{
						direction.y = - direction.y ;
					}

					// 決定
					m_Magnitude = magnitude ;
					m_Direction = direction ;
				}

				// コールバック呼び出し
				OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
			}
		}

		// Up
		protected override void OnPointerUpBasic( PointerEventData pointerEvent, bool fromScrollView )
		{
			base.OnPointerUpBasic( pointerEvent, fromScrollView ) ;

			if( pointerEvent.pointerId == m_PadAxisPointerId )
			{
				Vector2 movePosition = GetLocalPosition( pointerEvent ) ;

				CallRelease( movePosition ) ;
			}
		}

		// Update
		protected override void OnUpdate()
		{
			if( Application.isPlaying == true )
			{
				// レイキャスト状態の確認
				if( m_SurplusInputEnabled == false )
				{
					CheckRaycast() ;
				}
			}
		}

		// レイキャスト状態の確認
		private void CheckRaycast()
		{
			if( IsPadAvailable() == false )
			{
				// レイキャストがブロックされている
				if( m_PadAxisPointerId != m_UnKnownCode )
				{
					// 離された扱いとする
					CallRelease( Vector2.zero ) ;
				}
			}
		}

		// 離した状態にする
		private void CallRelease( Vector2 movePosition )
		{
			if( m_AlwaysDisplay == false )
			{
				// 押した位置に表示する
				m_Frame.SetActive( false ) ;
			}
			else
			{
				// 常に表示する
				m_Thumb.SetPosition( 0, 0 ) ;
			}

			//----------------------------------

			m_PadAxisPointerId = m_UnKnownCode ;	// +3～-3 あたりが識別子に使われる(-1は使用してはダメ)

			m_Magnitude = 0 ;
			m_Direction = Vector2.zero ;

			// コールバック呼び出し
			OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
			OnActivateAction?.Invoke( Identity, this, false, movePosition, Time.realtimeSinceStartup - m_BaseTime ) ;
		}

		//-----------------------------------------------------------

		private Vector2 m_AxisPrevious = Vector2.zero ;

		/// <summary>
		/// UIPadAdapter からの設定専用メソッド
		/// </summary>
		/// <param name="axis"></param>
		public void SetAxisFromPadAdapter( Vector2 axis )
		{
			if( m_AxisPrevious.x == axis.x && m_AxisPrevious.y == axis.y )
			{
				// 変化なし
				return ;
			}

			m_AxisPrevious.x = axis.x ;
			m_AxisPrevious.y = axis.y ;

			if( axis.x == 0 && axis.y == 0 )
			{
				m_Thumb.SetPosition( Vector2.zero ) ;

				m_Magnitude = 0 ;
				m_Direction = Vector2.zero ;

				// コールバック呼び出し
				OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;

				return ;
			}

			//----------------------------------------------------------

			if( m_ShapeType == ShapeTypes.Circle )
			{
				// 円
				float limit = ( m_Frame.Width - m_Thumb.Width ) * 0.5f ;

				// 傾き
				float magnitude = axis.magnitude ;

				// 方向
				Vector2 direction = axis.normalized ;

				if( m_HorizontalFunctionStop == true )
				{
					direction.x = 0 ;
				}

				if( m_VerticalFunctionStop == true )
				{
					direction.y = 0 ;
				}

				// 表示位置設定
				m_Thumb.SetPosition( limit * magnitude * direction ) ;

				if( m_YAxisInversion == true )
				{
					direction.y = - direction.y ;
				}

				// 決定
				m_Magnitude = magnitude ;
				m_Direction = direction ;
			}
			else
			{
				// 四角

				float limitX = ( m_Frame.Width  - m_Thumb.Width  ) * 0.5f ;
				float limitY = ( m_Frame.Height - m_Thumb.Height ) * 0.5f ;

				// 傾き
				float magnitude = axis.magnitude ;

				// 方向
				Vector2 direction = axis.normalized ;

				if( m_HorizontalFunctionStop == true )
				{
					direction.x = 0 ;
				}

				if( m_VerticalFunctionStop == true )
				{
					direction.y = 0 ;
				}

				m_Thumb.SetPosition( axis.x * limitX, axis.y * limitY ) ;

				if( m_YAxisInversion == true )
				{
					direction.y = - direction.y ;
				}

				// 決定
				m_Magnitude = magnitude ;
				m_Direction = direction ;
			}

			// コールバック呼び出し
			OnValueChangedAction?.Invoke( Identity, this, m_Magnitude, m_Direction ) ;
		}

		//-------------------------------------------------------------------------------------------
		// ゲームパッドのレイキャストヒット確認用

		//-------------------------------------------------------------------------------------------

		// パッドが押せるか確認する
		private readonly PointerEventData		m_PA_EventDataCurrentPosition = new ( EventSystem.current ) ;
		private readonly List<RaycastResult>	m_PA_Results = new () ;

		// バックキーが現在有効な状態か確認する
		private bool IsPadAvailable()
		{
			if( m_View == null || EventSystem.current == null )
			{
				// 使用可能扱いとする
				return true ;
			}

			//------------------------------------------------------------------------------------------

			// スクリーン座標を計算する
			( var padViewPoints, var padViewCenter ) = GetScreenArea( gameObject ) ;

			//----------------------------------

			// 一時的にレイキャストターゲットを有効化する(よって Graphic コンポーネント必須)
			bool raycastTarget = true ;
			if( m_View.RaycastTarget == false )
			{
				raycastTarget = m_View.RaycastTarget ;
				m_View.RaycastTarget = true ;
			}

			//----------------------------------------------------------

			bool isAvailable = false ;

			// レイキャストを実行しヒットする対象を検出する
			m_PA_EventDataCurrentPosition.position = padViewCenter ;
			m_PA_Results.Clear() ;
			EventSystem.current.RaycastAll( m_PA_EventDataCurrentPosition, m_PA_Results ) ;

			// ヒットしない事は基本的にありえない
			foreach( var result in m_PA_Results )
			{
				if( result.gameObject == gameObject )
				{
					// 有効
					isAvailable = true ;
					break ;
				}
				else
				{
					// レイキャストヒット対象がゲームパッド対象ビューそのものでなくても親に含まれていたらスルーする
					if( IsContainParent( gameObject, result.gameObject ) == false )
					{
						// 親では無い
						( var blockerPoints, var blockerCenter ) = GetScreenArea( result.gameObject ) ;
						if( IsCompleteBlocking( padViewPoints, blockerPoints ) == true )
						{
							// 無効
							isAvailable = false ;
							break ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// レイキャスト無効でもヒット判定を有効にしていた場合は設定を元に戻す
			if( raycastTarget == false )
			{
				m_View.RaycastTarget = raycastTarget ;
			}

			//----------------------------------

			// 対象のゲームパッド対応ビューが有効か無効か返す
			return isAvailable ;
		}

		// スクリーン上の矩形範囲を取得する
		private ( Vector2[], Vector2 ) GetScreenArea( GameObject go )
		{
			if( go.TryGetComponent<RectTransform>( out var rt ) == false )
			{
				// 取得出来ない
				throw new Exception( "Not foud rectTransform." ) ;
			}

			//----------------------------------

			// 横幅・縦幅
			float tw = rt.rect.width ;
			float th = rt.rect.height ;

			// レイキャストパディング
			Vector4 raycastPadding = Vector4.zero ;

			if( go.TryGetComponent<UnityEngine.UI.Image>( out var image ) == true )
			{
				raycastPadding = image.raycastPadding ;
			}

			float tx0 = ( tw * ( 0 - rt.pivot.x ) ) + raycastPadding.x ;	// x = left
			float ty0 = ( th * ( 0 - rt.pivot.y ) ) + raycastPadding.y ;	// y = bottom
			float tx1 = ( tw * ( 1 - rt.pivot.x ) ) - raycastPadding.z ;	// z = right
			float ty1 = ( th * ( 1 - rt.pivot.y ) ) - raycastPadding.w ;	// w = top

			// 角の座標(まだローカルの２次元)	※順番は右回りである事に注意(Ｚ型ではない)
			var points = new Vector2[ 4 ]
			{
				new ( tx0, ty0 ),
				new ( tx1, ty0 ),
				new ( tx1, ty1 ),
				new ( tx0, ty1 ),
			} ;

			// ローカル座標をワールド座標に変換(ローカルのローテーションとスケールも反映)
			int i, l = points.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// ローテーションとスケールも考慮するため個別に分ける
				points[ i ] = rt.TransformPoint( points[ i ] ) ;
			}

			//----------------------------------

			Camera targetCamera ;

			var parentCanvas = rt.transform.GetComponentInParent<Canvas>() ;
			if( parentCanvas != null )
			{
				if( parentCanvas.worldCamera != null )
				{
					// Screen Space - Camera
					targetCamera = parentCanvas.worldCamera ;
				}
				else
				{
					// Screen Space - Overlay
					targetCamera = Camera.main ;
				}
			}
			else
			{
				throw new Exception( "Not foud canvas." ) ;
			}

			// スクリーン座標に変換する
			Vector2 center = Vector2.zero ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				points[ i ] = RectTransformUtility.WorldToScreenPoint( targetCamera, points[ i ] ) ;
				center += points[ i ] ;
			}

			center /= l ;

			return ( points, center ) ;
		}

		// ブロッカーの親にバックキーが含まれているかどうか
		private bool IsContainParent( GameObject backKey, GameObject blocker )
		{
			while( blocker.transform.parent != null )
			{
				blocker = blocker.transform.parent.gameObject ;

				if( blocker == backKey )
				{
					// 含まれている
					return true ;
				}
			}

			// 含まれていない
			return false ;
		}

		// バックキーがブロッカーの内側に完全に隠されているか確認する
		private bool IsCompleteBlocking( Vector2[] backKeyPoints, Vector2[] blockerPoints )
		{
			int oi, ol = blockerPoints.Length ;
			int ii, il = backKeyPoints.Length ;

			for( oi = 0 ; oi <  ol ; oi ++ )
			{
				var op0 = blockerPoints[ oi ] ;
				var op1 = blockerPoints[ ( oi + 1 ) % ol ] ;

				for( ii = 0 ; ii <  il ; ii ++ )
				{
					var ip = backKeyPoints[ ii ] ;

					// 外積を用いて表裏判定を行う(Cross)
					var v0 = op1 - op0 ;
					var v1 = ip  - op0 ;
					var cross = ( v0.x * v1.y ) - ( v0.y * v1.x ) ;

					if( cross <  0 )
					{
						// 外側にある
						return false ;
					}
				}
			}

			// 全て内側にある
			return true ;
		}
	}
}
