using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.SceneManagement ;
using static DBS.LocalizeManager ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;

namespace DBS.nScreen.nWorld
{
	/// <summary>
	/// クライアント(メイン)
	/// </summary>
	public partial class WorldClient : MonoBehaviour
	{
		[SerializeField]
		protected UIImage			m_PointerBase = null ;

		[SerializeField]
		protected GameObject		m_CubePrefab_Template = null ;

		[SerializeField]
		protected GameObject		m_ChunkCubePrefab_Template = null ;


		[SerializeField]
		protected SoftTransform		m_BoxelRoot = null ;

		[SerializeField]
		protected SoftTransform		m_Player = null ;

		[SerializeField]
		protected Camera			m_Camera = null ;

		[SerializeField]
		protected Light				m_Light = null ;

		[SerializeField]
		protected WorldServer		m_WorldServer ;

		[SerializeField]
		protected UIImage			m_CrossHairPointer = null ; 

		[SerializeField]
		protected ItemShortCut[]	m_ItemShortCuts ;

		[SerializeField]
		protected UITextMesh		m_FPS = null ;

		[SerializeField]
		public UITextMesh			m_Log = null ;



		//---------------------------------------------------------------------------

		public float				RotationSpeed = 4f ;
		public float				TranslationSpeed = 4f ;

		private int					m_ItemShortCutIndex = 0 ;
		public int					SelectedBlockIndex = 1 ;


		//-----------------------------------------------------------
		// デバッグ用

		[SerializeField]
		protected UIImage[]			m_Boxes ;


		[SerializeField]
		protected UICircle			m_Circle_B ;

		[SerializeField]
		protected UICircle			m_Circle_A ;

		//-----------------------------------------------------


		// 視錐台情報
		private readonly ViewVolume	m_ViewVolume = new ViewVolume() ;

		public Dictionary<long, ActiveChunkSetData> ActiveChunkSets = new Dictionary<long, ActiveChunkSetData>() ;

		public Dictionary<long, ActiveChunkData> ActiveChunks = new Dictionary<long, ActiveChunkData>() ;

		public Material DefaultMaterial ;


		private bool isReady ;

		private int		m_FpsCount ;
		private float	m_FpsTimer ;

		private float	m_DateTime ;

		//-------------------------------------------------------------------------------------------

		void Awake()
		{
			// ApplicationManager を起動する(最初からヒエラルキーにインスタンスを生成しておいても良い)
			ApplicationManager.Create() ;

			m_CubePrefab_Template.SetActive( false ) ;

			m_ChunkCubePrefab_Template.SetActive( false ) ;

			// パーリンノイズのシード値を設定する
			PerlinNoise.Initialize( 13212 ) ;

			//---------------------------------
			// 四角と円の接触判定がうまくいくか確認する
/*
			bool r = CollisionCheck
			(
//				new Vector2[]{ new Vector2( -0.5f, -0.5f ), new Vector2(  0.5f, -0.5f ), new Vector2(  0.5f,  0.5f ), new Vector2(  -0.5f,  0.5f ) },
				new Vector2[]{ new Vector2( -0.5f, -0.5f ), new Vector2(  -0.5f,  0.5f ), new Vector2(  0.5f,  0.5f ), new Vector2(  0.5f, -0.5f ) },
				new Vector2( -0.6f,  0.0f ),
				0.4f
			) ;

			if( r == true )
			{
				Debug.LogWarning( "接触判定結果:接触している" ) ;
			}
			else
			{
				Debug.LogWarning( "接触判定結果:接触していない" ) ;
			}
*/

			// 現在位置

			int i, l = m_Boxes.Length ;

//			List<FlatBox> fb = new List<FlatBox>() ;
			List<CollisionLine>	cl = new List<CollisionLine>() ;
			List<CollisionPoint> cp = new List<CollisionPoint>() ;

			float bcx, bcy, blx, bly ;
			float bx0, bx1, by0, by1 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Boxes[ i ].ActiveSelf == true )
				{
					bcx = m_Boxes[ i ].Px * 0.01f ;
					bcy = m_Boxes[ i ].Py * 0.01f ;
					blx = m_Boxes[ i ].Width  * 0.01f ;
					bly = m_Boxes[ i ].Height * 0.01f ;

					bx0 = bcx - blx * 0.5f ;
					bx1 = bcx + blx * 0.5f ;
					by0 = bcy - bly * 0.5f ;
					by1 = bcy + bly * 0.5f ;

					cl.Add( new CollisionLine()
					{
						Points = new Vector2[]
						{
							new Vector2( bx1, by0 ),
							new Vector2( bx0, by0 )
						}
					} ) ;

					if( i == 0 )
					{
						cp.Add( new CollisionPoint()
						{
							Point = new Vector2( bx1, by0 ),
							Lines = new Vector2[]
							{
								new Vector2(  0, -1 ),	// ↓
								new Vector2( -1,  0 )	// ←
							}
						} );

						cl.Add( new CollisionLine()
						{
							Points = new Vector2[]
							{
								new Vector2( bx1, by1 ),
								new Vector2( bx1, by0 )
							}
						} ) ;
					}
				}
			}

			Vector2 cp_b = new Vector2( m_Circle_B.Px * 0.01f, m_Circle_B.Py * 0.01f ) ;
			Vector2 cp_a = new Vector2( m_Circle_A.Px * 0.01f, m_Circle_A.Py * 0.01f ) ;

			Vector2 mv_e = cp_a - cp_b ;
			Vector2 mv_t ;
			Vector2 mv_r = mv_e ;
			Vector2 mv_o ;

			if( cl.Count >  0 )
			{
				// 線の接触判定
				CheckCollisionLine( cl.ToArray(), cp_b, 0.4f, mv_e, out mv_t, out mv_o ) ;
				mv_r = mv_t ;
			}

			if( cp.Count >  0 )
			{
				// 点の接触判定
				CheckCollisionPoint( cp.ToArray(), cp_b, 0.4f, mv_e, out mv_t, out mv_o ) ;
				if( mv_t.magnitude <   mv_r.magnitude )
				{
					mv_r = mv_t ;
				}
			}

/*			l = fb.Count ;
			bool r ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				r = CollisionCheckHorizontal
				(
					fb[ i ].XZ,
					cp_b,
					0.4f,
					mv_e,
					out mv_t
				) ;

				if( r == true && mv_t.magnitude <  mv_r.magnitude )
				{
					mv_r = mv_t ;
				}
			}*/

			cp_a = cp_b + mv_r ;

			m_Circle_B.SetPosition( cp_b * 100 ) ;
			m_Circle_A.SetPosition( cp_a * 100 ) ;


//			Debug.LogWarning( "現在位置:" + cp + " 移動位置:" + mp + " 実移動量:" + ( mp - cp ) ) ;
		}

		IEnumerator Start()
		{
			// ApplicationManager の準備が整うのを待つ
			if( ApplicationManager.IsInitialized == false )
			{
				yield return new WaitWhile( () => ApplicationManager.IsInitialized == false ) ;
			}

			if( ScreenManager.IsProcessing == false )
			{
				// いきなりこのシーンを開いたケース(デバッグ動作)
				yield return ScreenManager.SetupAsync( Scene.Screen.World ) ;
			}

			//----------------------------------------------------------

			if( m_WorldServer == null )
			{
				m_WorldServer = GetComponent<WorldServer>() ;
			}

			ActiveChunkSets.Clear() ;
			ActiveChunks.Clear() ;
			float farClip = 16 * 8 ;

			m_Camera.farClipPlane = farClip ;
			SetFog( true, farClip + 0.1f ) ;

			m_PointerBase.isInteraction = true ;

			int i, l = m_ItemShortCuts.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ItemShortCuts[ i ].Setup( i, OnSelectedBlockIndex ) ;
			}

			m_ItemShortCutIndex = 0 ;
			SelectedBlockIndex = 1 ;
			UpdateItemShotCuts( m_ItemShortCutIndex ) ;

			m_CrossHairPointer.SetActive( m_Focus ) ;

			m_FpsCount = 0 ;
			m_FpsTimer = 0 ;

			//----------------------------------------------------------

			// フェードインを許可する
			Scene.Ready = true ;
			ScreenManager.Ready = true ;

			//----------------------------------------------------------

			// フェード完了を待つ
			yield return new WaitWhile( () => ( Scene.IsFading == true || ScreenManager.IsProcessing == true ) ) ;

			//----------------------------------------------------------

			isReady = true ;
		}

		/// <summary>
		/// アイテムショートカットを選択した
		/// </summary>
		/// <param name="selectedBlockIndex"></param>
		private void OnSelectedBlockIndex( int itemShortCutIndex )
		{
			if( m_ItemShortCutIndex != itemShortCutIndex )
			{
				m_ItemShortCutIndex  = itemShortCutIndex ;

				// 表示を更新する
				UpdateItemShotCuts( m_ItemShortCutIndex ) ;

				//---------------------------------
				// 選択中のブロックインデックスを更新する(仮)
				SelectedBlockIndex = 1 + m_ItemShortCutIndex ;
			}
		}

		/// <summary>
		/// アイテムショートカットの表示を更新する
		/// </summary>
		private void UpdateItemShotCuts( int selectedItemShortCutIndex )
		{
			int i, l = m_ItemShortCuts.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ItemShortCuts[ i ].SetCursor( i == selectedItemShortCutIndex ) ;
			}
		}


		//-------------------------------------------------------------

		private void Update()
		{
			if( isReady == false )
			{
				return ;
			}

			//-------------------------

			// 動作させる
			Process() ;

			// 時間経過を演出する
			ProcessDateTime() ;

			//-------------------------

			m_FpsCount ++ ;
			m_FpsTimer += Time.deltaTime ;
			if( m_FpsTimer >= 1.0f )
			{
				m_FPS.Text = TEXT( LocalizeKey.FPS ) + " " + m_FpsCount ;

				m_FpsTimer -= 1.0f ;
				m_FpsCount = 0 ;
			}
		}

		/// <summary>
		/// １日の時間経過を処理する
		/// </summary>
		private void ProcessDateTime()
		{
			float maxDateTime = 300.0f ;

			m_DateTime += Time.deltaTime ;
			m_DateTime %= maxDateTime ;

			m_Light.transform.localRotation = Quaternion.AngleAxis( 360.0f * ( m_DateTime / maxDateTime ), m_Light.transform.right ) ;
		}

		//---------------------------------------------------------------------------
	}
}
