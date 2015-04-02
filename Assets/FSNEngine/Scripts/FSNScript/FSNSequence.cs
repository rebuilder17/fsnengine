using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;


/// <summary>
/// 해석되어 메모리 상에 올라온 스크립트 시퀀스. 스크립트 파일을 parsing하면 나오는 결과물.
/// 스크립트 실행은 이 오브젝트를 참조하여 수행한다
/// </summary>
public class FSNScriptSequence
{
	/// <summary>
	/// 명령어 단위.
	/// </summary>
	public abstract class Segment
	{
		/// <summary>
		/// 명령어 종류
		/// </summary>
		public enum Type
		{
			Period,			// Period (대기상태)
			Label,			// 라벨

			Text,			// 텍스트
			Object,			// 오브젝트
			Setting,		// 세팅 변경 등

			Control,		// 엔진 컨트롤
		}

		/// <summary>
		/// segment 타입
		/// </summary>
		public abstract Type type { get; }

		///// <summary>
		///// 정확한 명령어 이름 (스크립트 상에서)
		///// </summary>
		//public string name			{ protected set; get; }

		///// <summary>
		///// 열기/닫기 세그먼트일 경우, 페어가 되는 다른 세그먼트
		///// </summary>
		//public Segment pairSegment	{ protected set; get; }

		/// <summary>
		/// 스크립트에서 해당하는 줄 번호 (디버깅 편의용)
		/// </summary>
		public int scriptLineNumber	= 0;
	}



	// Members

	List<Segment>			m_segments;				// Sequence에 포함된 모든 segments
	Dictionary<string, int>	m_labelToIndex;			// Label => list의 Index로

	public FSNScriptSequence()
	{
		m_segments		= new List<Segment>();
		m_labelToIndex	= new Dictionary<string, int>();

		Header			= new FSNScriptHeader();
	}

	//=====================================================================================

	/// <summary>
	/// Sequence 길이
	/// </summary>
	public int Length
	{
		get { return m_segments.Count; }
	}

	/// <summary>
	/// 해당 위치의 Segment 구하기
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Segment GetSegment(int index)
	{
		return m_segments[index];
	}

	public Segment this[int index]
	{
		get { return GetSegment(index); }
	}

	/// <summary>
	/// 원본 스크립트 경로
	/// </summary>
	public string OriginalScriptPath { get; private set; }

	/// <summary>
	/// 스크립트에서 생성한 해시 키
	/// </summary>
	public string ScriptHashKey { get; private set; }

	/// <summary>
	/// 현재 스크립트에서 해석한 헤더 정보
	/// </summary>
	public FSNScriptHeader Header { get; private set; }

	//=====================================================================================

	/// <summary>
	/// 라벨 지정
	/// </summary>
	/// <param name="index"></param>
	/// <param name="labelName"></param>
	public void SetLabel(int index, string labelName)
	{
		m_labelToIndex[labelName]	= index;
	}

	/// <summary>
	/// 가장 마지막 인덱스를 라벨로 지정
	/// </summary>
	/// <param name="labelName"></param>
	public void SetLastSegmentAsLabel(string labelName)
	{
		SetLabel(m_segments.Count - 1, labelName);
	}

	/// <summary>
	/// 가장 마지막 인덱스에 있는 세그먼트가 Label일 경우 등록
	/// </summary>
	public void RegisterLabelSegment()
	{
		var label	= m_segments[m_segments.Count - 1] as Segments.Label;
		if(label == null)
		{
			Debug.LogError("the segment just have been added is not a Label type.");
		}
		else
		{
			SetLastSegmentAsLabel(label.labelName);
		}
	}

	/// <summary>
	/// 라벨의 인덱스 구하기
	/// </summary>
	/// <param name="labelName"></param>
	/// <returns></returns>
	public int GetIndexOfLabel(string labelName)
	{
		if (!m_labelToIndex.ContainsKey(labelName))
			Debug.LogError("No label named " + labelName);

		return m_labelToIndex[labelName];
	}


	//--------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 스크립트 파서. 스크립트를 읽어 FSNScriptSequence를 생성해낸다.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// 새로 추가되는 segment에 대한 정보
		/// </summary>
		public class GeneratedSegmentInfo
		{
			public bool usePrevPeriod;					// 출력 계열 명령어일 경우, 추가되기 전에 이전 period는 처리해야함. 해당 상황을 나타내는 플래그.
			public bool selfPeriod;						// 명령어 스스로 period를 포함하는지
			public FSNScriptSequence.Segment newSeg;	// 이번에 새로 생성한 세그먼트
		}

		public interface ICommandGenerateProtocol
		{
			string[] parameters { get; }

			object	GetStateVar(string key);
			void	SetStateVar(string key, object var);

			void PushSegment(GeneratedSegmentInfo segmentInfo);
		}

		class CommandGenerateProtocol : ICommandGenerateProtocol
		{
			public string[] parameters { get; set; }

			// 현재 스크립트 처리중에, 명령어 등에서 해당 스크립트 처리 레벨에서만 스코프가 유지되는
			// 로컬 설정값을 보관할 수 있게 하기 위한 딕셔너리. (파라미터로 레퍼런스 전달)
			public Dictionary<string, object>		m_stateDict		= new Dictionary<string, object>();

			// 생성한 세그먼트들
			public Queue<GeneratedSegmentInfo>		m_generatedSegs	= new Queue<GeneratedSegmentInfo>();

			public object GetStateVar(string key)
			{
				object var	= null;
				m_stateDict.TryGetValue(key, out var);
				return var;
			}

			public void SetStateVar(string key, object var)
			{
				m_stateDict[key]	= var;
			}

			public void PushSegment(GeneratedSegmentInfo segmentInfo)
			{
				m_generatedSegs.Enqueue(segmentInfo);
			}

			/// <summary>
			/// 생성한 세그먼트 큐에서 가져옴. 없을 시 null
			/// </summary>
			/// <returns></returns>
			public GeneratedSegmentInfo PullSegment()
			{
				if (m_generatedSegs.Count == 0)
					return null;
				return m_generatedSegs.Dequeue();
			}
		}

		/// <summary>
		/// 커맨드 해석 등록 시 지정하는 메서드 델리게이트
		/// </summary>
		/// <returns>리턴한 segment를 추가. 만약 null일 경우 추가하지 않는다.</returns>
		public delegate void CommandSegmentGenerateFunc(ICommandGenerateProtocol protocol);



		// Constants

		// 기본 전제 : 줄 앞에 나오는 토큰들은 모두 1글자

		const string	c_token_Comment		= "#";			// 주석
		const string	c_token_SoftLabel	= ":";			// 레이블 (soft)
		const string	c_token_HardLabel	= "!";			// 레이블 (hard)
		const string	c_token_Command		= "/";			// 명령문
		const string	c_token_Period		= ".";			// period
		const string	c_token_ForceText	= "~";			// 해당 라인 강제로 텍스트로 인식하게
		const string	c_token_PreProcessor= "@";			// preprocessor 구문 (헤더 등)

		const string	c_token_LineConcat	= "//";			// 줄 붙이기 (텍스트 끝)

		/// <summary>
		/// 파라미터 각 항목을 감싸는데 사용할 수 있는 구분자 페어. 타입 구분 없이 모두 사용 가능
		/// </summary>
		static readonly Dictionary<char, char>	c_paramWrapperPair
			= new Dictionary<char,char>()
			{
				{'"', '"'},
				{'\'', '\''},
				{'[', ']'}
			};

		static readonly HashSet<char>			c_whiteSpaceSet
			= new HashSet<char>()
			{
				' ', '\t', '\r', '\n'
			};

		static readonly char[]					c_whiteSpaceArray
			= { ' ', '\t' };


		// static members

		/// <summary>
		/// 커맨드 함수
		/// </summary>
		static Dictionary<string, CommandSegmentGenerateFunc>	s_aliasToSegFunction	= new Dictionary<string, CommandSegmentGenerateFunc>();


		// static 초기화
		static Parser()
		{
			FSNBuiltInScriptCommands.Install();
		}

		/// <summary>
		/// 커맨드를 추가한다
		/// </summary>
		/// <param name="genfunc">Segment 생성 함수</param>
		/// <param name="commandName">커맨드 이름</param>
		/// <param name="commandAliases">커맨드 이름 추가 지정</param>
		public static void AddCommand(CommandSegmentGenerateFunc genfunc, string commandName, params string[] commandAliases)
		{
			s_aliasToSegFunction[commandName]	= genfunc;
			foreach(var name in commandAliases)
			{
				s_aliasToSegFunction[name]		= genfunc;
			}
		}

		/// <summary>
		/// 파라미터 문자열 파싱
		/// </summary>
		/// <param name="paramstring"></param>
		/// <returns></returns>
		public static string[] ParseParameters(string paramstring)
		{
			paramstring	= paramstring.Trim();						// 양 끝 공백 제거

			List<string> parsed	= new List<string>();
			var strBuilder		= new System.Text.StringBuilder();

			bool wrapMode		= false;							// 파라미터 인자 래핑 모드인지
			bool wrapModeFinished= false;							// 래핑 모드 탈출했는지 (이 뒤로 , 나 화이트스페이스 외에 다른 문자가 오면 안된다)
			bool findNormalChar	= false;							// 공백이나 래핑 문자 제외한 일반 문자를 만났는가 여부
			char escapeChar		= (char)0;							// 래핑 탈출 문자

			int count			= paramstring.Length;
			for (int i = 0; i < count; i++)							// 문자 하나씩 처리
			{
				char curC		= paramstring[i];

				if(wrapMode)										// * 파라미터 감싸기 모드
				{
					if(curC == escapeChar)							// 탈출 문자를 만났을 경우, 모드 변경
					{
						wrapMode			= false;
						wrapModeFinished	= true;
					}
					else
					{												// 다른 경우엔 문자 추가
						strBuilder.Append(curC);
						findNormalChar		= true;
					}
				}
				else
				{													// * 파라미터 감싸지 않을 때

					if(curC == ',')									// 쉼표 : 파라미터 구분, 기존 문자를 배열에 넣고 새 파라미터 해석 시작
					{
						var newparam	= strBuilder.ToString();
						if(!wrapModeFinished)						// wrap mode가 아니었던 경우 뒤쪽 공백 문자를 모두 제거한다.
						{
							newparam	= newparam.TrimEnd();
						}
						parsed.Add(newparam);

						strBuilder.Length	= 0;					// string builder 비우기
						wrapModeFinished	= false;				// 플래그 초기화
						findNormalChar		= false;
					}
					else if (c_paramWrapperPair.ContainsKey(curC))	// wrapper 시작
					{
						wrapMode	= true;
						escapeChar	= c_paramWrapperPair[curC];
					}
					else if (c_whiteSpaceSet.Contains(curC))		// 공백 문자,
					{
						if (!wrapModeFinished && findNormalChar)	// wrapMode였던 적이 없고, 일반 문자를 한번이라도 만난 적이 있을 때만 추가한다. (파라미터 앞쪽의 문자열들 제외)
							strBuilder.Append(curC);
					}
					else
					{												// 일반 문자 - 그냥 추가한다.
						strBuilder.Append(curC);
						findNormalChar	= true;
					}
				}
			}

			if(strBuilder.Length > 0)								// string builder가 비어있지 않다면, 해당 내용을 파라미터로 바꾼다
			{
				var newparam	= strBuilder.ToString();
				if (!wrapModeFinished)								// wrap mode가 아니었던 경우 뒤쪽 공백 문자를 모두 제거한다.
				{
					newparam	= newparam.TrimEnd();
				}
				parsed.Add(newparam);
			}

			return parsed.ToArray();
		}


		//---------------------------------------------------------------------------------------

		/// <summary>
		/// 문자열으로 스크립트 파싱
		/// </summary>
		/// <param name="scriptData"></param>
		/// <returns></returns>
		public static FSNScriptSequence FromString(string scriptData)
		{
			// 디버깅 세션 세팅
			FSNDebug.currentRuntimeStage = FSNDebug.RuntimeStage.Compile;

			var sequence				= new FSNScriptSequence();
			var strstream				= new System.IO.StringReader(scriptData);
			sequence.OriginalScriptPath	= "(string)";
			sequence.ScriptHashKey		= GenerateHashKeyFromScript(scriptData);	// 해시키 생성해두기 (세이브 파일과 스크립트 파일 버전 체크용)
			Debug.Log("ScriptHashKey : " + sequence.ScriptHashKey);

			// 스크립트 해석 상태값들
			CommandGenerateProtocol protocol	= new CommandGenerateProtocol();

			// flags
			Segments.Period	periodSeg			= null;				// Period 세그먼트. 먼저 만들어놓고 있다가 적당한 때에 삽입한다. (스크립트와 실제 세그먼트 순서가 다르기 때문)
			bool			textMultilineMode	= false;			// 텍스트 여러줄 처리중인지 (//)
			string			multilineText		= "";				// 멀티라인 모드에서, 텍스트 처리중일 때
			//

			// ** 스크립트 로드 후 첫번째 스냅샷에서 다시 이전으로 돌아가는 것은 불가능하므로, 맨 처음에 oneway 컨트롤 세그먼트를 추가해준다
			var onewayAtFirstSeg			= new Segments.Control();
			onewayAtFirstSeg.controlType	= Segments.Control.ControlType.Oneway;
			var onewaySegInfo				= new FSNScriptSequence.Parser.GeneratedSegmentInfo()
			{
				newSeg	= onewayAtFirstSeg
			};
			protocol.PushSegment(onewaySegInfo);
			//

			string line		= null;
			int linenumber	= 0;	// 줄 번호
			while ((line = strstream.ReadLine()) != null)				// 줄 단위로 읽는다.
			{
				linenumber++;
				FSNDebug.currentProcessingScriptLine	= linenumber;	// 디버깅 정보 설정

				if (!textMultilineMode && line.Length == 0)				// * 빈 줄은 스루. 단 여러줄 텍스트 모드일 경우 빈 줄에서는 여러줄 모드를 끝내게 한다.
					continue;

				if (line.EndsWith(c_token_LineConcat))					// * 여러줄 텍스트로 지정된 경우, 자동으로 여러줄 모드로. 해당 라인 붙이기
				{
					textMultilineMode	= true;

					if (multilineText.Length > 0)						// 이미 쌓여있는 텍스트가 있다면 공백 추가
						multilineText	+= "\n";
					multilineText		+= line.Substring(0, line.Length - c_token_LineConcat.Length);
				}
				else
				{
					//bool usePrevPeriod	= false;							// 출력 계열 명령어일 경우, 추가되기 전에 이전 period는 처리해야함. 해당 상황을 나타내는 플래그.
					//bool selfPeriod		= false;							// 명령어 스스로 period를 포함하는지
					//Segment newSeg		= null;								// 이번에 새로 생성한 세그먼트
					var pretoken		= line.Length > 0? line.Substring(0, 1) : "";
					switch (pretoken)										// 앞쪽 토큰으로 명령 구분
					{
						case c_token_Comment:								// * 주석
							// 스루. 뭐 왜 뭐 주석인데 뭐
							break;

						case c_token_PreProcessor:							// * 전처리 구문
							{
								var commandAndParam	= line.Substring(1).Split(c_whiteSpaceArray, 2);			// 명령어 파라미터 구분
								var command			= commandAndParam[0];
								var paramStr		= commandAndParam.Length > 1? commandAndParam[1] : "";

								// 아직까지는 header 커맨드밖에 없으므로 간단하게 if로만 체크한다. 더 늘어나면 리팩토링이 필요해질듯...
								if (command == "헤더" || command == "header")
								{
									sequence.Header.FromAsset(paramStr.Trim());
								}
								else
								{
									Debug.LogErrorFormat("[FSNSequence] line {0} : unknown preprocessor command {1}", linenumber, command);
								}
							}
							break;

						case c_token_Command:								// * 명령
							{
								var commandAndParam	= line.Substring(1).Split(c_whiteSpaceArray, 2);			// 명령어 파라미터 구분
								var command			= commandAndParam[0];
								var paramStr		= commandAndParam.Length > 1? commandAndParam[1] : "";

								CommandSegmentGenerateFunc genfunc = null;
								s_aliasToSegFunction.TryGetValue(command, out genfunc);
								if(genfunc == null)																// 등록된 명령어인지 체크
								{
									Debug.LogError("Unknown command : " + command);
								}
								else
								{
									//var param		= ParseParameters(paramStr);
									//newSeg			= genfunc(param, stateDict, out usePrevPeriod, out selfPeriod);
									protocol.parameters	= ParseParameters(paramStr);
									genfunc(protocol);
								}
							}
							break;

						case c_token_HardLabel:								// * hard label
							{
								var labelSeg		= new Segments.Label();
								labelSeg.labelName	= line.Substring(1);
								labelSeg.labelType	= Segments.Label.LabelType.Hard;

								var segInfo				= new GeneratedSegmentInfo();
								segInfo.newSeg			= labelSeg;
								segInfo.usePrevPeriod	= true;				// Label 전에 period로 다 출력해야함
								segInfo.selfPeriod		= false;
								protocol.PushSegment(segInfo);

								//newSeg				= labelSeg;
								//usePrevPeriod		= true;					// Label 전에 period로 다 출력해야함
								//selfPeriod			= false;
							}
							break;

						case c_token_SoftLabel:								// * soft label
							{
								var labelSeg		= new Segments.Label();
								labelSeg.labelName	= line.Substring(1);
								labelSeg.labelType	= Segments.Label.LabelType.Soft;

								var segInfo				= new GeneratedSegmentInfo();
								segInfo.newSeg			= labelSeg;
								segInfo.usePrevPeriod	= true;				// Label 전에 period로 다 출력해야함
								segInfo.selfPeriod		= false;
								protocol.PushSegment(segInfo);

								//newSeg				= labelSeg;
								//usePrevPeriod		= true;					// Label 전에 period로 다 출력해야함
								//selfPeriod			= false;
							}
							break;

						case c_token_Period:								// * period
							if (line.Length == 1)							// . 한글자일 때 - 일반 period
							{
								if(periodSeg != null)						// * 이미 period 명령어가 대기중일 때, 기존 명령어를 먼저 처리한다
								{
									sequence.m_segments.Add(periodSeg);
								}
								periodSeg	= new Segments.Period();
							}
							else if(line.Length == 2 && line[1].ToString() == c_token_Period)	// .. 으로 두 글자일 때 - 연결 period, 만약 period가 이전에 등장했다면 연결으로 변경
							{
								Segments.Period lastPeriodseg;

								if(periodSeg != null)											// 처리 안된 period가 있을 경우, 이것을 chaining으로 변경해준다
								{
									lastPeriodseg	= periodSeg;
								}
								else if((lastPeriodseg = sequence.m_segments[sequence.m_segments.Count - 1] as Segments.Period)
												.type == Segment.Type.Period)					// 아닐 경우, 가장 마지막으로 추가된 세그먼트가 period를 chaining으로 변경한다
								{
									//
								}
								else
								{																// 그도 아닐 경우 새로 period 생성
									periodSeg		= new Segments.Period();
									lastPeriodseg	= periodSeg;
								}

								lastPeriodseg.isChaining	= true;								// 선택한 period에 chaining속성 부여
							}
							else
							{
								Debug.LogError("invalid command... is it a period command?");
							}
							break;

						case c_token_ForceText:
						default:											// * 아무 토큰도 없음 : 텍스트
							{
								if (line.Length > 0 && line[0].ToString() == c_token_ForceText)	// 만약 강제 텍스트 토큰 (~) 이 붙어있었다면, 해당 토큰 제거
									line	= line.Substring(1);

								multilineText		+= multilineText.Length > 0? "\n" + line : line;
								var textSeg			= new Segments.Text();
								textSeg.text		= multilineText;
								textSeg.textType	= Segments.Text.TextType.Normal;

								multilineText		= "";	// 멀티라인 텍스트 보관되어있던 것을 초기화
								textMultilineMode	= false;

								var segInfo				= new GeneratedSegmentInfo();
								segInfo.newSeg			= textSeg;
								segInfo.usePrevPeriod	= true;	// 출력 명령어임
								segInfo.selfPeriod		= true;	// 스스로 period를 포함함
								protocol.PushSegment(segInfo);

								//newSeg				= textSeg;
								//usePrevPeriod		= true;	// 출력 명령어임
								//selfPeriod			= true;	// 스스로 period를 포함함
							}
							break;
					}

					GeneratedSegmentInfo newSegInfo	= null;
					while ((newSegInfo = protocol.PullSegment()) != null)	// 새로 생성된 시퀀스 모두 처리
					{
						if (newSegInfo.usePrevPeriod && periodSeg != null)	// * 선행 period를 먼저 처리해야하는 상황
						{
							sequence.m_segments.Add(periodSeg);
							periodSeg	= null;
						}

						newSegInfo.newSeg.scriptLineNumber	= linenumber;	// 줄번호 기록
						sequence.m_segments.Add(newSegInfo.newSeg);			// 시퀀스 추가
						if (newSegInfo.newSeg.type == Segment.Type.Label)	// 라벨일 경우 등록
							sequence.RegisterLabelSegment();

						if(newSegInfo.selfPeriod)							// * 방금 추가된 세그먼트가 period를 포함하는 개념이라면, period 대기시켜놓기
						{
							periodSeg	= new Segments.Period();
						}

						//usePrevPeriod	= false;
						//selfPeriod		= false;
					}
				}
			}

			if(periodSeg != null)											// 끝날 때까지 처리되지 않은 period가 있다면 여기서 추가해준다
			{
				sequence.m_segments.Add(periodSeg);
				periodSeg = null;
			}

			// 디버깅 세션 세팅
			FSNDebug.currentRuntimeStage = FSNDebug.RuntimeStage.Runtime;

			return sequence;
		}


		/// <summary>
		/// Asset에 포함된 텍스트 파일에서 스크립트를 읽는다
		/// </summary>
		/// <returns></returns>
		public static FSNScriptSequence FromAsset(string assetPath)
		{
			FSNDebug.currentProcessingScript = assetPath;	// 디버깅 정보 세팅

			var textfile	= Resources.Load<TextAsset>(assetPath);
			if (textfile == null)
			{
				Debug.LogErrorFormat("[FSNSequence] Cannot open script asset : {0}", assetPath);
			}
			var sequence	= FromString(textfile.text);
			sequence.OriginalScriptPath	= assetPath;	// 경로를 기록해둔다
			return sequence;
		}


		/// <summary>
		/// 스크립트로 MD5 해시키 생성
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>
		public static string GenerateHashKeyFromScript(string script)
		{
			var md5		= MD5.Create();
			var bytes	= Encoding.UTF8.GetBytes(script);
			var stream	= new System.IO.MemoryStream(bytes);
			return System.BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
		}
	}

	//--------------------------------------------------------------------------------------------

	#region TEST CODE

	public static FSNScriptSequence GenerateTestSequence()
	{
		var sequence		= new FSNScriptSequence();
		sequence.m_segments	= new List<Segment>();//임시, 나중에는 필요없어질것
		Segments.Text	tempTextSeg;
		Segments.Period	periodSeg	= new Segments.Period();

		tempTextSeg			= new Segments.Text();
		tempTextSeg.text	= "테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		sequence.m_segments.Add(periodSeg);

		//var settingSeg1		= new Segments.Setting();
		//settingSeg1.settingMethod				= Segments.Setting.SettingMethod.Push;
		//settingSeg1["CurrentFlowDirection"]		= FSNInGameSetting.FlowDirection.Right;
		//settingSeg1["BackwardFlowDirection"]	= FSNInGameSetting.FlowDirection.Left;
		//settingSeg1["FontSize"]					= 32f;
		//sequence.m_segments.Add(settingSeg1);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 02";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		//var settingSeg2		= new Segments.Setting();
		//settingSeg2.settingMethod				= Segments.Setting.SettingMethod.Pop;
		//sequence.m_segments.Add(settingSeg2);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 03";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		//var clearTextSeg		= new Segments.Text();
		//clearTextSeg.textType	= Segments.Text.TextType.Clear;
		//sequence.m_segments.Add(clearTextSeg);

		//tempTextSeg			= new Segments.Text();
		//tempTextSeg.text	= "테스트 04";
		//sequence.m_segments.Add(tempTextSeg);

		//sequence.m_segments.Add(periodSeg);

		var optionSeg		= new Segments.Text();
		optionSeg.textType	= Segments.Text.TextType.Options;
		optionSeg.text		= "선택지?";
		optionSeg.optionTexts	= new string[4];
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Up]	= "위";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Down]	= "아래";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Left]	= "왼쪽";
		optionSeg.optionTexts[(int)FSNInGameSetting.FlowDirection.Right]= "오른쪽";
		sequence.m_segments.Add(optionSeg);

		var userChoiceSeg			= new Segments.Control();
		userChoiceSeg.controlType	= Segments.Control.ControlType.SwipeOption;
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Up,		"label_up");
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Down,	"label_down");
		userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Left,	"label_left");
		//userChoiceSeg.SetSwipeOptionData(FSNInGameSetting.FlowDirection.Right,	"label_right");
		sequence.m_segments.Add(userChoiceSeg);

		sequence.m_segments.Add(periodSeg);

		var blockSeg			= new Segments.Control();
		blockSeg.controlType	= Segments.Control.ControlType.Block;
		sequence.m_segments.Add(blockSeg);	// 현재 흐름에서는 선택지를 끝으로 더이상 진행할 곳이 없으므로, block으로 막는다


		// 선택지 : 위
		var label_up			= new Segments.Label();
		label_up.labelName		= "label_up";
		sequence.m_segments.Add(label_up);
		sequence.RegisterLabelSegment();

		var lastOptionText		= new Segments.Text();
		lastOptionText.textType = Segments.Text.TextType.LastOption;
		sequence.m_segments.Add(lastOptionText);

		var chainPeriodSeg		= new Segments.Period();	// 뒤로 바로 넘어가지는 period
		chainPeriodSeg.isChaining	= true;
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);

		// 주 : 현재 Jump 를 처리하는 순서 때문에 period가 적용되고 완성된 Snapshot에서 적용된다.
		// 즉 GOTO를 제대로 쓰기 위해서는 period 보다 앞쪽에 배치해야함.
		var gotoSeg				= new Segments.Control();
		gotoSeg.controlType		= Segments.Control.ControlType.Goto;
		gotoSeg.SetGotoData("label_jumptest");
		sequence.m_segments.Add(gotoSeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 02";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		var label_jumptest		= new Segments.Label();
		label_jumptest.labelName= "label_jumptest";
		sequence.m_segments.Add(label_jumptest);
		sequence.RegisterLabelSegment();

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "up - 테스트 03 (label_jumptest)";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// 선택지 : 왼쪽
		var label_left			= new Segments.Label();
		label_left.labelName	= "label_left";
		sequence.m_segments.Add(label_left);
		sequence.RegisterLabelSegment();

		sequence.m_segments.Add(lastOptionText);
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 02";
		sequence.m_segments.Add(tempTextSeg);

		var reverseGotoSeg		= new Segments.Control();
		reverseGotoSeg.controlType	= Segments.Control.ControlType.ReverseGoto;
		reverseGotoSeg.SetReverseGotoData("label_reverse");
		sequence.m_segments.Add(reverseGotoSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "left - 테스트 03";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// ReverseGoto 테스트용
		var label_reverse		= new Segments.Label();
		label_reverse.labelName	= "label_reverse";
		sequence.m_segments.Add(label_reverse);
		sequence.RegisterLabelSegment();

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "you can't go back!";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "go ahead and you'll be fine.";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK

		// 선택지 : 아래쪽, 역방향 오버라이드가 제대로 되는지 테스트하기 위함.
		var label_down			= new Segments.Label();
		label_down.labelName	= "label_down";
		sequence.m_segments.Add(label_down);
		sequence.RegisterLabelSegment();

		sequence.m_segments.Add(lastOptionText);
		sequence.m_segments.Add(chainPeriodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 01";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 02, oneway";
		sequence.m_segments.Add(tempTextSeg);

		var onewaySeg			= new Segments.Control();	// ONEWAY
		onewaySeg.controlType	= Segments.Control.ControlType.Oneway;
		sequence.m_segments.Add(onewaySeg);

		sequence.m_segments.Add(periodSeg);

		tempTextSeg				= new Segments.Text();
		tempTextSeg.text		= "down - 테스트 03";
		sequence.m_segments.Add(tempTextSeg);
		sequence.m_segments.Add(periodSeg);

		sequence.m_segments.Add(blockSeg);// BLOCK


		return sequence;
	}

	#endregion
}
