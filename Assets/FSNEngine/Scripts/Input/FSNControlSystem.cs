using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


/// <summary>
/// FSNControlSystem 에서 전역으로 보내는 swipe 이벤트 메세지를 처리하는 핸들러 구현
/// </summary>
public interface IFSNSwipeHandler : IEventSystemHandler
{
	/// <summary>
	/// 없는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	void OnTryingSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction);

	/// <summary>
	/// 없는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	void OnReleaseSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction);

	/// <summary>
	/// 있는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	void OnTryingSwipe(FSNInGameSetting.FlowDirection direction);

	/// <summary>
	/// 있는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	void OnReleaseSwipe(FSNInGameSetting.FlowDirection direction);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="direction"></param>
	void OnCompleteSwipe(FSNInGameSetting.FlowDirection direction);
}

/// <summary>
/// 엔진 전체의 Pause/Resume 이벤트
/// </summary>
public interface IFSNPauseHandler : IEventSystemHandler
{
	void OnEnginePause(bool pause);
}




/// <summary>
/// 입력을 통한 엔진 요소 통제와 관련된 기능을 관리하는 레이어
/// </summary>
public sealed class FSNControlSystem : MonoBehaviour
{
	// constants

	const float				c_partialSwipeLimit	= 0.5f;		// partial swipe로 인정되는 최대 비율


	// Members

	FSNSequenceEngine		m_seqEngine;

	bool							m_swipping	= false;	// swipe중인지
	bool							m_swipeCompleted;		// swipe로 페이지가 넘어갔는지 여부. 넘어갔다면 clear될 때까지는 swipe를 무시한다
	FSNInGameSetting.FlowDirection	m_swipeDirection;		// 최근에 (혹은 현재) 민 방향
	float							m_swipeRatio;			// 전체에서 얼마만큼 밀었는지 (비율)

	bool							m_swipeEventSent;		// swipe 이벤트를 보냈는지
	bool							m_swipeEventSentWasWrongDir;	// 잘못된 방향으로 이벤트를 보낸 것이었는지

	//float							m_caughtSwipeRatio;		// swipe ratio가 0이 되지 않았을 때 다시 swipe를 시작할 경우 추가해줄 값

	bool							m_enginePause;			// 엔진 일시정지



	/// <summary>
	/// 엔진에서 호출하는 초기화
	/// </summary>
	public void Initialize()
	{
		m_seqEngine		= GetComponent<FSNSequenceEngine>();
	}

	void Update()
	{
		if (!m_swipping && !m_swipeCompleted && m_swipeRatio > 0)	// swipe중이 아니고 swipe를 완료하지도 않은 경우, 원래 위치로 튕겨져 돌아가는 효과
		{
			m_swipeRatio	*= 0.5f;
			m_seqEngine.PartialSwipe(m_swipeDirection, m_swipeRatio * c_partialSwipeLimit);
		}
	}


	// 이벤트 콜


	/// <summary>
	/// swipe 시작
	/// </summary>
	public void StartSwipe()
	{
		if (m_enginePause)		// 엔진 일시정지시에는 이벤트 처리를 받지 않는다.
			return;

		m_swipping			= true;
		m_swipeEventSent	= false;
		m_swipeEventSentWasWrongDir = false;
	}

	/// <summary>
	/// swipe 변위 보내기
	/// </summary>
	public void Swipe(FSNInGameSetting.FlowDirection direction, float distance)
	{
		if (m_enginePause)		// 엔진 일시정지시에는 이벤트 처리를 받지 않는다.
			return;

		if(!m_swipeCompleted && m_seqEngine.CanSwipe)							// swipe 가능한 상태
		{
			var engine		= FSNEngine.Instance;

			m_swipeDirection= direction;										// 민 방향을 보관해둔다

			var dirvalid	= m_seqEngine.SwipeDirectionAvailable(direction);
			if (dirvalid)														// 밀 수 있는 방향일 때
			{
				float weight	= engine.InGameSetting.SwipeWeight;
				float maxdist	= (direction == FSNInGameSetting.FlowDirection.Left || direction == FSNInGameSetting.FlowDirection.Right)? 
									engine.ScreenXSize : engine.ScreenYSize;	// 해당 방향으로 최대한 밀 수 있는 거리

				float fullDist		= weight * maxdist;
				float curSwipeRatio	= distance / fullDist;
				if (curSwipeRatio < 1.0f)										// 아직 덜 밀었을 때
				{
					m_seqEngine.PartialSwipe(direction, curSwipeRatio * c_partialSwipeLimit);// Partial swipe
					m_swipeRatio	= curSwipeRatio;


					if (!m_swipeEventSent || m_swipeEventSentWasWrongDir)		// 아직 이벤트를 보낸 적이 없거나 잘못된 방향으로 이벤트를 보냈을 경우
					{
						ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(engine.gameObject, null, (obj, param) =>
							{
								obj.OnTryingSwipe(direction);					// 옳은 방향으로 swipe 이벤트
							});

						m_swipeEventSent			= true;
						m_swipeEventSentWasWrongDir	= false;
					}
				}
				else if(!m_swipeCompleted)										// 완전히 밀었을 때, 아직 이전에 완전히 밀지는 않았을 경우
				{
					m_seqEngine.FullSwipe(direction, c_partialSwipeLimit);		// Full swipe
					m_swipeRatio	= 0;
					m_swipeCompleted = true;

					ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(engine.gameObject, null,
						(obj, param) =>
						{
							obj.OnTryingSwipe(direction);						// swipe완료 이벤트
						});
				}
			}
			else
			{																	// 밀 수 없는 방향일 때

				if (!m_swipeEventSent || !m_swipeEventSentWasWrongDir)			// 아직 이벤트를 보낸 적이 없거나 올바른 방향으로 이벤트를 보냈을 경우
				{
					ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(engine.gameObject, null, (obj, param) =>
						{
							obj.OnTryingSwipe(direction);						// 잘못된 방향으로 swipe 이벤트
						});

					m_swipeEventSent			= true;
					m_swipeEventSentWasWrongDir	= true;
				}
			}
		}
	}

	/// <summary>
	/// swipe 종료.
	/// </summary>
	public void ClearSwipe()
	{
		if (m_enginePause)		// 엔진 일시정지시에는 이벤트 처리를 받지 않는다.
			return;

		m_swipping			= false;
		m_swipeCompleted	= false;

		if (m_swipeEventSent)						// swipe 이벤트를 이미 보낸 경우, release 이벤트 보내기
		{
			if (m_swipeEventSentWasWrongDir)
			{
				ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(FSNEngine.Instance.gameObject, null,
					(obj, param) =>
					{
						obj.OnReleaseSwipeToWrongDirection(m_swipeDirection);
					});
			}
			else
			{
				ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(FSNEngine.Instance.gameObject, null,
					(obj, param) =>
					{
						obj.OnReleaseSwipe(m_swipeDirection);
					});
			}

			m_swipeEventSent	= false;
		}
		
	}

	/// <summary>
	/// Swipe가 현재 막혔는지
	/// </summary>
	public bool SwipeBlocked
	{
		get { return !m_seqEngine.CanSwipe; }
	}

	//==============================================================================

	/// <summary>
	/// 엔진 일시정지 상태로
	/// </summary>
	/// <param name="pause"></param>
	public void PauseEngine(bool pause = true)
	{
		if(pause)					// 일시정지를 거는 경우, swipe 상태를 해제한다.
		{
			ClearSwipe();
		}

		m_enginePause = pause;
	}

	/// <summary>
	/// 엔진 일시정지 해제
	/// </summary>
	public void ResumeEngine()
	{
		PauseEngine(false);
	}
}
