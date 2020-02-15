using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraControllHelper
{
	/// <summary>
	/// ＦＰＳ的な操作が行えるようにする
	/// </summary>
	public class FpsController : MonoBehaviour
	{
		/// <summary>
		/// 回転対称のトランスフォーム
		/// </summary>
		public Transform	TargetRotation ;

		/// <summary>
		/// 移動対象のトランスフォーム
		/// </summary>
		public Transform	TargetPosition ;

		/// <summary>
		/// 回転速度
		/// </summary>
		public float		RotationSpeed = 0.4f ;

		/// <summary>
		/// 移動を有効にするかどうか
		/// </summary>
		public bool			EnableTranslation = true ;

		/// <summary>
		/// 移動速度
		/// </summary>
		public float		TranslationSpeed = 4.0f ;

		//-----------------------------------------------------

		void Awake()
		{
			if( TargetRotation == null )
			{
				TargetRotation = transform ;
			}

			if( TargetPosition == null )
			{
				TargetPosition = transform ;
			}
		}

		void Update()
		{
			Process() ;	// 操作を処理する
		}

		//-----------------------------------------------------

		// マウスの基準位置
		private Vector3		m_BasePosition ;

		/// <summary>
		/// 操作を処理する
		/// </summary>
		private void Process()
		{
			// ホイールかＷで前進
			// ホイールかＳで後退
			// Ａで左移動
			// Ｄで右移動
			// ポインターで向き変更

			//-------------------------
			// 回転

			if( Input.GetMouseButtonDown( 0 ) == true )
			{
				m_BasePosition	= Input.mousePosition ;
			}
			if( Input.GetMouseButton( 0 ) == true )
			{
				Vector3 deltaPosition = Input.mousePosition - m_BasePosition ;
				m_BasePosition = Input.mousePosition ;

				// 縦回転
				TargetRotation.rotation =
					Quaternion.AngleAxis( - deltaPosition.y * RotationSpeed, TargetRotation.right ) *
					TargetRotation.rotation ;

				// 横回転
				TargetRotation.rotation =
					Quaternion.AngleAxis( deltaPosition.x * RotationSpeed, Vector3.up ) *
					TargetRotation.rotation ;
			}

			//-------------------------
			// 移動

			if( EnableTranslation == true )
			{
				float deltaTime = Time.deltaTime ;

				if( Input.GetKey( KeyCode.W ) == true )
				{
					// 前進
					TargetPosition.position +=
						TargetRotation.forward * deltaTime * TranslationSpeed ;
				}

				if( Input.GetKey( KeyCode.S ) == true )
				{
					// 後退
					TargetPosition.position -=
						TargetRotation.forward * deltaTime * TranslationSpeed ;
				}

				if( Input.GetKey( KeyCode.A ) == true )
				{
					// 左移動
					TargetPosition.position -=
						TargetRotation.right * deltaTime * TranslationSpeed ;
				}

				if( Input.GetKey( KeyCode.D ) == true )
				{
					// 右移動
					TargetPosition.position +=
						TargetRotation.right * deltaTime * TranslationSpeed ;
				}

				if( Input.GetKey( KeyCode.Space ) == true )
				{
					// 上移動
					TargetPosition.position +=
						Vector3.up * deltaTime * TranslationSpeed ;
				}

				if( Input.GetKey( KeyCode.LeftShift ) == true )
				{
					// 下移動
					TargetPosition.position -=
						Vector3.up * deltaTime * TranslationSpeed ;
				}
			}
		}
	}
}

