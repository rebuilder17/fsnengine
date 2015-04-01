using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 디버깅 관련
/// </summary>
public static class FSNDebug
{
	/// <summary>
	/// 현재 실행 상황
	/// </summary>
	public enum RuntimeStage
	{
		Compile,				// 스크립트 컴파일 (스크립트 -> Sequence)
		SnapshotBuild,			// 스냅샷 빌드 (Sequence -> SnapshotSequence)
		Runtime,				// 실제 실행중
	}


	// Members

	/// <summary>
	/// 현재의 런타임 스테이지 설정
	/// </summary>
	public static RuntimeStage currentRuntimeStage { get; set; }


}
