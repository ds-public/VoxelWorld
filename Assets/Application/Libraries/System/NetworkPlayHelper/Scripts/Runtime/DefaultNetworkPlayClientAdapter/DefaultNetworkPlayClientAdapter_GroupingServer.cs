#pragma warning disable IDE0350

//#if false

using System ;
using System.Collections.Generic ;
using System.Linq ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;

using SocketHelper ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// 標準のアダプター
	/// </summary>
	public partial class DefaultNetworkPlayClientAdapter : INetworkPlayClientAdapter
	{
		private GroupingServerProcesor	m_GroupingServerProcessor ;

		//-----------------------------------------------------------

		/// <summary>
		/// グルーピングサーバーのアドレス
		/// </summary>
		public	string		GroupingServer_Address			=> m_GroupingServerProcessor.Address ;

		/// <summary>
		/// グルーピングサーバーのＴＣＰポート
		/// </summary>
		public	ushort		GroupingServer_TcpPort			=> m_GroupingServerProcessor.TcpPort ;

		/// <summary>
		/// グルーピングサーバーに接続中かどうか
		/// </summary>
		public  bool        IsGroupingServerConnected       => m_GroupingServerProcessor.IsConnected ;

		//===================================================================================================================
		// 外部からの参照

		/// <summary>
		/// 関連性のあるユーザー情報群
		/// </summary>
		public List<RelatedUserData>		RelatedUsers		=> m_GroupingServerProcessor.RelatedUsers ;

		/// <summary>
		/// グループ招待情報群
		/// </summary>
		public List<GroupInvitationData>	GroupInvitations	=> m_GroupingServerProcessor.GroupInvitations ;

		/// <summary>
		/// グループメンバー情報群
		/// </summary>
		public List<GroupMemberData>		GroupMembers		=> m_GroupingServerProcessor.GroupMembers ;

		//---------------

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType			=> m_GroupingServerProcessor.GroupType ;

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId				=> m_GroupingServerProcessor.GroupId ;

		/// <summary>
		/// グループのリーダーであるかどうか
		/// </summary>
		public bool							IsGroupLeader		=> m_GroupingServerProcessor.IsGroupLeader ;

		/// <summary>
		/// グループの準備完了を実行中か
		/// </summary>
		public bool							IsGroupReady		=> m_GroupingServerProcessor.IsGroupReady ;

		//-----------------------------------

		/// <summary>
		/// 固有パラメータを設定する(グループに属する前でも可)
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<SetGroupingServiceParameter_Response> SetGroupingServiceParameterAsync
		(
			Dictionary<string,string>   parameters,
			CancellationToken           cancellationToken
		)
		{
			return await m_GroupingServerProcessor.SetGroupingServiceParameterAsync( parameters, cancellationToken ) ;
		}

		/// <summary>
		/// フレンド情報群(グルーピング用)を取得する
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<GetFriendsForGrouping_Response> GetFriendsForGroupingAsync
		(
			ushort offset,
			ushort length,
			CancellationToken cancellationToken
		)
		{
			return await m_GroupingServerProcessor.GetFriendsForGroupingAsync( offset, length, cancellationToken ) ;
		}

		//---------------
		// コールバック

		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
		{
			m_GroupingServerProcessor.AddOnRelatedUserUpdated( onRelatedUserUpdated ) ;
		}

		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
		{
			m_GroupingServerProcessor.RemoveOnRelatedUserUpdated( onRelatedUserUpdated ) ;
		}

		/// <summary>
		/// グループ招待に応答があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
		{
			m_GroupingServerProcessor.AddOnGroupInvitationResponded( onGroupInvitationResponded ) ;
		}

		/// <summary>
		/// グループ招待に応答があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
		{
			m_GroupingServerProcessor.RemoveOnGroupInvitationResponded( onGroupInvitationResponded ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
		{
			m_GroupingServerProcessor.AddOnGroupInvitationUpdated( onGroupInvitationUpdated ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
		{
			m_GroupingServerProcessor.RemoveOnGroupInvitationUpdated( onGroupInvitationUpdated ) ;
		}

		/// <summary>
		/// グループパラメータの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
		{
			m_GroupingServerProcessor.AddOnGroupUpdated( onGroupUpdated ) ;
		}

		/// <summary>
		/// グループパラメータの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
		{
			m_GroupingServerProcessor.RemoveOnGroupUpdated( onGroupUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
		{
			m_GroupingServerProcessor.AddOnGroupMemberUpdated( onGroupMemberUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
		{
			m_GroupingServerProcessor.RemoveOnGroupMemberUpdated( onGroupMemberUpdated ) ;
		}

		/// <summary>
		///マッチングが開始された際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
		{
			m_GroupingServerProcessor.AddOnMatchingStatusUpdated( onMatchingStatusUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
		{
			m_GroupingServerProcessor.RemoveOnMatchingStatusUpdated( onMatchingStatusUpdated ) ;
		}

		/// <summary>
		/// マッチング中かどうか
		/// </summary>
		public bool	IsMatchingRunning
			=> m_GroupingServerProcessor.IsMatchingRunning ;

		/// <summary>
		/// マッチングの取消が可能かどうか
		/// </summary>
		public bool IsMatchingStoppable
			=> m_GroupingServerProcessor.IsMatchingStoppable ;

		//---------------

		/// <summary>
		/// グループ招待を実行する
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<AffordGroupInvitation_Response> AffordGroupInvitationAsync
		(
			string						applicationId,		// アプリケーション識別子
			string						userId,				// 招待対象のユーザー識別子
			Dictionary<string, string>	parameters,			// 招待者の固有パラメータ
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.AffordGroupInvitationAsync( applicationId, userId, parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループ招待を取消する
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<CancelGroupInvitation_Response> CancelGroupInvitationAsync
		(
			string userId,							// 招待対象のユーザー識別子
			CancellationToken cancellationToken
		)
		{
			return await m_GroupingServerProcessor.CancelGroupInvitationAsync( userId, cancellationToken ) ;
		}

		/// <summary>
		/// グループ招待を承諾する　※メンバー限定行動
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<AcceptGroupInvitation_Response> AcceptGroupInvitationAsync
		(
			string						userId,							// 招待対象のユーザー識別子
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.AcceptGroupInvitationAsync( userId, parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループへの招待を拒否する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<RejectGroupInvitation_Response> RejectGroupInvitationAsync
		(
			string						userId,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.RejectGroupInvitationAsync( userId, cancellationToken ) ;
		}

		/// <summary>
		/// グループ情報群を取得する
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<GetGroups_Response> GetGroupsAsync
		(
			string                      applicationId,
			byte					    filter,
			ushort                      offset,
			ushort                      length,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.GetGroupsAsync( applicationId, filter, offset, length, cancellationToken ) ;
		}

		/// <summary>
		/// グループへ参加する　※フリーユーザー限定行動
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <param name="password"></param>
		/// <param name="parameters"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<JoinToGroup_Response> JoinToGroupAsync
		(
			string						userId,
			uint						groupId,
			string						password,
			Dictionary<string,string>	parameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.JoinToGroupAsync( userId, groupId, password, parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループ固有パラメータを設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<SetGroupParameter_Response> SetGroupParameterAsync
		(
			Dictionary<string,string> parameters,
			CancellationToken cancellationToken
		)
		{
			return await m_GroupingServerProcessor.SetGroupParameterAsync( parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループのメンバー固有パラメータを設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<SetGroupMemberParameter_Response> SetGroupMemberParameterAsync
		(
			Dictionary<string,string>   parameters,
			CancellationToken           cancellationToken
		)
		{
			return await m_GroupingServerProcessor.SetGroupMemberParameterAsync( parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループのメンバー準備可能を設定する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<SetGroupMemberReady_Response> SetGroupMemberReadyAsync
		(
			bool                        isReady,
			Dictionary<string,string>   parameters,
			CancellationToken           cancellationToken
		)
		{
			return await m_GroupingServerProcessor.SetGroupMemberReadyAsync( isReady, parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループから離脱(グループを解散)する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<LeaveFromGroup_Response> LeaveFromGroupAsync
		(
			CancellationToken cancellationToken
		)
		{
			return await m_GroupingServerProcessor.LeaveFromGroupAsync( cancellationToken ) ;
		}

		/// <summary>
		/// メンバーをグループから排除する　※リーダー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<RejectGroupMember_Response> RejectGroupMemberAsync
		(
			string				userId,
			CancellationToken	cancellationToken
		)
		{
			return await m_GroupingServerProcessor.RejectGroupMemberAsync( userId, cancellationToken ) ;
		}

		/// <summary>
		/// グループを生成する　※リーダー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<CreateGroup_Response> CreateGroupAsync
		(
			string						applicationId,
			GroupTypes					groupType,
			string						password,
			bool                        isAutomaticMatchingStarting,
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.CreateGroupAsync
			(
				applicationId,
				groupType,
				password,
				isAutomaticMatchingStarting,
				groupParameters,
				groupMemberParameters,
				cancellationToken
			) ;
		}

		/// <summary>
		/// ソロのマッチングを開始する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<StartSoloMatching_Response> StartSoloMatchingAsync
		(
			string						applicationId,
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.StartSoloMatchingAsync( applicationId, groupParameters, groupMemberParameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループのマッチングを開始する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<StartGroupMatching_Response> StartGroupMatchingAsync
		(
			Dictionary<string,string>	groupParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.StartGroupMatchingAsync( groupParameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループのマッチングを取消する
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<StopMatching_Response> StopMatchingAsync
		(
			CancellationToken			cancellationToken
		)
		{
			return await m_GroupingServerProcessor.StopMatchingAsync( cancellationToken ) ;
		}


		//===================================================================================================================

		/// <summary>
		/// グルーピングサーバーとの通信処理クラス
		/// </summary>
		public class GroupingServerProcesor
		{
			private readonly DefaultNetworkPlayClientAdapter	m_Owner ;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="owner"></param>
			public GroupingServerProcesor( DefaultNetworkPlayClientAdapter owner )
			{
				m_Owner = owner ;
			}

			/// <summary>
			/// 破棄する
			/// </summary>
			public void Dispose()
			{
				DeleteSocketClient() ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// アドレス
			/// </summary>
			public string					Address { get ; set ; }

			/// <summary>
			/// ＴＣＰポート
			/// </summary>
			public ushort					TcpPort { get ; set ; }

			/// <summary>
			/// ＵＤＰポート
			/// </summary>
			public ushort					UdpPort { get ; set ; }


			//----------------------------------------------------------

			/// <summary>
			/// 通知機能が有効になっているかどうか
			/// </summary>
			public  bool					IsConnected { get ; set ; } = false ;

			/// <summary>
			/// 完全に準備が整っている状態かどうか
			/// </summary>
			public	bool					IsReady
			{
				get
				{
					return m_ClientPhase == ClientPhases.Ready ;
				}
			}

			//-----------------------------------------------------------

			/// <summary>
			/// KeepAlive を使用するかどうか
			/// </summary>
			public bool						UseKeepAlive = true ;


			//-------------------------------------------------------------------------------------------

			// セッション通信用のソケットクライアント
			private SocketClient			m_SocketClient ;

			// セッションサーバーと通信時のタスクキャンセル用
			private CancellationTokenSource m_CancellationTokenSource ;

			// メインスレッドのコンテキスト
			private readonly SynchronizationContext	m_MainThreadContext ;

			//-------------------------------------------------------------------------------------------

			/// <summary>
			/// クライアントの状態
			/// </summary>
			public enum ClientPhases
			{
				/// <summary>
				/// 初期状態
				/// </summary>
				None			= 0,

				/// <summary>
				/// バインド要求中
				/// </summary>
				RequestBinding	= 1,

				/// <summary>
				/// 接続状態継続中
				/// </summary>
				Connecting		= 2,

				/// <summary>
				/// フレームの通信可能な状態
				/// </summary>
				Ready			= 3,

				/// <summary>
				/// 切断状態継続中
				/// </summary>
				Disconnecting	= 9,
			}

			// クライアントの状態
			private ClientPhases	m_ClientPhase		= ClientPhases.None ;

			//-----------------------------------

			// タイマー
			private long								m_LastSendTime ;

			//-----------------------------------------------------------
			// 受信時に呼び出すコールバック

			// 接続した際に呼び出される
			private Action								m_OnConnected ;

			// マッチングの結果を受信した際に呼び出されるコールバック
			private Action<MatchingToSessionResult>		m_OnMatchingToSession ;

			// 通常のフレームを受信した際に呼び出されるコールバック
			private Action<byte[]>						m_OnReceived ;

			// 切断された際に呼び出される
			private Action								m_OnDisconnected ;

			//-----------------------------------------------------------
	
			/// <summary>
			/// フレームの通信準備が整っているかどうか
			/// </summary>
			public bool Ready
			{
				get
				{
					return ( m_ClientPhase == ClientPhases.Ready ) ;
				}
			}

			//-------------------------------------------------------------------------------------------

			/// <summary>
			/// 自身がノーティフィケーションサーバーに接続した際に呼び出されるコールバックを設定する
			/// </summary>
			/// <param name="onDisconnected"></param>
			public void SetOnConnected( Action onConnected )
			{
				m_OnConnected = onConnected ;
			}

			/// <summary>
			/// データを受信した際に呼び出されるコールバックを設定する
			/// </summary>
			/// <param name="onReceived"></param>
			/// <param name="onReceivedToHost"></param>
			public void SetOnReceived( Action<byte[]> onReceived )
			{
				m_OnReceived = onReceived ;
			}

			/// <summary>
			/// データを受信した際に呼び出されるコールバックを取得する
			/// </summary>
			/// <returns></returns>
			public Action<byte[]> GetOnReceived()
			{
				return m_OnReceived ;
			}

			/// <summary>
			/// セッションにマッチングした際に呼び出されるコールバックを設定する
			/// </summary>
			/// <param name="onMatchingToSession"></param>
			public void SetOnMatchingToSession( Action<MatchingToSessionResult> onMatchingToSession )
			{
				m_OnMatchingToSession = onMatchingToSession ;
			}

			/// <summary>
			/// 自身がセッションから離脱した際に呼び出されるコールバックを設定する
			/// </summary>
			/// <param name="onDisconnected"></param>
			public void SetOnDisconnected( Action onDisconnected )
			{
				m_OnDisconnected = onDisconnected ;
			}

			//---------------

			/// <summary>
			/// 通知サービスを停止する
			/// </summary>
			public void StopService()
			{
				if( IsConnected == false )
				{
					return ;
				}

				if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
				{
					Debug.Log( "クライアントによる明示的切断(1)" ) ;
					m_ClientPhase  = ClientPhases.Disconnecting ;
				}
			}

			//-------------------------------------------------------------------------------------------------------------------

			// リアルタイム通信用のソケットクライアントを生成する
			private bool CreateSocketClient( CancellationToken ownerCancellationToken )
			{
				if( m_SocketClient != null )
				{
					// 既に生成済みは異常
					return false ;
				}

				//----------------------------------------------------------
				// タスク中断用のキャンセレーショントークン生成

				if( ownerCancellationToken == default )
				{
					m_CancellationTokenSource = new CancellationTokenSource() ;
				}
				else
				{
					m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( ownerCancellationToken ) ;
				}

				//----------------------------------------------------------

				// リアルタイム通信用のクライアントソケット生成
				m_SocketClient = new SocketClient
				(
					OnTcpReceived,
					OnTcpDeiconnected,
					null,
					m_Owner.MaxTcpPacketSize,
					m_CancellationTokenSource.Token
				) ;

				//----------------------------------------------------------
	
				// クライアントの状態を初期状態に初期化
				m_ClientPhase = ClientPhases.None ;

				//---------------------------------

				return true ;
			}

			// リアルタイム通信用のソケットクライアントを破棄する
			private void DeleteSocketClient()
			{
				// タスクをキャンセルする
				if( m_CancellationTokenSource != null )
				{
					if( m_CancellationTokenSource.IsCancellationRequested == false )
					{
						m_CancellationTokenSource.Cancel() ;
					}

					m_CancellationTokenSource.Dispose() ;
					m_CancellationTokenSource = null ;
				}

				// リアルタイム通信用のソケットクライアントを破棄する
				if( m_SocketClient != null )
				{
					m_SocketClient.Dispose() ;
					m_SocketClient = null ;
				}
			}

			//------------------------------------------------------------------------------------------

			/// <summary>
			/// ノーティフィケーションサーバーにＴＣＰで接続する
			/// </summary>
			/// <param name="address"></param>
			/// <param name="tcpPort"></param>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			/// <exception cref="OperationCanceledException"></exception>
			public async Task<( ResponseCodes,string )> Connect
			(
				string						address,
				ushort						tcpPort,
				Dictionary<string, string>	parameters,
				CancellationToken			ownerCancellationToken,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// ソケット生成
				if( CreateSocketClient( ownerCancellationToken ) == false )
				{
					return ( ResponseCodes.CouldNotConnectToSessionServer, "既にソケットが生成されている" ) ;
				}

				//----------------------------------------------------------
				// タスク中断用のキャンセレーショントークン生成

				CancellationTokenSource cancellationTokenSource ;
				bool isCanceled ;

				//----------------------------------------------------------
				// サーバーへの接続を行う

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token, cancellationToken ) ;
				}

				bool isConnected = false ;
				isCanceled = false ;

				try
				{
					// サーバーへＴＣＰ接続を行う(接続実行と受信開始)
					isConnected = await m_SocketClient.ConnectAsync( address, tcpPort, null, cancellationTokenSource.Token ) ;
				}
				catch( Exception e )
				{
					if( e is OperationCanceledException )
					{
						isCanceled = true ;
					}
					else
					{
						Debug.LogError( e.Message ) ;
					}
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				if( isConnected == false )
				{
					// 失敗
					return ( ResponseCodes.CouldNotConnectToSessionServer, "グルーピングサーバーとの接続に失敗しました\n" + address + " : " + tcpPort ) ;
				}

				Debug.Log( "<color=#FFFF00>無事に GroupingServer に接続 : クライアント側のポート番号 = " + m_SocketClient.GetTcpPort() + "</color>" ) ;

				//----------------------------------------------------------

				// 接続状態とする
				IsConnected = true ;

				//----------------------------------------------------------

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				//------------------------------------------------------------------------------------------

	//			Debug.Log( "-------状態 : " + m_ClientPhase ) ;

				// バインド要求を出している最中(送信前に状態を変える事)
				m_ClientPhase = ClientPhases.RequestBinding ;

				// クライアントとセッションプレイヤーのバインド要求を送る
				SendBindClientToUser( parameters ) ;

				while( m_SocketClient != null )
				{
					if( m_SocketClient.GetSendingTcpPacketCount() == 0 )
					{
						// 送信完了
						break ;
					}

					if( cancellationTokenSource.IsCancellationRequested == true )
					{
						isCanceled = true ;
						break ;
					}

					await Task.Yield() ;
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				Debug.Log( "<color=#FFFF00>GroupingServer にバインド要求を送信した</color>" ) ;

				//----------------------------------------------------------
				// 応答としてのセッションプレイヤー情報受信を待つ

				//----------------------------------------------------------

				// バインドが完了するまで待機する

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				while( m_SocketClient != null && m_CancellationTokenSource != null )
				{
					if( m_CancellationTokenSource.IsCancellationRequested == true )
					{
						isCanceled = true ;
						break ;
					}

					if( m_ClientPhase != ClientPhases.RequestBinding )
					{
						// 待機終了
						break ;
					}

					await Task.Yield() ;
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( m_ClientPhase == ClientPhases.Disconnecting )
				{
					// バインド失敗
					m_ClientPhase = ClientPhases.None ;

					DeleteSocketClient() ;

					IsConnected = false ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				if( m_ClientPhase == ClientPhases.Disconnecting || m_ClientPhase == ClientPhases.None )
				{
					// 失敗
					return ( ResponseCodes.CouldNotConnectToSessionServer, "グルーピングサーバーとのバインドに失敗しました : ClientPhase = " + m_ClientPhase ) ;
				}

				Debug.Log( "<color=#FFFF00>GroupingServer にバインド完了 : m_ClientPhase = " + m_ClientPhase + "</color>" ) ;

				//----------------------------------

				Debug.Log( "<color=#FFFF00>通知の受け取りが可能な状態になった</color>" ) ;

				//------------------------------------------------------------------------------------------

				// 監視タスクを起動する
				_ = ProcessClient() ;

				// 接続成功
				return ( ResponseCodes.Succeeded, string.Empty ) ;
			}

			// セッションの接続状況監視用の非同期タスク
			private async Task ProcessClient()
			{
				bool isCanceled = false ;

				long nowTicks ;

				long keepAliveIntervalTime = 300 * 1000 ;	// ３００(５分)秒経過

				while( m_SocketClient != null && m_CancellationTokenSource != null )
				{
					if( m_CancellationTokenSource.IsCancellationRequested == true )
					{
						// タスクがキャンセルされた
						isCanceled = true ;
						break ;
					}

					if( m_ClientPhase == ClientPhases.Ready )
					{
						// 接続状態を継続している

						//-------------------------------------------------------
						// グルーピングアクションのレスポンスを処理する

						ProcessAllGroupingActionResponses() ;

						//-------------------------------------------------------

						nowTicks = Timer.NowTicks ;

						if( UseKeepAlive == true )
						{
							// KeepAlive を使用する

							if( ( nowTicks - m_LastSendTime ) >= keepAliveIntervalTime )
							{
								// 前回の送信から一定時間経過

								// KeepAlive を送信する
								SendKeepAlive() ;
							}
						}
					}

					if( m_ClientPhase == ClientPhases.Disconnecting )
					{
						// ソケットが切断状態になっている
						Debug.Log( "ソケットの切断要求が出されている" ) ;
						break ;
					}

					// 少し待つ
					await Task.Yield() ;
				}

				//----------------------------------------------------------
				// 後始末を行う

				CloseService() ;

				//----------------------------------

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				//----------------------------------
				// 順番に注意(タスクキャンセルならコールバックは呼び出さない)

				if( m_OnDisconnected != null )
				{
					CallOnDisconnected() ;
				}
			}

			/// <summary>
			/// 強制的にサービスを終了させる
			/// </summary>
			public void CloseService()
			{
				m_ClientPhase = ClientPhases.None ;

				DeleteSocketClient() ;

				IsConnected	= false ;
			}

			//-------------------------------------------------------------------------------------------

			/// <summary>
			/// セッションから離脱する(離脱を完了するまで待つ)
			/// </summary>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			public async Task StopServiceAsync( CancellationToken cancellationToken = default )
			{
				if( IsConnected == false )
				{
					return ;
				}

				if( m_ClientPhase != ClientPhases.None && m_ClientPhase != ClientPhases.Disconnecting )
				{
					Debug.Log( "クライアントによる明示的切断(2)" ) ;
					m_ClientPhase  = ClientPhases.Disconnecting ;
				}
				else
				{
					return ;
				}

				//----------------------------------

				CancellationTokenSource cancellationTokenSource ;
				bool isCanceled ;

				//----------------------------------------------------------
				// セッションサーバーへからの離脱を行う

				if( cancellationToken == default )
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token ) ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				while( m_SocketClient != null && m_CancellationTokenSource != null && IsConnected == true )
				{
					if( cancellationTokenSource.IsCancellationRequested == true )
					{
						isCanceled = true ;
						break ;
					}

					await Task.Yield() ;
				}

				if( cancellationTokenSource.IsCancellationRequested == false )
				{
					// 念の為キャンセルが発行されていなければキャンセルする
					cancellationTokenSource.Cancel() ;
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}
			}


			//-------------------------------------------------------------------------------------------
			// SocketHelper 関連のコールバック

			// ＴＣＰパケットを受信した際に呼び出されるコールバック
			private void OnTcpReceived( ReadOnlyMemory<byte> data )
			{
				if( m_ClientPhase != ClientPhases.RequestBinding && m_ClientPhase != ClientPhases.Connecting && m_ClientPhase != ClientPhases.Ready )
				{
					// 異常な状態でコールバックが呼ばれたので無視する
					Debug.Log( "ＴＣＰ受信時のフェーズ異常による切断 : " + m_ClientPhase ) ;
					m_ClientPhase = ClientPhases.Disconnecting ;
					return ;
				}

				//----------------------------------------------------------

				// 全体で復号化する
				byte[] commandData = m_Owner.Crypter.DecryptXor( data.Span ) ;

				if( commandData == null || commandData.Length <  1 )
				{
					// 不正レスポンス(結果として切断する)
					Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(1) : " + m_ClientPhase ) ;
					m_ClientPhase = ClientPhases.Disconnecting ;
					return ;
				}

				//----------------------------------------------------------

				var commandType = ( CommandTypes )commandData[ 0 ] ;

//				Debug.Log( "<color=#3F7FFF>-------------->[TCP] コマンド受信 = " + commandType + " 現在のフェーズ : " + m_ClientPhase + "</color>" ) ;

				if( m_ClientPhase == ClientPhases.RequestBinding )
				{
					// まだバインドされていない

					if( commandType == CommandTypes.BindClientToUserComplated )
					{
						// バインド完了通知
						try
						{
							int offset = 1 ;

							long pingTicks = DataFormat.GetLong( commandData, ref offset ) ;

							Debug.Log( "<color=#3FFF3F>[GroupingServer] ------バインド実行時の往復時間 " + ( Timer.NowTicks - pingTicks ) + " ms</color>" ) ;
						}
						catch( Exception )
						{
							// データ異常
							Debug.LogWarning( "ＴＣＰ受信時のデータ異常による切断(2) : " + m_ClientPhase ) ;
							m_ClientPhase = ClientPhases.Disconnecting ;

							// 失敗
							return ;
						}

						//--------------------------------------------------------

						// 接続が確立された状態に以降する
						m_ClientPhase = ClientPhases.Ready ;

						// タイマー計測開始(初期化)
						m_LastSendTime = Timer.NowTicks	;	// これはローカルのタイマーを使用する

						Debug.Log( "<color=#FFFF00>[受信] 待ち受けユーザーとのバインド状態に移行した</color>" ) ;

						// 成功
						return ;
					}
				}

				if( m_ClientPhase == ClientPhases.Ready )
				{
					// フレームの通信が可能な状態

					if( commandType == CommandTypes.GroupingAction )
					{
						// グルーピングアクション

						int offset = 1 ;
						OnGroupingAction( commandData, ref offset ) ;

						return ;
					}
					else
					if( commandType == CommandTypes.NotificationMessage )
					{
						// フレーム(一般的なデータ)を受信

						int offset = 1 ;
						OnNotificationMessage( commandData, ref offset ) ;

						return ;
					}
				}

				//----------------------------------------------------------

				// ここに来るのはいずれのコマンドにも該当しなかったという事なので不正アクセス
				Debug.LogWarning( "ＴＣＰ受信時にいずれのコマンドに該当しなかった : Phase = " + m_ClientPhase + " Command = " + commandType ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
			}

			// ＴＣＰが切断された際に呼び出されるコールバック
			private void OnTcpDeiconnected()
			{
				// 切断状態
				Debug.LogWarning( "[GroupingServer] ＴＣＰ接続がサーバーから切断された : " + m_ClientPhase ) ;
				m_ClientPhase = ClientPhases.Disconnecting ;
			}

			//-------------------------------------------------------------------------------------------
			// NetworkPlay 関連のコールバック


			//------------------------------------------------------------------------------------------
			// グルーピングアクション関連

			private readonly object m_GroupingActionResponseQueue_LockObject = new () ;

			/// <summary>
			/// グルーピングアクションのレスポンス
			/// </summary>
			public class GroupingActionResponse
			{
				public GroupingActionTypes			GroupingActionType ;

				public GroupingActionResponseCodes	ResponseCode ;
				public string						ErrorMessage ;

				public bool							IsAsyncGroupingActionResponse ;
				public uint							AsyncGroupingActionSequence ;

				public byte[]						GroupingActionData ;
			}


			// グルーピングアクションのレスポンスのキュー
			private readonly List<GroupingActionResponse>	m_GroupingActionResponseQueue = new () ;

			// 非同期系グルーピングアクションに対して該当するレスポンスが返った事を通知するためのデータクラス
			public class AsyncGroupingActionResult
			{
				public uint							AsyncGroupingActionSequence ;

				public GroupingActionResponseCodes	ResponseCode ;
				public string						ErrorMessage ;
			}

			// 非同期グルーピングアクション用の結果キュー
			private readonly List<AsyncGroupingActionResult> m_AsyncGroupingActionResultQueue = new () ;

			//----------------------------------------------------------

			// ※多目的パラメータ内にキャラクターアイコン情報などを格納する

			// 関連性のあるユーザー情報群
			private readonly List<RelatedUserData> m_RelatedUsers = new () ;

			/// <summary>
			/// 関連ユーザー情報を群
			/// </summary>
			public List<RelatedUserData> RelatedUsers => m_RelatedUsers ;

			//--------------

			// 関連性のユーザーに変動があった際に呼び出されるコールバック
			private  Action<List<RelatedUserData>> m_OnRelatedUserUpdated ;

			/// <summary>
			/// 関連性のユーザーに変動があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
			{
				m_OnRelatedUserUpdated -= onRelatedUserUpdated ;
				m_OnRelatedUserUpdated += onRelatedUserUpdated ;
			}

			/// <summary>
			/// 関連性のユーザーに変動があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
			{
				m_OnRelatedUserUpdated -= onRelatedUserUpdated ;
			}

			//--------------

			// グループ招待に応答があった際に呼び出されるコールバック
			private  Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> m_OnGroupInvitationResponded ;

			/// <summary>
			/// グループ招待に応答があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
			{
				m_OnGroupInvitationResponded -= onGroupInvitationResponded ;
				m_OnGroupInvitationResponded += onGroupInvitationResponded ;
			}

			/// <summary>
			/// グループ招待に応答があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
			{
				m_OnGroupInvitationResponded -= onGroupInvitationResponded ;
			}

			//----------------------------------

			// グループ招待情報群
			private readonly List<GroupInvitationData> m_GroupInvitations = new () ;

			/// <summary>
			/// グループ招待情報群
			/// </summary>
			public List<GroupInvitationData> GroupInvitations => m_GroupInvitations ;

			/// <summary>
			/// 受けている招待の数
			/// </summary>
			public int GroupInvitationReceivingCount => m_GroupInvitations.Count ;

			//--------------

			// グループ招待に変動があった際に呼び出されるコールバック
			private  Action<int,GroupInvitationData,List<GroupInvitationData>> m_OnGroupInvitationUpdated ;

			/// <summary>
			/// グループ招待に変動があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
			{
				m_OnGroupInvitationUpdated -= onGroupInvitationUpdated ;
				m_OnGroupInvitationUpdated += onGroupInvitationUpdated ;
			}

			/// <summary>
			/// グループ招待に変動があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
			{
				m_OnGroupInvitationUpdated -= onGroupInvitationUpdated ;
			}

			//----------------------------------

			//--------------

			// ※多目的パラメータ内に希望の陣営情報なども格納する

			// グループパラメータ
			private Dictionary<string,string>	m_GroupParameters = new () ;

			/// <summary>
			/// グループパラメータ
			/// </summary>
			public Dictionary<string,string>	GroupParameters	=> m_GroupParameters ;

			//--------------

			// グループ状態に変化があった際に呼び出されるコールバック
			private  Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> m_OnGroupUpdated ;

			/// <summary>
			/// グループパラメータに変動があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
			{
				m_OnGroupUpdated -= onGroupUpdated ;
				m_OnGroupUpdated += onGroupUpdated ;
			}

			/// <summary>
			/// グループパラメータに変動があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
			{
				m_OnGroupUpdated -= onGroupUpdated ;
			}

			//--------------

			// ※多目的パラメータ内に希望の陣営情報なども格納する

			// 自身が作成または参加したグループのメンバー情報群
			private List<GroupMemberData>	m_GroupMembers = new () ;

			/// <summary>
			/// グループメンバー情報群
			/// </summary>
			public List<GroupMemberData>	GroupMembers	=> m_GroupMembers ;

			//--------------

			// グループメンバーに変動があった際に呼び出されるコールバック
			private  Action<int,GroupMemberData,List<GroupMemberData>> m_OnGroupMemberUpdated ;

			/// <summary>
			/// グループメンバーに変動があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
			{
				m_OnGroupMemberUpdated -= onGroupMemberUpdated ;
				m_OnGroupMemberUpdated += onGroupMemberUpdated ;
			}

			/// <summary>
			/// グループメンバーに変動があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
			{
				m_OnGroupMemberUpdated -= onGroupMemberUpdated ;
			}

			//----------------------------------
			// キャッシュするグループ情報群

			// キャッシュされたグループ情報群
			private readonly List<Group> m_CachedGroups = new () ;

			/// <summary>
			/// キャッシュされたグループ情報群
			/// </summary>
			public List<Group> CachedGroups => m_CachedGroups ;


			// グループ情報群のオフセット
			private ushort    m_OffsetOfGroups = 0 ;

			/// <summary>
			/// グループ情報群のオフセット
			/// </summary>
			public  ushort    OffsetOfGroups => m_OffsetOfGroups ;

			// グループ情報群のレングス
			private ushort    m_LengthOfGroups = 0 ;

			/// <summary>
			/// グループ情報群のレングス
			/// </summary>
			public  ushort    LengthOfGroups => m_LengthOfGroups ;

			// グループ情報群のカウント(全体数)
			private ushort    m_CountOfGroups = 0 ;

			/// <summary>
			/// グループ情報群のカウント(全体数)
			/// </summary>
			public  ushort    CountOfGroups => m_CountOfGroups ;

			//----------------------------------

			// 現在マッチング処理中かどうか
			private	bool	m_IsMatchingRunning = false ;

			/// <summary>
			/// 現在マッチング中かどうか
			/// </summary>
			public	bool	IsMatchingRunning => m_IsMatchingRunning ;

			// マッチングを取消できるかどうか
			private bool	m_IsMatchingStoppable = false ;

			/// <summary>
			/// マッチングを取消できるかどうか
			/// </summary>
			public	bool	IsMatchingStoppable => m_IsMatchingStoppable ;

			//----------------------------------

			// マッチングが開始された際に呼び出されるコールバック
			private  Action<MatchingStatus> m_OnMatchingStatusUpdated ;

			/// <summary>
			/// グループメンバーに変動があった際に呼び出されるコールバックを登録する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void AddOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
			{
				m_OnMatchingStatusUpdated -= onMatchingStatusUpdated ;
				m_OnMatchingStatusUpdated += onMatchingStatusUpdated ;
			}

			/// <summary>
			/// グループメンバーに変動があった際に呼び出されるコールバックを削除する
			/// </summary>
			/// <param name="onGroupInvitationUpdated"></param>
			public void RemoveOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
			{
				m_OnMatchingStatusUpdated -= onMatchingStatusUpdated ;
			}

			//----------------------------------

			// グループタイプ
			private GroupTypes	m_GroupType = GroupTypes.None ;

			/// <summary>
			/// グループタイプ
			/// </summary>
			public GroupTypes	GroupType	=> m_GroupType ;

			// グループ識別子
			private uint		m_GroupId	= 0 ;

			/// <summary>
			/// グループ識別子
			/// </summary>
			public uint			GroupId		=> m_GroupId ;

			// 自身がグループのリーダーであるかどうか
			private bool		m_IsGroupLeader	= false ;

			/// <summary>
			/// 自身がグループのリーダーであるかどうか
			/// </summary>
			public bool			IsGroupLeader	=> m_IsGroupLeader ;

			/// <summary>
			/// グループの準備完了を実行中か
			/// </summary>
			public bool         IsGroupReady
			{
				get
				{
					if( m_GroupMembers == null || m_GroupMembers.Count == 0 )
					{
						// メンバー情報が存在しない
						return false ;
					}

					// メンバー情報内の自身の固有パラメータを更新する
					var myselfGroupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == m_Owner.UserId ) ;
					if( myselfGroupMember == null )
					{
						// メンバー情報が存在しない
						return false ;
					}

					return myselfGroupMember.IsReady ;
				}
			}

			//----------------------------------------------------------


			// グルーピングアクションを受信した際に呼び出される
			private void OnGroupingAction
			(
				byte[] commandData,
				ref int offset
			)
			{
				//-----------------------------------------------------------------------------------------

				try
				{
					// グルーピングアクションの種別
					var groupingActionType = ( GroupingActionTypes )DataFormat.GetByte( commandData, ref offset ) ;

					// グーピングアクションの結果
					var responseCode	= ( GroupingActionResponseCodes )DataFormat.GetByte( commandData, ref offset ) ;
					var errorMessage	= DataFormat.GetString( commandData, ref offset ) ;

					// 非同期グルーピングアクション(待機あり)のレスポンスであるかどうか
					var isAsyncGroupingActionResponse = DataFormat.GetBool( commandData, ref offset ) ;
					uint asyncGroupingActionSequence = 0 ;
					if( isAsyncGroupingActionResponse == true )
					{
						// 非同期グルーピングアクション(待機あり)のレスポンス
						asyncGroupingActionSequence = DataFormat.GetUInt( commandData, ref offset ) ;
					}

					// データ部
					var groupingActionData = DataFormat.GetByteArray( commandData, ref offset ) ;

					//--------------------------------------------------------
					// 受信キューに貯める(メインスレッドで取り出して処理する)

					lock( m_GroupingActionResponseQueue_LockObject )
					{
						// グルーピングアクションのレスポンスキューにグルーピングアクションレスポンスを貯める
						m_GroupingActionResponseQueue.Add( new ()
						{
							GroupingActionType				= groupingActionType,

							ResponseCode					= responseCode,
							ErrorMessage					= errorMessage,

							IsAsyncGroupingActionResponse	= isAsyncGroupingActionResponse,
							AsyncGroupingActionSequence		= asyncGroupingActionSequence,

							GroupingActionData				= groupingActionData
						} ) ;
					}
				}
				catch( Exception e )
				{
					Debug.LogWarning( "グルーピングアクションの受信データが異常です\n" + e.Message ) ;
				}
			}

			/// <summary>
			/// キューに溜まっている全てのグルーピングアクションレスポンスを処理する(メインスレッド)
			/// </summary>
			public void ProcessAllGroupingActionResponses()
			{
				lock( m_GroupingActionResponseQueue_LockObject )
				{
					// キューに溜まっているものを全て処理する
					while( m_GroupingActionResponseQueue.Count >  0 )
					{
						var groupingActionResponse = m_GroupingActionResponseQueue[ 0 ] ;
						m_GroupingActionResponseQueue.Remove( groupingActionResponse ) ;

						// 取り出した１つのグルーピングアクションレスポンスを処理する
						ProcessGroupingActionResponse( groupingActionResponse ) ;
					}
				}
			}

			// グルーピングアクションレスポンスを処理する(キューがロック中である事に注意)
			private void ProcessGroupingActionResponse( GroupingActionResponse groupingActionResponse )
			{
				// 非同期グルーピングアクション系(自身由来の待機あり)かプッシュ通知(他者由来の待機なし)か処理が分かれる

				if( groupingActionResponse.IsAsyncGroupingActionResponse == true )
				{
					// 自身由来(待機あり)

					// アクションの種別によって処理の方法が異なる

					if( groupingActionResponse.ResponseCode == GroupingActionResponseCodes.Succeeded )
					{
						// 基本的に成功していなければ状態は更新されない

						Debug.Log( "<color=#FF7F00>-------->[Grouping]待機レスポンス : " + groupingActionResponse.GroupingActionType + "</color>" ) ;

						switch( groupingActionResponse.GroupingActionType )
						{
							// 関係性のあるユーザー情報群の確認
							case GroupingActionTypes.CheckRelatedUsers :
								ProcessGroupingActionResponse_CheckRelatedUsers( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待を送信する
							case GroupingActionTypes.AffordGroupInvitation :
								ProcessGroupingActionResponse_AffordGroupInvitation( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待を取消する
							case GroupingActionTypes.CancelGroupInvitation :
								ProcessGroupingActionResponse_CancelGroupInvitation( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待に了承する
							case GroupingActionTypes.AcceptGroupInvitation :
								ProcessGroupingActionResponse_AcceptGroupInvitation( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待を拒否する
							case GroupingActionTypes.RejectGroupInvitation :
								ProcessGroupingActionResponse_RejectGroupInvitation( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループ情報群を取得する
							case GroupingActionTypes.GetGroups :
								ProcessGroupingActionResponse_GetGroups( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループに参加する
							case GroupingActionTypes.JoinToGroup :
								ProcessGroupingActionResponse_JoinToGroup( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループ固有パラメータを更新する
							case GroupingActionTypes.SetGroupParameter :
								ProcessGroupingActionResponse_SetGroupParameter( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループ内の固有パラメータを更新する
							case GroupingActionTypes.SetGroupMemberParameter :
								ProcessGroupingActionResponse_SetGroupMemberParameter( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループ内の準備完了状態を設定する
							case GroupingActionTypes.SetGroupMemberReady :
								ProcessGroupingActionResponse_SetGroupMemberReady( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループから離脱する
							case GroupingActionTypes.LeaveFromGroup :
								ProcessGroupingActionResponse_LeaveFromGroup( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループから排除する
							case GroupingActionTypes.RejectGroupMember :
								ProcessGroupingActionResponse_RejectGroupMember( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループを生成する
							case GroupingActionTypes.CreateGroup :
								ProcessGroupingActionResponse_CreateGroup( groupingActionResponse.GroupingActionData ) ;
							break ;

							// ソロでマッチングを開始する
							case GroupingActionTypes.StartSoloMatching :
								ProcessGroupingActionResponse_StartSoloMatching( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループでマッチングを開始する
							case GroupingActionTypes.StartGroupMatching :
								ProcessGroupingActionResponse_StartGroupMatching( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループでマッチングを取消する
							case GroupingActionTypes.StopMatching :
								ProcessGroupingActionResponse_StopMatching( groupingActionResponse.GroupingActionData ) ;
							break ;
						}
					}

					// レスポンスを受信した事を非同期メソッドに通知するために積む(メインスレッドなので排他制御は気にしなくて良い)
					m_AsyncGroupingActionResultQueue.Add( new ()
					{
						// シーケンス(最も重要)
						AsyncGroupingActionSequence = groupingActionResponse.AsyncGroupingActionSequence,

						ResponseCode				= groupingActionResponse.ResponseCode,
						ErrorMessage				= groupingActionResponse.ErrorMessage
					} ) ;

					// ※非同期グルーピングアクションを並列で実行される可能性を考慮する(並列実行は推奨されない)
				}
				else
				{
					// 他者由来(待機なし)

					// アクションの種別によって処理の方法が異なる

					if( groupingActionResponse.ResponseCode == GroupingActionResponseCodes.Succeeded )
					{
						// 基本的に成功していなければ状態は更新されない


						Debug.Log( "<color=#FF7F00>-------->[Grouping]プッシュ通知 : " + groupingActionResponse.GroupingActionType + "</color>" ) ;

						switch( groupingActionResponse.GroupingActionType )
						{
							// グループへの招待を受け取った　※招待の受け取り側
							case GroupingActionTypes.GroupInvitationReceived :
								ProcessGroupingActionResponse_GroupInvitationReceived( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待が取り消された　※招待の受け取り側
							case GroupingActionTypes.GroupInvitationCanceled :
								ProcessGroupingActionResponse_GroupInvitationCanceled( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待が承諾された　※オーナー側
							case GroupingActionTypes.GroupInvitationAccepted :
								ProcessGroupingActionResponse_GroupInvitationAccepted( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループへの招待が拒否された　※オーナー側
							case GroupingActionTypes.GroupInvitationRejected :
								ProcessGroupingActionResponse_GroupInvitationRejected( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループが生成された
							case GroupingActionTypes.SmallGroupCreated :
								ProcessGroupingActionResponse_SmallGroupCreated( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループにメンバーが参加した
							case GroupingActionTypes.GroupMemberJoined :
								ProcessGroupingActionResponse_GroupMemberJoined( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループメンバーの固有パラメータが更新された
							case GroupingActionTypes.GroupParameterUpdated :
								ProcessGroupingActionResponse_GroupParameterUpdated( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループメンバーの固有パラメータが更新された
							case GroupingActionTypes.GroupMemberParameterUpdated :
								ProcessGroupingActionResponse_GroupMemberParameterUpdated( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループメンバーの準備完了が更新された
							case GroupingActionTypes.GroupMemberReadyUpdated :
								ProcessGroupingActionResponse_GroupMemberReadyUpdated( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループメンバーが離脱した
							case GroupingActionTypes.GroupMemberLeft :
								ProcessGroupingActionResponse_GroupMemberLeft( groupingActionResponse.GroupingActionData ) ;
							break ;

							// グループメンバー(自分)が排除された
							case GroupingActionTypes.GroupMemberRejected :
								ProcessGroupingActionResponse_GroupMemberRejected( groupingActionResponse.GroupingActionData ) ;
							break ;

							// 参加中のグループが解散した
							case GroupingActionTypes.GroupDeleted :
								ProcessGroupingActionResponse_GroupDeleted( groupingActionResponse.GroupingActionData ) ;
							break ;

							//----------

							// マッチングが開始された
							case GroupingActionTypes.MatchingStarted :
								ProcessGroupingActionResponse_MatchingStarted( groupingActionResponse.GroupingActionData ) ;
							break ;

							// マッチングが完了した(セッションに加わった状態になって呼び出される)
							case GroupingActionTypes.MatchingCompleted :
								ProcessGroupingActionResponse_MatchingCompleted( groupingActionResponse.GroupingActionData ) ;
							break ;
#if false
							// マッチングが失敗した(セッションに加わった状態になって呼び出される)
							case GroupingActionTypes.MatchingFailed :
								ProcessGroupingActionResponse_MatchingFailed( groupingActionResponse.GroupingActionData ) ;
							break ;
#endif
							// マッチングが中止された(Smallグループの場合は全員準備完了状態を解除する)
							case GroupingActionTypes.MatchingCanceled :
								ProcessGroupingActionResponse_MatchingCanceled( groupingActionResponse.GroupingActionData ) ;
							break ;
						}
					}
				}
			}

			// 各ユーザーに対する各種状態値を展開する
			private bool ProcessGroupingActionResponse_CheckRelatedUsers( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// オンラインであるユーザーの件数
					ushort i, l = DataFormat.GetVUShort( data, ref offset ) ;

					// オンラインであったユーザーの
					// ・ユーザー識別子
					// ・グループに参加済みかどうか
					// が取得できる

					string						userId ;
					string						applicationId ;
					GroupTypes					groupType ;
					bool						isInviting ;
					Dictionary<string, string>	parameters ;
					uint                        sessionId ;

					// グループの所属状況を取得する
					var onlineUsers = new Dictionary<string, ( string ApplicationId, GroupTypes GroupType, bool IsInviting, Dictionary<string, string> Parameters, uint SessionId )>() ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						// ユーザー識別子
						userId			= DataFormat.GetString( data, ref offset ) ;

						// アプリケーション識別子
						applicationId	= DataFormat.GetString( data, ref offset ) ;

						// グループタイプ
						groupType		= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

						// 招待を出しているかどうか
						isInviting		= DataFormat.GetBool( data, ref offset ) ;

						// 固有パラメータ
						parameters		= GetUserParameters( data, ref offset ) ;

						// 参加中のセッション識別子
						sessionId       = DataFormat.GetUInt( data, ref offset ) ;

						// 追加
						onlineUsers.Add( userId, ( applicationId, groupType, isInviting, parameters, sessionId ) ) ;
					}

					//-------------------------------
					// RelatedUsers に情報を有効させる

					foreach( var relatedUser in m_RelatedUsers )
					{
						if( onlineUsers.ContainsKey( relatedUser.UserId ) == true )
						{
							// このユーザーはオンライン
							var onlineUser = onlineUsers[ relatedUser.UserId ] ;

							relatedUser.IsOnline		= true ;
							relatedUser.ApplicationId	= onlineUser.ApplicationId ;
							relatedUser.GroupType		= onlineUser.GroupType ;
							relatedUser.IsInviting		= onlineUser.IsInviting ;
							relatedUser.Parameters		= onlineUser.Parameters ;
							relatedUser.SessionId       = onlineUser.SessionId ;
						}
						else
						{
							// このユーザーはオフライン
							relatedUser.IsOnline		= false ;
							relatedUser.ApplicationId	= string.Empty  ;
							relatedUser.GroupType		= GroupTypes.None ;
							relatedUser.IsInviting		= false ;
							relatedUser.Parameters		= new () ;
							relatedUser.SessionId       = 0 ;
						}
					}

					// コールバックを呼ぶ
					m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待を付与する　※リーダー側
			private bool ProcessGroupingActionResponse_AffordGroupInvitation( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を出したユーザーのユーザー識別子が返る
					string	userId			= DataFormat.GetString( data, ref offset ) ;

					// オンライン状態かどうか
					bool	isOnline		= DataFormat.GetBool( data, ref offset ) ;

					// アプリケーション識別子
					string applicationId	= DataFormat.GetString( data, ref offset ) ;

					// グループ種別
					var groupType			= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					//--------------------------------

					// 関連性のあるユーザーリストで招待を送信したチェックを入れる
					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						// オンライン状態を更新する
						relatedUser.IsOnline = isOnline ;

						// アプリケーション識別子
						relatedUser.ApplicationId = applicationId ;

						// グループ種別
						relatedUser.GroupType = groupType ;

						if( isOnline == true && groupType == GroupTypes.None )
						{
							// 招待を送っている状態になる
							relatedUser.IsInviting = true ;
						}
						else
						{
							// オフラインでは招待を送っていない状態にする
							relatedUser.IsInviting = false ;
						}

						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}
					else
					{
						Debug.LogWarning( "関連性のあるユーザー情報群に指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}


			// グループへの招待を取消する　※リーダー側
			private bool ProcessGroupingActionResponse_CancelGroupInvitation( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を出したユーザーのユーザー識別子が返る
					string userId		= DataFormat.GetString( data, ref offset ) ;

					// オンライン状態かどうか
					bool	isOnline	= DataFormat.GetBool( data, ref offset ) ;

					//--------------------------------

					// 関連性のあるユーザーリストで招待を送信したチェックを外す
					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						// オンライン状態を更新する
						relatedUser.IsOnline = isOnline ;

						// 招待を送っている状態になる
						relatedUser.IsInviting = false ;

						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}
					else
					{
						Debug.LogWarning( "関連性のあるユーザー情報群に指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待を了承する　※招待を受けた側
			private bool ProcessGroupingActionResponse_AcceptGroupInvitation( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を送ったオーナーのユーザー識別子
					string ownerUserId	= DataFormat.GetString( data, ref offset ) ;

					// グループタイプ
					m_GroupType			= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					// グループ識別子
					m_GroupId			= DataFormat.GetUInt( data, ref offset ) ;

					// リーダーではない
					m_IsGroupLeader		= false ;

					// グループ固有パラメータ
					m_GroupParameters	= GetUserParameters( data, ref offset ) ;

					// グループメンバー群を展開する
					m_GroupMembers		= GetGroupMembers( data, ref offset ) ;

					//--------------------------------
					// 自身が送信していた招待があるなら全て取消扱いになっているばすである
					// 実際の取消処理はサーバー側で行われている

					if( m_GroupType == GroupTypes.None || m_GroupId == 0 )
					{
						// オーナーがオフラインでグループに参加できない

						var groupInvitation = m_GroupInvitations.FirstOrDefault( _ => _.UserId ==  ownerUserId ) ;
						if( groupInvitation != null )
						{
							// オーナーの招待を削除
							m_GroupInvitations.Remove( groupInvitation ) ;

							// コールバックを呼ぶ
							m_OnGroupInvitationUpdated?.Invoke( -1, groupInvitation, m_GroupInvitations ) ;
						}

						return true ;
					}

					//----------------------------------------------------------------------------------------
					// 承諾が通った

					// 自身が出していた招待を全て取消状態にする
					int count = 0 ;
					foreach( var relatedUser in m_RelatedUsers )
					{
						if( relatedUser.IsInviting == true )
						{
							relatedUser.IsInviting = false;
							count ++ ;
						}
					}

					if( count >  0 )
					{
						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}

					//------------

					// 自身が受けていた招待を全て拒否状態にする
					if( m_GroupInvitations.Count >  0 )
					{
						m_GroupInvitations.Clear() ;

						// コールバックを呼ぶ
						m_OnGroupInvitationUpdated?.Invoke( 0, null, m_GroupInvitations ) ;
					}

					//------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Created,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					// グループメンバー
					m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待を拒否する　※招待を受けた側
			private bool ProcessGroupingActionResponse_RejectGroupInvitation( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 拒否したオーナーのユーザー識別子を取得する
					string userId = DataFormat.GetString( data, ref offset ) ;

					// 招待リストから消す
					var groupInvitation = m_GroupInvitations.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupInvitation != null )
					{
						m_GroupInvitations.Remove( groupInvitation ) ;

						// コールバックを呼ぶ
						m_OnGroupInvitationUpdated?.Invoke( -1, groupInvitation, m_GroupInvitations ) ;
					}
					else
					{
						Debug.LogWarning( "招待リストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループ情報群を取得する
			private bool ProcessGroupingActionResponse_GetGroups( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// フィルタ適用後の該当グループ総数
					m_CountOfGroups = DataFormat.GetVUShort( data, ref offset ) ;

					if( m_CountOfGroups >   0 )
					{
						// オフセット
						m_OffsetOfGroups = DataFormat.GetVUShort( data, ref offset ) ;

						// レングス
						m_LengthOfGroups = DataFormat.GetVUShort( data, ref offset ) ;

						//-------------------------------------
						// 展開

						m_CachedGroups.Clear() ;

						for( ushort index  = 0 ; index <  m_LengthOfGroups ; index ++ )
						{
							// グループタイプ
							GroupTypes groupType	    = ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

							// グループ識別子
							uint groupId			    = DataFormat.GetUInt( data, ref offset ) ;

							// パスワードが必要か
							bool hasPassword            = DataFormat.GetBool( data, ref offset ) ;

							// 招待数
							ushort invitationCount      = DataFormat.GetUShort( data, ref offset ) ;

							// 参加数
							ushort nowMembers           = DataFormat.GetUShort( data, ref offset ) ;

							// 最大数
							ushort maxMembers           = DataFormat.GetUShort( data, ref offset ) ;

							// グループの固有パラメータ
							var groupParameters         = GetUserParameters( data, ref offset ) ;

							//-------------

							// リーダーのユーザー識別子
							string groupLeaderUserId    = DataFormat.GetString( data, ref offset ) ;

							// リーダーのユーザー名
							string groupLeaderUserName  = DataFormat.GetString( data, ref offset ) ;

							// リーザーの固有パラメータ
							var groupLeaderParameters   = GetUserParameters( data, ref offset ) ;

							//-------------------------------------

							m_CachedGroups.Add( new
							(
								groupType, groupId, hasPassword, invitationCount, nowMembers, maxMembers, groupParameters,
								groupLeaderUserId, groupLeaderUserName, groupLeaderParameters
							) ) ;
						}
					}
					else
					{
						m_OffsetOfGroups = 0 ;

						m_LengthOfGroups = 0 ;

						m_CachedGroups.Clear() ;
					}

					//-----------------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの参加する　※メンバー
			private bool ProcessGroupingActionResponse_JoinToGroup( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を送ったオーナーのユーザー識別子(空のケースがありえる)
					string ownerUserId	= DataFormat.GetString( data, ref offset ) ;

					// グループタイプ
					m_GroupType			= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					// グループ識別子
					m_GroupId			= DataFormat.GetUInt( data, ref offset ) ;

					// リーダーではない
					m_IsGroupLeader		= false ;

					// グループ固有パラメータ
					m_GroupParameters	= GetUserParameters( data, ref offset ) ;

					// グループメンバー群を展開する
					m_GroupMembers		= GetGroupMembers( data, ref offset ) ;

					//--------------------------------
					// 自身が送信していた招待があるなら全て取消扱いになっているばすである
					// 実際の取消処理はサーバー側で行われている

					if( m_GroupType == GroupTypes.None || m_GroupId == 0 )
					{
						// オーナーがオフラインでグループに参加できない

						if( string.IsNullOrEmpty( ownerUserId ) == false )
						{
							// オーナーの招待に対して承諾したケース
							var groupInvitation = m_GroupInvitations.FirstOrDefault( _ => _.UserId == ownerUserId ) ;
							if( groupInvitation != null )
							{
								// オーナーの招待を削除
								m_GroupInvitations.Remove( groupInvitation ) ;

								// コールバックを呼ぶ
								m_OnGroupInvitationUpdated?.Invoke( -1, groupInvitation, m_GroupInvitations ) ;
							}
						}

						return true ;
					}

					//----------------------------------------------------------------------------------------
					// 承諾が通った

					// 自身が出していた招待を全て取消状態にする
					int count = 0 ;
					foreach( var relatedUser in m_RelatedUsers )
					{
						if( relatedUser.IsInviting == true )
						{
							relatedUser.IsInviting = false ;
							count ++ ;
						}
					}

					if( count >  0 )
					{
						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}

					//------------

					// 自身が受けていた招待を全て拒否状態にする
					if( m_GroupInvitations.Count >  0 )
					{
						m_GroupInvitations.Clear() ;

						// コールバックを呼ぶ
						m_OnGroupInvitationUpdated?.Invoke( 0, null, m_GroupInvitations ) ;
					}

					//------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Created,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					//------------

					// グループメンバー
					m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループ固有パラメータを設定する　※メンバー行動限定
			private bool ProcessGroupingActionResponse_SetGroupParameter( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 固有パラメータ
					var parameters = GetUserParameters( data, ref offset ) ;

					//------------

					m_GroupParameters = parameters ;

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.ParameterUpdated,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループのメンバー固有パラメータを設定する　※メンバー行動限定
			private bool ProcessGroupingActionResponse_SetGroupMemberParameter( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 固有パラメータ
					var parameters = GetUserParameters( data, ref offset ) ;

					//------------

					// メンバー情報内の自身の固有パラメータを更新する
					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == m_Owner.UserId ) ;
					if( groupMember != null )
					{
						groupMember.Parameters = parameters ;

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberUpdated,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
					}
					else
					{
						Debug.LogWarning( "グループメンバーリストに指定のユーザー識別子のものが見つからない UserId = " + m_Owner.UserId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループのメンバー準備完了を設定する　※メンバー行動限定
			private bool ProcessGroupingActionResponse_SetGroupMemberReady( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 準備ＯＫ
					bool	isReady	= DataFormat.GetBool( data, ref offset ) ;

					//------------

					// メンバー情報内の自身の準備ＯＫを更新する
					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == m_Owner.UserId ) ;
					if( groupMember != null )
					{
						groupMember.IsReady = isReady ;

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberUpdated,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
					}
					else
					{
						Debug.LogWarning( "グループメンバーリストに指定のユーザー識別子のものが見つからない UserId = " + m_Owner.UserId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループから離脱する　※メンバー行動限定
			private bool ProcessGroupingActionResponse_LeaveFromGroup( byte[] _ )
			{
				try
				{
					if( m_GroupMembers != null && m_GroupMembers.Count >  0 )
					{
						m_GroupMembers.Clear() ;

						// マッチング中であればサーバーで取消している
						m_IsMatchingRunning		= false ;

						//-------------------------------

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.Deleted,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

						//-------------------------------

						m_GroupType		= GroupTypes.None ;
						m_GroupId		= 0 ;
						m_IsGroupLeader = false ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループメンバーを排除する　※リーダー側
			private bool ProcessGroupingActionResponse_RejectGroupMember( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 排除したユーザーのユーザー識別子が返る
					string userId		= DataFormat.GetString( data, ref offset ) ;

					//-----------------------------------------

					// マッチング中であればサーバーで取消している
//					m_IsMatchingRunning		= false ;

					//-----------------------------------------

					// 関連性のあるユーザーリストで招待を送信したチェックを外す
					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						// オンライン状態を更新する
						relatedUser.GroupType = GroupTypes.None ;

						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}
					else
					{
						Debug.LogWarning( "関連性のあるユーザー情報群に指定のユーザー識別子のものが見つからない UserId = " + userId + "\n<color=#FFFF00>フレンドではないユーザーがグループから離脱したので、デバッグ機能を使用してグループに加わっていた可能性あり</color>" ) ;
					}

					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupMember != null )
					{
						m_GroupMembers.Remove( groupMember ) ;

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberLeft,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						m_OnGroupMemberUpdated?.Invoke( -1, groupMember, m_GroupMembers ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループを作成する　※フリーユーザー限定行動
			private bool ProcessGroupingActionResponse_CreateGroup( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループ種別
					m_GroupType			= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					// グループ識別子
					m_GroupId			= DataFormat.GetUInt( data, ref offset ) ;

					// 自身がグループリーダー
					m_IsGroupLeader		= true ;

					// グループ固有パラメータ
					m_GroupParameters	= GetUserParameters( data, ref offset ) ;

					// グループメンバー群を展開する
					m_GroupMembers	= GetGroupMembers( data, ref offset ) ;

					//--------------------------------

					// 自身が出していた招待を全て取消状態にする
					int count = 0 ;
					foreach( var relatedUser in m_RelatedUsers )
					{
						if( relatedUser.IsInviting == true )
						{
							relatedUser.IsInviting = false ;
							count ++ ;
						}
					}

					if( count >  0 )
					{
						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
					}

					//------------

					// 自身が受けていた招待を全て拒否状態にする
					if( m_GroupInvitations.Count >  0 )
					{
						m_GroupInvitations.Clear() ;

						// コールバックを呼ぶ
						m_OnGroupInvitationUpdated?.Invoke( 0, null, m_GroupInvitations ) ;
					}

					//------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Created,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					//------------

					// グループメンバー
					m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループを確定してマッチングを開始する　※リーダー限定行動
			private bool ProcessGroupingActionResponse_StartSoloMatching( byte[] _ )
			{
				try
				{
					// マッチング中
					m_IsMatchingRunning		= true ;

					// マッチング取消可能
					m_IsMatchingStoppable	= true ;

					// マッチングが開始された(ソロ申請の場合取消が可能)
					m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Started ) ;

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループを確定してマッチングを開始する　※リーダー限定行動
			private bool ProcessGroupingActionResponse_StartGroupMatching( byte[] _ )
			{
				try
				{
					// マッチング中
					m_IsMatchingRunning		= true ;

					// マッチング取消可能
					m_IsMatchingStoppable	= true ;

					// コールバックを呼ぶ
					m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Started ) ;

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// マッチングを取消する　※リーダー限定行動
			private bool ProcessGroupingActionResponse_StopMatching( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// マッチング取消を要求したユーザーのユーザー識別子(基本的には自分自身)
					string userId = DataFormat.GetString( data, ref offset ) ;

					// 実際に停止したかどうか(このタイミングで取消できる事はまずない)
					bool isCanceled = DataFormat.GetBool( data, ref offset ) ;

					Debug.Log( "実際にマッチングを取消できたか : " + isCanceled ) ;

					if( isCanceled == true )
					{
						//--------------------------------

						if( m_GroupType == GroupTypes.Small && m_GroupId != 0 )
						{
							// メンバー全員を準備完了でない状態にする
	//						foreach( var member in m_GroupMembers )
	//						{
	//							member.IsReady = false ;
	//						}

							// マッチング取消要求を出したユーザーを準備完了でない状態にする
							var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
							if( groupMember != null )
							{
								// 準備完了状態ではない
								groupMember.IsReady = false ;

								m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
							}

							//----------

							m_GroupType = GroupTypes.None ;
							m_GroupId	= 0 ;
						}

						//------------

						// 停止した
						m_IsMatchingRunning		= false ;

						// マッチング取消可能
						m_IsMatchingStoppable	= false ;

						// コールバックを呼ぶ
						m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Canceled ) ;
					}

					//--------------------------------
					// StopMatching と MatchingStopped では意味が異なる
					//
					// StopMatching で isStopped が false の場合は、まだ停止できる余地がある事を意味している(この後 MatchingStopped が呼ばれる)
					// MatchingStopped で isStopped が false の場合は、リクエストが SessionServer で処理されてしまった後でで、もはや取り消す事は出来ない事を意味する


					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}


			//----------------------------------
			// プッシュ通知系

			// グループへの招待を受け取った　※招待の受け取り側
			private bool ProcessGroupingActionResponse_GroupInvitationReceived( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// オーナーのユーザー識別子
					string userId		= DataFormat.GetString( data, ref offset ) ;

					// オーナーのユーザー名
					string userName		= DataFormat.GetString( data, ref offset ) ;

					// オーナーの固有パラメータ
					var parameters		= GetUserParameters( data, ref offset ) ;

					// アプリケーション識別子
					var applicationId	= DataFormat.GetString( data, ref offset ) ;

					// 招待したグループタイプ
					var groupType		= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					//------------

					var groupInvitation = new GroupInvitationData()
					{
						// オーナーの情報
						UserId			= userId,
						UserName		= userName,
						Parameters		= parameters,

						// アプリケーション識別子
						ApplicationId	= applicationId,

						// グループタイプ
						GroupType		= groupType,
					} ;

					m_GroupInvitations.Add( groupInvitation ) ;

					// 変更があったコールバックを呼ぶ
					m_OnGroupInvitationUpdated?.Invoke( +1, groupInvitation, m_GroupInvitations ) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待が取り消された　※招待の受け取り側
			private bool ProcessGroupingActionResponse_GroupInvitationCanceled( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// オーナーのユーザー識別子が送られてくる
					string	userId		= DataFormat.GetString( data, ref offset ) ;

					// オーナーがオンラインかどうか
					bool	isOnline	= DataFormat.GetBool( data, ref offset ) ;

					//--------------------------------

					var groupInvitation = m_GroupInvitations.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupInvitation != null )
					{
						// 招待を削除する
						m_GroupInvitations.Remove( groupInvitation ) ;

						// 変更があったコールバックを呼ぶ
						m_OnGroupInvitationUpdated?.Invoke( -1, groupInvitation, m_GroupInvitations ) ;
					}
					else
					{
						Debug.LogWarning( "招待リストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待が承諾された　※オーナー側
			private bool ProcessGroupingActionResponse_GroupInvitationAccepted( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を送ったユーザーのユーザー識別子
					string	userId		= DataFormat.GetString( data, ref offset ) ;

					// 招待を送ったユーザーのグループメンバー情報を展開する
					var groupMember		= GetGroupMember( data, ref offset ) ;

					//--------------------------------

					if( groupMember != null )
					{
						// グループメンバーを追加
						m_GroupMembers.Add( groupMember ) ;

						//--------------------------------

						var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == groupMember.UserId ) ;
						if( relatedUser != null )
						{
							// 関係性のあるユーザーリストでも自身と同じグループ参加状態にしておく
							relatedUser.GroupType = m_GroupType ;
							relatedUser.IsInviting = false ;

							// 変更があったコールバックを呼ぶ
							m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;

							// グループ招待の応答コールバックを呼ぶ
							m_OnGroupInvitationResponded?.Invoke
							(
								GroupInvitationResponseTypes.Accepted,
								m_GroupType,
								userId,
								relatedUser.UserName,
								true
							) ;
						}
						else
						{
							Debug.LogWarning( "フレンドリストに指定のユーザー識別子のものが見つからない UserId = " + userId + "\n<color=#FFFF00>招待を送っていないユーザーがグループに加わったため、デバッグ機能を使用してグループに加わった可能性あり</color>" ) ;
						}

						//------------

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( +1, groupMember, m_GroupMembers ) ;
					}

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループへの招待が拒否された　※オーナー側
			private bool ProcessGroupingActionResponse_GroupInvitationRejected( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 招待を送ったユーザーのユーザー識別子
					string	userId			= DataFormat.GetString( data, ref offset ) ;

					// 招待を送ったユーザーがオンラインかどうか
					bool	isOnline		= DataFormat.GetBool( data, ref offset ) ; 

					// アプリケーション識別子
					string	applicationId	= DataFormat.GetString( data, ref offset ) ;

					// グループタイプ
					var		groupType		= ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					//--------------------------------

					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						relatedUser.IsInviting		= false ;

						relatedUser.IsOnline		= isOnline ;

						relatedUser.ApplicationId	= applicationId ;

						relatedUser.GroupType		= groupType ;

						// 変更があったコールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;

						// グループ招待の応答コールバックを呼ぶ
						m_OnGroupInvitationResponded?.Invoke
						(
							GroupInvitationResponseTypes.Rejected,
							m_GroupType != GroupTypes.Large ? GroupTypes.Small : GroupTypes.Large,
							userId,
							relatedUser.UserName,
							isOnline
						) ;
					}
					else
					{
						Debug.LogWarning( "フレンドリストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// Smallグループが生成された　※オーナー限定
			private bool ProcessGroupingActionResponse_SmallGroupCreated( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					// 招待を承諾したユーザーのユーザー識別子
					string userId	    = DataFormat.GetString( data, ref offset ) ;

					// グループタイプ
					m_GroupType		    = ( GroupTypes )DataFormat.GetByte( data, ref offset ) ;

					// グループ識別子
					m_GroupId		    = DataFormat.GetUInt( data,ref offset ) ;

					// 自身がグループのリーダー
					m_IsGroupLeader     = true ;

					// グループ固有パラメータ
					m_GroupParameters	= GetUserParameters( data, ref offset ) ;

					// グループメンバー情報群
					m_GroupMembers      = GetGroupMembers( data, ref offset ) ;

					//-------------------------------

					// 関係性のあるユーザーの情報を更新する
					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						// 招待をクリアする

						relatedUser.GroupType	= m_GroupType ;
						relatedUser.IsInviting	= false ;

						// コールバックを呼ぶ
						m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;

						// グループ招待の応答コールバックを呼ぶ
						m_OnGroupInvitationResponded?.Invoke
						(
							GroupInvitationResponseTypes.Accepted,
							m_GroupType,
							userId,
							relatedUser.UserName,
							true
						) ;
					}

					//------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Created,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					//------------

					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupMember != null )
					{
						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( +1, groupMember, m_GroupMembers ) ;
					}
					else
					{
						// 異常
						Debug.LogWarning( "異常" ) ;
					}

					//--------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループにメンバーが参加した　※既に自身はグループに参加しているユーザー限定(リーダーを除く)
			private bool ProcessGroupingActionResponse_GroupMemberJoined( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					var groupMember = GetGroupMember( data, ref offset ) ;
					if( groupMember != null )
					{
						m_GroupMembers.Add( groupMember ) ;

						//--------------------------------

						var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == groupMember.UserId ) ;
						if( relatedUser != null )
						{
							// 関係性のあるユーザーリストでも自身と同じグループ参加状態にしておく
							relatedUser.GroupType = m_GroupType ;
							relatedUser.IsInviting = false ;

							m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
						}
						else
						{
//							Debug.LogWarning( "フレンドリストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
						}

						//--------------------------------

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberJoined,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( +1, groupMember, m_GroupMembers ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループ固有パラメータが更新された
			private bool ProcessGroupingActionResponse_GroupParameterUpdated( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					// 対象ユーザーの固有パラメータ
					var parameters = GetUserParameters( data, ref offset ) ;

					//------------

					m_GroupParameters = parameters ;

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.ParameterUpdated,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループメンバーの固有パラメータが更新された
			private bool ProcessGroupingActionResponse_GroupMemberParameterUpdated( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					// 対象ユーザーのユーザー識別子
					var userId	= DataFormat.GetString( data, ref offset ) ;

					// 対象ユーザーの固有パラメータ
					var parameters = GetUserParameters( data, ref offset ) ;

					//------------

					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupMember != null )
					{
						// 固有パラメータを更新する
						groupMember.Parameters = parameters ;

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberUpdated,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
					}
					else
					{
						Debug.LogWarning( "グループメンバーリストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループメンバーの準備完了が更新された
			private bool ProcessGroupingActionResponse_GroupMemberReadyUpdated( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					// 対象ユーザーのユーザー識別子
					var userId	= DataFormat.GetString( data, ref offset ) ;

					// 準備ＯＫ状態
					var isReady = DataFormat.GetBool( data, ref offset ) ;

					//------------

					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupMember != null )
					{
						// 準備ＯＫの状態を設定する
						groupMember.IsReady = isReady ;

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberUpdated,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
					}
					else
					{
						Debug.LogWarning( "グループメンバーリストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループメンバーが離脱した
			private bool ProcessGroupingActionResponse_GroupMemberLeft( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// グループに加わっているメンバーを取得する
					// 表示は行わないが自分自身もメンバー情報して保持している

					var userId	= DataFormat.GetString( data, ref offset ) ;

					//------------

					var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( groupMember != null )
					{
						var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == groupMember.UserId ) ;
						if( relatedUser != null )
						{
							// 関係性のあるユーザーリストでもグループ未参加状態に戻しておく
							relatedUser.GroupType = GroupTypes.None ;

							// コールバックを呼ぶ
							m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
						}

						//-------------------------------------------------------

						m_GroupMembers.Remove( groupMember ) ;

						// 自身はまだグループに残っているのでグループが解散状態という事はありえない

						// グループ更新
						m_OnGroupUpdated?.Invoke
						(
							GroupStatus.MemberLeft,
							m_GroupType,
							m_GroupId,
							m_IsGroupLeader,
							m_GroupParameters,
							m_GroupMembers
						) ;

						// コールバックを呼ぶ
						m_OnGroupMemberUpdated?.Invoke( -1, groupMember, m_GroupMembers ) ;
					}
					else
					{
						Debug.LogWarning( "グループメンバーリストに指定のユーザー識別子のものが見つからない UserId = " + userId ) ;
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// グループメンバー(自分)が排除させられた
			private bool ProcessGroupingActionResponse_GroupMemberRejected( byte[] _ )
			{
				try
				{
					// 自分自身は関係者リストには表示されないので m_RelatedUsers の更新は不要

					//--------------------------------

					m_GroupMembers.Clear() ;

					// コールバックを呼ぶ
					m_OnGroupMemberUpdated?.Invoke(  0, null, m_GroupMembers ) ;

					//--------------------------------------------------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Rejected,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					// コールバッグでメンバー除外前の情報を知りたいので初期化はコールバックを呼んだ後に行う
					m_GroupType	= GroupTypes.None ;
					m_GroupId	= 0 ;
					m_IsGroupLeader = false ;

					//--------------------------------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}


			// 参加中のグループが解散した　※メンバー限定受信
			private bool ProcessGroupingActionResponse_GroupDeleted( byte[] _ )
			{
				try
				{
					foreach( var groupMember in m_GroupMembers )
					{
						if( groupMember.UserId != m_Owner.UserId )
						{
							// 自分自身ではない

							var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == groupMember.UserId ) ;
							if( relatedUser != null )
							{
								// 関係性のあるユーザーリストでもグループ未参加状態に戻しておく
								relatedUser.GroupType = GroupTypes.None ;

								// コールバックを呼ぶ
								m_OnRelatedUserUpdated?.Invoke( m_RelatedUsers ) ;
							}
						}
					}

					//--------------------------------

					m_GroupMembers.Clear() ;

					// マッチング中であればサーバーで取消している
					m_IsMatchingRunning		= false ;

					// コールバックを呼ぶ
					m_OnGroupMemberUpdated?.Invoke(  0, null, m_GroupMembers ) ;

					//--------------------------------------------------------

					// グループ更新
					m_OnGroupUpdated?.Invoke
					(
						GroupStatus.Deleted,
						m_GroupType,
						m_GroupId,
						m_IsGroupLeader,
						m_GroupParameters,
						m_GroupMembers
					) ;

					// コールバッグでグループ削除前の情報を知りたいので初期化はコールバックを呼んだ後に行う
					m_GroupType	    = GroupTypes.None ;
					m_GroupId	    = 0 ;
					m_IsGroupLeader = false ;

					//--------------------------------------------------------

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			//--------------

			// マッチングが開始された
			private bool ProcessGroupingActionResponse_MatchingStarted( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// 取消を行う事が可能かどうか
					bool isStoppable = DataFormat.GetBool( data, ref offset ) ;

					//--------------------------------

					// マッチング開始完了
					m_IsMatchingRunning		= true ;

					// マッチング取消可能
					m_IsMatchingStoppable	= isStoppable ;

					// コールバックを呼ぶ
					m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Started ) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}

			// マッチングがキャンセルされた(Smallグループの場合は全員準備完了状態を解除する)
			private bool ProcessGroupingActionResponse_MatchingCanceled( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// マッチング取消を要求したユーザーのユーザー識別子
					string userId			= DataFormat.GetString( data, ref offset ) ;

					// 取消トリガー
					var reason				= ( MatchingCancellationResons )DataFormat.GetByte( data, ref offset ) ;

					// 失敗詳細
					string failureDetails	= DataFormat.GetString( data, ref offset ) ;

					// 実際に取消が行われたかどうか
					bool isCanceled			= DataFormat.GetBool( data, ref offset ) ;

					// ソロでのマッチング申請の場合は userId は null になっている

					Debug.Log( "<color=#FF7F00>------->マッチング取消リクエストの結果 isCanceled = " + isCanceled + "</color>\n" + reason + " " + failureDetails ) ;

					//--------------------------------

					if( string.IsNullOrEmpty( userId ) == false && m_GroupType == GroupTypes.Small )
					{
						// あくまでグループの場合のみ処理する

						// メンバー全員を準備完了でない状態にする
//						foreach( var groupMember in m_GroupMembers )
//						{
//							groupMember.IsReady = false ;
//						}

						// マッチング取消要求を出したユーザーを準備完了でない状態にする
						var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
						if( groupMember != null )
						{
							// 準備完了状態ではない
							groupMember.IsReady = false ;

							m_OnGroupMemberUpdated?.Invoke( 0, groupMember, m_GroupMembers ) ;
						}
					}

					//--------------------------------

					if( reason == MatchingCancellationResons.Requested )
					{
						// 取消
						if( isCanceled == true )
						{
							// マッチング停止完了
							m_IsMatchingRunning		= false ;

							// マッチング取消不可
							m_IsMatchingStoppable	= false ;

							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Canceled ) ;
						}
						else
						{
							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.NoCancellation ) ;
						}
					}
					else
					if( reason == MatchingCancellationResons.Dicconnected )
					{
						// 切断
						if( isCanceled == true )
						{
							// マッチング停止完了
							m_IsMatchingRunning		= false ;

							// マッチング取消不可
							m_IsMatchingStoppable	= false ;

							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Disconnected ) ;
						}
						else
						{
							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.NoCancellation ) ;
						}
					}
					else
					if( reason == MatchingCancellationResons.Failed )
					{
						// 失敗
						if( isCanceled == true )
						{
							// マッチング停止完了
							m_IsMatchingRunning		= false ;

							// マッチング取消不可
							m_IsMatchingStoppable	= false ;

							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Failed ) ;
						}
						else
						{
							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.NoCancellation ) ;
						}
					}
					else
					if( reason == MatchingCancellationResons.Timeout )
					{
						// 失敗
						if( isCanceled == true )
						{
							// マッチング停止完了
							m_IsMatchingRunning		= false ;

							// マッチング取消不可
							m_IsMatchingStoppable	= false ;

							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Timeout ) ;
						}
						else
						{
							// コールバックを呼ぶ
							m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.NoCancellation ) ;
						}
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}
#if false
			// マッチングが完了した(セッションに加わった状態になって呼び出される)　※Pending
			private bool ProcessGroupingActionResponse_MatchingSuccessful( byte[] _ )
			{
				try
				{
					// 全員を準備完了でない状態にする

					foreach( var groupMember in m_GroupMembers )
					{
						groupMember.IsReady = false ;
					}

					m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}
#endif

			// マッチングが完了した(セッションに加わった状態になって呼び出される)
			private bool ProcessGroupingActionResponse_MatchingCompleted( byte[] data )
			{
				try
				{
					int i, l ;

					//--------------------------------
					// 必要な情報を展開する

					int offset = 0 ;

					//------------
					// Session

					string	applicationId			= DataFormat.GetString( data, ref offset ) ;

					uint	sessionId				= DataFormat.GetUInt( data, ref offset ) ;
					string	description				= DataFormat.GetString( data, ref offset ) ;
					ushort	maxPlayers				= DataFormat.GetUShort( data, ref offset ) ;
					var		scopeType				= ( SessionScopeTypes )DataFormat.GetByte( data, ref offset ) ;
					var		managementType			= ( SessionManagementTypes )DataFormat.GetByte( data, ref offset ) ;
					bool	udpEnabled				= DataFormat.GetBool( data, ref offset ) ;
					bool	udpCorrectionEnabled	= DataFormat.GetBool( data, ref offset ) ;

					var parameters = new Dictionary<string,string>() ;
					l = DataFormat.GetByte( data, ref offset ) ;
					if( l >  0 )
					{
						for( i  = 0 ; i <  l ; i ++ )
						{
							string key   = DataFormat.GetString( data, ref offset ) ;
							string value = DataFormat.GetString( data, ref offset ) ;
							if( parameters.ContainsKey( key ) == false )
							{
								parameters.Add( key, value ) ;
							}
						}
					}

					bool	processorEnabled		= DataFormat.GetBool( data, ref offset ) ;

					//------------
					// SessionPlayers

					var sessionPlayers = new List<SessionPlayer>() ;

					l = DataFormat.GetVUShort( data, ref offset ) ;
					if( l >  0 )
					{
						string	userId ;
						string	userName ;
						bool	isGuest ;
						bool	isHost ;

						Dictionary<string, string>	sessionPlayerParameters ;
						int		pi, pl ;
						string	key, value ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							userId		= DataFormat.GetString( data, ref offset ) ;
							userName	= DataFormat.GetString( data, ref offset ) ;
							isGuest		= DataFormat.GetBool( data, ref offset ) ;
							isHost		= DataFormat.GetBool( data, ref offset ) ;

							sessionPlayerParameters	= new () ;

							pl = DataFormat.GetByte( data, ref offset ) ;
							if( pl >  0 )
							{
								for( pi  = 0 ; pi <  pl ; pi ++ )
								{
									key		= DataFormat.GetString( data, ref offset ) ;
									value	= DataFormat.GetString( data, ref offset ) ;

									sessionPlayerParameters.Add( key, value ) ;
								}
							}

							sessionPlayers.Add( new
							(
								userId,
								userName,
								isGuest,
								isHost,
								sessionPlayerParameters
							) ) ;
						}
					}

					//------------
					// ExchangeServer EndPoint

					string exchangeServer_Address = DataFormat.GetString( data, ref offset ) ;
					ushort exchangeServer_TcpPort = DataFormat.GetUShort( data, ref offset ) ;
					ushort exchangeServer_UdpPort = DataFormat.GetUShort( data, ref offset ) ;

					//--------------------------------------------------------

					// マッチング停止完了
					m_IsMatchingRunning		= false ;

					// マッチング取消不可
					m_IsMatchingStoppable	= false ;

					//--------------------------------------------------------
					// 全員を準備完了でない状態にする

					foreach( var groupMember in m_GroupMembers )
					{
						groupMember.IsReady = false ;
					}

					// コールバックを呼ぶ
					m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;

					//----------------------------------------------------------------------------------------
					// ExchangeServer への接続

					// ExchangeServer とのネゴシエーションを実行してもらい
					// 問題が無ければ改めてマッチング成功のコールバックを呼ぶ

					_ = ConnectToExchangeServer
					(
						//-----------
						// Session

						sessionId,

						maxPlayers,
						scopeType,
						managementType,
						udpEnabled,
						udpCorrectionEnabled,
						parameters,
						processorEnabled,

						sessionPlayers,

						exchangeServer_Address,
						exchangeServer_TcpPort,
						exchangeServer_UdpPort
					) ;

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}
#if false
			// マッチングが失敗した(セッションに加わった状態になって呼び出される)
			private bool ProcessGroupingActionResponse_MatchingFailed( byte[] data )
			{
				try
				{
					int offset = 0 ;

					// マッチング取消を要求したユーザーのユーザー識別子
					string userId = DataFormat.GetString( data, ref offset ) ;

					//--------------------------------

					// マッチング取消を要求したユーザーを準備完了でない状態にする

					if( string.IsNullOrEmpty( userId ) == false )
					{
						var groupMember = m_GroupMembers.FirstOrDefault( _ => _.UserId == userId ) ;
						if( groupMember != null )
						{
							groupMember.IsReady = false ;
	
							m_OnGroupMemberUpdated?.Invoke( 0, null, m_GroupMembers ) ;
						}
					}

					return true ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "受信データ異常です\n" + e.Message ) ;
					return false ;
				}
			}
#endif

			//------------------------------------------------------------------------------------------
#if MATCHING_SYSTEM_OLD_VERSION
			// マッチングに成功した際に呼び出される(旧版)
			private void OnMatchingToSessionSuccessful
			(
				byte[] commandData,
				ref int offset
			)
			{
				//-----------------------------------------------------------------------------------------

				if( m_Owner.IsMatchingToSessionRequested == false || m_Owner.MatchingId == 0 )
				{
					// クライアント側がマッチングの準備が整っていない
					Debug.LogWarning( "クライアントはマッチングの要求を出していない状態でマッチング結果を受信した" ) ;
					return ;
				}

				//---------------------------------------------------------

				var matchingToSessionResult = new MatchingToSessionResult() ;
				if( matchingToSessionResult.DecodeSuccessful( commandData, ref offset ) == false )
				{
					// デコードに失敗(異常)
					Debug.LogWarning( "異常なデータを受信した" ) ;
					return ;
				}

				//---------------------------------

				// 成功でも失敗でもマッチング要求をキャンセル状態にする
				if( m_Owner.m_ExchangeServerProcessor.CancelMatchingToSession( matchingToSessionResult.MatchingId ) == false )
				{
					Debug.LogWarning( "マッチング識別子に異常があります : " +  matchingToSessionResult.MatchingId ) ;
				}

				//---------------------------------

				// 念の為エラーが発生していないか確認する
				if( matchingToSessionResult.ResponseCode != ResponseCodes.Succeeded )
				{
					CallOnMatchingToSession( matchingToSessionResult ) ;

					return ;
				}

				//-----------------------------------------------------------------------------------------

				// ExchangeServer とのネゴシエーションを実行してもらい
				// 問題が無ければ改めてマッチング成功のコールバックを呼ぶ

				_ = ConnectToExchangeServer( matchingToSessionResult ) ;

			}

			// ExchangeServer へ接続する中継メソッド(旧版)
			private async Task ConnectToExchangeServer( MatchingToSessionResult matchingToSessionResult )
			{
				( var responseCode, var errorMessage ) = await m_Owner.ConnectToExchangeServerAsync
				(
					matchingToSessionResult.SessionId,

					matchingToSessionResult.MaxPlayers,
					matchingToSessionResult.ScopeType,
					matchingToSessionResult.ManagementType,
					matchingToSessionResult.UdpEnabled,
					matchingToSessionResult.UdpCorrectionEnabled,
					matchingToSessionResult.Parameters,
					matchingToSessionResult.ProcessorEnabled,

					matchingToSessionResult.SessionPlayers,

					matchingToSessionResult.ExchangeServer_Address,
					matchingToSessionResult.ExchangeServer_TcpPort,
					matchingToSessionResult.ExchangeServer_UdpPort,

					m_CancellationTokenSource.Token
				) ;

				//---------------------------------
				// ExchangeServer への接続結果をもってマッチング結果のコールバックを呼ぶ

				if( responseCode == ResponseCodes.Succeeded )
				{
					CallOnMatchingToSession( matchingToSessionResult ) ;
				}
				else
				{
					matchingToSessionResult.UpdateStatus( responseCode, errorMessage ) ;
					CallOnMatchingToSession( matchingToSessionResult ) ;
				}
			}
#endif
			// ExchangeServer へ接続する中継メソッド(新版)
			private async Task ConnectToExchangeServer
			(
				uint					    sessionId,

				ushort					    maxPlayers,
				SessionScopeTypes		    scopeType,
				SessionManagementTypes	    managementType,
				bool					    udpEnabled,
				bool					    udpCorrectionEnabled,
				Dictionary<string,string>   parameters,
				bool					    processorEnabled,

				List<SessionPlayer>		    sessionPlayers,

				string					    exchangeServer_Address,
				ushort					    exchangeServer_TcpPort,
				ushort					    exchangeServer_UdpPort
			)
			{
				( var responseCode, var errorMessage ) = await m_Owner.ConnectToExchangeServerAsync
				(
					sessionId,

					maxPlayers,
					scopeType,
					managementType,
					udpEnabled,
					udpCorrectionEnabled,
					parameters,
					processorEnabled,

					sessionPlayers,

					exchangeServer_Address,
					exchangeServer_TcpPort,
					exchangeServer_UdpPort,

					m_CancellationTokenSource.Token
				) ;

				//---------------------------------
				// ExchangeServer への接続結果をもってマッチング結果のコールバックを呼ぶ

				if( responseCode == ResponseCodes.Succeeded )
				{
					// 必ずメインスレッドで実行されるはずなので実行スレッドの確認は不要

					// マッチングが完了した
					m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Completed ) ;
				}
				else
				{
					// マッチングに失敗した
					Debug.LogWarning( "マッチングの失敗理由 : " + responseCode + "\n" + errorMessage ) ;
					m_OnMatchingStatusUpdated?.Invoke( MatchingStatus.Failed ) ;
				}
			}

#if MATCHING_SYSTEM_OLD_VERSION
			// マッチングに失敗した際に呼び出される
			private void OnMatchingToSessionFailed
			(
				byte[] commandData,
				ref int offset
			)
			{
				//-----------------------------------------------------------------------------------------

				if( m_Owner.IsMatchingToSessionRequested == false || m_Owner.MatchingId == 0 )
				{
					// クライアント側がマッチングの準備が整っていない
					Debug.LogWarning( "クライアントはマッチングの要求を出していない状態でマッチング結果を受信した" ) ;
					return ;
				}

				//---------------------------------------------------------

				var matchingToSessionResult = new MatchingToSessionResult() ;
				if( matchingToSessionResult.DecodeFailed( commandData, ref offset ) == false )
				{
					// デコードに失敗(異常)
					Debug.Log( "異常にデータを受信した" ) ;
					return ;
				}

				// マッチング要求をキャンセル状態にする
				m_Owner.m_ExchangeServerProcessor.CancelMatchingToSession( matchingToSessionResult.MatchingId ) ;

				CallOnMatchingToSession( matchingToSessionResult ) ;
			}
#endif
			//----------------------------------

			// 任意のメッセージ(バイト
			private void OnNotificationMessage
			(
				byte[] commandData,
				ref int offset
			)
			{
				// ※packetType は、ホストが転送アップする場合にのみ必要
				//----------------------------------------------------------
				// データ
				byte[] data ;

				try
				{
					data = DataFormat.GetByteArray( commandData, ref offset ) ;
					if( data == null || data.Length == 0 )
					{
						// データ異常
						Debug.LogWarning( "送信データが異常なダウンフレームを受信した" ) ;
						return ;
					}
				}
				catch( Exception )
				{
					// データ異常
					Debug.LogWarning( "送信データに何らかの異常が見られるダウンフレームを受信した" ) ;
					return ;
				}

				//----------------------------------

				// ユーザーのコールバックを呼ぶ(一般的なデータを受信)
				CallOnReceived( data ) ;
			}

			//-------------------------------------------------------------------------------------------

			// コマンドを送信する
			private bool SendCommand( CommandTypes commandType, Action<List<byte>> onDataAdditional )
			{
				if( m_SocketClient == null )
				{
					// 送信不可
					return false ;
				}

				//---------------------------------------------------------

				var command = new List<byte>() ;

				//----------------------------------
				// 平文部分

				// シグネチャ
				command.AddRange( m_Owner.Signature ) ;

				// バージョンコード
				command.Add( ( byte )(   m_Owner.VersionCode         & 0xFF ) ) ; 
				command.Add( ( byte )( ( m_Owner.VersionCode >>  8 ) & 0xFF ) ) ; 
				command.Add( ( byte )( ( m_Owner.VersionCode >> 16 ) & 0xFF ) ) ; 
				command.Add( ( byte )( ( m_Owner.VersionCode >> 24 ) & 0xFF ) ) ; 

				// 通信区分
				command.Add( ( byte )NetworkTargetTypes.Client ) ;

				//----------------------------------

				// アクセストークン
				DataFormat.PutString( command, m_Owner.AccessToken ) ;

				//----------------------------------
				// 暗号部分

				var commandContentData = new List<byte>() ;

				// コマンド種別
				DataFormat.PutByte( commandContentData, ( byte )commandType ) ;

				// データ追加のコールバック
				onDataAdditional?.Invoke( commandContentData ) ;

				// 暗号化
				byte[] encryptedData = m_Owner.Crypter.EncryptXor( commandContentData.ToArray() ) ;

				command.AddRange( encryptedData ) ;

				byte[] commandData = command.ToArray() ;

				//----------------------------------------------------------

				bool result ;

				// ＴＣＰでパケットを送信する
				result = m_SocketClient.SendTcp( commandData ) ;

				// 最後に送信した時間を更新する
				m_LastSendTime = Timer.NowTicks ;

				return result ;
			}

			// セッションへのバインド要求を送信する
			private void SendBindClientToUser( Dictionary<string, string> parameters )
			{
				// TCP
				SendCommand
				(
					CommandTypes.BindClientToUser,
					( List<byte> commandContentData ) =>
					{
						// 固有パラメータを格納する
						if( parameters == null || parameters.Count == 0 )
						{
							DataFormat.PutByte( commandContentData, 0 ) ;
						}
						else
						{
							DataFormat.PutByte( commandContentData, ( byte )parameters.Count ) ;
							foreach( ( var key, var value ) in parameters )
							{
								DataFormat.PutString( commandContentData, key ) ;
								DataFormat.PutString( commandContentData, value ) ;
							}
						}

						// おおよその応答までの時間計測のため現在の時間を格納する
						DataFormat.PutLong( commandContentData, Timer.NowTicks ) ;
					}
				) ;
			}

			// KeepAlive パケットを送る
			// 送るタイミングは、 LastUpdateTime から５秒経過していたら
			private bool SendKeepAlive()
			{
	//			Debug.Log( "<color=#FF7FFF>KeepAlive の送信起点 : Ticks = " + ticks + " PacketType = " + packetType + "</color>" ) ;

				return SendCommand
				(
					CommandTypes.KeepAlive,
					null
				) ;

				//----------------------------------------------------------

	//			Debug.Log( "<color=#7FFF7F>KeepAlive 送信 [ PacketType = " + packetType + " ] </color>" ) ;
			}

			//------------------------------------------------------------------------------------------

			/// 接続コールバックを呼ぶ
			/// </summary>
			public void CallOnConnected()
			{
				// メインスレッドからしか呼ばれない
				m_OnConnected?.Invoke() ;
			}

			// 受信コールバックのヘルパー
			private void CallOnDisconnected()
			{
				if( m_MainThreadContext != null )
				{
					// メインスレッドによる縛り：あり

					if( SynchronizationContext.Current == m_MainThreadContext )
					{
	//					Debug.Log( "<color=#FFFF00>CallOnDisconnected はメインスレッドで呼ばれた</color>" ) ;
						CallOnDisconnected_Inner() ;
					}
					else
					{
	//					Debug.Log( "<color=#FFFF00>CallOnDisconnected はサブスレッドで呼ばれた</color>" ) ;
						m_MainThreadContext.Post( ( _ ) =>
						{
							// メインスレッドのタイミングで受信処理を実行する)
							CallOnDisconnected_Inner() ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッドによる縛り：なし

					CallOnDisconnected_Inner() ;
				}

				void CallOnDisconnected_Inner()
				{
					try
					{
						// 不特定対象向けフレーム(→フレームはここが終点)
						m_OnDisconnected?.Invoke() ;
					}
					catch( Exception )
					{
						throw ;
					}
				}
			}

			// 受信コールバックのヘルパー
			private void CallOnReceived( byte[] data )
			{
				if( m_MainThreadContext != null )
				{
					// メインスレッドによる縛り：あり

					if( SynchronizationContext.Current == m_MainThreadContext )
					{
//						Debug.Log( "<color=#00FFFF>[NetworkPlay] CallOnReceived はメインスレッドで呼ばれた</color>" ) ;
						CallOnReceived_Inner( data ) ;
					}
					else
					{
	//					Debug.Log( "<color=#00FF00>[NetworkPlay] CallOnReceived  はサブスレッドで呼ばれた</color>" ) ;
						m_MainThreadContext.Post( ( _ ) =>
						{
							// メインスレッドのタイミングで受信処理を実行する)
							CallOnReceived_Inner( data ) ;
						}, null ) ;
					}
				}
				else
				{
					// メインスレッドによる縛り：なし

					CallOnReceived_Inner( data ) ;
				}

				void CallOnReceived_Inner( byte[] data )
				{
					try
					{
						// 不特定対象向けフレーム(→フレームはここが終点)
						m_OnReceived?.Invoke( data ) ;
					}
					catch( Exception )
					{
						throw ;
					}
				}
			}


			//------------------------------------------------------------------------------------------
			// 外部からのコマンド実行

			private uint	m_AsyncGroupingActionSequence = 0 ;

			/// <summary>
			/// グルーピングサーバーの固有パラメータを設定する
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<SetGroupingServiceParameter_Response> SetGroupingServiceParameterAsync
			(
				Dictionary<string,string> parameters,
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.SetGroupingServiceParameter ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( ResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( ResponseCodes.Succeeded, string.Empty ) ;
				}
				else
				{
					// 失敗
					return new ( ResponseCodes.Error, errorMessage ) ;
				}
			}

			/// <summary>
			/// 関連ユーザー情報を更新する
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<GetFriendsForGrouping_Response> GetFriendsForGroupingAsync( ushort offset, ushort length, CancellationToken cancellationToken )
			{
				//---------------------------------------------------------

				// 初期化
				m_RelatedUsers.Clear() ;

				// CommunicationServer からフレンド一覧を取得する

				var getFriends_Response = await m_Owner.GetFriendsAsync( offset, length, cancellationToken ) ;
				if( getFriends_Response.ResponseCode != ResponseCodes.Succeeded )
				{
					if( getFriends_Response.ResponseCode == ResponseCodes.AccessTokenFailed )
					{
						// 失敗
						return new ( GroupingActionResponseCodes.AccessTokenFailed, getFriends_Response.ErrorMessage ) ;
					}

					// 失敗
					return new ( GroupingActionResponseCodes.Error, getFriends_Response.ErrorMessage ) ;
				}

				// 最大件数
				int count = getFriends_Response.Count ;

				//-------------

				var userIds = getFriends_Response.Friends.Select( _ => _.UserId ).ToArray() ;
				if( userIds.Length == 0 )
				{
					// 関連性のあるユーザー情報を消去する

					// 成功
					return new ( m_RelatedUsers, count ) ;
				}

				//---------------------------------------------------------
				// フレンド情報を関連ユーザー情報リストに格納する

				foreach( var friend in getFriends_Response.Friends )
				{
					m_RelatedUsers.Add( new RelatedUserData()
					{
						UserId		= friend.UserId,
						UserName	= friend.UserName,

						IsOnline	= false,
						GroupType	= GroupTypes.None,

						IsInviting	= false,
						Parameters	= new (),

						SessionId   = 0
					} ) ;
				}

				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.CheckRelatedUsers ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 検査するユーザー識別子群
						DataFormat.PutVUShort( data, ( ushort )userIds.Length ) ;

						int i, l = userIds.Length ;
						for( i =  0 ; i <  l ; i ++ )
						{
							DataFormat.PutString( data, userIds[ i ] ) ;
						}

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( m_RelatedUsers, count ) ;
				}
				else
				{
					// 失敗
					return new ( responseCode, errorMessage ) ;
				}
			}


			/// <summary>
			/// グループへの招待を送信する(LargeGroup未所属ならSmallGroupの招待を送る)　※リーダー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<AffordGroupInvitation_Response> AffordGroupInvitationAsync
			(
				string						applicationId,		// アプリケーション識別子
				string						userId,				// 招待対象のユーザー識別子
				Dictionary<string, string>	parameters,			// 招待者の固有パラメータ
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.AffordGroupInvitation ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// アプリケーション識別子
						DataFormat.PutString( data, applicationId ) ;

						// 招待を送るユーザー識別子
						DataFormat.PutString( data, userId ) ;

						// 招待者の固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				//---------------------------------------------------------

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					var relatedUser = m_RelatedUsers.FirstOrDefault( _ => _.UserId == userId ) ;
					if( relatedUser != null )
					{
						if( relatedUser.IsOnline == false )
						{
							// 対象はオフラインになっている
							responseCode = GroupingActionResponseCodes.AlreadyTargetOffline ;
						}
						else
						if( relatedUser.GroupType != GroupTypes.None || relatedUser.IsInviting == false )
						{
							// 対象は既に他のグループに参加済みになっている
							responseCode = GroupingActionResponseCodes.AlreadyTargetGroupJoined ;
						}
					}
				}

				//---------------------------------------------------------

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループへの招待を取消する　※リーダー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<CancelGroupInvitation_Response> CancelGroupInvitationAsync
			(
				string userId,
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.CancelGroupInvitation ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 検査するユーザー識別子群
						DataFormat.PutString( data, userId ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var rorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, rorMessage ) ;
			}

			/// <summary>
			/// グループへの招待を了承する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<AcceptGroupInvitation_Response> AcceptGroupInvitationAsync
			(
				string						userId,
				Dictionary<string,string>	parameters,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.AcceptGroupInvitation ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 招待を送信したユーザー識別子
						DataFormat.PutString( data, userId ) ;

						// 初期のグループメンバーの固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( m_GroupType, m_GroupId, DuplicateGroupMembers() ) ;
				}
				else
				{
					// 失敗
					return new ( responseCode, errorMessage ) ;
				}
			}

			/// <summary>
			/// グループへの招待を拒否する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<RejectGroupInvitation_Response> RejectGroupInvitationAsync
			(
				string				userId,
				CancellationToken	cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.RejectGroupInvitation ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 拒否対象とする招待を送ったオーナーのユーザー識別子
						DataFormat.PutString( data, userId ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループ情報群を取得する
			/// </summary>
			/// <param name="filter"></param>
			/// <param name="offset"></param>
			/// <param name="length"></param>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			public async Task<GetGroups_Response> GetGroupsAsync
			(
				string                      applicationId,
				byte                        filter,
				ushort                      offset,
				ushort                      length,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.GetGroups ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// アプリケーション識別子
						DataFormat.PutString( data, applicationId ) ;

						// フィルター(0・3=すべて・1=Small・2=Large)
						DataFormat .PutByte( data, filter ) ;

						// オフセット
						DataFormat.PutVUShort( data, offset ) ;

						// レングス
						DataFormat.PutVUShort( data, length ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( m_CountOfGroups, m_OffsetOfGroups, m_CachedGroups ) ;
				}
				else
				{
					// 失敗
					return new ( responseCode, errorMessage ) ;
				}
			}

			/// <summary>
			/// グループへ参加する　※フリーユーザー限定行動
			/// </summary>
			/// <param name="userId"></param>
			/// <param name="groupId"></param>
			/// <param name="password"></param>
			/// <param name="parameters"></param>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			public async Task<JoinToGroup_Response> JoinToGroupAsync
			(
				string						userId,
				uint						groupId,
				string						password,
				Dictionary<string,string>	parameters,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.JoinToGroup ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// (招待されていた場合)オーナーのユーザー識別子
						DataFormat .PutString( data, userId ) ;

						// グループの識別子
						DataFormat.PutUInt( data, groupId ) ;

						// パスワード(オプション)
						DataFormat.PutString( data, password ) ;

						// グループメンバーの固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( m_GroupType, m_GroupId, DuplicateGroupMembers() ) ;
				}
				else
				{
					// 失敗
					return new ( responseCode, errorMessage ) ;
				}
			}

			/// <summary>
			/// グループ固有パラメータを設定する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<SetGroupParameter_Response> SetGroupParameterAsync
			(
				Dictionary<string,string> parameters,
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.SetGroupParameter ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 初期のグループメンバーの固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループのメンバー固有パラメータを設定する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<SetGroupMemberParameter_Response> SetGroupMemberParameterAsync
			(
				Dictionary<string,string> parameters,
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.SetGroupMemberParameter ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 初期のグループメンバーの固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループのメンバー準備可能を設定する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<SetGroupMemberReady_Response> SetGroupMemberReadyAsync
			(
				bool                        isReady,
				Dictionary<string, string>  parameters,
				CancellationToken           cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.SetGroupMemberReady ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 準備完了したかどうか
						DataFormat.PutBool( data, isReady ) ;

						// 固有パラメータ
						PutUserParameters( data, parameters ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループから離脱(グループを解散)する　※メンバー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<LeaveFromGroup_Response> LeaveFromGroupAsync
			(
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.LeaveFromGroup ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						DataFormat.PutByteArray( commandContentData, Array.Empty<byte>() ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// メンバーをグループから排除する　※リーダー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<RejectGroupMember_Response> RejectGroupMemberAsync
			(
				string				userId,
				CancellationToken	cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.RejectGroupMember ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						// 排除するユーザー識別子群
						DataFormat.PutString( data, userId ) ;

						//-----------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var rorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, rorMessage ) ;
			}

			/// <summary>
			/// グループを生成する　※リーダー限定行動
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<CreateGroup_Response> CreateGroupAsync
			(
				string						applicationId,
				GroupTypes					groupType,
				string						password,
				bool                        isAutomaticMatchingStarting,
				Dictionary<string,string>	groupParameters,
				Dictionary<string,string>	groupMemberParameters,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.CreateGroup ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						//-------------------------------

						// アプリケーション識別子
						DataFormat.PutString( data, applicationId ) ;

						// グループタイプ
						DataFormat.PutByte( data, ( byte )groupType ) ;

						// パスワード(オプショナル)
						DataFormat.PutString( data, password ) ;

						// 全員が準備完了で自動的にマッチングを開始するかどうか
						DataFormat.PutBool( data, isAutomaticMatchingStarting ) ;

						//-----------

						// グループのリーダーになるオーナーユーザーの固有情報
						PutUserParameters( data, groupParameters ) ;

						// グループのリーダーになるオーナーユーザーの固有情報
						PutUserParameters( data, groupMemberParameters ) ;

						//-------------------------------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				if( responseCode == GroupingActionResponseCodes.Succeeded )
				{
					// 成功
					return new ( m_GroupType, m_GroupId, m_GroupMembers ) ;
				}
				else
				{
					// 失敗
					return new ( responseCode, errorMessage ) ;
				}
			}

			//----------------------------------

			/// <summary>
			/// ソロのマッチングを開始する
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<StartSoloMatching_Response> StartSoloMatchingAsync
			(
				string						applicationId,
				Dictionary<string,string>	groupParameters,
				Dictionary<string,string>	groupMemberParameters,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.StartSoloMatching ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						//-------------------------------

						// アプリケーション識別子
						DataFormat.PutString( data, applicationId ) ;

						// グループのリーダーになるオーナーユーザーの固有情報
						PutUserParameters( data, groupParameters ) ;

						// グループのリーダーになるオーナーユーザーの固有情報
						PutUserParameters( data, groupMemberParameters ) ;

						//-------------------------------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループのマッチングを開始する
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<StartGroupMatching_Response> StartGroupMatchingAsync
			(
				Dictionary<string,string>	groupParameters,
				CancellationToken			cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.StartGroupMatching ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						var data = new List<byte>() ;

						//-------------------------------

						// グループのリーダーになるオーナーユーザーの固有情報
						PutUserParameters( data, groupParameters ) ;

						//-------------------------------

						DataFormat.PutByteArray( commandContentData, data ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}

			/// <summary>
			/// グループのマッチングを取消する
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public async Task<StopMatching_Response> StopMatchingAsync
			(
				CancellationToken cancellationToken
			)
			{
				//---------------------------------------------------------

				// 非同期グルーピングアクションのシーケンスを取得する
				uint asyncGroupingActionSequence = m_AsyncGroupingActionSequence ;

				// 非同期グルーピングアクションのシーケンスの値を変化させる
				m_AsyncGroupingActionSequence ++ ;

				// 非同期グルーピングアクションを送信する
				if( SendCommand
				(
					CommandTypes.GroupingAction,
					( List<byte> commandContentData ) =>
					{
						// グルーピングアクションの種別
						DataFormat.PutByte( commandContentData, ( byte )GroupingActionTypes.StopMatching ) ;

						// グルーピングアクションのコマンドシーケンス
						DataFormat.PutUInt( commandContentData, asyncGroupingActionSequence ) ;

						//-------------------------------
						// 以下がグルーピングアクション毎に異なるデータ

						DataFormat.PutByteArray( commandContentData, Array.Empty<byte>() ) ;
					}
				) == false )
				{
					return new ( GroupingActionResponseCodes.Error, "グーピングアクションの送信に失敗しました" ) ;
				}

				//---------------------------------------------------------
				// シーケンスに該当するグルーピングアクションを受信するまで待つ

				( var responseCode, var errorMessage ) = await WaitForAsyncGroupingActionResult( asyncGroupingActionSequence, cancellationToken ) ;

				return new ( responseCode, errorMessage ) ;
			}


			//----------------------------------------------------------
			// サブルーチン

			// グループメンバー情報群を取得する
			private List<GroupMemberData> GetGroupMembers( byte[] data, ref int offset )
			{
				try
				{
					var groupMembers = new List<GroupMemberData >() ;

					int i, l = DataFormat.GetByte( data, ref offset ) ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						var groupMember = GetGroupMember( data, ref offset ) ;
						if( groupMember != null )
						{
							groupMembers.Add( groupMember ) ;
						}
					}

					return groupMembers ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "異常発生\n" + e.Message ) ;
					return null ;
				}
			}

			// グループメンバー情報を展開する
			private GroupMemberData GetGroupMember( byte[] data, ref int offset )
			{
				try
				{
					return new ()
					{
						UserId		= DataFormat.GetString( data, ref offset ),
						UserName	= DataFormat.GetString( data, ref offset ),
						Parameters	= GetUserParameters( data, ref offset ),
						IsReady		= DataFormat.GetBool( data, ref offset ),
						IsLeader	= DataFormat.GetBool( data, ref offset )
					} ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "異常発生\n" + e.Message ) ;
					return null ;
				}
			}

			// ユーザーの固有バラメータを展開する
			private Dictionary<string,string> GetUserParameters( byte[] data, ref int offset )
			{
				try
				{
					var parameters = new Dictionary<string,string>() ;

					int i, l = DataFormat.GetByte( data, ref offset ) ;
					if( l >  0 )
					{
						string key, value ;

						for( i = 0 ; i <  l ; i ++ )
						{
							key		= DataFormat.GetString( data, ref offset ) ;
							value	= DataFormat.GetString( data, ref offset ) ;

							if( parameters.ContainsKey( key ) == false )
							{
								parameters.Add( key, value ) ;
							}
						}
					}

					return parameters ;
				}
				catch( Exception e )
				{
					Debug.LogWarning( "異常発生\n" + e.Message ) ;
					return null ;
				}
			}

			// ユーザーの固有パラメータを格納する
			private void PutUserParameters( List<byte> data, Dictionary<string, string> parameters )
			{
				try
				{
					// 初期のグループメンバーの固有パラメータ
					if( parameters == null || parameters.Count == 0 )
					{
						DataFormat.PutByte( data, 0 ) ;
					}
					else
					{
						DataFormat.PutByte( data, ( byte )parameters.Count ) ;

						foreach( ( var key, var value ) in parameters )
						{
							DataFormat.PutString( data, key ) ;
							DataFormat.PutString( data, value ) ;
						}
					}
				}
				catch( Exception e )
				{
					Debug.LogWarning( "異常発生\n" + e.Message ) ;
				}
			}

			// グループメンバー情報を複製する
			private List<GroupMemberData> DuplicateGroupMembers()
			{
				var duplicatedGroupMembers = new List<GroupMemberData>() ;

				foreach( var groupMember in m_GroupMembers )
				{
					duplicatedGroupMembers.Add( groupMember.Clone() ) ;
				}

				return duplicatedGroupMembers ;
			}


			//----------------------------------

			// 指定したシーケンスの非同期グルーピングアクションの結果が返るのを待つ
			private async Task<( GroupingActionResponseCodes, string )> WaitForAsyncGroupingActionResult( uint asyncGroupingActionSequence, CancellationToken cancellationToken )
			{
				AsyncGroupingActionResult result = null ;

				//---------------------------------------------------------
				// 待機

				CancellationTokenSource cancellationTokenSource ;
				bool isCanceled ;

				if( cancellationToken == default )
				{
					cancellationTokenSource = m_CancellationTokenSource ;
				}
				else
				{
					cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( m_CancellationTokenSource.Token, cancellationToken ) ;
				}

				isCanceled = false ;

				while( m_SocketClient != null && m_CancellationTokenSource != null && IsConnected == true )
				{
					if( cancellationTokenSource.IsCancellationRequested == true )
					{
						// キャンセルが実行された
						isCanceled = true ;
						break ;
					}

					// 該当するシーケンスの結果のキューからの取り出しを試みる
					result = m_AsyncGroupingActionResultQueue.FirstOrDefault( _ => _.AsyncGroupingActionSequence == asyncGroupingActionSequence ) ;
					if( result != null )
					{
						// 該当するレスポンスを受け取った
						break ;
					}

					await Task.Yield() ;
				}

				// 新規にキャンセレーショントークンソースを生成してれば破棄する
				if( cancellationToken != default )
				{
					cancellationTokenSource.Dispose() ;
				}

				if( isCanceled == true )
				{
					// キャンセルされていたらキャンセル例外を発行する
					throw new OperationCanceledException() ;
				}

				//---------------------------------------------------------

				if( result == null )
				{
					// 既に切断された可能性がある
					return ( GroupingActionResponseCodes.Error, "サーバーから切断された可能性があります" ) ;
				}

				//---------------------------------------------------------
				// キューから取り出しレスポンス情報を返す

				GroupingActionResponseCodes	responseCode = result.ResponseCode ;
				string						errorMessage = result.ErrorMessage ;

				m_AsyncGroupingActionResultQueue.Remove( result ) ;

				//---------------------------------------------------------

				// 成功
				return ( responseCode, errorMessage ) ;
			}

		}




	}	// DefaultNetworkPlayClientAdapter
}	// namespace

//#endif
