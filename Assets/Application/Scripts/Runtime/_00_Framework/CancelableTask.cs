using System ;
using System.Collections.Generic ;
using System.Threading ;
using System.Threading.Tasks ;

using Cysharp.Threading.Tasks ;

using UnityEngine ;

using EaseHelper ;

namespace DSW
{
	/// <summary>
	/// キャンセル可能なタスク Version 2023/02/03
	/// </summary>
	public class CancelableTask 
	{
		/// <summary>
		/// 名称
		/// </summary>
		public		string						name ;

		// オーナー
		protected	ExMonoBehaviour				m_OwnerForCancellationToken ;

		//-----------------------------------

		// CancelableTask のタスク群を明示的に中断するためのトークンソース
		private CancellationTokenSource			m_CancellationTokenSourceDecative ;

		/// <summary>
		/// CancelableTask のタスク群を明示的に中断するためのトークンを取得する
		/// </summary>
		/// <returns></returns>
		protected CancellationToken				GetCancellationTokenDeactive()
		{
			if( m_CancellationTokenSourceDecative == null )
			{
				m_CancellationTokenSourceDecative  = new CancellationTokenSource() ;
				m_CancellationTokenSourceDecative.Token.Register( () =>
				{
					// 明示的な中断が行われた際にコールバックを呼び出す
					OnTasksCanceled() ;
				} ) ;
			}

			return m_CancellationTokenSourceDecative.Token ;
		}

		// オーナーの ExMonoBehaviour で明示的なタスクキャンセルが行われた際に使用されるキャンセレーショントークン
		private CancellationToken				m_OwnerCancellationTokenDeactive ;

		/// <summary>
		/// オーナーの ExMonoBehaviour で明示的なタスクキャンセルが行われた際に使用されるキャンセレーショントークンを取得する
		/// </summary>
		/// <returns></returns>
		protected	CancellationToken			GetOwnerCancellationTokenDeactive()
		{
			if( m_OwnerForCancellationToken == null )
			{
				Debug.LogWarning( "[CancelableTask] Owner(ExMonoBehaviour) is not set : name = " + name ) ;
				return default ;
			}

			var ownerCancellationTokenDeactive = m_OwnerForCancellationToken.GetCancellationTokenDeactive() ;
			if( ownerCancellationTokenDeactive != m_OwnerCancellationTokenDeactive )
			{
				// 異なるインスタンスの場合は上書きする
				m_OwnerCancellationTokenDeactive = ownerCancellationTokenDeactive ;

				m_OwnerCancellationTokenDeactive.Register( () =>
				{
					// オーナでの明示的な中断が行われた

					// 自身のタスク群中断用のトークンソースを破棄する
					if( m_CancellationTokenSourceDecative != null )
					{
						m_CancellationTokenSourceDecative.Dispose() ;
						m_CancellationTokenSourceDecative = null ;
					}

					// タスク中断コールバックを呼び出す
					OnTasksCanceled() ;
				} ) ;
			}

			return m_OwnerCancellationTokenDeactive ;
		}

		// オーナーの ExMonoBehaviour 破棄時のタスクキャンセル用トークン
		protected	CancellationToken			m_OwnerCancellationTokenOnDestroy	= default ;

		//-----------------------------------
		// 外部アクセス用

		/// <summary>
		/// オーナーの ExMonoBehaviour の破棄によるタスクキャンセルが行われた際に使用されるキャンセレーショントークンを取得する
		/// </summary>
		/// <returns></returns>
		public		CancellationToken			GetCancellationTokenOnDestroy()
		{
			return m_OwnerCancellationTokenOnDestroy ;
		}

		//-----------------------------------------------------------

		// 既に GameObject が削除されてしまったか
		private	bool							m_GameObjectWasDestroyed = false ;

		//-----------------------------------------------------------

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="onAction"></param>
		public CancelableTask( ExMonoBehaviour owner )
		{
			if( owner != null )
			{
				name = "[" + owner.name + "]'s CancelableTask" ;

				m_OwnerForCancellationToken	= owner ;

				m_OwnerCancellationTokenOnDestroy = m_OwnerForCancellationToken.GetCancellationTokenOnDestroy() ;
				m_OwnerCancellationTokenOnDestroy.Register( OnOwnerDestroyed ) ;	// 自身用のトークンソースが残っていたら破棄する
			}
		}

		/// <summary>
		/// オーナーを設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner( ExMonoBehaviour owner )
		{
			if( owner != null )
			{
				name = "[" + owner.name + "]'s CancelableTask" ;

				m_OwnerForCancellationToken	= owner ;

				m_OwnerCancellationTokenOnDestroy = m_OwnerForCancellationToken.GetCancellationTokenOnDestroy() ;
				m_OwnerCancellationTokenOnDestroy.Register( OnOwnerDestroyed ) ;	// 自身用のトークンソースが残っていたら破棄する
			}
		}

		// オーナーが破棄された
		private void OnOwnerDestroyed()
		{
			m_GameObjectWasDestroyed = true ;	// GameObject が削除された状態になっている

			//----------------------------------

			// 自身のタスク群中断用のトークンソースを破棄する
			if( m_CancellationTokenSourceDecative != null )
			{
				m_CancellationTokenSourceDecative.Dispose() ;
				m_CancellationTokenSourceDecative = null ;
			}

			// CancelableTask 用の OnTasksCanceled() を呼び出す
			OnTasksCanceled() ;

			// CancelableTask 用の OnDestroy() を呼び出す
			OnDestroy() ;
		}

		/// <summary>
		/// CancelableTack が破棄される際に呼び出される
		/// </summary>
		virtual protected void OnDestroy()
		{
//			Debug.Log( "オーナー破棄によりタスク群が中断されました:" + name ) ;
		}

		//-----------------------------------------------------------

		// キャンセルトークンを取得する
		private ( CancellationTokenSource, CancellationToken ) GetActiveCancellation( CancellationToken cancellationToken )
		{
			// CacelableTask 自身のタスク群中断用のトークンを取得する。
			CancellationToken cancellationTokenDeactive			= GetCancellationTokenDeactive() ;

			// MonoBehaviour が破棄される、もしくは任意のタイミングでキャンセルが実行されるトークンを取得する。
			CancellationToken ownerCancellationTokenDeactive	= GetOwnerCancellationTokenDeactive() ;

			//----------------------------------------------------------

			CancellationTokenSource		resultCancellationTokenSource ;
			CancellationToken			resultCancellationToken ;

			if( cancellationToken == default || cancellationToken.CanBeCanceled == false )
			{
				// 基本のキャンセルトークン生成

				resultCancellationTokenSource	= CancellationTokenSource.CreateLinkedTokenSource( cancellationTokenDeactive, ownerCancellationTokenDeactive, m_OwnerCancellationTokenOnDestroy ) ;
				resultCancellationToken			= resultCancellationTokenSource.Token ;
			}
			else
			{
				// 独自のキャンセルトークン生成

				resultCancellationTokenSource	= CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, cancellationTokenDeactive, ownerCancellationTokenDeactive, m_OwnerCancellationTokenOnDestroy ) ;
				resultCancellationToken			= resultCancellationTokenSource.Token ;
			}

			return ( resultCancellationTokenSource, resultCancellationToken ) ;
		}

		/// <summary>
		/// 実行中のタスク群を中断する
		/// </summary>
		/// <returns></returns>
		public bool CancelTasks()
		{
			if( m_CancellationTokenSourceDecative != null )
			{
				// 明示的なタスク群の中断を実行する
				if( m_CancellationTokenSourceDecative.IsCancellationRequested == false )
				{
					m_CancellationTokenSourceDecative.Cancel() ;
				}

				m_CancellationTokenSourceDecative.Dispose() ;
				m_CancellationTokenSourceDecative = null ;
			}

			return true ;		// 実行中のタスクは中断されたはず
		}

		/// <summary>
		/// タスクがキャンセルされた際に呼び出されるコールバック
		/// </summary>
		virtual protected void OnTasksCanceled()
		{
//			Debug.Log( "タスク群が中断されました:" + name ) ;
		}

		//-------------------------------------------------------------------------------------------
		// 待機メソッド群

		/// <summary>
		/// １フレーム分だけ待つ
		/// </summary>
		/// <param name="timing"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask Yield( PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				await UniTask.Yield( timing, token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 指定した時間だけ待つ
		/// </summary>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask Delay( int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				var delayTimeSpan = TimeSpan.FromMilliseconds( millisecondsDelay ) ;
				await UniTask.Delay( delayTimeSpan, ignoreTimeScale, delayTiming, token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 指定したフレーム数だけ待つ
		/// </summary>
		/// <param name="delayFrameCount"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask DelayFrame( int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				await UniTask.DelayFrame( delayFrameCount, delayTiming, token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// フレームの終わりまで待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask WaitForFixedUpdate( CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				await UniTask.WaitForFixedUpdate( token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 指定した時間だけ待つ
		/// </summary>
		/// <param name="time"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="delayTiming"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask WaitForSeconds( float time, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				var delayTimeSpan = TimeSpan.FromMilliseconds( time * 1000 ) ;
				await UniTask.Delay( delayTimeSpan, ignoreTimeScale, delayTiming, token ) ;
			}
			catch( Exception e )
			{
				if( e is OperationCanceledException )
				{
//					Debug.Log( "タスクはキャンセルされました" ) ;
					isCanceled = true ;
				}
				else
				{
					Debug.LogError( e.Message ) ;
				}
			}
			finally
			{
				if( tokenSource != null )
				{
//					Debug.Log( "独自のトークンソースを破棄します" ) ;
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
//				Debug.Log( "タスクキャンセル例外を返します" ) ;
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 条件が満たされてる間(true)は待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask WaitWhile( Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				await UniTask.WaitWhile( predicate, timing, token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 条件が満たされるまでの間(false)は待機する
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async UniTask WaitUntil( Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			bool isCanceled = false ;
			try
			{
				await UniTask.WaitUntil( predicate, timing, token ) ;
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
			finally
			{
				if( tokenSource != null )
				{
					tokenSource.Dispose() ;
				}
			}

			if( isCanceled == true )
			{
				throw new OperationCanceledException() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// コールバック用の時間経過チェックメソッド
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected Func<bool> WaitForSeconds_NotTask( float duration )
		{
			float startTime = Time.realtimeSinceStartup ;

			bool IsOver()
			{
				return ( Time.realtimeSinceStartup - startTime ) >= duration ;
			} ;

			return IsOver ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, tasks ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( List<System.Object> tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, tasks.ToArray() ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAny( CancellationToken cancellationToken, params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			if( tasks == null || tasks.Length == 0 )
			{
				// 待機する意味が無い
				return ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int countMax ;
			int countNow ;

			bool isCanceled	= false ;

			while( true )
			{
				countMax = 0 ;
				countNow = 0 ;

				foreach( var task in tasks )
				{
					if( task != null )
					{
						if( task is List<Task> )
						{
							// List<Task>
							List<Task> s_tasks = task as List<Task> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;	// 実行中のタスク数
									if( s_task.Status != TaskStatus.Running )
									{
										countNow ++ ;	// 完了
									}
								}
							}
						}
						else
						if( task is List<UniTask> )
						{
							// List<UniTask>
							List<UniTask> s_tasks = task as List<UniTask> ;
							foreach( var s_task in s_tasks )
							{
								countMax ++ ;
								if( s_task.Status != UniTaskStatus.Pending )
								{
									countNow ++ ;	// 停止中
								}
							}
						}
						else
						if( task is List<CustomYieldInstruction> )
						{
							// List<CustomYieldInstruction>
							List<CustomYieldInstruction> s_tasks = task as List<CustomYieldInstruction> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.keepWaiting != true )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is Task[] )
						{
							// Task[]
							Task[] s_tasks = task as Task[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.Status != TaskStatus.Running )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is UniTask[] )
						{
							// UniTask[]
							UniTask[] s_tasks = task as UniTask[] ;
							foreach( var s_task in s_tasks )
							{
								countMax ++ ;
								if( s_task.Status != UniTaskStatus.Pending )
								{
									countNow ++ ;	// 停止中
								}
							}
						}
						else
						if( task is CustomYieldInstruction[] )
						{
							// CustomYieldInstruction[]
							CustomYieldInstruction[] s_tasks = task as CustomYieldInstruction[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null )
								{
									countMax ++ ;
									if( s_task.keepWaiting != true )
									{
										countNow ++ ;	// 停止中
									}
								}
							}
						}
						else
						if( task is Task _1 )
						{
							// Task
							countMax ++ ;
							if( _1.Status != TaskStatus.Running )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is UniTask _2 )
						{
							// UniTask
							countMax ++ ;
							if( _2.Status != UniTaskStatus.Pending )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is UniTask<bool>	_201 ){ countMax ++ ; if( _201.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<byte>	_202 ){ countMax ++ ; if( _202.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<char>	_203 ){ countMax ++ ; if( _203.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<short>	_204 ){ countMax ++ ; if( _204.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<ushort>	_205 ){ countMax ++ ; if( _205.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<int>	_206 ){ countMax ++ ; if( _206.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<uint>	_207 ){ countMax ++ ; if( _207.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<long>	_208 ){ countMax ++ ; if( _208.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<ulong>	_209 ){ countMax ++ ; if( _209.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<float>	_210 ){ countMax ++ ; if( _210.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<double>	_211 ){ countMax ++ ; if( _211.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						if( task is UniTask<string>	_212 ){ countMax ++ ; if( _212.Status != UniTaskStatus.Pending ){ countNow ++ ; } }
						else
						if( task is CustomYieldInstruction _3 )
						{
							// CustomYieldInstruction
							countMax ++ ;
							if( _3.keepWaiting != true )
							{
								countNow ++ ;	// 停止中
							}
						}
						else
						if( task is Func<bool> _4 )
						{
							// Func<bool>
							countMax ++ ;
							if( _4() != false )
							{
								countNow ++ ;	// 停止中
							}
						}
					}
				}

				if( countMax == 0 || countNow >  0 )
				{
					// 実行されているタスクは無いか１つ以上終了した
					break ;
				}

				//---------------------------------

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAny( params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAny( default, funcs ) ;
		}

		/// <summary>
		/// いずれか１つのタスクが完了になるまで待機する(全て未実行状態では待機しない)　※基本的に実行中のタスクのみ引数に渡すようにしてください
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAny( CancellationToken cancellationToken, params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			if( funcs == null || funcs.Length == 0 )
			{
				// 待機する意味が無い
				return ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int countMax ;
			int countNow ;

			bool isCanceled	= false ;

			while( true )
			{
				countMax = 0 ;
				countNow = 0 ;

				foreach( var func in funcs )
				{
					if( func != null )
					{
						if( func is Func<bool> _1 )
						{
							// Func<bool>
							countMax ++ ;
							if( _1() == true )
							{
								countNow ++ ;		// 完了中
							}
						}
					}
				}

				if( countMax == 0 || countNow >  0 )
				{
					// 実行されているタスクは無いか１つ以上終了した
					break ;
				}

				//---------------------------------

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, tasks ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( List<System.Object> tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, tasks.ToArray() ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAll( CancellationToken cancellationToken, params System.Object[] tasks )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			if( tasks == null || tasks.Length == 0 )
			{
				// 待機する意味が無い
				return ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int count ;

			bool isCanceled	= false ;

			while( true )
			{
				count = 0 ;

				foreach( var task in tasks )
				{
					if( task != null )
					{
						if( task is List<Task> )
						{
							// List<Task>
							List<Task> s_tasks = task as List<Task> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.Status == TaskStatus.Running )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is List<UniTask> )
						{
							// List<UniTask>
							List<UniTask> s_tasks = task as List<UniTask> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task.Status == UniTaskStatus.Pending )
								{
									count ++ ;	// 実行中(未実行は Succeeded)
								}
							}
						}
						else
						if( task is List<CustomYieldInstruction> )
						{
							// List<CustomYieldInstruction>
							List<CustomYieldInstruction> s_tasks = task as List<CustomYieldInstruction> ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.keepWaiting == true )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is Task[] )
						{
							// Task[]
							Task[] s_tasks = task as Task[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.Status == TaskStatus.Running )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is UniTask[] )
						{
							// UniTask[]
							UniTask[] s_tasks = task as UniTask[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task.Status == UniTaskStatus.Pending )
								{
									count ++ ;	// 実行中(未実行は Succeeded)
								}
							}
						}
						else
						if( task is CustomYieldInstruction[] )
						{
							// CustomYieldInstruction[]
							CustomYieldInstruction[] s_tasks = task as CustomYieldInstruction[] ;
							foreach( var s_task in s_tasks )
							{
								if( s_task != null && s_task.keepWaiting == true )
								{
									count ++ ;	// 実行中
								}
							}
						}
						else
						if( task is Task _1 )
						{
							// Task
							if( _1.Status == TaskStatus.Running )
							{
								count ++ ;	// 実行中
							}
						}
						else
						if( task is UniTask _2 )
						{
							// UniTask
							if( _2.Status == UniTaskStatus.Pending )
							{
								count ++ ;	// 実行中(未実行は Succeeded)
							}
						}
						else
						if( task is UniTask<bool>	_201 ){ if( _201.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<byte>	_202 ){ if( _202.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<char>	_203 ){ if( _203.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<short>	_204 ){ if( _204.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<ushort>	_205 ){ if( _205.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<int>	_206 ){ if( _206.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<uint>	_207 ){ if( _207.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<long>	_208 ){ if( _208.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<ulong>	_209 ){ if( _209.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<float>	_210 ){ if( _210.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<double>	_211 ){ if( _211.Status == UniTaskStatus.Pending ){ count ++ ; } }
						if( task is UniTask<string>	_212 ){ if( _212.Status == UniTaskStatus.Pending ){ count ++ ; } }
						else
						if( task is CustomYieldInstruction _3 )
						{
							// CustomYieldInstruction
							if( _3.keepWaiting == true )
							{
								count ++ ;	// 実行中
							}
						}
						else
						if( task is Func<bool> _4 )
						{
							// Func<bool>
							if( _4() == false )
							{
								count ++ ;	// 実行中
							}
						}
					}
				}

				if( count == 0 )
				{
					break ;
				}

				//---------------------------------------------------------

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public UniTask WhenAll( params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			return WhenAll( default, funcs ) ;
		}

		/// <summary>
		/// 全ての非同期処理の終了を待つ
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask WhenAll( CancellationToken cancellationToken, params Func<bool>[] funcs )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			if( funcs == null || funcs.Length == 0 )
			{
				// 待機する意味が無い
				return ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			int count ;

			bool isCanceled	= false ;

			while( true )
			{
				count = 0 ;

				foreach( var func in funcs )
				{
					if( func != null )
					{
						if( func is Func<bool> _1 )
						{
							// Func<bool>
							if( _1() == false )
							{
								count ++ ;	// 条件が満たされていない
							}
						}
					}
				}

				if( count == 0 )
				{
					break ;
				}

				//---------------------------------------------------------

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		/// <summary>
		/// １つのタスクを待ちつつその結果を取得する
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		public async UniTask<T> When<T>( UniTask<T> task, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない

			bool isCanceled	= false ;

			while( true )
			{
				if( task.Status != UniTaskStatus.Pending )
				{
					break ;	// 終了
				}

				//---------------------------------
				// 直接呼び出し元タスクのキャンセル対応

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}

			//------------------------------------------------------------------------------------------

			if( task.Status == UniTaskStatus.Faulted )
			{
				// 失敗：例外扱いとする
				string errorMessage = $"[CancelableTask] When<T> : Task failed." ;
				Debug.LogWarning( errorMessage ) ;
				throw new Exception( errorMessage ) ;
			}

			if( task.Status == UniTaskStatus.Canceled )
			{
				// 中断：デフォルト値が取得される(オーナータスクを含めた一括中断は行わない)
				return default ;
			}

			// 待機対象のタスクは主に常駐インスタンスにあるものであるためキャンセルがあっても直接の呼び出しタスクはキャンセルさせない
			// 結果値を返す
			return task.GetAwaiter().GetResult() ;
		}

		/// <summary>
		/// １つのタスクコルーチンの終了を待つ
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public async UniTask When( System.Object task, CancellationToken cancellationToken = default )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			if( task == null )
			{
				// 待機する意味が無い
				return ;
			}

			//----------------------------------------------------------

			// OnDestroy() 実行後の場合はリーク対策として強制的にキャンセルをかける
			( var tokenSource, var token ) = GetActiveCancellation( cancellationToken ) ;

			//----------------------------------

			// 重要
			// 判定対象のタスクは実行中以外(失敗・中断・終了)は全て終了扱いとする
			// よって判定対象のタスクが中断された場合のオーナータスクを含めた一括中断は行われない
			int count ;

			bool isCanceled	= false ;

			while( true )
			{
				if( task is Task _1 )
				{
					// Task
					if( _1.Status == TaskStatus.Faulted || _1.Status == TaskStatus.Canceled )
					{
						return ;
					}

					if( _1.Status != TaskStatus.Running )
					{
						return ;
					}
				}
				else
				if( task is UniTask _2 )
				{
					// UniTask
					if( _2.Status == UniTaskStatus.Faulted || _2.Status == UniTaskStatus.Canceled )
					{
						return ;
					}
					if( _2.Status == UniTaskStatus.Succeeded )
					{
						return ;
					}
				}
				else
				if( task is CustomYieldInstruction _3 )
				{
					// CustomYieldInstruction
					if( _3.keepWaiting != true )
					{
						// コルーチンが終了した(終了と非アクティブによる中断の区別はつかない)
						return ;
					}
				}
				else
				if( task is Func<bool> _4 )
				{
					// Func<bool>
					if( _4() != false )
					{
						return ;
					}
				}
				else
				//---------------------------------------------------------
				// 以下、誤って配列系を入力してしまった場合の保険
				if( task is List<Task> )
				{
					// List<Task>
					count = 0 ;
					List<Task> s_tasks = task as List<Task> ;
					foreach( var s_task in s_tasks )
					{
						if( s_task != null && s_task.Status == TaskStatus.Running )
						{
							count ++ ;	// 実行中
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				if( task is List<UniTask> )
				{
					// List<UniTask>
					count = 0 ;
					List<UniTask> s_tasks = task as List<UniTask> ;
					foreach( var s_task in s_tasks )
					{
						if( s_task.Status == UniTaskStatus.Pending )
						{
							count ++ ;	// 実行中(未実行は Succeeded)
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				if( task is List<CustomYieldInstruction> )
				{
					// List<CustomYieldInstruction>
					count = 0 ;
					List<CustomYieldInstruction> s_tasks = task as List<CustomYieldInstruction> ;
					foreach( var s_task in s_tasks )
					{
						if( s_task != null && s_task.keepWaiting == true )
						{
							count ++ ;	// 実行中
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				if( task is Task[] )
				{
					// Task[]
					count = 0 ;
					Task[] s_tasks = task as Task[] ;
					foreach( var s_task in s_tasks )
					{
						if( s_task != null && s_task.Status == TaskStatus.Running )
						{
							count ++ ;	// 実行中
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				if( task is UniTask[] )
				{
					// UniTask[]
					count = 0 ;
					UniTask[] s_tasks = task as UniTask[] ;
					foreach( var s_task in s_tasks )
					{
						if( s_task.Status == UniTaskStatus.Pending )
						{
							count ++ ;	// 実行中(未実行は Succeeded)
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				if( task is CustomYieldInstruction[] )
				{
					// CustomYieldInstruction[]
					count = 0 ;
					CustomYieldInstruction[] s_tasks = task as CustomYieldInstruction[] ;
					foreach( var s_task in s_tasks )
					{
						if( s_task != null && s_task.keepWaiting == true )
						{
							count ++ ;	// 実行中
						}
					}

					if( count == 0 )
					{
						return ;
					}
				}
				else
				{
					if( task != null )
					{
						Debug.LogWarning( "[CancelableTask]:When() Can not wait for task finish. because input object type is bad = " + task.GetType() ) ;
					}
					// task が null の場合は無視する
					return ;
				}

				//---------------------------------

				try
				{
					await UniTask.Yield( PlayerLoopTiming.Update, token ) ;
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
					break ;
				}
			}

			//----------------------------------------------------------
			// ループ終了後

			if( tokenSource != null )
			{
				tokenSource.Dispose() ;
			}

			if( isCanceled == true )
			{
				// 中断された場合は例外を投げる
				throw new OperationCanceledException() ;
			}
		}

		//-------------------------------------------------------------------------------------------
		// ユーティリティ

		/// <summary>
		/// トゥイーンのヘルパー
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Action<float> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1, Func<bool> onCancel = null )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				if( onCancel != null )
				{
					if( onCancel() == true )
					{
						// 中断
						break ;
					}
				}

				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				await Yield() ;
			}
		}

		/// <summary>
		/// トゥイーンのヘルパー(動的なタイムスケール変更に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,float> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1, Func<bool> onCancel = null )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				if( onCancel != null )
				{
					if( onCancel() == true )
					{
						// 中断
						break ;
					}
				}

				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				timeScale = onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				await Yield() ;
			}
		}


		/// <summary>
		/// トゥイーンのヘルパー(途中中断に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,bool> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1, Func<bool> onCancel = null )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				return ;
			}

			float timer = 0 ;
			while( timer <  duration )
			{
				if( onCancel != null )
				{
					if( onCancel() == true )
					{
						// 中断
						break ;
					}
				}

				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				if( onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) == true )
				{
					break ;
				}

				await Yield() ;
			}
		}

		/// <summary>
		/// トゥイーンのヘルパー(動的なタイムスケール変更と中断に対応)
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="easeType"></param>
		/// <returns></returns>
		protected async UniTask Tween( Func<float,( float, bool )> onFrameUpdate, float duration, EaseTypes easeType = EaseTypes.Linear, float timeScale = 1, Func<bool> onCancel = null )
		{
			if( m_GameObjectWasDestroyed == true )
			{
				// 既に GameObject が破棄されている

				// タスクをまとめてキャンセルする
				throw new OperationCanceledException() ;
			}

			//----------------------------------------------------------

			if( onFrameUpdate == null )
			{
				return ;
			}

			if( duration <= 0 )
			{
				onFrameUpdate( 1 ) ;
				return ;
			}

			float timer = 0 ;
			bool stop ;
			while( timer <  duration )
			{
				if( onCancel != null )
				{
					if( onCancel() == true )
					{
						// 中断
						break ;
					}
				}

				timer += ( Time.deltaTime * timeScale ) ;
				if( timer >  duration )
				{
					timer  = duration ;
				}

				( timeScale, stop ) = onFrameUpdate( Ease.GetValue( timer / duration, easeType ) ) ;

				if( stop == true )
				{
					break ;
				}

				await Yield() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 他のゲームオブジェクト(インスタンス)を破棄する
		/// </summary>
		/// <param name="go"></param>
		protected void DestroyInstance( UnityEngine.Object instance )
		{
#if UNITY_EDITOR
			if( Application.isPlaying == false )
			{
				GameObject.DestroyImmediate( instance, false ) ;
			}
			else
#endif
			{
				GameObject.Destroy( instance ) ;
			}
		}
	}
}
