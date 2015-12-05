using UnityEngine;
using System.Collections;

/// <summary>
/// FPS 표시기
/// </summary>
public class FSNFpsShow : MonoBehaviour
{
	static float oldtime    = 0;
	static float fps        = 0;

	void Awake()
	{
		if (!Debug.isDebugBuild)			// 디버그 빌드가 아닐 때는 끄기
			enabled = false;
	}

	void Update()
	{
		var curtime     = Time.time;
		fps             = (1 / (curtime - oldtime));
		oldtime         = curtime;
	}

	void OnGUI()
	{
		string text = "\nFPS : " + fps;
		GUI.Label(new Rect(0, 0, 200, 50), text);
	}
}
