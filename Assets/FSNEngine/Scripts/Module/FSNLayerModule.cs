using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 레이어 하나의 오브젝트
/// </summary>
public abstract class FSNLayerObject
{
	// Members

	Vector3		m_position;		// 현재 스냅샷에서의 위치 (트랜지션 거치기 전)
	Color		m_color;		// 색조
	float		m_alpha	= 1f;	// 트랜지션 알파 (유저 컨트롤 아님, Color에 곱해짐)

	GameObject	m_object;		// 이 FSNLayerObject가 맞물린 GameObject
	Transform	m_trans;		// Transform 캐시


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


	Color		m_finalColor;
	/// <summary>
	/// 알파까지 곱해진 최종 컬러
	/// </summary>
	protected Color FinalColor
	{
		get
		{
			m_finalColor.r	= m_color.r;
			m_finalColor.g	= m_color.g;
			m_finalColor.b	= m_color.b;
			m_finalColor.a	= m_color.a * m_alpha;

			return m_finalColor;
		}
	}


	public FSNLayerObject(GameObject realObject)
	{
		m_object	= realObject;
		m_trans		= m_object.transform;
	}

	/// <summary>
	/// 
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
}

/// <summary>
/// Snapshot의 특정 종류의 Layer를 담당해서 표시하는 모듈
/// </summary>
/// <typeparam name="ObjT">이 모듈이 컨트롤할 FSNLayerObject 타입</typeparam>
public abstract class FSNLayerModule<ElmT, ObjT> : FSNModule
	where ElmT : FSNSnapshot.IElement
	where ObjT : FSNLayerObject
{
	// Members


	/// <summary>
	/// 오브젝트 딕셔너리. Snapshot 상의 Element ID 가 키값
	/// </summary>
	private Dictionary<int, ObjT>	m_objectDict	= new Dictionary<int,ObjT>();



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
	/// 현재 존재하는 오브젝트들만 toLayer 의 상태에 맞춰 트랜지션. (자동재생 아님)
	/// 다음으로 넘기기 위해 Swipe하는 도중에 화면에 보여지는 상태.
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="ratio">트랜지션 비율. 0 : 현재 상태 그대로, 1 : 완전히 toLayer 상태로</param>
	/// <param name="backward">진행 반대 방향으로 swipe를 한 경우에는 false</param>
	public void OldElementOnlyTransition(FSNSnapshot.Layer toLayer, float ratio, bool backward)
	{

	}

	/// <summary>
	/// 트랜지션 시작. 현재 레이어에 이미 존재하고 있던 오브젝트만 한정
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="startRatio">시작 트랜지션 비율</param>
	/// <param name="backward">진행 반대 방향으로 swipe를 한 경우에는 false</param>
	public void StartOldElementTransition(FSNSnapshot.Layer toLayer, float startRatio, bool backward)
	{

	}

	/// <summary>
	/// 새 요소 트랜지션 시작. 새 레이어에만 존재하는 오브젝트 한정
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="backward">진행 반대 방향으로 swipe를 한 경우에는 false</param>
	public void StartNewElementTransition(FSNSnapshot.Layer toLayer, bool backward)
	{
		// Object한테 직접 Snapshot Elem 을 넘겨줘서 자기가 보고 트랜지션하게 할 것인가, 아니면 일일히 속성값을 줄것인가....
	}
}
