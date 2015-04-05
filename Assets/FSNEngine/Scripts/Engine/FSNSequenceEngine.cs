using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// FSNSequence를 해석하고 실행하는 메인 오브젝트
/// </summary>
public class FSNSequenceEngine : MonoBehaviour
{
	// Constants

	const float			c_maxSwipeToTransitionRatio	= 0.5f;				// 최대로 Swipe했을 때의 Transition 진행율
	//const float			c_maxSwipeToTransitionRatio	= 0.0f;				// 최대로 Swipe했을 때의 Transition 진행율


	// Members

	FSNSnapshotSequence						m_snapshotSeq;				// 스냅샷 시퀀스
	FSNSnapshotSequence.Traveler			m_snapshotTraveler;			// 스냅샷 트라벨러
	SortedDictionary<int, IFSNLayerModule>	m_layerModules;				// 레이어 모듈

	float									m_swipeAvailableTime;		// swipe가 가능해지는 시간. (이 시간이 지나야 가능해짐)
	bool									m_lastSwipeWasBackward;		// 최근에 한 swipe가 반대방향이었는지.


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
	/// Swipe 가능한지 여부
	/// </summary>
	public bool CanSwipe
	{
		get
		{
			return Time.time > m_swipeAvailableTime;
		}
	}

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
	}

	void Update()
	{
		if(m_snapshotTraveler != null && CanSwipe)					// 로드된 snapshot이 있고, swipe 가능할 시 (= idle 상황)
		{
			var curSnapshot	= m_snapshotTraveler.Current;

			// 정방향 혹은 역방향으로 진행했는데 해당 방향으로 연결된 snapshot이 있다면 바로 transition을 건다
			if(curSnapshot.NonstopToForward && !m_lastSwipeWasBackward)
			{
				var nextFlowDir	= m_snapshotTraveler.Next.InGameSetting.CurrentFlowDirection;
				//FullSwipe(curSnapshot.InGameSetting.CurrentFlowDirection, 0f);
				FullSwipe(nextFlowDir, 0f);
			}
			else if(curSnapshot.NonstopToBackward && m_lastSwipeWasBackward)
			{
				FullSwipe(curSnapshot.InGameSetting.BackwardFlowDirection, 0f);
			}
		}
	}

	/// <summary>
	/// Snapshot 시퀀스 시작
	/// </summary>
	/// <param name="sequence"></param>
	public void StartSnapshotSequence(FSNSnapshotSequence sequence, int snapshotIndex = 0)
	{
		FSNDebug.currentProcessingScript	= sequence.OriginalScriptPath;	// 디버깅 정보 설정

		m_snapshotSeq		= sequence;
		m_snapshotTraveler	= FSNSnapshotSequence.Traveler.GenerateFrom(sequence, snapshotIndex);
		m_snapshotTraveler.ScriptLoadRequested += OnScriptNeedToBeLoaded;	// 스크립트 로딩 이벤트 등록

		if(CurrentSession == null)
			CurrentSession	= new FSNSession();								// 진행중이던 세션이 없을 시엔 새 세션 생성. 세이브한 세션을 로드하는 경우라면 다시 여기에 덮어써야한다.


		var session	= CurrentSession;

		foreach (var pair in sequence.ScriptHeader.FlagDeclarations)		// 플래그 선언
		{
			if (!session.FlagIsDeclared(pair.Key))							// 아직 선언되지 않은 경우만 세팅
			{
				bool value	= string.IsNullOrEmpty(pair.Value)?
						false : FSNUtils.StringToValue<bool>(pair.Value);	// 초기값까지 선언한 경우 값 해독, 아니면 기본값 false
				session.SetFlagValue(pair.Key, value, true);
			}	
		}

		foreach (var pair in sequence.ScriptHeader.ValueDeclarations)		// 값 선언
		{
			if (!session.ValueIsDeclared(pair.Key))							// 아직 선언되지 않은 경우만 세팅
			{
				float value	= string.IsNullOrEmpty(pair.Value)?
						0 : FSNUtils.StringToValue<float>(pair.Value);		// 초기값까지 선언한 경우 값 해독, 아니면 기본값 false
				session.SetNumberValue(pair.Key, value, true);
			}
		}

		SaveToCurrentSession();												// 현재 스크립트 진행 정보를 세션쪽에 기록
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
				//Debug.Log("Layer processing");
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
			//Debug.Log("delay time : " + transTime);

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
		FSNEngine.Instance.RunScript(scriptFile);
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

		FSNEngine.Instance.RunScript(session.ScriptName);					// 스크립트 로드
		CurrentSession		= session;										// 로딩시의 세션 정보를 사용하도록 지정
		
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
				var newLayer	= curshot.GetLayer(layerID);

				if (newLayer != null && !newLayer.IsEmpty)
				{
					float curtt	= module.StartTransition(newLayer, curshot.InGameSetting, 0, false);	// 트랜지션

					if (transTime < curtt)									// 제일 긴 트랜지션 시간 추적
						transTime = curtt;
				}
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
