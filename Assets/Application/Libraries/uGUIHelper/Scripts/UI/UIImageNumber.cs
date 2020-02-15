using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( ImageNumber ) ) ]
	public class UIImageNumber : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color Color
		{
			get
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return Color.white ;
				}
				return imageNumber.color ;
			}
			set
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material Material
		{
			get
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return null ;
				}
				return imageNumber.material ;
			}
			set
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.material = value ;
			}
		}

		/// <summary>
		/// 値(ショートカット)
		/// </summary>
		public int Value
		{
			get
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return 0 ;
				}
				return imageNumber.value ;
			}
			set
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.value = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.preferredWidth, imageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字のアンカー(ショートカット)
		/// </summary>
		public TextAnchor Alignment
		{
			get
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return 0 ;
				}
				return imageNumber.alignment ;
			}
			set
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.alignment = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.preferredWidth, imageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置
		/// </summary>
		public Vector3[] CodeOffset
		{
			get
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return null ;
				}
				return imageNumber.codeOffset ;
			}
			set
			{
				ImageNumber imageNumber = _imageNumber ;
				if( imageNumber == null )
				{
					return ;
				}
				imageNumber.codeOffset = value ;

				if( m_AutoSizeFitting == true )
				{
					SetSize( imageNumber.preferredWidth, imageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置の設定
		/// </summary>
		/// <param name="tCodeOffset"></param>
		public void SetCodeOffset( Vector3[] codeOffset )
		{
			ImageNumber imageNumber = _imageNumber ;
			if( imageNumber == null )
			{
				return ;
			}
			imageNumber.SetCodeOffset( codeOffset ) ;
		}

		/// <summary>
		/// ＵＩのサイズを文字のサイズに自動調整するかどうか
		/// </summary>
		[SerializeField]
		protected bool m_AutoSizeFitting = true ;
		public bool AutoSizeFitting{ get{ return m_AutoSizeFitting ; } set{ m_AutoSizeFitting = true ; } }


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			ImageNumber imageNumber = _imageNumber ;

			if( imageNumber == null )
			{
				imageNumber = gameObject.AddComponent<ImageNumber>() ;
			}
			if( imageNumber == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			imageNumber.color = Color.white ;
			imageNumber.atlasSprite = UIAtlasSprite.Create( "uGUIHelper/Textures/UIDefaultImageNumber" ) ;

			if( IsCanvasOverlay == true )
			{
				imageNumber.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
		}

		override protected void OnLateUpdate()
		{
			if( m_AutoSizeFitting == true )
			{
				ImageNumber t = _imageNumber ;
				RectTransform r = GetRectTransform() ;
				if( r != null && t != null )
				{
					Vector2 tSize = r.sizeDelta ;

					if( r.anchorMin.x == r.anchorMax.x )
					{
						tSize.x = t.preferredWidth ;
					}
					if( r.anchorMin.y == r.anchorMax.y )
					{
						tSize.y = t.preferredHeight ;
					}

					r.sizeDelta = tSize ;
				}
			}
		}	
	}
}

