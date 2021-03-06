﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


/// <summary>
/// (interface) 게임 내 실행중 스크립트로 조작하는 세팅.
/// </summary>
public interface IInGameSetting
{
	/// <summary>
	/// 현재 진행 방향
	/// 주 : "현재 스냅샷"의 이동 방향을 의미. 다음 혹은 이전으로 넘어가기 위한 swipe 방향을 의미하는 것이 아니다.
	/// </summary>
	FSNInGameSetting.FlowDirection CurrentFlowDirection { get; }
	/// <summary>
	/// 반대 방향 구하기
	/// 주 : "현재 스냅샷"의 이동 방향을 의미. 다음 혹은 이전으로 넘어가기 위한 swipe 방향을 의미하는 것이 아니다.
	/// </summary>
	FSNInGameSetting.FlowDirection BackwardFlowDirection { get; }

	/// <summary>
	/// Swipe할 시의 "무게감" (얼만큼 swipe해야 다음으로 넘어가는가)
	/// </summary>
	float SwipeWeight { get; }

	/// <summary>
	/// 폰트 크기
	/// </summary>
	float FontSize { get; }

	/// <summary>
	/// 새 텍스트를 화면 언저리가 아닌 화면 한가운데로 끌어오는지
	/// </summary>
	bool ScreenCenterText { get; }

	/// <summary>
	/// 텍스트들을 쌓는지 여부. (이전 텍스트가 바로 없어지지 않고 화면 바깥으로 밀려나가야 사라지는 식)
	/// </summary>
	bool StackTexts { get; }

	/// <summary>
	/// 텍스트, 위쪽 여백
	/// </summary>
	float TextMarginTop { get; }
	/// <summary>
	/// 텍스트 아래쪽 여백
	/// </summary>
	float TextMarginBottom { get; }
	/// <summary>
	/// 텍스트 왼쪽 여백
	/// </summary>
	float TextMarginLeft { get; }
	/// <summary>
	/// 텍스트 오른쪽 여백
	/// </summary>
	float TextMarginRight { get; }

	/// <summary>
	/// 텍스트 정렬
	/// </summary>
	FSNInGameSetting.TextAlignType TextAlign { get; }

	/// <summary>
	/// 텍스트 줄 간격
	/// </summary>
	float TextLineSpacing { get; }

	/// <summary>
	/// 문단 간격
	/// </summary>
	float ParagraphSpacing { get; }

	/// <summary>
	/// 전환 속도 비율
	/// </summary>
	float TransitionSpeedRatio { get; }

	/// <summary>
	/// 텍스트 전환 속도
	/// </summary>
	float TextTransitionTime { get; }

	/// <summary>
	/// 세이브/로드 금지 (직접 호출은 가능, 메뉴에서만 접근 불가)
	/// </summary>
	bool PreventSaveAndLoadMenu { get; }
}

/// <summary>
/// 인게임 세팅의 베이스 클래스. 프로퍼티를 string으로 빠르게 찾을 수 있게 준비해놓은 것
/// </summary>
public abstract class BaseInGameSetting : IInGameSetting
{	
	// Members

	public delegate valT Getter<valT>();
	public delegate void Setter<valT>(valT value);

	struct PropertyDelegates
	{
		public System.Delegate getter;
		public System.Delegate setter;
	}

	Dictionary<string, PropertyDelegates>	m_propDelDict;	// 프로퍼티 getter/setter 델리게이트 캐시


	public BaseInGameSetting()
	{
		CachePropertiesForClass(this);

		m_propDelDict	= new Dictionary<string, PropertyDelegates>();
	}

	void CacheDelegate<valT>(string propertyName)
	{
		if(!m_propDelDict.ContainsKey(propertyName))		// 없을 경우에만 델리게이트를 만든다
		{
			var propInfo	= LookupPropertyInfo(this, propertyName);
			PropertyDelegates newInfo;

			newInfo.getter	= System.Delegate.CreateDelegate(typeof(Getter<valT>), this, propInfo.GetGetMethod());
			newInfo.setter	= System.Delegate.CreateDelegate(typeof(Setter<valT>), this, propInfo.GetSetMethod());

			m_propDelDict[propertyName]	= newInfo;
		}
	}

	/// <summary>
	/// 프로퍼티의 Getter 델리게이트를 얻어온다
	/// </summary>
	/// <typeparam name="valT"></typeparam>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	public Getter<valT> GetGetter<valT>(string propertyName)
	{
		CacheDelegate<valT>(propertyName);					// 아직 캐싱된 게 없을 경우엔 델리게이트를 생성해둔다
		return m_propDelDict[propertyName].getter as Getter<valT>;
	}

	/// <summary>
	/// 프로퍼티의 Setter 델리게이트를 얻어온다
	/// </summary>
	/// <typeparam name="valT"></typeparam>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	public Setter<valT> GetSetter<valT>(string propertyName)
	{
		CacheDelegate<valT>(propertyName);					// 아직 캐싱된 게 없을 경우엔 델리게이트를 생성해둔다
		return m_propDelDict[propertyName].setter as Setter<valT>;
	}

	//===================================================================================================

	static Dictionary<System.Type, Dictionary<string, PropertyInfo>> s_propLookupTable;	// 타입 => 프로퍼티 이름(문자열) => PropertyInfo
	static string[]	s_propertyNames;													// 프로퍼티 이름들 (IInGameSetting)

	/// <summary>
	/// 프로퍼티 이름 리스트
	/// </summary>
	public static string[] PropertyNames
	{
		get { return s_propertyNames; }
	}

	static BaseInGameSetting()
	{
		// IInGameSetting 의 모든 프로퍼티 이름들을 미리 구해놓는다
		var props			= typeof(IInGameSetting).GetProperties();
		int count			= props.Length;
		s_propertyNames		= new string[count];
		for(int i = 0; i < count; i++)
		{
			s_propertyNames[i]	= props[i].Name;
		}

		// 기타 초기화
		s_propLookupTable	= new Dictionary<System.Type, Dictionary<string, PropertyInfo>>();
	}

	/// <summary>
	/// 해당 오브젝트의 클래스 프로퍼티 정보를 전부 얻어둔다
	/// </summary>
	/// <param name="settingObj"></param>
	private static void CachePropertiesForClass(IInGameSetting settingObj)
	{
		System.Type type		= settingObj.GetType();

		if(s_propLookupTable.ContainsKey(type))							// * 이미 캐싱되었다면 패스
			return;

		//
		var propInfoTable		= new Dictionary<string, PropertyInfo>();
		s_propLookupTable[type]	= propInfoTable;

		// IInGameSetting 의 프로퍼티 이름으로 해당 클래스 타입에서 검색하여 PropertyInfo를 얻어온다

		int nameCount			= s_propertyNames.Length;
		for(int i = 0; i < nameCount; i++)
		{
			string name			= s_propertyNames[i];
			propInfoTable[name]	= type.GetProperty(name);
		}
	}

	/// <summary>
	/// 프로퍼티 정보를 얻어옴
	/// </summary>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	protected static PropertyInfo LookupPropertyInfo(IInGameSetting settingObj, string propertyName)
	{
		//Debug.Log(string.Format("LookupPropertyInfo : {0} {1}", settingObj.GetType(), propertyName));
		CachePropertiesForClass(settingObj);
		return s_propLookupTable[settingObj.GetType()][propertyName];
	}

	//=======================================================================

	public virtual FSNInGameSetting.FlowDirection CurrentFlowDirection { get; set; }
	public virtual FSNInGameSetting.FlowDirection BackwardFlowDirection { get; set; }
	public virtual float SwipeWeight { get; set; }
	public virtual float FontSize { get; set; }
	public virtual bool ScreenCenterText { get; set; }
	public virtual bool StackTexts { get; set; }
	public virtual float TextMarginLeft { get; set; }
	public virtual float TextMarginRight { get; set; }
	public virtual float TextMarginTop { get; set; }
	public virtual float TextMarginBottom { get; set; }
	public virtual FSNInGameSetting.TextAlignType TextAlign { get; set; }
	public virtual float TextLineSpacing { get; set; }
	public virtual float ParagraphSpacing { get; set; }

	public virtual float TransitionSpeedRatio { get; set; }
	public virtual float TextTransitionTime { get; set; }
	public virtual bool PreventSaveAndLoadMenu { get; set; }
}



/// <summary>
/// 게임 내 실행중 스크립트로 조작하는 세팅. 초기화 값이 곧 디폴트 세팅값
/// </summary>
public sealed class FSNInGameSetting : BaseInGameSetting
{
	/// <summary>
	/// 진행 방향
	/// </summary>
	public enum FlowDirection
	{
		Up		= 0,
		Down	= 1,
		Left	= 2,
		Right	= 3,

		None	= -1	// 방향 없음
	}

	/// <summary>
	/// 텍스트 정렬
	/// </summary>
	public enum TextAlignType
	{
		Left	= 0,
		Middle	= 1,
		Right	= 2,
	}

	//==============================================================================

	/// <summary>
	/// 프로퍼티 이름 별명
	/// </summary>
	readonly static Dictionary<string, string> s_propNameAlias	= new Dictionary<string,string>()
	{
		{"현재진행방향",			"CurrentFlowDirection"},	
		{"진행방향",				"CurrentFlowDirection"},	
		{"무게감",				"SwipeWeight"},				
		{"글자크기",				"FontSize"},				
		{"문장을화면가운데로",		"ScreenCenterText"},		
		{"문장쌓기",				"StackTexts"},				
		{"글위쪽여백",			"TextMarginTop"},
		{"글아래쪽여백",			"TextMarginBottom"},
		{"글왼쪽여백",			"TextMarginLeft"},
		{"글오른쪽여백",			"TextMarginRight"},
		{"문장정렬",				"TextAlign"},
		{"줄간격",				"TextLineSpacing"},
		{"문단간격",				"ParagraphSpacing"},
		{"속도비율",				"TransitionSpeedRatio"},
		{"글자속도",				"TextTransitionSpeed"},
		{"저장메뉴금지",			"PreventSaveAndLoadMenu"},
	};

	/// <summary>
	/// 속성 이름 별명을 원래 속성명으로 변환
	/// </summary>
	/// <param name="aliasname"></param>
	/// <returns></returns>
	public static string ConvertPropertyNameAlias(string aliasname)
	{
		string realname		= aliasname;
		string spaceRemoved	= FSNUtils.RemoveAllWhiteSpaces(aliasname);								// 공백 모두 제거
		
		return s_propNameAlias.TryGetValue(spaceRemoved, out realname)? realname : aliasname;		// 이름을 검색하지 못했다면 원래 이름을 그대로 반환한다
	}

	//==============================================================================

	/// <summary>
	/// 초기화
	/// </summary>
	public FSNInGameSetting(bool needDefaultSetting = true)
	{
		if(needDefaultSetting)
		{
			CurrentFlowDirection	= FlowDirection.Up;
			BackwardFlowDirection	= FlowDirection.Down;
			SwipeWeight				= 0.30f;
			FontSize				= 36;
			ScreenCenterText		= false;
			StackTexts				= true;

			TextMarginTop			= 30;
			TextMarginBottom		= 30;
			TextMarginLeft			= 30;
			TextMarginRight			= 30;

			TextAlign				= TextAlignType.Left;
			TextLineSpacing			= 1.2f;
			ParagraphSpacing		= 10f;

			TransitionSpeedRatio	= 1f;
			TextTransitionTime		= 0.5f;

			PreventSaveAndLoadMenu	= false;
		}
	}

	/// <summary>
	/// 세팅 복제
	/// </summary>
	/// <returns></returns>
	public FSNInGameSetting Clone()
	{
		return MemberwiseClone() as FSNInGameSetting;
	}

	//===============================================================================


	private static Dictionary<FlowDirection, FlowDirection>	s_oppositeDirection;	// FlowDirection의 반대방향 딕셔너리 (검색용)
	private static Dictionary<FlowDirection, Vector2>		s_dirUnitVector;		// FlowDirection에 각각 해당하는 단위 벡터

	/// <summary>
	/// 디폴트 인게임 세팅 (static)
	/// </summary>
	public static FSNInGameSetting DefaultInGameSetting { get; private set; }

	static FSNInGameSetting()
	{
		DefaultInGameSetting	= new FSNInGameSetting();

		s_oppositeDirection	= new Dictionary<FlowDirection, FlowDirection>();
		s_oppositeDirection[FlowDirection.Down]		= FlowDirection.Up;
		s_oppositeDirection[FlowDirection.Up]		= FlowDirection.Down;
		s_oppositeDirection[FlowDirection.Left]		= FlowDirection.Right;
		s_oppositeDirection[FlowDirection.Right]	= FlowDirection.Left;

		s_dirUnitVector	= new Dictionary<FlowDirection, Vector2>();
		s_dirUnitVector[FlowDirection.Up]			= Vector2.up;
		s_dirUnitVector[FlowDirection.Down]			= -Vector2.up;
		s_dirUnitVector[FlowDirection.Left]			= -Vector2.right;
		s_dirUnitVector[FlowDirection.Right]		= Vector2.right;
	}

	/// <summary>
	/// 반대 방향 FlowDirection 구하기
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static FlowDirection GetOppositeFlowDirection(FlowDirection direction)
	{
		return s_oppositeDirection[direction];
	}

	/// <summary>
	/// 서로 반대방향인지
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="another"></param>
	/// <returns></returns>
	public static bool IsOppositeDirection(FlowDirection direction, FlowDirection another)
	{
		return s_oppositeDirection[direction] == another;
	}

	/// <summary>
	/// FlowDirection을 Unit Vector 로 바꾸기
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public static Vector2 GetUnitVectorFromFlowDir(FlowDirection direction)
	{
		return s_dirUnitVector[direction];
	}

	//============================================================================

	/// <summary>
	/// FSNInGameSetting 값의 상속 기능을 구현하기 위한 클래스
	/// </summary>
	public sealed class Chain : IInGameSetting
	{

		// Members

		IInGameSetting		m_parent;					// 부모 세팅
		Chain				m_parentChainVer;			// 부모 세팅, Chain 타입일 경우 이게 null 이 아님

		FSNInGameSetting	m_currentRaw;				// 현재 설정값

		HashSet<string>		m_overridProperties;		// 이 Chain 오브젝트에 고유하게 설정된 (오버라이드된) 프로퍼티 이름


		/// <summary>
		/// 상위 계층의 Chain일 구함. 만약 Chain이 없으면 null
		/// </summary>
		public Chain ParentChain
		{
			get { return m_parentChainVer; }
		}

		public Chain(IInGameSetting parent)
		{
			m_parent			= parent;
			m_parentChainVer	= parent as Chain;

			m_currentRaw		= new FSNInGameSetting(false);
			m_overridProperties	= new HashSet<string>();
		}

		/// <summary>
		/// 프로퍼티 set, Generic버전
		/// </summary>
		/// <typeparam name="valT"></typeparam>
		/// <param name="setterName"></param>
		/// <param name="value"></param>
		void SetProperty<valT>(string setterName, valT value)
		{
			var propName		= setterName.Substring(4);				// set_ 부분을 제거

			m_currentRaw.GetSetter<valT>(propName)(value);			// raw 데이터 부분에 setting
			m_overridProperties.Add(propName);						// 오버라이드되었다는 표시를 남겨둔다
		}

		/// <summary>
		/// 프로퍼티 get, Generic버전
		/// </summary>
		/// <typeparam name="valT"></typeparam>
		/// <param name="getterName"></param>
		/// <returns></returns>
		valT GetProperty<valT>(string getterName)
		{
			var propName		= getterName.Substring(4);							// get_ 부분을 제거

			Chain settingRef	= this;
			BaseInGameSetting.Getter<valT> getter	= null;


			// settingRef - this를 시작으로 parent 까지 순회한다.
			bool keepSearch	= true;
			do
			{
				if(settingRef.m_overridProperties.Contains(propName))				// * 현재 settingRef 에 해당 프로퍼티가 세팅된 것이 있다면
				{
					getter		= settingRef.m_currentRaw.GetGetter<valT>(propName);	// raw 에서 getter를 얻어온다
					keepSearch	= false;
				}
				else if(settingRef.m_parentChainVer == null)						// * 해당 프로퍼티 정의도 없지만 더이상 연결된 Chain도 없다면
				{
					getter		= (settingRef.m_parent as BaseInGameSetting).GetGetter<valT>(propName);	// parent에서 (아마도 FSNInGameSetting) getter를 얻어온다
					keepSearch	= false;
				}
				else
				{
					settingRef	= settingRef.m_parentChainVer;						// 아니라면 부모 Chain 에서 탐색 계속
				}
			}
			while(keepSearch);

			if(getter == null)
				Debug.LogError("Cannot find proper getter!");

			return getter();
		}


		public object GetProperty(string propName)
		{
			object retv		= null;

			var propInfo	= typeof(Chain).GetProperty(propName);
			if(propInfo == null)
			{
				Debug.LogError("Cannot find a property named " + propName);
			}
			else
			{
				retv		= propInfo.GetValue(this, null);
			}

			return retv;
		}

		public void SetProperty(string propName, object value)
		{
			var propInfo	= typeof(Chain).GetProperty(propName);
			if(propInfo == null)
			{
				Debug.LogError("Cannot find a property named " + propName);
			}
			else
			{
				propInfo.SetValue(this, value, null);
			}
		}

		public void SetPropertyByString(string propName, string strval)
		{
			var propInfo	= typeof(Chain).GetProperty(propName);
			if (propInfo == null)
			{
				Debug.LogError("Cannot find a property named " + propName);
			}
			else
			{
				object value	= FSNUtils.StringToValue(propInfo.PropertyType, strval);
				propInfo.SetValue(this, value, null);
			}
		}


		/// <summary>
		/// 연결된 모든 Chain과 최종 Parent까지 모두 복제한 사본을 리턴한다
		/// </summary>
		/// <returns></returns>
		public Chain CloneEntireChain()
		{
			// 1. 최종 parent를 찾는다 + Chain을 거꾸로 뒤집어 보관해둔다

			FSNInGameSetting root;
			Chain			search	= this;
			Stack<Chain> revChain	= new Stack<Chain>();	// Chain을 거꾸로 쌓은 것

			revChain.Push(search);							// 맨 첫번째 Chain 우선 쌓아두기

			while(search.m_parentChainVer != null)			// (Parent가 더이상 Chain이 아니게 될 때까지 루프)
			{
				search	= search.m_parentChainVer;			// 다음 Chain으로 점프
				revChain.Push(search);						// 그 다음 Chain 쌓기
			}
			root		= search.m_parent as FSNInGameSetting;	// Chain이 아닌 최종 root를 찾음


			// 2. root 위의 chain 부터 하나씩 클로닝한다

			IInGameSetting parent	= root.Clone();			// root 부터 클론해서 사용
			Chain curClone;
			do
			{												// 거꾸로 Chain을 쌓은 stack을 모두 소비할 때까지 반복 (= 원래 Chain의 최상층까지 루프)
				Chain origChain	= revChain.Pop();
				curClone		= CloneChain(origChain, parent);
				parent			= curClone;					// 현재 복제한 것이 다음번 parent

			} while(revChain.Count > 0);

			return curClone;								// 최상층 복제를 리턴
		}
		/// <summary>
		/// 새 Parent를 지정하여 체인 1개 복제
		/// </summary>
		/// <param name="original"></param>
		/// <param name="newParent"></param>
		/// <returns></returns>
		static Chain CloneChain(Chain original, IInGameSetting newParent)
		{
			Chain cloned		= new Chain(newParent);								// 지정한 parent로 클로닝

			cloned.m_currentRaw	= original.m_currentRaw.Clone();					// 설정값 구조체 복제
			cloned.m_overridProperties.UnionWith(original.m_overridProperties);		// 오바라이드 목록도 복제

			return cloned;
		}

		/// <summary>
		/// 모든 Chain의 변경 사항을 FSNInGameSetting 하나로 옮긴다
		/// </summary>
		/// <returns></returns>
		public FSNInGameSetting Freeze()
		{
			FSNInGameSetting frozen	= new FSNInGameSetting(false);

			var propNames			= FSNInGameSetting.PropertyNames;
			int count				= propNames.Length;
			for(int i = 0; i < count; i++)											// 모든 프로퍼티 이름에 대해서 순회
			{
				string propname		= propNames[i];
				var value			= typeof(Chain).GetProperty(propname).GetValue(this, null);	// 현재 Chain에서 얻어온 값을
				typeof(FSNInGameSetting).GetProperty(propname).SetValue(frozen, value, null);	// FSNInGameSetting 에 설정
			}

			return frozen;
		}

		//======================================================================

		// Properties

		// 단순히 SetProperty<> GetProperty<> 를 콜하는 것들임.

		public FSNInGameSetting.FlowDirection CurrentFlowDirection
		{
			get { return GetProperty<FSNInGameSetting.FlowDirection>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<FSNInGameSetting.FlowDirection>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public FSNInGameSetting.FlowDirection BackwardFlowDirection
		{
			get { return GetProperty<FSNInGameSetting.FlowDirection>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<FSNInGameSetting.FlowDirection>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float SwipeWeight
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float FontSize
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public bool ScreenCenterText
		{
			get { return GetProperty<bool>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<bool>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public bool StackTexts
		{
			get { return GetProperty<bool>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<bool>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextMarginLeft
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextMarginRight
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextMarginTop
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextMarginBottom
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public FSNInGameSetting.TextAlignType TextAlign
		{
			get { return GetProperty<FSNInGameSetting.TextAlignType>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<FSNInGameSetting.TextAlignType>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextLineSpacing
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float ParagraphSpacing
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TransitionSpeedRatio
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}

		public float TextTransitionTime
		{
			get { return GetProperty<float>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<float>(MethodBase.GetCurrentMethod().Name, value); }
		}
		public bool PreventSaveAndLoadMenu
		{
			get { return GetProperty<bool>(MethodBase.GetCurrentMethod().Name); }
			set { SetProperty<bool>(MethodBase.GetCurrentMethod().Name, value); }
		}
	}
}

