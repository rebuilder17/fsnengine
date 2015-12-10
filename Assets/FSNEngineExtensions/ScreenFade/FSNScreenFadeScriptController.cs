using UnityEngine;
using System.Collections;

public class FSNScreenFadeScriptController : FSNBaseGameObjectEventListener
{
	public override void OnUpdateColor(Color color)
	{
		FSNScreenFade.instance.colorControl = color;
	}
}
