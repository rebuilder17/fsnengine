using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// UnityCall 방식으로 호출하는 델리게이트

public delegate void FSNUnityCallVoidDelegate(params string [] param);
public delegate bool FSNUnityCallBoolDelegate(params string [] param);

/// <summary>
/// UnityCall을 등록해두는 클래스 구현
/// </summary>
public interface IUnityCallRegister
{
	void RegisterUnityCall(string name, FSNUnityCallVoidDelegate method);
	void RegisterUnityCall(string name, FSNUnityCallBoolDelegate method);
	void UnRegisterUnityCall(string name);
}


/// <summary>
/// 스크립트에서 호출하는 함수 콜을 받기 위한 베이스
/// </summary>
public class FSNBaseUnityCallReceiver : MonoBehaviour
{
	// UnityCall 로 활용할 메서드에 붙이는 Attribute

	[AttributeUsage(AttributeTargets.Method)]
	protected class UnityCallVoidMethod : System.Attribute { }

	[AttributeUsage(AttributeTargets.Method)]
	protected class UnityCallBoolMethod : System.Attribute { }


	// Members

	IUnityCallRegister			m_methodRegister	= null;
	Dictionary<string, object>	m_delegateDict		= new Dictionary<string, object>();


	// Awake / OnDestroy 구현용 virtual 메서드

	public virtual void Awake_Derrived()		{ }
	public virtual void OnDestroy_Derrived()	{ }


	private void Awake()
	{
		// FSNEngine 이 부착된 게임 오브젝트 혹은 그 하부에서 IUnityCallRegister 를 찾는다.
		m_methodRegister	= FSNEngine.Instance.gameObject.GetComponentInChildren<IUnityCallRegister>();
		ScanAndRegisterMethods();

		Awake_Derrived();
	}

	private void OnDestroy()
	{
		UnregisterAllMethods();

		OnDestroy_Derrived();
	}

	/// <summary>
	/// 메소드를 찾아내서 전부 등록
	/// </summary>
	private void ScanAndRegisterMethods()
	{
		if (m_methodRegister != null)
		{
			Debug.Log(this.GetType().ToString());
			//var methods			= this.GetType().GetMethods(System.Reflection.BindingFlags.Default
			//												| System.Reflection.BindingFlags.FlattenHierarchy
			//												| System.Reflection.BindingFlags.Static
			//												| System.Reflection.BindingFlags.Public
			//												| System.Reflection.BindingFlags.NonPublic);
			var methods			= this.GetType().GetMethods();

			foreach (var method in methods)
			{
				bool isStatic	= method.IsStatic;
				Type delType	= null;
				foreach (var attr in method.GetCustomAttributes(true))		// UnityCall 로 지정하는 Attribute가 지정되었는지 체크
				{
					if (attr is UnityCallVoidMethod)
					{
						delType	= typeof(FSNUnityCallVoidDelegate);
						break;
					}
					else if (attr is UnityCallBoolMethod)
					{
						delType	= typeof(FSNUnityCallBoolDelegate);
						break;
					}
				}

				if (delType == typeof(FSNUnityCallVoidDelegate))			// 델리게이트로 등록 (void)
				{
					FSNUnityCallVoidDelegate del;
					if (isStatic)
					{
						del	= (FSNUnityCallVoidDelegate)Delegate.CreateDelegate(delType, method);
					}
					else
					{
						del	= (FSNUnityCallVoidDelegate)Delegate.CreateDelegate(delType, this, method);
					}
					m_methodRegister.RegisterUnityCall(method.Name, del);
					m_delegateDict[method.Name]	= del;						// 나중에 unregister할 수 있게 보관

					//Debug.LogWarningFormat("Found one! : " + method.Name);
				}
				else if (delType == typeof(FSNUnityCallBoolDelegate))		// 델리게이트로 등록 (bool)
				{
					FSNUnityCallBoolDelegate del;
					if (isStatic)
					{
						del	= (FSNUnityCallBoolDelegate)Delegate.CreateDelegate(delType, method);
					}
					else
					{
						del	= (FSNUnityCallBoolDelegate)Delegate.CreateDelegate(delType, this, method);
					}
					m_methodRegister.RegisterUnityCall(method.Name, del);
					m_delegateDict[method.Name]	= del;						// 나중에 unregister할 수 있게 보관
				}
				else
				{
					//Debug.LogFormat("[FSNBaseUnityCallReceiver] non-unity call method : {0}", method.Name);
				}
			}
		}
	}

	/// <summary>
	/// 등록했던 메소드 전부 해제
	/// </summary>
	private void UnregisterAllMethods()
	{
		if (m_methodRegister != null)
		{
			foreach (var name in m_delegateDict.Keys)
			{
				m_methodRegister.UnRegisterUnityCall(name);
			}

			m_delegateDict.Clear();
		}
	}
}
