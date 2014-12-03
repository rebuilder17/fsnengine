using UnityEngine;
using System.Collections;


namespace SnapshotElems
{
	/// <summary>
	/// 텍스트 Element
	/// </summary>
	public class Text : FSNSnapshot.Element<Text>
	{
		public string text;
		public float fontSize;

		/// <summary>
		/// 선택지 텍스트일 경우, 방향 지정
		/// </summary>
		FSNInGameSetting.FlowDirection optionDir	= FSNInGameSetting.FlowDirection.None;

		protected override void CopyDataTo(Text to)
		{
			base.CopyDataTo(to);
			to.text		= text;
			to.fontSize	= fontSize;
		}
	}
}
