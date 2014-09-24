using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

		/// <summary>
		/// 유저 입력을 기다리는지(false), 바로 뒷내용으로 넘어가는지(true)
		/// </summary>
		public bool	isChaining;
	}

	/// <summary>
	/// Label 세그먼트
	/// </summary>
	public class LabelSegment : FSNSequence.Segment
	{
		public override FSNSequence.Segment.Type type
		{
			get { return Type.Label; }
		}

		/// <summary>
		/// 라벨 종류.
		/// Hard : Label 진입 전에 모든 내용 Clean됨. Label 이전으로 되돌아갈 수 없음 (Default)
		/// Soft : Clean하지 않음. 해석시 각 브랜치에서 하나의 label로 흐름이 합쳐지는 경우, 실제 흐름을 통합하지 않고 각각 중복해서 해석하게 됨. 따라서 해석 중 무한 루프를 방지하기 위해서 이미 지나쳐온 Soft Label로는 GOTO로 점프할 수 없음 (GOTO 루프 불가)
		/// </summary>
		public enum LabelType
		{
			Hard,
			Soft,
		}

		/// <summary>
		/// Label 타입
		/// </summary>
		public LabelType labelType	= LabelType.Hard;
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


		private Dictionary<string, object>	m_settingDict	= null;

		/// <summary>
		/// 세팅 딕셔너리 구하기
		/// </summary>
		public Dictionary<string, object> RawSettingTable
		{
			get { return m_settingDict; }
		}

		/// <summary>
		/// 세팅
		/// </summary>
		public object this[string propname]
		{
			get
			{
				object value	= null;
				if(m_settingDict != null)
				{
					m_settingDict.TryGetValue(propname, out value);
				}

				return value;
			}

			set
			{
				if(m_settingDict == null)
					m_settingDict	= new Dictionary<string, object>();

				m_settingDict[propname]	= value;
			}
		}
	}
}


