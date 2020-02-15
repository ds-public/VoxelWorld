using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

using CSVHelper ;

using DBS.MassDataCategory ;

namespace DBS
{
	/// <summary>
	/// ゲーム全体から参照される静的データを保持するクラス
	/// </summary>
	public class MassData
	{
		//-------------------------------------------------------------------------------------------

		// クラス情報
		private	List<ClassData>							m_ClassTable ;
		public static List<ClassData>					  ClassTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_ClassTable ;
			}
		}

		//-----------------------------------

		// 経験値情報
		private	List<ExperienceData>					m_ExperienceTable ;
		public static List<ExperienceData>				  ExperienceTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_ExperienceTable ;
			}
		}

		//-----------------------------------

		// エネミーユニット
		private	List<EnemyUnitData>						m_EnemyUnitTable ;
		public static List<EnemyUnitData>				  EnemyUnitTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_EnemyUnitTable ;
			}
		}

		//-----------------------------------

		// エネミーチーム
		private	List<EnemyTeamData>						m_EnemyTeamTable ;
		public static List<EnemyTeamData>				  EnemyTeamTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_EnemyTeamTable ;
			}
		}

		//-----------------------------------

		// アイテム
		private	List<ItemData>							m_ItemTable ;
		public	static List<ItemData>					  ItemTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_ItemTable ;
			}
		}
		

		//-----------------------------------

		// 装備品
		private	List<EquipmentData>						m_EquipmentTable ;
		public static List<EquipmentData>				  EquipmentTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_EquipmentTable ;
			}
		}

		//-----------------------------------

		private	List<GoodsData>							m_GoodsTable ;
		public static List<GoodsData>					  GoodsTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_GoodsTable ;
			}
		}

		//-----------------------------------

		private	List<SkillData>							m_SkillTable ;
		public static List<SkillData>					  SkillTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_SkillTable ;
			}
		}

		//-----------------------------------

		private	List<InfluenceData>						m_InfluenceTable ;
		public static List<InfluenceData>				  InfluenceTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_InfluenceTable ;
			}
		}

		//-----------------------------------

		private	List<EffectData>						m_EffectTable ;
		public static List<EffectData>					  EffectTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_EffectTable ;
			}
		}

		//-----------------------------------


		private	List<ActionPatternContainerData>		m_ActionPatternContainerTable ;
		public static List<ActionPatternContainerData>	  ActionPatternContainerTable
		{
			get
			{
				return MassDataManager.Instance.mass.m_ActionPatternContainerTable ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プレイヤーデータをストレージから読み出す(ショートカットアクセス)
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
//		public static bool Load()
//		{
//			return MassDataManager.instance.mass.LoadFromFile() ;
//		}

		public static AsyncState LoadAsync()
		{
			AsyncState tState = new AsyncState() ;
			MassDataManager.Instance.StartCoroutine( MassDataManager.Instance.mass.LoadFromFileAsync( tState ) ) ;
			return tState ;
		}
		
		public IEnumerator LoadFromFileAsync( AsyncState state )
		{
			string path ;
			int i, l, j, m ;

			TextAsset ta ;

			//----------------------------------------------------------

			// プレイヤークラスのデータを読み込む(新)
			path = "Data/MassData//PlayerClass" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<ClassData>( ta.text, ref m_ClassTable, 1 ) ;
			}

//			PlayerClass pc = PlayerClass.GetById( 3 ) ;
//			Debug.LogWarning( "name:" + pc.name + " luck:" + pc.luck ) ;

			//----------------------------------------------------------

			// プレイヤーエクスペリエンスのデータを読み込む(新)
			path = "Data/MassData//PlayerExperience" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; }  ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<ExperienceData>( ta.text, ref m_ExperienceTable, 1 ) ;
			}

			ExperienceData.Prepare() ;

//			PlayerClass pc = PlayerClass.GetById( 3 ) ;
//			Debug.LogWarning( "name:" + pc.name + " luck:" + pc.luck ) ;

			//----------------------------------------------------------

			// エネミーユニットのデータを読み込む(新)
			path = "Data/MassData//EnemyUnit" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<EnemyUnitData>( ta.text, ref m_EnemyUnitTable, 1 ) ;
			}

			//----------------------------------------------------------

			// エネミーチームのデータを読み込む(新)
			path = "Data/MassData//EnemyTeam" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<EnemyTeamData>( ta.text, ref m_EnemyTeamTable, 1 ) ;
			}

			//----------------------------------------------------------

			// アイテムのデータを読み込む(新)
			path = "Data/MassData//Item" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<ItemData>( ta.text, ref m_ItemTable, 1 ) ;
			}

			//----------------------------------------------------------

			// 装備品のデータを読み込む(新)
			path = "Data/MassData//Equipment" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<EquipmentData>( ta.text, ref m_EquipmentTable, 1 ) ;
			}

			//----------------------------------------------------------

			// グッズのデータを読み込む(新)
			path = "Data/MassData//Goods" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<GoodsData>( ta.text, ref m_GoodsTable, 1 ) ;
			}

			m_GoodsTable.ForEach( _ => _.Prepare() ) ;

			//----------------------------------------------------------

			// スキルのデータを読み込む(新)
			path = "Data/MassData//Skill" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<SkillData>( ta.text, ref m_SkillTable, 1 ) ;
			}

			//----------------------------------------------------------

			// 道具・特技の効果データを読み込む(新)
			path = "Data/MassData//Influence" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<InfluenceData>( ta.text, ref m_InfluenceTable, 1 ) ;
			}

			//----------------------------------------------------------

			// エネミーチームのデータを読み込む(新)
			path = "Data/MassData//Effect" ;
			ta = Asset.Load<TextAsset>( path ) ;
			if( ta == null )
			{
				yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
			}
			if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
			{
				CSVObject.Load<EffectData>( ta.text, ref m_EffectTable, 1 ) ;
			}

			//----------------------------------------------------------

			// アクションパターンを読み込む
			string tBasePath = "Data/MassData//ActionPattern/" ;

			m_ActionPatternContainerTable = new List<ActionPatternContainerData>() ;


			List<ActionPatternData> actionPatternTable = null ;
			ActionPatternContainerData actionPatternContainer ;  


			l = 1 ;
			for( i  = 1 ; i <= l ; i ++ )
			{
				path = tBasePath + string.Format( "{0:D3}", i ) ;

				ta = Asset.Load<TextAsset>( path ) ;
				if( ta == null )
				{
					yield return Asset.LoadAsync<TextAsset>( path, ( _ ) => { ta = _ ; } ) ;
				}
				if( ta != null && string.IsNullOrEmpty( ta.text ) == false )
				{
					CSVObject.Load( ta.text, ref actionPatternTable, 1 ) ;

					actionPatternTable.Sort( ( a, b ) => ( b.priority - a.priority ) ) ;	// 優先度で降順にする

					m = actionPatternTable.Count ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						actionPatternTable[ j ].Convert( string.Format( "{0:D3}", i ) + " [ " + string.Format( "{0:D2}", j ) ) ;
					}

					actionPatternContainer = new ActionPatternContainerData
					{
						id = i,
						ActionPatternTable = actionPatternTable
					} ;

					m_ActionPatternContainerTable.Add( actionPatternContainer ) ;
				}
			}

			//----------------------------------------------------------

			if( state != null )
			{
				state.IsDone = true ;
			}
		}
	}
}
