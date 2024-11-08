using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace SpriteHelper
{
	/// <summary>
	/// スクリプトから Sprite をコントロールする際のアンカーの指定
	/// </summary>
	public enum SpriteAnchorTypes
	{
		LeftTop,
		CenterTop,
		RightTop,
		StretchTop,
	
		LeftMiddle,
		CenterMiddle,
		RightMiddle,
		StretchMiddle,
	
		LeftBottom,
		CenterBottom,
		RightBottom,
		StretchBottom,
	
		LeftStretch,
		CenterStretch,
		RightStretch,
		Stretch,
	
		Center,
	
		LeftCustom,
		CenterCustom,
		RightCustom,
		StretchCustom,
	
		CustomTop,
		CustomMiddle,
		CustomBottom,
		CustomStretch,
	}

	/// <summary>
	/// スクリプトから Sprite をコントロールする際のピボットの指定
	/// </summary>
	public enum SpritePivotTypes
	{
		LeftTop,
		CenterTop,
		RightTop,
	
		LeftMiddle,
		CenterMiddle,
		RightMiddle,
	
		LeftBottom,
		CenterBottom,
		RightBottom,
	
		Center,
	}

	/// <summary>
	/// ビューポートサイズのタイプ
	/// </summary>
	public enum ViewportSizeTypes
	{
		/// <summary>
		/// 固定
		/// </summary>
		Fixed,

		/// <summary>
		/// 追従
		/// </summary>
		Stretch,
	}

	/// <summary>
	/// マップチップのビューポート外に出た際の挙動タイプ
	/// </summary>
	public enum MapChipDestructionTypes
	{
		/// <summary>
		/// 破棄する
		/// </summary>
		Destroy,

		/// <summary>
		/// 非アクティブ状態にする
		/// </summary>
		Deactive,

		/// <summary>
		/// アクティブ状態を維持する
		/// </summary>
		Active,
	}
}
