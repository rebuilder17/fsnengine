using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// FSNSequence를 해석하고 실행하는 메인 오브젝트
/// </summary>
public class FSNSequenceEngine : MonoBehaviour
{
	// Constants

	const float			c_maxSwipeToTransitionRatio	= 0.5f;				// 최대로 Swipe했을 때의 Transition 진행율
	//const float			c_maxSwipeToTransitionRatio	= 0.0f;				// 최대로 Swipe했을 때의 Transition 진행율


	// Members

	FSNSnapshotSequence.Traveler			m_snapshotTraveler;			// 스냅샷 트라벨러
	SortedDictionary<int, IFSNLayerModule>	m_layerModules;				// 레이어 모듈

	float									m_swipeAvailableTime;		// swipe가 가능해지는 시간. (이 시간이 지나야 가능해짐)

	/// <summary>
	/// 현재 InGame 세팅
	/// </summary>
	public FSNInGameSetting InGameSetting
	{
		get
		{
			if(m_snapshotTraveler != null)
			{
				return m_snapshotTraveler.Current.InGameSetting;
			}
			else
			{
				return FSNInGameSetting.DefaultInGameSetting;
			}
		}
	}

	/// <summary>
	/// Swipe 가능한지 여부
	/// </summary>
	public bool CanSwipe
	{
		get
		{
			return Time.time > m_swipeAvailableTime;
		}
	}

	/// <summary>
	/// 초기화
	/// </summary>
	public virtual void Initialize()
	{
		m_layerModules	= new SortedDictionary<int, IFSNLayerModule>();

		foreach(var module in FSNEngine.Instance.AllModules)		// Module 중에서 LayerModule들을 찾는다
		{
			var layerModule	= module as IFSNLayerModule;
			if(layerModule != null)
			{
				m_layerModules[layerModule.LayerID]	= layerModule;
			}
		}
	}

	/// <summary>
	/// Snapshot 시퀀스 시작
	/// </summary>
	/// <param name="sequence"></param>
	public void StartSnapshotSequence(FSNSnapshotSequence sequence)
	{
		m_snapshotTraveler	= FSNSnapshotSequence.Traveler.GenerateFrom(sequence);
	}

	/// <summary>
	/// 아직 완전히 넘기지 못한 swipe에 관한 것
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="ratio">0 ~ 1 사이, 1은 완전히 넘긴 것 (그러나 완전히 넘길 때 처리는 다른 곳에서 한다)</param>
	/// <param name="backward">반대 방향으로 swipe하는 경우인지 여부</param>
	public void PartialSwipe(FSNInGameSetting.FlowDirection direction, float ratio)
	{
		if(!CanSwipe)														// Swipe 불가능한 상태면 리턴
			return;

		var nextshot	= m_snapshotTraveler.GetLinkedSnapshot(direction);
		if(nextshot != null)												// * 넘길 수 있는 경우만 처리
		{
			var curshot	= m_snapshotTraveler.Current;

			foreach(var module in m_layerModules.Values)				// 현재 로드된 모든 LayerModule 에 맞는 레이어를 찾아 각각 처리한다
			{
				int layerID		= module.LayerID;
				var oldLayer	= curshot.GetLayer(layerID)	?? FSNSnapshot.Layer.Empty;
				var newLayer	= nextshot.GetLayer(layerID) ?? FSNSnapshot.Layer.Empty;

				if(oldLayer.IsEmpty && newLayer.IsEmpty)				// * 둘 다 비어있으면 아무것도 하지 않는다
					continue;

				bool isBackward	= InGameSetting.BackwardFlowDirection == direction;
				float trRatio	= ratio * c_maxSwipeToTransitionRatio;
				module.OldElementOnlyTransition(newLayer, trRatio, isBackward);	// 트랜지션
			}
		}
	}

	/// <summary>
	/// 완전히 넘기는 Swipe 처리
	/// </summary>
	/// <param name="direction"></param>
	public void FullSwipe(FSNInGameSetting.FlowDirection direction)
	{
		if(!CanSwipe)														// Swipe 불가능한 상태면 리턴
			return;

		var nextshot	= m_snapshotTraveler.GetLinkedSnapshot(direction);
		if(nextshot != null)												// * 실제로 가능한 경우만 처리
		{
			var curshot	= m_snapshotTraveler.Current;

			float transTime		= 0f;										// 트랜지션 시간

			foreach(var module in m_layerModules.Values)					// 현재 로드된 모든 LayerModule 에 맞는 레이어를 찾아 각각 처리한다
			{
				int layerID		= module.LayerID;
				var oldLayer	= curshot.GetLayer(layerID)	?? FSNSnapshot.Layer.Empty;
				var newLayer	= nextshot.GetLayer(layerID) ?? FSNSnapshot.Layer.Empty;

				if(oldLayer.IsEmpty && newLayer.IsEmpty)					// * 둘 다 비어있으면 아무것도 하지 않는다
					continue;

				bool isBackward	= InGameSetting.BackwardFlowDirection == direction;
				float curtt		= module.StartTransition(newLayer, c_maxSwipeToTransitionRatio, isBackward);	// 트랜지션

				if(transTime < curtt)										// 제일 긴 트랜지션 시간 추적
					transTime = curtt;
			}

			m_swipeAvailableTime	= Time.time + transTime;				// 현재 시간 + 트랜지션에 걸리는 시간 뒤에 swipe가 가능해짐

			m_snapshotTraveler.TravelTo(direction);							// 해당 방향으로 넘기기
		}
	}
}
