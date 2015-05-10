using UnityEngine;
using System.Collections;

/// <summary>
/// 엔진 초기화가 종료되고 나서 바로 지정한 스크립트 실행
/// </summary>
public class FSNRunScriptOnLaunch : FSNModule
{
	[SerializeField]
	string			m_scriptPath;			// 바로 실행할 스크립트 경로(이름)


	public static string ModuleNameStatic
	{
		get { return "RunScriptOnLaunch"; }
	}

	public override string ModuleName
	{
		get { return ModuleNameStatic; }
	}

	public override void Initialize()
	{
		
	}

	public override void OnAfterEngineInit()
	{
		StartCoroutine(LateStart());
	}

	IEnumerator LateStart()
	{
		yield return null;									// 한 프레임 늦게 시작한다
		FSNEngine.Instance.RunScript(m_scriptPath);
	}
}
