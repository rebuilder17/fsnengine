using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LayerObjects
{
	public abstract class ImageLayerObject : BaseObjectLayerObject<SnapshotElems.Image>
	{
		// Members

		Texture2D m_texture;


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

		public override void SetStateFully(SnapshotElems.Image to)
		{
			base.SetStateFully(to);
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
			return FSNEngine.ModuleType.Image.ToString() + (m_layerID == 0? "" : m_layerID.ToString()); 
		}
	}

	protected override void OnCreateElement(Segments.Image segment, FSNSnapshot.Layer layer, SnapshotElems.Image elemCreated)
	{
		base.OnCreateElement(segment, layer, elemCreated);

		var texture							= FSNResourceCache.Load<Texture2D>(FSNResourceCache.Category.Script, segment.texturePath);
		elemCreated.texture					= texture;
		elemCreated.InitialState.texture	= texture;				// 실행 순서 문제 때문에 initial/finalstate의 텍스쳐를 직접 세팅해줘야함
		elemCreated.FinalState.texture		= texture;
	}
}
