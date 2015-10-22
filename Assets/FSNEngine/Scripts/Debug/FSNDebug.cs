using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 디버깅 관련
/// </summary>
public static class FSNDebug
{
	/// <summary>
	/// 현재 실행 상황
	/// </summary>
	public enum RuntimeStage
	{
		Compile,				// 스크립트 컴파일 (스크립트 -> Sequence)
		SnapshotBuild,			// 스냅샷 빌드 (Sequence -> SnapshotSequence)
		Runtime,				// 실제 실행중
	}


	// Members

	/// <summary>
	/// 현재의 런타임 스테이지 설정
	/// </summary>
	public static RuntimeStage	currentRuntimeStage	{ private get; set; }

	/// <summary>
	/// 현재 처리중인 스크립트
	/// </summary>
	public static string currentProcessingScript	{ private get; set; }

	/// <summary>
	/// 현재 처리중인 스크립트 줄 번호
	/// </summary>
	public static int currentProcessingScriptLine	{ private get; set; }



	public static bool Installed { get; private set; }



	public static void Install()
	{
		currentRuntimeStage	= RuntimeStage.Runtime;

		Application.logMessageReceived	+= HandleLog;

		Installed = true;
	}

	public static void Uninstall()
	{
		Application.logMessageReceived	-= HandleLog;

		Installed = false;
	}

	//
	static void HandleLog(string message, string stacktrace, LogType logtype)
	{
		if (logtype == LogType.Log || logtype == LogType.Warning)
			return;

		bool showStackTrace = false;
		string typetext	= "";
		switch(logtype)
		{
			case LogType.Warning:
				typetext	= "주의할 사항을 발견했습니다.";
				break;

			case LogType.Error:
				typetext	= "오류가 발생했습니다.";
				break;

			case LogType.Exception:
			case LogType.Assert:
				typetext	= "예외가 발생했습니다.";
				showStackTrace = true;
				break;
		}

		string header	= "";
		switch(currentRuntimeStage)
		{
			case RuntimeStage.Compile:
				header	= string.Format("스크립트 컴파일 중 {0} ({1}, line:{2}) :", typetext, currentProcessingScript, currentProcessingScriptLine);
				break;
			case RuntimeStage.SnapshotBuild:
				header	= string.Format("스냅샷 생성 중 {0} ({1}, line:{2}) :", typetext, currentProcessingScript, currentProcessingScriptLine);
				break;

			case RuntimeStage.Runtime:
				header	= string.Format("실행 중 {0} ({1}) : ", typetext, currentProcessingScript);
				break;
		}

		string completemsg	= header + message + (showStackTrace? ("\n" + stacktrace) : "");
		FSNEngine.Instance.StartCoroutine(LateLog(completemsg));
#if UNITY_EDITOR
		EditorUtility.DisplayDialog("FSNEngine", completemsg, "확인");		// 에디터상에서는 팝업으로 따로 알려줌
#endif
	}

	static IEnumerator LateLog(string msg)
	{
		yield return new WaitForEndOfFrame();
		Debug.Log(msg);
	}
}
