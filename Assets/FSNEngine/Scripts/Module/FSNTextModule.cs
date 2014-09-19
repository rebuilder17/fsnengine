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

	const int			c_textLife	= 5;							// 텍스트가 얼마나 유지되는지

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
		FSNSnapshot.Layer newLayer	= null;

		if(nextSeg.type == FSNSequence.Segment.Type.Text)										// ** 텍스트 세그먼트 처리 **
		{
			newLayer	= curLayer.Clone();

			// TODO : 상하좌우 여백, 정렬 등도 따져야함


			var textSeg				= nextSeg as Segments.TextSegment;							// 타입 변환
			var newTextSize			= CalculateTextSize(textSeg.Text, nextSetting.FontSize);	// 텍스트 영역 크기 미리 구하기


			// 새 텍스트 엘레먼트 세팅

			var newTextElem				= new SnapshotElems.Text();

			newTextElem.text			= textSeg.Text;
			newTextElem.fontSize		= nextSetting.FontSize;
			newTextElem.Color			= Color.white;//TODO
			newTextElem.Alpha			= 1;
			newTextElem.TransitionTime	= 1;//TODO
			newTextElem.MakeItUnique();

			newTextElem.InitialState.Alpha	= 0;
			newTextElem.FinalState.Alpha	= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)

			//Vector3 dirVec		= FSNInGameSetting.GetUnitVectorFromFlowDir(nextSetting.CurrentFlowDirection);
			Vector2 screenDim	= FSNEngine.Instance.ScreenDimension;	// (계산용) 화면 크기
			Vector3 fadeinpos;
			switch(nextSetting.CurrentFlowDirection)					// 흐름 방향에 따라 시작 위치를 지정해준다
			{
			case FSNInGameSetting.FlowDirection.Up:
				fadeinpos	= new Vector3(-screenDim.x / 2, -screenDim.y / 2);
				break;

			case FSNInGameSetting.FlowDirection.Down:
				fadeinpos	= new Vector3(-screenDim.x / 2, screenDim.y / 2 + newTextSize.y);
				break;

			case FSNInGameSetting.FlowDirection.Right:
				fadeinpos	= new Vector3(-screenDim.x / 2 - newTextSize.x, 0);
				break;

			case FSNInGameSetting.FlowDirection.Left:
				fadeinpos	= new Vector3(screenDim.x / 2, 0);
				break;

			default:
				throw new System.Exception("HUH???");
			}
			newTextElem.Position				= fadeinpos;			// 나중에 일괄적으로 이동시킬 것이기 때문에 시작 위치랑 화면 밖 위치를 같게 설정한다
			newTextElem.InitialState.Position	= fadeinpos;

			newLayer.AddElement(newTextElem);							// 텍스트 엘리멘트 추가
			PushTextsToDirection(newLayer, nextSetting.CurrentFlowDirection, newTextSize);	// 텍스트 일괄적으로 해당 방향으로 밀기
		}
		else
		{
			// TODO : 현재는 다른 명령이 들어오면 무조건 클리어하는 것으로. 복잡해지면 확장 필요

			newLayer	= curLayer.Clone();
			ClearTextsToDirection(newLayer, nextSetting.CurrentFlowDirection);
		}

		return newLayer;
	}

	/// <summary>
	/// Layer 안에 들어있는 텍스트들을 특정 방향으로 모두 밀어낸다. 알파값도 변경. 수명이 다 된 것은 제거 처리.
	/// </summary>
	/// <param name="layer">변경할 레이어 (이미 복제된 상태여야함)</param>
	/// <param name="direction"></param>
	/// <param name="newTextSize"></param>
	private void PushTextsToDirection(FSNSnapshot.Layer layer, FSNInGameSetting.FlowDirection direction, Vector2 newTextSize)
	{
		Vector2 dirVec		= FSNInGameSetting.GetUnitVectorFromFlowDir(direction);
		Vector3 transVec	= Vector2.Scale(newTextSize, dirVec);						// 이동할 벡터 양
		
		foreach(var uId in layer.UniqueIDList)
		{
			var textElem	= layer.GetElement(uId) as SnapshotElems.Text;

			int elemAge		= textElem.ChainedParentCount;
			if(elemAge < c_textLife)													// 텍스트가 아직 살아있어야하는 경우
			{
				textElem.Alpha		= (float)(c_textLife - elemAge) / (float)c_textLife;
				textElem.Position	= textElem.Position + transVec;
			}
			else
			{																			// 텍스트가 죽어야하는 경우
				textElem.FinalState.Position	= textElem.Position + transVec;
				layer.RemoveElement(uId);
			}
		}
	}

	/// <summary>
	/// 텍스트들을 일정 방향으로 모두 밀어내며 삭제한다.
	/// </summary>
	/// <param name="layer"></param>
	/// <param name="direction"></param>
	private void ClearTextsToDirection(FSNSnapshot.Layer layer, FSNInGameSetting.FlowDirection direction)
	{
		Vector3 screenHalf	= FSNEngine.Instance.ScreenDimension * 0.5f;				// 화면 크기 절반
		Vector3 dirVec		= FSNInGameSetting.GetUnitVectorFromFlowDir(direction);		// 흐름 방향 벡터

		foreach(var uId in layer.UniqueIDList)
		{
			var textElem	= layer.GetElement(uId) as SnapshotElems.Text;

			textElem.FinalState.Position	= textElem.Position + Vector3.Scale(screenHalf, dirVec);	// 화면 절반 거리만큼 해당 방향으로 이동
			layer.RemoveElement(uId);
		}
	}
}

