using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 스크립트 시퀀스의 세그먼트 정의 모음

namespace Segments
{
	/// <summary>
	/// Period 세그먼트
	/// </summary>
	public class Period : FSNScriptSequence.Segment
	{
		public override Type type
		{
			get { return Type.Period; }
		}

		/// <summary>
		/// 유저 입력을 기다리는지(false), 바로 뒷내용으로 넘어가는지(true)
		/// </summary>
		public bool	isChaining;
	}

	/// <summary>
	/// Label 세그먼트
	/// </summary>
	public class Label : FSNScriptSequence.Segment
	{
		public override FSNScriptSequence.Segment.Type type
		{
			get { return Type.Label; }
		}

		/// <summary>
		/// 라벨 종류.
		/// Hard : Label 진입 전에 모든 내용 Clean됨. Label 이전으로 되돌아갈 수 없음
		/// Soft : Clean하지 않음. 해석시 각 브랜치에서 하나의 label로 흐름이 합쳐지는 경우, 실제 흐름을 통합하지 않고 각각 중복해서 해석하게 됨.
		///        따라서 해석 중 무한 루프를 방지하기 위해서 이미 지나쳐온 Soft Label로는 GOTO로 점프할 수 없음 (GOTO 루프 불가)
		/// (현재는 Soft가 디폴트)
		/// </summary>
		public enum LabelType
		{
			Soft,
			Hard,
		}

		/// <summary>
		/// Label 타입
		/// </summary>
		public LabelType labelType	= LabelType.Soft;

		public string	labelName;
	}

	/// <summary>
	/// 텍스트 세그먼트
	/// </summary>
	public class Text : FSNScriptSequence.Segment
	{
		public override Type type
		{
			get { return Type.Text; }
		}

		/// <summary>
		/// 텍스트 종류
		/// </summary>
		public enum TextType
		{
			Normal,			// 일반 (디폴트)
			Options,		// 선택지

			LastOption,		// 방금 선택한 옵션 텍스트

			Clear,			// 텍스트만 모두 지우기
		}

		/// <summary>
		/// 한번에 표시되는 텍스트
		/// </summary>
		public string text;

		/// <summary>
		/// 텍스트 표시 종류
		/// </summary>
		public TextType textType;

		/// <summary>
		/// 선택지 텍스트
		/// </summary>
		public string[] optionTexts;
	}

	/// <summary>
	/// 베이스 타입 (Image 등이 이것을 사용함)
	/// </summary>
	public abstract class Object : FSNScriptSequence.Segment
	{
		public override Type type
		{
			get { return Type.Object; }
		}

		/// <summary>
		/// Object 세그먼트 종류
		/// </summary>
		public enum CommandType
		{
			Create,				// 생성
			SetInitial,			// 초기값
			SetKey,				// 키 지정 (움직임 등)
			SetFinal,			// 최종값
			Remove,				// 제거

			Custom	= 99		// 기타 다른 동작
		}
		//----------------------------------------------
		
		/// <summary>
		/// 사용하는 layer id
		/// </summary>
		public int layerID;

		/// <summary>
		/// 타겟 오브젝트 이름
		/// </summary>
		public string objectName;

		/// <summary>
		/// Object 명령 종류
		/// </summary>
		public CommandType command;

		//
		public const string		c_property_Position	= "Position";
		public const string		c_property_Scale	= "Scale";
		public const string		c_property_Rotation	= "Rotation";
		public const string		c_property_Color	= "Color";
		public const string		c_property_Alpha	= "Alpha";

		public Vector3			position;
		public Vector3			scale;
		public Vector3			rotation;
		public Color			color;
		public float			alpha;

		HashSet<string>	PropertySetList	= new HashSet<string>();

		/// <summary>
		/// 설정된 프로퍼티 리스트 리턴 (Ienumerable로 변환해서)
		/// </summary>
		public IEnumerable<string> PropertyNames
		{ get { return PropertySetList; } }

		/// <summary>
		/// 프로퍼티가 설정되었음을 나타내는 플래그 추가
		/// </summary>
		public void AddPropertySetFlag(string name)
		{
			var realname	= ConvertAliasPropertyName(FSNUtils.RemoveAllWhiteSpaces(name));	// 공백을 제거해서 별명 변환 시도
			PropertySetList.Add(realname);
		}

		/// <summary>
		/// 별명 변환하기. 별명이 추가될 경우 오버라이드하면 된다.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected virtual string ConvertAliasPropertyName(string name)
		{
			switch(name)
			{
				case "위치":
					return c_property_Position;
				case "비율":
					return c_property_Scale;
				case "회전":
					return c_property_Rotation;
				case "색상":
					return c_property_Color;
				case "알파":
					return c_property_Alpha;
			}
			return name;	// 변환 실패시 이름 그냥 리턴
		}

		/// <summary>
		/// 해당 이름으로 스크립트에서 읽어온 parameter를 세팅한다
		/// </summary>
		/// <param name="name"></param>
		/// <param name="param"></param>
		public void SetPropertyFromScriptParams(string name, string param)
		{
			var realname	= ConvertAliasPropertyName(FSNUtils.RemoveAllWhiteSpaces(name));	// 공백을 제거해서 별명 변환 시도
			
			if(SetPropertyImpl(realname, param))												// 파라미터 값을 적용했다면 setFlag 리스트에 추가
			{
				AddPropertySetFlag(name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">변환된 프로퍼티 이름</param>
		/// <param name="param">스크립트에서 읽어온 파라미터</param>
		/// <returns>세팅 성공시 true, 실패시 false</returns>
		protected virtual bool SetPropertyImpl(string name, string param)
		{
			bool success;
			switch(name)
			{
				case c_property_Position:													// * 위치
				{
					var splitparams	= FSNScriptSequence.Parser.ParseParameters(param);
					switch(splitparams.Length)
					{
						case 2:
							position.x	= float.Parse(splitparams[0]);
							position.y	= float.Parse(splitparams[1]);
							success		= true;
							break;

						case 3:
							position.x	= float.Parse(splitparams[0]);
							position.y	= float.Parse(splitparams[1]);
							position.z	= float.Parse(splitparams[2]);
							success	= true;
							break;

						default:
							Debug.LogError("Position property needs at least 2 parameters.");
							success	= false;
							break;
					}
				}
				break;

				case c_property_Scale:														// * 스케일
				{
					var splitparams	= FSNScriptSequence.Parser.ParseParameters(param);
					switch(splitparams.Length)
					{
						case 1:
							scale	= Vector3.one * float.Parse(splitparams[0]);
							success	= true;
							break;

						case 2:
							scale.x	= float.Parse(splitparams[0]);
							scale.y	= float.Parse(splitparams[1]);
							success	= true;
							break;

						case 3:
							scale.x	= float.Parse(splitparams[0]);
							scale.y	= float.Parse(splitparams[1]);
							scale.z	= float.Parse(splitparams[2]);
							success	= true;
							break;

						default:
							Debug.LogError("Scale property needs from 1 to 3 parameters.");
							success	= false;
							break;
					}
				}
				break;

				case c_property_Rotation:													// * 로테이션
				{
					var splitparams	= FSNScriptSequence.Parser.ParseParameters(param);
					switch (splitparams.Length)
					{
						case 1:
							rotation.z	= float.Parse(splitparams[0]);
							success	= true;
							break;

						case 3:
							rotation.x	= float.Parse(splitparams[0]);
							rotation.y	= float.Parse(splitparams[1]);
							rotation.z	= float.Parse(splitparams[2]);
							success	= true;
							break;

						default:
							Debug.LogError("Position property needs 1 or 3 parameters.");
							success	= false;
							break;
					}
				}
				break;

				case c_property_Color:														// * Color
				{
					var splitparams	= FSNScriptSequence.Parser.ParseParameters(param);
					switch (splitparams.Length)
					{
						case 1:
							color	= FSNUtils.ConvertHexCodeToColor(splitparams[0]);
							success	= true;
							break;

						case 3:
							color.r	= float.Parse(splitparams[0]);
							color.g	= float.Parse(splitparams[1]);
							color.b	= float.Parse(splitparams[2]);
							success	= true;
							break;

						case 4:
							color.r	= float.Parse(splitparams[0]);
							color.g	= float.Parse(splitparams[1]);
							color.b	= float.Parse(splitparams[2]);
							color.a	= float.Parse(splitparams[3]);
							success	= true;
							break;

						default:
							Debug.LogError("Position property needs 1 or 3 parameters.");
							success	= false;
							break;
					}
				}
				break;

				case c_property_Alpha:
					alpha	= float.Parse(param);
					success	= true;
					break;

				default:
					success	= false;
					break;
			}

			return success;
		}
	}

	/// <summary>
	/// 이미지
	/// </summary>
	public class Image : Object
	{
		public const string		c_property_TexturePath	= "Texture";

		public string			texturePath;
		protected override string ConvertAliasPropertyName(string name)
		{
			if(name == "파일")
			{
				return c_property_TexturePath;
			}
			return base.ConvertAliasPropertyName(name);
		}

		protected override bool SetPropertyImpl(string name, string param)
		{
			if(name == c_property_TexturePath)
			{
				texturePath	= param;
				return true;
			}
			return base.SetPropertyImpl(name, param);
		}
	}

	/// <summary>
	/// 세팅 세그먼트
	/// </summary>
	public class Setting : FSNScriptSequence.Segment
	{
		public override Type type
		{
			get { return Type.Setting; }
		}
		//

		public enum SettingMethod
		{
			Push,			// 세팅 적용
			Pop,			// 이전 세팅으로 복구
			Set,			// 강제 세팅. 세팅 스택을 오버라이드하는 건 아니라서 Pop하면 현재 상태와 함께 제거되는 것임.
		}

		/// <summary>
		/// 세팅 종류
		/// </summary>
		public SettingMethod settingMethod;


		private Dictionary<string, string>	m_settingDict	= null;

		/// <summary>
		/// 세팅 딕셔너리 구하기
		/// </summary>
		public Dictionary<string, string> RawSettingTable
		{
			get { return m_settingDict; }
		}

		/// <summary>
		/// 세팅
		/// </summary>
		public string this[string propname]
		{
			get
			{
				string value	= null;
				if(m_settingDict != null)
				{
					m_settingDict.TryGetValue(propname, out value);
				}

				return value;
			}

			set
			{
				if(m_settingDict == null)
					m_settingDict	= new Dictionary<string, string>();

				m_settingDict[propname]	= value;
			}
		}
	}

	/// <summary>
	/// 엔진 컨트롤 세그먼트
	/// </summary>
	public class Control : FSNScriptSequence.Segment
	{
		public override Type type
		{
			get { return Type.Control; }
		}
		//

		/// <summary>
		/// 컨트롤 데이터 기본형
		/// </summary>
		interface IControlData { }

		/// <summary>
		/// 선택지용 데이터
		/// </summary>
		private class SwipeOptionData : IControlData
		{
			public string [] m_dirLabelDict = new string[4];
		}

		/// <summary>
		/// GOTO 데이터
		/// </summary>
		private class GotoData : IControlData
		{
			public string m_gotoLabel;
		}

		private class LoadData : IControlData
		{
			public string m_scriptPath;
		}

		private class UnityCallData : IControlData
		{
			public string		m_messageName;
			public string []	m_parameters;
		}

		private class ConditionJumpData : IControlData
		{
			public struct condition
			{
				public string		m_messageName;
				public string []	m_parameters;
			}
			public Queue<condition>	m_conditions = new Queue<condition>();
			public string			m_jumpLabel;
		}

		//------------------------------------------------------------

		/// <summary>
		/// 컨트롤 종류
		/// </summary>
		public enum ControlType
		{
			Clear,					// 내용 모두 지우기

			Oneway,					// 반대 방향으로 진행하지 못하게 하는 플래그

			Goto,					// 라벨로 점프
			ReverseGoto,			// 역방향 진행시 점프 (역방향 오버라이드)
			SwipeOption,			// 여러 방향으로 swipe 가능 (선택지에 사용) (주 : 텍스트랑 분리되어있음)
			Block,					// 해당 분기의 스크립트 해석을 멈춘다. (주 : Period 가 포함되어있지 않으므로, 스크립트 파싱할 때 이 앞쪽에 Period를 넣어줘야함)
			Load,					// 새 스크립트 로드 (이 이전에 Clear 등이 선행되어야함)

			UnityCall,				// 유니티 콜 (SendMessage)
			ConditionJump,			// 조건부 점프
		}

		/// <summary>
		/// 컨트롤 명령 종류
		/// </summary>
		public ControlType	controlType;

		IControlData		m_controlData;	// 컨트롤 데이터


		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 타입 체크 후 지정된 형식의 OptionalData 할당
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="typeCheck"></param>
		private T CheckOptionData<T>(ControlType typeCheck)
			where T : class, IControlData, new()
		{
			if (controlType != typeCheck)							// 타입체크
				Debug.LogError(string.Format("cannot call {0} without {1} controlType", typeof(T).Name, typeCheck.ToString()));

			T data				= m_controlData as T;
			if (data == null)										// 생성 안된 경우에는 새로 생성
			{
				data			= new T();
				m_controlData	= data;
			}

			return data;
		}

		/// <summary>
		/// SwipeOption일 때, 방향에 따른 진행 라벨 추가
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="label"></param>
		public void SetSwipeOptionData(FSNInGameSetting.FlowDirection dir, string label)
		{
			var data	= CheckOptionData<SwipeOptionData>(ControlType.SwipeOption);
			data.m_dirLabelDict[(int)dir]	= label;
		}

		/// <summary>
		/// SwipeOption일 때, 방향에 따른 진행 라벨 구하기
		/// </summary>
		/// <param name="dir"></param>
		public string GetLabelFromSwipeOptionData(FSNInGameSetting.FlowDirection dir)
		{
			var data	= CheckOptionData<SwipeOptionData>(ControlType.SwipeOption);
			return data.m_dirLabelDict[(int)dir];
		}
		//

		/// <summary>
		/// Goto 데이터 세팅
		/// </summary>
		/// <param name="label"></param>
		public void SetGotoData(string label)
		{
			var data			= CheckOptionData<GotoData>(ControlType.Goto);
			data.m_gotoLabel	= label;
		}

		public string GetGotoLabel()
		{
			var data			= CheckOptionData<GotoData>(ControlType.Goto);
			return data.m_gotoLabel;
		}

		public void SetReverseGotoData(string label)
		{
			var data			= CheckOptionData<GotoData>(ControlType.ReverseGoto);
			data.m_gotoLabel	= label;
		}

		public string GetReverseGotoLabel()
		{
			var data			= CheckOptionData<GotoData>(ControlType.ReverseGoto);
			return data.m_gotoLabel;
		}

		public void SetLoadScriptData(string scriptpath)
		{
			var data			= CheckOptionData<LoadData>(ControlType.Load);
			data.m_scriptPath	= scriptpath;
		}

		public string GetLoadScriptData()
		{
			var data			= CheckOptionData<LoadData>(ControlType.Load);
			return data.m_scriptPath;
		}

		public void SetUnityCallData(string msgname, string [] param)
		{
			var data			= CheckOptionData<UnityCallData>(ControlType.UnityCall);
			data.m_messageName	= msgname;
			data.m_parameters	= param;
		}

		public void GetUnityCallData(out string msgname, out string [] param)
		{
			var data	= CheckOptionData<UnityCallData>(ControlType.UnityCall);
			msgname		= data.m_messageName;
			param		= data.m_parameters;
		}

		public void SetConditionJumpLabel(string label)
		{
			var data = CheckOptionData<ConditionJumpData>(ControlType.ConditionJump);
			data.m_jumpLabel	= label;
		}

		public string GetConditionJumpLabel()
		{
			var data = CheckOptionData<ConditionJumpData>(ControlType.ConditionJump);
			return data.m_jumpLabel;
		}

		public void EnqueueConditionJumpData(string msgname, string [] param)
		{
			var data = CheckOptionData<ConditionJumpData>(ControlType.ConditionJump);
			data.m_conditions.Enqueue(new ConditionJumpData.condition() { m_messageName = msgname, m_parameters = param });
		}

		public bool DequeueConditionJumpData(out string msgname, out string [] param)
		{
			var data	= CheckOptionData<ConditionJumpData>(ControlType.ConditionJump);
			if (data.m_conditions.Count == 0)
			{
				msgname = null;
				param = null;
				return false;
			}

			var entry	= data.m_conditions.Dequeue();
			msgname		= entry.m_messageName;
			param		= entry.m_parameters;
			return true;
		}
	}
}


