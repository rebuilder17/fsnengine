using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	public class Text_UnityGUI : TextLayerObject, IOnGUI
	{
		public Text_UnityGUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{
			
		}

		/// <summary>
		/// OnGUI 루프
		/// </summary>
		public void OnGUI(MonoBehaviour context)
		{
			
		}

		protected override void UpdateText(string newText)
		{
			gameObject.guiText.text	= newText;
		}
	}
}

public class FSNTextModule_UnityGUI : FSNTextModule<LayerObjects.Text_UnityGUI>
{
	protected override LayerObjects.Text_UnityGUI MakeNewLayerObject()
	{
		GameObject newObj		= new GameObject("Text_UnityGUI");
		newObj.transform.parent	= ObjectRoot;
		newObj.AddComponent<GUIText>();

		return new LayerObjects.Text_UnityGUI(this, newObj);
	}

	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, Segments.TextSegment nextSeg)
	{
		throw new System.NotImplementedException();
	}

	void OnGUI()
	{
		if(!Application.isPlaying)
			return;

		foreach(var obj in AllObjects)
		{
			obj.OnGUI(this);
		}
	}
}
