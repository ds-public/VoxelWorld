using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

/// <summary>
/// ＤＢＳパッケージ
/// </summary>
namespace DBS.MassDataCategory
{
	public enum ItemCategory
	{
		Unknown			= -1,

		// 道具種

		Tool			= 1000,	// 探索
		Aid				= 1001,	// 回復
		Scroll			= 1002,	// 戦闘

		// 武器種

		Sword			= 2000,	// 剣
		Blade			= 2001,	// 刀
		Dagger			= 2002,	// 小型武器

		Axe				= 2003,	// 斧
		Spear			= 2004,	// 槍
		Hammer			= 2005,	// 槌
		Scythe			= 2006,	// 鎌
		Whip			= 2007,	// 鞭
		Knuckle			= 2008,	// 拳

		Bow				= 2009,	// 弓
		Gun				= 2010,	// 銃

		Rod				= 2011,	// 棒
		Staff			= 2012,	// 杖
		Book			= 2013,	// 本


		// 補助種

		LargeShield		= 2100,	// 大型盾
		SmallShield		= 2101,	// 小型盾
		Grip			= 2102,	// グリップ
		Poison			= 2103,	// 毒
		Arrow			= 2104,	// 矢
		Bullet			= 2105,	// 弾
		Talisman		= 2106,	// 護符
		Minion			= 2107,	// 使い魔


		// 防具種

		HeavyBody		= 2200,	// 体(重:鉄)
		MediumBody		= 2201,	// 体(中:革)
		LightBody		= 2202,	// 体(軽:布)

		HeavyHead		= 2300,	// 頭(重:鉄)
		MediumHead		= 2301,	// 頭(中:革)
		LightHead		= 2302,	// 頭(軽:布)

		HeavyArm		= 2400,	// 手(重:鉄)
		MediumArm		= 2401,	// 手(中:革)
		LightArm		= 2402,	// 手(軽:布)
		
		HeavyLeg		= 2500,	// 足(重:鉄)
		MediumLeg		= 2501,	// 足(中:革)
		LightLeg		= 2502,	// 足(軽:布)


		// 装飾種

		Waist			= 2600,	// 腰(ベルト)
		Back			= 2601,	// 背(マント)
		Ear				= 2602,	// 耳(イヤリング)
		Neck			= 2603,	// 首(ネックレス)
		Wrist			= 2604,	// 手(ブレスレット)
		Finger			= 2605,	// 指(リング)


		// 魔石種

		MagicStone		= 2700,
	}
}
