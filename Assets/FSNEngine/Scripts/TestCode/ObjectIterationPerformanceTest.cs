using UnityEngine;
using System.Collections;

public class ObjectIterationPerformanceTest : MonoBehaviour
{
	const int       c_gameObjectCount   = 1000;
	const int       c_testCount         = 10000;

	void Start()
	{
		Debug.Log("Initializing...");
		InitTest();
		Debug.Log("Init finished");

		TestFunc("test1 : foreach", Test1);
		TestFunc("test2 : for and GetChild", Test2);
		TestFunc("test3 : getcomponents", Test3);
		TestFunc("test4 : using enumerator", Test4);
	}

	void InitTest()
	{
		var root    = transform;
		for (int i = 0; i < c_gameObjectCount; i++)
		{
			var go  = new GameObject();
			go.transform.SetParent(root);
		}
	}

	void TestFunc(string testname, System.Action func)
	{
		Debug.Log(testname + " start...");

		var starttime   = Time.realtimeSinceStartup;
		for(int i = 0; i < c_testCount; i++)
		{
			func();
		}

		Debug.Log(testname + " finished ... time elapsed : " + (Time.realtimeSinceStartup - starttime));
	}

	void Test1()
	{
		var root    = transform;
		foreach (var tr in root)
		{
			((Transform)tr).GetComponent<Transform>();
		}
	}

	void Test2()
	{
		var root    = transform;
		var count   = root.childCount;
		for (int i = 0; i < count; i++)
		{
			var tr  = root.GetChild(i);
			tr.GetComponent<Transform>();
		}
	}

	void Test3()
	{
		var root    = transform;
		var list    = root.GetComponentsInChildren<Transform>();
		var count   = list.Length;
		for (int i = 0; i < count; i++)
		{
			var tr  = list[i];
			if (tr.parent == root)
				tr.GetComponent<Transform>();
		}
	}

	void Test4()
	{
		var root    = transform;
		var iter    = transform.GetEnumerator();
		while(iter.MoveNext())
		{
			var tr  = iter.Current as Transform;
			tr.GetComponent<Transform>();
		}
	}
}
