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
		FSNScriptSequence.Parser.AddCommand(Goto,			"goto",			"이동");
		FSNScriptSequence.Parser.AddCommand(ReverseGoto,	"reversegoto",	"역방향");
		FSNScriptSequence.Parser.AddCommand(End,			"end",			"끝");
		FSNScriptSequence.Parser.AddCommand(Oneway,			"oneway",		"역방향금지");
		FSNScriptSequence.Parser.AddCommand(Clear,			"clear",		"지우기");
		FSNScriptSequence.Parser.AddCommand(TextClear,		"textclear",	"글자지우기");
		FSNScriptSequence.Parser.AddCommand(Load,			"load",			"불러오기");

		FSNScriptSequence.Parser.AddCommand(PushSetting,	"pushsetting",	"설정쌓기");
		FSNScriptSequence.Parser.AddCommand(PopSetting,		"popsetting",	"설정버리기");
		FSNScriptSequence.Parser.AddCommand(SetSetting,		"setsetting",	"설정값");

		FSNScriptSequence.Parser.AddCommand(Option_start,	"option",		"선택지");
		FSNScriptSequence.Parser.AddCommand(Option_left,	"left",			"왼쪽");
		FSNScriptSequence.Parser.AddCommand(Option_right,	"right",		"오른쪽");
		FSNScriptSequence.Parser.AddCommand(Option_up,		"up",			"위");
		FSNScriptSequence.Parser.AddCommand(Option_down,	"down",			"아래");
		FSNScriptSequence.Parser.AddCommand(Option_end,		"showoption",	"선택지표시");

		FSNScriptSequence.Parser.AddCommand(Image_start,	"showimage",	"이미지생성");
		FSNScriptSequence.Parser.AddCommand(Image_end,		"removeimage",	"이미지제거");
		FSNScriptSequence.Parser.AddCommand(Image_set,		"imageset",		"이미지설정");
		FSNScriptSequence.Parser.AddCommand(Image_initial,	"imageinit",	"이미지시작설정");
		FSNScriptSequence.Parser.AddCommand(Image_final,	"imagefinal",	"이미지종료설정");
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
			protocol.SetStateVar(c_key_optionTitle, protocol.parameters[0]);	// 선택지 타이틀 지정
		}
	}

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
			data[(int)dir]	= new string[2] { protocol.parameters[0], protocol.parameters[1] };
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

	static void _Image_setupSegment(Segments.Image seg, FSNScriptSequence.Parser.ICommandGenerateProtocol protocol)
	{
		bool useDefaultLayer	= !int.TryParse(protocol.parameters[0], out seg.layerID);	// 첫번째 파라미터가 정수라면 지정한 레이어를 사용

		if (useDefaultLayer)																// 기본 레이어를 사용하는 경우, layerID는 0 (기본값)으로
			seg.layerID	= 0;

		seg.objectName			= protocol.parameters[useDefaultLayer? 0 : 1];

		//
		int settingIndexStart	= useDefaultLayer? 1 : 2;									// 세팅값이 시작되는 인덱스
		int settingCount		= (protocol.parameters.Length - settingIndexStart) / 2;		// 세팅 pair 갯수
		for(int i = 0; i < settingCount; i++)
		{
			var pName	= protocol.parameters[settingIndexStart + i * 2];
			var pParam	= protocol.parameters[settingIndexStart + i * 2 + 1];
			seg.SetPropertyFromScriptParams(pName, pParam);									// 파라미터 하나씩 세팅
		}
	}
}
