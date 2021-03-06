﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public sealed class FSNEngine : MonoBehaviour
{
	/// <summary>
	/// 엔진에 필요한 모듈 이름들.
	/// </summary>
	public enum ModuleType
	{
		Text,
		Image,
		//Object,
	}

	/// <summary>
	/// 스크립트 실행 종류
	/// </summary>
	public enum ExecuteType
	{
		NewStart,				// 새로 실행
		LoadFromOtherScript,	// 다른 스크립트에서 호출하여 실행
		LoadFromSession,		// 세션 로드 (불러오기)
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

	bool							m_awake	= false;

	Dictionary<string, FSNModule>	m_moduleRefDict;								// 모듈들
	FSNInGameSetting				m_inGameSetting;								// 인게임 세팅 (기본세팅)

	FSNSequenceEngine				m_seqEngine;									//
	FSNDefaultUnityCallServer		m_unityCallSvr;									//
	FSNControlSystem				m_ctrlSystem;


	public ICollection<FSNModule>	AllModules
	{
		get { return m_moduleRefDict.Values; }
	}

	/// <summary>
	/// 기본 세팅
	/// </summary>
	public FSNInGameSetting DefaultInGameSetting
	{
		get { return m_inGameSetting; }
	}

	/// <summary>
	/// 현재 스크립트 실행 상태에서의 세팅
	/// </summary>
	public IInGameSetting InGameSetting
	{
		get { return m_seqEngine.InGameSetting; }
	}

	public FSNControlSystem ControlSystem
	{
		get { return m_ctrlSystem; }
	}

	public FSNSequenceEngine.FlowSpeedControl FlowSpeedCtrl
	{
		get { return m_seqEngine.flowSpeedControl; }
	}

	// Statics

	private static FSNEngine	s_instance;
	public static FSNEngine Instance
	{
		get
		{
			if(s_instance == null)
			{
				GameObject.FindObjectOfType<FSNEngine>().Awake();
			}
			return s_instance;
		}
	}


	//===========================================================================

	void Awake()
	{
		if (m_awake)
			return;
		m_awake	= true;

		if(s_instance)
		{
			Debug.LogError("[FSNEngine] Duplicated engine object!");
			return;
		}
		s_instance			= this;


		// 세팅 초기화

		m_inGameSetting		= new FSNInGameSetting(true);

		// Persistent Data 불러오기
		FSNPersistentData.Load();


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

		m_unityCallSvr		= gameObject.AddComponent<FSNDefaultUnityCallServer>();
		gameObject.AddComponent<FSNFundamentalScriptFunctions>();

		m_seqEngine			= gameObject.AddComponent<FSNSequenceEngine>();
		m_seqEngine.Initialize();

		m_ctrlSystem		= gameObject.AddComponent<FSNControlSystem>();
		m_ctrlSystem.Initialize();

		// 초기화 완료 콜백
		CallAfterInitEvents();

		// 디버그 모듈 초기화
		FSNDebug.Install();
	}

	void OnDestroy()
	{
		// Persistent Data 저장 (forced)
		FSNPersistentData.Save(true);

		// 디버그 모듈 해제
		FSNDebug.Uninstall();
	}

	void Update()
	{
		FSNPersistentData.Save();	// Persistent Data 변경점 세이브. (변화가 있을 때만 실제로 save한다)
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
	/// 초기화 종료 후 이벤트 호출
	/// </summary>
	void CallAfterInitEvents()
	{
		foreach (FSNModule module in m_moduleRefDict.Values)
		{
			module.OnAfterEngineInit();
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

	/// <summary>
	/// 레이어 ID로 모듈 가져오기
	/// </summary>
	/// <param name="layerID"></param>
	/// <returns></returns>
	public IFSNLayerModule GetModuleByLayerID(int layerID)
	{
		return m_seqEngine.GetModuleByLayerID(layerID);
	}


	/// <summary>
	/// 스크립트 실행
	/// </summary>
	/// <param name="filepath"></param>
	/// <param name="session">실행 중에 사용할 Session. 지정하지 않을 경우 새 세션을 사용</param>
	/// <param name="snapshotIndex">불러오기 시에만 사용. 시작할 Snapshot Index를 지정</param>
	public void RunScript(string filepath, ExecuteType exeType = ExecuteType.NewStart, int snapshotIndex = 0)
	{
		FSNResourceCache.StartLoadingSession(FSNResourceCache.Category.Script);             // 리소스 로딩 세션 시작
		
		if (exeType == ExecuteType.NewStart)												// 새 세션을 열어야하는 경우 미리 세션 생성하기 (새 게임)
		{
			m_seqEngine.PrepareNewSession();
		}

		FSNScriptSequence scriptSeq	= FSNScriptSequence.Parser.FromAsset(filepath, m_seqEngine.CurrentSession);	// 스크립트 해석

		// TODO : header 적용, 여기에 코드를 삽입하는 것이 맞는지는 잘 모르겠음. 리팩토링이 필요할수도.

		// 인게임 세팅

		var stchain	= new FSNInGameSetting.Chain(FSNInGameSetting.DefaultInGameSetting);	// 디폴트 속성을 베이스로 chain 만들기
		foreach (var pair in scriptSeq.Header.InGameSettings)
		{
			var alias	= FSNInGameSetting.ConvertPropertyNameAlias(pair.Key);
			stchain.SetPropertyByString(alias, pair.Value);
		}
		m_inGameSetting	= stchain.Freeze();													// 속성값 고정, 현재 엔진의 디폴트 속성을 덮어씌운다.
		//

		var sshotSeq = FSNSnapshotSequence.Builder.BuildSnapshotSequence(scriptSeq);		// Snapshot 시퀀스 생성

		FSNResourceCache.EndLoadingSession();                                               // 리소스 로딩 세션 종료

		bool overwriteScriptInfoToSession   = exeType != ExecuteType.LoadFromSession;		// 불러오기한 경우가 아니면 스크립트 정보를 세션에 덮어써야한다.
        m_seqEngine.StartSnapshotSequence(sshotSeq, overwriteScriptInfoToSession, snapshotIndex);	// 실행


		ControlSystem.ScriptLoadComplete();													// 스크립트 로딩 완료 이벤트
	}

	/// <summary>
	/// 세션 (세이브 파일) 로드
	/// </summary>
	/// <param name="filepath"></param>
	public void LoadSession(string filepath)
	{
		foreach(var module in m_moduleRefDict.Values)		// 로딩 직전의 이벤트 호출
		{
			module.OnBeforeLoadSession();
		}

		var session	= FSNSession.Load(filepath);
		m_seqEngine.LoadFromSession(session);
	}

	/// <summary>
	/// 세션 (세이브 파일) 저장
	/// </summary>
	/// <param name="filepath"></param>
	public void SaveSession(string filepath, string memo = "")
	{
		m_seqEngine.SaveToCurrentSession();
		FSNSession.Save(m_seqEngine.CurrentSession, filepath, memo);
	}


	/// <summary>
	/// Script에서 메서드 호출, 리턴값 없음
	/// </summary>
	/// <param name="funcname"></param>
	/// <param name="param"></param>
	public void ScriptUnityCallVoid(string funcname, params string[] param)
	{
		m_unityCallSvr.CallVoidMethod(funcname, param);
	}

	/// <summary>
	/// 스크립트에서 메서드 호출, 리턴값은 bool
	/// </summary>
	/// <param name="funcname"></param>
	/// <param name="param"></param>
	/// <returns></returns>
	public bool ScriptUnityCallBool(string funcname, params string [] param)
	{
		return m_unityCallSvr.CallBoolMethod(funcname, param);
	}


	/// <summary>
	/// 스크립트의 점프 조건 체크를 갱신한다. 현재 표시중인 스냅샷에서 내부적으로 변수값 등등이 바뀌어서 조건 점프 결과 등이 갱신되어야할 경우 호출해주면 됨.
	/// 실질적으로는 조건 체크 함수들을 다시 호출하는 결과를 부른다.
	/// </summary>
	public void UpdateScriptConditions()
	{
		m_seqEngine.UpdateScriptConditions();
	}


	public bool GetScriptFlag(string name)
	{
		return FSNPersistentData.IsPersistentVarName(name)?
			FSNPersistentData.GetScriptFlag(name) :
			m_seqEngine.CurrentSession.GetFlagValue(name);
	}

	public void SetScriptFlag(string name, bool value)
	{
		if(FSNPersistentData.IsPersistentVarName(name))
		{
			FSNPersistentData.SetScriptFlag(name, value);
		}
		else
		{
			m_seqEngine.CurrentSession.SetFlagValue(name, value);
		}
	}

	public float GetScriptValue(string name)
	{
		return FSNPersistentData.IsPersistentVarName(name)?
			FSNPersistentData.GetScriptValue(name) :
			m_seqEngine.CurrentSession.GetNumberValue(name);
	}

	public void SetScriptValue(string name, float value)
	{
		if (FSNPersistentData.IsPersistentVarName(name))
		{
			FSNPersistentData.SetScriptValue(name, value);
		}
		else
		{
			m_seqEngine.CurrentSession.SetNumberValue(name, value);
		}
	}

	public bool ScriptFlagIsDeclared(string name)
	{
		// note : persistent 변수값들은 변수가 있는지 여부를 체크하지 않는다. 무조건 true
		return FSNPersistentData.IsPersistentVarName(name) || m_seqEngine.CurrentSession.FlagIsDeclared(name);
	}

	public bool ScriptValueIsDeclared(string name)
	{
		// note : persistent 변수값들은 변수가 있는지 여부를 체크하지 않는다. 무조건 true
		return FSNPersistentData.IsPersistentVarName(name) || m_seqEngine.CurrentSession.ValueIsDeclared(name);
	}
}
