using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBS
{
	public class DictionaryUtility
	{
		public static T[] Sort<T>( ref Dictionary<T,int> d, bool tMode, bool tKeyOut = false )
		{
			if( d.Count == 0 )
			{
				return null ;
			}

			if( d.Count >= 2 )
			{
				List<KeyValuePair<T,int>> p = new List<KeyValuePair<T,int>>( d ) ;

				if( tMode == true )
				{
					p.Sort( ( a, b ) => ( a.Value - b.Value ) ) ;
				}
				else
				{
					p.Sort( ( a, b ) => ( b.Value - a.Value ) ) ;
				}

				d.Clear() ;

				for( int i  = 0 ; i <  p.Count ; i ++ )
				{
					d.Add( p[ i ].Key, p[ i ].Value ) ;
				}
			}

			//----------------------------------------------------------

			T[] tKey = null ;

			if( tKeyOut == true  )
			{
				int l = d.Keys.Count ;
				tKey = new T[ l ] ;
				d.Keys.CopyTo( tKey, 0 ) ;
			}

			return tKey ;
		}

		public static T[] Sort<T>( ref Dictionary<T,float> d, bool tMode, bool tKeyOut = false )
		{
			if( d.Count == 0 )
			{
				return null ;
			}

			if( d.Count >= 2 )
			{
				List<KeyValuePair<T,float>> p = new List<KeyValuePair<T,float>>( d ) ;

				if( tMode == true )
				{
					p.Sort( ( a, b ) => ( ( int )( a.Value - b.Value ) ) ) ;
				}
				else
				{
					p.Sort( ( a, b ) => ( ( int )( b.Value - a.Value ) ) ) ;
				}

				d.Clear() ;

				for( int i  = 0 ; i <  p.Count ; i ++ )
				{
					d.Add( p[ i ].Key, p[ i ].Value ) ;
				}
			}

			//----------------------------------------------------------

			T[] tKey = null ;

			if( tKeyOut == true  )
			{
				int l = d.Keys.Count ;
				tKey = new T[ l ] ;
				d.Keys.CopyTo( tKey, 0 ) ;
			}

			return tKey ;
		}

		public static T[] Sort<T>( ref Dictionary<T,double> d, bool tMode, bool tKeyOut = false )
		{
			if( d.Count == 0 )
			{
				return null ;
			}

			if( d.Count >= 2 )
			{
				List<KeyValuePair<T,double>> p = new List<KeyValuePair<T,double>>( d ) ;

				if( tMode == true )
				{
					p.Sort( ( a, b ) => ( ( int )( a.Value - b.Value ) ) ) ;
				}
				else
				{
					p.Sort( ( a, b ) => ( ( int )( b.Value - a.Value ) ) ) ;
				}

				d.Clear() ;

				for( int i  = 0 ; i <  p.Count ; i ++ )
				{
					d.Add( p[ i ].Key, p[ i ].Value ) ;
				}
			}

			//----------------------------------------------------------

			T[] tKey = null ;

			if( tKeyOut == true  )
			{
				int l = d.Keys.Count ;
				tKey = new T[ l ] ;
				d.Keys.CopyTo( tKey, 0 ) ;
			}

			return tKey ;
		}
	}
}
