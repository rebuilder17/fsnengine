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

	public static Color ConvertHexCodeToColor(string hexcode)
	{
		uint r = 0, g = 0, b = 0;
		uint a	= 255;

		if (hexcode[0] == '#')					// # 이 있을 경우 제거 (헥사코드 표시)
			hexcode	= hexcode.Substring(1);

		uint hexNumber	= uint.Parse(hexcode, System.Globalization.NumberStyles.HexNumber);

		switch(hexcode.Length)
		{
			case 3:								// * RGB
				r	= BitExtract(hexNumber, 0xf, 8);
				g	= BitExtract(hexNumber, 0xf, 4);
				b	= BitExtract(hexNumber, 0xf, 0);
				break;

			case 4:								// * RGBA
				r	= BitExtract(hexNumber, 0xf, 12);
				g	= BitExtract(hexNumber, 0xf, 8);
				b	= BitExtract(hexNumber, 0xf, 4);
				a	= BitExtract(hexNumber, 0xf, 0);
				break;

			case 6:								// * RRGGBB
				r	= BitExtract(hexNumber, 0xff, 16);
				g	= BitExtract(hexNumber, 0xff, 8);
				b	= BitExtract(hexNumber, 0xff, 0);
				break;

			case 8:								// * RRGGBBAA
				r	= BitExtract(hexNumber, 0xff, 24);
				g	= BitExtract(hexNumber, 0xff, 16);
				b	= BitExtract(hexNumber, 0xff, 8);
				a	= BitExtract(hexNumber, 0xff, 0);
				break;

			default:
				Debug.LogError("[ConvertHexCodeToColor] wrong hexcode : " + hexcode);
				break;
		}

		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
	}

	private static uint BitExtract(uint input, uint mask, byte shift)
	{
		return (input & (mask << shift)) >> shift;
	}
}
