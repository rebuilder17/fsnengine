using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	public class GObject : BaseObjectLayerObject<SnapshotElems.GObject>
	{
		// Members
		FSNBaseGameObjectEventListener m_listener;

		protected override bool useInnerObject
		{
			get { return false; }		// 내부 오브젝트는 사용하지 않는다.
		}

		public GObject(FSNModule parent, GameObject outerGameObj, GameObject realGameObj, IInGameSetting setting)
			: base(parent, outerGameObj, setting)
		{
			var newObjTr			= realGameObj.transform;
			newObjTr.SetParent(outerGameObj.transform);
			newObjTr.localPosition  = Vector3.zero;
			newObjTr.localRotation  = Quaternion.identity;
			newObjTr.localScale     = Vector3.one;

			// 리스너 구하기 (null이어도 무관)
			m_listener      = realGameObj.GetComponent<FSNBaseGameObjectEventListener>();
		}

		protected override void UpdateColor(Color color)
		{
			if (m_listener != null)
				m_listener.OnUpdateColor(color);
		}
	}
}

public class FSNGameObjectModule : FSNBaseObjectModule<Segments.GObject, SnapshotElems.GObject, LayerObjects.GObject>
{
	public override string ModuleName
	{
		get
		{
			// Layer ID가 100번이라면 기본 모듈로 인식한다
			// 아닐 경우, 뒤에 Layer ID가 추가로 붙는다
			return "Object" + (m_layerID - (int)FSNSnapshot.PreDefinedLayers.Object_Default);
		}
	}

	protected override void OnCreateElement(Segments.GObject segment, FSNSnapshot.Layer layer, SnapshotElems.GObject elemCreated)
	{
		base.OnCreateElement(segment, layer, elemCreated);

		var prefab							= FSNResourceCache.Load<GameObject>(FSNResourceCache.Category.Script, segment.prefabPath);
		if (prefab == null)
		{
			Debug.LogErrorFormat("프리팹을 열 수 없습니다. : {0}", segment.prefabPath);
		}
		elemCreated.prefab					= prefab;
		elemCreated.InitialState.prefab		= prefab;					// 실행 순서 문제 때문에 initial/finalstate의 프리팹을 직접 세팅해줘야함
		elemCreated.FinalState.prefab		= prefab;
	}

	public override void Initialize()
	{
		m_layerID	+= (int)FSNSnapshot.PreDefinedLayers.Object_Default;	// 레이어 번호 강제지정. 100을 기본으로 하도록 설정한다
	}

	protected override LayerObjects.GObject MakeNewLayerObject(SnapshotElems.GObject elem, IInGameSetting setting)
	{
		GameObject prefab	= elem.prefab;
		GameObject outerObj = new GameObject("(Outer)");
		GameObject newObj	= Instantiate<GameObject>(prefab);
		var lobj			= new LayerObjects.GObject(this, outerObj, newObj, setting);
		outerObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}
}
