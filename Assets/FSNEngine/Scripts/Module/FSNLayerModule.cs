using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 레이어 하나의 오브젝트
/// </summary>
public abstract class FSNLayerObject<ElmT>
	where ElmT : FSNSnapshot.IElement
{
	// Members

	Vector3		m_position;		// 현재 스냅샷에서의 위치 (트랜지션 거치기 전)
	Color		m_color;		// 색조
	float		m_alpha	= 1f;	// 트랜지션 알파 (유저 컨트롤 아님, Color에 곱해짐)

	GameObject	m_object;		// 이 FSNLayerObject가 맞물린 GameObject
	Transform	m_trans;		// Transform 캐시
	FSNModule	m_module;		// 이 오브젝트를 생성한 모듈
	FSNCoroutineComponent m_coComp;	// 코루틴 컴포넌트


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

	protected Color Color
	{
		get { return m_color; }
		set
		{
			m_color	= value;
			UpdateColor(FinalColor);
		}
	}

	protected float Alpha
	{
		get { return m_alpha; }
		set
		{
			m_alpha	= value;
			UpdateColor(FinalColor);
		}
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


	public FSNLayerObject(FSNModule parent, GameObject realObject)
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
	/// to 의 상태와 현재 상태를 ratio 비율만큼 섞는다. 현재 상태에 적용하지는 않는다.
	/// </summary>
	/// <param name="to"></param>
	/// <param name="ratio"></param>
	public virtual void TransitionWith(ElmT to, float ratio)
	{
		var trPos	= Vector3.Lerp(m_position, to.Position, ratio);
		var trColor	= Color.Lerp(m_color, to.Color, ratio);
		var trAlpha	= Mathf.Lerp(m_alpha, to.Alpha, ratio);

		UpdatePosition(trPos);
		UpdateColor(CalculateFinalColor(trColor, trAlpha));
	}

	/// <summary>
	/// 현재 상태에 to 내용을 완전히 적용.
	/// </summary>
	/// <param name="to"></param>
	public virtual void SetStateFully(ElmT to)
	{
		Position	= to.Position;
		Color		= to.Color;
		Alpha		= to.Alpha;
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
		float startTime	= Time.time;								// 시작 시간 기록
		float elapsed;
		while((elapsed = Time.time - startTime) <= duration)		// 지속시간동안 매 프레임마다 루프
		{
			TransitionWith(to, Mathf.Lerp(startRatio, 1, elapsed / duration));
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
	}

	//============================================================================

	/// <summary>
	/// GameObject 를 포함하여 클린업
	/// </summary>
	public virtual void Kill()
	{
		GameObject.Destroy(gameObject);
	}
}

/// <summary>
/// (interface) Snapshot의 특정 종류의 Layer를 담당해서 표시하는 모듈
/// </summary>
public interface IFSNLayerModule
{
	void OldElementOnlyTransition(FSNSnapshot.Layer toLayer, float ratio, bool backward);
	float StartTransition(FSNSnapshot.Layer toLayer, float startRatioForOlds, bool backward);
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

	//=============================================================================

	/// <summary>
	/// 새 레이어 오브젝트 인스턴스 생성
	/// </summary>
	/// <returns></returns>
	protected abstract ObjT MakeNewLayerObject();

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
	ObjT AddNewLayerObject(ElmT element)
	{
		ObjT newObj	= MakeNewLayerObject();
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
			var currentElem	= m_curLayerRef.GetElement(uId);
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
	public float StartTransition(FSNSnapshot.Layer toLayer, float startRatioForOlds, bool backward)
	{
		UpdateTargetLayerDiff(toLayer);											// 비교 업데이트
		float longestDuration	= 0f;											// 트랜지션 중 제일 오래걸리는 것의 시간

		// *** 유지되는 오브젝트들
		IterateMatchingUIDs((int uId) =>
		{
			var elem		= toLayer.GetElement(uId) as ElmT;
			float trTime	= elem.TransitionTime;

			m_objectDict[uId].DoTransition(elem, startRatioForOlds, trTime, false);

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});

		// *** 다음에 사라지는 오브젝트들
		IterateOnlyInThisUIDs((int uId) =>
		{
			var currentElem	= m_curLayerRef.GetElement(uId);
			var finalElem	= (!backward)?	currentElem.GenericFinalState		// 정방향일 경우 마지막 스테이트로 움직인 뒤 소멸,
										:	currentElem.GenericInitialState;	// 역방향일 경우 최초 스테이트로 움직인 뒤 소멸해야한다.
			float trTime	= finalElem.TransitionTime;

			m_objectDict[uId].DoTransition(finalElem as ElmT, startRatioForOlds, trTime, true);

			m_objectDict.Remove(uId);											// 딕셔너리에서 해당 오브젝트 제거

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});

		// *** 다음에 처음 등장하는 오브젝트들
		IterateOnlyInOtherUIDs((int uId) =>
		{
			var currentElem	= toLayer.GetElement(uId) as ElmT;
			var initialElem	= backward? currentElem.GenericFinalState : currentElem.GenericInitialState;	// 역방향이면 finalState, 정방향이면 InitialState 로 초기세팅한다
			float trTime	= initialElem.TransitionTime;						// 현재 상태로 transition하지만 시간값은 최초 상태값에 지정된 걸 사용한다.

			var newobj		= AddNewLayerObject(initialElem as ElmT);
			newobj.DoTransition(currentElem, 0, trTime, false);

			if(longestDuration < trTime) longestDuration = trTime;				// 제일 긴 트랜지션 시간 추적하기
		});


		// NOTE : 트랜지션이 완전히 끝난 뒤에 레이어를 교체해야할 수도 있다. 이슈가 생기면 그때 바꾸자...
		m_curLayerRef	= toLayer;												// 현재 레이어를 트랜지션 타겟 레이어로 교체.
		

		return longestDuration;
	}
}
