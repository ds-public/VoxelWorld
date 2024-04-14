using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Threading ;

using UnityEngine ;

using Cysharp.Threading.Tasks ;

using uGUIHelper ;
using TransformHelper ;

using MathHelper ;
using StorageHelper ;

using DSW.World ;

namespace DSW.World
{
	/// <summary>
	/// クライアント(メイン)
	/// </summary>
	public partial class WorldClient : ExMonoBehaviour
	{
		[SerializeField]
		protected UIView			m_PointerBase ;

		[SerializeField]
		protected SoftTransform		m_WorldRoot ;

		[SerializeField]
		protected PlayerActor		m_PlayerActor ;

		[SerializeField]
		protected PlayerActor		m_PlayerActor_Other ;

		[SerializeField]
		protected SoftTransform		m_ChunkRoot ;

		[SerializeField]
		protected Light				m_Light ;

		[Header( "ネームプレート用" )]

		[SerializeField]
		protected UIView			m_NamePlateRoot ;

		[SerializeField]
		protected NamePlate			m_NamePlate_Other ;



		[Header( "ＵＩ実行表示レイヤー" )]

		[SerializeField]
		protected UIView			m_PlayingLayer ;

		[SerializeField]
		protected UIImage			m_Compas ;

		[SerializeField]
		protected UIView			m_ActiveItemSlotBase ;

		[SerializeField]
		protected UINumberMesh		m_PlayerPositionX ;

		[SerializeField]
		protected UINumberMesh		m_PlayerPositionZ ;

		[SerializeField]
		protected UINumberMesh		m_PlayerPositionY ;

		[SerializeField]
		protected ActiveItemSlot[]	m_ActiveItemSlots ;

		[SerializeField]
		protected UIView			m_ExplanationBase ;

		[SerializeField]
		protected UIImage			m_CrossHairPointer ; 



		[Header( "ＵＩ停止表示レイヤー" )]

		[SerializeField]
		protected UIView			m_PausingLayer ;

		[SerializeField]
		protected UITextMesh		m_GuideMessage ;

		[SerializeField]
		protected UIButton			m_EndButton ;



		[Header( "ＵＩ常時表示レイヤー" ) ]

		[SerializeField]
		protected UIView			m_DisplayLayer ;

		[SerializeField]
		protected UITextMesh		m_FPS ;

		[SerializeField]
		protected UITextMesh		m_Log ;

		[SerializeField]
		protected UIView			m_PerformanceBase ;

		[SerializeField]
		protected UINumberMesh		m_P_ChunkSet_L ;

		[SerializeField]
		protected UINumberMesh		m_P_ChunkSet_K ;

		[SerializeField]
		protected UINumberMesh		m_P_Chunk_O ;

		[SerializeField]
		protected UINumberMesh		m_P_Chunk_E ;

		[SerializeField]
		protected UINumberMesh		m_P_Chunk_V ;



		[Header( "プレイヤーのコリジョン情報" )]

		// 円柱高さ
		[SerializeField]
		protected float				m_PlayerHeight = 1.48f ;

		// 円柱半径
		[SerializeField]
		protected float				m_PlayerRadius = 0.4f ;


		//-------------------------------------------------------------------------------------------

		//---------------------------------------------------------------------------

		[Header( "動き調整" )]

		[SerializeField]
		protected float				m_RotationSpeed = 4f ;

		[SerializeField]
		protected float				m_TranslationSpeed = 4f ;	// １秒間の移動量

		//-----------------------------------------------------------

		// クライアントで保持するプレイヤー情報
		private readonly Dictionary<string,ClientPlayerData>	m_ClientPlayers = new Dictionary<string, ClientPlayerData>() ;

		//-----------------------------------------------------------

		// 視錐台情報
		private readonly ViewVolume	m_ViewVolume = new ViewVolume() ;

		//-----------------------------------

		// 現在のアクティブなチャンクセット群
		private readonly Dictionary<int,ClientChunkSetData>	m_ActiveChunkSets = new Dictionary<int,ClientChunkSetData>() ;

		/// <summary>
		/// 現在アクティブなチャンクセット群を取得する
		/// </summary>
		/// <returns></returns>
		public Dictionary<int,ClientChunkSetData>			GetActiveChunkSets()	=> m_ActiveChunkSets ;

		// 現在のアクティブなチャンク群
		private readonly Dictionary<int,ClientChunkData>	 m_ActiveChunks = new Dictionary<int,ClientChunkData>() ;

		/// <summary>
		/// 現在アクティブなチャンク群を取得する
		/// </summary>
		/// <returns></returns>
		public Dictionary<int,ClientChunkData>				GetActiveChunks()	=> m_ActiveChunks ;

		// チャンクのリクエスト状態が保存される
		private readonly List<int>							m_ChunkSetRequests			= new List<int>() ;

		//-----------------------------------------------------------

		// メインスレッドのコンテキスト
		private SynchronizationContext						m_MainThreadContext ;

		// 全てのサブスレッドの動作を停止するキャンセレーショントークン
		private CancellationTokenSource						m_CancellationSource ;

		//-------------------------------------------------------------------------------------------

		// 接続までの準備が整ったかどうか
		private bool	m_IsReady ;

		/// <summary>
		/// クライアントを終了するかどうか
		/// </summary>
		public  bool	  IsQuit	=> m_IsQuit ;
		//  クライアントを終了するかどうか
		private bool	m_IsQuit ;

		//-----------------------------------

		private int		m_FpsCount ;
		private float	m_FpsTimer ;

		private float	m_DateTime ;

		// プレイヤーが行動可能になったかどうかの判定用
		private bool	m_EnableMoving = false ;

		// 選択中のアクティブアイテムのインデックス
		private int		m_ActiveItemSlotIndex = 0 ;

		// 選択中のブロック種別(後で処理を変える)
		private int		m_SelectedBlockIndex = 1 ;

		// ブロックのテクスチャ
		private Material m_BlockMaterial ;

		/// <summary>
		/// ブロックのマテリアル
		/// </summary>
		public	Material   BlockMaterial => m_BlockMaterial ;

		[Header( "ブロックマテリアル" )]
		[SerializeField]
		protected Material	m_DefaultMaterial ;

		// クライアント識別子
		private string	m_ClientId ;

		// プレイヤー識別子
		private string	m_PlayerId ;

		/// <summary>
		/// 視点の種別
		/// </summary>
		public enum PlayerViewTypes
		{
			/// <summary>
			/// 主観視点
			/// </summary>
			FirstPerson			= 0,

			/// <summary>
			/// 客観視点(通常)
			/// </summary>
			ThirdPerson_Normal	= 1,

			/// <summary>
			/// 客観視点(反転)
			/// </summary>
			ThirdPerson_Invert	= 2,
		}

		// 視点の種別
		private PlayerViewTypes	m_PlayerViewType = PlayerViewTypes.FirstPerson ;

		// プレイヤーがスニーク中かどうか
		private bool m_IsPlayerSneaking = false ;

		// 位置と方向を送信するかどうか判定用(変化が無ければ送信しない)
		private float	m_TransformInterval ;
		private Vector3	m_TransformPosition		= new Vector3() ;
		private Vector3 m_TransformDirection	= new Vector3() ;



		private int		m_Center_CsId ;

		// 現在位置のチャンクのみ表示する(デバッグ用)
		private bool	m_ViewVolumeCenterOnly ;

		//-----------------------------------

		private List<string>			m_LogMessages = new List<string>() ;

		private bool	m_IsLogin ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 準備を行う
		/// </summary>
		/// <returns></returns>
		public async UniTask<bool> Prepare()
		{

			Vector3 center = new Vector3( -0.2f,-0.2f,-0.2f ) ;
//			Vector3 center = new Vector3(  0.5f, 0.5f, 0.5f ) ;
			float radius = 0.4f ;
			( bool isHit, Vector3 p ) = IsHitSphereAndCube( center, radius, 0, 0, 0 ) ;
			Debug.Log( "<color=#FFFF00>--------------->結果:" + isHit + " p = " + p + "</color>" ) ;

			//----------------------------------------------------------


			// 状態を初期化する
			m_IsReady	= false ;
			m_IsQuit	= false ;

			//----------------------------------

			// 現在位置のチャンクのみ表示にするかどうか
			m_ViewVolumeCenterOnly = false ;

			// ログ関係
			m_LogMessages.Clear() ;
			m_Log.Text = string.Empty ;

			// パフォーマンス関係(初期は非表示)
			m_PerformanceBase.SetActive( false ) ;
			m_P_ChunkSet_L.Value			= 0 ;
			m_P_ChunkSet_K.Value			= 0 ;
			m_P_Chunk_O.Value				= 0 ;
			m_P_Chunk_E.Value				= 0 ;
			m_P_Chunk_V.Value				= 0 ;

			//------------------------------------------------------------------------------------------

			// ブロックのマテリアル(テクスチャ)をロードする
			m_BlockMaterial = await Asset.LoadAsync<Material>( "Materials//Face" ) ;

			//----------------------------------

			// 自分の見た目を設定する
			m_PlayerActor.SetColorType( PlayerData.ColorType ) ;
			m_PlayerActor.SetCameraEnabled( true ) ;
			m_PlayerActor.HideFigure() ;
			m_PlayerActor.SetActive( true ) ;


			// 他のプレイヤー用のアクターを非アクティブ化
			m_PlayerActor_Other.SetActive( false ) ;


			// 他のプレイヤー用のネームプレートを非アクティブ化
			m_NamePlate_Other.SetActive( false ) ;

			// 表示中のチャンク情報保存領域を初期化する
			m_ActiveChunkSets.Clear() ;
			m_ActiveChunks.Clear() ;

			// チャンクセットのリクエスト状態を初期化する
			m_ChunkSetRequests.Clear() ;

			// 現在場所のチャンクセット識別子を不明状態に設定
			m_Center_CsId = -1 ;

			//----------------------------------

			// 終了ボタンのコールバックを設定する
			m_EndButton.SetOnSimpleClick( () =>
			{
				if( m_IsLogin == false )
				{
					// 保険
					return ;
				}

				//---------------------------------

				SE.Play( SE.Bomb ) ;

				//----------------------------------

				AddLog( "ログアウトしました" ) ;

				// 終了する
				Shutdown() ;
			} ) ;
			m_EndButton.Interactable = false ;	// ログインするまでは押せない

			//----------------------------------

			// ファークリップを設定する
			float farClip = 16 * WorldSettings.DISPLAY_CHUNK_RANGE ;
			m_PlayerActor.GetCamera().farClipPlane = farClip ;

			// フォグはシーンのライト設定の有効化も必要
//			SetFog( true, farClip ) ;

			//----------------------------------
			// アイテム選択ＵＩ設定

			m_PointerBase.IsInteraction = true ;

			// アイテムショートカットを設定する
			PrepareActiveItemSlots() ;

			//----------------------------------
			// 各種ＵＩの状態設定を設定する

			SetVisible() ;

			//----------------------------------
			// FPS 計測用変数初期化

			SetPalyerPositionDisplay() ;

			m_FpsCount = 0 ;
			m_FpsTimer = 0 ;

			//----------------------------------------------------------

			// プレイヤー情報をロードする
			LoadLocalPlayer() ;

			//------------------------------------------------------------------------------------------

			// ログインしていない
			m_IsLogin = false ;

			// 接続待ち
			while( true )
			{
				// ソケットを開く(サーバーに接続を試みる)
				StartWebSocketClient() ;

				Progress.On( "サーバーへ接続中" ) ;

				// 接続待ち
				while( true )
				{
					if( m_WebSocket == null || m_IsDisconnected == true )
					{
						// 問題発生

						await Progress.OffAsync() ;

						int index = await Dialog.Open( "エラー", "サーバーへの接続に失敗しました", new string[]{ "リトライ", "あきらめる" } ) ;
						if( index == 0 )
						{
							// リトライ
							break ;
						}
						else
						{
							// 切断を実行する(Shutdown まで実行する必要は無い)
							EndWebSocketClient() ;

							// リブート
//							ApplicationManager.Reboot() ;

							// タスクをまとめてキャンセルする
//							throw new OperationCanceledException() ;

							// 失敗終了
							return false ;
						}
					}
					else
					if( m_WebSocket.IsConnecting == true )
					{
						// 接続した
						await Progress.OffAsync() ;

						AddLog( "サーバー(" + PlayerData.ServerAddress + ":" + PlayerData.ServerPortNumber +")に接続しました" ) ;
						break ;
					}

					await Yield() ;
				}

				if( m_WebSocket != null && m_WebSocket.IsConnecting == true )
				{
					// 接続した(外側のループを抜ける)
					break ;
				}
			}

			//----------------------------------

			// 基本的な準備は整った
			m_IsReady = true ;

			//----------------------------------

			// ログイン要求を出す
			WS_Send_Request_Login() ;

			//----------------------------------------------------------

			// 接続成功
			return true ;
		}
		
		//-------------------------------------------------------------

		/// <summary>
		/// 毎フレーム呼ばれる
		/// </summary>
		/// <param name="deltaTime"></param>
		internal void Update()
		{
			if( Input.GetKeyDown( KeyCode.U ) == true )
			{
				// 現在位置のチャンクのみ表示のトグルを切り替える
				m_ViewVolumeCenterOnly = !m_ViewVolumeCenterOnly ;
			}

			// パフォーマンスモニタの表示のオンオフを切り替える
			bool state = m_PerformanceBase.ActiveSelf ;
			if( Input.GetKeyDown( KeyCode.P ) == true )
			{
				m_PerformanceBase.SetActive( !state ) ;
			}

			//----------------------------------------------------------

			if( m_IsReady == false )
			{
				// 準備が整っていない
				return ;
			}

			//----------------------------------------------------------

			// １フレームの処理を行う
			Process() ;

			//----------------------------------------------------------
			// FPS 表示更新

			m_FpsCount ++ ;
			m_FpsTimer += Time.deltaTime ;
			if( m_FpsTimer >= 1.0f )
			{
				m_FPS.Text = "FPS" + " " + m_FpsCount ;

				m_FpsTimer -= 1.0f ;
				m_FpsCount = 0 ;
			}
		}
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 毎フレームの処理
		/// </summary>
		private void Process()
		{
			// ログのフェードアウトを処理する
			ProcessLogFadeOut() ;

			if( string.IsNullOrEmpty( m_PlayerId ) == true || m_IsLogin == false )
			{
				// まだログインしていない
				return ;
			}

			if( m_IsDisconnected == true )
			{
				// サーバーから切断された
				return ;
			}

			//----------------------------------------------------------

			if( m_EnableMoving == true )
			{
				// 行動可能になっている

				// 移動(上昇と下降)を処理する
				ProcessJumpingAndFalling() ;

				// 入力を処理する
				ProcessInteraction() ;

				// １秒経過且つ位置または方向が変わっていたら位置と方向を送信する
				float transformInterval = Time.realtimeSinceStartup ;
				if( ( transformInterval - m_TransformInterval ) >  0.05f )
				{
					Vector3 p = m_PlayerActor.Position ;
					Vector3 d = m_PlayerActor.GetCameraDirection() ;
					if
					(
						m_TransformPosition.x  != p.x || m_TransformPosition.z  != p.z || m_TransformPosition.y  != p.y ||
						m_TransformDirection.x != d.x || m_TransformDirection.z != d.z || m_TransformDirection.y != d.y
					)
					{
						m_TransformInterval		= transformInterval ;
						m_TransformPosition.Set( p.x, p.y, p.z ) ;
						m_TransformDirection.Set( d.x, d.y, d.z ) ;

//						Debug.Log( "方向:" + m_TransformDirection + " " + m_PlayerActor.Forward ) ;

						WS_Send_Request_SetPlayerTransform( m_TransformPosition, m_TransformDirection ) ;
					}
				}
			}
			else
			{
				// 行動可能になっていない

				// 周囲のチャンクの状態を確認して問題なければ行動可能にする
				var p = m_PlayerActor.Position ;
				int bx = ( int )p.x ;
				int bz = ( int )p.z ;

				if( IsChunkSetsLoaded( bx, bz, 3 ) == true )
				{
					// 行動可能にする
					m_EnableMoving = true ;
				}
			}

			//----------------------------------------------------------

			// チャンクを更新する
			UpdateChunks() ;

			// オクルージョンカリングで見えないチャンクを非アクティブにする
			OcclusionCulling( m_PlayerActor.GetCamera().transform.position ) ;

			// ネームプレートの表示位置を更新する
			UpdatePlayerNamePlates() ;

			// 現在位置の表示を更新する
			SetPalyerPositionDisplay() ;

			//----------------------------------------------------------

			// １日の時間経過を処理する(仮)
			ProcessDateTime() ;
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

		//-------------------------------------------------------------------------------------------

		//-------------------------------------------------------------------------------------------
		//-------------------------------------------------------------------------------------------

		private float m_LogDisplayKeepTime ; 

		// ログを追加する
		private void AddLog( string message )
		{
			if( m_LogMessages.Count >= 8 )
			{
				m_LogMessages.RemoveAt( 0 ) ;
			}

			m_LogMessages.Add( message ) ;

			//----------------------------------

			string logMessages = string.Empty ;

			foreach( var logMessage in m_LogMessages )
			{
				logMessages += logMessage + "\n" ;
			}

			m_Log.Text = logMessages ;

			m_LogDisplayKeepTime = 10 ;	// 表示を維持する時間
			m_Log.Alpha = 1 ;
		}

		// ログのフェードアウトを処理する
		private void ProcessLogFadeOut()
		{
			float delatTime = Time.deltaTime ;

			if( m_LogDisplayKeepTime >  0 )
			{
				// 表示中
				m_LogDisplayKeepTime -= delatTime ;

				if( m_LogDisplayKeepTime >  0 )
				{
					return ;
				}

				if( m_LogDisplayKeepTime <  0 )
				{
					m_LogDisplayKeepTime  = 0 ;
				}
			}

			if( m_LogDisplayKeepTime == 0 && m_Log.Alpha >  0 )
			{
				// 徐々にフェードアウト

				float alpha = m_Log.Alpha ;

				alpha -= ( delatTime / 0.25f ) ;
				if( alpha <  0 )
				{
					alpha  = 0 ;
				}

				m_Log.Alpha = alpha ;

			}
		}

		// 現在位置の表示を更新する
		private void SetPalyerPositionDisplay()
		{
			if( m_IsLogin == false )
			{
				m_PlayerPositionX.NoRefrect = true ;
				m_PlayerPositionZ.NoRefrect = true ;
				m_PlayerPositionY.NoRefrect = true ;

				m_PlayerPositionX.Text = "-" ;
				m_PlayerPositionZ.Text = "-" ;
				m_PlayerPositionY.Text = "-" ;

				return ;
			}

			//----------------------------------

			m_PlayerPositionX.NoRefrect = false ;
			m_PlayerPositionZ.NoRefrect = false ;
			m_PlayerPositionY.NoRefrect = false ;

			Vector3 p = m_PlayerActor.Position ;

			int bx = ( int )p.x ;
			int bz = ( int )p.z ;
			int by = ( int )p.y ;

			m_PlayerPositionX.Value = bx ;
			m_PlayerPositionZ.Value = bz ;
			m_PlayerPositionY.Value = by ;
		}

		// コンパスの表示を更新する
		private void UpdateCompas()
		{
			Vector3 direction = m_PlayerActor.Forward ;

			// コンパスの向きも設定する
			float angle = Mathf.Atan2( direction.z, direction.x ) ;
			angle = 180.0f * angle / Mathf.PI ;
			m_Compas.Roll = angle ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// クライアントを終了する
		/// </summary>
		public void Shutdown()
		{
			if( m_IsLogin == true )
			{
				// ログイン済みの場合はプレイヤー情報をセーブする(ログイン用の識別子)　※保険
				SaveLocalPlayer() ;
			}

			m_IsLogin	= false ;

			// ソケットを閉じる(Ready 状態に関係なく実行が必要)
			EndWebSocketClient() ;

			m_IsQuit	= true ;	// 終了			
			m_IsReady	= false ;
		}

		/// <summary>
		/// オブジェクトが破棄される際に呼び出される
		/// </summary>
		internal void OnDestroy()
		{
			Shutdown() ;
		}
	}
}
