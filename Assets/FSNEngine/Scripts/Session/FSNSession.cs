using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 플레이어 세션. 현재 플레이 위치, 변수 등 보관. 저장/로드 시 사용 가능.
/// </summary>
public class FSNSession
{
	/// <summary>
	/// 세이브 파일 정보
	/// </summary>
	public struct SaveInfo
	{
		public string	saveDateTime;
		public string	title;
	}

	// Constants

	public const string			c_saveFilePrefix	= "savefile";
	public const string			c_saveFileExt		= ".sav";


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
			Debug.LogWarningFormat("[FSNSession] flag named {0} has not been declared before. Assuming this is a new declaration.", name);
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
	const string			c_field_saveDateTime	= "SaveDateTime";
	const string			c_field_saveTitle		= "SaveTitle";

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
	public static void Save(FSNSession session, string filepath, string saveTitle = "")
	{
		{
			var json		= new JSONObject(JSONObject.Type.OBJECT);

			// Script 관련
			json.AddField(c_field_scriptName, session.ScriptName);
			json.AddField(c_field_scriptHash, session.ScriptHashKey);
			json.AddField(c_field_snapshotIndex, session.SnapshotIndex);

			// 세이브 정보
			json.AddField(c_field_saveDateTime, FSNUtils.GenerateCurrentDateAndTimeString());
			json.AddField(c_field_saveTitle, saveTitle);

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

			FSNUtils.SaveTextData(filepath, json.Print());
		}
	}

	/// <summary>
	/// 세이브 파일 정보 가져오기
	/// </summary>
	/// <param name="filepath"></param>
	/// <returns></returns>
	public static SaveInfo GetSaveFileInfo(string filepath)
	{
		var info				= new SaveInfo();

		var rawdata				= FSNUtils.LoadTextData(filepath);
		JSONObject json			= JSONObject.Create(rawdata);

		if (json != null)
		{
			info.saveDateTime	= json[c_field_saveDateTime].str;
			info.title			= json[c_field_saveTitle].str;
		}

		return info;
	}

	/// <summary>
	/// 세이브 경로 안의 세이브 파일 모두 읽어온다
	/// </summary>
	/// <returns></returns>
	public static string[] GetSaveFileList()
	{
		var allFiles			= Directory.GetFiles(Application.persistentDataPath);	// persistent 경로 안의 모든 파일 목록을 불러온다
		var count				= allFiles.Length;
		List<string> savfiles	= new List<string>();

		var rootURI				= new System.Uri(Application.persistentDataPath + "/");

		for(int i = 0; i < count; i++)													// 모든 파일 경로에 대해서
		{
			var filename		= allFiles[i];
			if (filename.EndsWith(c_saveFileExt))										// .sav 파일만 가져옴
			{
				var absoluteURI	= new System.Uri(filename);
				var relativeURI	= rootURI.MakeRelativeUri(absoluteURI);					// 상대경로로 변환
				savfiles.Add(relativeURI.ToString());
			}
		}

		return savfiles.ToArray();
	}
}
