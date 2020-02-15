using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViewVolumeHelper
{
	public class ViewVolumeGizmo : MonoBehaviour
	{
		[SerializeField]
		private Camera	m_Camera = null ;

		void OnDrawGizmos()
		{
			Camera targetCamera = m_Camera ;
			if( targetCamera == null )
			{
				targetCamera = GetComponentInChildren<Camera>() ;
				if( targetCamera == null )
				{
					return ;
				}
			}

			//-------------------------

			// 垂直方向の視野角
			float vFoV = Mathf.PI * targetCamera.fieldOfView / 360.0f ;	// Mathf はラジアン系

			// ニアクリップ
			float near = targetCamera.nearClipPlane ;

			// ファークリップ
			float far = targetCamera.farClipPlane ;

			// 画面サイズ
			Vector2 gameWindowSize = GetGameWindowSize() ;
			float viewWidth = gameWindowSize.x ;
			float viewHeight = gameWindowSize.y ;

			// ディスプレイ(ニアクリップ)から焦点までの距離を求める
			float n = viewHeight / Mathf.Tan( vFoV ) ;	// ディスプレイまでの距離(ニアクリップへの距離ではない事に注意)

			// 水平方向の視野角
			float hFoV = Mathf.Atan( viewWidth / n ) ;

			float nvx = near * Mathf.Tan( hFoV ) ;
			float nvy = near * Mathf.Tan( vFoV ) ;

			float fvx = far * Mathf.Tan( hFoV ) ;
			float fvy = far * Mathf.Tan( vFoV ) ;

			Vector3[] v = new Vector3[]
			{
				new Vector3( - nvx, - nvy, near ),
				new Vector3(   nvx, - nvy, near ),
				new Vector3( - nvx,   nvy, near ),
				new Vector3(   nvx,   nvy, near ),

				new Vector3( - fvx, - fvy, far ),
				new Vector3(   fvx, - fvy, far ),
				new Vector3( - fvx,   fvy, far ),
				new Vector3(   fvx,   fvy, far ),
			} ;

			Quaternion q = Quaternion.LookRotation( targetCamera.transform.forward, targetCamera.transform.up ) ;

			int i, l = 8 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				v[ i ] = ( q * v[ i ] ) + targetCamera.transform.position ;
			}

			//-------------------------

			Gizmos.color = Color.yellow ;

			Gizmos.DrawLine( v[ 0 ], v[ 1 ] ) ;
			Gizmos.DrawLine( v[ 1 ], v[ 3 ] ) ;
			Gizmos.DrawLine( v[ 3 ], v[ 2 ] ) ;
			Gizmos.DrawLine( v[ 2 ], v[ 0 ] ) ;

			Gizmos.DrawLine( v[ 4 ], v[ 5 ] ) ;
			Gizmos.DrawLine( v[ 5 ], v[ 7 ] ) ;
			Gizmos.DrawLine( v[ 7 ], v[ 6 ] ) ;
			Gizmos.DrawLine( v[ 6 ], v[ 4 ] ) ;

			Gizmos.DrawLine( v[ 0 ], v[ 4 ] ) ;
			Gizmos.DrawLine( v[ 1 ], v[ 5 ] ) ;
			Gizmos.DrawLine( v[ 2 ], v[ 6 ] ) ;
			Gizmos.DrawLine( v[ 3 ], v[ 7 ] ) ;
		}

		public Vector2 GetGameWindowSize()
		{
#if UNITY_EDITOR
			System.Type T = System.Type.GetType( "UnityEditor.GameView,UnityEditor" ) ;
			System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod( "GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static ) ;
			System.Object gameWindowSize = GetSizeOfMainGameView.Invoke( null,null ) ;
			return ( Vector2 )gameWindowSize ;
#else
			return new Vector2( Screen.width, Screen.height ) ;
#endif
		}
	}
}

