using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class FSNScriptHeader
{
	/// <summary>
	/// 매크로 관리
	/// </summary>
	public interface Macro
	{
		string Replace(string original);
	}


	/// <summary>
	/// 매크로 관리 클래스.
	/// </summary>
	class MacroImpl : Macro
	{
		class MacroEntry
		{
			class Conditioned
			{
				public string	flagName;				// 플래그 조건 사용시 플래그 이름 (true일 시 치환)
				public string	replaceText;			// 치환할 매크로 텍스트
			}

			List<Conditioned>	m_conditionedMacros;	// 조건이 있는 매크로
			string				m_defaultReplaceText;	// 맞는 조건이 없을 시 치환할 텍스트 (기본)


			public MacroEntry()
			{
				m_conditionedMacros		= new List<Conditioned>();
			}

			/// <summary>
			/// 기본 치환 문자열을 설정
			/// </summary>
			/// <param name="text"></param>
			public void SetDefaultReplaceText(string text)
			{
				m_defaultReplaceText	= text;
			}

			/// <summary>
			/// 조건 있는 치환 문자열을 설정
			/// </summary>
			/// <param name="flagName"></param>
			/// <param name="text"></param>
			public void AddConditionedText(string flagName, string text)
			{
				m_conditionedMacros.Add(new Conditioned() { flagName = flagName, replaceText = text });
			}

			/// <summary>
			/// 변환 텍스트 구하기
			/// </summary>
			public string Text
			{
				get
				{
					int ccount	= m_conditionedMacros.Count;
					for (int i = 0; i < ccount; i++)			// 조건 텍스트를 먼저 찾아서 리턴한다.
					{
						var entry	= m_conditionedMacros[i];
						if (FSNEngine.Instance.GetScriptFlag(entry.flagName))
						{
							return entry.replaceText;
						}
					}

					return m_defaultReplaceText;				// 다른 조건에 맞지 않을 때만 원래 텍스트 리턴
				}
			}
		}

		// 


		// Constants

		const char					c_flagToken		= '|';			// 조건 매크로일 경우 Key값에서 매크로 이름과 플래그 이름을 구분하는 구분자
		static readonly char[]		c_flagTokenArr	= { c_flagToken };

		static readonly char[]		c_replaceTokenPair	= { '{', '}' };



		// Members

		Dictionary<string, MacroEntry>			m_macroDict;	// 매크로 딕셔너리



		public MacroImpl()
		{
			m_macroDict	= new Dictionary<string, MacroEntry>();
		}


		/// <summary>
		/// 하나 등록하기
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Register(string key, string value)
		{
			if (key.IndexOf(c_flagToken) >= 0)				// 조건 매크로인 경우, Key에서 매크로명과 플래그명 분리하기
			{
				var splited	= key.Split(c_flagTokenArr, 2);
				RegisterConditionedMacro(splited[0], splited[1], value);
			}
			else
			{												// 일반 매크로인 경우
				RegisterDefaultMacro(key, value);
			}
		}


		/// <summary>
		/// 매크로 엔트리를 리턴. 없을 경우엔 생성한다.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		MacroEntry GetMacroEntry(string name)
		{
			MacroEntry	entry	= null;
			if (!m_macroDict.TryGetValue(name, out entry))		// 항목이 기존에 없었을 시에는 생성
			{
				entry				= new MacroEntry();
				m_macroDict[name]	= entry;
			}
			return entry;
		}

		/// <summary>
		/// 조건 없는 매크로
		/// </summary>
		/// <param name="macroname"></param>
		/// <param name="value"></param>
		void RegisterDefaultMacro(string macroname, string value)
		{
			var entry	= GetMacroEntry(macroname);
			entry.SetDefaultReplaceText(value);
		}

		/// <summary>
		/// 플래그 조건이 있는 매크로
		/// </summary>
		/// <param name="macroname"></param>
		/// <param name="flagname"></param>
		/// <param name="value"></param>
		void RegisterConditionedMacro(string macroname, string flagname, string value)
		{
			var entry	= GetMacroEntry(macroname);
			entry.AddConditionedText(flagname, value);
		}

		/// <summary>
		/// 치환하기
		/// </summary>
		public string Replace(string original)
		{
			if (original.IndexOf(c_replaceTokenPair[0]) < 0)							// Quick estimation : 매크로가 없는 문자열인 경우 그대로 리턴
			{
				return original;
			}

			var output	= new System.Text.StringBuilder();
			int starti	= 0;
			int mstarti	= -1;
			while ((mstarti = original.IndexOf(c_replaceTokenPair[0], starti)) >= 0)	// 매크로가 계속 존재하면 반복
			{
				int mendi	= original.IndexOf(c_replaceTokenPair[1], mstarti);
				if (mendi < 0)
				{
					throw new System.InvalidOperationException("여는 기호만 존재하고 닫는 기호가 존재하지 않아 매크로를 치환할 수 없습니다.");
				}
				else
				{
					output.Append(original.Substring(starti, mstarti - starti));		// 매크로 치환 이전까지의 텍스트 출력

					var macroname		= original.Substring(mstarti + 1, mendi - mstarti - 1);
					MacroEntry entry	= null;
					string replaced		= null;
					if (m_macroDict.TryGetValue(macroname, out entry))					// 매크로가 존재할 경우에만 치환할 텍스트를 얻어온다
					{
						replaced		= entry.Text;
					}

					if (replaced == null)												// 치환할 텍스트가 없다면 경고 메세지 후 매크로 이름을 대신 출력한다.
					{
						Debug.LogWarningFormat("해당 매크로가 존재하지 않거나, 현재 조건으로는 매크로를 치환할 수가 없습니다. - {0}", macroname);
						replaced		= string.Format("({0})", macroname);
					}

					output.Append(replaced);											// 치환한 텍스트 출력하기
					starti				= mendi + 1;									// 매크로 치환 기호 바로 뒤부터 다시 처리 시작
				}
			}

			output.Append(original.Substring(starti));									// 남은 문자열 모두 출력하기

			return output.ToString();
		}
	}
}
