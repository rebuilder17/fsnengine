using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// 스냅샷 리스트. 컨트롤을 위한 요소들도 같이 시퀀스에 포함되어있음.
/// </summary>
public sealed partial class FSNSnapshotSequence
{
	/// <summary>
	/// 스냅샷 종류 (컨트롤과 관련)
	/// </summary>
	public enum FlowType
	{
		Normal,			// 일반
		UserChoice,		// 선택지
		Load,			// Fade-in 트랜지션이 완전히 끝나면 다른 시퀀스 로드
	}

	/// <summary>
	/// 리스트 단위. 스냅샷 1개에 부가적인 컨트롤 명령어가 붙어있는 형태.
	/// </summary>
	class Segment
	{
		/// <summary>
		/// 각 방향마다 정보
		/// </summary>
		public struct FlowInfo
		{
			public Segment Linked;			// 연결된 세그먼트

			// TODO : 조건식 같은 것이 필요하다면 이쪽에
		}

		//====================================================================

		public int	Index;		// 인덱스
		public FSNSnapshot snapshot;	// 스냅샷 (본체)


		// TODO : 컨트롤 명령어

		/// <summary>
		/// 세그먼트 종류 (컨트롤 관련)
		/// </summary>
		public FlowType Type	{ get; set; }

		/// <summary>
		/// 보통의 경우, 진행 방향
		/// </summary>
		public FSNInGameSetting.FlowDirection FlowDirection;

		/// <summary>
		/// 반대방향
		/// </summary>
		public FSNInGameSetting.FlowDirection BackDirection;

		/// <summary>
		/// 방향마다 정보 (다음 세그먼트 등)
		/// </summary>
		FlowInfo[] Flows { get; set; }

		/// <summary>
		/// 파라미터
		/// </summary>
		public object Parameter	= null;


		//=======================================================================

		public Segment()
		{
			Flows	= new FlowInfo[4];
		}

		/// <summary>
		/// 해당 방향으로 움직일 수 있는지 여부 
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public bool CanFlowTo(FSNInGameSetting.FlowDirection dir)
		{
			return Flows[(int)dir].Linked != null;
		}
		/// <summary>
		/// 해당 방향의 Segment 구하기
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public Segment GetLinked(FSNInGameSetting.FlowDirection dir)
		{
			return Flows[(int)dir].Linked;
		}
		/// <summary>
		/// 연결 설정
		/// </summary>
		/// <param name="dir"></param>
		public void SetFlow(FSNInGameSetting.FlowDirection dir, FlowInfo flow)
		{
			Flows[(int)dir]	= flow;
		}
	}


	//====================================================================


	private List<Segment>	m_segments	= new List<Segment>();


	int Length
	{
		get { return m_segments.Count; }
	}
	
	void Add(Segment newSeg)
	{
		newSeg.Index	= m_segments.Count;
		m_segments.Add(newSeg);
	}

	Segment Get(int index)
	{
		return m_segments[index];
	}

	Segment LastSegment
	{
		get { return m_segments[m_segments.Count - 1]; }
	}

	//====================================================================

	/// <summary>
	/// FSNSnapshotSequence 를 스크립트 시퀀스에서 생성해낸다
	/// </summary>
	public static class Builder
	{
		/// <summary>
		/// 해석중에 사용하는 정보들
		/// </summary>
		class State
		{
			public FSNSequence				sequence;			// 처리중인 스크립트
			public int						segIndex;			// 세그먼트 인덱스

			public FSNInGameSetting.Chain	settings;			// 처리중 해석된 세팅

			FSNInGameSetting				m_frozenSetting;
			bool							m_settingIsDirty	= true;

			/// <summary>
			/// 현재 세팅을 고정시킨 세팅값 얻어오기
			/// </summary>
			public FSNInGameSetting			FrozenSetting
			{
				get
				{
					if(m_settingIsDirty)
					{
						m_frozenSetting		= settings.Freeze();
						m_settingIsDirty	= false;
					}

					return m_frozenSetting;
				}
			}

			/// <summary>
			/// 세팅 변경되었음 체크
			/// </summary>
			public void SetSettingDirty()
			{
				m_settingIsDirty	= true;
			}

			/// <summary>
			/// 복제
			/// </summary>
			/// <returns></returns>
			public State Clone()
			{
				State newState		= new State();

				newState.sequence	= sequence;
				newState.segIndex	= segIndex;
				settings			= settings.CloneEntireChain();

				newState.m_settingIsDirty	= true;	// 안전을 위해 무조건 Dirty 상태로 둔다

				return newState;
			}
		}

		/// <summary>
		/// 한 Snapshot 에 필요한 Module 콜들을 한번에 모았다가 처리하기 위한 헬퍼 클래스
		/// </summary>
		class ModuleCallQueue
		{
			Dictionary<IFSNProcessModule, List<FSNProcessModuleCallParam>>	m_moduleCallTable	= new Dictionary<IFSNProcessModule, List<FSNProcessModuleCallParam>>();

			/// <summary>
			/// 등록된 콜 모두 초기화
			/// </summary>
			public void ClearCall()
			{
				foreach(var callList in m_moduleCallTable.Values)
				{
					callList.Clear();
				}
			}

			/// <summary>
			/// 콜 추가
			/// </summary>
			/// <param name="module"></param>
			/// <param name="segment"></param>
			public void AddCall(IFSNProcessModule module, FSNSequence.Segment segment, IInGameSetting setting)
			{
				if(!m_moduleCallTable.ContainsKey(module))
					m_moduleCallTable[module]	= new List<FSNProcessModuleCallParam>();

				m_moduleCallTable[module].Add(new FSNProcessModuleCallParam() { segment = segment, setting = setting });
			}

			/// <summary>
			/// 쌓인 콜 모두 실행 후 Clear
			/// </summary>
			public void ProcessCall(FSNSnapshot prevSnapshot, FSNSnapshot curSnapshot)
			{
				foreach(var pair in m_moduleCallTable)
				{
					if(pair.Value.Count > 0)							// * 실질적인 call이 있을 때만 실행
					{
						var module		= pair.Key;
						var prevLayer	= prevSnapshot.GetLayer(module.LayerID) ?? FSNSnapshot.Layer.Empty;

						var callInfo	= pair.Value;

						var newLayer	= module.GenerateNextLayerImage(prevLayer, callInfo.ToArray());

						curSnapshot.SetLayer(module.LayerID, newLayer);
					}
				}

				ClearCall();
			}
		}

		//=======================================================================

		/// <summary>
		///  FSNSequence를 해석하여 FSNSnapshotSequence를 만든다.
		/// </summary>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static FSNSnapshotSequence BuildSnapshotSequence(FSNSequence sequence)
		{
			FSNSnapshotSequence	snapshotSeq		= new FSNSnapshotSequence();
			State				builderState	= new State();

			// State 초기 세팅

			builderState.sequence				= sequence;
			builderState.segIndex				= 0;
			builderState.settings				= new FSNInGameSetting.Chain(FSNEngine.Instance.InGameSetting);


			// 시작 Snapshot 만들기

			FSNSnapshot startSnapshot			= new FSNSnapshot();
			startSnapshot.LinkToForward			= true;
			startSnapshot.InGameSetting			= builderState.settings;

			Segment startSegment				= new Segment();
			startSegment.Type					= FlowType.Normal;
			startSegment.snapshot				= startSnapshot;

			snapshotSeq.Add(startSegment);


			// 빌드 시작
			ProcessSnapshotBuild(builderState, snapshotSeq, 0);

			return snapshotSeq;
		}

		/// <summary>
		/// FSNSequence 해석. 분기마다 builderState를 복제해야 하므로 새로 호출된다.
		/// </summary>
		/// <param name="builderState"></param>
		/// <param name="snapshotSeq"></param>
		/// <param name="bs">이번 콜에서 생성할 시퀀스 이전의 스냅샷 인덱스</param>
		/// <returns>이번 콜에서 생성한 시퀀스들 중 제일 첫번째 것. 다른 시퀀스 흐름과 연결할 때 사용 가능</returns>
		static Segment ProcessSnapshotBuild(State bs, FSNSnapshotSequence snapshotSeq, int prevSnapshotIndex)
		{
			ModuleCallQueue moduleCalls	= new ModuleCallQueue();

			Segment	sseg;														// 새로 생성할 Segment/Snapshot
			FSNSnapshot sshot;
			NewSnapshot(out sseg, out sshot);

			var firstSeg		= sseg;											// 최초 스냅샷
			var prevCallSeg		= snapshotSeq.Get(prevSnapshotIndex);			// 이번에 생성할 시퀀스의 이전 스냅샷

			var lastSeg			= prevCallSeg;									// 바로 이전에 처리한 스냅샷 (최초 루프시에는 실행 이전의 마지막 스냅샷과 동일)
			//


			// 스크립트 Sequence가 끝나거나 특정 명령을 만날 때까지 반복

			bool keepProcess	= true;
			while(keepProcess)
			{
				var curSeg	= bs.sequence[bs.segIndex];							// 현재 명령어 (Sequence 세그먼트)

				switch(curSeg.type)												// 명령어 타입 체크 후 분기
				{
				//////////////////////////////////////////////////////////////
				case FSNSequence.Segment.Type.Setting:							// ** 세팅
				{
					var settingSeg		= curSeg as Segments.SettingSegment;

					if(settingSeg.settingMethod == Segments.SettingSegment.SettingMethod.Pop)	// * 세팅 pop
					{
						if(bs.settings.ParentChain != null)
						{
							bs.settings	= bs.settings.ParentChain;
						}
						else
						{
							Debug.LogError("Cannot pop settings anymore - there is no settings pushed");
						}
					}
					else
					{
						if(settingSeg.settingMethod == Segments.SettingSegment.SettingMethod.Push)	// * Push일 경우에는 새 Chain을 생성한다
						{
							bs.settings	= new FSNInGameSetting.Chain(bs.settings);
						}

						foreach(var settingPair in settingSeg.RawSettingTable)	// Setting 설정
						{
							// TODO
						}
					}

					bs.SetSettingDirty();										// 세팅 변경됨 플래그 올리기
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNSequence.Segment.Type.Text:								// ** 텍스트
				{
					// TODO : 텍스트는 이렇게 그냥 해볼 수도 있겠지만, 다른 레이어들을 어떻게 처리하느냐에 따라서
					// 모았다가 한번에 Period 타이밍에 처리하는 식으로 바꿔야할수도...

					var module			= FSNEngine.Instance.GetModule(FSNEngine.ModuleType.Text) as IFSNProcessModule;

					moduleCalls.AddCall(module, curSeg, bs.FrozenSetting);		// 해당 명령 저장
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNSequence.Segment.Type.Period:							// ** Period
				{
					var periodSeg		= curSeg as Segments.PeriodSegment;

					sshot.InGameSetting	= bs.FrozenSetting;						// 현재까지의 세팅 적용 (굳힌거 사용)

					moduleCalls.ProcessCall(lastSeg.snapshot, sshot);			// 지금까지 모인 모듈 콜 집행하기

					if(lastSeg.Type != FlowType.UserChoice)						// 이전 스냅샷이 선택지가 아니면 바로 연결하기 (선택지일 경우 호출자가 결정하게 함... <- 확실하진 않음)
					{
						LinkSnapshots(lastSeg, sseg);

						if(periodSeg.isChaining)								// Chaining 옵션이 켜져있을 경우
						{
							lastSeg.snapshot.LinkToForward	= true;
							sseg.snapshot.LinkToBackward	= true;
						}
					}

					snapshotSeq.Add(sseg);										// 현재 스냅샷을 시퀀스에 추가
					lastSeg	= sseg;
					NewSnapshot(out sseg, out sshot);							// 새 스냅샷 인스턴스 준비
				}
				break;
				/////////////////////////////////////////////////////////////
				}


				//
				bs.segIndex++;													// 다음 명령어 인덱스

				if(bs.segIndex >= bs.sequence.Length)							// * Sequence 가 끝났다면 루프 종료
				{
					keepProcess	= false;
				}
			}

			return firstSeg;
		}

		/// <summary>
		/// 새 Segment/Snapshot 세팅 (숏컷)
		/// </summary>
		/// <param name="newSeg"></param>
		/// <param name="newSnapshot"></param>
		static void NewSnapshot(out Segment newSeg, out FSNSnapshot newSnapshot)
		{
			newSnapshot		= new FSNSnapshot();
			newSeg			= new Segment();
			newSeg.snapshot	= newSnapshot;
		}

		/// <summary>
		/// 스냅샷 단순 연결
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		static void LinkSnapshots(Segment prev, Segment next)
		{
			var swipeDir		= next.snapshot.InGameSetting.CurrentFlowDirection;		// 다음에 올 시퀀스의 설정값으로 링크 방향을 정한다
			var backDir			= next.snapshot.InGameSetting.BackwardFlowDirection;

			prev.FlowDirection	= swipeDir;
			next.BackDirection	= backDir;

			prev.SetFlow(swipeDir, new Segment.FlowInfo() { Linked = next });
			next.SetFlow(backDir, new Segment.FlowInfo() { Linked = prev });
		}
	}

	/// <summary>
	/// 스크립트 순회. 엔진에서 실행할 때 이 인스턴스를 사용한다.
	/// </summary>
	public class Traveler
	{
		Segment m_current;					// 현재 세그먼트


		/// <summary>
		/// 외부에서 함부로 생성 못하게
		/// </summary>
		private Traveler() { }

		/// <summary>
		/// Traveler 생성
		/// </summary>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static Traveler GenerateFrom(FSNSnapshotSequence sequence)
		{
			var newtr		= new Traveler();
			newtr.m_current	= sequence.Get(0);				// 첫번째 세그먼트부터 시작

			return newtr;
		}

		/// <summary>
		/// 현재 스냅샷
		/// </summary>
		public FSNSnapshot Current { get { return m_current.snapshot; } }

		/// <summary>
		/// 다음 snapshot 구하기
		/// </summary>
		public FSNSnapshot Next
		{
			get
			{
				if(m_current.Type == FlowType.Normal)		// 선택지 타입이 아닌 경우만 리턴
					return GetLinkedSnapshot(m_current.FlowDirection);

				return null;
			}
		}
		/// <summary>
		/// 이전 스냅샷 구하기
		/// </summary>
		public FSNSnapshot Prev
		{
			get
			{
				return GetLinkedSnapshot(m_current.BackDirection);
			}
		}
		/// <summary>
		/// 해당 방향 스냅샷 구하기
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public FSNSnapshot GetLinkedSnapshot(FSNInGameSetting.FlowDirection dir)
		{
			if(m_current.CanFlowTo(dir))
				return m_current.GetLinked(dir).snapshot;
			
			return null;
		}


		public FSNSnapshot TravelForward()
		{
			if(m_current.Type == FlowType.Normal)		// 선택지 타입이 아닌 경우만 진행
			{
				return TravelTo(m_current.FlowDirection);
			}

			return null;
		}

		public FSNSnapshot TravelBackward()
		{
			return TravelTo(m_current.BackDirection);
		}

		/// <summary>
		/// 해당 방향으로 진행하여 현재 상태 바꾸기.
		/// </summary>
		/// <param name="dir"></param>
		/// <returns>진행 가능할 경우, 해당 방향의 snapshot. 아니면 null</returns>
		public FSNSnapshot TravelTo(FSNInGameSetting.FlowDirection dir)
		{
			FSNSnapshot next	= null;
			if(m_current.CanFlowTo(dir))					// 해당 방향으로 진행 가능한 경우만 변경
			{
				m_current	= m_current.GetLinked(dir);
				next		= m_current.snapshot;
			}
			return next;
		}
	}
}

