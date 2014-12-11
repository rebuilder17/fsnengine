using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 각종 유틸리티 함수 모음
/// </summary>
public static class FSNUtils
{
	/// <summary>
	/// ICollection의 원소를 모두 복사한 Array를 만들어낸다
	/// </summary>
	/// <param name="enumerable"></param>
	/// <returns></returns>
	public static T[] MakeArray<T>(ICollection<T> collection)
	{
		T[] array	= new T[collection.Count];
		collection.CopyTo(array, 0);
		return array;
	}

	public static string RemoveAllWhiteSpaces(string str)
	{
		return System.Text.RegularExpressions.Regex.Replace(str, "[ \t\n\r]", "");	// 공백 모두 제거
	}

	/// <summary>
	/// 문자열을 type에 맞는 값으로 변환.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="strval"></param>
	/// <returns></returns>
	public static object StringToValue(System.Type type, string strval)
	{
		StringToValueFunc convFunc;
		s_stringToValFuncTable.TryGetValue(type, out convFunc);
		if(convFunc == null)
		{
			Debug.LogError("[StringToValue] Cannot conversion type " + type.ToString());
			return null;
		}
		else
		{
			return convFunc(strval);
		}
	}

	private delegate object StringToValueFunc(string strval);
	/// <summary>
	/// 문자열 변환 함수 딕셔너리
	/// </summary>
	private static readonly Dictionary<System.Type, StringToValueFunc> s_stringToValFuncTable
		= new Dictionary<System.Type,StringToValueFunc>()
		{
			{typeof(string), (strval) =>
			{
				return strval;
			}},
			{typeof(int), (strval) =>
			{
				return int.Parse(strval);
			}},
			{typeof(float), (strval) =>
			{
				return float.Parse(strval);
			}},
			{typeof(bool), (strval) =>
			{
				switch(strval.ToUpper())
				{
					case "TRUE":
					case "YES":
					case "1":
					case "예":
					case "예스":
					case "네":
					case "응":
					case "켜기":
					case "참":
						return true;
						
					case "FALSE":
					case "NO":
					case "0":
					case "아니오":
					case "노":
					case "아니":
					case "끄기":
					case "거짓":
						return false;
						
					default:
						Debug.LogError(strval + " is not treated as a boolean value");
						break;
				}
				return false;
			}},
			{typeof(FSNInGameSetting.FlowDirection), (strval) =>
			{
				switch(strval.ToUpper())
				{
					case "UP":
					case "위":
						return FSNInGameSetting.FlowDirection.Up;

					case "DOWN":
					case "아래":
						return FSNInGameSetting.FlowDirection.Down;

					case "LEFT":
					case "왼쪽":
						return FSNInGameSetting.FlowDirection.Left;

					case "RIGHT":
					case "오른쪽":
						return FSNInGameSetting.FlowDirection.Right;

					default:
						Debug.LogError(strval + " is not treated as a flow direction value");
						break;
				}
				return null;
			}},
		};
}
