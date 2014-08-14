using UnityEngine;
using System.Collections;


/// <summary>
/// Period 세그먼트
/// </summary>
public class PeriodSegment : FSNSequence.Segment
{
	public override Type type
	{
		get { return Type.Period; }
	}
}

/// <summary>
/// 텍스트 세그먼트
/// </summary>
public class TextSegment : FSNSequence.Segment
{
	public override Type type
	{
		get { return Type.Text; }
	}

	public string Text;
}


