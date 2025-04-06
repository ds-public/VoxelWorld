using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;


namespace SpriteHelper
{
	/// <summary>
	/// マルチタイプのスプライト管理用のクラス Version 2025/04/06
	/// </summary>
	[Serializable]
	public class SpriteSet
	{
		/// <summary>
		/// スプライト内のテクスチャ
		/// </summary>
		public  Texture2D   Texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				m_Texture = value ;
			}
		}

		[SerializeField][HideInInspector]
		private Texture2D m_Texture ;

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<Sprite> m_Sprites ;

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<string> m_Names ;

		//-----------------------------------

		// 個々のスプライトの参照高速化のためのインデックス(ハッシュ)
		private Dictionary<string, Sprite> m_Indices ;


		//-----------------------------------------------------------

		/// <summary>
		/// スプライト情報の展開の有無
		/// </summary>
		public bool Exist
		{
			get
			{
				if( m_Sprites != null && m_Sprites.Count >  0 )
				{
					return true ;
				}

				return false ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// アトラスタイプのスプライトをセットする
		/// </summary>
		/// <param name="spriteList"></param>
		public void SetSprites( Sprite[] sprites )
		{
			m_Sprites = new List<Sprite>() ;
			m_Names = new List<string>() ;

			if( m_Indices == null )
			{
				m_Indices = new Dictionary<string, Sprite>() ;
			}
			else
			{
				m_Indices.Clear() ;
			}

			int i, l = sprites.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Sprites.Add( sprites[ i ] ) ;
				m_Names.Add( sprites[ i ].name ) ;

				m_Indices.Add( sprites[ i ].name, sprites[ i ] ) ;
			}

			// テクスチャはどこも共通
			m_Texture = sprites[ 0 ].texture ;
		}



		/// <summary>
		/// 全ての有効なスプライトを取得する
		/// </summary>
		/// <returns></returns>
		public Sprite[] GetSprites()
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			CreateIndicesIfEmpty() ;

			var sprites = new List<Sprite>() ;

			int i, l = m_Sprites.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null )
				{
					sprites.Add( m_Sprites[ i ] ) ;
				}
				else
				{
					// 異常発生
				}
			}

			return sprites.ToArray() ;
		}

		/// <summary>
		/// アトラスタイプのスプライト情報をクリアする
		/// </summary>
		public void ClearSprites()
		{
			m_Texture	= null ;
			m_Sprites	= null ;
			m_Names		= null ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定した名前のスプライトを取得する
		/// </summary>
		/// <param name="spriteName"></param>
		/// <returns></returns>
		public Sprite GetSprite( string spriteName )
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			CreateIndicesIfEmpty() ;

			if( m_Indices.ContainsKey( spriteName ) == false )
			{
				return null ;
			}

			return m_Indices[ spriteName ] ;
		}

		// スプライトのインデックス(ハッシュ)を生成する(個々のスプライトの参照の高速化のため)
		private void CreateIndicesIfEmpty()
		{
			if( m_Indices != null )
			{
				return ;
			}

			//----------------------------------

			if( m_Indices == null )
			{
				m_Indices = new Dictionary<string, Sprite>() ;
			}
			else
			{
				m_Indices.Clear() ;
			}

			int i, l = m_Sprites.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null )
				{
					m_Indices.Add( m_Sprites[ i ].name, m_Sprites[ i ] ) ;
				}
			}
		}


		/// <summary>
		/// インデクサを使ってアクセスを簡易化
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Sprite this[ string spriteName ]
		{
			get
			{
				return GetSprite( spriteName ) ;
			}
		}

		/// <summary>
		/// アトラスタイプのスプライトの名前リストを取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNames()
		{
			if( m_Sprites == null || m_Sprites.Count == 0 )
			{
				return null ;
			}

			int i, l = m_Sprites.Count ;

			var names = new List<string>() ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Sprites[ i ] != null )
				{
					names.Add( m_Sprites[ i ].name ) ;
				}
				else
				{
					// 異常発生
				}
			}

			return names.ToArray() ;
		}

		/// <summary>
		/// 要素の数を返す
		/// </summary>
		/// <returns></returns>
		public int SpriteCount
		{
			get
			{
				if( m_Sprites == null )
				{
					return 0 ;
				}
	
				return m_Sprites.Count ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// データに異常が無く使用可能な状態にあるか
		/// </summary>
		public bool IsAvailable
		{
			get
			{
				if( m_Sprites == null || m_Names == null )
				{
					return false ;
				}

				if( m_Sprites.Count != m_Names.Count )
				{
					return false ;
				}

				int i, l = m_Sprites.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Sprites[ i ] == null || m_Names[ i ] == null )
					{
						return false ;
					}

					if( m_Sprites[ i ].name != m_Names[ i ] )
					{
						return false ;
					}
				}

				return true ;
			}
		}
	}
}
