using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace LayerObjects
{
	public class Text_NewUI : TextLayerObject
	{
		// Members

		Text			m_text;
		RectTransform	m_rectTrans;


		public Text_NewUI(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{
			m_text				= gameObj.AddComponent<Text>();
			m_rectTrans			= m_text.rectTransform;

			m_rectTrans.pivot	= Vector2.zero;
		}

		protected override void UpdatePosition(Vector3 position)
		{
			// Z 값은 어쩌지...?

			// 구 Transform은 사용하지 않는다.
			//base.UpdatePosition(position);

			//var screenDim			= FSNEngine.Instance.ScreenDimension;
			//m_realPos.x				= screenDim.x / 2 + (position.x);
			//m_realPos.y				= screenDim.y / 2 - (position.y);

			m_rectTrans.position	= position;
		}

		protected override void UpdateFontSize(float newSize)
		{
			m_text.fontSize	= (int)newSize;
		}

		protected override void UpdateText(string newText)
		{
			m_text.text	= newText;
		}

		protected override void UpdateColor(Color color)
		{
			//base.UpdateColor(color);
			m_text.color	= color;
		}
	}
}

public class FSNTextModule_NewUI : FSNTextModule<LayerObjects.Text_NewUI>
{
	// Members

	TextGenerator			m_textGenerator;		// 텍스트 크기 등을 알아보기 위한
	TextGenerationSettings	m_textGenSettings;


	public override void Initialize()
	{
		base.Initialize();

		// Text Generator 세팅
		m_textGenerator							= new TextGenerator();

		m_textGenSettings						= new TextGenerationSettings();
		m_textGenSettings.textAnchor			= TextAnchor.UpperLeft;
		m_textGenSettings.pivot					= Vector2.zero;
		m_textGenSettings.richText				= true;
		m_textGenSettings.fontStyle				= FontStyle.Normal;
		m_textGenSettings.horizontalOverflow	= HorizontalWrapMode.Wrap;
		m_textGenSettings.verticalOverflow		= VerticalWrapMode.Overflow;
	}

	protected override LayerObjects.Text_NewUI MakeNewLayerObject()
	{
		GameObject newObj		= new GameObject("Text_NewUI");
		newObj.transform.parent	= ObjectRoot;

		return new LayerObjects.Text_NewUI(this, newObj);
	}

	public override Vector2 CalculateTextSize(string text, IInGameSetting setting)
	{
		m_textGenSettings.fontSize	= (int)setting.FontSize;
		// TODO : 여백도 고려해야함
		m_textGenSettings.generationExtents	= new Vector2(FSNEngine.Instance.ScreenXSize, 1f);

		m_textGenerator.Populate(text, m_textGenSettings);
		var textRect	= m_textGenerator.rectExtents;
		return new Vector2(textRect.width, textRect.height);
	}
}
