using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 중 표시할 메인 메뉴
/// </summary>
public class FSNOverlayToggleMenu : FSNBaseOverlayDialog
{
	// 버튼 콜백

	public void OnBtn_QuickSave()
	{
		CloseSelf();
		var quicksaveModule	= FSNEngine.Instance.GetModule(FSNQuickSaveSupport.ModuleNameStatic) as FSNQuickSaveSupport;
		quicksaveModule.QuickSave();
	}

	public void OnBtn_QuickLoad()
	{
		CloseSelf();

		var msgbox	= FSNOverlayUI.Instance.GetDialog<FSNOverlayMessageDialog>();
		msgbox.SetupDialogTexts("빠른 저장 당시의 상황으로 현재의 모든 진행 상황을 덮어쓰며, 별도로 저장해두지 않는 이상 이를 되돌릴 방법은 없습니다.\n\n정말로 괜찮습니까?", "예", "아니오");
		msgbox.SetupCallbacks(() =>
			{
				var quicksaveModule	= FSNEngine.Instance.GetModule(FSNQuickSaveSupport.ModuleNameStatic) as FSNQuickSaveSupport;
				quicksaveModule.QuickLoad();
			});
		FSNOverlayUI.Instance.OpenDialog<FSNOverlayMessageDialog>();
	}

	public void OnBtn_Save()
	{
		FSNOverlayUI.Instance.OpenDialog<FSNOverlaySaveDialog>();
	}

	public void OnBtn_Close()
	{
		CloseSelf();
	}

	public void OnBtn_Quit()
	{
		CloseSelf();

		var msgbox	= FSNOverlayUI.Instance.GetDialog<FSNOverlayMessageDialog>();
		msgbox.SetupDialogTexts("앱을 종료할 시 진행 사항을 따로 저장하지 않으면 모두 소실됩니다.\n\n정말로 종료할까요?", "예", "아니오");
		msgbox.SetupCallbacks(() =>
		{
			FSNUtils.QuitApp();
		});
		FSNOverlayUI.Instance.OpenDialog<FSNOverlayMessageDialog>();
	}
}
