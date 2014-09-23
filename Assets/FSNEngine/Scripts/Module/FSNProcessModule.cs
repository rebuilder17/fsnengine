using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
	FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, FSNSequence.Segment nextSeg, IInGameSetting nextSetting);
}

/// <summary>
/// (interface) 프로세스 모듈에서 사용할 상태 보관용 오브젝트. 하나의 Sequence를 처리할 때마다 생성해서 사용한다. 처리가 끝나면 파기.
/// </summary>
public interface IFSNProcessModuleFootprint
{
	
}

/// <summary>
/// Snapshot 처리 등에 사용하는 기능을 지원하는 Module 정의
/// </summary>
/// <typeparam name="SegT">이 모듈에서 처리할 스크립트 명령어 조각 타입</typeparam>
/// <typeparam name="ObjT">이 모듈이 컨트롤할 FSNLayerObject 타입</typeparam>
public abstract class FSNProcessModule<SegT, ElmT, ObjT> : FSNLayerModule<ElmT, ObjT>, IFSNProcessModule
	where SegT : FSNSequence.Segment
	where ElmT : class, FSNSnapshot.IElement
	where ObjT : FSNLayerObject<ElmT>
{
	/// <summary>
	/// (이거 안쓸지도 모름.)
	/// </summary>
	protected class Footprint : IFSNProcessModuleFootprint
	{
		public struct PhaseAndLayerPair
		{
			public int					phaseCount;
			public FSNSnapshot.Layer	layer;
		}

		Dictionary<int, LinkedList<PhaseAndLayerPair>> m_uIdToLayerList;	// 오브젝트의 UID => 해당 오브젝트가 포함된 모든 레이어들(+ 각 레이어가 등장하는 페이즈 번호)

		//

		public Footprint()
		{
			m_uIdToLayerList	= new Dictionary<int, LinkedList<PhaseAndLayerPair>>();
		}

		/// <summary>
		/// 해당 uId 오브젝트의 흔적 추가
		/// </summary>
		/// <param name="uId"></param>
		/// <param name="phaseCount"></param>
		/// <param name="layer"></param>
		public void Add(int uId, int phaseCount, FSNSnapshot.Layer layer)
		{

		}
	}

	//======================================================================================

	/// <summary>
	/// 해당 layer와 다음 명령어 조각을 사용해서 다음 layer 상태를 생성하여 리턴한다.
	/// 현재 Snapshot에 특정 명령어가 적용된 후의 Snapshot을 만들어내는 데 사용.
	/// </summary>
	/// <param name="curLayer"></param>
	/// <param name="nextSeg"></param>
	/// <returns></returns>
	public abstract FSNSnapshot.Layer GenerateNextLayerImage(FSNSnapshot.Layer curLayer, FSNSequence.Segment nextSeg, IInGameSetting nextSetting);
}
