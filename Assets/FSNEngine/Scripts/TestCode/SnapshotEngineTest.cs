using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnapshotEngineTest : MonoBehaviour
{
	FSNSequenceEngine m_seqEngine;

	void Awake()
	{
	}

	void Start()
	{
		m_seqEngine	= GetComponent<FSNSequenceEngine>();
		FSNEngine.Instance.RunScript("testscript");
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F5))
		{
			Debug.Log("SAVE");
			FSNEngine.Instance.SaveSession("testsave.txt");
		}
		else if(Input.GetKeyDown(KeyCode.F9))
		{
			Debug.Log("Load");
			FSNEngine.Instance.LoadSession("testsave.txt");
		}
	}
}
