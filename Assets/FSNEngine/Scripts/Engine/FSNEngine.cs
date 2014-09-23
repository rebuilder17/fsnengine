using UnityEngine;
using System.Collections;
using System.Collections.Generic;



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

	[SerializeField]
	float							m_screenYSize	= 720;							// 계산에 사용할 화면 Y 길이


	/// <summary>
	/// 계산에 사용할 화면 Y 길이
	/// </summary>
	public float ScreenYSize
	{
		get { return m_screenYSize; }
	}

	/// <summary>
	/// 계산에 사용할 화면 X 길이
	/// </summary>
	public float ScreenXSize
	{
		get
		{
			float ratio	= (float)Screen.width / (float)Screen.height;
			return m_screenYSize * ratio;
		}
	}

	/// <summary>
	/// 계산에 사용할 화면 크기
	/// </summary>
	public Vector2 ScreenDimension
	{
		get
		{
			return new Vector2() { x = ScreenXSize, y = ScreenYSize };
		}
	}


	// Members

	Dictionary<string, FSNModule>	m_moduleRefDict;								// 모듈들
	FSNInGameSetting				m_inGameSetting;								// 인게임 세팅 (기본세팅)

	FSNSequenceEngine				m_seqEngine;									//


	public ICollection<FSNModule>	AllModules
	{
		get { return m_moduleRefDict.Values; }
	}

	public FSNInGameSetting InGameSetting
	{
		get { return m_inGameSetting; }
	}

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


		// 세팅 초기화

		m_inGameSetting		= new FSNInGameSetting(true);


		// 모듈 초기화

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


		// 보조 컴포넌트 초기화

		m_seqEngine				= GetComponent<FSNSequenceEngine>();
		m_seqEngine.Initialize();
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

	/// <summary>
	/// 모듈 가져오기
	/// </summary>
	/// <param name="moduleName"></param>
	/// <returns></returns>
	public FSNModule GetModule(string moduleName)
	{
		return m_moduleRefDict[moduleName];
	}
	/// <summary>
	/// 모듈 가져오기
	/// </summary>
	/// <param name="moduleType"></param>
	/// <returns></returns>
	public FSNModule GetModule(FSNEngine.ModuleType moduleType)
	{
		return m_moduleRefDict[moduleType.ToString()];
	}
}
