using UnityEngine;
using System.Collections;
using System.IO;

public static class FSNPersistentData
{
	// const

	const string		c_persistent_filename	= "persdata.dat";

	const string		c_field_scriptVars		= "ScriptVars";

	const string		c_field_flagTable		= "FlagTable";
	const string		c_field_valueTable		= "ValueTable";


	// members

	static JSONObject	m_persData;						// persistent 데이터


	/// <summary>
	/// 변경점이 있는지 여부
	/// </summary>
	public static bool IsDirty { get; private set; }



	/// <summary>
	/// Persistent 데이터 로딩
	/// </summary>
	public static void Load()
	{
		if (FSNUtils.CheckTextFileExists(c_persistent_filename))			// 파일이 존재하는 경우엔 단순 로드
		{
			m_persData	= JSONObject.Create(FSNUtils.LoadTextData(c_persistent_filename));
		}
		else
		{																	// 파일이 없을 시엔 새롭게 JSONObject 생성
			m_persData		= new JSONObject(JSONObject.Type.OBJECT);

			var scriptVars	= new JSONObject(JSONObject.Type.OBJECT);
			m_persData.AddField(c_field_scriptVars, scriptVars);

			scriptVars.AddField(c_field_flagTable, new JSONObject(JSONObject.Type.OBJECT));
			scriptVars.AddField(c_field_valueTable, new JSONObject(JSONObject.Type.OBJECT));

			IsDirty	= true;	// dirty 플래그 세우기
		}
	}

	/// <summary>
	/// Persistent 데이터 세이브
	/// </summary>
	public static void Save(bool force = false)
	{
		if (IsDirty || force)		// 변경점이 있을 때만 저장, force가 올라가있다면 무조건
		{
			FSNUtils.SaveTextData(c_persistent_filename, m_persData.Print());
			IsDirty = false;		// dirty 플래그 내리기
		}
	}

	//---------------------------------------------------------------------------------------

	public static bool GetScriptFlag(string name)
	{
		name		= StripPersVarPrefix(name);
		var table	= m_persData[c_field_scriptVars][c_field_flagTable];

		return table.HasField(name)? table[name].b : false;			// 필드가 없을 시엔 기본값 false
	}

	public static float GetScriptValue(string name)
	{
		name		= StripPersVarPrefix(name);
		var table	= m_persData[c_field_scriptVars][c_field_valueTable];

		return table.HasField(name)? table[name].n : 0;				// 필드가 없을 시엔 기본값 0
	}

	public static void SetScriptFlag(string name, bool value)
	{
		name		= StripPersVarPrefix(name);
		m_persData[c_field_scriptVars][c_field_flagTable].SetField(name, value);

		IsDirty	= true;	// dirty 플래그 세우기
	}

	public static void SetScriptValue(string name, float value)
	{
		name		= StripPersVarPrefix(name);
		m_persData[c_field_scriptVars][c_field_valueTable].SetField(name, value);

		IsDirty	= true;	// dirty 플래그 세우기
	}

	//-----------------------------------------------------------------------------------------

	/// <summary>
	/// Persistent 변수 이름 규칙에 부합하는 변수명인지 체크. $로 시작하면 persistent 변수
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static bool IsPersistentVarName(string name)
	{
		return name.Length > 0 && name[0] == '$';
	}

	/// <summary>
	/// 변수이름 앞쪽의 $ 를 떼어낸다.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	static string StripPersVarPrefix(string name)
	{
		return IsPersistentVarName(name)? name.Substring(1) : name;
	}
}
