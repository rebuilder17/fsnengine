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
/// 스크립트 로드 완료 이벤트
/// </summary>
public interface IFSNScriptLoadHandler : IEventSystemHandler
{
	void OnScriptLoadComplete();
}

/// <summary>
/// 게임 중 메뉴 토글 이벤트 (한 손가락으로 터치 후 swipe하지 않고 떼기)
/// </summary>
public interface IFSNMenuToggleHandler : IEventSystemHandler
{
	void OnToggleMenu();
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
	bool							m_swippedAnyway;		// 터치하고 나서 어쨌든 움직이긴 움직였는지
	FSNInGameSetting.FlowDirection	m_swipeDirection;		// 최근에 (혹은 현재) 민 방향
	float							m_swipeRatio;			// 전체에서 얼마만큼 밀었는지 (비율)

	bool							m_swipeEventSent;		// swipe 이벤트를 보냈는지
	bool							m_swipeEventSentWasWrongDir;	// 잘못된 방향으로 이벤트를 보낸 것이었는지

	//float							m_caughtSwipeRatio;		// swipe ratio가 0이 되지 않았을 때 다시 swipe를 시작할 경우 추가해줄 값

	bool							m_enginePause;			// 엔진 일시정지

	// NOTE : 현재 ExecuteEvents 가 원하는 방식대로 동작하지 않아서, 여기에 미리 핸들러들을 지정해둘 수 있게 함.
	// 나중에 이 부분이 해결되면 그에 맞게 수정할 것
	HashSet<GameObject>				m_swipeHandlers;



	/// <summary>
	/// 엔진에서 호출하는 초기화
	/// </summary>
	public void Initialize()
	{
		m_seqEngine		= GetComponent<FSNSequenceEngine>();

		m_swipeHandlers	= new HashSet<GameObject>();
	}

	void Update()
	{
		if (!m_swipping && !m_swipeCompleted && m_swipeRatio > 0)	// swipe중이 아니고 swipe를 완료하지도 않은 경우, 원래 위치로 튕겨져 돌아가는 효과
		{
			m_swipeRatio	*= 0.5f;
			if (m_swipeRatio < 0.01f) m_swipeRatio = 0;
			m_seqEngine.PartialSwipe(m_swipeDirection, m_swipeRatio * c_partialSwipeLimit);
		}

		if(Input.GetKeyUp(KeyCode.Escape))							// ESC (Andoid 백버튼) - 토글 메뉴 대응
		{
			ExecuteMenuToggleEvent(
				(obj, param) =>
				{
					obj.OnToggleMenu();
				});
			m_swippedAnyway = true;	// FIX : 중복 이벤트 실행을 막기 위해. 좀 지저분한 방법이라서 보완이 필요할지도.
		}
	}


	/// <summary>
	/// swipe 핸들러 추가
	/// </summary>
	/// <param name="handler"></param>
	public void AddSwipeHandler(GameObject handler)
	{
		m_swipeHandlers.Add(handler);
	}

	/// <summary>
	/// Swipe 핸들러 제거
	/// </summary>
	/// <param name="handler"></param>
	public void RemoveSwipeHandler(GameObject handler)
	{
		m_swipeHandlers.Remove(handler);
	}

	/// <summary>
	/// Swipe 이벤트 실행
	/// </summary>
	/// <param name="function"></param>
	public void ExecuteSwipeEvent(ExecuteEvents.EventFunction<IFSNSwipeHandler> function)
	{
		foreach (var handler in m_swipeHandlers)
		{
			ExecuteEvents.ExecuteHierarchy<IFSNSwipeHandler>(handler, null, function);
		}
	}

	/// <summary>
	/// 스크립트 로드 이벤트 실행
	/// </summary>
	/// <param name="function"></param>
	public void ExecuteScriptLoadEvent(ExecuteEvents.EventFunction<IFSNScriptLoadHandler> function)
	{
		// NOTE : 현재는 swipe handler 와 통합해서 사용중임. 나중에 분리해야할 때 분리할 것
		
		foreach (var handler in m_swipeHandlers)
		{
			ExecuteEvents.ExecuteHierarchy<IFSNScriptLoadHandler>(handler, null, function);
		}
	}

	/// <summary>
	/// 토글 메뉴 호출 이벤트 실행
	/// </summary>
	/// <param name="function"></param>
	public void ExecuteMenuToggleEvent(ExecuteEvents.EventFunction<IFSNMenuToggleHandler> function)
	{
		// NOTE : 현재는 swipe handler 와 통합해서 사용중임. 나중에 분리해야할 때 분리할 것

		foreach (var handler in m_swipeHandlers)
		{
			ExecuteEvents.ExecuteHierarchy<IFSNMenuToggleHandler>(handler, null, function);
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
		m_swippedAnyway		= !m_seqEngine.CanSwipe;	// 터치 시작시 Swipe가 블록된 상태에서는 메뉴를 부를 수 없음
	}

	/// <summary>
	/// swipe 변위 보내기
	/// </summary>
	public void Swipe(FSNInGameSetting.FlowDirection direction, float distance)
	{
		if (m_enginePause)		// 엔진 일시정지시에는 이벤트 처리를 받지 않는다.
			return;

		m_swippedAnyway		= true;												// 어쨌든 swipe를 하긴 했음. release 하더라도 메뉴 토글은 콜하지 않도록

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
						ExecuteSwipeEvent((obj, param) =>
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

					ExecuteSwipeEvent(
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
					ExecuteSwipeEvent((obj, param) =>
						{
							obj.OnTryingSwipeToWrongDirection(direction);		// 잘못된 방향으로 swipe 이벤트
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
				ExecuteSwipeEvent(
					(obj, param) =>
					{
						obj.OnReleaseSwipeToWrongDirection(m_swipeDirection);
					});
			}
			else
			{
				ExecuteSwipeEvent(
					(obj, param) =>
					{
						obj.OnReleaseSwipe(m_swipeDirection);
					});
			}

			m_swipeEventSent	= false;
		}

		if (m_seqEngine.CanSwipe && !m_swippedAnyway)	// 터치 후에 움직이지 않고 바로 뗀 경우, 메뉴 토글 호출 (단 Swipe가 막히지 않은 상태에서만)
		{
			ExecuteMenuToggleEvent(
				(obj, param) =>
				{
					obj.OnToggleMenu();
				});

			m_swippedAnyway = true;						// 중복 이벤트 실행을 막기 위해...
		}
		
	}

	/// <summary>
	/// Swipe가 현재 막혔는지
	/// </summary>
	public bool SwipeBlocked
	{
		get { return !m_seqEngine.CanSwipe; }
	}

	/// <summary>
	/// Swipe 가능한 방향들
	/// </summary>
	public bool [] SwipeVaildDirections
	{
		get
		{
			bool [] valid	= new bool[4];
			for (int i = 0; i < 4; i++)
			{
				valid[i]	= m_seqEngine.SwipeDirectionAvailable((FSNInGameSetting.FlowDirection)i);
			}
			return valid;
		}
	}

	//==============================================================================

	/// <summary>
	/// 엔진 일시정지 상태로
	/// </summary>
	/// <param name="pause"></param>
	public void PauseEngine(bool pause = true)
	{
		if (m_enginePause == pause)	// 상태가 실제로 변하지 않는 경우엔 리턴
			return;

		if(pause)					// 일시정지를 거는 경우, swipe 상태를 해제한다.
		{
			ClearSwipe();
		}

		m_enginePause = pause;

		// TODO : 이벤트 쏘기
	}

	/// <summary>
	/// 엔진 일시정지 해제
	/// </summary>
	public void ResumeEngine()
	{
		PauseEngine(false);
	}

	/// <summary>
	/// 스크립트 로딩 완료
	/// </summary>
	public void ScriptLoadComplete()
	{
		ExecuteScriptLoadEvent((obj, param) =>
			{
				obj.OnScriptLoadComplete();
			});
	}
}
