using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.U2D ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditorInternal ;
#endif


namespace SpriteHelper
{
	/// <summary>
	/// スプライト制御クラス  Version 2024/08/17
	/// </summary>
	public partial class SpriteActor : SpriteImage
	{
#if UNITY_EDITOR

		/// <summary>
		/// Actor Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteActor/Default", false, 0 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteActor/Default" )]					// ポップアップメニューから
		public static void CreateSpriteActor_Default()
		{
			CreateSpriteActor( string.Empty ) ;
		}

		/// <summary>
		/// Actor Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteActor/for Top View - Overlap", false, 1 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteActor/for Top View - Overlap" )]					// ポップアップメニューから
		public static void CreateSpriteActor_Top_Overlap()
		{
			CreateSpriteActor( "Actor Top Overlap" ) ;
		}

		/// <summary>
		/// Actor Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteActor/for Top View - Collide", false, 2 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteActor/for Top View - Collide" )]					// ポップアップメニューから
		public static void CreateSpriteActor_Top_Collide()
		{
			CreateSpriteActor( "Actor Top Collide" ) ;
		}

		/// <summary>
		/// Actor Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteActor/for Side View - Overlap", false, 3 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteActor/for Side View - Overlap" )]					// ポップアップメニューから
		public static void CreateSpriteActor_Side_Overlap()
		{
			CreateSpriteActor( "Actor Side Overlap" ) ;
		}

		/// <summary>
		/// Actor Sprite を生成
		/// </summary>
		[MenuItem( "GameObject/SpriteHelper/SpriteActor/for Side View - Collide", false, 4 )]	// メニューから
		[MenuItem( "SpriteHelper/Add a SpriteActor/for Side View - Collide" )]					// ポップアップメニューから
		public static void CreateSpriteActor_Side_Collide()
		{
			CreateSpriteActor( "Actor Side Collide" ) ;
		}

		public static void CreateSpriteActor( string typeName )
		{
			GameObject go = Selection.activeGameObject ;
			if( go == null )
			{
				return ;
			}

			if( WillLosePrefab( go ) == false )
			{
				return ;
			}

			Undo.RecordObject( go, "Add a child SpriteActor(for TopView)" ) ;	// アンドウバッファに登録

			var child = new GameObject( "SpriteActor" ) ;

			Transform t = child.transform ;
			t.SetParent( go.transform, false ) ;
			t.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity ) ;
			t.localScale = Vector3.one ;

			var component = child.AddComponent<SpriteActor>() ;
			component.SetDefault( typeName ) ;	// 初期状態に設定する

			// 一番上に移動させる
			while( ComponentUtility.MoveComponentUp( component ) ){}

			if( component.TryGetComponent<SpriteDrawer>( out var spriteDrawer ) == true )
			{
				// SpriteDrawer を一番上に移動させる
				while( ComponentUtility.MoveComponentUp( spriteDrawer ) ){}
			}

			Selection.activeGameObject = child ;

			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
		}

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
#endif
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 動的生成された際にデフォルト状態を設定する
		/// </summary>
		public override void SetDefault( string typeName )
		{
			base.SetDefault( typeName ) ;

			//-----------------------------------------------------------

			typeName = typeName.ToLower() ;

			if( typeName == "actor top overlap" )
			{
				// トップビュー用　接触◯　衝突✕

				SetSize( 16, 16 ) ;

				AddCollider<PolygonCollider2D>() ;
				ColliderAdjustment = true ;
				ColliderEdgeWidth = 1 ;

				IsTrigger		= true ;
				BodyType		= RigidbodyType2D.Dynamic ;
				GravityScale	= 0 ;
				FreezeRotation	= false ;
			}
			else
			if( typeName == "actor top collide" )
			{
				// トップビュー用　接触✕　衝突◯

				SetSize( 16, 16 ) ;

				AddCollider<PolygonCollider2D>() ;
				ColliderAdjustment = true ;
				ColliderEdgeWidth = 1 ;

				IsTrigger		= false ;
				BodyType		= RigidbodyType2D.Dynamic ;
				GravityScale	= 0 ;
				FreezeRotation	= true ;

				SetPhysicsMaterial2D( Resources.Load<PhysicsMaterial2D>( "SpriteHelper/PhysicsMaterial2D/Plane" ) ) ;

				//----------------------------------
#if UNITY_EDITOR
				var frame = SpriteFrame.CreateSpriteFrame( transform, string.Empty ) ;

				frame.name = "IsTrigger" ;

				frame.CST.HorizontalAnchorType	= HorizontalAnchorTypes.Stretch ;
				frame.CST.VerticalAnchorType	= VerticalAnchorTypes.Stretch ;

				frame.AddCollider<BoxCollider2D>() ;
				frame.ColliderAdjustment = true ;

				frame.IsTrigger			= true ;
				frame.BodyType			= RigidbodyType2D.Kinematic ;
				frame.GravityScale		= 0 ;
				frame.FreezeRotation	= false ;
#endif
			}
			else
			if( typeName == "actor side overlap" )
			{
				// トップビュー用　接触◯　衝突✕

				SetSize( 16, 16 ) ;

				AddCollider<PolygonCollider2D>() ;
				ColliderAdjustment = true ;
				ColliderEdgeWidth = 1 ;

				IsTrigger		= true ;
				BodyType		= RigidbodyType2D.Dynamic ;
				GravityScale	= 0 ;
				FreezeRotation	= false ;
			}
			else
			if( typeName == "actor side collide" )
			{
				// トップビュー用　接触✕　衝突◯

				SetSize( 16, 16 ) ;

				AddCollider<PolygonCollider2D>() ;
				ColliderAdjustment = true ;
				ColliderEdgeWidth = 1 ;

				IsTrigger		= false ;
				BodyType		= RigidbodyType2D.Dynamic ;
				GravityScale	= 10 ;
				FreezeRotation	= true ;

				SetPhysicsMaterial2D( Resources.Load<PhysicsMaterial2D>( "SpriteHelper/PhysicsMaterial2D/Plane" ) ) ;

				//----------------------------------
#if UNITY_EDITOR
				var frame = SpriteFrame.CreateSpriteFrame( transform, string.Empty ) ;

				frame.name = "IsTrigger" ;

				frame.CST.HorizontalAnchorType	= HorizontalAnchorTypes.Stretch ;
				frame.CST.VerticalAnchorType	= VerticalAnchorTypes.Stretch ;

				frame.AddCollider<BoxCollider2D>() ;
				frame.ColliderAdjustment = true ;

				frame.IsTrigger			= true ;
				frame.BodyType			= RigidbodyType2D.Kinematic ;
				frame.GravityScale		= 0 ;
				frame.FreezeRotation	= false ;
#endif
			}
		}

		//-------------------------------------------------------------------------------------------

	}
}
