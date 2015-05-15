using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// FSN엔진에 같이 포함되는 기본 스크립트 명령어
/// </summary>
public static class FSNBuiltInScriptCommands
{
	public static void Install()
	{
		FSNScriptSequence.Parser.AddCommand(Goto,			"goto",			"이동", "점프");
		FSNScriptSequence.Parser.AddCommand(ReverseGoto,	"reversegoto",	"역방향", "역방향점프");
		FSNScriptSequence.Parser.AddCommand(End,			"end",			"끝");
		FSNScriptSequence.Parser.AddCommand(Oneway,			"oneway",		"역방향금지");
		FSNScriptSequence.Parser.AddCommand(ForceBack,		"forceback",	"되돌아가기");
		FSNScriptSequence.Parser.AddCommand(Clear,			"clear",		"지우기");
		FSNScriptSequence.Parser.AddCommand(TextClear,		"textclear",	"글자지우기");
		FSNScriptSequence.Parser.AddCommand(Load,			"load",			"불러오기");
		FSNScriptSequence.Parser.AddCommand(Delay,			"delay",		"기다리기");

		FSNScriptSequence.Parser.AddCommand(PushSetting,	"pushsetting",	"설정쌓기");
		FSNScriptSequence.Parser.AddCommand(PopSetting,		"popsetting",	"설정버리기");
		FSNScriptSequence.Parser.AddCommand(SetSetting,		"setsetting",	"설정값");

		FSNScriptSequence.Parser.AddCommand(Option_start,	"option",		"선택지");
		FSNScriptSequence.Parser.AddCommand(Option_left,	"left",			"왼쪽");
		FSNScriptSequence.Parser.AddCommand(Option_right,	"right",		"오른쪽");
		FSNScriptSequence.Parser.AddCommand(Option_up,		"up",			"위");
		FSNScriptSequence.Parser.AddCommand(Option_down,	"down",			"아래");
		FSNScriptSequence.Parser.AddCommand(Option_end,		"showoption",	"선택지표시");
		FSNScriptSequence.Parser.AddCommand(Option_end_nontext,		"shownontextoption",	"글없는선택지표시");

		FSNScriptSequence.Parser.AddCommand(Image_start,	"showimage",	"이미지생성");
		FSNScriptSequence.Parser.AddCommand(Image_end,		"removeimage",	"이미지제거");
		FSNScriptSequence.Parser.AddCommand(Image_set,		"imageset",		"이미지설정");
		FSNScriptSequence.Parser.AddCommand(Image_initial,	"imageinit",	"이미지시작설정");
		FSNScriptSequence.Parser.AddCommand(Image_final,	"imagefinal",	"이미지종료설정");

		FSNScriptSequence.Parser.AddCommand(Object_start,	"showobject",	"객체생성");
		FSNScriptSequence.Parser.AddCommand(Object_end,		"removeobject",	"객체제거");
		FSNScriptSequence.Parser.AddCommand(Object_set,		"objectset",	"객체설정");
		FSNScriptSequence.Parser.AddCommand(Object_initial,	"objectinit",	"객체시작설정");
		FSNScriptSequence.Parser.AddCommand(Object_final,	"objectfinal",	"객체종료설정");

		FSNScriptSequence.Parser.AddCommand(Sound_start,	"startsound",	"소리생성");
		FSNScriptSequence.Parser.AddCommand(Sound_end,		"removesound",	"소리제거");
		FSNScriptSequence.Parser.AddCommand(Sound_set,		"soundset",		"소리설정");
		FSNScriptSequence.Parser.AddCommand(Sound_initial,	"soundinit",	"소리시작설정");
		FSNScriptSequence.Parser.AddCommand(Sound_final,	"soundfinal",	"소리종료설정");

		FSNScriptSequence.Parser.AddCommand(Sound_oneshot,	"sound",		"소리");

		FSNScriptSequence.Parser.AddCommand(UnityCall,					"call",			"함수");
		FSNScriptSequence.Parser.AddCommand(UnityCall_SetFlagTrue,		"flagon",		"플래그켜기", "플래그올리기", "플래그세우기");
		FSNScriptSequence.Parser.AddCommand(UnityCall_SetFlagFalse,		"flagoff",		"플래그끄기", "플래그내리기");
		FSNScriptSequence.Parser.AddCommand(UnityCall_SetFlags,			"setflag",		"플래그설정");
		FSNScriptSequence.Parser.AddCommand(UnityCall_SetValues,		"setvalues",	"변수값설정");
		FSNScriptSequence.Parser.AddCommand(UnityCall_ShowSaveDialog,	"savedialog",	"저장하기");
		FSNScriptSequence.Parser.AddCommand(UnityCall_ShowLoadDialog,	"loaddialog",	"불러오기");

		FSNScriptSequence.Parser.AddCommand(ConditionJump_UnityCall,			"jumpif_call",			"함수가참이면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_FlagIsTrue,			"jumpif_flagon",		"플래그가참이면", "플래그가켜졌으면", "플래그가섰으면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_FlagIsFalse,			"jumpif_flagoff",		"플래그가거짓이면", "플래그가꺼졌으면", "플래그가안섰으면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_CheckFlag,			"jumpif_flagequal",		"플래그가같다면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_IfValueEqual,			"jumpif_valueequal",	"값이같으면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_IfValueNotEqual,		"jumpif_valuenotequal",	"값이다르면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_IfValueGreaterThan,	"jumpif_valuegreater",	"왼쪽이크면", "오른쪽이작으면");
		FSNScriptSequence.Parser.AddCommand(ConditionJump_IfValueLesserThan,	"jumpif_valuelesser",	"왼쪽이작으면", "오른쪽이크면");
	}

	//-------------------------------------------------------------------------------

	static void Goto(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Goto;
		newseg.SetGotoData(protocol.parameters[0]);

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= false;
		newSegInfo.selfPeriod		= false;
		protocol.PushSegment(newSegInfo);
	}

	static void ReverseGoto(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.ReverseGoto;
		newseg.SetReverseGotoData(protocol.parameters[0]);

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= false;
		newSegInfo.selfPeriod		= false;
		protocol.PushSegment(newSegInfo);
	}

	static void End(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Block;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= true;
		newSegInfo.selfPeriod		= true;
		protocol.PushSegment(newSegInfo);
	}

	static void Clear(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Clear;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= true;
		newSegInfo.selfPeriod		= false;
		protocol.PushSegment(newSegInfo);
	}

	static void Oneway(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Oneway;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= false;
		newSegInfo.selfPeriod		= false;
		protocol.PushSegment(newSegInfo);
	}

	static void ForceBack(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.ForceBack;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= false;//??
		newSegInfo.selfPeriod		= true;//??
		protocol.PushSegment(newSegInfo);
	}

	static void TextClear(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Text();

		newseg.textType				= Segments.Text.TextType.Clear;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= true;
		newSegInfo.selfPeriod		= false;
		protocol.PushSegment(newSegInfo);
	}

	static void Load(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Load;
		newseg.SetLoadScriptData(protocol.parameters[0]);

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo();
		newSegInfo.newSeg			= newseg;
		newSegInfo.usePrevPeriod	= true;
		newSegInfo.selfPeriod		= true;
		protocol.PushSegment(newSegInfo);

		// Load를 하는 시점에서 해당 스크립트가 종료되므로, block
		var blockseg				= new Segments.Control();
		blockseg.controlType		= Segments.Control.ControlType.Block;
		var blockSegInfo			= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg					= blockseg,
			usePrevPeriod			= true
		};
		protocol.PushSegment(blockSegInfo);
	}

	static void Delay(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Control();

		newseg.controlType			= Segments.Control.ControlType.Delay;
		newseg.SetDelay(float.Parse(protocol.parameters[0]));

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg					= newseg,
			usePrevPeriod			= false,
			selfPeriod				= true
		};
		protocol.PushSegment(newSegInfo);
	}

	//------------------------------------------------------------------------------------

	static void PushSetting(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Setting();
		newseg.settingMethod		= Segments.Setting.SettingMethod.Push;

		_fillSettingTable(newseg, protocol);	// 설정값 세팅

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg					= newseg,
			usePrevPeriod			= true,
		};
		protocol.PushSegment(newSegInfo);
	}

	static void PopSetting(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Setting();
		newseg.settingMethod		= Segments.Setting.SettingMethod.Pop;

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg					= newseg,
			usePrevPeriod			= true,
		};
		protocol.PushSegment(newSegInfo);
	}

	static void SetSetting(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newseg					= new Segments.Setting();
		newseg.settingMethod		= Segments.Setting.SettingMethod.Set;

		_fillSettingTable(newseg, protocol);	// 설정값 세팅

		var newSegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg					= newseg,
			usePrevPeriod			= true,
		};
		protocol.PushSegment(newSegInfo);
	}

	static void _fillSettingTable(Segments.Setting seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var param	= protocol.parameters;
		if (param != null)
		{
			int settingCount	= param.Length / 2;				// 2개 짝이 안맞는 파라미터값들은 버린다.
			for (int i = 0; i < settingCount; i += 2)
			{
				var name	= FSNInGameSetting.ConvertPropertyNameAlias(param[i]);
				seg[name]	= param[i + 1];
			}
		}
	}

	//------------------------------------------------------------------------------------

	const string		c_key_optionData	= "optionData";			// 선택지 데이터
	const string		c_key_optionTitle	= "optionTitleText";	// 선택지 텍스트

	static void Option_start(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		// 선택지 시작에서는 세그먼트를 직접 만들지 않는다.

		if(protocol.GetStateVar(c_key_optionData) != null)
		{
			Debug.LogError("You can't make other options without finishing previous option sequence.");
		}
		else
		{
			var options	= new string[4][];
			protocol.SetStateVar(c_key_optionData, options);					// 선택지 방향 -> 라벨 배열

			var text	= protocol.parameters.Length > 0? protocol.parameters[0] : "";	// 텍스트를 지정하지 않았을 시 빈 문자열로
			protocol.SetStateVar(c_key_optionTitle, text);						// 선택지 타이틀 지정
		}
	}

	/// <summary>
	/// 글 없는 선택지 표시
	/// </summary>
	/// <param name="protocol"></param>
	static void Option_end_nontext(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var optionData	= protocol.GetStateVar(c_key_optionData) as string[][];
		if (optionData == null)
		{
			Debug.LogError("You can't make options without starting an option sequence.");
		}
		else
		{
			// 지정된 레이블로 점프하는 선택지 점프 세그먼트
			var userChoiceSeg			= new Segments.Control();
			userChoiceSeg.controlType	= Segments.Control.ControlType.SwipeOption;
			for(int i = 0; i < 4; i++)
			{
				var option = optionData[i];
				if (option != null)
				{
					userChoiceSeg.SetSwipeOptionData((FSNInGameSetting.FlowDirection)i, optionData[i][0]);
				}
			}
			userChoiceSeg.SetNonTextOptionFlag();			// 글 없는 선택지로 지정

			var userOptionControlSegInfo = new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= userChoiceSeg,
				selfPeriod		= false,
				usePrevPeriod	= false,
			};
			protocol.PushSegment(userOptionControlSegInfo);

			// period 세그먼트 (선택지 표시를 위해서)
			var periodSeg			= new Segments.Period();
			periodSeg.isChaining	= false;
			var periodSegInfo		= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg				= periodSeg
			};
			protocol.PushSegment(periodSegInfo);

			// 처리 블록 세그먼트
			var blockSeg			= new Segments.Control();
			blockSeg.controlType	= Segments.Control.ControlType.Block;
			var blockControlSegInfo	= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg				= blockSeg,
				selfPeriod			= false,
				usePrevPeriod		= false,
			};
			protocol.PushSegment(blockControlSegInfo);
		}
	}

	/// <summary>
	/// 일반 선택지 표시
	/// </summary>
	/// <param name="protocol"></param>
	static void Option_end(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var optionData	= protocol.GetStateVar(c_key_optionData) as string[][];
		if (optionData == null)
		{
			Debug.LogError("You can't make options without starting an option sequence.");
		}
		else
		{
			var newOptionTextSeg		= new Segments.Text();
			newOptionTextSeg.text		= protocol.GetStateVar(c_key_optionTitle) as string;
			newOptionTextSeg.textType	= Segments.Text.TextType.Options;

			// 선택지 선택 후 해당 선택지를 잠깐 보여주기 위해서,
			// 가상 Label을 추가한 뒤 LastOption 텍스트 출력, 이후 원래 Label로 점프하는 추가 시퀀스를 만든다.

			newOptionTextSeg.optionTexts	= new string[4];
			var optionTransitionLabels	= new string[4];	// 트랜지션용 임시 라벨 목록
			for(int i = 0; i < 4; i++)
			{
				var option	= optionData[i];
				if(option != null)
				{
					optionTransitionLabels[i]		= option[0] + "__transition";
					newOptionTextSeg.optionTexts[i]	= option[1];
				}
			}

			// 선택지 텍스트 세그먼트 푸시
			var newOptionTextSegInfo = new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newOptionTextSeg,
				selfPeriod		= false,
				usePrevPeriod	= true,
			};
			protocol.PushSegment(newOptionTextSegInfo);


			// 임시 레이블로 점프하는 선택지 점프 세그먼트
			var userChoiceSeg			= new Segments.Control();
			userChoiceSeg.controlType	= Segments.Control.ControlType.SwipeOption;
			for(int i = 0; i < 4; i++)
			{
				userChoiceSeg.SetSwipeOptionData((FSNInGameSetting.FlowDirection)i, optionTransitionLabels[i]);
			}

			var userOptionControlSegInfo = new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= userChoiceSeg,
				selfPeriod		= false,
				usePrevPeriod	= false,
			};
			protocol.PushSegment(userOptionControlSegInfo);

			// period 세그먼트 (선택지 표시를 위해서)
			var periodSeg			= new Segments.Period();
			periodSeg.isChaining	= false;
			var periodSegInfo		= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg				= periodSeg
			};
			protocol.PushSegment(periodSegInfo);


			// 처리 블록 세그먼트
			var blockSeg			= new Segments.Control();
			blockSeg.controlType	= Segments.Control.ControlType.Block;
			var blockControlSegInfo	= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg				= blockSeg,
				selfPeriod			= false,
				usePrevPeriod		= false,
			};
			protocol.PushSegment(blockControlSegInfo);


			// 각 임시 라벨에 해당하는 시퀀스 만들기
			//
			for(int i = 0; i < 4; i++)
			{
				if (optionTransitionLabels[i] == null)	// 라벨이 지정된 경우만 진행
					continue;


				// 라벨 (soft 라벨 모드를 사용한다)
				var labelSeg		= new Segments.Label();
				labelSeg.labelName	= optionTransitionLabels[i];
				labelSeg.labelType	= Segments.Label.LabelType.Soft;
				var labelSegInfo	= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg			= labelSeg,
				};
				protocol.PushSegment(labelSegInfo);

				// LastOption 텍스트
				var lastOptionSeg		= new Segments.Text();
				lastOptionSeg.textType	= Segments.Text.TextType.LastOption;
				var lastOptionSegInfo	= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg				= lastOptionSeg
				};
				protocol.PushSegment(lastOptionSegInfo);

				// 원래 label로 점프
				var gotoSeg				= new Segments.Control();
				gotoSeg.controlType		= Segments.Control.ControlType.Goto;
				gotoSeg.SetGotoData(optionData[i][0]);
				var gotoSegInfo			= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg				= gotoSeg
				};
				protocol.PushSegment(gotoSegInfo);

				// Period (chaining을 사용한다)
				var chainPeriodSeg		= new Segments.Period();
				chainPeriodSeg.isChaining= true;
				var chainPeriodSegInfo	= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
				{
					newSeg				= chainPeriodSeg
				};
				protocol.PushSegment(chainPeriodSegInfo);

				// 블럭 추가
				protocol.PushSegment(blockControlSegInfo);
			}
		}
	}

	static void Option_left(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_addOptions(FSNInGameSetting.FlowDirection.Left, protocol);
	}

	static void Option_right(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_addOptions(FSNInGameSetting.FlowDirection.Right, protocol);
	}

	static void Option_up(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_addOptions(FSNInGameSetting.FlowDirection.Up, protocol);
	}

	static void Option_down(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_addOptions(FSNInGameSetting.FlowDirection.Down, protocol);
	}

	static void _addOptions(FSNInGameSetting.FlowDirection dir, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var data	= protocol.GetStateVar(c_key_optionData) as string[][];
		if (data == null)
		{
			Debug.LogError("You can't make options without starting an option sequence.");
		}
		else
		{
			// 0번째 인덱스는 점프할 레이블, 1번째 인덱스는 텍스트
			string text	= protocol.parameters.Length > 1? protocol.parameters[1] : "";	// 텍스트가 지정되지 않았을 때는 빈 문자열로 대체
			data[(int)dir]	= new string[2] { protocol.parameters[0], text };
		}
	}

	//------------------------------------------------------------------------------------

	static void Image_start(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newImageSeg		= new Segments.Image();
		newImageSeg.command	= Segments.Object.CommandType.Create;

		_Image_setupSegment(newImageSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newImageSeg,
				usePrevPeriod	= true,
				selfPeriod		= false
			});
	}

	static void Image_end(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newImageSeg		= new Segments.Image();
		newImageSeg.command	= Segments.Object.CommandType.Remove;

		_Image_setupSegment(newImageSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newImageSeg,
				usePrevPeriod	= true,
				selfPeriod		= false
			});
	}

	static void Image_initial(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newImageSeg		= new Segments.Image();
		newImageSeg.command	= Segments.Object.CommandType.SetInitial;

		_Image_setupSegment(newImageSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newImageSeg,
				usePrevPeriod	= true,
				selfPeriod		= false
			});
	}

	static void Image_final(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newImageSeg		= new Segments.Image();
		newImageSeg.command	= Segments.Object.CommandType.SetFinal;

		_Image_setupSegment(newImageSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newImageSeg,
				usePrevPeriod	= true,
				selfPeriod		= false
			});
	}

	static void Image_set(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newImageSeg		= new Segments.Image();
		newImageSeg.command	= Segments.Object.CommandType.SetKey;

		_Image_setupSegment(newImageSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newImageSeg,
				usePrevPeriod	= true,
				selfPeriod		= false
			});
	}

	/// <summary>
	/// BaseObject 계열의 세그먼트를 파싱해주는 함수
	/// </summary>
	/// <typeparam name="SegT"></typeparam>
	/// <param name="defaultLayer"></param>
	/// <param name="seg"></param>
	/// <param name="protocol"></param>
	/// <param name="useObjName">오브젝트 이름을 사용할 것인지. 기본값은 true, false일 시 맨 첫번째 인자부터 파라미터 파싱을 시작한다.</param>
	static void _BaseObject_setupSegment<SegT>(int defaultLayer, SegT seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol, bool useObjName = true)
		where SegT : Segments.Object
	{
		bool useDefaultLayer	= !int.TryParse(protocol.parameters[0], out seg.layerID);	// 첫번째 파라미터가 정수라면 지정한 레이어를 사용

		if (useDefaultLayer)																// 기본 레이어를 사용하는 경우, layerID는 기본값으로
			seg.layerID			= defaultLayer;

		if (useObjName)
			seg.objectName		= protocol.parameters[useDefaultLayer? 0 : 1];

		//
		int settingIndexStart	= (useDefaultLayer? 1 : 2) - (useObjName? 0 : 1);			// 세팅값이 시작되는 인덱스
		int settingCount		= (protocol.parameters.Length - settingIndexStart) / 2;		// 세팅 pair 갯수
		for (int i = 0; i < settingCount; i++)
		{
			var pName	= protocol.parameters[settingIndexStart + i * 2];
			var pParam	= protocol.parameters[settingIndexStart + i * 2 + 1];
			seg.SetPropertyFromScriptParams(pName, pParam);									// 파라미터 하나씩 세팅
		}
	}

	static void _Image_setupSegment(Segments.Image seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_BaseObject_setupSegment<Segments.Image>((int)FSNSnapshot.PreDefinedLayers.Image_Default, seg, protocol);
	}

	//------------------------------------------------------------------------------------

	static void Object_start(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.GObject();
		newObjectSeg.command	= Segments.Object.CommandType.Create;

		_Object_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Object_end(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.GObject();
		newObjectSeg.command	= Segments.Object.CommandType.Remove;

		_Object_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Object_initial(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.GObject();
		newObjectSeg.command	= Segments.Object.CommandType.SetInitial;

		_Object_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Object_final(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.GObject();
		newObjectSeg.command	= Segments.Object.CommandType.SetFinal;

		_Object_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Object_set(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.GObject();
		newObjectSeg.command	= Segments.Object.CommandType.SetKey;

		_Object_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void _Object_setupSegment(Segments.GObject seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_BaseObject_setupSegment<Segments.GObject>((int)FSNSnapshot.PreDefinedLayers.Object_Default, seg, protocol);
	}

	//------------------------------------------------------------------------------------

	static void Sound_start(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.Create;

		_Sound_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Sound_end(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.Remove;

		_Sound_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Sound_initial(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.SetInitial;

		_Sound_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Sound_final(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.SetFinal;

		_Sound_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Sound_set(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.SetKey;

		_Sound_setupSegment(newObjectSeg, protocol);		// 셋업

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void Sound_oneshot(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newObjectSeg		= new Segments.Sound();
		newObjectSeg.command	= Segments.Object.CommandType.Custom;

		//_Sound_setupSegment(newObjectSeg, protocol);		// 셋업
		_BaseObject_setupSegment<Segments.Sound>((int)FSNSnapshot.PreDefinedLayers.Sound, newObjectSeg, protocol, false);	// 원샷 사운드는 오브젝트 이름을 지정하지 않는다.

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
		{
			newSeg			= newObjectSeg,
			usePrevPeriod	= true,
			selfPeriod		= false
		});
	}

	static void _Sound_setupSegment(Segments.Sound seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		_BaseObject_setupSegment<Segments.Sound>((int)FSNSnapshot.PreDefinedLayers.Sound, seg, protocol);
	}

	//------------------------------------------------------------------------------------

	static void SplitParams_SingleList(string [] original, out string single, out string[] list)
	{
		int paramcount	= original.Length;
		list			= new string[paramcount - 1];

		single			= original[0];
		for (int i = 1; i < paramcount; i++)
			list[i - 1]	= original[i];
	}

	static void SplitParams_SingleListSingle(string [] original, out string single, out string[] list, out string single2)
	{
		int paramcount	= original.Length;
		list			= new string[paramcount - 2];

		single			= original[0];
		single2			= original[paramcount - 1];
		for (int i = 1; i < paramcount - 1; i++)
			list[i - 1]	= original[i];
	}

	static void UnityCall(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		string funcname;																	// 함수 이름
		string [] param;																	// 함수 파라미터 (두번째부터 끝까지)
		SplitParams_SingleList(protocol.parameters, out funcname, out param);

		newCallSeg.SetUnityCallData(funcname, param);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false, // ? ... 실제 사용 양상에 따라서 정해줘야할듯... 일단 함수콜이 출력에 영향을 미치지는 않으므로 false로 해둠.
				selfPeriod		= false
			});
	}

	static void UnityCall_SetFlagTrue(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_SetFlagTrue", protocol.parameters[0]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void UnityCall_SetFlagFalse(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_SetFlagFalse", protocol.parameters[0]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void UnityCall_SetFlags(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_SetFlags", protocol.parameters);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void UnityCall_SetValues(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_SetValues", protocol.parameters);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void UnityCall_ShowLoadDialog(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_ShowLoadDialog", protocol.parameters);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void UnityCall_ShowSaveDialog(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.UnityCall;

		newCallSeg.SetUnityCallData("__fsnengine_ShowSaveDialog", protocol.parameters);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	//------------------------------------------------------------------------------------

	static void ConditionJump_UnityCall(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		//string funcname			= protocol.parameters[0];									// 함수 이름
		//int paramCount			= protocol.parameters.Length;
		//string [] param			= new string[paramCount - 2];
		//for (int i = 1; i < paramCount - 1; i++)												// 함수 파라미터 (두번째부터 끝에서 두번째까지)
		//	param[i - 1]		= protocol.parameters[i];
		//string label			= protocol.parameters[paramCount - 1];						// 맨 마지막은 점프할 레이블

		string funcname;																	// 함수 이름
		string [] param;																	// 함수 파라미터 (두번째부터 끝에서 두번째까지)
		string label;																		// 맨 마지막은 점프할 레이블
		SplitParams_SingleListSingle(protocol.parameters, out funcname, out param, out label);

		newCallSeg.EnqueueConditionJumpData(funcname, param);
		newCallSeg.SetConditionJumpLabel(label);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}


	static void ConditionJump_FlagIsTrue(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_IfFlagIsTrue", protocol.parameters[0]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[1]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_FlagIsFalse(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_IfFlagIsFalse", protocol.parameters[0]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[1]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_CheckFlag(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_CheckFlagValue", protocol.parameters[0], protocol.parameters[1]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[2]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_IfValueEqual(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_CheckValueIsEqualTo", protocol.parameters[0], protocol.parameters[1]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[2]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_IfValueNotEqual(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_CheckValueIsNotEqualTo", protocol.parameters[0], protocol.parameters[1]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[2]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_IfValueGreaterThan(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_CheckValueIsGreaterThan", protocol.parameters[0], protocol.parameters[1]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[2]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}

	static void ConditionJump_IfValueLesserThan(FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		var newCallSeg			= new Segments.Control();
		newCallSeg.controlType	= Segments.Control.ControlType.ConditionJump;

		newCallSeg.EnqueueConditionJumpData("__fsnengine_CheckValueIsLesserThan", protocol.parameters[0], protocol.parameters[1]);
		newCallSeg.SetConditionJumpLabel(protocol.parameters[2]);

		protocol.PushSegment(new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg			= newCallSeg,
				usePrevPeriod	= false,
				selfPeriod		= false
			});
	}
}
