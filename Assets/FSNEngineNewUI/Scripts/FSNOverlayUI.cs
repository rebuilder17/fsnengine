using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


/// <summary>
/// 오버레이 UI를 관리하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Canvas))]
public partial class FSNOverlayUI : MonoBehaviour, IFSNSwipeHandler, IFSNScriptLoadHandler, IFSNMenuToggleHandler
{
	// Constants

	// NOTE : 현재는 constant로 그대로 두지만, 나중에 옵션으로 설정 가능하게 해야할 수도 있음

	const float		c_idleVisibleDelay	= 1.0f;		// idle 판정 이후 idle UI 표시 지연 시간
	const float		c_swipeVisibleDelay	= 1.0f;		// idle 판정 이후 swipe표시 지연 시간
	const float		c_fadeInTime		= 1.0f;
	const float		c_fadeOutTime		= 0.15f;


	// Properties

	[SerializeField]
	CanvasGroup		m_idleUIGroup;			// Idle인 상황에 표시되는 Canvas Group

	[SerializeField]
	GameObject		m_indicatorSwipeUp;		// 위로 swipe 가능 표시
	[SerializeField]
	GameObject		m_indicatorSwipeDown;	// 아래로 swipe 가능 표시
	[SerializeField]
	GameObject		m_indicatorSwipeLeft;	//
	[SerializeField]
	GameObject		m_indicatorSwipeRight;

	[SerializeField]
	GameObject		m_indicatorInvalidUp;	// 위로 swipe 불가능 표시
	[SerializeField]
	GameObject		m_indicatorInvalidDown;
	[SerializeField]
	GameObject		m_indicatorInvalidLeft;
	[SerializeField]
	GameObject		m_indicatorInvalidRight;

	[SerializeField]
	float			m_swipeInvalidIndicatorTime	= 2f;	// swipe 불가능 표시의 최대 라이프타임

	[SerializeField]
	FSNBaseOverlayDialog[]	m_dialogs;					// OverlayUI에서 관리할 다이얼로그들

	// NOTE
	// swipe 가능 표시는 inactive 상태로 두다가 가능해질 경우 active로 전환한다.
	// swipe 불가능 표시는 inactive 상태로 항상 두고, 해당 오브젝트를 사유가 발생할 때마다 매번 복제한다.


	
	
	// Members

	bool			m_idle;								// idle 상태로 판정되었는지 여부
	bool			m_idleUIVisible;					// idle ui가 보이는 상태인지
	bool			m_swipeIndicatorVisible;			// swipe 표시가 보이는 상태인지

	float			m_nextIdleVisibleTime;				// Idle UI가 다시 보여질 시간
	float			m_nextSwipeIndVisibleTime;			// swipe 표시가 다시 보여질 시간

	Coroutine		m_currentIdleUICoRoutine;
	bool			m_idleChecking;						// Idle 체크 코루틴이 작동중인지

	DialogStack		m_dialogStack;						// 다이얼로그 스택



	// static
	public static FSNOverlayUI Instance { get; private set; }


	void Awake()
	{
		Instance = this;

		m_idle					= true;
		m_idleUIVisible			= false;
		m_swipeIndicatorVisible	= false;

		m_indicatorSwipeUp.SetActive(false);												// 방향 표시는 우선은 전부 감춰둔다
		m_indicatorSwipeDown.SetActive(false);
		m_indicatorSwipeLeft.SetActive(false);
		m_indicatorSwipeRight.SetActive(false);

		FSNEngine.Instance.ControlSystem.AddSwipeHandler(gameObject);	// 핸들러 등록. 임시방편에 가깝다....

		// 다이얼로그 스택 초기화

		m_dialogStack			= new DialogStack();
		int dialogCount			= m_dialogs.Length;
		for(int i = 0; i < dialogCount; i++)							// 다이얼로그들 스택에 미리 등록
		{
			m_dialogs[i].RegisterDialogProtocol(m_dialogStack);
		}
	}

	void OnDestroy()
	{
		FSNEngine.Instance.ControlSystem.RemoveSwipeHandler(gameObject);
		Instance = null;
	}

	void Update()
	{
		var dialogOpened	= !m_dialogStack.IsEmpty;
		FSNEngine.Instance.ControlSystem.PauseEngine(dialogOpened);									// 다이얼로그가 열려있는지 여부에 따라 엔진 일시정지 설정

		// 다이얼로그가 하나라도 열려있다면 idle 상태로 치지 않는다

		if (m_idle && !dialogOpened)																// ** Idle 상태인 경우 -> UI 표시 상태로 전환해야함
		{
			if (!m_idleUIVisible && m_nextIdleVisibleTime <= Time.time)								// idle UI가 아직 표시되지 않았고, 표시 시간에 도달한 경우
			{
				if (m_currentIdleUICoRoutine != null)												// coroutine이 아직도 돌고 있다면 멈추기
					StopCoroutine(m_currentIdleUICoRoutine);

				m_indicatorSwipeUp.SetActive(false);												// 방향 표시는 우선은 전부 감춰둔다
				m_indicatorSwipeDown.SetActive(false);
				m_indicatorSwipeLeft.SetActive(false);
				m_indicatorSwipeRight.SetActive(false);
				m_swipeIndicatorVisible		= false;


				m_currentIdleUICoRoutine	= StartCoroutine(IdleUIAlphaFade(0, 1, c_fadeInTime));	// Fade-in 시작

				m_idleUIVisible				= true;													// 상태값 설정
			}

			if (!m_swipeIndicatorVisible && m_nextSwipeIndVisibleTime <= Time.time)					// swipe 표시가 아직 표시되지 않았고, 표시 시간에 도달한 경우
			{
				var engine		= FSNEngine.Instance;
				var validDirs	= engine.ControlSystem.SwipeVaildDirections;						// 가능한 진행방향 모두 고르기
				var backdir		= engine.InGameSetting.BackwardFlowDirection;
				validDirs[(int)backdir]	= false;													// 반대방향은 표시하지 않는 것으로

				m_indicatorSwipeUp.SetActive(validDirs[(int)FSNInGameSetting.FlowDirection.Up]);	// Swipe 표시 active설정
				m_indicatorSwipeDown.SetActive(validDirs[(int)FSNInGameSetting.FlowDirection.Down]);
				m_indicatorSwipeLeft.SetActive(validDirs[(int)FSNInGameSetting.FlowDirection.Left]);
				m_indicatorSwipeRight.SetActive(validDirs[(int)FSNInGameSetting.FlowDirection.Right]);

				m_swipeIndicatorVisible			= true;												// 상태값 설정
			}
		}
		else
		{																// ** Idle 상태가 아닌 경우

			if (m_idleUIVisible)										// 표시중인 idle UI 감추기
			{
				if (m_currentIdleUICoRoutine != null)												// coroutine이 아직도 돌고 있다면 멈추기
					StopCoroutine(m_currentIdleUICoRoutine);

				m_currentIdleUICoRoutine	= StartCoroutine(IdleUIAlphaFade(1, 0, c_fadeOutTime));	// Fade-out 시작
				m_idleUIVisible				= false;												// 상태값 설정
			}
		}
	}

	IEnumerator IdleUIAlphaFade(float start, float end, float time)
	{
		// 현재의 alpha 값을 반영하여 알파 시작값, 실제 걸릴 시간을 다시 계산한다.

		float realStart	= m_idleUIGroup.alpha;
		float realTime	= time * (1 - (realStart - start) / (end - start));

		float startT	= Time.time;
		float endT		= Time.time + realTime;

		while (Time.time <= endT)						// 시간이 끝날 때까지 보간
		{
			m_idleUIGroup.alpha	= Mathf.Lerp(realStart, end, (Time.time - startT) / realTime);
			yield return null;
		}
		m_idleUIGroup.alpha		= end;					// 루프 끝날 때 최종 값을 확실하게 대입해준다

		m_currentIdleUICoRoutine = null;// 필요한가...??
	}

	IEnumerator CheckIdle()
	{
		if (m_idleChecking)								// 이 코루틴은 한번에 하나만 돌 수 있다
			yield break;
		m_idleChecking		= true;

		var ctrlSys	= FSNEngine.Instance.ControlSystem;

		while (ctrlSys.SwipeBlocked)					// swipe가 가능해질 때까지 대기
			yield return new WaitForSeconds(0.1f);
		SetIdle();										// 제약조건이 완전히 없어져야 Idle 체크


		m_idleChecking		= false;
	}

	//==============================================================

	/// <summary>
	/// Idle 상태로 전환
	/// </summary>
	void SetIdle()
	{
		m_idle						= true;
		//m_idleUIVisible				= false;
		//m_swipeIndicatorVisible		= false;

		m_nextIdleVisibleTime		= Time.time + c_idleVisibleDelay;
		m_nextSwipeIndVisibleTime	= Time.time + c_swipeVisibleDelay;
	}

	/// <summary>
	/// Busy (idle이 아님) 상태로 전환
	/// </summary>
	void SetBusy()
	{
		m_idle	= false;
	}

	//==============================================================


	/// <summary>
	/// 없는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	public void OnTryingSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction)
	{
		SetBusy();												// 움직임이므로 일단 바쁜 상태로

		GameObject orig	= null;
		switch(direction)
		{
			case FSNInGameSetting.FlowDirection.Up:
				orig	= m_indicatorInvalidUp;
				break;
			case FSNInGameSetting.FlowDirection.Down:
				orig	= m_indicatorInvalidDown;
				break;
			case FSNInGameSetting.FlowDirection.Left:
				orig	= m_indicatorInvalidLeft;
				break;
			case FSNInGameSetting.FlowDirection.Right:
				orig	= m_indicatorInvalidRight;
				break;
		}

		if (orig != null)										// swipe 불가 표시 생성
		{
			var newobj		= Instantiate<GameObject>(orig);
			var newtrans	= newobj.transform;
			var origtrans	= orig.transform;

			newtrans.SetParent(origtrans.parent);
			newtrans.position= origtrans.position;

			newobj.SetActive(true);
			Destroy(newobj, m_swipeInvalidIndicatorTime);		// 일정 시간 뒤에는 무조건 제거하도록
		}
	}

	/// <summary>
	/// 없는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	public void OnReleaseSwipeToWrongDirection(FSNInGameSetting.FlowDirection direction)
	{
		StartCoroutine(CheckIdle());	// 조건이 완전히 성립할 때까지 계속 Idle 체크
	}

	/// <summary>
	/// 있는 방향으로 swipe 시도, 시작
	/// </summary>
	/// <param name="direction"></param>
	public void OnTryingSwipe(FSNInGameSetting.FlowDirection direction)
	{
		SetBusy();
	}

	/// <summary>
	/// 있는 방향으로 swipe 시도하다가 중단함
	/// </summary>
	/// <param name="direction"></param>
	public void OnReleaseSwipe(FSNInGameSetting.FlowDirection direction)
	{
		StartCoroutine(CheckIdle());	// 조건이 완전히 성립할 때까지 계속 Idle 체크
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="direction"></param>
	public void OnCompleteSwipe(FSNInGameSetting.FlowDirection direction)
	{
		StartCoroutine(CheckIdle());	// 조건이 완전히 성립할 때까지 계속 Idle 체크
	}

	/// <summary>
	/// 스크립트 로딩 완료
	/// </summary>
	public void OnScriptLoadComplete()
	{
		m_idle = false;
		m_idleUIVisible			= false;
		m_swipeIndicatorVisible	= false;

		StartCoroutine(CheckIdle());	// 조건이 완전히 성립할 때까지 계속 Idle 체크
	}

	/// <summary>
	/// 토글 메뉴
	/// </summary>
	public void OnToggleMenu()
	{
		var menu	= GetDialog<FSNOverlayToggleMenu>();
		if (!menu.IsOpened)
		{
			if (m_dialogStack.IsEmpty)				// 다이얼로그가 하나도 없을 때만 연다
			{
				OpenDialog<FSNOverlayToggleMenu>();
			}
				
		}
		else
		{
			CloseDialog<FSNOverlayToggleMenu>();
		}
	}

	//==============================================================

	// 다이얼로그 관련

	/// <summary>
	/// 다이얼로그 객체 얻어오기
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T GetDialog<T>() where T : FSNBaseOverlayDialog
	{
		return m_dialogStack.GetDialog<T>();
	}

	/// <summary>
	/// 다이얼로그 열기
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public void OpenDialog<T>() where T : FSNBaseOverlayDialog
	{
		m_dialogStack.Open<T>();
	}

	/// <summary>
	/// 다이얼로그 닫기
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public void CloseDialog<T>() where T : FSNBaseOverlayDialog
	{
		m_dialogStack.Close<T>();
	}
}
