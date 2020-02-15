using System ;
using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;

namespace DBS.MassDataCategory
{
	/// <summary>
	/// 消耗品情報クラス
	/// </summary>
	public class GoodsData
	{
		// リフレクションで直接値を格納するフィールド
		
		protected long		id ;	// 識別子
		protected string	name ;	// 名称(使用しない)
		protected long		influence_id ;	// 効果
		protected string[]	materials ;	// 必要素材
		
		//---------------------------
		
		public class MaterialData
		{
			public	long	ItemId ;
			public	int		ItemCount ;
		}

		public		MaterialData[]	Materials ;

		//---------------------------------------------------------------------------

		public void Prepare()
		{
			int c = 0 ;

			int i, l = materials.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( materials[ i ].IsNullOrEmpty() == false )
				{
					c ++ ;
				}
			}

			Materials = new MaterialData[ c ] ;
			MaterialData m ;

			c = 0 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( materials[ i ].IsNullOrEmpty() == false )
				{
					m = new MaterialData() ;
					if( materials[ i ].Contains( "|" ) == false )
					{
						long.TryParse( materials[ i ], out m.ItemId ) ;
						m.ItemCount = 1 ;
					}
					else
					{
						string[] t = materials[ i ].Split( '|' ) ;
						long.TryParse( t[ 0 ], out m.ItemId ) ;
						int.TryParse( t[ 1 ], out m.ItemCount ) ;
					}
					Materials[ c ] = m ;
					c ++ ;
				}
			}

		}
		
		public static GoodsData GetById( long id )
		{
			return MassData.GoodsTable.FirstOrDefault( _ => _.id == id ) ;
		}
		
		public InfluenceData GetInfluence()
		{
			if( influence_id == 0 )
			{
				return null ;
			}
			
			return InfluenceData.GetById( influence_id ) ;
		}
		
		public int Scene
		{
			get
			{
				return GetInfluence().scene ;
			}
		}
		
		public int Area
		{
			get
			{
				return GetInfluence().area ;
			}
		}
		
		public int Width
		{
			get
			{
				return GetInfluence().width ;
			}
		}
		
		public int Range
		{
			get
			{
				return GetInfluence().range ;
			}
		}

		public bool Alive
		{
			get
			{
				return GetInfluence().alive ;
			}
		}

		public int Process
		{
			get
			{
				return GetInfluence().process ;
			}
		}

		public int[] Data
		{
			get
			{
				return GetInfluence().data ;
			}
		}
		
		public long EffectId
		{
			get
			{
				return GetInfluence().effect_id ;
			}
		}
		
		public int EffectTarget
		{
			get
			{
				return GetInfluence().effect_target ;
			}
		}
		
	}
}
