using UnityEngine;
using System.Collections;


// UnityCall 방식으로 호출하는 델리게이트

public delegate void FSNUnityCallVoid(params string [] param);
public delegate bool FSNUnityCallBool(params string [] param);

/// <summary>
/// UnityCall을 등록해두는 클래스 구현
/// </summary>
public interface IUnityCallRegister
{
	void RegistUnityCall(string name, FSNUnityCallVoid method);
	void RegistUnityCall(string name, FSNUnityCallBool method);
	void UnRegistUnityCall(string name);
}


/// <summary>
/// 스크립트에서 호출하는 함수 콜을 받기 위한 베이스
/// </summary>
public class FSNBaseUnityCallReceiver : MonoBehaviour
{
	// Members

	IUnityCallRegister	m_methodRegister	= null;

	private void Awake()
	{
		// TODO

		ScanAndRegisterMethods();
	}

	private void OnDestroy()
	{
		UnregisterAllMethods();
	}

	/// <summary>
	/// 메소드를 찾아내서 전부 등록
	/// </summary>
	void ScanAndRegisterMethods()
	{
		if (m_methodRegister != null)
		{
			var methods		= this.GetType().GetMethods();
			foreach (var method in methods)
			{
				
			}
		}
	}

	void UnregisterAllMethods()
	{
		if (m_methodRegister != null)
		{

		}
	}
}
