using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LayerObjects
{
	/// <summary>
	/// Image 계통 레이어 오브젝트 기반
	/// </summary>
	/// <typeparam name="ImageElem"></typeparam>
	public abstract class BaseObjectLayerObject<ImageElem> : FSNLayerObject<ImageElem>
		where ImageElem : SnapshotElems.ObjectBase<ImageElem>, new()
	{
		public BaseObjectLayerObject(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{

		}
	}
}

/// <summary>
/// 이미지 모듈 베이스. 이미지와 비슷한 동작을 하는 모든 모듈의 부모 클래스로 사용 가능
/// </summary>
public abstract class FSNBaseObjectModule<SegT, ElemT, ObjT> : FSNProcessModule<SegT, ElemT, ObjT>
	where SegT : Segments.Object
	where ElemT : SnapshotElems.ObjectBase<ElemT>, new()
	where ObjT : LayerObjects.BaseObjectLayerObject<ElemT>
	
{
	// constants

	const string		c_customDataName	= "ObjectList";


	// Members

	///// <summary>
	///// 이름 => 오브젝트 UID 딕셔너리
	///// </summary>
	//Dictionary<string, int>	m_lookupDict	= new Dictionary<string, int>();

	static bool AddToLookupDict(string name, ElemT elem, FSNSnapshot.Layer layer)
	{
		var nameDict = layer.GetCustomData(c_customDataName) as Dictionary<string, int>;
		if(nameDict == null)
		{
			nameDict	= new Dictionary<string,int>();
			//layer.CustomData	= nameDict;
			layer.SetCustomData(c_customDataName, nameDict);
		}

		if (nameDict.ContainsKey(name))
			return false;
		else
		{
			nameDict[name]	= elem.UniqueID;
			return true;
		}
	}

	static void RemoveFromLookupDict(string name, FSNSnapshot.Layer layer)
	{
		var nameDict = layer.GetCustomData(c_customDataName) as Dictionary<string, int>;
		if(nameDict == null)
		{
			nameDict	= new Dictionary<string,int>();
			//layer.CustomData	= nameDict;
			layer.SetCustomData(c_customDataName, nameDict);
		}

		nameDict.Remove(name);
	}

	/// <summary>
	/// 오브젝트 이름으로 UID 찾기
	/// </summary>
	/// <param name="name"></param>
	/// <param name="uid"></param>
	/// <returns></returns>
	protected static bool FindUIDFromLookupDict(string name, out int uid, FSNSnapshot.Layer layer)
	{
		var nameDict = layer.GetCustomData(c_customDataName) as Dictionary<string, int>;
		if(nameDict == null)
		{
			nameDict	= new Dictionary<string,int>();
			//layer.CustomData	= nameDict;
			layer.SetCustomData(c_customDataName, nameDict);
		}

		if(!nameDict.ContainsKey(name))
		{
			uid = 0;
			return false;
		}
		else
		{
			uid	= nameDict[name];
			return true;
		}
	}


	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, params FSNProcessModuleCallParam[] callParams)
	{
		FSNSnapshot.Layer newLayer	= curLayer.Clone();
		if(curLayer.GetCustomData(c_customDataName) != null)		// 이름 dictionary 카피
		{
			newLayer.SetCustomData(c_customDataName, new Dictionary<string, int>(curLayer.GetCustomData(c_customDataName) as Dictionary<string, int>));
		}

		foreach(var callParam in callParams)
		{
			if(callParam.segment.type == FSNScriptSequence.Segment.Type.Object)
			{
				var objSeg	= callParam.segment as SegT;

				switch(objSeg.command)
				{
					case Segments.Object.CommandType.Create:
						CreateElement(objSeg, newLayer);
						break;

					case Segments.Object.CommandType.Remove:
						RemoveElement(objSeg, newLayer);
						break;

					case Segments.Object.CommandType.SetInitial:
						SetElementInitial(objSeg, newLayer);
						break;

					case Segments.Object.CommandType.SetFinal:
						SetElementFinal(objSeg, newLayer);
						break;

					case Segments.Object.CommandType.SetKey:
						SetElement(objSeg, newLayer);
						break;

					case Segments.Object.CommandType.Custom:
						ProcessCustomElementCommand(objSeg, newLayer);
						break;
				}
			}
			else if(callParam.segment.type == FSNScriptSequence.Segment.Type.Control)
			{
				var controlSeg	= callParam.segment as Segments.Control;
				if(controlSeg.controlType == Segments.Control.ControlType.SwipeOption)	// 선택지 커맨드가 왔을 때는 모든 오브젝트에 분기점 포인트를 만들어둬야한다.
				{
					// TODO : 진짜 필요한가?;
				}
				else if(controlSeg.controlType == Segments.Control.ControlType.Clear)	// Clear 명령에 반응
				{
					foreach (var rawelem in newLayer.Elements)							// finalstate가 세팅되지 않은 오브젝트에 한해서 디폴트 세팅
					{
						var elem	= rawelem as ElemT;
						AutoSetFinalState(elem);
					}

					newLayer = new FSNSnapshot.Layer();									// 완전히 새로운 레이어로 교체

					// TODO : 힘들게 복제한 레이어를 버리는 구조임. 최적화 여지가 있음.
				}
			}
			else
			{

			}
		}

		// TODO : call 처리 이후, snapshot 마무리
		OnAfterGenerateNextLayerImage(newLayer);

		return newLayer;
	}

	/// <summary>
	/// 레이어에 후처리 필요할 때
	/// </summary>
	/// <param name="newLayer"></param>
	protected virtual void OnAfterGenerateNextLayerImage(FSNSnapshot.Layer newLayer)
	{

	}

	//========================================================================

	/// <summary>
	/// Segment에 실제로 세팅된 값들만 elem에 세팅하기
	/// </summary>
	/// <param name="elem"></param>
	/// <param name="seg"></param>
	protected virtual void SetElemBySegProperties(ElemT elem, SegT seg)
	{
		foreach(var name in seg.PropertyNames)
		{
			switch(name)
			{
				case Segments.Object.c_property_Position:
					elem.Position	= seg.position;
					break;

				case Segments.Object.c_property_Scale:
					elem.Scale		= seg.scale;
					break;

				case Segments.Object.c_property_Rotation:
					elem.Rotate		= seg.rotation;
					break;

				case Segments.Object.c_property_Color:
					elem.Color		= seg.color;
					break;

				case Segments.Object.c_property_Alpha:
					elem.Alpha		= seg.alpha;
					break;

				case Segments.Object.c_property_Transition:
					elem.TransitionTime	= seg.transition;
					break;
			}
		}
	}

	/// <summary>
	/// 계산되지 않은 이전 상태까지 계산한다
	/// </summary>
	protected void CalculateStates(ElemT elem)
	{
		LinkedList<ElemT> elems	= new LinkedList<ElemT>();	// 처리되지 않은 elem 리스트

		elems.AddFirst(elem);								// 첫번재 elem 세팅
		ElemT current	= elem.ClonedFrom;

		while(current != null)								// clone된 순서를 쭉 쫓아가면서, 오래된 것이 앞에 오도록 리스트에 추가한다.
		{
			elems.AddFirst(current);

			if(current.motionState != SnapshotElems.ObjectBase<ElemT>.State.NotCalculated)	// 처리가 이미 된 elem을 만난 경우에는 break한다.
				break;

			current		= current.ClonedFrom as ElemT;
		}

		float elemCount			= (float)elems.Count;		// 전체 갯수
		int needProcessedCount	= elems.Count - 2;			// 처리해야될 갯수 (맨 앞, 맨 뒤는 제외해야하므로 -2)

		if (needProcessedCount <= 0)						// * 처리할 게 없으면 리턴
			return;

		ElemT first				= elems.First.Value;
		ElemT last				= elems.Last.Value;
		var curNode				= elems.First.Next;
		for(int i = 0; i < needProcessedCount; i++)			// 중간 노드마다 Lerp해줌
		{
			float t				= (float)(i + 1) / (elemCount - 1);
			curNode.Value.LerpBetweenElems(first, last, t);

			curNode.Value.motionState	= SnapshotElems.ObjectBase<ElemT>.State.Calculated;	// "계산됨" 상태로

			curNode				= curNode.Next;
		}
	}

	/// <summary>
	/// Final state가 세팅되지 않은 오브젝트에 한해서 FinalState를 적절하게 세팅해준다
	/// </summary>
	/// <param name="elem"></param>
	private void AutoSetFinalState(ElemT elem)
	{
		if(!elem.finalStateSet)					// Final State가 하나도 세팅되지 않은 경우, 기본값부터 세팅
		{
			elem.CopyDataTo(elem.FinalState);	// (마지막 설정값 그대로 알파만 0)
			elem.FinalState.Alpha	= 0;
			elem.finalStateSet		= true;
		}
	}

	//========================================================================

	/// <summary>
	/// 오브젝트 생성 커맨드 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	void CreateElement(SegT segment, FSNSnapshot.Layer layer)
	{
		{
			int uid;
			if(FindUIDFromLookupDict(segment.objectName, out uid, layer))	// 이미 해당 이름으로 오브젝트가 존재한다면 에러
			{
				Debug.LogError("there is already a SnapshotElem named " + segment.objectName);
			}
		}

		var newElem			= new ElemT();

		newElem.Alpha		= 1;
		newElem.Color		= Color.white;
		newElem.TransitionTime	= 1;
		SetElemBySegProperties(newElem, segment);				// 세팅하기
		newElem.MakeItUnique();

		var initialState	= newElem.InitialState as ElemT;	// Inital State 기본값 주기 - 시작 세팅에서 알파만 0
		initialState.Alpha	= 0;
		initialState.TransitionTime	= 1;
		SetElemBySegProperties(initialState, segment);

		newElem.motionState	= SnapshotElems.ObjectBase<ElemT>.State.MotionKey; // Key로 지정

		layer.AddElement(newElem);
		AddToLookupDict(segment.objectName, newElem, layer);	// 오브젝트 이름 등록

		OnCreateElement(segment, layer, newElem);				// 추가 동작 실행
	}
	/// <summary>
	/// 오브젝트 생성 커맨드, 추가 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	/// <param name="elemCreated"></param>
	protected virtual void OnCreateElement(SegT segment, FSNSnapshot.Layer layer, ElemT elemCreated) { }

	/// <summary>
	/// 오브젝트 삭제 커맨드 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	void RemoveElement(SegT segment, FSNSnapshot.Layer layer)
	{
		int uid;
		if(!FindUIDFromLookupDict(segment.objectName, out uid, layer))	// 이름으로 uid 찾기
		{
			Debug.LogError("cannot find SnapshotElem named " + segment.objectName);
		}

		var elem	= layer.GetElement(uid) as ElemT;
		//if(!elem.finalStateSet)										// Final State가 하나도 세팅되지 않은 경우, 기본값부터 세팅
		//{
		//	elem.CopyDataTo(elem.FinalState);						// (마지막 설정값 그대로 알파만 0)
		//	elem.FinalState.Alpha	= 0;
		//	elem.finalStateSet		= true;
		//}
		AutoSetFinalState(elem);									// Final State가 하나도 세팅되지 않은 경우, 기본값부터 세팅

		SetElemBySegProperties(elem, segment);						// 마지막 설정값들 세팅
		elem.motionState			= SnapshotElems.ObjectBase<ElemT>.State.MotionKey;

		OnRemoveElement(segment, layer, elem);						// 추가 동작 실행
		
		CalculateStates(elem);										// 지금까지 좌표값들 보간

		RemoveFromLookupDict(segment.objectName, layer);			// 제거
		layer.RemoveElement(uid);
	}
	/// <summary>
	/// 오브젝트 삭제 커맨드, 추가 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	/// <param name="elemToBeRemoved"></param>
	protected virtual void OnRemoveElement(SegT segment, FSNSnapshot.Layer layer, ElemT elemToBeRemoved) { }

	/// <summary>
	/// 오브젝트 세팅 커맨드
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	void SetElement(SegT segment, FSNSnapshot.Layer layer)
	{
		int uid;
		if(!FindUIDFromLookupDict(segment.objectName, out uid, layer))	// 이름으로 uid 찾기
		{
			Debug.LogError("cannot find SnapshotElem named " + segment.objectName);
		}

		var elem	= layer.GetElement(uid) as ElemT;		
		SetElemBySegProperties(elem, segment);						// 설정값들 세팅
		elem.motionState			= SnapshotElems.ObjectBase<ElemT>.State.MotionKey;

		CalculateStates(elem);										// 지금까지 좌표값들 보간

		OnSetElement(segment, layer, elem);							// 추가 동작
	}
	/// <summary>
	/// 오브젝트 세팅, 추가 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	/// <param name="elemToSet"></param>
	protected virtual void OnSetElement(SegT segment, FSNSnapshot.Layer layer, ElemT elemToSet) { }

	/// <summary>
	/// 오브젝트 초기 세팅
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	void SetElementInitial(SegT segment, FSNSnapshot.Layer layer)
	{
		int uid;
		if(!FindUIDFromLookupDict(segment.objectName, out uid, layer))	// 이름으로 uid 찾기
		{
			Debug.LogError("cannot find SnapshotElem named " + segment.objectName);
		}

		var elem	= layer.GetElement(uid) as ElemT;
		SetElemBySegProperties(elem.InitialState as ElemT, segment);// 설정값들 세팅

		OnSetElementInitial(segment, layer, elem);					// 추가동작
	}
	/// <summary>
	/// 오브젝트 초기 세팅, 추가 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	/// <param name="elemToSet"></param>
	protected virtual void OnSetElementInitial(SegT segment, FSNSnapshot.Layer layer, ElemT elemToSet) { }

	/// <summary>
	/// 오브젝트 파괴 세팅
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	void SetElementFinal(SegT segment, FSNSnapshot.Layer layer)
	{
		int uid;
		if(!FindUIDFromLookupDict(segment.objectName, out uid, layer))	// 이름으로 uid 찾기
		{
			Debug.LogError("cannot find SnapshotElem named " + segment.objectName);
		}

		var elem	= layer.GetElement(uid) as ElemT;
		if(!elem.finalStateSet)										// Final State가 하나도 세팅되지 않은 경우, 기본값부터 세팅
		{
			elem.CopyDataTo(elem.FinalState);						// (마지막 설정값 그대로 알파만 0)
			elem.FinalState.Alpha	= 0;
			elem.finalStateSet		= true;
		}
		SetElemBySegProperties(elem.FinalState as ElemT, segment);	// 마지막 설정값들 세팅

		OnSetElementFinal(segment, layer, elem);					// 추가 동작
	}
	/// <summary>
	/// 오브젝트 파괴 세팅, 추가 처리
	/// </summary>
	/// <param name="segment"></param>
	/// <param name="layer"></param>
	/// <param name="elemToSet"></param>
	protected virtual void OnSetElementFinal(SegT segment, FSNSnapshot.Layer layer, ElemT elemToSet) { }

	public virtual void ProcessCustomElementCommand(SegT segment, FSNSnapshot.Layer layer)
	{
	}
}
