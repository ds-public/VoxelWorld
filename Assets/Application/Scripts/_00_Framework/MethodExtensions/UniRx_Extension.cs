using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx ;

namespace DBS
{
	/// <summary>
	/// UniRx のメソッド拡張
	/// </summary>
	public static class UniRx
	{
		/// <summary>
		/// UniRx のコルーチン呼び出しを StartCoroutine ぽく書けるラッパー
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static ObservableYieldInstruction<Unit> StartCoroutine( IEnumerator e )
		{
			return Observable.FromCoroutine( _ => e )
				.ToYieldInstruction() ;
		}
	}
}
