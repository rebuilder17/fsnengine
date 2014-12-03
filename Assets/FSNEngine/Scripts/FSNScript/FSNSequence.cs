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
		m_segments		= new List<Segment>();
		m_labelToIndex	= new Dictionary<string, int>();
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

	/// <summary>
	/// 라벨 지정
	/// </summary>
	/// <param name="index"></param>
	/// <param name="labelName"></param>
	public void SetLabel(int index, string labelName)
	{
		m_labelToIndex[labelName]	= index;
	}

	/// <summary>
	/// 가장 마지막 인덱스를 라벨로 지정
	/// </summary>
	/// <param name="labelName"></param>
	public void SetLastSegmentAsLabel(string labelName)
	{
		SetLabel(m_segments.Count - 1, labelName);
	}

	/// <summary>
	/// 가장 마지막 인덱스에 있는 세그먼트가 Label일 경우 등록
	/// </summary>
	public void RegisterLabelSegment()
	{
		var label	= m_segments[m_segments.Count - 1] as Segments.Label;
		if(label == null)
		{
			Debug.LogError("the segment just have been added is not a Label type.");
		}
		else
		{
			SetLastSegmentAsLabel(label.labelName);
		}
	}

	/// <summary>
	/// 라벨의 인덱스 구하기
	/// </summary>
	/// <param name="labelName"></param>
	/// <returns></returns>
	public int GetIndexOfLabel(string labelName)
	{
		if (!m_labelToIndex.ContainsKey(labelName))
			Debug.LogError("No label named " + labelName);

		return m_labelToIndex[labelName];
	}



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

		//var settingSeg1		= new Segments.Setting();
		//settingSeg1.settingMethod				= Segments.Setting.SettingMethod.Push;
		//settingSeg1["CurrentFlowDirection"]		= FSNInGameSetting.FlowDirection.Right;
		//settingSeg1["BackwardFlowDirection"]	= FSNInGameSetting.FlowDirection.Left;
		//settingSeg1["FontSize"]					= 32f;
		//sequence.m_segments.Add(settingSeg1);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 02";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		//var settingSeg2		= new Segments.Setting();
		//settingSeg2.settingMethod				= Segments.Setting.SettingMethod.Pop;
		//sequence.m_segments.Add(settingSeg2);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 03";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		//var clearTextSeg		= new Segments.Text();
		//clearTextSeg.textType	= Segments.Text.TextType.Clear;
		//sequence.m_segments.Add(clearTextSeg);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 04";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		var optionSeg		= new Segments.Text();
		optionSeg.textType	= Segments.Text.TextType.Options;
		optionSeg.text		= "선택지?";
		optionSeg.optionTexts	= new string[4];
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Up]	= "위";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Down]	= "아래";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Left]	= "왼쪽";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Right]= "오른쪽";
		sequence.m_segments.Add(optionSeg);

		var userChoiceSeg			= new Segments.Control();
		userChoiceSeg.controlType	= Segments.Control.ControlType.SwipeOption;
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Up,		"label_up");
		//userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Down,	"label_down");
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Left,	"label_left");
		//userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Right,	"label_right");
		sequence.m_segments.Add(userChoiceSeg);

		sequence.m_segments.Add(periodSeg);

		var blockSeg			= new Segments.Control();
		blockSeg.controlType	= Segments.Control.ControlType.Block;
		sequence.m_segments.Add(blockSeg);	// 현재 흐름에서는 선택지를 끝으로 더이상 진행할 곳이 없으므로, block으로 막는다


		// 선택지 : 위
		var label_up			= new Segments.Label();
		label_up.labelName		= "label_up";
		sequence.m_segments.Add(label_up);
		sequence.RegisterLabelSegment();

		var lastOptionText		= new Segments.Text();
		lastOptionText.textType = Segments.Text.TextType.LastOption;
		sequence.m_segments.Add(lastOptionText);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// 선택지 : 왼쪽
		var label_left			= new Segments.Label();
		label_left.labelName	= "label_left";
		sequence.m_segments.Add(label_left);
		sequence.RegisterLabelSegment();

		sequence.m_segments.Add(lastOptionText);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK



		return sequence;
	}

	#endregion
}
