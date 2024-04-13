using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// 描画を行わないが RaycastTarget で反応取りたい時に使用するコンポーネント
	/// </summary>
	[ RequireComponent( typeof( RectTransform ) ) ]
	[ RequireComponent( typeof( CanvasRenderer ) ) ]
	public class GraphicEmpty : Graphic
	{
		public override void SetMaterialDirty(){ return ; }
		public override void SetVerticesDirty(){ return ; }

		protected override void Awake()
		{
			base.Awake() ;

			if( TryGetComponent<CanvasRenderer>( out var canvasRenderer ) == true )
			{
				// 透明の場合は描画をスルーする
				canvasRenderer.cullTransparentMesh = true ;
			}
		}

		protected override void OnPopulateMesh( VertexHelper vh )
		{
			vh.Clear() ;
			return ;
		}
	}
}

