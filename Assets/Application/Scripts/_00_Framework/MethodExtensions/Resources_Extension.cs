using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// Resources 型のメソッド拡張
	/// </summary>
	public static class Resources_Extension
	{
		/// <summary>
		/// 配列が null もしくは要素数が 0 の時に true を返す
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static Dictionary<string,Sprite> LoadAllSprites( string path )
		{
			Sprite[] sprites = Resources.LoadAll<Sprite>( path ) ;
			if( sprites == null || sprites.Length == 0 )
			{
				return null ;
			}
			
			Dictionary<string,Sprite> list = new Dictionary<string, Sprite>() ;
			
			foreach( Sprite sprite in sprites )
			{
				list.Add( sprite.name, sprite ) ;
			}
			
			return list ;
		}
	}
}

