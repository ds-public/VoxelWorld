using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// クライアント(ムーブ)
	/// </summary>
	public partial class WorldClient : MonoBehaviour
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
		private bool m_Focus = false ;

		//---------------------------------------------------------------------------

		/// <summary>
		/// 毎フレームの処理
		/// </summary>
		private void Process()
		{
			// 移動処理を行う
			float fallingY ;
			float jumpingY ;

			if( m_IsJumping == false )
			{
				// 自由落下処理
				if( m_IsFalling == false )
				{
					// 落下開始
					m_FallingTime = Time.deltaTime ;
					m_FallingY = 0 ;
					fallingY = m_FallingSpeed * m_FallingTime * m_FallingTime ;
				}
				else
				{
					// 落下最中
					m_FallingTime += Time.deltaTime ;
					fallingY = m_FallingSpeed * m_FallingTime * m_FallingTime ;
				}
				m_IsFalling = ProcessFalling( fallingY - m_FallingY ) ;
				if( m_IsFalling == true )
				{
					// 落下継続
					m_FallingY = fallingY ;
				}
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
				m_JumpingTime += Time.deltaTime ;
				jumpingY = m_JumpingPower * m_JumpingTime - ( m_FallingSpeed * m_JumpingTime * m_JumpingTime ) ;

				if( jumpingY >  m_JumpingY )
				{
					m_IsJumping = ProcessJumping( jumpingY - m_JumpingY ) ;
				}
				else
				{
					// 落下に転じる
					float t = m_JumpingPower / ( 2.0f * m_FallingSpeed ) ;	// 最高高度の時間
					jumpingY = m_JumpingPower * t - ( m_FallingSpeed * t * t ) ;
					if( jumpingY >  m_JumpingY )
					{
						// 念の為差分判定(0上昇は禁止)
						ProcessJumping( jumpingY - m_JumpingY ) ;
					}

					m_IsJumping = false ;	// 落下中になる
				}
				if( m_IsJumping == true )
				{
					// 上昇継続
					m_JumpingY = jumpingY ;
				}
			}

			// 入力処理を行う
			ProcessInteraction() ;

			//-------------------------------------------------

			// チャンクを更新する
			UpdateChunk() ;

			// オクルージョンカリングで見えないチャンクを非アクティブにする
			OcclusionCulling( m_Camera.transform.position ) ;
		}

		/// <summary>
		/// 入力を処理する
		/// </summary>
		private void ProcessInteraction()
		{
			float deltaTime = Time.deltaTime ;

			//-------------------------
			// 回転

			if( m_Focus == false )
			{
				// フォーカスなし
				if( m_PointerBase.isHover == true )
				{
					if( Input.GetMouseButtonDown( 0 ) == true || Input.GetKeyDown( KeyCode.Return ) == true )
					{
						// フォーカス取得
						m_Focus = true ;
	
						Cursor.visible = false ;
						Cursor.lockState = CursorLockMode.Locked ;
						m_CrossHairPointer.SetActive( true ) ;
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
				Quaternion q1 = Quaternion.AngleAxis( - deltaPosition.y * RotationSpeed, Vector3.right ) *
					m_Camera.transform.localRotation ;

//				Vector3 u0 = q0 * Vector3.up ;
				Vector3 u1 = q1 * Vector3.up ;

				if( u1.y >= 0 )
				{
					// 限界以下までの回転
					m_Camera.transform.localRotation =
						Quaternion.AngleAxis( - deltaPosition.y * RotationSpeed, Vector3.right ) *
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
				m_Player.transform.rotation =
					Quaternion.AngleAxis( deltaPosition.x * RotationSpeed, Vector3.up ) *
					m_Player.transform.rotation ;

				if( Input.GetKeyDown( KeyCode.Escape ) == true )
				{
					// フォーカス解除
					Cursor.visible = true ;
					Cursor.lockState = CursorLockMode.None ;

					m_Focus = false ;
					m_CrossHairPointer.SetActive( false ) ;
				}
			}

			if( m_Focus == true )
			{
				BlockPosition exist ;
				BlockPosition empty ;

				if( GetRaycastTargetBlock( 4.0f, out exist, out empty ) == true )
				{
					m_CrossHairPointer.Color = Color.yellow ;

					Vector3 p = m_Camera.transform.position ;

//					m_Log.text = "ヒット:" + exist.ToString() + " px = " + ( int )p.x + " pz = " + ( int )p.z + " py = " + ( int )p.y ;

					if( Input.GetMouseButtonDown( 0 ) == true )
					{
						// 壊す
						Debug.LogWarning( "------>壊す:" + exist.ToString() ) ;
						SetBlock( exist.X, exist.Z, exist.Y, 0 ) ;
						m_WorldServer.SetBlock( exist.X, exist.Z, exist.Y, 0 ) ;
					}
					if( Input.GetMouseButtonDown( 1 ) == true )
					{
						// 作る

						if( IsCreateBlock( empty.X, empty.Z, empty.Y ) == true )
						{
							if( GetBlock( empty.X, empty.Z, empty.Y ) == 0 )
							{
								short blockIndex = ( short )( SelectedBlockIndex ) ;

								Debug.LogWarning( "------>作る:" + empty.ToString() ) ;
								SetBlock( empty.X, empty.Z, empty.Y, blockIndex ) ;
								m_WorldServer.SetBlock( empty.X, empty.Z, empty.Y, blockIndex ) ;
							}
						}
					}
				}
				else
				{
					m_CrossHairPointer.Color = Color.white ;

					m_Log.Text = "" ;
				}
			}

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
				l = m_ItemShortCuts.Length ;
				if( wheel <  0 )
				{
					i = ( m_ItemShortCutIndex + 1 + l ) % l ;
					OnSelectedBlockIndex( i ) ;
				}
				else
				if( wheel >  0 )
				{
					i = ( m_ItemShortCutIndex - 1 + l ) % l ;
					OnSelectedBlockIndex( i ) ;
				}
			}

			//-------------------------
			// 移動

			Vector2 ahead = new Vector2( m_Player.forward.x, m_Player.forward.z ) ;
			ahead.Normalize() ;

			Vector2 shift = new Vector2( m_Player.right.x, m_Player.right.z ) ;
			shift.Normalize() ;

			if( Input.GetKey( KeyCode.W ) == true )
			{
				// 前進
				ProcessMoving(   ahead * deltaTime * TranslationSpeed ) ;
			}
			else
			if( Input.GetKey( KeyCode.S ) == true )
			{
				// 後退
				ProcessMoving( -  ahead * deltaTime * TranslationSpeed ) ;
			}
			else
			if( Input.GetKey( KeyCode.A ) == true )
			{
				// 左移動
				ProcessMoving( - shift * deltaTime * TranslationSpeed ) ;
			}
			else
			if( Input.GetKey( KeyCode.D ) == true )
			{
				// 右移動
				ProcessMoving(   shift * deltaTime * TranslationSpeed ) ;
			}

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