using UnityEngine;
using System.Collections;

namespace Segments
{
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

		/// <summary>
		/// 한번에 표시되는 텍스트
		/// </summary>
		public string Text;
	}
}


