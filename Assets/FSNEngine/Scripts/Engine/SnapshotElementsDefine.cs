﻿using UnityEngine;
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

		public override void CopyDataTo(Text to)
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
	public abstract class ObjectBase<SelfT> : FSNSnapshot.Element<SelfT>
		where SelfT : ObjectBase<SelfT>, new()
	{
		/// <summary>
		/// 오브젝트의 움직임 계산 상태
		/// </summary>
		public enum State
		{
			NotCalculated	= 0,	// 아직 보간 계산 적용되지 않음.

			Calculated,				// 보간 적용됨
			MotionKey,				// 키 오브젝트. 이 오브젝트의 좌표값을 기준으로 움직임 보간 실행
		}

		/// <summary>
		/// 움직임 상태
		/// </summary>
		public State	motionState	= State.NotCalculated;

		/// <summary>
		/// Final State 설정값이 세팅된 적이 있는지
		/// </summary>
		public bool		finalStateSet	= false;


		public override void CopyDataTo(SelfT to)
		{
			base.CopyDataTo(to);
			to.finalStateSet	= finalStateSet;
		}

		/// <summary>
		/// 두 elem 사이의 t 비율에 해당하는 값으로 세팅.
		/// 주 : motionState는 세팅하지 않는다. 필요하다면 직접 해줘야함
		/// </summary>
		/// <param name="elem1"></param>
		/// <param name="elem2"></param>
		/// <param name="t"></param>
		public virtual void LerpBetweenElems(ObjectBase<SelfT> elem1, ObjectBase<SelfT> elem2, float t)
		{
			Position	= Vector3.Lerp(elem1.Position, elem2.Position, t);
			Color		= Color.Lerp(elem1.Color, elem2.Color, t);
			Alpha		= Mathf.Lerp(elem1.Alpha, elem2.Alpha, t);

			// TODO : Scale, Rotate
		}
	}

	/// <summary>
	/// 일반적인 이미지
	/// </summary>
	public class Image : ObjectBase<Image>
	{
		public Texture2D	texture;

		public override void CopyDataTo(Image to)
		{
			base.CopyDataTo(to);
			to.texture	= texture;
		}
	}
}
