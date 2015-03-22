using UnityEngine;
using System.Collections;

namespace LayerObjects
{
	public class Image_UnityGUI : ImageLayerObject, IOnGUI 
	{
		Vector2 m_realPos;
		Color m_realCol;
		Texture2D m_realTex;

		public Image_UnityGUI(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
		{
			
		}

		/// <summary>
		/// OnGUI 루프
		/// </summary>
		public void OnGUI(MonoBehaviour context)
		{
			var colorBackup	= GUI.color;

			GUI.color		= m_realCol;
			var rect		= new Rect(m_realPos.x, m_realPos.y, m_realTex.width, m_realTex.height);
			GUI.DrawTexture(rect, m_realTex, ScaleMode.StretchToFill, true);

			GUI.color		= colorBackup;
		}

		public override void UpdateTexture(Texture2D texture)
		{
			m_realTex	= texture;
		}

		protected override void UpdatePosition(Vector3 position)
		{
			//float toRealRatio	= Screen.height / FSNEngine.Instance.ScreenYSize;	// 가상 좌표를 실제 스크린 좌표로 변환
			
			//m_realPos.x			= Screen.width / 2 + (position.x * toRealRatio);
			//m_realPos.y			= Screen.height / 2 - (position.y * toRealRatio);

			var screenDim		= FSNEngine.Instance.ScreenDimension;
			m_realPos.x			= screenDim.x / 2 + (position.x);
			m_realPos.y			= screenDim.y / 2 - (position.y);
		}

		protected override void UpdateColor(Color color)
		{
			m_realCol	= color;
		}
	}
}


public class FSNImageModule_UnityGUI : FSNImageModule<LayerObjects.Image_UnityGUI>
{
	public override void Initialize()
	{
		
	}

	protected override LayerObjects.Image_UnityGUI MakeNewLayerObject(SnapshotElems.Image elem, IInGameSetting setting)
	{
		GameObject newObj		= new GameObject("Image_UnityGUI");
		newObj.transform.parent	= ObjectRoot;
		
		return new LayerObjects.Image_UnityGUI(this, newObj, setting);
	}

	void OnGUI()
	{
		if(!Application.isPlaying)
			return;

		float scale	= (float)Screen.height / FSNEngine.Instance.ScreenYSize;
		GUI.matrix	= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));

		foreach(var obj in AllObjects)
		{
			obj.OnGUI(this);
		}
	}
}
