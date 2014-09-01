using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// 스냅샷 리스트. 컨트롤을 위한 요소들도 같이 시퀀스에 포함되어있음.
/// </summary>
public sealed class FSNSnapshotSequence
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
		public FSNInGameSetting.FlowDirection BackDirection
		{
			get { return FSNInGameSetting.GetOppositeFlowDirection(FlowDirection); }
		}

		/// <summary>
		/// 방향마다 정보 (다음 세그먼트 등)
		/// </summary>
		FlowInfo[] Flows { get; set; }

		/// <summary>
		/// 파라미터
		/// </summary>
		public object Parameter;


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
	}


	//====================================================================


	private List<Segment>	m_segments	= new List<Segment>();



	
	void Add(Segment newSeg)
	{
		newSeg.Index	= m_segments.Count;
		m_segments.Add(newSeg);
	}

	Segment Get(int index)
	{
		return m_segments[index];
	}

	//====================================================================

	/// <summary>
	/// FSNSnapshotSequence 를 스크립트 시퀀스에서 생성해낸다
	/// </summary>
	public static class Builder
	{
		
	}

	/// <summary>
	/// 스크립트 순회. 엔진에서 실행할 때 이 인스턴스를 사용한다.
	/// </summary>
	public class Traveler
	{
		Segment m_current;					// 현재 세그먼트


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

