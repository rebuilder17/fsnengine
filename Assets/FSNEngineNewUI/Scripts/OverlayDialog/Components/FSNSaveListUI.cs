using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

/// <summary>
/// 세이브 파일 UI 리스트
/// </summary>
public class FSNSaveListUI : MonoBehaviour
{
	// Properties

	[SerializeField]
	FSNSaveListUIItem	m_newSaveItem;			// NEW 세이브 항목 (필요하면 active해서 사용함)
	[SerializeField]
	FSNSaveListUIItem	m_itemOriginal;			// 일반 세이브 항목 (비활성화로 두고 복제해서 사용)
	[SerializeField]
	Transform			m_gridTransform;		// Item들을 붙일 트랜스폼

	[SerializeField]
	bool				m_saveMode;				// 저장 모드인지 불러오기 모드인지


	// Members

	static Regex	s_regex_numbersOnly	= new Regex(@"\d+");	// 숫자만 걸러내는 regex

	List<FSNSaveListUIItem>	m_allItems = new List<FSNSaveListUIItem>();	// 모든 Item 목록

	/// <summary>
	/// 현재 활성화된 item
	/// </summary>
	FSNSaveListUIItem CurrentSelectedItem
	{
		get
		{
			int count = m_allItems.Count;
			for(int i = 0; i < count; i++)		// 모든 Item을 검색해서 on 상태인 것을 찾아 리턴한다.
			{
				var item	= m_allItems[i];
				if (item.IsOn)
					return item;
			}
			return null;
		}
	}


	// Events

	/// <summary>
	/// 아이템 선택시 이벤트
	/// </summary>
	public event System.Action<FSNSaveListUIItem> ItemSelected;



	void Awake()
	{
	}

	public void Clear()
	{
		if (m_allItems != null)						// 복제된 리스트 아이템 모두 제거
		{
			int count	= m_allItems.Count;
			for(int i = 0; i < count; i++)
			{
				var item	= m_allItems[i];
				if(!item.IsNewSaveItem)				// 새 세이브 슬롯만 빼고..
					Destroy(item.gameObject);
			}
		}
		m_newSaveItem.gameObject.SetActive(false);	// 뉴 세이브 아이템도 비활성화

		m_allItems.Clear();
	}

	public void PopulateList()
	{
		SortedList<long, FSNSaveListUIItem> sortList	= new SortedList<long, FSNSaveListUIItem>();

		var fileList	= FSNSession.GetSaveFileList();								// 파일 목록 읽어오기
		int count		= fileList.Length;
		for(int i = 0; i < count; i++)												// 리스트 항목 생성
		{
			var newitemgo	= Instantiate<GameObject>(m_itemOriginal.gameObject);	// 게임 오브젝트 복제
			newitemgo.SetActive(true);
			newitemgo.transform.SetParent(m_gridTransform, false);

			var newitem		= newitemgo.GetComponent<FSNSaveListUIItem>();			// 컴포넌트 찾기
			newitem.SetSaveFile(fileList[i]);										// 세이브 파일 지정하기
			m_allItems.Add(newitem);

			sortList.Add(newitem.DateTime.Ticks, newitem);							// 소팅 리스트에 집어넣기

			newitem.GetComponent<Toggle>().onValueChanged.AddListener(OnItemValueChanged);	// 이벤트 추가
		}

		int sortlistcount	= sortList.Count;
		foreach(var item in sortList.Values)										// 역순으로 아이템 순서 맞추기
		{
			item.transform.SetSiblingIndex(sortlistcount--);
		}

		if(m_saveMode)																// 저장 모드일 경우, 새 세이브 슬롯도 추가
		{
			m_newSaveItem.gameObject.SetActive(true);								// 활성화하기
			m_newSaveItem.transform.SetAsFirstSibling();							// 순서 맞추기 (제일 위로)

			var toggleComp	= m_newSaveItem.GetComponent<Toggle>();					// 이벤트 추가
			var onChanged	= toggleComp.onValueChanged;
			onChanged.RemoveAllListeners();
			onChanged.AddListener(OnItemValueChanged);

			m_allItems.Add(m_newSaveItem);											// 똑같이 아이템 리스트에 추가
		}
	}

	public void OnItemValueChanged(bool value)
	{
		if(value)									// Active 되는 경우만 반응한다
		{
			if(ItemSelected != null)
			{
				ItemSelected(CurrentSelectedItem);	// 이벤트 발동
			}
		}
	}

	/// <summary>
	/// 다음 세이브파일의 이름을 생성해낸다
	/// </summary>
	/// <returns></returns>
	public string GenerateNextSavefileName()
	{
		int maxfilenum	= -1;
		int count		= m_allItems.Count;
		for(int i = 0; i < count; i++)						// 모든 파일명에 대해서...
		{
			var path	= m_allItems[i].SaveFilePath;

			if (string.IsNullOrEmpty(path))					// 경로가 없는 항목은 그냥 스킵
				continue;

			var match	= s_regex_numbersOnly.Match(path);
			if (match.Success)								// 파일명에서 숫자만 따로 추출할 수 있는 경우
			{
				var num	= int.Parse(match.Value);
				if (maxfilenum < num)						// 가장 큰 파일 번호로 업데이트
					maxfilenum	= num;
			}
		}

		return string.Format("{0}{1}{2}", FSNSession.c_saveFilePrefix, maxfilenum + 1, FSNSession.c_saveFileExt);
	}
}
