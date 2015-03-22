using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	public class Sound : BaseObjectLayerObject<SnapshotElems.Sound>
	{
		// Members

		AudioSource		m_asource;
		float			m_volume	= 1;
		float			m_panning	= 0;
		//bool			m_loop		= false;

		protected float Volume
		{
			get { return m_volume; }
			set
			{
				m_volume			= value;
				m_asource.volume	= value;
			}
		}

		protected float Panning
		{
			get { return m_panning; }
			set
			{
				m_panning			= value;
				m_asource.panStereo	= value;
			}
		}



		public Sound(FSNModule parent, GameObject realObject, IInGameSetting setting)
			: base (parent, realObject, setting)
		{
			m_asource	= realObject.AddComponent<AudioSource>();
			m_asource.spatialBlend	= 0;	// 완전한 2D 사운드로
		}


		protected override void UpdatePosition(Vector3 position)
		{
			// 아무것도 하지 않는다
			//base.UpdatePosition(position);
		}

		protected override void UpdateScale(Vector3 scale)
		{
			//base.UpdateScale(scale);
		}

		protected override void UpdateRotate(Vector3 rotate)
		{
			//base.UpdateRotate(rotate);
		}


		public override void SetStateFully(SnapshotElems.Sound to)
		{
			//base.SetStateFully(to);
			Volume	= to.volume;
			Panning	= to.panning;

			if (m_asource.clip == null)	// 사운드를 맨 처음 지정하는 경우
			{
				m_asource.clip	= to.clip;
				m_asource.loop	= to.looping;
			}
		}

		public override void TransitionWith(SnapshotElems.Sound to, float ratio)
		{
			//base.TransitionWith(to, ratio);

			m_asource.volume	= Mathf.Lerp(m_volume, to.volume, ratio);
			m_asource.panStereo	= Mathf.Lerp(m_panning, to.panning, ratio);
		}
	}
}

public class FSNSoundModule : FSNBaseObjectModule<Segments.Sound, SnapshotElems.Sound, LayerObjects.Sound>
{
	public override string ModuleName
	{
		get
		{
			return "Sound";
		}
	}

	public override void Initialize()
	{
		m_layerID	= (int)FSNSnapshot.PreDefinedLayers.Sound;	// ID 강제지정
		UseTransitionDelay	= false;							// Sound 계열은 딜레이를 사용하지 않는다
	}

	protected override LayerObjects.Sound MakeNewLayerObject(SnapshotElems.Sound element, IInGameSetting setting)
	{
		GameObject newObj	= new GameObject("Sound");
		var lobj			= new LayerObjects.Sound(this, newObj, setting);
		newObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}

	protected override void SetElemBySegProperties(SnapshotElems.Sound elem, Segments.Sound seg)
	{
		base.SetElemBySegProperties(elem, seg);

		if (seg.IsPropertySet(Segments.Sound.c_property_looping))
		{
			elem.looping	= seg.looping;
		}
	}

	protected override void OnCreateElement(Segments.Sound segment, FSNSnapshot.Layer layer, SnapshotElems.Sound elemCreated)
	{
		base.OnCreateElement(segment, layer, elemCreated);

		var clip							= FSNResourceCache.Load<AudioClip>(FSNResourceCache.Category.Script, segment.clipPath);
		elemCreated.clip					= clip;
		elemCreated.InitialState.clip		= clip;				// 실행 순서 문제 때문에 initial/finalstate의 텍스쳐를 직접 세팅해줘야함
		elemCreated.FinalState.clip			= clip;
	}
}
