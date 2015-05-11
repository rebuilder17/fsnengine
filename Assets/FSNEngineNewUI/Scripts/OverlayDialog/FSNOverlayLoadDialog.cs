using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FSNOverlayLoadDialog : FSNBaseOverlayDialog
{
	// Properties

	[SerializeField]
	FSNSaveListUI	m_savelist;					// 세이브 파일 목록
	//[SerializeField]
	//InputField		m_memoInput;				// 세이브 파일 메모



	// Members

	FSNSaveListUIItem	m_lastSelectedItem;		// 마지막으로 선택한 아이템
	bool				m_shouldLoad;			// 로딩을 해야하는지


	protected override void Initialize()
	{
		m_savelist.ItemSelected	+=	OnItemSelected;
	}

	protected override void Reset()
	{
		m_lastSelectedItem	= null;
		m_shouldLoad		= false;
		m_savelist.PopulateList();				// 리스트 채우기
	}

	protected override void OnCloseComplete()
	{
		if (m_shouldLoad)						// 파일 로딩
		{
			FSNEngine.Instance.LoadSession(m_lastSelectedItem.SaveFilePath);
		}

		m_savelist.Clear();						// 리스트 청소
	}


	// 버튼 콜백

	void OnItemSelected(FSNSaveListUIItem item)
	{
		m_lastSelectedItem	= item;

		if (!item.IsNewSaveItem)						// 새 슬롯이 아닌 경우만
		{
			//m_memoInput.text	= item.SaveTitle;	// 현재 아이템에 저장된 메모를 UI로 복사
		}
	}

	public void OnBtn_Load()
	{
		if (m_lastSelectedItem == null)				// 선택된 게 없으면 리턴
			return;

		m_shouldLoad = true;					// 파일 로딩해야함
		CloseSelf();							// 바로 닫기
	}

	public void OnBtn_Close()
	{
		CloseSelf();
	}
}