using UnityEngine;
using System.Collections;

public class TestComponentMildFloating : FSNBaseComponent
{
	float	m_floatingRange		= 50;
	float	m_floatingRefresh	= 5;
	float	m_floatingEase		= 0.5f;

	Vector3 m_lastTargetPos		= Vector3.zero;
	Vector3	m_currentTargetPos	= Vector3.zero;

	void Start()
	{
		StartCoroutine(Floating());
	}

	IEnumerator Floating()
	{
		while(true)
		{
			float startTime		= Time.time;
			m_lastTargetPos		= m_currentTargetPos;
			m_currentTargetPos	= Random.insideUnitCircle * m_floatingRange;

			float elapsed		= 0f;
			do
			{
				elapsed			= Time.time - startTime;
				float t			= Mathf.Pow(elapsed / m_floatingRefresh, m_floatingEase);
				transform.localPosition	= Vector3.Lerp(m_lastTargetPos, m_currentTargetPos, t);
				yield return null;

			} while (elapsed <= m_floatingRefresh);
		}
	}

	public override void OnParameterChange(string param)
	{
		base.OnParameterChange(param);
	}
}
