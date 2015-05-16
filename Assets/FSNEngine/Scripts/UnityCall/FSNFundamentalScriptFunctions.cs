using UnityEngine;
using System.Collections;


/// <summary>
/// 엔진에서 사용하는 기본 Script 함수들
/// </summary>
public class FSNFundamentalScriptFunctions : FSNBaseUnityCallReceiver
{
	// Engine 레퍼런스 캐싱
	private static FSNEngine engine;

	public override void Awake_Derrived()
	{
		engine	= FSNEngine.Instance;
	}


	// Util funcs

	/// <summary>
	/// 파라미터 문자열을 변수명으로 해서 세션 플래그값을 가져오거나, 세션 플래그값이 아닌 것 같은 경우 리터럴로 처리
	/// </summary>
	/// <param name="param"></param>
	/// <returns></returns>
	static bool GetFlagOrLiteral(string param)
	{
		if (engine.ScriptFlagIsDeclared(param))
			return engine.GetScriptFlag(param);
		else
			return FSNUtils.StringToValue<bool>(param);
	}

	/// <summary>
	/// 파라미터 문자열을 변수명으로 해서 세션 변수값을 가져오거나, 세션 변수값이 아닌 것 같은 경우 리터럴로 처리
	/// </summary>
	/// <param name="param"></param>
	/// <returns></returns>
	static float GetValueOrLiteral(string param)
	{
		if (engine.ScriptValueIsDeclared(param))
			return engine.GetScriptValue(param);
		else
			return FSNUtils.StringToValue<float>(param);
	}


	//

	[UnityCallVoidMethod]
	public static void __fsnengine_SetFlagTrue(params string [] param)
	{
		int count	= param.Length;
		for (int i = 0; i < count; i++)
			engine.SetScriptFlag(param[i], true);
	}

	[UnityCallVoidMethod]
	public static void __fsnengine_SetFlagFalse(params string [] param)
	{
		int count	= param.Length;
		for (int i = 0; i < count; i++)
			engine.SetScriptFlag(param[i], false);
	}

	[UnityCallVoidMethod]
	public static void __fsnengine_SetFlags(params string [] param)
	{
		int count		= param.Length;
		string varname	= null;
		for (int i = 0; i < count; i++)
		{
			if (i % 2 == 0)				// 홀수번째 파라미터는 플래그 이름
			{
				varname	= param[i];
			}
			else
			{							// 짝수번째는 변수값. 바로 이전에 얻은 변수 이름으로 세팅한다
				engine.SetScriptFlag(varname, FSNUtils.StringToValue<bool>(param[i]));
				varname	= null;
			}
		}
	}

	[UnityCallVoidMethod]
	public static void __fsnengine_SetValues(params string [] param)
	{
		int count		= param.Length;
		string varname	= null;
		for (int i = 0; i < count; i++)
		{
			if (i % 2 == 0)				// 홀수번째 파라미터는 플래그 이름
			{
				varname	= param[i];
			}
			else
			{							// 짝수번째는 변수값. 바로 이전에 얻은 변수 이름으로 세팅한다
				engine.SetScriptValue(varname, FSNUtils.StringToValue<float>(param[i]));
				varname	= null;
			}
		}
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_IfFlagIsTrue(params string [] param)
	{
		return engine.GetScriptFlag(param[0]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_IfFlagIsFalse(params string [] param)
	{
		return !engine.GetScriptFlag(param[0]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_CheckFlagValue(params string [] param)
	{
		return GetFlagOrLiteral(param[0]) == GetFlagOrLiteral(param[1]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_CheckValueIsEqualTo(params string [] param)
	{
		return GetValueOrLiteral(param[0]) == GetValueOrLiteral(param[1]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_CheckValueIsNotEqualTo(params string [] param)
	{
		return GetValueOrLiteral(param[0]) != GetValueOrLiteral(param[1]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_CheckValueIsGreaterThan(params string [] param)
	{
		return GetValueOrLiteral(param[0]) > GetValueOrLiteral(param[1]);
	}

	[UnityCallBoolMethod]
	public static bool __fsnengine_CheckValueIsLesserThan(params string [] param)
	{
		return GetValueOrLiteral(param[0]) < GetValueOrLiteral(param[1]);
	}

	//=============================================================================

	[UnityCallVoidMethod]
	public static void __fsnengine_ShowSaveDialog(params string [] param)
	{
		FSNEngine.Instance.ControlSystem.ShowMenu(FSNControlSystem.EssentialMenu.Save);
	}

	[UnityCallVoidMethod]
	public static void __fsnengine_ShowLoadDialog(params string [] param)
	{
		FSNEngine.Instance.ControlSystem.ShowMenu(FSNControlSystem.EssentialMenu.Load);
	}
}
