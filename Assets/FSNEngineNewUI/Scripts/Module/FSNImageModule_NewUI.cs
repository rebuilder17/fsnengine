using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace LayerObjects
{
	public class Image_NewUI : ImageLayerObject
	{
		// Members

		RectTransform	m_rectTrans;
		RawImage		m_image;


		public Image_NewUI(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			m_image				= innerGO.AddComponent<RawImage>();	// 안쪽의 오브젝트에 추가하기
			m_rectTrans			= m_image.rectTransform;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			m_rectTrans.pivot	= Vector2.one / 2f;
		}

		public override void UpdatePivot(Vector2 pivot)
		{
			m_rectTrans.pivot	= pivot;
		}

		public override void UpdateTexture(Texture2D texture)
		{
			m_image.texture		= texture;

			// texture 크기에 맞추기, Pivot 설정하기
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, texture.width);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, texture.height);
		}

		protected override void UpdatePosition(Vector3 position)
		{
			m_rectTrans.localPosition	= position;
		}

		protected override void UpdateScale(Vector3 scale)
		{
			m_rectTrans.localScale		= scale;
		}

		protected override void UpdateRotate(Vector3 rotate)
		{
			m_rectTrans.localRotation	= Quaternion.Euler(rotate);
		}

		protected override void UpdateColor(Color color)
		{
			m_image.color	= color;
		}
	}
}

public class FSNImageModule_NewUI : FSNImageModule<LayerObjects.Image_NewUI>
{
	public override void Initialize()
	{

	}

	protected override LayerObjects.Image_NewUI MakeNewLayerObject(SnapshotElems.Image elem, IInGameSetting setting)
	{
		GameObject newObj	= new GameObject("Image_NewUI");
		newObj.layer		= gameObject.layer;
		var lobj			= new LayerObjects.Image_NewUI(this, newObj, setting);
		newObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}
}
