using UnityEngine;
using System.Collections;

namespace LayerObjects
{
	public class Image_UnityGUI : ImageLayerObject, IOnGUI 
	{
		public Image_UnityGUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}

		/// <summary>
		/// OnGUI 루프
		/// </summary>
		public void OnGUI(MonoBehaviour context)
		{

		}

		public override void UpdateTexture(Texture2D texture)
		{
			throw new System.NotImplementedException();
		}
	}
}


public class FSNImageModule_UnityGUI : FSNImageModule<LayerObjects.Image_UnityGUI>
{
	public override void Initialize()
	{
		throw new System.NotImplementedException();
	}

	protected override LayerObjects.Image_UnityGUI MakeNewLayerObject()
	{
		throw new System.NotImplementedException();
	}
}
