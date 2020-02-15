using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using uGUIHelper ;

namespace DBS
{
	public class VR
	{
		/// <summary>
		/// 同期させるカメラを設定する
		/// </summary>
		/// <param name="tVRCameraBase"></param>
		/// <returns></returns>
		public static bool ReplaceVRCamera( GameObject tVRCameraBase )
		{
			if( VRDeviceManager.Instance == null )
			{
				return false ;
			}

			return VRDeviceManager.ReplaceVRCamera( tVRCameraBase ) ;
		}

		/// <summary>
		/// 常に同じ位置に表示されるキャンバスを追加する
		/// </summary>
		/// <param name="tCanvas"></param>
		/// <returns></returns>
		public static bool AddAdjustCanvas( UICanvas tCanvas, float tFieldOfView )
		{
			if( VRDeviceManager.Instance == null )
			{
				return false ;
			}

			return VRDeviceManager.AddAdjustCanvas( tCanvas, tFieldOfView ) ;
		}

		/// <summary>
		/// 常に同じ位置に表示されるキャンバスを削除する
		/// </summary>
		/// <param name="tCanvas"></param>
		/// <returns></returns>
		public static bool RemoveAdjustCanvas( UICanvas tCanvas )
		{
			if( VRDeviceManager.Instance == null )
			{
				return false ;
			}

			return VRDeviceManager.RemoveAdjustCanvas( tCanvas ) ;
		}

		//-------------------------------------------------------------------

		// レティクル操作関係

		/// <summary>
		/// レティクルの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticle( bool tVisible, bool tProgress, float tGauge )
		{
			if( VRDeviceManager.Instance == null )
			{
				return false ;
			}

			return VRDeviceManager.SetReticle( tVisible, tProgress, tGauge ) ;
		}

		/// <summary>
		/// レティクルの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticleProgress( bool tProgress, float tGauge )
		{
			if( VRDeviceManager.Instance == null )
			{
				return false ;
			}

			return VRDeviceManager.SetReticleProgress( tProgress, tGauge ) ;
		}

	}
}
