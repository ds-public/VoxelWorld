using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using uGUIHelper ;

namespace DSW
{
	/// <summary>
	/// ブラー効果の補助コンポーネント
	/// </summary>
	public class BlurAuxiliary : MonoBehaviour
	{
		internal void Awake()
		{
			if( TryGetComponent<UIRawImage>( out var rawImage ) == true )
			{
				// ブラー処理時にリップルの表示のオンオフを行う
				rawImage.SetOnBlurProcessing( ( bool isProcessing ) =>
				{
					if( isProcessing == true )
					{
						// ブラーの処理が開始するのでタッチエフェクトの表示を禁止する
						Ripple.Off() ;
					}
					else
					{
						// ブラーの処理が終了したのでタッチエフェクトの表示を許可する
						Ripple.On() ;
					}
				} ) ;
			}
		}
	}
}

