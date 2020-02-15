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
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Dropdown ) ) ]
	public class UIDropdown : UIImage
	{
		/// <summary>
		/// captionText(ショートカット)
		/// </summary>
		public UIText CaptionText
		{
			get
			{
				if( _dropdown == null )
				{
					return null ;
				}

				return _dropdown.captionText.GetComponent<UIText>() ;
			}
		}


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string option = "" )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				dropdown = gameObject.AddComponent<Dropdown>() ;
			}
			if( dropdown == null )
			{
				// 異常
				return ;
			}

			Image image = _image ;

			//------------------------------------------

			Vector2 size = GetCanvasSize() ;
			if( size.x >  0 && size.y >  0 )
			{
				SetSize( size.y * 0.28f, size.y * 0.05f ) ;
			}
				
			ColorBlock colorBlock = dropdown.colors ;
			colorBlock.fadeDuration = 0.1f ;
			dropdown.colors = colorBlock ;
				
			image.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			image.color = Color.white ;
			image.type = Image.Type.Sliced ;

			// 初期のオプションを追加する
			Dropdown.OptionData dataA = new Dropdown.OptionData()
			{
				text = "Option A"
			} ;
			dropdown.options.Add( dataA ) ;
			Dropdown.OptionData dataB = new Dropdown.OptionData()
			{
				text = "Option B"
			} ;
			dropdown.options.Add( dataB ) ;
			Dropdown.OptionData dataC = new Dropdown.OptionData()
			{
				text = "Option C"
			} ;
			dropdown.options.Add( dataC ) ;

			// Label
			UIText label = AddView<UIText>( "Label", "FitOff" ) ;
			label.Text = dropdown.options[ 0 ].text ;
			label.SetAnchorToStretch() ;
			label.SetMargin( 10, 25,  7,  6 ) ;
			label._text.alignment = TextAnchor.MiddleLeft ;
//			label._text.fontSize = 0 ;
			label._text.color = new Color32(  50,  50,  50, 255 ) ;

			dropdown.captionText = label._text ;


			// Arrow
			UIImage arrow = AddView<UIImage>( "Arrow" ) ;
			arrow.Sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultArrowDown" ) ;
			arrow.Color = Color.white ;
			arrow.Type = Image.Type.Sliced ;
				
			arrow.SetAnchorToRightMiddle() ;
			arrow.SetPosition( -18,   0 ) ;
			float s = this.Height * 0.6f ;
			arrow.SetSize( s, s ) ;


			// ScrollView
			UIScrollView scrollView = AddView<UIScrollView>( "Template", "Dropdown" ) ;
			scrollView.SetAnchorToStretchBottom() ;
			scrollView.SetPosition(  0,  2 ) ;
			scrollView.SetSize(   0, this.Height * 5 ) ;
			scrollView.SetPivot( 0.5f, 1.0f ) ;
	//		scrollView.SetColor( 0xFFFFFFFF ) ;
			scrollView.isVerticalScrollber = true ;
			scrollView._scrollRect.verticalScrollbarSpacing = -2 ;

			scrollView.content.Height = this.Height ;

			dropdown.template = scrollView.GetRectTransform() ;

			// テンプレートアイテムを１つ追加する
			scrollView.dropdownItem.Height = this.Height ;

			// 最後に無効化
			scrollView.SetActive( false ) ;
				
			ResetRectTransform() ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _dropdown != null )
				{
					_dropdown.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIDropdown, int> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChanged( string identity, UIDropdown view, int value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChanged OnValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIDropdown, int> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChanged onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}
	
		// 内部リスナー登録
		private void OnValueChangedInner( int value )
		{
			if( OnValueChangedAction != null || OnValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValueChangedAction?.Invoke( identity, this, value ) ;
				OnValueChangedDelegate( identity, this, value ) ;
			}
		}
	
		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<int> onValueChanged )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown != null )
			{
				dropdown.onValueChanged.AddListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<int> onValueChanged )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown != null )
			{
				dropdown.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown != null )
			{
				dropdown.onValueChanged.RemoveAllListeners() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// オプションデータをまとめて設定する
		/// </summary>
		/// <param name="tName"></param>
		public bool Set( string[] dataTexts, int initailValue = -1 )
		{
			if( dataTexts == null )
			{
				return false ;
			}

			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				return false ;
			}

			int activeValue = dropdown.value ;

			dropdown.value = 0 ;
			dropdown.ClearOptions() ;

			int i, l = dataTexts.Length ;
			Dropdown.OptionData data ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				data = new Dropdown.OptionData()
				{
					text = dataTexts[ i ]
				} ;
				dropdown.options.Add( data ) ;
			}

			dropdown.captionText.text = dropdown.options[ _dropdown.value ].text ;

			// デフォルトカーソル位置
			if( initailValue >= 0 )
			{
				activeValue  = initailValue ;
			}

			dropdown.value  = activeValue ;

			return true ;
		}

		/// <summary>
		/// オプションデータを追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public bool Add( string dataText )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				return false ;
			}

			Dropdown.OptionData data = new Dropdown.OptionData()
			{
				text = dataText
			} ;
			dropdown.options.Add( data ) ;

			return true ;
		}

		/// <summary>
		/// オプションデータを挿入する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <param name="tName"></param>
		/// <returns></returns>
		public bool Insert( int index, string dataText )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				return false ;
			}

			Dropdown.OptionData data = new Dropdown.OptionData()
			{
				text = dataText
			} ;
			dropdown.options.Insert( index, data ) ;

			return true ;
		}

		/// <summary>
		/// オプションデータを削除する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <returns></returns>
		public bool RemoveAt( int index )
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				return false ;
			}

			dropdown.options.RemoveAt( index ) ;

			return true ;
		}

		/// <summary>
		/// カーソル位置
		/// </summary>
		public int Value
		{
			get
			{
				Dropdown dropdown = _dropdown ;
				if( dropdown == null )
				{
					return 0 ;
				}
				return dropdown.value ;
			}
			set
			{
				Dropdown dropdown = _dropdown ;
				if( dropdown == null )
				{
					return ;
				}

				if( value >= 0 && value <  dropdown.options.Count )
				{
					dropdown.value = value ;
				}
			}
		}

		/// <summary>
		/// 現在のカーソル位置の項目名
		/// </summary>
		public string DataText
		{
			get
			{
				Dropdown dropdown = _dropdown ;
				if( dropdown == null )
				{
					return "" ;
				}

				if( dropdown.options != null && dropdown.value >= 0 && dropdown.value <   dropdown.options.Count )
				{
					return dropdown.options[ dropdown.value ].text ;
				}

				return "" ;
			}
			set
			{
				Dropdown dropdown = _dropdown ;
				if( dropdown == null )
				{
					return ;
				}

				if( dropdown.options != null && dropdown.value >= 0 && dropdown.value <  dropdown.options.Count )
				{
					dropdown.options[ dropdown.value ].text = value ;
				}
			}
		}

		/// <summary>
		/// 項目一覧を取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetDataTexts()
		{
			Dropdown dropdown = _dropdown ;
			if( dropdown == null )
			{
				return null ;
			}

			if( dropdown.options == null || dropdown.options.Count == 0 )
			{
				return null ;
			}

			int i, l = dropdown.options.Count ;

			string[] dataTexts = new string[ l ] ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				dataTexts[ i ] = dropdown.options[ i ].text ;
			}

			return dataTexts ;
		}

		/// <summary>
		/// インデクサ(項目一覧へのショートカットアクセス)
		/// </summary>
		public string this[ int index ]
		{
			get
			{
				Dropdown dropdown = _dropdown ;
				if( dropdown == null )
				{
					return null ;
				}

				if( dropdown.options != null && dropdown.value >= 0 && dropdown.value <   dropdown.options.Count )
				{
					return dropdown.options[ index ].text ;
				}

				return null ;
			}
		}
	}
}
