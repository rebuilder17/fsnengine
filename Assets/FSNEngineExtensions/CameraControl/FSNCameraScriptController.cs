using UnityEngine;
using System.Collections;

/// <summary>
/// 부착된 오브젝트의 좌표를 매 프레임마다 FSNCameraControl의 좌표값으로 설정해준다.
/// </summary>
public class FSNCameraScriptController : FSNBaseComponent
{
	Transform m_tr;
	void Start()
	{
		m_tr    = transform.parent;
	}

	void Update()
	{
		FSNCameraControl.controlPosition    = m_tr.localPosition;
	}
}
