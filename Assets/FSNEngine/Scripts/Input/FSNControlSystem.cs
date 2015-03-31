using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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

	//float							m_caughtSwipeRatio;		// swipe ratio가 0이 되지 않았을 때 다시 swipe를 시작할 경우 추가해줄 값



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
		m_swipping	 = true;
	}

	/// <summary>
	/// swipe 변위 보내기
	/// </summary>
	public void Swipe(FSNInGameSetting.FlowDirection direction, float distance)
	{
		if(!m_swipeCompleted && m_seqEngine.CanSwipe && m_seqEngine.SwipeDirectionAvailable(direction))
		{
			m_swipeDirection= direction;

			var engine		= FSNEngine.Instance;

			float weight	= engine.InGameSetting.SwipeWeight;
			float maxdist	= (direction == FSNInGameSetting.FlowDirection.Left || direction == FSNInGameSetting.FlowDirection.Right)? 
								engine.ScreenXSize : engine.ScreenYSize;	// 해당 방향으로 최대한 밀 수 있는 거리

			float fullDist		= weight * maxdist;
			float curSwipeRatio	= distance / fullDist;
			if (curSwipeRatio < 1.0f)				// 아직 덜 밀었을 때
			{
				m_seqEngine.PartialSwipe(direction, curSwipeRatio * c_partialSwipeLimit);
				m_swipeRatio	= curSwipeRatio;
			}
			else
			{										// 완전히 밀었을 때
				m_seqEngine.FullSwipe(direction, c_partialSwipeLimit);
				m_swipeRatio	= 0;
			}
		}
	}

	/// <summary>
	/// swipe 종료.
	/// </summary>
	public void ClearSwipe()
	{
		m_swipping			= false;
		m_swipeCompleted	= false;
	}

	/// <summary>
	/// Swipe가 현재 막혔는지
	/// </summary>
	public bool SwipeBlocked
	{
		get { return !m_seqEngine.CanSwipe; }
	}
}
