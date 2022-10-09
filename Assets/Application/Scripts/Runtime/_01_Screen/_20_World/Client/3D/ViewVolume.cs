using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

namespace DBS.World
{
	public class ViewVolume
	{
		public bool Ready = false ;

		public class Surface
		{
			public Vector3	Center ;	// 基準点
			public Vector3	Normal ;	// 法線
		}

		public Surface[] Surfaces = new Surface[]
		{
			new Surface(),	// 左面
			new Surface(),	// 右面
			new Surface(),	// 下面
			new Surface(),	// 上面
			new Surface(),	// 近面
			new Surface(),	// 遠面
		} ;

		/// <summary>
		/// 視錐台をセットアップする
		/// </summary>
		/// <param name="fpsCamera"></param>
		/// <param name="cameraPosition"></param>
		public void Setup( Camera fpsCamera, Vector3 cameraPosition )
		{
			// 垂直方向の視野角
			float vFoV = Mathf.PI * fpsCamera.fieldOfView / 360.0f ;	// Mathf はラジアン系

			// ニアクリップ
			float near = fpsCamera.nearClipPlane ;

			// ファークリップ
			float far = fpsCamera.farClipPlane ;

			// 画面サイズ
			Vector2 gameWindowSize = GetGameWindowSize() ;
			float viewWidth = gameWindowSize.x ;
			float viewHeight = gameWindowSize.y ;

			// ディスプレイ(ニアクリップ)から焦点までの距離を求める
			float n = viewHeight / Mathf.Tan( vFoV ) ;	// ディスプレイまでの距離(ニアクリップへの距離ではない事に注意)

			// 水平方向の視野角
			float hFoV = Mathf.Atan( viewWidth / n ) ;	// 半分

			//----------------------------------

			// 初期姿勢の場合の視錐台の４ベクトルを求める

			float ht = Mathf.Tan( hFoV ) ;
			float vt = Mathf.Tan( vFoV ) ;

			// 視錐台の８点から６面の基準点と法線ベクトルを求める
			// 順番は -X +X -Y +Y -Z +Z

			// -X(左面)
			Surfaces[ 0 ].Center = Vector3.zero ;
			Surfaces[ 0 ].Normal = new Vector3( -1, 0, - ht ) ;
			Surfaces[ 0 ].Normal.Normalize() ;

			// +X(右面)
			Surfaces[ 1 ].Center = Vector3.zero ;
			Surfaces[ 1 ].Normal = new Vector3(  1, 0, - ht ) ;
			Surfaces[ 1 ].Normal.Normalize() ;

			// -Y(下面)
			Surfaces[ 2 ].Center = Vector3.zero ;
			Surfaces[ 2 ].Normal = new Vector3( 0, -1, - vt ) ;
			Surfaces[ 2 ].Normal.Normalize() ;

			// +Y(上面)
			Surfaces[ 3 ].Center = Vector3.zero ;
			Surfaces[ 3 ].Normal = new Vector3( 0,  1, - vt ) ;
			Surfaces[ 3 ].Normal.Normalize() ;

			// -Z(近面)
			Surfaces[ 4 ].Center = new Vector3( 0, 0, near ) ;
			Surfaces[ 4 ].Normal = new Vector3( 0, 0, -1 ) ;

			// +Z(遠面)
			Surfaces[ 5 ].Center = new Vector3( 0, 0, far ) ;
			Surfaces[ 5 ].Normal = new Vector3( 0, 0,  1 ) ;

			//----------------------------------

			// 回転量を取得
			Quaternion q = Quaternion.LookRotation( fpsCamera.transform.forward, fpsCamera.transform.up ) ;

			// 回転と平行移動を反映
			int i, l = 6 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Surfaces[ i ].Center = ( q * Surfaces[ i ].Center ) + cameraPosition ;
				Surfaces[ i ].Normal = q * Surfaces[ i ].Normal ;
			}

			Ready = true ;
		}

		/// <summary>
		/// ゲームウィンドウのサイズを取得する
		/// </summary>
		/// <returns></returns>
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

		public bool IsVisible( Vector3[] boundingBox )
		{
			// チャンクの８点
			// 内側の条件：点のうち１つでもいずれかの面の内側にある

			int vi, si ;
			Vector3 distance ;

			for( si  = 0 ; si <  6 ; si ++ )
			{
				for( vi  = 0 ; vi <  8 ; vi ++ )
				{
					distance = boundingBox[ vi ] - Surfaces[ si ].Center ;
					if( Vector3.Dot( distance, Surfaces[ si ].Normal ) <  0 )
					{
						// 内側にある
						break ;
					}
				}

				if( vi >= 8 )
				{
					// 全ての点が特定面の完全外側
					return false ;
				}
			}

			// 内側にある
			return true ;
		}
	}
}

