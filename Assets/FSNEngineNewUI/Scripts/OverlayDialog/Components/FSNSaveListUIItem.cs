using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;

public class FSNSaveListUIItem : MonoBehaviour
{
	// Properties

	[SerializeField]
	bool			m_isNewSaveItem;			// New Save 슬롯을 나타내는 아이템인지
	[SerializeField]
	Text			m_text;						// 텍스트 컴포넌트


	// Members

	Toggle			m_toggle;					// Toggle 컴포넌트
	string			m_savefilePath;				// 세이브 파일 경로
	FSNSession.SaveInfo	m_saveinfo;				// 세이브 정보



	/// <summary>
	/// 세이브 파일 경로
	/// </summary>
	public string SaveFilePath
	{
		get { return m_savefilePath; }
	}

	public string DateTimeString
	{
		get { return m_saveinfo.saveDateTime; }
	}

	public System.DateTime DateTime
	{
		get { return System.DateTime.Parse(m_saveinfo.saveDateTime); }
	}

	public string SaveTitle
	{
		get { return m_saveinfo.title; }
	}

	public bool IsNewSaveItem
	{
		get { return m_isNewSaveItem; }
	}

	public bool IsOn
	{
		get { return m_toggle.isOn; }
		set { m_toggle.isOn = value; }
	}


	void Awake()
	{
		m_toggle		= GetComponent<Toggle>();
	}

	/// <summary>
	/// 매칭되는 세이브 파일을 세팅하고 파일 정보를 읽어서 표시
	/// </summary>
	/// <param name="path"></param>
	public void SetSaveFile(string path)
	{
		m_savefilePath	= path;
		m_saveinfo		= FSNSession.GetSaveFileInfo(path);
		
		var title		= !string.IsNullOrEmpty(m_saveinfo.title)? m_saveinfo.title : "(제목 없음)";
		var text		= string.Format("{0}\n{1}", title, m_saveinfo.saveDateTime);
		m_text.text		= text;
	}
}
