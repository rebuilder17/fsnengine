using UnityEngine;
using System.Collections;


/// <summary>
/// Engine에서 사용하는 module 기본 정의
/// </summary>
public abstract class FSNModule : MonoBehaviour
{
	/// <summary>
	/// 모듈 이름. 엔진 필수 모듈일 경우 FSNEngine.ModuleType enum 값을 ToString 하여 리턴한다
	/// </summary>
	public abstract string ModuleName { get; }

	/// <summary>
	/// 초기화
	/// </summary>
	public abstract void Initialize();

	/// <summary>
	/// 모든 엔진 초기화 종료 후 호출됨
	/// </summary>
	public virtual void OnAfterEngineInit() { }

	/// <summary>
	/// 세이브 파일 로드하기 전에 호출됨. 일반적인 스크립트 로딩시에는 호출되지 않음.
	/// </summary>
	public virtual void OnBeforeLoadSession() { }
}

