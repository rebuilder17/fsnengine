using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EventSystemTest : MonoBehaviour
{
	void Update () 
	{
        if (Input.GetMouseButtonDown(0) )
        {
            if (EventSystem.current.lastSelectedGameObject != null)
                Debug.Log("left-click over a GUI element!");
 
            else Debug.Log("just a left-click!");
        }
    }
}
