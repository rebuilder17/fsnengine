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


		public Image_NewUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{
			m_image				= gameObj.AddComponent<RawImage>();
			m_rectTrans			= m_image.rectTransform;

			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			m_rectTrans.pivot	= Vector2.one / 2f;
		}

		///// <summary>
		///// OnGUI 루프
		///// </summary>
		//public void OnGUI(MonoBehaviour context)
		//{
		//	var colorBackup	= GUI.color;

		//	GUI.color		= m_realCol;
		//	var rect		= new Rect(m_realPos.x, m_realPos.y, m_realTex.width, m_realTex.height);
		//	GUI.DrawTexture(rect, m_realTex, ScaleMode.StretchToFill, true);

		//	GUI.color		= colorBackup;
		//}

		public override void UpdateTexture(Texture2D texture)
		{
			m_image.texture		= texture;

			// texture 크기에 맞추기, Pivot 설정하기
			var size			= new Vector2(texture.width, texture.height);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, texture.width);
			m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, texture.height);
		}

		protected override void UpdatePosition(Vector3 position)
		{
			//float toRealRatio	= Screen.height / FSNEngine.Instance.ScreenYSize;	// 가상 좌표를 실제 스크린 좌표로 변환
			
			//m_realPos.x			= Screen.width / 2 + (position.x * toRealRatio);
			//m_realPos.y			= Screen.height / 2 - (position.y * toRealRatio);

			//var screenDim		= FSNEngine.Instance.ScreenDimension;
			//m_realPos.x			= screenDim.x / 2 + (position.x);
			//m_realPos.y			= screenDim.y / 2 - (position.y);
			m_rectTrans.localPosition	= position;
		}

		protected override void UpdateColor(Color color)
		{
			//m_realCol	= color;
			m_image.color	= color;
		}
	}
}

public class FSNImageModule_NewUI : FSNImageModule<LayerObjects.Image_NewUI>
{
	public override void Initialize()
	{

	}

	protected override LayerObjects.Image_NewUI MakeNewLayerObject()
	{
		GameObject newObj	= new GameObject("Image_NewUI");
		var lobj			= new LayerObjects.Image_NewUI(this, newObj);
		newObj.transform.SetParent(ObjectRoot, false);

		return lobj;
	}
}
