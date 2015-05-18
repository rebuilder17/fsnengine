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
		/// 함수 호출 정보
		/// </summary>
		public struct CallFuncInfo
		{
			public string funcname;
			public string [] param;
		}

		/// <summary>
		/// 각 방향마다 정보
		/// </summary>
		public struct FlowInfo
		{
			/// <summary>
			/// 조건부 링크
			/// </summary>
			public struct ConditionLink
			{
				public CallFuncInfo [] funcinfo;	// 링크 조건 함수 콜 (and 관계)
				public Segment Linked;				// 조건이 모두 참일 때 이 세그먼트로 점프
			}

			public ConditionLink [] ConditionedLinks;	// 조건부 세그먼트 점프 (or 관계, 하나씩 체크하고 true가 있다면 바로 점프)
			public Segment Linked;					// 연결된 세그먼트 (조건부가 없거나 모두 false일 때 이쪽으로)
		}

		//====================================================================

		public int	Index;		// 인덱스
		public FSNSnapshot snapshot;	// 스냅샷 (본체)


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
		/// 단방향인지 (역방향 불가)
		/// </summary>
		public bool OneWay = false;

		/// <summary>
		/// 파라미터
		/// </summary>
		public object Parameter	= null;

		/// <summary>
		/// 함수 콜. 해당 세그먼트로 이동할 때 (swipe 애니메이션과는 무관하게. TravelTo) 호출됨
		/// </summary>
		public CallFuncInfo [] FunctionCalls;


		//=======================================================================

		public Segment()
		{
			Flows			= new FlowInfo[4];
			FlowDirection	= FSNInGameSetting.FlowDirection.None;
			BackDirection	= FSNInGameSetting.FlowDirection.None;
		}

		/// <summary>
		/// 해당 방향으로 움직일 수 있는지 여부. 조건부 점프는 제외된다. (따로 체크해야함)
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

		public void SetDirectFlow(FSNInGameSetting.FlowDirection dir, Segment linked)
		{
			Flows[(int)dir].Linked	= linked;
		}

		public void SetConditionFlow(FSNInGameSetting.FlowDirection dir, Segment.FlowInfo.ConditionLink [] links)
		{
			Flows[(int)dir].ConditionedLinks	= links;
		}

		/// <summary>
		/// 해당 방향의 조건 함수를 실행하여 모두 true일 때만 해당 방향 segment 리턴. 아니면 null
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public Segment CheckAndGetConditionFlow(FSNInGameSetting.FlowDirection dir)
		{
			var condlinks	= Flows[(int)dir].ConditionedLinks;
			if (condlinks == null)							// 조건부 링크가 없다면 null
				return null;

			var engine	= FSNEngine.Instance;

			foreach(var link in condlinks)					// 링크마다 반복
			{
				Segment foundSeg	= link.Linked;			// 찾아낸 segment. 일단은 조건을 만족하는 것으로 가정
				foreach(var funcinfo in link.funcinfo)
				{
					if (!engine.ScriptUnityCallBool(funcinfo.funcname, funcinfo.param))	// 조건이 하나라도 false가 나왔다면 비교 중단, 찾아낸 것은 null로
					{
						foundSeg	= null;
						break;
					}
				}

				if(foundSeg != null)						// 해당 조건이 모두 만족되었다면 이 segment 리턴
				{
					return foundSeg;
				}
			}

			return null;
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

	/// <summary>
	/// 원본 스크립트 경로
	/// </summary>
	public string OriginalScriptPath	{ get; private set; }

	/// <summary>
	/// 스크립트에서 생성한 해시 키
	/// </summary>
	public string ScriptHashKey			{ get; private set; }


	/// <summary>
	/// 스크립트에서 읽은 헤더
	/// </summary>
	public FSNScriptHeader ScriptHeader { get; private set; }


	//====================================================================

	

	/// <summary>
	/// 스크립트 순회. 엔진에서 실행할 때 이 인스턴스를 사용한다.
	/// </summary>
	public class Traveler
	{
		FSNSnapshotSequence	m_currentSeq;
		Segment				m_current;									// 현재 세그먼트
		Segment []			m_cachedConditionLinks	= new Segment[4];	// 조건부 링크 캐시


		/// <summary>
		/// 스크립트 로딩 요청 이벤트
		/// </summary>
		public event System.Action<string>	ScriptLoadRequested;

		/// <summary>
		/// 외부에서 함부로 생성 못하게
		/// </summary>
		private Traveler() { }

		/// <summary>
		/// Traveler 생성
		/// </summary>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static Traveler GenerateFrom(FSNSnapshotSequence sequence, int snapshotIndex = 0)
		{
			var newtr		= new Traveler();
			newtr.m_current	= sequence.Get(snapshotIndex);
			newtr.m_currentSeq	= sequence;

			return newtr;
		}

		/// <summary>
		/// 현재 스냅샷
		/// </summary>
		public FSNSnapshot Current { get { return m_current.snapshot; } }

		/// <summary>
		/// 현재 스냅샷 인덱스
		/// </summary>
		public int CurrentIndex { get { return m_current.Index; } }

		/// <summary>
		/// 현재 스냅샷이 선택지 페이지인지
		/// </summary>
		public bool CurrentIsSwipeOption
		{
			get { return m_current.Type == FlowType.UserChoice; }
		}

		/// <summary>
		/// 다음 snapshot 구하기
		/// </summary>
		public FSNSnapshot Next
		{
			get
			{
				if (m_current.Type == FlowType.Normal)		// 선택지 타입이 아닌 경우만 리턴
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
		/// 다음 스냅샷으로 넘어가기 위한 정방향
		/// </summary>
		public FSNInGameSetting.FlowDirection SwipeForwardDirection
		{
			get
			{
				return m_current.FlowDirection;
			}
		}

		/// <summary>
		/// 해당 방향 스냅샷 구하기
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public FSNSnapshot GetLinkedSnapshot(FSNInGameSetting.FlowDirection dir)
		{
			var linked			= GetLinkedToDir(dir);
			if (linked != null)
				return linked.snapshot;
			
			return null;
		}


		public FSNSnapshot TravelForward()
		{
			if(m_current.Type != FlowType.UserChoice)	// 선택지 타입이 아닌 경우만 진행
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
		/// 캐싱된 조건부 링크 등등을 종합해서 해당 방향으로 링크되는 Segment를 구한다. 없을 경우 null
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		private FSNSnapshotSequence.Segment GetLinkedToDir(FSNInGameSetting.FlowDirection dir)
		{
			var condlink	= m_cachedConditionLinks[(int)dir];	// 조건부 링크 구하기
			if (condlink != null)
				return condlink;

			if(m_current.CanFlowTo(dir))						// 일반 링크 구하기
			{
				return m_current.GetLinked(dir);
			}
			else
			{
				return null;									// 링크 없음
			}
		}

		/// <summary>
		/// 해당 방향으로 진행하여 현재 상태 바꾸기. 덤으로 해당 snapshot에 지정된 함수들도  호출한다.
		/// </summary>
		/// <param name="dir"></param>
		/// <returns>진행 가능할 경우, 해당 방향의 snapshot. 아니면 null</returns>
		public FSNSnapshot TravelTo(FSNInGameSetting.FlowDirection dir)
		{
			FSNSnapshot next	= null;
			var linked			= GetLinkedToDir(dir);
			if(linked != null)								// 해당 방향으로 진행 가능한 경우만 변경
			{
				m_current	= linked;
				next		= linked.snapshot;

				ExecuteSnapshotFunctions();					// 함수 실행

				if(m_current.Type == FlowType.Load)			// 스크립트 로딩 이벤트
				{
					ScriptLoadRequested(m_current.Parameter as string);
				}
			}
			return next;
		}

		/// <summary>
		/// 특정 인덱스로 바로 점프
		/// NOTE : Load 기능과 같이 사용하기 위한 메서드임. 일반적인 용도로는 쓰지 않음.
		/// </summary>
		/// <param name="index"></param>
		public void JumpToIndex(int index)
		{
			m_current	= m_currentSeq.Get(index);
		}

		/// <summary>
		/// 현재 스냅샷의 함수들 실행. 이후, 조건 체크 뒤 각 방향의 조건부 스냅샷 링크를 캐싱하는 역할도 수행한다.
		/// TravelTo 에서 자동으로 실행한다. 이외의 곳 (예 : 최초 실행, 로딩 후 바로 점프) 에서는 직접 호출해줘야한다.
		/// </summary>
		/// <param name="conditionCheckOnly">true일 경우 일반 함수는 실행하지 않고 조건부 점프 체크만 수행한다</param>
		public void ExecuteSnapshotFunctions(bool conditionCheckOnly = false)
		{
			// 일반 함수 실행

			if(!conditionCheckOnly)
			{
				var functions	= m_current.FunctionCalls;
				if (functions != null)
				{
					int funcCount	= functions.Length;
					for (int i = 0; i < funcCount; i++)
					{
						var funcInfo	= functions[i];
						FSNEngine.Instance.ScriptUnityCallVoid(funcInfo.funcname, funcInfo.param);
					}
				}
			}
			
			// 조건부 체크

			for(int i = 0; i < 4; i++)
			{
				m_cachedConditionLinks[i]	= m_current.CheckAndGetConditionFlow((FSNInGameSetting.FlowDirection)i);
			}
		}
	}
}

