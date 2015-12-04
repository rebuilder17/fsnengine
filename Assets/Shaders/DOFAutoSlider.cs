using UnityEngine;
using System.Collections;

public class DOFAutoSlider : MonoBehaviour
{
	const float c_timecycle = 5.0f;

	static float oldtime    = 0;
	static float fps        = 0;
	static float fpoint		= 0;

	void Start()
	{
		//oldtime = Time.time;
	}
	void Update ()
	{
		var curtime     = Time.time;
		var dof			= GetComponent<FSNDepthOfField>();
		//var dof			= GetComponent<UnityStandardAssets.ImageEffects.DepthOfFieldDeprecated>();
		//var dof         = GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
		dof.focalPoint	= Mathf.Sin(curtime / c_timecycle * 2 * Mathf.PI) * (700 / 2) + 450;
		fpoint			= dof.focalPoint;
		fps             = (1 / (curtime - oldtime));
		oldtime         = curtime;
    }

	void OnGUI()
	{
		var curtime = Time.time;
		string text = "time : " + curtime;
		text		+= "\nFPS : " + fps;
		text		+= "\nFocal Point : " + fpoint;
		oldtime = curtime;
		
        GUI.Label(new Rect(0, 0, 400, 200), text);
	}
}
