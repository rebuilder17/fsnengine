using UnityEngine;
using System.Collections;


/// <summary>
/// 텍스트 모듈, 기본형
/// </summary>
public abstract class FSNTextModule : FSNModule
{
	public override string ModuleName
	{
		get { return FSNEngine.ModuleType.Text.ToString(); }
	}


	//==================================================================

	public override void Initialize()
	{
		//
	}


}
