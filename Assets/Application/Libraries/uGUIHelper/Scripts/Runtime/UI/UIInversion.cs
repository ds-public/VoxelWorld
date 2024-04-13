using System ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

namespace uGUIHelper
{
	/// <summary>
	/// 画像を上下左右に反転・左右に回転するコンポーネント
	/// </summary>
	public class UIInversion : BaseMeshEffect
	{
/*
#if UNITY_EDITOR
		[MenuItem( "Tools/UIInversion/FieldRefactor" )]
		private static void FieldRefactor()
		{
			int c = 0 ;
			UIInversion[] views = UIEditorUtility.FindComponents<UIInversion>
			(
				"Assets/Application",
				( _ ) =>
				{
					_.m_DirectionType = ( DirectionTypes )_.m_Direction ;

					c ++ ;
				}
			) ;
			Debug.LogWarning( "------> UIInversionの数:" + c ) ;
		}
#endif
*/

		[SerializeField][HideInInspector]
		private   RectTransform	m_RectTransform ;

		protected RectTransform	  RectTransform
		{
			get
			{
				if( m_RectTransform != null )
				{
					return m_RectTransform ;
				}
				m_RectTransform = GetComponent<RectTransform>() ;
				return m_RectTransform ;
			}
		}

		/// <summary>
		/// 反転方向の種類
		/// </summary>
		public enum DirectionTypes
		{
			None,
			Horizontal,
			Vertical,
			Both,
		}
		
		[SerializeField]
		private DirectionTypes m_DirectionType = DirectionTypes.None ;

		/// <summary>
		/// 反転方向
		/// </summary>
		public DirectionTypes DirectionType
		{
			get
			{
				return m_DirectionType ;
			}
			set
			{
				if( m_DirectionType != value )
				{
					m_DirectionType  = value ;
					Refresh() ;
				}
			}
		}

		/// <summary>
		/// 回転方向の種類
		/// </summary>
		public enum RotationTypes
		{
			None,
			R90,
			L90,
		}
		
		[SerializeField]
		private RotationTypes m_RotationType = RotationTypes.None ;

		/// <summary>
		/// 回転方向
		/// </summary>
		public RotationTypes RotationType
		{
			get
			{
				return m_RotationType ;
			}
			set
			{
				if( m_RotationType != value )
				{
					m_RotationType  = value ;
					Refresh() ;
				}
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// メッシュを独自に操作する
		/// </summary>
		/// <param name="helper"></param>
		public override void ModifyMesh( VertexHelper helper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			var list = new List<UIVertex>() ;
			helper.GetUIVertexStream( list ) ;
			
			if( ( m_DirectionType != DirectionTypes.None || m_RotationType != RotationTypes.None ) && RectTransform != null )
			{
				ModifyVertices( list ) ;
			}
				
			helper.Clear() ;
			helper.AddUIVertexTriangleStream( list ) ;
		}

		// メッシュの頂点を操作する
		private void ModifyVertices( List<UIVertex> list )
		{
			if( IsActive() == false || list == null || list.Count == 0 )
			{
				return ;
			}
			
			UIVertex v ;

			//----------------------------------------------------------
			// 重要
			//
			// 頂点座標は矩形の四隅まで存在するのではなく、
			// 画像が存在する範囲にしか存在しない。
			//
			// よって、ＵＶ操作では反転を行う事は出来ない。
			// 頂点の操作によって、反転・回転を行う必要がある。
			//
			//----------------------------------------------------------

			float w = RectTransform.rect.width ;
			float h = RectTransform.rect.height ;

			if( w <= 0 || h <= 0 )
			{
				// 処理不可
				return ;
			}

			//----------------------------------------------------------

			// Pivot に関係なく矩形の中心を基準位置とする補正値を算出する
			float cx = w * ( RectTransform.pivot.x - 0.5f ) ;
			float cy = h * ( RectTransform.pivot.y - 0.5f ) ;
			// Pivot が 0 → - ハーフサイズ
			// Pivot が 1 → + ハーフサイズ
			// 頂点差表に加算する事で Pivot 関係なく矩形の中心を基準位置とする相対位置に変わる


			int i, l = list.Count ;
			float rx, ry ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				v = list[ i ] ;

				// Pivot に関係無く矩形の中心を基準位置とした相対座標に変える
				float x = v.position.x + cx ;
				float y = v.position.y + cy ;

				switch( m_DirectionType )
				{
					// 左右のみ
					case DirectionTypes.Horizontal :
						x = -x ;
					break ;

					// 上下のみ
					case DirectionTypes.Vertical :
						y = -y ;
					break ;

					// 左右上下
					case DirectionTypes.Both :
						x = -x ;
						y = -y ;
					break ;
				}

				switch( m_RotationType )
				{
					// 右回転
					case RotationTypes.R90 :
						rx = +y * w / h ;
						ry = -x * h / w ;
						x = rx ;
						y = ry ;
					break ;

					// 左回転
					case RotationTypes.L90 :
						rx = -y * w / h ;
						ry = +x * h / w ;
						x = rx ;
						y = ry ;
					break ;
				}

				// 再び Pivot を基準とした座標系に戻す
				v.position.x = x - cx ;
				v.position.y = y - cy ;

				list[ i ] = v ;
			}
		}

		/// <summary>
		/// メッシュを更新する
		/// </summary>
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}
	}
}
