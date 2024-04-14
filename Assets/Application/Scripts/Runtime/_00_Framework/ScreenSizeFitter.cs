using System ;
using UnityEngine ;
using UnityEngine.EventSystems ;

using ScreenSizeHelper ;


namespace DSW
{
	/// <summary>
	/// スクリーンのサイズ調整クラス Version 2023/12/17 0
	/// </summary>
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent( typeof( RectTransform ) )]
	[DefaultExecutionOrder( -5 )]	// 通常よりも早く Update をコールさせる(ただし SafeAreaCorrector より後)
	public class ScreenSizeFitter : ScreenSizeFitterBase
	{
		// Awake のタイミングで呼び出される
		protected override void OnAwake()
		{
			base.OnAwake() ;

			//----------------------------------
			// 解像度を設定する

			// 基準解像度
			float basicWidth  = 1080 ;
			float basicHeight = 1920 ;

			// 最大解像度
			float limitWidth  = 1440 ;
			float limitHeight = 2560 ;

			// 設定のロード
			Settings settings =	ApplicationManager.LoadSettings() ;
			if( settings != null )
			{
				basicWidth  = settings.BasicWidth ;
				basicHeight = settings.BasicHeight ;

				limitWidth  = settings.LimitWidth ;
				limitHeight = settings.LimitHeight ;
			}

			SetResolution( basicWidth, basicHeight, limitWidth, limitHeight ) ;
		}

		// セーフエリアの情報が必要な際に呼び出される
		protected override Rect GetSafeArea()
		{
			// セーフエリアの情報を返す
			return ApplicationManager.GetSafeArea() ;
		}
	}
}
