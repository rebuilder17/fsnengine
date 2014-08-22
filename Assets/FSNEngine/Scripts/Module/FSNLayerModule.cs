using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 레이어 하나의 오브젝트
/// </summary>
public class FSNLayerObject
{

}

/// <summary>
/// Snapshot의 특정 종류의 Layer를 담당해서 표시하는 모듈
/// </summary>
/// <typeparam name="ObjT">이 모듈이 컨트롤할 FSNLayerObject 타입</typeparam>
public abstract class FSNLayerModule<ObjT> : FSNModule
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
	public void OldElementOnlyTransition(FSNSnapshot.Layer toLayer, float ratio)
	{

	}

	/// <summary>
	/// 트랜지션 시작. 현재 레이어에 이미 존재하고 있던 오브젝트만 한정
	/// </summary>
	/// <param name="toLayer"></param>
	/// <param name="startRatio"></param>
	public void StartOldElementTransition(FSNSnapshot.Layer toLayer, float startRatio)
	{

	}

	/// <summary>
	/// 새 요소 트랜지션 시작. 새 레이어에만 존재하는 오브젝트 한정
	/// </summary>
	/// <param name="toLayer"></param>
	public void StartNewElementTransition(FSNSnapshot.Layer toLayer)
	{

	}
}
