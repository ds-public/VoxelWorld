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

}
