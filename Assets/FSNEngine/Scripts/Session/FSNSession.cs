using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 세션. 현재 플레이 위치, 변수 등 보관. 저장/로드 시 사용 가능.
/// </summary>
public class FSNSession
{
	// Members

	/// <summary>
	/// 현재 스크립트 이름(경로)
	/// </summary>
	public string ScriptName { get; set; }

	/// <summary>
	/// 현재(혹은 저장 당시의) 스크립트 해시 키.
	/// </summary>
	public string ScriptHashKey { get; set; }

	/// <summary>
	/// 저장/로드될 시점의 스냅샷 인덱스
	/// </summary>
	public int SnapshotIndex { get; set; }



}
