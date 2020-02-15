using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;
using TransformHelper ;

namespace DBS.nScreen.nWorld
{
    public struct BlockPosition
    {
        public int X ;
        public int Z ;
        public int Y ;

		public void Clear()
		{
			X = 0 ;
			Z = 0 ;
			Y = 0 ;
		}

		public override string ToString()
		{
			return "( " + X + ", " + Z + ", " + Y + " )" ;
		}
    }
}
