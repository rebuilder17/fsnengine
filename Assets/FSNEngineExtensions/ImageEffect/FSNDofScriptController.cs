using UnityEngine;
using System.Collections;

public class FSNDofScriptController : FSNBaseComponent
{
	Transform m_tr;
	void Start()
	{
		m_tr    = transform.parent;
	}
	void Update()
	{
		FSNDepthOfField.instance.zOffset = m_tr.localPosition.z;
    }
}
