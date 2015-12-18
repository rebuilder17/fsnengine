using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// FSNSequence를 해석하고 실행하는 메인 오브젝트
/// </summary>
public class FSNSequenceEngine : MonoBehaviour
{
	/// <summary>
	/// 진행 속도 조절
	/// NOTE : 현재는 정해진 배속을 강제로 지정하도록 설계되어있음. 따라서 게임 속도를 오버라이드하는 다른 코드가 게임에 존재할 경우 충돌이 생길 것이므로 주의.
	/// </summary>
	public interface FlowSpeedControl
	{
		/// <summary>
		/// 다음 Idle까지만 빠르게
		/// </summary>
		void SetFastUntilNextIdle();

		/// <summary>
		/// 계속 빠르게 스킵
		/// </summary>
		void SetFastSkipping();

		/// <summary>
		/// 정상 속도로 스킵
		/// </summary>
		void SetNormalSkipping();

		/// <summary>
		/// 스킵 상태 해제 (다음 idle때 중지된다)
		/// </summary>
		void ClearSkipping();
	}

	class FlowSpeedControlImpl : FlowSpeedControl
	{
		// Constants

		const float			c_timeRatio_normal				= 1.0f;
		//const float			c_timeRatio_skipUntilNextIdle	= 16.0f;
		const float			c_timeRatio_skipUntilNextIdle	= 4.0f;
		const float			c_timeRatio_fastskip			= 8.0f;


		// Members

		FSNSequenceEngine	m_seqengine;
		bool				m_fastUntilNextIdle;			// 다음 idle까지만 빠르게
		bool				m_keepSkipping;					// 계속 스킵하는지 (자동진행)


		public FlowSpeedControlImpl(FSNSequenceEngine seqengine)
		{
			m_seqengine	= seqengine;
		}

		public void update()
		{
			if (m_seqengine.CanSwipe)							// 엔진이 idle일 때만 반응
			{
				var forward			= m_seqengine.m_snapshotTraveler.SwipeForwardDirection;
				bool needClearSkip	= false;

				if(!m_keepSkipping && m_fastUntilNextIdle)		// 계속 스킵하는 상태는 아니고 다음 idle까지만 스킵하는 경우인지
				{
					needClearSkip		= true;
				}
				else if(m_keepSkipping &&
					(m_seqengine.m_snapshotTraveler.CurrentIsSwipeOption 
					|| forward == FSNInGameSetting.FlowDirection.None 
					|| !m_seqengine.SwipeDirectionAvailable(forward)))
					// 계속 스킵해야하는 상태인데 선택지를 만난 경우, 혹은 더이상 진행할 수 없는 경우
				{
					needClearSkip		= true;
				}

				if (needClearSkip)								// 스킵 상태 해제
				{
					m_keepSkipping		= false;
					m_fastUntilNextIdle	= false;
					Time.timeScale		= c_timeRatio_normal;
				}
				
				if(m_keepSkipping || m_fastUntilNextIdle)		// 스킵 해제 조건을 처리하고 나서도 어쨌든 현재 스킵해야하는 상황이면...진행
				{
					m_seqengine.FullSwipe(forward, 0);
				}
			}
		}

		//==========================================================================

		// 다음 Idle까지만 빠르게
		public void SetFastUntilNextIdle()
		{
			Time.timeScale		= c_timeRatio_skipUntilNextIdle;
			m_fastUntilNextIdle	= true;
		}

		/// <summary>
		/// 계속 빠르게 스킵
		/// </summary>
		public void SetFastSkipping()
		{
			Time.timeScale		= c_timeRatio_fastskip;
			m_fastUntilNextIdle	= true;
			m_keepSkipping		= true;
		}

		/// <summary>
		/// 정상 속도로 스킵
		/// </summary>
		public void SetNormalSkipping()
		{
			m_keepSkipping		= true;
		}

		/// <summary>
		/// 스킵 상태 해제 (다음 idle때 중지된다)
		/// </summary>
		public void ClearSkipping()
		{
			m_keepSkipping		= false;
		}
	}


	// Constants

	const float			c_maxSwipeToTransitionRatio	= 0.5f;				// 최대로 Swipe했을 때의 Transition 진행율


	// Members

	FSNSnapshotSequence						m_snapshotSeq;				// 스냅샷 시퀀스
	FSNSnapshotSequence.Traveler			m_snapshotTraveler;			// 스냅샷 트라벨러
	SortedDictionary<int, IFSNLayerModule>	m_layerModules;				// 레이어 모듈

	float									m_swipeAvailableTime;		// swipe가 가능해지는 시간. (이 시간이 지나야 가능해짐)
	bool									m_lastSwipeWasBackward;		// 최근에 한 swipe가 반대방향이었는지.

	FlowSpeedControlImpl					m_flowSpeedControl;			//


	/// <summary>
	/// 현재 세션
	/// </summary>
	public FSNSession CurrentSession
	{
		get;
		private set;
	}

	/// <summary>
	/// 현재 InGame 세팅
	/// </summary>
	public IInGameSetting InGameSetting
	{
		get
		{
			if(m_snapshotTraveler != null)
			{
				return m_snapshotTraveler.Current.InGameSetting;
			}
			else
			{
				return FSNInGameSetting.DefaultInGameSetting;
			}
		}
	}

	/// <summary>
	/// 레이어 ID로 모듈 찾기
	/// </summary>
	/// <param name="layerID"></param>
	/// <returns></returns>
	public IFSNLayerModule GetModuleByLayerID(int layerID)
	{
		return m_layerModules[layerID];
	}

	/// <summary>
	/// Swipe 가능한지 여부 ( = 현재 트랜지션이 모두 끝났는지)
	/// </summary>
	public bool CanSwipe
	{
		get
		{
			return Time.time > m_swipeAvailableTime;
		}
	}

	public FlowSpeedControl flowSpeedControl { get { return m_flowSpeedControl; } }

	/// <summary>
	/// 초기화
	/// </summary>
	public virtual void Initialize()
	{
		m_layerModules	= new SortedDictionary<int, IFSNLayerModule>();

		foreach(var module in FSNEngine.Instance.AllModules)		// Module 중에서 LayerModule들을 찾는다
		{
			var layerModule	= module as IFSNLayerModule;
			if(layerModule != null)
			{
				m_layerModules[layerModule.LayerID]	= layerModule;
			}
		}

		m_flowSpeedControl	= new FlowSpeedControlImpl(this);		// FlowSpeedControl 초기화
	}

	void Update()
	{
		if(m_snapshotTraveler != null && CanSwipe
			&& !FSNEngine.Instance.ControlSystem.enginePaused)		// 로드된 snapshot이 있고, swipe 가능할 시 (= idle 상황), pause도 아닐 경우
		{
			var curSnapshot	= m_snapshotTraveler.Current;

			// 정방향 혹은 역방향으로 진행했는데 해당 방향으로 연결된 snapshot이 있다면 바로 transition을 건다
			if(curSnapshot.NonstopToForward && !m_lastSwipeWasBackward)
			{
				var nextFlowDir	= m_snapshotTraveler.Next.InGameSetting.CurrentFlowDirection;
				FullSwipe(nextFlowDir, 0f);
			}
			else if(curSnapshot.NonstopToBackward && m_lastSwipeWasBackward)
			{
				FullSwipe(curSnapshot.InGameSetting.BackwardFlowDirection, 0f);
			}
			else if(curSnapshot.ForceBackward)						// 강제로 역방향으로 돌리는 경우
			{
				FullSwipe(curSnapshot.InGameSetting.BackwardFlowDirection, 0f);
			}
		}

		m_flowSpeedControl.update();								// flowspeedcontrol 업데이트
	}

	/// <summary>
	/// 새 게임시 Session 오브젝트를 준비한다.
	/// </summary>
	public void PrepareNewSession()
	{
		CurrentSession  = new FSNSession();
	}

	/// <summary>
	/// Snapshot 시퀀스 시작
	/// </summary>
	/// <param name="sequence"></param>
	/// <param name="overwriteScriptInfoToSession">현재 스크립트 정보를 세션에 덮어써야 하는 경우 true</param>
	public void StartSnapshotSequence(FSNSnapshotSequence sequence, bool overwriteScriptInfoToSession, int snapshotIndex = 0)
	{
		FSNDebug.currentProcessingScript	= sequence.OriginalScriptPath;	// 디버깅 정보 설정

		m_snapshotSeq		= sequence;
		m_snapshotTraveler	= FSNSnapshotSequence.Traveler.GenerateFrom(sequence, snapshotIndex);
		m_snapshotTraveler.ScriptLoadRequested += OnScriptNeedToBeLoaded;	// 스크립트 로딩 이벤트 등록
		
		if (overwriteScriptInfoToSession)
			SaveToCurrentSession();											// 새 게임을 시작하는 경우에만 현재 스크립트 진행 정보를 세션쪽에 기록
	}

	/// <summary>
	/// 아직 완전히 넘기지 못한 swipe에 관한 것
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="ratio">0 ~ 1 사이, 1은 완전히 넘긴 것 (그러나 완전히 넘길 때 처리는 다른 곳에서 한다)</param>
	/// <param name="backward">반대 방향으로 swipe하는 경우인지 여부</param>
	public void PartialSwipe(FSNInGameSetting.FlowDirection direction, float ratio)
	{
		if(!CanSwipe)														// Swipe 불가능한 상태면 리턴
			return;

		var nextshot	= m_snapshotTraveler.GetLinkedSnapshot(direction);
		if(nextshot != null)												// * 넘길 수 있는 경우만 처리
		{
			//Debug.Log("Link available");
			var curshot	= m_snapshotTraveler.Current;

			foreach(var module in m_layerModules.Values)				// 현재 로드된 모든 LayerModule 에 맞는 레이어를 찾아 각각 처리한다
			{
				int layerID		= module.LayerID;
				var oldLayer	= curshot.GetLayer(layerID)	?? FSNSnapshot.Layer.Empty;
				var newLayer	= nextshot.GetLayer(layerID) ?? FSNSnapshot.Layer.Empty;

				if(oldLayer.IsEmpty && newLayer.IsEmpty)				// * 둘 다 비어있으면 아무것도 하지 않는다
					continue;
				
				bool isBackward	= InGameSetting.BackwardFlowDirection == direction;
				float trRatio	= ratio * c_maxSwipeToTransitionRatio;
				module.OldElementOnlyTransition(newLayer, trRatio, isBackward);	// 트랜지션
			}
		}
	}

	/// <summary>
	/// 완전히 넘기는 Swipe 처리
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="transitionStartRatio">트랜지션 애니메이션을 어느 지점부터 시작할지 여부. 0은 처음부터, 1은 종료부분. 기본값은 c_maxSwipeToTransitionRatio</param>
	public void FullSwipe(FSNInGameSetting.FlowDirection direction, float transitionStartRatio = c_maxSwipeToTransitionRatio)
	{
		if(!CanSwipe)														// Swipe 불가능한 상태면 리턴
			return;

		var nextshot	= m_snapshotTraveler.GetLinkedSnapshot(direction);
		if(nextshot != null)												// * 실제로 가능한 경우만 처리
		{
			var curshot	= m_snapshotTraveler.Current;

			float transTime		= 0f;										// 트랜지션 시간

			// FIX : setting에 backward 방향이 등록되어있더라도 DisableBackward 플래그가 서있다면 역방향으로 따지지 않는다.
			bool isBackward		= !curshot.DisableBackward && InGameSetting.BackwardFlowDirection == direction;

			foreach(var module in m_layerModules.Values)					// 현재 로드된 모든 LayerModule 에 맞는 레이어를 찾아 각각 처리한다
			{
				int layerID		= module.LayerID;
				var oldLayer	= curshot.GetLayer(layerID)	?? FSNSnapshot.Layer.Empty;
				var newLayer	= nextshot.GetLayer(layerID) ?? FSNSnapshot.Layer.Empty;

				if(oldLayer.IsEmpty && newLayer.IsEmpty)					// * 둘 다 비어있으면 아무것도 하지 않는다
					continue;

				float curtt		= module.StartTransition(newLayer, nextshot.InGameSetting, transitionStartRatio, isBackward);	// 트랜지션

				if(transTime < curtt)										// 제일 긴 트랜지션 시간 추적
					transTime = curtt;
			}

			m_lastSwipeWasBackward	= isBackward;							// swipe 방향성 보관해두기 (연결된 snapshot 처리에 사용)
			transTime				+= nextshot.AfterSwipeDelay;			// swipe 시간에 스냅샷에 지정된 딜레이 시간까지 더하기
			m_swipeAvailableTime	= Time.time + transTime;				// 현재 시간 + 트랜지션에 걸리는 시간 뒤에 swipe가 가능해짐
			
			m_snapshotTraveler.TravelTo(direction);							// 해당 방향으로 넘기기
			CurrentSession.SnapshotIndex	= m_snapshotTraveler.CurrentIndex;	// Session 정보 업데이트 (스크립트는 변하지 않았으므로 snapshot index만 바꿔주면 된다)
		}
	}

	/// <summary>
	/// 특정 방향으로 진행 가능한지 여부
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public bool SwipeDirectionAvailable(FSNInGameSetting.FlowDirection direction)
	{
		return CanSwipe && m_snapshotTraveler.GetLinkedSnapshot(direction) != null;
	}

	/// <summary>
	/// 스크립트 로딩되어야만하는 경우
	/// </summary>
	/// <param name="scriptFile"></param>
	void OnScriptNeedToBeLoaded(string scriptFile)
	{
		FSNEngine.Instance.RunScript(scriptFile, FSNEngine.ExecuteType.LoadFromOtherScript);
		m_snapshotTraveler.ExecuteSnapshotFunctions();						// 첫째 스냅샷의 함수 실행은 자동으로 되지 않으므로, 수동으로 호출
	}

	/// <summary>
	/// Session에 기록된 값을 통해 스크립트 상태 로딩
	/// </summary>
	/// <param name="session"></param>
	/// <returns>정상적으로 로드했다면 true, 만약 스크립트 버전이 달라서 첫 번재 snapshot부터 보여줘야하는 경우 false</returns>
	public bool LoadFromSession(FSNSession session)
	{
		bool fullSuccess	= false;

		CurrentSession      = session;                                      // 로딩시의 세션 정보를 사용하도록 지정
		FSNEngine.Instance.RunScript(session.ScriptName, FSNEngine.ExecuteType.LoadFromSession);// 스크립트 로드
		
		if (m_snapshotSeq.ScriptHashKey == session.ScriptHashKey)			// 저장 당시의 hashkey가 일치한다면, 저장 당시의 snapshot index로 점프
		{
			m_snapshotTraveler.JumpToIndex(session.SnapshotIndex);
			m_snapshotTraveler.ExecuteSnapshotFunctions();					// 함수 실행이 자동으로 되지 않으므로, 수동으로 호출
			fullSuccess		= true;

			// 실제로 트랜지션

			var curshot			= m_snapshotTraveler.Current;
			float transTime		= 0f;										// 트랜지션 시간

			foreach(var module in m_layerModules.Values)					// 현재 로드된 모든 LayerModule 에 맞는 레이어를 찾아 각각 처리한다
			{
				int layerID		= module.LayerID;
				var newLayer	= curshot.GetLayer(layerID) ?? FSNSnapshot.Layer.Empty;

				// 로딩 직후의 상황이므로 이전 화면과는 어떤 연관성도 없는 것으로 간주, 빈 레이어도 강제로 트랜지션을 걸어준다.

				float curtt	= module.StartTransition(newLayer, curshot.InGameSetting, 0, false);	// 트랜지션

				if (transTime < curtt)									// 제일 긴 트랜지션 시간 추적
					transTime = curtt;
			}
			m_lastSwipeWasBackward	= false;								// swipe 방향성, 정방향으로 취급하기
			m_swipeAvailableTime	= Time.time + transTime;				// 현재 시간 + 트랜지션에 걸리는 시간 뒤에 swipe가 가능해짐
		}
		else
		{
			Debug.LogWarningFormat("[FSNSequenceEngine] Script version differs ({0}), so started from the begining", session.ScriptName);
		}

		return fullSuccess;
	}

	/// <summary>
	/// 현재 스크립트 진행 상태를 session에 기록
	/// </summary>
	/// <param name="session"></param>
	public void SaveToCurrentSession()
	{
		var session				= CurrentSession;
		session.ScriptName		= m_snapshotSeq.OriginalScriptPath;
		session.ScriptHashKey	= m_snapshotSeq.ScriptHashKey;
		session.SnapshotIndex	= m_snapshotTraveler.CurrentIndex;
	}

	/// <summary>
	/// 스크립트의 조건부 링크 등을 다시 체크해야할 경우 호출
	/// </summary>
	public void UpdateScriptConditions()
	{
		m_snapshotTraveler.ExecuteSnapshotFunctions(true);
	}
}
