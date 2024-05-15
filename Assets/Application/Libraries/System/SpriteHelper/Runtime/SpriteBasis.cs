using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2024/05/14
	/// </summary>
//	[ExecuteAlways]
	[DisallowMultipleComponent]
	public partial class SpriteBasis : MonoBehaviour
	{
		//-------------------------------------------------------------------------------------------

		// 属しているスプライトスクリーン
		private SpriteScreen	m_CachedSpriteScreen ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ヒエラルキーでの階層パス名を取得する
		/// </summary>
		public string Path
		{
			get
			{
				string path = name ;

				var t = transform.parent ;
				while( t != null )
				{
					path = $"{t.name}/{path}" ;
					t = t.parent ;
				}
				return path ;
			}
		}

		/// <summary>
		/// Component を追加する(ショートカット)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T : UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アクティブ状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// アクティブ状態
		/// </summary>
		public bool ActiveSelf
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		/// <summary>
		/// スプライトスクリーンを取得する
		/// </summary>
		/// <returns></returns>
		public SpriteScreen GetSpriteScreen()
		{
			if( m_CachedSpriteScreen == null )
			{
				m_CachedSpriteScreen = transform.GetComponentInParent<SpriteScreen>() ;
				if( m_CachedSpriteScreen == null )
				{
					Debug.LogWarning( $"Not found SpriteScreen component. [{Path}]" ) ;
					return null ;
				}
			}

			return m_CachedSpriteScreen ;
		}

		/// <summary>
		/// 扱う準備が整っているかどうか
		/// </summary>
		public bool IsReady
		{
			get
			{
				return GetSpriteScreen() != null ;
			}
		}

		/// <summary>
		/// スプライトスクリーンのサイズ(解像度)を取得する
		/// </summary>
		/// <returns></returns>
		public Vector2 GetScreenSize()
		{
			if( m_CachedSpriteScreen == null )
			{
				m_CachedSpriteScreen = GetSpriteScreen() ;
				if( m_CachedSpriteScreen == null )
				{
					return Vector2.zero ;
				}
			}

			return m_CachedSpriteScreen.GetSize() ;
		}

		/// <summary>
		/// スプライトスクリーンのサイズ(解像度)
		/// </summary>
		public  Vector2		ScreenSize => GetScreenSize() ;

		/// <summary>
		/// インスタンスを追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prefab"></param>
		/// <returns></returns>
		public T AddPrefab<T>( GameObject prefab ) where T : UnityEngine.Component
		{
			var go = Instantiate( prefab ) ;
			go.name = prefab.name + "(Clone)" ;

			go.transform.SetParent( transform, false ) ;

			return go.GetComponent<T>() ;			
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetPosition( float x, float y )
		{
			transform.localPosition = new Vector3( x, y, transform.localPosition.z ) ;
		}

		/// <summary>
		/// 位置Ｘを設定する
		/// </summary>
		/// <param name="x"></param>
		public void SetPositionX( float x )
		{
			transform.localPosition = new Vector3( x, transform.localPosition.y, transform.localPosition.z ) ;
		}

		/// <summary>
		/// 位置Ｙを設定する
		/// </summary>
		/// <param name="y"></param>
		public void SetPositionY( float y )
		{
			transform.localPosition = new Vector3( transform.localPosition.x, y, transform.localPosition.z ) ;
		}

		/// <summary>
		/// 位置Ｚを設定する
		/// </summary>
		/// <param name="z"></param>
		public void SetPositionZ( float z )
		{
			transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, z ) ;
		}


		/// <summary>
		/// 位置
		/// </summary>
		public Vector2 Position
		{
			get
			{
				return new Vector2( transform.localPosition.x, transform.localPosition.y ) ;
			}
			set
			{
				transform.localPosition = new Vector3( value.x, value.y, transform.localPosition.z ) ;
			}
		}

		/// <summary>
		/// Ｘ座標
		/// </summary>
		public float PositionX
		{
			get
			{
				return transform.localPosition.x ;
			}
			set
			{
				transform.localPosition = new Vector3( value, transform.localPosition.y, transform.localPosition.z ) ;
			}
		}

		/// <summary>
		/// Ｙ座標
		/// </summary>
		public float PositionY
		{
			get
			{
				return transform.localPosition.y ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, value, transform.localPosition.z ) ;
			}
		}

		/// <summary>
		/// Ｚ座標
		/// </summary>
		public float PositionZ
		{
			get
			{
				return transform.localPosition.z ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, value ) ;
			}
		}


		/// <summary>
		/// Ｘ軸回転
		/// </summary>
		public float RotationX
		{
			get
			{
				return transform.localRotation.eulerAngles.x ;
			}
			set
			{
				var euler = transform.localRotation.eulerAngles ;

				transform.localRotation = Quaternion.Euler( value, euler.y, euler.z ) ;
			}
		}

		/// <summary>
		/// Ｙ軸回転
		/// </summary>
		public float RotationY
		{
			get
			{
				return transform.localRotation.eulerAngles.y ;
			}
			set
			{
				var euler = transform.localRotation.eulerAngles ;

				transform.localRotation = Quaternion.Euler( euler.x, value, euler.z ) ;
			}
		}

		/// <summary>
		/// Ｚ軸回転
		/// </summary>
		public float RotationZ
		{
			get
			{
				return transform.localRotation.eulerAngles.z ;
			}
			set
			{
				var euler = transform.localRotation.eulerAngles ;

				transform.localRotation = Quaternion.Euler( euler.x, euler.y, value ) ;
			}
		}


		/// <summary>
		/// 縮尺
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return transform.localScale ;
			}
			set
			{
				transform.localScale = new Vector3( value.x, value.y, 1 ) ;
			}
		}


		//-------------------------------------------------------------------------------------------
	}
}
