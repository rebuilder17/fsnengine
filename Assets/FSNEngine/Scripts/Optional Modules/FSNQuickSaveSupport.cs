using UnityEngine;
using System.Collections;

/// <summary>
/// 퀵세이브 기능을 지원하는 모듈. 아울러 PC 환경에서는 퀵세이브 단축키 기능도 제공한다
/// </summary>
public class FSNQuickSaveSupport : FSNModule
{
	[SerializeField]
	string			m_quickSaveFileName = "quicksave.sav";


	/// <summary>
	/// 퀵세이브 파일 이름
	/// </summary>
	public string QuickSaveFileName
	{
		get { return m_quickSaveFileName; }
	}

	public static string ModuleNameStatic
	{
		get { return "QuickSaveSupport"; }
	}

	public override string ModuleName
	{
		get { return ModuleNameStatic; }
	}

	public override void Initialize()
	{
		
	}

	//void Update()
	//{
	//	if (Input.GetKeyDown(KeyCode.F5))
	//	{
	//		Debug.Log("SAVE");
	//		QuickSave();
	//	}
	//	else if (Input.GetKeyDown(KeyCode.F9))
	//	{
	//		Debug.Log("Load");
	//		QuickLoad();
	//	}
	//}

	//

	/// <summary>
	/// 퀵세이브
	/// </summary>
	public void QuickSave()
	{
		FSNEngine.Instance.SaveSession(m_quickSaveFileName);
	}

	/// <summary>
	/// 퀵로드
	/// </summary>
	public void QuickLoad()
	{
		FSNEngine.Instance.LoadSession(m_quickSaveFileName);
	}
}
