using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// 전체 화면 터치 입력을 인식하는 모듈
/// </summary>
public sealed class FSNTouchSwipeHandler : MonoBehaviour
{
	// Properties

	[SerializeField]
	float					m_decideZoneRadius = 5;				// 시작점부터 이 범위를 넘어서야 실제 입력으로 처리.


	// Members

	float					m_screenSizeRatio	= 1;			// 스크린(픽셀)좌표 => 엔진 좌표로 변환하는 배수

	int						m_currentTouchCount	= 0;			// 현재 터치 수.
	TouchProcess			m_currentTouchProcedure;			// 현재 터치 처리 프로시저

	Dictionary<int, TouchProcess>	m_touchProcedures;			// 터치 처리 프로시저 리스트

	Dictionary<int, Vector2>	m_touchStartPoints;				// 터치 시작점


	void Awake()
	{
		m_touchProcedures	= new Dictionary<int, TouchProcess>();
		m_touchStartPoints	= new Dictionary<int, Vector2>();

		m_screenSizeRatio	= (float)Screen.height / FSNEngine.Instance.ScreenYSize;

		// 프로세스 등록
		m_touchProcedures[1]	= new SingleTouchProcess(this);
	}

	void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		var touches			= MouseTouchMimic.UpdateAndGet();
#else
		var touches			= Input.touches;
#endif
		var newTouchCount	= calculateActiveTouchCounts(touches);


		if(m_currentTouchCount != newTouchCount)				// 터치 수가 달라졌다면
		{
			EndTouchTracking(touches);							// 기존 터치 상태 종료
			StartTouchTracking(touches);						// 새 터치 상태 시작
		}

		if (m_currentTouchProcedure != null)					// 터치 프로시저가 지정된 경우라면 실행
		{
			m_currentTouchProcedure.Process(touches);
		}
	}

	/// <summary>
	/// 터치 추적 시작
	/// </summary>
	/// <param name="touches"></param>
	void StartTouchTracking(Touch[] touches)
	{
		m_currentTouchCount = 0;								// 현재 터치 카운트 초기화

		int count	= touches.Length;
		for (int i = 0; i < count; i++)
		{
			var touch	= touches[i];
			if (isActiveTouch(touch))							// 끝난 터치가 아니라면(=유효한 터치) 시작점 보관해두기
			{
				m_touchStartPoints[touch.fingerId]	= touch.position * m_screenSizeRatio;

				m_currentTouchCount++;							// 터치 카운트 증가
			}
		}

		// 터치 프로시저 세팅
		m_touchProcedures.TryGetValue(m_currentTouchCount, out m_currentTouchProcedure);

		if (m_currentTouchProcedure != null)
			m_currentTouchProcedure.Begin(touches);				// 터치 프로세서 시작
	}

	/// <summary>
	/// 터치 추적 종료
	/// </summary>
	void EndTouchTracking(Touch[] touches)
	{
		m_currentTouchCount = 0;								// 현재 터치 카운트 초기화
		m_touchStartPoints.Clear();

		if (m_currentTouchProcedure != null)
			m_currentTouchProcedure.End(touches);				// 터치 프로세서 종료
	}

	/// <summary>
	/// 현재 활성화된 터치 수 계산
	/// </summary>
	/// <param name="touches"></param>
	/// <returns></returns>
	static int calculateActiveTouchCounts(Touch[] touches)
	{
		int count	= touches.Length;
		int actives	= 0;
		for (int i = 0; i < count; i++)
		{
			if (isActiveTouch(touches[i]))
				actives++;
		}

		return actives;
	}

	/// <summary>
	/// 현재 유효한 터치인지
	/// </summary>
	/// <param name="touch"></param>
	/// <returns></returns>
	static bool isActiveTouch(Touch touch)
	{
		var phase		= touch.phase;
		bool phaseOK	= phase != TouchPhase.Canceled && phase != TouchPhase.Ended;

		if (!phaseOK)							// 터치중인 것이 아니라면 false 리턴
			return false;

		var eventSys	= EventSystem.current;
		bool uihitOK	= eventSys.IsPointerOverGameObject() || eventSys.IsPointerOverGameObject(touch.fingerId);

		return !uihitOK;						// UI 를 터치중인 것이 아니어야만 true
	}


	// 터치 프로시저

	abstract class TouchProcess
	{
		protected FSNTouchSwipeHandler Detector { get; private set; }

		/// <summary>
		/// 터치 시작
		/// </summary>
		/// <param name="touches"></param>
		public abstract void Begin(Touch[] touches);
		/// <summary>
		/// 터치 종료
		/// </summary>
		/// <param name="touches"></param>
		public abstract void End(Touch[] touches);
		/// <summary>
		/// 터치 처리, 매 프레임마다 호출됨
		/// </summary>
		/// <param name="touches"></param>
		public abstract void Process(Touch[] touches);


		public TouchProcess(FSNTouchSwipeHandler detector)
		{
			Detector	= detector;
		}
	}

	//======================================================================

	/// <summary>
	/// 단일 터치 처리기
	/// </summary>
	class SingleTouchProcess : TouchProcess
	{
		// Members

		bool		m_singleTouchRecStart = false;			// 터치 시작 후 인식 영역을 벗어난 적이 있는지 여부
		Vector2		m_singleTouchAxisMask = Vector2.zero;	// 특정 축으로 입력 걸러내기

		public SingleTouchProcess(FSNTouchSwipeHandler detector)
			: base(detector)
		{

		}

		public override void Begin(Touch[] touches)
		{
			FSNEngine.Instance.ControlSystem.StartSwipe();
		}

		public override void End(Touch[] touches)
		{
			m_singleTouchRecStart	= false;
			
			FSNEngine.Instance.ControlSystem.ClearSwipe();
		}

		public override void Process(Touch[] touches)
		{
			if (FSNEngine.Instance.ControlSystem.SwipeBlocked)	// 현재 입력 블록 상태면 어떤 처리도 하지 않는다
				return;

			int count = touches.Length;
			for(int i = 0; i < count; i++)					// 끝난 터치에 대한 정보가 있을 수 있으므로 루프를 돌아 체크한다.
			{
				var touch = touches[i];
				if(isActiveTouch(touch))					// 현재 활성화된 터치를 찾았다면
				{
					var moveVec	= (touch.position * Detector.m_screenSizeRatio) - Detector.m_touchStartPoints[touch.fingerId];

					if (!m_singleTouchRecStart && moveVec.magnitude > Detector.m_decideZoneRadius)	// 입력 인식이 인정되지 않은 상태에서 인식 범위를 벗어남
					{
						m_singleTouchAxisMask	= (Mathf.Abs(moveVec.x) > Mathf.Abs(moveVec.y))?
													Vector2.right : Vector2.up;				// x축과 y축 중 더 많이 움직인 쪽으로 입력을 필터링한다.

						m_singleTouchRecStart	= true;		// 인식 시작
					}

					if (m_singleTouchRecStart)				// 인식 상태에서...
					{
						if(touch.phase == TouchPhase.Moved)	// 움직인 경우만 처리한다. 매 프레임 항상 처리해야할수도...?
						{
							moveVec.Scale(m_singleTouchAxisMask);	// 특정한 축으로 필터링

							var dir		= FilteredVectorToDirection(moveVec);
							var dist	= Mathf.Abs(moveVec.x + moveVec.y);

							FSNEngine.Instance.ControlSystem.Swipe(dir, dist);	// 엔진으로 보내기
						}
					}

					break;									// 단일 터치 처리중이므로 하나 처리했다면 루프를 종료해도 된다
				}
			}
		}
	}

	/// <summary>
	/// 축으로 필터링된 벡터에서 방향을 얻어옴
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	static FSNInGameSetting.FlowDirection FilteredVectorToDirection(Vector2 vec)
	{
		if (vec.y > 0)
			return FSNInGameSetting.FlowDirection.Up;
		else if (vec.y < 0)
			return FSNInGameSetting.FlowDirection.Down;
		else if (vec.x > 0)
			return FSNInGameSetting.FlowDirection.Right;
		else if (vec.x < 0)
			return FSNInGameSetting.FlowDirection.Left;

		return FSNInGameSetting.FlowDirection.None;
	}

	//======================================================================

	/// <summary>
	/// 마우스 입력을 터치로 변환해주는 헬퍼 클래스
	/// </summary>
	static class MouseTouchMimic
	{
		static bool		s_downLastFrame = false;
		static Vector2	s_lastPosition	= Vector2.zero;

		public static Touch[] UpdateAndGet()
		{
			UnityTouchCreator touch	= null;

			var mousePos3D	= Input.mousePosition;
			var mousePos	= new Vector2(mousePos3D.x, mousePos3D.y);
			var downNow		= Input.GetMouseButton(0);

			if (!s_downLastFrame && downNow)			// 안눌렸다가 새로 눌림
			{
				touch			= new UnityTouchCreator();
				touch.phase		= TouchPhase.Began;
			}
			else if(s_downLastFrame && downNow)			// 계속 눌린 경우
			{
				touch			= new UnityTouchCreator();
				touch.phase		= (mousePos == s_lastPosition)? TouchPhase.Stationary : TouchPhase.Moved;
			}
			else if(s_downLastFrame && !downNow)
			{											// 눌렀다가 뗀 경우
				touch			= new UnityTouchCreator();
				touch.phase		= TouchPhase.Ended;
			}

			s_downLastFrame		= downNow;				// 눌렸는지 여부 보관

			if (touch != null)							// touch 오브젝트를 어쨌거나 생성하는 경우
			{
				touch.fingerId	= 1;
				touch.position	= mousePos;
				touch.deltaPosition	= mousePos - s_lastPosition;

				s_lastPosition	= mousePos;				// 마우스 좌표 보관

				return new Touch[] { touch.Create() };
			}
			else
			{											// Touch 오브젝트를 생성하지 않는 경우, 빈 배열 리턴
				return new Touch[0];
			}
		}
	}
}
