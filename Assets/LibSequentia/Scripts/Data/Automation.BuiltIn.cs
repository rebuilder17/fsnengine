using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LibSequentia.Data
{
	public partial class Automation
	{
		// Static Members

		static Dictionary<TargetParam, float>	s_defaultValues;		// 오토메이션 기본값 모음
		static TargetParam []					s_targetParamEnumList;	// TargetParam enum의 값들


		/// <summary>
		/// built-in 오토메이션 : 볼륨 페이드 인
		/// </summary>
		public static Automation LinearFadeIn { get; private set; }
		/// <summary>
		/// built-in 오토메이션 : 볼륨 페이드 아웃
		/// </summary>
		public static Automation LinearFadeOut { get; private set; }
		public static Automation InstantMute { get; private set; }
		public static Automation InstantUnMute { get; private set; }


		static Automation()
		{
			// 파라미터 enum 쪼개기
			var enumValues			= System.Enum.GetValues(typeof(TargetParam));
			var enumCount			= enumValues.Length;
			s_targetParamEnumList	= new TargetParam[enumCount];
			for (int i = 0; i < enumCount; i++)
			{
				s_targetParamEnumList[i]	= (TargetParam)enumValues.GetValue(i);
			}


			// 기본값 설정

			s_defaultValues						= new Dictionary<TargetParam, float>();
			s_defaultValues[TargetParam.Volume]	=  1.0f;
			s_defaultValues[TargetParam.LowCut]	=  0.0f;



			// * 검증 코드. TargetParam마다 모두 기본값이 있어야한다.
			foreach(var key in s_targetParamEnumList)
			{
				if(!s_defaultValues.ContainsKey(key))
				{
					throw new System.InvalidProgramException("default value for TargetParam." + System.Enum.GetName(typeof(TargetParam), key) + "is not set.");
				}
			}

			// 빌트인 오토메이션 만들기

			var auto_fadein			= new Automation();
			auto_fadein.targetParam	= TargetParam.Volume;
			auto_fadein.AddPoint(0, 0);
			auto_fadein.AddPoint(1, 1);
			LinearFadeIn			= auto_fadein;

			var auto_fadeout		= new Automation();
			auto_fadeout.targetParam	= TargetParam.Volume;
			auto_fadeout.AddPoint(0, 1);
			auto_fadeout.AddPoint(1, 0);
			LinearFadeOut			= auto_fadeout;

			var auto_instmute		= new Automation();
			auto_instmute.targetParam	= TargetParam.Volume;
			auto_instmute.AddPoint(0, 0);
			InstantMute				= auto_instmute;

			var auto_instunmute		= new Automation();
			auto_instunmute.targetParam	= TargetParam.Volume;
			auto_instunmute.AddPoint(0, 1);
			InstantUnMute			= auto_instunmute;
		}

		/// <summary>
		/// TargetParam enum의 모든 값들
		/// </summary>
		public static TargetParam[] targetParamEnumValues
		{
			get { return s_targetParamEnumList; }
		}

		/// <summary>
		/// 각 TargetParam의 기본값을 구한다.
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public static float GetDefaultValue(TargetParam param)
		{
			return s_defaultValues[param];
		}
	}
}
