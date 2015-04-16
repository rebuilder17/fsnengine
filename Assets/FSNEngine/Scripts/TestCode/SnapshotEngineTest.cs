using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnapshotEngineTest : MonoBehaviour
{
	void Awake()
	{
	}

	void Start()
	{
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
