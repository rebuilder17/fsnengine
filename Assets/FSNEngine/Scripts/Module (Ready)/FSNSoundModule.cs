﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LayerObjects
{
	public class Sound : BaseObjectLayerObject<SnapshotElems.Sound>
	{
		// Members

		AudioSource		m_asource;
		float			m_volume	= 1;
		float			m_panning	= 0;

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
				m_asource.Play();
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
	const string		c_oneshotSoundData	= "OneShotSounds";


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

		foreach(var property in seg.PropertyNames)
		{
			switch(property)
			{
				case Segments.Sound.c_property_volume:
					elem.volume		= seg.volume;
					break;

				case Segments.Sound.c_property_panning:
					elem.panning	= seg.panning;
					break;

				case Segments.Sound.c_property_looping:
					elem.looping	= seg.looping;
					break;
			}
		}
	}

	protected override void OnCreateElement(Segments.Sound segment, FSNSnapshot.Layer layer, SnapshotElems.Sound elemCreated)
	{
		base.OnCreateElement(segment, layer, elemCreated);

		var clip							= FSNResourceCache.Load<AudioClip>(FSNResourceCache.Category.Script, segment.clipPath);
		elemCreated.clip					= clip;
		elemCreated.InitialState.clip		= clip;				// 실행 순서 문제 때문에 initial/finalstate의 텍스쳐를 직접 세팅해줘야함
		elemCreated.FinalState.clip			= clip;

		elemCreated.looping					= segment.looping;
		elemCreated.InitialState.looping	= segment.looping;
		elemCreated.FinalState.looping		= segment.looping;
	}

	public override void ProcessCustomElementCommand(Segments.Sound segment, FSNSnapshot.Layer layer)
	{
		//base.ProcessCustomElementCommand(segment, layer);

		// One-Shot 사운드에 대한 처리

		// 레이어에 one-shot 사운드 리스트가 없다면 생성
		var oneshotSounds	= layer.GetCustomData(c_oneshotSoundData) as List<Segments.Sound>;
		if (oneshotSounds == null)
		{
			oneshotSounds	= new List<Segments.Sound>();
			layer.SetCustomData(c_oneshotSoundData, oneshotSounds);
		}

		oneshotSounds.Add(segment);						// 세그먼트채로 집어넣는다.
	}

	protected override void OnLayerTransitionStart(FSNSnapshot.Layer toLayer)
	{
		//base.OnLayerTransitionStart(toLayer);

		var oneshotSounds	= toLayer.GetCustomData(c_oneshotSoundData) as List<Segments.Sound>;
		if(oneshotSounds != null)							// one-shot sound가 있는 경우에만, 트랜지션 시작시에 사운드 재생
		{
			foreach(var sound in oneshotSounds)				// 사운드마다 게임 오브젝트, 오디오소스 생성 등등...
			{
				var clip			= FSNResourceCache.Load<AudioClip>(FSNResourceCache.Category.Script, sound.clipPath);
				var go				= new GameObject("Sound oneshot");
				go.transform.SetParent(ObjectRoot, false);

				var source			= go.AddComponent<AudioSource>();
				source.volume		= sound.volume;
				source.panStereo	= sound.panning;
				source.PlayOneShot(clip);

				Destroy(go, clip.length + 0.1f);			// 오디오 재생 길이만큼만 게임 오브젝트 유지
			}
		}
	}
}
