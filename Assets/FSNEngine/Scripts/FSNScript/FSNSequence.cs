using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 해석되어 메모리 상에 올라온 스크립트 시퀀스. 스크립트 파일을 parsing하면 나오는 결과물.
/// 스크립트 실행은 이 오브젝트를 참조하여 수행한다
/// </summary>
public class FSNScriptSequence
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

	public FSNScriptSequence()
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


	//--------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 스크립트 파서. 스크립트를 읽어 FSNScriptSequence를 생성해낸다.
	/// </summary>
	public static class Parser
	{
		// Constants

		// 기본 전제 : 줄 앞에 나오는 토큰들은 모두 1글자

		const string	c_token_Comment		= "#";			// 주석
		const string	c_token_SoftLabel	= ":";			// 레이블 (soft)
		const string	c_token_HardLabel	= "!";			// 레이블 (hard)
		const string	c_token_Command		= "/";			// 명령문
		const string	c_token_Period		= ".";			// period

		const string	c_token_LineConcat	= "//";			// 줄 붙이기 (텍스트 끝)


		/// <summary>
		/// 문자열으로 스크립트 파싱
		/// </summary>
		/// <param name="scriptData"></param>
		/// <returns></returns>
		public static FSNScriptSequence FromString(string scriptData)
		{
			var sequence	= new FSNScriptSequence();
			var strstream	= new System.IO.StringReader(scriptData);

			string line		= null;
			while ((line = strstream.ReadLine()) != null)				// 줄 단위로 읽는다.
			{
				if (line.Length == 0)									// * 빈 줄은 스루
					continue;

				Segment newSeg	= null;									// 이번에 새로 생성한 세그먼트
				var pretoken	= line.Substring(0, 1);
				switch (pretoken)										// 앞쪽 토큰으로 명령 구분
				{
					case c_token_Comment:								// * 주석
						break;

					case c_token_Command:								// * 명령
						break;

					case c_token_HardLabel:								// * hard label

						break;

					case c_token_SoftLabel:								// * soft label

						break;

					case c_token_Period:								// * period
						if(line.Length == 1)							// 오직 . 한글자일 때만 인정
						{

						}
						break;

					default:											// * 아무 토큰도 없음 : 텍스트

						break;
				}

				if(newSeg != null)										// 새로 생성된 시퀀스가 있다면 추가
				{
					sequence.m_segments.Add(newSeg);
				}
			}

			return sequence;
		}
	}

	//--------------------------------------------------------------------------------------------

	#region TEST CODE

	public static FSNScriptSequence GenerateTestSequence()
	{
		var sequence		= new FSNScriptSequence();
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
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Down,	"label_down");
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

		var chainPeriodSeg		= new Segments.Period();	// 뒤로 바로 넘어가지는 period
		chainPeriodSeg.isChaining	= true;
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		// 주 : 현재 Jump 를 처리하는 순서 때문에 period가 적용되고 완성된 Snapshot에서 적용된다.
		// 즉 GOTO를 제대로 쓰기 위해서는 period 보다 앞쪽에 배치해야함.
		var gotoSeg				= new Segments.Control();
		gotoSeg.controlType		= Segments.Control.ControlType.Goto;
		gotoSeg.SetGotoData("label_jumptest");
		sequence.m_segments.Add(gotoSeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 02";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		var label_jumptest		= new Segments.Label();
		label_jumptest.labelName= "label_jumptest";
		sequence.m_segments.Add(label_jumptest);
		sequence.RegisterLabelSegment();

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 03 (label_jumptest)";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// 선택지 : 왼쪽
		var label_left			= new Segments.Label();
		label_left.labelName	= "label_left";
		sequence.m_segments.Add(label_left);
		sequence.RegisterLabelSegment();

		sequence.m_segments.Add(lastOptionText);
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 02";
		sequence.m_segments.Add(tempTextSeg);

		var reverseGotoSeg		= new Segments.Control();
		reverseGotoSeg.controlType	= Segments.Control.ControlType.ReverseGoto;
		reverseGotoSeg.SetReverseGotoData("label_reverse");
		sequence.m_segments.Add(reverseGotoSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 03";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// ReverseGoto 테스트용
		var label_reverse		= new Segments.Label();
		label_reverse.labelName	= "label_reverse";
		sequence.m_segments.Add(label_reverse);
		sequence.RegisterLabelSegment();

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "you can't go back!";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "go ahead and you'll be fine.";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// 선택지 : 아래쪽, 역방향 오버라이드가 제대로 되는지 테스트하기 위함.
		var label_down			= new Segments.Label();
		label_down.labelName	= "label_down";
		sequence.m_segments.Add(label_down);
		sequence.RegisterLabelSegment();

		sequence.m_segments.Add(lastOptionText);
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 02, oneway";
		sequence.m_segments.Add(tempTextSeg);

		var onewaySeg			= new Segments.Control();	// ONEWAY
		onewaySeg.controlType	= Segments.Control.ControlType.Oneway;
		sequence.m_segments.Add(onewaySeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 03";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK


		return sequence;
	}

	#endregion
}
