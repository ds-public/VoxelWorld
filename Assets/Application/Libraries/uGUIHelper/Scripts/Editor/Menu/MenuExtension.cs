#if UNITY_EDITOR

using UnityEngine ;
using UnityEditor ;
using System ;
using System.Collections.Generic ;

using UnityEditorInternal ;

using uGUIHelper.InputAdapter ;


namespace uGUIHelper
{
	/// <summary>
	/// メニューに生成機能追加 Version 2026/04/01
	/// </summary>
	public static class MenuExtension
	{
		/// <summary>
		/// View 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a View" )]
		[MenuItem( "GameObject/uGUIHelper/View", false, 22 )]
		public static void AddView()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI View" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIView>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIView>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Graphic 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Graphic Empty" ) ]
		[MenuItem( "GameObject/uGUIHelper/Graphic Empty", false, 22 )]
		public static void AddGraphicEmpty()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Graphic Empty" ) ;	// アンドウバッファに登録

			var child = new GameObject( "Graphic Empty", typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var graphicEmpty = child.AddComponent<GraphicEmpty>() ;

			var view = child.AddComponent<UIView>() ;
			view.SetDefault() ;

			// GraphicEmpty は下に持ってくる
			while( ComponentUtility.MoveComponentDown( graphicEmpty ) ){}

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Space 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Space" ) ]
		[MenuItem( "GameObject/uGUIHelper/Space", false, 22 )]
		public static void AddSpace()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Space" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UISpace>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UISpace>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// TextMesh 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a TextMesh" )]
		[MenuItem( "GameObject/uGUIHelper/TextMesh", false, 22 )]
		public static void AddTextMesh()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI TextMesh" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UITextMesh>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UITextMesh>() ;
			view.SetDefault() ;
			view.Text = "TextMesh" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Number 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a NumberMesh" ) ]
		[MenuItem( "GameObject/uGUIHelper/NumberMesh", false, 22 )]
		public static void AddNumberMesh()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI NumberMesh" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UINumberMesh>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UINumberMesh>() ;
			view.SetDefault() ;
			view.Text = "NumberMesh" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ImageNumber 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a ImageNumber" ) ]
		[MenuItem( "GameObject/uGUIHelper/ImageNumber", false, 22 )]
		public static void AddImageNumber()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ImageNumber" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIImageNumber>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;

			var view = child.AddComponent<UIImageNumber>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Image 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Image" )]
		[MenuItem( "GameObject/uGUIHelper/Image", false, 22 )]
		public static void AddImage()
		{
			AddImage( string.Empty ) ;
		}

		public static void AddImage( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Image" ) ;	// アンドウバッファに登録
			
			string name = "Image" ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = type ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIImage>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// RawImage 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a RawImage" ) ]
		[MenuItem( "GameObject/uGUIHelper/RawImage", false, 22 )]
		public static void AddRawImage()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI RawImage" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIRawImage>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIRawImage>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// GridMap 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a GridMap" )]
		[MenuItem( "GameObject/uGUIHelper/GridMap", false, 22 )]
		public static void AddGridMap()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI GridMap" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIGridMap>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIGridMap>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ComplexRectangle 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a ComplexRectangle" )]
		[MenuItem( "GameObject/uGUIHelper/ComplexRectangle", false, 22 )]
		public static void AddComplexRectangle()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ComplexRectangle" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIComplexRectangle>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIComplexRectangle>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Line 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Line" )]
		[MenuItem( "GameObject/uGUIHelper/Line", false, 22 )]
		public static void AddLine()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Line" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UILine>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UILine>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Circle 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Circle" )]
		[MenuItem( "GameObject/uGUIHelper/Circle", false, 22 )]
		public static void AddCircle()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Circle" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UICircle>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UICircle>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Arc 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Arc" )]
		[MenuItem( "GameObject/uGUIHelper/Arc", false, 22 )]
		public static void AddArc()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Arc" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIArc>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIArc>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Button 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Button" )]
		[MenuItem( "GameObject/uGUIHelper/Button", false, 22 )]
		public static void AddButton()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Button" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIButton>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIButton>() ;
			view.SetDefault() ;

			view.AddLabelMesh( "Button", 0xFF000000 ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Toggle 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Toggle" )]
		[MenuItem( "GameObject/uGUIHelper/Toggle", false, 22 )]
		public static void AddToggle()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Toggle" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIToggle>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIToggle>() ;
			view.SetDefault() ;

			view.LabelMesh.Text = "Toggle" ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Slider(Horizontal) 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Slider/Add a Slider(Horizontal)" )]
		[MenuItem( "GameObject/uGUIHelper/Slider/Horizontal", false, 22 )]
		public static void AddSliderH()
		{
			AddSlider( "H" ) ;
		}

		[MenuItem( "uGUIHelper/Slider/Add a Slider(Vertical)" )]
		[MenuItem( "GameObject/uGUIHelper/Slider/Vertical", false, 22 )]
		public static void AddSliderV()
		{
			AddSlider( "V" ) ;
		}

		public static void AddSlider( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Slider" ) ;	// アンドウバッファに登録

			string name = GetName<UISlider>() ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = $"{name}({type})" ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UISlider>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Scrollbar(Horizontal) 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Scrollbar/Add a Scrollbar(Horizontal)" )]
		[MenuItem( "GameObject/uGUIHelper/Scrollbar/Horizontal", false, 22 )]
		public static void AddScrollbarH()
		{
			AddScrollbar( "H" ) ;
		}

		/// <summary>
		/// Scrollbar(Vertical) 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Scrollbar/Add a Scrollbar(Vertical)" )]
		[MenuItem( "GameObject/uGUIHelper/Scrollbar/Vertical", false, 22 )]
		public static void AddScrollbarV()
		{
			AddScrollbar( "V" ) ;
		}

		public static void AddScrollbar( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Scrollbar" ) ;	// アンドウバッファに登録

			string name = GetName<UIScrollbar>() ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = $"{name}({type})" ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIScrollbar>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}
#if false
		/// <summary>
		/// Dropdown 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Dropdown" )]
		[MenuItem( "GameObject/uGUIHelper/Dropdown", false, 22 ) ]
		public static void AddDropdown()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Dropdown" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIDropdown>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var dropdown = child.AddComponent<UIDropdown>() ;
			dropdown.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}
#endif
		/// <summary>
		/// Pulldown 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Pulldown" )]
		[MenuItem( "GameObject/uGUIHelper/Pulldown", false, 22 ) ]
		public static void AddPulldown()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Pulldown" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIPulldown>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var dropdown = child.AddComponent<UIPulldown>() ;
			dropdown.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// InputField 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a InputField(S)" )]
		[MenuItem( "GameObject/uGUIHelper/InputField(S)", false, 22 ) ]
		public static void AddInputFieldSingle()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI InputField" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIInputField>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIInputField>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// InputField 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a InputField(M)" )]
		[MenuItem( "GameObject/uGUIHelper/InputField(M)", false, 22 )]
		public static void AddInputFieldMulti()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI InputField" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIInputField>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIInputField>() ;
			view.SetDefault( "MultiLine" ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Progressbar 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Progressbar/Add a Progressbar(Rectangle)" )]
		[MenuItem( "GameObject/uGUIHelper/Progressbar/Rectangle", false, 22 )]
		public static void AddProgressbarR()
		{
            AddProgressbar( "Rectangle" ) ;
        }

		/// <summary>
		/// Progressbar 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Progressbar/Add a Progressbar(Circle)" )]
		[MenuItem( "GameObject/uGUIHelper/Progressbar/Circle", false, 22 )]
		public static void AddProgressbarC()
		{
            AddProgressbar( "Circle" ) ;
        }

		public static void AddProgressbar( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI Progressbar" ) ;	// アンドウバッファに登録

			string name = GetName<UIProgressbar>() ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = $"{name}({type})" ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIProgressbar>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Canvas 生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Canvas" )]
		[MenuItem( "GameObject/uGUIHelper/Canvas", false, 22 )]
		public static void AddCanvas()
		{
			Transform parent = null ;

			var go = Selection.activeGameObject ;
			if( go != null )
			{
				if( WillLosePrefab( go ) == false )
				{
					return ;
				}

				parent = go.transform ;

				Undo.RecordObject( go, "Add a child UI Canvas" ) ; // アンドウバッファに登録
			}

			var canvas = UICanvas.Create( parent, 800, 600 ) ;

			Selection.activeGameObject = canvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Camera を子として持つ Canvas を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Canvas With Camera" )]
		[MenuItem( "GameObject/uGUIHelper/Canvas With Camera", false, 22 )]
		public static void AddCanvasWithCamera()
		{
			Transform parent = null ;

			var go = Selection.activeGameObject ;
			if( go != null )
			{
				if( WillLosePrefab( go ) == false )
				{
					return ;
				}

				parent = go.transform ;

				Undo.RecordObject( go, "Add a child UI Canvas With Camera" ) ; // アンドウバッファに登録
			}

			var canvas = UICanvas.CreateWithCamera( parent, 800, 600 ) ;

			Selection.activeGameObject = canvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Camera を親として持つ Canvas を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Canvas On Camera" )]
		[MenuItem( "GameObject/uGUIHelper/Canvas On Camera", false, 22 )]
		public static void AddCanvasOnCamera()
		{
			Transform parent = null ;

			var go = Selection.activeGameObject ;
			if( go != null )
			{
				if( WillLosePrefab( go ) == false )
				{
					return ;
				}

				parent = go.transform ;

				Undo.RecordObject( go, "Add a child UI Canvas On Camera" ) ;   // アンドウバッファに登録
			}

			var canvas = UICanvas.CreateOnCamera( parent, 800, 600 ) ;

			Selection.activeGameObject = canvas.gameObject ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Panel(Image) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a Panel" )]
		[MenuItem( "GameObject/uGUIHelper/Panel", false, 22 ) ]
		public static void AddPanel()
		{
			AddImage( "Panel" ) ;
		}

		/// <summary>
		/// ScrollView(Normal) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Basic)" )]
		[MenuItem( "GameObject/uGUIHelper/ScrollView/Basic", false, 22 )]
		public static void AddScrollView()
		{
			AddScrollView( string.Empty ) ;
		}

		/// <summary>
		/// ScrollView(Horizontal) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Horizontal)" )]
		[MenuItem( "GameObject/uGUIHelper/ScrollView/Horizontal", false, 22 )]
		public static void AddScrollViewSH()
		{
			AddScrollView( "SH" ) ;
		}

		/// <summary>
		/// ScrollView(Vertical) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/ScrollView/Add a ScrollView(Vertical)" ) ]
		[MenuItem( "GameObject/uGUIHelper/ScrollView/Vertical", false, 22 )]
		public static void AddScrollViewSV()
		{
			AddScrollView( "SV" ) ;
		}

		private static void AddScrollView( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ScrollView" ) ;	// アンドウバッファに登録

			string name = GetName<UIScrollView>() ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = $"{name}({type})" ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIScrollView>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// ListView(Horizontal) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/ListView/Add a ListView(Horizontal)" )]
		[MenuItem( "GameObject/uGUIHelper/ListView/Horizontal", false, 22 )]
		public static void AddListViewH()
		{
			AddListView( "HS" ) ;
		}

		/// <summary>
		/// ListView(Vertical) を生成
		/// </summary>
		[MenuItem( "uGUIHelper/ListView/Add a ListView(Vertical)" )]
		[MenuItem( "GameObject/uGUIHelper/ListView/Vertical", false, 22 )]
		public static void AddListViewV()
		{
			AddListView( "VS" ) ;
		}


		private static void AddListView( string type )
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI ListView" ) ;	// アンドウバッファに登録

			string name = GetName<UIListView>() ;
			if( string.IsNullOrEmpty( type ) == false )
			{
				name = $"{name}({type})" ;
			}

			var child = new GameObject( name, typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIListView>() ;
			view.SetDefault( type ) ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// PadButton を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a PadButton" )]
		[MenuItem( "GameObject/uGUIHelper/PadButton", false, 22 )]
		public static void AddPadButton()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI PadButton" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIPadButton>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIPadButton>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// PadAxis を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a PadAxis" )]
		[MenuItem( "GameObject/uGUIHelper/PadAxis", false, 22 )]
		public static void AddPadAxis()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI PadAxis" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIPadAxis>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIPadAxis>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// PadFocusController を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Add a PadFocusController" )]
		[MenuItem( "GameObject/uGUIHelper/PadFocusController", false, 22 )]
		public static void AddPadFocusController()
		{
			var go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child UI PadFocusController" ) ;	// アンドウバッファに登録

			var child = new GameObject( GetName<UIPadFocusController>(), typeof( RectTransform ) ) ;

			var t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var view = child.AddComponent<UIPadFocusController>() ;
			view.SetDefault() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// EventSyetem を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Create a EventSystem" )]
		[MenuItem( "GameObject/uGUIHelper/EventSystem", false, 22 )]
		public static void CreateEventSystem()
		{
			var parent = Selection.activeGameObject ;

			if( parent != null )
			{
				if( WillLosePrefab( parent ) == false )
				{
					return ;
				}

				Undo.RecordObject( parent, "Create a EventSystem" ) ;	// アンドウバッファに登録
			}

			var child = new GameObject( GetName<UIEventSystem>() ) ;

			var t = child.transform ;

			if( parent != null )
			{
				t.SetParent( parent.transform, false ) ;
			}
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			child.AddComponent<UIEventSystem>() ;

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// Scene を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Create a Scene" )]
		[MenuItem( "GameObject/uGUIHelper/Scene", false, 22 ) ]
		public static void CreateScene()
		{
			var go = new GameObject( "Scene" ) ;

			var t = go.transform ;
			t.SetParent( null ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			Selection.activeGameObject = go ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

		/// <summary>
		/// SpriteAnimation を生成
		/// </summary>
		[MenuItem( "uGUIHelper/Create a New SpriteAnimation" )]
		[MenuItem( "GameObject/uGUIHelper/SpriteAnimation", false, 22 )]
		[MenuItem( "Assets/Create/uGUIHelper/SpriteAnimation" )]
		static public void CreateSpriteAnimation()
		{
			string path = "Assets/" ;
			string name = "A New SpriteAnimation" ;

			if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
			{
				if( Selection.activeObject.GetType() != typeof( GameObject ) )
				{
					path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;

					string checkPath = path.Replace( "/", "\\" ) ;
					if( System.IO.File.Exists( checkPath ) == true )
					{
						// ファイルなのでフォルダ名を取得する

						// 最後のフォルダ区切り位置を取得する
						int s = path.LastIndexOf( '/' ) ;

						path = path[ ..s ] ;
					}

					path += "/" ;
				}
			}

			path = path + name + ".asset" ;

			var spriteAnimation = AssetDatabase.LoadAssetAtPath( path, typeof( UISpriteAnimation ) ) as UISpriteAnimation ;
			if( spriteAnimation != null )
			{
				// 既にアセットが存在する
				Selection.activeObject = spriteAnimation ;

				return ;
			}

			spriteAnimation = ScriptableObject.CreateInstance<UISpriteAnimation>() ;
			spriteAnimation.name = name ;

			AssetDatabase.CreateAsset( spriteAnimation, path ) ;
			AssetDatabase.Refresh() ;

			Selection.activeObject = spriteAnimation ;
		}

		//-------------------------------------------------

		private static bool WillLosePrefab( GameObject root )
		{
			if( root == null )
			{
				return false ;
			}

			if( root.transform != null )
			{
				PrefabAssetType type = PrefabUtility.GetPrefabAssetType( root ) ;

				if( type != PrefabAssetType.NotAPrefab )
				{
					return EditorUtility.DisplayDialog( "Losing prefab", "This action will lose the prefab connection. Are you sure you wish to continue?", "Continue", "Cancel" ) ;
				}
			}
			return true ;
		}

		private static string GetName<T> () where T : Component
		{
			string s = typeof( T ).ToString() ;

			int i ;

			i = s.IndexOf( "." ) ;
			if( i >= 0 )
			{
				s = s[ ( i + 1 ).. ] ;
			}

			i = s.IndexOf( "UI" ) ;
			if( i >= 0 )
			{
				s = s[ ( i + 2 ).. ] ;
			}

			return s ;
		}
	}
}

#endif
