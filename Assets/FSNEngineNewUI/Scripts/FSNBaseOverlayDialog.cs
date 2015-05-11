﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// FSNOverlayUI 에서 관리하는 다이얼로그 기반 클래스
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FSNBaseOverlayDialog : MonoBehaviour
{
	// Constants

	const float		c_transitionTime	= 0.3f;		// 전환 시간

	const float		c_transitionSclae	= 1.4f;		// 트랜지션시 스케일
	const float		c_transitionTimeT	= 0.2f;		// 시간(t)의 제곱수


	// Properties

	[SerializeField]
	bool			m_isPopup;					// 팝업 다이얼로그인지 여부
	[SerializeField]
	bool			m_closeWhenOtherDialogPops;	// 다른 다이얼로그가 팝업되면 자동으로 닫는다


	public bool IsPopup { get { return m_isPopup; } }
	public bool shouldCloseWhenOtherDialogPops { get { return m_closeWhenOtherDialogPops; } }


	// 초기화

	bool			m_awakeCalled = false;
	CanvasGroup		m_canvasGroup;
	RectTransform	m_trans;

	void Awake()
	{
		if (m_awakeCalled) return;
		m_awakeCalled	= true;

		m_canvasGroup	= GetComponent<CanvasGroup>();
		m_trans			= GetComponent<RectTransform>();

		InitTransitions();
	}


	//====================================================================

	public delegate bool ProtocolBoolDelegate();

	/// <summary>
	/// 외부에서 각종 기능을 호출하기 위한 프로토콜. FSNOverlayUI 에게만 실제 객체가 전달된다.
	/// </summary>
	public interface Protocol
	{
		/// <summary>
		/// 다이얼로그 본체
		/// </summary>
		FSNBaseOverlayDialog DialogRef { get; }

		/// <summary>
		/// 초기화
		/// </summary>
		System.Action Initialize { get; }

		/// <summary>
		/// 리셋
		/// </summary>
		System.Action Reset { get; }

		ProtocolBoolDelegate Open { get; }
		ProtocolBoolDelegate Close { get; }
		System.Action ToBack { get; }
		System.Action ToForth { get; }

		/// <summary>
		/// 입력 가능한 상태인지
		/// </summary>
		bool Interactable { get; set; }

		/// <summary>
		/// Z 오더
		/// </summary>
		int ZOrder { get; set; }

		// 다이얼로그에서 외부로 호출하는 콜백들. 외부에서 지정해줘야한다

		System.Action CallOpenCB { set; }
		System.Action CallCloseCB { set; }
	}

	/// <summary>
	/// 실제 Protocol 객체 구현
	/// </summary>
	private class ProtocolImpl : Protocol
	{
		public FSNBaseOverlayDialog DialogRef { get; set; }

		public System.Action		Initialize { get; set; }
		public System.Action		Reset { get; set; }
		public ProtocolBoolDelegate Open { get; set; }
		public ProtocolBoolDelegate Close { get; set; }
		public System.Action ToBack { get; set; }
		public System.Action ToForth { get; set; }

		public System.Action CallOpenCB
		{
			get { return DialogRef.OpenSelf; }
			set { DialogRef.OpenSelf = value; }
		}
		public System.Action CallCloseCB
		{
			get { return DialogRef.CloseSelf; }
			set { DialogRef.CloseSelf = value; }
		}

		public bool Interactable
		{
			get { return DialogRef.Interactable; }
			set { DialogRef.Interactable = value; }
		}

		public int ZOrder
		{
			get { return (int)DialogRef.ZOrder; }
			set { DialogRef.ZOrder = value; }
		}
	}

	/// <summary>
	/// 프로토콜 오브젝트 생성. (OverlayUI 에 전달하기 위함)
	/// </summary>
	/// <returns></returns>
	private Protocol GenerateProtocolObj()
	{
		var prot		= new ProtocolImpl();

		prot.DialogRef	= this;

		// 함수 바인딩
		prot.Initialize	= Initialize;
		prot.Reset		= Reset;
		prot.Open		= Open_real;
		prot.Close		= Close_real;
		prot.ToBack		= ToBack;
		prot.ToForth	= ToForth;

		return prot;
	}

	/// <summary>
	/// 다이얼로그 프로토콜을 추가한다
	/// </summary>
	/// <param name="protadd"></param>
	public void RegisterDialogProtocol(FSNOverlayUI.IDialogProtocolAdd protadd)
	{
		protadd.AddDialogProtocol(this.GetType(), GenerateProtocolObj());
	}

	//====================================================================

	// 전환 효과 관련


	/// <summary>
	/// 트랜지션 종류
	/// </summary>
	enum TransitionType { Open, Close, Back, Forth, };

	/// <summary>
	/// 트랜지션 코루틴 델리게이트
	/// </summary>
	/// <returns></returns>
	delegate IEnumerator TransitionCoroutine();

	/// <summary>
	/// 트랜지션 코루틴 딕셔너리
	/// </summary>
	Dictionary<TransitionType, TransitionCoroutine> m_transitionCoDict;
	
	private System.Action			m_transCompleteCB;		// 트랜지션 완료시 콜백
	Coroutine						m_curCoroutine;

	/// <summary>
	/// 현재 전환중인지 여부
	/// </summary>
	public bool IsTransitioning { get; private set; }

	/// <summary>
	/// 트랜지션 상태를 덮어쓸 수 있는지 여부
	/// </summary>
	public bool TransitionOverriadable { get; private set; }



	void InitTransitions()
	{
		m_transitionCoDict	= new Dictionary<TransitionType, TransitionCoroutine>()
		{
			{TransitionType.Open,	TransCo_Open},
			{TransitionType.Close,	TransCo_Close},
			{TransitionType.Back,	TransCo_Back},
			{TransitionType.Forth,	TransCo_Forth},
		};
	}

	/// <summary>
	/// 트랜지션 시작
	/// </summary>
	/// <param name="type"></param>
	bool StartTransition(TransitionType type, System.Action endCB)
	{
		if (IsTransitioning)						// 트랜지션 중일 때는 호출 불가능
		{
			if (TransitionOverriadable)				// 단 오버라이드를 허용할 때는 코루틴을 멈춘다
			{
				StopCoroutine(m_curCoroutine);
			}
			else
			{										// 오버라이드도 불가능할 때는 리턴
				return false;
			}
		}

		m_transCompleteCB	= endCB;				// 콜백 지정하기
		m_curCoroutine = StartCoroutine(m_transitionCoDict[type]());	// 코루틴 시작

		return true;
	}

	// 트랜지션 코루틴들

	/// <summary>
	/// 열기 트랜지션
	/// </summary>
	/// <returns></returns>
	IEnumerator TransCo_Open()
	{
		IsTransitioning	= true;							// 트랜지션 시작
		TransitionOverriadable	= false;
		float startTime	= Time.realtimeSinceStartup;

		while(true)
		{
			float elapsedTime	= Time.realtimeSinceStartup - startTime;
			if (elapsedTime > c_transitionTime)			// 트랜지션 시간이 지날 때까지 반복
				break;

			float t				= elapsedTime / c_transitionTime;
			m_canvasGroup.alpha	= Mathf.Lerp(0, 1, t);
			m_trans.localScale	= Vector3.one * Mathf.Lerp(c_transitionSclae, 1, Mathf.Pow(t, c_transitionTimeT));

			yield return null;
		}

		// 루프 종료 후에는 최종 설정값 한번 더 적용
		m_canvasGroup.alpha		= 1;
		m_trans.localScale		= Vector3.one;

		IsTransitioning	= false;						// 트랜지션 끝
		m_transCompleteCB();							// 트랜지션 완료 콜백 호출
	}

	/// <summary>
	/// 닫기 트랜지션
	/// </summary>
	/// <returns></returns>
	IEnumerator TransCo_Close()
	{
		IsTransitioning	= true;							// 트랜지션 시작
		TransitionOverriadable	= false;
		float startTime	= Time.realtimeSinceStartup;

		while (true)
		{
			float elapsedTime	= Time.realtimeSinceStartup - startTime;
			if (elapsedTime > c_transitionTime)			// 트랜지션 시간이 지날 때까지 반복
				break;

			float t				= elapsedTime / c_transitionTime;
			m_canvasGroup.alpha	= Mathf.Lerp(1, 0, t);
			m_trans.localScale	= Vector3.one * Mathf.Lerp(1, c_transitionSclae, Mathf.Pow(t, c_transitionTimeT));

			yield return null;
		}

		// 루프 종료 후에는 최종 설정값 한번 더 적용
		m_canvasGroup.alpha		= 0;

		IsTransitioning	= false;						// 트랜지션 끝
		m_transCompleteCB();							// 트랜지션 완료 콜백 호출
	}

	/// <summary>
	/// 뒤로 가는 트랜지션
	/// </summary>
	/// <returns></returns>
	IEnumerator TransCo_Back()
	{
		IsTransitioning			= true;					// 트랜지션 시작
		TransitionOverriadable	= true;
		float startTime			= Time.realtimeSinceStartup;
		float startAlpha		= m_canvasGroup.alpha;

		while (true)
		{
			float elapsedTime	= Time.realtimeSinceStartup - startTime;
			if (elapsedTime > c_transitionTime)			// 트랜지션 시간이 지날 때까지 반복
				break;

			float t				= elapsedTime / c_transitionTime;
			m_canvasGroup.alpha	= Mathf.Lerp(startAlpha, 0.1f, t);

			yield return null;
		}

		// 루프 종료 후에는 최종 설정값 한번 더 적용
		m_canvasGroup.alpha		= 0.1f;

		IsTransitioning	= false;						// 트랜지션 끝
		//m_transCompleteCB();							// 트랜지션 완료 콜백 호출
	}

	/// <summary>
	/// 앞으로 오는 트랜지션
	/// </summary>
	/// <returns></returns>
	IEnumerator TransCo_Forth()
	{
		IsTransitioning			= true;					// 트랜지션 시작
		TransitionOverriadable	= true;
		float startTime			= Time.realtimeSinceStartup;
		float startAlpha		= m_canvasGroup.alpha;

		while (true)
		{
			float elapsedTime	= Time.realtimeSinceStartup - startTime;
			if (elapsedTime > c_transitionTime)			// 트랜지션 시간이 지날 때까지 반복
				break;

			float t				= elapsedTime / c_transitionTime;
			m_canvasGroup.alpha	= Mathf.Lerp(startAlpha, 1, t);

			yield return null;
		}

		// 루프 종료 후에는 최종 설정값 한번 더 적용
		m_canvasGroup.alpha		= 1;

		IsTransitioning	= false;						// 트랜지션 끝
		//m_transCompleteCB();							// 트랜지션 완료 콜백 호출
	}



	//====================================================================

	// 다이얼로그 상태 관련

	/// <summary>
	/// 현재 열려있는지
	/// </summary>
	public bool IsOpened { get; private set; }

	/// <summary>
	/// 입력 가능한 상태 세팅
	/// </summary>
	public bool Interactable
	{
		get { return m_canvasGroup.interactable; }
		set { m_canvasGroup.interactable = value; }
	}

	private float ZOrder
	{
		get { return m_trans.anchoredPosition3D.z; }
		set
		{
			var pos	= m_trans.anchoredPosition3D;
			pos.z	= value;
			m_trans.anchoredPosition3D = pos;
		}
	}


	/// <summary>
	/// 다이얼로그 열기
	/// </summary>
	bool Open_real()
	{
		if (IsOpened)								// 이미 열린 경우엔 무시
			return false;
		IsOpened = true;

		gameObject.SetActive(true);					// 게임 오브젝트 활성화
		m_canvasGroup.interactable = false;			// 입력 받지 못하게

		OnOpen();									// 시작 이벤트

		StartTransition(TransitionType.Open, () =>	// 트랜지션 종료 이벤트
			{
				m_canvasGroup.interactable = true;	// 입력 다시 가능하게
				OnOpenComplete();
			});

		return true;
	}

	/// <summary>
	/// 다이얼로그 닫기
	/// </summary>
	/// <returns></returns>
	bool Close_real()
	{
		if (!IsOpened || IsTransitioning && !TransitionOverriadable)	// 닫혀있는 경우거나 Transition중인 경우엔 리턴
			return false;

		m_canvasGroup.interactable = false;			// 입력 받지 못하게

		OnClose();									// 시작 콜백
		StartTransition(TransitionType.Close, () =>
			{
				IsOpened	= false;				// 플래그 설정
				OnCloseComplete();					// 종료 콜백
				gameObject.SetActive(false);		// 비활성화
			});

		return true;
	}

	void ToBack()
	{
		StartTransition(TransitionType.Back, null);
	}

	void ToForth()
	{
		StartTransition(TransitionType.Forth, null);
	}

	/// <summary>
	/// 자기 자신 열기
	/// </summary>
	protected System.Action OpenSelf { get; private set; }
	/// <summary>
	/// 자기 자신 닫기
	/// </summary>
	protected System.Action CloseSelf { get; private set; }


	//====================================================================



	// 유저 구현 메서드

	/// <summary>
	/// OverlayUI에 등록될 때 딱 한 번만 실행됨
	/// </summary>
	protected virtual void Initialize() { }

	/// <summary>
	/// 창을 새로 열 때마다 맨 먼저 호출됨
	/// </summary>
	protected virtual void Reset() { }

	protected virtual void OnOpen() { }
	protected virtual void OnOpenComplete() { }
	protected virtual void OnClose() { }
	protected virtual void OnCloseComplete() { }
}
