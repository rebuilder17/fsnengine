using UnityEngine;
using System.Collections;


namespace SnapshotElems
{
	/// <summary>
	/// 텍스트 Element
	/// </summary>
	public class Text : FSNSnapshot.Element<Text>
	{
		public enum Type
		{
			Normal,				// 일반 텍스트
			OptionTexts,		// 선택지 타이밍에 나온 텍스트
			LastOption,			// 선택지 선택 후 나온 텍스트
		}

		public string	text;
		public float	fontSize;

		public Type		type		= Type.Normal;

		/// <summary>
		/// 선택지 텍스트일 경우, 방향 지정
		/// </summary>
		public FSNInGameSetting.FlowDirection optionDir	= FSNInGameSetting.FlowDirection.None;

		protected override void CopyDataTo(Text to)
		{
			base.CopyDataTo(to);
			to.text		= text;
			to.fontSize	= fontSize;

			to.type		= type;
			to.optionDir	= optionDir;
		}
	}

	/// <summary>
	/// 이미지 관련 (베이스)
	/// </summary>
	public class ImageObjBase : FSNSnapshot.Element<ImageObjBase>
	{

	}

	/// <summary>
	/// 일반적인 이미지
	/// </summary>
	public class Image : ImageObjBase
	{
		public Texture2D	texture;

		protected override void CopyDataTo(ImageObjBase to)
		{
			base.CopyDataTo(to);
			var toImg		= to as Image;
			toImg.texture	= texture;
		}
	}
}
