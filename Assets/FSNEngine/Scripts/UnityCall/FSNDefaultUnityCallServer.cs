using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Unity Call 을 관리하는 매개체.
/// </summary>
public class FSNDefaultUnityCallServer : MonoBehaviour, IUnityCallRegister
{
	// Members

	Dictionary<string, FSNUnityCallVoidDelegate>	m_voidFuncs	= new Dictionary<string, FSNUnityCallVoidDelegate>();
	Dictionary<string, FSNUnityCallBoolDelegate>	m_boolFuncs	= new Dictionary<string, FSNUnityCallBoolDelegate>();


	public void RegisterUnityCall(string name, FSNUnityCallVoidDelegate method)
	{
		if(m_voidFuncs.ContainsKey(name))
		{
			Debug.LogWarningFormat("[FSNDefaultUnityCallServer] function/method named {0} is already registered. Overwriting the registration.", name);
		}
		m_voidFuncs[name]	= method;
	}

	public void RegisterUnityCall(string name, FSNUnityCallBoolDelegate method)
	{
		if(m_boolFuncs.ContainsKey(name))
		{
			Debug.LogWarningFormat("[FSNDefaultUnityCallServer] function/method named {0} is already registered. Overwriting the registration.", name);
		}
		m_boolFuncs[name]	= method;
	}

	public void UnRegisterUnityCall(string name)
	{
		m_boolFuncs.Remove(name);
		m_voidFuncs.Remove(name);
	}


	public void CallVoidMethod(string name, params string [] param)
	{
		FSNUnityCallVoidDelegate func;
		if (m_voidFuncs.TryGetValue(name, out func))
		{
			func(param);
		}
		else
		{
			Debug.LogErrorFormat("[FSNDefaultUnityCallServer] Unregistered function {0}", name);
		}
	}

	public bool CallBoolMethod(string name, params string [] param)
	{
		FSNUnityCallBoolDelegate func;
		if (m_boolFuncs.TryGetValue(name, out func))
		{
			return func(param);
		}
		else
		{
			Debug.LogErrorFormat("[FSNDefaultUnityCallServer] Unregistered function {0}", name);
			return false;
		}
	}
}
