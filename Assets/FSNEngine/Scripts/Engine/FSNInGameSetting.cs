using UnityEngine;
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
	/// </summary>
	FSNInGameSetting.FlowDirection CurrentFlowDirection { get; }
	/// <summary>
	/// 반대 방향 구하기
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
}

/// <summary>
/// 게임 내 실행중 스크립트로 조작하는 세팅. 초기화 값이 곧 디폴트 세팅값
/// </summary>
public sealed class FSNInGameSetting : BaseInGameSetting
{
	// TODO : 세팅을 완전히 덮어쓰는 것이 아닌 일부만 세팅할 수 있게 하는 시스템이 필요함

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
	/// 초기화
	/// </summary>
	public FSNInGameSetting(bool needDefaultSetting = true)
	{
		if(needDefaultSetting)
		{
			CurrentFlowDirection	= FlowDirection.Up;
			BackwardFlowDirection	= FlowDirection.Down;
			SwipeWeight				= 0.3f;
			FontSize				= 18;
			ScreenCenterText		= false;
			StackTexts				= true;
		}
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


		public Chain(IInGameSetting parent)
		{
			m_parent			= parent;
			m_parentChainVer	= parent as Chain;

			m_currentRaw		= new FSNInGameSetting(false);
			m_overridProperties	= new HashSet<string>();
		}

		void SetProperty<valT>(string setterName, valT value)
		{
			var propName	= setterName.Substring(4);				// set_ 부분을 제거

			m_currentRaw.GetSetter<valT>(propName)(value);			// raw 데이터 부분에 setting
			m_overridProperties.Add(propName);						// 오버라이드되었다는 표시를 남겨둔다
		}

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

		//======================================================================

		// Properties

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
	}
}

