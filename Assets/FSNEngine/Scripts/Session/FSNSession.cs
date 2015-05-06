using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 플레이어 세션. 현재 플레이 위치, 변수 등 보관. 저장/로드 시 사용 가능.
/// </summary>
public class FSNSession
{
	// Members

	/// <summary>
	/// 현재 스크립트 이름(경로)
	/// </summary>
	public string ScriptName { get; set; }

	/// <summary>
	/// 현재(혹은 저장 당시의) 스크립트 해시 키.
	/// </summary>
	public string ScriptHashKey { get; set; }

	/// <summary>
	/// 저장/로드될 시점의 스냅샷 인덱스
	/// </summary>
	public int SnapshotIndex { get; set; }



	Dictionary<string, bool>	m_flagTable;	// flag 테이블
	Dictionary<string, float>	m_valueTable;	// 값 테이블

	public FSNSession()
	{
		m_flagTable		= new Dictionary<string, bool>();
		m_valueTable	= new Dictionary<string, float>();
	}

	//-----------------------------------------------------------------

	public bool GetFlagValue(string name)
	{
		bool value;
		if(!m_flagTable.TryGetValue(name, out value))
		{
			Debug.LogWarningFormat("[FSNSession] flag named {0} has not been declared before. Assuming the value is false.", name);
			value	= false;
		}
		return value;
	}

	public void SetFlagValue(string name, bool value, bool suppressWarning = false)
	{
		if(!m_flagTable.ContainsKey(name) && !suppressWarning)
		{
			Debug.LogWarningFormat("[FSNSession] flag named {0} has not been declared before. Assuming this is a new delcaration.", name);
		}
		m_flagTable[name]	= value;
	}

	public float GetNumberValue(string name)
	{
		float value;
		if(!m_valueTable.TryGetValue(name, out value))
		{
			Debug.LogWarningFormat("[FSNSession] flag named {0} has not been declared before. Assuming the value is 0 (zero).", name);
			value	= 0;
		}
		return value;
	}

	public void SetNumberValue(string name, float value, bool suppressWarning = false)
	{
		if(!m_valueTable.ContainsKey(name) && !suppressWarning)
		{
			Debug.LogWarningFormat("[FSNSession] flag named {0} has not been declared before. Assuming this is a new delcaration.", name);
		}
		m_valueTable[name]	= value;
	}

	public bool FlagIsDeclared(string name)
	{
		return m_flagTable.ContainsKey(name);
	}

	public bool ValueIsDeclared(string name)
	{
		return m_valueTable.ContainsKey(name);
	}

	//-----------------------------------------------------------------

	// JSON variable names

	const string			c_field_scriptName		= "ScriptName";
	const string			c_field_scriptHash		= "ScriptHash";
	const string			c_field_snapshotIndex	= "SnapshotIndex";

	const string			c_field_flagTable		= "FlagTable";
	const string			c_field_valueTable		= "ValueTable";
	
	//-----------------------------------------------------------------

	/// <summary>
	/// 세션 로드
	/// </summary>
	/// <param name="filepath"></param>
	/// <returns></returns>
	public static FSNSession Load(string filepath)
	{
		var newsession		= new FSNSession();

		//var fs				= File.OpenText(Application.persistentDataPath + "/" + filepath);
		//JSONObject json		= null;
		//using(fs)										// 파일을 읽고 json으로 파싱
		//{
		//	string text		= fs.ReadToEnd();
		//	json			= JSONObject.Create(text);
		//}
		var rawdata			= FSNUtils.LoadTextData(filepath);
		JSONObject json		= JSONObject.Create(rawdata);

		if (json != null)
		{
			// Script 관련
			newsession.ScriptName		= json[c_field_scriptName].str;
			newsession.ScriptHashKey	= json[c_field_scriptHash].str;
			newsession.SnapshotIndex	= (int)json[c_field_snapshotIndex].n;

			// 플래그 테이블
			var flagtable				= json[c_field_flagTable];
			foreach(var key in flagtable.keys)
			{
				newsession.SetFlagValue(key, flagtable[key].b, true);
			}

			// 값 테이블
			var valuetable				= json[c_field_valueTable];
			foreach(var key in valuetable.keys)
			{
				newsession.SetNumberValue(key, valuetable[key].n, true);
			}
		}
		else
		{
			Debug.LogErrorFormat("[FSNSession] error loading session file : {0}", filepath);
		}

		return newsession;
	}

	/// <summary>
	/// 세션 저장
	/// </summary>
	/// <param name="session"></param>
	/// <param name="filepath"></param>
	public static void Save(FSNSession session, string filepath)
	{
		//var fs				= File.Open(Application.persistentDataPath + "/" + filepath, FileMode.Create);
		//using(fs)
		{
			var json		= new JSONObject(JSONObject.Type.OBJECT);

			// Script 관련
			json.AddField(c_field_scriptName, session.ScriptName);
			json.AddField(c_field_scriptHash, session.ScriptHashKey);
			json.AddField(c_field_snapshotIndex, session.SnapshotIndex);

			// 플래그 테이블
			var flagtable	= new JSONObject(JSONObject.Type.OBJECT);
			json.AddField(c_field_flagTable, flagtable);
			foreach(var pair in session.m_flagTable)
			{
				flagtable.AddField(pair.Key, pair.Value);
			}

			// 값 테이블
			var valuetable	= new JSONObject(JSONObject.Type.OBJECT);
			json.AddField(c_field_valueTable, valuetable);
			foreach(var pair in session.m_valueTable)
			{
				valuetable.AddField(pair.Key, pair.Value);
			}


			// 실제로 파일에 기록
			//var writer		= new StreamWriter(fs);
			//using(writer)
			//{
			//	writer.Write(json.Print());
			//}
			FSNUtils.SaveTextData(filepath, json.Print());
		}
	}
}
