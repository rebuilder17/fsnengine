using UnityEngine;
using System.Collections;


/// <summary>
/// Engine에서 사용하는 module 기본 정의
/// </summary>
#if UNITY_4_5
[DisallowMultipleComponentAttribute]
#endif
public abstract class FSNModule : MonoBehaviour
{
	// Awake 나 Start는 아예 오버라이드하지 못하게 한다
	void Awake() { }
	void Start() { }


	/// <summary>
	/// 모듈 이름. 엔진 필수 모듈일 경우 FSNEngine.ModuleType enum 값을 ToString 하여 리턴한다
	/// </summary>
	public abstract string ModuleName { get; }

	/// <summary>
	/// 초기화
	/// </summary>
	public abstract void Initialize();
}

