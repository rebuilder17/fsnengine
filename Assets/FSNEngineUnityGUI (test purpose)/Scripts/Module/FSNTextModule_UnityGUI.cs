using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	public class Text_UnityGUI : TextLayerObject
	{
		public Text_UnityGUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}
	}
}

public class FSNTextModule_UnityGUI : FSNTextModule<LayerObjects.Text_UnityGUI>
{
	protected override LayerObjects.Text_UnityGUI MakeNewLayerObject()
	{
		throw new System.NotImplementedException();
	}

	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, Segments.TextSegment nextSeg)
	{
		throw new System.NotImplementedException();
	}
}
