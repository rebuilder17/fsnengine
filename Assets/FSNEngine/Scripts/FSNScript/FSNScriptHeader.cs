using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 스크립트에서 사용하는 헤더
/// </summary>
public sealed partial class FSNScriptHeader
{
	/*
	 * 헤더 문법
	 * 
	 * #주석
	 * @카테고리
	 * 키값 = 속성값
	 * 키값만 (<= 이 경우 속성값은 null 로 들어가게 됨)
	 */

	/// <summary>
	/// 키값 - 속성값 pair
	/// </summary>
	public struct Pair
	{
		public string Key;
		public string Value;
	}

	/// <summary>
	/// 옵션 카테고리 구분
	/// </summary>
	class Category
	{
		Dictionary<string, Pair>	m_pairDict	= new Dictionary<string, Pair>();
		Pair[]						m_pairArray	= null;


		/// <summary>
		/// Build된 배열 리턴
		/// </summary>
		public Pair[] Entries
		{
			get
			{
				if (m_pairArray == null)	// 아직 build되지 않은 경우엔
					BuildPairList();
				return m_pairArray;
			}
		}

		/// <summary>
		/// 딕셔너리 내용을 Array로
		/// </summary>
		public void BuildPairList()
		{
			m_pairArray				= new Pair[m_pairDict.Count];
			var enumerator			= m_pairDict.GetEnumerator();
			int index				= 0;
			while(enumerator.MoveNext())
			{
				m_pairArray[index++]	= enumerator.Current.Value;
			}
		}

		/// <summary>
		/// 헤더 한 줄을 해석, entry로 집어넣기
		/// </summary>
		/// <param name="line"></param>
		public void ParseAndAddEntry(string line)
		{
			var split	= line.Split(new char[] { '=' }, 2);				// 키 = 값 으로 나눈다.
			var key		= split[0].Trim();									// 앞뒤 공백은 모두 제거
			var value	= split.Length == 2? split[1].Trim() : null;		// 값이 있을 시엔 공백을 제거하고 넣기, 없으면 null

			m_pairDict[key]	= new Pair { Key = key, Value = value };

			OnNewEntry(key, value);											// 추가 동작 실행
		}

		/// <summary>
		/// 엔트리 추가된 후 실행
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		protected virtual void OnNewEntry(string key, string value)
		{

		}
	}

	/// <summary>
	/// Category 확장형. 엔트리를 추가할 때마다 매크로에 등록한다.
	/// </summary>
	class MacroCategory : Category
	{
		MacroImpl	m_macro;

		public MacroCategory(MacroImpl macro)
		{
			m_macro	= macro;
		}

		protected override void OnNewEntry(string key, string value)
		{
			//base.OnNewEntry(key, value);
			m_macro.Register(key, value);
		}
	}

	//=====================================================================



	// Members

	Category		m_inGameSettings;				// 인게임 세팅 설정
	Category		m_flagDecl;						// 플래그 선언
	Category		m_valueDecl;					// 변수 선언
	MacroCategory	m_macroDecl;					// 매크로 선언

	MacroImpl		m_macroObject;					// 매크로 오브젝트

	Dictionary<string, Category>	m_indexToCategory;	// 카테고리 매칭용


	/// <summary>
	/// 인게임 설정 목록
	/// </summary>
	public Pair[] InGameSettings
	{
		get { return m_inGameSettings.Entries; }
	}

	/// <summary>
	/// 플래그 선언 목록
	/// </summary>
	public Pair[] FlagDeclarations
	{
		get { return m_flagDecl.Entries; }
	}

	/// <summary>
	/// 변수 선언 목록
	/// </summary>
	public Pair[] ValueDeclarations
	{
		get { return m_valueDecl.Entries; }
	}

	//public Pair[] MacroDeclarations
	//{
	//	get { return m_macroDecl.Entries; }
	//}

	/// <summary>
	/// 매크로 변환용 오브젝트
	/// </summary>
	public Macro Macros
	{
		get { return m_macroObject; }
	}


	public FSNScriptHeader()
	{
		m_inGameSettings	= new Category();
		m_flagDecl			= new Category();
		m_valueDecl			= new Category();

		m_macroObject		= new MacroImpl();
		m_macroDecl			= new MacroCategory(m_macroObject);


		// 문자열로 카테고리를 매칭할 수 있게 dictionary 세팅
		m_indexToCategory	= new Dictionary<string, Category>()
		{
			{"InGameSettings",	m_inGameSettings},
			{"설정",				m_inGameSettings},

			{"FlagDecl",		m_flagDecl},
			{"플래그선언",		m_flagDecl},

			{"ValueDecl",		m_valueDecl},
			{"값선언",			m_valueDecl},

			{"Macro",			m_macroDecl},
			{"매크로",			m_macroDecl},
		};
	}

	//=====================================================================

	/// <summary>
	/// 문자열에서 직접 헤더 파싱. 이전에 읽은 내용도 제거하지 않고 계속 누적하여 읽어들인다.
	/// </summary>
	/// <param name="scriptData"></param>
	/// <returns></returns>
	public void FromString(string scriptData)
	{
		var strstream	= new System.IO.StringReader(scriptData);
		string line		= null;
		Category curCategory	= null;
		while ((line = strstream.ReadLine()) != null)				// 줄 단위로 읽는다.
		{
			line		= line.Trim();
			if (line.Length == 0)									// 공백을 제거하고도 빈 라인은 스킵한다
				continue;

			char firstc	= line[0];
			var afterc	= line.Substring(1);
			switch(firstc)											// 라인 첫 번째 문자로 어떤 구문인지 판단
			{
				case '#':	// 주석
					break;

				case '@':	// 카테고리 지정
					if(!m_indexToCategory.TryGetValue(afterc, out curCategory))
					{
						Debug.LogErrorFormat("[FSNScriptHeader] No such category named {0}", afterc);
					}
					break;

				default:	// 일반 구문
					if(curCategory == null)
					{
						Debug.LogError("[FSNScriptHeader] No category has been indicated.");
					}
					else
					{
						curCategory.ParseAndAddEntry(line);
					}
					break;
			}
		}

		// 카테고리 빌드
		m_inGameSettings.BuildPairList();
		m_flagDecl.BuildPairList();
		m_valueDecl.BuildPairList();
		m_macroDecl.BuildPairList();
	}

	/// <summary>
	/// Asset에서 헤더 읽어오기. 이전에 읽은 내용도 제거하지 않고 계속 누적하여 읽어들인다.
	/// </summary>
	/// <param name="assetPath"></param>
	/// <returns></returns>
	public void FromAsset(string assetPath)
	{
		var textfile	= Resources.Load<TextAsset>(assetPath);
		if (textfile == null)
		{
			Debug.LogErrorFormat("[FSNSequence] Cannot open header asset : {0}", assetPath);
		}
		else
		{
			FromString(textfile.text);
		}
	}
}
