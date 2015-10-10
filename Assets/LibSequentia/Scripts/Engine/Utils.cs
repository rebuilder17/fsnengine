using UnityEngine;
using System.Collections;

namespace LibSequentia.Engine
{
	public static class Utils
	{
		/// <summary>
		/// 문자열을 enum으로 변환
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="str"></param>
		/// <returns></returns>
		public static T parseEnum<T>(string str)
		{
			return (T)System.Enum.Parse(typeof(T), str, true);
		}

		/// <summary>
		/// json 형식으로 되어있는 오토메이션 정보 해석
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Data.Automation ParseAutomationFromJSON(JSONObject json)
		{
			var auto	= new Data.Automation();

			var param			= json.GetField("param").str;
			auto.targetParam	= parseEnum<Data.Automation.TargetParam>(param);

			json.GetField("data", (pointarr) =>
				{
					var list	= pointarr.list;
					var count	= list.Count;
					for (int i = 0; i < count; i++ )
					{
						var p	= list[i];
						var t	= p.GetField("t").f;
						var v	= p.GetField("v").f;
						auto.AddPoint(t, v);
					}
				});

			return auto;
		}
	}
}

