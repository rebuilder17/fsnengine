using UnityEngine;
using System.Collections;

public class DOFAutoSlider : MonoBehaviour
{
	const float c_timecycle = 5.0f;

	static float oldtime    = 0;
	static float fps        = 0;

	void Start()
	{
		//oldtime = Time.time;
	}
	void Update ()
	{
		var curtime     = Time.time;
		var dof			= GetComponent<UnityStandardAssets.ImageEffects.DepthOfFieldDeprecated>();
		//var dof         = GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
		dof.focalPoint	= Mathf.Sin(curtime / c_timecycle * 2 * Mathf.PI) * (950 / 2) + 525;
		fps             = (1 / (curtime - oldtime));
		oldtime         = curtime;
    }

	void OnGUI()
	{
		var curtime = Time.time;
		string text = "time : " + curtime;
		//if (curtime - oldtime > 0)
		{
			text    += "\nFPS : " + fps;
		}
		oldtime = curtime;
		
        GUI.Label(new Rect(0, 0, 400, 200), text);
	}
}
