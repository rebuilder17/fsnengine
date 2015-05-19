using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed partial class FSNSnapshotSequence
{
	/// <summary>
	/// FSNSnapshotSequence 를 스크립트 시퀀스에서 생성해낸다
	/// </summary>
	public static class Builder
	{
		/// <summary>
		/// 해석중에 사용하는 정보들
		/// </summary>
		class State
		{
			public FSNScriptSequence		sequence;			// 처리중인 스크립트
			public int						segIndex;			// 스크립트의 현재 처리중인 세그먼트 인덱스

			public FSNInGameSetting.Chain	settings;			// 처리중 해석된 세팅

			FSNInGameSetting				m_frozenSetting;
			bool							m_settingIsDirty	= true;

			public bool						prevPeriodWasChain	= false;	// 이전 period가 chaining이었는지 여부.

			/// <summary>
			/// 현재 세팅을 고정시킨 세팅값 얻어오기
			/// </summary>
			public FSNInGameSetting			FrozenSetting
			{
				get
				{
					if(m_settingIsDirty)
					{
						m_frozenSetting		= settings.Freeze();
						m_settingIsDirty	= false;
					}

					return m_frozenSetting;
				}
			}

			/// <summary>
			/// 세팅 변경되었음 체크
			/// </summary>
			public void SetSettingDirty()
			{
				m_settingIsDirty	= true;
			}

			/// <summary>
			/// 복제
			/// </summary>
			/// <returns></returns>
			public State Clone()
			{
				State newState		= new State();

				newState.sequence	= sequence;
				newState.segIndex	= segIndex;
				newState.settings	= settings.CloneEntireChain();

				newState.prevPeriodWasChain	= prevPeriodWasChain;

				newState.m_settingIsDirty	= true;	// 안전을 위해 무조건 Dirty 상태로 둔다

				return newState;
			}
		}

		/// <summary>
		/// 한 Snapshot 에 필요한 Module 콜들을 한번에 모았다가 처리하기 위한 헬퍼 클래스
		/// </summary>
		class ModuleCallQueue
		{
			Dictionary<IFSNProcessModule, List<FSNProcessModuleCallParam>>	m_moduleCallTable	= new Dictionary<IFSNProcessModule, List<FSNProcessModuleCallParam>>();

			/// <summary>
			/// 등록된 콜 모두 초기화
			/// </summary>
			public void ClearCall()
			{
				foreach(var callList in m_moduleCallTable.Values)
				{
					callList.Clear();
				}
			}

			/// <summary>
			/// 콜 추가
			/// </summary>
			/// <param name="module"></param>
			/// <param name="segment"></param>
			public void AddCall(IFSNProcessModule module, FSNScriptSequence.Segment segment, IInGameSetting setting)
			{
				if(!m_moduleCallTable.ContainsKey(module))
					m_moduleCallTable[module]	= new List<FSNProcessModuleCallParam>();

				m_moduleCallTable[module].Add(new FSNProcessModuleCallParam() { segment = segment, setting = setting });
			}

			/// <summary>
			/// 오브젝트가 존재하는 모든 모듈에 콜 보내기
			/// </summary>
			/// <param name="segment"></param>
			/// <param name="setting"></param>
			public void AddAllModuleCall(FSNScriptSequence.Segment segment, IInGameSetting setting)
			{
				foreach (var callList in m_moduleCallTable.Values)
				{
					callList.Add(new FSNProcessModuleCallParam() { segment = segment, setting = setting });
				}
			}

			/// <summary>
			/// 쌓인 콜 모두 실행 후 Clear
			/// </summary>
			public void ProcessCall(FSNSnapshot prevSnapshot, FSNSnapshot curSnapshot)
			{
				foreach(var pair in m_moduleCallTable)
				{
					var module		= pair.Key;
					var prevLayer	= prevSnapshot.GetLayer(module.LayerID) ?? FSNSnapshot.Layer.Empty;

					var callInfo	= pair.Value;

					var newLayer	= module.GenerateNextLayerImage(prevLayer, callInfo.ToArray());

					curSnapshot.SetLayer(module.LayerID, newLayer);
				}

				ClearCall();
			}

			/// <summary>
			/// 분기 혹은 점프 후, 이전 레이어 상태를 읽어서 오브젝트가 있는 Module들을 내부적으로 활성화해준다.
			/// 객체가 있는데도 모듈 콜이 누락되는 경우를 막기 위함
			/// </summary>
			public void CatchupPreviousLayerState(FSNSnapshot prevSnapshot)
			{
				var layerIDs	= prevSnapshot.AllLayerIDs;
				int count		= layerIDs.Length;
				for (int i = 0; i < count; i++)
				{
					var module	= FSNEngine.Instance.GetModuleByLayerID(layerIDs[i]) as IFSNProcessModule;
					if(module != null && !m_moduleCallTable.ContainsKey(module))
						m_moduleCallTable[module]	= new List<FSNProcessModuleCallParam>();
				}
			}
		}

		//=======================================================================

		/// <summary>
		///  FSNSequence를 해석하여 FSNSnapshotSequence를 만든다.
		/// </summary>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static FSNSnapshotSequence BuildSnapshotSequence(FSNScriptSequence sequence)
		{
			// 디버깅 정보 설정
			FSNDebug.currentRuntimeStage		= FSNDebug.RuntimeStage.SnapshotBuild;
			FSNDebug.currentProcessingScript	= sequence.OriginalScriptPath;

			FSNSnapshotSequence	snapshotSeq		= new FSNSnapshotSequence();
			State				builderState	= new State();

			snapshotSeq.OriginalScriptPath		= sequence.OriginalScriptPath;	// 스크립트 경로 보관
			snapshotSeq.ScriptHashKey			= sequence.ScriptHashKey;	// ScriptHashKey 복사해오기
			snapshotSeq.ScriptHeader			= sequence.Header;

			// State 초기 세팅

			builderState.sequence				= sequence;
			builderState.segIndex				= 0;
			builderState.settings				= new FSNInGameSetting.Chain(FSNEngine.Instance.DefaultInGameSetting);


			// 시작 Snapshot 만들기

			FSNSnapshot startSnapshot			= new FSNSnapshot();
			startSnapshot.NonstopToForward		= true;
			startSnapshot.InGameSetting			= builderState.settings;

			Segment startSegment				= new Segment();
			startSegment.Type					= FlowType.Normal;
			startSegment.snapshot				= startSnapshot;

			snapshotSeq.Add(startSegment);


			// 빌드 시작
			ProcessSnapshotBuild(builderState, snapshotSeq, 0);

			// 디버깅 정보 설정
			FSNDebug.currentRuntimeStage		= FSNDebug.RuntimeStage.Runtime;

			return snapshotSeq;
		}

		/// <summary>
		/// FSNSequence 해석. 분기마다 builderState를 복제해야 하므로 새로 호출된다.
		/// </summary>
		/// <param name="builderState"></param>
		/// <param name="snapshotSeq">이번 콜에서 생성할 시퀀스 이전의 스냅샷 인덱스</param>
		/// <param name="bs"></param>
		/// <param name="linkToLast">이 메서드를 호출하기 바로 이전의 Snapshot에 연결할지 여부. 기본은 true. 선택지 등등에서 false로 해줘야함</param>
		/// <returns>이번 콜에서 생성한 시퀀스들 중 제일 첫번째 것. 다른 시퀀스 흐름과 연결할 때 사용 가능</returns>
		static Segment ProcessSnapshotBuild(State bs, FSNSnapshotSequence snapshotSeq, int prevSnapshotIndex, bool linkToLast = true)
		{
			ModuleCallQueue moduleCalls	= new ModuleCallQueue();

			Segment	sseg;														// 새로 생성할 Segment/Snapshot
			FSNSnapshot sshot;
			NewSnapshot(out sseg, out sshot);

			var firstSeg		= sseg;											// 최초 스냅샷
			var prevCallSeg		= snapshotSeq.Get(prevSnapshotIndex);			// 이번에 생성할 시퀀스의 이전 스냅샷

			var lastSeg			= prevCallSeg;									// 바로 이전에 처리한 스냅샷 (최초 루프시에는 실행 이전의 마지막 스냅샷과 동일)

			List<Segments.Control>	jumpSegs	= new List<Segments.Control>();	// 점프(goto, SwipeOption 등) 세그먼트가 등장했을 경우 여기에 보관된다
			List<Segment.CallFuncInfo> funcCalls= new List<Segment.CallFuncInfo>();	// 함수 콜이 있을 경우 여기에 먼저 누적하고 period때 한번에 처리한다

			float snapshotDelay	= 0f;											// 스냅샷에 적용할 지연 시간 (/기다리기 커맨드)
			//

			// 처음 시작할 때는 무조건 모든 객체들을 Hard-Clone한다 (분기점에서 Initial/Final 스테이트 독립을 하기 위해)

			moduleCalls.CatchupPreviousLayerState(prevCallSeg.snapshot);		// 기존의 유효한 Layer들을 활성화 (안그러면 모듈 콜을 누락해버림)
			moduleCalls.AddAllModuleCall(new Segments.HardClone(), bs.settings);	


			// 스크립트 Sequence가 끝나거나 특정 명령을 만날 때까지 반복

			bool keepProcess	= true;
			while(keepProcess)
			{
				var curSeg	= bs.sequence[bs.segIndex];							// 현재 명령어 (Sequence 세그먼트)
				FSNDebug.currentProcessingScriptLine = curSeg.scriptLineNumber;	// 디버깅 정보 세팅 (줄 번호)

				switch(curSeg.type)												// 명령어 타입 체크 후 분기
				{
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Setting:							// ** 세팅
				{
					var settingSeg		= curSeg as Segments.Setting;

					if(settingSeg.settingMethod == Segments.Setting.SettingMethod.Pop)	// * 세팅 pop
					{
						if(bs.settings.ParentChain != null)
						{
							bs.settings	= bs.settings.ParentChain;
						}
						else
						{
							Debug.LogError("Cannot pop settings anymore - there is no settings pushed");
						}
					}
					else
					{
						if(settingSeg.settingMethod == Segments.Setting.SettingMethod.Push)	// * Push일 경우에는 새 Chain을 생성한다
						{
							bs.settings	= new FSNInGameSetting.Chain(bs.settings);
						}

						foreach(var settingPair in settingSeg.RawSettingTable)	// Setting 설정
						{
							bs.settings.SetPropertyByString(settingPair.Key, settingPair.Value);
						}
					}

					bs.SetSettingDirty();										// 세팅 변경됨 플래그 올리기
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Text:						// ** 텍스트
				{
					var module			= FSNEngine.Instance.GetModule(FSNEngine.ModuleType.Text) as IFSNProcessModule;

					moduleCalls.AddCall(module, curSeg, bs.FrozenSetting);		// 해당 명령 저장
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Object:						// ** 오브젝트 (이미지 등)
				{
					var objSeg	= curSeg as Segments.Object;
					var module	= FSNEngine.Instance.GetModuleByLayerID(objSeg.layerID) as IFSNProcessModule;

					moduleCalls.AddCall(module, objSeg, bs.FrozenSetting);
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Label:						// ** 레이블
				{
					//var labelSeg		= curSeg as Segments.Label;
					// 현재 이 시점에서는 labelSeg로 하는 일이 없다...
					//Debug.Log("Label : " + labelSeg.labelName);
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Control:					// ** 엔진 컨트롤
				{
					var controlSeg		= curSeg as Segments.Control;

					switch(controlSeg.controlType)								// 종류에 따라 처리
					{
						case Segments.Control.ControlType.Block:
							keepProcess	= false;	// 블로킹 - 이 분기에서는 해석 종료
							break;

						case Segments.Control.ControlType.SwipeOption:
							sseg.Type	= FlowType.UserChoice;					// 스냅샷 세그먼트 종류 변경 (유저 선택으로)

							jumpSegs.Add(controlSeg);							// 점프 명령어로 보관해두고 나중에 처리한다.

							break;

						case Segments.Control.ControlType.Goto:
						case Segments.Control.ControlType.ReverseGoto:
						case Segments.Control.ControlType.ConditionJump:
							jumpSegs.Add(controlSeg);							// 점프 명령어로 보관해두고 나중에 처리한다.
							break;

						case Segments.Control.ControlType.Clear:				// 모든 모듈에 clear를 보낸다
							moduleCalls.AddAllModuleCall(controlSeg, bs.FrozenSetting);
							break;

						case Segments.Control.ControlType.Oneway:
							sseg.OneWay	= true;
							break;

						case Segments.Control.ControlType.ForceBack:
							sseg.snapshot.ForceBackward = true;
							break;

						case Segments.Control.ControlType.Load:
							sseg.Type		= FlowType.Load;
							sseg.Parameter	= controlSeg.GetLoadScriptData();	// 스냅샷 세그먼트의 parameter에 스크립트 파일명 보관
							break;

						case Segments.Control.ControlType.UnityCall:			// 함수콜
							{
								var callinfo	= new Segment.CallFuncInfo();
								controlSeg.GetUnityCallData(out callinfo.funcname, out callinfo.param);
								funcCalls.Add(callinfo);
							}
							break;

						case Segments.Control.ControlType.Delay:				// 딜레이
							snapshotDelay	= controlSeg.GetDelay();			// 지연 시간을 미리 지정해둔다
							break;
					}
				}
				break;
				//////////////////////////////////////////////////////////////
				case FSNScriptSequence.Segment.Type.Period:						// ** Period : 현재까지 누적한 call을 실행하는 개념으로 사용중
				{
					var periodSeg		= curSeg as Segments.Period;

					// 다음 snapshot을 위해 현재 진행 방향의 반대방향으로 FlowDirection 정해놓기
					bs.settings.BackwardFlowDirection	= FSNInGameSetting.GetOppositeFlowDirection(bs.settings.CurrentFlowDirection);
					bs.SetSettingDirty();

					sshot.InGameSetting	= bs.FrozenSetting;						// 현재까지의 세팅 적용 (굳힌거 사용)

					moduleCalls.ProcessCall(lastSeg.snapshot, sshot);			// 지금까지 모인 모듈 콜 집행하기

					if(linkToLast)												// 이전 스냅샷에 붙여야하는경우
					{
						LinkSnapshots(lastSeg, sseg);

						if(bs.prevPeriodWasChain)								// 이전 period가 chaining이었다면, 역방향 chaining 걸기
						{
							sseg.snapshot.NonstopToBackward	= true;
							bs.prevPeriodWasChain			= false;			// (플래그 해제)
						}

						if(periodSeg.isChaining)								// Chaining 옵션이 켜져있을 경우
						{
							sseg.snapshot.NonstopToForward	= true;
							bs.prevPeriodWasChain			= true;				// (chaining 상태 기록)
						}
					}
					else
					{
						linkToLast = true;	// FIX : linkToLast는 맨 첫번째 스냅샷에만 해당하는 파라미터임. 두번째는 꺼준다
					}

					sseg.FunctionCalls	= funcCalls.ToArray();					// 쌓인 함수콜 리스트를 배열로 변환하여 세팅
					funcCalls.Clear();

					snapshotSeq.Add(sseg);										// 현재 스냅샷을 시퀀스에 추가
					//

					sseg.snapshot.AfterSwipeDelay	= snapshotDelay;			// 현재 지정된 딜레이를 적용.
					snapshotDelay					= 0f;
					//


					// Condition 링크 임시 보관소 - 나중에 한번에 특정 방향으로 몰아넣는다
					FSNInGameSetting.FlowDirection			conditionLinkDir		= FSNInGameSetting.FlowDirection.None;
					FSNInGameSetting.FlowDirection			conditionLinkBackDir	= FSNInGameSetting.FlowDirection.None;
					List<Segment.FlowInfo.ConditionLink>	conditionLinks			= null;
					Dictionary<string, Segment>				conditionLinkSegCache	= new Dictionary<string, Segment>();	// 조건부 점프시, 중복되는 점프는 이전 것을 사용하게

					foreach(var jumpSeg in jumpSegs)							// * 점프 세그먼트가 있을 경우 처리
					{
						// NOTE : 현재 Label은 Soft만 구현한다.

						if (jumpSeg.controlType == Segments.Control.ControlType.SwipeOption)		// *** 선택지
						{
							bool isNonTextOption	= jumpSeg.IsNonTextOption();
							for (int i = 0; i < 4; i++)												// 모든 방향마다 처리
							{
								var dir			= (FSNInGameSetting.FlowDirection)i;
								string label	= jumpSeg.GetLabelFromSwipeOptionData(dir);
								if (!string.IsNullOrEmpty(label))									// 라벨이 지정된 경우만 처리(= 해당 방향으로 분기가 있을 때만)
								{
									// FIX : 만약 해당 선택지 방향이 원래의 역방향에 해당하는 것이었다면, 역방향을 None으로 재설정한다. (역방향 오버라이드 지원)
									if (dir == sseg.BackDirection)
									{
										sseg.BackDirection	= FSNInGameSetting.FlowDirection.None;
										sseg.snapshot.DisableBackward = true;	// 역방향 비활성화
									}

									int labelIndex	= bs.sequence.GetIndexOfLabel(label);
									var labelSeg	= bs.sequence.GetSegment(labelIndex) as Segments.Label;

									if (labelSeg.labelType == Segments.Label.LabelType.Soft)		// * SOFT 라벨로 점프
									{
										if (labelIndex < bs.segIndex)								// SOFT 라벨은 거슬러올라갈 수 없다.
											Debug.LogError("Cannot jump to previous soft label");

										var clonnedState		= bs.Clone();						// 상태 복제
										clonnedState.segIndex	= labelIndex;						// 라벨 인덱스 세팅
										clonnedState.settings.CurrentFlowDirection	= dir;			// 진행 방향 세팅 - 선택지 방향으로 진행 방향을 강제 세팅한다
										clonnedState.settings.BackwardFlowDirection	= FSNInGameSetting.GetOppositeFlowDirection(dir);

										var newSeg	= ProcessSnapshotBuild(clonnedState, snapshotSeq, sseg.Index, false);	// 새 분기 해석하기. 이전 스냅샷에 바로 붙이지 않는다.
										LinkSnapshotAsOption(sseg, newSeg, dir, isNonTextOption);	// 선택지로 연결하기
									}
									else
									{																// * HARD 라벨로 점프
										Debug.LogError("Not implemented");
									}
								}
							}
						}
						else if(jumpSeg.controlType == Segments.Control.ControlType.Goto)			// *** GOTO
						{
							string label	= jumpSeg.GetGotoLabel();
							int labelIndex	= bs.sequence.GetIndexOfLabel(label);
							var labelSeg	= bs.sequence.GetSegment(labelIndex) as Segments.Label;

							if(labelSeg.labelType == Segments.Label.LabelType.Soft)					// * SOFT 라벨로 점프
							{
								if (labelIndex < bs.segIndex)										// SOFT 라벨은 거슬러올라갈 수 없다.
									Debug.LogError("Cannot jump to previous soft label");

								var clonnedState		= bs.Clone();								// 상태 복제
								clonnedState.segIndex	= labelIndex;								// 라벨 인덱스 세팅

								ProcessSnapshotBuild(clonnedState, snapshotSeq, sseg.Index);		// 새 분기 해석하기

								// SOFT 라벨로 점프하는 경우엔 사실상 이 분기점으로 다시 되돌아올 일이 생기지 않는다.
								// 추가 스크립트 해석을 중단한다.
								keepProcess	= false;
							}
							else
							{																		// * HARD 라벨로 점프
								Debug.LogError("Not implemented");
							}
						}
						else if(jumpSeg.controlType == Segments.Control.ControlType.ReverseGoto)	// ** ReverseGoto
						{
							string label	= jumpSeg.GetReverseGotoLabel();
							int labelIndex	= bs.sequence.GetIndexOfLabel(label);
							var labelSeg	= bs.sequence.GetSegment(labelIndex) as Segments.Label;

							if (labelSeg.labelType == Segments.Label.LabelType.Soft)				// * SOFT 라벨로 점프
							{
								if (labelIndex < bs.segIndex)										// SOFT 라벨은 거슬러올라갈 수 없다.
									Debug.LogError("Cannot jump to previous soft label");

								var clonnedState		= bs.Clone();								// 상태 복제
								clonnedState.segIndex	= labelIndex;								// 라벨 인덱스 세팅
								
								// 진행 방향을 역방향으로 세팅
								clonnedState.settings.CurrentFlowDirection	= FSNInGameSetting.GetOppositeFlowDirection(clonnedState.settings.CurrentFlowDirection);
								clonnedState.settings.BackwardFlowDirection	= FSNInGameSetting.GetOppositeFlowDirection(clonnedState.settings.BackwardFlowDirection);

								// 가장 마지막 세그먼트를 잠시동안만 UserChoice로 변경해서 새 스냅샷시퀀스를 정방향에 붙이지 못하게 막는다.
								// 생성한 스냅샷을 역방향에 직접 붙여줘야하기 때문.
								// 좀 Hacky한 방법이라서 변경이 필요할지도.

								var newSeg	= ProcessSnapshotBuild(clonnedState, snapshotSeq, sseg.Index, false);	// 새 분기 해석한 후 레퍼런스 받기. 링크는 하지 않음

								LinkSnapshotsReverseOverride(sseg, newSeg);	//붙이기

								sseg.snapshot.DisableBackward = true;		// 역방향 비활성화
							}
							else
							{																		// * HARD 라벨로 점프
								Debug.LogError("Not implemented");
							}
						}
						else if (jumpSeg.controlType == Segments.Control.ControlType.ConditionJump)	// ** 조건부 점프
						{
							string funcname;
							string [] param;
							string label	= jumpSeg.GetConditionJumpLabel();
							int labelIndex	= bs.sequence.GetIndexOfLabel(label);
							var labelSeg	= bs.sequence.GetSegment(labelIndex) as Segments.Label;

							if(labelSeg.labelType == Segments.Label.LabelType.Soft)					// * SOFT 라벨로 점프
							{
								if (labelIndex < bs.segIndex)										// SOFT 라벨은 거슬러올라갈 수 없다.
									Debug.LogError("Cannot jump to previous soft label");

								Segment newSeg;
								if(!conditionLinkSegCache.TryGetValue(label, out newSeg))			// 이전에 캐싱된 것이 없을 때만 새로 해석한다.
								{
									var clonnedState		= bs.Clone();							// 상태 복제
									clonnedState.segIndex	= labelIndex;							// 라벨 인덱스 세팅

									// 가장 마지막 세그먼트를 잠시동안만 UserChoice로 변경해서 새 스냅샷시퀀스를 정방향에 붙이지 못하게 막는다.
									// 생성한 스냅샷을 역방향에 직접 붙여줘야하기 때문.
									// 좀 Hacky한 방법이라서 변경이 필요할지도.

									newSeg		= ProcessSnapshotBuild(clonnedState, snapshotSeq, sseg.Index, false);	// 새 분기 해석한 후 레퍼런스 받기. 링크는 하지 않음

									conditionLinkSegCache[label]	= newSeg;									// 캐싱해두기
								}

								// Note : 현재 스크립트 스펙 상, 한 스냅샷 안에서 Condition Link은 한쪽 방향으로밖에 나올 수 없다.
								// 따라서 방향 구분 없이 리스트 하나에 모두 모아둔 후, 기록해둔 방향에 모두 집어넣는다.
								
								conditionLinkDir	= newSeg.snapshot.InGameSetting.CurrentFlowDirection;
								conditionLinkBackDir= newSeg.snapshot.InGameSetting.BackwardFlowDirection;

								// 만약 일반 링크가 등장하지 않아 방향이 설정되지 않을 때를 대비해서 세팅
								if (sseg.FlowDirection == FSNInGameSetting.FlowDirection.None)
									sseg.FlowDirection = conditionLinkDir;
								if (newSeg.BackDirection == FSNInGameSetting.FlowDirection.None)
									newSeg.BackDirection = conditionLinkBackDir;

								List<Segment.CallFuncInfo> callfuncs	= new List<Segment.CallFuncInfo>();
								while (jumpSeg.DequeueConditionJumpData(out funcname, out param))			// AND 조건으로 묶인 모든 condition 읽기
								{
									callfuncs.Add(new Segment.CallFuncInfo() { funcname = funcname, param = param });
								}
								if (conditionLinks == null)
									conditionLinks	= new List<Segment.FlowInfo.ConditionLink>();
								conditionLinks.Add(new Segment.FlowInfo.ConditionLink() { funcinfo = callfuncs.ToArray(), Linked = newSeg });

								if(!newSeg.OneWay)
									newSeg.SetDirectFlow(conditionLinkBackDir, sseg);						// 역방향 설정

							}
							else
							{																		// * HARD 라벨로 점프
								Debug.LogError("Not implemented");
							}
						}
						else
						{
							Debug.LogError("Unknown or not-yet-implemented jump statement!");
						}
					}
					jumpSegs.Clear();

					if (conditionLinks != null)									// 모아뒀던 조건부 점프 한번에 세팅
					{
						sseg.SetConditionFlow(conditionLinkDir, conditionLinks.ToArray());
					}

					lastSeg	= sseg;
					NewSnapshot(out sseg, out sshot);							// 새 스냅샷 인스턴스 준비
				}
				break;
				/////////////////////////////////////////////////////////////
				default:
				Debug.LogError("?????????");
				break;
				}


				//
				bs.segIndex++;													// 다음 명령어 인덱스

				if(bs.segIndex >= bs.sequence.Length)							// * Sequence 가 끝났다면 루프 종료
				{
					keepProcess	= false;
				}
			}

			return firstSeg;
		}

		/// <summary>
		/// 새 Segment/Snapshot 세팅 (숏컷)
		/// </summary>
		/// <param name="newSeg"></param>
		/// <param name="newSnapshot"></param>
		static void NewSnapshot(out Segment newSeg, out FSNSnapshot newSnapshot)
		{
			newSnapshot		= new FSNSnapshot();
			newSeg			= new Segment();
			newSeg.snapshot	= newSnapshot;
		}

		/// <summary>
		/// 스냅샷 단순 연결
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		static void LinkSnapshots(Segment prev, Segment next)
		{
			var swipeDir		= next.snapshot.InGameSetting.CurrentFlowDirection;		// 다음에 올 시퀀스의 설정값으로 링크 방향을 정한다
			var backDir			= next.snapshot.InGameSetting.BackwardFlowDirection;

			prev.FlowDirection	= swipeDir;
			next.BackDirection	= backDir;

			prev.SetDirectFlow(swipeDir, next);
			if (!next.OneWay)															// 단방향이 아닐 경우 반대로 돌아가는 링크도 생성
			{
				next.SetDirectFlow(backDir, prev);
			}
		}

		/// <summary>
		/// 스냅샷 단순 연결, 선택지 버전
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		/// <param name="dir"></param>
		static void LinkSnapshotAsOption(Segment prev, Segment next, FSNInGameSetting.FlowDirection dir, bool isNonTextOption)
		{
			var backDir			= FSNInGameSetting.GetOppositeFlowDirection(dir);

			next.BackDirection	= backDir;

			prev.SetDirectFlow(dir, next);
			next.SetDirectFlow(backDir, prev);

			if (!isNonTextOption)	// 텍스트가 있는 일반 선택지일 때만
			{
				// FIX : 선택지 분기는 period 세그먼트의 isChaining을 체크하는 부분이 위쪽 UserChoice를 체크하는 조건문에 걸려 실행되지 못함.
				// 선택지 바로 다음에는 LastOption 텍스트를 표시하는 snapshot이 무조건 나온다고 가정하고, 여기서 강제로 chaining을 해준다.
				next.snapshot.NonstopToForward	= true;
				next.snapshot.NonstopToBackward	= true;
			}
		}

		/// <summary>
		/// 스냅샷 단순 연결, 역방향
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		static void LinkSnapshotsReverseOverride(Segment prev, Segment next)
		{
			var swipeDir		= next.snapshot.InGameSetting.CurrentFlowDirection;		// 다음에 올 시퀀스의 설정값으로 링크 방향을 정한다
			var backDir			= next.snapshot.InGameSetting.BackwardFlowDirection;

			next.BackDirection	= backDir;

			prev.SetDirectFlow(swipeDir, next);
			if (!next.OneWay)															// 단방향이 아닐 경우 반대로 돌아가는 링크도 생성
			{
				next.SetDirectFlow(backDir, prev);
			}
		}
	}
}
