using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 게임 내 실행중 스크립트로 조작하는 세팅. 초기화 값이 곧 디폴트 세팅값
/// </summary>
public sealed class FSNInGameSetting
{
	/// <summary>
	/// 진행 방향
	/// </summary>
	public enum FlowDirection
	{
		Up,
		Down,
		Left,
		Right,
	}

	//==============================================================================


	/// <summary>
	/// 현재 진행 방향
	/// </summary>
	public FlowDirection CurrentFlowDirection	= FlowDirection.Down;

	/// <summary>
	/// 폰트 크기
	/// </summary>
	public int	FontSize	= 18;

	//===============================================================================

	/// <summary>
	/// 디폴트 인게임 세팅 (static)
	/// </summary>
	public static FSNInGameSetting DefaultInGameSetting { get; private set; }

	static FSNInGameSetting()
	{
		DefaultInGameSetting	= new FSNInGameSetting();
	}
}


public sealed class FSNEngine : MonoBehaviour
{
	/// <summary>
	/// 엔진에 필요한 모듈 이름들
	/// </summary>
	public enum ModuleType
	{
		Text,
	}

	//===========================================================================

	// Properties




	// Members

	Dictionary<string, FSNModule>	m_moduleRefDict;								// 모듈들
	FSNInGameSetting				m_inGameSetting;								// 현재 인게임 세팅


	// Statics

	private static FSNEngine	s_instance;
	public static FSNEngine Instance
	{
		get
		{
			return s_instance;
		}
	}


	//===========================================================================

	void Awake()
	{
		if(s_instance)
		{
			Debug.LogError("[FSNEngine] Duplicated engine object!");
			return;
		}
		s_instance			= this;

		m_moduleRefDict		= new Dictionary<string, FSNModule>();

		var modulesFound	= GetComponentsInChildren<FSNModule>();					// 오브젝트 구조 안에서 모듈 컴포넌트 찾기
		foreach(FSNModule module in modulesFound)
		{
			m_moduleRefDict[module.ModuleName]	= module;
		}

		// 필요 모듈 중에 빠진 게 있는지 체크
		var essentialModules	= System.Enum.GetNames(typeof(ModuleType));
		foreach(string moduleName in essentialModules)
		{
			if(!m_moduleRefDict.ContainsKey(moduleName))
			{
				Debug.LogError("[FSNEngine] Essential module not found : " + moduleName);
			}
		}

		InitAllModules();															// 찾아낸 모듈 모두 초기화
	}

	/// <summary>
	/// 모듈 모두 초기화
	/// </summary>
	void InitAllModules()
	{
		foreach(FSNModule module in m_moduleRefDict.Values)
		{
			module.Initialize();
		}
	}
}
