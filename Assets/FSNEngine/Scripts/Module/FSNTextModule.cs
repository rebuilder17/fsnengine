using UnityEngine;
using System.Collections;


namespace LayerObjects
{
	/// <summary>
	/// Text 오브젝트에 해당하는 LayerObject
	/// </summary>
	public abstract class TextLayerObject : FSNLayerObject<SnapshotElems.Text>
	{
		public enum TextAlign
		{
			Left,
			Center,
			Right,
		}

		//=========================================================================


		string			m_text	= "";
		/// <summary>
		/// 텍스트
		/// </summary>
		public string Text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text	= value == null? "" : value;
				UpdateText(m_text);
			}
		}

		float			m_fontSize;

		public float FontSize
		{
			get
			{
				return m_fontSize;
			}
			set
			{
				m_fontSize	= value;
				UpdateFontSize(m_fontSize);
			}
		}

		/// <summary>
		/// 정렬
		/// </summary>
		public TextAlign Align	{get;set;}


		public TextLayerObject(FSNModule parent, GameObject gameObj)
			: base(parent, gameObj)
		{

		}

		/// <summary>
		/// 텍스트 업데이트
		/// </summary>
		/// <param name="newText"></param>
		protected abstract void UpdateText(string newText);

		/// <summary>
		/// 폰트 사이즈 업데이트
		/// </summary>
		/// <param name="newSize"></param>
		protected abstract void UpdateFontSize(float newSize);


		public override void SetStateFully(SnapshotElems.Text to)
		{
			base.SetStateFully(to);

			Text		= to.text;
			FontSize	= to.fontSize;
		}
	}
}


/// <summary>
/// 텍스트 모듈, 기본형
/// </summary>
public abstract class FSNTextModule<ObjT> : FSNProcessModule<Segments.TextSegment, SnapshotElems.Text, ObjT>
	where ObjT : LayerObjects.TextLayerObject
{
	public override string ModuleName
	{
		get { return FSNEngine.ModuleType.Text.ToString(); }
	}


	//==================================================================

	public override void Initialize()
	{
		m_layerID	= (int)FSNSnapshot.PreDefinedLayers.Text;		// 레이어 ID 강제지정
	}

	/// <summary>
	/// 텍스트 영역 크기 계산
	/// </summary>
	/// <param name="text"></param>
	/// <param name="size"></param>
	public abstract Vector2 CalculateTextSize(string text, float size);


	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, FSNSequence.Segment nextSeg, FSNInGameSetting nextSetting)
	{
		var newLayer	= curLayer.Clone();

		if(nextSeg.type == FSNSequence.Segment.Type.Text)
		{
			// TODO : 상하좌우 여백, 정렬 등도 따져야함


			var textSeg				= nextSeg as Segments.TextSegment;							// 타입 변환
			var newTextSize			= CalculateTextSize(textSeg.Text, nextSetting.FontSize);	// 텍스트 영역 크기 미리 구하기


			// 새 텍스트 엘레먼트 세팅

			var newTextElem			= new SnapshotElems.Text();

			newTextElem.text		= textSeg.Text;
			newTextElem.fontSize	= nextSetting.FontSize;
			newTextElem.Color		= Color.white;//TODO
			newTextElem.Alpha		= 1;
			newTextElem.TransitionTime	= 1;//TODO
			newTextElem.MakeItUnique();

			newTextElem.InitialState.Alpha	= 0;
			newTextElem.FinalState.Alpha	= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)

			Vector2 screenDim	= FSNEngine.Instance.ScreenDimension;	// (계산용) 화면 크기
			Vector3 startpos, fadeinpos;
			switch(nextSetting.CurrentFlowDirection)
			{
			case FSNInGameSetting.FlowDirection.Up:
				startpos	= new Vector3(-screenDim.x / 2, -screenDim.y / 2 + newTextSize.y);
				fadeinpos	= startpos;
				fadeinpos.y -= newTextSize.y;
				break;

			case FSNInGameSetting.FlowDirection.Down:
				startpos	= new Vector3(-screenDim.x / 2, screenDim.y / 2);
				fadeinpos	= startpos;
				fadeinpos.y += newTextSize.y;
				break;

			case FSNInGameSetting.FlowDirection.Right:
				startpos	= new Vector3(-screenDim.x / 2, 0);
				fadeinpos	= startpos;
				fadeinpos.x -= newTextSize.x;
				break;

			case FSNInGameSetting.FlowDirection.Left:
				startpos	= new Vector3(screenDim.x / 2 - newTextSize.x, 0);
				fadeinpos	= startpos;
				fadeinpos.x += newTextSize.x;
				break;

			default:
				throw new System.Exception("HUH???");
			}
			newTextElem.Position				= startpos;
			newTextElem.InitialState.Position	= fadeinpos;

			// (sample)
			var text02_0					= new SnapshotElems.Text();
			text02_0.text					= "으오와아아아앙";
			text02_0.fontSize				= 32;
			text02_0.Position				= new Vector3(50, 200);
			text02_0.Color					= Color.white;
			text02_0.Alpha					= 1;
			text02_0.TransitionTime			= 1;
			text02_0.MakeItUnique();

			text02_0.InitialState.Alpha		= 0;
			text02_0.InitialState.Position	= new Vector3(-50, 200);

			text02_0.FinalState.Alpha		= 0;
			text02_0.FinalState.Position	= new Vector3(600, 200);
		}

		return newLayer;
	}
}

