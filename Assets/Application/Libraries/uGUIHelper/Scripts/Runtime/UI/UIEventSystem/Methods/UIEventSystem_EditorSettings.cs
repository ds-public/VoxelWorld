using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


namespace uGUIHelper.InputAdapter
{
	//--------------------------------------------------------------------------------------------
	// InputManager(旧システム)用のゲームパッド情報の登録

#if UNITY_EDITOR

	/// <summary>
	/// InputManager を設定するクラス(Editor専用)
	/// </summary>
	public class InputManagerSettings
	{
		public enum AxisType	
		{
			KeyOrMouseButton	= 0,
			MouseMovement		= 1,
			JoystickAxis		= 2,
		} ;

		public class Axis
		{
			public string	Name					= "" ;
			public string	DescriptiveName			= "" ;
			public string	DescriptiveNegativeName	= "" ;
			public string	NegativeButton			= "" ;
			public string	PositiveButton			= "" ;
			public string	AltNegativeButton		= "" ;
			public string	AltPositiveButton		= "" ;
	
			public float	Gravity					= 0 ;
			public float	Dead					= 0 ;
			public float	Sensitivity				= 0 ;
	
			public bool		Snap					= false ;
			public bool		Invert					= false ;
		
			public AxisType	Type					= AxisType.KeyOrMouseButton ;
	
			public int		AxisNum					= 1 ;
			public int		JoyNum					= 0 ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 設定する
		/// </summary>
		[ MenuItem( "Tools/Initialize UIEventSystem" ) ]
		public static void Initialize()
		{
			// 設定をクリアする
			Clear() ;

			//----------------------------------

			int i, p ;

			// 設定を追加する
			for( p  = 1 ; p <= 4 ; p ++ )
			{
				// ボタン
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					AddAxis( CreateButton( GamePad.GetButtonName( p, i ), $"joystick {p} button {i}", p ) ) ;
				}
				// アクシス
				for( i  =  1 ; i <= 15 ; i ++ )
				{
					AddAxis( CreatePadAxis( GamePad.GetAxisName( p, i ), p, i ) ) ;
				}
			}
		}

		/// <summary>
		/// 設定をクリアする
		/// </summary>
		private static void Clear()
		{
			var	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] ) ;
			var	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			int i, j, l, m, p ;

			//--------------

			var keys = new List<string>() ;

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				// ボタン
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					keys.Add( GamePad.GetButtonName( p, i ) ) ;
				}
				// アクシス
				for( i  =  1 ; i <= 15 ; i ++ )
				{
					keys.Add( GamePad.GetAxisName( p, i ) ) ;
				}
			}

			m = keys.Count ;

			//--------------

			SerializedProperty axisPropertyElement ;
			string axisName ;

			l = axesProperty.arraySize ;
			i = 0 ;

			while( true )
			{
				axisPropertyElement = axesProperty.GetArrayElementAtIndex( i ) ;
				axisName = GetChildProperty( axisPropertyElement, "m_Name" ).stringValue ;
				
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( axisName == keys[ j ] )
					{
						// 削除対象発見
						break ;
					}
				}

				if( j <  m )
				{
					// 削除対象
					axesProperty.DeleteArrayElementAtIndex( i ) ;
					l -- ;
				}
				else
				{
					i ++ ;
				}

				if( i >= l )
				{
					// 終了
					break ;
				}
			}

			serializedObject.ApplyModifiedProperties() ;
			serializedObject.Dispose() ;
		}

		/// <summary>
		/// InputManager に必要な設定が追加されているか確認する
		/// </summary>
		public static bool Check()
		{
			var	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			var	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			int i, l, m, p ;

			//--------------

			var keyAndValues = new Dictionary<string, bool>() ;

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					keyAndValues.Add( GamePad.GetButtonName( p, i ), false ) ;
				}

				for( i  =  1 ; i <= 15 ; i ++ )
				{
					keyAndValues.Add( GamePad.GetAxisName( p, i ), false ) ;
				}
			}

			m = keyAndValues.Count ;

			//--------------

			SerializedProperty axisPropertyElement ;
			string axisName ;

			l = axesProperty.arraySize ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				axisPropertyElement = axesProperty.GetArrayElementAtIndex( i ) ;
				axisName = GetChildProperty( axisPropertyElement, "m_Name" ).stringValue ;
				
				if( keyAndValues.ContainsKey( axisName ) == true )
				{
					keyAndValues[ axisName ] = true ;
				}
			}

			serializedObject.Dispose() ;

			string[] keys = new string[ m ] ;
			keyAndValues.Keys.CopyTo( keys, 0 ) ;

			for( i  = 0 ; i <  m ; i ++ )
			{
				if( keyAndValues[ keys[ i ] ] == false )
				{
					return false ;	// 設定されていないものがある
				}
			}

			return true ;
		}

		//-----------------------------------------------------------

		/// ボタンを生成する
		private static Axis CreateButton( string buttonName, string positiveButton, int joyNum )
		{
			var axis = new Axis()
			{
				Name			= buttonName,
				PositiveButton	= positiveButton,
				Gravity			= 1000,
				Dead			= 0.001f,
				Sensitivity		= 1000,
				Type			= AxisType.KeyOrMouseButton,
				JoyNum			= joyNum
			} ;
			
			return axis ;
		}

		// キーのアクシスを生成する(未使用)
		public static Axis CreateKeyAxis( string axisName, string negativeButton, string positiveButton )
		{
			var axis = new Axis()
			{
				Name			= axisName,
				NegativeButton	= negativeButton,
				PositiveButton	= positiveButton,
				Gravity			= 3,
				Sensitivity		= 3,
				Dead			= 0.001f,
				Type			= AxisType.KeyOrMouseButton
			} ;
			
			return axis ;
		}

		// ゲームパッドのアクシスを生成する
		public static Axis CreatePadAxis( string axisName, int joyNum, int axisNum )
		{
			var axis = new Axis()
			{
				Name			= axisName,
				Dead			= 0.001f,
				Sensitivity		= 1,
				Type			= AxisType.JoystickAxis,
				AxisNum			= axisNum,
				JoyNum			= joyNum,
			} ;
 
			return axis ;
		}

		// 設定を追加する
		private static void AddAxis( Axis axis )
		{
			var	serializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			var	axesProperty = serializedObject.FindProperty( "m_Axes" ) ;

			axesProperty.arraySize ++ ;
			serializedObject.ApplyModifiedProperties() ;
	
			SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex( axesProperty.arraySize - 1 ) ;
		
			GetChildProperty( axisProperty, "m_Name"					).stringValue	= axis.Name ;
			GetChildProperty( axisProperty, "descriptiveName"			).stringValue	= axis.DescriptiveName ;
			GetChildProperty( axisProperty, "descriptiveNegativeName"	).stringValue	= axis.DescriptiveNegativeName ;
			GetChildProperty( axisProperty, "negativeButton"			).stringValue	= axis.NegativeButton ;
			GetChildProperty( axisProperty, "positiveButton"			).stringValue	= axis.PositiveButton ;
			GetChildProperty( axisProperty, "altNegativeButton"			).stringValue	= axis.AltNegativeButton ;
			GetChildProperty( axisProperty, "altPositiveButton"			).stringValue	= axis.AltPositiveButton ;
			GetChildProperty( axisProperty, "gravity"					).floatValue	= axis.Gravity ;
			GetChildProperty( axisProperty, "dead"						).floatValue	= axis.Dead ;
			GetChildProperty( axisProperty, "sensitivity"				).floatValue	= axis.Sensitivity ;
			GetChildProperty( axisProperty, "snap"						).boolValue		= axis.Snap ;
			GetChildProperty( axisProperty, "invert"					).boolValue		= axis.Invert ;
			GetChildProperty( axisProperty, "type"						).intValue		= ( int )axis.Type ;
			GetChildProperty( axisProperty, "axis"						).intValue		= axis.AxisNum - 1 ;
			GetChildProperty( axisProperty, "joyNum"					).intValue		= axis.JoyNum ;
 
			serializedObject.ApplyModifiedProperties() ;

			serializedObject.Dispose() ;
		}
 
		private static SerializedProperty GetChildProperty( SerializedProperty parent, string childName )
		{
			SerializedProperty child = parent.Copy() ;
			child.Next( true ) ;

			do
			{
				if( child.name == childName )
				{
					return child ;
				}
			}
			while( child.Next( false ) ) ;

			return null ;
		}
	}
 
#endif
}
