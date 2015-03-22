using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
		public TextAlign Align { get; set; }


		public TextLayerObject(FSNModule parent, GameObject gameObj, IInGameSetting setting)
			: base(parent, gameObj, setting)
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
public abstract class FSNTextModule<ObjT> : FSNProcessModule<Segments.Text, SnapshotElems.Text, ObjT>
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
	public abstract Vector2 CalculateTextSize(string text, IInGameSetting setting);

	//=================================================================

	/// <summary>
	/// 가운데가 아닌 텍스트의 위치를 여백에 따라 보정
	/// </summary>
	/// <param name="textpos"></param>
	/// <param name="flow"></param>
	private static void ApplySideTextMargin(ref Vector3 textpos, IInGameSetting setting, FSNInGameSetting.FlowDirection flow)
	{
		//float xoffset	= setting.TextMarginLeft - setting.TextMarginRight;
		float yoffset	= setting.TextMarginBottom - setting.TextMarginTop;

		// 해당 사이드에서 여백만큼 떨어트리기
		switch(flow)
		{
			case FSNInGameSetting.FlowDirection.Up:
				textpos.y	+= setting.TextMarginBottom;
				textpos.x	+= setting.TextMarginLeft;
				break;
			case FSNInGameSetting.FlowDirection.Down:
				textpos.y	-= setting.TextMarginTop;
				textpos.x	+= setting.TextMarginLeft;
				break;
			case FSNInGameSetting.FlowDirection.Left:
				textpos.x	-= setting.TextMarginRight;
				textpos.y	+= yoffset;
				break;
			case FSNInGameSetting.FlowDirection.Right:
				textpos.x	+= setting.TextMarginLeft;
				textpos.y	+= yoffset;
				break;
		}
	}

	/// <summary>
	/// 가운데에 배치되는 텍스트의 위치를 여백에 따라 보정
	/// </summary>
	/// <param name="textpos"></param>
	/// <param name="setting"></param>
	private static void ApplyCenterTextMargin(ref Vector3 textpos, IInGameSetting setting)
	{
		float xoffset	= setting.TextMarginLeft - setting.TextMarginRight;
		float yoffset	= setting.TextMarginBottom - setting.TextMarginTop;

		textpos.x		+= xoffset;
		textpos.y		+= yoffset;
	}

	/// <summary>
	/// 텍스트 좌표를 가운데로 밀어내기. LastOption이나 가운데 텍스트 옵션 등에서 사용
	/// </summary>
	/// <param name="textpos"></param>
	/// <param name="setting"></param>
	private static void TextPositionToCenter(ref Vector3 textpos, Vector2 textSize, FSNInGameSetting.FlowDirection direction, IInGameSetting setting)
	{
		var oldPos	= textpos;
		switch(direction)
		{
			case FSNInGameSetting.FlowDirection.Up:
			case FSNInGameSetting.FlowDirection.Down:
				textpos.y	= textSize.y / 2f;
				ApplyCenterTextMargin(ref textpos, setting);
				textpos.x	= oldPos.x;
				break;

			case FSNInGameSetting.FlowDirection.Left:
			case FSNInGameSetting.FlowDirection.Right:
				textpos.x	= -textSize.x / 2f;
				ApplyCenterTextMargin(ref textpos, setting);
				textpos.y	= oldPos.y;
				break;
		}
	}

	//=================================================================

	public override FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, params FSNProcessModuleCallParam[] callParams)
	{
		FSNSnapshot.Layer newLayer	= curLayer.Clone();

		foreach(var callParam in callParams)
		{
			//Debug.Log("TextModule : " + callParam.segment.type.ToString());
			if(callParam.segment.type == FSNScriptSequence.Segment.Type.Text)	// ** 텍스트 세그먼트 처리 **
			{
				var textSeg	= callParam.segment as Segments.Text;				// 타입 변환

				switch(textSeg.textType)										// * 텍스트 종류에 따라 처리 분기
				{
					case Segments.Text.TextType.Normal:
						AddNormalText(newLayer, textSeg, callParam.setting);
						break;

					case Segments.Text.TextType.Clear:
						ClearTextsToDirection(newLayer, callParam.setting.CurrentFlowDirection);
						break;

					case Segments.Text.TextType.Options:
						ClearTextsToDirection(newLayer, callParam.setting.CurrentFlowDirection);	// 선택지 텍스트는 clear를 먼저 한 뒤에 표시
						ShowOptionTexts(newLayer, textSeg, callParam.setting);
						break;

					case Segments.Text.TextType.LastOption:
						AddLastOptionText(newLayer, textSeg, callParam.setting);
						break;
				}

			}
			else if(callParam.segment.type == FSNScriptSequence.Segment.Type.Control
				&& (callParam.segment as Segments.Control).controlType == Segments.Control.ControlType.Clear)	// Clear 명령에 반응
			{
				ClearTextsToDirection(newLayer, callParam.setting.CurrentFlowDirection);
			}
		}

		return newLayer;
	}

	private void AddNormalText(FSNSnapshot.Layer layer, Segments.Text textSeg, IInGameSetting setting)
	{
		// TODO : 상하좌우 여백, 정렬 등도 따져야함

		var newTextSize				= CalculateTextSize(textSeg.text, setting);		// 텍스트 영역 크기 미리 구하기


		// 새 텍스트 엘레먼트 세팅

		var newTextElem				= new SnapshotElems.Text();

		newTextElem.text			= textSeg.text;
		newTextElem.fontSize		= setting.FontSize;
		newTextElem.Color			= Color.white;//TODO
		newTextElem.Alpha			= 1;
		newTextElem.TransitionTime	= 1;//TODO
		newTextElem.MakeItUnique();

		newTextElem.InitialState.Alpha	= 0;
		newTextElem.FinalState.Alpha	= 0;

		// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)

		Vector2 screenDim	= FSNEngine.Instance.ScreenDimension;					// (계산용) 화면 크기
		Vector3 fadeinpos;
		switch(setting.CurrentFlowDirection)										// 흐름 방향에 따라 시작 위치를 지정해준다
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
		ApplySideTextMargin(ref fadeinpos, setting, setting.CurrentFlowDirection);	// 여백 적용

		newTextElem.Position				= fadeinpos;							// 나중에 일괄적으로 이동시킬 것이기 때문에 시작 위치랑 화면 밖 위치를 같게 설정한다
		newTextElem.InitialState.Position	= fadeinpos;

		if (!setting.StackTexts || setting.ScreenCenterText)						// 텍스트를 쌓지 않는 경우, 기존 텍스트를 전부 Clear한다.
																					// 가운데텍스트 모드일 경우에도 (텍스트 쌓기 여부와는 상관 없이) 기존 텍스트를 지운다.
		{
			ClearTextsToDirection(layer, setting.CurrentFlowDirection);
		}

		if(setting.ScreenCenterText)												// * 가운데 텍스트일 경우,
		{
			//PushTextsToDirection(layer, setting.CurrentFlowDirection, newTextSize);	// 기존 텍스트를 일괄적으로 해당 방향으로 밀기
			layer.AddElement(newTextElem);											// 텍스트 엘리멘트 추가

			var posToCenter		= newTextElem.Position;								
			TextPositionToCenter(ref posToCenter, newTextSize, setting.CurrentFlowDirection, setting);// 텍스트 중앙으로 움직이기
			newTextElem.Position	= posToCenter;
		}
		else
		{																			// * 일반 텍스트
			layer.AddElement(newTextElem);											// 텍스트 엘리멘트 추가
			PushTextsToDirection(layer, setting.CurrentFlowDirection, newTextSize, setting.ParagraphSpacing);	// 텍스트 일괄적으로 해당 방향으로 밀기
		}
		
	}


	private void ShowOptionTexts(FSNSnapshot.Layer layer, Segments.Text textSeg, IInGameSetting setting)
	{
		// TODO : 상하좌우 여백, 정렬 등도 따져야함

		Vector2 screenDim	= FSNEngine.Instance.ScreenDimension;	// (계산용) 화면 크기
		Vector3 fadePosOffset;
		switch (setting.CurrentFlowDirection)						// 흐름 방향에 따라 이동 오프셋 세팅
		{
			case FSNInGameSetting.FlowDirection.Up:
				fadePosOffset	= new Vector3(0, screenDim.y / 2);
				break;

			case FSNInGameSetting.FlowDirection.Down:
				fadePosOffset	= new Vector3(0, -screenDim.y / 2);
				break;

			case FSNInGameSetting.FlowDirection.Right:
				fadePosOffset	= new Vector3(screenDim.x / 2, 0);
				break;

			case FSNInGameSetting.FlowDirection.Left:
				fadePosOffset	= new Vector3(-screenDim.x / 2, 0);
				break;

			default:
				throw new System.Exception("HUH???");
		}
		//ApplySideTextMargin(ref fadePosOffset, setting, setting.CurrentFlowDirection);	// 여백 적용

		// 새 텍스트 엘레먼트 세팅 : 선택지 질문 텍스트

		var newTextSize					= CalculateTextSize(textSeg.text, setting);

		var questionTextElem			= new SnapshotElems.Text();

		questionTextElem.text			= textSeg.text;
		questionTextElem.fontSize		= setting.FontSize;
		questionTextElem.Color			= Color.white;//TODO
		questionTextElem.Alpha			= 1;
		questionTextElem.TransitionTime	= 1;//TODO
		questionTextElem.type			= SnapshotElems.Text.Type.OptionTexts;
		questionTextElem.MakeItUnique();

		questionTextElem.InitialState.Alpha	= 0;
		questionTextElem.FinalState.Alpha	= 0;

		// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)
		var qtextPos							= new Vector3(-newTextSize.x / 2f, newTextSize.y / 2f);
		ApplyCenterTextMargin(ref qtextPos, setting);					// 여백 지정

		questionTextElem.Position				= qtextPos;
		questionTextElem.InitialState.Position	= questionTextElem.Position - fadePosOffset;

		layer.AddElement(questionTextElem);								// 텍스트 엘리멘트 추가

		//
		int		dirIndex;
		string	dirText;

		// 선택지 : 위쪽 (등장 위치는 아래쪽)
		dirIndex	= (int)FSNInGameSetting.FlowDirection.Up;
		dirText		= textSeg.optionTexts[dirIndex];
		if (textSeg.optionTexts.Length - 1 >= dirIndex && !string.IsNullOrEmpty(dirText))
		{
			var upTextSize					= CalculateTextSize(dirText, setting);

			var upTextElem					= new SnapshotElems.Text();

			upTextElem.text					= dirText;
			upTextElem.fontSize				= setting.FontSize;
			upTextElem.Color				= Color.white;//TODO
			upTextElem.Alpha				= 1;
			upTextElem.TransitionTime		= 1;//TODO
			upTextElem.type					= SnapshotElems.Text.Type.OptionTexts;
			upTextElem.optionDir			= (FSNInGameSetting.FlowDirection)dirIndex;
			upTextElem.MakeItUnique();

			upTextElem.InitialState.Alpha	= 0;
			upTextElem.FinalState.Alpha		= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)
			var tpos							= new Vector3(upTextSize.x / 2f, -screenDim.y / 2f + upTextSize.y);
			ApplySideTextMargin(ref tpos, setting, FSNInGameSetting.FlowDirection.Up);
			upTextElem.Position					= tpos;
			upTextElem.InitialState.Position	= upTextElem.Position - fadePosOffset;

			layer.AddElement(upTextElem);								// 텍스트 엘리멘트 추가
		}

		// 선택지 : 아래쪽 (등장 위치는 위쪽)
		dirIndex	= (int)FSNInGameSetting.FlowDirection.Down;
		dirText		= textSeg.optionTexts[dirIndex];
		if (textSeg.optionTexts.Length - 1 >= dirIndex && !string.IsNullOrEmpty(dirText))
		{
			var downTextSize				= CalculateTextSize(dirText, setting);

			var downTextElem				= new SnapshotElems.Text();

			downTextElem.text				= dirText;
			downTextElem.fontSize			= setting.FontSize;
			downTextElem.Color				= Color.white;//TODO
			downTextElem.Alpha				= 1;
			downTextElem.TransitionTime		= 1;//TODO
			downTextElem.type				= SnapshotElems.Text.Type.OptionTexts;
			downTextElem.optionDir			= (FSNInGameSetting.FlowDirection)dirIndex;
			downTextElem.MakeItUnique();

			downTextElem.InitialState.Alpha	= 0;
			downTextElem.FinalState.Alpha	= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)
			var tpos							= new Vector3(downTextSize.x / 2f, screenDim.y / 2f);
			ApplySideTextMargin(ref tpos, setting, FSNInGameSetting.FlowDirection.Down);
			downTextElem.Position				= tpos;
			downTextElem.InitialState.Position	= downTextElem.Position - fadePosOffset;

			layer.AddElement(downTextElem);								// 텍스트 엘리멘트 추가
		}

		// 선택지 : 왼쪽 (등장 위치는 오른쪽)
		dirIndex	= (int)FSNInGameSetting.FlowDirection.Left;
		dirText		= textSeg.optionTexts[dirIndex];
		if (textSeg.optionTexts.Length - 1 >= dirIndex && !string.IsNullOrEmpty(dirText))
		{
			var leftTextSize				= CalculateTextSize(dirText, setting);

			var leftTextElem				= new SnapshotElems.Text();

			leftTextElem.text				= dirText;
			leftTextElem.fontSize			= setting.FontSize;
			leftTextElem.Color				= Color.white;//TODO
			leftTextElem.Alpha				= 1;
			leftTextElem.TransitionTime		= 1;//TODO
			leftTextElem.type				= SnapshotElems.Text.Type.OptionTexts;
			leftTextElem.optionDir			= (FSNInGameSetting.FlowDirection)dirIndex;
			leftTextElem.MakeItUnique();

			leftTextElem.InitialState.Alpha	= 0;
			leftTextElem.FinalState.Alpha	= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)
			var tpos							= new Vector3(screenDim.x / 2f - leftTextSize.x, -leftTextSize.y * 3f);
			ApplySideTextMargin(ref tpos, setting, FSNInGameSetting.FlowDirection.Left);
			leftTextElem.Position				= tpos;
			leftTextElem.InitialState.Position	= leftTextElem.Position - fadePosOffset;

			layer.AddElement(leftTextElem);								// 텍스트 엘리멘트 추가
		}

		// 선택지 : 오른쪽 (등장 위치는 왼쪽)
		dirIndex	= (int)FSNInGameSetting.FlowDirection.Right;
		dirText		= textSeg.optionTexts[dirIndex];
		if (textSeg.optionTexts.Length - 1 >= dirIndex && !string.IsNullOrEmpty(dirText))
		{
			var rightTextSize				= CalculateTextSize(dirText, setting);

			var rightTextElem				= new SnapshotElems.Text();

			rightTextElem.text				= dirText;
			rightTextElem.fontSize			= setting.FontSize;
			rightTextElem.Color				= Color.white;//TODO
			rightTextElem.Alpha				= 1;
			rightTextElem.TransitionTime	= 1;//TODO
			rightTextElem.type				= SnapshotElems.Text.Type.OptionTexts;
			rightTextElem.optionDir			= (FSNInGameSetting.FlowDirection)dirIndex;
			rightTextElem.MakeItUnique();

			rightTextElem.InitialState.Alpha= 0;
			rightTextElem.FinalState.Alpha	= 0;

			// 새 텍스트 엘레먼트 - 위치 세팅 (시작 위치만. 끝 위치는 프로세스 끝에 결정된다)
			var tpos							= new Vector3(-screenDim.x / 2f, rightTextSize.y * 3f);
			ApplySideTextMargin(ref tpos, setting, FSNInGameSetting.FlowDirection.Right);
			rightTextElem.Position				= tpos;
			rightTextElem.InitialState.Position	= rightTextElem.Position - fadePosOffset;

			layer.AddElement(rightTextElem);								// 텍스트 엘리멘트 추가
		}



		//PushTextsToDirection(layer, setting.CurrentFlowDirection, newTextSize);	// 텍스트 일괄적으로 해당 방향으로 밀기
	}

	private void AddLastOptionText(FSNSnapshot.Layer layer, Segments.Text textSeg, IInGameSetting setting)
	{
		// TODO : 상하좌우 여백, 정렬 등도 따져야함
		// NOTE : 선택한 방향은 setting 쪽에 설정되어 오는 것으로...

		SnapshotElems.Text optionText	= null;
		foreach (var elem in layer.Elements)												// * 이전 레이어를 전부 뒤져서 진행 방향이 같은 선택지 텍스트를 찾아온다
		{
			var textElem	= elem as SnapshotElems.Text;
			if(textElem.type == SnapshotElems.Text.Type.OptionTexts && textElem.optionDir == setting.CurrentFlowDirection)
			{
				optionText = textElem;
				break;
			}
		}

		if(optionText != null)																// * 찾은 경우에 한해서...
		{
			// 텍스트를 새로 만드는 것이 아니라 기존 것을 변경한다.
			// PushTextsToDirection 에서는 이 시점의 LastOption텍스트는 건들지 않는다.

			var textSize		= CalculateTextSize(optionText.text, setting);
			var posToCenter		= optionText.Position;

			TextPositionToCenter(ref posToCenter, textSize, optionText.optionDir, setting);	// 중앙 위치 맞추기
			
			optionText.Position	= posToCenter;
			optionText.type		= SnapshotElems.Text.Type.LastOption;						// LastOption 타입으로 변경

			PushTextsToDirection(layer, setting.CurrentFlowDirection, Vector2.zero);		// 기존 텍스트 일괄적으로 해당 방향으로 밀기 (내부 조건체크에 따라 LastOption은 이 타이밍에는 제외된다)
		}
		else
		{
			Debug.LogError("cannot find option text to direction : " + setting.CurrentFlowDirection.ToString());
		}
	}

	/// <summary>
	/// Layer 안에 들어있는 텍스트들을 특정 방향으로 모두 밀어낸다. 알파값도 변경. 수명이 다 된 것은 제거 처리.
	/// </summary>
	/// <param name="layer">변경할 레이어 (이미 복제된 상태여야함)</param>
	/// <param name="direction"></param>
	/// <param name="newTextSize"></param>
	private static void PushTextsToDirection(FSNSnapshot.Layer layer, FSNInGameSetting.FlowDirection direction, Vector2 newTextSize, float paraSpacing = 0)
	{
		Vector2 dirVec		= FSNInGameSetting.GetUnitVectorFromFlowDir(direction);
		List<int> UIDtoRemove	= new List<int>();	// 삭제 리스트
		
		foreach(var uId in layer.UniqueIDList)
		{
			var textElem	= layer.GetElement(uId) as SnapshotElems.Text;
			int elemAge		= textElem.ChainedParentCount;

			// 텍스트의 종류에 따라서 다른 룰을 적용한다.

			if (textElem.type == SnapshotElems.Text.Type.Normal)						// ** 일반 텍스트
			{
				Vector3 transVec		= Vector2.Scale(newTextSize, dirVec);			// 이동할 벡터 양
				if (elemAge > 0)
					transVec			+= (Vector3)(dirVec * paraSpacing);				// 최초에 등장한 이후엔 문단 간격도 적용

				if (elemAge < c_textLife)												// 텍스트가 아직 살아있어야하는 경우
				{
					textElem.Alpha		= (float)(c_textLife - elemAge) / (float)c_textLife;
					textElem.Position	= textElem.Position + transVec;
				}
				else
				{																		// 텍스트가 죽어야하는 경우
					textElem.FinalState.Position	= textElem.Position + transVec;
					//layer.RemoveElement(uId);
					UIDtoRemove.Add(uId);
				}
			}
			else
			{																			// ** 기타 (선택지 관련 텍스트)
				
				int killAge	= textElem.type == SnapshotElems.Text.Type.OptionTexts? 1 : 2;//(선택한 텍스트는 1턴 더 살아있어야 하므로)

				if (elemAge == killAge)													// 없어지는 타이밍
				{
					// NOTE : 현재 구조상의 문제로 인해 분기점 이후 바로 없어지는 오브젝트의 FinalState를 여러개 둘 수 없음.
					// 따라서 분기점 이후에도 1번은 오브젝트를 살려놓은 뒤 안보이게만 하고 다음번에 없애는 식으로.

					Vector2 halfScreen				= FSNEngine.Instance.ScreenDimension / 2f;
					Vector3 transVec				= Vector2.Scale(dirVec, halfScreen);

					textElem.Position				= textElem.Position + transVec;
					textElem.Alpha					= 0f;

					// TODO : Alpha를 0으로 하는 것 이외에 실제로 visible을 끌 수 있는 방법이 있다면 사용하도록 한다. 지금도 딱히 문제는 없긴 한데...
				}
				else if(elemAge == killAge + 1)											// 원래 없어져야했던 타이밍이 지나고 나서 실제로 없앤다.
				{
					UIDtoRemove.Add(uId);
				}
			}
		}

		int rmvCount	= UIDtoRemove.Count;
		for (int i = 0; i < rmvCount; i++)												// 삭제 리스트 처리
			layer.RemoveElement(UIDtoRemove[i]);
	}

	/// <summary>
	/// 텍스트들을 일정 방향으로 모두 밀어내며 삭제한다.
	/// </summary>
	/// <param name="layer"></param>
	/// <param name="direction"></param>
	private static void ClearTextsToDirection(FSNSnapshot.Layer layer, FSNInGameSetting.FlowDirection direction)
	{
		Vector3 screenHalf	= FSNEngine.Instance.ScreenDimension * 0.5f;				// 화면 크기 절반
		Vector3 dirVec		= FSNInGameSetting.GetUnitVectorFromFlowDir(direction);		// 흐름 방향 벡터

		var uidList			= layer.UniqueIDList;
		int count			= uidList.Count;
		int[] removeIDList	= new int[uidList.Count];
		uidList.CopyTo(removeIDList, 0);

		for(int i = 0; i < count; i++)
		{
			var textElem	= layer.GetElement(removeIDList[i]) as SnapshotElems.Text;

			textElem.FinalState.Position	= textElem.Position + Vector3.Scale(screenHalf, dirVec);	// 화면 절반 거리만큼 해당 방향으로 이동
			layer.RemoveElement(removeIDList[i]);
		}
	}
}

