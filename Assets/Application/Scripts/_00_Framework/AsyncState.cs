using System ;
using UnityEngine ;

namespace DBS
{
	/// <summary>
	/// ダイアログの表示状態クラス
	/// </summary>
	public class AsyncState : CustomYieldInstruction
	{
		public override bool keepWaiting
		{
			get
			{
				if( IsDone == false && string.IsNullOrEmpty( Error ) == true )
				{
					return true ;    // 継続
				}
				return false ;   // 終了
			}
		}

		/// <summary>
		/// 通信が終了したかどうか
		/// </summary>
		public bool IsDone ;

		/// <summary>
		/// エラーが発生したかどうか
		/// </summary>
		public string Error = string.Empty ;
		
		/// <summary>
		/// 結果値
		/// </summary>
		public System.Object	Result ;

		/// <summary>
		/// 任意の型にキャストして結果を取得する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetResult<T>( T defaultValue = default )
		{
			if( Result == null )
			{
				return defaultValue ;
			}

			return ( T )Result ;
		}

		public T GetCurrent<T>( T defaultValue = default )
		{
			if( Result == null )
			{
				return defaultValue ;
			}

			return ( T )Result ;
		}
	}
}

