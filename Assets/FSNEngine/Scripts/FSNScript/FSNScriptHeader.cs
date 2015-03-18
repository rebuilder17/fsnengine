using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 스크립트에서 사용하는 헤더
/// </summary>
public sealed class FSNScriptHeader
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
		}
	}

	//=====================================================================



	// Members

	Category		m_inGameSettings;				// 인게임 세팅 설정
	Category		m_flagDecl;						// 플래그 선언
	Category		m_valueDecl;					// 변수 선언

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


	private FSNScriptHeader()
	{
		m_inGameSettings	= new Category();
		m_flagDecl			= new Category();
		m_valueDecl			= new Category();

		// 문자열로 카테고리를 매칭할 수 있게 dictionary 세팅
		m_indexToCategory	= new Dictionary<string, Category>()
		{
			{"InGameSettings",	m_inGameSettings},
			{"설정",			m_inGameSettings},

			{"FlagDecl",		m_flagDecl},
			{"플래그선언",		m_flagDecl},

			{"ValueDecl",		m_valueDecl},
			{"값선언",			m_valueDecl},
		};
	}

	//=====================================================================

	/// <summary>
	/// 헤더 파서
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// 문자열에서 직접 헤더 파싱
		/// </summary>
		/// <param name="scriptData"></param>
		/// <returns></returns>
		public static FSNScriptHeader FromString(string scriptData)
		{
			return new FSNScriptHeader();
		}

		/// <summary>
		/// Asset에서 헤더 읽어오기
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public static FSNScriptHeader FromAsset(string assetPath)
		{
			return new FSNScriptHeader();
		}
	}
}
