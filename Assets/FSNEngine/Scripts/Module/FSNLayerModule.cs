using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 레이어 하나의 오브젝트
/// </summary>
public abstract class FSNLayerObject<ElmT>
	where ElmT : FSNSnapshot.IElement
{
	public delegate void KillDelegate(int uid, FSNLayerObject<ElmT> self);

	// Members

	Vector3		m_position;		// 현재 스냅샷에서의 위치 (트랜지션 거치기 전)
	Color		m_color;		// 색조
	float		m_alpha	= 1f;	// 트랜지션 알파 (유저 컨트롤 아님, Color에 곱해짐)

	Vector3		m_scale	= Vector3.one;	// 스케일
	Vector3		m_rotate;		// 회전 (Euler각)


	GameObject	m_object;		// 이 FSNLayerObject가 맞물린 GameObject
	Transform	m_trans;		// Transform 캐시
	FSNModule	m_module;		// 이 오브젝트를 생성한 모듈
	FSNCoroutineComponent m_coComp; // 코루틴 컴포넌트
	bool        m_trCoroutineLock   = false;	// 코루틴이 여러 개 걸리더라도 한 번에 하나만 실행하도록 하기 위한 플래그

	int			m_uId;			// Unique ID
	KillDelegate	m_killedDel;	// 오브젝트 Kill 시 호출할 델리게이트


	/// <summary>
	/// 이 오브젝트를 생성한 모듈
	/// </summary>
	protected FSNModule ParentModule
	{
		get { return m_module; }
	}

	/// <summary>
	/// 이 오브젝트가 붙잡고 있는 GameObject
	/// </summary>
	protected GameObject gameObject
	{
		get { return m_object; }
	}

	protected Transform transform
	{
		get { return m_trans; }
	}

	protected Vector3 Position
	{
		get { return m_position; }
		set
		{
			m_position	= value;
			UpdatePosition(m_position);
		}
	}

	protected Vector3 Scale
	{
		get { return m_scale; }
		set
		{
			m_scale		= value;
			UpdateScale(value);
		}
	}

	protected Vector3 Rotate
	{
		get { return m_rotate; }
		set
		{
			m_rotate	= value;
			UpdateRotate(value);
		}
	}

	protected Color Color
	{
		get { return m_color; }
		//set
		//{
		//	m_color	= value;
		//	UpdateColor(FinalColor);
		//}
	}

	protected float Alpha
	{
		get { return m_alpha; }
		//set
		//{
		//	m_alpha	= value;
		//	UpdateColor(FinalColor);
		//}
	}

	protected void SetColorAndAlpha(Color color, float alpha)
	{
		m_color	= color;
		m_alpha	= alpha;
		UpdateColor(FinalColor);
	}


	Color		m_finalColor;
	/// <summary>
	/// 알파까지 곱해진 최종 컬러
	/// </summary>
	protected Color FinalColor
	{
		get
		{
			m_finalColor	= CalculateFinalColor(m_color, m_alpha);
			return m_finalColor;
		}
	}

	protected static Color CalculateFinalColor(Color color, float alpha)
	{
		color.a *= alpha;
		return color;
	}


	public FSNLayerObject(FSNModule parent, GameObject realObject, IInGameSetting setting)
	{
		m_object	= realObject;
		m_trans		= m_object.transform;
		m_coComp	= FSNCoroutineComponent.GetFromGameObject(m_object);
		m_module	= parent;
	}

	//============================================================================

	/// <summary>
	/// 위치 업데이트
	/// </summary>
	/// <param name="position"></param>
	protected virtual void UpdatePosition(Vector3 position)
	{
		m_trans.localPosition	= position;
	}

	/// <summary>
	/// * color에 트랜지션 alpha까지 미리 합쳐져있음
	/// </summary>
	/// <param name="color"></param>
	protected virtual void UpdateColor(Color color)
	{
		//
	}

	/// <summary>
	/// 스케일 업데이트
	/// </summary>
	/// <param name="scale"></param>
	protected virtual void UpdateScale(Vector3 scale)
	{
		m_trans.localScale	= scale;
	}

	/// <summary>
	/// 회전 업데이트
	/// </summary>
	/// <param name="rotate"></param>
	protected virtual void UpdateRotate(Vector3 rotate)
	{
		m_trans.localRotation	= Quaternion.Euler(rotate);
	}

	/// <summary>
	/// to 의 상태와 현재 상태를 ratio 비율만큼 섞는다. 현재 상태에 적용하지는 않는다.
	/// </summary>
	/// <param name="to"></param>
	/// <param name="ratio"></param>
	public virtual void TransitionWith(ElmT to, float ratio)
	{
		ratio		= TimeRatioFunction(ratio);	// 시간 곡선 적용

		var trPos	= Vector3.Lerp(m_position, to.Position, ratio);
		var trColor	= Color.Lerp(m_color, to.Color, ratio);
		var trAlpha	= Mathf.Lerp(m_alpha, to.Alpha, ratio);
		var trScale	= Vector3.Lerp(m_scale, to.Scale, ratio);
		var trRotate= Vector3.Lerp(m_rotate, to.Rotate, ratio);

		UpdatePosition(trPos);
		UpdateColor(CalculateFinalColor(trColor, trAlpha));
		UpdateScale(trScale);
		UpdateRotate(trRotate);
	}

	/// <summary>
	/// 현재 상태에 to 내용을 완전히 적용. 오브젝트를 맨 처음 생성했을 시에도 호출된다.
	/// </summary>
	/// <param name="to"></param>
	public virtual void SetStateFully(ElmT to)
	{
		Position	= to.Position;
		//Color		= to.Color;
		//Alpha		= to.Alpha;
		SetColorAndAlpha(to.Color, to.Alpha);
		Scale		= to.Scale;
		Rotate		= to.Rotate;
	}

	/// <summary>
	/// 트랜지션 애니메이션 시작
	/// </summary>
	public void DoTransition(ElmT to, float startRatio, float duration, bool killAfterTransition)
	{
		m_coComp.StartCoroutine(Transition_co(to, startRatio, duration, killAfterTransition));
	}

	public IEnumerator Transition_co(ElmT to, float startRatio, float duration, bool killOrSet)
	{
		while (m_trCoroutineLock)									// 코루틴 대기
			yield return null;
		m_trCoroutineLock   = true;										// 코루틴 락 걸기

		float startTime	= Time.time;								// 시작 시간 기록
		float elapsed;
		while((elapsed = Time.time - startTime) <= duration)			// 지속시간동안 매 프레임마다 루프, 각 시점마다 진행율에 따라서 트랜지션
		{

			//float t		= TimeRatioFunction(elapsed / duration);	// TODO : Transition 애니메이션 시 t 곡선 커스터마이징 가능하게
			float t			= elapsed / duration;
			TransitionWith(to, Mathf.Lerp(startRatio, 1, t));
			yield return null;
		}

		if(killOrSet)												// * true일 경우 애니메이션 종료 후 kill, 아니면 내부 값까지 완전히 다음 상태로 변경
		{
			Kill();
		}
		else
		{
			SetStateFully(to);
		}

		m_trCoroutineLock   = false;								// 코루틴 락 해제
    }

	//============================================================================

	public void ConenctKillEvent(KillDelegate del, int uId)
	{
		m_uId		= uId;
		m_killedDel	= del;
	}

	/// <summary>
	/// GameObject 를 포함하여 클린업
	/// </summary>
	public virtual void Kill()
	{
		GameObject.Destroy(gameObject);
		if(m_killedDel != null)
			m_killedDel(m_uId, this);
	}

	//============================================================================

	/// <summary>
	/// 트랜지션 시의 T 곡선 함수. TransitionWith 에서 쓰기 위한 것.
	/// 기본형 TransitionWith 에 이미 적용되어있으며, 오버라이드해서 구현할 시에는 따로 적용해줘야한다.
	/// (파라미터로 들어오는 ratio에는 이 함수가 적용되어있지 않다.)
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	protected static float TimeRatioFunction(float t)
	{
		return Mathf.Pow(t, 0.7f);
	}
}

/// <summary>
/// (interface) Snapshot의 특정 종류의 Layer를 담당해서 표시하는 모듈
/// </summary>
public interface IFSNLayerModule
{
	void OldElementOnlyTransition(FSNSnapshot.Layer toLayer, float ratio, bool backward);
	float StartTransition(FSNSnapshot.Layer toLayer, IInGameSetting nextSetting, float startRatioForOlds, bool backward);
	int LayerID { get; }
}

/// <summary>
/// Snapshot의 특정 종류의 Layer를 담당해서 표시하는 모듈
/// </summary>
/// <typeparam name="ObjT">이 모듈이 컨트롤할 FSNLayerObject 타입</typeparam>
public abstract class FSNLayerModule<ElmT, ObjT> : FSNModule, IFSNLayerModule
	where ElmT : class, FSNSnapshot.IElement
	where ObjT : FSNLayerObject<ElmT>
{
	// Properties

	[SerializeField]
	protected int	m_layerID;												// 이 모듈이 나타내는 레이어 ID
	[SerializeField]
	Transform		m_objectRootTransform;									// LayerObject가 배치될 루트


	// Members


	/// <summary>
	/// 오브젝트 딕셔너리. Snapshot 상의 Element ID 가 키값
	/// </summary>
	Dictionary<int, ObjT>	m_objectDict	= new Dictionary<int,ObjT>();

	FSNSnapshot.Layer		m_curLayerRef	= FSNSnapshot.Layer.Empty;		// 현재 표시중인 레이어 레퍼런스

	FSNSnapshot.Layer		m_lastTargetLayerRef	= null;					// 가장 최근에 비교한 트랜지션 타겟 레이어
	FSNSnapshot.Layer.Match	m_lastTargetLayerDiff;							// 가장 최근에 비교한 내역

	bool					m_useTransitionDelay	= true;					// 트랜지션 시 delay 시간을 사용할지 여부


	/// <summary>
	/// 오브젝트가 배치될 루트
	/// </summary>
	public Transform ObjectRoot
	{
		get
		{
			if(m_objectRootTransform == null)								// 루트 트랜스폼이 지정되지 않은 경우, 자기 자신의 것을 대입
				m_objectRootTransform	= transform;
			return m_objectRootTransform;
		}
	}

	/// <summary>
	/// 모든 오브젝트 리스트
	/// </summary>
	protected ICollection<ObjT> AllObjects
	{
		get { return m_objectDict.Values; }
	}

	/// <summary>
	/// 모듈이 표시할 레이어 ID
	/// </summary>
	public int LayerID { get { return m_layerID; } }

	/// <summary>
	/// 트랜지션 시 트랜지션 시간만큼 Delay를 걸지 여부. 기본값은 True.
	/// False가 되면 SetTransition 의 리턴값이 0으로 된다.
	/// </summary>
	public bool UseTransitionDelay
	{ 
		get
		{
			return m_useTransitionDelay;
		}
		protected set
		{
			m_useTransitionDelay	= value;
		}
	}

	//=============================================================================

	/// <summary>
	/// 새 레이어 오브젝트 인스턴스 생성
	/// </summary>
	/// <returns></returns>
	protected abstract ObjT MakeNewLayerObject(ElmT element, IInGameSetting setting);

	/// <summary>
	/// 레이어 오브젝트 구하기
	/// </summary>
	/// <param name="elementID"></param>
	/// <returns></returns>
	public ObjT GetLayerObject(int elementID)
	{
		ObjT retv;
		m_objectDict.TryGetValue(elementID, out retv);
		return retv;
	}

	/// <summary>
	/// 레이어 오브젝트를 추가하고, 해당 스테이트로 초기화한다
	/// </summary>
	/// <param name="element"></param>
	/// <param name="backward"></param>
	ObjT AddNewLayerObject(ElmT element, IInGameSetting nextSetting)
	{
		ObjT newObj		= MakeNewLayerObject(element, nextSetting);
		newObj.ConenctKillEvent(OnObjectKilled, element.UniqueID);
		newObj.SetStateFully(element);
		m_objectDict[element.UniqueID]	= newObj;

		return newObj;
	}



	// 트랜지션 관련


	/// <summary>
	/// 타겟 레이어 비교. 이전에 비교한 레이어라면 캐싱된 것을 사용, 새로운 타겟이라면 캐시 업데이트.
	/// </summary>
	/// <param name="target"></param>
	void UpdateTargetLayerDiff(FSNSnapshot.Layer target)
	{
		if(m_lastTargetLayerRef != target)
		{
			m_lastTargetLayerDiff	= m_curLayerRef.CompareAndReturnElements(target);
			m_lastTargetLayerRef	= target;
		}
	}

	/// <summary>
	/// 이터레이션을 위한 유틸리티
	/// </summary>
	/// <param name="uIdArray"></param>
	/// <param name="action"></param>
	static void IterateUIDArray(int[] uIdArray, System.Action<int> action)
	{
		int count	= uIdArray.Length;
		for(int i = 0; i < count; i++)
		{
			action(uIdArray[i]);
		}
	}
	void IterateMatchingUIDs	(System.Action<int> action)	{ IterateUIDArray(m_lastTargetLayerDiff.Matching, action); }
	void IterateOnlyInThisUIDs	(System.Action<int> action) { IterateUIDArray(m_lastTargetLayerDiff.OnlyInThis, action); }
	void IterateOnlyInOtherUIDs	(System.Action<int> action) { IterateUIDArray(m_lastTargetLayerDiff.OnlyInOther, action); }

	/// <summary>
	/// 현재 존재하는 오브젝트들만 toLayer 의 상태에 맞춰 트랜지션. (자동재생 아님)
	/// 다음으로 넘기기 위해 Swipe하는 도중에 화면에 보여지는 상태.
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="ratio">트랜지션 비율. 0 : 현재 상태 그대로, 1 : 완전히 toLayer 상태로</param>
	/// <param name="backward">진행 반대 방향으로 swipe를 한 경우에는 false</param>
	public void OldElementOnlyTransition(FSNSnapshot.Layer toLayer, float ratio, bool backward)
	{
		UpdateTargetLayerDiff(toLayer);											// 비교 업데이트

		// *** 유지되는 오브젝트들
		IterateMatchingUIDs((int uId) =>
		{
			m_objectDict[uId].TransitionWith(toLayer.GetElement(uId) as ElmT, ratio);
		});

		// *** 다음에 사라지는 오브젝트들
		IterateOnlyInThisUIDs((int uId) =>
		{
			var currentElem	= toLayer.GetRemovedElementOrNull(uId)				// (정방향) 다음 레이어에 지워지는 해당 오브젝트에 관한 정보가 있다면 얻어오고
								?? m_curLayerRef.GetElement(uId);				// 아니면 현재 레이어의 해당 오브젝트를 얻는다

			var finalElem	= (!backward)?	currentElem.GenericFinalState		// 정방향일 경우 마지막 스테이트로 움직인 뒤 소멸,
										:	currentElem.GenericInitialState;	// 역방향일 경우 최초 스테이트로 움직인 뒤 소멸해야한다.

			m_objectDict[uId].TransitionWith(finalElem as ElmT, ratio);
		});
	}


	/// <summary>
	/// 트랜지션 애니메이션 시작.
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="startRatioForOlds">기존 오브젝트들은 해당 비율부터 애니메이션 시작</param>
	/// <param name="backward">진행 반대 방향으로 swipe를 한 경우에는 false</param>
	/// <returns>트랜지션이 모두 끝나는데 걸리는 시간</returns>
	public float StartTransition(FSNSnapshot.Layer toLayer, IInGameSetting nextSetting, float startRatioForOlds, bool backward)
	{
		UpdateTargetLayerDiff(toLayer);											// 비교 업데이트
		float longestDuration	= 0f;											// 트랜지션 중 제일 오래걸리는 것의 시간

		// *** 유지되는 오브젝트들
		IterateMatchingUIDs((int uId) =>
		{
			var elem		= toLayer.GetElement(uId) as ElmT;
			float trTime	= elem.TransitionTime / nextSetting.TransitionSpeedRatio; // 전환속도 비율 적용

			m_objectDict[uId].DoTransition(elem, startRatioForOlds, trTime, false);

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});

		// *** 다음에 사라지는 오브젝트들
		IterateOnlyInThisUIDs((int uId) =>
		{
			var currentElem	= m_curLayerRef.GetElement(uId);					// 다음 레이어에 없어질 현재 레이어 객체

			var refelem		= toLayer.GetRemovedElementOrNull(uId)				// (정방향) 다음 레이어에 지워지는 해당 오브젝트에 관한 정보가 있다면 이것을 사용
								?? currentElem;									// 아니면 현재 레이어의 해당 오브젝트를 사용하여 finalState를 구한다
			var finalElem	= (!backward)?	refelem.GenericFinalState			// 정방향일 경우 마지막 스테이트로 움직인 뒤 소멸,
										:	refelem.GenericInitialState;		// 역방향일 경우 최초 스테이트로 움직인 뒤 소멸해야한다.
			float trTime	= finalElem.TransitionTime / nextSetting.TransitionSpeedRatio; // 전환속도 비율 적용

			m_objectDict[uId].DoTransition(finalElem as ElmT, startRatioForOlds, trTime, true);

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});

		// *** 다음에 처음 등장하는 오브젝트들
		IterateOnlyInOtherUIDs((int uId) =>
		{
			var currentElem	= toLayer.GetElement(uId);							// 다음 레이어의 해당 객체

			var refelem		= m_curLayerRef.GetRemovedElementOrNull(uId)		// (역방향) 현재 레이어에 지워진 오브젝트의 정보가 있다면 그것을 사용,
								?? currentElem;									// 아니면 다음 레이어의 오브젝트를 참조해서 InitialState를 구한다
			var initialElem	= backward?	refelem.GenericFinalState
									:	refelem.GenericInitialState;			// 역방향이면 finalState, 정방향이면 InitialState 로 초기세팅한다
			float trTime	= initialElem.TransitionTime / nextSetting.TransitionSpeedRatio;// 현재 상태로 transition하지만 시간값은 최초 상태값에 지정된 걸 사용한다.

			var newobj		= AddNewLayerObject(initialElem as ElmT, nextSetting);
			newobj.DoTransition(currentElem as ElmT, 0, trTime, false);

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});


		// NOTE : 트랜지션이 완전히 끝난 뒤에 레이어를 교체해야할 수도 있다. 이슈가 생기면 그때 바꾸자...
		m_curLayerRef	= toLayer;												// 현재 레이어를 트랜지션 타겟 레이어로 교체.

		OnLayerTransitionStart(toLayer);										// 이벤트 호출


		return m_useTransitionDelay? longestDuration : 0;						// 트랜지션 딜레이를 사용하지 않는다면 딜레이 시간은 0으로
	}

	/// <summary>
	/// 오브젝트가 Kill되었을 때
	/// </summary>
	/// <param name="uId"></param>
	protected void OnObjectKilled(int uId, FSNLayerObject<ElmT> self)
	{
		var valueInDict	= m_objectDict[uId];
		if (valueInDict == self)
			m_objectDict.Remove(uId);	// 자기 자신인지 확실히 체크한 뒤 딕셔너리에서 삭제
	}

	/// <summary>
	/// 특정 레이어로 트랜지션하는 애니메이션이 시작될 때 호출됨
	/// </summary>
	/// <param name="toLayer"></param>
	protected virtual void OnLayerTransitionStart(FSNSnapshot.Layer toLayer) { }
}
