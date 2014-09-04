using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	public class Text_UnityGUI : TextLayerObject, IOnGUI
	{
		Vector2 m_realPos;
		string m_realStr;
		Color m_realCol;

		public Text_UnityGUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{
			//var module				= parent as FSNTextModule_UnityGUI;
			//gameObject.guiText.font	= module.font;
		}

		/// <summary>
		/// OnGUI 루프
		/// </summary>
		public void OnGUI(MonoBehaviour context)
		{
			var size		= GUI.skin.label.CalcSize(new GUIContent(m_realStr));
			var oldColor	= GUI.color;
			GUI.color		= m_realCol;
			GUI.Label(new Rect(m_realPos.x, m_realPos.y, size.x, size.y), m_realStr);
			GUI.color		= oldColor;

		}

		protected override void UpdateText(string newText)
		{
			//gameObject.guiText.text	= newText;
			m_realStr	= newText;
		}

		protected override void UpdatePosition(Vector3 position)
		{
			//base.UpdatePosition(position);
			Vector2 pixelPos;
			pixelPos.x		= position.x;
			//pixelPos.y		= Screen.height - position.y;
			pixelPos.y		= position.y;
			//gameObject.guiText.pixelOffset	= pixelPos;
			m_realPos	= pixelPos;
		}

		protected override void UpdateColor(Color color)
		{
			//base.UpdateColor(color);

			//gameObject.guiText.color	= color;
			m_realCol	= color;
		}
	}
}

public class FSNTextModule_UnityGUI : FSNTextModule<LayerObjects.Text_UnityGUI>
{
	// Properties

	[SerializeField]
	Font			m_font;				// 사용할 폰트


	/// <summary>
	/// Unity font
	/// </summary>
	public Font font
	{
		get { return m_font; }
	}

	public override void Initialize()
	{
		base.Initialize();
		
		//Debug.Log(GUI.skin.label.CalcSize(new GUIContent("HELLOOOO")));
	}

	protected override LayerObjects.Text_UnityGUI MakeNewLayerObject()
	{
		GameObject newObj		= new GameObject("Text_UnityGUI");
		newObj.transform.parent	= ObjectRoot;
		//newObj.AddComponent<GUIText>();
		
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

		GUI.skin.font	= m_font;

		foreach(var obj in AllObjects)
		{
			obj.OnGUI(this);
		}
	}
}
