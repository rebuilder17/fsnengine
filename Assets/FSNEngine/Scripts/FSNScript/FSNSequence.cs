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
		/// 명령어 종류
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

	/// <summary>
	/// Sequence 길이
	/// </summary>
	public int Length
	{
		get { return m_segments.Count; }
	}

	/// <summary>
	/// 해당 위치의 Segment 구하기
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Segment GetSegment(int index)
	{
		return m_segments[index];
	}

	public Segment this[int index]
	{
		get { return GetSegment(index); }
	}

	//=====================================================================================

	#region TEST CODE

	public static FSNSequence GenerateTestSequence()
	{
		var sequence		= new FSNSequence();
		sequence.m_segments	= new List<Segment>();//임시, 나중에는 필요없어질것
		Segments.Text	tempTextSeg;
		Segments.Period	periodSeg	= new Segments.Period();

		tempTextSeg			= new Segments.Text();
		tempTextSeg.text	= "테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		var settingSeg1		= new Segments.Setting();
		settingSeg1.settingMethod				= Segments.Setting.SettingMethod.Push;
		settingSeg1["CurrentFlowDirection"]		= FSNInGameSetting.FlowDirection.Right;
		settingSeg1["BackwardFlowDirection"]	= FSNInGameSetting.FlowDirection.Left;
		settingSeg1["FontSize"]					= 32f;
		sequence.m_segments.Add(settingSeg1);

		tempTextSeg			= new Segments.Text();
		tempTextSeg.text	= "테스트 02";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		var settingSeg2		= new Segments.Setting();
		settingSeg2.settingMethod				= Segments.Setting.SettingMethod.Pop;
		sequence.m_segments.Add(settingSeg2);

		tempTextSeg			= new Segments.Text();
		tempTextSeg.text	= "테스트 03";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		var clearTextSeg		= new Segments.Text();
		clearTextSeg.textType	= Segments.Text.TextType.Clear;
		sequence.m_segments.Add(clearTextSeg);

		tempTextSeg			= new Segments.Text();
		tempTextSeg.text	= "테스트 04";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		return sequence;
	}

	#endregion
}
