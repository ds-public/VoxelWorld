using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using TransformHelper ;

namespace DBS.World
{
	public class PlayerActor : ExMonoBehaviour
	{
		[SerializeField]
		protected Camera		m_Camera ;

		[SerializeField]
		protected Transform		m_Figure ;

		[SerializeField]
		protected SoftMesh		m_Body ;

		[SerializeField]
		protected SoftMesh		m_Head ;

		[SerializeField]
		protected Transform		m_NamePlateBase ;

		[SerializeField]
		protected SoftMesh[]	m_Arms = new SoftMesh[ 2 ] ;


		//-----------------------------------------------------------

		/// <summary>
		/// アクティブ状態を切り替える
		/// </summary>
		/// <param name="state"></param>
		public void SetActive( bool state )
		{
			gameObject.SetActive( state ) ;
		}

		private SoftTransform	m_ActorTransform ;

		/// <summary>
		/// トランスフォーム
		/// </summary>
		public  SoftTransform	  ActorTransform
		{
			get
			{
				if( m_ActorTransform == null )
				{
					m_ActorTransform  = GetComponent<SoftTransform>() ;
				}
				return m_ActorTransform ;
			}
		}


		/// <summary>
		/// 位置
		/// </summary>
		public Vector3 Position
		{
			get
			{
				return ActorTransform.Position ;
			}
			set
			{
				ActorTransform.Position = value ;
			}
		}

		// 方向
		public Quaternion Rotation
		{
			get
			{
				return ActorTransform.transform.rotation ;
			}
			set
			{
				ActorTransform.transform.rotation = value ;
			}
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public void SetPosition( float x, float y, float z )
		{
			ActorTransform.SetPosition( x, y, z ) ;
		}

		/// <summary>
		/// 位置を設定する
		/// </summary>
		/// <param name="position"></param>
		public void SetPosition( Vector3 position )
		{
			ActorTransform.SetPosition( position ) ;
		}

		/// <summary>
		/// 方向を設定する
		/// </summary>
		/// <param name="direction"></param>
		public void SetDirection( Vector3 direction )
		{
			Vector3 forward = new Vector3( direction.x, 0, direction.z ) ;
			forward.Normalize() ;

			ActorTransform.Forward = forward ;

			float y = direction.y ;
			float z = Mathf.Sqrt( 1 - ( y * y ) ) ;
			Vector3 f = new Vector3(  0,  y,  z ) ;
			Vector3 u = new Vector3(  0,  z, -y ) ; 

			m_Head.transform.localRotation = Quaternion.LookRotation( f, u ) ;
		}

		/// <summary>
		/// 方向
		/// </summary>
		public Vector3 Direction
		{
			get
			{
				return ActorTransform.Forward ;
			}
			set
			{
				ActorTransform.Forward = value ;
			}
		}

		/// <summary>
		/// カメラを取得する
		/// </summary>
		/// <returns></returns>
		public Camera GetCamera()
		{
			return m_Camera ;
		}

		/// <summary>
		/// カメラの方向を取得する
		/// </summary>
		/// <returns></returns>
		public Vector3 GetCameraDirection()
		{
			return m_Camera.transform.forward ;
		}

		/// <summary>
		/// 方向
		/// </summary>
		public Vector3 Forward
		{
			get
			{
				return ActorTransform.Forward ;
			}
			set
			{
				ActorTransform.Forward = value ;
			}
		}

		/// <summary>
		/// 方向
		/// </summary>
		public Vector3 Up
		{
			get
			{
				return ActorTransform.Up ;
			}
			set
			{
				ActorTransform.Up = value ;
			}
		}

		public Vector3 Right
		{
			get
			{
				return ActorTransform.Right ;
			}
			set
			{
				ActorTransform.Right = value ;
			}
		}

		/// <summary>
		/// ネームプレートの３Ｄ空間上での表示位置
		/// </summary>
		public Transform NamePlateBase
		{
			get
			{
				return m_NamePlateBase ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// カメラの状態を設定する
		/// </summary>
		/// <param name="state"></param>
		public void SetCameraEnabled( bool state )
		{
			m_Camera.gameObject.SetActive( state ) ;
		}

		/// <summary>
		/// 姿を表示する
		/// </summary>
		public void ShowFigure()
		{
			m_Figure.gameObject.SetActive( true ) ;
		}

		/// <summary>
		/// 姿を隠蔽する
		/// </summary>
		public void HideFigure()
		{
			m_Figure.gameObject.SetActive( false ) ;
		}

		/// <summary>
		/// 見た目の色を設定する
		/// </summary>
		/// <param name="colorType"></param>
		public void SetColorType( byte colorType )
		{
			Color color = WorldSettings.PlayerActorColors[ colorType ] ;

			m_Body.VertexColor = color ;
			m_Head.VertexColor = color ;

			int i, l = m_Arms.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Arms[ i ].VertexColor = color ;
			}

		}
	}
}
