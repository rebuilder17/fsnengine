using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 메세지 박스
/// </summary>
public class FSNOverlayMessageDialog : FSNBaseOverlayDialog
{
	// Properties

	[SerializeField]
	Text			m_message;				// 메세지 박스 텍스트 컴포넌트
	[SerializeField]
	GameObject []	m_buttons;				// 버튼 목록
	[SerializeField]
	Text []			m_buttonTexts;			// 버튼 텍스트들


	// Members

	System.Action[]	m_callbacks;			// 콜백 리스트
	int				m_clickedBtnIndex;		// 최근에 눌린 버튼 인덱스


	protected override void Reset()
	{
		m_clickedBtnIndex	= -1;
	}

	/// <summary>
	/// 텍스트, 버튼 설정
	/// </summary>
	/// <param name="message"></param>
	/// <param name="buttonTexts"></param>
	public void SetupDialogTexts(string message, params string[] buttonTexts)
	{
		m_message.text	= message;							// 메세지 설정

		if(buttonTexts.Length == 0)							// 만약 버튼이 하나도 지정되지 않았다면 기본 버튼 설정
		{
			buttonTexts	= new string[] { "확인" };
		}

		int btncount	= buttonTexts.Length;
		int i = 0;
		for(; i < btncount; i++)							// 버튼 텍스트/활성 여부 설정
		{
			var button	= m_buttons[i];
			button.SetActive(true);
			m_buttonTexts[i].text = buttonTexts[i];
		}
		for (; i < m_buttons.Length; i++)					// 나머지 버튼은 비활성화
		{
			m_buttons[i].SetActive(false);
		}
		m_callbacks		= new System.Action[btncount];		// 콜백 배열 초기화
	}

	/// <summary>
	/// 각 버튼의 콜백 세팅
	/// </summary>
	/// <param name="callbacks"></param>
	public void SetupCallbacks(params System.Action[] callbacks)
	{
		callbacks.CopyTo(m_callbacks, 0);
	}

	/// <summary>
	/// 버튼 클릭
	/// </summary>
	/// <param name="btnindex"></param>
	void OnButtonClick(int btnindex)
	{
		if (btnindex >= 0)
		{
			m_clickedBtnIndex	= btnindex;					// 누른 버튼 인덱스 보관
			CloseSelf();									// 스스로 닫기
		}
	}

	protected override void OnCloseComplete()
	{
		var cb	= m_callbacks[m_clickedBtnIndex];			// 가장 최근에 눌린 것에 대응하는 콜백 호출
		if (cb != null) cb();
	}


	// Button에서 들어오는 콜백들

	public void OnButton1()
	{
		OnButtonClick(0);
	}

	public void OnButton2()
	{
		OnButtonClick(1);
	}

	public void OnButton3()
	{
		OnButtonClick(2);
	}
}
