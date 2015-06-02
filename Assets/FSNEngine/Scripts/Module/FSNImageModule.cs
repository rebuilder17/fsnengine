﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LayerObjects
{
	public abstract class ImageLayerObject : BaseObjectLayerObject<SnapshotElems.Image>
	{
		static readonly Vector2 c_pivot_center = new Vector2(0.5f, 0.5f);


		// Members

		Texture2D	m_texture;
		Vector2		m_pivot	= c_pivot_center;


		public Vector2 Pivot
		{
			get { return m_pivot; }
			set
			{
				m_pivot = value;
				UpdatePivot(value);
			}
		}

		public Texture2D Texture
		{
			get { return m_texture; }
			set
			{
				m_texture	= value;
				UpdateTexture(value);
			}
		}

		/// <summary>
		/// 텍스쳐 변경
		/// </summary>
		/// <param name="texture"></param>
		public abstract void UpdateTexture(Texture2D texture);

		/// <summary>
		/// 중점 변경
		/// </summary>
		/// <param name="pivot"></param>
		public abstract void UpdatePivot(Vector2 pivot);

		public override void SetStateFully(SnapshotElems.Image to)
		{
			base.SetStateFully(to);
			Pivot	= to.pivot;
			Texture	= to.texture;
		}


		public ImageLayerObject(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			
		}
	}
}

public abstract class FSNImageModule<ObjT> : FSNBaseObjectModule<Segments.Image, SnapshotElems.Image, ObjT>
	where ObjT : LayerObjects.ImageLayerObject
{
	public override string ModuleName
	{
		get
		{ 
			// Layer ID가 0번이라면 기본 모듈로 인식한다 (이름 : Image)
			// 아닐 경우, 뒤에 Layer ID가 추가로 붙는다 (예 : Image1)
			return FSNEngine.ModuleType.Image.ToString() + (m_layerID == (int)FSNSnapshot.PreDefinedLayers.Image_Default? "" : m_layerID.ToString()); 
		}
	}

	protected override void OnCreateElement(Segments.Image segment, FSNSnapshot.Layer layer, SnapshotElems.Image elemCreated)
	{
		base.OnCreateElement(segment, layer, elemCreated);

		var texture							= FSNResourceCache.Load<Texture2D>(FSNResourceCache.Category.Script, segment.texturePath);
		elemCreated.texture					= texture;
		// 실행 순서 문제 때문에 initial/finalstate의 텍스쳐를 직접 세팅해줘야함 (initial state가 이미 초기화된 상태, 값이 자동으로 복사되지 않음)
		elemCreated.InitialState.texture	= texture;
		elemCreated.FinalState.texture		= texture;

		var pivotVec						= Segments.Image.ConvertPivotPresetToVector(segment.pivot);
		elemCreated.pivot					= pivotVec;
		elemCreated.InitialState.pivot		= pivotVec;
		elemCreated.FinalState.pivot		= pivotVec;
	}
}
