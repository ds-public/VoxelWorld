using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

// https://github.com/neuecc/MessagePack-CSharp
// https://github.com/neuecc/MessagePack-CSharp/releases
using MessagePack ;	

using __m = DBS.MassDataCategory ;
using __w = DBS.WorkDataCategory ;

namespace DBS.UserDataCategory
{
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class UnitData
	{
		//---------------------------------------------------------------------------
		// 保存対象

		public	long	id ;	// 保存対象

		public	int	category ;	// 0=player・1=unique・2=common

		[IgnoreMember]
		public int		Category
		{
			get
			{
				return category ;
			}
			set
			{
				category = value ;
			}
		}

		//-----------------------------------

		public	string	name ;	// 保存対象

		[IgnoreMember]
		public	string	Name
		{
			get
			{
				return name ;
			}
			set
			{
				name = value ;
			}
		}

		[SerializeField]
		protected	int	class_type ;	// 現在のクラス(MassData.ClassData.id)

		[IgnoreMember]
		public __m.ClassType ClassType
		{
			get
			{
				// 後でクラステーブルからクラスコードを取得するように変える
				__m.ClassData pc = __m.ClassData.GetByType( class_type ) ;
				return ( __m.ClassType )pc.type ;
			}
			set
			{
				class_type = ( int )value ;
			}
		}

		[IgnoreMember]
		public	string	ClassName
		{
			get
			{
				__m.ClassData pc = __m.ClassData.GetByType( class_type ) ;
				return pc.name ;
			}
		}

		// 保存対象
		public	int		experience ;

		/// <summary>
		/// 現在のレベルから次のレベルまでに得た経験値量
		/// </summary>
		[IgnoreMember]
		public	int		ExperienceNow
		{
			get
			{
				int level = Level ;
				if( level == 1 )
				{
					return experience ;
				}

				int total = __m.ExperienceData.GetTotal( level - 1 ) ;

				return experience - total ;
			}
		}

		/// <summary>
		/// 現在のレベルで次のレベルになるために必要な経験値量を取得する
		/// </summary>
		[IgnoreMember]
		public int		ExperienceMax
		{
			get
			{
				return __m.ExperienceData.GetValue( Level ) ;
			}
		}

		[IgnoreMember]
		public	int		Level
		{
			get
			{
				return __m.ExperienceData.GetLevel( experience ) ;
			}
		}

		//-----------------------------------

		// 保存対象
		public	int		hp ;

		[IgnoreMember]
		public	int		HpNow
		{
			get
			{
				int hpNow = hp ;
				if( hpNow >  HpMax )
				{
					hpNow  = HpMax ;
				}
				return hpNow ;
			}
			set
			{
				int hpNow = value ;
				if( hpNow >  HpMax )
				{
					hpNow  = HpMax ;
				}
				hp = hpNow ;
			}
		}

		[IgnoreMember]
		public	int		HpMax
		{
			get
			{
				int v = UndressedStatus[  0 ] + EquipmentStatus[  0 ] ;
				if( v <  1 )
				{
					v  = 1 ;
				}
				return v ;
			}
		}

		//---------------

		// 保存対象
		public	int		sp ;

		[IgnoreMember]
		public	int		SpNow
		{
			get
			{
				int spNow = sp ;
				if( spNow >  SpMax )
				{
					spNow  = SpMax ;
				}
				return spNow ;
			}
			set
			{
				int spNow = value ;
				if( spNow >  SpMax )
				{
					spNow  = SpMax ;
				}
				sp = spNow ;
			}
		}

		[IgnoreMember]
		public	int		SpMax
		{
			get
			{
				int v = UndressedStatus[  1 ] + EquipmentStatus[  1 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}
		
		//---------------

		// 保存対象
		public	int		mp ;

		[IgnoreMember]
		public	int		MpNow
		{
			get
			{
				int mpNow = mp ;
				if( mpNow >  MpMax )
				{
					mpNow  = MpMax ;
				}
				return mpNow ;
			}
			set
			{
				int mpNow = value ;
				if( mpNow >  MpMax )
				{
					mpNow  = MpMax ;
				}
				mp = mpNow ;
			}
		}

		[IgnoreMember]
		public	int		MpMax
		{
			get
			{
				int v = UndressedStatus[  2 ] + EquipmentStatus[  2 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}
		
		//-----------------------------------

		/// <summary>
		/// 物攻
		/// </summary>
		[IgnoreMember]
		public	int		PAttack
		{
			get
			{
				int v = UndressedStatus[  3 ] + EquipmentStatus[  3 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}
		
		/// <summary>
		/// 物防
		/// </summary>
		[IgnoreMember]
		public	int		PDefense
		{
			get
			{
				int v = UndressedStatus[  4 ] + EquipmentStatus[  4 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}

		/// <summary>
		/// 速度
		/// </summary>
		[IgnoreMember]
		public	int		Speed
		{
			get
			{
				int v = UndressedStatus[  5 ] + EquipmentStatus[  5 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}

		/// <summary>
		/// 魔攻
		/// </summary>
		[IgnoreMember]
		public	int		MAttack
		{
			get
			{
				int v = UndressedStatus[  6 ] + EquipmentStatus[  6 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}

		/// <summary>
		///魔治
		/// </summary>
		[IgnoreMember]
		public	int		MResilience
		{
			get
			{
				int v = UndressedStatus[  7 ] + EquipmentStatus[  7 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}

		/// <summary>
		/// 魔防
		/// </summary>
		[IgnoreMember]
		public	int		MDefense
		{
			get
			{
				// 武器の累積を返す
				int v = UndressedStatus[  8 ] + EquipmentStatus[  8 ] ;
				if( v <  0 )
				{
					v  = 0 ;
				}
				return v ;
			}
		}

		//-----------------------------------
		
		/// <summary>
		/// 射程距離
		/// </summary>
		[IgnoreMember]
		public int Range
		{
			get
			{
				// 射程距離(後で武器から取るようにする)
				if( Equipments[ 0 ] == null || Equipments[ 0 ].item_id == 0 )
				{
					return 1 ;
				}

				__m.EquipmentData e = Equipments[ 0 ].GetEquipment() ;

				return e.range ;
			}
		}

		//-----------------------------------

		// とりあえず仮の耐性値
		public	int[]	regist ;	// 物理耐性

		public int[] Regist
		{
			get
			{
				// 装備品等から算出した耐性値を返すようにする
				return regist ;
			}
		}

		public	long[]	skill_ids ;  // 使用可能なスキル

		// 表示イメージ
		public	int	image ;

		/// <summary>
		/// イメージパス
		/// </summary>
		public string ImagePath
		{
			get
			{
				return image.ToString( "D2" ) ;
			}
		}

		//-----------------------------------

		// 装備品(８部位)
		public ItemData[]		Equipments = new ItemData[ 8 ] ;

		//---------------------------------------------------------------------------

		// 装備品の補正がかからない基本パラメータ
		[NonSerialized][IgnoreMember]
		public	int[]	UndressedStatus ;

		// 装備品の補正合計パラメータ
		[NonSerialized][IgnoreMember]
		public	int[]	EquipmentStatus ;


		//---------------------------------------------------------------------------
		
		/// <summary>
		/// データを準備する
		/// </summary>
		public void Prepare()
		{
			UndressedStatus = GetUndressedStatus() ;
			EquipmentStatus = GetEquipmentStatus() ;
		}

		/// <summary>
		/// 装備品無し状態の主要１４パラメータを取得する
		/// </summary>
		/// <returns></returns>
		public int[] GetUndressedStatus()
		{
			int[] p = new int[  9 ] ;

			__m.ClassData pc = __m.ClassData.GetByType( class_type ) ;

			// Lv50 で 20倍 Lv100 で 60 倍
			double xb = WorkData.GrowingFactor[ "basis" ][ Level ].value ;

			// Lv50 で  5倍 Lv100 で 15 倍
//			double xr = WorkData.growingFactor[ "ratio" ][ Level ].value ;

			// Lv50 で  2倍 Lv100 で  6 倍
			double xs = WorkData.GrowingFactor[ "speed" ][ Level ].value ;

			//--------------

			// HpMax
			p[  0 ] = ( int )( pc.hp * xb ) ;

			// SpMax
			p[  1 ] = ( int )( pc.sp * xb ) ;

			// MpMax
			p[  2 ] = ( int )( pc.mp * xb ) ;

			//--------------

			// P_Attack
			p[  3 ] = ( int )( pc.attack * 1 * xb ) ;

			// P_Defense
			p[  4 ] = ( int )( pc.defense * 1 * xb ) ;

			// Speed
			p[  5 ] = ( int )( pc.speed * 1 * xs ) ;

			//--------------

			// M_Attack
			p[  6 ] = ( int )( pc.intelligence * 1 * xb ) ;

			// M_Resilience
			p[  7 ] = ( int )( pc.mind * 1 * xb ) ;

			// M_Defense
			p[  8 ] = ( int )( pc.grace * 1 * xb ) ;

			//----------------------------------

			return p ;
		}

		/// <summary>
		/// 装備品の補正合計値を取得する
		/// </summary>
		/// <param name="equipments"></param>
		/// <returns></returns>
		public int[] GetEquipmentStatus( ItemData[] equipments = null )
		{
			int[] p = new int[  9 ] ;

			if( equipments == null )
			{
				equipments = Equipments ;
			}

			__m.EquipmentData e ;

			foreach( var equipment in equipments )
			{
				if( equipment != null && equipment.item_id >  0 )
				{
					e = equipment.GetEquipment() ;
					if( e != null )
					{
						// HpMax
						p[  0 ] += e.hp ;
						
						// SpMax
						p[  1 ] += e.sp ;
						
						// MpMax
						p[  2 ] += e.mp ;

						// P_Attack
						p[  3 ] += e.attack ;

						// P_Defense
						p[  4 ] += e.defense ;

						// Speed
						p[  5 ] += e.speed ;

						// M_Attack
						p[  6 ] += e.intelligence ;

						// M_Resilience
						p[  7 ] += e.mind ;

						// M_Defense
						p[  8 ] += e.grace ;
					}
				}
			}

			return p ;
		}


		//---------------------------------------------------------------------------

		public static UnitData GetById( long id )
		{
			return UserData.Memory.Units.FirstOrDefault( _ => _.id == id ) ;
		}

		/// <summary>
		/// 新しいユニットを追加する
		/// </summary>
		/// <param name="unit"></param>
		public static long Add( UnitData unit )
		{
			// 空いている識別値を取得する
			long id = 1 ;

			UnitData[] units = UserData.Memory.Units.OrderBy( _ => _.id ).ToArray() ;
			if( units.IsNullOrEmpty() == false )
			{
				if( units[ 0 ].id <= id )
				{
					long firstId = units[ 0 ].id ;
					firstId ++ ;

					int i, l = units.Length ;
					for( i  = 1 ; i <  l ; i ++ )
					{
						if( firstId != units[ i ].id )
						{
							break ;
						}
						firstId ++ ;
					}

					id = firstId ;
				}
			}

			unit.id = id ;
			UserData.Memory.Units.Add( unit ) ;

			return id ;
		}

		/// <summary>
		/// 既存のユニットを削除する
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool Remove( long id )
		{
			UnitData unit = UserData.Memory.Units.FirstOrDefault( _ => _.id == id ) ;
			if( unit == null )
			{
				return false ;
			}

			UserData.Memory.Units.Remove( unit ) ;

			// 現在のチームにいればそれも外す
			__w.NormalUnit[][] units = TeamData.GetActiveTeam( true ) ;

			int line, side ;
			for( line  = 0 ; line <= 1 ; line ++ )
			{
				if( units[ line ] != null && units[ line ].Length >  0 )
				{
					for( side  = 0 ; side <  units[ line ].Length ; side ++ )
					{
						if( units[ line ][ side ] != null && units[ line ][ side ].UnitId == id )
						{
							units[ line ][ side ]  = null ;
						}
					}
				}
			}

			// チームセット内にも参照があれば外す
			List<TeamData> teams = UserData.Memory.Teams ;
			int i, l = teams.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( teams[ i ] != null )
				{
					if( teams[ i ].front != null && teams[ i ].front.Length >  0 )
					{
						for( side  = 0 ; side <  teams[ i ].front.Length ; side ++ )
						{
							if( teams[ i ].front[ side ] == id )
							{
								teams[ i ].front[ side ]  = 0 ;
							}
						}
					}

					if( teams[ i ].back != null && teams[ i ].back.Length >  0 )
					{
						for( side  = 0 ; side <  teams[ i ].back.Length ; side ++ )
						{
							if( teams[ i ].back[ side ] == id )
							{
								teams[ i ].back[ side ]  = 0 ;
							}
						}
					}
				}
			}
			

			return true ;
		}

		/// <summary>
		/// 削除可能系のユニットの登録数を取得する
		/// </summary>
		/// <returns></returns>
		public static int GetCommonUnitCount()
		{
			return UserData.Memory.Units.Count( _ => ( _.Category == 2 ) ) ;
		}

		/// <summary>
		/// 現在のアクティブチームに入っているかどうか
		/// </summary>
		public bool IsActiveTeamJoined()
		{
			UnitData[] teamUnits = TeamData.GetActiveTeamUnits() ;

			if( teamUnits.IsNullOrEmpty() == true )
			{
				return false ;
			}
			return teamUnits.Contains( this ) ;
		}

		//-----------------------------------------------------------
		// スキル関連


		/// <summary>
		/// 所持しているスキルを取得する(1=移動中可能・2=戦闘中可能)
		/// </summary>
		/// <returns></returns>
		public __m.SkillData[] GetSkills( int priority, bool filter )
		{
			if( skill_ids.IsNullOrEmpty() == true )
			{
				// 無し
				return null ;
			}

			List<__m.SkillData> skills = new List<__m.SkillData>() ;

			skill_ids.ForEach
			(
				( long id ) =>
				{
					if( id >  0 )
					{
						// 有効な識別子
						__m.SkillData skill = __m.SkillData.GetById( id ) ;
						if( priority == 0 || ( priority >= 1 && ( skill.Scene & priority ) != 0 ) )
						{
							skills.Add( skill ) ;
						}
					}
				}
			) ;

			if( skills.Count == 0 )
			{
				// 使用可能なスキルは１つも無い
				return null ;
			}

			if( priority == 0 || filter == true )
			{
				// 全対象なのでここで終了(もしくは使用可能なもののみ)
				return skills.ToArray() ;
			}

			//----------------------------------
			// 除外された側のカテゴリのスキルを追加する

			skill_ids.ForEach
			(
				( long id ) =>
				{
					if( id >  0 )
					{
						// 有効な識別子
						__m.SkillData skill = __m.SkillData.GetById( id ) ;
						if( skills.Contains( skill ) == false )
						{
							skills.Add( skill ) ;
						}
					}
				}
			) ;

			return skills.ToArray() ;
		}

		//-----------------------------------------------------------
		// 装備品関連

		/// <summary>
		/// 装備処理用に装備品情報をそのまま複製したものを返す
		/// </summary>
		/// <returns></returns>
		public ItemData[] DuplicacteEquipments()
		{
			int i, l = Equipments.Length ;

			ItemData[] equipments = new ItemData[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				equipments[ i ] = new ItemData() ;
				equipments[ i ].Write( Equipments[ i ] ) ;
			}

			return equipments ;
		}

		/// <summary>
		/// 装備を外す
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public bool FreeEquipment( int slot )
		{
			if( Equipments[ slot ].item_id == 0 )
			{
				// そもそも装備していない
				return true ;
			}

			if( InventoryData.Count >= CommonData.InventoryMax )
			{
				// アイテムがいっぱいで外せない
				return false ;
			}

			ItemData item = new ItemData
			{
				id		= 0,	// Add 時に自動的に設定される
				item_id	= Equipments[ slot ].item_id,
				data	= Equipments[ slot ].data
			} ;

			// アイテムを追加する
			InventoryData.Add( item ) ;

			// 装備品をクリアする
			Equipments[ slot ].Clear() ;

			//----------------------------------

			// 装備品が変更されたので装備品補正合計値を更新する
			EquipmentStatus = GetEquipmentStatus() ;

			return true ;
		}
		
		/// <summary>
		/// 道具袋から装備する
		/// </summary>
		/// <param name="slot">Slot.</param>
		/// <param name="u_ItemId">U item identifier.</param>
		public void KeepEquipment( int slot, long u_ItemId )
		{
			ItemData item ;

			// 現在装備中であればそれを外す
			if( Equipments[ slot ].item_id >  0 )
			{
				// アイテムを追加する(インスタンスを複製して情報をコピーする)
				InventoryData.Add( Equipments[ slot ] ) ;

				// 装備品をクリアする
				Equipments[ slot ].Clear() ;
			}

			//----------------------------------

			// 新たに装備品を取得する
			item = InventoryData.GetById( u_ItemId ) ;

			// アイテム情報を上書する
			Equipments[ slot ].Write( item ) ;

			// 所持品からアイテムを削除する
			InventoryData.Remove( u_ItemId ) ;

			//----------------------------------

			// 装備品が変更されたので装備品補正合計値を更新する
			EquipmentStatus = GetEquipmentStatus() ;
		}

		/// <summary>
		/// 他のメンバーの装備品を入手するか交換する
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="u_PlayerUnitId"></param>
		/// <param name="swap"></param>
		/// <returns></returns>
		public bool MoveEquipment( int slot, long u_PlayerUnitId, bool swap )
		{
			UnitData playerUnit = UnitData.GetById( u_PlayerUnitId ) ;

			if( swap == false )
			{
				// 入手する

				// まずは自身の装備を外す
				if( FreeEquipment( slot ) == false )
				{
					return false ;	// 所持品がいっぱいで外す事はできない
				}

				// 他のメンバーの装備品を受け取る
				Equipments[ slot ].Write( playerUnit.Equipments[ slot ] ) ;

				// 他のメンバーの装備品を外す
				playerUnit.Equipments[ slot ].Clear() ;
			}
			else
			{
				// 交換する

				// 自身の装備品情報を別に保存
				ItemData item = new ItemData() ;

				item.Write( Equipments[ slot ] ) ;

				// 他のメンバーの装備品を受け取る
				Equipments[ slot ].Write( playerUnit.Equipments[ slot ] ) ;

				// 他のメンバーむの装備品に自身の装備品を上書する(交換)
				playerUnit.Equipments[ slot ].Write( item ) ;
			}

			//----------------------------------

			// 装備品が変更されたので装備品補正合計値を更新する
			EquipmentStatus = GetEquipmentStatus() ;
			
			return true ;
		}

		//-----------------------------------------------------------
		// ステータス関連
		
		/// <summary>
		/// 基本１４種の能力値を取得する
		/// </summary>
		/// <returns></returns>
		public int[] GetBasicStatus( ItemData[] equipments = null )
		{
			// 最終的には装備品の識別子ではなく装備品のクラスインスタンスに変える(強化や付与による性能変化があるため)

			int[] p = new int[  9 ] ;

			int i, l = p.Length, v ;

			int[] e = equipments == null ? EquipmentStatus : GetEquipmentStatus( equipments ) ;

			// 現在の装備の状態での値を取得する
			for( i  = 0 ; i <  l ; i ++ )
			{
				v = UndressedStatus[ i ] + e[ i ] ;
				if( i == 0 )
				{
					// HpMax
					if( v <  1 )
					{
						v  = 1 ;
					}
				}
				else
				{
					if( v <  0 )
					{
						v  = 0 ;
					}
				}

				p[ i ] = v ;
			}

			return p ;
		}

		//-----------------------------------------------------------
		// 表示関連

		/// <summary>
		/// 顔画像
		/// </summary>
		[IgnoreMember]
		public Sprite FaceImage
		{
			get
			{
				return Asset.Load<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Face", Asset.CachingType.Same ) ;
			}
		}

		/// <summary>
		/// 顔画像
		/// </summary>
		[IgnoreMember]
		public Sprite TurnImage
		{
			get
			{
				return Asset.LoadSub<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Face", "Trun", Asset.CachingType.Same ) ;
			}
		}

		/// <summary>
		/// 体画像
		/// </summary>
		[IgnoreMember]
		public Sprite ShapeImage
		{
			get
			{
				return Asset.Load<Sprite>( "Textures/PlayerUnit/" + ImagePath + "//Shape", Asset.CachingType.Same ) ;
			}
		}

		//-----------------------------------------------------------
		// 操作系

		/// <summary>
		/// 生存していればＨＰ・ＳＰ・ＭＰを全回復させる
		/// </summary>
		/// <returns></returns>
		public bool Restore()
		{
			if( HpNow == 0 )
			{
				// 戦闘不能
				return false ;
			}

			HpNow = HpMax ;
			SpNow = SpMax ;
			MpNow = MpMax ;

			return true ;
		}
	}
}
