using UnityEngine;
using System.Collections;


/// <summary>
/// 엔진에서 사용하는 기본 Script 함수들
/// </summary>
public class FSNFundamentalScriptFunctions : FSNBaseUnityCallReceiver
{
	[UnityCallVoidMethod]
	public static void TestCall(params string [] param)
	{
		Debug.Log("AAAAAAAAAAAAA");
		foreach (var str in param)
		{
			Debug.Log(str);
		}
	}

	[UnityCallVoidMethod]
	public void TestCall2(params string [] param)
	{
		Debug.Log("TestCall2 called! " + gameObject.name);
	}

	static int testcounter = 0;

	[UnityCallBoolMethod]
	public bool TestBoolCheck(params string [] param)
	{
		testcounter = (testcounter + 1) % 4;
		return testcounter < 2;
	}
}
