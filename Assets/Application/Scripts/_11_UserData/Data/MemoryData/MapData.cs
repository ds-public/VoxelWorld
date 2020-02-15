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
	/// <summary>
	/// １階層分のマッピングデータ
	/// </summary>
	[Serializable][MessagePackObject(keyAsPropertyName:true)]
	public class MapData
	{
		[Serializable][MessagePackObject(keyAsPropertyName:true)]
		public class CellData
		{
			public byte[] Floor = new byte[ 2 ] ;
			public byte Event ;

			public byte[] Wall = new byte[ 4 ] ;
			public byte[] Mark = new byte[ 4 ] ;


			//----------------------------------



		}

		public CellData[,] Cells = new CellData[ 100, 100 ] ;	// 100階層で9.5MBほど

		public MapData()
		{
			int lx = Cells.GetLength( 1 ) ;
			int ly = Cells.GetLength( 0 ) ;
			int x, y ;

			for( y  = 0 ; y <  ly ; y ++ )
			{
				for( x  = 0 ; x <  lx ; x ++ )
				{
					Cells[ y, x ] = new CellData() ;
				}
			}
		}
	}
}
