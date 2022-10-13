using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using InputHelper ;

using MathHelper ;

namespace DBS.World
{
	/// <summary>
	/// クライアント(ムーブ)
	/// </summary>
	public partial class WorldClient
	{
		private bool			m_IsJumping = false ;
		private readonly float	m_JumpingPower = 6.4f ;
		private float			m_JumpingTime = 0 ;
		private float			m_JumpingY = 0 ;

		private bool			m_IsFalling = false ;
		private readonly float	m_FallingSpeed = 8 ;
		private float			m_FallingTime = 0 ;
		private float			m_FallingY = 0 ;

		// フォーカス
		private bool			m_Focus = false ;
		private bool			m_IgnoreTouch ;


		//---------------------------------------------------------------------------

		// 移動(上昇と下降)を処理する
		private void ProcessJumpingAndFalling()
		{
			//----------------------------------------------------------
			// 垂直方法の移動

			float deltaTime = 0.01666f ;	// ６０フレーム基準

			float jumpingY ;
			float fallingY ;

			// 突き抜け防止のため移動量を細かく分ける(６０フレームで１回・３０フレームで２回)
			int slice = ( int )( 60.0f / Application.targetFrameRate ) ;

			int times ;
			Vector3 afterPosition ;
			for( times  = 0 ; times <  slice ; times ++ )
			{
				if( m_IsJumping == false )
				{
					// 自由落下処理
					if( m_IsFalling == false )
					{
						// 落下開始
						m_FallingTime = deltaTime ;
						m_FallingY = 0 ;
						fallingY = m_FallingSpeed * m_FallingTime * m_FallingTime ;
					}
					else
					{
						// 落下最中
						m_FallingTime += deltaTime ;
						fallingY = m_FallingSpeed * m_FallingTime * m_FallingTime ;
					}
					( m_IsFalling, afterPosition ) = ProcessFalling( fallingY - m_FallingY, m_PlayerActor.Position, false ) ;
					if( m_IsFalling == true )
					{
						// 落下継続
//						Debug.Log( "<color=#FF7F00>落下すると判定された " + m_PlayerActor.Position.z + "</color>" ) ;
						m_FallingY = fallingY ;
					}
					m_PlayerActor.Position = afterPosition ;
				}

				if( m_IsFalling == false && m_IsJumping == false )
				{
					// 落下中でなければジャンプが可能
					if( Input.GetKeyDown( KeyCode.Space ) == true )
					{
						m_IsJumping = true ;
						m_JumpingTime = 0 ;
						m_JumpingY = 0 ;
					}
				}

				if( m_IsJumping == true )
				{
					// 上昇中
					m_JumpingTime += deltaTime ;
					jumpingY = m_JumpingPower * m_JumpingTime - ( m_FallingSpeed * m_JumpingTime * m_JumpingTime ) ;

					if( jumpingY >  m_JumpingY )
					{
						( m_IsJumping, afterPosition ) = ProcessJumping( jumpingY - m_JumpingY, m_PlayerActor.Position ) ;
						m_PlayerActor.Position = afterPosition ;
					}
					else
					{
						// 落下に転じる
						float t = m_JumpingPower / ( 2.0f * m_FallingSpeed ) ;	// 最高高度の時間
						jumpingY = m_JumpingPower * t - ( m_FallingSpeed * t * t ) ;
						if( jumpingY >  m_JumpingY )
						{
							// 念の為差分判定(0上昇は禁止)
							( m_IsJumping, afterPosition ) = ProcessJumping( jumpingY - m_JumpingY, m_PlayerActor.Position ) ;
							m_PlayerActor.Position = afterPosition ;
						}

						m_IsJumping = false ;	// 落下中になる
					}
					if( m_IsJumping == true )
					{
						// 上昇継続
						m_JumpingY = jumpingY ;
					}
				}
			}
		}

		// 入力を処理する
		private void ProcessInteraction()
		{
			if( Input.GetKeyDown( KeyCode.F5 ) == true )
			{
				// プレイヤーの視点の種別を切り替える
				switch( m_PlayerViewType )
				{
					case PlayerViewTypes.FirstPerson		: m_PlayerViewType = PlayerViewTypes.ThirdPerson_Normal	; break ;
					case PlayerViewTypes.ThirdPerson_Normal	: m_PlayerViewType = PlayerViewTypes.ThirdPerson_Invert	; break ;
					case PlayerViewTypes.ThirdPerson_Invert	: m_PlayerViewType = PlayerViewTypes.FirstPerson		; break ;
				}
			}

			m_IsPlayerSneaking = false ;
			if( Input.GetKey( KeyCode.LeftShift ) == true )
			{
				// スニーク有効
				m_IsPlayerSneaking = true ;
			}

			// プレイヤーアクターの状態を視点の種別に応じて切り替える
			UpdatePlayerActor() ;

			//-------------------------
			// フォーカスに関係なくエンターキーでログを表示する

			if( Input.GetKeyDown( KeyCode.Return ) == true )
			{
				m_LogDisplayKeepTime = 10 ;	// 表示を維持する時間
				m_Log.Alpha = 1 ;
			}

			//-------------------------
			// カメラの回転(移動方向設定)

			if( m_Focus == false )
			{
				// フォーカスなし
				if( m_PointerBase.IsHover == true )
				{
					if( Input.GetMouseButtonDown( 0 ) == true || Input.GetKeyDown( KeyCode.Return ) == true )
					{
						// フォーカス取得
						m_Focus = true ;
	
						Cursor.visible = false ;
						Cursor.lockState = CursorLockMode.Locked ;

						// プレイングレイヤー表示
						m_PlayingLayer.SetActive( true ) ;

						// ポージングレイヤー隠蔽
						m_PausingLayer.SetActive( false ) ;
						m_GuideMessage.StopAndResetAllTweens() ;

						m_IgnoreTouch = true ;	// 一度だけ入力を無効化する

						// フォーカスを得たフレームではプレイヤーインタラクションを処理しない
						return ;
					}
				}
			}
			else
			{
				// フォーカスあり

				// フレーム単位の移動量(カーソルをロックしても取得可能)
				Vector2 deltaPosition = new Vector2( Input.GetAxisRaw( "Mouse X" ), Input.GetAxisRaw( "Mouse Y" ) ) ;

				// 縦回転(上下には限界量を設定する)

//				Quaternion q0 = m_Camera.transform.localRotation ;
				Quaternion q1 = Quaternion.AngleAxis( - deltaPosition.y * m_RotationSpeed, Vector3.right ) *
					m_Camera.transform.localRotation ;

//				Vector3 u0 = q0 * Vector3.up ;
				Vector3 u1 = q1 * Vector3.up ;

				if( u1.y >= 0 )
				{
					// 限界以下までの回転
					m_Camera.transform.localRotation =
						Quaternion.AngleAxis( - deltaPosition.y * m_RotationSpeed, Vector3.right ) *
						m_Camera.transform.localRotation ;
				}
				else
				{
					// 回転量を限界までの量に抑える

					// 現在の方向ベクトル
					Vector3 f0 = m_Camera.transform.localRotation * Vector3.forward ;

					Vector3 f1 = q1 * Vector3.forward ;
					if( f1.y >  0 )
					{
						// 上
						f1 =   Vector3.up ;
					}
					else
					{
						// 下
						f1 = - Vector3.up ;
					}

					q1 = Quaternion.FromToRotation( f0, f1 ) ;

					m_Camera.transform.localRotation =
						q1 *
						m_Camera.transform.localRotation ;
				}

				// 横回転
				m_PlayerActor.Rotation =
					Quaternion.AngleAxis( deltaPosition.x * m_RotationSpeed, Vector3.up ) *
					m_PlayerActor.transform.rotation ;

				// コンパスの表示を更新する
				UpdateCompas() ;

				if( Input.GetKeyDown( KeyCode.Escape ) == true )
				{
					// フォーカス解除
					Cursor.visible = true ;
					Cursor.lockState = CursorLockMode.None ;

					m_Focus = false ;

					// プレイングレイヤー隠蔽
					m_PlayingLayer.SetActive( false ) ;

					// ポージングレイヤー表示
					m_PausingLayer.SetActive( true ) ;
					_ = m_GuideMessage.PlayTween( "Move" ) ;

					// タッチエフェクト無効化
					Ripple.Off() ;
				}
			}

			if( m_Focus == false )
			{
				return ;
			}

			//------------------------------------------------------------------------------------------
			// 以下はフォーカスを得ている場合のみ処理する

			if( m_IgnoreTouch == true )
			{
				// タッチエフェクト有効化
				Ripple.On() ;

				// フォーカスを得た直後は一度だけタッチを無効化する
				m_IgnoreTouch  = false ;
				return ;
			}

			//---------------------------------

			BlockPosition exist ;
			BlockPosition empty ;

			if( GetRaycastTargetBlock( 4.0f, out exist, out empty ) == true )
			{
				m_CrossHairPointer.Color = Color.yellow ;

				Vector3 p = m_Camera.transform.position ;

//				m_Log.text = "ヒット:" + exist.ToString() + " px = " + ( int )p.x + " pz = " + ( int )p.z + " py = " + ( int )p.y ;

				if( Input.GetMouseButtonDown( 0 ) == true )
				{
					// 壊す
					SE.Play( SE.Broken ) ;

//					Debug.Log( "------>壊す:" + exist.ToString() ) ;

					// クライアント側の情報を更新しメッシュを作り直す
					if( SetBlock( exist.X, exist.Z, exist.Y, 0 ) == true )
					{
						//----------------------------------------------------------
						// サーバーに変更リクエストを送る

						WS_Send_Request_SetWorldBlock( ( short )exist.X, ( short )exist.Z, ( short )exist.Y, 0 ) ;
					}
				}
				if( Input.GetMouseButtonDown( 1 ) == true )
				{
					// 作る

					if( IsCreateBlock( empty.X, empty.Z, empty.Y ) == true )
					{
						if( GetBlock( empty.X, empty.Z, empty.Y ) == 0 )
						{
							short blockIndex = ( short )( m_SelectedBlockIndex ) ;

							SE.Play( SE.Recover ) ;

//							Debug.Log( "------>作る:" + empty.ToString() ) ;

							// クライアント側の情報を更新しメッシュを作り直す
							if( SetBlock( empty.X, empty.Z, empty.Y, blockIndex ) == true )
							{
								//----------------------------------------------------------
								// サーバーに変更リクエストを送る

								WS_Send_Request_SetWorldBlock( ( short )empty.X, ( short )empty.Z, ( short )empty.Y, blockIndex ) ;
							}
						}
					}
				}
			}
			else
			{
				m_CrossHairPointer.Color = Color.white ;
			}

			//-----------------------------------------------------------------------------------------
			// アイテム選択(ブロック選択)

			KeyCode[] itemShortCutKeyCodes =
			{
				KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
			} ;

			int i, l = itemShortCutKeyCodes.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Input.GetKeyDown( itemShortCutKeyCodes[ i ] ) == true )
				{
					OnSelectedBlockIndex( i ) ;
				}
			}

			float wheel = Input.GetAxis( "Mouse ScrollWheel" ) ;
			if( wheel != 0 )
			{
				l = m_ActiveItemSlots.Length ;
				if( wheel <  0 )
				{
					i = ( m_ActiveItemSlotIndex + 1 + l ) % l ;
					OnSelectedBlockIndex( i ) ;
				}
				else
				if( wheel >  0 )
				{
					i = ( m_ActiveItemSlotIndex - 1 + l ) % l ;
					OnSelectedBlockIndex( i ) ;
				}
			}

			//------------------------------------------------------------------------------------------
			// 水平方向の移動

			Vector2 ahead = new Vector2( m_PlayerActor.Forward.x, m_PlayerActor.Forward.z ) ;
			ahead.Normalize() ;

			Vector2 shift = new Vector2( m_PlayerActor.Right.x, m_PlayerActor.Right.z ) ;
			shift.Normalize() ;

			// ６０フレーム時の移動量が最小単位
			float velocity = m_TranslationSpeed / 60.0f ;

			// 突き抜け防止のため移動量を細かく分ける(６０フレームで１回・３０フレームで２回)
			int slice = ( int )( 60.0f / Application.targetFrameRate ) ;

			int times ;
			if( Input.GetKey( KeyCode.W ) == true )
			{
				// 前進
				for( times  = 0 ; times <  slice ; times ++ )
				{
					if( ProcessMoving_Slice(   ( velocity * ahead ), m_IsPlayerSneaking ) == false )
					{
						break ;
					}
				}
			}
			else
			if( Input.GetKey( KeyCode.S ) == true )
			{
				// 後退
				for( times  = 0 ; times <  slice ; times ++ )
				{
					if( ProcessMoving_Slice( - ( velocity * ahead ), m_IsPlayerSneaking ) == false )
					{
						break ;
					}
				}
			}
			else
			if( Input.GetKey( KeyCode.A ) == true )
			{
				// 左移動
				for( times  = 0 ; times <  slice ; times ++ )
				{
					if( ProcessMoving_Slice( - ( velocity * shift ), m_IsPlayerSneaking ) == false )
					{
						break ;
					}
				}
			}
			else
			if( Input.GetKey( KeyCode.D ) == true )
			{
				// 右移動
				for( times  = 0 ; times <  slice ; times ++ )
				{
					if( ProcessMoving_Slice(   ( velocity * shift ), m_IsPlayerSneaking ) == false )
					{
						break ;
					}
				}
			}

//			Debug.Log( "<color=#FFFF00>フレームエンド pz = " + m_PlayerActor.Position.z + "</color>" ) ;

/*			if( Input.GetKey( KeyCode.Space ) == true )
			{
				// 上移動
				m_Player.position +=
					Vector3.up * deltaTime * TranslationSpeed ;
			}

			if( Input.GetKey( KeyCode.LeftShift ) == true )
			{
				// 下移動
				m_Player.position -=
					Vector3.up * deltaTime * TranslationSpeed ;
			}*/

		}

	}
}
