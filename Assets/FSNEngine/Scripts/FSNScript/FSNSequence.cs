using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 해석되어 메모리 상에 올라온 스크립트 시퀀스. 스크립트 파일을 parsing하면 나오는 결과물.
/// 스크립트 실행은 이 오브젝트를 참조하여 수행한다
/// </summary>
public class FSNSequence
{
	/// <summary>
	/// 명령어 단위.
	/// </summary>
	public abstract class Segment
	{
		/// <summary>
		/// 명령어 종류, 앞/뒤 캐싱 등에 활용
		/// </summary>
		public enum Type
		{
			Period,			// Period (대기상태)
			Label,			// 라벨

			Text,			// 텍스트
			Object,			// 오브젝트
			Setting,		// 세팅 변경 등

			Control,		// 엔진 컨트롤
		}

		/// <summary>
		/// segment 타입
		/// </summary>
		public abstract Type type { get; }

		/// <summary>
		/// 정확한 명령어 이름 (스크립트 상에서)
		/// </summary>
		public string name			{ protected set; get; }

		/// <summary>
		/// 열기/닫기 세그먼트일 경우, 페어가 되는 다른 세그먼트
		/// </summary>
		public Segment pairSegment	{ protected set; get; }
	}



	// Members

	List<Segment>			m_segments;				// Sequence에 포함된 모든 segments
	Dictionary<string, int>	m_labelToIndex;			// Label => list의 Index로

	public FSNSequence()
	{
		m_segments = new List<Segment>();
	}

	//=====================================================================================




	//=====================================================================================

	#region TEST CODE

	public static FSNSequence GenerateTestSequence()
	{
		var sequence		= new FSNSequence();
		sequence.m_segments	= new List<Segment>();//임시, 나중에는 필요없어질것
		Segments.TextSegment	tempTextSeg;
		Segments.PeriodSegment	periodSeg	= new Segments.PeriodSegment();

		tempTextSeg			= new Segments.TextSegment();
		tempTextSeg.Text	= "테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg			= new Segments.TextSegment();
		tempTextSeg.Text	= "테스트 02";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg			= new Segments.TextSegment();
		tempTextSeg.Text	= "테스트 03";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		return sequence;
	}

	#endregion
}
