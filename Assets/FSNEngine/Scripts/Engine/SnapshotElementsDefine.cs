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

		protected override void CopyDataTo(Text to)
		{
			base.CopyDataTo(to);
			to.text	= text;
		}
	}
}
