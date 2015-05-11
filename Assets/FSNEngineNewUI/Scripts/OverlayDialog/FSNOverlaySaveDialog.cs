using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 세이브 화면
/// </summary>
public class FSNOverlaySaveDialog : FSNBaseOverlayDialog
{
	// Properties

	[SerializeField]
	FSNSaveListUI	m_savelist;					// 세이브 파일 목록
	[SerializeField]
	InputField		m_memoInput;				// 세이브 파일 메모



	// Members

	FSNSaveListUIItem	m_lastSelectedItem;		// 마지막으로 선택한 아이템


	protected override void Initialize()
	{
		m_savelist.ItemSelected	+=	OnItemSelected;
	}

	protected override void Reset()
	{
		m_lastSelectedItem = null;
		m_savelist.PopulateList();				// 리스트 채우기
	}

	protected override void OnCloseComplete()
	{
		m_savelist.Clear();						// 리스트 청소
	}


	// 버튼 콜백

	void OnItemSelected(FSNSaveListUIItem item)
	{
		m_lastSelectedItem	= item;

		if(!item.IsNewSaveItem)						// 새 슬롯이 아닌 경우만
		{
			m_memoInput.text	= item.SaveTitle;	// 현재 아이템에 저장된 메모를 UI로 복사
		}
	}

	public void OnBtn_Save()
	{
		if (m_lastSelectedItem == null)				// 선택된 게 없으면 리턴
			return;
		
		if (m_lastSelectedItem.IsNewSaveItem)		// 새 슬롯이면 바로 저장
		{
			var filename = m_savelist.GenerateNextSavefileName();	// 새 파일이름 얻어오기
			FSNEngine.Instance.SaveSession(filename, m_memoInput.text);
			CloseSelf();							// 바로 닫기
		}
		else
		{											// 덮어쓰기일 경우 물어봐야한다.

			var msgbox	= FSNOverlayUI.Instance.GetDialog<FSNOverlayMessageDialog>();
			msgbox.SetupDialogTexts("기존 세이브 파일을 덮어씁니다.\n\n정말로 괜찮겠습니까?", "예", "아니오");
			msgbox.SetupCallbacks(() =>
				{
					FSNEngine.Instance.SaveSession(m_lastSelectedItem.SaveFilePath, m_memoInput.text);
					CloseSelf();
				});
			FSNOverlayUI.Instance.OpenDialog<FSNOverlayMessageDialog>();
		}
	}

	public void OnBtn_Close()
	{
		CloseSelf();
	}
}
