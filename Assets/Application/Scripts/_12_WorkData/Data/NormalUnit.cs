using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;


using __m = DBS.MassDataCategory ;
using __u = DBS.UserDataCategory ;


/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace DBS.WorkDataCategory
{
	/// <summary>
	/// ゲーム全体から参照されるプレイヤー系データを保持するクラス
	/// </summary>
	public class NormalUnit
	{
		public	long	UnitId ;		// __u.PlayerUnit.id または __m.EnemyUnitData.id

		//---------------
		// Enemy 用

		private	int		m_HpNow ;		// Enemy用 Hp現在値

		private	int		m_SpNow ;		// Enemy用 Sp現在値

		private	int		m_MpNow ;		// Enemy用 Mp現在値

		//---------------

		public	int		Area ;		//	0 = __u.PlayerUnit 1 = __m.EnemyUnitData.id
		public	int		Line ;
		public	int		Side ;

		// ※Line Side は、対象を直列的なリストに格納した際に、存在しないと対象の隊列位置が分からなくなるため必要である。
		
		//---------------------------------------------------------------------------

		public NormalUnit()
		{
		}
		
		// プレイヤー側
		public NormalUnit( __u.UnitData unit, int line, int side )
		{
			UnitId		= unit.id ;

			//----------------------------------

			Area		= 0 ;
			Line		= line ;
			Side		= side ;

			//----------------------------------
		}

		// エネミー側
		public NormalUnit( __m.EnemyUnitData unit, int line, int side )
		{
			UnitId		= unit.id ;

			//----------------------------------

			m_HpNow		= unit.hp_now ;
			m_SpNow		= unit.sp_now ;
			m_MpNow		= unit.mp_now ;

			//----------------------------------

			Area		= 1 ;
			Line		= line ;
			Side		= side ;

			//----------------------------------
		}

		//-----------------------------------------------------------

		/// <summary>
		/// プレイヤーユニットのインスタンスを取得する
		/// </summary>
		/// <returns></returns>
		public __u.UnitData GetPlayerUnit()
		{
			if( Area == 1 )
			{
				return null ;	// エネミー側です
			}

			return __u.UnitData.GetById( UnitId ) ;
		}
		
		/// <summary>
		/// エネミーユニットのインスタンスを取得する
		/// </summary>
		/// <returns></returns>
		public __m.EnemyUnitData GetEnemyUnitData()
		{
			if( Area == 0 )
			{
				return null ;	// プレイヤー側です
			}

			return __m.EnemyUnitData.GetById( UnitId ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		///  名前
		/// </summary>
		public string	Name
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.Name ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.name ;
				}
			}
		}

		public __m.ClassType ClassType
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					return GetPlayerUnit().ClassType ;
				}
				else
				{
					// Enemy
					return __m.ClassType.Unknown ;
				}

			}
		}

		/// <summary>
		/// レベル
		/// </summary>
		public int Level
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.Level ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.level ;
				}
			}
		}

		/// <summary>
		/// ＨＰ現在値
		/// </summary>
		public int HpNow
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.HpNow ;
				}
				else
				{
					// Enemy
					return m_HpNow ;
				}
			}
			set
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					unit.HpNow = value ;
				}
				else
				{
					// Enemy
					m_HpNow = value ;
				}
			}
		}

		/// <summary>
		/// ＨＰ最大値
		/// </summary>
		public int HpMax
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.HpMax ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.hp_max ;
				}
			}
		}

		/// <summary>
		/// ＨＰ残量割合
		/// </summary>
		public float HpRatio
		{
			get
			{
				return ( float )HpNow / ( float )HpMax ;
			}
		}

		/// <summary>
		/// ＳＰ現在値
		/// </summary>
		public int SpNow
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.SpNow ;
				}
				else
				{
					// Enemy
					return m_SpNow ;
				}
			}
			set
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					unit.SpNow = value ;
				}
				else
				{
					// Enemy
					m_SpNow = value ;
				}
			}
		}

		/// <summary>
		/// ＳＰ最大値
		/// </summary>
		public int SpMax
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.SpMax ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.sp_max ;
				}
			}
		}

		/// <summary>
		/// ＳＰ残量割合
		/// </summary>
		public float SpRatio
		{
			get
			{
				return ( float )SpNow / ( float )SpMax ;
			}
		}

		/// <summary>
		/// ＭＰ現在値
		/// </summary>
		public int MpNow
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.MpNow ;
				}
				else
				{
					// Enemy
					return m_MpNow ;
				}
			}
			set
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					unit.MpNow = value ;
				}
				else
				{
					// Enemy
					m_MpNow = value ;
				}
			}
		}

		/// <summary>
		/// ＭＰ最大値
		/// </summary>
		public int MpMax
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.MpMax ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.mp_max ;
				}
			}
		}
		
		/// <summary>
		/// ＭＰ残量割合
		/// </summary>
		public float MpRatio
		{
			get
			{
				return ( float )MpNow / ( float )MpMax ;
			}
		}


		//-----------------------------------

		/// <summary>
		/// 物攻
		/// </summary>
		public int PAttack
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.PAttack ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.attack ;
				}
			}
		}

		/// <summary>
		/// 物防
		/// </summary>
		public int PDefense
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.PDefense ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.defense ;
				}
			}
		}

		/// <summary>
		/// 速度
		/// </summary>
		public int Speed
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.Speed ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.speed ;
				}
			}
		}

		/// <summary>
		/// 魔攻
		/// </summary>
		public int MAttack
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.MAttack ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.intelligence ;
				}
			}
		}

		/// <summary>
		/// 魔治
		/// </summary>
		public int MResilience
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.MResilience ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.mind ;
				}
			}
		}

		/// <summary>
		/// 魔防
		/// </summary>
		public int MDefense
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.MDefense ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.grace ;
				}
			}
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 攻撃の射程距離
		/// </summary>
		public int Range
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					return GetPlayerUnit().Range ;
				}
				else
				{
					// Enemy
					return GetEnemyUnitData().Range ;
				}
			}
		}

		/// <summary>
		/// 耐性値
		/// </summary>
		public int[] Regist
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					return GetPlayerUnit().Regist ;
				}
				else
				{
					// Enemy
					return GetEnemyUnitData().regist ;
				}
			}
		}


		//-----------------------------------------------------------
		// スキル関係

		public __m.SkillData[] GetSkills( int priority, bool filter )
		{
			if( Area == 0 )
			{
				// Player
				return GetPlayerUnit().GetSkills( priority, filter ) ; 
			}
			else
			{
				// Enemy
				return GetEnemyUnitData().GetSkills( priority, filter ) ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// イメージパス
		/// </summary>
		public string ImagePath
		{
			get
			{
				if( Area == 0 )
				{
					// Player
					__u.UnitData unit = __u.UnitData.GetById( UnitId ) ;
					return unit.ImagePath ;
				}
				else
				{
					// Enemy
					__m.EnemyUnitData unit = __m.EnemyUnitData.GetById( UnitId ) ;
					return unit.image ;
				}
			}
		}

		/// <summary>
		/// 顔画像
		/// </summary>
		public Sprite FaceImage
		{
			get
			{
				if( Area == 0 )
				{
					// プレイヤー
					return Asset.Load<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Face", Asset.CachingType.Same ) ;
				}
				else
				{
					// エネミー
					return null ;
				}
			}
		}

		/// <summary>
		/// 顔画像
		/// </summary>
		public Sprite TurnImage
		{
			get
			{
				if( Area == 0 )
				{
					// プレイヤー
					return Asset.LoadSub<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Face", "Turn", Asset.CachingType.Same ) ;
				}
				else
				{
					// エネミー
					return Asset.LoadSub<Sprite>( "Textures/EnemyUnit/" + ImagePath + "//Shape", "Turn", Asset.CachingType.Same ) ;
				}
			}
		}

		/// <summary>
		/// 体画像
		/// </summary>
		public Sprite ShapeImage
		{
			get
			{
				if( Area == 0 )
				{
					// プレイヤー
					return Asset.Load<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Shape", Asset.CachingType.Same ) ;
				}
				else
				{
					// エネミー
					return Asset.Load<Sprite>( "Textures/EnemyUnit/" + ImagePath + "//Shape", Asset.CachingType.Same ) ;
				}
			}
		}

		/// <summary>
		/// 体画像
		/// </summary>
		public Sprite FrameImage
		{
			get
			{
				if( Area == 0 )
				{
					// プレイヤー
					return null ;
				}
				else
				{
					// エネミー
					return Asset.Load<Sprite>( "Textures/EnemyUnit/" + ImagePath + "//Frame", Asset.CachingType.Same ) ;
				}
			}
		}
	}
}
