using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using MessagePack ;	// 1.7.3.5 は Api Compatibility Lavel を .Net 4.x にしないと使えない

using StorageHelper ;
using uGUIHelper ;

using TransformHelper ;

namespace DSW.World
{
	/// <summary>
	/// クライアント側で保持する全プレイヤー情報
	/// </summary>
	public class ClientPlayerData : CancelableTask
	{
		/// <summary>
		/// クライアント識別子
		/// </summary>
		public string	ClientId ;

		//---------------

		/// <summary>
		/// プレイヤー名
		/// </summary>
		public string	Name ;

		/// <summary>
		/// プレイヤー色
		/// </summary>
		public byte		ColorType ;

		/// <summary>
		/// 位置
		/// </summary>
		public Vector3	Position ;

		/// <summary>
		/// 方向
		/// </summary>
		public Vector3	Direction ;

		//-------------------------------------------------------------------------------------------

		// アクターのインスタンス
		private PlayerActor	m_Actor ;

		// ネームプレートのインスタンス
		private NamePlate	m_NamePlate ;


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// デフォルトコンストラクタ
		/// </summary>
		/// <param name="owner"></param>
		public ClientPlayerData( ExMonoBehaviour owner ) : base( owner ){}

		/// <summary>
		/// アクターを生成する
		/// </summary>
		/// <param name="worldRoot"></param>
		/// <param name="playerActor_Other"></param>
		public void CreateActor( SoftTransform worldRoot, PlayerActor playerActor_Other )
		{
			m_Actor = worldRoot.AddPrefab<PlayerActor>( playerActor_Other.gameObject ) ;

			// 位置を設定する
			m_Actor.SetPosition( Position ) ;

			// 方向を設定する
			m_Actor.SetDirection( Direction ) ;

			// 色タイプを設定する
			m_Actor.SetColorType( ColorType ) ;

			// カメラの状態を設定する
			m_Actor.SetCameraEnabled( false ) ;

			// 姿を表示する
			m_Actor.ShowFigure() ;

			// 表示する
			m_Actor.SetActive( true ) ;
		}

		/// <summary>
		/// アクターを破棄する
		/// </summary>
		public void DeleteActor()
		{
			if( m_Actor != null )
			{
				DestroyInstance( m_Actor.gameObject ) ;
				m_Actor = null ;
			}
		}

		/// <summary>
		/// アクターを設定する
		/// </summary>
		/// <param name="actor"></param>
		public void SetActor( PlayerActor actor )
		{
			m_Actor = actor ;
/*
			// 位置を設定する
			m_Actor.SetPosition( Position ) ;

			// 方向を設定する
			m_Actor.SetDirection( Direction ) ;

			// 色タイプを設定する
			m_Actor.SetColorType( ColorType ) ;

			// カメラの状態を設定する
			m_Actor.SetCameraEnabled( false ) ;

			// 姿を表示する
			m_Actor.ShowFigure() ;

			// 表示する
			m_Actor.SetActive( true ) ;*/
		}

		/// <summary>
		/// アクターを取得する
		/// </summary>
		/// <returns></returns>
		public PlayerActor GetActor()
		{
			return m_Actor ;
		}

		/// <summary>
		/// ネームプレートを生成する
		/// </summary>
		/// <param name="namePlateRoot"></param>
		/// <param name="namePlate_Other"></param>
		/// <param name="playerCamera"></param>
		public void CreateNamePlate( UIView namePlateRoot, NamePlate namePlate_Other, Camera playerCamera )
		{
			if( m_Actor == null )
			{
				return ;
			}

//			Vector3 namePlatePosition3d = m_Actor.NamePlateBase.position ;
//			Debug.Log( "[NP]3D座標:" + namePlatePosition3d ) ;
//			Vector2 screenPosition = playerCamera.WorldToScreenPoint( namePlatePosition3d ) ;
//			Debug.Log( "[NP]Scren座標:" + screenPosition ) ;
//			Vector2 viewPosition = namePlateRoot.GetLocalPosition( screenPosition ) ;
//			Debug.Log( "[NP]View座標:" + viewPosition ) ;

			m_NamePlate = namePlateRoot.AddPrefab<NamePlate>( namePlate_Other.gameObject ) ;
			m_NamePlate.SetPlayerName( Name ) ;
			m_NamePlate.SetActive( false ) ;

			// ネームプレートの位置を設定する
//			SetNamePlatePosition( m_Actor.NamePlateBase.position, namePlateRoot, playerCamera ) ;
		}

		// ネームプレートの位置を設定する
/*		private void SetNamePlatePosition( Vector3 worldPosition, UIView namePlateRoot, Camera playerCamera )
		{
			Debug.Log( "[NP]3D座標:" + worldPosition ) ;
			Vector2 screenPosition = playerCamera.WorldToScreenPoint( worldPosition ) ;
			Debug.Log( "[NP]Scren座標:" + screenPosition ) ;
			Vector2 viewPosition = namePlateRoot.GetLocalPosition( screenPosition ) ;
			Debug.Log( "[NP]View座標:" + viewPosition ) ;

			Vector2 viewSize = namePlateRoot.Size ;
			float w = viewSize.x * 0.5f ;
			float h = viewSize.y * 0.5f ;

			if( viewPosition.x <  ( - w ) )
			{
				viewPosition.x  = ( - w ) ; 
			}
			else
			if( viewPosition.x >  ( + w ) )
			{
				viewPosition.x  = ( + w ) ; 
			}

			if( viewPosition.y <  ( - h ) )
			{
				viewPosition.y  = ( - h ) ; 
			}
			else
			if( viewPosition.y >  ( + h ) )
			{
				viewPosition.y  = ( + h ) ; 
			}

			m_NamePlate.SetPosition( viewPosition ) ;
		}*/


		/// <summary>
		/// ネームプレートを破棄する
		/// </summary>
		public void DeleteNamePlate()
		{
			if( m_NamePlate != null )
			{
				DestroyInstance( m_NamePlate.gameObject ) ;
				m_NamePlate = null ;
			}
		}

		/// <summary>
		/// 位置と方向を設定する
		/// </summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		public void SetTransform( Vector3 position, Vector3 direction, UIView namePlateRoot, Camera playerCamera )
		{
			Position	= position ;
			Direction	= direction ;

			if( m_Actor != null )
			{
				// アクターの位置と方向を設定する
				m_Actor.SetPosition( Position ) ;
				m_Actor.SetDirection( Direction ) ;

//				if( m_NamePlate != null )
//				{
//					// ネームプレートの位置を設定する
//					SetNamePlatePosition( m_Actor.NamePlateBase.position, namePlateRoot, playerCamera ) ;
//				}
			}
		}


		/// <summary>
		/// ネームプレートの表示位置を更新する
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="namePlateRoot"></param>
		/// <param name="playerCamera"></param>
		public void UpdateNamePlatePosition( UIView namePlateRoot, Camera playerCamera )
		{
			if( m_Actor == null )
			{
				return ;
			}

			//----------------------------------

			Vector3 worldPosition = m_Actor.NamePlateBase.position ;

			Vector3 cameraPosition  = playerCamera.transform.position ;
			Vector3 cameraDirection = playerCamera.transform.forward ;

			Vector3 direction = worldPosition - cameraPosition ;

			direction.Normalize() ;

			float angle = Vector3.Dot( cameraDirection, direction ) ;

			if( angle <= 0.4f )
			{
				// 横から後ろ
				m_NamePlate.SetActive( false ) ;
				return ;
			}


			Vector2 screenPosition = playerCamera.WorldToScreenPoint( worldPosition ) ;
			Vector2 viewPosition = namePlateRoot.GetLocalPosition( screenPosition ) ;

			Vector2 viewSize = namePlateRoot.Size ;
			float w = ( viewSize.x * 0.5f ) - 32 ;
			float h = ( viewSize.y * 0.5f ) + 32 ;

			if( viewPosition.x <  ( - w ) )
			{
				viewPosition.x  = ( - w ) ; 
			}
			else
			if( viewPosition.x >  ( + w ) )
			{
				viewPosition.x  = ( + w ) ; 
			}

			if( viewPosition.y <  ( - h ) )
			{
				m_NamePlate.SetActive( false ) ;
				return ;
			}
			else
			if( viewPosition.y >  ( + h ) )
			{
				m_NamePlate.SetActive( false ) ;
				return ;
			}

			m_NamePlate.SetPosition( viewPosition ) ;
			m_NamePlate.SetActive( true ) ;
		}




	}
}
