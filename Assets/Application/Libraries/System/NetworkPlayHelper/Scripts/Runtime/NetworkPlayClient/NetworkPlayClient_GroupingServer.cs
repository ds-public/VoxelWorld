#if UNITY_2019_4_OR_NEWER
#define UNITY
#endif

using System ;
using System.Collections.Generic ;

using System.Threading ;
using System.Threading.Tasks ;

using UnityEngine ;


namespace NetworkPlayHelper
{
	/// <summary>
	/// NetworkPlay 機能のクライアント側の管理用クラス
	/// </summary>
	public partial class NetworkPlayClient
	{
		/// <summary>
		/// 関連性のあるユーザー情報群
		/// </summary>
		public List<RelatedUserData>		RelatedUsers
		{
			get
			{
				return m_NetworkPlayClientAdapter.RelatedUsers ;
			}
		}

		/// <summary>
		/// グループ招待情報群
		/// </summary>
		public List<GroupInvitationData>	GroupInvitations
		{
			get
			{
				return m_NetworkPlayClientAdapter.GroupInvitations ;
			}
		}

		/// <summary>
		/// グループメンバー情報群
		/// </summary>
		public List<GroupMemberData>		GroupMembers
		{
			get
			{
				return m_NetworkPlayClientAdapter.GroupMembers ;
			}
		}

		//---------------

		/// <summary>
		/// グループタイプ
		/// </summary>
		public GroupTypes					GroupType
		{
			get
			{
				return m_NetworkPlayClientAdapter.GroupType ;
			}
		}

		/// <summary>
		/// グループ識別子
		/// </summary>
		public uint							GroupId
		{
			get
			{
				return m_NetworkPlayClientAdapter.GroupId ;
			}
		}

		/// <summary>
		/// グループのリーダーであるかどうか
		/// </summary>
		public bool							IsGroupLeader
		{
			get
			{
				return m_NetworkPlayClientAdapter.IsGroupLeader ;
			}
		}


		/// <summary>
		/// グループの準備完了を実行中か
		/// </summary>
		public bool							IsGroupReady
		{
			get
			{
				return m_NetworkPlayClientAdapter.IsGroupReady ;
			}
		}


		//-----------------------------------------------------------

		/// <summary>
		/// フレンド群の情報(グプーピング用)を取得する
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
			return await m_NetworkPlayClientAdapter.GetFriendsForGroupingAsync( offset, length, cancellationToken ) ;
		}

		//-----------------------------------------------------------
		// コールバック管理


		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
		{
			m_NetworkPlayClientAdapter.AddOnRelatedUserUpdated( onRelatedUserUpdated ) ;
		}

		/// <summary>
		/// 関連性のあるユーザーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnRelatedUserUpdated( Action<List<RelatedUserData>> onRelatedUserUpdated )
		{
			m_NetworkPlayClientAdapter.RemoveOnRelatedUserUpdated( onRelatedUserUpdated ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
		{
			m_NetworkPlayClientAdapter.AddOnGroupInvitationResponded( onGroupInvitationResponded ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveGroupInvitationResponded( Action<GroupInvitationResponseTypes,GroupTypes,string,string,bool> onGroupInvitationResponded )
		{
			m_NetworkPlayClientAdapter.RemoveGroupInvitationResponded( onGroupInvitationResponded ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
		{
			m_NetworkPlayClientAdapter.AddOnGroupInvitationUpdated( onGroupInvitationUpdated ) ;
		}

		/// <summary>
		/// グループ招待の更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupInvitationUpdated( Action<int,GroupInvitationData,List<GroupInvitationData>> onGroupInvitationUpdated )
		{
			m_NetworkPlayClientAdapter.RemoveOnGroupInvitationUpdated( onGroupInvitationUpdated ) ;
		}

		/// <summary>
		/// グループパラメータの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
		{
			m_NetworkPlayClientAdapter.AddOnGroupUpdated( onGroupUpdated ) ;
		}

		/// <summary>
		/// グループパラメータの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupUpdated( Action<GroupStatus,GroupTypes,uint,bool,Dictionary<string,string>,List<GroupMemberData>> onGroupUpdated )
		{
			m_NetworkPlayClientAdapter.RemoveOnGroupUpdated( onGroupUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
		{
			m_NetworkPlayClientAdapter.AddOnGroupMemberUpdated( onGroupMemberUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnGroupMemberUpdated( Action<int,GroupMemberData,List<GroupMemberData>> onGroupMemberUpdated )
		{
			m_NetworkPlayClientAdapter.RemoveOnGroupMemberUpdated( onGroupMemberUpdated ) ;
		}

		/// <summary>
		///マッチングが開始された際に呼び出されるコールバックを追加する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void AddOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
		{
			m_NetworkPlayClientAdapter.AddOnMatchingStatusUpdated( onMatchingStatusUpdated ) ;
		}

		/// <summary>
		/// グループメンバーの更新があった際に呼び出されるコールバックを削除する
		/// </summary>
		/// <param name="onRelatedUserUpdated"></param>
		public void RemoveOnMatchingStatusUpdated( Action<MatchingStatus> onMatchingStatusUpdated )
		{
			m_NetworkPlayClientAdapter.RemoveOnMatchingStatusUpdated( onMatchingStatusUpdated ) ;
		}

		/// <summary>
		/// マッチング中かどうか
		/// </summary>
		public bool	IsMatchingRunning
			=> m_NetworkPlayClientAdapter.IsMatchingRunning ;

		/// <summary>
		/// マッチングの取消が可能かどうか
		/// </summary>
		public bool IsMatchingStoppable
			=> m_NetworkPlayClientAdapter.IsMatchingStoppable ;

		//-----------------------------------------------------------

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
			return await m_NetworkPlayClientAdapter.AffordGroupInvitationAsync( applicationId, userId, parameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.CancelGroupInvitationAsync( userId, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.AcceptGroupInvitationAsync( userId, parameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.RejectGroupInvitationAsync( userId, cancellationToken ) ;
		}

		/// <summary>
		/// グループへ参加する　※フリーユーザー限定行動
		/// </summary>
		/// <param name="userIds"></param>
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
			return await m_NetworkPlayClientAdapter.JoinToGroupAsync( userId, groupId, password, parameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.SetGroupParameterAsync( parameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.SetGroupMemberParameterAsync( parameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.SetGroupMemberReadyAsync( isReady, parameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループから離脱(グループを解散)する　※メンバー限定行動
		/// </summary>
		/// <param name="userIds"></param>
		/// <returns></returns>
		public async Task<LeaveFromGroup_Response> LeaveFromGroupAsync
		(
			CancellationToken	cancellationToken
		)
		{
			return await m_NetworkPlayClientAdapter.LeaveFromGroupAsync( cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.RejectGroupMemberAsync( userId, cancellationToken ) ;
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
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_NetworkPlayClientAdapter.CreateGroupAsync( applicationId, groupType, password, groupParameters, groupMemberParameters, cancellationToken ) ;
		}


		/// <summary>
		/// ソロのマッチングを開始する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<StartSoloMatching_Response> StartSoloMatchingAsync
		(
			string						applicationId,
			Dictionary<string,string>	groupParameters,
			Dictionary<string,string>	groupMemberParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_NetworkPlayClientAdapter.StartSoloMatchingAsync( applicationId, groupParameters, groupMemberParameters, cancellationToken ) ;
		}

		/// <summary>
		/// グループのマッチングを開始する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<StartGroupMatching_Response> StartGroupMatchingAsync
		(
			Dictionary<string,string>	groupParameters,
			CancellationToken			cancellationToken
		)
		{
			return await m_NetworkPlayClientAdapter.StartGroupMatchingAsync( groupParameters, cancellationToken ) ;
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
			return await m_NetworkPlayClientAdapter.StopMatchingAsync( cancellationToken ) ;
		}

	}
}
