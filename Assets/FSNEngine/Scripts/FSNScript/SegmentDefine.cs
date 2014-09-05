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

	public class SettingSegment : FSNSequence.Segment
	{
		public override Type type
		{
			get { return Type.Setting; }
		}
		//

		public enum SettingMethod
		{
			Push,			// 세팅 적용
			Pop,			// 이전 세팅으로 복구
			Set,			// 강제 세팅. 세팅 스택을 오버라이드하는 건 아니라서 Pop하면 현재 상태와 함께 제거되는 것임.
		}

		/// <summary>
		/// 세팅 종류
		/// </summary>
		public SettingMethod settingMethod;

		/// <summary>
		/// 세팅
		/// </summary>
		public FSNInGameSetting setting	= null;
	}
}


