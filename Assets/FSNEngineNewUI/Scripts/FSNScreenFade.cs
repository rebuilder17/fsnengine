using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 스크린 페이드인/아웃 이펙트
/// </summary>
public class FSNScreenFade : MonoBehaviour
{
	// Properties

	[SerializeField]
	RawImage        m_colorPanel;               // 색상판


	// Members

	Color           m_loadingFadeColor;         // 로딩시 페이드 인 색상
	Color           m_controlFadeColor;         // 외부에서 컨트롤하는 화면 색상

	Coroutine       m_loadingFadeCO;
	Coroutine       m_controlFadeCO;

	/// <summary>
	/// 외부에서 페이드 색상 설정
	/// </summary>
	public Color colorControl
	{
		get { return m_controlFadeColor; }
		set { m_controlFadeColor = value; }
	}

	public static FSNScreenFade instance { get; private set; }

	void Awake()
	{
		m_loadingFadeColor  = new Color(0, 0, 0, 0);
		m_controlFadeColor  = new Color(0, 0, 0, 0);

		instance    = this;
	}

	void Update()
	{
		var lalpha      = m_loadingFadeColor.a;
		var colorMix    = (m_loadingFadeColor * lalpha) + (m_controlFadeColor * (1 - lalpha));	// 페이드 색상 합성
		var cpenable	= colorMix.a > 0.01;													// 알파값이 거의 0이라면 비활성화해야한다.

		if (cpenable != m_colorPanel.enabled)													// 실제 객체의 활성화 여부가 변경되어야할 때만 설정
		{
			m_colorPanel.enabled = cpenable;
		}

		if (cpenable)																			// 활성화 상태일 때만 패널 색상 설정
		{
			m_colorPanel.color  = colorMix;
		}
	}

	/// <summary>
	/// 로딩시 페이드인 색상
	/// </summary>
	/// <param name="duration"></param>
	/// <returns></returns>
	IEnumerator LoadingFadeCO(float duration)
	{
		m_loadingFadeColor  = new Color(0, 0, 0, 1);
		var start           = Time.time;
		var end             = start + duration;

		var curtime         = 0.0f;
		while((curtime = Time.time) < end)
		{
			var t           = (curtime - start) / duration;
			m_loadingFadeColor.a = 1 - t;
			yield return null;
		}
		m_loadingFadeColor.a = 0;
    }

	/// <summary>
	/// 로딩 코루틴 시작
	/// </summary>
	/// <param name="duration"></param>
	public void StartLoadingFade(float duration)
	{
		if (m_loadingFadeCO != null)		// 기존 코루틴이 있을 시 정지
		{
			StopCoroutine(m_loadingFadeCO);
		}

		m_loadingFadeCO = StartCoroutine(LoadingFadeCO(duration));
	}
}
