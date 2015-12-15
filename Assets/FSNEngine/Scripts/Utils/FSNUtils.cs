using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


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

	/// <summary>
	/// 문자열을 type에 맞는 값으로 변환.
	/// </summary>
	/// <param name="strval"></param>
	/// <returns></returns>
	public static T StringToValue<T>(string strval)
	{
		return (T)StringToValue(typeof(T), strval);
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
					case "ㅇㅇ":
					case "ㅇㅋ":
						return true;
						
					case "FALSE":
					case "NO":
					case "0":
					case "아니오":
					case "노":
					case "아니":
					case "끄기":
					case "거짓":
					case "ㄴㄴ":
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
					case "위쪽":
						return FSNInGameSetting.FlowDirection.Up;

					case "DOWN":
					case "아래":
					case "아래쪽":
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
			{typeof(FSNInGameSetting.TextAlignType), (strval) =>
			{
				switch(strval.ToUpper())
				{
					case "LEFT":
					case "왼쪽":
						return FSNInGameSetting.TextAlignType.Left;
					case "MIDDLE":
					case "가운데":
						return FSNInGameSetting.TextAlignType.Middle;
					case "RIGHT":
					case "오른쪽":
						return FSNInGameSetting.TextAlignType.Right;
				}
				return null;
			}},
			{typeof(Segments.Image.PivotPreset), (strval) =>
			{
				switch(strval.ToUpper())
				{
					case "CENTER":
					case "중앙":
					case "가운데":
						return Segments.Image.PivotPreset.Center;

					case "TOP":
					case "위쪽":
					case "위":
					case "상단":
					case "상":
						return Segments.Image.PivotPreset.Top;

					case "TOPRIGHT":
					case "오른쪽위":
					case "우상단":
					case "우상":
						return Segments.Image.PivotPreset.TopRight;

					case "RIGHT":
					case "오른쪽":
					case "우측":
					case "우":
						return Segments.Image.PivotPreset.Right;

					case "BOTTOMRIGHT":
					case "오른쪽아래":
					case "우하단":
					case "우하":
						return Segments.Image.PivotPreset.BottomRight;

					case "BOTTOM":
					case "아래쪽":
					case "아래":
					case "하단":
					case "하":
						return Segments.Image.PivotPreset.Bottom;

					case "BOTTOMLEFT":
					case "왼쪽아래":
					case "좌하단":
					case "좌하":
						return Segments.Image.PivotPreset.BottomLeft;

					case "LEFT":
					case "왼쪽":
					case "좌측":
					case "좌":
						return Segments.Image.PivotPreset.Left;

					case "TOPLEFT":
					case "왼쪽위":
					case "좌상단":
					case "좌상":
						return Segments.Image.PivotPreset.TopLeft;
						

					default:
						Debug.LogError(strval + " is not treated as a pivot value");
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


	/// <summary>
	/// persistent 경로에 해당 이름의 파일이 존재하는지
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public static bool CheckTextFileExists(string filename)
	{
		return File.Exists(Application.persistentDataPath + "/" + filename);
	}

	/// <summary>
	/// persistent 경로의 특정 파일에 문자열 저장
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="data"></param>
	public static void SaveTextData(string filename, string data)
	{
		var fs = File.Open(Application.persistentDataPath + "/" + filename, FileMode.Create);
		using(fs)
		{
			var writer = new StreamWriter(fs);
			using(writer)
			{
				writer.Write(data);
			}
		}
	}

	/// <summary>
	/// persistent 경로의 특정 파일에서 문자열 로드하기
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public static string LoadTextData(string filename)
	{
		var fs = File.OpenText(Application.persistentDataPath + "/" + filename);
		string text;
		using(fs)										// 파일을 읽고 json으로 파싱
		{
			text = fs.ReadToEnd();
		}
		return text;
	}


	/// <summary>
	/// 앱 종료, 혹은 Play 종료하기
	/// </summary>
	public static void QuitApp()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public static string GenerateCurrentDateAndTimeString()
	{
		var dateTime	= System.DateTime.Now;
		return dateTime.ToString();
	}


	/// <summary>
	/// 경로를 분리하여 디렉토리 부분과 파일 이름으로 나눈다
	/// </summary>
	/// <param name="origpath"></param>
	/// <param name="path"></param>
	/// <param name="name"></param>
	public static void StripPathAndName(string origpath, out string path, out string name)
	{
		var pathdel = origpath.LastIndexOf('/');
		if (pathdel != -1)
		{
			path    = origpath.Substring(0, pathdel);
			name    = origpath.Substring(pathdel + 1);
		}
		else
		{
			path    = "";
			name    = origpath;
		}
	}
	
	/// <summary>
	/// 파일 확장자 삭제
	/// </summary>
	/// <param name="origpath"></param>
	/// <returns></returns>
	public static string RemoveFileExt(string origpath)
	{
		var pos	= origpath.LastIndexOf('.');
		if (pos == -1)
			return origpath;
		else
		{
			return origpath.Substring(0, pos);
		}
	}
}
