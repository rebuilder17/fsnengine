using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct FSNProcessModuleCallParam
{
	public FSNScriptSequence.Segment	segment;
	public IInGameSetting		setting;
}

/// <summary>
/// (interface) Snapshot 처리 등에 사용하는 기능을 지원하는 Module 정의
/// </summary>
public interface IFSNProcessModule : IFSNLayerModule
{
	/// <summary>
	/// 해당 layer와 다음 명령어 조각을 사용해서 다음 layer 상태를 생성하여 리턴한다.
	/// 현재 Snapshot에 특정 명령어가 적용된 후의 Snapshot을 만들어내는 데 사용.
	/// </summary>
	/// <param name="phaseCount">몇번째 스냅샷 처리중인지</param>
	/// <param name="curLayer">이전 레이어</param>
	/// <param name="nextSeg">처리해야할 명령어</param>
	/// <param name="nextSetting">해당 명령어 시점에서의 설정값</param>
	FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, params FSNProcessModuleCallParam[] callParams);
}

/// <summary>
/// Snapshot 처리 등에 사용하는 기능을 지원하는 Module 정의
/// </summary>
/// <typeparam name="SegT">이 모듈에서 처리할 스크립트 명령어 조각 타입</typeparam>
/// <typeparam name="ObjT">이 모듈이 컨트롤할 FSNLayerObject 타입</typeparam>
public abstract class FSNProcessModule<SegT, ElmT, ObjT> : FSNLayerModule<ElmT, ObjT>, IFSNProcessModule
	where SegT : FSNScriptSequence.Segment
	where ElmT : class, FSNSnapshot.IElement
	where ObjT : FSNLayerObject<ElmT>
{
	/// <summary>
	/// 해당 layer와 다음 명령어 조각을 사용해서 다음 layer 상태를 생성하여 리턴한다.
	/// 현재 Snapshot에 특정 명령어가 적용된 후의 Snapshot을 만들어내는 데 사용.
	/// </summary>
	/// <param name="curLayer"></param>
	/// <param name="nextSeg"></param>
	/// <returns></returns>
	public abstract FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, params FSNProcessModuleCallParam[] callParams);
}
