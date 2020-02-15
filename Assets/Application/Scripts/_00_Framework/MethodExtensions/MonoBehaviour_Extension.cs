using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx ;

namespace DBS
{
	/// <summary>
	/// MonoBehaviour のメソッド拡張
	/// </summary>
	public static class MonoBehaviour_Extension
	{
		/// <summary>
		/// UniRx のコルーチン呼び出しを StartCoroutine ぽく書けるラッパー
		/// </summary>
		/// <returns>The start coroutine.</returns>
		/// <param name="m">M.</param>
		/// <param name="e">E.</param>
/*		public static ObservableYieldInstruction<Unit> RxStartCoroutine( this MonoBehaviour m, IEnumerator e )
		{
			return Observable.FromCoroutine( _ => e )
				.ToYieldInstruction() ;
		}*/
	}
}

