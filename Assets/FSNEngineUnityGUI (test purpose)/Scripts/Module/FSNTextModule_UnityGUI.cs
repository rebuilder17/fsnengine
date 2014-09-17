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
			var oldFontSize	= GUI.skin.label.fontSize;
			GUI.skin.label.fontSize	= (int)FontSize;

			var size		= GUI.skin.label.CalcSize(new GUIContent(m_realStr));
			var oldColor	= GUI.color;
			GUI.color		= m_realCol;
			GUI.Label(new Rect(m_realPos.x, m_realPos.y, size.x, size.y), m_realStr);

			// 옛날 값으로 복구
			GUI.color		= oldColor;
			GUI.skin.label.fontSize	= oldFontSize;
		}

		protected override void UpdateText(string newText)
		{
			//gameObject.guiText.text	= newText;
			m_realStr	= newText;
		}

		protected override void UpdatePosition(Vector3 position)
		{
			//base.UpdatePosition(position);


			float toRealRatio	= Screen.height / FSNEngine.Instance.ScreenYSize;	// 가상 좌표를 실제 스크린 좌표로 변환
			
			m_realPos.x			= Screen.width / 2 + (position.x * toRealRatio);
			m_realPos.y			= Screen.height / 2 - (position.y * toRealRatio);
		}

		protected override void UpdateColor(Color color)
		{
			//base.UpdateColor(color);

			//gameObject.guiText.color	= color;
			m_realCol	= color;
		}

		protected override void UpdateFontSize(float newSize)
		{
			
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

	public override Vector2 CalculateTextSize(string text, float size)
	{
		// GUI 루프 바깥에서는 GUI 콜을 할 수가 없음. 따라서 임의로 값을 계산한다. (테스트용이니까 어차피 괜찮음...)

		int lineCount	= 0;
		int maxColCount	= 0;

		foreach(string line in text.Split('\n'))
		{
			lineCount++;

			int charCount	= line.Length;
			int curColCount	= 0;
			for(int i = 0; i < charCount; i++)
			{
				curColCount += (text[i] > 255)? 2 : 1;
			}

			maxColCount	= Mathf.Max(maxColCount, curColCount);
		}

		Vector2 finalSize;
		finalSize.y	= lineCount * size;
		finalSize.x	= (float)maxColCount * size * 0.5f;
		return finalSize;
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
